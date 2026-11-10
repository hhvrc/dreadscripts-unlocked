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

One assembly per package. The restoration ships as separate VPM packages (Common, ControllerEditor,
ADOverhaul), each with its own .asmdef, so each is compiled here as its own assembly too, under its
asmdef name and against only the assemblies its asmdef references. Compiling them as one blob would
let ADOverhaul call into ControllerEditor and still pass, which is exactly the mistake a split into
independently installable packages cannot afford: in Unity that is a missing-type error in the
user's project, for a package they never asked to install.

Usage
-----
  python3 scripts/typecheck_package.py                  # every package
  python3 scripts/typecheck_package.py --package ado    # ADOverhaul and what it depends on
  python3 scripts/typecheck_package.py --package ce     # ControllerEditor and what it depends on
  python3 scripts/typecheck_package.py --package common # Common only
  python3 scripts/typecheck_package.py -v               # show the full compiler output

Requires the dotnet SDK. Nothing is written into either repo; the scratch project goes to a temp dir.
"""

import argparse
import json
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
ASSETS = ROOT.parent / "unity" / "Assets"

# --package shorthand -> asmdef name.
PACKAGES = {
    "ado":    "DreadScripts.ADOverhaul",
    "ce":     "DreadScripts.ControllerEditor",
    "common": "DreadScripts.Common",
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


def find_assemblies() -> dict:
    """{asmdef name: (asmdef dir, [referenced asmdef names])} for every restored package."""
    found = {}
    for asmdef in sorted(ASSETS.glob("com.dreadscripts.unlocked*/**/*.asmdef")):
        data = json.loads(asmdef.read_text(encoding="utf-8"))
        name = data["name"]
        if name in found:
            sys.exit(f"error: two asmdefs named {name}: {found[name][0]} and {asmdef.parent}")
        # Only references to other restored packages matter here; everything else the code uses is
        # a precompiled, auto-referenced DLL and is already in the reference set.
        found[name] = (asmdef.parent, list(data.get("references", [])))
    return found


def build_order(assemblies: dict, target: str | None) -> list[str]:
    """Dependencies first. With a target, only it and what it (transitively) references."""
    order, state = [], {}

    def visit(name, chain):
        if state.get(name) == "done":
            return
        if state.get(name) == "visiting":
            sys.exit(f"error: asmdef reference cycle: {' -> '.join(chain + [name])}")
        if name not in assemblies:
            sys.exit(f"error: {chain[-1]} references {name}, which no restored package defines")
        state[name] = "visiting"
        for dep in assemblies[name][1]:
            if dep.startswith("DreadScripts."):
                visit(dep, chain + [name])
        state[name] = "done"
        order.append(name)

    for name in ([target] if target else sorted(assemblies)):
        visit(name, [])
    return order


def collect_sources(asm_dir: Path) -> list[Path]:
    """The .cs files an asmdef owns: everything under its directory, minus nested asmdefs' trees."""
    nested = [p.parent for p in asm_dir.rglob("*.asmdef") if p.parent != asm_dir]
    return sorted(src for src in asm_dir.rglob("*.cs")
                  if not any(n in src.parents for n in nested))


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


def build_project(name: str, sources: list[Path], refs: list[Path]) -> ET.ElementTree:
    """Build the temporary project XML without hand-escaping paths."""
    root = ET.Element("Project", {"Sdk": "Microsoft.NET.Sdk"})
    properties = ET.SubElement(root, "PropertyGroup")
    for name, value in {
        "TargetFramework": "netstandard2.1",
        "LangVersion": "9.0",
        "EnableDefaultCompileItems": "false",
        "NoStdLib": "true",
        "DisableImplicitFrameworkReferences": "true",
        # The asmdef name, not a fixed one: Common grants InternalsVisibleTo by assembly name, so
        # a wrong name here would turn every internal access from the tools into a false error.
        "AssemblyName": name,
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

    assemblies = find_assemblies()
    if not assemblies:
        sys.exit(f"error: no .asmdef under {ASSETS}/com.dreadscripts.unlocked*")
    order = build_order(assemblies, PACKAGES[args.package] if args.package else None)
    refs = collect_refs()

    errors, seen, built = [], set(), {}
    with tempfile.TemporaryDirectory(prefix="devel_typecheck_") as temp:
        work = Path(temp)
        for name in order:
            asm_dir, asm_refs = assemblies[name]
            sources = collect_sources(asm_dir)
            own = [built[d] for d in asm_refs if d in built]
            print(f"{name}: {len(sources)} source file(s), "
                  f"{len(refs)} reference assemblies + {len(own)} restored")
            if not sources:
                continue

            proj_dir = work / name
            proj_dir.mkdir()
            proj = proj_dir / f"{name}.csproj"
            out_dir = proj_dir / "out"
            build_project(name, sources, refs + own).write(proj, encoding="utf-8", xml_declaration=True)
            res = subprocess.run(["dotnet", "build", str(proj), "-v", "q", "--nologo", "-o", str(out_dir)],
                                 capture_output=True, text=True)
            out = res.stdout + res.stderr
            if args.verbose:
                print(out)

            mine = []
            for line in out.splitlines():
                if "error CS" not in line:
                    continue
                clean = re.sub(r"\s*\[[^\]]*\.csproj\]\s*$", "", line).strip()
                clean = clean.replace(str(ASSETS) + "\\", "").replace(str(ASSETS) + "/", "")
                if clean not in seen:
                    seen.add(clean)
                    mine.append(clean)
            if mine:
                errors.extend(mine)
                # Anything depending on this assembly would only report cascading errors.
                print(f"  {len(mine)} error(s); not building what depends on it")
                break

            # A clean "error CS" grep is not the same as a successful build. MSBuild, the SDK and
            # NuGet all fail with messages that contain no "error CS" at all (missing SDK,
            # unresolvable reference, malformed csproj), and reporting "OK - 0 errors" for those
            # would mean the typecheck silently stopped checking anything.
            if res.returncode != 0:
                print(f"\nerror: dotnet build of {name} failed (exit {res.returncode}) without "
                      f"reporting any C# error.\n       This is a build/SDK/NuGet failure, NOT a clean "
                      f"typecheck -- nothing was verified.\n", file=sys.stderr)
                print(out, file=sys.stderr)
                return 2
            built[name] = out_dir / f"{name}.dll"

    if errors:
        print(f"\n{len(errors)} error(s):\n")
        for e in errors:
            print("  " + e)
        return 1
    print("OK - 0 errors")
    return 0


if __name__ == "__main__":
    sys.exit(main())
