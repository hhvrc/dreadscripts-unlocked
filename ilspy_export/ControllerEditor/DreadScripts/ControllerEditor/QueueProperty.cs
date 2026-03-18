using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class QueueProperty
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec m_WatcherProperty = new _003C_003Ec();

		public static Func<EditorCurveBinding, string> candidateProperty;

		public static Func<string, string> productProperty;

		internal static _003C_003Ec PushStruct;

		internal string PublishPage(EditorCurveBinding b)
		{
			return b.propertyName;
		}

		internal string PopPage(string s)
		{
			return s;
		}

		internal static bool PrepareStruct()
		{
			return PushStruct == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass27_0
	{
		public QueueProperty expressionProperty;

		public string m_SystemProperty;

		private static _003C_003Ec__DisplayClass27_0 PrintStruct;

		internal void ComputePage()
		{
			if (expressionProperty.RevertSerializer() != -1)
			{
				expressionProperty.m_HelperProperty = (from b in AnimationUtility.GetAnimatableBindings(expressionProperty.ManageSerializer(), expressionProperty.ManageSerializer())
					where b.type == expressionProperty._RecordProperty
					select b).Select(_003C_003Ec.m_WatcherProperty.PublishPage).OrderBy(_003C_003Ec.m_WatcherProperty.PopPage).ToArray();
			}
			else
			{
				expressionProperty.m_HelperProperty = new string[1] { "m_IsActive" };
			}
			if (expressionProperty._ConsumerProperty >= expressionProperty.m_HelperProperty.Length)
			{
				expressionProperty._ConsumerProperty = Mathf.Max(0, expressionProperty.m_HelperProperty.Length - 1);
			}
		}

		internal bool MovePage(EditorCurveBinding b)
		{
			return b.type == expressionProperty._RecordProperty;
		}

		internal bool ConcatPage()
		{
			int num = expressionProperty.m_HelperProperty.FindResolver((string s) => s == m_SystemProperty);
			if (num < 0)
			{
				return false;
			}
			expressionProperty._ConsumerProperty = num;
			return true;
		}

		internal bool CallPage(string s)
		{
			return s == m_SystemProperty;
		}

		internal static bool ResolveStruct()
		{
			return PrintStruct == null;
		}
	}

	private GameObject errorProperty;

	public Component[] m_SetterProperty;

	public int _ConnectionProperty = -1;

	public UnityEngine.Object contextProperty;

	public Type _RecordProperty;

	public string[] m_HelperProperty;

	public int _ConsumerProperty;

	public float m_AdapterProperty = 1f;

	internal static readonly Type[] _InterpreterProperty = new Type[3]
	{
		typeof(GameObject),
		typeof(Behaviour),
		typeof(Renderer)
	};

	internal static QueueProperty NewStruct;

	[SpecialName]
	public bool RegisterSerializer()
	{
		if ((bool)errorProperty && _ConnectionProperty < m_SetterProperty.Length)
		{
			return _ConsumerProperty < m_HelperProperty.Length;
		}
		return false;
	}

	[SpecialName]
	public bool PatchSerializer()
	{
		return m_AdapterProperty > 0f;
	}

	[SpecialName]
	internal GameObject ManageSerializer()
	{
		return errorProperty;
	}

	[SpecialName]
	internal void PrintSerializer(GameObject def)
	{
		if (errorProperty != def)
		{
			errorProperty = def;
			PrepareSerializer();
		}
	}

	[SpecialName]
	public int RevertSerializer()
	{
		return _ConnectionProperty;
	}

	[SpecialName]
	public void OrderPage(int param)
	{
		if (_ConnectionProperty != param)
		{
			_ConnectionProperty = param;
			UpdateSerializer();
			ChangeSerializer();
		}
	}

	[SpecialName]
	public string SetPage()
	{
		if (!m_HelperProperty.Any() || _ConsumerProperty >= m_HelperProperty.Length)
		{
			return string.Empty;
		}
		return m_HelperProperty[_ConsumerProperty];
	}

	public QueueProperty()
	{
		m_HelperProperty = Array.Empty<string>();
		m_SetterProperty = Array.Empty<Component>();
	}

	public QueueProperty(GameObject def)
	{
		PrintSerializer(def);
		m_AdapterProperty = (def.activeSelf ? 1 : 0);
		AssetSerializer();
		ChangeSerializer();
	}

	public void StopSerializer(bool hasparam)
	{
		do
		{
			int param = RevertSerializer() + 1;
			OrderPage(param);
		}
		while (hasparam && !SetupPage());
	}

	public void CheckSerializer(bool isreference)
	{
		do
		{
			int param = RevertSerializer() - 1;
			OrderPage(param);
		}
		while (isreference && !SetupPage());
	}

	private void PrepareSerializer()
	{
		AssetSerializer();
		if ((bool)errorProperty)
		{
			for (int i = 0; i < m_SetterProperty.Length; i++)
			{
				if (m_SetterProperty[i].GetType() == _RecordProperty)
				{
					OrderPage(i);
					ChangeSerializer();
					return;
				}
			}
			_ConnectionProperty = -1;
			ChangeSerializer();
		}
		else
		{
			ChangeSerializer();
		}
	}

	private void AssetSerializer()
	{
		m_SetterProperty = (ManageSerializer() ? ManageSerializer().GetComponents<Component>() : Array.Empty<Component>());
	}

	private void UpdateSerializer()
	{
		if (m_SetterProperty == null || _ConnectionProperty >= m_SetterProperty.Length)
		{
			AssetSerializer();
		}
		if (_ConnectionProperty >= m_SetterProperty.Length)
		{
			_ConnectionProperty = -1;
		}
		else if (_ConnectionProperty < -1)
		{
			_ConnectionProperty = m_SetterProperty.Length - 1;
		}
	}

	private void ChangeSerializer()
	{
		_003C_003Ec__DisplayClass27_0 _003C_003Ec__DisplayClass27_ = new _003C_003Ec__DisplayClass27_0();
		_003C_003Ec__DisplayClass27_.expressionProperty = this;
		contextProperty = ((!ManageSerializer()) ? null : ((RevertSerializer() != -1) ? ((UnityEngine.Object)m_SetterProperty[RevertSerializer()]) : ((UnityEngine.Object)ManageSerializer())));
		Type recordProperty = _RecordProperty;
		_RecordProperty = (contextProperty ? contextProperty.GetType() : null);
		if (!contextProperty || recordProperty == _RecordProperty)
		{
			return;
		}
		if (m_HelperProperty == null || _ConsumerProperty >= m_HelperProperty.Length || RevertSerializer() == -1)
		{
			_003C_003Ec__DisplayClass27_.ComputePage();
			return;
		}
		_003C_003Ec__DisplayClass27_.m_SystemProperty = m_HelperProperty[_ConsumerProperty];
		_003C_003Ec__DisplayClass27_.ComputePage();
		if (m_HelperProperty.Length != 0 && !_003C_003Ec__DisplayClass27_.ConcatPage())
		{
			_003C_003Ec__DisplayClass27_.m_SystemProperty = ((_003C_003Ec__DisplayClass27_.m_SystemProperty == "m_IsActive") ? "m_Enabled" : ((_003C_003Ec__DisplayClass27_.m_SystemProperty == "m_Enabled") ? "m_IsActive" : string.Empty));
			if (!_003C_003Ec__DisplayClass27_.ConcatPage())
			{
				_ConsumerProperty = 0;
			}
		}
	}

	[SpecialName]
	private bool SetupPage()
	{
		return _InterpreterProperty.Any((Type i) => _RecordProperty.InstantiateResolver(i));
	}

	[CompilerGenerated]
	private bool SortSerializer(Type i)
	{
		return _RecordProperty.InstantiateResolver(i);
	}

	internal static bool LoginStruct()
	{
		return NewStruct == null;
	}
}
