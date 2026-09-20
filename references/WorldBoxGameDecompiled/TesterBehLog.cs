using ai.behaviours;

public class TesterBehLog : BehaviourActionTester
{
	private string _msg;

	public TesterBehLog(string pMessage)
	{
		_msg = pMessage;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		new WorldLogMessage(WorldLogLibrary.auto_tester, _msg).add();
		return BehResult.Continue;
	}
}
