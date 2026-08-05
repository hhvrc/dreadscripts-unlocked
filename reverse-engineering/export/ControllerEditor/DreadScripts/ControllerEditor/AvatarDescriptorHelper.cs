using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace DreadScripts.ControllerEditor;

internal static class AvatarDescriptorHelper
{
	internal static VRCAvatarDescriptor[] selectedAvatars = new VRCAvatarDescriptor[1];

	internal static bool[] isHumanoid = new bool[1];

	internal static bool[] hasActionLayerBug = new bool[1];

	internal static VRCAvatarDescriptor[] sceneAvatars;

	internal static Action<int> onAvatarChanged;

	[SpecialName]
	public static VRCAvatarDescriptor Avatar()
	{
		return selectedAvatars[0];
	}

	[SpecialName]
	public static void Avatar(VRCAvatarDescriptor res)
	{
		selectedAvatars[0] = res;
	}

	public static void RefreshAvatars(Func<VRCAvatarDescriptor, bool> reference = null, Action cont = null)
	{
		for (int i = 0; i < selectedAvatars.Length; i++)
		{
			VRCAvatarDescriptor vRCAvatarDescriptor = selectedAvatars[i];
			if (vRCAvatarDescriptor != null && !vRCAvatarDescriptor.gameObject.activeInHierarchy)
			{
				selectedAvatars[i] = null;
			}
		}
		bool flag = false;
		sceneAvatars = UnityEngine.Object.FindObjectsOfType<VRCAvatarDescriptor>();
		if (sceneAvatars.Length == 0)
		{
			return;
		}
		for (int j = 0; j < selectedAvatars.Length; j++)
		{
			if (selectedAvatars[j] != null)
			{
				continue;
			}
			if (reference != null)
			{
				selectedAvatars[j] = sceneAvatars.FirstOrDefault(reference);
				flag |= (bool)selectedAvatars[j];
			}
			if (!selectedAvatars[j])
			{
				selectedAvatars[j] = sceneAvatars.FirstOrDefault((VRCAvatarDescriptor a) => !selectedAvatars.Contains(a));
				flag |= (bool)selectedAvatars[j];
			}
		}
		if (flag)
		{
			cont?.Invoke();
			onAvatarChanged?.Invoke(0);
		}
	}

	public static bool RefreshIssues(int version_res = 0, bool isconnection = true, bool fieldinstall = true)
	{
		if (!selectedAvatars[version_res])
		{
			return false;
		}
		isHumanoid[version_res] = selectedAvatars[version_res].IsHumanoid();
		hasActionLayerBug[version_res] = selectedAvatars[version_res].baseAnimationLayers.Length > 3 && selectedAvatars[version_res].baseAnimationLayers[3].type == selectedAvatars[version_res].baseAnimationLayers[4].type;
		if (!fieldinstall || !hasActionLayerBug[version_res])
		{
			if (isconnection)
			{
				return !isHumanoid[version_res];
			}
			return false;
		}
		return true;
	}

	public static bool DrawAvatarSelector(int res = 0, bool addb = true, bool includec = true, string pred2 = "Avatar", string spec3 = "The Targeted VRCAvatar", Action pred4 = null)
	{
		selectedAvatars[res] = DrawAvatarPopup(res, pred2, spec3, pred4);
		if ((bool)selectedAvatars[res])
		{
			return DrawWarnings(res, addb, includec);
		}
		return false;
	}

	public static VRCAvatarDescriptor DrawAvatarPopup(int v = 0, string ord = "Avatar", string res = "The Targeted VRCAvatar", Action res2 = null)
	{
		using (new GUILayout.HorizontalScope())
		{
			GUIContent label = new GUIContent(ord, res);
			if (sceneAvatars != null && sceneAvatars.Length != 0)
			{
				using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
				int num = EditorGUILayout.Popup(label, selectedAvatars[v] ? Array.IndexOf(sceneAvatars, selectedAvatars[v]) : (-1), (from x in sceneAvatars
					where x
					select x.name).ToArray());
				if (changeCheckScope.changed)
				{
					selectedAvatars[v] = sceneAvatars[num];
					EditorGUIUtility.PingObject(selectedAvatars[v]);
					res2?.Invoke();
					onAvatarChanged?.Invoke(v);
				}
			}
			else
			{
				EditorGUILayout.LabelField(label, new GUIContent("No Avatar Descriptors Found"));
			}
		}
		return selectedAvatars[v];
	}

	private static bool DrawWarnings(int idx_def = 0, bool testsecond = true, bool allowpool = true)
	{
		if (!allowpool || !DrawActionLayerWarning(idx_def))
		{
			if (!testsecond)
			{
				return false;
			}
			return DrawHumanoidWarning(idx_def);
		}
		return true;
	}

	private static bool DrawActionLayerWarning(int flagsv = 0)
	{
		VRCAvatarDescriptor vRCAvatarDescriptor = selectedAvatars[flagsv];
		if ((bool)vRCAvatarDescriptor)
		{
			VRCAvatarDescriptor.CustomAnimLayer[] baseAnimationLayers = vRCAvatarDescriptor.baseAnimationLayers;
			if (baseAnimationLayers.Length > 3)
			{
				if (baseAnimationLayers[3].type != baseAnimationLayers[4].type)
				{
					return false;
				}
				EditorGUILayout.HelpBox("Your Avatar's Action playable layer is set as FX. This is an uncommon bug.", MessageType.Error);
				if (EditorUtils.Button("Fix"))
				{
					vRCAvatarDescriptor.baseAnimationLayers[3].type = VRCAvatarDescriptor.AnimLayerType.Action;
					EditorUtility.SetDirty(vRCAvatarDescriptor);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private static bool DrawHumanoidWarning(int setup = 0)
	{
		if (!selectedAvatars[setup])
		{
			return false;
		}
		if (!isHumanoid[setup])
		{
			EditorGUILayout.HelpBox("Your Avatar's descriptor is set as Non-Humanoid! Please make sure that your Avatar's rig is Humanoid.", MessageType.Error);
			return true;
		}
		return false;
	}

	public static bool IsHumanoid(this VRCAvatarDescriptor def)
	{
		return def.baseAnimationLayers.Length > 3;
	}
}
