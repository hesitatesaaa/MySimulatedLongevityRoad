public class PlotListComponent : ComponentListBase<PlotListElement, Plot, PlotData, PlotListComponent>
{
	protected override MetaType meta_type => MetaType.Plot;

	protected override void setupSortingTabs()
	{
		sorting_tab.tryAddButton("ui/Icons/iconAge", "sort_by_age", show, delegate
		{
			current_sort = sortByAge;
		});
		sorting_tab.tryAddButton("ui/Icons/iconPopulation", "sort_by_members", show, delegate
		{
			current_sort = sortBySupporters;
		});
	}

	public static int sortByAge(Plot pPlot1, Plot pPlot2)
	{
		return -pPlot2.data.created_time.CompareTo(pPlot1.data.created_time);
	}

	public static int sortBySupporters(Plot pPlot1, Plot pPlot2)
	{
		return pPlot2.units.Count.CompareTo(pPlot1.units.Count);
	}
}
