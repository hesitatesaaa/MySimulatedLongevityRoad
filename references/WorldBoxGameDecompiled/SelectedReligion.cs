using UnityEngine;

public class SelectedReligion : SelectedMeta<Religion, ReligionData>
{
	[SerializeField]
	private CitiesKingdomsContainersController _banners_cities_kingdoms;

	protected override MetaType meta_type => MetaType.Religion;

	protected override string getPowerTabAssetID()
	{
		return "selected_religion";
	}

	protected override void showStatsGeneral(Religion pReligion)
	{
		base.showStatsGeneral(pReligion);
		setIconValue("i_kingdoms", pReligion.countKingdoms());
		setIconValue("i_cities", pReligion.countCities());
		setIconValue("i_books", pReligion.books.count());
	}

	protected override void updateElementsOnChange(Religion pNano)
	{
		base.updateElementsOnChange(pNano);
		_banners_cities_kingdoms.update(pNano);
	}

	protected override void checkAchievements(Religion pNano)
	{
		AchievementLibrary.not_just_a_cult.checkBySignal(pNano);
	}
}
