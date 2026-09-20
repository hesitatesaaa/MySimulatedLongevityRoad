using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class OnomasticsLibrary : AssetLibrary<OnomasticsAsset>
{
	private Dictionary<string, OnomasticsAsset> _dict_short_id = new Dictionary<string, OnomasticsAsset>();

	[NonSerialized]
	public List<OnomasticsAsset> list_special = new List<OnomasticsAsset>();

	private static readonly char[] _word_separators;

	private static Random _rand;

	private Dictionary<string, string> _test_templates = new Dictionary<string, string>
	{
		{ "01|0:a;1:b", "Ab" },
		{ "0_1|0:a;1:b", "A B" },
		{ "01b|0:a;1:b", "A" },
		{ "o|0:a;1:b;2:c;5:f", "Abcf" },
		{ "110c|0:a;1:b", "Bab" },
		{ "001c|0:a;1:b", "Aa" },
		{ "001v|0:a;1:b", "Aba" },
		{ "110v|0:a;1:b", "Bb" },
		{ "110C|0:a;1:b", "Bba" },
		{ "001C|0:a;1:b", "Aa" },
		{ "001V|0:a;1:b", "Aab" },
		{ "110V|0:a;1:b", "Bb" },
		{ "10E|0:a;1:b", "A" },
		{ "01E|0:a;1:b", "A" },
		{ "01e|0:a;1:b", "B" },
		{ "10e|0:a;1:b", "B" },
		{ "1,10E|0:a;1:b", "Ba" },
		{ "1-10E|0:a;1:b", "B-A" },
		{ "0,01e|0:a;1:b", "Ab" },
		{ "0-01e|0:a;1:b", "A-B" },
		{ "01D|0:a;1:b", "Abb" },
		{ "00D|0:a;1:b", "Aa" },
		{ "01d|0:a;1:b", "Aab" },
		{ "11d|0:a;1:b", "Bb" },
		{ "0r|0:a", "Aaa" },
		{ "0rr|0:a", "Aaaaaaa" },
		{ "0rbu|0:a", "AA" }
	};

	public override void init()
	{
		base.init();
		addGroups();
		addDice();
		addUnique();
		addSpecialCharacters();
		addVowelConsonantSpecials();
		addSexes();
	}

	private void addUnique()
	{
		add(new OnomasticsAsset
		{
			id = "clone_last",
			short_id = 'l',
			color_text = "#EB2931",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = (OnomasticsAsset _, OnomasticsData _, StringBuilderPool pStringBuilder, string _, int _, ActorSex _) => (pStringBuilder.Length == 0) ? string.Empty : getLastWord(pStringBuilder),
			affects_left_word = true
		});
		add(new OnomasticsAsset
		{
			id = "coin_flip",
			short_id = 'f',
			color_text = "#F5C308",
			type = OnomasticsAssetType.Special,
			check_delegate = (OnomasticsAsset _, OnomasticsData _, StringBuilderPool _, string _, int _, ActorSex _) => Randy.randomBool(),
			affects_left = true
		});
		add(new OnomasticsAsset
		{
			id = "mirror",
			short_id = 'm',
			color_text = "#FFFFFF",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = (OnomasticsAsset _, OnomasticsData _, StringBuilderPool pStringBuilder, string _, int _, ActorSex _) => (pStringBuilder.Length == 0) ? string.Empty : getLastWord(pStringBuilder).Reverse(),
			affects_left_word = true
		});
		add(new OnomasticsAsset
		{
			id = "wild_6",
			short_id = 'w',
			color_text = "#A1B1A2",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = delegate(OnomasticsAsset _, OnomasticsData pData, StringBuilderPool pStringBuilder, string pLastPart, int pIndex, ActorSex pSex)
			{
				using ListPool<string> listPool = new ListPool<string>(pData.groups.Count);
				foreach (KeyValuePair<string, OnomasticsDataGroup> group in pData.groups)
				{
					if (!group.Value.isEmpty() && AssetManager.onomastics_library.get(group.Key).group_id < 6)
					{
						listPool.Add(group.Key);
					}
				}
				if (listPool.Count == 0)
				{
					return string.Empty;
				}
				string random = GetRandom(listPool);
				OnomasticsAsset onomasticsAsset = AssetManager.onomastics_library.get(random);
				return onomasticsAsset.namemaker_delegate(onomasticsAsset, pData, pStringBuilder, pLastPart, pIndex, pSex);
			}
		});
		add(new OnomasticsAsset
		{
			id = "domino",
			short_id = 'o',
			color_text = "#7FA9BC",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = delegate(OnomasticsAsset _, OnomasticsData pData, StringBuilderPool pStringBuilder, string pLastPart, int pIndex, ActorSex pSex)
			{
				string text = "";
				for (int i = 0; i < 6; i++)
				{
					string pID = $"group_{i + 1}";
					OnomasticsAsset onomasticsAsset = AssetManager.onomastics_library.get(pID);
					text += onomasticsAsset.namemaker_delegate(onomasticsAsset, pData, pStringBuilder, pLastPart, pIndex, pSex);
				}
				return text;
			}
		});
		add(new OnomasticsAsset
		{
			id = "repeater",
			short_id = 'r',
			color_text = "#F3AB11",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = delegate(OnomasticsAsset _, OnomasticsData pData, StringBuilderPool pStringBuilder, string pLastPart, int pIndex, ActorSex pSex)
			{
				if (pIndex == 0)
				{
					return string.Empty;
				}
				int num = pIndex - 1;
				List<string> currentSubgroup = pData.getCurrentSubgroup();
				OnomasticsAsset onomasticsAsset = AssetManager.onomastics_library.get(currentSubgroup[num]);
				if (onomasticsAsset.is_divider)
				{
					return string.Empty;
				}
				while (onomasticsAsset.namemaker_delegate == null)
				{
					num--;
					if (num < 0)
					{
						return string.Empty;
					}
					onomasticsAsset = AssetManager.onomastics_library.get(currentSubgroup[num]);
					if (onomasticsAsset.is_divider)
					{
						return string.Empty;
					}
				}
				if (onomasticsAsset.namemaker_delegate == null)
				{
					return string.Empty;
				}
				string text = "";
				for (int i = 0; i < 2; i++)
				{
					if (!OnomasticsData.hasCheckOnTheRight(pData, num) || OnomasticsData.passesCheckOnTheRight(pData, pStringBuilder, pLastPart, num, pSex))
					{
						text += onomasticsAsset.namemaker_delegate(onomasticsAsset, pData, pStringBuilder, pLastPart, num, pSex);
					}
				}
				return text;
			},
			affects_left = true
		});
		add(new OnomasticsAsset
		{
			id = "upper",
			short_id = 'u',
			color_text = "#B2FF75",
			is_upper = true,
			type = OnomasticsAssetType.Special,
			affects_everything = true
		});
		add(new OnomasticsAsset
		{
			id = "backspace",
			short_id = 'b',
			color_text = "#FF6A43",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = delegate(OnomasticsAsset _, OnomasticsData _, StringBuilderPool pStringBuilder, string _, int _, ActorSex _)
			{
				if (pStringBuilder.Length == 0)
				{
					return string.Empty;
				}
				pStringBuilder.Remove(pStringBuilder.Length - 1, 1);
				return string.Empty;
			}
		});
	}

	private void addVowelConsonantSpecials()
	{
		add(new OnomasticsAsset
		{
			id = "consonant_separator",
			short_id = 'c',
			color_text = "#657CCE",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = delegate(OnomasticsAsset _, OnomasticsData pData, StringBuilderPool pStringBuilder, string _, int pIndex, ActorSex _)
			{
				if (pIndex == 0)
				{
					return string.Empty;
				}
				int index = pIndex - 1;
				List<string> currentSubgroup = pData.getCurrentSubgroup();
				OnomasticsAsset onomasticsAsset = AssetManager.onomastics_library.get(currentSubgroup[index]);
				if (!onomasticsAsset.isGroupType())
				{
					return string.Empty;
				}
				if (pData.isGroupEmpty(onomasticsAsset.id))
				{
					return string.Empty;
				}
				ConsonantSeparator.addRandomVowels(pStringBuilder, pData.getGroup(onomasticsAsset.id).characters);
				return string.Empty;
			},
			check_delegate = blockGroupOnLeft,
			affects_left = true,
			affects_left_word = true,
			affects_left_group_only = true
		});
		add(new OnomasticsAsset
		{
			id = "vowel_separator",
			short_id = 'v',
			color_text = "#FF68C5",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = delegate(OnomasticsAsset _, OnomasticsData pData, StringBuilderPool pStringBuilder, string _, int pIndex, ActorSex _)
			{
				if (pIndex == 0)
				{
					return string.Empty;
				}
				int index = pIndex - 1;
				List<string> currentSubgroup = pData.getCurrentSubgroup();
				OnomasticsAsset onomasticsAsset = AssetManager.onomastics_library.get(currentSubgroup[index]);
				if (!onomasticsAsset.isGroupType())
				{
					return string.Empty;
				}
				if (pData.isGroupEmpty(onomasticsAsset.id))
				{
					return string.Empty;
				}
				VowelSeparator.addRandomConsonants(pStringBuilder, pData.getGroup(onomasticsAsset.id).characters);
				return string.Empty;
			},
			check_delegate = blockGroupOnLeft,
			affects_left = true,
			affects_left_word = true,
			affects_left_group_only = true
		});
		add(new OnomasticsAsset
		{
			id = "vowel_duplicator",
			short_id = 'd',
			color_text = "#D9B03A",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = delegate(OnomasticsAsset _, OnomasticsData _, StringBuilderPool pStringBuilder, string _, int _, ActorSex _)
			{
				if (pStringBuilder.Length == 0)
				{
					return string.Empty;
				}
				var (pStart, pLength) = getLastWordBounds(pStringBuilder);
				using ListPool<int> listPool = VowelSeparator.findAllSingleVowels(pStringBuilder, pStart, pLength);
				if (listPool.Count == 0)
				{
					return string.Empty;
				}
				int random = GetRandom(listPool);
				pStringBuilder.Insert(random, pStringBuilder[random]);
				return string.Empty;
			},
			affects_left_word = true
		});
		add(new OnomasticsAsset
		{
			id = "consonant_duplicator",
			short_id = 'D',
			color_text = "#2E76AD",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = delegate(OnomasticsAsset _, OnomasticsData _, StringBuilderPool pStringBuilder, string _, int _, ActorSex _)
			{
				if (pStringBuilder.Length == 0)
				{
					return string.Empty;
				}
				var (pStart, pLength) = getLastWordBounds(pStringBuilder);
				using ListPool<int> listPool = ConsonantSeparator.findAllSingleConsonants(pStringBuilder, pStart, pLength);
				if (listPool.Count == 0)
				{
					return string.Empty;
				}
				int random = GetRandom(listPool);
				pStringBuilder.Insert(random, pStringBuilder[random]);
				return string.Empty;
			},
			affects_left_word = true
		});
		add(new OnomasticsAsset
		{
			id = "vowel_replacer",
			short_id = 'e',
			color_text = "#FF68C5",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = delegate(OnomasticsAsset _, OnomasticsData pData, StringBuilderPool pStringBuilder, string _, int pIndex, ActorSex _)
			{
				if (pIndex == 0)
				{
					return string.Empty;
				}
				int index = pIndex - 1;
				List<string> currentSubgroup = pData.getCurrentSubgroup();
				OnomasticsAsset onomasticsAsset = AssetManager.onomastics_library.get(currentSubgroup[index]);
				if (!onomasticsAsset.isGroupType())
				{
					return string.Empty;
				}
				if (pData.isGroupEmpty(onomasticsAsset.id))
				{
					return string.Empty;
				}
				var (pStart, pLength) = getLastWordBounds(pStringBuilder);
				using ListPool<int> listPool = VowelSeparator.findAllVowels(pStringBuilder, pStart, pLength);
				if (listPool.Count == 0)
				{
					return string.Empty;
				}
				string random = GetRandom(pData.getGroup(onomasticsAsset.id).characters);
				int random2 = GetRandom(listPool);
				pStringBuilder.Remove(random2, 1);
				pStringBuilder.Insert(random2, random);
				return string.Empty;
			},
			check_delegate = blockGroupOnLeft,
			affects_left = true,
			affects_left_word = true,
			affects_left_group_only = true
		});
		add(new OnomasticsAsset
		{
			id = "consonant_replacer",
			short_id = 'E',
			color_text = "#A8A8A8",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = delegate(OnomasticsAsset _, OnomasticsData pData, StringBuilderPool pStringBuilder, string _, int pIndex, ActorSex _)
			{
				if (pIndex == 0)
				{
					return string.Empty;
				}
				int index = pIndex - 1;
				List<string> currentSubgroup = pData.getCurrentSubgroup();
				OnomasticsAsset onomasticsAsset = AssetManager.onomastics_library.get(currentSubgroup[index]);
				if (!onomasticsAsset.isGroupType())
				{
					return string.Empty;
				}
				if (pData.isGroupEmpty(onomasticsAsset.id))
				{
					return string.Empty;
				}
				var (pStart, pLength) = getLastWordBounds(pStringBuilder);
				using ListPool<int> listPool = ConsonantSeparator.findAllConsonants(pStringBuilder, pStart, pLength);
				if (listPool.Count == 0)
				{
					return string.Empty;
				}
				string random = GetRandom(pData.getGroup(onomasticsAsset.id).characters);
				int random2 = GetRandom(listPool);
				pStringBuilder.Remove(random2, 1);
				pStringBuilder.Insert(random2, random);
				return string.Empty;
			},
			check_delegate = blockGroupOnLeft,
			affects_left = true,
			affects_left_word = true,
			affects_left_group_only = true
		});
		add(new OnomasticsAsset
		{
			id = "vowel_requirer",
			short_id = 'V',
			color_text = "#FBFDC2",
			type = OnomasticsAssetType.Special,
			check_delegate = (OnomasticsAsset _, OnomasticsData _, StringBuilderPool pStringBuilder, string _, int _, ActorSex _) => pStringBuilder.Length != 0 && VowelSeparator.isVowel(pStringBuilder[pStringBuilder.Length - 1]),
			affects_left = true
		});
		add(new OnomasticsAsset
		{
			id = "consonant_requirer",
			short_id = 'C',
			color_text = "#CAB2A8",
			type = OnomasticsAssetType.Special,
			check_delegate = (OnomasticsAsset _, OnomasticsData _, StringBuilderPool pStringBuilder, string _, int _, ActorSex _) => pStringBuilder.Length != 0 && ConsonantSeparator.isConsonant(pStringBuilder[pStringBuilder.Length - 1]),
			affects_left = true
		});
	}

	private void addDice()
	{
	}

	private void addGroups()
	{
		add(new OnomasticsAsset
		{
			id = "group_1",
			short_id = '0',
			color_text = "#31EB29",
			forced_locale_subname_id = "onomastics_group_subname",
			forced_locale_description_id = "onomastics_group_info",
			forced_locale_description_id_2 = "onomastics_group_info_2",
			type = OnomasticsAssetType.Group,
			namemaker_delegate = groupAction,
			group_id = 0
		});
		add(new OnomasticsAsset
		{
			id = "group_2",
			short_id = '1',
			color_text = "#28ECA0",
			forced_locale_subname_id = "onomastics_group_subname",
			forced_locale_description_id = "onomastics_group_info",
			forced_locale_description_id_2 = "onomastics_group_info_2",
			type = OnomasticsAssetType.Group,
			namemaker_delegate = groupAction,
			group_id = 1
		});
		add(new OnomasticsAsset
		{
			id = "group_3",
			short_id = '2',
			color_text = "#3FABE9",
			forced_locale_subname_id = "onomastics_group_subname",
			forced_locale_description_id = "onomastics_group_info",
			forced_locale_description_id_2 = "onomastics_group_info_2",
			type = OnomasticsAssetType.Group,
			namemaker_delegate = groupAction,
			group_id = 2
		});
		add(new OnomasticsAsset
		{
			id = "group_4",
			short_id = '3',
			color_text = "#6A78FF",
			forced_locale_subname_id = "onomastics_group_subname",
			forced_locale_description_id = "onomastics_group_info",
			forced_locale_description_id_2 = "onomastics_group_info_2",
			type = OnomasticsAssetType.Group,
			namemaker_delegate = groupAction,
			group_id = 3
		});
		add(new OnomasticsAsset
		{
			id = "group_5",
			short_id = '4',
			color_text = "#A129EB",
			forced_locale_subname_id = "onomastics_group_subname",
			forced_locale_description_id = "onomastics_group_info",
			forced_locale_description_id_2 = "onomastics_group_info_2",
			type = OnomasticsAssetType.Group,
			namemaker_delegate = groupAction,
			group_id = 4
		});
		add(new OnomasticsAsset
		{
			id = "group_6",
			short_id = '5',
			color_text = "#EB29B3",
			forced_locale_subname_id = "onomastics_group_subname",
			forced_locale_description_id = "onomastics_group_info",
			forced_locale_description_id_2 = "onomastics_group_info_2",
			type = OnomasticsAssetType.Group,
			namemaker_delegate = groupAction,
			group_id = 5
		});
		add(new OnomasticsAsset
		{
			id = "group_7",
			short_id = '6',
			color_text = "#EB2931",
			forced_locale_subname_id = "onomastics_group_subname",
			forced_locale_description_id = "onomastics_group_info",
			forced_locale_description_id_2 = "onomastics_group_info_2",
			type = OnomasticsAssetType.Group,
			namemaker_delegate = groupAction,
			group_id = 6
		});
		add(new OnomasticsAsset
		{
			id = "group_8",
			short_id = '7',
			color_text = "#FF8D1A",
			forced_locale_subname_id = "onomastics_group_subname",
			forced_locale_description_id = "onomastics_group_info",
			forced_locale_description_id_2 = "onomastics_group_info_2",
			type = OnomasticsAssetType.Group,
			namemaker_delegate = groupAction,
			group_id = 7
		});
		add(new OnomasticsAsset
		{
			id = "group_9",
			short_id = '8',
			color_text = "#EFCB00",
			forced_locale_subname_id = "onomastics_group_subname",
			forced_locale_description_id = "onomastics_group_info",
			forced_locale_description_id_2 = "onomastics_group_info_2",
			type = OnomasticsAssetType.Group,
			namemaker_delegate = groupAction,
			group_id = 8
		});
		add(new OnomasticsAsset
		{
			id = "group_10",
			short_id = '9',
			color_text = "#D2DAC0",
			forced_locale_subname_id = "onomastics_group_subname",
			forced_locale_description_id = "onomastics_group_info",
			forced_locale_description_id_2 = "onomastics_group_info_2",
			type = OnomasticsAssetType.Group,
			namemaker_delegate = groupAction,
			group_id = 9
		});
	}

	private void addSpecialCharacters()
	{
		add(new OnomasticsAsset
		{
			id = "space",
			short_id = '_',
			color_text = "#D2DAC0",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = delegate(OnomasticsAsset _, OnomasticsData _, StringBuilderPool pStringBuilder, string _, int _, ActorSex _)
			{
				if (pStringBuilder.Length == 0)
				{
					return string.Empty;
				}
				return (pStringBuilder[pStringBuilder.Length - 1] == ' ') ? string.Empty : " ";
			},
			is_word_divider = true
		});
		add(new OnomasticsAsset
		{
			id = "silent_space",
			short_id = ',',
			color_text = "#C93F3F",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = delegate(OnomasticsAsset _, OnomasticsData _, StringBuilderPool pStringBuilder, string _, int _, ActorSex _)
			{
				if (pStringBuilder.Length == 0)
				{
					return string.Empty;
				}
				if (pStringBuilder[pStringBuilder.Length - 1] == ' ')
				{
					return string.Empty;
				}
				return (pStringBuilder[pStringBuilder.Length - 1] == ',') ? string.Empty : ",";
			},
			is_word_divider = true
		});
		add(new OnomasticsAsset
		{
			id = "hyphen",
			short_id = '-',
			color_text = "#BBA826",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = (OnomasticsAsset _, OnomasticsData _, StringBuilderPool _, string _, int _, ActorSex _) => "-",
			is_word_divider = true
		});
		add(new OnomasticsAsset
		{
			id = "numbers",
			short_id = '#',
			color_text = "#35B929",
			type = OnomasticsAssetType.Special,
			namemaker_delegate = (OnomasticsAsset _, OnomasticsData _, StringBuilderPool _, string _, int _, ActorSex _) => Randy.randomInt(0, 10).ToString()
		});
		add(new OnomasticsAsset
		{
			id = "divider",
			short_id = '/',
			color_text = "#D2DAC0",
			type = OnomasticsAssetType.Special,
			is_divider = true,
			is_word_divider = true,
			is_immune = true
		});
	}

	private void addSexes()
	{
		add(new OnomasticsAsset
		{
			id = "sex_male",
			short_id = 'M',
			color_text = "#86BC4E",
			type = OnomasticsAssetType.Special,
			check_delegate = (OnomasticsAsset _, OnomasticsData _, StringBuilderPool _, string _, int _, ActorSex pSex) => pSex == ActorSex.Male,
			affects_left = true
		});
		add(new OnomasticsAsset
		{
			id = "sex_female",
			short_id = 'F',
			color_text = "#EB29B3",
			type = OnomasticsAssetType.Special,
			check_delegate = (OnomasticsAsset _, OnomasticsData _, StringBuilderPool _, string _, int _, ActorSex pSex) => pSex == ActorSex.Female,
			affects_left = true
		});
	}

	private string getLastWord(StringBuilderPool pStringBuilder)
	{
		var (num, num2) = getLastWordBounds(pStringBuilder, pPrevious: true);
		return pStringBuilder.ToString(num, num2);
	}

	private (int tStart, int tLength) getLastWordBounds(StringBuilderPool pStringBuilder, bool pPrevious = false)
	{
		if (pStringBuilder.Length == 0)
		{
			return (tStart: 0, tLength: 0);
		}
		int num = pStringBuilder.LastIndexOfAny(_word_separators) + 1;
		int num2 = pStringBuilder.Length - num;
		if (pPrevious && num2 == 0)
		{
			num = pStringBuilder.LastIndexOfAny(_word_separators, num - 2) + 1;
			num2 = pStringBuilder.Length - num - (num2 + 1);
		}
		return (tStart: num, tLength: num2);
	}

	private string groupAction(OnomasticsAsset pAsset, OnomasticsData pData, StringBuilderPool pStringBuilder, string pLastPart, int pIndex, ActorSex pSex)
	{
		return pData.getRandomPartFromGroup(pAsset.id);
	}

	private bool blockGroupOnLeft(OnomasticsAsset pAsset, OnomasticsData pData, StringBuilderPool pStringBuilder, string pLastPart, int pIndex, ActorSex pSex)
	{
		return !isGroupOnLeft(pAsset, pData, pStringBuilder, pLastPart, pIndex, pSex);
	}

	private bool isGroupOnLeft(OnomasticsAsset pAsset, OnomasticsData pData, StringBuilderPool pStringBuilder, string pLastPart, int pIndex, ActorSex pSex)
	{
		if (pIndex == 0)
		{
			return false;
		}
		int index = pIndex - 1;
		List<string> currentSubgroup = pData.getCurrentSubgroup();
		OnomasticsAsset onomasticsAsset = AssetManager.onomastics_library.get(currentSubgroup[index]);
		if (!onomasticsAsset.isGroupType())
		{
			return false;
		}
		if (pData.isGroupEmpty(onomasticsAsset.id))
		{
			return false;
		}
		return true;
	}

	public override OnomasticsAsset add(OnomasticsAsset pAsset)
	{
		base.add(pAsset);
		_dict_short_id.Add(pAsset.short_id.ToString(), pAsset);
		return pAsset;
	}

	public OnomasticsAsset getByShortId(string pShortID)
	{
		if (_dict_short_id.ContainsKey(pShortID))
		{
			return _dict_short_id[pShortID];
		}
		return null;
	}

	public static T GetRandom<T>(T[] list)
	{
		((Random)(ref _rand)).InitState(Randy.rand.state);
		return list[((Random)(ref _rand)).NextInt(list.Length)];
	}

	public static T GetRandom<T>(ListPool<T> list)
	{
		((Random)(ref _rand)).InitState(Randy.rand.state);
		return list[((Random)(ref _rand)).NextInt(list.Count)];
	}

	public override void post_init()
	{
		base.post_init();
		foreach (OnomasticsAsset item in list)
		{
			if (string.IsNullOrEmpty(item.path_icon))
			{
				item.path_icon = "ui/Icons/onomastics/onomastics_" + item.id;
			}
		}
	}

	public override void editorDiagnostic()
	{
		foreach (OnomasticsAsset item in list)
		{
			if ((Object)(object)SpriteTextureLoader.getSprite(item.path_icon) == (Object)null)
			{
				BaseAssetLibrary.logAssetError("Missing icon file", item.path_icon);
			}
		}
		foreach (OnomasticsAsset item2 in list)
		{
			foreach (OnomasticsAsset item3 in list)
			{
				if (item2 != item3 && item2.short_id == item3.short_id)
				{
					Debug.LogError((object)("OnomasticsAsset: Duplicate short_id: " + item2.short_id + " for " + item2.id + " and " + item3.id));
				}
			}
		}
		foreach (KeyValuePair<string, string> test_template in _test_templates)
		{
			string key = test_template.Key;
			string value = test_template.Value;
			string text = NameGenerator.generateNameFromOnomastics(null, key);
			if (text != value)
			{
				BaseAssetLibrary.logAssetError("<e>Onomastics</e>: Template test failed: " + text + " != " + value, key);
			}
		}
		base.editorDiagnostic();
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (OnomasticsAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
		}
	}

	static OnomasticsLibrary()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		_word_separators = new char[3] { ' ', ',', '-' };
		_rand = default(Random);
	}
}
