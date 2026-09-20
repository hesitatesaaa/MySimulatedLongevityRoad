namespace ai.behaviours;

public class BehDragonIdle : BehDragon
{
	public override BehResult execute(Actor pActor)
	{
		if (dragon.aggroTargets.Count > 0)
		{
			return BehResult.Continue;
		}
		if (dragon.idle_time == -1f)
		{
			dragon.idle_time = Randy.randomFloat(1f, 3f);
		}
		dragon.idle_time -= BehaviourActionBase<Actor>.world.elapsed;
		if (dragon.idle_time > 0f)
		{
			return BehResult.RepeatStep;
		}
		dragon.idle_time = -1f;
		return BehResult.Continue;
	}
}
