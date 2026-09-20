using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Batch<T>
{
	internal ObjectContainer<T> main;

	internal bool free_slots;

	private List<ObjectContainer<T>> containers = new List<ObjectContainer<T>>();

	internal List<Job<T>> jobs_pre = new List<Job<T>>();

	internal List<Job<T>> jobs_post = new List<Job<T>>();

	internal List<Job<T>> jobs_parallel = new List<Job<T>>();

	protected List<T> _list;

	protected T[] _array;

	protected int _count;

	protected float _elapsed;

	protected ObjectContainer<T> _cur_container;

	internal JobUpdater clearParallelResults;

	internal JobUpdater applyParallelResults;

	public int batch_id;

	public Batch()
	{
		createJobs();
		createHelpers();
	}

	public void updateJobsPre(float pElapsed)
	{
		_elapsed = pElapsed;
		List<Job<T>> list = jobs_pre;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			Job<T> job = list[i];
			_cur_container = job.container;
			if (job.current_skips > 0)
			{
				job.current_skips--;
			}
			else
			{
				runUpdater(job);
			}
		}
	}

	public void updateJobsParallel(float pElapsed)
	{
		List<Job<T>> list = jobs_parallel;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			Job<T> job = list[i];
			_cur_container = job.container;
			job.job_updater();
		}
	}

	public void updateJobsPost(float pElapsed)
	{
		_elapsed = pElapsed;
		List<Job<T>> list = jobs_post;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			Job<T> job = list[i];
			_cur_container = job.container;
			if (job.current_skips > 0)
			{
				job.current_skips--;
			}
			else
			{
				runUpdater(job);
			}
		}
	}

	private void runUpdater(Job<T> pObj)
	{
		double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
		pObj.job_updater();
		if (pObj.random_tick_skips > 0)
		{
			pObj.current_skips = Randy.randomInt(0, pObj.random_tick_skips);
		}
		double num = Time.realtimeSinceStartupAsDouble - realtimeSinceStartupAsDouble;
		pObj.time_benchmark += num;
		pObj.counter += _cur_container.Count;
	}

	internal virtual void prepare()
	{
		for (int i = 0; i < containers.Count; i++)
		{
			containers[i].doChecks();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void createJob(out ObjectContainer<T> pContainer, JobUpdater pJobUpdater, JobType pType, string pID, int pRandomTickSkips = 0)
	{
		pContainer = new ObjectContainer<T>();
		pContainer.prepareArray(JobConst.MAX_ELEMENTS);
		containers.Add(pContainer);
		if (pJobUpdater != null)
		{
			addJob(pContainer, pJobUpdater, pType, pID, pRandomTickSkips);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void addJob(ObjectContainer<T> pContainer, JobUpdater pJobUpdater, JobType pType, string pID, int pRandomTickSkips = 0)
	{
		switch (pType)
		{
		case JobType.Pre:
			putJob(pContainer, pJobUpdater, jobs_pre, pID, pRandomTickSkips);
			break;
		case JobType.Parallel:
			putJob(pContainer, pJobUpdater, jobs_parallel, pID, pRandomTickSkips);
			break;
		case JobType.Post:
			putJob(pContainer, pJobUpdater, jobs_post, pID, pRandomTickSkips);
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void putJob(ObjectContainer<T> pContainer, JobUpdater pJobUpdater, List<Job<T>> pListJobs, string pID, int pRandomTickSkips = 0)
	{
		pListJobs.Add(new Job<T>
		{
			container = pContainer,
			job_updater = pJobUpdater,
			id = pID,
			random_tick_skips = pRandomTickSkips
		});
	}

	internal virtual void clear()
	{
		for (int i = 0; i < containers.Count; i++)
		{
			containers[i].Clear();
		}
		if (_array != null)
		{
			Array.Clear(_array, 0, _array.Length);
		}
	}

	internal virtual void remove(T pObject)
	{
		for (int i = 0; i < containers.Count; i++)
		{
			containers[i].Remove(pObject);
		}
	}

	internal virtual void add(T pObject)
	{
		main.Add(pObject);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected bool check(ObjectContainer<T> pContainer)
	{
		if (pContainer.Count > 0 || pContainer.isDirtyContainer())
		{
			pContainer.checkAddRemove();
			_array = pContainer.getFastSimpleArray();
			_count = pContainer.Count;
			return true;
		}
		return false;
	}

	protected virtual void createJobs()
	{
	}

	protected virtual void createHelpers()
	{
	}

	public virtual void clearHelperLists()
	{
	}

	public void debug(DebugTool pTool)
	{
		pTool.setText("total", main.Count, 0f, pShowBar: false, 0L);
	}
}
