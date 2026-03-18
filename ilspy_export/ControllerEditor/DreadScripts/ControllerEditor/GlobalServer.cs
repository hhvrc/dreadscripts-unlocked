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
				if (ClassProperty.DisableQueue("Revert"))
				{
					AwakeContext();
				}
				if (ClassProperty.DisableQueue("Save"))
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
		string key = setup.ToString();
		while (true)
		{
			configurationServer[key] = selection;
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
		string value2 = ClassProperty.InvokeList(list);
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
				ClassProperty.ExporterObserver exporterObserver = new ClassProperty.ExporterObserver(ident);
				foreach (KeyValuePair<string, Type> item in _TaskServer)
				{
					string key = item.Key;
					Type value = item.Value;
					ClassProperty.RegistryObserver registryObserver = exporterObserver.UpdateError(key);
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
