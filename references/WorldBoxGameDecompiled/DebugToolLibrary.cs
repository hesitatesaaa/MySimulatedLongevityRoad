using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityPools;
using life.taxi;

public class DebugToolLibrary : AssetLibrary<DebugToolAsset>
{
	private UtilityBasedDecisionSystem _decision_system_debug;

	public override void init()
	{
		base.init();
		initBenchmarks();
		initMain();
		initGameplay();
		initMap();
		initAI();
		initCity();
		initSystems();
		initSubSystems();
		initFmod();
		initDiagnosticGameplay();
		initUI();
		initDebugConfigDefaults();
	}

	private void initDebugConfigDefaults()
	{
		foreach (string default_debug_tool in DebugConfig.default_debug_tools)
		{
			DebugToolAsset debugToolAsset = get(default_debug_tool);
			if (debugToolAsset != null)
			{
				debugToolAsset.show_on_start = true;
			}
		}
	}

	private void initDiagnosticGameplay()
	{
		add(new DebugToolAsset
		{
			id = "hotkeys_nanoobjects",
			action_1 = delegate(DebugTool pTool)
			{
				HotkeyTabsData hotkey_tabs_data = World.world.hotkey_tabs_data;
				Dictionary<string, PlayerOptionData> dictionary = PlayerConfig.dict;
				pTool.setText("#map:", "-------------", 0f, pShowBar: false, 0L);
				pTool.setText("hotkey_data_1:", hotkey_tabs_data.hotkey_data_1, 0f, pShowBar: false, 0L);
				pTool.setText("hotkey_data_2:", hotkey_tabs_data.hotkey_data_2, 0f, pShowBar: false, 0L);
				pTool.setText("hotkey_data_3:", hotkey_tabs_data.hotkey_data_3, 0f, pShowBar: false, 0L);
				pTool.setText("hotkey_data_4:", hotkey_tabs_data.hotkey_data_4, 0f, pShowBar: false, 0L);
				pTool.setText("hotkey_data_5:", hotkey_tabs_data.hotkey_data_5, 0f, pShowBar: false, 0L);
				pTool.setText("hotkey_data_6:", hotkey_tabs_data.hotkey_data_6, 0f, pShowBar: false, 0L);
				pTool.setText("hotkey_data_7:", hotkey_tabs_data.hotkey_data_7, 0f, pShowBar: false, 0L);
				pTool.setText("hotkey_data_8:", hotkey_tabs_data.hotkey_data_8, 0f, pShowBar: false, 0L);
				pTool.setText("hotkey_data_9:", hotkey_tabs_data.hotkey_data_9, 0f, pShowBar: false, 0L);
				pTool.setText("hotkey_data_0:", hotkey_tabs_data.hotkey_data_0, 0f, pShowBar: false, 0L);
				pTool.setText("#global_config:", "-------------", 0f, pShowBar: false, 0L);
				pTool.setText("global_hotkey_1:", dictionary["hotkey_1"].stringVal, 0f, pShowBar: false, 0L);
				pTool.setText("global_hotkey_2:", dictionary["hotkey_2"].stringVal, 0f, pShowBar: false, 0L);
				pTool.setText("global_hotkey_3:", dictionary["hotkey_3"].stringVal, 0f, pShowBar: false, 0L);
				pTool.setText("global_hotkey_4:", dictionary["hotkey_4"].stringVal, 0f, pShowBar: false, 0L);
				pTool.setText("global_hotkey_5:", dictionary["hotkey_5"].stringVal, 0f, pShowBar: false, 0L);
				pTool.setText("global_hotkey_6:", dictionary["hotkey_6"].stringVal, 0f, pShowBar: false, 0L);
				pTool.setText("global_hotkey_7:", dictionary["hotkey_7"].stringVal, 0f, pShowBar: false, 0L);
				pTool.setText("global_hotkey_8:", dictionary["hotkey_8"].stringVal, 0f, pShowBar: false, 0L);
				pTool.setText("global_hotkey_9:", dictionary["hotkey_9"].stringVal, 0f, pShowBar: false, 0L);
				pTool.setText("global_hotkey_0:", dictionary["hotkey_0"].stringVal, 0f, pShowBar: false, 0L);
			}
		});
		add(new DebugToolAsset
		{
			id = "reproduction_diagnostic_cursor",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					if (Zones.showCityZones())
					{
						City city = mouseTilePos.zone.city;
						if (city != null)
						{
							Subspecies mainSubspecies = city.getMainSubspecies();
							if (mainSubspecies != null)
							{
								showReproductionDebugInfo(pTool, mainSubspecies);
							}
						}
					}
					else if (Zones.showKingdomZones())
					{
						City city2 = mouseTilePos.zone.city;
						if (city2 != null)
						{
							Subspecies mainSubspecies2 = city2.kingdom.getMainSubspecies();
							if (mainSubspecies2 != null)
							{
								showReproductionDebugInfo(pTool, mainSubspecies2);
							}
						}
					}
					else if (Zones.showSpeciesZones())
					{
						ZoneMetaData zoneMetaData = ZoneMetaDataVisualizer.getZoneMetaData(mouseTilePos.zone);
						if (zoneMetaData.meta_object != null && zoneMetaData.meta_object.isAlive())
						{
							showReproductionDebugInfo(pTool, zoneMetaData.meta_object as Subspecies);
						}
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "reproduction_diagnostic_total",
			action_1 = delegate(DebugTool pTool)
			{
				Dictionary<string, int> dictionary = UnsafeCollectionPool<Dictionary<string, int>, KeyValuePair<string, int>>.Get();
				Dictionary<string, int> dictionary2 = UnsafeCollectionPool<Dictionary<string, int>, KeyValuePair<string, int>>.Get();
				foreach (Subspecies subspecy in World.world.subspecies)
				{
					foreach (RateCounter list_counter in subspecy.list_counters)
					{
						dictionary[list_counter.id] = dictionary.GetValueOrDefault(list_counter.id) + list_counter.getTotal();
						dictionary2[list_counter.id] = dictionary2.GetValueOrDefault(list_counter.id) + list_counter.getEventsPerMinute();
					}
				}
				foreach (KeyValuePair<string, int> item in dictionary)
				{
					pTool.setText(item.Key + ":", $"{dictionary2[item.Key]} | tot: {item.Value}", 0f, pShowBar: false, 0L);
				}
				UnsafeCollectionPool<Dictionary<string, int>, KeyValuePair<string, int>>.Release(dictionary);
				UnsafeCollectionPool<Dictionary<string, int>, KeyValuePair<string, int>>.Release(dictionary2);
			}
		});
	}

	private void showReproductionDebugInfo(DebugTool pTool, Subspecies pSubspecies)
	{
		pSubspecies.debugReproductionEvents(pTool);
	}

	private void initCity()
	{
		add(new DebugToolAsset
		{
			id = "Cities",
			action_1 = delegate(DebugTool pTool)
			{
				//IL_012d: Unknown result type (might be due to invalid IL or missing references)
				List<City> obj = new List<City>(World.world.cities);
				obj.Sort(pTool.citySorter);
				foreach (City item in obj)
				{
					if (pTool.textCount > 0)
					{
						pTool.setSeparator();
					}
					pTool.setText("#name:", item.name, 0f, pShowBar: false, 0L);
					pTool.setText("pep:", item.getPopulationPeople(), 0f, pShowBar: false, 0L);
					pTool.setText("units:", item.getUnitsTotal(), 0f, pShowBar: false, 0L);
					pTool.setText("boats:", item.countBoats(), 0f, pShowBar: false, 0L);
					pTool.setText("zones:", item.zones.Count, 0f, pShowBar: false, 0L);
					pTool.setText("buildings:", item.buildings.Count, 0f, pShowBar: false, 0L);
					pTool.setText("city_center:", item.city_center, 0f, pShowBar: false, 0L);
					if (pTool.textCount > 30)
					{
						pTool.setSeparator();
						pTool.setText("more...", "...", 0f, pShowBar: false, 0L);
						break;
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "City Loyalty",
			action_1 = delegate(DebugTool pTool)
			{
				List<City> obj = new List<City>(World.world.cities);
				obj.Sort(pTool.citySorter);
				int num = 0;
				int num2 = 0;
				foreach (City item2 in obj)
				{
					if (item2.getCachedLoyalty() >= 0)
					{
						num++;
					}
					else
					{
						num2++;
					}
				}
				pTool.setText("cities with loyalty above 0:", num, 0f, pShowBar: false, 0L);
				pTool.setText("cities with loyalty below 0:", num2, 0f, pShowBar: false, 0L);
			}
		});
		add(new DebugToolAsset
		{
			id = "City Capture",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					City city = mouseTilePos.zone.city;
					if (city != null)
					{
						if (city.being_captured_by != null && city.being_captured_by.isAlive())
						{
							pTool.setText("capturing by:", city.being_captured_by.name, 0f, pShowBar: false, 0L);
						}
						pTool.setText("ticks:", city.getCaptureTicks(), 0f, pShowBar: false, 0L);
						city.debugCaptureUnits(pTool);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "City Tasks",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					City city = mouseTilePos.zone.city;
					if (city != null)
					{
						pTool.setText("trees:", city.tasks.trees, 0f, pShowBar: false, 0L);
						pTool.setText("stone:", city.tasks.minerals, 0f, pShowBar: false, 0L);
						pTool.setText("minerals:", city.tasks.minerals, 0f, pShowBar: false, 0L);
						pTool.setText("bushes:", city.tasks.bushes, 0f, pShowBar: false, 0L);
						pTool.setText("plants:", city.tasks.plants, 0f, pShowBar: false, 0L);
						pTool.setText("hives:", city.tasks.hives, 0f, pShowBar: false, 0L);
						pTool.setText("farm_fields:", city.tasks.farm_fields, 0f, pShowBar: false, 0L);
						pTool.setText("wheats:", city.tasks.wheats, 0f, pShowBar: false, 0L);
						pTool.setText("ruins:", city.tasks.ruins, 0f, pShowBar: false, 0L);
						pTool.setText("poops:", city.tasks.poops, 0f, pShowBar: false, 0L);
						pTool.setText("roads:", city.tasks.roads, 0f, pShowBar: false, 0L);
						pTool.setText("fire:", city.tasks.fire, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "city_jobs",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					City city = mouseTilePos.zone.city;
					if (city != null)
					{
						int num = 0;
						int num2 = 0;
						foreach (CitizenJobAsset key in city.jobs.jobs.Keys)
						{
							int num3 = city.jobs.jobs[key];
							int num4 = 0;
							if (city.jobs.occupied.ContainsKey(key))
							{
								num4 = city.jobs.occupied[key];
							}
							num += num3;
							num2 += num4;
							pTool.setText(key.id + ":", num4 + "/" + num3, 0f, pShowBar: false, 0L);
						}
						foreach (CitizenJobAsset key2 in city.jobs.occupied.Keys)
						{
							if (!city.jobs.jobs.ContainsKey(key2))
							{
								int num5 = city.jobs.occupied[key2];
								num2 += num5;
								pTool.setText(key2.id + ":", num5 + "/" + 0, 0f, pShowBar: false, 0L);
							}
						}
						pTool.setSeparator();
						pTool.setText("total JOBS:", num2 + "/" + num, 0f, pShowBar: false, 0L);
						pTool.setSeparator();
						pTool.setText("pop:", city.getPopulationPeople() + " / " + city.getPopulationMaximum(), 0f, pShowBar: false, 0L);
						pTool.setText("adults/children:", city.countAdults() + "/" + city.countChildren(), 0f, pShowBar: false, 0L);
						pTool.setText("food:", city.countFood(), 0f, pShowBar: false, 0L);
						pTool.setText("hungry/starving:", city.countHungry() + "/" + city.countStarving(), 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "City Info",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					City city = mouseTilePos.zone.city;
					if (city != null)
					{
						pTool.setText("#name:", city.name, 0f, pShowBar: false, 0L);
						pTool.setText("city units all:", city.getUnitsTotal(), 0f, pShowBar: false, 0L);
						pTool.setText("city people:", city.getPopulationPeople(), 0f, pShowBar: false, 0L);
						pTool.setText("units:", city.getPopulationPeople() + "/" + city.getPopulationMaximum(), 0f, pShowBar: false, 0L);
						if (city.getPopulationMaximum() != city.status.housing_total)
						{
							pTool.setText("unit housing:", city.getPopulationPeople() + "/" + city.status.housing_total, 0f, pShowBar: false, 0L);
						}
						pTool.setText("in houses:", city.countInHouses(), 0f, pShowBar: false, 0L);
						pTool.setSeparator();
						if (city.hasLeader())
						{
							pTool.setText("leader:", city.leader.getName(), 0f, pShowBar: false, 0L);
						}
						if (city.hasKingdom())
						{
							pTool.setText("kingdom:", city.kingdom.name, 0f, pShowBar: false, 0L);
						}
						if (city.hasKingdom())
						{
							pTool.setText("#name:", city.kingdom.id, 0f, pShowBar: false, 0L);
						}
						pTool.setSeparator();
						pTool.setText("zones:", city.zones.Count, 0f, pShowBar: false, 0L);
						pTool.setText("buildings:", city.buildings.Count, 0f, pShowBar: false, 0L);
						pTool.setText("homes free:", city.status.housing_free, 0f, pShowBar: false, 0L);
						pTool.setText("homes occupied:", city.status.housing_occupied, 0f, pShowBar: false, 0L);
						pTool.setSeparator();
						pTool.setSeparator();
						pTool.setText("roads to build:", city.road_tiles_to_build.Count, 0f, pShowBar: false, 0L);
						pTool.setSeparator();
						pTool.setSeparator();
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "city_storage",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					City city = mouseTilePos.zone.city;
					if (city != null && city.hasStorages())
					{
						for (int i = 0; i < city.storages.Count; i++)
						{
							foreach (string key3 in city.storages[i].resources.getKeys())
							{
								pTool.setText("stock_" + i + ":" + key3 + ":", city.getResourcesAmount(key3), 0f, pShowBar: false, 0L);
							}
						}
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "City Buildings",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					City city = mouseTilePos.zone.city;
					if (city != null)
					{
						pTool.setSeparator();
						int num = 0;
						int num2 = 0;
						pTool.setText("#type", "", 0f, pShowBar: false, 0L);
						foreach (string key4 in city.buildings_dict_type.Keys)
						{
							pTool.setText(key4 + ":", city.buildings_dict_type[key4].Count, 0f, pShowBar: false, 0L);
							num += city.buildings_dict_type[key4].Count;
						}
						pTool.setSeparator();
						pTool.setText("#name", "", 0f, pShowBar: false, 0L);
						foreach (string key5 in city.buildings_dict_id.Keys)
						{
							pTool.setText(key5 + ":", city.buildings_dict_id[key5].Count, 0f, pShowBar: false, 0L);
							num2 += city.buildings_dict_id[key5].Count;
						}
						pTool.setSeparator();
						pTool.setText("total:", city.buildings.Count, 0f, pShowBar: false, 0L);
						pTool.setText("total by type:", num, 0f, pShowBar: false, 0L);
						pTool.setText("total by name:", num2, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "City Professions",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					City city = mouseTilePos.zone.city;
					if (city != null)
					{
						pTool.setSeparator();
						pTool.setText("total:", city.units.Count, 0f, pShowBar: false, 0L);
						pTool.setText("king:", city.countProfession(UnitProfession.King), 0f, pShowBar: false, 0L);
						pTool.setText("leader:", city.countProfession(UnitProfession.Leader), 0f, pShowBar: false, 0L);
						pTool.setText("units:", city.countProfession(UnitProfession.Unit), 0f, pShowBar: false, 0L);
						pTool.setText("babies:", city.countChildren(), 0f, pShowBar: false, 0L);
						pTool.setText("warriors:", city.countProfession(UnitProfession.Warrior), 0f, pShowBar: false, 0L);
						pTool.setText("null:", city.countProfession(UnitProfession.Nothing), 0f, pShowBar: false, 0L);
					}
				}
			}
		});
	}

	private void initSystems()
	{
		add(new DebugToolAsset
		{
			id = "Effects",
			action_1 = delegate(DebugTool pTool)
			{
				ExplosionChecker.debug(pTool);
				foreach (BaseEffectController item in World.world.stack_effects.list)
				{
					item.debug(pTool);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Auto Tester",
			action_1 = delegate(DebugTool pTool)
			{
				if ((Object)(object)World.world.auto_tester != (Object)null)
				{
					pTool.setText("active:", World.world.auto_tester.active, 0f, pShowBar: false, 0L);
					pTool.setText("d_string:", World.world.auto_tester.debugString, 0f, pShowBar: false, 0L);
					World.world.auto_tester.ai.debug(pTool);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Controllable Units",
			action_1 = delegate(DebugTool pTool)
			{
				//IL_0160: Unknown result type (might be due to invalid IL or missing references)
				pTool.setText("isOverUI:", World.world.isOverUI(), 0f, pShowBar: false, 0L);
				pTool.setText("isGameplayControlsLocked:", World.world.isGameplayControlsLocked(), 0f, pShowBar: false, 0L);
				pTool.setText("controlsLocked:", MapBox.controlsLocked(), 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("IsselectedUnit:", SelectedUnit.isSet(), 0f, pShowBar: false, 0L);
				pTool.setText("Total Selected:", SelectedUnit.getAllSelected().Count, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("ControllableUnit:", ControllableUnit.isControllingUnit(), 0f, pShowBar: false, 0L);
				pTool.setText("Total Controlled:", ControllableUnit.getCotrolledUnits().Count, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("Square Selection:", World.world.player_control.square_selection_started, 0f, pShowBar: false, 0L);
				pTool.setText("Square Selection Pos:", World.world.player_control.square_selection_position_current, 0f, pShowBar: false, 0L);
			}
		});
		add(new DebugToolAsset
		{
			id = "Selected Unit",
			action_1 = delegate(DebugTool pTool)
			{
				Actor unit = SelectedUnit.unit;
				if (unit != null)
				{
					Actor actorNearCursor = World.world.getActorNearCursor();
					if (actorNearCursor != null)
					{
						float num = unit.distanceToObjectTarget(actorNearCursor);
						pTool.setText("dist to target:", num, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Window",
			action_1 = delegate(DebugTool pTool)
			{
				ScrollWindow.debug(pTool);
				pTool.setSeparator();
				WindowHistory.debug(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "Selected Meta",
			action_1 = delegate(DebugTool pTool)
			{
				AssetManager.meta_type_library.debug(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "Camera",
			action_1 = delegate(DebugTool pTool)
			{
				//IL_059d: Unknown result type (might be due to invalid IL or missing references)
				//IL_05c1: Unknown result type (might be due to invalid IL or missing references)
				//IL_0654: Unknown result type (might be due to invalid IL or missing references)
				//IL_072b: Unknown result type (might be due to invalid IL or missing references)
				//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
				//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
				//IL_0302: Unknown result type (might be due to invalid IL or missing references)
				//IL_0307: Unknown result type (might be due to invalid IL or missing references)
				//IL_030a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0340: Unknown result type (might be due to invalid IL or missing references)
				//IL_0345: Unknown result type (might be due to invalid IL or missing references)
				//IL_0348: Unknown result type (might be due to invalid IL or missing references)
				//IL_037e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0383: Unknown result type (might be due to invalid IL or missing references)
				//IL_0386: Unknown result type (might be due to invalid IL or missing references)
				//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
				//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
				//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
				//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
				//IL_0438: Unknown result type (might be due to invalid IL or missing references)
				//IL_043d: Unknown result type (might be due to invalid IL or missing references)
				World.world.move_camera.debug(pTool);
				pTool.setSeparator();
				pTool.setText("zoom", World.world.camera.orthographicSize, 0f, pShowBar: false, 0L);
				pTool.setText("aspect", World.world.camera.aspect, 0f, pShowBar: false, 0L);
				pTool.setText("zoom_bound_mod", World.world.quality_changer.getZoomRateBoundLow(), 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("visible zones", World.world.zone_camera.countVisibleZones() + "/" + World.world.zone_calculator.zones.Count, 0f, pShowBar: false, 0L);
				pTool.setText("Input.touchCount", Input.touchCount, 0f, pShowBar: false, 0L);
				pTool.setText("origin_touch_dist", World.world.player_control.getDistanceBetweenOriginAndCurrentTouch(), 0f, pShowBar: false, 0L);
				pTool.setText("getDebugDragThreshold", World.world.player_control.getCurrentDragDistance().ToString("F3") + " / " + 0.007f, 0f, pShowBar: false, 0L);
				pTool.setText("getDebugDragThreshold %", (World.world.player_control.getCurrentDragDistance() * 100f).ToString("F3") + "%", 0f, pShowBar: false, 0L);
				pTool.setText("isTouchMoreThanDragThreshold %", World.world.player_control.isTouchMoreThanDragThreshold(), 0f, pShowBar: false, 0L);
				pTool.setText("already_used_camera_drag", World.world.player_control.already_used_camera_drag, 0f, pShowBar: false, 0L);
				pTool.setText("inspect_timer_click", World.world.player_control.inspect_timer_click, 0f, pShowBar: false, 0L);
				pTool.setText("touch_timer", World.world.player_control.touch_ticks_skip, 0f, pShowBar: false, 0L);
				if (Input.touchCount > 0)
				{
					for (int i = 0; i < Input.touchCount; i++)
					{
						string pT = "Touch.fingerId[" + i + "]";
						Touch touch = Input.GetTouch(i);
						pTool.setText(pT, ((Touch)(ref touch)).fingerId, 0f, pShowBar: false, 0L);
						string pT2 = "Touch.rawPosition[" + i + "]";
						touch = Input.GetTouch(i);
						pTool.setText(pT2, ((Touch)(ref touch)).rawPosition, 0f, pShowBar: false, 0L);
						string pT3 = "Touch.pos[" + i + "]";
						touch = Input.GetTouch(i);
						pTool.setText(pT3, ((Touch)(ref touch)).position, 0f, pShowBar: false, 0L);
						string pT4 = "Touch.dpos[" + i + "]";
						touch = Input.GetTouch(i);
						pTool.setText(pT4, ((Touch)(ref touch)).deltaPosition, 0f, pShowBar: false, 0L);
						string pT5 = "Touch.delta[" + i + "]";
						touch = Input.GetTouch(i);
						pTool.setText(pT5, ((Touch)(ref touch)).deltaTime, 0f, pShowBar: false, 0L);
						string pT6 = "Touch.radius[" + i + "]";
						touch = Input.GetTouch(i);
						pTool.setText(pT6, ((Touch)(ref touch)).radius, 0f, pShowBar: false, 0L);
						string pT7 = "Touch.pressure[" + i + "]";
						touch = Input.GetTouch(i);
						pTool.setText(pT7, ((Touch)(ref touch)).pressure, 0f, pShowBar: false, 0L);
					}
				}
				pTool.setText("Axis Vertical", Input.GetAxis("Vertical"), 0f, pShowBar: false, 0L);
				pTool.setText("Axis Horizontal", Input.GetAxis("Horizontal"), 0f, pShowBar: false, 0L);
				pTool.setText("Input.touchSupported", Input.touchSupported, 0f, pShowBar: false, 0L);
				pTool.setText("Input.touchPressureSupported", Input.touchPressureSupported, 0f, pShowBar: false, 0L);
				pTool.setText("Input.multiTouchEnabled", Input.multiTouchEnabled, 0f, pShowBar: false, 0L);
				pTool.setText("Input.stylusTouchSupported", Input.stylusTouchSupported, 0f, pShowBar: false, 0L);
				pTool.setText("Input.simulateMouseWithTouches", Input.simulateMouseWithTouches, 0f, pShowBar: false, 0L);
				pTool.setText("Input.mousePresent", Input.mousePresent, 0f, pShowBar: false, 0L);
				pTool.setText("Input.mousePosition", Input.mousePosition, 0f, pShowBar: false, 0L);
				pTool.setText("Input.mouseScrollDelta", Input.mouseScrollDelta, 0f, pShowBar: false, 0L);
				pTool.setText("Button 0", Input.GetMouseButton(0), 0f, pShowBar: false, 0L);
				pTool.setText("Button 1", Input.GetMouseButton(1), 0f, pShowBar: false, 0L);
				pTool.setText("Button 2", Input.GetMouseButton(2), 0f, pShowBar: false, 0L);
				pTool.setText("Axis ScrollWheel", Input.mouseScrollDelta.y, 0f, pShowBar: false, 0L);
				pTool.setText("Axis Mouse X", Input.GetAxis("Mouse X"), 0f, pShowBar: false, 0L);
				pTool.setText("Axis Mouse Y", Input.GetAxis("Mouse Y"), 0f, pShowBar: false, 0L);
				pTool.setText("Raw Mouse X", Input.GetAxisRaw("Mouse X"), 0f, pShowBar: false, 0L);
				pTool.setText("Raw Mouse Y", Input.GetAxisRaw("Mouse Y"), 0f, pShowBar: false, 0L);
				pTool.setText("Velocity", World.world.move_camera.getVelocity(), 0f, pShowBar: false, 0L);
			}
		});
	}

	private void initMap()
	{
		add(new DebugToolAsset
		{
			id = "tile_info",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					pTool.setText("x", mouseTilePos.x, 0f, pShowBar: false, 0L);
					pTool.setText("y", mouseTilePos.y, 0f, pShowBar: false, 0L);
					pTool.setText("id", mouseTilePos.data.tile_id, 0f, pShowBar: false, 0L);
					pTool.setText("height", mouseTilePos.data.height, 0f, pShowBar: false, 0L);
					pTool.setText("type", mouseTilePos.Type.id, 0f, pShowBar: false, 0L);
					pTool.setText("layer", mouseTilePos.Type.layer_type, 0f, pShowBar: false, 0L);
					pTool.setText("main tile", (mouseTilePos.main_type != null) ? mouseTilePos.main_type.id : "-", 0f, pShowBar: false, 0L);
					pTool.setText("cap tile", (mouseTilePos.top_type != null) ? mouseTilePos.top_type.id : "-", 0f, pShowBar: false, 0L);
					pTool.setText("burned", mouseTilePos.burned_stages, 0f, pShowBar: false, 0L);
					pTool.setText("targetedBy", mouseTilePos.isTargeted(), 0f, pShowBar: false, 0L);
					pTool.setText("units", mouseTilePos.countUnits(), 0f, pShowBar: false, 0L);
					pTool.setText("good_for_boat", mouseTilePos.isGoodForBoat(), 0f, pShowBar: false, 0L);
					pTool.setText("heat", mouseTilePos.heat, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("--zone:", "", 0f, pShowBar: false, 0L);
					TileZone zone = mouseTilePos.zone;
					if (zone.hasAnyBuildingsInSet(BuildingList.Civs))
					{
						pTool.setText("buildings:", zone.getHashset(BuildingList.Civs).Count, 0f, pShowBar: false, 0L);
					}
					if (zone.hasAnyBuildingsInSet(BuildingList.Ruins))
					{
						pTool.setText("ruins:", zone.getHashset(BuildingList.Ruins).Count, 0f, pShowBar: false, 0L);
					}
					if (zone.hasAnyBuildingsInSet(BuildingList.Trees))
					{
						pTool.setText("trees:", zone.getHashset(BuildingList.Trees).Count, 0f, pShowBar: false, 0L);
					}
					if (zone.hasAnyBuildingsInSet(BuildingList.Minerals))
					{
						pTool.setText("stone:", zone.getHashset(BuildingList.Minerals).Count, 0f, pShowBar: false, 0L);
					}
					if (zone.hasAnyBuildingsInSet(BuildingList.Food))
					{
						pTool.setText("fruits:", zone.getHashset(BuildingList.Food).Count, 0f, pShowBar: false, 0L);
					}
					if (zone.hasAnyBuildingsInSet(BuildingList.Hives))
					{
						pTool.setText("hives:", zone.getHashset(BuildingList.Hives).Count, 0f, pShowBar: false, 0L);
					}
					if (zone.hasAnyBuildingsInSet(BuildingList.Poops))
					{
						pTool.setText("poops:", zone.getHashset(BuildingList.Poops).Count, 0f, pShowBar: false, 0L);
					}
					if (zone.isZoneOnFire())
					{
						pTool.setText("fire:", WorldBehaviourActionFire.countFires(zone), 0f, pShowBar: false, 0L);
					}
					if (zone.tiles_with_liquid > 0)
					{
						pTool.setText("water tiles:", zone.tiles_with_liquid, 0f, pShowBar: false, 0L);
					}
					if (zone.tiles_with_ground > 0)
					{
						pTool.setText("ground tiles:", zone.tiles_with_ground, 0f, pShowBar: false, 0L);
					}
					if (zone.city != null)
					{
						pTool.setText("city:", zone.city.name, 0f, pShowBar: false, 0L);
					}
					if (zone.city != null && zone.city.kingdom != null)
					{
						pTool.setText("kingdom:", zone.city.kingdom.name, 0f, pShowBar: false, 0L);
					}
					if (mouseTilePos.hasBuilding())
					{
						pTool.setSeparator();
						pTool.setText("--building:", "", 0f, pShowBar: false, 0L);
						pTool.setText("resources:", mouseTilePos.building.hasResourcesToCollect(), 0f, pShowBar: false, 0L);
						pTool.setText("alive:", mouseTilePos.building.isAlive(), 0f, pShowBar: false, 0L);
						pTool.setText("is_usable:", mouseTilePos.building.isUsable(), 0f, pShowBar: false, 0L);
						pTool.setText("city:", (mouseTilePos.building.city != null) ? mouseTilePos.building.city.name : "-", 0f, pShowBar: false, 0L);
						pTool.setText("kingdom:", (mouseTilePos.building.city?.kingdom != null) ? mouseTilePos.building.city.kingdom.name : "-", 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Connections",
			action_1 = delegate(DebugTool pTool)
			{
				RegionLinkHashes.debug(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "Region",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null && mouseTilePos.region != null)
				{
					WorldTile mouseTilePos2 = World.world.getMouseTilePos();
					if (mouseTilePos2 != null)
					{
						MapRegion region = mouseTilePos2.region;
						if (region != null)
						{
							bool flag = false;
							string pT = "";
							foreach (MapRegion neighbour in region.neighbours)
							{
								if (neighbour.tiles.Count == 0)
								{
									flag = true;
									pT = neighbour.id.ToString();
									break;
								}
							}
							pTool.setText("- region id:", region.id, 0f, pShowBar: false, 0L);
							pTool.setText("-chunk id:", region.chunk.id, 0f, pShowBar: false, 0L);
							pTool.setText("-chunk xy:", region.chunk.x + " " + region.chunk.y, 0f, pShowBar: false, 0L);
							pTool.setText("- tEmptyRegionNeighbour:", flag, 0f, pShowBar: false, 0L);
							if (flag)
							{
								pTool.setText("- tEmptyRegionNeighbourID:", pT, 0f, pShowBar: false, 0L);
							}
							pTool.setText("- getEdgeTiles :", region.getEdgeTiles().Count, 0f, pShowBar: false, 0L);
							pTool.setText("- used in path :", region.used_by_path_lock + " " + region.region_path_id, 0f, pShowBar: false, 0L);
							pTool.setText("- region wave:", region.path_wave_id, 0f, pShowBar: false, 0L);
							pTool.setText("- centerRegion:", region.center_region, 0f, pShowBar: false, 0L);
							pTool.setText("- region tiles:", region.tiles.Count, 0f, pShowBar: false, 0L);
							pTool.setText("- region neigbours:", region.neighbours.Count, 0f, pShowBar: false, 0L);
							pTool.setText("- created:", region.created, 0f, pShowBar: false, 0L);
							pTool.setText("- island:", region.island == null, 0f, pShowBar: false, 0L);
							pTool.setText("- getEdgeRegions:", region.getEdgeRegions().Count, 0f, pShowBar: false, 0L);
							pTool.setText("- island connections:", region.island.getConnectedIslands().Count, 0f, pShowBar: false, 0L);
							pTool.setText("- debug_connections_left:", region.debug_blink_edges_left?.Count, 0f, pShowBar: false, 0L);
							pTool.setText("- debug_connections_right:", region.debug_blink_edges_right?.Count, 0f, pShowBar: false, 0L);
							pTool.setText("- debug_connections_up:", region.debug_blink_edges_up?.Count, 0f, pShowBar: false, 0L);
							pTool.setText("- debug_connections_down:", region.debug_blink_edges_down?.Count, 0f, pShowBar: false, 0L);
							mouseTilePos2.region.debugLinks(pTool);
						}
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Zone Info",
			action_1 = delegate(DebugTool pTool)
			{
				World.world.zone_calculator.debug(pTool);
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					_ = mouseTilePos.chunk;
					pTool.setText("visible:", mouseTilePos.zone.visible, 0f, pShowBar: false, 0L);
					pTool.setText("buildings:", mouseTilePos.zone.getHashset(BuildingList.Civs)?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("types:", mouseTilePos.zone.countNotNullTypes(), 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("id:", mouseTilePos.zone.id, 0f, pShowBar: false, 0L);
					pTool.setText("pos:", "x: " + mouseTilePos.zone.x + ", y: " + mouseTilePos.zone.y, 0f, pShowBar: false, 0L);
					pTool.setText("city:", mouseTilePos.zone.hasCity(), 0f, pShowBar: false, 0L);
					pTool.setText("bushes:", mouseTilePos.zone.getHashset(BuildingList.Food)?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("hives:", mouseTilePos.zone.getHashset(BuildingList.Hives)?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("trees:", mouseTilePos.zone.getHashset(BuildingList.Trees)?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("poops:", mouseTilePos.zone.getHashset(BuildingList.Poops)?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("deposits:", mouseTilePos.zone.getHashset(BuildingList.Minerals)?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("flore:", mouseTilePos.zone.getHashset(BuildingList.Flora)?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("buildings:", mouseTilePos.zone.getHashset(BuildingList.Civs)?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("buildings all:", mouseTilePos.zone.buildings_all.Count, 0f, pShowBar: false, 0L);
					pTool.setText("abandoned:", mouseTilePos.zone.getHashset(BuildingList.Abandoned)?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("ruins:", mouseTilePos.zone.getHashset(BuildingList.Ruins)?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("tilesWithGround:", mouseTilePos.zone.tiles_with_ground, 0f, pShowBar: false, 0L);
					pTool.setText("count deep ocean:", mouseTilePos.zone.getTilesOfType(TileLibrary.deep_ocean)?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("count soil:", mouseTilePos.zone.getTilesOfType(TileLibrary.soil_low)?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("count fuse:", mouseTilePos.zone.getTilesOfType(TopTileLibrary.fuse)?.Count, 0f, pShowBar: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "map_chunks",
			action_1 = delegate(DebugTool pTool)
			{
				int num = World.world.map_chunk_manager.chunks.Length;
				if (num >= 1)
				{
					int num2 = int.MaxValue;
					int num3 = 0;
					int num4 = 0;
					int num5 = int.MaxValue;
					int num6 = 0;
					int num7 = 0;
					MapChunk[] chunks = World.world.map_chunk_manager.chunks;
					foreach (MapChunk mapChunk in chunks)
					{
						int count = mapChunk.objects.kingdoms.Count;
						if (count < num2)
						{
							num2 = count;
						}
						if (count > num3)
						{
							num3 = count;
						}
						num4 += count;
						foreach (List<Actor> debugUnit in mapChunk.objects.getDebugUnits())
						{
							int count2 = debugUnit.Count;
							if (count2 < num5)
							{
								num5 = count2;
							}
							if (count2 > num6)
							{
								num6 = count2;
							}
							num7 += count2;
						}
						foreach (List<Building> debugBuilding in mapChunk.objects.getDebugBuildings())
						{
							int count3 = debugBuilding.Count;
							if (count3 < num5)
							{
								num5 = count3;
							}
							if (count3 > num6)
							{
								num6 = count3;
							}
							num7 += count3;
						}
					}
					pTool.setText("batches:", DebugConfig.isOn(DebugOption.ChunkBatches), 0f, pShowBar: false, 0L);
					pTool.setText("debug_batch_size:", ParallelHelper.DEBUG_BATCH_SIZE, 0f, pShowBar: false, 0L);
					pTool.setText("chunks:", num, 0f, pShowBar: false, 0L);
					pTool.setText("objects:", num4, 0f, pShowBar: false, 0L);
					pTool.setText("objects min:", num2, 0f, pShowBar: false, 0L);
					pTool.setText("objects max:", num3, 0f, pShowBar: false, 0L);
					pTool.setText("objects avg:", num4 / num, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("kingdom objects:", num7, 0f, pShowBar: false, 0L);
					pTool.setText("kingdom objects min:", num5, 0f, pShowBar: false, 0L);
					pTool.setText("kingdom objects max:", num6, 0f, pShowBar: false, 0L);
					pTool.setText("kingdom objects avg:", num7 / num, 0f, pShowBar: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Map Chunk",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					MapChunk chunk = mouseTilePos.chunk;
					pTool.setText("chunk_id:", chunk.id, 0f, pShowBar: false, 0L);
					pTool.setText("chunk_x/y:", chunk.x + "/" + chunk.y, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("kingdoms:", chunk.objects.kingdoms.Count, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setSeparator();
					pTool.setText("total_units:", chunk.objects.total_units, 0f, pShowBar: false, 0L);
					pTool.setText("total_buildings:", chunk.objects.total_buildings, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("total:", chunk.objects.total_units + chunk.objects.total_buildings, 0f, pShowBar: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Island Info",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null && mouseTilePos.region != null)
				{
					TileIsland island = mouseTilePos.region.island;
					if (island != null)
					{
						pTool.setText("islands:", World.world.islands_calculator.islands.Count, 0f, pShowBar: false, 0L);
						pTool.setText("regions:", island.regions.Count, 0f, pShowBar: false, 0L);
						pTool.setSeparator();
						pTool.setText("id:", island.id, 0f, pShowBar: false, 0L);
						pTool.setText("hash:", island.debug_hash_code, 0f, pShowBar: false, 0L);
						pTool.setText("tiles:", island.getTileCount(), 0f, pShowBar: false, 0L);
						pTool.setText("unit limit:", island.regions.Count * 4, 0f, pShowBar: false, 0L);
						pTool.setText("created:", island.created, 0f, pShowBar: false, 0L);
						pTool.setText("type:", island.type, 0f, pShowBar: false, 0L);
						pTool.setText("docks:", island.docks?.Count, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Tilemap Renderer",
			action_1 = delegate(DebugTool pTool)
			{
				World.world.tilemap.debug(pTool);
			}
		});
	}

	private void initSubSystems()
	{
		add(new DebugToolAsset
		{
			id = "boat",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					Actor actor = null;
					int num = int.MaxValue;
					foreach (Actor unit in World.world.units)
					{
						int num2 = Toolbox.SquaredDistTile(unit.current_tile, mouseTilePos);
						if (unit.asset.is_boat && num2 < num)
						{
							actor = unit;
							num = num2;
						}
					}
					if (actor != null)
					{
						Boat simpleComponent = actor.getSimpleComponent<Boat>();
						pTool.setSeparator();
						pTool.setText("units:", simpleComponent.countPassengers(), 0f, pShowBar: false, 0L);
						pTool.setText("passengerWaitCounter:", simpleComponent.passengerWaitCounter, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "taxi",
			action_1 = delegate(DebugTool pTool)
			{
				pTool.setText("requests:", TaxiManager.list.Count, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				TaxiManager.list.Sort((TaxiRequest a, TaxiRequest b) => b.countActors().CompareTo(a.countActors()));
				TaxiManager.list.ForEach(delegate(TaxiRequest tRequest)
				{
					int num = 0;
					if (tRequest.hasAssignedBoat())
					{
						num = tRequest.getBoat().countPassengers();
					}
					pTool.setText("state", tRequest.state.ToString() + " " + num + "/" + tRequest.countActors() + " | " + tRequest.hasAssignedBoat(), 0f, pShowBar: false, 0L);
				});
			}
		});
	}

	private void initGameplay()
	{
		add(new DebugToolAsset
		{
			id = "World Laws",
			action_1 = delegate(DebugTool pTool)
			{
				foreach (WorldLawAsset item in AssetManager.world_laws_library.list)
				{
					pTool.setText(item.id, item.isEnabled() + " : " + item.isEnabledRaw(), 0f, pShowBar: false, 0L);
				}
				pTool.setSeparator();
			}
		});
		add(new DebugToolAsset
		{
			id = "Building Manager",
			action_1 = delegate(DebugTool pTool)
			{
				pTool.setText("buildings:", World.world.buildings.Count, 0f, pShowBar: false, 0L);
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				foreach (Building building3 in World.world.buildings)
				{
					if (building3.is_visible)
					{
						num++;
					}
					if (building3.scale_helper.active)
					{
						num3++;
					}
				}
				pTool.setText("visible:", num + "/" + World.world.buildings.Count, 0f, pShowBar: false, 0L);
				pTool.setText("tweens:", num2 + "/" + World.world.buildings.Count, 0f, pShowBar: false, 0L);
				pTool.setText("tween_active:", num3 + "/" + World.world.buildings.Count, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
			}
		});
		add(new DebugToolAsset
		{
			id = "Cultures",
			action_1 = delegate(DebugTool pTool)
			{
				pTool.setText("cultures:", World.world.cultures.Count, 0f, pShowBar: false, 0L);
				foreach (Culture culture in World.world.cultures)
				{
					culture.debug(pTool);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Tile Types",
			action_1 = delegate(DebugTool pTool)
			{
				pTool.setText("tumor_low", TopTileLibrary.tumor_low.hashset.Count, 0f, pShowBar: false, 0L);
				pTool.setText("tumor_high", TopTileLibrary.tumor_high.hashset.Count, 0f, pShowBar: false, 0L);
				pTool.setText("biomass_low", TopTileLibrary.biomass_low.hashset.Count, 0f, pShowBar: false, 0L);
				pTool.setText("biomass_high", TopTileLibrary.biomass_high.hashset.Count, 0f, pShowBar: false, 0L);
				pTool.setText("pumpkin_low", TopTileLibrary.pumpkin_low.hashset.Count, 0f, pShowBar: false, 0L);
				pTool.setText("pumpkin_high", TopTileLibrary.pumpkin_high.hashset.Count, 0f, pShowBar: false, 0L);
				pTool.setText("cybertile_low", TopTileLibrary.cybertile_low.hashset.Count, 0f, pShowBar: false, 0L);
				pTool.setText("cybertile_high", TopTileLibrary.cybertile_high.hashset.Count, 0f, pShowBar: false, 0L);
				pTool.setText("deep_ocean", TileLibrary.deep_ocean.hashset.Count, 0f, pShowBar: false, 0L);
				pTool.setText("pit_deep_ocean", TileLibrary.pit_deep_ocean.hashset.Count, 0f, pShowBar: false, 0L);
			}
		});
		add(new DebugToolAsset
		{
			id = "Jobs Buildings",
			action_1 = delegate(DebugTool pTool)
			{
				World.world.buildings.debugJobManager(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "Jobs Actors",
			action_1 = delegate(DebugTool pTool)
			{
				World.world.units.debugJobManager(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "Building Info",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					Building building = mouseTilePos.building;
					if (building != null)
					{
						if (building.asset.docks)
						{
							pTool.setText("boats_fishing:", building.component_docks.countBoatTypes("boat_type_fishing"), 0f, pShowBar: false, 0L);
							pTool.setText("boats_transport:", building.component_docks.countBoatTypes("boat_type_transport"), 0f, pShowBar: false, 0L);
							pTool.setText("boats_trading:", building.component_docks.countBoatTypes("boat_type_trading"), 0f, pShowBar: false, 0L);
						}
						pTool.setText("id:", building.data.id, 0f, pShowBar: false, 0L);
						pTool.setText("hash:", building.GetHashCode(), 0f, pShowBar: false, 0L);
						pTool.setText("animData_index:", building.animData_index, 0f, pShowBar: false, 0L);
						pTool.setText("residents:", building.countResidents() + "/" + building.asset.housing_slots, 0f, pShowBar: false, 0L);
						pTool.setText("kingdom:", building.kingdom.id, 0f, pShowBar: false, 0L);
						pTool.setText("kingdom civ:", building.isKingdomCiv(), 0f, pShowBar: false, 0L);
						pTool.setText("animationState:", building.animation_state, 0f, pShowBar: false, 0L);
						pTool.setText("ownership:", building.state_ownership, 0f, pShowBar: false, 0L);
						pTool.setText("state:", building.data.state, 0f, pShowBar: false, 0L);
						pTool.setText("template:", building.data.asset_id, 0f, pShowBar: false, 0L);
						pTool.setText("health:", building.getHealth(), 0f, pShowBar: false, 0L);
						pTool.setText("health cur:", building.getMaxHealth(), 0f, pShowBar: false, 0L);
						if (building.hasKingdom())
						{
							pTool.setText("kingdom:", building.kingdom.name, 0f, pShowBar: false, 0L);
						}
						pTool.setSeparator();
						pTool.setText("tiles:", building.tiles.Count, 0f, pShowBar: false, 0L);
						pTool.setText("zones:", building.zones.Count, 0f, pShowBar: false, 0L);
						pTool.setSeparator();
						pTool.setText("alive:", building.isAlive(), 0f, pShowBar: false, 0L);
						pTool.setText("usable:", building.isUsable(), 0f, pShowBar: false, 0L);
						pTool.setText("under construction:", building.isUnderConstruction(), 0f, pShowBar: false, 0L);
						pTool.setText("progress:", building.getConstructionProgress(), 0f, pShowBar: false, 0L);
						if (building.city != null)
						{
							pTool.setText("city:", building.city.name, 0f, pShowBar: false, 0L);
						}
						pTool.setSeparator();
						pTool.setText("tween_active:", building.scale_helper.active, 0f, pShowBar: false, 0L);
						pTool.setSeparator();
						pTool.setText("state:", building.animation_state, 0f, pShowBar: false, 0L);
						pTool.setText("has_resources:", building.hasResourcesToCollect(), 0f, pShowBar: false, 0L);
						pTool.setText("is_visible:", building.is_visible, 0f, pShowBar: false, 0L);
						pTool.setText("scale_start:", building.scale_helper.scale_start, 0f, pShowBar: false, 0L);
						pTool.setText("currentScale.y:", building.current_scale.y, 0f, pShowBar: false, 0L);
						pTool.setSeparator();
						pTool.setText("flip.x:", building.flip_x, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Debug Buildings Render",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					Building building = mouseTilePos.building;
					if (building != null)
					{
						pTool.setText("flip.x:", building.flip_x, 0f, pShowBar: false, 0L);
						if (building.is_visible && !World.world.quality_changer.isLowRes())
						{
							int num = World.world.buildings.countVisibleBuildings();
							int num2 = 0;
							int num3 = 0;
							int num4 = 0;
							Building[] visibleBuildings = World.world.buildings.getVisibleBuildings();
							HashSet<Building> hashSet = UnsafeCollectionPool<HashSet<Building>, Building>.Get();
							for (int i = 0; i < num; i++)
							{
								Building building2 = visibleBuildings[i];
								if (building2 != null)
								{
									if (building2 == building)
									{
										num2 = i;
										pTool.setText("visible id:", i + "/" + visibleBuildings.Length, 0f, pShowBar: false, 0L);
									}
									hashSet.Add(building2);
									if (building2.isAlive())
									{
										num3++;
									}
									else
									{
										num4++;
									}
								}
							}
							pTool.setText("alive:", num3 + "/" + visibleBuildings.Length, 0f, pShowBar: false, 0L);
							pTool.setText("dead:", num4 + "/" + visibleBuildings.Length, 0f, pShowBar: false, 0L);
							pTool.setText("_visible_buildings_count:", num, 0f, pShowBar: false, 0L);
							pTool.setText("tUniqueBuildings:", hashSet.Count, 0f, pShowBar: false, 0L);
							UnsafeCollectionPool<HashSet<Building>, Building>.Release(hashSet);
							BuildingRenderData render_data = World.world.buildings.render_data;
							pTool.setText("render_data_flip:", render_data.flip_x_states[num2].ToString(), 0f, pShowBar: false, 0L);
							QuantumSpriteAsset quantumSpriteAsset = AssetManager.quantum_sprites.get("draw_buildings");
							QuantumSpriteCacheData cacheData = quantumSpriteAsset.group_system.getCacheData(num);
							if (cacheData != null)
							{
								if (cacheData.flip_x_states.Length <= num2)
								{
									return;
								}
								pTool.setText("render_data_flip:", cacheData.flip_x_states[num2].ToString(), 0f, pShowBar: false, 0L);
							}
							QuantumSprite[] fastActiveList = quantumSpriteAsset.group_system.getFastActiveList(num);
							if (fastActiveList.Length > num2)
							{
								pTool.setText("q flip x:", fastActiveList[num2].sprite_renderer.flipX.ToString(), 0f, pShowBar: false, 0L);
							}
						}
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Actor Statistics",
			action_1 = delegate(DebugTool pTool)
			{
				Actor actorNearCursor = World.world.getActorNearCursor();
				if (actorNearCursor != null)
				{
					pTool.setText("getSecondsLife:", StatTool.getStringSecondsLife(actorNearCursor), 0f, pShowBar: false, 0L);
					pTool.setText("getAmountBreeding:", StatTool.getStringAmountBreeding(actorNearCursor), 0f, pShowBar: false, 0L);
					pTool.setText("getAmountFood:", StatTool.getAmountFood(actorNearCursor), 0f, pShowBar: false, 0L);
					pTool.setText("getDPS:", StatTool.getDPS(actorNearCursor), 0f, pShowBar: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Biome Adaptation",
			action_1 = delegate(DebugTool pTool)
			{
				Actor unit = SelectedUnit.unit;
				if (unit != null && unit.hasSubspecies())
				{
					WorldTile mouseTilePos = World.world.getMouseTilePos();
					if (mouseTilePos != null)
					{
						mouseTilePos.zone.checkCanSettleInThisBiomes(unit.subspecies);
						pTool.setText("adapted:", TileZone.debug_adapted, 0f, pShowBar: false, 0L);
						pTool.setText("not_adapted:", TileZone.debug_not_adapted, 0f, pShowBar: false, 0L);
						pTool.setText("soil:", TileZone.debug_soil, 0f, pShowBar: false, 0L);
						pTool.setText("can_settle:", TileZone.debug_can_settle, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Kingdoms Wild",
			action_1 = delegate(DebugTool pTool)
			{
				pTool.setText("#wild_kingdoms:", World.world.kingdoms_wild.Count, 0f, pShowBar: false, 0L);
				foreach (Kingdom item2 in World.world.kingdoms_wild)
				{
					if (item2.hasUnits() || item2.hasBuildings())
					{
						pTool.setText(item2.name, item2.units.Count + " " + item2.buildings.Count, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Buildings Check",
			action_1 = delegate(DebugTool pTool)
			{
				int num = 0;
				int num2 = 0;
				foreach (Building building4 in World.world.buildings)
				{
					if (building4.getHealth() <= building4.getMaxHealth())
					{
						num++;
					}
					else
					{
						num2++;
					}
				}
				pTool.setText("within max health:", num, 0f, pShowBar: false, 0L);
				pTool.setText("higher than max health:", num2, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				foreach (Kingdom kingdom in World.world.kingdoms)
				{
					bool flag = kingdom.buildings.Count == kingdom.countBuildings();
					pTool.setText(kingdom.name, flag + " | " + kingdom.buildings.Count + " " + kingdom.countBuildings(), 0f, pShowBar: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Kingdoms Civ",
			action_1 = delegate(DebugTool pTool)
			{
				pTool.setText("#kingdoms:", World.world.kingdoms.Count, 0f, pShowBar: false, 0L);
				pTool.setText("- units total:", World.world.units.Count, 0f, pShowBar: false, 0L);
				int num = 0;
				foreach (Actor unit2 in World.world.units)
				{
					if (unit2.kingdom == null)
					{
						num++;
					}
				}
				pTool.setText("- units no kingdom:", num, 0f, pShowBar: false, 0L);
				List<Kingdom> obj = new List<Kingdom>(World.world.kingdoms);
				obj.Sort(pTool.kingdomSorter);
				foreach (Kingdom item3 in obj)
				{
					if (pTool.textCount > 0)
					{
						pTool.setSeparator();
					}
					pTool.setText("#id", item3.id, 0f, pShowBar: false, 0L);
					pTool.setText("#name", item3.name, 0f, pShowBar: false, 0L);
					pTool.setText("age", item3.getAge(), 0f, pShowBar: false, 0L);
					pTool.setText("units", item3.units.Count, 0f, pShowBar: false, 0L);
					pTool.setText("army", item3.countTotalWarriors() + "/" + item3.countWarriorsMax(), 0f, pShowBar: false, 0L);
					pTool.setText("buildings", item3.buildings.Count, 0f, pShowBar: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Behaviours",
			action_1 = delegate(DebugTool pTool)
			{
				World.world.drop_manager.debug(pTool);
				pTool.setText("dirty last:", World.world.dirty_tiles_last, 0f, pShowBar: false, 0L);
				pTool.setText("dirty tiles:", World.world.tiles_dirty.Count, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("tiles:", World.world.tiles_list.Length, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("water:", WorldBehaviourOcean.tiles.Count, 0f, pShowBar: false, 0L);
				pTool.setText("burned_tiles:", WorldBehaviourActionBurnedTiles.countBurnedTiles(), 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("grey goo:", World.world.grey_goo_layer.hashset?.Count, 0f, pShowBar: false, 0L);
				pTool.setText("conway", World.world.conway_layer.hashsetTiles?.Count, 0f, pShowBar: false, 0L);
				pTool.setText("flash effect:", World.world.flash_effects.pixels_to_update.Count, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("explosion layer:", World.world.explosion_layer.hashsetTiles?.Count, 0f, pShowBar: false, 0L);
				pTool.setText("bombDict:", World.world.explosion_layer.hashset_bombs.Count, 0f, pShowBar: false, 0L);
				pTool.setText("nextWave:", World.world.explosion_layer.nextWave.Count, 0f, pShowBar: false, 0L);
				pTool.setText("delayedBombs:", World.world.explosion_layer.nextWave.Count, 0f, pShowBar: false, 0L);
				pTool.setText("timedBombs:", World.world.explosion_layer.timedBombs.Count, 0f, pShowBar: false, 0L);
			}
		});
		add(new DebugToolAsset
		{
			id = "Unit Info",
			action_1 = delegate(DebugTool pTool)
			{
				Actor actorNearCursor = World.world.getActorNearCursor();
				if (actorNearCursor != null)
				{
					if (actorNearCursor.hasAnyStatusEffect())
					{
						pTool.setText("status effects", actorNearCursor.countStatusEffects(), 0f, pShowBar: false, 0L);
					}
					pTool.setText("profession:", actorNearCursor.getProfession(), 0f, pShowBar: false, 0L);
					if (actorNearCursor.ai.job != null)
					{
						pTool.setText("current_job:", actorNearCursor.ai.job.id, 0f, pShowBar: false, 0L);
					}
					else
					{
						pTool.setText("job:", "-", 0f, pShowBar: false, 0L);
					}
					pTool.setText("id:", actorNearCursor.data.id, 0f, pShowBar: false, 0L);
					if (actorNearCursor.hasTask())
					{
						pTool.setText("task:", actorNearCursor.ai.task.id, 0f, pShowBar: false, 0L);
					}
					else
					{
						pTool.setText("task:", "-", 0f, pShowBar: false, 0L);
					}
					pTool.setSeparator();
					pTool.setText("name:", actorNearCursor.getName(), 0f, pShowBar: false, 0L);
					pTool.setText("is_moving:", actorNearCursor.is_moving, 0f, pShowBar: false, 0L);
					pTool.setText("next_step:", actorNearCursor.next_step_position.x, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("stayingInBuilding:", actorNearCursor.inside_building != null, 0f, pShowBar: false, 0L);
					pTool.setText("bag.hasResources:", actorNearCursor.isCarryingResources(), 0f, pShowBar: false, 0L);
					pTool.setText("ignore:", actorNearCursor.countTargetsToIgnore(), 0f, pShowBar: false, 0L);
					pTool.setText("path global:", actorNearCursor.current_path_global?.Count, 0f, pShowBar: false, 0L);
					pTool.setText("path local:", actorNearCursor.current_path.Count, 0f, pShowBar: false, 0L);
					pTool.setText("path local index:", actorNearCursor.current_path_index, 0f, pShowBar: false, 0L);
					pTool.setText("path split status:", actorNearCursor.split_path.ToString(), 0f, pShowBar: false, 0L);
					pTool.setText("health:", actorNearCursor.getHealth() + "/" + actorNearCursor.getMaxHealth(), 0f, pShowBar: false, 0L);
					pTool.setText("damage:", actorNearCursor.asset.base_stats["damage"] + "/" + actorNearCursor.stats["damage"], 0f, pShowBar: false, 0L);
					pTool.setText("city:", (actorNearCursor.city == null) ? "-" : actorNearCursor.city.name, 0f, pShowBar: false, 0L);
					pTool.setText("kingdom:", (actorNearCursor.kingdom == null) ? "-" : actorNearCursor.kingdom.name, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("nutrition:", actorNearCursor.getNutrition() + "/" + actorNearCursor.getMaxNutrition(), 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					if (actorNearCursor.animation_container != null)
					{
						pTool.setText("actorAnimationData:", actorNearCursor.animation_container.id, 0f, pShowBar: false, 0L);
					}
					pTool.setText("stats name:", actorNearCursor.asset.id, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("timer_action:", actorNearCursor.timer_action, 0f, pShowBar: false, 0L);
					pTool.setText("_timeout_targets:", actorNearCursor._timeout_targets, 0f, pShowBar: false, 0L);
					pTool.setText("unitAttackTarget:", actorNearCursor.has_attack_target ? (actorNearCursor.isEnemyTargetAlive().ToString() ?? "") : "-", 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("attackTimer:", actorNearCursor.attack_timer, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setSeparator();
					pTool.setText("moveJumpOffset:", actorNearCursor.move_jump_offset.y, 0f, pShowBar: false, 0L);
					pTool.setText("alive:", actorNearCursor.isAlive(), 0f, pShowBar: false, 0L);
					pTool.setText("zPosition:", actorNearCursor.position_height, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("phenotype_index:", actorNearCursor.data.phenotype_index, 0f, pShowBar: false, 0L);
					pTool.setText("shade_id:", actorNearCursor.data.phenotype_shade, 0f, pShowBar: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Actor Stats",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					int num = int.MaxValue;
					Actor actor = null;
					foreach (Actor unit3 in World.world.units)
					{
						int num2 = Toolbox.SquaredDistTile(unit3.current_tile, mouseTilePos);
						if (num2 < num)
						{
							actor = unit3;
							num = num2;
						}
					}
					if (actor != null)
					{
						pTool.setText("name:", actor.getName(), 0f, pShowBar: false, 0L);
						pTool.setSeparator();
						List<BaseStatsContainer> list = actor.stats.getList();
						foreach (BaseStatsContainer item4 in list)
						{
							pTool.setText(item4.id, actor.stats[item4.id], 0f, pShowBar: false, 0L);
						}
						if (list.Count > 0)
						{
							pTool.setSeparator();
						}
						Dictionary<string, string> dictionary = actor.data.debug();
						foreach (string key in dictionary.Keys)
						{
							pTool.setText(key, dictionary[key], 0f, pShowBar: false, 0L);
						}
						if (dictionary.Count > 0)
						{
							pTool.setSeparator();
						}
						pTool.setText("currentTile:", (actor.current_tile == null) ? "-" : (actor.current_tile?.ToString() ?? ""), 0f, pShowBar: false, 0L);
						if (actor.current_tile != null)
						{
							pTool.setText("x / y", actor.current_tile.x + " " + actor.current_tile.y, 0f, pShowBar: false, 0L);
							pTool.setText("id", actor.current_tile.data.tile_id, 0f, pShowBar: false, 0L);
							pTool.setText("height", actor.current_tile.data.height, 0f, pShowBar: false, 0L);
							pTool.setText("type", actor.current_tile.Type.id, 0f, pShowBar: false, 0L);
							pTool.setText("layer", actor.current_tile.Type.layer_type, 0f, pShowBar: false, 0L);
							pTool.setText("main type", (actor.current_tile.main_type != null) ? actor.current_tile.main_type.id : "-", 0f, pShowBar: false, 0L);
							pTool.setText("top type", (actor.current_tile.top_type != null) ? actor.current_tile.top_type.id : "-", 0f, pShowBar: false, 0L);
							pTool.setText("targetedBy", actor.current_tile.isTargeted(), 0f, pShowBar: false, 0L);
							pTool.setText("units", actor.current_tile.countUnits(), 0f, pShowBar: false, 0L);
							pTool.setSeparator();
						}
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Unit Temperature",
			action_1 = delegate(DebugTool pTool)
			{
				WorldBehaviourUnitTemperatures.debug(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "Zoom",
			action_1 = delegate(DebugTool pTool)
			{
				World.world.quality_changer.debug(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "Mouse Cursor",
			action_1 = delegate(DebugTool pTool)
			{
				MouseCursor.debug(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "Selected Power",
			action_1 = delegate(DebugTool pTool)
			{
				if (!World.world.isAnyPowerSelected())
				{
					pTool.setText("no power selected", "", 0f, pShowBar: false, 0L);
				}
				else
				{
					pTool.setText("selectedPower:", World.world.getSelectedPowerID(), 0f, pShowBar: false, 0L);
					GodPower selectedPowerAsset = World.world.getSelectedPowerAsset();
					pTool.setText("type:", selectedPowerAsset.type, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("show_tool_sizes:", selectedPowerAsset.show_tool_sizes, 0f, pShowBar: false, 0L);
					pTool.setText("unselect_when_window:", selectedPowerAsset.unselect_when_window, 0f, pShowBar: false, 0L);
					pTool.setText("ignore_cursor_icon:", selectedPowerAsset.ignore_cursor_icon, 0f, pShowBar: false, 0L);
					pTool.setText("hold_action:", selectedPowerAsset.hold_action, 0f, pShowBar: false, 0L);
					pTool.setText("click_interval:", selectedPowerAsset.click_interval, 0f, pShowBar: false, 0L);
					pTool.setText("particle_interval:", selectedPowerAsset.particle_interval, 0f, pShowBar: false, 0L);
					pTool.setText("falling_chance:", selectedPowerAsset.falling_chance, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("click_brush_action:", selectedPowerAsset.click_brush_action, 0f, pShowBar: false, 0L);
					pTool.setText("click_action:", selectedPowerAsset.click_action, 0f, pShowBar: false, 0L);
					pTool.setText("click_special_action:", selectedPowerAsset.click_special_action, 0f, pShowBar: false, 0L);
					pTool.setText("click_power_brush_action:", selectedPowerAsset.click_power_brush_action, 0f, pShowBar: false, 0L);
					pTool.setText("click_power_action:", selectedPowerAsset.click_power_action, 0f, pShowBar: false, 0L);
					pTool.setText("select_button_action:", selectedPowerAsset.select_button_action, 0f, pShowBar: false, 0L);
					pTool.setText("toggle_action:", selectedPowerAsset.toggle_action, 0f, pShowBar: false, 0L);
					pTool.setSeparator();
					pTool.setText("actor_asset_id:", selectedPowerAsset.actor_asset_id, 0f, pShowBar: false, 0L);
					pTool.setText("actor_asset_ids:", selectedPowerAsset.actor_asset_ids, 0f, pShowBar: false, 0L);
					pTool.setText("toggle_name:", selectedPowerAsset.toggle_name, 0f, pShowBar: false, 0L);
					pTool.setText("map_modes_switch:", selectedPowerAsset.map_modes_switch, 0f, pShowBar: false, 0L);
					pTool.setText("show_spawn_effect:", selectedPowerAsset.show_spawn_effect, 0f, pShowBar: false, 0L);
					pTool.setText("activate_on_hotkey_select:", selectedPowerAsset.activate_on_hotkey_select, 0f, pShowBar: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Hotkeys",
			action_1 = delegate(DebugTool pTool)
			{
				AssetManager.hotkey_library.debug(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "Armies",
			action_1 = delegate(DebugTool pTool)
			{
				pTool.setText("groups:", World.world.armies.Count, 0f, pShowBar: false, 0L);
				foreach (Army army in World.world.armies)
				{
					pTool.setText(": " + army.id, army.getDebug(), 0f, pShowBar: false, 0L);
				}
				pTool.setSeparator();
			}
		});
		add(new DebugToolAsset
		{
			id = "Magnet Debug",
			action_1 = delegate(DebugTool pTool)
			{
				pTool.setText("hasUnits():", World.world.magnet.hasUnits(), 0f, pShowBar: false, 0L);
				pTool.setText("countUnits():", World.world.magnet.countUnits(), 0f, pShowBar: false, 0L);
				pTool.setText("magnetUnits.Count:", World.world.magnet.magnet_units.Count, 0f, pShowBar: false, 0L);
				int num = 0;
				foreach (Actor unit4 in World.world.units)
				{
					if (unit4.isAlive() && unit4.is_in_magnet)
					{
						num++;
					}
				}
				pTool.setText("tUnitsWithMagnetStatus:", num, 0f, pShowBar: false, 0L);
			}
		});
		add(new DebugToolAsset
		{
			id = "Mindmap Debug",
			action_1 = NeuronsOverview.debugTool
		});
	}

	private void initMain()
	{
		add(new DebugToolAsset
		{
			id = "Game Info",
			action_1 = delegate(DebugTool pTool)
			{
				pTool.setText("selected_unit?:", SelectedUnit.isSet(), 0f, pShowBar: false, 0L);
				pTool.setText("selected_unit_name:", SelectedUnit.isSet() ? SelectedUnit.unit.getName() : "-", 0f, pShowBar: false, 0L);
				pTool.setText("elapsed:", World.world.elapsed, 0f, pShowBar: false, 0L);
				pTool.setText("delta time:", World.world.delta_time, 0f, pShowBar: false, 0L);
				pTool.setText("actor0:", Bench.getBenchResult("actor0"), 0f, pShowBar: false, 0L);
				pTool.setText("actor1:", Bench.getBenchResult("actor1"), 0f, pShowBar: false, 0L);
				pTool.setText("actor2:", Bench.getBenchResult("actor2"), 0f, pShowBar: false, 0L);
				pTool.setText("actor_total:", Bench.getBenchResult("actor_total"), 0f, pShowBar: false, 0L);
				pTool.setText("test_follow:", Bench.getBenchResult("test_follow"), 0f, pShowBar: false, 0L);
				pTool.setText("rightClickTimer:", World.world.player_control.inspect_timer_click, 0f, pShowBar: false, 0L);
				pTool.setText("cache g paths:", World.world.region_path_finder.debug(), 0f, pShowBar: false, 0L);
				pTool.setText("units:", World.world.units.debugContainer(), 0f, pShowBar: false, 0L);
				pTool.setText("buildings:", World.world.buildings.debugContainer(), 0f, pShowBar: false, 0L);
				pTool.setText("cities:", World.world.cities.Count, 0f, pShowBar: false, 0L);
				pTool.setText("civ kingdoms:", World.world.kingdoms.Count, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("(d)gameTime:", World.world.game_stats.data.gameTime, 0f, pShowBar: false, 0L);
				pTool.setText("(f)gameTime:", (float)World.world.game_stats.data.gameTime, 0f, pShowBar: false, 0L);
				World.world.map_stats.debug(pTool);
				pTool.setText("gameLaunches:", World.world.game_stats.data.gameLaunches, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("size tiles:", World.world.tiles_map.Length, 0f, pShowBar: false, 0L);
				pTool.setText("chunks:", World.world.map_chunk_manager.chunks.Length, 0f, pShowBar: false, 0L);
				pTool.setText("- regions:", World.world.map_chunk_manager.countRegions(), 0f, pShowBar: false, 0L);
				pTool.setText("- hashes:", RegionLinkHashes.getCount(), 0f, pShowBar: false, 0L);
				pTool.setText("- islands:", World.world.islands_calculator.islands.Count, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				int count = World.world.units.visible_units.count;
				int num = 0;
				foreach (Building building in World.world.buildings)
				{
					if (building.is_visible)
					{
						num++;
					}
				}
				pTool.setSeparator();
				pTool.setText("visible buildings:", num + "/" + World.world.buildings.Count, 0f, pShowBar: false, 0L);
				pTool.setText("visible buildings:", World.world.buildings.countVisibleBuildings() + "/" + World.world.buildings.getVisibleBuildings().Length, 0f, pShowBar: false, 0L);
				pTool.setText("visible actors:", count + "/" + World.world.units.Count, 0f, pShowBar: false, 0L);
			}
		});
		add(new DebugToolAsset
		{
			id = "Basic Info",
			action_1 = delegate(DebugTool pTool)
			{
				//IL_0087: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
				//IL_0155: Unknown result type (might be due to invalid IL or missing references)
				//IL_015a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0241: Unknown result type (might be due to invalid IL or missing references)
				//IL_0265: Unknown result type (might be due to invalid IL or missing references)
				pTool.setText("Game Version:", Application.version, 0f, pShowBar: false, 0L);
				pTool.setText("Version Code:", Config.versionCodeText, 0f, pShowBar: false, 0L);
				pTool.setText("Git:", Config.gitCodeText, 0f, pShowBar: false, 0L);
				pTool.setText("Modded:", Config.MODDED, 0f, pShowBar: false, 0L);
				pTool.setText("operatingSystemFamily:", SystemInfo.operatingSystemFamily, 0f, pShowBar: false, 0L);
				pTool.setText("deviceModel:", SystemInfo.deviceModel, 0f, pShowBar: false, 0L);
				pTool.setText("deviceName:", SystemInfo.deviceName, 0f, pShowBar: false, 0L);
				pTool.setText("deviceType:", SystemInfo.deviceType, 0f, pShowBar: false, 0L);
				pTool.setText("systemMemorySize:", SystemInfo.systemMemorySize, 0f, pShowBar: false, 0L);
				pTool.setText("graphicsDeviceID:", SystemInfo.graphicsDeviceID, 0f, pShowBar: false, 0L);
				pTool.setText("graphicsActiveTier:", ((object)Graphics.activeTier/*cast due to constrained. prefix*/).ToString(), 0f, pShowBar: false, 0L);
				pTool.setText("GC.GetTotalMemory:", GC.GetTotalMemory(forceFullCollection: false) / 1000000 + " mb", 0f, pShowBar: false, 0L);
				pTool.setText("graphicsMemorySize:", SystemInfo.graphicsMemorySize, 0f, pShowBar: false, 0L);
				pTool.setText("maxTextureSize:", SystemInfo.maxTextureSize, 0f, pShowBar: false, 0L);
				pTool.setText("operatingSystem:", SystemInfo.operatingSystem, 0f, pShowBar: false, 0L);
				pTool.setText("processorType:", SystemInfo.processorType, 0f, pShowBar: false, 0L);
				pTool.setText("installMode:", Application.installMode, 0f, pShowBar: false, 0L);
				pTool.setText("sandboxType:", Application.sandboxType, 0f, pShowBar: false, 0L);
				pTool.setText("FPS:", FPS.getFPS(), 0f, pShowBar: false, 0L);
			}
		});
		add(new DebugToolAsset
		{
			id = "sprite_atlas_manager",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					int num = int.MaxValue;
					Actor actor = null;
					foreach (Actor unit in World.world.units)
					{
						int num2 = Toolbox.SquaredDistTile(unit.current_tile, mouseTilePos);
						if (num2 < num)
						{
							actor = unit;
							num = num2;
						}
					}
					if (actor != null)
					{
						AssetManager.dynamic_sprites_library.debug(pTool, actor);
						pTool.setSeparator();
						pTool.setText("sex:", actor.data.sex, 0f, pShowBar: false, 0L);
						pTool.setText("head:", actor.has_rendered_sprite_head ? ((Object)actor.cached_sprite_head).name : "-", 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Subspecies",
			action_1 = delegate(DebugTool pTool)
			{
				using ListPool<Subspecies> listPool = new ListPool<Subspecies>(World.world.subspecies.list);
				listPool.Sort((Subspecies a, Subspecies b) => b.units.Count.CompareTo(a.units.Count));
				foreach (ref Subspecies item3 in listPool)
				{
					Subspecies current = item3;
					pTool.setText("[" + current.getActorAsset().id + "] " + current.name + ": ", current.units.Count, 0f, pShowBar: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Items",
			action_1 = delegate(DebugTool pTool)
			{
				pTool.setText("total: ", World.world.items.Count, 0f, pShowBar: false, 0L);
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				int num6 = 0;
				int num7 = 0;
				int num8 = 0;
				int num9 = 0;
				int num10 = 0;
				int num11 = 0;
				int num12 = 0;
				foreach (Item item4 in World.world.items)
				{
					if (item4.unit_has_it)
					{
						num7++;
					}
					if (item4.city_has_it)
					{
						num8++;
					}
					if (item4.isRekt())
					{
						num++;
					}
					else
					{
						num2++;
						if (item4.hasActor())
						{
							num3++;
							if (item4.getActor().isRekt())
							{
								num5++;
							}
							else if (item4.getActor().hasEquipment())
							{
								num9++;
								bool flag = false;
								foreach (Item item5 in item4.getActor().equipment.getItems())
								{
									if (item5 == item4)
									{
										flag = true;
										break;
									}
								}
								if (flag)
								{
									num11++;
								}
								else
								{
									num12++;
								}
							}
							else
							{
								num10++;
							}
						}
						if (item4.hasCity())
						{
							num4++;
							if (item4.getCity().isRekt())
							{
								num6++;
							}
						}
					}
				}
				pTool.setSeparator();
				pTool.setText("alive: ", num2, 0f, pShowBar: false, 0L);
				pTool.setText("dead: ", num, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("has actor: ", num3, 0f, pShowBar: false, 0L);
				pTool.setText("has unit has it: ", num7, 0f, pShowBar: false, 0L);
				pTool.setText("has dead actor: ", num5, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("has city: ", num4, 0f, pShowBar: false, 0L);
				pTool.setText("has city has it: ", num8, 0f, pShowBar: false, 0L);
				pTool.setText("has dead city: ", num6, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("unit has equipment: ", num9, 0f, pShowBar: false, 0L);
				pTool.setText("unit w/o equipment: ", num10, 0f, pShowBar: false, 0L);
				pTool.setText("unit has item equipped: ", num11, 0f, pShowBar: false, 0L);
				pTool.setText("unit missing equipped item: ", num12, 0f, pShowBar: false, 0L);
			}
		});
		add(new DebugToolAsset
		{
			id = "Decisions Globals Use",
			action_1 = delegate(DebugTool pTool)
			{
				foreach (KeyValuePair<string, int> item6 in UtilityBasedDecisionSystem.debug_counter)
				{
					pTool.setText(item6.Key, item6.Value, 0f, pShowBar: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Actor Decisions",
			action_1 = delegate(DebugTool pTool)
			{
				Actor actorNearCursor = World.world.getActorNearCursor();
				if (actorNearCursor != null)
				{
					if (_decision_system_debug == null)
					{
						_decision_system_debug = new UtilityBasedDecisionSystem();
					}
					_decision_system_debug.debug(actorNearCursor, pTool);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Items Errors",
			action_1 = delegate(DebugTool pTool)
			{
				Dictionary<Item, string> dictionary = new Dictionary<Item, string>();
				foreach (Item item7 in World.world.items)
				{
					dictionary.Add(item7, "nobody");
				}
				Dictionary<Item, int> dictionary2 = new Dictionary<Item, int>();
				foreach (Actor unit2 in World.world.units)
				{
					if (unit2.hasEquipment())
					{
						foreach (ActorEquipmentSlot item8 in unit2.equipment)
						{
							if (!item8.isEmpty())
							{
								Item item = item8.getItem();
								dictionary[item] = "unit";
								if (!dictionary2.ContainsKey(item))
								{
									dictionary2.Add(item, 0);
								}
								dictionary2[item]++;
							}
						}
					}
				}
				foreach (City city in World.world.cities)
				{
					foreach (List<long> allEquipmentList in city.data.equipment.getAllEquipmentLists())
					{
						foreach (long item9 in allEquipmentList)
						{
							Item key = World.world.items.get(item9);
							dictionary[key] = "city";
							if (!dictionary2.ContainsKey(key))
							{
								dictionary2.Add(key, 0);
							}
							dictionary2[key]++;
						}
					}
				}
				foreach (KeyValuePair<Item, string> item10 in dictionary)
				{
					if (item10.Value == "nobody")
					{
						Item key2 = item10.Key;
						pTool.setText(key2.id.ToString(), "nobody", 0f, pShowBar: false, 0L);
					}
				}
				foreach (KeyValuePair<Item, int> item11 in dictionary2)
				{
					if (item11.Value > 1)
					{
						pTool.setText(item11.Key.id.ToString(), item11.Value, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Items Duplicates",
			action_1 = delegate(DebugTool pTool)
			{
				Dictionary<Item, int> dictionary = new Dictionary<Item, int>();
				foreach (Actor unit3 in World.world.units)
				{
					if (unit3.hasEquipment())
					{
						foreach (ActorEquipmentSlot item12 in unit3.equipment)
						{
							if (item12.getItem() != null)
							{
								Item item = item12.getItem();
								if (!dictionary.ContainsKey(item))
								{
									dictionary.Add(item, 0);
								}
								dictionary[item]++;
							}
						}
					}
				}
				foreach (KeyValuePair<Item, int> item13 in dictionary)
				{
					if (item13.Value >= 2)
					{
						pTool.setText("Item " + (item13.Key.data?.id.ToString() ?? "(dead)") + " shared between units: ", item13.Value, 0f, pShowBar: false, 0L);
					}
				}
				dictionary.Clear();
				foreach (City city2 in World.world.cities)
				{
					foreach (List<long> allEquipmentList2 in city2.data.equipment.getAllEquipmentLists())
					{
						foreach (long item14 in allEquipmentList2)
						{
							Item item2 = World.world.items.get(item14);
							if (item2 != null)
							{
								if (!dictionary.ContainsKey(item2))
								{
									dictionary.Add(item2, 0);
								}
								dictionary[item2]++;
							}
						}
					}
				}
				foreach (KeyValuePair<Item, int> item15 in dictionary)
				{
					if (item15.Value >= 2)
					{
						pTool.setText("Item " + (item15.Key.data?.id.ToString() ?? "(dead)") + " shared between сшешуы: ", item15.Value, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Items Ownership",
			action_1 = delegate(DebugTool pTool)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				int num6 = 0;
				int num7 = 0;
				int num8 = 0;
				new Dictionary<Item, string>();
				foreach (Item item16 in World.world.items)
				{
					if (!item16.unit_has_it && !item16.city_has_it)
					{
						num6++;
						if (item16.isRekt())
						{
							num8++;
						}
						else
						{
							if (item16.isFavorite())
							{
								num++;
							}
							if (item16.isEternal())
							{
								num3++;
							}
							if (item16.isFavorite() && item16.isEternal())
							{
								num5++;
							}
							else if (item16.isFavorite())
							{
								num2++;
							}
							else if (item16.isEternal())
							{
								num4++;
							}
							else
							{
								num7++;
							}
						}
					}
				}
				pTool.setText("total: ", World.world.items.Count, 0f, pShowBar: false, 0L);
				pTool.setText("total ownerless: ", num6, 0f, pShowBar: false, 0L);
				pTool.setText("ownerless favorited: ", num, 0f, pShowBar: false, 0L);
				pTool.setText("ownerless favorited only: ", num2, 0f, pShowBar: false, 0L);
				pTool.setText("ownerless eternal: ", num3, 0f, pShowBar: false, 0L);
				pTool.setText("ownerless eternal only: ", num4, 0f, pShowBar: false, 0L);
				pTool.setText("ownerless favorited eternal: ", num5, 0f, pShowBar: false, 0L);
				pTool.setText("ownerless error: ", num7, 0f, pShowBar: false, 0L);
				pTool.setText("ownerless rekt: ", num8, 0f, pShowBar: false, 0L);
			}
		});
		add(new DebugToolAsset
		{
			id = "Families",
			action_1 = delegate(DebugTool pTool)
			{
				float num = 0f;
				float num2 = 0f;
				foreach (Actor item17 in World.world.units.units_only_alive)
				{
					if (item17.hasFamily())
					{
						num++;
					}
					else
					{
						num2++;
					}
				}
				int num3 = 0;
				foreach (Family family in World.world.families)
				{
					if (family.units.Count == 1)
					{
						num3++;
					}
				}
				pTool.setText("total families", World.world.families.Count, 0f, pShowBar: false, 0L);
				pTool.setText("lonely families", num3, 0f, pShowBar: false, 0L);
				pTool.setText("total units", num + num2, 0f, pShowBar: false, 0L);
				pTool.setText("fam/no fam", num + "/" + num2, 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				List<Family> obj = new List<Family>(World.world.families);
				obj.Sort((Family a, Family b) => b.units.Count.CompareTo(a.units.Count));
				foreach (Family item18 in obj)
				{
					if (item18.units.Count >= 2)
					{
						pTool.setText("[" + item18.data.species_id + "] " + item18.name + ": ", item18.units.Count, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Languages",
			action_1 = delegate(DebugTool pTool)
			{
				foreach (Language language in World.world.languages)
				{
					pTool.setText("[] " + language.name + ": ", language.units.Count, 0f, pShowBar: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Religions",
			action_1 = delegate(DebugTool pTool)
			{
				foreach (Religion religion in World.world.religions)
				{
					pTool.setText("[] " + religion.name + ": ", religion.units.Count, 0f, pShowBar: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Actor Asset Units",
			action_1 = delegate(DebugTool pTool)
			{
				foreach (ActorAsset item19 in AssetManager.actor_library.list)
				{
					if (item19.units.Count != 0)
					{
						pTool.setText(item19.id + ": ", item19.units.Count, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Population",
			action_1 = delegate(DebugTool pTool)
			{
				int num = 0;
				foreach (City city3 in World.world.cities)
				{
					num += city3.getPopulationPeople();
				}
				pTool.setText("city units:", num, 0f, pShowBar: false, 0L);
				pTool.setText("unit list:", World.world.units.debugContainer(), 0f, pShowBar: false, 0L);
			}
		});
		add(new DebugToolAsset
		{
			id = "System Managers",
			action_1 = delegate(DebugTool pTool)
			{
				foreach (BaseSystemManager list_all_sim_manager in World.world.list_all_sim_managers)
				{
					list_all_sim_manager.showDebugTool(pTool);
				}
			}
		});
	}

	private void initAI()
	{
		add(new DebugToolAsset
		{
			id = "Actor AI",
			action_1 = delegate(DebugTool pTool)
			{
				//IL_00de: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
				//IL_011c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0121: Unknown result type (might be due to invalid IL or missing references)
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					int num = int.MaxValue;
					Actor actor = null;
					foreach (Actor unit in World.world.units)
					{
						if (!unit.isInsideSomething())
						{
							int num2 = Toolbox.SquaredDistTile(unit.current_tile, mouseTilePos);
							if (num2 < num)
							{
								actor = unit;
								num = num2;
							}
						}
					}
					if (actor != null)
					{
						pTool.setText("timer_action:", actor.timer_action, 0f, pShowBar: false, 0L);
						pTool.setText("stat id:", actor.asset.id, 0f, pShowBar: false, 0L);
						actor.ai.debug(pTool);
						WorldTile beh_tile_target = actor.beh_tile_target;
						int? num3;
						Vector2Int pos;
						if (beh_tile_target == null)
						{
							num3 = null;
						}
						else
						{
							pos = beh_tile_target.pos;
							num3 = ((Vector2Int)(ref pos))[0];
						}
						int? num4 = num3;
						string? text = num4.ToString();
						WorldTile beh_tile_target2 = actor.beh_tile_target;
						int? num5;
						if (beh_tile_target2 == null)
						{
							num5 = null;
						}
						else
						{
							pos = beh_tile_target2.pos;
							num5 = ((Vector2Int)(ref pos))[1];
						}
						num4 = num5;
						pTool.setText("beh_tile_target", text + ":" + num4, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Boat AI",
			action_1 = delegate(DebugTool pTool)
			{
				//IL_0140: Unknown result type (might be due to invalid IL or missing references)
				//IL_0145: Unknown result type (might be due to invalid IL or missing references)
				//IL_015f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0164: Unknown result type (might be due to invalid IL or missing references)
				//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
				//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
				//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
				//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					int num = int.MaxValue;
					Actor actor = null;
					foreach (Actor unit2 in World.world.units)
					{
						if (!unit2.isInsideSomething() && unit2.asset.is_boat)
						{
							int num2 = Toolbox.SquaredDistTile(unit2.current_tile, mouseTilePos);
							if (num2 < num)
							{
								actor = unit2;
								num = num2;
							}
						}
					}
					if (actor != null)
					{
						pTool.setText("action_timer:", actor.timer_action, 0f, pShowBar: false, 0L);
						pTool.setText("stat id:", actor.asset.id, 0f, pShowBar: false, 0L);
						TaxiRequest taxi_request = actor.getSimpleComponent<Boat>().taxi_request;
						if (taxi_request != null)
						{
							pTool.setText("taxi state:", taxi_request.state, 0f, pShowBar: false, 0L);
							pTool.setText("taxi actors:", taxi_request.countActors(), 0f, pShowBar: false, 0L);
							WorldTile tileTarget = taxi_request.getTileTarget();
							object pT;
							Vector2Int pos;
							if (tileTarget == null)
							{
								pT = "-";
							}
							else
							{
								pos = tileTarget.pos;
								string text = ((Vector2Int)(ref pos))[0].ToString();
								pos = tileTarget.pos;
								pT = text + ":" + ((Vector2Int)(ref pos))[1];
							}
							pTool.setText("taxi target:", pT, 0f, pShowBar: false, 0L);
							WorldTile tileStart = taxi_request.getTileStart();
							object pT2;
							if (tileStart == null)
							{
								pT2 = "-";
							}
							else
							{
								pos = tileStart.pos;
								string text2 = ((Vector2Int)(ref pos))[0].ToString();
								pos = tileStart.pos;
								pT2 = text2 + ":" + ((Vector2Int)(ref pos))[1];
							}
							pTool.setText("taxi start:", pT2, 0f, pShowBar: false, 0L);
						}
						actor.ai.debug(pTool);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "City AI",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					City city = mouseTilePos.zone.city;
					if (city != null)
					{
						pTool.setText("warrior_timer:", city.getTimerForNewWarrior(), 0f, pShowBar: false, 0L);
						pTool.setSeparator();
						if (city.ai != null)
						{
							city.ai.debug(pTool);
						}
						pTool.setSeparator();
						pTool.setText("action_timer:", city.timer_action, 0f, pShowBar: false, 0L);
					}
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Kingdom AI",
			action_1 = delegate(DebugTool pTool)
			{
				WorldTile mouseTilePos = World.world.getMouseTilePos();
				if (mouseTilePos != null)
				{
					City city = mouseTilePos.zone.city;
					if (city != null)
					{
						Kingdom kingdom = city.kingdom;
						if (kingdom.hasKing())
						{
							pTool.setText("personality:", kingdom.king.s_personality.id, 0f, pShowBar: false, 0L);
							pTool.setText("agression:", kingdom.king.stats["personality_aggression"], 0f, pShowBar: false, 0L);
							pTool.setText("administration:", kingdom.king.stats["personality_administration"], 0f, pShowBar: false, 0L);
							pTool.setText("diplomatic:", kingdom.king.stats["personality_diplomatic"], 0f, pShowBar: false, 0L);
							pTool.setSeparator();
						}
						pTool.setText("timer_action:", kingdom.timer_action, 0f, pShowBar: false, 0L);
						pTool.setText("timer_new_king:", kingdom.data.timer_new_king, 0f, pShowBar: false, 0L);
						pTool.setSeparator();
						pTool.setText("action_timer:", kingdom.timer_action, 0f, pShowBar: false, 0L);
						if (kingdom.ai != null)
						{
							kingdom.ai.debug(pTool);
						}
					}
				}
			}
		});
	}

	private void initFmod()
	{
		add(new DebugToolAsset
		{
			id = "FMOD",
			action_1 = delegate(DebugTool pTool)
			{
				MusicBox.debug_fmod(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "FMOD World Params",
			action_1 = delegate(DebugTool pTool)
			{
				MusicBox.inst.debug_world_params(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "FMOD Unit Params",
			action_1 = delegate(DebugTool pTool)
			{
				MusicBox.inst.debug_unit_params(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "FMOD Params",
			action_1 = delegate(DebugTool pTool)
			{
				MusicBox.inst.debug_params(pTool);
			}
		});
		add(new DebugToolAsset
		{
			id = "Cursor Speed",
			action_1 = delegate(DebugTool pTool)
			{
				MapBox.cursor_speed.debug(pTool);
			}
		});
	}

	private void initUI()
	{
		add(new DebugToolAsset
		{
			id = "screen_orientation",
			action_1 = delegate(DebugTool pTool)
			{
				//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
				pTool.setText("width:", Screen.width, 0f, pShowBar: false, 0L);
				pTool.setText("height:", Screen.height, 0f, pShowBar: false, 0L);
				pTool.setText("last width:", CanvasMain.instance.getLastWidth(), 0f, pShowBar: false, 0L);
				pTool.setText("last height:", CanvasMain.instance.getLastHeight(), 0f, pShowBar: false, 0L);
				pTool.setText("orientation:", Screen.orientation, 0f, pShowBar: false, 0L);
				pTool.setText("saved orientation:", (object)(ScreenOrientation)(PlayerConfig.optionBoolEnabled("portrait") ? 1 : 3), 0f, pShowBar: false, 0L);
				pTool.setText("rotation to portrait:", Screen.autorotateToPortrait, 0f, pShowBar: false, 0L);
				pTool.setText("rotation to landscape left:", Screen.autorotateToLandscapeLeft, 0f, pShowBar: false, 0L);
				pTool.setText("rotation to landscape right:", Screen.autorotateToLandscapeRight, 0f, pShowBar: false, 0L);
				pTool.setText("rotation to portrait reversed:", Screen.autorotateToPortraitUpsideDown, 0f, pShowBar: false, 0L);
			}
		});
	}

	public override void post_init()
	{
		base.post_init();
		list.Sort((DebugToolAsset a, DebugToolAsset b) => a.priority.CompareTo(b.priority));
		list.Sort((DebugToolAsset a, DebugToolAsset b) => string.Compare(a.id, b.id, StringComparison.InvariantCultureIgnoreCase));
		TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
		foreach (DebugToolAsset item in list)
		{
			if (item.id.ToLower() == item.id)
			{
				item.name = textInfo.ToTitleCase(item.id.Replace('_', ' '));
			}
			else
			{
				item.name = item.id;
			}
		}
	}

	public override DebugToolAsset get(string pId)
	{
		foreach (DebugToolAsset item in list)
		{
			if (item.name == pId)
			{
				return item;
			}
		}
		return base.get(pId);
	}

	private void initBenchmarks()
	{
		add(new DebugToolAsset
		{
			id = "Benchmark All",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			priority = 1,
			benchmark_group_id = "game_total",
			benchmark_total = "game_total",
			benchmark_total_group = "main",
			action_start = setBenchmarksDefaultValue,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Test Decisions",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			priority = 50,
			benchmark_group_id = "decisions_test",
			benchmark_total = "decisions_test",
			benchmark_total_group = "decisions_test_total",
			split_benchmark = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = true;
				pTool.show_averages = false;
				pTool.hide_zeroes = false;
				pTool.show_max = true;
				pTool.sort_by_names = false;
				pTool.sort_by_values = true;
				pTool.state = DebugToolState.Values;
			},
			action_update = delegate
			{
				if (World.world.units.Count != 0)
				{
					Actor pActor = World.world.units.getSimpleList()[0];
					Bench.bench("decisions_test", "decisions_test_total");
					Bench.bench("decisions", "decisions_test");
					for (int i = 0; i < 5000; i++)
					{
						DecisionHelper.runSimulation(pActor);
					}
					Bench.benchEnd("decisions", "decisions_test", pSaveCounter: false, 0L);
					Bench.benchEnd("decisions_test", "decisions_test_total", pSaveCounter: false, 0L);
				}
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Zone Camera",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "zone_camera",
			benchmark_total = "zone_camera",
			benchmark_total_group = "zone_camera_total",
			action_start = setBenchmarksDefaultValue,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom
		});
		add(new DebugToolAsset
		{
			id = "benchmark_chunks",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "chunks",
			benchmark_total = "chunks",
			benchmark_total_group = "chunks_total",
			split_benchmark = true,
			action_1 = delegate(DebugTool pTool)
			{
				double totalFrameBudget = getTotalFrameBudget();
				double benchResultAsDouble = Bench.getBenchResultAsDouble(pTool.asset.benchmark_total, pTool.asset.benchmark_total_group, pTool.isValueAverage());
				pTool.setText("group total:", trim(benchResultAsDouble, pAddMS: true), 100f, pShowBar: true, 0L);
				double num = benchResultAsDouble / (double)Time.deltaTime * 100.0;
				pTool.setText("total frame time spent:", trimPercent(num), (float)num, pShowBar: true, 0L);
				double num2 = benchResultAsDouble * 1000.0 / totalFrameBudget * 100.0;
				pTool.setText("total budget time spent:", trimPercent(num2), (float)num2, pShowBar: true, 0L);
				pTool.setSeparator();
				pTool.setText("########### last_dirty:", null, 0f, pShowBar: false, 0L);
				pTool.setText("chunks:", Bench.getBenchValue("m_dirtyChunks", "chunks"), 0f, pShowBar: false, 0L);
				pTool.setText("new regions:", Bench.getBenchValue("m_newRegions", "chunks"), 0f, pShowBar: false, 0L);
				pTool.setText("new links:", Bench.getBenchValue("m_newLinks", "chunks"), 0f, pShowBar: false, 0L);
				pTool.setText("new islands:", Bench.getBenchValue("m_newIslands", "chunks"), 0f, pShowBar: false, 0L);
				pTool.setText("last dirty islands:", Bench.getBenchValue("m_dirtyIslands", "chunks"), 0f, pShowBar: false, 0L);
				pTool.setText("last dirty corners:", Bench.getBenchValue("m_dirtyCorners", "chunks"), 0f, pShowBar: false, 0L);
				pTool.setText("dirty islands neighb:", Bench.getBenchValue("m_dirtyIslandNeighb", "chunks"), 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("########### last_bench:", null, 0f, pShowBar: false, 0L);
			},
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				pTool.show_averages = false;
				pTool.show_counter = true;
				pTool.show_max = false;
				pTool.hide_zeroes = false;
				pTool.state = DebugToolState.Percent;
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Quantum Sprites",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "quantum_sprites",
			benchmark_total = "quantum_sprites",
			benchmark_total_group = "game_total",
			split_benchmark = true,
			action_start = setBenchmarksDefaultValue,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Cache Manager",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "world_cache_manager",
			benchmark_total = "world_cache_manager",
			benchmark_total_group = "game_total",
			split_benchmark = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = false;
				pTool.hide_zeroes = false;
				pTool.show_max = false;
				pTool.state = DebugToolState.Values;
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Sim Zones",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "sim_zones",
			benchmark_total = "sim_zones",
			benchmark_total_group = "game_total",
			split_benchmark = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = false;
				pTool.hide_zeroes = false;
				pTool.show_max = false;
				pTool.state = DebugToolState.Values;
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark MusicBox",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "music_box",
			benchmark_total = "music_box",
			benchmark_total_group = "music_box_total",
			split_benchmark = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = false;
				pTool.hide_zeroes = false;
				pTool.show_max = false;
				pTool.state = DebugToolState.Values;
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Nameplates",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "nameplates",
			benchmark_total = "nameplates",
			benchmark_total_group = "nameplates_total",
			split_benchmark = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = false;
				pTool.hide_zeroes = false;
				pTool.show_max = false;
				pTool.state = DebugToolState.FrameBudget;
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Borderers Renderer",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "borders_renderer",
			benchmark_total = "borders_renderer",
			benchmark_total_group = "borders_renderer_total",
			split_benchmark = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = false;
				pTool.hide_zeroes = false;
				pTool.show_max = false;
				pTool.state = DebugToolState.FrameBudget;
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Fluid Zones Data",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "fluid_zones_data",
			benchmark_total = "fluid_zones_data",
			benchmark_total_group = "fluid_zones_data_total",
			split_benchmark = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = false;
				pTool.hide_zeroes = false;
				pTool.show_max = false;
				pTool.state = DebugToolState.FrameBudget;
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark World Beh",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "world_beh",
			benchmark_total = "world_beh",
			benchmark_total_group = "game_total",
			action_start = setBenchmarksDefaultValue,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Buildings",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "buildings",
			benchmark_total = "buildings",
			benchmark_total_group = "game_total",
			split_benchmark = true,
			action_start = setBenchmarksDefaultValue,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom
		});
		DebugToolAsset debugToolAsset = t;
		debugToolAsset.action_1 = (DebugToolAssetAction)Delegate.Combine(debugToolAsset.action_1, (DebugToolAssetAction)delegate(DebugTool pTool)
		{
			JobManagerBuildings jobManager = World.world.buildings.getJobManager();
			pTool.setText("batches total/free:", jobManager.debugBatchCount(), 0f, pShowBar: false, 0L);
			pTool.setText("active jobs:", jobManager.debugJobCount(), 0f, pShowBar: false, 0L);
			pTool.setSeparator();
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Actors",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "actors",
			benchmark_total = "actors",
			benchmark_total_group = "game_total",
			split_benchmark = true,
			action_start = setBenchmarksDefaultValue,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom
		});
		DebugToolAsset debugToolAsset2 = t;
		debugToolAsset2.action_1 = (DebugToolAssetAction)Delegate.Combine(debugToolAsset2.action_1, (DebugToolAssetAction)delegate(DebugTool pTool)
		{
			JobManagerActors jobManager = World.world.units.getJobManager();
			pTool.setText("batches total/free:", jobManager.debugBatchCount(), 0f, pShowBar: false, 0L);
			pTool.setText("active jobs:", jobManager.debugJobCount(), 0f, pShowBar: false, 0L);
			pTool.setSeparator();
		});
		add(new DebugToolAsset
		{
			id = "Benchmark AI Actions",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "ai_actions",
			benchmark_total = "ai_actions",
			benchmark_total_group = "ai_actions_total",
			split_benchmark = true,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_max = false;
				pTool.show_averages = true;
				pTool.hide_zeroes = true;
				pTool.show_max = false;
				pTool.sort_by_names = false;
				pTool.sort_by_values = true;
				pTool.state = DebugToolState.Values;
			},
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom
		});
		t.show_on_start = DebugConfig.isOn(DebugOption.BenchAiEnabled);
		add(new DebugToolAsset
		{
			id = "Benchmark AI Tasks",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "ai_tasks",
			benchmark_total = "ai_tasks",
			benchmark_total_group = "ai_tasks_total",
			split_benchmark = true,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_max = false;
				pTool.show_averages = true;
				pTool.hide_zeroes = true;
				pTool.show_max = false;
				pTool.sort_by_names = false;
				pTool.sort_by_values = true;
				pTool.state = DebugToolState.Values;
			},
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom
		});
		t.show_on_start = DebugConfig.isOn(DebugOption.BenchAiEnabled);
		add(new DebugToolAsset
		{
			id = "$benchmark_loops$",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "loops_test_100",
			benchmark_total = "loops_test_100",
			benchmark_total_group = "loops_test_total_100",
			show_last_count = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = true;
				pTool.show_averages = true;
				pTool.hide_zeroes = false;
				pTool.show_max = true;
				pTool.sort_by_names = false;
				pTool.sort_by_values = true;
				pTool.state = DebugToolState.Values;
			},
			action_update = delegate(DebugTool pTool)
			{
				BenchmarkLoops.update(pTool.asset);
			}
		});
		clone("Benchmark Loops 10", "$benchmark_loops$");
		t.benchmark_group_id = "loops_test_10";
		t.benchmark_total = "loops_test_10";
		t.benchmark_total_group = "loops_test_total_10";
		DebugToolAsset debugToolAsset3 = t;
		debugToolAsset3.action_start = (DebugToolAssetAction)Delegate.Combine(debugToolAsset3.action_start, (DebugToolAssetAction)delegate(DebugTool pTool)
		{
			new BenchmarkLoops(pTool.asset, 10);
		});
		clone("Benchmark Loops 100", "$benchmark_loops$");
		t.benchmark_group_id = "loops_test_100";
		t.benchmark_total = "loops_test_100";
		t.benchmark_total_group = "loops_test_total_100";
		DebugToolAsset debugToolAsset4 = t;
		debugToolAsset4.action_start = (DebugToolAssetAction)Delegate.Combine(debugToolAsset4.action_start, (DebugToolAssetAction)delegate(DebugTool pTool)
		{
			new BenchmarkLoops(pTool.asset, 100);
		});
		clone("Benchmark Loops 1000", "$benchmark_loops$");
		t.benchmark_group_id = "loops_test_1000";
		t.benchmark_total = "loops_test_1000";
		t.benchmark_total_group = "loops_test_total_1000";
		DebugToolAsset debugToolAsset5 = t;
		debugToolAsset5.action_start = (DebugToolAssetAction)Delegate.Combine(debugToolAsset5.action_start, (DebugToolAssetAction)delegate(DebugTool pTool)
		{
			new BenchmarkLoops(pTool.asset, 1000);
		});
		clone("Benchmark Loops 10000", "$benchmark_loops$");
		t.benchmark_group_id = "loops_test_10000";
		t.benchmark_total = "loops_test_10000";
		t.benchmark_total_group = "loops_test_total_10000";
		DebugToolAsset debugToolAsset6 = t;
		debugToolAsset6.action_start = (DebugToolAssetAction)Delegate.Combine(debugToolAsset6.action_start, (DebugToolAssetAction)delegate(DebugTool pTool)
		{
			new BenchmarkLoops(pTool.asset, 10000);
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Distance",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "dist_test",
			benchmark_total = "dist_test",
			benchmark_total_group = "dist_test_total",
			show_last_count = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = true;
				pTool.show_averages = false;
				pTool.hide_zeroes = false;
				pTool.show_max = true;
				pTool.sort_by_names = false;
				pTool.sort_by_values = true;
				pTool.state = DebugToolState.Percent;
				new BenchmarkDist();
			},
			action_update = delegate
			{
				BenchmarkDist.update();
			}
		});
		clone("$benchmark_shuffle_loops$", "$benchmark_loops$");
		t.action_update = delegate(DebugTool pTool)
		{
			BenchmarkShuffle.update(pTool.asset);
		};
		clone("Benchmark Shuffle Loops 10", "$benchmark_shuffle_loops$");
		t.benchmark_group_id = "shuffle_test_10";
		t.benchmark_total = "shuffle_test_10";
		t.benchmark_total_group = "shuffle_test_total_10";
		DebugToolAsset debugToolAsset7 = t;
		debugToolAsset7.action_start = (DebugToolAssetAction)Delegate.Combine(debugToolAsset7.action_start, (DebugToolAssetAction)delegate(DebugTool pTool)
		{
			new BenchmarkShuffle(pTool.asset, 10, 50);
		});
		clone("Benchmark Shuffle Loops 100", "$benchmark_shuffle_loops$");
		t.benchmark_group_id = "shuffle_test_100";
		t.benchmark_total = "shuffle_test_100";
		t.benchmark_total_group = "shuffle_test_total_100";
		DebugToolAsset debugToolAsset8 = t;
		debugToolAsset8.action_start = (DebugToolAssetAction)Delegate.Combine(debugToolAsset8.action_start, (DebugToolAssetAction)delegate(DebugTool pTool)
		{
			new BenchmarkShuffle(pTool.asset, 100, 500);
		});
		clone("Benchmark Shuffle Loops 1000", "$benchmark_shuffle_loops$");
		t.benchmark_group_id = "shuffle_test_1000";
		t.benchmark_total = "shuffle_test_1000";
		t.benchmark_total_group = "shuffle_test_total_1000";
		DebugToolAsset debugToolAsset9 = t;
		debugToolAsset9.action_start = (DebugToolAssetAction)Delegate.Combine(debugToolAsset9.action_start, (DebugToolAssetAction)delegate(DebugTool pTool)
		{
			new BenchmarkShuffle(pTool.asset, 1000, 5000);
		});
		clone("Benchmark Shuffle Loops 10000", "$benchmark_shuffle_loops$");
		t.benchmark_group_id = "shuffle_test_10000";
		t.benchmark_total = "shuffle_test_10000";
		t.benchmark_total_group = "shuffle_test_total_10000";
		DebugToolAsset debugToolAsset10 = t;
		debugToolAsset10.action_start = (DebugToolAssetAction)Delegate.Combine(debugToolAsset10.action_start, (DebugToolAssetAction)delegate(DebugTool pTool)
		{
			new BenchmarkShuffle(pTool.asset, 10000, 25000);
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Field Acess",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "field_acess_test",
			benchmark_total = "field_acess_test",
			benchmark_total_group = "field_acess_total",
			split_benchmark = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = true;
				pTool.show_averages = false;
				pTool.hide_zeroes = false;
				pTool.show_max = true;
				pTool.sort_by_names = false;
				pTool.sort_by_values = true;
				pTool.state = DebugToolState.Values;
			},
			action_update = delegate
			{
				BenchmarkFieldAccess.start();
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Sprites",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "sprites_test",
			benchmark_total = "sprites_test",
			benchmark_total_group = "sprites_test_total",
			split_benchmark = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = true;
				pTool.show_averages = false;
				pTool.hide_zeroes = false;
				pTool.show_max = true;
				pTool.sort_by_names = false;
				pTool.sort_by_values = true;
				pTool.state = DebugToolState.Values;
			},
			action_update = delegate
			{
				BenchmarkSprites.start();
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Struct Loops",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "loops_struct_test",
			benchmark_total = "loops_struct_test",
			benchmark_total_group = "loops_struct_test_total",
			split_benchmark = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = true;
				pTool.show_averages = false;
				pTool.hide_zeroes = false;
				pTool.show_max = true;
				pTool.sort_by_names = false;
				pTool.sort_by_values = true;
				pTool.state = DebugToolState.Values;
			},
			action_update = delegate
			{
				BenchmarkStructLoops.start();
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark ECS",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "ecs_test",
			benchmark_total = "ecs_test",
			benchmark_total_group = "ecs_test_total",
			split_benchmark = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = true;
				pTool.show_averages = false;
				pTool.hide_zeroes = false;
				pTool.show_max = true;
				pTool.sort_by_names = true;
				pTool.state = DebugToolState.Percent;
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Blacklist",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "blacklist_test",
			benchmark_total = "blacklist_test",
			benchmark_total_group = "blacklist_test_total",
			split_benchmark = true,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom,
			action_start = delegate(DebugTool pTool)
			{
				setBenchmarksDefaultValue(pTool);
				pTool.show_counter = true;
				pTool.show_averages = true;
				pTool.hide_zeroes = false;
				pTool.show_max = true;
				pTool.sort_by_names = false;
				pTool.sort_by_values = true;
				pTool.state = DebugToolState.TimeSpent;
			},
			action_update = delegate
			{
				BenchmarkBlacklist.start();
			}
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Trait Effects",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "effects_traits",
			benchmark_total = "effects_traits",
			benchmark_total_group = "game_total",
			split_benchmark = true,
			action_start = setBenchmarksDefaultValue,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom
		});
		add(new DebugToolAsset
		{
			id = "Benchmark Item Effects",
			show_benchmark_buttons = true,
			type = DebugToolType.Benchmarks,
			benchmark_group_id = "effects_items",
			benchmark_total = "effects_items",
			benchmark_total_group = "game_total",
			split_benchmark = true,
			action_start = setBenchmarksDefaultValue,
			action_1 = showGroupBenchmarkTop,
			action_2 = showGroupBenchmarkBottom
		});
		add(new DebugToolAsset
		{
			id = "Benchmark",
			type = DebugToolType.Benchmarks,
			priority = 2,
			action_1 = delegate(DebugTool pTool)
			{
				pTool.setText("CityBehCheckSettleTarget_tick:", Bench.getBenchResult("CityBehCheckSettleTarget", "main", pAverage: false), 0f, pShowBar: false, 0L);
				pTool.setSeparator();
				pTool.setText("test_follow:", Bench.getBenchResult("test_follow"), 0f, pShowBar: false, 0L);
			}
		});
	}

	private void setBenchmarksDefaultValue(DebugTool pTool)
	{
		pTool.sort_order_reversed = false;
		pTool.sort_by_names = false;
		pTool.sort_by_values = false;
		pTool.show_averages = true;
		pTool.hide_zeroes = true;
		pTool.show_counter = true;
		pTool.show_max = true;
		pTool.state = DebugToolState.FrameBudget;
		pTool.paused = false;
		pTool.percentage_slowest = false;
		if (Config.editor_mastef)
		{
			DebugConfig.debugToolMastefDefaults(pTool);
		}
	}

	private void showGroupBenchmarkTop(DebugTool pTool)
	{
		float deltaTime = Time.deltaTime;
		double totalFrameBudget = getTotalFrameBudget();
		double benchResultAsDouble = Bench.getBenchResultAsDouble("game_total", "main", pTool.isValueAverage());
		pTool.setText("game total:", trim(benchResultAsDouble, pAddMS: true), 0f, pShowBar: false, 0L);
		pTool.setText("fps:", FPS.getFPS(), 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		double benchResultAsDouble2 = Bench.getBenchResultAsDouble(pTool.asset.benchmark_total, pTool.asset.benchmark_total_group, pTool.isValueAverage());
		if (pTool.asset.benchmark_total != "game_total")
		{
			pTool.setText("group total:", trim(benchResultAsDouble2, pAddMS: true), 100f, pShowBar: true, 0L);
			double num = benchResultAsDouble2 / benchResultAsDouble * 100.0;
			pTool.setText("usage from total:", trimPercent(num), (float)num, pShowBar: true, 0L);
		}
		else
		{
			pTool.setSeparator();
			pTool.setSeparator();
		}
		double num2 = benchResultAsDouble2 / (double)deltaTime * 100.0;
		pTool.setText("total frame time spent:", trimPercent(num2), (float)num2, pShowBar: true, 0L);
		double num3 = benchResultAsDouble2 * 1000.0 / totalFrameBudget * 100.0;
		pTool.setText("total budget time spent:", trimPercent(num3), (float)num3, pShowBar: true, 0L);
		pTool.setSeparator();
	}

	private void showGroupBenchmarkBottom(DebugTool pTool)
	{
		double totalFrameBudget = getTotalFrameBudget();
		float deltaTime = Time.deltaTime;
		List<ToolBenchmarkData> list = new List<ToolBenchmarkData>(Bench.getGroup(pTool.asset.benchmark_group_id).dict_data.Values);
		if (!pTool.percentage_slowest)
		{
			double benchResultAsDouble = Bench.getBenchResultAsDouble(pTool.asset.benchmark_total, pTool.asset.benchmark_total_group, pTool.isValueAverage());
			foreach (ToolBenchmarkData item in list)
			{
				double num = item.latest_result;
				if (pTool.isValueAverage())
				{
					num = item.getAverage();
				}
				double calculated_percentage = num / benchResultAsDouble * 100.0;
				item.calculated_percentage = calculated_percentage;
			}
		}
		else
		{
			double num2 = 0.0;
			foreach (ToolBenchmarkData item2 in list)
			{
				double num3 = (pTool.isValueAverage() ? item2.getAverage() : item2.latest_result);
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
			foreach (ToolBenchmarkData item3 in list)
			{
				double num4 = (pTool.isValueAverage() ? item3.getAverage() : item3.latest_result) / num2 * 100.0;
				if (((float)num4).Equals(100f))
				{
					num4++;
				}
				item3.calculated_percentage = num4;
			}
		}
		if (pTool.sort_by_names)
		{
			list.Sort((ToolBenchmarkData a, ToolBenchmarkData b) => b.id.CompareTo(a.id));
		}
		else if (pTool.isState(DebugToolState.Percent))
		{
			list.Sort((ToolBenchmarkData a, ToolBenchmarkData b) => a.calculated_percentage.CompareTo(b.calculated_percentage));
		}
		else if (pTool.isValueAverage())
		{
			list.Sort((ToolBenchmarkData a, ToolBenchmarkData b) => a.getAverage().CompareTo(b.getAverage()));
		}
		else
		{
			list.Sort((ToolBenchmarkData a, ToolBenchmarkData b) => a.latest_result.CompareTo(b.latest_result));
		}
		if (!pTool.sort_order_reversed)
		{
			list.Reverse();
		}
		foreach (ToolBenchmarkData item4 in list)
		{
			double num5 = item4.latest_result;
			if (pTool.isValueAverage())
			{
				num5 = item4.getAverage();
			}
			long pCounter = 0L;
			bool pShowCounter = false;
			bool show_max = pTool.show_max;
			string pMaxValue = string.Empty;
			if (pTool.asset.split_benchmark && pTool.show_counter)
			{
				pCounter = item4.getAverageCount();
				pShowCounter = true;
			}
			else if (pTool.asset.show_last_count && pTool.show_counter)
			{
				pCounter = item4.getLastCount();
				pShowCounter = true;
			}
			string pT = string.Empty;
			string pT2 = string.Empty;
			double num6 = 0.0;
			switch (pTool.state)
			{
			case DebugToolState.Percent:
				if (pTool.hide_zeroes && item4.calculated_percentage < 0.1)
				{
					continue;
				}
				pT = item4.id + ":";
				pT2 = trimPercent(item4.calculated_percentage);
				num6 = item4.calculated_percentage;
				item4.saveLastMaxValue(item4.calculated_percentage);
				pMaxValue = trimPercent(item4.last_max_value);
				break;
			case DebugToolState.Values:
				if (pTool.hide_zeroes && num5 < 1E-06)
				{
					continue;
				}
				pT = item4.id + ":";
				pT2 = trim(num5);
				num6 = item4.calculated_percentage;
				item4.saveLastMaxValue(num5);
				pMaxValue = trim(item4.last_max_value);
				break;
			case DebugToolState.FrameBudget:
			{
				double num8 = num5 * 1000.0 / totalFrameBudget * 100.0;
				if (pTool.hide_zeroes && num8 < 0.1)
				{
					continue;
				}
				pT = item4.id + ":";
				pT2 = trimPercent(num8);
				num6 = num8;
				item4.saveLastMaxValue(num8);
				pMaxValue = trimPercent(item4.last_max_value);
				break;
			}
			case DebugToolState.TimeSpent:
			{
				double num7 = num5 / (double)deltaTime * 100.0;
				if (pTool.hide_zeroes && num7 < 0.1)
				{
					continue;
				}
				pT = item4.id + ":";
				pT2 = trimPercent(num7);
				num6 = num7;
				item4.saveLastMaxValue(num7);
				pMaxValue = trimPercent(item4.last_max_value);
				break;
			}
			}
			pTool.setText(pT, pT2, (float)num6, pShowBar: true, pCounter, pShowCounter, show_max, pMaxValue);
		}
	}

	private string trim(double pValue, bool pAddMS = false)
	{
		pValue *= 1000.0;
		string text = pValue.ToString("F5");
		if (pAddMS)
		{
			text += " ms";
		}
		return text;
	}

	private string trimPercent(double pValue, bool pAddPercent = true)
	{
		string text = pValue.ToString("F1");
		if (pAddPercent)
		{
			text += "%";
		}
		return text;
	}

	private double getTotalFrameBudget()
	{
		double num = 60.0;
		if (Config.fps_lock_30)
		{
			num = 30.0;
		}
		return 1000.0 / num * 0.6499999761581421;
	}
}
