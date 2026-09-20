using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class SubspeciesTrait : BaseTrait<SubspeciesTrait>, IAnimationFrames
{
	public bool in_mutation_pot_add;

	public bool in_mutation_pot_remove;

	public bool phenotype_skin;

	public bool phenotype_egg;

	public bool is_diet_related;

	[DefaultValue("")]
	public string id_phenotype = string.Empty;

	[DefaultValue("")]
	public string id_egg = string.Empty;

	public AfterHatchFromEggAction after_hatch_from_egg_action;

	public bool has_after_hatch_from_egg_action;

	public bool remove_for_zombies;

	public bool is_mutation_skin;

	public string sprite_path;

	public ActorTextureSubAsset texture_asset;

	public bool prevent_unconscious_rotation;

	public bool render_heads_for_children;

	public List<string> skin_citizen_male;

	public List<string> skin_citizen_female;

	public List<string> skin_warrior;

	public string[] animation_walk = ActorAnimationSequences.walk_0;

	[DefaultValue(10f)]
	public float animation_walk_speed = 10f;

	public string[] animation_swim = ActorAnimationSequences.swim_0;

	[DefaultValue(8f)]
	public float animation_swim_speed = 8f;

	public string[] animation_idle = ActorAnimationSequences.walk_0;

	[DefaultValue(10f)]
	public float animation_idle_speed = 10f;

	[DefaultValue(true)]
	public bool shadow = true;

	[DefaultValue("unitShadow_5")]
	public string shadow_texture = "unitShadow_5";

	[DefaultValue("unitShadow_2")]
	public string shadow_texture_egg = "unitShadow_2";

	[DefaultValue("unitShadow_4")]
	public string shadow_texture_baby = "unitShadow_4";

	protected override HashSet<string> progress_elements => base._progress_data?.unlocked_traits_subspecies;

	public override string typed_id => "subspecies_trait";

	public override BaseCategoryAsset getGroup()
	{
		return AssetManager.subspecies_trait_groups.get(group_id);
	}

	public override Sprite getSprite()
	{
		if (cached_sprite == null)
		{
			Sprite sprite = SpriteTextureLoader.getSprite(path_icon);
			if (special_icon_logic)
			{
				if (phenotype_skin)
				{
					PhenotypeAsset phenotypeAsset = getPhenotypeAsset();
					cached_sprite = DynamicSprites.getIconWithColors(sprite, phenotypeAsset, null);
				}
			}
			else
			{
				cached_sprite = sprite;
			}
		}
		return cached_sprite;
	}

	public PhenotypeAsset getPhenotypeAsset()
	{
		if (phenotype_skin)
		{
			return AssetManager.phenotype_library.get(id_phenotype);
		}
		return null;
	}

	protected override IEnumerable<ITraitsOwner<SubspeciesTrait>> getRelatedMetaList()
	{
		return World.world.subspecies;
	}

	public override string getCountRows()
	{
		return getCountRowsByCategories();
	}

	protected override bool isSapient(ITraitsOwner<SubspeciesTrait> pObject)
	{
		return ((Subspecies)pObject).isSapient();
	}

	public string[] getWalk()
	{
		return animation_walk;
	}

	public string[] getIdle()
	{
		return animation_idle;
	}

	public string[] getSwim()
	{
		return animation_swim;
	}
}
