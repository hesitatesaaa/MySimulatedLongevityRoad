namespace ai.behaviours;

public class BehFamilyGroupNew : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasFamily())
		{
			return BehResult.Stop;
		}
		Actor nearbySameSpecies = getNearbySameSpecies(pActor, pActor.asset, pActor.current_tile);
		if (nearbySameSpecies == null)
		{
			return BehResult.Stop;
		}
		BehaviourActionBase<Actor>.world.families.newFamily(pActor, pActor.current_tile, nearbySameSpecies);
		return BehResult.Continue;
	}

	public Actor getNearbySameSpecies(Actor pActor, ActorAsset pUnitAsset, WorldTile pTile)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 4, pUnitAsset.family_spawn_radius, pRandom: true))
		{
			if (item != pActor && item.current_tile.isSameIsland(pTile) && !item.hasFamily() && item.isSameSpecies(pUnitAsset.id) && item.isSameSubspecies(pActor.subspecies))
			{
				return item;
			}
		}
		return null;
	}
}
