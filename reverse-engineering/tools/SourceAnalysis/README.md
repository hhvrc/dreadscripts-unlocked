# SourceAnalysis

Roslyn/dnlib helper for the reverse-engineering scripts. Every successful command writes
JSON Lines to stdout; diagnostics go to stderr.

```sh
dotnet run --project tools/SourceAnalysis -- calls --in export/ControllerEditor/-Module-.cs
dotnet run --project tools/SourceAnalysis -- transforms --in export/ControllerEditor/-Module-.cs
dotnet run --project tools/SourceAnalysis -- state-machines --in export/ControllerEditor/-Module-.cs
dotnet run --project tools/SourceAnalysis -- patch-caller-check --in input.dll --out output.dll
```

`calls` reports live `global::_003CModule_003E.smethod_N<T>(integer)` invocations (ILSpy's
valid-C# spelling for `<Module>`) with `spanStart`, `spanLength`, `method`, and `key`.
`transforms` reports each decrypter method's `method`, `a`, and `b` from its
`id = (id * a) ^ b` assignment, including parenthesized, cast, and `unchecked` forms.
`state-machines` reports direct `while (true)` switch dispatchers, including the seed,
per-section outcomes, trace, and conservative verdict. `patch-caller-check` changes only
verified `GetExecutingAssembly`/`GetCallingAssembly` equality guards in `smethod_N` methods,
neutralising the guard so the decryption body always runs. The obfuscator emits the guard in
both polarities — `brfalse` past the body to a `return default`, and `brtrue` to the body with
the `return default` falling through — so the fix is decided by where the `return default` arm
actually sits, not by the branch opcode: the equality result is `pop`ed, and when the body is the
branch target (the `brtrue` shape) an unconditional `br` to it replaces the conditional jump,
leaving the `return default` arm as dead code. ilverify is unchanged by the edit (0 new errors),
and both shapes are covered.
