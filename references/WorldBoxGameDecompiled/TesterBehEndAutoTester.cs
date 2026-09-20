using ai.behaviours;

public class TesterBehEndAutoTester : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		BehaviourActionBase<AutoTesterBot>.world.auto_tester.active = false;
		return BehResult.RepeatStep;
	}
}
