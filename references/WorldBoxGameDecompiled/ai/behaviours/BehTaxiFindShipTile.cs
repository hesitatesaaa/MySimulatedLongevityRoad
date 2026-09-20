using life.taxi;

namespace ai.behaviours;

public class BehTaxiFindShipTile : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		TaxiRequest requestForActor = TaxiManager.getRequestForActor(pActor);
		if (requestForActor == null || !requestForActor.hasAssignedBoat() || requestForActor.state != TaxiRequestState.Loading)
		{
			return BehResult.Stop;
		}
		Boat boat = requestForActor.getBoat();
		WorldTile worldTile = null;
		if (boat.pickup_near_dock)
		{
			Building homeBuilding = boat.actor.getHomeBuilding();
			if (homeBuilding != null)
			{
				WorldTile constructionTile = homeBuilding.getConstructionTile();
				if (constructionTile != null)
				{
					worldTile = constructionTile.region.tiles.GetRandom();
				}
			}
		}
		if (worldTile == null)
		{
			worldTile = PathfinderTools.raycastTileForUnitToEmbark(pActor.current_tile, boat.actor.current_tile);
		}
		if (worldTile == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = worldTile;
		return BehResult.Continue;
	}
}
