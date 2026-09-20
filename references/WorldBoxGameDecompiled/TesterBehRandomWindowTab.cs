using UnityEngine;
using ai.behaviours;

public class TesterBehRandomWindowTab : BehaviourActionTester
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
			return BehResult.Stop;
		}
		if ((Object)(object)ScrollWindow.getCurrentWindow() == (Object)null)
		{
			return BehResult.Stop;
		}
		string pID = (Randy.randomBool() ? "window_tab_previous" : "window_tab_next");
		HotkeyAsset hotkeyAsset = AssetManager.hotkey_library.get(pID);
		if (hotkeyAsset == null)
		{
			return BehResult.Stop;
		}
		pObject.wait = 0.1f;
		hotkeyAsset.just_pressed_action?.Invoke(hotkeyAsset);
		return BehResult.Continue;
	}
}
