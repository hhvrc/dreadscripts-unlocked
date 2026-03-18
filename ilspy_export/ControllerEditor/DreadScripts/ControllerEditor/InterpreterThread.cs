using System.Collections.Generic;
using System.Reflection;

namespace DreadScripts.ControllerEditor;

internal struct InterpreterThread
{
	internal MemberInfo[] m_WatcherThread;

	internal Dictionary<string, FieldInfo> candidateThread;

	internal Dictionary<string, PropertyInfo> productThread;

	internal Dictionary<string, List<MethodInfo>> m_ExpressionThread;
}
