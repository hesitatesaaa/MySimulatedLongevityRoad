using ai.behaviours;

public class BehTrySleep : BehaviourActionActor
{
	private bool _sleep_outside;

	public BehTrySleep(bool pSleepOutside = false)
	{
		_sleep_outside = pSleepOutside;
	}

	public override BehResult execute(Actor pActor)
	{
		float waitTimer = getWaitTimer(pActor);
		pActor.makeSleep(waitTimer);
		if (pActor.hasCity() && !pActor.hasHouse() && pActor.isSapient())
		{
			pActor.changeHappiness("slept_outside");
		}
		return BehResult.Continue;
	}

	private float getWaitTimer(Actor pActor)
	{
		if (!pActor.hasSubspecies())
		{
			return 20f;
		}
		WorldAgeAsset currentAge = BehaviourActionBase<Actor>.world.era_manager.getCurrentAge();
		Subspecies subspecies = pActor.subspecies;
		float num = 0f;
		bool flag = false;
		if (currentAge.flag_winter && subspecies.hasTrait("winter_slumberers"))
		{
			flag = true;
		}
		else if (currentAge.flag_night && subspecies.hasTrait("nocturnal_dormancy"))
		{
			flag = true;
		}
		else if (!currentAge.flag_chaos && subspecies.hasTrait("chaos_driven"))
		{
			flag = true;
		}
		else if (currentAge.flag_light_age && subspecies.hasTrait("circadian_drift"))
		{
			flag = true;
		}
		if (flag)
		{
			num = 100f;
		}
		else
		{
			float pMinInclusive = 20f;
			float pMaxExclusive = 60f;
			if (subspecies.hasTrait("monophasic_sleep"))
			{
				pMinInclusive = 40f;
				pMaxExclusive = 90f;
			}
			num = Randy.randomFloat(pMinInclusive, pMaxExclusive);
			if (subspecies.hasTrait("prolonged_rest"))
			{
				num += Randy.randomFloat(pMinInclusive, pMaxExclusive);
			}
		}
		return num;
	}
}
