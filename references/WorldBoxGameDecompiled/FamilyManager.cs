using System.Collections.Generic;

public class FamilyManager : MetaSystemManager<Family, FamilyData>
{
	public FamilyManager()
	{
		type_id = "family";
	}

	public Family newFamily(Actor pActor, WorldTile pTile, Actor pActor2)
	{
		World.world.game_stats.data.familiesCreated++;
		World.world.map_stats.familiesCreated++;
		Family family = newObject();
		family.newFamily(pActor, pActor2, pTile);
		if (pActor.hasFamily())
		{
			family.saveOriginFamily1(pActor.family.id);
		}
		pActor.setFamily(family);
		if (pActor2 != null)
		{
			if (pActor2.hasFamily())
			{
				family.saveOriginFamily2(pActor2.family.id);
			}
			pActor2.setFamily(family);
		}
		return family;
	}

	public override void removeObject(Family pObject)
	{
		World.world.game_stats.data.familiesDestroyed++;
		World.world.map_stats.familiesDestroyed++;
		base.removeObject(pObject);
	}

	public Family getNearbyFamily(ActorAsset pUnitAsset, WorldTile pTile)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 4, pUnitAsset.family_spawn_radius, pRandom: true))
		{
			if (item.isAlive() && item.hasFamily() && !item.family.isFull() && item.family.isSameSpecies(pUnitAsset.id) && item.current_tile.isSameIsland(pTile))
			{
				return item.family;
			}
		}
		return null;
	}

	protected override void updateDirtyUnits()
	{
		List<Actor> units_only_alive = World.world.units.units_only_alive;
		for (int i = 0; i < units_only_alive.Count; i++)
		{
			Actor actor = units_only_alive[i];
			Family family = actor.family;
			if (family != null && family.isDirtyUnits())
			{
				family.listUnit(actor);
			}
		}
	}
}
