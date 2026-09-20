using System.Collections.Generic;

public class ItemModifierLibrary : ItemAssetLibrary<ItemModAsset>
{
	public const string WEAPON = "weapon";

	public const string ACCESSORY = "accessory";

	public const string ARMOR = "armor";

	public const string ALL = "weapon,accessory,armor";

	public Dictionary<string, List<ItemModAsset>> pools = new Dictionary<string, List<ItemModAsset>>();

	public override void init()
	{
		base.init();
		ItemModAsset obj = new ItemModAsset
		{
			id = "normal",
			mod_type = "normal",
			pool = "weapon,accessory,armor",
			rarity = 5,
			mod_can_be_given = false,
			has_locales = false
		};
		ItemModAsset pAsset = obj;
		t = obj;
		add(pAsset);
		ItemModAsset obj2 = new ItemModAsset
		{
			id = "power1",
			mod_type = "power",
			translation_key = "mod_power",
			rarity = 1,
			pool = "weapon",
			mod_rank = 1
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.base_stats["damage"] = 1f;
		clone("power2", "power1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 2;
		t.base_stats["damage"] = 2f;
		clone("power3", "power1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 3;
		t.base_stats["damage"] = 3f;
		clone("power4", "power1");
		t.quality = Rarity.R2_Epic;
		t.mod_rank = 4;
		t.base_stats["damage"] = 4f;
		clone("power5", "power1");
		t.quality = Rarity.R3_Legendary;
		t.mod_rank = 5;
		t.base_stats["damage"] = 5f;
		ItemModAsset obj3 = new ItemModAsset
		{
			id = "truth1",
			mod_type = "truth",
			translation_key = "mod_truth",
			rarity = 1,
			pool = "weapon",
			mod_rank = 1
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.base_stats["diplomacy"] = 1f;
		clone("truth2", "truth1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 2;
		t.base_stats["diplomacy"] = 2f;
		clone("truth3", "truth1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 3;
		t.base_stats["diplomacy"] = 3f;
		clone("truth4", "truth1");
		t.quality = Rarity.R2_Epic;
		t.mod_rank = 4;
		t.base_stats["diplomacy"] = 4f;
		clone("truth5", "truth1");
		t.quality = Rarity.R3_Legendary;
		t.mod_rank = 5;
		t.base_stats["diplomacy"] = 5f;
		ItemModAsset obj4 = new ItemModAsset
		{
			id = "protection1",
			mod_type = "protection",
			translation_key = "mod_protection",
			rarity = 1,
			pool = "armor"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.base_stats["armor"] = 3f;
		clone("protection2", "protection1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 2;
		t.base_stats["armor"] = 4f;
		clone("protection3", "protection1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 3;
		t.base_stats["armor"] = 5f;
		clone("protection4", "protection1");
		t.quality = Rarity.R2_Epic;
		t.mod_rank = 4;
		t.base_stats["armor"] = 6f;
		clone("protection5", "protection1");
		t.quality = Rarity.R3_Legendary;
		t.mod_rank = 5;
		t.base_stats["armor"] = 7f;
		ItemModAsset obj5 = new ItemModAsset
		{
			id = "speed1",
			mod_type = "speed",
			translation_key = "mod_speed",
			rarity = 3,
			pool = "weapon,accessory,armor"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.base_stats["speed"] = 2f;
		clone("speed2", "speed1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 2;
		t.base_stats["speed"] = 3f;
		clone("speed3", "speed1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 3;
		t.base_stats["speed"] = 4f;
		clone("speed4", "speed1");
		t.quality = Rarity.R2_Epic;
		t.mod_rank = 4;
		t.base_stats["speed"] = 5f;
		clone("speed5", "speed1");
		t.quality = Rarity.R3_Legendary;
		t.mod_rank = 5;
		t.base_stats["speed"] = 6f;
		ItemModAsset obj6 = new ItemModAsset
		{
			id = "balance1",
			mod_type = "balance",
			translation_key = "mod_balance",
			rarity = 3,
			pool = "weapon,accessory,armor"
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.base_stats["critical_chance"] = 0.05f;
		clone("balance2", "balance1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 2;
		t.base_stats["critical_chance"] = 0.15f;
		clone("balance3", "balance1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 3;
		t.base_stats["critical_chance"] = 0.2f;
		clone("balance4", "balance1");
		t.quality = Rarity.R2_Epic;
		t.mod_rank = 4;
		t.base_stats["critical_chance"] = 0.25f;
		clone("balance5", "balance1");
		t.quality = Rarity.R3_Legendary;
		t.mod_rank = 5;
		t.base_stats["critical_chance"] = 0.3f;
		ItemModAsset obj7 = new ItemModAsset
		{
			id = "health1",
			mod_type = "health",
			translation_key = "mod_health",
			rarity = 1,
			pool = "armoraccessory"
		};
		pAsset = obj7;
		t = obj7;
		add(pAsset);
		t.base_stats["health"] = 50f;
		clone("health2", "health1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 2;
		t.base_stats["health"] = 100f;
		clone("health3", "health1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 3;
		t.base_stats["health"] = 150f;
		clone("health4", "health1");
		t.quality = Rarity.R2_Epic;
		t.mod_rank = 4;
		t.base_stats["health"] = 200f;
		clone("health5", "health1");
		t.quality = Rarity.R3_Legendary;
		t.mod_rank = 5;
		t.base_stats["health"] = 250f;
		ItemModAsset obj8 = new ItemModAsset
		{
			id = "finesse1",
			mod_type = "finesse",
			translation_key = "mod_finesse",
			rarity = 1,
			pool = "weaponaccessory"
		};
		pAsset = obj8;
		t = obj8;
		add(pAsset);
		t.base_stats["attack_speed"] = 1f;
		clone("finesse2", "finesse1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 2;
		t.base_stats["attack_speed"] = 1f;
		t.base_stats["speed"] = 1f;
		clone("finesse3", "finesse1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 3;
		t.base_stats["attack_speed"] = 2f;
		t.base_stats["speed"] = 1f;
		clone("finesse4", "finesse1");
		t.quality = Rarity.R2_Epic;
		t.mod_rank = 4;
		t.base_stats["attack_speed"] = 2f;
		t.base_stats["speed"] = 2f;
		clone("finesse5", "finesse1");
		t.quality = Rarity.R3_Legendary;
		t.mod_rank = 5;
		t.base_stats["attack_speed"] = 3f;
		t.base_stats["speed"] = 3f;
		ItemModAsset obj9 = new ItemModAsset
		{
			id = "mastery1",
			mod_type = "mastery",
			translation_key = "mod_mastery",
			rarity = 1,
			pool = "accessory",
			mod_rank = 1
		};
		pAsset = obj9;
		t = obj9;
		add(pAsset);
		t.base_stats["warfare"] = 1f;
		clone("mastery2", "mastery1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 2;
		t.base_stats["warfare"] = 2f;
		clone("mastery3", "mastery1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 3;
		t.base_stats["warfare"] = 3f;
		clone("mastery4", "mastery1");
		t.quality = Rarity.R2_Epic;
		t.mod_rank = 4;
		t.base_stats["warfare"] = 4f;
		clone("mastery5", "mastery1");
		t.quality = Rarity.R3_Legendary;
		t.mod_rank = 5;
		t.base_stats["warfare"] = 5f;
		ItemModAsset obj10 = new ItemModAsset
		{
			id = "knowledge1",
			mod_type = "knowledge",
			translation_key = "mod_knowledge",
			rarity = 1,
			pool = "accessory",
			mod_rank = 1
		};
		pAsset = obj10;
		t = obj10;
		add(pAsset);
		t.base_stats["intelligence"] = 1f;
		clone("knowledge2", "knowledge1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 2;
		t.base_stats["intelligence"] = 2f;
		clone("knowledge3", "knowledge1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 3;
		t.base_stats["intelligence"] = 3f;
		clone("knowledge4", "knowledge1");
		t.quality = Rarity.R2_Epic;
		t.mod_rank = 4;
		t.base_stats["intelligence"] = 4f;
		clone("knowledge5", "knowledge1");
		t.quality = Rarity.R3_Legendary;
		t.mod_rank = 5;
		t.base_stats["intelligence"] = 7f;
		ItemModAsset obj11 = new ItemModAsset
		{
			id = "sharpness1",
			mod_type = "sharpness",
			translation_key = "mod_sharpness",
			rarity = 1,
			pool = "accessory",
			mod_rank = 1
		};
		pAsset = obj11;
		t = obj11;
		add(pAsset);
		t.base_stats["damage"] = 1f;
		t.base_stats["multiplier_crit"] = 0.05f;
		clone("sharpness2", "sharpness1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 2;
		t.base_stats["damage"] = 2f;
		t.base_stats["multiplier_crit"] = 0.06f;
		clone("sharpness3", "sharpness1");
		t.quality = Rarity.R1_Rare;
		t.mod_rank = 3;
		t.base_stats["damage"] = 3f;
		t.base_stats["multiplier_crit"] = 0.08f;
		clone("sharpness4", "sharpness1");
		t.quality = Rarity.R2_Epic;
		t.mod_rank = 4;
		t.base_stats["damage"] = 5f;
		t.base_stats["multiplier_crit"] = 0.1f;
		clone("sharpness5", "sharpness1");
		t.quality = Rarity.R3_Legendary;
		t.mod_rank = 5;
		t.base_stats["damage"] = 10f;
		t.base_stats["multiplier_crit"] = 0.1f;
		ItemModAsset obj12 = new ItemModAsset
		{
			id = "flame",
			mod_type = "flame",
			translation_key = "mod_flame",
			rarity = 0,
			pool = "weapon",
			mod_rank = 1
		};
		pAsset = obj12;
		t = obj12;
		add(pAsset);
		t.base_stats.addTag("immunity_fire");
		t.action_attack_target = ActionLibrary.addBurningEffectOnTarget;
		t.action_special_effect = ActionLibrary.flamingWeapon;
		t.special_effect_interval = 0.1f;
		ItemModAsset obj13 = new ItemModAsset
		{
			id = "ice",
			mod_type = "ice",
			translation_key = "mod_ice",
			rarity = 0,
			pool = "weapon",
			mod_rank = 1
		};
		pAsset = obj13;
		t = obj13;
		add(pAsset);
		t.base_stats.addTag("immunity_cold");
		t.action_attack_target = ActionLibrary.addFrozenEffectOnTarget20;
		ItemModAsset obj14 = new ItemModAsset
		{
			id = "stun",
			mod_type = "stun",
			translation_key = "mod_stun",
			rarity = 0,
			pool = "weapon",
			mod_rank = 1
		};
		pAsset = obj14;
		t = obj14;
		add(pAsset);
		t.action_attack_target = ActionLibrary.addStunnedEffectOnTarget20;
		ItemModAsset obj15 = new ItemModAsset
		{
			id = "slowness",
			mod_type = "slowness",
			translation_key = "mod_slowness",
			rarity = 0,
			pool = "weapon",
			mod_rank = 1
		};
		pAsset = obj15;
		t = obj15;
		add(pAsset);
		t.action_attack_target = ActionLibrary.addSlowEffectOnTarget20;
		ItemModAsset obj16 = new ItemModAsset
		{
			id = "poison",
			mod_type = "poison",
			translation_key = "mod_poison",
			rarity = 0,
			pool = "weapon",
			mod_rank = 1
		};
		pAsset = obj16;
		t = obj16;
		add(pAsset);
		t.action_attack_target = ActionLibrary.addPoisonedEffectOnTarget;
		ItemModAsset obj17 = new ItemModAsset
		{
			id = "eternal",
			mod_type = "eternal",
			translation_key = "mod_eternal",
			rarity = 0,
			pool = "weapon,accessory,armor",
			mod_rank = 5
		};
		pAsset = obj17;
		t = obj17;
		add(pAsset);
		ItemModAsset obj18 = new ItemModAsset
		{
			id = "cursed",
			mod_type = "cursed",
			translation_key = "mod_cursed",
			rarity = 0,
			pool = "weapon,accessory,armor",
			mod_rank = 5
		};
		pAsset = obj18;
		t = obj18;
		add(pAsset);
		ItemModAsset obj19 = new ItemModAsset
		{
			id = "divine_rune",
			mod_type = "divine_rune",
			translation_key = "divine_rune",
			rarity = 0,
			pool = "weapon,accessory,armor",
			mod_rank = 5
		};
		pAsset = obj19;
		t = obj19;
		add(pAsset);
	}

	public override void linkAssets()
	{
		base.linkAssets();
		pools.Add("weapon", new List<ItemModAsset>());
		pools.Add("armor", new List<ItemModAsset>());
		pools.Add("accessory", new List<ItemModAsset>());
		foreach (ItemModAsset item in list)
		{
			for (int i = 0; i < item.rarity; i++)
			{
				if (item.pool.Contains("weapon"))
				{
					pools["weapon"].Add(item);
				}
				if (item.pool.Contains("armor"))
				{
					pools["armor"].Add(item);
				}
				if (item.pool.Contains("accessory"))
				{
					pools["accessory"].Add(item);
				}
			}
		}
	}
}
