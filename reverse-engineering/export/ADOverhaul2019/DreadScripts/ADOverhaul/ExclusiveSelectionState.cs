namespace DreadScripts.ADOverhaul;

internal sealed class ExclusiveSelectionState
{
	private bool[] toggles;

	internal int activeIndex = -1;

	internal ExclusiveSelectionState(int max_def)
	{
		Resize(max_def);
	}

	internal void Resize(int taskmax)
	{
		if (toggles == null || toggles.Length != taskmax)
		{
			toggles = new bool[taskmax];
		}
		if (activeIndex > 0)
		{
			if (activeIndex < toggles.Length)
			{
				toggles[activeIndex] = true;
			}
			else
			{
				activeIndex = -1;
			}
		}
	}

	internal void Select(int idx_param)
	{
		if (idx_param >= 0 && idx_param < toggles.Length && activeIndex != idx_param)
		{
			if (activeIndex >= 0)
			{
				toggles[activeIndex] = false;
			}
			activeIndex = idx_param;
			toggles[activeIndex] = true;
		}
	}

	internal void SetSelected(int first_count, bool overridesecond)
	{
		if (first_count < 0 || first_count >= toggles.Length)
		{
			return;
		}
		if (activeIndex == first_count)
		{
			if (overridesecond)
			{
				return;
			}
			Clear();
		}
		if (activeIndex >= 0 && overridesecond)
		{
			toggles[activeIndex] = false;
		}
		if (overridesecond)
		{
			activeIndex = first_count;
		}
		toggles[first_count] = overridesecond;
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
