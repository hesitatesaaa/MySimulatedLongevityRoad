namespace ai.behaviours;

public class BehCityActorGetRandomZoneTile : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.city.hasZones())
		{
			return BehResult.Stop;
		}
		WorldTile worldTile = null;
		if (pActor.current_tile.zone.city != pActor.city || Randy.randomChance(0.2f))
		{
			worldTile = pActor.city.zones.GetRandom().getRandomTile();
			if (!worldTile.isSameIsland(pActor.current_tile))
			{
				return BehResult.Stop;
			}
		}
		else
		{
			worldTile = pActor.current_tile.region.tiles.GetRandom();
		}
		pActor.beh_tile_target = worldTile;
		return BehResult.Continue;
	}
}
