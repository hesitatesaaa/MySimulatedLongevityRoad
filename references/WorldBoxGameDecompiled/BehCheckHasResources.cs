using ai.behaviours;

public class BehCheckHasResources : BehaviourActionActor
{
	public override BehResult execute(Actor pObject)
	{
		if (pObject.isCarryingResources())
		{
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
