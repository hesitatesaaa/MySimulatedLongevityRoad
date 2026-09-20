public static class InitAds
{
	private static bool initiated;

	public static void initAdProviders()
	{
		if (!Config.adsInitialized && !initiated)
		{
			initiated = true;
		}
	}
}
