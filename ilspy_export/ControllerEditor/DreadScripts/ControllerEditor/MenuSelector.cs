using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace DreadScripts.ControllerEditor;

internal class MenuSelector : EditorWindow
{
	private AuthenticationThread _ExceptionThread;

	private bool objectThread = true;

	private int utilsThread = 1;

	private Action<VRCExpressionsMenu> m_ValThread;

	internal static HashSet<VRCExpressionsMenu> valueThread = new HashSet<VRCExpressionsMenu>();

	internal static VRCExpressionsMenu m_MerchantThread;

	internal static MenuSelector MapStatus;

	[SpecialName]
	private VRCExpressionsMenu DefineRecord()
	{
		return _ExceptionThread.composerThread;
	}

	[SpecialName]
	private VRCExpressionsMenu[] ReadRecord()
	{
		return _ExceptionThread.poolThread;
	}

	internal static void InvokeRecord(VRCExpressionsMenu reference, Action<VRCExpressionsMenu> caller, int column_c = 1, bool istoken2 = true)
	{
		MenuSelector window = EditorWindow.GetWindow<MenuSelector>("Menu Selector");
		VRCExpressionsMenu asset = ((m_MerchantThread == null) ? reference : ((!valueThread.Contains(reference)) ? reference : m_MerchantThread));
		window._ExceptionThread = new AuthenticationThread(asset);
		window._ExceptionThread.repositoryThread = window.FindRecord;
		window.m_ValThread = caller;
		window.utilsThread = column_c;
		window.objectThread = istoken2;
	}

	private void FindRecord(VRCExpressionsMenu info)
	{
		if (!(info == null) && (bool)info.QueryError(utilsThread))
		{
			m_ValThread?.Invoke(info);
			Close();
		}
	}

	private void OnGUI()
	{
		if (_ExceptionThread == null)
		{
			Close();
			return;
		}
		Rect rect = new Rect(0f, 0f, base.position.width, base.position.height - 20f);
		Rect rect2 = new Rect(0f, base.position.height - 30f, base.position.width, 30f);
		Rect rect3 = new Rect(0f, base.position.height - 21f, base.position.width, 1f);
		Rect def = new Rect(rect2);
		GUIContent content = new GUIContent("Show All Controls", "Shows all controls, including those that are not submenus.");
		def.x += 4f;
		def.y += def.height / 2f - 9f;
		def.height = 18f;
		Rect rect4 = def.SortResolver(120f, isfield: true);
		Rect rect5 = def.PatchResolver(80f, isserv: true, 4f, isvisitor3: true);
		Rect spec = def.PatchResolver(20f, isserv: true, 4f, isvisitor3: true);
		_ExceptionThread.OnGUI(rect);
		EditorGUI.DrawRect(rect3, new Color(0.2f, 0.2f, 0.2f));
		EditorGUI.DrawRect(rect2, new Color(0.25f, 0.25f, 0.25f));
		EditorGUI.BeginChangeCheck();
		bool parameterThread = GUI.Toggle(rect4, AuthenticationThread.m_ParameterThread, content);
		if (EditorGUI.EndChangeCheck())
		{
			AuthenticationThread.m_ParameterThread = parameterThread;
			_ExceptionThread.Reload();
		}
		using (new EditorGUI.DisabledScope(!_ExceptionThread.HasSelection() || _ExceptionThread.GetSelection().All((int ident) => !_ExceptionThread.m_ReponseThread.ContainsKey(ident) || !_ExceptionThread.m_ReponseThread[ident].RestartError(1))))
		{
			if (GUI.Button(rect5, "Select"))
			{
				int key = _ExceptionThread.GetSelection().FirstOrDefault((int last_max) => _ExceptionThread.m_ReponseThread.ContainsKey(last_max) && (bool)_ExceptionThread.m_ReponseThread[last_max].RestartError(1));
				VRCExpressionsMenu.Control control = _ExceptionThread.m_ReponseThread[key];
				FindRecord(control.subMenu);
			}
		}
		if (!objectThread)
		{
			return;
		}
		ClassProperty.StartQueue(spec);
		if (GUI.Button(spec, ClassProperty.DestroyError().invocationProcessor, ClassProperty.CalcError().broadcasterProcessor))
		{
			ClassProperty.ConcatList(null, typeof(VRCExpressionsMenu), null, null, loaddef3: false, null, delegate(UnityEngine.Object first)
			{
				FindRecord(first as VRCExpressionsMenu);
			});
		}
	}

	[CompilerGenerated]
	private bool ExcludeRecord(int ident)
	{
		if (_ExceptionThread.m_ReponseThread.ContainsKey(ident))
		{
			return !_ExceptionThread.m_ReponseThread[ident].RestartError(1);
		}
		return true;
	}

	[CompilerGenerated]
	private bool InitRecord(int last_max)
	{
		if (!_ExceptionThread.m_ReponseThread.ContainsKey(last_max))
		{
			return false;
		}
		return _ExceptionThread.m_ReponseThread[last_max].RestartError(1);
	}

	[CompilerGenerated]
	private void VisitRecord(UnityEngine.Object first)
	{
		FindRecord(first as VRCExpressionsMenu);
	}

	internal static bool AddStatus()
	{
		return (object)MapStatus == null;
	}
}
