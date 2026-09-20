using System.Collections.Generic;

public abstract class CityZoneWorkerBase
{
	protected bool debug;

	protected Queue<ZoneConnection> _wave = new Queue<ZoneConnection>();

	protected Queue<ZoneConnection> _next_wave = new Queue<ZoneConnection>();

	protected HashSet<ZoneConnection> _zones_checked = new HashSet<ZoneConnection>();

	protected void prepareWave()
	{
		_wave.Clear();
		_next_wave.Clear();
	}

	internal virtual void clearAll()
	{
		_zones_checked.Clear();
		_wave.Clear();
		_next_wave.Clear();
		checkZoneDebug();
	}

	protected virtual void queueConnection(ZoneConnection pConnection, Queue<ZoneConnection> pWave, bool pSetChecked = false)
	{
		pWave.Enqueue(pConnection);
		if (pSetChecked)
		{
			_zones_checked.Add(pConnection);
		}
	}

	protected void checkZoneDebug()
	{
		if (debug)
		{
			World.world.zone_calculator.clearDebug();
		}
	}
}
