using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public static class ButtonExtensions
{
	public static void TriggerHover(this Button button)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		if (Input.mousePresent)
		{
			EventTrigger val = ((Component)button).gameObject.GetComponent<EventTrigger>();
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)button).gameObject.AddComponent<EventTrigger>();
			}
			val.OnPointerEnter(new PointerEventData(EventSystem.current));
		}
	}

	public static void OnHover(this Button button, UnityAction call)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (Input.mousePresent)
		{
			EventTrigger val = ((Component)button).gameObject.GetComponent<EventTrigger>();
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)button).gameObject.AddComponent<EventTrigger>();
			}
			Entry val2 = new Entry();
			val2.eventID = (EventTriggerType)0;
			((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				call.Invoke();
			});
			val.triggers.Add(val2);
		}
	}

	public static void OnHoverOut(this Button button, UnityAction call)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (Input.mousePresent)
		{
			EventTrigger val = ((Component)button).gameObject.GetComponent<EventTrigger>();
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)button).gameObject.AddComponent<EventTrigger>();
			}
			Entry val2 = new Entry();
			val2.eventID = (EventTriggerType)1;
			((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				call.Invoke();
			});
			val.triggers.Add(val2);
		}
	}

	public static void OnHover(this Slider slider, UnityAction call)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (Input.mousePresent)
		{
			EventTrigger val = ((Component)slider).gameObject.GetComponent<EventTrigger>();
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)slider).gameObject.AddComponent<EventTrigger>();
			}
			Entry val2 = new Entry();
			val2.eventID = (EventTriggerType)0;
			((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				call.Invoke();
			});
			val.triggers.Add(val2);
		}
	}

	public static void OnHoverOut(this Slider slider, UnityAction call)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (Input.mousePresent)
		{
			EventTrigger val = ((Component)slider).gameObject.GetComponent<EventTrigger>();
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)slider).gameObject.AddComponent<EventTrigger>();
			}
			Entry val2 = new Entry();
			val2.eventID = (EventTriggerType)1;
			((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
			{
				call.Invoke();
			});
			val.triggers.Add(val2);
		}
	}
}
