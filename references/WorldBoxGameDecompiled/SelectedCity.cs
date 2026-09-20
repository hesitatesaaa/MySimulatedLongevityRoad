using UnityEngine;

public class SelectedCity : SelectedMetaWithUnit<City, CityData>
{
	[SerializeField]
	private CityLoyaltyElement _loyalty_element;

	[SerializeField]
	private CitySelectedResources _resources;

	[SerializeField]
	private CitySelectedResources _food;

	private int _last_storage_version;

	protected override MetaType meta_type => MetaType.City;

	public override string unit_title_locale_key => "titled_leader";

	public override bool hasUnit()
	{
		return nano_object.hasLeader();
	}

	public override Actor getUnit()
	{
		return nano_object.leader;
	}

	protected override string getPowerTabAssetID()
	{
		return "selected_city";
	}

	protected override void showStatsGeneral(City pCity)
	{
		base.showStatsGeneral(pCity);
		int populationPeople = pCity.getPopulationPeople();
		pCity.countFoodTotal();
		if (populationPeople > pCity.getPopulationMaximum())
		{
			setIconValue("i_population", populationPeople, pCity.getPopulationMaximum(), "#FB2C21");
		}
		else
		{
			setIconValue("i_population", populationPeople, pCity.getPopulationMaximum());
		}
		setIconValue("i_territory", pCity.countZones());
		setIconValue("i_boats", pCity.countBoats());
		setIconValue("i_books", pCity.countBooks());
		int loyalty = pCity.getLoyalty(pForceRecalc: true);
		if (loyalty > 0)
		{
			setIconValue("i_loyalty", loyalty, null, "#43FF43");
		}
		else
		{
			setIconValue("i_loyalty", loyalty, null, "#FB2C21");
		}
		_loyalty_element.setCity(pCity);
		if (WorldLawLibrary.world_law_civ_army.isEnabled())
		{
			setIconValue("i_army", pCity.countWarriors(), pCity.getMaxWarriors());
		}
		else
		{
			setIconValue("i_army", pCity.countWarriors());
		}
		setIconValue("i_houses", pCity.getHouseCurrent(), pCity.getHouseLimit());
	}

	protected override void updateElements(City pNano)
	{
		if (!pNano.isRekt())
		{
			base.updateElements(pNano);
			_last_storage_version = pNano.getStorageVersion();
		}
	}

	protected override void updateElementsAlways(City pNano)
	{
		base.updateElementsAlways(pNano);
		if (storageChanged(pNano))
		{
			_resources.update(pNano);
			_food.update(pNano);
		}
	}

	protected override void checkAchievements(City pCity)
	{
		AchievementLibrary.checkCityAchievements(pCity);
	}

	public void openInventoryTab()
	{
		ScrollWindow.showWindow(base.window_id);
		ScrollWindow.getCurrentWindow().tabs.showTab("Inventory");
	}

	public void openBooksTab()
	{
		ScrollWindow.showWindow(base.window_id);
		ScrollWindow.getCurrentWindow().tabs.showTab("Books");
	}

	public void openFamilyTab()
	{
		ScrollWindow.showWindow(base.window_id);
		ScrollWindow.getCurrentWindow().tabs.showTab("Family");
	}

	private bool storageChanged(City pCity)
	{
		if (pCity.getStorageVersion() == _last_storage_version)
		{
			return isNanoChanged(pCity);
		}
		return true;
	}
}
