using ai.behaviours;

public class TesterBehShutdown : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		return BehResult.Stop;
	}
}
