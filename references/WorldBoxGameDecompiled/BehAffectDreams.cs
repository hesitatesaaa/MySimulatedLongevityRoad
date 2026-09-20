using ai.behaviours;

public class BehAffectDreams : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		Actor randomDreamingActor = getRandomDreamingActor(pActor);
		if (randomDreamingActor == null)
		{
			return BehResult.Stop;
		}
		randomDreamingActor.tryToConvertActorToMetaFromActor(pActor);
		return BehResult.Continue;
	}

	private Actor getRandomDreamingActor(Actor pActor)
	{
		BehaviourActionBase<Actor>.world.units.checkSleepingUnits();
		if (BehaviourActionBase<Actor>.world.units.cached_sleeping_units.Count == 0)
		{
			return null;
		}
		foreach (Actor item in BehaviourActionBase<Actor>.world.units.cached_sleeping_units.LoopRandom())
		{
			if (item.isAlive() && item.hasSubspecies() && item.hasStatus("sleeping") && (item.subspecies.has_advanced_memory || item.subspecies.has_advanced_communication))
			{
				return item;
			}
		}
		return null;
	}
}
