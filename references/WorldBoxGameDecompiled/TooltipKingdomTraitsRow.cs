using System.Collections.Generic;

public class TooltipKingdomTraitsRow : TooltipTraitsRow<KingdomTrait>
{
	protected override IReadOnlyCollection<KingdomTrait> traits_hashset => tooltip_data.kingdom.getTraits();
}
