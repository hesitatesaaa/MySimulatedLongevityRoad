using ai.behaviours;

public class TesterBehOpenWindow : BehaviourActionTester
{
	private string _type;

	public TesterBehOpenWindow(string pType)
	{
		_type = pType;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		pObject.wait = 0.5f;
		if (ScrollWindow.isAnimationActive())
		{
			return BehResult.RepeatStep;
		}
		string type = _type;
		if (_type == "random")
		{
			type = AssetManager.window_library.getTestableWindows().GetRandom().id;
		}
		ScrollWindow.showWindow(type, pSkipAnimation: true);
		return BehResult.Continue;
	}
}
