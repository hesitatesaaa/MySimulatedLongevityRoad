namespace ai.behaviours;

public class BehBoatCheckFishingRepeat : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.inventory.getResource("fish") <= 10)
		{
			return BehResult.RestartTask;
		}
		return forceTask(pActor, "boat_return_to_dock");
	}
}
