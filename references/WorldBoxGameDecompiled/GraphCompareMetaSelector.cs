using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollableButton))]
public class GraphCompareMetaSelector : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IDraggable
{
	[SerializeField]
	private bool _spawn_particles_on_drag;

	private Vector3 _start_local_position;

	private Transform _start_parent;

	private ScrollableButton _scrollable_button;

	private readonly List<Graphic> _raycastables;

	private Vector2 _first_position;

	private bool _dragging;

	private readonly List<RectTransform> _dropzones;

	private GraphCompareWindow _window;

	public bool spawn_particles_on_drag => _spawn_particles_on_drag;

	private Transform _attach_parent => World.world.drag_parent;

	private void Awake()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		_scrollable_button = ((Component)this).GetComponent<ScrollableButton>();
		_start_parent = ((Component)this).transform.parent;
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener(new UnityAction(showTooltip));
	}

	private void showTooltip()
	{
		IBanner component = ((Component)this).GetComponent<IBanner>();
		if (!InputHelpers.mouseSupported && !Tooltip.isShowingFor(component))
		{
			component.showTooltip();
		}
	}

	public void addWindow(GraphCompareWindow pWindow)
	{
		_window = pWindow;
	}

	public void addDropzones(params RectTransform[] pDropzones)
	{
		_dropzones.Clear();
		_dropzones.AddRange(pDropzones);
	}

	public bool isBeingDragged()
	{
		return _dragging;
	}

	public void OnInitializePotentialDrag(PointerEventData pEventData)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		_dragging = false;
		_first_position = pEventData.position;
		_start_parent = ((Component)this).transform.parent;
		_start_local_position = ((Component)this).transform.localPosition;
	}

	public bool checkIfDragging(PointerEventData pEventData)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		if (_window.countNoosItems() < 5)
		{
			return true;
		}
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(float.MaxValue, 0f);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(float.MinValue, 0f);
		foreach (RectTransform dropzone in _dropzones)
		{
			Vector2 val3 = Vector2.op_Implicit(((Transform)dropzone).position);
			ref float x = ref val3.x;
			float num = x;
			Rect rect = dropzone.rect;
			x = num - ((Rect)(ref rect)).width * ((Transform)dropzone).lossyScale.x / 2f;
			ref float y = ref val3.y;
			float num2 = y;
			rect = dropzone.rect;
			y = num2 - ((Rect)(ref rect)).height * ((Transform)dropzone).lossyScale.y / 2f;
			Vector2 val4 = Vector2.op_Implicit(((Transform)dropzone).position);
			ref float x2 = ref val4.x;
			float num3 = x2;
			rect = dropzone.rect;
			x2 = num3 + ((Rect)(ref rect)).width * ((Transform)dropzone).lossyScale.x / 2f;
			ref float y2 = ref val4.y;
			float num4 = y2;
			rect = dropzone.rect;
			y2 = num4 - ((Rect)(ref rect)).height * ((Transform)dropzone).lossyScale.y / 2f;
			if (val3.x < val.x)
			{
				val = val3;
			}
			if (val4.x > val2.x)
			{
				val2 = val4;
			}
		}
		if (!Toolbox.isInTriangle(pEventData.position, _first_position, val, val2))
		{
			Vector2 val5 = pEventData.position - _first_position;
			if (Mathf.Abs(val5.x) > Mathf.Abs(val5.y))
			{
				return false;
			}
		}
		return true;
	}

	public void OnBeginDrag(PointerEventData pEventData)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (!Config.isDraggingItem() && !_dragging)
		{
			_dragging = checkIfDragging(pEventData);
			if (_dragging)
			{
				Config.setDraggingObject(this);
				((AbstractEventData)pEventData).Use();
				((Behaviour)_scrollable_button).enabled = false;
				GraphCompareMetaObject.disable_raycasts = true;
				((Component)this).transform.SetParent(_attach_parent);
				((Component)this).transform.position = Vector2.op_Implicit(pEventData.position);
				disableRaycast();
			}
		}
	}

	public void OnDrag(PointerEventData pEventData)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (_dragging && Config.isDraggingObject(this))
		{
			((AbstractEventData)pEventData).Use();
			((Component)this).transform.position = Vector2.op_Implicit(pEventData.position);
		}
	}

	public void OnEndDrag(PointerEventData pEventData)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		_scrollable_button.OnEndDrag(pEventData);
		if (_dragging && Config.isDraggingObject(this))
		{
			((AbstractEventData)pEventData).Use();
			((Component)this).transform.SetParent(_start_parent);
			((Component)this).transform.localPosition = _start_local_position;
			resetDrag();
		}
	}

	public void resetDrag()
	{
		if (!_dragging)
		{
			return;
		}
		Config.clearDraggingObject();
		_dragging = false;
		((Behaviour)_scrollable_button).enabled = true;
		GraphCompareMetaObject.disable_raycasts = false;
		foreach (Graphic raycastable in _raycastables)
		{
			raycastable.raycastTarget = true;
		}
	}

	private void disableRaycast()
	{
		_raycastables.Clear();
		Graphic[] componentsInChildren = ((Component)this).GetComponentsInChildren<Graphic>();
		foreach (Graphic val in componentsInChildren)
		{
			if (val.raycastTarget)
			{
				_raycastables.Add(val);
			}
		}
		foreach (Graphic raycastable in _raycastables)
		{
			raycastable.raycastTarget = false;
		}
	}

	private void OnDisable()
	{
		resetDrag();
	}

	public void KillDrag()
	{
		OnDisable();
	}

	public GraphCompareMetaSelector()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		_spawn_particles_on_drag = true;
		_raycastables = new List<Graphic>();
		_first_position = Vector2.zero;
		_dropzones = new List<RectTransform>();
		((MonoBehaviour)this)._002Ector();
	}
}
