using System.Collections.Generic;

namespace life.taxi;

public class TaxiRequest
{
	private WorldTile _tile_target;

	private WorldTile _tile_start;

	private readonly HashSet<Actor> _actors = new HashSet<Actor>();

	private Boat _boat;

	public TaxiRequestState state;

	private Kingdom _kingdom;

	public TaxiRequest(Actor pActor, Kingdom pKingdom, WorldTile pStartTile, WorldTile pTargetTile)
	{
		_kingdom = pKingdom;
		addActor(pActor);
		_tile_start = pStartTile;
		_tile_target = pTargetTile;
		setState(TaxiRequestState.Pending);
	}

	public WorldTile getTileTarget()
	{
		return _tile_target;
	}

	public WorldTile getTileStart()
	{
		return _tile_start;
	}

	public bool isSameKingdom(Kingdom pKingdom)
	{
		return _kingdom == pKingdom;
	}

	public HashSet<Actor> getActors()
	{
		return _actors;
	}

	public int countActors()
	{
		return _actors.Count;
	}

	public bool hasActor(Actor pActor)
	{
		if (_actors.Contains(pActor))
		{
			return true;
		}
		return false;
	}

	public bool addActor(Actor pActor)
	{
		return _actors.Add(pActor);
	}

	public void cancel()
	{
		setState(TaxiRequestState.Finished);
		clear();
	}

	public void finish()
	{
		setState(TaxiRequestState.Finished);
		clear();
	}

	private void checkList()
	{
		using ListPool<Actor> listPool = new ListPool<Actor>(_actors.Count);
		foreach (Actor actor in _actors)
		{
			bool flag = false;
			if (actor.isRekt())
			{
				flag = true;
			}
			else if (!actor.current_tile.isSameIsland(_tile_start))
			{
				flag = true;
			}
			else if (actor.current_tile.isSameIsland(_tile_target))
			{
				flag = true;
			}
			if (flag)
			{
				listPool.Add(actor);
			}
		}
		_actors.ExceptWith(listPool);
	}

	public void removeDeadUnits()
	{
		using ListPool<Actor> listPool = new ListPool<Actor>(_actors.Count);
		foreach (Actor actor in _actors)
		{
			if (actor.isRekt())
			{
				listPool.Add(actor);
			}
		}
		if (listPool.Count != 0)
		{
			_actors.ExceptWith(listPool);
		}
	}

	public bool isAlreadyUsedByBoat(Actor pBoatActor)
	{
		if (isState(TaxiRequestState.Assigned) && isStillLegit() && isAssignedToBoat(pBoatActor))
		{
			return true;
		}
		return false;
	}

	public bool isAssignedToBoat(Actor pBoatActor)
	{
		return _boat.actor == pBoatActor;
	}

	public bool isStillLegit()
	{
		if (!isState(TaxiRequestState.Pending) && !hasAssignedBoat())
		{
			return false;
		}
		if (isState(TaxiRequestState.Finished))
		{
			return false;
		}
		if ((isState(TaxiRequestState.Assigned) || isState(TaxiRequestState.Loading) || isState(TaxiRequestState.Transporting)) && hasAssignedBoat() && _boat.hasPassengers())
		{
			return true;
		}
		checkList();
		if (_actors.Count == 0)
		{
			return false;
		}
		return true;
	}

	public bool hasAssignedBoat()
	{
		if (_boat != null)
		{
			return !_boat.actor.isRekt();
		}
		return false;
	}

	public void assign(Boat pTaxi)
	{
		_boat = pTaxi;
		setState(TaxiRequestState.Assigned);
	}

	public Boat getBoat()
	{
		return _boat;
	}

	public bool isBoatNearDock()
	{
		return _boat.isNearDock();
	}

	public void setState(TaxiRequestState pState)
	{
		state = pState;
	}

	public void cancelForLatePassengers()
	{
		using ListPool<Actor> listPool = new ListPool<Actor>(_actors.Count);
		foreach (Actor actor in _actors)
		{
			if (!actor.is_inside_boat || actor.inside_boat != _boat)
			{
				listPool.Add(actor);
			}
		}
		_actors.ExceptWith(listPool);
	}

	public bool embarkToBoat(Actor pActor)
	{
		return _actors.Remove(pActor);
	}

	public bool everyoneEmbarked()
	{
		checkList();
		foreach (Actor actor in _actors)
		{
			if (!_boat.hasPassenger(actor))
			{
				return false;
			}
		}
		return true;
	}

	public bool isState(TaxiRequestState pState)
	{
		return state == pState;
	}

	public void clear()
	{
		_actors.Clear();
		_boat = null;
		_tile_target = null;
		_tile_start = null;
		_kingdom = null;
	}
}
