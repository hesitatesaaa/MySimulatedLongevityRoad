namespace ai.behaviours;

public class BehDragon : BehaviourActionActor
{
	internal Dragon dragon;

	public override void prepare(Actor pActor)
	{
		base.prepare(pActor);
		dragon = pActor.children_special[0] as Dragon;
	}
}
