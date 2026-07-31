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
		return bones.Select((BoneTransformData b) => b._CallbackThread);
	}

	internal PhysBoneChainData(VRCPhysBone first)
	{
		physBone = first;
		rootTransform = first.GetRootTransform();
		bones = new List<BoneTransformData>();
		BuildChain(rootTransform, 0);
		maxDepth = bones.Max((BoneTransformData b) => b.m_PrototypeThread);
	}

	internal void BuildChain(Transform info, int ord)
	{
		bool flag = false;
		BoneTransformData boneTransformData = new BoneTransformData();
		BoneTransformData ruleThread = null;
		BoneTransformData boneTransformData2 = null;
		Quaternion q = info.rotation;
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < info.childCount; i++)
		{
			Transform child = info.GetChild(i);
			if (!physBone.ignoreTransforms.Contains(child))
			{
				list.Add(child);
			}
		}
		bool issuerThread;
		if (issuerThread = list.Count == 0)
		{
			if (physBone.endpointPosition != Vector3.zero)
			{
				Vector3 pos = info.TransformPoint(physBone.endpointPosition);
				q = info.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(physBone.endpointPosition));
				BoneTransformData obj = new BoneTransformData
				{
					infoThread = this,
					m_FacadeThread = rootTransform,
					_CallbackThread = Matrix4x4.TRS(pos, q, info.lossyScale),
					m_PrototypeThread = ord + 1,
					indexerThread = true,
					issuerThread = true,
					m_SingletonThread = boneTransformData
				};
				ruleThread = obj;
				boneTransformData2 = obj;
			}
			else if (bones.Count != 0)
			{
				q = bones[bones.Count - 1]._CallbackThread.rotation;
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
				boneTransformData2 = (ruleThread = new BoneTransformData
				{
					infoThread = this,
					m_FacadeThread = rootTransform,
					_CallbackThread = Matrix4x4.TRS(zero, q, info.lossyScale),
					m_PrototypeThread = ord + 1,
					indexerThread = true,
					issuerThread = true,
					m_SingletonThread = boneTransformData
				});
			}
		}
		if (!flag)
		{
			boneTransformData.infoThread = this;
			boneTransformData.m_FacadeThread = rootTransform;
			boneTransformData.m_AdvisorThread = info;
			boneTransformData._CallbackThread = Matrix4x4.TRS(info.position, q, info.lossyScale);
			boneTransformData.m_PrototypeThread = ord;
			boneTransformData.issuerThread = issuerThread;
			boneTransformData.m_RuleThread = ruleThread;
			BoneTransformData boneTransformData3 = bones.LastOrDefault();
			if (boneTransformData3 != null && !boneTransformData3.issuerThread && boneTransformData3.m_RuleThread == null)
			{
				boneTransformData3.m_RuleThread = boneTransformData;
				boneTransformData.m_SingletonThread = boneTransformData3;
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
				for (BoneTransformData boneTransformData = bone; boneTransformData != null; boneTransformData = boneTransformData.m_RuleThread)
				{
					list.Add(boneTransformData);
					hashSet.Add(boneTransformData);
				}
				chains.Add(list);
			}
		}
	}
}
