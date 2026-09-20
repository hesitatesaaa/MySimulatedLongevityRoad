public class WorldBehaviourClouds
{
	public static void spawnRandomCloud()
	{
		if (World.world_era.clouds != null && World.world_era.clouds.Count != 0)
		{
			string random = World.world_era.clouds.GetRandom();
			CloudAsset cloudAsset = AssetManager.clouds.get(random);
			if (cloudAsset != null && (!cloudAsset.normal_cloud || WorldLawLibrary.world_law_clouds.isEnabled()) && (!cloudAsset.considered_disaster || WorldLawLibrary.world_law_disasters_nature.isEnabled()))
			{
				EffectsLibrary.spawn("fx_cloud", null, cloudAsset.id);
			}
		}
	}

	public static void setEra(WorldAgeAsset pAsset)
	{
		WorldBehaviourAsset worldBehaviourAsset = AssetManager.world_behaviours.get("clouds");
		worldBehaviourAsset.interval = pAsset.cloud_interval;
		worldBehaviourAsset.interval_random = pAsset.cloud_interval;
	}
}
