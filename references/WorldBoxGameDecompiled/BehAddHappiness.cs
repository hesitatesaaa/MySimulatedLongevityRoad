using ai.behaviours;

public class BehAddHappiness : BehaviourActionActor
{
	private string _happiness_id;

	public BehAddHappiness(string pHappinessID)
	{
		_happiness_id = pHappinessID;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.a.changeHappiness(_happiness_id);
		return BehResult.Continue;
	}
}
