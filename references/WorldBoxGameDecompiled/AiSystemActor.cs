using ai.behaviours;

public class AiSystemActor : AiSystem<Actor, ActorJob, BehaviourTaskActor, BehaviourActionActor, BehaviourActorCondition>
{
	public AiSystemActor(Actor pObject)
		: base(pObject)
	{
	}
}
