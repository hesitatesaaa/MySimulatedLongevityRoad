using System.Collections.Generic;
using UnityEngine;
using ai.behaviours;

public class BehKingCheckNewCityFoundation : BehaviourActionActor
{
	private const int MAX_MOVED = 6;

	private List<TileZone> _next_wave = new List<TileZone>();

	private List<TileZone> _wave = new List<TileZone>();

	private HashSet<TileZone> _checked_zones = new HashSet<TileZone>();

	private static Color _color1;

	private static Color _color2;

	private static Color _color3;

	private static Color _color4;

	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		uses_families = true;
		uses_cities = true;
		uses_kingdoms = true;
	}

	public override BehResult execute(Actor pActor)
	{
		Kingdom kingdom = pActor.kingdom;
		if (!kingdom.hasCapital())
		{
			return BehResult.Stop;
		}
		if (hasCitiesWithoutPopulation(kingdom))
		{
			return BehResult.Stop;
		}
		BehaviourActionBase<Actor>.world.city_zone_helper.city_place_finder.recalc();
		if (!BehaviourActionBase<Actor>.world.city_zone_helper.city_place_finder.hasPossibleZones())
		{
			return BehResult.Stop;
		}
		using ListPool<City> listPool = getCityListForExpansion(kingdom);
		if (listPool.Count == 0)
		{
			return BehResult.Stop;
		}
		TileZone tileZone = findZoneForExpansion(pActor, listPool, out var pCityExpandFrom);
		if (tileZone == null)
		{
			return BehResult.Stop;
		}
		City pNewCity = BehaviourActionBase<Actor>.world.cities.buildNewCity(pActor, tileZone);
		moveSomeUnitsToNewCity(pNewCity, pCityExpandFrom);
		return BehResult.Continue;
	}

	private bool hasCitiesWithoutPopulation(Kingdom pKingdom)
	{
		WorldTile tile = pKingdom.capital.getTile();
		if (tile == null)
		{
			return false;
		}
		foreach (City city in pKingdom.getCities())
		{
			if (city.countUnits() <= 30)
			{
				WorldTile tile2 = city.getTile();
				if (tile2 != null && tile2.reachableFrom(tile))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void moveSomeUnitsToNewCity(City pNewCity, City pFromCity)
	{
		int num = Mathf.Min(pFromCity.units.Count, 6);
		int num2 = 0;
		foreach (Actor item in pFromCity.units.LoopRandom())
		{
			if (isPossibleToMoveUnitToCity(item, pNewCity))
			{
				moveToCity(item, pNewCity);
				num2++;
				int num3 = checkUnitFamilyAndLovers(item, pNewCity, num2);
				num2 += num3;
				if (num2 >= num)
				{
					break;
				}
			}
		}
	}

	private int checkUnitFamilyAndLovers(Actor pActor, City pCity, int pMovedAlready)
	{
		int num = 0;
		if (pActor.hasLover())
		{
			Actor lover = pActor.lover;
			if (isPossibleToMoveUnitToCity(lover, pCity))
			{
				moveToCity(lover, pCity);
				num++;
			}
		}
		if (pActor.hasFamily())
		{
			foreach (Actor unit in pActor.family.units)
			{
				if (isPossibleToMoveUnitToCity(unit, pCity))
				{
					moveToCity(unit, pCity);
					num++;
				}
				if (num + pMovedAlready >= 6)
				{
					break;
				}
			}
		}
		return num;
	}

	private void moveToCity(Actor pActor, City pCity)
	{
		pActor.stopBeingWarrior();
		pActor.joinCity(pCity);
		pActor.cancelAllBeh();
	}

	private bool isPossibleToMoveUnitToCity(Actor pUnit, City pCity)
	{
		if (pUnit.isRekt())
		{
			return false;
		}
		if (!pUnit.isAdult())
		{
			return false;
		}
		if (pUnit.isCityLeader())
		{
			return false;
		}
		if (pUnit.isKing())
		{
			return false;
		}
		if (pUnit.isArmyGroupLeader())
		{
			return false;
		}
		if (pUnit.army != null)
		{
			return false;
		}
		if (pUnit.hasLover())
		{
			if (pUnit.lover.isKing())
			{
				return false;
			}
			if (pUnit.lover.isCityLeader())
			{
				return false;
			}
		}
		if (pUnit.city == pCity)
		{
			return false;
		}
		return true;
	}

	private TileZone findZoneForExpansion(Actor pActor, ListPool<City> pPossibleCitiesToExpandFrom, out City pCityExpandFrom)
	{
		pCityExpandFrom = null;
		TileZone tileZone = null;
		foreach (ref City item in pPossibleCitiesToExpandFrom)
		{
			City current = item;
			TileZone tileZone2 = findZoneForCityOnTheSameIsland2(pActor, current);
			if (tileZone2 != null)
			{
				pCityExpandFrom = current;
				tileZone = tileZone2;
				break;
			}
		}
		if (tileZone == null)
		{
			foreach (ref City item2 in pPossibleCitiesToExpandFrom)
			{
				City current2 = item2;
				if (current2.hasTransportBoats())
				{
					TileZone tileZone3 = findZoneForCityOnFarIsland(pActor, current2);
					if (tileZone3 != null)
					{
						pCityExpandFrom = current2;
						tileZone = tileZone3;
						break;
					}
				}
			}
		}
		return tileZone;
	}

	private TileZone findZoneForCityOnFarIsland(Actor pActor, City pCity)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		TileZone result = null;
		int num = int.MaxValue;
		WorldTile tile = pCity.getTile();
		if (tile == null)
		{
			return null;
		}
		Vector2Int pos = tile.pos;
		foreach (TileZone zone in BehaviourActionBase<Actor>.world.city_zone_helper.city_place_finder.zones)
		{
			int num2 = Toolbox.SquaredDistVec2(zone.centerTile.pos, pos);
			if (num2 <= num && tile.reachableFrom(zone.centerTile) && zone.checkCanSettleInThisBiomes(pActor.subspecies))
			{
				num = num2;
				result = zone;
			}
		}
		return result;
	}

	private TileZone findZoneForCityOnTheSameIsland2(Actor pActor, City pMainCity)
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		WorldTile tile = pMainCity.getTile();
		if (tile == null)
		{
			return null;
		}
		using ListPool<TileZone> listPool = new ListPool<TileZone>();
		bool flag = DebugConfig.isOn(DebugOption.CitySettleCalc);
		if (flag)
		{
			DebugHighlight.clear();
		}
		foreach (City city in pMainCity.kingdom.getCities())
		{
			if (city != pMainCity)
			{
				continue;
			}
			WorldTile tile2 = city.getTile();
			if (tile2 == null || !tile.isSameIsland(tile2))
			{
				continue;
			}
			foreach (TileZone neighbour_zone in city.neighbour_zones)
			{
				if (!neighbour_zone.hasCity())
				{
					_wave.Add(neighbour_zone);
					_checked_zones.Add(neighbour_zone);
				}
			}
		}
		int num = 0;
		while (_wave.Count > 0)
		{
			if (flag)
			{
				switch (num)
				{
				case 0:
					DebugHighlight.newHighlightList(_color1, _wave);
					break;
				case 1:
					DebugHighlight.newHighlightList(_color2, _wave);
					break;
				case 2:
					DebugHighlight.newHighlightList(_color3, _wave);
					break;
				case 3:
					DebugHighlight.newHighlightList(_color4, _wave);
					break;
				}
			}
			startWave(num, tile, listPool, pActor);
			if (_next_wave.Count > 0)
			{
				_wave.AddRange(_next_wave);
				_next_wave.Clear();
				num++;
				if (num >= 4)
				{
					break;
				}
			}
		}
		_wave.Clear();
		_checked_zones.Clear();
		if (listPool.Count == 0)
		{
			return null;
		}
		return listPool.GetRandom();
	}

	private void startWave(int pWave, WorldTile pCityTile, ListPool<TileZone> pGoodZones, Actor pActor)
	{
		List<TileZone> wave = _wave;
		HashSet<TileZone> checked_zones = _checked_zones;
		while (wave.Count > 0)
		{
			TileZone tileZone = wave.Pop();
			checked_zones.Add(tileZone);
			if (pWave > 2 && tileZone.isGoodForNewCity(pActor) && tileZone.centerTile.isSameIsland(pCityTile))
			{
				pGoodZones.Add(tileZone);
			}
			TileZone[] neighbours = tileZone.neighbours;
			foreach (TileZone tileZone2 in neighbours)
			{
				if (checked_zones.Add(tileZone2) && !tileZone2.hasCity())
				{
					_next_wave.Add(tileZone2);
				}
			}
		}
	}

	private ListPool<City> getCityListForExpansion(Kingdom pKingdom)
	{
		ListPool<City> listPool = new ListPool<City>(pKingdom.countCities());
		foreach (City city in pKingdom.getCities())
		{
			if (city.getTile() != null && city.status.population_adults >= 30 && !city.needSettlers())
			{
				listPool.Add(city);
			}
		}
		listPool.Shuffle();
		return listPool;
	}

	private TileZone findCityForMigration(City pCity)
	{
		WorldTile tile = pCity.getTile();
		if (tile == null)
		{
			return null;
		}
		foreach (City item in pCity.kingdom.getCities().LoopRandom())
		{
			if (item == pCity)
			{
				continue;
			}
			WorldTile tile2 = item.getTile();
			if (tile2 != null && tile.reachableFrom(tile2) && item.needSettlers())
			{
				TileZone tileZone = item.getTile()?.zone;
				if (tileZone != null)
				{
					return tileZone;
				}
			}
		}
		return null;
	}

	static BehKingCheckNewCityFoundation()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		_color1 = new Color(1f, 0f, 0f, 0.3f);
		_color2 = new Color(0f, 0f, 1f, 0.3f);
		_color3 = new Color(1f, 0.92f, 0.016f, 0.3f);
		_color4 = new Color(0f, 1f, 0f, 0.3f);
	}
}
