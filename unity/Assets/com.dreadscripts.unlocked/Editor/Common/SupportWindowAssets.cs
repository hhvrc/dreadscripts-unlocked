// Reconstructed from: decompiled/ControllerEditor/DreadScripts/Common/SupportThankies/SupportWindowAssets.cs
//
//   TextureAssets                -> TextureAssets, lines 9-14
//     merchant                   -> Icon
//     _Authentication            -> KofiBanner
//   StyleAssets                  -> StyleAssets, lines 16-48
//     _Pool                      -> Header
//     composer                   -> Name
//     repository                 -> Prefix
//     m_Mapping                  -> Suffix
//   _Object                      -> textures, line 50
//   m_Utils                      -> styles, line 52
//   [SpecialName] GetTextures()  -> Textures (property), line 55
//   [SpecialName] GetStyles()    -> Styles (property), line 61
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// NOTES
// The two accessor pairs were [SpecialName] methods in the decompilation, which is what a property
// getter decompiles to when the metadata for the property itself is stripped; they are restored as
// properties here. The two backing fields were `internal` in the source and are `private` here --
// nothing outside the type touched them.
//
// Audit status: VERIFIED against decompiled/ -- the whole decompiled file is 65 lines and every
// member, URL, cache key and style value above was read off it on 2026-08-05.

using UnityEditor;
using UnityEngine;

namespace DreadScripts.Common
{
    /// <summary>
    /// Lazily built textures and styles for <see cref="SupportWindow"/>.
    /// </summary>
    /// <remarks>
    /// The assets live in nested classes behind properties rather than as static fields, because
    /// <see cref="GUIStyle"/> construction reads <see cref="EditorStyles"/>, which is only valid
    /// once the editor GUI has been initialised -- a static field initialiser can run earlier than
    /// that and would silently produce broken styles.
    /// </remarks>
    internal static class SupportWindowAssets
    {
        /// <summary>Remote artwork. See the network notes on <see cref="RemoteTexture"/>.</summary>
        internal class TextureAssets
        {
            /// <summary>The heart icon used for the toolbar button and the window title.</summary>
            internal readonly RemoteTexture Icon = new RemoteTexture("https://i.imgur.com/iHszIY3.png", true, "ds-supporters-main");

            /// <summary>The Ko-fi banner drawn at the bottom of the window.</summary>
            internal readonly RemoteTexture KofiBanner = new RemoteTexture("https://i.imgur.com/FMv1R6A.png", true, "ds-supporters-kofi");
        }

        internal class StyleAssets
        {
            /// <summary>The window heading and the loading/failure messages.</summary>
            internal readonly GUIStyle Header = new GUIStyle(EditorStyles.whiteLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 18
            };

            /// <summary>A supporter's name, centred on their card.</summary>
            internal readonly GUIStyle Name = new GUIStyle(EditorStyles.whiteLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 16,
                richText = true
            };

            /// <summary>The decoration above a supporter's name, pinned to the left.</summary>
            internal readonly GUIStyle Prefix = new GUIStyle(EditorStyles.whiteLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 16,
                richText = true
            };

            /// <summary>The decoration below a supporter's name, pinned to the right.</summary>
            internal readonly GUIStyle Suffix = new GUIStyle(EditorStyles.whiteLabel)
            {
                alignment = TextAnchor.MiddleRight,
                fontStyle = FontStyle.Bold,
                fontSize = 16,
                richText = true
            };
        }

        private static TextureAssets textures;
        private static StyleAssets styles;

        internal static TextureAssets Textures
        {
            get
            {
                return textures ?? (textures = new TextureAssets());
            }
        }

        internal static StyleAssets Styles
        {
            get
            {
                return styles ?? (styles = new StyleAssets());
            }
        }
    }
}
