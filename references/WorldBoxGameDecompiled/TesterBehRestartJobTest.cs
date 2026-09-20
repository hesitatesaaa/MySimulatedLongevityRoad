using ai.behaviours;

public class TesterBehRestartJobTest : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		pObject.ai.restartJob();
		return BehResult.Continue;
	}
}
