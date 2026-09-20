public static class GenerateLLMPrompt
{
	public static string getText(Actor pActor)
	{
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		stringBuilderPool.AppendLine("World Name: " + World.world.map_stats.name);
		stringBuilderPool.AppendLine($"World Year: {Date.getCurrentYear()}");
		stringBuilderPool.AppendLine("World Age: " + World.world.era_manager.getCurrentAge().id);
		stringBuilderPool.AppendLine($"Deaths: {World.world.map_stats.deaths}, Population: {World.world.map_stats.population}, Mobs: {World.world.map_stats.current_mobs}");
		stringBuilderPool.AppendLine("God Architector Name: " + World.world.map_stats.player_name);
		stringBuilderPool.AppendLine();
		stringBuilderPool.AppendLine($"World has subspecies: {World.world.subspecies.Count}, families: {World.world.families.Count}, languages: {World.world.languages.Count}, religions: {World.world.religions.Count}, items: {World.world.items.Count}, buildings: {World.world.buildings.Count}, cultures: {World.world.cultures.Count}, kingdoms: {World.world.kingdoms.Count}, cities: {World.world.cities.Count}, clans: {World.world.clans.Count}, units: {World.world.units.Count}");
		stringBuilderPool.AppendLine($"World has islands: {World.world.islands_calculator.countLandIslands()}");
		stringBuilderPool.AppendLine("Unit Name: " + pActor.name);
		stringBuilderPool.AppendLine($"Age: {pActor.getAge()}, Species: {pActor.asset.id}, Sex: {pActor.data.sex}, Level: {pActor.data.level}");
		stringBuilderPool.AppendLine($"Births: {pActor.data.births}, Kills: {pActor.data.kills}, Generation: {pActor.data.generation}");
		stringBuilderPool.AppendLine("Actor Traits: " + pActor.getTraitsAsLocalizedString());
		if (pActor.hasSubspecies())
		{
			stringBuilderPool.AppendLine($"Subspecies: {pActor.subspecies.name}, Age: {pActor.subspecies.getAge()}");
			stringBuilderPool.AppendLine("Subspecies traits: " + pActor.subspecies.getTraitsAsLocalizedString());
		}
		if (pActor.hasKingdom() && pActor.isKingdomCiv())
		{
			stringBuilderPool.AppendLine("Kingdom: " + pActor.kingdom.name);
			stringBuilderPool.AppendLine($"Kingdom Age: {pActor.kingdom.getAge()}, Population: {pActor.kingdom.getPopulationPeople()}, Children: {pActor.kingdom.countChildren()}, Warriors: {pActor.kingdom.countTotalWarriors()}");
			if (pActor.kingdom.hasKing())
			{
				stringBuilderPool.AppendLine($"King: {pActor.kingdom.king.name}, Age: {pActor.kingdom.king.getAge()}");
				stringBuilderPool.AppendLine($"Births: {pActor.kingdom.king.data.births}, Kills: {pActor.kingdom.king.data.kills}, Level: {pActor.kingdom.king.data.level}");
			}
		}
		int num = 0;
		foreach (Actor parent in pActor.getParents())
		{
			stringBuilderPool.AppendLine($"Parent {++num}: {parent.name}, Age: {parent.getAge()}");
		}
		if (pActor.hasCity())
		{
			stringBuilderPool.AppendLine("City: " + pActor.city.name);
			stringBuilderPool.AppendLine($"City Age: {pActor.city.getAge()}, Population: {pActor.city.getPopulationPeople()}, Children: {pActor.city.countPopulationChildren()}, Warriors: {pActor.city.countWarriors()}");
		}
		if (pActor.hasClan())
		{
			stringBuilderPool.AppendLine($"Bloodline Clan is: {pActor.clan.name}, Members: {pActor.clan.countUnits()}, Age: {pActor.clan.getAge()} years");
			stringBuilderPool.AppendLine("Clan traits: " + pActor.clan.getTraitsAsLocalizedString());
		}
		if (pActor.hasFamily())
		{
			stringBuilderPool.AppendLine($"Family: {pActor.family.name}, Members: {pActor.family.countUnits()}, Age: {pActor.family.getAge()} years");
		}
		if (pActor.hasCulture())
		{
			stringBuilderPool.AppendLine($"Culture: {pActor.culture.name}, Followers: {pActor.culture.countUnits()}, Age: {pActor.culture.getAge()} years");
			stringBuilderPool.AppendLine("Culture traits: " + pActor.culture.getTraitsAsLocalizedString());
		}
		if (pActor.hasLanguage())
		{
			stringBuilderPool.AppendLine($"Language: {pActor.language.name}, Users: {pActor.language.countUnits()}, Age: {pActor.language.getAge()} years");
			stringBuilderPool.AppendLine("Language traits: " + pActor.language.getTraitsAsLocalizedString());
		}
		if (pActor.hasReligion())
		{
			stringBuilderPool.AppendLine($"Religion: {pActor.religion.name}, Followers: {pActor.religion.countUnits()}, age {pActor.religion.getAge()} years");
			stringBuilderPool.AppendLine("Religion traits: " + pActor.religion.getTraitsAsLocalizedString());
		}
		if (pActor.hasLover())
		{
			stringBuilderPool.AppendLine($"Lover: {pActor.lover.name}, {pActor.data.sex}, level: {pActor.data.level}, Age: {pActor.lover.getAge()}, money: {pActor.lover.data.money}, kills: {pActor.lover.data.kills}");
		}
		if (pActor.hasBestFriend())
		{
			stringBuilderPool.AppendLine($"Best Friend: {pActor.getBestFriend().name}. Age of friendship: {pActor.getBestFriend().getAge()}");
			if (pActor.getBestFriend().hasLover())
			{
				stringBuilderPool.AppendLine($"Best Friend's Lover: {pActor.getBestFriend().lover.name}. Of age {pActor.getBestFriend().lover.getAge()}");
			}
		}
		if (pActor.hasWeapon())
		{
			stringBuilderPool.AppendLine($"Weapon: {pActor.getWeapon().getName()}. Rarity: {pActor.getWeapon().getQuality()}. Age is {pActor.getWeapon().getAge()} years");
		}
		stringBuilderPool.AppendLine($"Happiness: {pActor.getHappiness()}/{pActor.getMaxHappiness()}");
		stringBuilderPool.AppendLine($"Health: {pActor.data.health}/{pActor.getMaxHealth()}");
		stringBuilderPool.AppendLine($"Stamina: {pActor.data.stamina}/{pActor.getMaxStamina()}");
		stringBuilderPool.AppendLine($"Nutrition: {pActor.data.nutrition}/{pActor.getMaxNutrition()}");
		stringBuilderPool.AppendLine($"Mana: {pActor.data.mana}/{pActor.getMaxMana()}");
		stringBuilderPool.AppendLine($"Money: {pActor.data.money}");
		stringBuilderPool.AppendLine();
		stringBuilderPool.AppendLine("He lives in a fantasy simulated world. Write a story about his life, his thoughts, and his adventures.");
		stringBuilderPool.AppendLine("Make it as though it were from a classic fantasy tale like Lord of the Rings or a D&D campaign.");
		stringBuilderPool.AppendLine("Make it epic, dramatic, and full of lore. Infuse it with light and darkness, fun and sadness.");
		stringBuilderPool.AppendLine("Give it character and heart, and make it unforgettable.");
		stringBuilderPool.AppendLine("Reply should be in this language: " + LocalizedTextManager.instance.language);
		return stringBuilderPool.ToString();
	}
}
