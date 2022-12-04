namespace WingChessAPI;

using System.Reflection;

internal static class AssemblyHelper
{
	public static List<MethodInfo> GetMethods(string methodString)
	{
		var split = methodString.Split("::");
		var packageName = split[0];
		var methodName = split[1];

		return Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(t => t.Namespace == packageName)
			.Where(t => t
				.GetMethods()
				.Any(m => m.Name == methodName))
			.Select(t => t
				.GetMethods()
				.First(m => m.Name == methodName))
			.ToList();
	}

	public static MethodInfo AssertSingletonAndGetValue(List<MethodInfo> methods, string name, string pattern, string typeName)
	{
		if (methods.Count == 0)
		{
			throw new($"Compilation failed on {typeName} {name}. No method could be found that matches {pattern}.");
		}
		else if (methods.Count > 1)
		{
			throw new($"Compilation failed on {typeName} {name}. Pattern {pattern} is ambiguous between {{ {string.Join(", ", methods.Select(m => m?.DeclaringType?.Name + "::" + m?.Name))} }}.");
		}
		else
		{
			return methods.First();
		}
	}
}
