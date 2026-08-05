#!/usr/bin/env python3
"""
typecheck_package.py
--------------------
Compile the restored Unity package in ../public/unity/ against the checked-in reference assemblies
(deps/unity + deps/vrchat + binaries/0Harmony.dll) and report C# errors.

Why this exists: neither repo has a build system, so reconstructed source was only ever checked by
reading. That misses whole classes of mistake -- a method moved to the wrong class, a call site left
pointing at a member that no longer exists, an argument order that no longer matches. Those are
exactly the errors reconstruction produces, and the compiler finds all of them in seconds.

It used to compile `dreadre-devel/Assets/`, and was called `typecheck_devel.py` for that reason. That
reconstruction went stale against export/ and was retired on 2026-07-31: the tree moved to
../public/unity/, its sources were dropped, and each file is now re-ported from export/ and polished
on the way in. So this runs against the destination, and is the gate each ported file has to pass
before it counts as landed. Renamed to match on 2026-08-05, the old name having outlived its tree by
five days of it meaning nothing. An empty package typechecks trivially -- that is expected early on,
not a bug.

It is a TYPE check, not a correctness check. A file that compiles can still be semantically wrong;
keep diffing against export/ for that (see the no-unverified-reconstruction rule). Think of this as
the cheap gate you run before committing, not as evidence the reconstruction is right.

Usage
-----
  python3 scripts/typecheck_package.py                  # the whole package
  python3 scripts/typecheck_package.py --package ado    # Editor/ADOverhaul only
  python3 scripts/typecheck_package.py --package ce     # Editor/ControllerEditor only
  python3 scripts/typecheck_package.py --package common # Editor/Common only
  python3 scripts/typecheck_package.py -v               # show the full compiler output

Requires the dotnet SDK. Nothing is written into either repo; the scratch project goes to a temp dir.
"""

import argparse
import re
import shutil
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import pipeline as pl  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
PACKAGE = ROOT.parent / "unity" / "Assets" / "com.dreadscripts.unlocked"
ASSETS = PACKAGE / "Editor"

# The two vendor packages were consolidated into one, so these select a subtree rather than a
# package. There is deliberately no entry for the licence/DRM code: it is not ported.
PACKAGES = {
    "ado":    "ADOverhaul",
    "ce":     "ControllerEditor",
    "common": "Common",
}

# deps/unity ships both UnityEditor.dll and the split UnityEditor.*Module assemblies, which declare
# the same types; referencing both makes every use of e.g. SerializedProperty/MouseCursor/SceneView
# ambiguous (CS0433), so the aggregate is dropped and the modules kept. UnityEngine.dll must NOT be
# dropped the same way -- it does not conflict, and some deps type-forward through it (dropping it
# gives CS0012 on ScriptableObject).
FACADE_ASSEMBLIES = {"UnityEditor.dll"}

# Warnings that are pure noise for decompiled-then-cleaned source: unused/never-assigned fields are
# expected while reconstruction is incomplete.
SUPPRESSED_WARNINGS = "CS0169;CS0414;CS0649;CS0108;CS0067"

def collect_sources(package: str | None) -> list[Path]:
    roots = [ASSETS / PACKAGES[package]] if package else [ASSETS]
    for r in roots:
        if not r.is_dir():
            sys.exit(f"error: no such directory: {r}")
    return sorted({source for root in roots for source in root.rglob("*.cs")})


def collect_refs() -> list[Path]:
    """
    The same reference set every other script uses, resolved by pipeline.

    This used to hardcode deps/unity + deps/vrchat + binaries/0Harmony.dll itself, which made it a
    fourth independent copy of the set; the drift between those copies is what pipeline.py exists to
    prevent. FACADE_ASSEMBLIES stays here because it is specific to compiling, not to resolving.
    """
    refs = pl.refs()
    refs.require_complete("typechecking the restored package")
    return refs.dll_paths(exclude=FACADE_ASSEMBLIES)


def build_project(sources: list[Path], refs: list[Path]) -> ET.ElementTree:
    """Build the temporary project XML without hand-escaping paths."""
    root = ET.Element("Project", {"Sdk": "Microsoft.NET.Sdk"})
    properties = ET.SubElement(root, "PropertyGroup")
    for name, value in {
        "TargetFramework": "netstandard2.1",
        "LangVersion": "9.0",
        "EnableDefaultCompileItems": "false",
        "NoStdLib": "true",
        "DisableImplicitFrameworkReferences": "true",
        "AssemblyName": "devel_typecheck",
        "NoWarn": SUPPRESSED_WARNINGS,
    }.items():
        ET.SubElement(properties, name).text = value

    source_group = ET.SubElement(root, "ItemGroup")
    for source in sources:
        ET.SubElement(source_group, "Compile", {"Include": str(source)})

    reference_group = ET.SubElement(root, "ItemGroup")
    for reference in refs:
        element = ET.SubElement(reference_group, "Reference", {"Include": reference.stem})
        ET.SubElement(element, "HintPath").text = str(reference)

    ET.indent(root, space="  ")
    return ET.ElementTree(root)


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--package", choices=sorted(PACKAGES), help="limit to one package")
    ap.add_argument("-v", "--verbose", action="store_true", help="print full compiler output")
    args = ap.parse_args()

    if not shutil.which("dotnet"):
        sys.exit("error: 'dotnet' not found on PATH")

    sources, refs = collect_sources(args.package), collect_refs()
    if not sources:
        # Nothing ported into this subtree yet. That is the expected state early in the port, so it
        # is a clean result, not a failure -- exiting non-zero here would make the gate unusable
        # exactly while it is most wanted.
        print(f"nothing to typecheck: no .cs under {ASSETS.relative_to(ROOT.parent)}"
              + (f"/{PACKAGES[args.package]}" if args.package else ""))
        print("OK - 0 errors")
        return 0
    print(f"typechecking {len(sources)} source file(s) against {len(refs)} reference assemblies")

    with tempfile.TemporaryDirectory(prefix="devel_typecheck_") as temp:
        work = Path(temp)
        proj = work / "devel_typecheck.csproj"
        build_project(sources, refs).write(proj, encoding="utf-8", xml_declaration=True)
        res = subprocess.run(["dotnet", "build", str(proj), "-v", "q", "--nologo"],
                             capture_output=True, text=True)
        out = res.stdout + res.stderr
        if args.verbose:
            print(out)

        errors, seen = [], set()
        for line in out.splitlines():
            if "error CS" not in line:
                continue
            clean = re.sub(r"\s*\[/.*?\.csproj\]\s*$", "", line).strip()
            try:  # make paths repo-relative so output is readable
                clean = re.sub(r"^" + re.escape(str(ROOT)) + "/", "", clean)
            except re.error:
                pass
            if clean not in seen:
                seen.add(clean)
                errors.append(clean)

        if errors:
            print(f"\n{len(errors)} error(s):\n")
            for e in errors:
                print("  " + e)
            return 1

        # A clean "error CS" grep is not the same as a successful build. MSBuild, the SDK and NuGet
        # all fail with messages that contain no "error CS" at all (missing SDK, unresolvable
        # reference, malformed csproj), and reporting "OK - 0 errors" for those would mean the
        # typecheck silently stopped checking anything.
        if res.returncode != 0:
            print(f"\nerror: dotnet build failed (exit {res.returncode}) without reporting any "
                  f"C# error.\n       This is a build/SDK/NuGet failure, NOT a clean typecheck -- "
                  f"nothing was verified.\n", file=sys.stderr)
            print(out, file=sys.stderr)
            return 2

        print("OK - 0 errors")
        return 0


if __name__ == "__main__":
    sys.exit(main())
