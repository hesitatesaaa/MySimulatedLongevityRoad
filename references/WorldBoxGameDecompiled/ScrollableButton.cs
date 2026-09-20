using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ScrollableButton : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler, IScrollHandler
{
	private ScrollRect _scroll_rect;

	private ScrollRectExtended _scroll_rect_extended;

	private Button _button;

	private bool _has_button;

	[SerializeField]
	private bool _scroll_wheel_only;

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
		_has_button = ((Component)this).gameObject.TryGetComponent<Button>(ref _button);
	}

	public void OnBeginDrag(PointerEventData pEventData)
	{
		if (!_scroll_wheel_only)
		{
			sendMessage("OnBeginDrag", pEventData);
			if (_has_button)
			{
				((Selectable)_button).interactable = false;
			}
		}
	}

	public void OnDrag(PointerEventData pEventData)
	{
		if (!_scroll_wheel_only)
		{
			sendMessage("OnDrag", pEventData);
		}
	}

	public void OnEndDrag(PointerEventData pEventData)
	{
		if (!_scroll_wheel_only)
		{
			sendMessage("OnEndDrag", pEventData);
			if (_has_button)
			{
				((Selectable)_button).interactable = true;
			}
		}
	}

	public void OnInitializePotentialDrag(PointerEventData pEventData)
	{
		if (!_scroll_wheel_only)
		{
			sendMessage("OnInitializePotentialDrag", pEventData);
		}
	}

	public void OnScroll(PointerEventData pEventData)
	{
		sendMessage("OnScroll", pEventData);
	}

	private void sendMessage(string pMethodName, PointerEventData pEventData)
	{
		ScrollRect scroll_rect = _scroll_rect;
		if (scroll_rect != null)
		{
			((Component)scroll_rect).SendMessage(pMethodName, (object)pEventData);
		}
		ScrollRectExtended scroll_rect_extended = _scroll_rect_extended;
		if (scroll_rect_extended != null)
		{
			((Component)scroll_rect_extended).SendMessage(pMethodName, (object)pEventData);
		}
	}
}
