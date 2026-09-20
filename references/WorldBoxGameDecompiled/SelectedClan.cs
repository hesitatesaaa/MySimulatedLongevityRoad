using UnityEngine;

public class SelectedClan : SelectedMetaWithUnit<Clan, ClanData>
{
	[SerializeField]
	private CitiesKingdomsContainersController _banners_cities_kingdoms;

	protected override MetaType meta_type => MetaType.Clan;

	public override string unit_title_locale_key => "titled_chief";

	public override bool hasUnit()
	{
		return !nano_object.getChief().isRekt();
	}

	public override Actor getUnit()
	{
		return nano_object.getChief();
	}

	protected override string getPowerTabAssetID()
	{
		return "selected_clan";
	}

	protected override void showStatsGeneral(Clan pClan)
	{
		base.showStatsGeneral(pClan);
		setIconValue("i_books_written", pClan.data.books_written);
	}

	public void openPeopleTab()
	{
		ScrollWindow.showWindow(base.window_id);
		ScrollWindow.getCurrentWindow().tabs.showTab("People");
	}

	protected override void updateElementsOnChange(Clan pNano)
	{
		base.updateElementsOnChange(pNano);
		_banners_cities_kingdoms.update(pNano);
	}
}
