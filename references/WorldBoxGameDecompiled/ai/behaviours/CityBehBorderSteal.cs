using System.Collections.Generic;

namespace ai.behaviours;

public class CityBehBorderSteal : BehaviourActionCity
{
	private static List<TileZone> _zones = new List<TileZone>();

	public override BehResult execute(City pCity)
	{
		if (!DebugConfig.isOn(DebugOption.SystemZoneGrowth))
		{
			return BehResult.Stop;
		}
		if (!WorldLawLibrary.world_law_border_stealing.isEnabled())
		{
			return BehResult.Stop;
		}
		if (pCity.status.population == 0)
		{
			return BehResult.Stop;
		}
		if (pCity.buildings.Count == 0)
		{
			return BehResult.Stop;
		}
		for (int i = 0; i < 3; i++)
		{
			if (tryStealZone(pCity))
			{
				break;
			}
		}
		return BehResult.Continue;
	}

	private bool tryStealZone(City pCity)
	{
		_zones.Clear();
		TileZone[] neighbours = pCity.buildings.GetRandom().current_tile.zone.neighbours;
		foreach (TileZone tileZone in neighbours)
		{
			if (tileZone.city != pCity && tileZone.city != null && !tileZone.hasAnyBuildingsInSet(BuildingList.Civs) && tileZone.city.kingdom.isEnemy(pCity.kingdom))
			{
				stealZone(tileZone, pCity);
				return true;
			}
		}
		return false;
	}

	private void stealZone(TileZone pZone, City pCity)
	{
		if (pZone.city != null)
		{
			pZone.city.removeZone(pZone);
		}
		pCity.addZone(pZone);
	}
}
