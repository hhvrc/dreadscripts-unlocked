#!/usr/bin/env python3
"""
Which methods a commit actually changed in the decompiled tree.

Why this exists
---------------
Reviewing a re-export means reviewing *methods*, and neither of the obvious shortcuts gives you the
right set. Filenames are far too coarse — one file holds hundreds of methods and a 248-line diff may
touch three. The rejection log is worse than coarse, it is a different question entirely: a de4dot
change resolves methods that were never rejected, so a review scoped to the rejection set silently
skips them. That gap is exactly how a large green-gated export gets accepted with an unreviewed
semantic change in it.

So: take the commit's own diff, map every changed line on the new side to the method that encloses
it, and report the union. A method appears if and only if a hunk landed inside it. Line-to-method
mapping comes from the Roslyn helper (`methods`), not a brace scan, so partial classes, nested types
and expression-bodied members are handled by a parser rather than a guess.

    python3 scripts/changed_methods.py de190c6
    python3 scripts/changed_methods.py de190c6 --json /tmp/changed.json
"""

import argparse
import json
import subprocess
import sys
from collections import defaultdict
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import pipeline as pl  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent


def changed_files(rev: str, subtree: str) -> list[str]:
    out = subprocess.run(["git", "diff", "--name-only", f"{rev}^", rev, "--", subtree],
                         cwd=ROOT, capture_output=True, text=True, check=True)
    return [line for line in out.stdout.splitlines() if line.endswith(".cs")]


def changed_lines(rev: str, path: str) -> set[int]:
    """Line numbers on the NEW side that the commit added or modified."""
    out = subprocess.run(["git", "diff", "--unified=0", f"{rev}^", rev, "--", path],
                         cwd=ROOT, capture_output=True, text=True, check=True)
    lines: set[int] = set()
    for line in out.stdout.splitlines():
        if not line.startswith("@@"):
            continue
        # @@ -old,count +new,count @@
        new = line.split("+", 1)[1].split(" ", 1)[0]
        start, _, count = new.partition(",")
        start, count = int(start), int(count or 1)
        lines.update(range(start, start + count))
    return lines


def deleted_line_anchors(rev: str, path: str) -> set[int]:
    """
    New-side positions where a pure deletion happened.

    A hunk that only removes lines has zero new-side length, so it would map to no method at all --
    yet a deletion is the change most worth reviewing. Anchor it to the surrounding line instead.
    """
    out = subprocess.run(["git", "diff", "--unified=0", f"{rev}^", rev, "--", path],
                         cwd=ROOT, capture_output=True, text=True, check=True)
    anchors: set[int] = set()
    for line in out.stdout.splitlines():
        if not line.startswith("@@"):
            continue
        new = line.split("+", 1)[1].split(" ", 1)[0]
        start, _, count = new.partition(",")
        if int(count or 1) == 0:
            anchors.add(max(1, int(start)))
    return anchors


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("rev", help="commit to inspect (its diff against its parent)")
    parser.add_argument("--subtree", default="export", help="tree to restrict to (default: export)")
    parser.add_argument("--json", type=Path, help="write the full result here")
    args = parser.parse_args()

    files = changed_files(args.rev, args.subtree)
    if not files:
        sys.exit(f"error: {args.rev} changed no .cs files under {args.subtree}/")

    results = []
    by_sample: dict[str, list] = defaultdict(list)
    for rel in files:
        path = ROOT / rel
        if not path.exists():
            results.append({"file": rel, "method": "<file deleted>", "type": "", "sample": ""})
            continue
        touched = changed_lines(args.rev, rel) | deleted_line_anchors(args.rev, rel)
        methods = pl.source_analysis("methods", path)
        sample = Path(rel).relative_to(args.subtree).parts[0]
        hit = []
        for m in methods:
            if any(m["startLine"] <= n <= m["endLine"] for n in touched):
                hit.append(m)
        # Lines outside every method (fields, type headers) still need surfacing -- an unexplained
        # change is the thing this is meant to prevent.
        covered = {n for n in touched
                   if any(m["startLine"] <= n <= m["endLine"] for m in methods)}
        outside = sorted(touched - covered)
        for m in hit:
            entry = {"sample": sample, "file": rel, "type": m["type"], "method": m["name"],
                     "startLine": m["startLine"], "endLine": m["endLine"]}
            results.append(entry)
            by_sample[sample].append(entry)
        if outside:
            entry = {"sample": sample, "file": rel, "type": "", "method": "<outside any method>",
                     "lines": outside[:20]}
            results.append(entry)
            by_sample[sample].append(entry)

    for sample in sorted(by_sample):
        print(f"\n=== {sample}")
        for e in by_sample[sample]:
            where = f"{e['file'].split('/')[-1]}"
            if e["method"] == "<outside any method>":
                print(f"  {where}: CHANGED OUTSIDE ANY METHOD at lines {e['lines']}")
            else:
                print(f"  {where}: {e['type']}::{e['method']}  (lines {e['startLine']}-{e['endLine']})")

    total = sum(1 for r in results if not r["method"].startswith("<"))
    print(f"\n{total} changed method(s) across {len(files)} file(s)")
    if args.json:
        args.json.write_text(json.dumps(results, indent=2), encoding="utf-8")
        print(f"wrote {args.json}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
