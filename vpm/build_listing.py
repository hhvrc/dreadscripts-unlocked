#!/usr/bin/env python3
"""
Build the VPM listing, and the package zips a release serves.

Why this exists
---------------
A VPM listing repeats every field of each package manifest, once per published version. Written by
hand that is a second copy of facts the `package.json` files already own, and it drifts the first
time a version is bumped in one place and not the other -- with the failure showing up as a silent
mismatch inside someone else's Unity project rather than as an error here.

So the listing is generated. Each package's `package.json` is the only place that package describes
itself; this reads them, zips each package, hashes the zip, and writes `index.json`. Published
versions already in the listing are preserved, because a listing is append-only from the consumer's
point of view: VCC resolves an installed project against versions it has seen, and deleting one
breaks that project.

Packages
--------
Every `unity/Assets/com.dreadscripts.unlocked*/package.json` is a package:

    com.dreadscripts.unlocked.common            shared editor code, a dependency of both tools
    com.dreadscripts.unlocked.controllereditor  Controller Editor
    com.dreadscripts.unlocked.adoverhaul        Avatar Dynamics Overhaul

Each is versioned on its own and released under its own tag, `<short name>-v<version>` (for example
`controllereditor-v0.2.0`), so releasing one tool does not bump the others.

Up to 0.1.0 both tools were one package, `com.dreadscripts.unlocked`. It is no longer built, but its
published version stays in the listing as it was released (tag `v0.1.0`, URL and hash included),
because projects that installed it still resolve against it. Both tools name it in
`legacyPackages`, so VCC removes it when either is installed.

    python3 vpm/build_listing.py                 # report what would change
    python3 vpm/build_listing.py --write         # write index.json and the zips

The zips are written to `vpm/dist/` and are NOT committed -- they are release artifacts. Attach each
to the GitHub release its listing URL names, so the URL resolves.
"""

import argparse
import hashlib
import json
import sys
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
ASSETS = ROOT / "unity" / "Assets"
PACKAGE_GLOB = "com.dreadscripts.unlocked*"
PREFIX = "com.dreadscripts.unlocked."
LISTING = ROOT / "vpm" / "index.json"
DIST = ROOT / "vpm" / "dist"

# Where the listing is served from, and where release artifacts live. Both are GitHub-derived; if
# the repository ever moves, REPO is the only line that needs to change.
REPO = "hhvrc/dreadscripts-unlocked"
LISTING_URL = f"https://{REPO.split('/')[0]}.github.io/{REPO.split('/')[1]}/index.json"
RELEASE_URL = f"https://github.com/{REPO}/releases/download/{{tag}}/{{name}}-{{version}}.zip"

LISTING_INFO = {
    "name": "DreadScripts Unlocked",
    "id": "com.hhvrc.dreadscripts-unlocked",
    "author": "dreadscripts-unlocked contributors",
    "url": LISTING_URL,
    "description": "Community restoration of two discontinued DreadScripts Unity Editor tools.",
}


def tag_for(name: str, version: str) -> str:
    """The release tag a package version is published under."""
    return f"{name[len(PREFIX):]}-v{version}"


def find_packages() -> list[tuple[Path, dict]]:
    found = []
    for manifest_path in sorted(ASSETS.glob(f"{PACKAGE_GLOB}/package.json")):
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        if manifest.get("name") != manifest_path.parent.name:
            sys.exit(f"error: {manifest_path} names itself {manifest.get('name')!r}, but its folder "
                     f"is {manifest_path.parent.name!r}; VCC installs it under the manifest name")
        found.append((manifest_path.parent, manifest))
    if not found:
        sys.exit(f"error: no package manifests under {ASSETS}/{PACKAGE_GLOB}")
    return found


def check_dependencies(packages: list[tuple[Path, dict]]) -> None:
    """Every dependency on one of our own packages must name one that exists here."""
    ours = {m["name"]: m["version"] for _, m in packages}
    for _, m in packages:
        for dep in m.get("vpmDependencies", {}):
            if dep.startswith(PREFIX) and dep not in ours:
                sys.exit(f"error: {m['name']} depends on {dep}, which is not one of the packages here")


def build_zip(package_dir: Path, manifest: dict, write: bool) -> tuple[Path, str]:
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
            for f in sorted(package_dir.rglob("*")):
                if f.is_file():
                    z.write(f, f.relative_to(package_dir).as_posix())
    if not out.exists():
        return out, ""
    return out, hashlib.sha256(out.read_bytes()).hexdigest()


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__.split("\n")[1],
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--write", action="store_true", help="write index.json and build the zips")
    args = ap.parse_args()

    packages = find_packages()
    check_dependencies(packages)

    old = json.loads(LISTING.read_text(encoding="utf-8")) if LISTING.is_file() else {}
    old_packages = old.get("packages", {})
    listing = dict(LISTING_INFO)
    listing["packages"] = {}

    # Packages that are in the listing but no longer in the tree keep their published versions.
    for name, entry in old_packages.items():
        listing["packages"][name] = {"versions": dict(entry.get("versions", {}))}

    republished = []
    for package_dir, manifest in packages:
        name, version = manifest["name"], manifest["version"]
        versions = listing["packages"].setdefault(name, {"versions": {}})["versions"]
        zip_path, digest = build_zip(package_dir, manifest, args.write)
        was = versions.get(version)

        entry = dict(manifest)
        # An already-published version keeps the URL it was published under: a listing entry whose
        # URL moved no longer installs.
        entry["url"] = was["url"] if was and "url" in was else RELEASE_URL.format(
            tag=tag_for(name, version), name=name, version=version)
        if digest:
            entry["zipSHA256"] = digest
        elif was and "zipSHA256" in was:
            # Keep the hash of an already-published version rather than dropping it when the zip is
            # not present locally -- a listing entry without one cannot be installed.
            entry["zipSHA256"] = was["zipSHA256"]

        if was and was.get("zipSHA256") and digest and was["zipSHA256"] != digest:
            republished.append(f"{name} {version}")
        versions[version] = entry

        state = "new version" if was is None else "updated in place" if was != entry else "unchanged"
        print(f"{name} {version}: {state}, {len(versions)} version(s) listed")
        print(f"  release tag: {tag_for(name, version) if was is None or 'url' not in was else '(as published)'}")
        print(f"  zip: {zip_path.relative_to(ROOT)}" + ("" if digest else "  (not built -- pass --write)"))
        if digest:
            print(f"  sha256: {digest}")

    for entry in listing["packages"].values():
        entry["versions"] = dict(sorted(entry["versions"].items()))
    listing["packages"] = dict(sorted(listing["packages"].items()))

    if republished:
        # The listed hash is what VCC checks the download against. Changing what an
        # already-published version contains breaks every project that installs it.
        print("\nwarning: these versions are already published and their contents changed; bump the "
              "version in package.json instead:", file=sys.stderr)
        for r in republished:
            print(f"  {r}", file=sys.stderr)

    verb = "wrote" if args.write else "would write"
    if args.write:
        LISTING.write_text(json.dumps(listing, indent=2) + "\n", encoding="utf-8")
    print(f"\n{verb} {LISTING.relative_to(ROOT)}")
    if args.write:
        print("Attach each zip to the GitHub release named by its release tag, so the URLs resolve.")
    else:
        print("re-run with --write to apply")
    return 1 if republished else 0


if __name__ == "__main__":
    raise SystemExit(main())
