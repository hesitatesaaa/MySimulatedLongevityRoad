namespace ai.behaviours;

public class BehCrabBurrow : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		force_animation = true;
		force_animation_id = "burrow";
		special_prevent_can_be_attacked = true;
	}

	public override BehResult execute(Actor pActor)
	{
		if (pActor.isHungry())
		{
			pActor.endJob();
			return BehResult.Stop;
		}
		if (!Toolbox.hasDifferentSpeciesInChunkAround(pActor.current_tile, pActor.asset.id))
		{
			pActor.endJob();
			return BehResult.Stop;
		}
		pActor.timer_action = Randy.randomFloat(10f, 20f);
		return BehResult.RepeatStep;
	}
}
