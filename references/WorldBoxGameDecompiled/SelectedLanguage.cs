using UnityEngine;

public class SelectedLanguage : SelectedMeta<Language, LanguageData>
{
	[SerializeField]
	private CitiesKingdomsContainersController _banners_cities_kingdoms;

	protected override MetaType meta_type => MetaType.Language;

	protected override string getPowerTabAssetID()
	{
		return "selected_language";
	}

	protected override void showStatsGeneral(Language pLanguage)
	{
		base.showStatsGeneral(pLanguage);
		setIconValue("i_books", pLanguage.books.count());
		setIconValue("i_kingdoms", pLanguage.countKingdoms());
		setIconValue("i_cities", pLanguage.countCities());
		setIconValue("i_books_written", pLanguage.data.books_written);
	}

	protected override void updateElementsOnChange(Language pNano)
	{
		base.updateElementsOnChange(pNano);
		_banners_cities_kingdoms.update(pNano);
	}

	protected override void checkAchievements(Language pNano)
	{
		AchievementLibrary.multiply_spoken.checkBySignal(pNano);
	}
}
