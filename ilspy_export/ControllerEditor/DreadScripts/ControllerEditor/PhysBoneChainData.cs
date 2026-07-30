using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ControllerEditor;

internal class PhysBoneChainData
{
	internal readonly VRCPhysBone requestThread;

	internal readonly Transform printerThread;

	internal readonly List<BoneTransformData> _WriterThread;

	internal readonly int paramsThread;

	internal List<List<BoneTransformData>> _ListenerThread;

	private static PhysBoneChainData ConcatStatus;

	[SpecialName]
	internal IEnumerable<Matrix4x4> PublishRecord()
	{
		return _WriterThread.Select((BoneTransformData b) => b._CallbackThread);
	}

	internal PhysBoneChainData(VRCPhysBone first)
	{
		requestThread = first;
		printerThread = first.GetRootTransform();
		_WriterThread = new List<BoneTransformData>();
		SetupRecord(printerThread, 0);
		paramsThread = _WriterThread.Max((BoneTransformData b) => b.m_PrototypeThread);
	}

	internal void SetupRecord(Transform info, int ord)
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
			if (!requestThread.ignoreTransforms.Contains(child))
			{
				list.Add(child);
			}
		}
		bool issuerThread;
		if (issuerThread = list.Count == 0)
		{
			if (requestThread.endpointPosition != Vector3.zero)
			{
				Vector3 pos = info.TransformPoint(requestThread.endpointPosition);
				q = info.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(requestThread.endpointPosition));
				BoneTransformData obj = new BoneTransformData
				{
					infoThread = this,
					m_FacadeThread = printerThread,
					_CallbackThread = Matrix4x4.TRS(pos, q, info.lossyScale),
					m_PrototypeThread = ord + 1,
					indexerThread = true,
					issuerThread = true,
					m_SingletonThread = boneTransformData
				};
				ruleThread = obj;
				boneTransformData2 = obj;
			}
			else if (_WriterThread.Count != 0)
			{
				q = _WriterThread[_WriterThread.Count - 1]._CallbackThread.rotation;
			}
		}
		else if (list.Count > 1)
		{
			if (requestThread.multiChildType != VRCPhysBoneBase.MultiChildType.Average)
			{
				if (requestThread.multiChildType == VRCPhysBoneBase.MultiChildType.Ignore)
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
					m_FacadeThread = printerThread,
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
			boneTransformData.m_FacadeThread = printerThread;
			boneTransformData.m_AdvisorThread = info;
			boneTransformData._CallbackThread = Matrix4x4.TRS(info.position, q, info.lossyScale);
			boneTransformData.m_PrototypeThread = ord;
			boneTransformData.issuerThread = issuerThread;
			boneTransformData.m_RuleThread = ruleThread;
			BoneTransformData boneTransformData3 = _WriterThread.LastOrDefault();
			if (boneTransformData3 != null && !boneTransformData3.issuerThread && boneTransformData3.m_RuleThread == null)
			{
				boneTransformData3.m_RuleThread = boneTransformData;
				boneTransformData.m_SingletonThread = boneTransformData3;
			}
			_WriterThread.Add(boneTransformData);
		}
		if (boneTransformData2 != null)
		{
			_WriterThread.Add(boneTransformData2);
		}
		foreach (Transform item2 in list)
		{
			SetupRecord(item2, ord + 1);
		}
	}

	internal void EnableRecord()
	{
		HashSet<BoneTransformData> hashSet = new HashSet<BoneTransformData>();
		_ListenerThread = new List<List<BoneTransformData>>();
		foreach (BoneTransformData item in _WriterThread)
		{
			if (!hashSet.Contains(item))
			{
				List<BoneTransformData> list = new List<BoneTransformData>();
				for (BoneTransformData boneTransformData = item; boneTransformData != null; boneTransformData = boneTransformData.m_RuleThread)
				{
					list.Add(boneTransformData);
					hashSet.Add(boneTransformData);
				}
				_ListenerThread.Add(list);
			}
		}
	}

	internal static bool CollectStatus()
	{
		return ConcatStatus == null;
	}
}
