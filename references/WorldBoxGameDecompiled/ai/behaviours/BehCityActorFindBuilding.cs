namespace ai.behaviours;

public class BehCityActorFindBuilding : BehCityActor
{
	private string _type;

	private string[] _types;

	private bool _only_free_tile;

	public BehCityActorFindBuilding(string pType, bool pFreeTile = true)
	{
		_type = pType;
		_only_free_tile = pFreeTile;
		if (pType.Contains(","))
		{
			_types = pType.Split(',');
		}
	}

	public override BehResult execute(Actor pActor)
	{
		if (_types != null)
		{
			_type = _types.GetRandom();
		}
		pActor.beh_building_target = ActorTool.findNewBuildingTarget(pActor, _type);
		return BehResult.Continue;
	}
}
