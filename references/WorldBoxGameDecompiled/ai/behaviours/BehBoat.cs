namespace ai.behaviours;

public class BehBoat : BehaviourActionActor
{
	internal Boat boat;

	internal void checkHomeDocks(Actor pActor)
	{
		ActorTool.checkHomeDocks(pActor);
	}

	public override void prepare(Actor pActor)
	{
		base.prepare(pActor);
		boat = pActor.getSimpleComponent<Boat>();
	}

	public override BehResult execute(Actor pActor)
	{
		return BehResult.Continue;
	}
}
