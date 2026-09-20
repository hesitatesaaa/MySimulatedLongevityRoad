using System.Collections;
using UnityEngine;

public class ArmyStatsElement : ArmyElement, IStatsElement, IRefreshElement
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
		if (base.army != null && base.army.isAlive())
		{
			_stats_icons.showGeneralIcons<Army, ArmyData>(base.army);
			setIconValue("i_army_size", base.army.countUnits());
			setIconValue("i_kills", base.army.getTotalKills());
			setIconValue("i_melee", base.army.countMelee());
			setIconValue("i_range", base.army.countRange());
		}
		yield break;
	}
}
