namespace ai.behaviours.conditions;

public class CondCurrentTileNoOtherUnits : BehaviourActorCondition
{
	public override bool check(Actor pActor)
	{
		return pActor.current_tile.countUnits() <= 1;
	}
}
