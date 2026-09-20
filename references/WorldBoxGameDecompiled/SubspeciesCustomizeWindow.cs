public class SubspeciesCustomizeWindow : GenericCustomizeWindow<Subspecies, SubspeciesData, SubspeciesBanner>
{
	protected override MetaType meta_type => MetaType.Subspecies;

	protected override Subspecies meta_object => SelectedMetas.selected_subspecies;

	protected override void onBannerChange()
	{
		image_banner_option_1.sprite = meta_object.getSpriteBackground();
		image_banner_option_2.sprite = meta_object.getSpriteIcon();
	}

	protected override void updateColorsBanner()
	{
	}
}
