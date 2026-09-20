using System.Collections;
using UnityEngine;

public class ReligionStatsElement : ReligionElement, IStatsElement, IRefreshElement
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
		if (base.religion != null && base.religion.isAlive())
		{
			_stats_icons.showGeneralIcons<Religion, ReligionData>(base.religion);
			setIconValue("i_kingdoms", base.religion.countKingdoms());
			setIconValue("i_cities", base.religion.countCities());
			setIconValue("i_books", meta_object.books.count());
		}
		yield break;
	}
}
