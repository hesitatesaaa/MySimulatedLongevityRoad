using System.Collections.Generic;
using UnityEngine;

public class ReligionWindow : WindowMetaGeneric<Religion, ReligionData>, ITraitWindow<ReligionTrait, ReligionTraitButton>, IAugmentationsWindow<ITraitsEditor<ReligionTrait>>, IBooksWindow
{
	public StatBar experienceBar;

	public override MetaType meta_type => MetaType.Religion;

	protected override Religion meta_object => SelectedMetas.selected_religion;

	public List<long> getBooks()
	{
		return meta_object.books.getList();
	}

	protected override void showTopPartInformation()
	{
		base.showTopPartInformation();
		_ = meta_object;
		AchievementLibrary.not_just_a_cult.checkBySignal(meta_object);
	}

	internal override void showStatsRows()
	{
		Religion religion = meta_object;
		tryShowPastNames();
		showStatRow("founded", religion.getFoundedDate(), MetaType.None, -1L, "iconAge");
		tryToShowActor("founder", religion.data.creator_id, religion.data.creator_name, null, "actor_traits/iconStupid");
		tryToShowMetaClan("founder_clan", religion.data.creator_clan_id, religion.data.creator_clan_name);
		tryToShowMetaKingdom("origin", religion.data.creator_kingdom_id, religion.data.creator_kingdom_name);
		tryToShowMetaCity("birthplace", religion.data.creator_city_id, religion.data.creator_city_name);
		tryToShowMetaSubspecies("founder_subspecies", religion.data.creator_subspecies_id, religion.data.creator_subspecies_name);
		tryToShowMetaSpecies("founder_species", religion.data.creator_species_id);
		showStatRow("deity", "??", ColorStyleLibrary.m.color_dead_text, MetaType.None, -1L, pColorText: false, "iconDivineLight");
	}

	public void testDebugNewBook()
	{
		if (meta_object.units.Count != 0)
		{
			Actor random = meta_object.units.GetRandom();
			if (random.getCity() != null && random.city.hasBookSlots())
			{
				World.world.books.generateNewBook(random);
				startShowingWindow();
				scroll_window.tabs.showTab(scroll_window.tabs.getActiveTab());
			}
		}
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
			if (!book.isRekt() && book.data.religion_id == iD)
			{
				book.data.religion_name = name;
			}
		}
		return true;
	}

	T IAugmentationsWindow<ITraitsEditor<ReligionTrait>>.GetComponentInChildren<T>(bool includeInactive)
	{
		return ((Component)this).GetComponentInChildren<T>(includeInactive);
	}
}
