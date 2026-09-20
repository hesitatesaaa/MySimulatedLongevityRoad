using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Serializable]
[Preserve]
public class GameProgressData
{
	[NonSerialized]
	internal List<HashSet<string>> all_hashsets = new List<HashSet<string>>();

	public HashSet<string> achievements;

	public HashSet<string> unlocked_traits_actor;

	public HashSet<string> unlocked_traits_culture;

	public HashSet<string> unlocked_traits_language;

	public HashSet<string> unlocked_traits_subspecies;

	public HashSet<string> unlocked_traits_clan;

	public HashSet<string> unlocked_traits_kingdom;

	public HashSet<string> unlocked_traits_religion;

	public HashSet<string> unlocked_equipment;

	public HashSet<string> unlocked_genes;

	public HashSet<string> unlocked_actors;

	public HashSet<string> unlocked_plots;

	public int saveVersion = 2;

	[Preserve]
	[Obsolete("use .unlocked_traits_actor instead", true)]
	public List<string> unlocked_traits
	{
		set
		{
			if (value != null)
			{
				if (unlocked_traits_actor == null)
				{
					unlocked_traits_actor = new HashSet<string>(value);
				}
				else
				{
					unlocked_traits_actor.UnionWith(value);
				}
			}
		}
	}

	public GameProgressData()
	{
		setDefaultValues();
	}

	public void setDefaultValues()
	{
		if (achievements == null)
		{
			achievements = new HashSet<string>();
		}
		if (unlocked_traits_actor == null)
		{
			unlocked_traits_actor = new HashSet<string>();
		}
		if (unlocked_traits_culture == null)
		{
			unlocked_traits_culture = new HashSet<string>();
		}
		if (unlocked_traits_language == null)
		{
			unlocked_traits_language = new HashSet<string>();
		}
		if (unlocked_traits_subspecies == null)
		{
			unlocked_traits_subspecies = new HashSet<string>();
		}
		if (unlocked_traits_clan == null)
		{
			unlocked_traits_clan = new HashSet<string>();
		}
		if (unlocked_traits_kingdom == null)
		{
			unlocked_traits_kingdom = new HashSet<string>();
		}
		if (unlocked_traits_religion == null)
		{
			unlocked_traits_religion = new HashSet<string>();
		}
		if (unlocked_equipment == null)
		{
			unlocked_equipment = new HashSet<string>();
		}
		if (unlocked_genes == null)
		{
			unlocked_genes = new HashSet<string>();
		}
		if (unlocked_actors == null)
		{
			unlocked_actors = new HashSet<string>();
		}
		if (unlocked_plots == null)
		{
			unlocked_plots = new HashSet<string>();
		}
	}

	public void prepare()
	{
		all_hashsets.Clear();
		all_hashsets.Add(unlocked_traits_actor);
		all_hashsets.Add(unlocked_traits_culture);
		all_hashsets.Add(unlocked_traits_language);
		all_hashsets.Add(unlocked_traits_subspecies);
		all_hashsets.Add(unlocked_traits_clan);
		all_hashsets.Add(unlocked_traits_kingdom);
		all_hashsets.Add(unlocked_traits_religion);
		all_hashsets.Add(unlocked_equipment);
		all_hashsets.Add(unlocked_genes);
		all_hashsets.Add(unlocked_actors);
		all_hashsets.Add(unlocked_plots);
	}
}
