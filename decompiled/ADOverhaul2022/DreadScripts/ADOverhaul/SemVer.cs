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

	internal SemVer(int item, int startcounter, int indexOfrole)
	{
		major = item;
		minor = startcounter;
		patch = indexOfrole;
	}

	internal SemVer(string spec)
	{
		string[] array = spec.Split(new char[1] { '.' });
		major = int.Parse(array[0]);
		minor = int.Parse(array[1]);
		patch = int.Parse(array[2]);
	}

	public static bool operator >(SemVer first, SemVer visitor)
	{
		if (first.Major() <= visitor.Major())
		{
			if (first.Major() >= visitor.Major())
			{
				if (first.Minor() <= visitor.Minor())
				{
					if (first.Minor() < visitor.Minor())
					{
						return false;
					}
					return first.Patch() > visitor.Patch();
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool operator <(SemVer spec, SemVer attr)
	{
		return attr > spec;
	}

	public static bool operator >=(SemVer item, SemVer caller)
	{
		return !(item < caller);
	}

	public static bool operator <=(SemVer last, SemVer reg)
	{
		return !(last > reg);
	}

	public static bool operator ==(SemVer ident, SemVer second)
	{
		if (ident.Major() != second.Major() || ident.Minor() != second.Minor())
		{
			return false;
		}
		return ident.Patch() == second.Patch();
	}

	public static bool operator !=(SemVer value, SemVer attr)
	{
		return !(value == attr);
	}

	private bool Equals(SemVer instance)
	{
		return this == instance;
	}

	public override bool Equals(object init)
	{
		if (this == init)
		{
			return true;
		}
		if (!(init is SemVer semVer))
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
