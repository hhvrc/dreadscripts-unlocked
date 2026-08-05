using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace DreadScripts.Common.SupportThankies;

internal readonly struct WebRequestJob : IDisposable
{
	internal readonly UnityWebRequest Request;

	private readonly int pollIntervalMilliseconds;

	private readonly Action onCompleted;

	internal static object NewCode;

	[SpecialName]
	internal bool IsError()
	{
		if (!Request.isNetworkError)
		{
			return Request.isHttpError;
		}
		return true;
	}

	internal WebRequestJob(string info, string token = null, int indexOf_util = 100)
		: this(info, null, token, indexOf_util)
	{
	}

	internal WebRequestJob(string ident, Action reg, string third = null, int t2max = 100)
	{
		if (string.IsNullOrWhiteSpace(third))
		{
			third = "GET";
		}
		Request = new UnityWebRequest(ident, third);
		onCompleted = reg;
		pollIntervalMilliseconds = t2max;
	}

	public void Dispose()
	{
		Request.Dispose();
	}

	internal async Task Process()
	{
		UnityWebRequestAsyncOperation op = Request.SendWebRequest();
		while (!op.isDone)
		{
			await Task.Delay(pollIntervalMilliseconds);
		}
		onCompleted?.Invoke();
	}

	internal static bool LoginCode()
	{
		return NewCode == null;
	}
}
