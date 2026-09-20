using UnityEngine;

public readonly struct ResourceThrowData
{
	public readonly Vector2 position_start;

	public readonly Vector2 position_end;

	public readonly string resource_asset_id;

	public readonly int resource_amount;

	public readonly double start_time;

	public readonly double end_time;

	public readonly long building_target_id;

	public readonly float height;

	public ResourceThrowData(Vector2 pPositionStart, Vector2 pPositionEnd, float pDuration, string pResourceAssetId, int pResourceAmount, long pBuildingTargetId, float pHeight)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		position_start = pPositionStart;
		position_end = pPositionEnd;
		resource_asset_id = pResourceAssetId;
		resource_amount = pResourceAmount;
		building_target_id = pBuildingTargetId;
		height = pHeight;
		start_time = World.world.getCurSessionTime();
		end_time = start_time + (double)pDuration;
	}

	public bool isFinished()
	{
		return World.world.getCurSessionTime() >= end_time;
	}

	public float getRatio()
	{
		return (float)((World.world.getCurSessionTime() - start_time) / (end_time - start_time));
	}
}
