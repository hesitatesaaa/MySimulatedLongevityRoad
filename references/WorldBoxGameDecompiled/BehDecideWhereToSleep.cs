using ai.behaviours;

public class BehDecideWhereToSleep : BehaviourActionActor
{
	public override BehResult execute(Actor pObject)
	{
		if (pObject.hasHouseCityInBordersAndSameIsland())
		{
			return forceTask(pObject, "sleep_inside", pClean: false, pForceAction: true);
		}
		return forceTask(pObject, "sleep_outside", pClean: false, pForceAction: true);
	}
}
