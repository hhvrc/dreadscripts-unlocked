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

internal sealed class ConfigurationTestStub
{
	private sealed class Queue : EditorWindow
	{
		private enum EasyDynamicsFunctions
		{
			EasyGrab,
			EasyTouch,
			EasyPat
		}

		private static int m_Annotation;

		private static readonly string[] _Value = new string[2] { "Easy Dynamics", "Cosmetic" };

		private static readonly AdvisorDicBridge.PolicyProducerList _Repository = new AdvisorDicBridge.PolicyProducerList("https://raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png", requiresvisitor: true, "DreadBanner.png");

		private static EasyDynamicsFunctions m_Ref = EasyDynamicsFunctions.EasyGrab;

		private static bool candidate;

		private static bool _Expression;

		private static bool _Stub;

		private static bool m_Mock;

		private static bool _Instance;

		private static bool listener;

		private static bool _Parameter;

		private static Queue CreateGlobal;

		[MenuItem("DreadTools/ADOverhaul", false, 6)]
		internal static void PublishTemplate()
		{
			EditorWindow.GetWindow<Queue>(utility: false, "Avatar Dynamics Overhaul", focus: true);
		}

		private void OnGUI()
		{
			if (QuerySystem(this))
			{
				RevertTemplate();
				AdvisorDicBridge.RemoveManager();
				SearchSystem();
				UpdateStruct();
				ListStruct();
				_Repository.ListWrapper(this);
			}
		}

		private void InstantiateTemplate()
		{
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				FillSystem();
			}
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				m_Ref = (EasyDynamicsFunctions)(object)EditorGUILayout.EnumPopup(AdvisorDicBridge.PrepareRequest().m_RegTemplate, m_Ref);
			}
			EditorGUILayout.HelpBox("Under Development", MessageType.Info);
		}

		private void RevertTemplate()
		{
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				_Instance = EditorGUILayout.Foldout(_Instance, "Editor", toggleOnLabelClick: true);
				if (_Instance)
				{
					EditorGUI.indentLevel++;
					ManagerStruct.SearchTest().editorAnimatedFoldouts.ReflectService(AdvisorDicBridge.PrepareRequest().m_ExpressionTemplate, null);
					EditorGUI.indentLevel--;
				}
			}
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				listener = EditorGUILayout.Foldout(listener, "Handles", toggleOnLabelClick: true);
				if (listener)
				{
					EditorGUI.indentLevel++;
					using (new GUILayout.HorizontalScope())
					{
						ManagerStruct.SearchTest().onSceneNameLabels.ReflectService(AdvisorDicBridge.PrepareRequest().m_StubTemplate, null);
						if ((bool)ManagerStruct.SearchTest().onSceneNameLabels)
						{
							ManagerStruct.SearchTest().labelColor.CountError(GUIContent.none, true);
						}
					}
					ManagerStruct.SearchTest().generalColor.CountError(AdvisorDicBridge.PrepareRequest()._InstanceTemplate, true);
					ManagerStruct.SearchTest().activeColor.CountError(AdvisorDicBridge.PrepareRequest().m_ListenerTemplate, true);
					ManagerStruct.SearchTest().inactiveColor.CountError(AdvisorDicBridge.PrepareRequest().observerTemplate, true);
					ManagerStruct.SearchTest().mixedColor.CountError(AdvisorDicBridge.PrepareRequest().m_ParameterTemplate, true);
					ManagerStruct.SearchTest().selectionColor.CountError(AdvisorDicBridge.PrepareRequest().importerTemplate, true);
					ManagerStruct.SearchTest().handleSizeMultiplier.VerifyService(AdvisorDicBridge.PrepareRequest()._CandidateTemplate, true, null);
					EditorGUI.indentLevel--;
				}
			}
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				_Parameter = EditorGUILayout.Foldout(_Parameter, "Overlay", toggleOnLabelClick: true);
				if (!_Parameter)
				{
					return;
				}
				EditorGUI.indentLevel++;
				using (new GUILayout.HorizontalScope())
				{
					ManagerStruct.SearchTest().onSceneToolSelection.ReflectService(new GUIContent("Tool Overlay", "Displays the tool selection overlay on the scene view."), null);
					using (new EditorGUI.DisabledScope(!ManagerStruct.SearchTest().onSceneToolSelection))
					{
						ManagerStruct.SearchTest().toolSelectionOverlayAlignment.ChangeService<AdvisorDicBridge.PositionFlag>("Position", requiresb: false, null, Array.Empty<GUILayoutOption>());
					}
				}
				using (new GUILayout.HorizontalScope())
				{
					ManagerStruct.SearchTest().onSceneEditingOverlay.ReflectService(AdvisorDicBridge.PrepareRequest().messageTemplate, null);
					using (new EditorGUI.DisabledScope(!ManagerStruct.SearchTest().onSceneEditingOverlay))
					{
						ManagerStruct.SearchTest().toolOverlayAlignment.ChangeService<AdvisorDicBridge.PositionFlag>("Position", requiresb: false, null, Array.Empty<GUILayoutOption>());
					}
				}
				ManagerStruct.SearchTest().onSceneTooltip.ReflectService(AdvisorDicBridge.PrepareRequest().bridgeTemplate, null);
				EditorGUI.indentLevel--;
			}
		}

		private void OnEnable()
		{
			CheckSystem(ref m_Mapping, ref m_Definition, VerifySystem);
		}

		internal static bool CancelGlobal()
		{
			return (object)CreateGlobal == null;
		}
	}

	private static class WorkerModelDispatcher
	{
		internal struct Reader
		{
			internal string _Writer;

			internal ushort m_Interpreter;

			internal ushort m_Attribute;

			internal string issuer;
		}

		private static string _Importer;

		private static bool _List;

		private static bool reg;

		private static bool m_Message;

		private static bool bridge;

		private static Reader? m_Utils;

		private static Reader? m_Identifier;

		private static Action global;

		private static ushort _Policy;

		internal static bool _Dispatcher;

		internal static readonly HashSet<Reader> m_Collection = new HashSet<Reader>();

		internal static WorkerModelDispatcher PrepareGlobal;

		[SpecialName]
		private static float DestroyDic()
		{
			return (float)(int)_Policy / 1f;
		}

		internal static void InsertDic(Action key, ushort cfg_end = 0, string helper = "", ushort indextask2 = 0, bool movepol3 = false, string ident4 = "")
		{
			PrepareDic(key, null, cfg_end, helper, indextask2, movepol3, ident4);
		}

		internal static void PrepareDic(Action reference, Action attr, ushort length_pool = 0, string spec2 = "", ushort min_asset3 = 0, bool containscust4 = false, string instance5 = "")
		{
			global = attr;
			if (length_pool > 0)
			{
				ResolveDic(length_pool, spec2, min_asset3);
			}
			try
			{
				reference();
			}
			catch (Exception res)
			{
				if (!_Dispatcher)
				{
					ListDic(res, containscust4, instance5);
					CompilationPipeline.compilationStarted -= InterruptDic;
					CompilationPipeline.compilationStarted += InterruptDic;
					throw;
				}
				throw;
			}
		}

		private static void ListDic(Exception res, bool removecfg = false, string c = "")
		{
			if (!m_Identifier.HasValue || m_Collection.Contains(m_Identifier.Value))
			{
				return;
			}
			_Importer = string.Empty;
			_List = false;
			m_Message = false;
			reg = false;
			m_Utils = new Reader
			{
				_Writer = m_Identifier.Value._Writer,
				m_Interpreter = m_Identifier.Value.m_Interpreter,
				m_Attribute = m_Identifier.Value.m_Attribute,
				issuer = res.Message
			};
			if (removecfg)
			{
				switch (EditorUtility.DisplayDialogComplex("Error!", (!string.IsNullOrWhiteSpace(c)) ? c : "An error has occurred! Do you want to try to find a solution for it?", "Find Solution", "Close", "Ignore"))
				{
				case 2:
					m_Collection.Add(m_Utils.Value);
					InterruptDic(null);
					break;
				case 1:
					InterruptDic(null);
					break;
				case 0:
					m_Collection.Add(m_Utils.Value);
					CollectTemplate(isres: true);
					break;
				}
			}
		}

		internal static void ManageDic()
		{
			if (!ReadDic())
			{
				return;
			}
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(AdvisorDicBridge.PrepareRequest().fieldTemplate, AdvisorDicBridge.ManageRequest().m_ReaderTemplate);
				GUILayout.Label("An error has occurred! Do you want to report it?", EditorStyles.boldLabel);
				if (AdvisorDicBridge.GetManager("Ignore"))
				{
					CompareDic(calcitem: false);
				}
				if (AdvisorDicBridge.GetManager("Find Solution"))
				{
					CompareDic(calcitem: true);
				}
			}
			AdvisorDicBridge.RemoveManager();
		}

		internal static bool ReadDic()
		{
			if (!m_Utils.HasValue)
			{
				return false;
			}
			if (!m_Collection.Contains(m_Utils.Value))
			{
				return true;
			}
			m_Utils = null;
			return false;
		}

		internal static void ResolveDic(ushort init_X, string cust = "", ushort no__res = 0)
		{
			m_Identifier = new Reader
			{
				m_Interpreter = init_X,
				_Writer = cust,
				m_Attribute = no__res
			};
		}

		internal static void VerifyDic()
		{
			_Importer = string.Empty;
			_List = false;
			_Dispatcher = false;
			_Policy = 0;
			m_Identifier = null;
		}

		internal static void ConnectDic()
		{
			CollectTemplate(m_Invocation && m_Utils.HasValue);
			if (!m_Message)
			{
				m_Message = true;
				bridge = true;
				List<(string, string)> list = PrintSystem("findsolution", new(string, string)[4]
				{
					("bug_id", m_Utils.Value.m_Interpreter.ToString()),
					("bug_version", m_Utils.Value.m_Attribute.ToString()),
					("bug_name", m_Utils.Value._Writer),
					("bug_exception", Uri.EscapeUriString(m_Utils.Value.issuer))
				});
				FindSystem(list);
				CountStruct(InstantiateSystem(list.ToArray())).PublishAccount(delegate(GetterDicBridge response)
				{
					bool flag = response.StartTest("success");
					string text = response.StartTest("message");
					uint num = default(uint);
					while (true)
					{
						reg = true;
						if (!string.IsNullOrWhiteSpace(text))
						{
							switch ((num = (uint)(0x2069BEBE ^ ((int)num * -302936629) ^ -1617907521)) % 6)
							{
							default:
								return;
							case 0u:
							case 5u:
								break;
							case 4u:
								goto end_IL_005c;
							case 2u:
								goto IL_0084;
							case 3u:
								goto IL_0090;
							case 1u:
								return;
							}
							continue;
						}
						goto IL_0084;
						IL_0090:
						_Importer = response.StartTest("solution");
						break;
						IL_0084:
						StopStruct(text, (!flag) ? CustomLogType.Warning : CustomLogType.Regular);
						goto IL_0090;
						continue;
						end_IL_005c:
						break;
					}
					_List = response.StartTest("complete");
				}, UnityEngine.Debug.LogException, null, null, delegate
				{
					bridge = false;
					SetStruct();
				});
			}
			EnableSystem((!bridge) ? "Bug Reporter" : "Finding a solution...", "If you have found a bug, please report it here!\nNote that the report is not anonymous. Abuse may result in blacklisting.");
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				if (bridge)
				{
					if (AdvisorDicBridge.ResetManager("Cancel", EditorStyles.toolbarButton))
					{
						CollectTemplate(isres: false);
					}
					return;
				}
				if (!reg)
				{
					using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
					{
						GUILayout.Label(AdvisorDicBridge.PrepareRequest().fieldTemplate, AdvisorDicBridge.ManageRequest().m_ReaderTemplate);
						using (new TaskServiceClass(TaskServiceClass.ColoringType.FG, AdvisorDicBridge.infoTemplate))
						{
							GUILayout.Label("There was an issue contacting the server for a solution.");
						}
					}
					if (AdvisorDicBridge.GetManager("Cancel"))
					{
						CollectTemplate(isres: false);
					}
					return;
				}
				if (!string.IsNullOrWhiteSpace(_Importer))
				{
					if (!_List)
					{
						using (new TaskServiceClass(TaskServiceClass.ColoringType.FG, AdvisorDicBridge._AuthenticationTemplate))
						{
							GUILayout.Label("Known issue! Details:");
						}
					}
					else
					{
						using (new TaskServiceClass(TaskServiceClass.ColoringType.FG, AdvisorDicBridge.m_InitializerTemplate))
						{
							GUILayout.Label("Solution Found!");
						}
					}
					EditorGUILayout.Space();
					EditorGUILayout.SelectableLabel(_Importer, GUI.skin.label, GUILayout.ExpandHeight(expand: false));
					if (AdvisorDicBridge.GetManager("Ok"))
					{
						CollectTemplate(isres: false);
					}
					return;
				}
				using (new TaskServiceClass(TaskServiceClass.ColoringType.FG, AdvisorDicBridge._AuthenticationTemplate))
				{
					GUILayout.Label("No solution Found! Please write the steps to reproduce this issue below:");
				}
				registry = EditorGUILayout.TextArea(registry, GUILayout.MinHeight(54f));
				if (!string.IsNullOrWhiteSpace(registry) && registry.Length > 2000)
				{
					registry = registry.Substring(0, 2000);
				}
				if (!string.IsNullOrWhiteSpace(_Importer))
				{
					return;
				}
				using (new GUILayout.HorizontalScope())
				{
					if (AdvisorDicBridge.GetManager("Cancel", GUILayout.ExpandWidth(expand: false)))
					{
						CollectTemplate(isres: false);
					}
					using (new EditorGUI.DisabledScope(_Role))
					{
						if (!AdvisorDicBridge.GetManager("Report Issue"))
						{
							return;
						}
						List<(string, string)> list2 = PrintSystem("reportbug", new(string, string)[5]
						{
							("bug_id", m_Utils.Value.m_Interpreter.ToString()),
							("bug_version", m_Utils.Value.m_Attribute.ToString()),
							("bug_name", m_Utils.Value._Writer),
							("bug_exception", m_Utils.Value.issuer),
							("feedback", Uri.EscapeUriString(registry))
						});
						FindSystem(list2);
						_Role = true;
						CountStruct(InstantiateSystem(list2.ToArray())).PublishAccount(delegate(GetterDicBridge response)
						{
							bool flag = response.StartTest("success");
							string text = response.StartTest("message");
							if (!string.IsNullOrEmpty(text))
							{
								StopStruct(text, (!flag) ? CustomLogType.Warning : CustomLogType.Regular);
							}
						}, UnityEngine.Debug.LogException, null, null, delegate
						{
							CollectTemplate(isres: false);
							_Role = false;
							SetStruct();
						});
					}
				}
			}
		}

		internal static void CompareDic(bool calcitem)
		{
			if (ReadDic() && m_Utils.HasValue)
			{
				if (m_Collection.Contains(m_Utils.Value))
				{
					m_Utils = null;
				}
				CollectTemplate(calcitem);
				m_Collection.Add(m_Utils.Value);
			}
		}

		internal static void InterruptDic(object ident)
		{
			if (m_Utils.HasValue && global != null)
			{
				InsertDic(global, m_Utils.Value.m_Interpreter, m_Utils.Value._Writer, m_Utils.Value.m_Attribute);
			}
			global = null;
			CompilationPipeline.compilationStarted -= InterruptDic;
		}

		internal static bool InstantiateGlobal()
		{
			return PrepareGlobal == null;
		}
	}

	private sealed class ListServiceSerializer
	{
		private readonly ProcessStartInfo proc;

		private Process _SystemStruct;

		private readonly Action<string> m_StructStruct;

		private readonly Action configStruct;

		private readonly bool m_ModelStruct;

		private string _TemplateStruct;

		private bool _DicStruct;

		internal bool _ServiceStruct;

		private bool errorStruct;

		private static ListServiceSerializer PushGlobal;

		internal ListServiceSerializer(string info, Action<string> second, bool iscontrol = false, bool iscust2 = false, Action v3 = null)
		{
			proc = new ProcessStartInfo((!iscontrol) ? "powershell.exe" : "cmd.exe")
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardInput = false,
				RedirectStandardOutput = true,
				Arguments = "/c " + info
			};
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
			proc.WorkingDirectory = folderPath;
			if (!iscontrol)
			{
				string text = Path.Combine(folderPath, "WindowsPowerShell", "v1.0");
				if (Directory.Exists(text))
				{
					proc.WorkingDirectory = text;
				}
			}
			m_StructStruct = second;
			configStruct = v3;
			m_ModelStruct = iscust2;
		}

		internal void SortDic()
		{
			_TemplateStruct = string.Empty;
			errorStruct = false;
			_ServiceStruct = false;
			_DicStruct = false;
			_SystemStruct = new Process();
			_SystemStruct.StartInfo = proc;
			_SystemStruct.Start();
			try
			{
				do
				{
					_TemplateStruct = _SystemStruct.StandardOutput.ReadToEnd();
				}
				while (string.IsNullOrEmpty(_TemplateStruct) && !_SystemStruct.HasExited);
				errorStruct = true;
				SetupDic();
			}
			catch (Exception ex)
			{
				errorStruct = false;
				_TemplateStruct = "Failure! Exception: " + ex.Message + "\n" + ex.StackTrace;
				_SystemStruct?.Close();
				((ListServiceSerializer)(object)_SystemStruct)?.SelectTest();
				SetupDic();
			}
			_SystemStruct.WaitForExit();
		}

		private void SetupDic()
		{
			if (_DicStruct)
			{
				return;
			}
			_DicStruct = true;
			try
			{
				string text = _TemplateStruct.ToString();
				if (string.IsNullOrWhiteSpace(text))
				{
					text = "Missing";
				}
				if (!errorStruct && !m_ModelStruct)
				{
					configStruct?.Invoke();
				}
				else
				{
					m_StructStruct(text);
				}
			}
			finally
			{
				_ServiceStruct = true;
			}
		}

		void SelectTest()
		{
			((System.ComponentModel.Component)this).Dispose();
		}

		internal static bool InvokeGlobal()
		{
			return PushGlobal == null;
		}
	}

	[DefaultMember("Item")]
	internal readonly struct GetterDicBridge
	{
		private readonly string _TaskStruct;

		private readonly Dictionary<string, ResolverStruct> producerStruct;

		internal readonly bool m_MethodStruct;

		private static object PostGlobal;

		internal GetterDicBridge(string def)
		{
			_TaskStruct = def;
			MatchCollection matchCollection = Regex.Matches(def, "\"(.*?)\":(?:(?:\"(.*?)\")|(?:(.*?)[,}]))");
			int count = matchCollection.Count;
			if (count != 0)
			{
				m_MethodStruct = false;
				producerStruct = new Dictionary<string, ResolverStruct>();
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
						producerStruct[value] = new ResolverStruct(value2);
					}
				}
			}
			else
			{
				m_MethodStruct = true;
				producerStruct = null;
			}
		}

		[SpecialName]
		internal ResolverStruct StartTest(string reference)
		{
			producerStruct.TryGetValue(reference, out var value);
			return value;
		}

		public override string ToString()
		{
			return _TaskStruct;
		}

		public string DefineTest(bool overridedef)
		{
			if (overridedef)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("{");
				foreach (KeyValuePair<string, ResolverStruct> item in producerStruct)
				{
					stringBuilder.AppendLine($"{item.Key}: {item.Value},");
				}
				stringBuilder.Append("}");
				return stringBuilder.ToString();
			}
			return ToString();
		}

		internal static bool CallGlobal()
		{
			return PostGlobal == null;
		}
	}

	internal readonly struct ResolverStruct
	{
		internal readonly string m_IteratorStruct;

		internal readonly string _RulesStruct;

		internal readonly bool m_TokenizerStruct;

		internal readonly float specificationStruct;

		internal readonly bool _AccountStruct;

		internal static object InterruptGlobal;

		internal ResolverStruct(string def)
		{
			m_IteratorStruct = def;
			_AccountStruct = true;
			if (def.Length > 1)
			{
				if (!def.StartsWith("\"") || !def.EndsWith("\""))
				{
					_RulesStruct = def;
				}
				else
				{
					_RulesStruct = ((def.Length != 2) ? def.Substring(1, def.Length - 2) : string.Empty);
				}
			}
			else
			{
				_RulesStruct = def;
			}
			m_TokenizerStruct = _RulesStruct == "true";
			float.TryParse(_RulesStruct, out specificationStruct);
		}

		public override string ToString()
		{
			return _RulesStruct;
		}

		public static implicit operator string(ResolverStruct first)
		{
			return first._RulesStruct;
		}

		public static implicit operator bool(ResolverStruct param)
		{
			return param.m_TokenizerStruct;
		}

		public static implicit operator float(ResolverStruct first)
		{
			return first.specificationStruct;
		}

		internal static bool UpdateGlobal()
		{
			return InterruptGlobal == null;
		}
	}

	internal enum CustomLogType
	{
		Regular,
		Warning,
		Error
	}

	[Serializable]
	private class ManagerStruct
	{
		internal class MappingStruct : IDisposable
		{
			private readonly Action parserStruct;

			private readonly bool definitionStruct;

			private readonly EditorGUI.ChangeCheckScope m_InitializerStruct;

			internal static MappingStruct ComputeGlobal;

			[SpecialName]
			internal bool AddTest()
			{
				return m_InitializerStruct.changed;
			}

			public MappingStruct(Action def = null)
			{
				parserStruct = def;
				definitionStruct = ValidateTest();
				RestartTest(checkinfo: true);
				m_InitializerStruct = new EditorGUI.ChangeCheckScope();
			}

			public void Dispose()
			{
				bool changed = m_InitializerStruct.changed;
				m_InitializerStruct.Dispose();
				if (changed)
				{
					parserStruct?.Invoke();
					ForgotTest();
				}
				RestartTest(definitionStruct);
			}

			public static implicit operator bool(MappingStruct init)
			{
				return init.m_InitializerStruct.changed;
			}

			internal static bool InitGlobal()
			{
				return ComputeGlobal == null;
			}
		}

		internal class StubTemplateMapper : IDisposable
		{
			private readonly bool m_InfoStruct;

			internal static StubTemplateMapper EnableGlobal;

			public StubTemplateMapper()
			{
				m_InfoStruct = ValidateTest();
				RestartTest(checkinfo: true);
			}

			public void Dispose()
			{
				RestartTest(m_InfoStruct);
			}

			internal static bool ForgotGlobal()
			{
				return EnableGlobal == null;
			}
		}

		[Serializable]
		internal class ParamsTestStub : TokenConfigCollection
		{
			[SerializeField]
			private bool _value;

			internal readonly Action m_AuthenticationStruct;

			private static ParamsTestStub VisitGlobal;

			[SpecialName]
			internal bool PrepareService()
			{
				return _value;
			}

			[SpecialName]
			internal void ListService(bool useres)
			{
				if (_value != useres)
				{
					_value = useres;
					m_AuthenticationStruct?.Invoke();
					ForgotTest();
				}
			}

			internal ParamsTestStub(bool compareinit, Action cust = null)
			{
				m_PredicateStruct = (string)(object)compareinit;
				_value = compareinit;
				m_AuthenticationStruct = cust;
			}

			internal void InstantiateTest()
			{
				ListService(!_value);
			}

			internal void RevertTest(string spec, GUIStyle token = null, params GUILayoutOption[] options)
			{
				ReflectService(new GUIContent(spec), token, options);
			}

			internal void ReflectService(GUIContent config, GUIStyle ord = null, params GUILayoutOption[] options)
			{
				if (ord != null)
				{
					ListService(EditorGUILayout.Toggle(config, PrepareService(), ord, options));
				}
				else
				{
					ListService(EditorGUILayout.Toggle(config, PrepareService(), options));
				}
			}

			internal void CountService(string asset, string second = null, bool filterneeded = false, Color? res2 = null, Color? value3 = null, params GUILayoutOption[] options)
			{
				SetService((!string.IsNullOrEmpty(asset)) ? new GUIContent(asset) : GUIContent.none, (!string.IsNullOrEmpty(second)) ? new GUIContent(second) : GUIContent.none, filterneeded, res2, value3, options);
			}

			internal void SetService(GUIContent asset, GUIContent col = null, bool forcerole = false, Color? token2 = null, Color? info3 = null, params GUILayoutOption[] options)
			{
				token2 = token2 ?? GUI.backgroundColor;
				info3 = info3 ?? GUI.backgroundColor;
				Color backgroundColor = GUI.backgroundColor;
				GUI.backgroundColor = ((!PrepareService()) ? info3.Value : token2.Value);
				ListService(GUILayout.Toggle(PrepareService(), (!PrepareService() && col != null) ? col : asset, (!forcerole) ? GUI.skin.button : EditorStyles.toolbarButton, options));
				GUI.backgroundColor = backgroundColor;
			}

			public static implicit operator bool(ParamsTestStub res)
			{
				return res._value;
			}

			internal override void SetupDefinition()
			{
				ListService((bool)(object)m_PredicateStruct);
			}

			internal static bool DeleteGlobal()
			{
				return VisitGlobal == null;
			}
		}

		[Serializable]
		internal class ClientStruct : TokenConfigCollection
		{
			[SerializeField]
			private float _value;

			internal readonly Action m_WorkerStruct;

			private static ClientStruct RevertGlobal;

			[SpecialName]
			internal float GetService()
			{
				return _value;
			}

			[SpecialName]
			internal void VisitService(float var1)
			{
				if (_value != var1)
				{
					_value = var1;
					m_WorkerStruct?.Invoke();
					ForgotTest();
				}
			}

			internal ClientStruct(float last, Action selection = null)
			{
				m_PredicateStruct = (string)(object)last;
				_value = last;
				m_WorkerStruct = selection;
			}

			internal void ReadService(string res, bool setresult = true, GUIStyle template = null, params GUILayoutOption[] options)
			{
				VerifyService(new GUIContent(res), setresult, template, options);
			}

			internal void ResolveService(string i, float b, bool isserv = true, GUIStyle spec2 = null, params GUILayoutOption[] options)
			{
				EditorGUIUtility.labelWidth = b;
				VerifyService(new GUIContent(i), isserv, spec2, options);
				EditorGUIUtility.labelWidth = 0f;
			}

			internal void VerifyService(GUIContent asset, bool readresult = true, GUIStyle proc = null, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					VisitService((proc == null) ? EditorGUILayout.FloatField(asset, GetService(), options) : EditorGUILayout.FloatField(asset, GetService(), proc, options));
					if (readresult && GUILayout.Button(AdvisorDicBridge.PrepareRequest().adapterTemplate, AdvisorDicBridge.ManageRequest()._InterpreterTemplate, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						SetupDefinition();
					}
				}
			}

			internal void ConnectService(GUIContent instance, float connection, bool isdic = true, GUIStyle info2 = null, params GUILayoutOption[] options)
			{
				EditorGUIUtility.labelWidth = connection;
				VerifyService(instance, isdic, info2, options);
				EditorGUIUtility.labelWidth = 0f;
			}

			internal void CompareService(string first, float cont, float temp, bool iscounter2 = true, params GUILayoutOption[] options)
			{
				InterruptService(new GUIContent(first), cont, temp, iscounter2, options);
			}

			internal void InterruptService(GUIContent reference, float second, float consumer, bool isord2 = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					VisitService(EditorGUILayout.Slider(reference, GetService(), second, consumer, options));
					if (isord2 && GUILayout.Button(AdvisorDicBridge.PrepareRequest().adapterTemplate, AdvisorDicBridge.ManageRequest()._InterpreterTemplate, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						SetupDefinition();
					}
				}
			}

			internal void ComputeService(string instance, bool containsselection = true, params GUILayoutOption[] options)
			{
				StartService(new GUIContent(instance), containsselection, options);
			}

			internal void StartService(GUIContent param, bool requirescaller = true, params GUILayoutOption[] options)
			{
				InterruptService(param, 0f, 1f, requirescaller, options);
			}

			internal override void SetupDefinition()
			{
				VisitService((float)(object)m_PredicateStruct);
			}

			public static implicit operator int(ClientStruct param)
			{
				return (int)param._value;
			}

			public static implicit operator float(ClientStruct item)
			{
				return item._value;
			}

			internal static bool CalcWorker()
			{
				return RevertGlobal == null;
			}
		}

		[Serializable]
		internal class QueueStructPolicy : ClientStruct
		{
			private static QueueStructPolicy AwakeWorker;

			[SerializeField]
			internal int RegisterService
			{
				get
				{
					return (int)GetService();
				}
				set
				{
					VisitService(value);
				}
			}

			internal QueueStructPolicy(int ident_size, Action attr = null)
				: base(ident_size, attr)
			{
			}

			internal T InvokeService<T>() where T : Enum
			{
				return (T)(object)RegisterService;
			}

			internal void CustomizeService(GUIContent value, GUIStyle caller = null, params GUILayoutOption[] options)
			{
				RegisterService = ((caller == null) ? EditorGUILayout.IntField(value, RegisterService, options) : EditorGUILayout.IntField(value, RegisterService, caller, options));
			}

			internal void MoveService(string param, GUIStyle caller = null, params GUILayoutOption[] options)
			{
				CustomizeService(new GUIContent(param), caller, options);
			}

			internal void FillService<T>(GUIContent v, bool ispol = false, GUIStyle util = null, params GUILayoutOption[] options) where T : Enum
			{
				if (!ispol)
				{
					RegisterService = ((util != null) ? ((int)(object)EditorGUILayout.EnumPopup(v, (T)(object)RegisterService, util, options)) : ((int)(object)EditorGUILayout.EnumPopup(v, (T)(object)RegisterService, options)));
				}
				else
				{
					RegisterService = ((util != null) ? ((int)(object)EditorGUILayout.EnumFlagsField(v, (T)(object)RegisterService, util, options)) : ((int)(object)EditorGUILayout.EnumFlagsField(v, (T)(object)RegisterService, options)));
				}
			}

			internal void ChangeService<T>(string key, bool requiresb = false, GUIStyle res = null, params GUILayoutOption[] options) where T : Enum
			{
				FillService<T>(new GUIContent(key), requiresb, res, options);
			}

			internal static QueueStructPolicy CalculateService<T>(T v, Action pred = null) where T : Enum
			{
				return new QueueStructPolicy((int)(object)v, pred);
			}

			public static implicit operator int(QueueStructPolicy ident)
			{
				return ident.RegisterService;
			}

			public static implicit operator float(QueueStructPolicy last)
			{
				return last.RegisterService;
			}

			internal static bool SortWorker()
			{
				return AwakeWorker == null;
			}
		}

		[Serializable]
		internal class ContainerStruct : TokenConfigCollection
		{
			[SerializeField]
			private float _valueX;

			[SerializeField]
			private float _valueY;

			[SerializeField]
			private float _valueZ;

			internal Action _ExceptionStruct;

			internal bool propertyStruct;

			internal Vector3 _DescriptorStruct;

			internal static ContainerStruct NewWorker;

			[SpecialName]
			internal Vector3 ConcatService()
			{
				if (!propertyStruct)
				{
					propertyStruct = true;
					_DescriptorStruct = new Vector3(_valueX, _valueY, _valueZ);
				}
				return _DescriptorStruct;
			}

			[SpecialName]
			internal void LogoutService(Vector3 instance)
			{
				if (_DescriptorStruct != instance)
				{
					_DescriptorStruct = instance;
					_valueX = instance.x;
					_valueY = instance.y;
					_valueZ = instance.z;
					_ExceptionStruct?.Invoke();
					ForgotTest();
				}
			}

			internal void PatchService(Vector3 reference, Action result)
			{
				m_PredicateStruct = (string)(object)reference;
				_ExceptionStruct = result;
				_valueX = reference.x;
				_valueY = reference.y;
				_valueZ = reference.z;
			}

			internal ContainerStruct(Vector3 def, Action pred = null)
			{
				PatchService(def, pred);
			}

			internal ContainerStruct(float last, float visitor, float third, Action config2 = null)
			{
				PatchService(new Vector3(last, visitor, third), config2);
			}

			internal ContainerStruct(float info, float result, Action temp = null)
			{
				PatchService(new Vector3(info, result), temp);
			}

			internal void CalcService(GUIContent value, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label(value, GUILayout.MaxWidth(117f));
					LogoutService(EditorGUILayout.Vector2Field(GUIContent.none, ConcatService(), options));
					if (GUILayout.Button(AdvisorDicBridge.PrepareRequest().adapterTemplate, AdvisorDicBridge.ManageRequest()._InterpreterTemplate, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						SetupDefinition();
					}
				}
			}

			internal void MapService(string v, params GUILayoutOption[] options)
			{
				CalcService(new GUIContent(v), options);
			}

			internal void SortService(GUIContent reference, params GUILayoutOption[] options)
			{
				LogoutService(EditorGUILayout.Vector3Field(reference, ConcatService(), options));
			}

			internal void SetupService(string config, params GUILayoutOption[] options)
			{
				SortService(new GUIContent(config), options);
			}

			internal override void SetupDefinition()
			{
				LogoutService((Vector3)(object)m_PredicateStruct);
			}

			public static implicit operator Vector2(ContainerStruct first)
			{
				return first.ConcatService();
			}

			internal static bool CalculateWorker()
			{
				return NewWorker == null;
			}
		}

		[Serializable]
		internal class FactoryStruct : TokenConfigCollection
		{
			[SerializeField]
			private string _value;

			internal readonly Action m_TagStruct;

			internal static FactoryStruct TestWorker;

			[SpecialName]
			internal string PublishService()
			{
				return _value;
			}

			[SpecialName]
			internal void InstantiateService(string init)
			{
				if (_value != init)
				{
					_value = init;
					m_TagStruct?.Invoke();
					ForgotTest();
				}
			}

			internal FactoryStruct(string first = "", Action selection = null)
			{
				m_PredicateStruct = first;
				_value = first;
				m_TagStruct = selection;
			}

			internal override void SetupDefinition()
			{
				InstantiateService(m_PredicateStruct);
			}

			public override string ToString()
			{
				return PublishService();
			}

			public static implicit operator string(FactoryStruct v)
			{
				return v._value;
			}

			internal static bool LoginWorker()
			{
				return TestWorker == null;
			}
		}

		[Serializable]
		internal class InfoSpecificationProducer : TokenConfigCollection
		{
			internal readonly Action m_ConfigurationStruct;

			[SerializeField]
			private float r;

			[SerializeField]
			private float g;

			[SerializeField]
			private float b;

			[SerializeField]
			private float a;

			internal static InfoSpecificationProducer FindWorker;

			[SpecialName]
			internal Color DefineError()
			{
				return new Color(r, g, b, a);
			}

			[SpecialName]
			internal void PushError(Color config)
			{
				r = config.r;
				g = config.g;
				b = config.b;
				a = config.a;
				m_ConfigurationStruct?.Invoke();
				ForgotTest();
			}

			internal InfoSpecificationProducer(float info, float map, float proc, float connection2 = 1f, Action visitor3 = null)
			{
				Color color = new Color(info, map, proc, connection2);
				m_PredicateStruct = (string)(object)color;
				r = info;
				g = map;
				b = proc;
				a = connection2;
				m_ConfigurationStruct = visitor3;
			}

			internal InfoSpecificationProducer(Color first, Action ord = null)
			{
				m_PredicateStruct = (string)(object)first;
				r = first.r;
				g = first.g;
				b = first.b;
				a = first.a;
				m_ConfigurationStruct = ord;
			}

			internal void ReflectError(string reference, bool canselection = true, params GUILayoutOption[] options)
			{
				CountError(new GUIContent(reference), canselection, options);
			}

			internal void CountError(GUIContent asset, bool rejectmap = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					PushError(EditorGUILayout.ColorField(asset, DefineError(), options));
					if (rejectmap && GUILayout.Button(AdvisorDicBridge.PrepareRequest().adapterTemplate, AdvisorDicBridge.ManageRequest()._InterpreterTemplate, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						SetupDefinition();
					}
				}
			}

			internal override void SetupDefinition()
			{
				PushError((Color)(object)m_PredicateStruct);
			}

			internal static bool SetWorker()
			{
				return FindWorker == null;
			}
		}

		[Serializable]
		internal class ParamsStruct : TokenConfigCollection
		{
			internal readonly Action serializerStruct;

			private readonly Type _InterceptorStruct;

			[SerializeField]
			internal string guid;

			[SerializeField]
			internal long localID;

			private string _DatabaseStruct;

			private long _ValStruct;

			private bool m_MerchantStruct;

			private UnityEngine.Object m_ClassStruct;

			private static ParamsStruct ValidateWorker;

			[SpecialName]
			internal UnityEngine.Object ForgotError()
			{
				if (!m_MerchantStruct)
				{
					while (true)
					{
						m_MerchantStruct = true;
					}
				}
				return m_ClassStruct;
			}

			[SpecialName]
			internal void AssetError(UnityEngine.Object ident)
			{
				if (m_ClassStruct != ident)
				{
					m_ClassStruct = ident;
					if (!(ident == null))
					{
						AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ident, out guid, out localID);
					}
					else
					{
						guid = string.Empty;
						localID = 0L;
					}
					serializerStruct?.Invoke();
					ForgotTest();
				}
			}

			internal ParamsStruct(Type first, string ord = "", long offset_dic = 0L, Action first2 = null)
			{
				_InterceptorStruct = first;
				_DatabaseStruct = ord;
				_ValStruct = offset_dic;
				guid = ord;
				localID = offset_dic;
				serializerStruct = first2;
			}

			internal void InsertError(string param, bool ismap = true, params GUILayoutOption[] options)
			{
				PrepareError(new GUIContent(param), ismap, options);
			}

			internal void PrepareError(GUIContent config, bool isreg = true, params GUILayoutOption[] options)
			{
				using (new GUILayout.HorizontalScope())
				{
					AssetError(EditorGUILayout.ObjectField(config, ForgotError(), _InterceptorStruct, allowSceneObjects: false, options));
					if (isreg && GUILayout.Button(AdvisorDicBridge.PrepareRequest().adapterTemplate, AdvisorDicBridge.ManageRequest()._InterpreterTemplate, GUILayout.Width(18f), GUILayout.Height(18f)))
					{
						SetupDefinition();
					}
				}
			}

			private static T ListError<T>(string instance, long visitor_start) where T : UnityEngine.Object
			{
				if (!string.IsNullOrWhiteSpace(instance))
				{
					if (visitor_start != 0L)
					{
						UnityEngine.Object[] array = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(instance));
						foreach (UnityEngine.Object obj in array)
						{
							AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string _, out long localId);
							if (localId == visitor_start)
							{
								return (T)obj;
							}
						}
						return null;
					}
					return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(instance));
				}
				return null;
			}

			internal T ManageError<T>() where T : UnityEngine.Object
			{
				return (T)ForgotError();
			}

			internal override void SetupDefinition()
			{
				AssetError(ListError<UnityEngine.Object>(_DatabaseStruct, _ValStruct));
			}

			public static implicit operator bool(ParamsStruct init)
			{
				return init.ForgotError();
			}

			internal static bool InsertWorker()
			{
				return ValidateWorker == null;
			}
		}

		internal abstract class TokenConfigCollection
		{
			internal string m_PredicateStruct;

			internal static TokenConfigCollection FillWorker;

			internal abstract void SetupDefinition();

			internal static bool SelectWorker()
			{
				return FillWorker == null;
			}
		}

		[AttributeUsage(AttributeTargets.Field)]
		internal class StateConfigCollection : Attribute
		{
			internal static StateConfigCollection CollectWorker;

			internal static bool VerifyWorker()
			{
				return CollectWorker == null;
			}
		}

		private static bool _ParamStruct;

		private static bool eventStruct;

		private static bool m_TestsStruct;

		private static FieldInfo[] _RequestStruct;

		private static ManagerStruct _WrapperStruct;

		internal static Action _AlgoStruct;

		[SerializeField]
		internal FactoryStruct u_updateLink = new FactoryStruct();

		[SerializeField]
		internal FactoryStruct u_updateVersion = new FactoryStruct();

		[SerializeField]
		internal FactoryStruct u_updateMessage = new FactoryStruct();

		[SerializeField]
		internal FactoryStruct u_updateChangelog = new FactoryStruct();

		[SerializeField]
		internal FactoryStruct u_updateDay = new FactoryStruct();

		[SerializeField]
		internal FactoryStruct u_announcement = new FactoryStruct();

		[SerializeField]
		internal FactoryStruct u_announcementLink = new FactoryStruct();

		[SerializeField]
		internal FactoryStruct u_announcementLinkName = new FactoryStruct();

		[SerializeField]
		internal FactoryStruct u_announcementHiddenDate = new FactoryStruct();

		[SerializeField]
		internal ParamsTestStub u_updateHidden = new ParamsTestStub(compareinit: false);

		[SerializeField]
		internal ParamsTestStub u_announcementHidden = new ParamsTestStub(compareinit: false);

		[SerializeField]
		internal ParamsTestStub a_HasSucceededLastVerification = new ParamsTestStub(compareinit: false);

		[SerializeField]
		internal ParamsTestStub a_VerifyOnDisplay = new ParamsTestStub(compareinit: true);

		[SerializeField]
		internal ParamsTestStub a_VerifyOnProjectLoad = new ParamsTestStub(compareinit: false);

		[SerializeField]
		internal ParamsTestStub gizmosActive = new ParamsTestStub(compareinit: true, ComparatorMethodObject.CancelProducer);

		[SerializeField]
		internal ParamsTestStub globalGizmo = new ParamsTestStub(compareinit: true, ComparatorMethodObject.CancelProducer);

		[SerializeField]
		internal ParamsTestStub editorAnimatedFoldouts = new ParamsTestStub(compareinit: true);

		[SerializeField]
		internal ParamsTestStub onSceneNameLabels = new ParamsTestStub(compareinit: true);

		[SerializeField]
		internal ParamsTestStub onSceneToolSelection = new ParamsTestStub(compareinit: true);

		[SerializeField]
		internal ParamsTestStub onSceneToolSelectionAlwaysVisible = new ParamsTestStub(compareinit: true);

		[SerializeField]
		internal ParamsTestStub onSceneEditingOverlay = new ParamsTestStub(compareinit: true);

		[SerializeField]
		internal ParamsTestStub onSceneOverlayInterceptsClick = new ParamsTestStub(compareinit: true);

		[SerializeField]
		internal ParamsTestStub onSceneTooltip = new ParamsTestStub(compareinit: true);

		[SerializeField]
		internal ParamsTestStub ignoreSceneClicks = new ParamsTestStub(compareinit: true);

		[SerializeField]
		internal ParamsTestStub hideToolsDuringTesting = new ParamsTestStub(compareinit: true);

		[SerializeField]
		internal ParamsTestStub hasReadColliderTestingWarning = new ParamsTestStub(compareinit: false);

		[SerializeField]
		internal QueueStructPolicy toolSelectionOverlayAlignment = QueueStructPolicy.CalculateService(AdvisorDicBridge.PositionFlag.BottomLeft);

		[SerializeField]
		internal QueueStructPolicy toolOverlayAlignment = QueueStructPolicy.CalculateService(AdvisorDicBridge.PositionFlag.BottomRight);

		[SerializeField]
		internal ClientStruct gizmoBoneOpacity = new ClientStruct(0.5f, ComparatorMethodObject.CancelProducer);

		[SerializeField]
		internal ClientStruct gizmoLimitOpacity = new ClientStruct(0.5f, ComparatorMethodObject.CancelProducer);

		[SerializeField]
		internal ClientStruct handleSizeMultiplier = new ClientStruct(1f);

		[SerializeField]
		internal InfoSpecificationProducer labelColor = new InfoSpecificationProducer(1f, 1f, 1f);

		[SerializeField]
		internal InfoSpecificationProducer generalColor = new InfoSpecificationProducer(1f, 1f, 1f);

		[SerializeField]
		internal InfoSpecificationProducer activeColor = new InfoSpecificationProducer(0.56f, 0.94f, 0.47f);

		[SerializeField]
		internal InfoSpecificationProducer inactiveColor = new InfoSpecificationProducer(1f, 0f, 0.3765f);

		[SerializeField]
		internal InfoSpecificationProducer mixedColor = new InfoSpecificationProducer(1f, 0.65f, 0f);

		[SerializeField]
		internal InfoSpecificationProducer selectionColor = new InfoSpecificationProducer(1f, 0.65f, 0f);

		private static ManagerStruct AssetGlobal;

		[SpecialName]
		internal static bool ValidateTest()
		{
			return m_TestsStruct;
		}

		[SpecialName]
		internal static void RestartTest(bool checkinfo)
		{
			bool testsStruct = m_TestsStruct;
			m_TestsStruct = checkinfo;
			if (testsStruct && !m_TestsStruct && eventStruct)
			{
				ForgotTest();
			}
		}

		[SpecialName]
		internal static ManagerStruct SearchTest()
		{
			if (_WrapperStruct == null)
			{
				AssetTest();
			}
			return _WrapperStruct;
		}

		private ManagerStruct()
		{
			_RequestStruct = (from m in typeof(ManagerStruct).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
				where m.IsDefined(typeof(StateConfigCollection), inherit: false)
				select m).ToArray();
		}

		internal static void ForgotTest()
		{
			eventStruct = false;
			if (!m_TestsStruct)
			{
				if (_ParamStruct)
				{
					return;
				}
				StringBuilder stringBuilder = new StringBuilder("MAIN[" + JsonUtility.ToJson(SearchTest()) + "]\u200b\u200b\u200b");
				FieldInfo[] requestStruct = _RequestStruct;
				foreach (FieldInfo fieldInfo in requestStruct)
				{
					try
					{
						string text = EditorJsonUtility.ToJson(fieldInfo.GetValue(SearchTest()));
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
			else
			{
				eventStruct = true;
			}
		}

		private static void AssetTest()
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
				_WrapperStruct = JsonUtility.FromJson<ManagerStruct>(value);
			}
			if (_WrapperStruct == null)
			{
				_WrapperStruct = new ManagerStruct();
			}
			FieldInfo[] requestStruct = _RequestStruct;
			foreach (FieldInfo fieldInfo in requestStruct)
			{
				object obj = fieldInfo.GetValue(_WrapperStruct) ?? Activator.CreateInstance(fieldInfo.FieldType);
				if (dictionary.TryGetValue(fieldInfo.Name, out var value2))
				{
					EditorJsonUtility.FromJsonOverwrite(value2, obj);
				}
				fieldInfo.SetValue(_WrapperStruct, obj);
				if (fieldInfo.GetValue(_WrapperStruct) == null)
				{
					fieldInfo.SetValue(_WrapperStruct, Activator.CreateInstance(fieldInfo.FieldType));
				}
			}
		}

		internal static void TestTest()
		{
			if (EditorUtility.DisplayDialog("Clearing Settings", "Are you sure you want to clear the settings?", "Clear", "Cancel"))
			{
				ResetTest();
			}
		}

		internal static void ResetTest()
		{
			_WrapperStruct = new ManagerStruct();
			FieldInfo[] requestStruct = _RequestStruct;
			foreach (FieldInfo fieldInfo in requestStruct)
			{
				fieldInfo.SetValue(_WrapperStruct, Activator.CreateInstance(fieldInfo.FieldType));
			}
			_AlgoStruct?.Invoke();
			ForgotTest();
		}

		[SpecialName]
		internal Color[] OrderTest()
		{
			return new Color[3]
			{
				inactiveColor.DefineError(),
				activeColor.DefineError(),
				mixedColor.DefineError()
			};
		}

		internal static bool ConnectGlobal()
		{
			return AssetGlobal == null;
		}
	}

	private sealed class RoleStruct : Editor
	{
		private static readonly AnimBool[] m_RegistryStruct = new AnimBool[3]
		{
			new AnimBool(value: true),
			new AnimBool(),
			new AnimBool()
		};

		private static bool _StrategyStruct = true;

		private ReorderableList _IndexerStruct;

		private SerializedProperty broadcasterStruct;

		private SerializedProperty _PublisherStruct;

		private SerializedProperty _MapperStruct;

		private SerializedProperty productStruct;

		private SerializedProperty _SetterStruct;

		private SerializedProperty _ObjectStruct;

		private SerializedProperty _VisitorStruct;

		private SerializedProperty statusStruct;

		private SerializedProperty m_TokenStruct;

		private SerializedProperty stateStruct;

		private SerializedProperty helperStruct;

		private SerializedProperty m_PageStruct;

		private SerializedProperty m_PrototypeStruct;

		private static Type _CreatorStruct;

		private static Type m_BaseStruct;

		private static RoleStruct CloneWorker;

		public override void OnInspectorGUI()
		{
			if (QuerySystem())
			{
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
					return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
				})())
				{
					return;
				}
				base.serializedObject.Update();
				CustomizeError();
				CompareStruct("Shape", m_RegistryStruct[0], null, delegate
				{
					ReflectSystem(CreateError(), new SerializedProperty[6] { broadcasterStruct, _PublisherStruct, _MapperStruct, productStruct, _SetterStruct, _ObjectStruct }, InvokeError, testtask2: false);
				});
				CompareStruct("Receiver", m_RegistryStruct[1], null, delegate
				{
					FillSystem();
					EditorGUILayout.PropertyField(helperStruct);
					ChangeSystem(m_PageStruct);
					if (helperStruct.hasMultipleDifferentValues || helperStruct.enumValueIndex == 1)
					{
						EditorGUILayout.PropertyField(m_PrototypeStruct);
					}
					ContactReceiver contactReceiver = CreateError() as ContactReceiver;
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
				CompareStruct("Filtering", m_RegistryStruct[2], null, delegate
				{
					FillSystem();
					using (new GUILayout.HorizontalScope())
					{
						InvokeSystem(statusStruct, statusStruct.CompareManager(), null);
						InvokeSystem(m_TokenStruct, m_TokenStruct.CompareManager(), null);
						InvokeSystem(stateStruct, stateStruct.CompareManager(), null);
					}
					_IndexerStruct.DoLayoutList();
				});
				base.serializedObject.ApplyModifiedProperties();
				SearchSystem();
				UpdateStruct();
			}
			else if (!m_Invocation)
			{
				PostSystem(ChangeError);
			}
		}

		private void method_0()
		{
			CountSystem(CreateError(), base.targets, 2, Color.cyan);
		}

		private void AwakeError(Rect instance, int remove_POLAt, bool nofield, bool isitem2)
		{
			PrepareSystem(_VisitorStruct, instance, remove_POLAt);
		}

		private void InvokeError()
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
			UpdateSystem(flag3, flag2, flag);
		}

		private void CustomizeError()
		{
			_PublisherStruct = base.serializedObject.FindProperty("rootTransform");
			broadcasterStruct = base.serializedObject.FindProperty("shapeType");
			_MapperStruct = base.serializedObject.FindProperty("radius");
			productStruct = base.serializedObject.FindProperty("height");
			_SetterStruct = base.serializedObject.FindProperty("position");
			_ObjectStruct = base.serializedObject.FindProperty("rotation");
			_VisitorStruct = base.serializedObject.FindProperty("collisionTags");
			statusStruct = base.serializedObject.FindProperty("allowSelf");
			m_TokenStruct = base.serializedObject.FindProperty("allowOthers");
			stateStruct = base.serializedObject.FindProperty("localOnly");
			helperStruct = base.serializedObject.FindProperty("receiverType");
			m_PageStruct = base.serializedObject.FindProperty("parameter");
			m_PrototypeStruct = base.serializedObject.FindProperty("minVelocity");
			_IndexerStruct = new ReorderableList(base.serializedObject, _VisitorStruct, draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
			{
				drawElementCallback = AwakeError,
				drawHeaderCallback = ListSystem
			};
		}

		private void OnEnable()
		{
			CompareSystem(m_RegistryStruct, Repaint);
			ManageSystem(InvokeError);
		}

		private void OnDisable()
		{
			ResolveSystem(compareres: false);
		}

		[MenuItem("CONTEXT/VRCContactReceiver/ADOverhaul/To Sender", false, 897)]
		private static void MoveError(MenuCommand param)
		{
			if (ComputeSystem())
			{
				VRCContactReceiver obj = (VRCContactReceiver)param.context;
				obj.WriteParam(obj.gameObject);
				Undo.DestroyObjectImmediate(obj);
			}
		}

		[MenuItem("CONTEXT/VRCContactReceiver/ADOverhaul/To Collider", false, 898)]
		private static void FillError(MenuCommand init)
		{
			if (ComputeSystem())
			{
				VRCContactReceiver obj = (VRCContactReceiver)init.context;
				obj.InsertParam(obj.gameObject);
				Undo.DestroyObjectImmediate(obj);
			}
		}

		[MenuItem("CONTEXT/VRCContactReceiver/ADOverhaul/Toggle Editor", false, 899)]
		private static void ChangeError()
		{
			CalculateError(_StrategyStruct);
		}

		internal static void CalculateError(bool islast = false)
		{
			if (_CreatorStruct == null)
			{
				_CreatorStruct = AdvisorDicBridge.ResolveManager("VRCContactReceiver");
			}
			if (m_BaseStruct == null)
			{
				m_BaseStruct = AdvisorDicBridge.ResolveManager("VRCContactReceiverEditor");
			}
			_StrategyStruct = !islast;
			AdvisorDicBridge.RevertManager(_CreatorStruct, (!_StrategyStruct) ? m_BaseStruct : typeof(RoleStruct));
		}

		[CompilerGenerated]
		private void PopError()
		{
			ReflectSystem(CreateError(), new SerializedProperty[6] { broadcasterStruct, _PublisherStruct, _MapperStruct, productStruct, _SetterStruct, _ObjectStruct }, InvokeError, testtask2: false);
		}

		[CompilerGenerated]
		private void CallError()
		{
			FillSystem();
			EditorGUILayout.PropertyField(helperStruct);
			ChangeSystem(m_PageStruct);
			if (helperStruct.hasMultipleDifferentValues || helperStruct.enumValueIndex == 1)
			{
				EditorGUILayout.PropertyField(m_PrototypeStruct);
			}
			ContactReceiver contactReceiver = CreateError() as ContactReceiver;
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
		private void PostError()
		{
			FillSystem();
			using (new GUILayout.HorizontalScope())
			{
				InvokeSystem(statusStruct, statusStruct.CompareManager(), null);
				InvokeSystem(m_TokenStruct, m_TokenStruct.CompareManager(), null);
				InvokeSystem(stateStruct, stateStruct.CompareManager(), null);
			}
			_IndexerStruct.DoLayoutList();
		}

		UnityEngine.Object CreateError()
		{
			return base.target;
		}

		internal static bool ListWorker()
		{
			return (object)CloneWorker == null;
		}
	}

	private sealed class ConsumerStruct : Editor
	{
		private static readonly AnimBool[] _AttrStruct = new AnimBool[2]
		{
			new AnimBool(value: true),
			new AnimBool()
		};

		private static bool _RecordStruct = true;

		private ReorderableList _ItemStruct;

		private SerializedProperty _DecoratorStruct;

		private SerializedProperty invocationStruct;

		private SerializedProperty m_ExporterStruct;

		private SerializedProperty fieldStruct;

		private SerializedProperty m_CallbackStruct;

		private SerializedProperty filterStruct;

		private SerializedProperty m_ProxyStruct;

		private static Type _ComparatorStruct;

		private static Type _AdapterStruct;

		private static ConsumerStruct RestartWorker;

		public override void OnInspectorGUI()
		{
			if (QuerySystem())
			{
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
					return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
				})())
				{
					return;
				}
				base.serializedObject.Update();
				WriteTask();
				CompareStruct("Shape", _AttrStruct[0], null, delegate
				{
					ReflectSystem(VerifyTask(), new SerializedProperty[6] { _DecoratorStruct, invocationStruct, m_ExporterStruct, fieldStruct, m_CallbackStruct, filterStruct }, StopTask, testtask2: false);
				});
				CompareStruct("Filtering", _AttrStruct[1], null, delegate
				{
					FillSystem();
					using (new GUILayout.VerticalScope())
					{
						_ItemStruct.DoLayoutList();
					}
				});
				base.serializedObject.ApplyModifiedProperties();
				SearchSystem();
				UpdateStruct();
			}
			else if (!m_Invocation)
			{
				PostSystem(UpdateTask);
			}
		}

		private void method_0()
		{
			CountSystem(VerifyTask(), base.targets, 1, Color.yellow);
		}

		private void RunTask(Rect var1, int indexOf_col, bool containsfilter, bool outputasset2)
		{
			PrepareSystem(m_ProxyStruct, var1, indexOf_col);
		}

		private void StopTask()
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
				default:
					flag = true;
					break;
				case 0:
					flag3 = true;
					break;
				}
			}
			UpdateSystem(flag3, flag2, flag);
		}

		private void WriteTask()
		{
			invocationStruct = base.serializedObject.FindProperty("rootTransform");
			_DecoratorStruct = base.serializedObject.FindProperty("shapeType");
			m_ExporterStruct = base.serializedObject.FindProperty("radius");
			fieldStruct = base.serializedObject.FindProperty("height");
			m_CallbackStruct = base.serializedObject.FindProperty("position");
			filterStruct = base.serializedObject.FindProperty("rotation");
			m_ProxyStruct = base.serializedObject.FindProperty("collisionTags");
			_ItemStruct = new ReorderableList(base.serializedObject, m_ProxyStruct, draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: false)
			{
				drawElementCallback = RunTask,
				drawHeaderCallback = ListSystem
			};
		}

		private void OnEnable()
		{
			CompareSystem(_AttrStruct, Repaint);
			ManageSystem(StopTask);
		}

		private void OnDisable()
		{
			ResolveSystem(compareres: false);
		}

		[MenuItem("CONTEXT/VRCContactSender/ADOverhaul/To Receiver", false, 897)]
		private static void DefineTask(MenuCommand param)
		{
			if (ComputeSystem())
			{
				VRCContactSender obj = (VRCContactSender)param.context;
				obj.PushParam(obj.gameObject);
				Undo.DestroyObjectImmediate(obj);
			}
		}

		[MenuItem("CONTEXT/VRCContactSender/ADOverhaul/To Collider", false, 898)]
		private static void PushTask(MenuCommand asset)
		{
			if (ComputeSystem())
			{
				VRCContactSender obj = (VRCContactSender)asset.context;
				obj.PrepareParam(obj.gameObject);
				Undo.DestroyObjectImmediate(obj);
			}
		}

		[MenuItem("CONTEXT/VRCContactSender/ADOverhaul/Toggle Editor", false, 899)]
		private static void UpdateTask()
		{
			InsertTask(_RecordStruct);
		}

		internal static void InsertTask(bool isvalue = false)
		{
			if (_ComparatorStruct == null)
			{
				_ComparatorStruct = AdvisorDicBridge.ResolveManager("VRCContactSender");
			}
			if (_AdapterStruct == null)
			{
				_AdapterStruct = AdvisorDicBridge.ResolveManager("VRCContactSenderEditor");
			}
			_RecordStruct = !isvalue;
			AdvisorDicBridge.RevertManager(_ComparatorStruct, (!_RecordStruct) ? _AdapterStruct : typeof(ConsumerStruct));
		}

		[CompilerGenerated]
		private void PrepareTask()
		{
			ReflectSystem(VerifyTask(), new SerializedProperty[6] { _DecoratorStruct, invocationStruct, m_ExporterStruct, fieldStruct, m_CallbackStruct, filterStruct }, StopTask, testtask2: false);
		}

		[CompilerGenerated]
		private void ListTask()
		{
			FillSystem();
			using (new GUILayout.VerticalScope())
			{
				_ItemStruct.DoLayoutList();
			}
		}

		UnityEngine.Object VerifyTask()
		{
			return base.target;
		}

		internal static bool MoveWorker()
		{
			return (object)RestartWorker == null;
		}
	}

	private sealed class ErrorServiceClass : Editor
	{
		private static readonly AnimBool[] m_ComposerStruct = new AnimBool[1]
		{
			new AnimBool(value: true)
		};

		private static bool _CodeStruct = true;

		private SerializedProperty facadeStruct;

		private SerializedProperty _ProcessStruct;

		private SerializedProperty connectionStruct;

		private SerializedProperty _CustomerStruct;

		private SerializedProperty m_QueueStruct;

		private SerializedProperty annotationStruct;

		private SerializedProperty valueStruct;

		private SerializedProperty _RepositoryStruct;

		private static Type m_RefStruct;

		private static Type candidateStruct;

		internal static ErrorServiceClass DefineWorker;

		public override void OnInspectorGUI()
		{
			if (!QuerySystem())
			{
				if (!m_Invocation)
				{
					PostSystem(PopTask);
				}
				return;
			}
			base.serializedObject.Update();
			LoginTask();
			if (((Func<bool>)delegate
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
				return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
			})())
			{
				if (CalculateSystem(base.targets))
				{
					CallSystem();
				}
				CompareStruct("Shape", m_ComposerStruct[0], null, delegate
				{
					ReflectSystem(RegisterTask(), new SerializedProperty[8] { _ProcessStruct, facadeStruct, m_QueueStruct, annotationStruct, valueStruct, _RepositoryStruct, connectionStruct, _CustomerStruct }, PostTask, testtask2: true);
				});
				if (PopSystem(base.serializedObject, base.targets))
				{
					SceneView.RepaintAll();
					_Predicate = true;
				}
				SearchSystem();
				UpdateStruct();
				ListStruct();
			}
		}

		public void method_0()
		{
			CountSystem(RegisterTask(), base.targets, 0, Color.green);
		}

		[MenuItem("CONTEXT/VRCPhysBoneCollider/ADOverhaul/Move To Empty", false, 896)]
		private static void FillTask(MenuCommand asset)
		{
			if (!ComputeSystem())
			{
			}
		}

		[MenuItem("CONTEXT/VRCPhysBoneCollider/ADOverhaul/To Sender", false, 897)]
		private static void ChangeTask(MenuCommand task)
		{
			if (ComputeSystem())
			{
				VRCPhysBoneCollider obj = (VRCPhysBoneCollider)task.context;
				obj.DefineParam(obj.gameObject);
				Undo.DestroyObjectImmediate(obj);
			}
		}

		[MenuItem("CONTEXT/VRCPhysBoneCollider/ADOverhaul/To Receiver", false, 898)]
		private static void CalculateTask(MenuCommand var1)
		{
			if (ComputeSystem())
			{
				VRCPhysBoneCollider obj = (VRCPhysBoneCollider)var1.context;
				obj.UpdateParam(obj.gameObject);
				Undo.DestroyObjectImmediate(obj);
			}
		}

		[MenuItem("CONTEXT/VRCPhysBoneCollider/ADOverhaul/Toggle Editor", false, 899)]
		private static void PopTask()
		{
			CallTask(_CodeStruct);
		}

		internal static void CallTask(bool cankey = false)
		{
			if (m_RefStruct == null)
			{
				m_RefStruct = AdvisorDicBridge.ResolveManager("VRCPhysBoneCollider");
			}
			if (candidateStruct == null)
			{
				candidateStruct = AdvisorDicBridge.ResolveManager("VRCPhysBoneColliderEditor");
			}
			_CodeStruct = !cankey;
			AdvisorDicBridge.RevertManager(m_RefStruct, _CodeStruct ? typeof(ErrorServiceClass) : candidateStruct);
		}

		private void PostTask()
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
			UpdateSystem(flag3, flag2, flag);
		}

		private void LoginTask()
		{
			facadeStruct = base.serializedObject.FindProperty("rootTransform");
			_ProcessStruct = base.serializedObject.FindProperty("shapeType");
			connectionStruct = base.serializedObject.FindProperty("insideBounds");
			_CustomerStruct = base.serializedObject.FindProperty("bonesAsSpheres");
			m_QueueStruct = base.serializedObject.FindProperty("radius");
			annotationStruct = base.serializedObject.FindProperty("height");
			valueStruct = base.serializedObject.FindProperty("position");
			_RepositoryStruct = base.serializedObject.FindProperty("rotation");
		}

		private void OnEnable()
		{
			CompareSystem(m_ComposerStruct, Repaint);
			ManageSystem(PostTask);
		}

		public void OnDisable()
		{
			ResolveSystem(compareres: false);
		}

		[CompilerGenerated]
		private void RemoveTask()
		{
			ReflectSystem(RegisterTask(), new SerializedProperty[8] { _ProcessStruct, facadeStruct, m_QueueStruct, annotationStruct, valueStruct, _RepositoryStruct, connectionStruct, _CustomerStruct }, PostTask, testtask2: true);
		}

		UnityEngine.Object RegisterTask()
		{
			return base.target;
		}

		internal static bool ViewWorker()
		{
			return (object)DefineWorker == null;
		}
	}

	private sealed class ComparatorMethodObject : Editor
	{
		internal class ContainerModelDispatcher
		{
			internal readonly string _StrategyConfig;

			internal readonly SerializedProperty _IndexerConfig;

			internal readonly SerializedProperty m_BroadcasterConfig;

			internal readonly string _PublisherConfig;

			internal readonly string _MapperConfig;

			internal readonly bool productConfig;

			internal readonly float _SetterConfig;

			internal readonly float _ObjectConfig;

			internal readonly int visitorConfig;

			internal readonly bool _StatusConfig;

			private static ContainerModelDispatcher SetupWorker;

			internal ContainerModelDispatcher(SerializedProperty setup, SerializedProperty cont, float res = 0f, float item2 = 1f, int column_second3 = 0)
				: this(setup?.displayName, setup, cont, res, item2, column_second3)
			{
			}

			internal ContainerModelDispatcher(string last, SerializedProperty counter, SerializedProperty tag, float map2 = 0f, float ident3 = 1f, int int_0 = 0)
			{
				_StrategyConfig = last;
				_IndexerConfig = counter;
				m_BroadcasterConfig = tag;
				_StatusConfig = counter != null;
				_PublisherConfig = (_StatusConfig ? counter.propertyPath : string.Empty);
				productConfig = tag != null;
				_MapperConfig = ((!productConfig) ? string.Empty : tag.propertyPath);
				_SetterConfig = map2;
				_ObjectConfig = ident3;
				visitorConfig = int_0;
			}

			internal static bool QueryWorker()
			{
				return SetupWorker == null;
			}
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec tokenConfig = new _003C_003Ec();

			public static Func<bool> _StateConfig;

			public static Action m_HelperConfig;

			public static Action pageConfig;

			public static Action _PrototypeConfig;

			public static Action creatorConfig;

			public static Action _BaseConfig;

			public static Func<string, string> m_GetterConfig;

			public static Func<AdvisorDicBridge.PageDic, bool> _AdvisorConfig;

			public static Action m_ConsumerConfig;

			public static Action _AttrConfig;

			public static Action recordConfig;

			public static Action<VRCPhysBone> m_ItemConfig;

			public static Func<AdvisorDicBridge.StructTemplateExpression, bool> decoratorConfig;

			public static Func<Keyframe, float> m_InvocationConfig;

			public static Func<AdvisorDicBridge.PageDic, bool> _ExporterConfig;

			public static Func<VRCPhysBone, IEnumerable<Transform>> m_FieldConfig;

			internal static _003C_003Ec PushWorker;

			internal bool NewResolver()
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
				return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
			}

			internal void SelectResolver()
			{
				bool flag = policyStruct.enumValueIndex == 1;
				IncludeProducer(0);
				IncludeProducer(1, new GUIContent(flag ? "Momentum" : "Spring", _AttributeStruct.tooltip));
				if (flag)
				{
					IncludeProducer(2);
				}
				IncludeProducer(3);
				IncludeProducer(4);
				IncludeProducer(5);
				if (_ReponseStruct != null)
				{
					EditorGUILayout.PropertyField(_ReponseStruct);
				}
			}

			internal void RunResolver()
			{
				int enumValueIndex = errorConfig.enumValueIndex;
				EditorGUILayout.PropertyField(errorConfig, new GUIContent("Type"));
				if (enumValueIndex <= 0)
				{
					return;
				}
				IncludeProducer(7);
				if (enumValueIndex == 3)
				{
					IncludeProducer(8);
				}
				EditorGUILayout.PropertyField(_IteratorConfig);
				using (new GUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField("Limit Rotation Curves");
					AssetProducer(_AccountConfig, "X", updatedic: false);
					AssetProducer(_ManagerConfig, "Y", updatedic: false);
					AssetProducer(_ParamConfig, "Z", updatedic: false);
					if (AdvisorDicBridge.AwakeManager(AdvisorDicBridge.PrepareRequest().refTemplate, GUI.skin.label, GUILayout.Width(14f)))
					{
						SerializedProperty accountConfig = _AccountConfig;
						SerializedProperty managerConfig = _ManagerConfig;
						AnimationCurve animationCurve = (_ParamConfig.animationCurveValue = new AnimationCurve());
						AnimationCurve animationCurveValue = (managerConfig.animationCurveValue = animationCurve);
						accountConfig.animationCurveValue = animationCurveValue;
					}
				}
			}

			internal void StopResolver()
			{
				AwakeSystem(m_ConfigConfig, "Allow Collsion", null, GUILayout.ExpandWidth(expand: false));
			}

			internal void WriteResolver()
			{
				AwakeSystem(eventConfig, "Allow Grabbing", null, GUILayout.ExpandWidth(expand: false));
				AwakeSystem(m_RequestConfig, "Allow Posing", null, GUILayout.ExpandWidth(expand: false));
			}

			internal void DefineResolver()
			{
				RateProducer(13);
				RateProducer(14);
				if (globalStruct.enumValueIndex > 0)
				{
					RateProducer(12);
				}
			}

			internal string PushResolver(string s)
			{
				return s.Substring(0, s.LastIndexOf('_'));
			}

			internal bool UpdateResolver(AdvisorDicBridge.PageDic pbp2)
			{
				return pbp2._BaseDic;
			}

			internal void InsertResolver()
			{
				AwakeSystem(propertyConfig, "Show Gizmos", delegate
				{
					if ((bool)ManagerStruct.SearchTest().globalGizmo)
					{
						ManagerStruct.SearchTest().gizmosActive.ListService(propertyConfig.boolValue);
					}
				}, GUILayout.ExpandWidth(expand: false));
				bool flag;
				string text = default(string);
				if (flag = ManagerStruct.SearchTest().globalGizmo)
				{
					text = "Global Setting";
				}
				using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, flag, AdvisorDicBridge.m_InitializerTemplate, AdvisorDicBridge._AuthenticationTemplate))
				{
					using (new ManagerStruct.MappingStruct(CancelProducer))
					{
						ManagerStruct.SearchTest().globalGizmo.ListService(GUILayout.Toggle(flag, text, GUI.skin.button, GUILayout.ExpandWidth(expand: false)));
					}
				}
			}

			internal void PrepareResolver()
			{
				if ((bool)ManagerStruct.SearchTest().globalGizmo)
				{
					ManagerStruct.SearchTest().gizmosActive.ListService(propertyConfig.boolValue);
				}
			}

			internal void ListResolver()
			{
				if (!ManagerStruct.SearchTest().globalGizmo)
				{
					descriptorConfig.floatValue = EditorGUILayout.Slider("Bone Opacity", descriptorConfig.floatValue, 0f, 1f);
					m_FactoryConfig.floatValue = EditorGUILayout.Slider("Limit Opacitiy", m_FactoryConfig.floatValue, 0f, 1f);
				}
				else
				{
					ManagerStruct.SearchTest().gizmoBoneOpacity.VisitService(EditorGUILayout.Slider("Bone Opacity", ManagerStruct.SearchTest().gizmoBoneOpacity, 0f, 1f));
					ManagerStruct.SearchTest().gizmoLimitOpacity.VisitService(EditorGUILayout.Slider("Limit Opacitiy", ManagerStruct.SearchTest().gizmoLimitOpacity, 0f, 1f));
				}
			}

			internal void ManageResolver(VRCPhysBone pb)
			{
				pb.configHasUpdated = true;
			}

			internal bool ReadResolver(AdvisorDicBridge.StructTemplateExpression b)
			{
				if (b.statusDic)
				{
					return !b.visitorDic;
				}
				return false;
			}

			internal float ResolveResolver(Keyframe k)
			{
				return k.value;
			}

			internal bool VerifyResolver(AdvisorDicBridge.PageDic p)
			{
				return p._BaseDic;
			}

			internal IEnumerable<Transform> ConnectResolver(VRCPhysBone pb)
			{
				return pb.GetRootTransform().GetComponentsInChildren<Transform>();
			}

			internal static bool InvokeWorker()
			{
				return PushWorker == null;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass108_1
		{
			public VRCAvatarDescriptor.CustomAnimLayer proxyConfig;

			public UnityEditor.Animations.AnimatorController comparatorConfig;

			internal static _003C_003Ec__DisplayClass108_1 InterruptWorker;

			internal static bool UpdateWorker()
			{
				return InterruptWorker == null;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass108_2
		{
			public AdvisorDicBridge.PageDic adapterConfig;

			public string m_ThreadConfig;

			public _003C_003Ec__DisplayClass108_1 m_PrinterConfig;

			internal static _003C_003Ec__DisplayClass108_2 ReadWorker;

			internal bool GetIterator(UnityEngine.AnimatorControllerParameter p)
			{
				return p.name == m_ThreadConfig;
			}

			internal void VisitIterator()
			{
				m_PrinterConfig.comparatorConfig.LogoutAccount(m_ThreadConfig, adapterConfig._CreatorDic, 0f);
				StopStruct($"Added {m_ThreadConfig} to {m_PrinterConfig.proxyConfig.type} ({m_PrinterConfig.comparatorConfig.name})");
				ConnectSystem();
			}

			internal static bool PopWorker()
			{
				return ReadWorker == null;
			}
		}

		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003C_003Ec__DisplayClass116_0
		{
			public VRCPhysBone[] _ListenerConfig;

			public VRCPhysBone m_ObserverConfig;
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass120_0
		{
			public VRCPhysBone m_BridgeConfig;

			public VRCPhysBone[] m_UtilsConfig;

			public SerializedProperty m_IdentifierConfig;

			public SerializedProperty _GlobalConfig;

			public float m_PolicyConfig;

			public float dispatcherConfig;

			public SerializedObject m_CollectionConfig;

			public ContainerModelDispatcher m_ReaderConfig;

			private static _003C_003Ec__DisplayClass120_0 ValidateClass;

			internal void CountRules(AdvisorDicBridge.StructTemplateExpression b, float m)
			{
				if (m != 0f)
				{
					Matrix4x4 objectDic = b._ObjectDic;
					Vector4 column = objectDic.GetColumn(3);
					float tag = m_BridgeConfig.radius * m;
					EditorGUI.BeginChangeCheck();
					float num = AdvisorDicBridge.PublishManager(objectDic.rotation, column, tag, !m_BridgeConfig.showGizmos, ManagerStruct.SearchTest().handleSizeMultiplier);
					if (EditorGUI.EndChangeCheck())
					{
						float delta = num / m - m_BridgeConfig.radius;
						SetRules(b, delta);
					}
					AdvisorDicBridge.LogoutManager(tag.ToString("F2"), column);
				}
			}

			internal void SetRules(AdvisorDicBridge.StructTemplateExpression bone, float delta)
			{
				Event current = Event.current;
				bool alt = current.alt;
				if (m_UtilsConfig.Length == 1)
				{
					if (alt)
					{
						ReadProducer(delta, bone, m_IdentifierConfig, _GlobalConfig, m_PolicyConfig, dispatcherConfig);
						m_CollectionConfig.ApplyModifiedProperties();
					}
					else
					{
						DeleteRules(m_BridgeConfig, delta);
					}
				}
				else if (alt)
				{
					DeleteRules(m_BridgeConfig, delta);
				}
				else if (current.shift)
				{
					_003C_003Ec__DisplayClass120_1 _003C_003Ec__DisplayClass120_ = new _003C_003Ec__DisplayClass120_1
					{
						m_PoolConfig = DeleteRules(m_BridgeConfig, delta)
					};
					VRCPhysBone[] array = m_UtilsConfig;
					foreach (VRCPhysBone vRCPhysBone in array)
					{
						if (vRCPhysBone != m_BridgeConfig)
						{
							NewRules(vRCPhysBone, _003C_003Ec__DisplayClass120_.ManageRules);
						}
					}
				}
				else
				{
					VRCPhysBone[] array = m_UtilsConfig;
					foreach (VRCPhysBone targetPhysbone in array)
					{
						DeleteRules(targetPhysbone, delta);
					}
				}
			}

			internal float DeleteRules(VRCPhysBone targetPhysbone, float delta)
			{
				_003C_003Ec__DisplayClass120_2 _003C_003Ec__DisplayClass120_ = new _003C_003Ec__DisplayClass120_2
				{
					_IssuerConfig = this,
					_InterpreterConfig = delta,
					attributeConfig = 0f
				};
				NewRules(targetPhysbone, _003C_003Ec__DisplayClass120_.ResolveRules);
				return _003C_003Ec__DisplayClass120_.attributeConfig;
			}

			internal void NewRules(VRCPhysBone targetPhysbone, Action<SerializedProperty> action)
			{
				SerializedObject serializedObject = new SerializedObject(targetPhysbone);
				serializedObject.UpdateIfRequiredOrScript();
				SerializedProperty obj = serializedObject.FindProperty(m_ReaderConfig._PublisherConfig);
				action(obj);
				serializedObject.ApplyModifiedProperties();
			}

			internal static bool InsertClass()
			{
				return ValidateClass == null;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass120_1
		{
			public float m_PoolConfig;

			public Action<SerializedProperty> writerConfig;

			private static _003C_003Ec__DisplayClass120_1 FillClass;

			internal void ManageRules(SerializedProperty sp)
			{
				sp.floatValue = m_PoolConfig;
			}

			internal static bool SelectClass()
			{
				return FillClass == null;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass120_2
		{
			public float _InterpreterConfig;

			public float attributeConfig;

			public _003C_003Ec__DisplayClass120_0 _IssuerConfig;

			internal static _003C_003Ec__DisplayClass120_2 CollectClass;

			internal void ResolveRules(SerializedProperty sp)
			{
				sp.floatValue = Mathf.Clamp(sp.floatValue + _InterpreterConfig, _IssuerConfig.m_PolicyConfig, _IssuerConfig.dispatcherConfig);
				attributeConfig = sp.floatValue;
			}

			internal static bool VerifyClass()
			{
				return CollectClass == null;
			}
		}

		private static readonly AnimBool[] m_MockStruct = new AnimBool[8];

		private static object _InstanceStruct;

		private static bool listenerStruct = true;

		private static VRCPhysBone[] observerStruct;

		private static VRCPhysBone[] parameterStruct;

		private static VRCPhysBoneCollider[] _ImporterStruct;

		private static Transform[] m_RegStruct;

		private static byte[] m_MessageStruct;

		private static Editor m_BridgeStruct;

		private static readonly int m_UtilsStruct = GUIUtility.GetControlID("ADOToolSelectionDragControlID".GetHashCode(), FocusType.Passive);

		private static readonly AdvisorDicBridge.PublisherTemplate m_IdentifierStruct = new AdvisorDicBridge.PublisherTemplate();

		private static SerializedProperty globalStruct;

		private static SerializedProperty policyStruct;

		private static SerializedProperty dispatcherStruct;

		private static SerializedProperty m_CollectionStruct;

		private static SerializedProperty readerStruct;

		private static SerializedProperty _PoolStruct;

		private static SerializedProperty _WriterStruct;

		private static SerializedProperty interpreterStruct;

		private static SerializedProperty _AttributeStruct;

		private static SerializedProperty m_IssuerStruct;

		private static SerializedProperty m_WatcherStruct;

		private static SerializedProperty contextStruct;

		private static SerializedProperty _SchemaStruct;

		private static SerializedProperty _ReponseStruct;

		private static SerializedProperty m_ProcessorStruct;

		private static SerializedProperty _SingletonStruct;

		private static SerializedProperty procStruct;

		private static SerializedProperty m_SystemConfig;

		private static SerializedProperty m_StructConfig;

		private static SerializedProperty m_ConfigConfig;

		private static SerializedProperty _ModelConfig;

		private static SerializedProperty templateConfig;

		private static SerializedProperty m_DicConfig;

		private static SerializedProperty m_ServiceConfig;

		private static SerializedProperty errorConfig;

		private static SerializedProperty taskConfig;

		private static SerializedProperty m_ProducerConfig;

		private static SerializedProperty methodConfig;

		private static SerializedProperty m_ResolverConfig;

		private static SerializedProperty _IteratorConfig;

		private static SerializedProperty _RulesConfig;

		private static SerializedProperty _TokenizerConfig;

		private static SerializedProperty m_SpecificationConfig;

		private static SerializedProperty _AccountConfig;

		private static SerializedProperty _ManagerConfig;

		private static SerializedProperty _ParamConfig;

		private static SerializedProperty eventConfig;

		private static SerializedProperty m_TestsConfig;

		private static SerializedProperty m_RequestConfig;

		private static SerializedProperty _WrapperConfig;

		private static SerializedProperty _AlgoConfig;

		private static SerializedProperty _MappingConfig;

		private static SerializedProperty parserConfig;

		private static SerializedProperty definitionConfig;

		private static SerializedProperty initializerConfig;

		private static SerializedProperty m_InfoConfig;

		private static SerializedProperty m_AuthenticationConfig;

		private static SerializedProperty m_ClientConfig;

		private static SerializedProperty _WorkerConfig;

		private static SerializedProperty m_ContainerConfig;

		private static SerializedProperty exceptionConfig;

		private static SerializedProperty propertyConfig;

		private static SerializedProperty descriptorConfig;

		private static SerializedProperty m_FactoryConfig;

		private static ContainerModelDispatcher[] _TagConfig;

		private static GUIContent[] _ConfigurationConfig;

		private static int[] _ParamsConfig;

		private static Dictionary<int, int> m_SerializerConfig;

		private static Dictionary<int, int> m_InterceptorConfig;

		private static bool _DatabaseConfig;

		private static int valConfig = -1;

		private static readonly TemplateTemplate _MerchantConfig = new TemplateTemplate(7);

		private static readonly string[] classConfig = new string[7] { "None", "End Position Edit", "Ignore Selection", "Ignore Copy", "Collision Selection", "Collision Copy", "Property Edit" };

		private static int m_PredicateConfig = -1;

		private static Vector3 serverConfig;

		private static Type ruleConfig;

		private static Type roleConfig;

		private static bool registryConfig;

		internal static ComparatorMethodObject MapWorker;

		[SpecialName]
		private static ContainerModelDispatcher PrintMethod()
		{
			if (CollectMethod())
			{
				return _TagConfig[valConfig];
			}
			return null;
		}

		[SpecialName]
		private static bool CollectMethod()
		{
			if (valConfig < 0)
			{
				return false;
			}
			return _MerchantConfig.serviceTemplate == 6;
		}

		[SpecialName]
		private static bool RestartMethod()
		{
			return _MerchantConfig.serviceTemplate == 1;
		}

		[SpecialName]
		private static void ViewMethod(bool isreference)
		{
			_MerchantConfig.ResetAccount(1, isreference);
		}

		[SpecialName]
		private static bool QueryMethod()
		{
			return _MerchantConfig.serviceTemplate == 2;
		}

		[SpecialName]
		private static void OrderMethod(bool injecttask)
		{
			_MerchantConfig.ResetAccount(2, injecttask);
		}

		[SpecialName]
		private static bool ConcatMethod()
		{
			return _MerchantConfig.serviceTemplate == 3;
		}

		[SpecialName]
		private static void LogoutMethod(bool islast)
		{
			_MerchantConfig.ResetAccount(3, islast);
		}

		[SpecialName]
		private static bool AddMethod()
		{
			return _MerchantConfig.serviceTemplate == 4;
		}

		[SpecialName]
		private static void PublishMethod(bool isinstance)
		{
			_MerchantConfig.ResetAccount(4, isinstance);
		}

		[SpecialName]
		private static bool RevertMethod()
		{
			return _MerchantConfig.serviceTemplate == 5;
		}

		[SpecialName]
		private static void ReflectResolver(bool validateident)
		{
			_MerchantConfig.ResetAccount(5, validateident);
		}

		public override void OnInspectorGUI()
		{
			if (QuerySystem())
			{
				if (!((Func<bool>)delegate
				{
					using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
					return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
				})())
				{
					return;
				}
				ListProducer();
				base.serializedObject.Update();
				CheckProducer();
				CalculateSystem(observerStruct);
				EditorGUIUtility.labelWidth = 160f;
				int num = 0;
				AnimBool[] mockStruct = m_MockStruct;
				num = 1;
				CompareStruct("Transforms", mockStruct[0], null, delegate
				{
					using (new GUILayout.HorizontalScope())
					{
						EditorGUILayout.PropertyField(dispatcherStruct, new GUIContent("Root"));
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
					ViewMethod(TestSystem(readerStruct, RestartMethod()));
					EditorGUILayout.PropertyField(_PoolStruct);
					using (new GUILayout.VerticalScope("box"))
					{
						using (new GUILayout.HorizontalScope())
						{
							GUILayout.Space(12f);
							m_CollectionStruct.isExpanded = EditorGUILayout.Foldout(m_CollectionStruct.isExpanded, "Ignore Transforms", toggleOnLabelClick: true);
							GUILayout.FlexibleSpace();
							LogoutMethod(ResetSystem(ConcatMethod(), AdvisorDicBridge.PrepareRequest()._FacadeTemplate));
							EditorGUI.BeginChangeCheck();
							OrderMethod(ResetSystem(QueryMethod(), AdvisorDicBridge.PrepareRequest().codeTemplate));
							if (EditorGUI.EndChangeCheck())
							{
								StartProducer();
							}
						}
						if (m_CollectionStruct.isExpanded)
						{
							EditorGUI.indentLevel++;
							AdvisorDicBridge.CountManager<Transform>(m_CollectionStruct);
							EditorGUI.indentLevel--;
						}
					}
				});
				AnimBool[] mockStruct2 = m_MockStruct;
				num = 2;
				CompareStruct("Forces", mockStruct2[1], DisableProducer, delegate
				{
					bool flag = policyStruct.enumValueIndex == 1;
					IncludeProducer(0);
					IncludeProducer(1, new GUIContent(flag ? "Momentum" : "Spring", _AttributeStruct.tooltip));
					if (flag)
					{
						IncludeProducer(2);
					}
					IncludeProducer(3);
					IncludeProducer(4);
					IncludeProducer(5);
					if (_ReponseStruct != null)
					{
						EditorGUILayout.PropertyField(_ReponseStruct);
					}
				});
				CompareStruct("Limits", m_MockStruct[num++], null, delegate
				{
					int enumValueIndex = errorConfig.enumValueIndex;
					EditorGUILayout.PropertyField(errorConfig, new GUIContent("Type"));
					if (enumValueIndex <= 0)
					{
						return;
					}
					IncludeProducer(7);
					if (enumValueIndex == 3)
					{
						IncludeProducer(8);
					}
					EditorGUILayout.PropertyField(_IteratorConfig);
					using (new GUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("Limit Rotation Curves");
						AssetProducer(_AccountConfig, "X", updatedic: false);
						AssetProducer(_ManagerConfig, "Y", updatedic: false);
						AssetProducer(_ParamConfig, "Z", updatedic: false);
						if (AdvisorDicBridge.AwakeManager(AdvisorDicBridge.PrepareRequest().refTemplate, GUI.skin.label, GUILayout.Width(14f)))
						{
							SerializedProperty accountConfig = _AccountConfig;
							SerializedProperty managerConfig = _ManagerConfig;
							AnimationCurve animationCurve = (_ParamConfig.animationCurveValue = new AnimationCurve());
							AnimationCurve animationCurveValue = (managerConfig.animationCurveValue = animationCurve);
							accountConfig.animationCurveValue = animationCurveValue;
						}
					}
				});
				bool m_FilterConfig = _ModelConfig != null;
				Action state = null;
				if (!m_FilterConfig)
				{
					state = delegate
					{
						AwakeSystem(m_ConfigConfig, "Allow Collsion", null, GUILayout.ExpandWidth(expand: false));
					};
				}
				CompareStruct("Collisions", m_MockStruct[num++], state, delegate
				{
					IncludeProducer(6);
					if (m_FilterConfig)
					{
						MoveSystem(m_ConfigConfig, _ModelConfig);
					}
					using (new GUILayout.VerticalScope(GUI.skin.box))
					{
						using (new GUILayout.HorizontalScope())
						{
							GUILayout.Space(12f);
							m_ServiceConfig.isExpanded = EditorGUILayout.Foldout(m_ServiceConfig.isExpanded, "Colliders", toggleOnLabelClick: true);
							GUILayout.FlexibleSpace();
							ReflectResolver(ResetSystem(RevertMethod(), AdvisorDicBridge.PrepareRequest()._FacadeTemplate));
							EditorGUI.BeginChangeCheck();
							PublishMethod(ResetSystem(AddMethod(), AdvisorDicBridge.PrepareRequest().codeTemplate));
							if (EditorGUI.EndChangeCheck())
							{
								ComputeProducer();
							}
						}
						if (m_ServiceConfig.isExpanded)
						{
							EditorGUI.indentLevel++;
							AdvisorDicBridge.CountManager<VRCPhysBoneCollider>(m_ServiceConfig);
							EditorGUI.indentLevel--;
						}
					}
				});
				Action state2 = null;
				if (!m_FilterConfig)
				{
					state2 = delegate
					{
						AwakeSystem(eventConfig, "Allow Grabbing", null, GUILayout.ExpandWidth(expand: false));
						AwakeSystem(m_RequestConfig, "Allow Posing", null, GUILayout.ExpandWidth(expand: false));
					};
				}
				CompareStruct("Grab & Pose", m_MockStruct[num++], state2, delegate
				{
					if (m_FilterConfig)
					{
						MoveSystem(eventConfig, m_TestsConfig);
						MoveSystem(m_RequestConfig, _WrapperConfig);
					}
					EditorGUILayout.PropertyField(_AlgoConfig);
					EditorGUILayout.PropertyField(_MappingConfig);
				});
				CompareStruct("Stretch & Squish", m_MockStruct[num++], null, delegate
				{
					RateProducer(13);
					RateProducer(14);
					if (globalStruct.enumValueIndex > 0)
					{
						RateProducer(12);
					}
				});
				CompareStruct("Options", m_MockStruct[num++], null, delegate
				{
					EditorGUILayout.PropertyField(globalStruct);
					EditorGUILayout.PropertyField(_WorkerConfig);
					EditorGUILayout.PropertyField(exceptionConfig);
					FillSystem();
					using (new GUILayout.HorizontalScope())
					{
						if ((bool)(UnityEngine.Object)(object)m_Mapping)
						{
							List<string> list = new List<string>();
							string[] initializer = m_Initializer;
							foreach (string text in initializer)
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
							string[] c = list.Select(_003C_003Ec.tokenConfig.PushResolver).Distinct().ToArray();
							string stringValue = m_ContainerConfig.stringValue;
							using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
							{
								stringValue = AdvisorDicBridge.CloneManager("Parameter", stringValue, c);
								if (changeCheckScope.changed)
								{
									m_ContainerConfig.stringValue = stringValue;
								}
							}
							using (new EditorGUI.DisabledScope((UnityEngine.Object)(object)m_Mapping == null || string.IsNullOrEmpty(m_ContainerConfig.stringValue)))
							{
								if (AdvisorDicBridge.RateManager(AdvisorDicBridge.PrepareRequest()._ConnectionTemplate))
								{
									GenericMenu genericMenu = new GenericMenu();
									using (IEnumerator<VRCAvatarDescriptor.CustomAnimLayer> enumerator = m_Mapping.baseAnimationLayers.Concat(m_Mapping.specialAnimationLayers).GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											_003C_003Ec__DisplayClass108_1 _003C_003Ec__DisplayClass108_ = new _003C_003Ec__DisplayClass108_1();
											_003C_003Ec__DisplayClass108_.proxyConfig = enumerator.Current;
											_003C_003Ec__DisplayClass108_.comparatorConfig = _003C_003Ec__DisplayClass108_.proxyConfig.animatorController as UnityEditor.Animations.AnimatorController;
											if (!(_003C_003Ec__DisplayClass108_.comparatorConfig == null))
											{
												UnityEngine.AnimatorControllerParameter[] parameters = _003C_003Ec__DisplayClass108_.comparatorConfig.parameters;
												AdvisorDicBridge.PageDic[] broadcasterTemplate = AdvisorDicBridge._BroadcasterTemplate;
												for (int i = 0; i < broadcasterTemplate.Length; i++)
												{
													_003C_003Ec__DisplayClass108_2 _003C_003Ec__DisplayClass108_2 = new _003C_003Ec__DisplayClass108_2();
													_003C_003Ec__DisplayClass108_2.m_PrinterConfig = _003C_003Ec__DisplayClass108_;
													_003C_003Ec__DisplayClass108_2.adapterConfig = broadcasterTemplate[i];
													_003C_003Ec__DisplayClass108_2.m_ThreadConfig = m_ContainerConfig.stringValue + _003C_003Ec__DisplayClass108_2.adapterConfig._PrototypeDic;
													if (!parameters.Any(_003C_003Ec__DisplayClass108_2.GetIterator))
													{
														genericMenu.AddItem(new GUIContent($"{_003C_003Ec__DisplayClass108_2.m_PrinterConfig.proxyConfig.type}/{_003C_003Ec__DisplayClass108_2.m_ThreadConfig}"), on: false, _003C_003Ec__DisplayClass108_2.VisitIterator);
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
							EditorGUILayout.PropertyField(m_ContainerConfig);
						}
					}
					VRCPhysBone vRCPhysBone = base.target as VRCPhysBone;
					if (vRCPhysBone != null && Application.isPlaying && !base.serializedObject.isEditingMultipleObjects && !string.IsNullOrEmpty(vRCPhysBone.parameter))
					{
						using (new EditorGUILayout.HorizontalScope())
						{
							SerializerTestStub.ResetMapping(_InstanceStruct, null);
							foreach (AdvisorDicBridge.PageDic item in AdvisorDicBridge._BroadcasterTemplate.Where(_003C_003Ec.tokenConfig.UpdateResolver))
							{
								using (new EditorGUILayout.VerticalScope())
								{
									GUILayout.Label(item._PrototypeDic, EditorStyles.boldLabel, GUILayout.ExpandWidth(expand: true));
									SerializerTestStub.ChangeMapping();
									GUILayout.Label(item.ChangeAlgo(vRCPhysBone));
								}
								SerializerTestStub.MoveMapping();
							}
							SerializerTestStub.AwakeMapping();
						}
					}
				});
				CompareStruct("Gizmos", m_MockStruct[num++], delegate
				{
					AwakeSystem(propertyConfig, "Show Gizmos", delegate
					{
						if ((bool)ManagerStruct.SearchTest().globalGizmo)
						{
							ManagerStruct.SearchTest().gizmosActive.ListService(propertyConfig.boolValue);
						}
					}, GUILayout.ExpandWidth(expand: false));
					bool flag;
					string text = default(string);
					if (flag = ManagerStruct.SearchTest().globalGizmo)
					{
						text = "Global Setting";
					}
					using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, flag, AdvisorDicBridge.m_InitializerTemplate, AdvisorDicBridge._AuthenticationTemplate))
					{
						using (new ManagerStruct.MappingStruct(CancelProducer))
						{
							ManagerStruct.SearchTest().globalGizmo.ListService(GUILayout.Toggle(flag, text, GUI.skin.button, GUILayout.ExpandWidth(expand: false)));
						}
					}
				}, delegate
				{
					if (!ManagerStruct.SearchTest().globalGizmo)
					{
						descriptorConfig.floatValue = EditorGUILayout.Slider("Bone Opacity", descriptorConfig.floatValue, 0f, 1f);
						m_FactoryConfig.floatValue = EditorGUILayout.Slider("Limit Opacitiy", m_FactoryConfig.floatValue, 0f, 1f);
					}
					else
					{
						ManagerStruct.SearchTest().gizmoBoneOpacity.VisitService(EditorGUILayout.Slider("Bone Opacity", ManagerStruct.SearchTest().gizmoBoneOpacity, 0f, 1f));
						ManagerStruct.SearchTest().gizmoLimitOpacity.VisitService(EditorGUILayout.Slider("Limit Opacitiy", ManagerStruct.SearchTest().gizmoLimitOpacity, 0f, 1f));
					}
				});
				PopSystem(base.serializedObject, observerStruct, delegate(VRCPhysBone pb)
				{
					pb.configHasUpdated = true;
				});
				SearchSystem();
				UpdateStruct();
			}
			else if (!m_Invocation)
			{
				PostSystem(CompareProducer);
			}
		}

		private void method_0()
		{
			if (_MerchantConfig.serviceTemplate < 0)
			{
				return;
			}
			VRCPhysBone vRCPhysBone = (VRCPhysBone)CustomizeProducer();
			if (!(vRCPhysBone == null))
			{
				Tools.hidden = true;
				AdvisorDicBridge.ObjectTokenizerResolver objectTokenizerResolver = new AdvisorDicBridge.ObjectTokenizerResolver(vRCPhysBone);
				objectTokenizerResolver.ResolveAlgo();
				if (RestartMethod())
				{
					PrepareProducer(observerStruct, objectTokenizerResolver);
				}
				if (CollectMethod())
				{
					ResolveProducer(observerStruct, objectTokenizerResolver, PrintMethod());
				}
			}
		}

		private static void DefineProducer(SceneView spec)
		{
			AdvisorDicBridge.StartManager();
			ListProducer();
			PushProducer(spec);
			if (AddMethod())
			{
				bool flag = ManagerStruct.SearchTest().onSceneNameLabels;
				using (new TaskServiceClass(TaskServiceClass.ColoringType.FG, flag, ManagerStruct.SearchTest().labelColor.DefineError()))
				{
					for (int i = 0; i < _ImporterStruct.Length; i++)
					{
						int _ComposerConfig = i;
						VRCPhysBoneCollider codeConfig = _ImporterStruct[_ComposerConfig];
						AdvisorDicBridge.ServiceDic setup = AdvisorDicBridge.ServiceDic.CountWrapper(codeConfig.transform.TransformPoint(codeConfig.position), (!flag) ? string.Empty : codeConfig.name, (float)ManagerStruct.SearchTest().handleSizeMultiplier * 0.05f, algo + i, delegate
						{
							m_ServiceConfig.RunManager<VRCPhysBoneCollider>(AdvisorDicBridge.ReadParam(m_MessageStruct, _ComposerConfig), codeConfig);
						});
						setup.managerDic = delegate(AdvisorDicBridge.ServiceDic sc2)
						{
							Handles.color = ManagerStruct.SearchTest().OrderTest()[m_MessageStruct[_ComposerConfig]];
							AdvisorDicBridge.ServiceDic.NewWrapper(sc2);
						};
						AdvisorDicBridge.EnableManager(setup);
					}
				}
			}
			if (QueryMethod())
			{
				bool flag2 = ManagerStruct.SearchTest().onSceneNameLabels;
				using (new TaskServiceClass(TaskServiceClass.ColoringType.FG, flag2, ManagerStruct.SearchTest().labelColor.DefineError()))
				{
					for (int num = 0; num < m_RegStruct.Length; num++)
					{
						Transform processConfig = m_RegStruct[num];
						int m_FacadeConfig = num;
						AdvisorDicBridge.ServiceDic setup2 = AdvisorDicBridge.ServiceDic.CountWrapper(processConfig.position, (!flag2) ? string.Empty : processConfig.name, (float)ManagerStruct.SearchTest().handleSizeMultiplier * 0.25f, algo + num, delegate
						{
							m_CollectionStruct.RunManager<Transform>(AdvisorDicBridge.ReadParam(m_MessageStruct, m_FacadeConfig), processConfig);
						});
						setup2.managerDic = delegate(AdvisorDicBridge.ServiceDic sc2)
						{
							Handles.color = ManagerStruct.SearchTest().OrderTest()[m_MessageStruct[m_FacadeConfig]];
							AdvisorDicBridge.ServiceDic.NewWrapper(sc2);
						};
						AdvisorDicBridge.EnableManager(setup2);
					}
				}
			}
			if (RevertMethod())
			{
				bool flag3 = ManagerStruct.SearchTest().onSceneNameLabels;
				Handles.color = ManagerStruct.SearchTest().selectionColor.DefineError();
				using (new TaskServiceClass(TaskServiceClass.ColoringType.FG, flag3, ManagerStruct.SearchTest().labelColor.DefineError()))
				{
					for (int num2 = 0; num2 < parameterStruct.Length; num2++)
					{
						VRCPhysBone vRCPhysBone = parameterStruct[num2];
						int m_ConnectionConfig = num2;
						AdvisorDicBridge.EnableManager(AdvisorDicBridge.ServiceDic.CountWrapper(vRCPhysBone.transform.position, (!flag3) ? string.Empty : vRCPhysBone.name, (float)ManagerStruct.SearchTest().handleSizeMultiplier * 0.25f, algo + num2, delegate
						{
							VRCPhysBone[] array = observerStruct;
							for (int j = 0; j < array.Length; j++)
							{
								array[j].colliders = parameterStruct[m_ConnectionConfig].colliders.ToList();
							}
							ReflectResolver(validateident: false);
							if (m_BridgeStruct != null)
							{
								m_BridgeStruct.Repaint();
							}
						}));
					}
				}
			}
			if (ConcatMethod())
			{
				bool flag4 = ManagerStruct.SearchTest().onSceneNameLabels;
				Handles.color = ManagerStruct.SearchTest().selectionColor.DefineError();
				using (new TaskServiceClass(TaskServiceClass.ColoringType.FG, flag4, ManagerStruct.SearchTest().labelColor.DefineError()))
				{
					for (int num3 = 0; num3 < parameterStruct.Length; num3++)
					{
						VRCPhysBone vRCPhysBone2 = parameterStruct[num3];
						int m_CustomerConfig = num3;
						AdvisorDicBridge.EnableManager(AdvisorDicBridge.ServiceDic.CountWrapper(vRCPhysBone2.transform.position, flag4 ? vRCPhysBone2.name : string.Empty, (float)ManagerStruct.SearchTest().handleSizeMultiplier * 0.25f, algo + num3, delegate
						{
							VRCPhysBone[] array = observerStruct;
							for (int j = 0; j < array.Length; j++)
							{
								array[j].ignoreTransforms = parameterStruct[m_CustomerConfig].ignoreTransforms.ToList();
							}
							LogoutMethod(islast: false);
							if (m_BridgeStruct != null)
							{
								m_BridgeStruct.Repaint();
							}
						}));
					}
				}
			}
			Event current = Event.current;
			if (Tools.current != Tool.View && !current.alt && (bool)ManagerStruct.SearchTest().ignoreSceneClicks && _MerchantConfig.serviceTemplate > 0 && current.type == EventType.MouseDown && current.button == 0)
			{
				GUIUtility.hotControl = algo - 1;
				current.Use();
			}
			AdvisorDicBridge.CheckManager();
		}

		private static void PushProducer(SceneView first)
		{
			Rect ivk = first.ExcludeManager();
			int num = _MerchantConfig.serviceTemplate;
			if (num < 0)
			{
				num = 0;
			}
			bool flag = num > 0;
			if ((bool)ManagerStruct.SearchTest().onSceneToolSelection && (flag || (bool)ManagerStruct.SearchTest().onSceneToolSelectionAlwaysVisible))
			{
				AdvisorDicBridge.PositionFlag positionFlag = ManagerStruct.SearchTest().toolSelectionOverlayAlignment.InvokeService<AdvisorDicBridge.PositionFlag>();
				bool flag2;
				using (new AdvisorDicBridge.AdvisorTemplate(first, 250f, 34f, positionFlag, m_IdentifierStruct))
				{
					Rect lastRect;
					using (new GUILayout.HorizontalScope())
					{
						using (new EditorGUI.DisabledScope(_MerchantConfig.serviceTemplate <= 0))
						{
							if (AdvisorDicBridge.RateManager(ManagerStruct.SearchTest().ignoreSceneClicks ? AdvisorDicBridge.PrepareRequest().m_ValueTemplate : AdvisorDicBridge.PrepareRequest()._AnnotationTemplate))
							{
								ManagerStruct.SearchTest().ignoreSceneClicks.InstantiateTest();
							}
						}
						GUILayout.FlexibleSpace();
						GUILayout.Label("ADO Tool:", AdvisorDicBridge.ManageRequest()._WriterTemplate);
						lastRect = GUILayoutUtility.GetLastRect();
						GUIContent content = new GUIContent(classConfig[num]);
						float x = GUI.skin.label.CalcSize(content).x;
						EditorGUI.BeginChangeCheck();
						int serviceTemplate = EditorGUILayout.Popup(GUIContent.none, num, classConfig, GUILayout.Width(x + 20f));
						if (EditorGUI.EndChangeCheck())
						{
							_MerchantConfig.serviceTemplate = serviceTemplate;
							if (_MerchantConfig.serviceTemplate != 0)
							{
								if (_MerchantConfig.serviceTemplate != 6)
								{
									VerifyProducer(-1);
									if (_MerchantConfig.serviceTemplate > 1 && _MerchantConfig.serviceTemplate < 4)
									{
										StartProducer();
									}
									else
									{
										ComputeProducer();
									}
								}
								else
								{
									VerifyProducer(0);
								}
							}
							else
							{
								ConnectProducer();
							}
							SceneView.RepaintAll();
						}
						GUILayout.FlexibleSpace();
						if (AdvisorDicBridge.RateManager(AdvisorDicBridge.PrepareRequest().m_ComposerTemplate))
						{
							Queue.PublishTemplate();
						}
					}
					flag2 = AdvisorDicBridge.DestroyManager(lastRect, m_UtilsStruct);
					AdvisorDicBridge.CallManager(lastRect, MouseCursor.Pan);
				}
				if (flag2)
				{
					Handles.BeginGUI();
					ManagerStruct.SearchTest().toolSelectionOverlayAlignment.RegisterService = (int)AdvisorDicBridge.ReflectManager(positionFlag, ivk);
					Handles.EndGUI();
				}
			}
			if (!flag || UpdateProducer(first))
			{
				InsertProducer(first);
			}
		}

		private static bool UpdateProducer(SceneView item)
		{
			bool drawGizmos = item.drawGizmos;
			if (!drawGizmos)
			{
				InterruptStruct(item, "Gizmos Disabled", delegate
				{
					GUILayout.Label("Handles are hidden.", AdvisorDicBridge.ManageRequest()._WriterTemplate);
					if (AdvisorDicBridge.GetManager("Enable Gizmos"))
					{
						item.drawGizmos = true;
					}
				}, 200f, 80f);
			}
			return drawGizmos;
		}

		private static void InsertProducer(SceneView i)
		{
			if (!ManagerStruct.SearchTest().onSceneEditingOverlay || _MerchantConfig.serviceTemplate <= 0)
			{
				return;
			}
			bool m_RefConfig = CollectMethod();
			bool m_MockConfig = ManagerStruct.SearchTest().onSceneTooltip;
			if (!m_RefConfig && !m_MockConfig)
			{
				return;
			}
			bool m_AnnotationConfig = observerStruct.Length > 1;
			bool flag = QueryMethod();
			bool flag2 = AddMethod();
			bool flag3 = ConcatMethod();
			bool flag4 = RevertMethod();
			bool instanceConfig = RestartMethod();
			bool candidateConfig = flag || flag3;
			bool _ExpressionConfig = flag2 || flag4;
			bool m_RepositoryConfig = flag3 || flag4;
			bool _ValueConfig = instanceConfig || m_RefConfig;
			float connection = ((!m_MockConfig) ? 33 : ((!m_RefConfig && !instanceConfig) ? 60 : (m_AnnotationConfig ? 100 : ((!m_RefConfig) ? 60 : 80))));
			float second = ((!m_AnnotationConfig) ? 240 : 340);
			Rect _StubConfig;
			ComputeStruct(i, delegate
			{
				using (new GUILayout.HorizontalScope())
				{
					string text = string.Concat(((!m_AnnotationConfig) ? "" : "Multi-") + (_ValueConfig ? "Editing" : (m_RepositoryConfig ? "Copying" : "Selecting")), m_RefConfig ? ":" : (candidateConfig ? " Ignore Transforms" : ((!_ExpressionConfig) ? " End Position" : " Colliders")));
					AdvisorDicBridge.CreateManager();
					GUILayout.FlexibleSpace();
					GUILayout.Label(text, AdvisorDicBridge.ManageRequest()._WriterTemplate);
					_StubConfig = GUILayoutUtility.GetLastRect();
					if (m_RefConfig)
					{
						EditorGUI.BeginChangeCheck();
						GUIContent content = _ConfigurationConfig[valConfig];
						float x = GUI.skin.label.CalcSize(content).x;
						int key = EditorGUILayout.IntPopup(GUIContent.none, m_InterceptorConfig[valConfig], _ConfigurationConfig, _ParamsConfig, GUILayout.Width(x + 20f));
						if (EditorGUI.EndChangeCheck())
						{
							valConfig = m_SerializerConfig[key];
							SceneView.RepaintAll();
						}
					}
					GUILayout.FlexibleSpace();
					StartStruct();
					return _StubConfig;
				}
			}, delegate
			{
				if (m_MockConfig)
				{
					GUILayout.Label("Press Enter or Escape to exit", AdvisorDicBridge.ManageRequest()._WriterTemplate);
					if (m_RefConfig || instanceConfig)
					{
						if (!m_AnnotationConfig)
						{
							if (m_RefConfig)
							{
								GUILayout.Label("Hold Alt to edit the curve", AdvisorDicBridge.ManageRequest()._WriterTemplate);
							}
						}
						else
						{
							GUILayout.Label("Hold Alt to edit the target physbone only", AdvisorDicBridge.ManageRequest()._WriterTemplate);
							GUILayout.Label("Hold Shift to set the physbones to the same value", AdvisorDicBridge.ManageRequest()._WriterTemplate);
						}
					}
				}
			}, second, connection);
		}

		private void PrepareProducer(VRCPhysBone[] first, AdvisorDicBridge.ObjectTokenizerResolver pred)
		{
			_003C_003Ec__DisplayClass116_0 cfg = default(_003C_003Ec__DisplayClass116_0);
			cfg._ListenerConfig = first;
			cfg.m_ObserverConfig = pred._ServerDic;
			AdvisorDicBridge.StructTemplateExpression[] array = pred._RoleDic.Where((AdvisorDicBridge.StructTemplateExpression b) => b.statusDic && !b.visitorDic).ToArray();
			foreach (AdvisorDicBridge.StructTemplateExpression structTemplateExpression in array)
			{
				Transform setterDic = structTemplateExpression.m_SetterDic;
				Vector3 vector = setterDic.TransformPoint(cfg.m_ObserverConfig.endpointPosition);
				if (!cfg.m_ObserverConfig.showGizmos || !(cfg.m_ObserverConfig.boneOpacity >= 0.05f))
				{
					Handles.DrawLine(setterDic.position, vector);
				}
				Quaternion rotation = ((Tools.pivotRotation != PivotRotation.Global) ? setterDic.rotation : Quaternion.identity);
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
				if (hotControl != m_PredicateConfig)
				{
					m_PredicateConfig = -1;
					direction = vector - setterDic.position;
					if (direction.magnitude < 0.01f)
					{
						direction = ((structTemplateExpression.m_HelperDic == null) ? (-setterDic.forward) : (vector - structTemplateExpression.m_HelperDic.m_SetterDic.position));
					}
				}
				else
				{
					direction = serverConfig;
				}
				Handles.color = AdvisorDicBridge._AuthenticationTemplate;
				EditorGUI.BeginChangeCheck();
				Vector3 vector4 = Handles.Slider(vector, direction);
				if (EditorGUI.EndChangeCheck())
				{
					if (hotControl != m_PredicateConfig)
					{
						m_PredicateConfig = hotControl;
						serverConfig = direction;
					}
					vector2 = vector4;
					flag = true;
				}
				if (flag)
				{
					TestProducer(setterDic.InverseTransformVector(vector2 - vector), ref cfg);
				}
			}
		}

		private static void ListProducer()
		{
			Event current = Event.current;
			if (current.type == EventType.Used || current.type != EventType.KeyDown)
			{
				return;
			}
			KeyCode keyCode = current.keyCode;
			if (_MerchantConfig.serviceTemplate < 0)
			{
				if (!current.control)
				{
					return;
				}
				switch (keyCode)
				{
				case KeyCode.T:
					StopSystem();
					current.Use();
					break;
				case KeyCode.E:
					if (CollectMethod())
					{
						ConnectProducer();
					}
					else
					{
						VerifyProducer(0);
					}
					if (producer)
					{
						StopSystem();
					}
					current.Use();
					break;
				}
			}
			else if (keyCode == KeyCode.Return || keyCode == KeyCode.KeypadEnter || keyCode == KeyCode.Escape)
			{
				ConnectProducer();
				current.Use();
			}
		}

		private static void ManageProducer(AdvisorDicBridge.ObjectTokenizerResolver key, AnimationCurve ord, Action<AdvisorDicBridge.StructTemplateExpression, float> state, bool ignoreattr2 = false)
		{
			bool flag = ord == null || ord.length == 0;
			foreach (AdvisorDicBridge.StructTemplateExpression item in key._RoleDic)
			{
				float num = ((!flag) ? ord.Evaluate(item.InvokeAlgo()) : 1f);
				if (ignoreattr2)
				{
					num *= item.VisitAlgo();
				}
				state(item, num);
			}
		}

		private static void ReadProducer(float v, AdvisorDicBridge.StructTemplateExpression caller, SerializedProperty template, SerializedProperty var12, float second3 = 0f, float value4 = float.PositiveInfinity)
		{
			AnimationCurve animationCurve = ((var12.animationCurveValue != null && var12.animationCurveValue.length >= 2) ? var12.animationCurveValue : new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f)));
			int num = -1;
			Keyframe[] keys = animationCurve.keys;
			for (int i = 0; i < keys.Length; i++)
			{
				if (!(Math.Abs(keys[i].time - caller.InvokeAlgo()) >= 0.01f))
				{
					num = i;
					break;
				}
			}
			float num2;
			if (num == -1)
			{
				num2 = animationCurve.Evaluate(caller.InvokeAlgo());
				num = animationCurve.AddKey(caller.InvokeAlgo(), num2);
			}
			else
			{
				num2 = keys[num].value;
			}
			float num3 = caller.ForgotAlgo(animationCurve);
			bool flag = second3 < 0f;
			if (!(template.floatValue * num3 >= 0f))
			{
				v *= -1f;
			}
			float num4 = ((!flag) ? (num2 * ((template.floatValue + v) / template.floatValue)) : (num2 + v / value4));
			if (num4 > 1f || !(num4 >= (float)(flag ? (-1) : 0)))
			{
				if (num4 >= (float)(flag ? (-1) : 0))
				{
					float num5 = 1f / num4;
					float num6 = template.floatValue / num5;
					if (!(num6 <= value4))
					{
						num5 = template.floatValue / value4;
					}
					if (num6 < second3)
					{
						num5 = template.floatValue / second3;
					}
					animationCurve.MoveKey(num, new Keyframe(caller.InvokeAlgo(), 1f));
					for (int j = 0; j < keys.Length; j++)
					{
						if (j != num)
						{
							animationCurve.MoveKey(j, new Keyframe(keys[j].time, keys[j].value * num5));
						}
					}
					template.floatValue /= num5;
				}
				else
				{
					animationCurve.MoveKey(num, new Keyframe(caller.InvokeAlgo(), flag ? (-1) : 0));
				}
			}
			else
			{
				animationCurve.MoveKey(num, new Keyframe(caller.InvokeAlgo(), num4));
			}
			float num7 = animationCurve.keys.Select((Keyframe k) => k.value).Prepend(0f).Max();
			if (num7 < 0.8f)
			{
				float num8 = 1f / num7;
				for (int num9 = 0; num9 < keys.Length; num9++)
				{
					animationCurve.MoveKey(num9, new Keyframe(keys[num9].time, keys[num9].value * num8));
				}
				template.floatValue /= num8;
			}
			var12.animationCurveValue = animationCurve;
		}

		private static void ResolveProducer(VRCPhysBone[] key, AdvisorDicBridge.ObjectTokenizerResolver attr, ContainerModelDispatcher res)
		{
			_003C_003Ec__DisplayClass120_0 CS_0024_003C_003E8__locals28 = new _003C_003Ec__DisplayClass120_0();
			CS_0024_003C_003E8__locals28.m_UtilsConfig = key;
			CS_0024_003C_003E8__locals28.m_ReaderConfig = res;
			CS_0024_003C_003E8__locals28.m_BridgeConfig = attr._ServerDic;
			if (CS_0024_003C_003E8__locals28.m_BridgeConfig == null)
			{
				return;
			}
			CS_0024_003C_003E8__locals28.m_CollectionConfig = new SerializedObject(CS_0024_003C_003E8__locals28.m_BridgeConfig);
			CS_0024_003C_003E8__locals28.m_CollectionConfig.UpdateIfRequiredOrScript();
			CS_0024_003C_003E8__locals28.m_IdentifierConfig = CS_0024_003C_003E8__locals28.m_CollectionConfig.FindProperty(CS_0024_003C_003E8__locals28.m_ReaderConfig._PublisherConfig);
			CS_0024_003C_003E8__locals28._GlobalConfig = CS_0024_003C_003E8__locals28.m_CollectionConfig.FindProperty(CS_0024_003C_003E8__locals28.m_ReaderConfig._MapperConfig);
			CS_0024_003C_003E8__locals28.m_PolicyConfig = CS_0024_003C_003E8__locals28.m_ReaderConfig._SetterConfig;
			CS_0024_003C_003E8__locals28.dispatcherConfig = CS_0024_003C_003E8__locals28.m_ReaderConfig._ObjectConfig;
			float num = ManagerStruct.SearchTest().handleSizeMultiplier;
			float num2 = Mathf.Clamp(HandleUtility.GetHandleSize(CS_0024_003C_003E8__locals28.m_BridgeConfig.transform.position) * 0.05f * num, 0.02f * num, num * 2f);
			_ = EditorStyles.boldLabel;
			Color color = ManagerStruct.SearchTest().generalColor.DefineError();
			Color color2 = Handles.color;
			Handles.color = color;
			AnimationCurve animationCurveValue = CS_0024_003C_003E8__locals28._GlobalConfig.animationCurveValue;
			List<List<AdvisorDicBridge.StructTemplateExpression>> strategyDic = attr._StrategyDic;
			if (CS_0024_003C_003E8__locals28.m_ReaderConfig.visitorConfig != 1)
			{
				Vector3 vector = Vector3.zero;
				Vector3[][] array = new Vector3[strategyDic.Count][];
				for (int i = 0; i < strategyDic.Count; i++)
				{
					List<AdvisorDicBridge.StructTemplateExpression> list = strategyDic[i];
					array[i] = new Vector3[list.Count];
					Vector3 vector2 = Vector3.zero;
					for (int j = 0; j < list.Count; j++)
					{
						AdvisorDicBridge.StructTemplateExpression structTemplateExpression = list[j];
						Vector3 vector3 = ((j != 0) ? vector2 : structTemplateExpression.ResetAlgo());
						if (j != list.Count - 1)
						{
							vector2 = list[j + 1].ResetAlgo();
							vector = vector2 - vector3;
						}
						if (Vector3.Angle(Vector3.right, vector) < 90f)
						{
							vector = -vector;
						}
						Vector3 up = Vector3.up;
						float num3 = structTemplateExpression.ForgotAlgo(animationCurveValue);
						float num4 = CS_0024_003C_003E8__locals28.m_IdentifierConfig.floatValue * num3;
						Vector3 vector4 = vector3 + up * (num * (num4 / CS_0024_003C_003E8__locals28.dispatcherConfig));
						array[i][j] = vector4;
						Handles.DrawDottedLine(vector3, vector4, 5f);
						AdvisorDicBridge.LogoutManager(num4.ToString("F2"), vector4, num2 + 0.01f);
						Vector3 vector5 = Handles.Slider(vector4, up, num2, Handles.DotHandleCap, 0f);
						if (!(vector4 == vector5))
						{
							float num5 = (vector5.y - vector4.y) / num * CS_0024_003C_003E8__locals28.dispatcherConfig;
							if (!(num3 >= 0f))
							{
								num5 *= -1f;
							}
							CS_0024_003C_003E8__locals28.SetRules(structTemplateExpression, num5);
						}
					}
				}
				Vector3[][] array2 = array;
				foreach (Vector3[] points in array2)
				{
					Handles.DrawAAPolyLine(3f * num, points);
				}
			}
			else
			{
				ManageProducer(attr, animationCurveValue, delegate(AdvisorDicBridge.StructTemplateExpression b, float m)
				{
					if (m != 0f)
					{
						Matrix4x4 objectDic = b._ObjectDic;
						Vector4 column = objectDic.GetColumn(3);
						float tag = CS_0024_003C_003E8__locals28.m_BridgeConfig.radius * m;
						EditorGUI.BeginChangeCheck();
						float num6 = AdvisorDicBridge.PublishManager(objectDic.rotation, column, tag, !CS_0024_003C_003E8__locals28.m_BridgeConfig.showGizmos, ManagerStruct.SearchTest().handleSizeMultiplier);
						if (EditorGUI.EndChangeCheck())
						{
							float delta = num6 / m - CS_0024_003C_003E8__locals28.m_BridgeConfig.radius;
							CS_0024_003C_003E8__locals28.SetRules(b, delta);
						}
						AdvisorDicBridge.LogoutManager(tag.ToString("F2"), column);
					}
				}, ignoreattr2: true);
			}
			Handles.color = color2;
		}

		private static void VerifyProducer(int info)
		{
			if (info < 0 || (CollectMethod() && valConfig == info))
			{
				valConfig = -1;
				_MerchantConfig.ResetAccount(6, overridesecond: false);
			}
			else
			{
				valConfig = info;
				_MerchantConfig.TestAccount(6);
			}
			SceneView.RepaintAll();
		}

		internal static void ConnectProducer()
		{
			_MerchantConfig.GetAccount();
			valConfig = -1;
		}

		[MenuItem("CONTEXT/VRCPhysBone/[ADO] Toggle Editor", false, 899)]
		private static void CompareProducer()
		{
			InterruptProducer(listenerStruct);
		}

		internal static void InterruptProducer(bool readv = false)
		{
			if (ruleConfig == null)
			{
				ruleConfig = AdvisorDicBridge.ResolveManager("VRCPhysBone");
			}
			if (roleConfig == null)
			{
				roleConfig = AdvisorDicBridge.ResolveManager("VRCPhysBoneEditor");
			}
			listenerStruct = !readv;
			AdvisorDicBridge.RevertManager(ruleConfig, listenerStruct ? typeof(ComparatorMethodObject) : roleConfig);
		}

		private static void ComputeProducer()
		{
			m_MessageStruct = new byte[_ImporterStruct.Length];
			bool flag = true;
			VRCPhysBone[] array = observerStruct;
			foreach (VRCPhysBone vRCPhysBone in array)
			{
				for (int j = 0; j < m_MessageStruct.Length; j++)
				{
					if (m_MessageStruct[j] == 2)
					{
						continue;
					}
					List<VRCPhysBoneColliderBase> colliders = vRCPhysBone.colliders;
					if (colliders != null && colliders.Contains(_ImporterStruct[j]))
					{
						if (m_MessageStruct[j] != 0 || flag)
						{
							m_MessageStruct[j] = 1;
						}
						else
						{
							m_MessageStruct[j] = 2;
						}
					}
					else if (m_MessageStruct[j] != 1 || flag)
					{
						m_MessageStruct[j] = 0;
					}
					else
					{
						m_MessageStruct[j] = 2;
					}
				}
				flag = false;
			}
		}

		private static void StartProducer()
		{
			VRCPhysBone[] array;
			if (!QueryMethod())
			{
				bool flag = false;
				array = observerStruct;
				foreach (VRCPhysBone vRCPhysBone in array)
				{
					for (int num = vRCPhysBone.ignoreTransforms.Count - 1; num >= 0; num--)
					{
						Transform _WatcherConfig = vRCPhysBone.ignoreTransforms[num];
						if (!(_WatcherConfig == null))
						{
							Transform transform = (vRCPhysBone.rootTransform ? vRCPhysBone.rootTransform : vRCPhysBone.transform);
							if (_WatcherConfig == transform || !_WatcherConfig.IsChildOf(transform) || vRCPhysBone.ignoreTransforms.Any((Transform t2) => _WatcherConfig != t2 && (bool)t2 && _WatcherConfig.IsChildOf(t2)))
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
					StopStruct("Optimized ignore transforms.");
				}
				return;
			}
			m_MessageStruct = new byte[m_RegStruct.Length];
			bool flag2 = true;
			array = observerStruct;
			foreach (VRCPhysBone vRCPhysBone2 in array)
			{
				for (int num2 = 0; num2 < m_MessageStruct.Length; num2++)
				{
					if (m_MessageStruct[num2] == 2)
					{
						continue;
					}
					List<Transform> ignoreTransforms = vRCPhysBone2.ignoreTransforms;
					if (ignoreTransforms == null || !ignoreTransforms.Contains(m_RegStruct[num2]))
					{
						if (m_MessageStruct[num2] != 1 || flag2)
						{
							m_MessageStruct[num2] = 0;
						}
						else
						{
							m_MessageStruct[num2] = 2;
						}
					}
					else if (m_MessageStruct[num2] == 0 && !flag2)
					{
						m_MessageStruct[num2] = 2;
					}
					else
					{
						m_MessageStruct[num2] = 1;
					}
				}
				flag2 = false;
			}
		}

		private static void InitProducer()
		{
			if (!registryConfig)
			{
				registryConfig = true;
				float[] array = new float[AdvisorDicBridge._BroadcasterTemplate.Count((AdvisorDicBridge.PageDic p) => p._BaseDic)];
				for (int num = 0; num < array.Length; num++)
				{
					array[num] = 1f / (float)array.Length;
				}
				_InstanceStruct = SerializerTestStub.TestMapping(array);
			}
		}

		private void OnEnable()
		{
			m_BridgeStruct = this;
			InitProducer();
			CompareSystem(m_MockStruct, Repaint);
			CancelProducer();
			CheckSystem(ref m_Mapping, ref m_Definition);
			VerifySystem();
			Transform root = ((VRCPhysBone)CustomizeProducer()).transform.root;
			observerStruct = base.targets.Cast<VRCPhysBone>().ToArray();
			_ImporterStruct = root.GetComponentsInChildren<VRCPhysBoneCollider>();
			parameterStruct = root.GetComponentsInChildren<VRCPhysBone>();
			m_RegStruct = observerStruct.SelectMany((VRCPhysBone pb) => pb.GetRootTransform().GetComponentsInChildren<Transform>()).ToArray();
			SceneView.duringSceneGui -= DefineProducer;
			SceneView.duringSceneGui += DefineProducer;
		}

		private void OnDisable()
		{
			ConnectProducer();
			SceneView.duringSceneGui -= DefineProducer;
			Tools.hidden = false;
		}

		private void CheckProducer()
		{
			globalStruct = base.serializedObject.FindProperty("version");
			policyStruct = base.serializedObject.FindProperty("integrationType");
			dispatcherStruct = base.serializedObject.FindProperty("rootTransform");
			m_CollectionStruct = base.serializedObject.FindProperty("ignoreTransforms");
			readerStruct = base.serializedObject.FindProperty("endpointPosition");
			_PoolStruct = base.serializedObject.FindProperty("multiChildType");
			_WriterStruct = base.serializedObject.FindProperty("pull");
			interpreterStruct = base.serializedObject.FindProperty("pullCurve");
			_AttributeStruct = base.serializedObject.FindProperty("spring");
			m_IssuerStruct = base.serializedObject.FindProperty("springCurve");
			m_WatcherStruct = base.serializedObject.FindProperty("stiffness");
			contextStruct = base.serializedObject.FindProperty("stiffnessCurve");
			_SchemaStruct = base.serializedObject.FindProperty("immobile");
			_ReponseStruct = base.serializedObject.FindProperty("immobileType");
			m_ProcessorStruct = base.serializedObject.FindProperty("immobileCurve");
			_SingletonStruct = base.serializedObject.FindProperty("gravity");
			procStruct = base.serializedObject.FindProperty("gravityCurve");
			m_SystemConfig = base.serializedObject.FindProperty("gravityFalloff");
			m_StructConfig = base.serializedObject.FindProperty("gravityFalloffCurve");
			m_ConfigConfig = base.serializedObject.FindProperty("allowCollision");
			_ModelConfig = base.serializedObject.FindProperty("collisionFilter");
			templateConfig = base.serializedObject.FindProperty("radius");
			m_DicConfig = base.serializedObject.FindProperty("radiusCurve");
			m_ServiceConfig = base.serializedObject.FindProperty("colliders");
			errorConfig = base.serializedObject.FindProperty("limitType");
			taskConfig = base.serializedObject.FindProperty("maxAngleX");
			m_ProducerConfig = base.serializedObject.FindProperty("maxAngleXCurve");
			methodConfig = base.serializedObject.FindProperty("maxAngleZ");
			m_ResolverConfig = base.serializedObject.FindProperty("maxAngleZCurve");
			_IteratorConfig = base.serializedObject.FindProperty("limitRotation");
			_RulesConfig = _IteratorConfig.FindPropertyRelative("x");
			_TokenizerConfig = _IteratorConfig.FindPropertyRelative("y");
			m_SpecificationConfig = _IteratorConfig.FindPropertyRelative("z");
			_AccountConfig = base.serializedObject.FindProperty("limitRotationXCurve");
			_ManagerConfig = base.serializedObject.FindProperty("limitRotationYCurve");
			_ParamConfig = base.serializedObject.FindProperty("limitRotationZCurve");
			eventConfig = base.serializedObject.FindProperty("allowGrabbing");
			m_RequestConfig = base.serializedObject.FindProperty("allowPosing");
			_WrapperConfig = base.serializedObject.FindProperty("poseFilter");
			m_TestsConfig = base.serializedObject.FindProperty("grabFilter");
			_AlgoConfig = base.serializedObject.FindProperty("grabMovement");
			_MappingConfig = base.serializedObject.FindProperty("snapToHand");
			parserConfig = base.serializedObject.FindProperty("stretchMotion");
			definitionConfig = base.serializedObject.FindProperty("stretchMotionCurve");
			initializerConfig = base.serializedObject.FindProperty("maxStretch");
			m_InfoConfig = base.serializedObject.FindProperty("maxStretchCurve");
			m_AuthenticationConfig = base.serializedObject.FindProperty("maxSquish");
			m_ClientConfig = base.serializedObject.FindProperty("maxSquishCurve");
			_WorkerConfig = base.serializedObject.FindProperty("isAnimated");
			m_ContainerConfig = base.serializedObject.FindProperty("parameter");
			exceptionConfig = base.serializedObject.FindProperty("resetWhenDisabled");
			propertyConfig = base.serializedObject.FindProperty("showGizmos");
			descriptorConfig = base.serializedObject.FindProperty("boneOpacity");
			m_FactoryConfig = base.serializedObject.FindProperty("limitOpacity");
			_TagConfig = new ContainerModelDispatcher[15]
			{
				new ContainerModelDispatcher(_WriterStruct, interpreterStruct),
				new ContainerModelDispatcher(_AttributeStruct, m_IssuerStruct),
				new ContainerModelDispatcher(m_WatcherStruct, contextStruct),
				new ContainerModelDispatcher(_SchemaStruct, m_ProcessorStruct),
				new ContainerModelDispatcher(_SingletonStruct, procStruct, -1f),
				new ContainerModelDispatcher(m_SystemConfig, m_StructConfig),
				new ContainerModelDispatcher(templateConfig, m_DicConfig, 0f, float.PositiveInfinity, 1),
				new ContainerModelDispatcher(taskConfig, m_ProducerConfig, 0f, 180f),
				new ContainerModelDispatcher(methodConfig, m_ResolverConfig, 0f, 90f),
				new ContainerModelDispatcher("Limit Rotation X", _RulesConfig, _AccountConfig, 0f, 360f),
				new ContainerModelDispatcher("Limit Rotation Y", _TokenizerConfig, _ManagerConfig, 0f, 360f),
				new ContainerModelDispatcher("Limit Rotation Z", m_SpecificationConfig, _ParamConfig, 0f, 360f),
				new ContainerModelDispatcher(parserConfig, definitionConfig),
				new ContainerModelDispatcher(initializerConfig, m_InfoConfig, 0f, float.PositiveInfinity),
				new ContainerModelDispatcher(m_AuthenticationConfig, m_ClientConfig)
			};
			if (_DatabaseConfig)
			{
				return;
			}
			List<GUIContent> list = new List<GUIContent>();
			m_SerializerConfig = new Dictionary<int, int>();
			m_InterceptorConfig = new Dictionary<int, int>();
			int key = 0;
			for (int i = 0; i < _TagConfig.Length; i++)
			{
				ContainerModelDispatcher containerModelDispatcher = _TagConfig[i];
				if (containerModelDispatcher._StatusConfig)
				{
					list.Add(new GUIContent(containerModelDispatcher._StrategyConfig));
					m_SerializerConfig.Add(key, i);
					m_InterceptorConfig.Add(i, key++);
				}
			}
			_ConfigurationConfig = list.ToArray();
			_ParamsConfig = m_SerializerConfig.Keys.ToArray();
			_DatabaseConfig = true;
		}

		internal static void CancelProducer()
		{
			if ((bool)ManagerStruct.SearchTest().globalGizmo)
			{
				VRCPhysBone[] array = UnityEngine.Object.FindObjectsOfType<VRCPhysBone>();
				int num = 0;
				if (0 < array.Length)
				{
					VRCPhysBone obj = array[num];
					obj.showGizmos = ManagerStruct.SearchTest().gizmosActive;
					obj.boneOpacity = ManagerStruct.SearchTest().gizmoBoneOpacity;
					obj.limitOpacity = ManagerStruct.SearchTest().gizmoLimitOpacity;
					num++;
				}
			}
		}

		private void DisableProducer()
		{
			bool flag = policyStruct.enumValueIndex == 1;
			int row_ord = (policyStruct.hasMultipleDifferentValues ? 2 : policyStruct.enumValueIndex);
			using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
			using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, row_ord, AdvisorDicBridge.ManageRequest().configDic))
			{
				flag = GUILayout.Toggle(flag, "Advanced", GUI.skin.button, GUILayout.ExpandWidth(expand: false));
			}
			if (changeCheckScope.changed)
			{
				policyStruct.enumValueIndex = (flag ? 1 : 0);
			}
		}

		private static void IncludeProducer(int spec, GUIContent second = null)
		{
			ContainerModelDispatcher containerModelDispatcher = _TagConfig[spec];
			SerializedProperty indexerConfig = containerModelDispatcher._IndexerConfig;
			SerializedProperty broadcasterConfig = containerModelDispatcher.m_BroadcasterConfig;
			using (new GUILayout.HorizontalScope())
			{
				if (second == null)
				{
					EditorGUILayout.PropertyField(indexerConfig);
				}
				else
				{
					EditorGUILayout.PropertyField(indexerConfig, second);
				}
				AssetProducer(broadcasterConfig, string.Empty);
				using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, CollectMethod() && PrintMethod() == containerModelDispatcher, AdvisorDicBridge.m_InitializerTemplate, AdvisorDicBridge.infoTemplate))
				{
					if (AdvisorDicBridge.AwakeManager(AdvisorDicBridge.PrepareRequest().m_PrinterTemplate, AdvisorDicBridge.ManageRequest().modelDic, GUILayout.ExpandWidth(expand: false)))
					{
						VerifyProducer(spec);
					}
				}
			}
		}

		private static void RateProducer(int instance_length)
		{
			if (_TagConfig[instance_length]._StatusConfig)
			{
				IncludeProducer(instance_length);
			}
		}

		private static bool ForgotProducer(bool countconfig, SerializedProperty pred, SerializedProperty util)
		{
			using (new GUILayout.HorizontalScope())
			{
				EditorGUILayout.PropertyField(pred);
				AssetProducer(util, string.Empty);
				return ResetSystem(countconfig, AdvisorDicBridge.PrepareRequest().m_PrinterTemplate);
			}
		}

		private static void AssetProducer(SerializedProperty param, string cust, bool updatedic = true)
		{
			if (!string.IsNullOrWhiteSpace(cust))
			{
				GUILayout.Label(cust, GUILayout.ExpandWidth(expand: false));
			}
			EditorGUILayout.CurveField(param, Color.cyan, new Rect(0f, 0f, 1f, 1f), GUIContent.none, GUILayout.MaxWidth(85f));
			if (param.animationCurveValue == null || param.animationCurveValue.length < 2)
			{
				GUI.Label(GUILayoutUtility.GetLastRect(), "///////////////////////////////", AdvisorDicBridge.ManageRequest().processorTemplate);
			}
			if (updatedic && AdvisorDicBridge.AwakeManager(AdvisorDicBridge.PrepareRequest().refTemplate, GUI.skin.label, GUILayout.Width(14f)))
			{
				param.animationCurveValue = new AnimationCurve();
			}
		}

		[CompilerGenerated]
		internal static void TestProducer(Vector3 value, ref _003C_003Ec__DisplayClass116_0 cfg)
		{
			Event current = Event.current;
			bool alt = current.alt;
			if (cfg._ListenerConfig.Length == 1)
			{
				ResetProducer(cfg.m_ObserverConfig, value);
				return;
			}
			if (alt)
			{
				ResetProducer(cfg.m_ObserverConfig, value);
				return;
			}
			VRCPhysBone[] array;
			if (!current.shift)
			{
				array = cfg._ListenerConfig;
				for (int i = 0; i < array.Length; i++)
				{
					ResetProducer(array[i], value);
				}
				return;
			}
			Vector3 m_ParameterConfig = ResetProducer(cfg.m_ObserverConfig, value);
			array = cfg._ListenerConfig;
			foreach (VRCPhysBone vRCPhysBone in array)
			{
				if (vRCPhysBone != cfg.m_ObserverConfig)
				{
					GetProducer(vRCPhysBone, readerStruct.propertyPath, delegate(SerializedProperty sp)
					{
						sp.vector3Value = m_ParameterConfig;
					});
				}
			}
		}

		[CompilerGenerated]
		internal static Vector3 ResetProducer(VRCPhysBone key, Vector3 caller)
		{
			Vector3 _MessageConfig = Vector3.zero;
			GetProducer(key, readerStruct.propertyPath, delegate(SerializedProperty sp)
			{
				sp.vector3Value += caller;
				_MessageConfig = sp.vector3Value;
			});
			return _MessageConfig;
		}

		[CompilerGenerated]
		internal static void GetProducer(VRCPhysBone spec, string vis, Action<SerializedProperty> consumer)
		{
			SerializedObject obj = new SerializedObject(spec);
			obj.UpdateIfRequiredOrScript();
			SerializedProperty obj2 = obj.FindProperty(vis);
			consumer(obj2);
			obj.ApplyModifiedProperties();
		}

		UnityEngine.Object CustomizeProducer()
		{
			return base.target;
		}

		internal static bool LogoutWorker()
		{
			return (object)MapWorker == null;
		}
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _ContextConfig = new _003C_003Ec();

		public static Func<bool> m_SchemaConfig;

		public static Func<bool> _ReponseConfig;

		public static Func<UnityEngine.Object, AdvisorDicBridge.ConfigurationDic> m_ProcessorConfig;

		public static Func<UnityEngine.Object, AdvisorDicBridge.ConfigurationDic> m_SingletonConfig;

		public static Func<UnityEngine.Object, AdvisorDicBridge.ConfigurationDic> _ProcConfig;

		public static Func<bool> m_SystemModel;

		public static Func<bool> m_StructModel;

		public static Func<bool> _ConfigModel;

		public static Func<bool> modelModel;

		public static Func<bool> templateModel;

		public static Func<Rect> m_DicModel;

		public static Action serviceModel;

		public static Func<Transform, GameObject> errorModel;

		public static Func<GameObject, IEnumerable<VRCPhysBone>> m_TaskModel;

		public static Func<GameObject, IEnumerable<VRCPhysBoneColliderBase>> _ProducerModel;

		public static Func<Transform, GameObject> methodModel;

		public static Func<GameObject, bool> m_ResolverModel;

		public static Func<bool> m_IteratorModel;

		public static Func<VRCContactSender, IEnumerable<string>> m_RulesModel;

		public static Func<VRCContactReceiver, IEnumerable<string>> tokenizerModel;

		public static Func<string, string> specificationModel;

		public static Func<VRCAvatarDescriptor.CustomAnimLayer, bool> accountModel;

		public static Func<VRCAvatarDescriptor.CustomAnimLayer, UnityEditor.Animations.AnimatorController> _ManagerModel;

		public static Func<UnityEditor.Animations.AnimatorController, bool> paramModel;

		public static Func<UnityEditor.Animations.AnimatorController, IEnumerable<UnityEngine.AnimatorControllerParameter>> eventModel;

		public static Func<UnityEngine.AnimatorControllerParameter, string> _TestsModel;

		public static Func<string, bool> requestModel;

		public static Func<VRCAvatarDescriptor, bool> wrapperModel;

		public static Func<VRCAvatarDescriptor, string> algoModel;

		public static Func<UnityEngine.Object, bool> mappingModel;

		public static Func<UnityEngine.Object, bool> parserModel;

		public static Action _DefinitionModel;

		public static Action initializerModel;

		public static Func<bool> m_InfoModel;

		public static Action<Exception> _AuthenticationModel;

		public static Action m_ClientModel;

		public static Action<GetterDicBridge> m_WorkerModel;

		public static Action<Exception> _ContainerModel;

		public static Func<ListServiceSerializer, bool> m_ExceptionModel;

		public static Func<string, bool> m_PropertyModel;

		public static Func<(bool, string), bool> _DescriptorModel;

		public static Func<(bool, string), string> factoryModel;

		public static Func<(bool, string), bool> _TagModel;

		public static Func<(bool, string), string> configurationModel;

		public static Func<bool> _ParamsModel;

		public static Func<bool> _SerializerModel;

		public static Action<GetterDicBridge> _InterceptorModel;

		public static Action<Exception> m_DatabaseModel;

		public static Action valModel;

		public static Action _MerchantModel;

		public static Action<GetterDicBridge> classModel;

		public static Action<Exception> _PredicateModel;

		public static Action _ServerModel;

		public static GenericMenu.MenuFunction m_RuleModel;

		public static GenericMenu.MenuFunction _RoleModel;

		public static GenericMenu.MenuFunction _RegistryModel;

		public static GenericMenu.MenuFunction m_StrategyModel;

		public static GenericMenu.MenuFunction _IndexerModel;

		public static GenericMenu.MenuFunction m_BroadcasterModel;

		public static GenericMenu.MenuFunction publisherModel;

		public static GenericMenu.MenuFunction mapperModel;

		public static Action _ProductModel;

		public static Action<Exception> m_SetterModel;

		public static Action _ObjectModel;

		public static Func<Task> visitorModel;

		internal static _003C_003Ec FlushClass;

		internal bool InitRules()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		}

		internal bool CheckRules()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		}

		internal AdvisorDicBridge.ConfigurationDic CancelRules(UnityEngine.Object t2)
		{
			return new AdvisorDicBridge.ConfigurationDic((VRCPhysBoneCollider)t2);
		}

		internal AdvisorDicBridge.ConfigurationDic DisableRules(UnityEngine.Object t2)
		{
			return new AdvisorDicBridge.ConfigurationDic((VRCContactSender)t2);
		}

		internal AdvisorDicBridge.ConfigurationDic IncludeRules(UnityEngine.Object t2)
		{
			return new AdvisorDicBridge.ConfigurationDic((VRCContactReceiver)t2);
		}

		internal bool RateRules()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		}

		internal bool ForgotRules()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		}

		internal bool AssetRules()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		}

		internal bool TestRules()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		}

		internal bool ResetRules()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		}

		internal Rect GetRules()
		{
			using (new GUILayout.HorizontalScope())
			{
				bool ordclose;
				string tooltip = ((ordclose = ManagerStruct.SearchTest().hideToolsDuringTesting) ? "Native tools are hidden during test." : "Native tools are visible during test.");
				using (new TaskServiceClass(TaskServiceClass.ColoringType.FG, ordclose, AdvisorDicBridge.m_InitializerTemplate, AdvisorDicBridge.infoTemplate))
				{
					if (AdvisorDicBridge.RateManager(new GUIContent(AdvisorDicBridge.PrepareRequest().repositoryTemplate)
					{
						tooltip = tooltip
					}))
					{
						ManagerStruct.SearchTest().hideToolsDuringTesting.InstantiateTest();
						Tools.hidden = false;
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.Label("Testing", AdvisorDicBridge.ManageRequest()._WriterTemplate);
				Rect lastRect = GUILayoutUtility.GetLastRect();
				GUILayout.FlexibleSpace();
				StartStruct();
				return lastRect;
			}
		}

		internal void VisitRules()
		{
			using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, AdvisorDicBridge.infoTemplate))
			{
				if (AdvisorDicBridge.GetManager("Stop Testing") || AdvisorDicBridge.SetupManager() || AdvisorDicBridge.SortManager())
				{
					StopSystem();
				}
			}
			using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, AdvisorDicBridge._ClientTemplate))
			{
				if (AdvisorDicBridge.GetManager("Restart"))
				{
					WriteSystem();
				}
			}
			using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, _Class, AdvisorDicBridge.m_InitializerTemplate))
			{
				using (new EditorGUI.DisabledScope(!_Class))
				{
					if (!AdvisorDicBridge.GetManager("Apply All Changes"))
					{
						return;
					}
					foreach (UnityEngine.Object item in merchant.Keys.ToList())
					{
						if (merchant[item])
						{
							UnityEngine.Object obj = m_Val[item];
							if (obj != null)
							{
								Undo.RecordObject(obj, "ADO - Apply Changes");
								EditorUtility.CopySerialized(item, obj);
								merchant[item] = false;
							}
						}
					}
					_Class = false;
					CallSystem();
				}
			}
		}

		internal GameObject AwakeRules(Transform t)
		{
			return t.root.gameObject;
		}

		internal IEnumerable<VRCPhysBone> InvokeRules(GameObject o)
		{
			return o.GetComponentsInChildren<VRCPhysBone>(includeInactive: true);
		}

		internal IEnumerable<VRCPhysBoneColliderBase> CustomizeRules(GameObject o)
		{
			return o.GetComponentsInChildren<VRCPhysBoneColliderBase>(includeInactive: true);
		}

		internal GameObject MoveRules(Transform t)
		{
			return t.gameObject;
		}

		internal bool FillRules(GameObject o)
		{
			return o;
		}

		internal bool ChangeRules()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		}

		internal IEnumerable<string> CalculateRules(VRCContactSender cs)
		{
			return cs.collisionTags;
		}

		internal IEnumerable<string> PopRules(VRCContactReceiver cr)
		{
			return cr.collisionTags;
		}

		internal string CallRules(string s)
		{
			return "Default/" + s;
		}

		internal bool PostRules(VRCAvatarDescriptor.CustomAnimLayer rc)
		{
			if (!rc.isDefault)
			{
				return rc.animatorController;
			}
			return false;
		}

		internal UnityEditor.Animations.AnimatorController LoginRules(VRCAvatarDescriptor.CustomAnimLayer rc)
		{
			return AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(AssetDatabase.GetAssetPath(rc.animatorController));
		}

		internal bool RemoveRules(UnityEditor.Animations.AnimatorController c)
		{
			return c;
		}

		internal IEnumerable<UnityEngine.AnimatorControllerParameter> DestroyRules(UnityEditor.Animations.AnimatorController c)
		{
			return c.parameters;
		}

		internal string CreateRules(UnityEngine.AnimatorControllerParameter p)
		{
			return p.name;
		}

		internal bool CloneRules(string p)
		{
			return !AdvisorDicBridge.strategyTemplate.Contains(p);
		}

		internal bool FlushRules(VRCAvatarDescriptor a)
		{
			return (UnityEngine.Object)(object)a;
		}

		internal string RegisterRules(VRCAvatarDescriptor x)
		{
			return ((UnityEngine.Object)(object)x).name;
		}

		internal bool PatchRules(UnityEngine.Object b)
		{
			if (!(b != null) || !merchant.ContainsKey(b))
			{
				return false;
			}
			return m_Val[b] != null;
		}

		internal bool CalcRules(UnityEngine.Object b)
		{
			return merchant[b];
		}

		internal void MapRules()
		{
			m_Indexer = false;
			_Strategy = false;
			SetStruct();
		}

		internal void SortRules()
		{
			CreateSystem(isres: false);
		}

		internal bool SetupRules()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		}

		internal void PrintRules(Exception exception)
		{
			m_Consumer = false;
			m_Invocation = false;
			_Helper = true;
			StopStruct($"Something went wrong while verifying license:\n\n{exception}", CustomLogType.Error);
		}

		internal void FindRules(GetterDicBridge response)
		{
			_Advisor = false;
			SetupSystem(response, delegate
			{
				state = false;
				ManagerStruct.SearchTest().a_HasSucceededLastVerification.ListService(useres: true);
				CreateSystem(isres: true);
			});
		}

		internal void CollectRules()
		{
			state = false;
			ManagerStruct.SearchTest().a_HasSucceededLastVerification.ListService(useres: true);
			CreateSystem(isres: true);
		}

		internal void ValidateRules(Exception exception)
		{
			_Advisor = false;
			StopStruct($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
		}

		internal bool RestartRules(ListServiceSerializer p)
		{
			return p._ServiceStruct;
		}

		internal bool ViewRules(string v)
		{
			return !string.IsNullOrWhiteSpace(v);
		}

		internal bool SearchRules((bool, string) i)
		{
			return !i.Item1;
		}

		internal string QueryRules((bool, string) i)
		{
			return i.Item2;
		}

		internal bool OrderRules((bool, string) i)
		{
			return !i.Item1;
		}

		internal string EnableRules((bool, string) i)
		{
			return i.Item2;
		}

		internal bool ConcatRules()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		}

		internal bool LogoutRules()
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		}

		internal void ExcludeRules()
		{
			List<(string, string)> list = PrintSystem("transferlicenserequest");
			FindSystem(list);
			CountStruct(InstantiateSystem(list.ToArray())).PublishAccount(delegate(GetterDicBridge response)
			{
				_003C_003Ec__DisplayClass179_0 _003C_003Ec__DisplayClass179_ = new _003C_003Ec__DisplayClass179_0
				{
					candidateModel = response
				};
				m_Proxy = false;
				SetupSystem(_003C_003Ec__DisplayClass179_.candidateModel, _003C_003Ec__DisplayClass179_.RegisterSpecification);
			}, delegate(Exception exception)
			{
				m_Proxy = false;
				StopStruct($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, SetStruct);
		}

		internal void AddRules(GetterDicBridge response)
		{
			_003C_003Ec__DisplayClass179_0 _003C_003Ec__DisplayClass179_ = new _003C_003Ec__DisplayClass179_0
			{
				candidateModel = response
			};
			m_Proxy = false;
			SetupSystem(_003C_003Ec__DisplayClass179_.candidateModel, _003C_003Ec__DisplayClass179_.RegisterSpecification);
		}

		internal void PublishRules(Exception exception)
		{
			m_Proxy = false;
			StopStruct($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
		}

		internal void InstantiateRules()
		{
			List<(string, string)> list = PrintSystem("transferlicenseconfirm");
			list.Add(("verification_code", m_Object));
			FindSystem(list);
			CountStruct(InstantiateSystem(list.ToArray())).PublishAccount(delegate(GetterDicBridge response)
			{
				comparator = false;
				SetupSystem(response, delegate
				{
					m_Callback = false;
					_Filter = false;
					state = false;
					CreateSystem(isres: true);
				});
			}, delegate(Exception exception)
			{
				comparator = false;
				StopStruct($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, SetStruct);
		}

		internal void RevertRules(GetterDicBridge response)
		{
			comparator = false;
			SetupSystem(response, delegate
			{
				m_Callback = false;
				_Filter = false;
				state = false;
				CreateSystem(isres: true);
			});
		}

		internal void ReflectTokenizer()
		{
			m_Callback = false;
			_Filter = false;
			state = false;
			CreateSystem(isres: true);
		}

		internal void CountTokenizer(Exception exception)
		{
			comparator = false;
			StopStruct($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
		}

		internal void SetTokenizer()
		{
			SessionState.EraseString("No1lKII9IzcBAbihub6nCg==updateinfo");
			ReadStruct();
		}

		internal void DeleteTokenizer()
		{
			_Strategy.PushManager();
		}

		internal void NewTokenizer()
		{
			ManagerStruct.SearchTest().a_VerifyOnDisplay.InstantiateTest();
			ManagerStruct.SearchTest().a_VerifyOnProjectLoad.ListService(useres: false);
		}

		internal void SelectTokenizer()
		{
			ManagerStruct.SearchTest().a_VerifyOnProjectLoad.InstantiateTest();
			ManagerStruct.SearchTest().a_VerifyOnDisplay.ListService(useres: false);
		}

		internal void RunTokenizer()
		{
			Application.OpenURL("");
		}

		internal void StopTokenizer()
		{
			Application.OpenURL(m_Customer[0].Item2);
		}

		internal void WriteTokenizer()
		{
			Application.OpenURL("");
		}

		internal void DefineTokenizer()
		{
			Application.OpenURL("https://dreadrith.com/license-tos");
		}

		internal void PushTokenizer()
		{
			ResolveStruct(isident: false);
		}

		internal void UpdateTokenizer(Exception exc)
		{
			StopStruct($"Something went wrong while checking for an update!\n\n{exc}", CustomLogType.Error);
		}

		internal void InsertTokenizer()
		{
			_Printer = false;
			SetStruct();
		}

		internal async Task PrepareTokenizer()
		{
			await Task.Delay(3000);
			ManagerStruct.SearchTest().u_updateHidden.ListService(useres: true);
			SetStruct();
		}

		internal static bool DestroyClass()
		{
			return FlushClass == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass132_0
	{
		public string helperModel;

		internal static _003C_003Ec__DisplayClass132_0 CreateClass;

		internal void PostTokenizer()
		{
			bool exporter = _Exporter;
			m_Invocation = false;
			_Exporter = false;
			decorator = (_Publisher = (product = string.Empty));
			ManagerStruct.SearchTest().a_HasSucceededLastVerification.ListService(useres: false);
			SessionState.EraseBool(helperModel);
			ViewSystem(exporter);
		}

		internal string LoginTokenizer(string key, ref _003C_003Ec__DisplayClass132_1 _003C_003Ec__DisplayClass132_1_0, ref _003C_003Ec__DisplayClass132_2 _003C_003Ec__DisplayClass132_2_0)
		{
			return RateStruct(SessionState.GetString(ForgotStruct(helperModel + key, ref _003C_003Ec__DisplayClass132_2_0), string.Empty), ref _003C_003Ec__DisplayClass132_1_0);
		}

		internal void RemoveTokenizer()
		{
			List<(string, string)> list = PrintSystem("verifylicense");
			FindSystem(list);
			CountStruct(InstantiateSystem(list.ToArray())).PublishAccount(delegate(GetterDicBridge response)
			{
				_003C_003Ec__DisplayClass132_3 _003C_003Ec__DisplayClass132_ = new _003C_003Ec__DisplayClass132_3
				{
					baseModel = this,
					_CreatorModel = response
				};
				m_Consumer = false;
				state = true;
				SetupSystem(_003C_003Ec__DisplayClass132_._CreatorModel, _003C_003Ec__DisplayClass132_.CalcTokenizer, delegate
				{
					bool exporter = _Exporter;
					m_Invocation = false;
					_Exporter = false;
					decorator = (_Publisher = (product = string.Empty));
					ManagerStruct.SearchTest().a_HasSucceededLastVerification.ListService(useres: false);
					SessionState.EraseBool(helperModel);
					ViewSystem(exporter);
				}, isres2: false);
			}, _003C_003Ec._ContextConfig.PrintRules, null, null, SetStruct);
		}

		internal void DestroyTokenizer(GetterDicBridge response)
		{
			_003C_003Ec__DisplayClass132_3 _003C_003Ec__DisplayClass132_ = new _003C_003Ec__DisplayClass132_3
			{
				baseModel = this,
				_CreatorModel = response
			};
			m_Consumer = false;
			state = true;
			SetupSystem(_003C_003Ec__DisplayClass132_._CreatorModel, _003C_003Ec__DisplayClass132_.CalcTokenizer, delegate
			{
				bool exporter = _Exporter;
				m_Invocation = false;
				_Exporter = false;
				decorator = (_Publisher = (product = string.Empty));
				ManagerStruct.SearchTest().a_HasSucceededLastVerification.ListService(useres: false);
				SessionState.EraseBool(helperModel);
				ViewSystem(exporter);
			}, isres2: false);
		}

		internal void CreateTokenizer(string key, string value, ref _003C_003Ec__DisplayClass132_4 _003C_003Ec__DisplayClass132_4_0, ref _003C_003Ec__DisplayClass132_5 _003C_003Ec__DisplayClass132_5_0)
		{
			SessionState.SetString(AssetStruct(helperModel + key, ref _003C_003Ec__DisplayClass132_5_0), TestStruct(value, ref _003C_003Ec__DisplayClass132_4_0));
		}

		internal static bool CancelClass()
		{
			return CreateClass == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass132_1
	{
		public AesManaged _PageModel;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass132_2
	{
		public HMACSHA1 prototypeModel;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass132_3
	{
		public GetterDicBridge _CreatorModel;

		public _003C_003Ec__DisplayClass132_0 baseModel;

		internal static _003C_003Ec__DisplayClass132_3 MapClass;

		internal void CalcTokenizer()
		{
			try
			{
				string text = _CreatorModel.StartTest("date");
				if (CollectSystem() != text)
				{
					StopStruct("Date Mismatch! Please make sure your system's date is correct.\nLocal: " + m_Getter + "  |  Remote: " + text, CustomLogType.Error);
					_Helper = true;
					baseModel.PostTokenizer();
					return;
				}
				_Publisher = _CreatorModel.StartTest("username");
				product = _CreatorModel.StartTest("variant");
				decorator = _CreatorModel.StartTest("token");
				FlushSystem();
				RegisterSystem();
				string info = _CreatorModel.StartTest("message");
				if (!_Exporter)
				{
					StopStruct(info);
				}
				m_Invocation = true;
				ManagerStruct.SearchTest().a_HasSucceededLastVerification.ListService(useres: true);
				EditorPrefs.SetString("No1lKII9IzcBAbihub6nCg==LK", setter);
				_003C_003Ec__DisplayClass132_4 _003C_003Ec__DisplayClass132_4_ = default(_003C_003Ec__DisplayClass132_4);
				_003C_003Ec__DisplayClass132_4_.getterModel = new AesManaged();
				try
				{
					_003C_003Ec__DisplayClass132_4_.getterModel.Key = Convert.FromBase64String("LWw2tFi+lgG6KK4+nMum8RuWZMIOhu1urChsHMbizPM=");
					_003C_003Ec__DisplayClass132_4_.getterModel.IV = Convert.FromBase64String("MEZqk6gCgPTwifeH3YrTlQ==");
					_003C_003Ec__DisplayClass132_5 _003C_003Ec__DisplayClass132_5_ = default(_003C_003Ec__DisplayClass132_5);
					_003C_003Ec__DisplayClass132_5_._AdvisorModel = new HMACSHA1(Encoding.UTF8.GetBytes(baseModel.helperModel));
					try
					{
						baseModel.CreateTokenizer("date", m_Getter, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
						baseModel.CreateTokenizer("u", _Publisher, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
						baseModel.CreateTokenizer("v", product, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
						baseModel.CreateTokenizer("r", decorator, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
						baseModel.CreateTokenizer("m", record, ref _003C_003Ec__DisplayClass132_4_, ref _003C_003Ec__DisplayClass132_5_);
					}
					finally
					{
						if (_003C_003Ec__DisplayClass132_5_._AdvisorModel != null)
						{
							((IDisposable)_003C_003Ec__DisplayClass132_5_._AdvisorModel).Dispose();
						}
					}
				}
				finally
				{
					if (_003C_003Ec__DisplayClass132_4_.getterModel != null)
					{
						((IDisposable)_003C_003Ec__DisplayClass132_4_.getterModel).Dispose();
					}
				}
				SessionState.SetBool(baseModel.helperModel, value: true);
				if (!new Func<bool>(_003C_003Ec._ContextConfig.SetupRules)())
				{
					baseModel.PostTokenizer();
				}
				RestartSystem(acceptident: false);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		internal static bool LogoutClass()
		{
			return MapClass == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass132_4
	{
		public AesManaged getterModel;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass132_5
	{
		public HMACSHA1 _AdvisorModel;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass138_0
	{
		public bool consumerModel;

		public string m_AttrModel;

		public StringBuilder m_RecordModel;

		public string[] m_ItemModel;

		public string[] decoratorModel;

		public string[] m_InvocationModel;

		public string[][] _ExporterModel;

		public StringBuilder fieldModel;

		public CancellationTokenSource m_CallbackModel;

		public ListServiceSerializer[] filterModel;

		public bool m_ProxyModel;

		public Action _ComparatorModel;

		internal static _003C_003Ec__DisplayClass138_0 PostClass;

		internal string ConcatTokenizer(string property, string[] extractedValues)
		{
			string text = extractedValues.FirstOrDefault(_003C_003Ec._ContextConfig.ViewRules);
			if (!consumerModel)
			{
				_003C_003Ec__DisplayClass138_3 _003C_003Ec__DisplayClass138_ = new _003C_003Ec__DisplayClass138_3();
				text = (CallStruct(m_AttrModel, property, out _003C_003Ec__DisplayClass138_.printerModel) ? (extractedValues.FirstOrDefault(_003C_003Ec__DisplayClass138_.MoveSpecification) ?? text) : text);
			}
			m_RecordModel.AppendLine(property + ": " + text);
			return text;
		}

		internal void LogoutTokenizer(string o)
		{
			m_ItemModel[0] = o;
		}

		internal void ExcludeTokenizer(string o)
		{
			m_ItemModel[1] = o;
		}

		internal void AddTokenizer(string o)
		{
			m_ItemModel[2] = o;
		}

		internal void PublishTokenizer(string o)
		{
			m_ItemModel[3] = o;
		}

		internal void InstantiateTokenizer(string o)
		{
			decoratorModel[0] = o;
		}

		internal void RevertTokenizer(string o)
		{
			decoratorModel[1] = o;
		}

		internal void ReflectSpecification(string o)
		{
			decoratorModel[2] = o;
		}

		internal void CountSpecification(string o)
		{
			decoratorModel[3] = o;
		}

		internal bool SetSpecification((List<string>, Dictionary<string, RangeInt>) cmdParsedOutput, string property, out string result)
		{
			if (PopStruct(cmdParsedOutput, property, out var role))
			{
				result = ConcatTokenizer(property, role);
				return true;
			}
			result = "Default String";
			return false;
		}

		internal bool DeleteSpecification(string fullInfo, out string result, string[] properties)
		{
			result = string.Empty;
			if (CalculateStruct(fullInfo, properties[0], out var c))
			{
				(bool, string)[] array = new(bool, string)[properties.Length];
				for (int i = 0; i < properties.Length; i++)
				{
					string result2;
					bool item = SetSpecification(c, properties[i], out result2);
					array[i] = (item, result2);
				}
				int num = Mathf.CeilToInt((float)array.Length / 2f);
				if (array.Count(_003C_003Ec._ContextConfig.SearchRules) < num)
				{
					result = string.Join(string.Empty, array.Select(_003C_003Ec._ContextConfig.QueryRules)).Replace(" ", string.Empty);
					return true;
				}
				return false;
			}
			return false;
		}

		internal void NewSpecification()
		{
			try
			{
				WriteSpecification(isCMD: true);
				PushSpecification();
			}
			catch (Exception exc)
			{
				DefineSpecification(isCMD: true, exc);
			}
		}

		internal bool SelectSpecification(string fullInfo, string property, out string result)
		{
			if (CallStruct(fullInfo, property, out var res))
			{
				result = ConcatTokenizer(property, res);
				return true;
			}
			result = "Default String";
			return false;
		}

		internal bool RunSpecification(string fullInfo, out string result, string[] properties)
		{
			result = string.Empty;
			(bool, string)[] array = new(bool, string)[properties.Length];
			for (int i = 0; i < properties.Length; i++)
			{
				string result2;
				bool item = SelectSpecification(fullInfo, properties[i], out result2);
				array[i] = (item, result2);
			}
			if (!array.All(_003C_003Ec._ContextConfig.OrderRules))
			{
				result = string.Join(string.Empty, array.Select(_003C_003Ec._ContextConfig.EnableRules)).Replace(" ", string.Empty);
				return true;
			}
			return false;
		}

		internal void StopSpecification()
		{
			try
			{
				WriteSpecification(isCMD: false);
				PushSpecification();
			}
			catch (Exception exc)
			{
				DefineSpecification(isCMD: false, exc);
			}
		}

		internal void WriteSpecification(bool isCMD)
		{
			bool[] array = new bool[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = ((!isCMD) ? RunSpecification(decoratorModel[i], out m_InvocationModel[i], _ExporterModel[i]) : DeleteSpecification(m_ItemModel[i], out m_InvocationModel[i], _ExporterModel[i]));
			}
			bool num = array[0] || array[1];
			bool flag = num;
			if ((!num || !array[2]) && (!flag || !array[3]) && (!array[2] || !array[3]))
			{
				throw new Exception("Failed to gather hardware info through " + ((!isCMD) ? "Shell" : "CMD"));
			}
		}

		internal void DefineSpecification(bool isCMD, Exception exc)
		{
			if (!isCMD)
			{
				comparator = false;
				m_Proxy = false;
				m_Consumer = false;
				_Advisor = false;
			}
			string text = ((!isCMD) ? "Shell" : "CMD");
			fieldModel.AppendLine("Failed " + text + " Parse");
			fieldModel.AppendLine("Reason: " + exc.Message);
			fieldModel.AppendLine(exc.StackTrace + Environment.NewLine);
			string[] array = ((!isCMD) ? decoratorModel : m_ItemModel);
			for (int i = 0; i < 4; i++)
			{
				fieldModel.AppendLine($"Info {i}:");
				try
				{
					fieldModel.AppendLine(array[i]);
				}
				catch
				{
					fieldModel.AppendLine($"Missing Info {i}!");
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
							File.WriteAllText(text3, MoveStruct(fieldModel.ToString()));
							AssetDatabase.ImportAsset(text3);
							EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(text3));
						}
					}
				}
				else
				{
					Application.OpenURL("https://dreadrith.com/HWIDHelp");
				}
				m_Consumer = false;
				_Advisor = false;
				SetStruct();
				return;
			}
			m_CallbackModel = new CancellationTokenSource();
			m_CallbackModel.CancelAfter(10000);
			ChangeStruct(filterModel, delegate
			{
				try
				{
					WriteSpecification(isCMD: false);
					PushSpecification();
				}
				catch (Exception exc2)
				{
					DefineSpecification(isCMD: false, exc2);
				}
			}, m_CallbackModel);
		}

		internal void PushSpecification()
		{
			EditorPrefs.SetString("DSLICINF", MoveStruct(m_RecordModel.ToString()));
			if (m_ProxyModel)
			{
				for (int i = 0; i < 4; i++)
				{
					m_InvocationModel[i] += "\r\r";
				}
			}
			string[] array = new string[3]
			{
				m_InvocationModel[0] + m_InvocationModel[1],
				m_InvocationModel[2],
				m_InvocationModel[3]
			};
			using (SHA1 sHA = SHA1.Create())
			{
				for (int j = 0; j < 3; j++)
				{
					array[j] = BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(array[j]))).Replace("-", "");
				}
			}
			record = string.Join("-", array);
			RegisterSystem();
			_ComparatorModel();
		}

		internal static bool CallClass()
		{
			return PostClass == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass138_1
	{
		public AesManaged _AdapterModel;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass138_3
	{
		public string[] printerModel;

		internal static _003C_003Ec__DisplayClass138_3 AssetClass;

		internal bool MoveSpecification(string v)
		{
			return v == printerModel[0];
		}

		internal static bool ConnectClass()
		{
			return AssetClass == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass165_0
	{
		[StructLayout(LayoutKind.Auto)]
		private struct AdapterMethodObject : IAsyncStateMachine
		{
			public int _ProcessModel;

			public AsyncTaskMethodBuilder connectionModel;

			public _003C_003Ec__DisplayClass165_0 customerModel;

			private HttpWebRequest queueModel;

			private StreamReader annotationModel;

			private StreamWriter m_ValueModel;

			private TaskAwaiter _RepositoryModel;

			private TaskAwaiter<string> _RefModel;

			private static object EnableClass;

			private void MoveNext()
			{
				int num = _ProcessModel;
				_003C_003Ec__DisplayClass165_0 _003C_003Ec__DisplayClass165_ = customerModel;
				try
				{
					TaskAwaiter<string> awaiter;
					if (num != 0)
					{
						if (num == 1)
						{
							awaiter = _RefModel;
							_RefModel = default(TaskAwaiter<string>);
							num = -1;
							_ProcessModel = -1;
							goto IL_0034;
						}
						queueModel = RevertSystem(_003C_003Ec__DisplayClass165_._ComposerModel);
						m_ValueModel = new StreamWriter(queueModel.GetRequestStream());
					}
					try
					{
						TaskAwaiter awaiter2;
						if (num == 0)
						{
							awaiter2 = _RepositoryModel;
							_RepositoryModel = default(TaskAwaiter);
							num = -1;
							_ProcessModel = -1;
						}
						else
						{
							awaiter2 = m_ValueModel.WriteAsync(_003C_003Ec__DisplayClass165_.codeModel).GetAwaiter();
							if (!awaiter2.IsCompleted)
							{
								num = 0;
								_ProcessModel = 0;
								_RepositoryModel = awaiter2;
								connectionModel.AwaitUnsafeOnCompleted(ref awaiter2, ref this);
								return;
							}
						}
						awaiter2.GetResult();
					}
					finally
					{
						if (num < 0 && m_ValueModel != null)
						{
							((IDisposable)m_ValueModel).Dispose();
						}
					}
					m_ValueModel = null;
					HttpWebResponse httpWebResponse = (HttpWebResponse)queueModel.GetResponse();
					annotationModel = new StreamReader(httpWebResponse.GetResponseStream());
					awaiter = annotationModel.ReadToEndAsync().GetAwaiter();
					if (!awaiter.IsCompleted)
					{
						num = 1;
						_ProcessModel = 1;
						_RefModel = awaiter;
						connectionModel.AwaitUnsafeOnCompleted(ref awaiter, ref this);
						return;
					}
					goto IL_0034;
					IL_0034:
					string result = awaiter.GetResult();
					annotationModel.Dispose();
					_003C_003Ec__DisplayClass165_.facadeModel = new GetterDicBridge(result);
				}
				catch (Exception exception)
				{
					_ProcessModel = -2;
					queueModel = null;
					annotationModel = null;
					connectionModel.SetException(exception);
					return;
				}
				_ProcessModel = -2;
				queueModel = null;
				annotationModel = null;
				connectionModel.SetResult();
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine i)
			{
				connectionModel.SetStateMachine(i);
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine i)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(i);
			}

			internal static bool ForgotClass()
			{
				return EnableClass == null;
			}
		}

		public string _ComposerModel;

		public string codeModel;

		public GetterDicBridge facadeModel;

		private static _003C_003Ec__DisplayClass165_0 ComputeClass;

		[AsyncStateMachine(typeof(AdapterMethodObject))]
		internal Task ChangeSpecification()
		{
			AdapterMethodObject adapterMethodObject = default(AdapterMethodObject);
			adapterMethodObject.connectionModel = AsyncTaskMethodBuilder.Create();
			adapterMethodObject.customerModel = this;
			while (true)
			{
				adapterMethodObject._ProcessModel = -1;
			}
		}

		internal static bool InitClass()
		{
			return ComputeClass == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass179_0
	{
		public GetterDicBridge candidateModel;

		private static _003C_003Ec__DisplayClass179_0 VisitClass;

		internal void RegisterSpecification()
		{
			visitor = candidateModel.StartTest("transfer_email");
			_Filter = true;
		}

		internal static bool DeleteClass()
		{
			return VisitClass == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass46_0
	{
		public bool _ParameterModel;

		public UnityEngine.Object m_ImporterModel;

		public UnityEngine.Object[] m_RegModel;

		public AdvisorDicBridge.ConfigurationDic[] messageModel;

		public int _BridgeModel;

		public int _UtilsModel;

		public Vector3 m_IdentifierModel;

		public float _GlobalModel;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass46_1
	{
		public float _PolicyModel;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass46_2
	{
		public float _DispatcherModel;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass46_3
	{
		public Vector3 m_CollectionModel;

		public Vector3 _ReaderModel;

		public Vector3 m_PoolModel;

		public Vector3 m_WriterModel;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass54_0
	{
		public List<Transform> interpreterModel;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass66_0
	{
		public FieldInfo _AttributeModel;

		internal static _003C_003Ec__DisplayClass66_0 RestartFactory;

		internal async void ResolveAccount()
		{
			try
			{
				int num = 0;
				bool flag2;
				while (true)
				{
					bool num2 = (bool)_AttributeModel.GetValue(null);
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
					ComparatorMethodObject.InterruptProducer();
					ErrorServiceClass.CallTask();
					ConsumerStruct.InsertTask();
					RoleStruct.CalculateError();
				}
			}
			catch (Exception message)
			{
				UnityEngine.Debug.LogError(message);
			}
		}

		internal static bool MoveFactory()
		{
			return RestartFactory == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass86_0
	{
		public SerializedProperty _SingletonModel;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass86_1
	{
		public UnityEditor.Animations.AnimatorController m_ProcModel;
	}

	private static bool m_System;

	private static MethodInfo @struct;

	private static MethodInfo m_Config;

	private static MethodInfo _Model;

	private static MethodInfo m_Template;

	private static MethodInfo _Dic;

	private static MethodInfo test;

	private static MethodInfo _Service;

	private static MethodInfo error;

	private static bool producer;

	private static PhysBoneManager method;

	private static GameObject m_Resolver;

	private static GameObject[] iterator;

	private static GameObject[] _Rules;

	private static GameObject specification;

	private static VRCPhysBone[] m_Account;

	private static bool[] manager;

	private static bool[] m_Param;

	private static VRCPhysBoneCollider[] @event;

	private static bool[] tests;

	private static bool[] request;

	private static readonly int algo = "ADOControlID".GetHashCode();

	private static VRCAvatarDescriptor m_Mapping;

	private static VRCAvatarDescriptor[] m_Definition;

	private static string[] m_Initializer;

	private static string[] _Info;

	private static string[] _Authentication;

	private static int[] m_Client;

	private static bool m_Worker;

	private static bool m_Container;

	private static bool _Property;

	private static bool descriptor;

	private static bool _Factory;

	private static bool tag;

	private static bool _Configuration;

	private static bool _Params;

	private static readonly AdvisorDicBridge.PublisherTemplate _Serializer = new AdvisorDicBridge.PublisherTemplate();

	private static readonly int interceptor = GUIUtility.GetControlID("ADOTooltipDragControlID".GetHashCode(), FocusType.Passive);

	private static Dictionary<UnityEngine.Object, UnityEngine.Object> m_Database = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

	private static Dictionary<UnityEngine.Object, UnityEngine.Object> m_Val = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

	private static Dictionary<UnityEngine.Object, bool> merchant = new Dictionary<UnityEngine.Object, bool>();

	private static bool _Class;

	private static bool _Predicate;

	private static bool _Server;

	private static bool rule;

	private static bool _Role;

	private static string registry;

	private static bool _Strategy;

	private static bool m_Indexer;

	private static string m_Broadcaster;

	private static string _Publisher;

	private static string mapper;

	private static string product;

	private static string setter = "";

	private static string m_Object = "";

	private static string visitor = "";

	private static string status;

	private static bool m_Token;

	private static bool state;

	private static bool _Helper;

	private static bool _Prototype;

	private static float m_Creator;

	private static string m_Getter;

	private static bool _Advisor;

	private static bool m_Consumer;

	private static string _Attr;

	private static string record;

	private static string decorator;

	private static bool m_Invocation;

	private static bool _Exporter;

	private static bool map;

	private static Action _Field;

	private static bool m_Callback;

	private static bool _Filter;

	private static bool m_Proxy;

	private static bool comparator;

	private static bool adapter;

	private static bool _Printer;

	private static bool m_Composer;

	private static bool code;

	private static readonly AnimBool m_Facade = new AnimBool();

	private static readonly AnimBool m_Process = new AnimBool();

	private static readonly SystemTemplate _Connection = new SystemTemplate("0.11.1");

	private static readonly (string, string)[] m_Customer = new(string, string)[0];

	internal static ConfigurationTestStub RestartGlobal;

	private static void ReflectSystem(UnityEngine.Object instance, SerializedProperty[] result, Action serv, bool testtask2)
	{
		SerializedProperty serializedProperty = result[0];
		SerializedProperty property = result[1];
		SerializedProperty serializedProperty2 = result[5];
		SerializedProperty serializedProperty3 = ((!testtask2) ? null : result[6]);
		SerializedProperty asset = (testtask2 ? result[7] : null);
		int intValue = serializedProperty.intValue;
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
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
					_Factory = false;
					_Property = false;
				}
				else if (intValue == 2)
				{
					_Property = false;
					m_Container = false;
				}
				serv();
			}
			if (testtask2 && serializedProperty3 != null)
			{
				using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, serializedProperty3.boolValue, AdvisorDicBridge.workerTemplate, AdvisorDicBridge.m_InitializerTemplate))
				{
					serializedProperty3.boolValue = AdvisorDicBridge.CustomizeManager(serializedProperty3.boolValue, (!serializedProperty3.boolValue) ? "Outside Bounds" : "Inside Bounds", GUI.skin.button, GUILayout.ExpandWidth(expand: false));
				}
			}
		}
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		})())
		{
			return;
		}
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(property, new GUIContent("Root"));
			if (GUILayout.Button(new GUIContent("S", "Set to Self"), GUILayout.Width(18f), GUILayout.Height(18f)))
			{
				Undo.RecordObject(instance, "Set Root to Self");
				UnityEngine.Component component = instance as UnityEngine.Component;
				if ((bool)component)
				{
					SerializedObject serializedObject = new SerializedObject(component);
					serializedObject.FindProperty("rootTransform").objectReferenceValue = component.transform;
					serializedObject.ApplyModifiedProperties();
				}
			}
		}
		EditorGUILayout.Space();
		InsertSystem(result, 0);
		InsertSystem(result, 1);
		InsertSystem(result, 2);
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
				using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, _Factory, Color.green, Color.red))
				{
					_Factory = GUILayout.Toggle(_Factory, AdvisorDicBridge.PrepareRequest().m_PrinterTemplate, AdvisorDicBridge.ManageRequest().modelDic, GUILayout.Width(18f), GUILayout.Height(18f));
				}
			}
		}
		if (testtask2)
		{
			CustomizeSystem(asset);
		}
	}

	private static void CountSystem(UnityEngine.Object first, UnityEngine.Object[] token, int length_util, Color instance2)
	{
		_003C_003Ec__DisplayClass46_0 pool = default(_003C_003Ec__DisplayClass46_0);
		pool.m_ImporterModel = first;
		pool.m_RegModel = token;
		if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Escape))
		{
			_Factory = false;
			descriptor = false;
			_Property = false;
			m_Container = false;
		}
		if (!pool.m_ImporterModel)
		{
			return;
		}
		Handles.color = instance2;
		pool._BridgeModel = 0;
		for (int i = 0; i < pool.m_RegModel.Length; i++)
		{
			if (pool.m_RegModel[i] == pool.m_ImporterModel)
			{
				pool._BridgeModel = i;
				break;
			}
		}
		if (length_util != 0)
		{
			if (length_util == 1)
			{
				pool.messageModel = pool.m_RegModel.Select((UnityEngine.Object t2) => new AdvisorDicBridge.ConfigurationDic((VRCContactSender)t2)).ToArray();
			}
			else
			{
				pool.messageModel = pool.m_RegModel.Select((UnityEngine.Object t2) => new AdvisorDicBridge.ConfigurationDic((VRCContactReceiver)t2)).ToArray();
			}
		}
		else
		{
			pool.messageModel = pool.m_RegModel.Select((UnityEngine.Object t2) => new AdvisorDicBridge.ConfigurationDic((VRCPhysBoneCollider)t2)).ToArray();
		}
		Transform interceptorDic = pool.messageModel[pool._BridgeModel].m_InterceptorDic;
		pool._GlobalModel = AssetSystem(interceptorDic);
		int databaseDic = pool.messageModel[pool._BridgeModel]._DatabaseDic;
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		})())
		{
			return;
		}
		Quaternion quaternion = interceptorDic.rotation * pool.messageModel[pool._BridgeModel]._PredicateDic;
		pool.m_IdentifierModel = interceptorDic.TransformPoint(pool.messageModel[pool._BridgeModel]._ClassDic);
		Vector3 vector = quaternion * Vector3.up;
		float num = pool.messageModel[pool._BridgeModel].merchantDic * 0.5f - pool.messageModel[pool._BridgeModel].m_ValDic;
		float num2 = pool.messageModel[pool._BridgeModel].m_ValDic * pool._GlobalModel;
		Vector3 vector2 = num2 * vector;
		Vector3 vector3 = pool.m_IdentifierModel + Mathf.Max(num * pool._GlobalModel, 0f) * (interceptorDic.rotation * pool.messageModel[pool._BridgeModel]._PredicateDic * Vector3.up);
		Vector3 vector4 = pool.m_IdentifierModel - Mathf.Max(num * pool._GlobalModel, 0f) * (interceptorDic.rotation * pool.messageModel[pool._BridgeModel]._PredicateDic * Vector3.up);
		pool._UtilsModel = (Event.current.shift ? 2 : (Event.current.alt ? 1 : 0));
		pool._ParameterModel = pool._UtilsModel == 1;
		if (descriptor)
		{
			using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
			bool flag = Tools.pivotRotation == PivotRotation.Local;
			Vector3 vector5 = Handles.PositionHandle(pool.m_IdentifierModel, flag ? quaternion : Quaternion.identity);
			if (changeCheckScope.changed)
			{
				if (pool._ParameterModel)
				{
					Undo.RecordObject(pool.m_ImporterModel, "Adjust Position");
				}
				else
				{
					Undo.RecordObjects(pool.m_RegModel, "Adjust Position");
				}
				Vector3 vector6 = vector5 - pool.m_IdentifierModel;
				if (flag || pool._UtilsModel != 0)
				{
					vector6 = interceptorDic.InverseTransformVector(vector6);
				}
				switch (pool._UtilsModel)
				{
				case 2:
				{
					pool.messageModel[pool._BridgeModel]._ClassDic += vector6;
					for (int num4 = 0; num4 < pool.messageModel.Length; num4++)
					{
						pool.messageModel[num4]._ClassDic = pool.messageModel[pool._BridgeModel]._ClassDic;
					}
					break;
				}
				case 0:
				{
					for (int num3 = 0; num3 < pool.messageModel.Length; num3++)
					{
						if (!flag)
						{
							pool.messageModel[num3]._ClassDic += pool.messageModel[num3].m_InterceptorDic.InverseTransformVector(vector6);
						}
						else if (pool.messageModel[num3].m_ParamsDic == pool.messageModel[pool._BridgeModel].m_ParamsDic)
						{
							pool.messageModel[pool._BridgeModel]._ClassDic += vector6;
						}
						else
						{
							pool.messageModel[num3]._ClassDic += pool.messageModel[num3]._PredicateDic * Quaternion.Inverse(pool.messageModel[pool._BridgeModel]._PredicateDic) * vector6;
						}
					}
					break;
				}
				case 1:
					pool.messageModel[pool._BridgeModel]._ClassDic += vector6;
					break;
				}
			}
		}
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		})())
		{
			return;
		}
		if (_Factory && databaseDic != 0)
		{
			using EditorGUI.ChangeCheckScope changeCheckScope2 = new EditorGUI.ChangeCheckScope();
			Quaternion quaternion2 = Handles.RotationHandle(quaternion, pool.m_IdentifierModel);
			if (changeCheckScope2.changed)
			{
				if (pool._ParameterModel)
				{
					Undo.RecordObject(pool.m_ImporterModel, "Adjust Rotation");
				}
				else
				{
					Undo.RecordObjects(pool.m_RegModel, "Adjust Rotation");
				}
				Quaternion predicateDic = Quaternion.Euler((Quaternion.Inverse(interceptorDic.rotation) * quaternion2).eulerAngles);
				switch (pool._UtilsModel)
				{
				case 0:
				case 2:
				{
					for (int num5 = 0; num5 < pool.messageModel.Length; num5++)
					{
						pool.messageModel[num5]._PredicateDic = predicateDic;
					}
					break;
				}
				case 1:
					pool.messageModel[pool._BridgeModel]._PredicateDic = predicateDic;
					break;
				}
			}
		}
		if (m_Container && databaseDic != 2)
		{
			bool flag2 = databaseDic == 1;
			_003C_003Ec__DisplayClass46_1 consumer = default(_003C_003Ec__DisplayClass46_1);
			using (EditorGUI.ChangeCheckScope changeCheckScope3 = new EditorGUI.ChangeCheckScope())
			{
				Vector3 position = (flag2 ? vector3 : pool.m_IdentifierModel);
				Quaternion rotation = (flag2 ? quaternion : Quaternion.identity);
				consumer._PolicyModel = Handles.RadiusHandle(rotation, position, num2, handlesOnly: true) / pool._GlobalModel;
				InitStruct(changeCheckScope3.changed, ref pool, ref consumer);
			}
			if (flag2)
			{
				using EditorGUI.ChangeCheckScope changeCheckScope4 = new EditorGUI.ChangeCheckScope();
				consumer._PolicyModel = Handles.RadiusHandle(quaternion, vector4, num2, handlesOnly: true) / pool._GlobalModel;
				InitStruct(changeCheckScope4.changed, ref pool, ref consumer);
			}
		}
		if (_Property && databaseDic == 1)
		{
			if (!((Func<bool>)delegate
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
				return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
			})())
			{
				return;
			}
			_003C_003Ec__DisplayClass46_3 col = default(_003C_003Ec__DisplayClass46_3);
			col.m_PoolModel = Vector3.zero;
			col.m_WriterModel = Vector3.zero;
			col.m_CollectionModel = vector3 + vector2;
			col._ReaderModel = vector4 - vector2;
			using (EditorGUI.ChangeCheckScope changeCheckScope5 = new EditorGUI.ChangeCheckScope())
			{
				col.m_PoolModel = Handles.Slider(col.m_CollectionModel, vector);
				CancelStruct(changeCheckScope5.changed, writeivk: true, ref pool, ref col);
			}
			using (EditorGUI.ChangeCheckScope changeCheckScope6 = new EditorGUI.ChangeCheckScope())
			{
				col.m_WriterModel = Handles.Slider(col._ReaderModel, vector * -1f);
				CancelStruct(changeCheckScope6.changed, writeivk: false, ref pool, ref col);
			}
			if (!((Func<bool>)delegate
			{
				using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
				return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
			})())
			{
				return;
			}
		}
		AdvisorDicBridge.ConfigurationDic[] array = pool.messageModel;
		foreach (AdvisorDicBridge.ConfigurationDic configurationDic in array)
		{
			configurationDic.UpdateAlgo();
		}
	}

	private static void SetSystem(SceneView var1)
	{
		if (!descriptor && !_Factory && !m_Container && !_Property)
		{
			return;
		}
		Tools.hidden = true;
		if (((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		})())
		{
			int num = 1;
			if (tag)
			{
				num++;
			}
			if (_Configuration)
			{
				num++;
			}
			if (_Params)
			{
				num++;
			}
			InterruptStruct(var1, "Editing", DeleteSystem, 200f, 45 + 20 * num);
		}
	}

	private static void DeleteSystem()
	{
		if (tag)
		{
			GetSystem("Radius", ref m_Container);
		}
		if (_Configuration)
		{
			GetSystem("Height", ref _Property);
		}
		GetSystem("Position", ref descriptor);
		if (_Params)
		{
			GetSystem("Rotation", ref _Factory);
		}
	}

	private static void NewSystem(SceneView value)
	{
		if (!producer)
		{
			return;
		}
		Tools.hidden |= ManagerStruct.SearchTest().hideToolsDuringTesting;
		EditorApplication.playModeStateChanged -= ReadSystem;
		EditorApplication.playModeStateChanged += ReadSystem;
		if (m_Resolver != null)
		{
			AdvisorDicBridge.ConcatManager(m_Resolver.transform, stripmap: true, writetag: true, isres2: false, isv3: false, movecounter4: false, insertasset5: true);
		}
		ComputeStruct(value, delegate
		{
			using (new GUILayout.HorizontalScope())
			{
				bool ordclose;
				string tooltip = ((ordclose = ManagerStruct.SearchTest().hideToolsDuringTesting) ? "Native tools are hidden during test." : "Native tools are visible during test.");
				using (new TaskServiceClass(TaskServiceClass.ColoringType.FG, ordclose, AdvisorDicBridge.m_InitializerTemplate, AdvisorDicBridge.infoTemplate))
				{
					if (AdvisorDicBridge.RateManager(new GUIContent(AdvisorDicBridge.PrepareRequest().repositoryTemplate)
					{
						tooltip = tooltip
					}))
					{
						ManagerStruct.SearchTest().hideToolsDuringTesting.InstantiateTest();
						Tools.hidden = false;
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.Label("Testing", AdvisorDicBridge.ManageRequest()._WriterTemplate);
				Rect lastRect = GUILayoutUtility.GetLastRect();
				GUILayout.FlexibleSpace();
				StartStruct();
				return lastRect;
			}
		}, delegate
		{
			using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, AdvisorDicBridge.infoTemplate))
			{
				if (AdvisorDicBridge.GetManager("Stop Testing") || AdvisorDicBridge.SetupManager() || AdvisorDicBridge.SortManager())
				{
					StopSystem();
				}
			}
			using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, AdvisorDicBridge._ClientTemplate))
			{
				if (AdvisorDicBridge.GetManager("Restart"))
				{
					WriteSystem();
				}
			}
			using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, _Class, AdvisorDicBridge.m_InitializerTemplate))
			{
				using (new EditorGUI.DisabledScope(!_Class))
				{
					if (AdvisorDicBridge.GetManager("Apply All Changes"))
					{
						foreach (UnityEngine.Object item in merchant.Keys.ToList())
						{
							if (merchant[item])
							{
								UnityEngine.Object obj = m_Val[item];
								if (obj != null)
								{
									Undo.RecordObject(obj, "ADO - Apply Changes");
									EditorUtility.CopySerialized(item, obj);
									merchant[item] = false;
								}
							}
						}
						_Class = false;
						CallSystem();
					}
				}
			}
		}, 200f, 104f);
	}

	private static void SelectSystem()
	{
		if (!method)
		{
			StopSystem();
			return;
		}
		@struct.Invoke(method, null);
		for (int i = 0; i < m_Account.Length; i++)
		{
			if (!m_Account[i])
			{
				if (manager[i])
				{
					manager[i] = false;
					_Dic.Invoke(m_Account[i], null);
				}
				continue;
			}
			bool flag = m_Account[i].enabled && m_Account[i].gameObject.activeInHierarchy;
			if (manager[i] == flag)
			{
				continue;
			}
			manager[i] = flag;
			if (flag)
			{
				m_Template.Invoke(m_Account[i], null);
				if (!m_Param[i])
				{
					m_Param[i] = true;
					_Model.Invoke(m_Account[i], null);
				}
			}
			else
			{
				_Dic.Invoke(m_Account[i], null);
			}
		}
		for (int j = 0; j < @event.Length; j++)
		{
			if (!@event[j])
			{
				if (tests[j])
				{
					tests[j] = false;
					error.Invoke(@event[j], null);
				}
				continue;
			}
			bool flag2 = @event[j].enabled && @event[j].gameObject.activeInHierarchy;
			if (tests[j] == flag2)
			{
				continue;
			}
			tests[j] = flag2;
			if (!flag2)
			{
				error.Invoke(@event[j], null);
				continue;
			}
			_Service.Invoke(@event[j], null);
			if (!request[j])
			{
				request[j] = true;
				test.Invoke(@event[j], null);
			}
		}
	}

	private static void RunSystem()
	{
		if (!m_System)
		{
			m_System = true;
			@struct = ((ConfigurationTestStub)(object)typeof(PhysBoneManager)).RateConfig("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
			m_Config = ((ConfigurationTestStub)(object)typeof(PhysBoneManager)).RateConfig("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic);
			_Model = ((ConfigurationTestStub)(object)typeof(VRCPhysBoneBase)).RateConfig("Start", BindingFlags.Instance | BindingFlags.NonPublic);
			m_Template = ((ConfigurationTestStub)(object)typeof(VRCPhysBoneBase)).RateConfig("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
			_Dic = ((ConfigurationTestStub)(object)typeof(VRCPhysBoneBase)).RateConfig("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
			test = ((ConfigurationTestStub)(object)typeof(VRCPhysBoneColliderBase)).RateConfig("Start", BindingFlags.Instance | BindingFlags.NonPublic);
			_Service = ((ConfigurationTestStub)(object)typeof(VRCPhysBoneColliderBase)).RateConfig("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
			error = ((ConfigurationTestStub)(object)typeof(VRCPhysBoneColliderBase)).RateConfig("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
		}
	}

	private static void StopSystem()
	{
		RunSystem();
		producer = !producer;
		if (Application.isPlaying)
		{
			producer = false;
		}
		if (!producer)
		{
			PushSystem();
		}
		else
		{
			DefineSystem();
		}
	}

	private static void WriteSystem()
	{
		if (producer)
		{
			StopSystem();
		}
		StopSystem();
	}

	private static void DefineSystem()
	{
		_Server |= ManagerStruct.SearchTest().hasReadColliderTestingWarning;
		_Rules = Selection.gameObjects;
		specification = Selection.activeGameObject;
		m_Database = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
		m_Val = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
		merchant = new Dictionary<UnityEngine.Object, bool>();
		_Class = false;
		_003C_003Ec__DisplayClass54_0 pred = default(_003C_003Ec__DisplayClass54_0);
		pred.interpreterModel = new List<Transform>();
		iterator = Selection.transforms.Select((Transform t) => t.root.gameObject).Distinct().ToArray();
		VRCPhysBone[] componentsToFind = iterator.SelectMany((GameObject o) => o.GetComponentsInChildren<VRCPhysBone>(includeInactive: true)).ToArray();
		VRCPhysBoneColliderBase[] componentsToFind2 = iterator.SelectMany((GameObject o) => o.GetComponentsInChildren<VRCPhysBoneColliderBase>(includeInactive: true)).ToArray();
		if (iterator.Length == 0)
		{
			StopStruct("No Active Objects with PhysBones found in the scene.", CustomLogType.Error);
			return;
		}
		m_Resolver = GameObject.Find("Physbone Tester");
		if ((bool)m_Resolver)
		{
			UnityEngine.Object.DestroyImmediate(m_Resolver);
		}
		m_Resolver = new GameObject("Physbone Tester")
		{
			hideFlags = (HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild)
		};
		m_Resolver.transform.position = specification.transform.position;
		GameObject[] array = iterator;
		foreach (GameObject gameObject in array)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation, m_Resolver.transform);
			Dictionary<VRCPhysBone, VRCPhysBone> dictionary = AdvisorDicBridge.ConnectManager(gameObject.transform, gameObject2.transform, isrole: true, componentsToFind);
			Dictionary<VRCPhysBoneColliderBase, VRCPhysBoneColliderBase> dictionary2 = AdvisorDicBridge.ConnectManager(gameObject.transform, gameObject2.transform, isrole: true, componentsToFind2);
			VRCPhysBone component = specification.GetComponent<VRCPhysBone>();
			if (!(component != null) || !dictionary.TryGetValue(component, out var value) || !(value != null))
			{
				VRCPhysBoneColliderBase component2 = specification.GetComponent<VRCPhysBoneColliderBase>();
				if (component2 != null && dictionary2.TryGetValue(component2, out var value2) && value2 != null)
				{
					Selection.activeGameObject = value2.gameObject;
				}
			}
			else
			{
				Selection.activeGameObject = value.gameObject;
			}
			DisableStruct(dictionary, ref pred);
			DisableStruct(dictionary2, ref pred);
			gameObject.SetActive(value: false);
		}
		method = m_Resolver.AddComponent<PhysBoneManager>();
		PhysBoneManager.Inst = method;
		method.IsSDK = true;
		method.Init();
		m_Account = m_Resolver.GetComponentsInChildren<VRCPhysBone>(includeInactive: true);
		manager = new bool[m_Account.Length];
		m_Param = new bool[m_Account.Length];
		@event = m_Resolver.GetComponentsInChildren<VRCPhysBoneCollider>(includeInactive: true);
		tests = new bool[@event.Length];
		request = new bool[@event.Length];
		UnityEngine.Object[] objects = pred.interpreterModel.Select((Transform t) => t.gameObject).ToArray();
		Selection.objects = objects;
		EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(SelectSystem));
		EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(SelectSystem));
		SceneView.duringSceneGui -= NewSystem;
		SceneView.duringSceneGui += NewSystem;
	}

	private static void PushSystem()
	{
		EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(SelectSystem));
		SceneView.duringSceneGui -= NewSystem;
		UnityEngine.Object[] rules = _Rules;
		Selection.objects = rules;
		Selection.activeObject = specification;
		m_Config.Invoke(method, null);
		if ((bool)m_Resolver)
		{
			UnityEngine.Object.DestroyImmediate(m_Resolver);
		}
		foreach (GameObject item in iterator.Where((GameObject o) => o))
		{
			item.SetActive(value: true);
		}
		m_Database = (m_Val = null);
		merchant = null;
		_Predicate = false;
		_Class = false;
	}

	private static void UpdateSystem(bool isparam, bool readcaller, bool countutil)
	{
		tag = isparam;
		_Configuration = readcaller;
		_Params = countutil;
		if (!tag)
		{
			m_Container = false;
		}
		if (!_Configuration)
		{
			_Property = false;
		}
		if (!_Params)
		{
			_Factory = false;
		}
	}

	private static void InsertSystem(SerializedProperty[] value, int map_min)
	{
		int intValue = value[0].intValue;
		switch (map_min)
		{
		case 2:
			descriptor = TestSystem(value[4], descriptor);
			break;
		case 1:
			if (intValue == 1)
			{
				_Property = TestSystem(value[3], _Property);
			}
			break;
		case 0:
			if (intValue != 2)
			{
				m_Container = TestSystem(value[2], m_Container);
			}
			break;
		}
	}

	private static void PrepareSystem(SerializedProperty key, Rect result, int indexOf_rule)
	{
		if (indexOf_rule >= key.arraySize || indexOf_rule < 0)
		{
			return;
		}
		SerializedProperty arrayElementAtIndex = key.GetArrayElementAtIndex(indexOf_rule);
		result.y += 1f;
		result.height = 18f;
		result.width -= 44f;
		Rect position = result;
		position.width = 21f;
		Rect rect = result;
		rect.x += 22f;
		rect.width -= 12f;
		Rect position2 = rect;
		position2.x += rect.width;
		position2.width = 28f;
		if (!((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		})())
		{
			return;
		}
		using (new EditorGUI.DisabledScope(!(UnityEngine.Object)(object)m_Mapping))
		{
			int num = EditorGUI.Popup(position, -1, _Info);
			if (num != -1)
			{
				arrayElementAtIndex.stringValue = Regex.Replace(_Info[num], "^Default/", string.Empty);
			}
		}
		EditorGUI.PropertyField(rect, arrayElementAtIndex, GUIContent.none);
		if (GUI.Button(position2, AdvisorDicBridge.PrepareRequest().m_ProcessTemplate, AdvisorDicBridge.ManageRequest().dicDic))
		{
			key.DeleteArrayElementAtIndex(indexOf_rule);
		}
	}

	private static void ListSystem(Rect info)
	{
		GUIStyle style = new GUIStyle("boldlabel");
		GUI.Label(info, "Collision Tags", style);
	}

	private static void ManageSystem(Action setup)
	{
		ResolveSystem(compareres: true);
		setup();
		CheckSystem(ref m_Mapping, ref m_Definition);
		VerifySystem();
	}

	private static void ReadSystem(PlayModeStateChange config)
	{
		if (config == PlayModeStateChange.ExitingEditMode && producer)
		{
			StopSystem();
		}
	}

	private static void ResolveSystem(bool compareres)
	{
		SceneView.duringSceneGui -= SetSystem;
		if (!compareres)
		{
			Tools.hidden = false;
		}
		else
		{
			SceneView.duringSceneGui += SetSystem;
		}
	}

	private static void VerifySystem()
	{
		AdvisorDicBridge.ListParam(m_Mapping, ref _Authentication, ref m_Client);
		if (!(UnityEngine.Object)(object)m_Mapping)
		{
			m_Initializer = Array.Empty<string>();
			_Info = Array.Empty<string>();
			return;
		}
		ConnectSystem();
		_Info = ((UnityEngine.Component)(object)m_Mapping).GetComponentsInChildren<VRCContactSender>().SelectMany((VRCContactSender cs) => cs.collisionTags).Concat(((UnityEngine.Component)(object)m_Mapping).GetComponentsInChildren<VRCContactReceiver>().SelectMany((VRCContactReceiver cr) => cr.collisionTags))
			.Except(AdvisorDicBridge.m_IndexerTemplate)
			.Concat(AdvisorDicBridge.m_IndexerTemplate.Select((string s) => "Default/" + s))
			.Distinct()
			.ToArray();
	}

	private static void ConnectSystem()
	{
		m_Initializer = (from p in (from rc in m_Mapping.baseAnimationLayers.Concat(m_Mapping.specialAnimationLayers)
				where !rc.isDefault && (bool)rc.animatorController
				select AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(AssetDatabase.GetAssetPath(rc.animatorController)) into c
				where c
				select c).SelectMany((UnityEditor.Animations.AnimatorController c) => c.parameters)
			select p.name into p
			where !AdvisorDicBridge.strategyTemplate.Contains(p)
			select p).Distinct().ToArray();
	}

	private static void CompareSystem(AnimBool[] item, UnityAction result)
	{
		for (int i = 0; i < item.Length; i++)
		{
			if (item[i] != null)
			{
				item[i] = new AnimBool(item[i].target);
			}
			else
			{
				item[i] = new AnimBool();
			}
			item[i].valueChanged.AddListener(result);
		}
	}

	[DidReloadScripts]
	private static void InterruptSystem()
	{
		_003C_003Ec__DisplayClass66_0 _003C_003Ec__DisplayClass66_ = new _003C_003Ec__DisplayClass66_0();
		Type type = Type.GetType("UnityEditor.CustomEditorAttributes, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		_003C_003Ec__DisplayClass66_._AttributeModel = type.GetField("s_Initialized", BindingFlags.Static | BindingFlags.NonPublic);
		_003C_003Ec__DisplayClass66_.ResolveAccount();
	}

	internal static bool ComputeSystem()
	{
		StopStruct(m_Consumer ? "Please wait for verification." : "Please activate your license.", CustomLogType.Error, !m_Invocation);
		return m_Invocation;
	}

	internal static void StartSystem(string[] instance, int[] visitor, string[] proc, out string[] vis2, out int[] pol3)
	{
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < instance.Length; i++)
		{
			for (int j = 0; j < proc.Length; j++)
			{
				list.Add(instance[i] + "/" + proc[j]);
				list2.Add(int.Parse($"{visitor[i]}{j}"));
			}
		}
		vis2 = list.ToArray();
		pol3 = list2.ToArray();
	}

	internal static int[] InitSystem(int end_spec, int visitoramount)
	{
		string text = end_spec.ToString();
		int[] array = new int[visitoramount];
		int num = visitoramount - text.Length;
		int num2 = 0;
		for (int i = 0; i < text.Length; i++)
		{
			array[i] = ((i >= num) ? (text[num2++] - 48) : 0);
		}
		return array;
	}

	internal static void CheckSystem(ref VRCAvatarDescriptor def, ref VRCAvatarDescriptor[] ivk, Action c = null, Func<VRCAvatarDescriptor, bool> reference2 = null)
	{
		ivk = UnityEngine.Object.FindObjectsOfType<VRCAvatarDescriptor>();
		if ((bool)(UnityEngine.Object)(object)def)
		{
			return;
		}
		if (ivk.Length != 0)
		{
			if (reference2 != null)
			{
				def = ivk.FirstOrDefault(reference2) ?? ivk[0];
			}
			else
			{
				def = ivk[0];
			}
		}
		c?.Invoke();
	}

	internal static bool CancelSystem(ref VRCAvatarDescriptor var1, VRCAvatarDescriptor[] selection, Action filter = null, bool explicitvar12 = true, bool iscol3 = true, bool validatereg4 = true, string token5 = "Avatar", string x6 = "The Targeted VRCAvatar", Action item7 = null)
	{
		if ((bool)(UnityEngine.Object)(object)DisableSystem(ref var1, selection, filter, token5, x6, item7))
		{
			return IncludeSystem(var1, explicitvar12, iscol3, validatereg4);
		}
		return false;
	}

	private static VRCAvatarDescriptor DisableSystem(ref VRCAvatarDescriptor last, VRCAvatarDescriptor[] selection, Action proc = null, string key2 = "Avatar", string pol3 = "The Targeted VRCAvatar", Action init4 = null)
	{
		using (new GUILayout.HorizontalScope())
		{
			GUIContent label = new GUIContent(key2, pol3);
			if (selection == null || selection.Length == 0)
			{
				EditorGUILayout.LabelField(label, new GUIContent("No Avatar Descriptors Found"));
			}
			else
			{
				using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
				int num = EditorGUILayout.Popup(label, ((UnityEngine.Object)(object)last) ? Array.IndexOf(selection, last) : (-1), (from x in selection
					where (UnityEngine.Object)(object)x
					select ((UnityEngine.Object)(object)x).name).ToArray());
				if (changeCheckScope.changed)
				{
					last = selection[num];
					EditorGUIUtility.PingObject((UnityEngine.Object)(object)last);
					proc?.Invoke();
				}
			}
			init4?.Invoke();
		}
		return last;
	}

	private static bool IncludeSystem(VRCAvatarDescriptor value, bool getcol = true, bool verifyfield = true, bool iskey2 = true)
	{
		if (!verifyfield || !RateSystem(value))
		{
			if (iskey2)
			{
				return !ForgotSystem(value, getcol);
			}
			return true;
		}
		return false;
	}

	private static bool RateSystem(VRCAvatarDescriptor ident)
	{
		if (!(UnityEngine.Object)(object)ident)
		{
			return false;
		}
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

	private static bool ForgotSystem(VRCAvatarDescriptor last, bool addcfg = true)
	{
		if ((bool)(UnityEngine.Object)(object)last)
		{
			VRCAvatarDescriptor.CustomAnimLayer[] baseAnimationLayers = last.baseAnimationLayers;
			if (baseAnimationLayers.Length > 3)
			{
				bool num = baseAnimationLayers[3].type == baseAnimationLayers[4].type;
				if (num)
				{
					EditorGUILayout.HelpBox("Your Avatar's Action playable layer is set as FX. This is an uncommon bug.", MessageType.Error);
					if (GUILayout.Button("Fix"))
					{
						last.baseAnimationLayers[3].type = VRCAvatarDescriptor.AnimLayerType.Action;
						EditorUtility.SetDirty((UnityEngine.Object)(object)last);
					}
				}
				return num;
			}
			if (addcfg)
			{
				EditorGUILayout.HelpBox("Your Avatar's descriptor is set as Non-Humanoid! Please make sure that your Avatar's rig is Humanoid.", MessageType.Error);
			}
			return addcfg;
		}
		return false;
	}

	private static float AssetSystem(Transform ident)
	{
		return Mathf.Max(ident.lossyScale.x, ident.lossyScale.y, ident.lossyScale.z);
	}

	private static bool TestSystem(SerializedProperty asset, bool checkord)
	{
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(asset);
			return ResetSystem(checkord, AdvisorDicBridge.PrepareRequest().m_PrinterTemplate);
		}
	}

	private static bool ResetSystem(bool isi, GUIContent ivk)
	{
		using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, isi, AdvisorDicBridge.m_InitializerTemplate, AdvisorDicBridge.infoTemplate))
		{
			isi = AdvisorDicBridge.ChangeManager(isi, ivk, AdvisorDicBridge.ManageRequest().modelDic, GUILayout.Width(18f), GUILayout.Height(18f));
			return isi;
		}
	}

	private static void GetSystem(string v, ref bool b, params GUILayoutOption[] options)
	{
		VisitSystem(new GUIContent(v), ref b, options);
	}

	private static void VisitSystem(GUIContent key, ref bool pol, params GUILayoutOption[] options)
	{
		using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, pol, AdvisorDicBridge.m_InitializerTemplate, AdvisorDicBridge.infoTemplate))
		{
			pol = AdvisorDicBridge.ChangeManager(pol, key, GUI.skin.button, options);
		}
	}

	private static void AwakeSystem(SerializedProperty ident, string vis, Action state = null, params GUILayoutOption[] options)
	{
		InvokeSystem(ident, new GUIContent(vis), state, options);
	}

	private static void InvokeSystem(SerializedProperty asset, GUIContent vis, Action field = null, params GUILayoutOption[] options)
	{
		int row_ord = (asset.hasMultipleDifferentValues ? 2 : (asset.boolValue ? 1 : 0));
		using EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope();
		bool boolValue;
		using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, row_ord, AdvisorDicBridge.ManageRequest().configDic))
		{
			boolValue = AdvisorDicBridge.ChangeManager(asset.boolValue, vis, GUI.skin.button, options);
		}
		if (changeCheckScope.changed)
		{
			asset.boolValue = boolValue;
			field?.Invoke();
		}
	}

	private static void CustomizeSystem(SerializedProperty asset)
	{
		if (asset != null)
		{
			EditorGUILayout.PropertyField(asset);
		}
	}

	private static void MoveSystem(SerializedProperty res, SerializedProperty cont)
	{
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PrefixLabel(new GUIContent(res.displayName, res.tooltip));
			SerializedProperty serializedProperty = cont.FindPropertyRelative("allowSelf");
			SerializedProperty serializedProperty2 = cont.FindPropertyRelative("allowOthers");
			bool flag = res.enumValueIndex == 1 || (res.enumValueIndex != 0 && serializedProperty.boolValue);
			bool flag2 = res.enumValueIndex == 1 || (res.enumValueIndex != 0 && serializedProperty2.boolValue);
			EditorGUI.BeginChangeCheck();
			EditorGUIUtility.labelWidth = 50f;
			using (new CandidateDic(res.hasMultipleDifferentValues || (res.enumValueIndex == 2 && serializedProperty.hasMultipleDifferentValues)))
			{
				flag = EditorGUILayout.Toggle("Self", flag);
			}
			using (new CandidateDic(res.hasMultipleDifferentValues || (res.enumValueIndex == 2 && serializedProperty2.hasMultipleDifferentValues)))
			{
				flag2 = EditorGUILayout.Toggle("Others", flag2);
			}
			EditorGUIUtility.labelWidth = 160f;
			if (EditorGUI.EndChangeCheck())
			{
				res.enumValueIndex = 2;
				serializedProperty.boolValue = flag;
				serializedProperty2.boolValue = flag2;
			}
		}
	}

	private static void FillSystem()
	{
		CancelSystem(ref m_Mapping, m_Definition, VerifySystem, explicitvar12: false, iscol3: false, validatereg4: true, "Target Avatar");
	}

	private static void ChangeSystem(SerializedProperty init)
	{
		_003C_003Ec__DisplayClass86_0 col = default(_003C_003Ec__DisplayClass86_0);
		col._SingletonModel = init;
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PropertyField(col._SingletonModel);
			int selectedIndex = -1;
			using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
			{
				selectedIndex = EditorGUILayout.Popup(selectedIndex, m_Initializer, "textfielddropdown", GUILayout.Width(18f));
				if (changeCheckScope.changed)
				{
					col._SingletonModel.stringValue = m_Initializer[selectedIndex];
				}
			}
			if (col._SingletonModel.hasMultipleDifferentValues || string.IsNullOrEmpty(col._SingletonModel.stringValue))
			{
				return;
			}
			Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Width(50f));
			StartSystem(_Authentication, m_Client, new string[3] { "Bool", "Int", "Float" }, out var vis, out var pol);
			EditorGUI.BeginChangeCheck();
			int end_spec = EditorGUI.IntPopup(controlRect, -1, vis, pol);
			if (EditorGUI.EndChangeCheck())
			{
				int[] array = InitSystem(end_spec, 2);
				_003C_003Ec__DisplayClass86_1 helper = default(_003C_003Ec__DisplayClass86_1);
				if (m_Mapping.ManageParam((VRCAvatarDescriptor.AnimLayerType)array[0], out helper.m_ProcModel))
				{
					switch (array[1])
					{
					case 2:
						IncludeStruct(helper.m_ProcModel.LogoutAccount(col._SingletonModel.stringValue, UnityEngine.AnimatorControllerParameterType.Float, 0f), ref col, ref helper);
						break;
					case 1:
						IncludeStruct(helper.m_ProcModel.LogoutAccount(col._SingletonModel.stringValue, UnityEngine.AnimatorControllerParameterType.Int, 0f), ref col, ref helper);
						break;
					case 0:
						IncludeStruct(helper.m_ProcModel.LogoutAccount(col._SingletonModel.stringValue, UnityEngine.AnimatorControllerParameterType.Bool, 0f), ref col, ref helper);
						break;
					}
				}
				else
				{
					StopStruct("Couldn't fetch selected playable layer!", CustomLogType.Error);
				}
			}
			controlRect.x += 3f;
			GUI.Label(controlRect, "Add");
		}
	}

	private static bool CalculateSystem(IEnumerable<UnityEngine.Object> init)
	{
		using (new GUILayout.HorizontalScope())
		{
			using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, producer, AdvisorDicBridge.infoTemplate))
			{
				bool isPlaying;
				string v = ((isPlaying = Application.isPlaying) ? "Editor is in PlayMode" : ((!producer) ? "Test PhysBones in Scene" : "Stop Testing - ESC / Enter"));
				using (new EditorGUI.DisabledScope(isPlaying))
				{
					if (AdvisorDicBridge.GetManager(v))
					{
						StopSystem();
					}
				}
			}
			if (!producer)
			{
				return false;
			}
			using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, AdvisorDicBridge._ClientTemplate))
			{
				if (AdvisorDicBridge.GetManager("Restart", GUILayout.ExpandWidth(expand: false)))
				{
					WriteSystem();
				}
			}
			UnityEngine.Object[] array = init.Where((UnityEngine.Object b) => b != null && merchant.ContainsKey(b) && m_Val[b] != null).ToArray();
			bool flag = array.Any((UnityEngine.Object b) => merchant[b]);
			using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, flag, AdvisorDicBridge.m_InitializerTemplate))
			{
				using (new EditorGUI.DisabledScope(!flag))
				{
					if (AdvisorDicBridge.GetManager("Apply Changes", GUILayout.ExpandWidth(expand: false)))
					{
						UnityEngine.Object[] array2 = array;
						foreach (UnityEngine.Object obj in array2)
						{
							UnityEngine.Object obj2 = m_Val[obj];
							using (new MessageServiceSerializer(obj2, false, "rootTransform", "ignoreTransforms", "colliders"))
							{
								Undo.RecordObject(obj2, "ADO - Apply Changes");
								EditorUtility.CopySerialized(obj, obj2);
								merchant[obj] = false;
							}
						}
						return true;
					}
				}
			}
		}
		return false;
	}

	private static bool PopSystem<T>(SerializedObject value, IEnumerable<T> caller, Action<T> template = null) where T : UnityEngine.Object
	{
		if ((bool)value.targetObject)
		{
			bool hasModifiedProperties;
			if (hasModifiedProperties = value.hasModifiedProperties)
			{
				foreach (T item in caller)
				{
					template?.Invoke(item);
					if (producer && merchant.ContainsKey(item))
					{
						merchant[item] = true;
						_Class = true;
					}
				}
			}
			value.ApplyModifiedProperties();
			return hasModifiedProperties;
		}
		return false;
	}

	private static void CallSystem()
	{
		if (producer && _Predicate && !_Server)
		{
			_Server = true;
			switch (EditorUtility.DisplayDialogComplex("Testing Restart Required", "Collider changes require a restart of the testing process. Do you want to restart testing?", "Yes", "No", "Don't ask again"))
			{
			case 2:
				ManagerStruct.SearchTest().hasReadColliderTestingWarning.ListService(useres: true);
				break;
			case 0:
				WriteSystem();
				break;
			}
		}
	}

	private static void PostSystem(Action setup)
	{
		if (m_Consumer || m_Proxy)
		{
			return;
		}
		EditorGUILayout.HelpBox("This is 'Avatar Dynamics Overhaul'. If you don't know what this is, you may have imported it from a package that shouldn't contain it. You can delete the editor script to revert back to original behaviour. Usually found in Packages > DreadScripts - Avatar Dynamics Overhaul. If this is the case, please notify the package creator about this.", MessageType.Warning);
		using (new GUILayout.HorizontalScope())
		{
			if (AdvisorDicBridge.ResetManager("Locate", EditorStyles.toolbarButton))
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
				if (!obj)
				{
					EditorUtility.DisplayDialog("Not Found", "Couldn't locate the script automatically.", "Ok");
				}
				else
				{
					EditorGUIUtility.PingObject(obj);
				}
			}
			if (AdvisorDicBridge.ResetManager("Info", EditorStyles.toolbarButton))
			{
				Application.OpenURL("https://linktr.ee/Dreadrith");
			}
			if (AdvisorDicBridge.ResetManager("Switch Editor", EditorStyles.toolbarButton))
			{
				setup();
			}
		}
	}

	[SpecialName]
	private static void CollectTemplate(bool isres)
	{
		bool flag = rule;
		rule = isres;
		if (!rule && flag)
		{
			WorkerModelDispatcher.InterruptDic(null);
		}
	}

	private static void LoginSystem()
	{
		EnableSystem("Send Feedback for ADOverhaul", "If you have a suggestion, preference, or something to comment, please send it here!\nNote that the feedback is not anonymous. Abuse may result in blacklisting.");
		_Strategy = m_Invocation;
		m_Broadcaster = EditorGUILayout.TextArea(m_Broadcaster, GUILayout.MinHeight(54f));
		using (new GUILayout.HorizontalScope())
		{
			if (AdvisorDicBridge.ResetManager("Cancel", EditorStyles.toolbarButton, GUILayout.ExpandWidth(expand: false)))
			{
				_Strategy = false;
			}
			using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(m_Broadcaster) || m_Indexer))
			{
				if (AdvisorDicBridge.ResetManager("Send Feedback", EditorStyles.toolbarButton))
				{
					if (m_Broadcaster.Length > 2000)
					{
						m_Broadcaster = m_Broadcaster.Substring(0, 2000);
					}
					List<(string, string)> list = PrintSystem("sendfeedback", new(string, string)[1] { ("feedback", Uri.EscapeUriString(m_Broadcaster)) });
					FindSystem(list);
					m_Indexer = true;
					CountStruct(InstantiateSystem(list.ToArray())).PublishAccount(SortSystem, UnityEngine.Debug.LogException, null, null, delegate
					{
						m_Indexer = false;
						_Strategy = false;
						SetStruct();
					});
				}
			}
		}
	}

	[SpecialName]
	private static float RestartTemplate()
	{
		return m_Creator - Time.realtimeSinceStartup;
	}

	[SpecialName]
	private static bool SearchTemplate()
	{
		return RestartTemplate() > 0f;
	}

	[InitializeOnLoadMethod]
	private static void RemoveSystem()
	{
		bool flag = CalcSystem();
		if (!ManagerStruct.SearchTest().a_HasSucceededLastVerification)
		{
			state = true;
			_Prototype = flag;
		}
		if (flag && (bool)ManagerStruct.SearchTest().a_VerifyOnProjectLoad)
		{
			AdvisorDicBridge.ExcludeAccount(delegate
			{
				CreateSystem(isres: false);
			});
		}
	}

	private static void DestroySystem()
	{
		if (!_Prototype && (bool)ManagerStruct.SearchTest().a_VerifyOnDisplay && CalcSystem())
		{
			CreateSystem(isres: false);
		}
	}

	private static void CreateSystem(bool isres)
	{
		_003C_003Ec__DisplayClass132_0 CS_0024_003C_003E8__locals10 = new _003C_003Ec__DisplayClass132_0();
		if ((!ManagerStruct.SearchTest().a_VerifyOnDisplay.PrepareService() && !ManagerStruct.SearchTest().a_VerifyOnProjectLoad.PrepareService() && !isres) || (state && !_Helper) || m_Consumer)
		{
			return;
		}
		_Helper = false;
		m_Consumer = true;
		_Prototype = true;
		CS_0024_003C_003E8__locals10.helperModel = "No1lKII9IzcBAbihub6nCg==" + EditorAnalyticsSessionInfo.id;
		try
		{
			if (SessionState.GetBool(CS_0024_003C_003E8__locals10.helperModel, defaultValue: false))
			{
				_003C_003Ec__DisplayClass132_1 _003C_003Ec__DisplayClass132_1_ = default(_003C_003Ec__DisplayClass132_1);
				_003C_003Ec__DisplayClass132_1_._PageModel = new AesManaged();
				try
				{
					_003C_003Ec__DisplayClass132_1_._PageModel.Key = Convert.FromBase64String("LWw2tFi+lgG6KK4+nMum8RuWZMIOhu1urChsHMbizPM=");
					_003C_003Ec__DisplayClass132_1_._PageModel.IV = Convert.FromBase64String("MEZqk6gCgPTwifeH3YrTlQ==");
					_003C_003Ec__DisplayClass132_2 _003C_003Ec__DisplayClass132_2_ = default(_003C_003Ec__DisplayClass132_2);
					_003C_003Ec__DisplayClass132_2_.prototypeModel = new HMACSHA1(Encoding.UTF8.GetBytes(CS_0024_003C_003E8__locals10.helperModel));
					try
					{
						if (CollectSystem() == CS_0024_003C_003E8__locals10.LoginTokenizer("date", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_))
						{
							_Publisher = CS_0024_003C_003E8__locals10.LoginTokenizer("u", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_);
							product = CS_0024_003C_003E8__locals10.LoginTokenizer("v", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_);
							decorator = CS_0024_003C_003E8__locals10.LoginTokenizer("r", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_);
							record = CS_0024_003C_003E8__locals10.LoginTokenizer("m", ref _003C_003Ec__DisplayClass132_1_, ref _003C_003Ec__DisplayClass132_2_);
							FlushSystem();
							RegisterSystem();
							m_Invocation = true;
							state = true;
							m_Consumer = false;
							_Exporter = true;
							RestartSystem(acceptident: true);
							DeleteStruct();
						}
					}
					finally
					{
						if (_003C_003Ec__DisplayClass132_2_.prototypeModel != null)
						{
							((IDisposable)_003C_003Ec__DisplayClass132_2_.prototypeModel).Dispose();
						}
					}
				}
				finally
				{
					if (_003C_003Ec__DisplayClass132_1_._PageModel != null)
					{
						((IDisposable)_003C_003Ec__DisplayClass132_1_._PageModel).Dispose();
					}
				}
			}
		}
		catch
		{
			StopStruct("failed to verify from cache.", CustomLogType.Warning);
		}
		MapSystem(delegate
		{
			List<(string, string)> list = PrintSystem("verifylicense");
			FindSystem(list);
			CountStruct(InstantiateSystem(list.ToArray())).PublishAccount(delegate(GetterDicBridge response)
			{
				_003C_003Ec__DisplayClass132_3 _003C_003Ec__DisplayClass132_ = new _003C_003Ec__DisplayClass132_3();
				_003C_003Ec__DisplayClass132_.baseModel = CS_0024_003C_003E8__locals10;
				_003C_003Ec__DisplayClass132_._CreatorModel = response;
				m_Consumer = false;
				state = true;
				SetupSystem(_003C_003Ec__DisplayClass132_._CreatorModel, _003C_003Ec__DisplayClass132_.CalcTokenizer, delegate
				{
					bool exporter = _Exporter;
					m_Invocation = false;
					_Exporter = false;
					decorator = (_Publisher = (product = string.Empty));
					ManagerStruct.SearchTest().a_HasSucceededLastVerification.ListService(useres: false);
					SessionState.EraseBool(CS_0024_003C_003E8__locals10.helperModel);
					ViewSystem(exporter);
				}, isres2: false);
			}, _003C_003Ec._ContextConfig.PrintRules, null, null, SetStruct);
		}, readvis: true);
	}

	private static void CloneSystem()
	{
		_Advisor = true;
		uint num = default(uint);
		while (!LogoutSystem())
		{
			switch ((num = 0xF4833053u ^ (num * 571586353) ^ 0x2EEB6183) % 5)
			{
			case 2u:
			case 3u:
				continue;
			default:
				return;
			case 4u:
				MapSystem(delegate
				{
					List<(string, string)> list = PrintSystem("activatelicense");
					FindSystem(list);
					CountStruct(InstantiateSystem(list.ToArray())).PublishAccount(delegate(GetterDicBridge response)
					{
						_Advisor = false;
						SetupSystem(response, delegate
						{
							state = false;
							ManagerStruct.SearchTest().a_HasSucceededLastVerification.ListService(useres: true);
							CreateSystem(isres: true);
						});
					}, delegate(Exception exception)
					{
						_Advisor = false;
						StopStruct($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
					}, null, null, SetStruct);
				}, readvis: true);
				return;
			case 1u:
				return;
			case 0u:
				break;
			}
			break;
		}
		StopStruct("Invalid License Key!", CustomLogType.Error);
	}

	private static void FlushSystem()
	{
		mapper = _Publisher;
		if (string.IsNullOrWhiteSpace(mapper))
		{
			return;
		}
		try
		{
			Match match = Regex.Match(mapper, "(?i)(?:<color=#(?:[0-9a-f]{8}|[0-9a-f]{6})>)?.*?(#\\d{4})(?:<\\/color>)?$");
			if (match.Success)
			{
				mapper = mapper.Remove(match.Groups[1].Index, match.Groups[1].Length);
			}
			if (mapper.Length > 1 && mapper[0] == '@')
			{
				mapper = mapper.Substring(1);
			}
		}
		catch
		{
		}
	}

	private static void RegisterSystem()
	{
		string[] array = record.Split(new char[1] { '-' });
		string[] array2 = CollectSystem().Split(new char[1] { '/' });
		array2[2] = array2[2].Substring(2, 2);
		_Attr = array2[2] + array[0].Substring(0, 10) + array2[1] + array[2].Substring(0, 10) + array2[0];
	}

	private static void PatchSystem()
	{
		if (string.IsNullOrWhiteSpace(status))
		{
			string key = "DreadScriptssid";
			status = EditorPrefs.GetString(key, string.Empty);
			if (string.IsNullOrWhiteSpace(status) || !Regex.IsMatch(status, "[0-9a-f]{32}"))
			{
				status = GUID.Generate().ToString();
				EditorPrefs.SetString(key, status);
			}
		}
	}

	private static bool CalcSystem()
	{
		if (string.IsNullOrWhiteSpace(setter))
		{
			setter = EditorPrefs.GetString("No1lKII9IzcBAbihub6nCg==LK", string.Empty);
			if (!ExcludeSystem())
			{
				setter = string.Empty;
			}
			return !(state = string.IsNullOrWhiteSpace(setter));
		}
		return true;
	}

	private static void MapSystem(Action setup, bool readvis = false)
	{
		_003C_003Ec__DisplayClass138_0 CS_0024_003C_003E8__locals31 = new _003C_003Ec__DisplayClass138_0();
		CS_0024_003C_003E8__locals31.m_ProxyModel = readvis;
		CS_0024_003C_003E8__locals31._ComparatorModel = setup;
		CS_0024_003C_003E8__locals31._ExporterModel = new string[4][]
		{
			new string[3] { "Manufacturer", "Product", "SerialNumber" },
			new string[1] { "ProcessorId" },
			new string[1] { "SerialNumber" },
			new string[4] { "Manufacturer", "PartNumber", "SerialNumber", "Capacity" }
		};
		CS_0024_003C_003E8__locals31.m_RecordModel = new StringBuilder();
		CS_0024_003C_003E8__locals31.fieldModel = new StringBuilder();
		CS_0024_003C_003E8__locals31.m_AttrModel = EditorPrefs.GetString("DSLICINF", string.Empty);
		CS_0024_003C_003E8__locals31.consumerModel = string.IsNullOrWhiteSpace(CS_0024_003C_003E8__locals31.m_AttrModel);
		if (!CS_0024_003C_003E8__locals31.consumerModel)
		{
			try
			{
				CS_0024_003C_003E8__locals31.m_AttrModel = FillStruct(CS_0024_003C_003E8__locals31.m_AttrModel);
			}
			catch
			{
				CS_0024_003C_003E8__locals31.m_AttrModel = string.Empty;
				CS_0024_003C_003E8__locals31.consumerModel = true;
				EditorPrefs.DeleteKey("DSLICINF");
			}
		}
		CS_0024_003C_003E8__locals31.m_ItemModel = new string[4];
		CS_0024_003C_003E8__locals31.decoratorModel = new string[4];
		CS_0024_003C_003E8__locals31.m_InvocationModel = new string[4];
		ListServiceSerializer[] param = new ListServiceSerializer[4]
		{
			new ListServiceSerializer("wmic baseboard get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ItemModel[0] = o;
			}, iscontrol: true),
			new ListServiceSerializer("wmic cpu get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ItemModel[1] = o;
			}, iscontrol: true),
			new ListServiceSerializer("wmic diskdrive get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ItemModel[2] = o;
			}, iscontrol: true),
			new ListServiceSerializer("wmic memorychip get *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.m_ItemModel[3] = o;
			}, iscontrol: true)
		};
		CS_0024_003C_003E8__locals31.filterModel = new ListServiceSerializer[4]
		{
			new ListServiceSerializer("Get-CimInstance -class Win32_baseboard | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.decoratorModel[0] = o;
			}),
			new ListServiceSerializer("Get-CimInstance -class Win32_processor | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.decoratorModel[1] = o;
			}),
			new ListServiceSerializer("Get-CimInstance -class Win32_diskdrive | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.decoratorModel[2] = o;
			}),
			new ListServiceSerializer("Get-CimInstance -class win32_physicalmemory | Select *", delegate(string o)
			{
				CS_0024_003C_003E8__locals31.decoratorModel[3] = o;
			})
		};
		CS_0024_003C_003E8__locals31.m_CallbackModel = new CancellationTokenSource();
		CS_0024_003C_003E8__locals31.m_CallbackModel.CancelAfter(10000);
		ChangeStruct(param, delegate
		{
			try
			{
				CS_0024_003C_003E8__locals31.WriteSpecification(isCMD: true);
				CS_0024_003C_003E8__locals31.PushSpecification();
			}
			catch (Exception exc)
			{
				CS_0024_003C_003E8__locals31.DefineSpecification(isCMD: true, exc);
			}
		}, CS_0024_003C_003E8__locals31.m_CallbackModel);
	}

	private static void SortSystem(GetterDicBridge var1)
	{
		SetupSystem(var1, null);
	}

	private static void SetupSystem(GetterDicBridge asset, Action caller, Action role = null, bool isres2 = true)
	{
		bool num = asset.StartTest("success");
		string text = asset.StartTest("message");
		string text2 = asset.StartTest("url");
		bool flag = !string.IsNullOrEmpty(text2);
		string text3 = asset.StartTest("url_name");
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
			if (!string.IsNullOrEmpty(text) && isres2)
			{
				StopStruct(text);
			}
			caller?.Invoke();
			return;
		}
		bool flag2 = asset.StartTest("wait_warn");
		float num2 = asset.StartTest("wait_time");
		m_Token |= flag2;
		if (!(num2 <= 0f))
		{
			m_Creator = Time.realtimeSinceStartup + num2;
		}
		role?.Invoke();
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		StopStruct(text, CustomLogType.Error);
		if (flag)
		{
			if (EditorUtility.DisplayDialog("Warning!", text, text3, "Ok"))
			{
				Application.OpenURL(text2);
			}
		}
		else
		{
			EditorUtility.DisplayDialog("Warning!", text, "Ok");
		}
	}

	private static List<(string, string)> PrintSystem(string param, IEnumerable<(string, string)> pol = null)
	{
		PatchSystem();
		List<(string, string)> list = new List<(string, string)>
		{
			("command", param),
			("product_id", "No1lKII9IzcBAbihub6nCg=="),
			("version", _Connection.ToString()),
			("HWID", record),
			("SID", status),
			("license_key", setter)
		};
		if (pol != null)
		{
			list.AddRange(pol);
		}
		return list;
	}

	private static void FindSystem(List<(string, string)> def)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (var item3 in def)
		{
			string item = item3.Item2;
			stringBuilder.Append(item);
		}
		using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?"));
		string item2 = Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString())));
		def.Add(("hash", item2));
	}

	private static string CollectSystem()
	{
		string text = PostStruct(DateTime.UtcNow.Day.ToString());
		string text2 = PostStruct(DateTime.UtcNow.Month.ToString());
		string text3 = DateTime.UtcNow.Year.ToString();
		m_Getter = text + "/" + text2 + "/" + text3;
		return m_Getter;
	}

	private static void ValidateSystem(Action spec)
	{
		if (!m_Invocation)
		{
			_Field = (Action)Delegate.Remove(_Field, spec);
			_Field = (Action)Delegate.Combine(_Field, spec);
		}
		else if (((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		})())
		{
			spec?.Invoke();
		}
	}

	private static void RestartSystem(bool acceptident)
	{
		if (m_Invocation && ((Func<bool>)delegate
		{
			using HMACSHA256 hMACSHA = new HMACSHA256(Encoding.UTF8.GetBytes("of,ejcX?$0 &n*Uc{lG6_vk5)i!F:;/B]asd(H8[N 2lGc~H+rNjZafKv!W< -LypW.GY]U$w&>'htNSyCuYlEYmnmqX_cpVbS)nBoB=T)*A=ay`phI qK_$*1;O KG?" + setter));
			return decorator == Convert.ToBase64String(hMACSHA.ComputeHash(Encoding.UTF8.GetBytes(m_Getter + record)));
		})())
		{
			if (!map)
			{
				_Field?.Invoke();
			}
			map = true;
		}
	}

	private static void ViewSystem(bool calcinfo)
	{
	}

	[SpecialName]
	private static string OrderTemplate()
	{
		string text = "";
		if (m_Token)
		{
			text += "Too many failed attempts! Further failed attempts will result in getting your device blocked!\n";
		}
		if (SearchTemplate())
		{
			text += $"Please wait {Mathf.CeilToInt(RestartTemplate())} seconds.";
		}
		return text;
	}

	private static void SearchSystem()
	{
		using (new GUILayout.HorizontalScope())
		{
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				GUILayout.Label("License: " + (string.IsNullOrWhiteSpace(product) ? "Personal" : product), AdvisorDicBridge.ManageRequest().processorTemplate);
				GUILayout.FlexibleSpace();
			}
			if (!string.IsNullOrWhiteSpace(mapper))
			{
				using (new GUILayout.HorizontalScope(GUI.skin.box))
				{
					GUILayout.Label("Authorized For: " + mapper, AdvisorDicBridge.ManageRequest().procTemplate);
					return;
				}
			}
		}
	}

	private static bool QuerySystem(EditorWindow instance = null, float map = 0f)
	{
		if (!m_Invocation)
		{
			if (Event.current.type == EventType.Repaint)
			{
				DestroySystem();
			}
			if ((object)instance != null)
			{
				AdvisorDicBridge.m_RoleTemplate.ListWrapper(instance, map);
			}
			ListStruct();
			if (!_Advisor && !m_Consumer)
			{
				if (m_Callback)
				{
					OrderSystem();
					return false;
				}
				if (state && !_Helper)
				{
					EnableSystem("Enter your license key", "Enter the license key you received with your purchase here. If your license was already activated, click on 'Transfer License'. For support, contact @Dreadrith.");
					bool flag = ConcatSystem(movefirst: false);
					if (OrderTemplate().Length > 0)
					{
						EditorGUILayout.HelpBox(OrderTemplate(), MessageType.Error);
					}
					bool flag2 = ExcludeSystem() && !SearchTemplate();
					flag &= flag2 && !_Prototype;
					using (new EditorGUI.DisabledScope(!flag2))
					{
						if (AdvisorDicBridge.GetManager("Activate") || flag)
						{
							CloneSystem();
						}
					}
					UpdateStruct(PublishSystem);
					return false;
				}
				EnableSystem("Check for License", "This will check for whether you already have a license for your device");
				if (AdvisorDicBridge.ResetManager((!_Helper) ? "Check" : "Retry", EditorStyles.toolbarButton))
				{
					CreateSystem(isres: true);
				}
				return false;
			}
			EnableSystem((!_Advisor) ? "Verifying License..." : "Activating License...", "Please wait till this finishes processing.");
			return false;
		}
		if (_Strategy)
		{
			LoginSystem();
			return false;
		}
		if (!rule)
		{
			return true;
		}
		WorkerModelDispatcher.ConnectDic();
		return false;
	}

	private static void OrderSystem()
	{
		EnableSystem("Transferring License", "This is for moving your license to a new device or re-activating it in case it fails to recognize your device.");
		if (_Filter)
		{
			EditorGUILayout.HelpBox("A 6-digit verification code was sent to " + visitor + ".\nIf this is not your email address, please contact support.\nIf you don't see the verification email, please check your spam folder.", MessageType.Info);
			m_Object = EditorGUILayout.TextField("Verification Code", m_Object);
			m_Object = Regex.Replace(m_Object, "[^0-9]", string.Empty, RegexOptions.Multiline);
			EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(!Regex.IsMatch(m_Object, "[0-9]{6}") || comparator);
			try
			{
				if (AdvisorDicBridge.GetManager(comparator ? "Transferring..." : "Transfer License"))
				{
					PushStruct();
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
			EditorGUI.DisabledScope disabledScope = new EditorGUI.DisabledScope(m_Proxy);
			try
			{
				ConcatSystem(movefirst: true);
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
			if (OrderTemplate().Length > 0)
			{
				EditorGUILayout.HelpBox(OrderTemplate(), MessageType.Error);
			}
			disabledScope = new EditorGUI.DisabledScope(!LogoutSystem() || m_Proxy);
			try
			{
				if (AdvisorDicBridge.GetManager((!m_Proxy) ? "Send Verification Code" : "Sending Verification Code..."))
				{
					DefineStruct();
				}
			}
			finally
			{
				((IDisposable)disabledScope/*cast due to .constrained prefix*/).Dispose();
			}
		}
		UpdateStruct(PublishSystem);
	}

	private static void EnableSystem(string setup, string token)
	{
		using (new GUILayout.HorizontalScope(AdvisorDicBridge.ManageRequest()._ReponseTemplate))
		{
			GUILayout.Label(string.Empty, GUILayout.Width(17f), GUILayout.Height(17f));
			GUILayout.Label(setup, AdvisorDicBridge.ManageRequest()._WriterTemplate);
			GUILayout.Label(new GUIContent(AdvisorDicBridge.PrepareRequest().m_ComparatorTemplate)
			{
				tooltip = token
			}, AdvisorDicBridge.ManageRequest().m_ReaderTemplate, GUILayout.Width(17f), GUILayout.Height(17f));
		}
	}

	private static bool ConcatSystem(bool movefirst)
	{
		using (new GUILayout.HorizontalScope())
		{
			string text = "ADOverhaulLicenseField";
			if (AdvisorDicBridge.SortManager(text))
			{
				GUI.FocusControl(null);
				return true;
			}
			if (AdvisorDicBridge.SetupManager(text))
			{
				GUI.FocusControl(null);
			}
			using (new GUILayout.HorizontalScope(GUI.skin.box))
			{
				if (movefirst)
				{
					EditorGUILayout.PrefixLabel("License Key");
				}
				GUI.SetNextControlName(text);
				setter = EditorGUILayout.TextField(string.Empty, setter).Trim();
				AdvisorDicBridge.LoginManager("License Key", string.IsNullOrWhiteSpace(setter), 80f);
			}
			if (!_Prototype && ExcludeSystem() && !SearchTemplate())
			{
				_Prototype = true;
				return true;
			}
		}
		return false;
	}

	private static bool LogoutSystem()
	{
		if (!m_Callback)
		{
			if (SearchTemplate())
			{
				return false;
			}
			return ExcludeSystem();
		}
		if (!SearchTemplate() && ExcludeSystem())
		{
			return AddSystem();
		}
		return false;
	}

	private static bool ExcludeSystem()
	{
		return Regex.Match(setter, "^[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}$").Success;
	}

	private static bool AddSystem()
	{
		if (_Filter)
		{
			return Regex.Match(m_Object, "^[a-zA-Z0-9]{6}$").Success;
		}
		return true;
	}

	private static void PublishSystem()
	{
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.FlexibleSpace();
			if (AdvisorDicBridge.CancelManager((!m_Callback) ? "Transfer License" : "Activate License"))
			{
				m_Callback = !m_Callback;
			}
		}
	}

	private static string InstantiateSystem(IEnumerable<(string, string)> res)
	{
		StringBuilder stringBuilder = new StringBuilder("{");
		bool flag = true;
		foreach (var (text, text2) in res)
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

	private static HttpWebRequest RevertSystem(string info)
	{
		HttpWebRequest httpWebRequest = WebRequest.CreateHttp(info);
		httpWebRequest.Method = "POST";
		httpWebRequest.Accept = "application/json";
		httpWebRequest.ContentType = "application/json";
		return httpWebRequest;
	}

	private static async Task<GetterDicBridge> ReflectStruct(string i, string col)
	{
		_003C_003Ec__DisplayClass165_0 CS_0024_003C_003E8__locals5 = new _003C_003Ec__DisplayClass165_0();
		CS_0024_003C_003E8__locals5._ComposerModel = i;
		CS_0024_003C_003E8__locals5.codeModel = col;
		CS_0024_003C_003E8__locals5.facadeModel = default(GetterDicBridge);
		await Task.Run([AsyncStateMachine(typeof(_003C_003Ec__DisplayClass165_0.AdapterMethodObject))] () =>
		{
			_003C_003Ec__DisplayClass165_0.AdapterMethodObject adapterMethodObject = default(_003C_003Ec__DisplayClass165_0.AdapterMethodObject);
			adapterMethodObject.connectionModel = AsyncTaskMethodBuilder.Create();
			adapterMethodObject.customerModel = CS_0024_003C_003E8__locals5;
			while (true)
			{
				adapterMethodObject._ProcessModel = -1;
			}
		});
		return CS_0024_003C_003E8__locals5.facadeModel;
	}

	private static Task<GetterDicBridge> CountStruct(string param)
	{
		return ReflectStruct("https://us-central1-dreadscripts-c6b62.cloudfunctions.net/receiveCommand", param);
	}

	private static void SetStruct()
	{
		AdvisorDicBridge.ExcludeAccount(DeleteStruct);
	}

	private static void DeleteStruct()
	{
		Queue[] array = Resources.FindObjectsOfTypeAll<Queue>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Repaint();
		}
	}

	private static void NewStruct()
	{
		using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, Color.clear))
		{
			if (GUILayout.Button(new GUIContent("Made By @Dreadrith ♡", "https://dreadrith.com/links"), AdvisorDicBridge.ManageRequest()._StructDic))
			{
				Application.OpenURL("https://dreadrith.com/links");
			}
			AdvisorDicBridge.IncludeManager();
		}
	}

	internal static bool SelectStruct(string var1, bool isvisitor = true)
	{
		return StopStruct(var1, CustomLogType.Warning, isvisitor);
	}

	internal static bool RunStruct(string setup, bool skipsecond = true)
	{
		return StopStruct(setup, CustomLogType.Error, skipsecond);
	}

	internal static bool StopStruct(string info, CustomLogType pred = CustomLogType.Regular, bool isdic = true)
	{
		if (isdic)
		{
			Color color = ((pred == CustomLogType.Regular) ? AdvisorDicBridge.m_InitializerTemplate : ((pred != CustomLogType.Warning) ? AdvisorDicBridge.infoTemplate : AdvisorDicBridge._AuthenticationTemplate));
			string message = "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">[ADOverhaul]</color> " + info.Replace("\\n", "\n");
			switch (pred)
			{
			case CustomLogType.Error:
				UnityEngine.Debug.LogError(message);
				break;
			case CustomLogType.Regular:
				UnityEngine.Debug.Log(message);
				break;
			case CustomLogType.Warning:
				UnityEngine.Debug.LogWarning(message);
				break;
			}
		}
		return isdic;
	}

	internal static void WriteStruct(string param, bool ignorecol = true)
	{
		if (ignorecol)
		{
			throw new Exception("<color=#" + ColorUtility.ToHtmlStringRGB(AdvisorDicBridge.infoTemplate) + ">[ADOverhaul]</color> " + param);
		}
	}

	private static void DefineStruct()
	{
		string message = "License transfer is subject to the Terms of Service.\nLicense will stop working on the device it was previously activated on.\nYou will not be able to transfer back or again for 30 days.";
		switch (EditorUtility.DisplayDialogComplex("Terms of Service", message, "Continue", "Terms of Service", "Cancel"))
		{
		case 1:
			Application.OpenURL("https://dreadrith.com/license-tos");
			break;
		case 0:
			m_Proxy = true;
			MapSystem(delegate
			{
				List<(string, string)> list = PrintSystem("transferlicenserequest");
				FindSystem(list);
				CountStruct(InstantiateSystem(list.ToArray())).PublishAccount(delegate(GetterDicBridge response)
				{
					_003C_003Ec__DisplayClass179_0 _003C_003Ec__DisplayClass179_ = new _003C_003Ec__DisplayClass179_0();
					_003C_003Ec__DisplayClass179_.candidateModel = response;
					m_Proxy = false;
					SetupSystem(_003C_003Ec__DisplayClass179_.candidateModel, _003C_003Ec__DisplayClass179_.RegisterSpecification);
				}, delegate(Exception exception)
				{
					m_Proxy = false;
					StopStruct($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
				}, null, null, SetStruct);
			}, readvis: true);
			break;
		}
	}

	private static void PushStruct()
	{
		comparator = true;
		MapSystem(delegate
		{
			List<(string, string)> list = PrintSystem("transferlicenseconfirm");
			list.Add(("verification_code", m_Object));
			FindSystem(list);
			CountStruct(InstantiateSystem(list.ToArray())).PublishAccount(delegate(GetterDicBridge response)
			{
				comparator = false;
				SetupSystem(response, delegate
				{
					m_Callback = false;
					_Filter = false;
					state = false;
					CreateSystem(isres: true);
				});
			}, delegate(Exception exception)
			{
				comparator = false;
				StopStruct($"Something went wrong transferring license! Please contact support.\n\n{exception}", CustomLogType.Error);
			}, null, null, SetStruct);
		}, readvis: true);
	}

	[SpecialName]
	private static bool ConcatTemplate()
	{
		return ManagerStruct.SearchTest().u_updateDay == CollectSystem();
	}

	private static void UpdateStruct(Action setup = null, Action<GenericMenu> map = null)
	{
		using (new GUILayout.VerticalScope(GUI.skin.box))
		{
			using (new GUILayout.HorizontalScope())
			{
				if (AdvisorDicBridge.RateManager(AdvisorDicBridge.PrepareRequest().m_CallbackTemplate))
				{
					InsertStruct(map);
				}
				if (!ManagerStruct.SearchTest().u_updateHidden && code && AdvisorDicBridge.RateManager(AdvisorDicBridge.PrepareRequest()._ItemTemplate))
				{
					m_Facade.target = !m_Facade.target;
				}
				GUILayout.Label("v" + _Connection, AdvisorDicBridge.ManageRequest().m_SystemDic, GUILayout.ExpandWidth(expand: false));
				if (setup == null)
				{
					GUILayout.FlexibleSpace();
					NewStruct();
				}
				else
				{
					setup();
				}
			}
			if (code)
			{
				PrepareStruct();
			}
		}
	}

	private static void InsertStruct(Action<GenericMenu> spec = null)
	{
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Check For Update"), on: false, (!_Printer && !m_Composer) ? ((GenericMenu.MenuFunction)delegate
		{
			SessionState.EraseString("No1lKII9IzcBAbihub6nCg==updateinfo");
			ReadStruct();
		}) : null);
		if (m_Invocation)
		{
			genericMenu.AddItem(new GUIContent("Send Feedback"), _Strategy, delegate
			{
				_Strategy.PushManager();
			});
		}
		if (m_Invocation)
		{
			if (spec != null)
			{
				spec(genericMenu);
				genericMenu.AddSeparator(string.Empty);
			}
			genericMenu.AddSeparator(string.Empty);
			genericMenu.AddItem(new GUIContent("Verify/On Display"), ManagerStruct.SearchTest().a_VerifyOnDisplay, delegate
			{
				ManagerStruct.SearchTest().a_VerifyOnDisplay.InstantiateTest();
				ManagerStruct.SearchTest().a_VerifyOnProjectLoad.ListService(useres: false);
			});
			genericMenu.AddItem(new GUIContent("Verify/On Project Load"), ManagerStruct.SearchTest().a_VerifyOnProjectLoad, delegate
			{
				ManagerStruct.SearchTest().a_VerifyOnProjectLoad.InstantiateTest();
				ManagerStruct.SearchTest().a_VerifyOnDisplay.ListService(useres: false);
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
		if (m_Invocation)
		{
			if (m_Customer.Length != 0)
			{
				if (m_Customer.Length <= 1)
				{
					genericMenu.AddItem(new GUIContent(m_Customer[0].Item1), on: false, delegate
					{
						Application.OpenURL(m_Customer[0].Item2);
					});
				}
				else
				{
					(string, string)[] customer = m_Customer;
					for (int num = 0; num < customer.Length; num++)
					{
						(string, string) tuple = customer[num];
						string item = tuple.Item1;
						string m_ExpressionModel = tuple.Item2;
						string text = "Samples/" + item;
						genericMenu.AddItem(new GUIContent(text), on: false, delegate
						{
							Application.OpenURL(m_ExpressionModel);
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

	private static void PrepareStruct(bool checkfirst = true)
	{
		if ((bool)ManagerStruct.SearchTest().u_updateHidden)
		{
			return;
		}
		m_Facade.InsertManager(delegate
		{
			if (checkfirst)
			{
				AdvisorDicBridge.RemoveManager();
			}
			EditorGUILayout.HelpBox($"Version {ManagerStruct.SearchTest().u_updateVersion}\n--------------\n{ManagerStruct.SearchTest().u_updateMessage}", MessageType.Info);
			bool flag = !string.IsNullOrWhiteSpace(ManagerStruct.SearchTest().u_updateLink);
			bool flag2 = !string.IsNullOrWhiteSpace(ManagerStruct.SearchTest().u_updateChangelog);
			using (new GUILayout.HorizontalScope())
			{
				if (flag)
				{
					using (new EditorGUI.DisabledScope(adapter))
					{
						if (AdvisorDicBridge.ResetManager("Download Update", EditorStyles.toolbarButton))
						{
							VerifyStruct();
						}
					}
				}
				if (flag2 && AdvisorDicBridge.AwakeManager(new GUIContent("Open Changelog", ManagerStruct.SearchTest().u_updateChangelog), EditorStyles.toolbarButton))
				{
					Application.OpenURL(ManagerStruct.SearchTest().u_updateChangelog);
				}
				if (AdvisorDicBridge.ResetManager("Skip for Today", EditorStyles.toolbarButton))
				{
					ManagerStruct.SearchTest().u_updateHidden.ListService(useres: true);
				}
			}
		}, DeleteStruct);
	}

	private static void ListStruct()
	{
		if ((bool)ManagerStruct.SearchTest().u_announcementHidden || string.IsNullOrWhiteSpace(ManagerStruct.SearchTest().u_announcement))
		{
			return;
		}
		using (new GUILayout.VerticalScope(EditorStyles.helpBox))
		{
			Rect m_MockModel = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(expand: true), GUILayout.Height(24f));
			Rect value = m_MockModel;
			GUI.Label(value.UpdateManager(24f, allowpool: true), AdvisorDicBridge.PrepareRequest()._InvocationTemplate);
			GUI.Label(value, "Announcement", AdvisorDicBridge.ManageRequest().policyTemplate);
			m_Process.InsertManager(delegate
			{
				m_MockModel.height += 18f;
				AdvisorDicBridge.RemoveManager();
				EditorGUILayout.HelpBox(ManagerStruct.SearchTest().u_announcement, MessageType.Info);
				using (new GUILayout.HorizontalScope())
				{
					if (!string.IsNullOrWhiteSpace(ManagerStruct.SearchTest().u_announcementLink) && AdvisorDicBridge.ResetManager(ManagerStruct.SearchTest().u_announcementLinkName, EditorStyles.toolbarButton))
					{
						Application.OpenURL(ManagerStruct.SearchTest().u_announcementLink);
					}
					if (m_Invocation && AdvisorDicBridge.ResetManager("Hide", EditorStyles.toolbarButton))
					{
						ManagerStruct.SearchTest().u_announcementHidden.ListService(useres: true);
						ManagerStruct.SearchTest().u_announcementHiddenDate.InstantiateService(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
					}
				}
			}, DeleteStruct);
			if (AdvisorDicBridge.CalculateManager(m_MockModel))
			{
				m_Process.target = !m_Process.target;
			}
		}
	}

	[InitializeOnLoadMethod]
	private static void ManageStruct()
	{
		if (ConcatTemplate() && !string.IsNullOrWhiteSpace(ManagerStruct.SearchTest().u_updateVersion.PublishService()))
		{
			ConnectStruct(iskey: false);
			return;
		}
		AdvisorDicBridge.ExcludeAccount(delegate
		{
			ResolveStruct(isident: false);
		});
	}

	private static void ReadStruct()
	{
		ResolveStruct(isident: true);
	}

	private static void ResolveStruct(bool isident)
	{
		if ((!isident && ConcatTemplate()) || m_Composer || _Printer)
		{
			return;
		}
		_Printer = true;
		CountStruct(InstantiateSystem(new List<(string, string)>
		{
			("command", "getdownloadinfo"),
			("product_id", "No1lKII9IzcBAbihub6nCg=="),
			("version", _Connection.ToString())
		})).PublishAccount(delegate(GetterDicBridge response)
		{
			m_Composer = true;
			string text = ManagerStruct.SearchTest().u_announcement.PublishService();
			using (new ManagerStruct.StubTemplateMapper())
			{
				ManagerStruct.SearchTest().u_updateLink.InstantiateService(response.StartTest("download_link"));
				ManagerStruct.SearchTest().u_updateMessage.InstantiateService(response.StartTest("download_message"));
				ManagerStruct.SearchTest().u_updateChangelog.InstantiateService(response.StartTest("changelog_link"));
				ManagerStruct.SearchTest().u_updateVersion.InstantiateService(response.StartTest("version"));
				ManagerStruct.SearchTest().u_updateDay.InstantiateService(CollectSystem());
				ManagerStruct.SearchTest().u_announcement.InstantiateService(response.StartTest("announcement"));
				if (!string.IsNullOrWhiteSpace(ManagerStruct.SearchTest().u_announcement))
				{
					ManagerStruct.SearchTest().u_announcement.InstantiateService(ManagerStruct.SearchTest().u_announcement.PublishService().Replace("\\\\n", "\n").Replace("\\n", "\n"));
				}
				ManagerStruct.SearchTest().u_announcementLink.InstantiateService(response.StartTest("announcement_link"));
				ManagerStruct.SearchTest().u_announcementLinkName.InstantiateService(response.StartTest("announcement_link_name"));
			}
			if (text != ManagerStruct.SearchTest().u_announcement.PublishService())
			{
				ManagerStruct.SearchTest().u_announcementHidden.ListService(useres: false);
			}
			ConnectStruct(isident);
		}, delegate(Exception exc)
		{
			StopStruct($"Something went wrong while checking for an update!\n\n{exc}", CustomLogType.Error);
		}, null, null, delegate
		{
			_Printer = false;
			SetStruct();
		});
	}

	private static void VerifyStruct()
	{
		adapter = true;
		UnityWebRequest listenerModel = new UnityWebRequest(ManagerStruct.SearchTest().u_updateLink);
		listenerModel.downloadHandler = new DownloadHandlerFile("Assets/ADOverhaul.unitypackage");
		listenerModel.SendWebRequest().completed += delegate
		{
			adapter = false;
			string text = "Assets/ADOverhaul.unitypackage";
			if (listenerModel.isNetworkError || listenerModel.isHttpError)
			{
				AssetDatabase.ImportAsset(text);
				AssetDatabase.DeleteAsset(text);
				listenerModel.Dispose();
				throw new Exception(listenerModel.error);
			}
			AssetDatabase.ImportPackage(text, interactive: true);
			AssetDatabase.DeleteAsset(text);
			listenerModel.Dispose();
		};
	}

	private static void ConnectStruct(bool iskey)
	{
		if ((bool)ManagerStruct.SearchTest().u_announcementHidden)
		{
			if (DateTime.TryParse(ManagerStruct.SearchTest().u_announcementHiddenDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result))
			{
				ManagerStruct.SearchTest().u_announcementHidden.ListService((DateTime.UtcNow - result).TotalDays < 7.0);
			}
			else
			{
				ManagerStruct.SearchTest().u_announcementHidden.ListService(useres: false);
			}
		}
		if (!(_Connection < new SystemTemplate(ManagerStruct.SearchTest().u_updateVersion.PublishService())))
		{
			if (!iskey)
			{
				ManagerStruct.SearchTest().u_updateHidden.ListService(useres: true);
				return;
			}
			StopStruct("Up to date!");
			Task.Run(async delegate
			{
				await Task.Delay(3000);
				ManagerStruct.SearchTest().u_updateHidden.ListService(useres: true);
				SetStruct();
			});
			return;
		}
		code = true;
		if (iskey)
		{
			ManagerStruct.SearchTest().u_updateHidden.ListService(useres: false);
			m_Facade.target = true;
		}
		if (!ManagerStruct.SearchTest().u_updateHidden)
		{
			StopStruct($"Update Available! <b>(v{ManagerStruct.SearchTest().u_updateVersion})</b>");
		}
	}

	internal static void CompareStruct(string def, AnimBool cust, Action state, Action token2)
	{
		using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(def, AdvisorDicBridge.ManageRequest().templateDic);
				state?.Invoke();
			}
			if (AdvisorDicBridge.CalculateManager())
			{
				cust.target = !cust.target;
				if (!ManagerStruct.SearchTest().editorAnimatedFoldouts)
				{
					cust.value = cust.target;
				}
			}
			cust.InsertManager(token2);
		}
	}

	internal static void InterruptStruct(SceneView instance, string connection, Action helper, float item2, float var13)
	{
		ComputeStruct(instance, delegate
		{
			using (new GUILayout.HorizontalScope())
			{
				AdvisorDicBridge.CreateManager();
				GUILayout.FlexibleSpace();
				GUILayout.Label(connection, AdvisorDicBridge.ManageRequest()._WriterTemplate);
				Rect lastRect = GUILayoutUtility.GetLastRect();
				GUILayout.FlexibleSpace();
				StartStruct();
				return lastRect;
			}
		}, helper, item2, var13);
	}

	internal static void ComputeStruct(SceneView def, Func<Rect> token, Action state, float second2, float connection3)
	{
		Rect ivk = def.ExcludeManager();
		AdvisorDicBridge.PositionFlag positionFlag = ManagerStruct.SearchTest().toolOverlayAlignment.InvokeService<AdvisorDicBridge.PositionFlag>();
		bool flag;
		using (new AdvisorDicBridge.AdvisorTemplate(def, second2, connection3, positionFlag, _Serializer))
		{
			Rect rect = token();
			AdvisorDicBridge.CallManager(rect, MouseCursor.Pan);
			flag = AdvisorDicBridge.DestroyManager(rect, interceptor);
			if (state != null)
			{
				AdvisorDicBridge.RemoveManager(2, 0);
				state();
			}
		}
		if (flag)
		{
			Handles.BeginGUI();
			ManagerStruct.SearchTest().toolOverlayAlignment.RegisterService = (int)AdvisorDicBridge.ReflectManager(positionFlag, ivk);
			Handles.EndGUI();
		}
	}

	internal static void StartStruct()
	{
		if (AdvisorDicBridge.RateManager(AdvisorDicBridge.PrepareRequest().m_ComposerTemplate))
		{
			Queue.PublishTemplate();
		}
	}

	[CompilerGenerated]
	internal static void InitStruct(bool deleteinstance, ref _003C_003Ec__DisplayClass46_0 visitor, ref _003C_003Ec__DisplayClass46_1 consumer)
	{
		if (!deleteinstance)
		{
			return;
		}
		if (visitor._ParameterModel)
		{
			Undo.RecordObject(visitor.m_ImporterModel, "Adjust Radius");
		}
		else
		{
			Undo.RecordObjects(visitor.m_RegModel, "Adjust Radius");
		}
		_003C_003Ec__DisplayClass46_2 first = default(_003C_003Ec__DisplayClass46_2);
		first._DispatcherModel = consumer._PolicyModel - visitor.messageModel[visitor._BridgeModel].m_ValDic;
		switch (visitor._UtilsModel)
		{
		case 2:
		{
			CheckStruct(visitor.messageModel[visitor._BridgeModel], out visitor.messageModel[visitor._BridgeModel].m_ValDic, out visitor.messageModel[visitor._BridgeModel].merchantDic, ref first);
			for (int j = 0; j < visitor.messageModel.Length; j++)
			{
				if (j != visitor._BridgeModel)
				{
					visitor.messageModel[j].m_ValDic = visitor.messageModel[visitor._BridgeModel].m_ValDic;
					if (visitor.messageModel[j]._DatabaseDic == 0)
					{
						visitor.messageModel[j].merchantDic = visitor.messageModel[j].m_ValDic * 2f;
					}
					else
					{
						visitor.messageModel[j].merchantDic += first._DispatcherModel * 2f;
					}
				}
			}
			break;
		}
		case 0:
		{
			for (int i = 0; i < visitor.messageModel.Length; i++)
			{
				CheckStruct(visitor.messageModel[i], out visitor.messageModel[i].m_ValDic, out visitor.messageModel[i].merchantDic, ref first);
			}
			break;
		}
		case 1:
			CheckStruct(visitor.messageModel[visitor._BridgeModel], out visitor.messageModel[visitor._BridgeModel].m_ValDic, out visitor.messageModel[visitor._BridgeModel].merchantDic, ref first);
			break;
		}
	}

	[CompilerGenerated]
	internal static void CheckStruct(AdvisorDicBridge.ConfigurationDic var1, out float vis, out float res, ref _003C_003Ec__DisplayClass46_2 first2)
	{
		vis = var1.m_ValDic + first2._DispatcherModel;
		if (var1._DatabaseDic != 0)
		{
			res = var1.merchantDic + first2._DispatcherModel * 2f;
		}
		else
		{
			res = vis * 2f;
		}
	}

	[CompilerGenerated]
	internal static void CancelStruct(bool isres, bool writeivk, ref _003C_003Ec__DisplayClass46_0 pool, ref _003C_003Ec__DisplayClass46_3 col2)
	{
		if (!isres)
		{
			return;
		}
		if (pool._ParameterModel)
		{
			Undo.RecordObject(pool.m_ImporterModel, "Adjust Height");
		}
		else
		{
			Undo.RecordObjects(pool.m_RegModel, "Adjust Height");
		}
		Vector3 vector = ((!writeivk) ? col2._ReaderModel : col2.m_CollectionModel);
		Vector3 vector2 = (writeivk ? col2.m_PoolModel : col2.m_WriterModel);
		bool flag = (pool.m_IdentifierModel - vector2).magnitude < (pool.m_IdentifierModel - vector).magnitude;
		float num = (vector2 - vector).magnitude * (float)((!flag) ? 1 : (-1)) * 2f / pool._GlobalModel;
		switch (pool._UtilsModel)
		{
		case 2:
		{
			pool.messageModel[pool._BridgeModel].merchantDic += num;
			for (int j = 0; j < pool.messageModel.Length; j++)
			{
				pool.messageModel[j].merchantDic = pool.messageModel[pool._BridgeModel].merchantDic;
			}
			break;
		}
		case 0:
		{
			for (int i = 0; i < pool.messageModel.Length; i++)
			{
				pool.messageModel[i].merchantDic += num;
			}
			break;
		}
		case 1:
			pool.messageModel[pool._BridgeModel].merchantDic += num;
			break;
		}
	}

	[CompilerGenerated]
	internal static void DisableStruct<T>(Dictionary<T, T> info, ref _003C_003Ec__DisplayClass54_0 pred) where T : UnityEngine.Component
	{
		foreach (KeyValuePair<T, T> item in info)
		{
			m_Database.Add(item.Key, item.Value);
			m_Val.Add(item.Value, item.Key);
			merchant.Add(item.Value, value: false);
			if (_Rules.Contains(item.Key.gameObject))
			{
				pred.interpreterModel.Add(item.Value.transform);
			}
		}
	}

	[CompilerGenerated]
	internal static void IncludeStruct(bool islast, ref _003C_003Ec__DisplayClass86_0 col, ref _003C_003Ec__DisplayClass86_1 helper)
	{
		StopStruct((!islast) ? (col._SingletonModel.stringValue + " already exists in " + helper.m_ProcModel.name) : (col._SingletonModel.stringValue + " added to " + helper.m_ProcModel.name));
	}

	[CompilerGenerated]
	internal static string RateStruct(string def, ref _003C_003Ec__DisplayClass132_1 cont)
	{
		if (string.IsNullOrEmpty(def))
		{
			return def;
		}
		ICryptoTransform cryptoTransform = cont._PageModel.CreateDecryptor(cont._PageModel.Key, cont._PageModel.IV);
		byte[] array = Convert.FromBase64String(def);
		byte[] bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
		return Encoding.UTF8.GetString(bytes);
	}

	[CompilerGenerated]
	internal static string ForgotStruct(string ident, ref _003C_003Ec__DisplayClass132_2 visitor)
	{
		return Convert.ToBase64String(visitor.prototypeModel.ComputeHash(Encoding.UTF8.GetBytes(ident)));
	}

	[CompilerGenerated]
	internal static string AssetStruct(string reference, ref _003C_003Ec__DisplayClass132_5 caller)
	{
		return Convert.ToBase64String(caller._AdvisorModel.ComputeHash(Encoding.UTF8.GetBytes(reference)));
	}

	[CompilerGenerated]
	internal static string TestStruct(string asset, ref _003C_003Ec__DisplayClass132_4 vis)
	{
		if (!string.IsNullOrEmpty(asset))
		{
			ICryptoTransform cryptoTransform = vis.getterModel.CreateEncryptor(vis.getterModel.Key, vis.getterModel.IV);
			byte[] bytes = Encoding.UTF8.GetBytes(asset);
			return Convert.ToBase64String(cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length));
		}
		return asset;
	}

	[CompilerGenerated]
	internal static void ResetStruct()
	{
		List<(string, string)> list = PrintSystem("activatelicense");
		FindSystem(list);
		CountStruct(InstantiateSystem(list.ToArray())).PublishAccount(delegate(GetterDicBridge response)
		{
			_Advisor = false;
			SetupSystem(response, delegate
			{
				state = false;
				ManagerStruct.SearchTest().a_HasSucceededLastVerification.ListService(useres: true);
				CreateSystem(isres: true);
			});
		}, delegate(Exception exception)
		{
			_Advisor = false;
			StopStruct($"Something went wrong activating license!\n\n{exception}", CustomLogType.Error);
		}, null, null, SetStruct);
	}

	[CompilerGenerated]
	internal static string GetStruct(string config)
	{
		_003C_003Ec__DisplayClass138_1 _003C_003Ec__DisplayClass138_ = default(_003C_003Ec__DisplayClass138_1);
		_003C_003Ec__DisplayClass138_._AdapterModel = new AesManaged();
		try
		{
			_003C_003Ec__DisplayClass138_._AdapterModel.Key = Convert.FromBase64String("3epqD3d1DrDCuf1yV3SkFhrll8jVCc4dbC0P1PhU/NA=");
			_003C_003Ec__DisplayClass138_._AdapterModel.IV = Convert.FromBase64String("MTOuc+v23iVKtf8SLX3WxQ==");
			return VisitStruct(config, ref _003C_003Ec__DisplayClass138_);
		}
		finally
		{
			if (_003C_003Ec__DisplayClass138_._AdapterModel != null)
			{
				((IDisposable)_003C_003Ec__DisplayClass138_._AdapterModel).Dispose();
			}
		}
	}

	[CompilerGenerated]
	internal static string VisitStruct(string v, ref _003C_003Ec__DisplayClass138_1 visitor)
	{
		ICryptoTransform cryptoTransform = visitor._AdapterModel.CreateEncryptor(visitor._AdapterModel.Key, visitor._AdapterModel.IV);
		byte[] bytes = Encoding.UTF8.GetBytes(v);
		return Convert.ToBase64String(cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length));
	}

	[CompilerGenerated]
	internal static string AwakeStruct(string spec)
	{
		using AesManaged aesManaged = new AesManaged();
		aesManaged.Key = Convert.FromBase64String("3epqD3d1DrDCuf1yV3SkFhrll8jVCc4dbC0P1PhU/NA=");
		aesManaged.IV = Convert.FromBase64String("MTOuc+v23iVKtf8SLX3WxQ==");
		ICryptoTransform cryptoTransform = aesManaged.CreateDecryptor(aesManaged.Key, aesManaged.IV);
		byte[] array = Convert.FromBase64String(spec);
		return Encoding.UTF8.GetString(cryptoTransform.TransformFinalBlock(array, 0, array.Length));
	}

	[CompilerGenerated]
	internal static string InvokeStruct(string asset, int[] ord)
	{
		foreach (int num in ord)
		{
			if (num > 0)
			{
				asset = CustomizeStruct(asset, num);
			}
		}
		return asset;
	}

	[CompilerGenerated]
	internal static string CustomizeStruct(string first, int column_b)
	{
		int num = 2;
		for (int i = column_b; i < first.Length; i += column_b)
		{
			num++;
			if (num == 3)
			{
				int num2 = i + column_b;
				if (num2 >= first.Length)
				{
					break;
				}
				char c = first[num2];
				first = first.Remove(num2, 1).Insert(num2, first[i].ToString());
				first = first.Remove(i, 1).Insert(i, c.ToString());
				num = 0;
			}
		}
		return first;
	}

	[CompilerGenerated]
	internal static string MoveStruct(string init)
	{
		return InvokeStruct(GetStruct(init), new int[7] { 3, 2, 6, 4, 2, 1, 8 });
	}

	[CompilerGenerated]
	internal static string FillStruct(string item)
	{
		return AwakeStruct(InvokeStruct(item, new int[7] { 8, 1, 2, 4, 6, 2, 3 }));
	}

	[CompilerGenerated]
	internal static async void ChangeStruct(ListServiceSerializer[] param, Action cust, CancellationTokenSource control)
	{
		try
		{
			await Task.Run(delegate
			{
				ListServiceSerializer[] array = param;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SortDic();
				}
			}, control.Token);
			while (!param.All((ListServiceSerializer p) => p._ServiceStruct))
			{
				control.Token.ThrowIfCancellationRequested();
				await Task.Delay(50, control.Token);
			}
		}
		finally
		{
			cust?.Invoke();
		}
	}

	[CompilerGenerated]
	internal static bool CalculateStruct(string first, string cont, out (List<string>, Dictionary<string, RangeInt>) c)
	{
		c = (new List<string>(), new Dictionary<string, RangeInt>());
		(List<string>, Dictionary<string, RangeInt>) tuple = c;
		List<string> item = tuple.Item1;
		Dictionary<string, RangeInt> item2 = tuple.Item2;
		string[] array = first.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		bool flag = false;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			if (!flag)
			{
				if (text.IndexOf(cont, StringComparison.OrdinalIgnoreCase) < 0)
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
			else
			{
				item.Add(text);
			}
		}
		return item.Count > 0;
	}

	[CompilerGenerated]
	internal static bool PopStruct((List<string>, Dictionary<string, RangeInt>) res, string b, out string[] role)
	{
		(List<string>, Dictionary<string, RangeInt>) tuple = res;
		List<string> item = tuple.Item1;
		Dictionary<string, RangeInt> item2 = tuple.Item2;
		role = new string[item.Count];
		if (item2.TryGetValue(b, out var value))
		{
			for (int i = 0; i < item.Count; i++)
			{
				string text = item[i];
				role[i] = text.Substring(value.start, value.length).Trim();
			}
			return !role.All(string.IsNullOrWhiteSpace);
		}
		return false;
	}

	[CompilerGenerated]
	internal static bool CallStruct(string init, string reg, out string[] res)
	{
		string pattern = "(?i).*" + reg + ".*?: *(.*)";
		MatchCollection matchCollection = Regex.Matches(init, pattern);
		if (matchCollection.Count != 0)
		{
			res = new string[matchCollection.Count];
			for (int i = 0; i < matchCollection.Count; i++)
			{
				Match match = matchCollection[i];
				res[i] = match.Groups[1].Value.Trim();
			}
			return !res.All(string.IsNullOrWhiteSpace);
		}
		res = Array.Empty<string>();
		return false;
	}

	[CompilerGenerated]
	internal static string PostStruct(string param)
	{
		if (param.Length < 2)
		{
			return "0" + param;
		}
		return param;
	}

	MethodInfo RateConfig(string item, BindingFlags b)
	{
		return ((Type)this).GetMethod(item, b);
	}

	internal static bool MoveGlobal()
	{
		return RestartGlobal == null;
	}
}
