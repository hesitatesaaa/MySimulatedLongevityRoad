using ai.behaviours;

public class TesterBehWait : BehaviourActionTester
{
	private float wait;

	public TesterBehWait(float pWait)
	{
		wait = pWait;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		pObject.wait = wait;
		return base.execute(pObject);
	}
}
