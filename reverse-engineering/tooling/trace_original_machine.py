#!/usr/bin/env python3
"""
Emit an inspectable execution trace of a dispatch machine, read from the ORIGINAL binary.

Why this exists
---------------
Verifying that a de4dot change kept a method faithful means deriving what the ORIGINAL does and
comparing. For a handful of assignments that is a five-minute hand decode. For a method that is a
long linear sequence of registration calls it is not: the same reconstruction — pull the affine
constants, iterate `(s*mul) ^ xor ^ K % M`, map each index to a block — has to be repeated across
dozens of states, and the failure mode is one call silently dropped or reordered in the middle of
many, which is exactly what a hand decode misses.

**This tool does not judge fidelity.** It emits evidence: states, selected cases, and the ordered
payload operations. Comparing that against the new export stays a human judgement, deliberately —
a tool that both derives the truth and grades the output can only ever agree with itself.

Read-only: it parses ILAsm text obtained through pipeline.py and never writes or runs anything.

    python3 scripts/trace_original_machine.py --sample ControllerEditor --type ControllerEditor \\
        --method AssetAlgo
"""

import argparse
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import pipeline as pl  # noqa: E402

UNKNOWN = "Unknown"
MASK = 0xFFFFFFFF

INSTR = re.compile(r"^\s*(IL_[0-9A-Fa-f]{4}):\s+([\w.]+)\s*(.*?)\s*$")


def u32(v):
    return v & MASK if isinstance(v, int) else UNKNOWN


def method_il(sample: str, type_name: str, method: str) -> list[tuple[str, str, str]]:
    """
    (label, opcode, operand) for one method, from the original binary.

    The name has to be matched inside a `.method` *declaration*, not anywhere it appears. Matching
    it anywhere finds call sites too -- asking for `Button` matched the `GUILayout.Button(` inside
    some unrelated body, and the slice then started mid-method, so branch targets pointed at labels
    outside it and the block builder died with `KeyError: 'IL_002a'` instead of saying the method
    was not found. ilspycmd splits a declaration across lines and the name is never on the `.method`
    line itself, so each declaration is taken as the text from `.method` up to its first IL label:
    that header holds the signature and nothing else.
    """
    text = pl.decompile_type(pl.sample(sample), type_name, il=True)
    name_in_header = re.compile(r"\b" + re.escape(method) + r"\s*\(")

    start, seen = None, []
    for decl in re.finditer(r"^\s*\.method\b", text, re.M):
        first_label = re.search(r"^\s*IL_[0-9A-Fa-f]{4}:", text[decl.start():], re.M)
        header_end = decl.start() + (first_label.start() if first_label else 400)
        header = text[decl.start():header_end]
        names = re.findall(r"\b([A-Za-z_<][\w`<>.]*)\s*\(", header)
        seen.extend(names)
        if name_in_header.search(header):
            start = decl.start()
            break

    if start is None:
        known = ", ".join(sorted(set(seen))[:20]) or "(none parsed)"
        sys.exit(f"error: {type_name}::{method} is not a method of {type_name} in the original "
                 f"{sample}.\n"
                 f"  Note this reads binaries/{sample}.dll, which is the OBFUSCATED assembly -- "
                 f"names from renames/ or export/ do not exist in it.\n"
                 f"  Methods found on this type: {known}")

    end = text.find(f"end of method", start)
    body = text[start:end if end > 0 else start + 200000]
    out = []
    for line in body.splitlines():
        m = INSTR.match(line)
        if m:
            out.append((m.group(1), m.group(2), m.group(3)))
    if not out:
        sys.exit(f"error: no instructions parsed for {type_name}::{method}")
    return out


def build_blocks(instrs):
    """Split into blocks at branch targets and after branches. Blocks get positional ids."""
    labels = {lbl: i for i, (lbl, _, _) in enumerate(instrs)}
    leaders = {0}
    for i, (_, op, arg) in enumerate(instrs):
        if op == "switch":
            for t in re.findall(r"IL_[0-9A-Fa-f]{4}", arg):
                leaders.add(labels[t])
            if i + 1 < len(instrs):
                leaders.add(i + 1)
        elif op.startswith("br") or op in ("leave", "leave.s"):
            t = re.search(r"IL_[0-9A-Fa-f]{4}", arg)
            if t:
                leaders.add(labels[t.group(0)])
            if i + 1 < len(instrs):
                leaders.add(i + 1)
        elif op in ("ret", "throw", "rethrow") and i + 1 < len(instrs):
            leaders.add(i + 1)
    starts = sorted(leaders)
    blocks, at = {}, {}
    for n, s in enumerate(starts):
        e = starts[n + 1] if n + 1 < len(starts) else len(instrs)
        blocks[n] = instrs[s:e]
        at[instrs[s][0]] = n           # label -> positional block id
    return blocks, at


def arg_count(sig: str) -> int:
    """Parameter count of a call signature, plus one for an instance receiver."""
    depth, inner, started = 0, "", False
    for ch in sig:
        if ch == "(":
            depth += 1
            if depth == 1:
                started = True
                continue
        elif ch == ")":
            depth -= 1
            if depth == 0:
                break
        if started:
            inner += ch
    n = 0
    if inner.strip():
        depth = 0
        n = 1
        for ch in inner:
            if ch in "(<[":
                depth += 1
            elif ch in ")>]":
                depth -= 1
            elif ch == "," and depth == 0:
                n += 1
    return n + (1 if sig.strip().startswith("instance") else 0)


BIN = {"mul": lambda a, b: a * b, "add": lambda a, b: a + b, "sub": lambda a, b: a - b,
       "xor": lambda a, b: a ^ b, "and": lambda a, b: a & b, "or": lambda a, b: a | b}


def trace(blocks, at, max_steps, max_events, policy="stop"):
    """
    Walk the machine, returning {path id: [events]}.

    On an opaque predicate -- a conditional whose value is not in the IL -- `stop` halts, `zero` and
    `nonzero` continue under a stated assumption, and `fork` explores BOTH successors as separate
    paths. Fork is the review mode: it shows what the original could do either way, so the reviewer
    compares the export against a complete picture rather than against the one branch someone already
    believed in. The tool never picks the branch that resembles the export.
    """
    paths = {}
    # (path id, block, stack, locals). Configurations already reached by any path, so a
    # reconvergence merges instead of re-exploring -- but only when stack AND tracked state are
    # structurally identical, since equal blocks with different state are different executions.
    globally_seen = set()
    queue = [("A", 0, [], {})]
    while queue:
        pid, block0, stack0, locals0 = queue.pop(0)
        events, step = [], 0
        stack, locals_ = list(stack0), dict(locals0)
        forked = _walk(blocks, at, stack, locals_, block0, events, max_steps, max_events,
                       policy, pid, globally_seen, queue)
        paths[pid] = events
    return paths


def _config(block, stack, locals_):
    return (block, tuple(map(str, stack)), tuple(sorted((k, str(v)) for k, v in locals_.items())))


def _walk(blocks, at, stack, locals_, block, events, max_steps, max_events, policy, pid,
          globally_seen, queue):
    step = 0

    def pop():
        return stack.pop() if stack else UNKNOWN

    while step < max_steps:
        step += 1
        cfg = _config(block, stack, locals_)
        if cfg in globally_seen:
            events.append(f"       (configuration already explored -- merged into an earlier path)")
            return
        globally_seen.add(cfg)
        events.append(f"\n#{step:<3} block b{block}   entry stack={list(stack) or '[]'}")
        for lbl, op, arg in blocks[block]:
            if op.startswith("ldc.i4"):
                v = arg.strip()
                if op == "ldc.i4.M1" or op.endswith(".m1"):
                    stack.append(-1 & MASK)
                elif re.fullmatch(r"ldc\.i4\.\d", op):
                    stack.append(int(op.rsplit(".", 1)[1]))
                else:
                    stack.append(u32(int(v)) if re.fullmatch(r"-?\d+", v) else UNKNOWN)
            elif op.startswith("ldloc"):
                k = arg.strip() or op.rsplit(".", 1)[1]
                stack.append(locals_.get(k.strip(), UNKNOWN))
            elif op.startswith("stloc"):
                k = (arg.strip() or op.rsplit(".", 1)[1]).strip()
                locals_[k] = pop()
                events.append(f"       V_{k} = {locals_[k]}")
            elif op.startswith("ldarg") or op.startswith("ldarga"):
                stack.append(UNKNOWN)
            elif op == "dup":
                v = stack[-1] if stack else UNKNOWN
                stack.append(v)
            elif op == "pop":
                pop()
            elif op in BIN:
                b, a = pop(), pop()
                stack.append(u32(BIN[op](a, b)) if isinstance(a, int) and isinstance(b, int) else UNKNOWN)
            elif op == "rem.un":
                b, a = pop(), pop()
                stack.append(a % b if isinstance(a, int) and isinstance(b, int) and b else UNKNOWN)
            elif op in ("call", "callvirt", "newobj"):
                n = arg_count(arg)
                args = [pop() for _ in range(n)][::-1]
                name = re.sub(r"\(.*", "", arg).split()[-1]
                events.append(f"       CALL {name}  args={args}")
                if not re.match(r"\s*(instance\s+)?void\b", arg) or op == "newobj":
                    stack.append(UNKNOWN)
            elif op in ("ldsfld", "ldfld", "ldftn", "ldnull", "ldstr", "newarr", "ldtoken"):
                if op == "ldfld":
                    pop()
                if op == "newarr":
                    pop()
                stack.append(UNKNOWN)
                if op in ("ldsfld", "ldfld"):
                    events.append(f"       load {re.sub(r'.*::', '', arg)} -> Unknown")
            elif op in ("stsfld",):
                events.append(f"       STORE {re.sub(r'.*::', '', arg)} = {pop()}")
            elif op in ("stfld", "stelem", "stelem.ref"):
                for _ in range(3 if op.startswith("stelem") else 2):
                    pop()
                events.append(f"       STORE {re.sub(r'.*::', '', arg) or op}")
            elif op == "switch":
                idx = pop()
                targets = re.findall(r"IL_[0-9A-Fa-f]{4}", arg)
                if not isinstance(idx, int) or idx >= len(targets):
                    events.append(f"       DISPATCH index={idx} -> Unknown (stop)")
                    return
                nxt = at[targets[idx]]
                events.append(f"       DISPATCH index={idx} of {len(targets)} -> b{nxt}")
                block = nxt
                break
            elif op in ("br", "br.s", "leave", "leave.s"):
                block = at[re.search(r"IL_[0-9A-Fa-f]{4}", arg).group(0)]
                break
            elif op in ("brtrue", "brtrue.s", "brfalse", "brfalse.s"):
                cond = pop()
                if not isinstance(cond, int):
                    tgt = at[re.search(r"IL_[0-9A-Fa-f]{4}", arg).group(0)]
                    field = "opaque predicate"
                    if policy == "stop":
                        events.append(f"       BRANCH {op} on Unknown -> cannot prove (stop)")
                        return
                    if policy == "fork":
                        for suffix, taken in (("t", True), ("f", False)):
                            nxt = tgt if (taken == op.startswith("brtrue")) else block + 1
                            queue.append((f"{pid}.{suffix}", nxt, list(stack), dict(locals_)))
                        events.append(f"       BRANCH {op} on Unknown ({field}) -> FORK into"
                                      f" {pid}.t (taken) and {pid}.f (not taken)")
                        return
                    assumed_true = (policy == "nonzero")
                    taken = assumed_true == op.startswith("brtrue")
                    events.append(f"       BRANCH {op} on Unknown ({field}) -> ASSUMED"
                                  f" {policy}, taken={taken}")
                    block = tgt if taken else block + 1
                    break
                taken = (cond != 0) == op.startswith("brtrue")
                events.append(f"       BRANCH {op} cond={cond} taken={taken}")
                block = at[re.search(r"IL_[0-9A-Fa-f]{4}", arg).group(0)] if taken else block + 1
                break
            elif op in ("ret", "throw", "rethrow"):
                events.append(f"       {op.upper()}")
                return
            elif op in ("ldc.r4", "ldc.r8", "ldloca.s", "ldloca", "ldarga.s", "ldarga"):
                # Values this tracer does not model, but pushing Unknown is exactly right: the state
                # machine's dispatch never turns on a float or an address, so refusing to continue
                # only hides the states downstream of one.
                stack.append(UNKNOWN)
            elif op == "stobj":
                pop()  # value
                pop()  # destination address
            elif op in ("isinst", "castclass", "unbox.any", "box"):
                # Pure value transforms: pop one, push an unknown result. Modelling them is sound --
                # the produced value is Unknown either way, so no control-flow decision can turn on
                # it -- and stopping instead ends the trace at the first `as`/cast, which in real
                # methods is mid-machine and leaves the states after it unexamined. That is how a
                # non-terminating machine stayed classified "undecidable".
                pop()
                stack.append(UNKNOWN)
            elif op == "nop":
                pass
            else:
                events.append(f"       {op} -> Unknown (unmodelled, stop)")
                return
        else:
            block += 1                 # fell off the end of a block
        if len(events) > max_events:
            events.append("       (event cap reached, stop)")
            return
    events.append("       (step cap reached, stop)")


def main() -> int:
    p = argparse.ArgumentParser(description=__doc__,
                                formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--sample", required=True)
    p.add_argument("--type", required=True, dest="type_name")
    p.add_argument("--method", required=True)
    p.add_argument("--max-steps", type=int, default=400)
    p.add_argument("--max-events", type=int, default=2000)
    p.add_argument("--assume-opaque", choices=("stop", "zero", "nonzero", "fork"), default="stop",
                   help="what to do at a conditional whose value is not in the IL. 'fork' explores "
                        "both successors as separate paths and is the review mode.")
    p.add_argument("--calls-only", action="store_true",
                   help="print just the ordered observable calls and stores")
    a = p.parse_args()

    instrs = method_il(a.sample, a.type_name, a.method)
    blocks, at = build_blocks(instrs)
    print(f"# {a.sample} {a.type_name}::{a.method} -- {len(instrs)} instrs, {len(blocks)} blocks")
    print("# Block ids are POSITIONAL. IL offsets are not identities: distinct blocks can share one.")
    print("# This trace is evidence, not a verdict. Compare it against the export yourself.\n")
    paths = trace(blocks, at, a.max_steps, a.max_events, a.assume_opaque)
    for pid in sorted(paths):
        if len(paths) > 1:
            print(f"\n===== path {pid} " + "=" * 50)
        for e in paths[pid]:
            if a.calls_only and not any(k in e for k in
                                        ("CALL", "STORE", "RET", "THROW", "FORK", "ASSUMED",
                                         "Unknown (", "merged")):
                continue
            print(e)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
