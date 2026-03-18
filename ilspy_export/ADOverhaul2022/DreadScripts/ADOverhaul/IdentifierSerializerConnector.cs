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

internal sealed class IdentifierSerializerConnector
{
	private sealed class MessageUtilsAttribute : EditorWindow
	{
		private enum EasyDynamicsFunctions
		{
			EasyGrab,
			EasyTouch,
			EasyPat
		}

		private static int m_Param;

		private static readonly string[] prototype = new string[2] { "Easy Dynamics", "Cosmetic" };

		private static readonly ExceptionSingletonStruct.ParserWatcherRule m_Base = new ExceptionSingletonStruct.ParserWatcherRule("https://raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png", addcol: true, "DreadBanner.png");

		private static EasyDynamicsFunctions _Request = EasyDynamicsFunctions.EasyGrab;

		private static bool m_Issuer;

		private static bool facade;

		private static bool m_Composer;

		private static bool m_Annotation;

		private static bool _Code;

		private static bool m_Callback;

		private static bool message;

		internal static MessageUtilsAttribute CallTokenizer;

		[MenuItem("DreadTools/ADOverhaul", false, 6)]
		internal static void CreateSerializer()
		{
			EditorWindow.GetWindow<MessageUtilsAttribute>(utility: false, "Avatar Dynamics Overhaul", focus: true);
		}

		private void OnGUI()
		{
			if (FlushConfiguration(this))
			{
				RevertSerializer();
				ExceptionSingletonStruct.DisableStatus();
				GetConfiguration();
				SortIdentifier();
				ConcatIdentifier();
			}
		}

		private void IncludeSerializer()
		{
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				PushConfiguration();
			}
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				_Request = (EasyDynamicsFunctions)(object)EditorGUILayout.EnumPopup(ExceptionSingletonStruct.CustomizeRef()._MapperSerializer, _Request);
			}
			EditorGUILayout.HelpBox("Under Development", MessageType.Info);
		}

		private void RevertSerializer()
		{
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				_Code = EditorGUILayout.Foldout(_Code, "Editor", toggleOnLabelClick: true);
				if (_Code)
				{
					EditorGUI.indentLevel++;
					RefImporterDescriptor.GetConsumer().editorAnimatedFoldouts.RunUtils(ExceptionSingletonStruct.CustomizeRef().issuerSerializer, null);
					EditorGUI.indentLevel--;
				}
			}
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				m_Callback = EditorGUILayout.Foldout(m_Callback, "Handles", toggleOnLabelClick: true);
				if (m_Callback)
				{
					EditorGUI.indentLevel++;
					using (new GUILayout.HorizontalScope())
					{
						RefImporterDescriptor.GetConsumer().onSceneNameLabels.RunUtils(ExceptionSingletonStruct.CustomizeRef()._FacadeSerializer, null);
						if ((bool)RefImporterDescriptor.GetConsumer().onSceneNameLabels)
						{
							RefImporterDescriptor.GetConsumer().labelColor.OrderPage(GUIContent.none, true);
						}
					}
					RefImporterDescriptor.GetConsumer().generalColor.OrderPage(ExceptionSingletonStruct.CustomizeRef().annotationSerializer, true);
					RefImporterDescriptor.GetConsumer().activeColor.OrderPage(ExceptionSingletonStruct.CustomizeRef().m_CodeSerializer, true);
					RefImporterDescriptor.GetConsumer().inactiveColor.OrderPage(ExceptionSingletonStruct.CustomizeRef()._CallbackSerializer, true);
					RefImporterDescriptor.GetConsumer().mixedColor.OrderPage(ExceptionSingletonStruct.CustomizeRef()._MessageSerializer, true);
					RefImporterDescriptor.GetConsumer().selectionColor.OrderPage(ExceptionSingletonStruct.CustomizeRef().policySerializer, true);
					RefImporterDescriptor.GetConsumer().handleSizeMultiplier.LogoutUtils(ExceptionSingletonStruct.CustomizeRef().m_RequestSerializer, true, null);
					EditorGUI.indentLevel--;
				}
			}
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				message = EditorGUILayout.Foldout(message, "Overlay", toggleOnLabelClick: true);
				if (!message)
				{
					return;
				}
				EditorGUI.indentLevel++;
				using (new GUILayout.HorizontalScope())
				{
					RefImporterDescriptor.GetConsumer().onSceneToolSelection.RunUtils(new GUIContent("Tool Overlay", "Displays the tool selection overlay on the scene view."), null);
					using (new EditorGUI.DisabledScope(!RefImporterDescriptor.GetConsumer().onSceneToolSelection))
					{
						RefImporterDescriptor.GetConsumer().toolSelectionOverlayAlignment.PrepareUtils<ExceptionSingletonStruct.PositionFlag>("Position", isb: false, null, Array.Empty<GUILayoutOption>());
					}
				}
				using (new GUILayout.HorizontalScope())
				{
					RefImporterDescriptor.GetConsumer().onSceneEditingOverlay.RunUtils(ExceptionSingletonStruct.CustomizeRef().mappingSerializer, null);
					using (new EditorGUI.DisabledScope(!RefImporterDescriptor.GetConsumer().onSceneEditingOverlay))
					{
						RefImporterDescriptor.GetConsumer().toolOverlayAlignment.PrepareUtils<ExceptionSingletonStruct.PositionFlag>("Position", isb: false, null, Array.Empty<GUILayoutOption>());
					}
				}
				RefImporterDescriptor.GetConsumer().onSceneTooltip.RunUtils(ExceptionSingletonStruct.CustomizeRef().queueSerializer, null);
				EditorGUI.indentLevel--;
			}
		}

		private void OnEnable()
		{
			PrintConfiguration(ref m_Predicate, ref _Collection, LogoutConfiguration);
		}

		internal static bool QueryTokenizer()
		{
			return (object)CallTokenizer == null;
		}
	}

	private static class Policy
	{
		internal struct BroadcasterContextEntry
		{
			internal string order;

			internal ushort _Container;

			internal ushort schema;

			internal string _Bridge;
		}

		private static string m_Mapping;

		private static bool queue;

		private static bool processor;

		private static bool _Tokenizer;

		private static bool _Exception;

		private static BroadcasterContextEntry? m_Value;

		private static BroadcasterContextEntry? error;

		private static Action producer;

		private static ushort template;

		internal static bool m_Writer;

		internal static readonly HashSet<BroadcasterContextEntry> m_Class = new HashSet<BroadcasterContextEntry>();

		internal static Policy CalculateTokenizer;

		[SpecialName]
		private static float VisitMethod()
		{
			return (float)(int)template / 1f;
		}

		internal static void InvokeMethod(Action ident, ushort IDcol = 0, string util = "", ushort removeSPEC2At = 0, bool deleteord3 = false, string info4 = "")
		{
			CustomizeMethod(ident, null, IDcol, util, removeSPEC2At, deleteord3, info4);
		}

		internal static void CustomizeMethod(Action task, Action selection, ushort filter_high = 0, string info2 = "", ushort no__reg3 = 0, bool requirest4 = false, string ivk5 = "")
		{
			producer = selection;
			if (filter_high > 0)
			{
				CancelMethod(filter_high, info2, no__reg3);
			}
			try
			{
				task();
			}
			catch (Exception param)
			{
				if (m_Writer)
				{
					throw;
				}
				ConcatMethod(param, requirest4, ivk5);
				CompilationPipeline.compilationStarted -= WriteMethod;
				CompilationPipeline.compilationStarted += WriteMethod;
				throw;
			}
		}

		private static void ConcatMethod(Exception param, bool getcfg = false, string serv = "")
		{
			if (!error.HasValue || m_Class.Contains(error.Value))
			{
				return;
			}
			m_Mapping = string.Empty;
			queue = false;
			_Tokenizer = false;
			processor = false;
			m_Value = new BroadcasterContextEntry
			{
				order = error.Value.order,
				_Container = error.Value._Container,
				schema = error.Value.schema,
				_Bridge = param.Message
			};
			if (getcfg)
			{
				switch (EditorUtility.DisplayDialogComplex("Error!", string.IsNullOrWhiteSpace(serv) ? "An error has occurred! Do you want to try to find a solution for it?" : serv, "Find Solution", "Close", "Ignore"))
				{
				case 2:
					m_Class.Add(m_Value.Value);
					WriteMethod(null);
					break;
				case 1:
					WriteMethod(null);
					break;
				case 0:
					m_Class.Add(m_Value.Value);
					RemoveSerializer(isi: true);
					break;
				}
			}
		}

		internal static void MapMethod()
		{
			if (!FillMethod())
			{
				return;
			}
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(ExceptionSingletonStruct.CustomizeRef().reponseSerializer, ExceptionSingletonStruct.MapRef().m_ProducerSerializer);
				GUILayout.Label("An error has occurred! Do you want to report it?", EditorStyles.boldLabel);
				if (ExceptionSingletonStruct.PatchStatus("Ignore"))
				{
					SelectMethod(moveinstance: false);
				}
				if (ExceptionSingletonStruct.PatchStatus("Find Solution"))
				{
					SelectMethod(moveinstance: true);
				}
			}
			ExceptionSingletonStruct.DisableStatus();
		}

		internal static bool FillMethod()
		{
			if (m_Value.HasValue)
			{
				if (m_Class.Contains(m_Value.Value))
				{
					m_Value = null;
					return false;
				}
				return true;
			}
			return false;
		}

		internal static void CancelMethod(ushort version_item, string ivk = "", ushort idx_util = 0)
		{
			error = new BroadcasterContextEntry
			{
				_Container = version_item,
				order = ivk,
				schema = idx_util
			};
		}

		internal static void LogoutMethod()
		{
			m_Mapping = string.Empty;
			queue = false;
			m_Writer = false;
			template = 0;
			error = null;
		}

		internal static void SetupMethod()
		{
			RemoveSerializer(_Service && m_Value.HasValue);
			if (!_Tokenizer)
			{
				_Tokenizer = true;
				_Exception = true;
				List<(string, string)> list = CountConfiguration("findsolution", new(string, string)[4]
				{
					("bug_id", m_Value.Value._Container.ToString()),
					("bug_version", m_Value.Value.schema.ToString()),
					("bug_name", m_Value.Value.order),
					("bug_exception", Uri.EscapeUriString(m_Value.Value._Bridge))
				});
				StartConfiguration(list);
				OrderIdentifier(IncludeConfiguration(list.ToArray())).CreateProcess(delegate(ParamsIdentifier response)
				{
					bool flag = response.PublishConsumer("success");
					string text = response.PublishConsumer("message");
					processor = true;
					if (!string.IsNullOrWhiteSpace(text))
					{
						NewIdentifier(text, (!flag) ? CustomLogType.Warning : CustomLogType.Regular);
					}
					m_Mapping = response.PublishConsumer("solution");
					queue = response.PublishConsumer("complete");
				}, UnityEngine.Debug.LogException, null, null, delegate
				{
					_Exception = false;
					CalculateIdentifier();
				});
			}
			InitConfiguration((!_Exception) ? "Bug Reporter" : "Finding a solution...", "If you have found a bug, please report it here!\nNote that the report is not anonymous. Abuse may result in blacklisting.");
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				if (_Exception)
				{
					if (ExceptionSingletonStruct.LoginStatus("Cancel", EditorStyles.toolbarButton))
					{
						RemoveSerializer(isi: false);
					}
					return;
				}
				if (processor)
				{
					if (string.IsNullOrWhiteSpace(m_Mapping))
					{
						using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.FG, ExceptionSingletonStruct._EventSerializer))
						{
							GUILayout.Label("No solution Found! Please write the steps to reproduce this issue below:");
						}
						thread = EditorGUILayout.TextArea(thread, GUILayout.MinHeight(54f));
						if (!string.IsNullOrWhiteSpace(thread) && thread.Length > 2000)
						{
							thread = thread.Substring(0, 2000);
						}
						if (!string.IsNullOrWhiteSpace(m_Mapping))
						{
							return;
						}
						using (new GUILayout.HorizontalScope())
						{
							if (ExceptionSingletonStruct.PatchStatus("Cancel", GUILayout.ExpandWidth(expand: false)))
							{
								RemoveSerializer(isi: false);
							}
							using (new EditorGUI.DisabledScope(getter))
							{
								if (!ExceptionSingletonStruct.PatchStatus("Report Issue"))
								{
									return;
								}
								List<(string, string)> list2 = CountConfiguration("reportbug", new(string, string)[5]
								{
									("bug_id", m_Value.Value._Container.ToString()),
									("bug_version", m_Value.Value.schema.ToString()),
									("bug_name", m_Value.Value.order),
									("bug_exception", m_Value.Value._Bridge),
									("feedback", Uri.EscapeUriString(thread))
								});
								StartConfiguration(list2);
								getter = true;
								OrderIdentifier(IncludeConfiguration(list2.ToArray())).CreateProcess(delegate(ParamsIdentifier response)
								{
									bool flag = response.PublishConsumer("success");
									string text = response.PublishConsumer("message");
									if (!string.IsNullOrEmpty(text))
									{
										NewIdentifier(text, (!flag) ? CustomLogType.Warning : CustomLogType.Regular);
									}
								}, UnityEngine.Debug.LogException, null, null, delegate
								{
									RemoveSerializer(isi: false);
									getter = false;
									CalculateIdentifier();
								});
								return;
							}
						}
					}
					if (!queue)
					{
						using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.FG, ExceptionSingletonStruct._EventSerializer))
						{
							GUILayout.Label("Known issue! Details:");
						}
					}
					else
					{
						using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.FG, ExceptionSingletonStruct._ObserverSerializer))
						{
							GUILayout.Label("Solution Found!");
						}
					}
					EditorGUILayout.Space();
					EditorGUILayout.SelectableLabel(m_Mapping, GUI.skin.label, GUILayout.ExpandHeight(expand: false));
					if (ExceptionSingletonStruct.PatchStatus("Ok"))
					{
						RemoveSerializer(isi: false);
					}
					return;
				}
				using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
				{
					GUILayout.Label(ExceptionSingletonStruct.CustomizeRef().reponseSerializer, ExceptionSingletonStruct.MapRef().m_ProducerSerializer);
					using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.FG, ExceptionSingletonStruct._BroadcasterSerializer))
					{
						GUILayout.Label("There was an issue contacting the server for a solution.");
					}
				}
				if (ExceptionSingletonStruct.PatchStatus("Cancel"))
				{
					RemoveSerializer(isi: false);
				}
			}
		}

		internal static void SelectMethod(bool moveinstance)
		{
			if (FillMethod() && m_Value.HasValue)
			{
				if (m_Class.Contains(m_Value.Value))
				{
					m_Value = null;
				}
				RemoveSerializer(moveinstance);
				m_Class.Add(m_Value.Value);
			}
		}

		internal static void WriteMethod(object asset)
		{
			if (m_Value.HasValue)
			{
				uint num4 = default(uint);
				while (true)
				{
					int num;
					int num2;
					if (producer != null)
					{
						num = -1329262136;
						num2 = -1329262136;
					}
					else
					{
						num = -527039267;
						num2 = -527039267;
					}
					int num3 = num ^ (int)(num4 * 669105039);
					while (true)
					{
						switch ((num4 = (uint)(num3 ^ -275398087)) % 4)
						{
						case 0u:
							InvokeMethod(producer, m_Value.Value._Container, m_Value.Value.order, m_Value.Value.schema);
							num3 = ((int)num4 * -644099936) ^ 0x24BBB284;
							continue;
						case 2u:
						case 3u:
							break;
						default:
							goto end_IL_005d;
						}
						break;
					}
					continue;
					end_IL_005d:
					break;
				}
			}
			producer = null;
			CompilationPipeline.compilationStarted -= WriteMethod;
		}

		internal static bool MoveTokenizer()
		{
			return CalculateTokenizer == null;
		}
	}

	private sealed class AuthenticationIdentifier
	{
		private readonly ProcessStartInfo m_ContextIdentifier;

		private Process m_SerializerIdentifier;

		private readonly Action<string> _MethodIdentifier;

		private readonly Action consumerIdentifier;

		private readonly bool utilsIdentifier;

		private string _PageIdentifier;

		private bool _PropertyIdentifier;

		internal bool m_SingletonIdentifier;

		private bool accountIdentifier;

		private static AuthenticationIdentifier ReadTokenizer;

		internal AuthenticationIdentifier(string i, Action<string> second, bool wantfilter = false, bool istask2 = false, Action token3 = null)
		{
			m_ContextIdentifier = new ProcessStartInfo((!wantfilter) ? "powershell.exe" : "cmd.exe")
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardInput = false,
				RedirectStandardOutput = true,
				Arguments = "/c " + i
			};
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
			m_ContextIdentifier.WorkingDirectory = folderPath;
			if (!wantfilter)
			{
				string text = Path.Combine(folderPath, "WindowsPowerShell", "v1.0");
				if (Directory.Exists(text))
				{
					m_ContextIdentifier.WorkingDirectory = text;
				}
			}
			_MethodIdentifier = second;
			consumerIdentifier = token3;
			utilsIdentifier = istask2;
		}

		internal void ComputeMethod()
		{
			_PageIdentifier = string.Empty;
			accountIdentifier = false;
			m_SingletonIdentifier = false;
			_PropertyIdentifier = false;
			m_SerializerIdentifier = new Process();
			m_SerializerIdentifier.StartInfo = m_ContextIdentifier;
			m_SerializerIdentifier.Start();
			try
			{
				do
				{
					_PageIdentifier = m_SerializerIdentifier.StandardOutput.ReadToEnd();
				}
				while (string.IsNullOrEmpty(_PageIdentifier) && !m_SerializerIdentifier.HasExited);
				accountIdentifier = true;
				QueryMethod();
			}
			catch (Exception ex)
			{
				accountIdentifier = false;
				_PageIdentifier = "Failure! Exception: " + ex.Message + "\n" + ex.StackTrace;
				m_SerializerIdentifier?.Close();
				((AuthenticationIdentifier)(object)m_SerializerIdentifier)?.DefineConsumer();
				QueryMethod();
			}
			m_SerializerIdentifier.WaitForExit();
		}

		private void QueryMethod()
		{
			if (_PropertyIdentifier)
			{
				return;
			}
			_PropertyIdentifier = true;
			try
			{
				string text = _PageIdentifier.ToString();
				if (string.IsNullOrWhiteSpace(text))
				{
					text = "Missing";
				}
				if (accountIdentifier || utilsIdentifier)
				{
					_MethodIdentifier(text);
				}
				else
				{
					consumerIdentifier?.Invoke();
				}
			}
			finally
			{
				m_SingletonIdentifier = true;
			}
		}

		void DefineConsumer()
		{
			((System.ComponentModel.Component)this).Dispose();
		}

		internal static bool LoginTokenizer()
		{
			return ReadTokenizer == null;
		}
	}

	[DefaultMember("Item")]
	internal readonly struct ParamsIdentifier
	{
		private readonly string m_ImporterIdentifier;

		private readonly Dictionary<string, PageUtilsWatcher> m_ServerIdentifier;

		internal readonly bool _WatcherIdentifier;

		internal static object LogoutTokenizer;

		internal ParamsIdentifier(string v)
		{
			m_ImporterIdentifier = v;
			MatchCollection matchCollection = Regex.Matches(v, "\"(.*?)\":(?:(?:\"(.*?)\")|(?:(.*?)[,}]))");
			int count = matchCollection.Count;
			if (count != 0)
			{
				_WatcherIdentifier = false;
				m_ServerIdentifier = new Dictionary<string, PageUtilsWatcher>();
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
						m_ServerIdentifier[value] = new PageUtilsWatcher(value2);
					}
				}
			}
			else
			{
				_WatcherIdentifier = true;
				m_ServerIdentifier = null;
			}
		}

		[SpecialName]
		internal PageUtilsWatcher PublishConsumer(string res)
		{
			m_ServerIdentifier.TryGetValue(res, out var value);
			return value;
		}

		public override string ToString()
		{
			return m_ImporterIdentifier;
		}

		public string VerifyConsumer(bool isvalue)
		{
			if (isvalue)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("{");
				foreach (KeyValuePair<string, PageUtilsWatcher> item in m_ServerIdentifier)
				{
					stringBuilder.AppendLine($"{item.Key}: {item.Value},");
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

	internal readonly struct PageUtilsWatcher
	{
		internal readonly string _RegIdentifier;

		internal readonly string m_ProcessIdentifier;

		internal readonly bool _StatusIdentifier;

		internal readonly float _ValIdentifier;

		internal readonly bool adapterIdentifier;

		internal static object FlushTokenizer;

		internal PageUtilsWatcher(string ident)
		{
			_RegIdentifier = ident;
			adapterIdentifier = true;
			if (ident.Length > 1)
			{
				if (ident.StartsWith("\"") && ident.EndsWith("\""))
				{
					m_ProcessIdentifier = ((ident.Length != 2) ? ident.Substring(1, ident.Length - 2) : string.Empty);
				}
				else
				{
					m_ProcessIdentifier = ident;
				}
			}
			else
			{
				m_ProcessIdentifier = ident;
			}
			_StatusIdentifier = m_ProcessIdentifier == "true";
			float.TryParse(m_ProcessIdentifier, out _ValIdentifier);
		}

		public override string ToString()
		{
			return m_ProcessIdentifier;
		}

		public static implicit operator string(PageUtilsWatcher setup)
		{
			return setup.m_ProcessIdentifier;
		}

		public static implicit operator bool(PageUtilsWatcher setup)
		{
			return setup._StatusIdentifier;
		}

		public static implicit operator float(PageUtilsWatcher spec)
		{
			return spec._ValIdentifier;
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
	private class RefImporterDescriptor
	{
		internal class TestPropertyFilter : IDisposable
		{
			private readonly Action collectionIdentifier;

			private readonly bool _InterceptorIdentifier;

			private readonly EditorGUI.ChangeCheckScope m_RegistryIdentifier;

			internal static TestPropertyFilter CancelTask;

			[SpecialName]
			internal bool ValidateConsumer()
			{
				return m_RegistryIdentifier.changed;
			}

			public TestPropertyFilter(Action reference = null)
			{
				collectionIdentifier = reference;
				_InterceptorIdentifier = ReflectConsumer();
				ResolveConsumer(isv: true);
				m_RegistryIdentifier = new EditorGUI.ChangeCheckScope();
			}

			public void Dispose()
			{
				bool changed = m_RegistryIdentifier.changed;
				m_RegistryIdentifier.Dispose();
				if (changed)
				{
					collectionIdentifier?.Invoke();
					ForgotConsumer();
				}
				ResolveConsumer(_InterceptorIdentifier);
			}

			public static implicit operator bool(TestPropertyFilter v)
			{
				return v.m_RegistryIdentifier.changed;
			}

			internal static bool PrepareTask()
			{
				return CancelTask == null;
			}
		}

		internal class RepositoryAuthenticationFactory : IDisposable
		{
			private readonly bool _ClientIdentifier;

			private static RepositoryAuthenticationFactory InstantiateTask;

			public RepositoryAuthenticationFactory()
			{
				_ClientIdentifier = ReflectConsumer();
				ResolveConsumer(isv: true);
			}

			public void Dispose()
			{
				ResolveConsumer(_ClientIdentifier);
			}

			internal static bool VisitTask()
			{
				return InstantiateTask == null;
			}
		}

		[Serializable]
		internal class ConnectionIdentifierService : WatcherSingletonManager
		{
			[SerializeField]
			private bool _value;

			internal readonly Action observerIdentifier;

			internal static ConnectionIdentifierService RateTask;

			[SpecialName]
			internal bool CustomizeUtils()
			{
				return _value;
			}

			[SpecialName]
			internal void ConcatUtils(bool nores)
			{
				if (_value != nores)
				{
					_value = nores;
					observerIdentifier?.Invoke();
					ForgotConsumer();
				}
			}

			internal ConnectionIdentifierService(bool forcev, Action connection = null)
			{
				m_TestsIdentifier = (string)(object)forcev;
				_value = forcev;
				observerIdentifier = connection;
			}

			internal void IncludeConsumer()
			{
				ConcatUtils(!_value);
			}

			internal void RevertConsumer(string res, GUIStyle ord = null, params GUILayoutOption[] options)
			{
				RunUtils(new GUIContent(res), ord, options);
			}

			internal void RunUtils(GUIContent info, GUIStyle selection = null, params GUILayoutOption[] options)
			{
				if (selection == null)
				{
					uint num = default(uint);
					while (true)
					{
						ConcatUtils(EditorGUILayout.Toggle(info, CustomizeUtils(), options));
						switch ((num = (uint)(((int)num * -77227172) ^ -344708589 ^ 0x1EC2843A)) % 5)
						{
						case 0u:
							break;
						default:
							return;
						case 4u:
							return;
						case 1u:
						case 2u:
							continue;
						case 3u:
							return;
						}
						break;
					}
				}
				ConcatUtils(EditorGUILayout.Toggle(info, CustomizeUtils(), selection, options));
			}

			internal void OrderUtils(string spec, string token = null, bool rejectproc = false, Color? reg2 = null, Color? config3 = null, params GUILayoutOption[] options)
			{
				CalculateUtils((!string.IsNullOrEmpty(spec)) ? new GUIContent(spec) : GUIContent.none, (!string.IsNullOrEmpty(token)) ? new GUIContent(token) : GUIContent.none, rejectproc, reg2, config3, options);
			}

			internal void CalculateUtils(GUIContent value, GUIContent cfg = null, bool isconsumer = false, Color? cust2 = null, Color? t3 = null, params GUILayoutOption[] options)
			{
				cust2 = cust2 ?? GUI.backgroundColor;
				t3 = t3 ?? GUI.backgroundColor;
				Color backgroundColor = GUI.backgroundColor;
				GUI.backgroundColor = ((!CustomizeUtils()) ? t3.Value : cust2.Value);
				ConcatUtils(GUILayout.Toggle(CustomizeUtils(), (!CustomizeUtils() && cfg != null) ? cfg : value, (!isconsumer) ? GUI.skin.button : EditorStyles.toolbarButton, options));
				GUI.backgroundColor = backgroundColor;
			}

			public static implicit operator bool(ConnectionIdentifierService config)
			{
				return config._value;
			}

			internal override void QueryCollection()
			{
				ConcatUtils((bool)(object)m_TestsIdentifier);
			}

			internal static bool NewTask()
			{
				return RateTask == null;
			}
		}

		[Serializable]
		internal class BroadcasterIdentifier : WatcherSingletonManager
		{
			[SerializeField]
			private float _value;

			internal readonly Action _EventIdentifier;

			private static BroadcasterIdentifier ChangeTask;

			[SpecialName]
			internal float PatchUtils()
			{
				return _value;
			}

			[SpecialName]
			internal void CheckUtils(float task)
			{
				if (_value != task)
				{
					_value = task;
					_EventIdentifier?.Invoke();
					ForgotConsumer();
				}
			}

			internal BroadcasterIdentifier(float i, Action counter = null)
			{
				m_TestsIdentifier = (string)(object)i;
				_value = i;
				_EventIdentifier = counter;
			}

			internal void FillUtils(string reference, bool nopol = true, GUIStyle comp = null, params GUILayoutOption[] options)
			{
				LogoutUtils(new GUIContent(reference), nopol, comp, options);
			}

			internal void CancelUtils(string info, float attr, bool moverule = true, GUIStyle setup2 = null, params GUILayoutOption[] options)
			{
				EditorGUIUtility.labelWidth = attr;
				while (true)
				{
					LogoutUtils(new GUIContent(info), moverule, setup2, options);
				}
			}

			internal void LogoutUtils(GUIContent spec, bool ispol = true, GUIStyle dir = null, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					CheckUtils((dir != null) ? EditorGUILayout.FloatField(spec, PatchUtils(), dir, options) : EditorGUILayout.FloatField(spec, PatchUtils(), options));
					if (!ispol)
					{
						return;
					}
					uint num4 = default(uint);
					while (true)
					{
						int num;
						int num2;
						if (!GUILayout.Button(ExceptionSingletonStruct.CustomizeRef()._ConfigSerializer, ExceptionSingletonStruct.MapRef()._ClassSerializer, GUILayout.Width(18f), GUILayout.Height(18f)))
						{
							num = 255188901;
							num2 = 255188901;
						}
						else
						{
							num = 1961405910;
							num2 = 1961405910;
						}
						int num3 = num ^ (int)(num4 * 1762542089);
						while (true)
						{
							switch ((num4 = (uint)(num3 ^ -1488522537)) % 4)
							{
							case 2u:
								goto IL_003b;
							case 0u:
							case 3u:
								break;
							default:
								return;
							case 1u:
								return;
							}
							break;
							IL_003b:
							QueryCollection();
							num3 = (int)(num4 * 1566378496) ^ -515627630;
						}
					}
				}
			}

			internal void SetupUtils(GUIContent config, float pol, bool removedic = true, GUIStyle col2 = null, params GUILayoutOption[] options)
			{
				EditorGUIUtility.labelWidth = pol;
				LogoutUtils(config, removedic, col2, options);
				EditorGUIUtility.labelWidth = 0f;
			}

			internal void SelectUtils(string item, float map, float temp, bool requiresreference2 = true, params GUILayoutOption[] options)
			{
				WriteUtils(new GUIContent(item), map, temp, requiresreference2, options);
			}

			internal void WriteUtils(GUIContent instance, float pol, float consumer, bool forcet2 = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					CheckUtils(EditorGUILayout.Slider(instance, PatchUtils(), pol, consumer, options));
					if (forcet2 && GUILayout.Button(ExceptionSingletonStruct.CustomizeRef()._ConfigSerializer, ExceptionSingletonStruct.MapRef()._ClassSerializer, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						QueryCollection();
					}
				}
			}

			internal void MoveUtils(string def, bool isb = true, params GUILayoutOption[] options)
			{
				PublishUtils(new GUIContent(def), isb, options);
			}

			internal void PublishUtils(GUIContent param, bool testpred = true, params GUILayoutOption[] options)
			{
				WriteUtils(param, 0f, 1f, testpred, options);
			}

			internal override void QueryCollection()
			{
				CheckUtils((float)(object)m_TestsIdentifier);
			}

			public static implicit operator int(BroadcasterIdentifier asset)
			{
				return (int)asset._value;
			}

			public static implicit operator float(BroadcasterIdentifier reference)
			{
				return reference._value;
			}

			internal static bool SetupTask()
			{
				return ChangeTask == null;
			}
		}

		[Serializable]
		internal class ListenerWatcherRule : BroadcasterIdentifier
		{
			private static ListenerWatcherRule PopTask;

			[SerializeField]
			internal int RestartUtils
			{
				get
				{
					return (int)PatchUtils();
				}
				set
				{
					CheckUtils(value);
				}
			}

			internal ListenerWatcherRule(int endident, Action counter = null)
				: base(endident, counter)
			{
			}

			internal T RegisterUtils<T>() where T : Enum
			{
				return (T)(object)RestartUtils;
			}

			internal void ChangeUtils(GUIContent setup, GUIStyle map = null, params GUILayoutOption[] options)
			{
				RestartUtils = ((map != null) ? EditorGUILayout.IntField(setup, RestartUtils, map, options) : EditorGUILayout.IntField(setup, RestartUtils, options));
			}

			internal void StopUtils(string first, GUIStyle ivk = null, params GUILayoutOption[] options)
			{
				ChangeUtils(new GUIContent(first), ivk, options);
			}

			internal void PushUtils<T>(GUIContent reference, bool acceptcont = false, GUIStyle dic = null, params GUILayoutOption[] options) where T : Enum
			{
				if (!acceptcont)
				{
					RestartUtils = ((dic != null) ? ((int)(object)EditorGUILayout.EnumPopup(reference, (T)(object)RestartUtils, dic, options)) : ((int)(object)EditorGUILayout.EnumPopup(reference, (T)(object)RestartUtils, options)));
				}
				else
				{
					RestartUtils = ((dic != null) ? ((int)(object)EditorGUILayout.EnumFlagsField(reference, (T)(object)RestartUtils, dic, options)) : ((int)(object)EditorGUILayout.EnumFlagsField(reference, (T)(object)RestartUtils, options)));
				}
			}

			internal void PrepareUtils<T>(string param, bool isb = false, GUIStyle temp = null, params GUILayoutOption[] options) where T : Enum
			{
				PushUtils<T>(new GUIContent(param), isb, temp, options);
			}

			internal static ListenerWatcherRule ReadUtils<T>(T last, Action selection = null) where T : Enum
			{
				return new ListenerWatcherRule((int)(object)last, selection);
			}

			public static implicit operator int(ListenerWatcherRule instance)
			{
				return instance.RestartUtils;
			}

			public static implicit operator float(ListenerWatcherRule task)
			{
				return task.RestartUtils;
			}

			internal static bool ViewTask()
			{
				return PopTask == null;
			}
		}

		[Serializable]
		internal class RecordIdentifier : WatcherSingletonManager
		{
			[SerializeField]
			private float _valueX;

			[SerializeField]
			private float _valueY;

			[SerializeField]
			private float _valueZ;

			internal Action _ResolverIdentifier;

			internal bool _TagIdentifier;

			internal Vector3 filterIdentifier;

			internal static RecordIdentifier PushTask;

			[SpecialName]
			internal Vector3 ConnectUtils()
			{
				if (!_TagIdentifier)
				{
					while (true)
					{
						_TagIdentifier = true;
						filterIdentifier = new Vector3(_valueX, _valueY, _valueZ);
					}
				}
				return filterIdentifier;
			}

			[SpecialName]
			internal void FindUtils(Vector3 config)
			{
				if (filterIdentifier != config)
				{
					filterIdentifier = config;
					_valueX = config.x;
					_valueY = config.y;
					_valueZ = config.z;
					_ResolverIdentifier?.Invoke();
					ForgotConsumer();
				}
			}

			internal void ManageUtils(Vector3 key, Action pol)
			{
				m_TestsIdentifier = (string)(object)key;
				_ResolverIdentifier = pol;
				_valueX = key.x;
				_valueY = key.y;
				_valueZ = key.z;
			}

			internal RecordIdentifier(Vector3 info, Action caller = null)
			{
				ManageUtils(info, caller);
			}

			internal RecordIdentifier(float var1, float token, float control, Action second2 = null)
			{
				ManageUtils(new Vector3(var1, token, control), second2);
			}

			internal RecordIdentifier(float config, float second, Action comp = null)
			{
				ManageUtils(new Vector3(config, second), comp);
			}

			internal void RateUtils(GUIContent task, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label(task, GUILayout.MaxWidth(117f));
					FindUtils(EditorGUILayout.Vector2Field(GUIContent.none, ConnectUtils(), options));
					if (GUILayout.Button(ExceptionSingletonStruct.CustomizeRef()._ConfigSerializer, ExceptionSingletonStruct.MapRef()._ClassSerializer, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						QueryCollection();
					}
				}
			}

			internal void CloneUtils(string reference, params GUILayoutOption[] options)
			{
				RateUtils(new GUIContent(reference), options);
			}

			internal void ComputeUtils(GUIContent setup, params GUILayoutOption[] options)
			{
				FindUtils(EditorGUILayout.Vector3Field(setup, ConnectUtils(), options));
			}

			internal void QueryUtils(string key, params GUILayoutOption[] options)
			{
				ComputeUtils(new GUIContent(key), options);
			}

			internal override void QueryCollection()
			{
				FindUtils((Vector3)(object)m_TestsIdentifier);
			}

			public static implicit operator Vector2(RecordIdentifier spec)
			{
				return spec.ConnectUtils();
			}

			internal static bool SortTask()
			{
				return PushTask == null;
			}
		}

		[Serializable]
		internal class FactoryIdentifier : WatcherSingletonManager
		{
			[SerializeField]
			private string _value;

			internal readonly Action m_AttributeIdentifier;

			internal static FactoryIdentifier CloneTask;

			[SpecialName]
			internal string CreateUtils()
			{
				return _value;
			}

			[SpecialName]
			internal void IncludeUtils(string ident)
			{
				if (_value != ident)
				{
					_value = ident;
					m_AttributeIdentifier?.Invoke();
					ForgotConsumer();
				}
			}

			internal FactoryIdentifier(string ident = "", Action attr = null)
			{
				m_TestsIdentifier = ident;
				_value = ident;
				m_AttributeIdentifier = attr;
			}

			internal override void QueryCollection()
			{
				IncludeUtils(m_TestsIdentifier);
			}

			public override string ToString()
			{
				return CreateUtils();
			}

			public static implicit operator string(FactoryIdentifier setup)
			{
				return setup._value;
			}

			internal static bool FindTask()
			{
				return CloneTask == null;
			}
		}

		[Serializable]
		internal class InstanceIdentifier : WatcherSingletonManager
		{
			internal readonly Action taskIdentifier;

			[SerializeField]
			private float r;

			[SerializeField]
			private float g;

			[SerializeField]
			private float b;

			[SerializeField]
			private float a;

			internal static InstanceIdentifier ResetTask;

			[SpecialName]
			internal Color VerifyPage()
			{
				return new Color(r, g, b, a);
			}

			[SpecialName]
			internal void SetPage(Color info)
			{
				r = info.r;
				g = info.g;
				b = info.b;
				a = info.a;
				taskIdentifier?.Invoke();
				ForgotConsumer();
			}

			internal InstanceIdentifier(float def, float vis, float dir, float token2 = 1f, Action task3 = null)
			{
				Color color = new Color(def, vis, dir, token2);
				m_TestsIdentifier = (string)(object)color;
				r = def;
				g = vis;
				b = dir;
				a = token2;
				taskIdentifier = task3;
			}

			internal InstanceIdentifier(Color init, Action attr = null)
			{
				m_TestsIdentifier = (string)(object)init;
				r = init.r;
				g = init.g;
				b = init.b;
				a = init.a;
				taskIdentifier = attr;
			}

			internal void RunPage(string v, bool appendresult = true, params GUILayoutOption[] options)
			{
				OrderPage(new GUIContent(v), appendresult, options);
			}

			internal void OrderPage(GUIContent key, bool iscol = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					SetPage(EditorGUILayout.ColorField(key, VerifyPage(), options));
					if (iscol && GUILayout.Button(ExceptionSingletonStruct.CustomizeRef()._ConfigSerializer, ExceptionSingletonStruct.MapRef()._ClassSerializer, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						QueryCollection();
					}
				}
			}

			internal override void QueryCollection()
			{
				SetPage((Color)(object)m_TestsIdentifier);
			}

			internal static bool ForgotTask()
			{
				return ResetTask == null;
			}
		}

		[Serializable]
		internal class CustomerIdentifier : WatcherSingletonManager
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

			private static CustomerIdentifier StopTask;

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
					ForgotConsumer();
				}
			}

			internal CustomerIdentifier(Type reference, string pred = "", long state_ID = 0L, Action ivk2 = null)
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
					if (ispol && GUILayout.Button(ExceptionSingletonStruct.CustomizeRef()._ConfigSerializer, ExceptionSingletonStruct.MapRef()._ClassSerializer, GUILayout.Width(18f), GUILayout.Height(18f)))
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

			public static implicit operator bool(CustomerIdentifier init)
			{
				return init.ForgotPage();
			}

			internal static bool StartTask()
			{
				return StopTask == null;
			}
		}

		internal abstract class WatcherSingletonManager
		{
			internal string m_TestsIdentifier;

			internal static WatcherSingletonManager ResolveTask;

			internal abstract void QueryCollection();

			internal static bool CountTask()
			{
				return ResolveTask == null;
			}
		}

		[AttributeUsage(AttributeTargets.Field)]
		internal class DefinitionIdentifier : Attribute
		{
			private static DefinitionIdentifier WriteTask;

			internal static bool CustomizeTask()
			{
				return WriteTask == null;
			}
		}

		private static bool _ProxyIdentifier;

		private static bool m_RefIdentifier;

		private static bool comparatorIdentifier;

		private static FieldInfo[] productIdentifier;

		private static RefImporterDescriptor m_IteratorIdentifier;

		internal static Action m_PredicateIdentifier;

		[SerializeField]
		internal FactoryIdentifier u_updateLink = new FactoryIdentifier();

		[SerializeField]
		internal FactoryIdentifier u_updateVersion = new FactoryIdentifier();

		[SerializeField]
		internal FactoryIdentifier u_updateMessage = new FactoryIdentifier();

		[SerializeField]
		internal FactoryIdentifier u_updateChangelog = new FactoryIdentifier();

		[SerializeField]
		internal FactoryIdentifier u_updateDay = new FactoryIdentifier();

		[SerializeField]
		internal FactoryIdentifier u_announcement = new FactoryIdentifier();

		[SerializeField]
		internal FactoryIdentifier u_announcementLink = new FactoryIdentifier();

		[SerializeField]
		internal FactoryIdentifier u_announcementLinkName = new FactoryIdentifier();

		[SerializeField]
		internal FactoryIdentifier u_announcementHiddenDate = new FactoryIdentifier();

		[SerializeField]
		internal ConnectionIdentifierService u_updateHidden = new ConnectionIdentifierService(forcev: false);

		[SerializeField]
		internal ConnectionIdentifierService u_announcementHidden = new ConnectionIdentifierService(forcev: false);

		[SerializeField]
		internal ConnectionIdentifierService a_HasSucceededLastVerification = new ConnectionIdentifierService(forcev: false);

		[SerializeField]
		internal ConnectionIdentifierService a_VerifyOnDisplay = new ConnectionIdentifierService(forcev: true);

		[SerializeField]
		internal ConnectionIdentifierService a_VerifyOnProjectLoad = new ConnectionIdentifierService(forcev: false);

		[SerializeField]
		internal ConnectionIdentifierService gizmosActive = new ConnectionIdentifierService(forcev: true, ComposerIdentifier.InterruptSingleton);

		[SerializeField]
		internal ConnectionIdentifierService globalGizmo = new ConnectionIdentifierService(forcev: true, ComposerIdentifier.InterruptSingleton);

		[SerializeField]
		internal ConnectionIdentifierService editorAnimatedFoldouts = new ConnectionIdentifierService(forcev: true);

		[SerializeField]
		internal ConnectionIdentifierService onSceneNameLabels = new ConnectionIdentifierService(forcev: true);

		[SerializeField]
		internal ConnectionIdentifierService onSceneToolSelection = new ConnectionIdentifierService(forcev: true);

		[SerializeField]
		internal ConnectionIdentifierService onSceneToolSelectionAlwaysVisible = new ConnectionIdentifierService(forcev: true);

		[SerializeField]
		internal ConnectionIdentifierService onSceneEditingOverlay = new ConnectionIdentifierService(forcev: true);

		[SerializeField]
		internal ConnectionIdentifierService onSceneOverlayInterceptsClick = new ConnectionIdentifierService(forcev: true);

		[SerializeField]
		internal ConnectionIdentifierService onSceneTooltip = new ConnectionIdentifierService(forcev: true);

		[SerializeField]
		internal ConnectionIdentifierService ignoreSceneClicks = new ConnectionIdentifierService(forcev: true);

		[SerializeField]
		internal ConnectionIdentifierService hideToolsDuringTesting = new ConnectionIdentifierService(forcev: true);

		[SerializeField]
		internal ConnectionIdentifierService hasReadColliderTestingWarning = new ConnectionIdentifierService(forcev: false);

		[SerializeField]
		internal ListenerWatcherRule toolSelectionOverlayAlignment = ListenerWatcherRule.ReadUtils(ExceptionSingletonStruct.PositionFlag.BottomLeft);

		[SerializeField]
		internal ListenerWatcherRule toolOverlayAlignment = ListenerWatcherRule.ReadUtils(ExceptionSingletonStruct.PositionFlag.BottomRight);

		[SerializeField]
		internal BroadcasterIdentifier gizmoBoneOpacity = new BroadcasterIdentifier(0.5f, ComposerIdentifier.InterruptSingleton);

		[SerializeField]
		internal BroadcasterIdentifier gizmoLimitOpacity = new BroadcasterIdentifier(0.5f, ComposerIdentifier.InterruptSingleton);

		[SerializeField]
		internal BroadcasterIdentifier handleSizeMultiplier = new BroadcasterIdentifier(1f);

		[SerializeField]
		internal InstanceIdentifier labelColor = new InstanceIdentifier(1f, 1f, 1f);

		[SerializeField]
		internal InstanceIdentifier generalColor = new InstanceIdentifier(1f, 1f, 1f);

		[SerializeField]
		internal InstanceIdentifier activeColor = new InstanceIdentifier(0.56f, 0.94f, 0.47f);

		[SerializeField]
		internal InstanceIdentifier inactiveColor = new InstanceIdentifier(1f, 0f, 0.3765f);

		[SerializeField]
		internal InstanceIdentifier mixedColor = new InstanceIdentifier(1f, 0.65f, 0f);

		[SerializeField]
		internal InstanceIdentifier selectionColor = new InstanceIdentifier(1f, 0.65f, 0f);

		internal static RefImporterDescriptor ConnectTokenizer;

		[SpecialName]
		internal static bool ReflectConsumer()
		{
			return comparatorIdentifier;
		}

		[SpecialName]
		internal static void ResolveConsumer(bool isv)
		{
			bool num = comparatorIdentifier;
			comparatorIdentifier = isv;
			if (num && !comparatorIdentifier && m_RefIdentifier)
			{
				ForgotConsumer();
			}
		}

		[SpecialName]
		internal static RefImporterDescriptor GetConsumer()
		{
			if (m_IteratorIdentifier == null)
			{
				UpdateConsumer();
			}
			return m_IteratorIdentifier;
		}

		private RefImporterDescriptor()
		{
			productIdentifier = (from m in typeof(RefImporterDescriptor).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
				where m.IsDefined(typeof(DefinitionIdentifier), inherit: false)
				select m).ToArray();
		}

		internal static void ForgotConsumer()
		{
			m_RefIdentifier = false;
			if (comparatorIdentifier)
			{
				m_RefIdentifier = true;
			}
			else
			{
				if (_ProxyIdentifier)
				{
					return;
				}
				StringBuilder stringBuilder = new StringBuilder("MAIN[" + JsonUtility.ToJson(GetConsumer()) + "]\u200b\u200b\u200b");
				FieldInfo[] array = productIdentifier;
				foreach (FieldInfo fieldInfo in array)
				{
					try
					{
						string text = EditorJsonUtility.ToJson(fieldInfo.GetValue(GetConsumer()));
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

		private static void UpdateConsumer()
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
				m_IteratorIdentifier = JsonUtility.FromJson<RefImporterDescriptor>(value);
			}
			if (m_IteratorIdentifier == null)
			{
				m_IteratorIdentifier = new RefImporterDescriptor();
			}
			FieldInfo[] array = productIdentifier;
			foreach (FieldInfo fieldInfo in array)
			{
				object obj = fieldInfo.GetValue(m_IteratorIdentifier) ?? Activator.CreateInstance(fieldInfo.FieldType);
				if (dictionary.TryGetValue(fieldInfo.Name, out var value2))
				{
					EditorJsonUtility.FromJsonOverwrite(value2, obj);
				}
				fieldInfo.SetValue(m_IteratorIdentifier, obj);
				if (fieldInfo.GetValue(m_IteratorIdentifier) == null)
				{
					fieldInfo.SetValue(m_IteratorIdentifier, Activator.CreateInstance(fieldInfo.FieldType));
				}
			}
		}

		internal static void SearchConsumer()
		{
			if (EditorUtility.DisplayDialog("Clearing Settings", "Are you sure you want to clear the settings?", "Clear", "Cancel"))
			{
				LoginConsumer();
			}
		}

		internal static void LoginConsumer()
		{
			m_IteratorIdentifier = new RefImporterDescriptor();
			FieldInfo[] array = productIdentifier;
			foreach (FieldInfo fieldInfo in array)
			{
				fieldInfo.SetValue(m_IteratorIdentifier, Activator.CreateInstance(fieldInfo.FieldType));
			}
			m_PredicateIdentifier?.Invoke();
			ForgotConsumer();
		}

		[SpecialName]
		internal Color[] ExcludeConsumer()
		{
			return new Color[3]
			{
				inactiveColor.VerifyPage(),
				activeColor.VerifyPage(),
				mixedColor.VerifyPage()
			};
		}

		internal static bool RegisterTokenizer()
		{
			return ConnectTokenizer == null;
		}
	}

	private sealed class AttributeConsumerExporter : Editor
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

		internal static AttributeConsumerExporter InvokeTask;

		public override void OnInspectorGUI()
		{
			if (!FlushConfiguration())
			{
				if (!_Service)
				{
					EnableConfiguration(PreparePage);
				}
			}
			else
			{
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
					return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
				})())
				{
					return;
				}
				base.serializedObject.Update();
				ChangePage();
				SelectIdentifier("Shape", _GetterIdentifier[0], null, delegate
				{
					RunConfiguration(AssetPage(), new SerializedProperty[6] { m_RoleIdentifier, visitorIdentifier, invocationIdentifier, m_ListenerIdentifier, m_ParserIdentifier, m_PrinterIdentifier }, RegisterPage, isres2: false);
				});
				SelectIdentifier("Receiver", _GetterIdentifier[1], null, delegate
				{
					PushConfiguration();
					EditorGUILayout.PropertyField(m_ManagerIdentifier);
					PrepareConfiguration(m_WorkerIdentifier);
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
				SelectIdentifier("Filtering", _GetterIdentifier[2], null, delegate
				{
					PushConfiguration();
					using (new GUILayout.HorizontalScope())
					{
						RegisterConfiguration(m_DescriptorIdentifier, m_DescriptorIdentifier.SelectStatus(), null);
						RegisterConfiguration(_StrategyIdentifier, _StrategyIdentifier.SelectStatus(), null);
						RegisterConfiguration(globalIdentifier, globalIdentifier.SelectStatus(), null);
					}
					m_AlgoIdentifier.DoLayoutList();
				});
				base.serializedObject.ApplyModifiedProperties();
				GetConfiguration();
				SortIdentifier();
			}
		}

		private void method_0()
		{
			OrderConfiguration(AssetPage(), base.targets, 2, Color.cyan);
		}

		private void CallPage(Rect setup, int version_attr, bool isserv, bool isvis2)
		{
			CustomizeConfiguration(m_RepositoryIdentifier, setup, version_attr);
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
			SortConfiguration(flag3, flag2, flag);
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
				drawHeaderCallback = ConcatConfiguration
			};
		}

		private void OnEnable()
		{
			SelectConfiguration(_GetterIdentifier, Repaint);
			MapConfiguration(RegisterPage);
		}

		private void OnDisable()
		{
			CancelConfiguration(isvar1: false);
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
				m_IndexerIdentifier = ExceptionSingletonStruct.CancelStatus("VRCContactReceiver");
			}
			if (poolIdentifier == null)
			{
				poolIdentifier = ExceptionSingletonStruct.CancelStatus("VRCContactReceiverEditor");
			}
			_ThreadIdentifier = !insertasset;
			ExceptionSingletonStruct.RevertStatus(m_IndexerIdentifier, (!_ThreadIdentifier) ? poolIdentifier : typeof(AttributeConsumerExporter));
		}

		[CompilerGenerated]
		private void TestPage()
		{
			RunConfiguration(AssetPage(), new SerializedProperty[6] { m_RoleIdentifier, visitorIdentifier, invocationIdentifier, m_ListenerIdentifier, m_ParserIdentifier, m_PrinterIdentifier }, RegisterPage, isres2: false);
		}

		[CompilerGenerated]
		private void InsertPage()
		{
			PushConfiguration();
			EditorGUILayout.PropertyField(m_ManagerIdentifier);
			PrepareConfiguration(m_WorkerIdentifier);
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
			PushConfiguration();
			using (new GUILayout.HorizontalScope())
			{
				RegisterConfiguration(m_DescriptorIdentifier, m_DescriptorIdentifier.SelectStatus(), null);
				RegisterConfiguration(_StrategyIdentifier, _StrategyIdentifier.SelectStatus(), null);
				RegisterConfiguration(globalIdentifier, globalIdentifier.SelectStatus(), null);
			}
			m_AlgoIdentifier.DoLayoutList();
		}

		UnityEngine.Object AssetPage()
		{
			return base.target;
		}

		internal static bool ConcatTask()
		{
			return (object)InvokeTask == null;
		}
	}

	private sealed class RuleIdentifier : Editor
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

		private static RuleIdentifier DefineTask;

		public override void OnInspectorGUI()
		{
			if (!FlushConfiguration())
			{
				if (!_Service)
				{
					EnableConfiguration(SortProperty);
				}
			}
			else
			{
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
					return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
				})())
				{
					return;
				}
				base.serializedObject.Update();
				CompareProperty();
				SelectIdentifier("Shape", _StructIdentifier[0], null, delegate
				{
					RunConfiguration(LogoutProperty(), new SerializedProperty[6] { attrIdentifier, objectIdentifier, m_ServiceIdentifier, _ReponseIdentifier, specificationIdentifier, _WrapperIdentifier }, NewProperty, isres2: false);
				});
				SelectIdentifier("Filtering", _StructIdentifier[1], null, delegate
				{
					PushConfiguration();
					using (new GUILayout.VerticalScope())
					{
						_ParameterIdentifier.DoLayoutList();
					}
				});
				base.serializedObject.ApplyModifiedProperties();
				GetConfiguration();
				SortIdentifier();
			}
		}

		private void method_0()
		{
			OrderConfiguration(LogoutProperty(), base.targets, 1, Color.yellow);
		}

		private void DestroyProperty(Rect config, int length_b, bool requiresc, bool isvis2)
		{
			CustomizeConfiguration(m_InfoIdentifier, config, length_b);
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
			SortConfiguration(flag3, flag2, flag);
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
				drawHeaderCallback = ConcatConfiguration
			};
		}

		private void OnEnable()
		{
			SelectConfiguration(_StructIdentifier, Repaint);
			MapConfiguration(NewProperty);
		}

		private void OnDisable()
		{
			CancelConfiguration(isvar1: false);
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
				m_ModelIdentifier = ExceptionSingletonStruct.CancelStatus("VRCContactSender");
			}
			if (m_ConfigIdentifier == null)
			{
				m_ConfigIdentifier = ExceptionSingletonStruct.CancelStatus("VRCContactSenderEditor");
			}
			_InterpreterIdentifier = !isinstance;
			ExceptionSingletonStruct.RevertStatus(m_ModelIdentifier, (!_InterpreterIdentifier) ? m_ConfigIdentifier : typeof(RuleIdentifier));
		}

		[CompilerGenerated]
		private void CustomizeProperty()
		{
			RunConfiguration(LogoutProperty(), new SerializedProperty[6] { attrIdentifier, objectIdentifier, m_ServiceIdentifier, _ReponseIdentifier, specificationIdentifier, _WrapperIdentifier }, NewProperty, isres2: false);
		}

		[CompilerGenerated]
		private void ConcatProperty()
		{
			PushConfiguration();
			using (new GUILayout.VerticalScope())
			{
				_ParameterIdentifier.DoLayoutList();
			}
		}

		UnityEngine.Object LogoutProperty()
		{
			return base.target;
		}

		internal static bool TestTask()
		{
			return (object)DefineTask == null;
		}
	}

	private sealed class IndexerMethodBridge : Editor
	{
		private static readonly AnimBool[] _FieldIdentifier = new AnimBool[1]
		{
			new AnimBool(value: true)
		};

		private static bool advisorIdentifier = true;

		private SerializedProperty exporterIdentifier;

		private SerializedProperty _CreatorIdentifier;

		private SerializedProperty m_DispatcherIdentifier;

		private SerializedProperty connectionIdentifier;

		private SerializedProperty expressionIdentifier;

		private SerializedProperty _DecoratorIdentifier;

		private SerializedProperty paramIdentifier;

		private SerializedProperty _PrototypeIdentifier;

		private static Type m_BaseIdentifier;

		private static Type _RequestIdentifier;

		internal static IndexerMethodBridge VerifyTask;

		public override void OnInspectorGUI()
		{
			if (FlushConfiguration())
			{
				base.serializedObject.Update();
				AwakeProperty();
				if (((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
					return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
				})())
				{
					if (ReadConfiguration(base.targets))
					{
						InsertConfiguration();
					}
					SelectIdentifier("Shape", _FieldIdentifier[0], null, delegate
					{
						RunConfiguration(RestartProperty(), new SerializedProperty[8] { _CreatorIdentifier, exporterIdentifier, expressionIdentifier, _DecoratorIdentifier, paramIdentifier, _PrototypeIdentifier, m_DispatcherIdentifier, connectionIdentifier }, EnableProperty, isres2: true);
					});
					if (TestConfiguration(base.serializedObject, base.targets))
					{
						SceneView.RepaintAll();
						_Rules = true;
					}
					GetConfiguration();
					SortIdentifier();
					ConcatIdentifier();
				}
			}
			else if (!_Service)
			{
				EnableConfiguration(TestProperty);
			}
		}

		public void method_0()
		{
			OrderConfiguration(RestartProperty(), base.targets, 0, Color.green);
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
			InsertProperty(advisorIdentifier);
		}

		internal static void InsertProperty(bool isi = false)
		{
			if (m_BaseIdentifier == null)
			{
				m_BaseIdentifier = ExceptionSingletonStruct.CancelStatus("VRCPhysBoneCollider");
			}
			if (_RequestIdentifier == null)
			{
				_RequestIdentifier = ExceptionSingletonStruct.CancelStatus("VRCPhysBoneColliderEditor");
			}
			advisorIdentifier = !isi;
			ExceptionSingletonStruct.RevertStatus(m_BaseIdentifier, (!advisorIdentifier) ? _RequestIdentifier : typeof(IndexerMethodBridge));
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
			SortConfiguration(flag3, flag2, flag);
		}

		private void AwakeProperty()
		{
			exporterIdentifier = base.serializedObject.FindProperty("rootTransform");
			_CreatorIdentifier = base.serializedObject.FindProperty("shapeType");
			m_DispatcherIdentifier = base.serializedObject.FindProperty("insideBounds");
			connectionIdentifier = base.serializedObject.FindProperty("bonesAsSpheres");
			expressionIdentifier = base.serializedObject.FindProperty("radius");
			_DecoratorIdentifier = base.serializedObject.FindProperty("height");
			paramIdentifier = base.serializedObject.FindProperty("position");
			_PrototypeIdentifier = base.serializedObject.FindProperty("rotation");
		}

		private void OnEnable()
		{
			SelectConfiguration(_FieldIdentifier, Repaint);
			MapConfiguration(EnableProperty);
		}

		public void OnDisable()
		{
			CancelConfiguration(isvar1: false);
		}

		[CompilerGenerated]
		private void DisableProperty()
		{
			RunConfiguration(RestartProperty(), new SerializedProperty[8] { _CreatorIdentifier, exporterIdentifier, expressionIdentifier, _DecoratorIdentifier, paramIdentifier, _PrototypeIdentifier, m_DispatcherIdentifier, connectionIdentifier }, EnableProperty, isres2: true);
		}

		UnityEngine.Object RestartProperty()
		{
			return base.target;
		}

		internal static bool PublishTask()
		{
			return (object)VerifyTask == null;
		}
	}

	private sealed class ComposerIdentifier : Editor
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

			private static AlgoAuthentication PatchTask;

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

			internal static bool RemoveTask()
			{
				return PatchTask == null;
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

			public static Func<ExceptionSingletonStruct.InstanceConsumerExporter, bool> structAuthentication;

			public static Action _InterpreterAuthentication;

			public static Action _ParameterAuthentication;

			public static Action m_AttrAuthentication;

			public static Action<VRCPhysBone> objectAuthentication;

			public static Func<ExceptionSingletonStruct.ClientRegDic, bool> serviceAuthentication;

			public static Func<Keyframe, float> _ReponseAuthentication;

			public static Func<ExceptionSingletonStruct.InstanceConsumerExporter, bool> _SpecificationAuthentication;

			public static Func<VRCPhysBone, IEnumerable<Transform>> m_WrapperAuthentication;

			internal static _003C_003Ec ReadTask;

			internal bool DeleteParams()
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
				return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
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
					UpdateSingleton(ComposerIdentifier.adapterAuthentication, "X", isproc: false);
					UpdateSingleton(_ProxyAuthentication, "Y", isproc: false);
					UpdateSingleton(m_RefAuthentication, "Z", isproc: false);
					if (ExceptionSingletonStruct.CallStatus(ExceptionSingletonStruct.CustomizeRef().baseSerializer, GUI.skin.label, GUILayout.Width(14f)))
					{
						SerializedProperty adapterAuthentication = ComposerIdentifier.adapterAuthentication;
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

			internal bool SortParams(ExceptionSingletonStruct.InstanceConsumerExporter pbp2)
			{
				return pbp2._IndexerMethod;
			}

			internal void InvokeParams()
			{
				CallConfiguration(m_FactoryAuthentication, "Show Gizmos", delegate
				{
					if ((bool)RefImporterDescriptor.GetConsumer().globalGizmo)
					{
						RefImporterDescriptor.GetConsumer().gizmosActive.ConcatUtils(m_FactoryAuthentication.boolValue);
					}
				}, GUILayout.ExpandWidth(expand: false));
				bool flag;
				string text = ((!(flag = RefImporterDescriptor.GetConsumer().globalGizmo)) ? "Local Setting" : "Global Setting");
				using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, flag, ExceptionSingletonStruct._ObserverSerializer, ExceptionSingletonStruct._EventSerializer))
				{
					using (new RefImporterDescriptor.TestPropertyFilter(InterruptSingleton))
					{
						RefImporterDescriptor.GetConsumer().globalGizmo.ConcatUtils(GUILayout.Toggle(flag, text, GUI.skin.button, GUILayout.ExpandWidth(expand: false)));
					}
				}
			}

			internal void CustomizeParams()
			{
				if ((bool)RefImporterDescriptor.GetConsumer().globalGizmo)
				{
					RefImporterDescriptor.GetConsumer().gizmosActive.ConcatUtils(m_FactoryAuthentication.boolValue);
				}
			}

			internal void ConcatParams()
			{
				if ((bool)RefImporterDescriptor.GetConsumer().globalGizmo)
				{
					RefImporterDescriptor.GetConsumer().gizmoBoneOpacity.CheckUtils(EditorGUILayout.Slider("Bone Opacity", RefImporterDescriptor.GetConsumer().gizmoBoneOpacity, 0f, 1f));
					RefImporterDescriptor.GetConsumer().gizmoLimitOpacity.CheckUtils(EditorGUILayout.Slider("Limit Opacitiy", RefImporterDescriptor.GetConsumer().gizmoLimitOpacity, 0f, 1f));
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

			internal bool FillParams(ExceptionSingletonStruct.ClientRegDic b)
			{
				if (!b.m_DescriptorMethod)
				{
					return false;
				}
				return !b.m_RepositoryMethod;
			}

			internal float CancelParams(Keyframe k)
			{
				return k.value;
			}

			internal bool LogoutParams(ExceptionSingletonStruct.InstanceConsumerExporter p)
			{
				return p._IndexerMethod;
			}

			internal IEnumerable<Transform> SetupParams(VRCPhysBone pb)
			{
				return pb.GetRootTransform().GetComponentsInChildren<Transform>();
			}

			internal static bool LoginTask()
			{
				return ReadTask == null;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass108_1
		{
			public VRCAvatarDescriptor.CustomAnimLayer configAuthentication;

			public UnityEditor.Animations.AnimatorController m_MockAuthentication;

			internal static _003C_003Ec__DisplayClass108_1 FlushTask;

			internal static bool OrderTask()
			{
				return FlushTask == null;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass108_2
		{
			public ExceptionSingletonStruct.InstanceConsumerExporter _StateAuthentication;

			public string _FieldAuthentication;

			public _003C_003Ec__DisplayClass108_1 advisorAuthentication;

			internal static _003C_003Ec__DisplayClass108_2 InterruptTask;

			internal bool PatchImporter(UnityEngine.AnimatorControllerParameter p)
			{
				return p.name == _FieldAuthentication;
			}

			internal void CheckImporter()
			{
				advisorAuthentication.m_MockAuthentication.FindProcess(_FieldAuthentication, _StateAuthentication._ItemMethod, 0f);
				NewIdentifier($"Added {_FieldAuthentication} to {advisorAuthentication.configAuthentication.type} ({advisorAuthentication.m_MockAuthentication.name})");
				SetupConfiguration();
			}

			internal static bool CheckTask()
			{
				return InterruptTask == null;
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

			internal static _003C_003Ec__DisplayClass120_0 StopAnnotation;

			internal void OrderServer(ExceptionSingletonStruct.ClientRegDic b, float m)
			{
				if (m != 0f)
				{
					Matrix4x4 printerMethod = b.printerMethod;
					Vector4 column = printerMethod.GetColumn(3);
					float comp = m_TokenizerAuthentication.radius * m;
					EditorGUI.BeginChangeCheck();
					float num = ExceptionSingletonStruct.CreateStatus(printerMethod.rotation, column, comp, !m_TokenizerAuthentication.showGizmos, RefImporterDescriptor.GetConsumer().handleSizeMultiplier);
					if (EditorGUI.EndChangeCheck())
					{
						float delta = num / m - m_TokenizerAuthentication.radius;
						CalculateServer(b, delta);
					}
					ExceptionSingletonStruct.FindStatus(comp.ToString("F2"), column);
				}
			}

			internal void CalculateServer(ExceptionSingletonStruct.ClientRegDic bone, float delta)
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

			internal static bool StartAnnotation()
			{
				return StopAnnotation == null;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass120_1
		{
			public float dicAuthentication;

			public Action<SerializedProperty> containerAuthentication;

			internal static _003C_003Ec__DisplayClass120_1 ResolveAnnotation;

			internal void MapServer(SerializedProperty sp)
			{
				sp.floatValue = dicAuthentication;
			}

			internal static bool CountAnnotation()
			{
				return ResolveAnnotation == null;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass120_2
		{
			public float m_SchemaAuthentication;

			public float _BridgeAuthentication;

			public _003C_003Ec__DisplayClass120_0 _PublisherAuthentication;

			internal static _003C_003Ec__DisplayClass120_2 WriteAnnotation;

			internal void CancelServer(SerializedProperty sp)
			{
				sp.floatValue = Mathf.Clamp(sp.floatValue + m_SchemaAuthentication, _PublisherAuthentication.producerAuthentication, _PublisherAuthentication._TemplateAuthentication);
				_BridgeAuthentication = sp.floatValue;
			}

			internal static bool CustomizeAnnotation()
			{
				return WriteAnnotation == null;
			}
		}

		private static readonly AnimBool[] _AnnotationIdentifier = new AnimBool[8];

		private static object m_CodeIdentifier;

		private static bool _CallbackIdentifier = true;

		private static VRCPhysBone[] _MessageIdentifier;

		private static VRCPhysBone[] _PolicyIdentifier;

		private static VRCPhysBoneCollider[] m_MapperIdentifier;

		private static Transform[] mappingIdentifier;

		private static byte[] m_QueueIdentifier;

		private static Editor m_ProcessorIdentifier;

		private static readonly int _TokenizerIdentifier = GUIUtility.GetControlID("ADOToolSelectionDragControlID".GetHashCode(), FocusType.Passive);

		private static readonly ExceptionSingletonStruct.ExporterServerStub m_ExceptionIdentifier = new ExceptionSingletonStruct.ExporterServerStub();

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

		private static AlgoAuthentication[] taskAuthentication;

		private static GUIContent[] m_CustomerAuthentication;

		private static int[] m_DatabaseAuthentication;

		private static Dictionary<int, int> _HelperAuthentication;

		private static Dictionary<int, int> _CandidateAuthentication;

		private static bool m_ReaderAuthentication;

		private static int m_StubAuthentication = -1;

		private static readonly TokenParamsDispatcher rulesAuthentication = new TokenParamsDispatcher(7);

		private static readonly string[] m_TestsAuthentication = new string[7] { "None", "End Position Edit", "Ignore Selection", "Ignore Copy", "Collision Selection", "Collision Copy", "Property Edit" };

		private static int _DefinitionAuthentication = -1;

		private static Vector3 initializerAuthentication;

		private static Type m_TokenAuthentication;

		private static Type getterAuthentication;

		private static bool threadAuthentication;

		private static ComposerIdentifier ReflectTask;

		[SpecialName]
		private static AlgoAuthentication CountAccount()
		{
			if (RemoveAccount())
			{
				return taskAuthentication[m_StubAuthentication];
			}
			return null;
		}

		[SpecialName]
		private static bool RemoveAccount()
		{
			if (m_StubAuthentication < 0)
			{
				return false;
			}
			return rulesAuthentication.singletonSerializer == 6;
		}

		[SpecialName]
		private static bool ResolveAccount()
		{
			return rulesAuthentication.singletonSerializer == 1;
		}

		[SpecialName]
		private static void ResetAccount(bool noinit)
		{
			rulesAuthentication.LoginProcess(1, noinit);
		}

		[SpecialName]
		private static bool FlushAccount()
		{
			return rulesAuthentication.singletonSerializer == 2;
		}

		[SpecialName]
		private static void ExcludeAccount(bool isinfo)
		{
			rulesAuthentication.LoginProcess(2, isinfo);
		}

		[SpecialName]
		private static bool ConnectAccount()
		{
			return rulesAuthentication.singletonSerializer == 3;
		}

		[SpecialName]
		private static void FindAccount(bool updatelast)
		{
			rulesAuthentication.LoginProcess(3, updatelast);
		}

		[SpecialName]
		private static bool ValidateAccount()
		{
			return rulesAuthentication.singletonSerializer == 4;
		}

		[SpecialName]
		private static void CreateAccount(bool loadlast)
		{
			rulesAuthentication.LoginProcess(4, loadlast);
		}

		[SpecialName]
		private static bool RevertAccount()
		{
			return rulesAuthentication.singletonSerializer == 5;
		}

		[SpecialName]
		private static void RunParams(bool updatelast)
		{
			rulesAuthentication.LoginProcess(5, updatelast);
		}

		public override void OnInspectorGUI()
		{
			if (!FlushConfiguration())
			{
				if (!_Service)
				{
					EnableConfiguration(SelectSingleton);
				}
			}
			else
			{
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
					return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
				})())
				{
					return;
				}
				ConcatSingleton();
				base.serializedObject.Update();
				PrintSingleton();
				ReadConfiguration(_MessageIdentifier);
				EditorGUIUtility.labelWidth = 160f;
				int num = 0;
				AnimBool[] annotationIdentifier = _AnnotationIdentifier;
				num = 1;
				SelectIdentifier("Transforms", annotationIdentifier[0], null, delegate
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
					ResetAccount(SearchConfiguration(_WriterIdentifier, ResolveAccount()));
					EditorGUILayout.PropertyField(classIdentifier);
					using (new GUILayout.VerticalScope("box"))
					{
						using (new GUILayout.HorizontalScope())
						{
							GUILayout.Space(12f);
							m_TemplateIdentifier.isExpanded = EditorGUILayout.Foldout(m_TemplateIdentifier.isExpanded, "Ignore Transforms", toggleOnLabelClick: true);
							GUILayout.FlexibleSpace();
							FindAccount(LoginConfiguration(ConnectAccount(), ExceptionSingletonStruct.CustomizeRef().m_ExporterSerializer));
							EditorGUI.BeginChangeCheck();
							ExcludeAccount(LoginConfiguration(FlushAccount(), ExceptionSingletonStruct.CustomizeRef().advisorSerializer));
							if (EditorGUI.EndChangeCheck())
							{
								PublishSingleton();
							}
						}
						if (m_TemplateIdentifier.isExpanded)
						{
							EditorGUI.indentLevel++;
							ExceptionSingletonStruct.OrderStatus<Transform>(m_TemplateIdentifier);
							EditorGUI.indentLevel--;
						}
					}
				});
				AnimBool[] annotationIdentifier2 = _AnnotationIdentifier;
				num = 2;
				SelectIdentifier("Forces", annotationIdentifier2[1], ViewSingleton, delegate
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
				SelectIdentifier("Limits", _AnnotationIdentifier[num++], null, delegate
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
						if (ExceptionSingletonStruct.CallStatus(ExceptionSingletonStruct.CustomizeRef().baseSerializer, GUI.skin.label, GUILayout.Width(14f)))
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
				SelectIdentifier("Collisions", _AnnotationIdentifier[num++], dir, delegate
				{
					PostSingleton(6);
					if (_ModelAuthentication)
					{
						StopConfiguration(consumerAuthentication, m_UtilsAuthentication);
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new GUILayout.HorizontalScope())
						{
							GUILayout.Space(12f);
							m_SingletonAuthentication.isExpanded = EditorGUILayout.Foldout(m_SingletonAuthentication.isExpanded, "Colliders", toggleOnLabelClick: true);
							GUILayout.FlexibleSpace();
							RunParams(LoginConfiguration(RevertAccount(), ExceptionSingletonStruct.CustomizeRef().m_ExporterSerializer));
							EditorGUI.BeginChangeCheck();
							CreateAccount(LoginConfiguration(ValidateAccount(), ExceptionSingletonStruct.CustomizeRef().advisorSerializer));
							if (EditorGUI.EndChangeCheck())
							{
								MoveSingleton();
							}
						}
						if (m_SingletonAuthentication.isExpanded)
						{
							EditorGUI.indentLevel++;
							ExceptionSingletonStruct.OrderStatus<VRCPhysBoneCollider>(m_SingletonAuthentication);
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
				SelectIdentifier("Grab & Pose", _AnnotationIdentifier[num++], dir2, delegate
				{
					if (_ModelAuthentication)
					{
						while (true)
						{
							StopConfiguration(comparatorAuthentication, _ProductAuthentication);
							StopConfiguration(iteratorAuthentication, m_PredicateAuthentication);
						}
					}
					EditorGUILayout.PropertyField(_CollectionAuthentication);
					EditorGUILayout.PropertyField(interceptorAuthentication);
				});
				SelectIdentifier("Stretch & Squish", _AnnotationIdentifier[num++], null, delegate
				{
					ListSingleton(13);
					ListSingleton(14);
					if (_ValueIdentifier.enumValueIndex > 0)
					{
						ListSingleton(12);
					}
				});
				SelectIdentifier("Options", _AnnotationIdentifier[num++], null, delegate
				{
					EditorGUILayout.PropertyField(_ValueIdentifier);
					EditorGUILayout.PropertyField(resolverAuthentication);
					EditorGUILayout.PropertyField(_FilterAuthentication);
					PushConfiguration();
					using (new GUILayout.HorizontalScope())
					{
						if ((bool)(UnityEngine.Object)(object)m_Predicate)
						{
							List<string> list = new List<string>();
							string[] registry = m_Registry;
							foreach (string text in registry)
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
								stringValue = ExceptionSingletonStruct.PopStatus("Parameter", stringValue, proc);
								if (changeCheckScope.changed)
								{
									tagAuthentication.stringValue = stringValue;
								}
							}
							using (new EditorGUI.DisabledScope((UnityEngine.Object)(object)m_Predicate == null || string.IsNullOrEmpty(tagAuthentication.stringValue)))
							{
								if (ExceptionSingletonStruct.ListStatus(ExceptionSingletonStruct.CustomizeRef().m_DispatcherSerializer))
								{
									GenericMenu genericMenu = new GenericMenu();
									using (IEnumerator<VRCAvatarDescriptor.CustomAnimLayer> enumerator = m_Predicate.baseAnimationLayers.Concat(m_Predicate.specialAnimationLayers).GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											_003C_003Ec__DisplayClass108_1 _003C_003Ec__DisplayClass108_ = new _003C_003Ec__DisplayClass108_1();
											_003C_003Ec__DisplayClass108_.configAuthentication = enumerator.Current;
											_003C_003Ec__DisplayClass108_.m_MockAuthentication = _003C_003Ec__DisplayClass108_.configAuthentication.animatorController as UnityEditor.Animations.AnimatorController;
											if (!(_003C_003Ec__DisplayClass108_.m_MockAuthentication == null))
											{
												UnityEngine.AnimatorControllerParameter[] parameters = _003C_003Ec__DisplayClass108_.m_MockAuthentication.parameters;
												ExceptionSingletonStruct.InstanceConsumerExporter[] visitorSerializer = ExceptionSingletonStruct.m_VisitorSerializer;
												for (int i = 0; i < visitorSerializer.Length; i++)
												{
													_003C_003Ec__DisplayClass108_2 _003C_003Ec__DisplayClass108_2 = new _003C_003Ec__DisplayClass108_2();
													_003C_003Ec__DisplayClass108_2.advisorAuthentication = _003C_003Ec__DisplayClass108_;
													_003C_003Ec__DisplayClass108_2._StateAuthentication = visitorSerializer[i];
													_003C_003Ec__DisplayClass108_2._FieldAuthentication = tagAuthentication.stringValue + _003C_003Ec__DisplayClass108_2._StateAuthentication.workerMethod;
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
					VRCPhysBone vRCPhysBone = base.target as VRCPhysBone;
					if (vRCPhysBone != null && Application.isPlaying && !base.serializedObject.isEditingMultipleObjects && !string.IsNullOrEmpty(vRCPhysBone.parameter))
					{
						using (new EditorGUILayout.HorizontalScope())
						{
							AdvisorMethod.LoginIterator(m_CodeIdentifier, null);
							foreach (ExceptionSingletonStruct.InstanceConsumerExporter item in ExceptionSingletonStruct.m_VisitorSerializer.Where(_003C_003Ec.m_ManagerAuthentication.SortParams))
							{
								using (new EditorGUILayout.VerticalScope())
								{
									GUILayout.Label(item.workerMethod, EditorStyles.boldLabel, GUILayout.ExpandWidth(expand: true));
									AdvisorMethod.PrepareIterator();
									GUILayout.Label(item.PrepareProduct(vRCPhysBone));
								}
								AdvisorMethod.StopIterator();
							}
							AdvisorMethod.CallIterator();
						}
					}
				});
				SelectIdentifier("Gizmos", _AnnotationIdentifier[num++], delegate
				{
					CallConfiguration(m_FactoryAuthentication, "Show Gizmos", delegate
					{
						if ((bool)RefImporterDescriptor.GetConsumer().globalGizmo)
						{
							RefImporterDescriptor.GetConsumer().gizmosActive.ConcatUtils(m_FactoryAuthentication.boolValue);
						}
					}, GUILayout.ExpandWidth(expand: false));
					bool flag;
					string text = ((!(flag = RefImporterDescriptor.GetConsumer().globalGizmo)) ? "Local Setting" : "Global Setting");
					using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, flag, ExceptionSingletonStruct._ObserverSerializer, ExceptionSingletonStruct._EventSerializer))
					{
						using (new RefImporterDescriptor.TestPropertyFilter(InterruptSingleton))
						{
							RefImporterDescriptor.GetConsumer().globalGizmo.ConcatUtils(GUILayout.Toggle(flag, text, GUI.skin.button, GUILayout.ExpandWidth(expand: false)));
						}
					}
				}, delegate
				{
					if ((bool)RefImporterDescriptor.GetConsumer().globalGizmo)
					{
						RefImporterDescriptor.GetConsumer().gizmoBoneOpacity.CheckUtils(EditorGUILayout.Slider("Bone Opacity", RefImporterDescriptor.GetConsumer().gizmoBoneOpacity, 0f, 1f));
						RefImporterDescriptor.GetConsumer().gizmoLimitOpacity.CheckUtils(EditorGUILayout.Slider("Limit Opacitiy", RefImporterDescriptor.GetConsumer().gizmoLimitOpacity, 0f, 1f));
					}
					else
					{
						m_AttributeAuthentication.floatValue = EditorGUILayout.Slider("Bone Opacity", m_AttributeAuthentication.floatValue, 0f, 1f);
						m_InstanceAuthentication.floatValue = EditorGUILayout.Slider("Limit Opacitiy", m_InstanceAuthentication.floatValue, 0f, 1f);
					}
				});
				TestConfiguration(base.serializedObject, _MessageIdentifier, delegate(VRCPhysBone pb)
				{
					pb.configHasUpdated = true;
				});
				GetConfiguration();
				SortIdentifier();
			}
		}

		private void method_0()
		{
			if (rulesAuthentication.singletonSerializer < 0)
			{
				return;
			}
			VRCPhysBone vRCPhysBone = (VRCPhysBone)ChangeSingleton();
			if (!(vRCPhysBone == null))
			{
				Tools.hidden = true;
				ExceptionSingletonStruct.StrategyAuthenticationFactory strategyAuthenticationFactory = new ExceptionSingletonStruct.StrategyAuthenticationFactory(vRCPhysBone);
				strategyAuthenticationFactory.CancelProduct();
				if (ResolveAccount())
				{
					CustomizeSingleton(_MessageIdentifier, strategyAuthenticationFactory);
				}
				if (RemoveAccount())
				{
					CancelSingleton(_MessageIdentifier, strategyAuthenticationFactory, CountAccount());
				}
			}
		}

		private static void VerifySingleton(SceneView task)
		{
			ExceptionSingletonStruct.PublishStatus();
			ConcatSingleton();
			SetSingleton(task);
			if (ValidateAccount())
			{
				bool flag = RefImporterDescriptor.GetConsumer().onSceneNameLabels;
				using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.FG, flag, RefImporterDescriptor.GetConsumer().labelColor.VerifyPage()))
				{
					for (int i = 0; i < m_MapperIdentifier.Length; i++)
					{
						int _ExporterAuthentication = i;
						VRCPhysBoneCollider _CreatorAuthentication = m_MapperIdentifier[_ExporterAuthentication];
						ExceptionSingletonStruct.PageMethod first = ExceptionSingletonStruct.PageMethod.OrderComparator(_CreatorAuthentication.transform.TransformPoint(_CreatorAuthentication.position), flag ? _CreatorAuthentication.name : string.Empty, (float)RefImporterDescriptor.GetConsumer().handleSizeMultiplier * 0.05f, _Iterator + i, delegate
						{
							m_SingletonAuthentication.DestroyStatus<VRCPhysBoneCollider>(ExceptionSingletonStruct.FillVal(m_QueueIdentifier, _ExporterAuthentication), _CreatorAuthentication);
						});
						first._ValMethod = delegate(ExceptionSingletonStruct.PageMethod sc2)
						{
							Handles.color = RefImporterDescriptor.GetConsumer().ExcludeConsumer()[m_QueueIdentifier[_ExporterAuthentication]];
							ExceptionSingletonStruct.PageMethod.DeleteComparator(sc2);
						};
						ExceptionSingletonStruct.InitStatus(first);
					}
				}
			}
			if (FlushAccount())
			{
				bool flag2 = RefImporterDescriptor.GetConsumer().onSceneNameLabels;
				using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.FG, flag2, RefImporterDescriptor.GetConsumer().labelColor.VerifyPage()))
				{
					for (int num = 0; num < mappingIdentifier.Length; num++)
					{
						Transform _ConnectionAuthentication = mappingIdentifier[num];
						int m_DispatcherAuthentication = num;
						ExceptionSingletonStruct.PageMethod first2 = ExceptionSingletonStruct.PageMethod.OrderComparator(_ConnectionAuthentication.position, (!flag2) ? string.Empty : _ConnectionAuthentication.name, (float)RefImporterDescriptor.GetConsumer().handleSizeMultiplier * 0.25f, _Iterator + num, delegate
						{
							m_TemplateIdentifier.DestroyStatus<Transform>(ExceptionSingletonStruct.FillVal(m_QueueIdentifier, m_DispatcherAuthentication), _ConnectionAuthentication);
						});
						first2._ValMethod = delegate(ExceptionSingletonStruct.PageMethod sc2)
						{
							Handles.color = RefImporterDescriptor.GetConsumer().ExcludeConsumer()[m_QueueIdentifier[m_DispatcherAuthentication]];
							ExceptionSingletonStruct.PageMethod.DeleteComparator(sc2);
						};
						ExceptionSingletonStruct.InitStatus(first2);
					}
				}
			}
			if (RevertAccount())
			{
				bool flag3 = RefImporterDescriptor.GetConsumer().onSceneNameLabels;
				Handles.color = RefImporterDescriptor.GetConsumer().selectionColor.VerifyPage();
				using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.FG, flag3, RefImporterDescriptor.GetConsumer().labelColor.VerifyPage()))
				{
					for (int num2 = 0; num2 < _PolicyIdentifier.Length; num2++)
					{
						VRCPhysBone vRCPhysBone = _PolicyIdentifier[num2];
						int _ExpressionAuthentication = num2;
						ExceptionSingletonStruct.InitStatus(ExceptionSingletonStruct.PageMethod.OrderComparator(vRCPhysBone.transform.position, flag3 ? vRCPhysBone.name : string.Empty, (float)RefImporterDescriptor.GetConsumer().handleSizeMultiplier * 0.25f, _Iterator + num2, delegate
						{
							VRCPhysBone[] messageIdentifier = _MessageIdentifier;
							for (int j = 0; j < messageIdentifier.Length; j++)
							{
								messageIdentifier[j].colliders = _PolicyIdentifier[_ExpressionAuthentication].colliders.ToList();
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
			if (ConnectAccount())
			{
				bool flag4 = RefImporterDescriptor.GetConsumer().onSceneNameLabels;
				Handles.color = RefImporterDescriptor.GetConsumer().selectionColor.VerifyPage();
				using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.FG, flag4, RefImporterDescriptor.GetConsumer().labelColor.VerifyPage()))
				{
					for (int num3 = 0; num3 < _PolicyIdentifier.Length; num3++)
					{
						VRCPhysBone vRCPhysBone2 = _PolicyIdentifier[num3];
						int _DecoratorAuthentication = num3;
						ExceptionSingletonStruct.InitStatus(ExceptionSingletonStruct.PageMethod.OrderComparator(vRCPhysBone2.transform.position, flag4 ? vRCPhysBone2.name : string.Empty, (float)RefImporterDescriptor.GetConsumer().handleSizeMultiplier * 0.25f, _Iterator + num3, delegate
						{
							VRCPhysBone[] messageIdentifier = _MessageIdentifier;
							for (int j = 0; j < messageIdentifier.Length; j++)
							{
								messageIdentifier[j].ignoreTransforms = _PolicyIdentifier[_DecoratorAuthentication].ignoreTransforms.ToList();
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
			if (Tools.current != Tool.View && !current.alt && (bool)RefImporterDescriptor.GetConsumer().ignoreSceneClicks && rulesAuthentication.singletonSerializer > 0 && current.type == EventType.MouseDown && current.button == 0)
			{
				GUIUtility.hotControl = _Iterator - 1;
				current.Use();
			}
			ExceptionSingletonStruct.PrintStatus();
		}

		private static void SetSingleton(SceneView info)
		{
			Rect ivk = info.AddStatus();
			int num = rulesAuthentication.singletonSerializer;
			if (num < 0)
			{
				num = 0;
			}
			bool flag = num > 0;
			if ((bool)RefImporterDescriptor.GetConsumer().onSceneToolSelection && (flag || (bool)RefImporterDescriptor.GetConsumer().onSceneToolSelectionAlwaysVisible))
			{
				ExceptionSingletonStruct.PositionFlag positionFlag = RefImporterDescriptor.GetConsumer().toolSelectionOverlayAlignment.RegisterUtils<ExceptionSingletonStruct.PositionFlag>();
				bool flag2;
				using (new ExceptionSingletonStruct.SystemSerializer(info, 250f, 34f, positionFlag, m_ExceptionIdentifier))
				{
					Rect lastRect;
					using (new GUILayout.HorizontalScope())
					{
						using (new EditorGUI.DisabledScope(rulesAuthentication.singletonSerializer <= 0))
						{
							if (ExceptionSingletonStruct.ListStatus((!RefImporterDescriptor.GetConsumer().ignoreSceneClicks) ? ExceptionSingletonStruct.CustomizeRef().decoratorSerializer : ExceptionSingletonStruct.CustomizeRef()._ParamSerializer))
							{
								RefImporterDescriptor.GetConsumer().ignoreSceneClicks.IncludeConsumer();
							}
						}
						GUILayout.FlexibleSpace();
						GUILayout.Label("ADO Tool:", ExceptionSingletonStruct.MapRef().m_WriterSerializer);
						lastRect = GUILayoutUtility.GetLastRect();
						GUIContent content = new GUIContent(m_TestsAuthentication[num]);
						float x = GUI.skin.label.CalcSize(content).x;
						EditorGUI.BeginChangeCheck();
						int singletonSerializer = EditorGUILayout.Popup(GUIContent.none, num, m_TestsAuthentication, GUILayout.Width(x + 20f));
						if (EditorGUI.EndChangeCheck())
						{
							rulesAuthentication.singletonSerializer = singletonSerializer;
							if (rulesAuthentication.singletonSerializer == 0)
							{
								SetupSingleton();
							}
							else if (rulesAuthentication.singletonSerializer == 6)
							{
								LogoutSingleton(0);
							}
							else
							{
								LogoutSingleton(-1);
								if (rulesAuthentication.singletonSerializer > 1 && rulesAuthentication.singletonSerializer < 4)
								{
									PublishSingleton();
								}
								else
								{
									MoveSingleton();
								}
							}
							SceneView.RepaintAll();
						}
						GUILayout.FlexibleSpace();
						if (ExceptionSingletonStruct.ListStatus(ExceptionSingletonStruct.CustomizeRef().fieldSerializer))
						{
							MessageUtilsAttribute.CreateSerializer();
						}
					}
					flag2 = ExceptionSingletonStruct.VisitStatus(lastRect, _TokenizerIdentifier);
					ExceptionSingletonStruct.InsertStatus(lastRect, MouseCursor.Pan);
				}
				if (flag2)
				{
					Handles.BeginGUI();
					RefImporterDescriptor.GetConsumer().toolSelectionOverlayAlignment.RestartUtils = (int)ExceptionSingletonStruct.RunStatus(positionFlag, ivk);
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
				WriteIdentifier(first, "Gizmos Disabled", delegate
				{
					GUILayout.Label("Handles are hidden.", ExceptionSingletonStruct.MapRef().m_WriterSerializer);
					if (ExceptionSingletonStruct.PatchStatus("Enable Gizmos"))
					{
						first.drawGizmos = true;
					}
				}, 200f, 80f);
			}
			return drawGizmos;
		}

		private static void InvokeSingleton(SceneView value)
		{
			if (!RefImporterDescriptor.GetConsumer().onSceneEditingOverlay || rulesAuthentication.singletonSerializer <= 0)
			{
				return;
			}
			bool _IssuerAuthentication = RemoveAccount();
			bool codeAuthentication = RefImporterDescriptor.GetConsumer().onSceneTooltip;
			if (!_IssuerAuthentication && !codeAuthentication)
			{
				return;
			}
			bool m_PrototypeAuthentication = _MessageIdentifier.Length > 1;
			bool flag = FlushAccount();
			bool flag2 = ValidateAccount();
			bool flag3 = ConnectAccount();
			bool flag4 = RevertAccount();
			bool callbackAuthentication = ResolveAccount();
			bool facadeAuthentication = flag || flag3;
			bool _ComposerAuthentication = flag2 || flag4;
			bool m_RequestAuthentication = flag3 || flag4;
			bool _BaseAuthentication = callbackAuthentication || _IssuerAuthentication;
			float ivk = ((!codeAuthentication) ? 33 : ((!_IssuerAuthentication && !callbackAuthentication) ? 60 : (m_PrototypeAuthentication ? 100 : ((!_IssuerAuthentication) ? 60 : 80))));
			float pol = ((!m_PrototypeAuthentication) ? 240 : 340);
			Rect annotationAuthentication;
			MoveIdentifier(value, delegate
			{
				using (new GUILayout.HorizontalScope())
				{
					string text = string.Concat(((!m_PrototypeAuthentication) ? "" : "Multi-") + (_BaseAuthentication ? "Editing" : ((!m_RequestAuthentication) ? "Selecting" : "Copying")), _IssuerAuthentication ? ":" : (facadeAuthentication ? " Ignore Transforms" : ((!_ComposerAuthentication) ? " End Position" : " Colliders")));
					ExceptionSingletonStruct.AssetStatus();
					GUILayout.FlexibleSpace();
					GUILayout.Label(text, ExceptionSingletonStruct.MapRef().m_WriterSerializer);
					annotationAuthentication = GUILayoutUtility.GetLastRect();
					if (_IssuerAuthentication)
					{
						EditorGUI.BeginChangeCheck();
						GUIContent content = m_CustomerAuthentication[m_StubAuthentication];
						float x = GUI.skin.label.CalcSize(content).x;
						int key = EditorGUILayout.IntPopup(GUIContent.none, _CandidateAuthentication[m_StubAuthentication], m_CustomerAuthentication, m_DatabaseAuthentication, GUILayout.Width(x + 20f));
						if (EditorGUI.EndChangeCheck())
						{
							m_StubAuthentication = _HelperAuthentication[key];
							SceneView.RepaintAll();
						}
					}
					GUILayout.FlexibleSpace();
					PublishIdentifier();
					return annotationAuthentication;
				}
			}, delegate
			{
				if (codeAuthentication)
				{
					GUILayout.Label("Press Enter or Escape to exit", ExceptionSingletonStruct.MapRef().m_WriterSerializer);
					if (_IssuerAuthentication || callbackAuthentication)
					{
						if (m_PrototypeAuthentication)
						{
							GUILayout.Label("Hold Alt to edit the target physbone only", ExceptionSingletonStruct.MapRef().m_WriterSerializer);
							GUILayout.Label("Hold Shift to set the physbones to the same value", ExceptionSingletonStruct.MapRef().m_WriterSerializer);
						}
						else if (_IssuerAuthentication)
						{
							GUILayout.Label("Hold Alt to edit the curve", ExceptionSingletonStruct.MapRef().m_WriterSerializer);
						}
					}
				}
			}, pol, ivk);
		}

		private void CustomizeSingleton(VRCPhysBone[] res, ExceptionSingletonStruct.StrategyAuthenticationFactory result)
		{
			_003C_003Ec__DisplayClass116_0 vis = default(_003C_003Ec__DisplayClass116_0);
			vis._MessageAuthentication = res;
			vis._PolicyAuthentication = result.definitionMethod;
			ExceptionSingletonStruct.ClientRegDic[] array = result.m_TokenMethod.Where((ExceptionSingletonStruct.ClientRegDic b) => b.m_DescriptorMethod && !b.m_RepositoryMethod).ToArray();
			foreach (ExceptionSingletonStruct.ClientRegDic clientRegDic in array)
			{
				Transform parserMethod = clientRegDic.parserMethod;
				Vector3 vector = parserMethod.TransformPoint(vis._PolicyAuthentication.endpointPosition);
				if (!vis._PolicyAuthentication.showGizmos || !(vis._PolicyAuthentication.boneOpacity >= 0.05f))
				{
					Handles.DrawLine(parserMethod.position, vector);
				}
				Quaternion rotation = ((Tools.pivotRotation != PivotRotation.Global) ? parserMethod.rotation : Quaternion.identity);
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
					direction = vector - parserMethod.position;
					if (!(direction.magnitude >= 0.01f))
					{
						direction = ((clientRegDic.managerMethod == null) ? (-parserMethod.forward) : (vector - clientRegDic.managerMethod.parserMethod.position));
					}
				}
				else
				{
					direction = initializerAuthentication;
				}
				Handles.color = ExceptionSingletonStruct._EventSerializer;
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
					SearchSingleton(parserMethod.InverseTransformVector(vector2 - vector), ref vis);
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
			if (rulesAuthentication.singletonSerializer >= 0)
			{
				if (keyCode == KeyCode.Return || keyCode == KeyCode.KeypadEnter || keyCode == KeyCode.Escape)
				{
					SetupSingleton();
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
					if (!RemoveAccount())
					{
						LogoutSingleton(0);
					}
					else
					{
						SetupSingleton();
					}
					if (_Account)
					{
						NewConfiguration();
					}
					current.Use();
					break;
				case KeyCode.T:
					NewConfiguration();
					current.Use();
					break;
				}
			}
		}

		private static void MapSingleton(ExceptionSingletonStruct.StrategyAuthenticationFactory def, AnimationCurve pred, Action<ExceptionSingletonStruct.ClientRegDic, float> proc, bool isinstance2 = false)
		{
			bool flag = pred == null || pred.length == 0;
			foreach (ExceptionSingletonStruct.ClientRegDic item in def.m_TokenMethod)
			{
				float num = ((!flag) ? pred.Evaluate(item.RegisterProduct()) : 1f);
				if (isinstance2)
				{
					num *= item.CheckProduct();
				}
				proc(item, num);
			}
		}

		private static void FillSingleton(float last, ExceptionSingletonStruct.ClientRegDic col, SerializedProperty proc, SerializedProperty selection2, float ident3 = 0f, float item4 = float.PositiveInfinity)
		{
			AnimationCurve animationCurve = ((selection2.animationCurveValue != null && selection2.animationCurveValue.length >= 2) ? selection2.animationCurveValue : new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f)));
			int num = -1;
			Keyframe[] keys = animationCurve.keys;
			for (int i = 0; i < keys.Length; i++)
			{
				if (!(Math.Abs(keys[i].time - col.RegisterProduct()) >= 0.01f))
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
				num2 = animationCurve.Evaluate(col.RegisterProduct());
				num = animationCurve.AddKey(col.RegisterProduct(), num2);
			}
			float num3 = col.ForgotProduct(animationCurve);
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
					animationCurve.MoveKey(num, new Keyframe(col.RegisterProduct(), flag ? (-1) : 0));
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
					animationCurve.MoveKey(num, new Keyframe(col.RegisterProduct(), 1f));
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
				animationCurve.MoveKey(num, new Keyframe(col.RegisterProduct(), num4));
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

		private static void CancelSingleton(VRCPhysBone[] instance, ExceptionSingletonStruct.StrategyAuthenticationFactory result, AlgoAuthentication comp)
		{
			_003C_003Ec__DisplayClass120_0 CS_0024_003C_003E8__locals28 = new _003C_003Ec__DisplayClass120_0();
			CS_0024_003C_003E8__locals28.exceptionAuthentication = instance;
			CS_0024_003C_003E8__locals28._ClassAuthentication = comp;
			CS_0024_003C_003E8__locals28.m_TokenizerAuthentication = result.definitionMethod;
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
			float num = RefImporterDescriptor.GetConsumer().handleSizeMultiplier;
			float num2 = Mathf.Clamp(HandleUtility.GetHandleSize(CS_0024_003C_003E8__locals28.m_TokenizerAuthentication.transform.position) * 0.05f * num, 0.02f * num, num * 2f);
			_ = EditorStyles.boldLabel;
			Color color = RefImporterDescriptor.GetConsumer().generalColor.VerifyPage();
			Color color2 = Handles.color;
			Handles.color = color;
			AnimationCurve animationCurveValue = CS_0024_003C_003E8__locals28.m_ErrorAuthentication.animationCurveValue;
			List<List<ExceptionSingletonStruct.ClientRegDic>> threadMethod = result.threadMethod;
			if (CS_0024_003C_003E8__locals28._ClassAuthentication.m_StrategyAuthentication == 1)
			{
				MapSingleton(result, animationCurveValue, delegate(ExceptionSingletonStruct.ClientRegDic b, float m)
				{
					if (m != 0f)
					{
						Matrix4x4 printerMethod = b.printerMethod;
						Vector4 column = printerMethod.GetColumn(3);
						float comp2 = CS_0024_003C_003E8__locals28.m_TokenizerAuthentication.radius * m;
						EditorGUI.BeginChangeCheck();
						float num9 = ExceptionSingletonStruct.CreateStatus(printerMethod.rotation, column, comp2, !CS_0024_003C_003E8__locals28.m_TokenizerAuthentication.showGizmos, RefImporterDescriptor.GetConsumer().handleSizeMultiplier);
						if (EditorGUI.EndChangeCheck())
						{
							float delta = num9 / m - CS_0024_003C_003E8__locals28.m_TokenizerAuthentication.radius;
							CS_0024_003C_003E8__locals28.CalculateServer(b, delta);
						}
						ExceptionSingletonStruct.FindStatus(comp2.ToString("F2"), column);
					}
				}, isinstance2: true);
			}
			else
			{
				Vector3 vector = Vector3.zero;
				Vector3[][] array = new Vector3[threadMethod.Count][];
				for (int num3 = 0; num3 < threadMethod.Count; num3++)
				{
					List<ExceptionSingletonStruct.ClientRegDic> list = threadMethod[num3];
					array[num3] = new Vector3[list.Count];
					Vector3 vector2 = Vector3.zero;
					for (int num4 = 0; num4 < list.Count; num4++)
					{
						ExceptionSingletonStruct.ClientRegDic clientRegDic = list[num4];
						Vector3 vector3 = ((num4 == 0) ? clientRegDic.LoginProduct() : vector2);
						if (num4 != list.Count - 1)
						{
							vector2 = list[num4 + 1].LoginProduct();
							vector = vector2 - vector3;
						}
						if (!(Vector3.Angle(Vector3.right, vector) >= 90f))
						{
							vector = -vector;
						}
						Vector3 up = Vector3.up;
						float num5 = clientRegDic.ForgotProduct(animationCurveValue);
						float num6 = CS_0024_003C_003E8__locals28.valueAuthentication.floatValue * num5;
						Vector3 vector4 = vector3 + up * (num * (num6 / CS_0024_003C_003E8__locals28._TemplateAuthentication));
						array[num3][num4] = vector4;
						Handles.DrawDottedLine(vector3, vector4, 5f);
						ExceptionSingletonStruct.FindStatus(num6.ToString("F2"), vector4, num2 + 0.01f);
						Vector3 vector5 = Handles.Slider(vector4, up, num2, Handles.DotHandleCap, 0f);
						if (!(vector4 == vector5))
						{
							float num7 = (vector5.y - vector4.y) / num * CS_0024_003C_003E8__locals28._TemplateAuthentication;
							if (num5 < 0f)
							{
								num7 *= -1f;
							}
							CS_0024_003C_003E8__locals28.CalculateServer(clientRegDic, num7);
						}
					}
				}
				Vector3[][] array2 = array;
				foreach (Vector3[] points in array2)
				{
					Handles.DrawAAPolyLine(3f * num, points);
				}
			}
			Handles.color = color2;
		}

		private static void LogoutSingleton(int end_v)
		{
			if (end_v < 0 || (RemoveAccount() && m_StubAuthentication == end_v))
			{
				m_StubAuthentication = -1;
				rulesAuthentication.LoginProcess(6, ispred: false);
			}
			else
			{
				m_StubAuthentication = end_v;
				rulesAuthentication.SearchProcess(6);
			}
			SceneView.RepaintAll();
		}

		internal static void SetupSingleton()
		{
			rulesAuthentication.PatchProcess();
			m_StubAuthentication = -1;
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
				m_TokenAuthentication = ExceptionSingletonStruct.CancelStatus("VRCPhysBone");
			}
			if (getterAuthentication == null)
			{
				getterAuthentication = ExceptionSingletonStruct.CancelStatus("VRCPhysBoneEditor");
			}
			_CallbackIdentifier = !issetup;
			ExceptionSingletonStruct.RevertStatus(m_TokenAuthentication, (!_CallbackIdentifier) ? getterAuthentication : typeof(ComposerIdentifier));
		}

		private static void MoveSingleton()
		{
			m_QueueIdentifier = new byte[m_MapperIdentifier.Length];
			bool flag = true;
			VRCPhysBone[] messageIdentifier = _MessageIdentifier;
			foreach (VRCPhysBone vRCPhysBone in messageIdentifier)
			{
				for (int j = 0; j < m_QueueIdentifier.Length; j++)
				{
					if (m_QueueIdentifier[j] == 2)
					{
						continue;
					}
					List<VRCPhysBoneColliderBase> colliders = vRCPhysBone.colliders;
					if (colliders != null && colliders.Contains(m_MapperIdentifier[j]))
					{
						if (m_QueueIdentifier[j] != 0 || flag)
						{
							m_QueueIdentifier[j] = 1;
						}
						else
						{
							m_QueueIdentifier[j] = 2;
						}
					}
					else if (m_QueueIdentifier[j] == 1 && !flag)
					{
						m_QueueIdentifier[j] = 2;
					}
					else
					{
						m_QueueIdentifier[j] = 0;
					}
				}
				flag = false;
			}
		}

		private static void PublishSingleton()
		{
			VRCPhysBone[] messageIdentifier;
			if (!FlushAccount())
			{
				bool flag = false;
				messageIdentifier = _MessageIdentifier;
				foreach (VRCPhysBone vRCPhysBone in messageIdentifier)
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
					NewIdentifier("Optimized ignore transforms.");
				}
				return;
			}
			m_QueueIdentifier = new byte[mappingIdentifier.Length];
			bool flag2 = true;
			messageIdentifier = _MessageIdentifier;
			foreach (VRCPhysBone vRCPhysBone2 in messageIdentifier)
			{
				for (int num2 = 0; num2 < m_QueueIdentifier.Length; num2++)
				{
					if (m_QueueIdentifier[num2] == 2)
					{
						continue;
					}
					List<Transform> ignoreTransforms = vRCPhysBone2.ignoreTransforms;
					if (ignoreTransforms != null && ignoreTransforms.Contains(mappingIdentifier[num2]))
					{
						if (m_QueueIdentifier[num2] != 0 || flag2)
						{
							m_QueueIdentifier[num2] = 1;
						}
						else
						{
							m_QueueIdentifier[num2] = 2;
						}
					}
					else if (m_QueueIdentifier[num2] == 1 && !flag2)
					{
						m_QueueIdentifier[num2] = 2;
					}
					else
					{
						m_QueueIdentifier[num2] = 0;
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
				float[] array = new float[ExceptionSingletonStruct.m_VisitorSerializer.Count((ExceptionSingletonStruct.InstanceConsumerExporter p) => p._IndexerMethod)];
				for (int num = 0; num < array.Length; num++)
				{
					array[num] = 1f / (float)array.Length;
				}
				m_CodeIdentifier = AdvisorMethod.SearchIterator(array);
			}
		}

		private void OnEnable()
		{
			m_ProcessorIdentifier = this;
			CollectSingleton();
			SelectConfiguration(_AnnotationIdentifier, Repaint);
			InterruptSingleton();
			PrintConfiguration(ref m_Predicate, ref _Collection);
			LogoutConfiguration();
			Transform root = ((VRCPhysBone)ChangeSingleton()).transform.root;
			_MessageIdentifier = base.targets.Cast<VRCPhysBone>().ToArray();
			m_MapperIdentifier = root.GetComponentsInChildren<VRCPhysBoneCollider>();
			_PolicyIdentifier = root.GetComponentsInChildren<VRCPhysBone>();
			mappingIdentifier = _MessageIdentifier.SelectMany((VRCPhysBone pb) => pb.GetRootTransform().GetComponentsInChildren<Transform>()).ToArray();
			SceneView.duringSceneGui -= VerifySingleton;
			SceneView.duringSceneGui += VerifySingleton;
		}

		private void OnDisable()
		{
			SetupSingleton();
			SceneView.duringSceneGui -= VerifySingleton;
			Tools.hidden = false;
		}

		private void PrintSingleton()
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
			taskAuthentication = new AlgoAuthentication[15]
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
			if (m_ReaderAuthentication)
			{
				return;
			}
			List<GUIContent> list = new List<GUIContent>();
			_HelperAuthentication = new Dictionary<int, int>();
			_CandidateAuthentication = new Dictionary<int, int>();
			int key = 0;
			for (int i = 0; i < taskAuthentication.Length; i++)
			{
				AlgoAuthentication algoAuthentication = taskAuthentication[i];
				if (algoAuthentication._GlobalAuthentication)
				{
					list.Add(new GUIContent(algoAuthentication.roleAuthentication));
					_HelperAuthentication.Add(key, i);
					_CandidateAuthentication.Add(i, key++);
				}
			}
			m_CustomerAuthentication = list.ToArray();
			m_DatabaseAuthentication = _HelperAuthentication.Keys.ToArray();
			m_ReaderAuthentication = true;
		}

		internal static void InterruptSingleton()
		{
			if ((bool)RefImporterDescriptor.GetConsumer().globalGizmo)
			{
				VRCPhysBone[] array = UnityEngine.Object.FindObjectsOfType<VRCPhysBone>();
				foreach (VRCPhysBone obj in array)
				{
					obj.showGizmos = RefImporterDescriptor.GetConsumer().gizmosActive;
					obj.boneOpacity = RefImporterDescriptor.GetConsumer().gizmoBoneOpacity;
					obj.limitOpacity = RefImporterDescriptor.GetConsumer().gizmoLimitOpacity;
				}
			}
		}

		private void ViewSingleton()
		{
			bool flag = _ErrorIdentifier.enumValueIndex == 1;
			int positionmap = (_ErrorIdentifier.hasMultipleDifferentValues ? 2 : _ErrorIdentifier.enumValueIndex);
			using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
			using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, positionmap, ExceptionSingletonStruct.MapRef().m_SerializerMethod))
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
			AlgoAuthentication algoAuthentication = taskAuthentication[ID_key];
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
				using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, RemoveAccount() && CountAccount() == algoAuthentication, ExceptionSingletonStruct._ObserverSerializer, ExceptionSingletonStruct._BroadcasterSerializer))
				{
					if (ExceptionSingletonStruct.CallStatus(ExceptionSingletonStruct.CustomizeRef().stateSerializer, ExceptionSingletonStruct.MapRef().methodMethod, GUILayout.ExpandWidth(expand: false)))
					{
						LogoutSingleton(ID_key);
					}
				}
			}
		}

		private static void ListSingleton(int var1)
		{
			if (taskAuthentication[var1]._GlobalAuthentication)
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
				return LoginConfiguration(countparam, ExceptionSingletonStruct.CustomizeRef().stateSerializer);
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
				GUI.Label(GUILayoutUtility.GetLastRect(), "///////////////////////////////", ExceptionSingletonStruct.MapRef().m_ProcSerializer);
			}
			if (isproc && ExceptionSingletonStruct.CallStatus(ExceptionSingletonStruct.CustomizeRef().baseSerializer, GUI.skin.label, GUILayout.Width(14f)))
			{
				i.animationCurveValue = new AnimationCurve();
			}
		}

		[CompilerGenerated]
		internal static void SearchSingleton(Vector3 first, ref _003C_003Ec__DisplayClass116_0 vis)
		{
			Event current = Event.current;
			bool alt = current.alt;
			if (vis._MessageAuthentication.Length == 1)
			{
				LoginSingleton(vis._PolicyAuthentication, first);
			}
			else if (!alt)
			{
				if (current.shift)
				{
					Vector3 _MapperAuthentication = LoginSingleton(vis._PolicyAuthentication, first);
					VRCPhysBone[] array = vis._MessageAuthentication;
					foreach (VRCPhysBone vRCPhysBone in array)
					{
						if (vRCPhysBone != vis._PolicyAuthentication)
						{
							PatchSingleton(vRCPhysBone, _WriterIdentifier.propertyPath, delegate(SerializedProperty sp)
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
						LoginSingleton(array[i], first);
					}
				}
			}
			else
			{
				LoginSingleton(vis._PolicyAuthentication, first);
			}
		}

		[CompilerGenerated]
		internal static Vector3 LoginSingleton(VRCPhysBone init, Vector3 map)
		{
			Vector3 m_ProcessorAuthentication = Vector3.zero;
			PatchSingleton(init, _WriterIdentifier.propertyPath, delegate(SerializedProperty sp)
			{
				sp.vector3Value += map;
				m_ProcessorAuthentication = sp.vector3Value;
			});
			return m_ProcessorAuthentication;
		}

		[CompilerGenerated]
		internal static void PatchSingleton(VRCPhysBone v, string counter, Action<SerializedProperty> dir)
		{
			SerializedObject obj = new SerializedObject(v);
			obj.UpdateIfRequiredOrScript();
			SerializedProperty obj2 = obj.FindProperty(counter);
			dir(obj2);
			obj.ApplyModifiedProperties();
		}

		UnityEngine.Object ChangeSingleton()
		{
			return base.target;
		}

		internal static bool CompareTask()
		{
			return (object)ReflectTask == null;
		}
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec m_ProcAuthentication = new _003C_003Ec();

		public static Func<bool> configurationContext;

		public static Func<bool> identifierContext;

		public static Func<UnityEngine.Object, ExceptionSingletonStruct.TaskMethod> m_AuthenticationContext;

		public static Func<UnityEngine.Object, ExceptionSingletonStruct.TaskMethod> contextContext;

		public static Func<UnityEngine.Object, ExceptionSingletonStruct.TaskMethod> _SerializerContext;

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

		public static Action<ParamsIdentifier> _TagContext;

		public static Action<Exception> filterContext;

		public static Func<AuthenticationIdentifier, bool> m_FactoryContext;

		public static Func<string, bool> m_AttributeContext;

		public static Func<(bool, string), bool> m_InstanceContext;

		public static Func<(bool, string), string> m_TaskContext;

		public static Func<(bool, string), bool> _CustomerContext;

		public static Func<(bool, string), string> _DatabaseContext;

		public static Func<bool> helperContext;

		public static Func<bool> _CandidateContext;

		public static Action<ParamsIdentifier> readerContext;

		public static Action<Exception> stubContext;

		public static Action _RulesContext;

		public static Action _TestsContext;

		public static Action<ParamsIdentifier> definitionContext;

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

		internal static _003C_003Ec InitAnnotation;

		internal bool CollectServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		}

		internal bool PrintServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		}

		internal ExceptionSingletonStruct.TaskMethod InterruptServer(UnityEngine.Object t2)
		{
			return new ExceptionSingletonStruct.TaskMethod((VRCPhysBoneCollider)t2);
		}

		internal ExceptionSingletonStruct.TaskMethod ViewServer(UnityEngine.Object t2)
		{
			return new ExceptionSingletonStruct.TaskMethod((VRCContactSender)t2);
		}

		internal ExceptionSingletonStruct.TaskMethod PostServer(UnityEngine.Object t2)
		{
			return new ExceptionSingletonStruct.TaskMethod((VRCContactReceiver)t2);
		}

		internal bool ListServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		}

		internal bool ForgotServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		}

		internal bool UpdateServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		}

		internal bool SearchServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		}

		internal bool LoginServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		}

		internal Rect PatchServer()
		{
			using (new GUILayout.HorizontalScope())
			{
				bool ignorecaller;
				string tooltip = ((!(ignorecaller = RefImporterDescriptor.GetConsumer().hideToolsDuringTesting)) ? "Native tools are visible during test." : "Native tools are hidden during test.");
				using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.FG, ignorecaller, ExceptionSingletonStruct._ObserverSerializer, ExceptionSingletonStruct._BroadcasterSerializer))
				{
					if (ExceptionSingletonStruct.ListStatus(new GUIContent(ExceptionSingletonStruct.CustomizeRef().prototypeSerializer)
					{
						tooltip = tooltip
					}))
					{
						RefImporterDescriptor.GetConsumer().hideToolsDuringTesting.IncludeConsumer();
						Tools.hidden = false;
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.Label("Testing", ExceptionSingletonStruct.MapRef().m_WriterSerializer);
				Rect lastRect = GUILayoutUtility.GetLastRect();
				GUILayout.FlexibleSpace();
				PublishIdentifier();
				return lastRect;
			}
		}

		internal void CheckServer()
		{
			using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, ExceptionSingletonStruct._BroadcasterSerializer))
			{
				if (ExceptionSingletonStruct.PatchStatus("Stop Testing") || ExceptionSingletonStruct.QueryStatus() || ExceptionSingletonStruct.ComputeStatus())
				{
					NewConfiguration();
				}
			}
			using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, ExceptionSingletonStruct.m_RecordSerializer))
			{
				if (ExceptionSingletonStruct.PatchStatus("Restart"))
				{
					CompareConfiguration();
				}
			}
			using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, m_Stub, ExceptionSingletonStruct._ObserverSerializer))
			{
				using (new EditorGUI.DisabledScope(!m_Stub))
				{
					if (!ExceptionSingletonStruct.PatchStatus("Apply All Changes"))
					{
						return;
					}
					foreach (UnityEngine.Object item in _Test.Keys.ToList())
					{
						if (_Test[item])
						{
							UnityEngine.Object obj = candidate[item];
							if (obj != null)
							{
								Undo.RecordObject(obj, "ADO - Apply Changes");
								EditorUtility.CopySerialized(item, obj);
								_Test[item] = false;
							}
						}
					}
					m_Stub = false;
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
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
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
			return !ExceptionSingletonStruct._AlgoSerializer.Contains(p);
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
			if (!(b != null) || !_Test.ContainsKey(b))
			{
				return false;
			}
			return candidate[b] != null;
		}

		internal bool RateServer(UnityEngine.Object b)
		{
			return _Test[b];
		}

		internal void CloneServer()
		{
			role = false;
			m_Algo = false;
			CalculateIdentifier();
		}

		internal void ComputeServer()
		{
			AssetConfiguration(testkey: false);
		}

		internal bool QueryServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		}

		internal void CountServer(Exception exception)
		{
			_Struct = false;
			_Service = false;
			indexer = true;
			NewIdentifier($"Something went wrong while verifying license:\n\n{exception}", CustomLogType.Error);
		}

		internal void StartServer(ParamsIdentifier response)
		{
			_Rule = false;
			QueryConfiguration(response, delegate
			{
				_Worker = false;
				RefImporterDescriptor.GetConsumer().a_HasSucceededLastVerification.ConcatUtils(nores: true);
				AssetConfiguration(testkey: true);
			});
		}

		internal void RemoveServer()
		{
			_Worker = false;
			RefImporterDescriptor.GetConsumer().a_HasSucceededLastVerification.ConcatUtils(nores: true);
			AssetConfiguration(testkey: true);
		}

		internal void ReflectServer(Exception exception)
		{
			_Rule = false;
			NewIdentifier($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
		}

		internal bool ResolveServer(AuthenticationIdentifier p)
		{
			return p.m_SingletonIdentifier;
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
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		}

		internal bool FindServer()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		}

		internal void AddServer()
		{
			List<(string, string)> list = CountConfiguration("transferlicenserequest");
			StartConfiguration(list);
			OrderIdentifier(IncludeConfiguration(list.ToArray())).CreateProcess(delegate(ParamsIdentifier response)
			{
				_003C_003Ec__DisplayClass179_0 _003C_003Ec__DisplayClass179_ = new _003C_003Ec__DisplayClass179_0
				{
					composerContext = response
				};
				_Mock = false;
				QueryConfiguration(_003C_003Ec__DisplayClass179_.composerContext, _003C_003Ec__DisplayClass179_.RestartReg);
			}, delegate(Exception exception)
			{
				_Mock = false;
				NewIdentifier($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, CalculateIdentifier);
		}

		internal void ValidateServer(ParamsIdentifier response)
		{
			_003C_003Ec__DisplayClass179_0 _003C_003Ec__DisplayClass179_ = new _003C_003Ec__DisplayClass179_0
			{
				composerContext = response
			};
			_Mock = false;
			QueryConfiguration(_003C_003Ec__DisplayClass179_.composerContext, _003C_003Ec__DisplayClass179_.RestartReg);
		}

		internal void CreateServer(Exception exception)
		{
			_Mock = false;
			NewIdentifier($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
		}

		internal void IncludeServer()
		{
			List<(string, string)> list = CountConfiguration("transferlicenseconfirm");
			list.Add(("verification_code", descriptor));
			StartConfiguration(list);
			OrderIdentifier(IncludeConfiguration(list.ToArray())).CreateProcess(delegate(ParamsIdentifier response)
			{
				state = false;
				QueryConfiguration(response, delegate
				{
					_Info = false;
					m_Config = false;
					while (true)
					{
						_Worker = false;
					}
				});
			}, delegate(Exception exception)
			{
				state = false;
				NewIdentifier($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, CalculateIdentifier);
		}

		internal void RevertServer(ParamsIdentifier response)
		{
			state = false;
			QueryConfiguration(response, delegate
			{
				_Info = false;
				m_Config = false;
				while (true)
				{
					_Worker = false;
				}
			});
		}

		internal void RunWatcher()
		{
			_Info = false;
			m_Config = false;
			while (true)
			{
				_Worker = false;
			}
		}

		internal void OrderWatcher(Exception exception)
		{
			state = false;
			NewIdentifier($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
		}

		internal void CalculateWatcher()
		{
			SessionState.EraseString("No1lKII9IzcBAbihub6nCg==updateinfo");
			FillIdentifier();
		}

		internal void CalcWatcher()
		{
			m_Algo.SetStatus();
		}

		internal void DeleteWatcher()
		{
			RefImporterDescriptor.GetConsumer().a_VerifyOnDisplay.IncludeConsumer();
			RefImporterDescriptor.GetConsumer().a_VerifyOnProjectLoad.ConcatUtils(nores: false);
		}

		internal void DefineWatcher()
		{
			RefImporterDescriptor.GetConsumer().a_VerifyOnProjectLoad.IncludeConsumer();
			RefImporterDescriptor.GetConsumer().a_VerifyOnDisplay.ConcatUtils(nores: false);
		}

		internal void DestroyWatcher()
		{
			Application.OpenURL("");
		}

		internal void NewWatcher()
		{
			Application.OpenURL(m_Decorator[0].Item2);
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
			NewIdentifier($"Something went wrong while checking for an update!\n\n{exc}", CustomLogType.Error);
		}

		internal void InvokeWatcher()
		{
			m_Advisor = false;
			CalculateIdentifier();
		}

		internal async Task CustomizeWatcher()
		{
			await Task.Delay(3000);
			RefImporterDescriptor.GetConsumer().u_updateHidden.ConcatUtils(nores: true);
			CalculateIdentifier();
		}

		internal static bool AwakeAnnotation()
		{
			return InitAnnotation == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass132_0
	{
		public string itemContext;

		internal static _003C_003Ec__DisplayClass132_0 CallAnnotation;

		internal void EnableWatcher()
		{
			bool reponse = _Reponse;
			_Service = false;
			_Reponse = false;
			_Object = (listener = (m_Printer = string.Empty));
			RefImporterDescriptor.GetConsumer().a_HasSucceededLastVerification.ConcatUtils(nores: false);
			SessionState.EraseBool(itemContext);
			ResetConfiguration(reponse);
		}

		internal string AwakeWatcher(string key, ref _003C_003Ec__DisplayClass132_1 _003C_003Ec__DisplayClass132_1_0, ref _003C_003Ec__DisplayClass132_2 _003C_003Ec__DisplayClass132_2_0)
		{
			return ListIdentifier(SessionState.GetString(ForgotIdentifier(itemContext + key, ref _003C_003Ec__DisplayClass132_2_0), string.Empty), ref _003C_003Ec__DisplayClass132_1_0);
		}

		internal void DisableWatcher()
		{
			List<(string, string)> list = CountConfiguration("verifylicense");
			StartConfiguration(list);
			OrderIdentifier(IncludeConfiguration(list.ToArray())).CreateProcess(delegate(ParamsIdentifier response)
			{
				_003C_003Ec__DisplayClass132_3 _003C_003Ec__DisplayClass132_ = new _003C_003Ec__DisplayClass132_3
				{
					m_SetterContext = this,
					_SystemContext = response
				};
				_Struct = false;
				_Worker = true;
				QueryConfiguration(_003C_003Ec__DisplayClass132_._SystemContext, _003C_003Ec__DisplayClass132_.RateWatcher, delegate
				{
					bool reponse = _Reponse;
					_Service = false;
					_Reponse = false;
					_Object = (listener = (m_Printer = string.Empty));
					RefImporterDescriptor.GetConsumer().a_HasSucceededLastVerification.ConcatUtils(nores: false);
					SessionState.EraseBool(itemContext);
					ResetConfiguration(reponse);
				}, comparesecond2: false);
			}, _003C_003Ec.m_ProcAuthentication.CountServer, null, null, CalculateIdentifier);
		}

		internal void VisitWatcher(ParamsIdentifier response)
		{
			_003C_003Ec__DisplayClass132_3 _003C_003Ec__DisplayClass132_ = new _003C_003Ec__DisplayClass132_3
			{
				m_SetterContext = this,
				_SystemContext = response
			};
			_Struct = false;
			_Worker = true;
			QueryConfiguration(_003C_003Ec__DisplayClass132_._SystemContext, _003C_003Ec__DisplayClass132_.RateWatcher, delegate
			{
				bool reponse = _Reponse;
				_Service = false;
				_Reponse = false;
				_Object = (listener = (m_Printer = string.Empty));
				RefImporterDescriptor.GetConsumer().a_HasSucceededLastVerification.ConcatUtils(nores: false);
				SessionState.EraseBool(itemContext);
				ResetConfiguration(reponse);
			}, comparesecond2: false);
		}

		internal void AssetWatcher(string key, string value, ref _003C_003Ec__DisplayClass132_4 _003C_003Ec__DisplayClass132_4_0, ref _003C_003Ec__DisplayClass132_5 _003C_003Ec__DisplayClass132_5_0)
		{
			SessionState.SetString(UpdateIdentifier(itemContext + key, ref _003C_003Ec__DisplayClass132_5_0), SearchIdentifier(value, ref _003C_003Ec__DisplayClass132_4_0));
		}

		internal static bool QueryAnnotation()
		{
			return CallAnnotation == null;
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
		public ParamsIdentifier _SystemContext;

		public _003C_003Ec__DisplayClass132_0 m_SetterContext;

		private static _003C_003Ec__DisplayClass132_3 ReflectAnnotation;

		internal void RateWatcher()
		{
			try
			{
				string text = _SystemContext.PublishConsumer("date");
				if (RemoveConfiguration() != text)
				{
					NewIdentifier("Date Mismatch! Please make sure your system's date is correct.\nLocal: " + setter + "  |  Remote: " + text, CustomLogType.Error);
					indexer = true;
					m_SetterContext.EnableWatcher();
					return;
				}
				listener = _SystemContext.PublishConsumer("username");
				m_Printer = _SystemContext.PublishConsumer("variant");
				_Object = _SystemContext.PublishConsumer("token");
				InstantiateConfiguration();
				RestartConfiguration();
				string def = _SystemContext.PublishConsumer("message");
				if (!_Reponse)
				{
					NewIdentifier(def);
				}
				_Service = true;
				RefImporterDescriptor.GetConsumer().a_HasSucceededLastVerification.ConcatUtils(nores: true);
				EditorPrefs.SetString("No1lKII9IzcBAbihub6nCg==LK", m_Repository);
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
						m_SetterContext.AssetWatcher("date", setter, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
						m_SetterContext.AssetWatcher("u", listener, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
						m_SetterContext.AssetWatcher("v", m_Printer, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
						m_SetterContext.AssetWatcher("r", _Object, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
						m_SetterContext.AssetWatcher("m", attr, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
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

		internal static bool CompareAnnotation()
		{
			return ReflectAnnotation == null;
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

		public AuthenticationIdentifier[] modelContext;

		public bool _ConfigContext;

		public Action m_MockContext;

		internal static _003C_003Ec__DisplayClass138_0 LogoutAnnotation;

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
				state = false;
				_Mock = false;
				_Struct = false;
				_Rule = false;
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
				_Struct = false;
				_Rule = false;
				CalculateIdentifier();
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
			attr = string.Join("-", array);
			RestartConfiguration();
			m_MockContext();
		}

		internal static bool CreateAnnotation()
		{
			return LogoutAnnotation == null;
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

		private static _003C_003Ec__DisplayClass138_3 ConnectAnnotation;

		internal bool StopReg(string v)
		{
			return v == m_AdvisorContext[0];
		}

		internal static bool RegisterAnnotation()
		{
			return ConnectAnnotation == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass179_0
	{
		public ParamsIdentifier composerContext;

		internal static _003C_003Ec__DisplayClass179_0 RateOrder;

		internal void RestartReg()
		{
			m_Strategy = composerContext.PublishConsumer("transfer_email");
			m_Config = true;
		}

		internal static bool NewOrder()
		{
			return RateOrder == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass46_0
	{
		public bool m_MappingContext;

		public UnityEngine.Object queueContext;

		public UnityEngine.Object[] m_ProcessorContext;

		public ExceptionSingletonStruct.TaskMethod[] m_TokenizerContext;

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

		private static _003C_003Ec__DisplayClass66_0 DefineOrder;

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
					ComposerIdentifier.WriteSingleton();
					IndexerMethodBridge.InsertProperty();
					RuleIdentifier.InvokeProperty();
					AttributeConsumerExporter.ReadPage();
				}
			}
			catch (Exception message)
			{
				UnityEngine.Debug.LogError(message);
			}
		}

		internal static bool TestOrder()
		{
			return DefineOrder == null;
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

	private static bool m_Configuration;

	private static MethodInfo _Identifier;

	private static MethodInfo context;

	private static MethodInfo _Serializer;

	private static MethodInfo method;

	private static MethodInfo utils;

	private static MethodInfo _Page;

	private static MethodInfo property;

	private static MethodInfo _Singleton;

	private static bool _Account;

	private static PhysBoneManager _Params;

	private static GameObject importer;

	private static GameObject[] m_Server;

	private static GameObject[] watcher;

	private static GameObject reg;

	private static VRCPhysBone[] _Process;

	private static bool[] m_Val;

	private static bool[] m_Adapter;

	private static VRCPhysBoneCollider[] proxy;

	private static bool[] m_Comparator;

	private static bool[] _Product;

	private static readonly int _Iterator = "ADOControlID".GetHashCode();

	private static VRCAvatarDescriptor m_Predicate;

	private static VRCAvatarDescriptor[] _Collection;

	private static string[] m_Registry;

	private static string[] m_Client;

	private static string[] _Observer;

	private static int[] m_Broadcaster;

	private static bool @event;

	private static bool record;

	private static bool _Resolver;

	private static bool _Tag;

	private static bool filter;

	private static bool m_Factory;

	private static bool _Attribute;

	private static bool task;

	private static readonly ExceptionSingletonStruct.ExporterServerStub customer = new ExceptionSingletonStruct.ExporterServerStub();

	private static readonly int m_Database = GUIUtility.GetControlID("ADOTooltipDragControlID".GetHashCode(), FocusType.Passive);

	private static Dictionary<UnityEngine.Object, UnityEngine.Object> helper = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

	private static Dictionary<UnityEngine.Object, UnityEngine.Object> candidate = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

	private static Dictionary<UnityEngine.Object, bool> _Test = new Dictionary<UnityEngine.Object, bool>();

	private static bool m_Stub;

	private static bool _Rules;

	private static bool _Definition;

	private static bool initializer;

	private static bool getter;

	private static string thread;

	private static bool m_Algo;

	private static bool role;

	private static string m_Invocation;

	private static string listener;

	private static string _Parser;

	private static string m_Printer;

	private static string m_Repository = "";

	private static string descriptor = "";

	private static string m_Strategy = "";

	private static string global;

	private static bool manager;

	private static bool _Worker;

	private static bool indexer;

	private static bool m_Pool;

	private static float _System;

	private static string setter;

	private static bool _Rule;

	private static bool _Struct;

	private static string _Interpreter;

	private static string attr;

	private static string _Object;

	private static bool _Service;

	private static bool _Reponse;

	private static bool specification;

	private static Action m_Wrapper;

	private static bool _Info;

	private static bool m_Config;

	private static bool _Mock;

	private static bool state;

	private static bool m_Field;

	private static bool m_Advisor;

	private static bool exporter;

	private static bool creator;

	private static readonly AnimBool dispatcher = new AnimBool();

	private static readonly AnimBool m_Connection = new AnimBool();

	private static readonly IssuerSerializerAdapter m_Expression = new IssuerSerializerAdapter("0.11.1");

	private static readonly (string, string)[] m_Decorator = new(string, string)[0];

	private static IdentifierSerializerConnector DefineTokenizer;

	private static void RunConfiguration(UnityEngine.Object res, SerializedProperty[] visitor, Action tag, bool isres2)
	{
		SerializedProperty serializedProperty = visitor[0];
		SerializedProperty serializedProperty2 = visitor[1];
		SerializedProperty serializedProperty3 = visitor[5];
		SerializedProperty serializedProperty4 = (isres2 ? visitor[6] : null);
		SerializedProperty spec = (isres2 ? visitor[7] : null);
		int intValue = serializedProperty.intValue;
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
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
					filter = false;
					_Resolver = false;
				}
				else if (intValue == 2)
				{
					_Resolver = false;
					record = false;
				}
				tag();
			}
			if (isres2 && serializedProperty4 != null)
			{
				using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, serializedProperty4.boolValue, ExceptionSingletonStruct.resolverSerializer, ExceptionSingletonStruct._ObserverSerializer))
				{
					serializedProperty4.boolValue = ExceptionSingletonStruct.ChangeStatus(serializedProperty4.boolValue, (!serializedProperty4.boolValue) ? "Outside Bounds" : "Inside Bounds", GUI.skin.button, GUILayout.ExpandWidth(expand: false));
				}
			}
		}
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		})())
		{
			return;
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(serializedProperty2, new GUIContent("Root"));
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
					Vector3 eulerAngles = serializedProperty3.quaternionValue.eulerAngles;
					eulerAngles = EditorGUILayout.Vector3Field(new GUIContent("Rotation", "Rotation offset from the root transform"), eulerAngles);
					if (changeCheckScope.changed)
					{
						serializedProperty3.quaternionValue = Quaternion.Euler(eulerAngles);
					}
				}
				using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, filter, Color.green, Color.red))
				{
					filter = GUILayout.Toggle(filter, ExceptionSingletonStruct.CustomizeRef().stateSerializer, ExceptionSingletonStruct.MapRef().methodMethod, GUILayout.Width(18f), GUILayout.Height(18f));
				}
			}
		}
		if (isres2)
		{
			ChangeConfiguration(spec);
		}
	}

	private static void OrderConfiguration(UnityEngine.Object task, UnityEngine.Object[] reg, int proc_Ptr, Color selection2)
	{
		_003C_003Ec__DisplayClass46_0 pool = default(_003C_003Ec__DisplayClass46_0);
		pool.queueContext = task;
		pool.m_ProcessorContext = reg;
		if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Escape))
		{
			filter = false;
			_Tag = false;
			_Resolver = false;
			record = false;
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
			pool.m_TokenizerContext = pool.m_ProcessorContext.Select((UnityEngine.Object t2) => new ExceptionSingletonStruct.TaskMethod((VRCPhysBoneCollider)t2)).ToArray();
		}
		else if (proc_Ptr == 1)
		{
			pool.m_TokenizerContext = pool.m_ProcessorContext.Select((UnityEngine.Object t2) => new ExceptionSingletonStruct.TaskMethod((VRCContactSender)t2)).ToArray();
		}
		else
		{
			pool.m_TokenizerContext = pool.m_ProcessorContext.Select((UnityEngine.Object t2) => new ExceptionSingletonStruct.TaskMethod((VRCContactReceiver)t2)).ToArray();
		}
		Transform helperMethod = pool.m_TokenizerContext[pool._ExceptionContext].m_HelperMethod;
		pool._ProducerContext = UpdateConfiguration(helperMethod);
		int candidateMethod = pool.m_TokenizerContext[pool._ExceptionContext]._CandidateMethod;
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		})())
		{
			return;
		}
		Quaternion quaternion = helperMethod.rotation * pool.m_TokenizerContext[pool._ExceptionContext].m_TestsMethod;
		pool.m_ErrorContext = helperMethod.TransformPoint(pool.m_TokenizerContext[pool._ExceptionContext]._RulesMethod);
		Vector3 vector = quaternion * Vector3.up;
		float num = pool.m_TokenizerContext[pool._ExceptionContext].m_StubMethod * 0.5f - pool.m_TokenizerContext[pool._ExceptionContext].m_ReaderMethod;
		float num2 = pool.m_TokenizerContext[pool._ExceptionContext].m_ReaderMethod * pool._ProducerContext;
		Vector3 vector2 = num2 * vector;
		Vector3 vector3 = pool.m_ErrorContext + Mathf.Max(num * pool._ProducerContext, 0f) * (helperMethod.rotation * pool.m_TokenizerContext[pool._ExceptionContext].m_TestsMethod * Vector3.up);
		Vector3 vector4 = pool.m_ErrorContext - Mathf.Max(num * pool._ProducerContext, 0f) * (helperMethod.rotation * pool.m_TokenizerContext[pool._ExceptionContext].m_TestsMethod * Vector3.up);
		pool.m_ValueContext = (Event.current.shift ? 2 : (Event.current.alt ? 1 : 0));
		pool.m_MappingContext = pool.m_ValueContext == 1;
		if (_Tag)
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
					vector6 = helperMethod.InverseTransformVector(vector6);
				}
				switch (pool.m_ValueContext)
				{
				case 2:
				{
					pool.m_TokenizerContext[pool._ExceptionContext]._RulesMethod += vector6;
					for (int num4 = 0; num4 < pool.m_TokenizerContext.Length; num4++)
					{
						pool.m_TokenizerContext[num4]._RulesMethod = pool.m_TokenizerContext[pool._ExceptionContext]._RulesMethod;
					}
					break;
				}
				case 1:
					pool.m_TokenizerContext[pool._ExceptionContext]._RulesMethod += vector6;
					break;
				case 0:
				{
					for (int num3 = 0; num3 < pool.m_TokenizerContext.Length; num3++)
					{
						if (!flag)
						{
							pool.m_TokenizerContext[num3]._RulesMethod += pool.m_TokenizerContext[num3].m_HelperMethod.InverseTransformVector(vector6);
						}
						else if (!(pool.m_TokenizerContext[num3].customerMethod == pool.m_TokenizerContext[pool._ExceptionContext].customerMethod))
						{
							pool.m_TokenizerContext[num3]._RulesMethod += pool.m_TokenizerContext[num3].m_TestsMethod * Quaternion.Inverse(pool.m_TokenizerContext[pool._ExceptionContext].m_TestsMethod) * vector6;
						}
						else
						{
							pool.m_TokenizerContext[pool._ExceptionContext]._RulesMethod += vector6;
						}
					}
					break;
				}
				}
			}
		}
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		})())
		{
			return;
		}
		if (filter && candidateMethod != 0)
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
				Quaternion testsMethod = Quaternion.Euler((Quaternion.Inverse(helperMethod.rotation) * quaternion2).eulerAngles);
				switch (pool.m_ValueContext)
				{
				case 0:
				case 2:
				{
					for (int num5 = 0; num5 < pool.m_TokenizerContext.Length; num5++)
					{
						pool.m_TokenizerContext[num5].m_TestsMethod = testsMethod;
					}
					break;
				}
				case 1:
					pool.m_TokenizerContext[pool._ExceptionContext].m_TestsMethod = testsMethod;
					break;
				}
			}
		}
		if (record && candidateMethod != 2)
		{
			bool flag2 = candidateMethod == 1;
			_003C_003Ec__DisplayClass46_1 third = default(_003C_003Ec__DisplayClass46_1);
			using (EditorGUI.ChangeCheckScope changeCheckScope3 = new EditorGUI.ChangeCheckScope())
			{
				Vector3 position = (flag2 ? vector3 : pool.m_ErrorContext);
				Quaternion rotation = ((!flag2) ? Quaternion.identity : quaternion);
				third._TemplateContext = Handles.RadiusHandle(rotation, position, num2, handlesOnly: true) / pool._ProducerContext;
				CollectIdentifier(changeCheckScope3.changed, ref pool, ref third);
			}
			if (flag2)
			{
				using EditorGUI.ChangeCheckScope changeCheckScope4 = new EditorGUI.ChangeCheckScope();
				third._TemplateContext = Handles.RadiusHandle(quaternion, vector4, num2, handlesOnly: true) / pool._ProducerContext;
				CollectIdentifier(changeCheckScope4.changed, ref pool, ref third);
			}
		}
		if (_Resolver && candidateMethod == 1)
		{
			if (!((Func<bool>)delegate
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
				return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
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
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
				return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
			})())
			{
				return;
			}
		}
		ExceptionSingletonStruct.TaskMethod[] array = pool.m_TokenizerContext;
		foreach (ExceptionSingletonStruct.TaskMethod taskMethod in array)
		{
			taskMethod.SortProduct();
		}
	}

	private static void CalculateConfiguration(SceneView res)
	{
		if (!_Tag && !filter && !record && !_Resolver)
		{
			return;
		}
		Tools.hidden = true;
		if (((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		})())
		{
			int num = 1;
			if (m_Factory)
			{
				num++;
			}
			if (_Attribute)
			{
				num++;
			}
			if (task)
			{
				num++;
			}
			WriteIdentifier(res, "Editing", CalcConfiguration, 200f, 45 + 20 * num);
		}
	}

	private static void CalcConfiguration()
	{
		if (m_Factory)
		{
			PatchConfiguration("Radius", ref record);
		}
		if (_Attribute)
		{
			PatchConfiguration("Height", ref _Resolver);
		}
		PatchConfiguration("Position", ref _Tag);
		if (task)
		{
			PatchConfiguration("Rotation", ref filter);
		}
	}

	private static void DeleteConfiguration(SceneView init)
	{
		if (!_Account)
		{
			return;
		}
		Tools.hidden |= RefImporterDescriptor.GetConsumer().hideToolsDuringTesting;
		EditorApplication.playModeStateChanged -= FillConfiguration;
		EditorApplication.playModeStateChanged += FillConfiguration;
		if (importer != null)
		{
			ExceptionSingletonStruct.ConnectStatus(importer.transform, counterinstall: true, skipthird: true, readparam2: false, usecaller3: false, ismap4: false, bool_0: true);
		}
		MoveIdentifier(init, delegate
		{
			using (new GUILayout.HorizontalScope())
			{
				bool ignorecaller;
				string tooltip = ((!(ignorecaller = RefImporterDescriptor.GetConsumer().hideToolsDuringTesting)) ? "Native tools are visible during test." : "Native tools are hidden during test.");
				using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.FG, ignorecaller, ExceptionSingletonStruct._ObserverSerializer, ExceptionSingletonStruct._BroadcasterSerializer))
				{
					if (ExceptionSingletonStruct.ListStatus(new GUIContent(ExceptionSingletonStruct.CustomizeRef().prototypeSerializer)
					{
						tooltip = tooltip
					}))
					{
						RefImporterDescriptor.GetConsumer().hideToolsDuringTesting.IncludeConsumer();
						Tools.hidden = false;
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.Label("Testing", ExceptionSingletonStruct.MapRef().m_WriterSerializer);
				Rect lastRect = GUILayoutUtility.GetLastRect();
				GUILayout.FlexibleSpace();
				PublishIdentifier();
				return lastRect;
			}
		}, delegate
		{
			using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, ExceptionSingletonStruct._BroadcasterSerializer))
			{
				if (ExceptionSingletonStruct.PatchStatus("Stop Testing") || ExceptionSingletonStruct.QueryStatus() || ExceptionSingletonStruct.ComputeStatus())
				{
					NewConfiguration();
				}
			}
			using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, ExceptionSingletonStruct.m_RecordSerializer))
			{
				if (ExceptionSingletonStruct.PatchStatus("Restart"))
				{
					CompareConfiguration();
				}
			}
			using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, m_Stub, ExceptionSingletonStruct._ObserverSerializer))
			{
				using (new EditorGUI.DisabledScope(!m_Stub))
				{
					if (ExceptionSingletonStruct.PatchStatus("Apply All Changes"))
					{
						foreach (UnityEngine.Object item in _Test.Keys.ToList())
						{
							if (_Test[item])
							{
								UnityEngine.Object obj = candidate[item];
								if (obj != null)
								{
									Undo.RecordObject(obj, "ADO - Apply Changes");
									EditorUtility.CopySerialized(item, obj);
									_Test[item] = false;
								}
							}
						}
						m_Stub = false;
						InsertConfiguration();
					}
				}
			}
		}, 200f, 104f);
	}

	private static void DefineConfiguration()
	{
		if (!_Params)
		{
			NewConfiguration();
			return;
		}
		_Identifier.Invoke(_Params, null);
		for (int i = 0; i < _Process.Length; i++)
		{
			if ((bool)_Process[i])
			{
				bool flag = _Process[i].enabled && _Process[i].gameObject.activeInHierarchy;
				if (m_Val[i] == flag)
				{
					continue;
				}
				m_Val[i] = flag;
				if (!flag)
				{
					utils.Invoke(_Process[i], null);
					continue;
				}
				method.Invoke(_Process[i], null);
				if (!m_Adapter[i])
				{
					m_Adapter[i] = true;
					_Serializer.Invoke(_Process[i], null);
				}
			}
			else if (m_Val[i])
			{
				m_Val[i] = false;
				utils.Invoke(_Process[i], null);
			}
		}
		for (int j = 0; j < proxy.Length; j++)
		{
			if (!proxy[j])
			{
				if (m_Comparator[j])
				{
					m_Comparator[j] = false;
					_Singleton.Invoke(proxy[j], null);
				}
				continue;
			}
			bool flag2 = proxy[j].enabled && proxy[j].gameObject.activeInHierarchy;
			if (m_Comparator[j] == flag2)
			{
				continue;
			}
			m_Comparator[j] = flag2;
			if (flag2)
			{
				property.Invoke(proxy[j], null);
				if (!_Product[j])
				{
					_Product[j] = true;
					_Page.Invoke(proxy[j], null);
				}
			}
			else
			{
				_Singleton.Invoke(proxy[j], null);
			}
		}
	}

	private static void DestroyConfiguration()
	{
		if (!m_Configuration)
		{
			m_Configuration = true;
			_Identifier = ((IdentifierSerializerConnector)(object)typeof(PhysBoneManager)).ListAuthentication("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
			context = ((IdentifierSerializerConnector)(object)typeof(PhysBoneManager)).ListAuthentication("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic);
			_Serializer = ((IdentifierSerializerConnector)(object)typeof(VRCPhysBoneBase)).ListAuthentication("Start", BindingFlags.Instance | BindingFlags.NonPublic);
			method = ((IdentifierSerializerConnector)(object)typeof(VRCPhysBoneBase)).ListAuthentication("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
			utils = ((IdentifierSerializerConnector)(object)typeof(VRCPhysBoneBase)).ListAuthentication("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
			_Page = ((IdentifierSerializerConnector)(object)typeof(VRCPhysBoneColliderBase)).ListAuthentication("Start", BindingFlags.Instance | BindingFlags.NonPublic);
			property = ((IdentifierSerializerConnector)(object)typeof(VRCPhysBoneColliderBase)).ListAuthentication("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
			_Singleton = ((IdentifierSerializerConnector)(object)typeof(VRCPhysBoneColliderBase)).ListAuthentication("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
		}
	}

	private static void NewConfiguration()
	{
		DestroyConfiguration();
		_Account = !_Account;
		if (Application.isPlaying)
		{
			_Account = false;
		}
		if (_Account)
		{
			VerifyConfiguration();
		}
		else
		{
			SetConfiguration();
		}
	}

	private static void CompareConfiguration()
	{
		if (_Account)
		{
			while (true)
			{
				NewConfiguration();
			}
		}
		NewConfiguration();
	}

	private static void VerifyConfiguration()
	{
		_Definition |= RefImporterDescriptor.GetConsumer().hasReadColliderTestingWarning;
		watcher = Selection.gameObjects;
		reg = Selection.activeGameObject;
		helper = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
		candidate = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
		_Test = new Dictionary<UnityEngine.Object, bool>();
		m_Stub = false;
		_003C_003Ec__DisplayClass54_0 pol = default(_003C_003Ec__DisplayClass54_0);
		pol.bridgeContext = new List<Transform>();
		m_Server = Selection.transforms.Select((Transform t) => t.root.gameObject).Distinct().ToArray();
		VRCPhysBone[] componentsToFind = m_Server.SelectMany((GameObject o) => o.GetComponentsInChildren<VRCPhysBone>(includeInactive: true)).ToArray();
		VRCPhysBoneColliderBase[] componentsToFind2 = m_Server.SelectMany((GameObject o) => o.GetComponentsInChildren<VRCPhysBoneColliderBase>(includeInactive: true)).ToArray();
		if (m_Server.Length == 0)
		{
			NewIdentifier("No Active Objects with PhysBones found in the scene.", CustomLogType.Error);
			return;
		}
		importer = GameObject.Find("Physbone Tester");
		if ((bool)importer)
		{
			UnityEngine.Object.DestroyImmediate(importer);
		}
		importer = new GameObject("Physbone Tester")
		{
			hideFlags = (HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild)
		};
		importer.transform.position = reg.transform.position;
		GameObject[] server = m_Server;
		foreach (GameObject gameObject in server)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation, importer.transform);
			Dictionary<VRCPhysBone, VRCPhysBone> dictionary = ExceptionSingletonStruct.SetupStatus(gameObject.transform, gameObject2.transform, skipfilter: true, componentsToFind);
			Dictionary<VRCPhysBoneColliderBase, VRCPhysBoneColliderBase> dictionary2 = ExceptionSingletonStruct.SetupStatus(gameObject.transform, gameObject2.transform, skipfilter: true, componentsToFind2);
			VRCPhysBone component = reg.GetComponent<VRCPhysBone>();
			if (component != null && dictionary.TryGetValue(component, out var value) && value != null)
			{
				Selection.activeGameObject = value.gameObject;
			}
			else
			{
				VRCPhysBoneColliderBase component2 = reg.GetComponent<VRCPhysBoneColliderBase>();
				if (component2 != null && dictionary2.TryGetValue(component2, out var value2) && value2 != null)
				{
					Selection.activeGameObject = value2.gameObject;
				}
			}
			ViewIdentifier(dictionary, ref pol);
			ViewIdentifier(dictionary2, ref pol);
			gameObject.SetActive(value: false);
		}
		_Params = importer.AddComponent<PhysBoneManager>();
		PhysBoneManager.Inst = _Params;
		_Params.IsSDK = true;
		_Params.Init();
		_Process = importer.GetComponentsInChildren<VRCPhysBone>(includeInactive: true);
		m_Val = new bool[_Process.Length];
		m_Adapter = new bool[_Process.Length];
		proxy = importer.GetComponentsInChildren<VRCPhysBoneCollider>(includeInactive: true);
		m_Comparator = new bool[proxy.Length];
		_Product = new bool[proxy.Length];
		UnityEngine.Object[] objects = pol.bridgeContext.Select((Transform t) => t.gameObject).ToArray();
		Selection.objects = objects;
		EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(DefineConfiguration));
		EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(DefineConfiguration));
		SceneView.duringSceneGui -= DeleteConfiguration;
		SceneView.duringSceneGui += DeleteConfiguration;
	}

	private static void SetConfiguration()
	{
		EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(DefineConfiguration));
		SceneView.duringSceneGui -= DeleteConfiguration;
		UnityEngine.Object[] objects = watcher;
		Selection.objects = objects;
		Selection.activeObject = reg;
		context.Invoke(_Params, null);
		if ((bool)importer)
		{
			UnityEngine.Object.DestroyImmediate(importer);
		}
		foreach (GameObject item in m_Server.Where((GameObject o) => o))
		{
			item.SetActive(value: true);
		}
		helper = (candidate = null);
		_Test = null;
		_Rules = false;
		m_Stub = false;
	}

	private static void SortConfiguration(bool isi, bool iscont, bool explicittemplate)
	{
		m_Factory = isi;
		_Attribute = iscont;
		task = explicittemplate;
		if (!m_Factory)
		{
			record = false;
		}
		if (!_Attribute)
		{
			_Resolver = false;
		}
		if (!task)
		{
			filter = false;
		}
	}

	private static void InvokeConfiguration(SerializedProperty[] var1, int indexOf_col)
	{
		int intValue = var1[0].intValue;
		switch (indexOf_col)
		{
		case 2:
			_Tag = SearchConfiguration(var1[4], _Tag);
			break;
		case 0:
			if (intValue != 2)
			{
				record = SearchConfiguration(var1[2], record);
			}
			break;
		case 1:
			if (intValue == 1)
			{
				_Resolver = SearchConfiguration(var1[3], _Resolver);
			}
			break;
		}
	}

	private static void CustomizeConfiguration(SerializedProperty reference, Rect col, int remove_FIELDAt)
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
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		})())
		{
			return;
		}
		using (new EditorGUI.DisabledScope(!(UnityEngine.Object)(object)m_Predicate))
		{
			int num = EditorGUI.Popup(position, -1, m_Client);
			if (num != -1)
			{
				arrayElementAtIndex.stringValue = Regex.Replace(m_Client[num], "^Default/", string.Empty);
			}
		}
		EditorGUI.PropertyField(rect, arrayElementAtIndex, GUIContent.none);
		if (GUI.Button(position2, ExceptionSingletonStruct.CustomizeRef()._CreatorSerializer, ExceptionSingletonStruct.MapRef().utilsMethod))
		{
			reference.DeleteArrayElementAtIndex(remove_FIELDAt);
		}
	}

	private static void ConcatConfiguration(Rect v)
	{
		GUIStyle style = new GUIStyle("boldlabel");
		GUI.Label(v, "Collision Tags", style);
	}

	private static void MapConfiguration(Action config)
	{
		CancelConfiguration(isvar1: true);
		config();
		PrintConfiguration(ref m_Predicate, ref _Collection);
		LogoutConfiguration();
	}

	private static void FillConfiguration(PlayModeStateChange spec)
	{
		if (spec == PlayModeStateChange.ExitingEditMode && _Account)
		{
			NewConfiguration();
		}
	}

	private static void CancelConfiguration(bool isvar1)
	{
		SceneView.duringSceneGui -= CalculateConfiguration;
		if (isvar1)
		{
			SceneView.duringSceneGui += CalculateConfiguration;
		}
		else
		{
			Tools.hidden = false;
		}
	}

	private static void LogoutConfiguration()
	{
		ExceptionSingletonStruct.ConcatVal(m_Predicate, ref _Observer, ref m_Broadcaster);
		if (!(UnityEngine.Object)(object)m_Predicate)
		{
			m_Registry = Array.Empty<string>();
		}
		SetupConfiguration();
		m_Client = ((UnityEngine.Component)(object)m_Predicate).GetComponentsInChildren<VRCContactSender>().SelectMany((VRCContactSender cs) => cs.collisionTags).Concat(((UnityEngine.Component)(object)m_Predicate).GetComponentsInChildren<VRCContactReceiver>().SelectMany((VRCContactReceiver cr) => cr.collisionTags))
			.Except(ExceptionSingletonStruct.m_RoleSerializer)
			.Concat(ExceptionSingletonStruct.m_RoleSerializer.Select((string s) => "Default/" + s))
			.Distinct()
			.ToArray();
	}

	private static void SetupConfiguration()
	{
		m_Registry = (from p in (from rc in m_Predicate.baseAnimationLayers.Concat(m_Predicate.specialAnimationLayers)
				where !rc.isDefault && (bool)rc.animatorController
				select AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(AssetDatabase.GetAssetPath(rc.animatorController)) into c
				where c
				select c).SelectMany((UnityEditor.Animations.AnimatorController c) => c.parameters)
			select p.name into p
			where !ExceptionSingletonStruct._AlgoSerializer.Contains(p)
			select p).Distinct().ToArray();
	}

	private static void SelectConfiguration(AnimBool[] i, UnityAction col)
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
		NewIdentifier((!_Struct) ? "Please activate your license." : "Please wait for verification.", CustomLogType.Error, !_Service);
		return _Service;
	}

	internal static void PublishConfiguration(string[] info, int[] attr, string[] role, out string[] cfg2, out int[] pred3)
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

	internal static int[] CollectConfiguration(int idx_v, int contstart)
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

	internal static void PrintConfiguration(ref VRCAvatarDescriptor spec, ref VRCAvatarDescriptor[] cust, Action dir = null, Func<VRCAvatarDescriptor, bool> caller2 = null)
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

	internal static bool InterruptConfiguration(ref VRCAvatarDescriptor var1, VRCAvatarDescriptor[] cust, Action helper = null, bool isresult2 = true, bool allowvalue3 = true, bool useasset4 = true, string res5 = "Avatar", string col6 = "The Targeted VRCAvatar", Action instance7 = null)
	{
		if (!(UnityEngine.Object)(object)ViewConfiguration(ref var1, cust, helper, res5, col6, instance7))
		{
			return false;
		}
		return PostConfiguration(var1, isresult2, allowvalue3, useasset4);
	}

	private static VRCAvatarDescriptor ViewConfiguration(ref VRCAvatarDescriptor res, VRCAvatarDescriptor[] cfg, Action util = null, string token2 = "Avatar", string value3 = "The Targeted VRCAvatar", Action t4 = null)
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

	private static bool PostConfiguration(VRCAvatarDescriptor setup, bool iscont = true, bool resreguired = true, bool loadpol2 = true)
	{
		if (!resreguired || !ListConfiguration(setup))
		{
			if (loadpol2)
			{
				return !ForgotConfiguration(setup, iscont);
			}
			return true;
		}
		return false;
	}

	private static bool ListConfiguration(VRCAvatarDescriptor ident)
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

	private static bool ForgotConfiguration(VRCAvatarDescriptor spec, bool insertreg = true)
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

	private static float UpdateConfiguration(Transform i)
	{
		return Mathf.Max(i.lossyScale.x, i.lossyScale.y, i.lossyScale.z);
	}

	private static bool SearchConfiguration(SerializedProperty def, bool testtoken)
	{
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(def);
			return LoginConfiguration(testtoken, ExceptionSingletonStruct.CustomizeRef().stateSerializer);
		}
	}

	private static bool LoginConfiguration(bool movefirst, GUIContent vis)
	{
		using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, movefirst, ExceptionSingletonStruct._ObserverSerializer, ExceptionSingletonStruct._BroadcasterSerializer))
		{
			movefirst = ExceptionSingletonStruct.PrepareStatus(movefirst, vis, ExceptionSingletonStruct.MapRef().methodMethod, GUILayout.Width(18f), GUILayout.Height(18f));
			return movefirst;
		}
	}

	private static void PatchConfiguration(string v, ref bool cfg, params GUILayoutOption[] options)
	{
		CheckConfiguration(new GUIContent(v), ref cfg, options);
	}

	private static void CheckConfiguration(GUIContent item, ref bool pol, params GUILayoutOption[] options)
	{
		using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, pol, ExceptionSingletonStruct._ObserverSerializer, ExceptionSingletonStruct._BroadcasterSerializer))
		{
			pol = ExceptionSingletonStruct.PrepareStatus(pol, item, GUI.skin.button, options);
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
		using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, positionmap, ExceptionSingletonStruct.MapRef().m_SerializerMethod))
		{
			boolValue = ExceptionSingletonStruct.PrepareStatus(def.boolValue, visitor, GUI.skin.button, options);
		}
		if (changeCheckScope.changed)
		{
			def.boolValue = boolValue;
			control?.Invoke();
		}
	}

	private static void ChangeConfiguration(SerializedProperty spec)
	{
		if (spec != null)
		{
			EditorGUILayout.PropertyField(spec);
		}
	}

	private static void StopConfiguration(SerializedProperty value, SerializedProperty token)
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
			using (new InfoAccountCollection(value.hasMultipleDifferentValues || (value.enumValueIndex == 2 && serializedProperty.hasMultipleDifferentValues)))
			{
				flag = EditorGUILayout.Toggle("Self", flag);
			}
			using (new InfoAccountCollection(value.hasMultipleDifferentValues || (value.enumValueIndex == 2 && serializedProperty2.hasMultipleDifferentValues)))
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

	private static void PushConfiguration()
	{
		InterruptConfiguration(ref m_Predicate, _Collection, LogoutConfiguration, isresult2: false, allowvalue3: false, useasset4: true, "Target Avatar");
	}

	private static void PrepareConfiguration(SerializedProperty task)
	{
		_003C_003Ec__DisplayClass86_0 visitor = default(_003C_003Ec__DisplayClass86_0);
		visitor.serializerSerializer = task;
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(visitor.serializerSerializer);
			int selectedIndex = -1;
			using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
			{
				selectedIndex = EditorGUILayout.Popup(selectedIndex, m_Registry, "textfielddropdown", GUILayout.Width(18f));
				if (changeCheckScope.changed)
				{
					visitor.serializerSerializer.stringValue = m_Registry[selectedIndex];
				}
			}
			if (visitor.serializerSerializer.hasMultipleDifferentValues || string.IsNullOrEmpty(visitor.serializerSerializer.stringValue))
			{
				return;
			}
			Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Width(50f));
			PublishConfiguration(_Observer, m_Broadcaster, new string[3] { "Bool", "Int", "Float" }, out var cfg, out var pred);
			EditorGUI.BeginChangeCheck();
			int idx_v = EditorGUI.IntPopup(controlRect, -1, cfg, pred);
			if (EditorGUI.EndChangeCheck())
			{
				int[] array = CollectConfiguration(idx_v, 2);
				_003C_003Ec__DisplayClass86_1 field = default(_003C_003Ec__DisplayClass86_1);
				if (m_Predicate.MapVal((VRCAvatarDescriptor.AnimLayerType)array[0], out field._MethodSerializer))
				{
					switch (array[1])
					{
					case 2:
						PostIdentifier(field._MethodSerializer.FindProcess(visitor.serializerSerializer.stringValue, UnityEngine.AnimatorControllerParameterType.Float, 0f), ref visitor, ref field);
						break;
					case 0:
						PostIdentifier(field._MethodSerializer.FindProcess(visitor.serializerSerializer.stringValue, UnityEngine.AnimatorControllerParameterType.Bool, 0f), ref visitor, ref field);
						break;
					case 1:
						PostIdentifier(field._MethodSerializer.FindProcess(visitor.serializerSerializer.stringValue, UnityEngine.AnimatorControllerParameterType.Int, 0f), ref visitor, ref field);
						break;
					}
				}
				else
				{
					NewIdentifier("Couldn't fetch selected playable layer!", CustomLogType.Error);
				}
			}
			controlRect.x += 3f;
			GUI.Label(controlRect, "Add");
		}
	}

	private static bool ReadConfiguration(IEnumerable<UnityEngine.Object> param)
	{
		using (new GUILayout.HorizontalScope())
		{
			using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, _Account, ExceptionSingletonStruct._BroadcasterSerializer))
			{
				bool isPlaying;
				string asset = ((isPlaying = Application.isPlaying) ? "Editor is in PlayMode" : ((!_Account) ? "Test PhysBones in Scene" : "Stop Testing - ESC / Enter"));
				using (new EditorGUI.DisabledScope(isPlaying))
				{
					if (ExceptionSingletonStruct.PatchStatus(asset))
					{
						NewConfiguration();
					}
				}
			}
			if (!_Account)
			{
				return false;
			}
			using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, ExceptionSingletonStruct.m_RecordSerializer))
			{
				if (ExceptionSingletonStruct.PatchStatus("Restart", GUILayout.ExpandWidth(expand: false)))
				{
					CompareConfiguration();
				}
			}
			UnityEngine.Object[] array = param.Where((UnityEngine.Object b) => b != null && _Test.ContainsKey(b) && candidate[b] != null).ToArray();
			bool flag = array.Any((UnityEngine.Object b) => _Test[b]);
			using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, flag, ExceptionSingletonStruct._ObserverSerializer))
			{
				using (new EditorGUI.DisabledScope(!flag))
				{
					if (ExceptionSingletonStruct.PatchStatus("Apply Changes", GUILayout.ExpandWidth(expand: false)))
					{
						UnityEngine.Object[] array2 = array;
						foreach (UnityEngine.Object obj in array2)
						{
							UnityEngine.Object obj2 = candidate[obj];
							using (new AuthenticationSerializerConnector(obj2, false, "rootTransform", "ignoreTransforms", "colliders"))
							{
								Undo.RecordObject(obj2, "ADO - Apply Changes");
								EditorUtility.CopySerialized(obj, obj2);
								_Test[obj] = false;
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
					if (_Account && _Test.ContainsKey(item))
					{
						_Test[item] = true;
						m_Stub = true;
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
		if (_Account && _Rules && !_Definition)
		{
			_Definition = true;
			switch (EditorUtility.DisplayDialogComplex("Testing Restart Required", "Collider changes require a restart of the testing process. Do you want to restart testing?", "Yes", "No", "Don't ask again"))
			{
			case 0:
				CompareConfiguration();
				break;
			case 2:
				RefImporterDescriptor.GetConsumer().hasReadColliderTestingWarning.ConcatUtils(nores: true);
				break;
			}
		}
	}

	private static void EnableConfiguration(Action ident)
	{
		if (_Struct || _Mock)
		{
			return;
		}
		EditorGUILayout.HelpBox("This is 'Avatar Dynamics Overhaul'. If you don't know what this is, you may have imported it from a package that shouldn't contain it. You can delete the editor script to revert back to original behaviour. Usually found in Packages > DreadScripts - Avatar Dynamics Overhaul. If this is the case, please notify the package creator about this.", MessageType.Warning);
		using (new GUILayout.HorizontalScope())
		{
			if (ExceptionSingletonStruct.LoginStatus("Locate", EditorStyles.toolbarButton))
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
			if (ExceptionSingletonStruct.LoginStatus("Info", EditorStyles.toolbarButton))
			{
				Application.OpenURL("https://linktr.ee/Dreadrith");
			}
			if (ExceptionSingletonStruct.LoginStatus("Switch Editor", EditorStyles.toolbarButton))
			{
				ident();
			}
		}
	}

	[SpecialName]
	private static void RemoveSerializer(bool isi)
	{
		bool flag = initializer;
		initializer = isi;
		if (!initializer && flag)
		{
			Policy.WriteMethod(null);
		}
	}

	private static void AwakeConfiguration()
	{
		InitConfiguration("Send Feedback for ADOverhaul", "If you have a suggestion, preference, or something to comment, please send it here!\nNote that the feedback is not anonymous. Abuse may result in blacklisting.");
		m_Algo = _Service;
		m_Invocation = EditorGUILayout.TextArea(m_Invocation, GUILayout.MinHeight(54f));
		using (new GUILayout.HorizontalScope())
		{
			if (ExceptionSingletonStruct.LoginStatus("Cancel", EditorStyles.toolbarButton, GUILayout.ExpandWidth(expand: false)))
			{
				m_Algo = false;
			}
			using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(m_Invocation) || role))
			{
				if (ExceptionSingletonStruct.LoginStatus("Send Feedback", EditorStyles.toolbarButton))
				{
					if (m_Invocation.Length > 2000)
					{
						m_Invocation = m_Invocation.Substring(0, 2000);
					}
					List<(string, string)> list = CountConfiguration("sendfeedback", new(string, string)[1] { ("feedback", Uri.EscapeUriString(m_Invocation)) });
					StartConfiguration(list);
					role = true;
					OrderIdentifier(IncludeConfiguration(list.ToArray())).CreateProcess(ComputeConfiguration, UnityEngine.Debug.LogException, null, null, delegate
					{
						role = false;
						m_Algo = false;
						CalculateIdentifier();
					});
				}
			}
		}
	}

	[SpecialName]
	private static float ResolveSerializer()
	{
		return _System - Time.realtimeSinceStartup;
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
		if (!RefImporterDescriptor.GetConsumer().a_HasSucceededLastVerification)
		{
			_Worker = true;
			m_Pool = flag;
		}
		if (flag && (bool)RefImporterDescriptor.GetConsumer().a_VerifyOnProjectLoad)
		{
			ExceptionSingletonStruct.AddProcess(delegate
			{
				AssetConfiguration(testkey: false);
			});
		}
	}

	private static void VisitConfiguration()
	{
		if (!m_Pool && (bool)RefImporterDescriptor.GetConsumer().a_VerifyOnDisplay && RateConfiguration())
		{
			AssetConfiguration(testkey: false);
		}
	}

	private static void AssetConfiguration(bool testkey)
	{
		_003C_003Ec__DisplayClass132_0 CS_0024_003C_003E8__locals10 = new _003C_003Ec__DisplayClass132_0();
		if ((!RefImporterDescriptor.GetConsumer().a_VerifyOnDisplay.CustomizeUtils() && !RefImporterDescriptor.GetConsumer().a_VerifyOnProjectLoad.CustomizeUtils() && !testkey) || (_Worker && !indexer) || _Struct)
		{
			return;
		}
		indexer = false;
		_Struct = true;
		m_Pool = true;
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
							listener = CS_0024_003C_003E8__locals10.AwakeWatcher("u", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_);
							m_Printer = CS_0024_003C_003E8__locals10.AwakeWatcher("v", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_);
							_Object = CS_0024_003C_003E8__locals10.AwakeWatcher("r", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_);
							attr = CS_0024_003C_003E8__locals10.AwakeWatcher("m", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_);
							InstantiateConfiguration();
							RestartConfiguration();
							_Service = true;
							_Worker = true;
							_Struct = false;
							_Reponse = true;
							ResolveConfiguration(istask: true);
							CalcIdentifier();
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
			NewIdentifier("failed to verify from cache.", CustomLogType.Warning);
		}
		CloneConfiguration(delegate
		{
			List<(string, string)> list = CountConfiguration("verifylicense");
			StartConfiguration(list);
			OrderIdentifier(IncludeConfiguration(list.ToArray())).CreateProcess(delegate(ParamsIdentifier response)
			{
				_003C_003Ec__DisplayClass132_3 _003C_003Ec__DisplayClass132_ = new _003C_003Ec__DisplayClass132_3();
				_003C_003Ec__DisplayClass132_.m_SetterContext = CS_0024_003C_003E8__locals10;
				_003C_003Ec__DisplayClass132_._SystemContext = response;
				_Struct = false;
				_Worker = true;
				QueryConfiguration(_003C_003Ec__DisplayClass132_._SystemContext, _003C_003Ec__DisplayClass132_.RateWatcher, delegate
				{
					bool reponse = _Reponse;
					_Service = false;
					_Reponse = false;
					_Object = (listener = (m_Printer = string.Empty));
					RefImporterDescriptor.GetConsumer().a_HasSucceededLastVerification.ConcatUtils(nores: false);
					SessionState.EraseBool(CS_0024_003C_003E8__locals10.itemContext);
					ResetConfiguration(reponse);
				}, comparesecond2: false);
			}, _003C_003Ec.m_ProcAuthentication.CountServer, null, null, CalculateIdentifier);
		}, ispred: true);
	}

	private static void PopConfiguration()
	{
		_Rule = true;
		while (FindConfiguration())
		{
		}
		NewIdentifier("Invalid License Key!", CustomLogType.Error);
	}

	private static void InstantiateConfiguration()
	{
		_Parser = listener;
		if (string.IsNullOrWhiteSpace(_Parser))
		{
			return;
		}
		try
		{
			Match match = Regex.Match(_Parser, "(?i)(?:<color=#(?:[0-9a-f]{8}|[0-9a-f]{6})>)?.*?(#\\d{4})(?:<\\/color>)?$");
			if (match.Success)
			{
				_Parser = _Parser.Remove(match.Groups[1].Index, match.Groups[1].Length);
			}
			if (_Parser.Length > 1 && _Parser[0] == '@')
			{
				_Parser = _Parser.Substring(1);
			}
		}
		catch
		{
		}
	}

	private static void RestartConfiguration()
	{
		string[] array = attr.Split(new char[1] { '-' });
		string[] array2 = RemoveConfiguration().Split(new char[1] { '/' });
		array2[2] = array2[2].Substring(2, 2);
		_Interpreter = array2[2] + array[0].Substring(0, 10) + array2[1] + array[2].Substring(0, 10) + array2[0];
	}

	private static void ManageConfiguration()
	{
		if (string.IsNullOrWhiteSpace(global))
		{
			string key = "DreadScriptssid";
			global = EditorPrefs.GetString(key, string.Empty);
			if (string.IsNullOrWhiteSpace(global) || !Regex.IsMatch(global, "[0-9a-f]{32}"))
			{
				global = GUID.Generate().ToString();
				EditorPrefs.SetString(key, global);
			}
		}
	}

	private static bool RateConfiguration()
	{
		if (!string.IsNullOrWhiteSpace(m_Repository))
		{
			return true;
		}
		m_Repository = EditorPrefs.GetString("No1lKII9IzcBAbihub6nCg==LK", string.Empty);
		if (!AddConfiguration())
		{
			m_Repository = string.Empty;
		}
		return !(_Worker = string.IsNullOrWhiteSpace(m_Repository));
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
		AuthenticationIdentifier[] reference = new AuthenticationIdentifier[4]
		{
			new AuthenticationIdentifier("wmic baseboard get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ObjectContext[0] = o;
			}, wantfilter: true),
			new AuthenticationIdentifier("wmic cpu get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ObjectContext[1] = o;
			}, wantfilter: true),
			new AuthenticationIdentifier("wmic diskdrive get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ObjectContext[2] = o;
			}, wantfilter: true),
			new AuthenticationIdentifier("wmic memorychip get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ObjectContext[3] = o;
			}, wantfilter: true)
		};
		CS_0024_003C_003E8__locals31.modelContext = new AuthenticationIdentifier[4]
		{
			new AuthenticationIdentifier("Get-CimInstance -class Win32_baseboard | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ServiceContext[0] = o;
			}),
			new AuthenticationIdentifier("Get-CimInstance -class Win32_processor | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ServiceContext[1] = o;
			}),
			new AuthenticationIdentifier("Get-CimInstance -class Win32_diskdrive | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ServiceContext[2] = o;
			}),
			new AuthenticationIdentifier("Get-CimInstance -class win32_physicalmemory | Select *", delegate(string o)
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

	private static void ComputeConfiguration(ParamsIdentifier init)
	{
		QueryConfiguration(init, null);
	}

	private static void QueryConfiguration(ParamsIdentifier i, Action selection, Action comp = null, bool comparesecond2 = true)
	{
		bool num = i.PublishConsumer("success");
		string text = i.PublishConsumer("message");
		string text2 = i.PublishConsumer("url");
		bool flag = !string.IsNullOrEmpty(text2);
		string text3 = i.PublishConsumer("url_name");
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
				NewIdentifier(text);
			}
			selection?.Invoke();
			return;
		}
		bool flag2 = i.PublishConsumer("wait_warn");
		float num2 = i.PublishConsumer("wait_time");
		manager |= flag2;
		if (!(num2 <= 0f))
		{
			_System = Time.realtimeSinceStartup + num2;
		}
		comp?.Invoke();
		if (!string.IsNullOrEmpty(text))
		{
			NewIdentifier(text, CustomLogType.Error);
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
			("version", m_Expression.ToString()),
			("HWID", attr),
			("SID", global),
			("license_key", m_Repository)
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
		setter = text + "/" + text2 + "/" + text3;
		return setter;
	}

	private static void ReflectConfiguration(Action value)
	{
		if (!_Service)
		{
			m_Wrapper = (Action)Delegate.Remove(m_Wrapper, value);
			m_Wrapper = (Action)Delegate.Combine(m_Wrapper, value);
		}
		else if (((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		})())
		{
			value?.Invoke();
		}
	}

	private static void ResolveConfiguration(bool istask)
	{
		if (_Service && ((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + m_Repository));
			return _Object == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(setter + attr)));
		})())
		{
			if (!specification)
			{
				m_Wrapper?.Invoke();
			}
			specification = true;
		}
	}

	private static void ResetConfiguration(bool stripinstance)
	{
	}

	[SpecialName]
	private static string ExcludeSerializer()
	{
		string text = "";
		if (manager)
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
				GUILayout.Label("License: " + (string.IsNullOrWhiteSpace(m_Printer) ? "Personal" : m_Printer), ExceptionSingletonStruct.MapRef().m_ProcSerializer);
				GUILayout.FlexibleSpace();
			}
			if (!string.IsNullOrWhiteSpace(_Parser))
			{
				using (new GUILayout.HorizontalScope(GUI.skin.box))
				{
					GUILayout.Label("Authorized For: " + _Parser, ExceptionSingletonStruct.MapRef()._IdentifierMethod);
					return;
				}
			}
		}
	}

	private static bool FlushConfiguration(EditorWindow first = null, float visitor = 0f)
	{
		if (!_Service)
		{
			if (Event.current.type == EventType.Repaint)
			{
				VisitConfiguration();
			}
			if ((object)first != null)
			{
				ExceptionSingletonStruct.getterSerializer.ConcatComparator(first, visitor);
			}
			ConcatIdentifier();
			if (_Rule || _Struct)
			{
				InitConfiguration(_Rule ? "Activating License..." : "Verifying License...", "Please wait till this finishes processing.");
				return false;
			}
			if (!_Info)
			{
				if (!_Worker || indexer)
				{
					InitConfiguration("Check for License", "This will check for whether you already have a license for your device");
					if (ExceptionSingletonStruct.LoginStatus(indexer ? "Retry" : "Check", EditorStyles.toolbarButton))
					{
						AssetConfiguration(testkey: true);
					}
					return false;
				}
				InitConfiguration("Enter your license key", "Enter the license key you received with your purchase here. If your license was already activated, click on 'Transfer License'. For support, contact @Dreadrith.");
				bool flag = ConnectConfiguration(applyvar1: false);
				if (ExcludeSerializer().Length > 0)
				{
					EditorGUILayout.HelpBox(ExcludeSerializer(), MessageType.Error);
				}
				bool flag2 = AddConfiguration() && !GetSerializer();
				flag &= flag2 && !m_Pool;
				using (new EditorGUI.DisabledScope(!flag2))
				{
					if (ExceptionSingletonStruct.PatchStatus("Activate") || flag)
					{
						PopConfiguration();
					}
				}
				SortIdentifier(CreateConfiguration);
				return false;
			}
			ExcludeConfiguration();
			return false;
		}
		if (m_Algo)
		{
			AwakeConfiguration();
			return false;
		}
		if (!initializer)
		{
			return true;
		}
		Policy.SetupMethod();
		return false;
	}

	private static void ExcludeConfiguration()
	{
		InitConfiguration("Transferring License", "This is for moving your license to a new device or re-activating it in case it fails to recognize your device.");
		if (m_Config)
		{
			EditorGUILayout.HelpBox("A 6-digit verification code was sent to " + m_Strategy + ".\nIf this is not your email address, please contact support.\nIf you don't see the verification email, please check your spam folder.", MessageType.Info);
			descriptor = EditorGUILayout.TextField("Verification Code", descriptor);
			descriptor = Regex.Replace(descriptor, "[^0-9]", string.Empty, RegexOptions.Multiline);
			EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(!Regex.IsMatch(descriptor, "[0-9]{6}") || state);
			try
			{
				if (ExceptionSingletonStruct.PatchStatus(state ? "Transferring..." : "Transfer License"))
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
			EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(_Mock);
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
			disabledScope = new EditorGUI.DisabledScope(!FindConfiguration() || _Mock);
			try
			{
				if (!_Mock)
				{
					goto IL_00ee;
				}
				object asset = "Sending Verification Code...";
				goto IL_00f3;
				IL_00ee:
				asset = "Send Verification Code";
				goto IL_00f3;
				IL_00f3:
				if (ExceptionSingletonStruct.PatchStatus((string)asset))
				{
					VerifyIdentifier();
					goto IL_00ee;
				}
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
		}
		SortIdentifier(CreateConfiguration);
	}

	private static void InitConfiguration(string var1, string ord)
	{
		using (new GUILayout.HorizontalScope(ExceptionSingletonStruct.MapRef()._MerchantSerializer))
		{
			GUILayout.Label(string.Empty, GUILayout.Width(17f), GUILayout.Height(17f));
			GUILayout.Label(var1, ExceptionSingletonStruct.MapRef().m_WriterSerializer);
			GUILayout.Label(new GUIContent(ExceptionSingletonStruct.CustomizeRef()._ModelSerializer)
			{
				tooltip = ord
			}, ExceptionSingletonStruct.MapRef().m_ProducerSerializer, GUILayout.Width(17f), GUILayout.Height(17f));
		}
	}

	private static bool ConnectConfiguration(bool applyvar1)
	{
		using (new GUILayout.HorizontalScope())
		{
			string text = "ADOverhaulLicenseField";
			if (ExceptionSingletonStruct.ComputeStatus(text))
			{
				GUI.FocusControl(null);
				return true;
			}
			if (ExceptionSingletonStruct.QueryStatus(text))
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
				m_Repository = EditorGUILayout.TextField(string.Empty, m_Repository).Trim();
				ExceptionSingletonStruct.AwakeStatus("License Key", string.IsNullOrWhiteSpace(m_Repository), 80f);
			}
			if (!m_Pool && AddConfiguration() && !GetSerializer())
			{
				m_Pool = true;
				return true;
			}
		}
		return false;
	}

	private static bool FindConfiguration()
	{
		if (_Info)
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
		return Regex.Match(m_Repository, "^[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}$").Success;
	}

	private static bool ValidateConfiguration()
	{
		if (m_Config)
		{
			return Regex.Match(descriptor, "^[a-zA-Z0-9]{6}$").Success;
		}
		return true;
	}

	private static void CreateConfiguration()
	{
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.FlexibleSpace();
			if (ExceptionSingletonStruct.InterruptStatus(_Info ? "Activate License" : "Transfer License"))
			{
				_Info = !_Info;
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

	private static async Task<ParamsIdentifier> RunIdentifier(string setup, string vis)
	{
		ParamsIdentifier _DispatcherContext = default(ParamsIdentifier);
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
			_DispatcherContext = new ParamsIdentifier(v);
		});
		return _DispatcherContext;
	}

	private static Task<ParamsIdentifier> OrderIdentifier(string first)
	{
		return RunIdentifier("https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand", first);
	}

	private static void CalculateIdentifier()
	{
		ExceptionSingletonStruct.AddProcess(CalcIdentifier);
	}

	private static void CalcIdentifier()
	{
		MessageUtilsAttribute[] array = Resources.FindObjectsOfTypeAll<MessageUtilsAttribute>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Repaint();
		}
	}

	private static void DeleteIdentifier()
	{
		using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, Color.clear))
		{
			if (GUILayout.Button(new GUIContent("Made By @Dreadrith ♡", "https://dreadrith.com/links"), ExceptionSingletonStruct.MapRef()._ContextMethod))
			{
				Application.OpenURL("https://dreadrith.com/links");
			}
			ExceptionSingletonStruct.PostStatus();
		}
	}

	internal static bool DefineIdentifier(string config, bool wantselection = true)
	{
		return NewIdentifier(config, CustomLogType.Warning, wantselection);
	}

	internal static bool DestroyIdentifier(string instance, bool ignorereg = true)
	{
		return NewIdentifier(instance, CustomLogType.Error, ignorereg);
	}

	internal static bool NewIdentifier(string def, CustomLogType reg = CustomLogType.Regular, bool includefilter = true)
	{
		if (includefilter)
		{
			Color color = ((reg == CustomLogType.Regular) ? ExceptionSingletonStruct._ObserverSerializer : ((reg != CustomLogType.Warning) ? ExceptionSingletonStruct._BroadcasterSerializer : ExceptionSingletonStruct._EventSerializer));
			string message = "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">[ADOverhaul]</color> " + def.Replace("\\n", "\n");
			switch (reg)
			{
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
		}
		return includefilter;
	}

	internal static void CompareIdentifier(string task, bool removecust = true)
	{
		if (removecust)
		{
			throw new Exception("<color=#" + ColorUtility.ToHtmlStringRGB(ExceptionSingletonStruct._BroadcasterSerializer) + ">[ADOverhaul]</color> " + task);
		}
	}

	private static void VerifyIdentifier()
	{
		string message = "License transfer is subject to the Terms of Service.\nLicense will stop working on the device it was previously activated on.\nYou will not be able to transfer back or again for 30 days.";
		switch (EditorUtility.DisplayDialogComplex("Terms of Service", message, "Continue", "Terms of Service", "Cancel"))
		{
		case 0:
			_Mock = true;
			CloneConfiguration(delegate
			{
				List<(string, string)> list = CountConfiguration("transferlicenserequest");
				StartConfiguration(list);
				OrderIdentifier(IncludeConfiguration(list.ToArray())).CreateProcess(delegate(ParamsIdentifier response)
				{
					_003C_003Ec__DisplayClass179_0 _003C_003Ec__DisplayClass179_ = new _003C_003Ec__DisplayClass179_0();
					_003C_003Ec__DisplayClass179_.composerContext = response;
					_Mock = false;
					QueryConfiguration(_003C_003Ec__DisplayClass179_.composerContext, _003C_003Ec__DisplayClass179_.RestartReg);
				}, delegate(Exception exception)
				{
					_Mock = false;
					NewIdentifier($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
				}, null, null, CalculateIdentifier);
			}, ispred: true);
			break;
		case 1:
			Application.OpenURL("https://dreadrith.com/license-tos");
			break;
		}
	}

	private static void SetIdentifier()
	{
		state = true;
		CloneConfiguration(delegate
		{
			List<(string, string)> list = CountConfiguration("transferlicenseconfirm");
			list.Add(("verification_code", descriptor));
			StartConfiguration(list);
			OrderIdentifier(IncludeConfiguration(list.ToArray())).CreateProcess(delegate(ParamsIdentifier response)
			{
				state = false;
				QueryConfiguration(response, delegate
				{
					_Info = false;
					m_Config = false;
					while (true)
					{
						_Worker = false;
					}
				});
			}, delegate(Exception exception)
			{
				state = false;
				NewIdentifier($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, CalculateIdentifier);
		}, ispred: true);
	}

	[SpecialName]
	private static bool ConnectSerializer()
	{
		return RefImporterDescriptor.GetConsumer().u_updateDay == RemoveConfiguration();
	}

	private static void SortIdentifier(Action first = null, Action<GenericMenu> col = null)
	{
		using (new GUILayout.VerticalScope(GUI.skin.box))
		{
			using (new GUILayout.HorizontalScope())
			{
				if (ExceptionSingletonStruct.ListStatus(ExceptionSingletonStruct.CustomizeRef().m_SpecificationSerializer))
				{
					InvokeIdentifier(col);
				}
				if (!RefImporterDescriptor.GetConsumer().u_updateHidden && creator && ExceptionSingletonStruct.ListStatus(ExceptionSingletonStruct.CustomizeRef()._ParameterSerializer))
				{
					dispatcher.target = !dispatcher.target;
				}
				GUILayout.Label("v" + m_Expression, ExceptionSingletonStruct.MapRef().m_AuthenticationMethod, GUILayout.ExpandWidth(expand: false));
				if (first == null)
				{
					GUILayout.FlexibleSpace();
					DeleteIdentifier();
				}
				else
				{
					first();
				}
			}
			if (creator)
			{
				CustomizeIdentifier();
			}
		}
	}

	private static void InvokeIdentifier(Action<GenericMenu> ident = null)
	{
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Check For Update"), on: false, (!m_Advisor && !exporter) ? ((GenericMenu.MenuFunction)delegate
		{
			SessionState.EraseString("No1lKII9IzcBAbihub6nCg==updateinfo");
			FillIdentifier();
		}) : null);
		if (_Service)
		{
			genericMenu.AddItem(new GUIContent("Send Feedback"), m_Algo, delegate
			{
				m_Algo.SetStatus();
			});
		}
		if (_Service)
		{
			if (ident != null)
			{
				ident(genericMenu);
				genericMenu.AddSeparator(string.Empty);
			}
			genericMenu.AddSeparator(string.Empty);
			genericMenu.AddItem(new GUIContent("Verify/On Display"), RefImporterDescriptor.GetConsumer().a_VerifyOnDisplay, delegate
			{
				RefImporterDescriptor.GetConsumer().a_VerifyOnDisplay.IncludeConsumer();
				RefImporterDescriptor.GetConsumer().a_VerifyOnProjectLoad.ConcatUtils(nores: false);
			});
			genericMenu.AddItem(new GUIContent("Verify/On Project Load"), RefImporterDescriptor.GetConsumer().a_VerifyOnProjectLoad, delegate
			{
				RefImporterDescriptor.GetConsumer().a_VerifyOnProjectLoad.IncludeConsumer();
				RefImporterDescriptor.GetConsumer().a_VerifyOnDisplay.ConcatUtils(nores: false);
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
		if (_Service)
		{
			if (m_Decorator.Length != 0)
			{
				if (m_Decorator.Length <= 1)
				{
					genericMenu.AddItem(new GUIContent(m_Decorator[0].Item1), on: false, delegate
					{
						Application.OpenURL(m_Decorator[0].Item2);
					});
				}
				else
				{
					(string, string)[] decorator = m_Decorator;
					for (int num = 0; num < decorator.Length; num++)
					{
						(string, string) tuple = decorator[num];
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

	private static void CustomizeIdentifier(bool isres = true)
	{
		if ((bool)RefImporterDescriptor.GetConsumer().u_updateHidden)
		{
			return;
		}
		dispatcher.InvokeStatus(delegate
		{
			if (isres)
			{
				ExceptionSingletonStruct.DisableStatus();
			}
			EditorGUILayout.HelpBox($"Version {RefImporterDescriptor.GetConsumer().u_updateVersion}\n--------------\n{RefImporterDescriptor.GetConsumer().u_updateMessage}", MessageType.Info);
			bool flag = !string.IsNullOrWhiteSpace(RefImporterDescriptor.GetConsumer().u_updateLink);
			bool flag2 = !string.IsNullOrWhiteSpace(RefImporterDescriptor.GetConsumer().u_updateChangelog);
			using (new GUILayout.HorizontalScope())
			{
				if (flag)
				{
					using (new EditorGUI.DisabledScope(m_Field))
					{
						if (ExceptionSingletonStruct.LoginStatus("Download Update", EditorStyles.toolbarButton))
						{
							LogoutIdentifier();
						}
					}
				}
				if (flag2 && ExceptionSingletonStruct.CallStatus(new GUIContent("Open Changelog", RefImporterDescriptor.GetConsumer().u_updateChangelog), EditorStyles.toolbarButton))
				{
					Application.OpenURL(RefImporterDescriptor.GetConsumer().u_updateChangelog);
				}
				if (ExceptionSingletonStruct.LoginStatus("Skip for Today", EditorStyles.toolbarButton))
				{
					RefImporterDescriptor.GetConsumer().u_updateHidden.ConcatUtils(nores: true);
				}
			}
		}, CalcIdentifier);
	}

	private static void ConcatIdentifier()
	{
		if ((bool)RefImporterDescriptor.GetConsumer().u_announcementHidden || string.IsNullOrWhiteSpace(RefImporterDescriptor.GetConsumer().u_announcement))
		{
			return;
		}
		using (new GUILayout.VerticalScope(EditorStyles.helpBox))
		{
			Rect _CallbackContext = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(expand: true), GUILayout.Height(24f));
			Rect setup = _CallbackContext;
			GUI.Label(setup.SortStatus(24f, isres: true), ExceptionSingletonStruct.CustomizeRef()._ObjectSerializer);
			GUI.Label(setup, "Announcement", ExceptionSingletonStruct.MapRef().m_ExceptionSerializer);
			m_Connection.InvokeStatus(delegate
			{
				_CallbackContext.height += 18f;
				ExceptionSingletonStruct.DisableStatus();
				EditorGUILayout.HelpBox(RefImporterDescriptor.GetConsumer().u_announcement, MessageType.Info);
				using (new GUILayout.HorizontalScope())
				{
					if (!string.IsNullOrWhiteSpace(RefImporterDescriptor.GetConsumer().u_announcementLink) && ExceptionSingletonStruct.LoginStatus(RefImporterDescriptor.GetConsumer().u_announcementLinkName, EditorStyles.toolbarButton))
					{
						Application.OpenURL(RefImporterDescriptor.GetConsumer().u_announcementLink);
					}
					if (_Service && ExceptionSingletonStruct.LoginStatus("Hide", EditorStyles.toolbarButton))
					{
						RefImporterDescriptor.GetConsumer().u_announcementHidden.ConcatUtils(nores: true);
						RefImporterDescriptor.GetConsumer().u_announcementHiddenDate.IncludeUtils(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
					}
				}
			}, CalcIdentifier);
			if (ExceptionSingletonStruct.ReadStatus(_CallbackContext))
			{
				m_Connection.target = !m_Connection.target;
			}
		}
	}

	[InitializeOnLoadMethod]
	private static void MapIdentifier()
	{
		if (!ConnectSerializer() || string.IsNullOrWhiteSpace(RefImporterDescriptor.GetConsumer().u_updateVersion.CreateUtils()))
		{
			ExceptionSingletonStruct.AddProcess(delegate
			{
				CancelIdentifier(isparam: false);
			});
		}
		else
		{
			SetupIdentifier(iskey: false);
		}
	}

	private static void FillIdentifier()
	{
		CancelIdentifier(isparam: true);
	}

	private static void CancelIdentifier(bool isparam)
	{
		if ((!isparam && ConnectSerializer()) || exporter || m_Advisor)
		{
			return;
		}
		m_Advisor = true;
		OrderIdentifier(IncludeConfiguration(new List<(string, string)>
		{
			("command", "getdownloadinfo"),
			("product_id", "No1lKII9IzcBAbihub6nCg=="),
			("version", m_Expression.ToString())
		})).CreateProcess(delegate(ParamsIdentifier response)
		{
			exporter = true;
			string text = RefImporterDescriptor.GetConsumer().u_announcement.CreateUtils();
			using (new RefImporterDescriptor.RepositoryAuthenticationFactory())
			{
				RefImporterDescriptor.GetConsumer().u_updateLink.IncludeUtils(response.PublishConsumer("download_link"));
				RefImporterDescriptor.GetConsumer().u_updateMessage.IncludeUtils(response.PublishConsumer("download_message"));
				RefImporterDescriptor.GetConsumer().u_updateChangelog.IncludeUtils(response.PublishConsumer("changelog_link"));
				RefImporterDescriptor.GetConsumer().u_updateVersion.IncludeUtils(response.PublishConsumer("version"));
				RefImporterDescriptor.GetConsumer().u_updateDay.IncludeUtils(RemoveConfiguration());
				RefImporterDescriptor.GetConsumer().u_announcement.IncludeUtils(response.PublishConsumer("announcement"));
				if (!string.IsNullOrWhiteSpace(RefImporterDescriptor.GetConsumer().u_announcement))
				{
					RefImporterDescriptor.GetConsumer().u_announcement.IncludeUtils(RefImporterDescriptor.GetConsumer().u_announcement.CreateUtils().Replace("\\\\n", "\n").Replace("\\n", "\n"));
				}
				RefImporterDescriptor.GetConsumer().u_announcementLink.IncludeUtils(response.PublishConsumer("announcement_link"));
				RefImporterDescriptor.GetConsumer().u_announcementLinkName.IncludeUtils(response.PublishConsumer("announcement_link_name"));
			}
			if (text != RefImporterDescriptor.GetConsumer().u_announcement.CreateUtils())
			{
				RefImporterDescriptor.GetConsumer().u_announcementHidden.ConcatUtils(nores: false);
			}
			SetupIdentifier(isparam);
		}, delegate(Exception exc)
		{
			NewIdentifier($"Something went wrong while checking for an update!\n\n{exc}", CustomLogType.Error);
		}, null, null, delegate
		{
			m_Advisor = false;
			CalculateIdentifier();
		});
	}

	private static void LogoutIdentifier()
	{
		m_Field = true;
		UnityWebRequest policyContext = new UnityWebRequest(RefImporterDescriptor.GetConsumer().u_updateLink);
		policyContext.downloadHandler = new DownloadHandlerFile("Assets/ADOverhaul.unitypackage");
		policyContext.SendWebRequest().completed += delegate
		{
			m_Field = false;
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

	private static void SetupIdentifier(bool iskey)
	{
		if ((bool)RefImporterDescriptor.GetConsumer().u_announcementHidden)
		{
			if (DateTime.TryParse(RefImporterDescriptor.GetConsumer().u_announcementHiddenDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result))
			{
				RefImporterDescriptor.GetConsumer().u_announcementHidden.ConcatUtils((DateTime.UtcNow - result).TotalDays < 7.0);
			}
			else
			{
				RefImporterDescriptor.GetConsumer().u_announcementHidden.ConcatUtils(nores: false);
			}
		}
		if (!(m_Expression < new IssuerSerializerAdapter(RefImporterDescriptor.GetConsumer().u_updateVersion.CreateUtils())))
		{
			if (iskey)
			{
				NewIdentifier("Up to date!");
				Task.Run(async delegate
				{
					await Task.Delay(3000);
					RefImporterDescriptor.GetConsumer().u_updateHidden.ConcatUtils(nores: true);
					CalculateIdentifier();
				});
			}
			else
			{
				RefImporterDescriptor.GetConsumer().u_updateHidden.ConcatUtils(nores: true);
			}
			return;
		}
		creator = true;
		if (iskey)
		{
			RefImporterDescriptor.GetConsumer().u_updateHidden.ConcatUtils(nores: false);
			dispatcher.target = true;
		}
		if (!RefImporterDescriptor.GetConsumer().u_updateHidden)
		{
			NewIdentifier($"Update Available! <b>(v{RefImporterDescriptor.GetConsumer().u_updateVersion})</b>");
		}
	}

	internal static void SelectIdentifier(string v, AnimBool caller, Action dir, Action task2)
	{
		using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(v, ExceptionSingletonStruct.MapRef()._ConsumerMethod);
				dir?.Invoke();
			}
			if (ExceptionSingletonStruct.ReadStatus())
			{
				caller.target = !caller.target;
				if (!RefImporterDescriptor.GetConsumer().editorAnimatedFoldouts)
				{
					caller.value = caller.target;
				}
			}
			caller.InvokeStatus(task2);
		}
	}

	internal static void WriteIdentifier(SceneView key, string reg, Action control, float key2, float task3)
	{
		MoveIdentifier(key, delegate
		{
			using (new GUILayout.HorizontalScope())
			{
				ExceptionSingletonStruct.AssetStatus();
				GUILayout.FlexibleSpace();
				GUILayout.Label(reg, ExceptionSingletonStruct.MapRef().m_WriterSerializer);
				Rect lastRect = GUILayoutUtility.GetLastRect();
				GUILayout.FlexibleSpace();
				PublishIdentifier();
				return lastRect;
			}
		}, control, key2, task3);
	}

	internal static void MoveIdentifier(SceneView ident, Func<Rect> result, Action consumer, float pol2, float ivk3)
	{
		Rect ivk4 = ident.AddStatus();
		ExceptionSingletonStruct.PositionFlag positionFlag = RefImporterDescriptor.GetConsumer().toolOverlayAlignment.RegisterUtils<ExceptionSingletonStruct.PositionFlag>();
		bool flag;
		using (new ExceptionSingletonStruct.SystemSerializer(ident, pol2, ivk3, positionFlag, customer))
		{
			Rect rect = result();
			ExceptionSingletonStruct.InsertStatus(rect, MouseCursor.Pan);
			flag = ExceptionSingletonStruct.VisitStatus(rect, m_Database);
			if (consumer != null)
			{
				ExceptionSingletonStruct.DisableStatus(2, 0);
				consumer();
			}
		}
		if (flag)
		{
			Handles.BeginGUI();
			RefImporterDescriptor.GetConsumer().toolOverlayAlignment.RestartUtils = (int)ExceptionSingletonStruct.RunStatus(positionFlag, ivk4);
			Handles.EndGUI();
		}
	}

	internal static void PublishIdentifier()
	{
		if (ExceptionSingletonStruct.ListStatus(ExceptionSingletonStruct.CustomizeRef().fieldSerializer))
		{
			MessageUtilsAttribute.CreateSerializer();
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
		var.m_WriterContext = third._TemplateContext - col.m_TokenizerContext[col._ExceptionContext].m_ReaderMethod;
		switch (col.m_ValueContext)
		{
		case 2:
		{
			PrintIdentifier(col.m_TokenizerContext[col._ExceptionContext], out col.m_TokenizerContext[col._ExceptionContext].m_ReaderMethod, out col.m_TokenizerContext[col._ExceptionContext].m_StubMethod, ref var);
			for (int j = 0; j < col.m_TokenizerContext.Length; j++)
			{
				if (j != col._ExceptionContext)
				{
					col.m_TokenizerContext[j].m_ReaderMethod = col.m_TokenizerContext[col._ExceptionContext].m_ReaderMethod;
					if (col.m_TokenizerContext[j]._CandidateMethod == 0)
					{
						col.m_TokenizerContext[j].m_StubMethod = col.m_TokenizerContext[j].m_ReaderMethod * 2f;
					}
					else
					{
						col.m_TokenizerContext[j].m_StubMethod += var.m_WriterContext * 2f;
					}
				}
			}
			break;
		}
		case 1:
			PrintIdentifier(col.m_TokenizerContext[col._ExceptionContext], out col.m_TokenizerContext[col._ExceptionContext].m_ReaderMethod, out col.m_TokenizerContext[col._ExceptionContext].m_StubMethod, ref var);
			break;
		case 0:
		{
			for (int i = 0; i < col.m_TokenizerContext.Length; i++)
			{
				PrintIdentifier(col.m_TokenizerContext[i], out col.m_TokenizerContext[i].m_ReaderMethod, out col.m_TokenizerContext[i].m_StubMethod, ref var);
			}
			break;
		}
		}
	}

	[CompilerGenerated]
	internal static void PrintIdentifier(ExceptionSingletonStruct.TaskMethod key, out float counter, out float consumer, ref _003C_003Ec__DisplayClass46_2 var12)
	{
		counter = key.m_ReaderMethod + var12.m_WriterContext;
		if (key._CandidateMethod != 0)
		{
			consumer = key.m_StubMethod + var12.m_WriterContext * 2f;
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
			pool.m_TokenizerContext[pool._ExceptionContext].m_StubMethod += num;
			for (int j = 0; j < pool.m_TokenizerContext.Length; j++)
			{
				pool.m_TokenizerContext[j].m_StubMethod = pool.m_TokenizerContext[pool._ExceptionContext].m_StubMethod;
			}
			break;
		}
		case 1:
			pool.m_TokenizerContext[pool._ExceptionContext].m_StubMethod += num;
			break;
		case 0:
		{
			for (int i = 0; i < pool.m_TokenizerContext.Length; i++)
			{
				pool.m_TokenizerContext[i].m_StubMethod += num;
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
			helper.Add(item.Key, item.Value);
			candidate.Add(item.Value, item.Key);
			_Test.Add(item.Value, value: false);
			if (watcher.Contains(item.Key.gameObject))
			{
				pol.bridgeContext.Add(item.Value.transform);
			}
		}
	}

	[CompilerGenerated]
	internal static void PostIdentifier(bool rejectkey, ref _003C_003Ec__DisplayClass86_0 visitor, ref _003C_003Ec__DisplayClass86_1 field)
	{
		NewIdentifier(rejectkey ? (visitor.serializerSerializer.stringValue + " added to " + field._MethodSerializer.name) : (visitor.serializerSerializer.stringValue + " already exists in " + field._MethodSerializer.name));
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
		OrderIdentifier(IncludeConfiguration(list.ToArray())).CreateProcess(delegate(ParamsIdentifier response)
		{
			_Rule = false;
			QueryConfiguration(response, delegate
			{
				_Worker = false;
				RefImporterDescriptor.GetConsumer().a_HasSucceededLastVerification.ConcatUtils(nores: true);
				AssetConfiguration(testkey: true);
			});
		}, delegate(Exception exception)
		{
			_Rule = false;
			NewIdentifier($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
		}, null, null, CalculateIdentifier);
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
	internal static async void PrepareIdentifier(AuthenticationIdentifier[] reference, Action visitor, CancellationTokenSource state)
	{
		try
		{
			await Task.Run(delegate
			{
				AuthenticationIdentifier[] array = reference;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].ComputeMethod();
				}
			}, state.Token);
			while (!reference.All((AuthenticationIdentifier p) => p.m_SingletonIdentifier))
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

	MethodInfo ListAuthentication(string setup, BindingFlags result)
	{
		return ((Type)this).GetMethod(setup, result);
	}

	internal static bool TestTokenizer()
	{
		return DefineTokenizer == null;
	}
}
