public class CityListComponent : ComponentListBase<CityListElement, City, CityData, CityListComponent>
{
	protected override MetaType meta_type => MetaType.City;

	protected override void setupSortingTabs()
	{
		genericMetaSortByAge(base.sortByAge);
		genericMetaSortByRenown(base.sortByRenown);
		genericMetaSortByPopulation(ComponentListBase<CityListElement, City, CityData, CityListComponent>.sortByPopulation);
		sorting_tab.tryAddButton("ui/Icons/iconLoyalty", "sort_by_loyalty", show, delegate
		{
			current_sort = sortByLoyalty;
		});
		sorting_tab.tryAddButton("ui/Icons/iconZones", "sort_by_area", show, delegate
		{
			current_sort = sortByArea;
		});
		sorting_tab.tryAddButton("ui/Icons/iconArmy", "sort_by_army", show, delegate
		{
			current_sort = sortByArmy;
		});
		sorting_tab.tryAddButton("ui/Icons/iconKingdom", "sort_by_kingdom", show, delegate
		{
			current_sort = sortByKingdom;
		});
	}

	private static int sortByKingdom(City p1, City p2)
	{
		return p2.kingdom.CompareTo(p1.kingdom);
	}

	private static int sortByArmy(City p1, City p2)
	{
		return p2.countWarriors().CompareTo(p1.countWarriors());
	}

	private static int sortByLoyalty(City p1, City p2)
	{
		return p2.getCachedLoyalty().CompareTo(p1.getCachedLoyalty());
	}

	private static int sortByArea(City p1, City p2)
	{
		return p2.zones.Count.CompareTo(p1.zones.Count);
	}
}
