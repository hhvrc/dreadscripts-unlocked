# VPM listing

A VRChat Package Manager listing, so the restored packages can be installed and updated through the
Creator Companion instead of by copying files into `Assets/`.

## Adding it

In VCC: **Settings → Packages → Add Repository**, and paste

```
https://hhvrc.github.io/dreadscripts-unlocked/index.json
```

The packages then appear in the "Manage Project" list for any Avatars project on Unity 2022.3:

| Package | Depends on |
|---|---|
| `com.dreadscripts.unlocked.controllereditor` | `common`, `com.vrchat.avatars` 3.x |
| `com.dreadscripts.unlocked.adoverhaul` | `common`, `com.vrchat.avatars` 3.x |
| `com.dreadscripts.unlocked.common` | `com.vrchat.avatars` 3.x |

VCC resolves dependencies itself. `common` is never installed on purpose; it is the code the two
tools shared, and comes in with either one.

Up to 0.1.0 both tools were one package, `com.dreadscripts.unlocked`. It is no longer built, but
0.1.0 stays in the listing, since projects that installed it still resolve against it. Both tools
name it in `legacyPackages`, so installing either one makes VCC remove it; otherwise the old copy and
the new one would both compile and declare every type twice.

## Publishing a version

Releases are automated. Nothing here is run by hand.

**To release, add a change file and push to `main`.** [`release.yml`](../.github/workflows/release.yml)
runs OpenShock's [release-tool](https://github.com/OpenShock/release-tool) on every push. With no
change files waiting in [`.changes/`](../.changes/README.md) it does nothing. With some, it computes
the next version from them, writes it into all three `package.json` files and `CHANGELOG.md`, commits,
tags `v<version>`, stages a draft GitHub release with the notes, and hands the tag to publish.yml.

```bash
release-tool new "Fix the collider gizmo drawing at the wrong scale" --kind fixed
```

[`publish.yml`](../.github/workflows/publish.yml) runs on every `v*` tag, including one pushed by hand
(its `package.json` files must already carry the version). It builds the three zips at the tag with
`build_listing.py --version`, attaches them to the release and publishes it, adds the version to
[`index.json`](index.json) on `main`, and redeploys the listing. The zips are attached before the
listing names them, so VCC never sees an entry whose download is missing.

All three packages share one version and one tag, because release-tool keeps one version per
repository. A tool therefore always depends on exactly the `common` it was released with: the
dependency is pinned to the release version in the zipped `package.json` and in the listing,
whatever range the committed manifest carries. `com.dreadscripts.unlocked` 0.1.0 predates the
split and stays listed under the `v0.1.0` release.

`index.json` is generated, not written. Each package's `package.json` is the only place it describes
itself; the listing repeats every one of those fields per version, so maintaining it by hand means
two copies of the same facts and a mismatch that surfaces inside someone else's project rather than
here. `python3 vpm/build_listing.py` previews what a build would produce.

Two properties worth knowing about the generator:

- **The listing is append-only.** Versions already published are preserved rather than regenerated,
  URL included. VCC resolves an installed project against the versions it has seen, so removing one
  breaks that project rather than tidying the file. For the same reason the generator exits non-zero
  if a published version's contents changed: the listed hash is what VCC checks the download
  against, so that version needs a new release instead.
- **The archive is deterministic.** Members are written in sorted order with fixed timestamps, so an
  unchanged package produces an unchanged zip and an unchanged hash, on any machine.

## Serving it

The `url` field must be the address the JSON is actually served from — VCC re-fetches it to check
for updates, and a listing that points somewhere else silently stops updating. The value above
assumes GitHub Pages on this repository. If Pages is served from a branch or a `docs/` folder rather
than the root, `index.json` has to be reachable at that exact URL, and `LISTING_URL` in
`build_listing.py` is the single line to change.
