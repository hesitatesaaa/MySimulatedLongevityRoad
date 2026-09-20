using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ai.behaviours;

public class TesterBehClickRandomButton : BehaviourActionTester
{
	private Type _type;

	public TesterBehClickRandomButton(Type pButtonType = null)
	{
		_type = pButtonType;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
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
		Component[] componentsInChildren = ((Component)currentWindow).GetComponentsInChildren(_type);
		if (componentsInChildren.Length == 0)
		{
			return BehResult.Stop;
		}
		Component random = Randy.getRandom(componentsInChildren);
		if ((Object)(object)random == (Object)null)
		{
			return BehResult.Stop;
		}
		Button val = default(Button);
		if (!random.TryGetComponent<Button>(ref val))
		{
			return BehResult.Stop;
		}
		pObject.wait = 0.5f;
		ButtonClickedEvent onClick = val.onClick;
		if (onClick != null)
		{
			((UnityEvent)onClick).Invoke();
		}
		return BehResult.Continue;
	}
}
