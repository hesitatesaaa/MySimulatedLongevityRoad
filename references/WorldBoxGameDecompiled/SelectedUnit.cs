using System;
using System.Collections.Generic;

public static class SelectedUnit
{
	private static Actor _unit_main;

	private static HashSet<Actor> _units_hashset = new HashSet<Actor>();

	private static List<Actor> _units_list = new List<Actor>();

	private static SelectedUnitClearEvent _on_clear_events;

	private static int _selection_version;

	public static Actor unit => _unit_main;

	public static HashSet<Actor> getAllSelected()
	{
		return _units_hashset;
	}

	public static List<Actor> getAllSelectedList()
	{
		return _units_list;
	}

	public static bool multipleSelected()
	{
		return _units_hashset.Count > 1;
	}

	public static int countSelected()
	{
		return _units_hashset.Count;
	}

	public static bool isSet()
	{
		if (_units_hashset.Count == 0)
		{
			return false;
		}
		return true;
	}

	public static void selectMultiple(ListPool<Actor> pActors)
	{
		Actor actor = null;
		foreach (ref Actor pActor in pActors)
		{
			Actor current = pActor;
			select(current, pSetMainUnit: false);
			if (actor == null || current.data.created_time < actor.data.created_time)
			{
				actor = current;
			}
		}
		if (actor != null)
		{
			makeMainSelected(actor);
		}
	}

	public static bool select(Actor pActor, bool pSetMainUnit = true)
	{
		if (pSetMainUnit)
		{
			makeMainSelected(pActor);
		}
		if (_units_hashset.Add(pActor))
		{
			hashsetChanged();
		}
		return isSet();
	}

	public static void unselect(Actor pActor)
	{
		if (_units_hashset.Remove(pActor))
		{
			hashsetChanged();
		}
	}

	public static void clear()
	{
		_units_hashset.Clear();
		hashsetChanged();
		clearMain();
		_on_clear_events?.Invoke();
	}

	private static void hashsetChanged()
	{
		_units_list.Clear();
		_units_list.AddRange(_units_hashset);
		_units_list.Shuffle();
		_selection_version++;
	}

	public static bool isMainSelected(Actor pActor)
	{
		if (!isSet())
		{
			return false;
		}
		return _unit_main == pActor;
	}

	public static bool isSelected(Actor pActor)
	{
		if (!isSet())
		{
			return false;
		}
		return _units_hashset.Contains(pActor);
	}

	public static void subscribeClearEvent(SelectedUnitClearEvent pEvent)
	{
		_on_clear_events = (SelectedUnitClearEvent)Delegate.Combine(_on_clear_events, pEvent);
	}

	public static void removeSelected(Actor pActor)
	{
		if (_units_hashset.Remove(pActor))
		{
			hashsetChanged();
		}
		if (_unit_main == pActor)
		{
			clearMain();
			trySelectNewMain();
		}
	}

	private static void clearMain()
	{
		_unit_main = null;
	}

	private static void trySelectNewMain()
	{
		if (_units_hashset.Count == 0)
		{
			clear();
		}
		else
		{
			makeMainSelected(_units_hashset.GetRandom());
		}
	}

	public static void nextMainUnit()
	{
		if (isSet())
		{
			makeMainSelected(_units_list.LoopNext(_unit_main));
		}
	}

	public static void killSelected()
	{
		if (!isSet())
		{
			return;
		}
		using ListPool<Actor> listPool = new ListPool<Actor>(_units_hashset);
		foreach (ref Actor item in listPool)
		{
			Actor current = item;
			current.getHit(current.getMaxHealth() * 2, pFlash: true, AttackType.Divine);
		}
		clear();
	}

	public static void makeMainSelected(Actor pActor)
	{
		if (_unit_main != pActor)
		{
			pActor.makeSpawnSound(pFromUI: true);
		}
		_unit_main = pActor;
	}

	public static int getSelectionVersion()
	{
		return _selection_version;
	}
}
