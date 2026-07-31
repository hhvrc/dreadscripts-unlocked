using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class RenameOverlayWrapper
{
	private static bool initialized;

	private static Type renameOverlayType;

	private static MethodInfo beginRenameMethod;

	private static MethodInfo endRenameMethod;

	private static MethodInfo isRenamingMethod;

	private static MethodInfo onGUIMethod;

	private static MethodInfo onEventMethod;

	private static MethodInfo clearMethod;

	private static FieldInfo editFieldRectField;

	private static FieldInfo userAcceptedRenameField;

	private static FieldInfo originalNameField;

	private static FieldInfo nameField;

	private static FieldInfo userDataField;

	private static FieldInfo isWaitingForDelayField;

	private object instance;

	private bool instanceResolved;

	private readonly Func<object> instanceGetter;

	internal Action<bool> onEndRename;

	private static void EnsureInitialized()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		try
		{
			renameOverlayType = EditorUtils.FillRules("UnityEditor.RenameOverlay, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			beginRenameMethod = renameOverlayType.GetMethod("BeginRename");
			endRenameMethod = renameOverlayType.GetMethod("EndRename");
			isRenamingMethod = renameOverlayType.GetMethod("IsRenaming");
			onGUIMethod = renameOverlayType.GetMethod("OnGUI", new Type[1] { typeof(GUIStyle) });
			onEventMethod = renameOverlayType.GetMethod("OnEvent");
			clearMethod = renameOverlayType.GetMethod("Clear");
			editFieldRectField = renameOverlayType.RestartList("m_EditFieldRect");
			userAcceptedRenameField = renameOverlayType.RestartList("m_UserAcceptedRename");
			originalNameField = renameOverlayType.RestartList("m_OriginalName");
			nameField = renameOverlayType.RestartList("m_Name");
			userDataField = renameOverlayType.RestartList("m_UserData");
			isWaitingForDelayField = renameOverlayType.RestartList("m_IsWaitingForDelay");
		}
		catch (Exception)
		{
			Debug.LogError("Rename Overlay Wrapper has failed to initialize!");
			throw;
		}
	}

	internal RenameOverlayWrapper()
	{
		EnsureInitialized();
		Instance(Activator.CreateInstance(renameOverlayType));
	}

	internal RenameOverlayWrapper(object value)
	{
		EnsureInitialized();
		Instance(value);
	}

	internal RenameOverlayWrapper(Func<object> i)
	{
		instanceGetter = i;
	}

	internal object ResolveInstance()
	{
		EnsureInitialized();
		instance = instanceGetter?.Invoke();
		instanceResolved = true;
		return instance;
	}

	[SpecialName]
	internal object Instance()
	{
		if (instance != null || instanceResolved)
		{
			return instance;
		}
		EnsureInitialized();
		ResolveInstance();
		return instance;
	}

	[SpecialName]
	internal void Instance(object reference)
	{
		instance = reference;
	}

	[SpecialName]
	internal Rect EditFieldRect()
	{
		return (Rect)editFieldRectField.GetValue(Instance());
	}

	[SpecialName]
	internal void EditFieldRect(Rect def)
	{
		editFieldRectField.SetValue(Instance(), def);
	}

	[SpecialName]
	internal bool UserAcceptedRename()
	{
		return (bool)userAcceptedRenameField.GetValue(Instance());
	}

	[SpecialName]
	internal bool IsRenaming()
	{
		object obj = Instance();
		return (bool)isRenamingMethod.Invoke(obj, null);
	}

	[SpecialName]
	internal int UserData()
	{
		return (int)userDataField.GetValue(Instance());
	}

	[SpecialName]
	internal void UserData(int key_Position)
	{
		userDataField.SetValue(Instance(), key_Position);
	}

	[SpecialName]
	internal bool IsWaitingForDelay()
	{
		return (bool)isWaitingForDelayField.GetValue(Instance());
	}

	[SpecialName]
	internal void IsWaitingForDelay(bool containslast)
	{
		isWaitingForDelayField.SetValue(Instance(), containslast);
	}

	[SpecialName]
	internal string Name()
	{
		return (string)nameField.GetValue(Instance());
	}

	[SpecialName]
	internal void Name(string key)
	{
		nameField.SetValue(Instance(), key);
	}

	[SpecialName]
	internal string OriginalName()
	{
		return (string)originalNameField.GetValue(Instance());
	}

	internal bool BeginRename(Rect first, string ivk, int minhelper, float map2)
	{
		bool result = BeginRename(ivk, minhelper, map2);
		object obj = Instance();
		editFieldRectField.SetValue(obj, first);
		return result;
	}

	internal bool BeginRename(string task, int indexvisitor, float helper)
	{
		object obj = Instance();
		return (bool)beginRenameMethod.Invoke(obj, new object[3] { task, indexvisitor, helper });
	}

	internal void EndRename(bool isconfig, bool iscfg = true)
	{
		if (IsRenaming())
		{
			object obj = Instance();
			endRenameMethod.Invoke(obj, new object[1] { isconfig });
			onEndRename?.Invoke(isconfig);
			if (iscfg)
			{
				Clear();
			}
		}
	}

	internal bool OnGUI(GUIStyle textFieldStyle = null)
	{
		object obj = Instance();
		return (bool)onGUIMethod.Invoke(obj, new object[1] { textFieldStyle });
	}

	internal bool OnEvent()
	{
		object obj = Instance();
		return (bool)onEventMethod.Invoke(obj, null);
	}

	internal void Clear()
	{
		object obj = Instance();
		clearMethod.Invoke(obj, null);
	}
}
