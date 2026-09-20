public class ClanCustomizeWindow : GenericCustomizeWindow<Clan, ClanData, ClanBanner>
{
	protected override MetaType meta_type => MetaType.Clan;

	protected override Clan meta_object => SelectedMetas.selected_clan;

	protected override void onBannerChange()
	{
		image_banner_option_1.sprite = meta_object.getBackgroundSprite();
		image_banner_option_2.sprite = meta_object.getIconSprite();
	}
}
