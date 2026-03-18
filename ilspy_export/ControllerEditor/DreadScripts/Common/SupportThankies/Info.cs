using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common.SupportThankies;

internal static class Info
{
	private static Texture2D facade;

	internal static Info FillIndexer;

	internal static T DestroyWrapper<T>(this T[] ident)
	{
		return ident[UnityEngine.Random.Range(0, ident.Length)];
	}

	public static Rect GetWrapper(this Rect config, float selection)
	{
		config.x += selection;
		config.y += selection;
		config.width -= selection * 2f;
		config.height -= selection * 2f;
		return config;
	}

	public static Rect CalcWrapper(Rect var1, float cont)
	{
		Rect result = var1;
		if (var1.width / var1.height > cont)
		{
			result.width = var1.height * cont;
			result.x += (var1.width - result.width) / 2f;
		}
		else
		{
			result.height = var1.width / cont;
			result.y += (var1.height - result.height) / 2f;
		}
		return result;
	}

	internal static Color IncludeWrapper(this Color init, Color second)
	{
		float num = second.a + init.a * (1f - second.a);
		float r = (second.r * second.a + init.r * init.a * (1f - second.a)) / num;
		float g = (second.g * second.a + init.g * init.a * (1f - second.a)) / num;
		float b = (second.b * second.a + init.b * init.a * (1f - second.a)) / num;
		return new Color(r, g, b, num);
	}

	internal static Rect RunWrapper(Rect reference, Color token = default(Color), Color util = default(Color), float cfg2 = 3f)
	{
		bool flag = token != Color.clear;
		bool flag2 = util != Color.clear;
		if (flag || flag2)
		{
			float num = cfg2 + 2f;
			Rect position = reference;
			position.x -= num / 2f;
			position.width += num;
			position.y -= num / 2f;
			position.height += num;
			if (flag)
			{
				GUI.DrawTexture(reference, CloneWrapper(token), ScaleMode.StretchToFill, alphaBlend: false, 0f, token, 0f, 8f);
			}
			if (flag2)
			{
				GUI.DrawTexture(position, CloneWrapper(util), ScaleMode.StretchToFill, alphaBlend: false, 0f, util, cfg2, 8f);
			}
		}
		Rect result = reference;
		result.x += 4f;
		result.width -= 8f;
		result.y += 4f;
		result.height -= 8f;
		return result;
	}

	internal static Texture2D CloneWrapper(Color instance)
	{
		if (facade == null)
		{
			while (true)
			{
				facade = new Texture2D(1, 1, TextureFormat.RGBAFloat, mipChain: false)
				{
					filterMode = FilterMode.Point,
					anisoLevel = 0
				};
			}
		}
		facade.SetPixel(0, 0, instance);
		facade.Apply();
		return facade;
	}

	internal static void LoginWrapper(byte[] config, string selection)
	{
		int[] value = ViewWrapper(config);
		SessionState.SetIntArray(selection, value);
	}

	internal static Texture2D ReflectWrapper(string last)
	{
		int[] intArray = SessionState.GetIntArray(last, null);
		if (intArray != null)
		{
			try
			{
				byte[] data = CollectWrapper(intArray);
				Texture2D texture2D = new Texture2D(0, 0);
				texture2D.LoadImage(data);
				texture2D.Apply();
				return texture2D;
			}
			catch (System.Exception exception)
			{
				Debug.LogException(exception);
				SessionState.EraseIntArray(last);
			}
		}
		return null;
	}

	internal static bool DeleteWrapper(string init, params GUILayoutOption[] options)
	{
		return NewWrapper(new GUIContent(init), null, options);
	}

	internal static bool CreateWrapper(string i, GUIStyle pol, params GUILayoutOption[] options)
	{
		return NewWrapper(new GUIContent(i), pol, options);
	}

	internal static bool NewWrapper(GUIContent def, GUIStyle second, params GUILayoutOption[] options)
	{
		if (second == null)
		{
			second = GUI.skin.button;
		}
		bool result = GUILayout.Button(def, second, options);
		EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
		return result;
	}

	internal static bool PushWrapper(Rect var1)
	{
		EditorGUIUtility.AddCursorRect(var1, MouseCursor.Link);
		Event current = Event.current;
		if (current.button == 0 && current.type == EventType.MouseDown)
		{
			return var1.Contains(current.mousePosition);
		}
		return false;
	}

	private static int[] ViewWrapper(byte[] param)
	{
		int num = param.Length;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = param[i];
		}
		return array;
	}

	private static byte[] CollectWrapper(int[] first)
	{
		byte[] array = new byte[first.Length];
		for (int i = 0; i < first.Length; i++)
		{
			array[i] = (byte)first[i];
		}
		return array;
	}

	internal static bool DeleteIndexer()
	{
		return FillIndexer == null;
	}
}
