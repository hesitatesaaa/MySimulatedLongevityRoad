using System.Collections.Generic;
using UnityEngine;

public class Nucleus
{
	public readonly List<Chromosome> chromosomes = new List<Chromosome>();

	private readonly BaseStats _merged_base_stats_male = new BaseStats();

	private readonly BaseStats _merged_base_stats_female = new BaseStats();

	private readonly BaseStats _merged_base_stats = new BaseStats();

	private readonly BaseStats _merged_base_stats_meta = new BaseStats();

	private bool _dirty = true;

	private readonly BaseStats[] _base_stats_all = new BaseStats[4];

	public readonly List<string> pot_possible_attributes = new List<string>();

	public BaseStats base_stats_male => _merged_base_stats_male;

	public BaseStats base_stats_female => _merged_base_stats_female;

	public Nucleus()
	{
		_base_stats_all[0] = _merged_base_stats;
		_base_stats_all[1] = _merged_base_stats_meta;
		_base_stats_all[2] = _merged_base_stats_male;
		_base_stats_all[3] = _merged_base_stats_female;
	}

	public void createFrom(ActorAsset pActorAsset)
	{
		reset();
		Chromosome chromosome = null;
		using ListPool<string> listPool = generateGenesFromBaseStats(pActorAsset.genome_parts);
		listPool.Shuffle();
		bool flag = WorldLawLibrary.world_law_gene_spaghetti.isEnabled();
		for (int i = 0; i < listPool.Count; i++)
		{
			string text = listPool[i];
			if (flag && Randy.randomChance(0.777f))
			{
				string randomNormalGene = AssetManager.gene_library.getRandomNormalGene();
				if (!string.IsNullOrEmpty(randomNormalGene))
				{
					text = randomNormalGene;
				}
			}
			GeneAsset geneAsset = AssetManager.gene_library.get(text);
			if (geneAsset == null)
			{
				Debug.LogError((object)("GeneAsset not found: " + text));
				continue;
			}
			if (chromosome != null && chromosome.canAddGene(geneAsset))
			{
				chromosome.addGene(geneAsset);
				continue;
			}
			List<string> chromosomes_first = pActorAsset.chromosomes_first;
			ChromosomeTypeAsset chromosomeTypeAsset = ((chromosomes_first == null || chromosomes_first.Count <= 0 || chromosomes.Count != 0) ? AssetManager.chromosome_type_library.list.GetRandom() : AssetManager.chromosome_type_library.get(pActorAsset.chromosomes_first.GetRandom()));
			Chromosome chromosome2 = new Chromosome(chromosomeTypeAsset.id, pNew: true);
			addChromosome(chromosome2);
			chromosome = chromosome2;
			chromosome.addGene(geneAsset);
		}
		fillAllEmptyLoci();
		shuffleGenesBetweenChromosomes();
		setDirty();
	}

	public void fillAllEmptyLoci()
	{
		foreach (Chromosome chromosome in chromosomes)
		{
			chromosome.fillEmptyLoci();
		}
	}

	private void shuffleGenesBetweenChromosomes()
	{
		using ListPool<GeneAsset> listPool = new ListPool<GeneAsset>();
		for (int i = 0; i < chromosomes.Count; i++)
		{
			Chromosome chromosome = chromosomes[i];
			for (int j = 0; j < chromosome.genes.Count; j++)
			{
				GeneAsset geneAsset = chromosome.genes[j];
				if (!geneAsset.is_empty)
				{
					chromosome.genes[j] = GeneLibrary.gene_for_generation;
					listPool.Add(geneAsset);
				}
			}
		}
		listPool.Shuffle();
		for (int k = 0; k < chromosomes.Count; k++)
		{
			Chromosome chromosome2 = chromosomes[k];
			for (int l = 0; l < chromosome2.genes.Count; l++)
			{
				if (chromosome2.genes[l].for_generation)
				{
					chromosome2.genes[l] = listPool.Pop();
				}
			}
		}
	}

	private bool isStatGene(string pID)
	{
		switch (pID)
		{
		case "bonus_male":
		case "bonus_female":
		case "bonus_sex_random":
		case "bad":
			return false;
		default:
			return true;
		}
	}

	private ListPool<string> generateGenesFromBaseStats(HashSet<GenomePart> pGeneTemplateStats)
	{
		ListPool<string> listPool = new ListPool<string>();
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		fillRemainingValues(pGeneTemplateStats, dictionary, listPool);
		bool flag = true;
		int num = 300;
		while (flag)
		{
			flag = false;
			if (num-- < 0)
			{
				Debug.LogError((object)"generateGenesFromBaseStats infinite loop");
				break;
			}
			foreach (GenomePart tGenomePart in pGeneTemplateStats)
			{
				if (!isStatGene(tGenomePart.id))
				{
					continue;
				}
				List<GeneAsset> genesWithStat = AssetManager.gene_library.getGenesWithStat(tGenomePart.id);
				if (Randy.randomChance(0.8f))
				{
					genesWithStat.Sort((GeneAsset pG1, GeneAsset pG2) => sortByStatValue(pG1, pG2, tGenomePart.id));
				}
				else
				{
					genesWithStat.Shuffle();
				}
				for (int num2 = 0; num2 < genesWithStat.Count; num2++)
				{
					GeneAsset geneAsset = genesWithStat[num2];
					bool flag2 = true;
					foreach (BaseStatsContainer item in geneAsset.base_stats.getList())
					{
						if (!dictionary.ContainsKey(item.id) || dictionary[item.id] < item.value)
						{
							flag2 = false;
							break;
						}
					}
					foreach (BaseStatsContainer item2 in geneAsset.base_stats_meta.getList())
					{
						if (!dictionary.ContainsKey(item2.id) || dictionary[item2.id] < item2.value)
						{
							flag2 = false;
							break;
						}
					}
					if (!flag2)
					{
						continue;
					}
					foreach (BaseStatsContainer item3 in geneAsset.base_stats.getList())
					{
						dictionary[item3.id] -= item3.value;
					}
					foreach (BaseStatsContainer item4 in geneAsset.base_stats_meta.getList())
					{
						dictionary[item4.id] -= item4.value;
					}
					listPool.Add(geneAsset.id);
					flag = true;
					break;
				}
				if (flag)
				{
					break;
				}
			}
		}
		return listPool;
	}

	private void fillRemainingValues(HashSet<GenomePart> pGeneTemplateStats, Dictionary<string, float> pDictRemainingValues, ListPool<string> pResultGenes)
	{
		foreach (GenomePart pGeneTemplateStat in pGeneTemplateStats)
		{
			string id = pGeneTemplateStat.id;
			if (isStatGene(id))
			{
				pDictRemainingValues.Add(id, pGeneTemplateStat.value);
			}
			else
			{
				addSpecialGenes(id, (int)pGeneTemplateStat.value, pResultGenes);
			}
		}
	}

	private void addSpecialGenes(string pSpecialGeneID, int pAmount, ListPool<string> pResultGenes)
	{
		for (int i = 0; i < pAmount; i++)
		{
			if (pSpecialGeneID == "bonus_sex_random")
			{
				if (Randy.randomBool())
				{
					pResultGenes.Add("bonus_male");
				}
				else
				{
					pResultGenes.Add("bonus_female");
				}
			}
			else
			{
				pResultGenes.Add(pSpecialGeneID);
			}
		}
	}

	private static int sortByStatValue(GeneAsset pA, GeneAsset pB, string pStatID)
	{
		if (!pA.base_stats.hasStat(pStatID) && !pB.base_stats.hasStat(pStatID))
		{
			return 0;
		}
		if (!pA.base_stats.hasStat(pStatID))
		{
			return 1;
		}
		if (!pB.base_stats.hasStat(pStatID))
		{
			return -1;
		}
		return pB.base_stats[pStatID].CompareTo(pA.base_stats[pStatID]);
	}

	public List<ChromosomeData> getListForSave()
	{
		List<ChromosomeData> list = new List<ChromosomeData>();
		foreach (Chromosome chromosome in chromosomes)
		{
			ChromosomeData dataForSave = chromosome.getDataForSave();
			list.Add(dataForSave);
		}
		return list;
	}

	public BaseStats getStats()
	{
		if (_dirty)
		{
			recalculate();
		}
		return _merged_base_stats;
	}

	public BaseStats getStatsMeta()
	{
		if (_dirty)
		{
			recalculate();
		}
		return _merged_base_stats_meta;
	}

	private void recalculate()
	{
		if (!_dirty)
		{
			return;
		}
		_dirty = false;
		BaseStats merged_base_stats = _merged_base_stats;
		BaseStats merged_base_stats_meta = _merged_base_stats_meta;
		BaseStats merged_base_stats_male = _merged_base_stats_male;
		BaseStats merged_base_stats_female = _merged_base_stats_female;
		clearAllBaseStats();
		foreach (Chromosome chromosome in chromosomes)
		{
			chromosome.recalculate();
			merged_base_stats.mergeStats(chromosome.getStats());
			merged_base_stats_meta.mergeStats(chromosome.getStatsMeta());
			merged_base_stats_male.mergeStats(chromosome.getStatsMale());
			merged_base_stats_female.mergeStats(chromosome.getStatsFemale());
		}
		pot_possible_attributes.Clear();
		pot_possible_attributes.AddTimes((int)merged_base_stats["intelligence"], "intelligence");
		pot_possible_attributes.AddTimes((int)merged_base_stats["warfare"], "warfare");
		pot_possible_attributes.AddTimes((int)merged_base_stats["stewardship"], "stewardship");
		pot_possible_attributes.AddTimes((int)merged_base_stats["diplomacy"], "diplomacy");
	}

	public void setDirty()
	{
		_dirty = true;
	}

	public void addChromosome(Chromosome pChromosome)
	{
		chromosomes.Add(pChromosome);
	}

	public void reset()
	{
		setDirty();
		chromosomes.Clear();
	}

	private void clearAllBaseStats()
	{
		BaseStats[] base_stats_all = _base_stats_all;
		for (int i = 0; i < base_stats_all.Length; i++)
		{
			base_stats_all[i].clear();
		}
	}

	public void cloneFrom(Nucleus pParentsSubspeciesNucleus)
	{
		chromosomes.Clear();
		foreach (Chromosome chromosome2 in pParentsSubspeciesNucleus.chromosomes)
		{
			Chromosome chromosome = new Chromosome(chromosome2.chromosome_type, pNew: false);
			chromosome.cloneFrom(chromosome2);
			addChromosome(chromosome);
		}
		setDirty();
	}

	public void doRandomGeneMutations(int pMutationAmount)
	{
		foreach (Chromosome chromosome in chromosomes)
		{
			for (int i = 0; i < pMutationAmount; i++)
			{
				chromosome.mutateRandomGene();
			}
		}
	}

	public void unstableGenomeEvent()
	{
		foreach (Chromosome chromosome in chromosomes)
		{
			chromosome.shuffleGenes();
		}
		setDirty();
	}
}
