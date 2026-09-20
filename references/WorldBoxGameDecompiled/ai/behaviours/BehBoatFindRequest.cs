using life.taxi;

namespace ai.behaviours;

public class BehBoatFindRequest : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		if (boat.taxi_request != null && !boat.taxi_request.isAlreadyUsedByBoat(pActor))
		{
			boat.taxi_request.cancel();
			boat.taxi_request = null;
		}
		boat.taxi_request = TaxiManager.getNewRequestForBoat(pActor);
		if (boat.taxi_request == null)
		{
			return BehResult.Stop;
		}
		boat.taxi_request.assign(boat);
		return forceTask(pActor, "boat_transport_go_load");
	}
}
