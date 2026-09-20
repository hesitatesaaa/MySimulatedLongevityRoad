using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Slider))]
[RequireComponent(typeof(SliderExtended))]
public class ScrollableSlider : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	private ScrollRect _scroll_rect;

	private ScrollRectExtended _scroll_rect_extended;

	protected void Start()
	{
		_scroll_rect_extended = ((Component)this).gameObject.GetComponentInParent<ScrollRectExtended>();
		if ((Object)(object)_scroll_rect_extended == (Object)null)
		{
			_scroll_rect = ((Component)this).gameObject.GetComponentInParent<ScrollRect>();
		}
		if ((Object)(object)_scroll_rect == (Object)null && (Object)(object)_scroll_rect_extended == (Object)null)
		{
			((Behaviour)this).enabled = false;
		}
	}

	public void OnScroll(PointerEventData pEventData)
	{
		ScrollRect scroll_rect = _scroll_rect;
		if (scroll_rect != null)
		{
			((Component)scroll_rect).SendMessage("OnScroll", (object)pEventData);
		}
		ScrollRectExtended scroll_rect_extended = _scroll_rect_extended;
		if (scroll_rect_extended != null)
		{
			((Component)scroll_rect_extended).SendMessage("OnScroll", (object)pEventData);
		}
	}
}
