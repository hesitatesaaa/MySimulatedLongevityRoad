using ai.behaviours;

public class BehSexualReproductionTry : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		base.execute(pActor);
		bool flag = (pActor.hasHouseCityInBordersAndSameIsland() ? true : false);
		pActor.subspecies.counter_reproduction_sexual_try?.registerEvent();
		if (flag)
		{
			return forceTask(pActor, "sexual_reproduction_inside", pClean: false, pForceAction: true);
		}
		return forceTask(pActor, "sexual_reproduction_check_outside", pClean: false, pForceAction: true);
	}
}
