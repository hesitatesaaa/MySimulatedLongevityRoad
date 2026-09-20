using ai.behaviours;

public class BehDecideWhereToPoop : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.isAdult() && pActor.hasHouseCityInBordersAndSameIsland())
		{
			return forceTask(pActor, "poop_inside", pClean: false, pForceAction: true);
		}
		return forceTask(pActor, "poop_outside", pClean: false, pForceAction: true);
	}
}
