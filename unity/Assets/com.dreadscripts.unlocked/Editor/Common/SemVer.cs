// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this
// type, under two different names. Reconstructed from both, which are behaviourally
// identical and differ only in obfuscated parameter names and in the branch shape the
// decompiler produced for operator >:
//   decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/SemVer.cs            (as SemVer)
//   decompiled/ADOverhaul2019/DreadScripts/ADOverhaul/SemVer.cs            (as SemVer)
//   decompiled/ControllerEditor/DreadScripts/ControllerEditor/VersionNumber.cs (as VersionNumber)
// The ADOverhaul name is kept here; ControllerEditor call sites that referred to
// VersionNumber use this type instead.
//
// Not ported: the private, never-called Equals(SemVer) overload present in all three
// sources — dead code, most likely the residue of a stripped IEquatable<T> implementation.
// It only forwarded to operator ==, which Equals(object) already does.
//
// Audit status: VERIFIED -- all three shipped copies diffed statement by statement against this
// file: both constructors, all six operators, Equals(object), GetHashCode and ToString. The three
// [SpecialName][CompilerGenerated] Major()/Minor()/Patch() methods are the getters of get-only
// auto-properties and are restored as such. ControllerEditor's and ADOverhaul2019's operator > have
// exactly the branch shape reproduced here; ADOverhaul2022's is the inverted-test rendering of the
// same decision tree, checked case by case to agree on every ordering. GetHashCode reproduces the
// 397 multipliers and their nesting exactly.

namespace DreadScripts.Common
{
    /// <summary>
    /// An immutable major.minor.patch version triple, ordered so that update checks can ask
    /// whether the version advertised by the server is newer than the one that is installed.
    /// </summary>
    /// <remarks>
    /// Despite the name this is not a full semantic version: there is no pre-release or build
    /// metadata. Every component must parse as an <see cref="int"/>, so a tag such as
    /// <c>1.2.3-beta</c> throws rather than sorting before <c>1.2.3</c>. Both tools only ever
    /// published plain numeric versions, so the distinction never arose in practice.
    /// </remarks>
    internal sealed class SemVer
    {
        internal int Major { get; }

        internal int Minor { get; }

        internal int Patch { get; }

        internal SemVer(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        /// <summary>Parses a dot-separated <c>major.minor.patch</c> string.</summary>
        /// <remarks>
        /// Deliberately unvalidated, matching the original: a string with fewer than three
        /// components throws <see cref="System.IndexOutOfRangeException"/>, a non-numeric
        /// component throws <see cref="System.FormatException"/>, and any fourth or later
        /// component is ignored. Callers parse strings they fetched from the update server, and
        /// the tools treated a malformed response as an error to surface rather than absorb.
        /// </remarks>
        internal SemVer(string version)
        {
            string[] components = version.Split('.');
            Major = int.Parse(components[0]);
            Minor = int.Parse(components[1]);
            Patch = int.Parse(components[2]);
        }

        /// <remarks>
        /// Compares most-significant component first and stops at the first difference, so a
        /// larger minor never outweighs a smaller major. Neither operand may be null — the
        /// original does not null-check, and comparing against null throws.
        /// </remarks>
        public static bool operator >(SemVer left, SemVer right)
        {
            if (left.Major > right.Major)
            {
                return true;
            }

            if (left.Major < right.Major)
            {
                return false;
            }

            if (left.Minor > right.Minor)
            {
                return true;
            }

            if (left.Minor < right.Minor)
            {
                return false;
            }

            return left.Patch > right.Patch;
        }

        public static bool operator <(SemVer left, SemVer right)
        {
            return right > left;
        }

        public static bool operator >=(SemVer left, SemVer right)
        {
            return !(left < right);
        }

        public static bool operator <=(SemVer left, SemVer right)
        {
            return !(left > right);
        }

        public static bool operator ==(SemVer left, SemVer right)
        {
            if (left.Major != right.Major || left.Minor != right.Minor)
            {
                return false;
            }

            return left.Patch == right.Patch;
        }

        public static bool operator !=(SemVer left, SemVer right)
        {
            return !(left == right);
        }

        /// <remarks>
        /// The reference comparison is against <see cref="object"/>, not the <c>==</c> operator
        /// above: <paramref name="obj"/> is typed as <see cref="object"/>, so the overload does
        /// not apply and this is an identity test that then falls through to the type check.
        /// Preserved as-is from the original.
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is SemVer other))
            {
                return false;
            }

            return this == other;
        }

        public override int GetHashCode()
        {
            return (((Major * 397) ^ Minor) * 397) ^ Patch;
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }
    }
}
