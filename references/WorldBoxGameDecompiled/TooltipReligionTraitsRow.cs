using System.Collections.Generic;

public class TooltipReligionTraitsRow : TooltipTraitsRow<ReligionTrait>
{
	protected override IReadOnlyCollection<ReligionTrait> traits_hashset => tooltip_data.religion.getTraits();
}
