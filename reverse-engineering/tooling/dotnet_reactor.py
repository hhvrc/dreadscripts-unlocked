#!/usr/bin/env python3
"""
dotnet_reactor.py
-----------------
Generic toolkit for .NET Reactor string-encryption reverse-engineering.

The transform used by .NET Reactor's smethod_N<string>(int key) is:
  transformed = (key * A) ^ B   [int32 overflow arithmetic]
  type_tag    = (uint)transformed >> 30   (0 = string, else non-string)
  offset      = (transformed & 0x3FFFFFFF) << 2
  string      = utf8_at(byte_0, offset)   (4-byte length prefix then UTF-8 data)

Subcommands
-----------
  detect  <Module.cs>           Auto-extract transform constants from decompiled Module source
  scan    <log> [src_dir]       Scan source dir for all (smethod_N, key) pairs and print resolved strings
  fill    <log> <src_dir>       In-place: replace <Module>.smethod_N<string>(key) with string literals
  dump    <log>                 Print all (offset, string) pairs parsed from a runtime log

Where <log> is either:
  • dumps/intercepted.txt  (Hooks.cs runtime format)
      [STRING] byte_0+OFFSET: "string"
  • dumps/<AssemblyName>.txt  (SmethodStringDumper offline format)
      smethod_N(key) = "string"

Usage examples
--------------
  python dotnet_reactor.py detect "export/Module (ControllerEditor).cs"
  python dotnet_reactor.py scan intercepted.txt ../public/unity/Assets/com.dreadscripts.unlocked/Editor/
  python dotnet_reactor.py fill intercepted.txt ../public/unity/Assets/com.dreadscripts.unlocked/Editor/ --dry-run
  python dotnet_reactor.py dump intercepted.txt

  # With explicit transforms (skips auto-detect or overrides it):
  python dotnet_reactor.py scan intercepted.txt ../public/unity/Assets/com.dreadscripts.unlocked/Editor/ \\
      --transforms '{"smethod_1":[1553271299,-1677909072]}'

  # With a JSON config file:
  python dotnet_reactor.py fill intercepted.txt ../public/unity/Assets/com.dreadscripts.unlocked/Editor/ --config transforms.json
"""

import argparse
import json
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import pipeline as pl  # noqa: E402

# ── Core arithmetic ───────────────────────────────────────────────────────────

def int32_mul(a: int, b: int) -> int:
    """Signed int32 multiply with C# overflow semantics."""
    result = (a * b) & 0xFFFFFFFF
    return result - 0x100000000 if result >= 0x80000000 else result


def key_to_offset(key: int, A: int, B: int) -> tuple[int, int]:
    """
    Apply the .NET Reactor key transform.
    Returns (offset, type_tag) where type_tag 0 == string.
    """
    transformed = int32_mul(key, A) ^ B
    u32 = transformed & 0xFFFFFFFF
    type_tag = u32 >> 30
    offset   = (u32 & 0x3FFFFFFF) << 2
    return offset, type_tag


# ── Log file parsing ─────────────────────────────────────────────────────────
# Two supported formats:
#  1. Hooks.cs runtime:          [STRING] byte_0+OFFSET: "value"
#  2. SmethodStringDumper:       smethod_N(key) = "value"
#
# load_strings()       → {offset: str}               (format 1)
# load_smethod_dump()  → {(method_name, key): str}   (format 2)
# load_log()           → auto-detects format, returns appropriate dict
#

_RUNTIME_RECORD = re.compile(r'^\[STRING\] byte_0\+(?P<offset>\d+): "(?P<value>.*)$')
_SMETHOD_RECORD = re.compile(r'^(?P<method>smethod_\d+)\((?P<key>-?\d+)\)\s*=\s*"(?P<value>.*)$')


def quoted_records(lines: list[str], pattern: re.Pattern):
    """Yield parsed log headers and their possibly multiline quoted value."""
    index = 0
    while index < len(lines):
        match = pattern.match(lines[index].rstrip("\r\n"))
        if match is None:
            index += 1
            continue
        value = match["value"]
        while not value.endswith('"') and index + 1 < len(lines):
            index += 1
            value += "\n" + lines[index].rstrip("\r\n")
        yield match, value[:-1] if value.endswith('"') else value
        index += 1


def parse_runtime_log(lines: list[str]) -> dict[int, str]:
    return {int(match["offset"]): value for match, value in quoted_records(lines, _RUNTIME_RECORD)}


def parse_smethod_dump(lines: list[str]) -> dict[tuple[str, int], str]:
    return {
        (match["method"], int(match["key"])): value
        for match, value in quoted_records(lines, _SMETHOD_RECORD)
    }


def load_strings(log_path: str) -> dict[int, str]:
    """Parse a Hooks.cs runtime log and return {offset: string_value}."""
    return parse_runtime_log(Path(log_path).read_text(encoding="utf-8", errors="replace").splitlines())


def load_smethod_dump(log_path: str) -> dict[tuple[str, int], str]:
    """Parse a SmethodStringDumper log and return {(method_name, key): string}."""
    return parse_smethod_dump(Path(log_path).read_text(encoding="utf-8", errors="replace").splitlines())


def load_log(log_path: str):
    """
    Auto-detect log format and return the appropriate string table.
    Returns (kind, table) where kind is 'offset' or 'direct':
      'offset': {int offset: str value}   — use with transforms + key_to_offset
      'direct': {(str method, int key): str value}   — use directly by (method, key)
    """
    lines = Path(log_path).read_text(encoding="utf-8", errors="replace").splitlines()
    if any(_SMETHOD_RECORD.match(line) for line in lines[:100]):
        return "direct", parse_smethod_dump(lines)
    return "offset", parse_runtime_log(lines)


def detect_transforms(module_cs: str) -> dict[str, tuple[int, int]]:
    """Extract per-method transforms using Roslyn syntax nodes, not text regexes."""
    return {
        record["method"]: (record["a"], record["b"])
        for record in pl.source_analysis("transforms", Path(module_cs))
    }


def scan_source_dir(src_dir: str) -> dict[str, set[int]]:
    """Walk src_dir for live smethod invocations identified by Roslyn."""
    found: dict[str, set[int]] = {}

    for path in Path(src_dir).rglob("*.cs"):
        for record in pl.source_analysis("calls", path):
            name = record["method"]
            key = record["key"]
            found.setdefault(name, set()).add(key)

    return found


# ── Resolve: (smethod, key) → string ─────────────────────────────────────────

def resolve_all(
    found_keys: dict[str, set[int]],
    transforms: dict[str, tuple[int, int]],
    strings: dict[int, str],
) -> dict[tuple[str, int], str | None]:
    """
    For every (method, key) pair, apply the transform and look up the string.
    Returns {(method, key): value_or_None}.
    """
    result: dict[tuple[str, int], str | None] = {}

    for method, keys in found_keys.items():
        AB = transforms.get(method)
        if AB is None:
            for key in keys:
                result[(method, key)] = None
            continue

        A, B = AB
        for key in sorted(keys):
            offset, type_tag = key_to_offset(key, A, B)
            value = strings.get(offset)
            result[(method, key)] = value

    return result


# ── C# string escaping ────────────────────────────────────────────────────────

def cs_escape(s: str) -> str:
    """Escape a Python string for use as a C# regular string literal."""
    s = s.replace('\\', '\\\\')
    s = s.replace('"',  '\\"')
    s = s.replace('\r\n', '\\r\\n')
    s = s.replace('\n',  '\\n')
    s = s.replace('\r',  '\\r')
    s = s.replace('\t',  '\\t')
    return s


# ── Fill: replace live smethod calls in source files ──────────────────────────


def fill_file(
    path: str,
    transforms: dict[str, tuple[int, int]],
    strings: dict[int, str],
    dry_run: bool = False,
    direct: dict[tuple[str, int], str] | None = None,
) -> tuple[int, int]:
    """
    Replace live smethod_N<T>(key) invocations in `path` with string literals.

    Two lookup modes (tried in order):
      1. direct  — {(method_name, key): value}  produced by SmethodStringDumper
      2. offset  — {offset: value} + transforms  produced by Hooks.cs runtime log

    Returns (replacements, skipped_nulls).
    """
    with open(path, encoding="utf-8") as f:
        content = f.read()

    replacements = 0
    skipped      = 0

    for call in reversed(pl.source_analysis("calls", Path(path))):
        method, key = call["method"], call["key"]
        if direct is not None:
            value = direct.get((method, key))
        else:
            transform = transforms.get(method)
            value = None
            if transform is not None:
                offset, _ = key_to_offset(key, *transform)
                value = strings.get(offset)
        if value is None:
            skipped += 1
            continue
        start, end = call["spanStart"], call["spanStart"] + call["spanLength"]
        content = content[:start] + f'"{cs_escape(value)}"' + content[end:]
        replacements += 1

    if replacements:
        if not dry_run:
            with open(path, "w", encoding="utf-8") as f:
                f.write(content)

    return replacements, skipped


# ── CLI subcommands ───────────────────────────────────────────────────────────

def cmd_detect(args: argparse.Namespace) -> None:
    """detect <module_cs>"""
    transforms = detect_transforms(args.module_cs)
    if not transforms:
        print("No transforms detected. Check the decompiler output format.", file=sys.stderr)
        sys.exit(1)

    print(f"Detected {len(transforms)} transform(s):")
    for name, (A, B) in sorted(transforms.items()):
        print(f"  {name}: A={A}, B={B}")

    if args.json:
        out = {k: list(v) for k, v in sorted(transforms.items())}
        print("\nJSON:", json.dumps(out))


def cmd_dump(args: argparse.Namespace) -> None:
    """dump <log>"""
    strings = load_strings(args.log)
    print(f"Loaded {len(strings)} string entries from {args.log}")
    for offset in sorted(strings):
        print(f"  byte_0+{offset}: \"{strings[offset]}\"")


def cmd_scan(args: argparse.Namespace) -> None:
    """scan <log> [src_dir]"""
    kind, table = load_log(args.log)

    if args.src_dir:
        found = scan_source_dir(args.src_dir)
    else:
        # Read from explicit key list if provided
        if not args.keys:
            print("Provide --src or a source directory.", file=sys.stderr)
            sys.exit(1)
        found = {}
        for entry in args.keys:
            name, key_str = entry.split(":")
            found.setdefault(name, set()).add(int(key_str))

    if kind == 'direct':
        # SmethodStringDumper format — direct (method, key) lookup, no transforms needed
        transforms: dict[str, tuple[int, int]] = {}
        resolved: dict[tuple[str, int], str | None] = {
            (method, key): table.get((method, key))
            for method, keys in found.items()
            for key in keys
        }
    else:
        # Hooks.cs runtime format — offset-based lookup via transforms
        transforms = _load_transforms(args)
        resolved = resolve_all(found, transforms, table)

    ok = 0; null = 0
    for (method, key), value in sorted(resolved.items()):
        if value is not None:
            print(f'{method}({key}) = "{value}"')
            ok += 1
        else:
            AB = transforms.get(method)
            if AB is not None:
                offset, tag = key_to_offset(key, AB[0], AB[1])
                print(f'{method}({key}) = NULL  [offset={offset} tag={tag}]')
            else:
                print(f'{method}({key}) = NULL')
            null += 1

    print(f"\nResolved: {ok}, NULL/missing: {null}", file=sys.stderr)


def cmd_fill(args: argparse.Namespace) -> None:
    """fill <log> <src_dir>"""
    dry_run = args.dry_run
    if dry_run:
        print("[DRY RUN — no files will be modified]")

    kind, table = load_log(args.log)

    if kind == 'direct':
        direct     = table
        transforms = {}
        strings    = {}
        print(f"Loaded {len(direct)} direct (method, key) -> string entries from {args.log}")
    else:
        direct     = None
        transforms = _load_transforms(args)
        strings    = table
        print(f"Loaded {len(strings)} offset->string entries from {args.log}")

    total_replaced = 0
    total_skipped  = 0
    unreadable: list[Path] = []

    for path in sorted(Path(args.src_dir).rglob("*.cs")):
        try:
            path.read_text(encoding="utf-8")
        except UnicodeDecodeError:
            unreadable.append(path)
            print(f"error: {path} is not valid UTF-8; no replacement was attempted",
                  file=sys.stderr)
            continue
        except OSError as exc:
            print(f"error: could not read {path}: {exc}", file=sys.stderr)
            continue
        r, s = fill_file(str(path), transforms, strings, dry_run=dry_run, direct=direct)
        total_replaced += r
        total_skipped  += s

        rel = path.relative_to(args.src_dir)
        if r or s:
            print(f"  {rel}: {r} replaced, {s} skipped")
        else:
            print(f"  {rel}: no changes")

    print(f"\nTotal: {total_replaced} replaced, {total_skipped} skipped.")
    if unreadable:
        sys.exit(f"error: {len(unreadable)} source file(s) could not be decoded as UTF-8")


# ── Transform loading helper ──────────────────────────────────────────────────

def _load_transforms(args: argparse.Namespace) -> dict[str, tuple[int, int]]:
    """
    Resolve transform constants from CLI args (in priority order):
    1. --config <json_file>
    2. --transforms <json_inline>
    3. Auto-detect from --module-cs
    4. Fail with a helpful message
    """
    if hasattr(args, 'config') and args.config:
        with open(args.config) as f:
            raw = json.load(f)
        return {k: tuple(v) for k, v in raw.items()}

    if hasattr(args, 'transforms') and args.transforms:
        raw = json.loads(args.transforms)
        return {k: tuple(v) for k, v in raw.items()}

    if hasattr(args, 'module_cs') and args.module_cs:
        transforms = detect_transforms(args.module_cs)
        if transforms:
            print(f"Auto-detected {len(transforms)} transform(s) from {args.module_cs}",
                  file=sys.stderr)
            return transforms

    print(
        "No transforms available. Provide one of:\n"
        "  --module-cs <path>       auto-detect from decompiled Module source\n"
        "  --transforms '<json>'    inline JSON, e.g. '{\"smethod_1\":[A,B]}'\n"
        "  --config <json_file>     JSON file with same format",
        file=sys.stderr,
    )
    sys.exit(1)


# ── Argument parser ───────────────────────────────────────────────────────────

def build_parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(
        description=__doc__,
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    sub = p.add_subparsers(dest="command", required=True)

    # ── detect ───────────────────────────────────────────────────────────────
    p_det = sub.add_parser("detect",
        help="Extract transform constants from a decompiled Module.cs")
    p_det.add_argument("module_cs", metavar="Module.cs",
        help="Path to the decompiled Module source file")
    p_det.add_argument("--json", action="store_true",
        help="Also print constants as JSON (for --transforms / --config)")

    # ── dump ─────────────────────────────────────────────────────────────────
    p_dump = sub.add_parser("dump",
        help="Print all (offset, string) entries from a Hooks.cs runtime log")
    p_dump.add_argument("log", metavar="intercepted.txt")

    # Shared transform options (used by scan + fill)
    _tf = argparse.ArgumentParser(add_help=False)
    _tf.add_argument("--module-cs", dest="module_cs",
        help="Decompiled Module.cs for auto-detecting transforms")
    _tf.add_argument("--transforms", metavar="JSON",
        help='Inline JSON transform constants, e.g. \'{"smethod_1":[A,B]}\'')
    _tf.add_argument("--config", metavar="FILE",
        help="JSON file containing transform constants")

    # ── scan ─────────────────────────────────────────────────────────────────
    p_scan = sub.add_parser("scan", parents=[_tf],
        help="Resolve all (smethod, key) pairs found in source files")
    p_scan.add_argument("log",     metavar="intercepted.txt")
    p_scan.add_argument("src_dir", metavar="src/", nargs="?",
        help="Directory to scan for .cs files (optional if --keys given)")
    p_scan.add_argument("--keys", nargs="*", metavar="smethod_N:KEY",
        help="Explicit list of method:key pairs instead of scanning source")

    # ── fill ─────────────────────────────────────────────────────────────────
    p_fill = sub.add_parser("fill", parents=[_tf],
        help="Replace smethod_N<string>(key) calls in source files with literals")
    p_fill.add_argument("log",     metavar="intercepted.txt")
    p_fill.add_argument("src_dir", metavar="src/")
    p_fill.add_argument("--dry-run", action="store_true",
        help="Print changes without writing files")

    return p


def main() -> None:
    parser = build_parser()
    args   = parser.parse_args()

    dispatch = {
        "detect": cmd_detect,
        "dump":   cmd_dump,
        "scan":   cmd_scan,
        "fill":   cmd_fill,
    }
    dispatch[args.command](args)


if __name__ == "__main__":
    main()
