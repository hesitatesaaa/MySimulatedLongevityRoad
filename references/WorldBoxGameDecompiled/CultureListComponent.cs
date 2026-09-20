public class CultureListComponent : ComponentListBase<CultureListElement, Culture, CultureData, CultureListComponent>
{
	protected override MetaType meta_type => MetaType.Culture;

	protected override void setupSortingTabs()
	{
		genericMetaSortByAge(base.sortByAge);
		genericMetaSortByRenown(base.sortByRenown);
		genericMetaSortByPopulation(ComponentListBase<CultureListElement, Culture, CultureData, CultureListComponent>.sortByPopulation);
		genericMetaSortByKills(ComponentListBase<CultureListElement, Culture, CultureData, CultureListComponent>.sortByKills);
		genericMetaSortByDeath(ComponentListBase<CultureListElement, Culture, CultureData, CultureListComponent>.sortByDeaths);
		sorting_tab.tryAddButton("ui/Icons/iconVillages", "sort_by_villages", show, delegate
		{
			current_sort = sortByVillages;
		});
	}

	public static int sortByVillages(Culture pCulture1, Culture pCulture2)
	{
		return pCulture2.countCities().CompareTo(pCulture1.countCities());
	}
}
