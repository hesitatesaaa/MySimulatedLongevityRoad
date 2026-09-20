public class SubspeciesTraitButton : TraitButton<SubspeciesTrait>
{
	protected override string tooltip_type => "subspecies_trait";

	internal override void load(string pTraitID)
	{
		SubspeciesTrait pElement = AssetManager.subspecies_traits.get(pTraitID);
		load(pElement);
	}

	protected override void startSignal()
	{
		AchievementLibrary.trait_explorer_subspecies.checkBySignal();
	}

	protected override TooltipData tooltipDataBuilder()
	{
		return new TooltipData
		{
			subspecies_trait = augmentation_asset
		};
	}
}
