using UnityEngine;

public class SelectedKingdom : SelectedMetaWithUnit<Kingdom, KingdomData>
{
	[SerializeField]
	private KingdomSelectedAlliesContainer _allies_container;

	[SerializeField]
	private KingdomSelectedWarsContainer _wars_container;

	protected override MetaType meta_type => MetaType.Kingdom;

	public override string unit_title_locale_key => "titled_king";

	public override bool hasUnit()
	{
		return !nano_object.king.isRekt();
	}

	public override Actor getUnit()
	{
		return nano_object.king;
	}

	protected override string getPowerTabAssetID()
	{
		return "selected_kingdom";
	}

	protected override void updateElementsOnChange(Kingdom pNano)
	{
		base.updateElementsOnChange(pNano);
		_allies_container.update(pNano);
		_wars_container.update(pNano);
	}

	protected override void showStatsGeneral(Kingdom pKingdom)
	{
		base.showStatsGeneral(pKingdom);
		if (pKingdom.countCities() > pKingdom.getMaxCities())
		{
			setIconValue("i_cities", pKingdom.countCities(), pKingdom.getMaxCities(), "#FB2C21");
		}
		else
		{
			setIconValue("i_cities", pKingdom.countCities(), pKingdom.getMaxCities());
		}
		setIconValue("i_food", pKingdom.countTotalFood());
		setIconValue("i_money", pKingdom.countTotalMoney());
		setIconValue("i_buildings", pKingdom.countBuildings());
		setIconValue("i_army", pKingdom.countTotalWarriors(), pKingdom.countWarriorsMax());
		setIconValue("i_territory", pKingdom.countZones());
	}

	public void openVillagesTab()
	{
		ScrollWindow.showWindow(base.window_id);
		ScrollWindow.getCurrentWindow().tabs.showTab("Villages");
	}
}
