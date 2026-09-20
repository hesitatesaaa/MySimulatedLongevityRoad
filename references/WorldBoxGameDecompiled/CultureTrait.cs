using System;
using System.Collections.Generic;

[Serializable]
public class CultureTrait : BaseTrait<CultureTrait>
{
	public bool is_weapon_trait;

	public List<string> related_weapon_subtype_ids;

	public List<string> related_weapons_ids;

	public bool town_layout_plan;

	public PassableZoneChecker passable_zone_checker;

	protected override HashSet<string> progress_elements => base._progress_data?.unlocked_traits_culture;

	public override string typed_id => "culture_trait";

	protected override IEnumerable<ITraitsOwner<CultureTrait>> getRelatedMetaList()
	{
		return World.world.cultures;
	}

	public override BaseCategoryAsset getGroup()
	{
		return AssetManager.culture_trait_groups.get(group_id);
	}

	public void addWeaponSpecial(string pID)
	{
		if (related_weapons_ids == null)
		{
			related_weapons_ids = new List<string>();
		}
		related_weapons_ids.Add(pID);
		is_weapon_trait = true;
	}

	public void addWeaponSubtype(string pSubtype)
	{
		if (related_weapon_subtype_ids == null)
		{
			related_weapon_subtype_ids = new List<string>();
		}
		related_weapon_subtype_ids.Add(pSubtype);
		is_weapon_trait = true;
	}

	public void setTownLayoutPlan(PassableZoneChecker pZoneCheckerDelegate)
	{
		town_layout_plan = true;
		passable_zone_checker = pZoneCheckerDelegate;
	}
}
