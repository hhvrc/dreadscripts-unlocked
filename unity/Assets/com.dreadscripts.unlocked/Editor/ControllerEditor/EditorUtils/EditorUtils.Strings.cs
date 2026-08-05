// Reconstructed from: reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static UpdateRules   -> TryMakeNameUnique,    line 5468
//   static ChangeRules   -> MakeNameUnique,       line 5474
//   static SortRules     -> MakeNameUnique,       line 5480
//   static RegisterRules  -> StripNumberSuffix,    line 5497
//   static LogoutRules    -> TryGetTrailingNumber, line 5502
//   static ResetResolver  -> IsNullOrEmpty,        line 2673
//   static FlushResolver  -> IsNullOrWhiteSpace,   line 2678
//   static ConnectResolver -> OrEmpty,             line 2683
//   static CalculateResolver -> Or,                line 2688
//   static NewResolver    -> Humanize,             line 2823
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against reverse-engineering/export/
//
// ChangeRules and SortRules are ported as overloads of one name: they are the same operation, one
// taking the set of names already in use and the other the predicate that decides availability.
// Nothing from the name-uniquing region (5468-5502) is left unported.
//
// The five extension helpers below come from the 2673-2839 region: null/blank tests, "" and
// fallback coalescers, and the Humanize word-splitter. PushResolver (2842, a GUIStyle text-width
// helper) is deliberately not here: it is a GUI measurement rather than a string operation, and it
// has since been ported as GetTextWidth beside the GUIContent scratch helper it depends on, in
// EditorUtils.GuiContent.cs. Nothing from the 2673-2839 string region is left unported.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>
        /// Matches the trailing " 12" of an auto-numbered name, so that re-numbering an already
        /// numbered name replaces the number instead of appending a second one.
        /// </summary>
        /// <remarks>
        /// The separator is a literal space, so only names this class itself produced are treated
        /// as numbered. A user-typed "Layer2" is left whole and becomes "Layer2 2" -- see the
        /// remarks on <see cref="MakeNameUnique"/>. The leading <c>(?=.*)</c> in the decompiled
        /// pattern is a lookahead that always succeeds and has been kept only because removing it
        /// would be an unnecessary edit to a shipped regex; it matches nothing and constrains
        /// nothing.
        /// </remarks>
        private const string NumberSuffixPattern = "(?=.*) \\d+$";

        /// <summary>Matches a run of digits at the end of a name, with or without a separator.</summary>
        private const string TrailingNumberPattern = "(?=.*)(\\d+)$";

        /// <summary>
        /// Makes <paramref name="name"/> unique and reports whether that changed it, which is what a
        /// rename field needs: the new name to show, and whether anything has to be written back.
        /// </summary>
        /// <param name="uniqueName">
        /// The accepted name, which is <paramref name="name"/> itself when it was already free.
        /// </param>
        /// <returns>True when the name had to be renumbered.</returns>
        internal static bool TryMakeNameUnique(string name, Func<string, bool> isAvailable, out string uniqueName)
        {
            uniqueName = MakeNameUnique(name, isAvailable);
            return uniqueName != name;
        }

        /// <summary>
        /// Returns <paramref name="name"/> if it is not among <paramref name="takenNames"/>, and
        /// otherwise the first "&lt;name&gt; &lt;number&gt;" variant that is not.
        /// </summary>
        /// <param name="takenNames">
        /// The names already in use. A <see cref="HashSet{T}"/> is used as given rather than copied,
        /// so a caller that built one with a case-insensitive comparer gets case-insensitive
        /// matching; any other sequence is copied into a set with the default, case-sensitive
        /// comparer. Case sensitivity is therefore the caller's choice, expressed by the collection
        /// it passes.
        /// </param>
        internal static string MakeNameUnique(string name, IEnumerable<string> takenNames)
        {
            HashSet<string> taken = (takenNames as HashSet<string>) ?? new HashSet<string>(takenNames);
            return MakeNameUnique(name, candidate => !taken.Contains(candidate));
        }

        /// <summary>
        /// Returns <paramref name="name"/> if <paramref name="isAvailable"/> accepts it, and
        /// otherwise the first "&lt;name&gt; &lt;number&gt;" variant it does accept.
        /// </summary>
        /// <param name="isAvailable">
        /// Decides whether a candidate may be used. Everything about matching -- whether it is
        /// case sensitive, what collection it consults, whether trimming matters -- lives in this
        /// delegate; the numbering here compares nothing itself.
        /// </param>
        /// <remarks>
        /// <para>
        /// Numbering starts from the number already on the name rather than from 1, so that
        /// duplicating "Layer 7" in a list that goes up to "Layer 20" walks forward from 7 instead
        /// of retrying 1..7 first. An unnumbered name starts at 1, giving "Layer 1".
        /// </para>
        /// <para>
        /// The first candidate tested is therefore the original name again whenever that name
        /// already ends in " &lt;number&gt;" -- one guaranteed-failing call to
        /// <paramref name="isAvailable"/>. Kept as written: the delegate is a set lookup at every
        /// call site, and skipping the repeat would change which number a name lands on.
        /// </para>
        /// <para>
        /// Naming does not round-trip for a name whose digits are not preceded by a space:
        /// "Layer2" is not recognised as numbered, so the suffix is appended rather than replaced
        /// and repeated duplication grows "Layer2 2", "Layer2 2 3" and so on. The starting number
        /// is still read from those digits, because <see cref="TryGetTrailingNumber"/> accepts
        /// digits directly against the name while <see cref="StripNumberSuffix"/> requires the
        /// space.
        /// </para>
        /// </remarks>
        internal static string MakeNameUnique(string name, Func<string, bool> isAvailable)
        {
            if (isAvailable(name))
            {
                return name;
            }

            if (!TryGetTrailingNumber(name, out int number))
            {
                number = 1;
            }

            string baseName = StripNumberSuffix(name);
            while (!isAvailable($"{baseName} {number}"))
            {
                number++;
            }

            return $"{baseName} {number}";
        }

        /// <summary>
        /// Removes a trailing " &lt;number&gt;" from <paramref name="name"/>, leaving any other
        /// name untouched.
        /// </summary>
        internal static string StripNumberSuffix(string name)
        {
            return Regex.Replace(name, NumberSuffixPattern, string.Empty);
        }

        /// <summary>
        /// Reads the run of digits at the end of <paramref name="name"/>, returning false when the
        /// name is null, blank, or does not end in a digit.
        /// </summary>
        /// <param name="number">The trailing digits as an integer, or 0 when there are none.</param>
        /// <remarks>
        /// No separator is required, so "Layer 3", "Layer3" and "3" all yield 3. Ported as
        /// written: a name ending in more digits than an <see cref="int"/> can hold throws from
        /// <see cref="int.Parse(string)"/> rather than returning false. Names reaching this come
        /// from a text field, so it is reachable, but nothing in the original guarded it.
        /// </remarks>
        internal static bool TryGetTrailingNumber(string name, out int number)
        {
            number = 0;

            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            Match match = Regex.Match(name, TrailingNumberPattern);
            if (!match.Success)
            {
                return false;
            }

            number = int.Parse(match.Groups[1].Value);
            return true;
        }

        /// <summary><see cref="string.IsNullOrEmpty(string)"/> as an extension method.</summary>
        internal static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary><see cref="string.IsNullOrWhiteSpace(string)"/> as an extension method.</summary>
        internal static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>Returns the string, or "" when it is null.</summary>
        internal static string OrEmpty(this string value)
        {
            return value ?? "";
        }

        /// <summary>Returns the string when it is non-empty, otherwise <paramref name="fallback"/>.</summary>
        internal static string Or(this string value, string fallback)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            return fallback;
        }

        /// <summary>
        /// Turns a camel/Pascal-case identifier into spaced words -- capitalises the first letter and
        /// inserts a space before each upper-case letter that follows a lower-case one.
        /// </summary>
        internal static string Humanize(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(char.ToUpper(value[0]));
            for (int i = 1; i < value.Length; i++)
            {
                if (char.IsUpper(value[i]) && !char.IsUpper(value[i - 1]))
                {
                    builder.Append(' ');
                }

                builder.Append(value[i]);
            }

            return builder.ToString();
        }
    }
}
