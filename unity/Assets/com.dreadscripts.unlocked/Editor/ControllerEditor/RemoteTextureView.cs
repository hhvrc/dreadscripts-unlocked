// Reconstructed from: decompiled/ControllerEditor/DreadScripts/ControllerEditor/RemoteTextureView.cs
//   candidatePolicy   -> cachedTexture,      line 11
//   _ProductPolicy    -> cacheLookupAllowed, line 13
//   _ExpressionPolicy -> onClick,            line 15
//   systemPolicy      -> url,                line 17
//   workerPolicy      -> autoDownload,       line 19
//   _FilterPolicy     -> sessionKey,         line 21
//   m_StubPolicy      -> isLoaded,           line 23
//   m_ReaderPolicy    -> isDownloading,      line 25
//   _BridgePolicy     -> downloadAttempted,  line 27
//   m_StrategyPolicy  -> layoutSettled,      line 29
//   CountHelper       -> texture (property), line 32
//   SetupHelper       -> Download,           line 72
//   EnableHelper      -> TryLoadFromCache,   line 110
//   PublishHelper     -> Draw,               line 127
//   PopHelper         -> DrawFitted(EditorWindow, ...), line 138
//   ComputeHelper     -> DrawFitted(float, float, ...), line 150
//   MoveHelper        -> DrawTexture,        line 171
//   ConcatHelper      -> DrawPlaceholderLayout, line 180
//   CallHelper        -> DrawPlaceholder,    line 186
//   CancelHelper      -> IsReady,            line 191
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// This type is a near-duplicate of DreadScripts.Common.RemoteTexture (the copy the support/thanks
// panel uses): same download-once-then-cache lifecycle, differing in the layout modes it offers and
// in going through CachedTextureContent for session storage. Both are ported, as shipped, rather
// than merged.
//
// The two constructors carry a trailing bool that the shipped build never reads; it is kept so the
// call signatures still match the original.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DreadScripts.ControllerEditor
{
    /// <summary>
    /// An image downloaded from a URL at editor time and drawn as a banner, optionally acting as a
    /// link when clicked.
    /// </summary>
    /// <remarks>
    /// <b>Network behaviour.</b> This is the only type in the tool that reaches the network. It
    /// issues a plain GET to the <c>url</c> its constructor was given — no headers, query string or
    /// body are added, so nothing about the project or the machine is sent beyond what any HTTP
    /// request discloses. The shipped build constructs exactly one instance, in
    /// <c>EditorUtils.setterProcessor</c>, pointing at
    /// <c>https://raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png</c>.
    /// <para>
    /// The request is made lazily, from the first repaint that asks for <see cref="texture"/> while
    /// <see cref="autoDownload"/> is set, and only after the session cache misses. It is attempted at
    /// most once per instance — <see cref="downloadAttempted"/> is never cleared — and the decoded
    /// image is written to <see cref="SessionState"/>, so a successful fetch is reused for the rest
    /// of the editor session and survives domain reloads. In practice that means one request per
    /// Unity launch, the first time a window showing the banner is drawn.
    /// </para>
    /// <para>
    /// Clicking the image runs <see cref="onClick"/>, which for the constructor that does not take
    /// one opens <c>https://dreadrith.com/links</c> in the system browser.
    /// </para>
    /// </remarks>
    internal sealed class RemoteTextureView
    {
        private Texture2D cachedTexture;

        /// <summary>
        /// Whether the session cache may still be consulted. Cleared by the first lookup and set
        /// again only when that lookup found something, so a miss is not retried but a hit can be
        /// re-read after a domain reload has destroyed the texture.
        /// </summary>
        private bool cacheLookupAllowed = true;

        internal Action onClick;

        private readonly string url;

        /// <summary>Whether drawing the image is allowed to trigger the download itself.</summary>
        private readonly bool autoDownload;

        /// <summary><see cref="SessionState"/> key the decoded image is cached under; may be empty.</summary>
        private readonly string sessionKey;

        internal bool isLoaded;

        internal bool isDownloading;

        private bool downloadAttempted;

        /// <summary>
        /// Set once the image was ready at a Layout event. From then on the view reports itself ready
        /// unconditionally, so that a download completing between a window's Layout and Repaint passes
        /// cannot change the number of controls drawn and unbalance the layout groups.
        /// </summary>
        private bool layoutSettled;

        /// <summary>
        /// The image, or null while it is still missing. Reading this is what starts the download.
        /// </summary>
        internal Texture2D texture
        {
            get
            {
                if (isLoaded)
                {
                    // A domain reload destroys the texture but not this object; the cache still has
                    // the bytes.
                    if (cacheLookupAllowed && !cachedTexture)
                    {
                        TryLoadFromCache();
                    }

                    return cachedTexture;
                }

                if (isDownloading)
                {
                    return null;
                }

                if (!autoDownload || downloadAttempted)
                {
                    return null;
                }

                downloadAttempted = true;
                isDownloading = true;
                Download();
                return null;
            }
        }

        /// <summary>
        /// Creates a view whose image links to the author's link page when clicked.
        /// </summary>
        internal RemoteTextureView(string url, bool autoDownload, string sessionKey, bool unused = false)
            : this(delegate
            {
                Application.OpenURL("https://dreadrith.com/links");
            }, url, autoDownload, sessionKey, unused)
        {
        }

        internal RemoteTextureView(Action onClick, string url, bool autoDownload, string sessionKey, bool unused = false)
        {
            this.url = url;
            this.autoDownload = autoDownload;
            this.sessionKey = sessionKey;
            this.onClick = onClick;
        }

        /// <summary>
        /// Fetches the image, unless the session cache already has it. Returns immediately; the
        /// texture appears on a later frame.
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
                // The shipped build tested isDone/isHttpError/isNetworkError; those properties are
                // deprecated, and result is their documented equivalent.
                if (!request.isDone || request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                {
                    // A failure is simply dropped: nothing is logged and nothing is retried, so a
                    // machine with no network access just never shows the banner.
                    request.Dispose();
                    return;
                }

                try
                {
                    byte[] data = request.downloadHandler.data;
                    cachedTexture = new Texture2D(0, 0);
                    cachedTexture.LoadImage(data);
                    cachedTexture.Apply();
                    isLoaded = true;

                    if (!string.IsNullOrWhiteSpace(sessionKey))
                    {
                        CachedTextureContent.SaveTexture(data, sessionKey);
                        cacheLookupAllowed = true;
                    }
                }
                finally
                {
                    request.Dispose();
                }
            };

            // Cleared as soon as the request is in flight rather than when it completes, which looks
            // like an oversight; what actually stops a second request is downloadAttempted, set by
            // the caller in the texture getter. Ported as it stands.
            isDownloading = false;
        }

        /// <summary>
        /// Restores the image from the session cache if it is there. Returns whether an image is now
        /// available.
        /// </summary>
        internal bool TryLoadFromCache()
        {
            if (cacheLookupAllowed && !string.IsNullOrWhiteSpace(sessionKey))
            {
                cacheLookupAllowed = false;
                Texture2D cached = CachedTextureContent.LoadTexture(sessionKey);
                if (cached != null)
                {
                    cachedTexture = cached;
                    isLoaded = true;
                    isDownloading = false;
                    cacheLookupAllowed = true;
                }
            }

            return cachedTexture != null;
        }

        /// <summary>
        /// Draws the image across the full layout width, at its own aspect ratio.
        /// </summary>
        /// <param name="placeholderAspect">
        /// Width-to-height ratio of the box drawn while the image is unavailable, so the layout does
        /// not jump when it arrives. The default suits a wide banner.
        /// </param>
        internal void Draw(float placeholderAspect = 7f)
        {
            if (!IsReady())
            {
                DrawPlaceholderLayout(placeholderAspect);
                return;
            }

            Rect rect = GUILayoutUtility.GetAspectRect((float)texture.width / texture.height);
            DrawTexture(rect);
        }

        /// <summary>
        /// Draws the image sized to <paramref name="window"/>, falling back to <see cref="Draw"/>
        /// when there is no window to measure.
        /// </summary>
        internal void DrawFitted(EditorWindow window, float xOffset = 0f, float bottomMargin = 60f, float placeholderAspect = 7f)
        {
            if (window == null)
            {
                Draw(placeholderAspect);
            }
            else
            {
                DrawFitted(window.position.width, window.position.height, xOffset, bottomMargin, placeholderAspect);
            }
        }

        /// <summary>
        /// Draws the image as wide as <paramref name="availableWidth"/>, shrinking it if that would
        /// leave less than <paramref name="bottomMargin"/> of the available height for whatever is
        /// drawn below, and centres what results.
        /// </summary>
        internal void DrawFitted(float availableWidth, float availableHeight, float xOffset = 0f, float bottomMargin = 60f, float placeholderAspect = 7f)
        {
            if (!IsReady())
            {
                DrawPlaceholderLayout(placeholderAspect);
                return;
            }

            float heightPerWidth = (float)texture.height / texture.width;
            float width = availableWidth;
            float height = width * heightPerWidth;
            float maxHeight = availableHeight - bottomMargin;

            if (height > maxHeight)
            {
                height = maxHeight;
                width = height / heightPerWidth;
            }

            Rect rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(expand: false));
            rect.x += (availableWidth - width) / 2f + xOffset;
            DrawTexture(rect);
        }

        private void DrawTexture(Rect rect)
        {
            // The click is handled before the draw so that the rect is claimed for the current event
            // regardless of which one it is.
            if (onClick != null && EditorUtils.ClickArea(rect))
            {
                onClick();
            }

            GUI.DrawTexture(rect, texture);
        }

        private void DrawPlaceholderLayout(float aspect = 7f)
        {
            Rect rect = GUILayoutUtility.GetAspectRect(aspect);
            DrawPlaceholder(rect);
        }

        private void DrawPlaceholder(Rect rect)
        {
            GUI.Box(rect, GUIContent.none);
        }

        /// <summary>Whether there is an image to draw.</summary>
        internal bool IsReady()
        {
            if (layoutSettled)
            {
                return true;
            }

            if (texture == null)
            {
                return false;
            }

            if (Event.current.type == EventType.Layout)
            {
                layoutSettled = true;
            }

            return true;
        }
    }
}
