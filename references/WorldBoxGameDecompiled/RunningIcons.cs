using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RunningIcons : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public enum Direction
	{
		Left,
		Right
	}

	[SerializeField]
	private Direction _direction;

	[SerializeField]
	private float speed;

	private IconGetter _get_next_icon;

	private RunningIconCallback _next_item_action;

	private RunningIconCallback _prev_item_action;

	private float _first_position_x = float.MaxValue;

	private float _last_position_x = float.MinValue;

	private int _last_index;

	private List<RunningIcon> _icons = new List<RunningIcon>();

	private bool _state;

	private bool _initialized;

	private float _step;

	public void addIcon(RunningIcon pIcon)
	{
		_icons.Add(pIcon);
	}

	public void init(RunningIconCallback pNextItemAction, RunningIconCallback pPrevItemAction)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		_initialized = true;
		_next_item_action = pNextItemAction;
		_prev_item_action = pPrevItemAction;
		foreach (Transform item in ((Component)this).transform)
		{
			Transform val = item;
			if (_first_position_x > val.localPosition.x)
			{
				_first_position_x = val.localPosition.x;
			}
			if (_last_position_x < val.localPosition.x)
			{
				_last_position_x = val.localPosition.x;
			}
		}
		float num = float.MaxValue;
		foreach (Transform item2 in ((Component)this).transform)
		{
			Transform val2 = item2;
			if (val2.localPosition.x != _first_position_x && num > val2.localPosition.x)
			{
				num = val2.localPosition.x;
			}
		}
		_step = num - _first_position_x;
		_last_position_x += _step;
		_last_index = ((Component)this).transform.childCount - 1;
		toggle(pState: true);
	}

	private void OnEnable()
	{
		reset();
	}

	private void reset()
	{
		if (_icons == null)
		{
			return;
		}
		foreach (RunningIcon icon in _icons)
		{
			_next_item_action(((Component)icon).transform);
		}
	}

	private void Update()
	{
		if (_initialized && _state)
		{
			moveBy(speed, _direction);
		}
	}

	public void moveBy(float pSpeed, Direction pDirection, int pCounter = 0)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = _last_index;
		float num3 = _step;
		if (pDirection == Direction.Left)
		{
			num = _last_index;
			num2 = 0;
			pSpeed *= -1f;
			num3 *= -1f;
		}
		foreach (Transform item in ((Component)this).transform)
		{
			Vector3 localPosition = item.localPosition;
			localPosition.x += pSpeed;
			item.localPosition = localPosition;
		}
		while (true)
		{
			Transform child = ((Component)this).transform.GetChild(num2);
			Vector3 localPosition2 = child.localPosition;
			if (endReached(localPosition2.x, pDirection))
			{
				Transform child2 = ((Component)this).transform.GetChild(num);
				localPosition2.x = child2.localPosition.x - num3;
				child.localPosition = localPosition2;
				child.SetSiblingIndex(num);
				if (pDirection == Direction.Left)
				{
					_prev_item_action(child);
				}
				else
				{
					_next_item_action(child);
				}
				continue;
			}
			break;
		}
	}

	public void toggle(bool pState)
	{
		_state = pState;
	}

	public bool getState()
	{
		return _state;
	}

	private bool endReached(float pPosition, Direction pDirection)
	{
		if (pDirection == Direction.Right)
		{
			return pPosition >= _last_position_x;
		}
		return pPosition <= _first_position_x;
	}

	public void OnSelect(BaseEventData pEventData)
	{
		if (!InputHelpers.mouseSupported)
		{
			toggle(pState: false);
		}
	}

	public void OnDeselect(BaseEventData pEventData)
	{
		if (!InputHelpers.mouseSupported)
		{
			toggle(pState: true);
		}
	}
}
