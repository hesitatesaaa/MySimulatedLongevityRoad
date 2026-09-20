using System.Collections.Generic;
using UnityEngine;

public class HistoryHud : MonoBehaviour
{
	public static HistoryHud instance;

	[SerializeField]
	private GameObject _template_obj;

	private List<HistoryHudItem> _history_items = new List<HistoryHudItem>(10);

	private ObjectPoolGenericMono<HistoryHudItem> _parked_items;

	private Transform _content_group;

	private Transform _parked_group;

	private const int HISTORY_ITEM_SIZE = 15;

	private const int MAX_HISTORY_ITEMS = 10;

	private const float START_POSITION = 0f;

	private bool _recalc;

	private static bool _raycast_on = true;

	public bool raycastOn
	{
		get
		{
			if (MoveCamera.camera_drag_run)
			{
				return false;
			}
			if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
			{
				return false;
			}
			if (MapBox.controlsLocked())
			{
				return false;
			}
			if (MapBox.isControllingUnit())
			{
				return false;
			}
			if (World.world.isAnyPowerSelected())
			{
				return false;
			}
			return _raycast_on;
		}
		set
		{
			_raycast_on = value;
		}
	}

	private void Awake()
	{
		instance = this;
		_content_group = ((Component)this).transform.Find("Scroll View/Viewport/Content");
		_parked_group = ((Component)this).transform.Find("Scroll View/Viewport/Parked");
		_parked_items = new ObjectPoolGenericMono<HistoryHudItem>(_template_obj.GetComponent<HistoryHudItem>(), _parked_group);
	}

	private void OnEnable()
	{
		if (Config.game_loaded)
		{
			((Component)_content_group).gameObject.GetComponent<RectTransform>().SetTop(0f);
			((Component)_content_group).gameObject.GetComponent<RectTransform>().SetLeft(0f);
		}
	}

	private void OnDisable()
	{
		Clear();
	}

	private void Update()
	{
		checkEnabled();
		if (_recalc)
		{
			_recalc = false;
			recalcPositions();
		}
	}

	public static void disableRaycasts()
	{
		instance.raycastOn = false;
	}

	public static void enableRaycasts()
	{
		instance.raycastOn = true;
	}

	private float recalcPositions()
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		if (_history_items.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		if (_history_items.Count > 10)
		{
			num3 = _history_items.Count - 10;
		}
		for (int i = 0; i < _history_items.Count; i++)
		{
			if (num3 > 0)
			{
				if (_history_items[i].target_bottom != (float)(num3 * -15))
				{
					_history_items[i].moveToAndDestroy(num3 * -15);
				}
				num3--;
			}
			else
			{
				if (_history_items[i].isRemoving())
				{
					continue;
				}
				if (_history_items[i].target_bottom != num)
				{
					_history_items[i].moveTo(num);
				}
				num += 15f;
			}
			num2 = 0f - ((Component)_history_items[i]).GetComponent<RectTransform>().offsetMax.y;
		}
		if (num2 >= num)
		{
			return num2 + 15f;
		}
		return num;
	}

	private bool checkEnabled()
	{
		if (!PlayerConfig.optionBoolEnabled("history_log"))
		{
			if (((Component)this).gameObject.activeSelf)
			{
				((Component)this).gameObject.SetActive(false);
			}
			return false;
		}
		if (!((Component)this).gameObject.activeSelf)
		{
			((Component)this).gameObject.SetActive(true);
		}
		return true;
	}

	public void newHistory(WorldLogMessage pMessage)
	{
		if (checkEnabled())
		{
			newText(pMessage);
			((Component)this).gameObject.SetActive(true);
		}
	}

	public void makeInactive(HistoryHudItem historyItem)
	{
		_parked_items.resetParent(historyItem);
		_parked_items.release(historyItem);
		_history_items.Remove(historyItem);
		_recalc = true;
	}

	public void Clear()
	{
		for (int num = _history_items.Count - 1; num >= 0; num--)
		{
			makeInactive(_history_items[num]);
		}
	}

	private void newText(WorldLogMessage pMessage)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		HistoryHudItem next = _parked_items.getNext();
		((Component)next).transform.SetParent(_content_group);
		((Object)((Component)next).gameObject).name = "HistoryItem " + (_history_items.Count + 1);
		((Component)next).gameObject.SetActive(true);
		RectTransform component = ((Component)next).GetComponent<RectTransform>();
		((Transform)component).localScale = Vector3.one;
		((Transform)component).localPosition = Vector3.zero;
		component.SetLeft(0f);
		float num = recalcPositions();
		component.SetTop(num);
		component.sizeDelta = new Vector2(component.sizeDelta.x, 15f);
		next.target_bottom = num;
		next.setMessage(pMessage);
		_history_items.Add(next);
		_recalc = true;
	}
}
