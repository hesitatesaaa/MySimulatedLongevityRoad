public static class Zones
{
	private static GodPower _selected_power => World.world.selected_power;

	internal static bool isPowerForceMapMode(MetaType pMode = MetaType.None)
	{
		if (!World.world.isAnyPowerSelected())
		{
			return false;
		}
		return _selected_power.force_map_mode == pMode;
	}

	public static bool isPowerForcedMapModeEnabled()
	{
		MetaType pType = MetaType.None;
		if (World.world.isAnyPowerSelected() && !_selected_power.force_map_mode.isNone())
		{
			pType = _selected_power.force_map_mode;
		}
		return !pType.isNone();
	}

	internal static MetaType getForcedMapMode()
	{
		MetaType result = MetaType.None;
		if (isPowerForcedMapModeEnabled())
		{
			result = _selected_power.force_map_mode;
		}
		else if (SelectedObjects.isNanoObjectSet())
		{
			MetaTypeAsset metaTypeAsset = SelectedObjects.getSelectedNanoObject().getMetaTypeAsset();
			if (metaTypeAsset.force_zone_when_selected)
			{
				result = metaTypeAsset.map_mode;
			}
		}
		return result;
	}

	public static bool hasPowerForceMapMode()
	{
		return !getForcedMapMode().isNone();
	}

	public static MetaTypeAsset getMapMetaAsset()
	{
		if (hasPowerForceMapMode())
		{
			return getForcedMapMode().getAsset();
		}
		for (int i = 0; i < AssetManager.meta_type_library.list.Count; i++)
		{
			MetaTypeAsset metaTypeAsset = AssetManager.meta_type_library.list[i];
			if (!string.IsNullOrEmpty(metaTypeAsset.option_id) && (AssetManager.options_library.get(metaTypeAsset.option_id).isActive() || isPowerForceMapMode(metaTypeAsset.map_mode)))
			{
				return metaTypeAsset;
			}
		}
		return null;
	}

	public static bool showMapNames()
	{
		if (!PlayerConfig.optionBoolEnabled("map_names"))
		{
			return hasPowerForceMapMode();
		}
		return true;
	}

	public static bool showMapBorders()
	{
		if (!isBordersEnabled())
		{
			return hasPowerForceMapMode();
		}
		return true;
	}

	public static bool isBordersEnabled()
	{
		return PlayerConfig.optionBoolEnabled("map_layers");
	}

	public static MetaType getCurrentMapBorderMode(bool pCheckOnlyOption = false)
	{
		if (showCultureZones(pCheckOnlyOption))
		{
			return MetaType.Culture;
		}
		if (showKingdomZones(pCheckOnlyOption))
		{
			return MetaType.Kingdom;
		}
		if (showClanZones(pCheckOnlyOption))
		{
			return MetaType.Clan;
		}
		if (showAllianceZones(pCheckOnlyOption))
		{
			return MetaType.Alliance;
		}
		if (showCityZones(pCheckOnlyOption))
		{
			return MetaType.City;
		}
		if (showSpeciesZones(pCheckOnlyOption))
		{
			return MetaType.Subspecies;
		}
		if (showFamiliesZones(pCheckOnlyOption))
		{
			return MetaType.Family;
		}
		if (showLanguagesZones(pCheckOnlyOption))
		{
			return MetaType.Language;
		}
		if (showReligionZones(pCheckOnlyOption))
		{
			return MetaType.Religion;
		}
		if (showArmyZones(pCheckOnlyOption))
		{
			return MetaType.Army;
		}
		return MetaType.None;
	}

	public static bool showCityZones(bool pCheckOnlyOption = false)
	{
		return MetaTypeLibrary.city.isActive(pCheckOnlyOption);
	}

	public static bool showKingdomZones(bool pCheckOnlyOption = false)
	{
		return MetaTypeLibrary.kingdom.isActive(pCheckOnlyOption);
	}

	public static bool showClanZones(bool pCheckOnlyOption = false)
	{
		return MetaTypeLibrary.clan.isActive(pCheckOnlyOption);
	}

	public static bool showAllianceZones(bool pCheckOnlyOption = false)
	{
		return MetaTypeLibrary.alliance.isActive(pCheckOnlyOption);
	}

	public static bool showCultureZones(bool pCheckOnlyOption = false)
	{
		return MetaTypeLibrary.culture.isActive(pCheckOnlyOption);
	}

	public static bool showSpeciesZones(bool pCheckOnlyOption = false)
	{
		return MetaTypeLibrary.subspecies.isActive(pCheckOnlyOption);
	}

	public static bool showFamiliesZones(bool pCheckOnlyOption = false)
	{
		return MetaTypeLibrary.family.isActive(pCheckOnlyOption);
	}

	public static bool showLanguagesZones(bool pCheckOnlyOption = false)
	{
		return MetaTypeLibrary.language.isActive(pCheckOnlyOption);
	}

	public static bool showReligionZones(bool pCheckOnlyOption = false)
	{
		return MetaTypeLibrary.religion.isActive(pCheckOnlyOption);
	}

	public static bool showArmyZones(bool pCheckOnlyOption = false)
	{
		return MetaTypeLibrary.army.isActive(pCheckOnlyOption);
	}
}
