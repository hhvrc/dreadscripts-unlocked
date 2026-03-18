namespace DreadScripts.ADOverhaul;

internal sealed class TokenParamsDispatcher
{
	private bool[] _PropertySerializer;

	internal int singletonSerializer = -1;

	internal static TokenParamsDispatcher ReadOrder;

	internal TokenParamsDispatcher(int indexOf_def)
	{
		UpdateProcess(indexOf_def);
	}

	internal void UpdateProcess(int columninstance)
	{
		if (_PropertySerializer == null || _PropertySerializer.Length != columninstance)
		{
			_PropertySerializer = new bool[columninstance];
		}
		if (singletonSerializer > 0)
		{
			if (singletonSerializer >= _PropertySerializer.Length)
			{
				singletonSerializer = -1;
			}
			else
			{
				_PropertySerializer[singletonSerializer] = true;
			}
		}
	}

	internal void SearchProcess(int ID_i)
	{
		if (ID_i >= 0 && ID_i < _PropertySerializer.Length && singletonSerializer != ID_i)
		{
			if (singletonSerializer >= 0)
			{
				_PropertySerializer[singletonSerializer] = false;
			}
			singletonSerializer = ID_i;
			_PropertySerializer[singletonSerializer] = true;
		}
	}

	internal void LoginProcess(int sumspec, bool ispred)
	{
		if (sumspec < 0 || sumspec >= _PropertySerializer.Length)
		{
			return;
		}
		if (singletonSerializer == sumspec)
		{
			if (ispred)
			{
				return;
			}
			PatchProcess();
		}
		if (singletonSerializer >= 0 && ispred)
		{
			_PropertySerializer[singletonSerializer] = false;
		}
		if (ispred)
		{
			singletonSerializer = sumspec;
		}
		_PropertySerializer[sumspec] = ispred;
	}

	internal void PatchProcess()
	{
		if (singletonSerializer >= 0)
		{
			_PropertySerializer[singletonSerializer] = false;
			singletonSerializer = -1;
		}
	}

	internal static bool LoginOrder()
	{
		return ReadOrder == null;
	}
}
