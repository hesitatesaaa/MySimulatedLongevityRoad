using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class DecisionAsset : Asset, ILocalizedAsset
{
	public DecisionAction action_check_launch;

	public float weight = 1f;

	[NonSerialized]
	public bool has_weight_custom;

	public DecisionActionWeight weight_calculate_custom;

	public string task_id;

	public int decision_index;

	public int cooldown;

	public string path_icon;

	public NeuroLayer priority;

	[NonSerialized]
	public int priority_int_cached;

	public bool cooldown_on_launch_failure;

	public bool only_special;

	public bool unique;

	public bool list_civ;

	public bool list_baby;

	public bool list_animal;

	public bool only_adult;

	public bool only_mob;

	public bool only_herd;

	public bool only_sapient;

	public bool only_safe;

	public bool only_hungry;

	public bool city_must_be_safe;

	[NonSerialized]
	public Sprite cached_sprite;

	public virtual Sprite getSprite()
	{
		if (cached_sprite == null)
		{
			cached_sprite = SpriteTextureLoader.getSprite(path_icon);
		}
		return cached_sprite;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isPossible(Actor pActor)
	{
		if (only_hungry && !pActor.isHungry())
		{
			return false;
		}
		if (only_safe && pActor.isFighting())
		{
			return false;
		}
		if (only_herd && !pActor.asset.follow_herd)
		{
			return false;
		}
		if (only_adult && !pActor.isAdult())
		{
			return false;
		}
		if (only_mob && pActor.isKingdomCiv())
		{
			return false;
		}
		if (only_sapient && !pActor.isSapient())
		{
			return false;
		}
		if (city_must_be_safe && pActor.inOwnCityBorders())
		{
			ProfessionAsset profession_asset = pActor.profession_asset;
			if (profession_asset != null && profession_asset.can_capture)
			{
				City city = pActor.current_zone.city;
				if (city != null && city.isInDanger())
				{
					return false;
				}
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isPossible(ref DecisionChecks pChecks)
	{
		if (only_hungry && !pChecks.is_hungry)
		{
			return false;
		}
		if (only_safe && pChecks.is_fighting)
		{
			return false;
		}
		if (only_herd && !pChecks.is_herd)
		{
			return false;
		}
		if (only_adult && !pChecks.is_adult)
		{
			return false;
		}
		if (only_mob && pChecks.is_civ)
		{
			return false;
		}
		if (only_sapient && !pChecks.is_sapient)
		{
			return false;
		}
		if (city_must_be_safe && pChecks.can_capture_city && pChecks.city_is_in_danger)
		{
			return false;
		}
		return true;
	}

	public string getLocalizedText()
	{
		return getLocaleID().Localize();
	}

	public string getLocaleID()
	{
		string pID = ((!string.IsNullOrEmpty(task_id)) ? task_id : id);
		return AssetManager.tasks_actor.get(pID).getLocaleID();
	}

	public string getFiringRate()
	{
		if (cooldown <= 0)
		{
			return "N/A";
		}
		float num = 1f / (float)cooldown;
		if (num < 0.1f)
		{
			return $"{num:0.000} Hz";
		}
		return $"{num:0.00} Hz";
	}
}
