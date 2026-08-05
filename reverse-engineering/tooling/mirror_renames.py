#!/usr/bin/env python3
"""
Mirror member names between two builds of the same plugin, by token, after checking the bodies agree.

Why this exists
---------------
ADOverhaul2019 and ADOverhaul2022 are the same plugin built for two Unity versions. Reactor
obfuscated them separately, so the same entity carries a different generated name in each -- but it
keeps the same metadata token, and de4dot preserves RIDs, so the maps are keyed by something that is
already common to both. Naming a member in one build and then hand-copying it into the other is pure
transcription, and transcription of ~950 entries is where mistakes come from.

What this does NOT do is trust the token on its own. Token identity across two separately-obfuscated
assemblies is an observation about these builds, not a guarantee -- RE_NOTES.md is explicit that it is
corroboration, never a substitute for matching on content. So every mirror is gated twice:

  1. the two `export/` files for the type must be identical once every identifier is masked out, so
     the bodies are the same code modulo naming; and
  2. the entry's kind and signature -- from the annotation ilrename writes above each key, with the
     member name itself removed -- must agree between the builds.

A member that fails either check is reported and left alone. Nothing is ever overwritten: if both
sides already carry a name and the names differ, that is a conflict for a human to resolve, not
something to silently pick a winner for.

    python3 scripts/mirror_renames.py                       # ADOverhaul2022 -> ADOverhaul2019
    python3 scripts/mirror_renames.py -n                    # report, write nothing
    python3 scripts/mirror_renames.py --type GUIColorScope
    python3 scripts/mirror_renames.py --from A --to B
"""

import argparse
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from refresh_renames import KEY_LINE, RENAMES, strip_comments, token_of  # noqa: E402
from rename_status import MEMBER_KEY, entries, export_file, owner_of, type_names  # noqa: E402
from apply_renames import MapLock  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent

DEFAULT_FROM = "ADOverhaul2022"
DEFAULT_TO = "ADOverhaul2019"

IDENT = re.compile(r"[A-Za-z_][A-Za-z0-9_]*")
# The annotation ilrename writes above each key, e.g. "//     field Color[] _ParamMethod"
# or "//     method Void AddIterator()".
ANNOTATION = re.compile(r"^\s*//\s*(?P<kind>field|method|class|struct|enum|interface)\s+(?P<rest>.*)$")


def masked(text: str) -> str:
    """Every identifier replaced by a single token, so only the code's shape remains."""
    return IDENT.sub("X", text)


DECL = r"\b(?:class|struct|interface|enum)\s+{}\b"


def type_body(path: Path, simple: str) -> str | None:
    """
    Just this type's declaration, carved out of its export file.

    Comparing whole files is wrong for a nested type, because the file it lives in is its outermost
    enclosing type's -- for most of these that is one huge shared file which differs between builds
    for reasons that have nothing to do with the type in hand. Comparing whole files rejected all but
    7 of 49 paired types for that reason alone.
    """
    text = path.read_text(encoding="utf-8")
    m = re.search(DECL.format(re.escape(simple)), text)
    if not m:
        return None
    open_brace = text.find("{", m.end())
    if open_brace == -1:
        return None
    depth = 0
    for i in range(open_brace, len(text)):
        if text[i] == "{":
            depth += 1
        elif text[i] == "}":
            depth -= 1
            if depth == 0:
                return text[m.start():i + 1]
    return None


def type_aliases(m: dict) -> dict:
    """
    Obfuscated simple type name -> assigned name, or "?" when nothing is assigned yet.

    A signature can name another obfuscated type -- most obviously a self-referential static field --
    and the two builds obfuscate that type to different names. Comparing the raw text would then
    reject a pair that is in fact identical, so both sides are rewritten into assigned names first,
    which is the vocabulary the builds actually share.
    """
    out = {}
    for key, value in m.items():
        if owner_of(key):
            continue
        simple = key.rsplit("#", 1)[0].rsplit(".", 1)[-1].rsplit("/", 1)[-1]
        out[simple] = value or "?"
    return out


def normalise(sig: str, aliases: dict) -> str:
    return IDENT.sub(lambda mo: aliases.get(mo.group(0), mo.group(0)), sig)


def signatures(path: Path) -> dict:
    """
    -> {key: (kind, signature-with-the-member-name-removed)}

    An entry can carry more than one annotation, because `refresh_renames` re-emits notes keyed by
    token and a pass that renumbers Method/Field RIDs lands some of them on entries they no longer
    describe. The one to trust is the annotation naming this member; taking the last, as this did
    first, compares a stale signature belonging to whatever previously held the token and rejects
    pairs that actually match.
    """
    out, pending = {}, []
    for line in path.read_text(encoding="utf-8").splitlines():
        ann = ANNOTATION.match(line)
        if ann:
            pending.append(ann)
            continue
        # A hand-written note may sit between the annotation and its key -- that is where the
        # reason for a name goes -- so a comment must not count as the end of the annotation.
        if line.lstrip().startswith("//"):
            continue
        km = KEY_LINE.match(line)
        if km and pending:
            key = km.group(2)
            member = MEMBER_KEY.match(key)
            chosen = pending[-1]
            if member:
                named = [a for a in pending
                         if re.search(rf"\b{re.escape(member.group('member'))}\s*(\(|$)",
                                      a.group("rest"))]
                if named:
                    chosen = named[0]
                elif len(pending) > 1:
                    # Ambiguous: no annotation names this member, so none can be trusted.
                    pending = []
                    continue
            rest = chosen.group("rest")
            if member:
                # Drop the member's own name; keep return type / field type and parameter list.
                rest = rest.replace(member.group("member"), "", 1)
            out[key] = (chosen.group("kind"), " ".join(rest.split()))
        pending = []
    return out


def top_level(type_key: str) -> str:
    """The outermost declaring type -- nested types live in their outer type's export file."""
    return type_key.split("/", 1)[0]


def main() -> int:
    ap = argparse.ArgumentParser(
        description="Mirror member names between two builds of the same plugin, by token, "
                    "after checking the bodies agree.")
    ap.add_argument("--from", dest="src", default=DEFAULT_FROM, help=f"source sample (default: {DEFAULT_FROM})")
    ap.add_argument("--to", dest="dst", default=DEFAULT_TO, help=f"target sample (default: {DEFAULT_TO})")
    ap.add_argument("--type", dest="type_filter", help="only types whose assigned name contains this")
    ap.add_argument("-n", "--dry-run", action="store_true", help="report, write nothing")
    ap.add_argument("-q", "--quiet", action="store_true", help="do not report lock waits")
    ap.add_argument("-v", "--verbose", action="store_true", help="name every mirrored member")
    args = ap.parse_args()

    src_path, dst_path = RENAMES / f"{args.src}.json", RENAMES / f"{args.dst}.json"
    for p in (src_path, dst_path):
        if not p.exists():
            print(f"no map at {p}")
            return 1

    # What to fill is decided from the map's contents, so the lock has to span the decision and the
    # write both -- deciding against a copy read before the lock is the same lost update the lock
    # exists to stop. A dry run reads only, so it takes nothing.
    lock = None
    if not args.dry_run:
        lock = MapLock(quiet=args.quiet).acquire()

    src_map, dst_map = entries(src_path), entries(dst_path)
    src_sig, dst_sig = signatures(src_path), signatures(dst_path)
    src_alias, dst_alias = type_aliases(src_map), type_aliases(dst_map)

    # Types are paired by the name they were both given -- that pairing was made on content by hand,
    # and this tool extends it to members rather than re-deriving it.
    def named_types(m):
        return {v: k.rsplit("#", 1)[0] for k, v in m.items() if v and owner_of(k) is None}

    src_types, dst_types = named_types(src_map), named_types(dst_map)
    shared = sorted(set(src_types) & set(dst_types))

    # key-without-token -> assigned name, for resolving an outer type's export filename.
    src_names, dst_names = type_names(src_map), type_names(dst_map)

    by_token_dst = {}
    for key in dst_map:
        if owner_of(key):
            by_token_dst.setdefault(token_of(key), []).append(key)

    fills: dict[str, str] = {}
    skipped_shape, conflicts, sig_mismatch, unpaired = [], [], [], []
    checked_files: dict[str, bool] = {}

    for name in shared:
        if args.type_filter and args.type_filter.lower() not in name.lower():
            continue
        src_type, dst_type = src_types[name], dst_types[name]

        # Gate 1: this type's own body must be the same code modulo naming.
        if name not in checked_files:
            a = export_file(args.src, src_type, src_names)
            b = export_file(args.dst, dst_type, dst_names)
            body_a = type_body(a, name) if a else None
            body_b = type_body(b, name) if b else None
            checked_files[name] = bool(body_a and body_b
                                       and masked(body_a) == masked(body_b))
        if not checked_files[name]:
            skipped_shape.append(name)
            continue

        for key, value in src_map.items():
            if not value or owner_of(key) != src_type:
                continue
            token = token_of(key)
            candidates = by_token_dst.get(token, [])
            twin = next((c for c in candidates if owner_of(c) == dst_type), None)
            if twin is None:
                unpaired.append((name, key))
                continue
            # Gate 2: same kind and same signature, with the member's own name removed and any
            # obfuscated type name rewritten to the name both builds agreed on.
            a_sig, b_sig = src_sig.get(key), dst_sig.get(twin)
            if a_sig and b_sig:
                a_sig = (a_sig[0], normalise(a_sig[1], src_alias))
                b_sig = (b_sig[0], normalise(b_sig[1], dst_alias))
            if a_sig != b_sig:
                sig_mismatch.append((name, key, src_sig.get(key), dst_sig.get(twin)))
                continue
            existing = dst_map.get(twin) or ""
            if existing and existing != value:
                conflicts.append((name, twin, existing, value))
            elif not existing:
                fills[twin] = value

    # Rewrite in place, line by line, so every annotation and hand-written comment survives.
    if fills and not args.dry_run:
        out = []
        for line in dst_path.read_text(encoding="utf-8").splitlines(keepends=True):
            km = KEY_LINE.match(line.rstrip("\n"))
            if km and km.group(2) in fills:
                indent, key, _, comma = km.groups()
                out.append(f'{indent}"{key}": "{fills[km.group(2)]}"{comma}\n')
            else:
                out.append(line)
        dst_path.write_text("".join(out), encoding="utf-8")

    if lock:
        lock.release()

    verb = "would fill" if args.dry_run else "filled"
    print(f"{args.src} -> {args.dst}: {len(shared)} type(s) paired by name, {verb} {len(fills)} member(s)")
    if args.verbose:
        for key, value in sorted(fills.items()):
            print(f"    {token_of(key)}  {MEMBER_KEY.match(key).group('member')} -> {value}")
    if skipped_shape:
        print(f"\n  {len(skipped_shape)} type(s) skipped -- export bodies differ once identifiers are "
              f"masked, so these are not the same code and must be named separately:")
        for n in skipped_shape:
            print(f"    {n}")
    if sig_mismatch:
        print(f"\n  {len(sig_mismatch)} member(s) skipped -- same token, different kind/signature:")
        for n, key, a, b in sig_mismatch[:20]:
            print(f"    {n}::{MEMBER_KEY.match(key).group('member')}  {a}  vs  {b}")
    if unpaired:
        print(f"\n  {len(unpaired)} named member(s) had no counterpart token in {args.dst}:")
        for n, key in unpaired[:20]:
            print(f"    {n}::{MEMBER_KEY.match(key).group('member')}  {token_of(key)}")
    if conflicts:
        print(f"\n  {len(conflicts)} CONFLICT(s) -- both sides named, differently. Nothing was "
              f"overwritten; resolve by hand:")
        for n, key, have, want in conflicts:
            print(f"    {n}::{MEMBER_KEY.match(key).group('member')}  has {have!r}, source says {want!r}")
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
