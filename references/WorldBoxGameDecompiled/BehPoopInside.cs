using ai.behaviours;

public class BehPoopInside : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.donePooping();
		string text = ((!pActor.hasSubspecies()) ? "poop" : pActor.subspecies.getRandomBioProduct());
		if (text != "poop")
		{
			BuildingHelper.tryToBuildNear(pActor.current_tile, text);
		}
		return BehResult.Continue;
	}
}
