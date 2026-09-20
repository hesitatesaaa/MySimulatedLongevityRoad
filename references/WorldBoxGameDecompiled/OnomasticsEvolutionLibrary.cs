public class OnomasticsEvolutionLibrary : AssetLibrary<OnomasticsEvolutionAsset>
{
	private static readonly char[] _vowels = AssetLibrary<OnomasticsEvolutionAsset>.a<char>('a', 'e', 'i', 'o', 'u', 'y');

	private static readonly char[] _vowels_h = AssetLibrary<OnomasticsEvolutionAsset>.a<char>('a', 'e', 'i', 'o', 'u', 'y', 'h');

	private static readonly char[] _consonants = AssetLibrary<OnomasticsEvolutionAsset>.a<char>('b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z');

	public override void init()
	{
		base.init();
		add(new OnomasticsEvolutionAsset
		{
			from = "k",
			to = "c",
			not_surrounded_by = _consonants,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "g",
			to = "k",
			not_surrounded_by = _consonants,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "f",
			to = "v",
			not_surrounded_by = _consonants,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "g",
			to = "gh",
			not_surrounded_by = _consonants,
			replacer = replace_in_end
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "f",
			to = "gh",
			not_surrounded_by = _consonants,
			replacer = replace_in_end
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "v",
			to = "b",
			not_surrounded_by = _consonants,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ci",
			to = "z",
			not_surrounded_by = _consonants,
			replacer = replace_not_in_start
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "s",
			to = "z",
			not_surrounded_by = _consonants,
			replacer = replace_in_end
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "t",
			to = "d",
			not_surrounded_by = _consonants,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ks",
			to = "x",
			not_surrounded_by = _consonants,
			replacer = replace_not_in_start
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "th",
			to = "f",
			not_surrounded_by = _consonants,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "th",
			to = "d",
			not_surrounded_by = _consonants,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "thi",
			to = "ti",
			not_surrounded_by = _consonants,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "er",
			to = "ar",
			not_surrounded_by = _consonants,
			replacer = replace_in_end
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "sh",
			to = "sch",
			not_surrounded_by = _consonants,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "sh",
			to = "sz",
			not_surrounded_by = _consonants,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ch",
			to = "cz",
			not_surrounded_by = _consonants,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "mm",
			to = "m",
			not_surrounded_by = _consonants,
			replacer = replace_in_end
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "mn",
			to = "m",
			not_surrounded_by = _consonants,
			replacer = replace_in_end
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "gg",
			to = "g",
			not_surrounded_by = _consonants,
			replacer = replace_not_in_start
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "nn",
			to = "n",
			not_surrounded_by = _consonants,
			replacer = replace_in_end
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "mm",
			to = "hm",
			not_surrounded_by = _consonants,
			replacer = replace_in_end
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ph",
			to = "f",
			not_surrounded_by = _consonants,
			replacer = replace_in_end
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ph",
			to = "pp",
			not_surrounded_by = _consonants,
			replacer = replace_in_end
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "p",
			to = "pp",
			not_surrounded_by = _consonants,
			replacer = replace_in_end
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ck",
			to = "k",
			not_surrounded_by = _consonants,
			replacer = replace_not_in_start
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ck",
			to = "gg",
			not_surrounded_by = _consonants,
			replacer = replace_not_in_start
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "e",
			to = "ai",
			not_surrounded_by = _vowels,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "e",
			to = "a",
			not_surrounded_by = _vowels,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "o",
			to = "a",
			not_surrounded_by = _vowels,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "u",
			to = "y",
			not_surrounded_by = _vowels,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ie",
			to = "y",
			not_surrounded_by = _vowels,
			replacer = replace_in_end
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "i",
			to = "y",
			not_surrounded_by = _vowels,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "y",
			to = "oe",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "e",
			to = "ae",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "a",
			to = "au",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "o",
			to = "au",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "oo",
			to = "ou",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "oo",
			to = "ue",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "oo",
			to = "oa",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "oo",
			to = "u",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ee",
			to = "i",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ie",
			to = "e",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ei",
			to = "ee",
			not_surrounded_by = _vowels,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ea",
			to = "ee",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ea",
			to = "ei",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ear",
			to = "ere",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "re",
			to = "ru",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "sp",
			to = "shp",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "sht",
			to = "st",
			not_surrounded_by = _vowels_h,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "u",
			to = "oe",
			not_surrounded_by = _vowels_h,
			reverse = false,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "o",
			to = "oe",
			not_surrounded_by = _vowels_h,
			reverse = false,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "oh",
			to = "oe",
			not_surrounded_by = _vowels,
			reverse = false,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ah",
			to = "ae",
			not_surrounded_by = _vowels,
			reverse = false,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "oe",
			to = "u",
			not_surrounded_by = _vowels_h,
			reverse = false,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "oe",
			to = "oh",
			not_surrounded_by = _vowels_h,
			reverse = false,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ae",
			to = "ah",
			not_surrounded_by = _vowels_h,
			reverse = false,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ah",
			to = "oh",
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "a",
			to = "ah",
			not_surrounded_by = _vowels_h,
			reverse = false,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "o",
			to = "oh",
			not_surrounded_by = _vowels_h,
			reverse = false,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "u",
			to = "uh",
			not_surrounded_by = _vowels_h,
			reverse = false,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "ah",
			to = "a",
			not_surrounded_by = _vowels,
			reverse = false,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "oh",
			to = "o",
			not_surrounded_by = _vowels,
			reverse = false,
			replacer = replace
		});
		add(new OnomasticsEvolutionAsset
		{
			from = "uh",
			to = "u",
			not_surrounded_by = _vowels,
			reverse = false,
			replacer = replace
		});
	}

	public static bool replace(OnomasticsEvolutionAsset pAsset, ref string pReplace)
	{
		if (!pReplace.Contains(pAsset.from))
		{
			return false;
		}
		int random = pReplace.AllIndexesOf(pAsset.from).GetRandom();
		char c = ((random > 0) ? pReplace[random - 1] : ' ');
		char c2 = ((random + pAsset.from.Length < pReplace.Length) ? pReplace[random + pAsset.from.Length] : ' ');
		char c3 = pAsset.to.First();
		char c4 = pAsset.to.Last();
		if (c == c4)
		{
			return false;
		}
		if (c == c3)
		{
			return false;
		}
		if (c2 == c3)
		{
			return false;
		}
		if (c2 == c4)
		{
			return false;
		}
		char[] not_surrounded_by = pAsset.not_surrounded_by;
		if (not_surrounded_by != null && not_surrounded_by.Contains(c))
		{
			return false;
		}
		char[] not_surrounded_by2 = pAsset.not_surrounded_by;
		if (not_surrounded_by2 != null && not_surrounded_by2.Contains(c2))
		{
			return false;
		}
		pReplace = pReplace.Remove(random, pAsset.from.Length).Insert(random, pAsset.to);
		return true;
	}

	public static bool replace_in_end(OnomasticsEvolutionAsset pAsset, ref string pReplace)
	{
		if (!pReplace.Contains(pAsset.from))
		{
			return false;
		}
		int num = pReplace.LastIndexOf(pAsset.from);
		if (num + pAsset.from.Length != pReplace.Length)
		{
			return false;
		}
		char c = ((num > 0) ? pReplace[num - 1] : ' ');
		char c2 = ((num + pAsset.from.Length < pReplace.Length) ? pReplace[num + pAsset.from.Length] : ' ');
		char c3 = pAsset.to.First();
		char c4 = pAsset.to.Last();
		if (c == c4)
		{
			return false;
		}
		if (c == c3)
		{
			return false;
		}
		if (c2 == c3)
		{
			return false;
		}
		if (c2 == c4)
		{
			return false;
		}
		char[] not_surrounded_by = pAsset.not_surrounded_by;
		if (not_surrounded_by != null && not_surrounded_by.Contains(c))
		{
			return false;
		}
		char[] not_surrounded_by2 = pAsset.not_surrounded_by;
		if (not_surrounded_by2 != null && not_surrounded_by2.Contains(c2))
		{
			return false;
		}
		pReplace = pReplace.Remove(num, pAsset.from.Length).Insert(num, pAsset.to);
		return true;
	}

	public static bool replace_not_in_start(OnomasticsEvolutionAsset pAsset, ref string pReplace)
	{
		if (!pReplace.Contains(pAsset.from))
		{
			return false;
		}
		if (pReplace.IndexOf(pAsset.from) == 0)
		{
			return false;
		}
		return replace(pAsset, ref pReplace);
	}

	public override OnomasticsEvolutionAsset add(OnomasticsEvolutionAsset pAsset)
	{
		pAsset.id = pAsset.from + "_" + pAsset.to;
		t = base.add(pAsset);
		if (t.reverse)
		{
			return add(new OnomasticsEvolutionAsset
			{
				id = t.to + "_" + t.from,
				from = t.to,
				to = t.from,
				not_surrounded_by = t.not_surrounded_by,
				replacer = t.replacer,
				reverse = false
			});
		}
		return t;
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (OnomasticsEvolutionAsset item in list)
		{
			OnomasticsEvolver.add(item);
			if (item.from.Length >= item.to.Length)
			{
				OnomasticsEvolver.add(item);
				OnomasticsEvolver.add(item);
			}
			if (item.from.Length > item.to.Length)
			{
				OnomasticsEvolver.add(item);
				OnomasticsEvolver.add(item);
			}
		}
		OnomasticsEvolver.shuffle();
	}
}
