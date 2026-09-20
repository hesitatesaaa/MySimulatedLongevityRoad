using ai.behaviours;

public class BehLaunchFireworks : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasEnoughMoney(SimGlobals.m.festive_fireworks_cost))
		{
			return BehResult.Stop;
		}
		pActor.finishStatusEffect("festive_spirit");
		spawnFireworksByUnit(pActor);
		pActor.spendMoney(SimGlobals.m.festive_fireworks_cost);
		return BehResult.Continue;
	}

	internal void spawnFireworksByUnit(Actor pActor)
	{
		EffectsLibrary.spawn("fx_fireworks", pActor.current_tile);
	}
}
