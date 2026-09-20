using UnityEngine;

public class Globals
{
	public const string HIDDEN_ID = "[HIDDEN]";

	public const int TOTAL_AGE_SLOTS = 8;

	public const long NEUTRAL_KINGDOM_NUMERIC_ID = 0L;

	public const int PAST_RULERS_LIMIT = 30;

	public const int TOTAL_RANKS = 10;

	public const float UNIT_COLOR_EFFECT_TIME = 0.3f;

	public const float PLOT_REMOVAL_EFFECT_TIME = 1f;

	public const int CITY_ITEMS_MAX = 15;

	public const int CITY_MIN_ISLAND_TILES = 300;

	public const int CITY_MIN_CAN_BE_FARMS_TILES = 5;

	public const float CITY_TIMER_SUPPLY_DEFAULT = 30f;

	public const float REGION_UPDATE_TIMEOUT = 0.5f;

	public const int DEFAULT_CONSTRUCTION_SPEED = 2;

	public const int HAPPINESS_FROM_BREEDING_ACTION = 40;

	public const float FAST_SWIMMING_SPEED_MULTIPLIER = 5f;

	public const int CULTURE_SPREAD_CHANCE_FROM_BOOK = 3;

	public const int CULTURE_SPREAD_CHANCE_FROM_TALK = 3;

	public const int LANGUAGE_SPREAD_CHANCE_FROM_TALK = 3;

	public const int LANGUAGE_SPREAD_CHANCE_FROM_BOOK = 3;

	public const int RELIGION_SPREAD_CHANCE_FROM_TALK = 3;

	public const int RELIGION_SPREAD_CHANCE_FROM_BOOK = 3;

	public const int CITY_POP_FOR_SETTLE_TARGET = 22;

	public static readonly bool AI_TEST_ACTIVE;

	public const float DEFAULT_SCALE_EFFECT = 0.25f;

	public const float DEFAULT_SCALE_UNIT = 0.1f;

	public const float DEFAULT_SCALE_BUILDING = 0.25f;

	public const float DEFAULT_ARROW_START_HEIGHT = 0.6f;

	public const float DEFAULT_PROJECTILE_START_HEIGHT = 0.2f;

	public const float DEFAULT_SLASH_POSITION = 0.5f;

	public const float DEFAULT_UNIT_SPEED = 10f;

	public const float DEFAULT_UNIT_SPEED_MIN = 4f;

	public const float SPEED_MULTIPLIER_IN_LIQUID_WITHOUT_STAMINA = 0.4f;

	public const float ACCURACY_MIN = 1f;

	public const float ACCURACY_MAX = 10f;

	public const float FIRE_EXTINGUISH_CHANCE_FROM_RAIN = 0.9f;

	public static readonly bool DIAGNOSTIC;

	public static readonly bool TRAILER_MODE;

	public static bool TRAILER_MODE_USE_RESOURCES;

	public static bool TRAILER_MODE_UPGRADE_BUILDINGS;

	public const int BOAT_TYPE_LIMIT_PER_DOCK = 1;

	public const int CAST_HEIGHT = 15;

	public const int UNITS_IN_SPAWNER_PER_REGION = 4;

	public const int PATHFINDER_REGION_LIMIT = 4;

	public const float DAYS_IN_YEAR = 360f;

	public const float DAYS_IN_MONTH = 30f;

	public const float MONTHS_IN_YEAR = 12f;

	public const float MONTH_TIME = 5f;

	public const float YEAR_TIME = 60f;

	public const int ATTRIBUTE_BAD = 4;

	public const int ATTRIBUTE_NORMAL = 9;

	public const int ATTRIBUTE_GOOD = 20;

	public const int BUILDING_TOWER_CAPTURE_POINTS = 10;

	public const int GENERATOR_MAX_TREES_PER_ZONE = 3;

	public const float MIN_ADS_INTERVAL_MINUTES = 2f;

	public const float ADS_INTERVAL_MINUTES = 5f;

	public static readonly bool specialAbstudio;

	public const float SAME_GRASS_CHANCE = 0.05f;

	public const int GREY_GOO_DAMAGE = 50;

	public const int DAMAGE_ACID = 20;

	public const int DAMAGE_HEAT = 50;

	public const int MINIMUM_UNITS_IN_CITY_BEFORE_POP_POINTS = 50;

	public const int CITIZEN_FOOD_COST = 1;

	public const int ZONES_BETWEEN_CITIES = 3;

	public const int BOMB_RANGE_ATOMIC_NUKE = 30;

	public const int BOMB_RANGE_CZAR_BOMB = 70;

	public const float SHAKE_DURATION = 0.3f;

	public const float SHAKE_INTERVAL = 0.01f;

	public const float SHAKE_INTENSITY = 2f;

	public const bool SHAKE_X = false;

	public const bool SHAKE_Y = true;

	public const float COUNTER_TWEEN_DURATION = 0.45f;

	public const int ANIMAL_BABY_MAKING_UNITS_AROUND_LIMIT = 6;

	public const int FOOD_FROM_FISH = 1;

	public const int FOOD_FROM_TILE = 1;

	public const int FOOD_FROM_FARM = 2;

	public const int FARM_DISTANCE_AREA = 9;

	public const int ANIMAL_GOOD_FOR_HUNTING_AGE = 3;

	public const int MAX_UNIT_LEVEL = 9999;

	public const int BOAT_UNITS_LIMIT = 100;

	public const int TRANSPORT_WAIT_TRY_LIMIT = 4;

	public const float COLORS_ZONE_ALPHA = 0.78f;

	public const int ISLAND_TILES_FOR_DOCKS = 2500;

	public const int FOOD_BUSH_REGROW_TIME = 90;

	public const int FOOD_BEEHIVE_FULL = 10;

	internal const int REWARD_MINUTES = 30;

	internal const int REWARD_MINUTES_CLOCK = 720;

	internal const int REWARD_HOURS_CLOCK = 12;

	internal const int REWARD_DURATION = 1800;

	internal const int REWARD_DURATION_CLOCK = 43200;

	internal const int REWARD_SAVESLOT_HOURS = 3;

	internal const int REWARD_SAVESLOT_DURATION = 10800;

	internal const int REWARDS_PER_GIFT = 3;

	internal const int FREE_SAVE_SLOTS = 15;

	internal const int REWARD_AD_SAVESLOTS_OLD = 6;

	internal const int REWARD_AD_SAVESLOTS_10HRS = 3;

	internal const int REWARD_AD_SAVESLOTS_20HRS = 6;

	internal const int DISCORD_MEMBERS = 560000;

	internal const int FACEBOOK_MEMBERS = 82000;

	internal const int TWITTER_MEMBERS = 56000;

	internal const int REDDIT_MEMBERS = 140000;

	internal const string WB_EXAMPLE_MAP = "WB-5555-1166-5555";

	public const int TIMEOUT_CONQUER = 300;

	public static readonly Vector3 POINT_IN_VOID;

	public static readonly Vector2 POINT_IN_VOID_2;

	public const string NORMAL = "0";

	public const int RATE_US_ID = 12;

	public const string voteLink = "https://play.google.com/store/apps/details?id=com.mkarpenko.worldbox";

	internal static Vector3 emptyVector;

	public const int TOTAL_WORLD_AGE_SLOTS = 8;

	static Globals()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		AI_TEST_ACTIVE = true;
		DIAGNOSTIC = false;
		TRAILER_MODE = false;
		TRAILER_MODE_USE_RESOURCES = true;
		TRAILER_MODE_UPGRADE_BUILDINGS = true;
		specialAbstudio = false;
		POINT_IN_VOID = new Vector3(-1000000f, -1000000f);
		POINT_IN_VOID_2 = new Vector2(-1000000f, -1000000f);
		emptyVector = new Vector3(-100000f, -10000f);
	}
}
