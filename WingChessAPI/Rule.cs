namespace WingChessAPI;

using Newtonsoft.Json.Linq;

public record Rule(string Name, string MethodName, bool AllowsRecursion)
{
	public delegate string RuleDelegate(Board board);
	public RuleDelegate Method { get; private set; } = _ => throw new NotImplementedException();

	public static string Illegal => "Illegal";
	public static string Ongoing => "Ongoing";
	public static string Win => "Win";
	public static string Draw => "Draw";

	public Rule(KeyValuePair<string, JToken?> kvp) : this
	(
		kvp.Key,
		(string?)kvp.Value?["method"] ?? throw new($"Compilation failed on rule {kvp.Key}. Rule must provide method."),
		(bool?)kvp.Value?["allow_recursion"] ?? true
	)
	{ }		

	public void Compile()
	{
		var methodFound = AssemblyHelper.AssertSingletonAndGetValue(AssemblyHelper.GetMethods(MethodName), Name, MethodName, "rule");
		Method = (RuleDelegate)Delegate.CreateDelegate(typeof(RuleDelegate), methodFound);
	}
}