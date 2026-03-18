using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class ThreadTests<T>
{
	internal readonly IList m_PolicyTests;

	private readonly ReorderableList m_SerializerTests;

	private object _PageTests;

	private readonly ReorderableList.ElementCallbackDelegate resolverTests;

	private readonly Action _PredicateTests;

	internal Action m_RulesTests;

	internal bool m_QueueTests = true;

	internal bool m_ErrorTests;

	internal static object CustomizeStruct;

	[SpecialName]
	internal int InterruptThread()
	{
		return m_SerializerTests.index = RegisterThread(m_SerializerTests.index);
	}

	[SpecialName]
	internal void ManageThread(int value_length)
	{
		m_SerializerTests.index = RegisterThread(value_length);
	}

	internal ThreadTests(Action last, IList caller, Action<ReorderableList> proc, ReorderableList.ElementCallbackDelegate attr2, ReorderableList.ElementHeightCallbackDelegate init3 = null)
	{
		_PredicateTests = last;
		m_PolicyTests = caller;
		resolverTests = attr2;
		m_SerializerTests = new ReorderableList(caller, typeof(T), draggable: true, displayHeader: false, displayAddButton: false, displayRemoveButton: false)
		{
			headerHeight = 1f,
			footerHeight = 0f,
			drawElementCallback = ChangeThread,
			onAddCallback = proc.Invoke
		};
		if (init3 != null)
		{
			m_SerializerTests.elementHeightCallback = init3;
		}
	}

	internal ThreadTests(string info, string pol, IList pool, Action<ReorderableList> item2, ReorderableList.ElementCallbackDelegate ivk3, ReorderableList.ElementHeightCallbackDelegate token4 = null)
		: this((Action)null, pool, item2, ivk3, token4)
	{
		ThreadTests<T> threadTests = this;
		_PredicateTests = delegate
		{
			threadTests.LogoutThread(info, pol);
			threadTests.PatchThread();
		};
	}

	private void ChangeThread(Rect param, int pol_low, bool allowcomp, bool skipfirst2)
	{
		if (m_PolicyTests.Count != 0 && pol_low >= 0 && pol_low < m_PolicyTests.Count)
		{
			if (!GUI.Button(new Rect(param.x + param.width - 28f, param.y + param.height / 2f - 8f, 32f, 18f), ClassProperty.DestroyError().m_DecoratorProcessor, ClassProperty.CalcError()._TemplateProcessor))
			{
				Rect rect = new Rect(param);
				rect.width = param.width - 29f;
				Rect rect2 = rect;
				resolverTests(rect2, pol_low, allowcomp, skipfirst2);
			}
			else
			{
				m_PolicyTests.RemoveAt(pol_low);
			}
		}
	}

	internal void SortThread()
	{
		bool flag = m_PolicyTests.Count == 0;
		if (m_RulesTests != null)
		{
			object obj = ((!flag) ? m_PolicyTests[InterruptThread()] : null);
			if (obj != _PageTests)
			{
				_PageTests = obj;
				m_RulesTests();
			}
		}
		if (_PredicateTests != null)
		{
			using (new EditorGUILayout.HorizontalScope("RL Header"))
			{
				_PredicateTests();
			}
		}
		if (m_QueueTests && (!flag || m_ErrorTests))
		{
			m_SerializerTests.DoLayoutList();
		}
	}

	internal int RegisterThread(int var1_X)
	{
		return Mathf.Clamp(var1_X, 0, m_PolicyTests.Count - 1);
	}

	internal void LogoutThread(string def, string vis = null)
	{
		GUILayout.Label(def, EditorStyles.boldLabel);
		if (!string.IsNullOrEmpty(vis))
		{
			GUILayout.Label(new GUIContent(ClassProperty.DestroyError()._AccountProcessor.CompareHelper(), vis), GUILayout.Width(14f), GUILayout.Height(18f));
		}
	}

	internal void PatchThread(bool rejectv = true, bool writeattr = true)
	{
		if (writeattr)
		{
			GUILayout.FlexibleSpace();
		}
		if (rejectv)
		{
			m_QueueTests = ClassProperty.ExcludeQueue(m_QueueTests, (!m_QueueTests) ? ClassProperty.DestroyError().m_StatusProcessor : ClassProperty.DestroyError()._RefProcessor, EditorStyles.label, GUILayout.Width(18f), GUILayout.Height(18f));
		}
		using (new EditorGUI.DisabledScope(!m_QueueTests))
		{
			if (ClassProperty.RestartQueue(EditorGUIUtility.IconContent("d_ol_plus"), GUI.skin.label, GUILayout.Width(18f)))
			{
				m_SerializerTests.onAddCallback(m_SerializerTests);
			}
		}
	}

	internal static bool SearchStruct()
	{
		return CustomizeStruct == null;
	}
}
