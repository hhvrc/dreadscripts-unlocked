#!/usr/bin/env python3
"""Convert header lines that write the line number in parentheses before the arrow.

    //   OnEnable  (decompiled line 162) -> PrintConfiguration
becomes
    //   OnEnable -> PrintConfiguration, line 162

That is the only transformation here. The sub-entry form (an arrow line with no
line number) needs no migration -- it is legal as written, and re-indenting it
would break the hand-aligned continuation lines those blocks rely on.

"not ported" lines are left alone and reported: they carry no line number, so
converting them to the NOT PORTED form means looking each one up in decompiled/,
which is a judgement call rather than a rewrite.

Usage: python tools/migrate-headers.py [path] [--apply]
"""
import os
import re
import sys
import collections

DEFAULT_ROOT = 'unity/Assets/com.dreadscripts.unlocked'

PAREN = re.compile(
    r'^(?P<i>//\s{3})(?P<d>.+?)\s+\((?:decompiled )?lines? (?P<n>\d+)\)\s*->\s*(?P<p>.+?)\s*$')
NOT_PORTED = re.compile(r'->\s*not ported', re.I)


def migrate(path, apply):
    src = open(path, encoding='utf-8', newline='').read()
    nl = '\r\n' if '\r\n' in src else '\n'
    lines = src.split(nl)
    end = next((i for i, l in enumerate(lines)
                if l.strip() and not l.startswith('//')), len(lines))

    out = list(lines)
    counts = collections.Counter()
    for i in range(end):
        m = PAREN.match(out[i])
        if m:
            out[i] = f'{m.group("i")}{m.group("d").rstrip()} -> {m.group("p")}, line {m.group("n")}'
            counts['converted'] += 1
        elif NOT_PORTED.search(out[i]):
            counts['left for review: "not ported", no line number'] += 1

    changed = out != lines
    if changed and apply:
        open(path, 'w', encoding='utf-8', newline='').write(nl.join(out))
    return counts, changed


def main():
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    root = args[0] if args else DEFAULT_ROOT
    apply = '--apply' in sys.argv

    total, touched = collections.Counter(), 0
    for dirpath, _, names in os.walk(root):
        for n in sorted(names):
            if n.endswith('.cs'):
                counts, changed = migrate(os.path.join(dirpath, n), apply)
                total.update(counts)
                touched += changed

    print(('applied to ' if apply else 'would touch ') + f'{touched} files')
    for k, v in total.most_common():
        print(f'  {v:5}  {k}')
    if not apply:
        print('\n(dry run -- pass --apply to write)')


if __name__ == '__main__':
    sys.exit(main())
