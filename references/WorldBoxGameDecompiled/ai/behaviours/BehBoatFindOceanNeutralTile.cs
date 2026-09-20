namespace ai.behaviours;

public class BehBoatFindOceanNeutralTile : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		checkHomeDocks(pActor);
		Building homeBuilding = boat.actor.getHomeBuilding();
		if (homeBuilding != null)
		{
			if (pActor.getSimpleComponent<Boat>().isNearDock())
			{
				return BehResult.Stop;
			}
			WorldTile oceanTileInSameOcean = homeBuilding.component_docks.getOceanTileInSameOcean(pActor.current_tile);
			if (oceanTileInSameOcean != null)
			{
				pActor.beh_tile_target = oceanTileInSameOcean;
				return BehResult.Continue;
			}
		}
		WorldTile randomTileForBoat = ActorTool.getRandomTileForBoat(pActor);
		if (randomTileForBoat == null)
		{
			return BehResult.Stop;
		}
		if (randomTileForBoat.zone.city != null && randomTileForBoat.zone.city.kingdom.isEnemy(pActor.kingdom))
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = randomTileForBoat;
		return BehResult.Continue;
	}
}
