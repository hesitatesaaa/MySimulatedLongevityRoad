using System;
using System.Collections.Generic;

public class CombatActionHolder
{
	private readonly List<CombatActionAsset>[] _combat_action_pools = new List<CombatActionAsset>[Enum.GetValues(typeof(CombatActionPool)).Length];

	private bool _has_combat_actions;

	public void fillFromIDS(List<string> pIDs)
	{
		foreach (string pID in pIDs)
		{
			CombatActionAsset combatActionAsset = AssetManager.combat_action_library.get(pID);
			if (combatActionAsset == null)
			{
				continue;
			}
			CombatActionPool[] pools = combatActionAsset.pools;
			foreach (CombatActionPool combatActionPool in pools)
			{
				if (_combat_action_pools[(int)combatActionPool] == null)
				{
					_combat_action_pools[(int)combatActionPool] = new List<CombatActionAsset>();
				}
				_combat_action_pools[(int)combatActionPool].Add(combatActionAsset);
			}
		}
	}

	public List<CombatActionAsset> getPool(CombatActionPool pPool)
	{
		return _combat_action_pools[(int)pPool];
	}

	public void reset()
	{
		if (_has_combat_actions)
		{
			for (int i = 0; i < _combat_action_pools.Length; i++)
			{
				_combat_action_pools[i]?.Clear();
			}
			_has_combat_actions = false;
		}
	}

	public void mergeWith(CombatActionHolder pCombatActions)
	{
		for (int i = 0; i < pCombatActions._combat_action_pools.Length; i++)
		{
			List<CombatActionAsset> list = pCombatActions._combat_action_pools[i];
			if (list != null && list.Count != 0)
			{
				if (_combat_action_pools[i] == null)
				{
					_combat_action_pools[i] = new List<CombatActionAsset>();
				}
				_combat_action_pools[i].AddRange(list);
				_has_combat_actions = true;
			}
		}
	}

	public bool isEmpty()
	{
		return !_has_combat_actions;
	}

	public bool hasAny()
	{
		return _has_combat_actions;
	}
}
