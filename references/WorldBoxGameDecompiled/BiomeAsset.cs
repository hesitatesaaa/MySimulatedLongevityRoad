using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

[Serializable]
public class BiomeAsset : Asset, IDescriptionAsset, ILocalizedAsset, IMultiLocalesAsset
{
	public GrowTypeSelector grow_type_selector_trees;

	public GrowTypeSelector grow_type_selector_plants;

	public GrowTypeSelector grow_type_selector_bushes;

	public GrowTypeSelector grow_type_selector_minerals;

	public List<string> pot_sapient_units_spawn;

	public List<string> pot_units_spawn;

	public List<string> pot_trees_spawn;

	public List<string> pot_plants_spawn;

	public List<string> pot_bushes_spawn;

	public List<string> pot_minerals_spawn;

	public bool grow_vegetation_auto;

	public bool grow_minerals_auto;

	public bool pot_spawn_units_auto;

	public bool cold_biome;

	public bool dark_biome;

	public bool spread_ignore_burned_stages;

	public bool spread_biome;

	public bool spread_by_drops_water;

	public bool spread_by_drops_fire;

	public bool spread_by_drops_curse;

	public bool spread_by_drops_blessing;

	public bool spread_by_drops_powerup;

	public bool spread_by_drops_acid;

	public bool spread_by_drops_coffee;

	public bool special_biome;

	[DefaultValue(6)]
	public int grow_strength = 6;

	public string spread_only_in_era;

	public string tile_low;

	public string tile_high;

	[NonSerialized]
	private TopTileType _cached_tile_high;

	[NonSerialized]
	private TopTileType _cached_tile_low;

	public int generator_pot_amount;

	public int generator_max_size;

	public string localized_key;

	public int loot_generation;

	public string[] subspecies_name_suffix;

	public List<string> spawn_trait_actor;

	public List<string> spawn_trait_subspecies;

	public List<string> spawn_trait_subspecies_always;

	public List<string> spawn_trait_culture;

	public List<string> spawn_trait_clan;

	public List<string> spawn_trait_language;

	public List<string> spawn_trait_religion;

	public List<string> evolution_trait_subspecies;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TopTileType getTile(WorldTile pTile)
	{
		return pTile.main_type.rank_type switch
		{
			TileRank.High => getTileHigh(), 
			TileRank.Low => getTileLow(), 
			_ => null, 
		};
	}

	public int getTileCount()
	{
		return 0 + (getTileHigh()?.getCurrentTiles().Count ?? 0) + (getTileLow()?.getCurrentTiles().Count ?? 0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CanBeNull]
	public TopTileType getTileHigh()
	{
		if (_cached_tile_high == null)
		{
			_cached_tile_high = AssetManager.top_tiles.get(tile_high);
		}
		return _cached_tile_high;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CanBeNull]
	public TopTileType getTileLow()
	{
		if (_cached_tile_low == null)
		{
			_cached_tile_low = AssetManager.top_tiles.get(tile_low);
		}
		return _cached_tile_low;
	}

	public void addTree(string pID, int pRateAmount = 1)
	{
		grow_vegetation_auto = true;
		if (pot_trees_spawn == null)
		{
			pot_trees_spawn = new List<string>();
		}
		for (int i = 0; i < pRateAmount; i++)
		{
			pot_trees_spawn.Add(pID);
		}
	}

	public void addPlant(string pID, int pRateAmount = 1)
	{
		grow_vegetation_auto = true;
		if (pot_plants_spawn == null)
		{
			pot_plants_spawn = new List<string>();
		}
		for (int i = 0; i < pRateAmount; i++)
		{
			pot_plants_spawn.Add(pID);
		}
	}

	public void addBush(string pID, int pRateAmount = 1)
	{
		grow_vegetation_auto = true;
		if (pot_bushes_spawn == null)
		{
			pot_bushes_spawn = new List<string>();
		}
		for (int i = 0; i < pRateAmount; i++)
		{
			pot_bushes_spawn.Add(pID);
		}
	}

	public void addMineral(string pID, int pRateAmount = 1)
	{
		grow_minerals_auto = true;
		if (pot_minerals_spawn == null)
		{
			pot_minerals_spawn = new List<string>();
		}
		for (int i = 0; i < pRateAmount; i++)
		{
			pot_minerals_spawn.Add(pID);
		}
	}

	public void addUnit(string pID, int pRateAmount = 1)
	{
		pot_spawn_units_auto = true;
		if (pot_units_spawn == null)
		{
			pot_units_spawn = new List<string>();
		}
		for (int i = 0; i < pRateAmount; i++)
		{
			pot_units_spawn.Add(pID);
		}
	}

	public void addSapientUnit(string pID, int pRateAmount = 1)
	{
		pot_spawn_units_auto = true;
		if (pot_sapient_units_spawn == null)
		{
			pot_sapient_units_spawn = new List<string>();
		}
		for (int i = 0; i < pRateAmount; i++)
		{
			pot_sapient_units_spawn.Add(pID);
		}
	}

	internal void addActorTrait(string pTrait)
	{
		if (spawn_trait_actor == null)
		{
			spawn_trait_actor = new List<string>();
		}
		if (!spawn_trait_actor.Contains(pTrait))
		{
			spawn_trait_actor.Add(pTrait);
		}
	}

	internal void addSubspeciesTrait(string pTrait)
	{
		if (spawn_trait_subspecies == null)
		{
			spawn_trait_subspecies = new List<string>();
		}
		if (!spawn_trait_subspecies.Contains(pTrait))
		{
			spawn_trait_subspecies.Add(pTrait);
		}
	}

	internal void addSubspeciesTraitAlways(string pTrait)
	{
		if (spawn_trait_subspecies_always == null)
		{
			spawn_trait_subspecies_always = new List<string>();
		}
		if (!spawn_trait_subspecies_always.Contains(pTrait))
		{
			spawn_trait_subspecies_always.Add(pTrait);
		}
	}

	internal void addSubspeciesTraitEvolution(string pTrait)
	{
		if (evolution_trait_subspecies == null)
		{
			evolution_trait_subspecies = new List<string>();
		}
		if (!evolution_trait_subspecies.Contains(pTrait))
		{
			evolution_trait_subspecies.Add(pTrait);
		}
	}

	internal void addCultureTrait(string pTrait)
	{
		if (spawn_trait_culture == null)
		{
			spawn_trait_culture = new List<string>();
		}
		if (!spawn_trait_culture.Contains(pTrait))
		{
			spawn_trait_culture.Add(pTrait);
		}
	}

	internal void addLanguageTrait(string pTrait)
	{
		if (spawn_trait_language == null)
		{
			spawn_trait_language = new List<string>();
		}
		if (!spawn_trait_language.Contains(pTrait))
		{
			spawn_trait_language.Add(pTrait);
		}
	}

	internal void addClanTrait(string pTrait)
	{
		if (spawn_trait_clan == null)
		{
			spawn_trait_clan = new List<string>();
		}
		if (!spawn_trait_clan.Contains(pTrait))
		{
			spawn_trait_clan.Add(pTrait);
		}
	}

	internal void addReligionTrait(string pTrait)
	{
		if (spawn_trait_religion == null)
		{
			spawn_trait_religion = new List<string>();
		}
		if (!spawn_trait_religion.Contains(pTrait))
		{
			spawn_trait_religion.Add(pTrait);
		}
	}

	public string getLocaleID()
	{
		return localized_key.Underscore();
	}

	public string getDescriptionID()
	{
		return getLocaleID() + "_description";
	}

	public IEnumerable<string> getLocaleIDs()
	{
		if (LocalizedTextManager.stringExists(getLocaleID() + "_seeds"))
		{
			yield return getLocaleID() + "_seeds";
		}
	}
}
