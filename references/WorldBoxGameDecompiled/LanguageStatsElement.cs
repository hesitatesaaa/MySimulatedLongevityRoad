using System.Collections;
using UnityEngine;

public class LanguageStatsElement : LanguageElement, IStatsElement, IRefreshElement
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
		if (base.language != null && base.language.isAlive())
		{
			_stats_icons.showGeneralIcons<Language, LanguageData>(base.language);
			setIconValue("i_books", base.language.books.count());
			setIconValue("i_kingdoms", base.language.countKingdoms());
			setIconValue("i_cities", base.language.countCities());
			setIconValue("i_books_written", base.language.data.books_written);
		}
		yield break;
	}
}
