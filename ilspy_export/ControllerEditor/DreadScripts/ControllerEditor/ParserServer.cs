using System;
using System.Text;
using UnityEditor;

namespace DreadScripts.ControllerEditor;

internal struct ParserServer
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

	internal ParserServer PublishContext(Action asset)
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

	internal ParserServer PopContext(string asset)
	{
		broadcasterServer = asset;
		return this;
	}

	internal ParserServer ComputeContext(string item)
	{
		proxyServer = item;
		return this;
	}

	internal ParserServer MoveContext(string setup)
	{
		_SchemaServer = setup;
		return this;
	}

	internal ParserServer ConcatContext()
	{
		_ItemServer++;
		return this;
	}

	internal ParserServer CallContext()
	{
		_ServiceServer = true;
		EditorUtility.DisplayProgressBar(broadcasterServer, $"{proxyServer} ({_ItemServer}/{specificationServer})", (float)_ItemServer / (float)specificationServer);
		return this;
	}

	internal ParserServer CancelContext()
	{
		broadcasterServer = (proxyServer = (_SchemaServer = string.Empty));
		_ItemServer = 0;
		_ManagerServer.Clear();
		m_StateServer = false;
		uint num4 = default(uint);
		while (true)
		{
			int num;
			int num2;
			if (!_ServiceServer)
			{
				num = -446460968;
				num2 = -446460968;
			}
			else
			{
				num = -40722905;
				num2 = -40722905;
			}
			int num3 = num ^ ((int)num4 * -1296928010);
			while (true)
			{
				switch ((num4 = (uint)(num3 ^ -1535384199)) % 4)
				{
				case 1u:
				case 2u:
					break;
				case 0u:
					goto IL_0073;
				default:
					return this;
				}
				break;
				IL_0073:
				EditorUtility.ClearProgressBar();
				num3 = ((int)num4 * -262750743) ^ -1873662458;
			}
		}
	}

	internal ParserServer CountContext(bool evaluateinstance)
	{
		m_StructServer = evaluateinstance;
		return this;
	}

	internal static bool RevertSystem()
	{
		return InstantiateSystem == null;
	}
}
