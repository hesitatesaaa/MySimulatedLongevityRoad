using System.Collections.Generic;

public class TooltipActorTraitsRow : TooltipTraitsRow<ActorTrait>
{
	protected override IReadOnlyCollection<ActorTrait> traits_hashset => tooltip_data.actor.getTraits();
}
