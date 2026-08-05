#!/usr/bin/env python3
"""
de4dot_scorecard.py
--------------------
Fast-iteration pipeline for testing a de4dot build against this project's real .NET Reactor
samples: deobfuscate -> decompile -> ilverify -> triage report, in one command instead of
re-typing the same multi-step manual pipeline every time.

This automates exactly the manual steps repeatedly typed out in a past investigation session
(PATH setup for ilspycmd/ilverify, the OPENSSL_ENABLE_SHA1_SIGNATURES env var de4dot needs on
some samples, the long Unity reference-assembly flag list, and grepping decompiled output for
smethod_N/goto/TODO-stub markers). See dreadful-re's `resolving-reference-assemblies-for-
decompilation` skill for why the reference set matters and what noise to expect.

IMPORTANT — this is a FAST TRIAGE tool, not the full rigorous correctness methodology.
The `verify` step reports raw ilverify error counts for the original vs. deobfuscated DLL,
categorized by whether the failing method touches a plugin-internal (DreadScripts.*) type.
It does NOT do method-token correlation between the two runs (original and deobfuscated use
different, unrelated symbol names, so a text diff between their error lists is meaningless) -
that rigorous "is this actually introduced by de4dot" comparison still needs a human/agent to
read the specific methods flagged and reason about it, same as the manual process this
session used. Treat this script's numbers as "where to look first," not a final verdict -
see de4dot's `measuring-deobfuscation-correctness-with-ilverify` skill for the full methodology.

Usage
-----
  # Full pipeline against one sample, using the checked-in de4dot binary
  python scripts/de4dot_scorecard.py full ADOverhaul2022

  # Against all three samples
  python scripts/de4dot_scorecard.py full --all

  # Using a freshly-built de4dot from the sibling fork instead of work/de4dot/de4dot
  python scripts/de4dot_scorecard.py full ADOverhaul2022 \\
      --de4dot-dll ../de4dot/Release/net10.0/linux-x64/de4dot.dll

  # Just the marker triage (smethod_N / goto / TODO stub counts) against existing export/ output
  python scripts/de4dot_scorecard.py markers

  # Just decompile+ilverify an already-deobfuscated DLL you produced some other way
  python scripts/de4dot_scorecard.py verify /path/to/some-deobf.dll --original binaries/ADOverhaul2022.dll

  # Every gate at once (1 ilverify, 5 state machines, 6 metadata round-trip, 7 decryption
  # coverage, plus metadata counts), exiting non-zero if any fails. This is the acceptance check
  # for a de4dot change -- run it before and after, with --json, and diff the two files.
  python scripts/de4dot_scorecard.py gates --all --json /tmp/before.json \\
      --de4dot-dll ../de4dot/Release/net10.0/linux-x64/de4dot.dll

Baselines live in pipeline.py next to the gate they belong to (STATE_MACHINE_BASELINE,
DECRYPT_BASELINE) rather than in this file, so a gate and its ceiling cannot drift apart. A gate
whose evidence is MISSING reports FAIL, never zero -- an absent measurement is not a clean one.

Requires ilspycmd and ilverify as dotnet global tools (checks PATH, falls back to ~/.dotnet/tools).
"""

import argparse
from contextlib import contextmanager
import json
import re
import sys
import tempfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import pipeline as pl  # noqa: E402

def deobfuscate(sample_name: str, de4dot_dll: Path, out_dir: Path,
                log_to: Path | None = None) -> Path:
    src = pl.sample(sample_name)
    if not src.exists():
        sys.exit(f"error: sample binary not found: {src}")
    out_dir.mkdir(parents=True, exist_ok=True)
    out = out_dir / f"{sample_name}-deobf.dll"

    # Positional form (`de4dot <file> -o <out>`), not `-f <file>`: -f takes the filename as its
    # value, so it must precede it, and pipeline.deobfuscate puts the file first.
    if pl.deobfuscate(src, out, pl.find_de4dot(de4dot_dll), log_to=log_to) is None:
        sys.exit(f"error: de4dot did not produce {out} (see output above)")
    return out


# `\w*`, not `\w+`: ilverify emits some errors with an EMPTY kind (`Error []:`), and requiring
# a non-empty kind silently dropped them from every count. That is how a dangling method token
# -- a `call` naming a method a later pass deleted -- was reported as "0 errors" while no
# decompiler could open the assembly at all. An uncounted error is worse than a loud one.
ERROR_LINE_RE = re.compile(r"\[IL\]: Error \[(?P<kind>\w*)\]:\s*\[(?P<rest>.*)")
FAILED_LOAD_RE = re.compile(r"Failed to load assembly '([\w.]+)'")


def categorize_ilverify_output(text: str) -> dict:
    total = 0
    unclassified = []
    target_internal = 0
    other = 0
    by_kind = {}
    target_internal_lines = []
    unresolved_assemblies = set()

    for line in text.splitlines():
        m = ERROR_LINE_RE.search(line)
        if not m:
            # A line that looks like a diagnostic but does not parse must NOT be dropped. Three
            # separate confident zeroes here came from exactly that: an incomplete reference set, a
            # regex too narrow to match, and an unchecked return code. A zero is trustworthy only
            # when the tool completed, its output was fully classified, and anything unrecognised is
            # treated as a failure.
            if "Error" in line and "[IL]" in line:
                unclassified.append(line.strip())
            continue
        total += 1
        kind = m.group("kind")
        by_kind[kind] = by_kind.get(kind, 0) + 1

        load_fail = FAILED_LOAD_RE.search(line)
        if load_fail:
            # NOT benign noise: every unresolvable assembly means ilverify skipped some methods
            # entirely, so the real-error count below is an UNDERCOUNT. Surfaced as a warning.
            unresolved_assemblies.add(load_fail.group(1))
            continue

        if "DreadScripts." in line:
            target_internal += 1
            target_internal_lines.append(line.strip())
        else:
            other += 1

    return {
        "total": total,
        "target_internal": target_internal,
        "other": other,
        "by_kind": by_kind,
        "target_internal_lines": target_internal_lines,
        "unresolved_assemblies": sorted(unresolved_assemblies),
        "unclassified": unclassified,
    }


def print_verify_report(label: str, stats: dict):
    print(f"\n=== ilverify: {label} ===")
    print(f"  total errors:            {stats['total']}")
    if stats["unclassified"]:
        print(f"  !! {len(stats['unclassified'])} UNCLASSIFIED diagnostic line(s) -- the counts below")
        print("  !! are NOT trustworthy. Every diagnostic must parse; an unparsed one is a failure,")
        print("  !! not a zero. Widen ERROR_LINE_RE to cover these:")
        for line in stats["unclassified"][:5]:
            print(f"       {line[:160]}")
    if stats["unresolved_assemblies"]:
        print(f"  !! UNRESOLVED ASSEMBLIES: {', '.join(stats['unresolved_assemblies'])}")
        print("  !! Counts below are UNDERCOUNTS -- ilverify silently skips methods it cannot")
        print("  !! fully resolve. Add these to deps/ before trusting any number here.")
    print(f"  other/unclassified:      {stats['other']}")
    print(f"  TARGET-INTERNAL (DreadScripts.*): {stats['target_internal']}  <- look here first")
    if stats["by_kind"]:
        print("  by error kind:", ", ".join(f"{k}={v}" for k, v in sorted(stats["by_kind"].items())))
    if stats["target_internal_lines"]:
        print("  target-internal error lines:")
        for line in stats["target_internal_lines"]:
            print(f"    {line}")


def print_marker_report(label: str, counts: dict, per_file: dict, top_n=5):
    print(f"\n=== marker triage: {label} ===")
    for name, total in counts.items():
        print(f"  {pl.MARKER_LABELS[name]}: {total}")
        for n, path in sorted(per_file[name], reverse=True)[:top_n]:
            print(f"      {n:4d}  {path}")


@contextmanager
def workspace(out: str | None):
    """Use --out persistently, otherwise delete this scorecard run's workspace on exit."""
    if out:
        root = Path(out)
        root.mkdir(parents=True, exist_ok=True)
        yield root
        return
    temporary_parent = pl.ROOT / "work"
    temporary_parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="de4dot_scorecard-", dir=temporary_parent) as temporary:
        yield Path(temporary)


def cmd_deobf(args):
    with workspace(args.out) as out_root:
        out = deobfuscate(args.sample, Path(args.de4dot_dll), out_root)
        print(f"\ndeobfuscated -> {out}")
        if not args.out:
            print("workspace discarded after this command; pass --out DIR to keep the DLL")


def cmd_verify(args):
    stats = categorize_ilverify_output(pl.ilverify(Path(args.dll)))
    print_verify_report(args.dll, stats)
    if args.original:
        orig_stats = categorize_ilverify_output(pl.ilverify(Path(args.original)))
        print_verify_report(f"{args.original} (ORIGINAL, for comparison)", orig_stats)
        print("\nNOTE: error counts between original and deobfuscated are NOT directly diffable by "
              "name (obfuscated vs. de4dot-renamed symbols don't correspond) — compare target-internal "
              "COUNTS as a rough signal, then manually inspect the specific flagged methods to judge "
              "whether each is pre-existing or de4dot-introduced.")


def cmd_markers(args):
    root = Path(args.root) if args.root else pl.ROOT / "export"
    counts, per_file = pl.scan_decompiled_tree(root, exclude_module=True)
    print_marker_report(str(root), counts, per_file)


def cmd_full(args):
    samples = pl.sample_names() if args.all else [args.sample]
    de4dot_dll = Path(args.de4dot_dll)
    with workspace(args.out) as out_root:
        for name in samples:
            print(f"\n{'#'*70}\n# {name}\n{'#'*70}")
            deobf_dll = deobfuscate(name, de4dot_dll, out_root)

            stats = categorize_ilverify_output(pl.ilverify(deobf_dll))
            print_verify_report(f"{name} (deobfuscated)", stats)

            if args.compare_original:
                orig_stats = categorize_ilverify_output(pl.ilverify(pl.sample(name)))
                print_verify_report(f"{name} (ORIGINAL)", orig_stats)

            decompiled_dir = out_root / f"{name}-decompiled"
            if pl.decompile(deobf_dll, decompiled_dir) is None:
                sys.exit(f"error: ilspycmd did not decompile {deobf_dll} (see output above)")
            counts, per_file = pl.scan_decompiled_tree(decompiled_dir, exclude_module=True)
            print_marker_report(f"{name} (fresh decompile)", counts, per_file)


def gate_report(name: str, de4dot_dll: Path, out_root: Path) -> dict:
    """
    Every gate that can be measured from one de4dot run, for one sample.

    Gates 2/3/4 are not separate steps here: ilverify covers stack imbalance, and empty bodies and
    missing exits show up in the metadata counts and in gate 6's decompile.
    """
    log = out_root / f"{name}-de4dot.log"
    dll = deobfuscate(name, de4dot_dll, out_root, log_to=log)
    result: dict = {"sample": name}

    summary, looping = pl.state_machine_trace(log.read_text(encoding="utf-8", errors="replace"))
    result["gate5"] = summary
    result["gate5_methods"] = sorted(looping)
    # Absence is a failure, not a zero: see pipeline.state_machine_trace.
    result["gate5_ok"] = summary is not None and summary["non_terminating"] <= pl.STATE_MACHINE_BASELINE

    stats = categorize_ilverify_output(pl.ilverify(dll))
    result["gate1"] = stats
    result["gate1_ok"] = (stats["target_internal"] == 0
                          and not stats["unclassified"]
                          and not stats["unresolved_assemblies"])

    ok, detail = pl.metadata_roundtrip(dll)
    result["gate6"] = {"ok": ok, "detail": detail.strip()[-300:]}
    result["gate6_ok"] = ok

    result["counts"] = pl.metadata_counts(dll)
    result["counts_original"] = pl.metadata_counts(pl.sample(name))

    decompiled = out_root / f"{name}-decompiled"
    result["decompiled"] = pl.decompile(dll, decompiled) is not None
    if not result["decompiled"]:
        result["gate7_ok"] = False
        return result
    total, within, detail = pl.decryption_coverage(decompiled, name)
    result["gate7"] = {"residual": total, "within_budget": within, "detail": detail}
    result["gate7_ok"] = within
    counts, _ = pl.scan_decompiled_tree(decompiled, exclude_module=True)
    result["markers"] = counts
    result["landmarks"] = pl.landmark_check(decompiled, name)
    result["landmarks_ok"] = all(l["ok"] for l in result["landmarks"])
    return result


def print_gate_report(result: dict):
    def verdict(ok):
        return "PASS" if ok else "FAIL"

    print(f"\n=== gates: {result['sample']} ===")
    print(f"  gate 1 ilverify           {verdict(result['gate1_ok'])}  "
          f"target-internal={result['gate1']['target_internal']} "
          f"unclassified={len(result['gate1']['unclassified'])}")
    if result["gate5"] is None:
        print("  gate 5 state machines     FAIL  summary line ABSENT from de4dot's output --")
        print("         this is NOT zero. Either the trace did not run, or its wording changed;")
        print("         fix that before reading any other number here.")
    else:
        g5 = result["gate5"]
        print(f"  gate 5 state machines     {verdict(result['gate5_ok'])}  "
              f"non-terminating={g5['non_terminating']} (ceiling {pl.STATE_MACHINE_BASELINE}), "
              f"exit-reachable={g5['exit_reachable']}, undecidable={g5['undecidable']}, "
              f"resolutions rejected={g5['rejected_resolutions']}")
        if g5.get("rejected_methods_mismatch"):
            print("         !! the rejection COUNT and the rejected-method NAMES disagree --")
            print("         !! one of the two log lines is stale; do not diff either until fixed.")
        for name in g5.get("rejected_methods", []):
            print(f"         rejected: {name}")
    for line in result["gate5_methods"]:
        print(f"         loops: {line}")
    for landmark in result.get("landmarks", []):
        state = "PASS" if landmark["ok"] else "FAIL"
        print(f"  landmark {state}  {landmark['name']}: "
              f"{landmark['found']} (minimum {landmark['minimum']})")
    print(f"  gate 6 metadata round-trip {verdict(result['gate6_ok'])}  {result['gate6']['detail']}")
    if "gate7" in result:
        print(f"  gate 7 decryption         {verdict(result['gate7_ok'])}  {result['gate7']['detail']}")
    else:
        print("  gate 7 decryption         FAIL  the output could not be decompiled")
    now, before = result["counts"], result["counts_original"]
    # `fields` is printed because leaving it out of this line once cost a correct change: a fix that
    # removed 274 injected fields and moved nothing else showed an identical-looking counts line, was
    # eyeballed as "no effect", and was reverted. The JSON always carried it -- but the printed
    # summary is what people actually compare, so every count the JSON has belongs here too.
    print(f"  counts  types {now['types']}/{before['types']}  methods {now['methods']}/{before['methods']}"
          f"  fields {now['fields']}/{before['fields']}"
          f"  bodies {now['bodies']}/{before['bodies']}  empty {now['empty_bodies']}"
          f"  instrs {now['instructions']}/{before['instructions']}   (deobfuscated/original)")


def cmd_gates(args):
    samples = pl.sample_names() if args.all else [args.sample]
    de4dot_dll = Path(args.de4dot_dll)
    results = []
    with workspace(args.out) as out_root:
        for name in samples:
            result = gate_report(name, de4dot_dll, out_root)
            results.append(result)
            print_gate_report(result)

    failed = [r["sample"] for r in results
              if not all(r.get(k, False) for k in ("gate1_ok", "gate5_ok", "gate6_ok", "gate7_ok", "landmarks_ok"))]
    print("\n=== summary ===")
    print(f"  samples: {len(results)}   failing: {len(failed)}"
          + (f" ({', '.join(failed)})" if failed else ""))
    if args.json:
        Path(args.json).write_text(json.dumps(results, indent=2))
        print(f"  wrote {args.json}")
    # Non-zero exit so this is usable as a check, not only as a report.
    return 1 if failed else 0


def build_parser():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = p.add_subparsers(dest="cmd", required=True)

    common_paths = argparse.ArgumentParser(add_help=False)
    common_paths.add_argument("--de4dot-dll", default=str(pl.DE4DOT_DEFAULT),
                               help=f"Path to de4dot executable or .dll (default: {pl.DE4DOT_DEFAULT})")
    common_paths.add_argument("--out",
                               help="Keep deobfuscated/decompiled output in this directory (default: temporary)")

    p_deobf = sub.add_parser("deobf", parents=[common_paths], help="Just deobfuscate one sample")
    p_deobf.add_argument("sample", choices=pl.sample_names())

    p_verify = sub.add_parser("verify", parents=[common_paths],
                               help="ilverify an arbitrary DLL (e.g. one you deobfuscated another way)")
    p_verify.add_argument("dll")
    p_verify.add_argument("--original", help="Also ilverify this original DLL for comparison")

    p_markers = sub.add_parser("markers", help="Count smethod_N/goto/TODO markers under a directory")
    p_markers.add_argument("--root", help="Directory to scan (default: export/)")

    p_full = sub.add_parser("full", parents=[common_paths],
                             help="deobfuscate -> ilverify -> decompile -> marker triage, one sample or --all")
    p_full.add_argument("sample", nargs="?", choices=pl.sample_names())
    p_full.add_argument("--all", action="store_true", help="Run all three samples")
    p_full.add_argument("--compare-original", action="store_true",
                         help="Also ilverify the original (obfuscated) DLL for a rough comparison")

    p_gates = sub.add_parser("gates", parents=[common_paths],
                              help="Run every gate (1, 5, 6, 7 + metadata counts) and exit non-zero on any failure")
    p_gates.add_argument("sample", nargs="?", choices=pl.sample_names())
    p_gates.add_argument("--all", action="store_true", help="Run all three samples")
    p_gates.add_argument("--json", help="Write the full per-sample result to this file, for diffing two runs")

    return p


def main():
    args = build_parser().parse_args()
    if args.cmd in ("full", "gates") and not args.all and not args.sample:
        sys.exit(f"error: '{args.cmd}' needs a sample name or --all")
    handler = {"deobf": cmd_deobf, "verify": cmd_verify, "markers": cmd_markers,
               "full": cmd_full, "gates": cmd_gates}[args.cmd]
    sys.exit(handler(args) or 0)


if __name__ == "__main__":
    main()
