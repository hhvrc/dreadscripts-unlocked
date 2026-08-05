using System.CommandLine;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace IlRename;

/// <summary>
/// Applies hand-chosen names to a deobfuscated assembly before it is decompiled.
///
/// The pipeline is  binaries/X.dll -> de4dot -> ilrename -> ilspycmd -> export/.
/// Renaming here rather than in the decompiled text means every use site follows the definition
/// automatically, because IL references entities by metadata token. Nothing is matched by string.
/// </summary>
public static class Program
{
    const string RootDescription = """
        Apply a rename map to a .NET assembly before decompiling it.

        Key forms:
          Ns.Type/Nested#0x02000060      name + metadata token  (preferred; the token wins)
          0x02000060                     token only
          Ns.Type::method(System.Int32)  name only

        Tokens survive the de4dot step because de4dot already preserves RIDs from the original
        binary. Do not add --preserve-tokens or --preserve-table to force it: the resulting module
        can no longer be rewritten. `report` flags any key whose token stopped matching its label.
        """;

    // The same Option instance has to build the command and read the value back out of the parse
    // result, so these are shared rather than constructed inline per subcommand.
    static readonly Option<FileInfo> InOption =
        new("--in") { Description = "Assembly to read (de4dot output).", Required = true };

    static readonly Option<string> OutOption =
        new("--out") { Description = "Path to write.", Required = true };

    static readonly Option<FileInfo[]> MapOption =
        new("--map") { Description = "Rename map JSON. Repeat to merge several.", Required = true };

    static readonly Option<FileInfo[]> OptionalMapOption =
        new("--map") { Description = "Rename map JSON. Repeat to merge several." };

    static readonly Option<string[]> NamespaceOption = new("--namespace")
    {
        Description = "Restrict to this namespace. Repeatable. Off by default.",
    };

    static readonly Option<string> EntityOption =
        new("--entity") { Description = "Entity to look up, in any key form.", Required = true };

    static readonly Option<bool> VerboseOption =
        new("--verbose", "-v") { Description = "List every unnamed entity instead of a sample." };

    static readonly Option<bool> AllMembersOption = new("--all-members")
    {
        Description = "Include every member, not only those with a generated-looking name.",
    };

    static readonly Option<bool> ForceOption = new("--force")
    {
        Description = "Apply renames the safety checks rejected. Expect a broken decompile.",
    };

    public static int Main(string[] args)
    {
        var template = new Command("template",
            "Write a map skeleton: every type (and every generated-looking member) as a key with "
            + "an empty value, annotated with its kind, signature and metadata token. Fill the "
            + "values in by hand — the tool never invents a name.")
        { InOption, OutOption, AllMembersOption, NamespaceOption };
        template.SetAction(Template);

        var report = new Command("report",
            "What the map covers, which keys no longer resolve, which names drifted, and what is "
            + "still unnamed. Run this after every re-export.")
        { InOption, OptionalMapOption, NamespaceOption, VerboseOption };
        report.SetAction(Report);

        var usages = new Command("usages",
            "Every method that references an entity, with reference counts — the evidence for "
            + "deciding what to call it.")
        { InOption, EntityOption };
        usages.SetAction(Usages);

        var apply = new Command("apply",
            "Apply the map. Refuses unsafe renames (virtuals, name collisions) unless --force.")
        { InOption, OutOption, MapOption, ForceOption };
        apply.SetAction(Apply);

        var counts = new Command("counts",
            "Type/method/body/instruction/field totals as JSON, read straight from the metadata. "
            + "Run against a deobfuscated assembly and its original to catch a pass that silently "
            + "deleted live code — a count is the one regression signal that survives renaming.")
        { InOption };
        counts.SetAction(Counts);

        var root = new RootCommand(RootDescription) { template, report, usages, apply, counts };
        return root.Parse(args).Invoke();
    }

    static ModuleDefMD Load(FileInfo file)
    {
        if (!file.Exists)
            throw new FileNotFoundException($"assembly not found: {file.FullName}");
        return ModuleDefMD.Load(file.FullName);
    }

    static int Template(ParseResult parse)
    {
        using var module = Load(parse.GetRequiredValue(InOption));
        var outPath = parse.GetRequiredValue(OutOption);
        var readme = $"""
            Rename map for {module.Assembly?.Name ?? module.Name}.
            Generated by `ilrename template` — fill in the empty values by hand.

            Each key is "<current name>#<metadata token>". The token identifies the entity, so a
            de4dot re-run that changes its generated name keeps the entry working; the name is a
            label that `ilrename report` checks and flags if it has drifted.

            An empty value means "not named yet" and is skipped, so this file is safe to apply at
            any point. Pick names by reading what the entity does — `ilrename usages` lists every
            call site. Virtual methods are listed but cannot be renamed; see the report.
            """;
        var entries = new Inventory(module).BuildTemplate(
            parse.GetValue(AllMembersOption), parse.GetValue(NamespaceOption));
        RenameMap.WriteTemplate(outPath, entries, readme);
        Console.WriteLine($"wrote {outPath}");
        return 0;
    }

    static int Report(ParseResult parse)
    {
        using var module = Load(parse.GetRequiredValue(InOption));
        var maps = parse.GetValue(OptionalMapOption) ?? [];
        var map = maps.Length > 0 ? RenameMap.Load(maps.Select(m => m.FullName)) : null;

        // No namespace filter by default: the obfuscator's own runtime is excluded structurally,
        // and real plugin types do live in the global namespace.
        new Inventory(module).Report(map, new Renamer(module), Console.Out,
                                     parse.GetValue(VerboseOption),
                                     parse.GetValue(NamespaceOption) ?? []);
        return 0;
    }

    static int Usages(ParseResult parse)
    {
        using var module = Load(parse.GetRequiredValue(InOption));
        var renamer = new Renamer(module);
        var key = EntityKey.Parse(parse.GetRequiredValue(EntityOption));
        var resolved = renamer.Resolve(RenameMap.Single(key));
        renamer.Diagnostics.Dump(Console.Error);
        if (resolved.Count == 0)
            return 1;

        var target = resolved[0].Target;
        Console.WriteLine($"{Renamer.FullNameOf(target)}  (0x{target.MDToken.Raw:x8})");
        var found = new Inventory(module).FindUsages(target).ToList();
        if (found.Count == 0)
        {
            Console.WriteLine("  no references found inside this assembly");
            return 0;
        }
        foreach (var (site, count) in found)
            Console.WriteLine($"  {count,4}x  {site}");
        return 0;
    }

    /// <summary>
    /// Metadata totals, as JSON so a caller can diff two runs field by field.
    ///
    /// Empty bodies are counted separately from non-empty ones rather than folded in: a method whose
    /// body a pass emptied still exists in the metadata, so a plain method count cannot see it.
    /// </summary>
    static int Counts(ParseResult parse)
    {
        using var module = Load(parse.GetRequiredValue(InOption));
        int types = 0, methods = 0, bodies = 0, emptyBodies = 0, instructions = 0, fields = 0;
        foreach (var type in module.GetTypes())
        {
            types++;
            fields += type.Fields.Count;
            foreach (var method in type.Methods)
            {
                methods++;
                if (!method.HasBody)
                    continue;
                if (method.Body.Instructions.Count == 0)
                    emptyBodies++;
                else
                {
                    bodies++;
                    instructions += method.Body.Instructions.Count;
                }
            }
        }

        Console.WriteLine($$"""
            {"types": {{types}}, "methods": {{methods}}, "bodies": {{bodies}}, "empty_bodies": {{emptyBodies}}, "instructions": {{instructions}}, "fields": {{fields}}}
            """);
        return 0;
    }

    static int Apply(ParseResult parse)
    {
        using var module = Load(parse.GetRequiredValue(InOption));
        var outPath = parse.GetRequiredValue(OutOption);
        var force = parse.GetValue(ForceOption);
        var map = RenameMap.Load(parse.GetRequiredValue(MapOption).Select(m => m.FullName));
        var renamer = new Renamer(module);

        var resolved = renamer.Resolve(map);
        var safe = renamer.Validate(resolved, force);
        renamer.Diagnostics.Dump(Console.Out);
        if (renamer.Diagnostics.Errors.Count > 0 && !force)
        {
            Console.Error.WriteLine(
                $"\n{renamer.Diagnostics.Errors.Count} error(s); nothing written. "
                + "Fix the map, or pass --force to apply anyway.");
            return 1;
        }

        Console.WriteLine($"\napplying {safe.Count} rename(s):");
        var applied = renamer.Apply(safe, Console.Out);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
        Write(module, outPath);
        Console.WriteLine($"\napplied {applied} rename(s) -> {outPath}");
        return 0;
    }

    /// <summary>
    /// Writes the module, preferring to keep RIDs so the tokens shown in the decompiled output's
    /// file headers still match the ones in the rename map.
    ///
    /// Some deobfuscated modules cannot be rewritten that way — de4dot strips obfuscator types but
    /// can leave references to them behind, and preserving RIDs forces dnlib to re-emit those rows.
    /// The renames themselves are unaffected, so fall back rather than fail: only the token
    /// annotations in the exported source drift, and map keys are resolved against the *input*
    /// module, not this output.
    /// </summary>
    static void Write(ModuleDefMD module, string outPath)
    {
        var options = new ModuleWriterOptions(module);
        options.MetadataOptions.Flags |= MetadataFlags.PreserveRids;
        try
        {
            module.Write(outPath, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nnote: could not preserve RIDs on write ({ex.Message.Trim()}); "
                              + "retrying without. Renames are unaffected; metadata tokens in the "
                              + "decompiled output will not match the rename map.");
            module.Write(outPath, new ModuleWriterOptions(module));
        }
    }
}
