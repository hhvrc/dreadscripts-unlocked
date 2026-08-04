// Reconstructed from: decompiled/ControllerEditor/DreadScripts/Common/SupportThankies/RemoteTexture.cs
//   RemoteTexture           -> RemoteTexture,        lines 8-254
//   TextureLayoutMethod     -> TextureLayoutMethod,  lines 10-16
//   TextureDisplayParams    -> TextureDisplayParams, lines 18-57
//     _Params -> hasValue, _Listener -> tilingX, getter -> tilingY, interceptor -> offset
//   TextureDisplayParams(float)                  -> TextureDisplayParams(float), line 30
//   TextureDisplayParams(float, float)           -> TextureDisplayParams(float, float), line 35
//   TextureDisplayParams(Vector2, float)         -> TextureDisplayParams(Vector2, float), line 40
//   TextureDisplayParams(Vector2, float, float)  -> TextureDisplayParams(Vector2, float, float), line 45
//   TextureDisplayParams.CustomizeIndexer -> NOT PORTED, line 28 -- an always-null static the obfuscator injected as tamper bait
//   TextureDisplayParams.SearchIndexer()  -> NOT PORTED, line 53 -- the null-check on that static; no callers, no effect on behaviour
//   the instance fields, lines 59-75:
//     m_Database -> texture, m_Exporter -> mayLoadFromCache, _Identifier -> url,
//     attr -> autoDownload, _Dispatcher -> cacheKey, _Registry -> IsLoaded,
//     importer -> IsDownloading, printer -> downloadRequested, order -> readyLatched
//   GetTexture()            -> Texture (property), line 78
//   RemoteTexture(..)       -> RemoteTexture(string, bool, string), line 102
//   Download()              -> Download,           line 109
//   DrawPattern()           -> DrawPattern,        line 147
//   Draw(Rect)              -> Draw(Rect),         line 152
//   Draw(Rect, ..)          -> Draw(Rect, TextureLayoutMethod, TextureDisplayParams), line 157
//   Clear()                 -> Clear,              line 202
//   TryLoadFromCache()      -> TryLoadFromCache,   line 216
//   DrawPlaceholder()       -> DrawPlaceholder,    line 233
//   IsReady()               -> IsReady,            line 238
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// NOTES
// GetTexture() carries [SpecialName] in the decompilation, which marks it as a property getter
// ILSpy could not re-form; it is restored to a property here.
//
// ---------------------------------------------------------------------------------------------
// NETWORK ACCESS -- this type performs unattended HTTP requests from the editor.
//
// What is contacted: whatever absolute URL is passed to the constructor. In this package the
// callers are:
//   * SupportWindowAssets  - https://i.imgur.com/iHszIY3.png (support-window icon)
//                            https://i.imgur.com/FMv1R6A.png (Ko-fi button art)
//   * SupporterEntry       - the <bgimage=URL> of a supporter record
//   * TextFragment         - each <image=URL> inside a supporter's name/prefix/suffix
// The latter two are URLs read out of the supporter list SupportWindow downloads, so they are
// attacker-controlled in the sense that whoever can write that file chooses them. All observed
// values point at i.imgur.com.
//
// When: lazily, on the first frame the texture is actually asked to draw itself (see
// <see cref="Texture"/>), and only when autoDownload is set -- which every caller in this package
// does. Nothing is fetched until a support/thankies UI is on screen.
//
// What is sent: a bare UnityWebRequest GET. No headers, query string, cookies or body are added,
// so nothing identifies the user or the project beyond what any HTTP client necessarily reveals
// (source IP, and Unity's default user agent). Responses are decoded as images only.
//
// Failure behaviour: a request that errors out is dropped silently -- the completion handler
// disposes the request and leaves the texture null, so <see cref="Draw(Rect)"/> keeps painting the
// placeholder box forever. No exception, no log, no retry.
//
// Caching: decoded bytes are kept in SessionState under cacheKey, i.e. for the lifetime of the
// editor process, so a domain reload re-uses them instead of re-fetching.
// ---------------------------------------------------------------------------------------------
//
// Audit status: PARTIAL -- every MAP entry above was re-derived from
// decompiled/ControllerEditor/DreadScripts/Common/SupportThankies/RemoteTexture.cs (lines 8-254)
// while writing this header. The NETWORK ACCESS block records observations about live hosts that
// cannot be checked against decompiled/ and was not re-verified.

using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DreadScripts.Common
{
    /// <summary>
    /// A texture that lives at a URL: downloaded on first use, cached in <see cref="SessionState"/>
    /// so it survives domain reloads, and drawn as a plain box until it arrives.
    /// </summary>
    /// <remarks>
    /// Read the NETWORK ACCESS block at the top of this file before using this type -- it fetches
    /// remote content from the editor without asking.
    /// </remarks>
    internal sealed class RemoteTexture
    {
        /// <summary>How the image is mapped onto the rect it is drawn into.</summary>
        internal enum TextureLayoutMethod
        {
            /// <summary>Fills the rect, cropping the overflowing axis.</summary>
            ScaleToFill,
            StretchToFill,
            /// <summary>Fits inside the rect, letterboxing the short axis.</summary>
            ScaleToFit,
            /// <summary>Tiles the image across the rect via texture coordinates.</summary>
            Pattern
        }

        /// <summary>
        /// Tiling and offset for <see cref="TextureLayoutMethod.Pattern"/>. A default-constructed
        /// value means "not supplied", which is why <see cref="hasValue"/> exists rather than the
        /// parameter being a nullable struct.
        /// </summary>
        internal struct TextureDisplayParams
        {
            internal readonly bool hasValue;
            internal readonly float tilingX;
            internal readonly float tilingY;
            internal readonly Vector2 offset;

            /// <summary>Uniform tiling, no offset.</summary>
            internal TextureDisplayParams(float tiling)
                : this(Vector2.zero, tiling, tiling)
            {
            }

            /// <summary>Per-axis tiling, no offset.</summary>
            internal TextureDisplayParams(float tilingX, float tilingY)
                : this(Vector2.zero, tilingX, tilingY)
            {
            }

            /// <summary>Uniform tiling at an offset.</summary>
            internal TextureDisplayParams(Vector2 offset, float tiling)
                : this(offset, tiling, tiling)
            {
            }

            internal TextureDisplayParams(Vector2 offset, float tilingX, float tilingY)
            {
                hasValue = true;
                this.tilingX = tilingX;
                this.tilingY = tilingY;
                this.offset = offset;
            }
        }

        private readonly string url;
        private readonly bool autoDownload;
        private readonly string cacheKey;

        private Texture2D texture;

        /// <summary>
        /// Whether the session cache may still be consulted. Cleared by the first cache probe so a
        /// miss is not paid for on every frame, and set again once a hit or a fresh download has
        /// put something worth re-reading into <see cref="SessionState"/>.
        /// </summary>
        private bool mayLoadFromCache = true;

        private bool downloadRequested;

        /// <summary>
        /// Latches once the texture has been seen as ready during a Layout event, so that the
        /// remaining events of that frame cannot disagree. Without it a download completing
        /// mid-frame would change the number of layout groups between Layout and Repaint.
        /// </summary>
        private bool readyLatched;

        /// <summary>Whether a decoded texture is available (or is recoverable from the cache).</summary>
        internal bool IsLoaded { get; private set; }

        /// <summary>Whether a fetch has been started and not yet handed off to its callback.</summary>
        internal bool IsDownloading { get; private set; }

        internal RemoteTexture(string url, bool autoDownload, string cacheKey)
        {
            this.url = url;
            this.autoDownload = autoDownload;
            this.cacheKey = cacheKey;
        }

        /// <summary>
        /// The decoded texture, or null while it is unavailable. Asking for it is what starts the
        /// download, so callers must tolerate a null for the first few frames.
        /// </summary>
        internal Texture2D Texture
        {
            get
            {
                if (IsLoaded)
                {
                    // The Texture2D does not survive a domain reload even though the cached bytes
                    // do, so a loaded-but-null texture means "re-decode from the session cache".
                    if (mayLoadFromCache && !texture)
                    {
                        TryLoadFromCache();
                    }

                    return texture;
                }

                if (IsDownloading)
                {
                    return null;
                }

                if (autoDownload && !downloadRequested)
                {
                    downloadRequested = true;
                    IsDownloading = true;
                    Download();
                }

                return null;
            }
        }

        /// <summary>
        /// Starts the fetch, unless the session cache can satisfy it. Returns immediately; the
        /// texture appears in a later frame.
        /// </summary>
        internal void Download()
        {
            if (TryLoadFromCache())
            {
                return;
            }

            UnityWebRequest request = new UnityWebRequest(url)
            {
                downloadHandler = new DownloadHandlerBuffer()
            };

            request.SendWebRequest().completed += delegate
            {
                if (!request.isDone || request.isHttpError || request.isNetworkError)
                {
                    // Failures are swallowed on purpose: decorative artwork must never interrupt
                    // the user with an error, and IsDownloading stays false so a later Clear()
                    // plus redraw can try again.
                    request.Dispose();
                    return;
                }

                try
                {
                    byte[] data = request.downloadHandler.data;
                    texture = new Texture2D(0, 0);
                    texture.LoadImage(data);
                    texture.Apply();
                    IsLoaded = true;

                    if (!string.IsNullOrWhiteSpace(cacheKey))
                    {
                        EditorGuiUtils.SaveTextureToSession(data, cacheKey);
                        mayLoadFromCache = true;
                    }
                }
                finally
                {
                    request.Dispose();
                }
            };

            // Cleared straight away rather than in the callback, so the pending request does not
            // block the "not yet requested" path above from ever running again.
            IsDownloading = false;
        }

        /// <summary>Draws the image tiled across <paramref name="rect"/>.</summary>
        internal void DrawPattern(Rect rect, TextureDisplayParams displayParams = default(TextureDisplayParams))
        {
            Draw(rect, TextureLayoutMethod.Pattern, displayParams);
        }

        /// <summary>Draws the image stretched to fill <paramref name="rect"/>.</summary>
        internal void Draw(Rect rect)
        {
            Draw(rect, TextureLayoutMethod.StretchToFill);
        }

        internal void Draw(Rect rect, TextureLayoutMethod layout, TextureDisplayParams displayParams = default(TextureDisplayParams))
        {
            if (!IsReady())
            {
                DrawPlaceholder(rect);
                return;
            }

            if (layout == TextureLayoutMethod.Pattern)
            {
                float tilingX;
                float tilingY;
                Vector2 offset;

                if (displayParams.hasValue)
                {
                    tilingX = displayParams.tilingX;
                    tilingY = displayParams.tilingY;
                    offset = displayParams.offset;
                }
                else
                {
                    // Default tiling is derived from the image's own size against a 256px
                    // reference, so that a small tile repeats more often than a large one.
                    tilingX = tilingY = (Texture.width / 256f + Texture.height / 256f) / 2f;
                    offset = new Vector2(Texture.width / 2f, Texture.height / 2f);
                }

                Rect texCoords = new Rect(
                    offset,
                    new Vector2(rect.width / Texture.width * tilingX, rect.height / Texture.height * tilingY));

                GUI.DrawTextureWithTexCoords(rect, Texture, texCoords);
                return;
            }

            ScaleMode scaleMode;
            switch (layout)
            {
                case TextureLayoutMethod.ScaleToFit:
                    scaleMode = ScaleMode.ScaleToFit;
                    break;
                case TextureLayoutMethod.ScaleToFill:
                    scaleMode = ScaleMode.ScaleAndCrop;
                    break;
                default:
                    scaleMode = ScaleMode.StretchToFill;
                    break;
            }

            GUI.DrawTexture(rect, Texture, scaleMode);
        }

        /// <summary>
        /// Drops the texture and the session cache entry, so the next draw fetches it again.
        /// </summary>
        internal void Clear()
        {
            if (!string.IsNullOrEmpty(cacheKey))
            {
                SessionState.EraseIntArray(cacheKey);
            }

            texture = null;
            readyLatched = false;
            downloadRequested = false;
            IsLoaded = false;
            IsDownloading = false;
            mayLoadFromCache = true;
        }

        /// <summary>
        /// Re-decodes the image from this editor session's cache. Returns whether a texture is now
        /// available.
        /// </summary>
        internal bool TryLoadFromCache()
        {
            if (mayLoadFromCache && !string.IsNullOrWhiteSpace(cacheKey))
            {
                mayLoadFromCache = false;

                Texture2D cached = EditorGuiUtils.LoadTextureFromSession(cacheKey);
                if (cached != null)
                {
                    texture = cached;
                    IsLoaded = true;
                    IsDownloading = false;
                    mayLoadFromCache = true;
                }
            }

            return texture != null;
        }

        private void DrawPlaceholder(Rect rect)
        {
            GUI.Box(rect, GUIContent.none);
        }

        /// <summary>
        /// Whether the texture can be drawn. Once true during a Layout event it stays true for the
        /// rest of the window's life, so the layout of a frame cannot change underneath Unity when
        /// a download lands between Layout and Repaint.
        /// </summary>
        internal bool IsReady()
        {
            if (readyLatched)
            {
                return true;
            }

            if (Texture == null)
            {
                return false;
            }

            if (Event.current.type == EventType.Layout)
            {
                readyLatched = true;
            }

            return true;
        }
    }
}
