using System.Collections.Generic;

public class TooltipSubspeciesTraitsRow : TooltipTraitsRow<SubspeciesTrait>
{
	protected override IReadOnlyCollection<SubspeciesTrait> traits_hashset => tooltip_data.subspecies.getTraits();
}
