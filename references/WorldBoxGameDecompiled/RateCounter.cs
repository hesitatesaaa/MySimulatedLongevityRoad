using System.Collections.Generic;

public class RateCounter
{
	private readonly List<RateCounterData> _timestamps = new List<RateCounterData>();

	private string _id;

	private double _ticks;

	private int _total;

	public string id => _id;

	public RateCounter(string pID, int pTicks = 60)
	{
		_id = pID;
		_ticks = pTicks;
	}

	public void reset()
	{
		_timestamps.Clear();
		_total = 0;
	}

	public void registerEvent()
	{
	}

	public void registerEvent(double pValue)
	{
	}

	private double getTime()
	{
		return World.world.getCurWorldTime();
	}

	public double getValuesAll()
	{
		double num = 0.0;
		foreach (RateCounterData timestamp in _timestamps)
		{
			num += timestamp.value;
		}
		return num;
	}

	public int getEventsPerTick()
	{
		double time = getTime();
		cleanupOldEvents(time);
		return _timestamps.Count;
	}

	private void cleanupOldEvents(double tNow)
	{
		if (_timestamps.Count != 0)
		{
			_timestamps.RemoveAll((RateCounterData t) => tNow - t.timestamp > _ticks);
		}
	}

	public string getInfo()
	{
		return $"{getEventsPerTick()} | tot: {_total}";
	}

	public int getTotal()
	{
		return _total;
	}

	public int getEventsPerMinute()
	{
		return getEventsPerTick();
	}
}
