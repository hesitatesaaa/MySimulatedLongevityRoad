using System.Collections.Generic;
using UnityEngine.UI;

public class CityWindow : WindowMetaGeneric<City, CityData>, IBooksWindow
{
	public Image raceTopIcon1;

	public Image raceTopIcon2;

	public LocalizedText village_title;

	public override MetaType meta_type => MetaType.City;

	protected override City meta_object => SelectedMetas.selected_city;

	public List<long> getBooks()
	{
		return meta_object.getBooks();
	}

	protected override void showTopPartInformation()
	{
		base.showTopPartInformation();
		City city = meta_object;
		if (city != null)
		{
			raceTopIcon1.sprite = city.getSpriteIcon();
			raceTopIcon2.sprite = city.getSpriteIcon();
		}
	}

	public override void startShowingWindow()
	{
		base.startShowingWindow();
		AchievementLibrary.checkCityAchievements(meta_object);
	}

	private void tryShowPastRulers()
	{
		List<LeaderEntry> past_rulers = meta_object.data.past_rulers;
		if (past_rulers != null && past_rulers.Count > 1)
		{
			showStatRow("past_leaders", meta_object.data.past_rulers.Count, MetaType.None, -1L, "iconVillages", "past_rulers", getTooltipPastRulers);
		}
	}

	private TooltipData getTooltipPastRulers()
	{
		return new TooltipData
		{
			tip_name = "past_leaders",
			meta_type = MetaType.City,
			past_rulers = new ListPool<LeaderEntry>(meta_object.data.past_rulers)
		};
	}

	protected override bool onNameChange(string pInput)
	{
		if (!base.onNameChange(pInput))
		{
			return false;
		}
		foreach (Religion religion in World.world.religions)
		{
			if (!religion.isRekt() && religion.data.creator_city_id == meta_object.getID())
			{
				religion.data.creator_city_name = meta_object.data.name;
			}
		}
		foreach (Culture culture in World.world.cultures)
		{
			if (!culture.isRekt() && culture.data.creator_city_id == meta_object.getID())
			{
				culture.data.creator_city_name = meta_object.data.name;
			}
		}
		foreach (Clan clan in World.world.clans)
		{
			if (!clan.isRekt() && clan.data.founder_city_id == meta_object.getID())
			{
				clan.data.founder_city_name = meta_object.data.name;
			}
		}
		foreach (Language language in World.world.languages)
		{
			if (!language.isRekt() && language.data.creator_city_id == meta_object.getID())
			{
				language.data.creator_city_name = meta_object.data.name;
			}
		}
		foreach (Family family in World.world.families)
		{
			if (!family.isRekt() && family.data.founder_city_id == meta_object.getID())
			{
				family.data.founder_city_name = meta_object.data.name;
			}
		}
		foreach (Book book in World.world.books)
		{
			if (!book.isRekt() && book.data.author_city_id == meta_object.getID())
			{
				book.data.author_city_name = meta_object.data.name;
			}
		}
		return true;
	}

	internal override void showStatsRows()
	{
		City city = meta_object;
		if (city != null)
		{
			if (city.kingdom.isNeutral())
			{
				village_title.setKeyAndUpdate("village_dying");
			}
			else
			{
				village_title.setKeyAndUpdate("village");
			}
			tryShowPastNames();
			showStatRow("founded", city.getFoundedDate(), MetaType.None, -1L, "iconAge");
			tryToShowActor("founder", city.data.founder_id, city.data.founder_name, null, "actor_traits/iconStupid");
			tryShowPastRulers();
			tryToShowActor("village_statistics_leader", -1L, null, city.leader, "iconLeaders");
			if (city.hasLeader())
			{
				showStatRow("ruler_money", city.leader.money, "#43FF43", MetaType.None, -1L, pColorText: false, "iconMoney");
			}
			string pValue = city.kingdom.getTaxRateLocal().ToString("0%");
			showStatRow("tax", pValue, "#43FF43", MetaType.None, -1L, pColorText: false, "kingdom_traits/kingdom_trait_tax_rate_local_low");
			string pValue2 = city.kingdom.getTaxRateTribute().ToString("0%");
			showStatRow("tribute", pValue2, "#43FF43", MetaType.None, -1L, pColorText: false, "kingdom_traits/kingdom_trait_tax_rate_tribute_high");
			tryToShowActor("king", -1L, null, city.kingdom.king, "iconKings");
			tryToShowMetaSpecies("founder_species", city.getFounderSpecies()?.id);
		}
	}

	public override void showMetaRows()
	{
		City city = meta_object;
		if (city != null && !city.kingdom.isNeutral())
		{
			meta_rows_container.tryToShowMetaClan("clan", -1L, null, city.leader?.clan);
			meta_rows_container.tryToShowMetaKingdom("kingdom", -1L, null, city.kingdom);
			meta_rows_container.tryToShowMetaAlliance("alliance", -1L, null, city.kingdom.getAlliance());
			meta_rows_container.tryToShowMetaCulture("culture", -1L, null, city.culture);
			meta_rows_container.tryToShowMetaLanguage("language", -1L, null, city.language);
			meta_rows_container.tryToShowMetaReligion("religion", -1L, null, city.religion);
			meta_rows_container.tryToShowMetaSubspecies("main_subspecies", -1L, null, city.getMainSubspecies());
			meta_rows_container.tryToShowMetaArmy("army", -1L, null, city.army);
		}
	}

	public void clickTestItemProduction()
	{
		ItemCrafting.tryToCraftRandomWeapon(meta_object.units.GetRandom(), meta_object);
		scroll_window.tabs.showTab(scroll_window.tabs.getActiveTab());
	}

	public void clickTestClearItems()
	{
		meta_object.data.equipment.clearItems();
		scroll_window.tabs.showTab(scroll_window.tabs.getActiveTab());
	}

	public void clickTestNewBook()
	{
		if (meta_object.hasLeader() && meta_object.leader.hasCulture() && meta_object.leader.hasLanguage())
		{
			World.world.books.generateNewBook(meta_object.leader);
			meta_object.forceDoChecks();
			scroll_window.tabs.showTab(scroll_window.tabs.getActiveTab());
		}
	}
}
