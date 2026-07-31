using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

internal static class AnimatorTypeCache
{
	internal class ParameterDriverBinding
	{
		internal class ParameterEntry
		{
			internal enum ChangeType
			{
				Set,
				Add,
				Random
			}

			internal ParameterDriverBinding _DispatcherProperty;

			internal SerializedProperty m_RegistryProperty;

			private bool _TagProperty;

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
					while (!RemovePage())
					{
						_DispatcherProperty.DefinePage();
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

			internal ParameterEntry(ParameterDriverBinding param, SerializedProperty b)
			{
				_DispatcherProperty = param;
				m_RegistryProperty = b;
			}
		}

		internal StateMachineBehaviour _StrategyProperty;

		internal SerializedObject m_CustomerProperty;

		internal List<ParameterEntry> m_DatabaseProperty = new List<ParameterEntry>();

		internal SerializedProperty m_ExporterProperty;

		private readonly SerializedProperty _IdentifierProperty;

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

		internal ParameterDriverBinding(StateMachineBehaviour value)
		{
			_StrategyProperty = value;
			m_CustomerProperty = new SerializedObject(value);
			m_ExporterProperty = m_CustomerProperty.FindProperty("parameters");
			_IdentifierProperty = m_CustomerProperty.FindProperty("localOnly");
			for (int i = 0; i < m_ExporterProperty.arraySize; i++)
			{
				m_DatabaseProperty.Add(new ParameterEntry(this, m_ExporterProperty.GetArrayElementAtIndex(i)));
			}
		}

		internal ParameterEntry ExcludePage(int instance_Z)
		{
			return new ParameterEntry(this, m_ExporterProperty.GetArrayElementAtIndex(instance_Z));
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

		internal ParameterEntry VisitPage()
		{
			m_ExporterProperty.InsertArrayElementAtIndex(m_ExporterProperty.arraySize);
			m_DatabaseProperty.Add(new ParameterEntry(this, m_ExporterProperty.GetArrayElementAtIndex(m_ExporterProperty.arraySize - 1)));
			m_CustomerProperty.ApplyModifiedProperties();
			return ExcludePage(m_ExporterProperty.arraySize - 1);
		}

		internal void DefinePage()
		{
			m_CustomerProperty.ApplyModifiedProperties();
		}
	}

	internal class AvatarDescriptorBinding
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

		internal AvatarDescriptorBinding(Component def)
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
	}

	[DefaultMember("Item")]
	internal class ExpressionsMenuBinding : SerializedObjectWrapper
	{
		internal readonly SerializedPropertyWrapper controls;

		internal ExpressionsMenuBinding(UnityEngine.Object spec)
			: base(spec)
		{
			controls = FindProperty("controls");
		}

		[SpecialName]
		public MenuControlBinding GetControl(int index_spec)
		{
			return new MenuControlBinding(controls.Item(index_spec));
		}
	}

	internal class MenuControlBinding : SerializedPropertyWrapper
	{
		[SpecialName]
		internal string GetName()
		{
			return Item("name").property.stringValue;
		}

		[SpecialName]
		internal void SetName(string res)
		{
			Item("name").property.stringValue = res;
		}

		[SpecialName]
		internal string GetParameterName()
		{
			return Item("parameter").Item("name").property.stringValue;
		}

		[SpecialName]
		internal void SetParameterName(string info)
		{
			Item("parameter").Item("name").property.stringValue = info;
		}

		[SpecialName]
		internal Texture2D GetIcon()
		{
			return (Texture2D)Item("icon").property.objectReferenceValue;
		}

		[SpecialName]
		internal void SetIcon(Texture2D param)
		{
			Item("icon").property.objectReferenceValue = param;
		}

		[SpecialName]
		internal ExpressionsMenuBinding GetSubmenu()
		{
			return new ExpressionsMenuBinding(Item("submenu").property.objectReferenceValue);
		}

		[SpecialName]
		internal void SetSubmenu(ExpressionsMenuBinding item)
		{
			Item("submenu").property.objectReferenceValue = item.targetObject;
		}

		public MenuControlBinding(SerializedProperty key)
			: base(key)
		{
		}
	}

	[DefaultMember("Item")]
	internal class SerializedObjectWrapper : SerializedObject
	{
		internal SerializedObjectWrapper(UnityEngine.Object info)
			: base(info)
		{
		}

		[SpecialName]
		public new SerializedPropertyWrapper FindProperty(string i)
		{
			return new SerializedPropertyWrapper(base.FindProperty(i));
		}
	}

	[DefaultMember("Item")]
	internal class SerializedPropertyWrapper
	{
		internal readonly SerializedProperty property;

		public SerializedPropertyWrapper(SerializedProperty setup)
		{
			property = setup;
		}

		[SpecialName]
		public SerializedPropertyWrapper Item(int mini)
		{
			return new SerializedPropertyWrapper(property.GetArrayElementAtIndex(mini));
		}

		[SpecialName]
		public SerializedPropertyWrapper Item(string instance)
		{
			return new SerializedPropertyWrapper(property.FindPropertyRelative(instance));
		}

		public static implicit operator SerializedProperty(SerializedPropertyWrapper last)
		{
			return last.property;
		}
	}

	private static Dictionary<string, Type> typeCache;

	private static bool sdkAvailable;

	private static bool hasChecked;

	[SpecialName]
	internal static Type GetAvatarDescriptorType()
	{
		return ResolveVRCType("VRCAvatarDescriptor");
	}

	[SpecialName]
	internal static Type GetParameterDriverType()
	{
		return ResolveVRCType("VRCAvatarParameterDriver");
	}

	[SpecialName]
	internal static Type GetTrackingControlType()
	{
		return ResolveVRCType("VRCAnimatorTrackingControl");
	}

	[SpecialName]
	internal static bool IsVRCSDKAvailable()
	{
		if (!hasChecked)
		{
			ResolveVRCType("VRCAvatarDescriptor");
		}
		return sdkAvailable;
	}

	internal static Type ResolveVRCType(string spec)
	{
		hasChecked = true;
		if (typeCache == null)
		{
			string text = "VRCAvatarDescriptor";
			typeCache = new Dictionary<string, Type>();
			Type type = EditorUtils.ForgotRules(text);
			if (type != null)
			{
				sdkAvailable = true;
				typeCache.Add(text, type);
			}
		}
		if (sdkAvailable)
		{
			if (typeCache.TryGetValue(spec, out var value))
			{
				return value;
			}
			Type type2 = EditorUtils.ForgotRules(spec);
			typeCache.Add(spec, type2);
			return type2;
		}
		return null;
	}
}
