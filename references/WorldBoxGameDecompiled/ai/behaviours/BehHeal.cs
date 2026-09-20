namespace ai.behaviours;

public class BehHeal : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasMaxHealth())
		{
			return BehResult.Stop;
		}
		AssetManager.spells.get("cast_blood_rain").action?.Invoke(pActor, pActor, pActor.current_tile);
		pActor.doCastAnimation();
		return BehResult.Continue;
	}
}
