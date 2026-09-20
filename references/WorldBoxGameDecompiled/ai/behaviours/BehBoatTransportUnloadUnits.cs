using life.taxi;

namespace ai.behaviours;

public class BehBoatTransportUnloadUnits : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		if (boat.taxi_target == null)
		{
			return BehResult.Stop;
		}
		WorldTile worldTile = PathfinderTools.raycastTileForUnitLandingFromOcean(pActor.current_tile, boat.taxi_target);
		if (worldTile.Type.ocean)
		{
			WorldTile[] neighboursAll = worldTile.neighboursAll;
			foreach (WorldTile worldTile2 in neighboursAll)
			{
				if (worldTile2.Type.ground)
				{
					worldTile = worldTile2;
					break;
				}
			}
		}
		boat.unloadPassengers(worldTile);
		if (boat.taxi_request != null)
		{
			TaxiManager.finish(boat.taxi_request);
			boat.taxi_request = null;
			boat.cancelWork(pActor);
		}
		return BehResult.Continue;
	}
}
