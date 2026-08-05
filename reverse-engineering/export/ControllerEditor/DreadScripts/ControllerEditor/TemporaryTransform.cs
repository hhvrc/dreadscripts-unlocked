using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class TemporaryTransform
{
	private readonly GameObject gameObject;

	private readonly Transform transform;

	internal TemporaryTransform(Transform info)
		: this(info.position, info.rotation, info.localScale, info.parent)
	{
	}

	internal TemporaryTransform(Vector3? asset, Quaternion? vis, Vector3? rule, Transform first2)
	{
		gameObject = new GameObject("Mirror Transform")
		{
			hideFlags = (HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild)
		};
		transform = gameObject.transform;
		transform.parent = first2;
		transform.position = asset ?? Vector3.zero;
		transform.rotation = vis ?? Quaternion.identity;
		transform.localScale = rule ?? Vector3.one;
	}

	internal void Destroy()
	{
		if ((bool)gameObject)
		{
			Object.DestroyImmediate(gameObject);
		}
	}

	public static implicit operator Transform(TemporaryTransform instance)
	{
		return instance.transform;
	}
}
