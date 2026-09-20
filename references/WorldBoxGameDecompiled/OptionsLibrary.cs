using SleekRender;
using UnityEngine;

public class OptionsLibrary : AssetLibrary<OptionAsset>
{
	public override void init()
	{
		base.init();
		initGameplayOptions();
		initLayerOptions();
		initAppOptions();
		initQualityOptions();
		initOtherOptions();
		initHotkeyOptions();
	}

	private void initLayerOptions()
	{
		add(new OptionAsset
		{
			id = "map_layers",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "map_species_families",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "map_kingdom_layer",
			default_int = 0,
			max_value = 1,
			multi_toggle = true,
			type = OptionType.Bool,
			locale_options_ids = AssetLibrary<OptionAsset>.a<string>("ui_zone_mode_borders", "ui_zone_mode_units")
		});
		add(new OptionAsset
		{
			id = "map_city_layer",
			default_int = 1,
			max_value = 1,
			multi_toggle = true,
			type = OptionType.Bool,
			locale_options_ids = AssetLibrary<OptionAsset>.a<string>("ui_zone_mode_borders", "ui_zone_mode_units")
		});
		add(new OptionAsset
		{
			id = "map_clan_layer",
			default_int = 0,
			max_value = 2,
			multi_toggle = true,
			type = OptionType.Bool,
			locale_options_ids = AssetLibrary<OptionAsset>.a<string>("ui_zone_mode_kingdoms", "ui_zone_mode_cities", "ui_zone_mode_units")
		});
		add(new OptionAsset
		{
			id = "map_religion_layer",
			default_int = 2,
			max_value = 2,
			multi_toggle = true,
			type = OptionType.Bool,
			locale_options_ids = AssetLibrary<OptionAsset>.a<string>("ui_zone_mode_kingdoms", "ui_zone_mode_cities", "ui_zone_mode_units")
		});
		add(new OptionAsset
		{
			id = "map_culture_layer",
			default_int = 2,
			max_value = 2,
			multi_toggle = true,
			type = OptionType.Bool,
			locale_options_ids = AssetLibrary<OptionAsset>.a<string>("ui_zone_mode_kingdoms", "ui_zone_mode_cities", "ui_zone_mode_units")
		});
		add(new OptionAsset
		{
			id = "map_subspecies_layer",
			default_int = 2,
			max_value = 2,
			multi_toggle = true,
			type = OptionType.Bool,
			locale_options_ids = AssetLibrary<OptionAsset>.a<string>("ui_zone_mode_kingdoms", "ui_zone_mode_cities", "ui_zone_mode_units")
		});
		add(new OptionAsset
		{
			id = "map_family_layer",
			default_int = 2,
			max_value = 2,
			multi_toggle = true,
			type = OptionType.Bool,
			locale_options_ids = AssetLibrary<OptionAsset>.a<string>("ui_zone_mode_kingdoms", "ui_zone_mode_cities", "ui_zone_mode_units")
		});
		add(new OptionAsset
		{
			id = "map_language_layer",
			default_int = 2,
			max_value = 2,
			multi_toggle = true,
			type = OptionType.Bool,
			locale_options_ids = AssetLibrary<OptionAsset>.a<string>("ui_zone_mode_kingdoms", "ui_zone_mode_cities", "ui_zone_mode_units")
		});
		add(new OptionAsset
		{
			id = "map_alliance_layer",
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "map_army_layer",
			default_bool = false,
			type = OptionType.Bool
		});
	}

	private void initGameplayOptions()
	{
		add(new OptionAsset
		{
			id = "map_names",
			default_bool = true,
			default_int = 0,
			max_value = 1,
			multi_toggle = true,
			type = OptionType.Bool,
			locale_options_ids = AssetLibrary<OptionAsset>.a<string>("ui_map_names_normal", "ui_map_names_banners_only")
		});
		add(new OptionAsset
		{
			id = "map_kings_leaders",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "marks_favorites",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "marks_favorite_items",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "marks_plots",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "marks_wars",
			default_bool = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "marks_armies",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "only_favorited_meta",
			default_bool = false,
			reset_to_default_on_launch = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "money_flow",
			default_bool = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "icons_happiness",
			default_bool = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "icons_tasks",
			default_bool = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "talk_bubbles",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "meta_conversions",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "unit_metas",
			default_bool = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "marks_battles",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "history_log",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "marks_boats",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "army_targets",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "tooltip_zones",
			default_bool = true,
			default_bool_mobile = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "tooltip_units",
			default_bool = true,
			default_bool_mobile = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "cursor_arrow_destination",
			default_bool = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "cursor_arrow_house",
			default_bool = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "cursor_arrow_lover",
			default_bool = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "cursor_arrow_family",
			default_bool = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "cursor_arrow_parents",
			default_bool = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "cursor_arrow_kids",
			default_bool = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "cursor_arrow_attack_target",
			default_bool = false,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "highlight_kingdom_enemies",
			default_bool = true,
			type = OptionType.Bool,
			override_bool_mobile = true,
			default_bool_mobile = false
		});
	}

	private void initAppOptions()
	{
		add(new OptionAsset
		{
			id = "autorotation",
			translation_key = "autorotation",
			translation_key_description = "option_description_autorotation",
			update_all_elements_after_click = true,
			default_bool = false,
			type = OptionType.Bool,
			action = delegate(OptionAsset pAsset)
			{
				Config.setAutorotation(getSavedBool(pAsset.id));
			}
		});
		add(new OptionAsset
		{
			id = "portrait",
			translation_key = "portrait_mode",
			translation_key_description = "option_description_portrait",
			default_bool = true,
			type = OptionType.Bool,
			action = delegate(OptionAsset pAsset)
			{
				Config.setPortrait(getSavedBool(pAsset.id));
			}
		});
		add(new OptionAsset
		{
			id = "fps_lock_30",
			translation_key_description = "option_description_fps_lock_30",
			default_bool = false,
			type = OptionType.Bool,
			update_all_elements_after_click = true,
			action = delegate(OptionAsset pAsset)
			{
				bool num = (Config.fps_lock_30 = getSavedBool(pAsset.id));
				if (Config.fps_lock_30)
				{
					Application.targetFrameRate = 30;
				}
				else
				{
					Application.targetFrameRate = 60;
				}
				if (num)
				{
					forceDisableOption("vsync");
				}
			}
		});
		add(new OptionAsset
		{
			id = "sound",
			translation_key = "sound_effects",
			translation_key_description = "option_description_sound_effects",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "volume_master_sound",
			translation_key_description = "option_description_volume_master_sound",
			default_int = 100,
			type = OptionType.Int,
			max_value = 100,
			min_value = 0,
			counter_percent = true
		});
		add(new OptionAsset
		{
			id = "volume_music",
			translation_key_description = "option_description_volume_music",
			default_int = 100,
			type = OptionType.Int,
			max_value = 100,
			min_value = 0,
			counter_percent = true
		});
		add(new OptionAsset
		{
			id = "volume_sound_effects",
			translation_key_description = "option_description_volume_sound_effects",
			default_int = 100,
			type = OptionType.Int,
			max_value = 100,
			min_value = 0,
			counter_percent = true
		});
		add(new OptionAsset
		{
			id = "volume_ui",
			translation_key_description = "option_description_volume_ui",
			default_int = 100,
			type = OptionType.Int,
			max_value = 100,
			min_value = 0,
			counter_percent = true
		});
		add(new OptionAsset
		{
			id = "music",
			translation_key_description = "option_description_music",
			translation_key = "music",
			default_bool = true,
			type = OptionType.Bool
		});
		add(new OptionAsset
		{
			id = "vsync",
			translation_key = "vsync",
			translation_key_description = "option_description_vsync",
			default_bool = false,
			type = OptionType.Bool,
			update_all_elements_after_click = true,
			computer_only = true,
			action = delegate(OptionAsset pAsset)
			{
				bool savedBool = getSavedBool(pAsset.id);
				PlayerConfig.setVsync(savedBool);
				if (savedBool)
				{
					forceDisableOption("fps_lock_30");
				}
			}
		});
		add(new OptionAsset
		{
			id = "fullscreen",
			default_bool = true,
			translation_key_description = "option_description_fullscreen",
			type = OptionType.Bool,
			computer_only = true,
			action = delegate(OptionAsset pAsset)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0040: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				Config.full_screen = getSavedBool(pAsset.id);
				if (Config.full_screen)
				{
					Resolution currentResolution = Screen.currentResolution;
					int width = ((Resolution)(ref currentResolution)).width;
					currentResolution = Screen.currentResolution;
					int height = ((Resolution)(ref currentResolution)).height;
					currentResolution = Screen.currentResolution;
					RefreshRate refreshRateRatio = ((Resolution)(ref currentResolution)).refreshRateRatio;
					Screen.SetResolution(width, height, (FullScreenMode)1, refreshRateRatio);
				}
				else
				{
					Screen.fullScreen = false;
				}
			}
		});
		add(new OptionAsset
		{
			id = "language",
			type = OptionType.String,
			default_string = "en"
		});
		add(new OptionAsset
		{
			id = "username",
			type = OptionType.String,
			default_string = "",
			has_locales = false
		});
		add(new OptionAsset
		{
			id = "wbb_confirmed",
			type = OptionType.Bool,
			default_bool = false,
			has_locales = false,
			action = delegate(OptionAsset pAsset)
			{
				Config.wbb_confirmed = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "ui_size_main",
			translation_key = "power_bar_size",
			translation_key_description = "option_description_power_bar_size",
			type = OptionType.Int,
			default_int = 100,
			max_value = 150,
			min_value = 50,
			action = delegate
			{
				CanvasMain.instance.resizeMainUI();
				PowerButtonSelector.instance.showBarTemporary();
			},
			counter_format = getCounterFormat100Percent
		});
		add(new OptionAsset
		{
			id = "ui_size_windows",
			translation_key_description = "option_description_ui_windows_size",
			type = OptionType.Int,
			default_int = 100,
			max_value = 115,
			min_value = 30,
			action = delegate
			{
				CanvasMain.instance.resizeWindowsUI();
				MetaSwitchManager.checkAndRefresh();
				HoveringBgIconManager.dropAll();
			},
			counter_format = getCounterFormat100Percent
		});
		add(new OptionAsset
		{
			id = "ui_size_tooltips",
			translation_key_description = "option_description_ui_tooltips_size",
			type = OptionType.Int,
			default_int = 100,
			max_value = 150,
			min_value = 30,
			action = delegate
			{
				CanvasMain.instance.resizeTooltipUI();
			},
			counter_format = getCounterFormat100Percent
		});
		add(new OptionAsset
		{
			id = "ui_size_map_names",
			translation_key_description = "option_description_ui_map_names_size",
			type = OptionType.Int,
			default_int = 70,
			max_value = 160,
			min_value = 30,
			action = delegate
			{
				CanvasMain.instance.resizeMapNames();
				World.world.nameplate_manager.clearCaches();
			},
			counter_format = getCounterFormat100Percent
		});
		add(new OptionAsset
		{
			id = "ui_map_border_opacity",
			translation_key_description = "option_description_map_border_opacity",
			type = OptionType.Int,
			default_int = 88,
			max_value = 100,
			min_value = 30,
			action = delegate(OptionAsset pAsset)
			{
				float minimap_opacity = (float)getSavedInt(pAsset.id) / 100f;
				World.world.zone_calculator.minimap_opacity = minimap_opacity;
			},
			counter_format = getCounterFormat100Percent
		});
		add(new OptionAsset
		{
			id = "ui_map_border_brightness",
			translation_key_description = "option_description_map_border_brightness",
			type = OptionType.Int,
			default_int = 100,
			max_value = 100,
			min_value = 60,
			action = delegate(OptionAsset pAsset)
			{
				float border_brightness = (float)getSavedInt(pAsset.id) / 100f;
				World.world.zone_calculator.border_brightness = border_brightness;
			},
			counter_format = getCounterFormat100Percent
		});
	}

	private void initQualityOptions()
	{
		add(new OptionAsset
		{
			id = "vignette",
			translation_key_description = "option_description_vignette",
			default_bool = true,
			type = OptionType.Bool,
			action = delegate(OptionAsset pAsset)
			{
				((Component)Camera.main).GetComponent<SleekRenderPostProcess>().settings.vignetteEnabled = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "bloom",
			translation_key_description = "option_description_bloom",
			default_bool = true,
			type = OptionType.Bool,
			action = delegate(OptionAsset pAsset)
			{
				((Component)Camera.main).GetComponent<SleekRenderPostProcess>().settings.bloomEnabled = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "fire",
			translation_key_description = "option_description_fire",
			default_bool = true,
			type = OptionType.Bool,
			action = delegate(OptionAsset pAsset)
			{
				((Behaviour)World.world.particles_fire).enabled = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "smoke",
			translation_key_description = "option_description_smoke",
			default_bool = true,
			type = OptionType.Bool,
			action = delegate(OptionAsset pAsset)
			{
				((Behaviour)World.world.particles_smoke).enabled = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "shadows",
			translation_key_description = "option_description_shadows",
			type = OptionType.Bool,
			default_bool = true,
			action = delegate(OptionAsset pAsset)
			{
				Config.shadows_active = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "tree_wind",
			translation_key_description = "option_description_tree_wind",
			type = OptionType.Bool,
			default_bool = true,
			action = delegate(OptionAsset pAsset)
			{
				BuildingRendererSettings.wobbly_material_enabled = getSavedBool(pAsset.id);
				World.world.buildings.checkWobblySetting();
			}
		});
		add(new OptionAsset
		{
			id = "minimap_transition_animation",
			translation_key_description = "option_description_minimap_transition_animation",
			type = OptionType.Bool,
			default_bool = true
		});
		add(new OptionAsset
		{
			id = "night_lights",
			translation_key_description = "option_description_night_lights",
			type = OptionType.Bool,
			default_bool = true
		});
		add(new OptionAsset
		{
			id = "cursor_lights",
			translation_key_description = "option_description_cursor_lights",
			type = OptionType.Bool,
			default_bool = true,
			default_bool_mobile = false
		});
		add(new OptionAsset
		{
			id = "age_particles",
			translation_key_description = "option_description_age_particles",
			type = OptionType.Bool,
			default_bool = true,
			action = delegate(OptionAsset pAsset)
			{
				WorldAgesParticles.effects_enabled = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "age_overlay_effect",
			translation_key_description = "option_description_age_overlay_effect",
			type = OptionType.Int,
			default_int = 100,
			max_value = 100,
			min_value = 0,
			counter_format = getCounterFormat100Percent
		});
		add(new OptionAsset
		{
			id = "age_night_effect",
			translation_key_description = "option_description_age_night_effect",
			type = OptionType.Int,
			default_int = 100,
			max_value = 100,
			min_value = 50,
			counter_format = getCounterFormat100Percent
		});
	}

	private string getCounterFormat100Percent(OptionAsset pAsset)
	{
		return PlayerConfig.getIntValue(pAsset.id) + "%";
	}

	private void initOtherOptions()
	{
		add(new OptionAsset
		{
			id = "preload_windows",
			translation_key_description = "option_description_preload_windows",
			default_bool = true,
			type = OptionType.Bool,
			override_bool_mobile = true,
			default_bool_mobile = false,
			action = delegate(OptionAsset pAsset)
			{
				Config.preload_windows = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "preload_quantum_sprites",
			translation_key_description = "option_description_preload_quantum_sprites",
			default_bool = true,
			type = OptionType.Bool,
			override_bool_mobile = true,
			default_bool_mobile = false,
			action = delegate(OptionAsset pAsset)
			{
				Config.preload_quantum_sprites = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "preload_buildings",
			translation_key_description = "option_description_preload_buildings",
			default_bool = true,
			type = OptionType.Bool,
			override_bool_mobile = true,
			default_bool_mobile = false,
			action = delegate(OptionAsset pAsset)
			{
				Config.preload_buildings = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "preload_units",
			translation_key_description = "option_description_preload_units",
			default_bool = true,
			type = OptionType.Bool,
			override_bool_mobile = true,
			default_bool_mobile = false,
			action = delegate(OptionAsset pAsset)
			{
				Config.preload_units = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "autosaves",
			translation_key_description = "option_description_autosaves",
			default_bool = true,
			type = OptionType.Bool,
			action = delegate(OptionAsset pAsset)
			{
				Config.autosaves = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "graphs",
			translation_key_description = "option_description_graphs",
			default_bool = true,
			type = OptionType.Bool,
			action = delegate(OptionAsset pAsset)
			{
				Config.graphs = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "tooltips",
			translation_key_description = "option_description_tooltips",
			default_bool = true,
			type = OptionType.Bool,
			override_bool_mobile = true,
			default_bool_mobile = false,
			action = delegate(OptionAsset pAsset)
			{
				Config.tooltips_active = getSavedBool(pAsset.id);
			}
		});
		add(new OptionAsset
		{
			id = "experimental",
			translation_key_description = "option_description_experimental",
			translation_key_description_2 = "option_description_experimental_warning",
			default_bool = false,
			type = OptionType.Bool,
			action = delegate(OptionAsset pAsset)
			{
				bool flag = getSavedBool(pAsset.id);
				if (flag)
				{
					string savedString = getSavedString("last_used_version");
					if (savedString != Application.version)
					{
						flag = false;
						Debug.LogWarning((object)("Last version and current version differ, disabling experimental mode " + savedString + " " + Application.version));
						setSavedBool(pAsset.id, flag);
					}
					else
					{
						Debug.Log((object)"Experimental mode is enabled");
						WorldTip.instance.showToolbarText("Experimental mode is enabled");
					}
				}
				Config.experimental_mode = flag;
			}
		});
		add(new OptionAsset
		{
			id = "last_used_version",
			default_string = "",
			type = OptionType.String,
			has_locales = false,
			action = delegate(OptionAsset pAsset)
			{
				PlayerConfig.setOptionString(pAsset.id, Application.version);
			}
		});
	}

	private void initHotkeyOptions()
	{
		add(new OptionAsset
		{
			id = "hotkey_1",
			default_string = string.Empty,
			type = OptionType.String,
			has_locales = false
		});
		clone("hotkey_2", "hotkey_1");
		clone("hotkey_3", "hotkey_1");
		clone("hotkey_4", "hotkey_1");
		clone("hotkey_5", "hotkey_1");
		clone("hotkey_6", "hotkey_1");
		clone("hotkey_7", "hotkey_1");
		clone("hotkey_8", "hotkey_1");
		clone("hotkey_9", "hotkey_1");
		clone("hotkey_0", "hotkey_1");
	}

	public void forceDisableOption(string pID)
	{
		OptionAsset optionAsset = get(pID);
		if (!optionAsset.computer_only || !Config.isMobile)
		{
			PlayerConfig.setOptionBool(optionAsset.id, pVal: false);
			optionAsset.action?.Invoke(optionAsset);
		}
	}

	private bool getSavedBool(string pAssetID)
	{
		return PlayerConfig.optionBoolEnabled(pAssetID);
	}

	private void setSavedBool(string pAssetID, bool pValue)
	{
		PlayerConfig.setOptionBool(pAssetID, pValue);
	}

	private int getSavedInt(string pAssetID)
	{
		return PlayerConfig.getOptionInt(pAssetID);
	}

	private string getSavedString(string pAssetID)
	{
		return PlayerConfig.getOptionString(pAssetID);
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (OptionAsset item in list)
		{
			if (!item.has_locales || item.type != OptionType.Bool || !string.IsNullOrEmpty(item.translation_key))
			{
				continue;
			}
			foreach (GodPower item2 in AssetManager.powers.list)
			{
				if (!(item2.toggle_name != item.id))
				{
					item.translation_key = item2.getLocaleID();
					if (string.IsNullOrEmpty(item.translation_key_description))
					{
						item.translation_key_description = item2.getDescriptionID();
					}
				}
			}
		}
	}

	public override void editorDiagnosticLocales()
	{
		foreach (OptionAsset item in list)
		{
			if (item.has_locales)
			{
				checkLocale(item, item.getLocaleID());
				checkLocale(item, item.getDescriptionID());
				checkLocale(item, item.getDescriptionID2());
			}
		}
		base.editorDiagnosticLocales();
	}
}
