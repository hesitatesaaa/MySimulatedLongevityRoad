using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BaseSimObject : NanoObject, IEquatable<BaseSimObject>
{
	public float position_height;

	public WorldTile current_tile;

	public Vector2 current_position;

	public Vector3 current_scale;

	internal Vector3 current_rotation;

	private HashSet<long> _targets_to_ignore;

	[NonSerialized]
	public Kingdom kingdom;

	private bool _stats_dirty;

	internal bool event_full_stats;

	internal readonly BaseStats stats = new BaseStats();

	internal Actor a;

	internal Building b;

	private MapObjectType _object_type;

	private readonly Dictionary<string, Status> _active_status_dict = new Dictionary<string, Status>();

	private bool _has_any_status_cached;

	private bool _has_any_status_to_render;

	internal Vector3 cur_transform_position;

	public TileIsland current_island => current_tile.region.island;

	public TileZone current_zone => current_tile.zone;

	public MapChunk current_chunk => current_tile.chunk;

	public MapRegion current_region => current_tile.region;

	public MapChunk chunk => current_tile.chunk;

	internal virtual void create()
	{
	}

	public int countStatusEffects()
	{
		return _active_status_dict.Count;
	}

	public Dictionary<string, Status>.ValueCollection getStatuses()
	{
		return _active_status_dict.Values;
	}

	public Dictionary<string, Status>.KeyCollection getStatusesIds()
	{
		return _active_status_dict.Keys;
	}

	public IReadOnlyDictionary<string, Status> getStatusesDict()
	{
		return _active_status_dict;
	}

	protected override void setDefaultValues()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		base.setDefaultValues();
		_stats_dirty = true;
		event_full_stats = false;
		current_rotation = default(Vector3);
		position_height = 0f;
		_has_any_status_cached = false;
		_has_any_status_to_render = false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool hasCity()
	{
		return getCity() != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual City getCity()
	{
		return null;
	}

	internal bool addStatusEffect(string pID, float pOverrideTimer = 0f, bool pColorEffect = true)
	{
		StatusAsset statusAsset = AssetManager.status.get(pID);
		if (statusAsset == null)
		{
			return false;
		}
		return addStatusEffect(statusAsset, pOverrideTimer, pColorEffect);
	}

	internal virtual bool addStatusEffect(StatusAsset pStatusAsset, float pOverrideTimer = 0f, bool pColorEffect = true)
	{
		if (!isAlive())
		{
			return false;
		}
		bool flag = isActor();
		if (flag && a.asset.allowed_status_tiers < pStatusAsset.tier)
		{
			return false;
		}
		bool flag2 = hasAnyStatusEffectRaw();
		if (flag2 && hasStatus(pStatusAsset.id))
		{
			if (!pStatusAsset.allow_timer_reset && pOverrideTimer == 0f)
			{
				return false;
			}
			Status status = _active_status_dict[pStatusAsset.id];
			float num = pStatusAsset.duration;
			if (pOverrideTimer != 0f)
			{
				num = pOverrideTimer;
			}
			if (status.getRemainingTime() < (double)num)
			{
				status.setDuration(num);
			}
			return true;
		}
		if (!canAddStatus(pStatusAsset, flag, pColorEffect))
		{
			return false;
		}
		addNewStatusEffect(pStatusAsset, pOverrideTimer, pColorEffect, flag, flag2);
		return true;
	}

	private bool canAddStatus(StatusAsset pStatusAsset, bool pIsActor, bool pHasAnyStatus)
	{
		if (pIsActor)
		{
			if (pStatusAsset.opposite_traits != null)
			{
				for (int i = 0; i < pStatusAsset.opposite_traits.Length; i++)
				{
					string pTraitID = pStatusAsset.opposite_traits[i];
					if (a.hasTrait(pTraitID))
					{
						return false;
					}
				}
			}
			if (pStatusAsset.opposite_tags != null && a.stats.hasTags() && a.stats.hasTags(pStatusAsset.opposite_tags))
			{
				return false;
			}
		}
		if ((pStatusAsset.opposite_status != null) & pHasAnyStatus)
		{
			for (int j = 0; j < pStatusAsset.opposite_status.Length; j++)
			{
				string pID = pStatusAsset.opposite_status[j];
				if (hasStatus(pID))
				{
					return false;
				}
			}
		}
		return true;
	}

	private void addNewStatusEffect(StatusAsset pStatusAsset, float pOverrideTimer, bool pColorEffect, bool pIsActor, bool pHasAnyStatus)
	{
		Status value = World.world.statuses.newStatus(this, pStatusAsset, pOverrideTimer);
		setStatsDirty();
		_active_status_dict.Add(pStatusAsset.id, value);
		_has_any_status_cached = true;
		if ((pIsActor && pStatusAsset.cancel_actor_job) & pColorEffect)
		{
			a.cancelAllBeh();
			a.startColorEffect();
		}
		if ((pStatusAsset.remove_status != null) & pHasAnyStatus)
		{
			for (int i = 0; i < pStatusAsset.remove_status.Length; i++)
			{
				string pID = pStatusAsset.remove_status[i];
				finishStatusEffect(pID);
			}
		}
		if (pIsActor)
		{
			pStatusAsset.action_on_receive?.Invoke(this);
		}
	}

	internal void finishAllStatusEffects()
	{
		foreach (Status value in _active_status_dict.Values)
		{
			value.finish();
			setStatsDirty();
		}
		_active_status_dict.Clear();
		_has_any_status_cached = false;
		_has_any_status_to_render = false;
	}

	public void finishStatusEffect(string pID)
	{
		if (hasAnyStatusEffect() && _active_status_dict.TryGetValue(pID, out var value))
		{
			value.finish();
			setStatsDirty();
		}
	}

	public virtual void setStatsDirty()
	{
		_stats_dirty = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool isActor()
	{
		return _object_type == MapObjectType.Actor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool isBuilding()
	{
		return _object_type == MapObjectType.Building;
	}

	public void setObjectType(MapObjectType pType)
	{
		_object_type = pType;
		if (_object_type == MapObjectType.Actor)
		{
			a = (Actor)this;
		}
		else
		{
			b = (Building)this;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool hasStatus(string pID)
	{
		return _active_status_dict.ContainsKey(pID);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool hasAnyStatusEffect()
	{
		return _has_any_status_cached;
	}

	internal bool hasAnyStatusEffectRaw()
	{
		return _active_status_dict.Count > 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool hasAnyStatusEffectToRender()
	{
		return _has_any_status_to_render;
	}

	public void removeFinishedStatusEffect(Status pStatusData)
	{
		_active_status_dict.Remove(pStatusData.asset.id);
		_has_any_status_cached = hasAnyStatusEffectRaw();
		setStatsDirty();
	}

	internal virtual void updateStats()
	{
		_stats_dirty = false;
		stats_dirty_version++;
		updateCachedStatusEffects();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isStatsDirty()
	{
		return _stats_dirty;
	}

	private void updateCachedStatusEffects()
	{
		_has_any_status_cached = hasAnyStatusEffectRaw();
		_has_any_status_to_render = false;
		if (!_has_any_status_cached)
		{
			return;
		}
		foreach (Status value in _active_status_dict.Values)
		{
			if (!value.is_finished && value.asset.need_visual_render)
			{
				_has_any_status_to_render = true;
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool isInLiquid()
	{
		return current_tile.Type.liquid;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool isInWater()
	{
		return current_tile.Type.ocean;
	}

	public bool isTouchingLiquid()
	{
		if (isInLiquid())
		{
			return !isInAir();
		}
		return false;
	}

	internal virtual bool isInAir()
	{
		return false;
	}

	internal virtual bool isFlying()
	{
		return false;
	}

	internal virtual float getHeight()
	{
		return 0f;
	}

	internal virtual void getHit(float pDamage, bool pFlash = true, AttackType pAttackType = AttackType.Other, BaseSimObject pAttacker = null, bool pSkipIfShake = true, bool pMetallicWeapon = false, bool pCheckDamageReduction = true)
	{
	}

	internal virtual void getHitFullHealth(AttackType pAttackType)
	{
	}

	internal BaseSimObject findEnemyObjectTarget(bool pAttackBuildings)
	{
		EnemyFinderData enemyFinderData = EnemiesFinder.findEnemiesFrom(current_tile, kingdom);
		if (enemyFinderData.isEmpty())
		{
			return null;
		}
		bool flag = true;
		if (enemyFinderData.list.Count > 50)
		{
			flag = Randy.randomChance(0.6f);
		}
		IEnumerable<BaseSimObject> pList;
		if (!flag)
		{
			pList = enemyFinderData.list.LoopRandom();
		}
		else
		{
			IEnumerable<BaseSimObject> list = enemyFinderData.list;
			pList = list;
		}
		return checkObjectList(pList, pAttackBuildings, flag, pIgnoreStunned: false);
	}

	protected BaseSimObject checkObjectList(IEnumerable<BaseSimObject> pList, bool pAttackBuildings, bool pFindClosest, bool pIgnoreStunned, int pMaxDist = int.MaxValue)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		int num = int.MaxValue;
		BaseSimObject result = null;
		long num2 = ((pMaxDist == int.MaxValue) ? pMaxDist : (pMaxDist * pMaxDist + 1));
		bool flag = isActor() && a.hasMeleeAttack();
		WorldTile worldTile = current_tile;
		Vector2Int pos = worldTile.pos;
		foreach (BaseSimObject p in pList)
		{
			if (!p.isAlive() || p == this)
			{
				continue;
			}
			WorldTile worldTile2 = p.current_tile;
			if (pFindClosest)
			{
				num = Toolbox.SquaredDistVec2(worldTile2.pos, pos);
				if (num >= num2)
				{
					continue;
				}
			}
			if ((!pIgnoreStunned || !p.isActor() || !p.a.hasStatusStunned()) && canAttackTarget(p, pCheckForFactions: true, pAttackBuildings) && (!flag || worldTile2.isSameIsland(worldTile) || (!worldTile2.Type.block && worldTile.region.island.isConnectedWith(worldTile2.region.island))) && (!p.isBuilding() || !isKingdomCiv() || !p.b.asset.city_building || p.b.asset.tower || !(p.kingdom.getSpecies() == kingdom.getSpecies())) && !shouldIgnoreTarget(p))
			{
				if (!pFindClosest)
				{
					return p;
				}
				if (num <= 4)
				{
					return p;
				}
				result = p;
				num2 = num;
			}
		}
		return result;
	}

	internal void ignoreTarget(BaseSimObject pTarget)
	{
		if (_targets_to_ignore == null)
		{
			_targets_to_ignore = new HashSet<long>();
		}
		_targets_to_ignore.Add(pTarget.getID());
	}

	internal bool shouldIgnoreTarget(BaseSimObject pTarget)
	{
		return _targets_to_ignore?.Contains(pTarget.getID()) ?? false;
	}

	internal void clearIgnoreTargets()
	{
		_targets_to_ignore?.Clear();
	}

	internal int countTargetsToIgnore()
	{
		return _targets_to_ignore?.Count ?? 0;
	}

	internal bool canAttackTarget(BaseSimObject pTarget, bool pCheckForFactions = true, bool pAttackBuildings = true)
	{
		if (!isAlive())
		{
			return false;
		}
		if (!pTarget.isAlive())
		{
			return false;
		}
		bool flag = isActor();
		if (pTarget.isBuilding() && !pAttackBuildings)
		{
			if (!flag || !a.asset.unit_zombie)
			{
				return false;
			}
			if (!pTarget.kingdom.asset.brain)
			{
				return false;
			}
		}
		string species;
		WeaponType weaponType;
		if (flag)
		{
			if (a.asset.skip_fight_logic)
			{
				return false;
			}
			species = a.asset.id;
			weaponType = a._attack_asset.attack_type;
		}
		else
		{
			species = b.kingdom.getSpecies();
			weaponType = WeaponType.Range;
		}
		if (pTarget.isActor())
		{
			Actor actor = pTarget.a;
			if (!actor.asset.can_be_killed_by_stuff)
			{
				return false;
			}
			if (actor.isInsideSomething())
			{
				return false;
			}
			if (actor.isFlying() && weaponType == WeaponType.Melee)
			{
				return false;
			}
			if (actor.ai.action != null && actor.ai.action.special_prevent_can_be_attacked)
			{
				return false;
			}
			if (actor.isInMagnet())
			{
				return false;
			}
			if (pCheckForFactions && areFoes(pTarget) && actor.isKingdomCiv() && isKingdomCiv() && !hasStatusTantrum() && !actor.hasStatusTantrum())
			{
				bool flag2 = (flag && a.hasXenophobic()) || actor.hasXenophobic();
				bool flag3 = (flag && a.hasXenophiles()) || actor.hasXenophiles();
				bool flag4 = flag && a.culture == actor.culture;
				bool flag5 = species == actor.asset.id;
				bool flag6 = ((flag5 | flag3) && !flag2) || (flag4 & flag5);
				if (!WorldLawLibrary.world_law_angry_civilians.isEnabled())
				{
					if (actor.profession_asset.is_civilian & flag6)
					{
						return false;
					}
					if ((flag && a.profession_asset.is_civilian) & flag6)
					{
						return false;
					}
				}
			}
			if ((pCheckForFactions & flag) && a.hasCannibalism() && a.isSameSpecies(actor))
			{
				Family family = a.family;
				Family family2 = actor.family;
				if (family2 == null || family == null)
				{
					return false;
				}
				if (a.hasFamily())
				{
					if (family2 == family)
					{
						return false;
					}
					if (!family2.areMostUnitsHungry() && !family.areMostUnitsHungry())
					{
						return false;
					}
				}
			}
		}
		else
		{
			Building building = pTarget.b;
			if (isKingdomCiv() && building.asset.city_building && building.asset.tower && !building.isCiv() && flag && a.profession_asset.is_civilian && !WorldLawLibrary.world_law_angry_civilians.isEnabled() && building.kingdom.getSpecies() == kingdom.getSpecies())
			{
				return false;
			}
		}
		if (flag)
		{
			ActorAsset asset = a.asset;
			if (!a.isWaterCreature() || !a.hasRangeAttack())
			{
				if (a.isWaterCreature() && !asset.force_land_creature)
				{
					if (!pTarget.isInLiquid())
					{
						return false;
					}
					if (!pTarget.current_tile.isSameIsland(current_tile))
					{
						return false;
					}
				}
				else if (weaponType == WeaponType.Melee && pTarget.isInLiquid() && !a.isWaterCreature())
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool areFoes(BaseSimObject pTarget)
	{
		return kingdom.isEnemy(pTarget.kingdom);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void setHealth(int pValue, bool pClamp = true)
	{
		BaseObjectData data = getData();
		if (pClamp)
		{
			pValue = Mathf.Clamp(pValue, 1, getMaxHealth());
		}
		data.health = pValue;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void setMaxHealth()
	{
		setHealth(getMaxHealth());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void changeHealth(int pValue)
	{
		BaseObjectData data = getData();
		int num = data.health + pValue;
		data.health = Mathf.Clamp(num, 0, getMaxHealth());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getHealth()
	{
		return getData().health;
	}

	public int getMaxHealthPercent(float pPercent)
	{
		int num = (int)((float)getMaxHealth() * pPercent);
		return Mathf.Max(1, num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool hasHealth()
	{
		return getHealth() > 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(BaseSimObject pObject)
	{
		return _hashcode == pObject.GetHashCode();
	}

	public int getMaxHealth()
	{
		return (int)stats["health"];
	}

	public override void Dispose()
	{
		current_tile = null;
		kingdom = null;
		stats.reset();
		clearIgnoreTargets();
		_targets_to_ignore = null;
		disposeStatusEffects();
		current_tile = null;
		base.Dispose();
	}

	private void disposeStatusEffects()
	{
		foreach (Status value in _active_status_dict.Values)
		{
			value.finish();
		}
		_active_status_dict.Clear();
		_has_any_status_cached = false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isKingdomCiv()
	{
		return kingdom.isCiv();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isKingdomMob()
	{
		return kingdom.isMobs();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool hasKingdom()
	{
		return kingdom != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual BaseObjectData getData()
	{
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public sealed override long getID()
	{
		return getData().id;
	}

	public override double getFoundedTimestamp()
	{
		return getData().created_time;
	}

	public virtual bool hasStatusTantrum()
	{
		return false;
	}

	public bool isSameIsland(WorldTile pTile)
	{
		return current_tile.isSameIsland(pTile);
	}

	public bool isSameIslandAs(BaseSimObject pTarget)
	{
		return current_tile.isSameIsland(pTarget.current_tile);
	}
}
