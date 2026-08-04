#!/usr/bin/env python3
"""Check whether "Audit status: VERIFIED" files have changed since they were stamped.

VERIFIED asserts the file was diffed against decompiled/. That assertion decays the
moment the file is edited, so this finds the commit that introduced the stamp and
compares that version against the working tree.

Header-only changes are reported separately from code changes: a reworded comment
does not invalidate an audit, an edited method body does.

With --requeue, files whose *code* has changed lose the VERIFIED assertion: the
line is rewritten to UNAUDITED, naming the commit that had verified it and what
changed, so the file goes back into the audit queue instead of carrying a claim
that is no longer true.

Usage: python tools/check-audit-freshness.py [path] [--stale-only] [--requeue]
"""
import os
import re
import sys
import subprocess
import collections

DEFAULT_ROOT = 'unity/Assets/com.dreadscripts.unlocked'
STAMP = 'Audit status: VERIFIED'
LF = chr(10)
CRLF = chr(13) + chr(10)

AUDIT_LINE = re.compile(r'^// Audit status:\s*VERIFIED.*$', re.M)


def git(*args):
    return subprocess.run(['git'] + list(args), capture_output=True,
                          text=True, errors='replace').stdout


def stamp_commit(rel):
    """Newest commit whose version of the file carries the stamp.

    `git log -S` alone is not enough: it reports any commit that changed the
    number of occurrences, including a removal. Confirm the stamp is present.
    """
    for c in git('log', '--format=%H', '-S', STAMP, '--', rel).split():
        if STAMP in git('show', f'{c}:{rel}'):
            return c
    return None


def code_lines(text):
    """The file's non-comment, non-blank lines -- what an audit actually covers."""
    return [s for s in (l.strip() for l in text.splitlines())
            if s and not s.startswith('//')]


def requeue(path, stamped, delta):
    """Drop the VERIFIED assertion, recording what it used to claim."""
    src = open(path, encoding='utf-8', newline='').read()
    nl = CRLF if CRLF in src else LF
    short = git('log', '-1', '--format=%h', stamped).strip()
    replacement = (
        f'// Audit status: UNAUDITED -- was VERIFIED in {short}, but the code has changed'
        f'{nl}// since ({delta}); needs re-checking against decompiled/ before the claim is restored.'
    )
    out = AUDIT_LINE.sub(lambda _: replacement, src, count=1)
    if out == src:
        return False
    open(path, 'w', encoding='utf-8', newline='').write(out)
    return True


def main():
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    root = args[0] if args else DEFAULT_ROOT
    stale_only = '--stale-only' in sys.argv
    do_requeue = '--requeue' in sys.argv

    rows, stats = [], collections.Counter()
    for dirpath, _, names in os.walk(root):
        for n in sorted(names):
            if not n.endswith('.cs'):
                continue
            path = os.path.join(dirpath, n)
            rel = path.replace(os.sep, '/')
            with open(path, encoding='utf-8', errors='replace') as fh:
                if STAMP not in fh.read():
                    continue
            stats['verified'] += 1

            stamped = stamp_commit(rel)
            if not stamped:
                rows.append((rel, None, None, None))
                stats['no stamp commit'] += 1
                continue

            # Compare content directly rather than walking history: after a merge,
            # "commits since" is reachability, and includes commits that predate the
            # stamp on the other branch.
            then = git('show', f'{stamped}:{rel}')
            with open(path, encoding='utf-8', errors='replace') as fh:
                now = fh.read()

            if then == now:
                stats['fresh'] += 1
                if not stale_only:
                    rows.append((rel, stamped, 'fresh', None))
                continue

            before, after = code_lines(then), code_lines(now)
            if before == after:
                stats['stale (comments only)'] += 1
                rows.append((rel, stamped, 'comments', None))
                continue

            delta = f'{len(after) - len(before):+d} code lines'
            stats['stale (code changed)'] += 1
            if do_requeue and requeue(path, stamped, delta):
                stats['requeued'] += 1
            rows.append((rel, stamped, 'code', delta))

    for rel, stamped, state, delta in rows:
        name = os.path.relpath(rel, root)
        if stamped is None:
            print(f'{name}\n    ?      stamp text not found in history')
            continue
        subject = git('log', '-1', '--format=%h %s', stamped).strip()[:80]
        if state == 'fresh':
            print(f'{name}\n    ok     unchanged since {subject}')
        elif state == 'comments':
            print(f'{name}\n    note   comments edited since {subject}; code identical')
        else:
            print(f'{name}\n    STALE  code changed since {subject} ({delta})')

    print()
    for k in ('verified', 'fresh', 'stale (comments only)', 'stale (code changed)',
              'requeued', 'no stamp commit'):
        if stats[k]:
            print(f'  {stats[k]:4}  {k}')
    return 1 if (stats['stale (code changed)'] and not do_requeue) else 0


if __name__ == '__main__':
    sys.exit(main())
