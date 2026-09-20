using ai.behaviours;

public class BehAddAggroForBehTarget : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_actor_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.addAggro(pActor.beh_actor_target.a);
		return BehResult.Continue;
	}
}
