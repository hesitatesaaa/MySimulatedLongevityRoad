using ai.behaviours;

public class TesterBehResetSeeds : BehaviourActionTester
{
	private int value;

	public TesterBehResetSeeds(int pValue)
	{
		value = pValue;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		Randy.resetSeed(value);
		return BehResult.Continue;
	}
}
