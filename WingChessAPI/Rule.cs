namespace WingChessAPI;

public record Rule(string Name, string ConditionName, string StageName, string ResultName)
{
	public void Compile()
	{
		throw new NotImplementedException();
		// $$$ implement this later
	}
}