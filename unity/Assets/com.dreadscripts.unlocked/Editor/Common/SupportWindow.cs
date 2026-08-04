// Reconstructed from: decompiled/ControllerEditor/DreadScripts/Common/SupportThankies/SupportWindow.cs
//   SupportWindow      -> the EditorWindow subclass of the same name, lines 11-256
//   the static fields, lines 13-39:
//     m_Advisor -> isFetching, _Callback -> isLoaded, m_Indexer -> hasFailed, issuer -> errorMessage,
//     m_Prototype -> headerContent, rule -> supporters, m_Singleton -> rawSupporterData,
//     _Factory -> gridRect, _Account -> lastRepaintGridRect, _Ref -> scrollPosition,
//     m_Status -> columnSplitterState, code -> rowSplitterState, m_Dic -> columnCount,
//     _Invocation -> rowCount
//   IsDone()           -> IsDone (property), line 42
//   InitHeaderContent  -> InitHeaderContent,  line 51
//   DrawButton         -> DrawButton,         line 56
//   Open               -> Open,               line 67
//   OnGUI              -> OnGUI,              line 72
//   DrawSupporters     -> DrawSupporters,     line 105
//   DrawKofiButton     -> DrawKofiButton,     line 175
//   OpenKofi           -> OpenKofi,           line 187
//   FetchSupporters    -> FetchSupporters,    line 192
//   ParseRawData       -> ParseRawData,       line 233
//   OnEnable           -> OnEnable,           line 244
//   ResetState         -> ResetState,         line 249
// Line numbers are relative to the decompiled snapshot at the time of the port; the member names
// are the durable reference.
//
// DELIBERATE DEVIATION
// The two URLs are string literals in the decompiled source -- the supporter list inline in
// FetchSupporters (line 199) and the Ko-fi link inline in OpenKofi (line 189). They are hoisted to
// the `SupportersUrl` and `KofiUrl` constants here so that every network destination this type
// reaches is visible at the top of the class; the values are unchanged.
//
// IsDone() carries [SpecialName] in the decompilation, marking it as a property getter ILSpy could
// not re-form; it is restored to a property here.
//
// Uses DreadScripts.Common.GUILayoutUtils in place of the EditorLayoutUtils copy that sits beside
// this type in the decompiled source; it is already ported and behaves identically.
//
// ---------------------------------------------------------------------------------------------
// NETWORK ACCESS -- opening this window makes unattended HTTP requests from the editor.
//
//   GET https://storage.googleapis.com/dreadscripts-c6b62.appspot.com/Dreadscripts/Supporters.txt
//     The supporter list. Requested from OnGUI the first time the window paints without a
//     result, i.e. only once the user has opened it, and again on Retry. Plain GET with a 10s
//     timeout: no auth, no query string, no body, no custom headers -- nothing is uploaded and
//     nothing identifies the user or the project. The response is a plain-text file, one
//     supporter per line, parsed by SupporterEntry.
//
//     This is a public object in the vendor's Firebase/Google Cloud Storage bucket
//     (project dreadscripts-c6b62). Despite the vendor's own backend being gone, the bucket is
//     still served: the URL returned 200 with live content when this port was made. Should it
//     later go away, the failure is contained -- FetchSupporters records the error, OnGUI shows
//     "Failed to load supporters." with the message and a Retry button, and the rest of the
//     package is unaffected. Nothing else in the tool depends on this request.
//
//   Supporter artwork
//     Each parsed entry may carry image URLs (<bgimage=...>, <image=...>) which RemoteTexture then
//     fetches; so far all of them point at i.imgur.com. See the network notes on RemoteTexture.
//
//   https://i.imgur.com/iHszIY3.png, https://i.imgur.com/FMv1R6A.png
//     Window icon and Ko-fi banner, fetched by SupportWindowAssets on first draw. Both were live
//     when this port was made.
//
//   https://ko-fi.com/dreadrith
//     Opened in the user's browser, and only in response to a click on the banner.
//
// Nothing here runs unless the user opens the window (or draws DrawButton, which only fetches the
// icon).
// ---------------------------------------------------------------------------------------------
//
// Audit status: PARTIAL -- every MAP entry above was re-derived from
// decompiled/ControllerEditor/DreadScripts/Common/SupportThankies/SupportWindow.cs (lines 11-256)
// while writing this header, as were the two hoisted URL literals. The NETWORK ACCESS block records
// observations about live hosts that cannot be checked against decompiled/ and was not re-verified.

using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DreadScripts.Common
{
    /// <summary>
    /// The "thankies" window: a grid of the people who support the author, downloaded at open time.
    /// </summary>
    internal class SupportWindow : EditorWindow
    {
        /*
         * <namecolor=#9B9DFB><prefix=<image=https://i.imgur.com/lFaZH7e.png>><bgimage=https://i.imgur.com/0j03vZ3.png><bgtype=scaletofill><bordercolor=#9B9DFB><tooltip=I love mah waif><onclick=https://zioketski.gumroad.com/>
         * <name=JelleJurre><namecolor=#FFC0CB><prefix=<color=#FFC0CB>â¤ï¸Ž</color>><suffix=<color=#FFC0CB>â¤ï¸Ž</color>><bordercolor=#FFC0CB><tooltip=Just because I'm a programmer doens't mean I'm a femboy. I mean, I am a femboy, but not because I'm a programmer!><onclick=https://jellejurre.dev/>
         * <name=ScarlettKat><namecolor=#FFCC13><prefix=<image=https://i.imgur.com/9o7yVIv.png>><suffix=<image=https://i.imgur.com/9o7yVIv.png>><bordercolor=#FFCC13>
         * <name=GHOST XO><namecolor=#FFCC13><prefix=<image=https://i.imgur.com/9o7yVIv.png>><suffix=<image=https://i.imgur.com/9o7yVIv.png>><bordercolor=#FFCC13>
         * <name=Buddy><namecolor=#FFCC13><prefix=<image=https://i.imgur.com/9o7yVIv.png>><suffix=<image=https://i.imgur.com/9o7yVIv.png>><bordercolor=#FFCC13>
         * <name=Z><namecolor=#FFCC13><prefix=<image=https://i.imgur.com/9o7yVIv.png>><suffix=<image=https://i.imgur.com/9o7yVIv.png>><bordercolor=#FFCC13>
         * <name=hfcRed><namecolor=#FFCC13><prefix=<image=https://i.imgur.com/9o7yVIv.png>><suffix=<image=https://i.imgur.com/9o7yVIv.png>><bordercolor=#FFCC13>
         * <name=Somebody><namecolor=#FFCC13><prefix=<image=https://i.imgur.com/9o7yVIv.png>><suffix=<image=https://i.imgur.com/9o7yVIv.png>><bordercolor=#FFCC13>
         */
        private const string SupportersUrl = "https://storage.googleapis.com/dreadscripts-c6b62.appspot.com/Dreadscripts/Supporters.txt";
        private const string KofiUrl = "https://ko-fi.com/dreadrith";

        private static bool isFetching;
        private static bool isLoaded;
        private static bool hasFailed;
        private static string errorMessage;

        private static GUIContent headerContent;
        private static SupporterEntry[] supporters;
        private static string rawSupporterData;

        /// <summary>The grid area as of the previous Repaint; see <see cref="DrawSupporters"/>.</summary>
        private static Rect gridRect = Rect.zero;
        private static Rect lastRepaintGridRect = Rect.zero;
        private static Vector2 scrollPosition;

        private static object columnSplitterState = GUILayoutUtils.CreateSplitterState(1f);
        private static object rowSplitterState = GUILayoutUtils.CreateSplitterState(1f);
        private static int columnCount = 1;
        private static int rowCount = 1;

        /// <summary>Whether the fetch has reached a conclusion, successful or not.</summary>
        private static bool IsDone
        {
            get
            {
                if (isLoaded)
                {
                    return true;
                }

                return hasFailed;
            }
        }

        private static void InitHeaderContent()
        {
            headerContent = new GUIContent(
                SupporterStrings.HeaderTexts.RandomElement(),
                SupporterStrings.HeaderTooltips.RandomElement());
        }

        /// <summary>
        /// Draws the small heart button that opens this window, sized to sit in a toolbar row.
        /// </summary>
        public static void DrawButton()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 16f, GUIStyle.none, GUILayout.Width(16f));
            rect.x -= 2f;

            SupportWindowAssets.Textures.Icon.Draw(rect);

            if (EditorGuiUtils.IsClicked(rect))
            {
                Open();
            }
        }

        public static void Open()
        {
            EditorWindow.GetWindow<SupportWindow>(SupporterStrings.WindowTitles.RandomElement()).titleContent.image =
                SupportWindowAssets.Textures.Icon.Texture;
        }

        public void OnGUI()
        {
            // The fetch is kicked off from OnGUI, not OnEnable, so that a window restored by a
            // domain reload retries rather than sitting empty.
            if (!IsDone && !isFetching)
            {
                FetchSupporters();
            }

            if (isFetching)
            {
                GUILayout.Label("Loading supporters...", SupportWindowAssets.Styles.Header);
            }

            if (hasFailed)
            {
                GUILayout.Label("Failed to load supporters.", SupportWindowAssets.Styles.Header);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
                }

                if (EditorGuiUtils.Button("Retry", EditorStyles.toolbarButton))
                {
                    ResetState();
                }
            }

            if (isLoaded)
            {
                using (new GUILayout.HorizontalScope("in bigtitle"))
                {
                    GUILayout.Label(headerContent, SupportWindowAssets.Styles.Header);
                }

                DrawSupporters();
            }

            DrawKofiButton();
        }

        /// <summary>
        /// Lays the supporter cards out in a grid whose shape follows the window, then draws them
        /// into a scroll view.
        /// </summary>
        /// <remarks>
        /// The grid is drawn inside <see cref="GUILayout.BeginArea(Rect)"/>, which needs a rect up
        /// front, but the rect itself comes from the layout system. The Repaint rect is therefore
        /// carried over into the following Layout event, which costs one frame of lag when the
        /// window is resized and keeps the two passes agreeing on the same area.
        /// </remarks>
        public void DrawSupporters()
        {
            Event current = Event.current;
            Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(60f), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (current.type == EventType.Repaint)
            {
                lastRepaintGridRect = controlRect;
            }
            else if (current.type == EventType.Layout)
            {
                gridRect = lastRepaintGridRect;
            }

            int count = supporters.Length;

            // Prefer as many columns as the window's aspect ratio suggests, but no more than are
            // needed to keep every card at least ~29px tall, and never fewer than one.
            float aspect = gridRect.width / gridRect.height;
            int columns = Mathf.Clamp(
                Mathf.Min(Mathf.RoundToInt(aspect), Mathf.CeilToInt(29f * count / gridRect.height)),
                1,
                count);
            int rows = Mathf.CeilToInt((float)count / columns);

            // The splitter states carry the sizes the user has dragged panes to, so they are only
            // rebuilt when the grid's shape actually changes.
            if (columnCount != columns)
            {
                columnCount = columns;
                columnSplitterState = GUILayoutUtils.CreateSplitterState(Enumerable.Repeat(1f, columns).ToArray());
            }

            if (rowCount != rows)
            {
                rowCount = rows;
                rowSplitterState = GUILayoutUtils.CreateSplitterState(Enumerable.Repeat(1f, rows).ToArray());
            }

            GUILayout.BeginArea(gridRect);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int index = 0;
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(4f);
                GUILayoutUtils.BeginSplit(columnSplitterState, null, false);

                for (int column = 0; column < columns; column++)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        using (new GUILayout.VerticalScope())
                        {
                            GUILayoutUtils.BeginSplit(rowSplitterState, null, true);

                            for (int row = 0; row < rows; row++)
                            {
                                if (index < supporters.Length)
                                {
                                    supporters[index++].DrawCard(25f);
                                }
                                else
                                {
                                    // The trailing cells of the last column still have to be
                                    // drawn, or the splitter would see fewer children than it has
                                    // panes.
                                    GUILayout.Label(GUIContent.none);
                                }
                            }

                            GUILayoutUtils.EndSplit();
                        }

                        if (column < columns - 1)
                        {
                            GUILayout.Space(4f);
                        }
                    }
                }

                GUILayoutUtils.EndSplit();
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>Draws the Ko-fi banner on its brand-blue rounded plate.</summary>
        public static void DrawKofiButton()
        {
            Rect rect = GUILayoutUtility.GetRect(100f, 200f, 16f, 32f);
            Rect bannerRect = EditorGuiUtils.FitAspectRatio(rect, 6.25f);

            GUI.DrawTexture(rect, EditorGuiUtils.GetColorTexture(Color.white), ScaleMode.StretchToFill, false, 0f, new Color(0.075f, 0.765f, 1f), 0f, 8f);
            SupportWindowAssets.Textures.KofiBanner.Draw(bannerRect);

            if (EditorGuiUtils.IsClicked(rect))
            {
                OpenKofi();
            }
        }

        public static void OpenKofi()
        {
            Application.OpenURL(KofiUrl);
        }

        /// <summary>
        /// Downloads the supporter list. See the network notes at the top of this file.
        /// </summary>
        public async Task FetchSupporters()
        {
            if (IsDone || isFetching)
            {
                return;
            }

            isFetching = true;

            WebRequestJob job = new WebRequestJob(SupportersUrl, method: "GET");
            try
            {
                UnityWebRequest request = job.Request;
                request.useHttpContinue = false;
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = 10;

                await job.Process();
                isFetching = false;

                if (job.IsError)
                {
                    hasFailed = true;
                    errorMessage = request.error;
                    return;
                }

                try
                {
                    rawSupporterData = request.downloadHandler.text;
                    ParseRawData();
                    isLoaded = true;
                }
                catch (Exception exception)
                {
                    // Recorded for the Retry UI and then rethrown, so a malformed list also shows
                    // up in the console rather than only in the window.
                    hasFailed = true;
                    errorMessage = exception.ToString();
                    throw;
                }
            }
            finally
            {
                job.Dispose();
            }
        }

        /// <summary>Turns the downloaded text into one <see cref="SupporterEntry"/> per line.</summary>
        public void ParseRawData()
        {
            string[] lines = rawSupporterData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            supporters = new SupporterEntry[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                supporters[i] = new SupporterEntry(lines[i]);
            }

            // The fetch completed off the GUI thread's schedule, so the window has to be told.
            Repaint();
        }

        public void OnEnable()
        {
            InitHeaderContent();
        }

        /// <summary>Clears the outcome of the last fetch so that OnGUI starts a new one.</summary>
        public static void ResetState()
        {
            hasFailed = false;
            isLoaded = false;
            isFetching = false;
            errorMessage = null;
        }
    }
}
