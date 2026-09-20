public class ReligionTraitButton : TraitButton<ReligionTrait>
{
	protected override string tooltip_type => "religion_trait";

	internal override void load(string pTraitID)
	{
		ReligionTrait pElement = AssetManager.religion_traits.get(pTraitID);
		load(pElement);
	}

	protected override void startSignal()
	{
		AchievementLibrary.trait_explorer_religion.checkBySignal();
	}

	protected override TooltipData tooltipDataBuilder()
	{
		return new TooltipData
		{
			religion_trait = augmentation_asset
		};
	}
}
