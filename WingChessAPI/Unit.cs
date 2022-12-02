namespace WingChessAPI;

public struct Unit
{
	public string Name { get; set; }
	public Team Team { get; set; }
	public string Id { get; set; }

	public Unit(string name, Team team, string id)
	{
		if (id == Empty.Id)
		{
			throw new ArgumentException($"Unit Id cannot be set to {Empty.Id}.");
		}

		Name = name;
		Team = team;
		Id = id;
	}

	public static Unit Empty => default;

	public override bool Equals(object? obj)
	{
		if (obj is not Unit unit)
		{
			return false;
		}

		return Id == unit.Id;
	}

	public override int GetHashCode() => base.GetHashCode();

	public static bool operator ==(Unit a, Unit b) => a.Equals(b);
	public static bool operator !=(Unit a, Unit b) => !a.Equals(b);
}