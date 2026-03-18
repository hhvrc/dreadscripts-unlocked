using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace DreadScripts.Common.SupportThankies;

internal readonly struct Annotation : IDisposable
{
	internal readonly UnityWebRequest _Algo;

	private readonly int mapper;

	private readonly Action initializer;

	internal static object NewCode;

	[SpecialName]
	internal bool OrderWrapper()
	{
		if (!_Algo.isNetworkError)
		{
			return _Algo.isHttpError;
		}
		return true;
	}

	internal Annotation(string info, string token = null, int indexOf_util = 100)
		: this(info, null, token, indexOf_util)
	{
	}

	internal Annotation(string ident, Action reg, string third = null, int t2max = 100)
	{
		if (string.IsNullOrWhiteSpace(third))
		{
			third = "GET";
		}
		_Algo = new UnityWebRequest(ident, third);
		initializer = reg;
		mapper = t2max;
	}

	public void Dispose()
	{
		_Algo.Dispose();
	}

	internal async Task Process()
	{
		UnityWebRequestAsyncOperation op = _Algo.SendWebRequest();
		while (!op.isDone)
		{
			await Task.Delay(mapper);
		}
		initializer?.Invoke();
	}

	internal static bool LoginCode()
	{
		return NewCode == null;
	}
}
