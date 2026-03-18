using System.Collections.Generic;
using System.Reflection;

namespace DreadScripts.ADOverhaul;

internal struct VisitorTokenizerResolver
{
	internal MemberInfo[] _WatcherDic;

	internal Dictionary<string, FieldInfo> _ContextDic;

	internal Dictionary<string, PropertyInfo> m_SchemaDic;

	internal Dictionary<string, List<MethodInfo>> m_ReponseDic;
}
