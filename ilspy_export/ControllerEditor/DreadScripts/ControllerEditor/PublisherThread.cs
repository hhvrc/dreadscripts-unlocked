using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class PublisherThread : IDisposable
{
	private readonly bool _ConfigurationThread;

	private readonly SchemaThread m_ProcThread;

	private static PublisherThread RunDecorator;

	public PublisherThread(bool vopen)
		: this(ref vopen, iscfg: false, null)
	{
	}

	public PublisherThread(ref bool def)
		: this(ref def, iscfg: false, null)
	{
	}

	public PublisherThread(ref bool value, string col, GUIStyle c = null)
		: this(ref value, iscfg: true, new GUIContent(col), c)
	{
	}

	public PublisherThread(ref bool info, GUIContent connection, GUIStyle filter = null)
		: this(ref info, iscfg: true, connection, filter)
	{
	}

	public PublisherThread(ref bool spec, bool iscfg, GUIContent proc, GUIStyle info2 = null)
	{
		if (iscfg)
		{
			if (info2 == null)
			{
				info2 = EditorStyles.foldout;
			}
			spec = EditorGUILayout.Foldout(spec, proc, info2);
		}
		bool num = spec;
		bool flag = num;
		_ConfigurationThread = num;
		if (flag)
		{
			m_ProcThread = new SchemaThread();
		}
	}

	public void Dispose()
	{
		if (_ConfigurationThread)
		{
			m_ProcThread.Dispose();
		}
	}

	public static implicit operator bool(PublisherThread last)
	{
		return last._ConfigurationThread;
	}

	internal static bool ComputeDecorator()
	{
		return RunDecorator == null;
	}
}
