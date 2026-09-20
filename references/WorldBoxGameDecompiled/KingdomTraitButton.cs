public class KingdomTraitButton : TraitButton<KingdomTrait>
{
	protected override string tooltip_type => "kingdom_trait";

	internal override void load(string pTraitID)
	{
		KingdomTrait pElement = AssetManager.kingdoms_traits.get(pTraitID);
		load(pElement);
	}

	protected override TooltipData tooltipDataBuilder()
	{
		return new TooltipData
		{
			kingdom_trait = augmentation_asset
		};
	}
}
