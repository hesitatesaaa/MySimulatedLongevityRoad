using UnityEngine;
using UnityEngine.UI;
using ai.behaviours;

public class SelectedUnitTab : SelectedNano<Actor>
{
	[SerializeField]
	private Image _task_icon_left;

	[SerializeField]
	private Image _task_icon_right;

	[SerializeField]
	private Text _task_title;

	[SerializeField]
	private Sprite _no_task_icon;

	[SerializeField]
	private StatBar _bar_health;

	[SerializeField]
	private StatBar _bar_mana;

	[SerializeField]
	private StatBar _bar_stamina;

	[SerializeField]
	private StatBar _bar_hunger;

	[SerializeField]
	private StatBar _bar_happiness;

	[SerializeField]
	private HappinessBarIcon _bar_happiness_icon;

	[SerializeField]
	private UiUnitAvatarElement _avatar_element;

	[SerializeField]
	private UnitTextManager _avatar_text;

	[SerializeField]
	private SwitchStateButton _button_traits_editor;

	[SerializeField]
	private SwitchStateButton _button_equipment_editor;

	[SerializeField]
	private SwitchStateButton _button_mind;

	[SerializeField]
	private SwitchStateButton _button_genealogy;

	[SerializeField]
	private SwitchStateButton _button_plots_tab;

	[SerializeField]
	private Image _button_equipment_editor_icon;

	[SerializeField]
	private Sprite _button_equipment_editor_sprite_normal;

	[SerializeField]
	private Sprite _button_equipment_editor_sprite_firearm;

	[SerializeField]
	private ActorSelectedContainerStatus _container_status;

	[SerializeField]
	private ActorSelectedContainerEquipment _container_equipment;

	[SerializeField]
	private DragSnapElement _drag_snap_avatar;

	private bool _requested_buttons_update;

	private void Start()
	{
		SelectedUnit.subscribeClearEvent(clearLastObject);
	}

	private void OnEnable()
	{
		if (!((Object)(object)_drag_snap_avatar == (Object)null))
		{
			if (InputHelpers.mouseSupported)
			{
				((Behaviour)_drag_snap_avatar).enabled = true;
			}
			else
			{
				((Behaviour)_drag_snap_avatar).enabled = false;
			}
		}
	}

	private void LateUpdate()
	{
		if (_requested_buttons_update)
		{
			_requested_buttons_update = false;
			PowerTabController.instance.tab_selected_unit.findNeighbours(pCheckForActive: true);
		}
	}

	protected override void updateElementsOnChange(Actor pActor)
	{
		base.updateElementsOnChange(pActor);
		pActor.asset.unlock();
		updateButtons(pActor);
		showStatsGeneral(pActor);
		updateStatuses(pActor);
		updateEquipment(pActor);
	}

	protected override void updateElementsAlways(Actor pActor)
	{
		showTask(pActor);
		showStatBars(pActor);
		checkAvatar(pActor);
		setIconValue("i_age", pActor.getAge());
		base.updateElementsAlways(pActor);
	}

	protected override void checkAchievements(Actor pActor)
	{
		AchievementLibrary.checkUnitAchievements(pActor);
	}

	private void updateButtons(Actor pActor)
	{
		((Component)_button_equipment_editor).gameObject.SetActive(pActor.asset.can_edit_equipment);
		((Component)_button_mind).gameObject.SetActive(pActor.asset.inspect_mind);
		((Component)_button_traits_editor).gameObject.SetActive(pActor.asset.can_edit_traits);
		((Component)_button_genealogy).gameObject.SetActive(pActor.asset.inspect_genealogy);
		((Component)_button_plots_tab).gameObject.SetActive(pActor.isKing() || pActor.isCityLeader() || pActor.hasClan());
		_button_equipment_editor_icon.sprite = (pActor.isWeaponFirearm() ? _button_equipment_editor_sprite_firearm : _button_equipment_editor_sprite_normal);
		_requested_buttons_update = true;
	}

	private void updateStatuses(Actor pActor)
	{
		_container_status.update(pActor);
	}

	private void updateEquipment(Actor pActor)
	{
		_container_equipment.update(pActor);
	}

	protected override void showStatsGeneral(Actor pActor)
	{
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		setIconValue("i_renown", pActor.renown);
		setIconValue("i_level", pActor.level);
		setIconValue("i_experience", pActor.data.experience, pActor.getExpToLevelup());
		setIconValue("i_money", pActor.money);
		setIconValue("i_loot", pActor.loot);
		setIconValue("i_kills", pActor.data.kills);
		setIconValue("i_children", pActor.current_children_count);
		int num = (int)pActor.stats["damage"];
		int num2 = (int)((float)num * pActor.stats["damage_range"]);
		setIconValue("i_damage", num2, num, "", pFloat: false, "", '-');
		name_field.text = pActor.getName();
		((Graphic)name_field).color = pActor.kingdom.getColor().getColorText();
		if (pActor.asset.is_boat)
		{
			icon_right.sprite = pActor.asset.getSpriteIcon();
		}
		else if (pActor.isSexMale())
		{
			icon_right.sprite = SpriteTextureLoader.getSprite("ui/icons/IconMale");
		}
		else
		{
			icon_right.sprite = SpriteTextureLoader.getSprite("ui/icons/IconFemale");
		}
		Sprite sprite = ((!pActor.asset.is_boat || !pActor.hasCity()) ? pActor.asset.getSpriteIcon() : pActor.city.getSpriteIcon());
		icon_left.sprite = sprite;
	}

	private void checkAvatar(Actor pActor)
	{
		if (isNanoChanged(pActor) || _avatar_element.avatarLoader.actorStateChanged())
		{
			_avatar_element.show(pActor);
		}
		else
		{
			_avatar_element.updateTileSprite();
		}
	}

	private void showStatAvatar(Actor pActor)
	{
		_avatar_element.show(pActor);
	}

	private void showTask(Actor pActor)
	{
		BehaviourTaskActor task = pActor.ai.task;
		string taskText = pActor.getTaskText();
		Sprite sprite = ((task != null) ? task.getSprite() : _no_task_icon);
		_task_icon_left.sprite = sprite;
		_task_icon_right.sprite = sprite;
		_task_title.text = taskText;
	}

	private void showStatBars(Actor pActor)
	{
		float pVal = pActor.getHealth();
		float num = pActor.getMaxHealth();
		_bar_health.setBar(pVal, num, "/" + ((int)num).ToText(4), pReset: false, pFloat: false, pUpdateText: true, 0.25f);
		bool pShow = pActor.hasEmotions();
		_bar_happiness_icon.load(pActor);
		checkShowProgressBar(_bar_happiness, "%", pActor.getHappinessPercent(), 100f, pShow);
		bool pShow2 = pActor.needsFood();
		float pCurrentValue = (float)pActor.getNutrition() / (float)pActor.getMaxNutrition() * 100f;
		checkShowProgressBar(_bar_hunger, "%", pCurrentValue, 100f, pShow2);
		bool pShow3 = !pActor.asset.force_hide_stamina;
		int maxStamina = pActor.getMaxStamina();
		float pCurrentValue2 = Mathf.Clamp(pActor.getStamina(), 0, maxStamina);
		checkShowProgressBar(_bar_stamina, "/" + maxStamina.ToText(), pCurrentValue2, maxStamina, pShow3);
		bool pShow4 = !pActor.asset.force_hide_mana;
		int maxMana = pActor.getMaxMana();
		float pCurrentValue3 = Mathf.Clamp(pActor.getMana(), 0, maxMana);
		checkShowProgressBar(_bar_mana, "/" + maxMana.ToText(), pCurrentValue3, maxMana, pShow4);
	}

	protected override string getPowerTabAssetID()
	{
		return "selected_unit";
	}

	private void checkShowProgressBar(StatBar pBar, string pEnding, float pCurrentValue, float pMax, bool pShow)
	{
		((Component)pBar).gameObject.SetActive(pShow);
		if (pShow)
		{
			pBar.setBar(pCurrentValue, pMax, pEnding, pReset: false, pFloat: false, pUpdateText: true, 0.25f);
		}
	}

	public void avatarTouchScream()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (SelectedUnit.isSet())
		{
			nano_object.pokeFromAvatarUI();
			World.world.locatePosition(Vector2.op_Implicit(nano_object.current_position));
		}
		_avatar_text.spawnAvatarText();
		((IShakable)ToolbarButtons.instance).shake();
	}
}
