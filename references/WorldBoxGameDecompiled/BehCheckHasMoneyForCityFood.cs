using ai.behaviours;

public class BehCheckHasMoneyForCityFood : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.canGetFoodFromCity())
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
