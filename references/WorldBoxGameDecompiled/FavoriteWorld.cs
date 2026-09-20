public static class FavoriteWorld
{
	private const int NO_WORLD_SET = -1;

	private static int _cache_favorite_world_id = -1;

	public static void checkFavoriteWorld()
	{
		if (hasFavoriteWorldSet(pCheck: false) && !SaveManager.slotExists(PlayerConfig.instance.data.favorite_world))
		{
			clearFavoriteWorld();
		}
	}

	public static void clearFavoriteWorld()
	{
		PlayerConfig.instance.data.favorite_world = -1;
		PlayerConfig.saveData();
	}

	public static bool hasFavoriteWorldSet(bool pCheck = true)
	{
		if (pCheck)
		{
			checkFavoriteWorld();
		}
		return PlayerConfig.instance.data.favorite_world != -1;
	}

	public static void restoreCachedFavoriteWorldOnSuccess()
	{
		if (_cache_favorite_world_id != -1)
		{
			PlayerConfig.instance.data.favorite_world = _cache_favorite_world_id;
			PlayerConfig.saveData();
			_cache_favorite_world_id = -1;
		}
	}

	public static void cacheSaveSlotID(int pID)
	{
		_cache_favorite_world_id = pID;
	}
}
