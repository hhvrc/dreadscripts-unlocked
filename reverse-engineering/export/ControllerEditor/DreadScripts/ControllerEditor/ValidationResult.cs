namespace DreadScripts.ControllerEditor;

internal struct ValidationResult
{
	internal readonly bool isValid;

	internal readonly string message;

	internal int errorCode;

	internal bool isSet;

	private static object RateStatus;

	internal ValidationResult(bool isparam, string b = "", int template_ID = 0)
	{
		isValid = isparam;
		message = b;
		errorCode = template_ID;
		isSet = true;
	}

	public static implicit operator bool(ValidationResult param)
	{
		return param.isValid;
	}

	public static implicit operator ValidationResult(bool isfirst)
	{
		return new ValidationResult(isfirst);
	}

	public static implicit operator string(ValidationResult ident)
	{
		return ident.message;
	}

	public static implicit operator ValidationResult((bool, string) reference)
	{
		return new ValidationResult(reference.Item1, reference.Item2);
	}

	public static implicit operator (bool, string)(ValidationResult init)
	{
		return (init.isValid, init.message);
	}

	public override string ToString()
	{
		return $"{isValid}: {message} ({errorCode})";
	}

	internal static bool PostStatus()
	{
		return RateStatus == null;
	}
}
