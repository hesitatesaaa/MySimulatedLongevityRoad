using ai.behaviours;

public class TesterBehRestartTask : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pActor)
	{
		return BehResult.RestartTask;
	}
}
