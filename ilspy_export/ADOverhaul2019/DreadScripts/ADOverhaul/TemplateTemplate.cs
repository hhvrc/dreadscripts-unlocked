namespace DreadScripts.ADOverhaul;

internal sealed class TemplateTemplate
{
	private bool[] m_DicTemplate;

	internal int serviceTemplate = -1;

	internal static TemplateTemplate PushFactory;

	internal TemplateTemplate(int max_def)
	{
		AssetAccount(max_def);
	}

	internal void AssetAccount(int taskmax)
	{
		if (m_DicTemplate == null || m_DicTemplate.Length != taskmax)
		{
			m_DicTemplate = new bool[taskmax];
		}
		if (serviceTemplate > 0)
		{
			if (serviceTemplate < m_DicTemplate.Length)
			{
				m_DicTemplate[serviceTemplate] = true;
			}
			else
			{
				serviceTemplate = -1;
			}
		}
	}

	internal void TestAccount(int idx_param)
	{
		if (idx_param >= 0 && idx_param < m_DicTemplate.Length && serviceTemplate != idx_param)
		{
			if (serviceTemplate >= 0)
			{
				m_DicTemplate[serviceTemplate] = false;
			}
			serviceTemplate = idx_param;
			m_DicTemplate[serviceTemplate] = true;
		}
	}

	internal void ResetAccount(int first_count, bool overridesecond)
	{
		if (first_count < 0 || first_count >= m_DicTemplate.Length)
		{
			return;
		}
		if (serviceTemplate == first_count)
		{
			if (overridesecond)
			{
				return;
			}
			GetAccount();
		}
		if (serviceTemplate >= 0 && overridesecond)
		{
			m_DicTemplate[serviceTemplate] = false;
		}
		if (overridesecond)
		{
			serviceTemplate = first_count;
		}
		m_DicTemplate[first_count] = overridesecond;
	}

	internal void GetAccount()
	{
		if (serviceTemplate >= 0)
		{
			m_DicTemplate[serviceTemplate] = false;
			serviceTemplate = -1;
		}
	}

	internal static bool InvokeFactory()
	{
		return PushFactory == null;
	}
}
