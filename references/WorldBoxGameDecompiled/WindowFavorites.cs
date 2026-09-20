using System.Collections.Generic;
using UnityEngine;

public class WindowFavorites : WindowListBaseActor
{
	private List<Actor> _temp_list_actor = new List<Actor>();

	protected override void setupSortingTabs()
	{
		sorting_tab.tryAddButton("ui/Icons/iconAge", "sort_by_age", show, delegate
		{
			current_sort = sortByAge;
		});
		sorting_tab.tryAddButton("ui/Icons/iconRenown", "sort_by_renown", show, delegate
		{
			current_sort = sortByRenown;
		});
		sorting_tab.tryAddButton("ui/Icons/iconLevels", "sort_by_level", show, delegate
		{
			current_sort = sortByLevel;
		});
		sorting_tab.tryAddButton("ui/Icons/iconKills", "sort_by_kills", show, delegate
		{
			current_sort = sortByKills;
		});
		sorting_tab.tryAddButton("ui/Icons/iconKingdom", "sort_by_kingdom", show, delegate
		{
			current_sort = sortByKingdom;
		});
	}

	protected override void show()
	{
		base.show();
		if ((Object)(object)_title_counter != (Object)null)
		{
			_title_counter.text = _temp_list_actor.Count.ToString();
		}
	}

	protected override List<Actor> getObjects()
	{
		_temp_list_actor.Clear();
		foreach (Actor unit in World.world.units)
		{
			if (unit.isAlive() && unit.isFavorite())
			{
				_temp_list_actor.Add(unit);
			}
		}
		return _temp_list_actor;
	}

	public static int sortByRenown(Actor pObject1, Actor pObject2)
	{
		return pObject2.data.renown.CompareTo(pObject1.data.renown);
	}

	public static int sortByKingdom(Actor pActor1, Actor pActor2)
	{
		return pActor2.kingdom.CompareTo(pActor1.kingdom);
	}

	public static int sortByAge(Actor pActor1, Actor pActor2)
	{
		return pActor2.getAge().CompareTo(pActor1.getAge());
	}

	public static int sortByLevel(Actor pActor1, Actor pActor2)
	{
		return pActor2.data.level.CompareTo(pActor1.data.level);
	}

	public static int sortByKills(Actor pActor1, Actor pActor2)
	{
		return pActor2.data.kills.CompareTo(pActor1.data.kills);
	}
}
