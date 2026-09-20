using System;
using System.Collections.Generic;
using UnityEngine;
using db;

public class Subspecies : MetaObjectWithTraits<SubspeciesData, SubspeciesTrait>, ISapient
{
	private const float AGE_THRESHOLD_ADULT = 30f;

	private const float AGE_AGE_BREEDING_CIV = 18f;

	private const float AGE_THRESHOLD_ADULT_CIV = 16f;

	private const float AGE_MAX_ADULT = 16f;

	private const float AGE_EXPONENTIAL_ADULT = 0.55f;

	private const float AGE_MULTIPLIER_ADULT = 1.1f;

	public readonly Nucleus nucleus = new Nucleus();

	private int _cached_phenotype_index_for_banner;

	private List<PhenotypeAsset> _phenotype_list_assets = new List<PhenotypeAsset>();

	private HashSet<int> _phenotypes_set_indexes = new HashSet<int>();

	private bool _has_egg_form;

	private bool _has_mutation_reskin;

	private bool _needs_food;

	private bool _needs_mate;

	private bool _can_process_emotions;

	private bool _is_sapient;

	private bool _has_advanced_memory;

	private bool _has_advanced_communication;

	private bool _damaged_by_water;

	private bool _timid;

	private bool _curious;

	private bool _water_creature;

	private bool _hovering;

	private bool _pollinating;

	private bool _magic;

	private bool _diet_flowers;

	private bool _diet_fruits;

	private bool _diet_crops;

	private bool _diet_vegetation;

	private bool _diet_meat;

	private bool _diet_blood;

	private bool _diet_minerals;

	private bool _diet_wood;

	private bool _diet_cannibalism;

	private int _cached_metabolic_rate;

	private bool _cached_energy_preserver;

	private int _cached_males;

	private int _cached_females;

	private string _egg_id;

	private SubspeciesTrait _egg_asset;

	private string _mutation_skin_id;

	private SubspeciesTrait _mutation_skin_asset;

	private string _cached_skin_male;

	private string _cached_skin_female;

	private string _cached_skin_warrior;

	private readonly HashSet<string> _allowed_food_by_diet = new HashSet<string>();

	private readonly SubspeciesActorBirthTraits _actor_birth_traits = new SubspeciesActorBirthTraits();

	private bool _trait_changed_event;

	private Sprite _cached_unit_sprite_for_banner;

	private const string reproduction_neuron = "reproduction_neuron";

	private const string reproduction_basics_1 = "reproduction_basics_1";

	private const string reproduction_basics_2 = "reproduction_basics_2";

	private const string reproduction_basics_3 = "reproduction_basics_3";

	private const string reproduction_basics_4 = "reproduction_basics_4";

	private const string reproduction_sexual_try = "reproduction_sexual_try";

	private const string reproduction_acts = "reproduction_acts";

	private const string reproduction = "reproduction";

	private const string births = "births";

	private const string new_adults = "new_adults";

	public static string[] ALL_REPRODUCTION_COUNTERS = new string[10] { "reproduction_neuron", "reproduction_basics_1", "reproduction_basics_2", "reproduction_basics_3", "reproduction_basics_4", "reproduction_sexual_try", "reproduction_acts", "reproduction", "births", "new_adults" };

	public RateCounter counter_reproduction_neuron;

	public RateCounter counter_reproduction_basics_1;

	public RateCounter counter_reproduction_basics_2;

	public RateCounter counter_reproduction_basics_3;

	public RateCounter counter_reproduction_basics_4;

	public RateCounter counter_reproduction_sexual_try;

	public RateCounter counter_reproduction;

	public RateCounter counter_reproduction_acts;

	public RateCounter counter_births;

	public RateCounter counter_new_adults;

	public List<RateCounter> list_counters = new List<RateCounter>();

	protected override MetaType meta_type => MetaType.Subspecies;

	public override BaseSystemManager manager => World.world.subspecies;

	protected override bool track_death_types => true;

	protected override AssetLibrary<SubspeciesTrait> trait_library => AssetManager.subspecies_traits;

	protected override List<string> default_traits => getActorAsset().default_subspecies_traits;

	protected override List<string> saved_traits => data.saved_traits;

	protected override string species_id => data.species_id;

	public int cached_males => _cached_males;

	public int cached_females => _cached_females;

	public bool has_trait_energy_preserver => _cached_energy_preserver;

	public bool has_trait_timid => _timid;

	public bool has_trait_curious => _curious;

	public bool has_trait_water_creature => _water_creature;

	public bool has_trait_hovering => _hovering;

	public bool has_trait_pollinating => _pollinating;

	public float age_adult => base_stats_meta["age_adult"];

	public float age_breeding => base_stats_meta["age_breeding"];

	public bool diet_vegetation => _diet_vegetation;

	public bool diet_meat => _diet_meat;

	public BaseStats base_stats_male => nucleus.base_stats_male;

	public BaseStats base_stats_female => nucleus.base_stats_female;

	public bool needs_food => _needs_food;

	public bool needs_mate => _needs_mate;

	public bool can_process_emotions => _can_process_emotions;

	public bool has_advanced_memory => _has_advanced_memory;

	public bool has_advanced_communication => _has_advanced_communication;

	public bool is_damaged_by_water => _damaged_by_water;

	public bool has_egg_form => _has_egg_form;

	public string egg_id => _egg_id;

	public string egg_sprite_path => _egg_asset.sprite_path;

	public SubspeciesTrait egg_asset => _egg_asset;

	public Sprite egg_sprite => _egg_asset.getSprite();

	public bool has_mutation_reskin => _has_mutation_reskin;

	public SubspeciesTrait mutation_skin_asset => _mutation_skin_asset;

	protected override void setDefaultValues()
	{
		base.setDefaultValues();
		initReproductionCounters();
	}

	public void newSpecies(ActorAsset pAsset, WorldTile pTile, bool pMutation = false)
	{
		data.species_id = pAsset.id;
		generateNewMetaObject();
		if (pMutation)
		{
			addDNAMutationToSeed();
		}
		generateNucleus();
		generateActorBirthTraits();
		generatePhenotype(pAsset, pTile);
		generateName(pAsset, pTile);
		createSkins();
		_trait_changed_event = false;
		recalcBaseStats();
	}

	protected override void generateNewMetaObject()
	{
		base.generateNewMetaObject();
		if (!WorldLawLibrary.world_law_mutant_box.isEnabled())
		{
			return;
		}
		int num = Randy.randomInt(1, 4);
		for (int i = 0; i < num; i++)
		{
			SubspeciesTrait randomSpawnTrait = AssetManager.subspecies_traits.getRandomSpawnTrait();
			if (randomSpawnTrait.isAvailable())
			{
				addTrait(randomSpawnTrait, pRemoveOpposites: true);
			}
		}
	}

	private void createSkins()
	{
		ActorAsset actorAsset = getActorAsset();
		int skin_id = Randy.randomInt(0, actorAsset.skin_citizen_female.Length);
		data.skin_id = skin_id;
	}

	public string getSkinFemale()
	{
		return _cached_skin_female;
	}

	public string getSkinMale()
	{
		return _cached_skin_male;
	}

	public string getSkinWarrior()
	{
		return _cached_skin_warrior;
	}

	public bool hasEvolvedIntoForm()
	{
		return data.evolved_into_subspecies.hasValue();
	}

	public Subspecies getEvolvedInto()
	{
		Subspecies subspecies = World.world.subspecies.get(data.evolved_into_subspecies);
		if (subspecies == null)
		{
			return null;
		}
		if (!subspecies.isAlive())
		{
			return null;
		}
		return subspecies;
	}

	public void setEvolutionSubspecies(Subspecies pSubspecies)
	{
		if (!data.evolved_into_subspecies.hasValue() || !(World.world.getWorldTimeElapsedSince(data.last_evolution_timestamp) < 60f))
		{
			data.last_evolution_timestamp = World.world.getCurWorldTime();
			data.evolved_into_subspecies = pSubspecies.getID();
		}
	}

	public int getMaxRandomMutations()
	{
		return (int)base_stats_meta["mutation"];
	}

	public int getAmountOfRandomMutationsSubspecies()
	{
		int maxRandomMutations = getMaxRandomMutations();
		if (maxRandomMutations == 0)
		{
			return 0;
		}
		return Randy.randomInt(0, maxRandomMutations + 1);
	}

	public int getAmountOfRandomMutationsActorTraits()
	{
		int pMaxExclusive = getMaxRandomMutations() + 1;
		return Randy.randomInt(0, pMaxExclusive);
	}

	public void mutateFrom(Subspecies pParentsSubspecies)
	{
		int amountOfRandomMutationsSubspecies = pParentsSubspecies.getAmountOfRandomMutationsSubspecies();
		cloneSubspeciesTraits(pParentsSubspecies);
		nucleus.cloneFrom(pParentsSubspecies.nucleus);
		nucleus.doRandomGeneMutations(amountOfRandomMutationsSubspecies + 1);
		mutateTraits(amountOfRandomMutationsSubspecies);
		genesChangedEvent();
		increaseGeneration(pParentsSubspecies.getGeneration());
	}

	private void increaseGeneration(int pFromGeneration)
	{
		setGeneration(pFromGeneration + 1);
	}

	private void setGeneration(int pValue)
	{
		data.generation = pValue;
	}

	public int getGeneration()
	{
		return data.generation;
	}

	private void cloneSubspeciesTraits(Subspecies pParentsSubspecies)
	{
		bool unit_zombie = getActorAsset().unit_zombie;
		clearTraits();
		foreach (SubspeciesTrait trait in pParentsSubspecies.getTraits())
		{
			if (!unit_zombie || !trait.remove_for_zombies)
			{
				addTrait(trait);
			}
		}
	}

	internal void mutateTraits(int pMutations)
	{
		int num = 0;
		for (int i = 0; i < pMutations; i++)
		{
			SubspeciesTrait randomMutationTraitToAdd = AssetManager.subspecies_traits.getRandomMutationTraitToAdd();
			if (addTrait(randomMutationTraitToAdd, pRemoveOpposites: true))
			{
				num++;
			}
		}
		if (num <= 0)
		{
			return;
		}
		int num2 = 0;
		for (int j = 0; j < num; j++)
		{
			SubspeciesTrait randomMutationTraitToRemove = AssetManager.subspecies_traits.getRandomMutationTraitToRemove();
			if (removeTrait(randomMutationTraitToRemove))
			{
				num2++;
			}
		}
	}

	public override void increaseBirths()
	{
		base.increaseBirths();
		addRenown(1);
		counter_births?.registerEvent();
	}

	public bool needOppositeSexTypeForReproduction()
	{
		if (hasTraitReproductionSexual())
		{
			return true;
		}
		return false;
	}

	public bool isPartnerSuitableForReproduction(Actor pActor, Actor pTarget)
	{
		if (needOppositeSexTypeForReproduction())
		{
			return pActor.data.sex != pTarget.data.sex;
		}
		return true;
	}

	public int getRandomPhenotypeIndex()
	{
		return getRandomPhenotypeAsset()?.phenotype_index ?? 0;
	}

	public PhenotypeAsset getRandomPhenotypeAsset()
	{
		if (_phenotype_list_assets.Count == 0)
		{
			return null;
		}
		return _phenotype_list_assets.GetRandom();
	}

	public int getMainPhenotypeIndexForBanner()
	{
		return _cached_phenotype_index_for_banner;
	}

	public void generateActorBirthTraits()
	{
		ActorAsset actorAsset = getActorAsset();
		_actor_birth_traits.init(actorAsset, this);
	}

	public void makeSapient()
	{
		addTrait("amygdala");
		addTrait("advanced_hippocampus");
		addTrait("prefrontal_cortex");
		addTrait("wernicke_area");
	}

	public void generateNucleus()
	{
		ActorAsset actorAsset = getActorAsset();
		Randy.resetSeed(World.world.map_stats.life_dna + actorAsset.getIndexID() + actorAsset.countSubspecies() + data.mutation);
		nucleus.createFrom(actorAsset);
	}

	public void addDNAMutationToSeed()
	{
		data.mutation = Randy.randomInt(0, 55555);
	}

	public void genesChangedEvent()
	{
		nucleus.setDirty();
		recalcBaseStats();
		makeAllUnitsDirtyAndConfused();
	}

	private void makeAllUnitsDirtyAndConfused()
	{
		foreach (Actor unit in base.units)
		{
			if (!unit.isRekt())
			{
				unit.event_full_stats = true;
				unit.setStatsDirty();
				unit.cancelAllBeh();
				unit.makeConfused();
			}
		}
	}

	public bool isBiomeSpecific()
	{
		if (data.biome_variant == "default_color")
		{
			return false;
		}
		return true;
	}

	public bool hasPhenotype()
	{
		return getActorAsset().use_phenotypes;
	}

	public override void generateBanner()
	{
		data.banner_background_id = AssetManager.subspecies_banners_library.getNewIndexBackground();
	}

	public int getMetabolicRate()
	{
		return _cached_metabolic_rate;
	}

	protected override void recalcBaseStats()
	{
		base.recalcBaseStats();
		clearVisualCache();
		if (_trait_changed_event)
		{
			_trait_changed_event = false;
			makeAllUnitsDirtyAndConfused();
		}
		base_stats.mergeStats(getActorAsset().base_stats);
		base_stats.mergeStats(nucleus.getStats());
		base_stats_meta.mergeStats(nucleus.getStatsMeta());
		base_stats["health"] = Mathf.Max(base_stats["health"], 1f);
		base_stats["damage"] = Mathf.Max(base_stats["damage"], 1f);
		base_stats["lifespan"] = Mathf.Max(base_stats["lifespan"], 1f);
		base_stats["speed"] = Mathf.Max(base_stats["speed"], 1f);
		_cached_metabolic_rate = (int)Mathf.Max((float)SimGlobals.m.base_metabolic_rate, base_stats["metabolic_rate"]);
		_cached_energy_preserver = hasTrait("energy_preserver");
		_timid = hasTrait("cautious_instincts");
		_curious = hasTrait("inquisitive_nature");
		_water_creature = hasTrait("aquatic");
		_hovering = hasTrait("hovering");
		checkForgetMetas();
		cacheTags();
		calcAllowedFoodByDiet();
		checkMutationSkin();
		cacheSkins();
		checkReproductionStrategy();
		calculateAgeRelatedStats();
		checkCurrentColor();
	}

	private void checkForgetMetas()
	{
		bool is_sapient = _is_sapient;
		bool flag = _has_advanced_memory;
		bool flag2 = _has_advanced_communication;
		bool flag3 = hasMetaTag("has_sapience");
		bool flag4 = hasMetaTag("has_advanced_memory");
		bool flag5 = hasMetaTag("has_advanced_communication");
		if (is_sapient && !flag3)
		{
			foreach (Actor unit in base.units)
			{
				if (!unit.isRekt() && unit.isKingdomCiv())
				{
					unit.forgetKingdomAndCity();
				}
			}
		}
		if (flag != flag4)
		{
			foreach (Actor unit2 in base.units)
			{
				if (!unit2.isRekt())
				{
					if (unit2.hasCulture())
					{
						unit2.forgetCulture();
					}
					if (unit2.hasReligion())
					{
						unit2.forgetReligion();
					}
				}
			}
		}
		if (flag2 == flag5)
		{
			return;
		}
		foreach (Actor unit3 in base.units)
		{
			if (!unit3.isRekt() && unit3.hasLanguage())
			{
				unit3.forgetLanguage();
			}
		}
	}

	private void calculateAgeRelatedStats()
	{
		getActorAsset();
		int num = (int)base_stats["lifespan"];
		float num2;
		float num3;
		if ((float)num > 30f && isSapient())
		{
			num2 = 16f;
			num3 = 18f;
		}
		else
		{
			num2 = Mathf.Pow((float)num, 0.55f) * 1.1f;
			num3 = num2;
		}
		if (num2 > 16f)
		{
			num2 = 16f;
		}
		if (isSapient() && num3 > 18f)
		{
			num3 = 18f;
		}
		base_stats_meta["age_adult"] = num2;
		base_stats_meta["age_breeding"] = num3;
	}

	private void cacheTags()
	{
		_is_sapient = hasMetaTag("has_sapience");
		_needs_food = hasMetaTag("needs_food");
		_needs_mate = hasMetaTag("needs_mate");
		_can_process_emotions = hasMetaTag("has_emotions");
		_has_advanced_memory = hasMetaTag("has_advanced_memory");
		_has_advanced_communication = hasMetaTag("has_advanced_communication");
		_damaged_by_water = hasMetaTag("damaged_by_water");
		_diet_vegetation = hasMetaTag("diet_vegetation");
		_diet_meat = hasMetaTag("diet_meat");
		_diet_blood = hasMetaTag("diet_blood");
		_diet_minerals = hasMetaTag("diet_minerals");
		_diet_wood = hasMetaTag("diet_wood");
		_diet_cannibalism = hasMetaTag("diet_same_species");
		_magic = hasMetaTag("magic");
	}

	public bool hasCannibalism()
	{
		return _diet_cannibalism;
	}

	public bool isSapient()
	{
		return _is_sapient;
	}

	public bool isMagic()
	{
		return _magic;
	}

	public ReproductiveStrategy getReproductionStrategy()
	{
		if (hasTraitOviparity())
		{
			return ReproductiveStrategy.Egg;
		}
		if (hasTraitViviparity())
		{
			return ReproductiveStrategy.Pregnancy;
		}
		return ReproductiveStrategy.SpawnUnitImmediate;
	}

	public bool isReproductionSexual()
	{
		return hasMetaTag("reproduction_sexual");
	}

	public bool hasTraitReproductionSexual()
	{
		return hasTrait("reproduction_sexual");
	}

	public bool hasTraitReproductionSexualHermaphroditic()
	{
		return hasTrait("reproduction_hermaphroditic");
	}

	public bool hasTraitOviparity()
	{
		return hasTrait("reproduction_strategy_oviparity");
	}

	public bool hasTraitViviparity()
	{
		return hasTrait("reproduction_strategy_viviparity");
	}

	private void checkReproductionStrategy()
	{
		bool flag = _has_egg_form;
		if (hasTrait("reproduction_strategy_oviparity"))
		{
			_has_egg_form = true;
			_egg_id = "egg_shell_plain";
			foreach (SubspeciesTrait trait in getTraits())
			{
				if (trait.phenotype_egg)
				{
					_egg_id = trait.id_egg;
					break;
				}
			}
			_egg_asset = AssetManager.subspecies_traits.get(_egg_id);
		}
		else
		{
			_has_egg_form = false;
		}
		if (flag == _has_egg_form)
		{
			return;
		}
		resetUnitSprites();
		foreach (Actor unit in base.units)
		{
			if (!unit.isRekt())
			{
				unit.cancelAllBeh();
			}
		}
	}

	private void checkMutationSkin()
	{
		_mutation_skin_asset = null;
		bool flag = _has_mutation_reskin;
		_has_mutation_reskin = false;
		foreach (SubspeciesTrait trait in getTraits())
		{
			if (trait.is_mutation_skin)
			{
				_mutation_skin_id = trait.id;
				_mutation_skin_asset = AssetManager.subspecies_traits.get(_mutation_skin_id);
				_has_mutation_reskin = true;
				break;
			}
		}
		if (flag != _has_mutation_reskin)
		{
			resetUnitSprites();
		}
	}

	private void cacheSkins()
	{
		int skin_id = data.skin_id;
		if (_has_mutation_reskin)
		{
			int count = _mutation_skin_asset.skin_citizen_male.Count;
			int index = Toolbox.loopIndex(skin_id, count);
			_cached_skin_male = _mutation_skin_asset.skin_citizen_male[index];
			_cached_skin_female = _mutation_skin_asset.skin_citizen_female[index];
			_cached_skin_warrior = _mutation_skin_asset.skin_warrior[index];
		}
		else
		{
			ActorAsset actorAsset = getActorAsset();
			_cached_skin_male = actorAsset.skin_citizen_male[skin_id];
			_cached_skin_female = actorAsset.skin_citizen_female[skin_id];
			_cached_skin_warrior = actorAsset.skin_warrior[skin_id];
		}
	}

	private void checkCurrentColor()
	{
		if (getActorAsset().use_phenotypes)
		{
			ListPool<PhenotypeAsset> pList = new ListPool<PhenotypeAsset>(_phenotype_list_assets);
			clearPhenotypeCache();
			fillPhenotypeCache();
			if (!Toolbox.areListsEqual(pList, _phenotype_list_assets))
			{
				resetUnitSprites();
				_cached_phenotype_index_for_banner = _phenotype_list_assets.GetRandom().phenotype_index;
			}
		}
	}

	private void fillPhenotypeCache()
	{
		ActorAsset actorAsset = getActorAsset();
		if (!actorAsset.use_phenotypes)
		{
			return;
		}
		foreach (SubspeciesTrait trait in getTraits())
		{
			if (trait.phenotype_skin)
			{
				PhenotypeAsset phenotypeAsset = trait.getPhenotypeAsset();
				cachePhenotype(phenotypeAsset);
			}
		}
		if (_phenotypes_set_indexes.Count == 0)
		{
			PhenotypeAsset defaultPhenotypeAsset = actorAsset.getDefaultPhenotypeAsset();
			cachePhenotype(defaultPhenotypeAsset);
		}
	}

	private void clearPhenotypeCache()
	{
		_phenotype_list_assets.Clear();
		_phenotypes_set_indexes.Clear();
	}

	private void cachePhenotype(PhenotypeAsset pPhenotypeAsset)
	{
		_phenotype_list_assets.Add(pPhenotypeAsset);
		_phenotypes_set_indexes.Add(pPhenotypeAsset.phenotype_index);
	}

	public void checkPhenotypeColor()
	{
		foreach (Actor unit in base.units)
		{
			if (!unit.isRekt())
			{
				checkIfPhenotypeIsLegit(unit);
			}
		}
	}

	private void checkIfPhenotypeIsLegit(Actor pActor)
	{
		int phenotype_index = pActor.data.phenotype_index;
		if (phenotype_index == 0 || !_phenotypes_set_indexes.Contains(phenotype_index))
		{
			pActor.generatePhenotypeAndShade();
		}
	}

	private void resetUnitSprites()
	{
		foreach (Actor unit in base.units)
		{
			if (!unit.isRekt())
			{
				checkIfPhenotypeIsLegit(unit);
				unit.setStatsDirty();
				unit.clearSprites();
				unit.clearLastColorCache();
			}
		}
	}

	public int countCurrentFamilies()
	{
		int num = 0;
		foreach (Family family in World.world.families)
		{
			if (family.data.subspecies_id == data.id)
			{
				num++;
			}
		}
		return num;
	}

	public Sprite getSpriteBackground()
	{
		return AssetManager.subspecies_banners_library.getSpriteBackground(data.banner_background_id);
	}

	protected override ColorLibrary getColorLibrary()
	{
		return AssetManager.subspecies_colors_library;
	}

	public bool isSpecies(string pSpeciesCheck)
	{
		return species_id == pSpeciesCheck;
	}

	private void generateName(ActorAsset pAsset, WorldTile pTile)
	{
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		stringBuilderPool.Append(pAsset.name_taxonomic_genus);
		if (!string.IsNullOrEmpty(pAsset.name_taxonomic_species))
		{
			stringBuilderPool.Append(" ");
			stringBuilderPool.Append(pAsset.name_taxonomic_species);
		}
		if (pAsset.name_subspecies_add_biome_suffix && pTile.Type.is_biome && pAsset.hasBiomePhenotype(pTile.Type.biome_asset.id))
		{
			string random = pTile.Type.biome_asset.subspecies_name_suffix.GetRandom();
			stringBuilderPool.Append(" ");
			stringBuilderPool.Append(random);
		}
		for (int i = 0; i < 5; i++)
		{
			if (!hasNameInWorld(stringBuilderPool))
			{
				break;
			}
			stringBuilderPool.Append(SubspeciesManager.NAME_ENDINGS.GetRandom());
		}
		stringBuilderPool.ToTitleCase();
		setName(stringBuilderPool.ToString());
	}

	private bool hasNameInWorld(StringBuilderPool pName)
	{
		ReadOnlySpan<char> source = pName.AsSpan();
		Span<char> span = new char[source.Length];
		source.ToLowerInvariant(span);
		Span<char> span2 = new char[pName.Length];
		foreach (Subspecies subspecy in World.world.subspecies)
		{
			if (subspecy == this)
			{
				continue;
			}
			string text = subspecy.name;
			if (text.Length == span.Length)
			{
				text.AsSpan().ToLowerInvariant(span2);
				if (span.SequenceEqual(span2))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void calcAllowedFoodByDiet()
	{
		_allowed_food_by_diet.Clear();
		foreach (KeyValuePair<string, List<string>> diet_food_pool in AssetManager.resources.diet_food_pools)
		{
			string key = diet_food_pool.Key;
			if (hasTrait(key))
			{
				List<string> value = diet_food_pool.Value;
				_allowed_food_by_diet.UnionWith(value);
			}
		}
	}

	public HashSet<string> getAllowedFoodByDiet()
	{
		return _allowed_food_by_diet;
	}

	private void generatePhenotype(ActorAsset pAsset, WorldTile pTile)
	{
		if (pAsset.use_phenotypes)
		{
			data.biome_variant = pTile.Type.biome_id;
			if (string.IsNullOrEmpty(data.biome_variant))
			{
				data.biome_variant = "default_color";
			}
			generatePhenotype(pAsset, data.biome_variant);
		}
	}

	private void generatePhenotype(ActorAsset pAsset, string pColorVariationForBiome = "default_color")
	{
		if (!pAsset.use_phenotypes)
		{
			return;
		}
		if (pAsset.phenotypes_dict == null || pAsset.phenotypes_dict.Count == 0)
		{
			Debug.LogError((object)("No phenotypes. Check assets " + pAsset.id));
			return;
		}
		if (!pAsset.hasBiomePhenotype(pColorVariationForBiome))
		{
			pColorVariationForBiome = "default_color";
		}
		List<string> list = pAsset.phenotypes_dict[pColorVariationForBiome];
		if (list.Count != 0)
		{
			string random = list.GetRandom();
			PhenotypeAsset phenotypeAsset = AssetManager.phenotype_library.get(random);
			SubspeciesTrait pTrait = AssetManager.subspecies_traits.get(phenotypeAsset.subspecies_trait_id);
			addTrait(pTrait);
		}
	}

	public override void loadData(SubspeciesData pData)
	{
		base.loadData(pData);
		nucleus.reset();
		List<ChromosomeData> saved_chromosome_data = data.saved_chromosome_data;
		if (saved_chromosome_data != null && saved_chromosome_data.Count > 0)
		{
			foreach (ChromosomeData saved_chromosome_datum in data.saved_chromosome_data)
			{
				Chromosome chromosome = new Chromosome(saved_chromosome_datum.chromosome_type, pNew: false);
				chromosome.load(saved_chromosome_datum);
				nucleus.addChromosome(chromosome);
			}
		}
		_actor_birth_traits.setSubspecies(this);
		_actor_birth_traits.reset();
		_actor_birth_traits.fillTraitAssetsFromStringList(data.saved_actor_birth_traits);
		recalcBaseStats();
	}

	public override void save()
	{
		base.save();
		data.saved_chromosome_data = nucleus.getListForSave();
		data.saved_traits = getTraitsAsStrings();
		data.saved_actor_birth_traits = _actor_birth_traits.getTraitsAsStrings();
	}

	public void debugClear()
	{
		loadData(data);
	}

	public string getRandomBioProduct()
	{
		using ListPool<string> listPool = new ListPool<string>();
		if (hasTrait("bioproduct_gems"))
		{
			listPool.Add("mineral_gems");
		}
		if (hasTrait("bioproduct_stone"))
		{
			listPool.Add("mineral_stone");
		}
		if (hasTrait("bioproduct_mushrooms"))
		{
			listPool.Add("mushroom_red");
			listPool.Add("mushroom_green");
			listPool.Add("mushroom_teal");
			listPool.Add("mushroom_white");
			listPool.Add("mushroom_yellow");
		}
		if (hasTrait("bioproduct_gold"))
		{
			listPool.Add("mineral_gold");
		}
		if (listPool.Count == 0)
		{
			return "poop";
		}
		return listPool.GetRandom();
	}

	public override void Dispose()
	{
		DBInserter.deleteData(getID(), "subspecies");
		_mutation_skin_asset = null;
		_cached_phenotype_index_for_banner = 0;
		_phenotype_list_assets.Clear();
		_phenotypes_set_indexes.Clear();
		base_stats.reset();
		nucleus.reset();
		_actor_birth_traits.reset();
		spells.reset();
		_egg_asset = null;
		base.Dispose();
	}

	public bool hasParentSubspecies()
	{
		return data.parent_subspecies.hasValue();
	}

	public void unstableGenomeEvent()
	{
		nucleus.unstableGenomeEvent();
		genesChangedEvent();
	}

	public void cacheCounters()
	{
		_cached_females = countFemales();
		_cached_males = countMales();
	}

	public void eventGMO()
	{
		addTrait("gmo");
		_trait_changed_event = true;
	}

	public float getMaturationTimeMonths()
	{
		return 0f + base_stats_meta["maturation"];
	}

	public override bool addTrait(SubspeciesTrait pTrait, bool pRemoveOpposites = false)
	{
		if (!canAddTrait(pTrait))
		{
			return false;
		}
		return base.addTrait(pTrait, pRemoveOpposites);
	}

	public bool canAddTrait(SubspeciesTrait pTrait)
	{
		ActorAsset actorAsset = getActorAsset();
		if (actorAsset.trait_filter_subspecies != null && actorAsset.trait_filter_subspecies.Contains(pTrait.id))
		{
			return false;
		}
		if (actorAsset.trait_group_filter_subspecies != null && actorAsset.trait_group_filter_subspecies.Contains(pTrait.group_id))
		{
			return false;
		}
		return true;
	}

	public string getPossibleAttribute()
	{
		if (nucleus.pot_possible_attributes.Count == 0)
		{
			return null;
		}
		return nucleus.pot_possible_attributes.GetRandom();
	}

	public bool addBirthTrait(string pActorTraitID)
	{
		ActorTrait actorTrait = AssetManager.traits.get(pActorTraitID);
		if (actorTrait == null)
		{
			return false;
		}
		return _actor_birth_traits.addTrait(actorTrait);
	}

	public SubspeciesActorBirthTraits getActorBirthTraits()
	{
		return _actor_birth_traits;
	}

	public int countMainKingdoms()
	{
		int num = 0;
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (kingdom.getMainSubspecies() == this)
			{
				num++;
			}
		}
		return num;
	}

	public bool hasPopulationLimit()
	{
		return base_stats_meta["limit_population"] > 0f;
	}

	public bool hasReachedPopulationLimit()
	{
		int num = (int)base_stats_meta["limit_population"];
		if (num == 0)
		{
			return false;
		}
		return countUnits() >= num;
	}

	public int countMainCities()
	{
		int num = 0;
		foreach (City city in World.world.cities)
		{
			if (city.getMainSubspecies() == this)
			{
				num++;
			}
		}
		return num;
	}

	public Subspecies getSkeletonForm()
	{
		long skeleton_form_id = data.skeleton_form_id;
		Subspecies subspecies = World.world.subspecies.get(skeleton_form_id);
		if (subspecies.isRekt())
		{
			return null;
		}
		return subspecies;
	}

	public override void traitModifiedEvent()
	{
		_trait_changed_event = true;
		base.traitModifiedEvent();
	}

	public void setSkeletonForm(Subspecies pSkeletonForm)
	{
		data.skeleton_form_id = pSkeletonForm.id;
		ActorAsset actorAsset = pSkeletonForm.getActorAsset();
		string text = "";
		if (actorAsset.generated_subspecies_names_prefixes != null)
		{
			text = actorAsset.generated_subspecies_names_prefixes.GetRandom();
		}
		if (!string.IsNullOrEmpty(text))
		{
			string pName = text.FirstToUpper() + " " + name;
			pSkeletonForm.setName(pName, pTrack: false);
		}
	}

	private void clearVisualCache()
	{
		_cached_unit_sprite_for_banner = null;
	}

	public Sprite getUnitSpriteForBanner()
	{
		if ((Object)(object)_cached_unit_sprite_for_banner != (Object)null)
		{
			return _cached_unit_sprite_for_banner;
		}
		ActorAsset actorAsset = getActorAsset();
		SubspeciesTrait subspeciesTrait = null;
		ActorTextureSubAsset texture_asset;
		if (has_mutation_reskin)
		{
			subspeciesTrait = mutation_skin_asset;
			texture_asset = subspeciesTrait.texture_asset;
		}
		else
		{
			texture_asset = actorAsset.texture_asset;
		}
		AnimationContainerUnit containerForUI = DynamicActorSpriteCreatorUI.getContainerForUI(actorAsset, pAdult: true, texture_asset, subspeciesTrait);
		ColorAsset default_kingdom_color = AssetManager.kingdoms.get(actorAsset.kingdom_id_wild).default_kingdom_color;
		int mainPhenotypeIndexForBanner = getMainPhenotypeIndexForBanner();
		int pPhenotypeShade = 0;
		return _cached_unit_sprite_for_banner = DynamicActorSpriteCreatorUI.getUnitSpriteForUI(actorAsset, containerForUI.walking.frames[0], containerForUI, pAdult: true, ActorSex.Male, mainPhenotypeIndexForBanner, pPhenotypeShade, default_kingdom_color, 0L, 0);
	}

	public override bool hasCities()
	{
		foreach (City city in World.world.cities)
		{
			if (city.getMainSubspecies() == this)
			{
				return true;
			}
		}
		return false;
	}

	public override IEnumerable<City> getCities()
	{
		foreach (City city in World.world.cities)
		{
			if (city.getMainSubspecies() == this)
			{
				yield return city;
			}
		}
	}

	public override bool hasKingdoms()
	{
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (kingdom.getMainSubspecies() == this)
			{
				return true;
			}
		}
		return false;
	}

	public override IEnumerable<Kingdom> getKingdoms()
	{
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (kingdom.getMainSubspecies() == this)
			{
				yield return kingdom;
			}
		}
	}

	public void initReproductionCounters()
	{
	}

	private RateCounter checkNewCounter(RateCounter pCounter, string pID)
	{
		if (pCounter == null)
		{
			pCounter = new RateCounter(pID);
			list_counters.Add(pCounter);
		}
		pCounter.reset();
		return pCounter;
	}

	public void debugReproductionEvents(DebugTool pTool)
	{
	}

	public void counterReproduction()
	{
		counter_reproduction?.registerEvent();
	}

	public void countReproductionNeuron()
	{
		counter_reproduction_neuron?.registerEvent();
	}
}
