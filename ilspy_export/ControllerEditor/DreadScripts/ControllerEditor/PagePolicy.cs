using System;
using System.Runtime.CompilerServices;

namespace DreadScripts.ControllerEditor;

internal class PagePolicy
{
	public readonly string resolverPolicy;

	public readonly bool predicatePolicy;

	public bool m_RulesPolicy;

	private Type queuePolicy;

	private static PagePolicy DefineDecorator;

	[SpecialName]
	public Type ChangeRecord()
	{
		if (!m_RulesPolicy)
		{
			while (true)
			{
				m_RulesPolicy = true;
			}
		}
		return queuePolicy;
	}

	public static implicit operator Type(PagePolicy task)
	{
		return task.ChangeRecord();
	}

	public PagePolicy(string spec, bool issecond = false)
	{
		resolverPolicy = spec;
		predicatePolicy = issecond;
	}

	public PagePolicy(Type value)
	{
		queuePolicy = value;
		resolverPolicy = value.FullName;
		m_RulesPolicy = true;
	}

	internal static bool EnableDecorator()
	{
		return DefineDecorator == null;
	}
}
