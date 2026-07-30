using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

[Serializable]
internal struct TransformControlSettings
{
	[SerializeField]
	public AxisControlSettings positionControl;

	[SerializeField]
	public AxisControlSettings rotationControl;

	[SerializeField]
	public AxisControlSettings scaleControl;

	[SerializeField]
	public bool uniformScaleOnly;

	internal static object ListDecorator;

	public TransformControlSettings(AxisControlSettings key, AxisControlSettings cfg, AxisControlSettings third, bool acceptinit2 = true)
	{
		positionControl = key;
		rotationControl = cfg;
		scaleControl = third;
		uniformScaleOnly = acceptinit2;
	}

	[SpecialName]
	public static TransformControlSettings ValidateHelper()
	{
		return new TransformControlSettings(AxisControlSettings.m_SingletonPolicy, AxisControlSettings.m_SingletonPolicy, AxisControlSettings.accountPolicy, acceptinit2: false);
	}

	[SpecialName]
	public static TransformControlSettings RateHelper()
	{
		return new TransformControlSettings(AxisControlSettings.m_SingletonPolicy, AxisControlSettings.accountPolicy, AxisControlSettings.accountPolicy);
	}

	[SpecialName]
	public static TransformControlSettings GetHelper()
	{
		return new TransformControlSettings(AxisControlSettings.accountPolicy, AxisControlSettings.m_SingletonPolicy, AxisControlSettings.accountPolicy);
	}

	internal static bool CalcDecorator()
	{
		return ListDecorator == null;
	}
}
