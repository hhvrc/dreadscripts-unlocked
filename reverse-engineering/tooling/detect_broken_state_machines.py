#!/usr/bin/env python3
"""Read-only classifier for residual while(true)/switch state machines in exports."""

import argparse
import sys
from collections import Counter
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import pipeline as pl  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent


def scan_file(path: Path):
    """Return conservative Roslyn-derived state-machine classifications for one C# file."""
    return [
        (
            record["verdict"],
            record["method"],
            record["whileLine"],
            record["variable"],
            record["seed"],
            record["path"],
        )
        for record in pl.source_analysis("state-machines", path)
    ]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--dir", default="export", help="tree to scan (default: export)")
    parser.add_argument("--show", choices=["LOOPS", "TERMINATES", "UNKNOWN", "ALL"])
    args = parser.parse_args()

    root = ROOT / args.dir
    if not root.is_dir():
        sys.exit(f"error: no such directory: {root}")

    totals, per_assembly, listed = Counter(), {}, []
    for source in sorted(root.rglob("*.cs")):
        results = scan_file(source)
        if not results:
            continue
        assembly = source.relative_to(root).parts[0]
        per_assembly.setdefault(assembly, Counter())
        for verdict, method, line, variable, seed, path in results:
            totals[verdict] += 1
            per_assembly[assembly][verdict] += 1
            if args.show in (verdict, "ALL"):
                listed.append((source, verdict, method, line, variable, seed, path))

    print("=== state-machine scan ===")
    print(f"TERMINATES: {totals['TERMINATES']}")
    print(f"LOOPS:      {totals['LOOPS']}")
    print(f"UNKNOWN:    {totals['UNKNOWN']}")
    for assembly, counts in sorted(per_assembly.items()):
        print(f"  {assembly}: T={counts['TERMINATES']} L={counts['LOOPS']} U={counts['UNKNOWN']}")

    for source, verdict, method, line, variable, seed, path in listed:
        print(f"{verdict:10} {source.relative_to(root)}:{line} {method}() "
              f"{variable}={seed} trace={' -> '.join(map(str, path or []))}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
