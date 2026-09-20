namespace ai.behaviours;

public class BehBeeReturnHome : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		Building homeBuilding = pActor.getHomeBuilding();
		if (homeBuilding.isRekt())
		{
			return BehResult.Stop;
		}
		if (Toolbox.DistTile(pActor.current_tile, homeBuilding.current_tile) > 3f)
		{
			return BehResult.Stop;
		}
		if (pActor.data.pollen == 3 && pActor.current_tile.building == homeBuilding)
		{
			pActor.data.pollen = 0;
			if (pActor.isKingdomCiv())
			{
				pActor.addToInventory("honey", 1);
			}
			else
			{
				homeBuilding.component_beehive.addHoney();
			}
			pActor.timer_action = 3f;
		}
		return BehResult.Continue;
	}
}
