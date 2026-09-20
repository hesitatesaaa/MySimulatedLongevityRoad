namespace ai.behaviours;

public class BehDragonSleep : BehDragon
{
	public override BehResult execute(Actor pActor)
	{
		pActor.setFlip(pFlip: false);
		pActor.data.set("sleepy", 0);
		if (dragon.sleep_time == -1f)
		{
			dragon.sleep_time = Randy.randomFloat(10f, 80f);
		}
		dragon.sleep_time -= BehaviourActionBase<Actor>.world.elapsed;
		if (!pActor.hasMaxHealth() && Randy.randomChance(0.1f))
		{
			pActor.restoreHealth(1);
		}
		if (dragon.sleep_time > 0f)
		{
			return BehResult.RepeatStep;
		}
		dragon.sleep_time = -1f;
		return BehResult.Continue;
	}
}
