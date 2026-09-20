namespace ai.behaviours;

public class BehCityActorGetRandomDangerZone : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		City city = pActor.city;
		if (!city.hasZones())
		{
			return BehResult.Stop;
		}
		if (!city.isInDanger())
		{
			return BehResult.Stop;
		}
		if (Randy.randomChance(0.2f))
		{
			foreach (TileZone danger_zone in city.danger_zones)
			{
				WorldTile random = danger_zone.tiles.GetRandom();
				if (random.isSameIsland(pActor.current_tile))
				{
					pActor.beh_tile_target = random;
					return BehResult.Continue;
				}
			}
		}
		int num = int.MaxValue;
		WorldTile worldTile = null;
		foreach (TileZone danger_zone2 in city.danger_zones)
		{
			WorldTile centerTile = danger_zone2.centerTile;
			int num2 = Toolbox.SquaredDistTile(pActor.current_tile, centerTile);
			if (num2 <= num && centerTile.isSameIsland(pActor.current_tile) && (num2 != num || !Randy.randomBool()))
			{
				num = num2;
				worldTile = centerTile;
			}
		}
		if (worldTile != null)
		{
			pActor.beh_tile_target = worldTile;
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
