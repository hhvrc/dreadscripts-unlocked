using System.Collections.Generic;
using System.Reflection;

namespace DreadScripts.ADOverhaul;

internal struct ContextSerializerConnector
{
	internal MemberInfo[] _BridgeMethod;

	internal Dictionary<string, FieldInfo> publisherMethod;

	internal Dictionary<string, PropertyInfo> m_MerchantMethod;

	internal Dictionary<string, List<MethodInfo>> _ProcMethod;
}
