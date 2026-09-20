using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[Serializable]
public class CitizenJobs
{
	public Dictionary<CitizenJobAsset, int> jobs = new Dictionary<CitizenJobAsset, int>();

	public Dictionary<CitizenJobAsset, int> occupied = new Dictionary<CitizenJobAsset, int>();

	private int _total_tasks;

	public void clear()
	{
		jobs.Clear();
		occupied.Clear();
	}

	public int getTotalTasks()
	{
		return _total_tasks;
	}

	public bool hasAnyTask()
	{
		return _total_tasks > 0;
	}

	public void clearJobs()
	{
		_total_tasks = 0;
		jobs.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void addToJob(CitizenJobAsset pJobAsset, int pValue)
	{
		_total_tasks += pValue;
		if (jobs.TryGetValue(pJobAsset, out var value))
		{
			jobs[pJobAsset] = value + pValue;
		}
		else
		{
			jobs.Add(pJobAsset, pValue);
		}
	}

	public bool continueJob(CitizenJobAsset pJobAsset)
	{
		return jobs.ContainsKey(pJobAsset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int countOccupied(CitizenJobAsset pJobAsset)
	{
		if (occupied.TryGetValue(pJobAsset, out var value))
		{
			return value;
		}
		return 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int countCurrentJobs(CitizenJobAsset pJobAsset)
	{
		if (jobs.TryGetValue(pJobAsset, out var value))
		{
			return value;
		}
		return 0;
	}

	public bool hasJob(CitizenJobAsset pJobAsset)
	{
		if (!jobs.TryGetValue(pJobAsset, out var value))
		{
			return false;
		}
		if (value == 0)
		{
			return false;
		}
		if (occupied.ContainsKey(pJobAsset) && occupied[pJobAsset] >= value)
		{
			return false;
		}
		return true;
	}

	public void takeJob(CitizenJobAsset pJobAsset)
	{
		if (!occupied.ContainsKey(pJobAsset))
		{
			occupied.Add(pJobAsset, 1);
		}
		else
		{
			occupied[pJobAsset]++;
		}
	}

	public void freeJob(CitizenJobAsset pJobAsset)
	{
		if (occupied.TryGetValue(pJobAsset, out var value))
		{
			if (value > 0)
			{
				occupied[pJobAsset]--;
			}
		}
		else
		{
			occupied.Add(pJobAsset, 0);
		}
	}
}
