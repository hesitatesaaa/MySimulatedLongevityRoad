namespace ai.behaviours;

public class BehDragonCheckOverTargetActor : BehDragon
{
	public override BehResult execute(Actor pActor)
	{
		if (WorldLawLibrary.world_law_peaceful_monsters.isEnabled())
		{
			return BehResult.Continue;
		}
		if (dragon.aggroTargets.Count == 0)
		{
			return BehResult.Continue;
		}
		if (!Dragon.canLand(pActor))
		{
			return BehResult.Continue;
		}
		if (dragon.targetsWithinLandAttackRange())
		{
			return forceTask(pActor, "dragon_land_attack");
		}
		return BehResult.Continue;
	}
}
