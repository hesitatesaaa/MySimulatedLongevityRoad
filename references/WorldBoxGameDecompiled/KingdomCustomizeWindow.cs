public class KingdomCustomizeWindow : GenericCustomizeWindow<Kingdom, KingdomData, KingdomBanner>
{
	protected override MetaType meta_type => MetaType.Kingdom;

	protected override Kingdom meta_object => SelectedMetas.selected_kingdom;

	protected override void onBannerChange()
	{
		meta_object.getActorAsset();
		image_banner_option_1.sprite = meta_object.getElementBackground();
		image_banner_option_2.sprite = meta_object.getElementIcon();
	}
}
