using System;
using System.Collections.Generic;

[Serializable]
public class GeneLibrary : BaseTraitLibrary<GeneAsset>
{
	[NonSerialized]
	private Dictionary<string, List<GeneAsset>> _cached_stat_genes_dictionary = new Dictionary<string, List<GeneAsset>>();

	[NonSerialized]
	private List<GeneAsset> _gene_assets_simple = new List<GeneAsset>();

	[NonSerialized]
	private List<GeneAsset> _gene_assets_mutations = new List<GeneAsset>();

	public static GeneAsset gene_for_generation;

	protected override string icon_path => "ui/Icons/genes/";

	protected override List<string> getDefaultTraitsForMeta(ActorAsset pAsset)
	{
		return null;
	}

	public override void init()
	{
		base.init();
		addSpecial();
		addBaseStats();
		addFightStats();
		addBonusStats();
		addAttributes();
		gene_for_generation = get("temp_for_generation");
	}

	public string getRandomNormalGene()
	{
		int num = 10;
		foreach (GeneAsset item in list.LoopRandom())
		{
			if (num <= 0)
			{
				break;
			}
			num--;
			if (item.isAvailable() && !item.is_empty && !item.for_generation)
			{
				return item.id;
			}
		}
		return null;
	}

	private void addBonusStats()
	{
		add(new GeneAsset
		{
			id = "attack_speed"
		});
		t.base_stats["attack_speed"] = 1f;
		t.setUnlockedWithAchievement("achievementGenesExplorer");
		add(new GeneAsset
		{
			id = "scale_plus"
		});
		t.base_stats["scale"] = 0.03f;
		t.setUnlockedWithAchievement("achievementSimpleStupidGenetics");
		add(new GeneAsset
		{
			id = "scale_minus"
		});
		t.base_stats["scale"] = -0.01f;
		t.setUnlockedWithAchievement("achievementAntWorld");
	}

	private void addFightStats()
	{
		add(new GeneAsset
		{
			id = "armor_1",
			is_simple = true
		});
		t.base_stats["armor"] = 1f;
		add(new GeneAsset
		{
			id = "armor_2"
		});
		t.base_stats["armor"] = 6f;
		add(new GeneAsset
		{
			id = "armor_3"
		});
		t.base_stats["armor"] = 10f;
		add(new GeneAsset
		{
			id = "damage_1",
			is_simple = true
		});
		t.base_stats["damage"] = 1f;
		add(new GeneAsset
		{
			id = "damage_2"
		});
		t.base_stats["damage"] = 6f;
		add(new GeneAsset
		{
			id = "damage_3"
		});
		t.base_stats["damage"] = 10f;
	}

	private void addBaseStats()
	{
		add(new GeneAsset
		{
			id = "birth_rate_1"
		});
		t.base_stats["birth_rate"] = 1f;
		add(new GeneAsset
		{
			id = "offspring_1"
		});
		t.base_stats["offspring"] = 1f;
		add(new GeneAsset
		{
			id = "offspring_2"
		});
		t.base_stats["offspring"] = 3f;
		add(new GeneAsset
		{
			id = "offspring_3"
		});
		t.base_stats["offspring"] = 5f;
		add(new GeneAsset
		{
			id = "offspring_4"
		});
		t.base_stats["offspring"] = 10f;
		add(new GeneAsset
		{
			id = "lifespan_1"
		});
		t.base_stats["lifespan"] = 5f;
		add(new GeneAsset
		{
			id = "lifespan_2"
		});
		t.base_stats["lifespan"] = 20f;
		add(new GeneAsset
		{
			id = "lifespan_3"
		});
		t.base_stats["lifespan"] = 50f;
		add(new GeneAsset
		{
			id = "lifespan_4"
		});
		t.base_stats["lifespan"] = 100f;
		add(new GeneAsset
		{
			id = "health_1",
			is_simple = true
		});
		t.base_stats["health"] = 1f;
		add(new GeneAsset
		{
			id = "health_2",
			is_simple = true
		});
		t.base_stats["health"] = 10f;
		add(new GeneAsset
		{
			id = "health_3"
		});
		t.base_stats["health"] = 50f;
		add(new GeneAsset
		{
			id = "health_4"
		});
		t.base_stats["health"] = 100f;
		add(new GeneAsset
		{
			id = "health_5"
		});
		t.base_stats["health"] = 300f;
		add(new GeneAsset
		{
			id = "stamina_1",
			is_simple = true
		});
		t.base_stats["stamina"] = 10f;
		add(new GeneAsset
		{
			id = "stamina_2"
		});
		t.base_stats["stamina"] = 50f;
		add(new GeneAsset
		{
			id = "stamina_3"
		});
		t.base_stats["stamina"] = 100f;
		add(new GeneAsset
		{
			id = "speed_1"
		});
		t.base_stats["speed"] = 1f;
		add(new GeneAsset
		{
			id = "speed_2"
		});
		t.base_stats["speed"] = 2f;
		add(new GeneAsset
		{
			id = "speed_3"
		});
		t.base_stats["speed"] = 5f;
	}

	private void addAttributes()
	{
		add(new GeneAsset
		{
			id = "diplomacy_1",
			is_simple = true
		});
		t.base_stats["diplomacy"] = 1f;
		add(new GeneAsset
		{
			id = "diplomacy_2"
		});
		t.base_stats["diplomacy"] = 2f;
		add(new GeneAsset
		{
			id = "diplomacy_3"
		});
		t.base_stats["diplomacy"] = 3f;
		add(new GeneAsset
		{
			id = "warfare_1",
			is_simple = true
		});
		t.base_stats["warfare"] = 1f;
		add(new GeneAsset
		{
			id = "warfare_2"
		});
		t.base_stats["warfare"] = 2f;
		add(new GeneAsset
		{
			id = "warfare_3"
		});
		t.base_stats["warfare"] = 3f;
		add(new GeneAsset
		{
			id = "stewardship_1",
			is_simple = true
		});
		t.base_stats["stewardship"] = 1f;
		add(new GeneAsset
		{
			id = "stewardship_2"
		});
		t.base_stats["stewardship"] = 2f;
		add(new GeneAsset
		{
			id = "stewardship_3"
		});
		t.base_stats["stewardship"] = 3f;
		add(new GeneAsset
		{
			id = "intelligence_1",
			is_simple = true
		});
		t.base_stats["intelligence"] = 1f;
		add(new GeneAsset
		{
			id = "intelligence_2"
		});
		t.base_stats["intelligence"] = 2f;
		add(new GeneAsset
		{
			id = "intelligence_3"
		});
		t.base_stats["intelligence"] = 3f;
	}

	private void addSpecial()
	{
		add(new GeneAsset
		{
			id = "empty",
			path_icon = "ui/Icons/iconEmptyLocus",
			is_empty = true,
			show_in_knowledge_window = false,
			needs_to_be_explored = false,
			show_genepool_nucleobases = false,
			can_drop_and_grab = false
		});
		add(new GeneAsset
		{
			id = "temp_for_generation",
			path_icon = "ui/Icons/iconEmptyLocus",
			for_generation = true,
			is_empty = true,
			show_in_knowledge_window = false,
			needs_to_be_explored = false,
			show_genepool_nucleobases = false,
			can_drop_and_grab = false,
			has_description_1 = false,
			has_description_2 = false,
			has_localized_id = false
		});
		add(new GeneAsset
		{
			id = "bad",
			is_stat_gene = false,
			is_bad = true,
			is_simple = true,
			needs_to_be_explored = true,
			synergy_sides_always = true,
			show_genepool_nucleobases = false
		});
		add(new GeneAsset
		{
			id = "bonus_male",
			is_stat_gene = false,
			show_genepool_nucleobases = false,
			synergy_sides_always = true,
			is_bonus_male = true
		});
		add(new GeneAsset
		{
			id = "bonus_female",
			is_stat_gene = false,
			show_genepool_nucleobases = false,
			synergy_sides_always = true,
			is_bonus_female = true
		});
		add(new GeneAsset
		{
			id = "mutagenic",
			is_stat_gene = false,
			synergy_sides_always = true,
			show_genepool_nucleobases = false
		});
		t.base_stats_meta["mutation"] = 1f;
	}

	public override GeneAsset add(GeneAsset pAsset)
	{
		GeneAsset geneAsset = base.add(pAsset);
		geneAsset.has_description_1 = false;
		geneAsset.has_description_2 = false;
		return geneAsset;
	}

	public GeneAsset getRandomSimpleGene()
	{
		return _gene_assets_simple.GetRandom();
	}

	public GeneAsset getRandomGeneForMutation()
	{
		return _gene_assets_mutations.GetRandom();
	}

	public void regenerateBasicDNACodesWithLifeSeed(long pLifeSeed)
	{
		foreach (GeneAsset item in list)
		{
			if (item.show_genepool_nucleobases)
			{
				long pSeed = pLifeSeed + item.getIndexID();
				item.generateDNA(pSeed);
			}
		}
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (GeneAsset item in list)
		{
			if (item.is_simple)
			{
				_gene_assets_simple.Add(item);
			}
		}
		foreach (GeneAsset item2 in list)
		{
			if (!item2.is_empty)
			{
				_gene_assets_mutations.Add(item2);
			}
		}
	}

	public List<GeneAsset> getGenesWithStat(string pStatID)
	{
		if (!_cached_stat_genes_dictionary.ContainsKey(pStatID))
		{
			List<GeneAsset> value = filterGenes(pStatID);
			_cached_stat_genes_dictionary[pStatID] = value;
		}
		return _cached_stat_genes_dictionary[pStatID];
	}

	private List<GeneAsset> filterGenes(string pStatID)
	{
		List<GeneAsset> list = new List<GeneAsset>();
		foreach (GeneAsset item in AssetManager.gene_library.list)
		{
			if (!item.is_empty && (item.base_stats.hasStat(pStatID) || item.base_stats_meta.hasStat(pStatID)))
			{
				list.Add(item);
			}
		}
		return list;
	}
}
