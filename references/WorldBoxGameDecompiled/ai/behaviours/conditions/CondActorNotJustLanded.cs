using UnityEngine;

namespace ai.behaviours.conditions;

public class CondActorNotJustLanded : BehaviourActorCondition
{
	public override bool check(Actor pActor)
	{
		pActor.data.get("justLanded", out var pResult, 0);
		bool result = pResult <= 0;
		pResult--;
		pActor.data.set("justLanded", Mathf.Max(pResult, 0));
		return result;
	}
}
