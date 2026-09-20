namespace ai.behaviours;

public class BehUFOCheckAttackCity : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("cityToAttack", out var pResult, -1L);
		if (pResult.hasValue() && pActor.current_tile.hasBuilding() && !WorldLawLibrary.world_law_peaceful_monsters.isEnabled() && pActor.current_tile.building.isUsable())
		{
			return forceTask(pActor, "ufo_attack");
		}
		pActor.data.get("attacksForCity", out var pResult2, 0);
		if (pResult2 > 0)
		{
			return BehResult.RestartTask;
		}
		return BehResult.Continue;
	}
}
