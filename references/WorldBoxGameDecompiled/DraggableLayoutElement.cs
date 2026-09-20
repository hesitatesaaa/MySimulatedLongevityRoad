using System;
using System.Collections.Generic;
using LayoutGroupExt;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableLayoutElement : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, ILayoutIgnorer, IDraggable
{
	public const float TOUCH_DELAY = 0.2f;

	[SerializeField]
	private bool _spawn_particles_on_drag = true;

	private RectTransform _rect;

	private CanvasGroup _canvas_group;

	private LayoutGroupExtended _parent_layout;

	private RectTransform _parent;

	private Rect _cached_parent_rect;

	private Vector3 _cached_parent_position;

	[SerializeField]
	private Transform _attach_parent;

	[SerializeField]
	private bool _touch_drag_delay;

	private DraggableLayoutElement _drag_object;

	private int _target_index = -1;

	private List<MonoBehaviour> _toggle_elements = new List<MonoBehaviour>(3);

	private static bool _any_dragging;

	private bool? _dragging_cache;

	[SerializeField]
	private bool _drag_only_over_parent = true;

	internal Action<DraggableLayoutElement> start_being_dragged;

	private float _drag_timer_started_at;

	public bool spawn_particles_on_drag => _spawn_particles_on_drag;

	public bool ignoreLayout { get; set; }

	private List<RectTransform> _siblings => _parent_layout.m_Children;

	private Vector2[] _sibling_positions => _parent_layout.m_Positions;

	private void Start()
	{
		_rect = ((Component)this).GetComponent<RectTransform>();
		_canvas_group = ((Component)this).GetComponent<CanvasGroup>();
		_parent = ((Component)((Component)this).transform.parent).GetComponent<RectTransform>();
		_parent_layout = ((Component)_parent).GetComponent<LayoutGroupExtended>();
		if (!((Object)(object)_parent_layout == (Object)null))
		{
			addToggleComponent<ScrollableButton>();
			addToggleComponent<Button>();
			addToggleComponent<TipButton>();
			if ((Object)(object)_attach_parent == (Object)null)
			{
				_attach_parent = World.world.drag_parent;
			}
		}
	}

	private void OnEnable()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		_cached_parent_position = new Vector3(-1000f, -1000f, -1000f);
		_target_index = -1;
	}

	public void KillDrag()
	{
		OnDisable();
	}

	private void OnDisable()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		if (_any_dragging && !((Object)(object)_drag_object == (Object)null))
		{
			OnEndDrag(new PointerEventData(EventSystem.current));
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
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!Config.isDraggingItem() && !isTouchDragDelayed() && !_any_dragging)
		{
			_any_dragging = true;
			_drag_object = Object.Instantiate<DraggableLayoutElement>(this, _attach_parent, true);
			((Component)_drag_object).transform.position = Vector2.op_Implicit(pEventData.position);
			_drag_object.ignoreLayout = true;
			_drag_object.start_being_dragged?.Invoke(this);
			_canvas_group.alpha = 0.2f;
			Config.setDraggingObject(_drag_object);
		}
	}

	public void OnDrag(PointerEventData pEventData)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!isTouchDragDelayed() && _any_dragging && Config.isDraggingObject(_drag_object))
		{
			((Component)_drag_object).transform.position = Vector2.op_Implicit(pEventData.position);
			if (isOverParent(pEventData.position))
			{
				findTarget();
			}
		}
	}

	public void OnEndDrag(PointerEventData pEventData)
	{
		ScrollRectExtended.SendMessageToAll("OnEndDrag", pEventData);
		if (_any_dragging && Config.isDraggingObject(_drag_object))
		{
			Config.clearDraggingObject();
			_any_dragging = false;
			_drag_timer_started_at = 0f;
			Object.Destroy((Object)(object)((Component)_drag_object).gameObject);
			_canvas_group.alpha = 1f;
		}
	}

	public void Update()
	{
		if (_dragging_cache != _any_dragging)
		{
			_dragging_cache = _any_dragging;
			_canvas_group.interactable = !_any_dragging;
			_canvas_group.blocksRaycasts = !_any_dragging;
			foreach (MonoBehaviour toggle_element in _toggle_elements)
			{
				if (toggle_element is Selectable)
				{
					((Selectable)((toggle_element is Selectable) ? toggle_element : null)).interactable = !_any_dragging;
				}
				else
				{
					((Behaviour)toggle_element).enabled = !_any_dragging;
				}
			}
		}
		moveToTarget();
	}

	internal void lockToParent(bool pLock = true)
	{
		_drag_only_over_parent = pLock;
	}

	internal void setDragParent(Transform pParent)
	{
		_attach_parent = pParent;
	}

	private void moveToTarget()
	{
		if (_target_index < 0)
		{
			return;
		}
		int num = _siblings.IndexOf(_rect);
		int num2 = _target_index;
		using ListPool<int> listPool = getNeighbours(num);
		if (!listPool.Contains(num2))
		{
			num2 = findClosestNeighbour(num2, listPool);
		}
		swapSiblings(num, num2);
		if (num2 == _target_index)
		{
			_target_index = -1;
		}
	}

	private void recalcParent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!(_cached_parent_position == ((Transform)_parent).position))
		{
			_cached_parent_position = ((Transform)_parent).position;
			_cached_parent_rect = _parent.GetWorldRect();
			Rect rect = _rect.rect;
			float num = ((Rect)(ref rect)).width * 10f;
			rect = _rect.rect;
			float num2 = ((Rect)(ref rect)).height * 10f;
			ref Rect cached_parent_rect = ref _cached_parent_rect;
			((Rect)(ref cached_parent_rect)).x = ((Rect)(ref cached_parent_rect)).x - num;
			ref Rect cached_parent_rect2 = ref _cached_parent_rect;
			((Rect)(ref cached_parent_rect2)).y = ((Rect)(ref cached_parent_rect2)).y - num2;
			ref Rect cached_parent_rect3 = ref _cached_parent_rect;
			((Rect)(ref cached_parent_rect3)).width = ((Rect)(ref cached_parent_rect3)).width + num * 2f;
			ref Rect cached_parent_rect4 = ref _cached_parent_rect;
			((Rect)(ref cached_parent_rect4)).height = ((Rect)(ref cached_parent_rect4)).height + num2 * 2f;
		}
	}

	private void findTarget()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_drag_object == (Object)null)
		{
			return;
		}
		Vector3 position = ((Component)_drag_object).transform.position;
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		int num3 = -1;
		int num4 = -1;
		for (int i = 0; i < _sibling_positions.Length; i++)
		{
			float num5 = Vector2.Distance(_sibling_positions[i], Vector2.op_Implicit(position));
			if ((Object)(object)_siblings[i] == (Object)(object)_rect)
			{
				num4 = i;
				num2 = num5;
			}
			if (num5 < num)
			{
				num = num5;
				num3 = i;
			}
		}
		if (num3 != num4 && !Mathf.Approximately(num2, num))
		{
			_target_index = num3;
		}
	}

	private bool isOverParent(Vector2 pPosition)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		recalcParent();
		if (((Rect)(ref _cached_parent_rect)).Contains(pPosition))
		{
			return true;
		}
		return false;
	}

	private void swapSiblings(int pStartIndex, int pTargetIndex)
	{
		if (pStartIndex < _siblings.Count && pTargetIndex < _siblings.Count)
		{
			int siblingIndex = ((Component)_siblings[pStartIndex]).transform.GetSiblingIndex();
			int siblingIndex2 = ((Component)_siblings[pTargetIndex]).transform.GetSiblingIndex();
			if (siblingIndex > siblingIndex2)
			{
				((Component)_siblings[pTargetIndex]).transform.SetSiblingIndex(siblingIndex);
				((Component)this).transform.SetSiblingIndex(siblingIndex2);
			}
			else
			{
				((Component)this).transform.SetSiblingIndex(siblingIndex2);
				((Component)_siblings[pTargetIndex]).transform.SetSiblingIndex(siblingIndex);
			}
			_siblings.Swap(pStartIndex, pTargetIndex);
		}
	}

	private int findClosestNeighbour(int pIndex, ListPool<int> pNeighbours)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		int result = pIndex;
		Vector2 val = _sibling_positions[pIndex];
		float num = float.MaxValue;
		foreach (ref int pNeighbour in pNeighbours)
		{
			int current = pNeighbour;
			float num2 = Vector2.Distance(_sibling_positions[current], val);
			if (num2 < num)
			{
				num = num2;
				result = current;
			}
		}
		return result;
	}

	private ListPool<int> getNeighbours(int pIndex)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		ListPool<int> listPool = new ListPool<int>(8);
		if (_sibling_positions.Length < 2)
		{
			return listPool;
		}
		Vector2 val = _sibling_positions[pIndex];
		float num = Vector2.Distance(_sibling_positions[0], _sibling_positions[1]) * 1.5f;
		for (int i = 0; i < _sibling_positions.Length; i++)
		{
			if (i != pIndex && Vector2.Distance(val, _sibling_positions[i]) <= num)
			{
				listPool.Add(i);
			}
		}
		return listPool;
	}

	private void addToggleComponent<T>() where T : MonoBehaviour
	{
		if (((Component)(object)this).HasComponent<T>())
		{
			_toggle_elements.Add((MonoBehaviour)(object)((Component)this).GetComponent<T>());
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
}
