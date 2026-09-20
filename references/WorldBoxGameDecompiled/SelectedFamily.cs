public class SelectedFamily : SelectedMetaWithUnit<Family, FamilyData>
{
	protected override MetaType meta_type => MetaType.Family;

	public override string unit_title_locale_key => "titled_alpha";

	public override bool hasUnit()
	{
		return !nano_object.getAlpha().isRekt();
	}

	public override Actor getUnit()
	{
		return nano_object.getAlpha();
	}

	protected override string getPowerTabAssetID()
	{
		return "selected_family";
	}

	public void openPeopleTab()
	{
		ScrollWindow.showWindow(base.window_id);
		ScrollWindow.getCurrentWindow().tabs.showTab("People");
	}
}
