// Shared by both tools, which shipped four copies of this reader between them. Reconstructed from
// all four; they are the same type under different obfuscated names:
//   reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//     struct ExporterObserver              -> struct JsonObject,      line 1149
//       ExporterObserver(string)           -> JsonObject(string),     line 1159
//       m_IdentifierObserver               -> stored (field)
//       m_AttrObserver                     -> values (field)
//       _DispatcherObserver                -> isEmpty (field)
//       UpdateError(string)                -> this[string] (indexer), line 1191
//     struct RegistryObserver              -> struct JsonValue,       line 1224
//       RegistryObserver(string)           -> JsonValue(string),      line 1238
//       _TagObserver                       -> rawValue (field)
//       importerObserver                   -> stringValue (field)
//       _RequestObserver                   -> boolValue (field)
//       _PrinterObserver                   -> floatValue (field)
//       m_WriterObserver                   -> hasValue (field)
//     static InvokeList                    -> Json.ToJsonObject,      line 6798
//   reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ControllerEditor.cs
//     JsonObject / JsonValue,              lines 2011-2148
//     static CallVisitor                   -> Json.ToJsonObject,      line 10782
//   reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//     JsonObject / JsonValue,              lines 605-742
//     static IncludeConfiguration          -> Json.ToJsonObject,      line 7720
//   reverse-engineering/export/ADOverhaul2019/DreadScripts/ADOverhaul/ADOverhaul.cs
//     JsonObject / JsonValue,              lines 605-742 (no divergence from the 2022 build)
//     static InstantiateSystem             -> Json.ToJsonObject,      line 7705
//
// Placed in DreadScripts.Common rather than nested inside EditorUtils, where the first port put it:
// nothing about it is EditorUtils-specific, the nesting was only an artifact of which decompiled
// file the first copy happened to live in, and leaving it there would force ADOverhaul call sites
// to reach through DreadScripts.ControllerEditor.EditorUtils — a product-to-product dependency the
// two shipped assemblies never had. Same reasoning as SemVer and GUIColorScope.
//
// DELIBERATE DEVIATION FROM THE SHIPPED BEHAVIOUR. The four copies differ in exactly one respect:
// the EditorUtils copy compares the bool case-insensitively (ToLower() == "true"), the other three
// compare ordinally (stringValue == "true"). Both are load-bearing where they sit. The EditorUtils
// copy reads back EditorPrefs blocks this package wrote via object.ToString(), which renders a bool
// as "True" — without the case-insensitive compare every persisted bool would read back false. The
// other three read server responses, which are lowercase. This consolidation keeps the
// case-insensitive form as the single implementation, which WIDENS the three network-facing call
// sites to also accept "True"/"TRUE" where the shipped DLLs were strict. No live behaviour changes
// (the server's own values are lowercase), but it is a real divergence and is recorded here rather
// than absorbed silently. The alternative was re-duplicating ~130 lines to preserve a distinction
// no caller can observe.
//
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and member
// names are the durable reference.
//
// The decompiled struct carried [DefaultMember("Item")] with a [SpecialName] method, which is how
// ILSpy renders an indexer whose name the obfuscator rewrote; it is restored as an indexer here
// because the call sites use indexer syntax.
//
// Deliberately unported: ExporterObserver.AssetError(bool) (line 1202) and its equivalents in the
// other three copies, a debug dump of the parsed pairs that nothing in any assembly calls, and the
// *Candidate static null-check stubs the obfuscator injected into both structs.
//
// Audit status: VERIFIED -- all four copies of the reader and all four copies of the writer diffed
// statement by statement against this file: both structs' fields, both constructors, the
// [DefaultMember("Item")] indexer, both ToString overrides, the three implicit conversions and
// ToJsonObject. The regex literal, the group-2-then-group-3 value selection, the empty-key skip,
// the Length > 1 / Length != 2 quote-stripping ladder and the float.TryParse are identical in every
// copy. The one substantive difference between the copies is the bool comparison recorded in the
// deviation note above: EditorUtils compares stringValue.ToLower() == "true", the other three
// compare stringValue == "true", re-confirmed in all four during this audit. The single-copy
// members named as unported (AssetError/ToString(bool) and the *Candidate stubs) were also confirmed
// present in all four and absent here. One MAP entry was corrected in the same pass: `static
// InstantiateSystem` was listed under ADOverhaul2022, which declares exactly one writer
// (IncludeConfiguration); InstantiateSystem is the ADOverhaul2019 copy and now sits under that file.

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DreadScripts.Common
{
    /// <summary>
    /// A flat, one-level view over a JSON object written by <see cref="Json.ToJsonObject"/>, scraped
    /// with a regex rather than parsed.
    /// </summary>
    /// <remarks>
    /// This exists to read back the settings blocks this package itself writes into EditorPrefs and
    /// the small flat responses the vendor's server returned, so it handles exactly those shapes and
    /// nothing more. It is not a JSON parser and must not be pointed at foreign JSON:
    /// <list type="bullet">
    /// <item><description>Nesting is not understood. A nested object or an array is matched as
    /// if it were a scalar, and its inner quotes and braces terminate the match early.</description></item>
    /// <item><description>There is no unescaping, matching the writer's lack of escaping. A value
    /// containing a double quote ends its own match, and a value containing <c>,</c> or <c>}</c>
    /// truncates an unquoted value at that character — either way the pair is silently read back
    /// wrong or dropped, and the pairs after it may shift.</description></item>
    /// <item><description>Duplicate keys resolve to the last occurrence; whitespace between
    /// tokens is not tolerated, since the writer emits none.</description></item>
    /// </list>
    /// A string that yields no pairs at all — empty, or malformed past recognition — leaves the
    /// object empty rather than throwing, and every subsequent lookup on it throws instead. The
    /// caller (EditorPrefsConfig.Load) relies on that: it treats a throw as "this block is
    /// unreadable" and falls back to defaults wholesale.
    /// </remarks>
    internal readonly struct JsonObject
    {
        private readonly string stored;

        private readonly Dictionary<string, JsonValue> values;

        /// <summary>True when the source string contained no recognisable key/value pair.</summary>
        internal readonly bool isEmpty;

        /// <summary>
        /// Scrapes every <c>"key":value</c> pair out of <paramref name="json"/>, accepting the
        /// value either quoted or bare up to the next <c>,</c> or <c>}</c>.
        /// </summary>
        internal JsonObject(string json)
        {
            stored = json;

            MatchCollection matches = Regex.Matches(json, "\"(.*?)\":(?:(?:\"(.*?)\")|(?:(.*?)[,}]))");
            int count = matches.Count;
            if (count == 0)
            {
                isEmpty = true;
                values = null;
                return;
            }

            isEmpty = false;
            values = new Dictionary<string, JsonValue>();
            for (int i = 0; i < count; i++)
            {
                Match match = matches[i];
                string key = match.Groups[1].Value;

                // Group 2 is the quoted alternative and group 3 the bare one; only one of them
                // participated in the match, so an empty group 2 means the value was bare.
                string value = match.Groups[2].Value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = match.Groups[3].Value;
                }

                if (!string.IsNullOrEmpty(key))
                {
                    values[key] = new JsonValue(value);
                }
            }
        }

        /// <summary>
        /// The value stored under <paramref name="key"/>, or a default <see cref="JsonValue"/>
        /// whose <see cref="JsonValue.hasValue"/> is false when the key is absent.
        /// </summary>
        /// <remarks>
        /// Throws <see cref="System.NullReferenceException"/> on an object that parsed to nothing
        /// (see the type remarks) — that is the signal callers use to discard the whole block.
        /// </remarks>
        internal JsonValue this[string key]
        {
            get
            {
                values.TryGetValue(key, out JsonValue value);
                return value;
            }
        }

        /// <summary>Returns the original string this object was scraped from, not a re-serialisation.</summary>
        public override string ToString()
        {
            return stored;
        }
    }

    /// <summary>
    /// One scraped value, pre-converted to the handful of types the callers store.
    /// </summary>
    /// <remarks>
    /// The conversions mirror the writer, which stores everything with <c>object.ToString()</c> and
    /// quotes it: a bool reads back true only for the literal text "true" compared
    /// case-insensitively (see the deviation note in the file header), and numbers go through
    /// <see cref="float.TryParse(string, out float)"/>, so a value that is not a number reads back
    /// as 0 rather than failing.
    /// <para>
    /// Both sides use the current culture, and the consequence differs by caller. For an EditorPrefs
    /// block the failure is symmetric: writer and reader agree, so a block round-trips correctly
    /// under a stable locale and only breaks when written under one and read under another. For a
    /// server response it is asymmetric and unconditional — the producer emits invariant <c>.</c>
    /// decimals while the reader uses the current culture, so under a comma-decimal locale (de-DE,
    /// nb-NO, fr-FR) <c>"1.5"</c> does not fail to 0: <c>.</c> is the group separator there, and it
    /// parses as <b>15</b>. The only float read from a response is a retry delay, so the blast
    /// radius is one wrong timeout rather than corrupted settings.
    /// </para>
    /// </remarks>
    internal readonly struct JsonValue
    {
        /// <summary>The value exactly as scraped, still carrying its quotes if it had any.</summary>
        internal readonly string rawValue;

        /// <summary>The value with one layer of surrounding double quotes removed.</summary>
        internal readonly string stringValue;

        internal readonly bool boolValue;

        internal readonly float floatValue;

        /// <summary>
        /// True for any constructed value, and false only for the default struct — which is what
        /// <see cref="JsonObject.this[string]"/> hands back for a missing key.
        /// </summary>
        internal readonly bool hasValue;

        internal JsonValue(string value)
        {
            rawValue = value;
            hasValue = true;

            if (value.Length > 1 && value.StartsWith("\"") && value.EndsWith("\""))
            {
                // Length 2 is the pair of quotes with nothing between them; Substring would be
                // asked for a negative length otherwise.
                stringValue = value.Length != 2 ? value.Substring(1, value.Length - 2) : string.Empty;
            }
            else
            {
                stringValue = value;
            }

            boolValue = stringValue.ToLower() == "true";
            float.TryParse(stringValue, out floatValue);
        }

        public override string ToString()
        {
            return stringValue;
        }

        public static implicit operator string(JsonValue value)
        {
            return value.stringValue;
        }

        public static implicit operator bool(JsonValue value)
        {
            return value.boolValue;
        }

        public static implicit operator float(JsonValue value)
        {
            return value.floatValue;
        }
    }

    /// <summary>
    /// The writer half of the pair, kept as a named host rather than a loose method so that the
    /// reader and writer of this format sit together.
    /// </summary>
    internal static class Json
    {
        /// <summary>
        /// Writes the given pairs as a flat JSON object, <c>{"key":"value",...}</c>, with every value
        /// quoted regardless of its type.
        /// </summary>
        /// <remarks>
        /// Nothing is escaped, so this is only safe for the keys and values this package controls: a
        /// key or value containing a double quote, a brace or a comma produces a string that
        /// <see cref="JsonObject"/> reads back incorrectly. Quoting everything is what lets the
        /// reader stay type-agnostic — the type each entry is restored as comes from the caller's
        /// own schema, not from the stored text.
        /// </remarks>
        internal static string ToJsonObject(IEnumerable<(string, string)> entries)
        {
            StringBuilder builder = new StringBuilder("{");
            bool isFirst = true;
            foreach ((string key, string value) in entries)
            {
                if (!isFirst)
                {
                    builder.Append(',');
                }

                builder.Append("\"" + key + "\":\"" + value + "\"");
                isFirst = false;
            }

            builder.Append("}");
            return builder.ToString();
        }
    }
}
