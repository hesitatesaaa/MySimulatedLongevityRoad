namespace ai.behaviours;

public class BehCheckForBabiesFromSexualReproduction : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasLover())
		{
			return BehResult.Stop;
		}
		pActor.addAfterglowStatus();
		pActor.lover.addAfterglowStatus();
		pActor.changeHappiness("just_kissed");
		pActor.lover.changeHappiness("just_kissed");
		if (BabyHelper.isMetaLimitsReached(pActor))
		{
			return BehResult.Stop;
		}
		pActor.subspecies.counter_reproduction_acts?.registerEvent();
		checkForBabies(pActor, pActor.lover);
		return BehResult.Continue;
	}

	private void checkForBabies(Actor pParentA, Actor pParentB)
	{
		checkFamily(pParentA, pParentB);
		Subspecies subspecies = pParentA.subspecies;
		Actor actor = null;
		ReproductiveStrategy reproductionStrategy = subspecies.getReproductionStrategy();
		if (subspecies.hasTraitReproductionSexual())
		{
			if (pParentA.isSexFemale())
			{
				actor = pParentA;
			}
			else if (pParentB.isSexFemale())
			{
				actor = pParentB;
			}
		}
		else if (subspecies.hasTraitReproductionSexualHermaphroditic())
		{
			actor = ((!Randy.randomBool()) ? pParentB : pParentA);
		}
		if (actor != null && BabyHelper.canMakeBabies(actor) && (!actor.isSexMale() || !actor.subspecies.hasTraitReproductionSexual()))
		{
			float maturationTimeSeconds = pParentA.getMaturationTimeSeconds();
			switch (reproductionStrategy)
			{
			case ReproductiveStrategy.Egg:
			case ReproductiveStrategy.SpawnUnitImmediate:
				BabyMaker.makeBabiesViaSexual(actor, pParentA, pParentB);
				actor.subspecies.counterReproduction();
				break;
			case ReproductiveStrategy.Pregnancy:
				BabyHelper.babyMakingStart(actor);
				actor.addStatusEffect("pregnant", maturationTimeSeconds);
				actor.subspecies.counterReproduction();
				break;
			}
		}
	}

	private void checkFamily(Actor pActor, Actor pLover)
	{
		bool flag = false;
		if (pActor.hasFamily())
		{
			if (pActor.family != pActor.lover.family)
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			BehaviourActionBase<Actor>.world.families.newFamily(pActor, pActor.current_tile, pLover);
		}
	}
}
