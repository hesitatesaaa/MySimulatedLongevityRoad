using UnityEngine;

public class WarListComponent : ComponentListBase<WarListElement, War, WarData, WarListComponent>
{
	protected override MetaType meta_type => MetaType.War;

	protected override void setupSortingTabs()
	{
		SortButton sortButton = sorting_tab.tryAddButton("ui/Icons/iconAge", "sort_by_age", show, delegate
		{
			current_sort = sortByAge;
		});
		sorting_tab.tryAddButton("ui/Icons/iconClock", "sort_by_duration", show, delegate
		{
			current_sort = sortByDuration;
		});
		if (getCurrentFilter() != ListItemsFilter.OnlyAlive)
		{
			sorting_tab.tryAddButton("ui/Icons/iconDeadKingdom", "sort_by_ended", show, delegate
			{
				current_sort = sortByEndedTime;
			});
		}
		sorting_tab.tryAddButton("ui/Icons/iconRenown", "sort_by_renown", show, delegate
		{
			current_sort = sortByRenown;
		});
		sorting_tab.tryAddButton("ui/Icons/iconArmy", "sort_by_army", show, delegate
		{
			current_sort = sortByArmy;
		});
		sorting_tab.tryAddButton("ui/Icons/iconKills", "sort_by_dead", show, delegate
		{
			current_sort = sortByDead;
		});
		sorting_tab.tryAddButton("ui/Icons/iconPopulation", "sort_by_population", show, delegate
		{
			current_sort = sortByPopulation;
		});
		if ((Object)(object)sortButton != (Object)null)
		{
			sortButton.click();
			sortButton.click();
		}
	}

	public static int sortByRenown(War pWar1, War pWar2)
	{
		if (sortByEnded(pWar1, pWar2) != 0)
		{
			return sortByEnded(pWar1, pWar2);
		}
		return pWar2.getRenown().CompareTo(pWar1.getRenown());
	}

	public static int sortByDuration(War pWar1, War pWar2)
	{
		if (sortByEnded(pWar1, pWar2) != 0)
		{
			return sortByEnded(pWar1, pWar2);
		}
		return -pWar2.getDuration().CompareTo(pWar1.getDuration());
	}

	public static int sortByAge(War pWar1, War pWar2)
	{
		if (sortByEnded(pWar1, pWar2) != 0)
		{
			return sortByEnded(pWar1, pWar2);
		}
		return -pWar2.data.created_time.CompareTo(pWar1.data.created_time);
	}

	public static int sortByArmy(War pWar1, War pWar2)
	{
		if (sortByEnded(pWar1, pWar2) != 0)
		{
			return sortByEnded(pWar1, pWar2);
		}
		return pWar1.countTotalArmy().CompareTo(pWar2.countTotalArmy());
	}

	public static int sortByPopulation(War pWar1, War pWar2)
	{
		if (sortByEnded(pWar1, pWar2) != 0)
		{
			return sortByEnded(pWar1, pWar2);
		}
		return pWar1.countTotalPopulation().CompareTo(pWar2.countTotalPopulation());
	}

	public static int sortByEndedTime(War pWar1, War pWar2)
	{
		if (sortByEnded(pWar1, pWar2) == 0)
		{
			return sortByAge(pWar1, pWar2);
		}
		return pWar2.data.died_time.CompareTo(pWar1.data.died_time);
	}

	public static int sortByDead(War pWar1, War pWar2)
	{
		if (sortByEnded(pWar1, pWar2) != 0)
		{
			return sortByEnded(pWar1, pWar2);
		}
		return pWar2.data.total_deaths.CompareTo(pWar1.getTotalDeaths());
	}

	private static int sortByEnded(War pWar1, War pWar2)
	{
		return pWar1.hasEnded().CompareTo(pWar2.hasEnded());
	}
}
