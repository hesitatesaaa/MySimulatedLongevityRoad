using UnityEngine;

public class SelectedCulture : SelectedMeta<Culture, CultureData>
{
	[SerializeField]
	private CultureSelectedOnomasticsNames _onomastics_names;

	[SerializeField]
	private CitiesKingdomsContainersController _banners_cities_kingdoms;

	protected override MetaType meta_type => MetaType.Culture;

	protected override string getPowerTabAssetID()
	{
		return "selected_culture";
	}

	protected override void showStatsGeneral(Culture pCulture)
	{
		base.showStatsGeneral(pCulture);
		setIconValue("i_kingdoms", pCulture.countKingdoms());
		setIconValue("i_cities", pCulture.countCities());
		setIconValue("i_books", pCulture.books.count());
	}

	protected override void updateElementsOnChange(Culture pNano)
	{
		base.updateElementsOnChange(pNano);
		_onomastics_names.load(pNano);
		_banners_cities_kingdoms.update(pNano);
	}

	protected override void updateElementsAlways(Culture pNano)
	{
		base.updateElementsAlways(pNano);
		_onomastics_names.update();
	}

	public void openOnomasticsTab()
	{
		ScrollWindow.showWindow(base.window_id);
		ScrollWindow.getCurrentWindow().tabs.showTab("Onomastics");
	}
}
