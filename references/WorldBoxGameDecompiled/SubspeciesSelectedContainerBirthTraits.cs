using System.Collections.Generic;

public class SubspeciesSelectedContainerBirthTraits : SelectedContainerTraits<ActorTrait, ActorTraitButton, SubspeciesBirthTraitsContainer, SubspeciesBirthTraitsEditor>
{
	protected override MetaType meta_type => MetaType.Subspecies;

	protected override IReadOnlyCollection<ActorTrait> getTraits()
	{
		return SelectedMetas.selected_subspecies.getActorBirthTraits().getTraits();
	}

	protected override bool canEditTraits()
	{
		return true;
	}
}
