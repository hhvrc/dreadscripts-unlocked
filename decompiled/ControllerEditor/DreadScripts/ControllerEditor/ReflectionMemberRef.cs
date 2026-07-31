using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DreadScripts.ControllerEditor;

internal class ReflectionMemberRef<T> where T : MemberInfo
{
	public readonly string m_RegPolicy;

	public readonly TypeResolver _TestsPolicy;

	public readonly BindingFlags propertyPolicy;

	public readonly Type[] processorPolicy;

	private readonly bool observerPolicy;

	public bool _ServerPolicy;

	private T[] _ThreadPolicy;

	private bool m_PolicyPolicy;

	private T m_SerializerPolicy;

	private static object StopDecorator;

	[SpecialName]
	public Type WriteRecord()
	{
		return processorPolicy[0];
	}

	[SpecialName]
	public T[] StopRecord()
	{
		if (!_ServerPolicy)
		{
			_ServerPolicy = true;
			Type typeFromHandle = typeof(T);
			MemberTypes type = ((typeFromHandle == typeof(FieldInfo)) ? MemberTypes.Field : ((typeFromHandle == typeof(PropertyInfo)) ? MemberTypes.Property : MemberTypes.Method));
			_ThreadPolicy = (T[])_TestsPolicy.ResolvedType().GetMember(m_RegPolicy, type, propertyPolicy);
		}
		return _ThreadPolicy;
	}

	[SpecialName]
	public T PrepareRecord()
	{
		if (!m_PolicyPolicy)
		{
			m_PolicyPolicy = true;
			if (StopRecord().Length == 0)
			{
				return null;
			}
			if (StopRecord().Length != 1 && !(WriteRecord() == null))
			{
				foreach (MethodInfo item in StopRecord().Cast<MethodInfo>())
				{
					ParameterInfo[] parameters = item.GetParameters();
					if ((!observerPolicy && parameters.Any((ParameterInfo asset) => asset.ParameterType == WriteRecord())) || (observerPolicy && parameters.Select((ParameterInfo p) => p.ParameterType).SequenceEqual(processorPolicy)))
					{
						m_SerializerPolicy = (T)(MemberInfo)item;
						break;
					}
				}
				return m_SerializerPolicy;
			}
			return m_SerializerPolicy = StopRecord()[0];
		}
		return m_SerializerPolicy;
	}

	public ReflectionMemberRef(TypeResolver res, string cust, Type state = null, BindingFlags t2 = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
		: this(res, cust, new Type[1] { state }, t2, iskey3: false)
	{
	}

	public ReflectionMemberRef(Type reference, string b, Type field = null, BindingFlags token2 = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
		: this(new TypeResolver(reference), b, new Type[1] { field }, token2, iskey3: false)
	{
	}

	public ReflectionMemberRef(TypeResolver config, string token, Type[] state, BindingFlags cont2)
		: this(config, token, state, cont2, iskey3: true)
	{
	}

	public ReflectionMemberRef(Type v, string vis, Type[] dic, BindingFlags item2)
		: this(new TypeResolver(v), vis, dic, item2, iskey3: true)
	{
	}

	public ReflectionMemberRef(TypeResolver setup, string reg, Type[] rule, BindingFlags spec2, bool iskey3)
	{
		m_RegPolicy = reg;
		_TestsPolicy = setup;
		propertyPolicy = spec2;
		processorPolicy = rule;
		observerPolicy = iskey3;
	}

	[CompilerGenerated]
	private bool FillRecord(ParameterInfo asset)
	{
		return asset.ParameterType == WriteRecord();
	}

	internal static bool ReflectDecorator()
	{
		return StopDecorator == null;
	}
}
