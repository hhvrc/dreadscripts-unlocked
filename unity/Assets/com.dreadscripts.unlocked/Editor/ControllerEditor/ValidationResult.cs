// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/ValidationResult.cs
//
// Audit status: VERIFIED -- diffed in full against export/. The four fields, the constructor, all
// five implicit conversions and ToString match statement for statement; only the declaration order
// of the bool/string conversions differs. The unreferenced static pair RateStatus/PostStatus is
// not ported, as an obfuscator decoy (see the same treatment in ReorderableListHelper.cs).

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// The outcome of a validation check: whether it passed, and why not if it did not.
    /// </summary>
    /// <remarks>
    /// The conversions let a check be written and consumed at whatever level of detail the caller
    /// needs — <c>return true;</c> or <c>return (false, "no descriptor");</c> to produce one, and
    /// <c>if (result)</c> to test one — without every call site having to name the type.
    /// </remarks>
    internal struct ValidationResult
    {
        internal readonly bool isValid;

        /// <summary>Why validation failed; empty when it passed.</summary>
        internal readonly string message;

        internal int errorCode;

        /// <summary>
        /// False for a default-constructed value, distinguishing "not checked yet" from "checked and
        /// failed" — both of which have <see cref="isValid"/> false.
        /// </summary>
        internal bool isSet;

        internal ValidationResult(bool isValid, string message = "", int errorCode = 0)
        {
            this.isValid = isValid;
            this.message = message;
            this.errorCode = errorCode;
            isSet = true;
        }

        public static implicit operator bool(ValidationResult result)
        {
            return result.isValid;
        }

        public static implicit operator string(ValidationResult result)
        {
            return result.message;
        }

        public static implicit operator ValidationResult(bool isValid)
        {
            return new ValidationResult(isValid);
        }

        public static implicit operator ValidationResult((bool isValid, string message) result)
        {
            return new ValidationResult(result.isValid, result.message);
        }

        public static implicit operator (bool, string)(ValidationResult result)
        {
            return (result.isValid, result.message);
        }

        public override string ToString()
        {
            return $"{isValid}: {message} ({errorCode})";
        }
    }
}
