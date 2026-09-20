using ai.behaviours;

public class TesterBehChangeWorldSpeed : BehaviourActionTester
{
	private string _time_scale_id;

	public TesterBehChangeWorldSpeed(string pTimeScaleID)
	{
		_time_scale_id = pTimeScaleID;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		Config.setWorldSpeed(_time_scale_id);
		return BehResult.Continue;
	}
}
