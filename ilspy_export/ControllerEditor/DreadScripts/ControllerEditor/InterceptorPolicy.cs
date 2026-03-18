using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ControllerEditor;

internal struct InterceptorPolicy
{
	internal readonly Object m_CreatorPolicy;

	internal bool _EventPolicy;

	internal readonly Transform _InfoPolicy;

	internal readonly int m_FacadePolicy;

	internal float m_AdvisorPolicy;

	internal float callbackPolicy;

	internal Vector3 _IndexerPolicy;

	internal Quaternion issuerPolicy;

	internal static object SetupDecorator;

	internal InterceptorPolicy(VRCPhysBoneColliderBase config)
	{
		m_CreatorPolicy = config;
		_EventPolicy = true;
		_InfoPolicy = config.GetRootTransform();
		m_FacadePolicy = (int)config.shapeType;
		m_AdvisorPolicy = config.radius;
		callbackPolicy = config.height;
		_IndexerPolicy = config.position;
		issuerPolicy = config.rotation;
	}

	internal InterceptorPolicy(ContactBase first)
	{
		m_CreatorPolicy = first;
		_EventPolicy = false;
		_InfoPolicy = first.GetRootTransform();
		m_FacadePolicy = (int)first.shapeType;
		m_AdvisorPolicy = first.radius;
		callbackPolicy = first.height;
		_IndexerPolicy = first.position;
		issuerPolicy = first.rotation;
	}

	internal void CalculateHelper()
	{
		if (!_EventPolicy)
		{
			ContactBase obj = (ContactBase)m_CreatorPolicy;
			obj.radius = m_AdvisorPolicy;
			obj.height = callbackPolicy;
			obj.position = _IndexerPolicy;
			obj.rotation = issuerPolicy;
			obj.shapeType = (ContactBase.ShapeType)m_FacadePolicy;
		}
		else
		{
			VRCPhysBoneColliderBase obj2 = (VRCPhysBoneColliderBase)m_CreatorPolicy;
			obj2.radius = m_AdvisorPolicy;
			obj2.height = callbackPolicy;
			obj2.position = _IndexerPolicy;
			obj2.rotation = issuerPolicy;
		}
	}

	internal void TestHelper(ContactBase first)
	{
		first.radius = m_AdvisorPolicy;
		first.height = callbackPolicy;
		first.position = _IndexerPolicy;
		first.rotation = issuerPolicy;
		first.shapeType = (ContactBase.ShapeType)m_FacadePolicy;
	}

	internal void MapHelper(VRCPhysBoneCollider config)
	{
		config.radius = m_AdvisorPolicy;
		config.height = callbackPolicy;
		config.position = _IndexerPolicy;
		config.rotation = issuerPolicy;
		config.shapeType = (VRCPhysBoneColliderBase.ShapeType)m_FacadePolicy;
	}

	internal static bool ExcludeDecorator()
	{
		return SetupDecorator == null;
	}
}
