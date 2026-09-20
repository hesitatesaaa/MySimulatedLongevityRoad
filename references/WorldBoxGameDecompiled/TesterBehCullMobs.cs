using ai.behaviours;

public class TesterBehCullMobs : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pActor)
	{
		foreach (Actor unit in BehaviourActionBase<AutoTesterBot>.world.units)
		{
			if (!unit.isRekt() && unit.isKingdomMob() && !Randy.randomChance(0.1f))
			{
				unit.getHit(10000f, pFlash: false, AttackType.Divine);
			}
		}
		return base.execute(pActor);
	}
}
