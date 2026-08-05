#!/usr/bin/env python3
"""
Re-export deobfuscated source from binaries.

Steps:
  1. Delete export/
  2. Deobfuscate each DLL in binaries/ using de4dot -> temp directory
  3. Apply renames/{name}.json with ilrename, unless --no-rename
  4. Export decompiled C# source using ilspycmd -p -> export/{name}/
  5. Delete temp directory, then print an ilrename coverage report

export/ is the one exported tree, and it is NAMED by default: renames/{name}.json is applied to the
assembly *metadata* before ILSpy runs, so every use site follows the definition and ILSpy names each
output file after the renamed type. An assembly with no map, or a map with nothing assigned yet, is
exported raw — there is nothing to apply, so the two modes produce the same tree for it.

`--no-rename` turns the rename pass off and rebuilds the same export/ from the raw deobfuscated
assemblies. This exists for the case where a chosen name is itself in question: ilrename refuses
virtual/override methods and constructors and only *warns* when a renamed name also appears in a
string literal, so when a name may be a reflection target, the raw tree is the ground truth to check
it against. It is a temporary state, not a second baseline — export/ is tracked, so leaving it raw
and committing silently changes what every later diff means. The tree records which mode produced it
in export/.renames-applied (present with a per-assembly summary when named, absent when raw), and
the script prints the mode at the start and end of every run.

This used to be two tracked trees, export/ (raw) and export-named/. That cost twice the tracked
decompiled source, and left every reader and every consumer having to know which of two nearly
identical trees was the one they wanted -- the published snapshot in ../public was taken from the
wrong one for months. One tree with a flag is the same capability without the standing ambiguity.

renames/{name}.json keys each entry by metadata token, which is what keeps it valid across de4dot
fork upgrades that change de4dot's *generated* names. That works because de4dot already preserves
RIDs from binaries/{name}.dll: measured on ADOverhaul2022, every TypeDef, Method and Field token in
the deobfuscated output is also present in the original binary. Do not add --preserve-table all to
force the issue — when it actually takes effect (it is silently ignored if placed after the
filename) the resulting module cannot be rewritten by dnlib, which is what ilrename uses.

Nothing depends on that preservation silently holding: every map key carries the entity's name
alongside its token, and `ilrename report` flags any key whose token now points somewhere else.
"""

import argparse
import re
import shutil
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import pipeline as pl  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
BINARIES = ROOT / "binaries"
EXPORT = ROOT / "export"
RENAMES = ROOT / "renames"

# Records which mode produced the tracked tree. export/ is regenerated wholesale, so without this
# there is nothing in the tree itself distinguishing a named export from a raw one.
MODE_MARKER = EXPORT / ".renames-applied"

# Deliberately NOT pipeline.SKIP_BINARIES. That set is the obfuscated *experiment* corpus; this one
# is "everything export/ should contain". They coincide today, but a binary kept out of the
# experiment corpus is not automatically one that should disappear from export/.
SKIP = {"0Harmony.dll"}


# de4dot comes from work/de4dot unless --de4dot says otherwise.
#
# This used to prefer a hardcoded machine-local path (E:\Development\...\de4dot.exe) over
# work/de4dot whenever it happened to exist. On that one machine export/ was therefore built by an
# unspecified de4dot while every convention here -- "republish work/de4dot, then re-export", and the
# staleness check below -- assumed work/de4dot was the binary in use. Same failure as the
# machine-local Unity: reproducible on nobody else's machine, and silent. An override is a flag now.
def rename_map_for(name: str) -> Path | None:
    """The rename map for an assembly, if it exists and actually assigns at least one name."""
    path = RENAMES / f"{name}.json"
    if not path.exists():
        return None
    # A template full of empty values is valid but would rename nothing, so skip the ilrename pass
    # entirely rather than paying for a no-op metadata rewrite.
    text = path.read_text(encoding="utf-8")
    stripped = "\n".join(l for l in text.splitlines() if not l.lstrip().startswith("//"))
    if re.search(r':\s*"[^"]+"', stripped):
        return path
    return None


def main() -> None:
    parser = argparse.ArgumentParser(description="Re-export deobfuscated source from binaries.")
    parser.add_argument("--il", action="store_true", help="Export IL code instead of C#")
    parser.add_argument(
        "--wait-lock",
        metavar="SECONDS",
        type=float,
        default=0.0,
        help="Queue behind another reexport for up to SECONDS instead of refusing immediately. "
             "Off by default: blocking invisibly behind another run is nearly as confusing as "
             "racing it.",
    )
    parser.add_argument(
        "--de4dot",
        metavar="PATH",
        default=None,
        help="de4dot binary to use instead of work/de4dot (explicit override; export/ is defined "
             "as the output of the sibling fork published into work/, so prefer republishing it)",
    )
    parser.add_argument(
        "--no-rename",
        action="store_true",
        help="Export the raw deobfuscated assemblies without applying renames/. Use to check a "
             "name that may be a reflection target against ground truth; do not commit the result, "
             "since export/ is tracked and named by default.",
    )
    parser.add_argument(
        "--unity-managed",
        metavar="DIR",
        action="append",
        dest="unity_managed",
        default=None,
        help=(
            "Extra Unity Managed directory to add to the reference set. May be repeated. "
            "The default set is the checked-in deps/unity + deps/vrchat; a machine-local Unity "
            "is only used as a fallback if deps/unity is missing, and warns loudly."
        ),
    )
    parser.add_argument(
        "--vrc-packages",
        metavar="DIR",
        default=None,
        help=(
            "Unused: VRC SDK references come from the checked-in deps/vrchat. Kept so existing "
            "invocations do not break; pass --ref DIR to add an arbitrary reference directory."
        ),
    )
    parser.add_argument(
        "-r", "--ref",
        metavar="DIR",
        action="append",
        dest="extra_refs",
        default=None,
        help="Additional assembly reference directory. May be specified multiple times.",
    )
    args = parser.parse_args()

    # Reference assemblies come from the shared resolver, which prefers the checked-in deps/ tree.
    # This script used to auto-detect a machine-local Unity Hub install instead, which meant the
    # canonical export/ was generated against whatever Unity happened to be installed -- and
    # against nothing at all on a machine without it. deps/ is checked in precisely so that
    # export/ is reproducible; see scripts/pipeline.py.
    refs = pl.configure_refs(list(args.unity_managed or []) + list(args.extra_refs or []))
    print(f"References: {refs.describe()}")
    for d in refs.dirs:
        print(f"  {d}")

    de4dot = pl.find_de4dot(args.de4dot or pl.DE4DOT_DEFAULT)
    pl.warn_if_de4dot_stale(de4dot)

    dlls = sorted(p for p in BINARIES.glob("*.dll") if p.name not in SKIP)
    if not dlls:
        print("error: no DLLs found in binaries/", file=sys.stderr)
        sys.exit(1)

    print(f"Found {len(dlls)} DLL(s): {', '.join(d.name for d in dlls)}")

    ilrename = None if args.no_rename else pl.ensure_ilrename()
    if ilrename is None and not args.no_rename:
        # Falling back to a raw export here would write an unnamed tree into the tracked, named-by-
        # default export/ with only a console note to say so -- and the note scrolls away while the
        # tree stays. Refuse instead, and make --no-rename the only way to get a raw tree.
        print("error: ilrename is unavailable, so renames/ cannot be applied.\n"
              "  export/ is named by default; building it raw would silently change what the\n"
              "  tracked tree means. Fix the ilrename build, or pass --no-rename deliberately.",
              file=sys.stderr)
        sys.exit(1)

    print(f"Mode: {'NAMED (renames/ applied)' if ilrename else 'RAW (--no-rename)'}")

    # 1. Delete the export tree.
    #
    # Everything from here to the end of the rebuild runs under an exclusive lock. This step deletes
    # a TRACKED tree before rebuilding it, so a second run overlapping this one does not collide
    # loudly -- it interleaves its delete with this one's writes and leaves a tree that is quietly
    # missing most of its files, with a diff that reads like a huge deobfuscator regression.
    with pl.exclusive_tree_lock(wait_seconds=args.wait_lock, purpose="regenerating export/"):
        _rebuild(args, de4dot, ilrename, dlls)


def _rebuild(args, de4dot, ilrename, dlls) -> None:
    if EXPORT.exists():
        print("Deleting export/ ...")
        shutil.rmtree(EXPORT)
    EXPORT.mkdir()

    # 2-4. Deobfuscate, rename, export each DLL via a temp directory
    tmpdir = ROOT / "work" / "_deobf_tmp"
    if tmpdir.exists():
        shutil.rmtree(tmpdir)
    tmpdir.mkdir(parents=True)

    mapped: list[tuple[str, Path, Path]] = []

    try:
        for dll in dlls:
            name = dll.stem
            deobf_dll = tmpdir / dll.name

            print(f"\n[{name}]")
            if pl.deobfuscate(dll, deobf_dll, de4dot,
                              desc=f"de4dot -> {deobf_dll.relative_to(ROOT)}") is None:
                sys.exit(1)
            # The assembly ILSpy actually sees: renamed when a usable map exists, otherwise the raw
            # deobfuscated one. An assembly with no assigned names exports identically either way,
            # so this is a skipped step, not a degraded export.
            source_dll = deobf_dll
            rename_map = rename_map_for(name) if ilrename else None
            if rename_map:
                # Same filename as the original, in a subdirectory. ILSpy names the .csproj it
                # emits after the input file, so a "{name}.named.dll" here would put a
                # "{name}.named.csproj" in the tracked tree -- and only for the assemblies that
                # happen to have a map, leaving the export inconsistent with itself.
                named_dir = tmpdir / "named"
                named_dir.mkdir(exist_ok=True)
                source_dll = named_dir / dll.name
                pl.ilrename_apply(ilrename, deobf_dll, source_dll, rename_map,
                                  desc=f"ilrename {rename_map.relative_to(ROOT)} "
                                       f"-> named/{source_dll.name}")
                mapped.append((name, deobf_dll, rename_map))
            elif ilrename:
                why = ("has no names assigned yet" if (RENAMES / f"{name}.json").exists()
                       else "does not exist — run `ilrename template` to create it")
                print(f"  renames/{name}.json {why}; exporting raw")

            if pl.decompile(source_dll, EXPORT / name, extra_args=["--nested-directories"],
                            il=args.il, flatten_project=True,
                            desc=f"ilspycmd {'(IL) ' if args.il else ''}-> export/{name}/") is None:
                sys.exit(1)

        # 5. Coverage report, while the deobfuscated modules are still around.
        for name, deobf_dll, rename_map in mapped:
            print(f"\n[{name}] naming coverage")
            pl.ilrename_report(ilrename, deobf_dll, rename_map)
    finally:
        # 6. Clean up temp directory
        print("\nCleaning up temp files ...")
        shutil.rmtree(tmpdir, ignore_errors=True)

    if ilrename:
        applied = "\n".join(f"{name}  <- renames/{rename_map.name}"
                            for name, _, rename_map in mapped) or "(no map had names assigned)"
        MODE_MARKER.write_text(
            "renames/ was applied to the assembly metadata before decompiling.\n"
            "Regenerate with scripts/reexport.py; --no-rename produces the raw tree instead.\n\n"
            f"{applied}\n",
            encoding="utf-8")

    print(f"\nDone — export/ is {'NAMED' if ilrename else 'RAW'}.")
    if not ilrename:
        print("  export/ is tracked and named by default. This raw tree is for checking a name\n"
              "  against ground truth; re-run without --no-rename before committing.")


if __name__ == "__main__":
    main()
