using System.Collections;
using UnityEngine;

public class CityStatsElement : CityElement, IStatsElement, IRefreshElement
{
	[SerializeField]
	private CityLoyaltyElement _loyalty_element;

	private StatsIconContainer _stats_icons;

	public void setIconValue(string pName, float pMainVal, float? pMax = null, string pColor = "", bool pFloat = false, string pEnding = "", char pSeparator = '/')
	{
		_stats_icons.setIconValue(pName, pMainVal, pMax, pColor, pFloat, pEnding, pSeparator);
	}

	protected override void Awake()
	{
		_stats_icons = ((Component)this).gameObject.AddOrGetComponent<StatsIconContainer>();
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		if (base.city != null && base.city.isAlive())
		{
			int populationPeople = base.city.getPopulationPeople();
			int num = base.city.countFoodTotal();
			_stats_icons.showGeneralIcons<City, CityData>(base.city);
			if (populationPeople > base.city.getPopulationMaximum())
			{
				setIconValue("i_population", populationPeople, base.city.getPopulationMaximum(), "#FB2C21");
			}
			else
			{
				setIconValue("i_population", populationPeople, base.city.getPopulationMaximum());
			}
			setIconValue("i_territory", base.city.countZones());
			setIconValue("i_boats", base.city.countBoats());
			CityStatsElement cityStatsElement = this;
			float pMainVal = num;
			string pColor = ((num > populationPeople * 4) ? "#43FF43" : "#FB2C21");
			cityStatsElement.setIconValue("i_food", pMainVal, null, pColor);
			setIconValue("i_farmers", base.city.jobs.countOccupied(CitizenJobLibrary.farmer));
			setIconValue("i_books", base.city.countBooks());
			int loyalty = base.city.getLoyalty(pForceRecalc: true);
			if (loyalty > 0)
			{
				setIconValue("i_loyalty", loyalty, null, "#43FF43");
			}
			else
			{
				setIconValue("i_loyalty", loyalty, null, "#FB2C21");
			}
			_loyalty_element.setCity(base.city);
			if (WorldLawLibrary.world_law_civ_army.isEnabled())
			{
				setIconValue("i_army", base.city.countWarriors(), base.city.getMaxWarriors());
			}
			else
			{
				setIconValue("i_army", base.city.countWarriors());
			}
			setIconValue("i_houses", base.city.getHouseCurrent(), base.city.getHouseLimit());
		}
		yield break;
	}
}
