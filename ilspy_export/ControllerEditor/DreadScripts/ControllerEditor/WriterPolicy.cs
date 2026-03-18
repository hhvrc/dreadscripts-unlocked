using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class WriterPolicy
{
	private readonly GameObject paramsPolicy;

	private readonly Transform m_ListenerPolicy;

	internal static WriterPolicy PrintDecorator;

	internal WriterPolicy(Transform info)
		: this(info.position, info.rotation, info.localScale, info.parent)
	{
	}

	internal WriterPolicy(Vector3? asset, Quaternion? vis, Vector3? rule, Transform first2)
	{
		paramsPolicy = new GameObject("Mirror Transform")
		{
			hideFlags = (HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild)
		};
		m_ListenerPolicy = paramsPolicy.transform;
		m_ListenerPolicy.parent = first2;
		m_ListenerPolicy.position = asset ?? Vector3.zero;
		m_ListenerPolicy.rotation = vis ?? Quaternion.identity;
		m_ListenerPolicy.localScale = rule ?? Vector3.one;
	}

	internal void FlushHelper()
	{
		if ((bool)paramsPolicy)
		{
			Object.DestroyImmediate(paramsPolicy);
		}
	}

	public static implicit operator Transform(WriterPolicy instance)
	{
		return instance.m_ListenerPolicy;
	}

	internal static bool ResolveDecorator()
	{
		return PrintDecorator == null;
	}
}
