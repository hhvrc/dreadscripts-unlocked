using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ControllerEditor;

internal class PhysBoneChainData
{
	internal readonly VRCPhysBone physBone;

	internal readonly Transform rootTransform;

	internal readonly List<BoneTransformData> bones;

	internal readonly int maxDepth;

	internal List<List<BoneTransformData>> chains;

	[SpecialName]
	internal IEnumerable<Matrix4x4> GetBoneMatrices()
	{
		return bones.Select((BoneTransformData b) => b.matrix);
	}

	internal PhysBoneChainData(VRCPhysBone first)
	{
		physBone = first;
		rootTransform = first.GetRootTransform();
		bones = new List<BoneTransformData>();
		BuildChain(rootTransform, 0);
		maxDepth = bones.Max((BoneTransformData b) => b.depth);
	}

	internal void BuildChain(Transform info, int ord)
	{
		bool flag = false;
		BoneTransformData boneTransformData = new BoneTransformData();
		BoneTransformData child = null;
		BoneTransformData boneTransformData2 = null;
		Quaternion q = info.rotation;
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < info.childCount; i++)
		{
			Transform child2 = info.GetChild(i);
			if (!physBone.ignoreTransforms.Contains(child2))
			{
				list.Add(child2);
			}
		}
		bool isEndBone;
		if (isEndBone = list.Count == 0)
		{
			if (physBone.endpointPosition != Vector3.zero)
			{
				Vector3 pos = info.TransformPoint(physBone.endpointPosition);
				q = info.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(physBone.endpointPosition));
				BoneTransformData obj = new BoneTransformData
				{
					chain = this,
					root = rootTransform,
					matrix = Matrix4x4.TRS(pos, q, info.lossyScale),
					depth = ord + 1,
					isVirtual = true,
					isEndBone = true,
					parent = boneTransformData
				};
				child = obj;
				boneTransformData2 = obj;
			}
			else if (bones.Count != 0)
			{
				q = bones[bones.Count - 1].matrix.rotation;
			}
		}
		else if (list.Count > 1)
		{
			if (physBone.multiChildType != VRCPhysBoneBase.MultiChildType.Average)
			{
				if (physBone.multiChildType == VRCPhysBoneBase.MultiChildType.Ignore)
				{
					flag = true;
				}
			}
			else
			{
				Vector3 zero = Vector3.zero;
				foreach (Transform item in list)
				{
					zero += item.position;
				}
				zero /= (float)list.Count;
				Vector3 toDirection = zero - info.position;
				q = info.rotation * Quaternion.FromToRotation(info.up, toDirection);
				boneTransformData2 = (child = new BoneTransformData
				{
					chain = this,
					root = rootTransform,
					matrix = Matrix4x4.TRS(zero, q, info.lossyScale),
					depth = ord + 1,
					isVirtual = true,
					isEndBone = true,
					parent = boneTransformData
				});
			}
		}
		if (!flag)
		{
			boneTransformData.chain = this;
			boneTransformData.root = rootTransform;
			boneTransformData.transform = info;
			boneTransformData.matrix = Matrix4x4.TRS(info.position, q, info.lossyScale);
			boneTransformData.depth = ord;
			boneTransformData.isEndBone = isEndBone;
			boneTransformData.child = child;
			BoneTransformData boneTransformData3 = bones.LastOrDefault();
			if (boneTransformData3 != null && !boneTransformData3.isEndBone && boneTransformData3.child == null)
			{
				boneTransformData3.child = boneTransformData;
				boneTransformData.parent = boneTransformData3;
			}
			bones.Add(boneTransformData);
		}
		if (boneTransformData2 != null)
		{
			bones.Add(boneTransformData2);
		}
		foreach (Transform item2 in list)
		{
			BuildChain(item2, ord + 1);
		}
	}

	internal void BuildChains()
	{
		HashSet<BoneTransformData> hashSet = new HashSet<BoneTransformData>();
		chains = new List<List<BoneTransformData>>();
		foreach (BoneTransformData bone in bones)
		{
			if (!hashSet.Contains(bone))
			{
				List<BoneTransformData> list = new List<BoneTransformData>();
				for (BoneTransformData boneTransformData = bone; boneTransformData != null; boneTransformData = boneTransformData.child)
				{
					list.Add(boneTransformData);
					hashSet.Add(boneTransformData);
				}
				chains.Add(list);
			}
		}
	}
}
