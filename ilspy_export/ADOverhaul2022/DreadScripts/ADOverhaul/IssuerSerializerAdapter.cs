using System.Runtime.CompilerServices;

namespace DreadScripts.ADOverhaul;

internal sealed class IssuerSerializerAdapter
{
	[CompilerGenerated]
	private readonly int m_ConsumerSerializer;

	[CompilerGenerated]
	private readonly int utilsSerializer;

	[CompilerGenerated]
	private readonly int _PageSerializer;

	private static IssuerSerializerAdapter PatchOrder;

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

	internal IssuerSerializerAdapter(int item, int startcounter, int indexOfrole)
	{
		m_ConsumerSerializer = item;
		utilsSerializer = startcounter;
		_PageSerializer = indexOfrole;
	}

	internal IssuerSerializerAdapter(string spec)
	{
		string[] array = spec.Split(new char[1] { '.' });
		m_ConsumerSerializer = int.Parse(array[0]);
		utilsSerializer = int.Parse(array[1]);
		_PageSerializer = int.Parse(array[2]);
	}

	public static bool operator >(IssuerSerializerAdapter first, IssuerSerializerAdapter visitor)
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

	public static bool operator <(IssuerSerializerAdapter spec, IssuerSerializerAdapter attr)
	{
		return attr > spec;
	}

	public static bool operator >=(IssuerSerializerAdapter item, IssuerSerializerAdapter caller)
	{
		return !(item < caller);
	}

	public static bool operator <=(IssuerSerializerAdapter last, IssuerSerializerAdapter reg)
	{
		return !(last > reg);
	}

	public static bool operator ==(IssuerSerializerAdapter ident, IssuerSerializerAdapter second)
	{
		if (ident.PrintProcess() != second.PrintProcess() || ident.ViewProcess() != second.ViewProcess())
		{
			return false;
		}
		return ident.ListProcess() == second.ListProcess();
	}

	public static bool operator !=(IssuerSerializerAdapter value, IssuerSerializerAdapter attr)
	{
		return !(value == attr);
	}

	private bool MoveProcess(IssuerSerializerAdapter instance)
	{
		return this == instance;
	}

	public override bool Equals(object init)
	{
		if (this == init)
		{
			return true;
		}
		if (!(init is IssuerSerializerAdapter issuerSerializerAdapter))
		{
			return false;
		}
		return this == issuerSerializerAdapter;
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
