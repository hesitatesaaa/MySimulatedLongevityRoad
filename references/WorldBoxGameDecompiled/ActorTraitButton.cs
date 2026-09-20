public class ActorTraitButton : TraitButton<ActorTrait>
{
	protected override string tooltip_type => "trait";

	internal override void load(string pTraitID)
	{
		ActorTrait pElement = AssetManager.traits.get(pTraitID);
		load(pElement);
	}

	protected override TooltipData tooltipDataBuilder()
	{
		return new TooltipData
		{
			trait = augmentation_asset
		};
	}
}
