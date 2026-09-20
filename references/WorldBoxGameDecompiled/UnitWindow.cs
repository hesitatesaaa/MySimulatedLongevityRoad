using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnitWindow : StatsWindow, ITraitWindow<ActorTrait, ActorTraitButton>, IAugmentationsWindow<ITraitsEditor<ActorTrait>>, IEquipmentWindow, IAugmentationsWindow<IEquipmentEditor>, IPlotsWindow, IAugmentationsWindow<IPlotsEditor>
{
	[SerializeField]
	private Image _mood_sprite;

	[SerializeField]
	private Image _mood_bg;

	public NameInput name_input;

	[SerializeField]
	private UiUnitAvatarElement _avatar_element;

	[SerializeField]
	private Image _icon_favorite;

	[SerializeField]
	private WindowMetaTab _button_trait_editor;

	[SerializeField]
	private WindowMetaTab _button_equipment_editor;

	[SerializeField]
	private WindowMetaTab _button_mind_tab;

	[SerializeField]
	private WindowMetaTab _button_genealogy_tab;

	[SerializeField]
	private WindowMetaTab _button_plots_tab;

	[SerializeField]
	private WindowMetaTab _button_possession;

	[SerializeField]
	private Image _equipment_editor_icon;

	[SerializeField]
	private Sprite _equipment_sprite_normal;

	[SerializeField]
	private Sprite _equipment_sprite_firearm;

	[SerializeField]
	private LocalizedText _main_tab_title;

	[SerializeField]
	private UnitTextManager _unit_text;

	private string _initial_name;

	public override MetaType meta_type => MetaType.Unit;

	internal Actor actor => SelectedUnit.unit;

	protected virtual bool onNameChange(string pInput)
	{
		if (string.IsNullOrWhiteSpace(pInput))
		{
			return false;
		}
		if (actor.isRekt())
		{
			return false;
		}
		string text = pInput.Trim();
		if (_initial_name == text)
		{
			return false;
		}
		actor.data.custom_name = true;
		actor.setName(text);
		_initial_name = text;
		name_input.SetOutline();
		foreach (City city in World.world.cities)
		{
			if (!city.isRekt())
			{
				city.updateRulers();
				if (city.data.founder_id == actor.getID())
				{
					city.data.founder_name = actor.data.name;
				}
			}
		}
		foreach (Army army in World.world.armies)
		{
			if (!army.isRekt())
			{
				army.updateCaptains();
			}
		}
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (!kingdom.isRekt())
			{
				kingdom.updateRulers();
			}
		}
		foreach (War war in World.world.wars)
		{
			if (!war.isRekt() && war.data.started_by_actor_id == actor.getID())
			{
				war.data.started_by_actor_name = actor.data.name;
			}
		}
		foreach (Alliance alliance in World.world.alliances)
		{
			if (!alliance.isRekt() && alliance.data.founder_actor_id == actor.getID())
			{
				alliance.data.founder_actor_name = actor.data.name;
			}
		}
		foreach (Religion religion in World.world.religions)
		{
			if (!religion.isRekt() && religion.data.creator_id == actor.getID())
			{
				religion.data.creator_name = actor.data.name;
			}
		}
		foreach (Culture culture in World.world.cultures)
		{
			if (!culture.isRekt() && culture.data.creator_id == actor.getID())
			{
				culture.data.creator_name = actor.data.name;
			}
		}
		foreach (Clan clan in World.world.clans)
		{
			if (!clan.isRekt())
			{
				if (clan.data.founder_actor_id == actor.getID())
				{
					clan.data.founder_actor_name = actor.data.name;
				}
				clan.updateChiefs();
			}
		}
		foreach (Language language in World.world.languages)
		{
			if (!language.isRekt() && language.data.creator_id == actor.getID())
			{
				language.data.creator_name = actor.data.name;
			}
		}
		foreach (Family family in World.world.families)
		{
			if (!family.isRekt())
			{
				if (family.data.main_founder_id_1 == actor.getID())
				{
					family.data.founder_actor_name_1 = actor.data.name;
				}
				if (family.data.main_founder_id_2 == actor.getID())
				{
					family.data.founder_actor_name_2 = actor.data.name;
				}
			}
		}
		foreach (Book book in World.world.books)
		{
			if (!book.isRekt() && book.data.author_id == actor.getID())
			{
				book.data.author_name = actor.data.name;
			}
		}
		foreach (Plot plot in World.world.plots)
		{
			if (!plot.isRekt() && plot.data.founder_id == actor.getID())
			{
				plot.data.founder_name = actor.data.name;
			}
		}
		foreach (Item item in World.world.items)
		{
			if (!item.isRekt() && item.data.creator_id == actor.getID())
			{
				item.data.by = actor.data.name;
			}
		}
		return true;
	}

	internal override bool checkCancelWindow()
	{
		if (actor.isRekt())
		{
			return true;
		}
		if (!actor.hasHealth())
		{
			return true;
		}
		return base.checkCancelWindow();
	}

	private void clear()
	{
		_button_trait_editor.toggleActive(pState: false);
		_button_equipment_editor.toggleActive(pState: false);
		_button_plots_tab.toggleActive(pState: false);
		_button_mind_tab.toggleActive(pState: false);
		_button_genealogy_tab.toggleActive(pState: false);
		_button_possession.toggleActive(pState: false);
		((Component)((Component)_icon_favorite).transform.parent).gameObject.SetActive(false);
		((Component)_mood_bg).gameObject.SetActive(false);
		name_input.setText("");
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		showInfo();
		if (!actor.asset.isAvailable() && !actor.asset.unlocked_with_achievement)
		{
			actor.asset.unlock();
		}
		AchievementLibrary.checkUnitAchievements(actor);
	}

	public void showInfo()
	{
		clear();
		loadNameInput();
		showMainInfo();
		checkShowMain();
	}

	private void checkShowMain()
	{
		WindowMetaTab activeTab = base.tabs.getActiveTab();
		if (((Object)(object)activeTab == (Object)(object)_button_trait_editor && !isTraitsEditorAllowed()) || ((Object)(object)activeTab == (Object)(object)_button_equipment_editor && !isEquipmentEditorAllowed()) || ((Object)(object)activeTab == (Object)(object)_button_mind_tab && !isMindTabAllowed()) || ((Object)(object)activeTab == (Object)(object)_button_genealogy_tab && !isGenealogyTabAllowed()) || ((Object)(object)activeTab == (Object)(object)_button_plots_tab && !isPlotsTabAllowed()))
		{
			showTab(base.tabs.tab_default);
		}
	}

	internal override void showStatsRows()
	{
		tryShowPastNames();
		showStatRow("birthday", actor.getBirthday(), MetaType.None, -1L);
		showStatRow("task", actor.getTaskText(), MetaType.None, -1L);
		if (actor.asset.inspect_generation)
		{
			showStatRow("generation", actor.data.generation, MetaType.None, -1L);
		}
		if (actor.data.loot > 0)
		{
			showStatRow("loot", actor.data.loot, "#43FF43", MetaType.None, -1L, pColorText: false, "iconLoot");
		}
		if (actor.data.money > 0)
		{
			showStatRow("money", actor.data.money, "#43FF43", MetaType.None, -1L, pColorText: false, "iconMoney");
		}
		if (actor.inventory.hasResources())
		{
			showStatRow("resources", actor.inventory.getRandomResourceID().Localize(), MetaType.None, -1L, null, "carrying_resources", getTooltipCarryingResources);
		}
		if (actor.hasBestFriend())
		{
			Actor bestFriend = actor.getBestFriend();
			if (!bestFriend.isRekt())
			{
				string pIconPath = "iconMale";
				if (bestFriend.isSexFemale())
				{
					pIconPath = "iconFemale";
				}
				tryToShowActor("best_friend", -1L, null, bestFriend, pIconPath);
			}
		}
		if (actor.hasLover())
		{
			Actor lover = actor.lover;
			if (!actor.lover.isRekt())
			{
				string pIconPath2 = "iconMale";
				if (lover.isSexFemale())
				{
					pIconPath2 = "iconFemale";
				}
				tryToShowActor("lover", -1L, null, lover, pIconPath2);
			}
		}
		if (actor.hasFavoriteFood())
		{
			showStatRow("creature_statistics_favorite_food", actor.favorite_food_asset.getTranslatedName(), MetaType.None, -1L);
		}
		if (actor.isSapient() && actor.s_personality != null)
		{
			showStatRow("creature_statistics_personality", actor.s_personality.getTranslatedName(), MetaType.None, -1L);
		}
		if (actor.asset.is_boat)
		{
			Boat simpleComponent = actor.getSimpleComponent<Boat>();
			string pTooltipId;
			TooltipDataGetter pTooltipData;
			if (!simpleComponent.hasPassengers())
			{
				pTooltipId = null;
				pTooltipData = null;
			}
			else
			{
				pTooltipId = "passengers";
				pTooltipData = getTooltipPassengers;
			}
			showStatRow("passengers", simpleComponent.countPassengers(), MetaType.None, -1L, null, pTooltipId, pTooltipData);
		}
		int massKG = actor.getMassKG();
		string pTooltipId2;
		TooltipDataGetter pTooltipData2;
		if (actor.asset.resources_given == null)
		{
			pTooltipId2 = null;
			pTooltipData2 = null;
		}
		else
		{
			pTooltipId2 = "mass";
			pTooltipData2 = getTooltipMass;
		}
		showStatRow("mass", $"{massKG} kg", MetaType.None, -1L, null, pTooltipId2, pTooltipData2);
		if (actor.asset.inspect_show_species)
		{
			tryToShowMetaSpecies("species", actor.asset.id);
		}
	}

	public override void showMetaRows()
	{
		if (actor.asset.inspect_home)
		{
			meta_rows_container.tryToShowMetaCity("creature_statistics_home_village", -1L, null, actor.city);
		}
		if (actor.hasKingdom() && actor.isKingdomCiv())
		{
			meta_rows_container.tryToShowMetaKingdom("kingdom", -1L, null, actor.kingdom);
			if (actor.kingdom.hasAlliance())
			{
				meta_rows_container.tryToShowMetaAlliance("alliance", -1L, null, actor.kingdom.getAlliance());
			}
		}
		if (actor.hasClan())
		{
			meta_rows_container.tryToShowMetaClan("clan", -1L, null, actor.clan);
		}
		if (actor.hasLanguage())
		{
			meta_rows_container.tryToShowMetaLanguage("language", -1L, null, actor.language);
		}
		if (actor.hasReligion())
		{
			meta_rows_container.tryToShowMetaReligion("religion", -1L, null, actor.religion);
		}
		if (actor.hasFamily())
		{
			meta_rows_container.tryToShowMetaFamily("family", -1L, null, actor.family);
		}
		if (actor.hasCulture())
		{
			meta_rows_container.tryToShowMetaCulture("culture", -1L, null, actor.culture);
		}
		if (actor.asset.inspect_show_species && actor.hasSubspecies())
		{
			meta_rows_container.tryToShowMetaSubspecies("subspecies", -1L, null, actor.subspecies);
		}
	}

	public TooltipData getTooltipMass()
	{
		return new TooltipData
		{
			tip_name = "mass",
			actor = actor
		};
	}

	private TooltipData getTooltipPassengers()
	{
		return new TooltipData
		{
			actor = actor
		};
	}

	public TooltipData getTooltipCarryingResources()
	{
		return new TooltipData
		{
			tip_name = "carrying",
			actor = actor
		};
	}

	private void loadNameInput()
	{
		if (!actor.isRekt())
		{
			((UnityEvent<string>)(object)name_input.inputField.onEndEdit).AddListener((UnityAction<string>)delegate(string pString)
			{
				onNameChange(pString);
			});
			string text = (_initial_name = actor.getName().Trim());
			name_input.setText(text);
			if (actor.data.custom_name)
			{
				name_input.SetOutline();
			}
		}
	}

	public void showMainInfo()
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (!actor.isRekt())
		{
			checkMainTabTitle();
			if (isTraitsEditorAllowed())
			{
				_button_trait_editor.toggleActive(pState: true);
			}
			if (isEquipmentEditorAllowed())
			{
				_button_equipment_editor.toggleActive(pState: true);
			}
			if (isPlotsTabAllowed())
			{
				_button_plots_tab.toggleActive(pState: true);
			}
			if (isMindTabAllowed())
			{
				_button_mind_tab.toggleActive(pState: true);
			}
			if (isGenealogyTabAllowed())
			{
				_button_genealogy_tab.toggleActive(pState: true);
			}
			if (actor.canBePossessed())
			{
				_button_possession.toggleActive(pState: true);
			}
			if (actor.isKingdomCiv())
			{
				((Graphic)name_input.textField).color = actor.kingdom.getColor().getColorText();
			}
			else
			{
				((Graphic)name_input.textField).color = Toolbox.color_log_neutral;
			}
			_avatar_element.show(actor);
			if (actor.isSapient())
			{
				((Component)_mood_bg).gameObject.SetActive(false);
			}
			if (actor.asset.can_be_favorited)
			{
				((Component)((Component)_icon_favorite).transform.parent).gameObject.SetActive(true);
			}
			setIconValue("i_age", actor.getAge());
			updateFavoriteIconFor(actor);
			checkEquipmentTabIcon();
		}
	}

	private void checkMainTabTitle()
	{
		ActorAsset asset = actor.asset;
		asset.getSpriteIcon();
		if (asset.is_boat)
		{
			_main_tab_title.setKeyAndUpdate("boat");
		}
		else
		{
			_main_tab_title.setKeyAndUpdate(asset.getLocaleID());
		}
	}

	public void reloadBanner()
	{
		_avatar_element.show(actor);
	}

	private bool isTraitsEditorAllowed()
	{
		return actor.asset.can_edit_traits;
	}

	private bool isEquipmentEditorAllowed()
	{
		return actor.canEditEquipment();
	}

	private bool isMindTabAllowed()
	{
		return actor.asset.inspect_mind;
	}

	private bool isGenealogyTabAllowed()
	{
		return actor.asset.inspect_genealogy;
	}

	private bool isPlotsTabAllowed()
	{
		if (!actor.isCityLeader() && !actor.isKing())
		{
			return actor.hasClan();
		}
		return true;
	}

	public void locateActorHouse()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (actor.getHomeBuilding() != null)
		{
			World.world.locatePosition(actor.getHomeBuilding().current_tile.posV3);
		}
	}

	private void updateFavoriteIconFor(Actor pUnit)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (pUnit.isFavorite())
		{
			((Graphic)_icon_favorite).color = ColorStyleLibrary.m.favorite_selected;
		}
		else
		{
			((Graphic)_icon_favorite).color = ColorStyleLibrary.m.favorite_not_selected;
		}
	}

	public void pressFavorite()
	{
		if (!actor.isRekt())
		{
			actor.switchFavorite();
			updateFavoriteIconFor(actor);
			refreshMetaList();
			SpriteSwitcher.checkAllStates();
			if (actor.data.favorite)
			{
				WorldTip.showNowTop("tip_favorite_icon");
			}
		}
	}

	private void OnDisable()
	{
		name_input.inputField.DeactivateInputField();
	}

	protected void setIconValue(string pName, float pMainVal, float? pMax = null, string pColor = "", bool pFloat = false, string pEnding = "", char pSeparator = '/')
	{
		Transform val = ((Component)this).transform.FindRecursive(pName);
		if (!((Object)(object)val == (Object)null))
		{
			StatsIcon component = ((Component)val).GetComponent<StatsIcon>();
			((Component)component).gameObject.SetActive(true);
			component.setValue(pMainVal, pMax, pColor, pFloat, pEnding, pSeparator);
		}
	}

	public void clickOpenAssetDebug()
	{
		if (!actor.isRekt())
		{
			BaseDebugAssetElement<ActorAsset>.selected_asset = actor.asset;
			ScrollWindow.showWindow("actor_asset");
		}
	}

	public void clickGetLLMPrompt()
	{
		if (!actor.isRekt())
		{
			GUIUtility.systemCopyBuffer = GenerateLLMPrompt.getText(actor);
		}
	}

	public void checkEquipmentTabIcon()
	{
		if (actor.hasEquipment())
		{
			_equipment_editor_icon.sprite = (actor.isWeaponFirearm() ? _equipment_sprite_firearm : _equipment_sprite_normal);
		}
	}

	public void avatarTouchScream()
	{
		if (!actor.isRekt())
		{
			actor.pokeFromAvatarUI();
		}
		_unit_text.spawnAvatarText();
		((IShakable)scroll_window).shake();
		ScrollWindow.randomDropHoveringIcon(1, 2);
		reloadBanner();
		base.tabs.reloadActiveTab();
	}

	internal void tryShowPastNames()
	{
		List<NameEntry> past_names = actor.data.past_names;
		if (past_names != null && past_names.Count > 0)
		{
			showStatRow("past_names", actor.data.past_names?.Count ?? 1, MetaType.None, -1L, "iconVillages", "past_names", getTooltipPastNames);
		}
	}

	private TooltipData getTooltipPastNames()
	{
		return new TooltipData
		{
			tip_name = "past_names",
			past_names = new ListPool<NameEntry>(actor.data.past_names),
			meta_type = MetaType.Unit
		};
	}

	T IAugmentationsWindow<ITraitsEditor<ActorTrait>>.GetComponentInChildren<T>(bool includeInactive)
	{
		return ((Component)this).GetComponentInChildren<T>(includeInactive);
	}

	T IAugmentationsWindow<IEquipmentEditor>.GetComponentInChildren<T>(bool includeInactive)
	{
		return ((Component)this).GetComponentInChildren<T>(includeInactive);
	}

	T IAugmentationsWindow<IPlotsEditor>.GetComponentInChildren<T>(bool includeInactive)
	{
		return ((Component)this).GetComponentInChildren<T>(includeInactive);
	}
}
