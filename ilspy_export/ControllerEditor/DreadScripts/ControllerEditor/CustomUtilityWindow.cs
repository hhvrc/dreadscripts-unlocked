using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal abstract class CustomUtilityWindow<T> : EditorWindow where T : CustomUtilityWindow<T>
{
	private static readonly PropertyInfo m_StatusPolicy = typeof(EditorWindow).GetProperty("position", BindingFlags.Instance | BindingFlags.Public);

	private T tokenPolicy;

	private bool _CodePolicy;

	internal bool m_DicPolicy = true;

	internal string invocationPolicy;

	private Vector2 m_RolePolicy;

	internal static object CancelClient;

	internal abstract string DreadScripts_002EControllerEditor_002ECustomUtilityWindow_003CDreadScripts_002EControllerEditor_002EControllerEditor_002EParameterRenameWindow_003E_002Etitle { get; }

	internal static T CloneHelper(bool compareparam = true, string vis = "")
	{
		ReflectHelper();
		T val = ScriptableObject.CreateInstance<T>();
		val.titleContent.text = val.DreadScripts_002EControllerEditor_002ECustomUtilityWindow_003CDreadScripts_002EControllerEditor_002EControllerEditor_002EParameterRenameWindow_003E_002Etitle;
		val._CodePolicy = compareparam;
		val.tokenPolicy = val;
		val.invocationPolicy = vis;
		return val;
	}

	private void OnGUI()
	{
		if (tokenPolicy == null)
		{
			Close();
			return;
		}
		using (new SpecificationThread(ref m_RolePolicy))
		{
			if (string.IsNullOrEmpty(invocationPolicy))
			{
			}
			DreadScripts_002EControllerEditor_002ECustomUtilityWindow_003CDreadScripts_002EControllerEditor_002EControllerEditor_002EParameterRenameWindow_003E_002EOnCustomGUI();
		}
		if (!_CodePolicy)
		{
			return;
		}
		using (new EditorGUI.DisabledScope(!m_DicPolicy))
		{
			if (ClassProperty.DisableQueue("Confirm"))
			{
				DeleteHelper();
			}
		}
	}

	internal void LoginHelper(Vector2 setup, Vector2 reg)
	{
		ShowUtility();
		base.position = new Rect(new Vector2(setup.x, setup.y), reg);
	}

	internal static void ReflectHelper()
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

	internal void DeleteHelper()
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
