using ai.behaviours;

public class TesterBehClearFavorites : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pActor)
	{
		foreach (Actor unit in BehaviourActionBase<AutoTesterBot>.world.units)
		{
			if (!unit.isRekt())
			{
				unit.data.favorite = false;
			}
		}
		return base.execute(pActor);
	}
}
