using System.Collections.Generic;

public class TooltipSpeciesSpawnTraitsRow : TooltipTraitsRow<SubspeciesTrait>
{
	protected override IReadOnlyCollection<SubspeciesTrait> traits_hashset => loadTraitsFromPowerAsset();

	private HashSet<SubspeciesTrait> loadTraitsFromPowerAsset()
	{
		string text = "";
		text = ((tooltip_data.power == null) ? tooltip_data.tip_name : tooltip_data.power.getActorAssetID());
		return AssetManager.actor_library.get(text)?.getDefaultSubspeciesTraits();
	}
}
