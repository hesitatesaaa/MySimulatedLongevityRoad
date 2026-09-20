using UnityEngine;

public class MetaCustomizationLibrary : AssetLibrary<MetaCustomizationAsset>
{
	public override void init()
	{
		base.init();
		add(new MetaCustomizationAsset
		{
			id = "religion",
			meta_type = MetaType.Religion,
			banner_prefab_id = "ui/PrefabBannerReligion",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				ReligionBanner religionBanner = Object.Instantiate<ReligionBanner>(Resources.Load<ReligionBanner>(pAsset.banner_prefab_id), pParent);
				religionBanner.enable_default_click = false;
				religionBanner.load(pNanoObject as Religion);
				return religionBanner;
			},
			customize_component = delegate(GameObject pGameObject)
			{
				pGameObject.AddComponent<ReligionCustomizeWindow>();
			},
			customize_window_id = "religion_customize",
			option_1_get = () => SelectedMetas.selected_religion.data.banner_background_id,
			option_1_set = delegate(int pValue)
			{
				SelectedMetas.selected_religion.data.banner_background_id = pValue;
			},
			option_2_get = () => SelectedMetas.selected_religion.data.banner_icon_id,
			option_2_set = delegate(int pValue)
			{
				SelectedMetas.selected_religion.data.banner_icon_id = pValue;
			},
			color_get = () => SelectedMetas.selected_religion.data.color_id,
			color_set = delegate(int pValue)
			{
				SelectedMetas.selected_religion.data.setColorID(pValue);
			},
			color_library = () => AssetManager.religion_colors_library,
			option_1_count = () => AssetManager.religion_banners_library.getCurrentAsset().backgrounds.Count,
			option_2_count = () => AssetManager.religion_banners_library.getCurrentAsset().icons.Count,
			title_locale = "customize_religion",
			option_1_locale = "religion_background",
			option_2_locale = "religion_element",
			color_locale = "religion_color",
			icon_banner = "iconReligion",
			icon_creature = "iconLivingPlants"
		});
		add(new MetaCustomizationAsset
		{
			id = "culture",
			meta_type = MetaType.Culture,
			banner_prefab_id = "ui/PrefabBannerCulture",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				CultureBanner cultureBanner = Object.Instantiate<CultureBanner>(Resources.Load<CultureBanner>(pAsset.banner_prefab_id), pParent);
				cultureBanner.enable_default_click = false;
				cultureBanner.load(pNanoObject as Culture);
				return cultureBanner;
			},
			customize_component = delegate(GameObject pGameObject)
			{
				pGameObject.AddComponent<CultureCustomizeWindow>();
			},
			customize_window_id = "culture_customize",
			option_1_get = () => SelectedMetas.selected_culture.data.banner_decor_id,
			option_1_set = delegate(int pValue)
			{
				SelectedMetas.selected_culture.data.banner_decor_id = pValue;
			},
			option_2_get = () => SelectedMetas.selected_culture.data.banner_element_id,
			option_2_set = delegate(int pValue)
			{
				SelectedMetas.selected_culture.data.banner_element_id = pValue;
			},
			color_get = () => SelectedMetas.selected_culture.data.color_id,
			color_set = delegate(int pValue)
			{
				SelectedMetas.selected_culture.data.setColorID(pValue);
			},
			color_library = () => AssetManager.culture_colors_library,
			option_1_count = () => AssetManager.culture_banners_library.getCurrentAsset().backgrounds.Count,
			option_2_count = () => AssetManager.culture_banners_library.getCurrentAsset().icons.Count,
			title_locale = "customize_culture",
			option_1_locale = "culture_decor",
			option_2_locale = "culture_element",
			color_locale = "culture_color",
			icon_banner = "iconCulture",
			icon_creature = "iconSuperPumpkin"
		});
		add(new MetaCustomizationAsset
		{
			id = "family",
			meta_type = MetaType.Family,
			banner_prefab_id = "ui/PrefabBannerFamily",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				FamilyBanner familyBanner = Object.Instantiate<FamilyBanner>(Resources.Load<FamilyBanner>(pAsset.banner_prefab_id), pParent);
				familyBanner.enable_default_click = false;
				familyBanner.load(pNanoObject as Family);
				return familyBanner;
			},
			customize_component = delegate(GameObject pGameObject)
			{
				pGameObject.AddComponent<FamilyCustomizeWindow>();
			},
			customize_window_id = "family_customize",
			option_1_get = () => SelectedMetas.selected_family.data.banner_background_id,
			option_1_set = delegate(int pValue)
			{
				SelectedMetas.selected_family.data.banner_background_id = pValue;
			},
			option_2_get = () => SelectedMetas.selected_family.data.banner_frame_id,
			option_2_set = delegate(int pValue)
			{
				SelectedMetas.selected_family.data.banner_frame_id = pValue;
			},
			option_2_color_editable = false,
			color_get = () => SelectedMetas.selected_family.data.color_id,
			color_set = delegate(int pValue)
			{
				SelectedMetas.selected_family.data.setColorID(pValue);
			},
			color_library = () => AssetManager.families_colors_library,
			option_1_count = () => AssetManager.family_banners_library.getCurrentAsset().backgrounds.Count,
			option_2_count = () => AssetManager.family_banners_library.getCurrentAsset().frames.Count,
			title_locale = "customize_family",
			option_1_locale = "family_background",
			option_2_locale = "family_frame",
			color_locale = "family_color",
			icon_banner = "iconFamily",
			icon_creature = "iconLivingPlants"
		});
		add(new MetaCustomizationAsset
		{
			id = "language",
			meta_type = MetaType.Language,
			banner_prefab_id = "ui/PrefabBannerLanguage",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				LanguageBanner languageBanner = Object.Instantiate<LanguageBanner>(Resources.Load<LanguageBanner>(pAsset.banner_prefab_id), pParent);
				languageBanner.enable_default_click = false;
				languageBanner.load(pNanoObject as Language);
				return languageBanner;
			},
			customize_component = delegate(GameObject pGameObject)
			{
				pGameObject.AddComponent<LanguageCustomizeWindow>();
			},
			customize_window_id = "language_customize",
			option_1_get = () => SelectedMetas.selected_language.data.banner_background_id,
			option_1_set = delegate(int pValue)
			{
				SelectedMetas.selected_language.data.banner_background_id = pValue;
			},
			option_2_get = () => SelectedMetas.selected_language.data.banner_icon_id,
			option_2_set = delegate(int pValue)
			{
				SelectedMetas.selected_language.data.banner_icon_id = pValue;
			},
			color_get = () => SelectedMetas.selected_language.data.color_id,
			color_set = delegate(int pValue)
			{
				SelectedMetas.selected_language.data.setColorID(pValue);
			},
			color_library = () => AssetManager.languages_colors_library,
			option_1_count = () => AssetManager.language_banners_library.getCurrentAsset().backgrounds.Count,
			option_2_count = () => AssetManager.language_banners_library.getCurrentAsset().icons.Count,
			title_locale = "customize_language",
			option_1_locale = "language_background",
			option_2_locale = "language_element",
			color_locale = "language_color",
			icon_banner = "iconLanguage",
			icon_creature = "iconLivingPlants"
		});
		add(new MetaCustomizationAsset
		{
			id = "subspecies",
			meta_type = MetaType.Subspecies,
			banner_prefab_id = "ui/PrefabBannerSubspecies",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				SubspeciesBanner subspeciesBanner = Object.Instantiate<SubspeciesBanner>(Resources.Load<SubspeciesBanner>(pAsset.banner_prefab_id), pParent);
				subspeciesBanner.enable_default_click = false;
				subspeciesBanner.load(pNanoObject as Subspecies);
				return subspeciesBanner;
			},
			customize_component = delegate(GameObject pGameObject)
			{
				pGameObject.AddComponent<SubspeciesCustomizeWindow>();
			},
			customize_window_id = "subspecies_customize",
			option_1_get = () => SelectedMetas.selected_subspecies.data.banner_background_id,
			option_1_set = delegate(int pValue)
			{
				SelectedMetas.selected_subspecies.data.banner_background_id = pValue;
			},
			color_get = () => SelectedMetas.selected_subspecies.data.color_id,
			color_set = delegate(int pValue)
			{
				SelectedMetas.selected_subspecies.data.setColorID(pValue);
			},
			color_library = () => AssetManager.subspecies_colors_library,
			option_1_count = () => AssetManager.subspecies_banners_library.getCurrentAsset().backgrounds.Count,
			option_2_editable = false,
			title_locale = "customize_subspecies",
			option_1_locale = "subspecies_background",
			color_locale = "subspecies_color",
			icon_banner = "iconSubspeciesList",
			icon_creature = "iconLivingPlants"
		});
		add(new MetaCustomizationAsset
		{
			id = "kingdom",
			meta_type = MetaType.Kingdom,
			banner_prefab_id = "ui/PrefabBannerKingdom",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				KingdomBanner kingdomBanner = Object.Instantiate<KingdomBanner>(Resources.Load<KingdomBanner>(pAsset.banner_prefab_id), pParent);
				kingdomBanner.enable_default_click = false;
				kingdomBanner.load(pNanoObject as Kingdom);
				return kingdomBanner;
			},
			customize_component = delegate(GameObject pGameObject)
			{
				pGameObject.AddComponent<KingdomCustomizeWindow>();
			},
			customize_window_id = "kingdom_customize",
			option_1_get = () => SelectedMetas.selected_kingdom.data.banner_background_id,
			option_1_set = delegate(int pValue)
			{
				SelectedMetas.selected_kingdom.data.banner_background_id = pValue;
			},
			option_2_get = () => SelectedMetas.selected_kingdom.data.banner_icon_id,
			option_2_set = delegate(int pValue)
			{
				SelectedMetas.selected_kingdom.data.banner_icon_id = pValue;
			},
			color_get = () => SelectedMetas.selected_kingdom.data.color_id,
			color_set = delegate(int pValue)
			{
				SelectedMetas.selected_kingdom.data.setColorID(pValue);
			},
			color_library = () => AssetManager.kingdom_colors_library,
			option_1_count = () => AssetManager.kingdom_banners_library.get(SelectedMetas.selected_kingdom.getActorAsset().banner_id).backgrounds.Count,
			option_2_count = () => AssetManager.kingdom_banners_library.get(SelectedMetas.selected_kingdom.getActorAsset().banner_id).icons.Count,
			title_locale = "customize_kingdom",
			option_1_locale = "banner_design",
			option_2_locale = "banner_emblem",
			color_locale = "kingdom_color",
			icon_banner = "iconCrown",
			icon_creature = "iconBiomass"
		});
		add(new MetaCustomizationAsset
		{
			id = "city",
			meta_type = MetaType.City,
			localization_title = "village",
			option_1_editable = false,
			option_2_editable = false,
			banner_prefab_id = "ui/PrefabBannerCity",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				CityBanner cityBanner = Object.Instantiate<CityBanner>(Resources.Load<CityBanner>(pAsset.banner_prefab_id), pParent);
				cityBanner.enable_default_click = false;
				cityBanner.load(pNanoObject as City);
				return cityBanner;
			},
			customize_window_id = "kingdom_customize"
		});
		add(new MetaCustomizationAsset
		{
			id = "army",
			meta_type = MetaType.Army,
			localization_title = "army",
			option_1_editable = false,
			option_2_editable = false,
			banner_prefab_id = "ui/PrefabBannerArmy",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				ArmyBanner armyBanner = Object.Instantiate<ArmyBanner>(Resources.Load<ArmyBanner>(pAsset.banner_prefab_id), pParent);
				armyBanner.enable_default_click = false;
				armyBanner.load(pNanoObject as Army);
				return armyBanner;
			},
			color_library = () => AssetManager.kingdom_colors_library,
			customize_window_id = "kingdom_customize"
		});
		add(new MetaCustomizationAsset
		{
			id = "clan",
			meta_type = MetaType.Clan,
			banner_prefab_id = "ui/PrefabBannerClan",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				ClanBanner clanBanner = Object.Instantiate<ClanBanner>(Resources.Load<ClanBanner>(pAsset.banner_prefab_id), pParent);
				clanBanner.enable_default_click = false;
				clanBanner.load(pNanoObject as Clan);
				return clanBanner;
			},
			customize_component = delegate(GameObject pGameObject)
			{
				pGameObject.AddComponent<ClanCustomizeWindow>();
			},
			customize_window_id = "clan_customize",
			option_1_get = () => SelectedMetas.selected_clan.data.banner_background_id,
			option_1_set = delegate(int pValue)
			{
				SelectedMetas.selected_clan.data.banner_background_id = pValue;
			},
			option_2_get = () => SelectedMetas.selected_clan.data.banner_icon_id,
			option_2_set = delegate(int pValue)
			{
				SelectedMetas.selected_clan.data.banner_icon_id = pValue;
			},
			color_get = () => SelectedMetas.selected_clan.data.color_id,
			color_set = delegate(int pValue)
			{
				SelectedMetas.selected_clan.data.setColorID(pValue);
			},
			color_library = () => AssetManager.clan_colors_library,
			option_1_count = () => AssetManager.clan_banners_library.getCurrentAsset().backgrounds.Count,
			option_2_count = () => AssetManager.clan_banners_library.getCurrentAsset().icons.Count,
			title_locale = "customize_clan",
			option_1_locale = "clan_background",
			option_2_locale = "clan_icon",
			color_locale = "clan_color",
			icon_banner = "iconClan",
			icon_creature = "iconSuperPumpkin"
		});
		add(new MetaCustomizationAsset
		{
			id = "alliance",
			meta_type = MetaType.Alliance,
			banner_prefab_id = "ui/PrefabBannerAlliance",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				AllianceBanner allianceBanner = Object.Instantiate<AllianceBanner>(Resources.Load<AllianceBanner>(pAsset.banner_prefab_id), pParent);
				allianceBanner.enable_default_click = false;
				allianceBanner.load(pNanoObject as Alliance);
				return allianceBanner;
			},
			customize_component = delegate(GameObject pGameObject)
			{
				pGameObject.AddComponent<AllianceCustomizeWindow>();
			},
			customize_window_id = "alliance_customize",
			option_1_get = () => SelectedMetas.selected_alliance.data.banner_background_id,
			option_1_set = delegate(int pValue)
			{
				SelectedMetas.selected_alliance.data.banner_background_id = pValue;
			},
			option_2_get = () => SelectedMetas.selected_alliance.data.banner_icon_id,
			option_2_set = delegate(int pValue)
			{
				SelectedMetas.selected_alliance.data.banner_icon_id = pValue;
			},
			color_get = () => SelectedMetas.selected_alliance.data.color_id,
			color_set = delegate(int pValue)
			{
				SelectedMetas.selected_alliance.data.setColorID(pValue);
			},
			color_library = () => AssetManager.kingdom_colors_library,
			option_1_count = () => World.world.alliances.getBackgroundsList().Length,
			option_2_count = () => World.world.alliances.getIconsList().Length,
			title_locale = "customize_alliance",
			option_1_locale = "alliance_background",
			option_2_locale = "alliance_icon",
			color_locale = "alliance_color",
			icon_banner = "iconAlliance",
			icon_creature = "iconSuperPumpkin"
		});
		add(new MetaCustomizationAsset
		{
			id = "plot",
			meta_type = MetaType.Plot,
			editable = false,
			banner_prefab_id = "ui/PrefabBannerPlot",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				PlotBanner plotBanner = Object.Instantiate<PlotBanner>(Resources.Load<PlotBanner>(pAsset.banner_prefab_id), pParent);
				plotBanner.load(pNanoObject);
				return plotBanner;
			}
		});
		add(new MetaCustomizationAsset
		{
			id = "war",
			meta_type = MetaType.War,
			editable = false,
			banner_prefab_id = "ui/PrefabBannerWar",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				WarBanner warBanner = Object.Instantiate<WarBanner>(Resources.Load<WarBanner>(pAsset.banner_prefab_id), pParent);
				warBanner.load(pNanoObject);
				return warBanner;
			}
		});
		add(new MetaCustomizationAsset
		{
			id = "unit",
			meta_type = MetaType.Unit,
			editable = false,
			banner_prefab_id = "ui/UnitAvatarElement",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				UiUnitAvatarElement uiUnitAvatarElement = Object.Instantiate<UiUnitAvatarElement>(Resources.Load<UiUnitAvatarElement>(pAsset.banner_prefab_id), pParent);
				uiUnitAvatarElement.load(pNanoObject);
				((Component)uiUnitAvatarElement).transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
				return uiUnitAvatarElement;
			}
		});
		add(new MetaCustomizationAsset
		{
			id = "item",
			meta_type = MetaType.Item,
			editable = false,
			banner_prefab_id = "ui/EquipmentButton",
			get_banner = delegate(MetaCustomizationAsset pAsset, NanoObject pNanoObject, Transform pParent)
			{
				EquipmentButton equipmentButton = Object.Instantiate<EquipmentButton>(Resources.Load<EquipmentButton>(pAsset.banner_prefab_id), pParent);
				equipmentButton.load(pNanoObject);
				return equipmentButton;
			}
		});
		add(new MetaCustomizationAsset
		{
			id = "world",
			meta_type = MetaType.World,
			editable = false,
			option_1_editable = false,
			option_2_editable = false,
			color_editable = false
		});
	}

	public MetaCustomizationAsset getAsset(MetaType pType)
	{
		return get(pType.AsString());
	}

	public override void post_init()
	{
		base.post_init();
		foreach (MetaCustomizationAsset item in list)
		{
			if (item.localization_title == null)
			{
				item.localization_title = item.id;
			}
		}
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (MetaCustomizationAsset tAsset in list)
		{
			if (tAsset.color_count == null)
			{
				tAsset.color_count = () => tAsset.color_library().list.Count;
			}
		}
	}

	public override void editorDiagnostic()
	{
		foreach (MetaCustomizationAsset item in list)
		{
			if (item.editable)
			{
				if (item.color_count == null)
				{
					BaseAssetLibrary.logAssetError("Missing <e>color_count</e>", item.id);
				}
				if (item.option_1_editable && item.option_1_count == null)
				{
					BaseAssetLibrary.logAssetError("Missing <e>option_1_count</e>", item.id);
				}
				if (item.option_2_editable && item.option_2_count == null)
				{
					BaseAssetLibrary.logAssetError("Missing <e>option_2_count</e>", item.id);
				}
			}
		}
		base.editorDiagnostic();
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (MetaCustomizationAsset item in list)
		{
			if (!item.editable)
			{
				continue;
			}
			foreach (string localeID in item.getLocaleIDs())
			{
				checkLocale(item, localeID);
			}
		}
	}
}
