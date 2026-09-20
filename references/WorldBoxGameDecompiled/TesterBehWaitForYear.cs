using ai.behaviours;

public class TesterBehWaitForYear : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		float num = pObject.beh_year_target;
		float num2 = Date.getCurrentYear();
		if (num - num2 <= 0f)
		{
			return BehResult.Continue;
		}
		pObject.wait = 1f;
		return BehResult.RepeatStep;
	}
}
