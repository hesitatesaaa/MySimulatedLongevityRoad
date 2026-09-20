using System.Collections.Generic;

public class CultureSelectedContainerTraits : SelectedContainerTraits<CultureTrait, CultureTraitButton, CultureTraitsContainer, CultureTraitsEditor>
{
	protected override MetaType meta_type => MetaType.Culture;

	protected override IReadOnlyCollection<CultureTrait> getTraits()
	{
		return SelectedMetas.selected_culture.getTraits();
	}

	protected override bool canEditTraits()
	{
		return true;
	}
}
