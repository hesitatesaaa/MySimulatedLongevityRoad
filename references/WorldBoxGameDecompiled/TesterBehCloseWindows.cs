using ai.behaviours;

public class TesterBehCloseWindows : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		if (ScrollWindow.isAnimationActive())
		{
			pObject.wait = 0.1f;
			return BehResult.RepeatStep;
		}
		if (!ScrollWindow.isWindowActive())
		{
			return BehResult.Continue;
		}
		ScrollWindow.hideAllEvent(pWithAnimation: false);
		pObject.wait = 0.25f;
		return BehResult.RepeatStep;
	}
}
