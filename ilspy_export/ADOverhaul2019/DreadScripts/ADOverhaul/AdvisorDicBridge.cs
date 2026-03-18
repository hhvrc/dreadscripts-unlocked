using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Networking;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ADOverhaul;

internal static class AdvisorDicBridge
{
	[Flags]
	internal enum PositionFlag
	{
		Middle = 1,
		Right = 2,
		Left = 4,
		Top = 8,
		Bottom = 0x10,
		TopRight = 0x20,
		TopLeft = 0x40,
		BottomRight = 0x80,
		BottomLeft = 0x100,
		All = -1
	}

	internal class PublisherTemplate
	{
		private struct PrototypeTemplate
		{
			internal PositionFlag _CreatorTemplate;

			internal Rect _BaseTemplate;

			internal int m_GetterTemplate;
		}

		private int m_MapperTemplate;

		private Vector2 productTemplate = Vector2.zero;

		private readonly int _SetterTemplate = GUIUtility.GetControlID("ResizeStateControlID".GetHashCode(), FocusType.Passive);

		public Action _ObjectTemplate;

		public float m_VisitorTemplate;

		public float m_StatusTemplate;

		public float tokenTemplate;

		public float stateTemplate;

		private bool helperTemplate;

		private bool pageTemplate;

		private static PublisherTemplate AssetFactory;

		[SpecialName]
		public bool GetRequest()
		{
			return helperTemplate;
		}

		[SpecialName]
		public void VisitRequest(bool ignoreinfo)
		{
			if (helperTemplate == ignoreinfo)
			{
				return;
			}
			helperTemplate = ignoreinfo;
			if (!ignoreinfo)
			{
				return;
			}
			if (m_VisitorTemplate != 0f)
			{
				m_StatusTemplate = m_VisitorTemplate;
			}
			else if (m_StatusTemplate == 0f)
			{
				if (tokenTemplate != 0f)
				{
					stateTemplate = tokenTemplate;
				}
				else if (stateTemplate != 0f)
				{
					tokenTemplate = stateTemplate;
				}
			}
			else
			{
				m_VisitorTemplate = m_StatusTemplate;
			}
		}

		public PublisherTemplate(bool evaluatesetup = false)
		{
			helperTemplate = evaluatesetup;
		}

		public void ResolveRequest()
		{
			m_VisitorTemplate = 0f;
			m_StatusTemplate = 0f;
			tokenTemplate = 0f;
			stateTemplate = 0f;
			_ObjectTemplate?.Invoke();
		}

		public Rect VerifyRequest(Rect init, PositionFlag vis = PositionFlag.Middle, Rect proc = default(Rect))
		{
			if (proc == default(Rect))
			{
				proc = new Rect(-1f, -1f, -1f, -1f);
			}
			bool flag = proc.x != -1f && proc.width != -1f;
			bool flag2 = proc.y != -1f && proc.height != -1f;
			float num = 10f;
			float num2 = init.width + m_VisitorTemplate + m_StatusTemplate;
			float num3 = init.height + tokenTemplate + stateTemplate;
			float num4 = init.x - (num2 - init.width) * CompareRequest(vis);
			float num5 = init.y - (init.height + tokenTemplate + stateTemplate - init.height) * InterruptRequest(vis);
			init.x = ((!flag) ? num4 : Mathf.Clamp(num4, proc.x, proc.x + proc.width - num));
			init.width = ((!flag) ? num2 : Mathf.Clamp(num2, num, proc.width - init.x));
			init.y = (flag2 ? Mathf.Clamp(num5, proc.y, proc.y + proc.height - num) : num5);
			init.height = (flag2 ? Mathf.Clamp(num3, num, proc.height - init.y) : num3);
			return init;
		}

		public void ConnectRequest(Rect ident, PositionFlag b = PositionFlag.Right | PositionFlag.Left, PositionFlag util = PositionFlag.Middle, float selection2 = 4f)
		{
			Event current = Event.current;
			if (pageTemplate && current.type == EventType.MouseUp)
			{
				if (GUIUtility.hotControl == _SetterTemplate)
				{
					GUIUtility.hotControl = 0;
				}
				ResolveRequest();
				current.Use();
				pageTemplate = false;
			}
			float num = selection2 * 2f;
			PrototypeTemplate[] array = new PrototypeTemplate[8]
			{
				new PrototypeTemplate
				{
					_CreatorTemplate = PositionFlag.Left,
					m_GetterTemplate = 0,
					_BaseTemplate = new Rect(ident.x - selection2, ident.y + selection2, num, ident.height - num)
				},
				new PrototypeTemplate
				{
					_CreatorTemplate = PositionFlag.TopLeft,
					m_GetterTemplate = 1,
					_BaseTemplate = new Rect(ident.x - selection2, ident.y - selection2, num, num)
				},
				new PrototypeTemplate
				{
					_CreatorTemplate = PositionFlag.Top,
					m_GetterTemplate = 2,
					_BaseTemplate = new Rect(ident.x + selection2, ident.y - selection2, ident.width - num, num)
				},
				new PrototypeTemplate
				{
					_CreatorTemplate = PositionFlag.TopRight,
					m_GetterTemplate = 3,
					_BaseTemplate = new Rect(ident.x + ident.width - selection2, ident.y - selection2, num, num)
				},
				new PrototypeTemplate
				{
					_CreatorTemplate = PositionFlag.Right,
					m_GetterTemplate = 4,
					_BaseTemplate = new Rect(ident.x + ident.width - selection2, ident.y + selection2, num, ident.height - num)
				},
				new PrototypeTemplate
				{
					_CreatorTemplate = PositionFlag.BottomRight,
					m_GetterTemplate = 5,
					_BaseTemplate = new Rect(ident.x + ident.width - selection2, ident.y + ident.height - selection2, num, num)
				},
				new PrototypeTemplate
				{
					_CreatorTemplate = PositionFlag.Bottom,
					m_GetterTemplate = 6,
					_BaseTemplate = new Rect(ident.x + selection2, ident.y + ident.height - selection2, ident.width - num, num)
				},
				new PrototypeTemplate
				{
					_CreatorTemplate = PositionFlag.BottomLeft,
					m_GetterTemplate = 7,
					_BaseTemplate = new Rect(ident.x - selection2, ident.y + ident.height - selection2, num, num)
				}
			};
			bool flag = current.button == 0;
			PrototypeTemplate[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				PrototypeTemplate prototypeTemplate = array2[i];
				if ((prototypeTemplate._CreatorTemplate & b) < prototypeTemplate._CreatorTemplate)
				{
					continue;
				}
				MouseCursor pred;
				switch (prototypeTemplate._CreatorTemplate)
				{
				case PositionFlag.TopRight:
				case PositionFlag.BottomLeft:
					pred = MouseCursor.ResizeUpRight;
					break;
				case PositionFlag.Right:
				case PositionFlag.Left:
					pred = MouseCursor.ResizeHorizontal;
					break;
				default:
					pred = MouseCursor.Arrow;
					break;
				case PositionFlag.TopLeft:
				case PositionFlag.BottomRight:
					pred = MouseCursor.ResizeUpLeft;
					break;
				case PositionFlag.Top:
				case PositionFlag.Bottom:
					pred = MouseCursor.ResizeVertical;
					break;
				}
				CallManager(prototypeTemplate._BaseTemplate, pred);
				Rect baseTemplate = prototypeTemplate._BaseTemplate;
				if (m_AlgoTemplate)
				{
					baseTemplate.y += 46f;
				}
				if (flag && current.type == EventType.MouseDown && baseTemplate.Contains(current.mousePosition))
				{
					if (current.clickCount == 2)
					{
						pageTemplate = true;
					}
					m_MapperTemplate = prototypeTemplate.m_GetterTemplate;
					GUIUtility.hotControl = _SetterTemplate;
					productTemplate = GUIUtility.GUIToScreenPoint(current.mousePosition);
					current.Use();
				}
			}
			if (current.type != EventType.MouseDrag || GUIUtility.hotControl != _SetterTemplate)
			{
				return;
			}
			PositionFlag creatorTemplate = array[m_MapperTemplate]._CreatorTemplate;
			Vector2 vector = GUIUtility.GUIToScreenPoint(current.mousePosition) - productTemplate;
			if (pageTemplate)
			{
				if (!(vector.sqrMagnitude > new Vector2(15f, 15f).sqrMagnitude))
				{
					return;
				}
				pageTemplate = false;
			}
			if (vector != Vector2.zero)
			{
				switch (creatorTemplate)
				{
				case PositionFlag.TopRight:
					m_StatusTemplate += vector.x;
					if (GetRequest())
					{
						if (!util.HasFlag(PositionFlag.Left))
						{
							m_VisitorTemplate -= vector.y;
						}
						else
						{
							m_StatusTemplate -= vector.y;
						}
					}
					else
					{
						tokenTemplate -= vector.y;
					}
					break;
				case PositionFlag.TopLeft:
					m_VisitorTemplate -= vector.x;
					if (GetRequest())
					{
						if (util.HasFlag(PositionFlag.Bottom))
						{
							tokenTemplate -= vector.x;
						}
						else
						{
							stateTemplate -= vector.x;
						}
					}
					else
					{
						tokenTemplate -= vector.y;
					}
					break;
				case PositionFlag.Bottom:
					stateTemplate += vector.y;
					if (GetRequest())
					{
						if (!util.HasFlag(PositionFlag.Left))
						{
							m_VisitorTemplate += vector.y;
						}
						else
						{
							m_StatusTemplate += vector.y;
						}
					}
					break;
				case PositionFlag.BottomRight:
					m_StatusTemplate += vector.x;
					if (GetRequest())
					{
						if (!util.HasFlag(PositionFlag.Top))
						{
							tokenTemplate += vector.x;
						}
						else
						{
							stateTemplate += vector.x;
						}
					}
					else
					{
						stateTemplate += vector.y;
					}
					break;
				case PositionFlag.BottomLeft:
					m_VisitorTemplate -= vector.x;
					if (GetRequest())
					{
						if (util.HasFlag(PositionFlag.Bottom))
						{
							tokenTemplate += vector.x;
						}
						else
						{
							stateTemplate += vector.x;
						}
					}
					else
					{
						stateTemplate += vector.y;
					}
					break;
				case PositionFlag.Top:
					tokenTemplate -= vector.y;
					if (GetRequest())
					{
						if (!util.HasFlag(PositionFlag.Left))
						{
							m_VisitorTemplate -= vector.y;
						}
						else
						{
							m_StatusTemplate -= vector.y;
						}
					}
					break;
				case PositionFlag.Right:
					m_StatusTemplate += vector.x;
					if (GetRequest())
					{
						if (!util.HasFlag(PositionFlag.Bottom))
						{
							stateTemplate += vector.x;
						}
						else
						{
							tokenTemplate += vector.x;
						}
					}
					break;
				case PositionFlag.Left:
					m_VisitorTemplate -= vector.x;
					if (GetRequest())
					{
						if (!util.HasFlag(PositionFlag.Bottom))
						{
							stateTemplate -= vector.x;
						}
						else
						{
							tokenTemplate -= vector.x;
						}
					}
					break;
				}
				_ObjectTemplate?.Invoke();
			}
			productTemplate = GUIUtility.GUIToScreenPoint(current.mousePosition);
		}

		public static float CompareRequest(PositionFlag instance, bool tokenclose = false)
		{
			if (tokenclose)
			{
				if (instance.PrintAccount())
				{
					return 0f;
				}
				if (instance.FindAccount())
				{
					return 1f;
				}
			}
			else
			{
				if (instance.PrintAccount())
				{
					return 1f;
				}
				if (instance.FindAccount())
				{
					return 0f;
				}
			}
			return 0.5f;
		}

		public static float InterruptRequest(PositionFlag task, bool isattr = false)
		{
			bool flag = task.CollectAccount();
			bool flag2 = task.ValidateAccount();
			if (!isattr)
			{
				if (flag2)
				{
					return 1f;
				}
				if (flag)
				{
					return 0f;
				}
			}
			else
			{
				if (flag)
				{
					return 1f;
				}
				if (flag2)
				{
					return 0f;
				}
			}
			return 0.5f;
		}

		internal static bool ConnectFactory()
		{
			return AssetFactory == null;
		}
	}

	internal class AdvisorTemplate : IDisposable
	{
		public readonly bool consumerTemplate;

		public readonly bool _AttrTemplate = true;

		private readonly Rect _RecordTemplate;

		private static AdvisorTemplate EnableFactory;

		public AdvisorTemplate(SceneView spec, string second, float consumer, int key2, float setup3 = 20f, PositionFlag col4 = PositionFlag.BottomRight, PublisherTemplate def5 = null)
			: this(spec, consumer, key2 + 2, setup3, col4, def5)
		{
			while (true)
			{
				GUILayout.Label(second, ManageRequest()._WriterTemplate);
			}
		}

		public AdvisorTemplate(SceneView init, float cust, int position_third, float map2 = 20f, PositionFlag v3 = PositionFlag.BottomRight, PublisherTemplate caller4 = null)
		{
			Handles.BeginGUI();
			Rect rect = init.ExcludeManager();
			Rect proc = new Rect(rect)
			{
				x = rect.x + 4f,
				y = rect.y + 4f,
				width = rect.width - 8f,
				height = rect.height - 8f
			};
			Rect rect2 = InvokeRequest(rect, cust, position_third, map2, v3, consumerTemplate);
			if (caller4 != null)
			{
				rect2 = caller4.VerifyRequest(rect2, v3, proc);
				caller4.ConnectRequest(rect2, v3.RestartAccount(isreg: true));
			}
			_RecordTemplate = ViewAccount(rect2);
			if (m_AlgoTemplate)
			{
				_RecordTemplate.y += 46f;
			}
			GUILayout.BeginArea(_RecordTemplate);
		}

		public AdvisorTemplate(SceneView key, float cust, float state = 20f, PositionFlag config2 = PositionFlag.BottomRight, PublisherTemplate map3 = null)
			: this(key, cust, 1, state, config2, map3)
		{
		}

		public void Dispose()
		{
			if (_AttrTemplate)
			{
				Event current = Event.current;
				if (current.type == EventType.MouseDown && !_RecordTemplate.Contains(current.mousePosition))
				{
					current.Use();
					GUIUtility.hotControl = 0;
				}
			}
			GUILayout.EndArea();
			Handles.EndGUI();
		}

		private static Rect InvokeRequest(Rect reference, float result, int template_end, float instance2 = 20f, PositionFlag param3 = PositionFlag.Bottom, bool isident4 = false)
		{
			Rect result2 = reference;
			reference.x += 4f;
			reference.width -= 8f;
			float num = ((!isident4) ? result : (result * reference.width / 100f));
			float num2 = (float)template_end * instance2;
			bool flag = param3.PrintAccount();
			bool num3 = param3.FindAccount();
			bool flag2 = param3.CollectAccount();
			bool flag3 = param3.ValidateAccount();
			float x = (num3 ? reference.x : ((!flag) ? (reference.x + reference.width / 2f - num / 2f) : (reference.x + reference.width - num)));
			float y = (flag2 ? reference.y : ((!flag3) ? (reference.y + reference.height / 2f - num2 / 2f) : (reference.y + reference.height - num2)));
			result2.x = x;
			result2.y = y;
			result2.width = num;
			result2.height = num2;
			return result2;
		}

		internal static bool ForgotFactory()
		{
			return EnableFactory == null;
		}
	}

	internal class TokenizerProducerEntry
	{
		internal readonly ExporterErrorStatus _ItemTemplate = StopParam("CollabConflict Icon", "ds-icon-updateAvailable", "Update Available");

		internal readonly ExporterErrorStatus m_DecoratorTemplate = StopParam("Refresh", "ds-icon-checkForUpdate", "Check For Update");

		internal readonly ExporterErrorStatus _InvocationTemplate = StopParam("console.infoicon.sml", "ds-icon-announcement");

		internal readonly ExporterErrorStatus exporterTemplate = StopParam("console.warnicon.sml", "ds-icon-warning");

		internal readonly ExporterErrorStatus fieldTemplate = StopParam("console.erroricon.sml", "ds-icon-error");

		internal readonly ExporterErrorStatus m_CallbackTemplate = StopParam("VerticalLayoutGroup Icon", "ds-icon-hamMenu");

		internal readonly ExporterErrorStatus m_FilterTemplate = StopParam("_Help", "ds-icon-help");

		internal readonly GUIContent _ProxyTemplate = PatchManager("TestPassed", "Up to Date!");

		internal readonly GUIContent m_ComparatorTemplate = PatchManager("UnityEditor.InspectorWindow");

		internal readonly GUIContent adapterTemplate = PatchManager("Refresh", "Reset");

		internal readonly GUIContent m_ThreadTemplate = PatchManager("FolderOpened Icon", "Select a folder");

		internal readonly GUIContent m_PrinterTemplate = PatchManager("editicon.sml");

		internal readonly GUIContent m_ComposerTemplate = PatchManager("settings");

		internal readonly GUIContent codeTemplate = PatchManager("Selectable Icon");

		internal readonly GUIContent _FacadeTemplate = PatchManager("eyeDropper.Large");

		internal readonly GUIContent m_ProcessTemplate = PatchManager("Toolbar Minus", "Remove selection from list");

		internal readonly GUIContent _ConnectionTemplate = PatchManager("CollabCreate Icon");

		internal readonly GUIContent _CustomerTemplate = PatchManager("IN LockButton");

		internal readonly GUIContent queueTemplate = PatchManager("IN LockButton on");

		internal readonly GUIContent _AnnotationTemplate = PatchManager("d_scenepicking_pickable_hover@2x");

		internal readonly GUIContent m_ValueTemplate = PatchManager("d_scenepicking_notpickable@2x");

		internal readonly GUIContent repositoryTemplate = PatchManager("d_CustomTool@2x");

		internal readonly GUIContent refTemplate = new GUIContent("X", "Clear");

		internal readonly GUIContent _CandidateTemplate = new GUIContent("Handle Size", "The size multiplier of the custom ADO gizmos");

		internal readonly GUIContent m_ExpressionTemplate = new GUIContent("Animated Foldouts", "Enable animated foldouts in the editor");

		internal readonly GUIContent m_StubTemplate = new GUIContent("Show Name Labels", "Show names of transforms when toggling or selecting");

		internal readonly GUIContent m_MockTemplate = new GUIContent("Label Color", "The color of text displayed in the scene view");

		internal readonly GUIContent _InstanceTemplate = new GUIContent("General Color", "The color of the handles used for editing");

		internal readonly GUIContent m_ListenerTemplate = new GUIContent("Active Color", "The color of handles that are selected");

		internal readonly GUIContent observerTemplate = new GUIContent("Inactive Color", "The color of handles that are not selected");

		internal readonly GUIContent m_ParameterTemplate = new GUIContent("Mixed Color", "The color of handles that are active in some of the currently selected PhysBones but not others");

		internal readonly GUIContent importerTemplate = new GUIContent("Selection Color", "The color of handles when selecting");

		internal readonly GUIContent m_RegTemplate = new GUIContent("Function", "What you'd like to set up on the avatar");

		internal readonly GUIContent messageTemplate = new GUIContent("Property & Tip Overlay", "Displays the overlay for tooltips and property selection on the scene view");

		internal readonly GUIContent bridgeTemplate = new GUIContent("Tooltips", "Displays tooltips on how to use the current tool");

		private static TokenizerProducerEntry VisitFactory;

		internal TokenizerProducerEntry()
		{
			_AnnotationTemplate.tooltip = "Scene view clicks are allowed while editing.";
			m_ValueTemplate.tooltip = "Scene view clicks are ignored while editing.";
			m_ComposerTemplate.tooltip = "Open ADO Settings";
			_FacadeTemplate.tooltip = "Copy from another component of the same type";
			codeTemplate.tooltip = "Select through the scene view";
			m_PrinterTemplate.tooltip = "Edit through the scene view";
		}

		internal static bool DeleteFactory()
		{
			return VisitFactory == null;
		}
	}

	internal class UtilsTemplate
	{
		internal static readonly Color m_IdentifierTemplate = new Color(0.357f, 0.357f, 0.357f);

		internal readonly GUILayoutOption[] m_GlobalTemplate = new GUILayoutOption[2]
		{
			GUILayout.Width(EditorGUIUtility.singleLineHeight),
			GUILayout.Height(EditorGUIUtility.singleLineHeight)
		};

		internal readonly GUIStyle policyTemplate = new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold,
			fontSize = 18,
			alignment = TextAnchor.MiddleLeft
		};

		internal readonly GUIStyle _DispatcherTemplate = new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold,
			fontSize = 14,
			alignment = TextAnchor.MiddleLeft
		};

		internal readonly GUIStyle m_CollectionTemplate = new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold,
			fontSize = 12,
			alignment = TextAnchor.MiddleLeft
		};

		internal readonly GUIStyle m_ReaderTemplate = new GUIStyle(GUI.skin.label)
		{
			padding = new RectOffset(1, 1, 1, 1),
			fixedWidth = 18f,
			fixedHeight = 18f
		};

		internal readonly GUIStyle poolTemplate = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleCenter,
			richText = true
		};

		internal readonly GUIStyle _WriterTemplate = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Bold,
			richText = true
		};

		internal readonly GUIStyle _InterpreterTemplate = new GUIStyle(GUI.skin.label)
		{
			padding = new RectOffset(),
			margin = new RectOffset(1, 1, 1, 1)
		};

		internal readonly GUIStyle attributeTemplate = new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold,
			richText = true,
			wordWrap = true
		};

		internal readonly GUIStyle issuerTemplate = new GUIStyle(GUI.skin.button)
		{
			fontSize = 18,
			fontStyle = FontStyle.Bold
		};

		internal readonly GUIStyle m_WatcherTemplate = new GUIStyle(GUI.skin.label)
		{
			name = "Toggle"
		};

		internal readonly GUIStyle contextTemplate = new GUIStyle(GUI.skin.label)
		{
			richText = true
		};

		internal readonly GUIStyle m_SchemaTemplate = "AssetLabel";

		internal readonly GUIStyle _ReponseTemplate = "in bigtitle";

		internal readonly GUIStyle processorTemplate = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = ((!EditorGUIUtility.isProSkin) ? m_IdentifierTemplate : Color.gray)
			}
		};

		internal readonly GUIStyle _SingletonTemplate = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = ((!EditorGUIUtility.isProSkin) ? m_IdentifierTemplate : Color.gray)
			}
		};

		internal readonly GUIStyle procTemplate = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleRight,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = (EditorGUIUtility.isProSkin ? Color.gray : m_IdentifierTemplate)
			}
		};

		internal readonly GUIStyle m_SystemDic = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = ((!EditorGUIUtility.isProSkin) ? m_IdentifierTemplate : Color.gray)
			},
			contentOffset = new Vector2(-3f, 1.5f)
		};

		internal readonly GUIStyle _StructDic = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = ((!EditorGUIUtility.isProSkin) ? m_IdentifierTemplate : Color.gray)
			},
			name = "Toggle",
			hover = 
			{
				textColor = new Color(0.3f, 0.7f, 1f)
			}
		};

		internal readonly Color[] configDic = new Color[3] { infoTemplate, m_InitializerTemplate, _AuthenticationTemplate };

		internal readonly GUIStyle modelDic = new GUIStyle(GUI.skin.button)
		{
			margin = new RectOffset(0, 0, 2, 0),
			padding = new RectOffset(1, 1, 1, 1)
		};

		internal readonly GUIStyle templateDic = new GUIStyle(GUI.skin.label)
		{
			stretchWidth = true,
			fontSize = 15,
			richText = true,
			margin = new RectOffset(10, 0, 0, 0),
			fontStyle = FontStyle.Bold
		};

		internal readonly GUIStyle dicDic = new GUIStyle("RL FooterButton");

		internal static UtilsTemplate RevertFactory;

		internal static bool CalcCandidate()
		{
			return RevertFactory == null;
		}
	}

	internal enum EventCommands
	{
		Copy,
		Cut,
		Paste,
		Duplicate,
		Delete,
		SoftDelete,
		SelectAll,
		Find,
		FrameSelected,
		FrameSelectedWithLock,
		FocusProjectWindow
	}

	internal struct ServiceDic
	{
		internal string _ErrorDic;

		internal GUIStyle m_TaskDic;

		internal Vector3 producerDic;

		internal Quaternion methodDic;

		internal Vector3 m_ResolverDic;

		internal float _IteratorDic;

		internal float[] rulesDic;

		internal int m_TokenizerDic;

		internal Action _SpecificationDic;

		internal Func<ServiceDic, float[]> _AccountDic;

		internal Action<ServiceDic> managerDic;

		private static object NewCandidate;

		internal static ServiceDic CountWrapper(Vector3 instance, string token = "", float dic = 0.05f, int position_asset2 = -1, Action init3 = null)
		{
			return new ServiceDic
			{
				managerDic = NewWrapper,
				m_TaskDic = new GUIStyle(EditorStyles.boldLabel),
				_AccountDic = (ServiceDic sc) => new float[1] { HandleUtility.DistanceToCircle(sc.producerDic, sc._IteratorDic / 2f) },
				producerDic = instance,
				_IteratorDic = dic,
				_ErrorDic = token,
				m_TokenizerDic = position_asset2,
				_SpecificationDic = init3
			};
		}

		internal void SetWrapper()
		{
			managerDic(this);
		}

		internal float[] DeleteWrapper()
		{
			return _AccountDic(this);
		}

		internal static void NewWrapper(ServiceDic value)
		{
			Handles.SphereHandleCap(value.m_TokenizerDic, value.producerDic, Quaternion.identity, value._IteratorDic, EventType.Repaint);
			if (!string.IsNullOrWhiteSpace(value._ErrorDic))
			{
				LogoutManager(value._ErrorDic, value.producerDic, value._IteratorDic, value.m_TaskDic);
			}
		}

		internal static bool CalculateCandidate()
		{
			return NewCandidate == null;
		}
	}

	internal sealed class PolicyProducerList
	{
		private Texture2D testsDic;

		private bool m_RequestDic = true;

		private readonly string m_WrapperDic;

		private readonly bool algoDic;

		private readonly string mappingDic;

		internal bool _ParserDic;

		internal bool definitionDic;

		private bool _InitializerDic;

		private bool m_InfoDic;

		internal static PolicyProducerList FindCandidate;

		[SpecialName]
		internal Texture2D ChangeWrapper()
		{
			if (_ParserDic)
			{
				if (m_RequestDic && !testsDic)
				{
					InsertWrapper();
				}
				return testsDic;
			}
			if (!definitionDic)
			{
				if (!algoDic || _InitializerDic)
				{
					return null;
				}
				_InitializerDic = true;
				definitionDic = true;
				UpdateWrapper();
				return null;
			}
			return null;
		}

		internal PolicyProducerList(string i, bool requiresvisitor, string proc, bool isfirst2 = false)
		{
			m_WrapperDic = i;
			algoDic = requiresvisitor;
			mappingDic = proc;
		}

		internal void UpdateWrapper()
		{
			if (InsertWrapper())
			{
				return;
			}
			UnityWebRequest authenticationDic = new UnityWebRequest(m_WrapperDic)
			{
				downloadHandler = new DownloadHandlerBuffer()
			};
			authenticationDic.SendWebRequest().completed += delegate
			{
				if (!authenticationDic.isDone || authenticationDic.isHttpError || authenticationDic.isNetworkError)
				{
					authenticationDic.Dispose();
					return;
				}
				try
				{
					byte[] data = authenticationDic.downloadHandler.data;
					testsDic = new Texture2D(0, 0);
					testsDic.LoadImage(data);
					testsDic.Apply();
					_ParserDic = true;
					if (!string.IsNullOrWhiteSpace(mappingDic))
					{
						ExporterErrorStatus.ConcatWrapper(data, mappingDic);
						m_RequestDic = true;
					}
				}
				finally
				{
					authenticationDic.Dispose();
				}
			};
			definitionDic = false;
		}

		internal bool InsertWrapper()
		{
			if (m_RequestDic && !string.IsNullOrWhiteSpace(mappingDic))
			{
				m_RequestDic = false;
				Texture2D texture2D = ExporterErrorStatus.EnableWrapper(mappingDic);
				if (texture2D != null)
				{
					testsDic = texture2D;
					_ParserDic = true;
					definitionDic = false;
					m_RequestDic = true;
				}
			}
			return testsDic;
		}

		internal void PrepareWrapper()
		{
			if (ResolveWrapper())
			{
				Rect aspectRect = GUILayoutUtility.GetAspectRect((float)ChangeWrapper().width / (float)ChangeWrapper().height);
				ReadWrapper(aspectRect);
			}
		}

		internal void ListWrapper(EditorWindow setup, float cust = 0f, float dir = 60f)
		{
			if (ResolveWrapper())
			{
				if (!(setup == null))
				{
					ManageWrapper(setup.position.width, setup.position.height, cust, dir);
				}
				else
				{
					PrepareWrapper();
				}
			}
		}

		internal void ManageWrapper(float ident, float col, float rule = 0f, float cfg2 = 60f)
		{
			float num = (float)ChangeWrapper().height / (float)ChangeWrapper().width;
			float num2 = ident;
			float num3 = num2 * num;
			float num4 = col - cfg2;
			if (num3 > num4)
			{
				num3 = num4;
				num2 = num3 / num;
			}
			Rect rect = GUILayoutUtility.GetRect(num2, num3, GUILayout.ExpandWidth(expand: false));
			rect.x += (ident - num2) / 2f + rule;
			ReadWrapper(rect);
		}

		private void ReadWrapper(Rect last)
		{
			Event current = Event.current;
			switch (current.type)
			{
			case EventType.MouseDown:
				if (last.Contains(current.mousePosition) && current.button == 0)
				{
					Application.OpenURL("https://dreadrith.com/links");
					current.Use();
				}
				break;
			}
			if (Event.current.type == EventType.Repaint)
			{
				EditorGUIUtility.AddCursorRect(last, MouseCursor.Link);
			}
			GUI.DrawTexture(last, ChangeWrapper());
		}

		internal bool ResolveWrapper()
		{
			if (!m_InfoDic)
			{
				if (ChangeWrapper() == null)
				{
					return false;
				}
				if (Event.current.type == EventType.Layout)
				{
					m_InfoDic = true;
				}
				return true;
			}
			return true;
		}

		internal static bool SetCandidate()
		{
			return FindCandidate == null;
		}
	}

	internal sealed class RegServiceSerializer : IDisposable
	{
		internal bool workerDic;

		internal Texture2D m_ContainerDic;

		private static RegServiceSerializer FillCandidate;

		internal RegServiceSerializer(Texture2D param)
		{
			try
			{
				param.GetPixel(0, 0);
				workerDic = false;
				m_ContainerDic = param;
			}
			catch
			{
				int width = param.width;
				int height = param.height;
				workerDic = true;
				param.filterMode = FilterMode.Point;
				RenderTexture temporary = RenderTexture.GetTemporary(width, height);
				temporary.filterMode = FilterMode.Point;
				RenderTexture.active = temporary;
				Graphics.Blit(param, temporary);
				Texture2D texture2D = new Texture2D(width, height);
				texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
				RenderTexture.active = null;
				m_ContainerDic = texture2D;
			}
		}

		public void Dispose()
		{
			if (workerDic)
			{
				UnityEngine.Object.DestroyImmediate(m_ContainerDic);
			}
		}

		public static implicit operator Texture2D(RegServiceSerializer ident)
		{
			return ident.m_ContainerDic;
		}

		internal static bool SelectCandidate()
		{
			return FillCandidate == null;
		}
	}

	internal sealed class ExporterErrorStatus
	{
		private bool m_ExceptionDic = true;

		private GUIContent _PropertyDic;

		private Texture2D m_DescriptorDic;

		private readonly string factoryDic;

		private readonly string m_TagDic;

		private static ExporterErrorStatus CollectCandidate;

		[SpecialName]
		private GUIContent StopAlgo()
		{
			if (_PropertyDic.image == null && m_ExceptionDic)
			{
				_PropertyDic = new GUIContent(DefineAlgo())
				{
					tooltip = m_TagDic
				};
			}
			return _PropertyDic;
		}

		[SpecialName]
		internal Texture2D DefineAlgo()
		{
			if (m_ExceptionDic)
			{
				while (m_DescriptorDic == null)
				{
					m_ExceptionDic = false;
				}
			}
			return m_DescriptorDic;
		}

		public ExporterErrorStatus(Texture2D i, string visitor, string template = "")
		{
			m_DescriptorDic = i;
			factoryDic = visitor;
			m_TagDic = template;
			if (!(m_DescriptorDic == null))
			{
				ConcatWrapper(i.EncodeToPNG(), visitor);
			}
			else
			{
				SearchWrapper();
			}
			_PropertyDic = new GUIContent(i)
			{
				tooltip = template
			};
		}

		private void SearchWrapper()
		{
			m_DescriptorDic = EnableWrapper(factoryDic);
		}

		private static byte[] QueryWrapper(int[] item)
		{
			byte[] array = new byte[item.Length];
			for (int i = 0; i < item.Length; i++)
			{
				array[i] = (byte)item[i];
			}
			return array;
		}

		private static int[] OrderWrapper(byte[] v)
		{
			int num = v.Length;
			int[] array = new int[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = v[i];
			}
			return array;
		}

		internal static Texture2D EnableWrapper(string item)
		{
			int[] intArray = SessionState.GetIntArray(item, null);
			if (intArray != null)
			{
				try
				{
					byte[] data = QueryWrapper(intArray);
					Texture2D texture2D = new Texture2D(0, 0);
					texture2D.LoadImage(data);
					texture2D.Apply();
					return texture2D;
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
					SessionState.EraseIntArray(item);
				}
			}
			return null;
		}

		internal static void ConcatWrapper(byte[] setup, string map)
		{
			int[] value = OrderWrapper(setup);
			SessionState.SetIntArray(map, value);
		}

		public static implicit operator GUIContent(ExporterErrorStatus last)
		{
			return last.StopAlgo();
		}

		internal static bool VerifyCandidate()
		{
			return CollectCandidate == null;
		}
	}

	internal struct ConfigurationDic
	{
		internal readonly UnityEngine.Object m_ParamsDic;

		internal bool serializerDic;

		internal readonly Transform m_InterceptorDic;

		internal readonly int _DatabaseDic;

		internal float m_ValDic;

		internal float merchantDic;

		internal Vector3 _ClassDic;

		internal Quaternion _PredicateDic;

		internal static object GetCandidate;

		internal ConfigurationDic(VRCPhysBoneColliderBase item)
		{
			m_ParamsDic = item;
			serializerDic = true;
			m_InterceptorDic = item.GetRootTransform();
			_DatabaseDic = (int)item.shapeType;
			m_ValDic = item.radius;
			merchantDic = item.height;
			_ClassDic = item.position;
			_PredicateDic = item.rotation;
		}

		internal ConfigurationDic(ContactBase config)
		{
			m_ParamsDic = config;
			serializerDic = false;
			m_InterceptorDic = config.GetRootTransform();
			_DatabaseDic = (int)config.shapeType;
			m_ValDic = config.radius;
			merchantDic = config.height;
			_ClassDic = config.position;
			_PredicateDic = config.rotation;
		}

		internal void UpdateAlgo()
		{
			if (serializerDic)
			{
				VRCPhysBoneColliderBase obj = (VRCPhysBoneColliderBase)m_ParamsDic;
				obj.radius = m_ValDic;
				obj.height = merchantDic;
				obj.position = _ClassDic;
				obj.rotation = _PredicateDic;
			}
			else
			{
				ContactBase obj2 = (ContactBase)m_ParamsDic;
				obj2.radius = m_ValDic;
				obj2.height = merchantDic;
				obj2.position = _ClassDic;
				obj2.rotation = _PredicateDic;
				obj2.shapeType = (ContactBase.ShapeType)_DatabaseDic;
			}
		}

		internal void InsertAlgo(ContactBase v)
		{
			v.radius = m_ValDic;
			v.height = merchantDic;
			v.position = _ClassDic;
			v.rotation = _PredicateDic;
			v.shapeType = (ContactBase.ShapeType)_DatabaseDic;
		}

		internal void PrepareAlgo(VRCPhysBoneCollider ident)
		{
			ident.radius = m_ValDic;
			ident.height = merchantDic;
			ident.position = _ClassDic;
			ident.rotation = _PredicateDic;
			ident.shapeType = (VRCPhysBoneColliderBase.ShapeType)_DatabaseDic;
		}

		internal static bool CustomizeCandidate()
		{
			return GetCandidate == null;
		}
	}

	internal class ObjectTokenizerResolver
	{
		internal readonly VRCPhysBone _ServerDic;

		internal readonly Transform ruleDic;

		internal readonly List<StructTemplateExpression> _RoleDic;

		internal readonly int registryDic;

		internal List<List<StructTemplateExpression>> _StrategyDic;

		internal static ObjectTokenizerResolver CloneCandidate;

		[SpecialName]
		internal IEnumerable<Matrix4x4> CancelAlgo()
		{
			return _RoleDic.Select((StructTemplateExpression b) => b._ObjectDic);
		}

		internal ObjectTokenizerResolver(VRCPhysBone item)
		{
			_ServerDic = item;
			ruleDic = item.GetRootTransform();
			_RoleDic = new List<StructTemplateExpression>();
			ReadAlgo(ruleDic, 0);
			registryDic = _RoleDic.Max((StructTemplateExpression b) => b._TokenDic);
		}

		internal void ReadAlgo(Transform def, int pred_Y)
		{
			bool flag = false;
			StructTemplateExpression structTemplateExpression = new StructTemplateExpression();
			StructTemplateExpression stateDic = null;
			StructTemplateExpression structTemplateExpression2 = null;
			Quaternion q = def.rotation;
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < def.childCount; i++)
			{
				Transform child = def.GetChild(i);
				if (!_ServerDic.ignoreTransforms.Contains(child))
				{
					list.Add(child);
				}
			}
			bool statusDic;
			if (statusDic = list.Count == 0)
			{
				if (_ServerDic.endpointPosition != Vector3.zero)
				{
					Vector3 pos = def.TransformPoint(_ServerDic.endpointPosition);
					q = def.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(_ServerDic.endpointPosition));
					StructTemplateExpression obj = new StructTemplateExpression
					{
						mapperDic = this,
						productDic = ruleDic,
						_ObjectDic = Matrix4x4.TRS(pos, q, def.lossyScale),
						_TokenDic = pred_Y + 1,
						visitorDic = true,
						statusDic = true,
						m_HelperDic = structTemplateExpression
					};
					stateDic = obj;
					structTemplateExpression2 = obj;
				}
				else if (_RoleDic.Count != 0)
				{
					q = _RoleDic[_RoleDic.Count - 1]._ObjectDic.rotation;
				}
			}
			else if (list.Count > 1)
			{
				if (_ServerDic.multiChildType == VRCPhysBoneBase.MultiChildType.Average)
				{
					Vector3 zero = Vector3.zero;
					foreach (Transform item in list)
					{
						zero += item.position;
					}
					zero /= (float)list.Count;
					Vector3 toDirection = zero - def.position;
					q = def.rotation * Quaternion.FromToRotation(def.up, toDirection);
					structTemplateExpression2 = (stateDic = new StructTemplateExpression
					{
						mapperDic = this,
						productDic = ruleDic,
						_ObjectDic = Matrix4x4.TRS(zero, q, def.lossyScale),
						_TokenDic = pred_Y + 1,
						visitorDic = true,
						statusDic = true,
						m_HelperDic = structTemplateExpression
					});
				}
				else if (_ServerDic.multiChildType == VRCPhysBoneBase.MultiChildType.Ignore)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				structTemplateExpression.mapperDic = this;
				structTemplateExpression.productDic = ruleDic;
				structTemplateExpression.m_SetterDic = def;
				structTemplateExpression._ObjectDic = Matrix4x4.TRS(def.position, q, def.lossyScale);
				structTemplateExpression._TokenDic = pred_Y;
				structTemplateExpression.statusDic = statusDic;
				structTemplateExpression.stateDic = stateDic;
				StructTemplateExpression structTemplateExpression3 = _RoleDic.LastOrDefault();
				if (structTemplateExpression3 != null && !structTemplateExpression3.statusDic && structTemplateExpression3.stateDic == null)
				{
					structTemplateExpression3.stateDic = structTemplateExpression;
					structTemplateExpression.m_HelperDic = structTemplateExpression3;
				}
				_RoleDic.Add(structTemplateExpression);
			}
			if (structTemplateExpression2 != null)
			{
				_RoleDic.Add(structTemplateExpression2);
			}
			foreach (Transform item2 in list)
			{
				ReadAlgo(item2, pred_Y + 1);
			}
		}

		internal void ResolveAlgo()
		{
			HashSet<StructTemplateExpression> hashSet = new HashSet<StructTemplateExpression>();
			_StrategyDic = new List<List<StructTemplateExpression>>();
			foreach (StructTemplateExpression item in _RoleDic)
			{
				if (!hashSet.Contains(item))
				{
					List<StructTemplateExpression> list = new List<StructTemplateExpression>();
					for (StructTemplateExpression structTemplateExpression = item; structTemplateExpression != null; structTemplateExpression = structTemplateExpression.stateDic)
					{
						list.Add(structTemplateExpression);
						hashSet.Add(structTemplateExpression);
					}
					_StrategyDic.Add(list);
				}
			}
		}

		internal static bool ListCandidate()
		{
			return CloneCandidate == null;
		}
	}

	internal class StructTemplateExpression
	{
		internal ObjectTokenizerResolver mapperDic;

		internal Transform productDic;

		internal Transform m_SetterDic;

		internal Matrix4x4 _ObjectDic;

		internal bool visitorDic;

		internal bool statusDic;

		internal int _TokenDic;

		internal StructTemplateExpression stateDic;

		internal StructTemplateExpression m_HelperDic;

		internal static StructTemplateExpression RestartCandidate;

		[SpecialName]
		internal Vector3 ResetAlgo()
		{
			return _ObjectDic.GetColumn(3);
		}

		[SpecialName]
		internal float VisitAlgo()
		{
			return Mathf.Max(_ObjectDic.lossyScale.x, _ObjectDic.lossyScale.y, _ObjectDic.lossyScale.z);
		}

		[SpecialName]
		internal float InvokeAlgo()
		{
			return 1f / (float)mapperDic.registryDic * (float)_TokenDic;
		}

		internal float ForgotAlgo(AnimationCurve key)
		{
			if (key == null || key.length < 2)
			{
				return 1f;
			}
			return key.Evaluate(InvokeAlgo());
		}

		internal static bool MoveCandidate()
		{
			return RestartCandidate == null;
		}
	}

	internal readonly struct PageDic
	{
		internal readonly string _PrototypeDic;

		internal readonly AnimatorControllerParameterType _CreatorDic;

		internal readonly bool _BaseDic;

		private readonly FieldInfo getterDic;

		internal static object CreateCandidate;

		internal PageDic(string ident, AnimatorControllerParameterType cont, string temp)
		{
			_PrototypeDic = ident;
			_CreatorDic = cont;
			getterDic = (string.IsNullOrWhiteSpace(temp) ? null : typeof(VRCPhysBoneBase).GetField(temp, BindingFlags.Instance | BindingFlags.Public));
			_BaseDic = getterDic != null;
		}

		internal float MoveAlgo(VRCPhysBoneBase reference)
		{
			return (float)getterDic.GetValue(reference);
		}

		internal bool FillAlgo(VRCPhysBoneBase res)
		{
			return (bool)getterDic.GetValue(res);
		}

		public string ChangeAlgo(VRCPhysBoneBase asset)
		{
			if (_CreatorDic == AnimatorControllerParameterType.Bool)
			{
				return FillAlgo(asset).ToString();
			}
			return MoveAlgo(asset).ToString();
		}

		internal static bool CancelCandidate()
		{
			return CreateCandidate == null;
		}
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec advisorDic = new _003C_003Ec();

		public static Func<ParameterInfo, Type> _ConsumerDic;

		public static Func<Type, string> attrDic;

		public static Func<Type, string> recordDic;

		public static Func<VRCAvatarDescriptor.CustomAnimLayer, RuntimeAnimatorController> m_ItemDic;

		private static _003C_003Ec DefineCandidate;

		internal Type RemoveAlgo(ParameterInfo p)
		{
			return p.ParameterType;
		}

		internal string DestroyAlgo(Type ht)
		{
			return ht.Name;
		}

		internal string CreateAlgo(Type ht)
		{
			return ht.Name;
		}

		internal RuntimeAnimatorController CloneAlgo(VRCAvatarDescriptor.CustomAnimLayer l)
		{
			return l.animatorController;
		}

		internal static bool ViewCandidate()
		{
			return DefineCandidate == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass19_0<T> where T : UnityEngine.Object
	{
		public Func<T, bool> acceptConditions;

		internal static object ComputeCandidate;

		internal bool SetMapping(T c)
		{
			if (!_003C_003Ec__DisplayClass19_0<T>.NewMapping((UnityEngine.Object)c, (UnityEngine.Object)null))
			{
				return false;
			}
			return acceptConditions?.Invoke(c) ?? true;
		}

		internal bool DeleteMapping(T el)
		{
			return acceptConditions?.Invoke(el) ?? true;
		}

		internal static bool InitCandidate()
		{
			return ComputeCandidate == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass20_0<T> where T : UnityEngine.Object
	{
		public Func<T, bool> acceptConditions;

		private static object EnableCandidate;

		internal bool SelectMapping(T c)
		{
			if (!_003C_003Ec__DisplayClass20_0<T>.RunMapping((UnityEngine.Object)c, (UnityEngine.Object)null))
			{
				return false;
			}
			return acceptConditions?.Invoke(c) ?? true;
		}

		internal static bool ForgotCandidate()
		{
			return EnableCandidate == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass24_0<T> where T : UnityEngine.Object
	{
		public T[] enumerable;

		internal static object RevertCandidate;

		internal void WriteMapping(SerializedProperty sp)
		{
			T[] array = enumerable;
			for (int i = 0; i < array.Length; i++)
			{
				_003C_003Ec__DisplayClass24_1<T> _003C_003Ec__DisplayClass24_ = new _003C_003Ec__DisplayClass24_1<T>
				{
					e = array[i]
				};
				if (sp.WriteManager(_003C_003Ec__DisplayClass24_.ManageMapping) < 0)
				{
					int num = _003C_003Ec__DisplayClass24_0<T>.DefineMapping(sp) + 1;
					_003C_003Ec__DisplayClass24_0<T>.PushMapping(sp, num);
					_003C_003Ec__DisplayClass24_0<T>.InsertMapping(_003C_003Ec__DisplayClass24_0<T>.UpdateMapping(sp, num - 1), (UnityEngine.Object)_003C_003Ec__DisplayClass24_.e);
				}
			}
			_003C_003Ec__DisplayClass24_0<T>.ListMapping(_003C_003Ec__DisplayClass24_0<T>.PrepareMapping(sp));
		}

		internal static bool CalcDatabase()
		{
			return RevertCandidate == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass24_1<T> where T : UnityEngine.Object
	{
		public T e;

		private static object AwakeDatabase;

		internal bool ManageMapping(SerializedProperty e2, int _)
		{
			return _003C_003Ec__DisplayClass24_1<T>.ResolveMapping(_003C_003Ec__DisplayClass24_1<T>.ReadMapping(e2), (UnityEngine.Object)e);
		}

		internal static bool SortDatabase()
		{
			return AwakeDatabase == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass26_0<T> where T : UnityEngine.Object
	{
		public T[] enumerable;

		internal static object NewDatabase;

		internal void VerifyMapping(SerializedProperty sp)
		{
			T[] array = enumerable;
			for (int i = 0; i < array.Length; i++)
			{
				int num = sp.WriteManager(new _003C_003Ec__DisplayClass26_1<T>
				{
					e = array[i]
				}.ComputeMapping);
				if (num >= 0)
				{
					_003C_003Ec__DisplayClass26_0<T>.ConnectMapping(sp, num);
				}
			}
			_003C_003Ec__DisplayClass26_0<T>.InterruptMapping(_003C_003Ec__DisplayClass26_0<T>.CompareMapping(sp));
		}

		internal static bool CalculateDatabase()
		{
			return NewDatabase == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass26_1<T> where T : UnityEngine.Object
	{
		public T e;

		internal static object TestDatabase;

		internal bool ComputeMapping(SerializedProperty e2, int i)
		{
			return _003C_003Ec__DisplayClass26_1<T>.InitMapping(_003C_003Ec__DisplayClass26_1<T>.StartMapping(e2), (UnityEngine.Object)e);
		}

		internal static bool LoginDatabase()
		{
			return TestDatabase == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CHandleTask_003Ed__18<T> : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder<T> _003C_003Et__builder;

		public Task<T> taskHandle;

		public Action onComplete;

		public Action<Exception> onFailure;

		public Action OnCancelled;

		public Action<T> onSuccess;

		public Action onFinale;

		private TaskAwaiter<T> _003C_003Eu__1;

		private void MoveNext()
		{
			int num = _003C_003E1__state;
			T result;
			try
			{
				object obj;
				try
				{
					TaskAwaiter<T> awaiter;
					if (num == 0)
					{
						awaiter = _003C_003Eu__1;
						_003C_003Eu__1 = default(TaskAwaiter<T>);
						num = -1;
						_003C_003E1__state = -1;
					}
					else
					{
						awaiter = taskHandle.GetAwaiter();
						if (!awaiter.IsCompleted)
						{
							num = 0;
							_003C_003E1__state = 0;
							_003C_003Eu__1 = awaiter;
							_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
							return;
						}
					}
					obj = awaiter.GetResult();
				}
				catch
				{
					obj = default(T);
				}
				if (_003CHandleTask_003Ed__18<T>._202B_200D_206E_202E_206D_206C_206D_206F_206D_200C_206F_206F_200C_200E_200D_206A_200C_206A_206B_200D_200F_206E_206C_206C_200E_202E_202B_206A_202E_202E_200E_206E_206F_206D_206D_202C_206F_202B_200B_206E_202E((Task)taskHandle))
				{
					if (onComplete != null)
					{
						try
						{
							onComplete();
						}
						catch (Exception ex)
						{
							_003CHandleTask_003Ed__18<T>._200C_206B_202D_202D_202B_202D_202B_200F_206F_200C_206D_206A_202C_200B_202E_200B_202B_200C_200C_200F_200B_202B_200F_202D_200C_200D_200B_200E_206F_200D_200B_206A_202D_200F_202A_206F_202C_200F_202E(ex);
							throw;
						}
					}
					if (!_003CHandleTask_003Ed__18<T>._206A_206B_206D_206E_202A_200B_202A_200F_200F_200F_206B_206D_206C_202E_206A_200D_202E_202D_200D_202D_206D_206F_200D_206C_206B_200B_202A_202E_200E_206B_200B_206A_206E_200F_202E_200E_200E_206D_202E((Task)taskHandle) || _003CHandleTask_003Ed__18<T>._200C_206D_200B_200E_202A_200F_206A_200D_206F_202C_202A_202D_206B_200C_206C_200D_202B_202A_206B_202B_206F_206A_202E_206A_202B_202E_206D_202B_206A_206D_200D_206A_202B_200F_202D_206C_202C_200E_202A_206B_202E((Task)taskHandle))
					{
						if (!_003CHandleTask_003Ed__18<T>._206A_206B_206D_206E_202A_200B_202A_200F_200F_200F_206B_206D_206C_202E_206A_200D_202E_202D_200D_202D_206D_206F_200D_206C_206B_200B_202A_202E_200E_206B_200B_206A_206E_200F_202E_200E_200E_206D_202E((Task)taskHandle) && _003CHandleTask_003Ed__18<T>._200C_206D_200B_200E_202A_200F_206A_200D_206F_202C_202A_202D_206B_200C_206C_200D_202B_202A_206B_202B_206F_206A_202E_206A_202B_202E_206D_202B_206A_206D_200D_206A_202B_200F_202D_206C_202C_200E_202A_206B_202E((Task)taskHandle))
						{
							if (OnCancelled != null)
							{
								try
								{
									OnCancelled();
								}
								catch (Exception ex2)
								{
									_003CHandleTask_003Ed__18<T>._200C_206B_202D_202D_202B_202D_202B_200F_206F_200C_206D_206A_202C_200B_202E_200B_202B_200C_200C_200F_200B_202B_200F_202D_200C_200D_200B_200E_206F_200D_200B_206A_202D_200F_202A_206F_202C_200F_202E(ex2);
									throw ex2;
								}
							}
						}
						else
						{
							try
							{
								onSuccess((T)obj);
							}
							catch (Exception ex3)
							{
								_003CHandleTask_003Ed__18<T>._200C_206B_202D_202D_202B_202D_202B_200F_206F_200C_206D_206A_202C_200B_202E_200B_202B_200C_200C_200F_200B_202B_200F_202D_200C_200D_200B_200E_206F_200D_200B_206A_202D_200F_202A_206F_202C_200F_202E(ex3);
								throw ex3;
							}
						}
					}
					else
					{
						Exception ex4 = _003CHandleTask_003Ed__18<T>._206E_202B_206B_206B_202E_202A_200F_206B_202D_200B_206B_202E_202E_206C_200C_206B_202A_206A_200B_206C_206E_202E_200C_206A_202D_200E_202E_202E_202D_202D_202C_206D_202A_206A_206B_202E_206D_200C_202B_202E((Exception)_003CHandleTask_003Ed__18<T>._200B_206F_200B_200F_206D_206D_200D_202B_200E_202B_202B_206E_200C_202D_206F_206D_200B_206D_206F_200E_202E_206E_202B_206B_200D_206D_200B_206F_202A_200F_202B_200B_200C_206A_206C_200D_200E_206C_200E_200F_202E((Task)taskHandle));
						if (onFailure == null)
						{
							_003CHandleTask_003Ed__18<T>._200C_206B_202D_202D_202B_202D_202B_200F_206F_200C_206D_206A_202C_200B_202E_200B_202B_200C_200C_200F_200B_202B_200F_202D_200C_200D_200B_200E_206F_200D_200B_206A_202D_200F_202A_206F_202C_200F_202E(ex4);
						}
						else
						{
							try
							{
								onFailure(ex4);
							}
							catch (Exception ex5)
							{
								_003CHandleTask_003Ed__18<T>._200C_206B_202D_202D_202B_202D_202B_200F_206F_200C_206D_206A_202C_200B_202E_200B_202B_200C_200C_200F_200B_202B_200F_202D_200C_200D_200B_200E_206F_200D_200B_206A_202D_200F_202A_206F_202C_200F_202E(ex5);
								throw ex5;
							}
						}
					}
					if (onFinale != null)
					{
						try
						{
							onFinale();
						}
						catch (Exception ex6)
						{
							_003CHandleTask_003Ed__18<T>._200C_206B_202D_202D_202B_202D_202B_200F_206F_200C_206D_206A_202C_200B_202E_200B_202B_200C_200C_200F_200B_202B_200F_202D_200C_200D_200B_200E_206F_200D_200B_206A_202D_200F_202A_206F_202C_200F_202E(ex6);
							throw ex6;
						}
					}
				}
				else
				{
					_003CHandleTask_003Ed__18<T>._206F_200B_200B_206A_200E_206A_202B_206D_200E_200D_202E_206B_206C_206E_206F_202D_200C_202B_202B_202E_200D_206D_206A_200E_202C_200D_200B_202C_206F_206E_200D_206C_206C_202B_206D_200F_206C_202D_206A_202B_202E((object)"FATAL ERROR! Task not completed?");
				}
				result = (T)obj;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private static readonly Queue<Action> m_RequestTemplate = new Queue<Action>();

	internal static string m_WrapperTemplate = Application.unityVersion;

	internal static bool m_AlgoTemplate = m_WrapperTemplate.Contains("2022");

	private static readonly Stack<(Rect, MouseCursor)> m_MappingTemplate = new Stack<(Rect, MouseCursor)>();

	private static bool m_ParserTemplate;

	private static MethodInfo m_DefinitionTemplate;

	internal static Color m_InitializerTemplate = new Color(0.56f, 0.94f, 0.47f);

	internal static Color infoTemplate = new Color(1f, 0.25f, 0.25f);

	internal static Color _AuthenticationTemplate = new Color(0.99f, 0.95f, 0f);

	internal static Color _ClientTemplate = new Color(0.3f, 0.7f, 1f);

	internal static Color workerTemplate = new Color(0.7f, 0.3f, 1f);

	internal static Color _ContainerTemplate = new Color(1f, 0.65f, 0f);

	internal static Color m_ExceptionTemplate = new Color(1f, 0.5f, 0.7f);

	internal static TokenizerProducerEntry _PropertyTemplate;

	internal static UtilsTemplate m_DescriptorTemplate;

	private static Mesh m_FactoryTemplate;

	private static Material _TagTemplate;

	private static readonly int configurationTemplate = Shader.PropertyToID("_Color");

	private static readonly int m_ParamsTemplate = "RadiusHandleHash".GetHashCode();

	internal static MethodInfo m_SerializerTemplate;

	internal static Type m_InterceptorTemplate;

	internal static bool _DatabaseTemplate;

	internal static Type m_ValTemplate;

	internal static Type m_MerchantTemplate;

	internal static FieldInfo _ClassTemplate;

	internal static FieldInfo m_PredicateTemplate;

	internal static Type m_ServerTemplate;

	internal static MethodInfo _RuleTemplate;

	internal static readonly PolicyProducerList m_RoleTemplate = new PolicyProducerList("https://raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png", requiresvisitor: true, "DreadBanner.png");

	private static Texture2D registryTemplate;

	internal static readonly string[] strategyTemplate = new string[23]
	{
		"IsLocal", "Viseme", "Voice", "GestureLeft", "GestureRight", "GestureLeftWeight", "GestureRightWeight", "AngularY", "VelocityX", "VelocityY",
		"VelocityZ", "VelocityMagnitude", "Upright", "Grounded", "Seated", "AFK", "TrackingType", "VRMode", "MuteSelf", "InStation",
		"Earmuffs", "IsOnFriendsList", "AvatarVersion"
	};

	internal static readonly string[] m_IndexerTemplate = new string[23]
	{
		"Head", "Torso", "Hand", "Foot", "Finger", "FingerIndex", "FingerMiddle", "FingerRing", "FingerLittle", "HandL",
		"FootL", "FingerL", "FingerIndexL", "FingerMiddleL", "FingerRingL", "FingerLittleL", "HandR", "FootR", "FingerR", "FingerIndexR",
		"FingerMiddleR", "FingerRingR", "FingerLittleR"
	};

	internal static PageDic[] _BroadcasterTemplate = new PageDic[5]
	{
		new PageDic("_IsGrabbed", AnimatorControllerParameterType.Bool, "param_IsGrabbedValue"),
		new PageDic("_IsPosed", AnimatorControllerParameterType.Bool, "param_IsPosedValue"),
		new PageDic("_Stretch", AnimatorControllerParameterType.Float, "param_StretchValue"),
		new PageDic("_Squish", AnimatorControllerParameterType.Float, "param_SquishValue"),
		new PageDic("_Angle", AnimatorControllerParameterType.Float, "param_AngleValue")
	};

	internal static AdvisorDicBridge InterruptFactory;

	internal static bool PrintAccount(this PositionFlag setup)
	{
		if (!setup.HasFlag(PositionFlag.Right) && !setup.HasFlag(PositionFlag.TopRight))
		{
			return setup.HasFlag(PositionFlag.BottomRight);
		}
		return true;
	}

	internal static bool FindAccount(this PositionFlag ident)
	{
		if (ident.HasFlag(PositionFlag.Left) || ident.HasFlag(PositionFlag.TopLeft))
		{
			return true;
		}
		return ident.HasFlag(PositionFlag.BottomLeft);
	}

	internal static bool CollectAccount(this PositionFlag v)
	{
		if (v.HasFlag(PositionFlag.Top) || v.HasFlag(PositionFlag.TopLeft))
		{
			return true;
		}
		return v.HasFlag(PositionFlag.TopRight);
	}

	internal static bool ValidateAccount(this PositionFlag info)
	{
		if (info.HasFlag(PositionFlag.Bottom) || info.HasFlag(PositionFlag.BottomLeft))
		{
			return true;
		}
		return info.HasFlag(PositionFlag.BottomRight);
	}

	public static PositionFlag RestartAccount(this PositionFlag reference, bool isreg = false, bool isc = false)
	{
		PositionFlag positionFlag;
		if (reference > PositionFlag.Bottom)
		{
			if (reference <= PositionFlag.TopLeft)
			{
				if (reference == PositionFlag.TopRight)
				{
					positionFlag = PositionFlag.Left | PositionFlag.Bottom;
				}
				else
				{
					if (reference != PositionFlag.TopLeft)
					{
						goto IL_0030;
					}
					positionFlag = PositionFlag.Right | PositionFlag.Bottom;
				}
			}
			else if (reference != PositionFlag.BottomRight)
			{
				if (reference != PositionFlag.BottomLeft)
				{
					goto IL_0030;
				}
				positionFlag = PositionFlag.Right | PositionFlag.Top;
			}
			else
			{
				positionFlag = PositionFlag.Left | PositionFlag.Top;
			}
		}
		else if (reference > PositionFlag.Left)
		{
			if (reference == PositionFlag.Top)
			{
				positionFlag = PositionFlag.Right | PositionFlag.Left | PositionFlag.Bottom;
			}
			else
			{
				if (reference != PositionFlag.Bottom)
				{
					goto IL_0030;
				}
				positionFlag = PositionFlag.Right | PositionFlag.Left | PositionFlag.Top;
			}
		}
		else if (reference != PositionFlag.Right)
		{
			if (reference != PositionFlag.Left)
			{
				goto IL_0030;
			}
			positionFlag = PositionFlag.Right | PositionFlag.Top | PositionFlag.Bottom;
		}
		else
		{
			positionFlag = PositionFlag.Left | PositionFlag.Top | PositionFlag.Bottom;
		}
		goto IL_0014;
		IL_0014:
		if (isreg)
		{
			positionFlag &= ~(PositionFlag.Top | PositionFlag.Bottom);
		}
		if (isc)
		{
			positionFlag &= ~(PositionFlag.Right | PositionFlag.Left);
		}
		return positionFlag;
		IL_0030:
		positionFlag = PositionFlag.Middle;
		goto IL_0014;
	}

	internal static Rect ViewAccount(Rect instance, float attr = 2f)
	{
		return SearchAccount(instance, new Color(0.03f, 0.03f, 0.03f, 0.5f), new Color(0.137f, 0.137f, 0.137f, 0.5f), attr);
	}

	internal static Rect SearchAccount(Rect v, Color cont, Color third, float def2 = 3f)
	{
		float num = def2 + 2f;
		Rect position = v;
		position.x -= num / 2f;
		position.width += num;
		position.y -= num / 2f;
		position.height += num;
		if (cont != Color.clear)
		{
			GUI.DrawTexture(v, RunParam(cont), ScaleMode.StretchToFill, alphaBlend: true, 0f, cont, 0f, 8f);
		}
		if (third != Color.clear)
		{
			GUI.DrawTexture(position, RunParam(third), ScaleMode.StretchToFill, alphaBlend: true, 0f, third, def2, 8f);
		}
		Rect result = v;
		result.x += 4f;
		result.width -= 8f;
		result.y += 4f;
		result.height -= 8f;
		return result;
	}

	internal static bool QueryAccount(this AnimationCurve res, float visitor, out Keyframe state, out Keyframe selection2)
	{
		state = default(Keyframe);
		selection2 = default(Keyframe);
		if (res.length != 0)
		{
			if (res.length == 1)
			{
				state = res[0];
				return false;
			}
			int num = 0;
			Keyframe keyframe;
			while (true)
			{
				if (num < res.length)
				{
					keyframe = res[num];
					if (keyframe.time == visitor)
					{
						state = (selection2 = keyframe);
						return true;
					}
					if (!(keyframe.time >= visitor))
					{
						state = keyframe;
						num++;
						continue;
					}
					break;
				}
				return false;
			}
			selection2 = keyframe;
			return true;
		}
		return false;
	}

	internal static bool OrderAccount(this AnimationCurve init, float map, out float template)
	{
		template = 0f;
		if (init.QueryAccount(map, out var state, out var selection))
		{
			if (state.time != selection.time)
			{
				template = ConcatAccount(state, selection, map);
				return true;
			}
			template = state.outTangent;
			return true;
		}
		return false;
	}

	internal static float EnableAccount(float config, float pol, float serv, float def2, float pred3)
	{
		while (true)
		{
		}
	}

	internal static float ConcatAccount(Keyframe last, Keyframe token, float proc)
	{
		float num = token.time - last.time;
		float num2 = 57.29578f * Mathf.Atan(last.outTangent);
		float num3 = 57.29578f * Mathf.Atan(token.inTangent);
		float value = last.value;
		float value2 = token.value;
		float config = last.value + Mathf.Tan(num2 + 180f) * num;
		float def = token.value + Mathf.Tan(num3 + 180f) * num;
		float num4 = EnableAccount(config, value, value2, def, proc);
		return (EnableAccount(config, value, value2, def, proc + 1E-05f) - num4) / 1E-05f;
	}

	internal static bool LogoutAccount(this AnimatorController init, string token, AnimatorControllerParameterType comp, float last2)
	{
		bool num = init.parameters.All((AnimatorControllerParameter p) => p.name != token);
		if (num)
		{
			init.AddParameter(new AnimatorControllerParameter
			{
				name = token,
				type = comp,
				defaultBool = (last2 != 0f),
				defaultInt = (int)last2,
				defaultFloat = last2
			});
		}
		return num;
	}

	internal static void ExcludeAccount(Action last)
	{
		bool num = m_RequestTemplate.Count == 0;
		m_RequestTemplate.Enqueue(last);
		if (num)
		{
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(AddAccount));
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, new EditorApplication.CallbackFunction(AddAccount));
		}
	}

	private static void AddAccount()
	{
		while (m_RequestTemplate.Count != 0)
		{
			Action action = m_RequestTemplate.Dequeue();
			try
			{
				action();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
		EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(AddAccount));
	}

	internal static async Task<T> PublishAccount<T>(this Task<T> spec, Action<T> ivk, Action<Exception> c = null, Action ident2 = null, Action item3 = null, Action ident4 = null)
	{
		object obj;
		try
		{
			obj = await spec;
		}
		catch
		{
			obj = default(T);
		}
		if (_003CHandleTask_003Ed__18<T>._202B_200D_206E_202E_206D_206C_206D_206F_206D_200C_206F_206F_200C_200E_200D_206A_200C_206A_206B_200D_200F_206E_206C_206C_200E_202E_202B_206A_202E_202E_200E_206E_206F_206D_206D_202C_206F_202B_200B_206E_202E((Task)spec))
		{
			if (item3 != null)
			{
				try
				{
					item3();
				}
				catch (Exception ex)
				{
					_003CHandleTask_003Ed__18<T>._200C_206B_202D_202D_202B_202D_202B_200F_206F_200C_206D_206A_202C_200B_202E_200B_202B_200C_200C_200F_200B_202B_200F_202D_200C_200D_200B_200E_206F_200D_200B_206A_202D_200F_202A_206F_202C_200F_202E(ex);
					throw;
				}
			}
			if (!_003CHandleTask_003Ed__18<T>._206A_206B_206D_206E_202A_200B_202A_200F_200F_200F_206B_206D_206C_202E_206A_200D_202E_202D_200D_202D_206D_206F_200D_206C_206B_200B_202A_202E_200E_206B_200B_206A_206E_200F_202E_200E_200E_206D_202E((Task)spec) || _003CHandleTask_003Ed__18<T>._200C_206D_200B_200E_202A_200F_206A_200D_206F_202C_202A_202D_206B_200C_206C_200D_202B_202A_206B_202B_206F_206A_202E_206A_202B_202E_206D_202B_206A_206D_200D_206A_202B_200F_202D_206C_202C_200E_202A_206B_202E((Task)spec))
			{
				if (!_003CHandleTask_003Ed__18<T>._206A_206B_206D_206E_202A_200B_202A_200F_200F_200F_206B_206D_206C_202E_206A_200D_202E_202D_200D_202D_206D_206F_200D_206C_206B_200B_202A_202E_200E_206B_200B_206A_206E_200F_202E_200E_200E_206D_202E((Task)spec) && _003CHandleTask_003Ed__18<T>._200C_206D_200B_200E_202A_200F_206A_200D_206F_202C_202A_202D_206B_200C_206C_200D_202B_202A_206B_202B_206F_206A_202E_206A_202B_202E_206D_202B_206A_206D_200D_206A_202B_200F_202D_206C_202C_200E_202A_206B_202E((Task)spec))
				{
					if (ident2 != null)
					{
						try
						{
							ident2();
						}
						catch (Exception ex2)
						{
							_003CHandleTask_003Ed__18<T>._200C_206B_202D_202D_202B_202D_202B_200F_206F_200C_206D_206A_202C_200B_202E_200B_202B_200C_200C_200F_200B_202B_200F_202D_200C_200D_200B_200E_206F_200D_200B_206A_202D_200F_202A_206F_202C_200F_202E(ex2);
							throw ex2;
						}
					}
				}
				else
				{
					try
					{
						ivk((T)obj);
					}
					catch (Exception ex3)
					{
						_003CHandleTask_003Ed__18<T>._200C_206B_202D_202D_202B_202D_202B_200F_206F_200C_206D_206A_202C_200B_202E_200B_202B_200C_200C_200F_200B_202B_200F_202D_200C_200D_200B_200E_206F_200D_200B_206A_202D_200F_202A_206F_202C_200F_202E(ex3);
						throw ex3;
					}
				}
			}
			else
			{
				Exception ex4 = _003CHandleTask_003Ed__18<T>._206E_202B_206B_206B_202E_202A_200F_206B_202D_200B_206B_202E_202E_206C_200C_206B_202A_206A_200B_206C_206E_202E_200C_206A_202D_200E_202E_202E_202D_202D_202C_206D_202A_206A_206B_202E_206D_200C_202B_202E((Exception)_003CHandleTask_003Ed__18<T>._200B_206F_200B_200F_206D_206D_200D_202B_200E_202B_202B_206E_200C_202D_206F_206D_200B_206D_206F_200E_202E_206E_202B_206B_200D_206D_200B_206F_202A_200F_202B_200B_200C_206A_206C_200D_200E_206C_200E_200F_202E((Task)spec));
				if (c == null)
				{
					_003CHandleTask_003Ed__18<T>._200C_206B_202D_202D_202B_202D_202B_200F_206F_200C_206D_206A_202C_200B_202E_200B_202B_200C_200C_200F_200B_202B_200F_202D_200C_200D_200B_200E_206F_200D_200B_206A_202D_200F_202A_206F_202C_200F_202E(ex4);
				}
				else
				{
					try
					{
						c(ex4);
					}
					catch (Exception ex5)
					{
						_003CHandleTask_003Ed__18<T>._200C_206B_202D_202D_202B_202D_202B_200F_206F_200C_206D_206A_202C_200B_202E_200B_202B_200C_200C_200F_200B_202B_200F_202D_200C_200D_200B_200E_206F_200D_200B_206A_202D_200F_202A_206F_202C_200F_202E(ex5);
						throw ex5;
					}
				}
			}
			if (ident4 != null)
			{
				try
				{
					ident4();
				}
				catch (Exception ex6)
				{
					_003CHandleTask_003Ed__18<T>._200C_206B_202D_202D_202B_202D_202B_200F_206F_200C_206D_206A_202C_200B_202E_200B_202B_200C_200C_200F_200B_202B_200F_202D_200C_200D_200B_200E_206F_200D_200B_206A_202D_200F_202A_206F_202C_200F_202E(ex6);
					throw ex6;
				}
			}
		}
		else
		{
			_003CHandleTask_003Ed__18<T>._206F_200B_200B_206A_200E_206A_202B_206D_200E_200D_202E_206B_206C_206E_206F_202D_200C_202B_202B_202E_200D_206D_206A_200E_202C_200D_200B_202C_206F_206E_200D_206C_206C_202B_206D_200F_206C_202D_206A_202B_202E((object)"FATAL ERROR! Task not completed?");
		}
		return (T)obj;
	}

	internal static void InstantiateAccount<T>(Rect instance, Action<T> pol, Func<T, bool> temp = null, Action token2 = null) where T : UnityEngine.Object
	{
		Event current = Event.current;
		if ((current.type == EventType.DragPerform || current.type == EventType.DragUpdated) && instance.Contains(current.mousePosition))
		{
			T val = ((!typeof(T).IsSubclassOf(typeof(Component))) ? DragAndDrop.objectReferences.OfType<T>().FirstOrDefault((T el) => temp?.Invoke(el) ?? true) : DragAndDrop.objectReferences.Select(delegate(UnityEngine.Object o)
			{
				GameObject obj = o as GameObject;
				return ((object)obj != null) ? obj.GetComponent<T>() : null;
			}).FirstOrDefault((T c) => _003C_003Ec__DisplayClass19_0<T>.NewMapping((UnityEngine.Object)c, (UnityEngine.Object)null) && (temp?.Invoke(c) ?? true)));
			bool flag;
			if (flag = val != null)
			{
				token2?.Invoke();
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			}
			if (current.type == EventType.DragPerform && flag)
			{
				DragAndDrop.AcceptDrag();
				pol(val);
			}
			current.Use();
		}
	}

	internal static void RevertAccount<T>(Rect res, Action<IEnumerable<T>> vis, Func<T, bool> dir = null, Action item2 = null) where T : UnityEngine.Object
	{
		Event current = Event.current;
		if ((current.type == EventType.DragPerform || current.type == EventType.DragUpdated) && res.Contains(current.mousePosition))
		{
			T[] array = ((!typeof(T).IsSubclassOf(typeof(Component))) ? DragAndDrop.objectReferences.OfType<T>().ToArray() : (from c in DragAndDrop.objectReferences.Select(delegate(UnityEngine.Object o)
				{
					GameObject obj = o as GameObject;
					return ((object)obj != null) ? obj.GetComponent<T>() : null;
				})
				where _003C_003Ec__DisplayClass20_0<T>.RunMapping((UnityEngine.Object)c, (UnityEngine.Object)null) && (dir?.Invoke(c) ?? true)
				select c).ToArray());
			bool flag;
			if (flag = array.Length != 0)
			{
				item2?.Invoke();
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			}
			if (current.type == EventType.DragPerform && flag)
			{
				DragAndDrop.AcceptDrag();
				vis(array);
			}
			current.Use();
		}
	}

	internal static PositionFlag ReflectManager(PositionFlag asset, Rect ivk, PositionFlag third = PositionFlag.All)
	{
		CallManager(ivk, MouseCursor.Pan);
		float num = ivk.width / 3f;
		float num2 = ivk.height / 3f;
		foreach (PositionFlag item in PositionFlag.All.ListManager())
		{
			if (item == (PositionFlag)0 || (item & (item - 1)) != 0)
			{
				continue;
			}
			Rect rect = ivk;
			if (!item.PrintAccount())
			{
				if (!item.FindAccount())
				{
					rect.x += num;
				}
			}
			else
			{
				rect.x += num * 2f;
			}
			if (!item.ValidateAccount())
			{
				if (!item.CollectAccount())
				{
					rect.y += num2;
				}
			}
			else
			{
				rect.y += num2 * 2f;
			}
			rect.width = num;
			rect.height = num2;
			float num3 = 3f;
			float num4 = 1.5f;
			Rect v = rect;
			v.x += num4;
			v.y += num4;
			v.width -= num3;
			v.height -= num3;
			SearchAccount(v, Color.clear, Color.grey);
			if (!third.HasFlag(item))
			{
				SearchAccount(rect, new Color(1f, 0.5f, 0.5f, 0.5f), Color.clear);
			}
			else if (Event.current.type == EventType.Repaint)
			{
				if (!rect.Contains(Event.current.mousePosition))
				{
					SearchAccount(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f), Color.clear);
					continue;
				}
				asset = item;
				SearchAccount(rect, new Color(0.5f, 1f, 0.5f, 0.33f), Color.clear);
			}
		}
		return asset;
	}

	internal static void CountManager<T>(SerializedProperty setup) where T : UnityEngine.Object
	{
		bool hasMultipleDifferentValues;
		if (!(hasMultipleDifferentValues = setup.hasMultipleDifferentValues))
		{
			for (int i = 0; i < setup.arraySize; i++)
			{
				SerializedProperty arrayElementAtIndex = setup.GetArrayElementAtIndex(i);
				if (arrayElementAtIndex == null)
				{
					continue;
				}
				if (!(arrayElementAtIndex.objectReferenceValue == null))
				{
					using (new GUILayout.HorizontalScope())
					{
						EditorGUILayout.PropertyField(arrayElementAtIndex, GUIContent.none);
						if (AwakeManager(PrepareRequest().m_ProcessTemplate, ManageRequest().m_ReaderTemplate))
						{
							setup.DeleteArrayElementAtIndex(i);
						}
					}
				}
				else
				{
					setup.DeleteArrayElementAtIndex(i);
					i--;
				}
			}
		}
		Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(expand: true));
		GUIContent content = ((!hasMultipleDifferentValues) ? new GUIContent("[Drag And Drop Or Click Here]") : new GUIContent("Editing Multiple Lists", "Editing multiple lists with different values is not supported."));
		GUI.Label(controlRect, content, ManageRequest()._SingletonTemplate);
		if (hasMultipleDifferentValues)
		{
			return;
		}
		RevertAccount<T>(controlRect, setup.DeleteManager<T>);
		if (CalculateManager(controlRect))
		{
			InstantiateManager(null, typeof(T), null, null, createitem3: true, null, delegate(UnityEngine.Object o)
			{
				setup.DeleteManager<_0021_00210>((IEnumerable<_0021_00210>)(object)new T[1] { o.PrepareManager<T>() });
			});
		}
	}

	internal static void DeleteManager<T>(this SerializedProperty setup, IEnumerable<T> ord) where T : UnityEngine.Object
	{
		T[] enumerable = (ord as T[]) ?? ord.ToArray();
		setup.DefineManager(delegate(SerializedProperty sp)
		{
			T[] array = enumerable;
			for (int i = 0; i < array.Length; i++)
			{
				_003C_003Ec__DisplayClass24_1<T> _003C_003Ec__DisplayClass24_ = new _003C_003Ec__DisplayClass24_1<T>();
				_003C_003Ec__DisplayClass24_.e = array[i];
				if (sp.WriteManager(_003C_003Ec__DisplayClass24_.ManageMapping) < 0)
				{
					int num = _003C_003Ec__DisplayClass24_0<T>.DefineMapping(sp) + 1;
					_003C_003Ec__DisplayClass24_0<T>.PushMapping(sp, num);
					_003C_003Ec__DisplayClass24_0<T>.InsertMapping(_003C_003Ec__DisplayClass24_0<T>.UpdateMapping(sp, num - 1), (UnityEngine.Object)_003C_003Ec__DisplayClass24_.e);
				}
			}
			_003C_003Ec__DisplayClass24_0<T>.ListMapping(_003C_003Ec__DisplayClass24_0<T>.PrepareMapping(sp));
		});
	}

	internal static void SelectManager<T>(this SerializedProperty var1, IEnumerable<T> vis) where T : UnityEngine.Object
	{
		T[] enumerable = (vis as T[]) ?? vis.ToArray();
		var1.DefineManager(delegate(SerializedProperty sp)
		{
			T[] array = enumerable;
			for (int i = 0; i < array.Length; i++)
			{
				_003C_003Ec__DisplayClass26_1<T> _003C_003Ec__DisplayClass26_ = new _003C_003Ec__DisplayClass26_1<T>();
				_003C_003Ec__DisplayClass26_.e = array[i];
				int num = sp.WriteManager(_003C_003Ec__DisplayClass26_.ComputeMapping);
				if (num >= 0)
				{
					_003C_003Ec__DisplayClass26_0<T>.ConnectMapping(sp, num);
				}
			}
			_003C_003Ec__DisplayClass26_0<T>.InterruptMapping(_003C_003Ec__DisplayClass26_0<T>.CompareMapping(sp));
		});
	}

	internal static void RunManager<T>(this SerializedProperty first, bool acceptivk, params T[] elements) where T : UnityEngine.Object
	{
		first.StopManager(elements, acceptivk);
	}

	internal static void StopManager<T>(this SerializedProperty first, IEnumerable<T> cont, bool istemplate) where T : UnityEngine.Object
	{
		if (!istemplate)
		{
			first.SelectManager(cont);
		}
		else
		{
			first.DeleteManager(cont);
		}
	}

	internal static int WriteManager(this SerializedProperty info, Func<SerializedProperty, int, bool> result)
	{
		int num = info.arraySize - 1;
		while (num >= 0)
		{
			SerializedProperty arrayElementAtIndex = info.GetArrayElementAtIndex(num);
			if (!result(arrayElementAtIndex, num))
			{
				num--;
				continue;
			}
			return num;
		}
		return -1;
	}

	internal static void DefineManager(this SerializedProperty reference, Action<SerializedProperty> selection)
	{
		if (!reference.hasMultipleDifferentValues)
		{
			selection(reference);
			return;
		}
		string propertyPath = reference.propertyPath;
		UnityEngine.Object[] targetObjects = reference.serializedObject.targetObjects;
		for (int i = 0; i < targetObjects.Length; i++)
		{
			SerializedProperty obj = new SerializedObject(targetObjects[i]).FindProperty(propertyPath);
			selection(obj);
		}
	}

	internal static bool PushManager(this ref bool init)
	{
		return init = !init;
	}

	internal static Rect UpdateManager(this ref Rect value, float second, bool allowpool = false, float cont2 = -1f, bool iscfg3 = false, bool removespec4 = true)
	{
		Rect result = value;
		result.width = ((!allowpool) ? (second * value.width / 100f) : second);
		result.height = value.height;
		result.x = ((cont2 == -1f) ? value.x : (iscfg3 ? cont2 : (value.x + cont2 * value.width / 100f)));
		result.y = value.y;
		if (removespec4)
		{
			value.x = result.x + result.width;
			value.width -= result.width;
		}
		return result;
	}

	internal static void InsertManager(this AnimBool def, Action pol, Action rule = null)
	{
		if (def.faded != 0f)
		{
			EditorGUILayout.BeginFadeGroup(def.faded);
			pol();
			if (rule != null && 0f < def.faded && !(def.faded >= 1f))
			{
				rule();
			}
			EditorGUILayout.EndFadeGroup();
		}
	}

	internal static T PrepareManager<T>(this UnityEngine.Object spec) where T : UnityEngine.Object
	{
		if (typeof(T).IsSubclassOf(typeof(Component)))
		{
			GameObject obj = spec as GameObject;
			if ((object)obj != null)
			{
				return obj.GetComponent<T>();
			}
			return null;
		}
		return spec as T;
	}

	internal static IEnumerable<T> ListManager<T>(this T task) where T : Enum
	{
		return Enum.GetValues(typeof(T)).Cast<T>().Where(delegate(T value)
		{
			ref T reference = ref task;
			object flag = value;
			return reference.HasFlag((Enum)flag);
		});
	}

	internal static void ManageManager<T>(this IEnumerable<T> item, Action<T> ord)
	{
		foreach (T item2 in item)
		{
			ord(item2);
		}
	}

	public static Func<T, bool> ReadManager<T>(this Func<T, bool> first, Func<T, bool> col)
	{
		return (T arg) => first(arg) && col(arg);
	}

	internal static Type ResolveManager(string asset)
	{
		Type type = Type.GetType(asset);
		if (type != null)
		{
			return type;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			Type[] types = assemblies[i].GetTypes();
			type = types.FirstOrDefault((Type t) => t.FullName == asset);
			if (!(type != null))
			{
				type = types.FirstOrDefault((Type t) => t.Name == asset);
				if (type != null)
				{
					return type;
				}
				continue;
			}
			return type;
		}
		return null;
	}

	internal static Dictionary<Transform, Transform> VerifyManager(Transform ident, Transform ivk, bool ignoreutil, params Transform[] transformsToFind)
	{
		Dictionary<Transform, Transform> dictionary = new Dictionary<Transform, Transform>();
		foreach (Transform transform in transformsToFind)
		{
			if (!transform.IsChildOf(ident))
			{
				if (!ignoreutil)
				{
					dictionary.Add(transform, null);
				}
				continue;
			}
			string n = AnimationUtility.CalculateTransformPath(transform, ident);
			Transform transform2 = ivk.Find(n);
			if (!(transform2 == null && ignoreutil))
			{
				dictionary.Add(transform, transform2);
			}
		}
		return dictionary;
	}

	internal static Dictionary<T, T> ConnectManager<T>(Transform var1, Transform result, bool isrole, params T[] componentsToFind) where T : Component
	{
		Dictionary<T, T> dictionary = new Dictionary<T, T>();
		foreach (T val in componentsToFind)
		{
			if (val.transform.IsChildOf(var1))
			{
				string n = AnimationUtility.CalculateTransformPath(val.transform, var1);
				Transform transform = result.Find(n);
				if (transform == null)
				{
					if (!isrole)
					{
						dictionary.Add(val, null);
					}
					continue;
				}
				T[] components = val.GetComponents<T>();
				T[] components2 = transform.GetComponents<T>();
				int num = Array.IndexOf(components, val);
				if (!(num >= components2.Length && isrole))
				{
					dictionary.Add(val, components2[num]);
				}
			}
			else if (!isrole)
			{
				dictionary.Add(val, null);
			}
		}
		return dictionary;
	}

	internal static GUIContent CompareManager(this SerializedProperty var1)
	{
		return new GUIContent(var1.displayName, var1.tooltip);
	}

	internal static object InterruptManager(this SerializedProperty setup)
	{
		SerializedPropertyType propertyType = setup.propertyType;
		switch (propertyType)
		{
		case SerializedPropertyType.Generic:
		case SerializedPropertyType.Gradient:
		case SerializedPropertyType.ManagedReference:
			UnityEngine.Debug.LogWarning("Property type " + propertyType.ToString() + " does not support get value.");
			return null;
		default:
			return null;
		case SerializedPropertyType.BoundsInt:
			return setup.boundsIntValue;
		case SerializedPropertyType.LayerMask:
			return setup.intValue;
		case SerializedPropertyType.Vector2Int:
			return setup.vector2IntValue;
		case SerializedPropertyType.Rect:
			return setup.rectValue;
		case SerializedPropertyType.ArraySize:
			return setup.arraySize;
		case SerializedPropertyType.Color:
			return setup.colorValue;
		case SerializedPropertyType.Bounds:
			return setup.boundsValue;
		case SerializedPropertyType.ExposedReference:
			return setup.exposedReferenceValue;
		case SerializedPropertyType.ObjectReference:
			return setup.objectReferenceValue;
		case SerializedPropertyType.RectInt:
			return setup.rectIntValue;
		case SerializedPropertyType.String:
			return setup.stringValue;
		case SerializedPropertyType.Character:
			return (char)setup.intValue;
		case SerializedPropertyType.Boolean:
			return setup.boolValue;
		case SerializedPropertyType.Float:
			return setup.floatValue;
		case SerializedPropertyType.Vector4:
			return setup.vector4Value;
		case SerializedPropertyType.Vector2:
			return setup.vector2Value;
		case SerializedPropertyType.Vector3Int:
			return setup.vector3IntValue;
		case SerializedPropertyType.Quaternion:
			return setup.quaternionValue;
		case SerializedPropertyType.Vector3:
			return setup.vector3Value;
		case SerializedPropertyType.AnimationCurve:
			return setup.animationCurveValue;
		case SerializedPropertyType.FixedBufferSize:
			return setup.fixedBufferSize;
		case SerializedPropertyType.Integer:
			return setup.intValue;
		case SerializedPropertyType.Enum:
			return setup.enumValueIndex;
		}
	}

	internal static void ComputeManager(this SerializedProperty instance, object pred)
	{
		SerializedPropertyType propertyType = instance.propertyType;
		switch (propertyType)
		{
		case SerializedPropertyType.Generic:
		case SerializedPropertyType.Gradient:
		case SerializedPropertyType.FixedBufferSize:
		case SerializedPropertyType.ManagedReference:
			UnityEngine.Debug.LogWarning("Property type " + propertyType.ToString() + " does not support set value.");
			break;
		case SerializedPropertyType.BoundsInt:
			instance.boundsIntValue = (BoundsInt)pred;
			break;
		case SerializedPropertyType.Rect:
			instance.rectValue = (Rect)pred;
			break;
		case SerializedPropertyType.Vector3:
			instance.vector3Value = (Vector3)pred;
			break;
		case SerializedPropertyType.Character:
			instance.intValue = (char)pred;
			break;
		case SerializedPropertyType.Color:
			instance.colorValue = (Color)pred;
			break;
		case SerializedPropertyType.Vector2:
			instance.vector2Value = (Vector2)pred;
			break;
		case SerializedPropertyType.ExposedReference:
			instance.exposedReferenceValue = (UnityEngine.Object)pred;
			break;
		case SerializedPropertyType.String:
			instance.stringValue = (string)pred;
			break;
		case SerializedPropertyType.RectInt:
			instance.rectIntValue = (RectInt)pred;
			break;
		case SerializedPropertyType.LayerMask:
			instance.intValue = (int)pred;
			break;
		case SerializedPropertyType.Boolean:
			instance.boolValue = (bool)pred;
			break;
		case SerializedPropertyType.Bounds:
			instance.boundsValue = (Bounds)pred;
			break;
		case SerializedPropertyType.ObjectReference:
			instance.objectReferenceValue = (UnityEngine.Object)pred;
			break;
		case SerializedPropertyType.Integer:
			instance.intValue = (int)pred;
			break;
		case SerializedPropertyType.AnimationCurve:
			instance.animationCurveValue = (AnimationCurve)pred;
			break;
		case SerializedPropertyType.Float:
			instance.floatValue = (float)pred;
			break;
		case SerializedPropertyType.Vector4:
			instance.vector4Value = (Vector4)pred;
			break;
		case SerializedPropertyType.Vector2Int:
			instance.vector2IntValue = (Vector2Int)pred;
			break;
		case SerializedPropertyType.Quaternion:
			instance.quaternionValue = (Quaternion)pred;
			break;
		case SerializedPropertyType.Vector3Int:
			instance.vector3IntValue = (Vector3Int)pred;
			break;
		case SerializedPropertyType.Enum:
			instance.enumValueIndex = (int)pred;
			break;
		case SerializedPropertyType.ArraySize:
			instance.arraySize = (int)pred;
			break;
		}
	}

	internal static void StartManager()
	{
		if (Event.current.type == EventType.Repaint)
		{
			m_ParserTemplate = true;
			InitManager();
		}
	}

	internal static void InitManager()
	{
		m_MappingTemplate.Clear();
	}

	internal static void CheckManager()
	{
		if (Event.current.type == EventType.Repaint)
		{
			m_ParserTemplate = false;
			while (m_MappingTemplate.Count > 0)
			{
				var (screenRect, mouse) = m_MappingTemplate.Pop();
				EditorGUIUtility.AddCursorRect(GUIUtility.ScreenToGUIRect(screenRect), mouse);
			}
		}
	}

	internal static bool CancelManager(string spec, Color? selection = null)
	{
		return DisableManager(new GUIContent(spec), selection);
	}

	internal static bool DisableManager(GUIContent key, Color? reg = null)
	{
		if (!reg.HasValue)
		{
			reg = new Color(0.3f, 0.7f, 1f);
		}
		using (new TaskServiceClass(TaskServiceClass.ColoringType.BG, Color.clear))
		{
			using (new TaskServiceClass(TaskServiceClass.ColoringType.FG, reg.Value))
			{
				bool result = AwakeManager(key, ManageRequest().m_WatcherTemplate, GUILayout.ExpandWidth(expand: false));
				IncludeManager(reg);
				return result;
			}
		}
	}

	internal static void IncludeManager(Color? setup = null)
	{
		if (!setup.HasValue)
		{
			setup = new Color(0.3f, 0.7f, 1f);
		}
		if (Event.current.type == EventType.Repaint)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			Vector2 mousePosition = Event.current.mousePosition;
			if (lastRect.Contains(mousePosition))
			{
				EditorGUI.DrawRect(new Rect(lastRect.x, lastRect.yMax - 1f, lastRect.width, 1f), setup.Value);
			}
			EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.Link);
		}
	}

	internal static bool RateManager(GUIContent ident, float pol = -1f, float dic = -1f)
	{
		if (pol != -1f)
		{
			goto IL_000a;
		}
		goto IL_0019;
		IL_0019:
		pol = EditorGUIUtility.singleLineHeight;
		goto IL_000a;
		IL_000a:
		if (dic == -1f)
		{
			dic = EditorGUIUtility.singleLineHeight;
			goto IL_0019;
		}
		bool result = GUILayout.Button(ident, ManageRequest().m_ReaderTemplate, GUILayout.Width(pol), GUILayout.Height(dic));
		PopManager();
		return result;
	}

	internal static bool ForgotManager(Rect i, string cust, GUIStyle role = null)
	{
		return InvokeManager(i, new GUIContent(cust), role);
	}

	internal static bool AssetManager(Rect init, string selection)
	{
		return InvokeManager(init, new GUIContent(selection));
	}

	internal static bool TestManager(Rect var1, GUIContent selection)
	{
		return InvokeManager(var1, selection);
	}

	internal static bool ResetManager(string first, GUIStyle attr = null, params GUILayoutOption[] options)
	{
		return AwakeManager(new GUIContent(first), attr, options);
	}

	internal static bool GetManager(string v, params GUILayoutOption[] options)
	{
		return AwakeManager(new GUIContent(v), null, options);
	}

	internal static bool VisitManager(GUIContent first, params GUILayoutOption[] options)
	{
		return AwakeManager(first, null, options);
	}

	internal static bool AwakeManager(GUIContent ident, GUIStyle reg = null, params GUILayoutOption[] options)
	{
		return ChangeManager(isdef: false, ident, reg, options);
	}

	internal static bool InvokeManager(Rect item, GUIContent b, GUIStyle comp = null)
	{
		if (comp == null)
		{
			comp = GUI.skin.button;
		}
		bool result = GUI.Button(item, b, comp);
		PopManager();
		return result;
	}

	internal static bool CustomizeManager(bool identstop, string selection, GUIStyle role = null, params GUILayoutOption[] options)
	{
		return ChangeManager(identstop, new GUIContent(selection), role, options);
	}

	internal static bool MoveManager(bool lastinstall, string reg, params GUILayoutOption[] options)
	{
		return ChangeManager(lastinstall, new GUIContent(reg), null, options);
	}

	internal static bool FillManager(bool getlast, GUIContent reg, params GUILayoutOption[] options)
	{
		return ChangeManager(getlast, reg, null, options);
	}

	internal static bool ChangeManager(bool isdef, GUIContent cfg, GUIStyle role = null, params GUILayoutOption[] options)
	{
		if (role == null)
		{
			role = GUI.skin.button;
		}
		bool result = GUILayout.Toggle(isdef, cfg, role, options);
		PopManager();
		return result;
	}

	internal static bool CalculateManager(Rect var1 = default(Rect))
	{
		if (var1 == default(Rect))
		{
			var1 = GUILayoutUtility.GetLastRect();
		}
		PopManager(var1);
		Event current = Event.current;
		if (current.type == EventType.MouseDown && current.button == 0)
		{
			return var1.Contains(current.mousePosition);
		}
		return false;
	}

	internal static void PopManager(Rect first = default(Rect), bool isord = false)
	{
		if (Event.current.type == EventType.Repaint)
		{
			if (first == default(Rect))
			{
				first = GUILayoutUtility.GetLastRect();
			}
			CallManager(first, MouseCursor.Link, isord);
		}
	}

	internal static void CallManager(Rect i, MouseCursor pred, bool isstate = false)
	{
		if (!GUI.enabled && !isstate)
		{
			return;
		}
		if (!m_ParserTemplate)
		{
			if (Event.current.type == EventType.Repaint)
			{
				EditorGUIUtility.AddCursorRect(i, pred);
			}
			return;
		}
		if (m_AlgoTemplate)
		{
			i.y += 46f;
		}
		m_MappingTemplate.Push((GUIUtility.GUIToScreenRect(i), pred));
	}

	internal static void PostManager(Rect init, string attr, bool applythird = true, float t2 = 0f, float item3 = 0f, bool calckey4 = true, GUIStyle vis5 = null)
	{
		if (applythird && init.width > t2 + item3)
		{
			if (!calckey4)
			{
				init.x -= item3 + 2.5f;
			}
			else
			{
				init.x += item3 + 2.5f;
			}
			GUI.Label(init, attr, vis5 ?? (calckey4 ? ManageRequest().processorTemplate : ManageRequest().procTemplate));
		}
	}

	internal static void LoginManager(string ident, bool testtoken = true, float c = 0f, float asset2 = 0f, bool allowsetup3 = true)
	{
		PostManager(GUILayoutUtility.GetLastRect(), ident, testtoken, c, asset2, allowsetup3);
	}

	internal static void RemoveManager(int flags_reference = 2, int attr_length = 10)
	{
		Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(flags_reference + attr_length));
		controlRect.height = flags_reference;
		controlRect.y += (float)attr_length / 2f;
		controlRect.x -= 2f;
		controlRect.width += 6f;
		ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#595959" : "#858585", out var color);
		EditorGUI.DrawRect(controlRect, color);
	}

	internal static bool DestroyManager(Rect first, int minb)
	{
		if (GUIUtility.hotControl != minb)
		{
			Event current = Event.current;
			if (current.type == EventType.MouseDown && first.Contains(current.mousePosition))
			{
				GUIUtility.hotControl = minb;
				current.Use();
			}
			return false;
		}
		return true;
	}

	internal static void CreateManager()
	{
		GUILayout.Label(GUIContent.none, GUILayout.Width(EditorGUIUtility.singleLineHeight));
	}

	[SpecialName]
	private static MethodInfo UpdateRequest()
	{
		return m_DefinitionTemplate ?? (m_DefinitionTemplate = ((AdvisorDicBridge)(object)typeof(EditorGUI)).QueryEvent("TextFieldDropDown", BindingFlags.Static | BindingFlags.NonPublic, (Binder)null, new Type[4]
		{
			typeof(Rect),
			typeof(GUIContent),
			typeof(string),
			typeof(string[])
		}, (ParameterModifier[])null));
	}

	internal static string CloneManager(string res, string cust, string[] c, params GUILayoutOption[] layoutOptions)
	{
		return FlushManager(new GUIContent(res), cust, c, layoutOptions);
	}

	internal static string FlushManager(GUIContent spec, string cust, string[] res, params GUILayoutOption[] layoutOptins)
	{
		if (UpdateRequest() != null)
		{
			Rect rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.textField, layoutOptins);
			return (string)UpdateRequest().Invoke(null, new object[4] { rect, spec, cust, res });
		}
		return cust;
	}

	internal static string RegisterManager(Rect config, string connection, string third, string[] token2)
	{
		if (!(UpdateRequest() != null))
		{
			return third;
		}
		return (string)UpdateRequest().Invoke(null, new object[4]
		{
			config,
			new GUIContent(connection),
			third,
			token2
		});
	}

	internal static GUIContent PatchManager(string asset, string attr = null)
	{
		return new GUIContent(EditorGUIUtility.IconContent(asset))
		{
			tooltip = attr
		};
	}

	[SpecialName]
	internal static TokenizerProducerEntry PrepareRequest()
	{
		return _PropertyTemplate ?? (_PropertyTemplate = new TokenizerProducerEntry());
	}

	[SpecialName]
	internal static UtilsTemplate ManageRequest()
	{
		return m_DescriptorTemplate ?? (m_DescriptorTemplate = new UtilsTemplate());
	}

	internal static bool CalcManager(EventCommands info, string pol = "", bool outputtemplate = true)
	{
		if (string.IsNullOrEmpty(pol) || !(GUI.GetNameOfFocusedControl() != pol))
		{
			Event current = Event.current;
			if (current.type == EventType.ExecuteCommand || current.type == EventType.ValidateCommand)
			{
				bool num = info.ToString() == current.commandName;
				if (num && outputtemplate)
				{
					current.Use();
				}
				return num;
			}
			return false;
		}
		return false;
	}

	internal static bool MapManager(KeyCode var1, string selection = "", bool ispool = true)
	{
		if (!string.IsNullOrEmpty(selection) && GUI.GetNameOfFocusedControl() != selection)
		{
			return false;
		}
		Event current = Event.current;
		bool num = current.type == EventType.KeyDown && current.keyCode == var1;
		if (num && ispool)
		{
			current.Use();
		}
		return num;
	}

	internal static bool SortManager(string var1 = "", bool applyselection = true)
	{
		if (MapManager(KeyCode.Return, var1, applyselection))
		{
			return true;
		}
		return MapManager(KeyCode.KeypadEnter, var1, applyselection);
	}

	internal static bool SetupManager(string info = "", bool allowtoken = true)
	{
		return MapManager(KeyCode.Escape, info, allowtoken);
	}

	internal static bool PrintManager(string reference = "", bool isb = true)
	{
		if (!CalcManager(EventCommands.SoftDelete, reference, isb))
		{
			return CalcManager(EventCommands.Delete, reference, isb);
		}
		return true;
	}

	internal static bool FindManager(string res = "", Action ord = null, Action rule = null)
	{
		if (SortManager(res))
		{
			ord?.Invoke();
			return true;
		}
		if (SetupManager(res))
		{
			rule?.Invoke();
			return true;
		}
		return false;
	}

	internal static bool CollectManager(string config, Action cust = null, Action field = null)
	{
		if (FindManager(config, cust, field))
		{
			GUI.FocusControl(null);
			return true;
		}
		return false;
	}

	private static void ValidateManager(Vector3 def, Vector3 map, Vector3 serv, int int_0 = -1, Color? counter3 = null)
	{
		if (!counter3.HasValue)
		{
			counter3 = Handles.color;
		}
		if (int_0 != -1 && GUIUtility.hotControl == int_0)
		{
			counter3 = Color.yellow;
		}
		if (m_FactoryTemplate == null)
		{
			m_FactoryTemplate = SearchManager();
		}
		if (_TagTemplate == null)
		{
			_TagTemplate = QueryManager();
		}
		OrderManager(_TagTemplate);
		float num = Vector3.Distance(def, map);
		Vector3 normalized = (map - def).normalized;
		Matrix4x4 matrix = Matrix4x4.TRS(def, Quaternion.LookRotation(normalized, serv), new Vector3(num, num, num));
		_TagTemplate.SetColor(configurationTemplate, counter3.Value);
		_TagTemplate.SetPass(0);
		Graphics.DrawMeshNow(m_FactoryTemplate, matrix);
	}

	private static void RestartManager(Vector3 v, Quaternion cfg, float util, int int_0 = -1, Color? token3 = null)
	{
		ViewManager(Matrix4x4.TRS(v, cfg, new Vector3(util, util, util)), int_0, token3);
	}

	private static void ViewManager(Matrix4x4 item, int counter_max = -1, Color? helper = null)
	{
		if (!helper.HasValue)
		{
			helper = Handles.color;
		}
		if (counter_max != -1 && GUIUtility.hotControl == counter_max)
		{
			helper = Color.yellow;
		}
		if (m_FactoryTemplate == null)
		{
			m_FactoryTemplate = SearchManager();
		}
		if (_TagTemplate == null)
		{
			_TagTemplate = QueryManager();
		}
		OrderManager(_TagTemplate);
		_TagTemplate.SetColor(configurationTemplate, helper.Value);
		_TagTemplate.SetPass(0);
		Graphics.DrawMeshNow(m_FactoryTemplate, item);
	}

	private static Mesh SearchManager()
	{
		Mesh mesh = new Mesh();
		mesh.MarkDynamic();
		Vector3[] array = new Vector3[24]
		{
			new Vector3(0.1f, 0.1f, 0.1f),
			new Vector3(0.1f, -0.1f, 0.1f),
			Vector3.zero,
			new Vector3(0.1f, -0.1f, 0.1f),
			new Vector3(-0.1f, -0.1f, 0.1f),
			Vector3.zero,
			new Vector3(-0.1f, -0.1f, 0.1f),
			new Vector3(-0.1f, 0.1f, 0.1f),
			Vector3.zero,
			new Vector3(-0.1f, 0.1f, 0.1f),
			new Vector3(0.1f, 0.1f, 0.1f),
			Vector3.zero,
			new Vector3(0.1f, -0.1f, 0.1f),
			new Vector3(0.1f, 0.1f, 0.1f),
			Vector3.forward,
			new Vector3(-0.1f, -0.1f, 0.1f),
			new Vector3(0.1f, -0.1f, 0.1f),
			Vector3.forward,
			new Vector3(-0.1f, 0.1f, 0.1f),
			new Vector3(-0.1f, -0.1f, 0.1f),
			Vector3.forward,
			new Vector3(0.1f, 0.1f, 0.1f),
			new Vector3(-0.1f, 0.1f, 0.1f),
			Vector3.forward
		};
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = i;
		}
		mesh.vertices = array;
		mesh.triangles = array2;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.UploadMeshData(markNoLongerReadable: true);
		mesh.hideFlags = HideFlags.DontSave;
		return mesh;
	}

	private static Material QueryManager()
	{
		Material material = new Material(Shader.Find("UI/Unlit/Text"));
		OrderManager(material);
		return material;
	}

	private static void OrderManager(Material asset)
	{
		asset.hideFlags = HideFlags.DontSave;
		asset.SetInt("_Cull", 2);
		asset.SetInt("_ZWrite", 0);
		asset.SetInt("_ZTest", 8);
	}

	internal static void EnableManager(ServiceDic setup)
	{
		Event current = Event.current;
		setup.managerDic?.Invoke(setup);
		int tokenizerDic = setup.m_TokenizerDic;
		switch (current.GetTypeForControl(tokenizerDic))
		{
		case EventType.MouseDown:
			if (HandleUtility.nearestControl == tokenizerDic && current.button == 0)
			{
				setup._SpecificationDic();
				current.Use();
			}
			break;
		case EventType.Layout:
		{
			float[] array = setup.DeleteWrapper();
			foreach (float distance in array)
			{
				HandleUtility.AddControl(tokenizerDic, distance);
			}
			break;
		}
		}
	}

	internal static void ConcatManager(Transform ident, bool stripmap = false, bool writetag = false, bool isres2 = false, bool isv3 = false, bool movecounter4 = false, bool insertasset5 = false)
	{
		if (ident == null)
		{
			return;
		}
		bool num = !isv3 && (stripmap || Tools.current == Tool.Move);
		bool flag = !movecounter4 && (writetag || Tools.current == Tool.Rotate);
		if (!insertasset5)
		{
			if (isres2)
			{
				_ = 1;
			}
			else
				_ = Tools.current == Tool.Scale;
		}
		else
			_ = 0;
		bool flag2 = Tools.pivotRotation == PivotRotation.Global;
		if (num)
		{
			if (!flag2)
			{
				ident.position = Handles.PositionHandle(ident.position, ident.localRotation);
			}
			else
			{
				ident.position = Handles.PositionHandle(ident.position, ident.rotation);
			}
		}
		if (flag)
		{
			if (!flag2)
			{
				ident.localRotation = Handles.RotationHandle(ident.localRotation, ident.position);
			}
			else
			{
				ident.rotation = Handles.RotationHandle(ident.rotation, ident.position);
			}
		}
	}

	internal static void LogoutManager(string var1, Vector3 counter, float tag = 0f, GUIStyle param2 = null)
	{
		if (param2 == null)
		{
			param2 = EditorStyles.boldLabel;
		}
		GUIContent content = new GUIContent(var1);
		float x = param2.CalcSize(content).x;
		Vector3 vector = HandleUtility.WorldToGUIPointWithDepth(counter);
		if (vector.z > 0f)
		{
			Vector3 vector2 = vector - new Vector3(x * 0.5f, tag * 500f * 1f / vector.z + vector.z / (vector.z * 0.05f));
			Handles.BeginGUI();
			GUI.Label(new Rect(vector2, new Vector2(x, 20f)), content, param2);
			Handles.EndGUI();
		}
	}

	internal static Rect ExcludeManager(this SceneView asset)
	{
		return AddManager(GUIUtility.ScreenToGUIRect(asset.position));
	}

	internal static Rect AddManager(Rect asset)
	{
		if (!m_AlgoTemplate)
		{
			asset.y += 40f;
		}
		asset.height -= ((!m_AlgoTemplate) ? 21f : 27f);
		return asset;
	}

	internal static float PublishManager(Quaternion value, Vector3 second, float tag, bool getvisitor2 = true, float last3 = 1f)
	{
		float num = 90f;
		Vector3[] array = new Vector3[4]
		{
			value * Vector3.right,
			value * Vector3.forward,
			value * -Vector3.right,
			value * -Vector3.forward
		};
		Vector3 vector;
		if (Camera.current.orthographic)
		{
			vector = Camera.current.transform.forward;
		}
		else
		{
			vector = second - Matrix4x4.Inverse(Handles.matrix).MultiplyPoint(Camera.current.transform.position);
			float sqrMagnitude = vector.sqrMagnitude;
			float num2 = tag * tag;
			float num3 = num2 * num2 / sqrMagnitude;
			num = ((!((double)(num3 / num2) >= 1.0)) ? (Mathf.Atan2(Mathf.Sqrt(num2 - num3), Mathf.Sqrt(num3)) * 57.29578f) : (-1000f));
		}
		Color color = Handles.color;
		for (int i = 0; i < 4; i++)
		{
			int controlID = GUIUtility.GetControlID(m_ParamsTemplate, FocusType.Passive);
			float num4 = Vector3.Angle(array[i], -vector);
			if (((double)num4 > 5.0 && (double)num4 < 175.0) || GUIUtility.hotControl == controlID)
			{
				float a = (((double)num4 > (double)num + 5.0) ? Mathf.Clamp01(0.2f * color.a * 2f) : Mathf.Clamp01(color.a * 2f));
				Color color2 = new Color(color.r, color.g, color.b, a);
				Handles.color = ((QualitySettings.activeColorSpace != ColorSpace.Linear) ? color2 : color2.linear);
				Vector3 position = second + tag * array[i];
				bool changed = GUI.changed;
				GUI.changed = false;
				Vector3 a2 = Handles.Slider(controlID, position, array[i], HandleUtility.GetHandleSize(position) * 0.05f * last3, Handles.DotHandleCap, 0f);
				if (GUI.changed)
				{
					tag = Vector3.Distance(a2, second);
				}
				GUI.changed |= changed;
				Handles.color = color;
			}
			if (getvisitor2)
			{
				Handles.DrawWireArc(second, array[i], array[(i + 1) % 4], 360f, tag);
			}
		}
		return tag;
	}

	internal static void InstantiateManager(UnityEngine.Object task, Type cust, UnityEngine.Object dir = null, SerializedProperty config2 = null, bool createitem3 = true, List<int> result4 = null, Action<UnityEngine.Object> setup5 = null, Action<UnityEngine.Object> cust6 = null, bool isvisitor7 = true)
	{
		if (m_InterceptorTemplate == null)
		{
			m_InterceptorTemplate = Type.GetType("UnityEditor.ObjectSelector, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		}
		if (m_SerializerTemplate == null)
		{
			Type[] second = new Type[4]
			{
				typeof(bool),
				typeof(List<int>),
				typeof(Action<UnityEngine.Object>),
				typeof(Action<UnityEngine.Object>)
			};
			Type[] types = new Type[3]
			{
				typeof(UnityEngine.Object),
				typeof(Type),
				typeof(UnityEngine.Object)
			}.Concat(second).Concat(new Type[1] { typeof(bool) }).ToArray();
			m_SerializerTemplate = m_InterceptorTemplate.GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic, null, types, null);
			_DatabaseTemplate = m_SerializerTemplate != null;
			if (!_DatabaseTemplate)
			{
				Type[] types2 = new Type[3]
				{
					typeof(UnityEngine.Object),
					typeof(Type),
					typeof(SerializedProperty)
				}.Concat(second).ToArray();
				m_SerializerTemplate = m_InterceptorTemplate.GetMethod("Show", BindingFlags.Static | BindingFlags.Public, null, types2, null);
			}
		}
		EditorWindow window = EditorWindow.GetWindow(m_InterceptorTemplate);
		object[] second2 = new object[4] { createitem3, result4, setup5, cust6 };
		second2 = ((!_DatabaseTemplate) ? new object[3] { task, cust, config2 }.Concat(second2).ToArray() : new object[3] { task, cust, dir }.Concat(second2).Concat(new object[1] { isvisitor7 }).ToArray());
		m_SerializerTemplate.Invoke(window, second2);
	}

	internal static void RevertManager(Type value, Type connection)
	{
		if (!(m_ValTemplate == null))
		{
			goto IL_002d;
		}
		m_ValTemplate = Type.GetType("UnityEditor.CustomEditorAttributes, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		m_MerchantTemplate = Type.GetType("UnityEditor.CustomEditorAttributes+MonoEditorType, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		goto IL_0060;
		IL_002d:
		IList list = (_ClassTemplate.GetValue(null) as IDictionary)[value] as IList;
		m_PredicateTemplate.SetValue(list[0], connection);
		ReflectParam();
		goto IL_0060;
		IL_0060:
		_ClassTemplate = m_ValTemplate.GetField("kSCustomMultiEditors", BindingFlags.Static | BindingFlags.NonPublic);
		m_PredicateTemplate = m_MerchantTemplate.GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public);
		goto IL_002d;
	}

	internal static void ReflectParam()
	{
		if (m_ServerTemplate == null)
		{
			m_ServerTemplate = Type.GetType("UnityEditor.InspectorWindow, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			_RuleTemplate = m_ServerTemplate.GetMethod("RefreshInspectors", BindingFlags.Static | BindingFlags.NonPublic);
		}
		_RuleTemplate.Invoke(null, null);
	}

	internal static MethodInfo CountParam(this Type i, string pol)
	{
		MethodInfo[] array = (from m in i.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.Name == pol
			select m).ToArray();
		switch (array.Length)
		{
		default:
			UnityEngine.Debug.LogError("Multiple methods named " + pol + " found in " + i.Name);
			return null;
		case 0:
			UnityEngine.Debug.LogError("Method " + pol + " not found in " + i.Name);
			return null;
		case 1:
			return array[0];
		}
	}

	internal static MethodInfo SetParam(this Type res, string visitor, Type helper)
	{
		MethodInfo[] array = (from m in res.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.Name == visitor && m.GetParameters().Any((ParameterInfo p) => p.ParameterType == helper)
			select m).ToArray();
		switch (array.Length)
		{
		case 0:
			UnityEngine.Debug.LogError("Method " + visitor + " not found in " + res.Name + " with parameter of type " + helper.Name);
			return null;
		case 1:
			return array[0];
		default:
			UnityEngine.Debug.LogError("Multiple methods named " + visitor + " found in " + res.Name + " with parameter of type " + helper.Name);
			return null;
		}
	}

	internal static MethodInfo DeleteParam(this Type instance, string visitor, Type[] res)
	{
		MethodInfo[] array = (from m in instance.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.Name == visitor && !res.Except(m.GetParameters().Select(_003C_003Ec.advisorDic.RemoveAlgo)).Any()
			select m).ToArray();
		switch (array.Length)
		{
		default:
			UnityEngine.Debug.LogError("Multiple methods named " + visitor + " found in " + instance.Name + " with parameters of types " + string.Join(", ", res.Select((Type ht) => ht.Name)));
			return null;
		case 0:
			UnityEngine.Debug.LogError("Method " + visitor + " not found in " + instance.Name + " with parameters of types " + string.Join(", ", res.Select((Type ht) => ht.Name)));
			return null;
		case 1:
			return array[0];
		}
	}

	internal static MethodInfo NewParam(this Type var1, string connection, int dic_max)
	{
		MethodInfo[] array = (from m in var1.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.Name == connection && m.GetParameters().Length == dic_max
			select m).ToArray();
		switch (array.Length)
		{
		default:
			UnityEngine.Debug.LogError($"Multiple methods named {connection} found in {var1.Name} with {dic_max} parameters");
			return null;
		case 1:
			return array[0];
		case 0:
			UnityEngine.Debug.LogError($"Method {connection} not found in {var1.Name} with {dic_max} parameters");
			return null;
		}
	}

	private static Texture2D SelectParam(Texture2D value, float caller = 0.2f, int serv = 1)
	{
		if (!(value == null))
		{
			using (RegServiceSerializer regServiceSerializer = new RegServiceSerializer(value))
			{
				Texture2D containerDic = regServiceSerializer.m_ContainerDic;
				int width = containerDic.width;
				int height = containerDic.height;
				int num = width;
				int num2 = 0;
				int num3 = height;
				int num4 = 0;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						if (containerDic.GetPixel(j, i).a >= caller)
						{
							if (j < num)
							{
								num = j;
							}
							if (j > num2)
							{
								num2 = j;
							}
							if (i < num3)
							{
								num3 = i;
							}
							if (i > num4)
							{
								num4 = i;
							}
						}
					}
				}
				int num5 = num2 - num + 1;
				int num6 = num4 - num3 + 1;
				int num7 = num5 + serv * 2;
				int num8 = num6 + serv * 2;
				if (num5 < 1 || num6 < 1)
				{
					UnityEngine.Debug.LogError("Trimmed texture has zero size.");
					return null;
				}
				Color[] pixels = containerDic.GetPixels(num, num3, num5, num6);
				Texture2D texture2D = new Texture2D(num7, num8);
				for (int k = 0; k < num8; k++)
				{
					for (int l = 0; l < num7; l++)
					{
						if (l < serv || l >= serv + num5 || k < serv || k >= serv + num6)
						{
							texture2D.SetPixel(l, k, Color.clear);
						}
					}
				}
				texture2D.SetPixels(serv, serv, num5, num6, pixels);
				texture2D.Apply();
				return texture2D;
			}
		}
		throw new ArgumentNullException("texture");
	}

	internal static Texture2D RunParam(Color init)
	{
		if (!(registryTemplate == null))
		{
			registryTemplate.SetPixel(0, 0, init);
		}
		else
		{
			registryTemplate = new Texture2D(1, 1, TextureFormat.RGBAFloat, mipChain: false)
			{
				filterMode = FilterMode.Point,
				anisoLevel = 0
			};
		}
		registryTemplate.Apply();
		return registryTemplate;
	}

	internal static ExporterErrorStatus StopParam(string setup, string token, string comp = "")
	{
		Texture2D i = null;
		GUIContent gUIContent = EditorGUIUtility.IconContent(setup);
		if (gUIContent != null)
		{
			uint num4 = default(uint);
			while (true)
			{
				int num;
				int num2;
				if (!(gUIContent.image != null))
				{
					num = -984079066;
					num2 = -984079066;
				}
				else
				{
					num = -266021196;
					num2 = -266021196;
				}
				int num3 = num ^ (int)(num4 * 259068305);
				while (true)
				{
					switch ((num4 = (uint)(num3 ^ 0x19EC9FAB)) % 4)
					{
					case 1u:
						i = SelectParam(gUIContent.image as Texture2D);
						num3 = ((int)num4 * -599345641) ^ 0x6B42EA2F;
						continue;
					case 0u:
					case 2u:
						break;
					default:
						goto end_IL_0034;
					}
					break;
				}
				continue;
				end_IL_0034:
				break;
			}
		}
		return new ExporterErrorStatus(i, token, comp);
	}

	internal static VRCContactSender WriteParam(this VRCContactReceiver def, GameObject vis)
	{
		VRCContactSender vRCContactSender = Undo.AddComponent<VRCContactSender>(vis);
		new ConfigurationDic(def).InsertAlgo(vRCContactSender);
		vRCContactSender.collisionTags = def.collisionTags;
		vRCContactSender.rootTransform = def.rootTransform;
		if (vRCContactSender.rootTransform == vRCContactSender.transform)
		{
			vRCContactSender.rootTransform = null;
		}
		return vRCContactSender;
	}

	internal static VRCContactSender DefineParam(this VRCPhysBoneCollider v, GameObject ivk)
	{
		VRCContactSender vRCContactSender = Undo.AddComponent<VRCContactSender>(ivk);
		new ConfigurationDic(v).InsertAlgo(vRCContactSender);
		vRCContactSender.rootTransform = v.rootTransform;
		if (vRCContactSender.rootTransform == vRCContactSender.transform)
		{
			vRCContactSender.rootTransform = null;
		}
		return vRCContactSender;
	}

	internal static VRCContactReceiver PushParam(this VRCContactSender i, GameObject vis)
	{
		VRCContactReceiver vRCContactReceiver = Undo.AddComponent<VRCContactReceiver>(vis);
		new ConfigurationDic(i).InsertAlgo(vRCContactReceiver);
		vRCContactReceiver.collisionTags = i.collisionTags;
		vRCContactReceiver.rootTransform = i.rootTransform;
		if (vRCContactReceiver.rootTransform == vRCContactReceiver.transform)
		{
			vRCContactReceiver.rootTransform = null;
		}
		return vRCContactReceiver;
	}

	internal static VRCContactReceiver UpdateParam(this VRCPhysBoneCollider init, GameObject vis)
	{
		VRCContactReceiver vRCContactReceiver = Undo.AddComponent<VRCContactReceiver>(vis);
		new ConfigurationDic(init).InsertAlgo(vRCContactReceiver);
		vRCContactReceiver.rootTransform = init.rootTransform;
		if (vRCContactReceiver.rootTransform == vRCContactReceiver.transform)
		{
			vRCContactReceiver.rootTransform = null;
		}
		return vRCContactReceiver;
	}

	internal static VRCPhysBoneCollider InsertParam(this VRCContactReceiver task, GameObject cfg)
	{
		VRCPhysBoneCollider vRCPhysBoneCollider = Undo.AddComponent<VRCPhysBoneCollider>(cfg);
		new ConfigurationDic(task).PrepareAlgo(vRCPhysBoneCollider);
		vRCPhysBoneCollider.rootTransform = task.rootTransform;
		if (vRCPhysBoneCollider.rootTransform == vRCPhysBoneCollider.transform)
		{
			vRCPhysBoneCollider.rootTransform = null;
		}
		return vRCPhysBoneCollider;
	}

	internal static VRCPhysBoneCollider PrepareParam(this VRCContactSender first, GameObject ivk)
	{
		VRCPhysBoneCollider vRCPhysBoneCollider = Undo.AddComponent<VRCPhysBoneCollider>(ivk);
		new ConfigurationDic(first).PrepareAlgo(vRCPhysBoneCollider);
		vRCPhysBoneCollider.rootTransform = first.rootTransform;
		if (vRCPhysBoneCollider.rootTransform == vRCPhysBoneCollider.transform)
		{
			vRCPhysBoneCollider.rootTransform = null;
		}
		return vRCPhysBoneCollider;
	}

	internal static void ListParam(VRCAvatarDescriptor value, ref string[] token, ref int[] res)
	{
		string[] array = new string[8] { "Base", "Additive", "Gesture", "Action", "FX", "Sitting", "TPose", "IKPose" };
		if ((bool)(UnityEngine.Object)(object)value)
		{
			List<(string, int)> list = new List<(string, int)>();
			for (int i = 0; i < array.Length; i++)
			{
				int num = ((i != 0) ? (i + 1) : i);
				if (value.ManageParam((VRCAvatarDescriptor.AnimLayerType)num, out var _))
				{
					list.Add((array[i], num));
				}
			}
			token = new string[list.Count];
			res = new int[list.Count];
			for (int j = 0; j < list.Count; j++)
			{
				token[j] = list[j].Item1;
				res[j] = list[j].Item2;
			}
		}
		else
		{
			token = Array.Empty<string>();
			res = Array.Empty<int>();
		}
	}

	internal static bool ManageParam(this VRCAvatarDescriptor config, VRCAvatarDescriptor.AnimLayerType caller, out AnimatorController tag)
	{
		tag = (from l in config.baseAnimationLayers.Concat(config.specialAnimationLayers)
			where l.type == caller
			select l.animatorController).FirstOrDefault() as AnimatorController;
		return tag != null;
	}

	internal static bool ReadParam(byte[] info, int vis_end, bool moverole = true)
	{
		switch (info[vis_end])
		{
		default:
			info[vis_end] = ((!moverole) ? ((byte)1) : ((byte)0));
			return moverole;
		case 0:
			info[vis_end] = 1;
			return true;
		case 1:
			info[vis_end] = 0;
			return false;
		}
	}

	MethodInfo QueryEvent(string info, BindingFlags pol, Binder serv, Type[] res2, ParameterModifier[] cont3)
	{
		return ((Type)this).GetMethod(info, pol, serv, res2, cont3);
	}

	internal static bool UpdateFactory()
	{
		return InterruptFactory == null;
	}
}
