using System.Collections.Generic;
using UnityEngine;

public class CultureWindow : WindowMetaGeneric<Culture, CultureData>, ITraitWindow<CultureTrait, CultureTraitButton>, IAugmentationsWindow<ITraitsEditor<CultureTrait>>, IBooksWindow
{
	public StatBar experienceBar;

	public override MetaType meta_type => MetaType.Culture;

	protected override Culture meta_object => SelectedMetas.selected_culture;

	public void testDebugNewBook()
	{
		meta_object.testDebugNewBook();
		startShowingWindow();
		scroll_window.tabs.showTab(scroll_window.tabs.getActiveTab());
	}

	public List<long> getBooks()
	{
		return meta_object.books.getList();
	}

	protected override void showTopPartInformation()
	{
		base.showTopPartInformation();
		_ = meta_object;
	}

	internal override void showStatsRows()
	{
		Culture culture = meta_object;
		tryShowPastNames();
		showStatRow("founded", culture.getFoundedDate(), MetaType.None, -1L);
		tryToShowActor("founder", culture.data.creator_id, culture.data.creator_name, null, "actor_traits/iconStupid");
		tryToShowMetaClan("founder_clan", culture.data.creator_clan_id, culture.data.creator_clan_name);
		tryToShowMetaKingdom("origin", culture.data.creator_kingdom_id, culture.data.creator_kingdom_name);
		tryToShowMetaCity("birthplace", culture.data.creator_city_id, culture.data.creator_city_name);
		tryToShowMetaSubspecies("founder_subspecies", culture.data.creator_subspecies_id, culture.data.creator_subspecies_name);
		tryToShowMetaSpecies("founder_species", culture.data.creator_species_id);
	}

	protected override bool onNameChange(string pInput)
	{
		if (!base.onNameChange(pInput))
		{
			return false;
		}
		long iD = meta_object.getID();
		string name = meta_object.data.name;
		foreach (Book book in World.world.books)
		{
			if (!book.isRekt() && book.data.culture_id == iD)
			{
				book.data.culture_name = name;
			}
		}
		return true;
	}

	T IAugmentationsWindow<ITraitsEditor<CultureTrait>>.GetComponentInChildren<T>(bool includeInactive)
	{
		return ((Component)this).GetComponentInChildren<T>(includeInactive);
	}
}
