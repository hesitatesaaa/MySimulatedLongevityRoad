namespace ai.behaviours;

public class BehBoatFindTargetForTrade : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		Docks dockTradeTarget = ActorTool.getDockTradeTarget(pActor);
		if (dockTradeTarget != null)
		{
			pActor.beh_building_target = dockTradeTarget.building;
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
