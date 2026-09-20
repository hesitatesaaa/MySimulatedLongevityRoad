using ai.behaviours;

public class BehCheckEnemyNotNear : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (Finder.isEnemyNearOnSameIsland(pActor))
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
