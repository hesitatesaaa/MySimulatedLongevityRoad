using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine.Scripting;

[Serializable]
[Preserve]
public class ActorData : BaseObjectData
{
	public List<long> saved_items;

	[DefaultValue(null)]
	public ActorBag inventory;

	public int x;

	public int y;

	[DefaultValue(-1L)]
	public long transportID = -1L;

	[DefaultValue(-1L)]
	public long homeBuildingID = -1L;

	[DefaultValue(UnitProfession.Nothing)]
	public UnitProfession profession;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public List<string> saved_traits;

	[DefaultValue(-1L)]
	public long cityID { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long civ_kingdom_id { get; set; } = -1L;

	[Preserve]
	[Obsolete("use .id instead", true)]
	public long actorID
	{
		set
		{
			if (value.hasValue() && !base.id.hasValue())
			{
				base.id = value;
			}
		}
	}

	[Preserve]
	[Obsolete("use .name instead", true)]
	public string firstName
	{
		set
		{
			if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(base.name))
			{
				base.name = value;
			}
		}
	}

	[Preserve]
	[Obsolete("use .asset_id instead", true)]
	public string statsID
	{
		set
		{
			if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(asset_id))
			{
				asset_id = value;
			}
		}
	}

	[DefaultValue("")]
	public string favorite_food { get; set; } = "";

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	[DefaultValue(ActorSex.Male)]
	public ActorSex sex { get; set; }

	[DefaultValue(-1)]
	public int head { get; set; } = -1;

	[DefaultValue(-1L)]
	public long culture { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long clan { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long subspecies { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long language { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long plot { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long religion { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long family { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long army { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long lover { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long best_friend_id { get; set; } = -1L;

	[DefaultValue(0)]
	public int renown { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string asset_id { get; set; }

	[DefaultValue(0)]
	public int kills { get; set; }

	[DefaultValue(0)]
	public int food_consumed { get; set; }

	[DefaultValue(1)]
	public int age_overgrowth { get; set; } = 1;

	[DefaultValue(0)]
	public int births { get; set; }

	[DefaultValue(-1L)]
	public long parent_id_1 { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long parent_id_2 { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long ancestor_family { get; set; } = -1L;

	[DefaultValue(1)]
	public int generation { get; set; } = 1;

	[DefaultValue(0)]
	public int pollen { get; set; }

	[DefaultValue(0)]
	public int loot { get; set; }

	[DefaultValue(0)]
	public int money { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	[DefaultValue(0)]
	public int nutrition { get; set; }

	[DefaultValue(0)]
	public int happiness { get; set; }

	[DefaultValue(0)]
	public int stamina { get; set; }

	[DefaultValue(0)]
	public int mana { get; set; }

	[DefaultValue(1)]
	public int level { get; set; } = 1;

	[DefaultValue(0)]
	public int experience { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	[DefaultValue(0)]
	public int phenotype_shade { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	[DefaultValue(0)]
	public int phenotype_index { get; set; }

	[Preserve]
	[Obsolete("use .nutrition instead", true)]
	public int hunger
	{
		set
		{
			if (nutrition == 0 && value != 0)
			{
				nutrition = value;
			}
		}
	}

	[Preserve]
	[Obsolete("use .saved_traits instead", true)]
	public List<string> traits
	{
		set
		{
			if (value != null && value.Count != 0)
			{
				List<string> list = saved_traits;
				if (list == null || list.Count <= 0)
				{
					saved_traits = value;
				}
			}
		}
	}

	[Preserve]
	[Obsolete("use .sex instead", true)]
	public ActorGender gender
	{
		set
		{
			if (sex == ActorSex.Male)
			{
				switch (value)
				{
				case ActorGender.Male:
					sex = ActorSex.Male;
					break;
				case ActorGender.Female:
					sex = ActorSex.Female;
					break;
				default:
					sex = ActorSex.Male;
					break;
				}
			}
		}
	}

	public int getAge()
	{
		return Date.getYearsSince(base.created_time) + age_overgrowth;
	}
}
