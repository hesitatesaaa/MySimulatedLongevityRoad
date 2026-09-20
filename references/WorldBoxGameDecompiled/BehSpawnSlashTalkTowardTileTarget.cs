using UnityEngine;
using ai.behaviours;

public class BehSpawnSlashTalkTowardTileTarget : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		pActor.spawnSlashTalk(Vector2Int.op_Implicit(pActor.beh_tile_target.pos));
		return BehResult.Continue;
	}
}
