using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DreadScripts.ControllerEditor;

[DefaultMember("Item")]
internal struct ImporterPolicy
{
	internal readonly GameObject requestPolicy;

	internal Component[] m_PrinterPolicy;

	private static object PushDecorator;

	[SpecialName]
	internal Transform ReadHelper()
	{
		return requestPolicy.transform;
	}

	[SpecialName]
	internal Component[] RemoveHelper()
	{
		return m_PrinterPolicy ?? (m_PrinterPolicy = requestPolicy.GetComponents<Component>());
	}

	internal ImporterPolicy(GameObject reference)
	{
		m_PrinterPolicy = null;
		requestPolicy = reference;
	}

	internal ImporterPolicy(Component setup)
	{
		m_PrinterPolicy = null;
		requestPolicy = setup.gameObject;
	}

	internal ImporterPolicy(Transform config)
	{
		m_PrinterPolicy = null;
		requestPolicy = config.gameObject;
	}

	public T StartHelper<T>() where T : Component
	{
		return requestPolicy.GetComponent<T>();
	}

	[SpecialName]
	public Component AwakeHelper(int version_init)
	{
		return RemoveHelper()[version_init];
	}

	public static implicit operator GameObject(ImporterPolicy def)
	{
		return def.requestPolicy;
	}

	internal static bool PrepareDecorator()
	{
		return PushDecorator == null;
	}
}
