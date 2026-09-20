using System.Collections.Generic;
using UnityEngine;

public class LanguageWindow : WindowMetaGeneric<Language, LanguageData>, ITraitWindow<LanguageTrait, LanguageTraitButton>, IAugmentationsWindow<ITraitsEditor<LanguageTrait>>, IBooksWindow
{
	public override MetaType meta_type => MetaType.Language;

	protected override Language meta_object => SelectedMetas.selected_language;

	protected override void showTopPartInformation()
	{
		base.showTopPartInformation();
		AchievementLibrary.multiply_spoken.checkBySignal(meta_object);
	}

	internal override void showStatsRows()
	{
		Language language = meta_object;
		tryShowPastNames();
		showStatRow("founded", language.getFoundedDate(), MetaType.None, -1L, "iconAge");
		tryToShowActor("creator", language.data.creator_id, language.data.creator_name, null, "actor_traits/iconStupid");
		tryToShowMetaClan("creators_clan", language.data.creator_clan_id, language.data.creator_clan_name);
		tryToShowMetaKingdom("origin", language.data.creator_kingdom_id, language.data.creator_kingdom_name);
		tryToShowMetaCity("birthplace", language.data.creator_city_id, language.data.creator_city_name);
		tryToShowMetaSubspecies("creator_subspecies", language.data.creator_subspecies_id, language.data.creator_subspecies_name);
		tryToShowMetaSpecies("creator_species", language.data.creator_species_id);
	}

	public List<long> getBooks()
	{
		return meta_object.books.getList();
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
			if (!book.isRekt() && book.data.language_id == iD)
			{
				book.data.language_name = name;
			}
		}
		return true;
	}

	T IAugmentationsWindow<ITraitsEditor<LanguageTrait>>.GetComponentInChildren<T>(bool includeInactive)
	{
		return ((Component)this).GetComponentInChildren<T>(includeInactive);
	}
}
