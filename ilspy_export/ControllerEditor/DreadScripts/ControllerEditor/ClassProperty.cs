using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace DreadScripts.ControllerEditor;

internal static class ClassProperty
{
	internal sealed class WatcherProcessor
	{
		internal readonly GUIContent candidateProcessor = new GUIContent(EditorGUIUtility.IconContent("Prefab Icon"))
		{
			tooltip = "Simple Mode"
		};

		internal readonly GUIContent productProcessor = new GUIContent(EditorGUIUtility.IconContent("GameObject Icon"))
		{
			tooltip = "Advanced Mode"
		};

		internal readonly GUIContent m_ExpressionProcessor = new GUIContent(EditorGUIUtility.IconContent("NetworkStartPosition Icon"))
		{
			tooltip = "Default Merge Clip"
		};

		internal readonly GUIContent systemProcessor = new GUIContent(EditorGUIUtility.IconContent("CompositeCollider2D Icon"))
		{
			tooltip = "Default Replace Clip"
		};

		internal readonly GUIContent workerProcessor = EditorGUIUtility.IconContent("curvekeyframesemiselectedoverlay");

		internal readonly GUIContent _FilterProcessor = EditorGUIUtility.IconContent("curvekeyframeweighted");

		internal readonly GUIContent stubProcessor = new GUIContent(EditorGUIUtility.IconContent("EditCollider"))
		{
			tooltip = "Linear"
		};

		internal readonly GUIContent readerProcessor = new GUIContent(EditorGUIUtility.IconContent("preAudioLoopOff"))
		{
			tooltip = "Looping Clip"
		};

		internal readonly GUIContent m_BridgeProcessor = new GUIContent(EditorGUIUtility.IconContent("SaveActive"))
		{
			tooltip = "Copy"
		};

		internal readonly GUIContent m_StrategyProcessor = new GUIContent(EditorGUIUtility.IconContent("Clipboard"))
		{
			tooltip = "Paste"
		};

		internal readonly GUIContent m_CustomerProcessor = new GUIContent(EditorGUIUtility.IconContent("Refresh"))
		{
			tooltip = "Restore Defaults"
		};

		internal readonly GUIContent _DatabaseProcessor = new GUIContent(EditorGUIUtility.IconContent("AnimatorState Icon"))
		{
			tooltip = "Animator States"
		};

		internal readonly GUIContent m_ExporterProcessor = new GUIContent(EditorGUIUtility.IconContent("UnityEditor.VersionControl"))
		{
			tooltip = "Switch"
		};

		internal readonly GUIContent _IdentifierProcessor = new GUIContent(EditorGUIUtility.IconContent("BlendTree Icon"))
		{
			tooltip = "Separate"
		};

		internal readonly GUIContent _AttrProcessor = new GUIContent(EditorGUIUtility.IconContent("AnimatorStateTransition Icon"))
		{
			tooltip = "Merge"
		};

		internal readonly GUIContent dispatcherProcessor = new GUIContent(EditorGUIUtility.IconContent("Animation.Record"))
		{
			tooltip = "Shared?"
		};

		internal readonly GUIContent m_RegistryProcessor = new GUIContent(EditorGUIUtility.IconContent("Animation.Play"))
		{
			tooltip = "Animation Clip"
		};

		internal readonly GUIContent _TagProcessor = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"))
		{
			tooltip = "Remove Condition"
		};

		internal readonly GUIContent importerProcessor = new GUIContent(EditorGUIUtility.IconContent("winbtn_win_close"))
		{
			tooltip = "Deselect"
		};

		internal readonly GUIContent _RequestProcessor = new GUIContent(EditorGUIUtility.IconContent("_Popup"))
		{
			tooltip = "Settings"
		};

		internal readonly GUIContent m_PrinterProcessor = new GUIContent(EditorGUIUtility.IconContent("BlendTree Icon"))
		{
			tooltip = "BlendTrees"
		};

		internal readonly GUIContent m_WriterProcessor = new GUIContent(EditorGUIUtility.IconContent("AnimatorStateMachine Icon"))
		{
			tooltip = "StateMachines"
		};

		internal readonly GUIContent m_ParamsProcessor = new GUIContent(EditorGUIUtility.IconContent("dll Script Icon"))
		{
			tooltip = "StateMachine Behaviors"
		};

		internal readonly GUIContent _ListenerProcessor = new GUIContent(EditorGUIUtility.IconContent("AnimatorStateTransition Icon"))
		{
			tooltip = "Transitions"
		};

		internal readonly Texture _GetterProcessor = EditorGUIUtility.IconContent("BodySilhouette").image;

		internal readonly Texture m_InterceptorProcessor = EditorGUIUtility.IconContent("TreeEditor.Trash").image;

		internal readonly GUIContent m_CreatorProcessor = new GUIContent(EditorGUIUtility.IconContent("CollabError"))
		{
			tooltip = "Invalid Pattern"
		};

		internal readonly GUIContent _EventProcessor = EditorGUIUtility.IconContent("FolderOpened Icon");

		internal readonly GUIContent infoProcessor = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"))
		{
			tooltip = "Remove element from list"
		};

		internal readonly GUIContent[] m_FacadeProcessor;

		internal readonly ErrorPolicy m_AdvisorProcessor = NewList("CollabConflict Icon", "ds-icon-updateAvailable", "Update Available");

		internal readonly ErrorPolicy _CallbackProcessor = NewList("Refresh", "ds-icon-refresh", "Reset");

		internal readonly ErrorPolicy indexerProcessor = NewList("console.infoicon.sml", "ds-icon-announcement");

		internal readonly ErrorPolicy issuerProcessor = NewList("console.warnicon.sml", "ds-icon-warning");

		internal readonly ErrorPolicy prototypeProcessor = NewList("console.warnicon.inactive.sml", "ds-icon-note");

		internal readonly ErrorPolicy ruleProcessor = NewList("console.erroricon.sml", "ds-icon-error");

		internal readonly ErrorPolicy m_SingletonProcessor = NewList("VerticalLayoutGroup Icon", "ds-icon-hamMenu");

		internal readonly ErrorPolicy _FactoryProcessor = NewList("Lightmapping", "ds-icon-light", "Ping");

		internal readonly ErrorPolicy _AccountProcessor = NewList("_Help", "ds-icon-help");

		internal readonly ErrorPolicy _RefProcessor = NewList("scenevis_visible_hover", "ds-icon-visible", "Visible");

		internal readonly ErrorPolicy m_StatusProcessor = NewList("scenevis_hidden_hover", "ds-icon-hidden", "Hidden");

		internal readonly ErrorPolicy m_TokenProcessor = NewList("Mirror", "ds-icon-mirror", "Mirror");

		internal readonly GUIContent m_CodeProcessor = PushQueue("TestPassed", "Up to Date!");

		internal readonly GUIContent _DicProcessor = PushQueue("UnityEditor.InspectorWindow");

		internal readonly GUIContent invocationProcessor = PushQueue("FolderOpened Icon", "Select a folder");

		internal readonly GUIContent roleProcessor = PushQueue("editicon.sml");

		internal readonly GUIContent _ParamProcessor = PushQueue("settings");

		internal readonly GUIContent m_ModelProcessor = PushQueue("Selectable Icon");

		internal readonly GUIContent m_TokenizerProcessor = PushQueue("eyeDropper.Large");

		internal readonly GUIContent m_DecoratorProcessor = PushQueue("Toolbar Minus", "Remove selection from list");

		internal readonly GUIContent m_ComparatorProcessor = PushQueue("CollabCreate Icon");

		internal readonly GUIContent exceptionProcessor = PushQueue("d_CreateAddNew@2x");

		internal readonly GUIContent _ObjectProcessor = PushQueue("IN LockButton");

		internal readonly GUIContent _UtilsProcessor = PushQueue("IN LockButton on");

		internal readonly GUIContent m_ValProcessor = PushQueue("d_scenepicking_pickable_hover@2x");

		internal readonly GUIContent _ValueProcessor = PushQueue("d_scenepicking_notpickable@2x");

		internal readonly GUIContent m_MerchantProcessor = PushQueue("d_CustomTool@2x");

		internal readonly GUIContent authenticationProcessor = PushQueue("winbtn_win_close");

		internal readonly GUIContent _ReponseProcessor = PushQueue("Search Icon");

		internal readonly GUIContent _PoolProcessor = PushQueue("BlendTree Icon");

		internal readonly GUIContent parameterProcessor = PushQueue("UnityEditor.FindDependencies");

		internal readonly GUIContent m_ComposerProcessor = PushQueue("UnityEditor.SceneHierarchyWindow");

		internal readonly GUIContent repositoryProcessor = PushQueue("Collab.FolderAdded");

		internal readonly GUIContent _MappingProcessor = PushQueue("AlphabeticalSorting", "Sort");

		private static WatcherProcessor ConnectCandidate;

		internal WatcherProcessor()
		{
			m_FacadeProcessor = new GUIContent[6] { m_WriterProcessor, _DatabaseProcessor, _ListenerProcessor, m_PrinterProcessor, m_ParamsProcessor, m_RegistryProcessor };
		}

		internal static bool ViewCandidate()
		{
			return ConnectCandidate == null;
		}
	}

	internal class BaseProcessor
	{
		internal MethodInfo m_ContainerProcessor;

		internal readonly GUIStyle _ClassProcessor = new GUIStyle(EditorStyles.miniLabel)
		{
			alignment = TextAnchor.MiddleCenter
		};

		internal readonly GUIStyle m_MockProcessor = new GUIStyle(GUI.skin.label)
		{
			normal = 
			{
				textColor = Color.cyan
			},
			hover = 
			{
				textColor = Color.cyan
			}
		};

		internal readonly GUIStyle m_InstanceProcessor = new GUIStyle
		{
			margin = new RectOffset(4, 4, 4, 4),
			alignment = TextAnchor.MiddleCenter
		};

		internal readonly GUIContent fieldProcessor = new GUIContent(EditorGUIUtility.IconContent("TreeEditor.Trash"))
		{
			tooltip = "Remove"
		};

		internal readonly GUIContent m_AttributeProcessor = new GUIContent("B", "Behaviours");

		internal readonly GUIContent _ClientProcessor = new GUIContent("WD", "Write Defaults");

		internal readonly GUIStyle configProcessor = new GUIStyle
		{
			padding = new RectOffset(2, 2, 2, 2),
			margin = new RectOffset()
		};

		internal readonly GUIStyle descriptorProcessor = new GUIStyle(GUI.skin.GetStyle("DropDownButton"))
		{
			alignment = TextAnchor.MiddleLeft,
			contentOffset = new Vector2(2.5f, 0f),
			fixedHeight = 0f
		};

		internal readonly GUIStyle _TemplateProcessor = GUI.skin.GetStyle("RL FooterButton");

		internal readonly GUIStyle messageProcessor;

		internal readonly GUIStyle m_CollectionProcessor;

		internal static readonly Color m_ParserProcessor = new Color(0.357f, 0.357f, 0.357f);

		internal readonly GUILayoutOption[] managerProcessor = new GUILayoutOption[2]
		{
			GUILayout.Width(EditorGUIUtility.singleLineHeight),
			GUILayout.Height(EditorGUIUtility.singleLineHeight)
		};

		internal readonly GUIStyle itemProcessor = new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold,
			fontSize = 18,
			alignment = TextAnchor.MiddleLeft
		};

		internal readonly GUIStyle _SpecificationProcessor = new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold,
			fontSize = 14,
			alignment = TextAnchor.MiddleLeft
		};

		internal readonly GUIStyle methodProcessor = new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold,
			fontSize = 12,
			alignment = TextAnchor.MiddleLeft
		};

		internal readonly GUIStyle schemaProcessor = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleCenter,
			richText = true,
			fontStyle = FontStyle.Bold,
			fontSize = 18
		};

		internal readonly GUIStyle broadcasterProcessor = new GUIStyle(GUI.skin.label)
		{
			padding = new RectOffset(1, 1, 1, 1),
			fixedWidth = 18f,
			fixedHeight = 18f
		};

		internal readonly GUIStyle _ProxyProcessor = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleCenter,
			richText = true
		};

		internal readonly GUIStyle _StructProcessor = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Bold,
			richText = true
		};

		internal readonly GUIStyle m_ServiceProcessor = new GUIStyle(GUI.skin.label)
		{
			padding = new RectOffset(),
			margin = new RectOffset(1, 1, 1, 1)
		};

		internal readonly GUIStyle m_StateProcessor = new GUIStyle(GUI.skin.button)
		{
			padding = new RectOffset(0, 0, 2, 2)
		};

		internal readonly GUIStyle m_GlobalProcessor = new GUIStyle(GUI.skin.label)
		{
			richText = true,
			wordWrap = true
		};

		internal readonly GUIStyle m_TaskProcessor = new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold,
			richText = true,
			wordWrap = true
		};

		internal readonly GUIStyle _ProcessProcessor = new GUIStyle(GUI.skin.button)
		{
			fontSize = 14,
			fixedHeight = 30f
		};

		internal readonly GUIStyle m_ProducerProcessor = new GUIStyle(GUI.skin.button)
		{
			fontSize = 18,
			fixedHeight = 40f,
			fontStyle = FontStyle.Bold
		};

		internal readonly GUIStyle _IteratorProcessor = new GUIStyle(GUI.skin.label)
		{
			name = "Toggle"
		};

		internal readonly GUIStyle _PublisherProcessor = new GUIStyle(GUI.skin.label)
		{
			richText = true
		};

		internal readonly GUIStyle m_ConfigurationProcessor = new GUIStyle(GUI.skin.textArea)
		{
			richText = true
		};

		internal readonly GUIStyle procProcessor = "AssetLabel";

		internal readonly GUIStyle _WrapperObserver = "in bigtitle";

		internal readonly GUIStyle annotationObserver = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = ((!EditorGUIUtility.isProSkin) ? m_ParserProcessor : Color.gray)
			}
		};

		internal readonly GUIStyle _VisitorObserver = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = (EditorGUIUtility.isProSkin ? Color.gray : m_ParserProcessor)
			}
		};

		internal readonly GUIStyle algoObserver = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleRight,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = ((!EditorGUIUtility.isProSkin) ? m_ParserProcessor : Color.gray)
			}
		};

		internal readonly GUIStyle _MapperObserver = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = ((!EditorGUIUtility.isProSkin) ? m_ParserProcessor : Color.gray)
			},
			contentOffset = new Vector2(-3f, 1.5f)
		};

		internal readonly GUIStyle m_InitializerObserver = new GUIStyle(EditorStyles.miniLabel)
		{
			alignment = TextAnchor.MiddleLeft,
			richText = true,
			contentOffset = new Vector2(-3f, 3f)
		};

		internal readonly GUIStyle m_DefinitionObserver = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = (EditorGUIUtility.isProSkin ? Color.gray : m_ParserProcessor)
			},
			name = "Toggle",
			hover = 
			{
				textColor = new Color(0.3f, 0.7f, 1f)
			}
		};

		internal readonly GUIStyle m_RegObserver = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleLeft,
			richText = true,
			name = "Toggle",
			hover = 
			{
				textColor = m_AlgoProcessor
			}
		};

		private static BaseProcessor ChangeCandidate;

		[SpecialName]
		internal MethodInfo LoginError()
		{
			return m_ContainerProcessor ?? (m_ContainerProcessor = typeof(EditorGUILayout).GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).First((MethodInfo m) => m.Name == "TextFieldDropDown" && m.GetParameters().Length == 3));
		}

		internal BaseProcessor()
		{
			messageProcessor = new GUIStyle(algoObserver)
			{
				contentOffset = new Vector2(-2.5f, 0f),
				normal = 
				{
					textColor = ((!EditorGUIUtility.isProSkin) ? PushList(91f) : Color.gray)
				}
			};
			m_CollectionProcessor = new GUIStyle(messageProcessor)
			{
				alignment = TextAnchor.MiddleLeft,
				contentOffset = new Vector2(2.5f, 0f),
				normal = 
				{
					textColor = ((!EditorGUIUtility.isProSkin) ? PushList(91f) : Color.gray)
				}
			};
		}

		internal static bool CalculateCandidate()
		{
			return ChangeCandidate == null;
		}
	}

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

	internal class ProcessorObserver
	{
		private struct ErrorObserver
		{
			internal PositionFlag _SetterObserver;

			internal Rect connectionObserver;

			internal int contextObserver;
		}

		private int m_ObserverObserver;

		private Vector2 _ServerObserver = Vector2.zero;

		private readonly int m_ThreadObserver = GUIUtility.GetControlID("ResizeStateControlID".GetHashCode(), FocusType.Passive);

		public Action _PolicyObserver;

		public float _SerializerObserver;

		public float pageObserver;

		public float resolverObserver;

		public float predicateObserver;

		private bool _RulesObserver;

		private bool m_QueueObserver;

		internal static ProcessorObserver RateCandidate;

		[SpecialName]
		public bool ResolveError()
		{
			return _RulesObserver;
		}

		[SpecialName]
		public void ListError(bool isinit)
		{
			if (_RulesObserver == isinit)
			{
				return;
			}
			_RulesObserver = isinit;
			if (!isinit)
			{
				return;
			}
			if (_SerializerObserver == 0f)
			{
				if (pageObserver == 0f)
				{
					if (resolverObserver == 0f)
					{
						if (predicateObserver != 0f)
						{
							resolverObserver = predicateObserver;
						}
					}
					else
					{
						predicateObserver = resolverObserver;
					}
				}
				else
				{
					_SerializerObserver = pageObserver;
				}
			}
			else
			{
				pageObserver = _SerializerObserver;
			}
		}

		public ProcessorObserver(bool usekey = false)
		{
			_RulesObserver = usekey;
		}

		public void CreateError()
		{
			_SerializerObserver = 0f;
			pageObserver = 0f;
			resolverObserver = 0f;
			predicateObserver = 0f;
			_PolicyObserver?.Invoke();
		}

		public Rect NewError(Rect config, PositionFlag pred = PositionFlag.Middle, Rect field = default(Rect))
		{
			if (field == default(Rect))
			{
				field = new Rect(-1f, -1f, -1f, -1f);
			}
			bool flag = field.x != -1f && field.width != -1f;
			bool flag2 = field.y != -1f && field.height != -1f;
			float num = 10f;
			float num2 = config.width + _SerializerObserver + pageObserver;
			float num3 = config.height + resolverObserver + predicateObserver;
			float num4 = config.x - (num2 - config.width) * ViewError(pred);
			float num5 = config.y - (config.height + resolverObserver + predicateObserver - config.height) * CollectError(pred);
			config.x = ((!flag) ? num4 : Mathf.Clamp(num4, field.x, field.x + field.width - num));
			config.width = ((!flag) ? num2 : Mathf.Clamp(num2, num, field.width - config.x));
			config.y = ((!flag2) ? num5 : Mathf.Clamp(num5, field.y, field.y + field.height - num));
			config.height = (flag2 ? Mathf.Clamp(num3, num, field.height - config.y) : num3);
			return config;
		}

		public void PushError(Rect info, PositionFlag pol = PositionFlag.Right | PositionFlag.Left, PositionFlag comp = PositionFlag.Middle, float param2 = 4f)
		{
			Event current = Event.current;
			if (m_QueueObserver && current.type == EventType.MouseUp)
			{
				if (GUIUtility.hotControl == m_ThreadObserver)
				{
					GUIUtility.hotControl = 0;
				}
				CreateError();
				current.Use();
				m_QueueObserver = false;
			}
			float num = param2 * 2f;
			ErrorObserver[] array = new ErrorObserver[8]
			{
				new ErrorObserver
				{
					_SetterObserver = PositionFlag.Left,
					contextObserver = 0,
					connectionObserver = new Rect(info.x - param2, info.y + param2, num, info.height - num)
				},
				new ErrorObserver
				{
					_SetterObserver = PositionFlag.TopLeft,
					contextObserver = 1,
					connectionObserver = new Rect(info.x - param2, info.y - param2, num, num)
				},
				new ErrorObserver
				{
					_SetterObserver = PositionFlag.Top,
					contextObserver = 2,
					connectionObserver = new Rect(info.x + param2, info.y - param2, info.width - num, num)
				},
				new ErrorObserver
				{
					_SetterObserver = PositionFlag.TopRight,
					contextObserver = 3,
					connectionObserver = new Rect(info.x + info.width - param2, info.y - param2, num, num)
				},
				new ErrorObserver
				{
					_SetterObserver = PositionFlag.Right,
					contextObserver = 4,
					connectionObserver = new Rect(info.x + info.width - param2, info.y + param2, num, info.height - num)
				},
				new ErrorObserver
				{
					_SetterObserver = PositionFlag.BottomRight,
					contextObserver = 5,
					connectionObserver = new Rect(info.x + info.width - param2, info.y + info.height - param2, num, num)
				},
				new ErrorObserver
				{
					_SetterObserver = PositionFlag.Bottom,
					contextObserver = 6,
					connectionObserver = new Rect(info.x + param2, info.y + info.height - param2, info.width - num, num)
				},
				new ErrorObserver
				{
					_SetterObserver = PositionFlag.BottomLeft,
					contextObserver = 7,
					connectionObserver = new Rect(info.x - param2, info.y + info.height - param2, num, num)
				}
			};
			bool flag = current.button == 0;
			ErrorObserver[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				ErrorObserver errorObserver = array2[i];
				if ((errorObserver._SetterObserver & pol) < errorObserver._SetterObserver)
				{
					continue;
				}
				MouseCursor pred;
				switch (errorObserver._SetterObserver)
				{
				default:
					pred = MouseCursor.Arrow;
					break;
				case PositionFlag.Right:
				case PositionFlag.Left:
					pred = MouseCursor.ResizeHorizontal;
					break;
				case PositionFlag.TopLeft:
				case PositionFlag.BottomRight:
					pred = MouseCursor.ResizeUpLeft;
					break;
				case PositionFlag.Top:
				case PositionFlag.Bottom:
					pred = MouseCursor.ResizeVertical;
					break;
				case PositionFlag.TopRight:
				case PositionFlag.BottomLeft:
					pred = MouseCursor.ResizeUpRight;
					break;
				}
				AwakeQueue(errorObserver.connectionObserver, pred);
				Rect connectionObserver = errorObserver.connectionObserver;
				if (m_ProxyProperty)
				{
					connectionObserver.y += 46f;
				}
				if (flag && current.type == EventType.MouseDown && connectionObserver.Contains(current.mousePosition))
				{
					if (current.clickCount == 2)
					{
						m_QueueObserver = true;
					}
					m_ObserverObserver = errorObserver.contextObserver;
					GUIUtility.hotControl = m_ThreadObserver;
					_ServerObserver = GUIUtility.GUIToScreenPoint(current.mousePosition);
					current.Use();
				}
			}
			if (current.type != EventType.MouseDrag || GUIUtility.hotControl != m_ThreadObserver)
			{
				return;
			}
			PositionFlag setterObserver = array[m_ObserverObserver]._SetterObserver;
			Vector2 vector = GUIUtility.GUIToScreenPoint(current.mousePosition) - _ServerObserver;
			if (m_QueueObserver)
			{
				if (!(vector.sqrMagnitude > new Vector2(15f, 15f).sqrMagnitude))
				{
					return;
				}
				m_QueueObserver = false;
			}
			if (vector != Vector2.zero)
			{
				switch (setterObserver)
				{
				case PositionFlag.TopRight:
					pageObserver += vector.x;
					if (!ResolveError())
					{
						resolverObserver -= vector.y;
					}
					else if (!comp.HasFlag(PositionFlag.Left))
					{
						_SerializerObserver -= vector.y;
					}
					else
					{
						pageObserver -= vector.y;
					}
					break;
				case PositionFlag.BottomLeft:
					_SerializerObserver -= vector.x;
					if (!ResolveError())
					{
						predicateObserver += vector.y;
					}
					else if (comp.HasFlag(PositionFlag.Bottom))
					{
						resolverObserver += vector.x;
					}
					else
					{
						predicateObserver += vector.x;
					}
					break;
				case PositionFlag.Top:
					resolverObserver -= vector.y;
					if (ResolveError())
					{
						if (comp.HasFlag(PositionFlag.Left))
						{
							pageObserver -= vector.y;
						}
						else
						{
							_SerializerObserver -= vector.y;
						}
					}
					break;
				case PositionFlag.Bottom:
					predicateObserver += vector.y;
					if (ResolveError())
					{
						if (comp.HasFlag(PositionFlag.Left))
						{
							pageObserver += vector.y;
						}
						else
						{
							_SerializerObserver += vector.y;
						}
					}
					break;
				case PositionFlag.TopLeft:
					_SerializerObserver -= vector.x;
					if (!ResolveError())
					{
						resolverObserver -= vector.y;
					}
					else if (!comp.HasFlag(PositionFlag.Bottom))
					{
						predicateObserver -= vector.x;
					}
					else
					{
						resolverObserver -= vector.x;
					}
					break;
				case PositionFlag.Left:
					_SerializerObserver -= vector.x;
					if (ResolveError())
					{
						if (comp.HasFlag(PositionFlag.Bottom))
						{
							resolverObserver -= vector.x;
						}
						else
						{
							predicateObserver -= vector.x;
						}
					}
					break;
				case PositionFlag.BottomRight:
					pageObserver += vector.x;
					if (ResolveError())
					{
						if (comp.HasFlag(PositionFlag.Top))
						{
							predicateObserver += vector.x;
						}
						else
						{
							resolverObserver += vector.x;
						}
					}
					else
					{
						predicateObserver += vector.y;
					}
					break;
				case PositionFlag.Right:
					pageObserver += vector.x;
					if (ResolveError())
					{
						if (comp.HasFlag(PositionFlag.Bottom))
						{
							resolverObserver += vector.x;
						}
						else
						{
							predicateObserver += vector.x;
						}
					}
					break;
				}
				_PolicyObserver?.Invoke();
			}
			_ServerObserver = GUIUtility.GUIToScreenPoint(current.mousePosition);
		}

		public static float ViewError(PositionFlag res, bool evaluateivk = false)
		{
			if (!evaluateivk)
			{
				if (res.PrintPage())
				{
					return 1f;
				}
				if (res.SearchPage())
				{
					return 0f;
				}
			}
			else
			{
				if (res.PrintPage())
				{
					return 0f;
				}
				if (res.SearchPage())
				{
					return 1f;
				}
			}
			return 0.5f;
		}

		public static float CollectError(PositionFlag i, bool forceattr = false)
		{
			bool flag = i.RevertPage();
			bool flag2 = i.OrderResolver();
			if (!forceattr)
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

		internal static bool PostCandidate()
		{
			return RateCandidate == null;
		}
	}

	internal class RecordObserver : IDisposable
	{
		public readonly bool helperObserver;

		public readonly bool _ConsumerObserver = true;

		private readonly Rect m_AdapterObserver;

		internal static RecordObserver DisableCandidate;

		public RecordObserver(SceneView res, string col, float pool = 200f, float item2 = 20f, PositionFlag v3 = PositionFlag.BottomRight, ProcessorObserver v4 = null)
			: this(res, pool, item2 + 40f, v3, v4)
		{
			while (true)
			{
				GUILayout.Label(col, CalcError()._StructProcessor);
			}
		}

		public RecordObserver(SceneView key, float map, float serv = 20f, PositionFlag config2 = PositionFlag.BottomRight, ProcessorObserver ident3 = null)
		{
			Handles.BeginGUI();
			Rect rect = key.SortQueue();
			Rect field = new Rect(rect)
			{
				x = rect.x + 4f,
				y = rect.y + 4f,
				width = rect.width - 8f,
				height = rect.height - 8f
			};
			Rect rect2 = FillError(rect, map, serv, config2, helperObserver);
			if (ident3 != null)
			{
				rect2 = ident3.NewError(rect2, config2, field);
				ident3.PushError(rect2, config2.CompareResolver(loadreg: true));
			}
			m_AdapterObserver = SetResolver(rect2);
			if (m_ProxyProperty)
			{
				m_AdapterObserver.y += 46f;
			}
			GUILayout.BeginArea(m_AdapterObserver);
		}

		public void Dispose()
		{
			if (_ConsumerObserver)
			{
				Event current = Event.current;
				if (current.type == EventType.MouseDown && !m_AdapterObserver.Contains(current.mousePosition))
				{
					current.Use();
					GUIUtility.hotControl = 0;
				}
			}
			GUILayout.EndArea();
			Handles.EndGUI();
		}

		private static Rect FillError(Rect info, float token, float pool = 20f, PositionFlag ivk2 = PositionFlag.Bottom, bool testreg3 = false)
		{
			Rect result = info;
			info.x += 4f;
			info.width -= 8f;
			float num = (testreg3 ? (token * info.width / 100f) : token);
			bool flag = ivk2.PrintPage();
			bool num2 = ivk2.SearchPage();
			bool flag2 = ivk2.RevertPage();
			bool flag3 = ivk2.OrderResolver();
			float x = (num2 ? info.x : (flag ? (info.x + info.width - num) : (info.x + info.width / 2f - num / 2f)));
			float y = (flag2 ? info.y : ((!flag3) ? (info.y + info.height / 2f - pool / 2f) : (info.y + info.height - pool)));
			result.x = x;
			result.y = y;
			result.width = num;
			result.height = pool;
			return result;
		}

		internal static bool VerifyCandidate()
		{
			return DisableCandidate == null;
		}
	}

	internal enum WriteDefaultSetSettings
	{
		Off,
		On,
		Automatic
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

	internal struct InterpreterObserver
	{
		internal string watcherObserver;

		internal GUIStyle _CandidateObserver;

		internal Vector3 _ProductObserver;

		internal Quaternion m_ExpressionObserver;

		internal Vector3 systemObserver;

		internal float _WorkerObserver;

		internal float[] m_FilterObserver;

		internal int _StubObserver;

		internal Action m_ReaderObserver;

		internal Func<InterpreterObserver, float[]> bridgeObserver;

		internal Action<InterpreterObserver> m_StrategyObserver;

		private static object TestCandidate;

		internal static InterpreterObserver WriteError(Vector3 spec, string token = "", float helper = 0.05f, int version_visitor2 = -1, Action map3 = null)
		{
			return new InterpreterObserver
			{
				m_StrategyObserver = CheckError,
				_CandidateObserver = new GUIStyle(EditorStyles.boldLabel),
				bridgeObserver = (InterpreterObserver sc) => new float[1] { HandleUtility.DistanceToCircle(sc._ProductObserver, sc._WorkerObserver / 2f) },
				_ProductObserver = spec,
				_WorkerObserver = helper,
				watcherObserver = token,
				_StubObserver = version_visitor2,
				m_ReaderObserver = map3
			};
		}

		internal void ForgotError()
		{
			m_StrategyObserver(this);
		}

		internal float[] StopError()
		{
			return bridgeObserver(this);
		}

		internal static void CheckError(InterpreterObserver config)
		{
			Handles.SphereHandleCap(config._StubObserver, config._ProductObserver, Quaternion.identity, config._WorkerObserver, EventType.Repaint);
			if (!string.IsNullOrWhiteSpace(config.watcherObserver))
			{
				CreateQueue(config.watcherObserver, config._ProductObserver, config._WorkerObserver, config._CandidateObserver);
			}
		}

		internal static bool IncludeCandidate()
		{
			return TestCandidate == null;
		}
	}

	[DefaultMember("Item")]
	internal readonly struct ExporterObserver
	{
		private readonly string m_IdentifierObserver;

		private readonly Dictionary<string, RegistryObserver> m_AttrObserver;

		internal readonly bool _DispatcherObserver;

		internal static object MapCandidate;

		internal ExporterObserver(string ident)
		{
			m_IdentifierObserver = ident;
			MatchCollection matchCollection = Regex.Matches(ident, "\"(.*?)\":(?:(?:\"(.*?)\")|(?:(.*?)[,}]))");
			int count = matchCollection.Count;
			if (count != 0)
			{
				_DispatcherObserver = false;
				m_AttrObserver = new Dictionary<string, RegistryObserver>();
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
						m_AttrObserver[value] = new RegistryObserver(value2);
					}
				}
			}
			else
			{
				_DispatcherObserver = true;
				m_AttrObserver = null;
			}
		}

		[SpecialName]
		internal RegistryObserver UpdateError(string def)
		{
			m_AttrObserver.TryGetValue(def, out var value);
			return value;
		}

		public override string ToString()
		{
			return m_IdentifierObserver;
		}

		public string AssetError(bool isident)
		{
			if (!isident)
			{
				return ToString();
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("{");
			foreach (KeyValuePair<string, RegistryObserver> item in m_AttrObserver)
			{
				stringBuilder.AppendLine($"{item.Key}: {item.Value},");
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}

		internal static bool AddCandidate()
		{
			return MapCandidate == null;
		}
	}

	internal readonly struct RegistryObserver
	{
		internal readonly string _TagObserver;

		internal readonly string importerObserver;

		internal readonly bool _RequestObserver;

		internal readonly float _PrinterObserver;

		internal readonly bool m_WriterObserver;

		internal static object NewCandidate;

		internal RegistryObserver(string ident)
		{
			_TagObserver = ident;
			m_WriterObserver = true;
			if (ident.Length > 1)
			{
				if (!ident.StartsWith("\"") || !ident.EndsWith("\""))
				{
					importerObserver = ident;
				}
				else
				{
					importerObserver = ((ident.Length != 2) ? ident.Substring(1, ident.Length - 2) : string.Empty);
				}
			}
			else
			{
				importerObserver = ident;
			}
			_RequestObserver = importerObserver.ToLower() == "true";
			float.TryParse(importerObserver, out _PrinterObserver);
		}

		public override string ToString()
		{
			return importerObserver;
		}

		public static implicit operator string(RegistryObserver param)
		{
			return param.importerObserver;
		}

		public static implicit operator bool(RegistryObserver reference)
		{
			return reference._RequestObserver;
		}

		public static implicit operator float(RegistryObserver setup)
		{
			return setup._PrinterObserver;
		}

		internal static bool LoginCandidate()
		{
			return NewCandidate == null;
		}
	}

	internal enum PathOption
	{
		Normal,
		ForceFolder,
		ForceFile
	}

	internal enum PlaneAxis
	{
		None,
		YZ,
		XZ,
		XY
	}

	internal struct ParamsObserver
	{
		internal VRCAvatarDescriptor m_ListenerObserver;

		internal VRCAvatarDescriptor.AnimLayerType _GetterObserver;

		internal bool interceptorObserver;

		internal UnityEditor.Animations.AnimatorController m_CreatorObserver;

		internal SystemThread eventObserver;

		internal bool infoObserver;

		internal static object AssetCandidate;

		[SpecialName]
		internal bool PrintError()
		{
			return eventObserver._WorkerThread;
		}

		internal ParamsObserver SortError(VRCAvatarDescriptor value, VRCAvatarDescriptor.AnimLayerType token, bool dorole = false)
		{
			return RegisterError(value, token, (!(value != null)) ? null : value.ChangeList(token), dorole);
		}

		internal ParamsObserver RegisterError(VRCAvatarDescriptor spec, VRCAvatarDescriptor.AnimLayerType connection, UnityEditor.Animations.AnimatorController pool, bool isdef2 = false)
		{
			eventObserver = new SystemThread(isparam: false, "Unknown Error");
			m_ListenerObserver = spec;
			_GetterObserver = connection;
			interceptorObserver = true;
			infoObserver = isdef2;
			return LogoutError(pool, isdef2);
		}

		internal ParamsObserver LogoutError(UnityEditor.Animations.AnimatorController task, bool istoken = false)
		{
			eventObserver = new SystemThread(isparam: false, "Unknown Error");
			infoObserver = istoken;
			m_CreatorObserver = task;
			return this;
		}

		internal ParamsObserver Process()
		{
			if (!interceptorObserver || !(m_ListenerObserver == null))
			{
				if (!(m_CreatorObserver == null) || infoObserver)
				{
					eventObserver = new SystemThread(isparam: true, "Check is valid");
					return this;
				}
				eventObserver = ((!interceptorObserver) ? new SystemThread(isparam: false, "Controller is not set (Null)") : new SystemThread(isparam: false, $"{_GetterObserver} Controller was not found"));
				return this;
			}
			eventObserver = new SystemThread(isparam: false, "Avatar is not set (Null)");
			return this;
		}

		internal void PatchError(Action<UnityEditor.Animations.AnimatorController> last, string ord = null)
		{
			VRCAvatarDescriptor listenerObserver = m_ListenerObserver;
			bool flag = listenerObserver == null;
			VRCAvatarDescriptor.AnimLayerType m_AdvisorObserver = _GetterObserver;
			string text = m_AdvisorObserver.ToString();
			UnityEditor.Animations.AnimatorController animatorController = (flag ? null : listenerObserver.ChangeList(m_AdvisorObserver));
			_ = animatorController != null;
			bool flag2 = interceptorObserver && !flag && animatorController == m_CreatorObserver;
			if (ord == null)
			{
				ord = ((!interceptorObserver) ? "Target Controller" : ("Target " + text + ":"));
			}
			bool cfg;
			string b = (m_CreatorObserver.CallRules(out cfg) ? ((!cfg) ? "No Controller Selected" : ((!flag2) ? "Controller Is Missing!" : ("[Avatar's " + text + " Is Missing!]"))) : ((!flag2) ? m_CreatorObserver.name : ("[Avatar's " + text + "]")));
			ParamsObserver _FacadeObserver = this;
			PopRules(ord, b, m_CreatorObserver, delegate(UnityEditor.Animations.AnimatorController c)
			{
				_FacadeObserver.InterruptError(c, m_AdvisorObserver, last);
			}, eventObserver, null, ManageError, infoObserver);
		}

		private void InterruptError(UnityEditor.Animations.AnimatorController spec, VRCAvatarDescriptor.AnimLayerType cfg, Action<UnityEditor.Animations.AnimatorController> template)
		{
			template(spec);
			if (interceptorObserver && m_ListenerObserver != null && m_ListenerObserver.ChangeList(cfg) == null)
			{
				VRCAvatarDescriptor prototypeObserver = m_ListenerObserver;
				GenericMenu genericMenu = new GenericMenu();
				genericMenu.AddItem(new GUIContent($"Set As Avatar's {cfg}?/Yes"), on: false, delegate
				{
					prototypeObserver.SortList(cfg, spec);
				});
				genericMenu.ShowAsContext();
			}
		}

		private void ManageError(UnityEditor.Animations.AnimatorController def)
		{
			def.AddLayer("Base Layer");
			EditorUtility.SetDirty(def);
		}

		internal static bool SelectCandidate()
		{
			return AssetCandidate == null;
		}
	}

	internal struct RuleObserver
	{
		internal VRCAvatarDescriptor _SingletonObserver;

		internal bool _FactoryObserver;

		internal VRCExpressionParameters accountObserver;

		internal int m_RefObserver;

		internal int statusObserver;

		internal int _TokenObserver;

		internal bool m_CodeObserver;

		internal bool _DicObserver;

		internal SystemThread m_InvocationObserver;

		internal List<(string, int)> roleObserver;

		internal string _ParamObserver;

		internal static object CompareCandidate;

		[SpecialName]
		internal bool CountSetter()
		{
			return m_InvocationObserver._WorkerThread;
		}

		internal void CompareSetter(VRCAvatarDescriptor ident, VRCExpressionParameters pred, bool comparedic = false)
		{
			_SingletonObserver = ident;
			_FactoryObserver = true;
			SetSetter(pred);
		}

		internal void SetSetter(VRCExpressionParameters task, bool iscust = false)
		{
			accountObserver = task;
			PostSetter();
		}

		internal void PostSetter(bool isasset = false)
		{
			roleObserver = new List<(string, int)>();
			_ParamObserver = string.Empty;
			m_InvocationObserver = new SystemThread(isparam: false, "Unknown Error", -1);
			_DicObserver = isasset;
		}

		internal void SetupSetter(string def, IEnumerable<VRCExpressionParameters> selection, bool excludeproc = true)
		{
			if (excludeproc)
			{
				PopSetter(def, selection.Sum((VRCExpressionParameters p) => p.CalcTotalCost()));
			}
		}

		internal void EnableSetter(string def, VRCExpressionParameters col, bool hasfilter = true)
		{
			if (hasfilter)
			{
				PopSetter(def, col.CalcTotalCost());
			}
		}

		internal void PublishSetter(string res, IEnumerable<VRCExpressionParameters.Parameter> map, bool appendfield = true)
		{
			if (appendfield)
			{
				PopSetter(res, map.Sum((VRCExpressionParameters.Parameter p) => VRCExpressionParameters.TypeCost(p.valueType)));
			}
		}

		internal void PopSetter(string res, int length_counter, bool isfield = true)
		{
			if (isfield)
			{
				roleObserver.Add((res, length_counter));
			}
		}

		internal void Process(bool willCreateIfNull = false)
		{
			if (!_FactoryObserver || !(_SingletonObserver == null))
			{
				if (!(accountObserver == null))
				{
					m_RefObserver = accountObserver.InterruptList(includesecond: true, willCreateIfNull);
					statusObserver = roleObserver.Sum(((string, int) c) => c.Item2);
					_TokenObserver = m_RefObserver - statusObserver;
					m_CodeObserver = _TokenObserver >= 0;
					m_InvocationObserver = ((!m_CodeObserver) ? new SystemThread(isparam: false, $"Adding {statusObserver} bits of parameters would exceed the maximum parameters memory of ${RunError()}", 2) : new SystemThread(isparam: true, "Success"));
					_ParamObserver = $"Remaining: {_TokenObserver}\n" + string.Join("\n", roleObserver.Select(((string, int) c) => c.Item1));
				}
				else
				{
					m_CodeObserver = false;
					m_InvocationObserver = new SystemThread(isparam: false, "Expression Parameters is not set (Null)", 1);
				}
			}
			else
			{
				m_CodeObserver = false;
				m_InvocationObserver = new SystemThread(isparam: false, "Avatar is not set (Null)");
			}
		}

		internal void ComputeSetter(Action<VRCExpressionParameters> spec, string connection = "Target Parameters:")
		{
			bool flag = _FactoryObserver && _SingletonObserver != null && _SingletonObserver.expressionParameters == accountObserver;
			bool cfg;
			string b = (accountObserver.CallRules(out cfg) ? ((!cfg) ? "No Parameters Selected" : ((!flag) ? "Parameters Are Missing!" : "[Avatar's Parameters Are Missing!]")) : ((!(_FactoryObserver && flag)) ? accountObserver.name : "[Avatar's Parameters]"));
			RuleObserver objectObserver = this;
			PopRules(connection, b, accountObserver, delegate(VRCExpressionParameters p)
			{
				objectObserver.CallSetter(p, spec);
			}, m_InvocationObserver, ConcatSetter, CancelSetter, _DicObserver);
		}

		internal void MoveSetter()
		{
			if (!CountSetter())
			{
				GUILayout.Label(new GUIContent(DestroyError().issuerProcessor)
				{
					tooltip = m_InvocationObserver.m_FilterThread
				}, CalcError().broadcasterProcessor);
			}
		}

		private void ConcatSetter()
		{
			using (new TemplateThread(TemplateThread.ColoringType.FG, m_CodeObserver, configurationProperty, _WrapperProcessor))
			{
				GUILayout.Label(new GUIContent($"{statusObserver}/{m_RefObserver}", _ParamObserver), CalcError().algoObserver, GUILayout.ExpandWidth(expand: false));
			}
		}

		private void CallSetter(VRCExpressionParameters last, Action<VRCExpressionParameters> connection)
		{
			connection(last);
			if (_FactoryObserver && _SingletonObserver != null && _SingletonObserver.expressionParameters == null)
			{
				VRCAvatarDescriptor valueObserver = _SingletonObserver;
				GenericMenu genericMenu = new GenericMenu();
				genericMenu.AddItem(new GUIContent("Set As Avatar's Parameters?/Yes"), on: false, delegate
				{
					valueObserver.CallError(last);
				});
				genericMenu.ShowAsContext();
			}
		}

		private void CancelSetter(VRCExpressionParameters reference)
		{
			reference.parameters = Array.Empty<VRCExpressionParameters.Parameter>();
			EditorUtility.SetDirty(reference);
		}

		internal static bool PublishCandidate()
		{
			return CompareCandidate == null;
		}
	}

	internal struct MerchantObserver
	{
		internal VRCAvatarDescriptor authenticationObserver;

		internal bool reponseObserver;

		internal VRCExpressionsMenu m_PoolObserver;

		internal VRCExpressionsMenu m_ParameterObserver;

		internal bool _ComposerObserver;

		internal List<VRCExpressionsMenu.Control> _RepositoryObserver;

		internal bool m_MappingObserver;

		internal int _BaseObserver;

		internal int m_ContainerObserver;

		internal int _ClassObserver;

		internal bool _MockObserver;

		internal SystemThread _InstanceObserver;

		private Action<VRCExpressionsMenu> _FieldObserver;

		private static object CancelField;

		[SpecialName]
		internal bool ResetSetter()
		{
			return _InstanceObserver._WorkerThread;
		}

		internal MerchantObserver ExcludeSetter(VRCAvatarDescriptor def, VRCExpressionsMenu token, VRCExpressionsMenu serv = null)
		{
			authenticationObserver = def;
			reponseObserver = true;
			return DefineSetter(token, serv);
		}

		internal MerchantObserver InitSetter(VRCAvatarDescriptor last, List<VRCExpressionsMenu.Control> cfg, VRCExpressionsMenu temp = null)
		{
			authenticationObserver = last;
			reponseObserver = true;
			_RepositoryObserver = cfg;
			m_MappingObserver = true;
			if (cfg != null)
			{
				return ReadSetter(cfg.Count, temp);
			}
			m_PoolObserver = temp;
			return this;
		}

		internal MerchantObserver VisitSetter(VRCAvatarDescriptor task, int bend, VRCExpressionsMenu template = null)
		{
			authenticationObserver = task;
			reponseObserver = true;
			return ReadSetter(bend, template);
		}

		internal MerchantObserver DefineSetter(VRCExpressionsMenu last, VRCExpressionsMenu ivk = null)
		{
			m_ParameterObserver = last;
			_ComposerObserver = true;
			if (last.controls == null)
			{
				last.controls = new List<VRCExpressionsMenu.Control>();
				EditorUtility.SetDirty(last);
			}
			if (last != null)
			{
				return StartSetter(last.controls, ivk);
			}
			m_PoolObserver = ivk;
			return this;
		}

		internal MerchantObserver StartSetter(List<VRCExpressionsMenu.Control> first, VRCExpressionsMenu cust = null)
		{
			_RepositoryObserver = first;
			m_MappingObserver = true;
			if (first == null)
			{
				m_PoolObserver = cust;
				return this;
			}
			return ReadSetter(first.Count, cust);
		}

		internal MerchantObserver ReadSetter(int position_ident, VRCExpressionsMenu second)
		{
			_BaseObserver = position_ident;
			m_PoolObserver = second;
			return this;
		}

		internal MerchantObserver Process()
		{
			if (reponseObserver && authenticationObserver == null)
			{
				_MockObserver = false;
				_InstanceObserver = (false, "Avatar is not set (Null)");
				return this;
			}
			if (_ComposerObserver && m_ParameterObserver == null)
			{
				_MockObserver = false;
				_InstanceObserver = new SystemThread(isparam: false, "Source Menu is not set (Null)")
				{
					_StubThread = 1
				};
				return this;
			}
			if (m_MappingObserver && _RepositoryObserver == null)
			{
				_MockObserver = false;
				_InstanceObserver = new SystemThread(isparam: false, "Source Controls are null")
				{
					_StubThread = 2
				};
				return this;
			}
			if (!(m_PoolObserver == null))
			{
				m_ContainerObserver = 8 - m_PoolObserver.controls.Count;
				_ClassObserver = m_ContainerObserver - _BaseObserver;
				_MockObserver = _ClassObserver >= 0;
				_InstanceObserver = ((!_MockObserver) ? new SystemThread(isparam: false, $"Adding {_BaseObserver} controls to {m_PoolObserver.name} would exceed the 8 controls limit")
				{
					_StubThread = 4
				} : new SystemThread(isparam: true, "Adding Controls Validated"));
				return this;
			}
			_MockObserver = false;
			_InstanceObserver = new SystemThread(isparam: false, "Target Menu is not set (Null)")
			{
				_StubThread = 3
			};
			return this;
		}

		internal void SelectSetter(Action<VRCExpressionsMenu> value, bool nocol = false, string filter = "Target Menu:")
		{
			_FieldObserver = value;
			bool flag = reponseObserver && authenticationObserver != null && authenticationObserver.expressionsMenu == m_PoolObserver;
			bool cfg;
			string b = (m_PoolObserver.CallRules(out cfg) ? ((!cfg) ? "No Menu Selected" : ((!flag) ? "Menu Is Missing!" : "[Avatar's Menu Is Missing!]")) : ((!flag) ? m_PoolObserver.name : "[Avatar's Main Menu]"));
			PopRules(filter, b, m_PoolObserver, InstantiateSetter, _InstanceObserver, RemoveSetter, AwakeSetter, nocol);
		}

		private void RemoveSetter()
		{
			using (new TemplateThread(TemplateThread.ColoringType.FG, _MockObserver, configurationProperty, _WrapperProcessor))
			{
				GUILayout.Label(new GUIContent($"{_BaseObserver}/{m_ContainerObserver}", $"Remaining: {_ClassObserver}"), CalcError().algoObserver, GUILayout.ExpandWidth(expand: false));
			}
			using (new EditorGUI.DisabledScope(m_PoolObserver == null))
			{
				if (RestartQueue(new GUIContent(DestroyError().m_ComposerProcessor)
				{
					tooltip = "Select Menu From TreeView"
				}, CalcError().broadcasterProcessor))
				{
					MenuSelector.InvokeRecord(m_PoolObserver, InstantiateSetter, _BaseObserver);
				}
			}
		}

		private void InstantiateSetter(VRCExpressionsMenu info)
		{
			_FieldObserver(info);
			if (reponseObserver && authenticationObserver != null && authenticationObserver.expressionsMenu == null)
			{
				VRCAvatarDescriptor m_ClientObserver = authenticationObserver;
				GenericMenu genericMenu = new GenericMenu();
				genericMenu.AddItem(new GUIContent("Set As Avatar's Main Menu?/Yes"), on: false, delegate
				{
					m_ClientObserver.ReadError(info);
				});
				genericMenu.ShowAsContext();
			}
		}

		private void AwakeSetter(VRCExpressionsMenu init)
		{
			if (init.controls == null)
			{
				init.controls = new List<VRCExpressionsMenu.Control>();
				EditorUtility.SetDirty(init);
			}
		}

		internal static bool RestartField()
		{
			return CancelField == null;
		}
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec m_ConfigObserver = new _003C_003Ec();

		public static Func<Keyframe, (float, float)> m_DescriptorObserver;

		public static Func<AnimatorStateTransition, string> templateObserver;

		public static Func<string, bool> messageObserver;

		public static Func<string, bool> collectionObserver;

		public static Func<int, bool> _ParserObserver;

		public static Func<ChildAnimatorState, AnimatorState> managerObserver;

		public static Func<ChildAnimatorStateMachine, AnimatorStateMachine> _ItemObserver;

		public static Func<object, bool> _SpecificationObserver;

		public static Func<Vector3, Vector3, Vector3> methodObserver;

		public static Func<ParameterInfo, Type> m_SchemaObserver;

		public static Func<Type, string> _BroadcasterObserver;

		public static Func<Type, string> m_ProxyObserver;

		public static Func<VRCAvatarDescriptor.CustomAnimLayer, RuntimeAnimatorController> m_StructObserver;

		public static Func<VRCExpressionParameters, bool> _ServiceObserver;

		public static Func<VRCExpressionParameters, IEnumerable<VRCExpressionParameters.Parameter>> _StateObserver;

		public static Func<VRCExpressionParameters.Parameter, bool> globalObserver;

		internal static _003C_003Ec RunField;

		internal (float, float) CalculateSetter(Keyframe k)
		{
			return (k.time, k.value);
		}

		internal string TestSetter(AnimatorStateTransition t)
		{
			return t.name;
		}

		internal bool MapSetter(string s)
		{
			return s[0] == '_';
		}

		internal bool ValidateSetter(string s)
		{
			return s[0] != '_';
		}

		internal bool CustomizeSetter(int c)
		{
			return c == 0;
		}

		internal AnimatorState RateSetter(ChildAnimatorState cs)
		{
			return cs.state;
		}

		internal AnimatorStateMachine DestroySetter(ChildAnimatorStateMachine cm)
		{
			return cm.stateMachine;
		}

		internal bool GetSetter(object e)
		{
			return e == null;
		}

		internal Vector3 CalcSetter(Vector3 current, Vector3 vec)
		{
			return current + vec;
		}

		internal Type IncludeSetter(ParameterInfo p)
		{
			return p.ParameterType;
		}

		internal string RunSetter(Type ht)
		{
			return ht.Name;
		}

		internal string CloneSetter(Type ht)
		{
			return ht.Name;
		}

		internal RuntimeAnimatorController LoginSetter(VRCAvatarDescriptor.CustomAnimLayer l)
		{
			return l.animatorController;
		}

		internal bool ReflectSetter(VRCExpressionParameters p)
		{
			return p != null;
		}

		internal IEnumerable<VRCExpressionParameters.Parameter> DeleteSetter(VRCExpressionParameters p)
		{
			return p.parameters;
		}

		internal bool CreateSetter(VRCExpressionParameters.Parameter p)
		{
			return p != null;
		}

		internal static bool ComputeField()
		{
			return RunField == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass128_0
	{
		public Dictionary<UnityEngine.Object, UnityEngine.Object> iteratorObserver;

		public UnityEditor.Animations.AnimatorController m_PublisherObserver;

		public bool configurationObserver;

		public Dictionary<UnityEngine.Object, UnityEngine.Object> _ProcObserver;

		public UnityEditor.Animations.AnimatorControllerLayer m_WrapperServer;

		public UnityEditor.Animations.AnimatorControllerLayer annotationServer;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass164_0
	{
		public Dictionary<UnityEngine.Object, RepositoryServer> m_SerializerServer;

		private static _003C_003Ec__DisplayClass164_0 ListField;

		internal void InterruptSetter(AnimatorStateMachine m)
		{
			_003C_003Ec__DisplayClass164_1 _003C_003Ec__DisplayClass164_ = new _003C_003Ec__DisplayClass164_1
			{
				_ResolverServer = this,
				_PageServer = m
			};
			if (_003C_003Ec__DisplayClass164_._PageServer == null)
			{
				return;
			}
			_003C_003Ec__DisplayClass164_._PageServer.ResolvePredicate(_003C_003Ec__DisplayClass164_.ManageSetter, moveserv: false);
			foreach (AnimatorState item in _003C_003Ec__DisplayClass164_._PageServer.states.Select(_003C_003Ec.m_ConfigObserver.RateSetter))
			{
				if (!m_SerializerServer.ContainsKey(item))
				{
					m_SerializerServer.Add(item, new RepositoryServer(item));
				}
			}
			foreach (AnimatorStateMachine item2 in _003C_003Ec__DisplayClass164_._PageServer.stateMachines.Select(_003C_003Ec.m_ConfigObserver.DestroySetter))
			{
				if (!m_SerializerServer.ContainsKey(item2))
				{
					m_SerializerServer.Add(item2, new RepositoryServer(_003C_003Ec__DisplayClass164_._PageServer, item2));
				}
			}
		}

		internal static bool CalcField()
		{
			return ListField == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass164_1
	{
		public AnimatorStateMachine _PageServer;

		public _003C_003Ec__DisplayClass164_0 _ResolverServer;

		internal static _003C_003Ec__DisplayClass164_1 CompareField;

		internal void ManageSetter(InstanceServer et)
		{
			if (!et.fieldServer)
			{
				return;
			}
			UnityEngine.Object obj = et.RegisterConnection();
			bool flag;
			if (!(flag = obj))
			{
				obj = et.PrintConnection();
			}
			if ((bool)obj)
			{
				bool num = _ResolverServer.m_SerializerServer.ContainsKey(obj);
				RepositoryServer value = (num ? _ResolverServer.m_SerializerServer[obj] : ((!flag) ? new RepositoryServer(_PageServer, et.PrintConnection()) : new RepositoryServer(et.RegisterConnection())));
				if (!num)
				{
					_ResolverServer.m_SerializerServer.Add(obj, value);
				}
				value.mockServer.Add(et.fieldServer);
			}
		}

		internal static bool PublishField()
		{
			return CompareField == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass210_1<T> where T : UnityEngine.Object
	{
		public T e;

		internal static object GetAttribute;

		internal bool MoveConnection(SerializedProperty e2, int _)
		{
			return e2.objectReferenceValue == e;
		}

		internal static bool VisitAttribute()
		{
			return GetAttribute == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass212_1<T> where T : UnityEngine.Object
	{
		public T e;

		internal static object RateAttribute;

		internal bool CallConnection(SerializedProperty e2, int i)
		{
			return e2.objectReferenceValue == e;
		}

		internal static bool PostAttribute()
		{
			return RateAttribute == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass402_0
	{
		public ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> _StubServer;

		internal static _003C_003Ec__DisplayClass402_0 PushAttribute;

		internal void InvokeConnection(ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> bag)
		{
			_003C_003Ec__DisplayClass402_1 _003C_003Ec__DisplayClass402_ = new _003C_003Ec__DisplayClass402_1
			{
				_BridgeServer = this,
				m_ReaderServer = bag
			};
			Parallel.ForEach(_003C_003Ec__DisplayClass402_.m_ReaderServer.Keys, _003C_003Ec__DisplayClass402_.FindConnection);
		}

		internal static bool PrepareAttribute()
		{
			return PushAttribute == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass402_1
	{
		public ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> m_ReaderServer;

		public _003C_003Ec__DisplayClass402_0 _BridgeServer;

		private static _003C_003Ec__DisplayClass402_1 PrintAttribute;

		internal void FindConnection(Transform t)
		{
			if (_BridgeServer._StubServer.ContainsKey(t))
			{
				_003C_003Ec__DisplayClass402_2 _003C_003Ec__DisplayClass402_ = new _003C_003Ec__DisplayClass402_2
				{
					strategyServer = _BridgeServer._StubServer[t]
				};
				Parallel.ForEach(m_ReaderServer[t], _003C_003Ec__DisplayClass402_.ExcludeConnection);
			}
			else
			{
				_BridgeServer._StubServer.TryAdd(t, m_ReaderServer[t]);
			}
		}

		internal static bool ResolveAttribute()
		{
			return PrintAttribute == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass402_2
	{
		public ConcurrentBag<Vector3> strategyServer;

		internal static _003C_003Ec__DisplayClass402_2 AssetAttribute;

		internal void ExcludeConnection(Vector3 v)
		{
			strategyServer.Add(v);
		}

		internal static bool SelectAttribute()
		{
			return AssetAttribute == null;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003C_003Ec__DisplayClass446_0
	{
		public VRCAvatarDescriptor.AnimLayerType _PrinterServer;

		public RuntimeAnimatorController _WriterServer;

		public VRCAvatarDescriptor _ParamsServer;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass465_1
	{
		public string _FacadeServer;

		private static _003C_003Ec__DisplayClass465_1 ConnectSystem;

		internal bool FlushConnection(VRCExpressionParameters.Parameter p2)
		{
			if (p2 != null)
			{
				return p2.name == _FacadeServer;
			}
			return false;
		}

		internal static bool ViewSystem()
		{
			return ConnectSystem == null;
		}
	}

	private static MethodInfo m_MockProperty;

	private static MethodInfo m_InstanceProperty;

	private static MethodInfo fieldProperty;

	private static MethodInfo _AttributeProperty;

	private static MethodInfo _ClientProperty;

	private static MethodInfo _ConfigProperty;

	private static MethodInfo m_DescriptorProperty;

	private static MethodInfo _TemplateProperty;

	private static MethodInfo _MessageProperty;

	internal static readonly GUIContent m_CollectionProperty = new GUIContent();

	private static MethodInfo m_ParserProperty;

	private static bool managerProperty;

	internal static string itemProperty = "Assets";

	internal static readonly Dictionary<Type, string> m_SpecificationProperty = new Dictionary<Type, string>
	{
		{
			typeof(UnityEditor.Animations.AnimatorController),
			"controller"
		},
		{
			typeof(AnimationClip),
			"anim"
		},
		{
			typeof(UnityEditor.Animations.BlendTree),
			"blendTree"
		},
		{
			typeof(Shader),
			"shader"
		},
		{
			typeof(Material),
			"mat"
		},
		{
			typeof(GameObject),
			"prefab"
		}
	};

	private static readonly Queue<Action> _MethodProperty = new Queue<Action>();

	private static readonly Queue<Action> _SchemaProperty = new Queue<Action>();

	internal static readonly string m_BroadcasterProperty = Application.unityVersion;

	internal static readonly bool m_ProxyProperty = m_BroadcasterProperty.Contains("2022");

	private static Dictionary<string, Type> structProperty;

	private static Dictionary<string, object> serviceProperty;

	private static readonly Stack<(Rect, MouseCursor)> stateProperty = new Stack<(Rect, MouseCursor)>();

	private static bool globalProperty;

	private static MethodInfo m_TaskProperty;

	private static Quaternion processProperty;

	private static bool m_ProducerProperty;

	private static readonly int iteratorProperty = "RadiusHandleHash".GetHashCode();

	internal static Color publisherProperty = new Color(0.5f, 0.8f, 1f);

	internal static Color configurationProperty = new Color(0.56f, 0.94f, 0.47f);

	internal static Color _ProcProperty = new Color(1f, 0.25f, 0.25f);

	internal static Color _WrapperProcessor = new Color(0.99f, 0.95f, 0f);

	internal static Color m_AnnotationProcessor = new Color(0.7f, 0.3f, 1f);

	internal static Color visitorProcessor = new Color(1f, 0.65f, 0f);

	internal static Color m_AlgoProcessor = new Color(1f, 0.5f, 0.7f);

	internal static Color m_MapperProcessor = new Color(0.3f, 0.7f, 1f);

	internal static WatcherProcessor _InitializerProcessor;

	internal static BaseProcessor _DefinitionProcessor;

	private static Mesh regProcessor;

	private static Material testsProcessor;

	private static readonly int _PropertyProcessor = Shader.PropertyToID("_Color");

	private static MethodInfo processorProcessor;

	private static Type observerProcessor;

	private static bool m_ServerProcessor;

	private static bool threadProcessor;

	private static Type m_PolicyProcessor;

	private static Type m_SerializerProcessor;

	private static FieldInfo _PageProcessor;

	private static FieldInfo resolverProcessor;

	internal static Type _PredicateProcessor;

	internal static MethodInfo _RulesProcessor;

	private static FieldInfo queueProcessor;

	private static bool errorProcessor;

	internal static readonly WatcherPolicy setterProcessor = new WatcherPolicy("https://raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png", checkselection: true, "DreadBanner.png");

	private static Texture2D connectionProcessor;

	private static Dictionary<GameObject, VRCAvatarDescriptor> m_ContextProcessor;

	private static int recordProcessor;

	private static FieldInfo helperProcessor;

	private static bool m_ConsumerProcessor;

	internal static readonly string[] m_AdapterProcessor = new string[28]
	{
		"IsLocal", "Viseme", "Voice", "GestureLeft", "GestureRight", "GestureLeftWeight", "GestureRightWeight", "AngularY", "VelocityX", "VelocityY",
		"VelocityZ", "VelocityMagnitude", "Upright", "Grounded", "Seated", "AFK", "TrackingType", "VRMode", "MuteSelf", "InStation",
		"Earmuffs", "IsOnFriendsList", "AvatarVersion", "ScaleModified", "ScaleFactor", "ScaleFactorInverse", "EyeHeightAsMeters", "EyeHeightAsPercent"
	};

	internal static readonly string[] interpreterProcessor = new string[23]
	{
		"Head", "Torso", "Hand", "Foot", "Finger", "FingerIndex", "FingerMiddle", "FingerRing", "FingerLittle", "HandL",
		"FootL", "FingerL", "FingerIndexL", "FingerMiddleL", "FingerRingL", "FingerLittleL", "HandR", "FootR", "FingerR", "FingerIndexR",
		"FingerMiddleR", "FingerRingR", "FingerLittleR"
	};

	private static ClassProperty RunCandidate;

	internal static bool PrintPage(this PositionFlag ident)
	{
		if (ident.HasFlag(PositionFlag.Right) || ident.HasFlag(PositionFlag.TopRight))
		{
			return true;
		}
		return ident.HasFlag(PositionFlag.BottomRight);
	}

	internal static bool SearchPage(this PositionFlag item)
	{
		if (item.HasFlag(PositionFlag.Left) || item.HasFlag(PositionFlag.TopLeft))
		{
			return true;
		}
		return item.HasFlag(PositionFlag.BottomLeft);
	}

	internal static bool RevertPage(this PositionFlag var1)
	{
		if (var1.HasFlag(PositionFlag.Top) || var1.HasFlag(PositionFlag.TopLeft))
		{
			return true;
		}
		return var1.HasFlag(PositionFlag.TopRight);
	}

	internal static bool OrderResolver(this PositionFlag init)
	{
		if (init.HasFlag(PositionFlag.Bottom) || init.HasFlag(PositionFlag.BottomLeft))
		{
			return true;
		}
		return init.HasFlag(PositionFlag.BottomRight);
	}

	public static PositionFlag CompareResolver(this PositionFlag info, bool loadreg = false, bool isserv = false)
	{
		PositionFlag positionFlag;
		if (info > PositionFlag.Bottom)
		{
			if (info > PositionFlag.TopLeft)
			{
				if (info != PositionFlag.BottomRight)
				{
					if (info != PositionFlag.BottomLeft)
					{
						goto IL_001e;
					}
					positionFlag = PositionFlag.Right | PositionFlag.Top;
				}
				else
				{
					positionFlag = PositionFlag.Left | PositionFlag.Top;
				}
			}
			else if (info == PositionFlag.TopRight)
			{
				positionFlag = PositionFlag.Left | PositionFlag.Bottom;
			}
			else
			{
				if (info != PositionFlag.TopLeft)
				{
					goto IL_001e;
				}
				positionFlag = PositionFlag.Right | PositionFlag.Bottom;
			}
		}
		else if (info > PositionFlag.Left)
		{
			if (info != PositionFlag.Top)
			{
				if (info != PositionFlag.Bottom)
				{
					goto IL_001e;
				}
				positionFlag = PositionFlag.Right | PositionFlag.Left | PositionFlag.Top;
			}
			else
			{
				positionFlag = PositionFlag.Right | PositionFlag.Left | PositionFlag.Bottom;
			}
		}
		else if (info == PositionFlag.Right)
		{
			positionFlag = PositionFlag.Left | PositionFlag.Top | PositionFlag.Bottom;
		}
		else
		{
			if (info != PositionFlag.Left)
			{
				goto IL_001e;
			}
			positionFlag = PositionFlag.Right | PositionFlag.Top | PositionFlag.Bottom;
		}
		goto IL_0020;
		IL_0020:
		if (loadreg)
		{
			positionFlag &= ~(PositionFlag.Top | PositionFlag.Bottom);
		}
		if (isserv)
		{
			positionFlag &= ~(PositionFlag.Right | PositionFlag.Left);
		}
		return positionFlag;
		IL_001e:
		positionFlag = PositionFlag.Middle;
		goto IL_0020;
	}

	internal static Rect SetResolver(Rect v, float col = 2f)
	{
		return PostResolver(v, new Color(0.03f, 0.03f, 0.03f, 0.5f), new Color(0.137f, 0.137f, 0.137f, 0.5f), col);
	}

	internal static Rect PostResolver(Rect key, Color map, Color filter, float x2 = 3f)
	{
		float num = x2 + 2f;
		Rect position = key;
		position.x -= num / 2f;
		position.width += num;
		position.y -= num / 2f;
		position.height += num;
		if (map != Color.clear)
		{
			GUI.DrawTexture(key, LoginList(map), ScaleMode.StretchToFill, alphaBlend: true, 0f, map, 0f, 8f);
		}
		if (filter != Color.clear)
		{
			GUI.DrawTexture(position, LoginList(filter), ScaleMode.StretchToFill, alphaBlend: true, 0f, filter, x2, 8f);
		}
		Rect result = key;
		result.x += 4f;
		result.width -= 8f;
		result.y += 4f;
		result.height -= 8f;
		return result;
	}

	internal static Color SetupResolver(this Color asset, float second)
	{
		return new Color(asset.r, asset.g, asset.b, second);
	}

	internal static Color EnableResolver(this Color init, Color visitor)
	{
		float num = visitor.a + init.a * (1f - visitor.a);
		float r = (visitor.r * visitor.a + init.r * init.a * (1f - visitor.a)) / num;
		float g = (visitor.g * visitor.a + init.g * init.a * (1f - visitor.a)) / num;
		float b = (visitor.b * visitor.a + init.b * init.a * (1f - visitor.a)) / num;
		return new Color(r, g, b, num);
	}

	internal static void PublishResolver(this IConstraint i, params float[] weights)
	{
		List<ConstraintSource> list = new List<ConstraintSource>();
		i.GetSources(list);
		for (int j = 0; j < weights.Length && j < list.Count; j++)
		{
			list[j] = new ConstraintSource
			{
				sourceTransform = list[j].sourceTransform,
				weight = weights[j]
			};
		}
		i.SetSources(list);
	}

	internal static void PopResolver(this ParentConstraint instance)
	{
		if (m_MockProperty == null)
		{
			m_MockProperty = typeof(ParentConstraint).GetMethod("ActivateAndPreserveOffset", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		m_MockProperty.Invoke(instance, null);
	}

	internal static void ComputeResolver(this ParentConstraint v)
	{
		if (m_InstanceProperty == null)
		{
			m_InstanceProperty = typeof(ParentConstraint).GetMethod("ActivateWithZeroOffset", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		m_InstanceProperty.Invoke(v, null);
	}

	internal static void MoveResolver(this RotationConstraint param)
	{
		if (fieldProperty == null)
		{
			fieldProperty = typeof(RotationConstraint).GetMethod("ActivateAndPreserveOffset", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		fieldProperty.Invoke(param, null);
	}

	internal static void ConcatResolver(this RotationConstraint task)
	{
		if (_AttributeProperty == null)
		{
			_AttributeProperty = typeof(RotationConstraint).GetMethod("ActivateWithZeroOffset", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		_AttributeProperty.Invoke(task, null);
	}

	internal static void CallResolver(this PositionConstraint reference)
	{
		if (_ClientProperty == null)
		{
			_ClientProperty = typeof(PositionConstraint).GetMethod("ActivateAndPreserveOffset", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		_ClientProperty.Invoke(reference, null);
	}

	internal static void CancelResolver(this PositionConstraint config)
	{
		if (_ConfigProperty == null)
		{
			_ConfigProperty = typeof(PositionConstraint).GetMethod("ActivateWithZeroOffset", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		_ConfigProperty.Invoke(config, null);
	}

	internal static void CountResolver(this AimConstraint ident)
	{
		if (m_DescriptorProperty == null)
		{
			m_DescriptorProperty = typeof(AimConstraint).GetMethod("ActivateAndPreserveOffset", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		m_DescriptorProperty.Invoke(ident, null);
	}

	internal static void DisableResolver(this AimConstraint def)
	{
		if (_TemplateProperty == null)
		{
			_TemplateProperty = typeof(AimConstraint).GetMethod("ActivateWithZeroOffset", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		_TemplateProperty.Invoke(def, null);
	}

	internal static void InsertResolver(this ScaleConstraint info)
	{
		if (_MessageProperty == null)
		{
			_MessageProperty = typeof(ScaleConstraint).GetMethod("ActivateAndPreserveOffset", BindingFlags.Instance | BindingFlags.NonPublic);
		}
		_MessageProperty.Invoke(info, null);
	}

	private static void RestartResolver(this IConstraint instance)
	{
		if (instance is ParentConstraint instance2)
		{
			instance2.PopResolver();
		}
		else if (!(instance is RotationConstraint param))
		{
			if (!(instance is PositionConstraint reference))
			{
				if (instance is AimConstraint ident)
				{
					ident.CountResolver();
				}
			}
			else
			{
				reference.CallResolver();
			}
		}
		else
		{
			param.MoveResolver();
		}
	}

	private static void QueryResolver(this IConstraint var1)
	{
		if (var1 is ParentConstraint v)
		{
			v.ComputeResolver();
		}
		else if (!(var1 is RotationConstraint task))
		{
			if (!(var1 is PositionConstraint config))
			{
				if (var1 is AimConstraint def)
				{
					def.DisableResolver();
				}
			}
			else
			{
				config.CancelResolver();
			}
		}
		else
		{
			task.ConcatResolver();
		}
	}

	internal static void InvokeResolver<T>(this IEnumerable<T> first, Action<T> visitor)
	{
		foreach (T item in first)
		{
			visitor(item);
		}
	}

	internal static int FindResolver<T>(this IEnumerable<T> var1, Func<T, bool> cont)
	{
		int num = -1;
		using (IEnumerator<T> enumerator = var1.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				num = checked(num + 1);
				if (enumerator.Current != null && cont(enumerator.Current))
				{
					return num;
				}
			}
		}
		return -1;
	}

	internal static bool ExcludeResolver<T>(this IEnumerable<T> param, Func<T, bool> caller, out int c)
	{
		c = param.FindResolver(caller);
		return c != -1;
	}

	internal static void InitResolver(this UnityEngine.Object first, bool removetoken)
	{
		if (!(first == null))
		{
			if (!removetoken)
			{
				first.hideFlags &= ~(HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild);
			}
			else
			{
				first.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			}
		}
	}

	internal static void VisitResolver(this UnityEngine.Object reference, bool isvisitor)
	{
		if (!(reference == null))
		{
			if (isvisitor)
			{
				reference.hideFlags |= HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}
			else
			{
				reference.hideFlags &= ~(HideFlags.HideInHierarchy | HideFlags.HideInInspector);
			}
		}
	}

	internal static void DefineResolver(this GameObject setup, bool getcfg)
	{
		Transform[] componentsInChildren = setup.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.InitResolver(getcfg);
		}
	}

	internal static void StartResolver(this Transform v)
	{
		v.localPosition = Vector3.zero;
		v.localRotation = Quaternion.identity;
		v.localScale = Vector3.one;
	}

	internal static Transform[] ReadResolver(this Transform def)
	{
		Transform[] array = new Transform[def.childCount];
		for (int i = 0; i < def.childCount; i++)
		{
			array[i] = def.GetChild(i);
		}
		return array;
	}

	internal static void SelectResolver(this Transform item, Vector3 counter)
	{
		item.localScale = Vector3.one;
		while (true)
		{
			_ = item.worldToLocalMatrix;
		}
	}

	internal static bool RemoveResolver<T>(this Type config)
	{
		if (config.IsSubclassOf(typeof(T)))
		{
			return true;
		}
		return config == typeof(T);
	}

	internal static bool InstantiateResolver(this Type reference, Type attr)
	{
		if (reference.IsSubclassOf(attr))
		{
			return true;
		}
		return reference == attr;
	}

	internal static bool AwakeResolver(this ref bool task)
	{
		return task = !task;
	}

	internal static bool ResetResolver(this string param)
	{
		return string.IsNullOrEmpty(param);
	}

	internal static bool FlushResolver(this string item)
	{
		return string.IsNullOrWhiteSpace(item);
	}

	internal static string ConnectResolver(this string instance)
	{
		return instance ?? "";
	}

	internal static string CalculateResolver(this string def, string ord)
	{
		if (!string.IsNullOrEmpty(def))
		{
			return def;
		}
		return ord;
	}

	internal static bool MapResolver(this float i, float cust, float dic)
	{
		if (i < cust)
		{
			return false;
		}
		return i <= dic;
	}

	internal static bool ValidateResolver(this int ident, int indexvisitor, int thirdoffset)
	{
		if (ident < indexvisitor)
		{
			return false;
		}
		return ident <= thirdoffset;
	}

	internal static bool CustomizeResolver(this int end_param, IList pred)
	{
		if (end_param >= 0)
		{
			return end_param < pred.Count;
		}
		return false;
	}

	internal static bool RateResolver(this int lastsize, Array ivk)
	{
		if (lastsize >= 0)
		{
			return lastsize < ivk.Length;
		}
		return false;
	}

	internal static bool DestroyResolver(this float init, float pol, float third)
	{
		if (!(init >= pol))
		{
			return true;
		}
		return init > third;
	}

	internal static bool GetResolver(this int offsetident, int countermin, int start_control)
	{
		if (offsetident < countermin)
		{
			return true;
		}
		return offsetident > start_control;
	}

	internal static int CalcResolver(this float spec, int counter_end)
	{
		return Mathf.RoundToInt(spec).IncludeResolver(counter_end);
	}

	internal static int IncludeResolver(this int minfirst, int ivkhigh)
	{
		return Mathf.RoundToInt((float)minfirst / (float)ivkhigh) * ivkhigh;
	}

	internal static string RunResolver(this string spec, Color reg)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(reg) + ">" + spec + "</color>";
	}

	internal static string CloneResolver(this string task, LogType caller)
	{
		switch (caller)
		{
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			return task.RunResolver(_ProcProperty);
		case LogType.Warning:
			return task.RunResolver(_WrapperProcessor);
		default:
			return task.RunResolver(configurationProperty);
		}
	}

	internal static void LoginResolver(this string param, LogType selection = LogType.Log)
	{
		switch (selection)
		{
		case LogType.Exception:
			Debug.LogException(new Exception(param));
			break;
		default:
			Debug.Log(param);
			break;
		case LogType.Assert:
			break;
		case LogType.Warning:
			Debug.LogWarning(param);
			break;
		case LogType.Error:
			Debug.LogError(param);
			break;
		}
	}

	internal static void ReflectResolver(this string item, LogType connection = LogType.Log)
	{
		item.CloneResolver(connection).LoginResolver(connection);
	}

	internal static GUIContent DeleteResolver(this string param, bool tokenneeded)
	{
		return param.CreateResolver("", tokenneeded);
	}

	internal static GUIContent CreateResolver(this string var1, string cfg = "", bool wanthelper = false)
	{
		m_CollectionProperty.text = var1;
		m_CollectionProperty.tooltip = cfg;
		if (!wanthelper)
		{
			return m_CollectionProperty;
		}
		return new GUIContent(m_CollectionProperty);
	}

	internal static string NewResolver(this string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return key;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(char.ToUpper(key[0]));
		for (int i = 1; i < key.Length; i++)
		{
			if (char.IsUpper(key[i]) && !char.IsUpper(key[i - 1]))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(key[i]);
		}
		return stringBuilder.ToString();
	}

	internal static float PushResolver(this string param, GUIStyle visitor = null)
	{
		if (visitor == null)
		{
			visitor = GUI.skin.label;
		}
		return visitor.CalcSize(param.CreateResolver()).x;
	}

	internal static Rect ViewResolver(this Rect var1)
	{
		Rect result = new Rect(var1);
		result.x = var1.x + var1.width;
		return result;
	}

	public static Rect CollectResolver(this Rect init, float result)
	{
		return init.VerifyResolver(0f - result);
	}

	public static Rect ResolveResolver(this Rect asset, float second)
	{
		return asset.FillResolver(0f - second);
	}

	public static Rect ListResolver(this Rect reference, float attr)
	{
		return reference.WriteResolver(0f - attr);
	}

	public static Rect VerifyResolver(this Rect def, float visitor)
	{
		def.x -= visitor;
		def.y -= visitor;
		def.width += visitor * 2f;
		def.height += visitor * 2f;
		return def;
	}

	public static Rect FillResolver(this Rect param, float pred)
	{
		param.x -= pred;
		param.width += pred * 2f;
		return param;
	}

	public static Rect WriteResolver(this Rect instance, float ivk)
	{
		instance.y -= ivk;
		instance.height += ivk * 2f;
		return instance;
	}

	public static Rect ForgotResolver(this Rect info, float pol)
	{
		info.x += pol;
		return info;
	}

	public static Rect StopResolver(this Rect init, float pred)
	{
		init.y += pred;
		return init;
	}

	public static Rect CheckResolver(this Rect task, float token)
	{
		task.width -= token;
		task.x += token;
		return task;
	}

	public static Rect PrepareResolver(this Rect setup, float vis)
	{
		setup.width += vis;
		return setup;
	}

	public static Rect AssetResolver(this Rect v, float pol)
	{
		v.height += pol;
		v.y -= pol;
		return v;
	}

	public static Rect UpdateResolver(this Rect last, float b)
	{
		last.height += b;
		return last;
	}

	public static Rect ChangeResolver(this ref Rect key, bool isconnection, float template, bool issecond2 = false, float config3 = -1f, bool allowx4 = false, bool comparev5 = true)
	{
		if (isconnection)
		{
			return key.SortResolver(template, issecond2, config3, allowx4, comparev5);
		}
		return key;
	}

	public static Rect SortResolver(this ref Rect def, float visitor, bool isfield = false, float vis2 = -1f, bool iscont3 = false, bool isattr4 = true)
	{
		Rect result = def;
		result.width = ((!isfield) ? (visitor * def.width / 100f) : visitor);
		result.height = def.height;
		float num = ((vis2 == -1f) ? 0f : ((!iscont3) ? (vis2 * def.width / 100f) : vis2));
		result.x = def.x + num;
		result.y = def.y;
		if (isattr4)
		{
			def.x = result.x + result.width;
			def.width -= result.width;
		}
		return result;
	}

	public static Rect RegisterResolver(this ref Rect res, float b, bool comparetemp = false, float result2 = -1f, bool isord3 = false, bool compareconfig4 = true)
	{
		Rect result3 = res;
		result3.height = ((!comparetemp) ? (b * res.height / 100f) : b);
		float num = ((result2 == -1f) ? 0f : (isord3 ? result2 : (result2 * res.height / 100f)));
		result3.y = res.y + num;
		if (compareconfig4)
		{
			res.y = result3.y + result3.height;
			res.height -= result3.height;
		}
		return result3;
	}

	public static Rect LogoutResolver(this ref Rect def, bool updatecounter, float util, bool compareinit2 = false, float attr3 = -1f, bool acceptvalue4 = false, bool rejectpred5 = true)
	{
		if (!updatecounter)
		{
			return def;
		}
		return def.PatchResolver(util, compareinit2, attr3, acceptvalue4, rejectpred5);
	}

	public static Rect PatchResolver(this ref Rect res, float map, bool isserv = false, float v2 = -1f, bool isvisitor3 = false, bool istoken4 = true)
	{
		Rect result = res;
		result.width = (isserv ? map : (map * res.width / 100f));
		float num = ((v2 == -1f) ? 0f : (isvisitor3 ? v2 : (v2 * res.width / 100f)));
		result.x = res.x + res.width - result.width - num;
		if (istoken4)
		{
			res.width -= result.width + num;
		}
		return result;
	}

	public static Rect InterruptResolver(this Rect asset, float connection)
	{
		asset.width = connection;
		return asset;
	}

	public static Rect ManageResolver(this Rect param, float result)
	{
		param.height = result;
		return param;
	}

	public static Rect PrintResolver(this Rect setup, float pol)
	{
		Rect result = setup;
		if (setup.width / setup.height <= pol)
		{
			result.height = setup.width / pol;
			result.y += (setup.height - result.height) / 2f;
		}
		else
		{
			result.width = setup.height * pol;
			result.x += (setup.width - result.width) / 2f;
		}
		return result;
	}

	internal static void SearchResolver(this AnimBool key, Action visitor, Action role = null)
	{
		if (key.faded != 0f)
		{
			EditorGUILayout.BeginFadeGroup(key.faded);
			visitor();
			if (role != null && 0f < key.faded && !(key.faded >= 1f))
			{
				role();
			}
			EditorGUILayout.EndFadeGroup();
		}
	}

	internal static T RevertResolver<T>(this UnityEngine.Object last) where T : UnityEngine.Object
	{
		if (typeof(T).IsSubclassOf(typeof(Component)))
		{
			GameObject obj = last as GameObject;
			if ((object)obj != null)
			{
				return obj.GetComponent<T>();
			}
			return null;
		}
		return last as T;
	}

	internal static void OrderPredicate<T>(this T reference, string col, bool moverole = false) where T : EditorWindow
	{
		string value = JsonUtility.ToJson(reference, prettyPrint: false);
		if (!moverole)
		{
			EditorPrefs.SetString(col, value);
		}
		else
		{
			PlayerPrefs.SetString(col, value);
		}
	}

	internal static void ComparePredicate<T>(this T task, string cfg, bool isthird = false) where T : EditorWindow
	{
		string defaultValue = JsonUtility.ToJson(task, prettyPrint: false);
		JsonUtility.FromJsonOverwrite((!isthird) ? EditorPrefs.GetString(cfg, defaultValue) : PlayerPrefs.GetString(cfg, defaultValue), task);
	}

	internal static Vector3 SetPredicate(this Vector3 item, int selectionLow)
	{
		return new Vector3(item.x.CalcResolver(selectionLow), item.y.CalcResolver(selectionLow), item.z.CalcResolver(selectionLow));
	}

	internal static Vector2 PostPredicate(this Vector2 task, int b_Z)
	{
		return new Vector2(task.x.CalcResolver(b_Z), task.y.CalcResolver(b_Z));
	}

	internal static float SetupPredicate(this Vector3 i)
	{
		return Mathf.Max(i.x, i.y, i.z);
	}

	internal static float EnablePredicate(this Vector3 last)
	{
		return Mathf.Min(last.x, last.y, last.z);
	}

	internal static float PublishPredicate(this Vector3 info)
	{
		return (info.x + info.y + info.z) / 3f;
	}

	internal static Vector3 PopPredicate(this Vector3 item, Axis cont = Axis.X)
	{
		float x = item.x * (float)(((cont & Axis.X) == 0) ? 1 : (-1));
		float y = item.y * (float)(((cont & Axis.Y) == 0) ? 1 : (-1));
		float z = item.z * (float)(((cont & Axis.Z) == 0) ? 1 : (-1));
		return new Vector3(x, y, z);
	}

	internal static Vector3 ComputePredicate(this Vector3 ident, Axis selection)
	{
		float x = ident.x * (float)(((selection & Axis.X) != Axis.None) ? 1 : 0);
		float y = ident.y * (float)(((selection & Axis.Y) != Axis.None) ? 1 : 0);
		float z = ident.z * (float)(((selection & Axis.Z) != Axis.None) ? 1 : 0);
		return new Vector3(x, y, z);
	}

	internal static Vector3 MovePredicate(this Vector3 item)
	{
		return new Vector3(item.x + 180f, item.y + 180f, item.z + 180f);
	}

	internal static Vector3 ConcatPredicate(this Vector3 v, Axis vis)
	{
		float x = v.x + (float)(((vis & Axis.X) != Axis.None) ? 180 : 0);
		float y = v.y + (float)(((vis & Axis.Y) != Axis.None) ? 180 : 0);
		float z = v.z + (float)(((vis & Axis.Z) != Axis.None) ? 180 : 0);
		return new Vector3(x, y, z);
	}

	internal static bool CallPredicate(this ReorderableList key)
	{
		return key.index.CustomizeResolver(key.list);
	}

	internal static IEnumerable<T> CancelPredicate<T>(this T asset) where T : Enum
	{
		return Enum.GetValues(typeof(T)).Cast<T>().Where(delegate(T value)
		{
			ref T reference = ref asset;
			object flag = value;
			return reference.HasFlag((Enum)flag);
		});
	}

	internal static T CountPredicate<T>(this T ident, GUIContent map, bool haveutil = true, params GUILayoutOption[] options) where T : UnityEngine.Object
	{
		return (T)EditorGUILayout.ObjectField(map, ident, typeof(T), haveutil, options);
	}

	internal static bool DisablePredicate(this AnimationCurve spec, float b, out Keyframe proc, out Keyframe result2)
	{
		proc = default(Keyframe);
		result2 = default(Keyframe);
		if (spec.length == 0)
		{
			return false;
		}
		if (spec.length != 1)
		{
			int num = 0;
			Keyframe keyframe;
			while (true)
			{
				if (num < spec.length)
				{
					keyframe = spec[num];
					if (keyframe.time == b)
					{
						break;
					}
					if (keyframe.time >= b)
					{
						result2 = keyframe;
						return true;
					}
					proc = keyframe;
					num++;
					continue;
				}
				return false;
			}
			proc = (result2 = keyframe);
			return true;
		}
		proc = spec[0];
		return false;
	}

	internal static bool InsertPredicate(this AnimationCurve spec, float reg, out float role)
	{
		role = 0f;
		if (!spec.DisablePredicate(reg, out var proc, out var result))
		{
			return false;
		}
		if (proc.time == result.time)
		{
			role = proc.outTangent;
			return true;
		}
		role = QueryPredicate(proc, result, reg);
		return true;
	}

	internal static float RestartPredicate(float reference, float col, float role, float result2, float result3)
	{
		float num = 2f * col;
		float num2 = role - reference;
		float num3 = 2f * reference - 5f * col + 4f * role - result2;
		float num4 = 0f - reference + 3f * col - 3f * role + result2;
		return 0.5f * (num + num2 * result3 + num3 * result3 * result3 + num4 * result3 * result3 * result3);
	}

	internal static float QueryPredicate(Keyframe info, Keyframe pred, float control)
	{
		float num = pred.time - info.time;
		float num2 = 57.29578f * Mathf.Atan(info.outTangent);
		float num3 = 57.29578f * Mathf.Atan(pred.inTangent);
		float value = info.value;
		float value2 = pred.value;
		float reference = info.value + Mathf.Tan(num2 + 180f) * num;
		float result = pred.value + Mathf.Tan(num3 + 180f) * num;
		float num4 = RestartPredicate(reference, value, value2, result, control);
		return (RestartPredicate(reference, value, value2, result, control + 1E-05f) - num4) / 1E-05f;
	}

	private static AnimationCurve AddPredicate(AnimationUtility.TangentMode var1 = AnimationUtility.TangentMode.Free, params Keyframe[] keyFrames)
	{
		return InvokePredicate(var1, keyFrames.Select((Keyframe k) => (k.time, k.value)).ToArray());
	}

	internal static AnimationCurve InvokePredicate(AnimationUtility.TangentMode setup = AnimationUtility.TangentMode.Free, params (float, float)[] timeValuePair)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		for (int i = 0; i < timeValuePair.Length; i++)
		{
			var (time, value) = timeValuePair[i];
			animationCurve.AddKey(time, value);
			if (i > 0)
			{
				AnimationUtility.SetKeyRightTangentMode(animationCurve, i - 1, setup);
			}
			AnimationUtility.SetKeyLeftTangentMode(animationCurve, i, setup);
		}
		return animationCurve;
	}

	internal static float FindPredicate(this AnimationClip v)
	{
		if (!AnimationUtility.GetCurveBindings(v).Any())
		{
			uint num4 = default(uint);
			while (true)
			{
				int num;
				int num2;
				if (!AnimationUtility.GetObjectReferenceCurveBindings(v).Any())
				{
					num = -1318284075;
					num2 = -1318284075;
				}
				else
				{
					num = -836449655;
					num2 = -836449655;
				}
				int num3 = num ^ (int)(num4 * 1738322238);
				while (true)
				{
					switch ((num4 = (uint)(num3 ^ -1080307183)) % 6)
					{
					case 1u:
					case 3u:
						break;
					case 5u:
						return 1f / 60f;
					case 0u:
					{
						int num5;
						int num6;
						if (!(v.frameRate > 0f))
						{
							num5 = -699664398;
							num6 = -699664398;
						}
						else
						{
							num5 = -1440947483;
							num6 = -1440947483;
						}
						num3 = num5 ^ (int)(num4 * 283448667);
						continue;
					}
					default:
						goto end_IL_000d;
					case 2u:
						return 1f / v.frameRate;
					}
					break;
				}
				continue;
				end_IL_000d:
				break;
			}
		}
		return v.length;
	}

	internal static bool ExcludePredicate(this AnimationClip task, EditorCurveBinding cfg, out EditorCurveBinding filter)
	{
		return (cfg.isDiscreteCurve ? AnimationUtility.GetCurveBindings(task) : AnimationUtility.GetObjectReferenceCurveBindings(task)).InitPredicate(cfg, out filter);
	}

	internal static bool InitPredicate(this IEnumerable<EditorCurveBinding> i, EditorCurveBinding attr, out EditorCurveBinding tag)
	{
		foreach (EditorCurveBinding item in i)
		{
			if (!(item.propertyName == attr.propertyName) || !(item.type == attr.type) || !(item.path == attr.path))
			{
				continue;
			}
			tag = item;
			return true;
		}
		tag = default(EditorCurveBinding);
		return false;
	}

	internal static UnityEditor.Animations.AnimatorControllerLayer VisitPredicate(this UnityEditor.Animations.AnimatorController task, string selection, float field, AvatarMask asset2 = null)
	{
		UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = new UnityEditor.Animations.AnimatorControllerLayer
		{
			name = selection,
			defaultWeight = field,
			avatarMask = asset2,
			stateMachine = new AnimatorStateMachine
			{
				name = selection,
				hideFlags = HideFlags.HideInHierarchy
			}
		};
		AssetDatabase.AddObjectToAsset(animatorControllerLayer.stateMachine, task);
		task.AddLayer(animatorControllerLayer);
		return animatorControllerLayer;
	}

	private static bool DefinePredicate(AnimatorStateTransition item)
	{
		if (item.isExit)
		{
			uint num4 = default(uint);
			while (true)
			{
				IL_000a:
				int num;
				int num2;
				if (!item.mute)
				{
					num = 924604632;
					num2 = 924604632;
				}
				else
				{
					num = 1653767409;
					num2 = 1653767409;
				}
				int num3 = num ^ (int)(num4 * 1361654938);
				while (true)
				{
					switch ((num4 = (uint)(num3 ^ 0x42B7561B)) % 5)
					{
					case 0u:
					case 2u:
						goto IL_000a;
					case 1u:
					{
						int num5;
						int num6;
						if (item.destinationState == null)
						{
							num5 = 2005329365;
							num6 = 2005329365;
						}
						else
						{
							num5 = 600466514;
							num6 = 600466514;
						}
						num3 = num5 ^ ((int)num4 * -1189089618);
						continue;
					}
					case 3u:
						return item.destinationStateMachine == null;
					}
					break;
				}
				break;
			}
		}
		return false;
	}

	internal static void StartPredicate(this UnityEditor.Animations.AnimatorControllerLayer info, string caller)
	{
		if (!info.SelectPredicate(caller))
		{
			AnimatorStateTransition animatorStateTransition = info.stateMachine.AddAnyStateTransition((AnimatorState)null);
			animatorStateTransition.isExit = true;
			animatorStateTransition.mute = true;
			animatorStateTransition.name = caller;
		}
	}

	internal static void ReadPredicate(this UnityEditor.Animations.AnimatorControllerLayer i, string visitor)
	{
		AnimatorStateTransition transition = i.stateMachine.anyStateTransitions.First((AnimatorStateTransition tr) => DefinePredicate(tr) && tr.name == visitor);
		i.stateMachine.RemoveAnyStateTransition(transition);
	}

	internal static bool SelectPredicate(this UnityEditor.Animations.AnimatorControllerLayer def, string result, bool iscontrol = true)
	{
		return def.stateMachine.anyStateTransitions.Any(delegate(AnimatorStateTransition t)
		{
			if (!DefinePredicate(t))
			{
				return false;
			}
			if (iscontrol && t.name == result)
			{
				return true;
			}
			return !iscontrol && t.name.Contains(result);
		});
	}

	internal static IEnumerable<string> RemovePredicate(this UnityEditor.Animations.AnimatorControllerLayer config)
	{
		return from t in config.stateMachine.anyStateTransitions.Where(DefinePredicate)
			select t.name;
	}

	internal static IEnumerable<string> InstantiatePredicate(this UnityEditor.Animations.AnimatorControllerLayer value)
	{
		return from s in value.RemovePredicate()
			where s[0] == '_'
			select s;
	}

	internal static IEnumerable<string> AwakePredicate(this UnityEditor.Animations.AnimatorControllerLayer res)
	{
		return from s in res.RemovePredicate()
			where s[0] != '_'
			select s;
	}

	internal static void ResetPredicate(UnityEditor.Animations.AnimatorController info, UnityEditor.Animations.AnimatorController cust, out UnityEditor.Animations.AnimatorControllerLayer[] dir)
	{
		try
		{
			UnityEditor.Animations.AnimatorControllerLayer[] layers = info.layers;
			int num = layers.Length;
			dir = new UnityEditor.Animations.AnimatorControllerLayer[num];
			for (int i = 0; i < num; i++)
			{
				EditorUtility.DisplayProgressBar("Copying Layers", $"Copying layer {i + 1} of {num}", (float)i / (float)num);
				dir[i] = CalculatePredicate(layers[i], cust);
			}
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}

	internal static void FlushPredicate(UnityEditor.Animations.AnimatorController instance, UnityEditor.Animations.AnimatorController cont, out UnityEditor.Animations.AnimatorControllerLayer[] comp, out UnityEngine.AnimatorControllerParameter[] config2, bool isreference3 = false)
	{
		try
		{
			UnityEditor.Animations.AnimatorControllerLayer[] layers = instance.layers;
			UnityEngine.AnimatorControllerParameter[] parameters = instance.parameters;
			int num = layers.Length;
			comp = new UnityEditor.Animations.AnimatorControllerLayer[num];
			config2 = new UnityEngine.AnimatorControllerParameter[parameters.Length];
			for (int i = 0; i < num; i++)
			{
				EditorUtility.DisplayProgressBar("Copying Layers", $"Copying layer {i + 1} of {num}", (float)i / (float)num);
				comp[i] = CalculatePredicate(layers[i], cont);
			}
			for (int j = 0; j < parameters.Length; j++)
			{
				EditorUtility.DisplayProgressBar("Copying Parameters", $"Copying parameter {j + 1} of {parameters.Length}", (float)j / (float)parameters.Length);
				bool dir;
				UnityEngine.AnimatorControllerParameter animatorControllerParameter = cont.DestroyPredicate(parameters[j], out dir);
				config2[j] = ((dir || isreference3) ? animatorControllerParameter : null);
			}
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}

	internal static UnityEngine.AnimatorControllerParameter[] ConnectPredicate(UnityEditor.Animations.AnimatorController asset, UnityEditor.Animations.AnimatorController selection, bool updatedic = false)
	{
		UnityEngine.AnimatorControllerParameter[] parameters = asset.parameters;
		UnityEngine.AnimatorControllerParameter[] array = new UnityEngine.AnimatorControllerParameter[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			EditorUtility.DisplayProgressBar("Copying Parameters", $"Copying parameter {i + 1} of {parameters.Length}", (float)i / (float)parameters.Length);
			bool dir;
			UnityEngine.AnimatorControllerParameter animatorControllerParameter = selection.DestroyPredicate(parameters[i], out dir);
			array[i] = ((!(dir || updatedic)) ? null : animatorControllerParameter);
		}
		EditorUtility.ClearProgressBar();
		return array;
	}

	internal static UnityEditor.Animations.AnimatorControllerLayer CalculatePredicate(UnityEditor.Animations.AnimatorControllerLayer value, UnityEditor.Animations.AnimatorController ivk, int row_role = -1, bool iscfg2 = false)
	{
		_003C_003Ec__DisplayClass128_0 connection = default(_003C_003Ec__DisplayClass128_0);
		connection.m_PublisherObserver = ivk;
		connection.configurationObserver = iscfg2;
		connection.m_WrapperServer = value;
		connection.annotationServer = new UnityEditor.Animations.AnimatorControllerLayer
		{
			name = connection.m_PublisherObserver.MakeUniqueLayerName(connection.m_WrapperServer.name)
		};
		TestPredicate(connection.m_WrapperServer, connection.annotationServer);
		connection.iteratorObserver = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
		connection._ProcObserver = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
		connection.annotationServer.stateMachine = ConnectError(connection.m_WrapperServer.stateMachine, ref connection);
		CalculateError(connection.annotationServer.stateMachine, ref connection);
		TestError(connection.annotationServer.stateMachine, ref connection);
		if (row_role >= 0 && row_role <= connection.m_PublisherObserver.layers.Length - 1)
		{
			UnityEditor.Animations.AnimatorControllerLayer[] array = connection.m_PublisherObserver.layers;
			ArrayUtility.Insert(ref array, row_role, connection.annotationServer);
			connection.m_PublisherObserver.layers = array;
		}
		else
		{
			connection.m_PublisherObserver.AddLayer(connection.annotationServer);
		}
		return connection.annotationServer;
	}

	internal static void TestPredicate(UnityEditor.Animations.AnimatorControllerLayer key, UnityEditor.Animations.AnimatorControllerLayer ord)
	{
		UnityEditor.Animations.AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(AssetDatabase.GetAssetPath(key.stateMachine));
		float defaultWeight = key.defaultWeight;
		if ((bool)animatorController && animatorController.layers.Length != 0 && animatorController.layers[0].stateMachine == key.stateMachine)
		{
			defaultWeight = 1f;
		}
		ord.defaultWeight = defaultWeight;
		ord.avatarMask = key.avatarMask;
		ord.blendingMode = key.blendingMode;
		ord.iKPass = key.iKPass;
		ord.syncedLayerAffectsTiming = key.syncedLayerAffectsTiming;
		ord.syncedLayerIndex = key.syncedLayerIndex;
	}

	internal static float MapPredicate(this UnityEngine.AnimatorControllerParameter instance)
	{
		return instance.type switch
		{
			UnityEngine.AnimatorControllerParameterType.Bool => instance.defaultBool ? 1 : 0, 
			UnityEngine.AnimatorControllerParameterType.Float => instance.defaultFloat, 
			UnityEngine.AnimatorControllerParameterType.Int => instance.defaultInt, 
			_ => 0f, 
		};
	}

	internal static void ValidatePredicate(this UnityEngine.AnimatorControllerParameter setup, float cust)
	{
		switch (setup.type)
		{
		case UnityEngine.AnimatorControllerParameterType.Bool:
			setup.defaultBool = cust > 0f;
			break;
		case UnityEngine.AnimatorControllerParameterType.Int:
			setup.defaultInt = Mathf.RoundToInt(cust);
			break;
		case UnityEngine.AnimatorControllerParameterType.Float:
			setup.defaultFloat = cust;
			break;
		case (UnityEngine.AnimatorControllerParameterType)2:
			break;
		}
	}

	internal static UnityEngine.AnimatorControllerParameter CustomizePredicate(this UnityEditor.Animations.AnimatorController init, string counter, UnityEngine.AnimatorControllerParameterType tag, float instance2 = 0f)
	{
		UnityEngine.AnimatorControllerParameter animatorControllerParameter = new UnityEngine.AnimatorControllerParameter
		{
			name = counter,
			type = tag,
			defaultBool = (instance2 != 0f),
			defaultInt = (int)instance2,
			defaultFloat = instance2
		};
		init.AddParameter(animatorControllerParameter);
		return animatorControllerParameter;
	}

	internal static UnityEngine.AnimatorControllerParameter RatePredicate(this UnityEditor.Animations.AnimatorController asset, UnityEngine.AnimatorControllerParameter map)
	{
		bool dir;
		return asset.DestroyPredicate(map, out dir);
	}

	internal static UnityEngine.AnimatorControllerParameter DestroyPredicate(this UnityEditor.Animations.AnimatorController var1, UnityEngine.AnimatorControllerParameter cust, out bool dir)
	{
		float attr = 0f;
		switch (cust.type)
		{
		case UnityEngine.AnimatorControllerParameterType.Bool:
			attr = (cust.defaultBool ? 1 : 0);
			break;
		case UnityEngine.AnimatorControllerParameterType.Float:
			attr = cust.defaultFloat;
			break;
		case UnityEngine.AnimatorControllerParameterType.Int:
			attr = cust.defaultInt;
			break;
		}
		return var1.IncludePredicate(cust.name, cust.type, attr, out dir);
	}

	internal static UnityEngine.AnimatorControllerParameter GetPredicate(this UnityEngine.AnimatorControllerParameter instance)
	{
		return new UnityEngine.AnimatorControllerParameter
		{
			name = instance.name,
			type = instance.type,
			defaultBool = instance.defaultBool,
			defaultInt = instance.defaultInt,
			defaultFloat = instance.defaultFloat
		};
	}

	internal static UnityEngine.AnimatorControllerParameter CalcPredicate(this UnityEditor.Animations.AnimatorController i, string connection, UnityEngine.AnimatorControllerParameterType helper, float map2)
	{
		bool pred;
		return i.IncludePredicate(connection, helper, map2, out pred);
	}

	internal static UnityEngine.AnimatorControllerParameter IncludePredicate(this UnityEditor.Animations.AnimatorController task, string pol, UnityEngine.AnimatorControllerParameterType proc, float attr2, out bool pred3)
	{
		UnityEngine.AnimatorControllerParameter[] parameters = task.parameters;
		int num = 0;
		UnityEngine.AnimatorControllerParameter animatorControllerParameter2;
		while (true)
		{
			if (num >= parameters.Length)
			{
				pred3 = true;
				UnityEngine.AnimatorControllerParameter animatorControllerParameter = new UnityEngine.AnimatorControllerParameter
				{
					name = pol,
					type = proc,
					defaultBool = (attr2 != 0f),
					defaultInt = (int)attr2,
					defaultFloat = attr2
				};
				task.AddParameter(animatorControllerParameter);
				return animatorControllerParameter;
			}
			animatorControllerParameter2 = parameters[num];
			if (animatorControllerParameter2.name != pol)
			{
				num++;
				continue;
			}
			break;
		}
		if (animatorControllerParameter2.type != proc)
		{
			$"Type mismatch! Parameter {pol} already exists in {task.name} but with type {animatorControllerParameter2.type} rather than {proc}".ReflectResolver(LogType.Warning);
		}
		pred3 = false;
		return animatorControllerParameter2;
	}

	internal static bool RunPredicate(this UnityEditor.Animations.AnimatorController ident, string selection, out UnityEngine.Object[] filter)
	{
		filter = null;
		if (managerProperty)
		{
			if (!(m_ParserProperty == null))
			{
				filter = (UnityEngine.Object[])m_ParserProperty.Invoke(ident, new object[1] { selection });
				return true;
			}
			return false;
		}
		while (true)
		{
			managerProperty = true;
		}
	}

	internal static bool ClonePredicate(this AnimatorTransitionBase i)
	{
		if (i.isExit)
		{
			return true;
		}
		if (!i.destinationState)
		{
			return !i.destinationStateMachine;
		}
		return false;
	}

	internal static int LoginPredicate(this AnimatorState def, StateMachineBehaviour reg)
	{
		return def.behaviours.FindResolver((StateMachineBehaviour b) => b == reg);
	}

	internal static void ReflectPredicate(this AnimatorState key, Type counter, bool isstate = false)
	{
		if (key.behaviours.ExcludeResolver((StateMachineBehaviour b) => b.GetType() == counter, out var c))
		{
			key.DeletePredicate(c, isstate);
		}
	}

	internal static void DeletePredicate(this AnimatorState spec, int visitor_Ptr, bool verifytemp = false)
	{
		if (visitor_Ptr < 0)
		{
			return;
		}
		if (verifytemp)
		{
			Undo.RecordObject(spec, "Delete Behaviour");
		}
		StateMachineBehaviour stateMachineBehaviour = spec.behaviours[visitor_Ptr];
		StateMachineBehaviour[] array = spec.behaviours;
		ArrayUtility.RemoveAt(ref array, visitor_Ptr);
		spec.behaviours = array;
		string assetPath = AssetDatabase.GetAssetPath(stateMachineBehaviour);
		if (!(AssetDatabase.LoadMainAssetAtPath(assetPath) == stateMachineBehaviour))
		{
			if (verifytemp)
			{
				Undo.DestroyObjectImmediate(stateMachineBehaviour);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(stateMachineBehaviour);
			}
		}
		else
		{
			AssetDatabase.DeleteAsset(assetPath);
		}
		EditorUtility.SetDirty(spec);
	}

	internal static bool CreatePredicate(IEnumerable<AnimatorCondition> key, IEnumerable<AnimatorCondition> b)
	{
		Dictionary<AnimatorCondition, int> dictionary = new Dictionary<AnimatorCondition, int>();
		foreach (AnimatorCondition item in key)
		{
			if (!dictionary.ContainsKey(item))
			{
				dictionary.Add(item, 1);
			}
			else
			{
				dictionary[item]++;
			}
		}
		foreach (AnimatorCondition item2 in b)
		{
			if (dictionary.ContainsKey(item2))
			{
				dictionary[item2]--;
				continue;
			}
			return false;
		}
		return dictionary.Values.All((int c) => c == 0);
	}

	internal static void NewPredicate(this UnityEditor.Animations.AnimatorController first, Action<AnimatorStateMachine> result, Action<AnimatorState> dic, Action<InstanceServer> config2)
	{
		UnityEditor.Animations.AnimatorControllerLayer[] layers = first.layers;
		for (int i = 0; i < layers.Length; i++)
		{
			layers[i].stateMachine.PushPredicate(result, dic, config2);
		}
	}

	internal static void PushPredicate(this AnimatorStateMachine last, Action<AnimatorStateMachine> ivk, Action<AnimatorState> comp, Action<InstanceServer> instance2, bool evaluateasset3 = true)
	{
		if (ivk != null)
		{
			last.CollectPredicate(ivk, evaluateasset3);
		}
		if (comp != null)
		{
			last.AssetPredicate(comp);
		}
		if (instance2 != null)
		{
			last.ResolvePredicate(instance2);
		}
	}

	internal static void ViewPredicate(this UnityEditor.Animations.AnimatorController i, Action<AnimatorStateMachine> pred)
	{
		UnityEditor.Animations.AnimatorControllerLayer[] layers = i.layers;
		foreach (UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer in layers)
		{
			pred(animatorControllerLayer.stateMachine);
		}
	}

	internal static void CollectPredicate(this AnimatorStateMachine init, Action<AnimatorStateMachine> col, bool containsthird = true)
	{
		col(init);
		if (containsthird)
		{
			ChildAnimatorStateMachine[] stateMachines = init.stateMachines;
			foreach (ChildAnimatorStateMachine childAnimatorStateMachine in stateMachines)
			{
				childAnimatorStateMachine.stateMachine.CollectPredicate(col);
			}
		}
	}

	internal static void ResolvePredicate(this AnimatorStateMachine asset, Action<InstanceServer> attr, bool moveserv = true)
	{
		if (!asset)
		{
			return;
		}
		AnimatorTransition[] entryTransitions = asset.entryTransitions;
		foreach (AnimatorTransition init in entryTransitions)
		{
			attr(new InstanceServer(init, InstanceServer.TransitionSourceType.EntryTransition, asset));
		}
		AnimatorStateTransition[] anyStateTransitions = asset.anyStateTransitions;
		foreach (AnimatorStateTransition init2 in anyStateTransitions)
		{
			attr(new InstanceServer(init2, InstanceServer.TransitionSourceType.AnyTransition, asset));
		}
		ChildAnimatorState[] states = asset.states;
		for (int i = 0; i < states.Length; i++)
		{
			ChildAnimatorState childAnimatorState = states[i];
			anyStateTransitions = childAnimatorState.state.transitions;
			foreach (AnimatorStateTransition param in anyStateTransitions)
			{
				attr(new InstanceServer(param, InstanceServer.TransitionSourceType.StateTransition, childAnimatorState.state));
			}
		}
		ChildAnimatorStateMachine[] stateMachines = asset.stateMachines;
		for (int i = 0; i < stateMachines.Length; i++)
		{
			ChildAnimatorStateMachine childAnimatorStateMachine = stateMachines[i];
			entryTransitions = asset.GetStateMachineTransitions(childAnimatorStateMachine.stateMachine);
			foreach (AnimatorTransition item in entryTransitions)
			{
				attr(new InstanceServer(item, InstanceServer.TransitionSourceType.MachineTransition, childAnimatorStateMachine.stateMachine, asset));
			}
		}
		if (!moveserv)
		{
			return;
		}
		stateMachines = asset.stateMachines;
		for (int i = 0; i < stateMachines.Length; i++)
		{
			ChildAnimatorStateMachine childAnimatorStateMachine2 = stateMachines[i];
			if (childAnimatorStateMachine2.stateMachine != asset)
			{
				childAnimatorStateMachine2.stateMachine.ResolvePredicate(attr);
			}
		}
	}

	internal static void ListPredicate(this UnityEditor.Animations.BlendTree var1, Action<AnimationClip> reg, bool dicreguired = true)
	{
		var1.VerifyPredicate(delegate(AnimationClip c)
		{
			reg(c);
			return false;
		}, dicreguired);
	}

	internal static bool VerifyPredicate(this UnityEditor.Animations.BlendTree reference, Func<AnimationClip, bool> cust, bool rejectrule = true)
	{
		ChildMotion[] children = reference.children;
		int num = 0;
		while (true)
		{
			if (num >= children.Length)
			{
				return false;
			}
			ChildMotion childMotion = children[num];
			if (!(childMotion.motion == null))
			{
				if (childMotion.motion is AnimationClip arg)
				{
					if (cust(arg))
					{
						return true;
					}
				}
				else if (rejectrule && childMotion.motion is UnityEditor.Animations.BlendTree reference2 && reference2.VerifyPredicate(cust))
				{
					break;
				}
			}
			num++;
		}
		return true;
	}

	internal static void FillPredicate(this UnityEditor.Animations.BlendTree res, Action<UnityEditor.Animations.BlendTree> ord, bool isserv = true, bool isvisitor2 = true)
	{
		res.WritePredicate(delegate(UnityEditor.Animations.BlendTree t)
		{
			ord(t);
			return false;
		}, isserv, isvisitor2);
	}

	internal static bool WritePredicate(this UnityEditor.Animations.BlendTree key, Func<UnityEditor.Animations.BlendTree, bool> cont, bool excludec = true, bool isident2 = true)
	{
		if (isident2 && cont(key))
		{
			return true;
		}
		return key.children.Any(delegate(ChildMotion c)
		{
			if (!(c.motion is UnityEditor.Animations.BlendTree blendTree) || !(blendTree != null))
			{
				return false;
			}
			if (cont(blendTree))
			{
				return true;
			}
			return excludec && blendTree.WritePredicate(cont, excludec: true, isident2: false);
		});
	}

	internal static void ForgotPredicate(this Motion config, Action<Motion> ivk)
	{
		config.CheckPredicate(delegate(UnityEditor.Animations.BlendTree t)
		{
			ivk(t);
			return false;
		}, delegate(AnimationClip c)
		{
			ivk(c);
			return false;
		});
	}

	internal static void StopPredicate(this Motion var1, Action<UnityEditor.Animations.BlendTree> vis, Action<AnimationClip> consumer)
	{
		var1.CheckPredicate(delegate(UnityEditor.Animations.BlendTree t)
		{
			vis(t);
			return false;
		}, delegate(AnimationClip c)
		{
			consumer(c);
			return false;
		});
	}

	internal static bool CheckPredicate(this Motion i, Func<UnityEditor.Animations.BlendTree, bool> result, Func<AnimationClip, bool> pool)
	{
		if (!(i == null))
		{
			if (!(i is UnityEditor.Animations.BlendTree blendTree))
			{
				if (pool != null && i is AnimationClip arg && pool(arg))
				{
					return true;
				}
			}
			else
			{
				if (result != null && result(blendTree))
				{
					return true;
				}
				ChildMotion[] children = blendTree.children;
				foreach (ChildMotion childMotion in children)
				{
					if (childMotion.motion.CheckPredicate(result, pool))
					{
						return true;
					}
				}
			}
			return false;
		}
		return false;
	}

	internal static void PreparePredicate(this UnityEditor.Animations.AnimatorController i, Action<AnimatorState> pol)
	{
		UnityEditor.Animations.AnimatorControllerLayer[] layers = i.layers;
		for (int j = 0; j < layers.Length; j++)
		{
			layers[j].stateMachine.AssetPredicate(pol);
		}
	}

	internal static void AssetPredicate(this AnimatorStateMachine param, Action<AnimatorState> second, bool requiresc = true)
	{
		ChildAnimatorState[] states = param.states;
		foreach (ChildAnimatorState childAnimatorState in states)
		{
			second(childAnimatorState.state);
		}
		if (!requiresc)
		{
			return;
		}
		ChildAnimatorStateMachine[] stateMachines = param.stateMachines;
		for (int i = 0; i < stateMachines.Length; i++)
		{
			ChildAnimatorStateMachine childAnimatorStateMachine = stateMachines[i];
			if (childAnimatorStateMachine.stateMachine != param)
			{
				childAnimatorStateMachine.stateMachine.AssetPredicate(second);
			}
		}
	}

	internal static bool UpdatePredicate(this UnityEditor.Animations.AnimatorController ident, Func<AnimatorState, bool> ord)
	{
		UnityEditor.Animations.AnimatorControllerLayer[] layers = ident.layers;
		int num = 0;
		while (true)
		{
			if (num >= layers.Length)
			{
				return false;
			}
			if (layers[num].stateMachine.ChangePredicate(ord))
			{
				break;
			}
			num++;
		}
		return true;
	}

	internal static bool ChangePredicate(this AnimatorStateMachine last, Func<AnimatorState, bool> second, bool containsdir = true)
	{
		if (!last.states.Any((ChildAnimatorState cs) => second(cs.state)))
		{
			if (containsdir)
			{
				return last.stateMachines.Where((ChildAnimatorStateMachine cm) => cm.stateMachine != last).Any((ChildAnimatorStateMachine cm) => cm.stateMachine.ChangePredicate(second));
			}
			return false;
		}
		return true;
	}

	internal static int SortPredicate(UnityEditor.Animations.AnimatorController i, bool extractselection = true)
	{
		if ((bool)i)
		{
			bool _ThreadServer = true;
			int policyServer = 2;
			i.UpdatePredicate(delegate(AnimatorState s)
			{
				_ThreadServer = false;
				if (s.writeDefaultValues)
				{
					int num = policyServer;
					if (num == 0)
					{
						policyServer = 2;
						return true;
					}
					if (num == 2)
					{
						policyServer = 1;
					}
				}
				else
				{
					int num = policyServer;
					if (num == 1)
					{
						policyServer = 2;
						return true;
					}
					if (num == 2)
					{
						policyServer = 0;
					}
				}
				return false;
			});
			if (_ThreadServer)
			{
				policyServer = (extractselection ? 1 : 0);
			}
			return policyServer;
		}
		if (extractselection)
		{
			return 1;
		}
		return 0;
	}

	internal static Dictionary<UnityEngine.Object, RepositoryServer> RegisterPredicate(this AnimatorStateMachine def, Dictionary<UnityEngine.Object, RepositoryServer> cust = null, bool addtemplate = true)
	{
		if (cust == null)
		{
			cust = new Dictionary<UnityEngine.Object, RepositoryServer>();
		}
		_003C_003Ec__DisplayClass164_0 CS_0024_003C_003E8__locals0;
		def.CollectPredicate(delegate(AnimatorStateMachine m)
		{
			_003C_003Ec__DisplayClass164_1 _003C_003Ec__DisplayClass164_ = new _003C_003Ec__DisplayClass164_1();
			_003C_003Ec__DisplayClass164_._ResolverServer = CS_0024_003C_003E8__locals0;
			_003C_003Ec__DisplayClass164_._PageServer = m;
			if (_003C_003Ec__DisplayClass164_._PageServer == null)
			{
				return;
			}
			_003C_003Ec__DisplayClass164_._PageServer.ResolvePredicate(_003C_003Ec__DisplayClass164_.ManageSetter, moveserv: false);
			foreach (AnimatorState item in _003C_003Ec__DisplayClass164_._PageServer.states.Select(_003C_003Ec.m_ConfigObserver.RateSetter))
			{
				if (!cust.ContainsKey(item))
				{
					cust.Add(item, new RepositoryServer(item));
				}
			}
			foreach (AnimatorStateMachine item2 in _003C_003Ec__DisplayClass164_._PageServer.stateMachines.Select(_003C_003Ec.m_ConfigObserver.DestroySetter))
			{
				if (!cust.ContainsKey(item2))
				{
					cust.Add(item2, new RepositoryServer(_003C_003Ec__DisplayClass164_._PageServer, item2));
				}
			}
		}, addtemplate);
		return cust;
	}

	internal static void LogoutPredicate(UnityEngine.Object config, bool isattr = false)
	{
		string assetPath = AssetDatabase.GetAssetPath(config);
		UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
		if (!obj)
		{
			return;
		}
		if (!(obj != config))
		{
			AssetDatabase.DeleteAsset(assetPath);
			return;
		}
		AssetDatabase.RemoveObjectFromAsset(config);
		if (isattr)
		{
			Undo.DestroyObjectImmediate(config);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(config);
		}
	}

	internal static bool PatchPredicate(UnityEngine.Object param, out UnityEngine.Object result)
	{
		result = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(param));
		return param != result;
	}

	internal static void InterruptPredicate(UnityEngine.Object asset, UnityEngine.Object counter, bool isutil = true, bool removeinstance2 = false)
	{
		AssetDatabase.AddObjectToAsset(asset, counter);
		if (isutil)
		{
			asset.hideFlags |= HideFlags.HideInHierarchy;
			asset.hideFlags |= HideFlags.HideInInspector;
		}
		if (removeinstance2)
		{
			asset.hideFlags |= HideFlags.NotEditable;
		}
		EditorUtility.SetDirty(counter);
	}

	internal static void ManagePredicate(this UnityEngine.Object first)
	{
		EditorUtility.SetDirty(first);
	}

	internal static void PrintPredicate(this UnityEngine.Object last)
	{
		try
		{
			if (!MapError(last))
			{
				EditorUtility.SetDirty(last);
			}
			if (last is GameObject gameObject)
			{
				EditorSceneManager.MarkSceneDirty(gameObject.scene);
			}
			else if (last is Component component)
			{
				EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
			}
		}
		catch
		{
		}
	}

	internal static T SearchPredicate<T>(string asset, long versioncaller) where T : UnityEngine.Object
	{
		T val = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(asset));
		string guid;
		if (val != null)
		{
			if (versioncaller == 0L)
			{
				return val;
			}
			AssetDatabase.TryGetGUIDAndLocalFileIdentifier((UnityEngine.Object)val, out guid, out long localId);
			if (localId == versioncaller)
			{
				return val;
			}
		}
		UnityEngine.Object[] array = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(asset));
		int num = 0;
		T val2;
		while (true)
		{
			if (num >= array.Length)
			{
				return null;
			}
			UnityEngine.Object obj = array[num];
			AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out long localId2);
			if (localId2 == versioncaller)
			{
				val2 = obj as T;
				if ((object)val2 != null)
				{
					break;
				}
			}
			num++;
		}
		return val2;
	}

	internal static T RevertPredicate<T>(string item, string result) where T : UnityEngine.Object
	{
		return SearchPredicate<T>(item, (!string.IsNullOrWhiteSpace(result)) ? long.Parse(result) : 0L);
	}

	internal static T OrderRules<T>(T setup, string pol) where T : UnityEngine.Object
	{
		string assetPath = AssetDatabase.GetAssetPath(setup);
		UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
		if (!obj)
		{
			return null;
		}
		if (setup != obj)
		{
			T val = UnityEngine.Object.Instantiate(setup);
			AssetDatabase.CreateAsset(val, pol);
			return val;
		}
		AssetDatabase.CopyAsset(assetPath, pol);
		return AssetDatabase.LoadAssetAtPath<T>(pol);
	}

	internal static T CompareRules<T>(T task) where T : UnityEngine.Object
	{
		if ((bool)task)
		{
			Type type = task.GetType();
			UnityEngine.Object obj = ((type.IsSubclassOf(typeof(ScriptableObject)) || type == typeof(ScriptableObject)) ? ScriptableObject.CreateInstance(type) : ((UnityEngine.Object)Activator.CreateInstance(type)));
			EditorUtility.CopySerialized(task, obj);
			return (T)obj;
		}
		return null;
	}

	internal static T SetRules<T>(T reference, string vis, out bool proc, bool isinfo2 = true) where T : UnityEngine.Object
	{
		proc = PatchPredicate(reference, out var _);
		T val = CompareRules(reference);
		if (!proc || isinfo2)
		{
			AssetDatabase.CreateAsset(val, vis);
		}
		return val;
	}

	internal static T PostRules<T>(T setup, UnityEngine.Object counter) where T : UnityEngine.Object
	{
		T val = CompareRules(setup);
		InterruptPredicate(val, counter);
		return val;
	}

	internal static void SetupRules<T>(Action<T> key, string ord = "Create New File", string control = "New Asset", string asset2 = "asset") where T : UnityEngine.Object
	{
		if (!AssetDatabase.IsValidFolder(itemProperty))
		{
			itemProperty = "Assets";
		}
		control = Path.GetFileNameWithoutExtension(AssetDatabase.GenerateUniqueAssetPath(itemProperty + "/" + control + "." + asset2));
		string text = EditorUtility.SaveFilePanel(ord, itemProperty, control, asset2);
		if (!string.IsNullOrWhiteSpace(text))
		{
			text = FileUtil.GetProjectRelativePath(text);
			if (text.StartsWith("Assets"))
			{
				itemProperty = Path.GetDirectoryName(text);
				Type typeFromHandle = typeof(T);
				UnityEngine.Object obj = ((typeFromHandle.IsSubclassOf(typeof(ScriptableObject)) || typeFromHandle == typeof(ScriptableObject)) ? ScriptableObject.CreateInstance(typeFromHandle) : (Activator.CreateInstance(typeFromHandle) as UnityEngine.Object));
				AssetDatabase.CreateAsset(obj, text);
				key?.Invoke(obj as T);
			}
			else
			{
				"Path must be in the Assets folder!".CloneResolver(LogType.Warning).LoginResolver(LogType.Warning);
			}
		}
	}

	internal static string EnableRules(string asset, string pol)
	{
		using (new GUILayout.HorizontalScope())
		{
			EditorGUILayout.PrefixLabel(pol);
			EditorGUILayout.SelectableLabel(asset, EditorStyles.objectField, GUILayout.Height(16f), GUILayout.ExpandWidth(expand: true));
			if (CallQueue(DestroyError().invocationProcessor))
			{
				string text = asset;
				if (!text.StartsWith("Assets"))
				{
					text = "Assets";
				}
				else
				{
					while (!AssetDatabase.IsValidFolder(text))
					{
						text = Path.GetDirectoryName(text);
					}
				}
				string text2 = EditorUtility.OpenFolderPanel(pol, text, string.Empty);
				if (string.IsNullOrEmpty(text2))
				{
					return asset;
				}
				string projectRelativePath = FileUtil.GetProjectRelativePath(text2);
				if (!projectRelativePath.StartsWith("Assets"))
				{
					"New Path must be a folder within Assets!".ReflectResolver(LogType.Warning);
					return asset;
				}
				asset = projectRelativePath;
			}
			using (new EditorGUI.DisabledScope(!AssetDatabase.IsValidFolder(asset)))
			{
				if (CallQueue(DestroyError()._FactoryProcessor))
				{
					EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(asset));
				}
			}
		}
		return asset;
	}

	internal static void PublishRules(UnityEngine.Object last, string second)
	{
		AssetDatabase.AddObjectToAsset(new TextAsset(second)
		{
			name = second,
			hideFlags = (HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable)
		}, last);
	}

	internal static void PopRules<T>(string asset, string b, T util, Action<T> param2, SystemThread config3, Action counter4, Action<T> ivk5 = null, bool isord6 = true, string cust7 = null) where T : UnityEngine.Object
	{
		ComputeRules(new GUIContent(asset), b, util, param2, config3, counter4, ivk5, isord6, cust7);
	}

	internal static void ComputeRules<T>(GUIContent i, string attr, T state, Action<T> selection2, SystemThread pol3, Action visitor4, Action<T> second5 = null, bool containstask6 = true, string asset7 = null) where T : UnityEngine.Object
	{
		if (asset7 == null)
		{
			CancelRules(typeof(T), out asset7);
		}
		bool readerThread = pol3.readerThread;
		bool workerThread = pol3._WorkerThread;
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Label(i, GUILayout.MaxWidth(120f));
			using (new GUILayout.HorizontalScope(EditorStyles.objectField, GUILayout.ExpandHeight(expand: true)))
			{
				TemplateThread templateThread = ((!readerThread) ? null : new TemplateThread(TemplateThread.ColoringType.FG, workerThread, configurationProperty, _WrapperProcessor));
				GUILayout.Label(attr, CalcError()._StructProcessor);
				if (readerThread)
				{
					templateThread.Dispose();
				}
			}
			Rect def = GUILayoutUtility.GetLastRect();
			Texture2D miniTypeThumbnail = AssetPreview.GetMiniTypeThumbnail(typeof(T));
			if (miniTypeThumbnail == AssetPreview.GetMiniTypeThumbnail(typeof(ScriptableObject)))
			{
				miniTypeThumbnail = AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
			}
			GUI.DrawTexture(def.SortResolver(18f, isfield: true, 2f, iscont3: true, isattr4: false).VerifyResolver(-1f), miniTypeThumbnail);
			if (readerThread)
			{
				Texture image = (workerThread ? DestroyError().m_CodeProcessor.image : DestroyError().issuerProcessor.CompareHelper());
				GUI.Label(def.PatchResolver(18f, isserv: true, 4f, isvisitor3: true, istoken4: false).VerifyResolver(-1f), workerThread ? new GUIContent(image)
				{
					tooltip = "All Good!"
				} : new GUIContent(image)
				{
					tooltip = pol3.m_FilterThread
				}, CalcError().broadcasterProcessor);
			}
			StartQueue(def);
			DispatcherPolicy dispatcherPolicy = new DispatcherPolicy(Event.current).InvokeHelper().QueryHelper(def);
			int controlID = GUIUtility.GetControlID(FocusType.Keyboard, def);
			if (containstask6 && GUIUtility.keyboardControl == controlID && VerifyQueue())
			{
				selection2(null);
			}
			if (dispatcherPolicy.m_TagPolicy)
			{
				GUIUtility.keyboardControl = controlID;
				if (state == null || (bool)dispatcherPolicy.VisitHelper().ExcludeHelper() || (bool)dispatcherPolicy.InitHelper())
				{
					ConcatList(state, typeof(T), null, null, loaddef3: false, null, null, delegate(UnityEngine.Object o)
					{
						if (containstask6 || o != null)
						{
							selection2((T)o);
						}
					});
				}
				else
				{
					PrintRules("ProjectBrowser", isattr: true);
					EditorGUIUtility.PingObject(state);
				}
				dispatcherPolicy.DefineHelper();
			}
			InstantiateRules(def, selection2);
			using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(90f)))
			{
				GUILayout.FlexibleSpace();
				visitor4?.Invoke();
				MoveRules(selection2, state, second5, asset7, containstask6, loadpred4: false);
			}
		}
	}

	internal static void MoveRules<T>(Action<T> instance, T second, Action<T> rule = null, string token2 = "asset", bool acceptconfig3 = true, bool loadpred4 = true) where T : UnityEngine.Object
	{
		if (instance != null)
		{
			string name = typeof(T).Name;
			if (CallQueue(new GUIContent(DestroyError().invocationProcessor)
			{
				tooltip = "Select from Project"
			}))
			{
				ConcatList(second, typeof(T), null, null, loaddef3: false, null, null, delegate(UnityEngine.Object o)
				{
					if (acceptconfig3 || o != null)
					{
						instance((T)o);
					}
				});
			}
			string text = "New " + name;
			using (new EditorGUI.DisabledScope(disabled: false))
			{
				if (CallQueue(new GUIContent(DestroyError().repositoryProcessor)
				{
					tooltip = "Create " + text
				}))
				{
					SetupRules(delegate(T f)
					{
						rule?.Invoke(f);
						instance(f);
					}, "Create " + text, text, token2);
				}
			}
		}
		if (loadpred4)
		{
			IncludeQueue(second);
		}
	}

	internal static bool ConcatRules(this UnityEngine.Object key)
	{
		return key == null;
	}

	internal static bool CallRules(this UnityEngine.Object instance, out bool cfg)
	{
		bool flag = (object)instance != null;
		cfg = flag && instance == null;
		return !flag | cfg;
	}

	internal static bool CancelRules(Type init, out string token)
	{
		if (!m_SpecificationProperty.TryGetValue(init, out token))
		{
			token = "asset";
			return false;
		}
		return true;
	}

	internal static void CountRules(Action spec)
	{
		bool num = _MethodProperty.Count == 0;
		_MethodProperty.Enqueue(spec);
		if (num)
		{
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(InsertRules));
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, new EditorApplication.CallbackFunction(InsertRules));
		}
	}

	internal static void DisableRules(Action config, bool isb = true)
	{
		bool num = _SchemaProperty.Count == 0;
		_SchemaProperty.Enqueue(config);
		if (num)
		{
			EditorApplication.hierarchyWindowItemOnGUI = (EditorApplication.HierarchyWindowItemCallback)Delegate.Remove(EditorApplication.hierarchyWindowItemOnGUI, new EditorApplication.HierarchyWindowItemCallback(RestartRules));
			EditorApplication.hierarchyWindowItemOnGUI = (EditorApplication.HierarchyWindowItemCallback)Delegate.Combine(EditorApplication.hierarchyWindowItemOnGUI, new EditorApplication.HierarchyWindowItemCallback(RestartRules));
		}
		if (isb)
		{
			EditorApplication.RepaintHierarchyWindow();
		}
	}

	private static void InsertRules()
	{
		while (_MethodProperty.Count != 0)
		{
			Action action = _MethodProperty.Dequeue();
			try
			{
				action();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(InsertRules));
	}

	private static void RestartRules(int end_init, Rect connection)
	{
		while (_SchemaProperty.Count != 0)
		{
			Action action = _SchemaProperty.Dequeue();
			try
			{
				action();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		EditorApplication.hierarchyWindowItemOnGUI = (EditorApplication.HierarchyWindowItemCallback)Delegate.Remove(EditorApplication.hierarchyWindowItemOnGUI, new EditorApplication.HierarchyWindowItemCallback(RestartRules));
	}

	internal static async Task<T> QueryRules<T>(this Task<T> config, Action<T> token, Action<Exception> c = null, Action last2 = null, Action connection3 = null, Action ident4 = null)
	{
		object obj;
		try
		{
			obj = await config;
		}
		catch
		{
			obj = default(T);
		}
		if (config.IsCompleted)
		{
			if (connection3 != null)
			{
				try
				{
					connection3();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					throw;
				}
			}
			if (!config.IsFaulted || config.IsCanceled)
			{
				if (config.IsFaulted || !config.IsCanceled)
				{
					try
					{
						token((T)obj);
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						throw ex;
					}
				}
				else if (last2 != null)
				{
					try
					{
						last2();
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						throw ex2;
					}
				}
			}
			else
			{
				Exception baseException = config.Exception.GetBaseException();
				if (c == null)
				{
					Debug.LogException(baseException);
				}
				else
				{
					try
					{
						c(baseException);
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						throw ex3;
					}
				}
			}
			if (ident4 != null)
			{
				try
				{
					ident4();
				}
				catch (Exception ex4)
				{
					Debug.LogException(ex4);
					throw ex4;
				}
			}
		}
		else
		{
			Debug.LogError("FATAL ERROR! Task not completed?");
		}
		return (T)obj;
	}

	internal static AvatarMask AddRules(UnityEditor.Animations.AnimatorController setup)
	{
		bool flag;
		if ((flag = setup.layers[0].avatarMask != null) && EditorUtility.DisplayDialog("Existing Mask!", "The Base Layer already uses a mask! Continue Anyway?", "Continue", "Cancel"))
		{
			flag = false;
		}
		if (flag)
		{
			return null;
		}
		AvatarMask avatarMask = RemoveRules();
		avatarMask.ExcludeRules();
		for (int i = 1; i < setup.layers.Length; i++)
		{
			UnityEditor.Animations.AnimatorControllerLayer animatorControllerLayer = setup.layers[i];
			if ((bool)animatorControllerLayer.avatarMask)
			{
				avatarMask.DefineRules(animatorControllerLayer.avatarMask);
			}
		}
		avatarMask.InitRules();
		return avatarMask;
	}

	internal static AvatarMask InvokeRules(UnityEditor.Animations.AnimatorController asset, Transform cust)
	{
		AvatarMask avatarMask = RemoveRules();
		avatarMask.ExcludeRules();
		UnityEditor.Animations.AnimatorControllerLayer[] layers = asset.layers;
		foreach (UnityEditor.Animations.AnimatorControllerLayer var in layers)
		{
			avatarMask.DefineRules(FindRules(var, cust));
		}
		avatarMask.InitRules();
		return avatarMask;
	}

	internal static AvatarMask FindRules(UnityEditor.Animations.AnimatorControllerLayer var1, Transform reg)
	{
		AvatarMask m_PredicateServer = new AvatarMask();
		Transform transform = reg;
		if (!reg)
		{
			transform = new GameObject("Dummy").transform;
		}
		m_PredicateServer.AddTransformPath(transform, recursive: false);
		if (!reg)
		{
			UnityEngine.Object.DestroyImmediate(transform.gameObject);
		}
		for (int i = 0; i < 13; i++)
		{
			m_PredicateServer.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, value: false);
		}
		HashSet<string> m_QueueServer = new HashSet<string>();
		var1.stateMachine.AssetPredicate(delegate(AnimatorState s)
		{
			s.motion.StopPredicate(null, delegate(AnimationClip c)
			{
				foreach (EditorCurveBinding item in AnimationUtility.GetCurveBindings(c).Concat(AnimationUtility.GetObjectReferenceCurveBindings(c)))
				{
					AvatarMaskBodyPart visitor;
					if (!(item.path == string.Empty))
					{
						MessageServer messageServer = null;
						Transform transform2;
						if (!reg)
						{
							messageServer = new MessageServer(item.path);
							transform2 = messageServer.m_CollectionServer.Last().transform;
						}
						else
						{
							transform2 = reg.Find(item.path);
						}
						if ((bool)transform2 && !m_QueueServer.Contains(item.path))
						{
							m_PredicateServer.AddTransformPath(transform2, recursive: false);
							m_QueueServer.Add(item.path);
						}
						messageServer?.EnableContext();
					}
					else if (item.type == typeof(Animator) && SelectRules(item.propertyName, out visitor))
					{
						m_PredicateServer.SetHumanoidBodyPartActive(visitor, value: true);
					}
				}
			});
		});
		m_PredicateServer.InitRules();
		return m_PredicateServer;
	}

	internal static void ExcludeRules(this AvatarMask spec, bool compareselection = false)
	{
		if (compareselection || spec.transformCount == 0)
		{
			GameObject gameObject = new GameObject();
			spec.AddTransformPath(gameObject.transform);
			UnityEngine.Object.DestroyImmediate(gameObject);
		}
	}

	internal static void InitRules(this AvatarMask config, bool compareb = false)
	{
		config.ExcludeRules();
		if (compareb || config.transformCount <= 1)
		{
			GameObject gameObject = new GameObject();
			GameObject gameObject2 = new GameObject("Dummy Transform");
			gameObject2.transform.parent = gameObject.transform;
			config.AddTransformPath(gameObject2.transform);
			UnityEngine.Object.DestroyImmediate(gameObject);
			UnityEngine.Object.DestroyImmediate(gameObject2);
		}
	}

	internal static void VisitRules(this AvatarMask res, string connection)
	{
		MessageServer messageServer = new MessageServer(connection);
		res.AddTransformPath(messageServer.m_CollectionServer.Last().transform);
		messageServer.EnableContext();
	}

	internal static void DefineRules(this AvatarMask v, AvatarMask token)
	{
		for (int i = 0; i < 13; i++)
		{
			if (token.GetHumanoidBodyPartActive((AvatarMaskBodyPart)i))
			{
				v.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, value: true);
			}
		}
		List<(string, bool)> list = token.StartRules();
		if (list.Count <= 0)
		{
			return;
		}
		HashSet<string> setterServer = v.ReadRules();
		foreach (var item2 in list.Where(((string, bool) s) => !setterServer.Contains(s.Item1) && s.Item2))
		{
			string item = item2.Item1;
			v.VisitRules(item);
			setterServer.Add(item);
		}
	}

	internal static List<(string, bool)> StartRules(this AvatarMask def)
	{
		List<(string, bool)> list = new List<(string, bool)>();
		int transformCount = def.transformCount;
		for (int i = 0; i < transformCount; i++)
		{
			list.Add((def.GetTransformPath(i), def.GetTransformActive(i)));
		}
		return list;
	}

	internal static HashSet<string> ReadRules(this AvatarMask task)
	{
		HashSet<string> hashSet = new HashSet<string>();
		int transformCount = task.transformCount;
		for (int i = 0; i < transformCount; i++)
		{
			hashSet.Add(task.GetTransformPath(i));
		}
		return hashSet;
	}

	internal static bool SelectRules(string setup, out AvatarMaskBodyPart visitor)
	{
		string[] source = new string[3] { "Arm", "Forearm", "Shoulder" };
		string[] source2 = new string[3] { "Leg", "Foot", "Toes" };
		string[] source3 = new string[4] { "Neck", "Head", "Eye", "Jaw" };
		string[] source4 = new string[2] { "Chest", "Spine" };
		bool flag = setup.Contains("Left");
		if (!setup.Contains("Hand"))
		{
			if (!setup.Contains("Root"))
			{
				if (!source4.Any(setup.Contains))
				{
					if (source.Any(setup.Contains))
					{
						visitor = ((!flag) ? AvatarMaskBodyPart.RightArm : AvatarMaskBodyPart.LeftArm);
						return true;
					}
					if (!source2.Any(setup.Contains))
					{
						if (!source3.Any(setup.Contains))
						{
							visitor = AvatarMaskBodyPart.LastBodyPart;
							return false;
						}
						visitor = AvatarMaskBodyPart.Head;
						return true;
					}
					visitor = ((!flag) ? AvatarMaskBodyPart.RightLeg : AvatarMaskBodyPart.LeftLeg);
					return true;
				}
				visitor = AvatarMaskBodyPart.Body;
				return true;
			}
			visitor = AvatarMaskBodyPart.Root;
			return true;
		}
		visitor = (flag ? AvatarMaskBodyPart.LeftFingers : AvatarMaskBodyPart.RightFingers);
		return true;
	}

	internal static AvatarMask RemoveRules()
	{
		AvatarMask avatarMask = new AvatarMask();
		for (int i = 0; i < 13; i++)
		{
			avatarMask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, value: false);
		}
		return avatarMask;
	}

	internal static void InstantiateRules<T>(Rect ident, Action<T> ivk, Func<T, bool> control = null, Action ident2 = null) where T : UnityEngine.Object
	{
		Event current = Event.current;
		if ((current.type == EventType.DragPerform || current.type == EventType.DragUpdated) && ident.Contains(current.mousePosition))
		{
			T val = ((!typeof(T).IsSubclassOf(typeof(Component))) ? DragAndDrop.objectReferences.OfType<T>().FirstOrDefault((T el) => control?.Invoke(el) ?? true) : DragAndDrop.objectReferences.Select(delegate(UnityEngine.Object o)
			{
				GameObject obj = o as GameObject;
				return ((object)obj != null) ? obj.GetComponent<T>() : null;
			}).FirstOrDefault((T c) => c != null && (control?.Invoke(c) ?? true)));
			bool flag;
			if (flag = val != null)
			{
				ident2?.Invoke();
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			}
			if (current.type == EventType.DragPerform && flag)
			{
				DragAndDrop.AcceptDrag();
				ivk(val);
			}
			current.Use();
		}
	}

	internal static void AwakeRules<T>(Rect value, Action<IEnumerable<T>> ord, Func<T, bool> filter = null, Action asset2 = null) where T : UnityEngine.Object
	{
		Event current = Event.current;
		if ((current.type == EventType.DragPerform || current.type == EventType.DragUpdated) && value.Contains(current.mousePosition))
		{
			T[] array = ((!typeof(T).IsSubclassOf(typeof(Component))) ? (from o in DragAndDrop.objectReferences.OfType<T>()
				where o != null && (filter?.Invoke(o) ?? true)
				select o).ToArray() : (from c in DragAndDrop.objectReferences.Select(delegate(UnityEngine.Object o)
				{
					GameObject obj = o as GameObject;
					return ((object)obj != null) ? obj.GetComponent<T>() : null;
				})
				where c != null && (filter?.Invoke(c) ?? true)
				select c).ToArray());
			bool flag;
			if (flag = array.Length != 0)
			{
				asset2?.Invoke();
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			}
			if (current.type == EventType.DragPerform && flag)
			{
				DragAndDrop.AcceptDrag();
				ord(array);
			}
			current.Use();
		}
	}

	internal static PositionFlag ResetRules(PositionFlag key, Rect vis, PositionFlag util = PositionFlag.All)
	{
		AwakeQueue(vis, MouseCursor.Pan);
		float num = vis.width / 3f;
		float num2 = vis.height / 3f;
		foreach (PositionFlag item in PositionFlag.All.CancelPredicate())
		{
			if (item == (PositionFlag)0 || (item & (item - 1)) != 0)
			{
				continue;
			}
			Rect rect = vis;
			if (item.PrintPage())
			{
				rect.x += num * 2f;
			}
			else if (!item.SearchPage())
			{
				rect.x += num;
			}
			if (!item.OrderResolver())
			{
				if (!item.RevertPage())
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
			Rect key2 = rect;
			key2.x += num4;
			key2.y += num4;
			key2.width -= num3;
			key2.height -= num3;
			PostResolver(key2, Color.clear, Color.grey);
			if (!util.HasFlag(item))
			{
				PostResolver(rect, new Color(1f, 0.5f, 0.5f, 0.5f), Color.clear);
			}
			else if (Event.current.type == EventType.Repaint)
			{
				if (rect.Contains(Event.current.mousePosition))
				{
					key = item;
					PostResolver(rect, new Color(0.5f, 1f, 0.5f, 0.33f), Color.clear);
				}
				else
				{
					PostResolver(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f), Color.clear);
				}
			}
		}
		return key;
	}

	internal static void FlushRules<T>(SerializedProperty init) where T : UnityEngine.Object
	{
		bool hasMultipleDifferentValues;
		if (!(hasMultipleDifferentValues = init.hasMultipleDifferentValues))
		{
			for (int i = 0; i < init.arraySize; i++)
			{
				SerializedProperty arrayElementAtIndex = init.GetArrayElementAtIndex(i);
				if (arrayElementAtIndex == null)
				{
					continue;
				}
				if (!(arrayElementAtIndex.objectReferenceValue == null))
				{
					using (new GUILayout.HorizontalScope())
					{
						EditorGUILayout.PropertyField(arrayElementAtIndex, GUIContent.none);
						if (RestartQueue(DestroyError().m_DecoratorProcessor, CalcError().broadcasterProcessor))
						{
							init.DeleteArrayElementAtIndex(i);
						}
					}
				}
				else
				{
					init.DeleteArrayElementAtIndex(i);
					i--;
				}
			}
		}
		Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(expand: true));
		GUIContent content = ((!hasMultipleDifferentValues) ? new GUIContent("[Drag And Drop Or Click Here]") : new GUIContent("Editing Multiple Lists", "Editing multiple lists with different values is not supported."));
		GUI.Label(controlRect, content, CalcError()._VisitorObserver);
		if (hasMultipleDifferentValues)
		{
			return;
		}
		AwakeRules<T>(controlRect, init.CalculateRules<T>);
		if (DefineQueue(controlRect))
		{
			ConcatList(null, typeof(T), null, null, loaddef3: true, null, delegate(UnityEngine.Object o)
			{
				init.CalculateRules<_0021_00210>((IEnumerable<_0021_00210>)(object)new T[1] { o.RevertResolver<T>() });
			});
		}
	}

	internal static void CalculateRules<T>(this SerializedProperty config, IEnumerable<T> second) where T : UnityEngine.Object
	{
		T[] enumerable = (second as T[]) ?? second.ToArray();
		config.DestroyRules(delegate(SerializedProperty sp)
		{
			T[] array = enumerable;
			for (int i = 0; i < array.Length; i++)
			{
				_003C_003Ec__DisplayClass210_1<T> _003C_003Ec__DisplayClass210_ = new _003C_003Ec__DisplayClass210_1<T>();
				_003C_003Ec__DisplayClass210_.e = array[i];
				if (sp.RateRules(_003C_003Ec__DisplayClass210_.MoveConnection) < 0)
				{
					sp.GetArrayElementAtIndex(++sp.arraySize - 1).objectReferenceValue = _003C_003Ec__DisplayClass210_.e;
				}
			}
			sp.serializedObject.ApplyModifiedProperties();
		});
	}

	internal static void MapRules<T>(this SerializedProperty instance, IEnumerable<T> map) where T : UnityEngine.Object
	{
		T[] enumerable = (map as T[]) ?? map.ToArray();
		instance.DestroyRules(delegate(SerializedProperty sp)
		{
			T[] array = enumerable;
			for (int i = 0; i < array.Length; i++)
			{
				_003C_003Ec__DisplayClass212_1<T> _003C_003Ec__DisplayClass212_ = new _003C_003Ec__DisplayClass212_1<T>();
				_003C_003Ec__DisplayClass212_.e = array[i];
				int num = sp.RateRules(_003C_003Ec__DisplayClass212_.CallConnection);
				if (num >= 0)
				{
					sp.DeleteArrayElementAtIndex(num);
				}
			}
			sp.serializedObject.ApplyModifiedProperties();
		});
	}

	internal static void ValidateRules<T>(this SerializedProperty config, bool countsecond, params T[] elements) where T : UnityEngine.Object
	{
		config.CustomizeRules(elements, countsecond);
	}

	internal static void CustomizeRules<T>(this SerializedProperty first, IEnumerable<T> result, bool extractfilter) where T : UnityEngine.Object
	{
		if (!extractfilter)
		{
			first.MapRules(result);
		}
		else
		{
			first.CalculateRules(result);
		}
	}

	internal static int RateRules(this SerializedProperty asset, Func<SerializedProperty, int, bool> attr)
	{
		int num = asset.arraySize - 1;
		while (true)
		{
			if (num < 0)
			{
				return -1;
			}
			SerializedProperty arrayElementAtIndex = asset.GetArrayElementAtIndex(num);
			if (attr(arrayElementAtIndex, num))
			{
				break;
			}
			num--;
		}
		return num;
	}

	internal static void DestroyRules(this SerializedProperty instance, Action<SerializedProperty> pred)
	{
		if (!instance.hasMultipleDifferentValues)
		{
			pred(instance);
			return;
		}
		string propertyPath = instance.propertyPath;
		UnityEngine.Object[] targetObjects = instance.serializedObject.targetObjects;
		for (int i = 0; i < targetObjects.Length; i++)
		{
			SerializedProperty obj = new SerializedObject(targetObjects[i]).FindProperty(propertyPath);
			pred(obj);
		}
	}

	internal static bool GetRules<T>(this T[] key, bool requiresord = false)
	{
		if (key == null || key.Length == 0)
		{
			return true;
		}
		if (!requiresord)
		{
			return false;
		}
		return key.All((T e) => e == null);
	}

	internal static bool CalcRules(this IList last, bool dosecond = false)
	{
		if (last == null || last.Count == 0)
		{
			return true;
		}
		if (dosecond)
		{
			return last.Cast<object>().All((object e) => e == null);
		}
		return false;
	}

	internal static void IncludeRules<T>(this IEnumerable<T> v, Func<T, string> counter)
	{
		foreach (T item in v)
		{
			Debug.Log(counter(item));
		}
	}

	internal static IEnumerable<T> RunRules<T>(this IEnumerable<T> config, Func<T, T, bool> cont)
	{
		HashSet<T> seen = new HashSet<T>();
		foreach (T element in config)
		{
			if (!seen.Any((T seenElement) => cont(element, seenElement)))
			{
				seen.Add(element);
				yield return element;
			}
		}
	}

	internal static IEnumerable<T> CloneRules<T>(this IEnumerable<T> info)
	{
		return info.Where((T x) => x != null);
	}

	internal static IEnumerable<T> LoginRules<T>(this IEnumerable<T> setup, IEnumerable<T> reg, Func<T, T, bool> temp)
	{
		T[] otherArr = (reg as T[]) ?? reg.ToArray();
		foreach (T element in setup)
		{
			if (!otherArr.Any((T otherElement) => temp(element, otherElement)))
			{
				yield return element;
			}
		}
	}

	internal static IEnumerable<T> ReflectRules<T>(this IEnumerator init)
	{
		while (init.MoveNext())
		{
			yield return (T)init.Current;
		}
	}

	internal static void CreateRules<T>(this IEnumerable asset, Action<T> connection)
	{
		foreach (object item in asset)
		{
			connection((T)item);
		}
	}

	internal static TT NewRules<T, TT>(this Dictionary<T, TT> ident, T pred, TT rule)
	{
		if (ident.TryGetValue(pred, out var value))
		{
			return value;
		}
		return rule;
	}

	internal static GameObject PushRules(GameObject spec, Transform cont, bool istemplate = true, bool isconnection2 = true, bool havex3 = false)
	{
		GameObject gameObject = (GameObject)PrefabUtility.InstantiatePrefab(spec, cont);
		if (istemplate)
		{
			PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
		}
		if (isconnection2)
		{
			Transform transform = gameObject.transform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
		}
		if (havex3)
		{
			gameObject.hideFlags |= HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
		}
		return gameObject;
	}

	internal static void ViewRules(GameObject var1)
	{
		if (PrefabUtility.IsPartOfAnyPrefab(var1))
		{
			PrefabUtility.RecordPrefabInstancePropertyModifications(PrefabUtility.GetOutermostPrefabInstanceRoot(var1));
		}
	}

	internal static Dictionary<Transform, Transform> CollectRules(Transform asset, Transform col, bool usefield, params Transform[] transformsToFind)
	{
		Dictionary<Transform, Transform> dictionary = new Dictionary<Transform, Transform>();
		foreach (Transform transform in transformsToFind)
		{
			if (!transform.IsChildOf(asset))
			{
				if (!usefield)
				{
					dictionary.Add(transform, null);
				}
				continue;
			}
			string n = AnimationUtility.CalculateTransformPath(transform, asset);
			Transform transform2 = col.Find(n);
			if (!(transform2 == null && usefield))
			{
				dictionary.Add(transform, transform2);
			}
		}
		return dictionary;
	}

	internal static Dictionary<T, T> ResolveRules<T>(Transform reference, Transform second, bool isutil, params T[] componentsToFind) where T : Component
	{
		Dictionary<T, T> dictionary = new Dictionary<T, T>();
		foreach (T val in componentsToFind)
		{
			if (!val.transform.IsChildOf(reference))
			{
				if (!isutil)
				{
					dictionary.Add(val, null);
				}
				continue;
			}
			string n = AnimationUtility.CalculateTransformPath(val.transform, reference);
			Transform transform = second.Find(n);
			if (transform == null)
			{
				if (!isutil)
				{
					dictionary.Add(val, null);
				}
				continue;
			}
			T[] components = val.GetComponents<T>();
			T[] components2 = transform.GetComponents<T>();
			int num = Array.IndexOf(components, val);
			if (!(num >= components2.Length && isutil))
			{
				dictionary.Add(val, components2[num]);
			}
		}
		return dictionary;
	}

	internal static Component ListRules(this GameObject var1, Type counter)
	{
		Component component = var1.GetComponent(counter);
		if (component == null)
		{
			component = var1.AddComponent(counter);
		}
		return component;
	}

	internal static T VerifyRules<T>(this GameObject v) where T : Component
	{
		T val = v.GetComponent<T>();
		if (val == null)
		{
			val = v.AddComponent<T>();
		}
		return val;
	}

	internal static Type FillRules(string config)
	{
		Type? type = Type.GetType(config);
		if (type == null)
		{
			throw new Exception("Type \"" + config + "\" not found.");
		}
		return type;
	}

	internal static Type WriteRules(string param)
	{
		Type type = ForgotRules(param);
		if (type == null)
		{
			throw new Exception("Type \"" + param + "\" not found.");
		}
		return type;
	}

	internal static Type ForgotRules(string task)
	{
		Type type = Type.GetType(task);
		if (!(type != null))
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			int num = 0;
			while (true)
			{
				if (num < assemblies.Length)
				{
					Type[] types = assemblies[num].GetTypes();
					type = types.FirstOrDefault((Type t) => t.FullName == task);
					if (!(type != null))
					{
						type = types.FirstOrDefault((Type t) => t.Name == task);
						if (type != null)
						{
							break;
						}
						num++;
						continue;
					}
					return type;
				}
				return null;
			}
			return type;
		}
		return type;
	}

	internal static GUIContent StopRules(this SerializedProperty param)
	{
		return new GUIContent(param.displayName, param.tooltip);
	}

	internal static T[] CheckRules<T>(params T[] arg)
	{
		return arg;
	}

	internal static object PrepareRules(this SerializedProperty asset)
	{
		SerializedPropertyType propertyType = asset.propertyType;
		switch (propertyType)
		{
		case SerializedPropertyType.Generic:
		case SerializedPropertyType.Gradient:
		case SerializedPropertyType.ManagedReference:
			Debug.LogWarning("Property type " + propertyType.ToString() + " does not support get value.");
			return null;
		default:
			return null;
		case SerializedPropertyType.LayerMask:
			return asset.intValue;
		case SerializedPropertyType.Float:
			return asset.floatValue;
		case SerializedPropertyType.ArraySize:
			return asset.arraySize;
		case SerializedPropertyType.FixedBufferSize:
			return asset.fixedBufferSize;
		case SerializedPropertyType.ObjectReference:
			return asset.objectReferenceValue;
		case SerializedPropertyType.Color:
			return asset.colorValue;
		case SerializedPropertyType.Vector2Int:
			return asset.vector2IntValue;
		case SerializedPropertyType.AnimationCurve:
			return asset.animationCurveValue;
		case SerializedPropertyType.Quaternion:
			return asset.quaternionValue;
		case SerializedPropertyType.Vector3Int:
			return asset.vector3IntValue;
		case SerializedPropertyType.Vector2:
			return asset.vector2Value;
		case SerializedPropertyType.Enum:
			return asset.enumValueIndex;
		case SerializedPropertyType.Vector4:
			return asset.vector4Value;
		case SerializedPropertyType.Rect:
			return asset.rectValue;
		case SerializedPropertyType.String:
			return asset.stringValue;
		case SerializedPropertyType.Bounds:
			return asset.boundsValue;
		case SerializedPropertyType.BoundsInt:
			return asset.boundsIntValue;
		case SerializedPropertyType.Character:
			return (char)asset.intValue;
		case SerializedPropertyType.Boolean:
			return asset.boolValue;
		case SerializedPropertyType.ExposedReference:
			return asset.exposedReferenceValue;
		case SerializedPropertyType.Integer:
			return asset.intValue;
		case SerializedPropertyType.Vector3:
			return asset.vector3Value;
		case SerializedPropertyType.RectInt:
			return asset.rectIntValue;
		}
	}

	internal static void AssetRules(this SerializedProperty def, object ivk)
	{
		SerializedPropertyType propertyType = def.propertyType;
		switch (propertyType)
		{
		case SerializedPropertyType.Generic:
		case SerializedPropertyType.Gradient:
		case SerializedPropertyType.FixedBufferSize:
		case SerializedPropertyType.ManagedReference:
			Debug.LogWarning("Property type " + propertyType.ToString() + " does not support set value.");
			break;
		case SerializedPropertyType.Vector4:
			def.vector4Value = (Vector4)ivk;
			break;
		case SerializedPropertyType.Vector2:
			def.vector2Value = (Vector2)ivk;
			break;
		case SerializedPropertyType.Enum:
			def.enumValueIndex = (int)ivk;
			break;
		case SerializedPropertyType.RectInt:
			def.rectIntValue = (RectInt)ivk;
			break;
		case SerializedPropertyType.Vector3Int:
			def.vector3IntValue = (Vector3Int)ivk;
			break;
		case SerializedPropertyType.Bounds:
			def.boundsValue = (Bounds)ivk;
			break;
		case SerializedPropertyType.Boolean:
			def.boolValue = (bool)ivk;
			break;
		case SerializedPropertyType.AnimationCurve:
			def.animationCurveValue = (AnimationCurve)ivk;
			break;
		case SerializedPropertyType.Float:
			def.floatValue = (float)ivk;
			break;
		case SerializedPropertyType.Vector2Int:
			def.vector2IntValue = (Vector2Int)ivk;
			break;
		case SerializedPropertyType.LayerMask:
			def.intValue = (int)ivk;
			break;
		case SerializedPropertyType.Vector3:
			def.vector3Value = (Vector3)ivk;
			break;
		case SerializedPropertyType.ExposedReference:
			def.exposedReferenceValue = (UnityEngine.Object)ivk;
			break;
		case SerializedPropertyType.BoundsInt:
			def.boundsIntValue = (BoundsInt)ivk;
			break;
		case SerializedPropertyType.ObjectReference:
			def.objectReferenceValue = (UnityEngine.Object)ivk;
			break;
		case SerializedPropertyType.Quaternion:
			def.quaternionValue = (Quaternion)ivk;
			break;
		case SerializedPropertyType.Character:
			def.intValue = (char)ivk;
			break;
		case SerializedPropertyType.Color:
			def.colorValue = (Color)ivk;
			break;
		case SerializedPropertyType.ArraySize:
			def.arraySize = (int)ivk;
			break;
		case SerializedPropertyType.String:
			def.stringValue = (string)ivk;
			break;
		case SerializedPropertyType.Rect:
			def.rectValue = (Rect)ivk;
			break;
		case SerializedPropertyType.Integer:
			def.intValue = (int)ivk;
			break;
		}
	}

	internal static bool UpdateRules(string last, Func<string, bool> ivk, out string rule)
	{
		rule = SortRules(last, ivk);
		return rule != last;
	}

	internal static string ChangeRules(string setup, IEnumerable<string> ord)
	{
		HashSet<string> m_ConsumerServer = (ord as HashSet<string>) ?? new HashSet<string>(ord);
		return SortRules(setup, (string s) => !m_ConsumerServer.Contains(s));
	}

	internal static string SortRules(string instance, Func<string, bool> cfg)
	{
		if (!cfg(instance))
		{
			if (!LogoutRules(instance, out var i))
			{
				i = 1;
			}
			string arg;
			for (arg = RegisterRules(instance); !cfg($"{arg} {i}"); i++)
			{
			}
			return $"{arg} {i}";
		}
		return instance;
	}

	internal static string RegisterRules(string ident)
	{
		return Regex.Replace(ident, "(?=.*) \\d+$", string.Empty);
	}

	internal static bool LogoutRules(string info, out int b)
	{
		b = 0;
		if (!string.IsNullOrWhiteSpace(info))
		{
			Match match = Regex.Match(info, "(?=.*)(\\d+)$");
			if (!match.Success)
			{
				return false;
			}
			b = int.Parse(match.Groups[1].Value);
			return true;
		}
		return false;
	}

	internal static void PatchRules()
	{
		EditorWindow[] array = Resources.FindObjectsOfTypeAll<EditorWindow>();
		foreach (EditorWindow editorWindow in array)
		{
			if (!(editorWindow == null))
			{
				editorWindow.maximized = false;
			}
		}
	}

	internal static EditorWindow InterruptRules(string v)
	{
		if (structProperty == null)
		{
			structProperty = new Dictionary<string, Type>();
		}
		if (!structProperty.ContainsKey(v))
		{
			Type type = ForgotRules(v);
			if (type == null)
			{
				return null;
			}
			structProperty.Add(v, type);
		}
		return Resources.FindObjectsOfTypeAll(structProperty[v]).FirstOrDefault() as EditorWindow;
	}

	internal static bool ManageRules(string i, out EditorWindow pol)
	{
		pol = InterruptRules(i);
		return pol != null;
	}

	internal static void PrintRules(string config, bool isattr = false)
	{
		if (!ManageRules(config, out var pol))
		{
			return;
		}
		EditorWindow focusedWindow = EditorWindow.focusedWindow;
		if (!(focusedWindow == pol))
		{
			pol.Focus();
			if (isattr)
			{
				focusedWindow.Focus();
			}
		}
	}

	internal static void SearchRules(string task, object cfg)
	{
		if (serviceProperty == null)
		{
			serviceProperty = new Dictionary<string, object>();
		}
		serviceProperty[task] = cfg;
	}

	internal static object RevertRules(string first)
	{
		if (serviceProperty != null)
		{
			if (serviceProperty.TryGetValue(first, out var value))
			{
				return value;
			}
			return null;
		}
		return null;
	}

	internal static T OrderQueue<T>(string last, T attr)
	{
		object value;
		if (serviceProperty != null)
		{
			return (T)((!serviceProperty.TryGetValue(last, out value)) ? ((object)attr) : value);
		}
		return attr;
	}

	internal static void CompareQueue(string v, object vis, EventType dic)
	{
		SetQueue(v, vis, dic, Event.current.type);
	}

	internal static void SetQueue(string init, object attr, EventType proc, EventType var12)
	{
		if (var12 == proc)
		{
			SearchRules(init, attr);
		}
	}

	internal static float PostQueue(Enum key, GUIStyle second = null)
	{
		return SetupQueue(key.ToString(), second);
	}

	internal static float SetupQueue(string res, GUIStyle cust = null)
	{
		if (cust == null)
		{
			cust = GUI.skin.label;
		}
		return cust.CalcSize(res.CreateResolver()).x;
	}

	internal static void EnableQueue()
	{
		if (Event.current.type == EventType.Repaint)
		{
			globalProperty = true;
			PublishQueue();
		}
	}

	internal static void PublishQueue()
	{
		stateProperty.Clear();
	}

	internal static void PopQueue()
	{
		if (Event.current.type == EventType.Repaint)
		{
			globalProperty = false;
			while (stateProperty.Count > 0)
			{
				var (screenRect, mouse) = stateProperty.Pop();
				EditorGUIUtility.AddCursorRect(GUIUtility.ScreenToGUIRect(screenRect), mouse);
			}
		}
	}

	internal static bool ComputeQueue(string first, Color? cust = null)
	{
		return MoveQueue(new GUIContent(first), cust);
	}

	internal static bool MoveQueue(GUIContent item, Color? cont = null)
	{
		if (!cont.HasValue)
		{
			cont = m_MapperProcessor;
		}
		using (new TemplateThread(TemplateThread.ColoringType.BG, Color.clear))
		{
			using (new TemplateThread(TemplateThread.ColoringType.FG, cont.Value))
			{
				bool result = RestartQueue(item, CalcError()._IteratorProcessor, GUILayout.ExpandWidth(expand: false));
				ConcatQueue(cont);
				return result;
			}
		}
	}

	internal static void ConcatQueue(Color? v = null)
	{
		if (!v.HasValue)
		{
			v = new Color(0.3f, 0.7f, 1f);
		}
		if (Event.current.type == EventType.Repaint)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			Vector2 mousePosition = Event.current.mousePosition;
			if (lastRect.Contains(mousePosition))
			{
				EditorGUI.DrawRect(new Rect(lastRect.x, lastRect.yMax - 1f, lastRect.width, 1f), v.Value);
			}
			EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.Link);
		}
	}

	internal static bool CallQueue(GUIContent setup, float cust = -1f, float consumer = -1f)
	{
		if (cust == -1f)
		{
			cust = EditorGUIUtility.singleLineHeight;
		}
		if (consumer == -1f)
		{
			consumer = EditorGUIUtility.singleLineHeight;
		}
		bool result = GUILayout.Button(setup, CalcError().broadcasterProcessor, GUILayout.Width(cust), GUILayout.Height(consumer));
		StartQueue();
		return result;
	}

	internal static bool CancelQueue(Rect var1, string counter, GUIStyle control = null)
	{
		return QueryQueue(var1, new GUIContent(counter), control);
	}

	internal static bool CountQueue(string info, GUIStyle result = null, params GUILayoutOption[] options)
	{
		return RestartQueue(new GUIContent(info), result, options);
	}

	internal static bool DisableQueue(string res, params GUILayoutOption[] options)
	{
		return RestartQueue(new GUIContent(res), null, options);
	}

	internal static bool InsertQueue(GUIContent var1, params GUILayoutOption[] options)
	{
		return RestartQueue(var1, null, options);
	}

	internal static bool RestartQueue(GUIContent item, GUIStyle selection = null, params GUILayoutOption[] options)
	{
		return ExcludeQueue(isfirst: false, item, selection, options);
	}

	internal static bool QueryQueue(Rect item, GUIContent second, GUIStyle helper = null)
	{
		if (helper == null)
		{
			helper = GUI.skin.button;
		}
		bool result = GUI.Button(item, second, helper);
		StartQueue(item);
		return result;
	}

	internal static bool AddQueue(bool extractparam, string vis, GUIStyle control = null, params GUILayoutOption[] options)
	{
		return ExcludeQueue(extractparam, new GUIContent(vis), control, options);
	}

	internal static bool InvokeQueue(bool isident, string map, params GUILayoutOption[] options)
	{
		return ExcludeQueue(isident, new GUIContent(map), null, options);
	}

	internal static bool FindQueue(bool isident, GUIContent reg, params GUILayoutOption[] options)
	{
		return ExcludeQueue(isident, reg, null, options);
	}

	internal static bool ExcludeQueue(bool isfirst, GUIContent visitor, GUIStyle consumer = null, params GUILayoutOption[] options)
	{
		if (consumer == null)
		{
			consumer = GUI.skin.button;
		}
		bool result = GUILayout.Toggle(isfirst, visitor, consumer, options);
		StartQueue();
		return result;
	}

	internal static bool InitQueue(bool valuereguired, string ord, GUIStyle state = null, params GUILayoutOption[] options)
	{
		return VisitQueue(valuereguired, new GUIContent(ord), state, options);
	}

	internal static bool VisitQueue(bool writevalue, GUIContent cust, GUIStyle tag = null, params GUILayoutOption[] options)
	{
		if (tag == null)
		{
			tag = GUI.skin.button;
		}
		bool flag = GUILayout.Toggle(writevalue, cust, tag, options);
		StartQueue();
		return writevalue != flag;
	}

	internal static bool DefineQueue(Rect v = default(Rect))
	{
		if (v == default(Rect))
		{
			v = GUILayoutUtility.GetLastRect();
		}
		StartQueue(v);
		Event current = Event.current;
		if (current.type != EventType.MouseDown || current.button != 0)
		{
			return false;
		}
		return v.Contains(current.mousePosition);
	}

	internal static void StartQueue(Rect spec = default(Rect), bool containsvisitor = false)
	{
		if (Event.current.type == EventType.Repaint)
		{
			if (spec == default(Rect))
			{
				spec = GUILayoutUtility.GetLastRect();
			}
			AwakeQueue(spec, MouseCursor.Link, containsvisitor);
		}
	}

	internal static bool ReadQueue(Rect param = default(Rect))
	{
		bool selection;
		return RemoveQueue(param, out selection);
	}

	internal static bool SelectQueue(out bool init)
	{
		return RemoveQueue(default(Rect), out init);
	}

	internal static bool RemoveQueue(Rect res, out bool selection)
	{
		Event current = Event.current;
		selection = false;
		if (current.type == EventType.MouseDown && current.button == 0)
		{
			selection = current.clickCount == 2;
			if (res == default(Rect))
			{
				res = GUILayoutUtility.GetLastRect();
			}
			if (res.Contains(current.mousePosition))
			{
				current.Use();
				return true;
			}
		}
		return false;
	}

	internal static bool InstantiateQueue(Rect reference = default(Rect))
	{
		Event current = Event.current;
		if (current.type == EventType.ContextClick)
		{
			if (reference == default(Rect))
			{
				reference = GUILayoutUtility.GetLastRect();
			}
			if (reference.Contains(current.mousePosition))
			{
				current.Use();
				return true;
			}
		}
		return false;
	}

	internal static void AwakeQueue(Rect ident, MouseCursor pred, bool hascontrol = false)
	{
		if (!GUI.enabled && !hascontrol)
		{
			return;
		}
		if (globalProperty)
		{
			if (m_ProxyProperty)
			{
				ident.y += 46f;
			}
			stateProperty.Push((GUIUtility.GUIToScreenRect(ident), pred));
		}
		else if (Event.current.type == EventType.Repaint)
		{
			EditorGUIUtility.AddCursorRect(ident, pred);
		}
	}

	internal static void ResetQueue(Rect config, Action<float> col)
	{
		Event current = Event.current;
		if (current.type == EventType.ScrollWheel)
		{
			if (config == default(Rect))
			{
				config = GUILayoutUtility.GetLastRect();
			}
			if (config.Contains(current.mousePosition))
			{
				col(current.delta.y);
			}
		}
	}

	internal static void FlushQueue(Rect res, string cfg, float dic = 0f, float pol2 = 0f, bool stripresult3 = true, GUIStyle cust4 = null)
	{
		ConnectQueue(res, cfg, overridethird: true, dic, pol2, stripresult3, cust4);
	}

	internal static void ConnectQueue(Rect key, string caller, bool overridethird = true, float second2 = 0f, float pred3 = 0f, bool isparam4 = true, GUIStyle pol5 = null)
	{
		if (overridethird && !(key.width <= second2 + pred3))
		{
			if (!isparam4)
			{
				key.x -= pred3 + 2.5f;
			}
			else
			{
				key.x += pred3 + 2.5f;
			}
			GUI.Label(key, caller, pol5 ?? ((!isparam4) ? CalcError().algoObserver : CalcError().annotationObserver));
		}
	}

	internal static void CalculateQueue(string ident, float col = 0f, float template = 0f, bool isreg2 = true)
	{
		ConnectQueue(GUILayoutUtility.GetLastRect(), ident, overridethird: true, col, template, isreg2);
	}

	internal static void TestQueue(string res, bool iscfg = true, float tag = 0f, float def2 = 0f, bool isivk3 = true)
	{
		ConnectQueue(GUILayoutUtility.GetLastRect(), res, iscfg, tag, def2, isivk3);
	}

	internal static void MapQueue(int idx_var1 = 2, int contHigh = 10, int state_start = 0)
	{
		Rect rect = ((state_start <= 0) ? EditorGUILayout.GetControlRect(GUILayout.Height(idx_var1 + contHigh)) : EditorGUILayout.GetControlRect(GUILayout.Height(idx_var1 + contHigh), GUILayout.MaxWidth(state_start)));
		rect.height = idx_var1;
		rect.y += (float)contHigh / 2f;
		rect.x -= 2f;
		rect.width += 6f;
		ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#595959" : "#858585", out var color);
		EditorGUI.DrawRect(rect, color);
	}

	internal static bool ValidateQueue(Rect def, int removeTOKENAt)
	{
		if (GUIUtility.hotControl != removeTOKENAt)
		{
			Event current = Event.current;
			if (current.type == EventType.MouseDown && def.Contains(current.mousePosition))
			{
				GUIUtility.hotControl = removeTOKENAt;
				current.Use();
			}
			return false;
		}
		return true;
	}

	public static Rect[] CustomizeQueue(Rect ident, int visitorPosition)
	{
		Rect[] array = new Rect[visitorPosition];
		if (visitorPosition <= 0)
		{
			return array;
		}
		int num = Mathf.CeilToInt(Mathf.Sqrt(visitorPosition));
		int num2 = Mathf.CeilToInt((float)visitorPosition / (float)num);
		float num3 = ident.width / (float)num;
		float num4 = ident.height / (float)num2;
		for (int i = 0; i < visitorPosition; i++)
		{
			int num5 = i / num;
			int num6 = i % num;
			float x = ident.x + (float)num6 * num3;
			float y = ident.y + (float)num5 * num4;
			array[i] = new Rect(x, y, num3, num4);
		}
		return array;
	}

	internal static void RateQueue()
	{
		GUILayout.Label(GUIContent.none, GUILayout.Width(EditorGUIUtility.singleLineHeight));
	}

	[SpecialName]
	private static MethodInfo CustomizeError()
	{
		return m_TaskProperty ?? (m_TaskProperty = typeof(EditorGUI).GetMethod("TextFieldDropDown", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[4]
		{
			typeof(Rect),
			typeof(GUIContent),
			typeof(string),
			typeof(string[])
		}, null));
	}

	internal static string DestroyQueue(string res, string second, string[] dir, params GUILayoutOption[] layoutOptions)
	{
		return GetQueue(new GUIContent(res), second, dir, layoutOptions);
	}

	internal static string GetQueue(GUIContent v, string b, string[] template, params GUILayoutOption[] layoutOptins)
	{
		if (CustomizeError() != null)
		{
			Rect rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.textField, layoutOptins);
			return (string)CustomizeError().Invoke(null, new object[4] { rect, v, b, template });
		}
		return b;
	}

	internal static string CalcQueue(Rect value, string map, string dic, string[] asset2)
	{
		if (CustomizeError() != null)
		{
			return (string)CustomizeError().Invoke(null, new object[4]
			{
				value,
				new GUIContent(map),
				dic,
				asset2
			});
		}
		return dic;
	}

	internal static void IncludeQueue(UnityEngine.Object instance)
	{
		using (new EditorGUI.DisabledScope(instance == null))
		{
			if (CallQueue(new GUIContent(DestroyError()._FactoryProcessor)
			{
				tooltip = "Ping in Project window"
			}, 10f))
			{
				EditorGUIUtility.PingObject(instance);
			}
		}
	}

	internal static void RunQueue(Transform asset, PrototypePolicy caller)
	{
		if (asset == null)
		{
			return;
		}
		bool num = Tools.current == Tool.Transform;
		bool flag = num || Tools.current == Tool.Move;
		bool flag2 = num || (!flag && Tools.current == Tool.Rotate);
		bool islast = num || (!flag && !flag2 && Tools.current == Tool.Scale);
		PivotRotation pivotRotation = Tools.pivotRotation;
		bool num2 = caller.positionControl.IncludeHelper(flag);
		bool flag3 = caller.rotationControl.IncludeHelper(flag2);
		bool flag4 = caller.scaleControl.IncludeHelper(islast);
		if (num2)
		{
			Vector3 position = asset.position;
			bool num3 = caller.positionControl.RunHelper(pivotRotation) == PivotRotation.Global;
			EditorGUI.BeginChangeCheck();
			position = (num3 ? Handles.PositionHandle(position, Quaternion.identity) : Handles.PositionHandle(position, asset.localRotation));
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(asset, "Custom Tool Control");
				asset.position = position;
			}
		}
		if (flag3)
		{
			bool outputres = caller.rotationControl.RunHelper(pivotRotation) == PivotRotation.Global;
			CloneQueue(asset, caller.rotationControl.axis, outputres);
		}
		if (flag4)
		{
			Vector3 localScale = asset.localScale;
			Vector3 position2 = asset.position;
			EditorGUI.BeginChangeCheck();
			localScale = Handles.ScaleHandle(localScale, position2, asset.rotation, HandleUtility.GetHandleSize(position2));
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(asset, "Custom Tool Control");
				asset.localScale = new Vector3(Mathf.Max(localScale.x, 0.0001f), Mathf.Max(localScale.y, 0.0001f), Mathf.Max(localScale.z, 0.0001f));
			}
		}
	}

	internal static void CloneQueue(Transform param, Axis attr = Axis.X | Axis.Y | Axis.Z, bool outputres = true)
	{
		if (attr != (Axis.X | Axis.Y | Axis.Z))
		{
			if (attr.HasFlag(Axis.X))
			{
				ReflectQueue(param, outputres ? Vector3.right : param.right);
			}
			if (attr.HasFlag(Axis.Y))
			{
				ReflectQueue(param, outputres ? Vector3.up : param.up);
			}
			if (attr.HasFlag(Axis.Z))
			{
				ReflectQueue(param, outputres ? Vector3.forward : param.forward);
			}
			return;
		}
		if (!outputres)
		{
			EditorGUI.BeginChangeCheck();
			Quaternion rotation = Handles.RotationHandle(param.rotation, param.position);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(param, "Custom Tool Control");
				param.rotation = rotation;
			}
			return;
		}
		int hotControl = GUIUtility.hotControl;
		bool flag = Event.current.type == EventType.MouseDrag;
		EditorGUI.BeginChangeCheck();
		Quaternion quaternion = Handles.RotationHandle(Quaternion.identity, param.position);
		if (hotControl != GUIUtility.hotControl)
		{
			processProperty = param.rotation;
			m_ProducerProperty = true;
		}
		else if (hotControl == 0)
		{
			m_ProducerProperty = false;
		}
		if (EditorGUI.EndChangeCheck() && m_ProducerProperty && flag)
		{
			Undo.RecordObject(param, "Custom Tool Control");
			param.rotation = quaternion * processProperty;
		}
	}

	internal static void LoginQueue(Vector3 info, Vector3 token, Axis serv = Axis.X | Axis.Y | Axis.Z)
	{
	}

	internal static void ReflectQueue(Transform first, Vector3 vis)
	{
		EditorGUI.BeginChangeCheck();
		Quaternion rotation = DeleteQueue(first.position, first.rotation, vis);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(first, "Custom Tool Control");
			first.rotation = rotation;
		}
	}

	internal static Quaternion DeleteQueue(Vector3 instance, Quaternion ord, Vector3 c)
	{
		Handles.color = configurationProperty;
		return Handles.Disc(ord, instance, c, HandleUtility.GetHandleSize(instance), cutoffPlane: false, 0f);
	}

	internal static void CreateQueue(string setup, Vector3 map, float comp = 0f, GUIStyle config2 = null)
	{
		if (config2 == null)
		{
			config2 = EditorStyles.boldLabel;
		}
		GUIContent content = new GUIContent(setup);
		float x = config2.CalcSize(content).x;
		Vector3 vector = HandleUtility.WorldToGUIPointWithDepth(map);
		if (!(vector.z <= 0f))
		{
			Vector3 vector2 = vector - new Vector3(x * 0.5f, comp * 500f * 1f / vector.z + vector.z / (vector.z * 0.05f));
			Handles.BeginGUI();
			GUI.Label(new Rect(vector2, new Vector2(x, 20f)), content, config2);
			Handles.EndGUI();
		}
	}

	internal static float NewQueue(Quaternion key, Vector3 pol, float util, bool asset2reguired = true, float init3 = 1f)
	{
		float num = 90f;
		Vector3[] array = new Vector3[4]
		{
			key * Vector3.right,
			key * Vector3.forward,
			key * -Vector3.right,
			key * -Vector3.forward
		};
		Vector3 vector;
		if (!Camera.current.orthographic)
		{
			vector = pol - Matrix4x4.Inverse(Handles.matrix).MultiplyPoint(Camera.current.transform.position);
			float sqrMagnitude = vector.sqrMagnitude;
			float num2 = util * util;
			float num3 = num2 * num2 / sqrMagnitude;
			num = ((!((double)(num3 / num2) >= 1.0)) ? (Mathf.Atan2(Mathf.Sqrt(num2 - num3), Mathf.Sqrt(num3)) * 57.29578f) : (-1000f));
		}
		else
		{
			vector = Camera.current.transform.forward;
		}
		Color color = Handles.color;
		for (int i = 0; i < 4; i++)
		{
			int controlID = GUIUtility.GetControlID(iteratorProperty, FocusType.Passive);
			float num4 = Vector3.Angle(array[i], -vector);
			if ((!((double)num4 <= 5.0) && (double)num4 < 175.0) || GUIUtility.hotControl == controlID)
			{
				float a = ((!((double)num4 <= (double)num + 5.0)) ? Mathf.Clamp01(0.2f * color.a * 2f) : Mathf.Clamp01(color.a * 2f));
				Color color2 = new Color(color.r, color.g, color.b, a);
				Handles.color = ((QualitySettings.activeColorSpace != ColorSpace.Linear) ? color2 : color2.linear);
				Vector3 position = pol + util * array[i];
				bool changed = GUI.changed;
				GUI.changed = false;
				Vector3 a2 = Handles.Slider(controlID, position, array[i], HandleUtility.GetHandleSize(position) * 0.05f * init3, Handles.DotHandleCap, 0f);
				if (GUI.changed)
				{
					util = Vector3.Distance(a2, pol);
				}
				GUI.changed |= changed;
				Handles.color = color;
			}
			if (asset2reguired)
			{
				Handles.DrawWireArc(pol, array[i], array[(i + 1) % 4], 360f, util);
			}
		}
		return util;
	}

	internal static GUIContent PushQueue(string config, string selection = null)
	{
		return new GUIContent(EditorGUIUtility.IconContent(config))
		{
			tooltip = selection
		};
	}

	[SpecialName]
	internal static WatcherProcessor DestroyError()
	{
		return _InitializerProcessor ?? (_InitializerProcessor = new WatcherProcessor());
	}

	[SpecialName]
	internal static BaseProcessor CalcError()
	{
		return _DefinitionProcessor ?? (_DefinitionProcessor = new BaseProcessor());
	}

	internal static bool ViewQueue(EventCommands setup, string result = "", bool consumerinstall = true)
	{
		if (!string.IsNullOrEmpty(result) && GUI.GetNameOfFocusedControl() != result)
		{
			return false;
		}
		Event current = Event.current;
		if (current.type == EventType.ExecuteCommand || current.type == EventType.ValidateCommand)
		{
			bool num = setup.ToString() == current.commandName;
			if (num && consumerinstall)
			{
				current.Use();
			}
			return num;
		}
		return false;
	}

	internal static bool CollectQueue(KeyCode var1, string visitor = "", bool iscomp = true)
	{
		if (!string.IsNullOrEmpty(visitor) && GUI.GetNameOfFocusedControl() != visitor)
		{
			return false;
		}
		Event current = Event.current;
		bool num = current.type == EventType.KeyDown && current.keyCode == var1;
		if (num && iscomp)
		{
			current.Use();
		}
		return num;
	}

	internal static bool ResolveQueue(string ident = "", bool readattr = true)
	{
		if (!CollectQueue(KeyCode.Return, ident, readattr))
		{
			return CollectQueue(KeyCode.KeypadEnter, ident, readattr);
		}
		return true;
	}

	internal static bool ListQueue(string setup = "", bool addpol = true)
	{
		return CollectQueue(KeyCode.Escape, setup, addpol);
	}

	internal static bool VerifyQueue(string last = "", bool havesecond = true)
	{
		if (!ViewQueue(EventCommands.SoftDelete, last, havesecond))
		{
			return ViewQueue(EventCommands.Delete, last, havesecond);
		}
		return true;
	}

	internal static bool FillQueue(string config = "", Action reg = null, Action temp = null)
	{
		if (!ResolveQueue(config))
		{
			if (!ListQueue(config))
			{
				return false;
			}
			temp?.Invoke();
			return true;
		}
		reg?.Invoke();
		return true;
	}

	internal static bool WriteQueue(string task, Action counter = null, Action serv = null)
	{
		if (FillQueue(task, counter, serv))
		{
			GUI.FocusControl(null);
			return true;
		}
		return false;
	}

	private static void ForgotQueue(Vector3 last, Vector3 pred, Vector3 c, int columnvis2 = -1, Color? selection3 = null)
	{
		if (!selection3.HasValue)
		{
			selection3 = Handles.color;
		}
		if (columnvis2 != -1 && GUIUtility.hotControl == columnvis2)
		{
			selection3 = Color.yellow;
		}
		if (regProcessor == null)
		{
			regProcessor = PrepareQueue();
		}
		if (testsProcessor == null)
		{
			testsProcessor = AssetQueue();
		}
		UpdateQueue(testsProcessor);
		float num = Vector3.Distance(last, pred);
		Vector3 normalized = (pred - last).normalized;
		Matrix4x4 matrix = Matrix4x4.TRS(last, Quaternion.LookRotation(normalized, c), new Vector3(num, num, num));
		testsProcessor.SetColor(_PropertyProcessor, selection3.Value);
		testsProcessor.SetPass(0);
		Graphics.DrawMeshNow(regProcessor, matrix);
	}

	private static void StopQueue(Vector3 config, Quaternion cfg, float temp, int length_map2 = -1, Color? ident3 = null)
	{
		CheckQueue(Matrix4x4.TRS(config, cfg, new Vector3(temp, temp, temp)), length_map2, ident3);
	}

	private static void CheckQueue(Matrix4x4 config, int visitor = -1, Color? dic = null)
	{
		if (!dic.HasValue)
		{
			dic = Handles.color;
		}
		if (visitor != -1 && GUIUtility.hotControl == visitor)
		{
			dic = Color.yellow;
		}
		if (regProcessor == null)
		{
			regProcessor = PrepareQueue();
		}
		if (testsProcessor == null)
		{
			testsProcessor = AssetQueue();
		}
		UpdateQueue(testsProcessor);
		testsProcessor.SetColor(_PropertyProcessor, dic.Value);
		testsProcessor.SetPass(0);
		Graphics.DrawMeshNow(regProcessor, config);
	}

	private static Mesh PrepareQueue()
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

	private static Material AssetQueue()
	{
		Material material = new Material(Shader.Find("UI/Unlit/Text"));
		UpdateQueue(material);
		return material;
	}

	private static void UpdateQueue(Material i)
	{
		i.hideFlags = HideFlags.DontSave;
		i.SetInt("_Cull", 2);
		i.SetInt("_ZWrite", 0);
		i.SetInt("_ZTest", 8);
	}

	internal static void ChangeQueue(InterpreterObserver key)
	{
		Event current = Event.current;
		key.m_StrategyObserver?.Invoke(key);
		int stubObserver = key._StubObserver;
		switch (current.GetTypeForControl(stubObserver))
		{
		case EventType.MouseDown:
			if (HandleUtility.nearestControl == stubObserver && current.button == 0)
			{
				key.m_ReaderObserver();
				current.Use();
			}
			break;
		case EventType.Layout:
		{
			float[] array = key.StopError();
			foreach (float distance in array)
			{
				HandleUtility.AddControl(stubObserver, distance);
			}
			break;
		}
		}
	}

	internal static Rect SortQueue(this SceneView ident)
	{
		return RegisterQueue(GUIUtility.ScreenToGUIRect(ident.position));
	}

	internal static Rect RegisterQueue(Rect ident)
	{
		if (!m_ProxyProperty)
		{
			ident.y += 40f;
		}
		ident.height -= (m_ProxyProperty ? 27f : 21f);
		return ident;
	}

	internal static void LogoutQueue(VRCAvatarDescriptor ident, IEnumerable<Vector3> cfg, bool calcserv, out Vector3 ivk2, out Quaternion last3, Vector3? counter4 = null, Vector3? item5 = null)
	{
		PatchQueue(ident, cfg.Aggregate(Vector3.zero, (Vector3 current, Vector3 vec) => current + vec) / cfg.Count(), calcserv, out ivk2, out last3, counter4, item5);
	}

	internal static void PatchQueue(VRCAvatarDescriptor setup, Vector3 visitor, bool isdir, out Vector3 caller2, out Quaternion visitor3, Vector3? x4 = null, Vector3? t5 = null)
	{
		setup.GetComponent<Animator>().InterruptQueue(out var b, isdir);
		Vector3 vector = b * Vector3.up;
		Vector3 vector2 = b * Vector3.right * (isdir ? 1 : (-1));
		visitor3 = b;
		float num = setup.IncludeList();
		float num2 = num * 0.02f;
		caller2 = visitor + vector * num2 + vector2 * num2;
		if (x4.HasValue)
		{
			if (!isdir)
			{
				x4 = new Vector3(0f - x4.Value.x, x4.Value.y, x4.Value.z);
			}
			Quaternion value = visitor3;
			Vector3? vector3 = x4;
			x4 = value * vector3 * num * 0.02f;
			caller2 += x4.Value;
		}
		if (t5.HasValue)
		{
			if (!isdir)
			{
				t5 = new Vector3(t5.Value.x, 0f - t5.Value.y, t5.Value.z);
			}
			visitor3 *= Quaternion.Euler(t5.Value);
		}
		if (!isdir)
		{
			visitor3 *= Quaternion.Euler(0f, 180f, 0f);
		}
	}

	internal static bool InterruptQueue(this Animator asset, out Quaternion b, bool ispool = true)
	{
		b = ((!ispool) ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.Euler(0f, 0f, -90f));
		try
		{
			Transform boneTransform = asset.GetBoneTransform(ispool ? HumanBodyBones.RightLowerArm : HumanBodyBones.LeftLowerArm);
			Vector3 vector = asset.GetBoneTransform((!ispool) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand).position - boneTransform.position;
			while (true)
			{
				Vector3 normalized = vector.normalized;
				float z = Vector3.SignedAngle(b * Vector3.up, normalized, asset.transform.forward);
				b *= Quaternion.Euler(0f, 0f, z);
			}
		}
		catch
		{
		}
		return false;
	}

	internal static bool ManageQueue(this Animator init, out Dictionary<HumanBodyBones, Transform> pol, params HumanBodyBones[] bones)
	{
		bool v;
		return init.SearchQueue(iscfg: true, usecomp: false, null, out v, out pol, bones);
	}

	internal static bool PrintQueue(this Animator ident, bool removeconnection, out Dictionary<HumanBodyBones, Transform> temp, params HumanBodyBones[] bones)
	{
		bool v;
		return ident.SearchQueue(removeconnection, usecomp: false, null, out v, out temp, bones);
	}

	internal static bool SearchQueue(this Animator key, bool iscfg, bool usecomp, string spec2, out bool v3, out Dictionary<HumanBodyBones, Transform> info4, params HumanBodyBones[] bones)
	{
		v3 = false;
		info4 = new Dictionary<HumanBodyBones, Transform>();
		bool flag = true;
		HumanBodyBones[] array = bones;
		foreach (HumanBodyBones humanBodyBones in array)
		{
			Transform transform = ((!iscfg) ? key.GetBoneTransform(humanBodyBones) : key.RevertQueue(humanBodyBones));
			info4.Add(humanBodyBones, transform);
			if (transform == null)
			{
				flag = false;
			}
		}
		if (!flag && usecomp)
		{
			StringBuilder stringBuilder = new StringBuilder();
			array = bones;
			for (int i = 0; i < array.Length; i++)
			{
				HumanBodyBones connection = array[i];
				if (!key.OrderList(connection))
				{
					stringBuilder.AppendLine(connection.ToString());
				}
			}
			if (spec2 == null)
			{
				spec2 = "The following bones are missing from the avatar's rig:";
			}
			string message = $"{spec2}\n{stringBuilder}\n\nContinue anyway?";
			v3 = !EditorUtility.DisplayDialog("Missing Bones", message, "Continue", "Cancel");
		}
		return flag;
	}

	internal static Transform RevertQueue(this Animator ident, HumanBodyBones attr, HumanBodyBones? role = null)
	{
		int num = (int)attr;
		Transform transform = ident.GetBoneTransform(attr);
		if (!transform)
		{
			if (!attr.CompareList() || num % 3 == 0)
			{
				if (attr.SetList())
				{
					num -= 14;
					transform = ident.GetBoneTransform((HumanBodyBones)num);
				}
			}
			else
			{
				transform = ident.RevertQueue((HumanBodyBones)(--num));
			}
			if (transform == null && role.HasValue)
			{
				transform = ident.RevertQueue(role.Value);
			}
			return transform;
		}
		return transform;
	}

	internal static bool OrderList(this Animator var1, HumanBodyBones connection)
	{
		return var1.GetBoneTransform(connection) != null;
	}

	internal static bool CompareList(this HumanBodyBones asset)
	{
		if (asset < HumanBodyBones.LeftThumbProximal)
		{
			return false;
		}
		return asset <= HumanBodyBones.RightLittleDistal;
	}

	internal static bool SetList(this HumanBodyBones var1)
	{
		if (var1 == HumanBodyBones.LeftToes)
		{
			return true;
		}
		return var1 == HumanBodyBones.RightToes;
	}

	internal static bool PostList(this HumanBodyBones param)
	{
		return param.ToString().StartsWith("Left");
	}

	internal static bool SetupList(this HumanBodyBones last)
	{
		return last.ToString().StartsWith("Right");
	}

	internal static bool EnableList(this HumanBodyBones last)
	{
		string text = last.ToString();
		if (text.StartsWith("Left"))
		{
			return true;
		}
		return text.StartsWith("Right");
	}

	internal static bool PublishList(int infoamount, out int ord)
	{
		ord = PopList(infoamount);
		return ord != infoamount;
	}

	internal static int PopList(int tasklow)
	{
		if (tasklow <= 0)
		{
			return tasklow;
		}
		int num = ((!tasklow.ValidateResolver(11, 22) && !tasklow.ValidateResolver(1, 6)) ? (tasklow.ValidateResolver(24, 38) ? 15 : (tasklow.ValidateResolver(39, 53) ? (-15) : 0)) : ((tasklow % 2 == 1) ? 1 : (-1)));
		return tasklow + num;
	}

	internal static bool MoveList(this HumanBodyBones info, out HumanBodyBones map)
	{
		if (!PublishList((int)info, out var ord))
		{
			map = info;
			return false;
		}
		map = (HumanBodyBones)ord;
		return true;
	}

	internal static void ConcatList(UnityEngine.Object info, Type pol, UnityEngine.Object role = null, SerializedProperty pol2 = null, bool loaddef3 = true, List<int> task4 = null, Action<UnityEngine.Object> result5 = null, Action<UnityEngine.Object> config6 = null, bool rejectresult7 = true)
	{
		if (observerProcessor == null)
		{
			observerProcessor = Type.GetType("UnityEditor.ObjectSelector, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		}
		if (processorProcessor == null)
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
			processorProcessor = observerProcessor.GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic, null, types, null);
			m_ServerProcessor = processorProcessor != null;
			if (!m_ServerProcessor)
			{
				Type[] types2 = new Type[3]
				{
					typeof(UnityEngine.Object),
					typeof(Type),
					typeof(SerializedProperty)
				}.Concat(second).ToArray();
				processorProcessor = observerProcessor.GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic, null, types2, null);
			}
		}
		EditorWindow window = EditorWindow.GetWindow(observerProcessor);
		object[] second2 = new object[4] { loaddef3, task4, result5, config6 };
		second2 = ((!m_ServerProcessor) ? new object[3] { info, pol, pol2 }.Concat(second2).ToArray() : new object[3] { info, pol, role }.Concat(second2).Concat(new object[1] { rejectresult7 }).ToArray());
		processorProcessor.Invoke(window, second2);
	}

	internal static void CallList(Type asset, Type visitor)
	{
		if (!threadProcessor)
		{
			threadProcessor = true;
			m_PolicyProcessor = FillRules("UnityEditor.CustomEditorAttributes, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			m_SerializerProcessor = FillRules("UnityEditor.CustomEditorAttributes+MonoEditorType, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			_PageProcessor = m_PolicyProcessor.RestartList("kSCustomMultiEditors");
			resolverProcessor = m_SerializerProcessor.RestartList("m_InspectorType");
		}
		IList list = (IList)((IDictionary)_PageProcessor.GetValue(null))[asset];
		resolverProcessor.SetValue(list[0], visitor);
		CancelList();
	}

	internal static void CancelList()
	{
		if (_PredicateProcessor == null)
		{
			_PredicateProcessor = Type.GetType("UnityEditor.InspectorWindow, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			_RulesProcessor = _PredicateProcessor.GetMethod("RefreshInspectors", BindingFlags.Static | BindingFlags.NonPublic);
		}
		_RulesProcessor.Invoke(null, null);
	}

	internal static IList CountList(this GenericMenu init)
	{
		if (!errorProcessor)
		{
			errorProcessor = true;
			queueProcessor = typeof(GenericMenu).RestartList("menuItems");
			if (queueProcessor == null)
			{
				queueProcessor = typeof(GenericMenu).RestartList("m_MenuItems");
			}
		}
		if (!(queueProcessor == null))
		{
			return (IList)queueProcessor.GetValue(init);
		}
		return null;
	}

	internal static MethodInfo DisableList(this Type i, string counter)
	{
		return i.GetMethod(counter, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	internal static MethodInfo InsertList(this Type init, string result, Type[] template)
	{
		return init.GetMethod(result, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, template, null);
	}

	internal static FieldInfo RestartList(this Type i, string ord)
	{
		return i.GetField(ord, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	internal static PropertyInfo QueryList(this Type i, string visitor)
	{
		return i.GetProperty(visitor, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	internal static ConstructorInfo AddList(this Type param, Type[] selection)
	{
		return param.GetConstructor(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, selection, null);
	}

	internal static string InvokeList(IEnumerable<(string, string)> config)
	{
		StringBuilder stringBuilder = new StringBuilder("{");
		bool flag = true;
		foreach (var (text, text2) in config)
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

	internal static void FindList(Transform task, out ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> cont)
	{
		ExcludeList(task, out var cfg, out var serv);
		cont = ReadList(new ConcurrentBag<ConcurrentDictionary<Transform, ConcurrentBag<Vector3>>> { cfg, serv });
	}

	internal static void ExcludeList(Transform v, out ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> cfg, out ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> serv)
	{
		InitList(v, out cfg);
		VisitList(v, out serv);
	}

	internal static void InitList(Transform v, out ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> reg)
	{
		try
		{
			SkinnedMeshRenderer[] componentsInChildren = v.GetComponentsInChildren<SkinnedMeshRenderer>();
			ConcurrentBag<ConcurrentDictionary<Transform, ConcurrentBag<Vector3>>> concurrentBag = new ConcurrentBag<ConcurrentDictionary<Transform, ConcurrentBag<Vector3>>>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = componentsInChildren[i];
				if ((bool)skinnedMeshRenderer.sharedMesh)
				{
					EditorUtility.DisplayProgressBar("Gathering Mesh Vertices", skinnedMeshRenderer.name + " (" + (i + 1) + "/" + componentsInChildren.Length + ")", (float)(i + 1) / (float)componentsInChildren.Length);
					skinnedMeshRenderer.DefineList(out var map);
					concurrentBag.Add(map);
				}
			}
			reg = ReadList(concurrentBag);
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}

	internal static void VisitList(Transform info, out ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> selection)
	{
		try
		{
			MeshFilter[] componentsInChildren = info.GetComponentsInChildren<MeshFilter>();
			ConcurrentBag<ConcurrentDictionary<Transform, ConcurrentBag<Vector3>>> concurrentBag = new ConcurrentBag<ConcurrentDictionary<Transform, ConcurrentBag<Vector3>>>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				MeshFilter meshFilter = componentsInChildren[i];
				if ((bool)meshFilter.sharedMesh)
				{
					EditorUtility.DisplayProgressBar("Gathering Mesh Vertices", meshFilter.name + " (" + (i + 1) + "/" + componentsInChildren.Length + ")", (float)(i + 1) / (float)componentsInChildren.Length);
					meshFilter.StartList(out var ivk);
					concurrentBag.Add(ivk);
				}
			}
			selection = ReadList(concurrentBag);
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}

	internal static void DefineList(this SkinnedMeshRenderer var1, out ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> map)
	{
		Transform[] _ExpressionServer = var1.bones;
		Matrix4x4[] m_WatcherServer = new Matrix4x4[var1.bones.Length];
		Mesh sharedMesh = var1.sharedMesh;
		Vector3[] _AdapterServer = sharedMesh.vertices;
		ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> m_ProductServer = new ConcurrentDictionary<Transform, ConcurrentBag<Vector3>>();
		if (_ExpressionServer.Length != 0)
		{
			int vertexCount = sharedMesh.vertexCount;
			Vector3[] m_CandidateServer = new Vector3[vertexCount];
			BoneWeight[] interpreterServer = sharedMesh.boneWeights;
			Matrix4x4[] bindposes = sharedMesh.bindposes;
			for (int i = 0; i < m_WatcherServer.Length; i++)
			{
				Transform transform = _ExpressionServer[i];
				if (!(transform == null))
				{
					m_WatcherServer[i] = transform.localToWorldMatrix * bindposes[i];
					m_ProductServer.TryAdd(transform, new ConcurrentBag<Vector3>());
				}
			}
			Parallel.For(0, _AdapterServer.Length, delegate(int num)
			{
				if (num < _AdapterServer.Length)
				{
					Matrix4x4 matrix4x = default(Matrix4x4);
					BoneWeight boneWeight = interpreterServer[num];
					Matrix4x4 matrix4x2 = m_WatcherServer[boneWeight.boneIndex0];
					Matrix4x4 matrix4x3 = m_WatcherServer[boneWeight.boneIndex1];
					Matrix4x4 matrix4x4 = m_WatcherServer[boneWeight.boneIndex2];
					Matrix4x4 matrix4x5 = m_WatcherServer[boneWeight.boneIndex3];
					float weight = boneWeight.weight0;
					float weight2 = boneWeight.weight1;
					float weight3 = boneWeight.weight2;
					float weight4 = boneWeight.weight3;
					matrix4x.m00 = matrix4x2.m00 * weight + matrix4x3.m00 * weight2 + matrix4x4.m00 * weight3 + matrix4x5.m00 * weight4;
					matrix4x.m01 = matrix4x2.m01 * weight + matrix4x3.m01 * weight2 + matrix4x4.m01 * weight3 + matrix4x5.m01 * weight4;
					matrix4x.m02 = matrix4x2.m02 * weight + matrix4x3.m02 * weight2 + matrix4x4.m02 * weight3 + matrix4x5.m02 * weight4;
					matrix4x.m03 = matrix4x2.m03 * weight + matrix4x3.m03 * weight2 + matrix4x4.m03 * weight3 + matrix4x5.m03 * weight4;
					matrix4x.m10 = matrix4x2.m10 * weight + matrix4x3.m10 * weight2 + matrix4x4.m10 * weight3 + matrix4x5.m10 * weight4;
					matrix4x.m11 = matrix4x2.m11 * weight + matrix4x3.m11 * weight2 + matrix4x4.m11 * weight3 + matrix4x5.m11 * weight4;
					matrix4x.m12 = matrix4x2.m12 * weight + matrix4x3.m12 * weight2 + matrix4x4.m12 * weight3 + matrix4x5.m12 * weight4;
					matrix4x.m13 = matrix4x2.m13 * weight + matrix4x3.m13 * weight2 + matrix4x4.m13 * weight3 + matrix4x5.m13 * weight4;
					matrix4x.m20 = matrix4x2.m20 * weight + matrix4x3.m20 * weight2 + matrix4x4.m20 * weight3 + matrix4x5.m20 * weight4;
					matrix4x.m21 = matrix4x2.m21 * weight + matrix4x3.m21 * weight2 + matrix4x4.m21 * weight3 + matrix4x5.m21 * weight4;
					matrix4x.m22 = matrix4x2.m22 * weight + matrix4x3.m22 * weight2 + matrix4x4.m22 * weight3 + matrix4x5.m22 * weight4;
					matrix4x.m23 = matrix4x2.m23 * weight + matrix4x3.m23 * weight2 + matrix4x4.m23 * weight3 + matrix4x5.m23 * weight4;
					m_CandidateServer[num] = matrix4x.MultiplyPoint3x4(_AdapterServer[num]);
					int num2 = SelectList(weight, weight2, weight3, weight4) switch
					{
						2 => boneWeight.boneIndex2, 
						1 => boneWeight.boneIndex1, 
						0 => boneWeight.boneIndex0, 
						_ => boneWeight.boneIndex3, 
					};
					m_ProductServer[_ExpressionServer[num2]].Add(m_CandidateServer[num]);
				}
			});
			map = m_ProductServer;
		}
		else
		{
			Transform transform2 = var1.transform;
			ConcurrentBag<Vector3> m_SystemServer = new ConcurrentBag<Vector3>();
			Matrix4x4 workerServer = transform2.localToWorldMatrix;
			Parallel.For(0, _AdapterServer.Length, delegate(int num)
			{
				m_SystemServer.Add(workerServer.MultiplyPoint3x4(_AdapterServer[num]));
			});
			m_ProductServer.TryAdd(transform2, m_SystemServer);
			map = m_ProductServer;
		}
	}

	internal static void StartList(this MeshFilter key, out ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> ivk)
	{
		ivk = new ConcurrentDictionary<Transform, ConcurrentBag<Vector3>>();
		Mesh sharedMesh = key.sharedMesh;
		Transform transform = key.transform;
		ivk.TryAdd(transform, new ConcurrentBag<Vector3>());
		for (int i = 0; i < sharedMesh.vertexCount; i++)
		{
			ivk[transform].Add(transform.TransformPoint(sharedMesh.vertices[i]));
		}
	}

	internal static ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> ReadList(ConcurrentBag<ConcurrentDictionary<Transform, ConcurrentBag<Vector3>>> setup)
	{
		ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> _StubServer = new ConcurrentDictionary<Transform, ConcurrentBag<Vector3>>();
		_003C_003Ec__DisplayClass402_0 CS_0024_003C_003E8__locals0;
		Parallel.ForEach(setup, delegate(ConcurrentDictionary<Transform, ConcurrentBag<Vector3>> bag)
		{
			_003C_003Ec__DisplayClass402_1 _003C_003Ec__DisplayClass402_ = new _003C_003Ec__DisplayClass402_1();
			_003C_003Ec__DisplayClass402_._BridgeServer = CS_0024_003C_003E8__locals0;
			_003C_003Ec__DisplayClass402_.m_ReaderServer = bag;
			Parallel.ForEach(_003C_003Ec__DisplayClass402_.m_ReaderServer.Keys, _003C_003Ec__DisplayClass402_.FindConnection);
		});
		return _StubServer;
	}

	internal static int SelectList(params float[] values)
	{
		int result = 0;
		float num = values[0];
		for (int i = 1; i < values.Length; i++)
		{
			if (!(values[i] <= num))
			{
				result = i;
				num = values[i];
			}
		}
		return result;
	}

	internal static void RemoveList(string first)
	{
		if (!Directory.Exists(first))
		{
			Directory.CreateDirectory(first);
		}
	}

	internal static string InstantiateList(string var1, bool overridecust = false, PathOption filter = PathOption.Normal)
	{
		bool flag = filter == PathOption.ForceFolder;
		bool flag2;
		var1 = ((flag2 = filter == PathOption.ForceFile) ? CalculateList(var1) : (flag ? ConnectList(var1) : FlushList(var1)));
		if (!flag && (flag2 || !string.IsNullOrEmpty(Path.GetExtension(var1))))
		{
			string text = Path.GetDirectoryName(var1);
			string fileName = Path.GetFileName(var1);
			if (string.IsNullOrEmpty(text))
			{
				text = "Assets";
			}
			else if (!text.StartsWith(Application.dataPath) && !text.StartsWith("Assets"))
			{
				text = "Assets/" + text;
			}
			if (text != "Assets" && !Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
				AssetDatabase.ImportAsset(text);
			}
			var1 = text + "/" + fileName;
			if (overridecust)
			{
				var1 = AssetDatabase.GenerateUniqueAssetPath(var1);
			}
		}
		else if (!Directory.Exists(var1))
		{
			Directory.CreateDirectory(var1);
			AssetDatabase.ImportAsset(var1);
		}
		else if (overridecust)
		{
			var1 = AssetDatabase.GenerateUniqueAssetPath(var1);
			Directory.CreateDirectory(var1);
			AssetDatabase.ImportAsset(var1);
		}
		return var1;
	}

	internal static string AwakeList(string i, string reg, bool writestate = false)
	{
		if (!string.IsNullOrEmpty(reg))
		{
			if (!string.IsNullOrEmpty(i))
			{
				return InstantiateList(ConnectList(i) + "/" + CalculateList(reg), writestate);
			}
			return InstantiateList(CalculateList(reg), writestate, PathOption.ForceFile);
		}
		return InstantiateList(ConnectList(i), writestate, PathOption.ForceFolder);
	}

	internal static string ResetList(UnityEngine.Object instance, string b = "", bool forcecomp = true)
	{
		string assetPath = AssetDatabase.GetAssetPath(instance);
		string? directoryName = Path.GetDirectoryName(assetPath);
		if (string.IsNullOrEmpty(b))
		{
			b = Path.GetFileName(assetPath);
		}
		if (b.StartsWith("."))
		{
			b = ((!string.IsNullOrEmpty(instance.name)) ? instance.name : "SomeAsset") + b;
		}
		return AwakeList(directoryName, b, forcecomp);
	}

	internal static string FlushList(string param)
	{
		if (!string.IsNullOrEmpty(param))
		{
			string extension = Path.GetExtension(param);
			if (string.IsNullOrEmpty(extension))
			{
				return ConnectList(param);
			}
			string directoryName = Path.GetDirectoryName(param);
			string text = CalculateList(Path.GetFileNameWithoutExtension(param));
			if (!string.IsNullOrEmpty(directoryName))
			{
				directoryName = ConnectList(directoryName);
				return directoryName + "/" + text + extension;
			}
			return text + extension;
		}
		Debug.LogWarning("Legalizing empty path! Returned path as 'EmptyPath'");
		return "EmptyPath";
	}

	internal static string ConnectList(string init)
	{
		string _CustomerServer = Regex.Escape(new string(Path.GetInvalidPathChars()));
		init = init.Replace('\\', '/');
		if (init.IndexOf('/') > 0)
		{
			init = string.Join("/", from s in init.Split(new char[1] { '/' })
				select Regex.Replace(s, "[" + _CustomerServer + "]", "-"));
		}
		return init;
	}

	internal static string CalculateList(string setup)
	{
		string text = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
		if (string.IsNullOrEmpty(setup))
		{
			return "Unnamed";
		}
		return Regex.Replace(setup, "[" + text + "]", "-");
	}

	internal static MethodInfo TestList(this Type info, string cust)
	{
		MethodInfo[] array = (from m in info.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.Name == cust
			select m).ToArray();
		switch (array.Length)
		{
		case 1:
			return array[0];
		default:
			Debug.LogError("Multiple methods named " + cust + " found in " + info.Name);
			return null;
		case 0:
			Debug.LogError("Method " + cust + " not found in " + info.Name);
			return null;
		}
	}

	internal static MethodInfo MapList(this Type ident, string pred, Type dic)
	{
		MethodInfo[] array = (from m in ident.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.Name == pred && m.GetParameters().Any((ParameterInfo p) => p.ParameterType == dic)
			select m).ToArray();
		switch (array.Length)
		{
		case 0:
			Debug.LogError("Method " + pred + " not found in " + ident.Name + " with parameter of type " + dic.Name);
			return null;
		case 1:
			return array[0];
		default:
			Debug.LogError("Multiple methods named " + pred + " found in " + ident.Name + " with parameter of type " + dic.Name);
			return null;
		}
	}

	internal static MethodInfo ValidateList(this Type setup, string ivk, Type[] temp)
	{
		MethodInfo[] array = (from m in setup.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.Name == ivk && !temp.Except(m.GetParameters().Select(_003C_003Ec.m_ConfigObserver.IncludeSetter)).Any()
			select m).ToArray();
		switch (array.Length)
		{
		case 0:
			Debug.LogError("Method " + ivk + " not found in " + setup.Name + " with parameters of types " + string.Join(", ", temp.Select((Type ht) => ht.Name)));
			return null;
		case 1:
			return array[0];
		default:
			Debug.LogError("Multiple methods named " + ivk + " found in " + setup.Name + " with parameters of types " + string.Join(", ", temp.Select((Type ht) => ht.Name)));
			return null;
		}
	}

	internal static MethodInfo CustomizeList(this Type res, string visitor, int serv_Low)
	{
		MethodInfo[] array = (from m in res.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.Name == visitor && m.GetParameters().Length == serv_Low
			select m).ToArray();
		switch (array.Length)
		{
		default:
			Debug.LogError($"Multiple methods named {visitor} found in {res.Name} with {serv_Low} parameters");
			return null;
		case 0:
			Debug.LogError($"Method {visitor} not found in {res.Name} with {serv_Low} parameters");
			return null;
		case 1:
			return array[0];
		}
	}

	internal static void RateList(Vector3 value, Vector3 pred, float res = 1.4f, bool stripres2 = false, bool isinstance3 = false)
	{
		Vector3 forward = value - (value + pred);
		GetList(value, res, Quaternion.LookRotation(forward), stripres2, isinstance3);
	}

	internal static void DestroyList(Vector3 instance, Vector3 b, bool loadrule = false, bool containssecond2 = false)
	{
		Vector3 forward = instance - (instance + b);
		GetList(instance, b.magnitude, Quaternion.LookRotation(forward), loadrule, containssecond2);
	}

	internal static void GetList(Vector3 value, float counter, Quaternion? dic = null, bool evaluatevis2 = true, bool issecond3 = false)
	{
		SceneView lastActiveSceneView = SceneView.lastActiveSceneView;
		if ((bool)lastActiveSceneView)
		{
			Camera camera = lastActiveSceneView.camera;
			if (!dic.HasValue)
			{
				dic = camera.transform.rotation;
			}
			float newSize = counter * Mathf.Sin(camera.fieldOfView * 0.5f * ((float)Math.PI / 180f));
			lastActiveSceneView.LookAt(value, dic.Value, newSize, issecond3, !evaluatevis2);
		}
	}

	internal static void CalcList(this VRCAvatarDescriptor item, bool iscfg = false)
	{
		float num = item.IncludeList();
		Transform transform = item.transform;
		Vector3 position = transform.position;
		Vector3 vector = position + Vector3.up * num;
		RateList((position + vector * 5f) / 6f, transform.forward, num * 0.66f, iscfg);
	}

	internal static float IncludeList(this VRCAvatarDescriptor first)
	{
		try
		{
			Vector3 position = first.transform.position;
			Animator component = first.GetComponent<Animator>();
			Transform boneTransform = component.GetBoneTransform(HumanBodyBones.Head);
			Transform boneTransform2 = component.GetBoneTransform(HumanBodyBones.Neck);
			Vector3 position2 = boneTransform.position;
			return (position2 + (position2 - boneTransform2.position) - position).y;
		}
		catch
		{
			return first.ViewPosition.y;
		}
	}

	internal static Vector3 RunList(Vector3 param, Vector3 selection, Vector3 third)
	{
		third = third.normalized;
		float num = Vector3.Dot(selection - param, third);
		return param + third * num;
	}

	private static Texture2D CloneList(Texture2D ident, float reg = 0.2f, int position_dic = 1)
	{
		if (!(ident == null))
		{
			using (ConsumerPolicy consumerPolicy = new ConsumerPolicy(ident))
			{
				Texture2D interpreterPolicy = consumerPolicy.m_InterpreterPolicy;
				int width = interpreterPolicy.width;
				int height = interpreterPolicy.height;
				int num = width;
				int num2 = 0;
				int num3 = height;
				int num4 = 0;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						if (interpreterPolicy.GetPixel(j, i).a >= reg)
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
				int num7 = num5 + position_dic * 2;
				int num8 = num6 + position_dic * 2;
				if (num5 < 1 || num6 < 1)
				{
					Debug.LogError("Trimmed texture has zero size.");
					return null;
				}
				Color[] pixels = interpreterPolicy.GetPixels(num, num3, num5, num6);
				Texture2D texture2D = new Texture2D(num7, num8);
				for (int k = 0; k < num8; k++)
				{
					for (int l = 0; l < num7; l++)
					{
						if (l < position_dic || l >= position_dic + num5 || k < position_dic || k >= position_dic + num6)
						{
							texture2D.SetPixel(l, k, Color.clear);
						}
					}
				}
				texture2D.SetPixels(position_dic, position_dic, num5, num6, pixels);
				texture2D.Apply();
				return texture2D;
			}
		}
		throw new ArgumentNullException("texture");
	}

	internal static Texture2D LoginList(Color res)
	{
		if (connectionProcessor == null)
		{
			connectionProcessor = new Texture2D(1, 1, TextureFormat.RGBAFloat, mipChain: false)
			{
				filterMode = FilterMode.Point,
				anisoLevel = 0
			};
		}
		connectionProcessor.SetPixel(0, 0, res);
		connectionProcessor.Apply();
		return connectionProcessor;
	}

	internal static Texture2D ReflectList(Color instance)
	{
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, mipChain: false);
		texture2D.filterMode = FilterMode.Point;
		texture2D.anisoLevel = 0;
		texture2D.SetPixel(0, 0, instance);
		texture2D.Apply();
		return texture2D;
	}

	internal static Texture2D DeleteList(Color v, int start_ivk)
	{
		return CreateList(v, start_ivk, Color.clear);
	}

	internal static Texture2D CreateList(Color first, int vis_Position, Color dic)
	{
		Texture2D texture2D = new Texture2D(vis_Position, vis_Position, TextureFormat.RGBA32, mipChain: false)
		{
			filterMode = FilterMode.Point,
			anisoLevel = 0
		};
		for (int i = 0; i < vis_Position; i++)
		{
			for (int j = 0; j < vis_Position; j++)
			{
				Vector2 a = new Vector2(j, i);
				Vector2 b = new Vector2((float)vis_Position / 2f, (float)vis_Position / 2f);
				if (Vector2.Distance(a, b) <= (float)vis_Position / 2f)
				{
					texture2D.SetPixel(j, i, first);
				}
				else
				{
					texture2D.SetPixel(j, i, dic);
				}
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	internal static ErrorPolicy NewList(string value, string vis, string template = "")
	{
		ErrorPolicy errorPolicy = new ErrorPolicy(vis, template);
		if (errorPolicy.CompareHelper() == null)
		{
			GUIContent gUIContent = EditorGUIUtility.IconContent(value);
			if (gUIContent != null && gUIContent.image != null)
			{
				errorPolicy.SetHelper(CloneList(gUIContent.image as Texture2D));
			}
		}
		return errorPolicy;
	}

	internal static Color PushList(float i)
	{
		if (i > 1f)
		{
			i /= 255f;
		}
		return new Color(i, i, i, 1f);
	}

	internal static Texture2D ViewList(Texture2D task, int ivk_counter, int version_res, out Color[] asset2, bool applyattr3 = false)
	{
		task.filterMode = FilterMode.Point;
		RenderTexture temporary = RenderTexture.GetTemporary(ivk_counter, version_res);
		temporary.filterMode = FilterMode.Point;
		RenderTexture.active = temporary;
		Graphics.Blit(task, temporary);
		Texture2D texture2D = new Texture2D(ivk_counter, version_res);
		texture2D.ReadPixels(new Rect(0f, 0f, ivk_counter, version_res), 0, 0);
		Color[] pixels = texture2D.GetPixels();
		RenderTexture.active = null;
		asset2 = pixels;
		if (!applyattr3)
		{
			return texture2D;
		}
		UnityEngine.Object.DestroyImmediate(texture2D);
		return null;
	}

	internal static void CollectList(Transform asset, Transform ivk, Transform template, bool moveres2, bool isparam3, Axis vis4 = Axis.X, PlaneAxis reference5 = PlaneAxis.YZ, Axis vis6 = Axis.None)
	{
		if (moveres2 && asset != asset.root)
		{
			Animator componentInChildren = asset.root.GetComponentInChildren<Animator>();
			if (componentInChildren != null && (bool)componentInChildren.avatar && componentInChildren.isHuman && asset.IsChildOf(componentInChildren.transform))
			{
				for (int i = 0; i < 55; i++)
				{
					Transform boneTransform = componentInChildren.GetBoneTransform((HumanBodyBones)i);
					if ((bool)boneTransform && boneTransform == asset.parent)
					{
						if (!PublishList(i, out var ord))
						{
							break;
						}
						Transform boneTransform2 = componentInChildren.GetBoneTransform((HumanBodyBones)ord);
						if (!boneTransform2)
						{
							Debug.LogWarning("Attempting to mirror through humanoid but mirror human bone can't be found!");
							break;
						}
						Vector3 localPosition = asset.localPosition;
						Quaternion localRotation = asset.localRotation;
						Undo.SetTransformParent(asset, boneTransform2, "Mirror Transform");
						ivk.localPosition = localPosition;
						ivk.localRotation = localRotation;
						Debug.Log("Mirrored!");
						return;
					}
				}
			}
		}
		if (!template)
		{
			ivk.position = asset.position.PopPredicate(vis4);
		}
		else
		{
			Vector3 position = asset.position;
			if (!isparam3)
			{
				Vector3 position2 = template.position;
				Vector3 ident = position2 + (position - position2).PopPredicate(vis4) - position;
				ident = ident.ComputePredicate(vis4);
				position += ident;
				ivk.position = position;
			}
			else
			{
				Vector3 position3 = template.InverseTransformPoint(position).PopPredicate(vis4);
				ivk.position = template.TransformPoint(position3);
			}
		}
		if (reference5 != PlaneAxis.None)
		{
			ivk.rotation = ListList(asset.rotation, reference5);
			if (vis6 != Axis.None)
			{
				ivk.Rotate(vis6.HasFlag(Axis.X) ? 180 : 0, vis6.HasFlag(Axis.Y) ? 180 : 0, vis6.HasFlag(Axis.Z) ? 180 : 0, Space.Self);
			}
		}
	}

	private static Quaternion ResolveList(Quaternion value, Vector3 ivk)
	{
		return Quaternion.LookRotation(Vector3.Reflect(value * Vector3.forward, ivk), Vector3.Reflect(value * Vector3.up, ivk));
	}

	private static Quaternion ListList(Quaternion def, PlaneAxis vis)
	{
		return ResolveList(def, vis switch
		{
			PlaneAxis.XY => Vector3.forward, 
			PlaneAxis.XZ => Vector3.up, 
			_ => Vector3.right, 
		});
	}

	internal static void VerifyList(Transform setup, Vector3 map, Transform[] res, bool forcekey2 = true)
	{
		if (forcekey2)
		{
			Undo.RecordObject(setup, "Scale and Preserve");
			foreach (Transform transform in res)
			{
				if (!(transform == null))
				{
					Undo.RecordObject(transform, "Scale and Preserve");
				}
			}
		}
		Vector3[] array = new Vector3[res.Length];
		for (int j = 0; j < res.Length; j++)
		{
			Transform transform2 = res[j];
			if (!(transform2 == null))
			{
				array[j] = transform2.position;
			}
		}
		setup.localScale = map;
		for (int k = 0; k < res.Length; k++)
		{
			Transform transform3 = res[k];
			if (!(transform3 == null))
			{
				transform3.position = array[k];
			}
		}
	}

	internal static VRCContactSender FillList(this VRCContactReceiver res, GameObject cfg)
	{
		VRCContactSender vRCContactSender = Undo.AddComponent<VRCContactSender>(cfg);
		new InterceptorPolicy(res).TestHelper(vRCContactSender);
		vRCContactSender.collisionTags = res.collisionTags;
		vRCContactSender.rootTransform = res.rootTransform;
		if (vRCContactSender.rootTransform == vRCContactSender.transform)
		{
			vRCContactSender.rootTransform = null;
		}
		return vRCContactSender;
	}

	internal static VRCContactSender WriteList(this VRCPhysBoneCollider asset, GameObject selection)
	{
		VRCContactSender vRCContactSender = Undo.AddComponent<VRCContactSender>(selection);
		new InterceptorPolicy(asset).TestHelper(vRCContactSender);
		vRCContactSender.rootTransform = asset.rootTransform;
		if (vRCContactSender.rootTransform == vRCContactSender.transform)
		{
			vRCContactSender.rootTransform = null;
		}
		return vRCContactSender;
	}

	internal static VRCContactReceiver ForgotList(this VRCContactSender init, GameObject result)
	{
		VRCContactReceiver vRCContactReceiver = Undo.AddComponent<VRCContactReceiver>(result);
		new InterceptorPolicy(init).TestHelper(vRCContactReceiver);
		vRCContactReceiver.collisionTags = init.collisionTags;
		vRCContactReceiver.rootTransform = init.rootTransform;
		if (vRCContactReceiver.rootTransform == vRCContactReceiver.transform)
		{
			vRCContactReceiver.rootTransform = null;
		}
		return vRCContactReceiver;
	}

	internal static VRCContactReceiver StopList(this VRCPhysBoneCollider param, GameObject cfg)
	{
		VRCContactReceiver vRCContactReceiver = Undo.AddComponent<VRCContactReceiver>(cfg);
		new InterceptorPolicy(param).TestHelper(vRCContactReceiver);
		vRCContactReceiver.rootTransform = param.rootTransform;
		if (vRCContactReceiver.rootTransform == vRCContactReceiver.transform)
		{
			vRCContactReceiver.rootTransform = null;
		}
		return vRCContactReceiver;
	}

	internal static VRCPhysBoneCollider CheckList(this VRCContactReceiver task, GameObject cont)
	{
		VRCPhysBoneCollider vRCPhysBoneCollider = Undo.AddComponent<VRCPhysBoneCollider>(cont);
		new InterceptorPolicy(task).MapHelper(vRCPhysBoneCollider);
		vRCPhysBoneCollider.rootTransform = task.rootTransform;
		if (vRCPhysBoneCollider.rootTransform == vRCPhysBoneCollider.transform)
		{
			vRCPhysBoneCollider.rootTransform = null;
		}
		return vRCPhysBoneCollider;
	}

	internal static VRCPhysBoneCollider PrepareList(this VRCContactSender reference, GameObject selection)
	{
		VRCPhysBoneCollider vRCPhysBoneCollider = Undo.AddComponent<VRCPhysBoneCollider>(selection);
		new InterceptorPolicy(reference).MapHelper(vRCPhysBoneCollider);
		vRCPhysBoneCollider.rootTransform = reference.rootTransform;
		if (vRCPhysBoneCollider.rootTransform == vRCPhysBoneCollider.transform)
		{
			vRCPhysBoneCollider.rootTransform = null;
		}
		return vRCPhysBoneCollider;
	}

	internal static void AssetList(VRCAvatarDescriptor spec, ref string[] caller, ref int[] res)
	{
		string[] array = new string[8] { "Base", "Additive", "Gesture", "Action", "FX", "Sitting", "TPose", "IKPose" };
		if ((bool)spec)
		{
			List<(string, int)> list = new List<(string, int)>();
			for (int i = 0; i < array.Length; i++)
			{
				int num = ((i != 0) ? (i + 1) : i);
				if (spec.UpdateList((VRCAvatarDescriptor.AnimLayerType)num, out var _))
				{
					list.Add((array[i], num));
				}
			}
			caller = new string[list.Count];
			res = new int[list.Count];
			for (int j = 0; j < list.Count; j++)
			{
				caller[j] = list[j].Item1;
				res[j] = list[j].Item2;
			}
		}
		else
		{
			caller = Array.Empty<string>();
			res = Array.Empty<int>();
		}
	}

	internal static bool UpdateList(this VRCAvatarDescriptor item, VRCAvatarDescriptor.AnimLayerType cust, out UnityEditor.Animations.AnimatorController proc)
	{
		proc = (from l in item.baseAnimationLayers.Concat(item.specialAnimationLayers)
			where l.type == cust
			select l.animatorController).FirstOrDefault() as UnityEditor.Animations.AnimatorController;
		return proc != null;
	}

	internal static UnityEditor.Animations.AnimatorController ChangeList(this VRCAvatarDescriptor def, VRCAvatarDescriptor.AnimLayerType cont)
	{
		if (def.UpdateList(cont, out var proc))
		{
			return proc;
		}
		return null;
	}

	internal static bool SortList(this VRCAvatarDescriptor setup, VRCAvatarDescriptor.AnimLayerType attr, RuntimeAnimatorController role)
	{
		_003C_003Ec__DisplayClass446_0 map = default(_003C_003Ec__DisplayClass446_0);
		map._PrinterServer = attr;
		map._WriterServer = role;
		map._ParamsServer = setup;
		if (ValidateError(map._ParamsServer.baseAnimationLayers, ref map))
		{
			return true;
		}
		return ValidateError(map._ParamsServer.specialAnimationLayers, ref map);
	}

	internal static int RegisterList(this VRCAvatarDescriptor instance, VRCAvatarDescriptor.AnimLayerType second)
	{
		return SortPredicate(instance.ChangeList(second));
	}

	internal static VRCAvatarDescriptor LogoutList(GameObject item, Action pol = null)
	{
		if (m_ContextProcessor == null)
		{
			m_ContextProcessor = new Dictionary<GameObject, VRCAvatarDescriptor>();
		}
		m_ContextProcessor.TryGetValue(item, out var value);
		VRCAvatarDescriptor vRCAvatarDescriptor = item.transform.root.GetComponentsInChildren<VRCAvatarDescriptor>().FirstOrDefault((VRCAvatarDescriptor a) => item.transform.IsChildOf(a.transform));
		if (vRCAvatarDescriptor != value)
		{
			m_ContextProcessor[item] = vRCAvatarDescriptor;
			pol?.Invoke();
		}
		return vRCAvatarDescriptor;
	}

	[SpecialName]
	internal static int RunError()
	{
		if (recordProcessor == 0)
		{
			try
			{
				recordProcessor = (int)ForgotRules("VRCExpressionParameters").GetField("MAX_PARAMETER_COST", BindingFlags.Static | BindingFlags.Public).GetValue(null);
			}
			catch
			{
				Debug.LogWarning("Failed to dynamically get MAX_PARAMETER_COST. Falling back to 256");
				recordProcessor = 256;
			}
		}
		return recordProcessor;
	}

	internal static int PatchList(this VRCExpressionParameters def)
	{
		int num = 0;
		List<string> list = new List<string>();
		VRCExpressionParameters.Parameter[] parameters = def.parameters;
		foreach (VRCExpressionParameters.Parameter parameter in parameters)
		{
			if (!string.IsNullOrEmpty(parameter.name) && !list.Contains(parameter.name) && parameter.MoveError())
			{
				list.Add(parameter.name);
				num += ((parameter.valueType == VRCExpressionParameters.ValueType.Bool) ? 1 : 8);
			}
		}
		return num;
	}

	internal static int InterruptList(this VRCExpressionParameters var1, bool includesecond = true, bool istemp = true)
	{
		if (var1 == null)
		{
			if (!istemp)
			{
				return 0;
			}
			return RunError();
		}
		int num = (includesecond ? var1.PatchList() : var1.CalcTotalCost());
		return RunError() - num;
	}

	internal static void ManageList(this VRCExpressionParameters info)
	{
		if (!info)
		{
			return;
		}
		List<string> list = new List<string>();
		List<VRCExpressionParameters.Parameter> list2 = info.parameters.ToList();
		for (int num = info.parameters.Length - 1; num >= 0; num--)
		{
			VRCExpressionParameters.Parameter parameter = info.parameters[num];
			if (string.IsNullOrEmpty(parameter.name) || list.Contains(parameter.name))
			{
				list2.RemoveAt(num);
			}
			else
			{
				list.Add(parameter.name);
			}
		}
		info.parameters = list2.ToArray();
		EditorUtility.SetDirty(info);
	}

	internal static SystemThread PrintList(this VRCAvatarDescriptor init, VRCExpressionParameters selection, bool validatedic = true, bool isparam2 = true)
	{
		return init.SearchList((!(selection == null)) ? selection.parameters.ToList() : null, validatedic, isparam2);
	}

	internal static SystemThread SearchList(this VRCAvatarDescriptor asset, IEnumerable<VRCExpressionParameters.Parameter> ivk, bool isproc = true, bool explicitx2 = true, bool iscol3 = false)
	{
		if (!(asset == null))
		{
			if (iscol3 || !(asset.expressionParameters == null))
			{
				return asset.expressionParameters.CompareError(ivk, isproc, explicitx2, iscol3);
			}
			SystemThread result = new SystemThread(isparam: false, "Avatar Expression Parameters are not set (Null)");
			result._StubThread = 1;
			return result;
		}
		return (false, "Avatar is not set (Null)");
	}

	internal static SystemThread RevertList(this VRCExpressionParameters init, VRCExpressionParameters pred, bool extractres = true, bool isident2 = true)
	{
		return init.CompareError(pred?.parameters.ToList(), extractres, isident2);
	}

	internal static SystemThread OrderError(this VRCExpressionParameters spec, IEnumerable<VRCExpressionParameters> token, bool isrule = true, bool overridesetup2 = true)
	{
		return spec.CompareError((from p in token?.Where((VRCExpressionParameters p) => p != null).SelectMany((VRCExpressionParameters p) => p.parameters)
			where p != null
			select p), isrule, overridesetup2);
	}

	internal static SystemThread CompareError(this VRCExpressionParameters var1, IEnumerable<VRCExpressionParameters.Parameter> map, bool isproc = true, bool moveresult2 = true, bool setconfig3 = false)
	{
		if (map != null)
		{
			bool flag = var1 == null;
			SystemThread result;
			if (!setconfig3 && flag)
			{
				result = new SystemThread(isparam: false, "Target Expression Parameters are not set (Null)");
				result._StubThread = 1;
				return result;
			}
			int num = ((!flag) ? ((!isproc) ? var1.CalcTotalCost() : var1.PatchList()) : 0);
			int num2 = 0;
			foreach (VRCExpressionParameters.Parameter item in map)
			{
				if (item != null && !string.IsNullOrEmpty(item.name) && item.MoveError() && (flag || moveresult2 || var1.FindParameter(item.name) == null))
				{
					num2 += VRCExpressionParameters.TypeCost(item.valueType);
				}
			}
			if (num + num2 <= RunError())
			{
				return (true, string.Empty);
			}
			result = new SystemThread(isparam: false, $"Expression Parameters would exceed the {RunError()} cost limit");
			result._StubThread = 2;
			return result;
		}
		return (false, "Expression Paramereters are not set (Null)");
	}

	internal static VRCExpressionParameters.Parameter[] SetError(this VRCAvatarDescriptor i, VRCExpressionParameters pred, bool istag = true, bool applyinstance2 = true)
	{
		return i.PostError((pred == null) ? null : pred.parameters, istag, applyinstance2);
	}

	internal static VRCExpressionParameters.Parameter[] PostError(this VRCAvatarDescriptor spec, IEnumerable<VRCExpressionParameters.Parameter> vis, bool isstate = true, bool usekey2 = true)
	{
		if (!(spec == null))
		{
			if (spec.expressionParameters == null)
			{
				throw new NullReferenceException("Avatar Expression Parameters are not set (Null)");
			}
			return spec.expressionParameters.EnableError(vis, isstate, usekey2);
		}
		throw new NullReferenceException("Avatar is not set (Null)");
	}

	internal static VRCExpressionParameters.Parameter[] SetupError(this VRCExpressionParameters def, VRCExpressionParameters attr, bool procstop = true, bool istask2 = true)
	{
		return def.EnableError((attr == null) ? null : attr.parameters, procstop, istask2);
	}

	internal static VRCExpressionParameters.Parameter[] EnableError(this VRCExpressionParameters def, IEnumerable<VRCExpressionParameters.Parameter> b, bool ignorefield = true, bool useresult2 = true, string map3 = "", string connection4 = "")
	{
		if (b != null)
		{
			if (def == null)
			{
				throw new NullReferenceException("Target Expression Parameters are not set (Null)");
			}
			if (map3 == null)
			{
				map3 = string.Empty;
			}
			if (connection4 == null)
			{
				connection4 = string.Empty;
			}
			string m_EventServer = string.Empty;
			VRCExpressionParameters.Parameter[] array = b.ToArray();
			VRCExpressionParameters.Parameter[] array2;
			if (!useresult2)
			{
				while (true)
				{
					array2 = array;
					int num = 0;
					string text2;
					while (num < array2.Length)
					{
						VRCExpressionParameters.Parameter parameter = array2[num];
						string text = map3 + parameter.name + connection4 + m_EventServer;
						text2 = SortRules(text, delegate(string s)
						{
							_003C_003Ec__DisplayClass465_1 _003C_003Ec__DisplayClass465_ = new _003C_003Ec__DisplayClass465_1();
							_003C_003Ec__DisplayClass465_._FacadeServer = s;
							return !def.parameters.Any(_003C_003Ec__DisplayClass465_.FlushConnection);
						});
						if (!(text2 != text))
						{
							num++;
							continue;
						}
						goto IL_00dd;
					}
					break;
					IL_00dd:
					LogoutRules(text2, out var b2);
					m_EventServer = $" {b2}";
				}
			}
			if (map3 != connection4 || map3 != m_EventServer || map3 != string.Empty)
			{
				array = array.Select(delegate(VRCExpressionParameters.Parameter p)
				{
					VRCExpressionParameters.Parameter parameter2 = p.PublishError();
					parameter2.name = map3 + p.name + connection4 + m_EventServer;
					return parameter2;
				}).ToArray();
			}
			List<VRCExpressionParameters.Parameter> list = new List<VRCExpressionParameters.Parameter>();
			array2 = array;
			foreach (VRCExpressionParameters.Parameter _AdvisorServer in array2)
			{
				if (!def.parameters.Any((VRCExpressionParameters.Parameter p2) => p2.name == _AdvisorServer.name))
				{
					VRCExpressionParameters.Parameter item = _AdvisorServer.PublishError();
					list.Add(item);
				}
			}
			def.parameters = def.parameters.Concat(list).ToArray();
			if (ignorefield)
			{
				def.ManageList();
			}
			EditorUtility.SetDirty(def);
			return list.ToArray();
		}
		throw new NullReferenceException("Expression Paramereters are not set (Null)");
	}

	internal static VRCExpressionParameters.Parameter PublishError(this VRCExpressionParameters.Parameter asset)
	{
		VRCExpressionParameters.Parameter parameter = new VRCExpressionParameters.Parameter();
		asset.PopError(parameter);
		return parameter;
	}

	internal static void PopError(this VRCExpressionParameters.Parameter def, VRCExpressionParameters.Parameter pol)
	{
		pol.name = def.name;
		pol.valueType = def.valueType;
		pol.saved = def.saved;
		pol.defaultValue = def.defaultValue;
		pol.ComputeError(def.MoveError());
	}

	internal static void ComputeError(this VRCExpressionParameters.Parameter res, bool appendpred)
	{
		if (res != null)
		{
			if (!m_ConsumerProcessor)
			{
				m_ConsumerProcessor = true;
				helperProcessor = res.GetType().GetField("networkSynced", BindingFlags.Instance | BindingFlags.Public);
			}
			if (!(helperProcessor == null))
			{
				helperProcessor.SetValue(res, appendpred);
			}
		}
	}

	internal static bool MoveError(this VRCExpressionParameters.Parameter ident)
	{
		if (ident == null)
		{
			return true;
		}
		if (!m_ConsumerProcessor)
		{
			m_ConsumerProcessor = true;
			helperProcessor = ident.GetType().GetField("networkSynced", BindingFlags.Instance | BindingFlags.Public);
		}
		if (!(helperProcessor == null))
		{
			return (bool)helperProcessor.GetValue(ident);
		}
		return true;
	}

	internal static VRCExpressionParameters ConcatError(this VRCAvatarDescriptor value, string pol, bool stripproc = false)
	{
		VRCExpressionParameters vRCExpressionParameters = value.expressionParameters;
		if ((bool)vRCExpressionParameters)
		{
			if (stripproc)
			{
				vRCExpressionParameters = OrderRules(vRCExpressionParameters, AwakeList(pol, vRCExpressionParameters.name + ".asset"));
			}
		}
		else
		{
			vRCExpressionParameters = ScriptableObject.CreateInstance<VRCExpressionParameters>();
			vRCExpressionParameters.parameters = Array.Empty<VRCExpressionParameters.Parameter>();
			AssetDatabase.CreateAsset(vRCExpressionParameters, AwakeList(pol, value.name + " Parameters.asset"));
		}
		value.customExpressions = true;
		value.expressionParameters = vRCExpressionParameters;
		EditorUtility.SetDirty(value);
		return vRCExpressionParameters;
	}

	internal static void CallError(this VRCAvatarDescriptor instance, VRCExpressionParameters visitor)
	{
		instance.expressionParameters = visitor;
		if (!visitor)
		{
			if (!instance.expressionsMenu)
			{
				instance.customExpressions = false;
			}
		}
		else
		{
			instance.customExpressions = true;
		}
		EditorUtility.SetDirty(instance);
	}

	internal static SystemThread CancelError(this VRCExpressionParameters.Parameter ident, VRCExpressionParameters.Parameter second, bool countcomp = true)
	{
		if (ident != second)
		{
			if (ident == null || second == null)
			{
				return new SystemThread(isparam: false, "One of the parameters is null");
			}
			if (!(ident.name == second.name))
			{
				return new SystemThread(isparam: false, "Parameters don't match by name", 1);
			}
			if (ident.valueType != second.valueType)
			{
				if (countcomp)
				{
					return true;
				}
				return new SystemThread(isparam: false, "Parameters don't match by type.", 2);
			}
			return true;
		}
		return true;
	}

	internal static SystemThread CountError(this VRCAvatarDescriptor i, VRCExpressionsMenu reg)
	{
		if (i == null)
		{
			return (false, "Avatar is not set (Null)");
		}
		if (!(i.expressionsMenu == null))
		{
			return i.expressionsMenu.DisableError(reg);
		}
		return new SystemThread(isparam: false, "Avatar Expressions Menu is not set (Null)");
	}

	internal static SystemThread DisableError(this VRCExpressionsMenu ident, VRCExpressionsMenu b)
	{
		if (b == null)
		{
			return new SystemThread(isparam: false, "Expression Menu is not set (Null)");
		}
		if (!(ident == null))
		{
			return ident.QueryError(b.controls.Count);
		}
		return new SystemThread(isparam: false, "Target Expression Menu is not set (Null)");
	}

	internal static SystemThread InsertError(this VRCExpressionsMenu.Control i, VRCExpressionsMenu selection)
	{
		if (selection == null)
		{
			return (false, "Expression Menu is not set (Null)");
		}
		return i.RestartError(selection.controls.Count);
	}

	internal static SystemThread RestartError(this VRCExpressionsMenu.Control ident, int startmap)
	{
		if (ident == null)
		{
			return (false, "Control is Null");
		}
		if (ident.type != VRCExpressionsMenu.Control.ControlType.SubMenu)
		{
			return (false, "Control is not a SubMenu");
		}
		return ident.subMenu.QueryError(startmap);
	}

	internal static SystemThread QueryError(this VRCExpressionsMenu first, int remove_VISITORAt)
	{
		if (first == null)
		{
			SystemThread result = new SystemThread(isparam: false, "SubMenu is Null");
			result._StubThread = 1;
			return result;
		}
		if (first.controls.Count + remove_VISITORAt > 8)
		{
			SystemThread result = new SystemThread(isparam: false, $"Adding {remove_VISITORAt} controls to {first.name} would exceed the 8 controls limit");
			result._StubThread = 2;
			return result;
		}
		return (true, "Can add controls");
	}

	internal static VRCExpressionsMenu.Control[] AddError(this VRCAvatarDescriptor config, VRCExpressionsMenu result)
	{
		if (!(config == null))
		{
			if (config.expressionsMenu == null)
			{
				throw new NullReferenceException("Avatar Expressions Menu is not set (Null)");
			}
			return config.expressionsMenu.InvokeError(result);
		}
		throw new NullReferenceException("Avatar is not set (Null)");
	}

	internal static VRCExpressionsMenu.Control[] InvokeError(this VRCExpressionsMenu var1, VRCExpressionsMenu reg)
	{
		if (var1 == null)
		{
			throw new NullReferenceException("Target Expression Menu is not set (Null)");
		}
		if (reg == null)
		{
			throw new NullReferenceException("Source Expression Menu is not set (Null)");
		}
		return var1.ExcludeError(reg.controls);
	}

	internal static VRCExpressionsMenu.Control[] FindError(this VRCExpressionsMenu.Control res, VRCExpressionsMenu ord)
	{
		if (res != null)
		{
			if (res.type != VRCExpressionsMenu.Control.ControlType.SubMenu)
			{
				throw new ArgumentException("Control is not a SubMenu");
			}
			return res.subMenu.InvokeError(ord);
		}
		throw new ArgumentException("Control is Null");
	}

	internal static VRCExpressionsMenu.Control[] ExcludeError(this VRCExpressionsMenu reference, IEnumerable<VRCExpressionsMenu.Control> col)
	{
		if (!(reference == null))
		{
			if (reference.controls == null)
			{
				reference.controls = new List<VRCExpressionsMenu.Control>();
			}
			if (col != null)
			{
				VRCExpressionsMenu.Control[] array = (col as VRCExpressionsMenu.Control[]) ?? col.ToArray();
				if (reference.controls.Count + array.Length > 8)
				{
					throw new Exception($"Adding {array.Length} controls to {reference.name} would exceed the 8 controls limit");
				}
				VRCExpressionsMenu.Control[] array2 = array.Select(InitError).ToArray();
				VRCExpressionsMenu.Control[] array3 = array2;
				foreach (VRCExpressionsMenu.Control item in array3)
				{
					reference.controls.Add(item);
				}
				EditorUtility.SetDirty(reference);
				return array2;
			}
			throw new NullReferenceException("New Controls are Null");
		}
		throw new NullReferenceException("Target Expression Menu is not set (Null)");
	}

	internal static VRCExpressionsMenu.Control InitError(VRCExpressionsMenu.Control reference)
	{
		VRCExpressionsMenu.Control control = new VRCExpressionsMenu.Control
		{
			type = reference.type,
			value = reference.value,
			icon = reference.icon,
			name = reference.name,
			subMenu = reference.subMenu,
			parameter = VisitError(reference.parameter)
		};
		if (reference.subParameters != null)
		{
			int num = reference.subParameters.Length;
			control.subParameters = new VRCExpressionsMenu.Control.Parameter[num];
			for (int i = 0; i < num; i++)
			{
				control.subParameters[i] = VisitError(reference.subParameters[i]);
			}
		}
		else
		{
			control.subParameters = Array.Empty<VRCExpressionsMenu.Control.Parameter>();
		}
		return control;
	}

	internal static VRCExpressionsMenu.Control.Parameter VisitError(VRCExpressionsMenu.Control.Parameter def)
	{
		return new VRCExpressionsMenu.Control.Parameter
		{
			name = ((def == null) ? string.Empty : def.name)
		};
	}

	internal static bool DefineError(this VRCExpressionsMenu value, Func<VRCExpressionsMenu, bool> selection, HashSet<VRCExpressionsMenu> tag = null)
	{
		if (!(value == null))
		{
			if (tag == null)
			{
				tag = new HashSet<VRCExpressionsMenu>();
			}
			if (tag.Add(value))
			{
				if (!selection(value))
				{
					foreach (VRCExpressionsMenu.Control control in value.controls)
					{
						if (control.type != VRCExpressionsMenu.Control.ControlType.SubMenu || !control.subMenu.DefineError(selection, tag))
						{
							continue;
						}
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	internal static VRCExpressionsMenu StartError(this VRCAvatarDescriptor asset, string cfg, bool isdic = false)
	{
		VRCExpressionsMenu vRCExpressionsMenu = asset.expressionsMenu;
		if ((bool)vRCExpressionsMenu)
		{
			if (isdic)
			{
				vRCExpressionsMenu = OrderRules(vRCExpressionsMenu, AwakeList(cfg, vRCExpressionsMenu.name + ".asset"));
			}
		}
		else
		{
			vRCExpressionsMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
			vRCExpressionsMenu.controls = new List<VRCExpressionsMenu.Control>();
			AssetDatabase.CreateAsset(vRCExpressionsMenu, AwakeList(cfg, asset.name + " Menu.asset"));
		}
		asset.customExpressions = true;
		asset.expressionsMenu = vRCExpressionsMenu;
		EditorUtility.SetDirty(asset);
		return vRCExpressionsMenu;
	}

	internal static void ReadError(this VRCAvatarDescriptor task, VRCExpressionsMenu pred)
	{
		task.expressionsMenu = pred;
		if (!pred)
		{
			if (!task.expressionParameters)
			{
				task.customExpressions = false;
			}
		}
		else
		{
			task.customExpressions = true;
		}
		EditorUtility.SetDirty(task);
	}

	internal static SystemThread SelectError(this VRCExpressionsMenu.Control key, VRCExpressionsMenu.Control visitor)
	{
		if (key != visitor)
		{
			if (key == null || visitor == null)
			{
				return new SystemThread(isparam: false, "One of the controls is null");
			}
			if (key.type == visitor.type)
			{
				if (!key.parameter.RemoveError(visitor.parameter))
				{
					return new SystemThread(isparam: false, "Parameter does not match", 2);
				}
				VRCExpressionsMenu.Control.ControlType type = key.type;
				if (type == VRCExpressionsMenu.Control.ControlType.SubMenu)
				{
					if (key.subMenu != visitor.subMenu)
					{
						return new SystemThread(isparam: false, "SubMenus do not match", 3);
					}
				}
				else if ((uint)(type - 201) <= 1u)
				{
					if (!key.subParameters.GetRules() || !visitor.subParameters.GetRules() || key.subParameters?.Length != visitor.subParameters?.Length)
					{
						return new SystemThread(isparam: false, "SubParameters do not match", 4);
					}
					if (key.subParameters != null && visitor.subParameters != null)
					{
						for (int i = 0; i < key.subParameters.Length; i++)
						{
							if (!key.subParameters[i].RemoveError(visitor.subParameters[i]))
							{
								return new SystemThread(isparam: false, "SubParameters do not match", 4);
							}
						}
					}
				}
				return true;
			}
			return new SystemThread(isparam: false, "Control types do not match", 1);
		}
		return true;
	}

	internal static bool RemoveError(this VRCExpressionsMenu.Control.Parameter spec, VRCExpressionsMenu.Control.Parameter caller)
	{
		if (!((spec == null) ^ (caller == null)))
		{
			if (spec != null && (!string.IsNullOrEmpty(spec.name) || !string.IsNullOrEmpty(caller.name)))
			{
				return spec.name == caller.name;
			}
			return true;
		}
		return false;
	}

	[CompilerGenerated]
	internal static T InstantiateError<T>(T def, ref _003C_003Ec__DisplayClass128_0 cfg) where T : UnityEngine.Object
	{
		if ((bool)def)
		{
			if (cfg.iteratorObserver.TryGetValue(def, out var value))
			{
				return (T)value;
			}
			T val = CompareRules(def);
			AssetDatabase.AddObjectToAsset(val, cfg.m_PublisherObserver);
			if (cfg.configurationObserver)
			{
				Undo.RegisterCreatedObjectUndo(val, "Copy Layer");
			}
			val.hideFlags = def.hideFlags;
			cfg.iteratorObserver.Add(def, val);
			cfg._ProcObserver.Add(val, def);
			return val;
		}
		return null;
	}

	[CompilerGenerated]
	internal static T AwakeError<T>(T setup, ref _003C_003Ec__DisplayClass128_0 counter) where T : Motion
	{
		if (setup is UnityEditor.Animations.BlendTree blendTree && AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(blendTree)) != blendTree)
		{
			UnityEditor.Animations.BlendTree blendTree2 = InstantiateError(blendTree, ref counter);
			ChildMotion[] children = blendTree2.children;
			for (int i = 0; i < blendTree2.children.Length; i++)
			{
				children[i].motion = AwakeError(children[i].motion, ref counter);
			}
			blendTree2.children = children;
			EditorUtility.SetDirty(blendTree2);
			setup = blendTree2 as T;
		}
		return setup;
	}

	[CompilerGenerated]
	internal static T[] ResetError<T>(T[] value, ref _003C_003Ec__DisplayClass128_0 reg) where T : UnityEngine.Object
	{
		T[] array = new T[value.Length];
		for (int i = 0; i < value.Length; i++)
		{
			array[i] = InstantiateError(value[i], ref reg);
		}
		return array;
	}

	[CompilerGenerated]
	internal static T[] FlushError<T>(T[] v, ref _003C_003Ec__DisplayClass128_0 cont) where T : AnimatorTransitionBase
	{
		T[] array = ResetError(v, ref cont);
		T[] array2 = array;
		foreach (T val in array2)
		{
			val.destinationState = InstantiateError(val.destinationState, ref cont);
			val.destinationStateMachine = InstantiateError(val.destinationStateMachine, ref cont);
		}
		return array;
	}

	[CompilerGenerated]
	internal static AnimatorStateMachine ConnectError(AnimatorStateMachine key, ref _003C_003Ec__DisplayClass128_0 connection)
	{
		AnimatorStateMachine animatorStateMachine = InstantiateError(key, ref connection);
		if (!animatorStateMachine)
		{
			return null;
		}
		if (animatorStateMachine.name == connection.m_WrapperServer.name)
		{
			animatorStateMachine.name = connection.annotationServer.name;
		}
		animatorStateMachine.behaviours = ResetError(key.behaviours, ref connection);
		ChildAnimatorStateMachine[] stateMachines = key.stateMachines;
		for (int i = 0; i < stateMachines.Length; i++)
		{
			stateMachines[i].stateMachine = ConnectError(key.stateMachines[i].stateMachine, ref connection);
		}
		animatorStateMachine.stateMachines = stateMachines;
		return animatorStateMachine;
	}

	[CompilerGenerated]
	internal static void CalculateError(AnimatorStateMachine ident, ref _003C_003Ec__DisplayClass128_0 token)
	{
		AnimatorStateMachine animatorStateMachine = (AnimatorStateMachine)token._ProcObserver[ident];
		ChildAnimatorState[] states = animatorStateMachine.states;
		for (int i = 0; i < states.Length; i++)
		{
			AnimatorState animatorState = InstantiateError(animatorStateMachine.states[i].state, ref token);
			if ((bool)animatorState)
			{
				states[i].state = animatorState;
				animatorState.behaviours = ResetError(animatorState.behaviours, ref token);
				if (animatorState.motion is UnityEditor.Animations.BlendTree blendTree && AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(blendTree)) != blendTree)
				{
					animatorState.motion = AwakeError(blendTree, ref token);
				}
			}
		}
		ident.states = states;
		ChildAnimatorStateMachine[] stateMachines = ident.stateMachines;
		foreach (ChildAnimatorStateMachine childAnimatorStateMachine in stateMachines)
		{
			CalculateError(childAnimatorStateMachine.stateMachine, ref token);
		}
	}

	[CompilerGenerated]
	internal static void TestError(AnimatorStateMachine item, ref _003C_003Ec__DisplayClass128_0 reg)
	{
		item.entryTransitions = FlushError(((AnimatorStateMachine)reg._ProcObserver[item]).entryTransitions, ref reg);
		item.anyStateTransitions = FlushError(((AnimatorStateMachine)reg._ProcObserver[item]).anyStateTransitions, ref reg);
		ChildAnimatorState[] states = item.states;
		for (int i = 0; i < states.Length; i++)
		{
			ChildAnimatorState childAnimatorState = states[i];
			childAnimatorState.state.transitions = FlushError(((AnimatorState)reg._ProcObserver[childAnimatorState.state]).transitions, ref reg);
		}
		ChildAnimatorStateMachine[] stateMachines = item.stateMachines;
		for (int i = 0; i < stateMachines.Length; i++)
		{
			ChildAnimatorStateMachine childAnimatorStateMachine = stateMachines[i];
			item.SetStateMachineTransitions(childAnimatorStateMachine.stateMachine, FlushError(item.GetStateMachineTransitions(childAnimatorStateMachine.stateMachine), ref reg));
		}
		stateMachines = item.stateMachines;
		foreach (ChildAnimatorStateMachine childAnimatorStateMachine2 in stateMachines)
		{
			TestError(childAnimatorStateMachine2.stateMachine, ref reg);
		}
		item.defaultState = InstantiateError(((AnimatorStateMachine)reg._ProcObserver[item]).defaultState, ref reg);
	}

	[CompilerGenerated]
	internal static bool MapError(UnityEngine.Object v)
	{
		if (PrefabUtility.GetPrefabAssetType(v) == PrefabAssetType.NotAPrefab)
		{
			return false;
		}
		PrefabUtility.RecordPrefabInstancePropertyModifications(v);
		return true;
	}

	[CompilerGenerated]
	internal static bool ValidateError(VRCAvatarDescriptor.CustomAnimLayer[] def, ref _003C_003Ec__DisplayClass446_0 map)
	{
		int num = 0;
		while (true)
		{
			if (num < def.Length)
			{
				if (def[num].type == map._PrinterServer)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		if ((bool)map._WriterServer)
		{
			map._ParamsServer.customizeAnimationLayers = true;
		}
		def[num].isDefault = !map._WriterServer;
		def[num].animatorController = map._WriterServer;
		EditorUtility.SetDirty(map._ParamsServer);
		return true;
	}

	internal static bool ComputeCandidate()
	{
		return RunCandidate == null;
	}
}
