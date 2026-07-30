using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class TempGameObjectHierarchy
{
	internal readonly GameObject[] m_CollectionServer;

	private static TempGameObjectHierarchy CompareSystem;

	internal TempGameObjectHierarchy(string res, bool ispol = true)
	{
		if (ispol)
		{
			res = "Dummy/" + res;
		}
		string[] array = res.Split(new char[1] { '/' });
		m_CollectionServer = new GameObject[array.Length];
		Transform parent = null;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = new GameObject(array[i]);
			gameObject.transform.parent = parent;
			parent = gameObject.transform;
			m_CollectionServer[i] = gameObject;
		}
	}

	internal void EnableContext()
	{
		Object.DestroyImmediate(m_CollectionServer[0]);
	}

	internal static bool PublishSystem()
	{
		return CompareSystem == null;
	}
}
