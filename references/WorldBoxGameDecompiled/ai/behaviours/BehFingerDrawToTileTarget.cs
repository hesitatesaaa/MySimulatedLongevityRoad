using System.Collections.Generic;
using UnityPools;

namespace ai.behaviours;

public class BehFingerDrawToTileTarget : BehFingerDrawAction
{
	public BehFingerDrawToTileTarget()
	{
		drawing_action = true;
	}

	public override BehResult execute(Actor pActor)
	{
		pickBrush(finger);
		pickPower(finger);
		using ListPool<WorldTile> pTiles = new ListPool<WorldTile>(finger.target_tiles);
		ExecuteEvent executeEvent;
		if (pActor.current_tile == pActor.beh_tile_target || Toolbox.DistTile(pActor.current_tile, pActor.beh_tile_target) < 6f)
		{
			executeEvent = ActorMove.goToCurved(pActor, pActor.current_tile, pActor.current_tile.neighboursAll.GetRandom(), pActor.current_tile.neighboursAll.GetRandom(), pActor.beh_tile_target.neighboursAll.GetRandom(), pActor.beh_tile_target.neighboursAll.GetRandom(), pActor.beh_tile_target);
		}
		else if (finger.target_tiles.Count > 10)
		{
			WorldTile randomTileWithinDistance = Toolbox.getRandomTileWithinDistance(pActor.current_tile, 25, pTiles);
			WorldTile randomTileWithinDistance2 = Toolbox.getRandomTileWithinDistance(randomTileWithinDistance, 25, pTiles);
			WorldTile randomTileWithinDistance3 = Toolbox.getRandomTileWithinDistance(pActor.beh_tile_target, 25, pTiles);
			WorldTile randomTileWithinDistance4 = Toolbox.getRandomTileWithinDistance(randomTileWithinDistance3, 25, pTiles);
			executeEvent = ActorMove.goToCurved(pActor, pActor.current_tile, randomTileWithinDistance, randomTileWithinDistance2, randomTileWithinDistance4, randomTileWithinDistance3, pActor.beh_tile_target);
		}
		else
		{
			executeEvent = ActorMove.goToCurved(pActor, pActor.current_tile, pActor.beh_tile_target);
		}
		pActor.timer_action = 0.5f;
		if (executeEvent == ExecuteEvent.False)
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}

	private static void pickBrush(GodFinger pFinger)
	{
		if (pFinger.target_tiles.Count > 0)
		{
			int pMinSize = pFinger.target_tiles.Count / 10;
			int pMaxSize = pFinger.target_tiles.Count / 3;
			pFinger.brush = Brush.getRandom(pMinSize, pMaxSize, brushFilter);
		}
	}

	private static bool brushFilter(BrushData pBrush)
	{
		if (!pBrush.id.StartsWith("circ_"))
		{
			return pBrush.id.StartsWith("special_");
		}
		return true;
	}

	private static void pickPower(GodFinger pFinger)
	{
		bool drawing_over_ground = pFinger.drawing_over_ground;
		bool drawing_over_water = pFinger.drawing_over_water;
		HashSet<WorldTile> target_tiles = pFinger.target_tiles;
		if (pFinger.god_power != null && ((drawing_over_water && GodFinger.power_over_water.Contains(pFinger.god_power.id)) || (drawing_over_ground && GodFinger.power_over_ground.Contains(pFinger.god_power.id))))
		{
			return;
		}
		Dictionary<string, int> dictionary = UnsafeCollectionPool<Dictionary<string, int>, KeyValuePair<string, int>>.Get();
		Dictionary<TileTypeBase, int> dictionary2 = UnsafeCollectionPool<Dictionary<TileTypeBase, int>, KeyValuePair<TileTypeBase, int>>.Get();
		HashSet<WorldTile> hashSet = UnsafeCollectionPool<HashSet<WorldTile>, WorldTile>.Get();
		int num = 0;
		foreach (WorldTile item in target_tiles)
		{
			WorldTile[] neighboursAll = item.neighboursAll;
			foreach (WorldTile worldTile in neighboursAll)
			{
				if (target_tiles.Contains(worldTile) || !hashSet.Add(worldTile))
				{
					continue;
				}
				num++;
				dictionary2.TryGetValue(worldTile.Type, out var value);
				value = (dictionary2[worldTile.Type] = value + 1);
				if (drawing_over_ground)
				{
					BiomeAsset biome_asset = worldTile.Type.biome_asset;
					if (biome_asset != null && !biome_asset.special_biome)
					{
						dictionary.TryGetValue(biome_asset.tile_high, out var value2);
						value2 = (dictionary[biome_asset.tile_high] = value2 + 1);
						dictionary.TryGetValue(biome_asset.tile_low, out var value3);
						value3 = (dictionary[biome_asset.tile_low] = value3 + 1);
					}
				}
			}
		}
		if (drawing_over_water)
		{
			using ListPool<string> listPool = new ListPool<string>(num);
			string[] power_over_water = GodFinger.power_over_water;
			foreach (string text in power_over_water)
			{
				GodPower godPower = AssetManager.powers.get(text);
				bool flag = false;
				TileType cached_tile_type_asset = godPower.cached_tile_type_asset;
				if (cached_tile_type_asset != null)
				{
					flag = true;
					if (dictionary2.TryGetValue(cached_tile_type_asset, out var value4))
					{
						listPool.AddTimes(value4, text);
					}
				}
				TopTileType cached_top_tile_type_asset = godPower.cached_top_tile_type_asset;
				if (cached_top_tile_type_asset != null)
				{
					flag = true;
					if (dictionary2.TryGetValue(cached_top_tile_type_asset, out var value5))
					{
						listPool.AddTimes(value5, text);
					}
				}
				if (!flag)
				{
					listPool.Add(text);
				}
			}
			string pID = Randy.getRandom(listPool) ?? Randy.getRandom(GodFinger.power_over_water);
			pFinger.god_power = AssetManager.powers.get(pID);
		}
		else if (drawing_over_ground)
		{
			using ListPool<string> listPool2 = new ListPool<string>(num);
			string[] power_over_water = GodFinger.power_over_ground;
			foreach (string text2 in power_over_water)
			{
				GodPower godPower2 = AssetManager.powers.get(text2);
				bool flag2 = false;
				DropAsset cached_drop_asset = godPower2.cached_drop_asset;
				if (cached_drop_asset != null)
				{
					if (!string.IsNullOrEmpty(cached_drop_asset.drop_type_high))
					{
						flag2 = true;
						if (dictionary.TryGetValue(cached_drop_asset.drop_type_high, out var value6))
						{
							listPool2.AddTimes(value6, text2);
						}
					}
					if (!string.IsNullOrEmpty(cached_drop_asset.drop_type_low))
					{
						flag2 = true;
						if (dictionary.TryGetValue(cached_drop_asset.drop_type_low, out var value7))
						{
							listPool2.AddTimes(value7, text2);
						}
					}
				}
				TileType cached_tile_type_asset2 = godPower2.cached_tile_type_asset;
				if (cached_tile_type_asset2 != null)
				{
					flag2 = true;
					if (dictionary2.TryGetValue(cached_tile_type_asset2, out var value8))
					{
						listPool2.AddTimes(value8, text2);
					}
				}
				TopTileType cached_top_tile_type_asset2 = godPower2.cached_top_tile_type_asset;
				if (cached_top_tile_type_asset2 != null)
				{
					flag2 = true;
					if (dictionary2.TryGetValue(cached_top_tile_type_asset2, out var value9))
					{
						listPool2.AddTimes(value9, text2);
					}
				}
				if (!flag2)
				{
					listPool2.Add(text2);
				}
			}
			string pID2 = Randy.getRandom(listPool2) ?? Randy.getRandom(GodFinger.power_over_ground);
			pFinger.god_power = AssetManager.powers.get(pID2);
		}
		else
		{
			pFinger.god_power = null;
		}
		UnsafeCollectionPool<Dictionary<string, int>, KeyValuePair<string, int>>.Release(dictionary);
		UnsafeCollectionPool<Dictionary<TileTypeBase, int>, KeyValuePair<TileTypeBase, int>>.Release(dictionary2);
		UnsafeCollectionPool<HashSet<WorldTile>, WorldTile>.Release(hashSet);
	}
}
