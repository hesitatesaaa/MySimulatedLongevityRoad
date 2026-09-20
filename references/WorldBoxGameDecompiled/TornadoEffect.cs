using System.Collections.Generic;
using UnityEngine;
using UnityPools;

public class TornadoEffect : BaseEffect
{
	private const float TORNADO_SCALE_DECREASE_SPLIT_MULTIPLIER = 0.5f;

	private const float TORNADO_SCALE_DECREASE_MULTIPLIER = 0.5f;

	private const float TORNADO_SCALE_INCREASE_MULTIPLIER = 1.5f;

	private const float TORNADO_SCALE_MIN = 0.1f;

	private const float TORNADO_SCALE_MAX = 5f;

	public const float TORNADO_SCALE_DEFAULT = 0.5f;

	private const int RANDOM_TILE_RANGE = 5;

	private const float ACTION_INTERVAL_FORCE = 0.1f;

	private const float ACTION_INTERVAL_TERRAFORM = 0.3f;

	private const float SHRINK_TIMER = 10f;

	private const float MOVE_SPEED = 0.15f;

	private float _tornado_timer_force;

	private float _tornado_timer_terraform;

	private float _shrink_timer;

	private float _target_scale = 0.5f;

	private WorldTile _target_tile;

	private static readonly Dictionary<WorldTile, HashSet<TornadoEffect>> _tornadoes_by_tiles = new Dictionary<WorldTile, HashSet<TornadoEffect>>();

	internal float colorEffect;

	internal Material colorMaterial;

	internal override void prepare(WorldTile pTile, float pScale = 0.5f)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.prepare(pTile, pScale);
		current_tile = World.world.GetTileSimple((int)((Component)this).transform.localPosition.x, (int)((Component)this).transform.localPosition.y);
		_target_tile = Toolbox.getRandomTileWithinDistance(current_tile, 5);
		setScale(0.11000001f);
		_target_scale = pScale;
		_tornado_timer_force = 0.1f;
		_tornado_timer_terraform = 0.3f;
		_shrink_timer = 10f;
		addTornadoToTile();
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		updateChangeScale(pElapsed);
		if (World.world.isPaused() || isKilled())
		{
			return;
		}
		updateColorEffect(pElapsed);
		if (state == 2)
		{
			deathAnimation(pElapsed);
			return;
		}
		updateMovement();
		updateShrinking(pElapsed);
		if (_tornado_timer_force > 0f)
		{
			_tornado_timer_force -= pElapsed;
		}
		else
		{
			_tornado_timer_force = 0.1f;
			tornadoActionForce(current_tile);
		}
		if (_tornado_timer_terraform > 0f)
		{
			_tornado_timer_terraform -= pElapsed;
			return;
		}
		_tornado_timer_terraform = 0.3f;
		tornadoActionTerraform(current_tile, scale);
	}

	private void updateMovement()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		WorldTile tileSimple = World.world.GetTileSimple((int)((Component)this).transform.localPosition.x, (int)((Component)this).transform.localPosition.y);
		if (tileSimple != current_tile)
		{
			removeTornadoFromTile();
			current_tile = tileSimple;
			addTornadoToTile();
		}
		if (current_tile == _target_tile)
		{
			_target_tile = Toolbox.getRandomTileWithinDistance(current_tile, 5);
		}
		Vector3 val = _target_tile.posV3 - ((Component)this).transform.localPosition;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Transform transform = ((Component)this).transform;
		transform.localPosition += normalized * 0.15f;
	}

	private void updateShrinking(float pElapsed)
	{
		if (_shrink_timer > 0f)
		{
			_shrink_timer -= pElapsed;
			return;
		}
		_shrink_timer = Randy.randomFloat(7.5f, 12.5f);
		shrink();
	}

	private void tornadoActionTerraform(WorldTile pTile, float pScale = 0.5f)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		BrushData brushData = Brush.get((int)(pScale * 6f));
		bool flag = true;
		if (!MapAction.checkTileDamageGaiaCovenant(pTile, pDamage: true))
		{
			flag = false;
		}
		for (int i = 0; i < brushData.pos.Length; i++)
		{
			Vector2Int pos = pTile.pos;
			int num = ((Vector2Int)(ref pos)).x + brushData.pos[i].x;
			pos = pTile.pos;
			int num2 = ((Vector2Int)(ref pos)).y + brushData.pos[i].y;
			if (num < 0 || num >= MapBox.width || num2 < 0 || num2 >= MapBox.height)
			{
				continue;
			}
			WorldTile tileSimple = World.world.GetTileSimple(num, num2);
			if (tileSimple.Type.ocean)
			{
				MapAction.removeLiquid(tileSimple);
				if (Randy.randomChance(0.15f))
				{
					spawnBurst(tileSimple, "rain", pScale);
				}
			}
			if (flag)
			{
				if (tileSimple.top_type != null || tileSimple.Type.life)
				{
					MapAction.decreaseTile(tileSimple, pDamage: false);
				}
				if (tileSimple.Type.lava)
				{
					LavaHelper.removeLava(tileSimple);
					spawnBurst(tileSimple, "lava", pScale);
				}
			}
			if (tileSimple.hasBuilding() && tileSimple.building.asset.can_be_damaged_by_tornado)
			{
				tileSimple.building.getHit(1f);
			}
			if (tileSimple.isTemporaryFrozen())
			{
				tileSimple.unfreeze(10);
			}
			if (tileSimple.isOnFire())
			{
				tileSimple.stopFire();
			}
		}
	}

	private void tornadoActionForce(WorldTile pTile)
	{
		World.world.applyForceOnTile(pTile, 10, 3f, pForceOut: false);
	}

	private static void spawnBurst(WorldTile pTile, string pType, float pScale)
	{
		if (World.world.drop_manager.getActiveIndex() <= 3000)
		{
			World.world.drop_manager.spawnParabolicDrop(pTile, pType, 0f, 0.62f, 104f * pScale, 0.7f, 23.5f * pScale);
		}
	}

	internal void shrink()
	{
		if (!isKilled())
		{
			float num = scale * 0.5f;
			if (num < 0.1f)
			{
				die();
			}
			else
			{
				resizeTornado(num);
			}
		}
	}

	internal void grow()
	{
		if (!isKilled())
		{
			float num = Mathf.Min(scale * 1.5f, 5f);
			if (num >= 5f)
			{
				AchievementLibrary.tornado.check();
			}
			resizeTornado(num);
		}
	}

	internal bool split()
	{
		if (isKilled())
		{
			return false;
		}
		AchievementLibrary.baby_tornado.check();
		float num = scale * 0.5f;
		if (num < 0.1f)
		{
			die();
			return true;
		}
		EffectsLibrary.spawnAtTile("fx_tornado", current_tile, num);
		resizeTornado(num);
		return true;
	}

	internal void resizeTornado(float pScale)
	{
		_target_scale = pScale;
	}

	public void startColorEffect(string pType = "red")
	{
		colorEffect = 0.3f;
		if (pType == "red")
		{
			colorMaterial = LibraryMaterials.instance.mat_damaged;
		}
		else if (pType == "white")
		{
			colorMaterial = LibraryMaterials.instance.mat_highlighted;
		}
		updateColorEffect(0f);
	}

	private void updateColorEffect(float pElapsed)
	{
		if (colorEffect != 0f)
		{
			colorEffect -= pElapsed;
			if (colorEffect < 0f)
			{
				colorEffect = 0f;
			}
			_ = colorEffect;
			_ = 0f;
		}
	}

	internal static void growTornados(WorldTile pTile)
	{
		resizeOnTile(pTile, "grow");
		for (int i = 0; i < pTile.neighboursAll.Length; i++)
		{
			resizeOnTile(pTile.neighboursAll[i], "grow");
		}
	}

	internal static void shrinkTornados(WorldTile pTile)
	{
		resizeOnTile(pTile, "shrink");
		for (int i = 0; i < pTile.neighboursAll.Length; i++)
		{
			resizeOnTile(pTile.neighboursAll[i], "shrink");
		}
	}

	internal static void resizeOnTile(WorldTile pTile, string direction)
	{
		foreach (BaseEffect item in World.world.stack_effects.get("fx_tornado").getList())
		{
			if (item.active && item.current_tile == pTile)
			{
				TornadoEffect tornadoEffect = item as TornadoEffect;
				if (direction == "grow")
				{
					tornadoEffect.grow();
				}
				else
				{
					tornadoEffect.shrink();
				}
			}
		}
	}

	private void deathAnimation(float pElapsed)
	{
		if (scale > 0f)
		{
			updateChangeScale(pElapsed);
		}
		else
		{
			kill();
		}
	}

	public void die()
	{
		state = 2;
		resizeTornado(0f);
		removeTornadoFromTile();
	}

	internal void updateChangeScale(float pElapsed)
	{
		if (scale == _target_scale)
		{
			return;
		}
		if (scale < _target_scale)
		{
			startColorEffect();
		}
		else
		{
			startColorEffect("white");
		}
		if (scale > _target_scale)
		{
			setScale(scale - 0.2f * pElapsed);
			if (scale < _target_scale)
			{
				setScale(_target_scale);
			}
		}
		else
		{
			setScale(scale + 0.2f * pElapsed);
			if (scale > _target_scale)
			{
				setScale(_target_scale);
			}
		}
		if (scale <= 0.1f)
		{
			die();
		}
	}

	private void addTornadoToTile()
	{
		if (!_tornadoes_by_tiles.TryGetValue(current_tile, out var value))
		{
			value = UnsafeCollectionPool<HashSet<TornadoEffect>, TornadoEffect>.Get();
			_tornadoes_by_tiles.Add(current_tile, value);
		}
		value.Add(this);
	}

	private void removeTornadoFromTile()
	{
		if (_tornadoes_by_tiles.TryGetValue(current_tile, out var value))
		{
			value.Remove(this);
			if (value.Count == 0)
			{
				UnsafeCollectionPool<HashSet<TornadoEffect>, TornadoEffect>.Release(value);
				_tornadoes_by_tiles.Remove(current_tile);
			}
		}
	}

	public static HashSet<TornadoEffect> getTornadoesFromTile(WorldTile pTile)
	{
		if (!_tornadoes_by_tiles.TryGetValue(pTile, out var value))
		{
			return null;
		}
		return value;
	}

	public static void Clear()
	{
		foreach (HashSet<TornadoEffect> value in _tornadoes_by_tiles.Values)
		{
			UnsafeCollectionPool<HashSet<TornadoEffect>, TornadoEffect>.Release(value);
		}
		_tornadoes_by_tiles.Clear();
	}
}
