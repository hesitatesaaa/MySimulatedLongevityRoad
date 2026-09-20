using life.taxi;

namespace ai.behaviours;

public class BehTaxiEmbark : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		TaxiRequest requestForActor = TaxiManager.getRequestForActor(pActor);
		if (requestForActor == null || !requestForActor.hasAssignedBoat() || requestForActor.state != TaxiRequestState.Loading)
		{
			return BehResult.Stop;
		}
		Boat boat = requestForActor.getBoat();
		bool flag = requestForActor.isBoatNearDock();
		if (((float)Toolbox.SquaredDistTile(boat.actor.current_tile, pActor.current_tile) < 25f) | flag)
		{
			pActor.beh_tile_target = null;
			pActor.embarkInto(requestForActor.getBoat());
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
