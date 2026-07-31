using System.Collections.Generic;
using System.Reflection;

namespace DreadScripts.ADOverhaul;

internal struct ReflectionCache
{
	internal MemberInfo[] members;

	internal Dictionary<string, FieldInfo> fields;

	internal Dictionary<string, PropertyInfo> properties;

	internal Dictionary<string, List<MethodInfo>> methods;
}
