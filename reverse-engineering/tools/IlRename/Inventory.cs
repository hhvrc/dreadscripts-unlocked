using System.Text.RegularExpressions;
using dnlib.DotNet;

namespace IlRename;

/// <summary>
/// Read-only views over a module: what is in it, what the map covers, and who uses a given member.
/// </summary>
public sealed class Inventory
{
    readonly ModuleDefMD _module;
    readonly Regex[] _obfuscated;

    /// <summary>
    /// Names that carry no meaning and are therefore the naming work queue. de4dot's generated
    /// names (`smethod_1`, `field_3`, `Class8`) plus .NET Reactor's habit of picking a real word
    /// and gluing on a role-sounding suffix (`IdentifierSerializerConnector`).
    /// </summary>
    public static readonly string[] DefaultObfuscatedPatterns =
    {
        @"^s?method_\d+$", @"^field_\d+$", @"^Class\d+$", @"^struct_\d+$", @"^_?[a-z]+_\d+$",
    };

    public Inventory(ModuleDefMD module, IEnumerable<string> patterns = null)
    {
        _module = module;
        _obfuscated = (patterns ?? DefaultObfuscatedPatterns)
            .Select(p => new Regex(p, RegexOptions.Compiled)).ToArray();
    }

    public bool LooksObfuscated(string name) =>
        _obfuscated.Any(r => r.IsMatch(name)) || IsVocabularyName(name);

    // ---------------------------------------------------------------- vocabulary detection

    /// <summary>
    /// A name is obfuscator vocabulary when every CamelCase token in it is one this assembly reuses
    /// far more often than real code would.
    /// </summary>
    /// <remarks>
    /// The digit patterns only catch de4dot's own placeholders (`method_3`, `Class12`). Reactor's
    /// renamer does something else: it recombines a small dictionary into pronounceable names, so
    /// `PushMapper`, `SortObserver` and `singletonSerializer` read as real code and slip past every
    /// pattern. In one sample, of 5764 distinct member names, 217 contain "processor", 216 "visitor",
    /// 212 "annotation". A vocabulary that concentrated is not something a person writes.
    ///
    /// So the threshold is derived from the module rather than hardcoded — no word list to maintain,
    /// and it calibrates itself to whatever dictionary a given sample was obfuscated with. A single
    /// token is never enough on its own: `Update` and `Draw` are legitimately common, and the signal
    /// is *recombination*, so at least two tokens must be present and all of them frequent.
    /// </remarks>
    public bool IsVocabularyName(string name)
    {
        if (_vocabulary is null)
            BuildVocabulary();
        // The head noun carries the signal, not every token. Reactor pairs an ordinary verb with a
        // dictionary noun -- RevertAnnotation, SetupDefinition, SortObserver, m_MapperPolicy -- so
        // requiring the whole name to be vocabulary misses exactly the names worth renaming, while
        // matching on any token would sweep in legitimate ones. Two or more tokens, last one frequent.
        var tokens = Tokenize(name);
        return tokens.Count >= 2 && _vocabulary.Contains(tokens[tokens.Count - 1]);
    }

    HashSet<string> _vocabulary;

    /// <summary>Tokens appearing in enough distinct names to be a generated dictionary, not prose.</summary>
    void BuildVocabulary()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in _module.GetTypes())
        {
            if (IsObfuscatorRuntime(type))
                continue;
            names.Add(type.Name);
            foreach (var f in type.Fields) names.Add(f.Name);
            foreach (var m in type.Methods) names.Add(m.Name);
            foreach (var pr in type.Properties) names.Add(pr.Name);
        }

        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in names)
            foreach (var t in Tokenize(n).Distinct())
                counts[t] = counts.TryGetValue(t, out var c) ? c + 1 : 1;

        // Scaled to the assembly: a token carried by >=1% of all names, with a floor so a tiny module
        // cannot make every token look significant.
        var threshold = Math.Max(12, names.Count / 100);
        _vocabulary = new HashSet<string>(
            counts.Where(kv => kv.Value >= threshold).Select(kv => kv.Key),
            StringComparer.OrdinalIgnoreCase);
    }

    static List<string> Tokenize(string name) =>
        Regex.Matches(name.TrimStart('_'), @"[A-Z][a-z]{2,}|^[a-z]{3,}")
             .Select(m => m.Value.ToLowerInvariant()).ToList();

    /// <summary>
    /// Optional extra namespace filter. Note that plenty of real plugin types sit in the global
    /// namespace (`MerchantPolicy`, `ModelAccountCollection`), so this must not be relied on to
    /// separate the plugin from the obfuscator — <see cref="IsObfuscatorRuntime"/> does that.
    /// </summary>
    public static bool InScope(TypeDef type, IReadOnlyList<string> namespaces)
    {
        if (namespaces is null || namespaces.Count == 0)
            return true;
        var ns = NamespaceOf(type);
        return namespaces.Any(n => ns == n || ns.StartsWith(n + ".", StringComparison.Ordinal));
    }

    /// <summary>A nested type carries no namespace of its own; it belongs to its outermost parent's.</summary>
    public static string NamespaceOf(TypeDef type)
    {
        var outer = type;
        while (outer.DeclaringType is not null)
            outer = outer.DeclaringType;
        return outer.Namespace.String ?? "";
    }

    /// <summary>
    /// Lambda closures and iterator state machines (`&lt;&gt;c__DisplayClass5_0`) are emitted by the
    /// C# compiler, not by the obfuscator. There is no meaningful name to give them, and ILSpy
    /// renders them back as lambdas anyway.
    /// </summary>
    public static bool IsCompilerGenerated(TypeDef type) => type.Name.String.StartsWith('<');

    /// <summary>
    /// .NET Reactor's own runtime lives in types nested inside &lt;Module&gt; (`Class0`, `Struct1`,
    /// the string decryptor). They outnumber the plugin's real code and are not what anyone is
    /// naming, so they stay out of templates and coverage counts by default.
    ///
    /// This is a structural test on purpose: filtering by namespace instead would also drop the
    /// plugin types that sit in the global namespace.
    /// </summary>
    public static bool IsObfuscatorRuntime(TypeDef type)
    {
        var outer = type;
        while (outer.DeclaringType is not null)
            outer = outer.DeclaringType;
        return outer.IsGlobalModuleType;
    }

    static string Token(IMDTokenProvider e) => $"0x{e.MDToken.Raw:x8}";

    // ------------------------------------------------------------------ template

    public IEnumerable<RenameMap.TemplateEntry> BuildTemplate(bool allMembers,
                                                              IReadOnlyList<string> namespaces)
    {
        foreach (var type in _module.GetTypes().OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            if (type.IsGlobalModuleType || IsObfuscatorRuntime(type)
                || !InScope(type, namespaces) || IsCompilerGenerated(type))
                continue;
            var group = NamespaceOf(type) is { Length: > 0 } ns ? ns : "<no namespace>";
            var kind = type.IsEnum ? "enum" : type.IsInterface ? "interface"
                     : type.IsValueType ? "struct" : "class";
            var baseName = type.BaseType is null or { FullName: "System.Object" }
                ? "" : $" : {type.BaseType.Name}";
            yield return new RenameMap.TemplateEntry(
                group,
                $"{type.FullName}#{Token(type)}",
                "",
                $"{kind} {type.Name}{baseName}  " +
                $"({type.Fields.Count} fields, {type.Methods.Count} methods)");

            foreach (var entry in MemberEntries(type, group, allMembers))
                yield return entry;
        }
    }

    IEnumerable<RenameMap.TemplateEntry> MemberEntries(TypeDef type, string group, bool allMembers)
    {
        foreach (var f in type.Fields.OrderBy(f => f.Name.String, StringComparer.Ordinal))
        {
            if (!allMembers && !LooksObfuscated(f.Name))
                continue;
            yield return new RenameMap.TemplateEntry(
                group, $"{type.FullName}::{f.Name}#{Token(f)}", "",
                $"    field {f.FieldType.TypeName} {f.Name}");
        }
        foreach (var p in type.Properties.OrderBy(p => p.Name.String, StringComparer.Ordinal))
        {
            if (!allMembers && !LooksObfuscated(p.Name))
                continue;
            yield return new RenameMap.TemplateEntry(
                group, $"{type.FullName}::{p.Name}#{Token(p)}", "",
                $"    property {p.PropertySig?.RetType.TypeName} {p.Name}");
        }
        foreach (var m in type.Methods.OrderBy(m => m.Name.String, StringComparer.Ordinal))
        {
            if (m.IsRuntimeSpecialName || m.IsGetter || m.IsSetter || m.IsAddOn || m.IsRemoveOn)
                continue;
            if (!allMembers && !LooksObfuscated(m.Name))
                continue;
            var ps = string.Join(", ", m.Parameters.Where(p => !p.IsHiddenThisParameter)
                                        .Select(p => p.Type.TypeName));
            var note = m.IsVirtual ? "  [virtual — not renamable, see report]" : "";
            yield return new RenameMap.TemplateEntry(
                group, $"{type.FullName}::{m.Name}#{Token(m)}", "",
                $"    method {m.ReturnType.TypeName} {m.Name}({ps}){note}");
        }
    }

    // ------------------------------------------------------------------ report

    public void Report(RenameMap map, Renamer renamer, TextWriter w, bool verbose,
                       IReadOnlyList<string> namespaces = null)
    {
        var types = _module.GetTypes()
            .Where(t => !t.IsGlobalModuleType && !IsObfuscatorRuntime(t)
                        && InScope(t, namespaces) && !IsCompilerGenerated(t))
            .ToList();
        var resolved = map is null ? new List<Resolved>() : renamer.Resolve(map);
        var mapped = resolved.Select(r => r.Target).ToHashSet();

        w.WriteLine($"assembly   {_module.Assembly?.FullName ?? _module.Name}");
        w.WriteLine($"types      {types.Count}");
        w.WriteLine($"methods    {types.Sum(t => t.Methods.Count)}");
        w.WriteLine($"fields     {types.Sum(t => t.Fields.Count)}");
        if (map is not null)
        {
            w.WriteLine($"map        {map.Entries.Count} entries from " +
                        $"{string.Join(", ", map.Sources.Select(Path.GetFileName))}");
            w.WriteLine($"resolved   {resolved.Count}");
        }

        var unnamedTypes = types.Where(t => !mapped.Contains(t)).ToList();
        var obfTypes = unnamedTypes.Where(t => LooksObfuscated(t.Name)).ToList();
        w.WriteLine();
        w.WriteLine($"[TODO] {unnamedTypes.Count} type(s) have no rename ({obfTypes.Count} " +
                    $"with an obviously generated name)");
        if (verbose)
            foreach (var t in unnamedTypes.OrderBy(t => t.FullName, StringComparer.Ordinal))
                w.WriteLine($"    {Token(t)}  {t.FullName}");

        var obfMembers = new List<string>();
        foreach (var t in types)
        {
            foreach (var f in t.Fields.Where(f => !mapped.Contains(f) && LooksObfuscated(f.Name)))
                obfMembers.Add($"    {Token(f)}  {t.FullName}::{f.Name}  (field)");
            foreach (var m in t.Methods.Where(m => !mapped.Contains(m) && LooksObfuscated(m.Name)))
                obfMembers.Add($"    {Token(m)}  {t.FullName}::{m.Name}  (method)");
        }
        w.WriteLine($"[TODO] {obfMembers.Count} member(s) still carry a generated name");
        foreach (var line in verbose ? obfMembers : obfMembers.Take(20))
            w.WriteLine(line);
        if (!verbose && obfMembers.Count > 20)
            w.WriteLine($"    ... {obfMembers.Count - 20} more (-v for all)");

        if (map is not null)
        {
            w.WriteLine();
            renamer.Validate(resolved, force: false);
            renamer.Diagnostics.Dump(w);
        }
    }

    // ------------------------------------------------------------------ usages

    /// <summary>
    /// Every method that references <paramref name="target"/>. This is the evidence for choosing a
    /// name: what a field or method is actually called by, and how often.
    /// </summary>
    public IEnumerable<(string Site, int Count)> FindUsages(IMDTokenProvider target)
    {
        // Code almost never mentions a type token directly — it calls the type's methods and reads
        // its fields. Counting only the TypeDef token would report a heavily used static utility
        // class as unreferenced, so for a type accept any member declared on it.
        var wanted = new HashSet<uint> { target.MDToken.Raw };
        if (target is TypeDef targetType)
        {
            foreach (var m in targetType.Methods) wanted.Add(m.MDToken.Raw);
            foreach (var f in targetType.Fields) wanted.Add(f.MDToken.Raw);
        }

        var hits = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var type in _module.GetTypes())
        foreach (var method in type.Methods.Where(m => m.HasBody))
        {
            if (target is TypeDef t && type == t)
                continue;                   // a type's own members are not "usages" of it
            int count = 0;
            foreach (var insn in method.Body.Instructions)
            {
                var referenced = insn.Operand switch
                {
                    IMethod im => (IMDTokenProvider)im.ResolveMethodDef(),
                    IField ifld => ifld.ResolveFieldDef(),
                    ITypeDefOrRef itd => itd.ResolveTypeDef(),
                    _ => null,
                };
                if (referenced is not null && wanted.Contains(referenced.MDToken.Raw))
                    count++;
            }
            if (count > 0)
                hits[$"{type.FullName}::{method.Name}"] = count;
        }
        return hits.OrderByDescending(h => h.Value).Select(h => (h.Key, h.Value));
    }
}
