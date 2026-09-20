using ai.behaviours;

public class BehActorChangeHappiness : BehaviourActionActor
{
	private string _happiness_id;

	public BehActorChangeHappiness(string pID)
	{
		_happiness_id = pID;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.changeHappiness(_happiness_id);
		return BehResult.Continue;
	}
}
