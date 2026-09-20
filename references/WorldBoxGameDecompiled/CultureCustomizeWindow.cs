public class CultureCustomizeWindow : GenericCustomizeWindow<Culture, CultureData, CultureBanner>
{
	protected override MetaType meta_type => MetaType.Culture;

	protected override Culture meta_object => SelectedMetas.selected_culture;

	protected override void onBannerChange()
	{
		image_banner_option_1.sprite = meta_object.getDecorSprite();
		image_banner_option_2.sprite = meta_object.getElementSprite();
	}
}
