using System.Collections.Generic;
using ai.behaviours;

public class TesterBehPickRandomRace : BehaviourActionTester
{
	private static List<string> assets;

	public TesterBehPickRandomRace()
	{
		if (assets == null)
		{
			assets = new List<string> { "human", "elf", "orc", "dwarf" };
		}
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		pObject.beh_asset_target = assets.GetRandom();
		return base.execute(pObject);
	}
}
