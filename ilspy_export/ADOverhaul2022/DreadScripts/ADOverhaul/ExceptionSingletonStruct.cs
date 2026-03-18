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

internal static class ExceptionSingletonStruct
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

	internal class ExporterServerStub
	{
		private struct RegistryRegDic
		{
			internal PositionFlag _ItemSerializer;

			internal Rect m_IndexerSerializer;

			internal int poolSerializer;
		}

		private int _InvocationSerializer;

		private Vector2 _ListenerSerializer = Vector2.zero;

		private readonly int parserSerializer = GUIUtility.GetControlID("ResizeStateControlID".GetHashCode(), FocusType.Passive);

		public Action m_PrinterSerializer;

		public float m_RepositorySerializer;

		public float _DescriptorSerializer;

		public float strategySerializer;

		public float globalSerializer;

		private bool _ManagerSerializer;

		private bool _WorkerSerializer;

		internal static ExporterServerStub ConnectOrder;

		[SpecialName]
		public bool PatchRef()
		{
			return _ManagerSerializer;
		}

		[SpecialName]
		public void CheckRef(bool injectvar1)
		{
			if (_ManagerSerializer == injectvar1)
			{
				return;
			}
			_ManagerSerializer = injectvar1;
			if (!injectvar1)
			{
				return;
			}
			if (m_RepositorySerializer == 0f)
			{
				if (_DescriptorSerializer != 0f)
				{
					m_RepositorySerializer = _DescriptorSerializer;
				}
				else if (strategySerializer != 0f)
				{
					globalSerializer = strategySerializer;
				}
				else if (globalSerializer != 0f)
				{
					strategySerializer = globalSerializer;
				}
			}
			else
			{
				_DescriptorSerializer = m_RepositorySerializer;
			}
		}

		public ExporterServerStub(bool islast = false)
		{
			_ManagerSerializer = islast;
		}

		public void CancelRef()
		{
			m_RepositorySerializer = 0f;
			_DescriptorSerializer = 0f;
			strategySerializer = 0f;
			globalSerializer = 0f;
			m_PrinterSerializer?.Invoke();
		}

		public Rect LogoutRef(Rect last, PositionFlag map = PositionFlag.Middle, Rect filter = default(Rect))
		{
			if (filter == default(Rect))
			{
				filter = new Rect(-1f, -1f, -1f, -1f);
			}
			bool flag = filter.x != -1f && filter.width != -1f;
			bool flag2 = filter.y != -1f && filter.height != -1f;
			float num = 10f;
			float num2 = last.width + m_RepositorySerializer + _DescriptorSerializer;
			float num3 = last.height + strategySerializer + globalSerializer;
			float num4 = last.x - (num2 - last.width) * SelectRef(map);
			float num5 = last.y - (last.height + strategySerializer + globalSerializer - last.height) * WriteRef(map);
			last.x = (flag ? Mathf.Clamp(num4, filter.x, filter.x + filter.width - num) : num4);
			last.width = ((!flag) ? num2 : Mathf.Clamp(num2, num, filter.width - last.x));
			last.y = (flag2 ? Mathf.Clamp(num5, filter.y, filter.y + filter.height - num) : num5);
			last.height = (flag2 ? Mathf.Clamp(num3, num, filter.height - last.y) : num3);
			return last;
		}

		public void SetupRef(Rect ident, PositionFlag cust = PositionFlag.Right | PositionFlag.Left, PositionFlag c = PositionFlag.Middle, float counter2 = 4f)
		{
			Event current = Event.current;
			if (_WorkerSerializer && current.type == EventType.MouseUp)
			{
				if (GUIUtility.hotControl == parserSerializer)
				{
					GUIUtility.hotControl = 0;
				}
				CancelRef();
				current.Use();
				_WorkerSerializer = false;
			}
			float num = counter2 * 2f;
			RegistryRegDic[] array = new RegistryRegDic[8]
			{
				new RegistryRegDic
				{
					_ItemSerializer = PositionFlag.Left,
					poolSerializer = 0,
					m_IndexerSerializer = new Rect(ident.x - counter2, ident.y + counter2, num, ident.height - num)
				},
				new RegistryRegDic
				{
					_ItemSerializer = PositionFlag.TopLeft,
					poolSerializer = 1,
					m_IndexerSerializer = new Rect(ident.x - counter2, ident.y - counter2, num, num)
				},
				new RegistryRegDic
				{
					_ItemSerializer = PositionFlag.Top,
					poolSerializer = 2,
					m_IndexerSerializer = new Rect(ident.x + counter2, ident.y - counter2, ident.width - num, num)
				},
				new RegistryRegDic
				{
					_ItemSerializer = PositionFlag.TopRight,
					poolSerializer = 3,
					m_IndexerSerializer = new Rect(ident.x + ident.width - counter2, ident.y - counter2, num, num)
				},
				new RegistryRegDic
				{
					_ItemSerializer = PositionFlag.Right,
					poolSerializer = 4,
					m_IndexerSerializer = new Rect(ident.x + ident.width - counter2, ident.y + counter2, num, ident.height - num)
				},
				new RegistryRegDic
				{
					_ItemSerializer = PositionFlag.BottomRight,
					poolSerializer = 5,
					m_IndexerSerializer = new Rect(ident.x + ident.width - counter2, ident.y + ident.height - counter2, num, num)
				},
				new RegistryRegDic
				{
					_ItemSerializer = PositionFlag.Bottom,
					poolSerializer = 6,
					m_IndexerSerializer = new Rect(ident.x + counter2, ident.y + ident.height - counter2, ident.width - num, num)
				},
				new RegistryRegDic
				{
					_ItemSerializer = PositionFlag.BottomLeft,
					poolSerializer = 7,
					m_IndexerSerializer = new Rect(ident.x - counter2, ident.y + ident.height - counter2, num, num)
				}
			};
			bool flag = current.button == 0;
			RegistryRegDic[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RegistryRegDic registryRegDic = array2[i];
				if ((registryRegDic._ItemSerializer & cust) < registryRegDic._ItemSerializer)
				{
					continue;
				}
				MouseCursor selection;
				switch (registryRegDic._ItemSerializer)
				{
				case PositionFlag.Right:
				case PositionFlag.Left:
					selection = MouseCursor.ResizeHorizontal;
					break;
				default:
					selection = MouseCursor.Arrow;
					break;
				case PositionFlag.Top:
				case PositionFlag.Bottom:
					selection = MouseCursor.ResizeVertical;
					break;
				case PositionFlag.TopRight:
				case PositionFlag.BottomLeft:
					selection = MouseCursor.ResizeUpRight;
					break;
				case PositionFlag.TopLeft:
				case PositionFlag.BottomRight:
					selection = MouseCursor.ResizeUpLeft;
					break;
				}
				InsertStatus(registryRegDic.m_IndexerSerializer, selection);
				Rect indexerSerializer = registryRegDic.m_IndexerSerializer;
				if (collectionSerializer)
				{
					indexerSerializer.y += 46f;
				}
				if (flag && current.type == EventType.MouseDown && indexerSerializer.Contains(current.mousePosition))
				{
					if (current.clickCount == 2)
					{
						_WorkerSerializer = true;
					}
					_InvocationSerializer = registryRegDic.poolSerializer;
					GUIUtility.hotControl = parserSerializer;
					_ListenerSerializer = GUIUtility.GUIToScreenPoint(current.mousePosition);
					current.Use();
				}
			}
			if (current.type != EventType.MouseDrag || GUIUtility.hotControl != parserSerializer)
			{
				return;
			}
			PositionFlag itemSerializer = array[_InvocationSerializer]._ItemSerializer;
			Vector2 vector = GUIUtility.GUIToScreenPoint(current.mousePosition) - _ListenerSerializer;
			if (_WorkerSerializer)
			{
				if (!(vector.sqrMagnitude > new Vector2(15f, 15f).sqrMagnitude))
				{
					return;
				}
				_WorkerSerializer = false;
			}
			if (vector != Vector2.zero)
			{
				switch (itemSerializer)
				{
				case PositionFlag.TopRight:
					_DescriptorSerializer += vector.x;
					if (PatchRef())
					{
						if (c.HasFlag(PositionFlag.Left))
						{
							_DescriptorSerializer -= vector.y;
						}
						else
						{
							m_RepositorySerializer -= vector.y;
						}
					}
					else
					{
						strategySerializer -= vector.y;
					}
					break;
				case PositionFlag.TopLeft:
					m_RepositorySerializer -= vector.x;
					if (!PatchRef())
					{
						strategySerializer -= vector.y;
					}
					else if (!c.HasFlag(PositionFlag.Bottom))
					{
						globalSerializer -= vector.x;
					}
					else
					{
						strategySerializer -= vector.x;
					}
					break;
				case PositionFlag.Right:
					_DescriptorSerializer += vector.x;
					if (PatchRef())
					{
						if (c.HasFlag(PositionFlag.Bottom))
						{
							strategySerializer += vector.x;
						}
						else
						{
							globalSerializer += vector.x;
						}
					}
					break;
				case PositionFlag.Bottom:
					globalSerializer += vector.y;
					if (PatchRef())
					{
						if (!c.HasFlag(PositionFlag.Left))
						{
							m_RepositorySerializer += vector.y;
						}
						else
						{
							_DescriptorSerializer += vector.y;
						}
					}
					break;
				case PositionFlag.Left:
					m_RepositorySerializer -= vector.x;
					if (PatchRef())
					{
						if (c.HasFlag(PositionFlag.Bottom))
						{
							strategySerializer -= vector.x;
						}
						else
						{
							globalSerializer -= vector.x;
						}
					}
					break;
				case PositionFlag.BottomRight:
					_DescriptorSerializer += vector.x;
					if (PatchRef())
					{
						if (!c.HasFlag(PositionFlag.Top))
						{
							strategySerializer += vector.x;
						}
						else
						{
							globalSerializer += vector.x;
						}
					}
					else
					{
						globalSerializer += vector.y;
					}
					break;
				case PositionFlag.BottomLeft:
					m_RepositorySerializer -= vector.x;
					if (PatchRef())
					{
						if (c.HasFlag(PositionFlag.Bottom))
						{
							strategySerializer += vector.x;
						}
						else
						{
							globalSerializer += vector.x;
						}
					}
					else
					{
						globalSerializer += vector.y;
					}
					break;
				case PositionFlag.Top:
					strategySerializer -= vector.y;
					if (PatchRef())
					{
						if (c.HasFlag(PositionFlag.Left))
						{
							_DescriptorSerializer -= vector.y;
						}
						else
						{
							m_RepositorySerializer -= vector.y;
						}
					}
					break;
				}
				m_PrinterSerializer?.Invoke();
			}
			_ListenerSerializer = GUIUtility.GUIToScreenPoint(current.mousePosition);
		}

		public static float SelectRef(PositionFlag def, bool wantresult = false)
		{
			if (wantresult)
			{
				if (def.CountProcess())
				{
					return 0f;
				}
				if (def.StartProcess())
				{
					return 1f;
				}
			}
			else
			{
				if (def.CountProcess())
				{
					return 1f;
				}
				if (def.StartProcess())
				{
					return 0f;
				}
			}
			return 0.5f;
		}

		public static float WriteRef(PositionFlag i, bool excludemap = false)
		{
			bool flag = i.RemoveProcess();
			bool flag2 = i.ReflectProcess();
			if (!excludemap)
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

		internal static bool RegisterOrder()
		{
			return ConnectOrder == null;
		}
	}

	internal class SystemSerializer : IDisposable
	{
		public readonly bool setterSerializer;

		public readonly bool _RuleSerializer = true;

		private readonly Rect m_StructSerializer;

		internal static SystemSerializer InstantiateDescriptor;

		public SystemSerializer(SceneView ident, string result, float tag, int first2_end, float value3 = 20f, PositionFlag first4 = PositionFlag.BottomRight, ExporterServerStub reference5 = null)
			: this(ident, tag, first2_end + 2, value3, first4, reference5)
		{
			GUILayout.Label(result, MapRef().m_WriterSerializer);
			DisableStatus(2, 0);
		}

		public SystemSerializer(SceneView value, float visitor, int offsetdir, float res2 = 20f, PositionFlag ord3 = PositionFlag.BottomRight, ExporterServerStub init4 = null)
		{
			Handles.BeginGUI();
			Rect rect = value.AddStatus();
			Rect filter = new Rect(rect)
			{
				x = rect.x + 4f,
				y = rect.y + 4f,
				width = rect.width - 8f,
				height = rect.height - 8f
			};
			Rect rect2 = RegisterRef(rect, visitor, offsetdir, res2, ord3, setterSerializer);
			if (init4 != null)
			{
				rect2 = init4.LogoutRef(rect2, ord3, filter);
				init4.SetupRef(rect2, ord3.ResolveProcess(evaluateivk: true));
			}
			m_StructSerializer = ResetProcess(rect2);
			if (collectionSerializer)
			{
				m_StructSerializer.y += 46f;
			}
			GUILayout.BeginArea(m_StructSerializer);
		}

		public SystemSerializer(SceneView instance, float col, float role = 20f, PositionFlag vis2 = PositionFlag.BottomRight, ExporterServerStub x3 = null)
			: this(instance, col, 1, role, vis2, x3)
		{
		}

		public void Dispose()
		{
			if (_RuleSerializer)
			{
				Event current = Event.current;
				if (current.type == EventType.MouseDown && !m_StructSerializer.Contains(current.mousePosition))
				{
					current.Use();
					GUIUtility.hotControl = 0;
				}
			}
			GUILayout.EndArea();
			Handles.EndGUI();
		}

		private static Rect RegisterRef(Rect last, float cfg, int column_dic, float reg2 = 20f, PositionFlag def3 = PositionFlag.Bottom, bool loadfirst4 = false)
		{
			Rect result = last;
			last.x += 4f;
			last.width -= 8f;
			float num = (loadfirst4 ? (cfg * last.width / 100f) : cfg);
			float num2 = (float)column_dic * reg2;
			bool flag = def3.CountProcess();
			bool num3 = def3.StartProcess();
			bool flag2 = def3.RemoveProcess();
			bool flag3 = def3.ReflectProcess();
			float x = (num3 ? last.x : (flag ? (last.x + last.width - num) : (last.x + last.width / 2f - num / 2f)));
			float y = (flag2 ? last.y : ((!flag3) ? (last.y + last.height / 2f - num2 / 2f) : (last.y + last.height - num2)));
			result.x = x;
			result.y = y;
			result.width = num;
			result.height = num2;
			return result;
		}

		internal static bool VisitDescriptor()
		{
			return InstantiateDescriptor == null;
		}
	}

	internal class InterpreterSerializer
	{
		internal readonly SchemaMapping _ParameterSerializer = NewVal("CollabConflict Icon", "ds-icon-updateAvailable", "Update Available");

		internal readonly SchemaMapping m_AttrSerializer = NewVal("Refresh", "ds-icon-checkForUpdate", "Check For Update");

		internal readonly SchemaMapping _ObjectSerializer = NewVal("console.infoicon.sml", "ds-icon-announcement");

		internal readonly SchemaMapping m_ServiceSerializer = NewVal("console.warnicon.sml", "ds-icon-warning");

		internal readonly SchemaMapping reponseSerializer = NewVal("console.erroricon.sml", "ds-icon-error");

		internal readonly SchemaMapping m_SpecificationSerializer = NewVal("VerticalLayoutGroup Icon", "ds-icon-hamMenu");

		internal readonly SchemaMapping wrapperSerializer = NewVal("_Help", "ds-icon-help");

		internal readonly GUIContent infoSerializer = ManageStatus("TestPassed", "Up to Date!");

		internal readonly GUIContent _ModelSerializer = ManageStatus("UnityEditor.InspectorWindow");

		internal readonly GUIContent _ConfigSerializer = ManageStatus("Refresh", "Reset");

		internal readonly GUIContent m_MockSerializer = ManageStatus("FolderOpened Icon", "Select a folder");

		internal readonly GUIContent stateSerializer = ManageStatus("editicon.sml");

		internal readonly GUIContent fieldSerializer = ManageStatus("settings");

		internal readonly GUIContent advisorSerializer = ManageStatus("Selectable Icon");

		internal readonly GUIContent m_ExporterSerializer = ManageStatus("eyeDropper.Large");

		internal readonly GUIContent _CreatorSerializer = ManageStatus("Toolbar Minus", "Remove selection from list");

		internal readonly GUIContent m_DispatcherSerializer = ManageStatus("CollabCreate Icon");

		internal readonly GUIContent connectionSerializer = ManageStatus("IN LockButton");

		internal readonly GUIContent expressionSerializer = ManageStatus("IN LockButton on");

		internal readonly GUIContent decoratorSerializer = ManageStatus("d_scenepicking_pickable_hover@2x");

		internal readonly GUIContent _ParamSerializer = ManageStatus("d_scenepicking_notpickable@2x");

		internal readonly GUIContent prototypeSerializer = ManageStatus("d_CustomTool@2x");

		internal readonly GUIContent baseSerializer = new GUIContent("X", "Clear");

		internal readonly GUIContent m_RequestSerializer = new GUIContent("Handle Size", "The size multiplier of the custom ADO gizmos");

		internal readonly GUIContent issuerSerializer = new GUIContent("Animated Foldouts", "Enable animated foldouts in the editor");

		internal readonly GUIContent _FacadeSerializer = new GUIContent("Show Name Labels", "Show names of transforms when toggling or selecting");

		internal readonly GUIContent composerSerializer = new GUIContent("Label Color", "The color of text displayed in the scene view");

		internal readonly GUIContent annotationSerializer = new GUIContent("General Color", "The color of the handles used for editing");

		internal readonly GUIContent m_CodeSerializer = new GUIContent("Active Color", "The color of handles that are selected");

		internal readonly GUIContent _CallbackSerializer = new GUIContent("Inactive Color", "The color of handles that are not selected");

		internal readonly GUIContent _MessageSerializer = new GUIContent("Mixed Color", "The color of handles that are active in some of the currently selected PhysBones but not others");

		internal readonly GUIContent policySerializer = new GUIContent("Selection Color", "The color of handles when selecting");

		internal readonly GUIContent _MapperSerializer = new GUIContent("Function", "What you'd like to set up on the avatar");

		internal readonly GUIContent mappingSerializer = new GUIContent("Property & Tip Overlay", "Displays the overlay for tooltips and property selection on the scene view");

		internal readonly GUIContent queueSerializer = new GUIContent("Tooltips", "Displays tooltips on how to use the current tool");

		internal static InterpreterSerializer RateDescriptor;

		internal InterpreterSerializer()
		{
			decoratorSerializer.tooltip = "Scene view clicks are allowed while editing.";
			_ParamSerializer.tooltip = "Scene view clicks are ignored while editing.";
			fieldSerializer.tooltip = "Open ADO Settings";
			m_ExporterSerializer.tooltip = "Copy from another component of the same type";
			advisorSerializer.tooltip = "Select through the scene view";
			stateSerializer.tooltip = "Edit through the scene view";
		}

		internal static bool NewDescriptor()
		{
			return RateDescriptor == null;
		}
	}

	internal class CreatorServerStub
	{
		internal static readonly Color processorSerializer = new Color(0.357f, 0.357f, 0.357f);

		internal readonly GUILayoutOption[] _TokenizerSerializer = new GUILayoutOption[2]
		{
			GUILayout.Width(EditorGUIUtility.singleLineHeight),
			GUILayout.Height(EditorGUIUtility.singleLineHeight)
		};

		internal readonly GUIStyle m_ExceptionSerializer = new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold,
			fontSize = 18,
			alignment = TextAnchor.MiddleLeft
		};

		internal readonly GUIStyle valueSerializer = new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold,
			fontSize = 14,
			alignment = TextAnchor.MiddleLeft
		};

		internal readonly GUIStyle m_ErrorSerializer = new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold,
			fontSize = 12,
			alignment = TextAnchor.MiddleLeft
		};

		internal readonly GUIStyle m_ProducerSerializer = new GUIStyle(GUI.skin.label)
		{
			padding = new RectOffset(1, 1, 1, 1),
			fixedWidth = 18f,
			fixedHeight = 18f
		};

		internal readonly GUIStyle templateSerializer = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleCenter,
			richText = true
		};

		internal readonly GUIStyle m_WriterSerializer = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Bold,
			richText = true
		};

		internal readonly GUIStyle _ClassSerializer = new GUIStyle(GUI.skin.label)
		{
			padding = new RectOffset(),
			margin = new RectOffset(1, 1, 1, 1)
		};

		internal readonly GUIStyle dicSerializer = new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold,
			richText = true,
			wordWrap = true
		};

		internal readonly GUIStyle m_ContainerSerializer = new GUIStyle(GUI.skin.button)
		{
			fontSize = 18,
			fontStyle = FontStyle.Bold
		};

		internal readonly GUIStyle _SchemaSerializer = new GUIStyle(GUI.skin.label)
		{
			name = "Toggle"
		};

		internal readonly GUIStyle _BridgeSerializer = new GUIStyle(GUI.skin.label)
		{
			richText = true
		};

		internal readonly GUIStyle publisherSerializer = "AssetLabel";

		internal readonly GUIStyle _MerchantSerializer = "in bigtitle";

		internal readonly GUIStyle m_ProcSerializer = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = (EditorGUIUtility.isProSkin ? Color.gray : processorSerializer)
			}
		};

		internal readonly GUIStyle configurationMethod = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = ((!EditorGUIUtility.isProSkin) ? processorSerializer : Color.gray)
			}
		};

		internal readonly GUIStyle _IdentifierMethod = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleRight,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = ((!EditorGUIUtility.isProSkin) ? processorSerializer : Color.gray)
			}
		};

		internal readonly GUIStyle m_AuthenticationMethod = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = (EditorGUIUtility.isProSkin ? Color.gray : processorSerializer)
			},
			contentOffset = new Vector2(-3f, 1.5f)
		};

		internal readonly GUIStyle _ContextMethod = new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleLeft,
			fontStyle = FontStyle.Italic,
			richText = true,
			fontSize = 11,
			normal = 
			{
				textColor = ((!EditorGUIUtility.isProSkin) ? processorSerializer : Color.gray)
			},
			name = "Toggle",
			hover = 
			{
				textColor = new Color(0.3f, 0.7f, 1f)
			}
		};

		internal readonly Color[] m_SerializerMethod = new Color[3] { _BroadcasterSerializer, _ObserverSerializer, _EventSerializer };

		internal readonly GUIStyle methodMethod = new GUIStyle(GUI.skin.button)
		{
			margin = new RectOffset(0, 0, 2, 0),
			padding = new RectOffset(1, 1, 1, 1)
		};

		internal readonly GUIStyle _ConsumerMethod = new GUIStyle(GUI.skin.label)
		{
			stretchWidth = true,
			fontSize = 15,
			richText = true,
			margin = new RectOffset(10, 0, 0, 0),
			fontStyle = FontStyle.Bold
		};

		internal readonly GUIStyle utilsMethod = new GUIStyle("RL FooterButton");

		internal static CreatorServerStub ChangeDescriptor;

		internal static bool SetupDescriptor()
		{
			return ChangeDescriptor == null;
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

	internal struct PageMethod
	{
		internal string m_PropertyMethod;

		internal GUIStyle _SingletonMethod;

		internal Vector3 m_AccountMethod;

		internal Quaternion _ParamsMethod;

		internal Vector3 m_ImporterMethod;

		internal float serverMethod;

		internal float[] watcherMethod;

		internal int regMethod;

		internal Action processMethod;

		internal Func<PageMethod, float[]> _StatusMethod;

		internal Action<PageMethod> _ValMethod;

		internal static object PushDescriptor;

		internal static PageMethod OrderComparator(Vector3 config, string reg = "", float pool = 0.05f, int int_0 = -1, Action reference3 = null)
		{
			return new PageMethod
			{
				_ValMethod = DeleteComparator,
				_SingletonMethod = new GUIStyle(EditorStyles.boldLabel),
				_StatusMethod = (PageMethod sc) => new float[1] { HandleUtility.DistanceToCircle(sc.m_AccountMethod, sc.serverMethod / 2f) },
				m_AccountMethod = config,
				serverMethod = pool,
				m_PropertyMethod = reg,
				regMethod = int_0,
				processMethod = reference3
			};
		}

		internal void CalculateComparator()
		{
			_ValMethod(this);
		}

		internal float[] CalcComparator()
		{
			return _StatusMethod(this);
		}

		internal static void DeleteComparator(PageMethod v)
		{
			Handles.SphereHandleCap(v.regMethod, v.m_AccountMethod, Quaternion.identity, v.serverMethod, EventType.Repaint);
			if (!string.IsNullOrWhiteSpace(v.m_PropertyMethod))
			{
				FindStatus(v.m_PropertyMethod, v.m_AccountMethod, v.serverMethod, v._SingletonMethod);
			}
		}

		internal static bool SortDescriptor()
		{
			return PushDescriptor == null;
		}
	}

	internal sealed class ParserWatcherRule
	{
		private Texture2D m_RefMethod;

		private bool comparatorMethod = true;

		private readonly string _ProductMethod;

		private readonly bool iteratorMethod;

		private readonly string m_PredicateMethod;

		internal bool _CollectionMethod;

		internal bool m_InterceptorMethod;

		private bool registryMethod;

		private bool m_ClientMethod;

		private static ParserWatcherRule ResetDescriptor;

		[SpecialName]
		internal Texture2D PrepareComparator()
		{
			if (_CollectionMethod)
			{
				if (comparatorMethod && !m_RefMethod)
				{
					InvokeComparator();
				}
				return m_RefMethod;
			}
			if (m_InterceptorMethod)
			{
				return null;
			}
			if (!iteratorMethod || registryMethod)
			{
				return null;
			}
			registryMethod = true;
			m_InterceptorMethod = true;
			SortComparator();
			return null;
		}

		internal ParserWatcherRule(string v, bool addcol, string res, bool striplast2 = false)
		{
			_ProductMethod = v;
			iteratorMethod = addcol;
			m_PredicateMethod = res;
		}

		internal void SortComparator()
		{
			if (InvokeComparator())
			{
				return;
			}
			UnityWebRequest observerMethod = new UnityWebRequest(_ProductMethod)
			{
				downloadHandler = new DownloadHandlerBuffer()
			};
			observerMethod.SendWebRequest().completed += delegate
			{
				if (!observerMethod.isDone || observerMethod.isHttpError || observerMethod.isNetworkError)
				{
					observerMethod.Dispose();
					return;
				}
				try
				{
					byte[] data = observerMethod.downloadHandler.data;
					m_RefMethod = new Texture2D(0, 0);
					m_RefMethod.LoadImage(data);
					m_RefMethod.Apply();
					_CollectionMethod = true;
					if (!string.IsNullOrWhiteSpace(m_PredicateMethod))
					{
						SchemaMapping.ConnectComparator(data, m_PredicateMethod);
						comparatorMethod = true;
					}
				}
				finally
				{
					observerMethod.Dispose();
				}
			};
			m_InterceptorMethod = false;
		}

		internal bool InvokeComparator()
		{
			if (comparatorMethod && !string.IsNullOrWhiteSpace(m_PredicateMethod))
			{
				comparatorMethod = false;
				Texture2D texture2D = SchemaMapping.InitComparator(m_PredicateMethod);
				if (texture2D != null)
				{
					m_RefMethod = texture2D;
					_CollectionMethod = true;
					m_InterceptorMethod = false;
					comparatorMethod = true;
				}
			}
			return m_RefMethod;
		}

		internal void CustomizeComparator()
		{
			if (CancelComparator())
			{
				Rect aspectRect = GUILayoutUtility.GetAspectRect((float)PrepareComparator().width / (float)PrepareComparator().height);
				FillComparator(aspectRect);
			}
		}

		internal void ConcatComparator(EditorWindow init, float visitor = 0f, float tag = 60f)
		{
			if (CancelComparator())
			{
				if (init == null)
				{
					CustomizeComparator();
				}
				else
				{
					MapComparator(init.position.width, init.position.height, visitor, tag);
				}
			}
		}

		internal void MapComparator(float res, float counter, float consumer = 0f, float second2 = 60f)
		{
			float num = (float)PrepareComparator().height / (float)PrepareComparator().width;
			float num2 = res;
			float num3 = num2 * num;
			float num4 = counter - second2;
			if (num3 > num4)
			{
				num3 = num4;
				num2 = num3 / num;
			}
			Rect rect = GUILayoutUtility.GetRect(num2, num3, GUILayout.ExpandWidth(expand: false));
			rect.x += (res - num2) / 2f + consumer;
			FillComparator(rect);
		}

		private void FillComparator(Rect param)
		{
			Event current = Event.current;
			switch (current.type)
			{
			case EventType.MouseDown:
				if (param.Contains(current.mousePosition) && current.button == 0)
				{
					Application.OpenURL("https://dreadrith.com/links");
					current.Use();
				}
				break;
			}
			if (Event.current.type == EventType.Repaint)
			{
				EditorGUIUtility.AddCursorRect(param, MouseCursor.Link);
			}
			GUI.DrawTexture(param, PrepareComparator());
		}

		internal bool CancelComparator()
		{
			if (m_ClientMethod)
			{
				return true;
			}
			if (PrepareComparator() == null)
			{
				return false;
			}
			if (Event.current.type == EventType.Layout)
			{
				m_ClientMethod = true;
			}
			return true;
		}

		internal static bool ForgotDescriptor()
		{
			return ResetDescriptor == null;
		}
	}

	internal sealed class EventMethod : IDisposable
	{
		internal bool m_RecordMethod;

		internal Texture2D m_ResolverMethod;

		private static EventMethod ResolveDescriptor;

		internal EventMethod(Texture2D spec)
		{
			try
			{
				spec.GetPixel(0, 0);
				m_RecordMethod = false;
				m_ResolverMethod = spec;
			}
			catch
			{
				int width = spec.width;
				int height = spec.height;
				m_RecordMethod = true;
				spec.filterMode = FilterMode.Point;
				RenderTexture temporary = RenderTexture.GetTemporary(width, height);
				temporary.filterMode = FilterMode.Point;
				RenderTexture.active = temporary;
				Graphics.Blit(spec, temporary);
				Texture2D texture2D = new Texture2D(width, height);
				texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
				RenderTexture.active = null;
				m_ResolverMethod = texture2D;
			}
		}

		public void Dispose()
		{
			if (m_RecordMethod)
			{
				UnityEngine.Object.DestroyImmediate(m_ResolverMethod);
			}
		}

		public static implicit operator Texture2D(EventMethod param)
		{
			return param.m_ResolverMethod;
		}

		internal static bool CountDescriptor()
		{
			return ResolveDescriptor == null;
		}
	}

	internal sealed class SchemaMapping
	{
		private bool _TagMethod = true;

		private GUIContent m_FilterMethod;

		private Texture2D factoryMethod;

		private readonly string _AttributeMethod;

		private readonly string instanceMethod;

		private static SchemaMapping WriteDescriptor;

		[SpecialName]
		private GUIContent NewProduct()
		{
			if (m_FilterMethod.image == null && _TagMethod)
			{
				m_FilterMethod = new GUIContent(VerifyProduct())
				{
					tooltip = instanceMethod
				};
			}
			return m_FilterMethod;
		}

		[SpecialName]
		internal Texture2D VerifyProduct()
		{
			if (_TagMethod && factoryMethod == null)
			{
				_TagMethod = false;
				GetComparator();
				_TagMethod = factoryMethod != null;
			}
			return factoryMethod;
		}

		public SchemaMapping(Texture2D last, string ivk, string proc = "")
		{
			factoryMethod = last;
			_AttributeMethod = ivk;
			instanceMethod = proc;
			if (!(factoryMethod == null))
			{
				ConnectComparator(last.EncodeToPNG(), ivk);
			}
			else
			{
				GetComparator();
			}
			m_FilterMethod = new GUIContent(last)
			{
				tooltip = proc
			};
		}

		private void GetComparator()
		{
			factoryMethod = InitComparator(_AttributeMethod);
		}

		private static byte[] FlushComparator(int[] info)
		{
			byte[] array = new byte[info.Length];
			for (int i = 0; i < info.Length; i++)
			{
				array[i] = (byte)info[i];
			}
			return array;
		}

		private static int[] ExcludeComparator(byte[] asset)
		{
			int num = asset.Length;
			int[] array = new int[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = asset[i];
			}
			return array;
		}

		internal static Texture2D InitComparator(string item)
		{
			int[] intArray = SessionState.GetIntArray(item, null);
			if (intArray != null)
			{
				try
				{
					byte[] data = FlushComparator(intArray);
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

		internal static void ConnectComparator(byte[] setup, string vis)
		{
			int[] value = ExcludeComparator(setup);
			SessionState.SetIntArray(vis, value);
		}

		public static implicit operator GUIContent(SchemaMapping param)
		{
			return param.NewProduct();
		}

		internal static bool CustomizeDescriptor()
		{
			return WriteDescriptor == null;
		}
	}

	internal struct TaskMethod
	{
		internal readonly UnityEngine.Object customerMethod;

		internal bool m_DatabaseMethod;

		internal readonly Transform m_HelperMethod;

		internal readonly int _CandidateMethod;

		internal float m_ReaderMethod;

		internal float m_StubMethod;

		internal Vector3 _RulesMethod;

		internal Quaternion m_TestsMethod;

		private static object ValidateDescriptor;

		internal TaskMethod(VRCPhysBoneColliderBase ident)
		{
			customerMethod = ident;
			m_DatabaseMethod = true;
			m_HelperMethod = ident.GetRootTransform();
			_CandidateMethod = (int)ident.shapeType;
			m_ReaderMethod = ident.radius;
			m_StubMethod = ident.height;
			_RulesMethod = ident.position;
			m_TestsMethod = ident.rotation;
		}

		internal TaskMethod(ContactBase var1)
		{
			customerMethod = var1;
			m_DatabaseMethod = false;
			m_HelperMethod = var1.GetRootTransform();
			_CandidateMethod = (int)var1.shapeType;
			m_ReaderMethod = var1.radius;
			m_StubMethod = var1.height;
			_RulesMethod = var1.position;
			m_TestsMethod = var1.rotation;
		}

		internal void SortProduct()
		{
			if (m_DatabaseMethod)
			{
				VRCPhysBoneColliderBase obj = (VRCPhysBoneColliderBase)customerMethod;
				obj.radius = m_ReaderMethod;
				obj.height = m_StubMethod;
				obj.position = _RulesMethod;
				obj.rotation = m_TestsMethod;
			}
			else
			{
				ContactBase obj2 = (ContactBase)customerMethod;
				obj2.radius = m_ReaderMethod;
				obj2.height = m_StubMethod;
				obj2.position = _RulesMethod;
				obj2.rotation = m_TestsMethod;
				obj2.shapeType = (ContactBase.ShapeType)_CandidateMethod;
			}
		}

		internal void InvokeProduct(ContactBase instance)
		{
			instance.radius = m_ReaderMethod;
			instance.height = m_StubMethod;
			instance.position = _RulesMethod;
			instance.rotation = m_TestsMethod;
			instance.shapeType = (ContactBase.ShapeType)_CandidateMethod;
		}

		internal void CustomizeProduct(VRCPhysBoneCollider reference)
		{
			reference.radius = m_ReaderMethod;
			reference.height = m_StubMethod;
			reference.position = _RulesMethod;
			while (true)
			{
				reference.rotation = m_TestsMethod;
			}
		}

		internal static bool EnableDescriptor()
		{
			return ValidateDescriptor == null;
		}
	}

	internal class StrategyAuthenticationFactory
	{
		internal readonly VRCPhysBone definitionMethod;

		internal readonly Transform m_InitializerMethod;

		internal readonly List<ClientRegDic> m_TokenMethod;

		internal readonly int _GetterMethod;

		internal List<List<ClientRegDic>> threadMethod;

		private static StrategyAuthenticationFactory InvokeDescriptor;

		[SpecialName]
		internal IEnumerable<Matrix4x4> InterruptProduct()
		{
			return m_TokenMethod.Select((ClientRegDic b) => b.printerMethod);
		}

		internal StrategyAuthenticationFactory(VRCPhysBone instance)
		{
			definitionMethod = instance;
			m_InitializerMethod = instance.GetRootTransform();
			m_TokenMethod = new List<ClientRegDic>();
			FillProduct(m_InitializerMethod, 0);
			_GetterMethod = m_TokenMethod.Max((ClientRegDic b) => b.m_StrategyMethod);
		}

		internal void FillProduct(Transform v, int next_cust)
		{
			bool flag = false;
			ClientRegDic clientRegDic = new ClientRegDic();
			ClientRegDic globalMethod = null;
			ClientRegDic clientRegDic2 = null;
			Quaternion q = v.rotation;
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < v.childCount; i++)
			{
				Transform child = v.GetChild(i);
				if (!definitionMethod.ignoreTransforms.Contains(child))
				{
					list.Add(child);
				}
			}
			bool descriptorMethod;
			if (!(descriptorMethod = list.Count == 0))
			{
				if (list.Count > 1)
				{
					if (definitionMethod.multiChildType == VRCPhysBoneBase.MultiChildType.Average)
					{
						Vector3 zero = Vector3.zero;
						foreach (Transform item in list)
						{
							zero += item.position;
						}
						zero /= (float)list.Count;
						Vector3 toDirection = zero - v.position;
						q = v.rotation * Quaternion.FromToRotation(v.up, toDirection);
						clientRegDic2 = (globalMethod = new ClientRegDic
						{
							m_InvocationMethod = this,
							m_ListenerMethod = m_InitializerMethod,
							printerMethod = Matrix4x4.TRS(zero, q, v.lossyScale),
							m_StrategyMethod = next_cust + 1,
							m_RepositoryMethod = true,
							m_DescriptorMethod = true,
							managerMethod = clientRegDic
						});
					}
					else if (definitionMethod.multiChildType == VRCPhysBoneBase.MultiChildType.Ignore)
					{
						flag = true;
					}
				}
			}
			else if (!(definitionMethod.endpointPosition != Vector3.zero))
			{
				if (m_TokenMethod.Count != 0)
				{
					q = m_TokenMethod[m_TokenMethod.Count - 1].printerMethod.rotation;
				}
			}
			else
			{
				Vector3 pos = v.TransformPoint(definitionMethod.endpointPosition);
				q = v.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(definitionMethod.endpointPosition));
				ClientRegDic obj = new ClientRegDic
				{
					m_InvocationMethod = this,
					m_ListenerMethod = m_InitializerMethod,
					printerMethod = Matrix4x4.TRS(pos, q, v.lossyScale),
					m_StrategyMethod = next_cust + 1,
					m_RepositoryMethod = true,
					m_DescriptorMethod = true,
					managerMethod = clientRegDic
				};
				globalMethod = obj;
				clientRegDic2 = obj;
			}
			if (!flag)
			{
				clientRegDic.m_InvocationMethod = this;
				clientRegDic.m_ListenerMethod = m_InitializerMethod;
				clientRegDic.parserMethod = v;
				clientRegDic.printerMethod = Matrix4x4.TRS(v.position, q, v.lossyScale);
				clientRegDic.m_StrategyMethod = next_cust;
				clientRegDic.m_DescriptorMethod = descriptorMethod;
				clientRegDic._GlobalMethod = globalMethod;
				ClientRegDic clientRegDic3 = m_TokenMethod.LastOrDefault();
				if (clientRegDic3 != null && !clientRegDic3.m_DescriptorMethod && clientRegDic3._GlobalMethod == null)
				{
					clientRegDic3._GlobalMethod = clientRegDic;
					clientRegDic.managerMethod = clientRegDic3;
				}
				m_TokenMethod.Add(clientRegDic);
			}
			if (clientRegDic2 != null)
			{
				m_TokenMethod.Add(clientRegDic2);
			}
			foreach (Transform item2 in list)
			{
				FillProduct(item2, next_cust + 1);
			}
		}

		internal void CancelProduct()
		{
			HashSet<ClientRegDic> hashSet = new HashSet<ClientRegDic>();
			threadMethod = new List<List<ClientRegDic>>();
			foreach (ClientRegDic item in m_TokenMethod)
			{
				if (!hashSet.Contains(item))
				{
					List<ClientRegDic> list = new List<ClientRegDic>();
					for (ClientRegDic clientRegDic = item; clientRegDic != null; clientRegDic = clientRegDic._GlobalMethod)
					{
						list.Add(clientRegDic);
						hashSet.Add(clientRegDic);
					}
					threadMethod.Add(list);
				}
			}
		}

		internal static bool ConcatDescriptor()
		{
			return InvokeDescriptor == null;
		}
	}

	internal class ClientRegDic
	{
		internal StrategyAuthenticationFactory m_InvocationMethod;

		internal Transform m_ListenerMethod;

		internal Transform parserMethod;

		internal Matrix4x4 printerMethod;

		internal bool m_RepositoryMethod;

		internal bool m_DescriptorMethod;

		internal int m_StrategyMethod;

		internal ClientRegDic _GlobalMethod;

		internal ClientRegDic managerMethod;

		internal static ClientRegDic DefineDescriptor;

		[SpecialName]
		internal Vector3 LoginProduct()
		{
			return printerMethod.GetColumn(3);
		}

		[SpecialName]
		internal float CheckProduct()
		{
			return Mathf.Max(printerMethod.lossyScale.x, printerMethod.lossyScale.y, printerMethod.lossyScale.z);
		}

		[SpecialName]
		internal float RegisterProduct()
		{
			return 1f / (float)m_InvocationMethod._GetterMethod * (float)m_StrategyMethod;
		}

		internal float ForgotProduct(AnimationCurve i)
		{
			if (i != null && i.length >= 2)
			{
				return i.Evaluate(RegisterProduct());
			}
			return 1f;
		}

		internal static bool TestDescriptor()
		{
			return DefineDescriptor == null;
		}
	}

	internal readonly struct InstanceConsumerExporter
	{
		internal readonly string workerMethod;

		internal readonly AnimatorControllerParameterType _ItemMethod;

		internal readonly bool _IndexerMethod;

		private readonly FieldInfo poolMethod;

		private static object CallDescriptor;

		internal InstanceConsumerExporter(string setup, AnimatorControllerParameterType token, string temp)
		{
			workerMethod = setup;
			_ItemMethod = token;
			poolMethod = ((!string.IsNullOrWhiteSpace(temp)) ? typeof(VRCPhysBoneBase).GetField(temp, BindingFlags.Instance | BindingFlags.Public) : null);
			_IndexerMethod = poolMethod != null;
		}

		internal float StopProduct(VRCPhysBoneBase config)
		{
			return (float)poolMethod.GetValue(config);
		}

		internal bool PushProduct(VRCPhysBoneBase def)
		{
			return (bool)poolMethod.GetValue(def);
		}

		public string PrepareProduct(VRCPhysBoneBase last)
		{
			if (_ItemMethod == AnimatorControllerParameterType.Bool)
			{
				return PushProduct(last).ToString();
			}
			return StopProduct(last).ToString();
		}

		internal static bool QueryDescriptor()
		{
			return CallDescriptor == null;
		}
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec m_SystemMethod = new _003C_003Ec();

		public static Func<ParameterInfo, Type> setterMethod;

		public static Func<Type, string> ruleMethod;

		public static Func<Type, string> structMethod;

		public static Func<VRCAvatarDescriptor.CustomAnimLayer, RuntimeAnimatorController> interpreterMethod;

		internal static _003C_003Ec VerifyDescriptor;

		internal Type DisableProduct(ParameterInfo p)
		{
			return p.ParameterType;
		}

		internal string VisitProduct(Type ht)
		{
			return ht.Name;
		}

		internal string AssetProduct(Type ht)
		{
			return ht.Name;
		}

		internal RuntimeAnimatorController PopProduct(VRCAvatarDescriptor.CustomAnimLayer l)
		{
			return l.animatorController;
		}

		internal static bool PublishDescriptor()
		{
			return VerifyDescriptor == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass19_0<T> where T : UnityEngine.Object
	{
		public Func<T, bool> acceptConditions;

		internal static object CancelState;

		internal bool CalculateIterator(T c)
		{
			if (!_003C_003Ec__DisplayClass19_0<T>.DeleteIterator((UnityEngine.Object)c, (UnityEngine.Object)null))
			{
				return false;
			}
			return acceptConditions?.Invoke(c) ?? true;
		}

		internal bool CalcIterator(T el)
		{
			return acceptConditions?.Invoke(el) ?? true;
		}

		internal static bool PrepareState()
		{
			return CancelState == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass20_0<T> where T : UnityEngine.Object
	{
		public Func<T, bool> acceptConditions;

		internal static object InstantiateState;

		internal bool DefineIterator(T c)
		{
			if (!_003C_003Ec__DisplayClass20_0<T>.DestroyIterator((UnityEngine.Object)c, (UnityEngine.Object)null))
			{
				return false;
			}
			return acceptConditions?.Invoke(c) ?? true;
		}

		internal static bool VisitState()
		{
			return InstantiateState == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass24_0<T> where T : UnityEngine.Object
	{
		public T[] enumerable;

		private static object ChangeState;

		internal void CompareIterator(SerializedProperty sp)
		{
			T[] array = enumerable;
			for (int i = 0; i < array.Length; i++)
			{
				_003C_003Ec__DisplayClass24_1<T> _003C_003Ec__DisplayClass24_ = new _003C_003Ec__DisplayClass24_1<T>
				{
					e = array[i]
				};
				if (sp.CompareStatus(_003C_003Ec__DisplayClass24_.MapIterator) < 0)
				{
					int num = _003C_003Ec__DisplayClass24_0<T>.VerifyIterator(sp) + 1;
					_003C_003Ec__DisplayClass24_0<T>.SetIterator(sp, num);
					_003C_003Ec__DisplayClass24_0<T>.InvokeIterator(_003C_003Ec__DisplayClass24_0<T>.SortIterator(sp, num - 1), (UnityEngine.Object)_003C_003Ec__DisplayClass24_.e);
				}
			}
			_003C_003Ec__DisplayClass24_0<T>.ConcatIterator(_003C_003Ec__DisplayClass24_0<T>.CustomizeIterator(sp));
		}

		internal static bool SetupState()
		{
			return ChangeState == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass24_1<T> where T : UnityEngine.Object
	{
		public T e;

		private static object PopState;

		internal bool MapIterator(SerializedProperty e2, int _)
		{
			return _003C_003Ec__DisplayClass24_1<T>.CancelIterator(_003C_003Ec__DisplayClass24_1<T>.FillIterator(e2), (UnityEngine.Object)e);
		}

		internal static bool ViewState()
		{
			return PopState == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass26_0<T> where T : UnityEngine.Object
	{
		public T[] enumerable;

		private static object PushState;

		internal void LogoutIterator(SerializedProperty sp)
		{
			T[] array = enumerable;
			for (int i = 0; i < array.Length; i++)
			{
				int num = sp.CompareStatus(new _003C_003Ec__DisplayClass26_1<T>
				{
					e = array[i]
				}.MoveIterator);
				if (num >= 0)
				{
					_003C_003Ec__DisplayClass26_0<T>.SetupIterator(sp, num);
				}
			}
			_003C_003Ec__DisplayClass26_0<T>.WriteIterator(_003C_003Ec__DisplayClass26_0<T>.SelectIterator(sp));
		}

		internal static bool SortState()
		{
			return PushState == null;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass26_1<T> where T : UnityEngine.Object
	{
		public T e;

		private static object CloneState;

		internal bool MoveIterator(SerializedProperty e2, int i)
		{
			return _003C_003Ec__DisplayClass26_1<T>.CollectIterator(_003C_003Ec__DisplayClass26_1<T>.PublishIterator(e2), (UnityEngine.Object)e);
		}

		internal static bool FindState()
		{
			return CloneState == null;
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
					if (num != 0)
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
					else
					{
						awaiter = _003C_003Eu__1;
						_003C_003Eu__1 = default(TaskAwaiter<T>);
						num = -1;
						_003C_003E1__state = -1;
					}
					obj = awaiter.GetResult();
				}
				catch
				{
					obj = default(T);
				}
				if (!_003CHandleTask_003Ed__18<T>._202C_206A_200F_200D_206F_206B_200D_200B_200E_202C_202E_202A_200F_200F_200C_202E_206D_200C_202B_202E_200B_200F_202C_200B_206A_200F_202C_200E_206E_206B_206D_200D_200B_206B_206E_206E_202C_200C_202D_200C_202E((Task)taskHandle))
				{
					_003CHandleTask_003Ed__18<T>._206C_206D_200B_206D_206B_206C_200F_206C_206B_202B_200B_206D_206C_200B_206E_200E_206F_202A_200E_202C_200D_206E_206D_206D_202C_200E_202C_200C_206B_206B_202E_206B_200D_206A_202A_202A_200B_202A_202A_206F_202E((object)"FATAL ERROR! Task not completed?");
				}
				else
				{
					if (onComplete != null)
					{
						try
						{
							onComplete();
						}
						catch (Exception ex)
						{
							_003CHandleTask_003Ed__18<T>._202A_206B_202C_202D_202A_206C_206F_200D_202D_202E_206D_200D_202D_206D_206D_202C_206F_206B_206A_206C_206A_202E_206E_206F_206A_202C_200E_206B_206B_206A_206F_202E_206D_206F_200E_206B_206B_206E_202B_202D_202E(ex);
							throw;
						}
					}
					if (!_003CHandleTask_003Ed__18<T>._206D_202C_202E_206D_206A_206A_202E_200D_206A_206F_206D_202A_202A_200D_202A_206D_206E_202A_206A_206B_206F_200F_206A_200C_206B_202D_200F_206D_200B_202D_200F_206F_206B_200F_202E_206F_206C_200C_200F_206A_202E((Task)taskHandle) || _003CHandleTask_003Ed__18<T>._206B_206A_200E_200C_202E_206A_200D_202B_206E_202D_200C_200D_202D_206E_206C_206B_206D_206E_200F_206D_202B_202D_200C_206E_200C_206E_206C_200D_206C_206C_206A_206C_206A_206B_206E_202E_202A_200F_200F_206A_202E((Task)taskHandle))
					{
						if (_003CHandleTask_003Ed__18<T>._206D_202C_202E_206D_206A_206A_202E_200D_206A_206F_206D_202A_202A_200D_202A_206D_206E_202A_206A_206B_206F_200F_206A_200C_206B_202D_200F_206D_200B_202D_200F_206F_206B_200F_202E_206F_206C_200C_200F_206A_202E((Task)taskHandle) || !_003CHandleTask_003Ed__18<T>._206B_206A_200E_200C_202E_206A_200D_202B_206E_202D_200C_200D_202D_206E_206C_206B_206D_206E_200F_206D_202B_202D_200C_206E_200C_206E_206C_200D_206C_206C_206A_206C_206A_206B_206E_202E_202A_200F_200F_206A_202E((Task)taskHandle))
						{
							try
							{
								onSuccess((T)obj);
							}
							catch (Exception ex2)
							{
								_003CHandleTask_003Ed__18<T>._202A_206B_202C_202D_202A_206C_206F_200D_202D_202E_206D_200D_202D_206D_206D_202C_206F_206B_206A_206C_206A_202E_206E_206F_206A_202C_200E_206B_206B_206A_206F_202E_206D_206F_200E_206B_206B_206E_202B_202D_202E(ex2);
								throw ex2;
							}
						}
						else if (OnCancelled != null)
						{
							try
							{
								OnCancelled();
							}
							catch (Exception ex3)
							{
								_003CHandleTask_003Ed__18<T>._202A_206B_202C_202D_202A_206C_206F_200D_202D_202E_206D_200D_202D_206D_206D_202C_206F_206B_206A_206C_206A_202E_206E_206F_206A_202C_200E_206B_206B_206A_206F_202E_206D_206F_200E_206B_206B_206E_202B_202D_202E(ex3);
								throw ex3;
							}
						}
					}
					else
					{
						Exception ex4 = _003CHandleTask_003Ed__18<T>._206E_202A_206B_202B_202A_202C_206E_200E_206E_200E_200D_202E_206C_206F_202E_206E_202C_206B_200F_206E_200E_206D_206C_200B_200D_202B_202D_206E_200B_200F_202A_206B_202B_200F_206A_200F_206E_200B_202E_202C_202E((Exception)_003CHandleTask_003Ed__18<T>._206B_202C_200C_206C_200D_206E_206E_206B_202A_202C_202C_206F_202B_202E_200C_202B_206D_206C_200C_202E_206C_206E_206B_206E_206F_202E_200B_206B_206A_206B_202A_206E_200E_200B_200F_206D_206D_200E_202B_200E_202E((Task)taskHandle));
						if (onFailure == null)
						{
							_003CHandleTask_003Ed__18<T>._202A_206B_202C_202D_202A_206C_206F_200D_202D_202E_206D_200D_202D_206D_206D_202C_206F_206B_206A_206C_206A_202E_206E_206F_206A_202C_200E_206B_206B_206A_206F_202E_206D_206F_200E_206B_206B_206E_202B_202D_202E(ex4);
						}
						else
						{
							try
							{
								onFailure(ex4);
							}
							catch (Exception ex5)
							{
								_003CHandleTask_003Ed__18<T>._202A_206B_202C_202D_202A_206C_206F_200D_202D_202E_206D_200D_202D_206D_206D_202C_206F_206B_206A_206C_206A_202E_206E_206F_206A_202C_200E_206B_206B_206A_206F_202E_206D_206F_200E_206B_206B_206E_202B_202D_202E(ex5);
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
							_003CHandleTask_003Ed__18<T>._202A_206B_202C_202D_202A_206C_206F_200D_202D_202E_206D_200D_202D_206D_206D_202C_206F_206B_206A_206C_206A_202E_206E_206F_206A_202C_200E_206B_206B_206A_206F_202E_206D_206F_200E_206B_206B_206E_202B_202D_202E(ex6);
							throw ex6;
						}
					}
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

	private static readonly Queue<Action> _IteratorSerializer = new Queue<Action>();

	internal static string _PredicateSerializer = Application.unityVersion;

	internal static bool collectionSerializer = _PredicateSerializer.Contains("2022");

	private static readonly Stack<(Rect, MouseCursor)> interceptorSerializer = new Stack<(Rect, MouseCursor)>();

	private static bool m_RegistrySerializer;

	private static MethodInfo _ClientSerializer;

	internal static Color _ObserverSerializer = new Color(0.56f, 0.94f, 0.47f);

	internal static Color _BroadcasterSerializer = new Color(1f, 0.25f, 0.25f);

	internal static Color _EventSerializer = new Color(0.99f, 0.95f, 0f);

	internal static Color m_RecordSerializer = new Color(0.3f, 0.7f, 1f);

	internal static Color resolverSerializer = new Color(0.7f, 0.3f, 1f);

	internal static Color _TagSerializer = new Color(1f, 0.65f, 0f);

	internal static Color _FilterSerializer = new Color(1f, 0.5f, 0.7f);

	internal static InterpreterSerializer factorySerializer;

	internal static CreatorServerStub _AttributeSerializer;

	private static Mesh m_InstanceSerializer;

	private static Material m_TaskSerializer;

	private static readonly int customerSerializer = Shader.PropertyToID("_Color");

	private static readonly int m_DatabaseSerializer = "RadiusHandleHash".GetHashCode();

	internal static MethodInfo helperSerializer;

	internal static Type candidateSerializer;

	internal static bool readerSerializer;

	internal static Type _StubSerializer;

	internal static Type rulesSerializer;

	internal static FieldInfo testsSerializer;

	internal static FieldInfo _DefinitionSerializer;

	internal static Type _InitializerSerializer;

	internal static MethodInfo _TokenSerializer;

	internal static readonly ParserWatcherRule getterSerializer = new ParserWatcherRule("https://raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png", addcol: true, "DreadBanner.png");

	private static Texture2D m_ThreadSerializer;

	internal static readonly string[] _AlgoSerializer = new string[23]
	{
		"IsLocal", "Viseme", "Voice", "GestureLeft", "GestureRight", "GestureLeftWeight", "GestureRightWeight", "AngularY", "VelocityX", "VelocityY",
		"VelocityZ", "VelocityMagnitude", "Upright", "Grounded", "Seated", "AFK", "TrackingType", "VRMode", "MuteSelf", "InStation",
		"Earmuffs", "IsOnFriendsList", "AvatarVersion"
	};

	internal static readonly string[] m_RoleSerializer = new string[23]
	{
		"Head", "Torso", "Hand", "Foot", "Finger", "FingerIndex", "FingerMiddle", "FingerRing", "FingerLittle", "HandL",
		"FootL", "FingerL", "FingerIndexL", "FingerMiddleL", "FingerRingL", "FingerLittleL", "HandR", "FootR", "FingerR", "FingerIndexR",
		"FingerMiddleR", "FingerRingR", "FingerLittleR"
	};

	internal static InstanceConsumerExporter[] m_VisitorSerializer = new InstanceConsumerExporter[5]
	{
		new InstanceConsumerExporter("_IsGrabbed", AnimatorControllerParameterType.Bool, "param_IsGrabbedValue"),
		new InstanceConsumerExporter("_IsPosed", AnimatorControllerParameterType.Bool, "param_IsPosedValue"),
		new InstanceConsumerExporter("_Stretch", AnimatorControllerParameterType.Float, "param_StretchValue"),
		new InstanceConsumerExporter("_Squish", AnimatorControllerParameterType.Float, "param_SquishValue"),
		new InstanceConsumerExporter("_Angle", AnimatorControllerParameterType.Float, "param_AngleValue")
	};

	internal static ExceptionSingletonStruct FlushOrder;

	internal static bool CountProcess(this PositionFlag item)
	{
		if (!item.HasFlag(PositionFlag.Right) && !item.HasFlag(PositionFlag.TopRight))
		{
			return item.HasFlag(PositionFlag.BottomRight);
		}
		return true;
	}

	internal static bool StartProcess(this PositionFlag item)
	{
		if (item.HasFlag(PositionFlag.Left) || item.HasFlag(PositionFlag.TopLeft))
		{
			return true;
		}
		return item.HasFlag(PositionFlag.BottomLeft);
	}

	internal static bool RemoveProcess(this PositionFlag last)
	{
		if (last.HasFlag(PositionFlag.Top) || last.HasFlag(PositionFlag.TopLeft))
		{
			return true;
		}
		return last.HasFlag(PositionFlag.TopRight);
	}

	internal static bool ReflectProcess(this PositionFlag first)
	{
		if (!first.HasFlag(PositionFlag.Bottom) && !first.HasFlag(PositionFlag.BottomLeft))
		{
			return first.HasFlag(PositionFlag.BottomRight);
		}
		return true;
	}

	public static PositionFlag ResolveProcess(this PositionFlag config, bool evaluateivk = false, bool ishelper = false)
	{
		PositionFlag positionFlag;
		if (config <= PositionFlag.Bottom)
		{
			if (config > PositionFlag.Left)
			{
				if (config != PositionFlag.Top)
				{
					if (config != PositionFlag.Bottom)
					{
						goto IL_004a;
					}
					positionFlag = PositionFlag.Right | PositionFlag.Left | PositionFlag.Top;
				}
				else
				{
					positionFlag = PositionFlag.Right | PositionFlag.Left | PositionFlag.Bottom;
				}
			}
			else if (config != PositionFlag.Right)
			{
				if (config != PositionFlag.Left)
				{
					goto IL_004a;
				}
				positionFlag = PositionFlag.Right | PositionFlag.Top | PositionFlag.Bottom;
			}
			else
			{
				positionFlag = PositionFlag.Left | PositionFlag.Top | PositionFlag.Bottom;
			}
		}
		else if (config > PositionFlag.TopLeft)
		{
			if (config != PositionFlag.BottomRight)
			{
				if (config != PositionFlag.BottomLeft)
				{
					goto IL_004a;
				}
				positionFlag = PositionFlag.Right | PositionFlag.Top;
			}
			else
			{
				positionFlag = PositionFlag.Left | PositionFlag.Top;
			}
		}
		else if (config != PositionFlag.TopRight)
		{
			if (config != PositionFlag.TopLeft)
			{
				goto IL_004a;
			}
			positionFlag = PositionFlag.Right | PositionFlag.Bottom;
		}
		else
		{
			positionFlag = PositionFlag.Left | PositionFlag.Bottom;
		}
		goto IL_0019;
		IL_0019:
		if (evaluateivk)
		{
			positionFlag &= ~(PositionFlag.Top | PositionFlag.Bottom);
		}
		if (ishelper)
		{
			positionFlag &= ~(PositionFlag.Right | PositionFlag.Left);
		}
		return positionFlag;
		IL_004a:
		positionFlag = PositionFlag.Middle;
		goto IL_0019;
	}

	internal static Rect ResetProcess(Rect item, float pred = 2f)
	{
		return GetProcess(item, new Color(0.03f, 0.03f, 0.03f, 0.5f), new Color(0.137f, 0.137f, 0.137f, 0.5f), pred);
	}

	internal static Rect GetProcess(Rect reference, Color cfg, Color comp, float key2 = 3f)
	{
		float num = key2 + 2f;
		Rect position = reference;
		position.x -= num / 2f;
		position.width += num;
		position.y -= num / 2f;
		position.height += num;
		if (cfg != Color.clear)
		{
			GUI.DrawTexture(reference, DestroyVal(cfg), ScaleMode.StretchToFill, alphaBlend: true, 0f, cfg, 0f, 8f);
		}
		if (comp != Color.clear)
		{
			GUI.DrawTexture(position, DestroyVal(comp), ScaleMode.StretchToFill, alphaBlend: true, 0f, comp, key2, 8f);
		}
		Rect result = reference;
		result.x += 4f;
		result.width -= 8f;
		result.y += 4f;
		result.height -= 8f;
		return result;
	}

	internal static bool FlushProcess(this AnimationCurve task, float col, out Keyframe field, out Keyframe pred2)
	{
		field = default(Keyframe);
		pred2 = default(Keyframe);
		if (task.length != 0)
		{
			if (task.length == 1)
			{
				field = task[0];
				return false;
			}
			int num = 0;
			Keyframe keyframe;
			while (true)
			{
				if (num >= task.length)
				{
					return false;
				}
				keyframe = task[num];
				if (keyframe.time == col)
				{
					break;
				}
				if (keyframe.time >= col)
				{
					pred2 = keyframe;
					return true;
				}
				field = keyframe;
				num++;
			}
			field = (pred2 = keyframe);
			return true;
		}
		return false;
	}

	internal static bool ExcludeProcess(this AnimationCurve item, float ord, out float dic)
	{
		dic = 0f;
		if (item.FlushProcess(ord, out var field, out var pred))
		{
			if (field.time != pred.time)
			{
				dic = ConnectProcess(field, pred, ord);
				return true;
			}
			dic = field.outTangent;
			return true;
		}
		return false;
	}

	internal static float InitProcess(float instance, float result, float helper, float first2, float info3)
	{
		float num = 2f * result;
		float num2 = helper - instance;
		float num3 = 2f * instance - 5f * result + 4f * helper - first2;
		float num4 = 0f - instance + 3f * result - 3f * helper + first2;
		return 0.5f * (num + num2 * info3 + num3 * info3 * info3 + num4 * info3 * info3 * info3);
	}

	internal static float ConnectProcess(Keyframe instance, Keyframe pol, float serv)
	{
		_ = pol.time - instance.time;
		_ = 57.29578f * Mathf.Atan(instance.outTangent);
		while (true)
		{
			_ = 57.29578f * Mathf.Atan(pol.inTangent);
		}
	}

	internal static bool FindProcess(this AnimatorController ident, string cont, AnimatorControllerParameterType dir, float key2)
	{
		bool num = ident.parameters.All((AnimatorControllerParameter p) => p.name != cont);
		if (num)
		{
			ident.AddParameter(new AnimatorControllerParameter
			{
				name = cont,
				type = dir,
				defaultBool = (key2 != 0f),
				defaultInt = (int)key2,
				defaultFloat = key2
			});
		}
		return num;
	}

	internal static void AddProcess(Action i)
	{
		bool num = _IteratorSerializer.Count == 0;
		_IteratorSerializer.Enqueue(i);
		if (num)
		{
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(ValidateProcess));
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, new EditorApplication.CallbackFunction(ValidateProcess));
		}
	}

	private static void ValidateProcess()
	{
		while (_IteratorSerializer.Count != 0)
		{
			Action action = _IteratorSerializer.Dequeue();
			try
			{
				action();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
		EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(ValidateProcess));
	}

	internal static async Task<T> CreateProcess<T>(this Task<T> res, Action<T> attr, Action<Exception> res = null, Action task2 = null, Action var13 = null, Action selection4 = null)
	{
		object obj;
		try
		{
			obj = await res;
		}
		catch
		{
			obj = default(T);
		}
		if (!_003CHandleTask_003Ed__18<T>._202C_206A_200F_200D_206F_206B_200D_200B_200E_202C_202E_202A_200F_200F_200C_202E_206D_200C_202B_202E_200B_200F_202C_200B_206A_200F_202C_200E_206E_206B_206D_200D_200B_206B_206E_206E_202C_200C_202D_200C_202E((Task)res))
		{
			_003CHandleTask_003Ed__18<T>._206C_206D_200B_206D_206B_206C_200F_206C_206B_202B_200B_206D_206C_200B_206E_200E_206F_202A_200E_202C_200D_206E_206D_206D_202C_200E_202C_200C_206B_206B_202E_206B_200D_206A_202A_202A_200B_202A_202A_206F_202E((object)"FATAL ERROR! Task not completed?");
		}
		else
		{
			if (var13 != null)
			{
				try
				{
					var13();
				}
				catch (Exception ex)
				{
					_003CHandleTask_003Ed__18<T>._202A_206B_202C_202D_202A_206C_206F_200D_202D_202E_206D_200D_202D_206D_206D_202C_206F_206B_206A_206C_206A_202E_206E_206F_206A_202C_200E_206B_206B_206A_206F_202E_206D_206F_200E_206B_206B_206E_202B_202D_202E(ex);
					throw;
				}
			}
			if (!_003CHandleTask_003Ed__18<T>._206D_202C_202E_206D_206A_206A_202E_200D_206A_206F_206D_202A_202A_200D_202A_206D_206E_202A_206A_206B_206F_200F_206A_200C_206B_202D_200F_206D_200B_202D_200F_206F_206B_200F_202E_206F_206C_200C_200F_206A_202E((Task)res) || _003CHandleTask_003Ed__18<T>._206B_206A_200E_200C_202E_206A_200D_202B_206E_202D_200C_200D_202D_206E_206C_206B_206D_206E_200F_206D_202B_202D_200C_206E_200C_206E_206C_200D_206C_206C_206A_206C_206A_206B_206E_202E_202A_200F_200F_206A_202E((Task)res))
			{
				if (_003CHandleTask_003Ed__18<T>._206D_202C_202E_206D_206A_206A_202E_200D_206A_206F_206D_202A_202A_200D_202A_206D_206E_202A_206A_206B_206F_200F_206A_200C_206B_202D_200F_206D_200B_202D_200F_206F_206B_200F_202E_206F_206C_200C_200F_206A_202E((Task)res) || !_003CHandleTask_003Ed__18<T>._206B_206A_200E_200C_202E_206A_200D_202B_206E_202D_200C_200D_202D_206E_206C_206B_206D_206E_200F_206D_202B_202D_200C_206E_200C_206E_206C_200D_206C_206C_206A_206C_206A_206B_206E_202E_202A_200F_200F_206A_202E((Task)res))
				{
					try
					{
						attr((T)obj);
					}
					catch (Exception ex2)
					{
						_003CHandleTask_003Ed__18<T>._202A_206B_202C_202D_202A_206C_206F_200D_202D_202E_206D_200D_202D_206D_206D_202C_206F_206B_206A_206C_206A_202E_206E_206F_206A_202C_200E_206B_206B_206A_206F_202E_206D_206F_200E_206B_206B_206E_202B_202D_202E(ex2);
						throw ex2;
					}
				}
				else if (task2 != null)
				{
					try
					{
						task2();
					}
					catch (Exception ex3)
					{
						_003CHandleTask_003Ed__18<T>._202A_206B_202C_202D_202A_206C_206F_200D_202D_202E_206D_200D_202D_206D_206D_202C_206F_206B_206A_206C_206A_202E_206E_206F_206A_202C_200E_206B_206B_206A_206F_202E_206D_206F_200E_206B_206B_206E_202B_202D_202E(ex3);
						throw ex3;
					}
				}
			}
			else
			{
				Exception ex4 = _003CHandleTask_003Ed__18<T>._206E_202A_206B_202B_202A_202C_206E_200E_206E_200E_200D_202E_206C_206F_202E_206E_202C_206B_200F_206E_200E_206D_206C_200B_200D_202B_202D_206E_200B_200F_202A_206B_202B_200F_206A_200F_206E_200B_202E_202C_202E((Exception)_003CHandleTask_003Ed__18<T>._206B_202C_200C_206C_200D_206E_206E_206B_202A_202C_202C_206F_202B_202E_200C_202B_206D_206C_200C_202E_206C_206E_206B_206E_206F_202E_200B_206B_206A_206B_202A_206E_200E_200B_200F_206D_206D_200E_202B_200E_202E((Task)res));
				if (res == null)
				{
					_003CHandleTask_003Ed__18<T>._202A_206B_202C_202D_202A_206C_206F_200D_202D_202E_206D_200D_202D_206D_206D_202C_206F_206B_206A_206C_206A_202E_206E_206F_206A_202C_200E_206B_206B_206A_206F_202E_206D_206F_200E_206B_206B_206E_202B_202D_202E(ex4);
				}
				else
				{
					try
					{
						res(ex4);
					}
					catch (Exception ex5)
					{
						_003CHandleTask_003Ed__18<T>._202A_206B_202C_202D_202A_206C_206F_200D_202D_202E_206D_200D_202D_206D_206D_202C_206F_206B_206A_206C_206A_202E_206E_206F_206A_202C_200E_206B_206B_206A_206F_202E_206D_206F_200E_206B_206B_206E_202B_202D_202E(ex5);
						throw ex5;
					}
				}
			}
			if (selection4 != null)
			{
				try
				{
					selection4();
				}
				catch (Exception ex6)
				{
					_003CHandleTask_003Ed__18<T>._202A_206B_202C_202D_202A_206C_206F_200D_202D_202E_206D_200D_202D_206D_206D_202C_206F_206B_206A_206C_206A_202E_206E_206F_206A_202C_200E_206B_206B_206A_206F_202E_206D_206F_200E_206B_206B_206E_202B_202D_202E(ex6);
					throw ex6;
				}
			}
		}
		return (T)obj;
	}

	internal static void IncludeProcess<T>(Rect last, Action<T> attr, Func<T, bool> control = null, Action vis2 = null) where T : UnityEngine.Object
	{
		Event current = Event.current;
		if ((current.type == EventType.DragPerform || current.type == EventType.DragUpdated) && last.Contains(current.mousePosition))
		{
			T val = ((!typeof(T).IsSubclassOf(typeof(Component))) ? DragAndDrop.objectReferences.OfType<T>().FirstOrDefault((T el) => control?.Invoke(el) ?? true) : DragAndDrop.objectReferences.Select(delegate(UnityEngine.Object o)
			{
				GameObject obj = o as GameObject;
				return ((object)obj != null) ? obj.GetComponent<T>() : null;
			}).FirstOrDefault((T c) => _003C_003Ec__DisplayClass19_0<T>.DeleteIterator((UnityEngine.Object)c, (UnityEngine.Object)null) && (control?.Invoke(c) ?? true)));
			bool flag;
			if (flag = val != null)
			{
				vis2?.Invoke();
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			}
			if (current.type == EventType.DragPerform && flag)
			{
				DragAndDrop.AcceptDrag();
				attr(val);
			}
			current.Use();
		}
	}

	internal static void RevertProcess<T>(Rect res, Action<IEnumerable<T>> ivk, Func<T, bool> rule = null, Action col2 = null) where T : UnityEngine.Object
	{
		Event current = Event.current;
		if ((current.type == EventType.DragPerform || current.type == EventType.DragUpdated) && res.Contains(current.mousePosition))
		{
			T[] array = ((!typeof(T).IsSubclassOf(typeof(Component))) ? DragAndDrop.objectReferences.OfType<T>().ToArray() : (from c in DragAndDrop.objectReferences.Select(delegate(UnityEngine.Object o)
				{
					GameObject obj = o as GameObject;
					return ((object)obj != null) ? obj.GetComponent<T>() : null;
				})
				where _003C_003Ec__DisplayClass20_0<T>.DestroyIterator((UnityEngine.Object)c, (UnityEngine.Object)null) && (rule?.Invoke(c) ?? true)
				select c).ToArray());
			bool flag;
			if (flag = array.Length != 0)
			{
				col2?.Invoke();
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			}
			if (current.type == EventType.DragPerform && flag)
			{
				DragAndDrop.AcceptDrag();
				ivk(array);
			}
			current.Use();
		}
	}

	internal static PositionFlag RunStatus(PositionFlag info, Rect ivk, PositionFlag consumer = PositionFlag.All)
	{
		InsertStatus(ivk, MouseCursor.Pan);
		float num = ivk.width / 3f;
		float num2 = ivk.height / 3f;
		foreach (PositionFlag item in PositionFlag.All.ConcatStatus())
		{
			if (item == (PositionFlag)0 || (item & (item - 1)) != 0)
			{
				continue;
			}
			Rect rect = ivk;
			if (item.CountProcess())
			{
				rect.x += num * 2f;
			}
			else if (!item.StartProcess())
			{
				rect.x += num;
			}
			if (item.ReflectProcess())
			{
				rect.y += num2 * 2f;
			}
			else if (!item.RemoveProcess())
			{
				rect.y += num2;
			}
			rect.width = num;
			rect.height = num2;
			float num3 = 3f;
			float num4 = 1.5f;
			Rect reference = rect;
			reference.x += num4;
			reference.y += num4;
			reference.width -= num3;
			reference.height -= num3;
			GetProcess(reference, Color.clear, Color.grey);
			if (!consumer.HasFlag(item))
			{
				GetProcess(rect, new Color(1f, 0.5f, 0.5f, 0.5f), Color.clear);
			}
			else if (Event.current.type == EventType.Repaint)
			{
				if (!rect.Contains(Event.current.mousePosition))
				{
					GetProcess(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f), Color.clear);
					continue;
				}
				info = item;
				GetProcess(rect, new Color(0.5f, 1f, 0.5f, 0.33f), Color.clear);
			}
		}
		return info;
	}

	internal static void OrderStatus<T>(SerializedProperty asset) where T : UnityEngine.Object
	{
		bool hasMultipleDifferentValues;
		if (!(hasMultipleDifferentValues = asset.hasMultipleDifferentValues))
		{
			for (int i = 0; i < asset.arraySize; i++)
			{
				SerializedProperty arrayElementAtIndex = asset.GetArrayElementAtIndex(i);
				if (arrayElementAtIndex == null)
				{
					continue;
				}
				if (!(arrayElementAtIndex.objectReferenceValue == null))
				{
					using (new GUILayout.HorizontalScope())
					{
						EditorGUILayout.PropertyField(arrayElementAtIndex, GUIContent.none);
						if (CallStatus(CustomizeRef()._CreatorSerializer, MapRef().m_ProducerSerializer))
						{
							asset.DeleteArrayElementAtIndex(i);
						}
					}
				}
				else
				{
					asset.DeleteArrayElementAtIndex(i);
					i--;
				}
			}
		}
		Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(expand: true));
		GUIContent content = ((!hasMultipleDifferentValues) ? new GUIContent("[Drag And Drop Or Click Here]") : new GUIContent("Editing Multiple Lists", "Editing multiple lists with different values is not supported."));
		GUI.Label(controlRect, content, MapRef().configurationMethod);
		if (hasMultipleDifferentValues)
		{
			return;
		}
		RevertProcess<T>(controlRect, asset.CalcStatus<T>);
		if (ReadStatus(controlRect))
		{
			IncludeStatus(null, typeof(T), null, null, requirescol3: true, null, delegate(UnityEngine.Object o)
			{
				asset.CalcStatus<_0021_00210>((IEnumerable<_0021_00210>)(object)new T[1] { o.CustomizeStatus<T>() });
			});
		}
	}

	internal static void CalcStatus<T>(this SerializedProperty task, IEnumerable<T> ord) where T : UnityEngine.Object
	{
		T[] enumerable = (ord as T[]) ?? ord.ToArray();
		task.VerifyStatus(delegate(SerializedProperty sp)
		{
			T[] array = enumerable;
			for (int i = 0; i < array.Length; i++)
			{
				_003C_003Ec__DisplayClass24_1<T> _003C_003Ec__DisplayClass24_ = new _003C_003Ec__DisplayClass24_1<T>();
				_003C_003Ec__DisplayClass24_.e = array[i];
				if (sp.CompareStatus(_003C_003Ec__DisplayClass24_.MapIterator) < 0)
				{
					int num = _003C_003Ec__DisplayClass24_0<T>.VerifyIterator(sp) + 1;
					_003C_003Ec__DisplayClass24_0<T>.SetIterator(sp, num);
					_003C_003Ec__DisplayClass24_0<T>.InvokeIterator(_003C_003Ec__DisplayClass24_0<T>.SortIterator(sp, num - 1), (UnityEngine.Object)_003C_003Ec__DisplayClass24_.e);
				}
			}
			_003C_003Ec__DisplayClass24_0<T>.ConcatIterator(_003C_003Ec__DisplayClass24_0<T>.CustomizeIterator(sp));
		});
	}

	internal static void DefineStatus<T>(this SerializedProperty v, IEnumerable<T> pred) where T : UnityEngine.Object
	{
		T[] enumerable = (pred as T[]) ?? pred.ToArray();
		v.VerifyStatus(delegate(SerializedProperty sp)
		{
			T[] array = enumerable;
			for (int i = 0; i < array.Length; i++)
			{
				_003C_003Ec__DisplayClass26_1<T> _003C_003Ec__DisplayClass26_ = new _003C_003Ec__DisplayClass26_1<T>();
				_003C_003Ec__DisplayClass26_.e = array[i];
				int num = sp.CompareStatus(_003C_003Ec__DisplayClass26_.MoveIterator);
				if (num >= 0)
				{
					_003C_003Ec__DisplayClass26_0<T>.SetupIterator(sp, num);
				}
			}
			_003C_003Ec__DisplayClass26_0<T>.WriteIterator(_003C_003Ec__DisplayClass26_0<T>.SelectIterator(sp));
		});
	}

	internal static void DestroyStatus<T>(this SerializedProperty key, bool ispred, params T[] elements) where T : UnityEngine.Object
	{
		key.NewStatus(elements, ispred);
	}

	internal static void NewStatus<T>(this SerializedProperty param, IEnumerable<T> counter, bool isfilter) where T : UnityEngine.Object
	{
		if (!isfilter)
		{
			param.DefineStatus(counter);
		}
		else
		{
			param.CalcStatus(counter);
		}
	}

	internal static int CompareStatus(this SerializedProperty config, Func<SerializedProperty, int, bool> vis)
	{
		int num = config.arraySize - 1;
		while (num >= 0)
		{
			SerializedProperty arrayElementAtIndex = config.GetArrayElementAtIndex(num);
			if (!vis(arrayElementAtIndex, num))
			{
				num--;
				continue;
			}
			return num;
		}
		return -1;
	}

	internal static void VerifyStatus(this SerializedProperty last, Action<SerializedProperty> counter)
	{
		if (!last.hasMultipleDifferentValues)
		{
			counter(last);
			return;
		}
		string propertyPath = last.propertyPath;
		UnityEngine.Object[] targetObjects = last.serializedObject.targetObjects;
		for (int i = 0; i < targetObjects.Length; i++)
		{
			SerializedProperty obj = new SerializedObject(targetObjects[i]).FindProperty(propertyPath);
			counter(obj);
		}
	}

	internal static bool SetStatus(this ref bool instance)
	{
		return instance = !instance;
	}

	internal static Rect SortStatus(this ref Rect setup, float attr, bool isres = false, float spec2 = -1f, bool getsetup3 = false, bool overrideres4 = true)
	{
		Rect result = setup;
		result.width = ((!isres) ? (attr * setup.width / 100f) : attr);
		result.height = setup.height;
		result.x = ((spec2 == -1f) ? setup.x : ((!getsetup3) ? (setup.x + spec2 * setup.width / 100f) : spec2));
		result.y = setup.y;
		if (overrideres4)
		{
			setup.x = result.x + result.width;
			setup.width -= result.width;
		}
		return result;
	}

	internal static void InvokeStatus(this AnimBool value, Action result, Action serv = null)
	{
		if (value.faded != 0f)
		{
			EditorGUILayout.BeginFadeGroup(value.faded);
			result();
			if (serv != null && !(0f >= value.faded) && value.faded < 1f)
			{
				serv();
			}
			EditorGUILayout.EndFadeGroup();
		}
	}

	internal static T CustomizeStatus<T>(this UnityEngine.Object ident) where T : UnityEngine.Object
	{
		if (typeof(T).IsSubclassOf(typeof(Component)))
		{
			GameObject obj = ident as GameObject;
			if ((object)obj != null)
			{
				return obj.GetComponent<T>();
			}
			return null;
		}
		return ident as T;
	}

	internal static IEnumerable<T> ConcatStatus<T>(this T task) where T : Enum
	{
		return Enum.GetValues(typeof(T)).Cast<T>().Where(delegate(T value)
		{
			ref T reference = ref task;
			object flag = value;
			return reference.HasFlag((Enum)flag);
		});
	}

	internal static void MapStatus<T>(this IEnumerable<T> info, Action<T> pol)
	{
		foreach (T item in info)
		{
			pol(item);
		}
	}

	public static Func<T, bool> FillStatus<T>(this Func<T, bool> ident, Func<T, bool> connection)
	{
		return (T arg) => ident(arg) && connection(arg);
	}

	internal static Type CancelStatus(string i)
	{
		Type type = Type.GetType(i);
		if (!(type != null))
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int j = 0; j < assemblies.Length; j++)
			{
				Type[] types = assemblies[j].GetTypes();
				type = types.FirstOrDefault((Type t) => t.FullName == i);
				if (!(type != null))
				{
					type = types.FirstOrDefault((Type t) => t.Name == i);
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
		return type;
	}

	internal static Dictionary<Transform, Transform> LogoutStatus(Transform task, Transform counter, bool isutil, params Transform[] transformsToFind)
	{
		Dictionary<Transform, Transform> dictionary = new Dictionary<Transform, Transform>();
		foreach (Transform transform in transformsToFind)
		{
			if (transform.IsChildOf(task))
			{
				string n = AnimationUtility.CalculateTransformPath(transform, task);
				Transform transform2 = counter.Find(n);
				if (!(transform2 == null && isutil))
				{
					dictionary.Add(transform, transform2);
				}
			}
			else if (!isutil)
			{
				dictionary.Add(transform, null);
			}
		}
		return dictionary;
	}

	internal static Dictionary<T, T> SetupStatus<T>(Transform param, Transform connection, bool skipfilter, params T[] componentsToFind) where T : Component
	{
		Dictionary<T, T> dictionary = new Dictionary<T, T>();
		foreach (T val in componentsToFind)
		{
			if (!val.transform.IsChildOf(param))
			{
				if (!skipfilter)
				{
					dictionary.Add(val, null);
				}
				continue;
			}
			string n = AnimationUtility.CalculateTransformPath(val.transform, param);
			Transform transform = connection.Find(n);
			if (!(transform == null))
			{
				T[] components = val.GetComponents<T>();
				T[] components2 = transform.GetComponents<T>();
				int num = Array.IndexOf(components, val);
				if (!(num >= components2.Length && skipfilter))
				{
					dictionary.Add(val, components2[num]);
				}
			}
			else if (!skipfilter)
			{
				dictionary.Add(val, null);
			}
		}
		return dictionary;
	}

	internal static GUIContent SelectStatus(this SerializedProperty config)
	{
		return new GUIContent(config.displayName, config.tooltip);
	}

	internal static object WriteStatus(this SerializedProperty ident)
	{
		SerializedPropertyType propertyType = ident.propertyType;
		switch (propertyType)
		{
		case SerializedPropertyType.Generic:
		case SerializedPropertyType.Gradient:
		case SerializedPropertyType.ManagedReference:
			UnityEngine.Debug.LogWarning("Property type " + propertyType.ToString() + " does not support get value.");
			return null;
		default:
			return null;
		case SerializedPropertyType.Vector3Int:
			return ident.vector3IntValue;
		case SerializedPropertyType.Integer:
			return ident.intValue;
		case SerializedPropertyType.LayerMask:
			return ident.intValue;
		case SerializedPropertyType.ExposedReference:
			return ident.exposedReferenceValue;
		case SerializedPropertyType.Bounds:
			return ident.boundsValue;
		case SerializedPropertyType.AnimationCurve:
			return ident.animationCurveValue;
		case SerializedPropertyType.ArraySize:
			return ident.arraySize;
		case SerializedPropertyType.Quaternion:
			return ident.quaternionValue;
		case SerializedPropertyType.Enum:
			return ident.enumValueIndex;
		case SerializedPropertyType.BoundsInt:
			return ident.boundsIntValue;
		case SerializedPropertyType.Boolean:
			return ident.boolValue;
		case SerializedPropertyType.Vector2:
			return ident.vector2Value;
		case SerializedPropertyType.Rect:
			return ident.rectValue;
		case SerializedPropertyType.ObjectReference:
			return ident.objectReferenceValue;
		case SerializedPropertyType.FixedBufferSize:
			return ident.fixedBufferSize;
		case SerializedPropertyType.Color:
			return ident.colorValue;
		case SerializedPropertyType.Float:
			return ident.floatValue;
		case SerializedPropertyType.Vector2Int:
			return ident.vector2IntValue;
		case SerializedPropertyType.Character:
			return (char)ident.intValue;
		case SerializedPropertyType.RectInt:
			return ident.rectIntValue;
		case SerializedPropertyType.Vector3:
			return ident.vector3Value;
		case SerializedPropertyType.String:
			return ident.stringValue;
		case SerializedPropertyType.Vector4:
			return ident.vector4Value;
		}
	}

	internal static void MoveStatus(this SerializedProperty ident, object b)
	{
		SerializedPropertyType propertyType = ident.propertyType;
		switch (propertyType)
		{
		case SerializedPropertyType.Generic:
		case SerializedPropertyType.Gradient:
		case SerializedPropertyType.FixedBufferSize:
		case SerializedPropertyType.ManagedReference:
			UnityEngine.Debug.LogWarning("Property type " + propertyType.ToString() + " does not support set value.");
			break;
		case SerializedPropertyType.Vector3:
			ident.vector3Value = (Vector3)b;
			break;
		case SerializedPropertyType.LayerMask:
			ident.intValue = (int)b;
			break;
		case SerializedPropertyType.Rect:
			ident.rectValue = (Rect)b;
			break;
		case SerializedPropertyType.Vector4:
			ident.vector4Value = (Vector4)b;
			break;
		case SerializedPropertyType.AnimationCurve:
			ident.animationCurveValue = (AnimationCurve)b;
			break;
		case SerializedPropertyType.RectInt:
			ident.rectIntValue = (RectInt)b;
			break;
		case SerializedPropertyType.ArraySize:
			ident.arraySize = (int)b;
			break;
		case SerializedPropertyType.String:
			ident.stringValue = (string)b;
			break;
		case SerializedPropertyType.Vector3Int:
			ident.vector3IntValue = (Vector3Int)b;
			break;
		case SerializedPropertyType.BoundsInt:
			ident.boundsIntValue = (BoundsInt)b;
			break;
		case SerializedPropertyType.Boolean:
			ident.boolValue = (bool)b;
			break;
		case SerializedPropertyType.Enum:
			ident.enumValueIndex = (int)b;
			break;
		case SerializedPropertyType.Integer:
			ident.intValue = (int)b;
			break;
		case SerializedPropertyType.Character:
			ident.intValue = (char)b;
			break;
		case SerializedPropertyType.Color:
			ident.colorValue = (Color)b;
			break;
		case SerializedPropertyType.Quaternion:
			ident.quaternionValue = (Quaternion)b;
			break;
		case SerializedPropertyType.ExposedReference:
			ident.exposedReferenceValue = (UnityEngine.Object)b;
			break;
		case SerializedPropertyType.Vector2Int:
			ident.vector2IntValue = (Vector2Int)b;
			break;
		case SerializedPropertyType.ObjectReference:
			ident.objectReferenceValue = (UnityEngine.Object)b;
			break;
		case SerializedPropertyType.Bounds:
			ident.boundsValue = (Bounds)b;
			break;
		case SerializedPropertyType.Vector2:
			ident.vector2Value = (Vector2)b;
			break;
		case SerializedPropertyType.Float:
			ident.floatValue = (float)b;
			break;
		}
	}

	internal static void PublishStatus()
	{
		if (Event.current.type == EventType.Repaint)
		{
			m_RegistrySerializer = true;
			CollectStatus();
		}
	}

	internal static void CollectStatus()
	{
		interceptorSerializer.Clear();
	}

	internal static void PrintStatus()
	{
		if (Event.current.type == EventType.Repaint)
		{
			m_RegistrySerializer = false;
			while (interceptorSerializer.Count > 0)
			{
				var (screenRect, mouse) = interceptorSerializer.Pop();
				EditorGUIUtility.AddCursorRect(GUIUtility.ScreenToGUIRect(screenRect), mouse);
			}
		}
	}

	internal static bool InterruptStatus(string v, Color? connection = null)
	{
		return ViewStatus(new GUIContent(v), connection);
	}

	internal static bool ViewStatus(GUIContent info, Color? selection = null)
	{
		if (!selection.HasValue)
		{
			selection = new Color(0.3f, 0.7f, 1f);
		}
		using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.BG, Color.clear))
		{
			using (new TaskConsumerExporter(TaskConsumerExporter.ColoringType.FG, selection.Value))
			{
				bool result = CallStatus(info, MapRef()._SchemaSerializer, GUILayout.ExpandWidth(expand: false));
				PostStatus(selection);
				return result;
			}
		}
	}

	internal static void PostStatus(Color? item = null)
	{
		if (!item.HasValue)
		{
			item = new Color(0.3f, 0.7f, 1f);
		}
		if (Event.current.type == EventType.Repaint)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			Vector2 mousePosition = Event.current.mousePosition;
			if (lastRect.Contains(mousePosition))
			{
				EditorGUI.DrawRect(new Rect(lastRect.x, lastRect.yMax - 1f, lastRect.width, 1f), item.Value);
			}
			EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.Link);
		}
	}

	internal static bool ListStatus(GUIContent item, float cont = -1f, float field = -1f)
	{
		if (cont == -1f)
		{
			cont = EditorGUIUtility.singleLineHeight;
		}
		if (field == -1f)
		{
			while (true)
			{
				field = EditorGUIUtility.singleLineHeight;
			}
		}
		bool result = GUILayout.Button(item, MapRef().m_ProducerSerializer, GUILayout.Width(cont), GUILayout.Height(field));
		TestStatus();
		return result;
	}

	internal static bool ForgotStatus(Rect asset, string map, GUIStyle dir = null)
	{
		return RegisterStatus(asset, new GUIContent(map), dir);
	}

	internal static bool UpdateStatus(Rect value, string ord)
	{
		return RegisterStatus(value, new GUIContent(ord));
	}

	internal static bool SearchStatus(Rect var1, GUIContent cust)
	{
		return RegisterStatus(var1, cust);
	}

	internal static bool LoginStatus(string value, GUIStyle counter = null, params GUILayoutOption[] options)
	{
		return CallStatus(new GUIContent(value), counter, options);
	}

	internal static bool PatchStatus(string asset, params GUILayoutOption[] options)
	{
		return CallStatus(new GUIContent(asset), null, options);
	}

	internal static bool CheckStatus(GUIContent first, params GUILayoutOption[] options)
	{
		return CallStatus(first, null, options);
	}

	internal static bool CallStatus(GUIContent param, GUIStyle attr = null, params GUILayoutOption[] options)
	{
		return PrepareStatus(isinstance: false, param, attr, options);
	}

	internal static bool RegisterStatus(Rect spec, GUIContent pred, GUIStyle field = null)
	{
		if (field == null)
		{
			field = GUI.skin.button;
		}
		bool result = GUI.Button(spec, pred, field);
		TestStatus();
		return result;
	}

	internal static bool ChangeStatus(bool isitem, string ivk, GUIStyle rule = null, params GUILayoutOption[] options)
	{
		return PrepareStatus(isitem, new GUIContent(ivk), rule, options);
	}

	internal static bool StopStatus(bool removev, string second, params GUILayoutOption[] options)
	{
		return PrepareStatus(removev, new GUIContent(second), null, options);
	}

	internal static bool PushStatus(bool skipinstance, GUIContent cont, params GUILayoutOption[] options)
	{
		return PrepareStatus(skipinstance, cont, null, options);
	}

	internal static bool PrepareStatus(bool isinstance, GUIContent map, GUIStyle tag = null, params GUILayoutOption[] options)
	{
		if (tag == null)
		{
			tag = GUI.skin.button;
		}
		bool result = GUILayout.Toggle(isinstance, map, tag, options);
		TestStatus();
		return result;
	}

	internal static bool ReadStatus(Rect value = default(Rect))
	{
		if (value == default(Rect))
		{
			value = GUILayoutUtility.GetLastRect();
		}
		TestStatus(value);
		Event current = Event.current;
		if (current.type == EventType.MouseDown && current.button == 0)
		{
			return value.Contains(current.mousePosition);
		}
		return false;
	}

	internal static void TestStatus(Rect task = default(Rect), bool readvis = false)
	{
		if (Event.current.type == EventType.Repaint)
		{
			if (task == default(Rect))
			{
				task = GUILayoutUtility.GetLastRect();
			}
			InsertStatus(task, MouseCursor.Link, readvis);
		}
	}

	internal static void InsertStatus(Rect i, MouseCursor selection, bool dofilter = false)
	{
		if (!GUI.enabled && !dofilter)
		{
			return;
		}
		if (m_RegistrySerializer)
		{
			if (collectionSerializer)
			{
				i.y += 46f;
			}
			interceptorSerializer.Push((GUIUtility.GUIToScreenRect(i), selection));
		}
		else if (Event.current.type == EventType.Repaint)
		{
			EditorGUIUtility.AddCursorRect(i, selection);
		}
	}

	internal static void EnableStatus(Rect param, string reg, bool dotag = true, float ivk2 = 0f, float ident3 = 0f, bool nopred4 = true, GUIStyle spec5 = null)
	{
		if (dotag && !(param.width <= ivk2 + ident3))
		{
			if (!nopred4)
			{
				param.x -= ident3 + 2.5f;
			}
			else
			{
				param.x += ident3 + 2.5f;
			}
			GUI.Label(param, reg, spec5 ?? (nopred4 ? MapRef().m_ProcSerializer : MapRef()._IdentifierMethod));
		}
	}

	internal static void AwakeStatus(string param, bool rejectcont = true, float pool = 0f, float visitor2 = 0f, bool haveparam3 = true)
	{
		EnableStatus(GUILayoutUtility.GetLastRect(), param, rejectcont, pool, visitor2, haveparam3);
	}

	internal static void DisableStatus(int param_count = 2, int minpred = 10)
	{
		Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(param_count + minpred));
		controlRect.height = param_count;
		controlRect.y += (float)minpred / 2f;
		controlRect.x -= 2f;
		controlRect.width += 6f;
		ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#595959" : "#858585", out var color);
		EditorGUI.DrawRect(controlRect, color);
	}

	internal static bool VisitStatus(Rect v, int cfg_X)
	{
		if (GUIUtility.hotControl != cfg_X)
		{
			Event current = Event.current;
			if (current.type == EventType.MouseDown && v.Contains(current.mousePosition))
			{
				GUIUtility.hotControl = cfg_X;
				current.Use();
			}
			return false;
		}
		return true;
	}

	internal static void AssetStatus()
	{
		GUILayout.Label(GUIContent.none, GUILayout.Width(EditorGUIUtility.singleLineHeight));
	}

	[SpecialName]
	private static MethodInfo SortRef()
	{
		return _ClientSerializer ?? (_ClientSerializer = ((ExceptionSingletonStruct)(object)typeof(EditorGUI)).FlushAdapter("TextFieldDropDown", BindingFlags.Static | BindingFlags.NonPublic, (Binder)null, new Type[4]
		{
			typeof(Rect),
			typeof(GUIContent),
			typeof(string),
			typeof(string[])
		}, (ParameterModifier[])null));
	}

	internal static string PopStatus(string var1, string attr, string[] proc, params GUILayoutOption[] layoutOptions)
	{
		return InstantiateStatus(new GUIContent(var1), attr, proc, layoutOptions);
	}

	internal static string InstantiateStatus(GUIContent instance, string ord, string[] template, params GUILayoutOption[] layoutOptins)
	{
		if (!(SortRef() != null))
		{
			return ord;
		}
		Rect rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.textField, layoutOptins);
		return (string)SortRef().Invoke(null, new object[4] { rect, instance, ord, template });
	}

	internal static string RestartStatus(Rect task, string ivk, string consumer, string[] key2)
	{
		if (SortRef() != null)
		{
			return (string)SortRef().Invoke(null, new object[4]
			{
				task,
				new GUIContent(ivk),
				consumer,
				key2
			});
		}
		return consumer;
	}

	internal static GUIContent ManageStatus(string task, string pol = null)
	{
		return new GUIContent(EditorGUIUtility.IconContent(task))
		{
			tooltip = pol
		};
	}

	[SpecialName]
	internal static InterpreterSerializer CustomizeRef()
	{
		return factorySerializer ?? (factorySerializer = new InterpreterSerializer());
	}

	[SpecialName]
	internal static CreatorServerStub MapRef()
	{
		return _AttributeSerializer ?? (_AttributeSerializer = new CreatorServerStub());
	}

	internal static bool RateStatus(EventCommands ident, string connection = "", bool getc = true)
	{
		if (!string.IsNullOrEmpty(connection) && GUI.GetNameOfFocusedControl() != connection)
		{
			return false;
		}
		Event current = Event.current;
		if (current.type == EventType.ExecuteCommand || current.type == EventType.ValidateCommand)
		{
			bool num = ident.ToString() == current.commandName;
			if (num && getc)
			{
				current.Use();
			}
			return num;
		}
		return false;
	}

	internal static bool CloneStatus(KeyCode ident, string ord = "", bool isstate = true)
	{
		if (string.IsNullOrEmpty(ord) || !(GUI.GetNameOfFocusedControl() != ord))
		{
			Event current = Event.current;
			bool num = current.type == EventType.KeyDown && current.keyCode == ident;
			if (num && isstate)
			{
				current.Use();
			}
			return num;
		}
		return false;
	}

	internal static bool ComputeStatus(string last = "", bool isord = true)
	{
		if (!CloneStatus(KeyCode.Return, last, isord))
		{
			return CloneStatus(KeyCode.KeypadEnter, last, isord);
		}
		return true;
	}

	internal static bool QueryStatus(string key = "", bool isb = true)
	{
		return CloneStatus(KeyCode.Escape, key, isb);
	}

	internal static bool CountStatus(string item = "", bool injectcounter = true)
	{
		if (RateStatus(EventCommands.SoftDelete, item, injectcounter))
		{
			return true;
		}
		return RateStatus(EventCommands.Delete, item, injectcounter);
	}

	internal static bool StartStatus(string reference = "", Action ord = null, Action control = null)
	{
		if (ComputeStatus(reference))
		{
			ord?.Invoke();
			return true;
		}
		if (QueryStatus(reference))
		{
			control?.Invoke();
			return true;
		}
		return false;
	}

	internal static bool RemoveStatus(string reference, Action result = null, Action filter = null)
	{
		if (!StartStatus(reference, result, filter))
		{
			return false;
		}
		GUI.FocusControl(null);
		return true;
	}

	private static void ReflectStatus(Vector3 reference, Vector3 selection, Vector3 rule, int instance2_Position = -1, Color? def3 = null)
	{
		if (!def3.HasValue)
		{
			def3 = Handles.color;
		}
		if (instance2_Position != -1 && GUIUtility.hotControl == instance2_Position)
		{
			def3 = Color.yellow;
		}
		if (m_InstanceSerializer == null)
		{
			m_InstanceSerializer = GetStatus();
		}
		if (m_TaskSerializer == null)
		{
			m_TaskSerializer = FlushStatus();
		}
		ExcludeStatus(m_TaskSerializer);
		float num = Vector3.Distance(reference, selection);
		Vector3 normalized = (selection - reference).normalized;
		Matrix4x4 matrix = Matrix4x4.TRS(reference, Quaternion.LookRotation(normalized, rule), new Vector3(num, num, num));
		m_TaskSerializer.SetColor(customerSerializer, def3.Value);
		m_TaskSerializer.SetPass(0);
		Graphics.DrawMeshNow(m_InstanceSerializer, matrix);
	}

	private static void ResolveStatus(Vector3 task, Quaternion counter, float control, int row_var12 = -1, Color? item3 = null)
	{
		ResetStatus(Matrix4x4.TRS(task, counter, new Vector3(control, control, control)), row_var12, item3);
	}

	private static void ResetStatus(Matrix4x4 res, int num_result = -1, Color? temp = null)
	{
		if (!temp.HasValue)
		{
			temp = Handles.color;
		}
		if (num_result != -1 && GUIUtility.hotControl == num_result)
		{
			temp = Color.yellow;
		}
		if (m_InstanceSerializer == null)
		{
			m_InstanceSerializer = GetStatus();
		}
		if (m_TaskSerializer == null)
		{
			m_TaskSerializer = FlushStatus();
		}
		ExcludeStatus(m_TaskSerializer);
		m_TaskSerializer.SetColor(customerSerializer, temp.Value);
		m_TaskSerializer.SetPass(0);
		Graphics.DrawMeshNow(m_InstanceSerializer, res);
	}

	private static Mesh GetStatus()
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

	private static Material FlushStatus()
	{
		Material material = new Material(Shader.Find("UI/Unlit/Text"));
		ExcludeStatus(material);
		return material;
	}

	private static void ExcludeStatus(Material last)
	{
		last.hideFlags = HideFlags.DontSave;
		last.SetInt("_Cull", 2);
		last.SetInt("_ZWrite", 0);
		last.SetInt("_ZTest", 8);
	}

	internal static void InitStatus(PageMethod first)
	{
		Event current = Event.current;
		first._ValMethod?.Invoke(first);
		int regMethod = first.regMethod;
		switch (current.GetTypeForControl(regMethod))
		{
		case EventType.MouseDown:
			if (HandleUtility.nearestControl == regMethod && current.button == 0)
			{
				first.processMethod();
				current.Use();
			}
			break;
		case EventType.Layout:
		{
			float[] array = first.CalcComparator();
			foreach (float distance in array)
			{
				HandleUtility.AddControl(regMethod, distance);
			}
			break;
		}
		}
	}

	internal static void ConnectStatus(Transform instance, bool counterinstall = false, bool skipthird = false, bool readparam2 = false, bool usecaller3 = false, bool ismap4 = false, bool bool_0 = false)
	{
		if (instance == null)
		{
			return;
		}
		bool num = !usecaller3 && (counterinstall || Tools.current == Tool.Move);
		bool flag = !ismap4 && (skipthird || Tools.current == Tool.Rotate);
		if (!bool_0)
		{
			if (readparam2)
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
				instance.position = Handles.PositionHandle(instance.position, instance.localRotation);
			}
			else
			{
				instance.position = Handles.PositionHandle(instance.position, instance.rotation);
			}
		}
		if (flag)
		{
			if (!flag2)
			{
				instance.localRotation = Handles.RotationHandle(instance.localRotation, instance.position);
			}
			else
			{
				instance.rotation = Handles.RotationHandle(instance.rotation, instance.position);
			}
		}
	}

	internal static void FindStatus(string info, Vector3 col, float template = 0f, GUIStyle selection2 = null)
	{
		if (selection2 == null)
		{
			selection2 = EditorStyles.boldLabel;
		}
		GUIContent content = new GUIContent(info);
		float x = selection2.CalcSize(content).x;
		Vector3 vector = HandleUtility.WorldToGUIPointWithDepth(col);
		if (vector.z > 0f)
		{
			Vector3 vector2 = vector - new Vector3(x * 0.5f, template * 500f * 1f / vector.z + vector.z / (vector.z * 0.05f));
			Handles.BeginGUI();
			GUI.Label(new Rect(vector2, new Vector2(x, 20f)), content, selection2);
			Handles.EndGUI();
		}
	}

	internal static Rect AddStatus(this SceneView param)
	{
		return ValidateStatus(GUIUtility.ScreenToGUIRect(param.position));
	}

	internal static Rect ValidateStatus(Rect reference)
	{
		if (!collectionSerializer)
		{
			reference.y += 40f;
		}
		reference.height -= ((!collectionSerializer) ? 21f : 27f);
		return reference;
	}

	internal static float CreateStatus(Quaternion param, Vector3 vis, float comp, bool iscol2 = true, float map3 = 1f)
	{
		float num = 90f;
		Vector3[] array = new Vector3[4]
		{
			param * Vector3.right,
			param * Vector3.forward,
			param * -Vector3.right,
			param * -Vector3.forward
		};
		Vector3 vector;
		if (Camera.current.orthographic)
		{
			vector = Camera.current.transform.forward;
		}
		else
		{
			vector = vis - Matrix4x4.Inverse(Handles.matrix).MultiplyPoint(Camera.current.transform.position);
			float sqrMagnitude = vector.sqrMagnitude;
			float num2 = comp * comp;
			float num3 = num2 * num2 / sqrMagnitude;
			num = ((!((double)(num3 / num2) < 1.0)) ? (-1000f) : (Mathf.Atan2(Mathf.Sqrt(num2 - num3), Mathf.Sqrt(num3)) * 57.29578f));
		}
		Color color = Handles.color;
		for (int i = 0; i < 4; i++)
		{
			int controlID = GUIUtility.GetControlID(m_DatabaseSerializer, FocusType.Passive);
			float num4 = Vector3.Angle(array[i], -vector);
			if ((!((double)num4 <= 5.0) && (double)num4 < 175.0) || GUIUtility.hotControl == controlID)
			{
				float a = ((!((double)num4 <= (double)num + 5.0)) ? Mathf.Clamp01(0.2f * color.a * 2f) : Mathf.Clamp01(color.a * 2f));
				Color color2 = new Color(color.r, color.g, color.b, a);
				Handles.color = ((QualitySettings.activeColorSpace != ColorSpace.Linear) ? color2 : color2.linear);
				Vector3 position = vis + comp * array[i];
				bool changed = GUI.changed;
				GUI.changed = false;
				Vector3 a2 = Handles.Slider(controlID, position, array[i], HandleUtility.GetHandleSize(position) * 0.05f * map3, Handles.DotHandleCap, 0f);
				if (GUI.changed)
				{
					comp = Vector3.Distance(a2, vis);
				}
				GUI.changed |= changed;
				Handles.color = color;
			}
			if (iscol2)
			{
				Handles.DrawWireArc(vis, array[i], array[(i + 1) % 4], 360f, comp);
			}
		}
		return comp;
	}

	internal static void IncludeStatus(UnityEngine.Object i, Type ord, UnityEngine.Object comp = null, SerializedProperty reference2 = null, bool requirescol3 = true, List<int> param4 = null, Action<UnityEngine.Object> selection5 = null, Action<UnityEngine.Object> counter6 = null, bool isinstance7 = true)
	{
		if (candidateSerializer == null)
		{
			candidateSerializer = Type.GetType("UnityEditor.ObjectSelector, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
		}
		if (helperSerializer == null)
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
			helperSerializer = candidateSerializer.GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic, null, types, null);
			readerSerializer = helperSerializer != null;
			if (!readerSerializer)
			{
				Type[] types2 = new Type[3]
				{
					typeof(UnityEngine.Object),
					typeof(Type),
					typeof(SerializedProperty)
				}.Concat(second).ToArray();
				helperSerializer = candidateSerializer.GetMethod("Show", BindingFlags.Static | BindingFlags.Public, null, types2, null);
			}
		}
		EditorWindow window = EditorWindow.GetWindow(candidateSerializer);
		object[] second2 = new object[4] { requirescol3, param4, selection5, counter6 };
		second2 = ((!readerSerializer) ? new object[3] { i, ord, reference2 }.Concat(second2).ToArray() : new object[3] { i, ord, comp }.Concat(second2).Concat(new object[1] { isinstance7 }).ToArray());
		helperSerializer.Invoke(window, second2);
	}

	internal static void RevertStatus(Type config, Type selection)
	{
		if (_StubSerializer == null)
		{
			_StubSerializer = Type.GetType("UnityEditor.CustomEditorAttributes, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			rulesSerializer = Type.GetType("UnityEditor.CustomEditorAttributes+MonoEditorType, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			testsSerializer = _StubSerializer.GetField("kSCustomMultiEditors", BindingFlags.Static | BindingFlags.NonPublic);
			_DefinitionSerializer = rulesSerializer.GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public);
		}
		IList list = (testsSerializer.GetValue(null) as IDictionary)[config] as IList;
		_DefinitionSerializer.SetValue(list[0], selection);
		RunVal();
	}

	internal static void RunVal()
	{
		if (_InitializerSerializer == null)
		{
			_InitializerSerializer = Type.GetType("UnityEditor.InspectorWindow, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			_TokenSerializer = _InitializerSerializer.GetMethod("RefreshInspectors", BindingFlags.Static | BindingFlags.NonPublic);
		}
		_TokenSerializer.Invoke(null, null);
	}

	internal static MethodInfo OrderVal(this Type info, string token)
	{
		MethodInfo[] array = (from m in info.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.Name == token
			select m).ToArray();
		switch (array.Length)
		{
		case 0:
			UnityEngine.Debug.LogError("Method " + token + " not found in " + info.Name);
			return null;
		case 1:
			return array[0];
		default:
			UnityEngine.Debug.LogError("Multiple methods named " + token + " found in " + info.Name);
			return null;
		}
	}

	internal static MethodInfo CalculateVal(this Type setup, string counter, Type proc)
	{
		MethodInfo[] array = (from m in setup.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.Name == counter && m.GetParameters().Any((ParameterInfo p) => p.ParameterType == proc)
			select m).ToArray();
		switch (array.Length)
		{
		case 0:
			UnityEngine.Debug.LogError("Method " + counter + " not found in " + setup.Name + " with parameter of type " + proc.Name);
			return null;
		case 1:
			return array[0];
		default:
			UnityEngine.Debug.LogError("Multiple methods named " + counter + " found in " + setup.Name + " with parameter of type " + proc.Name);
			return null;
		}
	}

	internal static MethodInfo CalcVal(this Type spec, string vis, Type[] pool)
	{
		MethodInfo[] array = (from m in spec.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.Name == vis && !pool.Except(m.GetParameters().Select(_003C_003Ec.m_SystemMethod.DisableProduct)).Any()
			select m).ToArray();
		switch (array.Length)
		{
		default:
			UnityEngine.Debug.LogError("Multiple methods named " + vis + " found in " + spec.Name + " with parameters of types " + string.Join(", ", pool.Select((Type ht) => ht.Name)));
			return null;
		case 1:
			return array[0];
		case 0:
			UnityEngine.Debug.LogError("Method " + vis + " not found in " + spec.Name + " with parameters of types " + string.Join(", ", pool.Select((Type ht) => ht.Name)));
			return null;
		}
	}

	internal static MethodInfo DeleteVal(this Type i, string ivk, int next_third)
	{
		MethodInfo[] array = (from m in i.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.Name == ivk && m.GetParameters().Length == next_third
			select m).ToArray();
		switch (array.Length)
		{
		default:
			UnityEngine.Debug.LogError($"Multiple methods named {ivk} found in {i.Name} with {next_third} parameters");
			return null;
		case 0:
			UnityEngine.Debug.LogError($"Method {ivk} not found in {i.Name} with {next_third} parameters");
			return null;
		case 1:
			return array[0];
		}
	}

	private static Texture2D DefineVal(Texture2D info, float second = 0.2f, int consumer_max = 1)
	{
		if (info == null)
		{
			throw new ArgumentNullException("texture");
		}
		using EventMethod eventMethod = new EventMethod(info);
		Texture2D resolverMethod = eventMethod.m_ResolverMethod;
		int width = resolverMethod.width;
		int height = resolverMethod.height;
		int num = width;
		int num2 = 0;
		int num3 = height;
		int num4 = 0;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				if (resolverMethod.GetPixel(j, i).a >= second)
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
		int num7 = num5 + consumer_max * 2;
		int num8 = num6 + consumer_max * 2;
		if (num5 >= 1 && num6 >= 1)
		{
			Color[] pixels = resolverMethod.GetPixels(num, num3, num5, num6);
			Texture2D texture2D = new Texture2D(num7, num8);
			for (int k = 0; k < num8; k++)
			{
				for (int l = 0; l < num7; l++)
				{
					if (l < consumer_max || l >= consumer_max + num5 || k < consumer_max || k >= consumer_max + num6)
					{
						texture2D.SetPixel(l, k, Color.clear);
					}
				}
			}
			texture2D.SetPixels(consumer_max, consumer_max, num5, num6, pixels);
			texture2D.Apply();
			return texture2D;
		}
		UnityEngine.Debug.LogError("Trimmed texture has zero size.");
		return null;
	}

	internal static Texture2D DestroyVal(Color info)
	{
		if (m_ThreadSerializer == null)
		{
			m_ThreadSerializer = new Texture2D(1, 1, TextureFormat.RGBAFloat, mipChain: false)
			{
				filterMode = FilterMode.Point,
				anisoLevel = 0
			};
		}
		m_ThreadSerializer.SetPixel(0, 0, info);
		m_ThreadSerializer.Apply();
		return m_ThreadSerializer;
	}

	internal static SchemaMapping NewVal(string asset, string ivk, string field = "")
	{
		Texture2D last = null;
		GUIContent gUIContent = EditorGUIUtility.IconContent(asset);
		if (gUIContent != null && gUIContent.image != null)
		{
			last = DefineVal(gUIContent.image as Texture2D);
		}
		return new SchemaMapping(last, ivk, field);
	}

	internal static VRCContactSender CompareVal(this VRCContactReceiver init, GameObject vis)
	{
		VRCContactSender vRCContactSender = Undo.AddComponent<VRCContactSender>(vis);
		new TaskMethod(init).InvokeProduct(vRCContactSender);
		vRCContactSender.collisionTags = init.collisionTags;
		vRCContactSender.rootTransform = init.rootTransform;
		if (vRCContactSender.rootTransform == vRCContactSender.transform)
		{
			vRCContactSender.rootTransform = null;
		}
		return vRCContactSender;
	}

	internal static VRCContactSender VerifyVal(this VRCPhysBoneCollider res, GameObject connection)
	{
		VRCContactSender vRCContactSender = Undo.AddComponent<VRCContactSender>(connection);
		new TaskMethod(res).InvokeProduct(vRCContactSender);
		vRCContactSender.rootTransform = res.rootTransform;
		if (vRCContactSender.rootTransform == vRCContactSender.transform)
		{
			vRCContactSender.rootTransform = null;
		}
		return vRCContactSender;
	}

	internal static VRCContactReceiver SetVal(this VRCContactSender first, GameObject result)
	{
		VRCContactReceiver vRCContactReceiver = Undo.AddComponent<VRCContactReceiver>(result);
		new TaskMethod(first).InvokeProduct(vRCContactReceiver);
		vRCContactReceiver.collisionTags = first.collisionTags;
		vRCContactReceiver.rootTransform = first.rootTransform;
		if (vRCContactReceiver.rootTransform == vRCContactReceiver.transform)
		{
			vRCContactReceiver.rootTransform = null;
		}
		return vRCContactReceiver;
	}

	internal static VRCContactReceiver SortVal(this VRCPhysBoneCollider param, GameObject selection)
	{
		VRCContactReceiver vRCContactReceiver = Undo.AddComponent<VRCContactReceiver>(selection);
		new TaskMethod(param).InvokeProduct(vRCContactReceiver);
		vRCContactReceiver.rootTransform = param.rootTransform;
		do
		{
			vRCContactReceiver.rootTransform = null;
		}
		while (vRCContactReceiver.rootTransform == vRCContactReceiver.transform);
		return vRCContactReceiver;
	}

	internal static VRCPhysBoneCollider InvokeVal(this VRCContactReceiver setup, GameObject cfg)
	{
		VRCPhysBoneCollider vRCPhysBoneCollider = Undo.AddComponent<VRCPhysBoneCollider>(cfg);
		new TaskMethod(setup).CustomizeProduct(vRCPhysBoneCollider);
		vRCPhysBoneCollider.rootTransform = setup.rootTransform;
		if (vRCPhysBoneCollider.rootTransform == vRCPhysBoneCollider.transform)
		{
			vRCPhysBoneCollider.rootTransform = null;
		}
		return vRCPhysBoneCollider;
	}

	internal static VRCPhysBoneCollider CustomizeVal(this VRCContactSender key, GameObject cont)
	{
		VRCPhysBoneCollider vRCPhysBoneCollider = Undo.AddComponent<VRCPhysBoneCollider>(cont);
		new TaskMethod(key).CustomizeProduct(vRCPhysBoneCollider);
		vRCPhysBoneCollider.rootTransform = key.rootTransform;
		if (vRCPhysBoneCollider.rootTransform == vRCPhysBoneCollider.transform)
		{
			vRCPhysBoneCollider.rootTransform = null;
		}
		return vRCPhysBoneCollider;
	}

	internal static void ConcatVal(VRCAvatarDescriptor param, ref string[] attr, ref int[] util)
	{
		string[] array = new string[8] { "Base", "Additive", "Gesture", "Action", "FX", "Sitting", "TPose", "IKPose" };
		if ((bool)(UnityEngine.Object)(object)param)
		{
			List<(string, int)> list = new List<(string, int)>();
			for (int i = 0; i < array.Length; i++)
			{
				int num = ((i != 0) ? (i + 1) : i);
				if (param.MapVal((VRCAvatarDescriptor.AnimLayerType)num, out var _))
				{
					list.Add((array[i], num));
				}
			}
			attr = new string[list.Count];
			util = new int[list.Count];
			for (int j = 0; j < list.Count; j++)
			{
				attr[j] = list[j].Item1;
				util[j] = list[j].Item2;
			}
		}
		else
		{
			attr = Array.Empty<string>();
			util = Array.Empty<int>();
		}
	}

	internal static bool MapVal(this VRCAvatarDescriptor init, VRCAvatarDescriptor.AnimLayerType b, out AnimatorController dic)
	{
		dic = (from l in init.baseAnimationLayers.Concat(init.specialAnimationLayers)
			where l.type == b
			select l.animatorController).FirstOrDefault() as AnimatorController;
		return dic != null;
	}

	internal static bool FillVal(byte[] reference, int numcol, bool calccontrol = true)
	{
		switch (reference[numcol])
		{
		case 0:
			reference[numcol] = 1;
			return true;
		default:
			reference[numcol] = ((!calccontrol) ? ((byte)1) : ((byte)0));
			return calccontrol;
		case 1:
			reference[numcol] = 0;
			return false;
		}
	}

	MethodInfo FlushAdapter(string spec, BindingFlags ord, Binder pool, Type[] ivk2, ParameterModifier[] pred3)
	{
		return ((Type)this).GetMethod(spec, ord, pool, ivk2, pred3);
	}

	internal static bool OrderOrder()
	{
		return FlushOrder == null;
	}
}
