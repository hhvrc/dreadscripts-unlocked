// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static SortRules     -> MakeNameUnique,       line 5480
//   static RegisterRules -> StripNumberSuffix,    line 5497
//   static LogoutRules   -> TryGetTrailingNumber, line 5502
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// Deliberately not ported here: the two thin callers that sit immediately above this region,
// UpdateRules (line 5468, "make unique and report whether that changed the name") and ChangeRules
// (line 5474, "make unique against a set of taken names"). They add no numbering logic of their
// own -- both just call SortRules -- and were left out only to avoid colliding with a concurrent
// port of the neighbouring region. Anyone adding them should put them in this file.

using System;
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
    }
}
