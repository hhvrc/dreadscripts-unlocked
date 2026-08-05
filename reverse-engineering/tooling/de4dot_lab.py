#!/usr/bin/env python3
"""
de4dot_lab.py
-------------
Controlled A/B experiments against a de4dot build, in a disposable workspace.

Why this exists
---------------
Investigating a de4dot behaviour means running the same loop over and over: deobfuscate the
samples, decompile them, and compare some measurement against a second build or a second set of
flags. Doing that by hand goes wrong in two specific ways, both of which have already happened:

  1. **Decompiling without reference assemblies.** `ilspycmd` with no `-r` still produces output,
     just degraded -- bodies fill up with `Unknown result type (might be due to invalid IL or
     missing references)` and some constructs decompile differently or not at all. An experiment
     comparing two such trees can differ for reasons that have nothing to do with de4dot. All
     reference handling lives in `pipeline.py` now, so every script uses the same complete set.
  2. **Reaching for export/.** export/ is the canonical tree and is regenerated only by
     reexport.py. An experiment must never write there. This script only ever writes to a temp
     workspace and deletes it unless you ask to keep it.

Usage
-----
  # One variant, default flags, report; workspace deleted afterwards
  python3 scripts/de4dot_lab.py run

  # Pass extra args / env to de4dot, and extra args to ilspycmd; keep the tree to inspect
  python3 scripts/de4dot_lab.py run --env DE4DOT_NO_XORSWITCH=1 --keep

  # Bisect levers the fork reads (all default-off). Attributing a defect to a pass means turning
  # that pass off and re-measuring, not reasoning about which one looks guilty:
  #   DE4DOT_NO_XORSWITCH=1        all XOR-switch dispatch resolution
  #   DE4DOT_NO_RELATIONAL=1       just the chained/two-variable resolver, leaving per-site on
  #   DE4DOT_NO_CFLOW_CONSTANTS=1  the <Module> constant fold that decides opaque predicates
  #   DE4DOT_NO_OPAQUE_PREDICATES=1  removal of Reactor's never-assigned static + `== null` pairs
  python3 scripts/de4dot_lab.py run --env DE4DOT_NO_RELATIONAL=1 --sample ADOverhaul2019

  # Extra de4dot flags work the same way
  python3 scripts/de4dot_lab.py run --de4dot-arg --no-cflow-deob --sample ADOverhaul2022

  # A/B two variants and diff the measurements (this is the main event)
  python3 scripts/de4dot_lab.py ab --a "" --b "--env DE4DOT_NO_XORSWITCH=1"

  # Look at one type's C# or IL from a variant
  python3 scripts/de4dot_lab.py show --type AdvisorTemplate --sample ADOverhaul2019
  python3 scripts/de4dot_lab.py show --type AdvisorTemplate --sample ADOverhaul2019 --il

  # Remove any workspaces left behind by --keep
  python3 scripts/de4dot_lab.py clean

Never touches export/. For the canonical regeneration use reexport.py; for a
correctness triage of a single build use de4dot_scorecard.py.
"""

import argparse
import atexit
import shlex
import shutil
import sys
import tempfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import pipeline as pl  # noqa: E402
from detect_broken_state_machines import scan_file  # noqa: E402

# Workspaces live under one parent so `clean` can find them all.
LAB_ROOT = Path(tempfile.gettempdir()) / "de4dot_lab"

# ---------------------------------------------------------------------------- variant

class Variant:
    """One (de4dot binary, de4dot args, env, ilspycmd args) combination."""

    def __init__(self, name, de4dot, de4dot_args, env_pairs, ilspy_args):
        self.name = name
        self.de4dot = pl.find_de4dot(de4dot)
        self.de4dot_args = de4dot_args
        self.ilspy_args = ilspy_args
        overrides = {}
        for pair in env_pairs:
            if "=" not in pair:
                sys.exit(f"error: --env expects KEY=VALUE, got '{pair}'")
            k, v = pair.split("=", 1)
            overrides[k] = v
        self.env_overrides = overrides
        self.env = pl.tool_env(overrides)

    def describe(self) -> str:
        bits = []
        if self.de4dot_args:
            bits.append("de4dot " + " ".join(self.de4dot_args))
        if self.env_overrides:
            bits.append("env " + " ".join(f"{k}={v}" for k, v in sorted(self.env_overrides.items())))
        if self.ilspy_args:
            bits.append("ilspy " + " ".join(self.ilspy_args))
        return "; ".join(bits) or "(defaults)"

    def build(self, sample_paths: list[Path], workspace: Path) -> tuple[Path, list[str]]:
        """
        Deobfuscate + decompile every sample.

        Returns (src root, names that failed). Failures are returned rather than skipped: a report
        built from a partial corpus reads exactly like a complete one, and comparing a 3-sample
        variant against a 2-sample variant would show a difference that is not about de4dot at all.
        """
        dll_dir = workspace / self.name / "dll"
        src_root = workspace / self.name / "src"
        log_dir = workspace / self.name / "log"
        failed: list[str] = []
        for sample in sample_paths:
            print(f"  {self.name}: {sample.stem} ...", file=sys.stderr)
            # Keep the run's log. Plenty of what de4dot reports about its own decisions -- rejected
            # dispatch resolutions, unproven-premise warnings -- exists only here and not in the
            # output assembly, so a workspace kept with --keep that threw the log away could not
            # answer why a variant decided what it did.
            deobf = pl.deobfuscate(sample, dll_dir / sample.name, self.de4dot,
                                   extra_args=self.de4dot_args, env=self.env,
                                   log_to=log_dir / f"{sample.stem}.log")
            if deobf is None:
                failed.append(sample.stem)
                continue
            if pl.decompile(deobf, src_root / sample.stem,
                            extra_args=self.ilspy_args, env=self.env) is None:
                failed.append(sample.stem)
        return src_root, failed


# ---------------------------------------------------------------------------- measure

def measure(src_root: Path) -> dict:
    """Every measurement this script knows how to take, over a decompiled tree."""
    stats = {"files": 0, "lines": 0, "TERMINATES": 0, "LOOPS": 0, "UNKNOWN": 0,
             "loops_detail": [], "unresolved_refs": 0}
    marker_counts, _ = pl.scan_decompiled_tree(
        src_root, marker_names=pl.LAB_MARKERS, exclude_names={"AssemblyInfo.cs"})
    stats.update(marker_counts)

    for cs in sorted(src_root.rglob("*.cs")):
        if cs.name == "AssemblyInfo.cs":
            continue
        text = cs.read_text(encoding="utf-8", errors="ignore")
        stats["files"] += 1
        stats["lines"] += text.count("\n")
        stats["unresolved_refs"] += text.count("might be due to invalid IL or missing references")
        for verdict, method, line, var, seed, path in scan_file(cs):
            stats[verdict] += 1
            if verdict == "LOOPS":
                stats["loops_detail"].append(
                    f"{cs.relative_to(src_root)}:{line} {method}() seed={seed} "
                    f"trace={' -> '.join(map(str, path or []))}")
    return stats


REPORT_ROWS = [
    ("files", "decompiled .cs files"),
    ("lines", "total lines"),
    ("unresolved_refs", "unresolved-reference comments"),
    ("switch-dispatch", "switch(var) dispatch sites"),
    ("LOOPS", "  ...never terminating"),
    ("TERMINATES", "  ...terminating"),
    ("UNKNOWN", "  ...undecidable"),
    ("opaque-dispatch", "opaque-predicate dispatch sites"),
    ("goto", "goto statements"),
    ("smethod_N", "undecrypted smethod_N calls"),
]


def print_report(title: str, stats: dict) -> None:
    print(f"\n=== {title} ===")
    for key, label in REPORT_ROWS:
        print(f"  {label:<34} {stats[key]:>8}")
    if stats["unresolved_refs"]:
        print(f"\n  note: {stats['unresolved_refs']} unresolved-reference comment(s) -- the "
              f"reference set is incomplete,\n        so this tree is degraded and not "
              f"comparable to export/.")


def print_ab(a_name, a, b_name, b) -> None:
    print(f"\n=== {a_name} vs {b_name} ===")
    print(f"  {'':<34} {a_name:>12} {b_name:>12} {'delta':>10}")
    for key, label in REPORT_ROWS:
        print(f"  {label:<34} {a[key]:>12} {b[key]:>12} {b[key] - a[key]:>+10}")


# ---------------------------------------------------------------------------- workspace

def make_workspace(keep: bool, out: str | None) -> Path:
    LAB_ROOT.mkdir(parents=True, exist_ok=True)
    if out:
        ws = Path(out).resolve()
        ws.mkdir(parents=True, exist_ok=True)
        return ws
    ws = Path(tempfile.mkdtemp(prefix="run-", dir=LAB_ROOT))
    if not keep:
        atexit.register(lambda: shutil.rmtree(ws, ignore_errors=True))
    return ws


def add_variant_args(p: argparse.ArgumentParser) -> None:
    p.add_argument("--de4dot", default=str(pl.DE4DOT_DEFAULT),
                   help="de4dot binary (default: work/de4dot/de4dot)")
    p.add_argument("--de4dot-arg", action="append", default=[], metavar="ARG",
                   help="extra arg passed to de4dot (repeatable)")
    p.add_argument("--env", action="append", default=[], metavar="K=V",
                   help="env var for de4dot/ilspycmd (repeatable)")
    p.add_argument("--ilspy-arg", action="append", default=[], metavar="ARG",
                   help="extra arg passed to ilspycmd (repeatable)")


def variant_from(args, name: str) -> Variant:
    return Variant(name, args.de4dot, args.de4dot_arg, args.env, args.ilspy_arg)


def parse_variant_spec(spec: str, name: str, base) -> Variant:
    """Parse an `ab --a "..."` spec using the same options as `run`."""
    p = argparse.ArgumentParser(prog=f"--{name}", add_help=False)
    add_variant_args(p)
    parsed, unknown = p.parse_known_args(shlex.split(spec))
    if unknown:
        sys.exit(f"error: unrecognised options in --{name}: {' '.join(unknown)}")
    # A --de4dot given once outside --a/--b applies to both variants.
    if parsed.de4dot == str(pl.DE4DOT_DEFAULT) and base.de4dot != str(pl.DE4DOT_DEFAULT):
        parsed.de4dot = base.de4dot
    return variant_from(parsed, name)


# ---------------------------------------------------------------------------- commands

def cmd_run(args) -> int:
    pl.configure_refs(args.ref)
    ws = make_workspace(args.keep, args.out)
    variant = variant_from(args, "run")
    pl.warn_if_de4dot_stale(variant.de4dot)
    print(f"variant: {variant.describe()}", file=sys.stderr)
    src, failed = variant.build(pl.samples(args.sample), ws)
    stats = measure(src)
    print_report(variant.describe(), stats)
    if failed:
        print(f"\n  !! {len(failed)} sample(s) FAILED to deobfuscate: {', '.join(failed)}\n"
              f"     The report above covers only the samples that succeeded.")
    if args.show_loops and stats["loops_detail"]:
        print("\n  non-terminating state machines:")
        for d in stats["loops_detail"]:
            print(f"    {d}")
    if args.keep or args.out:
        print(f"\nworkspace kept: {ws}")
    return 1 if failed else 0


def cmd_ab(args) -> int:
    pl.configure_refs(args.ref)
    ws = make_workspace(args.keep, args.out)
    a = parse_variant_spec(args.a, "a", args)
    b = parse_variant_spec(args.b, "b", args)
    for v in (a, b):
        pl.warn_if_de4dot_stale(v.de4dot)
    print(f"A: {a.describe()}\nB: {b.describe()}", file=sys.stderr)
    sample_paths = pl.samples(args.sample)
    a_src, a_failed = a.build(sample_paths, ws)
    b_src, b_failed = b.build(sample_paths, ws)
    if a_failed or b_failed:
        # Refuse to print a comparison between different corpora -- the deltas would be dominated
        # by the missing assembly rather than by the variant under test.
        print(f"\nerror: cannot compare -- deobfuscation failed for "
              f"A: {a_failed or 'none'}, B: {b_failed or 'none'}", file=sys.stderr)
        return 1
    a_stats = measure(a_src)
    b_stats = measure(b_src)
    print_report(f"A -- {a.describe()}", a_stats)
    print_report(f"B -- {b.describe()}", b_stats)
    print_ab("A", a_stats, "B", b_stats)

    only_a = sorted(set(a_stats["loops_detail"]) - set(b_stats["loops_detail"]))
    only_b = sorted(set(b_stats["loops_detail"]) - set(a_stats["loops_detail"]))
    if only_a or only_b:
        print("\n  non-terminating state machines only in A:")
        for d in only_a or ["(none)"]:
            print(f"    {d}")
        print("  non-terminating state machines only in B:")
        for d in only_b or ["(none)"]:
            print(f"    {d}")
    if args.keep or args.out:
        print(f"\nworkspace kept: {ws}")
    return 0


def cmd_show(args) -> int:
    pl.configure_refs(args.ref)
    ws = make_workspace(args.keep, args.out)
    variant = variant_from(args, "show")
    sample_paths = pl.samples(args.sample)
    if len(sample_paths) != 1:
        sys.exit("error: show needs exactly one --sample")

    if args.original:
        # Same reference set and the same decompiler invocation as the deobfuscated side, which is
        # the only reason the two dumps are comparable. "Check the original first" is this project's
        # most-repeated lesson and there was no sanctioned way to do it -- doing it by hand means a
        # bare ilspycmd with no -r, whose degraded output silently is not comparable to anything.
        target = sample_paths[0]
    else:
        target = pl.deobfuscate(sample_paths[0], ws / "show" / sample_paths[0].name,
                                variant.de4dot, extra_args=variant.de4dot_args, env=variant.env)
        if target is None:
            return 1

    print(pl.decompile_type(target, args.type, il=args.il,
                            extra_args=variant.ilspy_args, env=variant.env))
    if args.keep or args.out:
        print(f"\nworkspace kept: {ws}", file=sys.stderr)
    return 0


def cmd_clean(args) -> int:
    if not LAB_ROOT.is_dir():
        print(f"nothing to clean ({LAB_ROOT} does not exist)")
        return 0
    kept = sorted(LAB_ROOT.iterdir())
    if not kept:
        print(f"nothing to clean ({LAB_ROOT} is empty)")
        return 0
    total = 0
    for d in kept:
        size = sum(f.stat().st_size for f in d.rglob("*") if f.is_file())
        total += size
        print(f"  removing {d}  ({size / 1e6:.1f} MB)")
        shutil.rmtree(d, ignore_errors=True)
    print(f"freed {total / 1e6:.1f} MB")
    return 0


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest="cmd", required=True)

    common = argparse.ArgumentParser(add_help=False)
    common.add_argument("--sample", action="append", default=[],
                        help="sample stem, e.g. ADOverhaul2022 (repeatable; default: all)")
    common.add_argument("--keep", action="store_true",
                        help="keep the temp workspace and print its path")
    common.add_argument("--out", help="use this directory instead of a temp one (implies --keep)")
    # Top-level, not per-variant: the reference set is process-wide (see pipeline.refs()), so both
    # sides of an A/B must share it or the comparison is not measuring what it claims to.
    common.add_argument("--ref", action="append", default=[], metavar="DIR",
                        help="extra reference-assembly directory (repeatable; applies to all variants)")

    p_run = sub.add_parser("run", parents=[common], help="build one variant and report")
    add_variant_args(p_run)
    p_run.add_argument("--show-loops", action="store_true",
                       help="list every non-terminating state machine found")
    p_run.set_defaults(func=cmd_run)

    p_ab = sub.add_parser("ab", parents=[common], help="build two variants and diff them")
    add_variant_args(p_ab)
    p_ab.add_argument("--a", default="", help='variant A option string, e.g. "--env FOO=1"')
    p_ab.add_argument("--b", default="", help="variant B option string")
    p_ab.set_defaults(func=cmd_ab)

    p_show = sub.add_parser("show", parents=[common], help="dump one type's C# or IL")
    add_variant_args(p_show)
    p_show.add_argument("--type", help="type name to decompile (ilspycmd -t)")
    p_show.add_argument("--il", action="store_true", help="show IL instead of C#")
    p_show.add_argument("--original", action="store_true",
                        help="decompile the ORIGINAL obfuscated binary instead of a deobfuscated "
                             "one -- for checking whether something is de4dot's doing or the "
                             "input's")
    p_show.set_defaults(func=cmd_show)

    p_clean = sub.add_parser("clean", help="delete workspaces left by --keep")
    p_clean.set_defaults(func=cmd_clean)

    args = ap.parse_args()
    return args.func(args)


if __name__ == "__main__":
    sys.exit(main())
