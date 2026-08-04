// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/EditorUtils.cs
//   static StartResolver  -> ResetLocal,    line 2625
//   static ReadResolver   -> GetChildren,   line 2632
//   static SelectResolver -> SetLossyScale, line 2642
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against export
//
// Basic Transform helpers. SetLossyScale reproduces the vendor's world-scale setter: it neutralises
// the parent's scale by taking the world-to-local matrix with its translation column zeroed and
// mapping the desired lossy scale through it, so the resulting localScale yields that lossy scale.

using UnityEngine;

namespace DreadScripts.ControllerEditor
{
    internal static partial class EditorUtils
    {
        /// <summary>Resets the transform to identity local position, rotation and scale.</summary>
        internal static void ResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>Returns the transform's direct children, in sibling order.</summary>
        internal static Transform[] GetChildren(this Transform transform)
        {
            Transform[] children = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }

            return children;
        }

        /// <summary>
        /// Sets the transform's local scale so that its resulting lossy (world) scale equals
        /// <paramref name="lossyScale"/>, cancelling out the parent chain's accumulated scale.
        /// </summary>
        internal static void SetLossyScale(this Transform transform, Vector3 lossyScale)
        {
            transform.localScale = Vector3.one;
            Matrix4x4 worldToLocal = transform.worldToLocalMatrix;
            worldToLocal.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
            transform.localScale = worldToLocal.MultiplyPoint(lossyScale);
        }
    }
}
