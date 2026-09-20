namespace ai.behaviours;

public class BehAntSetup : BehaviourActionActor
{
	private static string[] _ant_tile_types = new string[8] { "deep_ocean", "close_ocean", "shallow_waters", "sand", "soil_low", "soil_high", "hills", "mountains" };

	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("tile_type1", out var pResult, null);
		if (string.IsNullOrEmpty(pResult))
		{
			pResult = getRandomTileType(pActor.current_tile?.Type?.id);
			string randomTileType = getRandomTileType(pResult);
			pActor.data.set("tile_type1", pResult);
			pActor.data.set("tile_type2", randomTileType);
		}
		return BehResult.Continue;
	}

	public static string getRandomTileType(string pExclude = null)
	{
		string random = _ant_tile_types.GetRandom();
		while (random == pExclude)
		{
			random = _ant_tile_types.GetRandom();
		}
		return random;
	}
}
