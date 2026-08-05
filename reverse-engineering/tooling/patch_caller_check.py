#!/usr/bin/env python3
"""Patch verified .NET Reactor Assembly caller guards in smethod_N methods."""
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import pipeline as pl  # noqa: E402

def patch(dll_path: Path, out_path: Path) -> None:
    records = pl.patch_caller_check(dll_path, out_path)
    patches = [record for record in records if "method" in record]
    for record in patches:
        print(f"  Patched {record['method']} at IL_{record['offset']:04X}: "
              f"{record['replacement']}")
    print(f"\nPatched {len(patches)} smethod guards -> {out_path}")


if __name__ == '__main__':
    if len(sys.argv) < 2:
        print("Usage: patch_caller_check.py <input.dll> [output.dll]")
        sys.exit(1)

    inp = Path(sys.argv[1])
    out = Path(sys.argv[2]) if len(sys.argv) > 2 else inp.with_stem(inp.stem + '-patched')

    print(f"Input:  {inp}")
    print(f"Output: {out}")
    patch(inp, out)
