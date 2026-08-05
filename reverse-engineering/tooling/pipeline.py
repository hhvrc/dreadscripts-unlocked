#!/usr/bin/env python3
"""
pipeline.py
-----------
Shared toolchain for the deobfuscate -> decompile pipeline. Imported by reexport.py,
de4dot_scorecard.py and de4dot_lab.py; not meant to be run directly, though
`python3 scripts/pipeline.py` prints the resolved reference set, which is a useful sanity check.

Why this is centralised
-----------------------
The three scripts each need to find the same tools, set the same environment, resolve the same
reference assemblies, and invoke de4dot and ilspycmd the same way. Each had drifted to its own
answer, and every divergence was a silent correctness problem rather than a style issue:

  * **Reference assemblies.** reexport.py auto-detected a *machine-local Unity Hub install*, so the
    canonical export/ was generated against whatever Unity happened to be installed -- and against
    no Unity at all on a machine without it. de4dot_scorecard.py used the checked-in deps/ for
    ilverify but passed **no -r at all** to ilspycmd, so the tree it triaged was degraded while the
    verification beside it was not. Ad-hoc experiment commands passed nothing anywhere.
  * **Why that matters more than it looks.** An incomplete set never fails loudly. ilspycmd emits
    "Unknown result type (might be due to invalid IL or missing references)" and carries on;
    ilverify **skips** methods it cannot fully resolve, which *under-counts* errors. That
    under-count is how a real baseline of 17 errors was recorded as 8 for months. Three different
    reference sets also make any comparison between the three scripts' output meaningless.
  * **Tool environment.** de4dot needs OPENSSL_ENABLE_SHA1_SIGNATURES=1 on SHA1-signed samples and
    ilspycmd needs DOTNET_ROLL_FORWARD=LatestMajor. Forgetting either produces a confusing failure
    that looks like a bug in the sample.

So deps/ is the answer, it is checked in on purpose, and it is complete. A machine-local Unity is
only ever a fallback, and it says so loudly.

If you are here because you are about to write a script
-------------------------------------------------------
Reusable tooling lives in `scripts/` and gets committed, even when written mid-task for a single
measurement -- see the "Scripts" section of AGENTS.md, which is the authoritative statement of this
rule. A genuinely single-use snippet (hand-simulating one method, decoding one constant, counting
something in one file) is fine to leave in scratch; anything that invokes the toolchain, or produces
a number a later session would want to compare against, is not.

Check first whether the job already has an owner and extend that instead of adding a rival:

  * `de4dot_scorecard.py`  -- run a de4dot build against the samples and report. `gates` is the
                              full acceptance check (gates 1/5/6/7 + metadata counts, non-zero exit
                              on failure, --json for diffing two runs).
  * `de4dot_lab.py`        -- A/B two de4dot configurations against each other.
  * `reexport.py`          -- the canonical, destructive export/ regeneration.
  * `tools/IlRename`       -- anything that reads or rewrites assembly metadata (`counts`, `report`,
                              `usages`, `apply`).

**If the workflow you need does not exist and a future session would plausibly need it too, add it**
-- a subcommand, a flag, a baseline constant -- then list it in AGENTS.md's tool table and in the
covering skill so the next agent finds it rather than rebuilding it slightly differently.

Two invariants for anything added here:

  * **Nothing but this module invokes the toolchain.** Import it; do not shell out to de4dot,
    ilspycmd, ilverify or ilrename yourself, or the reference set and argument order drift again.
  * **Baselines live next to their gate, in this file** (STATE_MACHINE_BASELINE, DECRYPT_BASELINE),
    never inline in the script that reads them, and a gate whose evidence is MISSING must report a
    failure rather than a zero.
"""

import os
import contextlib
import errno
import json
import re
import shutil
import socket
import subprocess
import tempfile
import time
import sys
from datetime import timedelta
from pathlib import Path

def configure_console() -> None:
    """
    Make stdout/stderr able to carry this toolkit's output on Windows.

    Every report here uses box-drawing rules, arrows and em dashes, and a Windows console hands
    Python a cp1252 stdout, which cannot encode any of them. The failure is not cosmetic: printing
    one of those characters raises UnicodeEncodeError mid-report, so a script dies *after* doing its
    work and before saying what it found -- `explore_dreadscripts.py` crashed that way, and
    `sync_public.py` printed its findings with the arrow replaced by a replacement character.

    errors="replace" as well as UTF-8, because a console whose code page is still legacy will
    otherwise fail on the same characters at write time. Losing a dash beats losing the report.
    """
    if sys.platform != "win32":
        return
    for stream in (sys.stdout, sys.stderr):
        reconfigure = getattr(stream, "reconfigure", None)
        if reconfigure is not None:
            try:
                reconfigure(encoding="utf-8", errors="replace")
            except (ValueError, OSError):
                pass  # a redirected or already-detached stream; not worth failing over


configure_console()

ROOT = Path(__file__).resolve().parent.parent
BINARIES = ROOT / "binaries"

# Checked into the repo on purpose: the set must be COMPLETE and REPRODUCIBLE.
DEPS_UNITY = ROOT / "deps" / "unity"
DEPS_VRCHAT = ROOT / "deps" / "vrchat"

# The reference assemblies are Unity's and VRChat's own redistributables, so this repository
# identifies them rather than shipping them: reference-assemblies.json lists every file with its
# SHA-256 and pins the exact product build. Reassemble the set at the two paths above and the whole
# pipeline works unchanged.
REFERENCE_MANIFEST = ROOT / "reference-assemblies.json"


def _reference_set_instructions() -> str:
    """
    What to do about a missing reference set. Deliberately an instruction rather than a diagnostic.

    The old wording said "restore the checked-in set", which was true when the assemblies lived in
    the repository and is actively misleading now that they do not -- it sends the reader looking
    for something that was never committed here.
    """
    return (
        f"The reference assemblies are not shipped with this repository; they are Unity's and\n"
        f"VRChat's redistributables. {REFERENCE_MANIFEST.name} lists every file with its SHA-256\n"
        f"and names the exact build each came from. Reassemble the set to:\n"
        f"  {DEPS_UNITY}\n"
        f"  {DEPS_VRCHAT}\n"
        f"and re-run. The manifest is enough to verify you have the identical files."
    )

# ============================================================ exclusive access to tracked trees

# Lives under work/, which is gitignored, so the lock is never itself a tracked artifact.
EXPORT_LOCK = ROOT / "work" / "export.lock"


@contextlib.contextmanager
def exclusive_tree_lock(lock_path: Path = EXPORT_LOCK, wait_seconds: float = 0.0,
                        purpose: str = "regenerating export/"):
    """
    Hold an exclusive lock for the duration of a destructive rebuild of a tracked tree.

    Why this exists: `reexport.py` **deletes export/ wholesale** and rebuilds it. Two of those
    running at once interleave a delete against the other's writes, and the result is not a failure
    anyone notices at the time -- it is a tracked tree that is silently missing most of its files,
    with a plausible-looking diff. That has actually happened here: a concurrent pair produced a
    67846-line deletion that read like a catastrophic de4dot regression and was neither.

    Fails fast by default rather than queueing, and says who holds it. A run that blocks invisibly
    behind another is nearly as confusing as one that races it; `wait_seconds` is opt-in for when you
    genuinely want to queue.
    """
    lock_path.parent.mkdir(parents=True, exist_ok=True)
    deadline = time.monotonic() + wait_seconds
    handle = None

    while True:
        try:
            handle = os.open(lock_path, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
            break
        except OSError as exc:
            if exc.errno != errno.EEXIST:
                raise
            holder = _read_lock_holder(lock_path)
            if _holder_is_dead(holder):
                # Only reclaim a lock whose owning process is gone AND which we can name; an
                # unreadable lock is left alone, because "I could not parse it" is not evidence
                # that nobody is using it.
                print(f"note: reclaiming stale lock from dead pid {holder.get('pid')}", file=sys.stderr)
                with contextlib.suppress(OSError):
                    lock_path.unlink()
                continue
            if time.monotonic() >= deadline:
                sys.exit(
                    f"error: {lock_path.name} is held -- another run is {holder.get('purpose', 'using this tree')}.\n"
                    f"  holder: pid {holder.get('pid', '?')} on {holder.get('host', '?')}, "
                    f"since {holder.get('started', '?')}\n"
                    f"  Refusing to start: this operation deletes the tree it rebuilds, so two at once\n"
                    f"  produce a silently incomplete result rather than an error. Wait for it to\n"
                    f"  finish, or pass --wait-lock SECONDS to queue behind it."
                )
            time.sleep(0.5)

    try:
        os.write(handle, json.dumps({
            "pid": os.getpid(),
            "host": socket.gethostname(),
            "started": time.strftime("%Y-%m-%d %H:%M:%S"),
            "purpose": purpose,
        }).encode())
        os.close(handle)
        handle = None
        yield
    finally:
        if handle is not None:
            with contextlib.suppress(OSError):
                os.close(handle)
        with contextlib.suppress(OSError):
            lock_path.unlink()


def _read_lock_holder(lock_path: Path) -> dict:
    try:
        return json.loads(lock_path.read_text() or "{}")
    except (OSError, ValueError):
        return {}


def _holder_is_dead(holder: dict) -> bool:
    """True only when the holder is this machine's and its process is provably gone."""
    pid = holder.get("pid")
    if not isinstance(pid, int) or holder.get("host") != socket.gethostname():
        return False
    try:
        os.kill(pid, 0)
    except ProcessLookupError:
        return True
    except OSError:
        return False
    return False


DE4DOT_SRC = ROOT.parent / "de4dot"
_EXE = ".exe" if sys.platform == "win32" else ""
DE4DOT_DEFAULT = ROOT / "work" / "de4dot" / f"de4dot{_EXE}"
DE4DOT_RID = "win-x64" if sys.platform == "win32" else "linux-x64"


# Files a usable build output must contain. A framework directory that has been created but not
# populated (a failed or partial build) is worse than a missing one: copying it into work/ produces a
# de4dot that launches and then fails obscurely.
_REQUIRED_BUILD_FILES = ("de4dot.dll", "de4dot.code.dll", "de4dot.blocks.dll", "dnlib.dll")

_SOLUTION_CANDIDATES = ("de4dot.net.slnf", "de4dot.slnx", "de4dot.netcore.sln")


def _build_out_hint() -> str:
    """Best-effort path for use *inside* error messages, which must never themselves exit."""
    out = de4dot_build_out(required=False)
    return str(out) if out else str(DE4DOT_SRC / "Release" / "net10.0" / DE4DOT_RID)


def de4dot_solution(required: bool = True) -> str | None:
    """
    The solution/filter to build.

    Probed rather than hardcoded: upstream renamed de4dot.netcore.sln to de4dot.net.slnf (plus
    de4dot.slnx) in the .NET 10 multitargeting change, and a stale name here produces a bare
    MSB1009 "Project file does not exist" that reads like a broken checkout.
    """
    for name in _SOLUTION_CANDIDATES:
        if (DE4DOT_SRC / name).exists():
            return name
    if not required:
        return None
    sys.exit(
        f"error: no supported de4dot solution found in {DE4DOT_SRC}\n"
        f"  looked for: {', '.join(_SOLUTION_CANDIDATES)}\n"
        "  Upstream renames these (netcore.sln -> net.slnf -> slnx). If the name changed again, add\n"
        "  it to _SOLUTION_CANDIDATES in scripts/pipeline.py rather than guessing at the call site."
    )


def de4dot_build_out(required: bool = True) -> Path | None:
    """
    The fork's build output directory, or None when `required` is False and it cannot be resolved.

    Discovered, not hardcoded: the target framework moves with upstream (net8.0 -> net10.0), and a
    hardcoded framework silently pointed the republish step at a directory that no longer existed.

    Fails loudly on all three ways this can go wrong, because each one otherwise ends in a confident
    wrong answer downstream:
      * nothing built yet -- no framework directory contains a host;
      * ambiguity -- more than one does, so "newest wins" would be an arbitrary choice between two
        real builds;
      * a populated-looking but incomplete directory -- host present, core assemblies missing.

    DE4DOT_BUILD_OUT in the environment overrides the probe for the ambiguous case.
    """
    override = os.environ.get("DE4DOT_BUILD_OUT")
    if override:
        candidates = [Path(override)]
    else:
        release = DE4DOT_SRC / "Release"
        candidates = [c for c in sorted(release.glob(f"net*/{DE4DOT_RID}"))
                      if (c / f"de4dot{_EXE}").exists()]

    if not candidates:
        if not required:
            return None
        sys.exit(
            f"error: no de4dot build output found under {DE4DOT_SRC / 'Release'}/net*/{DE4DOT_RID}\n"
            f"  Build the fork first:  dotnet build -c Release "
            f"\"{DE4DOT_SRC / (de4dot_solution(required=False) or _SOLUTION_CANDIDATES[0])}\""
        )

    if len(candidates) > 1:
        if not required:
            return None
        listed = "\n".join(f"    {c}" for c in candidates)
        sys.exit(
            "error: more than one de4dot build output contains a host, so the choice is ambiguous:\n"
            f"{listed}\n"
            "  Pick one explicitly with DE4DOT_BUILD_OUT=<dir>, or remove the stale Release/ tree and\n"
            "  rebuild. Guessing here would silently measure whichever build sorted last."
        )

    chosen = candidates[0]
    missing = [f for f in _REQUIRED_BUILD_FILES if not (chosen / f).exists()]
    if missing:
        if not required:
            return None
        sys.exit(
            f"error: de4dot build output at {chosen} is incomplete -- missing "
            f"{', '.join(missing)}\n"
            "  This is a failed or partial build. Rebuild rather than publishing it: a de4dot that\n"
            "  launches and then fails on a missing assembly is harder to diagnose than one that is\n"
            "  simply absent."
        )
    return chosen

DOTNET_TOOLS = Path.home() / ".dotnet" / "tools"

# Only used if deps/unity is missing.
UNITY_VERSION = "2022.3.22f1"

# Binaries in binaries/ that are not obfuscated samples.
SKIP_BINARIES = {"0Harmony.dll"}


# ============================================================ reference assemblies

class RefSet:
    """A resolved reference-assembly set, plus how complete it is."""

    def __init__(self, dirs: list[Path], warnings: list[str], from_deps: bool):
        self.dirs = dirs
        self.warnings = warnings
        self.from_deps = from_deps

    @property
    def complete(self) -> bool:
        """True only for the checked-in deps/ set; anything else may under-resolve."""
        return self.from_deps and not self.warnings

    def ilspy_flags(self) -> list[str]:
        """ilspycmd takes a directory per -r."""
        flags: list[str] = []
        for d in self.dirs:
            flags += ["-r", str(d)]
        return flags

    def ilverify_flags(self) -> list[str]:
        """ilverify takes a glob per -r."""
        flags: list[str] = []
        for d in self.dirs:
            flags += ["-r", str(d / "*.dll")]
        return flags

    def dll_paths(self, exclude: set[str] | None = None) -> list[Path]:
        """
        Every reference assembly as an individual file, for consumers that need explicit paths
        rather than -r directories (e.g. <Reference> elements in a generated csproj).

        `exclude` drops assemblies by file name -- needed because deps/unity ships both an
        aggregate facade and the split modules that declare the same types.
        """
        skip = exclude or set()
        found: dict[str, Path] = {}
        for d in self.dirs:
            if not d.is_dir():
                continue
            for dll in sorted(d.glob("*.dll")):
                if dll.name not in skip:
                    found.setdefault(dll.name, dll)
        return sorted(found.values())

    def warn(self, stream=sys.stderr) -> None:
        for w in self.warnings:
            print(f"warning: {w}", file=stream)
        if not self.from_deps:
            print(
                "warning: falling back to machine-local reference assemblies.\n"
                f"         The reproducible set lives in {DEPS_UNITY} and {DEPS_VRCHAT}.\n"
                "         Results are NOT comparable to export/ or to recorded baselines, and\n"
                "         ilverify will silently SKIP methods it cannot resolve, which\n"
                "         under-counts errors rather than failing loudly.",
                file=stream,
            )

    def describe(self) -> str:
        origin = "deps/ (checked in)" if self.from_deps else "machine-local (NOT reproducible)"
        return f"{len(self.dirs)} reference dir(s) from {origin}"

    def require_complete(self, why: str = "") -> None:
        """
        Hard-fail unless this is the complete checked-in set.

        For measurement (ilverify) an incomplete set is worse than no answer: it produces a
        confident number that is an under-count, because ilverify silently skips what it cannot
        resolve. So measurement callers exit here rather than warn.
        """
        if self.complete:
            return
        detail = "\n".join(f"  - {w}" for w in self.warnings) or "  - (reference set is incomplete)"
        sys.exit(
            f"error: incomplete reference-assembly set{' for ' + why if why else ''}:\n"
            f"{detail}\n"
            "The set must be COMPLETE or ilverify silently SKIPS methods it cannot fully resolve,\n"
            "which UNDER-COUNTS errors instead of failing. A confident wrong number is worse than\n"
            "no number: this is how a real baseline of 17 errors was recorded as 8 for months.\n"
            f"{_reference_set_instructions()}"
        )


_REFS: "RefSet | None" = None


def refs() -> "RefSet":
    """
    The process-wide reference set.

    No caller needs a *different* set -- there is exactly one correct answer per machine, and having
    each script resolve its own was how they drifted apart in the first place. So this resolves once,
    warns once, and every pipeline function defaults to it. A CLI `--ref` flag goes through
    configure_refs() at startup rather than being threaded through every call.
    """
    global _REFS
    if _REFS is None:
        _REFS = resolve_refs()
        _REFS.warn()
    return _REFS


def configure_refs(extra: list[str] | None = None, allow_machine_local: bool = True) -> "RefSet":
    """Set the process-wide reference set from CLI flags. Call once, early."""
    global _REFS
    _REFS = resolve_refs(extra, allow_machine_local)
    _REFS.warn()
    return _REFS


def _hub_roots() -> list[Path]:
    """Unity Hub install roots, for the fallback path only."""
    return [p for p in (
        Path("/opt/Unity/Hub/Editor"),
        Path.home() / "Unity" / "Hub" / "Editor",
        Path("C:/Program Files/Unity/Hub/Editor"),
    ) if p.is_dir()]


def _machine_local_unity() -> tuple[list[Path], list[str]]:
    for root in _hub_roots():
        managed = root / UNITY_VERSION / "Editor" / "Data" / "Managed"
        if managed.is_dir():
            dirs = [managed]
            ue = managed / "UnityEngine"       # UnityEngine module DLLs live in a subdir
            if ue.is_dir():
                dirs.append(ue)
            return dirs, []
    return [], [f"Unity {UNITY_VERSION} not found locally either; "
                f"decompiled output will lack Unity type info entirely"]


def resolve_refs(extra: list[str] | None = None, allow_machine_local: bool = True) -> RefSet:
    """
    Resolve the reference set, preferring the checked-in deps/ tree.

    `extra` adds directories after the resolved set (e.g. a --ref CLI flag). Missing extras are
    warned about but still appended, so a typo is visible rather than silently dropped.
    """
    dirs: list[Path] = []
    warnings: list[str] = []
    from_deps = True

    if DEPS_UNITY.is_dir():
        dirs.append(DEPS_UNITY)
    else:
        from_deps = False
        warnings.append(f"missing {DEPS_UNITY}")
        if allow_machine_local:
            local, local_warnings = _machine_local_unity()
            dirs.extend(local)
            warnings.extend(local_warnings)

    if DEPS_VRCHAT.is_dir():
        dirs.append(DEPS_VRCHAT)
    else:
        from_deps = False
        warnings.append(f"missing {DEPS_VRCHAT} -- VRChat SDK types will not resolve")

    # binaries/ is deliberately NOT a reference dir. Its 0Harmony.dll is byte-identical to the copy
    # in deps/unity, so adding it buys nothing -- and adding the whole directory would pull the
    # obfuscated sample assemblies into the set, where they would be handed to the compiler as
    # references for the reconstruction of those same assemblies.

    for e in extra or []:
        d = Path(e)
        if not d.is_dir():
            warnings.append(f"--ref path is not a directory: {d}")
        dirs.append(d)

    return RefSet(dirs, warnings, from_deps)


# ============================================================ tools and process

def find_tool(name: str, prefer: list[Path] | None = None) -> str:
    """Locate a dotnet global tool: `prefer` dirs first, then PATH, then ~/.dotnet/tools."""
    for d in prefer or []:
        candidate = d / f"{name}{_EXE}"
        if candidate.exists():
            return str(candidate)
    found = shutil.which(name)
    if found:
        return found
    candidate = DOTNET_TOOLS / f"{name}{_EXE}"
    if candidate.exists():
        return str(candidate)
    sys.exit(f"error: '{name}' not found on PATH or in {DOTNET_TOOLS} — "
             f"install with: dotnet tool install -g {name}")


def find_ilspycmd() -> str:
    """work/ilspycmd/ takes precedence, so a pinned local build can be dropped in."""
    return find_tool("ilspycmd", prefer=[ROOT / "work" / "ilspycmd"])


def tool_env(extra: dict[str, str] | None = None) -> dict[str, str]:
    """
    Environment both tools need.

    OPENSSL_ENABLE_SHA1_SIGNATURES: de4dot fails on SHA1-signed assemblies without it on modern
    OpenSSL, in a way that looks like a corrupt sample.
    DOTNET_ROLL_FORWARD: ilspycmd is built against an older runtime than is installed here.
    """
    env = os.environ.copy()
    env["OPENSSL_ENABLE_SHA1_SIGNATURES"] = "1"
    env["DOTNET_ROLL_FORWARD"] = "LatestMajor"
    env.update(extra or {})
    return env


def run(cmd, env=None, quiet=True, desc: str | None = None, check: bool = False):
    """Run a command, capturing output. `check=True` exits on nonzero, printing both streams."""
    if desc:
        print(f"  {desc}")
    elif not quiet:
        print(f"$ {' '.join(str(c) for c in cmd)}", file=sys.stderr)
    result = subprocess.run([str(c) for c in cmd], env=env if env is not None else tool_env(),
                            capture_output=True, text=True, encoding="utf-8", errors="replace")
    if check and result.returncode != 0:
        print(f"  FAILED: {' '.join(str(c) for c in cmd)}", file=sys.stderr)
        if result.stdout.strip():
            print(result.stdout, file=sys.stderr)
        if result.stderr.strip():
            print(result.stderr, file=sys.stderr)
        sys.exit(1)
    return result


# ============================================================ de4dot

def de4dot_missing_error(path: Path) -> str:
    """
    The message printed when de4dot is absent.

    Deliberately an instruction, not a diagnostic. An agent that reads "de4dot not found" tends to
    look for another copy on the machine, or to skip the step and carry on; both are wrong. export/
    is by definition the output of the fork at ../de4dot, and an experiment run against some other
    build measures something other than the source being edited.
    """
    worker_src = DE4DOT_SRC / "Release" / "net8.0" / DE4DOT_RID / "publish"
    worker_dst = DE4DOT_DEFAULT.parent / "constdata"
    copy_cmd = (
        f'xcopy /E /I /Y "{_build_out_hint()}" "{DE4DOT_DEFAULT.parent}"\n'
        f'      dotnet publish "{DE4DOT_SRC / "de4dot.constdata"}" -c Release -r {DE4DOT_RID}\n'
        f'      xcopy /E /I /Y "{worker_src}" "{worker_dst}"'
        if sys.platform == "win32"
        else f'cp -a "{_build_out_hint()}/." "{DE4DOT_DEFAULT.parent}/"\n'
             f'      dotnet publish "{DE4DOT_SRC / "de4dot.constdata"}" -c Release -r {DE4DOT_RID}\n'
             f'      mkdir -p "{worker_dst}" && cp -a "{worker_src}/." "{worker_dst}/"'
    )
    return f"""\
error: de4dot not found at {path}

  THIS IS A REQUIRED BUILD STEP, NOT AN OPTIONAL ONE. Do not look for a de4dot
  elsewhere on this machine, and do not skip it and continue -- export/ is by
  definition the output of the fork at {DE4DOT_SRC}, so any
  other de4dot produces output that silently does not match the source being edited.

  Build the fork and publish it into work/, then re-run:

      dotnet build -c Release "{DE4DOT_SRC / (de4dot_solution(required=False) or _SOLUTION_CANDIDATES[0])}"
      mkdir -p "{DE4DOT_DEFAULT.parent}"
      {copy_cmd}

  If the build fails, fix the build. This step is blocked until it succeeds.\
"""


def find_de4dot(path: str | Path | None = None) -> Path:
    """Resolve the de4dot binary, exiting with build instructions if it is not there."""
    p = Path(path) if path else DE4DOT_DEFAULT
    if p.exists():
        warn_if_constdata_worker_missing(p)
        return p
    sys.exit(de4dot_missing_error(p))


def warn_if_constdata_worker_missing(de4dot: Path) -> None:
    """
    Warn when the net8.0 extraction worker is not reachable from this de4dot.

    Worth its own check for the same reason as the staleness one above: without the worker, de4dot
    still exits 0 and still writes a deobfuscated assembly -- one in which nothing is decrypted.
    That output passes ilverify, the state-machine trace and the metadata round-trip, so the only
    gate that notices is 7, and it reads like a decrypter regression in the fork rather than a
    missing file here. It cost a session once: a copy of Release/net10.0/<rid>/ into work/ leaves
    the worker behind, because in the fork's own layout it is found by probing the *sibling*
    Release/net8.0/<rid>/ directory, and that sibling does not come along.

    Only a warning, never fatal: DE4DOT_CONSTDATA_WORKER can point somewhere else entirely, and
    deobfuscate() still fails hard on the marker if extraction really did not happen.
    """
    if os.environ.get("DE4DOT_CONSTDATA_WORKER"):
        return
    exe = f"de4dot.constdata{_EXE}"
    base = de4dot.parent
    if (base / exe).exists() or (base / "constdata" / exe).exists():
        return
    # The fork's dev layout: .../Release/<tfm>/<rid>/ with the worker under a sibling <tfm>.
    rid = base.name
    release = base.parent.parent
    if release.is_dir() and any((d / rid / exe).exists() for d in release.iterdir() if d.is_dir()):
        return
    print(f"warning: no {exe} found beside {de4dot}, in {base / 'constdata'}, or in a sibling\n"
          f"  framework directory. Constant and string decryption will silently produce nothing;\n"
          f"  gate 7 is the only gate that will notice. Publish the worker:\n"
          f"      dotnet publish {DE4DOT_SRC / 'de4dot.constdata'} -c Release -r {DE4DOT_RID}\n"
          f"  then copy Release/net8.0/{DE4DOT_RID}/publish/ into {base / 'constdata'}/",
          file=sys.stderr)


def warn_if_de4dot_stale(de4dot: Path | None = None) -> None:
    """
    Loudly flag a de4dot older than the fork's sources.

    This is the failure that has actually happened here: work/de4dot sat at a months-old build
    while ../de4dot kept improving, so every export/ regenerated in between silently predated the
    fixes it was supposed to contain. Unlike a missing binary, a stale one reports success -- which
    is why it needs its own check rather than being left to the caller to remember.
    """
    binary = Path(de4dot) if de4dot else DE4DOT_DEFAULT
    if not DE4DOT_SRC.is_dir():
        return  # fork not checked out beside this repo; nothing to compare against
    try:
        binary_mtime = binary.stat().st_mtime
    except OSError:
        return

    # Date the build by the NEWEST published file, not by the launcher. `de4dot` is the apphost: an
    # incremental build rewrites de4dot.code.dll and leaves the apphost untouched, so timing the
    # build by it reports a current build as stale on every run. A warning that cries wolf every
    # session is worse than no warning, because the real staleness this exists to catch (a
    # months-old work/de4dot) then gets waved through as the usual noise.
    for sibling in binary.parent.glob("*.dll"):
        try:
            binary_mtime = max(binary_mtime, sibling.stat().st_mtime)
        except OSError:
            continue

    newest, newest_src = binary_mtime, None
    for src in DE4DOT_SRC.rglob("*.cs"):
        if any(part in ("obj", "bin", "Release", "Debug") for part in src.parts):
            continue
        try:
            mtime = src.stat().st_mtime
        except OSError:
            continue
        if mtime > newest:
            newest, newest_src = mtime, src

    if newest_src is None:
        return

    print(
        f"\n!! {binary} is STALE -- {timedelta(seconds=int(newest - binary_mtime))} older than "
        f"the fork's sources.\n"
        f"   newest source: {newest_src.relative_to(DE4DOT_SRC)}\n"
        f"   Running now would produce output that does not contain those changes.\n"
        f"   Rebuild and re-publish before continuing:\n"
        f'       dotnet build -c Release "{DE4DOT_SRC / (de4dot_solution(required=False) or _SOLUTION_CANDIDATES[0])}"\n'
        f'       cp -a "{_build_out_hint()}/." "{DE4DOT_DEFAULT.parent}/"\n',
        file=sys.stderr,
    )


def deobfuscate(sample: Path, out: Path, de4dot: Path, extra_args=(), env=None,
                report_errors: bool = True, desc: str | None = None,
                log_to: Path | None = None) -> Path | None:
    """
    Run de4dot on one assembly. Returns the output path, or None if it produced nothing.

    de4dot's own "Could not resolve MethodRef" lines are surfaced by default: those were ignored
    for months while they were reporting a real bug (live methods deleted as unused).

    `log_to` keeps de4dot's combined stdout+stderr. Gate 5 is reported by de4dot itself and exists
    nowhere in the output assembly, so it can only be read back out of the run -- see
    `state_machine_trace`.
    """
    out.parent.mkdir(parents=True, exist_ok=True)
    # Remove any previous artifact FIRST. Callers reuse output directories across runs, so a
    # leftover file from an earlier run would otherwise satisfy the existence check below and be
    # reported as this run's output -- a failed run silently measured as a successful one.
    if out.exists():
        out.unlink()
    cmd = ["dotnet", str(de4dot)] if de4dot.suffix == ".dll" else [str(de4dot)]
    # Global options MUST precede the input file. de4dot parses per-file options (-o) after the
    # filename and SILENTLY IGNORES global options placed there -- no warning, no nonzero exit, just
    # a run that did not do what was asked. Verified: `--no-cflow-deob` after the filename produces
    # a byte-for-byte normal deobfuscation (263680 bytes on ADOverhaul2019), before it produces the
    # cflow-disabled one (456192). reexport.py's docstring records the same trap for
    # --preserve-table. Appending extra_args made every --de4dot-arg in de4dot_lab inert.
    cmd += [*extra_args, str(sample), "-o", str(out)]
    result = run(cmd, env=env if env is not None else tool_env(), desc=desc)
    if log_to is not None:
        log_to.parent.mkdir(parents=True, exist_ok=True)
        log_to.write_text(result.stdout + result.stderr, encoding="utf-8", errors="replace")
    # Existence alone is not success: de4dot can write a partial file and still fail.
    if result.returncode != 0 or not out.exists():
        why = (f"exit code {result.returncode}" if result.returncode != 0
               else "produced no output file")
        print(f"error: de4dot failed on {sample.name} ({why})", file=sys.stderr)
        print(result.stdout[-2000:], file=sys.stderr)
        print(result.stderr[-2000:], file=sys.stderr)
        if out.exists():
            out.unlink()  # do not leave a partial artifact for a later run to pick up
        return None
    blob = result.stdout + result.stderr
    if DECRYPT_FAILURE_MARKER in blob:
        # Not a warning to scroll past: with no data array nothing decrypts, and the output still
        # passes ilverify, the state-machine trace and the metadata round-trip. Historically this
        # meant de4dot had been built for a runtime whose loader rejects Reactor metadata.
        print(f"error: de4dot could not extract the constant decrypter data array for "
              f"{sample.name}.\n"
              f"  Every constant and string is left encrypted, and gates 1-6 cannot see it.\n"
              f"  Usual cause: the net8.0 extraction worker is missing next to the de4dot being run.\n"
              f"  The fork's net8.0 pin was lifted -- the host is net10.0 now and only\n"
              f"  de4dot.constdata stays on net8.0, because .NET 10's loader rejects Reactor\n"
              f"  metadata. de4dot finds that worker beside itself, in a constdata/ subdirectory, or\n"
              f"  in a sibling framework directory; the last of those is what makes it work in\n"
              f"  ../de4dot/Release/ and NOT after copying one framework directory into work/.\n"
              f"  Publish it too:\n"
              f"      dotnet publish {DE4DOT_SRC / 'de4dot.constdata'} -c Release -r {DE4DOT_RID}\n"
              f"      cp -a {DE4DOT_SRC / 'Release' / 'net8.0' / DE4DOT_RID / 'publish'}/. "
              f"{DE4DOT_DEFAULT.parent / 'constdata'}/\n"
              f"  See GenericConstantDecrypter.TryDynamicExtract and ConstantDataWorker.FindWorker.",
              file=sys.stderr)
        out.unlink(missing_ok=True)
        return None
    if report_errors:
        for line in blob.splitlines():
            if "Could not resolve" in line or "ERROR" in line:
                print(f"  [de4dot:{sample.stem}] {line.strip()}", file=sys.stderr)
    return out


# ============================================================ ilspycmd

def flatten_ilspy_project(out_dir: Path, dll: Path) -> None:
    """Flatten ilspycmd's optional solution/project wrapper into ``out_dir``."""
    solution = out_dir / "solution.sln"
    project_dir = out_dir / dll.stem
    if not solution.exists() or not project_dir.is_dir():
        return
    print("  Flattening nested project directory ...")
    solution.unlink()
    for item in list(project_dir.iterdir()):
        item.rename(out_dir / item.name)
    project_dir.rmdir()


def decompile(dll: Path, out_dir: Path, extra_args=(), il: bool = False,
              project: bool = True, flatten_project: bool = False, env=None,
              desc: str | None = None):
    """Decompile to a directory, always with the process-wide reference set."""
    # A failed ILSpy run can leave a partial tree behind. Callers often reuse a workspace, so
    # remove it before each run rather than letting a later marker scan read stale source.
    if out_dir.exists():
        shutil.rmtree(out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)
    cmd = [find_ilspycmd()]
    if project:
        cmd.append("-p")
    if il:
        cmd.append("-il")
    cmd += ["-o", str(out_dir), *refs().ilspy_flags(), *extra_args, str(dll)]
    result = run(cmd, env=env if env is not None else tool_env(), desc=desc)
    if result.returncode != 0:
        print(f"error: ilspycmd failed on {dll.name} (exit code {result.returncode})",
              file=sys.stderr)
        print(result.stdout[-2000:], file=sys.stderr)
        print(result.stderr[-2000:], file=sys.stderr)
        shutil.rmtree(out_dir, ignore_errors=True)
        return None
    if flatten_project:
        flatten_ilspy_project(out_dir, dll)
    return out_dir


def metadata_roundtrip(dll: Path) -> tuple[bool, str]:
    """
    Gate 6: can a full metadata consumer load and decompile every member of this assembly?

    `ilverify` clean does NOT imply this. A dangling method token -- a `call` whose operand names a
    method that a later pass deleted -- passes ilverify with zero errors and makes ILSpy throw
    `ArgumentNullException (Parameter 'methodReference')` while reading the body. That happened here:
    a rollback experiment produced an assembly ilverify called perfect and no decompiler could open.

    So the emitted assembly must round-trip through a real metadata consumer, independently of
    ilverify. Returns (ok, detail); detail carries the first meaningful error line on failure.
    """
    with tempfile.TemporaryDirectory(prefix="roundtrip_") as tmp:
        cmd = [find_ilspycmd(), "-p", "-o", tmp, *refs().ilspy_flags(), str(dll)]
        result = run(cmd, env=tool_env())
        if result.returncode == 0:
            return True, "loads and decompiles"
        blob = (result.stdout or "") + (result.stderr or "")
        for line in blob.splitlines():
            s = line.strip()
            if s and "not using the latest" not in s and "Latest version" not in s:
                return False, s
        return False, f"ilspycmd exit {result.returncode}"


def decompile_type(dll: Path, type_name: str | None, il: bool = False,
                   extra_args=(), env=None) -> str:
    """Decompile a single type (or the whole assembly if type_name is None) to stdout."""
    cmd = [find_ilspycmd(), *refs().ilspy_flags()]
    if il:
        cmd.append("-il")
    if type_name:
        cmd += ["-t", type_name]
    cmd += [*extra_args, str(dll)]
    result = run(cmd, env=env if env is not None else tool_env())
    if result.stderr.strip():
        print(result.stderr.strip()[:800], file=sys.stderr)
    return result.stdout


# ============================================================ ilrename

ILRENAME_SRC = ROOT / "tools" / "IlRename"
ILRENAME = ROOT / "work" / "ilrename" / f"ilrename{_EXE}"
SOURCE_ANALYSIS_SRC = ROOT / "tools" / "SourceAnalysis"
SOURCE_ANALYSIS = ROOT / "work" / "source-analysis" / f"SourceAnalysis{_EXE}"


def ensure_ilrename() -> Path | None:
    """Build tools/IlRename into work/ilrename/ if it isn't there yet."""
    if ILRENAME.exists():
        return ILRENAME
    if not ILRENAME_SRC.is_dir():
        return None
    print("Building ilrename ...")
    run(["dotnet", "publish", str(ILRENAME_SRC), "-c", "Release",
         "-o", str(ILRENAME.parent), "-v", "q", "--nologo"],
        desc=f"dotnet publish -> {ILRENAME.parent.relative_to(ROOT)}/", check=True)
    return ILRENAME if ILRENAME.exists() else None


def ensure_source_analysis() -> Path | None:
    """Build the Roslyn/dnlib helper used by source and IL analysis scripts."""
    if SOURCE_ANALYSIS.exists():
        return SOURCE_ANALYSIS
    if not SOURCE_ANALYSIS_SRC.is_dir():
        return None
    print("Building source-analysis helper ...")
    run(["dotnet", "publish", str(SOURCE_ANALYSIS_SRC), "-c", "Release",
         "-o", str(SOURCE_ANALYSIS.parent), "-v", "q", "--nologo"],
        desc=f"dotnet publish -> {SOURCE_ANALYSIS.parent.relative_to(ROOT)}/", check=True)
    return SOURCE_ANALYSIS if SOURCE_ANALYSIS.exists() else None


def source_analysis(command: str, source: Path) -> list[dict]:
    """Run a JSONL source-analysis command and return its records."""
    helper = ensure_source_analysis()
    if helper is None:
        sys.exit(f"error: source-analysis helper is unavailable at {SOURCE_ANALYSIS_SRC}")
    result = run([str(helper), command, "--in", str(source)])
    if result.returncode != 0:
        print(result.stderr, file=sys.stderr)
        sys.exit(f"error: source-analysis {command} failed for {source}")
    try:
        return [json.loads(line) for line in result.stdout.splitlines() if line.strip()]
    except json.JSONDecodeError as exc:
        sys.exit(f"error: source-analysis emitted invalid JSON: {exc}")


def patch_caller_check(source: Path, output: Path) -> list[dict]:
    """Patch verified .NET Reactor caller guards through the IL-aware helper."""
    helper = ensure_source_analysis()
    if helper is None:
        sys.exit(f"error: source-analysis helper is unavailable at {SOURCE_ANALYSIS_SRC}")
    result = run([str(helper), "patch-caller-check", "--in", str(source), "--out", str(output)])
    if result.returncode != 0:
        print(result.stderr, file=sys.stderr)
        sys.exit(f"error: caller-guard patch failed for {source}")
    try:
        return [json.loads(line) for line in result.stdout.splitlines() if line.strip()]
    except json.JSONDecodeError as exc:
        sys.exit(f"error: source-analysis emitted invalid JSON: {exc}")


def ilrename_apply(ilrename: Path, source: Path, out: Path, rename_map: Path,
                   desc: str | None = None) -> Path:
    """Apply a rename map to an assembly's metadata."""
    run([str(ilrename), "apply", "--in", str(source), "--out", str(out),
         "--map", str(rename_map)],
        desc=desc or f"ilrename {rename_map.name} -> {out.name}", check=True)
    return out


def metadata_counts(dll: Path) -> dict:
    """
    Type/method/body/instruction/field totals for an assembly, straight out of the metadata.

    Counts are the one regression signal that survives renaming: de4dot's generated names bear no
    relation to the original's, so nothing textual is diffable between a deobfuscated assembly and
    its input, but "did a pass delete 200 method bodies" is answerable either way.
    """
    ilrename = ensure_ilrename()
    if ilrename is None:
        sys.exit(f"error: ilrename is unavailable at {ILRENAME_SRC}")
    result = run([str(ilrename), "counts", "--in", str(dll)])
    if result.returncode != 0:
        print(result.stderr, file=sys.stderr)
        sys.exit(f"error: ilrename counts failed for {dll}")
    try:
        return json.loads(result.stdout.strip())
    except json.JSONDecodeError as exc:
        sys.exit(f"error: ilrename counts emitted invalid JSON for {dll}: {exc}")


def ilrename_report(ilrename: Path, source: Path, rename_map: Path) -> None:
    """
    Print naming coverage for a map. Never fails the caller.

    Captured rather than inherited: when the parent's stdout is a pipe or file it is block-buffered,
    while a child writing directly is not, so letting the child inherit made the whole report appear
    at the TOP of the log, detached from the "[Assembly] naming coverage" header it belongs to.
    """
    result = subprocess.run([str(ilrename), "report", "--in", str(source), "--map", str(rename_map)],
                            check=False, capture_output=True, text=True,
                            encoding="utf-8", errors="replace")
    if result.stdout.strip():
        print(result.stdout.rstrip())
    if result.stderr.strip():
        print(result.stderr.rstrip(), file=sys.stderr)


# ============================================================ ilverify

def ilverify(dll: Path, system_module: str = "mscorlib", require_complete: bool = True) -> str:
    """
    Verify IL, returning the combined output.

    Completeness is enforced here rather than left to each caller: ilverify silently *skips* any
    method it cannot fully resolve, so a missing assembly under-counts errors rather than failing.
    Callers must still treat a "Failed to load assembly" line in the *output* as invalidating the
    counts -- that catches an assembly missing from an otherwise-complete deps/ tree.
    """
    if require_complete:
        refs().require_complete("ilverify")
    result = run([find_tool("ilverify"), str(dll), *refs().ilverify_flags(), "-s", system_module])
    return result.stdout + result.stderr


# ============================================================ decryption coverage

# A de4dot run can succeed, emit verifiable IL, terminate, and round-trip through a decompiler while
# having decrypted NOTHING. That happened: building for net10.0 broke the Reactor constant decrypter
# (it extracts its data array by loading the target in-process, which .NET 10's loader refuses), and
# undecrypted call sites went 97 -> 3777 with a single warning to show for it. Gates 1-6 were all green.
DECRYPT_MARKER = re.compile(r"smethod_\d+(<[^>]*>)?\(")

# Baselines from a known-good net8.0 build. Some residue is expected -- a few decrypters cannot be
# resolved statically -- so these are budgets, not zeros. The signal worth catching is the
# order-of-magnitude jump that means the decrypter produced no data at all.
#
# Enforced PER ASSEMBLY as well as in aggregate. An aggregate-only ceiling can hide a large regression
# in one sample behind an improvement in another: 35/35/27 summing to 97 would still pass a limit of
# 150 if one assembly tripled while another fell to zero.
DECRYPT_BASELINE = {
    "ADOverhaul2019": 35,
    "ADOverhaul2022": 35,
    "ControllerEditor": 27,
}
# Absolute slack per assembly. Small on purpose: residual counts move by ones when a decrypter changes,
# and by hundreds when extraction fails outright.
DECRYPT_MARGIN = 10
# Aggregate ceiling, for trees scanned as a whole (e.g. all of export/) where per-sample attribution
# is not available.
DECRYPT_BUDGET = sum(DECRYPT_BASELINE.values()) + DECRYPT_MARGIN * len(DECRYPT_BASELINE)


def decryption_budget(sample: str | None = None) -> int:
    """Ceiling for one assembly by stem, or the aggregate when `sample` is None or unknown."""
    if sample is None or sample not in DECRYPT_BASELINE:
        return DECRYPT_BUDGET
    return DECRYPT_BASELINE[sample] + DECRYPT_MARGIN

# de4dot says this, once, when the constant decrypter got no data. Everything downstream is then
# garbage, so it is a hard failure rather than a warning to scroll past.
DECRYPT_FAILURE_MARKER = "Could not extract generic constant decrypter data array"


def decryption_coverage(src_root: Path, sample: str | None = None) -> tuple[int, bool, str]:
    """
    Gate 7: residual `smethod_N(...)` call sites in a decompiled tree.

    Returns (count, within_budget, detail). Pass `sample` (the assembly stem) to check that
    assembly's own ceiling instead of the aggregate -- see DECRYPT_BASELINE for why that matters.
    """
    total = 0
    for cs in src_root.rglob("*.cs"):
        total += len(DECRYPT_MARKER.findall(cs.read_text(encoding="utf-8", errors="ignore")))
    ceiling = decryption_budget(sample)
    scope = f"{sample} ceiling" if sample in DECRYPT_BASELINE else "aggregate ceiling"
    ok = total <= ceiling
    detail = (f"{total} residual call site(s), {scope} {ceiling}"
              + ("" if ok else " -- OVER: decryption regressed, check whether the constant decrypter "
                              "produced a data array at all"))
    return total, ok, detail


# ============================================================ gate 5: state-machine termination

# de4dot reports this once per assembly whether or not it found anything, so its ABSENCE is a
# failure signal rather than a clean result -- see state_machine_trace.
# "exit-reachable", not "terminating": de4dot's tracer over-approximates, so reaching an exit only
# proves one exists in a machine that is a superset of the real one. Another explored path may loop.
# Do not report this count as "methods that terminate" -- it is the absence of a non-termination
# proof, and only `non_terminating` is a proof of anything.
STATE_MACHINE_LINE = re.compile(
    r"State-machine trace: (\d+) non-terminating, (\d+) exit-reachable, (\d+) undecidable")
NON_TERMINATING_LINE = re.compile(r"Non-terminating dispatch in (.+?) \(states ([^)]*)\)")
# How many methods de4dot deliberately left unresolved because the resolved form never exits. The
# number to sanity-check gate 5 against: these are the machines that WOULD have been non-terminating.
DISPATCH_SELECTION_LINE = re.compile(
    r"Dispatch selection: kept the unresolved form of (\d+) method\(s\)")

# The identity of each rejected method, not just how many. A flat aggregate can hide one method fixed
# and another newly broken in the same run, so acceptance for any dispatch-resolution change is the
# diff of these SETS -- see de4dot ROADMAP §7 item 3.
REJECTED_METHOD_LINE = re.compile(
    r"Dispatch resolution rejected: (.+?) -- the resolved machine never exits")

# Ceiling per assembly, and it is zero. A non-terminating machine means a switch was resolved to the
# wrong target, and the damage is invisible in every other gate: the method verifies, keeps a
# reachable `ret`, and simply decompiles SHORTER, with every statement past the bad edge gone.
# The corpus stood at 19 before ObfuscatedFile.SelectDispatchCandidate began generating the method
# both ways and rejecting a resolution whose traced machine never exits.
STATE_MACHINE_BASELINE = 0


def state_machine_trace(de4dot_output: str) -> tuple[dict | None, list[str]]:
    """
    Gate 5, read back out of de4dot's own run: (summary, non-terminating methods).

    Returns None for the summary when the line is absent, and callers MUST treat that as a FAILED
    gate, never as zero. An absent line means de4dot did not run the trace, predates it, or changed
    the wording -- none of which is evidence that nothing loops. Reading an absence of output as a
    clean result is the same mistake as the incomplete reference set, the error regex that required
    a non-empty kind, and the unchecked exit code: three confident zeroes that measured nothing.
    """
    match = STATE_MACHINE_LINE.search(de4dot_output)
    methods = [f"{name} [{states}]" for name, states in NON_TERMINATING_LINE.findall(de4dot_output)]
    if match is None:
        return None, methods
    rejected = DISPATCH_SELECTION_LINE.search(de4dot_output)
    rejected_methods = sorted(REJECTED_METHOD_LINE.findall(de4dot_output))
    summary = {"non_terminating": int(match[1]), "exit_reachable": int(match[2]),
               "undecidable": int(match[3]),
               "rejected_resolutions": int(rejected[1]) if rejected else 0,
               "rejected_methods": rejected_methods}
    # The count and the names come from two different log lines; if they disagree, one of them is
    # stale and the set is the one worth trusting less loudly than a mismatch deserves.
    if rejected and len(rejected_methods) != int(rejected[1]):
        summary["rejected_methods_mismatch"] = True
    return (summary, methods)


# ============================================================ decompiled-tree markers and samples

MARKER_PATTERNS = {
    "smethod_N": re.compile(r"smethod_\d+(<[^>]*>)?\("),
    "goto": re.compile(r"\bgoto\b"),
    "TODO": re.compile(r"TODO|/\* stub \*/|NotImplementedException"),
    "switch-dispatch": re.compile(r"^\s*switch \([A-Za-z_]\w*\)\s*$", re.M),
    # `switch ((num = (num * A) ^ B) % 5)` -- Reactor's opaque-predicate dispatch, where the state
    # is transformed inline in the switch expression instead of being held in a plain variable.
    # "switch-dispatch" deliberately does not match these, so they need their own count.
    "opaque-dispatch": re.compile(r"^\s*switch \(\(.+\) % \d+\)\s*$", re.M),
}

MARKER_LABELS = {
    "smethod_N": "smethod_N calls (undecrypted strings/constants)",
    "goto": "goto statements (residual control flow)",
    "TODO": "TODO/stub markers",
    "switch-dispatch": "switch(var) dispatch sites",
    "opaque-dispatch": "opaque-predicate dispatch sites",
}

# ------------------------------------------------------------ live-code landmarks
#
# Specific code that MUST survive deobfuscation, named so its loss is a failed check rather than an
# improvement. Every readability signal here is deletion-gameable -- unresolved dispatches, goto
# density, instruction counts all get "better" when live code disappears -- and gates 1-7 are blind
# to a branch that is REMOVED rather than corrupted: the method still verifies, still terminates,
# still has a body.
#
# These are not hypothetical. A speculative constant-folding change deleted the license-activation
# branch of the method below while every gate stayed green, and it was caught by reading the original
# IL by hand rather than by any measurement. This is that measurement.
#
# A minimum, not an exact count: legitimate specialisation can duplicate a payload, so more is fine
# and fewer is not.
LANDMARKS = {
    "ADOverhaul2019": [
        ("license activation branch reachable",
         re.compile(r'PrintSystem\("activatelicense"\)'), 2),
    ],
}


def landmark_check(src_root: Path, sample: str) -> list[dict]:
    """Count each landmark across a decompiled tree. Empty list when a sample declares none."""
    wanted = LANDMARKS.get(sample, [])
    if not wanted:
        return []
    text = "\n".join(
        f.read_text(errors="ignore")
        for f in sorted(src_root.rglob("*.cs")) if f.name != "AssemblyInfo.cs")
    results = []
    for name, pattern, minimum in wanted:
        found = len(pattern.findall(text))
        results.append({"name": name, "found": found, "minimum": minimum, "ok": found >= minimum})
    return results


SCORECARD_MARKERS = ("smethod_N", "goto", "TODO")
LAB_MARKERS = ("goto", "smethod_N", "switch-dispatch", "opaque-dispatch")


def scan_decompiled_tree(root: Path, marker_names: tuple[str, ...] = SCORECARD_MARKERS,
                         exclude_module: bool = False,
                         exclude_names: set[str] | None = None) -> tuple[dict, dict]:
    """
    Count text markers in a decompiled C# tree and retain per-file totals.

    The source intentionally remains text-scanned: these are triage markers, including comments
    emitted by a decompiler, not semantic source analysis. All callers use this one definition so
    scorecards and A/B lab reports cannot silently drift in what they count.
    """
    unknown = set(marker_names) - MARKER_PATTERNS.keys()
    if unknown:
        raise ValueError(f"unknown marker(s): {', '.join(sorted(unknown))}")

    skipped = exclude_names or set()
    counts = {name: 0 for name in marker_names}
    per_file = {name: [] for name in marker_names}
    for cs_file in sorted(root.rglob("*.cs")):
        if cs_file.name in skipped or (exclude_module and cs_file.name.startswith("-Module-")):
            continue
        try:
            text = cs_file.read_text(errors="ignore")
        except OSError:
            continue
        try:
            display_path = str(cs_file.relative_to(ROOT))
        except ValueError:
            try:
                display_path = str(cs_file.relative_to(root))
            except ValueError:
                display_path = str(cs_file)
        for name in marker_names:
            count = len(MARKER_PATTERNS[name].findall(text))
            if count:
                counts[name] += count
                per_file[name].append((count, display_path))
    return counts, per_file


def samples(names: list[str] | None = None) -> list[Path]:
    """Obfuscated sample assemblies in binaries/, optionally filtered by stem."""
    available = sorted(p for p in BINARIES.glob("*.dll") if p.name not in SKIP_BINARIES)
    if not names:
        return available
    by_stem = {p.stem: p for p in available}
    out = []
    for n in names:
        if n not in by_stem:
            sys.exit(f"error: no such sample '{n}'. Available: {', '.join(sorted(by_stem))}")
        out.append(by_stem[n])
    return out


def sample_names() -> list[str]:
    """Sample stems for CLI choices."""
    return [sample.stem for sample in samples()]


def sample(name: str) -> Path:
    """One named obfuscated sample assembly."""
    return samples([name])[0]


if __name__ == "__main__":
    refs = resolve_refs()
    print(refs.describe())
    for d in refs.dirs:
        print(f"  {d}  ({len(list(d.glob('*.dll')))} dll)")
    refs.warn(sys.stdout)
    print(f"\nsamples: {', '.join(p.stem for p in samples())}")
    print(f"de4dot:  {DE4DOT_DEFAULT}{'' if DE4DOT_DEFAULT.exists() else '  (MISSING)'}")
