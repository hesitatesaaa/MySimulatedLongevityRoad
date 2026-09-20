using System;
using System.Collections.Generic;
using UnityEngine;

namespace ai;

public static class ActorTool
{
	public static int countContagiousNearby(Actor pActor)
	{
		int num = 0;
		foreach (Actor item in Finder.getUnitsFromChunk(pActor.current_tile, 1, 10f))
		{
			if (item.hasTrait("contagious"))
			{
				num++;
			}
		}
		return num;
	}

	public static Building findNewBuildingTarget(Actor pActor, string pType, bool pOnlyFreeTile = true)
	{
		using ListPool<Building> listPool = new ListPool<Building>(64);
		switch (pType)
		{
		case "new_building":
		{
			Building buildingToBuild = pActor.city.getBuildingToBuild();
			if (buildingToBuild != null && buildingToBuild.tiles.Count != 0)
			{
				WorldTile constructionTile = buildingToBuild.getConstructionTile();
				if (constructionTile != null && constructionTile.isSameIsland(pActor.current_tile))
				{
					return buildingToBuild;
				}
			}
			break;
		}
		case "random_house_building":
			foreach (Building item in pActor.city.buildings.LoopRandom())
			{
				if (item.isSameIslandAs(pActor) && !item.isUnderConstruction() && item.isUsable() && item.asset.hasHousingSlots())
				{
					return item;
				}
			}
			break;
		case "ruins":
			foreach (TileZone item2 in pActor.city.zones.LoopRandom())
			{
				HashSet<Building> hashset = item2.getHashset(BuildingList.Ruins);
				if (hashset != null)
				{
					listPool.AddRange(hashset);
				}
				HashSet<Building> hashset2 = item2.getHashset(BuildingList.Abandoned);
				if (hashset2 != null)
				{
					listPool.AddRange(hashset2);
				}
				if (listPool.Count > 0)
				{
					break;
				}
			}
			break;
		case "type_mine":
		case "type_bonfire":
		case "type_training_dummies":
		{
			Building buildingOfType = pActor.city.getBuildingOfType(pType, pCountOnlyFinished: true, pRandom: false, pOnlyFreeTile, pActor.current_island);
			if (buildingOfType != null)
			{
				return buildingOfType;
			}
			break;
		}
		default:
			return findNewTargetInZones(pActor, pType, listPool);
		}
		if (listPool.Count == 0)
		{
			return null;
		}
		return listPool.GetRandom();
	}

	public static Building findNewTargetInZones(Actor pActor, string pType, ListPool<Building> pPossibleBuildings)
	{
		foreach (TileZone zone in pActor.city.zones)
		{
			HashSet<Building> hashSet = null;
			switch (pType)
			{
			case "type_flower":
			case "type_vegetation":
				hashSet = zone.getHashset(BuildingList.Flora);
				break;
			case "type_tree":
				hashSet = zone.getHashset(BuildingList.Trees);
				break;
			case "type_mineral":
				hashSet = zone.getHashset(BuildingList.Minerals);
				break;
			case "type_fruits":
				hashSet = zone.getHashset(BuildingList.Food);
				break;
			case "type_hive":
				hashSet = zone.getHashset(BuildingList.Hives);
				break;
			case "type_poop":
				hashSet = zone.getHashset(BuildingList.Poops);
				break;
			}
			if (hashSet == null || hashSet.Count == 0)
			{
				continue;
			}
			foreach (Building item in hashSet)
			{
				BuildingAsset asset = item.asset;
				if (!item.current_tile.isTargeted() && item.isSameIslandAs(pActor) && ((asset.building_type != BuildingType.Building_Fruits && asset.building_type != BuildingType.Building_Tree) || item.hasResourcesToCollect()) && (asset.building_type != BuildingType.Building_Tree || asset.can_be_chopped_down))
				{
					pPossibleBuildings.Add(item);
				}
			}
			if (pPossibleBuildings.Count > 0)
			{
				break;
			}
		}
		if (pPossibleBuildings.Count == 0)
		{
			return null;
		}
		return pPossibleBuildings.GetRandom();
	}

	public static WorldTile getTileNearby(ActorTileTarget pTarget, MapChunk pChunk)
	{
		ListPool<WorldTile> tPossibleMoves = new ListPool<WorldTile>(64);
		try
		{
			(MapChunk[], int) allChunksFromChunk = Toolbox.getAllChunksFromChunk(pChunk);
			MapChunk[] item = allChunksFromChunk.Item1;
			int item2 = allChunksFromChunk.Item2;
			for (int i = 0; i < item2; i++)
			{
				MapChunk mapChunk = item[i];
				if (tPossibleMoves.Count > 20)
				{
					continue;
				}
				WorldTile[] tiles = mapChunk.tiles;
				int num = tiles.Length;
				for (int j = 0; j < num; j++)
				{
					WorldTile tTile = tiles[j];
					if (tPossibleMoves.Count > 20)
					{
						continue;
					}
					switch (pTarget)
					{
					case ActorTileTarget.RandomTNT:
						if (tTile.Type.explodable)
						{
							tPossibleMoves.Add(tTile);
						}
						break;
					case ActorTileTarget.RandomTileWithUnits:
						tTile.doUnits(delegate
						{
							tPossibleMoves.Add(tTile);
						});
						break;
					case ActorTileTarget.RandomBurnableTile:
						if (tTile.Type.burnable)
						{
							tPossibleMoves.Add(tTile);
						}
						break;
					case ActorTileTarget.RandomTileWithCivStructures:
						if (tTile.hasBuilding() && tTile.building.hasCity())
						{
							tPossibleMoves.Add(tTile);
						}
						if (tTile.Type.burnable && tTile.zone.city != null)
						{
							tPossibleMoves.Add(tTile);
						}
						break;
					}
				}
			}
			if (tPossibleMoves.Count == 0)
			{
				return null;
			}
			return tPossibleMoves.GetRandom();
		}
		finally
		{
			if (tPossibleMoves != null)
			{
				((IDisposable)tPossibleMoves).Dispose();
			}
		}
	}

	public static Docks getDockTradeTarget(Actor pActor)
	{
		return getDockTradeTarget(pActor.current_tile, pActor);
	}

	private static Docks getDockTradeTarget(WorldTile pTile, Actor pActor)
	{
		return getDockTradeTarget(pTile.region, pActor);
	}

	private static Docks getDockTradeTarget(MapRegion pRegion, Actor pActor)
	{
		return getDockTradeTarget(pRegion.island, pActor);
	}

	private static Docks getDockTradeTarget(TileIsland pIsland, Actor pActor)
	{
		return getDockTradeTarget(pIsland.docks, pActor);
	}

	private static Docks getDockTradeTarget(ListPool<Docks> pList, Actor pActor)
	{
		if (pList == null || pList.Count == 0)
		{
			return null;
		}
		foreach (Docks item in pList.LoopRandom())
		{
			if (item.building.hasCity() && pActor.getHomeBuilding() != item.building && item.building.isUsable() && !item.building.isAbandoned() && !item.building.city.kingdom.isEnemy(pActor.kingdom) && (item.isDockGood() || item.hasOceanTiles()))
			{
				return item;
			}
		}
		return null;
	}

	public static WorldTile getRandomTileForBoat(Actor pActor)
	{
		MapRegion mapRegion = pActor.current_tile.region;
		if (mapRegion.neighbours.Count > 0 && Randy.randomBool())
		{
			mapRegion = mapRegion.neighbours.GetRandom();
		}
		if (mapRegion.tiles.Count > 0)
		{
			return mapRegion.tiles.GetRandom();
		}
		return null;
	}

	public static int attributeDice(Actor pActor, int pAmount = 2)
	{
		if (pActor == null)
		{
			return int.MinValue;
		}
		int num = 0;
		int num2 = (int)(pActor.stats["diplomacy"] + pActor.stats["warfare"] + pActor.stats["stewardship"]);
		if (pActor.hasCulture())
		{
			Culture culture = pActor.culture;
			bool flag = culture.hasTrait("patriarchy");
			bool num3 = culture.hasTrait("matriarchy");
			if (flag && pActor.isSexMale())
			{
				num2 += 999;
			}
			if (num3 && pActor.isSexFemale())
			{
				num2 += 999;
			}
		}
		for (int i = 0; i < pAmount; i++)
		{
			num += Randy.randomInt(0, num2);
		}
		return num;
	}

	public static void checkHomeDocks(Actor pActor)
	{
		Building homeBuilding = pActor.getHomeBuilding();
		if (homeBuilding == null)
		{
			ListPool<Docks> docks = pActor.current_tile.region.island.docks;
			if (docks != null && docks.Count > 0)
			{
				for (int i = 0; i < docks.Count; i++)
				{
					Docks docks2 = docks[i];
					if (docks2.building.isUsable() && !docks2.building.isAbandoned() && !docks2.building.isUnderConstruction() && docks2.building.hasCity() && docks2.building.city.kingdom == pActor.kingdom && !docks2.building.city.kingdom.isEnemy(pActor.kingdom) && !docks2.isFull(pActor.asset.boat_type))
					{
						docks2.addBoatToDock(pActor);
						return;
					}
				}
			}
		}
		if (homeBuilding == null)
		{
			return;
		}
		if (!homeBuilding.isSameIslandAs(pActor))
		{
			pActor.clearHomeBuilding();
			return;
		}
		Docks component_docks = homeBuilding.component_docks;
		if (!component_docks.isDockGood() && !component_docks.hasOceanTiles())
		{
			pActor.clearHomeBuilding();
		}
	}

	public static void copyImportantData(ActorData pFrom, ActorData pCloneTo, bool pCopyAge)
	{
		pCloneTo.name = pFrom.name;
		pCloneTo.custom_name = pFrom.custom_name;
		if (pCopyAge)
		{
			pCloneTo.created_time = pFrom.created_time;
			pCloneTo.age_overgrowth = pFrom.age_overgrowth;
		}
		pCloneTo.asset_id = pFrom.asset_id;
		pCloneTo.kills = pFrom.kills;
		pCloneTo.births = pFrom.births;
		pCloneTo.favorite = pFrom.favorite;
		pCloneTo.food_consumed = pFrom.food_consumed;
		pCloneTo.favorite_food = pFrom.favorite_food;
		pCloneTo.head = pFrom.head;
		pCloneTo.generation = pFrom.generation;
		pCloneTo.parent_id_1 = pFrom.parent_id_1;
		pCloneTo.parent_id_2 = pFrom.parent_id_2;
		pCloneTo.ancestor_family = pFrom.ancestor_family;
		pCloneTo.best_friend_id = pFrom.best_friend_id;
		pCloneTo.lover = pFrom.lover;
		pCloneTo.experience = pFrom.experience;
		pCloneTo.renown = pFrom.renown;
		pCloneTo.loot = pFrom.loot;
		pCloneTo.money = pFrom.money;
		pCloneTo.level = pFrom.level;
		pCloneTo.sex = pFrom.sex;
		pCloneTo.phenotype_shade = pFrom.phenotype_shade;
		pCloneTo.phenotype_index = pFrom.phenotype_index;
		pCloneTo["diplomacy"] = pFrom["diplomacy"];
		pCloneTo["intelligence"] = pFrom["intelligence"];
		pCloneTo["stewardship"] = pFrom["stewardship"];
		pCloneTo["warfare"] = pFrom["warfare"];
	}

	public static void copyUnitToOtherUnit(Actor pParent, Actor pCloneTarget, bool pCopyAge = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		pCloneTarget.current_position = pParent.current_position;
		pCloneTarget.current_rotation = pParent.current_rotation;
		copyImportantData(pParent.data, pCloneTarget.data, pCopyAge);
		pCloneTarget.takeItems(pParent);
		foreach (ActorTrait trait in pParent.getTraits())
		{
			pCloneTarget.addTrait(trait);
		}
		pCloneTarget.setStatsDirty();
		if (MoveCamera.inSpectatorMode() && pParent.isCameraFollowingUnit())
		{
			MoveCamera.setFocusUnit(pCloneTarget);
		}
	}

	public static bool canBeCuredFromTraitsOrStatus(Actor pActor)
	{
		foreach (ActorTrait trait in pActor.getTraits())
		{
			if (trait.can_be_cured)
			{
				return true;
			}
		}
		if (pActor.hasAnyStatusEffect())
		{
			foreach (Status status in pActor.getStatuses())
			{
				if (status.asset.can_be_cured)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void applyForceToUnit(AttackData pData, BaseSimObject pTargetToCheck, float pMod = 1f, bool pCheckCancelJobOnLand = false)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		float num = pData.knockback * pMod;
		if (num > 0f && pTargetToCheck.isActor())
		{
			Vector2 val = Vector2.op_Implicit(pTargetToCheck.cur_transform_position);
			Vector2 val2 = Vector2.op_Implicit(pData.hit_position);
			pTargetToCheck.a.calculateForce(val.x, val.y, val2.x, val2.y, num, 0f, pCheckCancelJobOnLand);
		}
	}

	public static int countUnitsFrom(string pActorID)
	{
		return AssetManager.actor_library.get(pActorID).units.Count;
	}

	public static void checkFallInLove(Actor pActor, Actor pTarget)
	{
		if (pActor.canFallInLoveWith(pTarget) && pTarget.canFallInLoveWith(pActor))
		{
			pActor.becomeLoversWith(pTarget);
		}
	}

	public static void checkBecomingBestFriends(Actor pActor, Actor pTarget)
	{
		if (!pActor.hasBestFriend() && !pTarget.hasBestFriend() && (!pActor.isBaby() || !pTarget.isAdult()) && (!pActor.isAdult() || !pTarget.isBaby()))
		{
			float num = 0f;
			if (pActor.hasEmotions() && pTarget.hasEmotions())
			{
				float num2 = 1f - Mathf.Abs(pActor.getHappinessRatio() - pTarget.getHappinessRatio());
				num += num2 * 0.25f;
			}
			if (pActor.family == pTarget.family)
			{
				num += 0.1f;
			}
			num += calcLikeability(pActor, pTarget);
			if (!(num <= 0f) && Randy.randomChance(num))
			{
				pActor.setBestFriend(pTarget, pNew: true);
				pTarget.setBestFriend(pActor, pNew: true);
			}
		}
	}

	private static float calcLikeability(Actor pActor, Actor pTarget)
	{
		float num = 0f;
		foreach (ActorTrait trait in pActor.getTraits())
		{
			float num2 = 1f;
			if (trait.same_trait_mod != 0 && pTarget.hasTrait(trait))
			{
				num2 += (float)trait.same_trait_mod / 100f;
			}
			if (trait.opposite_trait_mod != 0)
			{
				foreach (ActorTrait opposite_trait in trait.opposite_traits)
				{
					if (pTarget.hasTrait(opposite_trait))
					{
						num2 += (float)trait.opposite_trait_mod / 100f;
					}
				}
			}
			num = ((trait.likeability != 0f) ? (num + trait.likeability * num2) : (num + num2 * 0.1f));
		}
		foreach (ActorTrait trait2 in pTarget.getTraits())
		{
			if (trait2.same_trait_mod == 0 && trait2.opposite_trait_mod == 0)
			{
				num += trait2.likeability;
			}
		}
		num *= 0.5f;
		if (pActor.areFoes(pTarget))
		{
			num -= 0.5f;
		}
		num = ((pActor.religion != pTarget.religion) ? (num - 0.25f) : (num + 0.1f));
		if (pActor.clan == pTarget.clan)
		{
			num += 0.1f;
		}
		if (pActor.culture == pTarget.culture)
		{
			num += 0.1f;
		}
		return num;
	}
}
