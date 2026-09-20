using tools;

namespace ai.behaviours;

public class BehBoatTransportFindTileUnload : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		if (boat.taxi_target == null)
		{
			return BehResult.Stop;
		}
		WorldTile worldTile = OceanHelper.findTileForBoat(pActor.current_tile, boat.taxi_target);
		if (worldTile == null)
		{
			boat.cancelWork(pActor);
			return BehResult.Stop;
		}
		pActor.beh_tile_target = worldTile;
		return BehResult.Continue;
	}
}
