using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragOrderContainer : MonoBehaviour
{
	public enum SnapAxis
	{
		Horizontal,
		Vertical,
		No
	}

	internal static float drag_delay = 0.25f;

	public MonoBehaviour scroll_rect;

	public SnapAxis snapping_axis = SnapAxis.No;

	public bool limit_moving;

	public bool delay_before_drag = true;

	public bool debug;

	public Action on_order_changed;

	internal DragOrderElement dragging_element;

	internal bool is_anything_dragging;

	internal RectTransform rect_transform;

	internal LayoutGroup grid_layout;

	internal LayoutElement layout_element;

	private List<DragOrderElement> _elements = new List<DragOrderElement>();

	private Dictionary<int, DragOrderElement> _elements_dict = new Dictionary<int, DragOrderElement>();

	private Dictionary<int, Vector2> _children_positions = new Dictionary<int, Vector2>();

	private Dictionary<int, Rect> _children_rects = new Dictionary<int, Rect>();

	private Transform _to_ignore_in_intersection;

	private int _previous_elements_count;

	private bool _marked_for_update;

	private int _marked_for_update_on_frame;

	private bool _initialized;

	private void Awake()
	{
		if ((Object)(object)scroll_rect == (Object)null)
		{
			scroll_rect = (MonoBehaviour)(object)((Component)this).GetComponentInParent<ScrollRectExtended>();
		}
		if ((Object)(object)scroll_rect == (Object)null)
		{
			scroll_rect = (MonoBehaviour)(object)((Component)this).GetComponentInParent<ScrollRect>();
		}
		rect_transform = ((Component)this).GetComponent<RectTransform>();
		grid_layout = ((Component)this).GetComponent<LayoutGroup>();
		layout_element = ((Component)this).gameObject.AddOrGetComponent<LayoutElement>();
		((Behaviour)layout_element).enabled = false;
	}

	private void markForUpdate()
	{
		_marked_for_update = true;
		_marked_for_update_on_frame = Time.frameCount;
	}

	private void OnApplicationFocus(bool pHasFocus)
	{
		if (!pHasFocus)
		{
			disable();
		}
	}

	private void OnEnable()
	{
		markForUpdate();
		ScrollWindow.addCallbackShow(onWindowClose);
		ScrollWindow.addCallbackHide(onWindowClose);
	}

	private void OnDisable()
	{
		disable();
		ScrollWindow.removeCallbackShow(onWindowClose);
		ScrollWindow.removeCallbackHide(onWindowClose);
	}

	private void onWindowClose(string pId)
	{
		disable();
	}

	private void disable()
	{
		((Behaviour)grid_layout).enabled = true;
		LayoutRebuilder.MarkLayoutForRebuild(rect_transform);
		if ((Object)(object)dragging_element != (Object)null)
		{
			dragging_element.stopDrag();
		}
		foreach (DragOrderElement element in _elements)
		{
			if (!element.is_target_reached)
			{
				element.is_target_reached = true;
				element.unsetOnTop();
			}
		}
	}

	private void Update()
	{
		if (_marked_for_update && _marked_for_update_on_frame != Time.frameCount)
		{
			_marked_for_update = false;
			updateChildrenData();
		}
		checkIntersections();
		updatePositions();
	}

	private void OnDrawGizmos()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (!debug)
		{
			return;
		}
		foreach (Rect value in _children_rects.Values)
		{
			Rect current = value;
			((Rect)(ref current)).min = Vector2.op_Implicit(((Transform)rect_transform).TransformPoint(Vector2.op_Implicit(((Rect)(ref current)).min)));
			((Rect)(ref current)).max = Vector2.op_Implicit(((Transform)rect_transform).TransformPoint(Vector2.op_Implicit(((Rect)(ref current)).max)));
			drawRect(current, Color.green);
		}
	}

	private void checkIntersections()
	{
		if (is_anything_dragging)
		{
			DragOrderElement intersectedWith = getIntersectedWith();
			if ((Object)(object)intersectedWith == (Object)null)
			{
				_to_ignore_in_intersection = null;
			}
			else if (!((Object)(object)intersectedWith.main_transform == (Object)(object)_to_ignore_in_intersection))
			{
				_to_ignore_in_intersection = (Transform)(object)intersectedWith.main_transform;
				switchElements(dragging_element, intersectedWith);
				on_order_changed?.Invoke();
			}
		}
	}

	private DragOrderElement getIntersectedWith()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		int order_index = dragging_element.order_index;
		Vector2 val = Vector2.op_Implicit(((Transform)dragging_element.main_transform).localPosition);
		RectTransform obj = rect_transform;
		Rect val2 = _children_rects[order_index];
		Debug.DrawLine(((Transform)obj).TransformPoint(Vector2.op_Implicit(((Rect)(ref val2)).center)), ((Transform)rect_transform).TransformPoint(Vector2.op_Implicit(val)));
		if (snapping_axis != SnapAxis.No)
		{
			int key = 0;
			int key2 = _elements.Count - 1;
			Rect val3 = _children_rects[key];
			Rect val4 = _children_rects[key2];
			if (snapping_axis == SnapAxis.Horizontal)
			{
				if (val.x <= ((Rect)(ref val3)).xMax)
				{
					return _elements_dict[key];
				}
				if (val.x >= ((Rect)(ref val4)).xMin)
				{
					return _elements_dict[key2];
				}
			}
			if (snapping_axis == SnapAxis.Vertical)
			{
				if (val.y >= ((Rect)(ref val3)).yMax)
				{
					return _elements_dict[key];
				}
				if (val.y <= ((Rect)(ref val4)).yMin)
				{
					return _elements_dict[key2];
				}
			}
		}
		for (int i = 0; i < _elements.Count; i++)
		{
			if (i != order_index)
			{
				Rect val5 = _children_rects[i];
				if (((Rect)(ref val5)).Contains(val))
				{
					return _elements_dict[i];
				}
			}
		}
		return null;
	}

	private void updatePositions()
	{
		if (((Behaviour)grid_layout).enabled)
		{
			return;
		}
		bool flag = false;
		foreach (DragOrderElement element in _elements)
		{
			if (!((Object)(object)element == (Object)(object)dragging_element))
			{
				element.updatePosition();
				if (!element.is_target_reached)
				{
					flag = true;
				}
			}
		}
		if (!flag && !is_anything_dragging)
		{
			((Behaviour)grid_layout).enabled = true;
		}
	}

	public void updateChildrenData()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		LayoutElement obj = layout_element;
		Rect rect = rect_transform.rect;
		obj.minHeight = ((Rect)(ref rect)).height;
		LayoutElement obj2 = layout_element;
		rect = rect_transform.rect;
		obj2.minWidth = ((Rect)(ref rect)).width;
		_elements.Clear();
		_elements_dict.Clear();
		_children_positions.Clear();
		_children_rects.Clear();
		DragOrderElement[] componentsInChildren = ((Component)rect_transform).GetComponentsInChildren<DragOrderElement>();
		int num = 0;
		DragOrderElement[] array = componentsInChildren;
		foreach (DragOrderElement dragOrderElement in array)
		{
			Vector2 val = ((!dragOrderElement.is_target_reached && _previous_elements_count == componentsInChildren.Length) ? dragOrderElement.current_destination : Vector2.op_Implicit(((Transform)dragOrderElement.main_transform).localPosition));
			dragOrderElement.order_index = num;
			_elements.Add(dragOrderElement);
			_elements_dict.Add(num, dragOrderElement);
			_children_positions.Add(num, val);
			Rect rect2 = dragOrderElement.getRect();
			_children_rects.Add(num, rect2);
			dragOrderElement.current_destination = val;
			dragOrderElement.unsetOnTop();
			num++;
		}
		_previous_elements_count = componentsInChildren.Length;
	}

	private void switchElements(DragOrderElement pFirst, DragOrderElement pSecond)
	{
		((Transform)pFirst.main_transform).SetSiblingIndex(((Transform)pSecond.main_transform).GetSiblingIndex());
		int order_index = pFirst.order_index;
		int order_index2 = pSecond.order_index;
		bool tIsAscending = order_index > order_index2;
		pFirst.order_index = order_index2;
		_elements.Sort((DragOrderElement e1, DragOrderElement e2) => sort(e1, e2, tIsAscending));
		int order_index3 = pFirst.order_index;
		foreach (DragOrderElement element in _elements)
		{
			if (!((Object)(object)element == (Object)(object)pFirst) && (!tIsAscending || element.order_index >= order_index3) && (tIsAscending || element.order_index <= order_index3) && element.order_index == order_index3)
			{
				element.order_index += (tIsAscending ? 1 : (-1));
				order_index3 = element.order_index;
			}
		}
		foreach (DragOrderElement element2 in _elements)
		{
			_elements_dict[element2.order_index] = element2;
		}
	}

	public Vector3 getChildPosition(int pIndex)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.op_Implicit(_children_positions[pIndex]);
	}

	private int sort(DragOrderElement pFirst, DragOrderElement pSecond, bool pIsAscending)
	{
		return pFirst.order_index.CompareTo(pSecond.order_index) * (pIsAscending ? 1 : (-1));
	}

	private static void drawRect(Rect pRect, Color pColor)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(((Rect)(ref pRect)).min);
		Vector3 val2 = Vector2.op_Implicit(((Rect)(ref pRect)).max);
		Debug.DrawLine(val, new Vector3(val.x, val2.y), pColor);
		Debug.DrawLine(new Vector3(val.x, val2.y), val2, pColor);
		Debug.DrawLine(val2, new Vector3(val2.x, val.y), pColor);
		Debug.DrawLine(val, new Vector3(val2.x, val.y), pColor);
	}
}
