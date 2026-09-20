public class SubspeciesListComponent : ComponentListSapient<SubspeciesListElement, Subspecies, SubspeciesData, SubspeciesListComponent>
{
	protected override MetaType meta_type => MetaType.Subspecies;

	protected override void setupSortingTabs()
	{
		genericMetaSortByAge(base.sortByAge);
		genericMetaSortByRenown(base.sortByRenown);
		genericMetaSortByPopulation(ComponentListBase<SubspeciesListElement, Subspecies, SubspeciesData, SubspeciesListComponent>.sortByPopulation);
		genericMetaSortByKills(ComponentListBase<SubspeciesListElement, Subspecies, SubspeciesData, SubspeciesListComponent>.sortByKills);
		genericMetaSortByDeath(ComponentListBase<SubspeciesListElement, Subspecies, SubspeciesData, SubspeciesListComponent>.sortByDeaths);
		sorting_tab.tryAddButton("ui/Icons/iconChildren", "sort_by_children", show, delegate
		{
			current_sort = sortByChildren;
		});
		sorting_tab.tryAddButton("ui/Icons/iconHelixDNA", "sort_by_species", show, delegate
		{
			current_sort = sortBySpecies;
		});
		sorting_tab.tryAddButton("ui/Icons/iconFamily", "sort_by_families", show, delegate
		{
			current_sort = sortByFamilies;
		});
	}

	public static int sortByChildren(Subspecies pObject1, Subspecies pObject2)
	{
		return pObject2.countChildren().CompareTo(pObject1.countChildren());
	}

	public static int sortBySpecies(Subspecies pObject1, Subspecies pObject2)
	{
		return pObject2.getActorAsset().GetHashCode().CompareTo(pObject1.getActorAsset().GetHashCode());
	}

	public static int sortByDead(Subspecies pObject1, Subspecies pObject2)
	{
		return pObject2.data.total_deaths.CompareTo(pObject1.data.total_deaths);
	}

	public static int sortByFamilies(Subspecies pObject1, Subspecies pObject2)
	{
		return pObject2.countCurrentFamilies().CompareTo(pObject1.countCurrentFamilies());
	}
}
