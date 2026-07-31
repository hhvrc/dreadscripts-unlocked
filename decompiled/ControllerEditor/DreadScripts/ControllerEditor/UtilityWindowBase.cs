using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal abstract class UtilityWindowBase<T> : EditorWindow where T : DreadScripts.ControllerEditor.UtilityWindowBase<T>
{
	private static readonly PropertyInfo positionProperty;

	private T self;

	private bool showConfirmButton;

	internal bool canConfirm;

	internal string helpMessage;

	private Vector2 scrollPosition;

	internal static object CancelClient;

	internal abstract string DreadScripts_002EControllerEditor_002ECustomUtilityWindow_003CDreadScripts_002EControllerEditor_002EControllerEditor_002EParameterRenameWindow_003E_002Etitle { get; }

	internal static T Create(bool compareparam = true, string vis = "")
	{
		DreadScripts.ControllerEditor.UtilityWindowBase<T>.ReflectHelper();
		T val = ScriptableObject.CreateInstance<T>();
		val.titleContent.text = val.DreadScripts_002EControllerEditor_002ECustomUtilityWindow_003CDreadScripts_002EControllerEditor_002EControllerEditor_002EParameterRenameWindow_003E_002Etitle;
		((DreadScripts.ControllerEditor.UtilityWindowBase<T>)val)._CodePolicy = compareparam;
		((DreadScripts.ControllerEditor.UtilityWindowBase<T>)val).tokenPolicy = val;
		((DreadScripts.ControllerEditor.UtilityWindowBase<T>)val).invocationPolicy = vis;
		return val;
	}

	private void OnGUI()
	{
		if (this.tokenPolicy == null)
		{
			Close();
			return;
		}
		using (new ScrollViewScope(ref this.m_RolePolicy))
		{
			if (!string.IsNullOrEmpty(this.invocationPolicy))
			{
				EditorGUILayout.HelpBox(this.invocationPolicy, MessageType.Info);
			}
			DreadScripts_002EControllerEditor_002ECustomUtilityWindow_003CDreadScripts_002EControllerEditor_002EControllerEditor_002EParameterRenameWindow_003E_002EOnCustomGUI();
		}
		if (!this._CodePolicy)
		{
			return;
		}
		using (new EditorGUI.DisabledScope(!this.m_DicPolicy))
		{
			if (EditorUtils.DisableQueue("Confirm"))
			{
				this.DeleteHelper();
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

	protected UtilityWindowBase()
	{
		this.m_DicPolicy = true;
		base._002Ector();
	}

	static UtilityWindowBase()
	{
		DreadScripts.ControllerEditor.UtilityWindowBase<T>.m_StatusPolicy = typeof(EditorWindow).GetProperty("position", BindingFlags.Instance | BindingFlags.Public);
	}

	internal static bool RestartClient()
	{
		return CancelClient == null;
	}
}
