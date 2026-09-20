using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class RunningIcon : MonoBehaviour, IDragHandler, IEventSystemHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler, IPointerClickHandler, IDraggable
{
	[SerializeField]
	private Image _icon;

	private RunningIcons _parent;

	private Vector2 _last_position;

	public bool spawn_particles_on_drag => false;

	public void Awake()
	{
		_parent = ((Component)this).GetComponentInParent<RunningIcons>();
	}

	public Image getIconImage()
	{
		return _icon;
	}

	public void setIcon(Sprite pIcon)
	{
		_icon.sprite = pIcon;
	}

	public void setIconColor(Color pColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)_icon).color = pColor;
	}

	public void OnBeginDrag(PointerEventData pEventData)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!Config.isDraggingItem())
		{
			Config.setDraggingObject(this);
			_last_position = pEventData.position;
			_parent.toggle(pState: false);
		}
	}

	public void OnDrag(PointerEventData pEventData)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!Config.isDraggingObject(this))
		{
			return;
		}
		_parent.toggle(pState: false);
		Vector2 val = pEventData.position - _last_position;
		_last_position = pEventData.position;
		if (val.x != 0f)
		{
			float num = val.x / CanvasMain.instance.canvas_ui.scaleFactor;
			if (num < 0f)
			{
				_parent.moveBy(Mathf.Abs(num), RunningIcons.Direction.Left);
			}
			else
			{
				_parent.moveBy(Mathf.Abs(num), RunningIcons.Direction.Right);
			}
		}
	}

	public void OnEndDrag(PointerEventData pEventData)
	{
		if (Config.isDraggingItem() && Config.isDraggingObject(this))
		{
			Config.clearDraggingObject();
			_parent.toggle(pState: true);
		}
	}

	public void OnScroll(PointerEventData pEventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (pEventData.scrollDelta.y < 0f)
		{
			_parent.moveBy(Mathf.Abs(pEventData.scrollDelta.y * 20f), RunningIcons.Direction.Left);
		}
		else
		{
			_parent.moveBy(Mathf.Abs(pEventData.scrollDelta.y * 20f), RunningIcons.Direction.Right);
		}
	}

	public void OnPointerClick(PointerEventData pEventData)
	{
		if (!InputHelpers.mouseSupported)
		{
			((UnityEvent)((Component)this).GetComponent<Button>().onClick).Invoke();
			if ((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)_parent).gameObject)
			{
				_parent.toggle(pState: false);
			}
			EventSystem.current.SetSelectedGameObject(((Component)_parent).gameObject);
		}
	}

	private void OnDisable()
	{
		KillDrag();
	}

	public void KillDrag()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		OnEndDrag(new PointerEventData(EventSystem.current));
	}
}
