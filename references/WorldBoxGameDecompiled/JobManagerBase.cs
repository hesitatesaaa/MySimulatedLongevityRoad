using System.Collections.Generic;
using System.Threading.Tasks;

public class JobManagerBase<TBatch, T> where TBatch : Batch<T>, new()
{
	protected readonly List<TBatch> _batches_active = new List<TBatch>();

	private readonly Stack<TBatch> _batches_free = new Stack<TBatch>();

	public string id;

	public string benchmark_id;

	private Dictionary<string, double> _dict_benchmark_time = new Dictionary<string, double>();

	private Dictionary<string, int> _dict_benchmark_counter = new Dictionary<string, int>();

	public JobManagerBase(string pID)
	{
		id = pID;
	}

	public void updateBase(float pElapsed)
	{
		clearJobBenchmarks();
		updateBaseJobsPre(pElapsed);
		updateBaseJobsParallel(pElapsed);
		updateBaseJobsPost(pElapsed);
		saveJobBenchmarks();
	}

	private void clearJobBenchmarks()
	{
		if (!Bench.bench_enabled)
		{
			return;
		}
		for (int i = 0; i < _batches_active.Count; i++)
		{
			TBatch val = _batches_active[i];
			for (int j = 0; j < val.jobs_pre.Count; j++)
			{
				Job<T> job = val.jobs_pre[j];
				job.time_benchmark = 0.0;
				job.counter = 0;
			}
			for (int k = 0; k < val.jobs_post.Count; k++)
			{
				Job<T> job2 = val.jobs_post[k];
				job2.time_benchmark = 0.0;
				job2.counter = 0;
			}
		}
	}

	private void saveJobBenchmarks()
	{
		if (!Bench.bench_enabled)
		{
			return;
		}
		_dict_benchmark_time.Clear();
		_dict_benchmark_counter.Clear();
		for (int i = 0; i < _batches_active.Count; i++)
		{
			TBatch val = _batches_active[i];
			checkListForBenchmark(val.jobs_pre);
			checkListForBenchmark(val.jobs_post);
		}
		foreach (var (text2, pValue) in _dict_benchmark_time)
		{
			int pCounter = _dict_benchmark_counter[text2];
			Bench.benchSave(text2, pValue, pCounter, benchmark_id);
			Bench.saveAverageCounter(text2, benchmark_id);
		}
	}

	private void checkListForBenchmark(List<Job<T>> pList)
	{
		for (int i = 0; i < pList.Count; i++)
		{
			Job<T> job = pList[i];
			if (!_dict_benchmark_time.ContainsKey(job.id))
			{
				_dict_benchmark_time.Add(job.id, 0.0);
				_dict_benchmark_counter.Add(job.id, 0);
			}
			_dict_benchmark_time[job.id] += job.time_benchmark;
			_dict_benchmark_counter[job.id] += job.counter;
		}
	}

	internal void updateBaseJobsPre(float pElapsed)
	{
		for (int i = 0; i < _batches_active.Count; i++)
		{
			_batches_active[i].updateJobsPre(pElapsed);
		}
	}

	internal void updateBaseJobsPost(float pElapsed)
	{
		for (int i = 0; i < _batches_active.Count; i++)
		{
			_batches_active[i].updateJobsPost(pElapsed);
		}
	}

	internal void updateBaseJobsParallel(float pElapsed)
	{
		clearParallelResults();
		Bench.bench("update_jobs_parallel", benchmark_id);
		if (Config.parallel_jobs_updater)
		{
			Parallel.ForEach(_batches_active, World.world.parallel_options, delegate(TBatch pBatch)
			{
				pBatch.updateJobsParallel(pElapsed);
			});
		}
		else
		{
			List<TBatch> batches_active = _batches_active;
			int count = batches_active.Count;
			for (int num = 0; num < count; num++)
			{
				batches_active[num].updateJobsParallel(pElapsed);
			}
		}
		Bench.benchEnd("update_jobs_parallel", benchmark_id, pSaveCounter: false, 0L);
		applyParallelResults();
	}

	internal void clearParallelResults()
	{
		Bench.bench("clear_parallel_results", benchmark_id);
		for (int i = 0; i < _batches_active.Count; i++)
		{
			_batches_active[i].clearParallelResults?.Invoke();
		}
		Bench.benchEnd("clear_parallel_results", benchmark_id, pSaveCounter: false, 0L);
	}

	internal void applyParallelResults()
	{
		Bench.bench("apply_parallel_results", benchmark_id);
		for (int i = 0; i < _batches_active.Count; i++)
		{
			_batches_active[i].applyParallelResults?.Invoke();
		}
		Bench.benchEnd("apply_parallel_results", benchmark_id, pSaveCounter: false, 0L);
	}

	internal void removeObject(T pObject, TBatch pBatch)
	{
		pBatch.remove(pObject);
		checkFree(pBatch);
	}

	protected TBatch newBatch()
	{
		TBatch val = new TBatch();
		_batches_active.Add(val);
		return val;
	}

	internal virtual void addNewObject(T pObject)
	{
		TBatch batch = getBatch();
		batch.add(pObject);
		batch.main.checkAddRemove();
		if (batch.main.Count >= JobConst.MAX_ELEMENTS)
		{
			batch.free_slots = false;
			_batches_free.Pop();
		}
	}

	internal TBatch getBatch()
	{
		if (_batches_free.Count == 0)
		{
			TBatch val = newBatch();
			val.batch_id = _batches_active.Count;
			makeFree(val);
			return val;
		}
		TBatch val2 = _batches_free.Peek();
		if (val2.main.Count == 0)
		{
			_batches_active.Add(val2);
		}
		return val2;
	}

	protected void checkFree(TBatch pBatch)
	{
		pBatch.main.checkAddRemove();
		if (pBatch.main.Count < JobConst.MAX_ELEMENTS)
		{
			makeFree(pBatch);
		}
		if (pBatch.main.Count == 0)
		{
			_batches_active.Remove(pBatch);
		}
	}

	protected virtual void makeFree(TBatch pBatch)
	{
		if (!pBatch.free_slots)
		{
			pBatch.free_slots = true;
			_batches_free.Push(pBatch);
		}
	}

	internal void clear()
	{
		_batches_free.Clear();
		for (int i = 0; i < _batches_active.Count; i++)
		{
			TBatch val = _batches_active[i];
			val.clear();
			val.free_slots = false;
			makeFree(val);
		}
	}

	internal void clearHelperLists()
	{
		for (int i = 0; i < _batches_active.Count; i++)
		{
			_batches_active[i].clearHelperLists();
		}
	}

	public void debug(DebugTool pTool)
	{
		int num = 0;
		for (int i = 0; i < _batches_active.Count; i++)
		{
			TBatch val = _batches_active[i];
			num += val.main.Count;
		}
		pTool.setText("batches all", _batches_active.Count, 0f, pShowBar: false, 0L);
		pTool.setText("objects", num, 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("parallel_jobs_updater_on", Config.parallel_jobs_updater, 0f, pShowBar: false, 0L);
	}

	public string debugBatchCount()
	{
		return _batches_active.Count + " / " + _batches_free.Count;
	}

	public string debugJobCount()
	{
		int num = 0;
		foreach (TBatch item in _batches_active)
		{
			num += item.jobs_post.Count;
			num += item.jobs_pre.Count;
			num += item.jobs_parallel.Count;
		}
		return num.ToString();
	}
}
