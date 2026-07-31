using System.Runtime.CompilerServices;

namespace DreadScripts.ControllerEditor;

internal sealed class VersionNumber
{
	[CompilerGenerated]
	private readonly int factoryServer;

	[CompilerGenerated]
	private readonly int m_AccountServer;

	[CompilerGenerated]
	private readonly int m_RefServer;

	[SpecialName]
	[CompilerGenerated]
	internal int TestConnection()
	{
		return factoryServer;
	}

	[SpecialName]
	[CompilerGenerated]
	internal int ValidateConnection()
	{
		return m_AccountServer;
	}

	[SpecialName]
	[CompilerGenerated]
	internal int RateConnection()
	{
		return m_RefServer;
	}

	internal VersionNumber(int previous_first, int col_Position, int maxtag)
	{
		factoryServer = previous_first;
		m_AccountServer = col_Position;
		m_RefServer = maxtag;
	}

	internal VersionNumber(string res)
	{
		string[] array = res.Split(new char[1] { '.' });
		factoryServer = int.Parse(array[0]);
		m_AccountServer = int.Parse(array[1]);
		m_RefServer = int.Parse(array[2]);
	}

	public static bool operator >(VersionNumber init, VersionNumber ord)
	{
		if (init.TestConnection() > ord.TestConnection())
		{
			return true;
		}
		if (init.TestConnection() >= ord.TestConnection())
		{
			if (init.ValidateConnection() > ord.ValidateConnection())
			{
				return true;
			}
			if (init.ValidateConnection() < ord.ValidateConnection())
			{
				return false;
			}
			return init.RateConnection() > ord.RateConnection();
		}
		return false;
	}

	public static bool operator <(VersionNumber task, VersionNumber vis)
	{
		return vis > task;
	}

	public static bool operator >=(VersionNumber item, VersionNumber connection)
	{
		return !(item < connection);
	}

	public static bool operator <=(VersionNumber last, VersionNumber ord)
	{
		return !(last > ord);
	}

	public static bool operator ==(VersionNumber res, VersionNumber col)
	{
		if (res.TestConnection() != col.TestConnection() || res.ValidateConnection() != col.ValidateConnection())
		{
			return false;
		}
		return res.RateConnection() == col.RateConnection();
	}

	public static bool operator !=(VersionNumber config, VersionNumber ivk)
	{
		return !(config == ivk);
	}

	private bool CalculateConnection(VersionNumber asset)
	{
		return this == asset;
	}

	public override bool Equals(object param)
	{
		if (this == param)
		{
			return true;
		}
		if (!(param is VersionNumber versionNumber))
		{
			return false;
		}
		return this == versionNumber;
	}

	public override int GetHashCode()
	{
		return (((TestConnection() * 397) ^ ValidateConnection()) * 397) ^ RateConnection();
	}

	public override string ToString()
	{
		return $"{TestConnection()}.{ValidateConnection()}.{RateConnection()}";
	}
}
