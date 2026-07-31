// Reconstructed from: decompiled/ControllerEditor/DreadScripts/Common/SupportThankies/EditorGuiUtils.cs
//   facade -> colorTexture
//   Button(string, GUILayoutOption[]), Button(string, GUIStyle, GUILayoutOption[]) and
//     Button(GUIContent, GUIStyle, GUILayoutOption[]) are collapsed into two overloads via an
//     optional style parameter.
// The companion file EditorLayoutUtils.cs from the same folder is NOT ported: every member of it
// already exists on DreadScripts.Common.GUILayoutUtils (splitter reflection, CreateSplitterState,
// Begin/EndSplit, DrawTitle -> TitleField, DrawHorizontalLine -> DrawHorizontalSeparator,
// DrawVerticalLine -> DrawVerticalSeparator, DrawUnderline). Callers here use GUILayoutUtils.

using System;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// Small immediate-mode GUI helpers used by the support window: rect maths, rounded boxes,
    /// click detection, and byte-array storage in <see cref="SessionState"/>.
    /// </summary>
    internal static class EditorGuiUtils
    {
        private static Texture2D colorTexture;

        internal static T RandomElement<T>(this T[] source)
        {
            return source[UnityEngine.Random.Range(0, source.Length)];
        }

        /// <summary>Insets <paramref name="rect"/> by <paramref name="amount"/> on every side.</summary>
        public static Rect Shrink(this Rect rect, float amount)
        {
            rect.x += amount;
            rect.y += amount;
            rect.width -= amount * 2f;
            rect.height -= amount * 2f;
            return rect;
        }

        /// <summary>
        /// The largest rect of the given width-to-height ratio that fits inside
        /// <paramref name="rect"/>, centred on the axis that had to give way.
        /// </summary>
        public static Rect FitAspectRatio(Rect rect, float aspectRatio)
        {
            Rect result = rect;

            if (rect.width / rect.height > aspectRatio)
            {
                result.width = rect.height * aspectRatio;
                result.x += (rect.width - result.width) / 2f;
            }
            else
            {
                result.height = rect.width / aspectRatio;
                result.y += (rect.height - result.height) / 2f;
            }

            return result;
        }

        /// <summary>
        /// Composites <paramref name="over"/> onto this colour with standard source-over alpha
        /// blending, so that a translucent supporter tint reads the same as it would if the GUI had
        /// actually blended it.
        /// </summary>
        internal static Color AlphaBlend(this Color under, Color over)
        {
            float a = over.a + under.a * (1f - over.a);
            float r = (over.r * over.a + under.r * under.a * (1f - over.a)) / a;
            float g = (over.g * over.a + under.g * under.a * (1f - over.a)) / a;
            float b = (over.b * over.a + under.b * under.a * (1f - over.a)) / a;
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Draws a rounded fill and/or a rounded border around <paramref name="rect"/> and returns
        /// the rect inset by the border, i.e. the area safe to put content in.
        /// </summary>
        /// <remarks>
        /// Both passes go through <see cref="GUI.DrawTexture(Rect, Texture, ScaleMode, bool, float, Color, float, float)"/>,
        /// which is the only GUI entry point that knows how to round corners; the texture itself is
        /// irrelevant since the colour argument tints it wholesale. <see cref="Color.clear"/> is the
        /// caller's way of saying "skip this pass".
        /// </remarks>
        internal static Rect DrawRoundedBox(Rect rect, Color fillColor = default(Color), Color borderColor = default(Color), float borderWidth = 3f)
        {
            bool hasFill = fillColor != Color.clear;
            bool hasBorder = borderColor != Color.clear;

            if (hasFill || hasBorder)
            {
                // The border is drawn on a rect grown by its own width, so that it sits around the
                // fill rather than on top of it.
                float growth = borderWidth + 2f;
                Rect borderRect = rect;
                borderRect.x -= growth / 2f;
                borderRect.width += growth;
                borderRect.y -= growth / 2f;
                borderRect.height += growth;

                if (hasFill)
                {
                    GUI.DrawTexture(rect, GetColorTexture(fillColor), ScaleMode.StretchToFill, false, 0f, fillColor, 0f, 8f);
                }

                if (hasBorder)
                {
                    GUI.DrawTexture(borderRect, GetColorTexture(borderColor), ScaleMode.StretchToFill, false, 0f, borderColor, borderWidth, 8f);
                }
            }

            Rect content = rect;
            content.x += 4f;
            content.width -= 8f;
            content.y += 4f;
            content.height -= 8f;
            return content;
        }

        /// <summary>A shared 1x1 texture, repainted to <paramref name="color"/> on every call.</summary>
        /// <remarks>
        /// Because the instance is shared and mutated in place, only the most recent colour is ever
        /// really uploaded; callers therefore pass the colour to
        /// <see cref="GUI.DrawTexture(Rect, Texture, ScaleMode, bool, float, Color, float, float)"/>
        /// as a tint as well and rely on this only for the texture's shape.
        /// </remarks>
        internal static Texture2D GetColorTexture(Color color)
        {
            // NOTE: the decompiled source has `while (true) { colorTexture = new Texture2D(...); }`
            // here, which would hang the editor on the first call and so cannot be what shipped --
            // the support window demonstrably draws. Ported as the plainly intended one-shot
            // initialisation; flagged here because it is the one place this file is not a literal
            // transcription.
            if (colorTexture == null)
            {
                colorTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false)
                {
                    filterMode = FilterMode.Point,
                    anisoLevel = 0
                };
            }

            colorTexture.SetPixel(0, 0, color);
            colorTexture.Apply();
            return colorTexture;
        }

        /// <summary>
        /// Stashes raw image bytes for the rest of the editor session under <paramref name="key"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="SessionState"/> offers no byte-array channel, so each byte is widened to an
        /// int -- four times the memory, in exchange for a cache that survives domain reloads
        /// without touching disk.
        /// </remarks>
        internal static void SaveTextureToSession(byte[] data, string key)
        {
            SessionState.SetIntArray(key, BytesToInts(data));
        }

        /// <summary>
        /// Decodes an image previously stored by <see cref="SaveTextureToSession"/>, or null when
        /// there is none. A corrupt entry is logged and erased so it cannot fail twice.
        /// </summary>
        internal static Texture2D LoadTextureFromSession(string key)
        {
            int[] stored = SessionState.GetIntArray(key, null);
            if (stored != null)
            {
                try
                {
                    Texture2D texture = new Texture2D(0, 0);
                    texture.LoadImage(IntsToBytes(stored));
                    texture.Apply();
                    return texture;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    SessionState.EraseIntArray(key);
                }
            }

            return null;
        }

        /// <summary>A layout button that shows the link cursor while hovered.</summary>
        internal static bool Button(string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(text), style, options);
        }

        /// <inheritdoc cref="Button(string, GUIStyle, GUILayoutOption[])"/>
        internal static bool Button(GUIContent content, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (style == null)
            {
                style = GUI.skin.button;
            }

            bool clicked = GUILayout.Button(content, style, options);
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            return clicked;
        }

        /// <summary>
        /// Whether <paramref name="rect"/> was left-clicked this event, and marks it as a link for
        /// cursor purposes. Used to make arbitrary drawn areas behave like hyperlinks.
        /// </summary>
        internal static bool IsClicked(Rect rect)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            Event current = Event.current;
            if (current.button == 0 && current.type == EventType.MouseDown)
            {
                return rect.Contains(current.mousePosition);
            }

            return false;
        }

        private static int[] BytesToInts(byte[] bytes)
        {
            int[] ints = new int[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                ints[i] = bytes[i];
            }

            return ints;
        }

        private static byte[] IntsToBytes(int[] ints)
        {
            byte[] bytes = new byte[ints.Length];
            for (int i = 0; i < ints.Length; i++)
            {
                bytes[i] = (byte)ints[i];
            }

            return bytes;
        }
    }
}
