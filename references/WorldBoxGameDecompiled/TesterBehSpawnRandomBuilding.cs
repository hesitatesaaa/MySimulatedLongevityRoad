using System.Collections.Generic;
using ai.behaviours;

public class TesterBehSpawnRandomBuilding : BehaviourActionTester
{
	private static List<string> assets = new List<string>();

	private static int last_id = 0;

	public TesterBehSpawnRandomBuilding()
	{
		if (assets.Count == 0)
		{
			assets.Add("tree_green_1");
			assets.Add("fruit_bush");
			assets.Add("palm_tree");
			assets.Add("pine_tree");
			assets.Add("tumor");
			assets.Add("golden_brain");
			assets.Add("corrupted_brain");
			assets.Add("beehive");
			assets.Add("ice_tower");
			assets.Add("flame_tower");
			assets.Add("volcano");
			assets.Add("geyser_acid");
			assets.Add("geyser");
		}
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		if (last_id > assets.Count - 1)
		{
			last_id = 0;
			assets.Shuffle();
		}
		string pID = assets[last_id++];
		for (int i = 0; i < 3; i++)
		{
			TileZone random = BehaviourActionBase<AutoTesterBot>.world.zone_calculator.zones.GetRandom();
			BehaviourActionBase<AutoTesterBot>.world.buildings.addBuilding(pID, random.centerTile, pCheckForBuild: true);
		}
		return base.execute(pObject);
	}
}
