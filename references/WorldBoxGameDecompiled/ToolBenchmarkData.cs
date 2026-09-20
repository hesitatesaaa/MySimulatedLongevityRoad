using System.Collections.Generic;

public class ToolBenchmarkData
{
	public string id;

	private const int MAXIMUM_VALUES = 64;

	private Queue<double> results = new Queue<double>(64);

	private Queue<long> results_counters = new Queue<long>(64);

	public double latest_time;

	public double latest_result;

	public double calculated_percentage;

	public long call_count;

	public long debug_value;

	public double last_max_value;

	public int max_value_ticks;

	public void newValue(int pValue)
	{
		debug_value = pValue;
	}

	public void newCount(long pValue)
	{
		call_count += pValue;
	}

	public void saveLastMaxValue(double pValue)
	{
		if (pValue > last_max_value || max_value_ticks <= 0)
		{
			last_max_value = pValue;
			max_value_ticks = 200;
		}
		if (max_value_ticks > 0)
		{
			max_value_ticks--;
		}
	}

	public long getAverageCount()
	{
		if (results_counters.Count == 0)
		{
			return 0L;
		}
		long num = 0L;
		foreach (long results_counter in results_counters)
		{
			num += results_counter;
		}
		return num / results_counters.Count;
	}

	public long getLastCount()
	{
		if (results_counters.Count == 0)
		{
			return 0L;
		}
		return results_counters.Peek();
	}

	public void saveAverageCounter()
	{
		if (results_counters.Count > 64)
		{
			results_counters.Dequeue();
		}
		results_counters.Enqueue(call_count);
		call_count = 0L;
	}

	public void start(double pTime)
	{
		latest_time = pTime;
	}

	public void end(double pTime)
	{
		latest_result = pTime;
		if (results.Count > 64)
		{
			results.Dequeue();
		}
		results.Enqueue(pTime);
	}

	public double getAverage()
	{
		double num = 0.0;
		foreach (double result in results)
		{
			num += result;
		}
		return num / (double)results.Count;
	}

	public void setResult(double pTime)
	{
		latest_time = pTime;
		latest_result = pTime;
	}
}
