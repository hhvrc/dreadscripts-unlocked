namespace DreadScripts.ControllerEditor;

internal struct SystemThread
{
	internal readonly bool _WorkerThread;

	internal readonly string m_FilterThread;

	internal int _StubThread;

	internal bool readerThread;

	private static object RateStatus;

	internal SystemThread(bool isparam, string b = "", int template_ID = 0)
	{
		_WorkerThread = isparam;
		m_FilterThread = b;
		_StubThread = template_ID;
		readerThread = true;
	}

	public static implicit operator bool(SystemThread param)
	{
		return param._WorkerThread;
	}

	public static implicit operator SystemThread(bool isfirst)
	{
		return new SystemThread(isfirst);
	}

	public static implicit operator string(SystemThread ident)
	{
		return ident.m_FilterThread;
	}

	public static implicit operator SystemThread((bool, string) reference)
	{
		return new SystemThread(reference.Item1, reference.Item2);
	}

	public static implicit operator (bool, string)(SystemThread init)
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
