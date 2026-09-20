using System.Collections;
using UnityEngine;

public class FamilyStatsElement : FamilyElement, IStatsElement, IRefreshElement
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
		if (base.family != null && base.family.isAlive())
		{
			_stats_icons.showGeneralIcons<Family, FamilyData>(base.family);
		}
		yield break;
	}
}
