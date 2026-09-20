using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderExtended : Slider, IEndDragHandler, IEventSystemHandler
{
	private SliderEndedEvent _on_sliding_ended;

	private SliderPointerDownEvent _on_pointer_down;

	public void OnEndDrag(PointerEventData pEventData)
	{
		_on_sliding_ended?.Invoke();
	}

	public override void OnPointerDown(PointerEventData pEventData)
	{
		((Slider)this).OnPointerDown(pEventData);
		ScrollWindow.getCurrentWindow().scrollRect.StopMovement();
		_on_pointer_down?.Invoke();
	}

	public void addCallbackDragEnd(SliderEndedEvent pCallback)
	{
		_on_sliding_ended = (SliderEndedEvent)Delegate.Combine(_on_sliding_ended, pCallback);
	}

	public void removeCallbackDragEnd(SliderEndedEvent pCallback)
	{
		_on_sliding_ended = (SliderEndedEvent)Delegate.Remove(_on_sliding_ended, pCallback);
	}

	public void addCallbackPointerDown(SliderPointerDownEvent pCallback)
	{
		_on_pointer_down = (SliderPointerDownEvent)Delegate.Combine(_on_pointer_down, pCallback);
	}

	public void removeCallbackPointerDown(SliderPointerDownEvent pCallback)
	{
		_on_pointer_down = (SliderPointerDownEvent)Delegate.Remove(_on_pointer_down, pCallback);
	}
}
