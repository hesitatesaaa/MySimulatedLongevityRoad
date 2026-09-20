using System.Collections.Generic;

public class CrabLimbGroup
{
	public CrabLimb crabLimb;

	private CrabLimbItem[] _list;

	private CrabLimbState _dmg_state;

	private Actor actor;

	private float _flicker_timer;

	private const float _flicker_interval = 0.15f;

	public CrabLimbGroup(CrabLimb pCrabLimb, Actor pActor)
	{
		actor = pActor;
		crabLimb = pCrabLimb;
		List<CrabLimbItem> list = new List<CrabLimbItem>();
		CrabLimbItem[] componentsInChildren = actor.avatar.GetComponentsInChildren<CrabLimbItem>(false);
		foreach (CrabLimbItem crabLimbItem in componentsInChildren)
		{
			if (crabLimbItem.crabLimb == crabLimb)
			{
				list.Add(crabLimbItem);
			}
		}
		_list = list.ToArray();
		_dmg_state = CrabLimbState.HighHP;
	}

	internal void update(float pElapsed)
	{
		if (_flicker_timer != 0f)
		{
			_flicker_timer -= pElapsed;
			if (_flicker_timer < 0f)
			{
				_flicker_timer = 0f;
			}
			float pProgress = 1f - _flicker_timer / 0.15f;
			CrabLimbItem[] list = _list;
			for (int i = 0; i < list.Length; i++)
			{
				list[i].flicker(pProgress);
			}
		}
	}

	internal void showDamage()
	{
		if (IsFlickering())
		{
			return;
		}
		int health = actor.getHealth();
		int maxHealth = actor.getMaxHealth();
		if ((float)health > (float)maxHealth * 0.7f)
		{
			if (_dmg_state == CrabLimbState.HighHP)
			{
				return;
			}
			_dmg_state = CrabLimbState.HighHP;
		}
		else if ((float)health > (float)maxHealth * 0.35f)
		{
			if (_dmg_state == CrabLimbState.MedHP)
			{
				return;
			}
			_dmg_state = CrabLimbState.MedHP;
		}
		else
		{
			if (_dmg_state == CrabLimbState.LowHP)
			{
				return;
			}
			_dmg_state = CrabLimbState.LowHP;
		}
		CrabLimbItem[] list = _list;
		for (int i = 0; i < list.Length; i++)
		{
			list[i].stateChange(_dmg_state);
		}
		_flicker_timer = 0.15f;
	}

	internal bool IsFlickering()
	{
		return _flicker_timer > 0f;
	}
}
