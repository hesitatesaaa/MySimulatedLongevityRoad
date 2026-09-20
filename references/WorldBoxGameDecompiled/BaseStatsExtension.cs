public static class BaseStatsExtension
{
	public static bool isEmpty(this BaseStats pBaseStats)
	{
		if (pBaseStats == null)
		{
			return true;
		}
		if (!pBaseStats.hasTags())
		{
			return !pBaseStats.hasStats();
		}
		return false;
	}
}
