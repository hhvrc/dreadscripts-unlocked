#!/usr/bin/env python3
"""Check package file headers against HEADER-FORMAT.md.

Usage:
    python tools/check-headers.py [path]        report violations
    python tools/check-headers.py --summary     counts only

Exit status is 1 if any error-level violation was found, so this can gate a hook.
"""
import os
import re
import sys
import collections

DEFAULT_ROOT = 'unity/Assets/com.dreadscripts.unlocked'

FIRST = re.compile(r'^// (?:Reconstructed from|Shared by both tools)\b')
SRC = re.compile(r'^//\s*Reconstructed from:\s*(\S+)')
AUDIT = re.compile(r'^// Audit status:\s*(VERIFIED|PARTIAL|UNAUDITED)\b')

# The five legal MAP forms.
MAP_HERE = re.compile(r'^//\s{3}(?P<d>.+?)\s*->\s*(?P<p>.+?),\s*line (?P<n>\d+)\s*$')
MAP_RANGE = re.compile(r'^//\s{3}(?P<d>.+?)\s*->\s*(?P<p>.+?),\s*lines (?P<n>\d+)-(?P<m>\d+)\s*$')
MAP_ELSEWHERE = re.compile(r'^//\s{3}(?P<d>.+?)\s*->\s*(?P<p>.+?),\s*line (?P<n>\d+),\s*in (?P<f>\S+\.cs)\s*$')
MAP_UNPORTED = re.compile(r'^//\s{3}(?P<d>.+?)\s*->\s*NOT PORTED,\s*line (?P<n>\d+)\s*--\s*(?P<why>.+)$')
# A sub-entry: a field of the member named by the preceding top-level entry. Five
# spaces of indent, and no line number of its own.
MAP_SUB = re.compile(r'^//\s{5,}(?P<d>[^->]+?)\s*->\s*(?P<p>.+?)\s*$')

ARROW = re.compile(r'^//\s.*->')
SECTION = re.compile(r'^//\s*(?:=+\s*)?(PARTIAL PORT|DELIBERATE DEVIATION|SHIPPED BUG'
                     r'|DEOBF-BUG|NOT PORTED|2019 vs 2022|NOTES|LICENCE)\b')

DECL_TMPL = (r'^\s*(?:\[[^\]]*\]\s*)?(?:internal|public|private|protected)\s'
             r'.*?\b{}\b\s*(?:<[^>()]*>)?\s*(?:\(|=|;|=>|\{{|$)')


def read_header(path):
    head, body = [], []
    with open(path, encoding='utf-8', errors='replace') as fh:
        in_head = True
        for line in fh:
            s = line.rstrip('\n').rstrip('\r')
            if in_head and (s.startswith('//') or s.strip() == ''):
                head.append(s)
            else:
                in_head = False
                body.append(s)
    while head and head[-1].strip() == '':
        head.pop()
    return head, '\n'.join(body)


# Shorthands in the ported-name column that mean "same identifier as the decompiled one".
SAME = {'same', 'unchanged'}


def declares(body, ported, decompiled):
    name = ported.strip()
    if name.lower().rstrip('.') in SAME:
        name = decompiled
    # Indexers and prose descriptors ("the styles accessor", "lifted to a top-level
    # type") name no single identifier to look for; the checker cannot verify these.
    if name.startswith('this[') or name.startswith('the ') or ' ' in name.split('(')[0].strip():
        return True
    base = re.match(r'([A-Za-z_]\w*)', name.strip())
    if not base:
        return True
    return re.search(DECL_TMPL.format(re.escape(base.group(1))), body, re.M) is not None


def check(root):
    errors = collections.defaultdict(list)
    warns = collections.defaultdict(list)
    claims = collections.defaultdict(list)
    stats = collections.Counter()

    for dirpath, _, names in os.walk(root):
        for n in sorted(names):
            if not n.endswith('.cs'):
                continue
            path = os.path.join(dirpath, n)
            head, body = read_header(path)
            stats['files'] += 1

            if not head:
                errors[path].append('no header comment')
                continue
            if not FIRST.match(head[0]):
                errors[path].append(f'first line must be "// Reconstructed from:" (got: {head[0][:50]})')

            source = None
            in_section = False
            saw_audit = False

            for i, line in enumerate(head):
                m = SRC.match(line)
                if m:
                    source = m.group(1).split('/')[-1]
                if AUDIT.match(line):
                    saw_audit = True
                    stats['audit:' + AUDIT.match(line).group(1)] += 1
                if SECTION.match(line):
                    in_section = True

                if not ARROW.match(line):
                    continue

                for rx, kind in ((MAP_ELSEWHERE, 'elsewhere'), (MAP_UNPORTED, 'unported'),
                                 (MAP_RANGE, 'range'), (MAP_HERE, 'here'), (MAP_SUB, 'sub')):
                    g = rx.match(line)
                    if g:
                        stats['map:' + kind] += 1
                        if source and kind != 'sub':
                            claims[(source, int(g.group('n')))].append((path, g.group('d')))
                        if kind in ('here', 'range') and not declares(body, g.group('p'), g.group('d')):
                            errors[path].append(
                                f'line {i + 1}: header claims "{g.group("p")}" but the file does not declare it')
                        break
                else:
                    if in_section:
                        stats['map:prose-in-section'] += 1
                    else:
                        errors[path].append(f'line {i + 1}: arrow line matches no MAP form -> {line.strip()[:70]}')

            if not saw_audit:
                warns[path].append('no "Audit status:" line')

    for key, rows in claims.items():
        if len(rows) > 1:
            files = {os.path.basename(p) for p, _ in rows}
            if len(files) > 1:
                errors[rows[0][0]].append(
                    f'{key[0]}:{key[1]} claimed by {len(files)} files: {", ".join(sorted(files))}')
    return errors, warns, stats


def main():
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    root = args[0] if args else DEFAULT_ROOT
    summary_only = '--summary' in sys.argv
    errors, warns, stats = check(root)

    n_err = sum(len(v) for v in errors.values())
    n_warn = sum(len(v) for v in warns.values())

    if not summary_only:
        for path in sorted(errors):
            print(os.path.relpath(path, root))
            for e in errors[path]:
                print(f'    ERROR  {e}')
        for path in sorted(warns):
            if path not in errors:
                print(os.path.relpath(path, root))
            for w in warns[path]:
                print(f'    warn   {w}')

    print(f'\n{stats["files"]} files: {n_err} errors, {n_warn} warnings')
    print(f'  MAP entries: ' + ', '.join(
        f'{k.split(":", 1)[1]}={v}' for k, v in sorted(stats.items()) if k.startswith('map:')))
    print(f'  Audit:       ' + ', '.join(
        f'{k.split(":", 1)[1]}={v}' for k, v in sorted(stats.items()) if k.startswith('audit:')))
    return 1 if n_err else 0


if __name__ == '__main__':
    sys.exit(main())
