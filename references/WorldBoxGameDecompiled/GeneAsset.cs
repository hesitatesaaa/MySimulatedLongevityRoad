using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeneAsset : BaseTrait<GeneAsset>
{
	private const string CHARS_FOR_CODONS = "ACGT";

	public bool is_stat_gene = true;

	public bool can_drop_and_grab = true;

	public bool is_empty;

	public bool for_generation;

	public bool is_bad;

	public bool is_simple;

	public bool is_bonus_male;

	public bool is_bonus_female;

	public bool show_genepool_nucleobases = true;

	public bool synergy_sides_always;

	private string _genetic_code;

	[NonSerialized]
	public char genetic_code_right;

	[NonSerialized]
	public char genetic_code_left;

	[NonSerialized]
	public char genetic_code_up;

	[NonSerialized]
	public char genetic_code_down;

	private string _cached_sequence;

	private string _cached_sequence_locked;

	private BaseStats _cached_half_stats;

	private BaseStats _cached_half_stats_meta;

	protected override HashSet<string> progress_elements => base._progress_data?.unlocked_genes;

	public override string typed_id => "gene";

	public GeneAsset()
	{
		group_id = "genes";
	}

	public override BaseCategoryAsset getGroup()
	{
		return null;
	}

	public string getSequence()
	{
		if (is_bad)
		{
			return getHarmfulSequence();
		}
		if (!show_genepool_nucleobases)
		{
			return getLockedSequence();
		}
		if (isAvailable())
		{
			return getColoredSequence();
		}
		return getLockedSequence();
	}

	private string getHarmfulSequence()
	{
		return InsultStringGenerator.getDNASequenceBad();
	}

	public string getColoredSequence()
	{
		if (string.IsNullOrEmpty(_cached_sequence))
		{
			_cached_sequence = NucleobaseHelper.getColoredSequence(_genetic_code);
		}
		return _cached_sequence;
	}

	public string getLockedSequence()
	{
		return "??? ??? ??? ??? ??? ???";
	}

	public BaseStats getHalfStats()
	{
		BaseStats baseStats = _cached_half_stats;
		if (baseStats == null)
		{
			baseStats = (_cached_half_stats = new BaseStats());
			BaseStatsContainer[] array = base_stats.getList().ToArray();
			foreach (BaseStatsContainer baseStatsContainer in array)
			{
				float num = ((!Mathf.Approximately(Mathf.Floor(baseStatsContainer.value), baseStatsContainer.value)) ? (baseStatsContainer.value * 0.5f) : Mathf.Floor(baseStatsContainer.value * 0.5f));
				baseStats[baseStatsContainer.id] = num;
			}
			baseStats.normalize();
		}
		return baseStats;
	}

	public BaseStats getHalfStatsMeta()
	{
		BaseStats baseStats = _cached_half_stats_meta;
		if (baseStats == null)
		{
			baseStats = (_cached_half_stats_meta = new BaseStats());
			BaseStatsContainer[] array = base_stats_meta.getList().ToArray();
			foreach (BaseStatsContainer baseStatsContainer in array)
			{
				float num = ((!Mathf.Approximately(Mathf.Floor(baseStatsContainer.value), baseStatsContainer.value)) ? (baseStatsContainer.value * 0.5f) : Mathf.Floor(baseStatsContainer.value * 0.5f));
				baseStats[baseStatsContainer.id] = num;
			}
		}
		baseStats.normalize();
		return baseStats;
	}

	public void generateDNA(long pSeed)
	{
		_genetic_code = generateRandomCodonString(pSeed, 15);
		genetic_code_left = _genetic_code[0];
		string genetic_code = _genetic_code;
		genetic_code_right = genetic_code[genetic_code.Length - 1];
		genetic_code_up = _genetic_code[8];
		genetic_code_down = _genetic_code[10];
	}

	private string generateRandomCodonString(long pSeed, int pLength)
	{
		Random random = new Random((int)pSeed);
		string text = "";
		for (int i = 0; i < pLength; i++)
		{
			text += "ACGT"[random.Next("ACGT".Length)];
			if ((i + 1) % 3 == 0 && i + 1 < pLength)
			{
				text += " ";
			}
		}
		return text;
	}

	protected override bool isDebugUnlockedAll()
	{
		return DebugConfig.isOn(DebugOption.UnlockAllGenes);
	}
}
