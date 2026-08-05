using System.Runtime.CompilerServices;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal class BoneTransformData
{
	internal PhysBoneChainData chain;

	internal Transform root;

	internal Transform transform;

	internal Matrix4x4 matrix;

	internal bool isVirtual;

	internal bool isEndBone;

	internal int depth;

	internal BoneTransformData child;

	internal BoneTransformData parent;

	[SpecialName]
	internal Vector3 GetPosition()
	{
		return matrix.GetColumn(3);
	}

	[SpecialName]
	internal float GetMaxScale()
	{
		return Mathf.Max(matrix.lossyScale.x, matrix.lossyScale.y, matrix.lossyScale.z);
	}

	[SpecialName]
	internal float GetNormalizedDepth()
	{
		return 1f / (float)chain.maxDepth * (float)depth;
	}

	internal float EvaluateCurve(AnimationCurve item)
	{
		if (item == null || item.length < 2)
		{
			return 1f;
		}
		return item.Evaluate(GetNormalizedDepth());
	}
}
