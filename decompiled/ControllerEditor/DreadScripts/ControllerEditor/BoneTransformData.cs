using System.Runtime.CompilerServices;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class BoneTransformData
{
	internal PhysBoneChainData infoThread;

	internal Transform m_FacadeThread;

	internal Transform m_AdvisorThread;

	internal Matrix4x4 _CallbackThread;

	internal bool indexerThread;

	internal bool issuerThread;

	internal int m_PrototypeThread;

	internal BoneTransformData m_RuleThread;

	internal BoneTransformData m_SingletonThread;

	[SpecialName]
	internal Vector3 CallRecord()
	{
		return _CallbackThread.GetColumn(3);
	}

	[SpecialName]
	internal float CountRecord()
	{
		return Mathf.Max(_CallbackThread.lossyScale.x, _CallbackThread.lossyScale.y, _CallbackThread.lossyScale.z);
	}

	[SpecialName]
	internal float InsertRecord()
	{
		return 1f / (float)infoThread.maxDepth * (float)m_PrototypeThread;
	}

	internal float ConcatRecord(AnimationCurve item)
	{
		if (item == null || item.length < 2)
		{
			return 1f;
		}
		return item.Evaluate(InsertRecord());
	}
}
