using System.Runtime.CompilerServices;

namespace DreadScripts.ControllerEditor;

internal sealed class SingletonServer
{
	[CompilerGenerated]
	private readonly int factoryServer;

	[CompilerGenerated]
	private readonly int m_AccountServer;

	[CompilerGenerated]
	private readonly int m_RefServer;

	private static SingletonServer TestSystem;

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

	internal SingletonServer(int previous_first, int col_Position, int maxtag)
	{
		factoryServer = previous_first;
		m_AccountServer = col_Position;
		m_RefServer = maxtag;
	}

	internal SingletonServer(string res)
	{
		string[] array = res.Split(new char[1] { '.' });
		factoryServer = int.Parse(array[0]);
		m_AccountServer = int.Parse(array[1]);
		m_RefServer = int.Parse(array[2]);
	}

	public static bool operator >(SingletonServer init, SingletonServer ord)
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

	public static bool operator <(SingletonServer task, SingletonServer vis)
	{
		return vis > task;
	}

	public static bool operator >=(SingletonServer item, SingletonServer connection)
	{
		return !(item < connection);
	}

	public static bool operator <=(SingletonServer last, SingletonServer ord)
	{
		return !(last > ord);
	}

	public static bool operator ==(SingletonServer res, SingletonServer col)
	{
		if (res.TestConnection() != col.TestConnection() || res.ValidateConnection() != col.ValidateConnection())
		{
			return false;
		}
		return res.RateConnection() == col.RateConnection();
	}

	public static bool operator !=(SingletonServer config, SingletonServer ivk)
	{
		return !(config == ivk);
	}

	private bool CalculateConnection(SingletonServer asset)
	{
		return this == asset;
	}

	public override bool Equals(object param)
	{
		if (this == param)
		{
			return true;
		}
		if (!(param is SingletonServer singletonServer))
		{
			return false;
		}
		return this == singletonServer;
	}

	public override int GetHashCode()
	{
		return (((TestConnection() * 397) ^ ValidateConnection()) * 397) ^ RateConnection();
	}

	public override string ToString()
	{
		return $"{TestConnection()}.{ValidateConnection()}.{RateConnection()}";
	}

	internal static bool IncludeSystem()
	{
		return (object)TestSystem == null;
	}
}
