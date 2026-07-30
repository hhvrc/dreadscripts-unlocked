using System;
using System.Text;
using UnityEditor;

namespace DreadScripts.ControllerEditor;

internal struct BatchOperationContext
{
	private StringBuilder _ManagerServer;

	internal int _ItemServer;

	internal int specificationServer;

	internal string m_MethodServer;

	private string _SchemaServer;

	private string broadcasterServer;

	private string proxyServer;

	private bool m_StructServer;

	internal bool _ServiceServer;

	internal bool m_StateServer;

	internal static object InstantiateSystem;

	internal BatchOperationContext PublishContext(Action asset)
	{
		if (_ManagerServer == null)
		{
			_ManagerServer = new StringBuilder();
		}
		try
		{
			asset();
		}
		catch (Exception ex)
		{
			m_StateServer = true;
			string text = _SchemaServer + " - " + broadcasterServer + " - " + proxyServer + "\n" + ex.Message;
			if (!string.IsNullOrEmpty(m_MethodServer))
			{
				text = m_MethodServer + " - " + text;
			}
			_ManagerServer.AppendLine("Error occured at step:\n" + text + "\n\n");
			if (!m_StructServer)
			{
				if (EditorUtility.DisplayDialog("Uh oh", $"Something went wrong!\n\n{_ManagerServer}Press Copy and send it to whoever is responsible for this.", "Copy", "Heck"))
				{
					EditorGUIUtility.systemCopyBuffer = _ManagerServer.ToString();
				}
				throw;
			}
		}
		finally
		{
			if (_ServiceServer)
			{
				EditorUtility.ClearProgressBar();
			}
		}
		return this;
	}

	internal BatchOperationContext PopContext(string asset)
	{
		broadcasterServer = asset;
		return this;
	}

	internal BatchOperationContext ComputeContext(string item)
	{
		proxyServer = item;
		return this;
	}

	internal BatchOperationContext MoveContext(string setup)
	{
		_SchemaServer = setup;
		return this;
	}

	internal BatchOperationContext ConcatContext()
	{
		_ItemServer++;
		return this;
	}

	internal BatchOperationContext CallContext()
	{
		_ServiceServer = true;
		EditorUtility.DisplayProgressBar(broadcasterServer, $"{proxyServer} ({_ItemServer}/{specificationServer})", (float)_ItemServer / (float)specificationServer);
		return this;
	}

	internal BatchOperationContext CancelContext()
	{
		broadcasterServer = (proxyServer = (_SchemaServer = string.Empty));
		_ItemServer = 0;
		_ManagerServer.Clear();
		m_StateServer = false;
		while (_ServiceServer)
		{
			EditorUtility.ClearProgressBar();
		}
		return this;
	}

	internal BatchOperationContext CountContext(bool evaluateinstance)
	{
		m_StructServer = evaluateinstance;
		return this;
	}

	internal static bool RevertSystem()
	{
		return InstantiateSystem == null;
	}
}
