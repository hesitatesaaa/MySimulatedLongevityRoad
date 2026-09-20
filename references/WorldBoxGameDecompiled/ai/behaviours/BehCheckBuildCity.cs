namespace ai.behaviours;

public class BehCheckBuildCity : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.current_tile.zone.hasCity())
		{
			return BehResult.Stop;
		}
		if (!WorldLawLibrary.world_law_kingdom_expansion.isEnabled())
		{
			return BehResult.Stop;
		}
		if (!pActor.current_tile.zone.isGoodForNewCity(pActor))
		{
			return BehResult.Stop;
		}
		City pCity = BehaviourActionBase<Actor>.world.cities.buildNewCity(pActor, pActor.current_zone);
		pActor.joinCity(pCity);
		return BehResult.Continue;
	}
}
