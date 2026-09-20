public class KingdomListComponent : ComponentListBase<KingdomListElement, Kingdom, KingdomData, KingdomListComponent>
{
	protected override MetaType meta_type => MetaType.Kingdom;

	protected override void setupSortingTabs()
	{
		genericMetaSortByAge(base.sortByAge);
		genericMetaSortByRenown(base.sortByRenown);
		genericMetaSortByPopulation(ComponentListBase<KingdomListElement, Kingdom, KingdomData, KingdomListComponent>.sortByPopulation);
		sorting_tab.tryAddButton("ui/Icons/iconArmy", "sort_by_army", show, delegate
		{
			current_sort = sortByArmy;
		});
		sorting_tab.tryAddButton("ui/Icons/iconChildren", "sort_by_children", show, delegate
		{
			current_sort = sortByChildren;
		});
		sorting_tab.tryAddButton("ui/Icons/iconVillages", "sort_by_villages", show, delegate
		{
			current_sort = sortByCities;
		});
		sorting_tab.tryAddButton("ui/Icons/iconZones", "sort_by_area", show, delegate
		{
			current_sort = sortByArea;
		});
	}

	private static int sortByArea(Kingdom p1, Kingdom p2)
	{
		return p2.countZones().CompareTo(p1.countZones());
	}

	public static int sortByArmy(Kingdom p1, Kingdom p2)
	{
		return p2.countTotalWarriors().CompareTo(p1.countTotalWarriors());
	}

	private static int sortByChildren(Kingdom p1, Kingdom p2)
	{
		return p2.countChildren().CompareTo(p1.countChildren());
	}

	private static int sortByCities(Kingdom p1, Kingdom p2)
	{
		return p2.countCities().CompareTo(p1.countCities());
	}
}
