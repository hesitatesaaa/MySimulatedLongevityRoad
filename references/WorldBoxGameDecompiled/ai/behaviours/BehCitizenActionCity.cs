namespace ai.behaviours;

public class BehCitizenActionCity : BehCityActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		uses_cities = true;
	}
}
