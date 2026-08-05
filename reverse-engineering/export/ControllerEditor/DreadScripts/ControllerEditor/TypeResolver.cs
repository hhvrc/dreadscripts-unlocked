using System;
using System.Runtime.CompilerServices;

namespace DreadScripts.ControllerEditor;

internal class TypeResolver
{
	public readonly string typeName;

	public readonly bool searchAllAssemblies;

	public bool resolved;

	private Type cachedType;

	[SpecialName]
	public Type ResolvedType()
	{
		if (!resolved)
		{
			while (true)
			{
				resolved = true;
			}
		}
		return cachedType;
	}

	public static implicit operator Type(TypeResolver task)
	{
		return task.ResolvedType();
	}

	public TypeResolver(string spec, bool issecond = false)
	{
		typeName = spec;
		searchAllAssemblies = issecond;
	}

	public TypeResolver(Type value)
	{
		cachedType = value;
		typeName = value.FullName;
		resolved = true;
	}
}
