using System;
using System.Collections.Generic;
using UnityEngine;
using ai.behaviours;
using life.taxi;

public class QuantumSpriteLibrary : AssetLibrary<QuantumSpriteAsset>
{
	public static QuantumSpriteAsset light_areas;

	private static readonly Sprite _sprite_pixel = SpriteTextureLoader.getSprite("effects/pixel_corner");

	private static readonly Sprite _sprite_attack_reload = SpriteTextureLoader.getSprite("ui/Icons/iconAttackReload");

	private static readonly Sprite _boat_sprite_small = SpriteTextureLoader.getSprite("civ/icons/minimap_boat_small");

	private static readonly Sprite _boat_sprite_big = SpriteTextureLoader.getSprite("civ/icons/minimap_boat_big");

	private static readonly Sprite[] _unexplored_sprites = SpriteTextureLoader.getSpriteList("effects/fx_unexplored");

	private static readonly Sprite[] _unit_selection_effect = SpriteTextureLoader.getSpriteList("effects/unit_selected_effect");

	private static readonly Sprite[] _unit_selection_effect_main = SpriteTextureLoader.getSpriteList("effects/unit_selected_effect_main");

	private static readonly Sprite[] _fire_sprites_1 = SpriteTextureLoader.getSpriteList("effects/fx_status_burning_t");

	private static readonly Sprite[] _fire_sprites_2 = SpriteTextureLoader.getSpriteList("effects/fx_status_burning_t_2");

	private static readonly Sprite[] _fire_sprites_3 = SpriteTextureLoader.getSpriteList("effects/fx_status_burning_t_3");

	private static readonly Sprite[][] _fire_sprites_sets = new Sprite[3][] { _fire_sprites_1, _fire_sprites_2, _fire_sprites_3 };

	private static readonly Sprite _king_sprite_normal = SpriteTextureLoader.getSprite("civ/icons/minimap_king_normal");

	private static readonly Sprite _king_sprite_angry = SpriteTextureLoader.getSprite("civ/icons/minimap_king_angry");

	private static readonly Sprite _king_sprite_surprised = SpriteTextureLoader.getSprite("civ/icons/minimap_king_surprised");

	private static readonly Sprite _king_sprite_happy = SpriteTextureLoader.getSprite("civ/icons/minimap_king_happy");

	private static readonly Sprite _king_sprite_sad = SpriteTextureLoader.getSprite("civ/icons/minimap_king_sad");

	private static readonly Sprite _leader_sprite_normal = SpriteTextureLoader.getSprite("civ/icons/minimap_leader_normal");

	private static readonly Sprite _leader_sprite_angry = SpriteTextureLoader.getSprite("civ/icons/minimap_leader_angry");

	private static readonly Sprite _leader_sprite_surprised = SpriteTextureLoader.getSprite("civ/icons/minimap_leader_surprised");

	private static readonly Sprite _leader_sprite_happy = SpriteTextureLoader.getSprite("civ/icons/minimap_leader_happy");

	private static readonly Sprite _leader_sprite_sad = SpriteTextureLoader.getSprite("civ/icons/minimap_leader_sad");

	private static readonly Sprite _flag_sprite = SpriteTextureLoader.getSprite("civ/icons/minimap_flag");

	public static double last_order_timestamp;

	private static int[] _q_render_indexes_units = new int[8192];

	private static int[] _q_render_indexes_shadows_units = new int[8192];

	private static int[] _q_render_indexes_shadows_buildings = new int[8192];

	private static int[] _q_render_indexes_sprites_fire = new int[4096];

	private static int[] _q_render_indexes_unit_items = new int[8192];

	private static readonly List<Vector3> _wars_pos_sword_main = new List<Vector3>();

	private static readonly List<Vector3> _wars_pos_shields_main = new List<Vector3>();

	private float _metas_fall_offset_timer;

	private MetaType _last_meta_type_metas;

	private const float STOCKPILE_ITEM_OFFSET = 0.4f;

	private const int STOCKPILE_MAX_STACKS = 7;

	private const int STOCKPILE_MAX_ROWS = 5;

	private const int STOCKPILE_MAX_COLUMNS = 7;

	private const float STOCKPILE_ROW_OFFSET = 0.58f;

	private const float STOCKPILE_COLUMN_OFFSET = 0.5f;

	private const float STOCKPILE_OFFSET_Z = 0.5f;

	private static Vector2[] _array_stockpile_slots;

	private const int MAX_SLOTS = 35;

	private static Actor[] visible_units => World.world.units.visible_units.array;

	private static int visible_units_count => World.world.units.visible_units.count;

	private static Actor[] visible_units_alive => World.world.units.visible_units_alive.array;

	private static int visible_units_alive_count => World.world.units.visible_units_alive.count;

	public override void init()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f11: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f16: Unknown result type (might be due to invalid IL or missing references)
		//IL_1510: Unknown result type (might be due to invalid IL or missing references)
		//IL_1515: Unknown result type (might be due to invalid IL or missing references)
		//IL_152f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1534: Unknown result type (might be due to invalid IL or missing references)
		//IL_159b: Unknown result type (might be due to invalid IL or missing references)
		//IL_15a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_15ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_15bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_1626: Unknown result type (might be due to invalid IL or missing references)
		//IL_162b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1645: Unknown result type (might be due to invalid IL or missing references)
		//IL_164a: Unknown result type (might be due to invalid IL or missing references)
		//IL_16b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_16b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_171d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1722: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b15: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b1a: Unknown result type (might be due to invalid IL or missing references)
		base.init();
		initDebugQuantumSpriteAssets();
		add(new QuantumSpriteAsset
		{
			id = "square_selection",
			id_prefab = "p_gameSprite",
			base_scale = 0.1f,
			arrow_animation = true,
			draw_call = drawSquareSelection,
			render_gameplay = true,
			turn_off_renderer = true,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("MapOverlay");
			},
			add_camera_zoom_multiplier_min = 0,
			add_camera_zoom_multiplier_max = 100,
			color = new Color(1f, 1f, 1f, 0.95f)
		});
		add(new QuantumSpriteAsset
		{
			id = "arrows_unit_cursor_destination",
			id_prefab = "p_mapArrow_stroke",
			base_scale = 0.1f,
			arrow_animation = true,
			draw_call = drawArrowsUnitCursor,
			render_gameplay = true,
			color = new Color(1f, 1f, 1f, 0.95f)
		});
		add(new QuantumSpriteAsset
		{
			id = "arrows_unit_cursor_destination_selected",
			id_prefab = "p_mapArrow_stroke",
			base_scale = 0.1f,
			arrow_animation = true,
			draw_call = drawArrowsUnitCursorSelected,
			render_gameplay = true,
			color = new Color(0.3f, 1f, 1f, 0.7f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_raycasts_controlled",
			id_prefab = "p_mapSprite",
			base_scale = 0.3f,
			render_gameplay = true,
			debug_option = DebugOption.ControlledUnitsAttackRaycast,
			draw_call = drawArrowsUnitCursorSelectedRaycasts,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_minis);
			}
		});
		add(new QuantumSpriteAsset
		{
			id = "arrows_unit_cursor_lover",
			id_prefab = "p_mapArrow_stroke",
			base_scale = 0.1f,
			arrow_animation = true,
			draw_call = drawArrowsUnitCursorLover,
			render_gameplay = true,
			color = new Color(1f, 0.4f, 0.77f, 0.95f)
		});
		add(new QuantumSpriteAsset
		{
			id = "arrows_unit_cursor_family",
			id_prefab = "p_mapArrow_stroke",
			base_scale = 0.05f,
			arrow_animation = true,
			draw_call = drawArrowsUnitCursorFamily,
			render_gameplay = true,
			color = new Color(1f, 1f, 0.28f, 0.7f)
		});
		add(new QuantumSpriteAsset
		{
			id = "arrows_unit_cursor_house",
			id_prefab = "p_mapArrow_stroke",
			base_scale = 0.05f,
			arrow_animation = true,
			draw_call = drawArrowsUnitCursorHouse,
			render_gameplay = true,
			color = new Color(0.2f, 0.72f, 0f, 0.95f)
		});
		add(new QuantumSpriteAsset
		{
			id = "cursor_arrow_parents",
			id_prefab = "p_mapArrow_stroke",
			base_scale = 0.05f,
			arrow_animation = true,
			draw_call = drawArrowsUnitCursorParents,
			render_gameplay = true,
			color = new Color(0.5f, 0.83f, 1f, 0.95f)
		});
		add(new QuantumSpriteAsset
		{
			id = "cursor_arrow_kids",
			id_prefab = "p_mapArrow_stroke",
			base_scale = 0.05f,
			arrow_animation = true,
			draw_call = drawArrowsUnitCursorKids,
			render_gameplay = true,
			color = new Color(0.63f, 0.16f, 0.92f, 0.95f)
		});
		add(new QuantumSpriteAsset
		{
			id = "cursor_arrow_attack_target",
			id_prefab = "p_mapArrow_stroke",
			base_scale = 0.05f,
			arrow_animation = true,
			draw_call = drawArrowsUnitCursorAttackTarget,
			render_gameplay = true,
			color = new Color(1f, 0f, 0f, 0.95f)
		});
		add(new QuantumSpriteAsset
		{
			id = "draw_walls",
			id_prefab = "p_mapSprite",
			add_camera_zoom_multiplier = false,
			turn_off_renderer = true,
			draw_call = drawWalls,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_world_object);
			},
			render_gameplay = true,
			default_amount = 500
		});
		add(new QuantumSpriteAsset
		{
			id = "draw_light_walls_light_blobs",
			id_prefab = "p_mapSprite",
			add_camera_zoom_multiplier = false,
			draw_call = drawWallLightBlobs,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_world_object);
			},
			render_gameplay = true,
			render_map = true
		});
		add(new QuantumSpriteAsset
		{
			id = "draw_lava_light_blobs",
			id_prefab = "p_mapSprite",
			add_camera_zoom_multiplier = false,
			draw_call = drawLavaLightBlobs,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_world_object);
			},
			render_gameplay = true,
			render_map = true
		});
		add(new QuantumSpriteAsset
		{
			id = "draw_units",
			id_prefab = "p_mapSprite",
			add_camera_zoom_multiplier = false,
			turn_off_renderer = true,
			draw_call = drawUnits,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_world_object);
			},
			render_gameplay = true,
			default_amount = 1000
		});
		add(new QuantumSpriteAsset
		{
			id = "draw_healthbars",
			id_prefab = "p_mapSprite",
			draw_call = drawHealthbars,
			turn_off_renderer = true,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("MapOverlay");
			},
			render_gameplay = true,
			add_camera_zoom_multiplier_min = 0,
			add_camera_zoom_multiplier_max = 100,
			default_amount = 100
		});
		add(new QuantumSpriteAsset
		{
			id = "draw_units_avatars",
			id_prefab = "p_mapSprite",
			add_camera_zoom_multiplier = false,
			draw_call = drawUnitsAvatars,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_world_object);
			},
			render_gameplay = true,
			render_map = true
		});
		add(new QuantumSpriteAsset
		{
			id = "unit_items",
			id_prefab = "p_mapSprite",
			base_scale = 0.15f,
			add_camera_zoom_multiplier = false,
			draw_call = drawUnitItems,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_world_object);
			},
			render_gameplay = true,
			default_amount = 200
		});
		add(new QuantumSpriteAsset
		{
			id = "draw_unit_hit_effect",
			id_prefab = "p_mapSprite",
			add_camera_zoom_multiplier = false,
			draw_call = drawUnitsEffectDamage,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_damaged);
			},
			render_gameplay = true,
			render_map = true
		});
		add(new QuantumSpriteAsset
		{
			id = "draw_parabolic_unload",
			id_prefab = "p_mapSprite",
			add_camera_zoom_multiplier = false,
			draw_call = drawParabolicUnload,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
			},
			render_gameplay = true,
			render_map = true
		});
		add(new QuantumSpriteAsset
		{
			id = "draw_unit_highlight_effect",
			id_prefab = "p_mapSprite",
			add_camera_zoom_multiplier = false,
			draw_call = drawUnitsEffectHighlight,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_highlighted);
			},
			render_gameplay = true,
			render_map = true
		});
		add(new QuantumSpriteAsset
		{
			id = "draw_buildings",
			id_prefab = "p_mapSprite",
			add_camera_zoom_multiplier = false,
			turn_off_renderer = true,
			draw_call = drawBuildings,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
			},
			render_gameplay = true,
			default_amount = 2000
		});
		add(new QuantumSpriteAsset
		{
			id = "draw_building_stockpiles",
			id_prefab = "p_mapSprite",
			add_camera_zoom_multiplier = false,
			turn_off_renderer = true,
			draw_call = drawStockpileResources,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
			},
			render_gameplay = true,
			default_amount = 100
		});
		add(new QuantumSpriteAsset
		{
			id = "projectiles",
			id_prefab = "p_mapSprite",
			render_gameplay = true,
			turn_off_renderer = true,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
			},
			draw_call = drawProjectiles,
			default_amount = 100
		});
		add(new QuantumSpriteAsset
		{
			id = "projectile_shadows",
			id_prefab = "p_shadow",
			turn_off_renderer = true,
			render_gameplay = true,
			draw_call = drawProjectileShadows,
			default_amount = 100
		});
		add(new QuantumSpriteAsset
		{
			id = "throwing_items_shadows",
			id_prefab = "p_shadow",
			turn_off_renderer = true,
			render_gameplay = true,
			draw_call = drawThrowingItemsShadows,
			default_amount = 100
		});
		add(new QuantumSpriteAsset
		{
			id = "shadows_buildings",
			id_prefab = "p_shadow",
			turn_off_renderer = true,
			render_gameplay = true,
			draw_call = drawShadowsBuildings,
			default_amount = 500
		});
		add(new QuantumSpriteAsset
		{
			id = "shadows_unit",
			id_prefab = "p_shadow",
			turn_off_renderer = true,
			render_gameplay = true,
			draw_call = drawShadowsUnit
		});
		add(new QuantumSpriteAsset
		{
			id = "unit_banners",
			id_prefab = "p_unitBanner",
			turn_off_renderer = true,
			render_gameplay = true,
			draw_call = drawUnitBanners
		});
		add(new QuantumSpriteAsset
		{
			id = "selected_units",
			id_prefab = "p_gameSprite",
			render_gameplay = true,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("EffectsBack");
			},
			draw_call = drawSelectedUnits
		});
		add(new QuantumSpriteAsset
		{
			id = "square_selection_to_select",
			id_prefab = "p_gameSprite",
			render_gameplay = true,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("EffectsBack");
			},
			draw_call = drawUnitsToBeSelectedBySquareTool
		});
		add(new QuantumSpriteAsset
		{
			id = "favorites_game",
			id_prefab = "p_gameSprite",
			render_gameplay = true,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				pQSprite.setSprite(SpriteTextureLoader.getSprite("ui/Icons/iconFavoriteStar"));
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
				((Renderer)pQSprite.sprite_renderer).sortingOrder = 1;
			},
			draw_call = drawFavoritesGame
		});
		add(new QuantumSpriteAsset
		{
			id = "favorites_items",
			id_prefab = "p_gameSprite",
			base_scale = 0.3f,
			render_map = true,
			selected_city_scale = true,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				pQSprite.setSprite(SpriteTextureLoader.getSprite("ui/Icons/iconFavoriteWeapon"));
			},
			draw_call = drawFavoriteItemsMap
		});
		add(new QuantumSpriteAsset
		{
			id = "unit_metas",
			id_prefab = "p_gameSprite",
			turn_off_renderer = true,
			base_scale = 0.3f,
			render_map = false,
			render_gameplay = true,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				pQSprite.setSprite(SpriteTextureLoader.getSprite("effects/unit_meta"));
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
			},
			draw_call = drawUnitMetas
		});
		add(new QuantumSpriteAsset
		{
			id = "happiness_icons",
			id_prefab = "p_gameSprite",
			turn_off_renderer = true,
			base_scale = 0.03f,
			render_gameplay = true,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
			},
			draw_call = drawUnitHappinessIcons
		});
		add(new QuantumSpriteAsset
		{
			id = "task_icons",
			id_prefab = "p_gameSprite",
			turn_off_renderer = true,
			base_scale = 0.04f,
			render_gameplay = true,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
			},
			draw_call = drawUnitTaskIcons
		});
		add(new QuantumSpriteAsset
		{
			id = "family_species_icons",
			id_prefab = "p_mapSprite",
			base_scale = 0.3f,
			add_camera_zoom_multiplier = false,
			draw_call = drawFamilySpeciesIcons,
			color = new Color(1f, 1f, 1f, 0.8f),
			create_object = delegate(QuantumSpriteAsset pAsset, QuantumSprite pQSprite)
			{
				pQSprite.setColor(ref pAsset.color);
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("EffectsBack");
				((Renderer)pQSprite.sprite_renderer).sortingOrder = 1;
			},
			render_map = true
		});
		add(new QuantumSpriteAsset
		{
			id = "favorites_map",
			id_prefab = "p_gameSprite",
			base_scale = 0.3f,
			render_map = true,
			selected_city_scale = true,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				pQSprite.setSprite(SpriteTextureLoader.getSprite("ui/Icons/iconFavoriteStar_Map"));
			},
			draw_call = drawFavoritesMap
		});
		add(new QuantumSpriteAsset
		{
			id = "status_effects",
			id_prefab = "p_gameSprite",
			render_gameplay = true,
			draw_call = drawStatusEffects,
			default_amount = 10
		});
		add(new QuantumSpriteAsset
		{
			id = "wars_lines",
			id_prefab = "p_mapArrow_arrows",
			line_width = 5,
			line_height = 7,
			arrow_animation = true,
			render_map = true,
			draw_call = drawWars
		});
		add(new QuantumSpriteAsset
		{
			id = "wars_icons",
			id_prefab = "p_mapSprite",
			render_map = true,
			draw_call = drawWarsIcons
		});
		add(new QuantumSpriteAsset
		{
			id = "plots",
			id_prefab = "p_plot",
			base_scale = 0.3f,
			render_map = true,
			render_gameplay = true,
			selected_city_scale = true,
			draw_call = drawPlots,
			default_amount = 10
		});
		add(new QuantumSpriteAsset
		{
			id = "plot_removals",
			id_prefab = "p_plot",
			base_scale = 0.3f,
			render_map = true,
			render_gameplay = true,
			selected_city_scale = true,
			draw_call = drawPlotRemovals,
			default_amount = 10
		});
		add(new QuantumSpriteAsset
		{
			id = "kings",
			id_prefab = "p_mapSprite",
			base_scale = 0.3f,
			render_map = true,
			selected_city_scale = true,
			draw_call = drawKings,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_minis);
			},
			default_amount = 10
		});
		add(new QuantumSpriteAsset
		{
			id = "leaders",
			id_prefab = "p_mapSprite",
			render_map = true,
			selected_city_scale = true,
			draw_call = drawLeaders,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_minis);
			},
			default_amount = 10
		});
		add(new QuantumSpriteAsset
		{
			id = "armies",
			id_prefab = "p_mapArmy",
			base_scale = 0.3f,
			render_map = true,
			selected_city_scale = true,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((QuantumSpriteWithText)pQSprite).initText();
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_minis);
			},
			draw_call = drawArmies,
			default_amount = 10
		});
		add(new QuantumSpriteAsset
		{
			id = "magnet_units",
			id_prefab = "p_mapSprite",
			render_map = true,
			render_gameplay = true,
			draw_call = drawMagnetUnits,
			default_amount = 10
		});
		add(new QuantumSpriteAsset
		{
			id = "boats_big",
			id_prefab = "p_mapSprite",
			base_scale = 0.3f,
			render_map = true,
			selected_city_scale = true,
			draw_call = drawBoatIcons,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_minis);
			},
			default_amount = 10
		});
		add(new QuantumSpriteAsset
		{
			id = "boats_small",
			id_prefab = "p_mapSprite",
			render_map = true,
			selected_city_scale = true,
			draw_call = drawBoatIcons,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_minis);
			},
			default_amount = 10
		});
		add(new QuantumSpriteAsset
		{
			id = "battles",
			id_prefab = "p_mapBattle",
			base_scale = 0.6f,
			flag_battle = true,
			path_icon = "civ/map_mark_battle_animation",
			render_map = true,
			draw_call = drawBattles,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_minis);
			},
			default_amount = 10
		});
		add(new QuantumSpriteAsset
		{
			id = "arrows_army_targets",
			id_prefab = "p_mapArrow_stroke",
			render_map = true,
			arrow_animation = true,
			base_scale = 0.3f,
			selected_city_scale = true,
			draw_call = drawArrowsArmyAttackTargets
		});
		add(new QuantumSpriteAsset
		{
			id = "highlight_cursor_zones",
			id_prefab = "p_mapZone",
			base_scale = 1f,
			draw_call = drawCursorZones,
			render_map = true,
			add_camera_zoom_multiplier = false,
			color = new Color(1f, 1f, 1f, 0.2f),
			color_2 = new Color(1f, 0.1f, 0.1f, 0.2f)
		});
		add(new QuantumSpriteAsset
		{
			id = "selected_kingdom",
			id_prefab = "p_mapZone",
			base_scale = 1f,
			draw_call = drawSelectedKingdomZones,
			render_map = true,
			add_camera_zoom_multiplier = false,
			color = new Color(1f, 1f, 1f, 0.4f),
			color_2 = new Color(1f, 0.1f, 0.1f, 0.2f)
		});
		add(new QuantumSpriteAsset
		{
			id = "whisper_of_war",
			id_prefab = "p_mapZone",
			base_scale = 1f,
			draw_call = drawWhisperOfWar,
			render_map = true,
			add_camera_zoom_multiplier = false,
			color = new Color(1f, 1f, 1f, 0.4f),
			color_2 = new Color(1f, 0.1f, 0.1f, 0.2f)
		});
		add(new QuantumSpriteAsset
		{
			id = "whisper_of_war_line",
			id_prefab = "p_mapArrow_line",
			base_scale = 0.5f,
			draw_call = drawWhisperOfWarLine,
			render_map = true,
			render_gameplay = true,
			color = new Color(0.4f, 0.4f, 1f, 0.9f)
		});
		add(new QuantumSpriteAsset
		{
			id = "unity_line",
			id_prefab = "p_mapArrow_line",
			base_scale = 0.5f,
			draw_call = drawUnityLine,
			render_map = true,
			render_gameplay = true,
			color = new Color(0.4f, 0.4f, 1f, 0.9f)
		});
		add(new QuantumSpriteAsset
		{
			id = "capturing_zones",
			id_prefab = "p_mapZone_lines",
			base_scale = 1f,
			draw_call = drawCapturingZones,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("EffectsBack");
				((Renderer)pQSprite.sprite_renderer).sortingOrder = 0;
			},
			render_map = true,
			add_camera_zoom_multiplier = false
		});
		add(new QuantumSpriteAsset
		{
			id = "ate_item",
			id_prefab = "p_mapSprite",
			base_scale = 0.15f,
			add_camera_zoom_multiplier = false,
			draw_call = drawJustAte,
			render_gameplay = true
		});
		add(new QuantumSpriteAsset
		{
			id = "socialize",
			id_prefab = "p_mapSprite",
			base_scale = 0.15f,
			add_camera_zoom_multiplier = false,
			draw_call = drawSocialize,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				pQSprite.setSharedMat(LibraryMaterials.instance.mat_socialize);
			},
			render_gameplay = true
		});
		add(new QuantumSpriteAsset
		{
			id = "cursor_power",
			id_prefab = "p_mapSprite",
			base_scale = 0.1f,
			draw_call = drawCursorSprite,
			add_camera_zoom_multiplier_min = 0,
			add_camera_zoom_multiplier_max = 100,
			render_gameplay = true,
			render_map = true
		});
		add(new QuantumSpriteAsset
		{
			id = "controlled_unit_attack_recharge",
			id_prefab = "p_attack_recharge",
			base_scale = 0.03f,
			draw_call = drawCursorAttackRecharge,
			add_camera_zoom_multiplier_min = 0,
			add_camera_zoom_multiplier_max = 100,
			render_gameplay = true,
			render_map = true
		});
		add(new QuantumSpriteAsset
		{
			id = "cursor_target_subspecies",
			id_prefab = "p_mapArrow_dna",
			base_scale = 0.1f,
			draw_call = drawCursorTargetSubspecies,
			arrow_animation = false,
			add_camera_zoom_multiplier_min = 0,
			add_camera_zoom_multiplier_max = 100,
			line_height = 6,
			line_width = 45,
			render_gameplay = true,
			render_map = true
		});
		add(new QuantumSpriteAsset
		{
			id = "buildings_light_windows",
			id_prefab = "p_windowLight",
			add_camera_zoom_multiplier = false,
			draw_call = drawBuildingsLightWindows,
			render_gameplay = true,
			render_map = true
		});
		light_areas = add(new QuantumSpriteAsset
		{
			id = "light_areas",
			id_prefab = "p_lightArea",
			add_camera_zoom_multiplier = false,
			draw_call = drawLightAreas,
			render_gameplay = true,
			render_map = true
		});
		add(new QuantumSpriteAsset
		{
			id = "fire_sprites",
			id_prefab = "p_mapSprite",
			base_scale = 0.15f,
			add_camera_zoom_multiplier = false,
			draw_call = drawFires,
			sound_idle = "event:/SFX/STATUS/StatusBurningBuilding",
			create_object = delegate(QuantumSpriteAsset pAsset, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
				pQSprite.setScale(pAsset.base_scale);
				pQSprite.sprite_renderer.sprite = _fire_sprites_1.GetRandom();
			},
			render_gameplay = true
		});
		add(new QuantumSpriteAsset
		{
			id = "unexplored_augmentations",
			id_prefab = "p_gameSprite",
			base_scale = 0.1f,
			add_camera_zoom_multiplier = false,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("Objects");
				((Renderer)pQSprite.sprite_renderer).sortingOrder = 1;
			},
			draw_call = drawUnexploredAugmentationSprite,
			color = new Color(1f, 1f, 1f, 0.8f),
			render_gameplay = true
		});
	}

	private void drawUnitHappinessIcons(QuantumSpriteAsset pAsset)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("icons_happiness"))
		{
			return;
		}
		float num = 18f;
		if (PlayerConfig.optionBoolEnabled("icons_tasks"))
		{
			num += 11f;
		}
		Actor[] array = visible_units_alive;
		int num2 = visible_units_alive_count;
		for (int i = 0; i < num2; i++)
		{
			Actor actor = array[i];
			if (actor.hasEmotions() && !actor.isInsideSomething())
			{
				float pForceScaleTo = actor.current_scale.y * 0.5f;
				Vector3 pPos = Vector2.op_Implicit(actor.current_position);
				pPos.z = num;
				pPos.y += num * actor.current_scale.y;
				Sprite spriteBasedOnHappinessValue = HappinessHelper.getSpriteBasedOnHappinessValue(actor.getHappiness());
				drawQuantumSprite(pAsset, pPos, null, null, null, null, 1f, pSetColor: false, pForceScaleTo).setSprite(spriteBasedOnHappinessValue);
			}
		}
	}

	private void drawUnitTaskIcons(QuantumSpriteAsset pAsset)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("icons_tasks"))
		{
			return;
		}
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		float num2 = 17.5f;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (!actor.isInsideSomething() && actor.asset.show_task_icon && actor.ai != null)
			{
				BehaviourTaskActor task = actor.ai.task;
				if (task != null && task.show_icon)
				{
					float pForceScaleTo = actor.current_scale.y * 0.5f;
					Vector3 pPos = Vector2.op_Implicit(actor.current_position);
					pPos.z = num2;
					pPos.y += num2 * actor.current_scale.y;
					drawQuantumSprite(pAsset, pPos, null, null, null, null, 1f, pSetColor: false, pForceScaleTo).setSprite(task.getSprite());
				}
			}
		}
	}

	private void drawUnitMetas(QuantumSpriteAsset pAsset)
	{
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		bool flag = PlayerConfig.optionBoolEnabled("unit_metas");
		bool flag2 = SelectedObjects.isNanoObjectSet();
		if (flag2)
		{
			flag = true;
		}
		if (!flag)
		{
			_last_meta_type_metas = MetaType.None;
			return;
		}
		_metas_fall_offset_timer += World.world.delta_time * 1f;
		if (_metas_fall_offset_timer > 1f)
		{
			_metas_fall_offset_timer = 1f;
		}
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		MetaType metaType = Zones.getCurrentMapBorderMode();
		if (metaType.isNone())
		{
			return;
		}
		bool flag3 = PlayerConfig.optionBoolEnabled("only_favorited_meta");
		NanoObject selectedNanoObject = SelectedObjects.getSelectedNanoObject();
		if (flag2)
		{
			metaType = selectedNanoObject.getMetaType();
		}
		if (_last_meta_type_metas != metaType)
		{
			_metas_fall_offset_timer = 0f;
		}
		_last_meta_type_metas = metaType;
		float num2 = (1f - iTween.easeOutBounce(0f, 1f, _metas_fall_offset_timer)) * 5f;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (actor.getMetaObjectOfType(metaType) is IMetaObject metaObject && (!flag2 || selectedNanoObject == metaObject) && (!flag3 || metaObject.isFavorite()))
			{
				ColorAsset color = metaObject.getColor();
				if (color == null)
				{
					Debug.LogError((object)("[drawUnitMetas] Forgot to set color asset for ? " + metaType));
					continue;
				}
				ref Color colorTextRef = ref color.getColorTextRef();
				QuantumSprite next = pAsset.group_system.getNext();
				Vector3 pScaleVec = actor.current_scale;
				Vector3 pPosition = Vector2.op_Implicit(actor.current_position);
				pPosition.y += num2;
				pPosition.z = -0.02f;
				next.setPosOnly(ref pPosition);
				next.setScale(ref pScaleVec);
				next.setColor(ref colorTextRef);
			}
		}
	}

	private static void showLightAt(Vector2 pPos, Color pColor, float pScale = 1f)
	{
		QuantumSprite next = light_areas.group_system.getNext();
		next.set(ref pPos, pScale);
		next.setColor(ref light_areas.color);
	}

	private static Color getColorForLight()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Color white = Color.white;
		if (MapBox.isRenderMiniMap())
		{
			white.a = World.world_era.era_effect_light_alpha_minimap;
			if (!World.world.zone_calculator.isModeNone())
			{
				white.a = 0.4f;
			}
		}
		else
		{
			white.a = World.world_era.era_effect_light_alpha_game;
		}
		return white;
	}

	private static void drawLightAreas(QuantumSpriteAsset pAsset)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("night_lights") || !World.world.era_manager.shouldShowLights())
		{
			return;
		}
		Color white = Color.white;
		Color colorForLight = getColorForLight();
		if (World.world.heat_ray_fx.isReady())
		{
			showLightAt(World.world.heat_ray_fx.getPosForLight(), white, 1.5f);
		}
		List<LightBlobData> light_blobs = World.world.stack_effects.light_blobs;
		if (light_blobs.Count > 0)
		{
			for (int i = 0; i < light_blobs.Count; i++)
			{
				LightBlobData lightBlobData = light_blobs[i];
				showLightAt(lightBlobData.position, white, lightBlobData.radius);
			}
		}
		if (MapBox.isRenderGameplay())
		{
			Actor[] array = visible_units;
			int num = visible_units_count;
			for (int j = 0; j < num; j++)
			{
				checkUnitLight(array[j], colorForLight);
			}
		}
		else
		{
			List<Actor> simpleList = World.world.units.getSimpleList();
			for (int k = 0; k < simpleList.Count; k++)
			{
				checkUnitLight(simpleList[k], colorForLight);
			}
		}
		if (World.world.quality_changer.shouldRenderBuildings())
		{
			int num2 = World.world.buildings.countVisibleBuildings();
			Building[] visibleBuildings = World.world.buildings.getVisibleBuildings();
			for (int l = 0; l < num2; l++)
			{
				checkBuildingLights(visibleBuildings[l], colorForLight);
			}
		}
		else
		{
			List<Building> simpleList2 = World.world.buildings.getSimpleList();
			for (int m = 0; m < simpleList2.Count; m++)
			{
				checkBuildingLights(simpleList2[m], colorForLight);
			}
		}
		if (MapBox.isRenderGameplay())
		{
			if (WorldBehaviourActionFire.hasFires())
			{
				List<TileZone> visibleZones = World.world.zone_camera.getVisibleZones();
				for (int n = 0; n < visibleZones.Count; n++)
				{
					TileZone tileZone = visibleZones[n];
					if (!WorldBehaviourActionFire.hasFires(tileZone))
					{
						continue;
					}
					WorldTile[] tiles = tileZone.tiles;
					int num3 = tiles.Length;
					for (int num4 = 0; num4 < num3; num4++)
					{
						WorldTile worldTile = tiles[num4];
						if (worldTile.isOnFire())
						{
							showLightAt(Vector2Int.op_Implicit(worldTile.pos), colorForLight, 0.2f);
						}
					}
				}
			}
		}
		else if (WorldBehaviourActionFire.hasFires())
		{
			foreach (TileZone zone in World.world.zone_calculator.zones)
			{
				if (!WorldBehaviourActionFire.hasFires(zone))
				{
					continue;
				}
				WorldTile[] tiles2 = zone.tiles;
				int num5 = tiles2.Length;
				for (int num6 = 0; num6 < num5; num6++)
				{
					WorldTile worldTile2 = tiles2[num6];
					if (worldTile2.isOnFire())
					{
						showLightAt(Vector2Int.op_Implicit(worldTile2.pos), colorForLight, 0.2f);
					}
				}
			}
		}
		if ((Config.isComputer || Config.isEditor) && PlayerConfig.optionBoolEnabled("cursor_lights"))
		{
			showLightAt(Vector2.op_Implicit(Vector2.op_Implicit(World.world.getMousePos())), white, 0.4f);
		}
	}

	private static void checkBuildingLights(Building pBuilding, Color pColor)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (pBuilding.hasAnyStatusEffect())
		{
			foreach (Status status in pBuilding.getStatuses())
			{
				if (status.asset.draw_light_area)
				{
					showLightAt(pBuilding.current_position, pColor, status.asset.draw_light_size);
				}
			}
		}
		if (pBuilding.asset.draw_light_area && pBuilding.isUsable() && !pBuilding.isAbandoned() && (!pBuilding.asset.hasHousingSlots() || pBuilding.hasResidents()))
		{
			Vector3 val = Vector2.op_Implicit(pBuilding.current_position);
			val.x += pBuilding.asset.draw_light_area_offset_x;
			val.y += pBuilding.asset.draw_light_area_offset_y;
			showLightAt(Vector2.op_Implicit(val), pColor, pBuilding.asset.draw_light_size);
		}
	}

	private static void checkUnitLight(Actor pActor, Color pColor)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (pActor.a.has_tag_generate_light)
		{
			Vector2 current_position = pActor.current_position;
			current_position.y += pActor.getHeight();
			showLightAt(current_position, pColor, 0.3f);
		}
		else
		{
			if (!pActor.hasAnyStatusEffect())
			{
				return;
			}
			foreach (Status status in pActor.getStatuses())
			{
				if (status.asset.draw_light_area)
				{
					showLightAt(pActor.current_position, pColor, status.asset.draw_light_size);
				}
			}
		}
	}

	private static void drawBuildingsLightWindows(QuantumSpriteAsset pAsset)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (!World.world.quality_changer.shouldRenderBuildings() || !World.world.era_manager.shouldShowLights() || !PlayerConfig.optionBoolEnabled("night_lights"))
		{
			return;
		}
		Color white = Color.white;
		if (Randy.randomBool())
		{
			white.a = 0.95f;
		}
		else
		{
			white.a = 1f;
		}
		int num = World.world.buildings.countVisibleBuildings();
		Building[] visibleBuildings = World.world.buildings.getVisibleBuildings();
		for (int i = 0; i < num; i++)
		{
			Building building = visibleBuildings[i];
			if (building.asset.city_building && building.isUsable() && !building.isAbandoned() && (!building.asset.hasHousingSlots() || building.hasResidents()))
			{
				Sprite buildingLight = DynamicSprites.getBuildingLight(building);
				if (!((Object)(object)buildingLight == (Object)null))
				{
					Vector3 cur_transform_position = building.cur_transform_position;
					cur_transform_position.z = -0.19f;
					drawQuantumSprite(pAsset, cur_transform_position, null, null, null, null, 1f, pSetColor: false, building.getCurrentScale().y).setSprite(buildingLight);
				}
			}
		}
	}

	private static void drawFamilySpeciesIcons(QuantumSpriteAsset pAsset)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("map_species_families"))
		{
			return;
		}
		foreach (Family family in World.world.families)
		{
			if (!family.isAlive())
			{
				continue;
			}
			ActorAsset actorAsset = family.getActorAsset();
			if (family.units.Count != 0)
			{
				Actor actor = family.units[0];
				if (!actor.isRekt() && actor.current_zone.visible)
				{
					Sprite spriteIcon = actorAsset.getSpriteIcon();
					drawQuantumSprite(pAsset, actor.current_tile.zone.centerTile.posV3).setSprite(spriteIcon);
				}
			}
		}
	}

	private static void drawCursorTargetSubspecies(QuantumSpriteAsset pAsset)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (!MapBox.isRenderGameplay() || !InputHelpers.mouseSupported || (Object)(object)World.world.selected_buttons.selectedButton == (Object)null || World.world.isBusyWithUI() || ControllableUnit.isControllingUnit() || MoveCamera.inSpectatorMode() || Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
		{
			return;
		}
		WorldTile mouseTilePosCachedFrame = World.world.getMouseTilePosCachedFrame();
		if (mouseTilePosCachedFrame == null)
		{
			return;
		}
		GodPower selectedPowerAsset = World.world.getSelectedPowerAsset();
		if (selectedPowerAsset.type == PowerActionType.PowerSpawnActor)
		{
			ActorAsset actorAsset = selectedPowerAsset.getActorAsset();
			if (actorAsset != null && actorAsset.can_have_subspecies && World.world.subspecies.getNearbySpecies(actorAsset, mouseTilePosCachedFrame, out var pSubspeciesActor) != null && pSubspeciesActor.is_visible)
			{
				Vector3 headOffsetPositionForFunRendering = pSubspeciesActor.getHeadOffsetPositionForFunRendering();
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(World.world.getMousePos()), headOffsetPositionForFunRendering, ref Toolbox.color_white);
			}
		}
	}

	private static void drawCursorSprite(QuantumSpriteAsset pAsset)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		if (InputHelpers.mouseSupported && !((Object)(object)World.world.selected_buttons.selectedButton == (Object)null) && !World.world.isBusyWithUI() && !ControllableUnit.isControllingUnit() && !MoveCamera.inSpectatorMode() && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2) && !World.world.getSelectedPowerAsset().ignore_cursor_icon)
		{
			float cameraScaleZoomMultiplier = getCameraScaleZoomMultiplier(pAsset);
			Vector2 mousePos = World.world.getMousePos();
			mousePos.x += -0.3f * cameraScaleZoomMultiplier;
			mousePos.y += -0.3f * cameraScaleZoomMultiplier;
			QuantumSprite quantumSprite = drawQuantumSprite(pAsset, Vector2.op_Implicit(mousePos));
			quantumSprite.setSprite(World.world.selected_buttons.selectedButton.icon.sprite);
			mousePos.x += 0.3f * cameraScaleZoomMultiplier;
			mousePos.y += 0.3f * cameraScaleZoomMultiplier;
			Color pColor = Toolbox.color_black;
			quantumSprite.setSprite(World.world.selected_buttons.selectedButton.icon.sprite);
			pColor.a = 0.3f;
			quantumSprite.setColor(ref pColor);
			((Renderer)quantumSprite.sprite_renderer).sortingOrder = 9;
			QuantumSprite quantumSprite2 = drawQuantumSprite(pAsset, Vector2.op_Implicit(mousePos));
			quantumSprite2.setSprite(World.world.selected_buttons.selectedButton.icon.sprite);
			((Renderer)quantumSprite2.sprite_renderer).sortingOrder = 10;
		}
	}

	private static void drawCursorAttackRecharge(QuantumSpriteAsset pAsset)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (InputHelpers.mouseSupported && !World.world.isBusyWithUI() && ControllableUnit.isControllingUnit())
		{
			Actor controllableUnit = ControllableUnit.getControllableUnit();
			if (!(controllableUnit.asset.id == "crabzilla") && !controllableUnit.isAttackReady())
			{
				float attackCooldownRatio = controllableUnit.getAttackCooldownRatio();
				float cameraScaleZoomMultiplier = getCameraScaleZoomMultiplier(pAsset);
				Vector2 mousePos = World.world.getMousePos();
				mousePos.x += 2.5f * cameraScaleZoomMultiplier;
				mousePos.y -= 2.5f * cameraScaleZoomMultiplier;
				CircleIconShaderMod component = ((Component)drawQuantumSprite(pAsset, Vector2.op_Implicit(mousePos))).GetComponent<CircleIconShaderMod>();
				component.sprite_renderer_with_mat.sprite = _sprite_attack_reload;
				component.setShaderVal(attackCooldownRatio);
			}
		}
	}

	private static void drawUnexploredAugmentationSprite(QuantumSpriteAsset pQAsset)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!PowerLibrary.inspect_unit.isSelected() || WorldLawLibrary.world_law_cursed_world.isEnabled())
		{
			return;
		}
		Sprite spriteFromListSessionTime = AnimationHelper.getSpriteFromListSessionTime(0, _unexplored_sprites, SimGlobals.m.unexplored_sprite_animation_speed);
		for (int i = 0; i < visible_units_alive_count; i++)
		{
			Actor actor = visible_units_alive[i];
			if (checkShouldDrawUnexploredSpriteFor(actor))
			{
				Vector3 headOffsetPositionForFunRendering = actor.getHeadOffsetPositionForFunRendering();
				drawQuantumSprite(pQAsset, headOffsetPositionForFunRendering, null, null, null, null, 1f, pSetColor: false, actor.current_scale.y).setSprite(spriteFromListSessionTime);
			}
		}
	}

	private static bool checkShouldDrawUnexploredSpriteFor(Actor pActor)
	{
		if (pActor.asset.is_boat)
		{
			return false;
		}
		ActorAsset asset = pActor.asset;
		if (!asset.isAvailable() && asset.needs_to_be_explored)
		{
			return true;
		}
		if (pActor.hasEquipment())
		{
			foreach (Item item in pActor.equipment.getItems())
			{
				EquipmentAsset asset2 = item.getAsset();
				if (!asset2.isAvailable() && !asset2.unlocked_with_achievement && asset2.needs_to_be_explored)
				{
					return true;
				}
			}
		}
		if (checkAssetsForUnexplored(pActor.traits))
		{
			return true;
		}
		if (pActor.hasClan() && checkAssetsForUnexplored(pActor.clan.getTraits()))
		{
			return true;
		}
		if (pActor.hasCulture() && checkAssetsForUnexplored(pActor.culture.getTraits()))
		{
			return true;
		}
		if (pActor.hasLanguage() && checkAssetsForUnexplored(pActor.language.getTraits()))
		{
			return true;
		}
		if (pActor.hasReligion() && checkAssetsForUnexplored(pActor.religion.getTraits()))
		{
			return true;
		}
		if (pActor.hasSubspecies() && checkAssetsForUnexplored(pActor.subspecies.getTraits()))
		{
			return true;
		}
		if (pActor.hasKingdom() && checkAssetsForUnexplored(pActor.kingdom.getTraits()))
		{
			return true;
		}
		return false;
	}

	private static bool checkAssetsForUnexplored(IReadOnlyCollection<BaseUnlockableAsset> pAssets)
	{
		foreach (BaseUnlockableAsset pAsset in pAssets)
		{
			if (!pAsset.isAvailable() && !pAsset.unlocked_with_achievement && pAsset.needs_to_be_explored)
			{
				return true;
			}
		}
		return false;
	}

	private static void drawBuildingsOld(QuantumSpriteAsset pAsset)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		int num = World.world.buildings.countVisibleBuildings();
		Building[] visibleBuildings = World.world.buildings.getVisibleBuildings();
		QuantumSprite[] fastActiveList = pAsset.group_system.getFastActiveList(num);
		for (int i = 0; i < num; i++)
		{
			Building obj = visibleBuildings[i];
			QuantumSprite quantumSprite = fastActiveList[i];
			Sprite sprite = obj.checkSpriteToRender();
			Vector3 pScaleVec = obj.getCurrentScale();
			Vector3 pPosition = obj.cur_transform_position;
			Vector3 pVec = obj.current_rotation;
			Material material = obj.material;
			bool flip_x = obj.flip_x;
			Color pColor = obj.kingdom.asset.color_building;
			quantumSprite.setSprite(sprite);
			quantumSprite.setScale(ref pScaleVec);
			quantumSprite.setSharedMat(material);
			quantumSprite.setPosOnly(ref pPosition);
			quantumSprite.setRotation(ref pVec);
			quantumSprite.setFlipX(flip_x);
			quantumSprite.setColor(ref pColor);
		}
	}

	private static void drawBuildingsCache(QuantumSpriteAsset pAsset)
	{
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		BuildingRenderData render_data = World.world.buildings.render_data;
		int num = World.world.buildings.countVisibleBuildings();
		QuantumSprite[] fastActiveList = pAsset.group_system.getFastActiveList(num);
		QuantumSpriteCacheData cacheData = pAsset.group_system.getCacheData(num);
		Sprite[] colored_sprites = render_data.colored_sprites;
		Material[] materials = render_data.materials;
		Vector3[] scales = render_data.scales;
		Vector3[] positions = render_data.positions;
		Vector3[] rotations = render_data.rotations;
		bool[] flip_x_states = render_data.flip_x_states;
		Color[] colors = render_data.colors;
		Sprite[] sprites = cacheData.sprites;
		Material[] materials2 = cacheData.materials;
		Vector3[] scales2 = cacheData.scales;
		Vector3[] positions2 = cacheData.positions;
		Vector3[] rotations2 = cacheData.rotations;
		bool[] flip_x_states2 = cacheData.flip_x_states;
		Color[] colors2 = cacheData.colors;
		for (int i = 0; i < num; i++)
		{
			Sprite val = colored_sprites[i];
			if (sprites[i] != val)
			{
				sprites[i] = val;
				fastActiveList[i].sprite_renderer.sprite = val;
			}
		}
		for (int j = 0; j < num; j++)
		{
			Material val2 = materials[j];
			if (materials2[j] != val2)
			{
				materials2[j] = val2;
				((Renderer)fastActiveList[j].sprite_renderer).sharedMaterial = val2;
			}
		}
		for (int k = 0; k < num; k++)
		{
			ref Vector3 reference = ref scales[k];
			ref Vector3 reference2 = ref scales2[k];
			if (reference.x != reference2.x || reference.y != reference2.y || reference.z != reference2.z)
			{
				reference2 = reference;
				fastActiveList[k].m_transform.localScale = reference;
			}
		}
		for (int l = 0; l < num; l++)
		{
			ref Vector3 reference3 = ref positions[l];
			ref Vector3 reference4 = ref positions2[l];
			if (reference3.x != reference4.x || reference3.y != reference4.y || reference3.z != reference4.z)
			{
				reference4 = reference3;
				fastActiveList[l].m_transform.position = reference3;
			}
		}
		for (int m = 0; m < num; m++)
		{
			ref Vector3 reference5 = ref rotations[m];
			ref Vector3 reference6 = ref rotations2[m];
			if (reference5.x != reference6.x || reference5.y != reference6.y || reference5.z != reference6.z)
			{
				reference6 = reference5;
				fastActiveList[m].m_transform.eulerAngles = reference5;
			}
		}
		for (int n = 0; n < num; n++)
		{
			ref bool reference7 = ref flip_x_states[n];
			ref bool reference8 = ref flip_x_states2[n];
			if (reference7 != reference8)
			{
				reference8 = reference7;
				fastActiveList[n].sprite_renderer.flipX = reference7;
			}
		}
		for (int num2 = 0; num2 < num; num2++)
		{
			ref Color reference9 = ref colors[num2];
			ref Color reference10 = ref colors2[num2];
			if (reference9.r != reference10.r || reference9.g != reference10.g || reference9.b != reference10.b || reference9.a != reference10.a)
			{
				reference10 = reference9;
				fastActiveList[num2].sprite_renderer.color = reference9;
			}
		}
	}

	private static void drawBuildings(QuantumSpriteAsset pAsset)
	{
		BuildingRenderData render_data = World.world.buildings.render_data;
		int num = World.world.buildings.countVisibleBuildings();
		QuantumSprite[] fastActiveList = pAsset.group_system.getFastActiveList(num);
		Sprite[] colored_sprites = render_data.colored_sprites;
		Material[] materials = render_data.materials;
		Vector3[] scales = render_data.scales;
		Vector3[] positions = render_data.positions;
		Vector3[] rotations = render_data.rotations;
		bool[] flip_x_states = render_data.flip_x_states;
		Color[] colors = render_data.colors;
		for (int i = 0; i < num; i++)
		{
			QuantumSprite obj = fastActiveList[i];
			Sprite sprite = colored_sprites[i];
			obj.setSprite(sprite);
			Material sharedMat = materials[i];
			obj.setSharedMat(sharedMat);
			obj.setScale(ref scales[i]);
			obj.setPosOnly(ref positions[i]);
			obj.setRotation(ref rotations[i]);
			obj.setFlipX(flip_x_states[i]);
			obj.setColor(ref colors[i]);
		}
	}

	private static void drawParabolicUnload(QuantumSpriteAsset pAsset)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		List<ResourceThrowData> list = World.world.resource_throw_manager.getList();
		QuantumSprite[] fastActiveList = pAsset.group_system.getFastActiveList(list.Count);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			ResourceThrowData resourceThrowData = list[num];
			QuantumSprite obj = fastActiveList[num];
			float ratio = resourceThrowData.getRatio();
			Vector3 pPosition = Vector2.op_Implicit(Toolbox.Parabola(resourceThrowData.position_start, resourceThrowData.position_end, resourceThrowData.height, ratio));
			pPosition.z = 4f;
			float scale = 0.1f;
			Sprite gameplaySprite = AssetManager.resources.get(resourceThrowData.resource_asset_id).getGameplaySprite();
			obj.setSprite(gameplaySprite);
			obj.setPosOnly(ref pPosition);
			obj.setScale(scale);
			((Component)obj).transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, ratio * 360f));
		}
	}

	private static void drawUnitsEffectDamage(QuantumSpriteAsset pAsset)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		List<ActorDamageEffectData> actor_effect_hit = World.world.stack_effects.actor_effect_hit;
		for (int num = actor_effect_hit.Count - 1; num >= 0; num--)
		{
			ActorDamageEffectData actorDamageEffectData = actor_effect_hit[num];
			float realTimeElapsedSince = World.world.getRealTimeElapsedSince(actorDamageEffectData.timestamp);
			Actor actor = actorDamageEffectData.actor;
			if (realTimeElapsedSince > 0.3f || !actor.isAlive() || !actor.is_visible)
			{
				actor_effect_hit.RemoveAt(num);
			}
			else
			{
				QuantumSprite next = pAsset.group_system.getNext();
				Vector3 pVec = actor.updateRotation();
				Vector3 pScaleVec = actor.current_scale;
				Vector3 pPosition = actor.cur_transform_position;
				Sprite sprite = actor.checkSpriteToRender();
				Color pColor = Color.white;
				pColor.a = 1f - realTimeElapsedSince / 0.3f;
				next.setSprite(sprite);
				next.setPosOnly(ref pPosition);
				next.setScale(ref pScaleVec);
				next.setRotation(ref pVec);
				next.setColor(ref pColor);
			}
		}
	}

	private static void drawUnitsEffectHighlight(QuantumSpriteAsset pAsset)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		List<ActorHighlightEffectData> actor_effect_highlight = World.world.stack_effects.actor_effect_highlight;
		for (int num = actor_effect_highlight.Count - 1; num >= 0; num--)
		{
			ActorHighlightEffectData actorHighlightEffectData = actor_effect_highlight[num];
			float num2 = World.world.getRealTimeElapsedSince(actorHighlightEffectData.timestamp);
			Actor actor = actorHighlightEffectData.actor;
			if (num2 > 0.3f || !actor.isAlive() || !actor.is_visible)
			{
				actor_effect_highlight.RemoveAt(num);
			}
			else
			{
				QuantumSprite next = pAsset.group_system.getNext();
				Vector3 pVec = actor.updateRotation();
				Vector3 pScaleVec = actor.current_scale;
				Vector3 pPosition = actor.cur_transform_position;
				Sprite sprite = actor.checkSpriteToRender();
				Color pColor = Color.white;
				pColor.a = 1f - num2 / 0.3f;
				next.setSprite(sprite);
				next.setPosOnly(ref pPosition);
				next.setScale(ref pScaleVec);
				next.setRotation(ref pVec);
				next.setColor(ref pColor);
			}
		}
	}

	private static void drawSquareSelection(QuantumSpriteAsset pAsset)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		if (World.world.player_control.square_selection_started)
		{
			float cameraScaleZoomMultiplier = getCameraScaleZoomMultiplier(pAsset);
			Color pColor = World.world.getArchitectColor();
			Vector2 square_selection_position_current = World.world.player_control.square_selection_position_current;
			Vector2 mousePos = World.world.getMousePos();
			float num = mousePos.x - square_selection_position_current.x;
			float num2 = mousePos.y - square_selection_position_current.y;
			float num3 = 0.1f * cameraScaleZoomMultiplier;
			Color pColor2 = pColor;
			pColor2.a = 0.3f;
			QuantumSprite quantumSprite = drawQuantumSprite(pAsset, Vector2.op_Implicit(square_selection_position_current));
			quantumSprite.setSprite(_sprite_pixel);
			((Component)quantumSprite).transform.localScale = new Vector3(num, num2);
			quantumSprite.setColor(ref pColor2);
			QuantumSprite quantumSprite2 = drawQuantumSprite(pAsset, Vector2.op_Implicit(square_selection_position_current));
			quantumSprite2.setSprite(_sprite_pixel);
			((Component)quantumSprite2).transform.localScale = new Vector3(num, num3);
			quantumSprite2.setColor(ref pColor);
			QuantumSprite quantumSprite3 = drawQuantumSprite(pAsset, Vector2.op_Implicit(square_selection_position_current));
			quantumSprite3.setSprite(_sprite_pixel);
			((Component)quantumSprite3).transform.localScale = new Vector3(num3, num2);
			quantumSprite3.setColor(ref pColor);
			QuantumSprite quantumSprite4 = drawQuantumSprite(pAsset, Vector2.op_Implicit(mousePos));
			quantumSprite4.setSprite(_sprite_pixel);
			((Component)quantumSprite4).transform.localScale = new Vector3(0f - num, num3);
			quantumSprite4.setColor(ref pColor);
			QuantumSprite quantumSprite5 = drawQuantumSprite(pAsset, Vector2.op_Implicit(mousePos));
			quantumSprite5.setSprite(_sprite_pixel);
			((Component)quantumSprite5).transform.localScale = new Vector3(num3, 0f - num2);
			quantumSprite5.setColor(ref pColor);
		}
	}

	private static void drawArrowsUnitCursor(QuantumSpriteAsset pAsset)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("cursor_arrow_destination") || ControllableUnit.isControllingUnit())
		{
			return;
		}
		Actor last_actor = UnitSelectionEffect.last_actor;
		if (!last_actor.isRekt() && !last_actor.isInMagnet())
		{
			WorldTile current_tile = last_actor.current_tile;
			WorldTile tile_target = last_actor.tile_target;
			if (current_tile != null && tile_target != null)
			{
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(last_actor.current_position), tile_target.posV3, ref pAsset.color);
			}
			if (last_actor.has_attack_target && last_actor.isEnemyTargetAlive())
			{
				QuantumSpriteAsset quantumSpriteAsset = AssetManager.quantum_sprites.get("debug_arrows_units_attack_targets");
				drawArrowQuantumSprite(quantumSpriteAsset, Vector2.op_Implicit(last_actor.current_position), Vector2.op_Implicit(last_actor.attack_target.current_position), ref quantumSpriteAsset.color);
			}
		}
	}

	private static void drawArrowsUnitCursorSelectedRaycasts(QuantumSpriteAsset pAsset)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (!ControllableUnit.isControllingUnit() || !Input.GetKey((KeyCode)304))
		{
			return;
		}
		foreach (Actor cotrolledUnit in ControllableUnit.getCotrolledUnits())
		{
			if (cotrolledUnit.isRekt() || cotrolledUnit.isInMagnet())
			{
				break;
			}
			Vector2 current_position = cotrolledUnit.current_position;
			Vector2 mousePos = World.world.getMousePos();
			Color pColor = Color.red;
			Color pColor2 = Color.white;
			Color pColor3 = Color.black;
			List<WorldTile> list = PathfinderTools.raycast(current_position, mousePos);
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				WorldTile worldTile = list[i];
				float pForceScaleTo = 0.05f + (float)i * 0.1f * 0.05f;
				bool flag2 = false;
				if (i > 0 && worldTile.countUnits() > 0)
				{
					flag = true;
					flag2 = true;
				}
				float num = Toolbox.getAngleDegrees(worldTile.x, worldTile.y, mousePos.x, mousePos.y) - 45f;
				QuantumSprite quantumSprite = drawQuantumSprite(pAsset, worldTile.posV3, null, null, null, null, 1f, pSetColor: false, pForceScaleTo);
				quantumSprite.setSprite(SpriteTextureLoader.getSprite("ui/Icons/iconAttack"));
				((Component)quantumSprite).transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, num));
				if (flag2)
				{
					quantumSprite.setColor(ref pColor);
				}
				else if (flag)
				{
					quantumSprite.setColor(ref pColor3);
				}
				else
				{
					quantumSprite.setColor(ref pColor2);
				}
			}
		}
	}

	private static void drawArrowsUnitCursorSelected(QuantumSpriteAsset pAsset)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("cursor_arrow_destination") || ControllableUnit.isControllingUnit())
		{
			return;
		}
		float num = World.world.getRealTimeElapsedSince(last_order_timestamp);
		if (num > 2f)
		{
			return;
		}
		float num2 = 1f - num / 2f;
		Color pColor = World.world.getArchitectColor();
		int num3 = 0;
		foreach (Actor allSelected in SelectedUnit.getAllSelectedList())
		{
			if (allSelected.isRekt() || allSelected.isInMagnet())
			{
				break;
			}
			if (SelectedUnit.isMainSelected(allSelected))
			{
				pColor.a = num2;
			}
			else
			{
				pColor.a = num2 * 0.4f;
			}
			WorldTile current_tile = allSelected.current_tile;
			WorldTile tile_target = allSelected.tile_target;
			if (current_tile != null && tile_target != null)
			{
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(allSelected.current_position), tile_target.posV3, ref pColor);
			}
			num3++;
			if (num3 > 20)
			{
				break;
			}
		}
	}

	private static void drawArrowsUnitCursorLover(QuantumSpriteAsset pAsset)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerConfig.optionBoolEnabled("cursor_arrow_lover") && !ControllableUnit.isControllingUnit())
		{
			Actor last_actor = UnitSelectionEffect.last_actor;
			if (!last_actor.isRekt() && !last_actor.isInMagnet() && last_actor.hasLover())
			{
				Vector3 pStart = Vector2.op_Implicit(last_actor.current_position);
				Vector3 pEnd = Vector2.op_Implicit(last_actor.lover.current_position);
				drawArrowQuantumSprite(pAsset, pStart, pEnd, ref pAsset.color);
			}
		}
	}

	private static void drawArrowsUnitCursorHouse(QuantumSpriteAsset pAsset)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerConfig.optionBoolEnabled("cursor_arrow_house") && !ControllableUnit.isControllingUnit())
		{
			Actor last_actor = UnitSelectionEffect.last_actor;
			if (!last_actor.isRekt() && !last_actor.isInMagnet() && last_actor.hasHouse())
			{
				Vector3 pStart = Vector2.op_Implicit(last_actor.current_position);
				Vector3 pEnd = Vector2.op_Implicit(last_actor.getHomeBuilding().current_position);
				drawArrowQuantumSprite(pAsset, pStart, pEnd, ref pAsset.color);
			}
		}
	}

	private static void drawArrowsUnitCursorFamily(QuantumSpriteAsset pAsset)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("cursor_arrow_family") || ControllableUnit.isControllingUnit())
		{
			return;
		}
		Actor last_actor = UnitSelectionEffect.last_actor;
		if (last_actor.isRekt() || last_actor.isInMagnet() || !last_actor.hasFamily())
		{
			return;
		}
		Vector3 pStart = Vector2.op_Implicit(last_actor.current_position);
		foreach (Actor unit in last_actor.family.units)
		{
			if (unit != last_actor && !unit.isRekt())
			{
				Vector3 pEnd = Vector2.op_Implicit(unit.current_position);
				drawArrowQuantumSprite(pAsset, pStart, pEnd, ref pAsset.color);
			}
		}
	}

	private static void drawArrowsUnitCursorParents(QuantumSpriteAsset pAsset)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("cursor_arrow_parents") || ControllableUnit.isControllingUnit())
		{
			return;
		}
		Actor last_actor = UnitSelectionEffect.last_actor;
		if (last_actor.isRekt() || last_actor.isInMagnet())
		{
			return;
		}
		Vector3 pStart = Vector2.op_Implicit(last_actor.current_position);
		foreach (Actor parent in last_actor.getParents())
		{
			Vector3 pEnd = Vector2.op_Implicit(parent.current_position);
			drawArrowQuantumSprite(pAsset, pStart, pEnd, ref pAsset.color);
		}
	}

	private static void drawArrowsUnitCursorKids(QuantumSpriteAsset pAsset)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("cursor_arrow_kids") || ControllableUnit.isControllingUnit())
		{
			return;
		}
		Actor last_actor = UnitSelectionEffect.last_actor;
		if (last_actor.isRekt() || last_actor.isInMagnet())
		{
			return;
		}
		Vector3 pStart = Vector2.op_Implicit(last_actor.current_position);
		foreach (Actor child in last_actor.getChildren(pOnlyCurrentFamily: false))
		{
			Vector3 pEnd = Vector2.op_Implicit(child.current_position);
			drawArrowQuantumSprite(pAsset, pStart, pEnd, ref pAsset.color);
		}
	}

	private static void drawArrowsUnitCursorAttackTarget(QuantumSpriteAsset pAsset)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerConfig.optionBoolEnabled("cursor_arrow_attack_target") && !ControllableUnit.isControllingUnit())
		{
			Actor last_actor = UnitSelectionEffect.last_actor;
			if (!last_actor.isRekt() && !last_actor.isInMagnet() && last_actor.has_attack_target && !last_actor.attack_target.isRekt())
			{
				BaseSimObject attack_target = last_actor.attack_target;
				Vector3 pStart = Vector2.op_Implicit(last_actor.current_position);
				Vector3 pEnd = Vector2.op_Implicit(attack_target.current_position);
				drawArrowQuantumSprite(pAsset, pStart, pEnd, ref pAsset.color);
			}
		}
	}

	private static void drawWalls(QuantumSpriteAsset pAsset)
	{
		bool pTransparentBuildings = World.world.getSelectedPowerAsset()?.make_buildings_transparent ?? false;
		Material mat_world_object = LibraryMaterials.instance.mat_world_object;
		drawWallType(TopTileLibrary.wall_order, pAsset, pTransparentBuildings, mat_world_object);
		drawWallType(TopTileLibrary.wall_evil, pAsset, pTransparentBuildings, mat_world_object);
		drawWallType(TopTileLibrary.wall_ancient, pAsset, pTransparentBuildings, mat_world_object);
		drawWallType(TopTileLibrary.wall_wild, pAsset, pTransparentBuildings, mat_world_object);
		drawWallType(TopTileLibrary.wall_iron, pAsset, pTransparentBuildings, mat_world_object);
		drawWallType(TopTileLibrary.wall_green, pAsset, pTransparentBuildings, mat_world_object);
		drawWallType(TopTileLibrary.wall_light, pAsset, pTransparentBuildings, World.world.library_materials.mat_world_object_lit);
	}

	private static void drawWallLightBlobs(QuantumSpriteAsset pAsset)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (!World.world.era_manager.shouldShowLights())
		{
			return;
		}
		List<WorldTile> currentTiles = TopTileLibrary.wall_light.getCurrentTiles();
		if (currentTiles.Count == 0)
		{
			return;
		}
		for (int i = 0; i < currentTiles.Count; i++)
		{
			WorldTile worldTile = currentTiles[i];
			if (worldTile.zone.visible)
			{
				World.world.stack_effects.light_blobs.Add(new LightBlobData
				{
					position = Vector2.op_Implicit(worldTile.posV3),
					radius = 0.3f
				});
			}
		}
	}

	private static void drawLavaLightBlobs(QuantumSpriteAsset pAsset)
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (!World.world.era_manager.shouldShowLights())
		{
			return;
		}
		List<TileZone> visibleZones = World.world.zone_camera.getVisibleZones();
		for (int i = 0; i < visibleZones.Count; i++)
		{
			TileZone tileZone = visibleZones[i];
			if (!tileZone.hasLava())
			{
				continue;
			}
			if (tileZone.countLava() < 5)
			{
				foreach (WorldTile item in tileZone.loopLava())
				{
					World.world.stack_effects.light_blobs.Add(new LightBlobData
					{
						position = Vector2.op_Implicit(item.posV3),
						radius = 0.2f
					});
				}
			}
			else
			{
				World.world.stack_effects.light_blobs.Add(new LightBlobData
				{
					position = Vector2.op_Implicit(tileZone.centerTile.posV3),
					radius = 1f
				});
			}
		}
	}

	private static void drawWallType(TopTileType pTileTypeAsset, QuantumSpriteAsset pAsset, bool pTransparentBuildings, Material pMaterial)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		List<WorldTile> currentTiles = pTileTypeAsset.getCurrentTiles();
		if (currentTiles.Count == 0)
		{
			return;
		}
		float num = World.world.quality_changer.getTweenBuildingsValue() * 0.25f;
		float pScaleY = num;
		float num2 = 0.1f;
		for (int i = 0; i < currentTiles.Count; i++)
		{
			WorldTile worldTile = currentTiles[i];
			if (worldTile.zone.visible)
			{
				Sprite sprite = WallHelper.getSprite(worldTile, pTileTypeAsset);
				QuantumSprite next = pAsset.group_system.getNext();
				next.setSprite(sprite);
				Vector3 pPosition = worldTile.posV3;
				pPosition.z = Mathf.Repeat(pPosition.x * 0.0001f, num2);
				next.setPosOnly(ref pPosition);
				next.setScale(num, pScaleY);
				next.setSharedMat(pMaterial);
			}
		}
	}

	private static void drawUnitsAvatars(QuantumSpriteAsset pAsset)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Actor[] array = World.world.units.visible_units_avatars.array;
		int count = World.world.units.visible_units_avatars.count;
		for (int i = 0; i < count; i++)
		{
			Actor actor = array[i];
			if (!actor.asset.ignore_generic_render)
			{
				Transform transform = actor.avatar.transform;
				if (!actor.is_visible)
				{
					transform.position = Globals.POINT_IN_VOID;
					continue;
				}
				Vector3 eulerAngles = actor.updateRotation();
				Vector3 current_scale = actor.current_scale;
				Vector3 position = actor.updatePos();
				transform.position = position;
				transform.localScale = current_scale;
				transform.eulerAngles = eulerAngles;
			}
		}
	}

	private static void drawHealthbars(QuantumSpriteAsset pAsset)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		bool flag = SelectedUnit.isSet();
		bool flag2 = HotkeyLibrary.isHoldingAlt();
		if (!flag2 && !flag)
		{
			return;
		}
		if (flag2)
		{
			flag = false;
		}
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		if (Zones.getCurrentMapBorderMode().isNone())
		{
			return;
		}
		ref Color health_bar_background = ref ColorStyleLibrary.m.health_bar_background;
		ref Color health_bar_main_green = ref ColorStyleLibrary.m.health_bar_main_green;
		ref Color health_bar_main_red = ref ColorStyleLibrary.m.health_bar_main_red;
		float num2 = getCameraScaleZoomMultiplier(pAsset) * 1.6f;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (flag && !SelectedUnit.isSelected(actor))
			{
				continue;
			}
			float healthRatio = actor.getHealthRatio();
			if (!(healthRatio >= 1f))
			{
				float num3 = 0.1f;
				float num4 = 9f * num3 * num2;
				float num5 = 1.5f * num3 * num2;
				Vector3 pPos = new Vector3
				{
					x = actor.cur_transform_position.x - num4 / 2f,
					y = actor.cur_transform_position.y + 13f * num3
				};
				if (healthRatio < 1f)
				{
					QuantumSprite quantumSprite = drawQuantumSprite(pAsset, pPos);
					quantumSprite.setSprite(_sprite_pixel);
					((Component)quantumSprite).transform.localScale = new Vector3(num4, num5);
					quantumSprite.setColor(ref health_bar_background);
				}
				ref Color color = ref health_bar_main_green;
				if (actor.getHealthRatio() < 0.4f)
				{
					color = ref health_bar_main_red;
				}
				pPos.z += 0.01f;
				QuantumSprite quantumSprite2 = drawQuantumSprite(pAsset, pPos);
				quantumSprite2.setSprite(_sprite_pixel);
				((Component)quantumSprite2).transform.localScale = new Vector3(num4 * healthRatio, num5);
				quantumSprite2.setColor(ref color);
			}
		}
	}

	private static void drawUnits(QuantumSpriteAsset pAsset)
	{
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		ActorRenderData render_data = World.world.units.render_data;
		int num = visible_units_count;
		if (num == 0)
		{
			return;
		}
		bool[] has_normal_render = render_data.has_normal_render;
		Sprite[] main_sprite_colored = render_data.main_sprite_colored;
		Vector3[] positions = render_data.positions;
		Vector3[] scales = render_data.scales;
		Vector3[] rotations = render_data.rotations;
		Color[] colors = render_data.colors;
		if (_q_render_indexes_units.Length < num)
		{
			_q_render_indexes_units = Toolbox.checkArraySize(_q_render_indexes_units, num);
		}
		int[] q_render_indexes_units = _q_render_indexes_units;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (has_normal_render[i])
			{
				q_render_indexes_units[num2++] = i;
			}
		}
		if (num2 == 0)
		{
			return;
		}
		QuantumSprite[] fastActiveList = pAsset.group_system.getFastActiveList(num2);
		QuantumSpriteCacheData cacheData = pAsset.group_system.getCacheData(num2);
		Sprite[] sprites = cacheData.sprites;
		Vector3[] positions2 = cacheData.positions;
		Vector3[] scales2 = cacheData.scales;
		Vector3[] rotations2 = cacheData.rotations;
		Color[] colors2 = cacheData.colors;
		for (int j = 0; j < num2; j++)
		{
			int num3 = q_render_indexes_units[j];
			Sprite val = main_sprite_colored[num3];
			if (sprites[j] != val)
			{
				sprites[j] = val;
				fastActiveList[j].sprite_renderer.sprite = val;
			}
		}
		for (int k = 0; k < num2; k++)
		{
			int num4 = q_render_indexes_units[k];
			Transform transform = fastActiveList[k].m_transform;
			ref Vector3 reference = ref positions[num4];
			ref Vector3 reference2 = ref positions2[k];
			if (reference.x != reference2.x || reference.y != reference2.y || reference.z != reference2.z)
			{
				reference2 = reference;
				transform.position = reference;
			}
			ref Vector3 reference3 = ref scales[num4];
			ref Vector3 reference4 = ref scales2[k];
			if (reference3.x != reference4.x || reference3.y != reference4.y || reference3.z != reference4.z)
			{
				reference4 = reference3;
				transform.localScale = reference3;
			}
			ref Vector3 reference5 = ref rotations[num4];
			ref Vector3 reference6 = ref rotations2[k];
			if (reference5.x != reference6.x || reference5.y != reference6.y || reference5.z != reference6.z)
			{
				reference6 = reference5;
				transform.eulerAngles = reference5;
			}
			ref Color reference7 = ref colors[num4];
			ref Color reference8 = ref colors2[k];
			if (reference7.r != reference8.r || reference7.g != reference8.g || reference7.b != reference8.b)
			{
				reference8 = reference7;
				fastActiveList[k].sprite_renderer.color = reference7;
			}
		}
	}

	private static void drawUnitItems(QuantumSpriteAsset pAsset)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		ActorRenderData render_data = World.world.units.render_data;
		int num = visible_units_count;
		if (num == 0)
		{
			return;
		}
		bool[] has_item = render_data.has_item;
		Vector3[] item_scale = render_data.item_scale;
		Vector3[] item_pos = render_data.item_pos;
		Vector3[] rotations = render_data.rotations;
		Sprite[] item_sprites = render_data.item_sprites;
		if (_q_render_indexes_unit_items.Length < num)
		{
			_q_render_indexes_unit_items = Toolbox.checkArraySize(_q_render_indexes_unit_items, num);
		}
		int[] q_render_indexes_unit_items = _q_render_indexes_unit_items;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (has_item[i])
			{
				q_render_indexes_unit_items[num2++] = i;
			}
		}
		if (num2 == 0)
		{
			return;
		}
		QuantumSpriteCacheData cacheData = pAsset.group_system.getCacheData(num2);
		Vector3[] scales = cacheData.scales;
		Vector3[] positions = cacheData.positions;
		Vector3[] rotations2 = cacheData.rotations;
		Sprite[] sprites = cacheData.sprites;
		QuantumSprite[] fastActiveList = pAsset.group_system.getFastActiveList(num2);
		for (int j = 0; j < num2; j++)
		{
			int num3 = q_render_indexes_unit_items[j];
			ref Vector3 reference = ref item_scale[num3];
			ref Vector3 reference2 = ref scales[j];
			if (reference.x != reference2.x || reference.y != reference2.y || reference.z != reference2.z)
			{
				reference2 = reference;
				fastActiveList[j].m_transform.localScale = reference;
			}
			ref Vector3 reference3 = ref item_pos[num3];
			ref Vector3 reference4 = ref positions[j];
			if (reference3.x != reference4.x || reference3.y != reference4.y || reference3.z != reference4.z)
			{
				reference4 = reference3;
				fastActiveList[j].m_transform.position = reference3;
			}
			ref Vector3 reference5 = ref rotations[num3];
			ref Vector3 reference6 = ref rotations2[j];
			if (reference5.x != reference6.x || reference5.y != reference6.y || reference5.z != reference6.z)
			{
				reference6 = reference5;
				fastActiveList[j].m_transform.eulerAngles = reference5;
			}
		}
		for (int k = 0; k < num2; k++)
		{
			int num4 = q_render_indexes_unit_items[k];
			Sprite val = item_sprites[num4];
			if (sprites[k] != val)
			{
				sprites[k] = val;
				fastActiveList[k].sprite_renderer.sprite = val;
			}
		}
	}

	private static void drawFires(QuantumSpriteAsset pAsset)
	{
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		if (!WorldBehaviourActionFire.hasFires())
		{
			return;
		}
		int num = 0;
		if (_q_render_indexes_sprites_fire.Length < World.world.tile_manager.tiles_count)
		{
			_q_render_indexes_sprites_fire = new int[World.world.tile_manager.tiles_count];
		}
		int[] q_render_indexes_sprites_fire = _q_render_indexes_sprites_fire;
		float animationGlobalTime = AnimationHelper.getAnimationGlobalTime(10f);
		Sprite[][] fire_sprites_sets = _fire_sprites_sets;
		int[] fires = WorldBehaviourActionFire.getFires();
		int[] random_seeds = World.world.tile_manager.random_seeds;
		int[] fire_animation_set = World.world.tile_manager.fire_animation_set;
		List<TileZone> visibleZones = World.world.zone_camera.getVisibleZones();
		Vector3[] positions_vector = World.world.tile_manager.positions_vector3;
		bool[] fires2 = World.world.tile_manager.fires;
		for (int i = 0; i < visibleZones.Count; i++)
		{
			TileZone tileZone = visibleZones[i];
			if (fires[tileZone.id] == 0)
			{
				continue;
			}
			WorldTile[] tiles = tileZone.tiles;
			int num2 = tiles.Length;
			for (int j = 0; j < num2; j++)
			{
				int tile_id = tiles[j].tile_id;
				if (fires2[tile_id])
				{
					q_render_indexes_sprites_fire[num++] = tile_id;
				}
			}
		}
		QuantumSprite[] fastActiveList = pAsset.group_system.getFastActiveList(num);
		QuantumSpriteCacheData cacheData = pAsset.group_system.getCacheData(num);
		Vector3[] positions = cacheData.positions;
		int[] indexes = cacheData.indexes;
		int[] indexes_ = cacheData.indexes_2;
		for (int k = 0; k < num; k++)
		{
			int num3 = q_render_indexes_sprites_fire[k];
			int num4 = fire_animation_set[num3];
			Sprite[] array = fire_sprites_sets[num4];
			Vector3 val = positions_vector[num3];
			ref Vector3 reference = ref positions[k];
			if (val.x != reference.x || val.y != reference.y || val.z != reference.z)
			{
				reference = val;
				fastActiveList[k].m_transform.position = val;
			}
			int num5 = (int)(animationGlobalTime + (float)(random_seeds[num3] * 100)) % array.Length;
			if (indexes[k] != num5 || indexes_[k] != num4)
			{
				indexes[k] = num5;
				indexes_[k] = num4;
				Sprite sprite = array[num5];
				fastActiveList[k].sprite_renderer.sprite = sprite;
			}
		}
	}

	private static void drawSocialize(QuantumSpriteAsset pAsset)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("talk_bubbles"))
		{
			return;
		}
		float num = 1f;
		double curSessionTime = World.world.getCurSessionTime();
		Actor[] array = World.world.units.visible_units_socialize.array;
		int count = World.world.units.visible_units_socialize.count;
		count = Math.Min(count, 1000);
		Vector2 pPosition = default(Vector2);
		for (int i = 0; i < count; i++)
		{
			Actor actor = array[i];
			if (!actor.hasTrait("mute"))
			{
				CommunicationAsset normal = CommunicationLibrary.normal;
				float num2 = (float)(curSessionTime - actor.timestamp_tween_session_social);
				if (num2 > num)
				{
					num2 = 1f;
				}
				Vector3 headOffsetPositionForFunRendering = actor.getHeadOffsetPositionForFunRendering();
				float num3 = iTween.easeOutCubic(0f, 1f, num2);
				float num4 = Randy.randomFloat(-0.03f, 0.03f);
				float num5 = Randy.randomFloat(-0.03f, 0.03f);
				Vector2 val = Vector2.op_Implicit(actor.current_scale);
				float num6 = headOffsetPositionForFunRendering.x + num4 * val.x;
				float num7 = headOffsetPositionForFunRendering.y + num5 * val.y;
				((Vector2)(ref pPosition))._002Ector(num6, num7);
				val.y *= num3;
				QuantumSprite next = pAsset.group_system.getNext();
				next.set(ref pPosition, val.y);
				Sprite spriteBubble = normal.getSpriteBubble();
				next.setSprite(spriteBubble);
				if (normal.show_topic)
				{
					Vector3 pPosition2 = Vector2.op_Implicit(pPosition);
					pPosition2.x += -1.65f * actor.current_scale.x;
					pPosition2.y += 10.04f * actor.current_scale.y;
					pPosition2.z = pPosition.y + 3f * actor.current_scale.y;
					QuantumSprite next2 = pAsset.group_system.getNext();
					next2.set(ref pPosition2, val.y * 0.35f);
					Sprite socializeTopic = actor.getSocializeTopic();
					next2.setSprite(socializeTopic);
				}
			}
		}
	}

	private static void drawJustAte(QuantumSpriteAsset pAsset)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		double curSessionTime = World.world.getCurSessionTime();
		Actor[] array = World.world.units.visible_units_just_ate.array;
		int count = World.world.units.visible_units_just_ate.count;
		Color pColor = default(Color);
		for (int i = 0; i < count; i++)
		{
			Actor actor = array[i];
			float num2 = (float)(curSessionTime - actor.timestamp_session_ate_food);
			if (num2 > num)
			{
				actor.timestamp_session_ate_food = 0.0;
				continue;
			}
			float num3 = num2 / num;
			float num4 = iTween.easeOutCubic(0f, 1f, num3);
			Vector3 pPos = Vector2.op_Implicit(actor.current_position);
			pPos.y += 1f + num4 * 2f;
			float num5 = num4;
			if (num5 > 0.5f)
			{
				num5 = 0.5f;
			}
			QuantumSprite quantumSprite = drawQuantumSprite(pAsset, pPos, null, null, null, null, num5);
			ResourceAsset resourceAsset = AssetManager.resources.get(actor.ate_last_item_id);
			quantumSprite.setSprite(resourceAsset.getSpriteIcon());
			((Component)quantumSprite).transform.eulerAngles = new Vector3(0f, 0f, num4 * 360f);
			float num6 = 1f;
			if ((double)num3 > 0.6)
			{
				num6 = (1f - num3) / 0.4f;
			}
			((Color)(ref pColor))._002Ector(num6, num6, num6, num6);
			quantumSprite.setColor(ref pColor);
		}
	}

	private static void drawCapturingZones(QuantumSpriteAsset pAsset)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		if (!Zones.showKingdomZones() && !Zones.showCityZones() && !Zones.showAllianceZones())
		{
			return;
		}
		using ListPool<TileZone> listPool = new ListPool<TileZone>();
		foreach (City city in World.world.cities)
		{
			if (!city.being_captured_by.isRekt() && city.hasZones())
			{
				float num = (float)city.last_visual_capture_ticks / 100f * (float)city.zones.Count;
				if (num > (float)city.zones.Count)
				{
					num = city.zones.Count;
				}
				CapturingZonesCalculator.getListToDraw(city, (int)num, listPool);
				for (int i = 0; i < listPool.Count; i++)
				{
					TileZone tileZone = listPool[i];
					QuantumSprite quantumSprite = drawQuantumSprite(pAsset, tileZone.centerTile, null);
					Color pColor = city.being_captured_by.getColor().getColorBorderOut_capture();
					quantumSprite.setColor(ref pColor);
				}
			}
		}
	}

	private static void drawUnityLine(QuantumSpriteAsset pAsset)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (!InputHelpers.mouseSupported || World.world.isBusyWithUI() || !World.world.isSelectedPower("unity"))
		{
			return;
		}
		Kingdom unity_A = Config.unity_A;
		if (unity_A == null)
		{
			return;
		}
		Vector2 mousePos = World.world.getMousePos();
		foreach (City city in unity_A.getCities())
		{
			Color pColor = unity_A.getColor().getColorMainSecond();
			drawArrowQuantumSprite(pAsset, city.getTile().posV, Vector2.op_Implicit(mousePos), ref pColor);
		}
	}

	private static void drawWhisperOfWarLine(QuantumSpriteAsset pAsset)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (!InputHelpers.mouseSupported || World.world.isBusyWithUI() || !World.world.isSelectedPower("whisper_of_war"))
		{
			return;
		}
		Kingdom whisper_A = Config.whisper_A;
		if (whisper_A == null)
		{
			return;
		}
		Vector2 mousePos = World.world.getMousePos();
		foreach (City city in whisper_A.getCities())
		{
			Color pColor = whisper_A.getColor().getColorMainSecond();
			drawArrowQuantumSprite(pAsset, city.getTile().posV, Vector2.op_Implicit(mousePos), ref pColor);
		}
	}

	private static void drawWhisperOfWar(QuantumSpriteAsset pAsset)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (World.world.isBusyWithUI() || !World.world.isSelectedPower("whisper_of_war"))
		{
			return;
		}
		City city = World.world.getMouseTilePosCachedFrame()?.zone.city;
		Kingdom kingdom = null;
		if (Config.whisper_A == null)
		{
			if (city == null)
			{
				return;
			}
			kingdom = city.kingdom;
		}
		else
		{
			kingdom = Config.whisper_A;
		}
		foreach (City city2 in kingdom.getCities())
		{
			colorZones(pAsset, city2.zones, pAsset.color);
		}
		colorEnemies(pAsset, kingdom);
	}

	private static void drawSelectedKingdomZones(QuantumSpriteAsset pAsset)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!World.world.isSelectedPower("relations") || SelectedMetas.selected_kingdom == null)
		{
			return;
		}
		foreach (City city in SelectedMetas.selected_kingdom.getCities())
		{
			colorZones(pAsset, city.zones, pAsset.color);
		}
		colorEnemies(pAsset, SelectedMetas.selected_kingdom);
	}

	private static void drawCursorZones(QuantumSpriteAsset pAsset)
	{
		if (!World.world.isBusyWithUI() && InputHelpers.mouseSupported && Zones.showMapBorders())
		{
			WorldTile mouseTilePosCachedFrame = World.world.getMouseTilePosCachedFrame();
			if (mouseTilePosCachedFrame != null)
			{
				MetaTypeAsset cachedMapMetaAsset = World.world.getCachedMapMetaAsset();
				cachedMapMetaAsset?.check_cursor_highlight(cachedMapMetaAsset, mouseTilePosCachedFrame, pAsset);
			}
		}
	}

	public static void colorEnemies(QuantumSpriteAsset pAsset, Kingdom pKingdom)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (!kingdom.isEnemy(pKingdom))
			{
				continue;
			}
			foreach (City city in kingdom.getCities())
			{
				Color color_ = pAsset.color_2;
				color_.a = 0.1f + QuantumSpriteManager.highlight_animation / 30f;
				colorZones(pAsset, city.zones, color_);
			}
		}
	}

	public static void colorZones(QuantumSpriteAsset pAsset, List<TileZone> pZones, Color pColor)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < pZones.Count; i++)
		{
			TileZone tileZone = pZones[i];
			if (tileZone.visible)
			{
				drawQuantumSprite(pAsset, tileZone.centerTile.posV).setColor(ref pColor);
			}
		}
	}

	public static void colorZones(QuantumSpriteAsset pAsset, ListPool<TileZone> pZones, Color pColor)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < pZones.Count; i++)
		{
			TileZone tileZone = pZones[i];
			if (tileZone.visible)
			{
				drawQuantumSprite(pAsset, tileZone.centerTile.posV).setColor(ref pColor);
			}
		}
	}

	private static void drawArrowsArmyAttackTargets(QuantumSpriteAsset pAsset)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("marks_armies") || !PlayerConfig.optionBoolEnabled("army_targets"))
		{
			return;
		}
		WorldTile mouseTilePosCachedFrame = World.world.getMouseTilePosCachedFrame();
		City city = null;
		if (mouseTilePosCachedFrame != null && DebugConfig.isOn(DebugOption.ArrowsOnlyForCursorCities))
		{
			city = mouseTilePosCachedFrame.zone.city;
		}
		foreach (City city2 in World.world.cities)
		{
			if (city2.target_attack_city != null && (!Zones.showCityZones() || city == null || city2 == city) && city2.hasArmy() && city2.army.hasCaptain())
			{
				Actor captain = city2.army.getCaptain();
				WorldTile current_tile = captain.current_tile;
				WorldTile beh_tile_target = captain.beh_tile_target;
				if (current_tile != null && beh_tile_target != null)
				{
					Color pColor = city2.kingdom.getColor().getColorMainSecond();
					drawArrowQuantumSprite(pAsset, current_tile.posV3, beh_tile_target.posV3, ref pColor, city2);
				}
			}
		}
	}

	private static void drawWarsIcons(QuantumSpriteAsset pAsset)
	{
		if (PlayerConfig.optionBoolEnabled("marks_wars"))
		{
			drawWarIconInList(_wars_pos_sword_main, "ui/Icons/iconAttack", pAsset, 0.2f);
			drawWarIconInList(_wars_pos_shields_main, "ui/Icons/iconShield", pAsset, 0.2f);
		}
	}

	private static void drawWarIconInList(List<Vector3> pList, string pPath, QuantumSpriteAsset pAsset, float pSize)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (pList.Count == 0)
		{
			return;
		}
		foreach (Vector3 p in pList)
		{
			float base_scale = pSize * p.z * 1.5f;
			pAsset.base_scale = base_scale;
			QuantumSprite quantumSprite = drawQuantumSprite(pAsset, p);
			quantumSprite.setSprite(SpriteTextureLoader.getSprite(pPath));
			((Renderer)quantumSprite.sprite_renderer).sortingOrder = 1;
		}
	}

	private static void drawProjectileShadows(QuantumSpriteAsset pAsset)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (!Config.shadows_active)
		{
			return;
		}
		foreach (Projectile item in World.world.projectiles.list)
		{
			ProjectileAsset asset = item.asset;
			if (!string.IsNullOrEmpty(asset.texture_shadow))
			{
				Vector3 pPosition = Vector2.op_Implicit(item.getCurrentPosition());
				float angleForShadow = item.getAngleForShadow();
				QuantumSprite next = pAsset.group_system.getNext();
				Sprite sprite = SpriteTextureLoader.getSprite(asset.texture_shadow);
				next.setSprite(sprite);
				next.set(ref pPosition, item.getCurrentScale());
				((Component)next).transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angleForShadow));
			}
		}
	}

	private static void drawProjectiles(QuantumSpriteAsset pAsset)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		Color pColor = default(Color);
		foreach (Projectile item in World.world.projectiles.list)
		{
			ProjectileAsset asset = item.asset;
			((Color)(ref pColor))._002Ector(1f, 1f, 1f, item.getAlpha());
			Vector3 pPosition = Vector2.op_Implicit(item.getTransformedPositionWithHeight());
			pPosition.z = item.getCurrentHeight();
			QuantumSprite next = pAsset.group_system.getNext();
			if (asset.animated)
			{
				Sprite spriteFromList = AnimationHelper.getSpriteFromList(item.GetHashCode(), asset.frames, asset.animation_speed);
				next.setSprite(spriteFromList);
			}
			else
			{
				Sprite sprite = asset.frames[0];
				next.setSprite(sprite);
			}
			next.set(ref pPosition, item.getCurrentScale());
			((Component)next).transform.rotation = item.rotation;
			next.setColor(ref pColor);
		}
	}

	private static void drawThrowingItemsShadows(QuantumSpriteAsset pAsset)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if (Config.shadows_active)
		{
			List<ResourceThrowData> list = World.world.resource_throw_manager.getList();
			QuantumSprite[] fastActiveList = pAsset.group_system.getFastActiveList(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				ResourceThrowData resourceThrowData = list[i];
				QuantumSprite obj = fastActiveList[i];
				float ratio = resourceThrowData.getRatio();
				Vector3 pPosition = Vector2.op_Implicit(Vector2.Lerp(resourceThrowData.position_start, resourceThrowData.position_end, ratio));
				pPosition.z = 4f;
				float pScale = 0.1f;
				Sprite gameplaySprite = AssetManager.resources.get(resourceThrowData.resource_asset_id).getGameplaySprite();
				obj.setSprite(gameplaySprite);
				obj.set(ref pPosition, pScale);
				((Component)obj).transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, ratio * 360f));
			}
		}
	}

	private static void drawShadowsBuildings(QuantumSpriteAsset pAsset)
	{
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		if (!World.world.quality_changer.shouldRenderBuildingShadows())
		{
			return;
		}
		int num = World.world.buildings.countVisibleBuildings();
		if (num == 0)
		{
			return;
		}
		BuildingRenderData render_data = World.world.buildings.render_data;
		bool[] shadows = render_data.shadows;
		Vector3[] positions = render_data.positions;
		Vector3[] scales = render_data.scales;
		Sprite[] shadow_sprites = render_data.shadow_sprites;
		if (_q_render_indexes_shadows_buildings.Length < num)
		{
			_q_render_indexes_shadows_buildings = Toolbox.checkArraySize(_q_render_indexes_shadows_buildings, num);
		}
		int[] q_render_indexes_shadows_buildings = _q_render_indexes_shadows_buildings;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (shadows[i])
			{
				q_render_indexes_shadows_buildings[num2++] = i;
			}
		}
		if (num2 == 0)
		{
			return;
		}
		QuantumSpriteCacheData cacheData = pAsset.group_system.getCacheData(num2);
		Vector3[] positions2 = cacheData.positions;
		Vector3[] scales2 = cacheData.scales;
		Sprite[] sprites = cacheData.sprites;
		QuantumSprite[] fastActiveList = pAsset.group_system.getFastActiveList(num2);
		for (int j = 0; j < num2; j++)
		{
			QuantumSprite quantumSprite = fastActiveList[j];
			int num3 = q_render_indexes_shadows_buildings[j];
			ref Vector3 reference = ref positions[num3];
			ref Vector3 reference2 = ref positions2[j];
			if (reference.x != reference2.x || reference.y != reference2.y || reference.z != reference2.z)
			{
				reference2 = reference;
				quantumSprite.m_transform.position = reference;
			}
			ref Vector3 reference3 = ref scales[num3];
			ref Vector3 reference4 = ref scales2[j];
			if (reference3.x != reference4.x || reference3.y != reference4.y || reference3.z != reference4.z)
			{
				reference4 = reference3;
				quantumSprite.m_transform.localScale = reference3;
			}
			Sprite val = shadow_sprites[num3];
			if (sprites[j] != val)
			{
				sprites[j] = val;
				quantumSprite.sprite_renderer.sprite = val;
			}
		}
	}

	private static void drawShadowsUnit(QuantumSpriteAsset pAsset)
	{
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		if (!World.world.quality_changer.shouldRenderUnitShadows())
		{
			return;
		}
		ActorRenderData render_data = World.world.units.render_data;
		int num = visible_units_count;
		if (num == 0)
		{
			return;
		}
		bool[] shadows = render_data.shadows;
		Vector3[] shadow_position = render_data.shadow_position;
		Vector3[] shadow_scales = render_data.shadow_scales;
		Sprite[] shadow_sprites = render_data.shadow_sprites;
		if (_q_render_indexes_shadows_units.Length < num)
		{
			_q_render_indexes_shadows_units = Toolbox.checkArraySize(_q_render_indexes_shadows_units, num);
		}
		int[] q_render_indexes_shadows_units = _q_render_indexes_shadows_units;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (shadows[i])
			{
				q_render_indexes_shadows_units[num2++] = i;
			}
		}
		if (num2 == 0)
		{
			return;
		}
		QuantumSprite[] fastActiveList = pAsset.group_system.getFastActiveList(num2);
		QuantumSpriteCacheData cacheData = pAsset.group_system.getCacheData(num2);
		Vector3[] positions = cacheData.positions;
		Vector3[] shadow_scales2 = cacheData.shadow_scales;
		Sprite[] sprites = cacheData.sprites;
		for (int j = 0; j < num2; j++)
		{
			int num3 = q_render_indexes_shadows_units[j];
			ref Vector3 reference = ref shadow_position[num3];
			ref Vector3 reference2 = ref positions[j];
			if (reference.x != reference2.x || reference.y != reference2.y || reference.z != reference2.z)
			{
				reference2 = reference;
				fastActiveList[j].m_transform.position = reference;
			}
			ref Vector3 reference3 = ref shadow_scales[num3];
			ref Vector3 reference4 = ref shadow_scales2[j];
			if (reference3.x != reference4.x || reference3.y != reference4.y || reference3.z != reference4.z)
			{
				reference4 = reference3;
				fastActiveList[j].m_transform.localScale = reference3;
			}
			Sprite val = shadow_sprites[num3];
			if (sprites[j] != val)
			{
				sprites[j] = val;
				fastActiveList[j].sprite_renderer.sprite = val;
			}
		}
	}

	private static void drawUnitBanners(QuantumSpriteAsset pAsset)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Actor[] array = World.world.units.visible_units_with_banner.array;
		int count = World.world.units.visible_units_with_banner.count;
		for (int i = 0; i < count; i++)
		{
			Actor actor = array[i];
			Vector3 headOffsetPositionForFunRendering = actor.getHeadOffsetPositionForFunRendering();
			QuantumSprite quantumSprite = drawQuantumSprite(pAsset, headOffsetPositionForFunRendering, null, null, null, null, 1f, pSetColor: false, actor.current_scale.y);
			Color pColor = actor.kingdom.getColor().getColorText();
			quantumSprite.setColor(ref pColor);
			quantumSprite.checkRotation(headOffsetPositionForFunRendering, actor, -0.01f);
		}
	}

	private static void drawFavoriteItemsMap(QuantumSpriteAsset pAsset)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("marks_favorite_items"))
		{
			return;
		}
		foreach (Item item in World.world.items)
		{
			if (item.isFavorite())
			{
				Actor actor = item.getActor();
				if (!actor.isRekt() && actor.current_zone.visible)
				{
					Vector3 pPos = Vector2.op_Implicit(actor.current_position);
					pPos.y++;
					QuantumSprite quantumSprite = drawQuantumSprite(pAsset, pPos, null, actor.kingdom, actor.city);
					Sprite sprite = item.getSprite();
					quantumSprite.setSprite(sprite);
				}
			}
		}
	}

	private static void drawFavoritesMap(QuantumSpriteAsset pAsset)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerConfig.optionBoolEnabled("marks_favorites"))
		{
			Actor[] array = World.world.units.visible_units_with_favorite.array;
			int count = World.world.units.visible_units_with_favorite.count;
			for (int i = 0; i < count; i++)
			{
				Actor actor = array[i];
				Vector3 pPos = Vector2.op_Implicit(actor.current_position);
				pPos.y -= 3f;
				drawQuantumSprite(pAsset, pPos, null, actor.kingdom, actor.city);
			}
		}
	}

	private static void drawUnitsToBeSelectedBySquareTool(QuantumSpriteAsset pAsset)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!World.world.player_control.square_selection_started)
		{
			return;
		}
		using ListPool<Actor> listPool = World.world.player_control.getUnitsToBeSelected();
		if (listPool == null || listPool.Count == 0)
		{
			return;
		}
		Sprite spriteFromListSessionTime = AnimationHelper.getSpriteFromListSessionTime(0, _unit_selection_effect, 10f);
		Color pColor = World.world.getArchitectColor();
		pColor.a = 0.7f;
		foreach (ref Actor item in listPool)
		{
			Actor current = item;
			Vector3 pPos = Vector2.op_Implicit(current.current_position);
			float y = current.current_scale.y;
			QuantumSprite quantumSprite = drawQuantumSprite(pAsset, pPos, null, null, null, null, 1f, pSetColor: false, y);
			quantumSprite.setSprite(spriteFromListSessionTime);
			quantumSprite.setColor(ref pColor);
		}
	}

	private static void drawSelectedUnits(QuantumSpriteAsset pAsset)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!SelectedUnit.isSet())
		{
			return;
		}
		Sprite spriteFromListSessionTime = AnimationHelper.getSpriteFromListSessionTime(0, _unit_selection_effect, 10f);
		Sprite spriteFromListSessionTime2 = AnimationHelper.getSpriteFromListSessionTime(0, _unit_selection_effect_main, 10f);
		Color pColor = World.world.getArchitectColor();
		pColor.a = 0.8f;
		Color pColor2 = World.world.getArchitectColor();
		foreach (Actor item in SelectedUnit.getAllSelected())
		{
			Vector3 pPos = Vector2.op_Implicit(item.current_position);
			float y = item.current_scale.y;
			if (SelectedUnit.isMainSelected(item))
			{
				QuantumSprite quantumSprite = drawQuantumSprite(pAsset, pPos, null, null, null, null, 1f, pSetColor: false, y * 1.1f);
				quantumSprite.setSprite(spriteFromListSessionTime2);
				quantumSprite.setColor(ref pColor2);
			}
			else
			{
				QuantumSprite quantumSprite2 = drawQuantumSprite(pAsset, pPos, null, null, null, null, 1f, pSetColor: false, y);
				quantumSprite2.setSprite(spriteFromListSessionTime);
				quantumSprite2.setColor(ref pColor);
			}
		}
	}

	private static void drawFavoritesGame(QuantumSpriteAsset pAsset)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("marks_favorites"))
		{
			return;
		}
		float num = 20f;
		if (PlayerConfig.optionBoolEnabled("icons_tasks"))
		{
			num += 11.5f;
		}
		if (PlayerConfig.optionBoolEnabled("icons_happiness"))
		{
			num += 11.5f;
		}
		Actor[] array = World.world.units.visible_units_with_favorite.array;
		int count = World.world.units.visible_units_with_favorite.count;
		Vector3 pPos = default(Vector3);
		for (int i = 0; i < count; i++)
		{
			Actor actor = array[i];
			if (!actor.isInMagnet())
			{
				actor.updatePos();
				float x = actor.cur_transform_position.x;
				float num2 = actor.cur_transform_position.y + num * actor.current_scale.y;
				((Vector3)(ref pPos))._002Ector(x, num2);
				drawQuantumSprite(pAsset, pPos, null, null, null, null, 1f, pSetColor: false, actor.current_scale.y);
			}
		}
	}

	private static void drawStatusEffects(QuantumSpriteAsset pAsset)
	{
		Actor[] array = World.world.units.visible_units_with_status.array;
		int count = World.world.units.visible_units_with_status.count;
		for (int i = 0; i < count; i++)
		{
			drawStatusEffectFor(array[i], pAsset);
		}
		int num = World.world.buildings.countVisibleBuildings();
		Building[] visibleBuildings = World.world.buildings.getVisibleBuildings();
		for (int j = 0; j < num; j++)
		{
			Building building = visibleBuildings[j];
			if (building.hasAnyStatusEffectToRender())
			{
				drawStatusEffectFor(building, pAsset);
			}
		}
	}

	private static void drawStatusEffectFor(BaseSimObject pSimObject, QuantumSpriteAsset pAsset)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		Vector3 pVec = default(Vector3);
		foreach (Status status in pSimObject.getStatuses())
		{
			StatusAsset asset = status.asset;
			if (!asset.need_visual_render)
			{
				continue;
			}
			Vector3 pPosition = pSimObject.cur_transform_position;
			if (pSimObject.isActor())
			{
				pPosition.x += asset.offset_x * pSimObject.a.getScaleMod();
				pPosition.y += asset.offset_y * pSimObject.a.getScaleMod();
			}
			if (asset.has_override_sprite_position)
			{
				Vector3 val = asset.get_override_sprite_position(pSimObject, status.anim_frame);
				pPosition += val;
			}
			if (pSimObject.isActor() && !status.asset.render_check(pSimObject.a.asset))
			{
				continue;
			}
			QuantumSprite next = pAsset.group_system.getNext();
			next.setScale(pSimObject.current_scale.y * asset.scale);
			Sprite sprite = ((!asset.has_override_sprite) ? asset.sprite_list[status.anim_frame] : asset.get_override_sprite(pSimObject, status.anim_frame));
			next.setSprite(sprite);
			next.setPosOnly(ref pPosition);
			if (asset.use_parent_rotation)
			{
				next.setFlipX(pFlipX: false);
				next.checkRotation(pPosition, pSimObject, asset.position_z);
			}
			else
			{
				if (pSimObject.isActor() && asset.can_be_flipped)
				{
					next.setFlipX(pSimObject.a.flip);
				}
				else
				{
					next.setFlipX(pFlipX: false);
				}
				((Vector3)(ref pVec))._002Ector(0f, 0f, 0f);
				next.setRotation(ref pVec);
			}
			if (asset.rotation_z != 0f)
			{
				Vector3 pVec2 = pSimObject.current_rotation;
				if (asset.has_override_sprite_rotation_z)
				{
					pVec2.z += asset.get_override_sprite_rotation_z(pSimObject, status.anim_frame);
				}
				else
				{
					pVec2.z += asset.rotation_z;
				}
				next.setRotation(ref pVec2);
			}
			next.setSharedMat(asset.material);
		}
	}

	private static void drawWars(QuantumSpriteAsset pAsset)
	{
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("marks_wars"))
		{
			return;
		}
		_wars_pos_sword_main.Clear();
		_wars_pos_shields_main.Clear();
		if (World.world.wars.Count == 0)
		{
			return;
		}
		Kingdom kingdom = null;
		foreach (Kingdom kingdom2 in World.world.kingdoms)
		{
			if (kingdom2.isCursorOver())
			{
				kingdom = kingdom2;
				break;
			}
		}
		float num = 1f;
		bool flag = false;
		foreach (War war in World.world.wars)
		{
			flag = false;
			if (war.hasEnded() || war.isTotalWar())
			{
				continue;
			}
			if (kingdom != null && war.hasKingdom(kingdom))
			{
				flag = true;
			}
			if (kingdom != null)
			{
				if (flag)
				{
					pAsset.base_scale = 1f;
					num = 1f;
				}
				else
				{
					pAsset.base_scale = 0.2f;
					num = 0.1f;
				}
			}
			else
			{
				pAsset.base_scale = 0.5f;
			}
			Kingdom main_attacker = war.main_attacker;
			Kingdom main_defender = war.main_defender;
			if (!main_attacker.isRekt() && !main_defender.isRekt() && main_attacker.hasCapital() && main_defender.hasCapital() && main_attacker.capital.isValidTargetForWar() && main_defender.capital.isValidTargetForWar())
			{
				Vector3 val = Vector2.op_Implicit(main_attacker.capital.city_center);
				Vector3 val2 = Vector2.op_Implicit(main_defender.capital.city_center);
				val.y -= 20f;
				val2.y -= 20f;
				val.z = pAsset.base_scale;
				val2.z = pAsset.base_scale;
				_wars_pos_sword_main.Add(val);
				_wars_pos_shields_main.Add(val2);
				pAsset.base_scale *= 0.6f;
				QuantumSpriteArrows quantumSpriteArrows = drawArrowQuantumSprite(pAsset, val, val2, ref Toolbox.color_white);
				Color colorMainSecond = main_attacker.getColor().getColorMainSecond();
				colorMainSecond.a = num;
				if ((Object)(object)quantumSpriteArrows != (Object)null)
				{
					quantumSpriteArrows.spriteArrowMiddle.color = colorMainSecond;
					((Renderer)quantumSpriteArrows.spriteArrowMiddle).sortingOrder = -1;
				}
			}
		}
	}

	private static void drawPlots(QuantumSpriteAsset pAsset)
	{
		if (!PlayerConfig.optionBoolEnabled("marks_plots"))
		{
			return;
		}
		foreach (Plot plot in World.world.plots)
		{
			if (plot.isActive())
			{
				drawPlotIcon(pAsset, plot);
			}
		}
	}

	private static void drawPlotIcon(QuantumSpriteAsset pAsset, Plot pPlot)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		foreach (Actor unit in pPlot.units)
		{
			if (!unit.isRekt() && unit.current_zone.visible)
			{
				Vector3 pPos = Vector2.op_Implicit(unit.current_position);
				City city = unit.city;
				float num = 5.5f;
				num *= getCameraScaleZoomMultiplier(pAsset);
				if (city != null)
				{
					num *= city.mark_scale_effect;
				}
				pPos.y += num;
				QuantumSprite quantumSprite = drawQuantumSprite(pAsset, pPos, null, null, city, null, pPlot.transition_animation);
				Sprite sprite = pPlot.getSprite();
				quantumSprite.setSprite(sprite);
				CircleIconShaderMod component = ((Component)quantumSprite).GetComponent<CircleIconShaderMod>();
				component.sprite_renderer_with_mat.sprite = sprite;
				component.setShaderVal(pPlot.getProgressMod());
			}
		}
	}

	private static void drawPlotRemovals(QuantumSpriteAsset pAsset)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("marks_plots"))
		{
			return;
		}
		List<PlotIconData> plot_removals = World.world.stack_effects.plot_removals;
		if (plot_removals.Count <= 0)
		{
			return;
		}
		for (int num = plot_removals.Count - 1; num >= 0; num--)
		{
			PlotIconData plotIconData = plot_removals[num];
			Actor actor = plotIconData.actor;
			float realTimeElapsedSince = World.world.getRealTimeElapsedSince(plotIconData.timestamp);
			if (realTimeElapsedSince > 1f || !actor.isAlive())
			{
				plot_removals.RemoveAt(num);
			}
			else
			{
				Vector3 pPos = Vector2.op_Implicit(actor.current_position);
				City city = actor.city;
				float num2 = 5.5f;
				num2 *= getCameraScaleZoomMultiplier(pAsset);
				if (city != null)
				{
					num2 *= city.mark_scale_effect;
				}
				pPos.y += num2;
				float num3 = realTimeElapsedSince / 1f;
				float pModScale = Mathf.Lerp(1.3f, 0f, num3);
				QuantumSprite quantumSprite = drawQuantumSprite(pAsset, pPos, null, null, city, null, pModScale);
				Sprite sprite = SpriteTextureLoader.getSprite(plotIconData.sprite);
				quantumSprite.setSprite(sprite);
				CircleIconShaderMod component = ((Component)quantumSprite).GetComponent<CircleIconShaderMod>();
				component.sprite_renderer_with_mat.sprite = sprite;
				component.setShaderVal(1f);
			}
		}
	}

	private static void drawKings(QuantumSpriteAsset pAsset)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("map_kings_leaders"))
		{
			return;
		}
		int num = 0;
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (num > 2)
			{
				break;
			}
			Actor king = kingdom.king;
			if (!king.isRekt() && !king.isInMagnet() && king.current_zone.visible)
			{
				Vector3 pPos = Vector2.op_Implicit(king.current_position);
				pPos.y -= 3f;
				Sprite pSprite = (king.has_attack_target ? _king_sprite_angry : (king.hasPlot() ? _king_sprite_surprised : (kingdom.hasEnemies() ? _king_sprite_normal : _king_sprite_happy)));
				if (!pAsset.group_system.is_within_active_index)
				{
					num++;
				}
				QuantumSprite quantumSprite = drawQuantumSprite(pAsset, pPos, null, kingdom, king.city);
				Sprite icon = DynamicSprites.getIcon(pSprite, kingdom.getColor());
				quantumSprite.setSprite(icon);
			}
		}
	}

	private static void drawLeaders(QuantumSpriteAsset pAsset)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("map_kings_leaders"))
		{
			return;
		}
		int num = 0;
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (num > 2)
			{
				break;
			}
			foreach (City city in kingdom.getCities())
			{
				Actor leader = city.leader;
				if (!leader.isRekt() && !leader.isInMagnet() && !leader.isKing() && leader.current_zone.visible)
				{
					Vector3 pPos = Vector2.op_Implicit(leader.current_position);
					pPos.y -= 3f;
					Sprite pSprite = (leader.has_attack_target ? _leader_sprite_angry : (leader.hasPlot() ? _leader_sprite_surprised : (kingdom.hasEnemies() ? _leader_sprite_normal : ((!leader.isHappy()) ? _leader_sprite_sad : _leader_sprite_happy))));
					if (!pAsset.group_system.is_within_active_index)
					{
						num++;
					}
					QuantumSprite quantumSprite = drawQuantumSprite(pAsset, pPos, null, kingdom, city);
					Sprite icon = DynamicSprites.getIcon(pSprite, kingdom.getColor());
					quantumSprite.setSprite(icon);
				}
			}
		}
	}

	private static void drawBattles(QuantumSpriteAsset pAsset)
	{
		if (!PlayerConfig.optionBoolEnabled("marks_battles"))
		{
			return;
		}
		HashSet<BattleContainer> hashSet = BattleKeeperManager.get();
		if (hashSet.Count == 0)
		{
			return;
		}
		foreach (BattleContainer item in hashSet)
		{
			if (item.isRendered())
			{
				QuantumSprite quantumSprite = drawQuantumSprite(pAsset, item.tile, null, null, null, item);
				Sprite sprite = SpriteTextureLoader.getSpriteList(pAsset.path_icon)[item.frame];
				quantumSprite.setSprite(sprite);
			}
		}
	}

	private static void drawBoatIcons(QuantumSpriteAsset pAsset)
	{
		if (!PlayerConfig.optionBoolEnabled("marks_boats"))
		{
			return;
		}
		foreach (ActorAsset list_only_boat_asset in AssetManager.actor_library.list_only_boat_assets)
		{
			drawBoatIcons(pAsset, list_only_boat_asset.id);
		}
	}

	private static void drawBoatIcons(QuantumSpriteAsset pAsset, string pActorAssetID)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		HashSet<Actor> units = AssetManager.actor_library.get(pActorAssetID).units;
		if (units.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (Actor item in units)
		{
			if (num > 2)
			{
				break;
			}
			if (!item.isRekt() && item.current_zone.visible && item.asset.draw_boat_mark && item.isKingdomCiv() && (!(pAsset.id == "boats_big") || item.asset.draw_boat_mark_big) && (!(pAsset.id == "boats_small") || !item.asset.draw_boat_mark_big) && !item.isInMagnet())
			{
				ColorAsset color = item.kingdom.getColor();
				if (!pAsset.group_system.is_within_active_index)
				{
					num++;
				}
				QuantumSprite quantumSprite = ((color == null) ? drawQuantumSprite(pAsset, Vector2.op_Implicit(item.current_position), null, pCity: item.city, pKingdom: item.kingdom) : drawQuantumSprite(pAsset, Vector2.op_Implicit(item.current_position), null, pCity: item.city, pKingdom: item.kingdom));
				Sprite sprite = ((!item.asset.draw_boat_mark_big) ? DynamicSprites.getIcon(_boat_sprite_small, item.kingdom.getColor()) : DynamicSprites.getIcon(_boat_sprite_big, item.kingdom.getColor()));
				quantumSprite.setSprite(sprite);
			}
		}
	}

	private static void drawMagnetUnits(QuantumSpriteAsset pAsset)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (!World.world.magnet.hasUnits())
		{
			return;
		}
		List<Actor> magnet_units = World.world.magnet.magnet_units;
		for (int i = 0; i < magnet_units.Count; i++)
		{
			Actor actor = magnet_units[i];
			if (!actor.isRekt())
			{
				QuantumSprite quantumSprite = drawQuantumSprite(pAsset, Vector2.op_Implicit(actor.current_position), null, actor.kingdom);
				quantumSprite.setScale(actor.current_scale.y);
				((Component)quantumSprite).transform.rotation = Quaternion.Euler(0f, 0f, World.world.magnet.moving_angle);
				quantumSprite.setSprite(actor.getSpriteToRender());
			}
		}
	}

	private static void drawArmies(QuantumSpriteAsset pAsset)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerConfig.optionBoolEnabled("marks_armies"))
		{
			return;
		}
		int num = 0;
		if (Zones.showArmyZones() && Zones.showMapNames())
		{
			return;
		}
		for (int i = 0; i < World.world.armies.list.Count; i++)
		{
			if (num > 2)
			{
				break;
			}
			Army army = World.world.armies.list[i];
			if (!army.hasCaptain())
			{
				continue;
			}
			Actor captain = army.getCaptain();
			if (!captain.isInMagnet() && captain.current_zone.visible && captain.isKingdomCiv())
			{
				Kingdom kingdom = captain.kingdom;
				QuantumSpriteWithText quantumSpriteWithText = (QuantumSpriteWithText)drawQuantumSprite(pAsset, Vector2.op_Implicit(captain.current_position), null, kingdom, captain.city);
				if (DebugConfig.isOn(DebugOption.ShowAmountNearArmy))
				{
					((Component)quantumSpriteWithText.text).gameObject.SetActive(true);
					quantumSpriteWithText.text.text = army.countUnits().ToString() ?? "";
					((Component)quantumSpriteWithText.text).GetComponent<Renderer>().sortingLayerID = ((Renderer)quantumSpriteWithText.sprite_renderer).sortingLayerID;
					((Component)quantumSpriteWithText.text).GetComponent<Renderer>().sortingOrder = ((Renderer)quantumSpriteWithText.sprite_renderer).sortingOrder;
				}
				else
				{
					((Component)quantumSpriteWithText.text).gameObject.SetActive(false);
				}
				if (!pAsset.group_system.is_within_active_index)
				{
					num++;
				}
				Sprite icon = DynamicSprites.getIcon(_flag_sprite, kingdom.getColor());
				quantumSpriteWithText.setSprite(icon);
			}
		}
	}

	private static QuantumSpriteArrows drawArrowQuantumSprite(QuantumSpriteAsset pAsset, Vector3 pStart, Vector3 pEnd, ref Color pColor, City pCity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		if (pStart.x == pEnd.x && pStart.y == pEnd.y)
		{
			return null;
		}
		float num = Toolbox.Dist(pStart.x, pStart.y, pEnd.x, pEnd.y);
		float num2 = pAsset.base_scale * getCameraScaleZoomMultiplier(pAsset);
		if (pCity != null)
		{
			num2 *= pCity.mark_scale_effect;
		}
		num /= num2;
		if (num < (float)pAsset.line_width)
		{
			return null;
		}
		float num3 = QuantumSpriteManager.arrow_middle_current;
		if (!pAsset.arrow_animation)
		{
			num3 = 0f;
		}
		QuantumSpriteArrows quantumSpriteArrows = (QuantumSpriteArrows)pAsset.group_system.getNext();
		((Renderer)quantumSpriteArrows.spriteArrowEnd).enabled = pAsset.render_arrow_end;
		((Renderer)quantumSpriteArrows.spriteArrowStart).enabled = pAsset.render_arrow_start;
		if (num < (float)(pAsset.line_width + 2))
		{
			((Renderer)quantumSpriteArrows.spriteArrowEnd).enabled = false;
		}
		if (((Renderer)quantumSpriteArrows.spriteArrowEnd).enabled)
		{
			quantumSpriteArrows.spriteArrowEnd.color = pColor;
			((Component)quantumSpriteArrows.spriteArrowEnd).transform.localPosition = new Vector3(num, 0f, 0f);
		}
		if (((Renderer)quantumSpriteArrows.spriteArrowStart).enabled)
		{
			quantumSpriteArrows.spriteArrowStart.color = pColor;
		}
		quantumSpriteArrows.spriteArrowMiddle.color = pColor;
		Vector3 position = pStart;
		position.z = (float)pAsset.group_system.countActive() * 0.001f;
		((Component)quantumSpriteArrows).transform.position = position;
		float angleDegrees = Toolbox.getAngleDegrees(pStart.x, pStart.y, pEnd.x, pEnd.y);
		((Component)quantumSpriteArrows).transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angleDegrees));
		float num4 = num - num3;
		if (((Renderer)quantumSpriteArrows.spriteArrowEnd).enabled)
		{
			num4 -= 5f;
		}
		quantumSpriteArrows.spriteArrowMiddle.size = new Vector2(num4, (float)pAsset.line_height);
		((Component)quantumSpriteArrows.spriteArrowMiddle).transform.localPosition = new Vector3(num3, 0f, 0f);
		((Component)quantumSpriteArrows).transform.localScale = new Vector3(num2, num2, 1f);
		return quantumSpriteArrows;
	}

	private static QuantumSprite drawQuantumSprite(QuantumSpriteAsset pAsset, Vector3 pPos, WorldTile pTileTarget = null, Kingdom pKingdom = null, City pCity = null, BattleContainer pBattle = null, float pModScale = 1f, bool pSetColor = false, float pForceScaleTo = -1f)
	{
		QuantumSprite next = pAsset.group_system.getNext();
		if (pSetColor)
		{
			next.setColor(ref Toolbox.color_white);
		}
		float num;
		if (pForceScaleTo == -1f)
		{
			num = pAsset.base_scale * pModScale;
			if (pAsset.flag_battle)
			{
				num = num * pBattle.timer * 0.2f;
			}
			if (pAsset.add_camera_zoom_multiplier)
			{
				num *= getCameraScaleZoomMultiplier(pAsset);
			}
			if (pAsset.selected_city_scale)
			{
				num = ((pCity == null) ? (num * 0.5f) : (num * pCity.mark_scale_effect));
			}
		}
		else
		{
			num = pForceScaleTo;
		}
		next.set(ref pPos, num);
		return next;
	}

	private static QuantumSprite drawQuantumSprite(QuantumSpriteAsset pAsset, WorldTile pTile, WorldTile pTileTarget, Kingdom pKingdom = null, City pCity = null, BattleContainer pBattle = null)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (pTile == null)
		{
			return null;
		}
		return drawQuantumSprite(pAsset, pTile.posV3, pTileTarget, pKingdom, pCity, pBattle);
	}

	private static float getCameraScaleZoomMultiplier(QuantumSpriteAsset pAsset)
	{
		return Mathf.Clamp(MoveCamera.instance.main_camera.orthographicSize / 30f, (float)pAsset.add_camera_zoom_multiplier_min, (float)pAsset.add_camera_zoom_multiplier_max);
	}

	public void initDebugQuantumSpriteAssets()
	{
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0595: Unknown result type (might be due to invalid IL or missing references)
		//IL_059a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0602: Unknown result type (might be due to invalid IL or missing references)
		//IL_0607: Unknown result type (might be due to invalid IL or missing references)
		//IL_0672: Unknown result type (might be due to invalid IL or missing references)
		//IL_0677: Unknown result type (might be due to invalid IL or missing references)
		//IL_06df: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_074f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0754: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0950: Unknown result type (might be due to invalid IL or missing references)
		//IL_0955: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bbb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c45: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c4a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d66: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d6b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0de1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0de6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e77: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f80: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f85: Unknown result type (might be due to invalid IL or missing references)
		add(new QuantumSpriteAsset
		{
			id = "draw_money",
			id_prefab = "p_mapSprite",
			add_camera_zoom_multiplier = false,
			debug_option = DebugOption.ShowMoneyIcons,
			draw_call = drawMoney,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("EffectsTop");
				pQSprite.sprite_renderer.sprite = SpriteTextureLoader.getSprite("ui/Icons/iconResGold");
			},
			render_gameplay = true,
			default_amount = 10
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_arrows_settlers",
			id_prefab = "p_mapArrow_stroke",
			render_map = true,
			arrow_animation = true,
			draw_call = debugDrawArrowsSettlers,
			debug_option = DebugOption.CivDrawSettleTarget
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_arrows_land_claim",
			id_prefab = "p_mapArrow_stroke",
			render_map = true,
			arrow_animation = true,
			draw_call = debugDrawClaimZone,
			debug_option = DebugOption.CivDrawCityClaimZone
		});
		add(new QuantumSpriteAsset
		{
			base_scale = 0.35f,
			id = "debug_kingdom_attack_targets",
			id_prefab = "p_mapArrow_stroke",
			render_arrow_end = true,
			render_arrow_start = true,
			arrow_animation = true,
			render_map = true,
			draw_call = debugDrawArrowsKingdomAttackTarget,
			debug_option = DebugOption.KingdomDrawAttackTarget
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_unit_attack_range",
			id_prefab = "p_mapSprite",
			base_scale = 0.1f,
			draw_call = drawUnitAttackRange,
			debug_option = DebugOption.CursorUnitAttackRange,
			create_object = delegate(QuantumSpriteAsset pAsset, QuantumSprite pQSprite)
			{
				Sprite sprite = SpriteTextureLoader.getSprite("ui/Icons/iconWhiteCircle");
				pQSprite.setSprite(sprite);
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("EffectsBack");
				((Renderer)pQSprite.sprite_renderer).sortingOrder = 10;
				pQSprite.setColor(ref pAsset.color);
			},
			render_gameplay = true,
			color = new Color(1f, 1f, 1f, 0.3f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_unit_attack_size",
			id_prefab = "p_mapSprite",
			base_scale = 0.1f,
			draw_call = drawUnitSize,
			debug_option = DebugOption.CursorUnitSize,
			create_object = delegate(QuantumSpriteAsset pAsset, QuantumSprite pQSprite)
			{
				Sprite sprite = SpriteTextureLoader.getSprite("ui/Icons/iconWhiteCircle");
				pQSprite.setSprite(sprite);
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("EffectsBack");
				((Renderer)pQSprite.sprite_renderer).sortingOrder = 10;
				pQSprite.setColor(ref pAsset.color);
			},
			render_gameplay = true,
			color = new Color(0.2f, 0.2f, 1f, 0.4f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_arrows_units_attack_targets",
			id_prefab = "p_mapArrow_stroke",
			base_scale = 0.1f,
			draw_call = debugDrawArrowsUnitAttackTargets,
			debug_option = DebugOption.ArrowsUnitsAttackTargets,
			arrow_animation = true,
			render_gameplay = true,
			color = new Color(1f, 0f, 0f, 0.7f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_arrows_units_actor_targets",
			id_prefab = "p_mapArrow_stroke",
			base_scale = 0.1f,
			draw_call = debugDrawArrowsUnitBehTarget,
			debug_option = DebugOption.ArrowUnitsBehActorTarget,
			arrow_animation = true,
			render_gameplay = true,
			color = new Color(1f, 1f, 0f, 0.7f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_arrows_units_navigation_targets",
			id_prefab = "p_mapArrow_stroke",
			base_scale = 0.1f,
			draw_call = debugDrawArrowsUnitNavigationTargets,
			debug_option = DebugOption.ArrowsUnitsNavigationTargets,
			arrow_animation = true,
			render_gameplay = true,
			color = new Color(0.9f, 0.9f, 0.9f, 0.5f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_arrows_units_height",
			id_prefab = "p_mapArrow_line",
			base_scale = 0.1f,
			draw_call = debugDrawArrowsUnitHeight,
			debug_option = DebugOption.ArrowsUnitsHeight,
			render_gameplay = true,
			color = new Color(0f, 1f, 0f, 0.5f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_arrows_units_navigation_path",
			id_prefab = "p_mapArrow_line",
			base_scale = 0.08f,
			draw_call = debugDrawArrowsUnitNavigationPath,
			debug_option = DebugOption.ArrowsUnitsPaths,
			render_gameplay = true,
			color = new Color(0f, 0f, 0f, 0.5f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_arrows_units_next_step_tile",
			id_prefab = "p_mapArrow_line",
			base_scale = 0.08f,
			draw_call = debugDrawArrowsUnitNextStepTile,
			debug_option = DebugOption.ArrowsUnitsNextStepTile,
			render_gameplay = true,
			color = new Color(0.4f, 1f, 1f, 0.9f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_arrows_units_next_position",
			id_prefab = "p_mapArrow_line",
			base_scale = 0.08f,
			draw_call = debugDrawArrowsUnitNextStepPosition,
			debug_option = DebugOption.ArrowsUnitsNextStepPosition,
			render_gameplay = true,
			color = new Color(0.4f, 0.4f, 1f, 0.9f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_arrows_units_current_position",
			id_prefab = "p_mapArrow_line",
			base_scale = 0.08f,
			draw_call = debugDrawArrowsUnitCurrentPosition,
			debug_option = DebugOption.ArrowsUnitsCurrentPosition,
			render_gameplay = true,
			color = new Color(0f, 1f, 0f, 0.9f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_boat_passenger_lines",
			id_prefab = "p_mapArrow_line",
			base_scale = 0.08f,
			draw_call = debugDrawArrowsBoatPassengers,
			debug_option = DebugOption.BoatPassengerLines,
			render_gameplay = true,
			color = new Color(1f, 1f, 0f, 0.9f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_boat_taxi_request",
			id_prefab = "p_mapArrow_line",
			base_scale = 0.08f,
			draw_call = debugDrawArrowsPassengerTaxiRequestTargets,
			debug_option = DebugOption.ActorGizmosBoatTaxiRequestTargets,
			render_gameplay = true,
			color = new Color(0f, 1f, 0f, 0.9f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_building_residents",
			id_prefab = "p_mapArrow_line",
			base_scale = 0.08f,
			draw_call = debugDrawArrowsBuildingResidents,
			debug_option = DebugOption.BuildingResidents,
			render_gameplay = true,
			color = new Color(1f, 1f, 0f, 0.3f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_lovers",
			id_prefab = "p_mapArrow_line",
			base_scale = 0.08f,
			draw_call = debugDrawArrowsLovers,
			debug_option = DebugOption.Lovers,
			render_gameplay = true,
			color = new Color(1f, 0f, 0f, 0.5f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_favorite_foods",
			id_prefab = "p_mapSprite",
			base_scale = 0.2f,
			add_camera_zoom_multiplier = false,
			draw_call = debugDrawFavoriteFoods,
			debug_option = DebugOption.RenderFavoriteFoods,
			render_gameplay = true
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_show_kingdom_icons",
			id_prefab = "p_mapSprite",
			base_scale = 0.1f,
			add_camera_zoom_multiplier = false,
			draw_call = debugDrawKingdomIcons,
			debug_option = DebugOption.ShowKingdomIcons,
			render_gameplay = true
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_holding_items",
			id_prefab = "p_mapSprite",
			base_scale = 0.1f,
			add_camera_zoom_multiplier = false,
			draw_call = debugDrawHoldingFoods,
			debug_option = DebugOption.RenderHoldingResources,
			render_gameplay = true
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_show_zones_mush",
			id_prefab = "p_mapZone",
			base_scale = 1f,
			draw_call = debugDrawMushInfection,
			debug_option = DebugOption.ShowMushInfection,
			create_object = delegate(QuantumSpriteAsset pAsset, QuantumSprite pQSprite)
			{
				pQSprite.setColor(ref pAsset.color);
			},
			render_map = true,
			add_camera_zoom_multiplier = false,
			color = Toolbox.makeColor("#FF5E6A", 0.2f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_show_highlighted_zones",
			id_prefab = "p_mapZone",
			base_scale = 1f,
			draw_call = drawDebugHighlightedZones,
			render_map = true,
			render_gameplay = true,
			add_camera_zoom_multiplier = false
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_show_godfinger_tiles",
			id_prefab = "p_mapZone",
			base_scale = 0.15f,
			draw_call = debugDrawGodFingerTiles,
			debug_option = DebugOption.ShowGodFingerTargetting,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("EffectsBack");
				((Renderer)pQSprite.sprite_renderer).sortingOrder = 0;
			},
			render_map = true,
			render_gameplay = true,
			add_camera_zoom_multiplier = false
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_show_dragon_attack_tiles",
			id_prefab = "p_mapZone",
			base_scale = 0.15f,
			draw_call = debugDrawDragonAttackTiles,
			debug_option = DebugOption.ShowDragonTargetting,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("EffectsBack");
				((Renderer)pQSprite.sprite_renderer).sortingOrder = 0;
			},
			render_map = true,
			render_gameplay = true,
			add_camera_zoom_multiplier = false
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_show_swim_targets",
			id_prefab = "p_mapZone",
			base_scale = 0.15f,
			draw_call = drawSwimTargets,
			debug_option = DebugOption.ShowSwimToIslandLogic,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				((Renderer)pQSprite.sprite_renderer).sortingLayerID = SortingLayer.NameToID("EffectsBack");
				((Renderer)pQSprite.sprite_renderer).sortingOrder = 0;
			},
			render_map = true,
			render_gameplay = true,
			add_camera_zoom_multiplier = false
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_show_zones_zombie_infection",
			id_prefab = "p_mapZone",
			base_scale = 1f,
			draw_call = debugDrawZombieInfection,
			debug_option = DebugOption.ShowZombieInfection,
			create_object = delegate(QuantumSpriteAsset pAsset, QuantumSprite pQSprite)
			{
				pQSprite.setColor(ref pAsset.color);
			},
			render_map = true,
			add_camera_zoom_multiplier = false,
			color = Toolbox.makeColor("#3FC668", 0.2f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_show_zones_plague",
			id_prefab = "p_mapZone",
			base_scale = 1f,
			draw_call = debugDrawPlagueInfection,
			debug_option = DebugOption.ShowPlagueInfection,
			create_object = delegate(QuantumSpriteAsset pAsset, QuantumSprite pQSprite)
			{
				pQSprite.setColor(ref pAsset.color);
			},
			render_map = true,
			add_camera_zoom_multiplier = false,
			color = Toolbox.makeColor("#C444FF", 0.2f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_show_zones_curse",
			id_prefab = "p_mapZone",
			base_scale = 1f,
			draw_call = debugDrawCurseInfection,
			debug_option = DebugOption.ShowCursed,
			create_object = delegate(QuantumSpriteAsset pAsset, QuantumSprite pQSprite)
			{
				pQSprite.setColor(ref pAsset.color);
			},
			render_map = true,
			add_camera_zoom_multiplier = false,
			color = Toolbox.makeColor("#852EAD", 0.2f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_dead_units",
			id_prefab = "p_mapZone",
			base_scale = 0.2f,
			draw_call = debugDrawDeadUnits,
			debug_option = DebugOption.DeadUnits,
			create_object = delegate(QuantumSpriteAsset _, QuantumSprite pQSprite)
			{
				pQSprite.setSprite(SpriteTextureLoader.getSprite("ui/Icons/iconSkulls"));
			},
			render_map = true,
			render_gameplay = true,
			color = Toolbox.makeColor("#FFFFFF", 0.1f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_draw_bad_links",
			id_prefab = "p_mapArrow_line",
			base_scale = 0.4f,
			draw_call = debugDrawBadLinks,
			debug_option = DebugOption.DrawBadLinksDiag,
			render_arrow_end = true,
			render_arrow_start = true,
			render_map = true,
			render_gameplay = true,
			color = Toolbox.makeColor("#D300B0", 0.8f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_cursor_city_zone_range",
			id_prefab = "p_mapZone",
			base_scale = 1f,
			add_camera_zoom_multiplier = false,
			draw_call = debugCityZoneRange,
			debug_option = DebugOption.CursorCityZoneRange,
			create_object = delegate(QuantumSpriteAsset pAsset, QuantumSprite pQSprite)
			{
				pQSprite.setColor(ref pAsset.color);
			},
			render_map = true,
			render_gameplay = true,
			color = Toolbox.makeColor("#00FF00", 0.5f)
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_enemy_finder",
			id_prefab = "p_mapSprite",
			base_scale = 0.2f,
			debug_option = DebugOption.CursorEnemyFinderChunks,
			draw_call = debugEnemyFinder,
			create_object = delegate(QuantumSpriteAsset pAsset, QuantumSprite pQSprite)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				Color white = Color.white;
				white.a = 0.8f;
				pQSprite.setSprite(SpriteTextureLoader.getSprite("ui/Icons/iconAccuracy"));
				pQSprite.setColor(ref pAsset.color);
			},
			render_map = true,
			render_gameplay = true
		});
		add(new QuantumSpriteAsset
		{
			id = "debug_show_population",
			id_prefab = "p_mapZone",
			base_scale = 1f,
			draw_call = debugDrawPopulation,
			debug_option = DebugOption.ShowPopulationTotal,
			create_object = delegate(QuantumSpriteAsset pAsset, QuantumSprite pQSprite)
			{
				pQSprite.setColor(ref pAsset.color);
			},
			render_map = true,
			add_camera_zoom_multiplier = false,
			color = Toolbox.makeColor("#FFFFFF", 0.1f)
		});
	}

	private static void drawMoney(QuantumSpriteAsset pAsset)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		foreach (Actor unit in World.world.units)
		{
			if (unit.isAlive() && (unit.data.money != 0 || unit.data.loot != 0))
			{
				Vector3 pPos = Vector2.op_Implicit(unit.current_position);
				pPos.y++;
				drawQuantumSprite(pAsset, pPos);
			}
		}
	}

	private static void debugDrawArrowsSettlers(QuantumSpriteAsset pAsset)
	{
	}

	private static void debugDrawClaimZone(QuantumSpriteAsset pAsset)
	{
		WorldTile mouseTilePos = World.world.getMouseTilePos();
		City city = null;
		if (mouseTilePos != null && DebugConfig.isOn(DebugOption.ArrowsOnlyForCursorCities))
		{
			city = mouseTilePos.zone.city;
		}
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (kingdom.hasKing() && kingdom.king.isTask("claim_land"))
			{
				checkDrawClaimLand(pAsset, kingdom.king);
			}
			foreach (City city2 in kingdom.getCities())
			{
				if ((city == null || city2 == city) && city2.hasLeader() && city2.leader.isTask("claim_land"))
				{
					checkDrawClaimLand(pAsset, city2.leader);
				}
			}
		}
	}

	private static void checkDrawClaimLand(QuantumSpriteAsset pAsset, Actor pActor)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!pActor.city.isRekt())
		{
			WorldTile current_tile = pActor.current_tile;
			WorldTile beh_tile_target = pActor.beh_tile_target;
			if (current_tile != null && beh_tile_target != null)
			{
				drawArrowQuantumSprite(pAsset, current_tile.posV3, beh_tile_target.posV3, ref Toolbox.color_yellow);
			}
		}
	}

	private static void debugDrawArrowsKingdomAttackTarget(QuantumSpriteAsset pAsset)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		WorldTile mouseTilePos = World.world.getMouseTilePos();
		City city = null;
		if (mouseTilePos != null && DebugConfig.isOn(DebugOption.ArrowsOnlyForCursorCities))
		{
			city = mouseTilePos.zone.city;
		}
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			foreach (City city2 in kingdom.getCities())
			{
				if (city2.target_attack_city != null && (!Zones.showCityZones() || city == null || city2 == city))
				{
					WorldTile tile = city2.getTile();
					WorldTile centerTile = city2.target_attack_zone.centerTile;
					if (tile != null && centerTile != null)
					{
						drawArrowQuantumSprite(pAsset, tile.posV3, centerTile.posV3, ref Toolbox.color_red);
					}
				}
			}
		}
	}

	private static void drawUnitAttackRange(QuantumSpriteAsset pAsset)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (!ControllableUnit.isControllingUnit())
		{
			Actor last_actor = UnitSelectionEffect.last_actor;
			if (last_actor != null && !last_actor.isInMagnet())
			{
				float pForceScaleTo = last_actor.getAttackRange() / 13f;
				((Component)drawQuantumSprite(pAsset, Vector2.op_Implicit(last_actor.current_position), null, null, null, null, 1f, pSetColor: false, pForceScaleTo)).transform.position = Vector2.op_Implicit(last_actor.current_position);
			}
		}
	}

	private static void drawUnitSize(QuantumSpriteAsset pAsset)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Actor last_actor = UnitSelectionEffect.last_actor;
		if (last_actor != null && !last_actor.isInMagnet())
		{
			float pForceScaleTo = last_actor.stats["size"] / 13f;
			((Component)drawQuantumSprite(pAsset, Vector2.op_Implicit(last_actor.current_position), null, null, null, null, 1f, pSetColor: false, pForceScaleTo)).transform.position = Vector2.op_Implicit(last_actor.current_position);
		}
	}

	private static void debugDrawArrowsUnitAttackTargets(QuantumSpriteAsset pAsset)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		bool flag = DebugConfig.isOn(DebugOption.ArrowsUnitsFavoritesOnly);
		Actor[] array = visible_units;
		int num = visible_units_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (actor.has_attack_target && (!flag || actor.isFavorite()) && actor.isEnemyTargetAlive())
			{
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(actor.current_position), Vector2.op_Implicit(actor.attack_target.current_position), ref pAsset.color);
			}
		}
	}

	private static void debugDrawArrowsUnitBehTarget(QuantumSpriteAsset pAsset)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		bool flag = DebugConfig.isOn(DebugOption.ArrowsUnitsFavoritesOnly);
		Actor[] array = visible_units;
		int num = visible_units_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (actor.beh_actor_target != null && (!flag || actor.isFavorite()) && actor.beh_actor_target != null)
			{
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(actor.current_position), Vector2.op_Implicit(actor.beh_actor_target.current_position), ref pAsset.color);
			}
		}
	}

	private static void debugDrawArrowsUnitNavigationTargets(QuantumSpriteAsset pAsset)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		bool flag = DebugConfig.isOn(DebugOption.ArrowsUnitsFavoritesOnly);
		Actor[] array = visible_units;
		int num = visible_units_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (actor.tile_target != null && (!flag || actor.isFavorite()))
			{
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(actor.current_position), actor.tile_target.posV3, ref pAsset.color);
			}
		}
	}

	private static void debugDrawArrowsUnitHeight(QuantumSpriteAsset pAsset)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		bool flag = DebugConfig.isOn(DebugOption.ArrowsUnitsFavoritesOnly);
		Actor[] array = visible_units;
		int num = visible_units_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (!flag || actor.isFavorite())
			{
				Vector3 pStart = Vector2.op_Implicit(actor.current_position);
				Vector3 pEnd = Vector2.op_Implicit(actor.current_position);
				pEnd.y += actor.getHeight();
				drawArrowQuantumSprite(pAsset, pStart, pEnd, ref pAsset.color);
			}
		}
	}

	private static void debugDrawArrowsUnitNavigationPath(QuantumSpriteAsset pAsset)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		bool flag = DebugConfig.isOn(DebugOption.ArrowsUnitsFavoritesOnly);
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (actor.current_path.Count == 0 || (flag && !actor.isFavorite()))
			{
				continue;
			}
			WorldTile worldTile = null;
			foreach (WorldTile item in actor.current_path)
			{
				if (worldTile == null)
				{
					worldTile = item;
					drawArrowQuantumSprite(pAsset, actor.current_tile.posV3, worldTile.posV3, ref pAsset.color);
				}
				else
				{
					drawArrowQuantumSprite(pAsset, worldTile.posV3, item.posV3, ref pAsset.color);
					worldTile = item;
				}
			}
		}
	}

	private static void debugDrawArrowsUnitNextStepTile(QuantumSpriteAsset pAsset)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (actor.debug_next_step_tile != null && (!DebugConfig.isOn(DebugOption.ArrowsUnitsFavoritesOnly) || actor.isFavorite()))
			{
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(actor.current_position), actor.debug_next_step_tile.posV3, ref pAsset.color);
			}
		}
	}

	private static void debugDrawArrowsUnitNextStepPosition(QuantumSpriteAsset pAsset)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		bool flag = DebugConfig.isOn(DebugOption.ArrowsUnitsFavoritesOnly);
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (actor.is_moving && (!flag || actor.isFavorite()))
			{
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(actor.current_position), Vector2.op_Implicit(actor.next_step_position), ref pAsset.color);
			}
		}
	}

	private static void debugDrawArrowsUnitCurrentPosition(QuantumSpriteAsset pAsset)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = DebugConfig.isOn(DebugOption.ArrowsUnitsFavoritesOnly);
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (!flag || actor.isFavorite())
			{
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(actor.current_position), actor.current_tile.posV3, ref pAsset.color);
			}
		}
	}

	private static void debugDrawArrowsBoatPassengers(QuantumSpriteAsset pAsset)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (!actor.asset.is_boat || !actor.asset.is_boat_transport)
			{
				continue;
			}
			TaxiRequest taxi_request = actor.getSimpleComponent<Boat>().taxi_request;
			if (taxi_request == null)
			{
				continue;
			}
			foreach (Actor actor2 in taxi_request.getActors())
			{
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(actor2.current_position), actor.current_tile.posV3, ref pAsset.color);
			}
		}
	}

	private static void debugDrawArrowsPassengerTaxiRequestTargets(QuantumSpriteAsset pAsset)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		Color pColor = Color.cyan;
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (!actor.asset.is_boat || !actor.asset.is_boat_transport)
			{
				continue;
			}
			TaxiRequest taxi_request = actor.getSimpleComponent<Boat>().taxi_request;
			if (taxi_request == null)
			{
				continue;
			}
			foreach (Actor actor2 in taxi_request.getActors())
			{
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(actor2.current_position), taxi_request.getTileStart().posV3, ref pAsset.color);
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(actor2.current_position), taxi_request.getTileTarget().posV3, ref pColor);
			}
		}
	}

	private static void debugDrawArrowsBuildingResidents(QuantumSpriteAsset pAsset)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			Building homeBuilding = actor.getHomeBuilding();
			if (homeBuilding != null)
			{
				drawArrowQuantumSprite(pAsset, Vector2.op_Implicit(actor.current_position), homeBuilding.current_tile.posV3, ref pAsset.color);
			}
		}
	}

	private static void debugDrawArrowsLovers(QuantumSpriteAsset pAsset)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (actor.hasLover() && !(actor.data.created_time < actor.lover.data.created_time))
			{
				Actor lover = actor.lover;
				Vector3 pStart = Vector2.op_Implicit(actor.current_position);
				pStart.y += 0.5f;
				Color pColor = pAsset.color;
				if (actor.kingdom != lover.kingdom)
				{
					pColor.a = 0.1f;
				}
				else if (actor.city != lover.city)
				{
					pColor.a = 0.2f;
				}
				else
				{
					pColor.a = 0.5f;
				}
				if (actor.isKingdomCiv())
				{
					pColor.r = 1f;
					pColor.g = 0f;
					pColor.b = 0f;
				}
				else
				{
					pColor.r = 1f;
					pColor.g = 1f;
					pColor.b = 0f;
				}
				drawArrowQuantumSprite(pAsset, pStart, Vector2.op_Implicit(lover.current_position), ref pColor);
			}
		}
	}

	private static void debugDrawFavoriteFoods(QuantumSpriteAsset pAsset)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (actor.hasFavoriteFood())
			{
				QuantumSprite quantumSprite = drawQuantumSprite(pAsset, Vector2.op_Implicit(actor.current_position));
				ResourceAsset favorite_food_asset = actor.favorite_food_asset;
				quantumSprite.setSprite(favorite_food_asset.getSpriteIcon());
			}
		}
	}

	private static void debugDrawKingdomIcons(QuantumSpriteAsset pAsset)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (actor.kingdom.asset.show_icon)
			{
				Vector3 pPos = Vector2.op_Implicit(actor.current_position);
				pPos.y++;
				drawQuantumSprite(pAsset, pPos).setSprite(actor.kingdom.asset.getSprite());
			}
		}
	}

	private static void debugDrawHoldingFoods(QuantumSpriteAsset pAsset)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Actor[] array = visible_units_alive;
		int num = visible_units_alive_count;
		for (int i = 0; i < num; i++)
		{
			Actor actor = array[i];
			if (actor.isCarryingResources())
			{
				string itemIDToRender = actor.inventory.getItemIDToRender();
				if (!string.IsNullOrEmpty(itemIDToRender))
				{
					Vector3 pPos = Vector2.op_Implicit(actor.current_position);
					pPos.y += 2f;
					QuantumSprite quantumSprite = drawQuantumSprite(pAsset, pPos);
					ResourceAsset resourceAsset = AssetManager.resources.get(itemIDToRender);
					quantumSprite.setSprite(resourceAsset.getSpriteIcon());
				}
			}
		}
	}

	private static void debugDrawMushInfection(QuantumSpriteAsset pAsset)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		foreach (Actor unit in World.world.units)
		{
			if (unit.hasTrait("mush_spores"))
			{
				drawQuantumSprite(pAsset, unit.current_tile.zone.centerTile.posV);
			}
		}
	}

	private static void drawDebugHighlightedZones(QuantumSpriteAsset pAsset)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (DebugHighlight.hashset.Count == 0)
		{
			return;
		}
		foreach (DebugHighlightContainer item in DebugHighlight.hashset)
		{
			QuantumSprite quantumSprite = null;
			if (item.zone != null)
			{
				quantumSprite = drawQuantumSprite(pAsset, item.zone.centerTile.posV);
			}
			else if (item.chunk != null)
			{
				quantumSprite = drawQuantumSprite(pAsset, item.chunk.tiles[0].zone.centerTile.posV);
			}
			Color pColor = item.color;
			pColor.a = item.timer / item.interval * item.color.a;
			quantumSprite.setColor(ref pColor);
		}
	}

	private static void debugDrawGodFingerTiles(QuantumSpriteAsset pAsset)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		foreach (Actor unit in World.world.kingdoms_wild.get("godfinger").units)
		{
			if (!unit.isAlive())
			{
				continue;
			}
			GodFinger actorComponent = unit.getActorComponent<GodFinger>();
			Color pColor = actorComponent.debug_color;
			pColor.a = 0.9f;
			foreach (WorldTile target_tile in actorComponent.target_tiles)
			{
				drawQuantumSprite(pAsset, target_tile.posV).setColor(ref pColor);
			}
			GodFinger.debug_trail(actorComponent);
		}
	}

	private static void debugDrawDragonAttackTiles(QuantumSpriteAsset pAsset)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		Kingdom kingdom = World.world.kingdoms_wild.get("dragons");
		Kingdom kingdom2 = World.world.kingdoms_wild.get("undead");
		if (kingdom == null && kingdom2 == null)
		{
			return;
		}
		if (kingdom != null && kingdom.units.Count > 0)
		{
			debugDrawDragonAttackTiles(pAsset, kingdom.units);
		}
		if (kingdom2 != null && kingdom2.units.Count > 0)
		{
			debugDrawDragonAttackTiles(pAsset, kingdom2.units);
		}
		foreach (WorldTile temp_list_tile in Toolbox.temp_list_tiles)
		{
			QuantumSprite quantumSprite = drawQuantumSprite(pAsset, temp_list_tile.posV);
			Color pColor = Toolbox.color_mushSpores;
			pColor.a = 0.4f;
			quantumSprite.setColor(ref pColor);
		}
	}

	private static void debugDrawDragonAttackTiles(QuantumSpriteAsset pAsset, List<Actor> pUnits)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		foreach (Actor pUnit in pUnits)
		{
			if (!pUnit.isAlive())
			{
				continue;
			}
			Dragon actorComponent = pUnit.getActorComponent<Dragon>();
			if ((Object)(object)actorComponent == (Object)null)
			{
				continue;
			}
			Color pColor = Toolbox.color_infected;
			float num = 0.1f + (float)actorComponent._landAttackCache * 0.1f;
			pColor.a = Mathf.Min(num, 0.8f);
			foreach (WorldTile landAttackTile in actorComponent.getLandAttackTiles())
			{
				drawQuantumSprite(pAsset, landAttackTile.posV).setColor(ref pColor);
			}
			pColor = Color32.op_Implicit(Toolbox.color_phenotype_green_0);
			num = 0.1f + (float)actorComponent._slideAttackTilesFlipCache * 0.1f;
			pColor.a = Mathf.Min(num, 0.8f);
			foreach (WorldTile item in actorComponent._slideAttackTilesFlip)
			{
				drawQuantumSprite(pAsset, item.posV).setColor(ref pColor);
			}
			pColor = Color32.op_Implicit(Toolbox.color_magenta_1);
			num = 0.1f + (float)actorComponent._slideAttackTilesNoFlipCache * 0.1f;
			pColor.a = Mathf.Min(num, 0.8f);
			foreach (WorldTile item2 in actorComponent._slideAttackTilesNoFlip)
			{
				drawQuantumSprite(pAsset, item2.posV).setColor(ref pColor);
			}
			pColor = Toolbox.color_red;
			if (pUnit.tile_target != null)
			{
				drawQuantumSprite(pAsset, pUnit.tile_target.posV).setColor(ref pColor);
			}
			pColor = Toolbox.color_heal;
			if (pUnit.beh_tile_target != null)
			{
				drawQuantumSprite(pAsset, pUnit.beh_tile_target.posV).setColor(ref pColor);
			}
		}
	}

	private static void drawSwimTargets(QuantumSpriteAsset pAsset)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		Color pColor = Toolbox.color_infected;
		pColor.a = 0.8f;
		foreach (KeyValuePair<int, MapRegion> bestRegion in BehGoToStablePlace.bestRegions)
		{
			List<WorldTile> tiles = bestRegion.Value.tiles;
			for (int i = 0; i < tiles.Count; i++)
			{
				drawQuantumSprite(pAsset, tiles[i].posV).setColor(ref pColor);
			}
		}
		if (BehGoToStablePlace.best_tile != null)
		{
			pColor = Toolbox.color_red;
			drawQuantumSprite(pAsset, BehGoToStablePlace.best_tile.posV).setColor(ref pColor);
		}
	}

	private static void debugDrawZombieInfection(QuantumSpriteAsset pAsset)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		foreach (Actor unit in World.world.units)
		{
			if (unit.hasTrait("infected") || unit.hasTrait("zombie"))
			{
				drawQuantumSprite(pAsset, unit.current_tile.zone.centerTile.posV);
			}
		}
	}

	private static void debugDrawPlagueInfection(QuantumSpriteAsset pAsset)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		foreach (Actor unit in World.world.units)
		{
			if (unit.hasTrait("plague"))
			{
				drawQuantumSprite(pAsset, unit.current_tile.zone.centerTile.posV);
			}
		}
	}

	private static void debugDrawCurseInfection(QuantumSpriteAsset pAsset)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		foreach (Actor unit in World.world.units)
		{
			if (unit.hasStatus("cursed"))
			{
				drawQuantumSprite(pAsset, unit.current_tile.zone.centerTile.posV);
			}
		}
	}

	private static void debugDrawDeadUnits(QuantumSpriteAsset pAsset)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		foreach (Actor unit in World.world.units)
		{
			if (!unit.isAlive())
			{
				drawQuantumSprite(pAsset, Vector2.op_Implicit(unit.current_position));
			}
		}
	}

	private static void debugDrawCitizenJobs(QuantumSpriteAsset pAsset)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		foreach (Actor unit in World.world.units)
		{
			if (unit.citizen_job != null && (DebugConfig.isOn(DebugOption.DrawCitizenJobIconsAll) || DebugConfig.isOn(unit.citizen_job.debug_option)))
			{
				QuantumSprite quantumSprite = drawQuantumSprite(pAsset, Vector2.op_Implicit(unit.current_position));
				Sprite sprite = SpriteTextureLoader.getSprite(unit.citizen_job.path_icon);
				quantumSprite.setSprite(sprite);
			}
		}
	}

	private static void debugDrawBadLinks(QuantumSpriteAsset pAsset)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		MapChunk[] chunks = World.world.map_chunk_manager.chunks;
		for (int i = 0; i < chunks.Length; i++)
		{
			foreach (MapRegion region in chunks[i].regions)
			{
				foreach (MapRegion neighbour in region.neighbours)
				{
					if (!(Toolbox.Dist(neighbour.chunk.x, neighbour.chunk.y, region.chunk.x, region.chunk.y) < 1.5f))
					{
						Vector3 posV = region.tiles[0].posV;
						Vector3 posV2 = neighbour.tiles[0].posV;
						drawArrowQuantumSprite(pAsset, posV, posV2, ref pAsset.color);
						break;
					}
				}
			}
		}
	}

	private static void debugCityZoneRange(QuantumSpriteAsset pAsset)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		WorldTile mouseTilePos = World.world.getMouseTilePos();
		if (mouseTilePos == null)
		{
			return;
		}
		City city = mouseTilePos.zone.city;
		if (city.isRekt())
		{
			return;
		}
		HashSet<TileZone> hashSet = new HashSet<TileZone>();
		Bench.bench("debugCityZoneRange", "meh");
		World.world.city_zone_helper.city_growth.getZoneToClaim(null, city, pDebug: true, hashSet);
		Debug.Log((object)("bench city growth: " + Bench.benchEnd("debugCityZoneRange", "meh", pSaveCounter: false, 0L)));
		foreach (TileZone item in hashSet)
		{
			drawQuantumSprite(pAsset, item.centerTile.posV);
		}
	}

	private static void debugEnemyFinder(QuantumSpriteAsset pAsset)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (World.world.getMouseTilePos() == null)
		{
			return;
		}
		Actor actorNearCursor = World.world.getActorNearCursor();
		if (actorNearCursor == null || actorNearCursor.isInMagnet())
		{
			return;
		}
		EnemyFinderData enemyFinderData = EnemiesFinder.findEnemiesFrom(actorNearCursor.current_tile, actorNearCursor.kingdom);
		if (enemyFinderData.isEmpty())
		{
			return;
		}
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(0f, 1f);
		foreach (BaseSimObject item in enemyFinderData.list)
		{
			drawQuantumSprite(pAsset, Vector2.op_Implicit(item.current_position + val), null, null, null, null, 0.2f);
		}
	}

	private static void debugDrawPopulation(QuantumSpriteAsset pAsset)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		foreach (Actor unit in World.world.units)
		{
			drawQuantumSprite(pAsset, unit.current_tile.zone.centerTile.posV);
		}
	}

	private static void drawStockpileResources(QuantumSpriteAsset pAsset)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		List<Building> visible_stockpiles = World.world.buildings.visible_stockpiles;
		if (visible_stockpiles.Count == 0)
		{
			return;
		}
		if (_array_stockpile_slots == null)
		{
			_array_stockpile_slots = (Vector2[])(object)new Vector2[35];
			for (int i = 0; i < 5; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					int num = i * 7 + j;
					_array_stockpile_slots[num] = new Vector2((float)j, (float)i);
				}
			}
			_array_stockpile_slots.Shuffle();
		}
		foreach (Building item in visible_stockpiles)
		{
			if (item.is_visible && item.isUsable() && !item.isUnderConstruction())
			{
				drawStockpileResourcesForBuilding(pAsset, item);
			}
		}
	}

	private static void drawStockpileResourcesForBuilding(QuantumSpriteAsset pAsset, Building pBuilding)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		float tweenBuildingsValue = World.world.quality_changer.getTweenBuildingsValue();
		Color pColor = Toolbox.color_white;
		if (!pBuilding.hasCity())
		{
			pColor = Toolbox.color_abandoned_building;
		}
		Vector3 cur_transform_position = pBuilding.cur_transform_position;
		cur_transform_position.x += pBuilding.asset.stockpile_top_left_offset.x * tweenBuildingsValue;
		cur_transform_position.y += pBuilding.asset.stockpile_top_left_offset.y * tweenBuildingsValue;
		cur_transform_position.z = 0f;
		using ListPool<SlotDrawAmount> listPool = new ListPool<SlotDrawAmount>();
		foreach (CityStorageSlot slot in pBuilding.resources.getSlots())
		{
			if (slot.amount != 0)
			{
				listPool.Add(new SlotDrawAmount
				{
					resource_id = slot.id,
					amount = slot.amount / slot.asset.stack_size + 1
				});
			}
		}
		int num = 0;
		int num2 = 0;
		while (num < 35 && listPool.Count > 0)
		{
			SlotDrawAmount value = listPool[num2];
			if (value.amount <= 0)
			{
				listPool.RemoveAt(num2);
				if (num2 >= listPool.Count)
				{
					num2 = 0;
				}
				continue;
			}
			int amount = value.amount;
			if (amount <= 0)
			{
				break;
			}
			int pRow = (int)_array_stockpile_slots[num].x;
			int num3 = (int)_array_stockpile_slots[num].y;
			ResourceAsset asset = value.asset;
			int num4 = amount;
			Sprite gameplaySprite = asset.getGameplaySprite();
			int num5 = Mathf.Clamp(num4, 1, 7);
			if (num3 % 2 != 0)
			{
				num5--;
			}
			value.amount -= num5;
			listPool[num2] = value;
			for (int i = 0; i < num5; i++)
			{
				drawResourceIconOnStockpile(pAsset, cur_transform_position, gameplaySprite, i, pRow, num3, ref pColor);
			}
			num++;
			num2++;
			if (num2 >= listPool.Count)
			{
				num2 = 0;
			}
			if (num < 35)
			{
				continue;
			}
			break;
		}
	}

	private static void drawResourceIconOnStockpile(QuantumSpriteAsset pAsset, Vector3 pMainPosition, Sprite pSprite, int pIndex, int pRow, int pColumn, ref Color pColor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Vector3 pPos = pMainPosition;
		pPos.x += 0.58f * (float)pRow;
		pPos.y -= 0.5f * (float)pColumn;
		if (pColumn % 2 != 0)
		{
			pPos.x += 0.29f;
		}
		pPos.y += 0.4f * (float)pIndex;
		pPos.z += 0.5f * (float)pIndex;
		QuantumSprite quantumSprite = drawQuantumSprite(pAsset, pPos);
		quantumSprite.setSprite(pSprite);
		quantumSprite.setColor(ref pColor);
	}
}
