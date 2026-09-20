using LayoutGroupExt;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class DragOrderElement : MonoBehaviour, IDraggable, IEndDragHandler, IEventSystemHandler
{
	[SerializeField]
	private bool _spawn_particles_on_drag = true;

	public RectTransform main_transform;

	public bool can_be_dragged = true;

	private DragOrderContainer _container;

	private int _parent_canvas_sorting_order;

	private Canvas _canvas;

	private GraphicRaycaster _raycaster;

	private Button _button;

	private Transform _current_parent;

	internal Vector2 current_destination;

	internal bool is_target_reached;

	internal int order_index;

	private bool _drag_initialized;

	private float _drag_started_at;

	private int _mouse_button = -1;

	private Vector3 _prev_mouse_position;

	public bool spawn_particles_on_drag => _spawn_particles_on_drag;

	private void Start()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		if ((Object)(object)main_transform == (Object)null)
		{
			main_transform = ((Component)this).GetComponent<RectTransform>();
		}
		_parent_canvas_sorting_order = ((Component)main_transform).gameObject.GetComponentInParent<Canvas>().sortingOrder;
		_canvas = ((Component)main_transform).gameObject.AddComponent<Canvas>();
		_canvas.sortingOrder = _parent_canvas_sorting_order;
		_canvas.overrideSorting = false;
		_raycaster = ((Component)main_transform).gameObject.AddComponent<GraphicRaycaster>();
		_raycaster.blockingObjects = (BlockingObjects)3;
		_raycaster.blockingMask = LayerMask.op_Implicit(-1);
		_raycaster.ignoreReversedGraphics = true;
		_button = ((Component)this).GetComponent<Button>();
		((UnityEvent)_button.onClick).AddListener((UnityAction)delegate
		{
			_container?.updateChildrenData();
		});
		is_target_reached = true;
		checkContainer();
	}

	private void checkContainer()
	{
		if (!((Object)(object)_container != (Object)null))
		{
			_container = ((Component)main_transform).GetComponentInParent<DragOrderContainer>();
		}
	}

	private void checkParent()
	{
		Transform parent = ((Transform)main_transform).parent;
		if (!((Object)(object)_current_parent == (Object)(object)parent))
		{
			_current_parent = parent;
			checkContainer();
		}
	}

	private void Update()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		checkParent();
		if ((Object)(object)_container == (Object)null)
		{
			return;
		}
		checkDrag();
		if (((Behaviour)_container.grid_layout).enabled)
		{
			return;
		}
		if ((Object)(object)_container.dragging_element == (Object)(object)this)
		{
			moveDraggingTab();
		}
		else if (!is_target_reached)
		{
			if (Vector3.Distance(((Transform)main_transform).localPosition, Vector2.op_Implicit(current_destination)) < 0.1f)
			{
				is_target_reached = true;
				unsetOnTop();
			}
			else
			{
				((Transform)main_transform).localPosition = Vector3.Lerp(((Transform)main_transform).localPosition, Vector2.op_Implicit(current_destination), Time.deltaTime * 10f);
			}
		}
	}

	private void setOnTop()
	{
		_canvas.overrideSorting = true;
		_canvas.sortingOrder = 24;
	}

	internal void unsetOnTop()
	{
		if (_canvas.overrideSorting)
		{
			_canvas.sortingOrder = _parent_canvas_sorting_order;
			_canvas.overrideSorting = false;
		}
	}

	public void updatePosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(getChildPositionInContainer());
		if (!(Vector2.op_Implicit(((Transform)main_transform).localPosition) == val) && !(current_destination == val))
		{
			current_destination = val;
			is_target_reached = false;
		}
	}

	private void moveDraggingTab()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (!_container.is_anything_dragging)
		{
			endDrag();
			return;
		}
		if (!InputHelpers.GetMouseButton(_mouse_button))
		{
			endDrag();
			return;
		}
		Vector3 mousePosition = Input.mousePosition;
		switch (_container.snapping_axis)
		{
		case DragOrderContainer.SnapAxis.Horizontal:
			mousePosition.y = ((Transform)main_transform).position.y;
			break;
		case DragOrderContainer.SnapAxis.Vertical:
			mousePosition.x = ((Transform)main_transform).position.x;
			break;
		}
		if (!_container.limit_moving)
		{
			((Transform)main_transform).position = mousePosition;
			return;
		}
		Rect worldRect = _container.rect_transform.GetWorldRect();
		getGridValues(_container.grid_layout, out var pCellSize, out var _);
		pCellSize *= 0.5f;
		mousePosition.x = Mathf.Min(mousePosition.x, ((Rect)(ref worldRect)).xMax - pCellSize.x);
		mousePosition.x = Mathf.Max(mousePosition.x, ((Rect)(ref worldRect)).xMin + pCellSize.x);
		mousePosition.y = Mathf.Min(mousePosition.y, ((Rect)(ref worldRect)).yMax - pCellSize.y);
		mousePosition.y = Mathf.Max(mousePosition.y, ((Rect)(ref worldRect)).yMin + pCellSize.y);
		((Transform)main_transform).position = mousePosition;
	}

	private void checkDrag()
	{
		checkDragBegin();
		checkDragEnd();
	}

	private void checkDragBegin()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		if (Config.isDraggingItem() || _container.is_anything_dragging || !can_be_dragged)
		{
			return;
		}
		if (_container.delay_before_drag && !isMouseOver())
		{
			_drag_initialized = false;
			return;
		}
		if (InputHelpers.GetAnyMouseButtonDown() && isMouseOver())
		{
			_mouse_button = InputHelpers.GetAnyMouseButtonDownIndex();
			_drag_started_at = Time.time;
			if (!_container.delay_before_drag)
			{
				_drag_started_at -= DragOrderContainer.drag_delay;
				_prev_mouse_position = Input.mousePosition;
			}
			_drag_initialized = true;
		}
		if (_drag_initialized)
		{
			if (!InputHelpers.GetMouseButton(_mouse_button))
			{
				_drag_initialized = false;
			}
			else if ((_container.delay_before_drag || shouldStartDrag(Vector2.op_Implicit(Input.mousePosition), Vector2.op_Implicit(_prev_mouse_position))) && !(Time.time - _drag_started_at < DragOrderContainer.drag_delay))
			{
				startDrag();
			}
		}
	}

	private void checkDragEnd()
	{
		if (InputHelpers.GetMouseButtonUp(_mouse_button) && _container.is_anything_dragging)
		{
			_drag_initialized = false;
			endDrag();
		}
	}

	public void OnEndDrag(PointerEventData pData)
	{
		if (_container.is_anything_dragging)
		{
			_drag_initialized = false;
			endDrag();
		}
	}

	private void startDrag()
	{
		_drag_started_at = Time.realtimeSinceStartup;
		_container.dragging_element = this;
		Config.setDraggingObject(this);
		_container.is_anything_dragging = true;
		((Behaviour)_container.grid_layout).enabled = false;
		((Behaviour)_container.layout_element).enabled = true;
		((Selectable)_button).interactable = false;
		if ((Object)(object)_container.scroll_rect != (Object)null)
		{
			((Behaviour)_container.scroll_rect).enabled = false;
		}
		_container.updateChildrenData();
		setOnTop();
	}

	public void stopDrag()
	{
		endDrag();
	}

	private void endDrag()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (Config.isDraggingObject(this))
		{
			((Selectable)_button).interactable = true;
			_mouse_button = -1;
			if ((Object)(object)_container.scroll_rect != (Object)null)
			{
				((Behaviour)_container.scroll_rect).enabled = true;
			}
			Vector3 childPositionInContainer = getChildPositionInContainer();
			current_destination = Vector2.op_Implicit(childPositionInContainer);
			is_target_reached = false;
			if (!((Object)(object)_container.dragging_element != (Object)(object)this))
			{
				((Behaviour)_container.layout_element).enabled = false;
				_drag_initialized = false;
				_container.dragging_element = null;
				Config.clearDraggingObject();
				_container.is_anything_dragging = false;
			}
		}
	}

	private Vector3 getChildPositionInContainer()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return _container.getChildPosition(order_index);
	}

	private bool isMouseOver()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(((Transform)_container.rect_transform).InverseTransformPoint(Input.mousePosition));
		Rect rect = getRect();
		return ((Rect)(ref rect)).Contains(val);
	}

	public Rect getRect()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		getGridValues(_container.grid_layout, out var pCellSize, out var pSpacing);
		Vector2 val = Vector2.op_Implicit(((Transform)main_transform).localPosition) - pCellSize * main_transform.pivot - pSpacing / 2f;
		Vector2 val2 = pCellSize + pSpacing;
		return new Rect(val, val2);
	}

	private void getGridValues(LayoutGroup pLayoutGroup, out Vector2 pCellSize, out Vector2 pSpacing)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		GridLayoutGroup val = (GridLayoutGroup)(object)((pLayoutGroup is GridLayoutGroup) ? pLayoutGroup : null);
		if (val == null)
		{
			if (pLayoutGroup is GridLayoutGroupExtended gridLayoutGroupExtended)
			{
				pCellSize = gridLayoutGroupExtended.cellSize;
				pSpacing = gridLayoutGroupExtended.spacing;
			}
			else
			{
				pCellSize = Vector2.zero;
				pSpacing = Vector2.zero;
			}
		}
		else
		{
			pCellSize = val.cellSize;
			pSpacing = val.spacing;
		}
	}

	private static bool shouldStartDrag(Vector2 pPressPos, Vector2 pCurrentPos)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		float num = EventSystem.current.pixelDragThreshold;
		Vector2 val = pPressPos - pCurrentPos;
		return ((Vector2)(ref val)).sqrMagnitude >= num * num;
	}

	private void OnDisable()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		DragOrderContainer container = _container;
		if (container != null && container.is_anything_dragging)
		{
			OnEndDrag(new PointerEventData(EventSystem.current));
		}
	}

	public void KillDrag()
	{
		OnDisable();
	}
}
