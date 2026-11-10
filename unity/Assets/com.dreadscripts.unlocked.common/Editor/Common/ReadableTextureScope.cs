// Shared by both tools: ADOverhaul and ControllerEditor shipped their own copy of this type,
// under two different names. Reconstructed from both, which are statement-for-statement
// identical and differ only in the obfuscated name of the constructor parameter:
//   reverse-engineering/export/ControllerEditor/DreadScripts/ControllerEditor/ReadableTextureScope.cs
//     (shipped as ReadableTextureScope, a top-level type)
//   reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs, lines 1103-1146
//     (shipped as ReadableTexture, nested in ADOEditorUtility)
//   reverse-engineering/export/ADOverhaul2019/DreadScripts/ADOverhaul/ADOEditorUtility.cs, lines 1102-1145
//     (same, no divergence from the 2022 build)
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
//
// The ControllerEditor name is kept here. Placed in DreadScripts.Common rather than left in either
// product's namespace for the same reason as SemVer, GUIColorScope and the JSON reader: nothing
// about it is specific to either tool, and keeping one copy per product would force one of them to
// reach across into the other's namespace -- a product-to-product dependency the two shipped
// assemblies never had.
//
// DELIBERATE DEVIATION FROM BOTH SHIPPED COPIES. Four faults, present identically in all three
// decompiled sources, are corrected here rather than reproduced:
//
//   1. Readability was probed by calling GetPixel(0, 0) inside a try/catch and treating the failure
//      as "not readable". Texture2D.isReadable answers the same question directly, so the common
//      path of opening an unreadable texture no longer costs a thrown and caught exception. Same
//      outcome, without using exceptions for control flow.
//   2. The temporary RenderTexture was never released. RenderTexture.GetTemporary draws from a pool
//      that only returns memory on ReleaseTemporary, so every scope over an unreadable texture
//      leaked a full-size render target for the rest of the session. Both call sites run per icon,
//      so this accumulated. It is now released in a finally block.
//   3. RenderTexture.active was set to null on the way out instead of being restored to its previous
//      value. Clearing it corrupts any rendering already in progress further up the stack -- an
//      enclosing scope that had bound its own target finds the binding silently gone. The previous
//      value is now captured and restored, also in the finally block.
//   4. source.filterMode was assigned FilterMode.Point, which persists on the caller's texture after
//      the scope is gone. That is a visible side effect on an asset the caller owns and did not ask
//      to have modified; for a built-in editor icon it changes how that icon samples everywhere else
//      in the editor. Point filtering on the destination render target alone is enough to keep the
//      blit a straight copy, so the source is left untouched.
//
// One further departure, of the same family: the shipped copies never called Apply() after
// ReadPixels, leaving the new Texture2D's GPU-side contents undefined. Both known call sites only
// read pixels back on the CPU, where ReadPixels alone suffices, so this was unobservable in the
// shipped tools -- but the type hands its texture out through an implicit Texture2D conversion, and
// anything that draws that texture would get garbage. Apply() is called here.
//
// Audit status: VERIFIED -- all three shipped copies (ControllerEditor's top-level
// ReadableTextureScope, ADOverhaul 2022 and 2019's nested ReadableTexture) diffed statement by
// statement against this file: both fields, the constructor, Dispose and the implicit Texture2D
// conversion. The three are identical apart from the constructor parameter name, confirmed by
// diffing them against each other. Every difference this file has from them is one of the five
// deviations listed above; nothing else in the constructor changed -- the width/height capture, the
// GetTemporary call, the Blit, the new Texture2D and the ReadPixels rect are transcribed.

using System;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// Yields a CPU-readable copy of a texture, so its pixels can be sampled regardless of whether
    /// the asset was imported with Read/Write enabled. Disposing frees the copy if one was made.
    /// </summary>
    /// <remarks>
    /// A texture without Read/Write enabled has no pixel data on the CPU side. The way to get at it
    /// without changing the user's import settings is to blit through a <see cref="RenderTexture"/>
    /// and read back from the GPU.
    /// </remarks>
    internal sealed class ReadableTextureScope : IDisposable
    {
        /// <summary>True when <see cref="texture"/> is a copy this scope owns and must destroy.</summary>
        internal bool isTemporary;

        internal Texture2D texture;

        internal ReadableTextureScope(Texture2D source)
        {
            if (source.isReadable)
            {
                isTemporary = false;
                texture = source;
                return;
            }

            isTemporary = true;

            int width = source.width;
            int height = source.height;

            RenderTexture temporary = RenderTexture.GetTemporary(width, height);
            RenderTexture previouslyActive = RenderTexture.active;
            try
            {
                // Point filtering on the target keeps the blit a straight copy rather than a
                // resample. The shipped build also forced it on the source; see the header.
                temporary.filterMode = FilterMode.Point;
                RenderTexture.active = temporary;
                Graphics.Blit(source, temporary);

                texture = new Texture2D(width, height);
                texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                texture.Apply();
            }
            finally
            {
                RenderTexture.active = previouslyActive;
                RenderTexture.ReleaseTemporary(temporary);
            }
        }

        public void Dispose()
        {
            if (isTemporary)
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        public static implicit operator Texture2D(ReadableTextureScope scope)
        {
            return scope.texture;
        }
    }
}
