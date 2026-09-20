public class AllianceListComponent : ComponentListBase<AllianceListElement, Alliance, AllianceData, AllianceListComponent>
{
	protected override MetaType meta_type => MetaType.Alliance;

	protected override void setupSortingTabs()
	{
		genericMetaSortByAge(base.sortByAge);
		genericMetaSortByRenown(base.sortByRenown);
		genericMetaSortByPopulation(ComponentListBase<AllianceListElement, Alliance, AllianceData, AllianceListComponent>.sortByPopulation);
		sorting_tab.tryAddButton("ui/Icons/iconArmy", "sort_by_army", show, delegate
		{
			current_sort = sortByArmy;
		});
		sorting_tab.tryAddButton("ui/Icons/iconKingdomList", "sort_by_kingdoms", show, delegate
		{
			current_sort = sortByKingdoms;
		});
		sorting_tab.tryAddButton("ui/Icons/iconVillages", "sort_by_villages", show, delegate
		{
			current_sort = sortByVillages;
		});
	}

	public static int sortByArmy(Alliance pAlliance1, Alliance pAlliance2)
	{
		return pAlliance2.countWarriors().CompareTo(pAlliance1.countWarriors());
	}

	public static int sortByKingdoms(Alliance pAlliance1, Alliance pAlliance2)
	{
		return pAlliance2.countKingdoms().CompareTo(pAlliance1.countKingdoms());
	}

	public static int sortByVillages(Alliance pAlliance1, Alliance pAlliance2)
	{
		return pAlliance2.countCities().CompareTo(pAlliance1.countCities());
	}
}
