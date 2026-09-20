using System.Collections.Generic;

public class ReligionSelectedContainerTraits : SelectedContainerTraits<ReligionTrait, ReligionTraitButton, ReligionTraitsContainer, ReligionTraitsEditor>
{
	protected override MetaType meta_type => MetaType.Religion;

	protected override IReadOnlyCollection<ReligionTrait> getTraits()
	{
		return SelectedMetas.selected_religion.getTraits();
	}

	protected override bool canEditTraits()
	{
		return true;
	}
}
