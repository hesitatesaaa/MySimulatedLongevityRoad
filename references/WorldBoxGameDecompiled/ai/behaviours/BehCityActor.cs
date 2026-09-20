namespace ai.behaviours;

public class BehCityActor : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_city = true;
	}
}
