using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PreloadHelpers
{
	private static bool _initiated = false;

	public static int total_building_sprite_containers = 0;

	public static int total_building_sprites = 0;

	public static List<Sprite> all_preloaded_sprites_buildings = new List<Sprite>();

	public static void init()
	{
		if (!_initiated)
		{
			_initiated = true;
			WindowPreloader.addWindowPreloadResources();
			SmoothLoader.add(delegate
			{
				HandRendererTexturePreloader.launch();
			}, "Preload hand item textures...", pSkipFrame: false, 0.2f);
			SmoothLoader.add(delegate
			{
				AssetManager.actor_library.preloadMainUnitSprites();
				Debug.Log((object)("Loaded unit related sprites: " + ActorTextureSubAsset.all_preloaded_sprites_units.Count));
			}, "Preload main unit textures...", pSkipFrame: false, 0.1f);
			SmoothLoader.add(delegate
			{
				AssetManager.subspecies_traits.preloadMainUnitSprites();
				Debug.Log((object)("Loaded unit related sprites: " + ActorTextureSubAsset.all_preloaded_sprites_units.Count));
			}, "Preload unit trait textures...", pSkipFrame: false, 0.1f);
			SmoothLoader.add(delegate
			{
				preloadBuildingSpriteLists();
			}, "Preload building sprite lists...", pSkipFrame: false, 0.1f);
			SmoothLoader.add(delegate
			{
				preloadBuildingSprites();
			}, "Preload building sprites...", pSkipFrame: false, 0.1f);
			SmoothLoader.add(delegate
			{
				preloadBoatAnimations();
			}, "Preload boat animations...", pSkipFrame: false, 0.1f);
			SmoothLoader.add(delegate
			{
				preloadActorAnimations();
			}, "Preload unit animations...", pSkipFrame: false, 0.1f);
			SmoothLoader.add(delegate
			{
				preloadPixelBagsUnits();
			}, "Preload pixel bags units...", pSkipFrame: false, 0.1f);
			SmoothLoader.add(delegate
			{
				preloadPixelBagsBuildings();
			}, "Preload pixel bags buildings...", pSkipFrame: false, 0.1f);
		}
	}

	private static void preloadBuildingSpriteLists()
	{
		if (!Config.preload_buildings)
		{
			return;
		}
		foreach (BuildingAsset item in AssetManager.buildings.list)
		{
			item.loadBuildingSpriteList();
		}
	}

	private static void preloadBuildingSprites()
	{
		if (!Config.preload_buildings)
		{
			return;
		}
		foreach (BuildingAsset item in AssetManager.buildings.list)
		{
			item.loadBuildingSprites();
		}
		AssetManager.dynamic_sprites_library.checkDirty();
	}

	private static void preloadBoatAnimations()
	{
		if (!Config.preload_units)
		{
			return;
		}
		foreach (ActorAsset list_only_boat_asset in AssetManager.actor_library.list_only_boat_assets)
		{
			ActorAnimationLoader.loadAnimationBoat(list_only_boat_asset.boat_texture_id);
		}
	}

	private static void preloadActorAnimations()
	{
		if (!Config.preload_units)
		{
			return;
		}
		foreach (ActorAsset item in AssetManager.actor_library.list)
		{
			if (item.isTemplateAsset() || item.is_boat || item.ignore_generic_render || item.texture_asset == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, Sprite[]> dict_main in item.texture_asset.dict_mains)
			{
				ActorAnimationLoader.getAnimationContainer(dict_main.Key, item);
			}
		}
	}

	private static void preloadPixelBagsUnits()
	{
		if (!Config.preload_units)
		{
			return;
		}
		foreach (Sprite all_preloaded_sprites_unit in ActorTextureSubAsset.all_preloaded_sprites_units)
		{
			PixelBagManager.preloadPixelBagUnit(all_preloaded_sprites_unit);
		}
	}

	private static void preloadPixelBagsBuildings()
	{
		if (!Config.preload_buildings)
		{
			return;
		}
		foreach (Sprite all_preloaded_sprites_building in all_preloaded_sprites_buildings)
		{
			PixelBagManager.preloadPixelBagUnit(all_preloaded_sprites_building);
		}
	}

	private static void createPreloadReport()
	{
		string path = "GenAssets/wbdiag/preloaded_assets_report.txt";
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		int num = 0;
		num += ActorTextureSubAsset.all_preloaded_sprites_units.Count;
		num += PixelBagManager.total;
		num += SpriteTextureLoader.total_sprites;
		num += SpriteTextureLoader.total_sprites_list;
		num += SpriteTextureLoader.total_sprites_list_single_sprites;
		num += ActorAnimationLoader.count_units;
		num += ActorAnimationLoader.count_heads;
		num += ActorAnimationLoader.count_boats;
		num += ActorTextureSubAsset.getTotal();
		num += total_building_sprite_containers;
		num += total_building_sprites;
		num += HandRendererTexturePreloader.getTotal();
		int num2 = 0;
		foreach (BaseAssetLibrary item in AssetManager.getList())
		{
			num2 += item.total_items;
		}
		stringBuilderPool.AppendLine("# Preloaded Assets Report");
		stringBuilderPool.AppendLine();
		stringBuilderPool.AppendLine("Total Preloaded Graphical Stuff: " + num);
		stringBuilderPool.AppendLine("Total Preloaded Assets: " + num2);
		stringBuilderPool.AppendLine();
		stringBuilderPool.AppendLine("[Sprites] Preloaded Sprites: " + ActorTextureSubAsset.all_preloaded_sprites_units.Count);
		stringBuilderPool.AppendLine("[Sprites] Preloaded Building Sprites: " + total_building_sprites);
		stringBuilderPool.AppendLine("[Sprites] Hand Renderer Sprites: " + HandRendererTexturePreloader.getTotal());
		stringBuilderPool.AppendLine("[Objects] Preloaded Building Sprite Containers: " + total_building_sprite_containers);
		stringBuilderPool.AppendLine("[Objects] Preloaded Pixel Bags: " + PixelBagManager.total);
		stringBuilderPool.AppendLine("[Objects] Texture Sub Assets: " + ActorTextureSubAsset.getTotal());
		stringBuilderPool.AppendLine();
		stringBuilderPool.AppendLine("[SpriteTextureLoader] Total Single Sprites: " + SpriteTextureLoader.total_sprites);
		stringBuilderPool.AppendLine("[SpriteTextureLoader] Total Sprites Lists: " + SpriteTextureLoader.total_sprites_list);
		stringBuilderPool.AppendLine("[SpriteTextureLoader] Total Sprites Lists Single Sprites: " + SpriteTextureLoader.total_sprites_list_single_sprites);
		stringBuilderPool.AppendLine();
		stringBuilderPool.AppendLine("[ActorAnimationLoader] Total Units: " + ActorAnimationLoader.count_units);
		stringBuilderPool.AppendLine("[ActorAnimationLoader] Total Heads: " + ActorAnimationLoader.count_heads);
		stringBuilderPool.AppendLine("[ActorAnimationLoader] Total Boats: " + ActorAnimationLoader.count_boats);
		stringBuilderPool.AppendLine();
		foreach (BaseAssetLibrary item2 in AssetManager.getList())
		{
			stringBuilderPool.AppendLine(item2.id + ": " + item2.total_items);
		}
		File.WriteAllTextAsync(path, stringBuilderPool.ToString());
	}
}
