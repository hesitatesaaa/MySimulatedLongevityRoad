using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectedMultipleUnitsTab : SelectedNano<Actor>, ISelectedMetaWithUnit
{
	private const int MAX_UNITS_PER_FOLD = 21;

	private const int MAX_UNITS_TOTAL = 100;

	[SerializeField]
	private SelectedMetaUnitElement _unit_element;

	[SerializeField]
	private GameObject _unit_element_separator;

	[SerializeField]
	private ActorSelectedContainerStatus _container_status;

	[SerializeField]
	private ActorSelectedContainerEquipment _container_equipment;

	[SerializeField]
	private RectTransform _avatars_container;

	[SerializeField]
	private UiUnitAvatarElement _avatar_prefab;

	[SerializeField]
	private UnfoldButton _unfolder;

	[SerializeField]
	private Image _unfolder_background;

	[SerializeField]
	private Sprite _unfolder_active;

	[SerializeField]
	private Sprite _unfolder_inactive;

	private ObjectPoolGenericMono<UiUnitAvatarElement> _pool_avatars;

	private int _last_selection_version;

	private List<UiUnitAvatarElement> _showing_avatars = new List<UiUnitAvatarElement>();

	private List<int> _stats_version = new List<int>();

	private int _offset;

	protected override Actor nano_object => SelectedUnit.unit;

	public SelectedMetaUnitElement unit_element => _unit_element;

	public GameObject unit_element_separator => _unit_element_separator;

	private ISelectedMetaWithUnit as_meta_with_unit => this;

	public int last_dirty_stats_unit { get; set; }

	public Actor last_unit { get; set; }

	public string unit_title_locale_key => null;

	public bool hasUnit()
	{
		return SelectedUnit.isSet();
	}

	public Actor getUnit()
	{
		return SelectedUnit.unit;
	}

	protected override void Awake()
	{
		base.Awake();
		_pool_avatars = new ObjectPoolGenericMono<UiUnitAvatarElement>(_avatar_prefab, (Transform)(object)_avatars_container);
		_unfolder.setCallback(delegate
		{
			showAvatars(getOffset(), getNextAmount());
		});
	}

	private void Start()
	{
		SelectedUnit.subscribeClearEvent(clearLastObject);
	}

	protected override void updateElementsAlways(Actor pNano)
	{
		base.updateElementsAlways(pNano);
		bool num = as_meta_with_unit.checkUnitElement();
		if (hasUnit())
		{
			_unit_element.updateBarAndTask(getUnit());
		}
		if (num)
		{
			updateAvatars();
			return;
		}
		List<Actor> allSelectedList = SelectedUnit.getAllSelectedList();
		using ListPool<int> listPool = new ListPool<int>();
		for (int i = 0; i < _showing_avatars.Count; i++)
		{
			UiUnitAvatarElement uiUnitAvatarElement = _showing_avatars[i];
			if (i > allSelectedList.Count - 1)
			{
				listPool.Add(i);
				continue;
			}
			Actor actor = allSelectedList[i];
			int num2 = _stats_version[i];
			int statsDirtyVersion = actor.getStatsDirtyVersion();
			if (!actor.isRekt())
			{
				if (statsDirtyVersion != num2 || actor != uiUnitAvatarElement.getActor() || uiUnitAvatarElement.avatarLoader.actorStateChanged())
				{
					_stats_version[i] = statsDirtyVersion;
					uiUnitAvatarElement.load(actor);
				}
				else
				{
					uiUnitAvatarElement.updateTileSprite();
				}
			}
		}
		listPool.Sort();
		listPool.Reverse();
		for (int j = 0; j < listPool.Count; j++)
		{
			int index = listPool[j];
			UiUnitAvatarElement pElement = _showing_avatars[index];
			_showing_avatars.RemoveAt(index);
			_stats_version.RemoveAt(index);
			_pool_avatars.release(pElement);
		}
		updateUnfolderButton();
		if (listPool.Count > 0)
		{
			recalcTabSize();
		}
	}

	protected override void updateElementsOnChange(Actor pNano)
	{
		base.updateElementsOnChange(pNano);
		updateAvatars();
		updateStatuses(pNano);
		updateEquipment(pNano);
	}

	private void updateStatuses(Actor pActor)
	{
		_container_status.update(pActor);
	}

	private void updateEquipment(Actor pActor)
	{
		_container_equipment.update(pActor);
	}

	private void updateAvatars()
	{
		int selectionVersion = SelectedUnit.getSelectionVersion();
		if (selectionVersion != _last_selection_version)
		{
			_last_selection_version = selectionVersion;
			if (_offset == 0)
			{
				clear();
				showAvatars(getOffset(), getNextAmount());
			}
		}
	}

	private void showAvatars(int pOffset, int pAmount)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		using ListPool<Actor> listPool = new ListPool<Actor>(SelectedUnit.getAllSelected());
		listPool.Remove(SelectedUnit.unit);
		Button val = default(Button);
		Button val2 = default(Button);
		for (int i = pOffset; i < pOffset + pAmount; i++)
		{
			Actor actor = listPool[i];
			UiUnitAvatarElement tAvatar = _pool_avatars.getNext();
			if (!((Component)tAvatar).TryGetComponent<Button>(ref val))
			{
				((Component)(object)tAvatar).AddComponent<Button>();
			}
			UnitAvatarLoader avatarLoader = tAvatar.avatarLoader;
			tAvatar.load(actor);
			if (!((Component)avatarLoader).TryGetComponent<Button>(ref val2))
			{
				val2 = ((Component)(object)avatarLoader).AddComponent<Button>();
				((UnityEventBase)val2.onClick).RemoveAllListeners();
				((UnityEvent)val2.onClick).AddListener((UnityAction)delegate
				{
					Actor actor2 = tAvatar.getActor();
					tAvatar.show(SelectedUnit.unit);
					SelectedUnit.makeMainSelected(actor2);
					int index = _showing_avatars.IndexOf(tAvatar);
					_stats_version[index] = SelectedUnit.unit.getStatsDirtyVersion();
					showWorldTip(actor2);
					PowerTabController.instance.resetToStartScrollPosition();
				});
			}
			CanvasGroup component = ((Component)avatarLoader).GetComponent<CanvasGroup>();
			component.interactable = true;
			component.blocksRaycasts = true;
			_showing_avatars.Add(tAvatar);
			_stats_version.Add(actor.getStatsDirtyVersion());
		}
		_offset += pAmount;
		updateUnfolderButton();
		recalcTabSize();
	}

	private void updateUnfolderButton()
	{
		int num = SelectedUnit.countSelected() - 1 - _offset;
		if (num > 0)
		{
			((Component)_unfolder).transform.SetSiblingIndex(((Transform)_avatars_container).childCount - 1);
			((Component)_unfolder).gameObject.SetActive(true);
			_unfolder.setText($"+{num}");
			bool flag = _offset >= 100;
			((Selectable)_unfolder.getButton()).interactable = !flag;
			if (flag)
			{
				_unfolder_background.sprite = _unfolder_inactive;
			}
			else
			{
				_unfolder_background.sprite = _unfolder_active;
			}
		}
		else
		{
			((Component)_unfolder).gameObject.SetActive(false);
		}
	}

	protected override void showStatsGeneral(Actor pMeta)
	{
		base.showStatsGeneral(pMeta);
		if (hasUnit())
		{
			Actor unit = getUnit();
			_unit_element.showStats(unit);
		}
	}

	public void avatarTouchScream()
	{
		as_meta_with_unit.avatarTouch();
	}

	protected override void clearLastObject()
	{
		base.clearLastObject();
		as_meta_with_unit.clearLastUnit();
		_offset = 0;
	}

	private int getNextAmount()
	{
		int num = SelectedUnit.countSelected() - 1 - _offset;
		return Mathf.Min(21, num);
	}

	private int getOffset()
	{
		return _offset;
	}

	private void showWorldTip(Actor pActor)
	{
		string text = LocalizedTextManager.getText("now_looking_at");
		string color_text = pActor.getColor().color_text;
		string newValue = pActor.name.ColorHex(color_text);
		text = text.Replace("$name$", newValue);
		WorldTip.instance.showToolbarText(text);
	}

	private void clear()
	{
		_offset = 0;
		_pool_avatars.clear();
		_showing_avatars.Clear();
		_stats_version.Clear();
	}
}
