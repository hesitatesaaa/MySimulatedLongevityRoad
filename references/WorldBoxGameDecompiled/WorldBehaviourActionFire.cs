using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using ai;
using strings;

public static class WorldBehaviourActionFire
{
	private static readonly HashSetWorldTile _tiles = new HashSetWorldTile();

	private static int[] _fires;

	private static readonly List<WorldTile> _list_updater = new List<WorldTile>();

	public static void prepare()
	{
		if (_fires == null || _fires.Length != World.world.zone_calculator.zones.Count)
		{
			_fires = new int[World.world.zone_calculator.zones.Count];
		}
		else
		{
			clearFires();
		}
	}

	public static void clearFires()
	{
		if (_fires != null)
		{
			for (int i = 0; i < _fires.Length; i++)
			{
				_fires[i] = 0;
			}
		}
	}

	public static void addFire(WorldTile pTile)
	{
		if (_tiles.Add(pTile))
		{
			World.world.fire_layer.setTileDirty(pTile);
			_fires[pTile.zone.id]++;
		}
	}

	public static void removeFire(WorldTile pTile)
	{
		if (_tiles.Remove(pTile))
		{
			World.world.fire_layer.setTileDirty(pTile);
			if (_fires[pTile.zone.id] == 0)
			{
				Debug.Log((object)"FIRE ERROR");
			}
			else
			{
				_fires[pTile.zone.id]--;
			}
		}
	}

	public static void updateFire()
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		if (_tiles.Count == 0)
		{
			return;
		}
		_list_updater.Clear();
		_list_updater.AddRange(_tiles);
		foreach (WorldTile item in _list_updater)
		{
			float worldTimeElapsedSince = World.world.getWorldTimeElapsedSince(item.data.fire_timestamp);
			bool flag = true;
			if (World.world.era_manager.getCurrentAge().particles_rain)
			{
				flag = false;
			}
			if (WorldLawLibrary.world_law_gaias_covenant.isEnabled())
			{
				flag = false;
			}
			if ((worldTimeElapsedSince >= SimGlobals.m.fire_spread_time) & flag)
			{
				for (int i = 0; i < item.neighbours.Length; i++)
				{
					WorldTile worldTile = item.neighbours[i];
					if (Randy.randomChance(worldTile.Type.fire_chance * World.world_era.fire_spread_rate_bonus) && worldTile.startFire())
					{
						World.world.flash_effects.flashPixel(worldTile, 10);
					}
				}
			}
			if (Randy.randomChance(0.1f))
			{
				World.world.particles_fire.spawn(item.posV3);
			}
			bool flag2 = false;
			float pVal = 0f;
			if (worldTimeElapsedSince > SimGlobals.m.fire_stop_time)
			{
				pVal = 0.1f + worldTimeElapsedSince / SimGlobals.m.fire_time * 0.3f;
			}
			if (item.Type.ocean)
			{
				flag2 = true;
			}
			else if (worldTimeElapsedSince >= SimGlobals.m.fire_time)
			{
				flag2 = true;
			}
			else if (Randy.randomChance(pVal))
			{
				flag2 = true;
			}
			if (flag2)
			{
				item.stopFire();
				checkFireElementalSpawn(item);
			}
		}
	}

	private static void checkFireElementalSpawn(WorldTile pTile)
	{
		if (WorldLawLibrary.world_law_disasters_other.isEnabled() && World.world_era.era_disaster_fire_elemental_spawn_on_fire && Randy.randomChance(World.world_era.fire_elemental_spawn_chance) && ActorTool.countUnitsFrom("fire_elemental") <= 100)
		{
			string random = SA.fire_elementals.GetRandom();
			World.world.units.spawnNewUnit(random, pTile, pSpawnSound: false, pMiracleSpawn: false, 6f, null, pGiveOwnerlessItems: false, pAdultAge: true);
		}
	}

	public static void clear()
	{
		_tiles.Clear();
		_list_updater.Clear();
	}

	public static bool hasFires()
	{
		return _tiles.Count > 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int countFires(TileZone pZone)
	{
		return _fires[pZone.id];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool hasFires(TileZone pZone)
	{
		return _fires[pZone.id] > 0;
	}

	public static int[] getFires()
	{
		return _fires;
	}
}
