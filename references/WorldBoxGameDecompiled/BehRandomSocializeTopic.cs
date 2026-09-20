using ai.behaviours;

public class BehRandomSocializeTopic : BehaviourActionActor
{
	private float _timer_min;

	private float _timer_max;

	private float _chance;

	public BehRandomSocializeTopic(float pMinTimer, float pMaxTimer, float pChance)
	{
		socialize = true;
		_timer_min = pMinTimer;
		_timer_max = pMaxTimer;
		_chance = pChance;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.clearLastTopicSprite();
		if (!pActor.hasSubspecies())
		{
			return BehResult.Stop;
		}
		if (!pActor.subspecies.has_advanced_communication)
		{
			return BehResult.Stop;
		}
		if (!Randy.randomChance(_chance))
		{
			return BehResult.Stop;
		}
		float pValue = Randy.randomFloat(_timer_min, _timer_max);
		pActor.makeWait(pValue);
		return BehResult.Continue;
	}
}
