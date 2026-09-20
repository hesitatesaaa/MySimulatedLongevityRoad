namespace ai.behaviours;

public class BehFinger : BehaviourActionActor
{
	protected GodFinger finger;

	public bool drawing_action;

	public override void prepare(Actor pActor)
	{
		finger = pActor.children_special[0] as GodFinger;
		base.prepare(pActor);
	}
}
