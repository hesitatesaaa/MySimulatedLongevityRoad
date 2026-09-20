using System.Collections.Generic;

public class ActorSelectedContainerTraits : SelectedContainerTraits<ActorTrait, ActorTraitButton, ActorTraitsContainer, ActorTraitsEditor>
{
	protected override MetaType meta_type => MetaType.Unit;

	protected override IReadOnlyCollection<ActorTrait> getTraits()
	{
		return SelectedUnit.unit.getTraits();
	}

	protected override bool canEditTraits()
	{
		return SelectedUnit.unit.asset.can_edit_traits;
	}
}
