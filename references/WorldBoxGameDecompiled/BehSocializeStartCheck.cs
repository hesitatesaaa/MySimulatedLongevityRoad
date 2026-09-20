using ai.behaviours;

public class BehSocializeStartCheck : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		base.execute(pActor);
		if (pActor.hasTelepathicLink())
		{
			return forceTask(pActor, "socialize_try_to_start_immediate", pClean: false);
		}
		if (pActor.hasCity() && pActor.city.hasBuildingType("type_bonfire", pCountOnlyFinished: true, pActor.current_island))
		{
			return forceTask(pActor, "socialize_try_to_start_near_bonfire", pClean: false);
		}
		return forceTask(pActor, "socialize_try_to_start_immediate", pClean: false);
	}
}
