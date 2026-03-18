using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DreadScripts.Common.SupportThankies;

internal class Rules
{
	internal readonly string m_Queue;

	internal readonly List<Filter> error;

	internal readonly List<Filter> m_Setter;

	internal readonly List<Filter> m_Connection;

	internal readonly Customer m_Consumer;

	internal readonly Customer.TextureLayoutMethod adapter;

	internal readonly Color? m_Interpreter;

	internal readonly Color? _Watcher;

	internal readonly Color? candidate;

	internal readonly string _Product;

	internal readonly string _Expression;

	internal readonly object system = Definition.SetWrapper(1f, 1f, 1f);

	internal Rect worker;

	private static Rules ListCode;

	internal Rules(string spec)
	{
		m_Queue = spec;
		StartWrapper("onclick", out _Expression);
		_Product = ((!StartWrapper("tooltip", out var col)) ? Test.decorator.DestroyWrapper() : col);
		if (!StartWrapper("bgtype", out var col2) || !Enum.TryParse<Customer.TextureLayoutMethod>(col2, ignoreCase: true, out adapter))
		{
			adapter = Customer.TextureLayoutMethod.Pattern;
		}
		if (StartWrapper("name", out var col3))
		{
			error = Filter.RemoveWrapper(col3);
		}
		if (StartWrapper("prefix", out var col4))
		{
			m_Setter = Filter.RemoveWrapper(col4);
		}
		if (StartWrapper("suffix", out var col5))
		{
			m_Connection = Filter.RemoveWrapper(col5);
		}
		if (StartWrapper("namecolor", out var col6))
		{
			candidate = ((!ColorUtility.TryParseHtmlString(col6, out var color)) ? ((Color?)null) : new Color?(color));
		}
		if (StartWrapper("bgcolor", out var col7))
		{
			m_Interpreter = (ColorUtility.TryParseHtmlString(col7, out var color2) ? new Color?(color2) : ((Color?)null));
		}
		if (StartWrapper("bordercolor", out var col8))
		{
			_Watcher = ((!ColorUtility.TryParseHtmlString(col8, out var color3)) ? ((Color?)null) : new Color?(color3));
		}
		if (StartWrapper("bgimage", out var col9))
		{
			m_Consumer = new Customer(col9, overridesecond: true, col9);
		}
	}

	internal void DefineWrapper(float v = 20f)
	{
		Rect wrapper = worker.GetWrapper(2f);
		using (new Serializer(Serializer.ColoringType.General, (!m_Interpreter.HasValue) ? GUI.color : GUI.color.IncludeWrapper(m_Interpreter.Value)))
		{
			m_Consumer?.FlushWrapper(wrapper, adapter);
		}
		Info.RunWrapper(wrapper, (m_Consumer != null) ? Color.clear : (m_Interpreter ?? new Color(0f, 0f, 0f, 0.4f)), _Watcher.GetValueOrDefault(), 1f);
		using (new GUILayout.VerticalScope())
		{
			using (new GUILayout.VerticalScope())
			{
				GUILayout.FlexibleSpace();
				Definition.EnableWrapper(system, null, false);
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(8f);
					if (m_Setter != null)
					{
						foreach (Filter item in m_Setter)
						{
							item.ReadWrapper(Exception.RegisterWrapper().repository, v);
						}
					}
					else
					{
						GUILayout.Label(GUIContent.none);
					}
				}
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.FlexibleSpace();
					if (error != null)
					{
						using (new Serializer(Serializer.ColoringType.General, candidate ?? GUI.color))
						{
							foreach (Filter item2 in error)
							{
								item2.ReadWrapper(Exception.RegisterWrapper().composer, v);
							}
						}
					}
					GUILayout.FlexibleSpace();
				}
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.FlexibleSpace();
					if (m_Connection == null)
					{
						GUILayout.Label(GUIContent.none);
					}
					else
					{
						foreach (Filter item3 in m_Connection)
						{
							item3.ReadWrapper(Exception.RegisterWrapper().m_Mapping, v);
						}
					}
					GUILayout.Space(8f);
				}
				Definition.PublishWrapper();
				GUILayout.FlexibleSpace();
			}
			if (Event.current.type == EventType.Repaint)
			{
				worker = GUILayoutUtility.GetLastRect();
			}
			GUILayout.Space(4f);
		}
		GUI.Label(worker, new GUIContent(string.Empty, _Product));
		if (!string.IsNullOrWhiteSpace(_Expression) && Info.PushWrapper(worker))
		{
			Application.OpenURL(_Expression);
		}
	}

	internal bool StartWrapper(string v, out string col)
	{
		string pattern = "<" + v + "=(.*?)>(?:<|$)";
		Match match = Regex.Match(m_Queue, pattern);
		bool success = match.Success;
		col = (success ? match.Groups[1].Value : null);
		return success;
	}

	internal static bool CalcCode()
	{
		return ListCode == null;
	}
}
