using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class TempGameObjectHierarchy
{
	internal readonly GameObject[] gameObjects;

	internal TempGameObjectHierarchy(string res, bool ispol = true)
	{
		if (ispol)
		{
			res = "Dummy/" + res;
		}
		string[] array = res.Split(new char[1] { '/' });
		gameObjects = new GameObject[array.Length];
		Transform parent = null;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject gameObject = new GameObject(array[i]);
			gameObject.transform.parent = parent;
			parent = gameObject.transform;
			gameObjects[i] = gameObject;
		}
	}

	internal void Destroy()
	{
		Object.DestroyImmediate(gameObjects[0]);
	}
}
