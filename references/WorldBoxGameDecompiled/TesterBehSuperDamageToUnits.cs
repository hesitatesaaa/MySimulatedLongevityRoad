using ai.behaviours;

public class TesterBehSuperDamageToUnits : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		foreach (Actor unit in BehaviourActionBase<AutoTesterBot>.world.units)
		{
			if (unit.asset.can_be_killed_by_stuff)
			{
				unit.getHit(1E+17f, pFlash: true, AttackType.Divine);
			}
		}
		return base.execute(pObject);
	}
}
