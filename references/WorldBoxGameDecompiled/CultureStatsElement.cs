using System.Collections;
using UnityEngine;

public class CultureStatsElement : CultureElement, IStatsElement, IRefreshElement
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
		if (base.culture != null && base.culture.isAlive())
		{
			_stats_icons.showGeneralIcons<Culture, CultureData>(base.culture);
			setIconValue("i_cities", base.culture.countCities());
			setIconValue("i_kingdoms", base.culture.countKingdoms());
			setIconValue("i_books", base.culture.books.count());
		}
		yield break;
	}
}
