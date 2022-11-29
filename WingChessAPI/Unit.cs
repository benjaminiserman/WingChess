namespace WingChessAPI;

using System.ComponentModel;

public class Unit
{
	public string Name { get; set; }
	public Team Team { get; set; }
	public Unit(string name, Team team)
	{
		Name = name;
		Team = team;
	}

	public static Unit Empty => new(string.Empty, new(string.Empty));

	public override bool Equals(object? obj)
	{
		if (obj is not Unit unit)
		{
			return false;
		}

		if (Name == Empty.Name && Team == Empty.Team && unit.Name == Empty.Name && unit.Team == Empty.Team)
		{
			return true;
		}
		else
		{
			return ReferenceEquals(obj, this);
		}
	}

	public override int GetHashCode() => base.GetHashCode();

	public static bool operator ==(Unit a, Unit b) => a.Equals(b);
	public static bool operator !=(Unit a, Unit b) => !a.Equals(b);
}