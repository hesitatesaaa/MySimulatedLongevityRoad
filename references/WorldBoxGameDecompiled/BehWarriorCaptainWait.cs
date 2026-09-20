using ai.behaviours;

public class BehWarriorCaptainWait : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.isArmyGroupLeader())
		{
			return BehResult.Stop;
		}
		Army army = pActor.army;
		WorldTile current_tile = pActor.current_tile;
		int num = 0;
		foreach (Actor unit in army.units)
		{
			if (Toolbox.SquaredDist(current_tile.posV3.x, current_tile.posV3.y, unit.current_tile.x, unit.current_tile.y) < 100f)
			{
				num++;
			}
		}
		float num2 = 2f;
		float num3 = (float)num / (float)army.units.Count;
		if (num3 < 0.2f)
		{
			num2 = 13f;
		}
		else if (num3 < 0.4f)
		{
			num2 = 7f;
		}
		else if (num3 < 0.6f)
		{
			num2 = 4f;
		}
		pActor.timer_action = Randy.randomFloat(num2, num2 * 2f);
		return BehResult.Continue;
	}
}
