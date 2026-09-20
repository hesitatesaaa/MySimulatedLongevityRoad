using tools;

namespace ai.behaviours;

public class BehBoatTransportFindTilePickUp : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		if (boat.taxi_request == null || !boat.taxi_request.isStillLegit())
		{
			boat.cancelWork(pActor);
			return BehResult.Stop;
		}
		WorldTile worldTile = null;
		boat.pickup_near_dock = false;
		ActorTool.checkHomeDocks(pActor);
		Building homeBuilding = boat.actor.getHomeBuilding();
		if (homeBuilding != null)
		{
			WorldTile oceanTileInSameOcean = homeBuilding.component_docks.getOceanTileInSameOcean(pActor.current_tile);
			if (oceanTileInSameOcean != null && oceanTileInSameOcean.isSameIsland(boat.taxi_request.getTileStart()))
			{
				boat.pickup_near_dock = true;
				if (boat.isNearDock())
				{
					boat.passengerWaitCounter = 0;
					pActor.beh_tile_target = pActor.current_tile;
					return BehResult.Continue;
				}
				pActor.beh_tile_target = oceanTileInSameOcean;
				return BehResult.Continue;
			}
		}
		worldTile = OceanHelper.findTileForBoat(pActor.current_tile, boat.taxi_request.getTileStart());
		if (worldTile == null)
		{
			boat.cancelWork(pActor);
			return BehResult.Stop;
		}
		boat.passengerWaitCounter = 0;
		pActor.beh_tile_target = worldTile;
		return BehResult.Continue;
	}
}
