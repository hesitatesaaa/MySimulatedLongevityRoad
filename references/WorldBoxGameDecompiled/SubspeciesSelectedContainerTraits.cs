using System.Collections.Generic;

public class SubspeciesSelectedContainerTraits : SelectedContainerTraits<SubspeciesTrait, SubspeciesTraitButton, SubspeciesTraitsContainer, SubspeciesTraitsEditor>
{
	protected override MetaType meta_type => MetaType.Subspecies;

	protected override IReadOnlyCollection<SubspeciesTrait> getTraits()
	{
		return SelectedMetas.selected_subspecies.getTraits();
	}

	protected override bool canEditTraits()
	{
		return true;
	}
}
