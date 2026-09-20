public class BehaviourActionCity : BehaviourActionBase<City>
{
	public override bool errorsFound(City pCity)
	{
		if (!pCity.hasZones() || pCity.getPopulationPeople() == 0)
		{
			return true;
		}
		return base.errorsFound(pCity);
	}

	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		uses_kingdoms = true;
		uses_cities = true;
	}
}
