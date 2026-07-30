using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace DreadScripts.ControllerEditor;

[Serializable]
internal struct AxisControlSettings
{
	[SerializeField]
	public Axis axis;

	[SerializeField]
	public OptionState state;

	[SerializeField]
	public OrientationState orientation;

	public static AxisControlSettings m_SingletonPolicy = new AxisControlSettings(Axis.X | Axis.Y | Axis.Z, OptionState.Allowed, OrientationState.Both);

	public static AxisControlSettings factoryPolicy = new AxisControlSettings(OptionState.Forced);

	public static AxisControlSettings accountPolicy = new AxisControlSettings(OptionState.Off);

	internal static object CompareDecorator;

	public AxisControlSettings(OptionState init)
		: this(Axis.X | Axis.Y | Axis.Z, init)
	{
	}

	public AxisControlSettings(OrientationState item)
		: this(Axis.X | Axis.Y | Axis.Z, OptionState.Allowed, item)
	{
	}

	public AxisControlSettings(OptionState last, OrientationState counter)
		: this(Axis.X | Axis.Y | Axis.Z, last, counter)
	{
	}

	public AxisControlSettings(Axis task = Axis.X | Axis.Y | Axis.Z, OptionState cust = OptionState.Allowed, OrientationState util = OrientationState.Both)
	{
		axis = task;
		state = cust;
		orientation = util;
	}

	public bool IncludeHelper(bool islast)
	{
		if (axis > Axis.None && state > OptionState.Off)
		{
			if (state != OptionState.Forced)
			{
				return state == OptionState.Allowed && islast;
			}
			return true;
		}
		return false;
	}

	public PivotRotation RunHelper(PivotRotation instance)
	{
		return orientation switch
		{
			OrientationState.Local => PivotRotation.Local, 
			OrientationState.Global => PivotRotation.Global, 
			_ => instance, 
		};
	}

	internal static bool PublishDecorator()
	{
		return CompareDecorator == null;
	}
}
