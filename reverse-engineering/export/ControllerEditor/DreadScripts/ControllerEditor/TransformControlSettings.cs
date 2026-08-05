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
	public static TransformControlSettings PositionAndRotation()
	{
		return new TransformControlSettings(AxisControlSettings.allowed, AxisControlSettings.allowed, AxisControlSettings.off, acceptinit2: false);
	}

	[SpecialName]
	public static TransformControlSettings PositionOnly()
	{
		return new TransformControlSettings(AxisControlSettings.allowed, AxisControlSettings.off, AxisControlSettings.off);
	}

	[SpecialName]
	public static TransformControlSettings RotationOnly()
	{
		return new TransformControlSettings(AxisControlSettings.off, AxisControlSettings.allowed, AxisControlSettings.off);
	}

	internal static bool CalcDecorator()
	{
		return ListDecorator == null;
	}
}
