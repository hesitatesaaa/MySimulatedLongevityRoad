using UnityEngine;

public static class NameGeneratorReplacers
{
	public static void replaceKingdom(ref string pName, Kingdom pKingdom)
	{
		if (pName.Contains("$kingdom$"))
		{
			if (pKingdom == null)
			{
				pName = "";
			}
			else
			{
				pName = pName.Replace("$kingdom$", pKingdom.name);
			}
		}
	}

	public static void replaceEnemyKing(ref string pName, Actor pActor)
	{
		using ListPool<Kingdom> list = pActor.kingdom.getEnemiesKingdoms();
		foreach (Kingdom item in list.LoopRandom())
		{
			if (item.hasKing() && Toolbox.isFirstLatin(item.king.getName()))
			{
				pName = pName.Replace("$king$", "King " + item.king.getName());
				return;
			}
		}
		pName = "";
	}

	public static void replaceOwnKingdom(ref string pName, Actor pActor)
	{
		if (pName.Contains("$kingdom$"))
		{
			if (!pActor.hasKingdom())
			{
				pName = "";
				return;
			}
			Kingdom kingdom = pActor.kingdom;
			pName = pName.Replace("$kingdom$", kingdom.name);
		}
	}

	public static void replaceEnemyKingdom(ref string pName, Actor pActor)
	{
		if (!pName.Contains("$kingdom$"))
		{
			return;
		}
		using ListPool<Kingdom> list = pActor.kingdom.getEnemiesKingdoms();
		foreach (Kingdom item in list.LoopRandom())
		{
			if (Toolbox.isFirstLatin(item.name))
			{
				pName = pName.Replace("$kingdom$", item.name);
				return;
			}
		}
		pName = "";
	}

	public static void replaceFavoriteFood(ref string pName, Actor pActor)
	{
		if (pName.Contains("$food$"))
		{
			Kingdom kingdom = pActor.kingdom;
			string newValue;
			if (kingdom != null && kingdom.king?.hasFavoriteFood() == true)
			{
				newValue = pActor.kingdom.king.favorite_food_asset.getTranslatedName();
			}
			else
			{
				City city = pActor.city;
				newValue = ((city != null && city.leader?.hasFavoriteFood() == true) ? pActor.city.leader.favorite_food_asset.getTranslatedName() : ((!pActor.hasFavoriteFood()) ? AssetManager.resources.list.GetRandom().getTranslatedName() : pActor.favorite_food_asset.getTranslatedName()));
			}
			pName = pName.Replace("$food$", newValue);
		}
	}

	public static void replaceOwnName(ref string pName, Actor pActor)
	{
		if (pName.Contains("$unit$"))
		{
			pName = pName.Replace("$unit$", pActor.getName());
		}
	}

	public static void replaceOwnCity(ref string pName, Actor pActor)
	{
		if (pName.Contains("$city$"))
		{
			if (!pActor.hasCity())
			{
				pName = "";
				return;
			}
			City city = pActor.city;
			pName = pName.Replace("$city$", city.name);
		}
	}

	public static void replaceOwnSubspecies(ref string pName, Actor pActor)
	{
		if (pName.Contains("$subspecies$"))
		{
			if (!pActor.hasSubspecies())
			{
				pName = "";
				return;
			}
			Subspecies subspecies = pActor.subspecies;
			pName = pName.Replace("$subspecies$", subspecies.name);
		}
	}

	public static void replaceOwnAlliance(ref string pName, Actor pActor)
	{
		if (!pName.Contains("$alliance$"))
		{
			return;
		}
		if (!pActor.hasKingdom())
		{
			pName = "";
			return;
		}
		Kingdom kingdom = pActor.kingdom;
		if (!kingdom.hasAlliance())
		{
			pName = "";
			return;
		}
		Alliance alliance = kingdom.getAlliance();
		pName = pName.Replace("$alliance$", alliance.name);
	}

	public static void replaceOwnKingClan(ref string pName, Actor pActor)
	{
		if (!pName.Contains("$clan$"))
		{
			return;
		}
		Kingdom kingdom = pActor.kingdom;
		if (!kingdom.hasKing())
		{
			pName = "";
			return;
		}
		Actor king = kingdom.king;
		if (!king.hasClan())
		{
			pName = "";
		}
		else
		{
			pName = pName.Replace("$clan$", king.clan.name);
		}
	}

	public static void replaceOwnLeader(ref string pName, Actor pActor)
	{
		if (!pName.Contains("$leader$"))
		{
			return;
		}
		if (!pActor.hasCity())
		{
			pName = "";
			return;
		}
		City city = pActor.city;
		if (!city.hasLeader())
		{
			pName = "";
			return;
		}
		Actor leader = city.leader;
		pName = pName.Replace("$leader$", leader.getName());
	}

	public static void replaceFigure(ref string pName, Actor pActor)
	{
		replaceOwnLeader(ref pName, pActor);
		replaceOwnKing(ref pName, pActor);
		replaceOwnKingClan(ref pName, pActor);
	}

	public static void replaceAnyCity(ref string pName, Actor pActor)
	{
		if (pName.Contains("$city_random$"))
		{
			if (!World.world.cities.hasAny())
			{
				pName = "";
				return;
			}
			City random = World.world.cities.getRandom();
			pName = pName.Replace("$city_random$", random.name);
		}
	}

	public static void replaceAnyKingdom(ref string pName, Actor _)
	{
		if (pName.Contains("$kingdom_random$"))
		{
			if (!World.world.kingdoms.hasAny())
			{
				pName = "";
				return;
			}
			Kingdom random = World.world.kingdoms.getRandom();
			pName = pName.Replace("$kingdom_random$", random.name);
		}
	}

	public static void replaceAnyCulture(ref string pName, Actor _)
	{
		if (pName.Contains("$culture_random$"))
		{
			if (!World.world.cultures.hasAny())
			{
				pName = "";
				return;
			}
			Culture random = World.world.cultures.getRandom();
			pName = pName.Replace("$culture_random$", random.name);
		}
	}

	public static void replaceAnyFamily(ref string pName, Actor pActor)
	{
		if (!pName.Contains("$family_random$"))
		{
			return;
		}
		if (!World.world.families.hasAny())
		{
			pName = "";
			return;
		}
		int count = World.world.families.Count;
		do
		{
			Family random = World.world.families.getRandom();
			if (random.isSameSpecies(pActor.asset.id))
			{
				Family family = random;
				pName = pName.Replace("$family_random$", family.name);
				return;
			}
		}
		while (count-- > 0);
		pName = "";
	}

	public static void replaceAnySubspecies(ref string pName, Actor pActor)
	{
		if (pName.Contains("$random_subspecies$"))
		{
			if (!World.world.subspecies.hasAny())
			{
				pName = "";
				return;
			}
			Subspecies random = World.world.subspecies.getRandom();
			pName = pName.Replace("$random_subspecies$", random.name);
		}
	}

	public static void replaceAnyClan(ref string pName, Actor pActor)
	{
		if (pName.Contains("$clan_random$"))
		{
			if (!World.world.clans.hasAny())
			{
				pName = "";
				return;
			}
			Clan random = World.world.clans.getRandom();
			pName = pName.Replace("$clan_random$", random.name);
		}
	}

	public static void replaceAnyKing(ref string pName, Actor pActor)
	{
		if (!pName.Contains("$king_random$"))
		{
			return;
		}
		if (!World.world.kingdoms.hasAny())
		{
			pName = "";
			return;
		}
		int num = 0;
		Kingdom kingdom = null;
		while (kingdom == null || !kingdom.hasKing())
		{
			if (num++ > 10)
			{
				pName = "";
				return;
			}
			kingdom = World.world.kingdoms.getRandom();
		}
		Actor king = kingdom.king;
		pName = pName.Replace("$king_random$", king.getName());
	}

	public static void replaceAnyLeader(ref string pName, Actor pActor)
	{
		if (!pName.Contains("$leader_random$"))
		{
			return;
		}
		if (!World.world.cities.hasAny())
		{
			pName = "";
			return;
		}
		int num = 0;
		City city = null;
		while (city == null || !city.hasLeader())
		{
			if (num++ > 10)
			{
				pName = "";
				return;
			}
			city = World.world.cities.getRandom();
		}
		Actor leader = city.leader;
		pName = pName.Replace("$leader_random$", leader.getName());
	}

	public static void replaceOwnKing(ref string pName, Actor pActor)
	{
		if (!pName.Contains("$king$"))
		{
			return;
		}
		if (!pActor.hasKingdom())
		{
			pName = "";
			return;
		}
		Kingdom kingdom = pActor.kingdom;
		if (!kingdom.hasKing())
		{
			pName = "";
			return;
		}
		Actor king = kingdom.king;
		pName = pName.Replace("$king$", king.getName());
	}

	public static void replaceOwnKingLover(ref string pName, Actor pActor)
	{
		if (!pName.Contains("$king_lover$"))
		{
			return;
		}
		if (!pActor.hasKingdom())
		{
			pName = "";
			return;
		}
		Kingdom kingdom = pActor.kingdom;
		if (!kingdom.hasKing())
		{
			pName = "";
			return;
		}
		Actor king = kingdom.king;
		if (!king.hasLover())
		{
			pName = "";
			return;
		}
		Actor lover = king.lover;
		pName = pName.Replace("$king$", king.getName());
		pName = pName.Replace("$king_lover$", lover.getName());
	}

	public static void replaceOwnCulture(ref string pName, Actor pActor)
	{
		if (pName.Contains("$culture$"))
		{
			if (!pActor.hasCulture())
			{
				pName = "";
				return;
			}
			Culture culture = pActor.culture;
			pName = pName.Replace("$culture$", culture.name);
		}
	}

	public static void replaceOwnLanguage(ref string pName, Actor pActor)
	{
		if (pName.Contains("$language$"))
		{
			if (!pActor.hasLanguage())
			{
				pName = "";
				return;
			}
			Language language = pActor.language;
			pName = pName.Replace("$language$", language.name);
		}
	}

	public static void replaceOwnReligion(ref string pName, Actor pActor)
	{
		if (pName.Contains("$religion$"))
		{
			if (!pActor.hasReligion())
			{
				pName = "";
				return;
			}
			Religion religion = pActor.religion;
			pName = pName.Replace("$religion$", religion.name);
		}
	}

	public static void replaceOwnFamily(ref string pName, Actor pActor)
	{
		if (pName.Contains("$family$"))
		{
			if (!pActor.hasFamily())
			{
				pName = "";
				return;
			}
			Family family = pActor.family;
			pName = pName.Replace("$family$", family.name);
		}
	}

	public static void replaceAnyFamilyFounders(ref string pName, Actor pActor)
	{
		if (!pName.Contains("$family_founder_1$") && !pName.Contains("$family_founder_2$"))
		{
			return;
		}
		if (!World.world.families.hasAny())
		{
			pName = "";
			return;
		}
		int count = World.world.families.list.Count;
		do
		{
			Family random = World.world.families.getRandom();
			if (random.isSameSpecies(pActor.asset.id) && random.hasFounders())
			{
				Family family = random;
				replaceFamilyFounder1(ref pName, family.units[0]);
				replaceFamilyFounder2(ref pName, family.units[0]);
				return;
			}
		}
		while (count-- > 0);
		pName = "";
	}

	public static void replaceOwnFamilyFounders(ref string pName, Actor pActor)
	{
		if (pName.Contains("$family_founder_1$") || pName.Contains("$family_founder_2$"))
		{
			if (!pActor.hasFamily())
			{
				pName = "";
				return;
			}
			replaceFamilyFounder1(ref pName, pActor);
			replaceFamilyFounder2(ref pName, pActor);
		}
	}

	public static void replaceFamilyFounder1(ref string pName, Actor pActor)
	{
		if (!pName.Contains("$family_founder_1$"))
		{
			return;
		}
		if (!pActor.hasFamily())
		{
			pName = "";
			return;
		}
		string founder_actor_name_ = pActor.family.data.founder_actor_name_1;
		if (string.IsNullOrEmpty(founder_actor_name_))
		{
			pName = "";
		}
		else
		{
			pName = pName.Replace("$family_founder_1$", founder_actor_name_);
		}
	}

	public static void replaceFamilyFounder2(ref string pName, Actor pActor)
	{
		if (!pName.Contains("$family_founder_2$"))
		{
			return;
		}
		if (!pActor.hasFamily())
		{
			pName = "";
			return;
		}
		string founder_actor_name_ = pActor.family.data.founder_actor_name_2;
		if (string.IsNullOrEmpty(founder_actor_name_))
		{
			pName = "";
		}
		else
		{
			pName = pName.Replace("$family_founder_2$", founder_actor_name_);
		}
	}

	public static void replaceWorldName(ref string pName, Actor pActor)
	{
		if (pName.Contains("$world_name$"))
		{
			pName = pName.Replace("$world_name$", World.world.map_stats.name);
		}
	}

	public static void replaceArchitectName(ref string pName, Actor pActor)
	{
		if (pName.Contains("$architect_name$"))
		{
			pName = pName.Replace("$architect_name$", World.world.map_stats.player_name);
		}
	}

	public static void replacer_debug(ref string pName)
	{
		pName = pName.Replace("$alliance$", "Pact of Gregs");
		pName = pName.Replace("$food$", "Tea");
		pName = pName.Replace("$family$", "Gregovich");
		pName = pName.Replace("$family_random$", "Urg Zurg");
		pName = pName.Replace("$family_founder_1$", "Greg");
		pName = pName.Replace("$family_founder_2$", "Gregia");
		pName = pName.Replace("$king$", "Gregor");
		pName = pName.Replace("$king_lover$", "Gregoria");
		pName = pName.Replace("$king_random$", "Zurg Gurg");
		pName = pName.Replace("$kingdom$", "Kingdom of Greg");
		pName = pName.Replace("$kingdom_random$", "Brothers of Wargh");
		pName = pName.Replace("$clan$", "Greg Clan");
		pName = pName.Replace("$clan_random$", "Deze Zaz");
		pName = pName.Replace("$leader$", "Gregoryl");
		pName = pName.Replace("$leader_random$", "Orcaryl");
		pName = pName.Replace("$culture$", "Gragian Culture");
		pName = pName.Replace("$culture_random$", "Orkian Kult");
		pName = pName.Replace("$city$", "Gregopolis");
		pName = pName.Replace("$city_random$", "Orcville");
		pName = pName.Replace("$unit$", "Greg the Great");
		pName = pName.Replace("$warrior$", "Greg the Warrior");
		pName = pName.Replace("$language$", "Gregian Language");
		pName = pName.Replace("$religion$", "Gregianity");
		pName = pName.Replace("$subspecies$", "Gregian Sapient");
		pName = pName.Replace("$random_subspecies$", "Weird Dudes");
		pName = pName.Replace("$world_name$", "The Bad Place");
		pName = pName.Replace("$architect_name$", "Your Mom");
		pName = pName.Replace("$item$", "Legendary Greg Axe");
		if (pName.Contains('$'))
		{
			Debug.LogWarning((object)("replacer_debug missing variable " + pName));
		}
	}
}
