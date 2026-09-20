using UnityEngine;

namespace ai.behaviours;

public class BehThrowResourceAnimation : BehCityActor
{
	private string _resource_id;

	public BehThrowResourceAnimation(string pResourceId)
	{
		_resource_id = pResourceId;
	}

	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		check_building_target_non_usable = true;
		null_check_building_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		Building beh_building_target = pActor.beh_building_target;
		float num = Toolbox.DistTile(pActor.current_tile, beh_building_target.current_tile);
		num = Mathf.Max(num, 1f);
		if (num > 1.5f)
		{
			num = 1.5f;
		}
		if (pActor.is_visible)
		{
			float pDuration = num;
			Vector2 pStart = Vector2.op_Implicit(pActor.getThrowStartPosition());
			Vector2 pEnd = beh_building_target.current_position + beh_building_target.asset.stockpile_center_offset;
			pEnd.x += Randy.randomFloat(-0.1f, 0.1f);
			pEnd.y += Randy.randomFloat(-0.1f, 0.1f);
			BehaviourActionBase<Actor>.world.resource_throw_manager.addNew(pStart, pEnd, pDuration, _resource_id, 1, 2f, beh_building_target);
		}
		return BehResult.Continue;
	}
}
