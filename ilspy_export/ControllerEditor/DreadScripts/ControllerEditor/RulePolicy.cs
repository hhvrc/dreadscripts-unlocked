using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace DreadScripts.ControllerEditor;

[Serializable]
internal struct RulePolicy
{
	[SerializeField]
	public Axis axis;

	[SerializeField]
	public OptionState state;

	[SerializeField]
	public OrientationState orientation;

	public static RulePolicy m_SingletonPolicy = new RulePolicy(Axis.X | Axis.Y | Axis.Z, OptionState.Allowed, OrientationState.Both);

	public static RulePolicy factoryPolicy = new RulePolicy(OptionState.Forced);

	public static RulePolicy accountPolicy = new RulePolicy(OptionState.Off);

	internal static object CompareDecorator;

	public RulePolicy(OptionState init)
		: this(Axis.X | Axis.Y | Axis.Z, init)
	{
	}

	public RulePolicy(OrientationState item)
		: this(Axis.X | Axis.Y | Axis.Z, OptionState.Allowed, item)
	{
	}

	public RulePolicy(OptionState last, OrientationState counter)
		: this(Axis.X | Axis.Y | Axis.Z, last, counter)
	{
	}

	public RulePolicy(Axis task = Axis.X | Axis.Y | Axis.Z, OptionState cust = OptionState.Allowed, OrientationState util = OrientationState.Both)
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
