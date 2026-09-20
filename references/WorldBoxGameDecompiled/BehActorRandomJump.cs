using UnityEngine;
using ai.behaviours;

public class BehActorRandomJump : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		float num = Randy.randomFloat(1f, 5f);
		float pForceHeight = Randy.randomFloat(1f, 2f);
		Vector2 current_position = pActor.current_position;
		float degrees = Randy.randomFloat(-180f, 180f);
		Vector2 val = current_position + Toolbox.rotateVector(current_position, degrees) * num;
		pActor.calculateForce(current_position.x, current_position.y, val.x, val.y, num, pForceHeight);
		pActor.punchTargetAnimation(Vector2.op_Implicit(current_position), pFlip: false, pReverse: false, -60f);
		if (pActor.is_visible)
		{
			Vector2 current_position2 = pActor.current_position;
			BaseEffect baseEffect = EffectsLibrary.spawnAt("fx_dodge", current_position2, pActor.actor_scale);
			if ((Object)(object)baseEffect != (Object)null)
			{
				((Component)baseEffect).transform.rotation = Toolbox.getEulerAngle(current_position, val);
			}
		}
		return BehResult.Continue;
	}
}
