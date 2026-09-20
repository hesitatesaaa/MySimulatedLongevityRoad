using ai.behaviours;

public class TesterBehChangeWorldLaw : BehaviourActionTester
{
	private string world_law;

	private bool value;

	public TesterBehChangeWorldLaw(string pWorldLaw, bool pValue)
	{
		world_law = pWorldLaw;
		value = pValue;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		BehaviourActionBase<AutoTesterBot>.world.world_laws.dict[world_law].boolVal = value;
		return BehResult.Continue;
	}
}
