public class AllianceCustomizeWindow : GenericCustomizeWindow<Alliance, AllianceData, AllianceBanner>
{
	protected override MetaType meta_type => MetaType.Alliance;

	protected override Alliance meta_object => SelectedMetas.selected_alliance;

	protected override void onBannerChange()
	{
		image_banner_option_1.sprite = meta_object.getBackgroundSprite();
		image_banner_option_2.sprite = meta_object.getIconSprite();
	}
}
