using System;
using System.Collections.Generic;
using System.ComponentModel;

[Serializable]
public class NameGeneratorAsset : Asset
{
	public static string[] vowels_all = new string[6] { "a", "e", "i", "o", "u", "y" };

	public static string[] consonants_all = new string[20]
	{
		"b", "c", "d", "f", "g", "h", "j", "k", "l", "m",
		"n", "p", "q", "r", "s", "t", "v", "w", "x", "z"
	};

	public static string[] consonants_sounds = new string[19]
	{
		"b", "c", "d", "f", "g", "h", "ph", "ch", "k", "l",
		"m", "n", "p", "r", "s", "t", "v", "w", "sh"
	};

	public string[] special1;

	public string[] special2;

	public string[] vowels;

	public string[] consonants;

	public string[] parts;

	public string[] addition_start;

	public string[] addition_ending;

	public List<string> onomastics_templates = new List<string>();

	public List<string> part_groups;

	public List<string> part_groups2;

	public List<string> part_groups3;

	public Dictionary<string, string> dict_parts;

	public bool use_dictionary;

	public List<string[]> templates;

	[DefaultValue(1)]
	public int max_vowels_in_row = 1;

	[DefaultValue(1)]
	public int max_consonants_in_row = 1;

	[DefaultValue(0.5f)]
	public float add_addition_chance = 0.5f;

	public NameGeneratorCheck check;

	public NameGeneratorReplacer replacer;

	public NameGeneratorReplacerKingdom replacer_kingdom;

	public NameGeneratorFinalizer finalizer;

	public bool hasOnomastics()
	{
		return onomastics_templates.Count > 0;
	}

	public void addOnomastic(string pOnomastic)
	{
		if (onomastics_templates == null)
		{
			onomastics_templates = new List<string>();
		}
		onomastics_templates.Add(pOnomastic);
	}

	public void addTemplate(string pTemplateString)
	{
		if (templates == null)
		{
			templates = new List<string[]>();
		}
		string[] item = pTemplateString.Split(',');
		templates.Add(item);
	}

	public void addPartGroup(string pGroupString)
	{
		if (part_groups == null)
		{
			part_groups = new List<string>();
		}
		part_groups.Add(pGroupString);
	}

	public void addPartGroup2(string pGroupString)
	{
		if (part_groups2 == null)
		{
			part_groups2 = new List<string>();
		}
		part_groups2.Add(pGroupString);
	}

	public void addPartGroup3(string pGroupString)
	{
		if (part_groups3 == null)
		{
			part_groups3 = new List<string>();
		}
		part_groups3.Add(pGroupString);
	}

	public void addDictPart(string pID, string pListString)
	{
		if (dict_parts == null)
		{
			dict_parts = new Dictionary<string, string>();
		}
		dict_parts.Add(pID, pListString);
	}
}
