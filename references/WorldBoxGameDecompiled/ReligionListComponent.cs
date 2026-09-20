public class ReligionListComponent : ComponentListBase<ReligionListElement, Religion, ReligionData, ReligionListComponent>
{
	protected override MetaType meta_type => MetaType.Religion;

	protected override void setupSortingTabs()
	{
		genericMetaSortByAge(base.sortByAge);
		genericMetaSortByRenown(base.sortByRenown);
		genericMetaSortByPopulation(ComponentListBase<ReligionListElement, Religion, ReligionData, ReligionListComponent>.sortByPopulation);
		genericMetaSortByKills(ComponentListBase<ReligionListElement, Religion, ReligionData, ReligionListComponent>.sortByKills);
		genericMetaSortByDeath(ComponentListBase<ReligionListElement, Religion, ReligionData, ReligionListComponent>.sortByDeaths);
		sorting_tab.tryAddButton("ui/Icons/iconVillages", "sort_by_villages", show, delegate
		{
			current_sort = sortByVillages;
		});
	}

	public static int sortByVillages(Religion p1, Religion p2)
	{
		return p2.cities.Count.CompareTo(p1.cities.Count);
	}
}
