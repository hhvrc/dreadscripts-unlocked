namespace DreadScripts.ControllerEditor;

internal struct ValidationResult
{
	internal readonly bool _WorkerThread;

	internal readonly string m_FilterThread;

	internal int _StubThread;

	internal bool readerThread;

	private static object RateStatus;

	internal ValidationResult(bool isparam, string b = "", int template_ID = 0)
	{
		_WorkerThread = isparam;
		m_FilterThread = b;
		_StubThread = template_ID;
		readerThread = true;
	}

	public static implicit operator bool(ValidationResult param)
	{
		return param._WorkerThread;
	}

	public static implicit operator ValidationResult(bool isfirst)
	{
		return new ValidationResult(isfirst);
	}

	public static implicit operator string(ValidationResult ident)
	{
		return ident.m_FilterThread;
	}

	public static implicit operator ValidationResult((bool, string) reference)
	{
		return new ValidationResult(reference.Item1, reference.Item2);
	}

	public static implicit operator (bool, string)(ValidationResult init)
	{
		return (init._WorkerThread, init.m_FilterThread);
	}

	public override string ToString()
	{
		return $"{_WorkerThread}: {m_FilterThread} ({_StubThread})";
	}

	internal static bool PostStatus()
	{
		return RateStatus == null;
	}
}
