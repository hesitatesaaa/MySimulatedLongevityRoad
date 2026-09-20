namespace ai.behaviours.conditions;

public class CondNoPeace : BehaviourActorCondition
{
	public override bool check(Actor pActor)
	{
		return !WorldLawLibrary.world_law_peaceful_monsters.isEnabled();
	}
}
