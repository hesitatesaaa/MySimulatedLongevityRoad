using ai.behaviours;

public class TesterBehDestroySimObjects : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		foreach (Actor unit in BehaviourActionBase<AutoTesterBot>.world.units)
		{
			unit.getHitFullHealth(AttackType.Divine);
		}
		foreach (Building building in BehaviourActionBase<AutoTesterBot>.world.buildings)
		{
			building.getHitFullHealth(AttackType.Divine);
		}
		return base.execute(pObject);
	}
}
