#!/usr/bin/env python3
"""
Measure how much compiler-generated closure structure survives into a decompiled tree.

Why this exists
---------------
`ROADMAP.md` in the de4dot fork carried a readability item worded "nested `<>c__DisplayClass`
closures are not recursively inlined". That is a claim about the *output*, and it had never been
measured -- so it could not be sized, and there was no way to tell an obfuscator artifact de4dot
should remove from a decompiler limitation de4dot cannot do anything about. This produces that
number, and splits it by cause.

What it measures
----------------
A closure the decompiler inlines leaves no trace: it becomes a lambda and its type disappears from
the output. So every closure type still present is one that was NOT inlined, and its shape says why:

  residue   a static field of the closure's own type -- .NET Reactor injects one per closure, and it
            is the thing that stops the decompiler recognising the type at all. de4dot's
            DisplayClassCleaner is supposed to strip these, so any that survive are a de4dot bug.
  captured  an instance field whose type is another closure -- a captured parent scope. This is the
            "nested" case, and it is a documented decompiler limitation, not obfuscation.
  plain     neither -- a closure the decompiler declined to inline for its own reasons.

The first two are independent, not exclusive: a closure can carry residue *and* capture a parent, and
is counted under both. Only "plain" is a true complement, so the three do not sum to the total.

Reads only the canonical `export/` tree; never runs de4dot and never writes to export/. All
toolchain access goes through pipeline.py, which owns the source-analysis helper.
"""

import argparse
import json
import sys
from collections import Counter
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import pipeline as pl  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent


def scan_tree(root: Path) -> tuple[dict, list]:
    """Return (closureTypes by name, constructionSites) for one decompiled tree."""
    types: dict[str, dict] = {}
    sites: list[dict] = []
    for source in sorted(root.rglob("*.cs")):
        if source.name == "AssemblyInfo.cs":
            continue
        rel = str(source.relative_to(root))
        for record in pl.source_analysis("closures", source):
            record["file"] = rel
            if record["kind"] == "closureType":
                # A name can legitimately repeat across outer types; key on file+name so the two
                # are not silently merged into one.
                types[f"{rel}::{record['name']}"] = record
            else:
                sites.append(record)
    return types, sites


def matches(record: dict, bucket: str) -> bool:
    """The two causes are independent — a closure can carry residue *and* capture a parent."""
    if bucket == "residue":
        return bool(record["staticSelfRefFields"])
    if bucket == "captured":
        return bool(record["capturedClosureFields"])
    if bucket == "plain":
        return not record["staticSelfRefFields"] and not record["capturedClosureFields"]
    return True


def summarize(name: str, types: dict, sites: list) -> dict:
    counts = Counter()
    for t in types.values():
        for bucket in ("residue", "captured", "plain"):
            if matches(t, bucket):
                counts[bucket] += 1

    named = {bucket: {t["name"] for t in types.values() if matches(t, bucket)}
             for bucket in ("residue", "captured", "plain")}
    site_counts = Counter()
    for site in sites:
        for bucket in ("residue", "captured", "plain"):
            if site["closureType"] in named[bucket]:
                site_counts[bucket] += 1

    return {
        "sample": name,
        "closureTypes": len(types),
        "closureTypesResidue": counts["residue"],
        "closureTypesCaptured": counts["captured"],
        "closureTypesPlain": counts["plain"],
        "constructionSites": len(sites),
        "sitesResidue": site_counts["residue"],
        "sitesCaptured": site_counts["captured"],
        "sitesPlain": site_counts["plain"],
    }


ROWS = [
    ("closureTypes", "closure types still present"),
    ("closureTypesResidue", "  ...carrying obfuscator residue (de4dot's to fix)"),
    ("closureTypesCaptured", "  ...capturing a parent closure (decompiler limit)"),
    ("closureTypesPlain", "  ...neither"),
    ("constructionSites", "explicit construction sites"),
    ("sitesResidue", "  ...of a residue-carrying closure"),
    ("sitesCaptured", "  ...of a parent-capturing closure"),
    ("sitesPlain", "  ...of neither"),
]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--dir", default="export", help="tree to scan (default: export)")
    parser.add_argument("--sample", action="append",
                        help="sample subdirectory (repeatable; default: all)")
    parser.add_argument("--list", choices=["residue", "captured", "plain", "all"],
                        help="also list the affected closure types and their construction sites")
    parser.add_argument("--json", type=Path, help="write the full per-sample result here")
    args = parser.parse_args()

    root = ROOT / args.dir
    if not root.is_dir():
        sys.exit(f"error: no such directory: {root}")

    samples = args.sample or sorted(p.name for p in root.iterdir() if p.is_dir())
    results, detail = [], {}
    for name in samples:
        tree = root / name
        if not tree.is_dir():
            sys.exit(f"error: no such sample tree: {tree}")
        types, sites = scan_tree(tree)
        results.append(summarize(name, types, sites))
        detail[name] = {"types": types, "sites": sites}

    for result in results:
        print(f"\n=== closures: {result['sample']} ===")
        for key, label in ROWS:
            print(f"  {label:<48} {result[key]:>6}")

    if args.list:
        for name in samples:
            types, sites = detail[name]["types"], detail[name]["sites"]
            wanted = {t["name"] for t in types.values()
                      if args.list == "all" or matches(t, args.list)}
            if not wanted:
                continue
            print(f"\n--- {name}: {args.list} closures")
            for key in sorted(types):
                t = types[key]
                if t["name"] not in wanted:
                    continue
                captured = ", ".join(f"{f['field']}:{f['type']}" for f in t["capturedClosureFields"])
                print(f"  {t['file']}:{t['line']} {t['name']}  "
                      f"fields={t['instanceFields']} "
                      f"residue={t['staticSelfRefFields'] or '-'} "
                      f"captures={captured or '-'} "
                      f"targets={','.join(t['delegateTargets']) or '-'}")
            for site in sites:
                if site["closureType"] in wanted:
                    print(f"    site {site['file']}:{site['line']} in {site['method']}()"
                          f" -> {site['closureType']}")

    total = {key: sum(r[key] for r in results) for key, _ in ROWS}
    print("\n=== total ===")
    for key, label in ROWS:
        print(f"  {label:<48} {total[key]:>6}")

    if args.json:
        args.json.write_text(json.dumps(results, indent=2), encoding="utf-8")
        print(f"\nwrote {args.json}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
