using System.Collections.Generic;
using UnityEngine;

public static class BattleKeeperManager
{
	private const int MAX_FRAMES = 8;

	private static HashSet<BattleContainer> _hashset;

	private static readonly List<BattleContainer> _to_remove = new List<BattleContainer>();

	public static void clear()
	{
		if (_hashset == null)
		{
			_hashset = new HashSet<BattleContainer>();
		}
		_hashset.Clear();
		_to_remove.Clear();
	}

	public static HashSet<BattleContainer> get()
	{
		return _hashset;
	}

	public static void update(float pElapsed)
	{
		if (_hashset.Count == 0)
		{
			return;
		}
		foreach (BattleContainer item in _hashset)
		{
			if (item.timer > 1f)
			{
				item.timer -= pElapsed;
				item.timer = Mathf.Clamp(item.timer, 1f, item.timer);
			}
			if (item.isRendered())
			{
				if (item.timer_animation > 0f)
				{
					item.timer_animation -= pElapsed;
				}
				else
				{
					item.timer_animation = 0.04f;
					item.frame++;
					if (item.frame >= 8)
					{
						item.frame = 7;
					}
				}
			}
			if (item.timeout > 0f)
			{
				item.timeout -= pElapsed;
				continue;
			}
			item.timer -= pElapsed;
			if (item.timer <= 0f)
			{
				_to_remove.Add(item);
			}
		}
		if (_to_remove.Count <= 0)
		{
			return;
		}
		foreach (BattleContainer item2 in _to_remove)
		{
			_hashset.Remove(item2);
		}
		_to_remove.Clear();
	}

	public static void addUnitKilled(Actor pActor)
	{
		BattleContainer battleContainer = null;
		foreach (BattleContainer item in _hashset)
		{
			if ((float)Toolbox.SquaredDistTile(item.tile, pActor.current_tile) < 1600f)
			{
				battleContainer = item;
				break;
			}
		}
		if (battleContainer != null || pActor.isSapient())
		{
			if (battleContainer == null)
			{
				battleContainer = new BattleContainer();
				battleContainer.tile = pActor.current_tile;
				_hashset.Add(battleContainer);
			}
			battleContainer.increaseDeaths(pActor);
			if (battleContainer.tile != pActor.current_tile && ((float)Toolbox.SquaredDistTile(battleContainer.tile, pActor.current_tile) < 25f || battleContainer.getDeathsTotal() < 3))
			{
				battleContainer.tile = pActor.current_tile;
			}
			battleContainer.timer = 1.2f;
			battleContainer.timeout = 1f;
			if (battleContainer.frame >= 7)
			{
				battleContainer.frame = 0;
			}
		}
	}
}
