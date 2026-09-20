using ai.behaviours;

public class BehBoatCheckExistence : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		if (boat.actor.getHomeBuilding() == null)
		{
			pActor.data.get("existence_check", out var pResult, 0);
			if (pResult == 0)
			{
				pActor.data.set("existence_check", (int)BehaviourActionBase<Actor>.world.getCurWorldTime());
			}
			else if (Date.getMonthsSince(pResult) > 2)
			{
				pActor.getHitFullHealth(AttackType.Explosion);
			}
		}
		else
		{
			pActor.data.removeInt("existence_check");
		}
		return BehResult.Continue;
	}
}
