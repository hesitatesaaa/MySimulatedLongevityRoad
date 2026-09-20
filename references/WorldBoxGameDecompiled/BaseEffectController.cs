using System.Collections.Generic;
using UnityEngine;

public class BaseEffectController : BaseMapObject
{
	public Transform prefab;

	private int _active_index;

	private readonly List<BaseEffect> _list = new List<BaseEffect>();

	private float _timer;

	private float _timer_interval = 1f;

	private bool _object_limit_used;

	private int _object_limit;

	private bool _limit_unload;

	public bool useInterval = true;

	public EffectAsset asset;

	internal override void create()
	{
		base.create();
		_timer_interval = 0.9f;
	}

	public void setLimits(int pLimitObjects, bool pLimitUnload)
	{
		if (pLimitObjects > 0)
		{
			_object_limit_used = true;
		}
		_object_limit = pLimitObjects;
		_limit_unload = pLimitUnload;
	}

	public BaseEffect GetObject()
	{
		BaseEffect baseEffect = null;
		List<BaseEffect> list = _list;
		if (list.Count > _active_index)
		{
			baseEffect = list[_active_index];
		}
		else
		{
			baseEffect = ((Component)Object.Instantiate<Transform>(prefab)).gameObject.GetComponent<BaseEffect>();
			addNewObject(baseEffect);
			if (!baseEffect.created)
			{
				baseEffect.create();
			}
			list.Add(baseEffect);
			baseEffect.effectIndex = list.Count;
		}
		_active_index++;
		baseEffect.activate();
		return baseEffect;
	}

	public int getActiveIndex()
	{
		return _active_index;
	}

	internal void addNewObject(BaseEffect pEffect)
	{
		pEffect.controller = this;
		((Component)pEffect).transform.parent = ((Component)this).transform;
	}

	public void killObject(BaseEffect pObject)
	{
		if (pObject.active)
		{
			makeInactive(pObject);
			List<BaseEffect> list = _list;
			int num = pObject.effectIndex - 1;
			int num2 = _active_index - 1;
			if (num != num2)
			{
				BaseEffect baseEffect = list[num2];
				list[num2] = pObject;
				list[num] = baseEffect;
				pObject.effectIndex = num2 + 1;
				baseEffect.effectIndex = num + 1;
			}
			if (_active_index > 0)
			{
				_active_index--;
			}
		}
	}

	private void makeInactive(BaseEffect pObject)
	{
		pObject.deactivate();
	}

	private void debugString()
	{
		string text = "";
		List<BaseEffect> list = _list;
		for (int i = 0; i < list.Count; i++)
		{
			text = ((!list[i].active) ? (text + "x") : (text + "O"));
		}
		Debug.Log((object)(text + " ::: " + _active_index));
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		updateChildren(pElapsed);
		updateSpawn(pElapsed);
	}

	private void updateSpawn(float pElapsed)
	{
		if (!World.world.isPaused() && useInterval)
		{
			if (_timer > 0f)
			{
				_timer -= pElapsed;
				return;
			}
			_timer = _timer_interval;
			spawn();
		}
	}

	private void updateChildren(float pElapsed)
	{
		List<BaseEffect> list = _list;
		for (int num = _active_index - 1; num >= 0; num--)
		{
			BaseEffect baseEffect = list[num];
			if (baseEffect.created && baseEffect.active)
			{
				baseEffect.update(pElapsed);
			}
		}
	}

	public virtual void spawn()
	{
	}

	public BaseEffect spawnNew()
	{
		if (isLimitReached())
		{
			if (!_limit_unload)
			{
				return null;
			}
			killOldest();
		}
		BaseEffect baseEffect = GetObject();
		if ((Object)(object)baseEffect.sprite_animation != (Object)null)
		{
			baseEffect.sprite_animation.resetAnim();
		}
		return baseEffect;
	}

	private void killOldest()
	{
		if (_list.Count == 0)
		{
			return;
		}
		BaseEffect pObject = _list[0];
		double num = double.MaxValue;
		foreach (BaseEffect item in _list)
		{
			if (item.timestamp_spawned < num)
			{
				pObject = item;
				num = item.timestamp_spawned;
			}
		}
		killObject(pObject);
	}

	internal bool isLimitReached()
	{
		if (_object_limit_used && _active_index >= _object_limit)
		{
			return true;
		}
		return false;
	}

	internal void clear()
	{
		List<BaseEffect> list = _list;
		for (int i = 0; i < list.Count; i++)
		{
			BaseEffect pObject = list[i];
			makeInactive(pObject);
		}
		_active_index = 0;
	}

	public bool isAnyActive()
	{
		return _active_index > 0;
	}

	internal void debug(DebugTool pTool)
	{
		pTool.setText(((Object)this).name, _active_index + "/" + _list.Count, 0f, pShowBar: false, 0L);
	}

	internal List<BaseEffect> getList()
	{
		return _list;
	}
}
