using System.Runtime.CompilerServices;

namespace DreadScripts.ADOverhaul;

internal sealed class SemVer
{
	[CompilerGenerated]
	private readonly int structTemplate;

	[CompilerGenerated]
	private readonly int _ConfigTemplate;

	[CompilerGenerated]
	private readonly int modelTemplate;

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

	internal SemVer(int task_high, int max_visitor, int dir_size)
	{
		structTemplate = task_high;
		_ConfigTemplate = max_visitor;
		modelTemplate = dir_size;
	}

	internal SemVer(string ident)
	{
		string[] array = ident.Split(new char[1] { '.' });
		structTemplate = int.Parse(array[0]);
		_ConfigTemplate = int.Parse(array[1]);
		modelTemplate = int.Parse(array[2]);
	}

	public static bool operator >(SemVer config, SemVer counter)
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

	public static bool operator <(SemVer ident, SemVer connection)
	{
		return connection > ident;
	}

	public static bool operator >=(SemVer reference, SemVer ord)
	{
		return !(reference < ord);
	}

	public static bool operator <=(SemVer item, SemVer counter)
	{
		return !(item > counter);
	}

	public static bool operator ==(SemVer spec, SemVer result)
	{
		if (spec.CheckAccount() != result.CheckAccount() || spec.DisableAccount() != result.DisableAccount())
		{
			return false;
		}
		return spec.RateAccount() == result.RateAccount();
	}

	public static bool operator !=(SemVer value, SemVer ivk)
	{
		return !(value == ivk);
	}

	private bool ComputeAccount(SemVer param)
	{
		return this == param;
	}

	public override bool Equals(object i)
	{
		if (this == i)
		{
			return true;
		}
		if (!(i is SemVer semVer))
		{
			return false;
		}
		return this == semVer;
	}

	public override int GetHashCode()
	{
		return (((CheckAccount() * 397) ^ DisableAccount()) * 397) ^ RateAccount();
	}

	public override string ToString()
	{
		return $"{CheckAccount()}.{DisableAccount()}.{RateAccount()}";
	}
}
