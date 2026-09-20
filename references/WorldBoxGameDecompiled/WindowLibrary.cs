using System;
using System.Collections.Generic;
using System.IO;

public class WindowLibrary : AssetLibrary<WindowAsset>
{
	[NonSerialized]
	private List<WindowAsset> _testable_windows = new List<WindowAsset>();

	public override void init()
	{
		base.init();
		add(new WindowAsset
		{
			id = "achievements",
			icon_path = "iconAchievements2",
			preload = true
		});
		add(new WindowAsset
		{
			id = "actor_asset",
			icon_path = "iconDebug",
			is_testable = false
		});
		add(new WindowAsset
		{
			id = "ad_loading_error",
			icon_path = "iconDeleteWorld"
		});
		add(new WindowAsset
		{
			id = "alliance",
			related_parent_window = "list_alliances",
			icon_path = "iconAlliance",
			preload = true
		});
		add(new WindowAsset
		{
			id = "alliance_customize",
			related_parent_window = "list_alliances",
			icon_path = "iconColorCustomization"
		});
		add(new WindowAsset
		{
			id = "auto_saves_browse",
			icon_path = "actor_traits/iconBlessing"
		});
		add(new WindowAsset
		{
			id = "brushes",
			icon_path = "iconColorCirlce2",
			preload = true,
			is_testable = false
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("brushes/brush_circ_1", "brushes/brush_circ_10", "brushes/brush_circ_15", "brushes/brush_circ_2", "brushes/brush_circ_5", "brushes/brush_sqr_1", "brushes/brush_sqr_10", "brushes/brush_sqr_15", "brushes/brush_sqr_2", "brushes/brush_sqr_5");
		add(new WindowAsset
		{
			id = "building_asset",
			icon_path = "iconBuildings",
			is_testable = false
		});
		add(new WindowAsset
		{
			id = "chart_comparer",
			icon_path = "iconCompareStatistics"
		});
		add(new WindowAsset
		{
			id = "city",
			related_parent_window = "list_cities",
			icon_path = "iconCity",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconCityInspect", SelectedMetas.selected_city.getActorAsset().icon);
		add(new WindowAsset
		{
			id = "army",
			related_parent_window = "list_armies",
			icon_path = "iconArmy",
			preload = true
		});
		WindowAsset windowAsset = t;
		windowAsset.get_hovering_icons = (HoveringBGIconsGetter)Delegate.Combine(windowAsset.get_hovering_icons, (HoveringBGIconsGetter)((WindowAsset _) => getHoveringIcons(SelectedMetas.selected_army.getActorAsset().icon)));
		add(new WindowAsset
		{
			id = "clan",
			related_parent_window = "list_clans",
			icon_path = "iconClan",
			preload = true
		});
		WindowAsset windowAsset2 = t;
		windowAsset2.get_hovering_icons = (HoveringBGIconsGetter)Delegate.Combine(windowAsset2.get_hovering_icons, (HoveringBGIconsGetter)((WindowAsset _) => getHoveringIcons(SelectedMetas.selected_clan.getActorAsset().icon)));
		add(new WindowAsset
		{
			id = "clan_customize",
			related_parent_window = "list_clans",
			icon_path = "iconColorCustomization"
		});
		add(new WindowAsset
		{
			id = "community_links",
			icon_path = "actor_traits/iconCommunity"
		});
		add(new WindowAsset
		{
			id = "credits",
			icon_path = "iconCoffee",
			window_toolbar_enabled = false
		});
		WindowAsset windowAsset3 = t;
		windowAsset3.get_hovering_icons = (HoveringBGIconsGetter)Delegate.Combine(windowAsset3.get_hovering_icons, (HoveringBGIconsGetter)((WindowAsset _) => getHoveringIcons("clan_traits/clan_trait_gods_chosen")));
		add(new WindowAsset
		{
			id = "credits_community",
			icon_path = "actor_traits/iconStrong"
		});
		add(new WindowAsset
		{
			id = "culture",
			related_parent_window = "list_cultures",
			icon_path = "iconCulture",
			preload = true
		});
		WindowAsset windowAsset4 = t;
		windowAsset4.get_hovering_icons = (HoveringBGIconsGetter)Delegate.Combine(windowAsset4.get_hovering_icons, (HoveringBGIconsGetter)((WindowAsset _) => getHoveringIconsUnits(SelectedMetas.selected_culture.units)));
		add(new WindowAsset
		{
			id = "culture_customize",
			related_parent_window = "list_cultures",
			icon_path = "iconColorCustomization"
		});
		add(new WindowAsset
		{
			id = "debug",
			icon_path = "iconDebug",
			is_testable = false
		});
		add(new WindowAsset
		{
			id = "debug_avatars",
			icon_path = "iconDebug",
			is_testable = false
		});
		add(new WindowAsset
		{
			id = "empty",
			icon_path = "iconEmptyLocus"
		});
		add(new WindowAsset
		{
			id = "equipment_rain_editor",
			icon_path = "iconCraftAdamantine",
			preload = true
		});
		add(new WindowAsset
		{
			id = "error_happened",
			icon_path = "iconDeleteWorld"
		});
		add(new WindowAsset
		{
			id = "error_with_reason",
			icon_path = "iconDeleteWorld"
		});
		add(new WindowAsset
		{
			id = "family",
			related_parent_window = "list_families",
			icon_path = "iconFamily",
			preload = true
		});
		add(new WindowAsset
		{
			id = "family_customize",
			related_parent_window = "list_families",
			icon_path = "iconColorCustomization"
		});
		add(new WindowAsset
		{
			id = "item",
			related_parent_window = "list_favorite_items",
			icon_path = "iconFavoriteWeapon",
			preload = true
		});
		add(new WindowAsset
		{
			id = "kingdom",
			related_parent_window = "list_kingdoms",
			icon_path = "iconCrown",
			preload = true
		});
		add(new WindowAsset
		{
			id = "kingdom_customize",
			related_parent_window = "list_kingdoms",
			icon_path = "iconColorCustomization"
		});
		add(new WindowAsset
		{
			id = "language",
			related_parent_window = "list_languages",
			icon_path = "iconLanguage",
			preload = true
		});
		WindowAsset windowAsset5 = t;
		windowAsset5.get_hovering_icons = (HoveringBGIconsGetter)Delegate.Combine(windowAsset5.get_hovering_icons, (HoveringBGIconsGetter)((WindowAsset _) => getHoveringIconsUnits(SelectedMetas.selected_language.units)));
		add(new WindowAsset
		{
			id = "language_customize",
			related_parent_window = "list_languages",
			icon_path = "iconColorCustomization"
		});
		add(new WindowAsset
		{
			id = "list_alliances",
			icon_path = "iconAllianceList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconAlliance");
		add(new WindowAsset
		{
			id = "list_cities",
			icon_path = "iconCityList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconCityInspect");
		add(new WindowAsset
		{
			id = "list_clans",
			icon_path = "iconClanList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconClan");
		add(new WindowAsset
		{
			id = "list_armies",
			icon_path = "iconArmy",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconArmy");
		add(new WindowAsset
		{
			id = "list_cultures",
			icon_path = "iconCultureList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconCulture");
		add(new WindowAsset
		{
			id = "list_families",
			icon_path = "iconFamilyList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconFamily");
		add(new WindowAsset
		{
			id = "list_favorite_items",
			icon_path = "iconFavoriteItemsList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconFavoriteStar");
		add(new WindowAsset
		{
			id = "list_favorite_units",
			icon_path = "iconFavoritesList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconFavoriteStar");
		add(new WindowAsset
		{
			id = "list_kingdoms",
			icon_path = "iconKingdomList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconCrown");
		add(new WindowAsset
		{
			id = "list_languages",
			icon_path = "iconLanguageList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconLanguage");
		add(new WindowAsset
		{
			id = "list_knowledge",
			icon_path = "iconKnowledge",
			preload = true
		});
		add(new WindowAsset
		{
			id = "list_plots",
			icon_path = "iconPlotList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconPlot");
		add(new WindowAsset
		{
			id = "list_religions",
			icon_path = "iconReligionList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconReligion");
		add(new WindowAsset
		{
			id = "list_subspecies",
			icon_path = "iconSubspeciesList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconSpecies");
		add(new WindowAsset
		{
			id = "list_wars",
			icon_path = "iconWarList",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconWar");
		add(new WindowAsset
		{
			id = "load_world",
			icon_path = "iconSaveLocal",
			window_toolbar_enabled = false,
			preload = true
		});
		WindowAsset windowAsset6 = t;
		windowAsset6.get_hovering_icons = (HoveringBGIconsGetter)Delegate.Combine(windowAsset6.get_hovering_icons, (HoveringBGIconsGetter)((WindowAsset _) => getHoveringIcons("iconBox")));
		add(new WindowAsset
		{
			id = "moonbox_promo",
			icon_path = "iconMoonBox",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "new_world_templates",
			icon_path = "iconBrowse",
			preload = true
		});
		add(new WindowAsset
		{
			id = "new_world_templates_2",
			related_parent_window = "new_world_templates",
			icon_path = "iconBrowse"
		});
		add(new WindowAsset
		{
			id = "news",
			icon_path = "iconDocument",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "not_found",
			icon_path = "iconDebug",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "other",
			icon_path = "iconOptions"
		});
		add(new WindowAsset
		{
			id = "patch_log",
			icon_path = "iconDocument",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "plot",
			related_parent_window = "list_plots",
			icon_path = "iconPlot"
		});
		add(new WindowAsset
		{
			id = "premium_menu",
			icon_path = "iconPremium",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "premium_help",
			icon_path = "iconPremium",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "premium_purchase_error",
			icon_path = "iconDeleteWorld",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "premium_unlocked",
			icon_path = "iconPremium",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "quit_game",
			icon_path = "iconClose",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "rate_us",
			icon_path = "iconHealth",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "rate_us_no",
			icon_path = "iconCloudRain",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "rate_us_yes",
			icon_path = "iconHealth",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "religion",
			related_parent_window = "list_religions",
			icon_path = "iconReligion",
			preload = true
		});
		add(new WindowAsset
		{
			id = "religion_customize",
			related_parent_window = "list_religions",
			icon_path = "iconColorCustomization"
		});
		add(new WindowAsset
		{
			id = "reward_ads",
			icon_path = "iconAdReward",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "reward_ads_power",
			icon_path = "iconAdReward",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "reward_ads_received",
			icon_path = "iconAdReward",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "reward_ads_saveslot",
			icon_path = "iconAdReward",
			window_toolbar_enabled = false
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconSaveLocal");
		add(new WindowAsset
		{
			id = "save_delete_confirm",
			related_parent_window = "saves_list",
			window_toolbar_enabled = false,
			icon_path = "iconDeleteWorld"
		});
		add(new WindowAsset
		{
			id = "save_load_confirm",
			related_parent_window = "saves_list",
			window_toolbar_enabled = false,
			icon_path = "iconBox"
		});
		add(new WindowAsset
		{
			id = "save_slot",
			related_parent_window = "saves_list",
			icon_path = "iconSaveLocal"
		});
		WindowAsset windowAsset7 = t;
		windowAsset7.get_hovering_icons = (HoveringBGIconsGetter)Delegate.Combine(windowAsset7.get_hovering_icons, (HoveringBGIconsGetter)((WindowAsset _) => getHoveringIcons("iconBox")));
		add(new WindowAsset
		{
			id = "save_slot_new",
			related_parent_window = "saves_list",
			icon_path = "iconSaveLocal"
		});
		add(new WindowAsset
		{
			id = "save_world_confirm",
			related_parent_window = "saves_list",
			window_toolbar_enabled = false,
			icon_path = "iconSaveLocal"
		});
		add(new WindowAsset
		{
			id = "saves_list",
			icon_path = "iconBrowse",
			preload = true
		});
		add(new WindowAsset
		{
			id = "settings",
			related_parent_window = "other",
			icon_path = "iconOptions"
		});
		add(new WindowAsset
		{
			id = "settings_old",
			icon_path = "iconOptions",
			is_testable = false
		});
		add(new WindowAsset
		{
			id = "statistics",
			icon_path = "iconStatistics"
		});
		add(new WindowAsset
		{
			id = "steam",
			icon_path = "iconSteam",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "steam_workshop_browse",
			related_parent_window = "steam",
			icon_path = "iconSteam",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "steam_workshop_empty",
			related_parent_window = "steam",
			icon_path = "iconSteam",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "steam_workshop_main",
			related_parent_window = "steam",
			icon_path = "iconSteam",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "steam_workshop_play_world",
			related_parent_window = "steam",
			icon_path = "iconSteam",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "steam_workshop_upload_world",
			related_parent_window = "steam",
			icon_path = "iconSteam",
			window_toolbar_enabled = false
		});
		WindowAsset windowAsset8 = t;
		windowAsset8.get_hovering_icons = (HoveringBGIconsGetter)Delegate.Combine(windowAsset8.get_hovering_icons, (HoveringBGIconsGetter)((WindowAsset _) => getHoveringIcons("iconSaveCloud")));
		add(new WindowAsset
		{
			id = "steam_workshop_uploading",
			related_parent_window = "steam",
			icon_path = "iconSteam",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "subspecies",
			related_parent_window = "list_subspecies",
			icon_path = "iconSpecies",
			preload = true
		});
		add(new WindowAsset
		{
			id = "subspecies_customize",
			related_parent_window = "list_subspecies",
			icon_path = "iconColorCustomization"
		});
		add(new WindowAsset
		{
			id = "thanks_for_testing",
			icon_path = "actor_traits/iconEyePatch",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "trait_rain_editor",
			icon_path = "actor_traits/iconDivineScar"
		});
		add(new WindowAsset
		{
			id = "under_development",
			icon_path = "iconDebug"
		});
		add(new WindowAsset
		{
			id = "unit",
			related_parent_window = "list_favorite_units",
			icon_path = "iconInspect",
			preload = true
		});
		WindowAsset windowAsset9 = t;
		windowAsset9.get_hovering_icons = (HoveringBGIconsGetter)Delegate.Combine(windowAsset9.get_hovering_icons, (HoveringBGIconsGetter)((WindowAsset _) => getHoveringIcons(SelectedUnit.unit.asset.icon)));
		add(new WindowAsset
		{
			id = "update_available",
			icon_path = "iconCrit",
			window_toolbar_enabled = false
		});
		add(new WindowAsset
		{
			id = "war",
			related_parent_window = "list_wars",
			icon_path = "iconWar",
			preload = true
		});
		add(new WindowAsset
		{
			id = "welcome",
			window_toolbar_enabled = false,
			icon_path = "iconAye",
			preload = true
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("language_traits/", "culture_traits/", "clan_traits/", "subspecies_traits/", "religion_traits/", "kingdom_traits/");
		add(new WindowAsset
		{
			id = "world_ages",
			icon_path = "iconAges",
			preload = true
		});
		add(new WindowAsset
		{
			id = "world_history",
			icon_path = "iconWorldLog",
			preload = true
		});
		add(new WindowAsset
		{
			id = "world_info",
			icon_path = "iconWorldInfo",
			preload = true
		});
		add(new WindowAsset
		{
			id = "world_languages",
			related_parent_window = "other",
			icon_path = "iconLanguage"
		});
		t.get_hovering_icons = (WindowAsset _) => getHoveringIcons("iconLanguage");
		add(new WindowAsset
		{
			id = "world_laws",
			icon_path = "iconWorldLaws",
			preload = true
		});
	}

	public override void post_init()
	{
		base.post_init();
		ScrollWindow.addCallbackOpen(delegate
		{
			Config.debug_window_stats.opens++;
			HoveringBgIconManager.show();
		});
		ScrollWindow.addCallbackClose(delegate
		{
			Config.debug_window_stats.closes++;
			HoveringBgIconManager.hide();
		});
		ScrollWindow.addCallbackShowStarted(delegate(string pWindowId)
		{
			Config.debug_window_stats.shows++;
			Config.debug_window_stats.setCurrent(pWindowId);
		});
		ScrollWindow.addCallbackShow(delegate(string pWindowId)
		{
			HoveringBgIconManager.showWindow(get(pWindowId));
		});
		ScrollWindow.addCallbackShowFinished(delegate
		{
			ScrollWindow.checkElements();
		});
		ScrollWindow.addCallbackHide(delegate
		{
			Config.debug_window_stats.hides++;
		});
		foreach (WindowAsset item in list)
		{
			if (item.is_testable)
			{
				item.is_testable = isTestable(item);
			}
			if (item.is_testable)
			{
				_testable_windows.Add(item);
			}
		}
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (MetaTypeAsset item in AssetManager.meta_type_library.list)
		{
			if (string.IsNullOrEmpty(item.window_name))
			{
				continue;
			}
			WindowAsset windowAsset = get(item.window_name);
			if (windowAsset == null)
			{
				BaseAssetLibrary.logAssetError("WindowAsset not found for MetaTypeAsset ", item.id);
				continue;
			}
			windowAsset.meta_type_asset = item;
			if (has(item.window_name + "_customize"))
			{
				get(item.window_name + "_customize").meta_type_asset = item;
			}
			if (windowAsset.related_parent_window != null)
			{
				WindowAsset windowAsset2 = get(windowAsset.related_parent_window);
				if (windowAsset2 != null)
				{
					windowAsset2.meta_type_asset = item;
				}
			}
		}
	}

	internal List<WindowAsset> getTestableWindows()
	{
		return _testable_windows;
	}

	private bool isTestable(WindowAsset pWindowAsset)
	{
		string text = pWindowAsset.id;
		if (text.Contains("upload"))
		{
			return false;
		}
		if (text.Contains("_testing_"))
		{
			return false;
		}
		if (text.StartsWith("worldnet"))
		{
			return false;
		}
		switch (text)
		{
		case "register":
		case "settings_old":
		case "empty":
		case "not_found":
		case "create_predefined_world":
		case "kingdom_technology":
		case "moonbox_promo":
		case "more_games":
		case "lsflw2_promo":
		case "brushes":
		case "debug":
		case "create_custom_world":
			return false;
		default:
			return true;
		}
	}

	public override void editorDiagnostic()
	{
		base.editorDiagnostic();
		string[] files = Directory.GetFiles("Assets/Resources/windows", "*.prefab", SearchOption.TopDirectoryOnly);
		using ListPool<string> listPool = new ListPool<string>();
		string[] array = files;
		for (int i = 0; i < array.Length; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(array[i]);
			if (dict.ContainsKey(fileNameWithoutExtension))
			{
				listPool.Add(fileNameWithoutExtension);
			}
			else if (!(fileNameWithoutExtension == "list_window"))
			{
				BaseAssetLibrary.logAssetError("No associated WindowAsset found for window ", fileNameWithoutExtension);
			}
		}
		foreach (WindowAsset item in list)
		{
			if (!listPool.Contains(item.id))
			{
				BaseAssetLibrary.logAssetError("Window prefab not found for WindowAsset ", item.id);
			}
		}
	}

	private static IEnumerable<string> getHoveringIconsUnits(List<Actor> pUnits)
	{
		HashSet<string> tIcons = new HashSet<string>();
		foreach (Actor pUnit in pUnits)
		{
			string icon = pUnit.asset.icon;
			if (tIcons.Add(icon))
			{
				yield return icon;
			}
		}
	}

	private static IEnumerable<string> getHoveringIcons(params string[] pPaths)
	{
		for (int i = 0; i < pPaths.Length; i++)
		{
			yield return pPaths[i];
		}
	}
}
