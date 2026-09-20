public class LanguageTraitButton : TraitButton<LanguageTrait>
{
	protected override string tooltip_type => "language_trait";

	internal override void load(string pTraitID)
	{
		LanguageTrait pElement = AssetManager.language_traits.get(pTraitID);
		load(pElement);
	}

	protected override void startSignal()
	{
		AchievementLibrary.trait_explorer_language.checkBySignal();
	}

	protected override TooltipData tooltipDataBuilder()
	{
		return new TooltipData
		{
			language_trait = augmentation_asset
		};
	}
}
