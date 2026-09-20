using System.Collections.Generic;
using UnityEngine;

public class Item : CoreSystemObject<ItemData>
{
	private EquipmentAsset _asset;

	private string _texture_id;

	public bool unit_has_it;

	public bool city_has_it;

	public bool shouldbe_removed;

	private Actor _actor;

	private City _city;

	private int _item_value;

	private readonly BaseStats _total_stats = new BaseStats();

	private Rarity _quality;

	public AttackAction action_attack_target;

	protected override MetaType meta_type => MetaType.Item;

	public override BaseSystemManager manager => World.world.items;

	public EquipmentAsset asset => getAsset();

	public override string name
	{
		get
		{
			return getName(pWithMaterial: false);
		}
		protected set
		{
			data.name = value;
		}
	}

	public override void setFavorite(bool pState)
	{
		base.setFavorite(pState);
		if (isReadyForRemoval())
		{
			World.world.items.setDirty();
		}
	}

	public bool isCursed()
	{
		return hasMod("cursed");
	}

	public bool isEternal()
	{
		return hasMod("eternal");
	}

	public bool isDestroyable()
	{
		if (!isFavorite())
		{
			return !isEternal();
		}
		return false;
	}

	public bool isReadyForRemoval()
	{
		if (!hasCity() && !hasActor())
		{
			return isDestroyable();
		}
		return false;
	}

	public void fullRepair()
	{
		data.durability = 100;
	}

	public void getDamaged(int pDamage)
	{
		if (!hasMod("eternal"))
		{
			data.durability -= pDamage;
			if (data.durability < 0)
			{
				data.durability = 0;
			}
		}
	}

	public void newItem(EquipmentAsset pAsset)
	{
		setAsset(pAsset);
		data.durability = pAsset.durability;
	}

	public bool isBroken()
	{
		return getDurabilityCurrent() <= 0;
	}

	public bool needRepair()
	{
		return getDurabilityCurrent() < getDurabilityMax();
	}

	public int getDurabilityCurrent()
	{
		return data.durability;
	}

	public int getDurabilityMax()
	{
		return _asset.durability;
	}

	public string getDurabilityString()
	{
		return getDurabilityCurrent() + "/" + getDurabilityMax();
	}

	private void setAsset(EquipmentAsset pAsset)
	{
		_asset = pAsset;
		data.asset_id = _asset.id;
		recalculateTexturePath();
	}

	private void recalculateTexturePath()
	{
		_texture_id = asset.path_gameplay_sprite;
	}

	protected sealed override void setDefaultValues()
	{
		base.setDefaultValues();
		_asset = null;
		unit_has_it = false;
		city_has_it = false;
		shouldbe_removed = false;
		_item_value = 0;
		_quality = Rarity.R0_Normal;
	}

	public void reforge(int pTries)
	{
		clearForReforge();
		World.world.items.generateModsFor(this, pTries);
		string pName = NameGenerator.getName("reforged_item");
		setName(pName);
		calculateValues();
		if (_actor != null)
		{
			_actor.addTrait("scar_of_divinity");
		}
	}

	private void clearForReforge()
	{
		data.modifiers.Clear();
	}

	public void transmute()
	{
		clearForReforge();
		bool flag = isCheatEnabled();
		using ListPool<EquipmentAsset> listPool = new ListPool<EquipmentAsset>();
		List<EquipmentAsset> list = (_asset.is_pool_weapon ? ((!flag) ? AssetManager.items.pot_weapon_assets_unlocked : AssetManager.items.pot_weapon_assets_all) : ((!flag) ? AssetManager.items.pot_equipment_by_groups_unlocked[_asset.group_id] : AssetManager.items.pot_equipment_by_groups_all[_asset.group_id]));
		foreach (EquipmentAsset item in list)
		{
			if (item.isAvailable())
			{
				listPool.Add(item);
			}
		}
		listPool.Shuffle();
		foreach (ref EquipmentAsset item2 in listPool)
		{
			EquipmentAsset current2 = item2;
			if (_asset != current2)
			{
				setAsset(current2);
				break;
			}
		}
		reforge(5);
		calculateValues();
	}

	public bool hasMod(string pID)
	{
		if (data.modifiers.Contains(pID))
		{
			return true;
		}
		return false;
	}

	public bool hasMod(ItemModAsset pModAsset)
	{
		return hasMod(pModAsset.id);
	}

	public bool addMod(string pModId)
	{
		ItemModAsset pModAsset = AssetManager.items_modifiers.get(pModId);
		return addMod(pModAsset);
	}

	public bool addMod(ItemModAsset pModAsset)
	{
		if (hasMod(pModAsset))
		{
			return false;
		}
		foreach (ref string modifier in data.modifiers)
		{
			string current = modifier;
			if (AssetManager.items_modifiers.get(current).mod_type == pModAsset.mod_type)
			{
				return false;
			}
		}
		data.modifiers.Add(pModAsset.id);
		World.world.items.setDirty();
		return true;
	}

	public bool removeMod(string pModId)
	{
		World.world.items.setDirty();
		return data.modifiers.Remove(pModId);
	}

	public void setUnitHasIt(Actor pActor)
	{
		_actor = pActor;
		_city = null;
		unit_has_it = true;
		city_has_it = false;
	}

	public void setInCityStorage(City pCity)
	{
		_actor = null;
		_city = pCity;
		unit_has_it = false;
		city_has_it = true;
	}

	public void setShouldBeRemoved()
	{
		shouldbe_removed = true;
	}

	public void clearUnit()
	{
		_actor = null;
		unit_has_it = false;
		World.world.items.setDirty();
	}

	public void clearCity()
	{
		_city = null;
		city_has_it = false;
		World.world.items.setDirty();
	}

	public Actor getActor()
	{
		return _actor;
	}

	public City getCity()
	{
		return _city;
	}

	public void calculateValues()
	{
		recalculateTexturePath();
		ItemTools.calcItemValues(this, _total_stats);
		_item_value = ItemTools.s_value;
		_quality = ItemTools.s_quality;
		data.durability = _asset.durability;
	}

	public void initItem()
	{
		initFields();
		calculateValues();
	}

	public override void loadData(ItemData pData)
	{
		base.loadData(pData);
		initFields();
		calculateValues();
	}

	private void initFields()
	{
		EquipmentAsset equipmentAsset = AssetManager.items.get(data.asset_id);
		setAsset(equipmentAsset);
	}

	public EquipmentAsset getAsset()
	{
		return _asset;
	}

	public string getItemDescription()
	{
		string pKey = ((!data.created_by_player) ? "item_template_description_full" : "item_template_description_full_player");
		string text = LocalizedTextManager.getText(pKey);
		string text2 = LocalizedTextManager.getText("item_template_description_age_only");
		if (string.IsNullOrEmpty(data.by))
		{
			text = text2;
		}
		int age = getAge();
		text = text.Replace("$item_creator_name$", Toolbox.coloredString(data.by, data.byColor));
		text = text.Replace("$item_creator_kingdom$", Toolbox.coloredString(data.from, data.fromColor));
		text = text.Replace("$item_creator_years$", age.ToString() ?? "");
		if (age == 1)
		{
			return text.Replace("$year_ending$", LocalizedTextManager.getText("item_template_description_year") ?? "");
		}
		return text.Replace("$year_ending$", LocalizedTextManager.getText("item_template_description_years") ?? "");
	}

	public string getItemKeyType()
	{
		return _asset.getLocaleRarity(getQuality());
	}

	public void countKill()
	{
		data.kills++;
	}

	public bool isRangeAttack()
	{
		return getAsset().attack_type == WeaponType.Range;
	}

	public BaseStats getFullStats()
	{
		return _total_stats;
	}

	public int getValue()
	{
		return _item_value;
	}

	public Rarity getQuality()
	{
		return _quality;
	}

	public string getTextureID()
	{
		return _texture_id;
	}

	public string getName(bool pWithMaterial = true)
	{
		if (!string.IsNullOrEmpty(data.name))
		{
			return data.name;
		}
		ItemTools.getTooltipTitle(getAsset(), out var pName, out var pMaterial);
		return pMaterial + pName;
	}

	public string getQualityColor()
	{
		return getQuality().getRarityColorHex();
	}

	public static string getQualityString(Rarity pRarity)
	{
		string result = "";
		switch (pRarity)
		{
		case Rarity.R0_Normal:
			result = "";
			break;
		case Rarity.R1_Rare:
			result = "_rare";
			break;
		case Rarity.R2_Epic:
			result = "_epic";
			break;
		case Rarity.R3_Legendary:
			result = "_legendary";
			break;
		}
		return result;
	}

	public Sprite getSprite()
	{
		return _asset.getSprite();
	}

	public bool hasActor()
	{
		return getActor() != null;
	}

	public bool hasCity()
	{
		return getCity() != null;
	}

	public override void Dispose()
	{
		_asset = null;
		_texture_id = null;
		_total_stats.reset();
		_actor = null;
		action_attack_target = null;
		base.Dispose();
	}

	private bool isCheatEnabled()
	{
		if (!DebugConfig.isOn(DebugOption.UnlockAllEquipment))
		{
			return WorldLawLibrary.world_law_cursed_world.isEnabled();
		}
		return true;
	}
}
