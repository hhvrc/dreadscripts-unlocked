using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

[DefaultMember("Item")]
internal class GlobalServer
{
	internal class VisitorThread : IDisposable
	{
		private readonly GlobalServer m_AlgoThread;

		private readonly EditorGUI.ChangeCheckScope mapperThread;

		internal static VisitorThread CustomizeStatus;

		public VisitorThread(GlobalServer key)
		{
			m_AlgoThread = key;
			mapperThread = new EditorGUI.ChangeCheckScope();
		}

		public void Dispose()
		{
			if (mapperThread.changed)
			{
				m_AlgoThread.InstantiateContext();
			}
		}

		internal static bool SearchStatus()
		{
			return CustomizeStatus == null;
		}
	}

	internal readonly Dictionary<string, Type> _TaskServer;

	internal readonly Dictionary<string, object> _ProcessServer;

	internal readonly Dictionary<string, string> _ProducerServer;

	internal readonly string _IteratorServer;

	internal readonly Action publisherServer;

	internal IDictionary<string, object> configurationServer;

	internal bool _ProcServer;

	internal bool _WrapperThread;

	private readonly HashSet<string> _AnnotationThread = new HashSet<string>();

	internal static GlobalServer RegisterStatus;

	internal GlobalServer(string info, Dictionary<Enum, Type> pol, Dictionary<Enum, object> proc, Dictionary<Enum, string> item2 = null, Action reference3 = null)
		: this(info, pol.ToDictionary((KeyValuePair<Enum, Type> kvp) => kvp.Key.ToString(), (KeyValuePair<Enum, Type> kvp) => kvp.Value), proc.ToDictionary((KeyValuePair<Enum, object> kvp) => kvp.Key.ToString(), (KeyValuePair<Enum, object> kvp) => kvp.Value), item2?.ToDictionary((KeyValuePair<Enum, string> kvp) => kvp.Key.ToString(), (KeyValuePair<Enum, string> kvp) => kvp.Value), reference3)
	{
	}

	internal GlobalServer(string reference, Dictionary<string, Type> pol, Dictionary<string, object> state, Dictionary<string, string> ident2 = null, Action pred3 = null)
	{
		_IteratorServer = reference;
		publisherServer = pred3 ?? new Action(InsertContext);
		_TaskServer = pol;
		_ProcessServer = state;
		_ProducerServer = ident2;
		_WrapperThread = ident2 != null;
		AwakeContext();
	}

	internal void DisableContext()
	{
		publisherServer();
	}

	internal void InsertContext()
	{
		KeyValuePair<string, object>[] array = configurationServer.Select((KeyValuePair<string, object> kvp) => kvp).ToArray();
		EditorGUI.BeginChangeCheck();
		KeyValuePair<string, object>[] array2 = array;
		for (int num = 0; num < array2.Length; num++)
		{
			KeyValuePair<string, object> keyValuePair = array2[num];
			string key = keyValuePair.Key;
			if (_AnnotationThread.Contains(key))
			{
				continue;
			}
			object value = keyValuePair.Value;
			string value2;
			GUIContent label = new GUIContent(ObjectNames.NicifyVariableName(key), (!_WrapperThread || !_ProducerServer.TryGetValue(key, out value2)) ? string.Empty : value2);
			if (!_TaskServer.TryGetValue(key, out var value3))
			{
				continue;
			}
			Type type = value3;
			if ((object)type == null)
			{
				continue;
			}
			if (type == typeof(bool))
			{
				configurationServer[key] = EditorGUILayout.Toggle(label, (bool)value);
			}
			else if (!(type == typeof(int)))
			{
				if (type == typeof(float))
				{
					configurationServer[key] = EditorGUILayout.FloatField(label, (float)value);
				}
				else if (type == typeof(string))
				{
					configurationServer[key] = EditorGUILayout.TextField(label, (string)value);
				}
			}
			else
			{
				configurationServer[key] = EditorGUILayout.IntField(label, (int)value);
			}
		}
		if (EditorGUI.EndChangeCheck())
		{
			_ProcServer = true;
		}
		using (new GUILayout.HorizontalScope())
		{
			using (new EditorGUI.DisabledScope(!_ProcServer))
			{
				if (EditorUtils.DisableQueue("Revert"))
				{
					AwakeContext();
				}
				if (EditorUtils.DisableQueue("Save"))
				{
					InstantiateContext();
				}
			}
		}
	}

	internal void RestartContext(string ident)
	{
		_AnnotationThread.Add(ident);
	}

	internal void QueryContext(params string[] keys)
	{
		_AnnotationThread.UnionWith(keys);
	}

	internal void AddContext(Enum info)
	{
		QueryContext(info.ToString());
	}

	internal void InvokeContext(params Enum[] keys)
	{
		QueryContext(keys.Select((Enum k) => k.ToString()).ToArray());
	}

	internal void FindContext(string res)
	{
		_AnnotationThread.Remove(res);
	}

	internal void ExcludeContext(params string[] keys)
	{
		_AnnotationThread.ExceptWith(keys);
	}

	internal void InitContext(Enum info)
	{
		FindContext(info.ToString());
	}

	internal void VisitContext(params Enum[] keys)
	{
		ExcludeContext(keys.Select((Enum k) => k.ToString()).ToArray());
	}

	private void DefineContext()
	{
		configurationServer = new Dictionary<string, object>();
		foreach (string key in _TaskServer.Keys)
		{
			StartContext(key);
		}
		_ProcServer = false;
	}

	private void StartContext(string info)
	{
		if (_TaskServer.TryGetValue(info, out var value))
		{
			object cust;
			if (_ProcessServer != null && _ProcessServer.TryGetValue(info, out var value2))
			{
				configurationServer[info] = value2;
			}
			else if (ReadContext(value, out cust))
			{
				configurationServer[info] = cust;
			}
		}
	}

	private bool ReadContext(Type v, out object cust)
	{
		cust = null;
		if ((object)v != null)
		{
			if (v == typeof(bool))
			{
				cust = false;
				return true;
			}
			if (v == typeof(int))
			{
				cust = 0;
				return true;
			}
			if (v == typeof(float))
			{
				cust = 0f;
				return true;
			}
			if (v == typeof(string))
			{
				cust = string.Empty;
				return true;
			}
		}
		return false;
	}

	[SpecialName]
	internal object ResetContext(string config)
	{
		return configurationServer[config];
	}

	[SpecialName]
	internal void FlushContext(string instance, object selection)
	{
		configurationServer[instance] = selection;
		_ProcServer = true;
	}

	[SpecialName]
	internal object CalculateContext(Enum asset)
	{
		return configurationServer[asset.ToString()];
	}

	[SpecialName]
	internal void TestContext(Enum setup, object selection)
	{
		int num = 2;
		uint num3 = default(uint);
		string key = default(string);
		while (true)
		{
			int num2;
			switch (num)
			{
			default:
				_ProcServer = true;
				num = 5;
				break;
			case 4:
				goto IL_002c;
			case 3:
				num2 = (int)((num3 * 899230434) ^ 0x2B271009);
				goto IL_0054;
			case 1:
				goto IL_004f;
			case 2:
				key = setup.ToString();
				num = 1;
				break;
			case 5:
				return;
				IL_0054:
				switch ((num3 = (uint)(num2 ^ -944865669)) % 3)
				{
				case 1u:
					break;
				case 2u:
					goto IL_002c;
				case 0u:
					goto IL_004f;
				default:
					goto IL_007f;
				}
				goto default;
				IL_007f:
				num = 0;
				break;
				IL_004f:
				num2 = -304830982;
				goto IL_0054;
				IL_002c:
				configurationServer[key] = selection;
				num = 3;
				break;
			}
		}
	}

	internal T SelectContext<T>(string first)
	{
		return (T)configurationServer[first];
	}

	internal T RemoveContext<T>(Enum init)
	{
		return (T)configurationServer[init.ToString()];
	}

	internal void InstantiateContext()
	{
		List<(string, string)> list = new List<(string, string)>();
		foreach (KeyValuePair<string, object> item in configurationServer)
		{
			string key = item.Key;
			object value = item.Value;
			list.Add((key, value.ToString()));
		}
		string value2 = EditorUtils.InvokeList(list);
		EditorPrefs.SetString(_IteratorServer, value2);
		_ProcServer = false;
	}

	internal void AwakeContext()
	{
		if (!EditorPrefs.HasKey(_IteratorServer))
		{
			DefineContext();
		}
		else
		{
			string ident = EditorPrefs.GetString(_IteratorServer);
			try
			{
				configurationServer = new Dictionary<string, object>();
				EditorUtils.ExporterObserver exporterObserver = new EditorUtils.ExporterObserver(ident);
				foreach (KeyValuePair<string, Type> item in _TaskServer)
				{
					string key = item.Key;
					Type value = item.Value;
					EditorUtils.RegistryObserver registryObserver = exporterObserver.UpdateError(key);
					if (registryObserver.m_WriterObserver)
					{
						Type type = value;
						if ((object)type == null)
						{
							continue;
						}
						if (!(type == typeof(bool)))
						{
							if (!(type == typeof(int)))
							{
								if (type == typeof(float))
								{
									configurationServer[key] = registryObserver._PrinterObserver;
								}
								else if (type == typeof(string))
								{
									configurationServer[key] = registryObserver.importerObserver;
								}
							}
							else
							{
								configurationServer[key] = (int)registryObserver._PrinterObserver;
							}
						}
						else
						{
							configurationServer[key] = registryObserver._RequestObserver;
						}
					}
					else
					{
						StartContext(key);
					}
				}
			}
			catch (Exception arg)
			{
				$"Failed to load settings from {_IteratorServer}:\n{arg}".LoginResolver(LogType.Error);
				DefineContext();
			}
		}
		_ProcServer = false;
	}

	internal static bool FlushStatus()
	{
		return RegisterStatus == null;
	}
}
