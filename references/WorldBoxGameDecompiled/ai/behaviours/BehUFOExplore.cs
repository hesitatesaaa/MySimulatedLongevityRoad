namespace ai.behaviours;

public class BehUFOExplore : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.data.set("exploringTicks", Randy.randomInt(3, 7));
		return BehResult.Continue;
	}
}
