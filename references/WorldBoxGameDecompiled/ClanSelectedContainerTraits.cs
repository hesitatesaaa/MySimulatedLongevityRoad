using System.Collections.Generic;

public class ClanSelectedContainerTraits : SelectedContainerTraits<ClanTrait, ClanTraitButton, ClanTraitsContainer, ClanTraitsEditor>
{
	protected override MetaType meta_type => MetaType.Clan;

	protected override IReadOnlyCollection<ClanTrait> getTraits()
	{
		return SelectedMetas.selected_clan.getTraits();
	}

	protected override bool canEditTraits()
	{
		return true;
	}
}
