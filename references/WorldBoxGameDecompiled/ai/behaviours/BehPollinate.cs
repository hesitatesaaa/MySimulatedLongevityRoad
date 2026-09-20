namespace ai.behaviours;

public class BehPollinate : BehaviourActionActor
{
	public BehPollinate()
	{
		land_if_hovering = true;
	}

	public override BehResult execute(Actor pActor)
	{
		if (!pActor.current_tile.hasBuilding())
		{
			return BehResult.Stop;
		}
		if (pActor.current_tile.building.asset.type == "type_flower")
		{
			pActor.data.pollen++;
			pActor.current_tile.pollinate();
			if (pActor.asset.id != "bee" && pActor.data.pollen >= 10)
			{
				pActor.data.pollen -= 10;
				if (pActor.isKingdomCiv())
				{
					pActor.addToInventory("honey", 1);
				}
			}
		}
		pActor.timer_action = Randy.randomFloat(4f, 10f);
		return BehResult.Continue;
	}
}
