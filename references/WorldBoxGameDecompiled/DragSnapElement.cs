using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
public class DragSnapElement : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler, IDraggable, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private bool _spawn_particles_on_drag;

	[SerializeField]
	private bool _touch_drag_delay;

	private float _drag_timer_started_at;

	private Tweener _tweener;

	private LayoutElement _layout_element;

	private Button _button;

	private Vector3 _start_local_position;

	private Transform _start_parent;

	public float limit_max_drag_distance;

	public float snapback_max_distance;

	public float snapback_speed_max_distance;

	public float snapback_min_distance;

	public float snapback_speed_min_distance;

	public Transform attach_parent;

	public Transform fly_back_parent;

	public Ease ease;

	public float speed;

	private ScrollRect _scroll_rect;

	private ScrollRectExtended _scroll_rect_extended;

	private bool _initial_ignore_layout;

	private bool _hovered;

	private bool _is_dragging;

	public bool spawn_particles_on_drag => _spawn_particles_on_drag;

	private void Start()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		_layout_element = ((Component)this).GetComponent<LayoutElement>();
		_button = ((Component)this).GetComponent<Button>();
		_start_parent = ((Component)this).transform.parent;
		_start_local_position = ((Component)this).transform.localPosition;
		_initial_ignore_layout = _layout_element.ignoreLayout;
		if ((Object)(object)attach_parent == (Object)null)
		{
			attach_parent = World.world.drag_parent;
		}
		if ((Object)(object)fly_back_parent == (Object)null)
		{
			fly_back_parent = ((Component)this).transform.FindParentWithName("Content", "Viewport") ?? attach_parent;
		}
		ScrollableButton scrollableButton = default(ScrollableButton);
		if (((Component)this).gameObject.TryGetComponent<ScrollableButton>(ref scrollableButton))
		{
			((Behaviour)scrollableButton).enabled = false;
			_scroll_rect_extended = ((Component)this).gameObject.GetComponentInParent<ScrollRectExtended>();
			if ((Object)(object)_scroll_rect_extended == (Object)null)
			{
				_scroll_rect = ((Component)this).gameObject.GetComponentInParent<ScrollRect>();
			}
		}
	}

	public void OnInitializePotentialDrag(PointerEventData pEventData)
	{
		if (_touch_drag_delay)
		{
			_drag_timer_started_at = Time.time;
		}
	}

	public void OnBeginDrag(PointerEventData pEventData)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (!_is_dragging && !Config.isDraggingItem() && !isTouchDragDelayed())
		{
			TweenExtensions.Kill((Tween)(object)_tweener, false);
			Config.setDraggingObject(this);
			_is_dragging = true;
			((Component)this).transform.SetParent(attach_parent);
			((Behaviour)_button).enabled = false;
			((Behaviour)_layout_element).enabled = true;
			updatePosition(Vector2.op_Implicit(pEventData.position));
		}
	}

	public void OnDrag(PointerEventData pEventData)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (Config.isDraggingObject(this) && !isTouchDragDelayed())
		{
			updatePosition(Vector2.op_Implicit(pEventData.position));
		}
	}

	public float getDragMod()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!_is_dragging)
		{
			return 0f;
		}
		Vector3 val = _start_parent.TransformPoint(_start_local_position);
		Vector3 val2 = val - ((Component)this).transform.position;
		float num = Mathf.Clamp01(((Vector3)(ref val2)).magnitude / limit_max_drag_distance);
		if (val.y > ((Component)this).transform.position.y)
		{
			num = 0f - num;
		}
		return num;
	}

	private void updatePosition(Vector3 pTargetPosition)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = _start_parent.TransformPoint(_start_local_position);
		Vector3 val2 = pTargetPosition - val;
		if (((Vector3)(ref val2)).magnitude > limit_max_drag_distance)
		{
			((Component)this).transform.position = val + ((Vector3)(ref val2)).normalized * limit_max_drag_distance;
		}
		else
		{
			((Component)this).transform.position = pTargetPosition;
		}
	}

	public void OnEndDrag(PointerEventData pEventData)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		if (Config.isDraggingItem() && Config.isDraggingObject(this))
		{
			Config.clearDraggingObject();
			_is_dragging = false;
			_drag_timer_started_at = 0f;
			_layout_element.ignoreLayout = true;
			((Component)this).transform.SetParent(fly_back_parent);
			Tweener tweener = _tweener;
			if (tweener != null)
			{
				TweenExtensions.Kill((Tween)(object)tweener, false);
			}
			Vector3 val = _start_parent.TransformPoint(_start_local_position);
			Vector3 val2 = val - ((Component)this).transform.position;
			float magnitude = ((Vector3)(ref val2)).magnitude;
			float num = dragSpeed(magnitude);
			_tweener = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(DOTween.To((DOGetter<Vector3>)delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return ((Component)this).transform.position;
			}, (DOSetter<Vector3>)delegate(Vector3 pVector)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				((Component)this).transform.position = pVector;
			}, val, num), ease), new TweenCallback(resetElement));
			Tooltip.blockTooltips(num * 0.7f);
		}
	}

	public void resetElement()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (_hovered)
		{
			_button.TriggerHover();
		}
		if (!((Object)(object)_start_parent == (Object)null) && (!((Object)(object)fly_back_parent != (Object)(object)_start_parent) || !((Object)(object)_start_parent == (Object)(object)((Component)this).transform.parent)))
		{
			((Component)this).transform.SetParent(_start_parent);
			((Component)this).transform.localPosition = _start_local_position;
			((Behaviour)_button).enabled = true;
			_layout_element.ignoreLayout = _initial_ignore_layout;
			((Behaviour)_layout_element).enabled = false;
		}
	}

	public float dragSpeed(float pDistance)
	{
		float num = (Mathf.Clamp(pDistance, snapback_min_distance, snapback_max_distance) - snapback_min_distance) / (snapback_max_distance - snapback_min_distance);
		return Mathf.Lerp(snapback_speed_min_distance, snapback_speed_max_distance, num);
	}

	public void onWindowClose(string pId)
	{
		TweenExtensions.Kill((Tween)(object)_tweener, true);
	}

	public void OnScroll(PointerEventData pEventData)
	{
		sendMessage("OnScroll", pEventData);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_hovered = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_hovered = false;
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

	public void OnEnable()
	{
		ScrollWindow.addCallbackHide(onWindowClose);
	}

	public void OnDisable()
	{
		ScrollWindow.removeCallbackHide(onWindowClose);
		KillDrag();
		if (TweenExtensions.IsActive((Tween)(object)_tweener))
		{
			Debug.LogError((object)"OnDisable kill called, shouldn't happen", (Object)(object)this);
			TweenExtensions.Kill((Tween)(object)_tweener, false);
		}
	}

	public void KillDrag()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		if (_is_dragging)
		{
			OnEndDrag(new PointerEventData(EventSystem.current));
			TweenExtensions.Kill((Tween)(object)_tweener, true);
		}
	}

	private bool isTouchDragDelayed()
	{
		if (_touch_drag_delay && !InputHelpers.mouseSupported)
		{
			return Time.time - _drag_timer_started_at < 0.2f;
		}
		return false;
	}

	public DragSnapElement()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_spawn_particles_on_drag = true;
		limit_max_drag_distance = 77f;
		snapback_max_distance = 77f;
		snapback_speed_max_distance = 0.35f;
		snapback_min_distance = 22f;
		snapback_speed_min_distance = 0.9f;
		ease = (Ease)24;
		speed = 0.4f;
		((MonoBehaviour)this)._002Ector();
	}
}
