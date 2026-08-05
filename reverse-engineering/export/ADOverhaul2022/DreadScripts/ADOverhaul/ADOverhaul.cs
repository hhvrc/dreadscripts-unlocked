using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul;

internal sealed class ADOverhaul
{
	private sealed class ADOverhaulWindow : EditorWindow
	{
		private enum EasyDynamicsFunctions
		{
			EasyGrab,
			EasyTouch,
			EasyPat
		}

		private static int m_Param;

		private static readonly string[] prototype = new string[2] { "Easy Dynamics", "Cosmetic" };

		private static readonly ADOEditorUtility.BannerDownloader banner = new ADOEditorUtility.BannerDownloader("https://raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png", addcol: true, "DreadBanner.png");

		private static EasyDynamicsFunctions selectedFunction = EasyDynamicsFunctions.EasyGrab;

		private static bool m_Issuer;

		private static bool facade;

		private static bool m_Composer;

		private static bool m_Annotation;

		private static bool editorFoldout;

		private static bool handlesFoldout;

		private static bool overlayFoldout;

		[MenuItem("DreadTools/ADOverhaul", false, 6)]
		internal static void ShowWindow()
		{
			EditorWindow.GetWindow<ADOverhaulWindow>(utility: false, "Avatar Dynamics Overhaul", focus: true);
		}

		private void OnGUI()
		{
			if (FlushConfiguration(this))
			{
				DrawSettingsGUI();
				ADOEditorUtility.Separator();
				GetConfiguration();
				DrawToolHeader();
				DrawAnnouncementBanner();
				banner.Draw(this);
			}
		}

		private void DrawEasyDynamicsGUI()
		{
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				DrawTargetAvatarSelector();
			}
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				selectedFunction = (EasyDynamicsFunctions)(object)EditorGUILayout.EnumPopup(ADOEditorUtility.CustomizeRef()._MapperSerializer, selectedFunction);
			}
			EditorGUILayout.HelpBox("Under Development", MessageType.Info);
		}

		private void DrawSettingsGUI()
		{
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				editorFoldout = EditorGUILayout.Foldout(editorFoldout, "Editor", toggleOnLabelClick: true);
				if (editorFoldout)
				{
					EditorGUI.indentLevel++;
					ADOSettings.Instance().editorAnimatedFoldouts.DrawContent(ADOEditorUtility.CustomizeRef().issuerSerializer, null);
					EditorGUI.indentLevel--;
				}
			}
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				handlesFoldout = EditorGUILayout.Foldout(handlesFoldout, "Handles", toggleOnLabelClick: true);
				if (handlesFoldout)
				{
					EditorGUI.indentLevel++;
					using (new GUILayout.HorizontalScope())
					{
						ADOSettings.Instance().onSceneNameLabels.DrawContent(ADOEditorUtility.CustomizeRef()._FacadeSerializer, null);
						if ((bool)ADOSettings.Instance().onSceneNameLabels)
						{
							ADOSettings.Instance().labelColor.DrawContent(GUIContent.none, true);
						}
					}
					ADOSettings.Instance().generalColor.DrawContent(ADOEditorUtility.CustomizeRef().annotationSerializer, true);
					ADOSettings.Instance().activeColor.DrawContent(ADOEditorUtility.CustomizeRef().m_CodeSerializer, true);
					ADOSettings.Instance().inactiveColor.DrawContent(ADOEditorUtility.CustomizeRef()._CallbackSerializer, true);
					ADOSettings.Instance().mixedColor.DrawContent(ADOEditorUtility.CustomizeRef()._MessageSerializer, true);
					ADOSettings.Instance().selectionColor.DrawContent(ADOEditorUtility.CustomizeRef().policySerializer, true);
					ADOSettings.Instance().handleSizeMultiplier.DrawFieldContent(ADOEditorUtility.CustomizeRef().m_RequestSerializer, true, null);
					EditorGUI.indentLevel--;
				}
			}
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				overlayFoldout = EditorGUILayout.Foldout(overlayFoldout, "Overlay", toggleOnLabelClick: true);
				if (!overlayFoldout)
				{
					return;
				}
				EditorGUI.indentLevel++;
				using (new GUILayout.HorizontalScope())
				{
					ADOSettings.Instance().onSceneToolSelection.DrawContent(new GUIContent("Tool Overlay", "Displays the tool selection overlay on the scene view."), null);
					using (new EditorGUI.DisabledScope(!ADOSettings.Instance().onSceneToolSelection))
					{
						ADOSettings.Instance().toolSelectionOverlayAlignment.DrawEnumPopup<ADOEditorUtility.PositionFlag>("Position", isb: false, null, Array.Empty<GUILayoutOption>());
					}
				}
				using (new GUILayout.HorizontalScope())
				{
					ADOSettings.Instance().onSceneEditingOverlay.DrawContent(ADOEditorUtility.CustomizeRef().mappingSerializer, null);
					using (new EditorGUI.DisabledScope(!ADOSettings.Instance().onSceneEditingOverlay))
					{
						ADOSettings.Instance().toolOverlayAlignment.DrawEnumPopup<ADOEditorUtility.PositionFlag>("Position", isb: false, null, Array.Empty<GUILayoutOption>());
					}
				}
				ADOSettings.Instance().onSceneTooltip.DrawContent(ADOEditorUtility.CustomizeRef().queueSerializer, null);
				EditorGUI.indentLevel--;
			}
		}

		private void OnEnable()
		{
			RefreshSceneAvatars(ref selectedAvatar, ref sceneAvatars, RefreshAvatarTables);
		}
	}

	private static class BugReporter
	{
		internal struct ErrorInfo
		{
			internal string name;

			internal ushort id;

			internal ushort version;

			internal string exceptionMessage;
		}

		private static string solution;

		private static bool solutionComplete;

		private static bool responseReceived;

		private static bool requestSent;

		private static bool isSearching;

		private static ErrorInfo? pendingError;

		private static ErrorInfo? errorContext;

		private static Action retryAction;

		private static ushort template;

		internal static bool suppressReporting;

		internal static readonly HashSet<ErrorInfo> handledErrors = new HashSet<ErrorInfo>();

		[SpecialName]
		private static float VisitMethod()
		{
			return (float)(int)template / 1f;
		}

		internal static void Run(Action ident, ushort IDcol = 0, string util = "", ushort removeSPEC2At = 0, bool deleteord3 = false, string info4 = "")
		{
			Run(ident, null, IDcol, util, removeSPEC2At, deleteord3, info4);
		}

		internal static void Run(Action task, Action selection, ushort filter_high = 0, string info2 = "", ushort no__reg3 = 0, bool requirest4 = false, string ivk5 = "")
		{
			retryAction = selection;
			if (filter_high > 0)
			{
				SetContext(filter_high, info2, no__reg3);
			}
			try
			{
				task();
			}
			catch (Exception param)
			{
				if (suppressReporting)
				{
					throw;
				}
				CaptureException(param, requirest4, ivk5);
				CompilationPipeline.compilationStarted -= OnCompilationStarted;
				CompilationPipeline.compilationStarted += OnCompilationStarted;
				throw;
			}
		}

		private static void CaptureException(Exception param, bool getcfg = false, string serv = "")
		{
			if (!errorContext.HasValue || handledErrors.Contains(errorContext.Value))
			{
				return;
			}
			solution = string.Empty;
			solutionComplete = false;
			requestSent = false;
			responseReceived = false;
			pendingError = new ErrorInfo
			{
				name = errorContext.Value.name,
				id = errorContext.Value.id,
				version = errorContext.Value.version,
				exceptionMessage = param.Message
			};
			if (getcfg)
			{
				switch (EditorUtility.DisplayDialogComplex("Error!", string.IsNullOrWhiteSpace(serv) ? "An error has occurred! Do you want to try to find a solution for it?" : serv, "Find Solution", "Close", "Ignore"))
				{
				case 2:
					handledErrors.Add(pendingError.Value);
					OnCompilationStarted(null);
					break;
				case 1:
					OnCompilationStarted(null);
					break;
				case 0:
					handledErrors.Add(pendingError.Value);
					BugReporterOpen(isi: true);
					break;
				}
			}
		}

		internal static void DrawReportPrompt()
		{
			if (!HasPendingReport())
			{
				return;
			}
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(ADOEditorUtility.CustomizeRef().reponseSerializer, ADOEditorUtility.MapRef().m_ProducerSerializer);
				GUILayout.Label("An error has occurred! Do you want to report it?", EditorStyles.boldLabel);
				if (ADOEditorUtility.PatchStatus("Ignore"))
				{
					Respond(moveinstance: false);
				}
				if (ADOEditorUtility.PatchStatus("Find Solution"))
				{
					Respond(moveinstance: true);
				}
			}
			ADOEditorUtility.Separator();
		}

		internal static bool HasPendingReport()
		{
			if (pendingError.HasValue)
			{
				if (handledErrors.Contains(pendingError.Value))
				{
					pendingError = null;
					return false;
				}
				return true;
			}
			return false;
		}

		internal static void SetContext(ushort version_item, string ivk = "", ushort idx_util = 0)
		{
			errorContext = new ErrorInfo
			{
				id = version_item,
				name = ivk,
				version = idx_util
			};
		}

		internal static void Reset()
		{
			solution = string.Empty;
			solutionComplete = false;
			suppressReporting = false;
			template = 0;
			errorContext = null;
		}

		internal static void DrawWindow()
		{
			BugReporterOpen(isLicensed && pendingError.HasValue);
			if (!requestSent)
			{
				requestSent = true;
				isSearching = true;
				List<(string, string)> list = CountConfiguration("findsolution", new(string, string)[4]
				{
					("bug_id", pendingError.Value.id.ToString()),
					("bug_version", pendingError.Value.version.ToString()),
					("bug_name", pendingError.Value.name),
					("bug_exception", Uri.EscapeUriString(pendingError.Value.exceptionMessage))
				});
				StartConfiguration(list);
				OrderIdentifier(IncludeConfiguration(list.ToArray())).HandleTask(delegate(JsonObject response)
				{
					bool flag = response.Item("success");
					string text = response.Item("message");
					responseReceived = true;
					if (string.IsNullOrWhiteSpace(text))
					{
						Log(text, (!flag) ? CustomLogType.Warning : CustomLogType.Regular);
						solution = response.Item("solution");
						solutionComplete = response.Item("complete");
					}
				}, UnityEngine.Debug.LogException, null, null, delegate
				{
					isSearching = false;
					RepaintOpenWindowsDelayed();
				});
			}
			DrawPanelHeader((!isSearching) ? "Bug Reporter" : "Finding a solution...", "If you have found a bug, please report it here!\nNote that the report is not anonymous. Abuse may result in blacklisting.");
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				if (isSearching)
				{
					if (ADOEditorUtility.LoginStatus("Cancel", EditorStyles.toolbarButton))
					{
						BugReporterOpen(isi: false);
					}
					return;
				}
				if (responseReceived)
				{
					if (string.IsNullOrWhiteSpace(solution))
					{
						using (new GUIColorScope(GUIColorScope.ColoringType.FG, ADOEditorUtility.warningColor))
						{
							GUILayout.Label("No solution Found! Please write the steps to reproduce this issue below:");
						}
						bugReportText = EditorGUILayout.TextArea(bugReportText, GUILayout.MinHeight(54f));
						if (!string.IsNullOrWhiteSpace(bugReportText) && bugReportText.Length > 2000)
						{
							bugReportText = bugReportText.Substring(0, 2000);
						}
						if (!string.IsNullOrWhiteSpace(solution))
						{
							return;
						}
						using (new GUILayout.HorizontalScope())
						{
							if (ADOEditorUtility.PatchStatus("Cancel", GUILayout.ExpandWidth(expand: false)))
							{
								BugReporterOpen(isi: false);
							}
							using (new EditorGUI.DisabledScope(isSendingBugReport))
							{
								if (!ADOEditorUtility.PatchStatus("Report Issue"))
								{
									return;
								}
								List<(string, string)> list2 = CountConfiguration("reportbug", new(string, string)[5]
								{
									("bug_id", pendingError.Value.id.ToString()),
									("bug_version", pendingError.Value.version.ToString()),
									("bug_name", pendingError.Value.name),
									("bug_exception", pendingError.Value.exceptionMessage),
									("feedback", Uri.EscapeUriString(bugReportText))
								});
								StartConfiguration(list2);
								isSendingBugReport = true;
								OrderIdentifier(IncludeConfiguration(list2.ToArray())).HandleTask(delegate(JsonObject response)
								{
									bool flag = response.Item("success");
									string text = response.Item("message");
									if (!string.IsNullOrEmpty(text))
									{
										Log(text, (!flag) ? CustomLogType.Warning : CustomLogType.Regular);
									}
								}, UnityEngine.Debug.LogException, null, null, delegate
								{
									BugReporterOpen(isi: false);
									isSendingBugReport = false;
									RepaintOpenWindowsDelayed();
								});
								return;
							}
						}
					}
					if (!solutionComplete)
					{
						using (new GUIColorScope(GUIColorScope.ColoringType.FG, ADOEditorUtility.warningColor))
						{
							GUILayout.Label("Known issue! Details:");
						}
					}
					else
					{
						using (new GUIColorScope(GUIColorScope.ColoringType.FG, ADOEditorUtility.validColor))
						{
							GUILayout.Label("Solution Found!");
						}
					}
					EditorGUILayout.Space();
					EditorGUILayout.SelectableLabel(solution, GUI.skin.label, GUILayout.ExpandHeight(expand: false));
					if (ADOEditorUtility.PatchStatus("Ok"))
					{
						BugReporterOpen(isi: false);
					}
					return;
				}
				using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
				{
					GUILayout.Label(ADOEditorUtility.CustomizeRef().reponseSerializer, ADOEditorUtility.MapRef().m_ProducerSerializer);
					using (new GUIColorScope(GUIColorScope.ColoringType.FG, ADOEditorUtility.errorColor))
					{
						GUILayout.Label("There was an issue contacting the server for a solution.");
					}
				}
				if (ADOEditorUtility.PatchStatus("Cancel"))
				{
					BugReporterOpen(isi: false);
				}
			}
		}

		internal static void Respond(bool moveinstance)
		{
			if (HasPendingReport() && pendingError.HasValue)
			{
				if (handledErrors.Contains(pendingError.Value))
				{
					pendingError = null;
				}
				BugReporterOpen(moveinstance);
				handledErrors.Add(pendingError.Value);
			}
		}

		internal static void OnCompilationStarted(object asset)
		{
			if (pendingError.HasValue && retryAction != null)
			{
				Run(retryAction, pendingError.Value.id, pendingError.Value.name, pendingError.Value.version);
			}
			retryAction = null;
			CompilationPipeline.compilationStarted -= OnCompilationStarted;
		}
	}

	private sealed class ProcessRunner
	{
		private readonly ProcessStartInfo startInfo;

		private Process process;

		private readonly Action<string> onOutput;

		private readonly Action onFailure;

		private readonly bool ignoreFailure;

		private string output;

		private bool callbackInvoked;

		internal bool isFinished;

		private bool succeeded;

		internal ProcessRunner(string i, Action<string> second, bool wantfilter = false, bool istask2 = false, Action token3 = null)
		{
			startInfo = new ProcessStartInfo((!wantfilter) ? "powershell.exe" : "cmd.exe")
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardInput = false,
				RedirectStandardOutput = true,
				Arguments = "/c " + i
			};
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
			startInfo.WorkingDirectory = folderPath;
			if (!wantfilter)
			{
				string text = Path.Combine(folderPath, "WindowsPowerShell", "v1.0");
				if (Directory.Exists(text))
				{
					startInfo.WorkingDirectory = text;
				}
			}
			onOutput = second;
			onFailure = token3;
			ignoreFailure = istask2;
		}

		internal void Run()
		{
			output = string.Empty;
			succeeded = false;
			isFinished = false;
			callbackInvoked = false;
			process = new Process();
			process.StartInfo = startInfo;
			process.Start();
			try
			{
				do
				{
					output = process.StandardOutput.ReadToEnd();
				}
				while (string.IsNullOrEmpty(output) && !process.HasExited);
				succeeded = true;
				Complete();
			}
			catch (Exception ex)
			{
				succeeded = false;
				output = "Failure! Exception: " + ex.Message + "\n" + ex.StackTrace;
				process?.Close();
				Process obj = process;
				if (obj != null)
				{
					DisposeComponent((System.ComponentModel.Component)obj);
				}
				Complete();
			}
			process.WaitForExit();
		}

		private void Complete()
		{
			if (callbackInvoked)
			{
				return;
			}
			callbackInvoked = true;
			try
			{
				string text = output.ToString();
				if (string.IsNullOrWhiteSpace(text))
				{
					text = "Missing";
				}
				if (succeeded || ignoreFailure)
				{
					onOutput(text);
				}
				else
				{
					onFailure?.Invoke();
				}
			}
			finally
			{
				isFinished = true;
			}
		}

		static void DisposeComponent(System.ComponentModel.Component component_0)
		{
			component_0.Dispose();
		}
	}

	[DefaultMember("Item")]
	internal readonly struct JsonObject
	{
		private readonly string raw;

		private readonly Dictionary<string, JsonValue> values;

		internal readonly bool isEmpty;

		internal static object LogoutTokenizer;

		internal JsonObject(string v)
		{
			raw = v;
			MatchCollection matchCollection = Regex.Matches(v, "\"(.*?)\":(?:(?:\"(.*?)\")|(?:(.*?)[,}]))");
			int count = matchCollection.Count;
			if (count != 0)
			{
				isEmpty = false;
				values = new Dictionary<string, JsonValue>();
				for (int i = 0; i < count; i++)
				{
					Match match = matchCollection[i];
					string value = match.Groups[1].Value;
					string value2 = match.Groups[2].Value;
					if (string.IsNullOrWhiteSpace(value2))
					{
						value2 = match.Groups[3].Value;
					}
					if (!string.IsNullOrEmpty(value))
					{
						values[value] = new JsonValue(value2);
					}
				}
			}
			else
			{
				isEmpty = true;
				values = null;
			}
		}

		[SpecialName]
		internal JsonValue Item(string res)
		{
			values.TryGetValue(res, out var value);
			return value;
		}

		public override string ToString()
		{
			return raw;
		}

		public string ToString(bool isvalue)
		{
			if (isvalue)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("{");
				foreach (KeyValuePair<string, JsonValue> value in values)
				{
					stringBuilder.AppendLine($"{value.Key}: {value.Value},");
				}
				stringBuilder.Append("}");
				return stringBuilder.ToString();
			}
			return ToString();
		}

		internal static bool CreateTokenizer()
		{
			return LogoutTokenizer == null;
		}
	}

	internal readonly struct JsonValue
	{
		internal readonly string raw;

		internal readonly string stringValue;

		internal readonly bool boolValue;

		internal readonly float floatValue;

		internal readonly bool hasValue;

		internal static object FlushTokenizer;

		internal JsonValue(string ident)
		{
			raw = ident;
			hasValue = true;
			if (ident.Length > 1)
			{
				if (ident.StartsWith("\"") && ident.EndsWith("\""))
				{
					stringValue = ((ident.Length != 2) ? ident.Substring(1, ident.Length - 2) : string.Empty);
				}
				else
				{
					stringValue = ident;
				}
			}
			else
			{
				stringValue = ident;
			}
			boolValue = stringValue == "true";
			float.TryParse(stringValue, out floatValue);
		}

		public override string ToString()
		{
			return stringValue;
		}

		public static implicit operator string(JsonValue setup)
		{
			return setup.stringValue;
		}

		public static implicit operator bool(JsonValue setup)
		{
			return setup.boolValue;
		}

		public static implicit operator float(JsonValue spec)
		{
			return spec.floatValue;
		}

		internal static bool OrderTokenizer()
		{
			return FlushTokenizer == null;
		}
	}

	internal enum CustomLogType
	{
		Regular,
		Warning,
		Error
	}

	[Serializable]
	private class ADOSettings
	{
		internal class SettingsChangeScope : IDisposable
		{
			private readonly Action onChanged;

			private readonly bool previousDeferred;

			private readonly EditorGUI.ChangeCheckScope changeCheck;

			[SpecialName]
			internal bool IsChanged()
			{
				return changeCheck.changed;
			}

			public SettingsChangeScope(Action reference = null)
			{
				onChanged = reference;
				previousDeferred = IsDeferred();
				SetDeferred(isv: true);
				changeCheck = new EditorGUI.ChangeCheckScope();
			}

			public void Dispose()
			{
				bool changed = changeCheck.changed;
				changeCheck.Dispose();
				if (changed)
				{
					onChanged?.Invoke();
					Save();
				}
				SetDeferred(previousDeferred);
			}

			public static implicit operator bool(SettingsChangeScope v)
			{
				return v.changeCheck.changed;
			}
		}

		internal class SettingsDeferScope : IDisposable
		{
			private readonly bool previousDeferred;

			public SettingsDeferScope()
			{
				previousDeferred = IsDeferred();
				SetDeferred(isv: true);
			}

			public void Dispose()
			{
				SetDeferred(previousDeferred);
			}
		}

		[Serializable]
		internal class BoolSetting : SettingBase
		{
			[SerializeField]
			private bool _value;

			internal readonly Action onChange;

			[SpecialName]
			internal bool GetValue()
			{
				return _value;
			}

			[SpecialName]
			internal void SetValue(bool nores)
			{
				if (_value != nores)
				{
					_value = nores;
					onChange?.Invoke();
					Save();
				}
			}

			internal BoolSetting(bool forcev, Action connection = null)
			{
				defaultValue = forcev;
				_value = forcev;
				onChange = connection;
			}

			internal void Toggle()
			{
				SetValue(!_value);
			}

			internal void Draw(string res, GUIStyle ord = null, params GUILayoutOption[] options)
			{
				DrawContent(new GUIContent(res), ord, options);
			}

			internal void DrawContent(GUIContent info, GUIStyle selection = null, params GUILayoutOption[] options)
			{
				if (selection != null)
				{
					SetValue(EditorGUILayout.Toggle(info, GetValue(), selection, options));
				}
				else
				{
					SetValue(EditorGUILayout.Toggle(info, GetValue(), options));
				}
			}

			internal void DrawButton(string spec, string token = null, bool rejectproc = false, Color? reg2 = null, Color? config3 = null, params GUILayoutOption[] options)
			{
				DrawButtonContent((!string.IsNullOrEmpty(spec)) ? new GUIContent(spec) : GUIContent.none, (!string.IsNullOrEmpty(token)) ? new GUIContent(token) : GUIContent.none, rejectproc, reg2, config3, options);
			}

			internal void DrawButtonContent(GUIContent value, GUIContent cfg = null, bool isconsumer = false, Color? cust2 = null, Color? t3 = null, params GUILayoutOption[] options)
			{
				cust2 = cust2 ?? GUI.backgroundColor;
				t3 = t3 ?? GUI.backgroundColor;
				Color backgroundColor = GUI.backgroundColor;
				GUI.backgroundColor = ((!GetValue()) ? t3.Value : cust2.Value);
				SetValue(GUILayout.Toggle(GetValue(), (!GetValue() && cfg != null) ? cfg : value, (!isconsumer) ? GUI.skin.button : EditorStyles.toolbarButton, options));
				GUI.backgroundColor = backgroundColor;
			}

			public static implicit operator bool(BoolSetting config)
			{
				return config._value;
			}

			internal override void QueryCollection()
			{
				SetValue((bool)defaultValue);
			}
		}

		[Serializable]
		internal class FloatSetting : SettingBase
		{
			[SerializeField]
			private float _value;

			internal readonly Action onChanged;

			[SpecialName]
			internal float GetValue()
			{
				return _value;
			}

			[SpecialName]
			internal void SetValue(float task)
			{
				if (_value != task)
				{
					_value = task;
					onChanged?.Invoke();
					Save();
				}
			}

			internal FloatSetting(float i, Action counter = null)
			{
				defaultValue = i;
				_value = i;
				onChanged = counter;
			}

			internal void DrawField(string reference, bool nopol = true, GUIStyle comp = null, params GUILayoutOption[] options)
			{
				DrawFieldContent(new GUIContent(reference), nopol, comp, options);
			}

			internal void DrawFieldWithLabelWidth(string info, float attr, bool moverule = true, GUIStyle setup2 = null, params GUILayoutOption[] options)
			{
				EditorGUIUtility.labelWidth = attr;
				DrawFieldContent(new GUIContent(info), moverule, setup2, options);
				EditorGUIUtility.labelWidth = 0f;
			}

			internal void DrawFieldContent(GUIContent spec, bool ispol = true, GUIStyle dir = null, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					SetValue((dir != null) ? EditorGUILayout.FloatField(spec, GetValue(), dir, options) : EditorGUILayout.FloatField(spec, GetValue(), options));
					if (ispol && GUILayout.Button(ADOEditorUtility.CustomizeRef()._ConfigSerializer, ADOEditorUtility.MapRef()._ClassSerializer, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						QueryCollection();
					}
				}
			}

			internal void DrawFieldWithLabelWidthContent(GUIContent config, float pol, bool removedic = true, GUIStyle col2 = null, params GUILayoutOption[] options)
			{
				EditorGUIUtility.labelWidth = pol;
				DrawFieldContent(config, removedic, col2, options);
				EditorGUIUtility.labelWidth = 0f;
			}

			internal void DrawSlider(string item, float map, float temp, bool requiresreference2 = true, params GUILayoutOption[] options)
			{
				DrawSliderContent(new GUIContent(item), map, temp, requiresreference2, options);
			}

			internal void DrawSliderContent(GUIContent instance, float pol, float consumer, bool forcet2 = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					SetValue(EditorGUILayout.Slider(instance, GetValue(), pol, consumer, options));
					if (forcet2 && GUILayout.Button(ADOEditorUtility.CustomizeRef()._ConfigSerializer, ADOEditorUtility.MapRef()._ClassSerializer, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						QueryCollection();
					}
				}
			}

			internal void DrawNormalizedSlider(string def, bool isb = true, params GUILayoutOption[] options)
			{
				DrawNormalizedSliderContent(new GUIContent(def), isb, options);
			}

			internal void DrawNormalizedSliderContent(GUIContent param, bool testpred = true, params GUILayoutOption[] options)
			{
				DrawSliderContent(param, 0f, 1f, testpred, options);
			}

			internal override void QueryCollection()
			{
				SetValue((float)defaultValue);
			}

			public static implicit operator int(FloatSetting asset)
			{
				return (int)asset._value;
			}

			public static implicit operator float(FloatSetting reference)
			{
				return reference._value;
			}
		}

		[Serializable]
		internal class EnumSetting : FloatSetting
		{
			[SerializeField]
			internal int IntValue
			{
				get
				{
					return (int)GetValue();
				}
				set
				{
					SetValue(value);
				}
			}

			internal EnumSetting(int endident, Action counter = null)
				: base(endident, counter)
			{
			}

			internal T GetEnumValue<T>() where T : Enum
			{
				return (T)(object)IntValue;
			}

			internal void DrawIntFieldContent(GUIContent setup, GUIStyle map = null, params GUILayoutOption[] options)
			{
				IntValue = ((map != null) ? EditorGUILayout.IntField(setup, IntValue, map, options) : EditorGUILayout.IntField(setup, IntValue, options));
			}

			internal void DrawIntField(string first, GUIStyle ivk = null, params GUILayoutOption[] options)
			{
				DrawIntFieldContent(new GUIContent(first), ivk, options);
			}

			internal void DrawEnumPopupContent<T>(GUIContent reference, bool acceptcont = false, GUIStyle dic = null, params GUILayoutOption[] options) where T : Enum
			{
				if (!acceptcont)
				{
					IntValue = ((dic != null) ? ((int)(object)EditorGUILayout.EnumPopup(reference, (T)(object)IntValue, dic, options)) : ((int)(object)EditorGUILayout.EnumPopup(reference, (T)(object)IntValue, options)));
				}
				else
				{
					IntValue = ((dic != null) ? ((int)(object)EditorGUILayout.EnumFlagsField(reference, (T)(object)IntValue, dic, options)) : ((int)(object)EditorGUILayout.EnumFlagsField(reference, (T)(object)IntValue, options)));
				}
			}

			internal void DrawEnumPopup<T>(string param, bool isb = false, GUIStyle temp = null, params GUILayoutOption[] options) where T : Enum
			{
				DrawEnumPopupContent<T>(new GUIContent(param), isb, temp, options);
			}

			internal static EnumSetting FromEnum<T>(T last, Action selection = null) where T : Enum
			{
				return new EnumSetting((int)(object)last, selection);
			}

			public static implicit operator int(EnumSetting instance)
			{
				return instance.IntValue;
			}

			public static implicit operator float(EnumSetting task)
			{
				return task.IntValue;
			}
		}

		[Serializable]
		internal class VectorSetting : SettingBase
		{
			[SerializeField]
			private float _valueX;

			[SerializeField]
			private float _valueY;

			[SerializeField]
			private float _valueZ;

			internal Action onChanged;

			internal bool isCached;

			internal Vector3 cachedValue;

			[SpecialName]
			internal Vector3 GetValue()
			{
				if (!isCached)
				{
					while (true)
					{
						isCached = true;
						cachedValue = new Vector3(_valueX, _valueY, _valueZ);
					}
				}
				return cachedValue;
			}

			[SpecialName]
			internal void SetValue(Vector3 config)
			{
				if (cachedValue != config)
				{
					cachedValue = config;
					_valueX = config.x;
					_valueY = config.y;
					_valueZ = config.z;
					onChanged?.Invoke();
					Save();
				}
			}

			internal void Initialize(Vector3 key, Action pol)
			{
				defaultValue = key;
				onChanged = pol;
				_valueX = key.x;
				_valueY = key.y;
				_valueZ = key.z;
			}

			internal VectorSetting(Vector3 info, Action caller = null)
			{
				Initialize(info, caller);
			}

			internal VectorSetting(float var1, float token, float control, Action second2 = null)
			{
				Initialize(new Vector3(var1, token, control), second2);
			}

			internal VectorSetting(float config, float second, Action comp = null)
			{
				Initialize(new Vector3(config, second), comp);
			}

			internal void DrawVector2FieldContent(GUIContent task, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label(task, GUILayout.MaxWidth(117f));
					SetValue(EditorGUILayout.Vector2Field(GUIContent.none, GetValue(), options));
					if (GUILayout.Button(ADOEditorUtility.CustomizeRef()._ConfigSerializer, ADOEditorUtility.MapRef()._ClassSerializer, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						QueryCollection();
					}
				}
			}

			internal void DrawVector2Field(string reference, params GUILayoutOption[] options)
			{
				DrawVector2FieldContent(new GUIContent(reference), options);
			}

			internal void DrawVector3FieldContent(GUIContent setup, params GUILayoutOption[] options)
			{
				SetValue(EditorGUILayout.Vector3Field(setup, GetValue(), options));
			}

			internal void DrawVector3Field(string key, params GUILayoutOption[] options)
			{
				DrawVector3FieldContent(new GUIContent(key), options);
			}

			internal override void QueryCollection()
			{
				SetValue((Vector3)defaultValue);
			}

			public static implicit operator Vector2(VectorSetting spec)
			{
				return spec.GetValue();
			}
		}

		[Serializable]
		internal class StringSetting : SettingBase
		{
			[SerializeField]
			private string _value;

			internal readonly Action onChanged;

			[SpecialName]
			internal string GetValue()
			{
				return _value;
			}

			[SpecialName]
			internal void SetValue(string ident)
			{
				if (_value != ident)
				{
					_value = ident;
					onChanged?.Invoke();
					Save();
				}
			}

			internal StringSetting(string ident = "", Action attr = null)
			{
				defaultValue = ident;
				_value = ident;
				onChanged = attr;
			}

			internal override void QueryCollection()
			{
				SetValue((string)defaultValue);
			}

			public override string ToString()
			{
				return GetValue();
			}

			public static implicit operator string(StringSetting setup)
			{
				return setup._value;
			}
		}

		[Serializable]
		internal class ColorSetting : SettingBase
		{
			internal readonly Action onChange;

			[SerializeField]
			private float r;

			[SerializeField]
			private float g;

			[SerializeField]
			private float b;

			[SerializeField]
			private float a;

			[SpecialName]
			internal Color GetValue()
			{
				return new Color(r, g, b, a);
			}

			[SpecialName]
			internal void SetValue(Color info)
			{
				r = info.r;
				g = info.g;
				b = info.b;
				a = info.a;
				onChange?.Invoke();
				Save();
			}

			internal ColorSetting(float def, float vis, float dir, float token2 = 1f, Action task3 = null)
			{
				Color color = new Color(def, vis, dir, token2);
				defaultValue = color;
				r = def;
				g = vis;
				b = dir;
				a = token2;
				onChange = task3;
			}

			internal ColorSetting(Color init, Action attr = null)
			{
				defaultValue = init;
				r = init.r;
				g = init.g;
				b = init.b;
				a = init.a;
				onChange = attr;
			}

			internal void Draw(string v, bool appendresult = true, params GUILayoutOption[] options)
			{
				DrawContent(new GUIContent(v), appendresult, options);
			}

			internal void DrawContent(GUIContent key, bool iscol = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					SetValue(EditorGUILayout.ColorField(key, GetValue(), options));
					if (iscol && GUILayout.Button(ADOEditorUtility.CustomizeRef()._ConfigSerializer, ADOEditorUtility.MapRef()._ClassSerializer, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						QueryCollection();
					}
				}
			}

			internal override void QueryCollection()
			{
				SetValue((Color)defaultValue);
			}
		}

		[Serializable]
		internal class ObjectReferenceSetting : SettingBase
		{
			internal readonly Action m_DatabaseIdentifier;

			private readonly Type _HelperIdentifier;

			[SerializeField]
			internal string guid;

			[SerializeField]
			internal long localID;

			private string m_CandidateIdentifier;

			private long m_ReaderIdentifier;

			private bool m_StubIdentifier;

			private UnityEngine.Object rulesIdentifier;

			[SpecialName]
			internal UnityEngine.Object ForgotPage()
			{
				if (!m_StubIdentifier)
				{
					m_StubIdentifier = true;
					rulesIdentifier = ConcatPage<UnityEngine.Object>(guid, localID);
				}
				return rulesIdentifier;
			}

			[SpecialName]
			internal void UpdatePage(UnityEngine.Object i)
			{
				if (rulesIdentifier != i)
				{
					rulesIdentifier = i;
					if (i == null)
					{
						guid = string.Empty;
						localID = 0L;
					}
					else
					{
						AssetDatabase.TryGetGUIDAndLocalFileIdentifier(i, out guid, out localID);
					}
					m_DatabaseIdentifier?.Invoke();
					Save();
				}
			}

			internal ObjectReferenceSetting(Type reference, string pred = "", long state_ID = 0L, Action ivk2 = null)
			{
				_HelperIdentifier = reference;
				m_CandidateIdentifier = pred;
				m_ReaderIdentifier = state_ID;
				guid = pred;
				localID = state_ID;
				m_DatabaseIdentifier = ivk2;
			}

			internal void InvokePage(string param, bool applymap = true, params GUILayoutOption[] options)
			{
				CustomizePage(new GUIContent(param), applymap, options);
			}

			internal void CustomizePage(GUIContent init, bool ispol = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					UpdatePage(EditorGUILayout.ObjectField(init, ForgotPage(), _HelperIdentifier, allowSceneObjects: false, options));
					if (ispol && GUILayout.Button(ADOEditorUtility.CustomizeRef()._ConfigSerializer, ADOEditorUtility.MapRef()._ClassSerializer, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						QueryCollection();
					}
				}
			}

			private static T ConcatPage<T>(string i, long ord_X) where T : UnityEngine.Object
			{
				if (!string.IsNullOrWhiteSpace(i))
				{
					if (ord_X != 0L)
					{
						UnityEngine.Object[] array = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(i));
						foreach (UnityEngine.Object obj in array)
						{
							AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string _, out long localId);
							if (localId == ord_X)
							{
								return (T)obj;
							}
						}
						return null;
					}
					return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(i));
				}
				return null;
			}

			internal T MapPage<T>() where T : UnityEngine.Object
			{
				return (T)ForgotPage();
			}

			internal override void QueryCollection()
			{
				UpdatePage(ConcatPage<UnityEngine.Object>(m_CandidateIdentifier, m_ReaderIdentifier));
			}

			public static implicit operator bool(ObjectReferenceSetting init)
			{
				return init.ForgotPage();
			}
		}

		internal abstract class SettingBase
		{
			internal object defaultValue;

			internal abstract void QueryCollection();
		}

		[AttributeUsage(AttributeTargets.Field)]
		internal class NonSerializedSettingAttribute : Attribute
		{
		}

		private static bool _ProxyIdentifier;

		private static bool savePending;

		private static bool deferred;

		private static FieldInfo[] nonSerializedFields;

		private static ADOSettings instance;

		internal static Action onCleared;

		[SerializeField]
		internal StringSetting u_updateLink = new StringSetting();

		[SerializeField]
		internal StringSetting u_updateVersion = new StringSetting();

		[SerializeField]
		internal StringSetting u_updateMessage = new StringSetting();

		[SerializeField]
		internal StringSetting u_updateChangelog = new StringSetting();

		[SerializeField]
		internal StringSetting u_updateDay = new StringSetting();

		[SerializeField]
		internal StringSetting u_announcement = new StringSetting();

		[SerializeField]
		internal StringSetting u_announcementLink = new StringSetting();

		[SerializeField]
		internal StringSetting u_announcementLinkName = new StringSetting();

		[SerializeField]
		internal StringSetting u_announcementHiddenDate = new StringSetting();

		[SerializeField]
		internal BoolSetting u_updateHidden = new BoolSetting(forcev: false);

		[SerializeField]
		internal BoolSetting u_announcementHidden = new BoolSetting(forcev: false);

		[SerializeField]
		internal BoolSetting a_HasSucceededLastVerification = new BoolSetting(forcev: false);

		[SerializeField]
		internal BoolSetting a_VerifyOnDisplay = new BoolSetting(forcev: true);

		[SerializeField]
		internal BoolSetting a_VerifyOnProjectLoad = new BoolSetting(forcev: false);

		[SerializeField]
		internal BoolSetting gizmosActive = new BoolSetting(forcev: true, PhysBoneEditor.ApplyGlobalGizmoSettings);

		[SerializeField]
		internal BoolSetting globalGizmo = new BoolSetting(forcev: true, PhysBoneEditor.ApplyGlobalGizmoSettings);

		[SerializeField]
		internal BoolSetting editorAnimatedFoldouts = new BoolSetting(forcev: true);

		[SerializeField]
		internal BoolSetting onSceneNameLabels = new BoolSetting(forcev: true);

		[SerializeField]
		internal BoolSetting onSceneToolSelection = new BoolSetting(forcev: true);

		[SerializeField]
		internal BoolSetting onSceneToolSelectionAlwaysVisible = new BoolSetting(forcev: true);

		[SerializeField]
		internal BoolSetting onSceneEditingOverlay = new BoolSetting(forcev: true);

		[SerializeField]
		internal BoolSetting onSceneOverlayInterceptsClick = new BoolSetting(forcev: true);

		[SerializeField]
		internal BoolSetting onSceneTooltip = new BoolSetting(forcev: true);

		[SerializeField]
		internal BoolSetting ignoreSceneClicks = new BoolSetting(forcev: true);

		[SerializeField]
		internal BoolSetting hideToolsDuringTesting = new BoolSetting(forcev: true);

		[SerializeField]
		internal BoolSetting hasReadColliderTestingWarning = new BoolSetting(forcev: false);

		[SerializeField]
		internal EnumSetting toolSelectionOverlayAlignment = EnumSetting.FromEnum(ADOEditorUtility.PositionFlag.BottomLeft);

		[SerializeField]
		internal EnumSetting toolOverlayAlignment = EnumSetting.FromEnum(ADOEditorUtility.PositionFlag.BottomRight);

		[SerializeField]
		internal FloatSetting gizmoBoneOpacity = new FloatSetting(0.5f, PhysBoneEditor.ApplyGlobalGizmoSettings);

		[SerializeField]
		internal FloatSetting gizmoLimitOpacity = new FloatSetting(0.5f, PhysBoneEditor.ApplyGlobalGizmoSettings);

		[SerializeField]
		internal FloatSetting handleSizeMultiplier = new FloatSetting(1f);

		[SerializeField]
		internal ColorSetting labelColor = new ColorSetting(1f, 1f, 1f);

		[SerializeField]
		internal ColorSetting generalColor = new ColorSetting(1f, 1f, 1f);

		[SerializeField]
		internal ColorSetting activeColor = new ColorSetting(0.56f, 0.94f, 0.47f);

		[SerializeField]
		internal ColorSetting inactiveColor = new ColorSetting(1f, 0f, 0.3765f);

		[SerializeField]
		internal ColorSetting mixedColor = new ColorSetting(1f, 0.65f, 0f);

		[SerializeField]
		internal ColorSetting selectionColor = new ColorSetting(1f, 0.65f, 0f);

		[SpecialName]
		internal static bool IsDeferred()
		{
			return deferred;
		}

		[SpecialName]
		internal static void SetDeferred(bool isv)
		{
			bool num = deferred;
			deferred = isv;
			if (num && !deferred && savePending)
			{
				Save();
			}
		}

		[SpecialName]
		internal static ADOSettings Instance()
		{
			if (instance == null)
			{
				Load();
			}
			return instance;
		}

		private ADOSettings()
		{
			nonSerializedFields = (from m in typeof(ADOSettings).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
				where m.IsDefined(typeof(NonSerializedSettingAttribute), inherit: false)
				select m).ToArray();
		}

		internal static void Save()
		{
			savePending = false;
			if (deferred)
			{
				savePending = true;
			}
			else
			{
				if (_ProxyIdentifier)
				{
					return;
				}
				StringBuilder stringBuilder = new StringBuilder("MAIN[" + JsonUtility.ToJson(Instance()) + "]\u200b\u200b\u200b");
				FieldInfo[] array = nonSerializedFields;
				foreach (FieldInfo fieldInfo in array)
				{
					try
					{
						string text = EditorJsonUtility.ToJson(fieldInfo.GetValue(Instance()));
						stringBuilder.Append(fieldInfo.Name + "[" + text + "]\u200b\u200b\u200b");
					}
					catch (Exception message)
					{
						UnityEngine.Debug.LogError(message);
					}
				}
				string value = stringBuilder.ToString();
				EditorPrefs.SetString("No1lKII9IzcBAbihub6nCg==SettingsJSON", value);
			}
		}

		private static void Load()
		{
			string text = string.Empty;
			if (EditorPrefs.HasKey("No1lKII9IzcBAbihub6nCg==SettingsJSON"))
			{
				text = EditorPrefs.GetString("No1lKII9IzcBAbihub6nCg==SettingsJSON", string.Empty);
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(text))
			{
				MatchCollection matchCollection = Regex.Matches(text, "(\\w+)\\[(.*?)\\]\\u200B\\u200B\\u200B");
				for (int i = 0; i < matchCollection.Count; i++)
				{
					Match match = matchCollection[i];
					dictionary.Add(match.Groups[1].Value, match.Groups[2].Value);
				}
			}
			if (dictionary.TryGetValue("MAIN", out var value))
			{
				instance = JsonUtility.FromJson<ADOSettings>(value);
			}
			if (instance == null)
			{
				instance = new ADOSettings();
			}
			FieldInfo[] array = nonSerializedFields;
			foreach (FieldInfo fieldInfo in array)
			{
				object obj = fieldInfo.GetValue(instance) ?? Activator.CreateInstance(fieldInfo.FieldType);
				if (dictionary.TryGetValue(fieldInfo.Name, out var value2))
				{
					EditorJsonUtility.FromJsonOverwrite(value2, obj);
				}
				fieldInfo.SetValue(instance, obj);
				if (fieldInfo.GetValue(instance) == null)
				{
					fieldInfo.SetValue(instance, Activator.CreateInstance(fieldInfo.FieldType));
				}
			}
		}

		internal static void PromptClear()
		{
			if (EditorUtility.DisplayDialog("Clearing Settings", "Are you sure you want to clear the settings?", "Clear", "Cancel"))
			{
				Clear();
			}
		}

		internal static void Clear()
		{
			instance = new ADOSettings();
			FieldInfo[] array = nonSerializedFields;
			foreach (FieldInfo fieldInfo in array)
			{
				fieldInfo.SetValue(instance, Activator.CreateInstance(fieldInfo.FieldType));
			}
			onCleared?.Invoke();
			Save();
		}

		[SpecialName]
		internal Color[] StateColors()
		{
			return new Color[3]
			{
				inactiveColor.GetValue(),
				activeColor.GetValue(),
				mixedColor.GetValue()
			};
		}
	}

	private sealed class ContactReceiverEditor : Editor
	{
		private static readonly AnimBool[] _GetterIdentifier = new AnimBool[3]
		{
			new AnimBool(value: true),
			new AnimBool(),
			new AnimBool()
		};

		private static bool _ThreadIdentifier = true;

		private ReorderableList m_AlgoIdentifier;

		private SerializedProperty m_RoleIdentifier;

		private SerializedProperty visitorIdentifier;

		private SerializedProperty invocationIdentifier;

		private SerializedProperty m_ListenerIdentifier;

		private SerializedProperty m_ParserIdentifier;

		private SerializedProperty m_PrinterIdentifier;

		private SerializedProperty m_RepositoryIdentifier;

		private SerializedProperty m_DescriptorIdentifier;

		private SerializedProperty _StrategyIdentifier;

		private SerializedProperty globalIdentifier;

		private SerializedProperty m_ManagerIdentifier;

		private SerializedProperty m_WorkerIdentifier;

		private SerializedProperty m_ItemIdentifier;

		private static Type m_IndexerIdentifier;

		private static Type poolIdentifier;

		public override void OnInspectorGUI()
		{
			if (!FlushConfiguration())
			{
				if (!isLicensed)
				{
					EnableConfiguration(PreparePage);
				}
			}
			else
			{
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
					return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
				})())
				{
					return;
				}
				base.serializedObject.Update();
				ChangePage();
				DrawFoldoutBox("Shape", _GetterIdentifier[0], null, delegate
				{
					DrawShapeProperties(AssetPage(), new SerializedProperty[6] { m_RoleIdentifier, visitorIdentifier, invocationIdentifier, m_ListenerIdentifier, m_ParserIdentifier, m_PrinterIdentifier }, RegisterPage, isres2: false);
				});
				DrawFoldoutBox("Receiver", _GetterIdentifier[1], null, delegate
				{
					DrawTargetAvatarSelector();
					EditorGUILayout.PropertyField(m_ManagerIdentifier);
					DrawAvatarParameterField(m_WorkerIdentifier);
					if (m_ManagerIdentifier.hasMultipleDifferentValues || m_ManagerIdentifier.enumValueIndex == 1)
					{
						EditorGUILayout.PropertyField(m_ItemIdentifier);
					}
					ContactReceiver contactReceiver = AssetPage() as ContactReceiver;
					if (contactReceiver != null && Application.isPlaying && !base.serializedObject.isEditingMultipleObjects && !string.IsNullOrEmpty(contactReceiver.parameter))
					{
						EditorGUI.indentLevel++;
						using (new EditorGUI.DisabledScope(disabled: true))
						{
							EditorGUILayout.FloatField(contactReceiver.parameter, contactReceiver.paramValue);
						}
						EditorGUI.indentLevel--;
					}
				});
				DrawFoldoutBox("Filtering", _GetterIdentifier[2], null, delegate
				{
					DrawTargetAvatarSelector();
					using (new GUILayout.HorizontalScope())
					{
						RegisterConfiguration(m_DescriptorIdentifier, m_DescriptorIdentifier.GetContent(), null);
						RegisterConfiguration(_StrategyIdentifier, _StrategyIdentifier.GetContent(), null);
						RegisterConfiguration(globalIdentifier, globalIdentifier.GetContent(), null);
					}
					m_AlgoIdentifier.DoLayoutList();
				});
				base.serializedObject.ApplyModifiedProperties();
				GetConfiguration();
				DrawToolHeader();
			}
		}

		private void method_0()
		{
			DrawShapeHandles(AssetPage(), base.targets, 2, Color.cyan);
		}

		private void CallPage(Rect setup, int version_attr, bool isserv, bool isvis2)
		{
			DrawCollisionTagElement(m_RepositoryIdentifier, setup, version_attr);
		}

		private void RegisterPage()
		{
			base.serializedObject.ApplyModifiedProperties();
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			UnityEngine.Object[] array = base.targets;
			for (int i = 0; i < array.Length; i++)
			{
				VRCContactReceiver vRCContactReceiver = (VRCContactReceiver)array[i];
				if (flag3 && flag2 && flag)
				{
					break;
				}
				switch ((int)vRCContactReceiver.shapeType)
				{
				default:
					flag = true;
					break;
				case 0:
					flag3 = true;
					break;
				case 1:
					flag = true;
					flag2 = true;
					flag3 = true;
					break;
				}
			}
			SetShapeCapabilities(flag3, flag2, flag);
		}

		private void ChangePage()
		{
			visitorIdentifier = base.serializedObject.FindProperty("rootTransform");
			m_RoleIdentifier = base.serializedObject.FindProperty("shapeType");
			invocationIdentifier = base.serializedObject.FindProperty("radius");
			m_ListenerIdentifier = base.serializedObject.FindProperty("height");
			m_ParserIdentifier = base.serializedObject.FindProperty("position");
			m_PrinterIdentifier = base.serializedObject.FindProperty("rotation");
			m_RepositoryIdentifier = base.serializedObject.FindProperty("collisionTags");
			m_DescriptorIdentifier = base.serializedObject.FindProperty("allowSelf");
			_StrategyIdentifier = base.serializedObject.FindProperty("allowOthers");
			globalIdentifier = base.serializedObject.FindProperty("localOnly");
			m_ManagerIdentifier = base.serializedObject.FindProperty("receiverType");
			m_WorkerIdentifier = base.serializedObject.FindProperty("parameter");
			m_ItemIdentifier = base.serializedObject.FindProperty("minVelocity");
			m_AlgoIdentifier = new ReorderableList(base.serializedObject, m_RepositoryIdentifier, draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
			{
				drawElementCallback = CallPage,
				drawHeaderCallback = DrawCollisionTagsHeader
			};
		}

		private void OnEnable()
		{
			ResetFoldouts(_GetterIdentifier, Repaint);
			MapConfiguration(RegisterPage);
		}

		private void OnDisable()
		{
			SetShapeEditOverlayActive(isvar1: false);
		}

		[MenuItem("CONTEXT/VRCContactReceiver/ADOverhaul/To Sender", false, 897)]
		private static void StopPage(MenuCommand config)
		{
			if (MoveConfiguration())
			{
				VRCContactReceiver obj = (VRCContactReceiver)config.context;
				obj.CompareVal(obj.gameObject);
				Undo.DestroyObjectImmediate(obj);
			}
		}

		[MenuItem("CONTEXT/VRCContactReceiver/ADOverhaul/To Collider", false, 898)]
		private static void PushPage(MenuCommand key)
		{
			if (MoveConfiguration())
			{
				VRCContactReceiver obj = (VRCContactReceiver)key.context;
				obj.InvokeVal(obj.gameObject);
				Undo.DestroyObjectImmediate(obj);
			}
		}

		[MenuItem("CONTEXT/VRCContactReceiver/ADOverhaul/Toggle Editor", false, 899)]
		private static void PreparePage()
		{
			ReadPage(_ThreadIdentifier);
		}

		internal static void ReadPage(bool insertasset = false)
		{
			if (m_IndexerIdentifier == null)
			{
				m_IndexerIdentifier = ADOEditorUtility.FindType("VRCContactReceiver");
			}
			if (poolIdentifier == null)
			{
				poolIdentifier = ADOEditorUtility.FindType("VRCContactReceiverEditor");
			}
			_ThreadIdentifier = !insertasset;
			ADOEditorUtility.OverrideCustomEditor(m_IndexerIdentifier, (!_ThreadIdentifier) ? poolIdentifier : typeof(ContactReceiverEditor));
		}

		[CompilerGenerated]
		private void TestPage()
		{
			DrawShapeProperties(AssetPage(), new SerializedProperty[6] { m_RoleIdentifier, visitorIdentifier, invocationIdentifier, m_ListenerIdentifier, m_ParserIdentifier, m_PrinterIdentifier }, RegisterPage, isres2: false);
		}

		[CompilerGenerated]
		private void InsertPage()
		{
			DrawTargetAvatarSelector();
			EditorGUILayout.PropertyField(m_ManagerIdentifier);
			DrawAvatarParameterField(m_WorkerIdentifier);
			if (m_ManagerIdentifier.hasMultipleDifferentValues || m_ManagerIdentifier.enumValueIndex == 1)
			{
				EditorGUILayout.PropertyField(m_ItemIdentifier);
			}
			ContactReceiver contactReceiver = AssetPage() as ContactReceiver;
			if (contactReceiver != null && Application.isPlaying && !base.serializedObject.isEditingMultipleObjects && !string.IsNullOrEmpty(contactReceiver.parameter))
			{
				EditorGUI.indentLevel++;
				using (new EditorGUI.DisabledScope(disabled: true))
				{
					EditorGUILayout.FloatField(contactReceiver.parameter, contactReceiver.paramValue);
				}
				EditorGUI.indentLevel--;
			}
		}

		[CompilerGenerated]
		private void EnablePage()
		{
			DrawTargetAvatarSelector();
			using (new GUILayout.HorizontalScope())
			{
				RegisterConfiguration(m_DescriptorIdentifier, m_DescriptorIdentifier.GetContent(), null);
				RegisterConfiguration(_StrategyIdentifier, _StrategyIdentifier.GetContent(), null);
				RegisterConfiguration(globalIdentifier, globalIdentifier.GetContent(), null);
			}
			m_AlgoIdentifier.DoLayoutList();
		}

		UnityEngine.Object AssetPage()
		{
			return base.target;
		}
	}

	private sealed class ContactSenderEditor : Editor
	{
		private static readonly AnimBool[] _StructIdentifier = new AnimBool[2]
		{
			new AnimBool(value: true),
			new AnimBool()
		};

		private static bool _InterpreterIdentifier = true;

		private ReorderableList _ParameterIdentifier;

		private SerializedProperty attrIdentifier;

		private SerializedProperty objectIdentifier;

		private SerializedProperty m_ServiceIdentifier;

		private SerializedProperty _ReponseIdentifier;

		private SerializedProperty specificationIdentifier;

		private SerializedProperty _WrapperIdentifier;

		private SerializedProperty m_InfoIdentifier;

		private static Type m_ModelIdentifier;

		private static Type m_ConfigIdentifier;

		public override void OnInspectorGUI()
		{
			if (!FlushConfiguration())
			{
				if (!isLicensed)
				{
					EnableConfiguration(SortProperty);
				}
			}
			else
			{
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
					return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
				})())
				{
					return;
				}
				base.serializedObject.Update();
				CompareProperty();
				DrawFoldoutBox("Shape", _StructIdentifier[0], null, delegate
				{
					DrawShapeProperties(LogoutProperty(), new SerializedProperty[6] { attrIdentifier, objectIdentifier, m_ServiceIdentifier, _ReponseIdentifier, specificationIdentifier, _WrapperIdentifier }, NewProperty, isres2: false);
				});
				DrawFoldoutBox("Filtering", _StructIdentifier[1], null, delegate
				{
					DrawTargetAvatarSelector();
					using (new GUILayout.VerticalScope())
					{
						_ParameterIdentifier.DoLayoutList();
					}
				});
				base.serializedObject.ApplyModifiedProperties();
				GetConfiguration();
				DrawToolHeader();
			}
		}

		private void method_0()
		{
			DrawShapeHandles(LogoutProperty(), base.targets, 1, Color.yellow);
		}

		private void DestroyProperty(Rect config, int length_b, bool requiresc, bool isvis2)
		{
			DrawCollisionTagElement(m_InfoIdentifier, config, length_b);
		}

		private void NewProperty()
		{
			base.serializedObject.ApplyModifiedProperties();
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			UnityEngine.Object[] array = base.targets;
			for (int i = 0; i < array.Length; i++)
			{
				VRCContactSender vRCContactSender = (VRCContactSender)array[i];
				if (flag3 && flag2 && flag)
				{
					break;
				}
				switch ((int)vRCContactSender.shapeType)
				{
				case 1:
					flag = true;
					flag2 = true;
					flag3 = true;
					break;
				case 0:
					flag3 = true;
					break;
				default:
					flag = true;
					break;
				}
			}
			SetShapeCapabilities(flag3, flag2, flag);
		}

		private void CompareProperty()
		{
			objectIdentifier = base.serializedObject.FindProperty("rootTransform");
			attrIdentifier = base.serializedObject.FindProperty("shapeType");
			m_ServiceIdentifier = base.serializedObject.FindProperty("radius");
			_ReponseIdentifier = base.serializedObject.FindProperty("height");
			specificationIdentifier = base.serializedObject.FindProperty("position");
			_WrapperIdentifier = base.serializedObject.FindProperty("rotation");
			m_InfoIdentifier = base.serializedObject.FindProperty("collisionTags");
			_ParameterIdentifier = new ReorderableList(base.serializedObject, m_InfoIdentifier, draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
			{
				drawElementCallback = DestroyProperty,
				drawHeaderCallback = DrawCollisionTagsHeader
			};
		}

		private void OnEnable()
		{
			ResetFoldouts(_StructIdentifier, Repaint);
			MapConfiguration(NewProperty);
		}

		private void OnDisable()
		{
			SetShapeEditOverlayActive(isvar1: false);
		}

		[MenuItem("CONTEXT/VRCContactSender/ADOverhaul/To Receiver", false, 897)]
		private static void VerifyProperty(MenuCommand var1)
		{
			if (MoveConfiguration())
			{
				VRCContactSender obj = (VRCContactSender)var1.context;
				obj.SetVal(obj.gameObject);
				Undo.DestroyObjectImmediate(obj);
			}
		}

		[MenuItem("CONTEXT/VRCContactSender/ADOverhaul/To Collider", false, 898)]
		private static void SetProperty(MenuCommand def)
		{
			if (MoveConfiguration())
			{
				VRCContactSender obj = (VRCContactSender)def.context;
				obj.CustomizeVal(obj.gameObject);
				Undo.DestroyObjectImmediate(obj);
			}
		}

		[MenuItem("CONTEXT/VRCContactSender/ADOverhaul/Toggle Editor", false, 899)]
		private static void SortProperty()
		{
			InvokeProperty(_InterpreterIdentifier);
		}

		internal static void InvokeProperty(bool isinstance = false)
		{
			if (m_ModelIdentifier == null)
			{
				m_ModelIdentifier = ADOEditorUtility.FindType("VRCContactSender");
			}
			if (m_ConfigIdentifier == null)
			{
				m_ConfigIdentifier = ADOEditorUtility.FindType("VRCContactSenderEditor");
			}
			_InterpreterIdentifier = !isinstance;
			ADOEditorUtility.OverrideCustomEditor(m_ModelIdentifier, (!_InterpreterIdentifier) ? m_ConfigIdentifier : typeof(ContactSenderEditor));
		}

		[CompilerGenerated]
		private void CustomizeProperty()
		{
			DrawShapeProperties(LogoutProperty(), new SerializedProperty[6] { attrIdentifier, objectIdentifier, m_ServiceIdentifier, _ReponseIdentifier, specificationIdentifier, _WrapperIdentifier }, NewProperty, isres2: false);
		}

		[CompilerGenerated]
		private void ConcatProperty()
		{
			DrawTargetAvatarSelector();
			using (new GUILayout.VerticalScope())
			{
				_ParameterIdentifier.DoLayoutList();
			}
		}

		UnityEngine.Object LogoutProperty()
		{
			return base.target;
		}
	}

	private sealed class PhysBoneColliderEditor : Editor
	{
		private static readonly AnimBool[] shapeFoldout = new AnimBool[1]
		{
			new AnimBool(value: true)
		};

		private static bool editorOverrideEnabled = true;

		private SerializedProperty rootTransform;

		private SerializedProperty shapeType;

		private SerializedProperty insideBounds;

		private SerializedProperty bonesAsSpheres;

		private SerializedProperty radius;

		private SerializedProperty height;

		private SerializedProperty position;

		private SerializedProperty rotation;

		private static Type m_BaseIdentifier;

		private static Type _RequestIdentifier;

		public override void OnInspectorGUI()
		{
			if (FlushConfiguration())
			{
				base.serializedObject.Update();
				CacheProperties();
				if (((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
					return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
				})())
				{
					if (ReadConfiguration(base.targets))
					{
						InsertConfiguration();
					}
					DrawFoldoutBox("Shape", shapeFoldout[0], null, delegate
					{
						DrawShapeProperties(RestartProperty(), new SerializedProperty[8] { shapeType, rootTransform, radius, height, position, rotation, insideBounds, bonesAsSpheres }, EnableProperty, isres2: true);
					});
					if (TestConfiguration(base.serializedObject, base.targets))
					{
						SceneView.RepaintAll();
						colliderChangedDuringTest = true;
					}
					GetConfiguration();
					DrawToolHeader();
					DrawAnnouncementBanner();
				}
			}
			else if (!isLicensed)
			{
				EnableConfiguration(TestProperty);
			}
		}

		public void method_0()
		{
			DrawShapeHandles(RestartProperty(), base.targets, 0, Color.green);
		}

		[MenuItem("CONTEXT/VRCPhysBoneCollider/ADOverhaul/Move To Empty", false, 896)]
		private static void PushProperty(MenuCommand config)
		{
			if (MoveConfiguration())
			{
				UnityEngine.Component component = config.context as UnityEngine.Component;
				ComponentUtility.CopyComponent(component);
				GameObject gameObject = new GameObject(component.gameObject.name + " Collider");
				Undo.RegisterCreatedObjectUndo(gameObject, "Move Colliders To Empty");
				Transform transform = component.transform;
				gameObject.transform.parent = transform.parent;
				gameObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
				gameObject.transform.localScale = transform.localScale;
				ComponentUtility.PasteComponentAsNew(gameObject);
				Undo.DestroyObjectImmediate(component);
			}
		}

		[MenuItem("CONTEXT/VRCPhysBoneCollider/ADOverhaul/To Sender", false, 897)]
		private static void PrepareProperty(MenuCommand i)
		{
			if (MoveConfiguration())
			{
				VRCPhysBoneCollider obj = (VRCPhysBoneCollider)i.context;
				obj.VerifyVal(obj.gameObject);
				Undo.DestroyObjectImmediate(obj);
			}
		}

		[MenuItem("CONTEXT/VRCPhysBoneCollider/ADOverhaul/To Receiver", false, 898)]
		private static void ReadProperty(MenuCommand spec)
		{
			if (MoveConfiguration())
			{
				VRCPhysBoneCollider obj = (VRCPhysBoneCollider)spec.context;
				obj.SortVal(obj.gameObject);
				Undo.DestroyObjectImmediate(obj);
			}
		}

		[MenuItem("CONTEXT/VRCPhysBoneCollider/ADOverhaul/Toggle Editor", false, 899)]
		private static void TestProperty()
		{
			InsertProperty(editorOverrideEnabled);
		}

		internal static void InsertProperty(bool isi = false)
		{
			if (m_BaseIdentifier == null)
			{
				m_BaseIdentifier = ADOEditorUtility.FindType("VRCPhysBoneCollider");
			}
			if (_RequestIdentifier == null)
			{
				_RequestIdentifier = ADOEditorUtility.FindType("VRCPhysBoneColliderEditor");
			}
			editorOverrideEnabled = !isi;
			ADOEditorUtility.OverrideCustomEditor(m_BaseIdentifier, (!editorOverrideEnabled) ? _RequestIdentifier : typeof(PhysBoneColliderEditor));
		}

		private void EnableProperty()
		{
			base.serializedObject.ApplyModifiedProperties();
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			UnityEngine.Object[] array = base.targets;
			for (int i = 0; i < array.Length; i++)
			{
				VRCPhysBoneCollider vRCPhysBoneCollider = (VRCPhysBoneCollider)array[i];
				if (flag3 && flag2 && flag)
				{
					break;
				}
				switch ((int)vRCPhysBoneCollider.shapeType)
				{
				default:
					flag = true;
					break;
				case 1:
					flag = true;
					flag2 = true;
					flag3 = true;
					break;
				case 0:
					flag3 = true;
					break;
				}
			}
			SetShapeCapabilities(flag3, flag2, flag);
		}

		private void CacheProperties()
		{
			rootTransform = base.serializedObject.FindProperty("rootTransform");
			shapeType = base.serializedObject.FindProperty("shapeType");
			insideBounds = base.serializedObject.FindProperty("insideBounds");
			bonesAsSpheres = base.serializedObject.FindProperty("bonesAsSpheres");
			radius = base.serializedObject.FindProperty("radius");
			height = base.serializedObject.FindProperty("height");
			position = base.serializedObject.FindProperty("position");
			rotation = base.serializedObject.FindProperty("rotation");
		}

		private void OnEnable()
		{
			ResetFoldouts(shapeFoldout, Repaint);
			MapConfiguration(EnableProperty);
		}

		public void OnDisable()
		{
			SetShapeEditOverlayActive(isvar1: false);
		}

		[CompilerGenerated]
		private void DisableProperty()
		{
			DrawShapeProperties(RestartProperty(), new SerializedProperty[8] { shapeType, rootTransform, radius, height, position, rotation, insideBounds, bonesAsSpheres }, EnableProperty, isres2: true);
		}

		UnityEngine.Object RestartProperty()
		{
			return base.target;
		}
	}

	private sealed class PhysBoneEditor : Editor
	{
		internal class AlgoAuthentication
		{
			internal readonly string roleAuthentication;

			internal readonly SerializedProperty m_VisitorAuthentication;

			internal readonly SerializedProperty _InvocationAuthentication;

			internal readonly string m_ListenerAuthentication;

			internal readonly string m_ParserAuthentication;

			internal readonly bool printerAuthentication;

			internal readonly float repositoryAuthentication;

			internal readonly float _DescriptorAuthentication;

			internal readonly int m_StrategyAuthentication;

			internal readonly bool _GlobalAuthentication;

			internal AlgoAuthentication(SerializedProperty res, SerializedProperty token, float temp = 0f, float token2 = 1f, int info3 = 0)
				: this(res?.displayName, res, token, temp, token2, info3)
			{
			}

			internal AlgoAuthentication(string init, SerializedProperty cfg, SerializedProperty comp, float visitor2 = 0f, float attr3 = 1f, int def4_length = 0)
			{
				roleAuthentication = init;
				m_VisitorAuthentication = cfg;
				_InvocationAuthentication = comp;
				_GlobalAuthentication = cfg != null;
				m_ListenerAuthentication = (_GlobalAuthentication ? cfg.propertyPath : string.Empty);
				printerAuthentication = comp != null;
				m_ParserAuthentication = (printerAuthentication ? comp.propertyPath : string.Empty);
				repositoryAuthentication = visitor2;
				_DescriptorAuthentication = attr3;
				m_StrategyAuthentication = def4_length;
			}
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec m_ManagerAuthentication = new _003C_003Ec();

			public static Func<bool> workerAuthentication;

			public static Action m_ItemAuthentication;

			public static Action m_IndexerAuthentication;

			public static Action poolAuthentication;

			public static Action m_SystemAuthentication;

			public static Action m_SetterAuthentication;

			public static Func<string, string> ruleAuthentication;

			public static Func<ADOEditorUtility.PhysBoneParameter, bool> structAuthentication;

			public static Action _InterpreterAuthentication;

			public static Action _ParameterAuthentication;

			public static Action m_AttrAuthentication;

			public static Action<VRCPhysBone> objectAuthentication;

			public static Func<ADOEditorUtility.BoneNode, bool> serviceAuthentication;

			public static Func<Keyframe, float> _ReponseAuthentication;

			public static Func<ADOEditorUtility.PhysBoneParameter, bool> _SpecificationAuthentication;

			public static Func<VRCPhysBone, IEnumerable<Transform>> m_WrapperAuthentication;

			internal bool DeleteParams()
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
				return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
			}

			internal void DefineParams()
			{
				bool flag = _ErrorIdentifier.enumValueIndex == 1;
				PostSingleton(0);
				PostSingleton(1, new GUIContent((!flag) ? "Spring" : "Momentum", m_SchemaIdentifier.tooltip));
				if (flag)
				{
					PostSingleton(2);
				}
				PostSingleton(3);
				PostSingleton(4);
				PostSingleton(5);
				if (configurationAuthentication != null)
				{
					EditorGUILayout.PropertyField(configurationAuthentication);
				}
			}

			internal void DestroyParams()
			{
				int enumValueIndex = _AccountAuthentication.enumValueIndex;
				EditorGUILayout.PropertyField(_AccountAuthentication, new GUIContent("Type"));
				if (enumValueIndex <= 0)
				{
					return;
				}
				PostSingleton(7);
				if (enumValueIndex == 3)
				{
					PostSingleton(8);
				}
				EditorGUILayout.PropertyField(regAuthentication);
				using (new GUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField("Limit Rotation Curves");
					UpdateSingleton(PhysBoneEditor.adapterAuthentication, "X", isproc: false);
					UpdateSingleton(_ProxyAuthentication, "Y", isproc: false);
					UpdateSingleton(m_RefAuthentication, "Z", isproc: false);
					if (ADOEditorUtility.CallStatus(ADOEditorUtility.CustomizeRef().baseSerializer, GUI.skin.label, GUILayout.Width(14f)))
					{
						SerializedProperty adapterAuthentication = PhysBoneEditor.adapterAuthentication;
						SerializedProperty proxyAuthentication = _ProxyAuthentication;
						AnimationCurve animationCurve = (m_RefAuthentication.animationCurveValue = new AnimationCurve());
						AnimationCurve animationCurveValue = (proxyAuthentication.animationCurveValue = animationCurve);
						adapterAuthentication.animationCurveValue = animationCurveValue;
					}
				}
			}

			internal void NewParams()
			{
				CallConfiguration(consumerAuthentication, "Allow Collsion", null, GUILayout.ExpandWidth(expand: false));
			}

			internal void CompareParams()
			{
				CallConfiguration(comparatorAuthentication, "Allow Grabbing", null, GUILayout.ExpandWidth(expand: false));
				CallConfiguration(iteratorAuthentication, "Allow Posing", null, GUILayout.ExpandWidth(expand: false));
			}

			internal void VerifyParams()
			{
				ListSingleton(13);
				ListSingleton(14);
				if (_ValueIdentifier.enumValueIndex > 0)
				{
					ListSingleton(12);
				}
			}

			internal string SetParams(string s)
			{
				return s.Substring(0, s.LastIndexOf('_'));
			}

			internal bool SortParams(ADOEditorUtility.PhysBoneParameter pbp2)
			{
				return pbp2.hasBackingField;
			}

			internal void InvokeParams()
			{
				CallConfiguration(m_FactoryAuthentication, "Show Gizmos", delegate
				{
					if ((bool)ADOSettings.Instance().globalGizmo)
					{
						ADOSettings.Instance().gizmosActive.SetValue(m_FactoryAuthentication.boolValue);
					}
				}, GUILayout.ExpandWidth(expand: false));
				bool flag;
				string text = ((!(flag = ADOSettings.Instance().globalGizmo)) ? "Local Setting" : "Global Setting");
				using (new GUIColorScope(GUIColorScope.ColoringType.BG, flag, ADOEditorUtility.validColor, ADOEditorUtility.warningColor))
				{
					using (new ADOSettings.SettingsChangeScope(ApplyGlobalGizmoSettings))
					{
						ADOSettings.Instance().globalGizmo.SetValue(GUILayout.Toggle(flag, text, GUI.skin.button, GUILayout.ExpandWidth(expand: false)));
					}
				}
			}

			internal void CustomizeParams()
			{
				if ((bool)ADOSettings.Instance().globalGizmo)
				{
					ADOSettings.Instance().gizmosActive.SetValue(m_FactoryAuthentication.boolValue);
				}
			}

			internal void ConcatParams()
			{
				if ((bool)ADOSettings.Instance().globalGizmo)
				{
					ADOSettings.Instance().gizmoBoneOpacity.SetValue(EditorGUILayout.Slider("Bone Opacity", ADOSettings.Instance().gizmoBoneOpacity, 0f, 1f));
					ADOSettings.Instance().gizmoLimitOpacity.SetValue(EditorGUILayout.Slider("Limit Opacitiy", ADOSettings.Instance().gizmoLimitOpacity, 0f, 1f));
				}
				else
				{
					m_AttributeAuthentication.floatValue = EditorGUILayout.Slider("Bone Opacity", m_AttributeAuthentication.floatValue, 0f, 1f);
					m_InstanceAuthentication.floatValue = EditorGUILayout.Slider("Limit Opacitiy", m_InstanceAuthentication.floatValue, 0f, 1f);
				}
			}

			internal void MapParams(VRCPhysBone pb)
			{
				pb.configHasUpdated = true;
			}

			internal bool FillParams(ADOEditorUtility.BoneNode b)
			{
				if (!b.isEndBone)
				{
					return false;
				}
				return !b.isVirtual;
			}

			internal float CancelParams(Keyframe k)
			{
				return k.value;
			}

			internal bool LogoutParams(ADOEditorUtility.PhysBoneParameter p)
			{
				return p.hasBackingField;
			}

			internal IEnumerable<Transform> SetupParams(VRCPhysBone pb)
			{
				return pb.GetRootTransform().GetComponentsInChildren<Transform>();
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass108_0
		{
			public PhysBoneEditor m_InfoAuthentication;

			public bool _ModelAuthentication;

			internal void InstantiateParams()
			{
				using (new GUILayout.HorizontalScope())
				{
					EditorGUILayout.PropertyField(producerIdentifier, new GUIContent("Root"));
					if (GUILayout.Button(new GUIContent("S", "Set to Self"), GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						UnityEngine.Object[] targets = m_InfoAuthentication.targets;
						for (int i = 0; i < targets.Length; i++)
						{
							VRCPhysBone vRCPhysBone = targets[i] as VRCPhysBone;
							if ((bool)vRCPhysBone)
							{
								SerializedObject serializedObject = new SerializedObject(vRCPhysBone);
								serializedObject.FindProperty("rootTransform").objectReferenceValue = vRCPhysBone.transform;
								serializedObject.ApplyModifiedProperties();
							}
						}
					}
				}
				ResetAccount(DrawPropertyWithEditToggle(_WriterIdentifier, isEditingEndpoints()));
				EditorGUILayout.PropertyField(classIdentifier);
				using (new GUILayout.VerticalScope("box"))
				{
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.Space(12f);
						m_TemplateIdentifier.isExpanded = EditorGUILayout.Foldout(m_TemplateIdentifier.isExpanded, "Ignore Transforms", toggleOnLabelClick: true);
						GUILayout.FlexibleSpace();
						FindAccount(DrawIconToggle(isCopyingIgnoreTransforms(), ADOEditorUtility.CustomizeRef().m_ExporterSerializer));
						EditorGUI.BeginChangeCheck();
						ExcludeAccount(DrawIconToggle(isSelectingIgnoreTransforms(), ADOEditorUtility.CustomizeRef().advisorSerializer));
						if (EditorGUI.EndChangeCheck())
						{
							RefreshIgnoreTransformStates();
						}
					}
					if (m_TemplateIdentifier.isExpanded)
					{
						EditorGUI.indentLevel++;
						ADOEditorUtility.ObjectListField<Transform>(m_TemplateIdentifier);
						EditorGUI.indentLevel--;
					}
				}
			}

			internal void RestartParams()
			{
				PostSingleton(6);
				if (_ModelAuthentication)
				{
					DrawSelfOthersToggles(consumerAuthentication, m_UtilsAuthentication);
				}
				using (new GUILayout.VerticalScope(GUI.skin.box))
				{
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.Space(12f);
						m_SingletonAuthentication.isExpanded = EditorGUILayout.Foldout(m_SingletonAuthentication.isExpanded, "Colliders", toggleOnLabelClick: true);
						GUILayout.FlexibleSpace();
						RunParams(DrawIconToggle(isCopyingColliders(), ADOEditorUtility.CustomizeRef().m_ExporterSerializer));
						EditorGUI.BeginChangeCheck();
						CreateAccount(DrawIconToggle(isSelectingColliders(), ADOEditorUtility.CustomizeRef().advisorSerializer));
						if (EditorGUI.EndChangeCheck())
						{
							RefreshColliderStates();
						}
					}
					if (m_SingletonAuthentication.isExpanded)
					{
						EditorGUI.indentLevel++;
						ADOEditorUtility.ObjectListField<VRCPhysBoneCollider>(m_SingletonAuthentication);
						EditorGUI.indentLevel--;
					}
				}
			}

			internal void ManageParams()
			{
				if (_ModelAuthentication)
				{
					while (true)
					{
						DrawSelfOthersToggles(comparatorAuthentication, _ProductAuthentication);
						DrawSelfOthersToggles(iteratorAuthentication, m_PredicateAuthentication);
					}
				}
				EditorGUILayout.PropertyField(_CollectionAuthentication);
				EditorGUILayout.PropertyField(interceptorAuthentication);
			}

			internal void RateParams()
			{
				EditorGUILayout.PropertyField(_ValueIdentifier);
				EditorGUILayout.PropertyField(resolverAuthentication);
				EditorGUILayout.PropertyField(_FilterAuthentication);
				DrawTargetAvatarSelector();
				using (new GUILayout.HorizontalScope())
				{
					if ((bool)(UnityEngine.Object)(object)selectedAvatar)
					{
						List<string> list = new List<string>();
						string[] avatarParameterNames = ADOverhaul.avatarParameterNames;
						foreach (string text in avatarParameterNames)
						{
							int num = text.LastIndexOf("_IsGrabbed", StringComparison.Ordinal);
							if (num < 0)
							{
								num = text.LastIndexOf("_Angle", StringComparison.Ordinal);
							}
							if (num < 0)
							{
								num = text.LastIndexOf("_Stretch", StringComparison.Ordinal);
							}
							if (num >= 0)
							{
								list.Add(text);
							}
						}
						string[] proc = list.Select(_003C_003Ec.m_ManagerAuthentication.SetParams).Distinct().ToArray();
						string stringValue = tagAuthentication.stringValue;
						using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
						{
							stringValue = ADOEditorUtility.PopStatus("Parameter", stringValue, proc);
							if (changeCheckScope.changed)
							{
								tagAuthentication.stringValue = stringValue;
							}
						}
						using (new EditorGUI.DisabledScope((UnityEngine.Object)(object)selectedAvatar == null || string.IsNullOrEmpty(tagAuthentication.stringValue)))
						{
							if (ADOEditorUtility.IconButton(ADOEditorUtility.CustomizeRef().m_DispatcherSerializer))
							{
								GenericMenu genericMenu = new GenericMenu();
								using (IEnumerator<VRCAvatarDescriptor.CustomAnimLayer> enumerator = selectedAvatar.baseAnimationLayers.Concat(selectedAvatar.specialAnimationLayers).GetEnumerator())
								{
									while (enumerator.MoveNext())
									{
										_003C_003Ec__DisplayClass108_1 _003C_003Ec__DisplayClass108_ = new _003C_003Ec__DisplayClass108_1();
										_003C_003Ec__DisplayClass108_.configAuthentication = enumerator.Current;
										_003C_003Ec__DisplayClass108_.m_MockAuthentication = _003C_003Ec__DisplayClass108_.configAuthentication.animatorController as UnityEditor.Animations.AnimatorController;
										if (_003C_003Ec__DisplayClass108_.m_MockAuthentication == null)
										{
											continue;
										}
										UnityEngine.AnimatorControllerParameter[] parameters = _003C_003Ec__DisplayClass108_.m_MockAuthentication.parameters;
										ADOEditorUtility.PhysBoneParameter[] physBoneParameters = ADOEditorUtility.physBoneParameters;
										for (int i = 0; i < physBoneParameters.Length; i++)
										{
											_003C_003Ec__DisplayClass108_2 _003C_003Ec__DisplayClass108_2 = new _003C_003Ec__DisplayClass108_2();
											_003C_003Ec__DisplayClass108_2.advisorAuthentication = _003C_003Ec__DisplayClass108_;
											_003C_003Ec__DisplayClass108_2._StateAuthentication = physBoneParameters[i];
											_003C_003Ec__DisplayClass108_2._FieldAuthentication = tagAuthentication.stringValue + _003C_003Ec__DisplayClass108_2._StateAuthentication.suffix;
											if (!parameters.Any(_003C_003Ec__DisplayClass108_2.PatchImporter))
											{
												genericMenu.AddItem(new GUIContent($"{_003C_003Ec__DisplayClass108_2.advisorAuthentication.configAuthentication.type}/{_003C_003Ec__DisplayClass108_2._FieldAuthentication}"), on: false, _003C_003Ec__DisplayClass108_2.CheckImporter);
											}
										}
									}
								}
								genericMenu.ShowAsContext();
							}
						}
					}
					else
					{
						EditorGUILayout.PropertyField(tagAuthentication);
					}
				}
				VRCPhysBone vRCPhysBone = PublishImporter((Editor)m_InfoAuthentication) as VRCPhysBone;
				if (!(vRCPhysBone != null) || !Application.isPlaying || m_InfoAuthentication.serializedObject.isEditingMultipleObjects || string.IsNullOrEmpty(vRCPhysBone.parameter))
				{
					return;
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayoutUtils.LoginIterator(m_CodeIdentifier, null);
					foreach (ADOEditorUtility.PhysBoneParameter item in ADOEditorUtility.physBoneParameters.Where(_003C_003Ec.m_ManagerAuthentication.SortParams))
					{
						using (new EditorGUILayout.VerticalScope())
						{
							GUILayout.Label(item.suffix, EditorStyles.boldLabel, GUILayout.ExpandWidth(expand: true));
							GUILayoutUtils.PrepareIterator();
							GUILayout.Label(item.GetValueString(vRCPhysBone));
						}
						GUILayoutUtils.StopIterator();
					}
					GUILayoutUtils.CallIterator();
				}
			}

			static UnityEngine.Object PublishImporter(Editor editor_0)
			{
				return editor_0.target;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass108_1
		{
			public VRCAvatarDescriptor.CustomAnimLayer configAuthentication;

			public UnityEditor.Animations.AnimatorController m_MockAuthentication;
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass108_2
		{
			public ADOEditorUtility.PhysBoneParameter _StateAuthentication;

			public string _FieldAuthentication;

			public _003C_003Ec__DisplayClass108_1 advisorAuthentication;

			internal bool PatchImporter(UnityEngine.AnimatorControllerParameter p)
			{
				return p.name == _FieldAuthentication;
			}

			internal void CheckImporter()
			{
				advisorAuthentication.m_MockAuthentication.AddParameterIfMissing(_FieldAuthentication, _StateAuthentication.parameterType, 0f);
				Log($"Added {_FieldAuthentication} to {advisorAuthentication.configAuthentication.type} ({advisorAuthentication.m_MockAuthentication.name})");
				RefreshAvatarParameterNames();
			}
		}

		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003C_003Ec__DisplayClass116_0
		{
			public VRCPhysBone[] _MessageAuthentication;

			public VRCPhysBone _PolicyAuthentication;
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass120_0
		{
			public VRCPhysBone m_TokenizerAuthentication;

			public VRCPhysBone[] exceptionAuthentication;

			public SerializedProperty valueAuthentication;

			public SerializedProperty m_ErrorAuthentication;

			public float producerAuthentication;

			public float _TemplateAuthentication;

			public SerializedObject m_WriterAuthentication;

			public AlgoAuthentication _ClassAuthentication;

			internal void OrderServer(ADOEditorUtility.BoneNode b, float m)
			{
				if (m != 0f)
				{
					Matrix4x4 matrix = b.matrix;
					Vector4 column = matrix.GetColumn(3);
					float comp = m_TokenizerAuthentication.radius * m;
					EditorGUI.BeginChangeCheck();
					float num = ADOEditorUtility.RadiusHandle(matrix.rotation, column, comp, !m_TokenizerAuthentication.showGizmos, ADOSettings.Instance().handleSizeMultiplier);
					if (EditorGUI.EndChangeCheck())
					{
						float delta = num / m - m_TokenizerAuthentication.radius;
						CalculateServer(b, delta);
					}
					ADOEditorUtility.FindStatus(comp.ToString("F2"), column);
				}
			}

			internal void CalculateServer(ADOEditorUtility.BoneNode bone, float delta)
			{
				Event current = Event.current;
				bool alt = current.alt;
				if (exceptionAuthentication.Length == 1)
				{
					if (alt)
					{
						FillSingleton(delta, bone, valueAuthentication, m_ErrorAuthentication, producerAuthentication, _TemplateAuthentication);
						m_WriterAuthentication.ApplyModifiedProperties();
					}
					else
					{
						CalcServer(m_TokenizerAuthentication, delta);
					}
				}
				else if (alt)
				{
					CalcServer(m_TokenizerAuthentication, delta);
				}
				else if (current.shift)
				{
					_003C_003Ec__DisplayClass120_1 _003C_003Ec__DisplayClass120_ = new _003C_003Ec__DisplayClass120_1
					{
						dicAuthentication = CalcServer(m_TokenizerAuthentication, delta)
					};
					VRCPhysBone[] array = exceptionAuthentication;
					foreach (VRCPhysBone vRCPhysBone in array)
					{
						if (vRCPhysBone != m_TokenizerAuthentication)
						{
							DeleteServer(vRCPhysBone, _003C_003Ec__DisplayClass120_.MapServer);
						}
					}
				}
				else
				{
					VRCPhysBone[] array = exceptionAuthentication;
					foreach (VRCPhysBone targetPhysbone in array)
					{
						CalcServer(targetPhysbone, delta);
					}
				}
			}

			internal float CalcServer(VRCPhysBone targetPhysbone, float delta)
			{
				_003C_003Ec__DisplayClass120_2 _003C_003Ec__DisplayClass120_ = new _003C_003Ec__DisplayClass120_2
				{
					_PublisherAuthentication = this,
					m_SchemaAuthentication = delta,
					_BridgeAuthentication = 0f
				};
				DeleteServer(targetPhysbone, _003C_003Ec__DisplayClass120_.CancelServer);
				return _003C_003Ec__DisplayClass120_._BridgeAuthentication;
			}

			internal void DeleteServer(VRCPhysBone targetPhysbone, Action<SerializedProperty> action)
			{
				SerializedObject serializedObject = new SerializedObject(targetPhysbone);
				serializedObject.UpdateIfRequiredOrScript();
				SerializedProperty obj = serializedObject.FindProperty(_ClassAuthentication.m_ListenerAuthentication);
				action(obj);
				serializedObject.ApplyModifiedProperties();
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass120_1
		{
			public float dicAuthentication;

			public Action<SerializedProperty> containerAuthentication;

			internal void MapServer(SerializedProperty sp)
			{
				sp.floatValue = dicAuthentication;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass120_2
		{
			public float m_SchemaAuthentication;

			public float _BridgeAuthentication;

			public _003C_003Ec__DisplayClass120_0 _PublisherAuthentication;

			internal void CancelServer(SerializedProperty sp)
			{
				sp.floatValue = Mathf.Clamp(sp.floatValue + m_SchemaAuthentication, _PublisherAuthentication.producerAuthentication, _PublisherAuthentication._TemplateAuthentication);
				_BridgeAuthentication = sp.floatValue;
			}
		}

		private static readonly AnimBool[] _AnnotationIdentifier = new AnimBool[8];

		private static object m_CodeIdentifier;

		private static bool _CallbackIdentifier = true;

		private static VRCPhysBone[] selectedPhysBones;

		private static VRCPhysBone[] scenePhysBones;

		private static VRCPhysBoneCollider[] sceneColliders;

		private static Transform[] candidateTransforms;

		private static byte[] membershipStates;

		private static Editor m_ProcessorIdentifier;

		private static readonly int _TokenizerIdentifier = GUIUtility.GetControlID("ADOToolSelectionDragControlID".GetHashCode(), FocusType.Passive);

		private static readonly ADOEditorUtility.ResizeHandle m_ExceptionIdentifier = new ADOEditorUtility.ResizeHandle();

		private static SerializedProperty _ValueIdentifier;

		private static SerializedProperty _ErrorIdentifier;

		private static SerializedProperty producerIdentifier;

		private static SerializedProperty m_TemplateIdentifier;

		private static SerializedProperty _WriterIdentifier;

		private static SerializedProperty classIdentifier;

		private static SerializedProperty _DicIdentifier;

		private static SerializedProperty _ContainerIdentifier;

		private static SerializedProperty m_SchemaIdentifier;

		private static SerializedProperty bridgeIdentifier;

		private static SerializedProperty publisherIdentifier;

		private static SerializedProperty _MerchantIdentifier;

		private static SerializedProperty m_ProcIdentifier;

		private static SerializedProperty configurationAuthentication;

		private static SerializedProperty _IdentifierAuthentication;

		private static SerializedProperty m_AuthenticationAuthentication;

		private static SerializedProperty contextAuthentication;

		private static SerializedProperty _SerializerAuthentication;

		private static SerializedProperty m_MethodAuthentication;

		private static SerializedProperty consumerAuthentication;

		private static SerializedProperty m_UtilsAuthentication;

		private static SerializedProperty _PageAuthentication;

		private static SerializedProperty propertyAuthentication;

		private static SerializedProperty m_SingletonAuthentication;

		private static SerializedProperty _AccountAuthentication;

		private static SerializedProperty m_ParamsAuthentication;

		private static SerializedProperty importerAuthentication;

		private static SerializedProperty serverAuthentication;

		private static SerializedProperty m_WatcherAuthentication;

		private static SerializedProperty regAuthentication;

		private static SerializedProperty processAuthentication;

		private static SerializedProperty statusAuthentication;

		private static SerializedProperty m_ValAuthentication;

		private static SerializedProperty adapterAuthentication;

		private static SerializedProperty _ProxyAuthentication;

		private static SerializedProperty m_RefAuthentication;

		private static SerializedProperty comparatorAuthentication;

		private static SerializedProperty _ProductAuthentication;

		private static SerializedProperty iteratorAuthentication;

		private static SerializedProperty m_PredicateAuthentication;

		private static SerializedProperty _CollectionAuthentication;

		private static SerializedProperty interceptorAuthentication;

		private static SerializedProperty m_RegistryAuthentication;

		private static SerializedProperty _ClientAuthentication;

		private static SerializedProperty m_ObserverAuthentication;

		private static SerializedProperty broadcasterAuthentication;

		private static SerializedProperty m_EventAuthentication;

		private static SerializedProperty m_RecordAuthentication;

		private static SerializedProperty resolverAuthentication;

		private static SerializedProperty tagAuthentication;

		private static SerializedProperty _FilterAuthentication;

		private static SerializedProperty m_FactoryAuthentication;

		private static SerializedProperty m_AttributeAuthentication;

		private static SerializedProperty m_InstanceAuthentication;

		private static AlgoAuthentication[] bindings;

		private static GUIContent[] bindingLabels;

		private static int[] bindingPopupValues;

		private static Dictionary<int, int> popupValueToBindingIndex;

		private static Dictionary<int, int> bindingIndexToPopupValue;

		private static bool bindingLabelsBuilt;

		private static int editedBindingIndex = -1;

		private static readonly ExclusiveSelectionState toolModes = new ExclusiveSelectionState(7);

		private static readonly string[] toolModeNames = new string[7] { "None", "End Position Edit", "Ignore Selection", "Ignore Copy", "Collision Selection", "Collision Copy", "Property Edit" };

		private static int _DefinitionAuthentication = -1;

		private static Vector3 initializerAuthentication;

		private static Type m_TokenAuthentication;

		private static Type getterAuthentication;

		private static bool threadAuthentication;

		[SpecialName]
		private static AlgoAuthentication activeBinding()
		{
			if (isEditingProperty())
			{
				return bindings[editedBindingIndex];
			}
			return null;
		}

		[SpecialName]
		private static bool isEditingProperty()
		{
			if (editedBindingIndex < 0)
			{
				return false;
			}
			return toolModes.activeIndex == 6;
		}

		[SpecialName]
		private static bool isEditingEndpoints()
		{
			return toolModes.activeIndex == 1;
		}

		[SpecialName]
		private static void ResetAccount(bool noinit)
		{
			toolModes.SetSelected(1, noinit);
		}

		[SpecialName]
		private static bool isSelectingIgnoreTransforms()
		{
			return toolModes.activeIndex == 2;
		}

		[SpecialName]
		private static void ExcludeAccount(bool isinfo)
		{
			toolModes.SetSelected(2, isinfo);
		}

		[SpecialName]
		private static bool isCopyingIgnoreTransforms()
		{
			return toolModes.activeIndex == 3;
		}

		[SpecialName]
		private static void FindAccount(bool updatelast)
		{
			toolModes.SetSelected(3, updatelast);
		}

		[SpecialName]
		private static bool isSelectingColliders()
		{
			return toolModes.activeIndex == 4;
		}

		[SpecialName]
		private static void CreateAccount(bool loadlast)
		{
			toolModes.SetSelected(4, loadlast);
		}

		[SpecialName]
		private static bool isCopyingColliders()
		{
			return toolModes.activeIndex == 5;
		}

		[SpecialName]
		private static void RunParams(bool updatelast)
		{
			toolModes.SetSelected(5, updatelast);
		}

		public override void OnInspectorGUI()
		{
			if (!FlushConfiguration())
			{
				if (!isLicensed)
				{
					EnableConfiguration(SelectSingleton);
				}
			}
			else
			{
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
					return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
				})())
				{
					return;
				}
				ConcatSingleton();
				base.serializedObject.Update();
				CacheProperties();
				ReadConfiguration(selectedPhysBones);
				EditorGUIUtility.labelWidth = 160f;
				int num = 0;
				AnimBool[] annotationIdentifier = _AnnotationIdentifier;
				num = 1;
				DrawFoldoutBox("Transforms", annotationIdentifier[0], null, delegate
				{
					using (new GUILayout.HorizontalScope())
					{
						EditorGUILayout.PropertyField(producerIdentifier, new GUIContent("Root"));
						if (GUILayout.Button(new GUIContent("S", "Set to Self"), GUILayout.Width(18f), GUILayout.Height(18f)))
						{
							UnityEngine.Object[] array = base.targets;
							for (int i = 0; i < array.Length; i++)
							{
								VRCPhysBone vRCPhysBone = array[i] as VRCPhysBone;
								if ((bool)vRCPhysBone)
								{
									SerializedObject obj = new SerializedObject(vRCPhysBone);
									obj.FindProperty("rootTransform").objectReferenceValue = vRCPhysBone.transform;
									obj.ApplyModifiedProperties();
								}
							}
						}
					}
					ResetAccount(DrawPropertyWithEditToggle(_WriterIdentifier, isEditingEndpoints()));
					EditorGUILayout.PropertyField(classIdentifier);
					using (new GUILayout.VerticalScope("box"))
					{
						using (new GUILayout.HorizontalScope())
						{
							GUILayout.Space(12f);
							m_TemplateIdentifier.isExpanded = EditorGUILayout.Foldout(m_TemplateIdentifier.isExpanded, "Ignore Transforms", toggleOnLabelClick: true);
							GUILayout.FlexibleSpace();
							FindAccount(DrawIconToggle(isCopyingIgnoreTransforms(), ADOEditorUtility.CustomizeRef().m_ExporterSerializer));
							EditorGUI.BeginChangeCheck();
							ExcludeAccount(DrawIconToggle(isSelectingIgnoreTransforms(), ADOEditorUtility.CustomizeRef().advisorSerializer));
							if (EditorGUI.EndChangeCheck())
							{
								RefreshIgnoreTransformStates();
							}
						}
						if (m_TemplateIdentifier.isExpanded)
						{
							EditorGUI.indentLevel++;
							ADOEditorUtility.ObjectListField<Transform>(m_TemplateIdentifier);
							EditorGUI.indentLevel--;
						}
					}
				});
				AnimBool[] annotationIdentifier2 = _AnnotationIdentifier;
				num = 2;
				DrawFoldoutBox("Forces", annotationIdentifier2[1], ViewSingleton, delegate
				{
					bool flag = _ErrorIdentifier.enumValueIndex == 1;
					PostSingleton(0);
					PostSingleton(1, new GUIContent((!flag) ? "Spring" : "Momentum", m_SchemaIdentifier.tooltip));
					if (flag)
					{
						PostSingleton(2);
					}
					PostSingleton(3);
					PostSingleton(4);
					PostSingleton(5);
					if (configurationAuthentication != null)
					{
						EditorGUILayout.PropertyField(configurationAuthentication);
					}
				});
				DrawFoldoutBox("Limits", _AnnotationIdentifier[num++], null, delegate
				{
					int enumValueIndex = _AccountAuthentication.enumValueIndex;
					EditorGUILayout.PropertyField(_AccountAuthentication, new GUIContent("Type"));
					if (enumValueIndex <= 0)
					{
						return;
					}
					PostSingleton(7);
					if (enumValueIndex == 3)
					{
						PostSingleton(8);
					}
					EditorGUILayout.PropertyField(regAuthentication);
					using (new GUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("Limit Rotation Curves");
						UpdateSingleton(adapterAuthentication, "X", isproc: false);
						UpdateSingleton(_ProxyAuthentication, "Y", isproc: false);
						UpdateSingleton(m_RefAuthentication, "Z", isproc: false);
						if (ADOEditorUtility.CallStatus(ADOEditorUtility.CustomizeRef().baseSerializer, GUI.skin.label, GUILayout.Width(14f)))
						{
							SerializedProperty serializedProperty = adapterAuthentication;
							SerializedProperty proxyAuthentication = _ProxyAuthentication;
							AnimationCurve animationCurve = (m_RefAuthentication.animationCurveValue = new AnimationCurve());
							AnimationCurve animationCurveValue = (proxyAuthentication.animationCurveValue = animationCurve);
							serializedProperty.animationCurveValue = animationCurveValue;
						}
					}
				});
				bool _ModelAuthentication = m_UtilsAuthentication != null;
				Action dir = null;
				if (!_ModelAuthentication)
				{
					dir = delegate
					{
						CallConfiguration(consumerAuthentication, "Allow Collsion", null, GUILayout.ExpandWidth(expand: false));
					};
				}
				DrawFoldoutBox("Collisions", _AnnotationIdentifier[num++], dir, delegate
				{
					PostSingleton(6);
					if (_ModelAuthentication)
					{
						DrawSelfOthersToggles(consumerAuthentication, m_UtilsAuthentication);
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new GUILayout.HorizontalScope())
						{
							GUILayout.Space(12f);
							m_SingletonAuthentication.isExpanded = EditorGUILayout.Foldout(m_SingletonAuthentication.isExpanded, "Colliders", toggleOnLabelClick: true);
							GUILayout.FlexibleSpace();
							RunParams(DrawIconToggle(isCopyingColliders(), ADOEditorUtility.CustomizeRef().m_ExporterSerializer));
							EditorGUI.BeginChangeCheck();
							CreateAccount(DrawIconToggle(isSelectingColliders(), ADOEditorUtility.CustomizeRef().advisorSerializer));
							if (EditorGUI.EndChangeCheck())
							{
								RefreshColliderStates();
							}
						}
						if (m_SingletonAuthentication.isExpanded)
						{
							EditorGUI.indentLevel++;
							ADOEditorUtility.ObjectListField<VRCPhysBoneCollider>(m_SingletonAuthentication);
							EditorGUI.indentLevel--;
						}
					}
				});
				Action dir2 = null;
				if (!_ModelAuthentication)
				{
					dir2 = delegate
					{
						CallConfiguration(comparatorAuthentication, "Allow Grabbing", null, GUILayout.ExpandWidth(expand: false));
						CallConfiguration(iteratorAuthentication, "Allow Posing", null, GUILayout.ExpandWidth(expand: false));
					};
				}
				DrawFoldoutBox("Grab & Pose", _AnnotationIdentifier[num++], dir2, delegate
				{
					if (_ModelAuthentication)
					{
						while (true)
						{
							DrawSelfOthersToggles(comparatorAuthentication, _ProductAuthentication);
							DrawSelfOthersToggles(iteratorAuthentication, m_PredicateAuthentication);
						}
					}
					EditorGUILayout.PropertyField(_CollectionAuthentication);
					EditorGUILayout.PropertyField(interceptorAuthentication);
				});
				DrawFoldoutBox("Stretch & Squish", _AnnotationIdentifier[num++], null, delegate
				{
					ListSingleton(13);
					ListSingleton(14);
					if (_ValueIdentifier.enumValueIndex > 0)
					{
						ListSingleton(12);
					}
				});
				DrawFoldoutBox("Options", _AnnotationIdentifier[num++], null, delegate
				{
					EditorGUILayout.PropertyField(_ValueIdentifier);
					EditorGUILayout.PropertyField(resolverAuthentication);
					EditorGUILayout.PropertyField(_FilterAuthentication);
					DrawTargetAvatarSelector();
					using (new GUILayout.HorizontalScope())
					{
						if ((bool)(UnityEngine.Object)(object)selectedAvatar)
						{
							List<string> list = new List<string>();
							string[] avatarParameterNames = ADOverhaul.avatarParameterNames;
							foreach (string text in avatarParameterNames)
							{
								int num2 = text.LastIndexOf("_IsGrabbed", StringComparison.Ordinal);
								if (num2 < 0)
								{
									num2 = text.LastIndexOf("_Angle", StringComparison.Ordinal);
								}
								if (num2 < 0)
								{
									num2 = text.LastIndexOf("_Stretch", StringComparison.Ordinal);
								}
								if (num2 >= 0)
								{
									list.Add(text);
								}
							}
							string[] proc = list.Select(_003C_003Ec.m_ManagerAuthentication.SetParams).Distinct().ToArray();
							string stringValue = tagAuthentication.stringValue;
							using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
							{
								stringValue = ADOEditorUtility.PopStatus("Parameter", stringValue, proc);
								if (changeCheckScope.changed)
								{
									tagAuthentication.stringValue = stringValue;
								}
							}
							using (new EditorGUI.DisabledScope((UnityEngine.Object)(object)selectedAvatar == null || string.IsNullOrEmpty(tagAuthentication.stringValue)))
							{
								if (ADOEditorUtility.IconButton(ADOEditorUtility.CustomizeRef().m_DispatcherSerializer))
								{
									GenericMenu genericMenu = new GenericMenu();
									using (IEnumerator<VRCAvatarDescriptor.CustomAnimLayer> enumerator = selectedAvatar.baseAnimationLayers.Concat(selectedAvatar.specialAnimationLayers).GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											_003C_003Ec__DisplayClass108_1 _003C_003Ec__DisplayClass108_ = new _003C_003Ec__DisplayClass108_1();
											_003C_003Ec__DisplayClass108_.configAuthentication = enumerator.Current;
											_003C_003Ec__DisplayClass108_.m_MockAuthentication = _003C_003Ec__DisplayClass108_.configAuthentication.animatorController as UnityEditor.Animations.AnimatorController;
											if (!(_003C_003Ec__DisplayClass108_.m_MockAuthentication == null))
											{
												UnityEngine.AnimatorControllerParameter[] parameters = _003C_003Ec__DisplayClass108_.m_MockAuthentication.parameters;
												ADOEditorUtility.PhysBoneParameter[] physBoneParameters = ADOEditorUtility.physBoneParameters;
												for (int i = 0; i < physBoneParameters.Length; i++)
												{
													_003C_003Ec__DisplayClass108_2 _003C_003Ec__DisplayClass108_2 = new _003C_003Ec__DisplayClass108_2();
													_003C_003Ec__DisplayClass108_2.advisorAuthentication = _003C_003Ec__DisplayClass108_;
													_003C_003Ec__DisplayClass108_2._StateAuthentication = physBoneParameters[i];
													_003C_003Ec__DisplayClass108_2._FieldAuthentication = tagAuthentication.stringValue + _003C_003Ec__DisplayClass108_2._StateAuthentication.suffix;
													if (!parameters.Any(_003C_003Ec__DisplayClass108_2.PatchImporter))
													{
														genericMenu.AddItem(new GUIContent($"{_003C_003Ec__DisplayClass108_2.advisorAuthentication.configAuthentication.type}/{_003C_003Ec__DisplayClass108_2._FieldAuthentication}"), on: false, _003C_003Ec__DisplayClass108_2.CheckImporter);
													}
												}
											}
										}
									}
									genericMenu.ShowAsContext();
								}
							}
						}
						else
						{
							EditorGUILayout.PropertyField(tagAuthentication);
						}
					}
					VRCPhysBone vRCPhysBone = _003C_003Ec__DisplayClass108_0.PublishImporter((Editor)this) as VRCPhysBone;
					if (vRCPhysBone != null && Application.isPlaying && !base.serializedObject.isEditingMultipleObjects && !string.IsNullOrEmpty(vRCPhysBone.parameter))
					{
						using (new EditorGUILayout.HorizontalScope())
						{
							GUILayoutUtils.LoginIterator(m_CodeIdentifier, null);
							foreach (ADOEditorUtility.PhysBoneParameter item in ADOEditorUtility.physBoneParameters.Where(_003C_003Ec.m_ManagerAuthentication.SortParams))
							{
								using (new EditorGUILayout.VerticalScope())
								{
									GUILayout.Label(item.suffix, EditorStyles.boldLabel, GUILayout.ExpandWidth(expand: true));
									GUILayoutUtils.PrepareIterator();
									GUILayout.Label(item.GetValueString(vRCPhysBone));
								}
								GUILayoutUtils.StopIterator();
							}
							GUILayoutUtils.CallIterator();
						}
					}
				});
				DrawFoldoutBox("Gizmos", _AnnotationIdentifier[num++], delegate
				{
					CallConfiguration(m_FactoryAuthentication, "Show Gizmos", delegate
					{
						if ((bool)ADOSettings.Instance().globalGizmo)
						{
							ADOSettings.Instance().gizmosActive.SetValue(m_FactoryAuthentication.boolValue);
						}
					}, GUILayout.ExpandWidth(expand: false));
					bool flag;
					string text = ((!(flag = ADOSettings.Instance().globalGizmo)) ? "Local Setting" : "Global Setting");
					using (new GUIColorScope(GUIColorScope.ColoringType.BG, flag, ADOEditorUtility.validColor, ADOEditorUtility.warningColor))
					{
						using (new ADOSettings.SettingsChangeScope(ApplyGlobalGizmoSettings))
						{
							ADOSettings.Instance().globalGizmo.SetValue(GUILayout.Toggle(flag, text, GUI.skin.button, GUILayout.ExpandWidth(expand: false)));
						}
					}
				}, delegate
				{
					if ((bool)ADOSettings.Instance().globalGizmo)
					{
						ADOSettings.Instance().gizmoBoneOpacity.SetValue(EditorGUILayout.Slider("Bone Opacity", ADOSettings.Instance().gizmoBoneOpacity, 0f, 1f));
						ADOSettings.Instance().gizmoLimitOpacity.SetValue(EditorGUILayout.Slider("Limit Opacitiy", ADOSettings.Instance().gizmoLimitOpacity, 0f, 1f));
					}
					else
					{
						m_AttributeAuthentication.floatValue = EditorGUILayout.Slider("Bone Opacity", m_AttributeAuthentication.floatValue, 0f, 1f);
						m_InstanceAuthentication.floatValue = EditorGUILayout.Slider("Limit Opacitiy", m_InstanceAuthentication.floatValue, 0f, 1f);
					}
				});
				TestConfiguration(base.serializedObject, selectedPhysBones, delegate(VRCPhysBone pb)
				{
					pb.configHasUpdated = true;
				});
				GetConfiguration();
				DrawToolHeader();
			}
		}

		private void method_0()
		{
			if (toolModes.activeIndex < 0)
			{
				return;
			}
			VRCPhysBone vRCPhysBone = (VRCPhysBone)TargetObject();
			if (!(vRCPhysBone == null))
			{
				Tools.hidden = true;
				ADOEditorUtility.BoneChainTree boneChainTree = new ADOEditorUtility.BoneChainTree(vRCPhysBone);
				boneChainTree.BuildChains();
				if (isEditingEndpoints())
				{
					CustomizeSingleton(selectedPhysBones, boneChainTree);
				}
				if (isEditingProperty())
				{
					CancelSingleton(selectedPhysBones, boneChainTree, activeBinding());
				}
			}
		}

		private static void VerifySingleton(SceneView task)
		{
			ADOEditorUtility.BeginDeferredCursorRects();
			ConcatSingleton();
			SetSingleton(task);
			if (isSelectingColliders())
			{
				bool flag = ADOSettings.Instance().onSceneNameLabels;
				using (new GUIColorScope(GUIColorScope.ColoringType.FG, flag, ADOSettings.Instance().labelColor.GetValue()))
				{
					for (int i = 0; i < sceneColliders.Length; i++)
					{
						int _ExporterAuthentication = i;
						VRCPhysBoneCollider _CreatorAuthentication = sceneColliders[_ExporterAuthentication];
						ADOEditorUtility.SphereHandle first = ADOEditorUtility.SphereHandle.Create(_CreatorAuthentication.transform.TransformPoint(_CreatorAuthentication.position), flag ? _CreatorAuthentication.name : string.Empty, (float)ADOSettings.Instance().handleSizeMultiplier * 0.05f, handleControlIdBase + i, delegate
						{
							m_SingletonAuthentication.DestroyStatus<VRCPhysBoneCollider>(ADOEditorUtility.CycleToggleState(membershipStates, _ExporterAuthentication), _CreatorAuthentication);
						});
						first.onDraw = delegate(ADOEditorUtility.SphereHandle sc2)
						{
							Handles.color = ADOSettings.Instance().StateColors()[membershipStates[_ExporterAuthentication]];
							ADOEditorUtility.SphereHandle.DrawDefault(sc2);
						};
						ADOEditorUtility.DrawSphereHandle(first);
					}
				}
			}
			if (isSelectingIgnoreTransforms())
			{
				bool flag2 = ADOSettings.Instance().onSceneNameLabels;
				using (new GUIColorScope(GUIColorScope.ColoringType.FG, flag2, ADOSettings.Instance().labelColor.GetValue()))
				{
					for (int num = 0; num < candidateTransforms.Length; num++)
					{
						Transform _ConnectionAuthentication = candidateTransforms[num];
						int m_DispatcherAuthentication = num;
						ADOEditorUtility.SphereHandle first2 = ADOEditorUtility.SphereHandle.Create(_ConnectionAuthentication.position, (!flag2) ? string.Empty : _ConnectionAuthentication.name, (float)ADOSettings.Instance().handleSizeMultiplier * 0.25f, handleControlIdBase + num, delegate
						{
							m_TemplateIdentifier.DestroyStatus<Transform>(ADOEditorUtility.CycleToggleState(membershipStates, m_DispatcherAuthentication), _ConnectionAuthentication);
						});
						first2.onDraw = delegate(ADOEditorUtility.SphereHandle sc2)
						{
							Handles.color = ADOSettings.Instance().StateColors()[membershipStates[m_DispatcherAuthentication]];
							ADOEditorUtility.SphereHandle.DrawDefault(sc2);
						};
						ADOEditorUtility.DrawSphereHandle(first2);
					}
				}
			}
			if (isCopyingColliders())
			{
				bool flag3 = ADOSettings.Instance().onSceneNameLabels;
				Handles.color = ADOSettings.Instance().selectionColor.GetValue();
				using (new GUIColorScope(GUIColorScope.ColoringType.FG, flag3, ADOSettings.Instance().labelColor.GetValue()))
				{
					for (int num2 = 0; num2 < scenePhysBones.Length; num2++)
					{
						VRCPhysBone vRCPhysBone = scenePhysBones[num2];
						int _ExpressionAuthentication = num2;
						ADOEditorUtility.DrawSphereHandle(ADOEditorUtility.SphereHandle.Create(vRCPhysBone.transform.position, flag3 ? vRCPhysBone.name : string.Empty, (float)ADOSettings.Instance().handleSizeMultiplier * 0.25f, handleControlIdBase + num2, delegate
						{
							VRCPhysBone[] array = selectedPhysBones;
							for (int j = 0; j < array.Length; j++)
							{
								array[j].colliders = scenePhysBones[_ExpressionAuthentication].colliders.ToList();
							}
							RunParams(updatelast: false);
							if (m_ProcessorIdentifier != null)
							{
								m_ProcessorIdentifier.Repaint();
							}
						}));
					}
				}
			}
			if (isCopyingIgnoreTransforms())
			{
				bool flag4 = ADOSettings.Instance().onSceneNameLabels;
				Handles.color = ADOSettings.Instance().selectionColor.GetValue();
				using (new GUIColorScope(GUIColorScope.ColoringType.FG, flag4, ADOSettings.Instance().labelColor.GetValue()))
				{
					for (int num3 = 0; num3 < scenePhysBones.Length; num3++)
					{
						VRCPhysBone vRCPhysBone2 = scenePhysBones[num3];
						int _DecoratorAuthentication = num3;
						ADOEditorUtility.DrawSphereHandle(ADOEditorUtility.SphereHandle.Create(vRCPhysBone2.transform.position, flag4 ? vRCPhysBone2.name : string.Empty, (float)ADOSettings.Instance().handleSizeMultiplier * 0.25f, handleControlIdBase + num3, delegate
						{
							VRCPhysBone[] array = selectedPhysBones;
							for (int j = 0; j < array.Length; j++)
							{
								array[j].ignoreTransforms = scenePhysBones[_DecoratorAuthentication].ignoreTransforms.ToList();
							}
							FindAccount(updatelast: false);
							if (m_ProcessorIdentifier != null)
							{
								m_ProcessorIdentifier.Repaint();
							}
						}));
					}
				}
			}
			Event current = Event.current;
			if (Tools.current != Tool.View && !current.alt && (bool)ADOSettings.Instance().ignoreSceneClicks && toolModes.activeIndex > 0 && current.type == EventType.MouseDown && current.button == 0)
			{
				GUIUtility.hotControl = handleControlIdBase - 1;
				current.Use();
			}
			ADOEditorUtility.EndDeferredCursorRects();
		}

		private static void SetSingleton(SceneView info)
		{
			Rect ivk = info.AddStatus();
			int num = toolModes.activeIndex;
			if (num < 0)
			{
				num = 0;
			}
			bool flag = num > 0;
			if ((bool)ADOSettings.Instance().onSceneToolSelection && (flag || (bool)ADOSettings.Instance().onSceneToolSelectionAlwaysVisible))
			{
				ADOEditorUtility.PositionFlag enumValue = ADOSettings.Instance().toolSelectionOverlayAlignment.GetEnumValue<ADOEditorUtility.PositionFlag>();
				bool flag2;
				using (new ADOEditorUtility.SceneViewPanel(info, 250f, 34f, enumValue, m_ExceptionIdentifier))
				{
					Rect lastRect;
					using (new GUILayout.HorizontalScope())
					{
						using (new EditorGUI.DisabledScope(toolModes.activeIndex <= 0))
						{
							if (ADOEditorUtility.IconButton((!ADOSettings.Instance().ignoreSceneClicks) ? ADOEditorUtility.CustomizeRef().decoratorSerializer : ADOEditorUtility.CustomizeRef()._ParamSerializer))
							{
								ADOSettings.Instance().ignoreSceneClicks.Toggle();
							}
						}
						GUILayout.FlexibleSpace();
						GUILayout.Label("ADO Tool:", ADOEditorUtility.MapRef().m_WriterSerializer);
						lastRect = GUILayoutUtility.GetLastRect();
						GUIContent content = new GUIContent(toolModeNames[num]);
						float x = GUI.skin.label.CalcSize(content).x;
						EditorGUI.BeginChangeCheck();
						int activeIndex = EditorGUILayout.Popup(GUIContent.none, num, toolModeNames, GUILayout.Width(x + 20f));
						if (EditorGUI.EndChangeCheck())
						{
							toolModes.activeIndex = activeIndex;
							if (toolModes.activeIndex == 0)
							{
								ExitTool();
							}
							else if (toolModes.activeIndex == 6)
							{
								SetPropertyEditTarget(0);
							}
							else
							{
								SetPropertyEditTarget(-1);
								if (toolModes.activeIndex > 1 && toolModes.activeIndex < 4)
								{
									RefreshIgnoreTransformStates();
								}
								else
								{
									RefreshColliderStates();
								}
							}
							SceneView.RepaintAll();
						}
						GUILayout.FlexibleSpace();
						if (ADOEditorUtility.IconButton(ADOEditorUtility.CustomizeRef().fieldSerializer))
						{
							ADOverhaulWindow.ShowWindow();
						}
					}
					flag2 = ADOEditorUtility.HasMouseCapture(lastRect, _TokenizerIdentifier);
					ADOEditorUtility.AddCursorRect(lastRect, MouseCursor.Pan);
				}
				if (flag2)
				{
					Handles.BeginGUI();
					ADOSettings.Instance().toolSelectionOverlayAlignment.IntValue = (int)ADOEditorUtility.AnchorPicker(enumValue, ivk);
					Handles.EndGUI();
				}
			}
			if (!flag || SortSingleton(info))
			{
				InvokeSingleton(info);
			}
		}

		private static bool SortSingleton(SceneView first)
		{
			bool drawGizmos = first.drawGizmos;
			if (!drawGizmos)
			{
				DrawTitledOverlay(first, "Gizmos Disabled", delegate
				{
					GUILayout.Label("Handles are hidden.", ADOEditorUtility.MapRef().m_WriterSerializer);
					if (ADOEditorUtility.PatchStatus("Enable Gizmos"))
					{
						first.drawGizmos = true;
					}
				}, 200f, 80f);
			}
			return drawGizmos;
		}

		private static void InvokeSingleton(SceneView value)
		{
			if (!ADOSettings.Instance().onSceneEditingOverlay || toolModes.activeIndex <= 0)
			{
				return;
			}
			bool _IssuerAuthentication = isEditingProperty();
			bool codeAuthentication = ADOSettings.Instance().onSceneTooltip;
			if (!_IssuerAuthentication && !codeAuthentication)
			{
				return;
			}
			bool m_PrototypeAuthentication = selectedPhysBones.Length > 1;
			bool flag = isSelectingIgnoreTransforms();
			bool flag2 = isSelectingColliders();
			bool flag3 = isCopyingIgnoreTransforms();
			bool flag4 = isCopyingColliders();
			bool callbackAuthentication = isEditingEndpoints();
			bool facadeAuthentication = flag || flag3;
			bool _ComposerAuthentication = flag2 || flag4;
			bool m_RequestAuthentication = flag3 || flag4;
			bool _BaseAuthentication = callbackAuthentication || _IssuerAuthentication;
			float ivk = ((!codeAuthentication) ? 33 : ((!_IssuerAuthentication && !callbackAuthentication) ? 60 : (m_PrototypeAuthentication ? 100 : ((!_IssuerAuthentication) ? 60 : 80))));
			float pol = ((!m_PrototypeAuthentication) ? 240 : 340);
			Rect annotationAuthentication;
			DrawOverlay(value, delegate
			{
				using (new GUILayout.HorizontalScope())
				{
					string text = string.Concat(((!m_PrototypeAuthentication) ? "" : "Multi-") + (_BaseAuthentication ? "Editing" : ((!m_RequestAuthentication) ? "Selecting" : "Copying")), _IssuerAuthentication ? ":" : (facadeAuthentication ? " Ignore Transforms" : ((!_ComposerAuthentication) ? " End Position" : " Colliders")));
					ADOEditorUtility.IconSpacer();
					GUILayout.FlexibleSpace();
					GUILayout.Label(text, ADOEditorUtility.MapRef().m_WriterSerializer);
					annotationAuthentication = GUILayoutUtility.GetLastRect();
					if (_IssuerAuthentication)
					{
						EditorGUI.BeginChangeCheck();
						GUIContent content = bindingLabels[editedBindingIndex];
						float x = GUI.skin.label.CalcSize(content).x;
						int key = EditorGUILayout.IntPopup(GUIContent.none, bindingIndexToPopupValue[editedBindingIndex], bindingLabels, bindingPopupValues, GUILayout.Width(x + 20f));
						if (EditorGUI.EndChangeCheck())
						{
							editedBindingIndex = popupValueToBindingIndex[key];
							SceneView.RepaintAll();
						}
					}
					GUILayout.FlexibleSpace();
					DrawSettingsButton();
					return annotationAuthentication;
				}
			}, delegate
			{
				if (codeAuthentication)
				{
					GUILayout.Label("Press Enter or Escape to exit", ADOEditorUtility.MapRef().m_WriterSerializer);
					if (_IssuerAuthentication || callbackAuthentication)
					{
						if (m_PrototypeAuthentication)
						{
							GUILayout.Label("Hold Alt to edit the target physbone only", ADOEditorUtility.MapRef().m_WriterSerializer);
							GUILayout.Label("Hold Shift to set the physbones to the same value", ADOEditorUtility.MapRef().m_WriterSerializer);
						}
						else if (_IssuerAuthentication)
						{
							GUILayout.Label("Hold Alt to edit the curve", ADOEditorUtility.MapRef().m_WriterSerializer);
						}
					}
				}
			}, pol, ivk);
		}

		private void CustomizeSingleton(VRCPhysBone[] res, ADOEditorUtility.BoneChainTree result)
		{
			_003C_003Ec__DisplayClass116_0 vis = default(_003C_003Ec__DisplayClass116_0);
			vis._MessageAuthentication = res;
			vis._PolicyAuthentication = result.physBone;
			ADOEditorUtility.BoneNode[] array = result.nodes.Where((ADOEditorUtility.BoneNode b) => b.isEndBone && !b.isVirtual).ToArray();
			foreach (ADOEditorUtility.BoneNode boneNode in array)
			{
				Transform transform = boneNode.transform;
				Vector3 vector = transform.TransformPoint(vis._PolicyAuthentication.endpointPosition);
				if (!vis._PolicyAuthentication.showGizmos || !(vis._PolicyAuthentication.boneOpacity >= 0.05f))
				{
					Handles.DrawLine(transform.position, vector);
				}
				Quaternion rotation = ((Tools.pivotRotation != PivotRotation.Global) ? transform.rotation : Quaternion.identity);
				Vector3 vector2 = Vector3.zero;
				bool flag = false;
				EditorGUI.BeginChangeCheck();
				Vector3 vector3 = Handles.PositionHandle(vector, rotation);
				if (EditorGUI.EndChangeCheck())
				{
					vector2 = vector3;
					flag = true;
				}
				int hotControl = GUIUtility.hotControl;
				Vector3 direction;
				if (hotControl != _DefinitionAuthentication)
				{
					_DefinitionAuthentication = -1;
					direction = vector - transform.position;
					if (!(direction.magnitude >= 0.01f))
					{
						direction = ((boneNode.parent == null) ? (-transform.forward) : (vector - boneNode.parent.transform.position));
					}
				}
				else
				{
					direction = initializerAuthentication;
				}
				Handles.color = ADOEditorUtility.warningColor;
				EditorGUI.BeginChangeCheck();
				Vector3 vector4 = Handles.Slider(vector, direction);
				if (EditorGUI.EndChangeCheck())
				{
					if (hotControl != _DefinitionAuthentication)
					{
						_DefinitionAuthentication = hotControl;
						initializerAuthentication = direction;
					}
					vector2 = vector4;
					flag = true;
				}
				if (flag)
				{
					ApplyEndpointOffset(transform.InverseTransformVector(vector2 - vector), ref vis);
				}
			}
		}

		private static void ConcatSingleton()
		{
			Event current = Event.current;
			if (current.type == EventType.Used || current.type != EventType.KeyDown)
			{
				return;
			}
			KeyCode keyCode = current.keyCode;
			if (toolModes.activeIndex >= 0)
			{
				if (keyCode == KeyCode.Return || keyCode == KeyCode.KeypadEnter || keyCode == KeyCode.Escape)
				{
					ExitTool();
					current.Use();
				}
			}
			else
			{
				if (!current.control)
				{
					return;
				}
				switch (keyCode)
				{
				case KeyCode.E:
					if (!isEditingProperty())
					{
						SetPropertyEditTarget(0);
					}
					else
					{
						ExitTool();
					}
					if (isTesting)
					{
						ToggleTestMode();
					}
					current.Use();
					break;
				case KeyCode.T:
					ToggleTestMode();
					current.Use();
					break;
				}
			}
		}

		private static void MapSingleton(ADOEditorUtility.BoneChainTree def, AnimationCurve pred, Action<ADOEditorUtility.BoneNode, float> proc, bool isinstance2 = false)
		{
			bool flag = pred == null || pred.length == 0;
			foreach (ADOEditorUtility.BoneNode node in def.nodes)
			{
				float num = ((!flag) ? pred.Evaluate(node.GetNormalizedDepth()) : 1f);
				if (isinstance2)
				{
					num *= node.GetMaxScale();
				}
				proc(node, num);
			}
		}

		private static void FillSingleton(float last, ADOEditorUtility.BoneNode col, SerializedProperty proc, SerializedProperty selection2, float ident3 = 0f, float item4 = float.PositiveInfinity)
		{
			AnimationCurve animationCurve = ((selection2.animationCurveValue != null && selection2.animationCurveValue.length >= 2) ? selection2.animationCurveValue : new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f)));
			int num = -1;
			Keyframe[] keys = animationCurve.keys;
			for (int i = 0; i < keys.Length; i++)
			{
				if (!(Math.Abs(keys[i].time - col.GetNormalizedDepth()) >= 0.01f))
				{
					num = i;
					break;
				}
			}
			float num2;
			if (num != -1)
			{
				num2 = keys[num].value;
			}
			else
			{
				num2 = animationCurve.Evaluate(col.GetNormalizedDepth());
				num = animationCurve.AddKey(col.GetNormalizedDepth(), num2);
			}
			float num3 = col.EvaluateCurve(animationCurve);
			bool flag = ident3 < 0f;
			if (!(proc.floatValue * num3 >= 0f))
			{
				last *= -1f;
			}
			float num4 = (flag ? (num2 + last / item4) : (num2 * ((proc.floatValue + last) / proc.floatValue)));
			if (!(num4 <= 1f) || num4 < (float)(flag ? (-1) : 0))
			{
				if (num4 < (float)(flag ? (-1) : 0))
				{
					animationCurve.MoveKey(num, new Keyframe(col.GetNormalizedDepth(), flag ? (-1) : 0));
				}
				else
				{
					float num5 = 1f / num4;
					float num6 = proc.floatValue / num5;
					if (!(num6 <= item4))
					{
						num5 = proc.floatValue / item4;
					}
					if (!(num6 >= ident3))
					{
						num5 = proc.floatValue / ident3;
					}
					animationCurve.MoveKey(num, new Keyframe(col.GetNormalizedDepth(), 1f));
					for (int j = 0; j < keys.Length; j++)
					{
						if (j != num)
						{
							animationCurve.MoveKey(j, new Keyframe(keys[j].time, keys[j].value * num5));
						}
					}
					proc.floatValue /= num5;
				}
			}
			else
			{
				animationCurve.MoveKey(num, new Keyframe(col.GetNormalizedDepth(), num4));
			}
			float num7 = animationCurve.keys.Select((Keyframe k) => k.value).Prepend(0f).Max();
			if (num7 < 0.8f)
			{
				float num8 = 1f / num7;
				for (int num9 = 0; num9 < keys.Length; num9++)
				{
					animationCurve.MoveKey(num9, new Keyframe(keys[num9].time, keys[num9].value * num8));
				}
				proc.floatValue /= num8;
			}
			selection2.animationCurveValue = animationCurve;
		}

		private static void CancelSingleton(VRCPhysBone[] instance, ADOEditorUtility.BoneChainTree result, AlgoAuthentication comp)
		{
			_003C_003Ec__DisplayClass120_0 CS_0024_003C_003E8__locals28 = new _003C_003Ec__DisplayClass120_0();
			CS_0024_003C_003E8__locals28.exceptionAuthentication = instance;
			CS_0024_003C_003E8__locals28._ClassAuthentication = comp;
			CS_0024_003C_003E8__locals28.m_TokenizerAuthentication = result.physBone;
			if (CS_0024_003C_003E8__locals28.m_TokenizerAuthentication == null)
			{
				return;
			}
			CS_0024_003C_003E8__locals28.m_WriterAuthentication = new SerializedObject(CS_0024_003C_003E8__locals28.m_TokenizerAuthentication);
			CS_0024_003C_003E8__locals28.m_WriterAuthentication.UpdateIfRequiredOrScript();
			CS_0024_003C_003E8__locals28.valueAuthentication = CS_0024_003C_003E8__locals28.m_WriterAuthentication.FindProperty(CS_0024_003C_003E8__locals28._ClassAuthentication.m_ListenerAuthentication);
			CS_0024_003C_003E8__locals28.m_ErrorAuthentication = CS_0024_003C_003E8__locals28.m_WriterAuthentication.FindProperty(CS_0024_003C_003E8__locals28._ClassAuthentication.m_ParserAuthentication);
			CS_0024_003C_003E8__locals28.producerAuthentication = CS_0024_003C_003E8__locals28._ClassAuthentication.repositoryAuthentication;
			CS_0024_003C_003E8__locals28._TemplateAuthentication = CS_0024_003C_003E8__locals28._ClassAuthentication._DescriptorAuthentication;
			float num = ADOSettings.Instance().handleSizeMultiplier;
			float num2 = Mathf.Clamp(HandleUtility.GetHandleSize(CS_0024_003C_003E8__locals28.m_TokenizerAuthentication.transform.position) * 0.05f * num, 0.02f * num, num * 2f);
			_ = EditorStyles.boldLabel;
			Color value = ADOSettings.Instance().generalColor.GetValue();
			Color color = Handles.color;
			Handles.color = value;
			AnimationCurve animationCurveValue = CS_0024_003C_003E8__locals28.m_ErrorAuthentication.animationCurveValue;
			List<List<ADOEditorUtility.BoneNode>> chains = result.chains;
			if (CS_0024_003C_003E8__locals28._ClassAuthentication.m_StrategyAuthentication == 1)
			{
				MapSingleton(result, animationCurveValue, delegate(ADOEditorUtility.BoneNode b, float m)
				{
					if (m != 0f)
					{
						Matrix4x4 matrix = b.matrix;
						Vector4 column = matrix.GetColumn(3);
						float comp2 = CS_0024_003C_003E8__locals28.m_TokenizerAuthentication.radius * m;
						EditorGUI.BeginChangeCheck();
						float num9 = ADOEditorUtility.RadiusHandle(matrix.rotation, column, comp2, !CS_0024_003C_003E8__locals28.m_TokenizerAuthentication.showGizmos, ADOSettings.Instance().handleSizeMultiplier);
						if (EditorGUI.EndChangeCheck())
						{
							float delta = num9 / m - CS_0024_003C_003E8__locals28.m_TokenizerAuthentication.radius;
							CS_0024_003C_003E8__locals28.CalculateServer(b, delta);
						}
						ADOEditorUtility.FindStatus(comp2.ToString("F2"), column);
					}
				}, isinstance2: true);
			}
			else
			{
				Vector3 vector = Vector3.zero;
				Vector3[][] array = new Vector3[chains.Count][];
				for (int num3 = 0; num3 < chains.Count; num3++)
				{
					List<ADOEditorUtility.BoneNode> list = chains[num3];
					array[num3] = new Vector3[list.Count];
					Vector3 vector2 = Vector3.zero;
					for (int num4 = 0; num4 < list.Count; num4++)
					{
						ADOEditorUtility.BoneNode boneNode = list[num4];
						Vector3 vector3 = ((num4 == 0) ? boneNode.GetPosition() : vector2);
						if (num4 != list.Count - 1)
						{
							vector2 = list[num4 + 1].GetPosition();
							vector = vector2 - vector3;
						}
						if (!(Vector3.Angle(Vector3.right, vector) >= 90f))
						{
							vector = -vector;
						}
						Vector3 up = Vector3.up;
						float num5 = boneNode.EvaluateCurve(animationCurveValue);
						float num6 = CS_0024_003C_003E8__locals28.valueAuthentication.floatValue * num5;
						Vector3 vector4 = vector3 + up * (num * (num6 / CS_0024_003C_003E8__locals28._TemplateAuthentication));
						array[num3][num4] = vector4;
						Handles.DrawDottedLine(vector3, vector4, 5f);
						ADOEditorUtility.FindStatus(num6.ToString("F2"), vector4, num2 + 0.01f);
						Vector3 vector5 = Handles.Slider(vector4, up, num2, Handles.DotHandleCap, 0f);
						if (!(vector4 == vector5))
						{
							float num7 = (vector5.y - vector4.y) / num * CS_0024_003C_003E8__locals28._TemplateAuthentication;
							if (num5 < 0f)
							{
								num7 *= -1f;
							}
							CS_0024_003C_003E8__locals28.CalculateServer(boneNode, num7);
						}
					}
				}
				Vector3[][] array2 = array;
				foreach (Vector3[] points in array2)
				{
					Handles.DrawAAPolyLine(3f * num, points);
				}
			}
			Handles.color = color;
		}

		private static void SetPropertyEditTarget(int end_v)
		{
			if (end_v < 0 || (isEditingProperty() && editedBindingIndex == end_v))
			{
				editedBindingIndex = -1;
				toolModes.SetSelected(6, ispred: false);
			}
			else
			{
				editedBindingIndex = end_v;
				toolModes.Select(6);
			}
			SceneView.RepaintAll();
		}

		internal static void ExitTool()
		{
			toolModes.Clear();
			editedBindingIndex = -1;
		}

		[MenuItem("CONTEXT/VRCPhysBone/[ADO] Toggle Editor", false, 899)]
		private static void SelectSingleton()
		{
			WriteSingleton(_CallbackIdentifier);
		}

		internal static void WriteSingleton(bool issetup = false)
		{
			if (m_TokenAuthentication == null)
			{
				m_TokenAuthentication = ADOEditorUtility.FindType("VRCPhysBone");
			}
			if (getterAuthentication == null)
			{
				getterAuthentication = ADOEditorUtility.FindType("VRCPhysBoneEditor");
			}
			_CallbackIdentifier = !issetup;
			ADOEditorUtility.OverrideCustomEditor(m_TokenAuthentication, (!_CallbackIdentifier) ? getterAuthentication : typeof(PhysBoneEditor));
		}

		private static void RefreshColliderStates()
		{
			membershipStates = new byte[sceneColliders.Length];
			bool flag = true;
			VRCPhysBone[] array = selectedPhysBones;
			foreach (VRCPhysBone vRCPhysBone in array)
			{
				for (int j = 0; j < membershipStates.Length; j++)
				{
					if (membershipStates[j] == 2)
					{
						continue;
					}
					List<VRCPhysBoneColliderBase> colliders = vRCPhysBone.colliders;
					if (colliders != null && colliders.Contains(sceneColliders[j]))
					{
						if (membershipStates[j] != 0 || flag)
						{
							membershipStates[j] = 1;
						}
						else
						{
							membershipStates[j] = 2;
						}
					}
					else if (membershipStates[j] == 1 && !flag)
					{
						membershipStates[j] = 2;
					}
					else
					{
						membershipStates[j] = 0;
					}
				}
				flag = false;
			}
		}

		private static void RefreshIgnoreTransformStates()
		{
			VRCPhysBone[] array;
			if (!isSelectingIgnoreTransforms())
			{
				bool flag = false;
				array = selectedPhysBones;
				foreach (VRCPhysBone vRCPhysBone in array)
				{
					for (int num = vRCPhysBone.ignoreTransforms.Count - 1; num >= 0; num--)
					{
						Transform _MerchantAuthentication = vRCPhysBone.ignoreTransforms[num];
						if (!(_MerchantAuthentication == null))
						{
							Transform transform = ((!vRCPhysBone.rootTransform) ? vRCPhysBone.transform : vRCPhysBone.rootTransform);
							if (_MerchantAuthentication == transform || !_MerchantAuthentication.IsChildOf(transform) || vRCPhysBone.ignoreTransforms.Any((Transform t2) => _MerchantAuthentication != t2 && (bool)t2 && _MerchantAuthentication.IsChildOf(t2)))
							{
								vRCPhysBone.ignoreTransforms.RemoveAt(num);
								flag = true;
							}
						}
						else
						{
							vRCPhysBone.ignoreTransforms.RemoveAt(num);
						}
					}
					EditorUtility.SetDirty(vRCPhysBone);
				}
				if (flag)
				{
					Log("Optimized ignore transforms.");
				}
				return;
			}
			membershipStates = new byte[candidateTransforms.Length];
			bool flag2 = true;
			array = selectedPhysBones;
			foreach (VRCPhysBone vRCPhysBone2 in array)
			{
				for (int num2 = 0; num2 < membershipStates.Length; num2++)
				{
					if (membershipStates[num2] == 2)
					{
						continue;
					}
					List<Transform> ignoreTransforms = vRCPhysBone2.ignoreTransforms;
					if (ignoreTransforms != null && ignoreTransforms.Contains(candidateTransforms[num2]))
					{
						if (membershipStates[num2] != 0 || flag2)
						{
							membershipStates[num2] = 1;
						}
						else
						{
							membershipStates[num2] = 2;
						}
					}
					else if (membershipStates[num2] == 1 && !flag2)
					{
						membershipStates[num2] = 2;
					}
					else
					{
						membershipStates[num2] = 0;
					}
				}
				flag2 = false;
			}
		}

		private static void CollectSingleton()
		{
			if (!threadAuthentication)
			{
				threadAuthentication = true;
				float[] array = new float[ADOEditorUtility.physBoneParameters.Count((ADOEditorUtility.PhysBoneParameter p) => p.hasBackingField)];
				for (int num = 0; num < array.Length; num++)
				{
					array[num] = 1f / (float)array.Length;
				}
				m_CodeIdentifier = GUILayoutUtils.SearchIterator(array);
			}
		}

		private void OnEnable()
		{
			m_ProcessorIdentifier = this;
			CollectSingleton();
			ResetFoldouts(_AnnotationIdentifier, Repaint);
			ApplyGlobalGizmoSettings();
			RefreshSceneAvatars(ref selectedAvatar, ref sceneAvatars);
			RefreshAvatarTables();
			Transform root = ((VRCPhysBone)TargetObject()).transform.root;
			selectedPhysBones = base.targets.Cast<VRCPhysBone>().ToArray();
			sceneColliders = root.GetComponentsInChildren<VRCPhysBoneCollider>();
			scenePhysBones = root.GetComponentsInChildren<VRCPhysBone>();
			candidateTransforms = selectedPhysBones.SelectMany((VRCPhysBone pb) => pb.GetRootTransform().GetComponentsInChildren<Transform>()).ToArray();
			SceneView.duringSceneGui -= VerifySingleton;
			SceneView.duringSceneGui += VerifySingleton;
		}

		private void OnDisable()
		{
			ExitTool();
			SceneView.duringSceneGui -= VerifySingleton;
			Tools.hidden = false;
		}

		private void CacheProperties()
		{
			_ValueIdentifier = base.serializedObject.FindProperty("version");
			_ErrorIdentifier = base.serializedObject.FindProperty("integrationType");
			producerIdentifier = base.serializedObject.FindProperty("rootTransform");
			m_TemplateIdentifier = base.serializedObject.FindProperty("ignoreTransforms");
			_WriterIdentifier = base.serializedObject.FindProperty("endpointPosition");
			classIdentifier = base.serializedObject.FindProperty("multiChildType");
			_DicIdentifier = base.serializedObject.FindProperty("pull");
			_ContainerIdentifier = base.serializedObject.FindProperty("pullCurve");
			m_SchemaIdentifier = base.serializedObject.FindProperty("spring");
			bridgeIdentifier = base.serializedObject.FindProperty("springCurve");
			publisherIdentifier = base.serializedObject.FindProperty("stiffness");
			_MerchantIdentifier = base.serializedObject.FindProperty("stiffnessCurve");
			m_ProcIdentifier = base.serializedObject.FindProperty("immobile");
			configurationAuthentication = base.serializedObject.FindProperty("immobileType");
			_IdentifierAuthentication = base.serializedObject.FindProperty("immobileCurve");
			m_AuthenticationAuthentication = base.serializedObject.FindProperty("gravity");
			contextAuthentication = base.serializedObject.FindProperty("gravityCurve");
			_SerializerAuthentication = base.serializedObject.FindProperty("gravityFalloff");
			m_MethodAuthentication = base.serializedObject.FindProperty("gravityFalloffCurve");
			consumerAuthentication = base.serializedObject.FindProperty("allowCollision");
			m_UtilsAuthentication = base.serializedObject.FindProperty("collisionFilter");
			_PageAuthentication = base.serializedObject.FindProperty("radius");
			propertyAuthentication = base.serializedObject.FindProperty("radiusCurve");
			m_SingletonAuthentication = base.serializedObject.FindProperty("colliders");
			_AccountAuthentication = base.serializedObject.FindProperty("limitType");
			m_ParamsAuthentication = base.serializedObject.FindProperty("maxAngleX");
			importerAuthentication = base.serializedObject.FindProperty("maxAngleXCurve");
			serverAuthentication = base.serializedObject.FindProperty("maxAngleZ");
			m_WatcherAuthentication = base.serializedObject.FindProperty("maxAngleZCurve");
			regAuthentication = base.serializedObject.FindProperty("limitRotation");
			processAuthentication = regAuthentication.FindPropertyRelative("x");
			statusAuthentication = regAuthentication.FindPropertyRelative("y");
			m_ValAuthentication = regAuthentication.FindPropertyRelative("z");
			adapterAuthentication = base.serializedObject.FindProperty("limitRotationXCurve");
			_ProxyAuthentication = base.serializedObject.FindProperty("limitRotationYCurve");
			m_RefAuthentication = base.serializedObject.FindProperty("limitRotationZCurve");
			comparatorAuthentication = base.serializedObject.FindProperty("allowGrabbing");
			iteratorAuthentication = base.serializedObject.FindProperty("allowPosing");
			m_PredicateAuthentication = base.serializedObject.FindProperty("poseFilter");
			_ProductAuthentication = base.serializedObject.FindProperty("grabFilter");
			_CollectionAuthentication = base.serializedObject.FindProperty("grabMovement");
			interceptorAuthentication = base.serializedObject.FindProperty("snapToHand");
			m_RegistryAuthentication = base.serializedObject.FindProperty("stretchMotion");
			_ClientAuthentication = base.serializedObject.FindProperty("stretchMotionCurve");
			m_ObserverAuthentication = base.serializedObject.FindProperty("maxStretch");
			broadcasterAuthentication = base.serializedObject.FindProperty("maxStretchCurve");
			m_EventAuthentication = base.serializedObject.FindProperty("maxSquish");
			m_RecordAuthentication = base.serializedObject.FindProperty("maxSquishCurve");
			resolverAuthentication = base.serializedObject.FindProperty("isAnimated");
			tagAuthentication = base.serializedObject.FindProperty("parameter");
			_FilterAuthentication = base.serializedObject.FindProperty("resetWhenDisabled");
			m_FactoryAuthentication = base.serializedObject.FindProperty("showGizmos");
			m_AttributeAuthentication = base.serializedObject.FindProperty("boneOpacity");
			m_InstanceAuthentication = base.serializedObject.FindProperty("limitOpacity");
			bindings = new AlgoAuthentication[15]
			{
				new AlgoAuthentication(_DicIdentifier, _ContainerIdentifier),
				new AlgoAuthentication(m_SchemaIdentifier, bridgeIdentifier),
				new AlgoAuthentication(publisherIdentifier, _MerchantIdentifier),
				new AlgoAuthentication(m_ProcIdentifier, _IdentifierAuthentication),
				new AlgoAuthentication(m_AuthenticationAuthentication, contextAuthentication, -1f),
				new AlgoAuthentication(_SerializerAuthentication, m_MethodAuthentication),
				new AlgoAuthentication(_PageAuthentication, propertyAuthentication, 0f, float.PositiveInfinity, 1),
				new AlgoAuthentication(m_ParamsAuthentication, importerAuthentication, 0f, 180f),
				new AlgoAuthentication(serverAuthentication, m_WatcherAuthentication, 0f, 90f),
				new AlgoAuthentication("Limit Rotation X", processAuthentication, adapterAuthentication, 0f, 360f),
				new AlgoAuthentication("Limit Rotation Y", statusAuthentication, _ProxyAuthentication, 0f, 360f),
				new AlgoAuthentication("Limit Rotation Z", m_ValAuthentication, m_RefAuthentication, 0f, 360f),
				new AlgoAuthentication(m_RegistryAuthentication, _ClientAuthentication),
				new AlgoAuthentication(m_ObserverAuthentication, broadcasterAuthentication, 0f, float.PositiveInfinity),
				new AlgoAuthentication(m_EventAuthentication, m_RecordAuthentication)
			};
			if (bindingLabelsBuilt)
			{
				return;
			}
			List<GUIContent> list = new List<GUIContent>();
			popupValueToBindingIndex = new Dictionary<int, int>();
			bindingIndexToPopupValue = new Dictionary<int, int>();
			int key = 0;
			for (int i = 0; i < bindings.Length; i++)
			{
				AlgoAuthentication algoAuthentication = bindings[i];
				if (algoAuthentication._GlobalAuthentication)
				{
					list.Add(new GUIContent(algoAuthentication.roleAuthentication));
					popupValueToBindingIndex.Add(key, i);
					bindingIndexToPopupValue.Add(i, key++);
				}
			}
			bindingLabels = list.ToArray();
			bindingPopupValues = popupValueToBindingIndex.Keys.ToArray();
			bindingLabelsBuilt = true;
		}

		internal static void ApplyGlobalGizmoSettings()
		{
			if ((bool)ADOSettings.Instance().globalGizmo)
			{
				VRCPhysBone[] array = UnityEngine.Object.FindObjectsOfType<VRCPhysBone>();
				foreach (VRCPhysBone obj in array)
				{
					obj.showGizmos = ADOSettings.Instance().gizmosActive;
					obj.boneOpacity = ADOSettings.Instance().gizmoBoneOpacity;
					obj.limitOpacity = ADOSettings.Instance().gizmoLimitOpacity;
				}
			}
		}

		private void ViewSingleton()
		{
			bool flag = _ErrorIdentifier.enumValueIndex == 1;
			int positionmap = (_ErrorIdentifier.hasMultipleDifferentValues ? 2 : _ErrorIdentifier.enumValueIndex);
			using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
			using (new GUIColorScope(GUIColorScope.ColoringType.BG, positionmap, ADOEditorUtility.MapRef().m_SerializerMethod))
			{
				flag = GUILayout.Toggle(flag, "Advanced", GUI.skin.button, GUILayout.ExpandWidth(expand: false));
			}
			if (changeCheckScope.changed)
			{
				_ErrorIdentifier.enumValueIndex = (flag ? 1 : 0);
			}
		}

		private static void PostSingleton(int ID_key, GUIContent pol = null)
		{
			AlgoAuthentication algoAuthentication = bindings[ID_key];
			SerializedProperty visitorAuthentication = algoAuthentication.m_VisitorAuthentication;
			SerializedProperty invocationAuthentication = algoAuthentication._InvocationAuthentication;
			using (new GUILayout.HorizontalScope())
			{
				if (pol != null)
				{
					EditorGUILayout.PropertyField(visitorAuthentication, pol);
				}
				else
				{
					EditorGUILayout.PropertyField(visitorAuthentication);
				}
				UpdateSingleton(invocationAuthentication, string.Empty);
				using (new GUIColorScope(GUIColorScope.ColoringType.BG, isEditingProperty() && activeBinding() == algoAuthentication, ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
				{
					if (ADOEditorUtility.CallStatus(ADOEditorUtility.CustomizeRef().stateSerializer, ADOEditorUtility.MapRef().methodMethod, GUILayout.ExpandWidth(expand: false)))
					{
						SetPropertyEditTarget(ID_key);
					}
				}
			}
		}

		private static void ListSingleton(int var1)
		{
			if (bindings[var1]._GlobalAuthentication)
			{
				PostSingleton(var1);
			}
		}

		private static bool ForgotSingleton(bool countparam, SerializedProperty visitor, SerializedProperty template)
		{
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(visitor);
				UpdateSingleton(template, string.Empty);
				return DrawIconToggle(countparam, ADOEditorUtility.CustomizeRef().stateSerializer);
			}
		}

		private static void UpdateSingleton(SerializedProperty i, string col, bool isproc = true)
		{
			if (!string.IsNullOrWhiteSpace(col))
			{
				GUILayout.Label(col, GUILayout.ExpandWidth(expand: false));
			}
			EditorGUILayout.CurveField(i, Color.cyan, new Rect(0f, 0f, 1f, 1f), GUIContent.none, GUILayout.MaxWidth(85f));
			if (i.animationCurveValue == null || i.animationCurveValue.length < 2)
			{
				GUI.Label(GUILayoutUtility.GetLastRect(), "///////////////////////////////", ADOEditorUtility.MapRef().m_ProcSerializer);
			}
			if (isproc && ADOEditorUtility.CallStatus(ADOEditorUtility.CustomizeRef().baseSerializer, GUI.skin.label, GUILayout.Width(14f)))
			{
				i.animationCurveValue = new AnimationCurve();
			}
		}

		[CompilerGenerated]
		internal static void ApplyEndpointOffset(Vector3 first, ref _003C_003Ec__DisplayClass116_0 vis)
		{
			Event current = Event.current;
			bool alt = current.alt;
			if (vis._MessageAuthentication.Length == 1)
			{
				OffsetEndpoint(vis._PolicyAuthentication, first);
			}
			else if (!alt)
			{
				if (current.shift)
				{
					Vector3 _MapperAuthentication = OffsetEndpoint(vis._PolicyAuthentication, first);
					VRCPhysBone[] array = vis._MessageAuthentication;
					foreach (VRCPhysBone vRCPhysBone in array)
					{
						if (vRCPhysBone != vis._PolicyAuthentication)
						{
							EditProperty(vRCPhysBone, _WriterIdentifier.propertyPath, delegate(SerializedProperty sp)
							{
								sp.vector3Value = _MapperAuthentication;
							});
						}
					}
				}
				else
				{
					VRCPhysBone[] array = vis._MessageAuthentication;
					for (int i = 0; i < array.Length; i++)
					{
						OffsetEndpoint(array[i], first);
					}
				}
			}
			else
			{
				OffsetEndpoint(vis._PolicyAuthentication, first);
			}
		}

		[CompilerGenerated]
		internal static Vector3 OffsetEndpoint(VRCPhysBone init, Vector3 map)
		{
			Vector3 m_ProcessorAuthentication = Vector3.zero;
			EditProperty(init, _WriterIdentifier.propertyPath, delegate(SerializedProperty sp)
			{
				sp.vector3Value += map;
				m_ProcessorAuthentication = sp.vector3Value;
			});
			return m_ProcessorAuthentication;
		}

		[CompilerGenerated]
		internal static void EditProperty(VRCPhysBone v, string counter, Action<SerializedProperty> dir)
		{
			SerializedObject obj = new SerializedObject(v);
			obj.UpdateIfRequiredOrScript();
			SerializedProperty obj2 = obj.FindProperty(counter);
			dir(obj2);
			obj.ApplyModifiedProperties();
		}

		UnityEngine.Object TargetObject()
		{
			return base.target;
		}
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec m_ProcAuthentication = new _003C_003Ec();

		public static Func<bool> configurationContext;

		public static Func<bool> identifierContext;

		public static Func<UnityEngine.Object, ADOEditorUtility.ShapeSnapshot> m_AuthenticationContext;

		public static Func<UnityEngine.Object, ADOEditorUtility.ShapeSnapshot> contextContext;

		public static Func<UnityEngine.Object, ADOEditorUtility.ShapeSnapshot> _SerializerContext;

		public static Func<bool> methodContext;

		public static Func<bool> consumerContext;

		public static Func<bool> utilsContext;

		public static Func<bool> m_PageContext;

		public static Func<bool> m_PropertyContext;

		public static Func<Rect> _SingletonContext;

		public static Action _AccountContext;

		public static Func<Transform, GameObject> m_ParamsContext;

		public static Func<GameObject, IEnumerable<VRCPhysBone>> _ImporterContext;

		public static Func<GameObject, IEnumerable<VRCPhysBoneColliderBase>> _ServerContext;

		public static Func<Transform, GameObject> _WatcherContext;

		public static Func<GameObject, bool> _RegContext;

		public static Func<bool> _ProcessContext;

		public static Func<VRCContactSender, IEnumerable<string>> statusContext;

		public static Func<VRCContactReceiver, IEnumerable<string>> valContext;

		public static Func<string, string> _AdapterContext;

		public static Func<VRCAvatarDescriptor.CustomAnimLayer, bool> m_ProxyContext;

		public static Func<VRCAvatarDescriptor.CustomAnimLayer, UnityEditor.Animations.AnimatorController> m_RefContext;

		public static Func<UnityEditor.Animations.AnimatorController, bool> m_ComparatorContext;

		public static Func<UnityEditor.Animations.AnimatorController, IEnumerable<UnityEngine.AnimatorControllerParameter>> m_ProductContext;

		public static Func<UnityEngine.AnimatorControllerParameter, string> _IteratorContext;

		public static Func<string, bool> predicateContext;

		public static Func<VRCAvatarDescriptor, bool> collectionContext;

		public static Func<VRCAvatarDescriptor, string> m_InterceptorContext;

		public static Func<UnityEngine.Object, bool> registryContext;

		public static Func<UnityEngine.Object, bool> m_ClientContext;

		public static Action _ObserverContext;

		public static Action m_BroadcasterContext;

		public static Func<bool> _EventContext;

		public static Action<Exception> _RecordContext;

		public static Action m_ResolverContext;

		public static Action<JsonObject> _TagContext;

		public static Action<Exception> filterContext;

		public static Func<ProcessRunner, bool> m_FactoryContext;

		public static Func<string, bool> m_AttributeContext;

		public static Func<(bool, string), bool> m_InstanceContext;

		public static Func<(bool, string), string> m_TaskContext;

		public static Func<(bool, string), bool> _CustomerContext;

		public static Func<(bool, string), string> _DatabaseContext;

		public static Func<bool> helperContext;

		public static Func<bool> _CandidateContext;

		public static Action<JsonObject> readerContext;

		public static Action<Exception> stubContext;

		public static Action _RulesContext;

		public static Action _TestsContext;

		public static Action<JsonObject> definitionContext;

		public static Action<Exception> _InitializerContext;

		public static Action _TokenContext;

		public static GenericMenu.MenuFunction m_GetterContext;

		public static GenericMenu.MenuFunction _ThreadContext;

		public static GenericMenu.MenuFunction algoContext;

		public static GenericMenu.MenuFunction m_RoleContext;

		public static GenericMenu.MenuFunction _VisitorContext;

		public static GenericMenu.MenuFunction invocationContext;

		public static GenericMenu.MenuFunction _ListenerContext;

		public static GenericMenu.MenuFunction _ParserContext;

		public static Action printerContext;

		public static Action<Exception> repositoryContext;

		public static Action descriptorContext;

		public static Func<Task> strategyContext;

		internal bool CollectServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool PrintServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal ADOEditorUtility.ShapeSnapshot InterruptServer(UnityEngine.Object t2)
		{
			return new ADOEditorUtility.ShapeSnapshot((VRCPhysBoneCollider)t2);
		}

		internal ADOEditorUtility.ShapeSnapshot ViewServer(UnityEngine.Object t2)
		{
			return new ADOEditorUtility.ShapeSnapshot((VRCContactSender)t2);
		}

		internal ADOEditorUtility.ShapeSnapshot PostServer(UnityEngine.Object t2)
		{
			return new ADOEditorUtility.ShapeSnapshot((VRCContactReceiver)t2);
		}

		internal bool ListServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool ForgotServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool UpdateServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool SearchServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool LoginServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal Rect PatchServer()
		{
			using (new GUILayout.HorizontalScope())
			{
				bool ignorecaller;
				string tooltip = ((!(ignorecaller = ADOSettings.Instance().hideToolsDuringTesting)) ? "Native tools are visible during test." : "Native tools are hidden during test.");
				using (new GUIColorScope(GUIColorScope.ColoringType.FG, ignorecaller, ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
				{
					if (ADOEditorUtility.IconButton(new GUIContent(ADOEditorUtility.CustomizeRef().prototypeSerializer)
					{
						tooltip = tooltip
					}))
					{
						ADOSettings.Instance().hideToolsDuringTesting.Toggle();
						Tools.hidden = false;
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.Label("Testing", ADOEditorUtility.MapRef().m_WriterSerializer);
				Rect lastRect = GUILayoutUtility.GetLastRect();
				GUILayout.FlexibleSpace();
				DrawSettingsButton();
				return lastRect;
			}
		}

		internal void CheckServer()
		{
			using (new GUIColorScope(GUIColorScope.ColoringType.BG, ADOEditorUtility.errorColor))
			{
				if (ADOEditorUtility.PatchStatus("Stop Testing") || ADOEditorUtility.CancelPressed() || ADOEditorUtility.SubmitPressed())
				{
					ToggleTestMode();
				}
			}
			using (new GUIColorScope(GUIColorScope.ColoringType.BG, ADOEditorUtility.secondaryActionColor))
			{
				if (ADOEditorUtility.PatchStatus("Restart"))
				{
					RestartTestMode();
				}
			}
			using (new GUIColorScope(GUIColorScope.ColoringType.BG, hasUnappliedTestChanges, ADOEditorUtility.validColor))
			{
				using (new EditorGUI.DisabledScope(!hasUnappliedTestChanges))
				{
					if (!ADOEditorUtility.PatchStatus("Apply All Changes"))
					{
						return;
					}
					foreach (UnityEngine.Object item in cloneHasUnappliedChanges.Keys.ToList())
					{
						if (cloneHasUnappliedChanges[item])
						{
							UnityEngine.Object obj = cloneToOriginal[item];
							if (obj != null)
							{
								Undo.RecordObject(obj, "ADO - Apply Changes");
								EditorUtility.CopySerialized(item, obj);
								cloneHasUnappliedChanges[item] = false;
							}
						}
					}
					hasUnappliedTestChanges = false;
					InsertConfiguration();
				}
			}
		}

		internal GameObject CallServer(Transform t)
		{
			return t.root.gameObject;
		}

		internal IEnumerable<VRCPhysBone> RegisterServer(GameObject o)
		{
			return o.GetComponentsInChildren<VRCPhysBone>(includeInactive: true);
		}

		internal IEnumerable<VRCPhysBoneColliderBase> ChangeServer(GameObject o)
		{
			return o.GetComponentsInChildren<VRCPhysBoneColliderBase>(includeInactive: true);
		}

		internal GameObject StopServer(Transform t)
		{
			return t.gameObject;
		}

		internal bool PushServer(GameObject o)
		{
			return o;
		}

		internal bool PrepareServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal IEnumerable<string> ReadServer(VRCContactSender cs)
		{
			return cs.collisionTags;
		}

		internal IEnumerable<string> TestServer(VRCContactReceiver cr)
		{
			return cr.collisionTags;
		}

		internal string InsertServer(string s)
		{
			return "Default/" + s;
		}

		internal bool EnableServer(VRCAvatarDescriptor.CustomAnimLayer rc)
		{
			if (!rc.isDefault)
			{
				return rc.animatorController;
			}
			return false;
		}

		internal UnityEditor.Animations.AnimatorController AwakeServer(VRCAvatarDescriptor.CustomAnimLayer rc)
		{
			return AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(AssetDatabase.GetAssetPath(rc.animatorController));
		}

		internal bool DisableServer(UnityEditor.Animations.AnimatorController c)
		{
			return c;
		}

		internal IEnumerable<UnityEngine.AnimatorControllerParameter> VisitServer(UnityEditor.Animations.AnimatorController c)
		{
			return c.parameters;
		}

		internal string AssetServer(UnityEngine.AnimatorControllerParameter p)
		{
			return p.name;
		}

		internal bool PopServer(string p)
		{
			return !ADOEditorUtility.reservedAvatarParameters.Contains(p);
		}

		internal bool InstantiateServer(VRCAvatarDescriptor a)
		{
			return (UnityEngine.Object)(object)a;
		}

		internal string RestartServer(VRCAvatarDescriptor x)
		{
			return ((UnityEngine.Object)(object)x).name;
		}

		internal bool ManageServer(UnityEngine.Object b)
		{
			if (!(b != null) || !cloneHasUnappliedChanges.ContainsKey(b))
			{
				return false;
			}
			return cloneToOriginal[b] != null;
		}

		internal bool RateServer(UnityEngine.Object b)
		{
			return cloneHasUnappliedChanges[b];
		}

		internal void CloneServer()
		{
			isSendingFeedback = false;
			feedbackPanelOpen = false;
			RepaintOpenWindowsDelayed();
		}

		internal void ComputeServer()
		{
			AssetConfiguration(testkey: false);
		}

		internal bool QueryServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal void CountServer(Exception exception)
		{
			isVerifyingLicense = false;
			isLicensed = false;
			licenseCheckRetryOffered = true;
			Log($"Something went wrong while verifying license:\n\n{exception}", CustomLogType.Error);
		}

		internal void StartServer(JsonObject response)
		{
			isActivatingLicense = false;
			QueryConfiguration(response, delegate
			{
				licenseKeyEntryRequired = false;
				ADOSettings.Instance().a_HasSucceededLastVerification.SetValue(nores: true);
				AssetConfiguration(testkey: true);
			});
		}

		internal void RemoveServer()
		{
			licenseKeyEntryRequired = false;
			ADOSettings.Instance().a_HasSucceededLastVerification.SetValue(nores: true);
			AssetConfiguration(testkey: true);
		}

		internal void ReflectServer(Exception exception)
		{
			isActivatingLicense = false;
			Log($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
		}

		internal bool ResolveServer(ProcessRunner p)
		{
			return p.isFinished;
		}

		internal bool ResetServer(string v)
		{
			return !string.IsNullOrWhiteSpace(v);
		}

		internal bool GetServer((bool, string) i)
		{
			return !i.Item1;
		}

		internal string FlushServer((bool, string) i)
		{
			return i.Item2;
		}

		internal bool ExcludeServer((bool, string) i)
		{
			return !i.Item1;
		}

		internal string InitServer((bool, string) i)
		{
			return i.Item2;
		}

		internal bool ConnectServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal bool FindServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		}

		internal void AddServer()
		{
			List<(string, string)> list = CountConfiguration("transferlicenserequest");
			StartConfiguration(list);
			OrderIdentifier(IncludeConfiguration(list.ToArray())).HandleTask(delegate(JsonObject response)
			{
				_003C_003Ec__DisplayClass179_0 _003C_003Ec__DisplayClass179_ = new _003C_003Ec__DisplayClass179_0
				{
					composerContext = response
				};
				isRequestingTransferCode = false;
				QueryConfiguration(_003C_003Ec__DisplayClass179_.composerContext, _003C_003Ec__DisplayClass179_.RestartReg);
			}, delegate(Exception exception)
			{
				isRequestingTransferCode = false;
				Log($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, RepaintOpenWindowsDelayed);
		}

		internal void ValidateServer(JsonObject response)
		{
			_003C_003Ec__DisplayClass179_0 _003C_003Ec__DisplayClass179_ = new _003C_003Ec__DisplayClass179_0
			{
				composerContext = response
			};
			isRequestingTransferCode = false;
			QueryConfiguration(_003C_003Ec__DisplayClass179_.composerContext, _003C_003Ec__DisplayClass179_.RestartReg);
		}

		internal void CreateServer(Exception exception)
		{
			isRequestingTransferCode = false;
			Log($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
		}

		internal void IncludeServer()
		{
			List<(string, string)> list = CountConfiguration("transferlicenseconfirm");
			list.Add(("verification_code", transferVerificationCode));
			StartConfiguration(list);
			OrderIdentifier(IncludeConfiguration(list.ToArray())).HandleTask(delegate(JsonObject response)
			{
				isConfirmingTransfer = false;
				QueryConfiguration(response, delegate
				{
					showingTransferPanel = false;
					transferCodeSent = false;
					licenseKeyEntryRequired = false;
					AssetConfiguration(testkey: true);
				});
			}, delegate(Exception exception)
			{
				isConfirmingTransfer = false;
				Log($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, RepaintOpenWindowsDelayed);
		}

		internal void RevertServer(JsonObject response)
		{
			isConfirmingTransfer = false;
			QueryConfiguration(response, delegate
			{
				showingTransferPanel = false;
				transferCodeSent = false;
				licenseKeyEntryRequired = false;
				AssetConfiguration(testkey: true);
			});
		}

		internal void RunWatcher()
		{
			showingTransferPanel = false;
			transferCodeSent = false;
			licenseKeyEntryRequired = false;
			AssetConfiguration(testkey: true);
		}

		internal void OrderWatcher(Exception exception)
		{
			isConfirmingTransfer = false;
			Log($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
		}

		internal void CalculateWatcher()
		{
			SessionState.EraseString("No1lKII9IzcBAbihub6nCg==updateinfo");
			FillIdentifier();
		}

		internal void CalcWatcher()
		{
			feedbackPanelOpen.Toggle();
		}

		internal void DeleteWatcher()
		{
			ADOSettings.Instance().a_VerifyOnDisplay.Toggle();
			ADOSettings.Instance().a_VerifyOnProjectLoad.SetValue(nores: false);
		}

		internal void DefineWatcher()
		{
			ADOSettings.Instance().a_VerifyOnProjectLoad.Toggle();
			ADOSettings.Instance().a_VerifyOnDisplay.SetValue(nores: false);
		}

		internal void DestroyWatcher()
		{
			Application.OpenURL("");
		}

		internal void NewWatcher()
		{
			Application.OpenURL(extraMenuLinks[0].Item2);
		}

		internal void CompareWatcher()
		{
			Application.OpenURL("");
		}

		internal void VerifyWatcher()
		{
			Application.OpenURL("https://dreadrith.com/license-tos");
		}

		internal void SetWatcher()
		{
			CancelIdentifier(isparam: false);
		}

		internal void SortWatcher(Exception exc)
		{
			Log($"Something went wrong while checking for an update!\n\n{exc}", CustomLogType.Error);
		}

		internal void InvokeWatcher()
		{
			isCheckingForUpdate = false;
			RepaintOpenWindowsDelayed();
		}

		internal async Task CustomizeWatcher()
		{
			await Task.Delay(3000);
			ADOSettings.Instance().u_updateHidden.SetValue(nores: true);
			RepaintOpenWindowsDelayed();
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass132_0
	{
		public string itemContext;

		internal void EnableWatcher()
		{
			bool wasLicensedBeforeReset = ADOverhaul.wasLicensedBeforeReset;
			isLicensed = false;
			ADOverhaul.wasLicensedBeforeReset = false;
			licenseToken = (licenseUsername = (licenseVariant = string.Empty));
			ADOSettings.Instance().a_HasSucceededLastVerification.SetValue(nores: false);
			SessionState.EraseBool(itemContext);
			ResetConfiguration(wasLicensedBeforeReset);
		}

		internal string AwakeWatcher(string key, ref _003C_003Ec__DisplayClass132_1 _003C_003Ec__DisplayClass132_1_0, ref _003C_003Ec__DisplayClass132_2 _003C_003Ec__DisplayClass132_2_0)
		{
			return ListIdentifier(SessionState.GetString(ForgotIdentifier(itemContext + key, ref _003C_003Ec__DisplayClass132_2_0), string.Empty), ref _003C_003Ec__DisplayClass132_1_0);
		}

		internal void DisableWatcher()
		{
			List<(string, string)> list = CountConfiguration("verifylicense");
			StartConfiguration(list);
			OrderIdentifier(IncludeConfiguration(list.ToArray())).HandleTask(delegate(JsonObject response)
			{
				_003C_003Ec__DisplayClass132_3 _003C_003Ec__DisplayClass132_ = new _003C_003Ec__DisplayClass132_3
				{
					m_SetterContext = this,
					_SystemContext = response
				};
				isVerifyingLicense = false;
				licenseKeyEntryRequired = true;
				QueryConfiguration(_003C_003Ec__DisplayClass132_._SystemContext, _003C_003Ec__DisplayClass132_.RateWatcher, delegate
				{
					bool wasLicensedBeforeReset = ADOverhaul.wasLicensedBeforeReset;
					isLicensed = false;
					ADOverhaul.wasLicensedBeforeReset = false;
					licenseToken = (licenseUsername = (licenseVariant = string.Empty));
					ADOSettings.Instance().a_HasSucceededLastVerification.SetValue(nores: false);
					SessionState.EraseBool(itemContext);
					ResetConfiguration(wasLicensedBeforeReset);
				}, comparesecond2: false);
			}, _003C_003Ec.m_ProcAuthentication.CountServer, null, null, RepaintOpenWindowsDelayed);
		}

		internal void VisitWatcher(JsonObject response)
		{
			_003C_003Ec__DisplayClass132_3 _003C_003Ec__DisplayClass132_ = new _003C_003Ec__DisplayClass132_3
			{
				m_SetterContext = this,
				_SystemContext = response
			};
			isVerifyingLicense = false;
			licenseKeyEntryRequired = true;
			QueryConfiguration(_003C_003Ec__DisplayClass132_._SystemContext, _003C_003Ec__DisplayClass132_.RateWatcher, delegate
			{
				bool wasLicensedBeforeReset = ADOverhaul.wasLicensedBeforeReset;
				isLicensed = false;
				ADOverhaul.wasLicensedBeforeReset = false;
				licenseToken = (licenseUsername = (licenseVariant = string.Empty));
				ADOSettings.Instance().a_HasSucceededLastVerification.SetValue(nores: false);
				SessionState.EraseBool(itemContext);
				ResetConfiguration(wasLicensedBeforeReset);
			}, comparesecond2: false);
		}

		internal void AssetWatcher(string key, string value, ref _003C_003Ec__DisplayClass132_4 _003C_003Ec__DisplayClass132_4_0, ref _003C_003Ec__DisplayClass132_5 _003C_003Ec__DisplayClass132_5_0)
		{
			SessionState.SetString(UpdateIdentifier(itemContext + key, ref _003C_003Ec__DisplayClass132_5_0), SearchIdentifier(value, ref _003C_003Ec__DisplayClass132_4_0));
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass132_1
	{
		public AesManaged _IndexerContext;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass132_2
	{
		public HMACSHA1 m_PoolContext;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass132_3
	{
		public JsonObject _SystemContext;

		public _003C_003Ec__DisplayClass132_0 m_SetterContext;

		internal void RateWatcher()
		{
			try
			{
				string text = _SystemContext.Item("date");
				if (RemoveConfiguration() != text)
				{
					Log("Date Mismatch! Please make sure your system's date is correct.\nLocal: " + currentDateStamp + "  |  Remote: " + text, CustomLogType.Error);
					licenseCheckRetryOffered = true;
					m_SetterContext.EnableWatcher();
					return;
				}
				licenseUsername = _SystemContext.Item("username");
				licenseVariant = _SystemContext.Item("variant");
				licenseToken = _SystemContext.Item("token");
				InstantiateConfiguration();
				RestartConfiguration();
				string def = _SystemContext.Item("message");
				if (!wasLicensedBeforeReset)
				{
					Log(def);
				}
				isLicensed = true;
				ADOSettings.Instance().a_HasSucceededLastVerification.SetValue(nores: true);
				EditorPrefs.SetString("No1lKII9IzcBAbihub6nCg==LK", licenseKey);
				_003C_003Ec__DisplayClass132_4 _003C_003Ec__DisplayClass132_4_ = default(_003C_003Ec__DisplayClass132_4);
				_003C_003Ec__DisplayClass132_4_.ruleContext = new AesManaged();
				try
				{
					_003C_003Ec__DisplayClass132_4_.ruleContext.Key = Convert.FromBase64String("LWw2tFi+lgG6KK4+nMum8RuWZMIOhu1urChsHMbizPM=");
					_003C_003Ec__DisplayClass132_4_.ruleContext.IV = Convert.FromBase64String("MEZqk6gCgPTwifeH3YrTlQ==");
					_003C_003Ec__DisplayClass132_5 _003C_003Ec__DisplayClass132_5_ = default(_003C_003Ec__DisplayClass132_5);
					_003C_003Ec__DisplayClass132_5_._StructContext = new HMACSHA1(Encoding.UTF8.GetBytes(m_SetterContext.itemContext));
					try
					{
						m_SetterContext.AssetWatcher("date", currentDateStamp, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
						m_SetterContext.AssetWatcher("u", licenseUsername, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
						m_SetterContext.AssetWatcher("v", licenseVariant, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
						m_SetterContext.AssetWatcher("r", licenseToken, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
						m_SetterContext.AssetWatcher("m", hardwareId, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
					}
					finally
					{
						if (_003C_003Ec__DisplayClass132_5_._StructContext != null)
						{
							((IDisposable)_003C_003Ec__DisplayClass132_5_._StructContext).Dispose();
						}
					}
				}
				finally
				{
					if (_003C_003Ec__DisplayClass132_4_.ruleContext != null)
					{
						((IDisposable)_003C_003Ec__DisplayClass132_4_.ruleContext).Dispose();
					}
				}
				SessionState.SetBool(m_SetterContext.itemContext, value: true);
				if (!new Func<bool>(_003C_003Ec.m_ProcAuthentication.QueryServer)())
				{
					m_SetterContext.EnableWatcher();
				}
				ResolveConfiguration(istask: false);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass132_4
	{
		public AesManaged ruleContext;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass132_5
	{
		public HMACSHA1 _StructContext;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass138_0
	{
		public bool m_InterpreterContext;

		public string _ParameterContext;

		public StringBuilder attrContext;

		public string[] m_ObjectContext;

		public string[] m_ServiceContext;

		public string[] reponseContext;

		public string[][] _SpecificationContext;

		public StringBuilder _WrapperContext;

		public CancellationTokenSource _InfoContext;

		public ProcessRunner[] modelContext;

		public bool _ConfigContext;

		public Action m_MockContext;

		internal string ConnectWatcher(string property, string[] extractedValues)
		{
			string text = extractedValues.FirstOrDefault(_003C_003Ec.m_ProcAuthentication.ResetServer);
			if (!m_InterpreterContext)
			{
				_003C_003Ec__DisplayClass138_3 _003C_003Ec__DisplayClass138_ = new _003C_003Ec__DisplayClass138_3();
				text = ((!InsertIdentifier(_ParameterContext, property, out _003C_003Ec__DisplayClass138_.m_AdvisorContext)) ? text : (extractedValues.FirstOrDefault(_003C_003Ec__DisplayClass138_.StopReg) ?? text));
			}
			attrContext.AppendLine(property + ": " + text);
			return text;
		}

		internal void FindWatcher(string o)
		{
			m_ObjectContext[0] = o;
		}

		internal void AddWatcher(string o)
		{
			m_ObjectContext[1] = o;
		}

		internal void ValidateWatcher(string o)
		{
			m_ObjectContext[2] = o;
		}

		internal void CreateWatcher(string o)
		{
			m_ObjectContext[3] = o;
		}

		internal void IncludeWatcher(string o)
		{
			m_ServiceContext[0] = o;
		}

		internal void RevertWatcher(string o)
		{
			m_ServiceContext[1] = o;
		}

		internal void RunReg(string o)
		{
			m_ServiceContext[2] = o;
		}

		internal void OrderReg(string o)
		{
			m_ServiceContext[3] = o;
		}

		internal bool CalculateReg((List<string>, Dictionary<string, RangeInt>) cmdParsedOutput, string property, out string result)
		{
			if (TestIdentifier(cmdParsedOutput, property, out var rule))
			{
				result = ConnectWatcher(property, rule);
				return true;
			}
			result = "Default String";
			return false;
		}

		internal bool CalcReg(string fullInfo, out string result, string[] properties)
		{
			result = string.Empty;
			if (!ReadIdentifier(fullInfo, properties[0], out var template))
			{
				return false;
			}
			(bool, string)[] array = new(bool, string)[properties.Length];
			for (int i = 0; i < properties.Length; i++)
			{
				string result2;
				bool item = CalculateReg(template, properties[i], out result2);
				array[i] = (item, result2);
			}
			int num = Mathf.CeilToInt((float)array.Length / 2f);
			if (array.Count(_003C_003Ec.m_ProcAuthentication.GetServer) >= num)
			{
				return false;
			}
			result = string.Join(string.Empty, array.Select(_003C_003Ec.m_ProcAuthentication.FlushServer)).Replace(" ", string.Empty);
			return true;
		}

		internal void DeleteReg()
		{
			try
			{
				CompareReg(isCMD: true);
				SetReg();
			}
			catch (Exception exc)
			{
				VerifyReg(isCMD: true, exc);
			}
		}

		internal bool DefineReg(string fullInfo, string property, out string result)
		{
			if (!InsertIdentifier(fullInfo, property, out var filter))
			{
				result = "Default String";
				return false;
			}
			result = ConnectWatcher(property, filter);
			return true;
		}

		internal bool DestroyReg(string fullInfo, out string result, string[] properties)
		{
			result = string.Empty;
			(bool, string)[] array = new(bool, string)[properties.Length];
			for (int i = 0; i < properties.Length; i++)
			{
				string result2;
				bool item = DefineReg(fullInfo, properties[i], out result2);
				array[i] = (item, result2);
			}
			if (array.All(_003C_003Ec.m_ProcAuthentication.ExcludeServer))
			{
				return false;
			}
			result = string.Join(string.Empty, array.Select(_003C_003Ec.m_ProcAuthentication.InitServer)).Replace(" ", string.Empty);
			return true;
		}

		internal void NewReg()
		{
			try
			{
				CompareReg(isCMD: false);
				SetReg();
			}
			catch (Exception exc)
			{
				VerifyReg(isCMD: false, exc);
			}
		}

		internal void CompareReg(bool isCMD)
		{
			bool[] array = new bool[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = ((!isCMD) ? DestroyReg(m_ServiceContext[i], out reponseContext[i], _SpecificationContext[i]) : CalcReg(m_ObjectContext[i], out reponseContext[i], _SpecificationContext[i]));
			}
			bool num = array[0] || array[1];
			bool flag = num;
			if ((!num || !array[2]) && (!flag || !array[3]) && (!array[2] || !array[3]))
			{
				throw new Exception("Failed to gather hardware info through " + ((!isCMD) ? "Shell" : "CMD"));
			}
		}

		internal void VerifyReg(bool isCMD, Exception exc)
		{
			if (!isCMD)
			{
				isConfirmingTransfer = false;
				isRequestingTransferCode = false;
				isVerifyingLicense = false;
				isActivatingLicense = false;
			}
			string text = (isCMD ? "CMD" : "Shell");
			_WrapperContext.AppendLine("Failed " + text + " Parse");
			_WrapperContext.AppendLine("Reason: " + exc.Message);
			_WrapperContext.AppendLine(exc.StackTrace + Environment.NewLine);
			string[] array = ((!isCMD) ? m_ServiceContext : m_ObjectContext);
			for (int i = 0; i < 4; i++)
			{
				_WrapperContext.AppendLine($"Info {i}:");
				try
				{
					_WrapperContext.AppendLine(array[i]);
				}
				catch
				{
					_WrapperContext.AppendLine($"Missing Info {i}!");
				}
			}
			if (!isCMD)
			{
				int num = EditorUtility.DisplayDialogComplex("Error!", "Generating HWID failed and cannot proceed!\nPlease try the 'Troubleshoot' instructions.\nIf troubleshooting didn't work, press 'Report'.", "Troubleshoot", "Close", "Report");
				if (num != 0)
				{
					if (num == 2)
					{
						string text2 = "HWIDInfo";
						string text3 = "Assets/" + text2;
						if (EditorUtility.DisplayDialog("Reporting", "Pressing the 'Proceed' button below will generate the file '" + text2 + "' in Assets with your Hardware Information for debugging purposes. Please send that file to @Dreadrith#3238", "Ok", "Cancel"))
						{
							File.WriteAllText(text3, StopIdentifier(_WrapperContext.ToString()));
							AssetDatabase.ImportAsset(text3);
							EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(text3));
						}
					}
				}
				else
				{
					Application.OpenURL("https://dreadrith.com/HWIDHelp");
				}
				isVerifyingLicense = false;
				isActivatingLicense = false;
				RepaintOpenWindowsDelayed();
				return;
			}
			_InfoContext = new CancellationTokenSource();
			_InfoContext.CancelAfter(10000);
			PrepareIdentifier(modelContext, delegate
			{
				try
				{
					CompareReg(isCMD: false);
					SetReg();
				}
				catch (Exception exc2)
				{
					VerifyReg(isCMD: false, exc2);
				}
			}, _InfoContext);
		}

		internal void SetReg()
		{
			EditorPrefs.SetString("DSLICINF", StopIdentifier(attrContext.ToString()));
			if (_ConfigContext)
			{
				for (int i = 0; i < 4; i++)
				{
					reponseContext[i] += "\r\r";
				}
			}
			string[] array = new string[3]
			{
				reponseContext[0] + reponseContext[1],
				reponseContext[2],
				reponseContext[3]
			};
			using (SHA1 sHA = SHA1.Create())
			{
				for (int j = 0; j < 3; j++)
				{
					array[j] = BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(array[j]))).Replace("-", "");
				}
			}
			hardwareId = string.Join("-", array);
			RestartConfiguration();
			m_MockContext();
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass138_1
	{
		public AesManaged _StateContext;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass138_3
	{
		public string[] m_AdvisorContext;

		internal bool StopReg(string v)
		{
			return v == m_AdvisorContext[0];
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass179_0
	{
		public JsonObject composerContext;

		internal void RestartReg()
		{
			transferTargetEmail = composerContext.Item("transfer_email");
			transferCodeSent = true;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass46_0
	{
		public bool m_MappingContext;

		public UnityEngine.Object queueContext;

		public UnityEngine.Object[] m_ProcessorContext;

		public ADOEditorUtility.ShapeSnapshot[] m_TokenizerContext;

		public int _ExceptionContext;

		public int m_ValueContext;

		public Vector3 m_ErrorContext;

		public float _ProducerContext;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass46_1
	{
		public float _TemplateContext;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass46_2
	{
		public float m_WriterContext;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass46_3
	{
		public Vector3 m_ClassContext;

		public Vector3 _DicContext;

		public Vector3 m_ContainerContext;

		public Vector3 schemaContext;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass54_0
	{
		public List<Transform> bridgeContext;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass66_0
	{
		public FieldInfo publisherContext;

		internal async void CancelProcess()
		{
			try
			{
				int num = 0;
				bool flag2;
				while (true)
				{
					bool num2 = (bool)publisherContext.GetValue(null);
					bool flag = num2;
					flag2 = num2;
					if (flag)
					{
						break;
					}
					await Task.Delay(200);
					num++;
					if (num > 30)
					{
						UnityEngine.Debug.LogError("Failed to apply ADO's custom editors automatically.");
						break;
					}
				}
				if (flag2)
				{
					PhysBoneEditor.WriteSingleton();
					PhysBoneColliderEditor.InsertProperty();
					ContactSenderEditor.InvokeProperty();
					ContactReceiverEditor.ReadPage();
				}
			}
			catch (Exception message)
			{
				UnityEngine.Debug.LogError(message);
			}
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass86_0
	{
		public SerializedProperty serializerSerializer;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass86_1
	{
		public UnityEditor.Animations.AnimatorController _MethodSerializer;
	}

	private static bool physBoneReflectionResolved;

	private static MethodInfo physBoneManagerLateUpdate;

	private static MethodInfo physBoneManagerOnDestroy;

	private static MethodInfo physBoneStart;

	private static MethodInfo physBoneOnEnable;

	private static MethodInfo physBoneOnDisable;

	private static MethodInfo physBoneColliderStart;

	private static MethodInfo physBoneColliderOnEnable;

	private static MethodInfo physBoneColliderOnDisable;

	private static bool isTesting;

	private static PhysBoneManager testPhysBoneManager;

	private static GameObject testRoot;

	private static GameObject[] testSourceRoots;

	private static GameObject[] selectedObjectsBeforeTest;

	private static GameObject activeObjectBeforeTest;

	private static VRCPhysBone[] testPhysBones;

	private static bool[] testPhysBoneEnabled;

	private static bool[] testPhysBoneStarted;

	private static VRCPhysBoneCollider[] testColliders;

	private static bool[] testColliderEnabled;

	private static bool[] testColliderStarted;

	private static readonly int handleControlIdBase = "ADOControlID".GetHashCode();

	private static VRCAvatarDescriptor selectedAvatar;

	private static VRCAvatarDescriptor[] sceneAvatars;

	private static string[] avatarParameterNames;

	private static string[] avatarCollisionTags;

	private static string[] avatarPlayableLayerNames;

	private static int[] avatarPlayableLayerTypes;

	private static bool @event;

	private static bool editingRadius;

	private static bool editingHeight;

	private static bool editingPosition;

	private static bool editingRotation;

	private static bool shapeHasRadius;

	private static bool shapeHasHeight;

	private static bool shapeHasRotation;

	private static readonly ADOEditorUtility.ResizeHandle sceneViewPanelResizeHandle = new ADOEditorUtility.ResizeHandle();

	private static readonly int tooltipDragControlId = GUIUtility.GetControlID("ADOTooltipDragControlID".GetHashCode(), FocusType.Passive);

	private static Dictionary<UnityEngine.Object, UnityEngine.Object> originalToClone = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

	private static Dictionary<UnityEngine.Object, UnityEngine.Object> cloneToOriginal = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

	private static Dictionary<UnityEngine.Object, bool> cloneHasUnappliedChanges = new Dictionary<UnityEngine.Object, bool>();

	private static bool hasUnappliedTestChanges;

	private static bool colliderChangedDuringTest;

	private static bool hasShownColliderRestartPrompt;

	private static bool bugReporterOpen;

	private static bool isSendingBugReport;

	private static string bugReportText;

	private static bool feedbackPanelOpen;

	private static bool isSendingFeedback;

	private static string feedbackText;

	private static string licenseUsername;

	private static string licensedToDisplayName;

	private static string licenseVariant;

	private static string licenseKey = "";

	private static string transferVerificationCode = "";

	private static string transferTargetEmail = "";

	private static string sessionId;

	private static bool serverWarnedTooManyAttempts;

	private static bool licenseKeyEntryRequired;

	private static bool licenseCheckRetryOffered;

	private static bool licenseCheckedThisSession;

	private static float retryAllowedAtRealtime;

	private static string currentDateStamp;

	private static bool isActivatingLicense;

	private static bool isVerifyingLicense;

	private static string unreadDeviceDateFingerprint;

	private static string hardwareId;

	private static string licenseToken;

	private static bool isLicensed;

	private static bool wasLicensedBeforeReset;

	private static bool licensedCallbacksFlushed;

	private static Action pendingLicensedCallbacks;

	private static bool showingTransferPanel;

	private static bool transferCodeSent;

	private static bool isRequestingTransferCode;

	private static bool isConfirmingTransfer;

	private static bool isDownloadingUpdate;

	private static bool isCheckingForUpdate;

	private static bool hasCheckedForUpdate;

	private static bool updateAvailable;

	private static readonly AnimBool updateFoldout = new AnimBool();

	private static readonly AnimBool announcementFoldout = new AnimBool();

	private static readonly SemVer version = new SemVer("0.11.1");

	private static readonly (string, string)[] extraMenuLinks = new(string, string)[0];

	private static void DrawShapeProperties(UnityEngine.Object res, SerializedProperty[] visitor, Action tag, bool isres2)
	{
		SerializedProperty serializedProperty = visitor[0];
		SerializedProperty property = visitor[1];
		SerializedProperty serializedProperty2 = visitor[5];
		SerializedProperty serializedProperty3 = (isres2 ? visitor[6] : null);
		SerializedProperty spec = (isres2 ? visitor[7] : null);
		int intValue = serializedProperty.intValue;
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			return;
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedProperty, new GUIContent("Type"));
			if (EditorGUI.EndChangeCheck())
			{
				if (intValue == 0)
				{
					editingRotation = false;
					editingHeight = false;
				}
				else if (intValue == 2)
				{
					editingHeight = false;
					editingRadius = false;
				}
				tag();
			}
			if (isres2 && serializedProperty3 != null)
			{
				using (new GUIColorScope(GUIColorScope.ColoringType.BG, serializedProperty3.boolValue, ADOEditorUtility.highlightColor, ADOEditorUtility.validColor))
				{
					serializedProperty3.boolValue = ADOEditorUtility.ChangeStatus(serializedProperty3.boolValue, (!serializedProperty3.boolValue) ? "Outside Bounds" : "Inside Bounds", GUI.skin.button, GUILayout.ExpandWidth(expand: false));
				}
			}
		}
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			return;
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(property, new GUIContent("Root"));
			if (GUILayout.Button(new GUIContent("S", "Set to Self"), GUILayout.Width(18f), GUILayout.Height(18f)))
			{
				Undo.RecordObject(res, "Set Root to Self");
				UnityEngine.Component component = res as UnityEngine.Component;
				if ((bool)component)
				{
					SerializedObject serializedObject = new SerializedObject(component);
					serializedObject.FindProperty("rootTransform").objectReferenceValue = component.transform;
					serializedObject.ApplyModifiedProperties();
				}
			}
		}
		EditorGUILayout.Space();
		InvokeConfiguration(visitor, 0);
		InvokeConfiguration(visitor, 1);
		InvokeConfiguration(visitor, 2);
		if (serializedProperty.enumValueIndex != 0)
		{
			using (new GUILayout.HorizontalScope())
			{
				using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
				{
					Vector3 eulerAngles = serializedProperty2.quaternionValue.eulerAngles;
					eulerAngles = EditorGUILayout.Vector3Field(new GUIContent("Rotation", "Rotation offset from the root transform"), eulerAngles);
					if (changeCheckScope.changed)
					{
						serializedProperty2.quaternionValue = Quaternion.Euler(eulerAngles);
					}
				}
				using (new GUIColorScope(GUIColorScope.ColoringType.BG, editingRotation, Color.green, Color.red))
				{
					editingRotation = GUILayout.Toggle(editingRotation, ADOEditorUtility.CustomizeRef().stateSerializer, ADOEditorUtility.MapRef().methodMethod, GUILayout.Width(18f), GUILayout.Height(18f));
				}
			}
		}
		if (isres2)
		{
			DrawOptionalProperty(spec);
		}
	}

	private static void DrawShapeHandles(UnityEngine.Object task, UnityEngine.Object[] reg, int proc_Ptr, Color selection2)
	{
		_003C_003Ec__DisplayClass46_0 pool = default(_003C_003Ec__DisplayClass46_0);
		pool.queueContext = task;
		pool.m_ProcessorContext = reg;
		if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Escape))
		{
			editingRotation = false;
			editingPosition = false;
			editingHeight = false;
			editingRadius = false;
		}
		if (!pool.queueContext)
		{
			return;
		}
		Handles.color = selection2;
		pool._ExceptionContext = 0;
		for (int i = 0; i < pool.m_ProcessorContext.Length; i++)
		{
			if (pool.m_ProcessorContext[i] == pool.queueContext)
			{
				pool._ExceptionContext = i;
				break;
			}
		}
		if (proc_Ptr == 0)
		{
			pool.m_TokenizerContext = pool.m_ProcessorContext.Select((UnityEngine.Object t2) => new ADOEditorUtility.ShapeSnapshot((VRCPhysBoneCollider)t2)).ToArray();
		}
		else if (proc_Ptr == 1)
		{
			pool.m_TokenizerContext = pool.m_ProcessorContext.Select((UnityEngine.Object t2) => new ADOEditorUtility.ShapeSnapshot((VRCContactSender)t2)).ToArray();
		}
		else
		{
			pool.m_TokenizerContext = pool.m_ProcessorContext.Select((UnityEngine.Object t2) => new ADOEditorUtility.ShapeSnapshot((VRCContactReceiver)t2)).ToArray();
		}
		Transform rootTransform = pool.m_TokenizerContext[pool._ExceptionContext].rootTransform;
		pool._ProducerContext = GetMaxLossyScale(rootTransform);
		int shapeType = pool.m_TokenizerContext[pool._ExceptionContext].shapeType;
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			return;
		}
		Quaternion quaternion = rootTransform.rotation * pool.m_TokenizerContext[pool._ExceptionContext].rotation;
		pool.m_ErrorContext = rootTransform.TransformPoint(pool.m_TokenizerContext[pool._ExceptionContext].position);
		Vector3 vector = quaternion * Vector3.up;
		float num = pool.m_TokenizerContext[pool._ExceptionContext].height * 0.5f - pool.m_TokenizerContext[pool._ExceptionContext].radius;
		float num2 = pool.m_TokenizerContext[pool._ExceptionContext].radius * pool._ProducerContext;
		Vector3 vector2 = num2 * vector;
		Vector3 vector3 = pool.m_ErrorContext + Mathf.Max(num * pool._ProducerContext, 0f) * (rootTransform.rotation * pool.m_TokenizerContext[pool._ExceptionContext].rotation * Vector3.up);
		Vector3 vector4 = pool.m_ErrorContext - Mathf.Max(num * pool._ProducerContext, 0f) * (rootTransform.rotation * pool.m_TokenizerContext[pool._ExceptionContext].rotation * Vector3.up);
		pool.m_ValueContext = (Event.current.shift ? 2 : (Event.current.alt ? 1 : 0));
		pool.m_MappingContext = pool.m_ValueContext == 1;
		if (editingPosition)
		{
			using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
			bool flag = Tools.pivotRotation == PivotRotation.Local;
			Vector3 vector5 = Handles.PositionHandle(pool.m_ErrorContext, flag ? quaternion : Quaternion.identity);
			if (changeCheckScope.changed)
			{
				if (pool.m_MappingContext)
				{
					Undo.RecordObject(pool.queueContext, "Adjust Position");
				}
				else
				{
					Undo.RecordObjects(pool.m_ProcessorContext, "Adjust Position");
				}
				Vector3 vector6 = vector5 - pool.m_ErrorContext;
				if (flag || pool.m_ValueContext != 0)
				{
					vector6 = rootTransform.InverseTransformVector(vector6);
				}
				int num4 = default(int);
				switch (pool.m_ValueContext)
				{
				default:
					pool.m_TokenizerContext[num4].position = pool.m_TokenizerContext[pool._ExceptionContext].position;
					num4++;
					goto IL_029b;
				case 2:
					pool.m_TokenizerContext[pool._ExceptionContext].position += vector6;
					num4 = 0;
					goto IL_029b;
				case 1:
					pool.m_TokenizerContext[pool._ExceptionContext].position += vector6;
					break;
				case 0:
					{
						for (int num3 = 0; num3 < pool.m_TokenizerContext.Length; num3++)
						{
							if (!flag)
							{
								pool.m_TokenizerContext[num3].position += pool.m_TokenizerContext[num3].rootTransform.InverseTransformVector(vector6);
							}
							else if (!(pool.m_TokenizerContext[num3].source == pool.m_TokenizerContext[pool._ExceptionContext].source))
							{
								pool.m_TokenizerContext[num3].position += pool.m_TokenizerContext[num3].rotation * Quaternion.Inverse(pool.m_TokenizerContext[pool._ExceptionContext].rotation) * vector6;
							}
							else
							{
								pool.m_TokenizerContext[pool._ExceptionContext].position += vector6;
							}
						}
						break;
					}
					IL_029b:
					if (num4 >= pool.m_TokenizerContext.Length)
					{
						break;
					}
					goto default;
				}
			}
		}
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			return;
		}
		if (editingRotation && shapeType != 0)
		{
			using EditorGUI.ChangeCheckScope changeCheckScope2 = new EditorGUI.ChangeCheckScope();
			Quaternion quaternion2 = Handles.RotationHandle(quaternion, pool.m_ErrorContext);
			if (changeCheckScope2.changed)
			{
				if (pool.m_MappingContext)
				{
					Undo.RecordObject(pool.queueContext, "Adjust Rotation");
				}
				else
				{
					Undo.RecordObjects(pool.m_ProcessorContext, "Adjust Rotation");
				}
				Quaternion rotation = Quaternion.Euler((Quaternion.Inverse(rootTransform.rotation) * quaternion2).eulerAngles);
				switch (pool.m_ValueContext)
				{
				case 0:
				case 2:
				{
					for (int num5 = 0; num5 < pool.m_TokenizerContext.Length; num5++)
					{
						pool.m_TokenizerContext[num5].rotation = rotation;
					}
					break;
				}
				case 1:
					pool.m_TokenizerContext[pool._ExceptionContext].rotation = rotation;
					break;
				}
			}
		}
		if (editingRadius && shapeType != 2)
		{
			bool flag2 = shapeType == 1;
			_003C_003Ec__DisplayClass46_1 third = default(_003C_003Ec__DisplayClass46_1);
			using (EditorGUI.ChangeCheckScope changeCheckScope3 = new EditorGUI.ChangeCheckScope())
			{
				Vector3 position = (flag2 ? vector3 : pool.m_ErrorContext);
				Quaternion rotation2 = ((!flag2) ? Quaternion.identity : quaternion);
				third._TemplateContext = Handles.RadiusHandle(rotation2, position, num2, handlesOnly: true) / pool._ProducerContext;
				CollectIdentifier(changeCheckScope3.changed, ref pool, ref third);
			}
			if (flag2)
			{
				using EditorGUI.ChangeCheckScope changeCheckScope4 = new EditorGUI.ChangeCheckScope();
				third._TemplateContext = Handles.RadiusHandle(quaternion, vector4, num2, handlesOnly: true) / pool._ProducerContext;
				CollectIdentifier(changeCheckScope4.changed, ref pool, ref third);
			}
		}
		if (editingHeight && shapeType == 1)
		{
			if (!((Func<bool>)delegate
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
				return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
			})())
			{
				return;
			}
			_003C_003Ec__DisplayClass46_3 v = default(_003C_003Ec__DisplayClass46_3);
			v.m_ContainerContext = Vector3.zero;
			v.schemaContext = Vector3.zero;
			v.m_ClassContext = vector3 + vector2;
			v._DicContext = vector4 - vector2;
			using (EditorGUI.ChangeCheckScope changeCheckScope5 = new EditorGUI.ChangeCheckScope())
			{
				v.m_ContainerContext = Handles.Slider(v.m_ClassContext, vector);
				InterruptIdentifier(changeCheckScope5.changed, forcecounter: true, ref pool, ref v);
			}
			using (EditorGUI.ChangeCheckScope changeCheckScope6 = new EditorGUI.ChangeCheckScope())
			{
				v.schemaContext = Handles.Slider(v._DicContext, vector * -1f);
				InterruptIdentifier(changeCheckScope6.changed, forcecounter: false, ref pool, ref v);
			}
			if (!((Func<bool>)delegate
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
				return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
			})())
			{
				return;
			}
		}
		ADOEditorUtility.ShapeSnapshot[] array = pool.m_TokenizerContext;
		foreach (ADOEditorUtility.ShapeSnapshot shapeSnapshot in array)
		{
			shapeSnapshot.Apply();
		}
	}

	private static void DrawShapeEditOverlay(SceneView res)
	{
		if (!editingPosition && !editingRotation && !editingRadius && !editingHeight)
		{
			return;
		}
		Tools.hidden = true;
		if (((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			int num = 1;
			if (shapeHasRadius)
			{
				num++;
			}
			if (shapeHasHeight)
			{
				num++;
			}
			if (shapeHasRotation)
			{
				num++;
			}
			DrawTitledOverlay(res, "Editing", DrawShapeEditToggles, 200f, 45 + 20 * num);
		}
	}

	private static void DrawShapeEditToggles()
	{
		if (shapeHasRadius)
		{
			PatchConfiguration("Radius", ref editingRadius);
		}
		if (shapeHasHeight)
		{
			PatchConfiguration("Height", ref editingHeight);
		}
		PatchConfiguration("Position", ref editingPosition);
		if (shapeHasRotation)
		{
			PatchConfiguration("Rotation", ref editingRotation);
		}
	}

	private static void DrawTestModeOverlay(SceneView init)
	{
		if (!isTesting)
		{
			return;
		}
		Tools.hidden |= ADOSettings.Instance().hideToolsDuringTesting;
		EditorApplication.playModeStateChanged -= FillConfiguration;
		EditorApplication.playModeStateChanged += FillConfiguration;
		if (testRoot != null)
		{
			ADOEditorUtility.TransformHandles(testRoot.transform, counterinstall: true, skipthird: true, readparam2: false, usecaller3: false, ismap4: false, bool_0: true);
		}
		DrawOverlay(init, delegate
		{
			using (new GUILayout.HorizontalScope())
			{
				bool ignorecaller;
				string tooltip = ((!(ignorecaller = ADOSettings.Instance().hideToolsDuringTesting)) ? "Native tools are visible during test." : "Native tools are hidden during test.");
				using (new GUIColorScope(GUIColorScope.ColoringType.FG, ignorecaller, ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
				{
					if (ADOEditorUtility.IconButton(new GUIContent(ADOEditorUtility.CustomizeRef().prototypeSerializer)
					{
						tooltip = tooltip
					}))
					{
						ADOSettings.Instance().hideToolsDuringTesting.Toggle();
						Tools.hidden = false;
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.Label("Testing", ADOEditorUtility.MapRef().m_WriterSerializer);
				Rect lastRect = GUILayoutUtility.GetLastRect();
				GUILayout.FlexibleSpace();
				DrawSettingsButton();
				return lastRect;
			}
		}, delegate
		{
			using (new GUIColorScope(GUIColorScope.ColoringType.BG, ADOEditorUtility.errorColor))
			{
				if (ADOEditorUtility.PatchStatus("Stop Testing") || ADOEditorUtility.CancelPressed() || ADOEditorUtility.SubmitPressed())
				{
					ToggleTestMode();
				}
			}
			using (new GUIColorScope(GUIColorScope.ColoringType.BG, ADOEditorUtility.secondaryActionColor))
			{
				if (ADOEditorUtility.PatchStatus("Restart"))
				{
					RestartTestMode();
				}
			}
			using (new GUIColorScope(GUIColorScope.ColoringType.BG, hasUnappliedTestChanges, ADOEditorUtility.validColor))
			{
				using (new EditorGUI.DisabledScope(!hasUnappliedTestChanges))
				{
					if (ADOEditorUtility.PatchStatus("Apply All Changes"))
					{
						foreach (UnityEngine.Object item in cloneHasUnappliedChanges.Keys.ToList())
						{
							if (cloneHasUnappliedChanges[item])
							{
								UnityEngine.Object obj = cloneToOriginal[item];
								if (obj != null)
								{
									Undo.RecordObject(obj, "ADO - Apply Changes");
									EditorUtility.CopySerialized(item, obj);
									cloneHasUnappliedChanges[item] = false;
								}
							}
						}
						hasUnappliedTestChanges = false;
						InsertConfiguration();
					}
				}
			}
		}, 200f, 104f);
	}

	private static void TickTestSimulation()
	{
		if (!testPhysBoneManager)
		{
			ToggleTestMode();
			return;
		}
		physBoneManagerLateUpdate.Invoke(testPhysBoneManager, null);
		for (int i = 0; i < testPhysBones.Length; i++)
		{
			if ((bool)testPhysBones[i])
			{
				bool flag = testPhysBones[i].enabled && testPhysBones[i].gameObject.activeInHierarchy;
				if (testPhysBoneEnabled[i] == flag)
				{
					continue;
				}
				testPhysBoneEnabled[i] = flag;
				if (!flag)
				{
					physBoneOnDisable.Invoke(testPhysBones[i], null);
					continue;
				}
				physBoneOnEnable.Invoke(testPhysBones[i], null);
				if (!testPhysBoneStarted[i])
				{
					testPhysBoneStarted[i] = true;
					physBoneStart.Invoke(testPhysBones[i], null);
				}
			}
			else if (testPhysBoneEnabled[i])
			{
				testPhysBoneEnabled[i] = false;
				physBoneOnDisable.Invoke(testPhysBones[i], null);
			}
		}
		for (int j = 0; j < testColliders.Length; j++)
		{
			if (!testColliders[j])
			{
				if (testColliderEnabled[j])
				{
					testColliderEnabled[j] = false;
					physBoneColliderOnDisable.Invoke(testColliders[j], null);
				}
				continue;
			}
			bool flag2 = testColliders[j].enabled && testColliders[j].gameObject.activeInHierarchy;
			if (testColliderEnabled[j] == flag2)
			{
				continue;
			}
			testColliderEnabled[j] = flag2;
			if (flag2)
			{
				physBoneColliderOnEnable.Invoke(testColliders[j], null);
				if (!testColliderStarted[j])
				{
					testColliderStarted[j] = true;
					physBoneColliderStart.Invoke(testColliders[j], null);
				}
			}
			else
			{
				physBoneColliderOnDisable.Invoke(testColliders[j], null);
			}
		}
	}

	private static void ResolvePhysBoneReflection()
	{
		if (!physBoneReflectionResolved)
		{
			physBoneReflectionResolved = true;
			physBoneManagerLateUpdate = ListAuthentication(typeof(PhysBoneManager), "LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
			physBoneManagerOnDestroy = ListAuthentication(typeof(PhysBoneManager), "OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic);
			physBoneStart = ListAuthentication(typeof(VRCPhysBoneBase), "Start", BindingFlags.Instance | BindingFlags.NonPublic);
			physBoneOnEnable = ListAuthentication(typeof(VRCPhysBoneBase), "OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
			physBoneOnDisable = ListAuthentication(typeof(VRCPhysBoneBase), "OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
			physBoneColliderStart = ListAuthentication(typeof(VRCPhysBoneColliderBase), "Start", BindingFlags.Instance | BindingFlags.NonPublic);
			physBoneColliderOnEnable = ListAuthentication(typeof(VRCPhysBoneColliderBase), "OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
			physBoneColliderOnDisable = ListAuthentication(typeof(VRCPhysBoneColliderBase), "OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
		}
	}

	private static void ToggleTestMode()
	{
		ResolvePhysBoneReflection();
		isTesting = !isTesting;
		if (Application.isPlaying)
		{
			isTesting = false;
		}
		if (isTesting)
		{
			StartTestMode();
		}
		else
		{
			StopTestMode();
		}
	}

	private static void RestartTestMode()
	{
		if (isTesting)
		{
			ToggleTestMode();
		}
		ToggleTestMode();
	}

	private static void StartTestMode()
	{
		hasShownColliderRestartPrompt |= ADOSettings.Instance().hasReadColliderTestingWarning;
		selectedObjectsBeforeTest = Selection.gameObjects;
		activeObjectBeforeTest = Selection.activeGameObject;
		originalToClone = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
		cloneToOriginal = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
		cloneHasUnappliedChanges = new Dictionary<UnityEngine.Object, bool>();
		hasUnappliedTestChanges = false;
		_003C_003Ec__DisplayClass54_0 pol = default(_003C_003Ec__DisplayClass54_0);
		pol.bridgeContext = new List<Transform>();
		testSourceRoots = Selection.transforms.Select((Transform t) => t.root.gameObject).Distinct().ToArray();
		VRCPhysBone[] componentsToFind = testSourceRoots.SelectMany((GameObject o) => o.GetComponentsInChildren<VRCPhysBone>(includeInactive: true)).ToArray();
		VRCPhysBoneColliderBase[] componentsToFind2 = testSourceRoots.SelectMany((GameObject o) => o.GetComponentsInChildren<VRCPhysBoneColliderBase>(includeInactive: true)).ToArray();
		if (testSourceRoots.Length == 0)
		{
			Log("No Active Objects with PhysBones found in the scene.", CustomLogType.Error);
			return;
		}
		testRoot = GameObject.Find("Physbone Tester");
		if ((bool)testRoot)
		{
			UnityEngine.Object.DestroyImmediate(testRoot);
		}
		testRoot = new GameObject("Physbone Tester")
		{
			hideFlags = (HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild)
		};
		testRoot.transform.position = activeObjectBeforeTest.transform.position;
		GameObject[] array = testSourceRoots;
		foreach (GameObject gameObject in array)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation, testRoot.transform);
			Dictionary<VRCPhysBone, VRCPhysBone> dictionary = ADOEditorUtility.MapComponents(gameObject.transform, gameObject2.transform, skipfilter: true, componentsToFind);
			Dictionary<VRCPhysBoneColliderBase, VRCPhysBoneColliderBase> dictionary2 = ADOEditorUtility.MapComponents(gameObject.transform, gameObject2.transform, skipfilter: true, componentsToFind2);
			VRCPhysBone component = activeObjectBeforeTest.GetComponent<VRCPhysBone>();
			if (component != null && dictionary.TryGetValue(component, out var value) && value != null)
			{
				Selection.activeGameObject = value.gameObject;
			}
			else
			{
				VRCPhysBoneColliderBase component2 = activeObjectBeforeTest.GetComponent<VRCPhysBoneColliderBase>();
				if (component2 != null && dictionary2.TryGetValue(component2, out var value2) && value2 != null)
				{
					Selection.activeGameObject = value2.gameObject;
				}
			}
			ViewIdentifier(dictionary, ref pol);
			ViewIdentifier(dictionary2, ref pol);
			gameObject.SetActive(value: false);
		}
		testPhysBoneManager = testRoot.AddComponent<PhysBoneManager>();
		PhysBoneManager.Inst = testPhysBoneManager;
		testPhysBoneManager.IsSDK = true;
		testPhysBoneManager.Init();
		testPhysBones = testRoot.GetComponentsInChildren<VRCPhysBone>(includeInactive: true);
		testPhysBoneEnabled = new bool[testPhysBones.Length];
		testPhysBoneStarted = new bool[testPhysBones.Length];
		testColliders = testRoot.GetComponentsInChildren<VRCPhysBoneCollider>(includeInactive: true);
		testColliderEnabled = new bool[testColliders.Length];
		testColliderStarted = new bool[testColliders.Length];
		UnityEngine.Object[] objects = pol.bridgeContext.Select((Transform t) => t.gameObject).ToArray();
		Selection.objects = objects;
		EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(TickTestSimulation));
		EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(TickTestSimulation));
		SceneView.duringSceneGui -= DrawTestModeOverlay;
		SceneView.duringSceneGui += DrawTestModeOverlay;
	}

	private static void StopTestMode()
	{
		EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(TickTestSimulation));
		SceneView.duringSceneGui -= DrawTestModeOverlay;
		UnityEngine.Object[] objects = selectedObjectsBeforeTest;
		Selection.objects = objects;
		Selection.activeObject = activeObjectBeforeTest;
		physBoneManagerOnDestroy.Invoke(testPhysBoneManager, null);
		if ((bool)testRoot)
		{
			UnityEngine.Object.DestroyImmediate(testRoot);
		}
		foreach (GameObject item in testSourceRoots.Where((GameObject o) => o))
		{
			item.SetActive(value: true);
		}
		originalToClone = (cloneToOriginal = null);
		cloneHasUnappliedChanges = null;
		colliderChangedDuringTest = false;
		hasUnappliedTestChanges = false;
	}

	private static void SetShapeCapabilities(bool isi, bool iscont, bool explicittemplate)
	{
		shapeHasRadius = isi;
		shapeHasHeight = iscont;
		shapeHasRotation = explicittemplate;
		if (!shapeHasRadius)
		{
			editingRadius = false;
		}
		if (!shapeHasHeight)
		{
			editingHeight = false;
		}
		if (!shapeHasRotation)
		{
			editingRotation = false;
		}
	}

	private static void InvokeConfiguration(SerializedProperty[] var1, int indexOf_col)
	{
		int intValue = var1[0].intValue;
		switch (indexOf_col)
		{
		case 2:
			editingPosition = DrawPropertyWithEditToggle(var1[4], editingPosition);
			break;
		case 0:
			if (intValue != 2)
			{
				editingRadius = DrawPropertyWithEditToggle(var1[2], editingRadius);
			}
			break;
		case 1:
			if (intValue == 1)
			{
				editingHeight = DrawPropertyWithEditToggle(var1[3], editingHeight);
			}
			break;
		}
	}

	private static void DrawCollisionTagElement(SerializedProperty reference, Rect col, int remove_FIELDAt)
	{
		if (remove_FIELDAt >= reference.arraySize || remove_FIELDAt < 0)
		{
			return;
		}
		SerializedProperty arrayElementAtIndex = reference.GetArrayElementAtIndex(remove_FIELDAt);
		col.y += 1f;
		col.height = 18f;
		col.width -= 44f;
		Rect position = col;
		position.width = 21f;
		Rect rect = col;
		rect.x += 22f;
		rect.width -= 12f;
		Rect position2 = rect;
		position2.x += rect.width;
		position2.width = 28f;
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			return;
		}
		using (new EditorGUI.DisabledScope(!(UnityEngine.Object)(object)selectedAvatar))
		{
			int num = EditorGUI.Popup(position, -1, avatarCollisionTags);
			if (num != -1)
			{
				arrayElementAtIndex.stringValue = Regex.Replace(avatarCollisionTags[num], "^Default/", string.Empty);
			}
		}
		EditorGUI.PropertyField(rect, arrayElementAtIndex, GUIContent.none);
		if (GUI.Button(position2, ADOEditorUtility.CustomizeRef()._CreatorSerializer, ADOEditorUtility.MapRef().utilsMethod))
		{
			reference.DeleteArrayElementAtIndex(remove_FIELDAt);
		}
	}

	private static void DrawCollisionTagsHeader(Rect v)
	{
		GUIStyle style = new GUIStyle("boldlabel");
		GUI.Label(v, "Collision Tags", style);
	}

	private static void MapConfiguration(Action config)
	{
		SetShapeEditOverlayActive(isvar1: true);
		config();
		RefreshSceneAvatars(ref selectedAvatar, ref sceneAvatars);
		RefreshAvatarTables();
	}

	private static void FillConfiguration(PlayModeStateChange spec)
	{
		if (spec == PlayModeStateChange.ExitingEditMode && isTesting)
		{
			ToggleTestMode();
		}
	}

	private static void SetShapeEditOverlayActive(bool isvar1)
	{
		SceneView.duringSceneGui -= DrawShapeEditOverlay;
		if (isvar1)
		{
			SceneView.duringSceneGui += DrawShapeEditOverlay;
		}
		else
		{
			Tools.hidden = false;
		}
	}

	private static void RefreshAvatarTables()
	{
		ADOEditorUtility.GetPopulatedPlayableLayers(selectedAvatar, ref avatarPlayableLayerNames, ref avatarPlayableLayerTypes);
		if (!(UnityEngine.Object)(object)selectedAvatar)
		{
			avatarParameterNames = Array.Empty<string>();
		}
		RefreshAvatarParameterNames();
		avatarCollisionTags = ((UnityEngine.Component)(object)selectedAvatar).GetComponentsInChildren<VRCContactSender>().SelectMany((VRCContactSender cs) => cs.collisionTags).Concat(((UnityEngine.Component)(object)selectedAvatar).GetComponentsInChildren<VRCContactReceiver>().SelectMany((VRCContactReceiver cr) => cr.collisionTags))
			.Except(ADOEditorUtility.defaultCollisionTags)
			.Concat(ADOEditorUtility.defaultCollisionTags.Select((string s) => "Default/" + s))
			.Distinct()
			.ToArray();
	}

	private static void RefreshAvatarParameterNames()
	{
		avatarParameterNames = (from p in (from rc in selectedAvatar.baseAnimationLayers.Concat(selectedAvatar.specialAnimationLayers)
				where !rc.isDefault && (bool)rc.animatorController
				select AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(AssetDatabase.GetAssetPath(rc.animatorController)) into c
				where c
				select c).SelectMany((UnityEditor.Animations.AnimatorController c) => c.parameters)
			select p.name into p
			where !ADOEditorUtility.reservedAvatarParameters.Contains(p)
			select p).Distinct().ToArray();
	}

	private static void ResetFoldouts(AnimBool[] i, UnityAction col)
	{
		for (int j = 0; j < i.Length; j++)
		{
			if (i[j] == null)
			{
				i[j] = new AnimBool();
			}
			else
			{
				i[j] = new AnimBool(i[j].target);
			}
			i[j].valueChanged.AddListener(col);
		}
	}

	[DidReloadScripts]
	private static void WriteConfiguration()
	{
		_003C_003Ec__DisplayClass66_0 _003C_003Ec__DisplayClass66_ = new _003C_003Ec__DisplayClass66_0();
		Type type = Type.GetType("UnityEditor.CustomEditorAttributes, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		_003C_003Ec__DisplayClass66_.publisherContext = type.GetField("s_Initialized", BindingFlags.Static | BindingFlags.NonPublic);
		_003C_003Ec__DisplayClass66_.CancelProcess();
	}

	internal static bool MoveConfiguration()
	{
		Log((!isVerifyingLicense) ? "Please activate your license." : "Please wait for verification.", CustomLogType.Error, !isLicensed);
		return isLicensed;
	}

	internal static void BuildLayerParameterOptions(string[] info, int[] attr, string[] role, out string[] cfg2, out int[] pred3)
	{
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < info.Length; i++)
		{
			for (int j = 0; j < role.Length; j++)
			{
				list.Add(info[i] + "/" + role[j]);
				list2.Add(int.Parse($"{attr[i]}{j}"));
			}
		}
		cfg2 = list.ToArray();
		pred3 = list2.ToArray();
	}

	internal static int[] SplitDigits(int idx_v, int contstart)
	{
		string text = idx_v.ToString();
		int[] array = new int[contstart];
		int num = contstart - text.Length;
		int num2 = 0;
		for (int i = 0; i < text.Length; i++)
		{
			array[i] = ((i >= num) ? (text[num2++] - 48) : 0);
		}
		return array;
	}

	internal static void RefreshSceneAvatars(ref VRCAvatarDescriptor spec, ref VRCAvatarDescriptor[] cust, Action dir = null, Func<VRCAvatarDescriptor, bool> caller2 = null)
	{
		cust = UnityEngine.Object.FindObjectsOfType<VRCAvatarDescriptor>();
		if ((bool)(UnityEngine.Object)(object)spec)
		{
			return;
		}
		if (cust.Length != 0)
		{
			if (caller2 == null)
			{
				spec = cust[0];
			}
			else
			{
				spec = cust.FirstOrDefault(caller2) ?? cust[0];
			}
		}
		dir?.Invoke();
	}

	internal static bool DrawAvatarSelector(ref VRCAvatarDescriptor var1, VRCAvatarDescriptor[] cust, Action helper = null, bool isresult2 = true, bool allowvalue3 = true, bool useasset4 = true, string res5 = "Avatar", string col6 = "The Targeted VRCAvatar", Action instance7 = null)
	{
		if (!(UnityEngine.Object)(object)DrawAvatarPopup(ref var1, cust, helper, res5, col6, instance7))
		{
			return false;
		}
		return DrawAvatarWarnings(var1, isresult2, allowvalue3, useasset4);
	}

	private static VRCAvatarDescriptor DrawAvatarPopup(ref VRCAvatarDescriptor res, VRCAvatarDescriptor[] cfg, Action util = null, string token2 = "Avatar", string value3 = "The Targeted VRCAvatar", Action t4 = null)
	{
		using (new GUILayout.HorizontalScope())
		{
			GUIContent label = new GUIContent(token2, value3);
			if (cfg == null || cfg.Length == 0)
			{
				EditorGUILayout.LabelField(label, new GUIContent("No Avatar Descriptors Found"));
			}
			else
			{
				using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
				int num = EditorGUILayout.Popup(label, (!(UnityEngine.Object)(object)res) ? (-1) : Array.IndexOf(cfg, res), (from x in cfg
					where (UnityEngine.Object)(object)x
					select ((UnityEngine.Object)(object)x).name).ToArray());
				if (changeCheckScope.changed)
				{
					res = cfg[num];
					EditorGUIUtility.PingObject((UnityEngine.Object)(object)res);
					util?.Invoke();
				}
			}
			t4?.Invoke();
		}
		return res;
	}

	private static bool DrawAvatarWarnings(VRCAvatarDescriptor setup, bool iscont = true, bool resreguired = true, bool loadpol2 = true)
	{
		if (!resreguired || !DrawPrefabWarning(setup))
		{
			if (loadpol2)
			{
				return !DrawPlayableLayerWarning(setup, iscont);
			}
			return true;
		}
		return false;
	}

	private static bool DrawPrefabWarning(VRCAvatarDescriptor ident)
	{
		if ((bool)(UnityEngine.Object)(object)ident)
		{
			bool num = PrefabUtility.IsPartOfAnyPrefab(((UnityEngine.Component)(object)ident).gameObject);
			if (num)
			{
				EditorGUILayout.HelpBox("Target Avatar is a part of a prefab. Prefab unpacking is required.", MessageType.Error);
				if (GUILayout.Button("Unpack"))
				{
					PrefabUtility.UnpackPrefabInstance(((UnityEngine.Component)(object)ident).gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
				}
			}
			return num;
		}
		return false;
	}

	private static bool DrawPlayableLayerWarning(VRCAvatarDescriptor spec, bool insertreg = true)
	{
		if ((bool)(UnityEngine.Object)(object)spec)
		{
			VRCAvatarDescriptor.CustomAnimLayer[] baseAnimationLayers = spec.baseAnimationLayers;
			if (baseAnimationLayers.Length <= 3)
			{
				if (insertreg)
				{
					EditorGUILayout.HelpBox("Your Avatar's descriptor is set as Non-Humanoid! Please make sure that your Avatar's rig is Humanoid.", MessageType.Error);
				}
				return insertreg;
			}
			bool num = baseAnimationLayers[3].type == baseAnimationLayers[4].type;
			if (num)
			{
				EditorGUILayout.HelpBox("Your Avatar's Action playable layer is set as FX. This is an uncommon bug.", MessageType.Error);
				if (GUILayout.Button("Fix"))
				{
					spec.baseAnimationLayers[3].type = VRCAvatarDescriptor.AnimLayerType.Action;
					EditorUtility.SetDirty((UnityEngine.Object)(object)spec);
				}
			}
			return num;
		}
		return false;
	}

	private static float GetMaxLossyScale(Transform i)
	{
		return Mathf.Max(i.lossyScale.x, i.lossyScale.y, i.lossyScale.z);
	}

	private static bool DrawPropertyWithEditToggle(SerializedProperty def, bool testtoken)
	{
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(def);
			return DrawIconToggle(testtoken, ADOEditorUtility.CustomizeRef().stateSerializer);
		}
	}

	private static bool DrawIconToggle(bool movefirst, GUIContent vis)
	{
		using (new GUIColorScope(GUIColorScope.ColoringType.BG, movefirst, ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
		{
			movefirst = ADOEditorUtility.PrepareStatus(movefirst, vis, ADOEditorUtility.MapRef().methodMethod, GUILayout.Width(18f), GUILayout.Height(18f));
			return movefirst;
		}
	}

	private static void PatchConfiguration(string v, ref bool cfg, params GUILayoutOption[] options)
	{
		CheckConfiguration(new GUIContent(v), ref cfg, options);
	}

	private static void CheckConfiguration(GUIContent item, ref bool pol, params GUILayoutOption[] options)
	{
		using (new GUIColorScope(GUIColorScope.ColoringType.BG, pol, ADOEditorUtility.validColor, ADOEditorUtility.errorColor))
		{
			pol = ADOEditorUtility.PrepareStatus(pol, item, GUI.skin.button, options);
		}
	}

	private static void CallConfiguration(SerializedProperty init, string cont, Action dir = null, params GUILayoutOption[] options)
	{
		RegisterConfiguration(init, new GUIContent(cont), dir, options);
	}

	private static void RegisterConfiguration(SerializedProperty def, GUIContent visitor, Action control = null, params GUILayoutOption[] options)
	{
		int positionmap = (def.hasMultipleDifferentValues ? 2 : (def.boolValue ? 1 : 0));
		using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
		bool boolValue;
		using (new GUIColorScope(GUIColorScope.ColoringType.BG, positionmap, ADOEditorUtility.MapRef().m_SerializerMethod))
		{
			boolValue = ADOEditorUtility.PrepareStatus(def.boolValue, visitor, GUI.skin.button, options);
		}
		if (changeCheckScope.changed)
		{
			def.boolValue = boolValue;
			control?.Invoke();
		}
	}

	private static void DrawOptionalProperty(SerializedProperty spec)
	{
		if (spec != null)
		{
			EditorGUILayout.PropertyField(spec);
		}
	}

	private static void DrawSelfOthersToggles(SerializedProperty value, SerializedProperty token)
	{
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PrefixLabel(new GUIContent(value.displayName, value.tooltip));
			SerializedProperty serializedProperty = token.FindPropertyRelative("allowSelf");
			SerializedProperty serializedProperty2 = token.FindPropertyRelative("allowOthers");
			bool flag = value.enumValueIndex == 1 || (value.enumValueIndex != 0 && serializedProperty.boolValue);
			bool flag2 = value.enumValueIndex == 1 || (value.enumValueIndex != 0 && serializedProperty2.boolValue);
			EditorGUI.BeginChangeCheck();
			EditorGUIUtility.labelWidth = 50f;
			using (new ShowMixedValueScope(value.hasMultipleDifferentValues || (value.enumValueIndex == 2 && serializedProperty.hasMultipleDifferentValues)))
			{
				flag = EditorGUILayout.Toggle("Self", flag);
			}
			using (new ShowMixedValueScope(value.hasMultipleDifferentValues || (value.enumValueIndex == 2 && serializedProperty2.hasMultipleDifferentValues)))
			{
				flag2 = EditorGUILayout.Toggle("Others", flag2);
			}
			EditorGUIUtility.labelWidth = 160f;
			if (EditorGUI.EndChangeCheck())
			{
				value.enumValueIndex = 2;
				serializedProperty.boolValue = flag;
				serializedProperty2.boolValue = flag2;
			}
		}
	}

	private static void DrawTargetAvatarSelector()
	{
		DrawAvatarSelector(ref selectedAvatar, sceneAvatars, RefreshAvatarTables, isresult2: false, allowvalue3: false, useasset4: true, "Target Avatar");
	}

	private static void DrawAvatarParameterField(SerializedProperty task)
	{
		_003C_003Ec__DisplayClass86_0 visitor = default(_003C_003Ec__DisplayClass86_0);
		visitor.serializerSerializer = task;
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(visitor.serializerSerializer);
			int selectedIndex = -1;
			using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
			{
				selectedIndex = EditorGUILayout.Popup(selectedIndex, avatarParameterNames, "textfielddropdown", GUILayout.Width(18f));
				if (changeCheckScope.changed)
				{
					visitor.serializerSerializer.stringValue = avatarParameterNames[selectedIndex];
				}
			}
			if (visitor.serializerSerializer.hasMultipleDifferentValues || string.IsNullOrEmpty(visitor.serializerSerializer.stringValue))
			{
				return;
			}
			Rect controlRect;
			_003C_003Ec__DisplayClass86_1 field = default(_003C_003Ec__DisplayClass86_1);
			while (true)
			{
				controlRect = EditorGUILayout.GetControlRect(GUILayout.Width(50f));
				BuildLayerParameterOptions(avatarPlayableLayerNames, avatarPlayableLayerTypes, new string[3] { "Bool", "Int", "Float" }, out var cfg, out var pred);
				EditorGUI.BeginChangeCheck();
				int idx_v = EditorGUI.IntPopup(controlRect, -1, cfg, pred);
				if (!EditorGUI.EndChangeCheck())
				{
					break;
				}
				int[] array = SplitDigits(idx_v, 2);
				if (selectedAvatar.TryGetAnimatorController((VRCAvatarDescriptor.AnimLayerType)array[0], out field._MethodSerializer))
				{
					switch (array[1])
					{
					default:
						continue;
					case 2:
						PostIdentifier(field._MethodSerializer.AddParameterIfMissing(visitor.serializerSerializer.stringValue, UnityEngine.AnimatorControllerParameterType.Float, 0f), ref visitor, ref field);
						break;
					case 0:
						PostIdentifier(field._MethodSerializer.AddParameterIfMissing(visitor.serializerSerializer.stringValue, UnityEngine.AnimatorControllerParameterType.Bool, 0f), ref visitor, ref field);
						break;
					case 1:
						PostIdentifier(field._MethodSerializer.AddParameterIfMissing(visitor.serializerSerializer.stringValue, UnityEngine.AnimatorControllerParameterType.Int, 0f), ref visitor, ref field);
						break;
					}
				}
				else
				{
					Log("Couldn't fetch selected playable layer!", CustomLogType.Error);
				}
				break;
			}
			controlRect.x += 3f;
			GUI.Label(controlRect, "Add");
		}
	}

	private static bool ReadConfiguration(IEnumerable<UnityEngine.Object> param)
	{
		using (new GUILayout.HorizontalScope())
		{
			using (new GUIColorScope(GUIColorScope.ColoringType.BG, isTesting, ADOEditorUtility.errorColor))
			{
				bool isPlaying;
				string asset = ((isPlaying = Application.isPlaying) ? "Editor is in PlayMode" : ((!isTesting) ? "Test PhysBones in Scene" : "Stop Testing - ESC / Enter"));
				using (new EditorGUI.DisabledScope(isPlaying))
				{
					if (ADOEditorUtility.PatchStatus(asset))
					{
						ToggleTestMode();
					}
				}
			}
			if (!isTesting)
			{
				return false;
			}
			using (new GUIColorScope(GUIColorScope.ColoringType.BG, ADOEditorUtility.secondaryActionColor))
			{
				if (ADOEditorUtility.PatchStatus("Restart", GUILayout.ExpandWidth(expand: false)))
				{
					RestartTestMode();
				}
			}
			UnityEngine.Object[] array = param.Where((UnityEngine.Object b) => b != null && cloneHasUnappliedChanges.ContainsKey(b) && cloneToOriginal[b] != null).ToArray();
			bool flag = array.Any((UnityEngine.Object b) => cloneHasUnappliedChanges[b]);
			using (new GUIColorScope(GUIColorScope.ColoringType.BG, flag, ADOEditorUtility.validColor))
			{
				using (new EditorGUI.DisabledScope(!flag))
				{
					if (ADOEditorUtility.PatchStatus("Apply Changes", GUILayout.ExpandWidth(expand: false)))
					{
						UnityEngine.Object[] array2 = array;
						foreach (UnityEngine.Object obj in array2)
						{
							UnityEngine.Object obj2 = cloneToOriginal[obj];
							using (new ReflectionRestoreScope(obj2, false, "rootTransform", "ignoreTransforms", "colliders"))
							{
								Undo.RecordObject(obj2, "ADO - Apply Changes");
								EditorUtility.CopySerialized(obj, obj2);
								cloneHasUnappliedChanges[obj] = false;
							}
						}
						return true;
					}
				}
			}
		}
		return false;
	}

	private static bool TestConfiguration<T>(SerializedObject reference, IEnumerable<T> reg, Action<T> proc = null) where T : UnityEngine.Object
	{
		if ((bool)reference.targetObject)
		{
			bool hasModifiedProperties;
			if (hasModifiedProperties = reference.hasModifiedProperties)
			{
				foreach (T item in reg)
				{
					proc?.Invoke(item);
					if (isTesting && cloneHasUnappliedChanges.ContainsKey(item))
					{
						cloneHasUnappliedChanges[item] = true;
						hasUnappliedTestChanges = true;
					}
				}
			}
			reference.ApplyModifiedProperties();
			return hasModifiedProperties;
		}
		return false;
	}

	private static void InsertConfiguration()
	{
		if (isTesting && colliderChangedDuringTest && !hasShownColliderRestartPrompt)
		{
			hasShownColliderRestartPrompt = true;
			switch (EditorUtility.DisplayDialogComplex("Testing Restart Required", "Collider changes require a restart of the testing process. Do you want to restart testing?", "Yes", "No", "Don't ask again"))
			{
			case 0:
				RestartTestMode();
				break;
			case 2:
				ADOSettings.Instance().hasReadColliderTestingWarning.SetValue(nores: true);
				break;
			}
		}
	}

	private static void EnableConfiguration(Action ident)
	{
		if (isVerifyingLicense || isRequestingTransferCode)
		{
			return;
		}
		EditorGUILayout.HelpBox("This is 'Avatar Dynamics Overhaul'. If you don't know what this is, you may have imported it from a package that shouldn't contain it. You can delete the editor script to revert back to original behaviour. Usually found in Packages > DreadScripts - Avatar Dynamics Overhaul. If this is the case, please notify the package creator about this.", MessageType.Warning);
		using (new GUILayout.HorizontalScope())
		{
			if (ADOEditorUtility.LoginStatus("Locate", EditorStyles.toolbarButton))
			{
				UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath("Packages/com.dreadscripts.avatardynamicsoverhaul");
				UnityEngine.Debug.Log("Found through path: " + obj);
				if (!obj)
				{
					string[] array = AssetDatabase.FindAssets("t:script ADOverhaul");
					if (array.Length != 0)
					{
						obj = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(array[0]));
					}
				}
				if ((bool)obj)
				{
					EditorGUIUtility.PingObject(obj);
				}
				else
				{
					EditorUtility.DisplayDialog("Not Found", "Couldn't locate the script automatically.", "Ok");
				}
			}
			if (ADOEditorUtility.LoginStatus("Info", EditorStyles.toolbarButton))
			{
				Application.OpenURL("https://linktr.ee/Dreadrith");
			}
			if (ADOEditorUtility.LoginStatus("Switch Editor", EditorStyles.toolbarButton))
			{
				ident();
			}
		}
	}

	[SpecialName]
	private static void BugReporterOpen(bool isi)
	{
		bool flag = bugReporterOpen;
		bugReporterOpen = isi;
		if (!bugReporterOpen && flag)
		{
			BugReporter.OnCompilationStarted(null);
		}
	}

	private static void AwakeConfiguration()
	{
		DrawPanelHeader("Send Feedback for ADOverhaul", "If you have a suggestion, preference, or something to comment, please send it here!\nNote that the feedback is not anonymous. Abuse may result in blacklisting.");
		feedbackPanelOpen = isLicensed;
		feedbackText = EditorGUILayout.TextArea(feedbackText, GUILayout.MinHeight(54f));
		using (new GUILayout.HorizontalScope())
		{
			if (ADOEditorUtility.LoginStatus("Cancel", EditorStyles.toolbarButton, GUILayout.ExpandWidth(expand: false)))
			{
				feedbackPanelOpen = false;
			}
			using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(feedbackText) || isSendingFeedback))
			{
				if (ADOEditorUtility.LoginStatus("Send Feedback", EditorStyles.toolbarButton))
				{
					if (feedbackText.Length > 2000)
					{
						feedbackText = feedbackText.Substring(0, 2000);
					}
					List<(string, string)> list = CountConfiguration("sendfeedback", new(string, string)[1] { ("feedback", Uri.EscapeUriString(feedbackText)) });
					StartConfiguration(list);
					isSendingFeedback = true;
					OrderIdentifier(IncludeConfiguration(list.ToArray())).HandleTask(ComputeConfiguration, UnityEngine.Debug.LogException, null, null, delegate
					{
						isSendingFeedback = false;
						feedbackPanelOpen = false;
						RepaintOpenWindowsDelayed();
					});
				}
			}
		}
	}

	[SpecialName]
	private static float ResolveSerializer()
	{
		return retryAllowedAtRealtime - Time.realtimeSinceStartup;
	}

	[SpecialName]
	private static bool GetSerializer()
	{
		return ResolveSerializer() > 0f;
	}

	[InitializeOnLoadMethod]
	private static void DisableConfiguration()
	{
		bool flag = RateConfiguration();
		if (!ADOSettings.Instance().a_HasSucceededLastVerification)
		{
			licenseKeyEntryRequired = true;
			licenseCheckedThisSession = flag;
		}
		if (flag && (bool)ADOSettings.Instance().a_VerifyOnProjectLoad)
		{
			ADOEditorUtility.DelayCall(delegate
			{
				AssetConfiguration(testkey: false);
			});
		}
	}

	private static void VisitConfiguration()
	{
		if (!licenseCheckedThisSession && (bool)ADOSettings.Instance().a_VerifyOnDisplay && RateConfiguration())
		{
			AssetConfiguration(testkey: false);
		}
	}

	private static void AssetConfiguration(bool testkey)
	{
		_003C_003Ec__DisplayClass132_0 CS_0024_003C_003E8__locals10 = new _003C_003Ec__DisplayClass132_0();
		if ((!ADOSettings.Instance().a_VerifyOnDisplay.GetValue() && !ADOSettings.Instance().a_VerifyOnProjectLoad.GetValue() && !testkey) || (licenseKeyEntryRequired && !licenseCheckRetryOffered) || isVerifyingLicense)
		{
			return;
		}
		licenseCheckRetryOffered = false;
		isVerifyingLicense = true;
		licenseCheckedThisSession = true;
		CS_0024_003C_003E8__locals10.itemContext = "No1lKII9IzcBAbihub6nCg==" + EditorAnalyticsSessionInfo.id;
		try
		{
			if (SessionState.GetBool(CS_0024_003C_003E8__locals10.itemContext, defaultValue: false))
			{
				_003C_003Ec__DisplayClass132_1 _003C_003Ec__DisplayClass132_1_ = default(_003C_003Ec__DisplayClass132_1);
				_003C_003Ec__DisplayClass132_1_._IndexerContext = new AesManaged();
				try
				{
					_003C_003Ec__DisplayClass132_1_._IndexerContext.Key = Convert.FromBase64String("LWw2tFi+lgG6KK4+nMum8RuWZMIOhu1urChsHMbizPM=");
					_003C_003Ec__DisplayClass132_1_._IndexerContext.IV = Convert.FromBase64String("MEZqk6gCgPTwifeH3YrTlQ==");
					_003C_003Ec__DisplayClass132_2 _003C_003Ec__DisplayClass132_2_ = default(_003C_003Ec__DisplayClass132_2);
					_003C_003Ec__DisplayClass132_2_.m_PoolContext = new HMACSHA1(Encoding.UTF8.GetBytes(CS_0024_003C_003E8__locals10.itemContext));
					try
					{
						if (RemoveConfiguration() == CS_0024_003C_003E8__locals10.AwakeWatcher("date", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_))
						{
							licenseUsername = CS_0024_003C_003E8__locals10.AwakeWatcher("u", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_);
							licenseVariant = CS_0024_003C_003E8__locals10.AwakeWatcher("v", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_);
							licenseToken = CS_0024_003C_003E8__locals10.AwakeWatcher("r", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_);
							hardwareId = CS_0024_003C_003E8__locals10.AwakeWatcher("m", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_);
							InstantiateConfiguration();
							RestartConfiguration();
							isLicensed = true;
							licenseKeyEntryRequired = true;
							isVerifyingLicense = false;
							wasLicensedBeforeReset = true;
							ResolveConfiguration(istask: true);
							RepaintOpenWindows();
						}
					}
					finally
					{
						if (_003C_003Ec__DisplayClass132_2_.m_PoolContext != null)
						{
							((IDisposable)_003C_003Ec__DisplayClass132_2_.m_PoolContext).Dispose();
						}
					}
				}
				finally
				{
					if (_003C_003Ec__DisplayClass132_1_._IndexerContext != null)
					{
						((IDisposable)_003C_003Ec__DisplayClass132_1_._IndexerContext).Dispose();
					}
				}
			}
		}
		catch
		{
			Log("failed to verify from cache.", CustomLogType.Warning);
		}
		CloneConfiguration(delegate
		{
			List<(string, string)> list = CountConfiguration("verifylicense");
			StartConfiguration(list);
			OrderIdentifier(IncludeConfiguration(list.ToArray())).HandleTask(delegate(JsonObject response)
			{
				_003C_003Ec__DisplayClass132_3 _003C_003Ec__DisplayClass132_ = new _003C_003Ec__DisplayClass132_3();
				_003C_003Ec__DisplayClass132_.m_SetterContext = CS_0024_003C_003E8__locals10;
				_003C_003Ec__DisplayClass132_._SystemContext = response;
				isVerifyingLicense = false;
				licenseKeyEntryRequired = true;
				QueryConfiguration(_003C_003Ec__DisplayClass132_._SystemContext, _003C_003Ec__DisplayClass132_.RateWatcher, delegate
				{
					bool stripinstance = wasLicensedBeforeReset;
					isLicensed = false;
					wasLicensedBeforeReset = false;
					licenseToken = (licenseUsername = (licenseVariant = string.Empty));
					ADOSettings.Instance().a_HasSucceededLastVerification.SetValue(nores: false);
					SessionState.EraseBool(CS_0024_003C_003E8__locals10.itemContext);
					ResetConfiguration(stripinstance);
				}, comparesecond2: false);
			}, _003C_003Ec.m_ProcAuthentication.CountServer, null, null, RepaintOpenWindowsDelayed);
		}, ispred: true);
	}

	private static void PopConfiguration()
	{
		isActivatingLicense = true;
		if (!FindConfiguration())
		{
			Log("Invalid License Key!", CustomLogType.Error);
			return;
		}
		CloneConfiguration(delegate
		{
			List<(string, string)> list = CountConfiguration("activatelicense");
			StartConfiguration(list);
			OrderIdentifier(IncludeConfiguration(list.ToArray())).HandleTask(delegate(JsonObject response)
			{
				isActivatingLicense = false;
				QueryConfiguration(response, delegate
				{
					licenseKeyEntryRequired = false;
					ADOSettings.Instance().a_HasSucceededLastVerification.SetValue(nores: true);
					AssetConfiguration(testkey: true);
				});
			}, delegate(Exception exception)
			{
				isActivatingLicense = false;
				Log($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
			}, null, null, RepaintOpenWindowsDelayed);
		}, ispred: true);
	}

	private static void InstantiateConfiguration()
	{
		licensedToDisplayName = licenseUsername;
		if (string.IsNullOrWhiteSpace(licensedToDisplayName))
		{
			return;
		}
		try
		{
			Match match = Regex.Match(licensedToDisplayName, "(?i)(?:<color=#(?:[0-9a-f]{8}|[0-9a-f]{6})>)?.*?(#\\d{4})(?:<\\/color>)?$");
			if (match.Success)
			{
				licensedToDisplayName = licensedToDisplayName.Remove(match.Groups[1].Index, match.Groups[1].Length);
			}
			if (licensedToDisplayName.Length > 1 && licensedToDisplayName[0] == '@')
			{
				licensedToDisplayName = licensedToDisplayName.Substring(1);
			}
		}
		catch
		{
		}
	}

	private static void RestartConfiguration()
	{
		string[] array = hardwareId.Split(new char[1] { '-' });
		string[] array2 = RemoveConfiguration().Split(new char[1] { '/' });
		array2[2] = array2[2].Substring(2, 2);
		unreadDeviceDateFingerprint = array2[2] + array[0].Substring(0, 10) + array2[1] + array[2].Substring(0, 10) + array2[0];
	}

	private static void ManageConfiguration()
	{
		if (string.IsNullOrWhiteSpace(sessionId))
		{
			string key = "DreadScriptssid";
			sessionId = EditorPrefs.GetString(key, string.Empty);
			if (string.IsNullOrWhiteSpace(sessionId) || !Regex.IsMatch(sessionId, "[0-9a-f]{32}"))
			{
				sessionId = GUID.Generate().ToString();
				EditorPrefs.SetString(key, sessionId);
			}
		}
	}

	private static bool RateConfiguration()
	{
		if (!string.IsNullOrWhiteSpace(licenseKey))
		{
			return true;
		}
		licenseKey = EditorPrefs.GetString("No1lKII9IzcBAbihub6nCg==LK", string.Empty);
		if (!AddConfiguration())
		{
			licenseKey = string.Empty;
		}
		return !(licenseKeyEntryRequired = string.IsNullOrWhiteSpace(licenseKey));
	}

	private static void CloneConfiguration(Action v, bool ispred = false)
	{
		_003C_003Ec__DisplayClass138_0 CS_0024_003C_003E8__locals31 = new _003C_003Ec__DisplayClass138_0();
		CS_0024_003C_003E8__locals31._ConfigContext = ispred;
		CS_0024_003C_003E8__locals31.m_MockContext = v;
		CS_0024_003C_003E8__locals31._SpecificationContext = new string[4][]
		{
			new string[3] { "Manufacturer", "Product", "SerialNumber" },
			new string[1] { "ProcessorId" },
			new string[1] { "SerialNumber" },
			new string[4] { "Manufacturer", "PartNumber", "SerialNumber", "Capacity" }
		};
		CS_0024_003C_003E8__locals31.attrContext = new StringBuilder();
		CS_0024_003C_003E8__locals31._WrapperContext = new StringBuilder();
		CS_0024_003C_003E8__locals31._ParameterContext = EditorPrefs.GetString("DSLICINF", string.Empty);
		CS_0024_003C_003E8__locals31.m_InterpreterContext = string.IsNullOrWhiteSpace(CS_0024_003C_003E8__locals31._ParameterContext);
		if (!CS_0024_003C_003E8__locals31.m_InterpreterContext)
		{
			try
			{
				CS_0024_003C_003E8__locals31._ParameterContext = PushIdentifier(CS_0024_003C_003E8__locals31._ParameterContext);
			}
			catch
			{
				CS_0024_003C_003E8__locals31._ParameterContext = string.Empty;
				CS_0024_003C_003E8__locals31.m_InterpreterContext = true;
				EditorPrefs.DeleteKey("DSLICINF");
			}
		}
		CS_0024_003C_003E8__locals31.m_ObjectContext = new string[4];
		CS_0024_003C_003E8__locals31.m_ServiceContext = new string[4];
		CS_0024_003C_003E8__locals31.reponseContext = new string[4];
		ProcessRunner[] reference = new ProcessRunner[4]
		{
			new ProcessRunner("wmic baseboard get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ObjectContext[0] = o;
			}, wantfilter: true),
			new ProcessRunner("wmic cpu get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ObjectContext[1] = o;
			}, wantfilter: true),
			new ProcessRunner("wmic diskdrive get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ObjectContext[2] = o;
			}, wantfilter: true),
			new ProcessRunner("wmic memorychip get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ObjectContext[3] = o;
			}, wantfilter: true)
		};
		CS_0024_003C_003E8__locals31.modelContext = new ProcessRunner[4]
		{
			new ProcessRunner("Get-CimInstance -class Win32_baseboard | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ServiceContext[0] = o;
			}),
			new ProcessRunner("Get-CimInstance -class Win32_processor | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ServiceContext[1] = o;
			}),
			new ProcessRunner("Get-CimInstance -class Win32_diskdrive | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ServiceContext[2] = o;
			}),
			new ProcessRunner("Get-CimInstance -class win32_physicalmemory | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ServiceContext[3] = o;
			})
		};
		CS_0024_003C_003E8__locals31._InfoContext = new CancellationTokenSource();
		CS_0024_003C_003E8__locals31._InfoContext.CancelAfter(10000);
		PrepareIdentifier(reference, delegate
		{
			try
			{
				CS_0024_003C_003E8__locals31.CompareReg(isCMD: true);
				CS_0024_003C_003E8__locals31.SetReg();
			}
			catch (Exception exc)
			{
				CS_0024_003C_003E8__locals31.VerifyReg(isCMD: true, exc);
			}
		}, CS_0024_003C_003E8__locals31._InfoContext);
	}

	private static void ComputeConfiguration(JsonObject init)
	{
		QueryConfiguration(init, null);
	}

	private static void QueryConfiguration(JsonObject i, Action selection, Action comp = null, bool comparesecond2 = true)
	{
		bool num = i.Item("success");
		string text = i.Item("message");
		string text2 = i.Item("url");
		bool flag = !string.IsNullOrEmpty(text2);
		string text3 = i.Item("url_name");
		if (string.IsNullOrWhiteSpace(text3))
		{
			text3 = "Link";
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			text = text.Replace("\\n", "\n");
		}
		if (num)
		{
			if (!string.IsNullOrEmpty(text) && comparesecond2)
			{
				Log(text);
			}
			selection?.Invoke();
			return;
		}
		bool flag2 = i.Item("wait_warn");
		float num2 = i.Item("wait_time");
		serverWarnedTooManyAttempts |= flag2;
		if (!(num2 <= 0f))
		{
			retryAllowedAtRealtime = Time.realtimeSinceStartup + num2;
		}
		comp?.Invoke();
		if (!string.IsNullOrEmpty(text))
		{
			Log(text, CustomLogType.Error);
			if (!flag)
			{
				EditorUtility.DisplayDialog("Warning!", text, "Ok");
			}
			else if (EditorUtility.DisplayDialog("Warning!", text, text3, "Ok"))
			{
				Application.OpenURL(text2);
			}
		}
	}

	private static List<(string, string)> CountConfiguration(string i, IEnumerable<(string, string)> connection = null)
	{
		ManageConfiguration();
		List<(string, string)> list = new List<(string, string)>
		{
			("command", i),
			("product_id", "No1lKII9IzcBAbihub6nCg=="),
			("version", version.ToString()),
			("HWID", hardwareId),
			("SID", sessionId),
			("license_key", licenseKey)
		};
		if (connection != null)
		{
			list.AddRange(connection);
		}
		return list;
	}

	private static void StartConfiguration(List<(string, string)> item)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (var item4 in item)
		{
			string item2 = item4.Item2;
			stringBuilder.Append(item2);
		}
		using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?"));
		string item3 = Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString())));
		item.Add(("hash", item3));
	}

	private static string RemoveConfiguration()
	{
		string text = EnableIdentifier(DateTime.UtcNow.Day.ToString());
		string text2 = EnableIdentifier(DateTime.UtcNow.Month.ToString());
		string text3 = DateTime.UtcNow.Year.ToString();
		currentDateStamp = text + "/" + text2 + "/" + text3;
		return currentDateStamp;
	}

	private static void ReflectConfiguration(Action value)
	{
		if (!isLicensed)
		{
			pendingLicensedCallbacks = (Action)Delegate.Remove(pendingLicensedCallbacks, value);
			pendingLicensedCallbacks = (Action)Delegate.Combine(pendingLicensedCallbacks, value);
		}
		else if (((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			value?.Invoke();
		}
	}

	private static void ResolveConfiguration(bool istask)
	{
		if (isLicensed && ((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + licenseKey));
			return licenseToken == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(currentDateStamp + hardwareId)));
		})())
		{
			if (!licensedCallbacksFlushed)
			{
				pendingLicensedCallbacks?.Invoke();
			}
			licensedCallbacksFlushed = true;
		}
	}

	private static void ResetConfiguration(bool stripinstance)
	{
	}

	[SpecialName]
	private static string ExcludeSerializer()
	{
		string text = "";
		if (serverWarnedTooManyAttempts)
		{
			text += "Too many failed attempts! Further failed attempts will result in getting your device blocked!\n";
		}
		if (GetSerializer())
		{
			text += $"Please wait {Mathf.CeilToInt(ResolveSerializer())} seconds.";
		}
		return text;
	}

	private static void GetConfiguration()
	{
		using (new GUILayout.HorizontalScope())
		{
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				GUILayout.Label("License: " + (string.IsNullOrWhiteSpace(licenseVariant) ? "Personal" : licenseVariant), ADOEditorUtility.MapRef().m_ProcSerializer);
				GUILayout.FlexibleSpace();
			}
			if (!string.IsNullOrWhiteSpace(licensedToDisplayName))
			{
				using (new GUILayout.HorizontalScope(GUI.skin.box))
				{
					GUILayout.Label("Authorized For: " + licensedToDisplayName, ADOEditorUtility.MapRef()._IdentifierMethod);
					return;
				}
			}
		}
	}

	private static bool FlushConfiguration(EditorWindow first = null, float visitor = 0f)
	{
		if (!isLicensed)
		{
			if (Event.current.type == EventType.Repaint)
			{
				VisitConfiguration();
			}
			if ((object)first != null)
			{
				ADOEditorUtility.getterSerializer.Draw(first, visitor);
			}
			DrawAnnouncementBanner();
			if (isActivatingLicense || isVerifyingLicense)
			{
				DrawPanelHeader(isActivatingLicense ? "Activating License..." : "Verifying License...", "Please wait till this finishes processing.");
				return false;
			}
			if (!showingTransferPanel)
			{
				if (!licenseKeyEntryRequired || licenseCheckRetryOffered)
				{
					DrawPanelHeader("Check for License", "This will check for whether you already have a license for your device");
					if (ADOEditorUtility.LoginStatus(licenseCheckRetryOffered ? "Retry" : "Check", EditorStyles.toolbarButton))
					{
						AssetConfiguration(testkey: true);
					}
					return false;
				}
				DrawPanelHeader("Enter your license key", "Enter the license key you received with your purchase here. If your license was already activated, click on 'Transfer License'. For support, contact @Dreadrith.");
				bool flag = ConnectConfiguration(applyvar1: false);
				if (ExcludeSerializer().Length > 0)
				{
					EditorGUILayout.HelpBox(ExcludeSerializer(), MessageType.Error);
				}
				bool flag2 = AddConfiguration() && !GetSerializer();
				flag &= flag2 && !licenseCheckedThisSession;
				using (new EditorGUI.DisabledScope(!flag2))
				{
					if (ADOEditorUtility.PatchStatus("Activate") || flag)
					{
						PopConfiguration();
					}
				}
				DrawToolHeader(CreateConfiguration);
				return false;
			}
			ExcludeConfiguration();
			return false;
		}
		if (feedbackPanelOpen)
		{
			AwakeConfiguration();
			return false;
		}
		if (!bugReporterOpen)
		{
			return true;
		}
		BugReporter.DrawWindow();
		return false;
	}

	private static void ExcludeConfiguration()
	{
		DrawPanelHeader("Transferring License", "This is for moving your license to a new device or re-activating it in case it fails to recognize your device.");
		if (transferCodeSent)
		{
			EditorGUILayout.HelpBox("A 6-digit verification code was sent to " + transferTargetEmail + ".\nIf this is not your email address, please contact support.\nIf you don't see the verification email, please check your spam folder.", MessageType.Info);
			transferVerificationCode = EditorGUILayout.TextField("Verification Code", transferVerificationCode);
			transferVerificationCode = Regex.Replace(transferVerificationCode, "[^0-9]", string.Empty, RegexOptions.Multiline);
			EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(!Regex.IsMatch(transferVerificationCode, "[0-9]{6}") || isConfirmingTransfer);
			try
			{
				if (ADOEditorUtility.PatchStatus(isConfirmingTransfer ? "Transferring..." : "Transfer License"))
				{
					SetIdentifier();
				}
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
		}
		else
		{
			EditorGUILayout.HelpBox("Use this to move your own license from another device.\nAfter entering your license key, press 'Send Verification Code' to send a 6-digit code to the email address associated with the license key.", MessageType.Info);
			EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(isRequestingTransferCode);
			try
			{
				ConnectConfiguration(applyvar1: true);
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
			if (ExcludeSerializer().Length > 0)
			{
				EditorGUILayout.HelpBox(ExcludeSerializer(), MessageType.Error);
			}
			disabledScope = new EditorGUI.DisabledScope(!FindConfiguration() || isRequestingTransferCode);
			try
			{
				if (ADOEditorUtility.PatchStatus((!isRequestingTransferCode) ? "Send Verification Code" : "Sending Verification Code..."))
				{
					VerifyIdentifier();
				}
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
		}
		DrawToolHeader(CreateConfiguration);
	}

	private static void DrawPanelHeader(string var1, string ord)
	{
		using (new GUILayout.HorizontalScope(ADOEditorUtility.MapRef()._MerchantSerializer))
		{
			GUILayout.Label(string.Empty, GUILayout.Width(17f), GUILayout.Height(17f));
			GUILayout.Label(var1, ADOEditorUtility.MapRef().m_WriterSerializer);
			GUILayout.Label(new GUIContent(ADOEditorUtility.CustomizeRef()._ModelSerializer)
			{
				tooltip = ord
			}, ADOEditorUtility.MapRef().m_ProducerSerializer, GUILayout.Width(17f), GUILayout.Height(17f));
		}
	}

	private static bool ConnectConfiguration(bool applyvar1)
	{
		using (new GUILayout.HorizontalScope())
		{
			string text = "ADOverhaulLicenseField";
			if (ADOEditorUtility.SubmitPressed(text))
			{
				GUI.FocusControl(null);
				return true;
			}
			if (ADOEditorUtility.CancelPressed(text))
			{
				GUI.FocusControl(null);
			}
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				if (applyvar1)
				{
					EditorGUILayout.PrefixLabel("License Key");
				}
				GUI.SetNextControlName(text);
				licenseKey = EditorGUILayout.TextField(string.Empty, licenseKey).Trim();
				ADOEditorUtility.AwakeStatus("License Key", string.IsNullOrWhiteSpace(licenseKey), 80f);
			}
			if (!licenseCheckedThisSession && AddConfiguration() && !GetSerializer())
			{
				licenseCheckedThisSession = true;
				return true;
			}
		}
		return false;
	}

	private static bool FindConfiguration()
	{
		if (showingTransferPanel)
		{
			if (!GetSerializer() && AddConfiguration())
			{
				return ValidateConfiguration();
			}
			return false;
		}
		if (GetSerializer())
		{
			return false;
		}
		return AddConfiguration();
	}

	private static bool AddConfiguration()
	{
		return Regex.Match(licenseKey, "^[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}$").Success;
	}

	private static bool ValidateConfiguration()
	{
		if (transferCodeSent)
		{
			return Regex.Match(transferVerificationCode, "^[a-zA-Z0-9]{6}$").Success;
		}
		return true;
	}

	private static void CreateConfiguration()
	{
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.FlexibleSpace();
			if (ADOEditorUtility.InterruptStatus(showingTransferPanel ? "Activate License" : "Transfer License"))
			{
				showingTransferPanel = !showingTransferPanel;
			}
		}
	}

	private static string IncludeConfiguration(IEnumerable<(string, string)> v)
	{
		StringBuilder stringBuilder = new StringBuilder("{");
		bool flag = true;
		foreach (var (text, text2) in v)
		{
			if (!flag)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append("\"" + text + "\":\"" + text2 + "\"");
			flag = false;
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	private static HttpWebRequest RevertConfiguration(string instance)
	{
		HttpWebRequest httpWebRequest = WebRequest.CreateHttp(instance);
		httpWebRequest.Method = "POST";
		httpWebRequest.Accept = "application/json";
		httpWebRequest.ContentType = "application/json";
		return httpWebRequest;
	}

	private static async Task<JsonObject> RunIdentifier(string setup, string vis)
	{
		JsonObject _DispatcherContext = default(JsonObject);
		await Task.Run(async delegate
		{
			HttpWebRequest httpWebRequest = RevertConfiguration(setup);
			using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				await streamWriter.WriteAsync(vis);
			}
			HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
			string v = await streamReader.ReadToEndAsync();
			streamReader.Dispose();
			_DispatcherContext = new JsonObject(v);
		});
		return _DispatcherContext;
	}

	private static Task<JsonObject> OrderIdentifier(string first)
	{
		return RunIdentifier("https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand", first);
	}

	private static void RepaintOpenWindowsDelayed()
	{
		ADOEditorUtility.DelayCall(RepaintOpenWindows);
	}

	private static void RepaintOpenWindows()
	{
		ADOverhaulWindow[] array = Resources.FindObjectsOfTypeAll<ADOverhaulWindow>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Repaint();
		}
	}

	private static void DrawCreditLink()
	{
		using (new GUIColorScope(GUIColorScope.ColoringType.BG, Color.clear))
		{
			if (GUILayout.Button(new GUIContent("Made By @Dreadrith ♡", "https://dreadrith.com/links"), ADOEditorUtility.MapRef()._ContextMethod))
			{
				Application.OpenURL("https://dreadrith.com/links");
			}
			ADOEditorUtility.MarkAsLink();
		}
	}

	internal static bool LogWarning(string config, bool wantselection = true)
	{
		return Log(config, CustomLogType.Warning, wantselection);
	}

	internal static bool LogError(string instance, bool ignorereg = true)
	{
		return Log(instance, CustomLogType.Error, ignorereg);
	}

	internal static bool Log(string def, CustomLogType reg = CustomLogType.Regular, bool includefilter = true)
	{
		if (includefilter)
		{
			while (true)
			{
				Color color = ((reg == CustomLogType.Regular) ? ADOEditorUtility.validColor : ((reg != CustomLogType.Warning) ? ADOEditorUtility.errorColor : ADOEditorUtility.warningColor));
				string message = "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">[ADOverhaul]</color> " + def.Replace("\\n", "\n");
				switch (reg)
				{
				default:
					continue;
				case CustomLogType.Error:
					UnityEngine.Debug.LogError(message);
					break;
				case CustomLogType.Warning:
					UnityEngine.Debug.LogWarning(message);
					break;
				case CustomLogType.Regular:
					UnityEngine.Debug.Log(message);
					break;
				}
				break;
			}
		}
		return includefilter;
	}

	internal static void ThrowError(string task, bool removecust = true)
	{
		if (removecust)
		{
			throw new Exception("<color=#" + ColorUtility.ToHtmlStringRGB(ADOEditorUtility.errorColor) + ">[ADOverhaul]</color> " + task);
		}
	}

	private static void VerifyIdentifier()
	{
		string message = "License transfer is subject to the Terms of Service.\nLicense will stop working on the device it was previously activated on.\nYou will not be able to transfer back or again for 30 days.";
		switch (EditorUtility.DisplayDialogComplex("Terms of Service", message, "Continue", "Terms of Service", "Cancel"))
		{
		case 0:
			isRequestingTransferCode = true;
			CloneConfiguration(delegate
			{
				List<(string, string)> list = CountConfiguration("transferlicenserequest");
				StartConfiguration(list);
				OrderIdentifier(IncludeConfiguration(list.ToArray())).HandleTask(delegate(JsonObject response)
				{
					_003C_003Ec__DisplayClass179_0 _003C_003Ec__DisplayClass179_ = new _003C_003Ec__DisplayClass179_0();
					_003C_003Ec__DisplayClass179_.composerContext = response;
					isRequestingTransferCode = false;
					QueryConfiguration(_003C_003Ec__DisplayClass179_.composerContext, _003C_003Ec__DisplayClass179_.RestartReg);
				}, delegate(Exception exception)
				{
					isRequestingTransferCode = false;
					Log($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
				}, null, null, RepaintOpenWindowsDelayed);
			}, ispred: true);
			break;
		case 1:
			Application.OpenURL("https://dreadrith.com/license-tos");
			break;
		}
	}

	private static void SetIdentifier()
	{
		isConfirmingTransfer = true;
		CloneConfiguration(delegate
		{
			List<(string, string)> list = CountConfiguration("transferlicenseconfirm");
			list.Add(("verification_code", transferVerificationCode));
			StartConfiguration(list);
			OrderIdentifier(IncludeConfiguration(list.ToArray())).HandleTask(delegate(JsonObject response)
			{
				isConfirmingTransfer = false;
				QueryConfiguration(response, delegate
				{
					showingTransferPanel = false;
					transferCodeSent = false;
					licenseKeyEntryRequired = false;
					AssetConfiguration(testkey: true);
				});
			}, delegate(Exception exception)
			{
				isConfirmingTransfer = false;
				Log($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, RepaintOpenWindowsDelayed);
		}, ispred: true);
	}

	[SpecialName]
	private static bool ConnectSerializer()
	{
		return ADOSettings.Instance().u_updateDay == RemoveConfiguration();
	}

	private static void DrawToolHeader(Action first = null, Action<GenericMenu> col = null)
	{
		using (new GUILayout.VerticalScope(GUI.skin.box))
		{
			using (new GUILayout.HorizontalScope())
			{
				if (ADOEditorUtility.IconButton(ADOEditorUtility.CustomizeRef().m_SpecificationSerializer))
				{
					ShowContextMenu(col);
				}
				if (!ADOSettings.Instance().u_updateHidden && updateAvailable && ADOEditorUtility.IconButton(ADOEditorUtility.CustomizeRef()._ParameterSerializer))
				{
					updateFoldout.target = !updateFoldout.target;
				}
				GUILayout.Label("v" + version, ADOEditorUtility.MapRef().m_AuthenticationMethod, GUILayout.ExpandWidth(expand: false));
				if (first == null)
				{
					GUILayout.FlexibleSpace();
					DrawCreditLink();
				}
				else
				{
					first();
				}
			}
			if (updateAvailable)
			{
				DrawUpdateBanner();
			}
		}
	}

	private static void ShowContextMenu(Action<GenericMenu> ident = null)
	{
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Check For Update"), on: false, (!isCheckingForUpdate && !hasCheckedForUpdate) ? ((GenericMenu.MenuFunction)delegate
		{
			SessionState.EraseString("No1lKII9IzcBAbihub6nCg==updateinfo");
			FillIdentifier();
		}) : null);
		if (isLicensed)
		{
			genericMenu.AddItem(new GUIContent("Send Feedback"), feedbackPanelOpen, delegate
			{
				feedbackPanelOpen.Toggle();
			});
		}
		if (isLicensed)
		{
			if (ident != null)
			{
				ident(genericMenu);
				genericMenu.AddSeparator(string.Empty);
			}
			genericMenu.AddSeparator(string.Empty);
			genericMenu.AddItem(new GUIContent("Verify/On Display"), ADOSettings.Instance().a_VerifyOnDisplay, delegate
			{
				ADOSettings.Instance().a_VerifyOnDisplay.Toggle();
				ADOSettings.Instance().a_VerifyOnProjectLoad.SetValue(nores: false);
			});
			genericMenu.AddItem(new GUIContent("Verify/On Project Load"), ADOSettings.Instance().a_VerifyOnProjectLoad, delegate
			{
				ADOSettings.Instance().a_VerifyOnProjectLoad.Toggle();
				ADOSettings.Instance().a_VerifyOnDisplay.SetValue(nores: false);
			});
		}
		genericMenu.AddSeparator(string.Empty);
		if (!string.IsNullOrWhiteSpace(""))
		{
			genericMenu.AddItem(new GUIContent("Documentation"), on: false, delegate
			{
				Application.OpenURL("");
			});
		}
		if (isLicensed)
		{
			if (extraMenuLinks.Length != 0)
			{
				if (extraMenuLinks.Length <= 1)
				{
					genericMenu.AddItem(new GUIContent(extraMenuLinks[0].Item1), on: false, delegate
					{
						Application.OpenURL(extraMenuLinks[0].Item2);
					});
				}
				else
				{
					(string, string)[] array = extraMenuLinks;
					for (int num = 0; num < array.Length; num++)
					{
						(string, string) tuple = array[num];
						string item = tuple.Item1;
						string _AnnotationContext = tuple.Item2;
						string text = "Samples/" + item;
						genericMenu.AddItem(new GUIContent(text), on: false, delegate
						{
							Application.OpenURL(_AnnotationContext);
						});
					}
				}
			}
			if (!string.IsNullOrWhiteSpace(""))
			{
				genericMenu.AddItem(new GUIContent("Changelog"), on: false, delegate
				{
					Application.OpenURL("");
				});
			}
		}
		genericMenu.AddItem(new GUIContent("ToS and Privacy Policy"), on: false, delegate
		{
			Application.OpenURL("https://dreadrith.com/license-tos");
		});
		genericMenu.ShowAsContext();
	}

	private static void DrawUpdateBanner(bool isres = true)
	{
		if ((bool)ADOSettings.Instance().u_updateHidden)
		{
			return;
		}
		updateFoldout.FadeGroup(delegate
		{
			if (isres)
			{
				ADOEditorUtility.Separator();
			}
			EditorGUILayout.HelpBox($"Version {ADOSettings.Instance().u_updateVersion}\n--------------\n{ADOSettings.Instance().u_updateMessage}", MessageType.Info);
			bool flag = !string.IsNullOrWhiteSpace(ADOSettings.Instance().u_updateLink);
			bool flag2 = !string.IsNullOrWhiteSpace(ADOSettings.Instance().u_updateChangelog);
			using (new GUILayout.HorizontalScope())
			{
				if (flag)
				{
					using (new EditorGUI.DisabledScope(isDownloadingUpdate))
					{
						if (ADOEditorUtility.LoginStatus("Download Update", EditorStyles.toolbarButton))
						{
							LogoutIdentifier();
						}
					}
				}
				if (flag2 && ADOEditorUtility.CallStatus(new GUIContent("Open Changelog", ADOSettings.Instance().u_updateChangelog), EditorStyles.toolbarButton))
				{
					Application.OpenURL(ADOSettings.Instance().u_updateChangelog);
				}
				if (ADOEditorUtility.LoginStatus("Skip for Today", EditorStyles.toolbarButton))
				{
					ADOSettings.Instance().u_updateHidden.SetValue(nores: true);
				}
			}
		}, RepaintOpenWindows);
	}

	private static void DrawAnnouncementBanner()
	{
		if ((bool)ADOSettings.Instance().u_announcementHidden || string.IsNullOrWhiteSpace(ADOSettings.Instance().u_announcement))
		{
			return;
		}
		using (new GUILayout.VerticalScope(EditorStyles.helpBox))
		{
			Rect _CallbackContext = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(expand: true), GUILayout.Height(24f));
			Rect setup = _CallbackContext;
			GUI.Label(setup.SliceLeft(24f, isres: true), ADOEditorUtility.CustomizeRef()._ObjectSerializer);
			GUI.Label(setup, "Announcement", ADOEditorUtility.MapRef().m_ExceptionSerializer);
			announcementFoldout.FadeGroup(delegate
			{
				_CallbackContext.height += 18f;
				ADOEditorUtility.Separator();
				EditorGUILayout.HelpBox(ADOSettings.Instance().u_announcement, MessageType.Info);
				using (new GUILayout.HorizontalScope())
				{
					if (!string.IsNullOrWhiteSpace(ADOSettings.Instance().u_announcementLink) && ADOEditorUtility.LoginStatus(ADOSettings.Instance().u_announcementLinkName, EditorStyles.toolbarButton))
					{
						Application.OpenURL(ADOSettings.Instance().u_announcementLink);
					}
					if (isLicensed && ADOEditorUtility.LoginStatus("Hide", EditorStyles.toolbarButton))
					{
						ADOSettings.Instance().u_announcementHidden.SetValue(nores: true);
						ADOSettings.Instance().u_announcementHiddenDate.SetValue(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
					}
				}
			}, RepaintOpenWindows);
			if (ADOEditorUtility.ClickArea(_CallbackContext))
			{
				announcementFoldout.target = !announcementFoldout.target;
			}
		}
	}

	[InitializeOnLoadMethod]
	private static void MapIdentifier()
	{
		if (!ConnectSerializer() || string.IsNullOrWhiteSpace(ADOSettings.Instance().u_updateVersion.GetValue()))
		{
			ADOEditorUtility.DelayCall(delegate
			{
				CancelIdentifier(isparam: false);
			});
		}
		else
		{
			ApplyCachedUpdateInfo(iskey: false);
		}
	}

	private static void FillIdentifier()
	{
		CancelIdentifier(isparam: true);
	}

	private static void CancelIdentifier(bool isparam)
	{
		if ((!isparam && ConnectSerializer()) || hasCheckedForUpdate || isCheckingForUpdate)
		{
			return;
		}
		isCheckingForUpdate = true;
		OrderIdentifier(IncludeConfiguration(new List<(string, string)>
		{
			("command", "getdownloadinfo"),
			("product_id", "No1lKII9IzcBAbihub6nCg=="),
			("version", version.ToString())
		})).HandleTask(delegate(JsonObject response)
		{
			hasCheckedForUpdate = true;
			string value = ADOSettings.Instance().u_announcement.GetValue();
			using (new ADOSettings.SettingsDeferScope())
			{
				ADOSettings.Instance().u_updateLink.SetValue(response.Item("download_link"));
				ADOSettings.Instance().u_updateMessage.SetValue(response.Item("download_message"));
				ADOSettings.Instance().u_updateChangelog.SetValue(response.Item("changelog_link"));
				ADOSettings.Instance().u_updateVersion.SetValue(response.Item("version"));
				ADOSettings.Instance().u_updateDay.SetValue(RemoveConfiguration());
				ADOSettings.Instance().u_announcement.SetValue(response.Item("announcement"));
				if (!string.IsNullOrWhiteSpace(ADOSettings.Instance().u_announcement))
				{
					ADOSettings.Instance().u_announcement.SetValue(ADOSettings.Instance().u_announcement.GetValue().Replace("\\\\n", "\n").Replace("\\n", "\n"));
				}
				ADOSettings.Instance().u_announcementLink.SetValue(response.Item("announcement_link"));
				ADOSettings.Instance().u_announcementLinkName.SetValue(response.Item("announcement_link_name"));
			}
			if (value != ADOSettings.Instance().u_announcement.GetValue())
			{
				ADOSettings.Instance().u_announcementHidden.SetValue(nores: false);
			}
			ApplyCachedUpdateInfo(isparam);
		}, delegate(Exception exc)
		{
			Log($"Something went wrong while checking for an update!\n\n{exc}", CustomLogType.Error);
		}, null, null, delegate
		{
			isCheckingForUpdate = false;
			RepaintOpenWindowsDelayed();
		});
	}

	private static void LogoutIdentifier()
	{
		isDownloadingUpdate = true;
		UnityWebRequest policyContext = new UnityWebRequest(ADOSettings.Instance().u_updateLink);
		policyContext.downloadHandler = new DownloadHandlerFile("Assets/ADOverhaul.unitypackage");
		policyContext.SendWebRequest().completed += delegate
		{
			isDownloadingUpdate = false;
			string text = "Assets/ADOverhaul.unitypackage";
			if (policyContext.isNetworkError || policyContext.isHttpError)
			{
				AssetDatabase.ImportAsset(text);
				AssetDatabase.DeleteAsset(text);
				policyContext.Dispose();
				throw new Exception(policyContext.error);
			}
			AssetDatabase.ImportPackage(text, interactive: true);
			AssetDatabase.DeleteAsset(text);
			policyContext.Dispose();
		};
	}

	private static void ApplyCachedUpdateInfo(bool iskey)
	{
		if ((bool)ADOSettings.Instance().u_announcementHidden)
		{
			if (DateTime.TryParse(ADOSettings.Instance().u_announcementHiddenDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result))
			{
				ADOSettings.Instance().u_announcementHidden.SetValue((DateTime.UtcNow - result).TotalDays < 7.0);
			}
			else
			{
				ADOSettings.Instance().u_announcementHidden.SetValue(nores: false);
			}
		}
		if (!(version < new SemVer(ADOSettings.Instance().u_updateVersion.GetValue())))
		{
			if (iskey)
			{
				Log("Up to date!");
				Task.Run(async delegate
				{
					await Task.Delay(3000);
					ADOSettings.Instance().u_updateHidden.SetValue(nores: true);
					RepaintOpenWindowsDelayed();
				});
			}
			else
			{
				ADOSettings.Instance().u_updateHidden.SetValue(nores: true);
			}
			return;
		}
		updateAvailable = true;
		if (iskey)
		{
			ADOSettings.Instance().u_updateHidden.SetValue(nores: false);
			updateFoldout.target = true;
		}
		if (!ADOSettings.Instance().u_updateHidden)
		{
			Log($"Update Available! <b>(v{ADOSettings.Instance().u_updateVersion})</b>");
		}
	}

	internal static void DrawFoldoutBox(string v, AnimBool caller, Action dir, Action task2)
	{
		using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(v, ADOEditorUtility.MapRef()._ConsumerMethod);
				dir?.Invoke();
			}
			if (ADOEditorUtility.ClickArea())
			{
				caller.target = !caller.target;
				if (!ADOSettings.Instance().editorAnimatedFoldouts)
				{
					caller.value = caller.target;
				}
			}
			caller.FadeGroup(task2);
		}
	}

	internal static void DrawTitledOverlay(SceneView key, string reg, Action control, float key2, float task3)
	{
		DrawOverlay(key, delegate
		{
			using (new GUILayout.HorizontalScope())
			{
				ADOEditorUtility.IconSpacer();
				GUILayout.FlexibleSpace();
				GUILayout.Label(reg, ADOEditorUtility.MapRef().m_WriterSerializer);
				Rect lastRect = GUILayoutUtility.GetLastRect();
				GUILayout.FlexibleSpace();
				DrawSettingsButton();
				return lastRect;
			}
		}, control, key2, task3);
	}

	internal static void DrawOverlay(SceneView ident, Func<Rect> result, Action consumer, float pol2, float ivk3)
	{
		Rect ivk4 = ident.AddStatus();
		ADOEditorUtility.PositionFlag enumValue = ADOSettings.Instance().toolOverlayAlignment.GetEnumValue<ADOEditorUtility.PositionFlag>();
		bool flag;
		using (new ADOEditorUtility.SceneViewPanel(ident, pol2, ivk3, enumValue, sceneViewPanelResizeHandle))
		{
			Rect rect = result();
			ADOEditorUtility.AddCursorRect(rect, MouseCursor.Pan);
			flag = ADOEditorUtility.HasMouseCapture(rect, tooltipDragControlId);
			if (consumer != null)
			{
				ADOEditorUtility.Separator(2, 0);
				consumer();
			}
		}
		if (flag)
		{
			Handles.BeginGUI();
			ADOSettings.Instance().toolOverlayAlignment.IntValue = (int)ADOEditorUtility.AnchorPicker(enumValue, ivk4);
			Handles.EndGUI();
		}
	}

	internal static void DrawSettingsButton()
	{
		if (ADOEditorUtility.IconButton(ADOEditorUtility.CustomizeRef().fieldSerializer))
		{
			ADOverhaulWindow.ShowWindow();
		}
	}

	[CompilerGenerated]
	internal static void CollectIdentifier(bool testident, ref _003C_003Ec__DisplayClass46_0 col, ref _003C_003Ec__DisplayClass46_1 third)
	{
		if (!testident)
		{
			return;
		}
		if (col.m_MappingContext)
		{
			Undo.RecordObject(col.queueContext, "Adjust Radius");
		}
		else
		{
			Undo.RecordObjects(col.m_ProcessorContext, "Adjust Radius");
		}
		_003C_003Ec__DisplayClass46_2 var = default(_003C_003Ec__DisplayClass46_2);
		var.m_WriterContext = third._TemplateContext - col.m_TokenizerContext[col._ExceptionContext].radius;
		switch (col.m_ValueContext)
		{
		case 2:
		{
			PrintIdentifier(col.m_TokenizerContext[col._ExceptionContext], out col.m_TokenizerContext[col._ExceptionContext].radius, out col.m_TokenizerContext[col._ExceptionContext].height, ref var);
			for (int j = 0; j < col.m_TokenizerContext.Length; j++)
			{
				if (j != col._ExceptionContext)
				{
					col.m_TokenizerContext[j].radius = col.m_TokenizerContext[col._ExceptionContext].radius;
					if (col.m_TokenizerContext[j].shapeType == 0)
					{
						col.m_TokenizerContext[j].height = col.m_TokenizerContext[j].radius * 2f;
					}
					else
					{
						col.m_TokenizerContext[j].height += var.m_WriterContext * 2f;
					}
				}
			}
			break;
		}
		case 1:
			PrintIdentifier(col.m_TokenizerContext[col._ExceptionContext], out col.m_TokenizerContext[col._ExceptionContext].radius, out col.m_TokenizerContext[col._ExceptionContext].height, ref var);
			break;
		default:
		{
			for (int i = 0; i < col.m_TokenizerContext.Length; i++)
			{
				PrintIdentifier(col.m_TokenizerContext[i], out col.m_TokenizerContext[i].radius, out col.m_TokenizerContext[i].height, ref var);
			}
			break;
		}
		}
	}

	[CompilerGenerated]
	internal static void PrintIdentifier(ADOEditorUtility.ShapeSnapshot key, out float counter, out float consumer, ref _003C_003Ec__DisplayClass46_2 var12)
	{
		counter = key.radius + var12.m_WriterContext;
		if (key.shapeType != 0)
		{
			consumer = key.height + var12.m_WriterContext * 2f;
		}
		else
		{
			consumer = counter * 2f;
		}
	}

	[CompilerGenerated]
	internal static void InterruptIdentifier(bool nores, bool forcecounter, ref _003C_003Ec__DisplayClass46_0 pool, ref _003C_003Ec__DisplayClass46_3 v2)
	{
		if (!nores)
		{
			return;
		}
		if (pool.m_MappingContext)
		{
			Undo.RecordObject(pool.queueContext, "Adjust Height");
		}
		else
		{
			Undo.RecordObjects(pool.m_ProcessorContext, "Adjust Height");
		}
		Vector3 vector = ((!forcecounter) ? v2._DicContext : v2.m_ClassContext);
		Vector3 vector2 = ((!forcecounter) ? v2.schemaContext : v2.m_ContainerContext);
		bool flag = (pool.m_ErrorContext - vector2).magnitude < (pool.m_ErrorContext - vector).magnitude;
		float num = (vector2 - vector).magnitude * (float)((!flag) ? 1 : (-1)) * 2f / pool._ProducerContext;
		switch (pool.m_ValueContext)
		{
		case 2:
		{
			pool.m_TokenizerContext[pool._ExceptionContext].height += num;
			for (int j = 0; j < pool.m_TokenizerContext.Length; j++)
			{
				pool.m_TokenizerContext[j].height = pool.m_TokenizerContext[pool._ExceptionContext].height;
			}
			break;
		}
		case 1:
			pool.m_TokenizerContext[pool._ExceptionContext].height += num;
			break;
		default:
		{
			for (int i = 0; i < pool.m_TokenizerContext.Length; i++)
			{
				pool.m_TokenizerContext[i].height += num;
			}
			break;
		}
		}
	}

	[CompilerGenerated]
	internal static void ViewIdentifier<T>(Dictionary<T, T> instance, ref _003C_003Ec__DisplayClass54_0 pol) where T : UnityEngine.Component
	{
		foreach (KeyValuePair<T, T> item in instance)
		{
			originalToClone.Add(item.Key, item.Value);
			cloneToOriginal.Add(item.Value, item.Key);
			cloneHasUnappliedChanges.Add(item.Value, value: false);
			if (selectedObjectsBeforeTest.Contains(item.Key.gameObject))
			{
				pol.bridgeContext.Add(item.Value.transform);
			}
		}
	}

	[CompilerGenerated]
	internal static void PostIdentifier(bool rejectkey, ref _003C_003Ec__DisplayClass86_0 visitor, ref _003C_003Ec__DisplayClass86_1 field)
	{
		Log(rejectkey ? (visitor.serializerSerializer.stringValue + " added to " + field._MethodSerializer.name) : (visitor.serializerSerializer.stringValue + " already exists in " + field._MethodSerializer.name));
	}

	[CompilerGenerated]
	internal static string ListIdentifier(string instance, ref _003C_003Ec__DisplayClass132_1 cont)
	{
		if (string.IsNullOrEmpty(instance))
		{
			return instance;
		}
		ICryptoTransform cryptoTransform = cont._IndexerContext.CreateDecryptor(cont._IndexerContext.Key, cont._IndexerContext.IV);
		byte[] array = Convert.FromBase64String(instance);
		byte[] bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
		return Encoding.UTF8.GetString(bytes);
	}

	[CompilerGenerated]
	internal static string ForgotIdentifier(string reference, ref _003C_003Ec__DisplayClass132_2 counter)
	{
		return Convert.ToBase64String(counter.m_PoolContext.ComputeHash(Encoding.UTF8.GetBytes(reference)));
	}

	[CompilerGenerated]
	internal static string UpdateIdentifier(string v, ref _003C_003Ec__DisplayClass132_5 attr)
	{
		return Convert.ToBase64String(attr._StructContext.ComputeHash(Encoding.UTF8.GetBytes(v)));
	}

	[CompilerGenerated]
	internal static string SearchIdentifier(string last, ref _003C_003Ec__DisplayClass132_4 second)
	{
		if (!string.IsNullOrEmpty(last))
		{
			ICryptoTransform cryptoTransform = second.ruleContext.CreateEncryptor(second.ruleContext.Key, second.ruleContext.IV);
			byte[] bytes = Encoding.UTF8.GetBytes(last);
			return Convert.ToBase64String(cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length));
		}
		return last;
	}

	[CompilerGenerated]
	internal static void LoginIdentifier()
	{
		List<(string, string)> list = CountConfiguration("activatelicense");
		StartConfiguration(list);
		OrderIdentifier(IncludeConfiguration(list.ToArray())).HandleTask(delegate(JsonObject response)
		{
			isActivatingLicense = false;
			QueryConfiguration(response, delegate
			{
				licenseKeyEntryRequired = false;
				ADOSettings.Instance().a_HasSucceededLastVerification.SetValue(nores: true);
				AssetConfiguration(testkey: true);
			});
		}, delegate(Exception exception)
		{
			isActivatingLicense = false;
			Log($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
		}, null, null, RepaintOpenWindowsDelayed);
	}

	[CompilerGenerated]
	internal static string PatchIdentifier(string item)
	{
		_003C_003Ec__DisplayClass138_1 vis = default(_003C_003Ec__DisplayClass138_1);
		vis._StateContext = new AesManaged();
		try
		{
			vis._StateContext.Key = Convert.FromBase64String("3epqD3d1DrDCuf1yV3SkFhrll8jVCc4dbC0P1PhU/NA=");
			vis._StateContext.IV = Convert.FromBase64String("MTOuc+v23iVKtf8SLX3WxQ==");
			return CheckIdentifier(item, ref vis);
		}
		finally
		{
			if (vis._StateContext != null)
			{
				((IDisposable)vis._StateContext).Dispose();
			}
		}
	}

	[CompilerGenerated]
	internal static string CheckIdentifier(string config, ref _003C_003Ec__DisplayClass138_1 vis)
	{
		ICryptoTransform cryptoTransform = vis._StateContext.CreateEncryptor(vis._StateContext.Key, vis._StateContext.IV);
		byte[] bytes = Encoding.UTF8.GetBytes(config);
		return Convert.ToBase64String(cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length));
	}

	[CompilerGenerated]
	internal static string CallIdentifier(string i)
	{
		using AesManaged aesManaged = new AesManaged();
		aesManaged.Key = Convert.FromBase64String("3epqD3d1DrDCuf1yV3SkFhrll8jVCc4dbC0P1PhU/NA=");
		aesManaged.IV = Convert.FromBase64String("MTOuc+v23iVKtf8SLX3WxQ==");
		ICryptoTransform cryptoTransform = aesManaged.CreateDecryptor(aesManaged.Key, aesManaged.IV);
		byte[] array = Convert.FromBase64String(i);
		return Encoding.UTF8.GetString(cryptoTransform.TransformFinalBlock(array, 0, array.Length));
	}

	[CompilerGenerated]
	internal static string RegisterIdentifier(string def, int[] caller)
	{
		foreach (int num in caller)
		{
			if (num > 0)
			{
				def = ChangeIdentifier(def, num);
			}
		}
		return def;
	}

	[CompilerGenerated]
	internal static string ChangeIdentifier(string def, int callerZ)
	{
		int num = 2;
		for (int i = callerZ; i < def.Length; i += callerZ)
		{
			num++;
			if (num == 3)
			{
				int num2 = i + callerZ;
				if (num2 >= def.Length)
				{
					break;
				}
				char c = def[num2];
				def = def.Remove(num2, 1).Insert(num2, def[i].ToString());
				def = def.Remove(i, 1).Insert(i, c.ToString());
				num = 0;
			}
		}
		return def;
	}

	[CompilerGenerated]
	internal static string StopIdentifier(string item)
	{
		return RegisterIdentifier(PatchIdentifier(item), new int[7] { 3, 2, 6, 4, 2, 1, 8 });
	}

	[CompilerGenerated]
	internal static string PushIdentifier(string item)
	{
		return CallIdentifier(RegisterIdentifier(item, new int[7] { 8, 1, 2, 4, 6, 2, 3 }));
	}

	[CompilerGenerated]
	internal static async void PrepareIdentifier(ProcessRunner[] reference, Action visitor, CancellationTokenSource state)
	{
		try
		{
			await Task.Run(delegate
			{
				ProcessRunner[] array = reference;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Run();
				}
			}, state.Token);
			while (!reference.All((ProcessRunner p) => p.isFinished))
			{
				state.Token.ThrowIfCancellationRequested();
				await Task.Delay(50, state.Token);
			}
		}
		finally
		{
			visitor?.Invoke();
		}
	}

	[CompilerGenerated]
	internal static bool ReadIdentifier(string ident, string second, out (List<string>, Dictionary<string, RangeInt>) template)
	{
		template = (new List<string>(), new Dictionary<string, RangeInt>());
		(List<string>, Dictionary<string, RangeInt>) tuple = template;
		List<string> item = tuple.Item1;
		Dictionary<string, RangeInt> item2 = tuple.Item2;
		string[] array = ident.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		bool flag = false;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			if (flag)
			{
				item.Add(text);
			}
			else
			{
				if (text.IndexOf(second, StringComparison.OrdinalIgnoreCase) < 0)
				{
					continue;
				}
				string pattern = "(\\w+?)\\b *";
				MatchCollection matchCollection = Regex.Matches(text, pattern);
				for (int j = 0; j < matchCollection.Count; j++)
				{
					Match match = matchCollection[j];
					if (match.Success)
					{
						string value = match.Groups[1].Value;
						RangeInt value2 = new RangeInt(match.Groups[0].Index, match.Groups[0].Length);
						item2.Add(value, value2);
					}
				}
				flag = true;
			}
		}
		return item.Count > 0;
	}

	[CompilerGenerated]
	internal static bool TestIdentifier((List<string>, Dictionary<string, RangeInt>) key, string b, out string[] rule)
	{
		(List<string>, Dictionary<string, RangeInt>) tuple = key;
		List<string> item = tuple.Item1;
		Dictionary<string, RangeInt> item2 = tuple.Item2;
		rule = new string[item.Count];
		if (item2.TryGetValue(b, out var value))
		{
			for (int i = 0; i < item.Count; i++)
			{
				string text = item[i];
				rule[i] = text.Substring(value.start, value.length).Trim();
			}
			return !rule.All(string.IsNullOrWhiteSpace);
		}
		return false;
	}

	[CompilerGenerated]
	internal static bool InsertIdentifier(string config, string result, out string[] filter)
	{
		string pattern = "(?i).*" + result + ".*?: *(.*)";
		MatchCollection matchCollection = Regex.Matches(config, pattern);
		if (matchCollection.Count == 0)
		{
			filter = Array.Empty<string>();
			return false;
		}
		filter = new string[matchCollection.Count];
		for (int i = 0; i < matchCollection.Count; i++)
		{
			Match match = matchCollection[i];
			filter[i] = match.Groups[1].Value.Trim();
		}
		return !filter.All(string.IsNullOrWhiteSpace);
	}

	[CompilerGenerated]
	internal static string EnableIdentifier(string asset)
	{
		if (asset.Length >= 2)
		{
			return asset;
		}
		return "0" + asset;
	}

	static MethodInfo ListAuthentication(Type type_0, string setup, BindingFlags result)
	{
		return type_0.GetMethod(setup, result);
	}
}
