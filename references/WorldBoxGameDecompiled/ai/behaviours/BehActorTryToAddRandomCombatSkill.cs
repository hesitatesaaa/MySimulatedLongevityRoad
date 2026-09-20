namespace ai.behaviours;

public class BehActorTryToAddRandomCombatSkill : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!Randy.randomChance(0.15f))
		{
			return BehResult.Stop;
		}
		ActorTrait random = AssetManager.traits.pot_traits_combat.GetRandom();
		pActor.addTrait(random);
		return BehResult.Continue;
	}
}
