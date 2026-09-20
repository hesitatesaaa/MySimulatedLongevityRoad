using System.Collections.Generic;

public class TooltipClanTraitsRow : TooltipTraitsRow<ClanTrait>
{
	protected override IReadOnlyCollection<ClanTrait> traits_hashset => tooltip_data.clan.getTraits();
}
