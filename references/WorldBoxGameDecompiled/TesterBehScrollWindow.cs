using UnityEngine;
using UnityEngine.UI;
using ai.behaviours;

public class TesterBehScrollWindow : BehaviourActionTester
{
	private static string[] skipWindows = new string[2] { "saves_list", "patch_log" };

	public override BehResult execute(AutoTesterBot pObject)
	{
		ScrollWindow currentWindow = ScrollWindow.getCurrentWindow();
		string screen_id = currentWindow.screen_id;
		if (skipWindows.IndexOf(screen_id) > -1)
		{
			return BehResult.Continue;
		}
		Transform val = ((Component)currentWindow).transform.FindRecursive("Scrollbar Vertical");
		if (!((Component)val).gameObject.activeInHierarchy)
		{
			return BehResult.Continue;
		}
		Scrollbar component = ((Component)val).gameObject.GetComponent<Scrollbar>();
		float value = component.value;
		float size = component.size;
		if (size < 0.05f)
		{
			return BehResult.Continue;
		}
		if (size > 0.95f)
		{
			return BehResult.Continue;
		}
		if (value > 0.1f)
		{
			value -= size;
			if (value < 0f)
			{
				value = 0f;
			}
			component.value = value;
			return BehResult.RestartTask;
		}
		return BehResult.Continue;
	}
}
