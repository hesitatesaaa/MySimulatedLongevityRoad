public class ClanListComponent : ComponentListBase<ClanListElement, Clan, ClanData, ClanListComponent>
{
	protected override MetaType meta_type => MetaType.Clan;

	protected override void setupSortingTabs()
	{
		genericMetaSortByAge(base.sortByAge);
		genericMetaSortByRenown(base.sortByRenown);
		genericMetaSortByPopulation(ComponentListBase<ClanListElement, Clan, ClanData, ClanListComponent>.sortByPopulation);
		genericMetaSortByKills(ComponentListBase<ClanListElement, Clan, ClanData, ClanListComponent>.sortByKills);
		genericMetaSortByDeath(ComponentListBase<ClanListElement, Clan, ClanData, ClanListComponent>.sortByDeaths);
		sorting_tab.tryAddButton("ui/Icons/iconKingdom", "sort_by_kingdom", show, delegate
		{
			current_sort = sortByKingdom;
		});
	}

	private static int sortByKingdom(Clan p1, Clan p2)
	{
		Actor chief = p1.getChief();
		return p2.getChief().kingdom.CompareTo(chief.kingdom);
	}
}
