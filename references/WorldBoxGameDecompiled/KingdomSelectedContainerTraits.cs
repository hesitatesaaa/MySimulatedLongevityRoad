using System.Collections.Generic;

public class KingdomSelectedContainerTraits : SelectedContainerTraits<KingdomTrait, KingdomTraitButton, KingdomTraitsContainer, KingdomTraitsEditor>
{
	protected override MetaType meta_type => MetaType.Kingdom;

	protected override IReadOnlyCollection<KingdomTrait> getTraits()
	{
		return SelectedMetas.selected_kingdom.getTraits();
	}

	protected override bool canEditTraits()
	{
		return true;
	}
}
