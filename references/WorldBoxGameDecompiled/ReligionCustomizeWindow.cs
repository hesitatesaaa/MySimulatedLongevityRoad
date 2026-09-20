public class ReligionCustomizeWindow : GenericCustomizeWindow<Religion, ReligionData, ReligionBanner>
{
	protected override MetaType meta_type => MetaType.Religion;

	protected override Religion meta_object => SelectedMetas.selected_religion;

	protected override void onBannerChange()
	{
		image_banner_option_1.sprite = meta_object.getBackgroundSprite();
		image_banner_option_2.sprite = meta_object.getIconSprite();
	}
}
