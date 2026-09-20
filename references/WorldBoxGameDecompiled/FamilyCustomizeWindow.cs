public class FamilyCustomizeWindow : GenericCustomizeWindow<Family, FamilyData, FamilyBanner>
{
	protected override MetaType meta_type => MetaType.Family;

	protected override Family meta_object => SelectedMetas.selected_family;

	protected override void onBannerChange()
	{
		image_banner_option_1.sprite = meta_object.getSpriteBackground();
		image_banner_option_2.sprite = meta_object.getSpriteFrame();
	}
}
