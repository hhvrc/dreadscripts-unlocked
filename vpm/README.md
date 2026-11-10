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

[`index.json`](index.json) is generated, not written. Each package's `package.json` is the only
place it describes itself; the listing repeats every one of those fields per version, so maintaining
it by hand means two copies of the same facts and a mismatch that surfaces inside someone else's
project rather than here.

Each package is versioned and released on its own, under a tag of its own:

| Package | Release tag |
|---|---|
| `com.dreadscripts.unlocked.controllereditor` | `controllereditor-v<version>` |
| `com.dreadscripts.unlocked.adoverhaul` | `adoverhaul-v<version>` |
| `com.dreadscripts.unlocked.common` | `common-v<version>` |

To release, bump `version` in the packages that changed (and the dependency ranges on them, if a
change needs a newer `common`), then:

```bash
python3 vpm/build_listing.py            # report what would change
python3 vpm/build_listing.py --write    # build the zips, hash them, rewrite index.json
```

Create each new version's release under its tag and attach `vpm/dist/<name>-<version>.zip`, which is
where that version's `url` in the listing points. Do this before pushing `index.json`: the push
publishes the listing, and an entry whose zip is not there yet fails to install. The zips are release
artifacts and are not committed. `com.dreadscripts.unlocked` 0.1.0 predates the split and stays at
the `v0.1.0` tag it was released under.

Two properties worth knowing about the generator:

- **The listing is append-only.** Versions already published are preserved rather than regenerated,
  URL included. VCC resolves an installed project against the versions it has seen, so removing one
  breaks that project rather than tidying the file. For the same reason the generator exits non-zero
  if a published version's contents changed: the listed hash is what VCC checks the download
  against, so that version needs a bump instead.
- **The archive is deterministic.** Members are written in sorted order, so an unchanged package
  produces an unchanged zip and an unchanged hash. Otherwise the listing looks stale every time it
  is rebuilt.

## Serving it

The `url` field must be the address the JSON is actually served from — VCC re-fetches it to check
for updates, and a listing that points somewhere else silently stops updating. The value above
assumes GitHub Pages on this repository. If Pages is served from a branch or a `docs/` folder rather
than the root, `index.json` has to be reachable at that exact URL, and `LISTING_URL` in
`build_listing.py` is the single line to change.
