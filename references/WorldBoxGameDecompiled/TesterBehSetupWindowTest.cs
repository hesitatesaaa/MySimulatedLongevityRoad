using System.Collections.Generic;
using ai.behaviours;

public class TesterBehSetupWindowTest : BehaviourActionTester
{
	private int currentWindow;

	private City city;

	private Clan clan;

	private Plot plot;

	private Alliance alliance;

	private War war;

	private Kingdom kingdom;

	private Culture culture;

	private Actor unit;

	public override BehResult execute(AutoTesterBot pObject)
	{
		List<WindowAsset> testableWindows = AssetManager.window_library.getTestableWindows();
		if (currentWindow >= testableWindows.Count)
		{
			currentWindow = 0;
		}
		string text = testableWindows[currentWindow++].id;
		pickTheBest();
		SelectedMetas.selected_city = city;
		SelectedMetas.selected_clan = clan;
		SelectedMetas.selected_plot = plot;
		SelectedMetas.selected_alliance = alliance;
		SelectedMetas.selected_war = war;
		SelectedMetas.selected_kingdom = kingdom;
		SelectedMetas.selected_culture = culture;
		if (!unit.isRekt())
		{
			SelectedUnit.select(unit);
		}
		else
		{
			SelectedUnit.clear();
		}
		Config.current_brush = Brush.getRandom();
		Config.power_to_unlock = GodPower.premium_powers.Find((GodPower tPower) => tPower.id == "cybercore");
		Config.selected_trait_editor = PowerLibrary.traits_delta_rain_edit.id;
		SaveManager.setCurrentSlot(1);
		if (text.Contains("workshop"))
		{
			SaveManager.currentWorkshopMapData = WorkshopMapData.currentMapToWorkshop();
		}
		return BehResult.Continue;
	}

	private void pickTheBest()
	{
		List<City> list = new List<City>();
		list.AddRange(BehaviourActionBase<AutoTesterBot>.world.cities);
		list.Sort(ComponentListBase<CityListElement, City, CityData, CityListComponent>.sortByPopulation);
		city = Randy.getRandom(list);
		List<Clan> list2 = new List<Clan>();
		list2.AddRange(BehaviourActionBase<AutoTesterBot>.world.clans);
		list2.Sort(ComponentListBase<ClanListElement, Clan, ClanData, ClanListComponent>.sortByPopulation);
		clan = Randy.getRandom(list2);
		List<Actor> list3 = new List<Actor>();
		list3.AddRange(BehaviourActionBase<AutoTesterBot>.world.units);
		list3.Sort(sortByActorMaturity);
		unit = Randy.getRandom(list3);
		List<Kingdom> list4 = new List<Kingdom>();
		list4.AddRange(BehaviourActionBase<AutoTesterBot>.world.kingdoms);
		list4.Sort(KingdomListComponent.sortByArmy);
		kingdom = Randy.getRandom(list4);
		List<Alliance> list5 = new List<Alliance>();
		list5.AddRange(BehaviourActionBase<AutoTesterBot>.world.alliances);
		list5.Sort(ComponentListBase<AllianceListElement, Alliance, AllianceData, AllianceListComponent>.sortByPopulation);
		alliance = Randy.getRandom(list5);
		List<Culture> list6 = new List<Culture>();
		list6.AddRange(BehaviourActionBase<AutoTesterBot>.world.cultures);
		list6.Sort(ComponentListBase<CultureListElement, Culture, CultureData, CultureListComponent>.sortByPopulation);
		culture = Randy.getRandom(list6);
		List<Plot> list7 = new List<Plot>();
		list7.AddRange(BehaviourActionBase<AutoTesterBot>.world.plots);
		list7.Sort(PlotListComponent.sortBySupporters);
		plot = Randy.getRandom(list7);
		List<War> list8 = new List<War>();
		list8.AddRange(BehaviourActionBase<AutoTesterBot>.world.wars);
		list8.Sort(WarListComponent.sortByAge);
		war = Randy.getRandom(list8);
	}

	public static int sortByActorMaturity(Actor pActor1, Actor pActor2)
	{
		if (pActor2.hasClan() && !pActor1.hasClan())
		{
			return 1;
		}
		if (pActor1.hasClan() && !pActor2.hasClan())
		{
			return -1;
		}
		if (pActor2.hasCulture() && !pActor1.hasCulture())
		{
			return 1;
		}
		if (pActor1.hasCulture() && !pActor2.hasCulture())
		{
			return -1;
		}
		if (pActor2.isKing() && !pActor1.isKing())
		{
			return 1;
		}
		if (pActor1.isKing() && !pActor2.isKing())
		{
			return -1;
		}
		int num = pActor2.countTraits().CompareTo(pActor1.countTraits());
		if (num != 0)
		{
			return num;
		}
		int num2 = pActor2.data.level.CompareTo(pActor1.data.level);
		if (num2 != 0)
		{
			return num2;
		}
		return pActor2.getAge().CompareTo(pActor1.getAge());
	}
}
