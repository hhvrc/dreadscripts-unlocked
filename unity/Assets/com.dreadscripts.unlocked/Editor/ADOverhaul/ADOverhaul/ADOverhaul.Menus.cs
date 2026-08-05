// Reconstructed from: reverse-engineering/export/ADOverhaul2022/DreadScripts/ADOverhaul/ADOverhaul.cs
//
// Ported region: the tool's chrome -- the hamburger context menu, the update and announcement
// banners, the shared foldout box and the scene-view overlay frame (decompiled lines 7904-8094 and
// 8185-8296). Line numbers move with the snapshot; the decompiled names below are the durable
// reference. Field references go through the table in ADOverhaul.State.cs.
//
//   SortIdentifier      -> DrawToolHeader,           line 7904
//   InvokeIdentifier    -> ShowContextMenu,          line 7936
//   CustomizeIdentifier -> DrawUpdateBanner,         line 8020
//   ConcatIdentifier    -> DrawAnnouncementBanner,   line 8059
//   SetupIdentifier     -> ApplyCachedUpdateInfo,    line 8185
//   SelectIdentifier    -> DrawFoldoutBox,           line 8228
//   WriteIdentifier     -> DrawTitledOverlay,        line 8249
//   MoveIdentifier      -> DrawOverlay,              line 8266
//   PublishIdentifier   -> DrawSettingsButton,       line 8290
//
// This file was written in two passes. The first could port only PublishIdentifier, because
// everything else here reads ADOSettings, which was not reconstructed yet. ADOSettings has since
// landed at Editor/ADOverhaul/ADOSettings/ and the rest of the region is now complete; nothing in it
// is still deferred. What remains unported is dropped on purpose, and each drop is accounted for
// below.
//
// ========================= THREE HELPERS THIS REGION USES, DEFINED BELOW =======================
//
// Three members this file calls are decompiled lines 7770, 7775 and 7784 -- strictly the tail of the
// update-request region, not of this one, but every surviving caller is in this file:
//
//   CalculateIdentifier -> RepaintOpenWindowsDelayed, line 7770
//   CalcIdentifier      -> RepaintOpenWindows,        line 7775
//   DeleteIdentifier    -> DrawCreditLink,            line 7784
//
// Ownership note, since the history is confusing and the files disagreed for a moment. This file and
// ADOverhaul.Licensing.cs were written in the same wave, and each ended up believing the other
// defined these three -- so both omitted them and the package briefly did not compile. They live
// here, next to their callers; ADOverhaul.Licensing.cs's header points at this file and defines
// nothing. None of the three touches the licence or the network: two are repaint helpers and one
// draws the author credit link.
//
// ===================== DROPPED: THE DEAD UPDATE REQUEST AND WHAT HANGS OFF IT ===================
//
// CancelIdentifier (line 8117) is the update check's request. It POSTs
//
//     { command: "getdownloadinfo", product_id: "No1lKII9IzcBAbihub6nCg==", version: <tool version> }
//
// through OrderIdentifier (line 7765) to
// https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand, and on success writes
// the reply into ADOSettings inside a SettingsDeferScope: u_updateLink, u_updateMessage,
// u_updateChangelog, u_updateVersion, u_updateDay, u_announcement (with literal "\\n" and "\n"
// unescaped to real newlines), u_announcementLink and u_announcementLinkName; it clears
// u_announcementHidden if the announcement text changed, then hands off to SetupIdentifier.
//
// NOT PORTED. That host has been shut down and its DNS resolves to 0.0.0.0, so the request can only
// ever fail, and what it would leave behind is an error toast on every editor start. Note what is
// being dropped and what is not: unlike the licensing verification (see the audit in
// ADOverhaul.Lifecycle.cs) this request carries no machine fingerprint, no session id and no licence
// key, and spawns no subprocess -- it is a plain version ping. It is omitted because it is dead, not
// because it is invasive.
//
// FillIdentifier (line 8112) is likewise not ported: its entire body is
// `CancelIdentifier(isparam: true)`. It is the forced-check entry point, meaningless without the
// request.
//
// The hamburger menu's "Check For Update" item (line 7939) goes with them. It erased the cached
// reply out of SessionState and then called FillIdentifier; with the request gone it could only ever
// be a control that visibly does nothing, which is worse than its absence. Its shipped
// disabled-state logic is recorded here for completeness: it was added with a null callback -- so
// greyed out -- whenever `isCheckingForUpdate` or `hasCheckedForUpdate` was set.
//
// LogoutIdentifier (line 8163), the "Download Update" button's handler, and the button itself
// (lines 8037-8046) are also dropped -- see the DELIBERATE DEVIATION block below, because unlike the
// two above this one is not unambiguously dead.
//
// What IS kept is everything local. ApplyCachedUpdateInfo, both banners and the "Open Changelog" and
// "Skip for Today" and "Hide" buttons read only what a previous run cached, and a legitimate holder
// who ran the shipped build before the shutdown still has that cache in EditorPrefs. The banners are
// therefore live code, not decoration.
//
// ========================== DELIBERATE DEVIATION: the Download Update button ===================
//
// The shipped update banner draws a "Download Update" button whenever `u_updateLink` is non-blank,
// which downloads that URL to Assets/ADOverhaul.unitypackage, imports it and deletes the file. Both
// the button and its handler (LogoutIdentifier, line 8163) are omitted here.
//
// The reasoning this file was handed was that the button is dead by construction, `u_updateLink`
// being written by nothing but the removed request. That is only half right, and the correction
// matters: ADOSettings persists to EditorPrefs, which are per-user and per-machine and survive
// uninstalling the tool. A holder who ran the shipped build while the vendor was still up has a
// populated `u_updateLink` in that block, so on their machine the button WOULD draw. Omitting it is
// therefore a real, if narrow, behavioural change and is recorded as one rather than as housekeeping.
//
// It is omitted anyway, for two reasons. The URL is vendor-issued and points at infrastructure that
// went down with the rest, so following it can only produce a failed download; and the handler
// carries a shipped bug that makes that failure destructive -- its error branch calls
// AssetDatabase.ImportAsset on the half-written .unitypackage BEFORE deleting it and rethrowing, so
// a truncated file is imported into the project first. Preserving a shipped bug is this project's
// default, but not when the only reachable path through the code is the buggy one.
//
// The rest of the banner is unchanged. "Open Changelog" and "Skip for Today" are ported exactly, and
// the version/message help box still draws from the cached values.
//
// ================================= DROPPED: THE LICENCE GATES ==================================
//
// The vendor's validation backend is permanently shut down, so a licence gate can now only ever fail
// and would lock a legitimate holder out of a tool they own. Removing them is the point of this
// package. Two kinds appear in this region.
//
// First, `isLicensed` (the `_Service` field, ADOverhaul.State.cs line 5714) guards four blocks, three
// of them in ShowContextMenu: the "Send Feedback" item (decompiled line 7944), the caller-supplied
// injection point together with its two separators and the two Verify items (line 7951), and the
// extra-links block together with the Changelog item (line 7978). The fourth is outside the menu
// entirely and was not in the first pass's notes: the announcement banner's "Hide" button
// (line 8082) is written `if (isLicensed && Button("Hide", ...))`, so on an unlicensed install the
// announcement could be read but never dismissed. All four guards are dropped and their contents now
// draw unconditionally. On the shipped build an unlicensed install saw a two-entry menu and an
// undismissable announcement; here both are whole. Nothing inside those blocks does anything a
// licence check could sensibly protect -- one toggles a local bool, one hands the menu to the
// caller, one opens URLs, one writes a local dismissal flag -- so this is exactly the case the gate
// removal exists for.
//
// Second, the two items the gate existed to configure:
//
//   "Verify/On Display"       decompiled line 7959
//   "Verify/On Project Load"  decompiled line 7964
//
// a pair of mutually exclusive toggles (each sets its own setting and clears the other) over
// ADOSettings.a_VerifyOnDisplay and a_VerifyOnProjectLoad. Those two settings were deliberately left
// out of the ADOSettings port as licensing-gate remnants -- see the omission note in ADOSettings.cs,
// which records that a_VerifyOnProjectLoad was the only thing gating the startup hook that spawns
// the hardware-fingerprint subprocesses. With the settings gone the items have nothing to drive, and
// re-adding them would mean re-adding the trigger for machine fingerprinting. Both items are
// dropped. The `AddSeparator` calls around them are kept, since they separate the injection point
// from what follows rather than belonging to the toggles.
//
// ======================= SHIPPED DEAD CODE PRESERVED, NOT SILENTLY DROPPED ======================
//
// ShowContextMenu's "Documentation" (line 7971) and "Changelog" (line 8005) items are each guarded by
// `!string.IsNullOrWhiteSpace("")` -- a string literal, so the guard is constantly false and neither
// item can ever be added to the menu. This is not decompiler noise: both shipped builds have it, and
// it is the residue of a per-product documentation URL that was left blank for this product. The
// guards are ported literally, comment included, because that is what shipped and because the
// alternative -- deleting the items -- would erase the evidence that they existed. They compile (the
// argument is a literal but the call is not a constant expression, so there is no unreachable-code
// warning) and they cost one string test per menu open.
//
// `extraMenuLinks` (ADOverhaul.State.cs) ships as an empty array in both builds, so the block that
// reads it -- one entry as a plain item, several under a "Samples/" submenu -- never produces
// anything either. It is ported as written; unlike the two above it is at least data-driven and
// would work if the array were ever populated.
//
// ============================ COMPILER-GENERATED SCAFFOLDING, NOT PORTED ========================
//
// Decompiled lines 4481-4495 declare eight `public static GenericMenu.MenuFunction` fields inside a
// closure-cache class alongside a handful of Action fields. They are the cached delegate instances
// the C# compiler emits for the non-capturing lambdas ShowContextMenu passes to GenericMenu.AddItem
// -- an implementation detail of how those lambdas are allocated once rather than per call, not
// members the author wrote. The lambdas are restored inline below and the cache class is not
// reproduced.
//
// ConnectSerializer (line 7898) is a [SpecialName] accessor -- really an `updateCheckedToday`
// property, `ADOSettings.instance().u_updateDay == RemoveConfiguration()` -- and belongs with the
// update-day helper RemoveConfiguration (line 7434) in another region, alongside the two members
// that read it. It is not this file's. MapIdentifier (line 8096, the [InitializeOnLoadMethod] hook)
// is already audited and deferred in ADOverhaul.Lifecycle.cs.
//
// ============================= CORRECTIONS TO THE FIRST PASS'S NOTES ===========================
//
// Three things this file previously recorded were checked against the source and are wrong:
//
//   * The version label in DrawToolHeader uses styles.noteLeftTight, not styles.noteLeft. The
//     decompiled style is m_AuthenticationMethod (ADOEditorUtility.cs line 787), whose distinguishing
//     feature is `contentOffset = (-3, 1.5)`; that is noteLeftTight. noteLeft is m_ProcSerializer
//     (line 751), the same style without the offset.
//   * "Eleven members wait on ADOSettings" counted FillIdentifier and CancelIdentifier, which are
//     not blocked on anything -- they are dropped for being dead. The real count was nine.
//   * SelectIdentifier was listed among the scene-view overlays. It is not one: it is a generic
//     inspector foldout box and shares nothing with DrawOverlay but adjacency in the source.
//
// ================================== 2019 vs 2022 ==============================================
//
// No behavioural divergence anywhere in this region. The 2019 counterparts are ADOverhaul2019
// ADOverhaul.cs lines 7883-8069 and 8162-8271, under a different obfuscated name set (UpdateStruct,
// InsertStruct, PrepareStruct, ListStruct, ConnectStruct, CompareStruct, InterruptStruct,
// ComputeStruct, StartStruct), and they are identical statement for statement including the two dead
// IsNullOrWhiteSpace("") guards and the empty extra-links array. The only differences are the usual
// ILSpy ones: ConnectStruct emits the up-to-date test's two arms in the opposite order with the
// branch polarity inverted, and the extra-links loop spells its tuple deconstruction differently.
// No spurious `while (true)` loop appears in either build's copy of these members.
//
// Audit status: VERIFIED -- all twelve members diffed statement by statement against the 2022
// snapshot (the nine region members plus RepaintOpenWindowsDelayed / RepaintOpenWindows /
// DrawCreditLink, all carrying those names there now). Every documented removal was confirmed
// against the snapshot rather than assumed: the "Check For Update" item, the two "Verify/..."
// toggles, the four `isLicensed` guards (including the announcement banner's "Hide"), and the
// "Download Update" button with its `u_updateLink` guard are each present there and absent here, and
// the two surviving AddSeparator calls are the ones the gate left behind. The style corrections were
// re-derived: m_AuthenticationMethod is the one with contentOffset (-3, 1.5), m_ProcSerializer the
// one without. The 2019 claims were re-checked too, and one name was missing from the list above and
// has been added (ListStruct, the announcement banner). Line numbers not checked -- located by name.

using System;
using System.Globalization;
using System.Threading.Tasks;
using DreadScripts.Common;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace DreadScripts.ADOverhaul
{
    internal sealed partial class ADOverhaul
    {
        /// <summary>
        /// Draws the boxed header row every ADOverhaul surface opens with: the hamburger menu button,
        /// the update-banner toggle when an update is waiting, the tool's version, and then either
        /// caller-supplied content or the credit link. The update banner itself follows inside the
        /// same box.
        /// </summary>
        /// <param name="trailingContent">
        /// Drawn at the end of the row instead of the credit link. Callers that pass this are
        /// responsible for their own <see cref="GUILayout.FlexibleSpace"/> -- the row does not add
        /// one, so content passed here butts against the version label rather than being pushed
        /// right.
        /// </param>
        /// <param name="extraMenuItems">
        /// Handed the <see cref="GenericMenu"/> before it is shown, so each surface can add its own
        /// entries. See <see cref="ShowContextMenu"/>.
        /// </param>
        internal static void DrawToolHeader(Action trailingContent = null, Action<GenericMenu> extraMenuItems = null)
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (ADOEditorUtility.IconButton(ADOEditorUtility.contents.hamburgerMenu))
                    {
                        ShowContextMenu(extraMenuItems);
                    }

                    // The toggle is offered only while the banner is collapsed: once the banner is
                    // showing, its own "Skip for Today" is what closes it again.
                    if (!ADOSettings.instance.u_updateHidden && updateAvailable &&
                        ADOEditorUtility.IconButton(ADOEditorUtility.contents.updateAvailable))
                    {
                        updateFoldout.target = !updateFoldout.target;
                    }

                    GUILayout.Label("v" + version, ADOEditorUtility.styles.noteLeftTight, GUILayout.ExpandWidth(false));

                    if (trailingContent == null)
                    {
                        GUILayout.FlexibleSpace();
                        DrawCreditLink();
                    }
                    else
                    {
                        trailingContent();
                    }
                }

                if (updateAvailable)
                {
                    DrawUpdateBanner();
                }
            }
        }

        /// <summary>
        /// Builds and shows the hamburger menu.
        /// </summary>
        /// <param name="extraMenuItems">
        /// Invoked with the menu part-built, after "Send Feedback" and before the separators, so a
        /// surface's own entries appear as their own section. A separator is added after it, on top
        /// of the one that always follows -- two <see cref="GenericMenu.AddSeparator"/> calls in a
        /// row, which Unity collapses into one line.
        /// </param>
        /// <remarks>
        /// See this file's header for what was removed: the "Check For Update" item, whose request is
        /// dead, and the two "Verify/..." items, which drove the licence check. The two
        /// <c>IsNullOrWhiteSpace("")</c> guards below are shipped dead code and are kept deliberately.
        /// </remarks>
        internal static void ShowContextMenu(Action<GenericMenu> extraMenuItems = null)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Send Feedback"), feedbackPanelOpen, delegate
            {
                feedbackPanelOpen.Toggle();
            });

            if (extraMenuItems != null)
            {
                extraMenuItems(menu);
                menu.AddSeparator(string.Empty);
            }

            menu.AddSeparator(string.Empty);
            menu.AddSeparator(string.Empty);

            // Shipped dead code, preserved: the argument is a literal, so the guard is always false
            // and this item can never be added. It is what is left of a per-product documentation
            // URL that was never filled in for ADOverhaul. Both builds ship it.
            if (!string.IsNullOrWhiteSpace(""))
            {
                menu.AddItem(new GUIContent("Documentation"), false, delegate
                {
                    Application.OpenURL("");
                });
            }

            // Ships empty in both builds, so neither branch produces an entry. A single link is a
            // top-level item; two or more are grouped under "Samples/".
            if (extraMenuLinks.Length != 0)
            {
                if (extraMenuLinks.Length <= 1)
                {
                    menu.AddItem(new GUIContent(extraMenuLinks[0].Item1), false, delegate
                    {
                        Application.OpenURL(extraMenuLinks[0].Item2);
                    });
                }
                else
                {
                    foreach ((string label, string url) in extraMenuLinks)
                    {
                        menu.AddItem(new GUIContent("Samples/" + label), false, delegate
                        {
                            Application.OpenURL(url);
                        });
                    }
                }
            }

            // The same always-false guard as "Documentation" above.
            if (!string.IsNullOrWhiteSpace(""))
            {
                menu.AddItem(new GUIContent("Changelog"), false, delegate
                {
                    Application.OpenURL("");
                });
            }

            menu.AddItem(new GUIContent("ToS and Privacy Policy"), false, delegate
            {
                Application.OpenURL("https://dreadrith.com/license-tos");
            });

            menu.ShowAsContext();
        }

        /// <summary>
        /// Draws the update banner -- the cached version and message, a link to the changelog and a
        /// "Skip for Today" dismissal -- inside the fade group the header row's toggle drives.
        /// </summary>
        /// <param name="drawSeparator">
        /// Draws a separator above the banner. Off for callers that already have a rule there.
        /// </param>
        /// <remarks>
        /// The values shown come from <see cref="ADOSettings"/>, cached by the last successful update
        /// check; see the header for why the check itself and the download button are not ported.
        /// The early return means a dismissed banner costs nothing rather than merely animating to
        /// zero height.
        /// </remarks>
        internal static void DrawUpdateBanner(bool drawSeparator = true)
        {
            if (ADOSettings.instance.u_updateHidden)
            {
                return;
            }

            updateFoldout.FadeGroup(delegate
            {
                if (drawSeparator)
                {
                    ADOEditorUtility.Separator();
                }

                EditorGUILayout.HelpBox(
                    $"Version {ADOSettings.instance.u_updateVersion}\n--------------\n{ADOSettings.instance.u_updateMessage}",
                    MessageType.Info);

                bool hasChangelog = !string.IsNullOrWhiteSpace(ADOSettings.instance.u_updateChangelog);

                using (new GUILayout.HorizontalScope())
                {
                    // The changelog URL doubles as the button's tooltip, so it can be read before
                    // it is followed.
                    if (hasChangelog && ADOEditorUtility.Button(
                        new GUIContent("Open Changelog", ADOSettings.instance.u_updateChangelog),
                        EditorStyles.toolbarButton))
                    {
                        Application.OpenURL(ADOSettings.instance.u_updateChangelog);
                    }

                    if (ADOEditorUtility.Button("Skip for Today", EditorStyles.toolbarButton))
                    {
                        ADOSettings.instance.u_updateHidden.value = true;
                    }
                }
            }, RepaintOpenWindows);
        }

        /// <summary>
        /// Draws the announcement banner: a clickable title row, and under it the cached announcement
        /// text with its optional link and a "Hide" button.
        /// </summary>
        /// <remarks>
        /// The title row's rect is captured before the fade group so that clicking anywhere along it
        /// toggles the banner. While the group is open the captured rect is grown by 18 pixels so the
        /// click area covers the icon's overhang as well; that adjustment happens inside the fade
        /// group's body, which means it only applies on frames where the body actually drew.
        /// </remarks>
        internal static void DrawAnnouncementBanner()
        {
            if (ADOSettings.instance.u_announcementHidden || string.IsNullOrWhiteSpace(ADOSettings.instance.u_announcement))
            {
                return;
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Rect clickArea = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(24f));

                Rect titleRow = clickArea;
                GUI.Label(titleRow.SliceLeft(24f, true), ADOEditorUtility.contents.announcement);
                GUI.Label(titleRow, "Announcement", ADOEditorUtility.styles.title);

                announcementFoldout.FadeGroup(delegate
                {
                    clickArea.height += 18f;
                    ADOEditorUtility.Separator();
                    EditorGUILayout.HelpBox(ADOSettings.instance.u_announcement, MessageType.Info);

                    using (new GUILayout.HorizontalScope())
                    {
                        if (!string.IsNullOrWhiteSpace(ADOSettings.instance.u_announcementLink) &&
                            ADOEditorUtility.Button(ADOSettings.instance.u_announcementLinkName, EditorStyles.toolbarButton))
                        {
                            Application.OpenURL(ADOSettings.instance.u_announcementLink);
                        }

                        // Dismissal is stamped with the moment it happened, so
                        // ApplyCachedUpdateInfo can expire it a week later.
                        if (ADOEditorUtility.Button("Hide", EditorStyles.toolbarButton))
                        {
                            ADOSettings.instance.u_announcementHidden.value = true;
                            ADOSettings.instance.u_announcementHiddenDate.Value = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                }, RepaintOpenWindows);

                if (ADOEditorUtility.ClickArea(clickArea))
                {
                    announcementFoldout.target = !announcementFoldout.target;
                }
            }
        }

        /// <summary>
        /// Decides from the cached update information whether the update banner should be raised,
        /// and expires a stale announcement dismissal.
        /// </summary>
        /// <param name="userRequested">
        /// The user asked for this rather than it running at startup, which is what makes it report
        /// "Up to date!" and force the banner open when there is something to show.
        /// </param>
        /// <remarks>
        /// <para>
        /// Purely local: it reads only what a previous run left in <see cref="ADOSettings"/>. A
        /// dismissed announcement comes back after seven days; a dismissal timestamp that will not
        /// parse is treated as no dismissal at all, so a corrupted value fails towards showing the
        /// announcement rather than hiding it forever.
        /// </para>
        /// <para>
        /// SHIPPED ODDITY, preserved: the up-to-date branch's <paramref name="userRequested"/> path
        /// hands the "hide the banner again" step to a <see cref="Task"/> that sleeps three seconds
        /// -- so the "Up to date!" toast stays up for a moment -- and then writes a setting and
        /// requests a repaint from a worker thread. Both of those touch editor state off the main
        /// thread. It is ported as shipped; <see cref="RepaintOpenWindowsDelayed"/> queues rather than repaints,
        /// which is what keeps the consequences invisible in practice.
        /// </para>
        /// </remarks>
        internal static void ApplyCachedUpdateInfo(bool userRequested)
        {
            if (ADOSettings.instance.u_announcementHidden)
            {
                if (DateTime.TryParse(ADOSettings.instance.u_announcementHiddenDate, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal, out DateTime hiddenAt))
                {
                    ADOSettings.instance.u_announcementHidden.value = (DateTime.UtcNow - hiddenAt).TotalDays < 7.0;
                }
                else
                {
                    ADOSettings.instance.u_announcementHidden.value = false;
                }
            }

            if (!(version < new SemVer(ADOSettings.instance.u_updateVersion.Value)))
            {
                if (userRequested)
                {
                    Log("Up to date!");
                    Task.Run(async delegate
                    {
                        await Task.Delay(3000);
                        ADOSettings.instance.u_updateHidden.value = true;
                        RepaintOpenWindowsDelayed();
                    });
                }
                else
                {
                    ADOSettings.instance.u_updateHidden.value = true;
                }

                return;
            }

            updateAvailable = true;
            if (userRequested)
            {
                ADOSettings.instance.u_updateHidden.value = false;
                updateFoldout.target = true;
            }

            // Only announced in the console when the banner is actually visible, so a user who
            // dismissed it is not told again on every domain reload.
            if (!ADOSettings.instance.u_updateHidden)
            {
                Log($"Update Available! <b>(v{ADOSettings.instance.u_updateVersion})</b>");
            }
        }

        /// <summary>
        /// Draws a titled, collapsible box -- the frame most of the inspectors' sections are built
        /// out of.
        /// </summary>
        /// <param name="headerContent">Drawn to the right of the title, on the same row.</param>
        /// <param name="body">Drawn inside the fade group.</param>
        /// <remarks>
        /// The whole box is the click target, not just the title, which is why the click test comes
        /// after the header row rather than being attached to a control. When animated foldouts are
        /// turned off the <see cref="AnimBool"/>'s value is snapped to its target in the same frame,
        /// so the group opens and closes instantly while still going through the same code path.
        /// </remarks>
        internal static void DrawFoldoutBox(string title, AnimBool foldout, Action headerContent, Action body)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(title, ADOEditorUtility.styles.indentedHeaderLabel);
                    headerContent?.Invoke();
                }

                if (ADOEditorUtility.ClickArea())
                {
                    foldout.target = !foldout.target;
                    if (!ADOSettings.instance.editorAnimatedFoldouts)
                    {
                        foldout.value = foldout.target;
                    }
                }

                foldout.FadeGroup(body);
            }
        }

        /// <summary>
        /// Draws the standard scene-view overlay: a centred rich-text title flanked by an icon-width
        /// spacer on the left and the settings button on the right, with <paramref name="body"/>
        /// underneath.
        /// </summary>
        /// <remarks>
        /// The spacer exists so the title is centred against the panel rather than against the space
        /// left over by the settings button. The title's own rect is what
        /// <see cref="DrawOverlay"/> is given as the drag handle, so the panel is moved by dragging
        /// its title and not by dragging the buttons beside it.
        /// </remarks>
        internal static void DrawTitledOverlay(SceneView sceneView, string title, Action body, float width, float height)
        {
            DrawOverlay(sceneView, delegate
            {
                using (new GUILayout.HorizontalScope())
                {
                    ADOEditorUtility.IconSpacer();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(title, ADOEditorUtility.styles.centeredBoldRichLabel);
                    Rect titleRect = GUILayoutUtility.GetLastRect();
                    GUILayout.FlexibleSpace();
                    DrawSettingsButton();
                    return titleRect;
                }
            }, body, width, height);
        }

        /// <summary>
        /// Opens a scene-view panel at the corner the user last dropped it on, and lets the panel be
        /// dragged to another corner.
        /// </summary>
        /// <param name="drawHeader">
        /// Draws the panel's header row and returns the rect within it that acts as the drag handle.
        /// </param>
        /// <param name="body">Drawn under the header, separated by a rule. May be null.</param>
        /// <remarks>
        /// <para>
        /// The anchor picker is drawn after the panel has closed rather than inside it, so the
        /// nine-cell grid covers the whole scene view instead of being clipped to the panel; it needs
        /// its own <see cref="Handles.BeginGUI"/> pair for the same reason. Its result is written
        /// straight to <see cref="EnumSetting.IntValue"/>, which persists the new corner immediately
        /// -- there is no commit step, so releasing the drag over a cell is what saves it.
        /// </para>
        /// <para>
        /// Note that the picker only reports a cell on repaint events (see
        /// <see cref="ADOEditorUtility.AnchorPicker"/>), so what is stored is the cell hovered on
        /// the last repaint before the drag ended rather than the one under the cursor at
        /// mouse-up.
        /// </para>
        /// </remarks>
        internal static void DrawOverlay(SceneView sceneView, Func<Rect> drawHeader, Action body, float width, float height)
        {
            Rect sceneViewRect = sceneView.GetSceneViewRect();
            PositionFlag alignment = ADOSettings.instance.toolOverlayAlignment.GetEnumValue<PositionFlag>();

            bool isDragging;
            using (new SceneViewPanel(sceneView, width, height, alignment, sceneViewPanelResizeHandle))
            {
                Rect dragHandle = drawHeader();
                ADOEditorUtility.AddCursorRect(dragHandle, MouseCursor.Pan);
                isDragging = ADOEditorUtility.HasMouseCapture(dragHandle, tooltipDragControlId);

                if (body != null)
                {
                    ADOEditorUtility.Separator(2, 0);
                    body();
                }
            }

            if (isDragging)
            {
                Handles.BeginGUI();
                ADOSettings.instance.toolOverlayAlignment.IntValue = (int)ADOEditorUtility.AnchorPicker(alignment, sceneViewRect);
                Handles.EndGUI();
            }
        }

        /// <summary>
        /// Draws the gear button that opens the ADOverhaul window, as used in the top-right of the
        /// scene-view overlay panel.
        /// </summary>
        /// <remarks>
        /// The same three lines appear inline in the PhysBone inspector's toolbar (decompiled
        /// ADOverhaul.cs line 3529) rather than calling this; that call site belongs to an unported
        /// region and should be pointed here when it lands.
        /// </remarks>
        internal static void DrawSettingsButton()
        {
            if (ADOEditorUtility.IconButton(ADOEditorUtility.contents.settings))
            {
                ADOverhaulWindow.ShowWindow();
            }
        }

        /// <summary>
        /// Repaints every open <see cref="ADOverhaulWindow"/> on the next editor tick rather than
        /// immediately.
        /// </summary>
        /// <remarks>
        /// The deferral exists because the callers are response handlers and settings-change hooks
        /// that can run off the GUI thread, where repainting directly is not allowed.
        /// </remarks>
        private static void RepaintOpenWindowsDelayed()
        {
            ADOEditorUtility.DelayCall(RepaintOpenWindows);
        }

        /// <summary>Repaints every open <see cref="ADOverhaulWindow"/>, including hidden ones.</summary>
        /// <remarks>
        /// <c>Resources.FindObjectsOfTypeAll</c> is used rather than <c>GetWindow</c> so that no
        /// window is created as a side effect of asking whether one exists.
        /// </remarks>
        private static void RepaintOpenWindows()
        {
            ADOverhaulWindow[] windows = Resources.FindObjectsOfTypeAll<ADOverhaulWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                windows[i].Repaint();
            }
        }

        /// <summary>Draws the author credit at the foot of the window, as a link.</summary>
        /// <remarks>
        /// The label is a <see cref="GUILayout.Button(GUIContent, GUIStyle, GUILayoutOption[])"/>
        /// with its background colour cleared, so it reads as text but behaves as a link. The URL is
        /// duplicated as the tooltip so hovering shows where it goes, matching the shipped build.
        ///
        /// This is the author's personal links page, not vendor infrastructure, so unlike the
        /// update and licence endpoints it is left intact rather than dropped as dead. Whether it
        /// still resolves is not something this package should assert either way.
        /// </remarks>
        private static void DrawCreditLink()
        {
            using (new GUIColorScope(GUIColorScope.ColoringType.BG, Color.clear))
            {
                if (GUILayout.Button(
                        new GUIContent("Made By @Dreadrith ♡", "https://dreadrith.com/links"),
                        ADOEditorUtility.styles.linkNote))
                {
                    Application.OpenURL("https://dreadrith.com/links");
                }

                ADOEditorUtility.MarkAsLink();
            }
        }
    }
}
