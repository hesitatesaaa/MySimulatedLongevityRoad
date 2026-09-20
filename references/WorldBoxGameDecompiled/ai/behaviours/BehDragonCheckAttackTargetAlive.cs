namespace ai.behaviours;

public class BehDragonCheckAttackTargetAlive : BehDragon
{
	public override BehResult execute(Actor pActor)
	{
		if (dragon.aggroTargets.Count == 0)
		{
			return BehResult.Continue;
		}
		dragon.aggroTargets.RemoveWhere((Actor tAttacker) => tAttacker == null || !tAttacker.isAlive());
		return BehResult.Continue;
	}
}
