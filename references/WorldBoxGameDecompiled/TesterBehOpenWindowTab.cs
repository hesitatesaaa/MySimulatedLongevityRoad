using UnityEngine;
using ai.behaviours;

public class TesterBehOpenWindowTab : BehaviourActionTester
{
	private string _tab;

	public TesterBehOpenWindowTab(string pTab = null)
	{
		_tab = pTab;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		pObject.wait = 0.5f;
		if (ScrollWindow.isAnimationActive())
		{
			return BehResult.RepeatStep;
		}
		if (!ScrollWindow.isWindowActive())
		{
			return BehResult.Stop;
		}
		ScrollWindow currentWindow = ScrollWindow.getCurrentWindow();
		if ((Object)(object)currentWindow == (Object)null)
		{
			return BehResult.Stop;
		}
		currentWindow.tabs.showTab(_tab);
		return BehResult.Continue;
	}
}
