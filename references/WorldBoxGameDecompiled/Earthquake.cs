using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Earthquake : BaseMapObject
{
	private PrintTemplate _current_print;

	private int _print_tick;

	private WorldTile _print_tile_origin;

	private float _timer;

	private const float INTERVAL = 0.05f;

	private bool _quake_active;

	private EarthquakeType _type;

	private int _current_print_index;

	private static Earthquake _instance;

	public void Awake()
	{
		_instance = this;
	}

	public static void startQuake(WorldTile pTile, EarthquakeType pType = EarthquakeType.RandomPower)
	{
		_instance.spawnQuake(pTile, pType);
	}

	private void spawnQuake(WorldTile pTile, EarthquakeType pType)
	{
		if (isQuakeActive())
		{
			return;
		}
		MusicBox.playSound("event:/SFX/NATURE/EarthQuake", pTile);
		_type = pType;
		_quake_active = true;
		List<PrintTemplate> quakes = PrintLibrary.getQuakes();
		_current_print_index++;
		if (_current_print_index >= quakes.Count)
		{
			quakes.Shuffle();
			_current_print_index = 0;
		}
		_current_print = quakes[_current_print_index];
		_current_print.steps.Shuffle();
		_print_tile_origin = pTile;
		_print_tick = 0;
		if (pType == EarthquakeType.RandomPower)
		{
			if (Randy.randomChance(0.5f))
			{
				_type = EarthquakeType.BigDecrease;
			}
			else
			{
				_type = EarthquakeType.BigIncrease;
			}
		}
	}

	private void Update()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (!isQuakeActive())
		{
			return;
		}
		if (_timer > 0f)
		{
			_timer -= World.world.elapsed;
			return;
		}
		_timer = 0.05f;
		for (int i = 0; i < 300; i++)
		{
			if (_print_tick >= _current_print.steps.Length)
			{
				endQuake();
				break;
			}
			PrintStep printStep = _current_print.steps[_print_tick];
			_print_tick++;
			MapBox world = World.world;
			Vector2Int pos = _print_tile_origin.pos;
			int pX = ((Vector2Int)(ref pos)).x + printStep.x;
			pos = _print_tile_origin.pos;
			WorldTile tile = world.GetTile(pX, ((Vector2Int)(ref pos)).y + printStep.y);
			if (tile != null)
			{
				tileAction(tile);
				if (_print_tick >= _current_print.steps.Length)
				{
					endQuake();
					break;
				}
			}
		}
		World.world.startShake(0.3f, 0.01f, 0.23f, pShakeX: true);
	}

	private void tileAction(WorldTile pTile)
	{
		if (MapAction.checkTileDamageGaiaCovenant(pTile, pDamage: true))
		{
			switch (_type)
			{
			case EarthquakeType.BigDecrease:
				MapAction.decreaseTile(pTile, pDamage: true, "earthquake");
				break;
			case EarthquakeType.BigIncrease:
				MapAction.increaseTile(pTile, pDamage: true, "earthquake");
				break;
			case EarthquakeType.SmallDisaster:
				MapAction.terraformMain(pTile, pTile.main_type, AssetManager.terraform.get("earthquake_disaster"));
				break;
			}
		}
		pTile.removeBurn();
		pTile.doUnits((Action<Actor>)unitAction);
	}

	private void unitAction(Actor pActor)
	{
		if (Randy.randomBool())
		{
			pActor.makeConfused();
		}
		else
		{
			pActor.applyRandomForce();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool isQuakeActive()
	{
		return _instance._quake_active;
	}

	private void endQuake()
	{
		_quake_active = false;
	}
}
