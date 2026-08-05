using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DreadScripts.ControllerEditor;

internal class ReflectionMemberRef<T> where T : MemberInfo
{
	public readonly string memberName;

	public readonly TypeResolver typeResolver;

	public readonly BindingFlags bindingFlags;

	public readonly Type[] parameterTypes;

	private readonly bool matchExactSignature;

	public bool membersResolved;

	private T[] cachedMembers;

	private bool memberResolved;

	private T cachedMember;

	private static object StopDecorator;

	[SpecialName]
	public Type GetFirstParameterType()
	{
		return parameterTypes[0];
	}

	[SpecialName]
	public T[] GetMembers()
	{
		if (!membersResolved)
		{
			membersResolved = true;
			Type typeFromHandle = typeof(T);
			MemberTypes type = ((typeFromHandle == typeof(FieldInfo)) ? MemberTypes.Field : ((typeFromHandle == typeof(PropertyInfo)) ? MemberTypes.Property : MemberTypes.Method));
			cachedMembers = (T[])typeResolver.ResolvedType().GetMember(memberName, type, bindingFlags);
		}
		return cachedMembers;
	}

	[SpecialName]
	public T GetMember()
	{
		if (!memberResolved)
		{
			memberResolved = true;
			if (GetMembers().Length == 0)
			{
				return null;
			}
			if (GetMembers().Length != 1 && !(GetFirstParameterType() == null))
			{
				foreach (MethodInfo item in GetMembers().Cast<MethodInfo>())
				{
					ParameterInfo[] parameters = item.GetParameters();
					if ((!matchExactSignature && parameters.Any((ParameterInfo asset) => asset.ParameterType == GetFirstParameterType())) || (matchExactSignature && parameters.Select((ParameterInfo p) => p.ParameterType).SequenceEqual(parameterTypes)))
					{
						cachedMember = (T)(MemberInfo)item;
						break;
					}
				}
				return cachedMember;
			}
			return cachedMember = GetMembers()[0];
		}
		return cachedMember;
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
		memberName = reg;
		typeResolver = setup;
		bindingFlags = spec2;
		parameterTypes = rule;
		matchExactSignature = iskey3;
	}

	[CompilerGenerated]
	private bool FillRecord(ParameterInfo asset)
	{
		return asset.ParameterType == GetFirstParameterType();
	}

	internal static bool ReflectDecorator()
	{
		return StopDecorator == null;
	}
}
