using System.Collections.Generic;
using FMOD.Studio;

public class MusicBoxIdle
{
	private List<BaseSimObject> _toRemove = new List<BaseSimObject>();

	public Dictionary<BaseSimObject, EventInstance> currentAttachedSounds = new Dictionary<BaseSimObject, EventInstance>();

	private float _timer;

	public void update(float pElapsed)
	{
		if (_timer > 2f)
		{
			_timer -= pElapsed;
			return;
		}
		_timer = 2f;
		_toRemove.Clear();
		if (World.world.quality_changer.isLowRes())
		{
			clearAllSounds();
		}
		checkDeadSounds();
		if (!World.world.quality_changer.isLowRes())
		{
			updateBuildings();
		}
	}

	public virtual void checkDeadSounds()
	{
		foreach (BaseSimObject key in currentAttachedSounds.Keys)
		{
			bool flag = false;
			if (!key.isAlive())
			{
				flag = true;
			}
			if (flag)
			{
				_toRemove.Add(key);
			}
		}
		foreach (BaseSimObject item in _toRemove)
		{
			removeSound(item);
		}
	}

	private void updateBuildings()
	{
	}

	private void removeSound(BaseSimObject pObj)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		currentAttachedSounds.TryGetValue(pObj, out var value);
		if (((EventInstance)(ref value)).isValid())
		{
			((EventInstance)(ref value)).stop((STOP_MODE)0);
			((EventInstance)(ref value)).release();
			currentAttachedSounds.Remove(pObj);
		}
	}

	private void playAttachedSound(BaseSimObject pObject, string pSound)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (MusicBox.sounds_on)
		{
			currentAttachedSounds.TryGetValue(pObject, out var value);
			if (!((EventInstance)(ref value)).isValid())
			{
				currentAttachedSounds.Add(pObject, value);
			}
		}
	}

	private bool isPlaying(BaseSimObject pObject)
	{
		currentAttachedSounds.TryGetValue(pObject, out var value);
		if (((EventInstance)(ref value)).isValid())
		{
			return true;
		}
		return false;
	}

	public void clearAllSounds()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		foreach (EventInstance value in currentAttachedSounds.Values)
		{
			EventInstance current = value;
			((EventInstance)(ref current)).stop((STOP_MODE)0);
			((EventInstance)(ref current)).release();
		}
		currentAttachedSounds.Clear();
	}

	public int CountCurrentSounds()
	{
		return currentAttachedSounds.Count;
	}
}
