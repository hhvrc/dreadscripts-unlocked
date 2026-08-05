#!/usr/bin/env python3
"""
Build the VPM listing, and the package zip a release serves.

Why this exists
---------------
A VPM listing repeats every field of the package manifest, once per published version. Written by
hand that is a second copy of facts `package.json` already owns, and it drifts the first time a
version is bumped in one place and not the other -- with the failure showing up as a silent
mismatch inside someone else's Unity project rather than as an error here.

So the listing is generated. `package.json` is the only place the package describes itself; this
reads it, zips the package, hashes the zip, and writes `index.json`. Published versions already in
the listing are preserved, because a listing is append-only from the consumer's point of view: VCC
resolves an installed project against versions it has seen, and deleting one breaks that project.

    python3 vpm/build_listing.py                 # report what would change
    python3 vpm/build_listing.py --write         # write index.json and the zip

The zip is written to `vpm/dist/` and is NOT committed -- it is a release artifact. Attach it to a
GitHub release tagged `v<version>` so the URL in the listing resolves.
"""

import argparse
import hashlib
import json
import shutil
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
PACKAGE = ROOT / "unity" / "Assets" / "com.dreadscripts.unlocked"
LISTING = ROOT / "vpm" / "index.json"
DIST = ROOT / "vpm" / "dist"

# Where the listing is served from, and where release artifacts live. Both are GitHub-derived; if
# the repository ever moves, these are the only two lines that need to change.
REPO = "hhvrc/dreadscripts-unlocked"
LISTING_URL = f"https://{REPO.split('/')[0]}.github.io/{REPO.split('/')[1]}/index.json"
RELEASE_URL = f"https://github.com/{REPO}/releases/download/v{{version}}/{{name}}-{{version}}.zip"

LISTING_INFO = {
    "name": "DreadScripts Unlocked",
    "id": "com.hhvrc.dreadscripts-unlocked",
    "author": "dreadscripts-unlocked contributors",
    "url": LISTING_URL,
    "description": "Community restoration of two discontinued DreadScripts Unity Editor tools.",
}


def build_zip(manifest: dict, write: bool) -> tuple[Path, str]:
    """(zip path, sha256). The hash is what VCC verifies, so it is computed from the real bytes."""
    name, version = manifest["name"], manifest["version"]
    out = DIST / f"{name}-{version}.zip"
    if write:
        DIST.mkdir(parents=True, exist_ok=True)
        if out.exists():
            out.unlink()
        # Deterministic member order so an unchanged package produces an unchanged archive; a hash
        # that moves for no reason makes the listing look stale when nothing has happened.
        with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as z:
            for f in sorted(PACKAGE.rglob("*")):
                if f.is_file():
                    z.write(f, f.relative_to(PACKAGE).as_posix())
    if not out.exists():
        return out, ""
    return out, hashlib.sha256(out.read_bytes()).hexdigest()


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__.split("\n")[1],
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--write", action="store_true", help="write index.json and build the zip")
    args = ap.parse_args()

    manifest_path = PACKAGE / "package.json"
    if not manifest_path.is_file():
        sys.exit(f"error: no package manifest at {manifest_path}")
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    name, version = manifest["name"], manifest["version"]

    listing = json.loads(LISTING.read_text(encoding="utf-8")) if LISTING.is_file() else {}
    versions = listing.get("packages", {}).get(name, {}).get("versions", {})

    zip_path, digest = build_zip(manifest, args.write)

    entry = dict(manifest)
    entry["url"] = RELEASE_URL.format(name=name, version=version)
    if digest:
        entry["zipSHA256"] = digest
    elif version in versions and "zipSHA256" in versions[version]:
        # Keep the hash of an already-published version rather than dropping it when the zip is not
        # present locally -- a listing entry without one cannot be installed.
        entry["zipSHA256"] = versions[version]["zipSHA256"]

    was = versions.get(version)
    versions[version] = entry
    listing = dict(LISTING_INFO)
    listing["packages"] = {name: {"versions": dict(sorted(versions.items()))}}

    verb = "wrote" if args.write else "would write"
    print(f"{name} {version}")
    print(f"  {'new version' if was is None else 'updated in place' if was != entry else 'unchanged'}")
    print(f"  {len(versions)} version(s) in the listing")
    print(f"  zip: {zip_path.relative_to(ROOT)}" + ("" if digest else "  (not built -- pass --write)"))
    if digest:
        print(f"  sha256: {digest}")

    if args.write:
        LISTING.write_text(json.dumps(listing, indent=2) + "\n", encoding="utf-8")
    print(f"\n{verb} {LISTING.relative_to(ROOT)}")
    if args.write:
        print(f"Attach {zip_path.name} to a GitHub release tagged v{version} so the URL resolves.")
    else:
        print("re-run with --write to apply")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
