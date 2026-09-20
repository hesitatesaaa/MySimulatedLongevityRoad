using ai.behaviours;

public class TesterBehPause : BehaviourActionTester
{
	private bool value;

	public TesterBehPause(bool pValue)
	{
		value = pValue;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		Config.paused = value;
		return BehResult.Continue;
	}
}
