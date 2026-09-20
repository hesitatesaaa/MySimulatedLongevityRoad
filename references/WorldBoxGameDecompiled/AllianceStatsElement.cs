using System.Collections;
using UnityEngine;

public class AllianceStatsElement : AllianceElement, IStatsElement, IRefreshElement
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
		if (base.alliance != null && base.alliance.isAlive())
		{
			_stats_icons.showGeneralIcons<Alliance, AllianceData>(base.alliance);
			setIconValue("i_population", base.alliance.countPopulation());
			setIconValue("i_army", base.alliance.countWarriors());
			setIconValue("i_kingdoms", base.alliance.countKingdoms());
			setIconValue("i_cities", base.alliance.countCities());
			setIconValue("i_buildings", base.alliance.countBuildings());
			setIconValue("i_zones", base.alliance.countZones());
		}
		yield break;
	}
}
