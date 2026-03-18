using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal static class WorkerProperty
{
	internal class BridgeProperty
	{
		internal class AttrProperty
		{
			internal enum ChangeType
			{
				Set,
				Add,
				Random
			}

			internal BridgeProperty _DispatcherProperty;

			internal SerializedProperty m_RegistryProperty;

			private bool _TagProperty;

			internal static AttrProperty ListStruct;

			[SpecialName]
			internal bool RemovePage()
			{
				return _TagProperty;
			}

			[SpecialName]
			internal void InstantiatePage(bool validatekey)
			{
				if (_TagProperty && !validatekey)
				{
					_DispatcherProperty.DefinePage();
				}
				_TagProperty = validatekey;
			}

			[SpecialName]
			internal string ResetPage()
			{
				return m_RegistryProperty.FindPropertyRelative("name").stringValue;
			}

			[SpecialName]
			internal void FlushPage(string def)
			{
				m_RegistryProperty.FindPropertyRelative("name").stringValue = def;
				if (!RemovePage())
				{
					_DispatcherProperty.DefinePage();
				}
			}

			[SpecialName]
			internal string CalculatePage()
			{
				try
				{
					return m_RegistryProperty.FindPropertyRelative("source").stringValue;
				}
				catch
				{
					return string.Empty;
				}
			}

			[SpecialName]
			internal void TestPage(string spec)
			{
				try
				{
					m_RegistryProperty.FindPropertyRelative("source").stringValue = spec;
					uint num4 = default(uint);
					while (true)
					{
						int num;
						int num2;
						if (!RemovePage())
						{
							num = -2113787510;
							num2 = -2113787510;
						}
						else
						{
							num = -1526977687;
							num2 = -1526977687;
						}
						int num3 = num ^ ((int)num4 * -1344518119);
						while (true)
						{
							switch ((num4 = (uint)(num3 ^ -692394164)) % 4)
							{
							default:
								return;
							case 0u:
							case 3u:
								break;
							case 1u:
								goto IL_0057;
							case 2u:
								return;
							}
							break;
							IL_0057:
							_DispatcherProperty.DefinePage();
							num3 = ((int)num4 * -456668155) ^ 0x9B8C1A3;
						}
					}
				}
				catch
				{
				}
			}

			[SpecialName]
			internal float ValidatePage()
			{
				return m_RegistryProperty.FindPropertyRelative("value").floatValue;
			}

			[SpecialName]
			internal void CustomizePage(float info)
			{
				m_RegistryProperty.FindPropertyRelative("value").floatValue = info;
				if (!RemovePage())
				{
					_DispatcherProperty.DefinePage();
				}
			}

			[SpecialName]
			internal float DestroyPage()
			{
				return m_RegistryProperty.FindPropertyRelative("chance").floatValue;
			}

			[SpecialName]
			internal void GetPage(float def)
			{
				m_RegistryProperty.FindPropertyRelative("chance").floatValue = def;
				if (!RemovePage())
				{
					_DispatcherProperty.DefinePage();
				}
			}

			[SpecialName]
			internal float IncludePage()
			{
				return m_RegistryProperty.FindPropertyRelative("valueMin").floatValue;
			}

			[SpecialName]
			internal void RunPage(float setup)
			{
				m_RegistryProperty.FindPropertyRelative("valueMin").floatValue = setup;
				if (!RemovePage())
				{
					_DispatcherProperty.DefinePage();
				}
			}

			[SpecialName]
			internal float LoginPage()
			{
				return m_RegistryProperty.FindPropertyRelative("valueMax").floatValue;
			}

			[SpecialName]
			internal void ReflectPage(float setup)
			{
				m_RegistryProperty.FindPropertyRelative("valueMax").floatValue = setup;
				if (!RemovePage())
				{
					_DispatcherProperty.DefinePage();
				}
			}

			[SpecialName]
			internal ChangeType CreatePage()
			{
				return (ChangeType)m_RegistryProperty.FindPropertyRelative("type").enumValueIndex;
			}

			[SpecialName]
			internal void NewPage(ChangeType instance)
			{
				m_RegistryProperty.FindPropertyRelative("type").enumValueIndex = (int)instance;
				_DispatcherProperty.m_CustomerProperty.ApplyModifiedProperties();
			}

			internal AttrProperty(BridgeProperty param, SerializedProperty b)
			{
				_DispatcherProperty = param;
				m_RegistryProperty = b;
			}

			internal static bool CalcStruct()
			{
				return ListStruct == null;
			}
		}

		internal StateMachineBehaviour _StrategyProperty;

		internal SerializedObject m_CustomerProperty;

		internal List<AttrProperty> m_DatabaseProperty = new List<AttrProperty>();

		internal SerializedProperty m_ExporterProperty;

		private readonly SerializedProperty _IdentifierProperty;

		private static BridgeProperty SetupStruct;

		[SpecialName]
		internal bool StartPage()
		{
			return _IdentifierProperty.boolValue;
		}

		[SpecialName]
		internal void ReadPage(bool instanceinstall)
		{
			_IdentifierProperty.boolValue = instanceinstall;
			DefinePage();
		}

		internal BridgeProperty(StateMachineBehaviour value)
		{
			_StrategyProperty = value;
			m_CustomerProperty = new SerializedObject(value);
			m_ExporterProperty = m_CustomerProperty.FindProperty("parameters");
			_IdentifierProperty = m_CustomerProperty.FindProperty("localOnly");
			for (int i = 0; i < m_ExporterProperty.arraySize; i++)
			{
				m_DatabaseProperty.Add(new AttrProperty(this, m_ExporterProperty.GetArrayElementAtIndex(i)));
			}
		}

		internal AttrProperty ExcludePage(int instance_Z)
		{
			return new AttrProperty(this, m_ExporterProperty.GetArrayElementAtIndex(instance_Z));
		}

		internal bool InitPage(int init)
		{
			m_DatabaseProperty.RemoveAt(init);
			m_ExporterProperty.DeleteArrayElementAtIndex(init);
			m_CustomerProperty.ApplyModifiedProperties();
			if (m_ExporterProperty.arraySize != 0)
			{
				return false;
			}
			return true;
		}

		internal AttrProperty VisitPage()
		{
			m_ExporterProperty.InsertArrayElementAtIndex(m_ExporterProperty.arraySize);
			m_DatabaseProperty.Add(new AttrProperty(this, m_ExporterProperty.GetArrayElementAtIndex(m_ExporterProperty.arraySize - 1)));
			m_CustomerProperty.ApplyModifiedProperties();
			return ExcludePage(m_ExporterProperty.arraySize - 1);
		}

		internal void DefinePage()
		{
			m_CustomerProperty.ApplyModifiedProperties();
		}

		internal static bool ExcludeStruct()
		{
			return SetupStruct == null;
		}
	}

	internal class ImporterProperty
	{
		private Component _RequestProperty;

		private SerializedObject _PrinterProperty;

		internal SerializedProperty writerProperty;

		internal SerializedProperty _ParamsProperty;

		internal SerializedProperty listenerProperty;

		internal SerializedProperty _GetterProperty;

		internal SerializedProperty interceptorProperty;

		internal SerializedProperty creatorProperty;

		internal SerializedProperty m_EventProperty;

		internal SerializedProperty m_InfoProperty;

		internal SerializedProperty _FacadeProperty;

		internal SerializedProperty m_AdvisorProperty;

		internal SerializedProperty _CallbackProperty;

		internal SerializedProperty indexerProperty;

		internal SerializedProperty _IssuerProperty;

		internal SerializedProperty _PrototypeProperty;

		internal SerializedProperty _RuleProperty;

		internal SerializedProperty _SingletonProperty;

		internal SerializedProperty m_FactoryProperty;

		internal SerializedProperty m_AccountProperty;

		internal SerializedProperty _RefProperty;

		internal SerializedProperty statusProperty;

		internal SerializedProperty tokenProperty;

		internal SerializedProperty _CodeProperty;

		internal SerializedProperty dicProperty;

		internal SerializedProperty m_InvocationProperty;

		internal SerializedProperty _RoleProperty;

		internal SerializedProperty paramProperty;

		internal SerializedProperty _ModelProperty;

		internal SerializedProperty _TokenizerProperty;

		internal SerializedProperty decoratorProperty;

		internal SerializedProperty comparatorProperty;

		internal SerializedProperty _ExceptionProperty;

		internal SerializedProperty _ObjectProperty;

		internal SerializedProperty _UtilsProperty;

		internal SerializedProperty _ValProperty;

		internal SerializedProperty m_ValueProperty;

		internal SerializedProperty _MerchantProperty;

		internal SerializedProperty authenticationProperty;

		internal SerializedProperty _ReponseProperty;

		internal SerializedProperty m_PoolProperty = new SerializedObject((UnityEngine.Object)null).FindProperty("collider_fingerIndexL");

		private static ImporterProperty InstantiateStruct;

		internal ImporterProperty(Component def)
		{
			_RequestProperty = def;
			_PrinterProperty = new SerializedObject(def);
			writerProperty = _PrinterProperty.FindProperty("ViewPosition");
			_ParamsProperty = _PrinterProperty.FindProperty("Animations");
			listenerProperty = _PrinterProperty.FindProperty("ScaleIPD");
			_GetterProperty = _PrinterProperty.FindProperty("lipSync");
			interceptorProperty = _PrinterProperty.FindProperty("lipSyncJawBone");
			creatorProperty = _PrinterProperty.FindProperty("lipSyncJawClosed");
			m_EventProperty = _PrinterProperty.FindProperty("lipSyncJawOpen");
			m_InfoProperty = _PrinterProperty.FindProperty("VisemeSkinnedMesh");
			_FacadeProperty = _PrinterProperty.FindProperty("MouthOpenBlendShapeName");
			m_AdvisorProperty = _PrinterProperty.FindProperty("VisemeBlendShapes");
			_CallbackProperty = _PrinterProperty.FindProperty("unityVersion");
			indexerProperty = _PrinterProperty.FindProperty("portraitCameraPositionOffset");
			_IssuerProperty = _PrinterProperty.FindProperty("portraitCameraRotationOffset");
			_PrototypeProperty = _PrinterProperty.FindProperty("customExpressions");
			_RuleProperty = _PrinterProperty.FindProperty("expressionsMenu");
			_SingletonProperty = _PrinterProperty.FindProperty("expressionParameters");
			m_FactoryProperty = _PrinterProperty.FindProperty("enableEyeLook");
			m_AccountProperty = _PrinterProperty.FindProperty("customEyeLookSettings");
			_RefProperty = _PrinterProperty.FindProperty("customizeAnimationLayers");
			statusProperty = _PrinterProperty.FindProperty("baseAnimationLayers");
			tokenProperty = _PrinterProperty.FindProperty("specialAnimationLayers");
			_CodeProperty = _PrinterProperty.FindProperty("AnimationPreset");
			dicProperty = _PrinterProperty.FindProperty("animationHashSet");
			m_InvocationProperty = _PrinterProperty.FindProperty("autoFootsteps");
			_RoleProperty = _PrinterProperty.FindProperty("autoLocomotion");
			paramProperty = _PrinterProperty.FindProperty("collider_head");
			_ModelProperty = _PrinterProperty.FindProperty("collider_torso");
			_TokenizerProperty = _PrinterProperty.FindProperty("collider_footR");
			decoratorProperty = _PrinterProperty.FindProperty("collider_footL");
			comparatorProperty = _PrinterProperty.FindProperty("collider_handR");
			_ExceptionProperty = _PrinterProperty.FindProperty("collider_handL");
			_ObjectProperty = _PrinterProperty.FindProperty("collider_fingerIndexL");
			_UtilsProperty = _PrinterProperty.FindProperty("collider_fingerMiddleL");
			_ValProperty = _PrinterProperty.FindProperty("collider_fingerRingL");
			m_ValueProperty = _PrinterProperty.FindProperty("collider_fingerLittleL");
			_MerchantProperty = _PrinterProperty.FindProperty("collider_fingerIndexR");
			authenticationProperty = _PrinterProperty.FindProperty("collider_fingerMiddleR");
			_ReponseProperty = _PrinterProperty.FindProperty("collider_fingerRingR");
			m_PoolProperty = _PrinterProperty.FindProperty("collider_fingerLittleR");
		}

		internal static bool RevertStruct()
		{
			return InstantiateStruct == null;
		}
	}

	[DefaultMember("Item")]
	internal class ParameterProperty : MappingProperty
	{
		internal readonly BaseProperty _ComposerProperty;

		private static ParameterProperty RegisterCandidate;

		internal ParameterProperty(UnityEngine.Object spec)
			: base(spec)
		{
			_ComposerProperty = SortPage("controls");
		}

		[SpecialName]
		public RepositoryProperty ViewPage(int index_spec)
		{
			return new RepositoryProperty(_ComposerProperty.LogoutPage(index_spec));
		}

		internal static bool FlushCandidate()
		{
			return RegisterCandidate == null;
		}
	}

	internal class RepositoryProperty : BaseProperty
	{
		private static RepositoryProperty CustomizeCandidate;

		[SpecialName]
		internal string ResolvePage()
		{
			return InterruptPage("name").m_ContainerProperty.stringValue;
		}

		[SpecialName]
		internal void ListPage(string res)
		{
			InterruptPage("name").m_ContainerProperty.stringValue = res;
		}

		[SpecialName]
		internal string FillPage()
		{
			return InterruptPage("parameter").InterruptPage("name").m_ContainerProperty.stringValue;
		}

		[SpecialName]
		internal void WritePage(string info)
		{
			InterruptPage("parameter").InterruptPage("name").m_ContainerProperty.stringValue = info;
		}

		[SpecialName]
		internal Texture2D StopPage()
		{
			return (Texture2D)InterruptPage("icon").m_ContainerProperty.objectReferenceValue;
		}

		[SpecialName]
		internal void CheckPage(Texture2D param)
		{
			InterruptPage("icon").m_ContainerProperty.objectReferenceValue = param;
		}

		[SpecialName]
		internal ParameterProperty AssetPage()
		{
			return new ParameterProperty(InterruptPage("submenu").m_ContainerProperty.objectReferenceValue);
		}

		[SpecialName]
		internal void UpdatePage(ParameterProperty item)
		{
			InterruptPage("submenu").m_ContainerProperty.objectReferenceValue = item.targetObject;
		}

		public RepositoryProperty(SerializedProperty key)
			: base(key)
		{
		}

		internal static bool SearchCandidate()
		{
			return CustomizeCandidate == null;
		}
	}

	[DefaultMember("Item")]
	internal class MappingProperty : SerializedObject
	{
		internal static MappingProperty CancelCandidate;

		internal MappingProperty(UnityEngine.Object info)
			: base(info)
		{
		}

		[SpecialName]
		public BaseProperty SortPage(string i)
		{
			return new BaseProperty(FindProperty(i));
		}

		internal static bool RestartCandidate()
		{
			return CancelCandidate == null;
		}
	}

	[DefaultMember("Item")]
	internal class BaseProperty
	{
		internal readonly SerializedProperty m_ContainerProperty;

		internal static BaseProperty FillCandidate;

		public BaseProperty(SerializedProperty setup)
		{
			m_ContainerProperty = setup;
		}

		[SpecialName]
		public BaseProperty LogoutPage(int mini)
		{
			return new BaseProperty(m_ContainerProperty.GetArrayElementAtIndex(mini));
		}

		[SpecialName]
		public BaseProperty InterruptPage(string instance)
		{
			return new BaseProperty(m_ContainerProperty.FindPropertyRelative(instance));
		}

		public static implicit operator SerializedProperty(BaseProperty last)
		{
			return last.m_ContainerProperty;
		}

		internal static bool DeleteCandidate()
		{
			return FillCandidate == null;
		}
	}

	private static Dictionary<string, Type> m_FilterProperty;

	private static bool stubProperty;

	private static bool readerProperty;

	internal static WorkerProperty AssetStruct;

	[SpecialName]
	internal static Type CountPage()
	{
		return CancelPage("VRCAvatarDescriptor");
	}

	[SpecialName]
	internal static Type InsertPage()
	{
		return CancelPage("VRCAvatarParameterDriver");
	}

	[SpecialName]
	internal static Type QueryPage()
	{
		return CancelPage("VRCAnimatorTrackingControl");
	}

	[SpecialName]
	internal static bool InvokePage()
	{
		if (!readerProperty)
		{
			CancelPage("VRCAvatarDescriptor");
		}
		return stubProperty;
	}

	internal static Type CancelPage(string spec)
	{
		readerProperty = true;
		if (m_FilterProperty == null)
		{
			string text = "VRCAvatarDescriptor";
			m_FilterProperty = new Dictionary<string, Type>();
			Type type = ClassProperty.ForgotRules(text);
			if (type != null)
			{
				stubProperty = true;
				m_FilterProperty.Add(text, type);
			}
		}
		if (stubProperty)
		{
			if (m_FilterProperty.TryGetValue(spec, out var value))
			{
				return value;
			}
			Type type2 = ClassProperty.ForgotRules(spec);
			m_FilterProperty.Add(spec, type2);
			return type2;
		}
		return null;
	}

	internal static bool SelectStruct()
	{
		return AssetStruct == null;
	}
}
