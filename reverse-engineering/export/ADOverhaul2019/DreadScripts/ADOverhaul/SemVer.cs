using System.Runtime.CompilerServices;

namespace DreadScripts.ADOverhaul;

internal sealed class SemVer
{
	[CompilerGenerated]
	private readonly int major;

	[CompilerGenerated]
	private readonly int minor;

	[CompilerGenerated]
	private readonly int patch;

	[SpecialName]
	[CompilerGenerated]
	internal int Major()
	{
		return major;
	}

	[SpecialName]
	[CompilerGenerated]
	internal int Minor()
	{
		return minor;
	}

	[SpecialName]
	[CompilerGenerated]
	internal int Patch()
	{
		return patch;
	}

	internal SemVer(int task_high, int max_visitor, int dir_size)
	{
		major = task_high;
		minor = max_visitor;
		patch = dir_size;
	}

	internal SemVer(string ident)
	{
		string[] array = ident.Split(new char[1] { '.' });
		major = int.Parse(array[0]);
		minor = int.Parse(array[1]);
		patch = int.Parse(array[2]);
	}

	public static bool operator >(SemVer config, SemVer counter)
	{
		if (config.Major() <= counter.Major())
		{
			if (config.Major() >= counter.Major())
			{
				if (config.Minor() > counter.Minor())
				{
					return true;
				}
				if (config.Minor() < counter.Minor())
				{
					return false;
				}
				return config.Patch() > counter.Patch();
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
		if (spec.Major() != result.Major() || spec.Minor() != result.Minor())
		{
			return false;
		}
		return spec.Patch() == result.Patch();
	}

	public static bool operator !=(SemVer value, SemVer ivk)
	{
		return !(value == ivk);
	}

	private bool Equals(SemVer param)
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
		return (((Major() * 397) ^ Minor()) * 397) ^ Patch();
	}

	public override string ToString()
	{
		return $"{Major()}.{Minor()}.{Patch()}";
	}
}
