namespace ai.behaviours;

public class BehReplenishNutrition : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.addNutritionFromEating(pActor.getMaxNutrition(), pSetMaxNutrition: false, pSetJustAte: true);
		pActor.countConsumed();
		return BehResult.Continue;
	}
}
