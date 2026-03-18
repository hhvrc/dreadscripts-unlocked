using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal sealed class ErrorPolicy
{
	private GUIContent _SetterPolicy;

	private Texture2D _ConnectionPolicy;

	private readonly string contextPolicy;

	private readonly string _RecordPolicy;

	private bool _HelperPolicy;

	private static ErrorPolicy DisableDecorator;

	[SpecialName]
	private GUIContent SearchRecord()
	{
		if (_HelperPolicy && _SetterPolicy.image == null)
		{
			RegisterRecord();
		}
		return _SetterPolicy;
	}

	[SpecialName]
	private void RevertRecord(GUIContent item)
	{
		_SetterPolicy = item;
	}

	[SpecialName]
	internal Texture2D CompareHelper()
	{
		if (_HelperPolicy && _ConnectionPolicy == null)
		{
			RegisterRecord();
		}
		return _ConnectionPolicy;
	}

	[SpecialName]
	internal void SetHelper(Texture2D setup)
	{
		_ConnectionPolicy = setup;
		_HelperPolicy = _ConnectionPolicy != null;
		if (_HelperPolicy)
		{
			PrintRecord(setup.EncodeToPNG(), contextPolicy);
		}
		LogoutRecord();
	}

	public ErrorPolicy(string i, string reg = "")
	{
		contextPolicy = i;
		_RecordPolicy = reg;
		RegisterRecord();
		LogoutRecord();
	}

	private void RegisterRecord()
	{
		SetHelper(ManageRecord(contextPolicy));
	}

	private void LogoutRecord()
	{
		RevertRecord(new GUIContent(CompareHelper(), _RecordPolicy));
	}

	private static byte[] PatchRecord(int[] key)
	{
		byte[] array = new byte[key.Length];
		for (int i = 0; i < key.Length; i++)
		{
			array[i] = (byte)key[i];
		}
		return array;
	}

	private static int[] InterruptRecord(byte[] value)
	{
		int num = value.Length;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = value[i];
		}
		return array;
	}

	internal static Texture2D ManageRecord(string task)
	{
		int[] intArray = SessionState.GetIntArray(task, null);
		if (intArray != null)
		{
			try
			{
				byte[] data = PatchRecord(intArray);
				Texture2D texture2D = new Texture2D(0, 0);
				texture2D.LoadImage(data);
				texture2D.Apply();
				return texture2D;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				SessionState.EraseIntArray(task);
			}
		}
		return null;
	}

	internal static void PrintRecord(byte[] v, string b)
	{
		int[] value = InterruptRecord(v);
		SessionState.SetIntArray(b, value);
	}

	public static implicit operator GUIContent(ErrorPolicy param)
	{
		return param.SearchRecord();
	}

	internal static bool VerifyDecorator()
	{
		return DisableDecorator == null;
	}
}
