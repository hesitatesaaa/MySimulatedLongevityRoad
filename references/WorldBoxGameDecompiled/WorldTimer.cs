using System;
using UnityEngine;

public class WorldTimer
{
	public bool isActive;

	private bool isStopWatch;

	private Action callback;

	private float interval;

	internal float timer;

	public WorldTimer(float pInterval, Action pCallback)
	{
		interval = pInterval;
		callback = pCallback;
		timer = interval;
	}

	public void setTime(float pNewTime)
	{
		timer = pNewTime;
	}

	internal void setInterval(float pInterval)
	{
		interval = pInterval;
	}

	public WorldTimer(float pInterval, bool pStopWatch)
	{
		isStopWatch = pStopWatch;
		interval = pInterval;
		timer = 0f;
		isActive = false;
	}

	public void startTimer(float pRate = -1f)
	{
		if (pRate != -1f)
		{
			interval = pRate;
		}
		timer = interval;
	}

	public void stop()
	{
		isActive = false;
	}

	public void update(float pElapsed = -1f)
	{
		if (pElapsed == -1f)
		{
			pElapsed = Time.deltaTime;
		}
		if (isStopWatch)
		{
			if (timer > 0f)
			{
				timer -= pElapsed;
				isActive = true;
			}
			else
			{
				isActive = false;
			}
		}
		else if (timer > 0f)
		{
			timer -= pElapsed;
		}
		else
		{
			timer = interval;
			callback();
		}
	}
}
