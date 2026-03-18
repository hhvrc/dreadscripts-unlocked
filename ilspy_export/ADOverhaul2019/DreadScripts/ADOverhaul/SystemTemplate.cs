using System.Runtime.CompilerServices;

namespace DreadScripts.ADOverhaul;

internal sealed class SystemTemplate
{
	[CompilerGenerated]
	private readonly int structTemplate;

	[CompilerGenerated]
	private readonly int _ConfigTemplate;

	[CompilerGenerated]
	private readonly int modelTemplate;

	internal static SystemTemplate SetupFactory;

	[SpecialName]
	[CompilerGenerated]
	internal int CheckAccount()
	{
		return structTemplate;
	}

	[SpecialName]
	[CompilerGenerated]
	internal int DisableAccount()
	{
		return _ConfigTemplate;
	}

	[SpecialName]
	[CompilerGenerated]
	internal int RateAccount()
	{
		return modelTemplate;
	}

	internal SystemTemplate(int task_high, int max_visitor, int dir_size)
	{
		structTemplate = task_high;
		_ConfigTemplate = max_visitor;
		modelTemplate = dir_size;
	}

	internal SystemTemplate(string ident)
	{
		string[] array = ident.Split(new char[1] { '.' });
		structTemplate = int.Parse(array[0]);
		_ConfigTemplate = int.Parse(array[1]);
		modelTemplate = int.Parse(array[2]);
	}

	public static bool operator >(SystemTemplate config, SystemTemplate counter)
	{
		if (config.CheckAccount() <= counter.CheckAccount())
		{
			if (config.CheckAccount() >= counter.CheckAccount())
			{
				if (config.DisableAccount() > counter.DisableAccount())
				{
					return true;
				}
				if (config.DisableAccount() < counter.DisableAccount())
				{
					return false;
				}
				return config.RateAccount() > counter.RateAccount();
			}
			return false;
		}
		return true;
	}

	public static bool operator <(SystemTemplate ident, SystemTemplate connection)
	{
		return connection > ident;
	}

	public static bool operator >=(SystemTemplate reference, SystemTemplate ord)
	{
		return !(reference < ord);
	}

	public static bool operator <=(SystemTemplate item, SystemTemplate counter)
	{
		return !(item > counter);
	}

	public static bool operator ==(SystemTemplate spec, SystemTemplate result)
	{
		if (spec.CheckAccount() != result.CheckAccount() || spec.DisableAccount() != result.DisableAccount())
		{
			return false;
		}
		return spec.RateAccount() == result.RateAccount();
	}

	public static bool operator !=(SystemTemplate value, SystemTemplate ivk)
	{
		return !(value == ivk);
	}

	private bool ComputeAccount(SystemTemplate param)
	{
		return this == param;
	}

	public override bool Equals(object i)
	{
		if (this == i)
		{
			return true;
		}
		if (!(i is SystemTemplate systemTemplate))
		{
			return false;
		}
		return this == systemTemplate;
	}

	public override int GetHashCode()
	{
		return (((CheckAccount() * 397) ^ DisableAccount()) * 397) ^ RateAccount();
	}

	public override string ToString()
	{
		return $"{CheckAccount()}.{DisableAccount()}.{RateAccount()}";
	}

	internal static bool QueryFactory()
	{
		return (object)SetupFactory == null;
	}
}
