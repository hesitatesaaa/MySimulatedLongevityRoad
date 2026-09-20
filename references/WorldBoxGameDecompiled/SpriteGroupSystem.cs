using System;
using UnityEngine;

public class SpriteGroupSystem<T> : MonoBehaviour where T : GroupSpriteObject
{
	private T[] _sprites = new T[32];

	private int _total;

	internal T prefab;

	internal int count_active_debug;

	private int _count_total_debug;

	private int _used_index;

	private int _active_index;

	public bool turn_off_renderer;

	internal bool is_within_active_index => _active_index > _used_index;

	public virtual void create()
	{
		((Object)((Component)this).transform).name = "GroupSpriteController";
		((Component)this).transform.parent = ((Component)World.world).transform;
	}

	public T[] getAll()
	{
		return _sprites;
	}

	public void prepare()
	{
		_used_index = 0;
	}

	public virtual void update(float pElapsed)
	{
		finale();
	}

	private void finale()
	{
		clearLast();
		count_active_debug = _active_index;
		_count_total_debug = _total;
	}

	public void clearFull()
	{
		for (int i = 0; i < _active_index; i++)
		{
			disableSprite(_sprites[i]);
		}
		_active_index = 0;
		_used_index = 0;
	}

	public void clearLast()
	{
		int num = _active_index - _used_index;
		for (int i = 0; i < num; i++)
		{
			int num2 = _active_index - 1 - i;
			T val = _sprites[num2];
			disableSprite(val);
			deactivate(val);
		}
		_active_index -= num;
	}

	private void disableSprite(GroupSpriteObject pQ)
	{
		if (turn_off_renderer)
		{
			((Renderer)pQ.sprite_renderer).enabled = false;
		}
		else if (((Component)pQ).gameObject.activeSelf)
		{
			((Component)pQ).gameObject.SetActive(false);
		}
	}

	private void enableSprite(GroupSpriteObject pQ)
	{
		if (turn_off_renderer)
		{
			((Renderer)pQ.sprite_renderer).enabled = true;
		}
		else if (!((Component)pQ).gameObject.activeSelf)
		{
			((Component)pQ).gameObject.SetActive(true);
		}
	}

	public virtual void deactivate(T pObject)
	{
	}

	public virtual void checkActiveAction(T pObject)
	{
	}

	internal T getNext()
	{
		T val;
		if (is_within_active_index)
		{
			val = _sprites[_used_index];
		}
		else
		{
			if (_active_index < _total)
			{
				val = _sprites[_active_index];
				enableSprite(val);
			}
			else
			{
				val = createNew();
			}
			_active_index++;
		}
		checkActiveAction(val);
		_used_index++;
		return val;
	}

	public T[] getFastActiveList(int pPlannedSize)
	{
		int active_index = _active_index;
		if (active_index < pPlannedSize)
		{
			_used_index = active_index;
			int num = pPlannedSize - active_index;
			for (int i = 0; i < num; i++)
			{
				getNext();
			}
		}
		else
		{
			_used_index = pPlannedSize;
		}
		return _sprites;
	}

	protected virtual T createNew()
	{
		T val = Object.Instantiate<T>(prefab, ((Component)this).gameObject.transform);
		if (_total >= _sprites.Length)
		{
			T[] array = new T[_sprites.Length * 2];
			Array.Copy(_sprites, array, _sprites.Length);
			_sprites = array;
		}
		_sprites[_total++] = val;
		return val;
	}

	public int countActive()
	{
		return _active_index;
	}

	public void debug(DebugTool pTool)
	{
		pTool.setSeparator();
		pTool.setText("count_active", count_active_debug, 0f, pShowBar: false, 0L);
		pTool.setText("count_total", _count_total_debug, 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("active_len", _active_index, 0f, pShowBar: false, 0L);
		pTool.setText("used_index", _used_index, 0f, pShowBar: false, 0L);
	}
}
