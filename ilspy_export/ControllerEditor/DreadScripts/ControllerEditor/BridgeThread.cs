using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace DreadScripts.ControllerEditor;

internal static class BridgeThread
{
	internal static VRCAvatarDescriptor[] m_StrategyThread = new VRCAvatarDescriptor[1];

	internal static bool[] customerThread = new bool[1];

	internal static bool[] _DatabaseThread = new bool[1];

	internal static VRCAvatarDescriptor[] _ExporterThread;

	internal static Action<int> identifierThread;

	private static BridgeThread DefineStatus;

	[SpecialName]
	public static VRCAvatarDescriptor SearchContext()
	{
		return m_StrategyThread[0];
	}

	[SpecialName]
	public static void RevertContext(VRCAvatarDescriptor res)
	{
		m_StrategyThread[0] = res;
	}

	public static void ChangeContext(Func<VRCAvatarDescriptor, bool> reference = null, Action cont = null)
	{
		for (int i = 0; i < m_StrategyThread.Length; i++)
		{
			VRCAvatarDescriptor vRCAvatarDescriptor = m_StrategyThread[i];
			if (vRCAvatarDescriptor != null && !vRCAvatarDescriptor.gameObject.activeInHierarchy)
			{
				m_StrategyThread[i] = null;
			}
		}
		bool flag = false;
		_ExporterThread = UnityEngine.Object.FindObjectsOfType<VRCAvatarDescriptor>();
		if (_ExporterThread.Length == 0)
		{
			return;
		}
		for (int j = 0; j < m_StrategyThread.Length; j++)
		{
			if (m_StrategyThread[j] != null)
			{
				continue;
			}
			if (reference != null)
			{
				m_StrategyThread[j] = _ExporterThread.FirstOrDefault(reference);
				flag |= (bool)m_StrategyThread[j];
			}
			if (!m_StrategyThread[j])
			{
				m_StrategyThread[j] = _ExporterThread.FirstOrDefault((VRCAvatarDescriptor a) => !m_StrategyThread.Contains(a));
				flag |= (bool)m_StrategyThread[j];
			}
		}
		if (flag)
		{
			cont?.Invoke();
			identifierThread?.Invoke(0);
		}
	}

	public static bool SortContext(int version_res = 0, bool isconnection = true, bool fieldinstall = true)
	{
		if (!m_StrategyThread[version_res])
		{
			return false;
		}
		customerThread[version_res] = m_StrategyThread[version_res].PrintContext();
		_DatabaseThread[version_res] = m_StrategyThread[version_res].baseAnimationLayers.Length > 3 && m_StrategyThread[version_res].baseAnimationLayers[3].type == m_StrategyThread[version_res].baseAnimationLayers[4].type;
		if (!fieldinstall || !_DatabaseThread[version_res])
		{
			if (isconnection)
			{
				return !customerThread[version_res];
			}
			return false;
		}
		return true;
	}

	public static bool RegisterContext(int res = 0, bool addb = true, bool includec = true, string pred2 = "Avatar", string spec3 = "The Targeted VRCAvatar", Action pred4 = null)
	{
		m_StrategyThread[res] = LogoutContext(res, pred2, spec3, pred4);
		if ((bool)m_StrategyThread[res])
		{
			return PatchContext(res, addb, includec);
		}
		return false;
	}

	public static VRCAvatarDescriptor LogoutContext(int v = 0, string ord = "Avatar", string res = "The Targeted VRCAvatar", Action res2 = null)
	{
		using (new GUILayout.HorizontalScope())
		{
			GUIContent label = new GUIContent(ord, res);
			if (_ExporterThread != null && _ExporterThread.Length != 0)
			{
				using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
				int num = EditorGUILayout.Popup(label, m_StrategyThread[v] ? Array.IndexOf(_ExporterThread, m_StrategyThread[v]) : (-1), (from x in _ExporterThread
					where x
					select x.name).ToArray());
				if (changeCheckScope.changed)
				{
					m_StrategyThread[v] = _ExporterThread[num];
					EditorGUIUtility.PingObject(m_StrategyThread[v]);
					res2?.Invoke();
					identifierThread?.Invoke(v);
				}
			}
			else
			{
				EditorGUILayout.LabelField(label, new GUIContent("No Avatar Descriptors Found"));
			}
		}
		return m_StrategyThread[v];
	}

	private static bool PatchContext(int idx_def = 0, bool testsecond = true, bool allowpool = true)
	{
		if (!allowpool || !InterruptContext(idx_def))
		{
			if (!testsecond)
			{
				return false;
			}
			return ManageContext(idx_def);
		}
		return true;
	}

	private static bool InterruptContext(int flagsv = 0)
	{
		VRCAvatarDescriptor vRCAvatarDescriptor = m_StrategyThread[flagsv];
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
				if (ClassProperty.DisableQueue("Fix"))
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

	private static bool ManageContext(int setup = 0)
	{
		if (!m_StrategyThread[setup])
		{
			return false;
		}
		if (!customerThread[setup])
		{
			EditorGUILayout.HelpBox("Your Avatar's descriptor is set as Non-Humanoid! Please make sure that your Avatar's rig is Humanoid.", MessageType.Error);
			return true;
		}
		return false;
	}

	public static bool PrintContext(this VRCAvatarDescriptor def)
	{
		return def.baseAnimationLayers.Length > 3;
	}

	internal static bool EnableStatus()
	{
		return DefineStatus == null;
	}
}
