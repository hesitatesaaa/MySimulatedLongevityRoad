using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class OnomasticsData : IDisposable
{
	public const char SEPARATOR_GROUPS = '|';

	public const char SEPARATOR_PARTS = ';';

	public const char SEPARATOR_PART_CONTAINER = ':';

	public const int MAX_NAME_LENGTH = 30;

	public Dictionary<string, OnomasticsDataGroup> groups = new Dictionary<string, OnomasticsDataGroup>();

	private List<string> _template_data = new List<string>();

	private List<string> _current_subgroup = new List<string>();

	public const string DEFAULT_NAME_FAILED = "Rebr";

	public void setTemplateData(IReadOnlyList<string> pTemplateData)
	{
		_template_data.Clear();
		_template_data.AddRange(pTemplateData);
	}

	public List<string> getFullTemplateData()
	{
		return _template_data;
	}

	public List<string> getCurrentSubgroup()
	{
		return _current_subgroup;
	}

	public void addToTemplateData(string pID)
	{
		_template_data.Add(pID);
	}

	public void shuffleAllCards()
	{
		_template_data.Shuffle();
	}

	public void clearTemplateData()
	{
		_template_data.Clear();
	}

	public string getGroupString(string pGroupID)
	{
		if (tryGetGroup(pGroupID, out var pGroup))
		{
			return pGroup.characters_string;
		}
		return null;
	}

	public bool isGroupEmpty(string pGroupID)
	{
		OnomasticsDataGroup pGroup = getGroup(pGroupID);
		return isGroupEmpty(pGroup);
	}

	public bool isGroupEmpty(OnomasticsDataGroup pGroup)
	{
		if (pGroup == null || pGroup.isEmpty())
		{
			return true;
		}
		return false;
	}

	public OnomasticsDataGroup getGroup(string pGroupID)
	{
		groups.TryGetValue(pGroupID, out var value);
		return value;
	}

	public bool tryGetGroup(string pGroupID, out OnomasticsDataGroup pGroup)
	{
		if (groups.TryGetValue(pGroupID, out pGroup))
		{
			return !pGroup.isEmpty();
		}
		return false;
	}

	public string getRandomPartFromGroup(string pGroupID)
	{
		if (tryGetGroup(pGroupID, out var pGroup))
		{
			return pGroup.characters.GetRandom();
		}
		return string.Empty;
	}

	private bool saveGroup(string pGroupID, string pCharacters)
	{
		bool result = true;
		if (!groups.TryGetValue(pGroupID, out var value))
		{
			value = new OnomasticsDataGroup();
			groups.Add(pGroupID, value);
			result = false;
		}
		string characters_string = value.characters_string;
		pCharacters = pCharacters.Replace("\n", " ");
		pCharacters = pCharacters.Replace("\r", " ");
		while (pCharacters.Contains("  "))
		{
			pCharacters = pCharacters.Replace("  ", " ");
		}
		pCharacters = pCharacters.Trim('\n', '\r', ' ');
		if (pCharacters.Length == 0)
		{
			value.characters_string = string.Empty;
			value.characters = null;
		}
		else
		{
			string[] array = pCharacters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			Array.Sort(array, (IComparer<string>?)StringComparer.OrdinalIgnoreCase);
			value.characters_string = string.Join(' ', array);
			value.characters = array;
		}
		if (string.Equals(characters_string, value.characters_string, StringComparison.OrdinalIgnoreCase))
		{
			result = false;
		}
		return result;
	}

	public void cloneFrom(OnomasticsData pOriginalData)
	{
		_template_data.AddRange(pOriginalData._template_data);
		foreach (KeyValuePair<string, OnomasticsDataGroup> group in pOriginalData.groups)
		{
			setGroup(group.Key, group.Value.characters_string);
		}
	}

	public void setDebugTest()
	{
		loadFromShortTemplate("0312|0:pl p s l d b;1:mp rp rt b;2:kin tin le ee;3:a e i o u y oo");
	}

	internal static bool hasCheckOnTheRight(OnomasticsData pData, int pIndex)
	{
		List<string> currentSubgroup = pData.getCurrentSubgroup();
		int num = pIndex + 1;
		if (num >= currentSubgroup.Count)
		{
			return false;
		}
		string pID = currentSubgroup[num];
		return AssetManager.onomastics_library.get(pID).check_delegate != null;
	}

	internal static bool passesCheckOnTheRight(OnomasticsData pData, StringBuilderPool pLocalNameBuilder, string pLastPart, int pIndex, ActorSex pSex)
	{
		List<string> currentSubgroup = pData.getCurrentSubgroup();
		int num = pIndex + 1;
		if (num >= currentSubgroup.Count)
		{
			return true;
		}
		string pID = currentSubgroup[num];
		OnomasticsAsset onomasticsAsset = AssetManager.onomastics_library.get(pID);
		bool flag = onomasticsAsset.check_delegate(onomasticsAsset, pData, pLocalNameBuilder, pLastPart, num, pSex);
		if (flag && hasCheckOnTheRight(pData, num))
		{
			return passesCheckOnTheRight(pData, pLocalNameBuilder, pLastPart, num, pSex);
		}
		return flag;
	}

	public string generateName(ActorSex pSex = ActorSex.None, int pCalls = 0, long? pSeed = null)
	{
		if (pCalls > 100)
		{
			return "Rebr";
		}
		if (pSex == ActorSex.None)
		{
			pSex = (Randy.randomBool() ? ActorSex.Female : ActorSex.Male);
		}
		string pLastPart = string.Empty;
		if (pSeed.HasValue)
		{
			Randy.resetSeed(pSeed.Value);
		}
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		using ListPool<string> collection = getSubgroup(_template_data);
		_current_subgroup.Clear();
		_current_subgroup.AddRange(collection);
		int count = _current_subgroup.Count;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			string pID = _current_subgroup[i];
			OnomasticsAsset onomasticsAsset = AssetManager.onomastics_library.get(pID);
			if (hasCheckOnTheRight(this, i) && !passesCheckOnTheRight(this, stringBuilderPool, pLastPart, i, pSex))
			{
				continue;
			}
			OnomasticsAssetType type = onomasticsAsset.type;
			if ((uint)type <= 1u)
			{
				string text = onomasticsAsset.namemaker_delegate?.Invoke(onomasticsAsset, this, stringBuilderPool, pLastPart, i, pSex);
				if (text != null && text.Length > 0)
				{
					pLastPart = text;
					stringBuilderPool.Append(text);
				}
				if (onomasticsAsset.is_upper)
				{
					flag = true;
				}
			}
		}
		stringBuilderPool.Remove(',');
		stringBuilderPool.TrimEnd(' ', '-');
		if (stringBuilderPool.Length > 30)
		{
			stringBuilderPool.Cut(0, 30);
		}
		if (stringBuilderPool.Length == 0)
		{
			return generateName(pSex, ++pCalls);
		}
		if (Blacklist.checkBlackList(stringBuilderPool))
		{
			return generateName(pSex, ++pCalls);
		}
		if (flag)
		{
			stringBuilderPool.ToUpperInvariant();
		}
		else
		{
			stringBuilderPool.ToTitleCase();
		}
		return stringBuilderPool.ToString();
	}

	private ListPool<string> getSubgroup(List<string> pTemplateData)
	{
		int num = countDividers(pTemplateData) + 1;
		if (num > 1)
		{
			int pIndex = Randy.randomInt(0, num);
			return getSubgroupByIndex(pIndex);
		}
		return new ListPool<string>(pTemplateData);
	}

	private ListPool<string> getSubgroupByIndex(int pIndex)
	{
		ListPool<string> listPool = new ListPool<string>();
		int num = 0;
		foreach (string template_datum in _template_data)
		{
			if (template_datum == "divider")
			{
				num++;
				if (num > pIndex)
				{
					break;
				}
			}
			else if (num == pIndex)
			{
				listPool.Add(template_datum);
			}
		}
		return listPool;
	}

	private static int countDividers(List<string> pTemplateData)
	{
		int num = 0;
		foreach (string pTemplateDatum in pTemplateData)
		{
			if (pTemplateDatum == "divider")
			{
				num++;
			}
		}
		return num;
	}

	public static string convertToCultureTitleCase(string pString)
	{
		string text = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(pString.ToLower());
		if (text.Contains('\''))
		{
			text = capitalizeAfterApostrophe(text);
		}
		return text;
	}

	private static string capitalizeAfterApostrophe(string pInput)
	{
		TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
		char[] array = pInput.ToCharArray();
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			if (flag && char.IsLetter(array[i]))
			{
				array[i] = textInfo.ToUpper(array[i]);
				flag = false;
			}
			if (array[i] == '\'')
			{
				flag = true;
			}
		}
		return new string(array);
	}

	public string getShortTemplate()
	{
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		foreach (string template_datum in _template_data)
		{
			OnomasticsAsset onomasticsAsset = AssetManager.onomastics_library.get(template_datum);
			stringBuilderPool.Append(onomasticsAsset.short_id);
		}
		stringBuilderPool.Append('|');
		bool flag = true;
		foreach (KeyValuePair<string, OnomasticsDataGroup> item in groups.OrderBy((KeyValuePair<string, OnomasticsDataGroup> p) => AssetManager.onomastics_library.get(p.Key).short_id))
		{
			if (!item.Value.isEmpty())
			{
				if (!flag)
				{
					stringBuilderPool.Append(';');
				}
				OnomasticsAsset onomasticsAsset2 = AssetManager.onomastics_library.get(item.Key);
				stringBuilderPool.Append(onomasticsAsset2.short_id);
				stringBuilderPool.Append(':');
				stringBuilderPool.Append(item.Value.characters_string.ToLower());
				flag = false;
			}
		}
		return stringBuilderPool.ToString();
	}

	public bool templateIsValid(string pShortTemplate)
	{
		if (string.IsNullOrEmpty(pShortTemplate))
		{
			return false;
		}
		if (!pShortTemplate.Contains('|'))
		{
			return false;
		}
		if (pShortTemplate.Split('|').Length != 2)
		{
			return false;
		}
		return true;
	}

	public void loadFromShortTemplate(string pShortTemplate)
	{
		if (string.IsNullOrEmpty(pShortTemplate))
		{
			pShortTemplate = "|";
		}
		if (!pShortTemplate.Contains('|'))
		{
			throw new ArgumentException("Invalid template format: (NS) " + pShortTemplate);
		}
		pShortTemplate = pShortTemplate.Trim('\n', '\r', ' ', '"', '`');
		if (pShortTemplate.Contains('\n'))
		{
			throw new ArgumentException("Invalid template format: (NL) " + pShortTemplate);
		}
		if (pShortTemplate.Contains('\r'))
		{
			throw new ArgumentException("Invalid template format: (NL) " + pShortTemplate);
		}
		string[] array = pShortTemplate.Split('|');
		if (array.Length != 2)
		{
			throw new ArgumentException($"Invalid template format: (L{array.Length}) " + pShortTemplate);
		}
		char[] array2 = array[0].ToCharArray();
		using ListPool<string> listPool = new ListPool<string>(array2.Length);
		char[] array3 = array2;
		for (int i = 0; i < array3.Length; i++)
		{
			char c = array3[i];
			OnomasticsAsset byShortId = AssetManager.onomastics_library.getByShortId(c.ToString());
			if (byShortId != null)
			{
				listPool.Add(byShortId.id);
				continue;
			}
			throw new ArgumentException($"Invalid template format: (0{c}) " + pShortTemplate);
		}
		setTemplateData(listPool);
		string[] array4 = array[1].Split(';');
		groups.Clear();
		string[] array5 = array4;
		for (int i = 0; i < array5.Length; i++)
		{
			string[] array6 = array5[i].Split(':');
			if (array6.Length == 2)
			{
				string text = array6[0];
				string pCharacters = array6[1];
				OnomasticsAsset byShortId2 = AssetManager.onomastics_library.getByShortId(text);
				if (byShortId2 == null)
				{
					throw new ArgumentException("Invalid template format: (G" + text + ") " + pShortTemplate);
				}
				saveGroup(byShortId2.id, pCharacters);
			}
		}
	}

	public bool isGendered()
	{
		foreach (string template_datum in _template_data)
		{
			OnomasticsAsset onomasticsAsset = AssetManager.onomastics_library.get(template_datum);
			if (onomasticsAsset.id == "sex_male" || onomasticsAsset.id == "sex_female")
			{
				return true;
			}
		}
		return false;
	}

	public bool isEmpty()
	{
		return _template_data.Count == 0;
	}

	public bool setGroup(string pID, string pString)
	{
		return saveGroup(pID, pString);
	}

	public void Dispose()
	{
		groups.Clear();
		groups = null;
		_template_data.Clear();
		_template_data = null;
		_current_subgroup.Clear();
		_current_subgroup = null;
	}
}
