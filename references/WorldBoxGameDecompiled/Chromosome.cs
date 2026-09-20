using System.Collections.Generic;
using UnityEngine;

public class Chromosome
{
	private const string IMAGE_PATH_NORMAL = "chromosomes/normal/";

	private const string IMAGE_PATH_GOLD = "chromosomes/golden/";

	private const string STRING_UNKOWN = "???????";

	private const string COLOR_BOUND = "#444444";

	private const string COLORED_UNKOWN_TEXT = "<color=#444444>???????</color>";

	public readonly List<GeneAsset> genes = new List<GeneAsset>();

	private readonly BaseStats _merged_base_stats_male = new BaseStats();

	private readonly BaseStats _merged_base_stats_female = new BaseStats();

	private readonly BaseStats _merged_base_stats = new BaseStats();

	private readonly BaseStats _merged_base_stats_meta = new BaseStats();

	private static readonly (int, int)[] DIRECTIONS = new(int, int)[4]
	{
		(0, -1),
		(0, 1),
		(-1, 0),
		(1, 0)
	};

	private bool _dirty = true;

	private Sprite _cached_sprite;

	private int _cached_sprite_index = -1;

	public string chromosome_type;

	private readonly List<int> _loci_amplifiers = new List<int>();

	private readonly List<int> _loci_empty = new List<int>();

	private readonly BaseStats[] _base_stats_all = new BaseStats[4];

	private readonly int _columns;

	public Chromosome(string pType, bool pNew)
	{
		chromosome_type = pType;
		if (pNew)
		{
			int amount_loci = getAsset().amount_loci;
			GeneAsset item = AssetManager.gene_library.get("empty");
			for (int i = 0; i < amount_loci; i++)
			{
				genes.Add(item);
			}
			generateAmplifiers(pType);
		}
		_base_stats_all[0] = _merged_base_stats;
		_base_stats_all[1] = _merged_base_stats_meta;
		_base_stats_all[2] = _merged_base_stats_male;
		_base_stats_all[3] = _merged_base_stats_female;
		_columns = getAsset().amount_loci / 6;
	}

	public bool isLocusAmplifier(int pX, int pY)
	{
		int indexFrom = getIndexFrom(pX, pY);
		return isLocusAmplifier(indexFrom);
	}

	public bool isLocusAmplifier(int pLocusIndex)
	{
		return _loci_amplifiers.Contains(pLocusIndex);
	}

	public bool isVoidLocus(int pLocusIndex)
	{
		return _loci_empty.Contains(pLocusIndex);
	}

	public bool isSpecialLocusAt(int pX, int pY)
	{
		int indexFrom = getIndexFrom(pX, pY);
		return isSpecialLocus(indexFrom);
	}

	public bool isVoidLocusAt(int pX, int pY)
	{
		int indexFrom = getIndexFrom(pX, pY);
		return isVoidLocus(indexFrom);
	}

	public bool isAllSidesVoidLocus(int pX, int pY)
	{
		int num = countBounds(pX, pY);
		int num2 = 0;
		if (isVoidLocusAt(pX - 1, pY))
		{
			num2++;
		}
		if (isVoidLocusAt(pX + 1, pY))
		{
			num2++;
		}
		if (isVoidLocusAt(pX, pY + 1))
		{
			num2++;
		}
		if (isVoidLocusAt(pX, pY - 1))
		{
			num2++;
		}
		return num2 == num;
	}

	private bool isAmplifierLocusAt(int pX, int pY)
	{
		int indexFrom = getIndexFrom(pX, pY);
		return isLocusAmplifier(indexFrom);
	}

	private bool isForcedSynergyAt(int pX, int pY)
	{
		if (isAmplifierLocusAt(pX, pY))
		{
			return true;
		}
		if (getGeneAt(pX, pY).synergy_sides_always)
		{
			return true;
		}
		return false;
	}

	public bool isForcedSynergyLeft(int pX, int pY)
	{
		if (hasBoundLeft(pX, pY))
		{
			return false;
		}
		var (num, num2) = getDirectionOffset(GeneDirection.Left);
		return isForcedSynergyAt(pX + num, pY + num2);
	}

	public bool isForcedSynergyRight(int pX, int pY)
	{
		if (hasBoundRight(pX, pY))
		{
			return false;
		}
		var (num, num2) = getDirectionOffset(GeneDirection.Right);
		return isForcedSynergyAt(pX + num, pY + num2);
	}

	public bool isForcedSynergyUp(int pX, int pY)
	{
		if (hasBoundUp(pX, pY))
		{
			return false;
		}
		var (num, num2) = getDirectionOffset(GeneDirection.Up);
		return isForcedSynergyAt(pX + num, pY + num2);
	}

	public bool isForcedSynergyDown(int pX, int pY)
	{
		if (hasBoundDown(pX, pY))
		{
			return false;
		}
		var (num, num2) = getDirectionOffset(GeneDirection.Down);
		return isForcedSynergyAt(pX + num, pY + num2);
	}

	public LocusType getLocusType(int pLocusIndex)
	{
		if (isLocusAmplifier(pLocusIndex))
		{
			return LocusType.Amplifier;
		}
		if (isVoidLocus(pLocusIndex))
		{
			return LocusType.Empty;
		}
		return LocusType.Standard;
	}

	public void fillStatsForTooltip(LocusElement pLocus, BaseStats pStatsContainer)
	{
		int locus_index = pLocus.locus_index;
		if (!isVoidLocus(locus_index))
		{
			GeneAsset geneAsset = pLocus.getGeneAsset();
			if (geneAsset.is_bonus_male)
			{
				combineBonusesForSides(locus_index, pStatsContainer);
			}
			else if (geneAsset.is_bonus_female)
			{
				combineBonusesForSides(locus_index, pStatsContainer);
			}
			else
			{
				getBonusesFromGene(pLocus.locus_index, pStatsContainer, null, pCombineMeta: true);
			}
			pStatsContainer.normalize();
		}
	}

	private void generateAmplifiers(string pType)
	{
		ChromosomeTypeAsset chromosomeTypeAsset = AssetManager.chromosome_type_library.get(pType);
		using ListPool<int> listPool = new ListPool<int>();
		for (int i = 0; i < chromosomeTypeAsset.amount_loci; i++)
		{
			listPool.Add(i);
		}
		listPool.Shuffle();
		int num = Randy.randomInt(chromosomeTypeAsset.amount_loci_min_amplifier, chromosomeTypeAsset.amount_loci_max_amplifier);
		int num2 = Randy.randomInt(chromosomeTypeAsset.amount_loci_min_empty, chromosomeTypeAsset.amount_loci_max_empty);
		for (int j = 0; j < num; j++)
		{
			_loci_amplifiers.Add(listPool.Pop());
		}
		for (int k = 0; k < num2; k++)
		{
			_loci_empty.Add(listPool.Pop());
		}
	}

	public bool canAddGene(GeneAsset pAsset)
	{
		if (countEmpty() == 0)
		{
			return false;
		}
		return true;
	}

	public void setGene(GeneAsset pAsset, int pIndex)
	{
		genes[pIndex] = pAsset;
	}

	public GeneAsset getGene(int pIndex)
	{
		return genes[pIndex];
	}

	public ChromosomeTypeAsset getAsset()
	{
		return AssetManager.chromosome_type_library.get(chromosome_type);
	}

	public void load(ChromosomeData pData)
	{
		chromosome_type = pData.chromosome_type;
		foreach (string item in pData.loci)
		{
			GeneAsset geneAsset = AssetManager.gene_library.get(item);
			if (geneAsset != null)
			{
				genes.Add(geneAsset);
			}
		}
		_loci_amplifiers.AddRange(pData.super_loci);
		_loci_empty.AddRange(pData.void_loci);
	}

	public ChromosomeData getDataForSave()
	{
		ChromosomeData chromosomeData = new ChromosomeData();
		foreach (GeneAsset gene in genes)
		{
			chromosomeData.loci.Add(gene.id);
		}
		chromosomeData.super_loci.AddRange(_loci_amplifiers);
		chromosomeData.void_loci.AddRange(_loci_empty);
		chromosomeData.chromosome_type = chromosome_type;
		return chromosomeData;
	}

	public void addGene(GeneAsset pGeneAsset)
	{
		for (int i = 0; i < genes.Count; i++)
		{
			if (genes[i].is_empty && canAddToLocus(i))
			{
				genes[i] = pGeneAsset;
				break;
			}
		}
		setDirty();
	}

	public bool isSpecialLocus(int pIndex)
	{
		if (!_loci_amplifiers.Contains(pIndex))
		{
			return _loci_empty.Contains(pIndex);
		}
		return true;
	}

	public bool canAddToLocus(int pIndex)
	{
		if (_loci_amplifiers.Contains(pIndex))
		{
			return false;
		}
		if (_loci_empty.Contains(pIndex))
		{
			return false;
		}
		return true;
	}

	public int countNonEmpty()
	{
		int num = 0;
		for (int i = 0; i < genes.Count; i++)
		{
			if (!genes[i].is_empty && canAddToLocus(i))
			{
				num++;
			}
		}
		return num;
	}

	public int countEmpty()
	{
		int num = 0;
		for (int i = 0; i < genes.Count; i++)
		{
			if (genes[i].is_empty && canAddToLocus(i))
			{
				num++;
			}
		}
		return num;
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

	public BaseStats getStatsMale()
	{
		if (_dirty)
		{
			recalculate();
		}
		return _merged_base_stats_male;
	}

	public BaseStats getStatsFemale()
	{
		if (_dirty)
		{
			recalculate();
		}
		return _merged_base_stats_female;
	}

	public void setDirty()
	{
		_dirty = true;
	}

	public void recalculate()
	{
		if (!_dirty)
		{
			return;
		}
		_dirty = false;
		clearAllBaseStats();
		BaseStats merged_base_stats = _merged_base_stats;
		BaseStats merged_base_stats_meta = _merged_base_stats_meta;
		BaseStats merged_base_stats_male = _merged_base_stats_male;
		BaseStats merged_base_stats_female = _merged_base_stats_female;
		for (int i = 0; i < genes.Count; i++)
		{
			if (!isVoidLocus(i))
			{
				getBonusesFromGene(i, merged_base_stats, merged_base_stats_meta);
			}
		}
		for (int j = 0; j < genes.Count; j++)
		{
			GeneAsset geneAsset = genes[j];
			if (!isVoidLocus(j))
			{
				if (geneAsset.is_bonus_male)
				{
					combineBonusesForSides(j, merged_base_stats_male);
				}
				if (geneAsset.is_bonus_female)
				{
					combineBonusesForSides(j, merged_base_stats_female);
				}
			}
		}
	}

	private void combineBonusesForSides(int pLocusIndex, BaseStats pBaseStatsMain)
	{
		(int, int) xYFromIndex = getXYFromIndex(pLocusIndex);
		int item = xYFromIndex.Item1;
		int item2 = xYFromIndex.Item2;
		bool num = isNextToBad(pLocusIndex);
		getBonusesFromGene(item, item2 + 1, pBaseStatsMain);
		getBonusesFromGene(item, item2 - 1, pBaseStatsMain);
		getBonusesFromGene(item - 1, item2, pBaseStatsMain);
		getBonusesFromGene(item + 1, item2, pBaseStatsMain);
		if (num)
		{
			BaseStatsContainer[] array = pBaseStatsMain.getList().ToArray();
			foreach (BaseStatsContainer baseStatsContainer in array)
			{
				float value = ((!Mathf.Approximately(Mathf.Floor(baseStatsContainer.value), baseStatsContainer.value)) ? (baseStatsContainer.value * 0.5f) : Mathf.Floor(baseStatsContainer.value * 0.5f));
				pBaseStatsMain[baseStatsContainer.id] = value;
			}
			pBaseStatsMain.normalize();
		}
	}

	private void getBonusesFromGene(int pX, int pY, BaseStats pBaseStatsMain, BaseStats pBaseStatsMeta = null, bool pCombineMeta = false)
	{
		if (getGeneAt(pX, pY) != null)
		{
			int indexFrom = getIndexFrom(pX, pY);
			getBonusesFromGene(indexFrom, pBaseStatsMain, pBaseStatsMeta, pCombineMeta);
		}
	}

	private void getBonusesFromGene(int pLocusIndex, BaseStats pBaseStatsMain, BaseStats pBaseStatsMeta = null, bool pCombineMeta = false)
	{
		GeneAsset geneAsset = genes[pLocusIndex];
		bool flag = hasFullSynergy(pLocusIndex);
		bool num = isNextToBad(pLocusIndex);
		if (num)
		{
			flag = false;
		}
		if (num)
		{
			pBaseStatsMain.mergeStats(geneAsset.getHalfStats());
			if (pCombineMeta)
			{
				pBaseStatsMain.mergeStats(geneAsset.getHalfStatsMeta());
			}
			else
			{
				pBaseStatsMeta?.mergeStats(geneAsset.getHalfStatsMeta());
			}
			return;
		}
		pBaseStatsMain.mergeStats(geneAsset.base_stats);
		if (pCombineMeta)
		{
			pBaseStatsMain.mergeStats(geneAsset.base_stats_meta);
		}
		pBaseStatsMeta?.mergeStats(geneAsset.base_stats_meta);
		if (flag && !geneAsset.synergy_sides_always)
		{
			pBaseStatsMain.mergeStats(geneAsset.base_stats);
			if (pCombineMeta)
			{
				pBaseStatsMain.mergeStats(geneAsset.base_stats_meta);
			}
			else
			{
				pBaseStatsMeta?.mergeStats(geneAsset.base_stats_meta);
			}
		}
	}

	private void clearAllBaseStats()
	{
		BaseStats[] base_stats_all = _base_stats_all;
		for (int i = 0; i < base_stats_all.Length; i++)
		{
			base_stats_all[i].clear();
		}
	}

	public bool hasFullSynergyAt(int pX, int pY)
	{
		int num = 0;
		if (isAllSidesVoidLocus(pX, pY))
		{
			return false;
		}
		if (isNextToBad(pX, pY))
		{
			return false;
		}
		if (isNextToBadAmplifier(pX, pY))
		{
			return false;
		}
		if (hasSynergyConnectionLeft(pX, pY))
		{
			num++;
		}
		if (hasSynergyConnectionRight(pX, pY))
		{
			num++;
		}
		if (hasSynergyConnectionUp(pX, pY))
		{
			num++;
		}
		if (hasSynergyConnectionDown(pX, pY))
		{
			num++;
		}
		int num2 = 0;
		if (!hasBoundLeft(pX, pY))
		{
			num2++;
		}
		if (!hasBoundRight(pX, pY))
		{
			num2++;
		}
		if (!hasBoundUp(pX, pY))
		{
			num2++;
		}
		if (!hasBoundDown(pX, pY))
		{
			num2++;
		}
		return num == num2;
	}

	public bool hasFullSynergy(int pLocusIndex)
	{
		var (pX, pY) = getXYFromIndex(pLocusIndex);
		return hasFullSynergyAt(pX, pY);
	}

	public bool hasAnySynergy(int pLocusIndex)
	{
		var (pFromX, pFromY) = getXYFromIndex(pLocusIndex);
		if (hasSynergyConnectionLeft(pFromX, pFromY))
		{
			return true;
		}
		if (hasSynergyConnectionRight(pFromX, pFromY))
		{
			return true;
		}
		if (hasSynergyConnectionUp(pFromX, pFromY))
		{
			return true;
		}
		if (hasSynergyConnectionDown(pFromX, pFromY))
		{
			return true;
		}
		return false;
	}

	public string getSynergyTooltipText(int pLocusIndex)
	{
		(int, int) xYFromIndex = getXYFromIndex(pLocusIndex);
		int item = xYFromIndex.Item1;
		int item2 = xYFromIndex.Item2;
		GeneAsset geneAt = getGeneAt(item, item2);
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		bool flag = isBadAt(item, item2);
		if (hasAnySynergy(pLocusIndex) && !flag)
		{
			stringBuilderPool.Append(Toolbox.coloredString(LocalizedTextManager.getText("sequence_synergy"), "#FFFFAA"));
		}
		else
		{
			stringBuilderPool.Append(LocalizedTextManager.getText("sequence_synergy"));
		}
		stringBuilderPool.Append("\n");
		bool num = hasSynergyConnectionLeft(item, item2);
		bool flag2 = hasSynergyConnectionRight(item, item2);
		bool num2 = hasSynergyConnectionUp(item, item2);
		bool flag3 = hasSynergyConnectionDown(item, item2);
		GeneAsset geneLeft = getGeneLeft(item, item2);
		GeneAsset geneRight = getGeneRight(item, item2);
		GeneAsset geneUp = getGeneUp(item, item2);
		GeneAsset geneDown = getGeneDown(item, item2);
		bool flag4 = isForcedSynergyAt(item, item2);
		if (num2)
		{
			if (flag || isBadAt(item, item2 - 1) || hasAmplifierBad(item, item2 - 1))
			{
				stringBuilderPool.Append(getBadConnectionString());
			}
			else if (flag4)
			{
				stringBuilderPool.Append(NucleobaseHelper.getColoredNucleobaseFull(geneUp.genetic_code_down));
			}
			else
			{
				stringBuilderPool.Append(NucleobaseHelper.getColoredNucleobaseFull(geneAt.genetic_code_up));
			}
		}
		else if (hasBoundUp(item, item2) || isConnectionDeniedUp(item, item2))
		{
			stringBuilderPool.Append("<color=#444444>???????</color>");
		}
		else
		{
			stringBuilderPool.Append(getNotConnectedText(geneAt.genetic_code_up, World.world.getCurSessionTime()));
		}
		stringBuilderPool.Append("\n");
		if (num)
		{
			if (flag || isBadAt(item - 1, item2) || hasAmplifierBad(item - 1, item2))
			{
				stringBuilderPool.Append(getBadConnectionString());
			}
			else if (flag4)
			{
				stringBuilderPool.Append(NucleobaseHelper.getColoredNucleobaseFull(geneLeft.genetic_code_right));
			}
			else
			{
				stringBuilderPool.Append(NucleobaseHelper.getColoredNucleobaseFull(geneAt.genetic_code_left));
			}
		}
		else if (hasBoundLeft(item, item2) || isConnectionDeniedLeft(item, item2))
		{
			stringBuilderPool.Append("<color=#444444>???????</color>");
		}
		else
		{
			stringBuilderPool.Append(getNotConnectedText(geneAt.genetic_code_left, World.world.getCurSessionTime()));
		}
		stringBuilderPool.Append(" ... ");
		if (flag2)
		{
			if (flag || isBadAt(item + 1, item2) || hasAmplifierBad(item + 1, item2))
			{
				stringBuilderPool.Append(getBadConnectionString());
			}
			else if (flag4)
			{
				stringBuilderPool.Append(NucleobaseHelper.getColoredNucleobaseFull(geneRight.genetic_code_left));
			}
			else
			{
				stringBuilderPool.Append(NucleobaseHelper.getColoredNucleobaseFull(geneAt.genetic_code_right));
			}
		}
		else if (hasBoundRight(item, item2) || isConnectionDeniedRight(item, item2))
		{
			stringBuilderPool.Append("<color=#444444>???????</color>");
		}
		else
		{
			stringBuilderPool.Append(getNotConnectedText(geneAt.genetic_code_right, World.world.getCurSessionTime()));
		}
		stringBuilderPool.Append("\n");
		if (flag3)
		{
			if (flag || isBadAt(item, item2 + 1) || hasAmplifierBad(item, item2 + 1))
			{
				stringBuilderPool.Append(getBadConnectionString());
			}
			else if (flag4)
			{
				stringBuilderPool.Append(NucleobaseHelper.getColoredNucleobaseFull(geneDown.genetic_code_up));
			}
			else
			{
				stringBuilderPool.Append(NucleobaseHelper.getColoredNucleobaseFull(geneAt.genetic_code_down));
			}
		}
		else if (hasBoundDown(item, item2) || isConnectionDeniedDown(item, item2))
		{
			stringBuilderPool.Append("<color=#444444>???????</color>");
		}
		else
		{
			stringBuilderPool.Append(getNotConnectedText(geneAt.genetic_code_down, World.world.getCurSessionTime()));
		}
		stringBuilderPool.Append("\n");
		stringBuilderPool.Append("\n");
		return stringBuilderPool.ToString();
	}

	private string getBadConnectionString()
	{
		return InsultStringGenerator.getBadConnectionString();
	}

	private string getNotConnectedText(char pChar, double pTime)
	{
		string fullNucleobaseName = NucleobaseHelper.getFullNucleobaseName(pChar);
		string colorHex = NucleobaseHelper.getColorHex(pChar, pDark: true);
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		for (int i = 0; i < fullNucleobaseName.Length; i++)
		{
			stringBuilderPool.Append(fullNucleobaseName[i]);
		}
		int num = pChar * 100;
		int index = (int)((pTime + (double)num) * 8.0 % (double)fullNucleobaseName.Length);
		stringBuilderPool[index] = '?';
		return Toolbox.coloredString(stringBuilderPool.ToString(), colorHex);
	}

	private int getIndexFrom(int pX, int pY)
	{
		return pX + pY * 6;
	}

	public (int, int) getXYFromIndex(int pIndex)
	{
		int item = pIndex % 6;
		int item2 = pIndex / 6;
		return (item, item2);
	}

	public Sprite getSpriteNormal()
	{
		Sprite[] spriteList = SpriteTextureLoader.getSpriteList("chromosomes/normal/");
		if (_cached_sprite_index == -1)
		{
			_cached_sprite_index = Randy.randomInt(0, spriteList.Length - 1);
		}
		_cached_sprite = spriteList[_cached_sprite_index];
		return _cached_sprite;
	}

	public Sprite getSpriteGolden()
	{
		Sprite[] spriteList = SpriteTextureLoader.getSpriteList("chromosomes/golden/");
		if (_cached_sprite_index == -1)
		{
			_cached_sprite_index = Randy.randomInt(0, spriteList.Length - 1);
		}
		_cached_sprite = spriteList[_cached_sprite_index];
		return _cached_sprite;
	}

	public void cloneFrom(Chromosome pParentChromosome)
	{
		genes.AddRange(pParentChromosome.genes);
		_loci_empty.AddRange(pParentChromosome._loci_empty);
		_loci_amplifiers.AddRange(pParentChromosome._loci_amplifiers);
		setDirty();
	}

	public void mutateRandomGene()
	{
		using ListPool<int> listPool = new ListPool<int>();
		for (int i = 0; i < genes.Count; i++)
		{
			if (!isSpecialLocus(i))
			{
				listPool.Add(i);
			}
		}
		int random = listPool.GetRandom();
		GeneAsset randomGeneForMutation = AssetManager.gene_library.getRandomGeneForMutation();
		setGene(randomGeneForMutation, random);
		setDirty();
	}

	public bool hasGene(GeneAsset pAsset)
	{
		return genes?.Contains(pAsset) ?? false;
	}

	public GeneAsset getGeneAtDirectionFrom(int pFromX, int pFromY, GeneDirection pDirection)
	{
		(int, int) directionOffset = getDirectionOffset(pDirection);
		int item = directionOffset.Item1;
		int item2 = directionOffset.Item2;
		int pX = pFromX + item;
		int pY = pFromY + item2;
		if (!isCoordinatesValid(pX, pY))
		{
			return null;
		}
		int indexFrom = getIndexFrom(pFromX + item, pFromY + item2);
		return genes[indexFrom];
	}

	public GeneAsset getGeneAt(int pFromX, int pFromY)
	{
		if (!isCoordinatesValid(pFromX, pFromY))
		{
			return null;
		}
		int indexFrom = getIndexFrom(pFromX, pFromY);
		if (!isIndexValid(indexFrom))
		{
			return null;
		}
		return genes[indexFrom];
	}

	public GeneAsset getGeneLeft(int pFromX, int pFromY)
	{
		return getGeneAtDirectionFrom(pFromX, pFromY, GeneDirection.Left);
	}

	public GeneAsset getGeneRight(int pFromX, int pFromY)
	{
		return getGeneAtDirectionFrom(pFromX, pFromY, GeneDirection.Right);
	}

	public GeneAsset getGeneUp(int pFromX, int pFromY)
	{
		return getGeneAtDirectionFrom(pFromX, pFromY, GeneDirection.Up);
	}

	public GeneAsset getGeneDown(int pFromX, int pFromY)
	{
		return getGeneAtDirectionFrom(pFromX, pFromY, GeneDirection.Down);
	}

	private bool isIndexValid(int pIndex)
	{
		if (pIndex < 0)
		{
			return false;
		}
		if (pIndex >= genes.Count)
		{
			return false;
		}
		return true;
	}

	private bool isCoordinatesValid(int pX, int pY)
	{
		if (pX < 0)
		{
			return false;
		}
		if (pY < 0)
		{
			return false;
		}
		if (pX >= 6)
		{
			return false;
		}
		if (pY >= _columns)
		{
			return false;
		}
		return true;
	}

	public (int, int) getDirectionOffset(GeneDirection pDirection)
	{
		return pDirection switch
		{
			GeneDirection.Up => DIRECTIONS[0], 
			GeneDirection.Down => DIRECTIONS[1], 
			GeneDirection.Left => DIRECTIONS[2], 
			GeneDirection.Right => DIRECTIONS[3], 
			_ => (0, 0), 
		};
	}

	public bool canBeConnectedTo(int pFromX, int pFromY, int pToX, int pToY)
	{
		GeneAsset geneAt = getGeneAt(pFromX, pFromY);
		GeneAsset geneAt2 = getGeneAt(pToX, pToY);
		if (geneAt == null || geneAt2 == null)
		{
			return false;
		}
		if (!geneAt.is_empty)
		{
			_ = geneAt2.is_empty;
		}
		return false;
	}

	public int countBounds(int pX, int pY)
	{
		int num = 0;
		if (isCoordinatesValid(pX - 1, pY))
		{
			num++;
		}
		if (isCoordinatesValid(pX + 1, pY))
		{
			num++;
		}
		if (isCoordinatesValid(pX, pY - 1))
		{
			num++;
		}
		if (isCoordinatesValid(pX, pY + 1))
		{
			num++;
		}
		return num;
	}

	public bool hasSynergyConnectionLeft(int pFromX, int pFromY)
	{
		if (hasBoundLeft(pFromX, pFromY))
		{
			return false;
		}
		return hasSynergyConnection(pFromX, pFromY, GeneDirection.Left);
	}

	public bool hasSynergyConnectionRight(int pFromX, int pFromY)
	{
		if (hasBoundRight(pFromX, pFromY))
		{
			return false;
		}
		return hasSynergyConnection(pFromX, pFromY, GeneDirection.Right);
	}

	public bool hasSynergyConnectionUp(int pFromX, int pFromY)
	{
		if (hasBoundUp(pFromX, pFromY))
		{
			return false;
		}
		return hasSynergyConnection(pFromX, pFromY, GeneDirection.Up);
	}

	public bool hasSynergyConnectionDown(int pFromX, int pFromY)
	{
		if (hasBoundDown(pFromX, pFromY))
		{
			return false;
		}
		return hasSynergyConnection(pFromX, pFromY, GeneDirection.Down);
	}

	public bool isAllLociSynergy()
	{
		int num = -1;
		foreach (GeneAsset gene in genes)
		{
			num++;
			if (!gene.is_empty && !gene.synergy_sides_always)
			{
				if (gene.is_bad)
				{
					return false;
				}
				var (pFromX, pFromY) = getXYFromIndex(num);
				if (!hasAllSynergiesAt(pFromX, pFromY, pCheckBounds: false))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool hasAllSynergiesAt(int pFromX, int pFromY, bool pCheckBounds = true)
	{
		if (isAllSidesVoidLocus(pFromX, pFromY))
		{
			return false;
		}
		bool num = (pCheckBounds ? hasSynergyConnectionLeft(pFromX, pFromY) : (hasBoundLeft(pFromX, pFromY) || hasSynergyConnection(pFromX, pFromY, GeneDirection.Left)));
		bool flag = (pCheckBounds ? hasSynergyConnectionRight(pFromX, pFromY) : (hasBoundRight(pFromX, pFromY) || hasSynergyConnection(pFromX, pFromY, GeneDirection.Right)));
		bool flag2 = (pCheckBounds ? hasSynergyConnectionUp(pFromX, pFromY) : (hasBoundUp(pFromX, pFromY) || hasSynergyConnection(pFromX, pFromY, GeneDirection.Up)));
		bool flag3 = (pCheckBounds ? hasSynergyConnectionDown(pFromX, pFromY) : (hasBoundDown(pFromX, pFromY) || hasSynergyConnection(pFromX, pFromY, GeneDirection.Down)));
		return num & flag & flag2 & flag3;
	}

	public bool hasSynergyConnection(int pFromX, int pFromY, GeneDirection pDirection)
	{
		GeneAsset geneAt = getGeneAt(pFromX, pFromY);
		bool flag = isAmplifierLocusAt(pFromX, pFromY);
		bool flag2 = false;
		if (geneAt.synergy_sides_always)
		{
			flag = true;
		}
		if (geneAt.is_bad)
		{
			flag2 = true;
		}
		if (!flag && geneAt.is_empty)
		{
			return false;
		}
		(int, int) directionOffset = getDirectionOffset(pDirection);
		int item = directionOffset.Item1;
		int item2 = directionOffset.Item2;
		GeneAsset geneAt2 = getGeneAt(pFromX + item, pFromY + item2);
		bool flag3 = isAmplifierLocusAt(pFromX + item, pFromY + item2);
		if (geneAt2 != null && geneAt2.synergy_sides_always)
		{
			flag3 = true;
		}
		if (geneAt2 != null && geneAt2.is_bad)
		{
			flag2 = true;
		}
		if (!flag3)
		{
			if (geneAt2 == null)
			{
				return false;
			}
			if (geneAt2.is_empty)
			{
				return false;
			}
		}
		if (!flag2 & flag & flag3)
		{
			return false;
		}
		switch (pDirection)
		{
		case GeneDirection.Up:
			if (flag | flag3)
			{
				return true;
			}
			if (geneAt.genetic_code_up == geneAt2.genetic_code_down)
			{
				return true;
			}
			break;
		case GeneDirection.Down:
			if (flag | flag3)
			{
				return true;
			}
			if (geneAt.genetic_code_down == geneAt2.genetic_code_up)
			{
				return true;
			}
			break;
		case GeneDirection.Left:
			if (flag | flag3)
			{
				return true;
			}
			if (geneAt.genetic_code_left == geneAt2.genetic_code_right)
			{
				return true;
			}
			break;
		case GeneDirection.Right:
			if (flag | flag3)
			{
				return true;
			}
			if (geneAt.genetic_code_right == geneAt2.genetic_code_left)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public bool isConnectionDeniedUp(int pFromX, int pFromY)
	{
		if (hasBoundAt(pFromX, pFromY - 1))
		{
			return true;
		}
		if (isForcedSynergyUp(pFromX, pFromY) && isForcedSynergyAt(pFromX, pFromY))
		{
			return true;
		}
		return false;
	}

	public bool isConnectionDeniedDown(int pFromX, int pFromY)
	{
		if (hasBoundAt(pFromX, pFromY + 1))
		{
			return true;
		}
		if (isForcedSynergyDown(pFromX, pFromY) && isForcedSynergyAt(pFromX, pFromY))
		{
			return true;
		}
		return false;
	}

	public bool isConnectionDeniedLeft(int pFromX, int pFromY)
	{
		if (hasBoundAt(pFromX - 1, pFromY))
		{
			return true;
		}
		if (isForcedSynergyLeft(pFromX, pFromY) && isForcedSynergyAt(pFromX, pFromY))
		{
			return true;
		}
		return false;
	}

	public bool isConnectionDeniedRight(int pFromX, int pFromY)
	{
		if (hasBoundAt(pFromX + 1, pFromY))
		{
			return true;
		}
		if (isForcedSynergyRight(pFromX, pFromY) && isForcedSynergyAt(pFromX, pFromY))
		{
			return true;
		}
		return false;
	}

	public bool hasBoundAt(int pX, int pY)
	{
		if (!isCoordinatesValid(pX, pY))
		{
			return true;
		}
		if (isVoidLocusAt(pX, pY))
		{
			return true;
		}
		return false;
	}

	public bool hasBoundLeft(int pX, int pY)
	{
		return hasBoundAt(pX - 1, pY);
	}

	public bool hasBoundRight(int pX, int pY)
	{
		return hasBoundAt(pX + 1, pY);
	}

	public bool hasBoundUp(int pX, int pY)
	{
		return hasBoundAt(pX, pY - 1);
	}

	public bool hasBoundDown(int pX, int pY)
	{
		return hasBoundAt(pX, pY + 1);
	}

	public void fillEmptyLoci()
	{
		for (int i = 0; i < genes.Count; i++)
		{
			GeneAsset geneAsset = genes[i];
			if (!isSpecialLocus(i) && geneAsset.is_empty)
			{
				GeneAsset randomSimpleGene = AssetManager.gene_library.getRandomSimpleGene();
				setGene(randomSimpleGene, i);
			}
		}
	}

	public bool isNextToBad(int pLocusIndex)
	{
		var (pX, pY) = getXYFromIndex(pLocusIndex);
		return isNextToBad(pX, pY);
	}

	public bool isNextToBad(int pX, int pY)
	{
		(int, int)[] dIRECTIONS = DIRECTIONS;
		for (int i = 0; i < dIRECTIONS.Length; i++)
		{
			var (num, num2) = dIRECTIONS[i];
			if (isBadAt(pX + num, pY + num2))
			{
				return true;
			}
		}
		return false;
	}

	public bool hasGenesAround(int pIndex)
	{
		var (pX, pY) = getXYFromIndex(pIndex);
		return hasGenesAround(pX, pY);
	}

	public bool hasGenesAround(int pX, int pY)
	{
		(int, int)[] dIRECTIONS = DIRECTIONS;
		for (int i = 0; i < dIRECTIONS.Length; i++)
		{
			(int, int) tuple = dIRECTIONS[i];
			int item = tuple.Item1;
			int item2 = tuple.Item2;
			int num = pX + item;
			int num2 = pY + item2;
			if (isAmplifierLocusAt(num, num2))
			{
				return true;
			}
			GeneAsset geneAt = getGeneAt(num, num2);
			if (geneAt != null && !geneAt.is_empty)
			{
				return true;
			}
		}
		return false;
	}

	public bool isNextToBadAmplifier(int pX, int pY)
	{
		(int, int)[] dIRECTIONS = DIRECTIONS;
		for (int i = 0; i < dIRECTIONS.Length; i++)
		{
			var (num, num2) = dIRECTIONS[i];
			if (hasAmplifierBad(pX + num, pY + num2))
			{
				return true;
			}
		}
		return false;
	}

	public bool isBadAt(int pX, int pY)
	{
		if (isVoidLocusAt(pX, pY))
		{
			return false;
		}
		GeneAsset geneAt = getGeneAt(pX, pY);
		if (geneAt == null)
		{
			return false;
		}
		if (geneAt.is_bad)
		{
			return true;
		}
		return false;
	}

	public bool hasAmplifierBad(int pX, int pY)
	{
		if (!isLocusAmplifier(pX, pY))
		{
			return false;
		}
		if (isNextToBad(pX, pY))
		{
			return true;
		}
		return false;
	}

	public void shuffleGenes()
	{
		GeneAsset gene_for_generation = GeneLibrary.gene_for_generation;
		using ListPool<GeneAsset> listPool = new ListPool<GeneAsset>();
		for (int i = 0; i < genes.Count; i++)
		{
			GeneAsset geneAsset = genes[i];
			if (!geneAsset.is_empty)
			{
				listPool.Add(geneAsset);
				genes[i] = gene_for_generation;
			}
		}
		listPool.Shuffle();
		for (int j = 0; j < genes.Count; j++)
		{
			if (listPool.Count == 0)
			{
				break;
			}
			if (genes[j].for_generation)
			{
				genes[j] = listPool.Pop();
			}
		}
		setDirty();
	}
}
