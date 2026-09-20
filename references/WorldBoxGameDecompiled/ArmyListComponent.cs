public class ArmyListComponent : ComponentListBase<ArmyListElement, Army, ArmyData, ArmyListComponent>
{
	protected override MetaType meta_type => MetaType.Army;

	protected override void setupSortingTabs()
	{
		genericMetaSortByAge(base.sortByAge);
		genericMetaSortByRenown(base.sortByRenown);
		genericMetaSortByPopulation(ComponentListBase<ArmyListElement, Army, ArmyData, ArmyListComponent>.sortByPopulation);
		genericMetaSortByKills(ComponentListBase<ArmyListElement, Army, ArmyData, ArmyListComponent>.sortByKills);
		genericMetaSortByDeath(ComponentListBase<ArmyListElement, Army, ArmyData, ArmyListComponent>.sortByDeaths);
		sorting_tab.tryAddButton("ui/Icons/iconKingdom", "sort_by_kingdom", show, delegate
		{
			current_sort = sortByKingdom;
		});
	}

	private static int sortByKingdom(Army p1, Army p2)
	{
		return p2.getKingdom().CompareTo(p1.getKingdom());
	}
}
