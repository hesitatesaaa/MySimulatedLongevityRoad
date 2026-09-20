public class DynamicSpritesLibrary : AssetLibrary<DynamicSpritesAsset>
{
	public static DynamicSpritesAsset units;

	public static DynamicSpritesAsset boats;

	public static DynamicSpritesAsset units_shadows;

	public static DynamicSpritesAsset building_lights;

	public static DynamicSpritesAsset building_shadows;

	public static DynamicSpritesAsset icons;

	public static DynamicSpritesAsset items;

	public static DynamicSpritesAsset zombies;

	private bool _dirty;

	public static long _debug_id;

	private static long _debug_kingdom_color_id;

	private static long _debug_head_id;

	private static long _debug_main_body_sprite;

	private static long _debug_phenotype_index;

	private static long _debug_shade_id;

	public override void init()
	{
		base.init();
		units = add(new DynamicSpritesAsset
		{
			id = "units",
			atlas_id = UnitTextureAtlasID.Units
		});
		units_shadows = add(new DynamicSpritesAsset
		{
			id = "units_shadows",
			atlas_id = UnitTextureAtlasID.UnitsShadows
		});
		boats = add(new DynamicSpritesAsset
		{
			id = "boats",
			atlas_id = UnitTextureAtlasID.Boats
		});
		add(new DynamicSpritesAsset
		{
			id = "buildings",
			export_folder_path = "buildings",
			atlas_id = UnitTextureAtlasID.Buildings,
			buildings = true
		});
		add(new DynamicSpritesAsset
		{
			id = "buildings_trees",
			export_folder_path = "buildings_trees",
			check_wobbly_setting = true,
			atlas_id = UnitTextureAtlasID.BuildingsTrees,
			buildings = true
		});
		add(new DynamicSpritesAsset
		{
			id = "buildings_wobbly",
			export_folder_path = "buildings_wobbly",
			check_wobbly_setting = true,
			atlas_id = UnitTextureAtlasID.BuildingsWobbly,
			buildings = true
		});
		add(new DynamicSpritesAsset
		{
			id = "buildings_trees_big",
			export_folder_path = "buildings_trees_big",
			check_wobbly_setting = true,
			atlas_id = UnitTextureAtlasID.BuildingsTreesBig,
			buildings = true
		});
		building_lights = add(new DynamicSpritesAsset
		{
			id = "building_lights",
			atlas_id = UnitTextureAtlasID.BuildingsLights
		});
		building_shadows = add(new DynamicSpritesAsset
		{
			id = "building_shadows",
			atlas_id = UnitTextureAtlasID.BuildingsShadows
		});
		icons = add(new DynamicSpritesAsset
		{
			id = "icons",
			big_atlas = false,
			atlas_id = UnitTextureAtlasID.Icons
		});
		items = add(new DynamicSpritesAsset
		{
			id = "items",
			atlas_id = UnitTextureAtlasID.Items
		});
		zombies = add(new DynamicSpritesAsset
		{
			id = "zombies",
			atlas_id = UnitTextureAtlasID.Zombies
		});
	}

	public override DynamicSpritesAsset add(DynamicSpritesAsset pAsset)
	{
		base.add(pAsset);
		return pAsset;
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (DynamicSpritesAsset item in list)
		{
			item.create();
		}
	}

	public void clear()
	{
		foreach (DynamicSpritesAsset item in list)
		{
			item.clear();
		}
	}

	public void debug(DebugTool pTool, Actor pActor)
	{
		DynamicSpriteCreator.debug_actor = pActor;
		pTool.setText("preloaded_sprites_units:", ActorTextureSubAsset.all_preloaded_sprites_units.Count, 0f, pShowBar: false, 0L);
		pTool.setText("pixel_bags:", PixelBagManager.total, 0f, pShowBar: false, 0L);
		pTool.setText("total_sprite_textures:", SpriteTextureLoader.total_sprites, 0f, pShowBar: false, 0L);
		pTool.setText("total_sprites_lists:", SpriteTextureLoader.total_sprites_list, 0f, pShowBar: false, 0L);
		pTool.setText("total_sprites_lists_single_sprites:", SpriteTextureLoader.total_sprites_list_single_sprites, 0f, pShowBar: false, 0L);
		pTool.setText("total_texture_sub_assets:", ActorTextureSubAsset.getTotal(), 0f, pShowBar: false, 0L);
		pTool.setText("hand_renderer_textures:", HandRendererTexturePreloader.getTotal(), 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("[ActorAnimationLoader] units:", ActorAnimationLoader.count_units, 0f, pShowBar: false, 0L);
		pTool.setText("[ActorAnimationLoader] heads:", ActorAnimationLoader.count_heads, 0f, pShowBar: false, 0L);
		pTool.setText("[ActorAnimationLoader] boats:", ActorAnimationLoader.count_boats, 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		foreach (DynamicSpritesAsset item in AssetManager.dynamic_sprites_library.list)
		{
			pTool.setText("sprites " + item.id + ":", item.countSprites(), 0f, pShowBar: false, 0L);
			pTool.setText("textures " + item.id + ":", item.countTextures(), 0f, pShowBar: false, 0L);
		}
		pTool.setText("units:", units.getAtlas().debug(), 0f, pShowBar: false, 0L);
		pTool.setText("boats:", boats.getAtlas().debug(), 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("_debug_id:", _debug_id, 0f, pShowBar: false, 0L);
		pTool.setText("_debug_head_id:", _debug_head_id, 0f, pShowBar: false, 0L);
		pTool.setText("_debug_main_body_sprite:", _debug_main_body_sprite, 0f, pShowBar: false, 0L);
		pTool.setText("_debug_phenotype_index:", _debug_phenotype_index, 0f, pShowBar: false, 0L);
		pTool.setText("_debug_kingdom_color_id:", _debug_kingdom_color_id, 0f, pShowBar: false, 0L);
	}

	public void setDirty()
	{
		_dirty = true;
	}

	public void checkDirty()
	{
		if (!_dirty)
		{
			return;
		}
		_dirty = false;
		foreach (DynamicSpritesAsset item in AssetManager.dynamic_sprites_library.list)
		{
			item.checkAtlasDirty();
		}
	}

	public void setDebugActor(long pID, long pKingdomColorID, long pHeadID, long pMainBodySpriteID, long pPhenotypeIndex, long pShadeID)
	{
		_debug_id = pID;
		_debug_kingdom_color_id = pKingdomColorID;
		_debug_head_id = pHeadID;
		_debug_main_body_sprite = pMainBodySpriteID;
		_debug_phenotype_index = pPhenotypeIndex;
		_debug_shade_id = pShadeID;
	}

	public void export()
	{
	}
}
