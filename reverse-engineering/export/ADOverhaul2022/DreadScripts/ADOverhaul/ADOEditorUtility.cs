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

internal static class ADOEditorUtility
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

	internal class ResizeHandle
	{
		private struct ResizeZone
		{
			internal PositionFlag position;

			internal Rect rect;

			internal int index;
		}

		private int activeZoneIndex;

		private Vector2 lastMousePosition = Vector2.zero;

		private readonly int controlID = GUIUtility.GetControlID("ResizeStateControlID".GetHashCode(), FocusType.Passive);

		public Action onResized;

		public float leftOffset;

		public float rightOffset;

		public float topOffset;

		public float bottomOffset;

		private bool uniformResize;

		private bool pendingReset;

		[SpecialName]
		public bool GetUniformResize()
		{
			return uniformResize;
		}

		[SpecialName]
		public void SetUniformResize(bool injectvar1)
		{
			if (uniformResize == injectvar1)
			{
				return;
			}
			uniformResize = injectvar1;
			if (!injectvar1)
			{
				return;
			}
			if (leftOffset == 0f)
			{
				if (rightOffset != 0f)
				{
					leftOffset = rightOffset;
				}
				else if (topOffset != 0f)
				{
					bottomOffset = topOffset;
				}
				else if (bottomOffset != 0f)
				{
					topOffset = bottomOffset;
				}
			}
			else
			{
				rightOffset = leftOffset;
			}
		}

		public ResizeHandle(bool islast = false)
		{
			uniformResize = islast;
		}

		public void ResetSize()
		{
			leftOffset = 0f;
			rightOffset = 0f;
			topOffset = 0f;
			bottomOffset = 0f;
			onResized?.Invoke();
		}

		public Rect GetResizedRect(Rect last, PositionFlag map = PositionFlag.Middle, Rect filter = default(Rect))
		{
			if (filter == default(Rect))
			{
				filter = new Rect(-1f, -1f, -1f, -1f);
			}
			bool flag = filter.x != -1f && filter.width != -1f;
			bool flag2 = filter.y != -1f && filter.height != -1f;
			float num = 10f;
			float num2 = last.width + leftOffset + rightOffset;
			float num3 = last.height + topOffset + bottomOffset;
			float num4 = last.x - (num2 - last.width) * GetHorizontalPivot(map);
			float num5 = last.y - (last.height + topOffset + bottomOffset - last.height) * GetVerticalPivot(map);
			last.x = (flag ? Mathf.Clamp(num4, filter.x, filter.x + filter.width - num) : num4);
			last.width = ((!flag) ? num2 : Mathf.Clamp(num2, num, filter.width - last.x));
			last.y = (flag2 ? Mathf.Clamp(num5, filter.y, filter.y + filter.height - num) : num5);
			last.height = (flag2 ? Mathf.Clamp(num3, num, filter.height - last.y) : num3);
			return last;
		}

		public void HandleResize(Rect ident, PositionFlag cust = PositionFlag.Right | PositionFlag.Left, PositionFlag c = PositionFlag.Middle, float counter2 = 4f)
		{
			Event current = Event.current;
			if (pendingReset)
			{
				goto IL_000e;
			}
			goto IL_003e;
			IL_000e:
			if (current.type == EventType.MouseUp)
			{
				if (GUIUtility.hotControl == controlID)
				{
					GUIUtility.hotControl = 0;
				}
				ResetSize();
				current.Use();
				pendingReset = false;
			}
			goto IL_003e;
			IL_003e:
			float num = counter2 * 2f;
			ResizeZone[] array = new ResizeZone[8]
			{
				new ResizeZone
				{
					position = PositionFlag.Left,
					index = 0,
					rect = new Rect(ident.x - counter2, ident.y + counter2, num, ident.height - num)
				},
				new ResizeZone
				{
					position = PositionFlag.TopLeft,
					index = 1,
					rect = new Rect(ident.x - counter2, ident.y - counter2, num, num)
				},
				new ResizeZone
				{
					position = PositionFlag.Top,
					index = 2,
					rect = new Rect(ident.x + counter2, ident.y - counter2, ident.width - num, num)
				},
				new ResizeZone
				{
					position = PositionFlag.TopRight,
					index = 3,
					rect = new Rect(ident.x + ident.width - counter2, ident.y - counter2, num, num)
				},
				new ResizeZone
				{
					position = PositionFlag.Right,
					index = 4,
					rect = new Rect(ident.x + ident.width - counter2, ident.y + counter2, num, ident.height - num)
				},
				new ResizeZone
				{
					position = PositionFlag.BottomRight,
					index = 5,
					rect = new Rect(ident.x + ident.width - counter2, ident.y + ident.height - counter2, num, num)
				},
				new ResizeZone
				{
					position = PositionFlag.Bottom,
					index = 6,
					rect = new Rect(ident.x + counter2, ident.y + ident.height - counter2, ident.width - num, num)
				},
				new ResizeZone
				{
					position = PositionFlag.BottomLeft,
					index = 7,
					rect = new Rect(ident.x - counter2, ident.y + ident.height - counter2, num, num)
				}
			};
			bool flag = current.button == 0;
			ResizeZone[] array2 = array;
			int num2 = 0;
			ResizeZone resizeZone = default(ResizeZone);
			while (true)
			{
				Vector2 vector;
				if (num2 >= array2.Length)
				{
					if (current.type != EventType.MouseDrag || GUIUtility.hotControl != controlID)
					{
						return;
					}
					PositionFlag position = array[activeZoneIndex].position;
					vector = GUIUtility.GUIToScreenPoint(current.mousePosition) - lastMousePosition;
					if (pendingReset)
					{
						if (!(vector.sqrMagnitude > new Vector2(15f, 15f).sqrMagnitude))
						{
							return;
						}
						pendingReset = false;
					}
					if (!(vector != Vector2.zero))
					{
						goto IL_0399;
					}
					if (position > PositionFlag.Bottom)
					{
						if (position > PositionFlag.TopLeft)
						{
							if (position == PositionFlag.BottomRight)
							{
								rightOffset += vector.x;
								if (GetUniformResize())
								{
									if (!c.HasFlag(PositionFlag.Top))
									{
										topOffset += vector.x;
									}
									else
									{
										bottomOffset += vector.x;
									}
								}
								else
								{
									bottomOffset += vector.y;
								}
							}
							else if (position == PositionFlag.BottomLeft)
							{
								leftOffset -= vector.x;
								if (GetUniformResize())
								{
									if (c.HasFlag(PositionFlag.Bottom))
									{
										topOffset += vector.x;
									}
									else
									{
										bottomOffset += vector.x;
									}
								}
								else
								{
									bottomOffset += vector.y;
								}
							}
						}
						else if (position == PositionFlag.TopRight)
						{
							rightOffset += vector.x;
							if (GetUniformResize())
							{
								if (c.HasFlag(PositionFlag.Left))
								{
									rightOffset -= vector.y;
								}
								else
								{
									leftOffset -= vector.y;
								}
							}
							else
							{
								topOffset -= vector.y;
							}
						}
						else if (position == PositionFlag.TopLeft)
						{
							leftOffset -= vector.x;
							if (!GetUniformResize())
							{
								topOffset -= vector.y;
							}
							else if (!c.HasFlag(PositionFlag.Bottom))
							{
								bottomOffset -= vector.x;
							}
							else
							{
								topOffset -= vector.x;
							}
						}
						goto IL_0388;
					}
					switch (position)
					{
					case PositionFlag.Middle:
					case PositionFlag.Middle | PositionFlag.Right:
						goto IL_0388;
					case PositionFlag.Right:
						goto IL_05a3;
					case PositionFlag.Left:
						goto IL_068b;
					}
				}
				else
				{
					resizeZone = array2[num2];
				}
				if ((resizeZone.position & cust) < resizeZone.position)
				{
					goto IL_0360;
				}
				PositionFlag position2 = resizeZone.position;
				if (position2 <= PositionFlag.Bottom)
				{
					switch (position2)
					{
					case PositionFlag.Right:
					case PositionFlag.Left:
						goto IL_0336;
					case PositionFlag.Middle:
					case PositionFlag.Middle | PositionFlag.Right:
						goto IL_04c5;
					}
					break;
				}
				if (position2 <= PositionFlag.TopLeft)
				{
					if (position2 == PositionFlag.TopRight)
					{
						goto IL_05f3;
					}
					if (position2 != PositionFlag.TopLeft)
					{
						goto IL_04c5;
					}
				}
				else if (position2 != PositionFlag.BottomRight)
				{
					if (position2 != PositionFlag.BottomLeft)
					{
						goto IL_04c5;
					}
					goto IL_05f3;
				}
				MouseCursor selection = MouseCursor.ResizeUpLeft;
				goto IL_0339;
				IL_04c5:
				selection = MouseCursor.Arrow;
				goto IL_0339;
				IL_05a3:
				rightOffset += vector.x;
				if (GetUniformResize())
				{
					if (c.HasFlag(PositionFlag.Bottom))
					{
						topOffset += vector.x;
					}
					else
					{
						bottomOffset += vector.x;
					}
				}
				goto IL_0388;
				IL_0399:
				lastMousePosition = GUIUtility.GUIToScreenPoint(current.mousePosition);
				return;
				IL_0360:
				num2++;
				continue;
				IL_0336:
				selection = MouseCursor.ResizeHorizontal;
				goto IL_0339;
				IL_05f3:
				selection = MouseCursor.ResizeUpRight;
				goto IL_0339;
				IL_0339:
				AddCursorRect(resizeZone.rect, selection);
				Rect rect = resizeZone.rect;
				if (IsUnity2022)
				{
					rect.y += 46f;
				}
				if (flag && current.type == EventType.MouseDown && rect.Contains(current.mousePosition))
				{
					if (current.clickCount == 2)
					{
						pendingReset = true;
					}
					activeZoneIndex = resizeZone.index;
					GUIUtility.hotControl = controlID;
					lastMousePosition = GUIUtility.GUIToScreenPoint(current.mousePosition);
					current.Use();
				}
				goto IL_0360;
				IL_0388:
				onResized?.Invoke();
				goto IL_0399;
				IL_068b:
				leftOffset -= vector.x;
				if (GetUniformResize())
				{
					if (c.HasFlag(PositionFlag.Bottom))
					{
						topOffset -= vector.x;
					}
					else
					{
						bottomOffset -= vector.x;
					}
				}
				goto IL_0388;
			}
			goto IL_000e;
		}

		public static float GetHorizontalPivot(PositionFlag def, bool wantresult = false)
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

		public static float GetVerticalPivot(PositionFlag i, bool excludemap = false)
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
	}

	internal class SceneViewPanel : IDisposable
	{
		public readonly bool widthIsPercentage;

		public readonly bool consumeMouseDown = true;

		private readonly Rect area;

		public SceneViewPanel(SceneView ident, string result, float tag, int first2_end, float value3 = 20f, PositionFlag first4 = PositionFlag.BottomRight, ResizeHandle reference5 = null)
			: this(ident, tag, first2_end + 2, value3, first4, reference5)
		{
			GUILayout.Label(result, MapRef().m_WriterSerializer);
			Separator(2, 0);
		}

		public SceneViewPanel(SceneView value, float visitor, int offsetdir, float res2 = 20f, PositionFlag ord3 = PositionFlag.BottomRight, ResizeHandle init4 = null)
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
			Rect rect2 = GetAnchoredRect(rect, visitor, offsetdir, res2, ord3, widthIsPercentage);
			if (init4 != null)
			{
				rect2 = init4.GetResizedRect(rect2, ord3, filter);
				init4.HandleResize(rect2, ord3.ResolveProcess(evaluateivk: true));
			}
			area = ResetProcess(rect2);
			if (IsUnity2022)
			{
				area.y += 46f;
			}
			GUILayout.BeginArea(area);
		}

		public SceneViewPanel(SceneView instance, float col, float role = 20f, PositionFlag vis2 = PositionFlag.BottomRight, ResizeHandle x3 = null)
			: this(instance, col, 1, role, vis2, x3)
		{
		}

		public void Dispose()
		{
			if (consumeMouseDown)
			{
				Event current = Event.current;
				if (current.type == EventType.MouseDown && !area.Contains(current.mousePosition))
				{
					current.Use();
					GUIUtility.hotControl = 0;
				}
			}
			GUILayout.EndArea();
			Handles.EndGUI();
		}

		private static Rect GetAnchoredRect(Rect last, float cfg, int column_dic, float reg2 = 20f, PositionFlag def3 = PositionFlag.Bottom, bool loadfirst4 = false)
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
	}

	internal class Contents
	{
		internal readonly CachedIcon _ParameterSerializer = TrimmedIcon("CollabConflict Icon", "ds-icon-updateAvailable", "Update Available");

		internal readonly CachedIcon m_AttrSerializer = TrimmedIcon("Refresh", "ds-icon-checkForUpdate", "Check For Update");

		internal readonly CachedIcon _ObjectSerializer = TrimmedIcon("console.infoicon.sml", "ds-icon-announcement");

		internal readonly CachedIcon m_ServiceSerializer = TrimmedIcon("console.warnicon.sml", "ds-icon-warning");

		internal readonly CachedIcon reponseSerializer = TrimmedIcon("console.erroricon.sml", "ds-icon-error");

		internal readonly CachedIcon m_SpecificationSerializer = TrimmedIcon("VerticalLayoutGroup Icon", "ds-icon-hamMenu");

		internal readonly CachedIcon wrapperSerializer = TrimmedIcon("_Help", "ds-icon-help");

		internal readonly GUIContent infoSerializer = IconContent("TestPassed", "Up to Date!");

		internal readonly GUIContent _ModelSerializer = IconContent("UnityEditor.InspectorWindow");

		internal readonly GUIContent _ConfigSerializer = IconContent("Refresh", "Reset");

		internal readonly GUIContent m_MockSerializer = IconContent("FolderOpened Icon", "Select a folder");

		internal readonly GUIContent stateSerializer = IconContent("editicon.sml");

		internal readonly GUIContent fieldSerializer = IconContent("settings");

		internal readonly GUIContent advisorSerializer = IconContent("Selectable Icon");

		internal readonly GUIContent m_ExporterSerializer = IconContent("eyeDropper.Large");

		internal readonly GUIContent _CreatorSerializer = IconContent("Toolbar Minus", "Remove selection from list");

		internal readonly GUIContent m_DispatcherSerializer = IconContent("CollabCreate Icon");

		internal readonly GUIContent connectionSerializer = IconContent("IN LockButton");

		internal readonly GUIContent expressionSerializer = IconContent("IN LockButton on");

		internal readonly GUIContent decoratorSerializer = IconContent("d_scenepicking_pickable_hover@2x");

		internal readonly GUIContent _ParamSerializer = IconContent("d_scenepicking_notpickable@2x");

		internal readonly GUIContent prototypeSerializer = IconContent("d_CustomTool@2x");

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

		internal Contents()
		{
			decoratorSerializer.tooltip = "Scene view clicks are allowed while editing.";
			_ParamSerializer.tooltip = "Scene view clicks are ignored while editing.";
			fieldSerializer.tooltip = "Open ADO Settings";
			m_ExporterSerializer.tooltip = "Copy from another component of the same type";
			advisorSerializer.tooltip = "Select through the scene view";
			stateSerializer.tooltip = "Edit through the scene view";
		}
	}

	internal class Styles
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

		internal readonly Color[] m_SerializerMethod = new Color[3] { errorColor, validColor, warningColor };

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

	internal struct SphereHandle
	{
		internal string label;

		internal GUIStyle labelStyle;

		internal Vector3 position;

		internal Quaternion _ParamsMethod;

		internal Vector3 m_ImporterMethod;

		internal float size;

		internal float[] watcherMethod;

		internal int controlId;

		internal Action onClick;

		internal Func<SphereHandle, float[]> getDistances;

		internal Action<SphereHandle> onDraw;

		internal static object PushDescriptor;

		internal static SphereHandle Create(Vector3 config, string reg = "", float pool = 0.05f, int int_0 = -1, Action reference3 = null)
		{
			return new SphereHandle
			{
				onDraw = DrawDefault,
				labelStyle = new GUIStyle(EditorStyles.boldLabel),
				getDistances = (SphereHandle sc) => new float[1] { HandleUtility.DistanceToCircle(sc.position, sc.size / 2f) },
				position = config,
				size = pool,
				label = reg,
				controlId = int_0,
				onClick = reference3
			};
		}

		internal void Draw()
		{
			onDraw(this);
		}

		internal float[] GetDistances()
		{
			return getDistances(this);
		}

		internal static void DrawDefault(SphereHandle v)
		{
			Handles.SphereHandleCap(v.controlId, v.position, Quaternion.identity, v.size, EventType.Repaint);
			if (!string.IsNullOrWhiteSpace(v.label))
			{
				FindStatus(v.label, v.position, v.size, v.labelStyle);
			}
		}

		internal static bool SortDescriptor()
		{
			return PushDescriptor == null;
		}
	}

	internal sealed class BannerDownloader
	{
		private Texture2D texture;

		private bool canResolve = true;

		private readonly string url;

		private readonly bool autoDownload;

		private readonly string cacheKey;

		internal bool isLoaded;

		internal bool isDownloading;

		private bool hasRequestedDownload;

		private bool isReady;

		[SpecialName]
		internal Texture2D GetTexture()
		{
			if (isLoaded)
			{
				if (canResolve && !texture)
				{
					TryLoadFromCache();
				}
				return texture;
			}
			if (isDownloading)
			{
				return null;
			}
			if (!autoDownload || hasRequestedDownload)
			{
				return null;
			}
			hasRequestedDownload = true;
			isDownloading = true;
			Download();
			return null;
		}

		internal BannerDownloader(string v, bool addcol, string res, bool striplast2 = false)
		{
			url = v;
			autoDownload = addcol;
			cacheKey = res;
		}

		internal void Download()
		{
			if (TryLoadFromCache())
			{
				return;
			}
			UnityWebRequest observerMethod = new UnityWebRequest(url)
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
					observerMethod.Dispose();
				}
			};
			isDownloading = false;
		}

		internal bool TryLoadFromCache()
		{
			if (canResolve && !string.IsNullOrWhiteSpace(cacheKey))
			{
				canResolve = false;
				Texture2D texture2D = CachedIcon.LoadFromCache(cacheKey);
				if (texture2D != null)
				{
					texture = texture2D;
					isLoaded = true;
					isDownloading = false;
					canResolve = true;
				}
			}
			return texture;
		}

		internal void Draw()
		{
			if (CanDraw())
			{
				Rect aspectRect = GUILayoutUtility.GetAspectRect((float)GetTexture().width / (float)GetTexture().height);
				Draw(aspectRect);
			}
		}

		internal void Draw(EditorWindow init, float visitor = 0f, float tag = 60f)
		{
			if (CanDraw())
			{
				if (init == null)
				{
					Draw();
				}
				else
				{
					Draw(init.position.width, init.position.height, visitor, tag);
				}
			}
		}

		internal void Draw(float res, float counter, float consumer = 0f, float second2 = 60f)
		{
			float num = (float)GetTexture().height / (float)GetTexture().width;
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
			Draw(rect);
		}

		private void Draw(Rect param)
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
			GUI.DrawTexture(param, GetTexture());
		}

		internal bool CanDraw()
		{
			if (isReady)
			{
				return true;
			}
			if (GetTexture() == null)
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

	internal sealed class ReadableTexture : IDisposable
	{
		internal bool isTemporary;

		internal Texture2D texture;

		internal ReadableTexture(Texture2D spec)
		{
			try
			{
				spec.GetPixel(0, 0);
				isTemporary = false;
				texture = spec;
			}
			catch
			{
				int width = spec.width;
				int height = spec.height;
				isTemporary = true;
				spec.filterMode = FilterMode.Point;
				RenderTexture temporary = RenderTexture.GetTemporary(width, height);
				temporary.filterMode = FilterMode.Point;
				RenderTexture.active = temporary;
				Graphics.Blit(spec, temporary);
				Texture2D texture2D = new Texture2D(width, height);
				texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
				RenderTexture.active = null;
				texture = texture2D;
			}
		}

		public void Dispose()
		{
			if (isTemporary)
			{
				UnityEngine.Object.DestroyImmediate(texture);
			}
		}

		public static implicit operator Texture2D(ReadableTexture param)
		{
			return param.texture;
		}
	}

	internal sealed class CachedIcon
	{
		private bool canResolve = true;

		private GUIContent content;

		private Texture2D texture;

		private readonly string cacheKey;

		private readonly string tooltip;

		[SpecialName]
		private GUIContent GetContent()
		{
			if (content.image == null && canResolve)
			{
				content = new GUIContent(GetTexture())
				{
					tooltip = tooltip
				};
			}
			return content;
		}

		[SpecialName]
		internal Texture2D GetTexture()
		{
			if (canResolve && texture == null)
			{
				canResolve = false;
				ResolveTexture();
				canResolve = texture != null;
			}
			return texture;
		}

		public CachedIcon(Texture2D last, string ivk, string proc = "")
		{
			texture = last;
			cacheKey = ivk;
			tooltip = proc;
			if (!(texture == null))
			{
				SaveToCache(last.EncodeToPNG(), ivk);
			}
			else
			{
				ResolveTexture();
			}
			content = new GUIContent(last)
			{
				tooltip = proc
			};
		}

		private void ResolveTexture()
		{
			texture = LoadFromCache(cacheKey);
		}

		private static byte[] ToBytes(int[] info)
		{
			byte[] array = new byte[info.Length];
			for (int i = 0; i < info.Length; i++)
			{
				array[i] = (byte)info[i];
			}
			return array;
		}

		private static int[] ToInts(byte[] asset)
		{
			int num = asset.Length;
			int[] array = new int[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = asset[i];
			}
			return array;
		}

		internal static Texture2D LoadFromCache(string item)
		{
			int[] intArray = SessionState.GetIntArray(item, null);
			if (intArray != null)
			{
				try
				{
					byte[] data = ToBytes(intArray);
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

		internal static void SaveToCache(byte[] setup, string vis)
		{
			int[] value = ToInts(setup);
			SessionState.SetIntArray(vis, value);
		}

		public static implicit operator GUIContent(CachedIcon param)
		{
			return param.GetContent();
		}
	}

	internal struct ShapeSnapshot
	{
		internal readonly UnityEngine.Object source;

		internal bool isPhysBoneCollider;

		internal readonly Transform rootTransform;

		internal readonly int shapeType;

		internal float radius;

		internal float height;

		internal Vector3 position;

		internal Quaternion rotation;

		private static object ValidateDescriptor;

		internal ShapeSnapshot(VRCPhysBoneColliderBase ident)
		{
			source = ident;
			isPhysBoneCollider = true;
			rootTransform = ident.GetRootTransform();
			shapeType = (int)ident.shapeType;
			radius = ident.radius;
			height = ident.height;
			position = ident.position;
			rotation = ident.rotation;
		}

		internal ShapeSnapshot(ContactBase var1)
		{
			source = var1;
			isPhysBoneCollider = false;
			rootTransform = var1.GetRootTransform();
			shapeType = (int)var1.shapeType;
			radius = var1.radius;
			height = var1.height;
			position = var1.position;
			rotation = var1.rotation;
		}

		internal void Apply()
		{
			if (isPhysBoneCollider)
			{
				VRCPhysBoneColliderBase obj = (VRCPhysBoneColliderBase)source;
				obj.radius = radius;
				obj.height = height;
				obj.position = position;
				obj.rotation = rotation;
			}
			else
			{
				ContactBase obj2 = (ContactBase)source;
				obj2.radius = radius;
				obj2.height = height;
				obj2.position = position;
				obj2.rotation = rotation;
				obj2.shapeType = (ContactBase.ShapeType)shapeType;
			}
		}

		internal void Apply(ContactBase instance)
		{
			instance.radius = radius;
			instance.height = height;
			instance.position = position;
			instance.rotation = rotation;
			instance.shapeType = (ContactBase.ShapeType)shapeType;
		}

		internal void Apply(VRCPhysBoneCollider reference)
		{
			reference.radius = radius;
			reference.height = height;
			reference.position = position;
			reference.rotation = rotation;
			reference.shapeType = (VRCPhysBoneColliderBase.ShapeType)shapeType;
		}

		internal static bool EnableDescriptor()
		{
			return ValidateDescriptor == null;
		}
	}

	internal class BoneChainTree
	{
		internal readonly VRCPhysBone physBone;

		internal readonly Transform rootTransform;

		internal readonly List<BoneNode> nodes;

		internal readonly int maxDepth;

		internal List<List<BoneNode>> chains;

		[SpecialName]
		internal IEnumerable<Matrix4x4> GetNodeMatrices()
		{
			return nodes.Select((BoneNode b) => b.matrix);
		}

		internal BoneChainTree(VRCPhysBone instance)
		{
			physBone = instance;
			rootTransform = instance.GetRootTransform();
			nodes = new List<BoneNode>();
			BuildNodes(rootTransform, 0);
			maxDepth = nodes.Max((BoneNode b) => b.depth);
		}

		internal void BuildNodes(Transform v, int next_cust)
		{
			bool flag = false;
			BoneNode boneNode = new BoneNode();
			BoneNode child = null;
			BoneNode boneNode2 = null;
			Quaternion q = v.rotation;
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < v.childCount; i++)
			{
				Transform child2 = v.GetChild(i);
				if (!physBone.ignoreTransforms.Contains(child2))
				{
					list.Add(child2);
				}
			}
			bool isEndBone;
			if (!(isEndBone = list.Count == 0))
			{
				if (list.Count > 1)
				{
					if (physBone.multiChildType == VRCPhysBoneBase.MultiChildType.Average)
					{
						Vector3 zero = Vector3.zero;
						foreach (Transform item in list)
						{
							zero += item.position;
						}
						zero /= (float)list.Count;
						Vector3 toDirection = zero - v.position;
						q = v.rotation * Quaternion.FromToRotation(v.up, toDirection);
						boneNode2 = (child = new BoneNode
						{
							tree = this,
							root = rootTransform,
							matrix = Matrix4x4.TRS(zero, q, v.lossyScale),
							depth = next_cust + 1,
							isVirtual = true,
							isEndBone = true,
							parent = boneNode
						});
					}
					else if (physBone.multiChildType == VRCPhysBoneBase.MultiChildType.Ignore)
					{
						flag = true;
					}
				}
			}
			else if (!(physBone.endpointPosition != Vector3.zero))
			{
				if (nodes.Count != 0)
				{
					q = nodes[nodes.Count - 1].matrix.rotation;
				}
			}
			else
			{
				Vector3 pos = v.TransformPoint(physBone.endpointPosition);
				q = v.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(physBone.endpointPosition));
				BoneNode obj = new BoneNode
				{
					tree = this,
					root = rootTransform,
					matrix = Matrix4x4.TRS(pos, q, v.lossyScale),
					depth = next_cust + 1,
					isVirtual = true,
					isEndBone = true,
					parent = boneNode
				};
				child = obj;
				boneNode2 = obj;
			}
			if (!flag)
			{
				boneNode.tree = this;
				boneNode.root = rootTransform;
				boneNode.transform = v;
				boneNode.matrix = Matrix4x4.TRS(v.position, q, v.lossyScale);
				boneNode.depth = next_cust;
				boneNode.isEndBone = isEndBone;
				boneNode.child = child;
				BoneNode boneNode3 = nodes.LastOrDefault();
				if (boneNode3 != null && !boneNode3.isEndBone && boneNode3.child == null)
				{
					boneNode3.child = boneNode;
					boneNode.parent = boneNode3;
				}
				nodes.Add(boneNode);
			}
			if (boneNode2 != null)
			{
				nodes.Add(boneNode2);
			}
			foreach (Transform item2 in list)
			{
				BuildNodes(item2, next_cust + 1);
			}
		}

		internal void BuildChains()
		{
			HashSet<BoneNode> hashSet = new HashSet<BoneNode>();
			chains = new List<List<BoneNode>>();
			foreach (BoneNode node in nodes)
			{
				if (!hashSet.Contains(node))
				{
					List<BoneNode> list = new List<BoneNode>();
					for (BoneNode boneNode = node; boneNode != null; boneNode = boneNode.child)
					{
						list.Add(boneNode);
						hashSet.Add(boneNode);
					}
					chains.Add(list);
				}
			}
		}
	}

	internal class BoneNode
	{
		internal BoneChainTree tree;

		internal Transform root;

		internal Transform transform;

		internal Matrix4x4 matrix;

		internal bool isVirtual;

		internal bool isEndBone;

		internal int depth;

		internal BoneNode child;

		internal BoneNode parent;

		[SpecialName]
		internal Vector3 GetPosition()
		{
			return matrix.GetColumn(3);
		}

		[SpecialName]
		internal float GetMaxScale()
		{
			return Mathf.Max(matrix.lossyScale.x, matrix.lossyScale.y, matrix.lossyScale.z);
		}

		[SpecialName]
		internal float GetNormalizedDepth()
		{
			return 1f / (float)tree.maxDepth * (float)depth;
		}

		internal float EvaluateCurve(AnimationCurve i)
		{
			if (i != null && i.length >= 2)
			{
				return i.Evaluate(GetNormalizedDepth());
			}
			return 1f;
		}
	}

	internal readonly struct PhysBoneParameter
	{
		internal readonly string suffix;

		internal readonly AnimatorControllerParameterType parameterType;

		internal readonly bool hasBackingField;

		private readonly FieldInfo valueField;

		private static object CallDescriptor;

		internal PhysBoneParameter(string setup, AnimatorControllerParameterType token, string temp)
		{
			suffix = setup;
			parameterType = token;
			valueField = ((!string.IsNullOrWhiteSpace(temp)) ? typeof(VRCPhysBoneBase).GetField(temp, BindingFlags.Instance | BindingFlags.Public) : null);
			hasBackingField = valueField != null;
		}

		internal float GetFloat(VRCPhysBoneBase config)
		{
			return (float)valueField.GetValue(config);
		}

		internal bool GetBool(VRCPhysBoneBase def)
		{
			return (bool)valueField.GetValue(def);
		}

		public string GetValueString(VRCPhysBoneBase last)
		{
			if (parameterType == AnimatorControllerParameterType.Bool)
			{
				return GetBool(last).ToString();
			}
			return GetFloat(last).ToString();
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
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass19_0<T> where T : UnityEngine.Object
	{
		public Func<T, bool> acceptConditions;

		internal static object CancelState;

		internal bool CalculateIterator(T c)
		{
			if (!DeleteIterator((UnityEngine.Object)c, (UnityEngine.Object)null))
			{
				return false;
			}
			return acceptConditions?.Invoke(c) ?? true;
		}

		internal bool CalcIterator(T el)
		{
			return acceptConditions?.Invoke(el) ?? true;
		}

		static bool DeleteIterator(UnityEngine.Object object_0, UnityEngine.Object object_1)
		{
			return object_0 != object_1;
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
			if (!DestroyIterator((UnityEngine.Object)c, (UnityEngine.Object)null))
			{
				return false;
			}
			return acceptConditions?.Invoke(c) ?? true;
		}

		static bool DestroyIterator(UnityEngine.Object object_0, UnityEngine.Object object_1)
		{
			return object_0 != object_1;
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
				if (sp.FindLastIndex(_003C_003Ec__DisplayClass24_.MapIterator) < 0)
				{
					int num = VerifyIterator(sp) + 1;
					SetIterator(sp, num);
					InvokeIterator(SortIterator(sp, num - 1), (UnityEngine.Object)_003C_003Ec__DisplayClass24_.e);
				}
			}
			ConcatIterator(CustomizeIterator(sp));
		}

		static int VerifyIterator(SerializedProperty serializedProperty_0)
		{
			return serializedProperty_0.arraySize;
		}

		static void SetIterator(SerializedProperty serializedProperty_0, int int_0)
		{
			serializedProperty_0.arraySize = int_0;
		}

		static SerializedProperty SortIterator(SerializedProperty serializedProperty_0, int int_0)
		{
			return serializedProperty_0.GetArrayElementAtIndex(int_0);
		}

		static void InvokeIterator(SerializedProperty serializedProperty_0, UnityEngine.Object object_0)
		{
			serializedProperty_0.objectReferenceValue = object_0;
		}

		static SerializedObject CustomizeIterator(SerializedProperty serializedProperty_0)
		{
			return serializedProperty_0.serializedObject;
		}

		static bool ConcatIterator(SerializedObject serializedObject_0)
		{
			return serializedObject_0.ApplyModifiedProperties();
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
			return CancelIterator(FillIterator(e2), (UnityEngine.Object)e);
		}

		static UnityEngine.Object FillIterator(SerializedProperty serializedProperty_0)
		{
			return serializedProperty_0.objectReferenceValue;
		}

		static bool CancelIterator(UnityEngine.Object object_0, UnityEngine.Object object_1)
		{
			return object_0 == object_1;
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
				int num = sp.FindLastIndex(new _003C_003Ec__DisplayClass26_1<T>
				{
					e = array[i]
				}.MoveIterator);
				if (num >= 0)
				{
					SetupIterator(sp, num);
				}
			}
			WriteIterator(SelectIterator(sp));
		}

		static void SetupIterator(SerializedProperty serializedProperty_0, int int_0)
		{
			serializedProperty_0.DeleteArrayElementAtIndex(int_0);
		}

		static SerializedObject SelectIterator(SerializedProperty serializedProperty_0)
		{
			return serializedProperty_0.serializedObject;
		}

		static bool WriteIterator(SerializedObject serializedObject_0)
		{
			return serializedObject_0.ApplyModifiedProperties();
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
			return CollectIterator(PublishIterator(e2), (UnityEngine.Object)e);
		}

		static UnityEngine.Object PublishIterator(SerializedProperty serializedProperty_0)
		{
			return serializedProperty_0.objectReferenceValue;
		}

		static bool CollectIterator(UnityEngine.Object object_0, UnityEngine.Object object_1)
		{
			return object_0 == object_1;
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
				if (!smethod_0((Task)taskHandle))
				{
					smethod_1((object)"FATAL ERROR! Task not completed?");
				}
				else
				{
					if (onComplete != null)
					{
						try
						{
							onComplete();
						}
						catch (Exception exception_)
						{
							smethod_2(exception_);
							throw;
						}
					}
					if (!smethod_3((Task)taskHandle) || smethod_4((Task)taskHandle))
					{
						if (smethod_3((Task)taskHandle) || !smethod_4((Task)taskHandle))
						{
							try
							{
								onSuccess((T)obj);
							}
							catch (Exception ex)
							{
								smethod_2(ex);
								throw ex;
							}
						}
						else if (OnCancelled != null)
						{
							try
							{
								OnCancelled();
							}
							catch (Exception ex2)
							{
								smethod_2(ex2);
								throw ex2;
							}
						}
					}
					else
					{
						Exception ex3 = smethod_6((Exception)smethod_5((Task)taskHandle));
						if (onFailure == null)
						{
							smethod_2(ex3);
						}
						else
						{
							try
							{
								onFailure(ex3);
							}
							catch (Exception ex4)
							{
								smethod_2(ex4);
								throw ex4;
							}
						}
					}
					if (onFinale != null)
					{
						try
						{
							onFinale();
						}
						catch (Exception ex5)
						{
							smethod_2(ex5);
							throw ex5;
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

		static bool smethod_0(Task task_0)
		{
			return task_0.IsCompleted;
		}

		static void smethod_1(object object_0)
		{
			UnityEngine.Debug.LogError(object_0);
		}

		static void smethod_2(Exception exception_0)
		{
			UnityEngine.Debug.LogException(exception_0);
		}

		static bool smethod_3(Task task_0)
		{
			return task_0.IsFaulted;
		}

		static bool smethod_4(Task task_0)
		{
			return task_0.IsCanceled;
		}

		static AggregateException smethod_5(Task task_0)
		{
			return task_0.Exception;
		}

		static Exception smethod_6(Exception exception_0)
		{
			return exception_0.GetBaseException();
		}
	}

	private static readonly Queue<Action> _IteratorSerializer = new Queue<Action>();

	internal static string UnityVersion = Application.unityVersion;

	internal static bool IsUnity2022 = UnityVersion.Contains("2022");

	private static readonly Stack<(Rect, MouseCursor)> DeferredCursorRects = new Stack<(Rect, MouseCursor)>();

	private static bool deferringCursorRects;

	private static MethodInfo _ClientSerializer;

	internal static Color validColor = new Color(0.56f, 0.94f, 0.47f);

	internal static Color errorColor = new Color(1f, 0.25f, 0.25f);

	internal static Color warningColor = new Color(0.99f, 0.95f, 0f);

	internal static Color secondaryActionColor = new Color(0.3f, 0.7f, 1f);

	internal static Color highlightColor = new Color(0.7f, 0.3f, 1f);

	internal static Color cautionColor = new Color(1f, 0.65f, 0f);

	internal static Color accentColor = new Color(1f, 0.5f, 0.7f);

	internal static Contents factorySerializer;

	internal static Styles _AttributeSerializer;

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

	internal static readonly BannerDownloader getterSerializer = new BannerDownloader("https://raw.githubusercontent.com/Dreadrith/DreadScripts/main/Other/DreadBanner.png", addcol: true, "DreadBanner.png");

	private static Texture2D m_ThreadSerializer;

	internal static readonly string[] reservedAvatarParameters = new string[23]
	{
		"IsLocal", "Viseme", "Voice", "GestureLeft", "GestureRight", "GestureLeftWeight", "GestureRightWeight", "AngularY", "VelocityX", "VelocityY",
		"VelocityZ", "VelocityMagnitude", "Upright", "Grounded", "Seated", "AFK", "TrackingType", "VRMode", "MuteSelf", "InStation",
		"Earmuffs", "IsOnFriendsList", "AvatarVersion"
	};

	internal static readonly string[] defaultCollisionTags = new string[23]
	{
		"Head", "Torso", "Hand", "Foot", "Finger", "FingerIndex", "FingerMiddle", "FingerRing", "FingerLittle", "HandL",
		"FootL", "FingerL", "FingerIndexL", "FingerMiddleL", "FingerRingL", "FingerLittleL", "HandR", "FootR", "FingerR", "FingerIndexR",
		"FingerMiddleR", "FingerRingR", "FingerLittleR"
	};

	internal static PhysBoneParameter[] physBoneParameters = new PhysBoneParameter[5]
	{
		new PhysBoneParameter("_IsGrabbed", AnimatorControllerParameterType.Bool, "param_IsGrabbedValue"),
		new PhysBoneParameter("_IsPosed", AnimatorControllerParameterType.Bool, "param_IsPosedValue"),
		new PhysBoneParameter("_Stretch", AnimatorControllerParameterType.Float, "param_StretchValue"),
		new PhysBoneParameter("_Squish", AnimatorControllerParameterType.Float, "param_SquishValue"),
		new PhysBoneParameter("_Angle", AnimatorControllerParameterType.Float, "param_AngleValue")
	};

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
			GUI.DrawTexture(reference, SolidColorTexture(cfg), ScaleMode.StretchToFill, alphaBlend: true, 0f, cfg, 0f, 8f);
		}
		if (comp != Color.clear)
		{
			GUI.DrawTexture(position, SolidColorTexture(comp), ScaleMode.StretchToFill, alphaBlend: true, 0f, comp, key2, 8f);
		}
		Rect result = reference;
		result.x += 4f;
		result.width -= 8f;
		result.y += 4f;
		result.height -= 8f;
		return result;
	}

	internal static bool TryGetSurroundingKeyframes(this AnimationCurve task, float col, out Keyframe field, out Keyframe pred2)
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

	internal static bool TryEvaluateTangent(this AnimationCurve item, float ord, out float dic)
	{
		dic = 0f;
		if (item.TryGetSurroundingKeyframes(ord, out var field, out var pred))
		{
			if (field.time != pred.time)
			{
				dic = TangentBetween(field, pred, ord);
				return true;
			}
			dic = field.outTangent;
			return true;
		}
		return false;
	}

	internal static float CatmullRom(float instance, float result, float helper, float first2, float info3)
	{
		float num = 2f * result;
		float num2 = helper - instance;
		float num3 = 2f * instance - 5f * result + 4f * helper - first2;
		float num4 = 0f - instance + 3f * result - 3f * helper + first2;
		return 0.5f * (num + num2 * info3 + num3 * info3 * info3 + num4 * info3 * info3 * info3);
	}

	internal static float TangentBetween(Keyframe instance, Keyframe pol, float serv)
	{
		float num = pol.time - instance.time;
		float num2 = 57.29578f * Mathf.Atan(instance.outTangent);
		float num3 = 57.29578f * Mathf.Atan(pol.inTangent);
		float value = instance.value;
		float value2 = pol.value;
		float instance2 = instance.value + Mathf.Tan(num2 + 180f) * num;
		float first = pol.value + Mathf.Tan(num3 + 180f) * num;
		float num4 = CatmullRom(instance2, value, value2, first, serv);
		return (CatmullRom(instance2, value, value2, first, serv + 1E-05f) - num4) / 1E-05f;
	}

	internal static bool AddParameterIfMissing(this AnimatorController ident, string cont, AnimatorControllerParameterType dir, float key2)
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

	internal static void DelayCall(Action i)
	{
		bool num = _IteratorSerializer.Count == 0;
		_IteratorSerializer.Enqueue(i);
		if (num)
		{
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(RunDelayedCalls));
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, new EditorApplication.CallbackFunction(RunDelayedCalls));
		}
	}

	private static void RunDelayedCalls()
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
		EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.delayCall, new EditorApplication.CallbackFunction(RunDelayedCalls));
	}

	internal static async Task<T> HandleTask<T>(this Task<T> res, Action<T> attr, Action<Exception> res = null, Action task2 = null, Action var13 = null, Action selection4 = null)
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
		if (!_003CHandleTask_003Ed__18<T>.smethod_0((Task)res))
		{
			_003CHandleTask_003Ed__18<T>.smethod_1((object)"FATAL ERROR! Task not completed?");
		}
		else
		{
			if (var13 != null)
			{
				try
				{
					var13();
				}
				catch (Exception exception_)
				{
					_003CHandleTask_003Ed__18<T>.smethod_2(exception_);
					throw;
				}
			}
			if (!_003CHandleTask_003Ed__18<T>.smethod_3((Task)res) || _003CHandleTask_003Ed__18<T>.smethod_4((Task)res))
			{
				if (_003CHandleTask_003Ed__18<T>.smethod_3((Task)res) || !_003CHandleTask_003Ed__18<T>.smethod_4((Task)res))
				{
					try
					{
						attr((T)obj);
					}
					catch (Exception ex)
					{
						_003CHandleTask_003Ed__18<T>.smethod_2(ex);
						throw ex;
					}
				}
				else if (task2 != null)
				{
					try
					{
						task2();
					}
					catch (Exception ex2)
					{
						_003CHandleTask_003Ed__18<T>.smethod_2(ex2);
						throw ex2;
					}
				}
			}
			else
			{
				Exception ex3 = _003CHandleTask_003Ed__18<T>.smethod_6((Exception)_003CHandleTask_003Ed__18<T>.smethod_5((Task)res));
				if (res == null)
				{
					_003CHandleTask_003Ed__18<T>.smethod_2(ex3);
				}
				else
				{
					try
					{
						res(ex3);
					}
					catch (Exception ex4)
					{
						_003CHandleTask_003Ed__18<T>.smethod_2(ex4);
						throw ex4;
					}
				}
			}
			if (selection4 != null)
			{
				try
				{
					selection4();
				}
				catch (Exception ex5)
				{
					_003CHandleTask_003Ed__18<T>.smethod_2(ex5);
					throw ex5;
				}
			}
		}
		return (T)obj;
	}

	internal static void HandleDragAndDrop<T>(Rect last, Action<T> attr, Func<T, bool> control = null, Action vis2 = null) where T : UnityEngine.Object
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

	internal static void HandleMultiDragAndDrop<T>(Rect res, Action<IEnumerable<T>> ivk, Func<T, bool> rule = null, Action col2 = null) where T : UnityEngine.Object
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

	internal static PositionFlag AnchorPicker(PositionFlag info, Rect ivk, PositionFlag consumer = PositionFlag.All)
	{
		AddCursorRect(ivk, MouseCursor.Pan);
		float num = ivk.width / 3f;
		float num2 = ivk.height / 3f;
		foreach (PositionFlag flag in PositionFlag.All.GetFlags())
		{
			if (flag == (PositionFlag)0 || (flag & (flag - 1)) != 0)
			{
				continue;
			}
			Rect rect = ivk;
			if (flag.CountProcess())
			{
				rect.x += num * 2f;
			}
			else if (!flag.StartProcess())
			{
				rect.x += num;
			}
			if (flag.ReflectProcess())
			{
				rect.y += num2 * 2f;
			}
			else if (!flag.RemoveProcess())
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
			if (!consumer.HasFlag(flag))
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
				info = flag;
				GetProcess(rect, new Color(0.5f, 1f, 0.5f, 0.33f), Color.clear);
			}
		}
		return info;
	}

	internal static void ObjectListField<T>(SerializedProperty asset) where T : UnityEngine.Object
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
		HandleMultiDragAndDrop<T>(controlRect, asset.AddToArray<T>);
		if (ClickArea(controlRect))
		{
			ShowObjectSelector(null, typeof(T), null, null, requirescol3: true, null, delegate(UnityEngine.Object o)
			{
				asset.AddToArray<_0021_00210>((IEnumerable<_0021_00210>)(object)new T[1] { o.CustomizeStatus<T>() });
			});
		}
	}

	internal static void AddToArray<T>(this SerializedProperty task, IEnumerable<T> ord) where T : UnityEngine.Object
	{
		T[] enumerable = (ord as T[]) ?? ord.ToArray();
		task.ForEachTarget(delegate(SerializedProperty sp)
		{
			T[] array = enumerable;
			for (int i = 0; i < array.Length; i++)
			{
				_003C_003Ec__DisplayClass24_1<T> _003C_003Ec__DisplayClass24_ = new _003C_003Ec__DisplayClass24_1<T>();
				_003C_003Ec__DisplayClass24_.e = array[i];
				if (sp.FindLastIndex(_003C_003Ec__DisplayClass24_.MapIterator) < 0)
				{
					int num = _003C_003Ec__DisplayClass24_0<T>.VerifyIterator(sp) + 1;
					_003C_003Ec__DisplayClass24_0<T>.SetIterator(sp, num);
					_003C_003Ec__DisplayClass24_0<T>.InvokeIterator(_003C_003Ec__DisplayClass24_0<T>.SortIterator(sp, num - 1), (UnityEngine.Object)_003C_003Ec__DisplayClass24_.e);
				}
			}
			_003C_003Ec__DisplayClass24_0<T>.ConcatIterator(_003C_003Ec__DisplayClass24_0<T>.CustomizeIterator(sp));
		});
	}

	internal static void RemoveFromArray<T>(this SerializedProperty v, IEnumerable<T> pred) where T : UnityEngine.Object
	{
		T[] enumerable = (pred as T[]) ?? pred.ToArray();
		v.ForEachTarget(delegate(SerializedProperty sp)
		{
			T[] array = enumerable;
			for (int i = 0; i < array.Length; i++)
			{
				_003C_003Ec__DisplayClass26_1<T> _003C_003Ec__DisplayClass26_ = new _003C_003Ec__DisplayClass26_1<T>();
				_003C_003Ec__DisplayClass26_.e = array[i];
				int num = sp.FindLastIndex(_003C_003Ec__DisplayClass26_.MoveIterator);
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
			param.RemoveFromArray(counter);
		}
		else
		{
			param.AddToArray(counter);
		}
	}

	internal static int FindLastIndex(this SerializedProperty config, Func<SerializedProperty, int, bool> vis)
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

	internal static void ForEachTarget(this SerializedProperty last, Action<SerializedProperty> counter)
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

	internal static bool Toggle(this ref bool instance)
	{
		return instance = !instance;
	}

	internal static Rect SliceLeft(this ref Rect setup, float attr, bool isres = false, float spec2 = -1f, bool getsetup3 = false, bool overrideres4 = true)
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

	internal static void FadeGroup(this AnimBool value, Action result, Action serv = null)
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

	internal static IEnumerable<T> GetFlags<T>(this T task) where T : Enum
	{
		return Enum.GetValues(typeof(T)).Cast<T>().Where(delegate(T value)
		{
			ref T reference = ref task;
			object flag = value;
			return reference.HasFlag((Enum)flag);
		});
	}

	internal static void ForEach<T>(this IEnumerable<T> info, Action<T> pol)
	{
		foreach (T item in info)
		{
			pol(item);
		}
	}

	public static Func<T, bool> And<T>(this Func<T, bool> ident, Func<T, bool> connection)
	{
		return (T arg) => ident(arg) && connection(arg);
	}

	internal static Type FindType(string i)
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

	internal static Dictionary<Transform, Transform> MapTransforms(Transform task, Transform counter, bool isutil, params Transform[] transformsToFind)
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

	internal static Dictionary<T, T> MapComponents<T>(Transform param, Transform connection, bool skipfilter, params T[] componentsToFind) where T : Component
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

	internal static GUIContent GetContent(this SerializedProperty config)
	{
		return new GUIContent(config.displayName, config.tooltip);
	}

	internal static object GetValue(this SerializedProperty ident)
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

	internal static void SetValue(this SerializedProperty ident, object b)
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

	internal static void BeginDeferredCursorRects()
	{
		if (Event.current.type == EventType.Repaint)
		{
			deferringCursorRects = true;
			ClearDeferredCursorRects();
		}
	}

	internal static void ClearDeferredCursorRects()
	{
		DeferredCursorRects.Clear();
	}

	internal static void EndDeferredCursorRects()
	{
		if (Event.current.type == EventType.Repaint)
		{
			deferringCursorRects = false;
			while (DeferredCursorRects.Count > 0)
			{
				var (screenRect, mouse) = DeferredCursorRects.Pop();
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
		using (new GUIColorScope(GUIColorScope.ColoringType.BG, Color.clear))
		{
			using (new GUIColorScope(GUIColorScope.ColoringType.FG, selection.Value))
			{
				bool result = CallStatus(info, MapRef()._SchemaSerializer, GUILayout.ExpandWidth(expand: false));
				MarkAsLink(selection);
				return result;
			}
		}
	}

	internal static void MarkAsLink(Color? item = null)
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

	internal static bool IconButton(GUIContent item, float cont = -1f, float field = -1f)
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
		AddLinkCursor();
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
		AddLinkCursor();
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
		AddLinkCursor();
		return result;
	}

	internal static bool ClickArea(Rect value = default(Rect))
	{
		if (value == default(Rect))
		{
			value = GUILayoutUtility.GetLastRect();
		}
		AddLinkCursor(value);
		Event current = Event.current;
		if (current.type == EventType.MouseDown && current.button == 0)
		{
			return value.Contains(current.mousePosition);
		}
		return false;
	}

	internal static void AddLinkCursor(Rect task = default(Rect), bool readvis = false)
	{
		if (Event.current.type == EventType.Repaint)
		{
			if (task == default(Rect))
			{
				task = GUILayoutUtility.GetLastRect();
			}
			AddCursorRect(task, MouseCursor.Link, readvis);
		}
	}

	internal static void AddCursorRect(Rect i, MouseCursor selection, bool dofilter = false)
	{
		if (!GUI.enabled && !dofilter)
		{
			return;
		}
		if (deferringCursorRects)
		{
			if (IsUnity2022)
			{
				i.y += 46f;
			}
			DeferredCursorRects.Push((GUIUtility.GUIToScreenRect(i), selection));
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

	internal static void Separator(int param_count = 2, int minpred = 10)
	{
		Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.Height(param_count + minpred));
		controlRect.height = param_count;
		controlRect.y += (float)minpred / 2f;
		controlRect.x -= 2f;
		controlRect.width += 6f;
		ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#595959" : "#858585", out var color);
		EditorGUI.DrawRect(controlRect, color);
	}

	internal static bool HasMouseCapture(Rect v, int cfg_X)
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

	internal static void IconSpacer()
	{
		GUILayout.Label(GUIContent.none, GUILayout.Width(EditorGUIUtility.singleLineHeight));
	}

	[SpecialName]
	private static MethodInfo textFieldDropDownMethod()
	{
		return _ClientSerializer ?? (_ClientSerializer = FlushAdapter(typeof(EditorGUI), "TextFieldDropDown", BindingFlags.Static | BindingFlags.NonPublic, (Binder)null, new Type[4]
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
		if (!(textFieldDropDownMethod() != null))
		{
			return ord;
		}
		Rect rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.textField, layoutOptins);
		return (string)textFieldDropDownMethod().Invoke(null, new object[4] { rect, instance, ord, template });
	}

	internal static string RestartStatus(Rect task, string ivk, string consumer, string[] key2)
	{
		if (textFieldDropDownMethod() != null)
		{
			return (string)textFieldDropDownMethod().Invoke(null, new object[4]
			{
				task,
				new GUIContent(ivk),
				consumer,
				key2
			});
		}
		return consumer;
	}

	internal static GUIContent IconContent(string task, string pol = null)
	{
		return new GUIContent(EditorGUIUtility.IconContent(task))
		{
			tooltip = pol
		};
	}

	[SpecialName]
	internal static Contents CustomizeRef()
	{
		return factorySerializer ?? (factorySerializer = new Contents());
	}

	[SpecialName]
	internal static Styles MapRef()
	{
		return _AttributeSerializer ?? (_AttributeSerializer = new Styles());
	}

	internal static bool CommandIssued(EventCommands ident, string connection = "", bool getc = true)
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

	internal static bool KeyPressed(KeyCode ident, string ord = "", bool isstate = true)
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

	internal static bool SubmitPressed(string last = "", bool isord = true)
	{
		if (!KeyPressed(KeyCode.Return, last, isord))
		{
			return KeyPressed(KeyCode.KeypadEnter, last, isord);
		}
		return true;
	}

	internal static bool CancelPressed(string key = "", bool isb = true)
	{
		return KeyPressed(KeyCode.Escape, key, isb);
	}

	internal static bool DeletePressed(string item = "", bool injectcounter = true)
	{
		if (CommandIssued(EventCommands.SoftDelete, item, injectcounter))
		{
			return true;
		}
		return CommandIssued(EventCommands.Delete, item, injectcounter);
	}

	internal static bool SubmitOrCancel(string reference = "", Action ord = null, Action control = null)
	{
		if (SubmitPressed(reference))
		{
			ord?.Invoke();
			return true;
		}
		if (CancelPressed(reference))
		{
			control?.Invoke();
			return true;
		}
		return false;
	}

	internal static bool SubmitOrCancelAndDefocus(string reference, Action result = null, Action filter = null)
	{
		if (!SubmitOrCancel(reference, result, filter))
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
			m_InstanceSerializer = CreateSpindleMesh();
		}
		if (m_TaskSerializer == null)
		{
			m_TaskSerializer = CreateSpindleMaterial();
		}
		ConfigureHandleMaterial(m_TaskSerializer);
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
			m_InstanceSerializer = CreateSpindleMesh();
		}
		if (m_TaskSerializer == null)
		{
			m_TaskSerializer = CreateSpindleMaterial();
		}
		ConfigureHandleMaterial(m_TaskSerializer);
		m_TaskSerializer.SetColor(customerSerializer, temp.Value);
		m_TaskSerializer.SetPass(0);
		Graphics.DrawMeshNow(m_InstanceSerializer, res);
	}

	private static Mesh CreateSpindleMesh()
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

	private static Material CreateSpindleMaterial()
	{
		Material material = new Material(Shader.Find("UI/Unlit/Text"));
		ConfigureHandleMaterial(material);
		return material;
	}

	private static void ConfigureHandleMaterial(Material last)
	{
		last.hideFlags = HideFlags.DontSave;
		last.SetInt("_Cull", 2);
		last.SetInt("_ZWrite", 0);
		last.SetInt("_ZTest", 8);
	}

	internal static void DrawSphereHandle(SphereHandle first)
	{
		Event current = Event.current;
		first.onDraw?.Invoke(first);
		int controlId = first.controlId;
		switch (current.GetTypeForControl(controlId))
		{
		case EventType.MouseDown:
			if (HandleUtility.nearestControl == controlId && current.button == 0)
			{
				first.onClick();
				current.Use();
			}
			break;
		case EventType.Layout:
		{
			float[] distances = first.GetDistances();
			foreach (float distance in distances)
			{
				HandleUtility.AddControl(controlId, distance);
			}
			break;
		}
		}
	}

	internal static void TransformHandles(Transform instance, bool counterinstall = false, bool skipthird = false, bool readparam2 = false, bool usecaller3 = false, bool ismap4 = false, bool bool_0 = false)
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
		if (!IsUnity2022)
		{
			reference.y += 40f;
		}
		reference.height -= ((!IsUnity2022) ? 21f : 27f);
		return reference;
	}

	internal static float RadiusHandle(Quaternion param, Vector3 vis, float comp, bool iscol2 = true, float map3 = 1f)
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

	internal static void ShowObjectSelector(UnityEngine.Object i, Type ord, UnityEngine.Object comp = null, SerializedProperty reference2 = null, bool requirescol3 = true, List<int> param4 = null, Action<UnityEngine.Object> selection5 = null, Action<UnityEngine.Object> counter6 = null, bool isinstance7 = true)
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

	internal static void OverrideCustomEditor(Type config, Type selection)
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
		RefreshInspectors();
	}

	internal static void RefreshInspectors()
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

	private static Texture2D TrimTransparentBorder(Texture2D info, float second = 0.2f, int consumer_max = 1)
	{
		if (info == null)
		{
			throw new ArgumentNullException("texture");
		}
		using ReadableTexture readableTexture = new ReadableTexture(info);
		Texture2D texture = readableTexture.texture;
		int width = texture.width;
		int height = texture.height;
		int num = width;
		int num2 = 0;
		int num3 = height;
		int num4 = 0;
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				if (texture.GetPixel(j, i).a >= second)
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
			Color[] pixels = texture.GetPixels(num, num3, num5, num6);
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

	internal static Texture2D SolidColorTexture(Color info)
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

	internal static CachedIcon TrimmedIcon(string asset, string ivk, string field = "")
	{
		Texture2D last = null;
		GUIContent gUIContent = EditorGUIUtility.IconContent(asset);
		if (gUIContent != null && gUIContent.image != null)
		{
			last = TrimTransparentBorder(gUIContent.image as Texture2D);
		}
		return new CachedIcon(last, ivk, field);
	}

	internal static VRCContactSender CompareVal(this VRCContactReceiver init, GameObject vis)
	{
		VRCContactSender vRCContactSender = Undo.AddComponent<VRCContactSender>(vis);
		new ShapeSnapshot(init).Apply(vRCContactSender);
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
		new ShapeSnapshot(res).Apply(vRCContactSender);
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
		new ShapeSnapshot(first).Apply(vRCContactReceiver);
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
		new ShapeSnapshot(param).Apply(vRCContactReceiver);
		vRCContactReceiver.rootTransform = param.rootTransform;
		if (vRCContactReceiver.rootTransform == vRCContactReceiver.transform)
		{
			while (true)
			{
				vRCContactReceiver.rootTransform = null;
			}
		}
		return vRCContactReceiver;
	}

	internal static VRCPhysBoneCollider InvokeVal(this VRCContactReceiver setup, GameObject cfg)
	{
		VRCPhysBoneCollider vRCPhysBoneCollider = Undo.AddComponent<VRCPhysBoneCollider>(cfg);
		new ShapeSnapshot(setup).Apply(vRCPhysBoneCollider);
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
		new ShapeSnapshot(key).Apply(vRCPhysBoneCollider);
		vRCPhysBoneCollider.rootTransform = key.rootTransform;
		if (vRCPhysBoneCollider.rootTransform == vRCPhysBoneCollider.transform)
		{
			vRCPhysBoneCollider.rootTransform = null;
		}
		return vRCPhysBoneCollider;
	}

	internal static void GetPopulatedPlayableLayers(VRCAvatarDescriptor param, ref string[] attr, ref int[] util)
	{
		string[] array = new string[8] { "Base", "Additive", "Gesture", "Action", "FX", "Sitting", "TPose", "IKPose" };
		if ((bool)(UnityEngine.Object)(object)param)
		{
			List<(string, int)> list = new List<(string, int)>();
			for (int i = 0; i < array.Length; i++)
			{
				int num = ((i != 0) ? (i + 1) : i);
				if (param.TryGetAnimatorController((VRCAvatarDescriptor.AnimLayerType)num, out var _))
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

	internal static bool TryGetAnimatorController(this VRCAvatarDescriptor init, VRCAvatarDescriptor.AnimLayerType b, out AnimatorController dic)
	{
		dic = (from l in init.baseAnimationLayers.Concat(init.specialAnimationLayers)
			where l.type == b
			select l.animatorController).FirstOrDefault() as AnimatorController;
		return dic != null;
	}

	internal static bool CycleToggleState(byte[] reference, int numcol, bool calccontrol = true)
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

	static MethodInfo FlushAdapter(Type type_0, string spec, BindingFlags ord, Binder pool, Type[] ivk2, ParameterModifier[] pred3)
	{
		return type_0.GetMethod(spec, ord, pool, ivk2, pred3);
	}
}
