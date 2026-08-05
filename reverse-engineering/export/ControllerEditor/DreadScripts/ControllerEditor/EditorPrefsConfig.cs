using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

[DefaultMember("Item")]
internal class EditorPrefsConfig
{
	internal class PrefsChangeScope : IDisposable
	{
		private readonly EditorPrefsConfig config;

		private readonly EditorGUI.ChangeCheckScope changeCheck;

		public PrefsChangeScope(EditorPrefsConfig key)
		{
			config = key;
			changeCheck = new EditorGUI.ChangeCheckScope();
		}

		public void Dispose()
		{
			if (changeCheck.changed)
			{
				config.Save();
			}
		}
	}

	internal readonly Dictionary<string, Type> settingTypes;

	internal readonly Dictionary<string, object> defaultValues;

	internal readonly Dictionary<string, string> tooltips;

	internal readonly string prefsKey;

	internal readonly Action drawAction;

	internal IDictionary<string, object> values;

	internal bool isDirty;

	internal bool hasTooltips;

	private readonly HashSet<string> hiddenKeys = new HashSet<string>();

	internal EditorPrefsConfig(string info, Dictionary<Enum, Type> pol, Dictionary<Enum, object> proc, Dictionary<Enum, string> item2 = null, Action reference3 = null)
		: this(info, pol.ToDictionary((KeyValuePair<Enum, Type> kvp) => kvp.Key.ToString(), (KeyValuePair<Enum, Type> kvp) => kvp.Value), proc.ToDictionary((KeyValuePair<Enum, object> kvp) => kvp.Key.ToString(), (KeyValuePair<Enum, object> kvp) => kvp.Value), item2?.ToDictionary((KeyValuePair<Enum, string> kvp) => kvp.Key.ToString(), (KeyValuePair<Enum, string> kvp) => kvp.Value), reference3)
	{
	}

	internal EditorPrefsConfig(string reference, Dictionary<string, Type> pol, Dictionary<string, object> state, Dictionary<string, string> ident2 = null, Action pred3 = null)
	{
		prefsKey = reference;
		drawAction = pred3 ?? new Action(DrawDefault);
		settingTypes = pol;
		defaultValues = state;
		tooltips = ident2;
		hasTooltips = ident2 != null;
		Load();
	}

	internal void Draw()
	{
		drawAction();
	}

	internal void DrawDefault()
	{
		KeyValuePair<string, object>[] array = values.Select((KeyValuePair<string, object> kvp) => kvp).ToArray();
		EditorGUI.BeginChangeCheck();
		KeyValuePair<string, object>[] array2 = array;
		for (int num = 0; num < array2.Length; num++)
		{
			KeyValuePair<string, object> keyValuePair = array2[num];
			string key = keyValuePair.Key;
			if (hiddenKeys.Contains(key))
			{
				continue;
			}
			object value = keyValuePair.Value;
			string value2;
			GUIContent label = new GUIContent(ObjectNames.NicifyVariableName(key), (!hasTooltips || !tooltips.TryGetValue(key, out value2)) ? string.Empty : value2);
			if (!settingTypes.TryGetValue(key, out var value3))
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
				values[key] = EditorGUILayout.Toggle(label, (bool)value);
			}
			else if (!(type == typeof(int)))
			{
				if (type == typeof(float))
				{
					values[key] = EditorGUILayout.FloatField(label, (float)value);
				}
				else if (type == typeof(string))
				{
					values[key] = EditorGUILayout.TextField(label, (string)value);
				}
			}
			else
			{
				values[key] = EditorGUILayout.IntField(label, (int)value);
			}
		}
		if (EditorGUI.EndChangeCheck())
		{
			isDirty = true;
		}
		using (new GUILayout.HorizontalScope())
		{
			using (new EditorGUI.DisabledScope(!isDirty))
			{
				if (EditorUtils.Button("Revert"))
				{
					Load();
				}
				if (EditorUtils.Button("Save"))
				{
					Save();
				}
			}
		}
	}

	internal void Hide(string ident)
	{
		hiddenKeys.Add(ident);
	}

	internal void Hide(params string[] keys)
	{
		hiddenKeys.UnionWith(keys);
	}

	internal void Hide(Enum info)
	{
		Hide(new string[1] { info.ToString() });
	}

	internal void Hide(params Enum[] keys)
	{
		Hide(keys.Select((Enum k) => k.ToString()).ToArray());
	}

	internal void Show(string res)
	{
		hiddenKeys.Remove(res);
	}

	internal void Show(params string[] keys)
	{
		hiddenKeys.ExceptWith(keys);
	}

	internal void Show(Enum info)
	{
		Show(info.ToString());
	}

	internal void Show(params Enum[] keys)
	{
		Show(keys.Select((Enum k) => k.ToString()).ToArray());
	}

	private void LoadDefaults()
	{
		values = new Dictionary<string, object>();
		foreach (string key in settingTypes.Keys)
		{
			LoadDefault(key);
		}
		isDirty = false;
	}

	private void LoadDefault(string info)
	{
		if (settingTypes.TryGetValue(info, out var value))
		{
			object cust;
			if (defaultValues != null && defaultValues.TryGetValue(info, out var value2))
			{
				values[info] = value2;
			}
			else if (TryGetDefaultValue(value, out cust))
			{
				values[info] = cust;
			}
		}
	}

	private bool TryGetDefaultValue(Type v, out object cust)
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
	internal object Item(string config)
	{
		return values[config];
	}

	[SpecialName]
	internal void Item(string instance, object selection)
	{
		values[instance] = selection;
		isDirty = true;
	}

	[SpecialName]
	internal object Item(Enum asset)
	{
		return values[asset.ToString()];
	}

	[SpecialName]
	internal void Item(Enum setup, object selection)
	{
		string key = setup.ToString();
		values[key] = selection;
		isDirty = true;
	}

	internal T GetValue<T>(string first)
	{
		return (T)values[first];
	}

	internal T GetValue<T>(Enum init)
	{
		return (T)values[init.ToString()];
	}

	internal void Save()
	{
		List<(string, string)> list = new List<(string, string)>();
		foreach (KeyValuePair<string, object> value3 in values)
		{
			string key = value3.Key;
			object value = value3.Value;
			list.Add((key, value.ToString()));
		}
		string value2 = EditorUtils.InvokeList(list);
		EditorPrefs.SetString(prefsKey, value2);
		isDirty = false;
	}

	internal void Load()
	{
		if (!EditorPrefs.HasKey(prefsKey))
		{
			LoadDefaults();
		}
		else
		{
			string ident = EditorPrefs.GetString(prefsKey);
			try
			{
				values = new Dictionary<string, object>();
				EditorUtils.ExporterObserver exporterObserver = new EditorUtils.ExporterObserver(ident);
				foreach (KeyValuePair<string, Type> settingType in settingTypes)
				{
					string key = settingType.Key;
					Type value = settingType.Value;
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
									values[key] = registryObserver._PrinterObserver;
								}
								else if (type == typeof(string))
								{
									values[key] = registryObserver.importerObserver;
								}
							}
							else
							{
								values[key] = (int)registryObserver._PrinterObserver;
							}
						}
						else
						{
							values[key] = registryObserver._RequestObserver;
						}
					}
					else
					{
						LoadDefault(key);
					}
				}
			}
			catch (Exception arg)
			{
				$"Failed to load settings from {prefsKey}:\n{arg}".Log(LogType.Error);
				LoadDefaults();
			}
		}
		isDirty = false;
	}
}
