using UnityEngine;
using ai.behaviours;

public class BehGenerateLootFromHouse : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasHouse())
		{
			return BehResult.Stop;
		}
		Building homeBuilding = pActor.getHomeBuilding();
		int loot_generation = homeBuilding.asset.loot_generation;
		int num = 0;
		BiomeAsset biome = homeBuilding.current_tile.getBiome();
		if (biome != null)
		{
			num = biome.loot_generation;
		}
		int num2 = loot_generation + num;
		num2 = Mathf.Max(1, num2);
		pActor.addLoot(num2);
		return BehResult.Continue;
	}
}
