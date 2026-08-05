#!/usr/bin/env python3
"""
Per-type naming coverage, read from renames/*.json alone.

Why this exists
---------------
`ilrename report` already reports coverage, but it needs the deobfuscated assembly, and that only
exists inside a `reexport.py` run (work/_deobf_tmp, deleted on the way out). So the one question that
starts most sessions -- "which types still have unnamed members, and which are worth doing next" --
could not be answered without either a full re-export or a manual de4dot run.

The maps are enough to answer it: every entity has a key, and an empty value means "not named yet".
This reads that directly, groups members under their declaring type, and orders the result so the
work queue is obvious. It reports what is *unnamed*, never what a name should be.

Members that already carry their real name -- `Dispose`, `op_Implicit`, `.ctor`, Unity messages the
engine calls by reflection -- are counted separately rather than sitting in the queue, since renaming
them would destroy information. See KEEPS_ITS_NAME. That detection is deliberately conservative:
vendor fields that survived obfuscation still read as unnamed, because telling `defaultState` from an
obfuscator's invention needs someone to read the export, and guessing wrong in the quiet direction
would hide real work.

The remaining caveat: a type with nothing left to name is not "done" in any deeper sense. A name can
be present and wrong -- three were, and had to be corrected with `apply_renames.py set --force`. Use
this to find work, not to certify that work is finished.

    python3 scripts/rename_status.py                    # all samples, summary + per-type table
    python3 scripts/rename_status.py --sample ADOverhaul2022
    python3 scripts/rename_status.py --smallest 20      # quickest wins first
    python3 scripts/rename_status.py --type GUIColorScope
"""

import argparse
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from refresh_renames import EXPORT, RENAMES, strip_comments, token_of  # noqa: E402

import json  # noqa: E402

# "Namespace.Type::member#0xTOKEN" -- the "::" is what separates a member from its declaring type.
MEMBER_KEY = re.compile(r"^(?P<owner>.+?)::(?P<member>[^:#]+)#")

# Members that already carry their real name, so an empty value means "nothing to do" rather than
# "still to do". The maps were regenerated with --all-members on 2026-07-31, which stopped filtering
# on whether a name looks obfuscated -- necessary, because that filter was also hiding genuinely
# obfuscated members nobody could then name, but it means the unnamed count now includes members
# whose names were never obfuscated in the first place. Renaming any of these would destroy
# information, so they are counted apart rather than sitting in the queue forever.
#
# This is deliberately a list of things we can be *certain* about: interface and object members,
# operators, constructors, compiler-generated names, and the Unity message names the engine calls by
# reflection. Vendor fields that survived obfuscation (`defaultState`, `u_updateLink`,
# `a_VerifyOnDisplay`) are NOT detectable this way and still show as unnamed -- deciding those needs
# a human reading the export, which is the right default for an ambiguous case.
KEEPS_ITS_NAME = {
    # System.Object / IDisposable / common interfaces
    "Dispose", "ToString", "Equals", "GetHashCode", "Finalize", "Clone", "CompareTo",
    "GetEnumerator", "MoveNext", "Current", "GetObjectData", "Deconstruct",
    # Unity messages, invoked by the engine rather than by name in code
    "Awake", "Start", "Update", "LateUpdate", "FixedUpdate", "OnEnable", "OnDisable",
    "OnDestroy", "OnGUI", "OnInspectorGUI", "OnSceneGUI", "OnPreviewGUI", "OnValidate",
    "OnFocus", "OnLostFocus", "OnSelectionChange", "OnHierarchyChange", "OnProjectChange",
    "CreateInspectorGUI", "HasPreviewGUI", "GetPreviewTitle", "RequiresConstantRepaint",
    "OnActivate", "OnDeactivate", "OnHeaderGUI", "AddItemsToMenu",
    # UnityEditor.IMGUI.Controls.TreeView overrides
    "BuildRoot", "RowGUI", "CanChangeExpandedState", "DoubleClickedItem", "ContextClickedItem",
    "SelectionChanged", "CanStartDrag", "SetupDragAndDrop", "HandleDragAndDrop",
    "BuildRows", "GetAncestors", "GetDescendantsThatHaveChildren",
}


def keeps_its_name(member: str) -> bool:
    """True when a member's existing name is real and must not be replaced."""
    return (member in KEEPS_ITS_NAME
            or member.startswith("op_")          # operators: op_Implicit, op_Equality, ...
            or member.startswith(".")            # .ctor / .cctor
            or member.startswith("<")            # compiler-generated backing members
            or "." in member)                    # explicit interface implementation


def entries(path: Path) -> dict:
    return json.loads(strip_comments(path.read_text(encoding="utf-8")))


def owner_of(key: str) -> str | None:
    """The declaring type of a member key, or None when the key is itself a type."""
    m = MEMBER_KEY.match(key)
    return m.group("owner") if m else None


def type_names(m: dict) -> dict:
    """Type key, member suffix stripped -> the name assigned to it, for every named type in a map."""
    return {k.rsplit("#", 1)[0]: v for k, v in m.items() if v and owner_of(k) is None}


def export_file(sample: str, type_key: str, name_of: dict) -> Path | None:
    """
    The export/ file a type's source lives in, or None if it is not uniquely there.

    Two things stop this being a matter of taking the key apart. ilrename renames before ILSpy
    runs, so the file is named after the *assigned* name rather than the obfuscated one in the key;
    and a nested type has no file of its own, it is declared inside its outermost enclosing type's,
    so resolution has to walk out to that type first. Most of ControllerEditor is nested, so without
    the walk-out the source pointer comes back blank exactly where it is most needed.

    `name_of` maps an outer type key to its assigned name -- build it with `type_names`.

    This lives here, next to `owner_of` and the rest of the rename-map vocabulary, because it was
    previously implemented once in apply_renames.py and once in mirror_renames.py, with two
    docstrings explaining the same nested-type reasoning and two ways of doing the lookup.
    """
    outer_key = type_key.split("/", 1)[0]
    simple = name_of.get(outer_key) or outer_key.rsplit(".", 1)[-1]
    root = EXPORT / sample
    if not root.is_dir():
        return None
    hits = list(root.rglob(f"{simple}.cs"))
    return hits[0] if len(hits) == 1 else None


def collect(path: Path):
    """-> {type_name: {"named": str|None, "members": [(member_key, value)]}}"""
    types: dict[str, dict] = {}
    for key, value in entries(path).items():
        owner = owner_of(key)
        if owner is None:
            name = key.rsplit("#", 1)[0]
            types.setdefault(name, {"named": None, "members": []})["named"] = value or None
        else:
            types.setdefault(owner, {"named": None, "members": []})["members"].append((key, value))
    return types


def main() -> int:
    ap = argparse.ArgumentParser(
        description="Per-type naming coverage, read from renames/*.json alone.")
    ap.add_argument("--sample", action="append",
                    help="sample stem (repeatable; default: all)")
    ap.add_argument("--type", dest="type_filter",
                    help="only types whose key or assigned name contains this substring")
    ap.add_argument("--smallest", type=int, metavar="N",
                    help="show the N types with the fewest unnamed members (quickest wins)")
    ap.add_argument("--all", action="store_true",
                    help="include types that have no unnamed members")
    ap.add_argument("-v", "--verbose", action="store_true",
                    help="list every unnamed member, not just the counts")
    args = ap.parse_args()

    stems = args.sample or sorted(p.stem for p in RENAMES.glob("*.json"))
    exit_code = 0

    for stem in stems:
        path = RENAMES / f"{stem}.json"
        if not path.exists():
            print(f"{stem}: no map at {path.relative_to(RENAMES.parent)}")
            exit_code = 1
            continue

        types = collect(path)
        rows = []
        for tname, info in types.items():
            unnamed = [k for k, v in info["members"]
                       if not v and not keeps_its_name(MEMBER_KEY.match(k).group("member"))]
            if not unnamed and not args.all:
                continue
            if args.type_filter:
                hay = f"{tname} {info['named'] or ''}"
                if args.type_filter.lower() not in hay.lower():
                    continue
            rows.append((tname, info, unnamed))

        total_members = sum(len(i["members"]) for i in types.values())
        total_unnamed = sum(1 for i in types.values() for k, v in i["members"]
                            if not v and not keeps_its_name(MEMBER_KEY.match(k).group("member")))
        total_keep = sum(1 for i in types.values() for k, v in i["members"]
                         if not v and keeps_its_name(MEMBER_KEY.match(k).group("member")))
        named_types = sum(1 for i in types.values() if i["named"])

        print(f"\n===== {stem} =====")
        nameable = total_members - total_keep
        print(f"types {named_types}/{len(types)} named   "
              f"members {nameable - total_unnamed}/{nameable} named   "
              f"({total_unnamed} to go, {total_keep} already carry their real name)")

        rows.sort(key=lambda r: (len(r[2]), r[0]))
        if args.smallest:
            rows = rows[:args.smallest]
        else:
            rows.reverse()  # biggest first, so the bulk of the work is at the top

        if not rows:
            print("  nothing unnamed" if not args.all else "  no types")
            continue

        width = max(len(r[1]["named"] or r[0].rsplit(".", 1)[-1]) for r in rows)
        for tname, info, unnamed in rows:
            label = info["named"] or tname.rsplit(".", 1)[-1]
            mark = " " if info["named"] else "*"
            print(f"  {mark}{label:<{width}}  {len(unnamed):>4} unnamed "
                  f"of {len(info['members']):>4}")
            if args.verbose:
                for key in unnamed:
                    member = MEMBER_KEY.match(key).group("member")
                    print(f"        {token_of(key)}  {member}")
        print("  (* = the type itself is not named either)")

    return exit_code


if __name__ == "__main__":
    raise SystemExit(main())
