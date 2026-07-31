namespace DreadScripts.ADOverhaul;

internal sealed class ExclusiveSelectionState
{
	private bool[] toggles;

	internal int activeIndex = -1;

	internal ExclusiveSelectionState(int indexOf_def)
	{
		Resize(indexOf_def);
	}

	internal void Resize(int columninstance)
	{
		if (toggles == null || toggles.Length != columninstance)
		{
			toggles = new bool[columninstance];
		}
		if (activeIndex > 0)
		{
			if (activeIndex >= toggles.Length)
			{
				activeIndex = -1;
			}
			else
			{
				toggles[activeIndex] = true;
			}
		}
	}

	internal void Select(int ID_i)
	{
		if (ID_i >= 0 && ID_i < toggles.Length && activeIndex != ID_i)
		{
			if (activeIndex >= 0)
			{
				toggles[activeIndex] = false;
			}
			activeIndex = ID_i;
			toggles[activeIndex] = true;
		}
	}

	internal void SetSelected(int sumspec, bool ispred)
	{
		if (sumspec < 0 || sumspec >= toggles.Length)
		{
			return;
		}
		if (activeIndex == sumspec)
		{
			if (ispred)
			{
				return;
			}
			Clear();
		}
		if (activeIndex >= 0 && ispred)
		{
			toggles[activeIndex] = false;
		}
		if (ispred)
		{
			activeIndex = sumspec;
		}
		toggles[sumspec] = ispred;
	}

	internal void Clear()
	{
		if (activeIndex >= 0)
		{
			toggles[activeIndex] = false;
			activeIndex = -1;
		}
	}
}
