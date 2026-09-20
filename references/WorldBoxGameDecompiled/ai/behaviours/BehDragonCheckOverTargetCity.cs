namespace ai.behaviours;

public class BehDragonCheckOverTargetCity : BehDragon
{
	public override BehResult execute(Actor pActor)
	{
		if (WorldLawLibrary.world_law_peaceful_monsters.isEnabled())
		{
			return BehResult.Continue;
		}
		pActor.data.get("attacksForCity", out var pResult, 0);
		if (pResult == 0)
		{
			return BehResult.Continue;
		}
		pActor.data.get("cityToAttack", out var pResult2, -1L);
		if ((pResult2.hasValue() ? BehaviourActionBase<Actor>.world.cities.get(pResult2) : null) == null)
		{
			return BehResult.Continue;
		}
		if (Randy.randomChance(0.8f))
		{
			return BehResult.Continue;
		}
		if (pActor.isFlying() && !Dragon.canLand(pActor) && dragon.hasTargetsForSlide() && Randy.randomBool())
		{
			pActor.data.set("attacksForCity", --pResult);
			return forceTask(pActor, "dragon_slide");
		}
		if (!pActor.isFlying() && Dragon.canLand(pActor))
		{
			pActor.data.set("attacksForCity", --pResult);
			return forceTask(pActor, "dragon_land_attack");
		}
		return BehResult.Continue;
	}
}
