#!/usr/bin/env python3
"""Measure how much of export/ the port actually covers, in lines of code.

check-headers.py answers "is every claim well-formed and unique". It does not answer "how much of
the decompiled source is still unaccounted for" -- the count of MAP entries says nothing, because
one entry can claim a two-line field or a three-hundred-line method.

This walks the same headers, resolves each claimed member to its extent in the export file by
brace-matching from the declaration line, and reports the union as a fraction of the export's code
lines. Three buckets, because they mean different things:

    ported      a member some file claims and declares
    excluded    a member some file claims as NOT PORTED, with a reason
    unaccounted neither -- nobody has looked at it, or looked and said nothing

Unaccounted is the number that matters. It is the only one that can hide a whole subsystem, which
is how the inspector install path stayed missing: every individual header was honest, and no
measurement asked what was left over.

Usage:
    python reverse-engineering/tools/port-coverage.py                 summary per export file
    python reverse-engineering/tools/port-coverage.py --gaps          + the largest unaccounted runs
    python reverse-engineering/tools/port-coverage.py --gaps -n 25    how many runs to list
    python reverse-engineering/tools/port-coverage.py --file ADOverhaul.cs   restrict to one export file
    python reverse-engineering/tools/port-coverage.py --json          machine-readable

Exit status is 0 unless --min-coverage is given and some file falls under it, so this can gate a
hook once the numbers are where you want them.

CAVEATS, so the number is read correctly:

  - A claimed line number that no longer points at a declaration is counted as `stale` and covers
    nothing. export/ was renumbered in 561e9ec and many headers still carry pre-561e9ec numbers, so
    expect a nonzero stale count; it is a measure of that debt, not of missing work.
  - Brace matching is textual. It skips string, char and comment content, but it is not a parser.
  - `, in <file>.cs` entries are pointers, not claims, and are ignored -- the owning file's own
    entry is what counts.
  - A ported file naming several `Reconstructed from:` sources attributes its claims to the first.
    Those files are listed under `ambiguous` so the attribution can be checked by hand.
"""
import argparse
import collections
import json
import os
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent.parent
PKG = ROOT / 'unity' / 'Assets'  # every com.dreadscripts.unlocked* package

# Export builds that are a reference, not a porting target: ADOverhaul2019 is the same plugin as
# ADOverhaul2022 built for an earlier Unity (port_status.py treats it the same way). A file may still
# claim lines in it, when it carries a third copy of something both products already account for, but
# the 2019 build's unclaimed remainder is not missing work, so it is left out of the measurement.
REFERENCE_ONLY = ('reverse-engineering/export/ADOverhaul2019/',)

SRC = re.compile(r'^//\s*Reconstructed from:\s*(\S+)')
SRC_EXTRA = re.compile(r'^//\s{3,}(\S+\.cs)\s*$')
MAP_HERE = re.compile(r'^//\s{3}(?P<d>.+?)\s*->\s*(?P<p>.+?),\s*line (?P<n>\d+)\s*$')
MAP_RANGE = re.compile(r'^//\s{3}(?P<d>.+?)\s*->\s*(?P<p>.+?),\s*lines (?P<n>\d+)-(?P<m>\d+)\s*$')
MAP_ELSEWHERE = re.compile(r'^//\s{3}(?P<d>.+?)\s*->\s*.+?,\s*line (?P<n>\d+),\s*in \S+\.cs\s*$')
MAP_UNPORTED = re.compile(r'^//\s{3}(?P<d>.+?)\s*->\s*NOT PORTED,\s*line (?P<n>\d+)\s*--\s*.+$')

# A line that plausibly opens a member or type declaration. Deliberately loose: the export is
# decompiler output, so it is regular, but attributes and modifiers vary.
DECL = re.compile(
    r'^\s*(\[.*\]\s*)?((public|private|protected|internal|static|readonly|sealed|abstract|override'
    r'|virtual|const|extern|unsafe|partial|async|new|volatile|event|implicit|explicit)\s+)*'
    r'[\w<>\[\],.?]+[\s\w@<>\[\],.?()]*\s*[({=;]'
    r'|^\s*(\[.*\]\s*)?((public|private|protected|internal|static|sealed|abstract|partial|new|unsafe'
    r'|readonly|ref)\s+)*(class|struct|interface|enum|delegate|record)\s')
ATTRIBUTE_ONLY = re.compile(r'^\s*\[.*\]\s*$')


def read_header(path):
    head = []
    with open(path, encoding='utf-8', errors='replace') as fh:
        for line in fh:
            s = line.rstrip('\n').rstrip('\r')
            if s.startswith('//') or s.strip() == '':
                head.append(s)
            else:
                break
    return head


def parse_claims(path):
    """-> (sources, [(line, kind)]) where kind is 'ported' or 'excluded'."""
    head = read_header(path)
    sources, claims, in_src_block = [], [], False

    for line in head:
        m = SRC.match(line)
        if m:
            sources.append(m.group(1))
            in_src_block = True
            continue
        if in_src_block:
            extra = SRC_EXTRA.match(line)
            if extra:
                sources.append(extra.group(1))
                continue
            if line.strip() not in ('//', ''):
                in_src_block = False

        if MAP_ELSEWHERE.match(line):
            continue
        m = MAP_UNPORTED.match(line)
        if m:
            claims.append((int(m.group('n')), 'excluded'))
            continue
        m = MAP_RANGE.match(line)
        if m:
            claims.append(((int(m.group('n')), int(m.group('m'))), 'ported'))
            continue
        m = MAP_HERE.match(line)
        if m:
            claims.append((int(m.group('n')), 'ported'))

    return sources, claims


def strip_noncode(text):
    """Blank out string/char literals and comments so brace matching is not fooled by them."""
    out, i, n = [], 0, len(text)
    while i < n:
        c = text[i]
        if c == '/' and i + 1 < n and text[i + 1] == '/':
            out.append(' ' * (n - i))
            break
        if c == '/' and i + 1 < n and text[i + 1] == '*':
            j = text.find('*/', i + 2)
            j = n if j < 0 else j + 2
            out.append(' ' * (j - i))
            i = j
            continue
        if c in '"\'':
            verbatim = i > 0 and text[i - 1] == '@'
            j = i + 1
            while j < n:
                if not verbatim and text[j] == '\\':
                    j += 2
                    continue
                if text[j] == c:
                    if verbatim and j + 1 < n and text[j + 1] == c:
                        j += 2
                        continue
                    j += 1
                    break
                j += 1
            out.append(' ' * (j - i))
            i = j
            continue
        out.append(c)
        i += 1
    return ''.join(out)


def member_extent(lines, start, limit=4000):
    """1-based inclusive (first, last) for the member declared at `start`, or None if it does not
    look like a declaration."""
    if start < 1 or start > len(lines):
        return None

    idx = start - 1
    # A claim may point at an attribute that sits on its own line above the declaration.
    while idx < len(lines) - 1 and ATTRIBUTE_ONLY.match(lines[idx]):
        idx += 1
    if not DECL.match(lines[idx]):
        return None

    depth, seen_brace = 0, False
    for i in range(idx, min(len(lines), idx + limit)):
        code = strip_noncode(lines[i])
        for c in code:
            if c == '{':
                depth += 1
                seen_brace = True
            elif c == '}':
                depth -= 1
                if seen_brace and depth <= 0:
                    return (start, i + 1)
        if not seen_brace and code.rstrip().endswith(';'):
            return (start, i + 1)  # field, or expression-bodied member
    return (start, min(len(lines), idx + limit))


def module_of(source):
    """The DreadScripts module an export file belongs to: the folder under DreadScripts/, labelled
    with its assembly when the two differ (ADOverhaul is decompiled from ADOverhaul2022)."""
    parts = Path(source).parts
    assembly = parts[2] if len(parts) > 2 else '?'
    if 'DreadScripts' in parts:
        i = parts.index('DreadScripts')
        module = '/'.join(parts[i + 1:-1]) or assembly
    else:
        module = assembly
    return module if module == assembly else f'{module} ({assembly})'


def is_code(line):
    s = line.strip()
    return bool(s) and not s.startswith('//') and not s.startswith('*') and not s.startswith('/*')


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--gaps', action='store_true', help='list the largest unaccounted runs')
    ap.add_argument('-n', type=int, default=10, help='how many runs to list per file')
    ap.add_argument('--file', help='restrict to export files whose path contains this')
    ap.add_argument('--json', action='store_true')
    ap.add_argument('--min-coverage', type=float, help='exit 1 if any file is under this %%')
    args = ap.parse_args()

    by_source = collections.defaultdict(list)
    ambiguous, unresolved = [], set()

    for dirpath, _, names in os.walk(PKG):
        for name in sorted(names):
            if not name.endswith('.cs'):
                continue
            path = Path(dirpath) / name
            sources, claims = parse_claims(path)
            if not sources or not claims:
                continue
            if len(sources) > 1:
                ambiguous.append((str(path.relative_to(PKG)), len(sources)))
            by_source[sources[0]].extend(claims)

    report = {}
    for source, claims in sorted(by_source.items()):
        if source.startswith(REFERENCE_ONLY):
            continue
        export_path = ROOT / source
        if not export_path.is_file():
            unresolved.add(source)
            continue
        if args.file and args.file not in source:
            continue

        lines = export_path.read_text(encoding='utf-8', errors='replace').splitlines()
        total = len(lines)
        code_lines = {i for i in range(1, total + 1) if is_code(lines[i - 1])}

        covered, excluded, stale = set(), set(), 0
        for claim, kind in claims:
            if isinstance(claim, tuple):
                span = (claim[0], claim[1])
            else:
                span = member_extent(lines, claim)
                if span is None:
                    stale += 1
                    continue
            target = covered if kind == 'ported' else excluded
            target.update(range(span[0], span[1] + 1))

        covered &= code_lines
        excluded = (excluded & code_lines) - covered
        unaccounted = code_lines - covered - excluded

        gaps = []
        if unaccounted:
            run_start = prev = None
            for ln in sorted(unaccounted):
                if prev is None or ln != prev + 1:
                    if run_start is not None:
                        gaps.append((run_start, prev, prev - run_start + 1))
                    run_start = ln
                prev = ln
            gaps.append((run_start, prev, prev - run_start + 1))
            gaps.sort(key=lambda g: -g[2])

        report[source] = {
            'module': module_of(source),
            'codeLines': len(code_lines),
            'ported': len(covered),
            'excluded': len(excluded),
            'unaccounted': len(unaccounted),
            'staleClaims': stale,
            'coveragePct': round(100.0 * len(covered) / len(code_lines), 1) if code_lines else 0.0,
            'accountedPct': round(100.0 * (len(covered) + len(excluded)) / len(code_lines), 1) if code_lines else 0.0,
            'largestGaps': [{'from': a, 'to': b, 'lines': n} for a, b, n in gaps[:args.n]],
        }

    keys = ('codeLines', 'ported', 'excluded', 'unaccounted', 'staleClaims')
    modules = collections.defaultdict(collections.Counter)
    for r in report.values():
        for k in keys:
            modules[r['module']][k] += r[k]

    def pcts(t):
        code = t['codeLines']
        return ((round(100.0 * t['ported'] / code, 1) if code else 0.0),
                (round(100.0 * (t['ported'] + t['excluded']) / code, 1) if code else 0.0))

    if args.json:
        mod_out = {}
        for m, t in sorted(modules.items()):
            port_pct, acct_pct = pcts(t)
            mod_out[m] = dict(t, coveragePct=port_pct, accountedPct=acct_pct)
        print(json.dumps({'modules': mod_out, 'files': report, 'ambiguous': ambiguous,
                          'unresolvedSources': sorted(unresolved)}, indent=2))
    else:
        name_w = max([len(Path(s).name) + 2 for s in report] + [len(m) for m in modules] + [20])
        rule = '-' * (name_w + 50)

        def row(label, t):
            port_pct, acct_pct = pcts(t)
            return (f'{label.ljust(name_w)}  {t["codeLines"]:>6} {t["ported"]:>7} {t["excluded"]:>6} '
                    f'{t["unaccounted"]:>7} {port_pct:>5.1f}% {acct_pct:>5.1f}% {t["staleClaims"]:>6}')

        print(f'{"module / export file".ljust(name_w)}  {"code":>6} {"ported":>7} {"excl":>6} '
              f'{"unacct":>7} {"port%":>6} {"acct%":>6} {"stale":>6}')
        tot = collections.Counter()
        for m, t in sorted(modules.items(), key=lambda kv: -kv[1]['unaccounted']):
            print(rule)
            print(row(m, t))
            files = [(s, r) for s, r in report.items() if r['module'] == m]
            for source, r in sorted(files, key=lambda kv: -kv[1]['unaccounted']):
                print(row('  ' + Path(source).name, r))
            tot.update(t)
        print(rule)
        print(row('TOTAL', tot))

        if args.gaps:
            for source, r in sorted(report.items(),
                                    key=lambda kv: (-modules[kv[1]['module']]['unaccounted'],
                                                    -kv[1]['unaccounted'])):
                if not r['largestGaps']:
                    continue
                print(f'\n[{r["module"]}] {Path(source).name} -- largest unaccounted runs')
                for g in r['largestGaps']:
                    print(f'    lines {g["from"]:>6}-{g["to"]:<6} {g["lines"]:>5} lines')

        if ambiguous:
            print(f'\n{len(ambiguous)} ported file(s) name several sources; claims attributed to the first:')
            for rel, count in ambiguous[:args.n]:
                print(f'    {rel}  ({count} sources)')
        if unresolved:
            print(f'\n{len(unresolved)} header source path(s) did not resolve:')
            for s in sorted(unresolved):
                print(f'    {s}')

    if args.min_coverage is not None:
        under = [s for s, r in report.items() if r['coveragePct'] < args.min_coverage]
        if under:
            print(f'\n{len(under)} file(s) under {args.min_coverage}% coverage', file=sys.stderr)
            return 1
    return 0


if __name__ == '__main__':
    sys.exit(main())
