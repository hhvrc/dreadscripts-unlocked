using System.Runtime.CompilerServices;

namespace DreadScripts.ADOverhaul;

internal sealed class SemVer
{
	[CompilerGenerated]
	private readonly int m_ConsumerSerializer;

	[CompilerGenerated]
	private readonly int utilsSerializer;

	[CompilerGenerated]
	private readonly int _PageSerializer;

	private static SemVer PatchOrder;

	[SpecialName]
	[CompilerGenerated]
	internal int PrintProcess()
	{
		return m_ConsumerSerializer;
	}

	[SpecialName]
	[CompilerGenerated]
	internal int ViewProcess()
	{
		return utilsSerializer;
	}

	[SpecialName]
	[CompilerGenerated]
	internal int ListProcess()
	{
		return _PageSerializer;
	}

	internal SemVer(int item, int startcounter, int indexOfrole)
	{
		m_ConsumerSerializer = item;
		utilsSerializer = startcounter;
		_PageSerializer = indexOfrole;
	}

	internal SemVer(string spec)
	{
		string[] array = spec.Split(new char[1] { '.' });
		m_ConsumerSerializer = int.Parse(array[0]);
		utilsSerializer = int.Parse(array[1]);
		_PageSerializer = int.Parse(array[2]);
	}

	public static bool operator >(SemVer first, SemVer visitor)
	{
		if (first.PrintProcess() <= visitor.PrintProcess())
		{
			if (first.PrintProcess() >= visitor.PrintProcess())
			{
				if (first.ViewProcess() <= visitor.ViewProcess())
				{
					if (first.ViewProcess() < visitor.ViewProcess())
					{
						return false;
					}
					return first.ListProcess() > visitor.ListProcess();
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
		if (ident.PrintProcess() != second.PrintProcess() || ident.ViewProcess() != second.ViewProcess())
		{
			return false;
		}
		return ident.ListProcess() == second.ListProcess();
	}

	public static bool operator !=(SemVer value, SemVer attr)
	{
		return !(value == attr);
	}

	private bool MoveProcess(SemVer instance)
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
		return (((PrintProcess() * 397) ^ ViewProcess()) * 397) ^ ListProcess();
	}

	public override string ToString()
	{
		return $"{PrintProcess()}.{ViewProcess()}.{ListProcess()}";
	}

	internal static bool RemoveOrder()
	{
		return (object)PatchOrder == null;
	}
}
