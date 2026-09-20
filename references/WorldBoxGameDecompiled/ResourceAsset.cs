using System;
using UnityEngine;

[Serializable]
public class ResourceAsset : Asset, ILocalizedAsset, IHandRenderer
{
	public string path_icon;

	public string path_gameplay_sprite = "bag_default";

	[NonSerialized]
	public Sprite[] gameplay_sprites;

	public int drop_max = 30;

	public int drop_per_mass = 50;

	private Sprite _cached_sprite_icon;

	private Sprite _cached_gameplay_sprite;

	public ResType type = ResType.Ingredient;

	public int mine_rate;

	public int maximum = 999;

	public bool wood;

	public bool mineral;

	public ResourceEatAction eat_action;

	public int restore_nutrition;

	public int restore_happiness;

	public int restore_mana;

	public int restore_stamina;

	public int produce_min = 10;

	public int stack_size = 15;

	public int loot_value = 1;

	public int money_cost = 2;

	public float restore_health;

	public int give_experience;

	public int ingredients_amount = 1;

	public string[] ingredients;

	public string[] diet;

	public bool food;

	public int supply_bound_give = 30;

	public int supply_bound_take = 10;

	public float favorite_food_chance = 0.5f;

	public float tastiness = 1f;

	public int supply_give = 10;

	public int trade_bound = 40;

	public int trade_give = 10;

	public int trade_cost = 1;

	public int storage_max = 50;

	public float give_chance;

	public string[] give_trait_id;

	[NonSerialized]
	public ActorTrait[] give_trait;

	public string[] give_status_id;

	[NonSerialized]
	public StatusAsset[] give_status;

	public ResourceEatAction give_action;

	[NonSerialized]
	public int order = -1;

	public string tooltip = "city_resource";

	public string full_sprite_path;

	public bool is_colored => false;

	public bool is_animated => false;

	public Sprite getSpriteIcon()
	{
		if ((Object)(object)_cached_sprite_icon == (Object)null)
		{
			_cached_sprite_icon = SpriteTextureLoader.getSprite("ui/Icons/" + path_icon);
		}
		return _cached_sprite_icon;
	}

	public Sprite getGameplaySprite()
	{
		if ((Object)(object)_cached_gameplay_sprite == (Object)null)
		{
			_cached_gameplay_sprite = gameplay_sprites[0];
		}
		return _cached_gameplay_sprite;
	}

	public string getLocaleID()
	{
		return id;
	}

	public string getTranslatedName()
	{
		return getLocaleID().Localize();
	}

	public Sprite[] getSprites()
	{
		return gameplay_sprites;
	}
}
