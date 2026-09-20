using UnityEngine;

public class FamilyListComponent : ComponentListSapient<FamilyListElement, Family, FamilyData, FamilyListComponent>
{
	private IMetaWithFamiliesWindow _families_window;

	protected override MetaType meta_type => MetaType.Family;

	protected override bool change_asset_sort_order => _families_window == null;

	protected override void create()
	{
		base.create();
		_families_window = ((Component)this).GetComponentInParent<IMetaWithFamiliesWindow>();
		if (_families_window != null)
		{
			get_objects_delegate = (FamilyListComponent _) => _families_window.getFamilies();
		}
	}

	protected override void setupSortingTabs()
	{
		genericMetaSortByAge(base.sortByAge);
		genericMetaSortByRenown(base.sortByRenown);
		genericMetaSortByPopulation(ComponentListBase<FamilyListElement, Family, FamilyData, FamilyListComponent>.sortByPopulation);
		genericMetaSortByKills(ComponentListBase<FamilyListElement, Family, FamilyData, FamilyListComponent>.sortByKills);
		genericMetaSortByDeath(ComponentListBase<FamilyListElement, Family, FamilyData, FamilyListComponent>.sortByDeaths);
		sorting_tab.tryAddButton("ui/Icons/iconAdults", "sort_by_adults", show, delegate
		{
			current_sort = sortByAdults;
		});
		sorting_tab.tryAddButton("ui/Icons/iconChildren", "sort_by_children", show, delegate
		{
			current_sort = sortByChildren;
		});
		sorting_tab.tryAddButton("ui/Icons/iconHelixDNA", "sort_by_species", show, delegate
		{
			current_sort = sortBySpecies;
		});
	}

	public override bool isEmpty()
	{
		if (_families_window != null)
		{
			return !_families_window.hasFamilies();
		}
		return base.isEmpty();
	}

	public static int sortByAdults(Family pObject1, Family pObject2)
	{
		return pObject2.countAdults().CompareTo(pObject1.countAdults());
	}

	public static int sortByChildren(Family pObject1, Family pObject2)
	{
		return pObject2.countChildren().CompareTo(pObject1.countChildren());
	}

	public static int sortBySpecies(Family pObject1, Family pObject2)
	{
		return pObject2.getActorAsset().GetHashCode().CompareTo(pObject1.getActorAsset().GetHashCode());
	}
}
