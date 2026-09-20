using ai.behaviours;

public class TesterBehWaitYears : BehaviourActionTester
{
	private int wait_years;

	public TesterBehWaitYears(int pWaitYears)
	{
		wait_years = pWaitYears;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		pObject.beh_year_target = Date.getCurrentYear() + wait_years;
		return BehResult.Continue;
	}
}
