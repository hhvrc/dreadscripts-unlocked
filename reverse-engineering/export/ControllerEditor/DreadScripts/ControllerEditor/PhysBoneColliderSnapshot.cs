using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ControllerEditor;

internal struct PhysBoneColliderSnapshot
{
	internal readonly Object source;

	internal bool isPhysBoneCollider;

	internal readonly Transform rootTransform;

	internal readonly int shapeType;

	internal float radius;

	internal float height;

	internal Vector3 position;

	internal Quaternion rotation;

	internal static object SetupDecorator;

	internal PhysBoneColliderSnapshot(VRCPhysBoneColliderBase config)
	{
		source = config;
		isPhysBoneCollider = true;
		rootTransform = config.GetRootTransform();
		shapeType = (int)config.shapeType;
		radius = config.radius;
		height = config.height;
		position = config.position;
		rotation = config.rotation;
	}

	internal PhysBoneColliderSnapshot(ContactBase first)
	{
		source = first;
		isPhysBoneCollider = false;
		rootTransform = first.GetRootTransform();
		shapeType = (int)first.shapeType;
		radius = first.radius;
		height = first.height;
		position = first.position;
		rotation = first.rotation;
	}

	internal void Apply()
	{
		if (!isPhysBoneCollider)
		{
			ContactBase obj = (ContactBase)source;
			obj.radius = radius;
			obj.height = height;
			obj.position = position;
			obj.rotation = rotation;
			obj.shapeType = (ContactBase.ShapeType)shapeType;
		}
		else
		{
			VRCPhysBoneColliderBase obj2 = (VRCPhysBoneColliderBase)source;
			obj2.radius = radius;
			obj2.height = height;
			obj2.position = position;
			obj2.rotation = rotation;
		}
	}

	internal void Apply(ContactBase first)
	{
		first.radius = radius;
		first.height = height;
		first.position = position;
		first.rotation = rotation;
		first.shapeType = (ContactBase.ShapeType)shapeType;
	}

	internal void Apply(VRCPhysBoneCollider config)
	{
		config.radius = radius;
		config.height = height;
		config.position = position;
		config.rotation = rotation;
		config.shapeType = (VRCPhysBoneColliderBase.ShapeType)shapeType;
	}

	internal static bool ExcludeDecorator()
	{
		return SetupDecorator == null;
	}
}
