using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace IlRename;

/// <summary>
/// A flat key -> value rename map, loaded from JSON (comments and trailing commas allowed).
///
/// Values are the new simple name. For a top-level type the value may be namespace-qualified
/// (<c>DreadScripts.ADOverhaul.LicenseManager</c>) to move the type as well as rename it. An empty
/// value means "not named yet" and is skipped, so a half-filled template is always safe to apply.
///
/// Three key forms are accepted:
///
///   <c>"DreadScripts.ADOverhaul.Foo/Nested#0x02000060"</c>
///       Name plus metadata token. This is what <c>template</c> emits and the form to prefer. The
///       token identifies the entity; the name is a label that the tool verifies and reports on if
///       it has drifted, which is how a re-export that shuffled de4dot's generated names shows up.
///
///   <c>"0x02000060"</c>
///       Token only. Unambiguous but unreadable; useful when de4dot's name is genuinely useless.
///
///   <c>"DreadScripts.ADOverhaul.Foo::smethod_1(System.Int32)"</c>
///       Name only. Readable and easy to hand-write, but a de4dot upgrade that renames the entity
///       silently orphans the entry — the report flags it as unresolved rather than guessing.
///
/// Tokens survive the de4dot step because de4dot preserves RIDs from the original binary: measured
/// on ADOverhaul2022, every TypeDef, Method and Field token in the deobfuscated output is also
/// present in binaries/ADOverhaul2022.dll. Do not add <c>--preserve-tokens</c> or
/// <c>--preserve-table</c> to force the issue — when those actually take effect the resulting
/// module can no longer be rewritten by dnlib. Nothing relies on that preservation holding
/// silently: the name label in each key is checked against the token, so drift is reported.
/// </summary>
public class RenameMap
{
    public List<MapEntry> Entries { get; } = new();
    public List<string> Sources { get; } = new();

    static readonly JsonSerializerOptions Options = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    // Keys contain `<`, `>` and `/` from nested-type names; the default encoder escapes them to
    // \u003C and makes the map unreadable for the human who has to fill it in.
    static readonly JsonSerializerOptions WriteOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static RenameMap Load(IEnumerable<string> paths)
    {
        var map = new RenameMap();
        var seen = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var path in paths)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"rename map not found: {path}");
            Dictionary<string, string> raw;
            try
            {
                raw = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    File.ReadAllText(path), Options) ?? new();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"{path}: {ex.Message}", ex);
            }

            foreach (var (key, value) in raw)
            {
                if (key.StartsWith('_'))   // `_readme` and friends are notes for the human
                    continue;
                if (string.IsNullOrWhiteSpace(value))
                    continue;
                if (seen.TryGetValue(key, out var existing) && existing != value)
                    throw new InvalidOperationException(
                        $"conflicting rename for '{key}': '{existing}' vs '{value}' (in {path})");
                seen[key] = value;
                map.Entries.Add(new MapEntry(EntityKey.Parse(key), value.Trim(), path));
            }
            map.Sources.Add(path);
        }
        return map;
    }

    /// <summary>
    /// The namespaces this map talks about, so a report can scope itself to the code being
    /// reverse engineered instead of also listing the obfuscator's own runtime helpers.
    /// </summary>
    public IReadOnlyList<string> Namespaces()
    {
        var namespaces = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in Entries)
        {
            if (entry.Key.TypeName is not { } name)
                continue;                       // token-only key: nothing to derive from
            var outer = name.Split('/')[0];     // nested types inherit the outer type's namespace
            var dot = outer.LastIndexOf('.');
            if (dot > 0)
                namespaces.Add(outer[..dot]);
        }
        return namespaces.ToList();
    }

    /// <summary>A one-entry map, so ad-hoc lookups can reuse the resolver.</summary>
    public static RenameMap Single(EntityKey key)
    {
        var map = new RenameMap();
        map.Entries.Add(new MapEntry(key, "-", "<cli>"));
        return map;
    }

    public static void WriteTemplate(string path, IEnumerable<TemplateEntry> entries, string readme)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        foreach (var line in readme.Split('\n'))
            sb.AppendLine($"  // {line.TrimEnd()}");
        sb.AppendLine();

        string lastGroup = null;
        var list = entries.ToList();
        for (int i = 0; i < list.Count; i++)
        {
            var e = list[i];
            if (e.Group != lastGroup)
            {
                if (lastGroup is not null) sb.AppendLine();
                sb.AppendLine($"  // ======== {e.Group} ========");
                lastGroup = e.Group;
            }
            if (!string.IsNullOrEmpty(e.Comment))
                sb.AppendLine($"  // {e.Comment}");
            var comma = i == list.Count - 1 ? "" : ",";
            sb.AppendLine(
                $"  {JsonSerializer.Serialize(e.Key, WriteOptions)}: "
                + $"{JsonSerializer.Serialize(e.Value, WriteOptions)}{comma}");
        }
        sb.AppendLine("}");
        File.WriteAllText(path, sb.ToString());
    }

    public readonly record struct TemplateEntry(string Group, string Key, string Value, string Comment);
}

public sealed record MapEntry(EntityKey Key, string NewName, string Source);

/// <summary>Metadata table of a token, as encoded in the token's high byte.</summary>
public enum TokenTable : byte
{
    TypeDef = 0x02,
    Field = 0x04,
    Method = 0x06,
    Event = 0x14,
    Property = 0x17,
}

/// <summary>
/// A parsed map key. Either a token, a name, or both — when both are present the token wins and
/// the name is kept so drift can be reported.
/// </summary>
public readonly record struct EntityKey(
    uint Token, string TypeName, string MemberName, string Signature, string Raw)
{
    public bool HasToken => Token != 0;
    public bool HasName => TypeName is not null;
    public bool IsTypeByName => HasName && MemberName is null;

    public TokenTable Table => (TokenTable)(byte)(Token >> 24);

    public static EntityKey Parse(string key)
    {
        var raw = key.Trim();
        var text = raw;
        uint token = 0;

        var hash = text.LastIndexOf('#');
        if (hash >= 0)
        {
            token = ParseToken(text[(hash + 1)..].Trim(), raw);
            text = text[..hash].Trim();
            if (text.Length == 0)
                return new EntityKey(token, null, null, null, raw);
        }
        else if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return new EntityKey(ParseToken(text, raw), null, null, null, raw);
        }

        var sep = text.IndexOf("::", StringComparison.Ordinal);
        if (sep < 0)
            return new EntityKey(token, text, null, null, raw);

        var typeName = text[..sep].Trim();
        var member = text[(sep + 2)..].Trim();
        var paren = member.IndexOf('(');
        return paren < 0
            ? new EntityKey(token, typeName, member, null, raw)
            : new EntityKey(token, typeName, member[..paren].Trim(), member[paren..].Trim(), raw);
    }

    static uint ParseToken(string text, string raw)
    {
        var hex = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? text[2..] : text;
        if (!uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
            throw new InvalidOperationException($"malformed metadata token in key '{raw}'");
        if (!Enum.IsDefined(typeof(TokenTable), (byte)(value >> 24)))
            throw new InvalidOperationException(
                $"token 0x{value:x8} in key '{raw}' is not a TypeDef/Field/Method/Event/Property token");
        return value;
    }

    public string DisplayName => MemberName is null
        ? TypeName ?? $"0x{Token:x8}"
        : $"{TypeName}::{MemberName}{Signature}";

    public override string ToString() => Raw;
}
