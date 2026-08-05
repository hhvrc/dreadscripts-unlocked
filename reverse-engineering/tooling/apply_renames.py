#!/usr/bin/env python3
"""
The only writer for renames/*.json, safe to call from several agents at once.

Why this exists
---------------
Naming members is the one job in this repo that parallelises well -- each type is independent -- but
every worker has to write back into the same three JSON files. Hand-editing them concurrently loses
writes: two workers read the same file, each edits its own type, and whichever saves second silently
reverts the other. Nothing about that failure is visible afterwards, because both files are valid
JSON and both contain a plausible set of names.

So all writes go through here, and every write takes an exclusive lock across the whole
read-modify-write. A worker that does not win the lock **waits** for it rather than failing, so
callers never need retry logic and no write is ever dropped. The map is re-read inside the lock --
reading outside it would defeat the point -- and replaced atomically, so a crash mid-write cannot
leave a half-written map.

What it refuses
---------------
It will not invent a name, accept an identifier C# would reject, or give two *fields* of one type
the same name. It also refuses to overwrite a name that is already set unless `--force` is passed:
concurrent workers must not silently clobber each other, but a name discovered to be *wrong* has to
be correctable, and until it was the only recourse was leaving two trees knowingly disagreeing.
Every forced change is reported as `FIX old -> new` so it shows up in a transcript rather than
passing as an ordinary write. Methods are deliberately allowed to share a
name: the original sources used overloads, and refusing them forces an invented suffix that claims a
distinction the program never had. ilrename stays the authority on collisions -- its method check
keys on name plus parameter types plus return type -- so a genuine clash still fails at re-export.
Refusals are per-member and reported; the rest of the batch still applies.

`set --set-type` names the type itself. Type names live in the same map, so editing one by hand is
the same lost write as editing a member by hand; it takes the same lock and the same rules, and
additionally refuses a name already held by a sibling type in the same enclosing type or namespace.

    python3 scripts/apply_renames.py show  --sample ADOverhaul2022 --type GUIColorScope
    python3 scripts/apply_renames.py set   --sample ADOverhaul2022 --type GUIColorScope \
        --set m_BaseMethod=captured --set AddIterator=Capture
    python3 scripts/apply_renames.py set   --sample ControllerEditor --type WatcherProcessor \
        --set-type Contents
"""

import argparse
import os
import re
import sys
import tempfile
import time
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from refresh_renames import KEY_LINE, RENAMES, strip_comments  # noqa: E402
from rename_status import MEMBER_KEY, entries, export_file, owner_of, type_names  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
LOCK = ROOT / "work" / "renames.lock"

IDENTIFIER = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")
# C# keywords a rename must not produce; ilrename would emit them unescaped.
RESERVED = {
    "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class",
    "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event",
    "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
    "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new",
    "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
    "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static",
    "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
    "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
}


# Whole-file advisory locking, spelled differently on each platform: fcntl.flock on POSIX,
# msvcrt.locking on Windows, which has no flock. Windows is not a footnote here -- the ported Unity
# project in ../public/unity/ is worked on from one, so a naming session runs there too, and an
# unconditional `import fcntl` made this module (the *only* sanctioned writer) fail at import.
# The temptation then is to hand-edit the JSON, which is exactly the lost-write failure the lock
# exists to prevent.
if sys.platform == "win32":
    import msvcrt

    def _lock(handle, blocking: bool):
        # msvcrt locks a byte range rather than the file, so every holder must agree on the same
        # range: byte 0 of a file kept at that size on purpose. LK_NBLCK fails immediately;
        # LK_LOCK retries for ~10s and then raises, so blocking mode loops.
        if blocking:
            while True:
                try:
                    msvcrt.locking(handle.fileno(), msvcrt.LK_LOCK, 1)
                    return
                except OSError:
                    continue
        msvcrt.locking(handle.fileno(), msvcrt.LK_NBLCK, 1)

    def _unlock(handle):
        handle.seek(0)
        msvcrt.locking(handle.fileno(), msvcrt.LK_UNLCK, 1)
else:
    import fcntl

    def _lock(handle, blocking: bool):
        fcntl.flock(handle, fcntl.LOCK_EX if blocking else fcntl.LOCK_EX | fcntl.LOCK_NB)

    def _unlock(handle):
        fcntl.flock(handle, fcntl.LOCK_UN)


class MapLock:
    """Exclusive lock over every map. Waits for its turn instead of giving up."""

    def __init__(self, quiet=False):
        self.quiet = quiet
        self.handle = None

    def acquire(self):
        LOCK.parent.mkdir(parents=True, exist_ok=True)
        # Opened r+ and created if absent, never "w": truncating would drop the byte range the
        # Windows lock is taken on, and would do it *before* the lock is held.
        if not LOCK.exists():
            LOCK.touch()
        self.handle = open(LOCK, "r+")
        self.handle.write("\0")
        self.handle.flush()
        self.handle.seek(0)
        try:
            _lock(self.handle, blocking=False)
        except OSError:
            if not self.quiet:
                print("waiting for the rename lock ...", file=sys.stderr)
            start = time.monotonic()
            _lock(self.handle, blocking=True)  # blocks until the holder is done
            if not self.quiet:
                print(f"acquired after {time.monotonic() - start:.1f}s", file=sys.stderr)
        return self

    def release(self):
        if self.handle is None:
            return
        _unlock(self.handle)
        self.handle.close()
        self.handle = None

    def __enter__(self):
        return self.acquire()

    def __exit__(self, *exc):
        self.release()
        return False


def is_field(key: str) -> bool:
    """
    Whether a map key names a field, from its metadata token's table byte (0x04 field, 0x06 method).

    Read from the token rather than the annotation comment: the comments can be left over from a
    previous token assignment, and this is the one part of the key that cannot be stale.
    """
    token = key.rsplit("#", 1)[-1].lower()
    return token.startswith("0x04")


def field_names(members) -> set:
    return {value for key, _, value, _ in members if value and is_field(key)}


def applied_fields(applied: dict) -> set:
    return {name for key, name in applied.items() if is_field(key)}


def map_path(sample: str) -> Path:
    path = RENAMES / f"{sample}.json"
    if not path.exists():
        sys.exit(f"no map at {path}")
    return path


def type_entry(m: dict, wanted: str):
    """The (key-without-token, assigned name) for a type named or keyed by `wanted`."""
    hits = []
    for key, value in m.items():
        if owner_of(key):
            continue
        stem = key.rsplit("#", 1)[0]
        simple = stem.rsplit(".", 1)[-1].rsplit("/", 1)[-1]
        if wanted in (value, stem, simple):
            hits.append((stem, value))
    if not hits:
        sys.exit(f"no type matching {wanted!r}")
    if len(hits) > 1:
        sys.exit(f"{wanted!r} is ambiguous: {', '.join(h[0] for h in hits)}")
    return hits[0]


def members_of(path: Path, stem: str):
    """
    -> [(map key, member name, current value, annotation)] in file order.

    An entry can carry more than one annotation: `refresh_renames` re-emits notes keyed by token,
    and a pass that renumbers Method/Field RIDs makes some of those land on an entry they no longer
    describe. The annotation to trust is the one whose trailing identifier is this member's name;
    anything else belongs to whatever used to hold the token. Taking the last one -- which is what
    this did first -- reports a plausible signature for the wrong entity.
    """
    out, pending = [], []
    for line in path.read_text(encoding="utf-8").splitlines():
        stripped = line.strip()
        if stripped.startswith("//"):
            if re.match(r"//\s*(field|method)\s", stripped):
                pending.append(stripped.lstrip("/ ").strip())
            continue
        km = KEY_LINE.match(line)
        if km:
            key = km.group(2)
            mm = MEMBER_KEY.match(key)
            if mm and mm.group("owner") == stem:
                member = mm.group("member")
                match = [a for a in pending
                         if re.search(rf"\b{re.escape(member)}\s*(\(|$)", a)]
                if match:
                    annotation = match[0]
                elif len(pending) == 1:
                    annotation = pending[0]
                else:
                    # Several candidates and none names this member: say so rather than pick.
                    annotation = "(annotation uncertain -- read export/)" if pending else None
                out.append((key, member, km.group(3), annotation))
            pending = []
    return out


def cmd_show(args) -> int:
    path = map_path(args.sample)
    m = entries(path)
    stem, name = type_entry(m, args.type)
    members = members_of(path, stem)
    unnamed = [x for x in members if not x[2]]

    src = export_file(args.sample, stem, type_names(m))
    nested = "/" in stem
    print(f"sample   {args.sample}")
    print(f"type     {name or '(unnamed)'}   key {stem}")
    print(f"source   {src.relative_to(ROOT) if src else '(no single export file)'}"
          + ("   (nested -- search within this file)" if nested and src else ""))
    print(f"members  {len(members) - len(unnamed)}/{len(members)} named\n")
    for key, member, value, annotation in members:
        mark = "  " if value else "* "
        print(f"{mark}{member:34} {value or '':22} {annotation or ''}")
    if unnamed:
        print("\n* = still unnamed")
    return 0


def type_scope(stem: str) -> str:
    """
    The scope a type name has to be unique within: its enclosing type, or its namespace.

    Mirrors how ilrename groups types for its collision check, so a clash is reported here rather
    than at re-export, by which point the map has already been written.
    """
    return stem.rsplit("/", 1)[0] if "/" in stem else stem.rsplit(".", 1)[0]


def type_siblings(m: dict, stem: str) -> set:
    """The final names of every other type sharing this one's scope."""
    scope, out = type_scope(stem), set()
    for key, value in m.items():
        if owner_of(key):
            continue
        other = key.rsplit("#", 1)[0]
        if other == stem or type_scope(other) != scope:
            continue
        out.add(value or other.rsplit("/", 1)[-1].rsplit(".", 1)[-1])
    return out


def cmd_set(args) -> int:
    pairs = []
    for item in args.set:
        if "=" not in item:
            sys.exit(f"--set expects old=New, got {item!r}")
        old, new = item.split("=", 1)
        pairs.append((old.strip(), new.strip()))
    if not pairs and not args.set_type:
        sys.exit("nothing to set")

    # Everything below happens under the lock, including re-reading the map: a decision made
    # against a copy read earlier could be based on a state another worker has already replaced.
    with MapLock(quiet=args.quiet):
        path = map_path(args.sample)
        m = entries(path)
        stem, type_name = type_entry(m, args.type)
        members = members_of(path, stem)
        by_member = {}
        for key, member, value, _ in members:
            by_member.setdefault(member, []).append((key, value))

        applied, refused, corrected = {}, [], []

        # The type's own name, which lives on the one key for this type that has no "::" member
        # part. Handled here rather than by hand because a type rename is a write to the same map
        # and needs the same lock: the maps carry type names too, and editing one directly is the
        # lost-write this module exists to prevent.
        type_set = None
        if args.set_type:
            new = args.set_type.strip()
            key = next(k for k in m if not owner_of(k) and k.rsplit("#", 1)[0] == stem)
            if not IDENTIFIER.match(new) or new in RESERVED:
                refused.append((stem, new, "not a usable C# identifier"))
            elif type_name and not args.force:
                refused.append((stem, new, f"already named {type_name!r}; --force to correct it"))
            elif new in type_siblings(m, stem):
                refused.append((stem, new, f"{new!r} already names a type in {type_scope(stem)}"))
            else:
                if type_name:
                    corrected.append((stem, type_name, new))
                type_set = (key, new)

        for old, new in pairs:
            if not IDENTIFIER.match(new) or new in RESERVED:
                refused.append((old, new, "not a usable C# identifier"))
                continue
            hits = by_member.get(old)
            if not hits:
                refused.append((old, new, f"no member {old!r} on {type_name or stem}"))
                continue
            if len(hits) > 1:
                refused.append((old, new, "ambiguous member name"))
                continue
            key, value = hits[0]
            if value and not args.force:
                refused.append((old, new, f"already named {value!r}; --force to correct it"))
                continue
            if value:
                corrected.append((old, value, new))
            # Fields cannot overload, so a repeated field name is always a collision. Methods can,
            # and the original source plainly used overloads -- forcing a suffix on the second one
            # invents a distinction the program never had. ilrename is the authority here: its
            # method check keys on name + parameter types + return type, so it still catches a real
            # collision at re-export. The token's table byte is what says which kind this is, and
            # unlike the annotation comments it cannot go stale.
            if is_field(key) and (new in field_names(members) or new in applied_fields(applied)):
                refused.append((old, new, f"{new!r} already used by another field on this type"))
                continue
            applied[key] = new

        writes = dict(applied)
        if type_set:
            writes[type_set[0]] = type_set[1]

        if writes:
            out = []
            for line in path.read_text(encoding="utf-8").splitlines(keepends=True):
                km = KEY_LINE.match(line.rstrip("\n"))
                if km and km.group(2) in writes:
                    indent, key, _, comma = km.groups()
                    out.append(f'{indent}"{key}": "{writes[key]}"{comma}\n')
                else:
                    out.append(line)
            # Atomic replace, so an interrupted write cannot truncate the map.
            fd, tmp = tempfile.mkstemp(dir=str(path.parent), prefix=path.name, suffix=".tmp")
            with os.fdopen(fd, "w", encoding="utf-8") as fh:
                fh.write("".join(out))
            os.replace(tmp, path)

    if type_set:
        print(f"  set  type {stem} -> {type_set[1]}")
    for key, new in applied.items():
        print(f"  set  {MEMBER_KEY.match(key).group('member')} -> {new}")
    for old, was, new in corrected:
        print(f"  FIX  {old}: {was!r} -> {new!r}  (overwrote an existing name)")
    for old, new, why in refused:
        print(f"  SKIP {old} -> {new}: {why}")
    print(f"{args.sample}/{type_name or stem}: {len(writes)} set, {len(refused)} refused")
    return 1 if refused else 0


def main() -> int:
    ap = argparse.ArgumentParser(description="The only writer for renames/*.json.")
    ap.add_argument("-q", "--quiet", action="store_true", help="do not report lock waits")
    sub = ap.add_subparsers(dest="cmd", required=True)

    show = sub.add_parser("show", help="a type's members, named and not, with its export file")
    show.add_argument("--sample", required=True)
    show.add_argument("--type", required=True)
    show.set_defaults(func=cmd_show)

    st = sub.add_parser("set", help="assign member names under the lock")
    st.add_argument("--sample", required=True)
    st.add_argument("--type", required=True)
    st.add_argument("--set", action="append", metavar="OLD=NEW", default=[],
                    help="repeatable; OLD is the current member name")
    st.add_argument("--set-type", metavar="NEW",
                    help="name the type itself, not a member. Same lock, same rules: refused if the "
                         "type is already named (use --force) or if NEW already names a sibling "
                         "type in the same enclosing type or namespace")
    st.add_argument("--force", action="store_true",
                    help="replace a name that is already set. Off by default so concurrent workers "
                         "cannot silently clobber each other; use it to CORRECT a name known to be "
                         "wrong, and say in your report what it was and why it changed.")
    st.set_defaults(func=cmd_set)

    args = ap.parse_args()
    return args.func(args)


if __name__ == "__main__":
    raise SystemExit(main())
