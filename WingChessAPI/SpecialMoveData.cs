namespace WingChessAPI;

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

public record struct SpecialMoveData(string? Method, string? Shortform, List<string> Tags)
{
	public SpecialMoveData(JToken token) : this
	(
		(string?)token["method"],
		(string?)token["shortform"],
		((JArray?)token["tags"])?.Select(s => (string)s!).ToList() ?? new()
	)
	{ }
}
