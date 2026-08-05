using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace IlRename;

public sealed record Resolved(MapEntry Entry, IMDTokenProvider Target, string OldFullName);

public sealed class Diagnostics
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public List<string> Notes { get; } = new();

    public void Dump(TextWriter w)
    {
        foreach (var n in Notes) w.WriteLine($"  note:    {n}");
        foreach (var x in Warnings) w.WriteLine($"  WARNING: {x}");
        foreach (var e in Errors) w.WriteLine($"  ERROR:   {e}");
    }
}

/// <summary>
/// Applies a <see cref="RenameMap"/> to a module.
///
/// Renaming happens on the metadata definitions, so every use site inside the assembly follows
/// automatically: IL operands, signatures, base-type references and custom attributes all point at
/// a TypeDef/MethodDef/FieldDef by token, not by name. That is the whole reason this runs before
/// ilspycmd instead of rewriting decompiled text afterwards — there is no such thing as a missed
/// call site or a false hit inside an unrelated identifier.
///
/// What metadata does *not* carry by token is handled by the checks below: names spelled inside
/// string literals (reflection), custom-attribute named arguments, and virtual methods whose
/// identity is matched by name against a base type in another assembly.
/// </summary>
public sealed class Renamer
{
    readonly ModuleDefMD _module;
    readonly Diagnostics _diag = new();

    public Renamer(ModuleDefMD module) => _module = module;
    public Diagnostics Diagnostics => _diag;

    // ------------------------------------------------------------------ resolution

    public List<Resolved> Resolve(RenameMap map)
    {
        var resolved = new List<Resolved>();
        foreach (var entry in map.Entries)
        {
            var target = ResolveOne(entry.Key);
            if (target is null)
                continue;
            resolved.Add(new Resolved(entry, target, FullNameOf(target)));
        }
        return resolved;
    }

    IMDTokenProvider ResolveOne(EntityKey key)
    {
        if (key.HasToken)
        {
            var byToken = ResolveToken(key);
            if (byToken is null)
            {
                _diag.Errors.Add($"{key}: token 0x{key.Token:x8} is not present in this assembly");
                return null;
            }
            if (key.HasName)
            {
                var actual = FullNameOf(byToken);
                var expected = key.DisplayName;
                if (!NameMatches(actual, expected))
                    _diag.Warnings.Add(
                        $"{key}: token 0x{key.Token:x8} is now '{actual}', but the key says " +
                        $"'{expected}'. The token wins. If de4dot's naming changed, update the " +
                        $"label; if the token drifted, de4dot stopped preserving RIDs and every " +
                        $"token key in the map needs regenerating.");
            }
            return byToken;
        }

        var type = _module.GetTypes().FirstOrDefault(t => t.FullName == key.TypeName);
        if (type is null)
        {
            _diag.Errors.Add($"{key}: no type named '{key.TypeName}' in this assembly");
            return null;
        }
        if (key.IsTypeByName)
            return type;

        var candidates = new List<IMDTokenProvider>();
        candidates.AddRange(type.Fields.Where(f => f.Name == key.MemberName));
        candidates.AddRange(type.Properties.Where(p => p.Name == key.MemberName));
        candidates.AddRange(type.Events.Where(e => e.Name == key.MemberName));
        candidates.AddRange(type.Methods.Where(m => m.Name == key.MemberName &&
                                                    SignatureMatches(m, key.Signature)));
        switch (candidates.Count)
        {
            case 0:
                _diag.Errors.Add($"{key}: '{key.TypeName}' has no member '{key.MemberName}'" +
                                 (key.Signature is null ? "" : $" matching {key.Signature}"));
                return null;
            case 1:
                return candidates[0];
            default:
                // Overloads of one logical method are fine to rename together; a name that hits
                // several *kinds* of member is not something to guess at.
                if (candidates.All(c => c is MethodDef))
                {
                    _diag.Notes.Add($"{key}: matches {candidates.Count} overloads; renaming all. " +
                                    "Add a signature or use a token to target one.");
                    return candidates[0];
                }
                _diag.Errors.Add($"{key}: ambiguous — matches {candidates.Count} members of " +
                                 "different kinds; use a metadata token");
                return null;
        }
    }

    IMDTokenProvider ResolveToken(EntityKey key)
    {
        return key.Table switch
        {
            TokenTable.TypeDef => _module.ResolveToken(key.Token) as TypeDef,
            TokenTable.Method => _module.ResolveToken(key.Token) as MethodDef,
            TokenTable.Field => _module.ResolveToken(key.Token) as FieldDef,
            TokenTable.Event => _module.ResolveToken(key.Token) as EventDef,
            TokenTable.Property => _module.ResolveToken(key.Token) as PropertyDef,
            _ => null,
        };
    }

    static bool NameMatches(string actual, string expected) =>
        actual == expected ||
        // the key's label may omit the signature
        (expected.IndexOf('(') < 0 && actual.StartsWith(expected + "(", StringComparison.Ordinal));

    static bool SignatureMatches(MethodDef m, string signature)
    {
        if (signature is null)
            return true;
        var want = signature.Trim('(', ')').Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim()).ToArray();
        var have = m.Parameters.Where(p => !p.IsHiddenThisParameter)
                    .Select(p => p.Type.FullName).ToArray();
        return want.Length == have.Length && want.SequenceEqual(have, StringComparer.Ordinal);
    }

    public static string FullNameOf(IMDTokenProvider entity) => entity switch
    {
        TypeDef t => t.FullName,
        MethodDef m => $"{m.DeclaringType.FullName}::{m.Name}" +
                       $"({string.Join(",", m.Parameters.Where(p => !p.IsHiddenThisParameter)
                                              .Select(p => p.Type.FullName))})",
        FieldDef f => $"{f.DeclaringType.FullName}::{f.Name}",
        PropertyDef p => $"{p.DeclaringType.FullName}::{p.Name}",
        EventDef e => $"{e.DeclaringType.FullName}::{e.Name}",
        _ => entity.ToString(),
    };

    static string SimpleNameOf(IMDTokenProvider entity) => entity switch
    {
        TypeDef t => t.Name,
        MethodDef m => m.Name,
        FieldDef f => f.Name,
        PropertyDef p => p.Name,
        EventDef e => e.Name,
        _ => "",
    };

    // ------------------------------------------------------------------ validation

    /// <summary>Drops renames that would be unsafe, recording why. Returns what is safe to apply.</summary>
    public List<Resolved> Validate(List<Resolved> resolved, bool force)
    {
        var safe = new List<Resolved>();
        foreach (var r in resolved)
        {
            var reason = UnsafeReason(r);
            if (reason is not null && !force)
            {
                _diag.Warnings.Add($"skipped {r.OldFullName} -> {r.Entry.NewName}: {reason}");
                continue;
            }
            if (reason is not null)
                _diag.Warnings.Add($"forced {r.OldFullName} -> {r.Entry.NewName} despite: {reason}");
            safe.Add(r);
        }
        CheckCollisions(safe);
        CheckStringLiterals(safe);
        CheckAttributeNamedArguments(safe);
        return safe;
    }

    static string UnsafeReason(Resolved r)
    {
        if (r.Target is not MethodDef m)
            return null;
        if (m.IsRuntimeSpecialName || m.IsSpecialName && (m.Name == ".ctor" || m.Name == ".cctor"))
            return "constructors cannot be renamed";
        if (m.IsVirtual)
            // A virtual method's identity is matched by name+signature against its base type. If
            // that base lives in another assembly (UnityEditor.Editor.OnInspectorGUI), renaming
            // silently detaches the override and the decompiled source stops making sense.
            return "virtual/override or interface implementation — renaming would detach it from " +
                   "the member it overrides (use --force if the base type is in this assembly too)";
        if (m.IsPinvokeImpl && string.IsNullOrEmpty(m.ImplMap?.Name))
            return "P/Invoke without an explicit EntryPoint — the name is the native symbol";
        return null;
    }

    void CheckCollisions(List<Resolved> safe)
    {
        var newNames = safe.ToDictionary(r => r.Target, r => Simple(r.Entry.NewName));

        string Final(IMDTokenProvider e) =>
            newNames.TryGetValue(e, out var n) ? n : SimpleNameOf(e);

        // types, grouped by their enclosing scope. Keyed with generic arity because that is part of
        // a type's identity in metadata -- Action`1 and Action`2 are different types and coexist in
        // one scope, which is how the BCL declares its own delegate families. Keying on the simple
        // name alone rejects any attempt to name a family of ref/out delegate shims uniformly.
        foreach (var group in _module.GetTypes().GroupBy(t => (object)t.DeclaringType ?? t.Namespace.String))
            ReportDuplicates(group.Select(t => (Name: ArityKey(Final(t), t), What: t.FullName)),
                             $"type scope '{group.Key}'");

        foreach (var type in _module.GetTypes())
        {
            ReportDuplicates(type.Fields.Select(f => (Final(f), $"{type.Name}::{f.Name}")),
                             $"fields of {type.FullName}");
            ReportDuplicates(type.Properties.Select(p => (Final(p), $"{type.Name}::{p.Name}")),
                             $"properties of {type.FullName}");
            ReportDuplicates(
                type.Methods.Select(m => (Final(m) + SigKey(m), $"{type.Name}::{m.Name}")),
                $"methods of {type.FullName}");
        }
    }

    static string ArityKey(string name, TypeDef t) =>
        t.GenericParameters.Count == 0 ? name : $"{name}`{t.GenericParameters.Count}";

    // Conversion operators (op_Implicit/op_Explicit) overload on return type alone, so a key built
    // from parameters only would report every such pair as a collision.
    static string SigKey(MethodDef m) =>
        "(" + string.Join(",", m.Parameters.Where(p => !p.IsHiddenThisParameter)
                                .Select(p => p.Type.FullName)) + "):" + m.ReturnType.FullName;

    void ReportDuplicates(IEnumerable<(string Name, string What)> items, string scope)
    {
        foreach (var dup in items.GroupBy(i => i.Name).Where(g => g.Count() > 1))
            _diag.Errors.Add($"name collision in {scope}: {dup.Count()} members would be called " +
                             $"'{dup.Key}' ({string.Join(", ", dup.Select(d => d.What))})");
    }

    void CheckStringLiterals(List<Resolved> safe)
    {
        var watched = safe.Select(r => SimpleNameOf(r.Target))
                          .Where(n => !string.IsNullOrEmpty(n))
                          .ToHashSet(StringComparer.Ordinal);
        if (watched.Count == 0)
            return;
        foreach (var type in _module.GetTypes())
        foreach (var method in type.Methods.Where(m => m.HasBody))
        foreach (var insn in method.Body.Instructions)
        {
            if (insn.OpCode.Code == Code.Ldstr && insn.Operand is string s && watched.Contains(s))
                _diag.Warnings.Add(
                    $"'{s}' is renamed but also appears as a string literal in " +
                    $"{type.Name}::{method.Name} — if that is a reflection lookup, the decompiled " +
                    $"source will no longer line up with the name it resolves");
        }
    }

    void CheckAttributeNamedArguments(List<Resolved> safe)
    {
        var watched = safe.Where(r => r.Target is FieldDef or PropertyDef)
                          .Select(r => SimpleNameOf(r.Target)).ToHashSet(StringComparer.Ordinal);
        if (watched.Count == 0)
            return;
        foreach (var type in _module.GetTypes())
        foreach (var ca in AllAttributes(type))
        foreach (var named in ca.NamedArguments)
        {
            // Attribute named arguments store the target field/property name as a plain string in
            // the blob; dnlib will not follow a rename for us.
            if (watched.Contains(named.Name))
                _diag.Warnings.Add(
                    $"custom attribute [{ca.TypeFullName}] sets named argument '{named.Name}', " +
                    $"which is being renamed — the attribute blob keeps the old spelling");
        }
    }

    static IEnumerable<CustomAttribute> AllAttributes(TypeDef type)
    {
        foreach (var ca in type.CustomAttributes) yield return ca;
        foreach (var m in type.Methods)
            foreach (var ca in m.CustomAttributes) yield return ca;
        foreach (var f in type.Fields)
            foreach (var ca in f.CustomAttributes) yield return ca;
        foreach (var p in type.Properties)
            foreach (var ca in p.CustomAttributes) yield return ca;
    }

    // ------------------------------------------------------------------ application

    public int Apply(List<Resolved> safe, TextWriter log)
    {
        int count = 0;
        // (declaring type, old member name, is-field) -> new name, collected as we rename the
        // definitions so the MemberRef sweep below can find what became stale. See FixMemberRefs.
        var renamedMembers = new Dictionary<(TypeDef, string, bool), string>();
        foreach (var r in safe)
        {
            var newName = r.Entry.NewName;
            if (r.Target is MethodDef rm && rm.DeclaringType is not null)
                renamedMembers[(rm.DeclaringType, rm.Name.String, false)] = Simple(newName);
            else if (r.Target is FieldDef rf && rf.DeclaringType is not null)
                renamedMembers[(rf.DeclaringType, rf.Name.String, true)] = Simple(newName);
            switch (r.Target)
            {
                case TypeDef t:
                    var (ns, name) = SplitTypeName(t, newName);
                    if (ns is not null) t.Namespace = ns;
                    t.Name = name;
                    break;
                case MethodDef m:
                    m.Name = Simple(newName);
                    break;
                case FieldDef f:
                    f.Name = Simple(newName);
                    break;
                case PropertyDef p:
                    RenameAccessors(p.GetMethods.Concat(p.SetMethods), p.Name, Simple(newName));
                    p.Name = Simple(newName);
                    break;
                case EventDef e:
                    var accessors = new[] { e.AddMethod, e.RemoveMethod, e.InvokeMethod }
                        .Where(a => a is not null);
                    RenameAccessors(accessors, e.Name, Simple(newName));
                    e.Name = Simple(newName);
                    break;
                default:
                    continue;
            }
            log.WriteLine($"  {r.OldFullName}  ->  {newName}");
            count++;
        }
        count += FixMemberRefs(renamedMembers, log);
        return count;
    }

    /// <summary>
    ///     Re-point MemberRefs that still name a member we just renamed.
    ///
    ///     Renaming the definition is enough for an ordinary type, because IL reaches its members by
    ///     token. It is NOT enough for a generic one: code inside <c>Foo&lt;T&gt;</c> reaching its own
    ///     field goes through a MemberRef whose parent is a TypeSpec, and a MemberRef carries its own
    ///     name string. Rename only the definition and the declaration moves while every use site
    ///     keeps the old name -- which decompiles to source that declares <c>entries</c> and then
    ///     reads <c>this.m_DicServer</c>. That is not merely ugly, it does not compile, and nothing
    ///     downstream notices because the assembly itself is still internally consistent by token.
    ///
    ///     Matching is on declaring type plus old name plus member kind. The kind matters: a type may
    ///     legally have a field and a method sharing a name, and renaming one must not drag the other.
    /// </summary>
    int FixMemberRefs(Dictionary<(TypeDef, string, bool), string> renamed, TextWriter log)
    {
        if (renamed.Count == 0)
            return 0;

        int fixedUp = 0;
        var seen = new HashSet<MemberRef>();

        void Repair(MemberRef mr)
        {
            if (mr is null || !seen.Add(mr))
                return;
            var declaring = mr.Class switch
            {
                TypeSpec ts => ts.ScopeType?.ResolveTypeDef(),
                ITypeDefOrRef tdr => tdr.ResolveTypeDef(),
                _ => null,
            };
            if (declaring is null)
                return;
            // MemberRef signatures distinguish the two kinds, and that is what tells a field
            // reference from a method reference without resolving anything further.
            var isField = mr.IsFieldRef;
            if (renamed.TryGetValue((declaring, mr.Name.String, isField), out var newName))
            {
                mr.Name = newName;
                fixedUp++;
            }
        }

        foreach (var type in _module.GetTypes())
        {
            foreach (var method in type.Methods)
            {
                if (method.Body is null)
                    continue;
                foreach (var instr in method.Body.Instructions)
                {
                    switch (instr.Operand)
                    {
                        case MemberRef mr:
                            Repair(mr);
                            break;
                        // A call to a generic method of a generic type arrives wrapped like this.
                        case MethodSpec { Method: MemberRef inner }:
                            Repair(inner);
                            break;
                    }
                }
            }
        }

        if (fixedUp > 0)
            log.WriteLine($"  re-pointed {fixedUp} MemberRef(s) left stale by a rename "
                          + "(generic types reach their own members this way)");
        return fixedUp;
    }

    /// <summary>Keeps `get_Foo`/`set_Foo`/`add_Foo` in step with the member they belong to.</summary>
    static void RenameAccessors(IEnumerable<MethodDef> accessors, string oldName, string newName)
    {
        foreach (var a in accessors)
        {
            var underscore = a.Name.String.IndexOf('_');
            if (underscore <= 0)
                continue;
            var prefix = a.Name.String[..(underscore + 1)];
            if (a.Name.String[(underscore + 1)..] == oldName)
                a.Name = prefix + newName;
        }
    }

    static string Simple(string name)
    {
        var dot = name.LastIndexOf('.');
        return dot < 0 ? name : name[(dot + 1)..];
    }

    static (string Namespace, string Name) SplitTypeName(TypeDef type, string newName)
    {
        var dot = newName.LastIndexOf('.');
        if (dot < 0)
            return (null, newName);
        if (type.DeclaringType is not null)
            // Nested types have no namespace of their own; a dotted value here is a mistake worth
            // surfacing rather than silently truncating.
            throw new InvalidOperationException(
                $"'{newName}' is namespace-qualified but {type.FullName} is a nested type");
        return (newName[..dot], newName[(dot + 1)..]);
    }
}
