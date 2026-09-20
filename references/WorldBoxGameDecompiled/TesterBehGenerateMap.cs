using ai.behaviours;

public class TesterBehGenerateMap : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		Config.customZoneX = 7;
		Config.customZoneY = 7;
		BehaviourActionBase<AutoTesterBot>.world.generateNewMap();
		return base.execute(pObject);
	}
}
