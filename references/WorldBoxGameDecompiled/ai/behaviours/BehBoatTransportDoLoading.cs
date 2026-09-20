using life.taxi;

namespace ai.behaviours;

public class BehBoatTransportDoLoading : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		TaxiRequest taxi_request = boat.taxi_request;
		if (taxi_request == null)
		{
			boat.cancelWork(pActor);
			return BehResult.Stop;
		}
		bool flag = true;
		if (boat.passengerWaitCounter > 4 || boat.countPassengers() >= 100)
		{
			flag = false;
		}
		else if (taxi_request.everyoneEmbarked())
		{
			flag = false;
		}
		if (flag)
		{
			foreach (Actor actor in taxi_request.getActors())
			{
				if (!actor.is_inside_boat && !actor.isFighting() && (!actor.hasTask() || !actor.ai.task.flag_boat_related))
				{
					actor.stopSleeping();
					actor.cancelAllBeh();
					actor.setTask("force_into_a_boat");
				}
			}
			taxi_request.setState(TaxiRequestState.Loading);
			pActor.timer_action = 12f;
			boat.passengerWaitCounter++;
			return BehResult.RepeatStep;
		}
		if (!boat.hasPassengers())
		{
			boat.cancelWork(pActor);
			return BehResult.Stop;
		}
		taxi_request.setState(TaxiRequestState.Transporting);
		taxi_request.cancelForLatePassengers();
		boat.taxi_target = taxi_request.getTileTarget();
		return BehResult.Continue;
	}
}
