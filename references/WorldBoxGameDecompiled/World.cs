using System.Runtime.CompilerServices;

public static class World
{
	public static MapBox world
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return MapBox.instance;
		}
	}

	public static WorldAgeAsset world_era
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return MapBox.instance.era_manager.getCurrentAge();
		}
	}
}
