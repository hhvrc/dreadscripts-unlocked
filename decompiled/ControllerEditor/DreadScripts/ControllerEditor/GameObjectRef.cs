using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

[DefaultMember("Item")]
internal struct GameObjectRef
{
	internal readonly GameObject gameObject;

	internal Component[] cachedComponents;

	private static object PushDecorator;

	[SpecialName]
	internal Transform Transform()
	{
		return gameObject.transform;
	}

	[SpecialName]
	internal Component[] Components()
	{
		return cachedComponents ?? (cachedComponents = gameObject.GetComponents<Component>());
	}

	internal GameObjectRef(GameObject reference)
	{
		cachedComponents = null;
		gameObject = reference;
	}

	internal GameObjectRef(Component setup)
	{
		cachedComponents = null;
		gameObject = setup.gameObject;
	}

	internal GameObjectRef(Transform config)
	{
		cachedComponents = null;
		gameObject = config.gameObject;
	}

	public T GetComponent<T>() where T : Component
	{
		return gameObject.GetComponent<T>();
	}

	[SpecialName]
	public Component Item(int version_init)
	{
		return Components()[version_init];
	}

	public static implicit operator GameObject(GameObjectRef def)
	{
		return def.gameObject;
	}

	internal static bool PrepareDecorator()
	{
		return PushDecorator == null;
	}
}
