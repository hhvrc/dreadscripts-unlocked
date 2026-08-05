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

			internal ParameterDriverBinding driver;

			internal SerializedProperty property;

			private bool deferApply;

			[SpecialName]
			internal bool GetDeferApply()
			{
				return deferApply;
			}

			[SpecialName]
			internal void SetDeferApply(bool validatekey)
			{
				if (deferApply && !validatekey)
				{
					driver.Apply();
				}
				deferApply = validatekey;
			}

			[SpecialName]
			internal string GetName()
			{
				return property.FindPropertyRelative("name").stringValue;
			}

			[SpecialName]
			internal void SetName(string def)
			{
				property.FindPropertyRelative("name").stringValue = def;
				if (!GetDeferApply())
				{
					driver.Apply();
				}
			}

			[SpecialName]
			internal string GetSource()
			{
				try
				{
					return property.FindPropertyRelative("source").stringValue;
				}
				catch
				{
					return string.Empty;
				}
			}

			[SpecialName]
			internal void SetSource(string spec)
			{
				try
				{
					property.FindPropertyRelative("source").stringValue = spec;
					while (!GetDeferApply())
					{
						driver.Apply();
					}
				}
				catch
				{
				}
			}

			[SpecialName]
			internal float GetValue()
			{
				return property.FindPropertyRelative("value").floatValue;
			}

			[SpecialName]
			internal void SetValue(float info)
			{
				property.FindPropertyRelative("value").floatValue = info;
				if (!GetDeferApply())
				{
					driver.Apply();
				}
			}

			[SpecialName]
			internal float GetChance()
			{
				return property.FindPropertyRelative("chance").floatValue;
			}

			[SpecialName]
			internal void SetChance(float def)
			{
				property.FindPropertyRelative("chance").floatValue = def;
				if (!GetDeferApply())
				{
					driver.Apply();
				}
			}

			[SpecialName]
			internal float GetValueMin()
			{
				return property.FindPropertyRelative("valueMin").floatValue;
			}

			[SpecialName]
			internal void SetValueMin(float setup)
			{
				property.FindPropertyRelative("valueMin").floatValue = setup;
				if (!GetDeferApply())
				{
					driver.Apply();
				}
			}

			[SpecialName]
			internal float GetValueMax()
			{
				return property.FindPropertyRelative("valueMax").floatValue;
			}

			[SpecialName]
			internal void SetValueMax(float setup)
			{
				property.FindPropertyRelative("valueMax").floatValue = setup;
				if (!GetDeferApply())
				{
					driver.Apply();
				}
			}

			[SpecialName]
			internal ChangeType GetChangeType()
			{
				return (ChangeType)property.FindPropertyRelative("type").enumValueIndex;
			}

			[SpecialName]
			internal void SetChangeType(ChangeType instance)
			{
				property.FindPropertyRelative("type").enumValueIndex = (int)instance;
				driver.serializedObject.ApplyModifiedProperties();
			}

			internal ParameterEntry(ParameterDriverBinding param, SerializedProperty b)
			{
				driver = param;
				property = b;
			}
		}

		internal StateMachineBehaviour behaviour;

		internal SerializedObject serializedObject;

		internal List<ParameterEntry> parameters = new List<ParameterEntry>();

		internal SerializedProperty parametersProperty;

		private readonly SerializedProperty localOnlyProperty;

		[SpecialName]
		internal bool GetLocalOnly()
		{
			return localOnlyProperty.boolValue;
		}

		[SpecialName]
		internal void SetLocalOnly(bool instanceinstall)
		{
			localOnlyProperty.boolValue = instanceinstall;
			Apply();
		}

		internal ParameterDriverBinding(StateMachineBehaviour value)
		{
			behaviour = value;
			serializedObject = new SerializedObject(value);
			parametersProperty = serializedObject.FindProperty("parameters");
			localOnlyProperty = serializedObject.FindProperty("localOnly");
			for (int i = 0; i < parametersProperty.arraySize; i++)
			{
				parameters.Add(new ParameterEntry(this, parametersProperty.GetArrayElementAtIndex(i)));
			}
		}

		internal ParameterEntry GetParameter(int instance_Z)
		{
			return new ParameterEntry(this, parametersProperty.GetArrayElementAtIndex(instance_Z));
		}

		internal bool RemoveParameter(int init)
		{
			parameters.RemoveAt(init);
			parametersProperty.DeleteArrayElementAtIndex(init);
			serializedObject.ApplyModifiedProperties();
			if (parametersProperty.arraySize != 0)
			{
				return false;
			}
			return true;
		}

		internal ParameterEntry AddParameter()
		{
			parametersProperty.InsertArrayElementAtIndex(parametersProperty.arraySize);
			parameters.Add(new ParameterEntry(this, parametersProperty.GetArrayElementAtIndex(parametersProperty.arraySize - 1)));
			serializedObject.ApplyModifiedProperties();
			return GetParameter(parametersProperty.arraySize - 1);
		}

		internal void Apply()
		{
			serializedObject.ApplyModifiedProperties();
		}
	}

	internal class AvatarDescriptorBinding
	{
		private Component descriptor;

		private SerializedObject serializedObject;

		internal SerializedProperty viewPositionProperty;

		internal SerializedProperty animationsProperty;

		internal SerializedProperty scaleIPDProperty;

		internal SerializedProperty lipSyncProperty;

		internal SerializedProperty lipSyncJawBoneProperty;

		internal SerializedProperty lipSyncJawClosedProperty;

		internal SerializedProperty lipSyncJawOpenProperty;

		internal SerializedProperty visemeSkinnedMeshProperty;

		internal SerializedProperty mouthOpenBlendShapeNameProperty;

		internal SerializedProperty visemeBlendShapesProperty;

		internal SerializedProperty unityVersionProperty;

		internal SerializedProperty portraitCameraPositionOffsetProperty;

		internal SerializedProperty portraitCameraRotationOffsetProperty;

		internal SerializedProperty customExpressionsProperty;

		internal SerializedProperty expressionsMenuProperty;

		internal SerializedProperty expressionParametersProperty;

		internal SerializedProperty enableEyeLookProperty;

		internal SerializedProperty customEyeLookSettingsProperty;

		internal SerializedProperty customizeAnimationLayersProperty;

		internal SerializedProperty baseAnimationLayersProperty;

		internal SerializedProperty specialAnimationLayersProperty;

		internal SerializedProperty animationPresetProperty;

		internal SerializedProperty animationHashSetProperty;

		internal SerializedProperty autoFootstepsProperty;

		internal SerializedProperty autoLocomotionProperty;

		internal SerializedProperty colliderHeadProperty;

		internal SerializedProperty colliderTorsoProperty;

		internal SerializedProperty colliderFootRProperty;

		internal SerializedProperty colliderFootLProperty;

		internal SerializedProperty colliderHandRProperty;

		internal SerializedProperty colliderHandLProperty;

		internal SerializedProperty colliderFingerIndexLProperty;

		internal SerializedProperty colliderFingerMiddleLProperty;

		internal SerializedProperty colliderFingerRingLProperty;

		internal SerializedProperty colliderFingerLittleLProperty;

		internal SerializedProperty colliderFingerIndexRProperty;

		internal SerializedProperty colliderFingerMiddleRProperty;

		internal SerializedProperty colliderFingerRingRProperty;

		internal SerializedProperty colliderFingerLittleRProperty = new SerializedObject((UnityEngine.Object)null).FindProperty("collider_fingerIndexL");

		internal AvatarDescriptorBinding(Component def)
		{
			descriptor = def;
			serializedObject = new SerializedObject(def);
			viewPositionProperty = serializedObject.FindProperty("ViewPosition");
			animationsProperty = serializedObject.FindProperty("Animations");
			scaleIPDProperty = serializedObject.FindProperty("ScaleIPD");
			lipSyncProperty = serializedObject.FindProperty("lipSync");
			lipSyncJawBoneProperty = serializedObject.FindProperty("lipSyncJawBone");
			lipSyncJawClosedProperty = serializedObject.FindProperty("lipSyncJawClosed");
			lipSyncJawOpenProperty = serializedObject.FindProperty("lipSyncJawOpen");
			visemeSkinnedMeshProperty = serializedObject.FindProperty("VisemeSkinnedMesh");
			mouthOpenBlendShapeNameProperty = serializedObject.FindProperty("MouthOpenBlendShapeName");
			visemeBlendShapesProperty = serializedObject.FindProperty("VisemeBlendShapes");
			unityVersionProperty = serializedObject.FindProperty("unityVersion");
			portraitCameraPositionOffsetProperty = serializedObject.FindProperty("portraitCameraPositionOffset");
			portraitCameraRotationOffsetProperty = serializedObject.FindProperty("portraitCameraRotationOffset");
			customExpressionsProperty = serializedObject.FindProperty("customExpressions");
			expressionsMenuProperty = serializedObject.FindProperty("expressionsMenu");
			expressionParametersProperty = serializedObject.FindProperty("expressionParameters");
			enableEyeLookProperty = serializedObject.FindProperty("enableEyeLook");
			customEyeLookSettingsProperty = serializedObject.FindProperty("customEyeLookSettings");
			customizeAnimationLayersProperty = serializedObject.FindProperty("customizeAnimationLayers");
			baseAnimationLayersProperty = serializedObject.FindProperty("baseAnimationLayers");
			specialAnimationLayersProperty = serializedObject.FindProperty("specialAnimationLayers");
			animationPresetProperty = serializedObject.FindProperty("AnimationPreset");
			animationHashSetProperty = serializedObject.FindProperty("animationHashSet");
			autoFootstepsProperty = serializedObject.FindProperty("autoFootsteps");
			autoLocomotionProperty = serializedObject.FindProperty("autoLocomotion");
			colliderHeadProperty = serializedObject.FindProperty("collider_head");
			colliderTorsoProperty = serializedObject.FindProperty("collider_torso");
			colliderFootRProperty = serializedObject.FindProperty("collider_footR");
			colliderFootLProperty = serializedObject.FindProperty("collider_footL");
			colliderHandRProperty = serializedObject.FindProperty("collider_handR");
			colliderHandLProperty = serializedObject.FindProperty("collider_handL");
			colliderFingerIndexLProperty = serializedObject.FindProperty("collider_fingerIndexL");
			colliderFingerMiddleLProperty = serializedObject.FindProperty("collider_fingerMiddleL");
			colliderFingerRingLProperty = serializedObject.FindProperty("collider_fingerRingL");
			colliderFingerLittleLProperty = serializedObject.FindProperty("collider_fingerLittleL");
			colliderFingerIndexRProperty = serializedObject.FindProperty("collider_fingerIndexR");
			colliderFingerMiddleRProperty = serializedObject.FindProperty("collider_fingerMiddleR");
			colliderFingerRingRProperty = serializedObject.FindProperty("collider_fingerRingR");
			colliderFingerLittleRProperty = serializedObject.FindProperty("collider_fingerLittleR");
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
			Type type = EditorUtils.FindType(text);
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
			Type type2 = EditorUtils.FindType(spec);
			typeCache.Add(spec, type2);
			return type2;
		}
		return null;
	}
}
