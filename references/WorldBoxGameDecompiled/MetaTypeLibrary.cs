using System;
using System.Collections.Generic;
using UnityEngine;

public class MetaTypeLibrary : AssetLibrary<MetaTypeAsset>
{
	[NonSerialized]
	public static MetaTypeAsset alliance;

	[NonSerialized]
	public static MetaTypeAsset city;

	[NonSerialized]
	public static MetaTypeAsset clan;

	[NonSerialized]
	public static MetaTypeAsset culture;

	[NonSerialized]
	public static MetaTypeAsset family;

	[NonSerialized]
	public static MetaTypeAsset army;

	[NonSerialized]
	public static MetaTypeAsset kingdom;

	[NonSerialized]
	public static MetaTypeAsset language;

	[NonSerialized]
	public static MetaTypeAsset plot;

	[NonSerialized]
	public static MetaTypeAsset religion;

	[NonSerialized]
	public static MetaTypeAsset subspecies;

	[NonSerialized]
	public static MetaTypeAsset unit;

	[NonSerialized]
	public static MetaTypeAsset war;

	[NonSerialized]
	public static MetaTypeAsset item;

	private ZoneCalculator zone_manager => World.world.zone_calculator;

	public override void init()
	{
		base.init();
		add(new MetaTypeAsset
		{
			id = "world",
			window_name = "world_info",
			get_list = () => new NanoObject[0],
			has_any = () => false,
			get_selected = () => World.world.world_object,
			set_selected = delegate(NanoObject pElement)
			{
				World.world.world_object = pElement as WorldObject;
			},
			get = (long _) => World.world.world_object
		});
		item = add(new MetaTypeAsset
		{
			id = "item",
			window_name = "item",
			window_action_clear = delegate
			{
				SelectedMetas.selected_item = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.item = SelectedMetas.selected_item;
				if (WindowHistory.hasHistory() && ((Component)WindowHistory.list.Last().window).GetComponent<ItemWindow>() != null)
				{
					ScrollWindow.setPreviousWindowSprite(pHistoryData.item.getSprite());
				}
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_item = pHistoryData.item;
			},
			get_list = () => World.world.items,
			custom_sorted_list = delegate
			{
				ListPool<NanoObject> listPool = new ListPool<NanoObject>(64);
				foreach (Item item2 in World.world.items)
				{
					if (item2.isFavorite())
					{
						listPool.Add(item2);
					}
				}
				return listPool;
			},
			has_any = () => World.world.items.hasAny(),
			get_selected = () => SelectedMetas.selected_item,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_item = pElement as Item;
			},
			get = (long pId) => World.world.items.get(pId),
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				Item pObject = World.world.items.get(pMetaId);
				if (!pObject.isRekt())
				{
					Tooltip.show(pField, "equipment", new TooltipData
					{
						item = pObject
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				Item item = World.world.items.get(pMetaId);
				if (!item.isRekt())
				{
					SelectedMetas.selected_item = item;
					ScrollWindow.showWindow("item");
				}
			}
		});
		unit = add(new MetaTypeAsset
		{
			id = "unit",
			ranks = generateExponentialRanks(100.0, 1.5),
			window_name = "unit",
			power_tab_id = "selected_unit",
			icon_single_path = "ui/icons/iconSpecies",
			window_action_clear = delegate
			{
				if (SelectedUnit.isSet() && SelectedObjects.getSelectedNanoObject() is Actor)
				{
					PowerTabController.showTabSelectedUnit();
				}
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				if (SelectedUnit.isSet())
				{
					pHistoryData.unit = SelectedUnit.unit;
					if (WindowHistory.hasHistory())
					{
						WindowHistoryData windowHistoryData = WindowHistory.list.Last();
						if (((Component)windowHistoryData.window).GetComponent<UnitWindow>() != null && !windowHistoryData.unit.isRekt())
						{
							ScrollWindow.setPreviousWindowSprite(windowHistoryData.unit.asset.getSpriteIcon());
						}
					}
				}
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				if (!pHistoryData.unit.isRekt())
				{
					SelectedUnit.clear();
					SelectedUnit.select(pHistoryData.unit);
				}
				else
				{
					SelectedUnit.clear();
				}
			},
			get_list = () => World.world.units,
			custom_sorted_list = delegate
			{
				ListPool<NanoObject> listPool = new ListPool<NanoObject>(64);
				foreach (Actor unit in World.world.units)
				{
					if (!unit.isRekt() && unit.isFavorite())
					{
						listPool.Add(unit);
					}
				}
				return listPool;
			},
			has_any = () => World.world.units.Count > 0,
			get_selected = () => SelectedUnit.unit,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedUnit.select(pElement as Actor);
			},
			get = (long pId) => World.world.units.get(pId),
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				Actor actor = World.world.units.get(pMetaId);
				if (!actor.isRekt())
				{
					string pType = "actor";
					if (actor.isKing())
					{
						pType = "actor_king";
					}
					if (actor.isCityLeader())
					{
						pType = "actor_leader";
					}
					Tooltip.show(pField, pType, new TooltipData
					{
						actor = actor
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				Actor actor = World.world.units.get(pMetaId);
				if (!actor.isRekt())
				{
					ActionLibrary.openUnitWindow(actor);
				}
			}
		});
		war = add(new MetaTypeAsset
		{
			id = "war",
			window_name = "war",
			icon_list = "iconWarList",
			window_action_clear = delegate
			{
				SelectedMetas.selected_war = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.war = SelectedMetas.selected_war;
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_war = pHistoryData.war;
			},
			reports = new string[7] { "war_high_casualties", "war_long", "war_fresh", "war_defenders_getting_captured", "war_attackers_getting_captured", "war_quiet", "war_full_on_battle" },
			get_list = () => World.world.wars,
			has_any = () => World.world.wars.hasAny(),
			get_selected = () => SelectedMetas.selected_war,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_war = pElement as War;
			},
			get = (long pId) => World.world.wars.get(pId),
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				War pObject = World.world.wars.get(pMetaId);
				if (!pObject.isRekt())
				{
					Tooltip.show(pField, "war", new TooltipData
					{
						war = pObject
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				War war = World.world.wars.get(pMetaId);
				if (!war.isRekt())
				{
					SelectedMetas.selected_war = war;
					ScrollWindow.showWindow("war");
				}
			}
		});
		plot = add(new MetaTypeAsset
		{
			id = "plot",
			window_name = "plot",
			icon_list = "iconPlotList",
			window_action_clear = delegate
			{
				SelectedMetas.selected_plot = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.plot = SelectedMetas.selected_plot;
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_plot = pHistoryData.plot;
			},
			get_list = () => World.world.plots,
			has_any = () => World.world.plots.hasAny(),
			get_selected = () => SelectedMetas.selected_plot,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_plot = pElement as Plot;
			},
			get = (long pId) => World.world.plots.get(pId),
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				Plot pObject = World.world.plots.get(pMetaId);
				if (!pObject.isRekt())
				{
					Tooltip.show(pField, "plot", new TooltipData
					{
						plot = pObject
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				Plot plot = World.world.plots.get(pMetaId);
				if (!plot.isRekt())
				{
					SelectedMetas.selected_plot = plot;
					ScrollWindow.showWindow("plot");
				}
			},
			decision_ids = new string[1] { "check_plot" }
		});
		religion = add(new MetaTypeAsset
		{
			id = "religion",
			ranks = generateExponentialRanks(100.0, 1.5),
			window_name = "religion",
			power_tab_id = "selected_religion",
			force_zone_when_selected = true,
			set_icon_for_cancel_button = true,
			icon_list = "iconReligionList",
			icon_single_path = "ui/icons/iconReligion",
			window_action_clear = delegate
			{
				SelectedMetas.selected_religion = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.religion = SelectedMetas.selected_religion;
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_religion = pHistoryData.religion;
			},
			reports = new string[4] { "happy", "unhappy", "many_children", "many_homeless" },
			get_list = () => World.world.religions,
			has_any = () => World.world.religions.hasAny(),
			get_selected = () => SelectedMetas.selected_religion,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_religion = pElement as Religion;
			},
			get = (long pId) => World.world.religions.get(pId),
			map_mode = MetaType.Religion,
			option_id = "map_religion_layer",
			power_option_zone_id = "religion_layer",
			has_dynamic_zones = true,
			click_action_zone = ActionLibrary.inspectReligion,
			selected_tab_action_meta = defaultClickActionZone,
			check_unit_has_meta = (Actor pActor) => pActor.hasReligion(),
			set_unit_set_meta_for_meta_for_window = delegate(Actor pActor)
			{
				SelectedMetas.selected_religion = pActor.religion;
			},
			draw_zones = delegate(MetaTypeAsset pMetaTypeAsset)
			{
				if (religion.getZoneOptionState() == 2)
				{
					drawDefaultFluid(pMetaTypeAsset);
				}
				else
				{
					drawDefaultMeta(pMetaTypeAsset);
				}
			},
			check_cursor_highlight = delegate(MetaTypeAsset pMetaTypeAsset, WorldTile pTile, QuantumSpriteAsset pQAsset)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
				//IL_006c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00df: Unknown result type (might be due to invalid IL or missing references)
				Color color = pQAsset.color;
				switch (pMetaTypeAsset.getZoneOptionState())
				{
				case 0:
				{
					City city2 = pTile.zone.city;
					if (!city2.isRekt())
					{
						Religion religion = city2.kingdom.getReligion();
						if (!religion.isRekt())
						{
							{
								foreach (City city3 in city2.kingdom.getCities())
								{
									QuantumSpriteLibrary.colorZones(pQAsset, city3.zones, color);
								}
								break;
							}
						}
					}
					break;
				}
				case 1:
				{
					City city = pTile.zone.city;
					if (!city.isRekt())
					{
						Religion religion = city.getReligion();
						if (!religion.isRekt())
						{
							{
								foreach (City city4 in World.world.cities)
								{
									if (city4.getReligion() == religion)
									{
										QuantumSpriteLibrary.colorZones(pQAsset, city4.zones, color);
									}
								}
								break;
							}
						}
					}
					break;
				}
				default:
					highlightDefault(pTile, pQAsset, color);
					break;
				}
			},
			tile_get_metaobject = (TileZone pZone, int pZoneOption) => pZone.getReligionOnZone(pZoneOption),
			tile_get_metaobject_0 = (TileZone pZone) => pZone.city?.kingdom.religion,
			tile_get_metaobject_1 = (TileZone pZone) => pZone.city?.religion,
			tile_get_metaobject_2 = (TileZone pZone) => ZoneMetaDataVisualizer.getZoneMetaData(pZone).meta_object,
			check_tile_has_meta = checkTileHasMetaDefault,
			check_cursor_tooltip = checkCursorTooltipDefault,
			cursor_tooltip_action = delegate(NanoObject pMeta)
			{
				Religion religion = pMeta as Religion;
				if (!religion.isRekt())
				{
					string text = "religion";
					Tooltip.hideTooltip(religion, pOnlySimObjects: true, text);
					Tooltip.show(religion, text, new TooltipData
					{
						religion = religion,
						tooltip_scale = 0.7f,
						is_sim_tooltip = true
					});
				}
			},
			dynamic_zones = delegate
			{
				List<Actor> simpleList = World.world.units.getSimpleList();
				double curWorldTime = World.world.getCurWorldTime();
				int i = 0;
				for (int count = simpleList.Count; i < count; i++)
				{
					Actor actor = simpleList[i];
					if (actor.asset.show_on_meta_layer)
					{
						TileZone zone = actor.current_tile.zone;
						if (actor.hasReligion())
						{
							ZoneMetaDataVisualizer.countMetaZone(zone, actor.religion, curWorldTime);
						}
					}
				}
			},
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				Religion pObject = World.world.religions.get(pMetaId);
				if (!pObject.isRekt())
				{
					Tooltip.show(pField, "religion", new TooltipData
					{
						religion = pObject
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				Religion religion = World.world.religions.get(pMetaId);
				if (!religion.isRekt())
				{
					SelectedMetas.selected_religion = religion;
					ScrollWindow.showWindow("religion");
				}
			}
		});
		culture = add(new MetaTypeAsset
		{
			id = "culture",
			ranks = generateExponentialRanks(100.0, 1.5),
			window_name = "culture",
			power_tab_id = "selected_culture",
			force_zone_when_selected = true,
			set_icon_for_cancel_button = true,
			icon_list = "iconCultureList",
			icon_single_path = "ui/icons/iconCulture",
			window_action_clear = delegate
			{
				SelectedMetas.selected_culture = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.culture = SelectedMetas.selected_culture;
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_culture = pHistoryData.culture;
			},
			reports = new string[4] { "happy", "unhappy", "many_children", "many_homeless" },
			get_list = () => World.world.cultures,
			has_any = () => World.world.cultures.hasAny(),
			get_selected = () => SelectedMetas.selected_culture,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_culture = pElement as Culture;
			},
			get = (long pId) => World.world.cultures.get(pId),
			map_mode = MetaType.Culture,
			option_id = "map_culture_layer",
			power_option_zone_id = "culture_layer",
			has_dynamic_zones = true,
			click_action_zone = ActionLibrary.inspectCulture,
			selected_tab_action_meta = defaultClickActionZone,
			check_unit_has_meta = (Actor pActor) => pActor.hasCulture(),
			set_unit_set_meta_for_meta_for_window = delegate(Actor pActor)
			{
				SelectedMetas.selected_culture = pActor.culture;
			},
			draw_zones = delegate(MetaTypeAsset pMetaTypeAsset)
			{
				if (pMetaTypeAsset.isMetaZoneOptionSelectedFluid())
				{
					drawDefaultFluid(pMetaTypeAsset);
				}
				else
				{
					drawDefaultMeta(pMetaTypeAsset);
				}
			},
			check_cursor_highlight = delegate(MetaTypeAsset pMetaTypeAsset, WorldTile pTile, QuantumSpriteAsset pQAsset)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
				//IL_006c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00df: Unknown result type (might be due to invalid IL or missing references)
				Color color = pQAsset.color;
				switch (pMetaTypeAsset.getZoneOptionState())
				{
				case 0:
				{
					City city2 = pTile.zone.city;
					if (!city2.isRekt())
					{
						Culture culture = city2.kingdom.getCulture();
						if (!culture.isRekt())
						{
							{
								foreach (City city5 in city2.kingdom.getCities())
								{
									QuantumSpriteLibrary.colorZones(pQAsset, city5.zones, color);
								}
								break;
							}
						}
					}
					break;
				}
				case 1:
				{
					City city = pTile.zone.city;
					if (!city.isRekt())
					{
						Culture culture = city.getCulture();
						if (!culture.isRekt())
						{
							{
								foreach (City city6 in World.world.cities)
								{
									if (city6.getCulture() == culture)
									{
										QuantumSpriteLibrary.colorZones(pQAsset, city6.zones, color);
									}
								}
								break;
							}
						}
					}
					break;
				}
				default:
					highlightDefault(pTile, pQAsset, color);
					break;
				}
			},
			tile_get_metaobject = (TileZone pZone, int pZoneOption) => pZone.getCultureOnZone(pZoneOption),
			tile_get_metaobject_0 = (TileZone pZone) => pZone.city?.kingdom.culture,
			tile_get_metaobject_1 = (TileZone pZone) => pZone.city?.culture,
			tile_get_metaobject_2 = (TileZone pZone) => ZoneMetaDataVisualizer.getZoneMetaData(pZone).meta_object,
			check_tile_has_meta = checkTileHasMetaDefault,
			check_cursor_tooltip = checkCursorTooltipDefault,
			cursor_tooltip_action = delegate(NanoObject pMeta)
			{
				Culture culture = pMeta as Culture;
				if (!culture.isRekt())
				{
					string text = "culture";
					Tooltip.hideTooltip(culture, pOnlySimObjects: true, text);
					Tooltip.show(culture, text, new TooltipData
					{
						culture = culture,
						tooltip_scale = 0.7f,
						is_sim_tooltip = true
					});
				}
			},
			dynamic_zones = delegate
			{
				List<Actor> simpleList = World.world.units.getSimpleList();
				double curWorldTime = World.world.getCurWorldTime();
				int i = 0;
				for (int count = simpleList.Count; i < count; i++)
				{
					Actor actor = simpleList[i];
					if (actor.asset.show_on_meta_layer)
					{
						TileZone zone = actor.current_tile.zone;
						if (actor.hasCulture())
						{
							ZoneMetaDataVisualizer.countMetaZone(zone, actor.culture, curWorldTime);
						}
					}
				}
			},
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				Culture pObject = World.world.cultures.get(pMetaId);
				if (!pObject.isRekt())
				{
					Tooltip.show(pField, "culture", new TooltipData
					{
						culture = pObject
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				Culture culture = World.world.cultures.get(pMetaId);
				if (!culture.isRekt())
				{
					SelectedMetas.selected_culture = culture;
					ScrollWindow.showWindow("culture");
				}
			}
		});
		family = add(new MetaTypeAsset
		{
			id = "family",
			window_name = "family",
			power_tab_id = "selected_family",
			force_zone_when_selected = true,
			set_icon_for_cancel_button = true,
			unit_amount_alpha = true,
			icon_list = "iconFamilyList",
			icon_single_path = "ui/icons/iconFamily",
			window_action_clear = delegate
			{
				SelectedMetas.selected_family = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.family = SelectedMetas.selected_family;
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_family = pHistoryData.family;
			},
			reports = new string[4] { "happy", "unhappy", "many_children", "many_homeless" },
			get_list = () => World.world.families,
			has_any = () => World.world.families.hasAny(),
			get_selected = () => SelectedMetas.selected_family,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_family = pElement as Family;
			},
			get = (long pId) => World.world.families.get(pId),
			map_mode = MetaType.Family,
			option_id = "map_family_layer",
			power_option_zone_id = "family_layer",
			has_dynamic_zones = true,
			decision_ids = new string[5] { "family_check_existence", "family_alpha_move", "family_group_follow", "family_group_leave", "child_follow_parent" },
			click_action_zone = ActionLibrary.inspectFamily,
			selected_tab_action_meta = defaultClickActionZone,
			check_unit_has_meta = (Actor pActor) => pActor.hasFamily(),
			set_unit_set_meta_for_meta_for_window = delegate(Actor pActor)
			{
				SelectedMetas.selected_family = pActor.family;
			},
			draw_zones = delegate(MetaTypeAsset pMetaTypeAsset)
			{
				if (pMetaTypeAsset.isMetaZoneOptionSelectedFluid())
				{
					drawDefaultFluid(pMetaTypeAsset);
				}
				else
				{
					drawDefaultMeta(pMetaTypeAsset);
				}
			},
			check_cursor_highlight = delegate(MetaTypeAsset pMetaTypeAsset, WorldTile pTile, QuantumSpriteAsset pQAsset)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0164: Unknown result type (might be due to invalid IL or missing references)
				//IL_0095: Unknown result type (might be due to invalid IL or missing references)
				//IL_0144: Unknown result type (might be due to invalid IL or missing references)
				Color color = pQAsset.color;
				switch (pMetaTypeAsset.getZoneOptionState())
				{
				case 0:
				{
					City city2 = pTile.zone.city;
					if (!city2.isRekt() && city2.kingdom.hasKing() && city2.kingdom.king.hasFamily())
					{
						Family family = city2.kingdom.king.family;
						if (!family.isRekt())
						{
							{
								foreach (City city7 in city2.kingdom.getCities())
								{
									QuantumSpriteLibrary.colorZones(pQAsset, city7.zones, color);
								}
								break;
							}
						}
					}
					break;
				}
				case 1:
				{
					City city = pTile.zone.city;
					if (!city.isRekt() && city.hasLeader() && city.leader.hasFamily())
					{
						Family family = city.leader.family;
						if (!family.isRekt())
						{
							{
								foreach (City city8 in World.world.cities)
								{
									if (!city8.hasLeader() || !city8.leader.hasFamily())
									{
										break;
									}
									if (city8.leader.family == family)
									{
										QuantumSpriteLibrary.colorZones(pQAsset, city8.zones, color);
									}
								}
								break;
							}
						}
					}
					break;
				}
				default:
					highlightDefault(pTile, pQAsset, color);
					break;
				}
			},
			tile_get_metaobject = (TileZone pZone, int pZoneOption) => pZone.getFamilyOnZone(pZoneOption),
			tile_get_metaobject_0 = (TileZone pZone) => pZone.city?.kingdom.king?.family,
			tile_get_metaobject_1 = (TileZone pZone) => pZone.city?.leader?.family,
			tile_get_metaobject_2 = (TileZone pZone) => ZoneMetaDataVisualizer.getZoneMetaData(pZone).meta_object,
			check_tile_has_meta = checkTileHasMetaDefault,
			check_cursor_tooltip = checkCursorTooltipDefault,
			cursor_tooltip_action = delegate(NanoObject pMeta)
			{
				Family family = pMeta as Family;
				if (!family.isRekt())
				{
					string text = "family";
					Tooltip.hideTooltip(family, pOnlySimObjects: true, text);
					Tooltip.show(family, text, new TooltipData
					{
						family = family,
						tooltip_scale = 0.7f,
						is_sim_tooltip = true
					});
				}
			},
			dynamic_zones = delegate
			{
				List<Actor> simpleList = World.world.units.getSimpleList();
				double curWorldTime = World.world.getCurWorldTime();
				int i = 0;
				for (int count = simpleList.Count; i < count; i++)
				{
					Actor actor = simpleList[i];
					if (actor.asset.show_on_meta_layer)
					{
						TileZone zone = actor.current_tile.zone;
						if (actor.hasFamily() && actor.family.units.Count >= 2)
						{
							ZoneMetaDataVisualizer.countMetaZone(zone, actor.family, curWorldTime);
						}
					}
				}
			},
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				Family pObject = World.world.families.get(pMetaId);
				if (!pObject.isRekt())
				{
					Tooltip.show(pField, "family", new TooltipData
					{
						family = pObject
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				Family family = World.world.families.get(pMetaId);
				if (!family.isRekt())
				{
					SelectedMetas.selected_family = family;
					ScrollWindow.showWindow("family");
				}
			}
		});
		army = add(new MetaTypeAsset
		{
			id = "army",
			window_name = "army",
			power_tab_id = "selected_army",
			force_zone_when_selected = true,
			set_icon_for_cancel_button = true,
			icon_list = "iconArmyList",
			icon_single_path = "ui/icons/iconArmy",
			window_action_clear = delegate
			{
				SelectedMetas.selected_army = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.army = SelectedMetas.selected_army;
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_army = pHistoryData.army;
			},
			get_list = () => World.world.armies,
			has_any = () => World.world.armies.hasAny(),
			get_selected = () => SelectedMetas.selected_army,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_army = pElement as Army;
			},
			get = (long pId) => World.world.armies.get(pId),
			map_mode = MetaType.Army,
			option_id = "map_army_layer",
			power_option_zone_id = "army_layer",
			has_dynamic_zones = true,
			dynamic_zone_option = 0,
			click_action_zone = ActionLibrary.inspectArmy,
			selected_tab_action_meta = defaultClickActionZone,
			check_unit_has_meta = (Actor pActor) => pActor.hasArmy(),
			set_unit_set_meta_for_meta_for_window = delegate(Actor pActor)
			{
				SelectedMetas.selected_army = pActor.army;
			},
			reports = new string[2] { "happy", "unhappy" },
			draw_zones = delegate(MetaTypeAsset pMetaTypeAsset)
			{
				if (pMetaTypeAsset.isMetaZoneOptionSelectedFluid())
				{
					drawDefaultFluid(pMetaTypeAsset);
				}
				else
				{
					drawDefaultMeta(pMetaTypeAsset);
				}
			},
			check_cursor_highlight = delegate(MetaTypeAsset pMetaTypeAsset, WorldTile pTile, QuantumSpriteAsset pQAsset)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				Color color = pQAsset.color;
				pMetaTypeAsset.getZoneOptionState();
				highlightDefault(pTile, pQAsset, color);
			},
			tile_get_metaobject = (TileZone pZone, int pZoneOption) => pZone.getArmyOnZone(pZoneOption),
			tile_get_metaobject_0 = (TileZone _) => (IMetaObject)null,
			tile_get_metaobject_1 = (TileZone _) => (IMetaObject)null,
			tile_get_metaobject_2 = (TileZone pZone) => ZoneMetaDataVisualizer.getZoneMetaData(pZone).meta_object,
			check_tile_has_meta = checkTileHasMetaDefault,
			check_cursor_tooltip = checkCursorTooltipDefault,
			cursor_tooltip_action = delegate(NanoObject pMeta)
			{
				Army army = pMeta as Army;
				if (!army.isRekt())
				{
					string text = "army";
					Tooltip.hideTooltip(army, pOnlySimObjects: true, text);
					Tooltip.show(army, text, new TooltipData
					{
						army = army,
						tooltip_scale = 0.7f,
						is_sim_tooltip = true
					});
				}
			},
			dynamic_zones = delegate
			{
				List<Actor> simpleList = World.world.units.getSimpleList();
				double curWorldTime = World.world.getCurWorldTime();
				int i = 0;
				for (int count = simpleList.Count; i < count; i++)
				{
					Actor actor = simpleList[i];
					if (actor.asset.show_on_meta_layer)
					{
						TileZone zone = actor.current_tile.zone;
						if (actor.hasArmy())
						{
							ZoneMetaDataVisualizer.countMetaZone(zone, actor.army, curWorldTime);
						}
					}
				}
			},
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				Army pObject = World.world.armies.get(pMetaId);
				if (!pObject.isRekt())
				{
					Tooltip.show(pField, "army", new TooltipData
					{
						army = pObject
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				Army army = World.world.armies.get(pMetaId);
				if (!army.isRekt())
				{
					SelectedMetas.selected_army = army;
					ScrollWindow.showWindow("army");
				}
			}
		});
		language = add(new MetaTypeAsset
		{
			id = "language",
			ranks = generateExponentialRanks(100.0, 1.5),
			window_name = "language",
			power_tab_id = "selected_language",
			force_zone_when_selected = true,
			set_icon_for_cancel_button = true,
			icon_list = "iconLanguageList",
			icon_single_path = "ui/icons/iconLanguage",
			window_action_clear = delegate
			{
				SelectedMetas.selected_language = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.language = SelectedMetas.selected_language;
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_language = pHistoryData.language;
			},
			reports = new string[4] { "happy", "unhappy", "many_children", "many_homeless" },
			get_list = () => World.world.languages,
			has_any = () => World.world.languages.hasAny(),
			get_selected = () => SelectedMetas.selected_language,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_language = pElement as Language;
			},
			get = (long pId) => World.world.languages.get(pId),
			map_mode = MetaType.Language,
			option_id = "map_language_layer",
			power_option_zone_id = "language_layer",
			has_dynamic_zones = true,
			click_action_zone = ActionLibrary.inspectLanguage,
			selected_tab_action_meta = defaultClickActionZone,
			check_unit_has_meta = (Actor pActor) => pActor.hasLanguage(),
			set_unit_set_meta_for_meta_for_window = delegate(Actor pActor)
			{
				SelectedMetas.selected_language = pActor.language;
			},
			draw_zones = delegate(MetaTypeAsset pMetaTypeAsset)
			{
				if (pMetaTypeAsset.isMetaZoneOptionSelectedFluid())
				{
					drawDefaultFluid(pMetaTypeAsset);
				}
				else
				{
					drawDefaultMeta(pMetaTypeAsset);
				}
			},
			check_cursor_highlight = delegate(MetaTypeAsset pMetaTypeAsset, WorldTile pTile, QuantumSpriteAsset pQAsset)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
				//IL_006c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00df: Unknown result type (might be due to invalid IL or missing references)
				Color color = pQAsset.color;
				switch (pMetaTypeAsset.getZoneOptionState())
				{
				case 0:
				{
					City city2 = pTile.zone.city;
					if (!city2.isRekt())
					{
						Language language = city2.kingdom.getLanguage();
						if (!language.isRekt())
						{
							{
								foreach (City city9 in city2.kingdom.getCities())
								{
									QuantumSpriteLibrary.colorZones(pQAsset, city9.zones, color);
								}
								break;
							}
						}
					}
					break;
				}
				case 1:
				{
					City city = pTile.zone.city;
					if (!city.isRekt())
					{
						Language language = city.getLanguage();
						if (!language.isRekt())
						{
							{
								foreach (City city10 in World.world.cities)
								{
									if (city10.getLanguage() == language)
									{
										QuantumSpriteLibrary.colorZones(pQAsset, city10.zones, color);
									}
								}
								break;
							}
						}
					}
					break;
				}
				default:
					highlightDefault(pTile, pQAsset, color);
					break;
				}
			},
			tile_get_metaobject = (TileZone pZone, int pZoneOption) => pZone.getLanguageOnZone(pZoneOption),
			tile_get_metaobject_0 = (TileZone pZone) => pZone.city?.kingdom.getLanguage(),
			tile_get_metaobject_1 = (TileZone pZone) => pZone.city?.getLanguage(),
			tile_get_metaobject_2 = (TileZone pZone) => ZoneMetaDataVisualizer.getZoneMetaData(pZone).meta_object,
			check_tile_has_meta = checkTileHasMetaDefault,
			check_cursor_tooltip = checkCursorTooltipDefault,
			cursor_tooltip_action = delegate(NanoObject pMeta)
			{
				Language language = pMeta as Language;
				if (!language.isRekt())
				{
					string text = "language";
					Tooltip.hideTooltip(language, pOnlySimObjects: true, text);
					Tooltip.show(language, text, new TooltipData
					{
						language = language,
						tooltip_scale = 0.7f,
						is_sim_tooltip = true
					});
				}
			},
			dynamic_zones = delegate
			{
				List<Actor> simpleList = World.world.units.getSimpleList();
				double curWorldTime = World.world.getCurWorldTime();
				int i = 0;
				for (int count = simpleList.Count; i < count; i++)
				{
					Actor actor = simpleList[i];
					if (actor.asset.show_on_meta_layer)
					{
						TileZone zone = actor.current_tile.zone;
						if (actor.hasLanguage())
						{
							ZoneMetaDataVisualizer.countMetaZone(zone, actor.language, curWorldTime);
						}
					}
				}
			},
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				Language pObject = World.world.languages.get(pMetaId);
				if (!pObject.isRekt())
				{
					Tooltip.show(pField, "language", new TooltipData
					{
						language = pObject
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				Language language = World.world.languages.get(pMetaId);
				if (!language.isRekt())
				{
					SelectedMetas.selected_language = language;
					ScrollWindow.showWindow("language");
				}
			}
		});
		subspecies = add(new MetaTypeAsset
		{
			id = "subspecies",
			ranks = generateExponentialRanks(100.0, 1.5),
			window_name = "subspecies",
			power_tab_id = "selected_subspecies",
			force_zone_when_selected = true,
			set_icon_for_cancel_button = true,
			unit_amount_alpha = true,
			icon_list = "iconSubspeciesList",
			icon_single_path = "ui/icons/iconSpecies",
			window_action_clear = delegate
			{
				SelectedMetas.selected_subspecies = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.subspecies = SelectedMetas.selected_subspecies;
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_subspecies = pHistoryData.subspecies;
			},
			reports = new string[4] { "happy", "unhappy", "many_children", "many_homeless" },
			get_list = () => World.world.subspecies,
			has_any = () => World.world.subspecies.hasAny(),
			get_selected = () => SelectedMetas.selected_subspecies,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_subspecies = pElement as Subspecies;
			},
			get = (long pId) => World.world.subspecies.get(pId),
			map_mode = MetaType.Subspecies,
			option_id = "map_subspecies_layer",
			power_option_zone_id = "subspecies_layer",
			has_dynamic_zones = true,
			click_action_zone = ActionLibrary.inspectSubspecies,
			selected_tab_action_meta = defaultClickActionZone,
			check_unit_has_meta = (Actor pActor) => pActor.hasSubspecies(),
			set_unit_set_meta_for_meta_for_window = delegate(Actor pActor)
			{
				SelectedMetas.selected_subspecies = pActor.subspecies;
			},
			draw_zones = delegate(MetaTypeAsset pMetaTypeAsset)
			{
				if (pMetaTypeAsset.isMetaZoneOptionSelectedFluid())
				{
					drawDefaultFluid(pMetaTypeAsset);
				}
				else
				{
					drawDefaultMeta(pMetaTypeAsset);
				}
			},
			check_cursor_highlight = delegate(MetaTypeAsset pMetaTypeAsset, WorldTile pTile, QuantumSpriteAsset pQAsset)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0108: Unknown result type (might be due to invalid IL or missing references)
				//IL_0075: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
				Color color = pQAsset.color;
				switch (pMetaTypeAsset.getZoneOptionState())
				{
				case 0:
				{
					City city2 = pTile.zone.city;
					if (!city2.isRekt())
					{
						Subspecies mainSubspecies = city2.kingdom.getMainSubspecies();
						if (!mainSubspecies.isRekt())
						{
							{
								foreach (City city11 in World.world.cities)
								{
									if (city11.getMainSubspecies() == mainSubspecies)
									{
										QuantumSpriteLibrary.colorZones(pQAsset, city11.zones, color);
									}
								}
								break;
							}
						}
					}
					break;
				}
				case 1:
				{
					City city = pTile.zone.city;
					if (!city.isRekt())
					{
						Subspecies mainSubspecies = city.getMainSubspecies();
						if (!mainSubspecies.isRekt())
						{
							{
								foreach (City city12 in World.world.cities)
								{
									if (city12.getMainSubspecies() == mainSubspecies)
									{
										QuantumSpriteLibrary.colorZones(pQAsset, city12.zones, color);
									}
								}
								break;
							}
						}
					}
					break;
				}
				default:
					highlightDefault(pTile, pQAsset, color);
					break;
				}
			},
			tile_get_metaobject = (TileZone pZone, int pZoneOption) => pZone.getSubspeciesOnZone(pZoneOption),
			tile_get_metaobject_0 = (TileZone pZone) => pZone.city?.kingdom.getMainSubspecies(),
			tile_get_metaobject_1 = (TileZone pZone) => pZone.city?.getMainSubspecies(),
			tile_get_metaobject_2 = (TileZone pZone) => ZoneMetaDataVisualizer.getZoneMetaData(pZone).meta_object,
			check_tile_has_meta = checkTileHasMetaDefault,
			check_cursor_tooltip = checkCursorTooltipDefault,
			cursor_tooltip_action = delegate(NanoObject pMeta)
			{
				Subspecies subspecies = pMeta as Subspecies;
				if (!subspecies.isRekt())
				{
					string text = "subspecies";
					Tooltip.hideTooltip(subspecies, pOnlySimObjects: true, text);
					Tooltip.show(subspecies, text, new TooltipData
					{
						subspecies = subspecies,
						tooltip_scale = 0.7f,
						is_sim_tooltip = true
					});
				}
			},
			dynamic_zones = delegate
			{
				List<Actor> simpleList = World.world.units.getSimpleList();
				double curWorldTime = World.world.getCurWorldTime();
				int i = 0;
				for (int count = simpleList.Count; i < count; i++)
				{
					Actor actor = simpleList[i];
					if (actor.asset.show_on_meta_layer)
					{
						TileZone zone = actor.current_tile.zone;
						if (actor.hasSubspecies())
						{
							ZoneMetaDataVisualizer.countMetaZone(zone, actor.subspecies, curWorldTime);
						}
					}
				}
			},
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				Subspecies pObject = World.world.subspecies.get(pMetaId);
				if (!pObject.isRekt())
				{
					Tooltip.show(pField, "subspecies", new TooltipData
					{
						subspecies = pObject
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				Subspecies subspecies = World.world.subspecies.get(pMetaId);
				if (!subspecies.isRekt())
				{
					SelectedMetas.selected_subspecies = subspecies;
					ScrollWindow.showWindow("subspecies");
				}
			}
		});
		city = add(new MetaTypeAsset
		{
			id = "city",
			ranks = generateExponentialRanks(100.0, 1.5),
			window_name = "city",
			power_tab_id = "selected_city",
			force_zone_when_selected = true,
			set_icon_for_cancel_button = true,
			icon_list = "iconCityList",
			icon_single_path = "ui/icons/iconCity",
			window_action_clear = delegate
			{
				SelectedMetas.selected_city = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.city = SelectedMetas.selected_city;
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_city = pHistoryData.city;
			},
			has_dynamic_zones = true,
			dynamic_zone_option = 1,
			reports = new string[11]
			{
				"happy", "unhappy", "food_none", "food_plenty", "food_running_out", "wood_none", "stone_none", "gold_none", "metal_none", "many_children",
				"many_homeless"
			},
			get_list = () => World.world.cities,
			has_any = () => World.world.cities.hasAny(),
			get_selected = () => SelectedMetas.selected_city,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_city = pElement as City;
			},
			get = (long pId) => World.world.cities.get(pId),
			map_mode = MetaType.City,
			option_id = "map_city_layer",
			power_option_zone_id = "city_layer",
			decision_ids = new string[9] { "give_tax", "store_resources", "make_items", "find_house", "try_to_take_city_item", "repair_equipment", "city_idle_walking", "replenish_energy", "put_out_fire" },
			click_action_zone = ActionLibrary.inspectCity,
			selected_tab_action_meta = defaultClickActionZone,
			check_unit_has_meta = (Actor pActor) => pActor.hasCity(),
			set_unit_set_meta_for_meta_for_window = delegate(Actor pActor)
			{
				SelectedMetas.selected_city = pActor.city;
			},
			draw_zones = delegate(MetaTypeAsset pMetaTypeAsset)
			{
				if (pMetaTypeAsset.isMetaZoneOptionSelectedFluid())
				{
					drawDefaultFluid(pMetaTypeAsset);
				}
				else
				{
					drawDefaultMeta(pMetaTypeAsset);
					drawForCities(pMetaTypeAsset, WildKingdomsManager.neutral.getCities(), getZoneDelegate(pMetaTypeAsset));
				}
			},
			dynamic_zones = delegate
			{
				List<Actor> simpleList = World.world.units.getSimpleList();
				double curWorldTime = World.world.getCurWorldTime();
				int i = 0;
				for (int count = simpleList.Count; i < count; i++)
				{
					Actor actor = simpleList[i];
					if (actor.asset.show_on_meta_layer)
					{
						TileZone zone = actor.current_tile.zone;
						if (actor.hasCity() && actor.isKingdomCiv())
						{
							ZoneMetaDataVisualizer.countMetaZone(zone, actor.city, curWorldTime);
						}
					}
				}
			},
			check_cursor_highlight = delegate(MetaTypeAsset pMetaTypeAsset, WorldTile pTile, QuantumSpriteAsset pQAsset)
			{
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				bool flag = PlayerConfig.optionBoolEnabled("highlight_kingdom_enemies");
				if (pMetaTypeAsset.getZoneOptionState() == 0)
				{
					if (!pTile.zone.city.isRekt())
					{
						QuantumSpriteLibrary.colorZones(pQAsset, pTile.zone.city.zones, pQAsset.color);
						if (flag)
						{
							QuantumSpriteLibrary.colorEnemies(pQAsset, pTile.zone.city.kingdom);
						}
					}
				}
				else
				{
					highlightDefault(pTile, pQAsset, pQAsset.color);
				}
			},
			tile_get_metaobject = (TileZone pZone, int pZoneOption) => pZone.getCityOnZone(pZoneOption),
			tile_get_metaobject_0 = (TileZone pZone) => pZone.city,
			tile_get_metaobject_1 = (TileZone pZone) => ZoneMetaDataVisualizer.getZoneMetaData(pZone).meta_object,
			tile_get_metaobject_2 = (TileZone pZone) => ZoneMetaDataVisualizer.getZoneMetaData(pZone).meta_object,
			check_tile_has_meta = (TileZone pZone, MetaTypeAsset pAsset, int pZoneOption) => pAsset.tile_get_metaobject(pZone, pZoneOption) != null,
			check_cursor_tooltip = checkCursorTooltipDefault,
			cursor_tooltip_action = delegate(NanoObject pMeta)
			{
				City city = pMeta as City;
				if (!city.isRekt())
				{
					string text = "city";
					Tooltip.hideTooltip(city, pOnlySimObjects: true, text);
					Tooltip.show(city, text, new TooltipData
					{
						city = city,
						tooltip_scale = 0.7f,
						is_sim_tooltip = true
					});
				}
			},
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				City pObject = World.world.cities.get(pMetaId);
				if (!pObject.isRekt())
				{
					Tooltip.show(pField, "city", new TooltipData
					{
						city = pObject
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				City city = World.world.cities.get(pMetaId);
				if (!city.isRekt())
				{
					SelectedMetas.selected_city = city;
					ScrollWindow.showWindow("city");
				}
			}
		});
		kingdom = add(new MetaTypeAsset
		{
			id = "kingdom",
			ranks = generateExponentialRanks(100.0, 1.5),
			window_name = "kingdom",
			power_tab_id = "selected_kingdom",
			force_zone_when_selected = true,
			set_icon_for_cancel_button = true,
			icon_list = "iconKingdomList",
			icon_single_path = "ui/icons/iconKingdom",
			window_action_clear = delegate
			{
				SelectedMetas.selected_kingdom = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.kingdom = SelectedMetas.selected_kingdom;
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_kingdom = pHistoryData.kingdom;
			},
			has_dynamic_zones = true,
			dynamic_zone_option = 1,
			reports = new string[4] { "happy", "unhappy", "many_children", "many_homeless" },
			get_list = () => World.world.kingdoms,
			has_any = () => World.world.kingdoms.hasAny(),
			get_selected = () => SelectedMetas.selected_kingdom,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_kingdom = pElement as Kingdom;
			},
			get = (long pId) => World.world.kingdoms.get(pId),
			map_mode = MetaType.Kingdom,
			option_id = "map_kingdom_layer",
			power_option_zone_id = "kingdom_layer",
			click_action_zone = ActionLibrary.inspectKingdom,
			selected_tab_action_meta = defaultClickActionZone,
			check_unit_has_meta = (Actor pActor) => pActor.isKingdomCiv(),
			set_unit_set_meta_for_meta_for_window = delegate(Actor pActor)
			{
				SelectedMetas.selected_kingdom = pActor.kingdom;
			},
			draw_zones = delegate(MetaTypeAsset pMetaTypeAsset)
			{
				if (pMetaTypeAsset.isMetaZoneOptionSelectedFluid())
				{
					drawDefaultFluid(pMetaTypeAsset);
				}
				else
				{
					drawDefaultMeta(pMetaTypeAsset);
					drawForCities(pMetaTypeAsset, WildKingdomsManager.neutral.getCities(), getZoneDelegate(pMetaTypeAsset));
				}
			},
			dynamic_zones = delegate
			{
				List<Actor> simpleList = World.world.units.getSimpleList();
				double curWorldTime = World.world.getCurWorldTime();
				int i = 0;
				for (int count = simpleList.Count; i < count; i++)
				{
					Actor actor = simpleList[i];
					if (actor.asset.show_on_meta_layer)
					{
						TileZone zone = actor.current_tile.zone;
						if (actor.hasKingdom() && actor.isKingdomCiv())
						{
							ZoneMetaDataVisualizer.countMetaZone(zone, actor.kingdom, curWorldTime);
						}
					}
				}
			},
			check_cursor_highlight = delegate(MetaTypeAsset pMetaTypeAsset, WorldTile pTile, QuantumSpriteAsset pQAsset)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_007f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0052: Unknown result type (might be due to invalid IL or missing references)
				bool flag = PlayerConfig.optionBoolEnabled("highlight_kingdom_enemies");
				Color color = pQAsset.color;
				if (pMetaTypeAsset.getZoneOptionState() == 0)
				{
					City city = pTile.zone.city;
					if (!city.isRekt())
					{
						foreach (City city13 in city.kingdom.getCities())
						{
							QuantumSpriteLibrary.colorZones(pQAsset, city13.zones, color);
						}
						if (flag)
						{
							QuantumSpriteLibrary.colorEnemies(pQAsset, city.kingdom);
						}
					}
				}
				else
				{
					highlightDefault(pTile, pQAsset, color);
				}
			},
			tile_get_metaobject = delegate(TileZone pZone, int pZoneOption)
			{
				IMetaObject kingdomOnZone = pZone.getKingdomOnZone(pZoneOption);
				if (kingdomOnZone == null)
				{
					return (IMetaObject)null;
				}
				return ((Kingdom)kingdomOnZone).isNeutral() ? null : kingdomOnZone;
			},
			tile_get_metaobject_0 = (TileZone pZone) => pZone.city?.kingdom,
			tile_get_metaobject_1 = (TileZone pZone) => ZoneMetaDataVisualizer.getZoneMetaData(pZone).meta_object,
			tile_get_metaobject_2 = (TileZone pZone) => ZoneMetaDataVisualizer.getZoneMetaData(pZone).meta_object,
			check_tile_has_meta = delegate(TileZone pZone, MetaTypeAsset pAsset, int pZoneOption)
			{
				IMetaObject metaObject = pAsset.tile_get_metaobject(pZone, pZoneOption);
				if (metaObject == null)
				{
					return false;
				}
				return !((Kingdom)metaObject).isNeutral();
			},
			check_cursor_tooltip = checkCursorTooltipDefault,
			cursor_tooltip_action = delegate(NanoObject pMeta)
			{
				Kingdom kingdom = pMeta as Kingdom;
				if (!kingdom.isRekt())
				{
					string text = "kingdom";
					Tooltip.hideTooltip(kingdom, pOnlySimObjects: true, text);
					Tooltip.show(kingdom, text, new TooltipData
					{
						kingdom = kingdom,
						tooltip_scale = 0.7f,
						is_sim_tooltip = true
					});
				}
			},
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				Kingdom kingdom = World.world.kingdoms.get(pMetaId);
				if (!kingdom.isRekt() && !kingdom.isNeutral())
				{
					Tooltip.show(pField, "kingdom", new TooltipData
					{
						kingdom = kingdom
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				Kingdom kingdom = World.world.kingdoms.get(pMetaId);
				if (!kingdom.isRekt() && !kingdom.isNeutral())
				{
					SelectedMetas.selected_kingdom = kingdom;
					ScrollWindow.showWindow("kingdom");
				}
			}
		});
		clan = add(new MetaTypeAsset
		{
			id = "clan",
			ranks = generateExponentialRanks(100.0, 1.5),
			window_name = "clan",
			power_tab_id = "selected_clan",
			force_zone_when_selected = true,
			set_icon_for_cancel_button = true,
			icon_list = "iconClanList",
			icon_single_path = "ui/icons/iconClan",
			window_action_clear = delegate
			{
				SelectedMetas.selected_clan = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.clan = SelectedMetas.selected_clan;
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_clan = pHistoryData.clan;
			},
			reports = new string[4] { "happy", "unhappy", "many_children", "many_homeless" },
			get_list = () => World.world.clans,
			has_any = () => World.world.clans.hasAny(),
			get_selected = () => SelectedMetas.selected_clan,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_clan = pElement as Clan;
			},
			get = (long pId) => World.world.clans.get(pId),
			map_mode = MetaType.Clan,
			option_id = "map_clan_layer",
			power_option_zone_id = "clan_layer",
			has_dynamic_zones = true,
			dynamic_zone_option = 2,
			click_action_zone = ActionLibrary.inspectClan,
			selected_tab_action_meta = defaultClickActionZone,
			check_unit_has_meta = (Actor pActor) => pActor.hasClan(),
			set_unit_set_meta_for_meta_for_window = delegate(Actor pActor)
			{
				SelectedMetas.selected_clan = pActor.clan;
			},
			decision_ids = new string[1] { "try_new_plot" },
			draw_zones = delegate(MetaTypeAsset pMetaTypeAsset)
			{
				if (pMetaTypeAsset.isMetaZoneOptionSelectedFluid())
				{
					drawDefaultFluid(pMetaTypeAsset);
				}
				else
				{
					drawDefaultMeta(pMetaTypeAsset);
				}
			},
			check_cursor_highlight = delegate(MetaTypeAsset pMetaTypeAsset, WorldTile pTile, QuantumSpriteAsset pQAsset)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
				//IL_006c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00df: Unknown result type (might be due to invalid IL or missing references)
				Color color = pQAsset.color;
				switch (pMetaTypeAsset.getZoneOptionState())
				{
				case 0:
				{
					City city2 = pTile.zone.city;
					if (!city2.isRekt())
					{
						Clan royalClan = city2.kingdom.getKingClan();
						if (!royalClan.isRekt())
						{
							{
								foreach (City city14 in city2.kingdom.getCities())
								{
									QuantumSpriteLibrary.colorZones(pQAsset, city14.zones, color);
								}
								break;
							}
						}
					}
					break;
				}
				case 1:
				{
					City city = pTile.zone.city;
					if (!city.isRekt())
					{
						Clan royalClan = city.getRoyalClan();
						if (!royalClan.isRekt())
						{
							{
								foreach (City city15 in World.world.cities)
								{
									if (city15.getRoyalClan() == royalClan)
									{
										QuantumSpriteLibrary.colorZones(pQAsset, city15.zones, color);
									}
								}
								break;
							}
						}
					}
					break;
				}
				default:
					highlightDefault(pTile, pQAsset, color);
					break;
				}
			},
			tile_get_metaobject = (TileZone pZone, int pZoneOption) => pZone.getClanOnZone(pZoneOption),
			tile_get_metaobject_0 = (TileZone pZone) => pZone.city?.kingdom.getKingClan(),
			tile_get_metaobject_1 = (TileZone pZone) => pZone.city?.getRoyalClan(),
			tile_get_metaobject_2 = (TileZone pZone) => ZoneMetaDataVisualizer.getZoneMetaData(pZone).meta_object,
			check_tile_has_meta = checkTileHasMetaDefault,
			check_cursor_tooltip = checkCursorTooltipDefault,
			cursor_tooltip_action = delegate(NanoObject pMeta)
			{
				Clan clan = pMeta as Clan;
				if (!clan.isRekt())
				{
					string text = "clan";
					Tooltip.hideTooltip(clan, pOnlySimObjects: true, text);
					Tooltip.show(clan, text, new TooltipData
					{
						clan = clan,
						tooltip_scale = 0.7f,
						is_sim_tooltip = true
					});
				}
			},
			dynamic_zones = delegate
			{
				List<Actor> simpleList = World.world.units.getSimpleList();
				double curWorldTime = World.world.getCurWorldTime();
				int i = 0;
				for (int count = simpleList.Count; i < count; i++)
				{
					Actor actor = simpleList[i];
					if (actor.asset.show_on_meta_layer)
					{
						TileZone zone = actor.current_tile.zone;
						if (actor.hasClan())
						{
							ZoneMetaDataVisualizer.countMetaZone(zone, actor.clan, curWorldTime);
						}
					}
				}
			},
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				Clan pObject = World.world.clans.get(pMetaId);
				if (!pObject.isRekt())
				{
					Tooltip.show(pField, "clan", new TooltipData
					{
						clan = pObject
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				Clan clan = World.world.clans.get(pMetaId);
				if (!clan.isRekt())
				{
					SelectedMetas.selected_clan = clan;
					ScrollWindow.showWindow("clan");
				}
			}
		});
		alliance = add(new MetaTypeAsset
		{
			id = "alliance",
			window_name = "alliance",
			power_tab_id = "selected_alliance",
			force_zone_when_selected = true,
			set_icon_for_cancel_button = true,
			icon_list = "iconAllianceList",
			icon_single_path = "ui/icons/iconAlliance",
			window_action_clear = delegate
			{
				SelectedMetas.selected_alliance = null;
			},
			window_history_action_update = delegate(ref WindowHistoryData pHistoryData)
			{
				pHistoryData.alliance = SelectedMetas.selected_alliance;
			},
			window_history_action_restore = delegate(ref WindowHistoryData pHistoryData)
			{
				SelectedMetas.selected_alliance = pHistoryData.alliance;
			},
			reports = new string[4] { "happy", "unhappy", "many_children", "many_homeless" },
			get_list = () => World.world.alliances,
			has_any = () => World.world.alliances.hasAny(),
			get_selected = () => SelectedMetas.selected_alliance,
			set_selected = delegate(NanoObject pElement)
			{
				SelectedMetas.selected_alliance = pElement as Alliance;
			},
			get = (long pId) => World.world.alliances.get(pId),
			map_mode = MetaType.Alliance,
			option_id = "map_alliance_layer",
			power_option_zone_id = "alliance_layer",
			click_action_zone = ActionLibrary.inspectAlliance,
			selected_tab_action_meta = defaultClickActionZone,
			check_unit_has_meta = (Actor pActor) => pActor.kingdom.hasAlliance(),
			set_unit_set_meta_for_meta_for_window = delegate(Actor pActor)
			{
				SelectedMetas.selected_alliance = pActor.kingdom.getAlliance();
			},
			draw_zones = delegate(MetaTypeAsset pMetaTypeAsset)
			{
				int zoneOptionState = pMetaTypeAsset.getZoneOptionState();
				foreach (Alliance alliance2 in World.world.alliances)
				{
					foreach (Kingdom item3 in alliance2.kingdoms_hashset)
					{
						foreach (City city16 in item3.getCities())
						{
							foreach (TileZone zone2 in city16.zones)
							{
								zone_manager.drawBegin();
								zone_manager.drawZoneAlliance(zone2, zoneOptionState);
								zone_manager.drawEnd(zone2);
							}
						}
					}
				}
				foreach (Kingdom kingdom2 in World.world.kingdoms)
				{
					if (!kingdom2.hasAlliance())
					{
						foreach (City city17 in kingdom2.getCities())
						{
							foreach (TileZone zone3 in city17.zones)
							{
								zone_manager.drawBegin();
								zone_manager.drawZoneCity(zone3);
								zone_manager.drawEnd(zone3);
							}
						}
					}
				}
			},
			check_cursor_highlight = delegate(MetaTypeAsset pMetaTypeAsset, WorldTile pTile, QuantumSpriteAsset pQAsset)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
				//IL_0070: Unknown result type (might be due to invalid IL or missing references)
				bool flag = PlayerConfig.optionBoolEnabled("highlight_kingdom_enemies");
				Color color = pQAsset.color;
				City city = pTile.zone.city;
				if (!city.isRekt())
				{
					Kingdom kingdom = city.kingdom;
					if (kingdom.hasAlliance())
					{
						foreach (Kingdom item4 in kingdom.getAlliance().kingdoms_hashset)
						{
							foreach (City city18 in item4.getCities())
							{
								QuantumSpriteLibrary.colorZones(pQAsset, city18.zones, color);
							}
						}
					}
					else
					{
						foreach (City city19 in city.kingdom.getCities())
						{
							QuantumSpriteLibrary.colorZones(pQAsset, city19.zones, color);
						}
					}
					if (flag)
					{
						QuantumSpriteLibrary.colorEnemies(pQAsset, kingdom);
					}
				}
			},
			check_tile_has_meta = delegate(TileZone pZone, MetaTypeAsset pAsset, int pZoneOption)
			{
				City city = pZone.city;
				if (city.isRekt())
				{
					return false;
				}
				return !city.kingdom.getAlliance().isRekt();
			},
			check_cursor_tooltip = delegate(TileZone pZone, MetaTypeAsset pAsset, int pZoneOption)
			{
				City city = pZone.city;
				if (city.isRekt())
				{
					return false;
				}
				Alliance alliance = city.kingdom.getAlliance();
				if (alliance.isRekt())
				{
					return kingdom.check_cursor_tooltip(pZone, kingdom, pZoneOption);
				}
				alliance.meta_type_asset.cursor_tooltip_action(alliance);
				return true;
			},
			cursor_tooltip_action = delegate(NanoObject pMeta)
			{
				Alliance alliance = pMeta as Alliance;
				if (!alliance.isRekt())
				{
					string text = "alliance";
					Tooltip.hideTooltip(alliance, pOnlySimObjects: true, text);
					Tooltip.show(alliance, text, new TooltipData
					{
						alliance = alliance,
						tooltip_scale = 0.7f,
						is_sim_tooltip = true
					});
				}
			},
			stat_hover = delegate(long pMetaId, MonoBehaviour pField)
			{
				Alliance pObject = World.world.alliances.get(pMetaId);
				if (!pObject.isRekt())
				{
					Tooltip.show(pField, "alliance", new TooltipData
					{
						alliance = pObject
					});
				}
			},
			stat_click = delegate(long pMetaId, MonoBehaviour _)
			{
				Alliance alliance = World.world.alliances.get(pMetaId);
				if (!alliance.isRekt())
				{
					SelectedMetas.selected_alliance = alliance;
					ScrollWindow.showWindow("alliance");
				}
			}
		});
	}

	private MetaZoneGetMetaSimple getZoneDelegate(MetaTypeAsset pMetaTypeAsset)
	{
		return pMetaTypeAsset.getZoneOptionState() switch
		{
			0 => pMetaTypeAsset.tile_get_metaobject_0, 
			1 => pMetaTypeAsset.tile_get_metaobject_1, 
			2 => pMetaTypeAsset.tile_get_metaobject_2, 
			_ => pMetaTypeAsset.tile_get_metaobject_2, 
		};
	}

	private void drawDefaultFluid(MetaTypeAsset pMetaTypeAsset)
	{
		foreach (ZoneMetaData value in ZoneMetaDataVisualizer.zone_data_dict.Values)
		{
			if (value.meta_object != null && value.meta_object.isAlive())
			{
				zone_manager.drawBegin();
				zone_manager.drawGenericFluid(value, pMetaTypeAsset);
				zone_manager.drawEnd(value.zone);
			}
		}
	}

	private void drawDefaultMeta(MetaTypeAsset pMetaTypeAsset)
	{
		MetaZoneGetMetaSimple zoneDelegate = getZoneDelegate(pMetaTypeAsset);
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			drawForCities(pMetaTypeAsset, kingdom.getCities(), zoneDelegate);
		}
	}

	private void drawForCities(MetaTypeAsset pMetaTypeAsset, IEnumerable<City> pListCities, MetaZoneGetMetaSimple pZoneGetDelegate)
	{
		foreach (City pListCity in pListCities)
		{
			drawZonesForMeta(pMetaTypeAsset, pListCity.zones, pZoneGetDelegate);
		}
	}

	private void drawZonesForMeta(MetaTypeAsset pMetaTypeAsset, List<TileZone> pZones, MetaZoneGetMetaSimple pZoneGetDelegate)
	{
		foreach (TileZone pZone in pZones)
		{
			zone_manager.drawBegin();
			zone_manager.drawZoneMeta(pZone, pMetaTypeAsset, pZoneGetDelegate);
			zone_manager.drawEnd(pZone);
		}
	}

	private void defaultClickActionZone(MetaTypeAsset pMetaTypeAsset)
	{
		PowerTabController.showTabSelectedMeta(pMetaTypeAsset);
	}

	private bool checkCursorTooltipDefault(TileZone pTile, MetaTypeAsset pAsset, int pZoneOption)
	{
		IMetaObject metaObject = pAsset.tile_get_metaobject(pTile, pZoneOption);
		if (metaObject == null)
		{
			return false;
		}
		pAsset.cursor_tooltip_action(metaObject as NanoObject);
		return true;
	}

	private bool checkTileHasMetaDefault(TileZone pZone, MetaTypeAsset pAsset, int pZoneOption)
	{
		return pAsset.tile_get_metaobject(pZone, pZoneOption) != null;
	}

	private void highlightDefault(WorldTile pTile, QuantumSpriteAsset pQAsset, Color pColorAnimated)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		ZoneMetaData zoneMetaData = ZoneMetaDataVisualizer.getZoneMetaData(pTile.zone);
		if (zoneMetaData.meta_object == null || !zoneMetaData.meta_object.isAlive())
		{
			return;
		}
		using ListPool<TileZone> pZones = ZoneMetaDataVisualizer.getZonesWithMeta(zoneMetaData.meta_object);
		QuantumSpriteLibrary.colorZones(pQAsset, pZones, pColorAnimated);
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (MetaTypeAsset item in list)
		{
			if (item.decision_ids != null)
			{
				item.decisions_assets = new DecisionAsset[item.decision_ids.Length];
				for (int i = 0; i < item.decision_ids.Length; i++)
				{
					string pID = item.decision_ids[i];
					DecisionAsset decisionAsset = AssetManager.decisions_library.get(pID);
					item.decisions_assets[i] = decisionAsset;
				}
			}
			if (!string.IsNullOrEmpty(item.option_id))
			{
				item.option_asset = AssetManager.options_library.get(item.option_id);
			}
		}
	}

	public override void editorDiagnostic()
	{
		base.editorDiagnostic();
	}

	public static int[] generateExponentialRanks(double pBasePoints, double pGrowthFactor)
	{
		int[] array = new int[10];
		double num = pBasePoints;
		for (int i = 1; i <= 10; i++)
		{
			array[i - 1] = roundToNiceNumber(num);
			num += pBasePoints * Math.Pow(pGrowthFactor, i - 1);
		}
		return array;
	}

	private static int roundToNiceNumber(double value)
	{
		if (value < 1000.0)
		{
			return (int)(Math.Round(value / 100.0) * 100.0);
		}
		return (int)(Math.Round(value / 500.0) * 500.0);
	}

	public MetaTypeAsset getAsset(MetaType pType)
	{
		return get(pType.AsString());
	}

	public MetaTypeAsset getFromPower(string pPower)
	{
		GodPower pPower2 = AssetManager.powers.get(pPower);
		return getFromPower(pPower2);
	}

	public MetaTypeAsset getFromPower(GodPower pPower)
	{
		foreach (MetaTypeAsset item in list)
		{
			if (item.power_option_zone_id == pPower.id)
			{
				return item;
			}
		}
		return null;
	}

	public void debug(DebugTool pTool)
	{
		foreach (MetaTypeAsset item in AssetManager.meta_type_library.list)
		{
			NanoObject nanoObject = item.get_selected();
			if (!nanoObject.isRekt())
			{
				pTool.setText(item.id + ":", nanoObject.getTypeID(), 0f, pShowBar: false, 0L);
			}
			else
			{
				pTool.setText(item.id + ":", "-", 0f, pShowBar: false, 0L);
			}
		}
	}
}
