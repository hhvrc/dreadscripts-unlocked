#!/usr/bin/env python3
"""
Measure the .NET Reactor opaque-predicate pairs that survive into export/.

The pattern
-----------
Reactor injects, into class C, a static field of type C that nothing ever assigns, plus a
parameterless static bool whose whole body is `return thatField == null;`. Since the field is never
assigned the predicate is a constant `true`. Both members are scaffolding.

Removing them is de4dot's job, not this repo's -- `OpaquePredicateRemover` in the fork drops the ones
it can prove dead, and that pass owns the rule for when removal is safe. This script measures what is
left over afterwards, which is the number that says how much headroom that pass still has. It never
writes anything.

Reading the output
------------------
"provably dead" here is a weaker claim than the deobfuscator's. This works from decompiled C# after
the fact, so it can only see the final module; the pass sees the module mid-pipeline, where other
injected methods still read these same fields, and it refuses anything it cannot prove. A pair
counted dead here and kept by the pass is that gap, not a bug in either.

One trap this script exists to avoid: Reactor reuses method names across unrelated classes, so
grepping for `Foo()` across the corpus finds calls to a *different* Foo and makes a dead predicate
look live. Every apparent call site found so far has been one of those collisions, so a call only
counts when no other method in the corpus shares the name.

    python3 scripts/find_reactor_decoys.py
    python3 scripts/find_reactor_decoys.py -v      # list every surviving pair
"""

import argparse
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
EXPORT = ROOT / "export"

# `internal static bool IsInactive() { return activeScope == null; }`
PREDICATE = re.compile(
    r"(?:internal|public|private)\s+static\s+bool\s+(?P<method>\w+)\(\)\s*\{\s*"
    r"return\s+(?P<field>\w+)\s*==\s*null;\s*\}", re.S)


def declaring_class(text: str, field: str) -> str | None:
    """
    The class the pair belongs to, read off the field's own declaration.

    The field's declared type *is* its declaring class -- that is the shape of the pattern -- so this
    avoids having to track brace nesting to work out which nested type a match sits in.
    """
    m = re.search(rf"static\s+(?P<type>[\w.]+)\s+{re.escape(field)}\s*;", text)
    return m.group("type").rsplit(".", 1)[-1] if m else None


def scan(sample_dirs):
    """-> [(sample, class, field, method, live_calls, assignments)]"""
    files = {p: p.read_text(errors="ignore") for d in sample_dirs for p in d.rglob("*.cs")}

    def definitions(name):
        return sum(len(re.findall(rf"\b[\w\[\]<>,`.]+\s+{re.escape(name)}\s*\(", t))
                   for t in files.values())

    found = []
    for path, text in files.items():
        for m in PREDICATE.finditer(text):
            method, field = m.group("method"), m.group("field")
            cls = declaring_class(text, field)
            if cls is None:
                continue
            assigned = len(re.findall(rf"\b{re.escape(field)}\s*=(?!=)", text))
            calls = sum(len(re.findall(rf"(?<!bool ){re.escape(method)}\(\)", t))
                        for t in files.values())
            live = calls if (calls and definitions(method) == 1) else 0
            found.append((path.relative_to(EXPORT).parts[0], cls, field, method, live, assigned))
    return found


def main() -> int:
    ap = argparse.ArgumentParser(
        description="Measure the Reactor opaque-predicate pairs surviving into export/.")
    ap.add_argument("-v", "--verbose", action="store_true", help="list every surviving pair")
    args = ap.parse_args()

    if not EXPORT.is_dir():
        print(f"no export tree at {EXPORT}")
        return 1
    samples = sorted(d for d in EXPORT.iterdir() if d.is_dir())

    found = scan(samples)
    real = [f for f in found if f[4] or f[5]]
    dead = [f for f in found if not f[4] and not f[5]]

    print(f"{len(found)} opaque-predicate pair(s) still in export/ across {len(samples)} sample(s)")
    by_sample = {}
    for f in dead:
        by_sample.setdefault(f[0], []).append(f)
    for s in sorted({f[0] for f in found}):
        print(f"  {s:18} {len(by_sample.get(s, [])):>4} dead-looking")
    print(f"\n  {len(dead)} look dead from the decompiled source -- headroom for the fork's "
          f"OpaquePredicateRemover")
    print(f"  {len(real)} are not scaffolding at all (assigned, or called under a unique name):")
    for s, cls, field, method, live, asg in real:
        why = []
        if live:
            why.append(f"{live} call site(s)")
        if asg:
            why.append(f"{asg} assignment(s)")
        print(f"      {s}/{cls}::{method}  ({', '.join(why)})")

    if args.verbose:
        for s, pairs in sorted(by_sample.items()):
            print(f"\n  {s}:")
            for _, cls, field, method, _, _ in sorted(pairs, key=lambda x: x[1]):
                print(f"      {cls}::{field} / {method}()")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
