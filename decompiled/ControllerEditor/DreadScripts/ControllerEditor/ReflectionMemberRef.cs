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
		return this.processorPolicy[0];
	}

	[SpecialName]
	public T[] GetMembers()
	{
		if (!this._ServerPolicy)
		{
			this._ServerPolicy = true;
			Type typeFromHandle = typeof(T);
			MemberTypes type = ((typeFromHandle == typeof(FieldInfo)) ? MemberTypes.Field : ((typeFromHandle == typeof(PropertyInfo)) ? MemberTypes.Property : MemberTypes.Method));
			this._ThreadPolicy = (T[])this._TestsPolicy.ResolvedType().GetMember(this.m_RegPolicy, type, this.propertyPolicy);
		}
		return this._ThreadPolicy;
	}

	[SpecialName]
	public T GetMember()
	{
		if (!this.m_PolicyPolicy)
		{
			this.m_PolicyPolicy = true;
			if (this.StopRecord().Length == 0)
			{
				return null;
			}
			if (this.StopRecord().Length != 1 && !(this.WriteRecord() == null))
			{
				foreach (MethodInfo item in this.StopRecord().Cast<MethodInfo>())
				{
					ParameterInfo[] parameters = item.GetParameters();
					if ((!this.observerPolicy && parameters.Any((ParameterInfo asset) => asset.ParameterType == this.WriteRecord())) || (this.observerPolicy && parameters.Select((ParameterInfo p) => p.ParameterType).SequenceEqual(this.processorPolicy)))
					{
						this.m_SerializerPolicy = (T)(MemberInfo)item;
						break;
					}
				}
				return this.m_SerializerPolicy;
			}
			return this.m_SerializerPolicy = this.StopRecord()[0];
		}
		return this.m_SerializerPolicy;
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
		this.m_RegPolicy = reg;
		this._TestsPolicy = setup;
		this.propertyPolicy = spec2;
		this.processorPolicy = rule;
		this.observerPolicy = iskey3;
	}

	[CompilerGenerated]
	private bool FillRecord(ParameterInfo asset)
	{
		return asset.ParameterType == this.WriteRecord();
	}

	internal static bool ReflectDecorator()
	{
		return StopDecorator == null;
	}
}
