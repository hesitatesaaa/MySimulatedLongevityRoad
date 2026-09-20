using System.Collections.Generic;
using ai.behaviours;

public class TesterBehSpawnRandomPower : TesterBehSpawnPower
{
	private static List<string> events;

	public TesterBehSpawnRandomPower()
	{
		if (events != null)
		{
			return;
		}
		events = new List<string>();
		foreach (GodPower item in AssetManager.powers.list)
		{
			if (item.id[0] != '_' && item.tester_enabled)
			{
				events.Add(item.id);
				if (item.type == PowerActionType.PowerDrawTile)
				{
					events.Add(item.id);
					events.Add(item.id);
					events.Add(item.id);
				}
			}
		}
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		_power = events.GetRandom();
		return base.execute(pObject);
	}
}
