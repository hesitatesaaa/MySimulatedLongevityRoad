using System.Collections.Generic;

public class LanguageTraitLibrary : BaseTraitLibrary<LanguageTrait>
{
	protected override string icon_path => "ui/Icons/language_traits/";

	protected override List<string> getDefaultTraitsForMeta(ActorAsset pAsset)
	{
		return pAsset.default_language_traits;
	}

	public override void init()
	{
		base.init();
		add(new LanguageTrait
		{
			id = "melodic",
			value = 3f,
			group_id = "harmony"
		});
		add(new LanguageTrait
		{
			id = "stylish_writing",
			value = 3f,
			group_id = "harmony"
		});
		add(new LanguageTrait
		{
			id = "scribble",
			group_id = "knowledge"
		});
		t.addOpposite("nicely_structured_grammar");
		add(new LanguageTrait
		{
			id = "nicely_structured_grammar",
			value = 2f,
			group_id = "knowledge"
		});
		t.addOpposite("scribble");
		add(new LanguageTrait
		{
			id = "beautiful_calligraphy",
			value = 2f,
			group_id = "harmony"
		});
		add(new LanguageTrait
		{
			id = "magic_words",
			group_id = "spirit"
		});
		t.setUnlockedWithAchievement("achievementTraitExplorerLanguage");
		add(new LanguageTrait
		{
			id = "words_of_madness",
			value = 0.1f,
			group_id = "chaos",
			spawn_random_trait_allowed = false,
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (!a.hasTrait("evil") && !a.hasTrait("blessed") && Randy.randomChance(pTrait.value))
				{
					a.addTrait("madness");
				}
			}
		});
		t.setUnlockedWithAchievement("achievementCursedWorld");
		add(new LanguageTrait
		{
			id = "cursed_font",
			value = 0.2f,
			group_id = "chaos",
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (!a.hasTrait("evil") && Randy.randomChance(pTrait.value))
				{
					a.addStatusEffect("cursed");
				}
			}
		});
		t.setUnlockedWithAchievement("achievementTheCorruptedTrees");
		add(new LanguageTrait
		{
			id = "font_of_gods",
			value = 0.2f,
			group_id = "spirit",
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (Randy.randomChance(pTrait.value))
				{
					a.addStatusEffect("enchanted");
				}
			}
		});
		add(new LanguageTrait
		{
			id = "chilly_font",
			value = 0.4f,
			group_id = "chaos",
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (Randy.randomChance(pTrait.value))
				{
					a.addStatusEffect("frozen");
				}
			}
		});
		add(new LanguageTrait
		{
			id = "ancient_runes",
			value = 0.4f,
			group_id = "spirit",
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (Randy.randomChance(pTrait.value))
				{
					a.addStatusEffect("spell_boost");
				}
			}
		});
		t.setUnlockedWithAchievement("achievementPlotsExplorer");
		add(new LanguageTrait
		{
			id = "repeated_sentences",
			value = 0.1f,
			group_id = "miscellaneous",
			spawn_random_trait_allowed = false,
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (Randy.randomChance(pTrait.value) && a.hasCity() && a.asset.can_be_cloned && a.city.hasFreeHouseSlots() && !a.city.hasReachedWorldLawLimit() && (!a.hasSubspecies() || !a.subspecies.hasReachedPopulationLimit()))
				{
					World.world.units.cloneUnit(a);
				}
			}
		});
		t.setUnlockedWithAchievement("achievementCloneWars");
		add(new LanguageTrait
		{
			id = "spooky_language",
			value = 0.2f,
			group_id = "chaos",
			spawn_random_trait_allowed = false,
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (Randy.randomChance(pTrait.value) && a.hasCity())
				{
					World.world.units.spawnNewUnit("ghost", a.current_tile.neighboursAll.GetRandom());
				}
			}
		});
		t.setUnlockedWithAchievement("achievementChildNamedToto");
		add(new LanguageTrait
		{
			id = "powerful_words",
			value = 0.2f,
			group_id = "spirit",
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (Randy.randomChance(pTrait.value))
				{
					a.addStatusEffect("powerup");
				}
			}
		});
		add(new LanguageTrait
		{
			id = "confusing_semantics",
			value = 0.2f,
			group_id = "chaos",
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (Randy.randomChance(pTrait.value))
				{
					a.makeStunned();
					if (a.hasLanguage())
					{
						a.joinLanguage(null);
					}
				}
			}
		});
		add(new LanguageTrait
		{
			id = "raging_paragraphs",
			value = 0.2f,
			group_id = "chaos",
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (Randy.randomChance(pTrait.value))
				{
					a.addStatusEffect("rage");
				}
			}
		});
		add(new LanguageTrait
		{
			id = "mortal_tongue",
			value = 0.2f,
			group_id = "chaos",
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (!a.hasTrait("evil") && !a.hasTrait("blessed") && Randy.randomChance(pTrait.value))
				{
					a.getHitFullHealth(AttackType.Divine);
				}
			}
		});
		t.setUnlockedWithAchievement("achievementCursedWorld");
		add(new LanguageTrait
		{
			id = "scorching_words",
			value = 0.5f,
			group_id = "chaos",
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (Randy.randomChance(pTrait.value))
				{
					a.addStatusEffect("burning");
				}
			}
		});
		add(new LanguageTrait
		{
			id = "doomed_glyphs",
			value = 0.1f,
			group_id = "chaos",
			spawn_random_trait_allowed = false,
			read_book_trait_action = delegate(Actor a, LanguageTrait pTrait, Book _)
			{
				if (!a.hasTrait("lucky") && WorldLawLibrary.world_law_disasters_nature.isEnabled() && Randy.randomChance(pTrait.value))
				{
					Meteorite.spawnMeteoriteDisaster(a.current_tile, a);
				}
			}
		});
		t.setUnlockedWithAchievement("achievementMultiplySpoken");
		add(new LanguageTrait
		{
			id = "enlightening_script",
			group_id = "knowledge"
		});
		t.base_stats["intelligence"] = 10f;
		add(new LanguageTrait
		{
			id = "foolish_glyphs",
			group_id = "knowledge"
		});
		t.base_stats["intelligence"] = -5f;
		add(new LanguageTrait
		{
			id = "eternal_text",
			group_id = "miscellaneous",
			spawn_random_trait_allowed = false
		});
		t.setUnlockedWithAchievement("achievementPie");
		t.base_stats["lifespan"] = 500f;
		add(new LanguageTrait
		{
			id = "elegant_words",
			group_id = "spirit"
		});
		t.base_stats["offspring"] = 4f;
		add(new LanguageTrait
		{
			id = "strict_spelling",
			group_id = "knowledge"
		});
		t.base_stats["warfare"] = 10f;
		add(new LanguageTrait
		{
			id = "divine_encryption",
			group_id = "special",
			can_be_given = false,
			can_be_removed = false,
			can_be_in_book = false,
			spawn_random_trait_allowed = false
		});
		add(new LanguageTrait
		{
			id = "grin_mark",
			group_id = "fate",
			spawn_random_trait_allowed = false,
			priority = -100
		});
		t.setTraitInfoToGrinMark();
		t.setUnlockedWithAchievement("achievementCreaturesExplorer");
	}

	public static float getValueFloat(string pID)
	{
		return AssetManager.language_traits.get(pID).value;
	}

	public static int getValue(string pID)
	{
		return (int)AssetManager.language_traits.get(pID).value;
	}
}
