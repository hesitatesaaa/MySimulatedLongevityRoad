public class LanguageCustomizeWindow : GenericCustomizeWindow<Language, LanguageData, LanguageBanner>
{
	protected override MetaType meta_type => MetaType.Language;

	protected override Language meta_object => SelectedMetas.selected_language;

	protected override void onBannerChange()
	{
		image_banner_option_1.sprite = meta_object.getBackgroundSprite();
		image_banner_option_2.sprite = meta_object.getIconSprite();
	}
}
