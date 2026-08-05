#!/usr/bin/env python3
"""
Regenerate renames/<sample>.json from the current deobfuscated assemblies, keeping the names already
chosen.

Why this exists
---------------
`ilrename template` writes a skeleton with empty values, and it is the only thing that knows how to
enumerate entities and annotate them with kind, signature and token. But it writes *empty* values, so
running it over a map that already has names chosen throws those names away — and there is no
`ilrename merge`. The result is that the maps drift: they were generated once, and entities that
appeared later (members, or types de4dot only started emitting after a fix) are simply absent, so
nobody can name them because there is no key to fill in.

So: generate a fresh skeleton, then re-inject every name already chosen, matching on the map key. The
key is `<current name>#<token>`, and the token half is the identity — a de4dot change that alters the
generated name keeps the entry working, which is the whole reason the maps are keyed that way.

This never invents a name and never deletes one silently. Entries whose key no longer resolves are
reported, not dropped quietly, because a vanished key means either a renamed entity or a deleted one
and those want different responses.

    python3 scripts/refresh_renames.py            # all samples, in place
    python3 scripts/refresh_renames.py -n         # show what would change
    python3 scripts/refresh_renames.py --all-members
"""

import argparse
import json
import re
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import pipeline as pl  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
RENAMES = ROOT / "renames"
EXPORT = ROOT / "export"

# Map keys are `<name>#<token>`; the token is the identity and the name is a drift label.
KEY_LINE = re.compile(r'^(\s*)"((?:[^"\\]|\\.)*)"\s*:\s*"((?:[^"\\]|\\.)*)"\s*(,?)\s*$')


def strip_comments(text: str) -> str:
    """The maps are JSONC -- ilrename annotates every entry -- so json can't read them directly."""
    return re.sub(r"^\s*//.*$", "", text, flags=re.M)


def chosen_names(path: Path) -> dict:
    if not path.exists():
        return {}
    data = json.loads(strip_comments(path.read_text(encoding="utf-8")))
    return {k: v for k, v in data.items() if v}


def hand_written_notes(path: Path) -> dict:
    """
    Comment lines a person added above an entry, keyed by that entry's key.

    Names are not the only thing in these maps worth keeping. Entries carry hand-added provenance --
    "documented by an earlier session; not re-verified against this export" and the like -- which says
    how much a name is trusted. Regenerating the skeleton would drop every one of them, replacing a
    qualified name with one that looks confirmed. Losing that is worse than losing the name, because a
    name reads the same either way.
    """
    if not path.exists():
        return {}
    notes, pending = {}, []
    for line in path.read_text(encoding="utf-8").splitlines():
        stripped = line.strip()
        if stripped.startswith("//"):
            pending.append(line)
            continue
        m = KEY_LINE.match(line)
        if m:
            notes[m.group(2)] = pending
        pending = []
    return notes


def token_of(key: str) -> str:
    """The `#<token>` half, which is what survives a de4dot-generated name change."""
    return key.rsplit("#", 1)[-1] if "#" in key else key


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("-n", "--dry-run", action="store_true", help="report, write nothing")
    parser.add_argument("--all-members", action="store_true",
                        help="include every member, not only generated-looking ones")
    parser.add_argument("--sample", action="append", help="sample stem (repeatable; default: all)")
    args = parser.parse_args()

    # pipeline owns where the tool lives, and its path carries the platform's executable suffix.
    # This used to probe for a pl.find_ilrename() that has never existed and fall back to a
    # hardcoded POSIX path, so on Windows it reported a perfectly good ilrename.exe as missing.
    ilrename = pl.ILRENAME
    if not ilrename.exists():
        sys.exit(f"error: ilrename not found at {ilrename}; reexport.py builds it")

    # Only the maps that exist. binaries/ also holds dependencies (a Harmony build, for one) which
    # are not ours to rename and would add thousands of meaningless entries if globbed blindly.
    samples = args.sample or sorted(p.stem for p in RENAMES.glob("*.json"))

    workdir = Path(tempfile.mkdtemp(prefix="refresh-renames-"))
    rc = 0
    try:
        for stem in samples:
            sample = ROOT / "binaries" / f"{stem}.dll"
            mapfile = RENAMES / f"{stem}.json"
            if not sample.exists():
                print(f"skip {stem}: {sample} not found")
                continue

            # Deobfuscate to a scratch copy. The map is generated against de4dot output, never the
            # original -- the tokens in it are the ones the deobfuscated assembly carries.
            deobf = pl.deobfuscate(sample, workdir / f"{stem}.dll", pl.find_de4dot(),
                                   desc=f"deobfuscating {stem}")
            if deobf is None:
                print(f"error: could not deobfuscate {stem}", file=sys.stderr)
                rc = 1
                continue

            skeleton = workdir / f"{stem}.template.json"
            cmd = [str(ilrename), "template", "--in", str(deobf), "--out", str(skeleton)]
            if args.all_members:
                cmd.append("--all-members")
            subprocess.run(cmd, check=True, capture_output=True)

            existing = chosen_names(mapfile)
            by_token = {token_of(k): v for k, v in existing.items()}
            notes = hand_written_notes(mapfile)
            notes_by_token = {token_of(k): v for k, v in notes.items()}

            # Token-only matching handles the case the map was designed for -- de4dot renames an
            # entity and the token holds it -- but not the opposite one, which turns out to be the
            # common case: a pass that adds or removes members renumbers every Method and Field RID
            # after it while every name stays put. Matching those by name too is what stops a
            # deobfuscator change from quietly wiping the member half of the map. Only names that
            # are unique on both sides qualify, so an ambiguous match is reported as lost instead of
            # being guessed at.
            def entity_name(key):
                return key.rsplit("#", 1)[0]

            existing_by_name = {}
            for k in existing:
                existing_by_name.setdefault(entity_name(k), []).append(k)

            kept = renamed_key = recovered_by_name = carried_notes = 0
            out_lines = []
            seen_tokens = set()
            claimed_by_name = set()
            fresh_block = []
            skeleton_lines = skeleton.read_text(encoding="utf-8").splitlines()

            skeleton_name_counts = {}
            for line in skeleton_lines:
                m = KEY_LINE.match(line)
                if m:
                    n = entity_name(m.group(2))
                    skeleton_name_counts[n] = skeleton_name_counts.get(n, 0) + 1

            for line in skeleton_lines:
                m = KEY_LINE.match(line)
                if not m or m.group(3):
                    if line.strip().startswith("//"):
                        fresh_block.append(line.strip())
                    else:
                        fresh_block = []
                    out_lines.append(line)
                    continue
                indent, key, _, comma = m.groups()
                # Re-emit any hand-added comment the skeleton does not already carry. Comparing
                # against the fresh block is what keeps ilrename's own annotations from doubling up.
                for note in notes.get(key) or notes_by_token.get(token_of(key)) or []:
                    if note.strip() not in fresh_block:
                        out_lines.append(note)
                        carried_notes += 1
                fresh_block = []
                token = token_of(key)
                seen_tokens.add(token)
                name = existing.get(key) or by_token.get(token)
                from_name = False
                if not name:
                    ename = entity_name(key)
                    twins = existing_by_name.get(ename) or []
                    if len(twins) == 1 and skeleton_name_counts.get(ename) == 1:
                        name = existing[twins[0]]
                        from_name = True
                if name:
                    if from_name:
                        recovered_by_name += 1
                        claimed_by_name.add(twins[0])
                    elif key not in existing:
                        renamed_key += 1   # same entity, de4dot's generated name moved
                    kept += 1
                    out_lines.append(f'{indent}"{key}": "{name}"{comma}')
                else:
                    out_lines.append(line)

            lost = sorted(k for k in existing
                          if token_of(k) not in seen_tokens and k not in claimed_by_name)
            total = sum(1 for l in out_lines if KEY_LINE.match(l))
            named = kept
            print(f"{stem}: {total} entries, {named} named "
                  f"({renamed_key} re-keyed, {recovered_by_name} recovered by name, "
                  f"{carried_notes} note(s) carried), {total - named} empty"
                  + (f", {len(lost)} NAME(S) NO LONGER RESOLVE" if lost else ""))
            for k in lost:
                print(f"    lost: {k} = {existing[k]!r}")
                rc = 1

            if args.dry_run:
                continue
            mapfile.write_text("\n".join(out_lines) + "\n", encoding="utf-8")
    finally:
        shutil.rmtree(workdir, ignore_errors=True)

    if rc:
        print("\nA key that no longer resolves is a chosen name with nowhere to go: the entity was "
              "renamed by a de4dot change, or deleted. Decide which before re-running.", file=sys.stderr)
    return rc


if __name__ == "__main__":
    raise SystemExit(main())
