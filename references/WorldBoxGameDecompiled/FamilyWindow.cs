using UnityEngine.UI;

public class FamilyWindow : WindowMetaGeneric<Family, FamilyData>
{
	public Text title_family;

	public override MetaType meta_type => MetaType.Family;

	protected override Family meta_object => SelectedMetas.selected_family;

	protected override void showTopPartInformation()
	{
		base.showTopPartInformation();
		Family family = meta_object;
		if (family != null)
		{
			ActorAsset actorAsset = family.getActorAsset();
			title_family.text = LocalizedTextManager.getText(actorAsset.getCollectiveTermID());
		}
	}

	internal override void showStatsRows()
	{
		Family family = meta_object;
		tryShowPastNames();
		showStatRow("founded", family.getFoundedDate(), MetaType.None, -1L, "iconAge");
		tryToShowActor("founder", family.data.main_founder_id_1, family.data.founder_actor_name_1, null, "actor_traits/iconStupid");
		if (family.data.main_founder_id_2 != -1)
		{
			tryToShowActor("founder", family.data.main_founder_id_2, family.data.founder_actor_name_2, null, "actor_traits/iconStupid");
		}
		tryToShowMetaKingdom("origin", family.data.founder_kingdom_id, family.data.founder_kingdom_name);
		tryToShowMetaCity("birthplace", family.data.founder_city_id, family.data.founder_city_name);
		tryToShowMetaSubspecies("founder_subspecies", family.data.subspecies_id, family.data.subspecies_name);
		foreach (Family originFamily in family.getOriginFamilies())
		{
			tryToShowMetaFamily("origin_family", -1L, null, originFamily);
		}
		tryToShowMetaSpecies("founder_species", family.data.species_id);
	}
}
