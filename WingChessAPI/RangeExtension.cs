namespace WingChessAPI;

internal static class RangeExtension
{
	public static IEnumerator<int> GetEnumerator(this Range range)
	{
		var (start, end) = (range.Start.Value, range.End.Value);
		if (start < end)
		{
			for (var i = start; i < end; i++)
			{
				yield return i;
			}
		}
		else if (start > end)
		{
			for (var i = end + 1; i <= start; i++)
			{
				yield return i;
			}
		}
	}
}
