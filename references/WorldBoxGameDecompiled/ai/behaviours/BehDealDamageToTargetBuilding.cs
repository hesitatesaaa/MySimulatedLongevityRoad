using UnityEngine;

namespace ai.behaviours;

public class BehDealDamageToTargetBuilding : BehaviourActionActor
{
	private const float DAMAGE_MULTIPLIER = 0.1f;

	private float _min;

	private float _max;

	public BehDealDamageToTargetBuilding(float pMinMultiplier, float pMaxMultiplier)
	{
		_min = pMinMultiplier;
		_max = pMaxMultiplier;
		null_check_building_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Building beh_building_target = pActor.beh_building_target;
		float num = Randy.randomFloat(_min, _max);
		if (num <= 0f)
		{
			return BehResult.Continue;
		}
		int num2 = (int)Mathf.Max(pActor.stats["damage"] * num, 1f);
		beh_building_target.getHit(num2);
		pActor.spawnSlash(beh_building_target.current_position);
		return BehResult.Continue;
	}
}
