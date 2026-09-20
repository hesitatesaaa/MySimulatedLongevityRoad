using System.Collections;
using UnityEngine;

public class KingdomStatsElement : KingdomElement, IStatsElement, IRefreshElement
{
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
		if (base.kingdom != null && base.kingdom.isAlive())
		{
			_stats_icons.showGeneralIcons<Kingdom, KingdomData>(base.kingdom);
			setIconValue("i_population", base.kingdom.getPopulationPeople(), base.kingdom.getPopulationTotalPossible());
			setIconValue("i_army", base.kingdom.countTotalWarriors(), base.kingdom.countWarriorsMax());
			if (base.kingdom.countCities() > base.kingdom.getMaxCities())
			{
				setIconValue("i_cities", base.kingdom.countCities(), base.kingdom.getMaxCities(), "#FB2C21");
			}
			else
			{
				setIconValue("i_cities", base.kingdom.countCities(), base.kingdom.getMaxCities());
			}
			setIconValue("i_territory", base.kingdom.countZones());
			setIconValue("i_buildings", base.kingdom.countBuildings());
			setIconValue("i_food", base.kingdom.countTotalFood());
		}
		yield break;
	}
}
