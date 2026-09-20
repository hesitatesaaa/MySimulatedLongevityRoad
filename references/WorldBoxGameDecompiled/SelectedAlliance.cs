using UnityEngine;

public class SelectedAlliance : SelectedMeta<Alliance, AllianceData>
{
	[SerializeField]
	private AllianceSelectedKingdomsContainer _kingdoms_container;

	protected override MetaType meta_type => MetaType.Alliance;

	protected override string getPowerTabAssetID()
	{
		return "selected_alliance";
	}

	protected override void updateElementsOnChange(Alliance pNano)
	{
		base.updateElementsOnChange(pNano);
		_kingdoms_container.update(pNano);
	}

	protected override void showStatsGeneral(Alliance pAlliance)
	{
		base.showStatsGeneral(pAlliance);
		setIconValue("i_army", pAlliance.countWarriors());
		setIconValue("i_kingdoms", pAlliance.countKingdoms());
		setIconValue("i_zones", pAlliance.countZones());
		setIconValue("i_cities", pAlliance.countCities());
		setIconValue("i_money", pAlliance.countTotalMoney());
		setIconValue("i_buildings", pAlliance.countBuildings());
		setIconValue("i_territory", pAlliance.countZones());
	}

	protected override void setTitleIcons(Alliance pAlliance)
	{
	}
}
