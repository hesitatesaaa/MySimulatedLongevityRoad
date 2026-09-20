public class BehaviourActionKingdom : BehaviourActionBase<Kingdom>
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		uses_kingdoms = true;
		uses_cities = true;
	}
}
