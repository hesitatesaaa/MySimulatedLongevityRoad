using UnityEngine;
using ai.behaviours;

public class BehSpawnPlotProgressEffect : BehaviourActionActor
{
	private int _amount;

	public BehSpawnPlotProgressEffect(int pAmount = 1)
	{
		_amount = pAmount;
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		_ = pActor.current_tile.zone;
		for (int i = 0; i < _amount; i++)
		{
			Vector3 pPos = Vector2.op_Implicit(pActor.current_position);
			pPos.y += 5f * pActor.actor_scale;
			pPos.y += Randy.randomFloat((0f - pActor.actor_scale) * 3f, pActor.actor_scale * 3f);
			pPos.x += Randy.randomFloat((0f - pActor.actor_scale) * 2f, pActor.actor_scale * 2f);
			_ = (Object)(object)EffectsLibrary.spawnAt("fx_plot_progress", pPos, pActor.actor_scale * 0.8f) == (Object)null;
		}
		return BehResult.Continue;
	}
}
