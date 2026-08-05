using System.Runtime.CompilerServices;

namespace DreadScripts.ControllerEditor;

internal sealed class VersionNumber
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

	internal VersionNumber(int previous_first, int col_Position, int maxtag)
	{
		major = previous_first;
		minor = col_Position;
		patch = maxtag;
	}

	internal VersionNumber(string res)
	{
		string[] array = res.Split(new char[1] { '.' });
		major = int.Parse(array[0]);
		minor = int.Parse(array[1]);
		patch = int.Parse(array[2]);
	}

	public static bool operator >(VersionNumber init, VersionNumber ord)
	{
		if (init.Major() > ord.Major())
		{
			return true;
		}
		if (init.Major() >= ord.Major())
		{
			if (init.Minor() > ord.Minor())
			{
				return true;
			}
			if (init.Minor() < ord.Minor())
			{
				return false;
			}
			return init.Patch() > ord.Patch();
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
		if (res.Major() != col.Major() || res.Minor() != col.Minor())
		{
			return false;
		}
		return res.Patch() == col.Patch();
	}

	public static bool operator !=(VersionNumber config, VersionNumber ivk)
	{
		return !(config == ivk);
	}

	private bool Equals(VersionNumber asset)
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
		return (((Major() * 397) ^ Minor()) * 397) ^ Patch();
	}

	public override string ToString()
	{
		return $"{Major()}.{Minor()}.{Patch()}";
	}
}
