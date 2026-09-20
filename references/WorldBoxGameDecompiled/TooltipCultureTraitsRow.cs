using System.Collections.Generic;

public class TooltipCultureTraitsRow : TooltipTraitsRow<CultureTrait>
{
	protected override IReadOnlyCollection<CultureTrait> traits_hashset => tooltip_data.culture.getTraits();
}
