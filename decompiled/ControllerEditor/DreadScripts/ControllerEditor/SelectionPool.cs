using System;

namespace DreadScripts.ControllerEditor;

internal sealed class SelectionPool
{
	private bool[] selections;

	internal int selectedIndex = -1;

	internal Action onSelectionChanged;

	internal SelectionPool(int setup, Action cfg = null)
	{
		Resize(setup);
		onSelectionChanged = cfg;
	}

	internal void Resize(int ident_Ptr)
	{
		if (selections == null || selections.Length != ident_Ptr)
		{
			selections = new bool[ident_Ptr];
		}
		if (selectedIndex > 0)
		{
			if (selectedIndex < selections.Length)
			{
				selections[selectedIndex] = true;
			}
			else
			{
				selectedIndex = -1;
			}
		}
	}

	internal void Select(int max_setup)
	{
		if (max_setup >= 0 && max_setup < selections.Length && selectedIndex != max_setup)
		{
			if (selectedIndex >= 0)
			{
				selections[selectedIndex] = false;
			}
			selectedIndex = max_setup;
			onSelectionChanged?.Invoke();
			selections[selectedIndex] = true;
		}
	}

	internal void SetSelected(int previousvar1, bool addmap)
	{
		if (previousvar1 < 0 || previousvar1 >= selections.Length)
		{
			return;
		}
		if (selectedIndex == previousvar1)
		{
			if (addmap)
			{
				return;
			}
			ClearSelection();
		}
		if (selectedIndex >= 0 && addmap)
		{
			selections[selectedIndex] = false;
		}
		if (addmap)
		{
			selectedIndex = previousvar1;
			onSelectionChanged?.Invoke();
		}
		selections[previousvar1] = addmap;
	}

	internal void ClearSelection()
	{
		if (selectedIndex >= 0)
		{
			selections[selectedIndex] = false;
			selectedIndex = -1;
			onSelectionChanged?.Invoke();
		}
	}
}
