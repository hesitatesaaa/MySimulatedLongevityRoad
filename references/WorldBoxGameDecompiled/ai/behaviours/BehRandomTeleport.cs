namespace ai.behaviours;

public class BehRandomTeleport : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasMaxHealth())
		{
			return BehResult.Stop;
		}
		if (!Randy.randomChance(0.3f))
		{
			return BehResult.Stop;
		}
		SpellAsset spellAsset = AssetManager.spells.get("teleport");
		bool flag = false;
		if (spellAsset.action != null)
		{
			flag = spellAsset.action.RunAnyTrue(pActor, pActor, pActor.current_tile);
		}
		if (flag)
		{
			pActor.doCastAnimation();
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
