using UnityEngine;

namespace ai.behaviours;

public class BehWormDive : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("dive_steps", out var pResult, 0);
		if (--pResult < 1)
		{
			pResult = Randy.randomInt(Randy.randomInt(1, 6), Randy.randomInt(10, 60));
			pActor.data.get("size", out var pResult2, 0);
			pResult2 = Mathf.Clamp((!Randy.randomBool()) ? (--pResult2) : (++pResult2), 0, 2);
			pActor.data.set("size", pResult2);
		}
		pActor.data.set("dive_steps", pResult);
		return BehResult.Continue;
	}
}
