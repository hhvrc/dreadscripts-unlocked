// Reconstructed from: decompiled/ADOverhaul2022/DreadScripts/ADOverhaul/ADOEditorUtility.cs
//   nested class BannerDownloader -> BannerDownloader, lines 918-1101
//   GetTexture()                  -> Texture (property), line 939
//   .ctor(string, bool, string, bool) -> .ctor(string, bool, string), line 963
//   Download                      -> Download,          line 970
//   TryLoadFromCache              -> TryLoadFromCache,  line 1008
//   Draw()                        -> Draw(),            line 1025
//   Draw(EditorWindow, float, float) -> Draw(EditorWindow, float, float), line 1034
//   Draw(float, float, float, float) -> Draw(float, float, float, float), line 1049
//   Draw(Rect)                    -> Draw(Rect),        line 1065
//   CanDraw                       -> CanDraw,           line 1085
//   static field getterSerializer -> banner,            line 2106
// Line numbers are relative to the decompiled snapshot at the time of the port; the type and
// member names are the durable reference.
// Audit status: VERIFIED against decompiled/ -- every field and every statement below was transcribed
// from the region above.
//
// DEOBF-BUG(resolved): GetTexture carried [SpecialName] with no matching setter -- ILSpy's
// rendering of a property getter it could not re-form. Restored as a read-only property, matching
// CachedIcon and RemoteTexture.
//
// Almost every member kept its original name: texture, canResolve, url, autoDownload, cacheKey,
// isLoaded, isDownloading, hasRequestedDownload, isReady, Download, TryLoadFromCache, Draw and
// CanDraw all read as English and none rhymes with the Serializer family. Only the constructor
// parameters are generated names, and a fourth constructor parameter (`striplast2`) is assigned by
// nothing and read by nothing -- it is dropped here, and the one call site passes only three
// arguments.
//
// ---------------------------------------------------------------------------------------------
// NETWORK ACCESS -- this type performs an unattended HTTP request from the editor, and its click
// handler opens an external URL in the user's browser.
//
// What is contacted: whatever absolute URL is passed to the constructor. The single instance in
// this package is `banner` below, which fetches
//     https://raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png
// -- the vendor's banner image, shown at the top of the ADOverhaul window. Clicking the drawn banner
// opens https://dreadrith.com/links.
//
// When: lazily, the first time the texture is asked to draw itself, and only because autoDownload is
// set. Nothing is fetched until the window is on screen. The result is cached in SessionState under
// the key "DreadBanner.png" through CachedIcon, so it survives a domain reload but not an editor
// restart.
//
// This is presentation only -- no identifiers are sent, and the response is used as an image and
// nothing else. It is recorded here because a restored package should not make a network call the
// reader does not know about; see DreadScripts.Common.RemoteTexture for the same disclosure on the
// other product's downloader.
// ---------------------------------------------------------------------------------------------
//
// Overlap with DreadScripts.Common.RemoteTexture, deliberately not consolidated: the two share a
// download/cache core -- the same lazy-fetch state machine, the same isLoaded/isDownloading/
// canResolve flags -- but their drawing halves are unrelated. RemoteTexture draws into a caller's
// Rect with a layout mode and tiling parameters; this one owns its layout, fitting itself to an
// aspect ratio or to a window, and carries a fixed click-through. Merging them would mean either
// giving RemoteTexture a hard-coded vendor URL or giving this one a layout API nothing uses.

using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DreadScripts.ADOverhaul
{
    internal static partial class ADOEditorUtility
    {
        /// <summary>
        /// The vendor's banner image, shown at the top of the tool's window. Fetched once per
        /// session; see the network-access note at the top of this file.
        /// </summary>
        internal static readonly BannerDownloader banner = new BannerDownloader(
            "https://raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png",
            autoDownload: true,
            "DreadBanner.png");

        /// <summary>
        /// A remote image that downloads itself on first draw, caches itself for the session, and
        /// lays itself out as a full-width banner.
        /// </summary>
        internal sealed class BannerDownloader
        {
            private Texture2D texture;

            /// <summary>
            /// Whether the session cache is still worth consulting. Cleared for the duration of a
            /// lookup so a miss is not retried on every repaint.
            /// </summary>
            private bool canResolve = true;

            private readonly string url;

            private readonly bool autoDownload;

            /// <summary>SessionState key the downloaded bytes are cached under; empty disables caching.</summary>
            private readonly string cacheKey;

            internal bool isLoaded;

            internal bool isDownloading;

            /// <summary>Latched once the download has been kicked off, so it is never started twice.</summary>
            private bool hasRequestedDownload;

            /// <summary>Latched on the first Layout event after the texture arrived; see <see cref="CanDraw"/>.</summary>
            private bool isReady;

            /// <summary>
            /// The image, or null while it is still being fetched. Reading it is what starts the
            /// fetch.
            /// </summary>
            internal Texture2D Texture
            {
                get
                {
                    if (isLoaded)
                    {
                        // The texture itself does not survive a domain reload even though isLoaded
                        // does, so a reload is detected here and repaired from the cache.
                        if (canResolve && !texture)
                        {
                            TryLoadFromCache();
                        }

                        return texture;
                    }

                    if (isDownloading || !autoDownload || hasRequestedDownload)
                    {
                        return null;
                    }

                    hasRequestedDownload = true;
                    isDownloading = true;
                    Download();
                    return null;
                }
            }

            internal BannerDownloader(string url, bool autoDownload, string cacheKey)
            {
                this.url = url;
                this.autoDownload = autoDownload;
                this.cacheKey = cacheKey;
            }

            /// <summary>
            /// Fetches the image, unless the session cache already has it.
            /// </summary>
            /// <remarks>
            /// The request is fire-and-forget: nothing awaits it and nothing reports a failure, so a
            /// machine with no network simply never shows the banner. A failed or errored response
            /// disposes the request and leaves every flag as it was, which -- together with
            /// <see cref="hasRequestedDownload"/> -- means one failure ends the attempt for the
            /// session.
            /// </remarks>
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
                        request.Dispose();
                        return;
                    }

                    try
                    {
                        byte[] data = request.downloadHandler.data;
                        texture = new Texture2D(0, 0);
                        texture.LoadImage(data);
                        texture.Apply();
                        isLoaded = true;

                        if (!string.IsNullOrWhiteSpace(cacheKey))
                        {
                            CachedIcon.SaveToCache(data, cacheKey);
                            canResolve = true;
                        }
                    }
                    finally
                    {
                        request.Dispose();
                    }
                };

                // Cleared immediately rather than in the callback, so this is false for the whole
                // time the request is actually in flight. Reproduced as shipped; nothing in this
                // package reads it.
                isDownloading = false;
            }

            /// <summary>Tries to fill <see cref="texture"/> from the session cache.</summary>
            /// <returns>Whether a texture is now present -- which may be one loaded earlier.</returns>
            internal bool TryLoadFromCache()
            {
                if (canResolve && !string.IsNullOrWhiteSpace(cacheKey))
                {
                    canResolve = false;

                    Texture2D cached = CachedIcon.LoadFromCache(cacheKey);
                    if (cached != null)
                    {
                        texture = cached;
                        isLoaded = true;
                        isDownloading = false;
                        canResolve = true;
                    }
                }

                return texture;
            }

            /// <summary>Draws the banner at the full width of the current layout area.</summary>
            internal void Draw()
            {
                if (!CanDraw())
                {
                    return;
                }

                Draw(GUILayoutUtility.GetAspectRect((float)Texture.width / (float)Texture.height));
            }

            /// <summary>
            /// Draws the banner fitted to <paramref name="window"/>, so it never takes so much of a
            /// short window that nothing else fits.
            /// </summary>
            /// <param name="offsetX">Horizontal nudge applied after centring.</param>
            /// <param name="reservedHeight">Height to leave for the rest of the window's content.</param>
            internal void Draw(EditorWindow window, float offsetX = 0f, float reservedHeight = 60f)
            {
                if (!CanDraw())
                {
                    return;
                }

                if (window == null)
                {
                    Draw();
                }
                else
                {
                    Draw(window.position.width, window.position.height, offsetX, reservedHeight);
                }
            }

            /// <summary>
            /// Draws the banner across <paramref name="availableWidth"/>, shrinking it if that would
            /// leave less than <paramref name="reservedHeight"/> of
            /// <paramref name="availableHeight"/> for everything else.
            /// </summary>
            /// <remarks>
            /// The rect is requested without width expansion and then centred by hand, because a
            /// shrunk banner would otherwise be laid out against the left edge.
            /// </remarks>
            internal void Draw(float availableWidth, float availableHeight, float offsetX = 0f, float reservedHeight = 60f)
            {
                float aspect = (float)Texture.height / (float)Texture.width;

                float width = availableWidth;
                float height = width * aspect;

                float maxHeight = availableHeight - reservedHeight;
                if (height > maxHeight)
                {
                    height = maxHeight;
                    width = height / aspect;
                }

                Rect rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(expand: false));
                rect.x += (availableWidth - width) / 2f + offsetX;
                Draw(rect);
            }

            /// <summary>Draws the banner into <paramref name="rect"/> and handles a click on it.</summary>
            /// <remarks>
            /// A left click opens the vendor's links page in the user's browser; see the
            /// network-access note at the top of this file.
            /// </remarks>
            private void Draw(Rect rect)
            {
                Event current = Event.current;

                if (current.type == EventType.MouseDown && rect.Contains(current.mousePosition) && current.button == 0)
                {
                    Application.OpenURL("https://dreadrith.com/links");
                    current.Use();
                }

                if (Event.current.type == EventType.Repaint)
                {
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                }

                GUI.DrawTexture(rect, Texture);
            }

            /// <summary>Whether the image has arrived and it is safe to lay it out.</summary>
            /// <remarks>
            /// The Layout latch is the point of this method. IMGUI requires the same sequence of
            /// layout calls on the Layout event and on the Repaint event that follows it, so a
            /// texture that arrives between the two would otherwise add a control mid-frame and
            /// desynchronise the layout. Waiting for a Layout event to see it first, and latching
            /// that, keeps the two passes in step.
            /// </remarks>
            internal bool CanDraw()
            {
                if (isReady)
                {
                    return true;
                }

                if (Texture == null)
                {
                    return false;
                }

                if (Event.current.type == EventType.Layout)
                {
                    isReady = true;
                }

                return true;
            }
        }
    }
}
