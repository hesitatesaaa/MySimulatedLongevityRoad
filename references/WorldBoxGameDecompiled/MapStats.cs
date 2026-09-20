using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

[Serializable]
public class MapStats
{
	public string name = "WorldBox";

	public string description = "";

	public string player_name;

	public string player_mood;

	public SaveCustomData custom_data;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public double world_time;

	public int history_current_year = -1;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string world_age_id;

	public int world_age_slot_index;

	public double world_age_started_at;

	public double same_world_age_started_at;

	public float current_world_ages_duration;

	public float current_age_progress;

	public bool is_world_ages_paused;

	[DefaultValue(1f)]
	public float world_ages_speed_multiplier = 1f;

	public string[] world_ages_slots;

	public long housesBuilt;

	public long housesDestroyed;

	public long population;

	public long creaturesBorn;

	public long creaturesCreated;

	public long subspeciesCreated;

	public long subspeciesExtinct;

	public long languagesCreated;

	public long languagesForgotten;

	public long booksWritten;

	public long booksRead;

	public long booksBurnt;

	public long culturesCreated;

	public long culturesForgotten;

	public long religionsCreated;

	public long religionsForgotten;

	public long kingdomsCreated;

	public long kingdomsDestroyed;

	public long citiesCreated;

	public long citiesConquered;

	public long citiesRebelled;

	public long citiesDestroyed;

	public long alliancesMade;

	public long alliancesDissolved;

	public long warsStarted;

	public long peacesMade;

	public long familiesCreated;

	public long armiesCreated;

	public long armiesDestroyed;

	public long familiesDestroyed;

	public long clansCreated;

	public long clansDestroyed;

	public long plotsStarted;

	public long plotsSucceeded;

	public long plotsForgotten;

	public double exploding_mushrooms_enabled_at;

	[DefaultValue(1L)]
	public long id_unit = 1L;

	[DefaultValue(1L)]
	public long id_building = 1L;

	[DefaultValue(1L)]
	public long id_kingdom = 1L;

	[DefaultValue(1L)]
	public long id_city = 1L;

	[DefaultValue(1L)]
	public long id_culture = 1L;

	[DefaultValue(1L)]
	public long id_clan = 1L;

	[DefaultValue(1L)]
	public long id_alliance = 1L;

	[DefaultValue(1L)]
	public long id_war = 1L;

	[DefaultValue(1L)]
	public long id_projectile = 1L;

	[DefaultValue(1L)]
	public long id_status = 1L;

	[DefaultValue(1L)]
	public long id_plot = 1L;

	[DefaultValue(1L)]
	public long id_book = 1L;

	[DefaultValue(1L)]
	public long id_subspecies = 1L;

	[DefaultValue(1L)]
	public long id_family = 1L;

	[DefaultValue(1L)]
	public long id_army = 1L;

	[DefaultValue(1L)]
	public long id_language = 1L;

	[DefaultValue(1L)]
	public long id_religion = 1L;

	[DefaultValue(1L)]
	public long id_item = 1L;

	[DefaultValue(1L)]
	public long id_diplomacy = 1L;

	[DefaultValue(1L)]
	public long life_dna = 1L;

	[NonSerialized]
	public long current_infected;

	[NonSerialized]
	public long current_mobs;

	[NonSerialized]
	public long current_houses;

	[NonSerialized]
	public long current_vegetation;

	[NonSerialized]
	public long current_infected_plague;

	private int _last_year = -1;

	private int _last_month = -1;

	private float _timer_stats = 0.1f;

	public static string[] possible_formats = new string[20]
	{
		"pr_", "st_", "w_", "a_", "c_", "u_", "b_", "k_", "c_", "cl_",
		"p_", "bo_", "sp_", "lang_", "rel_", "it_", "f_", "fa_", "army_", "d_"
	};

	[Preserve]
	[Obsolete("use .world_age_id instead", true)]
	public string era_id
	{
		set
		{
			if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(world_age_id))
			{
				world_age_id = value;
			}
		}
	}

	public long deaths { get; set; }

	public long deaths_age { get; set; }

	public long deaths_hunger { get; set; }

	public long deaths_eaten { get; set; }

	public long deaths_plague { get; set; }

	public long deaths_poison { get; set; }

	public long deaths_infection { get; set; }

	public long deaths_tumor { get; set; }

	public long deaths_acid { get; set; }

	public long deaths_fire { get; set; }

	public long deaths_divine { get; set; }

	public long deaths_weapon { get; set; }

	public long deaths_gravity { get; set; }

	public long deaths_drowning { get; set; }

	public long deaths_water { get; set; }

	public long deaths_explosion { get; set; }

	public long metamorphosis { get; set; }

	public long evolutions { get; set; }

	public long deaths_other { get; set; }

	public long deaths_smile { get; set; }

	[JsonIgnore]
	public int year => _last_year;

	[Preserve]
	[JsonProperty("year")]
	[Obsolete("use .world_time instead", true)]
	public int year_obsolete
	{
		set
		{
			if (value != 0)
			{
				world_time += (float)value * 60f;
			}
		}
	}

	[Preserve]
	[JsonProperty("month")]
	[Obsolete("use .world_time instead", true)]
	public int month_obsolete
	{
		set
		{
			if (value != 0)
			{
				world_time += (float)value * 5f;
			}
		}
	}

	[Preserve]
	[JsonProperty("worldTime")]
	[Obsolete("use .world_time instead", true)]
	public double worldTime_obsolete
	{
		set
		{
			if (value != 0.0)
			{
				world_time += value;
			}
		}
	}

	public MapStats()
	{
		checkDefault();
	}

	private void checkDefault()
	{
		if (world_ages_slots == null)
		{
			world_ages_slots = new string[8];
		}
		if (string.IsNullOrEmpty(player_name))
		{
			player_name = "The Creator";
		}
		if (string.IsNullOrEmpty(player_mood))
		{
			setDefaultMood();
		}
		if (custom_data == null)
		{
			custom_data = new SaveCustomData();
		}
	}

	public void generateLifeDNA()
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		long num = long.Parse(DateTime.UtcNow.ToString("yyyyMMddHH", invariantCulture));
		life_dna = num;
	}

	internal void updateStatsForPanel(float pElapsed)
	{
		if (_timer_stats > 0f)
		{
			_timer_stats -= pElapsed;
			return;
		}
		_timer_stats = 0.1f;
		recalcCounters();
	}

	internal void updateWorldTime(float pElapsed)
	{
		world_time += pElapsed;
		int currentYear = Date.getCurrentYear();
		int currentMonth = Date.getCurrentMonth();
		if (_last_year != currentYear)
		{
			World.world.updateObjectAge();
		}
		if (_last_year != currentYear)
		{
			_last_month = -1;
		}
		_last_year = currentYear;
		_last_month = currentMonth;
	}

	public void load()
	{
		checkDefault();
		_last_year = Date.getCurrentYear();
		_last_month = Date.getCurrentMonth();
	}

	private void recalcCounters()
	{
		current_infected = 0L;
		current_mobs = 0L;
		current_houses = 0L;
		current_vegetation = 0L;
		current_infected_plague = 0L;
		List<Actor> simpleList = World.world.units.getSimpleList();
		int i = 0;
		for (int count = simpleList.Count; i < count; i++)
		{
			Actor actor = simpleList[i];
			if (actor.hasTrait("plague"))
			{
				current_infected_plague++;
			}
			if (actor.isSick())
			{
				current_infected++;
			}
			if (actor.asset.count_as_unit && !actor.isSapient())
			{
				current_mobs++;
			}
		}
		List<Building> simpleList2 = World.world.buildings.getSimpleList();
		int j = 0;
		for (int count2 = simpleList2.Count; j < count2; j++)
		{
			Building building = simpleList2[j];
			if (building.isCiv())
			{
				current_houses++;
			}
			else if (building.asset.is_vegetation)
			{
				current_vegetation++;
			}
		}
	}

	public long getNextId(string pType)
	{
		long result = 0L;
		switch (pType)
		{
		case "projectile":
			result = id_projectile++;
			break;
		case "statuses":
			result = id_status++;
			break;
		case "war":
			result = id_war++;
			break;
		case "alliance":
			result = id_alliance++;
			break;
		case "culture":
			result = id_culture++;
			break;
		case "unit":
			result = id_unit++;
			break;
		case "building":
			result = id_building++;
			break;
		case "kingdom":
			result = id_kingdom++;
			break;
		case "city":
			result = id_city++;
			break;
		case "clan":
			result = id_clan++;
			break;
		case "plot":
			result = id_plot++;
			break;
		case "book":
			result = id_book++;
			break;
		case "subspecies":
			result = id_subspecies++;
			break;
		case "language":
			result = id_language++;
			break;
		case "religion":
			result = id_religion++;
			break;
		case "item":
			result = id_item++;
			break;
		case "family":
			result = id_family++;
			break;
		case "army":
			result = id_army++;
			break;
		case "diplomacy":
			result = id_diplomacy++;
			break;
		default:
			Debug.LogError((object)("NO pType for id " + pType));
			break;
		}
		return result;
	}

	public static string formatId(string pType, long pID)
	{
		switch (pType)
		{
		case "projectile":
			return "pr_" + pID;
		case "statuses":
			return "st_" + pID;
		case "war":
			return "w_" + pID;
		case "alliance":
			return "a_" + pID;
		case "culture":
			return "c_" + pID;
		case "unit":
			return "u_" + pID;
		case "building":
			return "b_" + pID;
		case "kingdom":
			return "k_" + pID;
		case "city":
			return "c_" + pID;
		case "clan":
			return "cl_" + pID;
		case "plot":
			return "p_" + pID;
		case "book":
			return "bo_" + pID;
		case "subspecies":
			return "sp_" + pID;
		case "language":
			return "lang_" + pID;
		case "religion":
			return "rel_" + pID;
		case "item":
			return "it_" + pID;
		case "family":
			return "fa_" + pID;
		case "army":
			return "army_" + pID;
		case "diplomacy":
			return "d_" + pID;
		default:
			Debug.LogError((object)("NO pType for id " + pType));
			return "???_" + pID;
		}
	}

	public void debug(DebugTool pTool)
	{
		pTool.setText("(d)worldTime:", world_time, 0f, pShowBar: false, 0L);
		pTool.setText("(f)worldTime:", getWorldTime(), 0f, pShowBar: false, 0L);
		pTool.setText("cur month:", Date.getCurrentMonth(), 0f, pShowBar: false, 0L);
		pTool.setText("cur year:", Date.getCurrentYear(), 0f, pShowBar: false, 0L);
		pTool.setText("last_year:", _last_year, 0f, pShowBar: false, 0L);
		pTool.setText("last_month:", _last_month, 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("months since 0:", Date.getMonthsSince(0.0), 0f, pShowBar: false, 0L);
		pTool.setText("years since 0:", Date.getYearsSince(0.0), 0f, pShowBar: false, 0L);
		pTool.setText("months since now:", Date.getMonthsSince(world_time), 0f, pShowBar: false, 0L);
		pTool.setText("years since now:", Date.getYearsSince(world_time), 0f, pShowBar: false, 0L);
		pTool.setText("month time:", Date.getMonthTime(), 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("getDate 0:", Date.getDate(0.0), 0f, pShowBar: false, 0L);
		pTool.setText("getYearDate 0:", Date.getYearDate(0.0), 0f, pShowBar: false, 0L);
		pTool.setText("getYear 0:", Date.getYear(0.0), 0f, pShowBar: false, 0L);
		pTool.setText("getYear0 0:", Date.getYear0(0.0), 0f, pShowBar: false, 0L);
		pTool.setText("getDate now:", Date.getDate(world_time), 0f, pShowBar: false, 0L);
		pTool.setText("getYearDate now:", Date.getYearDate(world_time), 0f, pShowBar: false, 0L);
		pTool.setText("getYear now:", Date.getYear(world_time), 0f, pShowBar: false, 0L);
		pTool.setText("getYear0 now:", Date.getYear0(world_time), 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("max_float:", float.MaxValue, 0f, pShowBar: false, 0L);
	}

	public float getWorldTime()
	{
		return (float)world_time;
	}

	public void initNewWorld()
	{
		generateLifeDNA();
		generatePlayerName();
		setDefaultMood();
		name = NameGenerator.getName("world_name");
		AssetManager.gene_library.regenerateBasicDNACodesWithLifeSeed(life_dna);
	}

	private void generatePlayerName()
	{
		player_name = NameGenerator.getName("player_name", ActorSex.Male, pForceLegacy: false, null, null, pIgnoreBlackList: true);
	}

	private void setDefaultMood()
	{
		player_mood = "serene";
	}

	public ArchitectMood getArchitectMood()
	{
		if (string.IsNullOrEmpty(player_mood))
		{
			player_mood = "serene";
		}
		ArchitectMood architectMood = AssetManager.architect_mood_library.get(player_mood);
		if (architectMood == null)
		{
			architectMood = AssetManager.architect_mood_library.get("serene");
		}
		return architectMood;
	}
}
