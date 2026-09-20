using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class ItemAsset : BaseAugmentationAsset, IDescriptionAsset, ILocalizedAsset, IMultiLocalesAsset
{
	public string path_gameplay_sprite;

	[NonSerialized]
	public Sprite[] gameplay_sprites;

	public string material;

	public bool colored;

	public int minimum_city_storage_resource_1;

	public int cost_gold;

	public int cost_coins_resources;

	public int cost_resource_1;

	[DefaultValue("none")]
	public string cost_resource_id_1 = "none";

	public int cost_resource_2;

	[DefaultValue("none")]
	public string cost_resource_id_2 = "none";

	[DefaultValue(1)]
	public int rarity = 1;

	public int equipment_value;

	public bool metallic;

	[DefaultValue(1)]
	public int mod_rank = 1;

	public string mod_type;

	[DefaultValue(true)]
	public bool mod_can_be_given = true;

	public string translation_key;

	public bool animated;

	public string pool;

	[DefaultValue("")]
	public string path_slash_animation = string.Empty;

	public string projectile;

	[DefaultValue(Rarity.R0_Normal)]
	public Rarity quality;

	[DefaultValue(WeaponType.Melee)]
	public WeaponType attack_type;

	public EquipmentType equipment_type;

	public string equipment_subtype;

	public string name_class;

	public List<string> name_templates;

	public string[] item_modifier_ids;

	[NonSerialized]
	public ItemModAsset[] item_modifiers;

	public bool is_pool_weapon;

	public int pool_rate;

	[DefaultValue(100)]
	public int durability = 100;

	[DefaultValue(1)]
	public int rigidity_rating = 1;

	protected override HashSet<string> progress_elements => base._progress_data?.unlocked_equipment;

	[JsonIgnore]
	public int get_total_cost => cost_gold + cost_coins_resources;

	public override BaseCategoryAsset getGroup()
	{
		return AssetManager.item_groups.get(group_id);
	}

	public string getLocaleRarity(Rarity pRarity)
	{
		return name_class + Item.getQualityString(pRarity);
	}

	public override string getLocaleID()
	{
		if (!has_locales)
		{
			return null;
		}
		return translation_key ?? ("item_" + (equipment_subtype ?? id));
	}

	public string getDescriptionID()
	{
		if (!has_locales || isMod())
		{
			return null;
		}
		return id + "_description";
	}

	public string getMaterialID()
	{
		if (!has_locales || isMod())
		{
			return null;
		}
		return "item_mat_" + material;
	}

	public IEnumerable<string> getLocaleIDs()
	{
		if (!has_locales)
		{
			yield break;
		}
		yield return getLocaleID();
		if (isMod())
		{
			yield break;
		}
		yield return getDescriptionID();
		if (material != "basic")
		{
			yield return getMaterialID();
		}
		foreach (Rarity value in Enum.GetValues(typeof(Rarity)))
		{
			yield return getLocaleRarity(value);
		}
	}

	public string getTranslatedName()
	{
		string text = LocalizedTextManager.getText(getLocaleID());
		if (!string.IsNullOrEmpty(material) && material != "basic")
		{
			string text2 = LocalizedTextManager.getText(getMaterialID());
			text = text + " (" + text2 + ")";
		}
		return text;
	}

	public string getTranslatedDescription()
	{
		return LocalizedTextManager.getText(getDescriptionID());
	}

	public void setCost(int pGoldCost, string pResourceID_1 = "none", int pCostResource_1 = 0, string pResourceID_2 = "none", int pCostResource_2 = 0)
	{
		cost_gold = pGoldCost;
		cost_resource_id_1 = pResourceID_1;
		cost_resource_1 = pCostResource_1;
		cost_resource_id_2 = pResourceID_2;
		cost_resource_2 = pCostResource_2;
	}

	public bool isMod()
	{
		return mod_type != null;
	}

	public string getRandomNameTemplate(Actor pActor = null)
	{
		foreach (string item in name_templates.LoopRandom())
		{
			NameGeneratorAsset nameGeneratorAsset = AssetManager.name_generator.get(item);
			if (nameGeneratorAsset.check == null || nameGeneratorAsset.check(pActor))
			{
				return item;
			}
		}
		return null;
	}

	public override bool unlock(bool pSaveData = true)
	{
		if (!base.unlock(pSaveData))
		{
			return false;
		}
		EquipmentAsset item = this as EquipmentAsset;
		List<EquipmentAsset> pot_weapon_assets_unlocked = AssetManager.items.pot_weapon_assets_unlocked;
		if (is_pool_weapon && !pot_weapon_assets_unlocked.Contains(item))
		{
			pot_weapon_assets_unlocked.Add(item);
		}
		Dictionary<string, List<EquipmentAsset>> pot_equipment_by_groups_unlocked = AssetManager.items.pot_equipment_by_groups_unlocked;
		if (!is_pool_weapon)
		{
			if (!pot_equipment_by_groups_unlocked.ContainsKey(group_id))
			{
				pot_equipment_by_groups_unlocked.Add(group_id, new List<EquipmentAsset>());
			}
			List<EquipmentAsset> list = pot_equipment_by_groups_unlocked[group_id];
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return true;
	}

	protected override bool isDebugUnlockedAll()
	{
		return DebugConfig.isOn(DebugOption.UnlockAllEquipment);
	}
}
