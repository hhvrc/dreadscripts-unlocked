#!/usr/bin/env python3
"""
Create the Unity .meta files the restored package is missing, and only those.

Why this exists
---------------
`.meta` files are tracked in this repo rather than left for Unity to produce, so a file ported into
../public/unity/ without one is an incomplete port -- Unity would generate a fresh GUID on first
import and the tracked tree would differ from every other checkout. Porting agents were hand-writing
these, which is three chances to get the YAML shape wrong and one chance to reuse a GUID by accident.

On GUIDs. The splitting-large-classes-into-partials skill says to derive the GUID deterministically
from the asset path. That is not what the tree actually contains: measured 2026-08-04, 130 of 132
.cs.meta files carry a random uuid4 and only two (EditorUtils.Buttons, EditorUtils.Cursors) carry
md5(basename). Rather than rewrite 130 GUIDs -- churn with no benefit, and a real hazard for any
asset whose GUID is referenced by serialized data -- this follows the majority convention and
generates uuid4. What actually matters for parallel porting is that GUIDs are unique and stable once
written, and this script guarantees both by never touching a .meta that already exists.

    python3 scripts/gen_meta.py            # report what is missing, write nothing
    python3 scripts/gen_meta.py --write    # create the missing .meta files
"""

import argparse
import sys
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PACKAGE = ROOT.parent / "unity" / "Assets" / "com.dreadscripts.unlocked"

SCRIPT_META = """fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""

FOLDER_META = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def existing_guids(root: Path) -> set:
    """Every GUID already in the tree, so a new one can be checked against them."""
    guids = set()
    for m in root.rglob("*.meta"):
        for line in m.read_text(encoding="utf-8", errors="replace").splitlines():
            if line.startswith("guid:"):
                guids.add(line.split(":", 1)[1].strip())
                break
    return guids


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__.split("\n")[1],
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--write", action="store_true",
                    help="create the missing files (default is to report only)")
    ap.add_argument("--root", type=Path, default=PACKAGE,
                    help="package root to scan (default: the restored package)")
    args = ap.parse_args()

    root = args.root.resolve()
    if not root.is_dir():
        print(f"no package at {root}")
        return 1

    taken = existing_guids(root)

    # A .meta is needed for every asset and every folder under the package root, but not for the
    # root itself (its .meta lives in the parent directory) and never for a .meta.
    missing = []
    for p in sorted(root.rglob("*")):
        if p.suffix == ".meta" or p.name.startswith("."):
            continue
        if any(part.startswith(".") for part in p.relative_to(root).parts):
            continue
        meta = p.with_name(p.name + ".meta")
        if not meta.exists():
            missing.append((p, meta))

    # A .meta that exists but does not end in a newline is worse than a missing one: Unity refuses
    # the asset with "Parser Failure at line 11: Expect ':' between key and value within mapping",
    # pointing at a line that does not exist, because the file stops mid-token on the last key.
    # Twelve of these sat in the package for months -- a malformed meta is completely silent until
    # an editor actually reads it, so nothing here noticed. This check costs nothing and is the
    # difference between finding it now and finding it when someone opens the project.
    truncated = []
    for p in sorted(root.rglob("*.meta")):
        data = p.read_bytes()
        if data and not data.endswith(b"\n"):
            truncated.append(p)
    if truncated:
        print(f"{len(truncated)} .meta file(s) do not end in a newline; Unity will refuse them:")
        for p in truncated:
            print(f"  {p.relative_to(root)}")
        if args.write:
            for p in truncated:
                data = p.read_bytes()
                p.write_bytes(data + (b"\r\n" if b"\r\n" in data else b"\n"))
            print(f"fixed {len(truncated)}")
        else:
            print("re-run with --write to terminate them")

    if not missing:
        if not truncated:
            print("OK - every asset has a .meta")
        return 1 if truncated and not args.write else 0

    for asset, meta in missing:
        guid = uuid.uuid4().hex
        while guid in taken:
            guid = uuid.uuid4().hex
        taken.add(guid)

        rel = asset.relative_to(root)
        if args.write:
            template = FOLDER_META if asset.is_dir() else SCRIPT_META
            # .meta is LF, unlike the CRLF pinned on the rest of unity/ -- see ../public/.gitattributes,
            # which carves .meta out because Unity's serialiser rewrites it with LF on every save.
            meta.write_text(template.format(guid=guid), encoding="utf-8", newline="\n")
            print(f"wrote  {rel}.meta  guid={guid}")
        else:
            print(f"missing  {rel}.meta")

    if not args.write:
        print(f"\n{len(missing)} missing .meta file(s); re-run with --write to create them")
    return 0


if __name__ == "__main__":
    sys.exit(main())
