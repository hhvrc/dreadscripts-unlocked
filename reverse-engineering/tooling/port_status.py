#!/usr/bin/env python3
"""
What is left to port from export/ into the restored Unity package, and what is deliberately excluded.

Why this exists
---------------
The restored package in ../public/unity/ is built by porting one file at a time out of export/ and
polishing it on the way in. That needs a record of what has landed -- but a hand-kept checklist is
exactly the kind of second copy of a fact this project keeps getting burned by: it drifts the moment
someone ports a file and forgets the list, and then nobody can trust it without re-deriving it.

So nothing is recorded by hand. The two sides are both on disk already: export/ says what exists,
and the package says what has landed. This diffs them. The only thing it holds itself is the
exclusion list below, because "we chose not to ship this" is a decision that is not inferable from
either tree.

    python3 scripts/port_status.py                 # summary per assembly
    python3 scripts/port_status.py -v              # name every file still to port
    python3 scripts/port_status.py --excluded      # show what is excluded, and why
"""

import argparse
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
EXPORT = ROOT / "export"
PACKAGE = ROOT.parent / "unity" / "Assets" / "com.dreadscripts.unlocked" / "Editor"

# Where each export subtree is ported to, relative to PACKAGE.
DESTINATIONS = {
    "ADOverhaul2022/DreadScripts/ADOverhaul": "ADOverhaul",
    "ControllerEditor/DreadScripts/ControllerEditor": "ControllerEditor",
    "ControllerEditor/DreadScripts/Common/SupportThankies": "Common/SupportThankies",
}

# Both products shipped their own copy of these types, identical but for obfuscated parameter
# names. They are ported once into Editor/Common (namespace DreadScripts.Common) under whichever
# of the two names reads better, so a stem check against the destination directory alone would
# report both copies as missing. Maps export stem -> the file it landed in under Common/.
SHARED = {
    "GUIColorScope": "GUIColorScope",
    "GUILayoutUtils": "GUILayoutUtils",
    "SplitterGUIUtils": "GUILayoutUtils",
    "ShowMixedValueScope": "MixedValueScope",
    "MixedValueScope": "MixedValueScope",
    "ReflectionAccessor": "ReflectionAccessor",
    "ObjectReflector": "ReflectionAccessor",
    "ReflectionCache": "ReflectionCache",
    "TypeReflectionData": "ReflectionCache",
    "ReflectionRestoreScope": "ReflectionRestoreScope",
    "PropertyRestoreScope": "ReflectionRestoreScope",
    "ScrollViewScope": "ScrollViewScope",
    "FloatingActionWindow": "AutoSizedPopupWindow",
    "AutoSizedPopupWindow": "AutoSizedPopupWindow",
    # ADOverhaul's copy is nested inside ADOEditorUtility as ReadableTexture rather than being a
    # file of its own, so only the ControllerEditor stem appears here.
    "ReadableTextureScope": "ReadableTextureScope",
}

# ADOverhaul2019 is the same plugin as ADOverhaul2022, built for an earlier Unity. Only one of them
# is ported; the 2019 tree stays a reference for checking a body the 2022 decompile mangled.
REFERENCE_ONLY = {"ADOverhaul2019"}

# Deliberately not ported, read from the package's own exclusion list rather than held here.
#
# This used to be two dicts in this file, which put "we chose not to ship this" in the tooling while
# the thing it describes lives in the package -- so removing a subsystem and recording why were two
# edits in two repos, and the second kept not happening. Worse, most of what gets removed is a region
# inside one of the two root classes rather than a file, so this script cannot see its absence at all
# and the list was the only trace. Now there is one list, it sits with the package it describes, and
# it is prose a human is expected to read.
EXCLUSIONS = ROOT / "vendor-backend" / "EXCLUDED.md"

# A row is `| identifier | product | removed | why |`, the identifier in backticks. Anything else in
# the file -- prose, the "not excluded" section, the format description -- is ignored, so the
# document stays readable rather than being bent into a data format.
EXCLUSION_ROW = re.compile(r"^\|\s*`(?P<id>[^`]+)`\s*\|[^|]*\|[^|]*\|\s*(?P<why>.+?)\s*\|\s*$")

# Whole export subtrees that are not ported. Matched against the export path.
EXCLUDED_TREES = {
    "Common/SupportThankies":
        "the supporter window and the transport it drove -- fetched a supporter list over the "
        "network at editor time; removed 2026-08-05, see vendor-backend/EXCLUDED.md",
}


def load_exclusions() -> dict:
    """{identifier: why} from the package's EXCLUDED.md, or {} with a warning if it is missing."""
    if not EXCLUSIONS.is_file():
        print(f"warning: no exclusion list at {EXCLUSIONS}; nothing will be reported as excluded")
        return {}
    out = {}
    for line in EXCLUSIONS.read_text(encoding="utf-8", errors="replace").splitlines():
        m = EXCLUSION_ROW.match(line)
        if m:
            # Strip the markdown links the reasons carry; they are for the reader, not for this.
            why = re.sub(r"\[([^\]]+)\]\([^)]+\)", r"\1", m.group("why"))
            out.setdefault(m.group("id"), why)
    return out


EXCLUDED = load_exclusions()

# Files that ARE ported, but only after the licence code inside them is removed. This is separate
# from EXCLUDED and easy to get wrong: ControllerEditor keeps its DRM inside its main class rather
# than in a file of its own, so excluding whole files would either ship the DRM or drop the tool.
# Whoever ports one of these has to strip, not just copy.
STRIP_DRM = {
    "ControllerEditor": "holds HWID/HMACSHA1 licence validation inline; strip it during the port",
    # Was excluded outright while it was called LicenseManager, which would have dropped the whole
    # ADOverhaul tool: the class is not the DRM file, it is the product's root class -- the editor
    # window, all four component inspectors, the ADOSettings framework, BugReporter, ProcessRunner
    # and a JSON reader, with the licence code mixed in among them. Renamed to ADOverhaul in
    # renames/ on 2026-07-31; it strips exactly like ControllerEditor does.
    "ADOverhaul": "the ADOverhaul root class, with licence validation inline; strip it during the port",
    # The export file only becomes ADOverhaul.cs at the next re-export. Keeping the old stem here
    # means the warning still fires in the meantime, rather than going quiet for exactly the window
    # in which someone might port it unwarned. Harmless to leave once the rename has landed.
    "LicenseManager": "same file under its pre-2026-07-31 name; strip the licence code, do not skip",
}


# How a partial declares that part of its class has not landed. The splitting skill requires every
# partial to carry an audit-status header and gives "Partial in progress: ... not ported yet" as the
# phrasing for a region still missing, which makes the headers the authoritative record of progress.
# Reading them beats keeping a list here that would drift the moment someone finished a split.
INCOMPLETE_MARKERS = ("partial in progress", "not ported yet", "not yet ported")

# A type declaration in decompiled C#. Nested types are the whole reason this is needed: both
# products keep most of their code inside one enormous root class, and the port hoists each nested
# type into a file of its own -- correct, and invisible to any check that compares file names.
TYPE_DECL_RE = re.compile(r"\b(?:class|struct|interface|enum|record)\s+([A-Za-z_][A-Za-z0-9_]*)")


def file_is_incomplete(path) -> bool:
    """Whether a ported file's own audit header says part of it has not landed."""
    text = path.read_text(encoding="utf-8", errors="replace").lower()
    return any(marker in text for marker in INCOMPLETE_MARKERS)


def declared_types(root) -> set:
    """Every type name declared anywhere under `root`, nested ones included."""
    names = set()
    for f in root.rglob("*.cs"):
        names.update(TYPE_DECL_RE.findall(f.read_text(encoding="utf-8", errors="replace")))
    return names


def has_provenance(path) -> bool:
    """
    Whether a ported file's header says which decompiled source it came from.

    The package's header format (see the package's own HEADER-FORMAT.md) requires
    `// Reconstructed from: <path>` as the first line, and `reverse-engineering/tools/check-headers.py` over in the
    public repo is what validates the rest of it -- that the MAP entries are well formed and that
    each decompiled member is claimed exactly once. This asks only the one question that tool does
    not: whether the file claims a source at all. Anything more would be a second implementation of
    a check that already has an owner.
    """
    with path.open(encoding="utf-8", errors="replace") as fh:
        for line in fh:
            if line.strip():
                return line.startswith("// Reconstructed from:")
    return False


def by_stem(root) -> dict:
    """stem -> the file(s) it landed in. A stem maps to several files when a class was split."""
    out = {}
    if root.is_dir():
        for p in root.rglob("*.cs"):
            out.setdefault(p.stem, []).append(p)
    return out


def is_excluded(stem: str):
    for key, why in EXCLUDED.items():
        if stem == key or stem.startswith(key):
            return why
    return None


def main() -> int:
    ap = argparse.ArgumentParser(
        description="What is left to port from export/ into the restored Unity package.")
    ap.add_argument("-v", "--verbose", action="store_true", help="name every file still to port")
    ap.add_argument("--excluded", action="store_true", help="list excluded files and why")
    args = ap.parse_args()

    if not EXPORT.is_dir():
        print(f"no export tree at {EXPORT}")
        return 1

    # Types shared by both products live here rather than under either tool's directory.
    shared_files = by_stem(PACKAGE / "Common")
    shared_landed = set(shared_files)

    # Every package file already reported on, so the closing sweep can tell an unported region that
    # has been accounted for from one that no export stem maps to at all.
    accounted = set()
    total_done = total_todo = total_skip = 0
    for src_rel, dst_rel in DESTINATIONS.items():
        src, dst = EXPORT / src_rel, PACKAGE / dst_rel
        if not src.is_dir():
            print(f"{src_rel}: missing from export/")
            continue

        tree_why = EXCLUDED_TREES.get(dst_rel)
        if tree_why:
            count = len(list(src.rglob("*.cs")))
            total_skip += count
            print(f"{dst_rel:24} {count:>3} file(s) excluded -- {tree_why}")
            continue

        landed_files = by_stem(dst)
        landed = set(landed_files)

        # A class ported under the partial-class convention never gets a <Stem>.cs. It becomes a
        # <Stem>/ folder of <Stem>.<Section>.cs files (see the splitting-large-classes-into-partials
        # skill), which a plain stem check reads as "not started" -- the opposite of the truth for
        # the four big classes, where most of the porting effort has gone. It is not completion
        # either: the outer class body lands section by section, so this is its own state.
        partial_dirs = {d.name: d for d in dst.rglob("*")
                        if d.is_dir() and any(d.glob(f"{d.name}.*.cs"))} if dst.is_dir() else {}

        done, todo, partial, skipped = [], [], [], []
        incomplete_of = {}   # stem -> the file(s) under it still declaring an unported region
        for p in sorted(src.rglob("*.cs")):
            why = is_excluded(p.stem)
            if why:
                skipped.append((p.stem, why))
            elif p.stem in landed or SHARED.get(p.stem) in shared_landed:
                # Landing a file is not the same as finishing it. A file whose own header still
                # declares an unported region is in progress, not done -- checking only for
                # existence is how MenuSelector and SearchablePickerPopup were both counted as
                # ported while their headers said otherwise.
                files = landed_files.get(p.stem) or shared_files.get(SHARED.get(p.stem), [])
                accounted.update(files)
                still = [f for f in files if file_is_incomplete(f)]
                if still:
                    incomplete_of[p.stem] = still
                    partial.append(p.stem)
                else:
                    done.append(p.stem)
            elif p.stem in partial_dirs:
                # A finished split is ported, not perpetually "in progress" -- it just never gets a
                # <Stem>.cs, so its own headers are the only thing that can say it is done.
                files = sorted(partial_dirs[p.stem].glob("*.cs"))
                accounted.update(files)
                still = [f for f in files if file_is_incomplete(f)]
                if still:
                    incomplete_of[p.stem] = still
                    partial.append(p.stem)
                else:
                    done.append(p.stem)
            else:
                todo.append(p.stem)

        total_done += len(done)
        total_todo += len(todo) + len(partial)
        total_skip += len(skipped)
        portable = len(done) + len(todo) + len(partial)
        pct = f"{100 * len(done) // portable}%" if portable else "n/a"
        in_progress = f"   {len(partial)} in progress" if partial else ""
        print(f"{dst_rel:24} {len(done):>3}/{portable:<3} ported ({pct})"
              f"   {len(skipped)} excluded{in_progress}")
        if args.verbose and partial:
            for stem in partial:
                warn = STRIP_DRM.get(stem)
                still = incomplete_of[stem]
                d = partial_dirs.get(stem)
                if d is not None:
                    n = len(list(d.glob(f"{stem}.*.cs")))
                    head = (f"SPLIT {stem} -- {n} partial(s) landed, {len(still)} still declaring "
                            f"unported regions")
                else:
                    # Not a split at all: one file, landed, still declaring an unported region.
                    head = f"PART  {stem} -- landed, but its header declares an unported region"
                print(f"      {head}" + (f"   <-- {warn}" if warn else ""))
                for f in sorted(still):
                    print(f"              {f.name}")
        if args.verbose and todo:
            for stem in todo:
                warn = STRIP_DRM.get(stem)
                print(f"      TODO  {stem}" + (f"   <-- {warn}" if warn else ""))
        if args.excluded and skipped:
            for stem, why in skipped:
                print(f"      skip  {stem:28} {why}")

        # A file in the package with no counterpart in export/ is either renamed or invented, and
        # both are worth knowing about immediately rather than at review time.
        #
        # "Counterpart" is not "file of the same name", though, and reading it that way made this
        # check useless: both products keep most of their code as types nested inside one root class
        # (ControllerEditor.cs is 18,535 lines), and the port hoists each nested type into a file of
        # its own. Against file stems alone that reported 54 EXTRAs, every one of them a correctly
        # hoisted nested type -- which is precisely the burial the note below warns about, so a real
        # invented type could not have been picked out of them. Match against every type DECLARED in
        # export/ instead. Name-level, so a nested type sharing a name with an unrelated one is not
        # distinguished; that is the intended trade for a check that fires only on real surprises.
        export_stems = {p.stem for p in src.rglob("*.cs")}
        known = export_stems | declared_types(src)
        # A <Stem>.<Section>.cs partial of a class that does exist in export/ is the convention
        # working as intended, not an invented type; reporting each one buried the real EXTRAs.
        extra = {s for s in landed - export_stems if s.split(".", 1)[0] not in known}
        # Of what is left, a deliberate rename is the common case and an invented file the rare one,
        # and only the second is a problem. A rename says so in its own header -- ControllerMerge
        # records that it lifted a static member out of the window god class, ParameterRewriter that
        # it restored a compiler-generated closure -- so provenance is what separates them. Renames
        # are summarised on one line; a file claiming no source at all is named.
        renamed = sorted(s for s in extra
                         if all(has_provenance(f) for f in landed_files[s]))
        for stem in sorted(extra - set(renamed)):
            print(f"      EXTRA {stem} -- in the package, not in export/, and its header "
                  f"names no decompiled source")
        if renamed:
            print(f"      renamed on the way in, provenance in their headers: {', '.join(renamed)}")

    portable = total_done + total_todo
    print(f"\n{total_done} of {portable} portable file(s) ported, "
          f"{total_todo} to go, {total_skip} excluded by policy")

    # A file hoisted out of a root class has no <Stem>.cs in export/ to be matched against, so the
    # loop above never looks at it and an unported region inside it goes unreported entirely. This
    # sweep is the only thing that can see those, and it found five the per-stem walk could not.
    orphan = sorted(p for p in PACKAGE.rglob("*.cs")
                    if p not in accounted and file_is_incomplete(p))
    if orphan:
        print(f"\n{len(orphan)} unported region(s) in files with no export counterpart "
              f"(hoisted out of a root class, so no stem matches them):")
        for p in orphan:
            print(f"  {p.relative_to(PACKAGE)}")
    if STRIP_DRM:
        print("\nported only after stripping the licence code inside them:")
        for stem, why in sorted(STRIP_DRM.items()):
            print(f"  {stem:28} {why}")
    if REFERENCE_ONLY:
        print(f"reference only, never ported: {', '.join(sorted(REFERENCE_ONLY))}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
