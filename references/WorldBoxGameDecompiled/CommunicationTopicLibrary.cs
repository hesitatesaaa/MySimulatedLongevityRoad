using System.Collections.Generic;
using UnityEngine;

public class CommunicationTopicLibrary : AssetLibrary<CommunicationAsset>
{
	private List<Sprite> _cached_sprites_religion = new List<Sprite>();

	private List<Sprite> _cached_sprites_culture = new List<Sprite>();

	private List<Sprite> _cached_sprites_family = new List<Sprite>();

	private List<Sprite> _cached_sprites_kingdom = new List<Sprite>();

	private List<Sprite> _cached_sprites_city = new List<Sprite>();

	private List<Sprite> _cached_sprites_clan = new List<Sprite>();

	private List<Sprite> _cached_sprites_time_and_death = new List<Sprite>();

	private List<Sprite> _cached_sprites_general_topics = new List<Sprite>();

	private List<Sprite> _cached_sprites_boats_water = new List<Sprite>();

	private List<Sprite> _cached_sprites_housed = new List<Sprite>();

	private List<Sprite> _cached_sprites_homeless = new List<Sprite>();

	private const int MAX_TOPIC_SPRITES = 10;

	public override void init()
	{
		add(new CommunicationAsset
		{
			id = "emotions",
			rate = 0.9f,
			check = (Actor pActor) => pActor.hasEmotions(),
			pot_fill = delegate(Actor pActor, ListPool<Sprite> pPotSprites)
			{
				Sprite spriteBasedOnHappinessValue = HappinessHelper.getSpriteBasedOnHappinessValue(pActor.getHappiness());
				for (int i = 0; i < 3; i++)
				{
					pPotSprites.Add(spriteBasedOnHappinessValue);
				}
				if (!pActor.hasHappinessHistory())
				{
					return;
				}
				foreach (HappinessHistory item in pActor.happiness_change_history)
				{
					Sprite sprite = item.asset.getSprite();
					pPotSprites.Add(sprite);
				}
			}
		});
		add(new CommunicationAsset
		{
			id = "is_housed",
			rate = 0.2f,
			check = (Actor pActor) => pActor.hasCity() && pActor.hasHouse(),
			pot_fill = delegate(Actor _, ListPool<Sprite> pPotSprites)
			{
				pPotSprites.AddRange(_cached_sprites_housed);
			}
		});
		add(new CommunicationAsset
		{
			id = "is_homeless",
			rate = 0.4f,
			check = (Actor pActor) => pActor.hasCity() && !pActor.hasHouse(),
			pot_fill = delegate(Actor _, ListPool<Sprite> pPotSprites)
			{
				pPotSprites.AddRange(_cached_sprites_homeless);
			}
		});
		add(new CommunicationAsset
		{
			id = "favorite_food",
			rate = 0.4f,
			check = (Actor pActor) => pActor.hasFavoriteFood(),
			pot_fill = delegate(Actor pActor, ListPool<Sprite> pPotSprites)
			{
				Sprite spriteIcon = pActor.favorite_food_asset.getSpriteIcon();
				if ((Object)(object)spriteIcon != (Object)null)
				{
					pPotSprites.Add(spriteIcon);
				}
			}
		});
		add(new CommunicationAsset
		{
			id = "religion",
			rate = 0.2f,
			check = (Actor pActor) => pActor.hasReligion(),
			pot_fill = delegate(Actor pActor, ListPool<Sprite> pPotSprites)
			{
				Sprite topicSprite = pActor.religion.getTopicSprite();
				if ((Object)(object)topicSprite != (Object)null)
				{
					pPotSprites.Add(topicSprite);
				}
				pPotSprites.AddRange(_cached_sprites_religion);
			}
		});
		add(new CommunicationAsset
		{
			id = "culture",
			rate = 0.15f,
			check = (Actor pActor) => pActor.hasCulture(),
			pot_fill = delegate(Actor pActor, ListPool<Sprite> pPotSprites)
			{
				Sprite topicSprite = pActor.culture.getTopicSprite();
				if ((Object)(object)topicSprite != (Object)null)
				{
					pPotSprites.Add(topicSprite);
				}
				pPotSprites.AddRange(_cached_sprites_culture);
			}
		});
		add(new CommunicationAsset
		{
			id = "equipment",
			rate = 0.2f,
			check = (Actor pActor) => pActor.hasEquipment(),
			pot_fill = delegate(Actor pActor, ListPool<Sprite> pPotSprites)
			{
				foreach (ActorEquipmentSlot item2 in pActor.equipment)
				{
					Sprite sprite = item2.getItem().getAsset().getSprite();
					if ((Object)(object)sprite != (Object)null)
					{
						pPotSprites.Add(sprite);
					}
				}
			}
		});
		add(new CommunicationAsset
		{
			id = "language",
			rate = 0.15f,
			check = (Actor pActor) => pActor.hasLanguage(),
			pot_fill = delegate(Actor pActor, ListPool<Sprite> pPotSprites)
			{
				Sprite topicSprite = pActor.language.getTopicSprite();
				if ((Object)(object)topicSprite != (Object)null)
				{
					pPotSprites.Add(topicSprite);
				}
			}
		});
		add(new CommunicationAsset
		{
			id = "actor_traits",
			rate = 0.3f,
			check = (Actor pActor) => pActor.hasTraits(),
			pot_fill = delegate(Actor pActor, ListPool<Sprite> pPotSprites)
			{
				Sprite topicSpriteTrait = pActor.getTopicSpriteTrait();
				if ((Object)(object)topicSpriteTrait != (Object)null)
				{
					pPotSprites.Add(topicSpriteTrait);
				}
			}
		});
		add(new CommunicationAsset
		{
			id = "family",
			rate = 0.3f,
			check = (Actor pActor) => pActor.hasFamily(),
			pot_fill = delegate(Actor _, ListPool<Sprite> pPotSprites)
			{
				pPotSprites.AddRange(_cached_sprites_family);
			}
		});
		add(new CommunicationAsset
		{
			id = "kingdom_civ",
			rate = 0.2f,
			check = (Actor pActor) => pActor.isKingdomCiv(),
			pot_fill = delegate(Actor pActor, ListPool<Sprite> pPotSprites)
			{
				Sprite topicSprite = pActor.kingdom.getTopicSprite();
				if ((Object)(object)topicSprite != (Object)null)
				{
					pPotSprites.Add(topicSprite);
				}
				pPotSprites.AddRange(_cached_sprites_kingdom);
			}
		});
		add(new CommunicationAsset
		{
			id = "statuses",
			rate = 0.7f,
			check = (Actor pActor) => pActor.hasAnyStatusEffect(),
			pot_fill = delegate(Actor pActor, ListPool<Sprite> pPotSprites)
			{
				foreach (Status status in pActor.getStatuses())
				{
					pPotSprites.Add(status.asset.getSprite());
				}
			}
		});
		add(new CommunicationAsset
		{
			id = "city",
			rate = 0.3f,
			check = (Actor pActor) => pActor.hasCity(),
			pot_fill = delegate(Actor pActor, ListPool<Sprite> pPotSprites)
			{
				pPotSprites.AddRange(_cached_sprites_city);
				if (pActor.city.hasStorages())
				{
					ResourceAsset randomFoodAsset = pActor.city.storages.GetRandom().resources.getRandomFoodAsset();
					if (randomFoodAsset != null)
					{
						Sprite spriteIcon = randomFoodAsset.getSpriteIcon();
						if ((Object)(object)spriteIcon != (Object)null)
						{
							pPotSprites.Add(spriteIcon);
						}
					}
				}
			}
		});
		add(new CommunicationAsset
		{
			id = "city_boats",
			rate = 0.1f,
			check = (Actor pActor) => pActor.hasCity() && pActor.city.countBoats() > 0,
			pot_fill = delegate(Actor _, ListPool<Sprite> pPotSprites)
			{
				pPotSprites.AddRange(_cached_sprites_boats_water);
			}
		});
		add(new CommunicationAsset
		{
			id = "clan",
			rate = 0.3f,
			check = (Actor pActor) => pActor.hasClan(),
			pot_fill = delegate(Actor pActor, ListPool<Sprite> pPotSprites)
			{
				Sprite topicSprite = pActor.clan.getTopicSprite();
				if ((Object)(object)topicSprite != (Object)null)
				{
					pPotSprites.Add(topicSprite);
				}
				pPotSprites.AddRange(_cached_sprites_clan);
			}
		});
		add(new CommunicationAsset
		{
			id = "time_and_death",
			rate = 0.3f,
			check = (Actor _) => true,
			pot_fill = delegate(Actor _, ListPool<Sprite> pPotSprites)
			{
				pPotSprites.AddRange(_cached_sprites_time_and_death);
			}
		});
		add(new CommunicationAsset
		{
			id = "world_subspecies",
			rate = 0.1f,
			check = (Actor _) => World.world.subspecies.hasAny(),
			pot_fill = delegate(Actor _, ListPool<Sprite> pPotSprites)
			{
				pPotSprites.AddRange(_cached_sprites_general_topics);
			}
		});
		add(new CommunicationAsset
		{
			id = "general_topics",
			rate = 1f,
			check = (Actor _) => true,
			pot_fill = delegate(Actor _, ListPool<Sprite> pPotSprites)
			{
				pPotSprites.AddRange(_cached_sprites_general_topics);
			}
		});
	}

	public override void linkAssets()
	{
		cacheSpritesGeneralTopics();
		base.linkAssets();
	}

	public Sprite getTopicSprite(Actor pActor)
	{
		using ListPool<Sprite> listPool = new ListPool<Sprite>();
		list.Shuffle();
		foreach (CommunicationAsset item in list)
		{
			if (Randy.randomChance(item.rate) && item.check(pActor))
			{
				item.pot_fill(pActor, listPool);
				if (listPool.Count > 10)
				{
					break;
				}
			}
		}
		return listPool.GetRandom();
	}

	private void cacheSpritesGeneralTopics()
	{
		_cached_sprites_housed.Add(SpriteTextureLoader.getSprite("ui/Icons/iconHoused"));
		_cached_sprites_homeless.Add(SpriteTextureLoader.getSprite("ui/Icons/iconHomeless"));
		_cached_sprites_religion.Add(SpriteTextureLoader.getSprite("ui/Icons/iconReligion"));
		_cached_sprites_religion.Add(SpriteTextureLoader.getSprite("ui/Icons/iconReligionList"));
		_cached_sprites_culture.Add(SpriteTextureLoader.getSprite("ui/Icons/iconCulture"));
		_cached_sprites_culture.Add(SpriteTextureLoader.getSprite("ui/Icons/iconCultureList"));
		_cached_sprites_family.Add(SpriteTextureLoader.getSprite("ui/Icons/iconFamily"));
		_cached_sprites_family.Add(SpriteTextureLoader.getSprite("ui/Icons/iconFamilyList"));
		_cached_sprites_family.Add(SpriteTextureLoader.getSprite("ui/Icons/iconChildren"));
		_cached_sprites_kingdom.Add(SpriteTextureLoader.getSprite("ui/Icons/iconKingdom"));
		_cached_sprites_kingdom.Add(SpriteTextureLoader.getSprite("ui/Icons/iconKingdomList"));
		_cached_sprites_kingdom.Add(SpriteTextureLoader.getSprite("ui/Icons/iconRebellion"));
		_cached_sprites_kingdom.Add(SpriteTextureLoader.getSprite("ui/Icons/iconKings"));
		_cached_sprites_city.Add(SpriteTextureLoader.getSprite("ui/Icons/iconCity"));
		_cached_sprites_city.Add(SpriteTextureLoader.getSprite("ui/Icons/iconCityList"));
		_cached_sprites_city.Add(SpriteTextureLoader.getSprite("ui/Icons/iconLeaders"));
		_cached_sprites_clan.Add(SpriteTextureLoader.getSprite("ui/Icons/iconClan"));
		_cached_sprites_clan.Add(SpriteTextureLoader.getSprite("ui/Icons/iconClanList"));
		_cached_sprites_time_and_death.Add(SpriteTextureLoader.getSprite("ui/Icons/iconClock"));
		_cached_sprites_time_and_death.Add(SpriteTextureLoader.getSprite("ui/Icons/iconDead"));
		_cached_sprites_time_and_death.Add(SpriteTextureLoader.getSprite("ui/Icons/iconSkulls"));
		_cached_sprites_time_and_death.Add(SpriteTextureLoader.getSprite("ui/Icons/iconKills"));
		_cached_sprites_time_and_death.Add(SpriteTextureLoader.getSprite("ui/Icons/iconAge"));
		_cached_sprites_time_and_death.Add(SpriteTextureLoader.getSprite("ui/Icons/iconRenown"));
		_cached_sprites_general_topics.Add(SpriteTextureLoader.getSprite("ui/Icons/iconGodFinger"));
		_cached_sprites_general_topics.Add(SpriteTextureLoader.getSprite("ui/Icons/iconBre"));
		_cached_sprites_boats_water.Add(SpriteTextureLoader.getSprite("ui/Icons/iconBoat"));
		_cached_sprites_boats_water.Add(SpriteTextureLoader.getSprite("ui/Icons/iconTileDeepOcean"));
		_cached_sprites_boats_water.Add(SpriteTextureLoader.getSprite("ui/Icons/iconTileCloseOcean"));
	}
}
