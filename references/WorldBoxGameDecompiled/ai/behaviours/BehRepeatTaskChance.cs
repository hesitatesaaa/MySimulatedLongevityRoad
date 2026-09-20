namespace ai.behaviours;

public class BehRepeatTaskChance : BehaviourActionActor
{
	private float chance;

	public BehRepeatTaskChance(float pChance = 0.5f)
	{
		chance = pChance;
	}

	public override BehResult execute(Actor pActor)
	{
		if (Randy.randomChance(chance))
		{
			return BehResult.RestartTask;
		}
		return BehResult.Continue;
	}
}
