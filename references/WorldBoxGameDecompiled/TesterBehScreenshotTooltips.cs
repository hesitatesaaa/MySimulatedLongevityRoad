using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using ai.behaviours;

public class TesterBehScreenshotTooltips : BehaviourActionTester
{
	private int screenshots;

	private TooltipScreenshotState state;

	private List<ButtonTrigger> triggers = new List<ButtonTrigger>();

	private ButtonTrigger activeTrigger;

	private bool _screenshot;

	public TesterBehScreenshotTooltips(bool pScreenshot = true)
	{
		_screenshot = pScreenshot;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		string screenshotFolder = TesterBehScreenshotFolder.getScreenshotFolder(LocalizedTextManager.instance.language);
		ScrollWindow currentWindow = ScrollWindow.getCurrentWindow();
		string screen_id = currentWindow.screen_id;
		RectTransform component = ((Component)((Component)currentWindow).transform.FindRecursive("Viewport")).gameObject.GetComponent<RectTransform>();
		string text = ((int)((Transform)((Component)((Component)currentWindow).transform.FindRecursive("Content")).gameObject.GetComponent<RectTransform>()).localPosition.y).ToString("D4");
		switch (state)
		{
		case TooltipScreenshotState.Load:
		{
			Button[] componentsInChildren = ((Component)currentWindow).gameObject.GetComponentsInChildren<Button>();
			foreach (Button val in componentsInChildren)
			{
				if (!((Behaviour)val).isActiveAndEnabled || !((Component)val).gameObject.activeInHierarchy)
				{
					continue;
				}
				EventTrigger component2 = ((Component)val).gameObject.GetComponent<EventTrigger>();
				if ((Object)(object)component2 == (Object)null || ((Object)val).name == "Close")
				{
					continue;
				}
				if ((Object)(object)((Component)((Component)val).transform).GetComponentInParent<ScrollWindow>() != (Object)null)
				{
					Rect worldRect = ((Component)val).gameObject.GetComponent<RectTransform>().GetWorldRect();
					if (!((Rect)(ref worldRect)).Overlaps(component.GetWorldRect()))
					{
						continue;
					}
				}
				int num = 0;
				foreach (Entry trigger in component2.triggers)
				{
					if ((int)trigger.eventID == 0)
					{
						triggers.Add(new ButtonTrigger(val, trigger, ++num));
					}
				}
			}
			state = TooltipScreenshotState.NextTrigger;
			return BehResult.RepeatStep;
		}
		case TooltipScreenshotState.NextTrigger:
			if (triggers.Count == 0)
			{
				state = TooltipScreenshotState.Finish;
				return BehResult.RepeatStep;
			}
			activeTrigger = triggers.Shift();
			if (!((Behaviour)activeTrigger.button).isActiveAndEnabled)
			{
				Debug.LogWarning((object)("button was already disabled: " + ((Object)activeTrigger.button).name), (Object)(object)activeTrigger.button);
				return BehResult.RepeatStep;
			}
			((UnityEvent<BaseEventData>)(object)activeTrigger.entry.callback).Invoke(new BaseEventData(EventSystem.current));
			state = TooltipScreenshotState.Screenshot;
			pObject.wait = 0.01f;
			return BehResult.RepeatStep;
		case TooltipScreenshotState.Cleanup:
			Tooltip.hideTooltipNow();
			state = TooltipScreenshotState.NextTrigger;
			return BehResult.RepeatStep;
		case TooltipScreenshotState.Screenshot:
			if (!Tooltip.anyActive())
			{
				state = TooltipScreenshotState.NextTrigger;
				return BehResult.RepeatStep;
			}
			if (_screenshot)
			{
				screenshots++;
				string text2 = "";
				if (activeTrigger.index > 1)
				{
					text2 = "_" + activeTrigger.index;
				}
				string text3 = screen_id + "_" + text + "_" + screenshots.ToString("D3") + "_" + ((Object)activeTrigger.button).name + text2 + "_5";
				ScreenCapture.CaptureScreenshot(screenshotFolder + "/" + text3 + ".png");
			}
			state = TooltipScreenshotState.Cleanup;
			return BehResult.RepeatStep;
		case TooltipScreenshotState.Finish:
			state = TooltipScreenshotState.Load;
			screenshots = 0;
			activeTrigger = default(ButtonTrigger);
			triggers.Clear();
			return BehResult.Continue;
		default:
			Debug.LogError((object)("TesterBehScreenshotTooltips: Unknown state: " + state));
			return BehResult.Stop;
		}
	}
}
