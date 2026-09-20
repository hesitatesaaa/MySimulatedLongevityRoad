namespace ai.behaviours;

public class BehFindBuilding : BehaviourActionActor
{
	private readonly bool _only_non_targeted;

	private readonly bool _only_with_resources;

	private readonly string _type;

	public BehFindBuilding(string pType, bool pOnlyNonTargeted, bool pOnlyWithResources)
	{
		_type = pType;
		_only_non_targeted = pOnlyNonTargeted;
		_only_with_resources = pOnlyWithResources;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.beh_building_target = findBuildingType(pActor, _type);
		if (pActor.beh_building_target != null)
		{
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}

	private Building findBuildingType(Actor pActor, string pType)
	{
		Building result = null;
		var (array, pLength) = Toolbox.getAllChunksFromTile(pActor.current_tile);
		foreach (MapChunk item in array.LoopRandom(pLength))
		{
			foreach (Building item2 in Toolbox.getBuildingsTypeFromChunk(item, pType, _only_non_targeted, _only_with_resources))
			{
				if (item2.current_tile.isSameIsland(pActor.current_tile))
				{
					return item2;
				}
				result = item2;
			}
		}
		return result;
	}
}
