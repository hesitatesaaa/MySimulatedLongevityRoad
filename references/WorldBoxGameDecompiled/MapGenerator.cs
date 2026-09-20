using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
	public static MapGenTemplate template;

	private static int _width = 0;

	private static int _height = 0;

	private static WorldTile[,] _tilesMap;

	private static List<GeneratedRoom> _rooms = new List<GeneratedRoom>();

	private static bool reported_tryMakeBiomeSteps = false;

	private static MapGenValues gen_values => template.values;

	public static void clear()
	{
		_tilesMap = null;
		_rooms.Clear();
	}

	public static void prepare()
	{
		template = AssetManager.map_gen_templates.get(Config.current_map_template);
		_width = MapBox.width;
		_height = MapBox.height;
		_tilesMap = World.world.tiles_map;
		schedulePerlinNoiseMap();
		scheduleUpdateTileTypes();
		if (gen_values.forbidden_knowledge_start)
		{
			World.world.world_laws.enable("world_law_cursed_world");
			CursedSacrifice.loadAlreadyCursedState();
		}
		if (gen_values.remove_mountains)
		{
			SmoothLoader.add(delegate
			{
				removeMountains();
			}, "Normalize Ground");
		}
		if (template.special_anthill)
		{
			SmoothLoader.add(delegate
			{
				specialAnthill();
			}, "Anthill");
		}
		if (template.special_checkerboard)
		{
			SmoothLoader.add(delegate
			{
				specialCheckerBoard();
			}, "Checkerboard");
		}
		if (template.special_cubicles)
		{
			SmoothLoader.add(delegate
			{
				specialCubicles();
			}, "Cubicles");
		}
		scheduleRandomShapes(Randy.randomBool());
		if (template.perlin_replace.Count > 0)
		{
			foreach (PerlinReplaceContainer tOption in template.perlin_replace)
			{
				SmoothLoader.add(delegate
				{
					GeneratorTool.ApplyPerlinReplace(tOption);
				}, "Perlin Replace");
			}
		}
		SmoothLoader.add(delegate
		{
			World.world.map_chunk_manager.allDirty();
		}, "Map Chunk Manager (1/2)", pSkipFrame: false, 0.1f);
		SmoothLoader.add(delegate
		{
			World.world.map_chunk_manager.update(0f, pForce: true);
		}, "Map Chunk Manager (2/2)", pSkipFrame: false, 0.1f);
		if (gen_values.random_biomes)
		{
			SmoothLoader.add(delegate
			{
				generateBiomes();
			}, "Add Random Biome");
		}
		if (gen_values.add_mountain_edges)
		{
			SmoothLoader.add(delegate
			{
				addMountainEdges();
			}, "Add Mountain Edges");
		}
		if (template.freeze_mountains)
		{
			SmoothLoader.add(delegate
			{
				freezeMountainTops();
			}, "Freeze Mountain Tops");
		}
		if (gen_values.add_vegetation)
		{
			int num = 12;
			int vegSpread = World.world.tiles_list.Length / 20;
			for (int num2 = 0; num2 < num; num2++)
			{
				SmoothLoader.add(delegate
				{
					AssetManager.tiles.setListTo(DepthGeneratorType.Gameplay);
					spawnVegetation(vegSpread);
				}, "Add Vegetation (" + (num2 + 1) + "/" + num + ")");
			}
		}
		if (gen_values.add_resources)
		{
			SmoothLoader.add(delegate
			{
				spawnResources();
			}, "Add Resources");
		}
	}

	private static void scheduleUpdateTileTypes()
	{
		int num = 4;
		int num2 = World.world.tiles_list.Length / num;
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			int tileAmount = Mathf.Min(World.world.tiles_list.Length - num3, num2);
			int startIndex = num3;
			num3 += tileAmount;
			SmoothLoader.add(delegate
			{
				GeneratorTool.UpdateTileTypes(pGeneratorStage: true, startIndex, tileAmount);
			}, "Generate Tiles (" + num3 + "/" + World.world.tiles_list.Length + ")", pSkipFrame: true);
		}
	}

	private static void scheduleRandomShapes(bool pSubstract)
	{
		if (gen_values.random_shapes_amount == 0)
		{
			return;
		}
		SmoothLoader.add(delegate
		{
			GeneratorTool.Init();
		}, "Perlin Random Shapes (Init)");
		for (int num = 0; num < gen_values.random_shapes_amount; num++)
		{
			SmoothLoader.add(delegate
			{
				GeneratorTool.ApplyRandomShape("height", 2f, 0.7f, pSubstract);
			}, "Perlin Random Shapes (" + (num + 1) + "/" + gen_values.random_shapes_amount + ")");
		}
	}

	private static void specialCubicles()
	{
		HashSet<MapChunk> pChunksUsed = new HashSet<MapChunk>();
		List<MapChunk> list = new List<MapChunk>();
		MapChunk[] chunks = World.world.map_chunk_manager.chunks;
		foreach (MapChunk item in chunks)
		{
			list.Add(item);
		}
		_rooms.Clear();
		while (list.Count > 0)
		{
			MapChunk random = list.GetRandom();
			startCubicle(pChunksUsed, list, random);
		}
		createDoors();
	}

	private static void startCubicle(HashSet<MapChunk> pChunksUsed, List<MapChunk> pListLeft, MapChunk pStartRoomChunk)
	{
		MapChunk mapChunk = pStartRoomChunk;
		List<MapChunk> list = new List<MapChunk>();
		rememberChunk(mapChunk, pChunksUsed, pListLeft, list);
		int pMinInclusive = 2;
		int pMaxExclusive = gen_values.cubicle_size + 2;
		int num = Randy.randomInt(pMinInclusive, pMaxExclusive);
		for (int i = 0; i < num; i++)
		{
			if (mapChunk == null)
			{
				break;
			}
			mapChunk = mapChunk.chunk_right;
			if (mapChunk != null && !pChunksUsed.Contains(mapChunk))
			{
				rememberChunk(mapChunk, pChunksUsed, pListLeft, list);
			}
		}
		mapChunk = pStartRoomChunk;
		num = Randy.randomInt(pMinInclusive, pMaxExclusive);
		for (int j = 0; j < num; j++)
		{
			if (mapChunk == null)
			{
				break;
			}
			mapChunk = mapChunk.chunk_left;
			if (mapChunk != null && !pChunksUsed.Contains(mapChunk))
			{
				rememberChunk(mapChunk, pChunksUsed, pListLeft, list);
			}
		}
		List<MapChunk> list2 = new List<MapChunk>();
		list2.AddRange(list);
		List<MapChunk> list3 = new List<MapChunk>();
		list3.AddRange(list2);
		int num2 = Randy.randomInt(pMinInclusive, pMaxExclusive);
		for (int k = 0; k < num2; k++)
		{
			List<MapChunk> list4 = new List<MapChunk>();
			bool flag = true;
			foreach (MapChunk item in list3)
			{
				if (item.chunk_down == null)
				{
					flag = false;
					k = num2;
					break;
				}
				if (pChunksUsed.Contains(item.chunk_down))
				{
					flag = false;
					k = num2;
					break;
				}
				list4.Add(item);
			}
			if (!flag)
			{
				continue;
			}
			list3.Clear();
			foreach (MapChunk item2 in list4)
			{
				rememberChunk(item2.chunk_down, pChunksUsed, pListLeft, list);
				list3.Add(item2.chunk_down);
			}
		}
		list3.Clear();
		list3.AddRange(list2);
		num2 = Randy.randomInt(pMinInclusive, pMaxExclusive);
		for (int l = 0; l < num2; l++)
		{
			List<MapChunk> list5 = new List<MapChunk>();
			bool flag2 = true;
			foreach (MapChunk item3 in list3)
			{
				if (item3.chunk_up == null)
				{
					flag2 = false;
					l = num2;
					break;
				}
				if (pChunksUsed.Contains(item3.chunk_up))
				{
					flag2 = false;
					l = num2;
					break;
				}
				list5.Add(item3);
			}
			if (!flag2)
			{
				continue;
			}
			list3.Clear();
			foreach (MapChunk item4 in list5)
			{
				rememberChunk(item4.chunk_up, pChunksUsed, pListLeft, list);
				list3.Add(item4.chunk_up);
			}
		}
		finishRoom(list);
	}

	private static void rememberChunk(MapChunk pChunk, HashSet<MapChunk> pChunksUsed, List<MapChunk> pListLeft, List<MapChunk> pNewRoom)
	{
		pChunksUsed.Add(pChunk);
		pListLeft.Remove(pChunk);
		pNewRoom.Add(pChunk);
	}

	private static void finishRoom(List<MapChunk> pChunks)
	{
		BiomeAsset random = BiomeLibrary.pool_biomes.GetRandom();
		TileType pNewTypeMain = TileLibrary.soil_high;
		if (Randy.randomBool())
		{
			pNewTypeMain = TileLibrary.soil_low;
		}
		WorldTile tileSimple = World.world.GetTileSimple(0, 0);
		WorldTile tileSimple2 = World.world.GetTileSimple(MapBox.width - 1, 0);
		WorldTile tileSimple3 = World.world.GetTileSimple(0, MapBox.height - 1);
		WorldTile tileSimple4 = World.world.GetTileSimple(MapBox.width - 1, MapBox.height - 1);
		WorldTile worldTile = null;
		WorldTile worldTile2 = null;
		WorldTile worldTile3 = null;
		WorldTile worldTile4 = null;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		for (int i = 0; i < pChunks.Count; i++)
		{
			WorldTile[] tiles = pChunks[i].tiles;
			int num5 = tiles.Length;
			for (int j = 0; j < num5; j++)
			{
				WorldTile worldTile5 = tiles[j];
				MapAction.terraformTile(worldTile5, pNewTypeMain, null);
				if (gen_values.random_biomes)
				{
					DropsLibrary.useSeedOn(worldTile5, random.getTileLow(), random.getTileHigh());
				}
				float num6 = Toolbox.DistTile(tileSimple, worldTile5);
				float num7 = Toolbox.DistTile(tileSimple2, worldTile5);
				float num8 = Toolbox.DistTile(tileSimple3, worldTile5);
				float num9 = Toolbox.DistTile(tileSimple4, worldTile5);
				if (worldTile == null || num6 < num)
				{
					worldTile = worldTile5;
					num = num6;
				}
				if (worldTile2 == null || num7 < num2)
				{
					worldTile2 = worldTile5;
					num2 = num7;
				}
				if (worldTile3 == null || num8 < num3)
				{
					worldTile3 = worldTile5;
					num3 = num8;
				}
				if (worldTile4 == null || num9 < num4)
				{
					worldTile4 = worldTile5;
					num4 = num9;
				}
			}
		}
		GeneratedRoom generatedRoom = new GeneratedRoom();
		generatedRoom.id_debug = _rooms.Count;
		_rooms.Add(generatedRoom);
		generatedRoom.edges_up = fillTiles(worldTile, worldTile2, TileLibrary.mountains);
		generatedRoom.edges_down = fillTiles(worldTile3, worldTile4, TileLibrary.mountains);
		generatedRoom.edges_left = fillTiles(worldTile, worldTile3, TileLibrary.mountains);
		generatedRoom.edges_right = fillTiles(worldTile4, worldTile2, TileLibrary.mountains);
	}

	private static void createDoors()
	{
		foreach (GeneratedRoom room in _rooms)
		{
			makeDoor(room.edges_down);
			makeDoor(room.edges_left);
			makeDoor(room.edges_right);
			makeDoor(room.edges_up);
		}
	}

	private static void makeDoor(List<WorldTile> pTiles)
	{
		foreach (WorldTile pTile in pTiles)
		{
			if (pTile.main_type != TileLibrary.mountains)
			{
				return;
			}
		}
		WorldTile worldTile;
		if (pTiles.Count > 3)
		{
			int index = Randy.randomInt(3, pTiles.Count - 3);
			worldTile = pTiles[index];
		}
		else
		{
			int index2 = pTiles.Count / 2;
			worldTile = pTiles[index2];
		}
		MapAction.terraformTile(worldTile, TileLibrary.hills, null);
		WorldTile[] neighboursAll = worldTile.neighboursAll;
		foreach (WorldTile worldTile2 in neighboursAll)
		{
			if (worldTile2.main_type == TileLibrary.mountains)
			{
				MapAction.terraformTile(worldTile2, TileLibrary.hills, null);
			}
		}
	}

	private static void specialCheckerBoard()
	{
		BiomeAsset random = BiomeLibrary.pool_biomes.GetRandom();
		BiomeAsset random2 = BiomeLibrary.pool_biomes.GetRandom();
		MapChunk[] chunks = World.world.map_chunk_manager.chunks;
		foreach (MapChunk mapChunk in chunks)
		{
			WorldTile[] tiles = mapChunk.tiles;
			if ((mapChunk.x + mapChunk.y) % 2 == 0)
			{
				int num = tiles.Length;
				for (int j = 0; j < num; j++)
				{
					WorldTile pTile = tiles[j];
					MapAction.terraformTile(pTile, TileLibrary.soil_high, null);
					if (gen_values.random_biomes)
					{
						DropsLibrary.useSeedOn(pTile, random.getTileLow(), random.getTileHigh());
					}
				}
				continue;
			}
			int num2 = tiles.Length;
			for (int k = 0; k < num2; k++)
			{
				WorldTile pTile2 = tiles[k];
				MapAction.terraformTile(pTile2, TileLibrary.soil_low, null);
				if (gen_values.random_biomes)
				{
					DropsLibrary.useSeedOn(pTile2, random2.getTileLow(), random2.getTileHigh());
				}
			}
		}
	}

	private static void specialAnthill()
	{
		WorldTile[] tiles_list = World.world.tiles_list;
		for (int i = 0; i < tiles_list.Length; i++)
		{
			MapAction.terraformTile(tiles_list[i], TileLibrary.mountains, null);
		}
		List<TileZone> list = new List<TileZone>();
		List<WorldTile> list2 = new List<WorldTile>();
		ZoneCalculator zone_calculator = World.world.zone_calculator;
		int num = zone_calculator.zones_total_x / 10 + 1;
		int num2 = zone_calculator.zones_total_y / 10 + 1;
		TileZone tileZone = zone_calculator.map[num, num2];
		TileZone tileZone2 = zone_calculator.map[zone_calculator.zones_total_x - num, num2];
		TileZone tileZone3 = zone_calculator.map[zone_calculator.zones_total_x - num, zone_calculator.zones_total_y - num2];
		TileZone tileZone4 = zone_calculator.map[num, zone_calculator.zones_total_y - num2];
		makeJailRoom(list, zone_calculator.map[zone_calculator.zones_total_x / 2, zone_calculator.zones_total_y / 2]);
		makeJailRoom(list, zone_calculator.map[zone_calculator.zones_total_x / 2, zone_calculator.zones_total_y / 2]);
		makeJailRoom(list, tileZone);
		makeJailRoom(list, tileZone);
		makeJailRoom(list, tileZone2);
		makeJailRoom(list, tileZone2);
		makeJailRoom(list, tileZone3);
		makeJailRoom(list, tileZone3);
		makeJailRoom(list, tileZone4);
		makeJailRoom(list, tileZone4);
		makeWay(tileZone, tileZone2, list2);
		makeWay(tileZone4, tileZone3, list2);
		makeWay(tileZone, tileZone4, list2);
		makeWay(tileZone3, tileZone2, list2);
		makeWay(tileZone3, tileZone, list2);
		foreach (TileZone item in list)
		{
			carveZone(item);
		}
		foreach (WorldTile item2 in list2)
		{
			carveTunnel(item2);
		}
	}

	private static List<WorldTile> fillTiles(WorldTile pTile1, WorldTile pTile2, TileType pType)
	{
		List<WorldTile> list = PathfinderTools.raycast(pTile1, pTile2, 1f);
		List<WorldTile> list2 = new List<WorldTile>(list);
		list.Clear();
		foreach (WorldTile item in list2)
		{
			MapAction.terraformTile(item, pType, null);
		}
		return list2;
	}

	private static void makeWay(TileZone tZone1, TileZone tZone2, List<WorldTile> pTunnels)
	{
		List<WorldTile> list = PathfinderTools.raycast(tZone1.centerTile, tZone2.centerTile);
		foreach (WorldTile item in list)
		{
			WorldTile[] neighboursAll = item.neighboursAll;
			foreach (WorldTile worldTile in neighboursAll)
			{
				MapAction.terraformTile(worldTile, TileLibrary.soil_high, null);
				pTunnels.Add(worldTile);
			}
		}
		list.Clear();
	}

	private static void carveTunnel(WorldTile pTile)
	{
		for (int i = 0; i < 10; i++)
		{
			WorldTile worldTile = pTile.neighbours.GetRandom();
			for (int num = 10; num > 0; num--)
			{
				WorldTile random = worldTile.neighbours.GetRandom();
				if (random.Type.rocks)
				{
					MapAction.terraformTile(worldTile, TileLibrary.soil_high, null);
					worldTile = random;
				}
			}
		}
	}

	private static void carveZone(TileZone pZone)
	{
		for (int i = 0; i < 20; i++)
		{
			WorldTile worldTile = pZone.tiles.GetRandom();
			for (int num = 15; num > 0; num--)
			{
				WorldTile random = worldTile.neighbours.GetRandom();
				if (random.Type.rocks)
				{
					MapAction.terraformTile(worldTile, TileLibrary.soil_high, null);
					worldTile = random;
				}
			}
		}
	}

	private static void makeJailRoom(List<TileZone> pZones, TileZone pStartZone)
	{
		int num = World.world.zone_calculator.zones.Count / 10;
		TileZone tileZone = pStartZone;
		if (tileZone.world_edge)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			if (tileZone.world_edge)
			{
				tileZone = tileZone.neighbours.GetRandom();
				continue;
			}
			WorldTile[] tiles = tileZone.tiles;
			int num2 = tiles.Length;
			for (int j = 0; j < num2; j++)
			{
				MapAction.terraformTile(tiles[j], TileLibrary.soil_high, null);
			}
			pZones.Add(tileZone);
			tileZone = tileZone.neighbours.GetRandom();
		}
	}

	private static void schedulePerlinNoiseMap()
	{
		scheduleRandomShapes(pSubstract: true);
		if (gen_values.main_perlin_noise_stage && gen_values.perlin_scale_stage_1 > 0)
		{
			SmoothLoader.add(delegate
			{
				int num = Randy.randomInt(0, 1000000);
				int num2 = Randy.randomInt(0, 1000000);
				GeneratorTool.ApplyPerlinNoise(_tilesMap, _width, _height, num, num2, 1f, 1f * (float)gen_values.perlin_scale_stage_1);
			}, "Perlin Noise", pSkipFrame: true);
		}
		if (template.force_height_to > 0)
		{
			SmoothLoader.add(delegate
			{
				forceHeight();
			}, "Add Height");
		}
		if (gen_values.add_center_gradient_land)
		{
			SmoothLoader.add(delegate
			{
				addCenterGradient();
			}, "Center Gradient");
		}
		if (gen_values.center_gradient_mountains)
		{
			SmoothLoader.add(delegate
			{
				addCenterMountains();
			}, "Center Mountains");
		}
		if (gen_values.add_center_lake)
		{
			SmoothLoader.add(delegate
			{
				addCenterLake();
			}, "Center Lake");
		}
		if (gen_values.perlin_noise_stage_2 && gen_values.perlin_scale_stage_2 > 0)
		{
			SmoothLoader.add(delegate
			{
				float num = gen_values.perlin_scale_stage_2;
				int num2 = Randy.randomInt(0, 1000000);
				int num3 = Randy.randomInt(0, 1000000);
				GeneratorTool.ApplyPerlinNoise(_tilesMap, _width, _height, num2, num3, 0.2f, 4f * num, pSubtract: true);
			}, "Perlin Noise (1)");
		}
		if (gen_values.perlin_noise_stage_3 && gen_values.perlin_scale_stage_3 > 0)
		{
			SmoothLoader.add(delegate
			{
				float num = gen_values.perlin_scale_stage_3;
				int num2 = Randy.randomInt(0, 1000000);
				int num3 = Randy.randomInt(0, 1000000);
				GeneratorTool.ApplyPerlinNoise(_tilesMap, _width, _height, num2, num3, 0.1f, num * 10f, pSubtract: true);
			}, "Perlin Noise (2)");
		}
		if (gen_values.low_ground)
		{
			SmoothLoader.add(delegate
			{
				lowGround();
			}, "Lower Ground");
		}
		if (gen_values.high_ground)
		{
			SmoothLoader.add(delegate
			{
				highGround();
			}, "High Ground");
		}
		scheduleRandomShapes(pSubstract: true);
		if (gen_values.ring_effect)
		{
			SmoothLoader.add(delegate
			{
				GeneratorTool.ApplyRingEffect();
			}, "Perlin Ring", pSkipFrame: true);
		}
		if (gen_values.gradient_round_edges)
		{
			SmoothLoader.add(delegate
			{
				MapEdges.AddEdgeGradientCircle(World.world.tiles_map, "height");
			}, "Gradient Circle Edges", pSkipFrame: true);
		}
		if (gen_values.square_edges)
		{
			SmoothLoader.add(delegate
			{
				MapEdges.AddEdgeSquare(World.world.tiles_map, "height");
			}, "Gradient Circle Edges", pSkipFrame: true);
		}
	}

	private static void forceHeight()
	{
		WorldTile[] tiles_list = World.world.tiles_list;
		for (int i = 0; i < tiles_list.Length; i++)
		{
			tiles_list[i].Height = template.force_height_to;
		}
	}

	private static void removeMountains()
	{
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			if (worldTile.Type.rocks)
			{
				MapAction.decreaseTile(worldTile, pDamage: false);
			}
			if (worldTile.Type.rocks)
			{
				MapAction.decreaseTile(worldTile, pDamage: false);
			}
		}
	}

	private static void lowGround()
	{
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			if (worldTile.Height > 150)
			{
				worldTile.Height -= 50;
			}
			if (worldTile.Height > 130)
			{
				worldTile.Height -= 20;
			}
		}
	}

	private static void highGround()
	{
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			if (worldTile.Height < 20)
			{
				worldTile.Height += 80;
			}
			else if (worldTile.Height < 120)
			{
				worldTile.Height += 40;
			}
		}
	}

	private static void addCenterGradient()
	{
		WorldTile pT = World.world.tiles_map[MapBox.width / 2, MapBox.height / 2];
		float num = 0.9f;
		float num2 = 0.6f;
		float num3 = (float)(MapBox.width / 2) * num;
		float num4 = (float)(MapBox.width / 2) * num2;
		float num5 = num3 - num4;
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			float num6 = Toolbox.DistTile(worldTile, pT);
			if (!(num6 > num3))
			{
				float num7 = (num3 - num6) / num5;
				int num8 = (int)(45f * num7);
				worldTile.Height += num8;
			}
		}
	}

	private static void addCenterLake()
	{
		WorldTile pT = World.world.tiles_map[MapBox.width / 2, MapBox.height / 2];
		float num = 0.6f;
		float num2 = 0.2f;
		float num3 = (float)(MapBox.width / 2) * num;
		float num4 = (float)(MapBox.width / 2) * num2;
		float num5 = num3 - num4;
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			float num6 = Toolbox.DistTile(worldTile, pT);
			if (!(num6 > num3))
			{
				float num7 = num3 - num6;
				float num8 = 1f - num7 / num5;
				int height = (int)((float)worldTile.Height * num8);
				worldTile.Height = height;
			}
		}
	}

	private static void addCenterMountains()
	{
		WorldTile pT = World.world.tiles_map[MapBox.width / 2, MapBox.height / 2];
		float num = 0.3f;
		float num2 = 0f;
		float num3 = (float)(MapBox.width / 2) * num;
		float num4 = (float)(MapBox.width / 2) * num2;
		float num5 = num3 - num4;
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile obj in tiles_list)
		{
			float num6 = Toolbox.DistTile(obj, pT);
			float num7 = (num3 - num6) / num5;
			int num8 = (int)(75f * num7);
			obj.Height -= num8;
		}
	}

	private static void generateBiomes()
	{
		HashSetWorldTile hashSetWorldTile = new HashSetWorldTile();
		for (int i = 0; i < World.world.tiles_list.Length; i++)
		{
			WorldTile worldTile = World.world.tiles_list[i];
			if (worldTile.Type.soil)
			{
				hashSetWorldTile.Add(worldTile);
			}
		}
		using ListPool<WorldTile> listPool = new ListPool<WorldTile>(hashSetWorldTile.Count);
		bool flag = true;
		while (hashSetWorldTile.Count > 0)
		{
			if (flag)
			{
				flag = false;
				recreateSoilList(hashSetWorldTile, listPool);
			}
			WorldTile pStartTile = listPool.Last();
			BiomeAsset random = BiomeLibrary.pool_biomes.GetRandom();
			int num = tryMakeBiomeSteps(pStartTile, random);
			if (num == 0)
			{
				listPool.Pop();
				continue;
			}
			tryMakeBiome(pStartTile, hashSetWorldTile, num, random);
			flag = true;
		}
	}

	private static void recreateSoilList(HashSetWorldTile pHashSet, ListPool<WorldTile> pTempSoilTiles)
	{
		pTempSoilTiles.Clear();
		pTempSoilTiles.AddRange(pHashSet);
		pTempSoilTiles.Shuffle();
	}

	private static int tryMakeBiomeSteps(WorldTile pStartTile, BiomeAsset pBiome)
	{
		if (!reported_tryMakeBiomeSteps)
		{
			if (pBiome == null)
			{
				Debug.Log((object)"pBiome is null");
				reported_tryMakeBiomeSteps = true;
			}
			if (pStartTile == null)
			{
				Debug.Log((object)"pStartTile is null");
				reported_tryMakeBiomeSteps = true;
			}
			if (pStartTile.region == null)
			{
				Debug.Log((object)"pStartTile.region is null");
				reported_tryMakeBiomeSteps = true;
			}
			if (pStartTile.region.island == null)
			{
				Debug.Log((object)"pStartTile.region.island is null");
				reported_tryMakeBiomeSteps = true;
			}
		}
		int tileCount = pStartTile.region.island.getTileCount();
		int num = ((tileCount < 400) ? tileCount : ((tileCount >= 600) ? (tileCount / 3) : (tileCount / 2)));
		if (num > pBiome.generator_max_size && pBiome.generator_max_size != 0)
		{
			num = pBiome.generator_max_size;
		}
		return num;
	}

	private static void tryMakeBiome(WorldTile pStartTile, HashSetWorldTile pSoilTiles, int pMaxSteps, BiomeAsset pBiome)
	{
		int num = 0;
		using ListPool<WorldTile> listPool = new ListPool<WorldTile>(pMaxSteps);
		HashSetWorldTile hashSetWorldTile = new HashSetWorldTile();
		listPool.Add(pStartTile);
		hashSetWorldTile.Add(pStartTile);
		while (listPool.Count > 0 && num < pMaxSteps)
		{
			listPool.ShuffleLast();
			WorldTile worldTile = listPool.Pop();
			if (worldTile.isTileRank(TileRank.Low))
			{
				worldTile.setTopTileType(pBiome.getTileLow());
			}
			else
			{
				worldTile.setTopTileType(pBiome.getTileHigh());
			}
			pSoilTiles.Remove(worldTile);
			num++;
			for (int i = 0; i < worldTile.neighboursAll.Length; i++)
			{
				WorldTile worldTile2 = worldTile.neighboursAll[i];
				if (!hashSetWorldTile.Contains(worldTile2) && worldTile2.Type.soil)
				{
					listPool.Add(worldTile2);
					hashSetWorldTile.Add(worldTile2);
				}
			}
		}
		if (num <= 10)
		{
			removeSmallBiomePatches(hashSetWorldTile, pStartTile);
		}
	}

	private static void removeSmallBiomePatches(HashSetWorldTile pTiles, WorldTile pStartTile)
	{
		WorldTile worldTile = null;
		WorldTile[] neighboursAll = pStartTile.neighboursAll;
		foreach (WorldTile worldTile2 in neighboursAll)
		{
			if (worldTile2.Type.is_biome && !worldTile2.Type.biome_asset.special_biome && worldTile2.Type.biome_asset != pStartTile.Type.biome_asset)
			{
				worldTile = worldTile2;
				break;
			}
		}
		if (worldTile == null)
		{
			return;
		}
		BiomeAsset biome_asset = worldTile.top_type.biome_asset;
		foreach (WorldTile pTile in pTiles)
		{
			TopTileType tile = biome_asset.getTile(pTile);
			pTile.setTopTileType(tile);
		}
	}

	private static void addMountainEdges()
	{
		int num = 0;
		int num2 = 0;
		WorldTile tileSimple = World.world.GetTileSimple(num, num2);
		WorldTile tileSimple2 = World.world.GetTileSimple(MapBox.width - num - 1, num2);
		WorldTile tileSimple3 = World.world.GetTileSimple(MapBox.width - num - 1, MapBox.height - num2 - 1);
		WorldTile tileSimple4 = World.world.GetTileSimple(num, MapBox.height - num2 - 1);
		fillTiles(tileSimple, tileSimple2, TileLibrary.mountains);
		fillTiles(tileSimple4, tileSimple3, TileLibrary.mountains);
		fillTiles(tileSimple, tileSimple4, TileLibrary.mountains);
		fillTiles(tileSimple3, tileSimple2, TileLibrary.mountains);
	}

	private static void freezeMountainTops()
	{
		for (int i = 0; i < World.world.tiles_list.Length; i++)
		{
			WorldTile worldTile = World.world.tiles_list[i];
			if (worldTile.Type.IsType("mountains") && worldTile.Height > 220)
			{
				worldTile.freeze();
			}
		}
	}

	private static void spawnVegetation(int pAmount)
	{
		for (int i = 0; i < pAmount; i++)
		{
			WorldTile random = World.world.tiles_list.GetRandom();
			if (!random.Type.ground || random.zone.countBuildingsType(BuildingList.Trees) >= 3)
			{
				continue;
			}
			BiomeAsset biome_asset = random.Type.biome_asset;
			if (biome_asset != null && biome_asset.grow_vegetation_auto)
			{
				switch (Randy.randomInt(0, 3))
				{
				case 0:
					BuildingActions.tryGrowVegetationRandom(random, VegetationType.Plants, pOnStart: true);
					break;
				case 1:
					BuildingActions.tryGrowVegetationRandom(random, VegetationType.Trees, pOnStart: true);
					break;
				case 2:
					BuildingActions.tryGrowVegetationRandom(random, VegetationType.Bushes, pOnStart: true);
					break;
				}
			}
		}
	}

	private static void spawnResources()
	{
		BuildingActions.spawnResource(World.world.tiles_list.Length / 1000 / 2 / 2, "fruit_bush", pRandomSize: false);
	}
}
