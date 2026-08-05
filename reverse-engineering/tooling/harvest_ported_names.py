#!/usr/bin/env python3
"""
Write names chosen during porting back into renames/, reading them out of the ported headers.

Why this exists
---------------
A name decided while porting is recorded twice: in the ported file (as the identifier, and as the
`<decompiled name> -> <ported name>` MAP entry in its header) and in `renames/<sample>.json`, which
is the only copy the pipeline can act on. The second half gets forgotten, because the port compiles
and reads correctly without it -- so the divergence is invisible from either side.

It is not a small drift. When this was first measured, the package's headers recorded 1,169 names
that `renames/` still had as unnamed. `rename_status.py` was therefore reporting coverage far below
the truth, and `ManageWrapper` -- ported as `SyncSelection` in a wave whose commit message says so
outright -- was indistinguishable from a member nobody had ever looked at. Anything that reasons
about what is left to name from `renames/` alone was reading a number that was wrong by roughly a
third of the work already done.

The headers are the right source to recover from: HEADER-FORMAT.md requires the MAP entry, and
`check-headers.py` already enforces that the ported name in it is a member the file really declares.

    python3 scripts/harvest_ported_names.py              # report what would be written
    python3 scripts/harvest_ported_names.py --apply      # write it, via apply_renames.py

What it deliberately will not do
--------------------------------
It only fills members `renames/` has as UNNAMED, and it delegates every write to
`apply_renames.py set`, which holds the lock and refuses to overwrite an existing name. So this can
never clobber a name a human chose; the worst it can do is decline to add one.

It skips anything ambiguous rather than guessing: a decompiled name that occurs under more than one
type, or that the package maps to more than one ported name. Those are reported and left for a human,
because picking wrong here writes a wrong name into the assembly metadata, which is exactly the
failure the no-unverified-reconstruction rule exists to prevent.

A caveat worth knowing before running it
----------------------------------------
Once a name is in `renames/`, the next `reexport.py` makes `export/` show the NEW name -- so the
MAP entry's LEFT column, which HEADER-FORMAT.md defines as "the obfuscated identifier as it appears
in reverse-engineering/export/", stops matching anything there. That is already true of the names written back by
hand in earlier waves (`EditorUtils.Behaviours.cs` maps `InvokeResolver -> ForEach` while `export/`
has said `ForEach` at that line for some time). This makes the condition more widespread, not new.
The ported name is the durable reference either way, which is what those headers' own boilerplate
says; a pass to refresh the left columns after a re-export is the clean fix.
"""

import argparse
import re
import subprocess
import sys
from collections import defaultdict
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from rename_status import entries, owner_of  # noqa: E402
from refresh_renames import RENAMES  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
PACKAGE = ROOT.parent / "unity" / "Assets" / "com.dreadscripts.unlocked" / "Editor"

SRC = re.compile(r"^//\s*Reconstructed from:\s*(\S+)")
# The MAP forms that carry a decompiled name and a ported name. Sub-entries (no line number) are
# included deliberately: a nested type's field is still a member that needs its name back.
MAP = re.compile(r"^//\s{3,}(?P<d>.+?)\s*->\s*(?P<p>.+?)\s*(?:,\s*lines? \d+.*)?$")

IDENTIFIER = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")
# Ported-name column entries that name no single identifier, so there is nothing to write back.
NOT_A_NAME = re.compile(r"^(NOT PORTED|same|unchanged|the |this\[|inlined|lifted|dissolved)", re.I)


DECL = re.compile(r"^\s*(?:\[[^\]]*\]\s*)?(?:(?:internal|public|private|protected|static|sealed"
                  r"|abstract|partial|readonly|unsafe|new)\s+)*(?:class|struct|interface|enum"
                  r"|record)\s+(?P<name>[A-Za-z_]\w*)")


def harvest() -> list:
    """
    One record per ported file: (sample, {ported type names it declares}, {decompiled: {ported}}).

    Kept per file rather than flattened into one map per sample, because the file is what says
    which TYPE its entries belong to -- and without that, matching a member by name alone across
    the whole assembly picks the wrong one. Measured, not feared: flattening proposed writing
    BehaviourPropertyMultiEditor's `entry` onto an unrelated NodeType, and a `colorTexture` onto
    SupportThankies.Info, a type this package excludes entirely and will never port.

    Line numbers cannot stand in for the type either. They are stale by a different amount in
    different files -- uniformly +204 across the ADOverhaul partials, exact in the MultiEditors,
    and never decompiled lines at all in MenuSelector -- so resolving the enclosing type by line
    would land on the wrong type just as silently.
    """
    out = []
    for path in sorted(PACKAGE.rglob("*.cs")):
        sample, pairs, types, in_header = None, defaultdict(set), set(), True
        for line in path.read_text(encoding="utf-8", errors="replace").splitlines():
            if in_header and not line.startswith("//"):
                if line.strip():
                    in_header = False
            if not in_header:
                d = DECL.match(line)
                if d:
                    types.add(d.group("name"))
                continue
            m = SRC.match(line)
            if m:
                parts = m.group(1).split("/")
                sample = parts[1] if len(parts) > 1 else None
            g = MAP.match(line)
            if not (g and sample):
                continue
            # Strip the modifier the headers prefix onto statics, and any signature.
            decompiled = re.sub(r"^(static|nested)\s+(class|struct|readonly struct)?\s*", "",
                                g.group("d").strip()).split("(")[0].strip()
            ported = g.group("p").strip()
            if IDENTIFIER.match(decompiled) and not NOT_A_NAME.match(ported):
                ported = ported.split("(")[0].strip()
                # An identity mapping is a member that was never obfuscated -- OnGUI, Awake, a
                # [SerializeField] that had to keep its name. Writing it back renames nothing and
                # actively misleads: rename_status counts it as "named" instead of "already carries
                # its real name", which inflates the coverage figure this whole exercise exists to
                # make honest. It also makes ilrename warn about a string-literal collision for a
                # rename that is not happening.
                if IDENTIFIER.match(ported) and ported != decompiled:
                    pairs[decompiled].add(ported)
        if sample and pairs:
            out.append((sample, types, pairs))
    return out


def simple(name: str) -> str:
    """The last segment of a type key: 'Ns.Outer/Nested#0xTOK' -> 'Nested'."""
    return name.rsplit("#", 1)[0].rsplit("/", 1)[-1].rsplit(".", 1)[-1]


def index(sample: str):
    """(types by the names they can be referred to, unnamed members by (type key, member))."""
    path = RENAMES / f"{sample}.json"
    if not path.exists():
        return None, None
    by_name, unnamed = defaultdict(set), {}
    for key, value in entries(path).items():
        owner = owner_of(key)
        if owner is None:
            # A type is findable under its obfuscated simple name AND under whatever it was
            # renamed to, because the ported file only ever shows the latter.
            type_key = key.rsplit("#", 1)[0]
            by_name[simple(key)].add(type_key)
            if value:
                by_name[value].add(type_key)
        elif not value:
            unnamed[(owner, key.split("::", 1)[1].rsplit("#", 1)[0])] = key
    return by_name, unnamed


def plan(records: list, sample_filter=None):
    """(writes, skipped) across all ported files. writes is {(sample, type key): [(old, new)]}."""
    # {(sample, type key): {decompiled: ported}} -- a dict, not a list, because one member is often
    # recorded by several ported files (a class split into partials repeats its field table), and
    # counting those repeats as distinct members made the collapse check below see "CloneReg,
    # CloneReg" as two members fighting over one name.
    cache, writes, skipped = {}, defaultdict(dict), []
    for sample, ported_types, pairs in records:
        if sample_filter and sample != sample_filter:
            continue
        if sample not in cache:
            cache[sample] = index(sample)
        by_name, unnamed = cache[sample]
        if by_name is None:
            continue

        # The types this file is responsible for. A file declares the ported type name, so that is
        # what binds its MAP entries to a decompiled type -- not the member name, and not a line.
        scope = {k for t in ported_types for k in by_name.get(t, ())}
        if not scope:
            skipped.append((sample, "?", sorted(ported_types)[:1], "no decompiled type resolved"))
            continue

        for decompiled, ported_names in sorted(pairs.items()):
            hits = [(t, unnamed[(t, decompiled)]) for t in scope if (t, decompiled) in unnamed]
            if not hits:
                continue                    # already named, or not a member of any type in scope
            if len(hits) > 1 or len(ported_names) > 1:
                skipped.append((sample, decompiled, sorted(ported_names),
                                "maps to several ported names" if len(ported_names) > 1
                                else f"unnamed under {len(hits)} of this file's types"))
                continue
            writes[(sample, hits[0][0])][decompiled] = next(iter(ported_names))

    # A port may collapse several decompiled members into one ported member -- EditorUtils'
    # PushResolver and SetupQueue have identical bodies and become a single GetTextWidth, which that
    # file's header states outright. The package can say that; renames/ cannot, because renaming both
    # produces two metadata members of identical signature. ilrename refuses the whole map when that
    # happens, so one collapsed pair anywhere costs the entire re-export -- which is exactly how this
    # was found. Drop every member of a collapsed group rather than picking one arbitrarily: which of
    # them "is" the ported member is a judgement about intent, not something to infer from a name.
    #
    # This does also drop genuine overloads, whose differing signatures ilrename would have accepted.
    # That is the safe direction: the cost is a name not written, against an export that will not build.
    for key, pairs in list(writes.items()):
        by_new = defaultdict(list)
        for old, new in pairs.items():
            by_new[new].append(old)
        collapsed = {new for new, olds in by_new.items() if len(olds) > 1}
        if collapsed:
            writes[key] = {o: n for o, n in pairs.items() if n not in collapsed}
            for new in sorted(collapsed):
                skipped.append((key[0], ", ".join(sorted(by_new[new])), [new],
                                "several decompiled members collapse into one ported member"))
            if not writes[key]:
                del writes[key]
    return writes, skipped


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--apply", action="store_true", help="write the names (default: report only)")
    ap.add_argument("--sample", help="limit to one sample")
    args = ap.parse_args()

    if not PACKAGE.is_dir():
        sys.exit(f"error: no ported package at {PACKAGE}")

    writes, skipped = plan(harvest(), args.sample)
    failed = 0

    per_sample = defaultdict(int)
    for (sample, _), pairs in writes.items():
        per_sample[sample] += len(pairs)
    for sample in sorted(per_sample):
        types = sum(1 for s, _ in writes if s == sample)
        print(f"{sample}: {per_sample[sample]} name(s) to write across {types} type(s)")

    for sample, decompiled, ported, why in skipped:
        print(f"      skip {sample:16} {decompiled:28} {why}"
              + (f": {', '.join(ported)}" if ported else ""))

    if args.apply:
        for (sample, type_key), pairs in sorted(writes.items()):
            cmd = [sys.executable, str(Path(__file__).parent / "apply_renames.py"), "-q", "set",
                   "--sample", sample, "--type", type_key]
            for old, new in sorted(pairs.items()):
                cmd += ["--set", f"{old}={new}"]
            res = subprocess.run(cmd, capture_output=True, text=True)
            out = res.stdout + res.stderr
            # apply_renames exits non-zero when ANY member in the batch was refused, so the exit
            # code says nothing about how many landed. Read its own tally instead of assuming the
            # whole batch failed -- doing that reported 65 failures for a run that in fact wrote
            # everything it could, and the residual dry-run was what caught the lie.
            tally = re.search(r"(\d+) set, (\d+) refused", out)
            refused = int(tally.group(2)) if tally else (len(pairs) if res.returncode else 0)
            failed += refused
            if refused:
                reasons = "; ".join(sorted({m.strip() for m in
                                            re.findall(r"SKIP .*?: (.*)$", out, re.M)}))
                print(f"      {refused} refused in {simple(type_key)}: {reasons[:150]}")

    total = sum(per_sample.values())
    verb = "wrote" if args.apply else "would write"
    print(f"\n{verb} {total - failed} name(s); {len(skipped)} skipped as ambiguous"
          + (f"; {failed} refused by apply_renames" if failed else ""))
    if not args.apply:
        print("re-run with --apply to write them")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
