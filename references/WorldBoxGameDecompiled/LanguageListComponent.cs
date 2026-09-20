public class LanguageListComponent : ComponentListBase<LanguageListElement, Language, LanguageData, LanguageListComponent>
{
	protected override MetaType meta_type => MetaType.Language;

	protected override void setupSortingTabs()
	{
		genericMetaSortByAge(base.sortByAge);
		genericMetaSortByRenown(base.sortByRenown);
		genericMetaSortByPopulation(ComponentListBase<LanguageListElement, Language, LanguageData, LanguageListComponent>.sortByPopulation);
		genericMetaSortByKills(ComponentListBase<LanguageListElement, Language, LanguageData, LanguageListComponent>.sortByKills);
		genericMetaSortByDeath(ComponentListBase<LanguageListElement, Language, LanguageData, LanguageListComponent>.sortByDeaths);
		sorting_tab.tryAddButton("ui/Icons/iconVillages", "sort_by_villages", show, delegate
		{
			current_sort = sortByVillages;
		});
	}

	public static int sortByVillages(Language pObject1, Language pObject2)
	{
		return pObject2.cities.Count.CompareTo(pObject1.cities.Count);
	}
}
