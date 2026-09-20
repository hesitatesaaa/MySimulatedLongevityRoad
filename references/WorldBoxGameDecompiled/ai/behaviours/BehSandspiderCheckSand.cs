using System.Collections.Generic;

namespace ai.behaviours;

public class BehSandspiderCheckSand : BehaviourActionActor
{
	private static List<int> _list_directions = new List<int>(3);

	public override BehResult execute(Actor pActor)
	{
		WorldTile beh_tile_target = pActor.beh_tile_target;
		if (beh_tile_target == null)
		{
			return BehResult.Continue;
		}
		pActor.data.get("changed_direction", out var pResult, false);
		if (!pResult && beh_tile_target.Type.IsType("sand"))
		{
			pActor.data.get("direction", out var pResult2, 0);
			int newDirectionIndex = getNewDirectionIndex(pResult2);
			pActor.data.set("direction", newDirectionIndex);
			pActor.data.set("changed_direction", pData: true);
			return BehResult.RestartTask;
		}
		return BehResult.Continue;
	}

	private static int getNewDirectionIndex(int pOldIndex)
	{
		_list_directions.Clear();
		for (int i = 0; i < Toolbox.directions.Length; i++)
		{
			if (i != pOldIndex)
			{
				_list_directions.Add(i);
			}
		}
		return _list_directions.GetRandom();
	}
}
