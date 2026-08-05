# DreadScripts Unlocked

Two Unity Editor tools for VRChat avatar work — **Controller Editor** and **Avatar Dynamics
Overhaul** — restored and open sourced after they were discontinued.

They were paid tools that validated a licence against a server on every project load. That server
was shut down permanently, which left everyone who had bought them holding software that refuses to
run. This is those tools rebuilt from the shipped assemblies, with the licence check gone.

The originals are the work of their author, who has asked not to be named here. This is a
community restoration and is not affiliated with or endorsed by them.

## Install

In the VRChat Creator Companion: **Settings → Packages → Add Repository**, and paste

```
https://hhvrc.github.io/dreadscripts-unlocked/index.json
```

The package then appears in Manage Project for any Avatars project on Unity 2022.3. It needs
`com.vrchat.avatars` 3.x, which VCC resolves for you.

## What they do

**Controller Editor** replaces most of the friction in working with animator controllers. A layer
list with category and compact views, tagging and alphabetical sorting; multi-editing of transition
conditions and parameter drivers across a selection; copy and paste of layers, states, transitions
and conditions between controllers; a quick-toggle builder; state styling; drag-and-drop of motions
into blend trees; and a set of animation-window improvements including shader property names for
renderers.

**Avatar Dynamics Overhaul** rebuilds the PhysBone and collider inspectors. Scene-view editing of
radius, endpoints and limits with real handles instead of number fields, curve editing in the scene,
multi-object apply across a selection, collider and ignore-transform picking by clicking in the
scene, and a test mode that runs the dynamics without entering play mode.

Both shipped as separate products and shared most of their code, so they are one package here.

## What is different from the originals

**No licence check, and no network access.** The activation, verification and transfer flow is gone,
along with the hardware fingerprinting that fed it — the tools no longer spawn `wmic` to read your
disk and board serials, and they send nothing anywhere.

Also removed, because each existed only to talk to a server that is gone: the update check, the bug
reporter, and the supporter window.
[`reverse-engineering/vendor-backend/EXCLUDED.md`](reverse-engineering/vendor-backend/EXCLUDED.md)
is the complete list, with the reason for each.

Everything else is the tools as they shipped, including their bugs. Where the original had a defect,
the port reproduces it and says so in a comment rather than quietly fixing it, so that the
restoration stays a restoration.

## Running the original builds instead

If you would rather run the assemblies you bought, [`drm_server/`](drm_server/) is a local server
that answers the dead validation endpoint. It needs a hosts-file entry and a self-signed
certificate; see [`drm_server/README.md`](drm_server/README.md).

You do not need it for the package above.

## How this was made

Almost all of this was written by Claude, with me directing it, making the calls and pulling it back
when it went wrong. The tooling is the part worth explaining, because building that was most of the
work — the porting at the end was the easy bit.

**First the assemblies had to be made readable at all.** They were commercially obfuscated: no
usable names, strings encrypted at runtime, control flow flattened into state machines that decompile
into unreadable dispatch loops. Getting from that to something a person could reason about took
several rounds of building a thing, finding where it fell short on the real binaries, and rebuilding
it — string decryption driven from the transform constants recovered per method, control-flow
tracing for the dispatch loops the decompiler could not resolve, and a way to record what a member
*is* once you work it out.

That last piece shaped everything after it. Naming is applied to the **assembly metadata** before
decompilation rather than edited into the decompiled text, so one decision updates the declaration,
every call site and the filename together — and the decompiled source stays a regenerable artefact
instead of something hand-edited and therefore impossible to reproduce.

Names come from evidence, not shape. `ReflectVisitor` reflects nothing and visits nothing; it draws
the transition section, which you can only establish by reading what it touches and who calls it.
Roughly 1,800 members were named this way.

**More tooling followed as the gaps showed up**, and the pattern that kept proving right was to
derive facts rather than record them:

- Port status is computed by diffing the decompiled tree against the package, so it cannot drift the
  way a hand-kept checklist does.
- Every ported file carries a machine-checked header naming the decompiled members it is responsible
  for. A checker confirms each claimed member exists and that no two files claim the same one —
  which caught members ported twice under different names, something the C# compiler cannot see.
- Names decided while porting but recorded only in a file header get harvested back into the maps.
  That gap had swallowed about a third of the naming work before anyone noticed.

**Only then did the actual porting happen**, and by that point I could have Claude run it as
multi-agent workflows — dispatching a batch of sub-agents per wave, each with a disjoint set of
files, the decompiled source as ground truth, and an instruction to report rather than guess, then
checking the results itself before anything landed. Disjoint sets are what makes that safe: two
agents never edit the same file, and shared state stays single-owner. When one needed something
outside its set it said so instead of reaching for it — which is how a `private` member that three
files needed got widened once, deliberately, instead of three times by accident.

Everything was then audited file by file against the source it claims to come from. That pass is
where the interesting failures were: headers asserting things the source contradicts, notes naming
blockers that had been resolved long before, deviations that existed but were written down nowhere.

The result is that every line here is traceable — what it came from, what changed, and why. Where the
port deviates from the original, a decompiler artefact corrected or a shipped bug reproduced, the
file says so at the top.

[`reverse-engineering/`](reverse-engineering/README.md) has all of it: the pipeline, the maps, the
notes, the open questions, and the binaries themselves, so none of the above has to be taken on
trust.

## What's next

**The reverse engineering is finished, and the handoff is done.** Both products are fully ported —
every file that was going to be restored has been, each one checked against the source it came from.
What was a reverse-engineering project is now just a Unity tool, and everything from here is
maintenance and improvement: bug reports, Unity and SDK compatibility, and features on top of what
the originals did.

That changes what a contribution looks like. There is no longer a queue of things to decompile; the
useful work is the ordinary kind — fixing something that misbehaves, keeping it building against new
Unity and VRChat SDK versions, and improving tools that are no longer maintained upstream.

A few known remnants are recorded rather than hidden: a small number of members deliberately left
unported with the reason stated, and shipped bugs reproduced on purpose so the restoration stays
faithful. Both are marked in the files themselves, so anything you find is either documented or a
genuine bug worth reporting.

## Layout

```
unity/                    the restored package
vpm/                      the VPM listing VCC installs from
drm_server/               local stand-in for the shut-down validation endpoint
reverse-engineering/      how all of the above was produced
```

## Legal and ethical context

These tools were sold, then discontinued, and the server they validated against was shut down
permanently — which left every copy unable to run, including paid ones. This restores them. The
products are no longer sold and the licensing is no longer enforced by anyone, because there is
nothing left running to enforce it.

The restored package is derived work and this repository claims no licence over it; see the
`NOTICE` beside the package and the table below.

## Licences

Not one licence, because not all of this is the same kind of thing. Each directory below carries the
licence that applies to it.

| Path | Licence | |
|---|---|---|
| `unity/Assets/com.dreadscripts.unlocked/` | **not ours to license** | The restored package — derived from the original author's work |
| `drm_server/` | MIT | |
| `vpm/` | MIT | |
| `reverse-engineering/tooling/` | **GPL-3.0** | The pipeline |
| `reverse-engineering/tools/` | **GPL-3.0** | `IlRename`, `SourceAnalysis`, the checkers |
| `reverse-engineering/binaries/` | **not ours to license** | The vendor's compiled products, included so the analysis can be checked |
| `reverse-engineering/export/` | **not ours to license** | Decompiler output — a build artefact regenerated from the binaries |
| everything else | MIT | Notes, maps, documentation |

The pipeline is GPL-3.0 and is wholly this project's work, so it is ours to license that way.

The restored package is not. It reconstructs the original author's code deliberately faithfully —
reproducing its bugs on purpose, because that is what a restoration is — so the expression in it
stays theirs. Claiming MIT over it would be granting something we do not hold. The same is true of
`binaries/`, which is the author's own build, and `export/`, which is that build mechanically
transformed. All three carry a `NOTICE` saying so plainly rather than a licence implying otherwise.

What this project *did* contribute to the package — the naming, the structure, the provenance
headers, the corrections — is recorded per file, so it is visible without being overstated.
