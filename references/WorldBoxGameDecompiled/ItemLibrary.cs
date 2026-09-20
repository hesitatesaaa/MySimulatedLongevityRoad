using System;
using System.Collections.Generic;
using Beebyte.Obfuscator;
using UnityEngine;

[ObfuscateLiterals]
public class ItemLibrary : ItemAssetLibrary<EquipmentAsset>
{
	[NonSerialized]
	public List<EquipmentAsset> pot_weapon_assets_all = new List<EquipmentAsset>();

	[NonSerialized]
	public Dictionary<string, List<EquipmentAsset>> pot_equipment_by_groups_all = new Dictionary<string, List<EquipmentAsset>>();

	[NonSerialized]
	public List<EquipmentAsset> pot_weapon_assets_unlocked = new List<EquipmentAsset>();

	[NonSerialized]
	public Dictionary<string, List<EquipmentAsset>> pot_equipment_by_groups_unlocked = new Dictionary<string, List<EquipmentAsset>>();

	[NonSerialized]
	public Dictionary<string, List<EquipmentAsset>> equipment_by_subtypes = new Dictionary<string, List<EquipmentAsset>>();

	public static readonly string[] default_weapon_pool = new string[6] { "sword", "axe", "hammer", "spear", "bow", "stick" };

	public static EquipmentAsset base_attack;

	private const string TEMPLATE_EQUIPMENT = "$equipment";

	private const string TEMPLATE_ARMOR = "$armor";

	private const string TEMPLATE_BOOTS = "$boots";

	private const string TEMPLATE_HELMET = "$helmet";

	private const string TEMPLATE_ACCESSORY = "$accessory";

	private const string TEMPLATE_RING = "$ring";

	private const string TEMPLATE_AMULET = "$amulet";

	private const string TEMPLATE_WEAPON = "$weapon";

	private const string TEMPLATE_MELEE = "$melee";

	private const string TEMPLATE_RANGE = "$range";

	private const string TEMPLATE_BOW = "$bow";

	private const string TEMPLATE_SWORD = "$sword";

	private const string TEMPLATE_AXE = "$axe";

	private const string TEMPLATE_HAMMER = "$hammer";

	private const string TEMPLATE_SPEAR = "$spear";

	public override void init()
	{
		base.init();
		initTemplates();
		initNormalEquipment();
		initNormalWeapons();
		initWeaponsUnique();
		initBoats();
		initBaseAttacks();
	}

	public override void post_init()
	{
		foreach (EquipmentAsset item in list)
		{
			if (item.is_pool_weapon)
			{
				item.path_gameplay_sprite = "items/weapons/w_" + item.id;
			}
			if (string.IsNullOrEmpty(item.path_icon))
			{
				item.path_icon = "ui/Icons/items/icon_" + item.id;
				int num = 0;
				if (item.cost_resource_id_1 != "none")
				{
					ResourceAsset resourceAsset = AssetManager.resources.get(item.cost_resource_id_1);
					num += resourceAsset.money_cost;
				}
				if (item.cost_resource_id_2 != "none")
				{
					ResourceAsset resourceAsset2 = AssetManager.resources.get(item.cost_resource_id_2);
					num += resourceAsset2.money_cost;
				}
				item.cost_coins_resources = num;
			}
		}
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (EquipmentAsset item in list)
		{
			if (item.item_modifier_ids == null)
			{
				continue;
			}
			item.item_modifiers = new ItemModAsset[item.item_modifier_ids.Length];
			for (int i = 0; i < item.item_modifier_ids.Length; i++)
			{
				string text = item.item_modifier_ids[i];
				ItemModAsset itemModAsset = AssetManager.items_modifiers.get(text);
				if (itemModAsset == null)
				{
					BaseAssetLibrary.logAssetError("ItemLibrary: Item Modifier Asset <e>not found</e>", text);
				}
				else
				{
					item.item_modifiers[i] = itemModAsset;
				}
			}
		}
		fillSubtypesAndGroups();
		fillUnlockedPools();
		foreach (EquipmentAsset item2 in list)
		{
			item2.linkSpells();
		}
	}

	public override EquipmentAsset add(EquipmentAsset pAsset)
	{
		EquipmentAsset equipmentAsset = base.add(pAsset);
		if (equipmentAsset.base_stats == null)
		{
			equipmentAsset.base_stats = new BaseStats();
		}
		return equipmentAsset;
	}

	public string getEquipmentType(EquipmentType pType)
	{
		return pType switch
		{
			EquipmentType.Weapon => "weapon", 
			EquipmentType.Helmet => "helmet", 
			EquipmentType.Armor => "armor", 
			EquipmentType.Boots => "boots", 
			EquipmentType.Ring => "ring", 
			EquipmentType.Amulet => "amulet", 
			_ => null, 
		};
	}

	private void fillSubtypesAndGroups()
	{
		foreach (EquipmentAsset item in list)
		{
			if (!equipment_by_subtypes.ContainsKey(item.equipment_subtype))
			{
				equipment_by_subtypes.Add(item.equipment_subtype, new List<EquipmentAsset>());
			}
			equipment_by_subtypes[item.equipment_subtype].Add(item);
			if (item.is_pool_weapon)
			{
				pot_weapon_assets_all.Add(item);
			}
			if (!item.is_pool_weapon)
			{
				string group_id = item.group_id;
				if (!pot_equipment_by_groups_all.ContainsKey(group_id))
				{
					pot_equipment_by_groups_all.Add(group_id, new List<EquipmentAsset>());
				}
				pot_equipment_by_groups_all[group_id].Add(item);
			}
		}
	}

	private void fillUnlockedPools()
	{
		foreach (string item in GameProgress.instance.data.unlocked_equipment)
		{
			EquipmentAsset equipmentAsset = get(item);
			if (equipmentAsset == null)
			{
				continue;
			}
			if (equipmentAsset.is_pool_weapon && !pot_weapon_assets_unlocked.Contains(equipmentAsset))
			{
				pot_weapon_assets_unlocked.Add(equipmentAsset);
			}
			if (!equipmentAsset.is_pool_weapon)
			{
				string group_id = equipmentAsset.group_id;
				if (!pot_equipment_by_groups_unlocked.ContainsKey(group_id))
				{
					pot_equipment_by_groups_unlocked.Add(group_id, new List<EquipmentAsset>());
				}
				List<EquipmentAsset> list = pot_equipment_by_groups_unlocked[group_id];
				if (!list.Contains(equipmentAsset))
				{
					list.Add(equipmentAsset);
				}
			}
		}
	}

	public string addToGameplayReport(string pWhat)
	{
		string empty = string.Empty;
		empty = empty + pWhat + "\n";
		foreach (EquipmentAsset item in list)
		{
			if (item.has_locales && !item.isTemplateAsset())
			{
				string translatedName = item.getTranslatedName();
				string translatedDescription = item.getTranslatedDescription();
				string text = "\n" + translatedName;
				text += "\n";
				if (!string.IsNullOrEmpty(translatedDescription))
				{
					text = text + "1: " + translatedDescription;
				}
				empty += text;
			}
		}
		return empty + "\n\n";
	}

	public void loadSprites()
	{
		foreach (EquipmentAsset item in list)
		{
			if (item.is_pool_weapon)
			{
				item.gameplay_sprites = SpriteTextureLoader.getSpriteList(item.path_gameplay_sprite);
				if (item.gameplay_sprites.Length == 0)
				{
					Debug.LogError((object)("Weapon Texture is Missing: " + item.path_gameplay_sprite));
				}
			}
		}
	}

	private void initBaseAttacks()
	{
		base_attack = clone("base_attack", "$melee");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_base";
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("hands", "$melee");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_punch";
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("fire_hands", "hands");
		t.has_locales = false;
		t.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("jaws", "$melee");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_jaws";
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("claws", "$melee");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_claws";
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("snowball", "$range");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_punch";
		t.base_stats["range"] = 6f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.projectile = "snowball";
		t.base_stats["projectiles"] = 1f;
		t.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("ice");
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("bite", "$melee");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_jaws";
		t.attack_type = WeaponType.Melee;
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("rocks", "$range");
		t.has_locales = false;
		t.projectile = "rock";
		t.path_slash_animation = "effects/slashes/slash_punch";
		t.base_stats["range"] = 15f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["projectiles"] = 1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
	}

	private void initBoats()
	{
		clone("boat_cannonball", "$range");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_cannonball";
		t.base_stats["damage"] = 50f;
		t.base_stats["range"] = 14f;
		t.base_stats["targets"] = 4f;
		t.base_stats["area_of_effect"] = 3f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.projectile = "cannonball";
		t.base_stats["projectiles"] = 1f;
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("boat_arrow", "$range");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_bow";
		t.base_stats["damage"] = 30f;
		t.base_stats["range"] = 9f;
		t.base_stats["targets"] = 4f;
		t.base_stats["area_of_effect"] = 0f;
		t.base_stats["accuracy"] = 4f;
		t.base_stats["critical_chance"] = 0.2f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.base_stats["attack_speed"] = 0.5f;
		t.projectile = "arrow";
		t.base_stats["projectiles"] = 5f;
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("boat_snowball", "$range");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_snowball";
		t.base_stats["damage"] = 50f;
		t.base_stats["range"] = 14f;
		t.base_stats["targets"] = 4f;
		t.base_stats["area_of_effect"] = 3f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.projectile = "snowball";
		t.base_stats["projectiles"] = 1f;
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("boat_plasma_ball", "$range");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_plasma_ball";
		t.base_stats["damage"] = 65f;
		t.base_stats["range"] = 20f;
		t.base_stats["targets"] = 4f;
		t.base_stats["area_of_effect"] = 2f;
		t.base_stats["critical_chance"] = 0.2f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.projectile = "plasma_ball";
		t.base_stats["projectiles"] = 1f;
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("boat_necro_ball", "$range");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_necro_ball";
		t.base_stats["damage"] = 45f;
		t.base_stats["range"] = 12f;
		t.base_stats["targets"] = 4f;
		t.base_stats["area_of_effect"] = 3f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.projectile = "skull";
		t.base_stats["projectiles"] = 3f;
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("boat_fireball", "$range");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_cannonball";
		t.base_stats["damage"] = 30f;
		t.base_stats["range"] = 12f;
		t.base_stats["targets"] = 4f;
		t.base_stats["area_of_effect"] = 2f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.projectile = "fireball";
		t.base_stats["projectiles"] = 1f;
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("boat_freeze_ball", "$range");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_snowball";
		t.base_stats["damage"] = 30f;
		t.base_stats["range"] = 12f;
		t.base_stats["targets"] = 4f;
		t.base_stats["area_of_effect"] = 2f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.projectile = "freeze_orb";
		t.base_stats["projectiles"] = 3f;
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
		clone("boat_acid_ball", "$range");
		t.has_locales = false;
		t.path_slash_animation = "effects/slashes/slash_acid_ball";
		t.base_stats["damage"] = 50f;
		t.base_stats["range"] = 14f;
		t.base_stats["targets"] = 4f;
		t.base_stats["area_of_effect"] = 4f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.projectile = "acid_ball";
		t.base_stats["projectiles"] = 5f;
		t.show_in_meta_editor = false;
		t.show_in_knowledge_window = false;
	}

	private void initNormalEquipment()
	{
		initArmors();
		initBoots();
		initHelmets();
		initRings();
		initAmulets();
	}

	private void initAmulets()
	{
		clone("amulet_bone", "$amulet");
		t.material = "bone";
		t.equipment_value = 5;
		t.setCost(0, "bones", 1, "gems", 1);
		t.rigidity_rating = 1;
		t.base_stats["critical_chance"] = 0.02f;
		t.base_stats["stamina"] = 5f;
		clone("amulet_copper", "$amulet");
		t.material = "copper";
		t.equipment_value = 10;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 1, "gems", 1);
		t.rigidity_rating = 2;
		t.base_stats["critical_chance"] = 0.03f;
		t.base_stats["stamina"] = 5f;
		clone("amulet_bronze", "$amulet");
		t.material = "bronze";
		t.equipment_value = 15;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 1, "gems", 1);
		t.rigidity_rating = 3;
		t.base_stats["critical_chance"] = 0.04f;
		t.base_stats["stamina"] = 5f;
		clone("amulet_silver", "$amulet");
		t.material = "silver";
		t.equipment_value = 20;
		t.setCost(0, "silver", 1, "gems", 1);
		t.rigidity_rating = 2;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["mana"] = 10f;
		t.base_stats["stamina"] = 5f;
		clone("amulet_iron", "$amulet");
		t.material = "iron";
		t.equipment_value = 30;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 2, "gems", 1);
		t.rigidity_rating = 4;
		t.base_stats["critical_chance"] = 0.06f;
		t.base_stats["stamina"] = 5f;
		clone("amulet_steel", "$amulet");
		t.material = "steel";
		t.equipment_value = 40;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 3, "gems", 1);
		t.rigidity_rating = 5;
		t.base_stats["critical_chance"] = 0.07f;
		t.base_stats["stamina"] = 5f;
		clone("amulet_mythril", "$amulet");
		t.material = "mythril";
		t.equipment_value = 50;
		t.setCost(0, "mythril", 1, "gems", 1);
		t.rigidity_rating = 6;
		t.base_stats["critical_chance"] = 0.08f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 5f;
		clone("amulet_adamantine", "$amulet");
		t.material = "adamantine";
		t.equipment_value = 70;
		t.setCost(0, "adamantine", 1, "gems", 1);
		t.rigidity_rating = 7;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["mana"] = 20f;
		t.base_stats["stamina"] = 5f;
	}

	private void initHelmets()
	{
		clone("helmet_leather", "$helmet");
		t.material = "leather";
		t.equipment_value = 5;
		t.setCost(0, "leather", 1);
		t.rigidity_rating = 1;
		t.base_stats["armor"] = 2f;
		t.base_stats["stamina"] = 10f;
		t.base_stats["speed"] = 1f;
		clone("helmet_copper", "$helmet");
		t.material = "copper";
		t.equipment_value = 10;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 1);
		t.rigidity_rating = 2;
		t.base_stats["armor"] = 3f;
		t.base_stats["stamina"] = 5f;
		clone("helmet_bronze", "$helmet");
		t.material = "bronze";
		t.equipment_value = 15;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 2);
		t.rigidity_rating = 3;
		t.base_stats["armor"] = 4f;
		t.base_stats["stamina"] = 5f;
		clone("helmet_silver", "$helmet");
		t.material = "silver";
		t.equipment_value = 20;
		t.metallic = true;
		t.setCost(0, "silver", 1);
		t.rigidity_rating = 2;
		t.base_stats["armor"] = 5f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 5f;
		clone("helmet_iron", "$helmet");
		t.material = "iron";
		t.equipment_value = 30;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 3);
		t.rigidity_rating = 4;
		t.base_stats["armor"] = 6f;
		t.base_stats["stamina"] = 5f;
		clone("helmet_steel", "$helmet");
		t.material = "steel";
		t.equipment_value = 40;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 4);
		t.rigidity_rating = 5;
		t.base_stats["armor"] = 7f;
		t.base_stats["stamina"] = 5f;
		clone("helmet_mythril", "$helmet");
		t.material = "mythril";
		t.equipment_value = 50;
		t.metallic = true;
		t.setCost(0, "mythril", 1);
		t.rigidity_rating = 6;
		t.base_stats["armor"] = 8f;
		t.base_stats["mana"] = 25f;
		t.base_stats["stamina"] = 10f;
		clone("helmet_adamantine", "$helmet");
		t.material = "adamantine";
		t.equipment_value = 70;
		t.metallic = true;
		t.setCost(0, "adamantine", 1);
		t.rigidity_rating = 7;
		t.base_stats["armor"] = 10f;
		t.base_stats["mana"] = 35f;
		t.base_stats["stamina"] = 10f;
	}

	private void initArmors()
	{
		clone("armor_leather", "$armor");
		t.material = "leather";
		t.equipment_value = 5;
		t.setCost(0, "leather", 1);
		t.rigidity_rating = 1;
		t.base_stats["armor"] = 2f;
		t.base_stats["stamina"] = 20f;
		t.base_stats["speed"] = 1f;
		clone("armor_copper", "$armor");
		t.material = "copper";
		t.equipment_value = 10;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 1);
		t.rigidity_rating = 2;
		t.base_stats["armor"] = 3f;
		t.base_stats["stamina"] = 15f;
		clone("armor_bronze", "$armor");
		t.material = "bronze";
		t.equipment_value = 15;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 2);
		t.rigidity_rating = 3;
		t.base_stats["armor"] = 4f;
		t.base_stats["stamina"] = 10f;
		clone("armor_silver", "$armor");
		t.material = "silver";
		t.equipment_value = 20;
		t.metallic = true;
		t.setCost(0, "silver", 1);
		t.rigidity_rating = 2;
		t.base_stats["armor"] = 5f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 5f;
		clone("armor_iron", "$armor");
		t.material = "iron";
		t.equipment_value = 30;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 3);
		t.rigidity_rating = 4;
		t.base_stats["armor"] = 6f;
		t.base_stats["stamina"] = 5f;
		clone("armor_steel", "$armor");
		t.material = "steel";
		t.equipment_value = 40;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 4);
		t.rigidity_rating = 5;
		t.base_stats["armor"] = 7f;
		t.base_stats["stamina"] = 5f;
		clone("armor_mythril", "$armor");
		t.material = "mythril";
		t.equipment_value = 50;
		t.metallic = true;
		t.setCost(0, "mythril", 1);
		t.rigidity_rating = 6;
		t.base_stats["armor"] = 8f;
		t.base_stats["mana"] = 25f;
		t.base_stats["stamina"] = 5f;
		clone("armor_adamantine", "$armor");
		t.material = "adamantine";
		t.equipment_value = 70;
		t.metallic = true;
		t.setCost(0, "adamantine", 1);
		t.rigidity_rating = 7;
		t.base_stats["armor"] = 10f;
		t.base_stats["mana"] = 35f;
		t.base_stats["stamina"] = 5f;
	}

	private void initBoots()
	{
		clone("boots_leather", "$boots");
		t.material = "leather";
		t.equipment_value = 5;
		t.setCost(0, "leather", 1);
		t.rigidity_rating = 1;
		t.base_stats["armor"] = 2f;
		t.base_stats["stamina"] = 10f;
		t.base_stats["speed"] = 1f;
		clone("boots_copper", "$boots");
		t.material = "copper";
		t.equipment_value = 10;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 1);
		t.rigidity_rating = 2;
		t.base_stats["armor"] = 3f;
		t.base_stats["stamina"] = 5f;
		clone("boots_bronze", "$boots");
		t.material = "bronze";
		t.equipment_value = 15;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 2);
		t.rigidity_rating = 3;
		t.base_stats["armor"] = 4f;
		t.base_stats["stamina"] = 5f;
		clone("boots_silver", "$boots");
		t.material = "silver";
		t.equipment_value = 20;
		t.metallic = true;
		t.setCost(0, "silver", 1);
		t.rigidity_rating = 2;
		t.base_stats["armor"] = 5f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 5f;
		clone("boots_iron", "$boots");
		t.material = "iron";
		t.equipment_value = 30;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 3);
		t.rigidity_rating = 4;
		t.base_stats["armor"] = 6f;
		t.base_stats["stamina"] = 5f;
		clone("boots_steel", "$boots");
		t.material = "steel";
		t.equipment_value = 40;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 4);
		t.rigidity_rating = 5;
		t.base_stats["armor"] = 7f;
		t.base_stats["stamina"] = 5f;
		clone("boots_mythril", "$boots");
		t.material = "mythril";
		t.equipment_value = 50;
		t.metallic = true;
		t.setCost(0, "mythril", 1);
		t.rigidity_rating = 6;
		t.base_stats["armor"] = 8f;
		t.base_stats["mana"] = 25f;
		t.base_stats["stamina"] = 10f;
		clone("boots_adamantine", "$boots");
		t.material = "adamantine";
		t.equipment_value = 70;
		t.metallic = true;
		t.setCost(0, "adamantine", 1);
		t.rigidity_rating = 7;
		t.base_stats["armor"] = 10f;
		t.base_stats["mana"] = 35f;
		t.base_stats["stamina"] = 10f;
	}

	private void initRings()
	{
		clone("ring_bone", "$ring");
		t.material = "bone";
		t.equipment_value = 5;
		t.setCost(0, "bones", 1, "gems", 1);
		t.rigidity_rating = 1;
		t.base_stats["critical_chance"] = 0.02f;
		t.base_stats["stamina"] = 5f;
		clone("ring_copper", "$ring");
		t.material = "copper";
		t.equipment_value = 10;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 1, "gems", 1);
		t.rigidity_rating = 2;
		t.base_stats["critical_chance"] = 0.03f;
		t.base_stats["stamina"] = 5f;
		clone("ring_bronze", "$ring");
		t.material = "bronze";
		t.equipment_value = 15;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 1, "gems", 1);
		t.rigidity_rating = 3;
		t.base_stats["critical_chance"] = 0.04f;
		t.base_stats["stamina"] = 5f;
		clone("ring_silver", "$ring");
		t.material = "silver";
		t.equipment_value = 20;
		t.setCost(0, "silver", 1, "gems", 1);
		t.rigidity_rating = 2;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["mana"] = 10f;
		t.base_stats["stamina"] = 5f;
		clone("ring_iron", "$ring");
		t.material = "iron";
		t.equipment_value = 30;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 2, "gems", 1);
		t.rigidity_rating = 4;
		t.base_stats["critical_chance"] = 0.06f;
		t.base_stats["stamina"] = 5f;
		clone("ring_steel", "$ring");
		t.material = "steel";
		t.equipment_value = 40;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "common_metals", 3, "gems", 1);
		t.rigidity_rating = 5;
		t.base_stats["critical_chance"] = 0.07f;
		t.base_stats["stamina"] = 5f;
		clone("ring_mythril", "$ring");
		t.material = "mythril";
		t.equipment_value = 50;
		t.setCost(0, "mythril", 1, "gems", 1);
		t.rigidity_rating = 6;
		t.base_stats["critical_chance"] = 0.08f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 5f;
		clone("ring_adamantine", "$ring");
		t.material = "adamantine";
		t.equipment_value = 70;
		t.setCost(0, "adamantine", 1, "gems", 1);
		t.rigidity_rating = 7;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["mana"] = 20f;
		t.base_stats["stamina"] = 5f;
	}

	private void initTemplates()
	{
		EquipmentAsset obj = new EquipmentAsset
		{
			id = "$equipment",
			pool = "equipment",
			equipment_subtype = "basic"
		};
		EquipmentAsset pAsset = obj;
		t = obj;
		add(pAsset);
		initTemplatesEquipment();
		initTemplatesWeapons();
	}

	private void initTemplatesEquipment()
	{
		clone("$armor", "$equipment");
		t.equipment_type = EquipmentType.Armor;
		t.name_class = "item_class_armor";
		t.equipment_subtype = "armor";
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("armor_name");
		t.group_id = "armor";
		clone("$boots", "$equipment");
		t.equipment_type = EquipmentType.Boots;
		t.name_class = "item_class_armor";
		t.equipment_subtype = "boots";
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("boots_name");
		t.group_id = "boots";
		clone("$helmet", "$equipment");
		t.equipment_type = EquipmentType.Helmet;
		t.name_class = "item_class_armor";
		t.equipment_subtype = "helmet";
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("helmet_name");
		t.group_id = "helmet";
		clone("$accessory", "$equipment");
		t.name_class = "item_class_accessory";
		clone("$ring", "$accessory");
		t.equipment_subtype = "ring";
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("ring_name");
		t.equipment_type = EquipmentType.Ring;
		t.group_id = "ring";
		clone("$amulet", "$accessory");
		t.equipment_type = EquipmentType.Amulet;
		t.equipment_subtype = "amulet";
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("amulet_name");
		t.group_id = "amulet";
	}

	private void initTemplatesWeapons()
	{
		clone("$weapon", "$equipment");
		t.material = "basic";
		t.group_id = "sword";
		t.equipment_type = EquipmentType.Weapon;
		t.path_slash_animation = "effects/slashes/slash_base";
		t.name_class = "item_class_weapon";
		t.base_stats["damage_range"] = 0.5f;
		clone("$melee", "$weapon");
		t.pool = "melee";
		clone("$range", "$weapon");
		t.pool = "range";
		t.attack_type = WeaponType.Range;
		t.base_stats["projectiles"] = 1f;
		t.base_stats["damage_range"] = 0.6f;
		clone("$bow", "$range");
		t.equipment_subtype = "bow";
		t.is_pool_weapon = true;
		t.pool_rate = 10;
		t.projectile = "arrow";
		t.path_slash_animation = "effects/slashes/slash_bow";
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.base_stats["recoil"] = 1f;
		t.name_templates = new List<string>();
		t.name_templates.AddTimes(30, "bow_name");
		t.name_templates.Add("weapon_name_city");
		t.name_templates.Add("weapon_name_kingdom");
		t.name_templates.Add("weapon_name_culture");
		t.name_templates.Add("weapon_name_enemy_king");
		t.name_templates.Add("weapon_name_enemy_kingdom");
		t.group_id = "bow";
		clone("$sword", "$melee");
		t.equipment_subtype = "sword";
		t.is_pool_weapon = true;
		t.pool_rate = 10;
		t.path_slash_animation = "effects/slashes/slash_sword";
		t.base_stats["damage"] = 1f;
		t.base_stats["damage_range"] = 0.8f;
		t.name_templates = new List<string>();
		t.name_templates.AddTimes(30, "sword_name");
		t.name_templates.AddTimes(3, "sword_name_king");
		t.name_templates.Add("weapon_name_city");
		t.name_templates.Add("weapon_name_kingdom");
		t.name_templates.Add("weapon_name_culture");
		t.name_templates.Add("weapon_name_enemy_king");
		t.name_templates.Add("weapon_name_enemy_kingdom");
		t.group_id = "sword";
		clone("$axe", "$melee");
		t.equipment_subtype = "axe";
		t.is_pool_weapon = true;
		t.pool_rate = 10;
		t.path_slash_animation = "effects/slashes/slash_axe";
		t.base_stats["damage_range"] = 0.6f;
		t.name_templates = new List<string>();
		t.name_templates.AddTimes(30, "axe_name");
		t.name_templates.AddTimes(3, "axe_name_king");
		t.name_templates.Add("weapon_name_city");
		t.name_templates.Add("weapon_name_kingdom");
		t.name_templates.Add("weapon_name_culture");
		t.name_templates.Add("weapon_name_enemy_king");
		t.name_templates.Add("weapon_name_enemy_kingdom");
		t.group_id = "axe";
		clone("$hammer", "$melee");
		t.equipment_subtype = "hammer";
		t.is_pool_weapon = true;
		t.pool_rate = 10;
		t.path_slash_animation = "effects/slashes/slash_hammer";
		t.base_stats["targets"] = 2f;
		t.base_stats["damage_range"] = 0.1f;
		t.name_templates = new List<string>();
		t.name_templates.AddTimes(30, "hammer_name");
		t.name_templates.Add("weapon_name_city");
		t.name_templates.Add("weapon_name_kingdom");
		t.name_templates.Add("weapon_name_culture");
		t.name_templates.Add("weapon_name_enemy_king");
		t.name_templates.Add("weapon_name_enemy_kingdom");
		t.group_id = "hammer";
		t.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("stun");
		clone("$spear", "$melee");
		t.equipment_subtype = "spear";
		t.is_pool_weapon = true;
		t.pool_rate = 10;
		t.path_slash_animation = "effects/slashes/slash_spear";
		t.base_stats["range"] = 1f;
		t.base_stats["damage_range"] = 0.7f;
		t.name_templates = new List<string>();
		t.name_templates.AddTimes(30, "spear_name");
		t.name_templates.Add("weapon_name_city");
		t.name_templates.Add("weapon_name_kingdom");
		t.name_templates.Add("weapon_name_culture");
		t.name_templates.Add("weapon_name_enemy_king");
		t.name_templates.Add("weapon_name_enemy_kingdom");
		t.group_id = "spear";
	}

	private void initNormalWeapons()
	{
		initWeaponsBasic();
		initWeaponsAdvanced();
	}

	private void initWeaponsAdvanced()
	{
		initWeaponsBows();
		initWeaponsSwords();
		initWeaponsAxes();
		initWeaponsSpears();
		initWeaponsHammers();
	}

	private void initWeaponsBasic()
	{
		clone("stick_wood", "$melee");
		t.equipment_subtype = "stick";
		t.material = "wood";
		t.is_pool_weapon = true;
		t.pool_rate = 10;
		t.path_slash_animation = "effects/slashes/slash_punch";
		t.name_templates = new List<string>();
		t.name_templates.AddTimes(30, "stick_name");
		t.name_templates.Add("weapon_name_city");
		t.name_templates.Add("weapon_name_kingdom");
		t.name_templates.Add("weapon_name_culture");
		t.name_templates.Add("weapon_name_enemy_king");
		t.name_templates.Add("weapon_name_enemy_kingdom");
		t.group_id = "staff";
		t.equipment_value = 1;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "wood", 1);
		t.rigidity_rating = 1;
		t.base_stats["damage"] = 1f;
		t.base_stats["stamina"] = 5f;
		t.base_stats["mana"] = 5f;
	}

	private void initWeaponsSwords()
	{
		clone("sword_wood", "$sword");
		t.material = "wood";
		t.equipment_value = 1;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "wood", 1);
		t.rigidity_rating = 1;
		t.base_stats["damage"] = 1f;
		t.base_stats["stamina"] = 15f;
		clone("sword_stone", "$sword");
		t.material = "stone";
		t.equipment_value = 10;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "stone", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 3f;
		t.base_stats["speed"] = -2f;
		clone("sword_copper", "$sword");
		t.material = "copper";
		t.equipment_value = 10;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 3f;
		t.base_stats["stamina"] = 10f;
		clone("sword_bronze", "$sword");
		t.material = "bronze";
		t.equipment_value = 15;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 2);
		t.rigidity_rating = 3;
		t.base_stats["damage"] = 4f;
		t.base_stats["stamina"] = 5f;
		clone("sword_silver", "$sword");
		t.material = "silver";
		t.equipment_value = 20;
		t.metallic = true;
		t.setCost(0, "silver", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 5f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 5f;
		clone("sword_iron", "$sword");
		t.material = "iron";
		t.equipment_value = 30;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 3);
		t.rigidity_rating = 4;
		t.base_stats["damage"] = 6f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["stamina"] = 5f;
		clone("sword_steel", "$sword");
		t.material = "steel";
		t.equipment_value = 40;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 4);
		t.rigidity_rating = 5;
		t.base_stats["damage"] = 7f;
		t.base_stats["speed"] = 1f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["stamina"] = 5f;
		clone("sword_mythril", "$sword");
		t.material = "mythril";
		t.equipment_value = 50;
		t.metallic = true;
		t.setCost(0, "mythril", 1);
		t.rigidity_rating = 6;
		t.base_stats["damage"] = 8f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["mana"] = 25f;
		t.base_stats["stamina"] = 10f;
		clone("sword_adamantine", "$sword");
		t.material = "adamantine";
		t.equipment_value = 70;
		t.metallic = true;
		t.setCost(0, "adamantine", 1);
		t.rigidity_rating = 7;
		t.base_stats["damage"] = 10f;
		t.base_stats["speed"] = 1f;
		t.base_stats["critical_chance"] = 0.15f;
		t.base_stats["mana"] = 35f;
		t.base_stats["stamina"] = 10f;
	}

	private void initWeaponsBows()
	{
		clone("bow_wood", "$bow");
		t.material = "wood";
		t.equipment_value = 1;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "wood", 1);
		t.rigidity_rating = 1;
		t.base_stats["damage"] = 1f;
		t.base_stats["range"] = 6f;
		t.base_stats["stamina"] = 15f;
		clone("bow_copper", "$bow");
		t.material = "copper";
		t.equipment_value = 10;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 3f;
		t.base_stats["range"] = 6f;
		t.base_stats["stamina"] = 10f;
		clone("bow_bronze", "$bow");
		t.material = "bronze";
		t.equipment_value = 15;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 2);
		t.rigidity_rating = 3;
		t.base_stats["damage"] = 4f;
		t.base_stats["range"] = 7f;
		t.base_stats["stamina"] = 5f;
		clone("bow_silver", "$bow");
		t.material = "silver";
		t.equipment_value = 20;
		t.metallic = true;
		t.setCost(0, "silver", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 5f;
		t.base_stats["range"] = 8f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 5f;
		clone("bow_iron", "$bow");
		t.material = "iron";
		t.equipment_value = 30;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 3);
		t.rigidity_rating = 4;
		t.base_stats["damage"] = 6f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["range"] = 9f;
		t.base_stats["stamina"] = 5f;
		clone("bow_steel", "$bow");
		t.material = "steel";
		t.equipment_value = 40;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 4);
		t.rigidity_rating = 5;
		t.base_stats["damage"] = 7f;
		t.base_stats["speed"] = 1f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["range"] = 10f;
		t.base_stats["stamina"] = 5f;
		clone("bow_mythril", "$bow");
		t.material = "mythril";
		t.equipment_value = 50;
		t.metallic = true;
		t.setCost(0, "mythril", 1);
		t.rigidity_rating = 6;
		t.base_stats["damage"] = 8f;
		t.base_stats["range"] = 11f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["mana"] = 25f;
		t.base_stats["stamina"] = 10f;
		clone("bow_adamantine", "$bow");
		t.material = "adamantine";
		t.equipment_value = 70;
		t.metallic = true;
		t.setCost(0, "adamantine", 1);
		t.rigidity_rating = 7;
		t.base_stats["damage"] = 10f;
		t.base_stats["range"] = 12f;
		t.base_stats["speed"] = 1f;
		t.base_stats["critical_chance"] = 0.15f;
		t.base_stats["mana"] = 35f;
		t.base_stats["stamina"] = 10f;
	}

	private void initWeaponsAxes()
	{
		clone("axe_wood", "$axe");
		t.material = "wood";
		t.equipment_value = 1;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "wood", 1);
		t.rigidity_rating = 1;
		t.base_stats["damage"] = 1f;
		t.base_stats["stamina"] = 15f;
		clone("axe_stone", "$axe");
		t.material = "stone";
		t.equipment_value = 10;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "stone", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 3f;
		t.base_stats["speed"] = -2f;
		clone("axe_copper", "$axe");
		t.material = "copper";
		t.equipment_value = 10;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 3f;
		t.base_stats["stamina"] = 10f;
		clone("axe_bronze", "$axe");
		t.material = "bronze";
		t.equipment_value = 15;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 2);
		t.rigidity_rating = 3;
		t.base_stats["damage"] = 4f;
		t.base_stats["stamina"] = 5f;
		clone("axe_silver", "$axe");
		t.material = "silver";
		t.equipment_value = 20;
		t.metallic = true;
		t.setCost(0, "silver", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 5f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 5f;
		clone("axe_iron", "$axe");
		t.material = "iron";
		t.equipment_value = 30;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 3);
		t.rigidity_rating = 4;
		t.base_stats["damage"] = 6f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["stamina"] = 5f;
		clone("axe_steel", "$axe");
		t.material = "steel";
		t.equipment_value = 40;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 4);
		t.rigidity_rating = 5;
		t.base_stats["damage"] = 7f;
		t.base_stats["speed"] = 1f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["stamina"] = 5f;
		clone("axe_mythril", "$axe");
		t.material = "mythril";
		t.equipment_value = 50;
		t.metallic = true;
		t.setCost(0, "mythril", 1);
		t.rigidity_rating = 6;
		t.base_stats["damage"] = 8f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["mana"] = 25f;
		t.base_stats["stamina"] = 10f;
		clone("axe_adamantine", "$axe");
		t.material = "adamantine";
		t.equipment_value = 70;
		t.metallic = true;
		t.setCost(0, "adamantine", 1);
		t.rigidity_rating = 7;
		t.base_stats["damage"] = 10f;
		t.base_stats["speed"] = 1f;
		t.base_stats["critical_chance"] = 0.15f;
		t.base_stats["mana"] = 35f;
		t.base_stats["stamina"] = 10f;
	}

	private void initWeaponsSpears()
	{
		clone("spear_wood", "$spear");
		t.material = "wood";
		t.equipment_value = 1;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "wood", 1);
		t.rigidity_rating = 1;
		t.base_stats["damage"] = 1f;
		t.base_stats["stamina"] = 15f;
		clone("spear_stone", "$spear");
		t.material = "stone";
		t.equipment_value = 10;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "stone", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 3f;
		t.base_stats["speed"] = -2f;
		clone("spear_copper", "$spear");
		t.material = "copper";
		t.equipment_value = 10;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 3f;
		t.base_stats["stamina"] = 10f;
		clone("spear_bronze", "$spear");
		t.material = "bronze";
		t.equipment_value = 15;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 2);
		t.rigidity_rating = 3;
		t.base_stats["damage"] = 4f;
		t.base_stats["stamina"] = 5f;
		clone("spear_silver", "$spear");
		t.material = "silver";
		t.equipment_value = 20;
		t.metallic = true;
		t.setCost(0, "silver", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 5f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 5f;
		clone("spear_iron", "$spear");
		t.material = "iron";
		t.equipment_value = 30;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 3);
		t.rigidity_rating = 4;
		t.base_stats["damage"] = 6f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["stamina"] = 5f;
		clone("spear_steel", "$spear");
		t.material = "steel";
		t.equipment_value = 40;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 4);
		t.rigidity_rating = 5;
		t.base_stats["damage"] = 7f;
		t.base_stats["speed"] = 1f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["stamina"] = 5f;
		clone("spear_mythril", "$spear");
		t.material = "mythril";
		t.equipment_value = 50;
		t.metallic = true;
		t.setCost(0, "mythril", 1);
		t.rigidity_rating = 6;
		t.base_stats["damage"] = 8f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["mana"] = 25f;
		t.base_stats["stamina"] = 10f;
		clone("spear_adamantine", "$spear");
		t.material = "adamantine";
		t.equipment_value = 70;
		t.metallic = true;
		t.setCost(0, "adamantine", 1);
		t.rigidity_rating = 7;
		t.base_stats["damage"] = 10f;
		t.base_stats["speed"] = 1f;
		t.base_stats["critical_chance"] = 0.15f;
		t.base_stats["mana"] = 35f;
		t.base_stats["stamina"] = 10f;
	}

	private void initWeaponsHammers()
	{
		clone("hammer_wood", "$hammer");
		t.material = "wood";
		t.equipment_value = 1;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "wood", 1);
		t.rigidity_rating = 1;
		t.base_stats["damage"] = 1f;
		t.base_stats["stamina"] = 15f;
		clone("hammer_stone", "$hammer");
		t.material = "stone";
		t.equipment_value = 10;
		t.minimum_city_storage_resource_1 = 15;
		t.setCost(0, "stone", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 3f;
		t.base_stats["speed"] = -2f;
		clone("hammer_copper", "$hammer");
		t.material = "copper";
		t.equipment_value = 10;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 3f;
		t.base_stats["stamina"] = 10f;
		clone("hammer_bronze", "$hammer");
		t.material = "bronze";
		t.equipment_value = 15;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 2);
		t.rigidity_rating = 3;
		t.base_stats["damage"] = 4f;
		t.base_stats["stamina"] = 5f;
		clone("hammer_silver", "$hammer");
		t.material = "silver";
		t.equipment_value = 20;
		t.metallic = true;
		t.setCost(0, "silver", 1);
		t.rigidity_rating = 2;
		t.base_stats["damage"] = 5f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 5f;
		clone("hammer_iron", "$hammer");
		t.material = "iron";
		t.equipment_value = 30;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 3);
		t.rigidity_rating = 4;
		t.base_stats["damage"] = 6f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["stamina"] = 5f;
		clone("hammer_steel", "$hammer");
		t.material = "steel";
		t.equipment_value = 40;
		t.metallic = true;
		t.minimum_city_storage_resource_1 = 10;
		t.setCost(0, "common_metals", 4);
		t.rigidity_rating = 5;
		t.base_stats["damage"] = 7f;
		t.base_stats["speed"] = 1f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["stamina"] = 5f;
		clone("hammer_mythril", "$hammer");
		t.material = "mythril";
		t.equipment_value = 50;
		t.metallic = true;
		t.setCost(0, "mythril", 1);
		t.rigidity_rating = 6;
		t.base_stats["damage"] = 8f;
		t.base_stats["critical_chance"] = 0.05f;
		t.base_stats["mana"] = 25f;
		t.base_stats["stamina"] = 10f;
		clone("hammer_adamantine", "$hammer");
		t.material = "adamantine";
		t.equipment_value = 70;
		t.metallic = true;
		t.setCost(0, "adamantine", 1);
		t.rigidity_rating = 7;
		t.base_stats["damage"] = 10f;
		t.base_stats["speed"] = 1f;
		t.base_stats["critical_chance"] = 0.15f;
		t.base_stats["mana"] = 35f;
		t.base_stats["stamina"] = 10f;
	}

	private void initWeaponsUnique()
	{
		clone("alien_blaster", "$range");
		t.setUnlockedWithAchievement("achievementEquipmentExplorer");
		t.equipment_subtype = "alien_blaster";
		t.setCost(100, "adamantine", 10, "gems", 20);
		t.rigidity_rating = 7;
		t.is_pool_weapon = true;
		t.pool_rate = 1;
		t.path_icon = "ui/Icons/items/icon_alien_blaster";
		t.material = "basic";
		t.projectile = "plasma_ball";
		t.path_slash_animation = "effects/slashes/slash_punch";
		t.base_stats["range"] = 20f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["targets"] = 1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.base_stats["damage_range"] = 0.6f;
		t.base_stats["mana"] = 20f;
		t.base_stats["stamina"] = 20f;
		t.equipment_value = 500;
		t.base_stats["damage"] = 30f;
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("blaster_name");
		t.group_id = "firearm";
		clone("shotgun", "$range");
		t.setUnlockedWithAchievement("achievementTLDR");
		t.equipment_subtype = "shotgun";
		t.setCost(100, "adamantine", 10, "mythril", 5);
		t.rigidity_rating = 6;
		t.is_pool_weapon = true;
		t.pool_rate = 1;
		t.path_icon = "ui/Icons/items/icon_shotgun";
		t.projectile = "shotgun_bullet";
		t.path_slash_animation = "effects/slashes/slash_punch";
		t.base_stats["projectiles"] = 12f;
		t.base_stats["range"] = 10f;
		t.base_stats["targets"] = 1f;
		t.base_stats["damage"] = 10f;
		t.base_stats["damage_range"] = 0.9f;
		t.base_stats["mana"] = 5f;
		t.base_stats["stamina"] = 10f;
		t.equipment_value = 600;
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("shotgun_name");
		t.group_id = "firearm";
		clone("flame_hammer", "$weapon");
		t.setUnlockedWithAchievement("achievementGodlySmithing");
		t.equipment_subtype = "flame_hammer";
		t.setCost(10, "dragon_scales", 3);
		t.is_pool_weapon = true;
		t.animated = true;
		t.pool_rate = 2;
		t.material = "basic";
		t.path_slash_animation = "effects/slashes/slash_hammer";
		t.rigidity_rating = 6;
		t.base_stats["damage"] = 20f;
		t.base_stats["speed"] = 1f;
		t.base_stats["critical_chance"] = 0.15f;
		t.base_stats["targets"] = 3f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 15f;
		t.equipment_value = 400;
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("flame_hammer_name");
		t.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");
		t.group_id = "hammer";
		clone("ice_hammer", "$weapon");
		t.setUnlockedWithAchievement("achievementMakeWilhelmScream");
		t.equipment_subtype = "ice_hammer";
		t.setCost(10, "mythril", 10, "gems", 2);
		t.rigidity_rating = 6;
		t.is_pool_weapon = true;
		t.animated = true;
		t.pool_rate = 2;
		t.material = "basic";
		t.path_slash_animation = "effects/slashes/slash_hammer";
		t.base_stats["damage"] = 20f;
		t.base_stats["speed"] = 1f;
		t.base_stats["critical_chance"] = 0.15f;
		t.base_stats["targets"] = 3f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 15f;
		t.equipment_value = 400;
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("ice_hammer_name");
		t.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("ice", "stun");
		t.group_id = "hammer";
		clone("flame_sword", "$weapon");
		t.equipment_subtype = "flame_sword";
		t.setCost(0, "dragon_scales", 2);
		t.is_pool_weapon = true;
		t.animated = true;
		t.pool_rate = 2;
		t.material = "basic";
		t.path_slash_animation = "effects/slashes/slash_sword";
		t.rigidity_rating = 6;
		t.base_stats["damage"] = 33f;
		t.base_stats["targets"] = 2f;
		t.base_stats["critical_damage_multiplier"] = 0.1f;
		t.base_stats["mana"] = 15f;
		t.base_stats["stamina"] = 15f;
		t.equipment_value = 300;
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("flame_sword_name");
		t.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");
		t.group_id = "sword";
		t.base_stats.addTag("building_immunity_fire");
		clone("necromancer_staff", "$range");
		t.equipment_subtype = "necromancer_staff";
		t.setCost(10, "mythril", 2, "gems", 3);
		t.is_pool_weapon = true;
		t.pool_rate = 1;
		t.material = "basic";
		t.projectile = "skull";
		t.path_slash_animation = "effects/slashes/slash_punch";
		t.rigidity_rating = 5;
		t.base_stats["range"] = 13f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["targets"] = 1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.base_stats["mana"] = 40f;
		t.equipment_value = 500;
		t.base_stats["damage"] = 30f;
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("necromancer_staff_name");
		t.group_id = "staff";
		t.addSpell("spawn_skeleton");
		t.addSpell("cast_curse");
		clone("evil_staff", "$range");
		t.equipment_subtype = "evil_staff";
		t.setCost(20, "mythril", 3, "gems", 2);
		t.is_pool_weapon = true;
		t.durability = 300;
		t.pool_rate = 1;
		t.material = "basic";
		t.projectile = "red_orb";
		t.path_slash_animation = "effects/slashes/slash_punch";
		t.rigidity_rating = 5;
		t.base_stats["range"] = 13f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["targets"] = 1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.base_stats["mana"] = 40f;
		t.equipment_value = 500;
		t.base_stats["projectiles"] = 20f;
		t.base_stats["damage"] = 10f;
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("evil_staff_name");
		t.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");
		t.group_id = "staff";
		t.base_stats.addTag("building_immunity_fire");
		t.addSpell("cast_fire");
		clone("white_staff", "$range");
		t.equipment_subtype = "white_staff";
		t.setCost(20, "mythril", 3, "gems", 2);
		t.is_pool_weapon = true;
		t.durability = 300;
		t.pool_rate = 3;
		t.material = "basic";
		t.projectile = "freeze_orb";
		t.path_slash_animation = "effects/slashes/slash_punch";
		t.rigidity_rating = 5;
		t.base_stats["range"] = 18f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["targets"] = 1f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.base_stats["damage"] = 35f;
		t.base_stats["mana"] = 40f;
		t.equipment_value = 500;
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("white_staff_name");
		t.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("ice");
		t.group_id = "staff";
		t.addSpell("cast_blood_rain");
		t.addSpell("summon_lightning");
		clone("plague_doctor_staff", "$weapon");
		t.equipment_subtype = "plague_doctor_staff";
		t.setCost(5, "mythril", 2, "gems", 1);
		t.is_pool_weapon = true;
		t.pool_rate = 3;
		t.material = "basic";
		t.path_slash_animation = "effects/slashes/slash_punch";
		t.rigidity_rating = 5;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["targets"] = 3f;
		t.base_stats["critical_damage_multiplier"] = 0.5f;
		t.base_stats["damage"] = 35f;
		t.base_stats["mana"] = 40f;
		t.equipment_value = 200;
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("plague_doctor_staff_name");
		t.group_id = "staff";
		t.addSpell("cast_fire");
		t.addSpell("cast_cure");
		clone("druid_staff", "$range");
		t.equipment_subtype = "druid_staff";
		t.setCost(7, "mythril", 3, "gems", 1);
		t.is_pool_weapon = true;
		t.pool_rate = 3;
		t.material = "basic";
		t.projectile = "green_orb";
		t.path_slash_animation = "effects/slashes/slash_punch";
		t.rigidity_rating = 5;
		t.base_stats["range"] = 20f;
		t.base_stats["critical_chance"] = 0.1f;
		t.base_stats["targets"] = 1f;
		t.base_stats["critical_damage_multiplier"] = 0.3f;
		t.base_stats["damage"] = 12f;
		t.base_stats["mana"] = 40f;
		t.equipment_value = 300;
		t.base_stats["projectiles"] = 2f;
		t.name_templates = AssetLibrary<EquipmentAsset>.l<string>("druid_staff_name");
		t.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("slowness");
		t.group_id = "staff";
		t.addSpell("cast_blood_rain");
		t.addSpell("spawn_vegetation");
	}
}
