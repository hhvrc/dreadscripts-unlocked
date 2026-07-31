using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class ComponentQueue
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec m_WatcherProperty = new _003C_003Ec();

		public static Func<EditorCurveBinding, string> candidateProperty;

		public static Func<string, string> productProperty;

		internal string PublishPage(EditorCurveBinding b)
		{
			return b.propertyName;
		}

		internal string PopPage(string s)
		{
			return s;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass27_0
	{
		public ComponentQueue expressionProperty;

		public string m_SystemProperty;

		internal void ComputePage()
		{
			if (expressionProperty.ComponentIndex() != -1)
			{
				expressionProperty.propertyNames = (from b in AnimationUtility.GetAnimatableBindings(expressionProperty.GameObject(), expressionProperty.GameObject())
					where b.type == expressionProperty.targetType
					select b).Select(_003C_003Ec.m_WatcherProperty.PublishPage).OrderBy(_003C_003Ec.m_WatcherProperty.PopPage).ToArray();
			}
			else
			{
				expressionProperty.propertyNames = new string[1] { "m_IsActive" };
			}
			if (expressionProperty.propertyIndex >= expressionProperty.propertyNames.Length)
			{
				expressionProperty.propertyIndex = Mathf.Max(0, expressionProperty.propertyNames.Length - 1);
			}
		}

		internal bool MovePage(EditorCurveBinding b)
		{
			return b.type == expressionProperty.targetType;
		}

		internal bool ConcatPage()
		{
			int num = expressionProperty.propertyNames.FindResolver((string s) => s == m_SystemProperty);
			if (num < 0)
			{
				return false;
			}
			expressionProperty.propertyIndex = num;
			return true;
		}

		internal bool CallPage(string s)
		{
			return s == m_SystemProperty;
		}
	}

	private GameObject gameObject;

	public Component[] components;

	public int componentIndex = -1;

	public UnityEngine.Object target;

	public Type targetType;

	public string[] propertyNames;

	public int propertyIndex;

	public float value = 1f;

	internal static readonly Type[] toggleableTypes = new Type[3]
	{
		typeof(GameObject),
		typeof(Behaviour),
		typeof(Renderer)
	};

	[SpecialName]
	public bool IsValid()
	{
		if ((bool)gameObject && componentIndex < components.Length)
		{
			return propertyIndex < propertyNames.Length;
		}
		return false;
	}

	[SpecialName]
	public bool IsOn()
	{
		return value > 0f;
	}

	[SpecialName]
	internal GameObject GameObject()
	{
		return gameObject;
	}

	[SpecialName]
	internal void GameObject(GameObject def)
	{
		if (gameObject != def)
		{
			gameObject = def;
			Refresh();
		}
	}

	[SpecialName]
	public int ComponentIndex()
	{
		return componentIndex;
	}

	[SpecialName]
	public void ComponentIndex(int param)
	{
		if (componentIndex != param)
		{
			componentIndex = param;
			WrapComponentIndex();
			UpdateTarget();
		}
	}

	[SpecialName]
	public string PropertyName()
	{
		if (!propertyNames.Any() || propertyIndex >= propertyNames.Length)
		{
			return string.Empty;
		}
		return propertyNames[propertyIndex];
	}

	public ComponentQueue()
	{
		propertyNames = Array.Empty<string>();
		components = Array.Empty<Component>();
	}

	public ComponentQueue(GameObject def)
	{
		GameObject(def);
		value = (def.activeSelf ? 1 : 0);
		RefreshComponents();
		UpdateTarget();
	}

	public void Next(bool hasparam)
	{
		do
		{
			int param = ComponentIndex() + 1;
			ComponentIndex(param);
		}
		while (hasparam && !IsToggleable());
	}

	public void Previous(bool isreference)
	{
		do
		{
			int param = ComponentIndex() - 1;
			ComponentIndex(param);
		}
		while (isreference && !IsToggleable());
	}

	private void Refresh()
	{
		RefreshComponents();
		if ((bool)gameObject)
		{
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i].GetType() == targetType)
				{
					ComponentIndex(i);
					UpdateTarget();
					return;
				}
			}
			componentIndex = -1;
			UpdateTarget();
		}
		else
		{
			UpdateTarget();
		}
	}

	private void RefreshComponents()
	{
		components = (GameObject() ? GameObject().GetComponents<Component>() : Array.Empty<Component>());
	}

	private void WrapComponentIndex()
	{
		if (components == null || componentIndex >= components.Length)
		{
			RefreshComponents();
		}
		if (componentIndex >= components.Length)
		{
			componentIndex = -1;
		}
		else if (componentIndex < -1)
		{
			componentIndex = components.Length - 1;
		}
	}

	private void UpdateTarget()
	{
		_003C_003Ec__DisplayClass27_0 _003C_003Ec__DisplayClass27_ = new _003C_003Ec__DisplayClass27_0();
		_003C_003Ec__DisplayClass27_.expressionProperty = this;
		target = ((!GameObject()) ? null : ((ComponentIndex() != -1) ? ((UnityEngine.Object)components[ComponentIndex()]) : ((UnityEngine.Object)GameObject())));
		Type type = targetType;
		targetType = (target ? target.GetType() : null);
		if (!target || type == targetType)
		{
			return;
		}
		if (propertyNames == null || propertyIndex >= propertyNames.Length || ComponentIndex() == -1)
		{
			_003C_003Ec__DisplayClass27_.ComputePage();
			return;
		}
		_003C_003Ec__DisplayClass27_.m_SystemProperty = propertyNames[propertyIndex];
		_003C_003Ec__DisplayClass27_.ComputePage();
		if (propertyNames.Length != 0 && !_003C_003Ec__DisplayClass27_.ConcatPage())
		{
			_003C_003Ec__DisplayClass27_.m_SystemProperty = ((_003C_003Ec__DisplayClass27_.m_SystemProperty == "m_IsActive") ? "m_Enabled" : ((_003C_003Ec__DisplayClass27_.m_SystemProperty == "m_Enabled") ? "m_IsActive" : string.Empty));
			if (!_003C_003Ec__DisplayClass27_.ConcatPage())
			{
				propertyIndex = 0;
			}
		}
	}

	[SpecialName]
	private bool IsToggleable()
	{
		return toggleableTypes.Any((Type i) => targetType.InstantiateResolver(i));
	}

	[CompilerGenerated]
	private bool IsToggleable(Type i)
	{
		return targetType.InstantiateResolver(i);
	}
}
