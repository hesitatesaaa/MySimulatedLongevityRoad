using System.Collections.Generic;
using UnityEngine;

public class SubspeciesManager : MetaSystemManager<Subspecies, SubspeciesData>
{
	public static readonly string[] NAME_ENDINGS = new string[9] { "us", "as", "os", "is", "es", "um", "ys", "bres", "bros" };

	private const float UNSTABLE_GENOME_INTERVAL = 300f;

	private float _timer_unstable_genome;

	public SubspeciesManager()
	{
		type_id = "subspecies";
	}

	public Subspecies newSpecies(ActorAsset pAsset, WorldTile pTile, bool pMutation = false)
	{
		World.world.game_stats.data.subspeciesCreated++;
		World.world.map_stats.subspeciesCreated++;
		Subspecies subspecies = newObject();
		subspecies.newSpecies(pAsset, pTile, pMutation);
		addRandomTraitFromBiomeToSubspecies(subspecies, pTile);
		addTraitsFromBiomeToSubspecies(subspecies, pTile);
		return subspecies;
	}

	protected override void finishDirtyUnits()
	{
		base.finishDirtyUnits();
		cacheCounters();
	}

	private void cacheCounters()
	{
		foreach (Subspecies item in list)
		{
			item.cacheCounters();
		}
	}

	public void addRandomTraitFromBiomeToSubspecies(Subspecies pSubspecies, WorldTile pTile)
	{
		pSubspecies.addRandomTraitFromBiome(pTile, pTile.Type.biome_asset?.spawn_trait_subspecies, AssetManager.subspecies_traits);
	}

	public void addTraitsFromBiomeToSubspecies(Subspecies pSubspecies, WorldTile pTile)
	{
		pSubspecies.addTraitFromBiome(pTile, pTile.Type.biome_asset?.spawn_trait_subspecies_always, AssetManager.subspecies_traits);
	}

	public override void removeObject(Subspecies pObject)
	{
		World.world.game_stats.data.subspeciesExtinct++;
		World.world.map_stats.subspeciesExtinct++;
		base.removeObject(pObject);
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		if (!World.world.isPaused())
		{
			_timer_unstable_genome -= pElapsed;
			if (_timer_unstable_genome <= 0f)
			{
				_timer_unstable_genome = Randy.randomFloat(300f, 600f);
				checkSpecialTraits();
			}
		}
	}

	private void checkSpecialTraits()
	{
		foreach (Subspecies item in list)
		{
			if (item.hasTrait("unstable_genome"))
			{
				item.unstableGenomeEvent();
			}
		}
	}

	protected override void updateDirtyUnits()
	{
		for (int i = 0; i < World.world.units.units_only_dying.Count; i++)
		{
			Actor actor = World.world.units.units_only_dying[i];
			if (actor.subspecies != null)
			{
				actor.subspecies.preserveAlive();
			}
		}
		List<Actor> units_only_alive = World.world.units.units_only_alive;
		for (int j = 0; j < units_only_alive.Count; j++)
		{
			Actor actor2 = units_only_alive[j];
			Subspecies subspecies = actor2.subspecies;
			if (subspecies != null && subspecies.isDirtyUnits())
			{
				subspecies.listUnit(actor2);
			}
		}
	}

	public override void clear()
	{
		base.clear();
		_timer_unstable_genome = Randy.randomFloat(300f, 600f);
	}

	public Subspecies getNearbySpecies(ActorAsset pAsset, WorldTile pTile, out Actor pSubspeciesActor, bool pLookForSapientSubspecies = false, bool pStopAtFirst = false)
	{
		Subspecies result = null;
		pSubspeciesActor = null;
		using ListPool<Actor> listPool = new ListPool<Actor>(pAsset.units);
		string.IsNullOrEmpty(pTile.Type.biome_id);
		float num = pAsset.species_spawn_radius;
		for (int i = 0; i < listPool.Count; i++)
		{
			Actor actor = listPool[i];
			if (!actor.isAlive() || !actor.hasSubspecies() || !actor.subspecies.isSpecies(pAsset.id) || (pLookForSapientSubspecies && !actor.subspecies.isSapient()))
			{
				continue;
			}
			float num2 = Toolbox.DistTile(pTile, actor.current_tile);
			if (!(num2 > (float)pAsset.species_spawn_radius) && num2 < num)
			{
				num = num2;
				result = actor.subspecies;
				pSubspeciesActor = actor;
				if (pStopAtFirst && num < 10f)
				{
					break;
				}
			}
		}
		return result;
	}

	public Sprite getTopicSprite()
	{
		return list.GetRandom()?.getActorAsset().getSpriteIcon();
	}
}
