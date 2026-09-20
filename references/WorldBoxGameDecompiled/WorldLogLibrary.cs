public class WorldLogLibrary : AssetLibrary<WorldLogAsset>
{
	public static WorldLogAsset king_new;

	public static WorldLogAsset king_left;

	public static WorldLogAsset king_fled_city;

	public static WorldLogAsset king_fled_capital;

	public static WorldLogAsset king_dead;

	public static WorldLogAsset king_killed;

	public static WorldLogAsset favorite_dead;

	public static WorldLogAsset favorite_killed;

	public static WorldLogAsset city_new;

	public static WorldLogAsset log_city_revolted;

	public static WorldLogAsset diplomacy_war_ended;

	public static WorldLogAsset diplomacy_war_started;

	public static WorldLogAsset total_war_started;

	public static WorldLogAsset alliance_new;

	public static WorldLogAsset alliance_dissolved;

	public static WorldLogAsset kingdom_new;

	public static WorldLogAsset kingdom_destroyed;

	public static WorldLogAsset kingdom_shattered;

	public static WorldLogAsset kingdom_fractured;

	public static WorldLogAsset city_destroyed;

	public static WorldLogAsset kingdom_royal_clan_new;

	public static WorldLogAsset kingdom_royal_clan_changed;

	public static WorldLogAsset kingdom_royal_clan_dead;

	public static WorldLogAsset auto_tester;

	private void updateText(ref string pText, WorldLogMessage pMessage, string pKey, int pSpecialId)
	{
		string special = pMessage.getSpecial(pSpecialId);
		pText = pText.Replace(pKey, special);
	}

	public override void init()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0613: Unknown result type (might be due to invalid IL or missing references)
		//IL_0618: Unknown result type (might be due to invalid IL or missing references)
		//IL_0668: Unknown result type (might be due to invalid IL or missing references)
		//IL_066d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0712: Unknown result type (might be due to invalid IL or missing references)
		//IL_0717: Unknown result type (might be due to invalid IL or missing references)
		//IL_0755: Unknown result type (might be due to invalid IL or missing references)
		//IL_075a: Unknown result type (might be due to invalid IL or missing references)
		base.init();
		king_new = add(new WorldLogAsset
		{
			id = "king_new",
			group = "kings",
			path_icon = "ui/Icons/iconKings",
			color = Toolbox.color_log_neutral,
			text_replacer = kingReplacer
		});
		king_left = add(new WorldLogAsset
		{
			id = "king_left",
			group = "kings",
			path_icon = "ui/Icons/actor_traits/iconStupid",
			color = Toolbox.color_log_warning,
			text_replacer = kingReplacer
		});
		king_fled_capital = add(new WorldLogAsset
		{
			id = "king_fled_capital",
			group = "kings",
			random_ids = 3,
			path_icon = "ui/Icons/actor_traits/iconStupid",
			color = Toolbox.color_log_warning,
			text_replacer = kingCityReplacer
		});
		king_fled_city = add(new WorldLogAsset
		{
			id = "king_fled_city",
			group = "kings",
			random_ids = 3,
			path_icon = "ui/Icons/actor_traits/iconStupid",
			color = Toolbox.color_log_warning,
			text_replacer = kingCityReplacer
		});
		king_dead = add(new WorldLogAsset
		{
			id = "king_dead",
			group = "kings",
			path_icon = "ui/Icons/iconDead",
			color = Toolbox.color_log_warning,
			text_replacer = kingReplacer
		});
		king_killed = add(new WorldLogAsset
		{
			id = "king_killed",
			group = "kings",
			random_ids = 3,
			path_icon = "ui/Icons/actor_traits/iconKingslayer",
			color = Toolbox.color_log_warning,
			text_replacer = delegate(WorldLogMessage pMessage, ref string pText)
			{
				updateText(ref pText, pMessage, "$kingdom$", 1);
				updateText(ref pText, pMessage, "$king$", 2);
				updateText(ref pText, pMessage, "$name$", 3);
			}
		});
		favorite_dead = add(new WorldLogAsset
		{
			id = "favorite_dead",
			group = "favorite_units",
			random_ids = 3,
			path_icon = "ui/Icons/iconFavoriteKilled",
			color = Toolbox.color_log_warning,
			text_replacer = nameReplacer
		});
		favorite_killed = add(new WorldLogAsset
		{
			id = "favorite_killed",
			group = "favorite_units",
			random_ids = 3,
			path_icon = "ui/Icons/iconFavoriteKilled",
			color = Toolbox.color_log_warning,
			text_replacer = delegate(WorldLogMessage pMessage, ref string pText)
			{
				updateText(ref pText, pMessage, "$name$", 1);
				updateText(ref pText, pMessage, "$killer$", 2);
			}
		});
		city_new = add(new WorldLogAsset
		{
			id = "city_new",
			group = "cities",
			path_icon = "ui/Icons/iconCitySelect",
			color = Toolbox.color_log_neutral,
			text_replacer = nameReplacer
		});
		log_city_revolted = add(new WorldLogAsset
		{
			id = "log_city_revolted",
			group = "cities",
			path_icon = "ui/Icons/iconInspiration",
			color = Toolbox.color_log_good,
			text_replacer = delegate(WorldLogMessage pMessage, ref string pText)
			{
				updateText(ref pText, pMessage, "$name$", 1);
				updateText(ref pText, pMessage, "$kingdom$", 2);
			}
		});
		diplomacy_war_ended = add(new WorldLogAsset
		{
			id = "diplomacy_war_ended",
			group = "wars",
			path_icon = "ui/Icons/actor_traits/iconPacifist",
			color = Toolbox.color_log_good,
			text_replacer = nameReplacer
		});
		diplomacy_war_started = add(new WorldLogAsset
		{
			id = "diplomacy_war_started",
			group = "wars",
			path_icon = "ui/Icons/iconWar",
			color = Toolbox.color_log_warning,
			text_replacer = delegate(WorldLogMessage pMessage, ref string pText)
			{
				updateText(ref pText, pMessage, "$kingdom1$", 1);
				updateText(ref pText, pMessage, "$kingdom2$", 2);
			}
		});
		total_war_started = add(new WorldLogAsset
		{
			id = "total_war_started",
			group = "wars",
			path_icon = "ui/Icons/iconWar",
			color = Toolbox.color_log_warning,
			text_replacer = delegate(WorldLogMessage pMessage, ref string pText)
			{
				updateText(ref pText, pMessage, "$kingdom$", 1);
			}
		});
		alliance_new = add(new WorldLogAsset
		{
			id = "alliance_new",
			path_icon = "ui/Icons/iconAlliance",
			color = Toolbox.color_log_neutral,
			text_replacer = nameReplacer
		});
		alliance_dissolved = add(new WorldLogAsset
		{
			id = "alliance_dissolved",
			path_icon = "ui/Icons/iconAlliance",
			color = Toolbox.color_log_warning,
			text_replacer = nameReplacer
		});
		kingdom_new = add(new WorldLogAsset
		{
			id = "kingdom_new",
			group = "kingdoms",
			path_icon = "ui/Icons/iconKingdom",
			color = Toolbox.color_log_neutral,
			text_replacer = nameReplacer
		});
		kingdom_destroyed = add(new WorldLogAsset
		{
			id = "kingdom_destroyed",
			group = "kingdoms",
			path_icon = "ui/Icons/actor_traits/iconPyromaniac",
			color = Toolbox.color_log_warning,
			text_replacer = nameReplacer
		});
		kingdom_shattered = add(new WorldLogAsset
		{
			id = "kingdom_shattered",
			group = "kingdoms",
			path_icon = "ui/Icons/actor_traits/iconPyromaniac",
			random_ids = 3,
			color = Toolbox.color_log_warning,
			text_replacer = kingdomReplacer
		});
		kingdom_fractured = add(new WorldLogAsset
		{
			id = "kingdom_fractured",
			group = "kingdoms",
			path_icon = "ui/Icons/actor_traits/iconPyromaniac",
			random_ids = 3,
			color = Toolbox.color_log_warning,
			text_replacer = kingdomReplacer
		});
		kingdom_royal_clan_new = add(new WorldLogAsset
		{
			id = "kingdom_royal_clan_new",
			group = "clans",
			path_icon = "ui/Icons/iconClan",
			color = Toolbox.color_log_neutral,
			random_ids = 3,
			text_replacer = delegate(WorldLogMessage pMessage, ref string pText)
			{
				updateText(ref pText, pMessage, "$kingdom$", 1);
				updateText(ref pText, pMessage, "$clan$", 2);
				updateText(ref pText, pMessage, "$king$", 3);
			}
		});
		kingdom_royal_clan_changed = add(new WorldLogAsset
		{
			id = "kingdom_royal_clan_changed",
			group = "clans",
			path_icon = "ui/Icons/iconClan",
			color = Toolbox.color_log_neutral,
			random_ids = 3,
			text_replacer = delegate(WorldLogMessage pMessage, ref string pText)
			{
				updateText(ref pText, pMessage, "$kingdom$", 1);
				updateText(ref pText, pMessage, "$old_clan$", 2);
				updateText(ref pText, pMessage, "$new_clan$", 3);
			}
		});
		kingdom_royal_clan_dead = add(new WorldLogAsset
		{
			id = "kingdom_royal_clan_dead",
			group = "clans",
			path_icon = "ui/Icons/iconClan",
			color = Toolbox.color_log_warning,
			random_ids = 3,
			text_replacer = delegate(WorldLogMessage pMessage, ref string pText)
			{
				updateText(ref pText, pMessage, "$kingdom$", 1);
				updateText(ref pText, pMessage, "$clan$", 2);
			}
		});
		city_destroyed = add(new WorldLogAsset
		{
			id = "city_destroyed",
			group = "cities",
			path_icon = "ui/Icons/actor_traits/iconPyromaniac",
			color = Toolbox.color_log_warning,
			text_replacer = nameReplacer
		});
		auto_tester = add(new WorldLogAsset
		{
			id = "auto_tester",
			path_icon = "ui/Icons/iconPlay",
			color = Toolbox.color_log_warning,
			text_replacer = delegate(WorldLogMessage pMessage, ref string pText)
			{
				pText = pMessage.special1;
			}
		});
		addDisasters();
	}

	private void addDisasters()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		add(new WorldLogAsset
		{
			id = "$basic_disaster$",
			color = Toolbox.color_log_warning,
			group = "disasters"
		});
		clone("disaster_tornado", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_tornado";
		t.path_icon = "ui/Icons/iconTornado";
		clone("disaster_meteorite", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_meteorite";
		t.path_icon = "ui/Icons/iconMeteorite";
		clone("disaster_hellspawn", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_hellspawn";
		t.path_icon = "ui/Icons/iconDemon";
		clone("disaster_earthquake", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_earthquake";
		t.path_icon = "ui/Icons/iconEarthquake";
		clone("disaster_greg_abominations", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_greg_abominations";
		t.path_icon = "ui/Icons/iconGreg";
		clone("disaster_ice_ones", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_ice_ones";
		t.path_icon = "ui/Icons/iconWalker";
		clone("disaster_sudden_snowman", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_sudden_snowman";
		t.path_icon = "ui/Icons/iconSnowMan";
		clone("disaster_garden_surprise", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_garden_surprise";
		t.path_icon = "ui/Icons/iconSuperPumpkin";
		clone("disaster_dragon_from_farlands", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_dragon_from_farlands";
		t.path_icon = "ui/Icons/iconDragon";
		clone("disaster_bandits", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_bandits";
		t.path_icon = "ui/Icons/iconBandit";
		clone("disaster_alien_invasion", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_alien_invasion";
		t.path_icon = "ui/Icons/iconUfo";
		clone("disaster_biomass", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_biomass";
		t.path_icon = "ui/Icons/iconBiomass";
		clone("disaster_tumor", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_tumor";
		t.path_icon = "ui/Icons/iconTumor";
		clone("disaster_heatwave", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_heatwave";
		t.path_icon = "ui/Icons/iconFire";
		clone("disaster_evil_mage", "$basic_disaster$");
		t.locale_id = "worldlog_disaster_evil_mage";
		t.path_icon = "ui/Icons/iconEvilMage";
		t.text_replacer = nameReplacer;
		clone("$city_name_disaster$", "$basic_disaster$");
		t.text_replacer = delegate(WorldLogMessage pMessage, ref string pText)
		{
			updateText(ref pText, pMessage, "$name$", 1);
			updateText(ref pText, pMessage, "$city$", 2);
		};
		clone("disaster_underground_necromancer", "$city_name_disaster$");
		t.locale_id = "worldlog_disaster_underground_necromancer";
		t.path_icon = "ui/Icons/iconNecromancer";
		clone("disaster_mad_thoughts", "$city_name_disaster$");
		t.locale_id = "worldlog_disaster_mad_thoughts";
		t.path_icon = "ui/Icons/actor_traits/iconMadness";
	}

	private void nameReplacer(WorldLogMessage pMessage, ref string pText)
	{
		updateText(ref pText, pMessage, "$name$", 1);
	}

	private void kingReplacer(WorldLogMessage pMessage, ref string pText)
	{
		updateText(ref pText, pMessage, "$kingdom$", 1);
		updateText(ref pText, pMessage, "$king$", 2);
	}

	private void kingCityReplacer(WorldLogMessage pMessage, ref string pText)
	{
		updateText(ref pText, pMessage, "$kingdom$", 1);
		updateText(ref pText, pMessage, "$king$", 2);
		updateText(ref pText, pMessage, "$city$", 3);
	}

	private void kingdomReplacer(WorldLogMessage pMessage, ref string pText)
	{
		updateText(ref pText, pMessage, "$kingdom$", 1);
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (WorldLogAsset item in list)
		{
			foreach (string localeID in item.getLocaleIDs())
			{
				checkLocale(item, localeID);
			}
		}
	}
}
