public class SelectedArmy : SelectedMetaWithUnit<Army, ArmyData>
{
	protected override MetaType meta_type => MetaType.Army;

	public override string unit_title_locale_key => "titled_captain";

	public override bool hasUnit()
	{
		return !nano_object.getCaptain().isRekt();
	}

	public override Actor getUnit()
	{
		return nano_object.getCaptain();
	}

	protected override string getPowerTabAssetID()
	{
		return "selected_army";
	}

	protected override void showStatsGeneral(Army pArmy)
	{
		base.showStatsGeneral(pArmy);
		setIconValue("i_army_size", pArmy.countUnits());
		setIconValue("i_money", pArmy.countTotalMoney());
		setIconValue("i_melee", pArmy.countMelee());
		setIconValue("i_range", pArmy.countRange());
	}
}
