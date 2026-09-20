using System.Collections.Generic;
using UnityEngine;

public class Dragon : BaseActorComponent
{
	private DragonAsset dragonAsset;

	private DragonState state;

	internal float idle_time = -1f;

	internal float sleep_time = -1f;

	internal SpriteAnimation spriteAnimation;

	internal HashSet<Actor> aggroTargets = new HashSet<Actor>();

	internal WorldTile lastLanded;

	private HashSet<WorldTile> _landAttackTiles = new HashSet<WorldTile>();

	private WorldTile _landAttackPosCheck;

	internal int _landAttackCache;

	internal HashSet<WorldTile> _slideAttackTilesFlip = new HashSet<WorldTile>();

	internal HashSet<WorldTile> _slideAttackTilesNoFlip = new HashSet<WorldTile>();

	private WorldTile _slideAttackPosCheckFlip;

	private WorldTile _slideAttackPosCheckNoFlip;

	internal int _slideAttackTilesFlipCache;

	internal int _slideAttackTilesNoFlipCache;

	internal override void create(Actor pActor)
	{
		base.create(pActor);
		spriteAnimation = ((Component)this).GetComponent<SpriteAnimation>();
		if (actor.asset.id == "zombie_dragon")
		{
			dragonAsset = PrefabLibrary.instance.zombieDragonAsset;
		}
		else
		{
			dragonAsset = PrefabLibrary.instance.dragonAsset;
		}
		actor.setFlying(pVal: true);
		setFrames(DragonState.Fly, pForce: true);
	}

	private void playSound(DragonState pState)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		switch (state)
		{
		case DragonState.LandAttack:
			MusicBox.playSound("event:/SFX/UNITS/dragon/fire_breath", ((Component)this).transform.localPosition.x, ((Component)this).transform.localPosition.y);
			break;
		case DragonState.Slide:
			MusicBox.playSound("event:/SFX/UNITS/dragon/swoop", ((Component)this).transform.localPosition.x, ((Component)this).transform.localPosition.y);
			break;
		}
	}

	internal static bool shouldFly(Actor pActor, WorldTile pTile = null)
	{
		if (pTile == null)
		{
			pTile = pActor.current_tile;
		}
		return !canLand(pActor, pTile);
	}

	internal static bool canLand(Actor pActor, WorldTile pTile = null)
	{
		if (pTile == null)
		{
			pTile = pActor.current_tile;
		}
		if (!pTile.Type.ground)
		{
			if (pTile.Type.lava)
			{
				return !pActor.asset.die_in_lava;
			}
			return false;
		}
		return true;
	}

	internal void attackTile(WorldTile pTile)
	{
		if (pTile == null)
		{
			return;
		}
		bool flag = actor.hasTrait("zombie");
		if (flag)
		{
			DropsLibrary.action_acid(pTile);
			if (pTile.hasUnits() || Randy.randomBool())
			{
				World.world.drop_manager.spawnParabolicDrop(pTile, "acid", 0f, 0.1f, 3.5f, 0.5f, 4f, Randy.randomFloat(0.025f, 0.2f));
			}
		}
		else
		{
			pTile.startFire(pForce: true);
			if (pTile.hasBuilding())
			{
				pTile.building.getHit(10f);
			}
			if (pTile.hasUnits() || Randy.randomBool())
			{
				World.world.drop_manager.spawnParabolicDrop(pTile, "fire", 0f, 0.1f, 3.5f, 0.5f, 4f, Randy.randomFloat(0.025f, 0.2f));
			}
		}
		if (pTile.hasUnits())
		{
			MapAction.damageWorld(pTile, 2, AssetManager.terraform.get(flag ? "zombie_dragon_attack" : "dragon_attack"), actor);
		}
	}

	internal bool hasTargetsForSlide()
	{
		if (WorldLawLibrary.world_law_peaceful_monsters.isEnabled())
		{
			return false;
		}
		attackRange(actor.flip);
		foreach (WorldTile item in actor.flip ? _slideAttackTilesFlip : _slideAttackTilesNoFlip)
		{
			if (hasTarget(item, actor))
			{
				return true;
			}
		}
		return false;
	}

	internal bool targetWithinSlide(WorldTile pTargetTile)
	{
		if (WorldLawLibrary.world_law_peaceful_monsters.isEnabled())
		{
			return false;
		}
		attackRange(flip: true);
		if (_slideAttackTilesFlip.Contains(pTargetTile))
		{
			actor.setFlip(pFlip: true);
			return true;
		}
		attackRange(flip: false);
		if (_slideAttackTilesNoFlip.Contains(pTargetTile))
		{
			actor.setFlip(pFlip: false);
			return true;
		}
		return false;
	}

	internal static Kingdom getIgnoredKingdom(Actor pActor)
	{
		if (pActor.hasTrait("zombie"))
		{
			return World.world.kingdoms_wild.get("undead");
		}
		return World.world.kingdoms_wild.get("dragons");
	}

	internal bool targetsWithinLandAttackRange()
	{
		foreach (Actor aggroTarget in aggroTargets)
		{
			if (!aggroTarget.isRekt() && landAttackRange(aggroTarget.current_tile))
			{
				return true;
			}
		}
		return false;
	}

	internal bool landAttackRange(WorldTile pTargetTile)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int pos = actor.current_tile.pos;
		int x = ((Vector2Int)(ref pos)).x;
		pos = actor.current_tile.pos;
		int y = ((Vector2Int)(ref pos)).y;
		pos = pTargetTile.pos;
		int x2 = ((Vector2Int)(ref pos)).x;
		pos = pTargetTile.pos;
		if (Toolbox.Dist(x, y, x2, ((Vector2Int)(ref pos)).y) > 9f)
		{
			return false;
		}
		landAttackTiles(actor.current_tile);
		return _landAttackTiles.Contains(pTargetTile);
	}

	internal HashSet<WorldTile> landAttackTiles(WorldTile pTile)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (_landAttackPosCheck == pTile)
		{
			_landAttackCache++;
			return _landAttackTiles;
		}
		_landAttackCache = 0;
		_landAttackTiles.Clear();
		_landAttackPosCheck = pTile;
		for (int i = 0; i < 12; i++)
		{
			for (int j = 0; j < 20; j++)
			{
				MapBox world = World.world;
				Vector2Int pos = pTile.pos;
				int pX = ((Vector2Int)(ref pos)).x + j - 10;
				pos = pTile.pos;
				WorldTile tile = world.GetTile(pX, ((Vector2Int)(ref pos)).y - i + 1);
				if (tile != null)
				{
					pos = pTile.pos;
					int x = ((Vector2Int)(ref pos)).x;
					pos = pTile.pos;
					int y = ((Vector2Int)(ref pos)).y;
					pos = tile.pos;
					int x2 = ((Vector2Int)(ref pos)).x;
					pos = tile.pos;
					if (!(Toolbox.Dist(x, y, x2, ((Vector2Int)(ref pos)).y) > 9f))
					{
						_landAttackTiles.Add(tile);
					}
				}
			}
		}
		return _landAttackTiles;
	}

	internal WorldTile randomTileWithinLandAttackRange(WorldTile pTile)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		Toolbox.temp_list_tiles.Clear();
		Vector2Int pos;
		for (int num = 9; num > 1; num--)
		{
			MapBox world = World.world;
			pos = pTile.pos;
			int x = ((Vector2Int)(ref pos)).x;
			pos = pTile.pos;
			WorldTile tile = world.GetTile(x, ((Vector2Int)(ref pos)).y + num);
			if (tile != null)
			{
				pTile = tile;
				break;
			}
		}
		for (int i = 0; i < 12; i++)
		{
			for (int j = 0; j < 20; j++)
			{
				MapBox world2 = World.world;
				pos = pTile.pos;
				int pX = ((Vector2Int)(ref pos)).x + j - 10;
				pos = pTile.pos;
				WorldTile tile2 = world2.GetTile(pX, ((Vector2Int)(ref pos)).y - i + 1);
				if (tile2 != null)
				{
					pos = pTile.pos;
					int x2 = ((Vector2Int)(ref pos)).x;
					pos = pTile.pos;
					int y = ((Vector2Int)(ref pos)).y;
					pos = tile2.pos;
					int x3 = ((Vector2Int)(ref pos)).x;
					pos = tile2.pos;
					if (!(Toolbox.Dist(x2, y, x3, ((Vector2Int)(ref pos)).y) > 9f) && canLand(actor, tile2))
					{
						Toolbox.temp_list_tiles.Add(tile2);
					}
				}
			}
		}
		if (Toolbox.temp_list_tiles.Count == 0)
		{
			return pTile;
		}
		return Toolbox.temp_list_tiles.GetRandom();
	}

	internal HashSet<WorldTile> attackRange(bool flip)
	{
		if (flip)
		{
			if (_slideAttackPosCheckFlip == actor.current_tile)
			{
				_slideAttackTilesFlipCache++;
				return _slideAttackTilesFlip;
			}
			_slideAttackTilesFlipCache = 0;
			_slideAttackTilesFlip.Clear();
			_slideAttackPosCheckFlip = actor.current_tile;
		}
		else
		{
			if (_slideAttackPosCheckNoFlip == actor.current_tile)
			{
				_slideAttackTilesNoFlipCache++;
				return _slideAttackTilesNoFlip;
			}
			_slideAttackTilesNoFlipCache = 0;
			_slideAttackTilesNoFlip.Clear();
			_slideAttackPosCheckNoFlip = actor.current_tile;
		}
		int num = 0;
		num = ((!flip) ? 20 : (-25));
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 35; j++)
			{
				WorldTile tile = World.world.GetTile(actor.current_tile.x + j - 15 + num, actor.current_tile.y - i + 2);
				if (tile != null)
				{
					if (flip)
					{
						_slideAttackTilesFlip.Add(tile);
					}
					if (!flip)
					{
						_slideAttackTilesNoFlip.Add(tile);
					}
				}
			}
		}
		if (flip)
		{
			return _slideAttackTilesFlip;
		}
		return _slideAttackTilesNoFlip;
	}

	private static bool hasTarget(WorldTile tTile, Actor pActor)
	{
		if (tTile.hasBuilding() && tTile.building.isUsable())
		{
			return true;
		}
		if (!tTile.hasUnits())
		{
			return false;
		}
		Kingdom tIgnoredKingdom = getIgnoredKingdom(pActor);
		bool tTargetFound = false;
		tTile.doUnits(delegate(Actor actor)
		{
			if (actor.position_height > 0f)
			{
				return true;
			}
			if (actor.kingdom == tIgnoredKingdom)
			{
				return true;
			}
			tTargetFound = true;
			return false;
		});
		return tTargetFound;
	}

	public void setFrames(DragonState pDragonState, bool pForce = false)
	{
		if (state != pDragonState || pForce)
		{
			actor.setShowShadow(pDragonState == DragonState.Fly);
			state = pDragonState;
			playSound(state);
			DragonAssetContainer asset = dragonAsset.getAsset(pDragonState);
			spriteAnimation.setFrames(asset.frames);
			spriteAnimation.timeBetweenFrames = asset.speed;
			spriteAnimation.resetAnim();
			spriteAnimation.looped = true;
		}
	}

	internal static bool clickToWakeup(BaseSimObject pTarget, WorldTile pTile = null)
	{
		if (pTarget.a.isTask("dragon_sleep"))
		{
			pTarget.a.cancelAllBeh();
			pTarget.a.setTask("dragon_wakeup");
			return true;
		}
		return false;
	}

	internal static bool canFlip(BaseSimObject pTarget = null, WorldTile pTile = null)
	{
		switch (pTarget.a.getActorComponent<Dragon>().state)
		{
		case DragonState.Fly:
		case DragonState.Idle:
			return true;
		case DragonState.LandAttack:
		case DragonState.Death:
		case DragonState.SleepStart:
		case DragonState.SleepLoop:
		case DragonState.SleepUp:
		case DragonState.Landing:
		case DragonState.Slide:
		case DragonState.Up:
			return false;
		default:
			return true;
		}
	}

	internal static bool getHit(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
	{
		Actor a = pSelf.a;
		Dragon actorComponent = a.getActorComponent<Dragon>();
		if (WorldLawLibrary.world_law_peaceful_monsters.isEnabled())
		{
			return true;
		}
		bool flag = false;
		actorComponent.aggroTargets.RemoveWhere((Actor tAttacker) => tAttacker.isRekt());
		if (pAttackedBy != null)
		{
			if (pAttackedBy.isActor() && actorComponent.aggroTargets.Add(pAttackedBy.a))
			{
				flag = actorComponent.aggroTargets.Count == 1;
			}
			if (pAttackedBy.hasCity())
			{
				a.data.set("cityToAttack", pAttackedBy.getCity().data.id);
				a.data.set("attacksForCity", Randy.randomInt(4, 12));
			}
		}
		switch (a.ai.task?.id)
		{
		case "dragon_sleep":
			a.data.set("justGotHit", pData: true);
			a.cancelAllBeh();
			a.setTask("dragon_wakeup");
			break;
		case "dragon_idle":
		{
			a.data.get("landAttacks", out var pResult, 0);
			if (pResult > 2 || shouldFly(a) || pAttackedBy == null)
			{
				a.data.set("justGotHit", pData: true);
				a.cancelAllBeh();
				a.setTask("dragon_up");
			}
			else if (!pAttackedBy.isFlying() && actorComponent.landAttackRange(pAttackedBy.current_tile) && canLand(a))
			{
				a.cancelAllBeh();
				a.setTask("dragon_land_attack");
			}
			break;
		}
		case "dragon_fly":
			if (flag)
			{
				a.cancelAllBeh();
				if (!pAttackedBy.isFlying() && actorComponent.landAttackRange(pAttackedBy.current_tile) && canLand(a) && actorComponent.lastLanded != a.current_tile)
				{
					a.setTask("dragon_land");
				}
				else if (actorComponent.targetWithinSlide(pAttackedBy.current_tile))
				{
					a.setTask("dragon_slide");
				}
				else
				{
					a.setTask("dragon_fly");
				}
			}
			break;
		case "dragon_wakeup":
		case "dragon_up":
			a.data.set("justGotHit", pData: true);
			break;
		}
		return true;
	}

	internal static bool dragonFall(BaseSimObject pTarget, WorldTile pTile, float pElapsed)
	{
		Dragon actorComponent = pTarget.a.getActorComponent<Dragon>();
		SpriteAnimation spriteAnimation = actorComponent.spriteAnimation;
		spriteAnimation.looped = false;
		spriteAnimation.ignorePause = true;
		if (pTarget.isFlying())
		{
			actorComponent.setFrames(DragonState.Landing);
			if (spriteAnimation.currentFrameIndex < spriteAnimation.frames.Length - 1)
			{
				return true;
			}
			pTarget.a.setFlying(pVal: false);
			return true;
		}
		actorComponent.setFrames(DragonState.Death);
		if (spriteAnimation.currentFrameIndex == spriteAnimation.frames.Length - 1)
		{
			pTarget.a.updateDeadBlackAnimation(World.world.elapsed);
		}
		return true;
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		if (!actor.isRekt() && !World.world.isPaused())
		{
			checkLiquid();
		}
	}

	internal void checkLiquid()
	{
		if (actor.isFlying() || actor.is_moving || actor.isEgg() || !actor.current_tile.Type.liquid)
		{
			return;
		}
		if (actor.hasTask())
		{
			if (actor.isTask("dragon_up") || actor.isTask("dragon_wakeup"))
			{
				return;
			}
			if (actor.isTask("dragon_sleep"))
			{
				actor.cancelAllBeh();
				actor.setTask("dragon_wakeup");
				return;
			}
		}
		actor.cancelAllBeh();
		actor.setTask("dragon_up");
	}

	public HashSet<WorldTile> getLandAttackTiles()
	{
		return _landAttackTiles;
	}
}
