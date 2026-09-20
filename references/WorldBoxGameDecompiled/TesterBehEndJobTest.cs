using ai.behaviours;

public class TesterBehEndJobTest : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		pObject.ai.reset();
		pObject.stopAutoTester();
		return BehResult.Continue;
	}
}
