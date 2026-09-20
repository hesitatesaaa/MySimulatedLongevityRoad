namespace ai.behaviours;

public class BehAnimalBreedingTime : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_actor_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (Toolbox.DistTile(pActor.current_tile, pActor.beh_actor_target.current_tile) > 1f)
		{
			return BehResult.Stop;
		}
		pActor.beh_actor_target.a.startShake();
		pActor.startShake();
		pActor.beh_actor_target.a.timer_action = 2f;
		EffectsLibrary.spawnAt("fx_hearts", pActor.current_position, 0.15f);
		return BehResult.Continue;
	}
}
