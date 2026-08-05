# VPM listing

A VRChat Package Manager listing, so the restored package can be installed and updated through the
Creator Companion instead of by copying files into `Assets/`.

## Adding it

In VCC: **Settings → Packages → Add Repository**, and paste

```
https://hhvrc.github.io/dreadscripts-unlocked/index.json
```

The package then appears in the "Manage Project" list for any Avatars project on Unity 2022.3.
It depends on `com.vrchat.avatars` 3.x, which VCC resolves itself.

## Publishing a version

[`index.json`](index.json) is generated, not written. `package.json` inside the package is the only
place the package describes itself; the listing repeats every one of those fields per version, so
maintaining it by hand means two copies of the same facts and a mismatch that surfaces inside
someone else's project rather than here.

```bash
python3 vpm/build_listing.py            # report what would change
python3 vpm/build_listing.py --write    # build the zip, hash it, rewrite index.json
```

Then attach `vpm/dist/<name>-<version>.zip` to a GitHub release tagged `v<version>`, which is where
the `url` in the listing points. The zip is a release artifact and is not committed.

Two properties worth knowing about the generator:

- **The listing is append-only.** Versions already published are preserved rather than regenerated.
  VCC resolves an installed project against the versions it has seen, so removing one breaks that
  project rather than tidying the file.
- **The archive is deterministic.** Members are written in sorted order, so an unchanged package
  produces an unchanged zip and an unchanged hash. Otherwise the listing looks stale every time it
  is rebuilt.

## Serving it

The `url` field must be the address the JSON is actually served from — VCC re-fetches it to check
for updates, and a listing that points somewhere else silently stops updating. The value above
assumes GitHub Pages on this repository. If Pages is served from a branch or a `docs/` folder rather
than the root, `index.json` has to be reachable at that exact URL, and `LISTING_URL` in
`build_listing.py` is the single line to change.
