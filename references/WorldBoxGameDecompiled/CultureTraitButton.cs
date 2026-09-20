public class CultureTraitButton : TraitButton<CultureTrait>
{
	protected override string tooltip_type => "culture_trait";

	internal override void load(string pTraitID)
	{
		CultureTrait pElement = AssetManager.culture_traits.get(pTraitID);
		load(pElement);
	}

	protected override void startSignal()
	{
		AchievementLibrary.trait_explorer_culture.checkBySignal();
	}

	protected override TooltipData tooltipDataBuilder()
	{
		return new TooltipData
		{
			culture_trait = augmentation_asset
		};
	}
}
