using System.Collections.Generic;

public class CapturingZonesCalculator
{
	private static int _zoneTicks = 0;

	private static Queue<TileZone> _currentWave = new Queue<TileZone>();

	private static Queue<TileZone> _nextWave = new Queue<TileZone>();

	private static HashSet<TileZone> _waveChecked = new HashSet<TileZone>();

	public static void getListToDraw(City pCity, int pTicks, ListPool<TileZone> pResults)
	{
		pResults.Clear();
		TileZone tileZone = pCity.getTile()?.zone;
		if (tileZone == null)
		{
			tileZone = pCity.zones[0];
		}
		Queue<TileZone> queue = _currentWave;
		queue.Enqueue(tileZone);
		_zoneTicks = pTicks;
		while (queue.Count > 0 && _zoneTicks != 0)
		{
			TileZone tileZone2 = queue.Dequeue();
			check(tileZone2, pCity);
			pResults.Add(tileZone2);
			if (queue.Count == 0)
			{
				Queue<TileZone> nextWave = queue;
				queue = _nextWave;
				_nextWave = nextWave;
			}
		}
		_nextWave.Clear();
		_waveChecked.Clear();
		queue.Clear();
	}

	private static void check(TileZone pTargetZone, City pCity)
	{
		_zoneTicks--;
		_waveChecked.Add(pTargetZone);
		TileZone[] neighbours = pTargetZone.neighbours;
		foreach (TileZone tileZone in neighbours)
		{
			if (tileZone.city == pCity && !_waveChecked.Contains(tileZone))
			{
				_waveChecked.Add(tileZone);
				_nextWave.Enqueue(tileZone);
			}
		}
	}
}
