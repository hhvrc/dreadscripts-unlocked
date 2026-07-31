using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal abstract class UtilityWindowBase<T> : EditorWindow where T : DreadScripts.ControllerEditor.UtilityWindowBase<T>
{
	private static readonly PropertyInfo positionProperty = typeof(EditorWindow).GetProperty("position", BindingFlags.Instance | BindingFlags.Public);

	private T self;

	private bool showConfirmButton;

	internal bool canConfirm = true;

	internal string helpMessage;

	private Vector2 scrollPosition;

	internal static object CancelClient;

	internal abstract string DreadScripts_002EControllerEditor_002ECustomUtilityWindow_003CDreadScripts_002EControllerEditor_002EControllerEditor_002EParameterRenameWindow_003E_002Etitle { get; }

	internal static T Create(bool compareparam = true, string vis = "")
	{
		CloseAll();
		T val = ScriptableObject.CreateInstance<T>();
		val.titleContent.text = val.DreadScripts_002EControllerEditor_002ECustomUtilityWindow_003CDreadScripts_002EControllerEditor_002EControllerEditor_002EParameterRenameWindow_003E_002Etitle;
		val.showConfirmButton = compareparam;
		val.self = val;
		val.helpMessage = vis;
		return val;
	}

	private void OnGUI()
	{
		if (self == null)
		{
			Close();
			return;
		}
		using (new ScrollViewScope(ref scrollPosition))
		{
			if (!string.IsNullOrEmpty(helpMessage))
			{
				EditorGUILayout.HelpBox(helpMessage, MessageType.Info);
			}
			DreadScripts_002EControllerEditor_002ECustomUtilityWindow_003CDreadScripts_002EControllerEditor_002EControllerEditor_002EParameterRenameWindow_003E_002EOnCustomGUI();
		}
		if (!showConfirmButton)
		{
			return;
		}
		using (new EditorGUI.DisabledScope(!canConfirm))
		{
			if (EditorUtils.DisableQueue("Confirm"))
			{
				Confirm();
			}
		}
	}

	internal void ShowAt(Vector2 setup, Vector2 reg)
	{
		ShowUtility();
		base.position = new Rect(new Vector2(setup.x, setup.y), reg);
	}

	internal static void CloseAll()
	{
		T[] array = Resources.FindObjectsOfTypeAll<T>();
		foreach (T val in array)
		{
			try
			{
				val.Close();
			}
			catch
			{
				Object.DestroyImmediate(val);
			}
		}
	}

	internal void Confirm()
	{
		OnCustomConfirm();
		Close();
	}

	internal abstract void DreadScripts_002EControllerEditor_002ECustomUtilityWindow_003CDreadScripts_002EControllerEditor_002EControllerEditor_002EParameterRenameWindow_003E_002EOnCustomGUI();

	internal abstract void OnCustomConfirm();

	internal static bool RestartClient()
	{
		return CancelClient == null;
	}
}
