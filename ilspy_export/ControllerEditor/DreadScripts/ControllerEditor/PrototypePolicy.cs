using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

[Serializable]
internal struct PrototypePolicy
{
	[SerializeField]
	public RulePolicy positionControl;

	[SerializeField]
	public RulePolicy rotationControl;

	[SerializeField]
	public RulePolicy scaleControl;

	[SerializeField]
	public bool uniformScaleOnly;

	internal static object ListDecorator;

	public PrototypePolicy(RulePolicy key, RulePolicy cfg, RulePolicy third, bool acceptinit2 = true)
	{
		positionControl = key;
		rotationControl = cfg;
		scaleControl = third;
		uniformScaleOnly = acceptinit2;
	}

	[SpecialName]
	public static PrototypePolicy ValidateHelper()
	{
		return new PrototypePolicy(RulePolicy.m_SingletonPolicy, RulePolicy.m_SingletonPolicy, RulePolicy.accountPolicy, acceptinit2: false);
	}

	[SpecialName]
	public static PrototypePolicy RateHelper()
	{
		return new PrototypePolicy(RulePolicy.m_SingletonPolicy, RulePolicy.accountPolicy, RulePolicy.accountPolicy);
	}

	[SpecialName]
	public static PrototypePolicy GetHelper()
	{
		return new PrototypePolicy(RulePolicy.accountPolicy, RulePolicy.m_SingletonPolicy, RulePolicy.accountPolicy);
	}

	internal static bool CalcDecorator()
	{
		return ListDecorator == null;
	}
}
