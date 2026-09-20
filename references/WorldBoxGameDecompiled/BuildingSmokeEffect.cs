using UnityEngine;

public class BuildingSmokeEffect : BaseBuildingComponent
{
	private float smokeTimer;

	private Vector3 centerTopVec;

	internal override void create(Building pBuilding)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		base.create(pBuilding);
		Sprite val = building.asset.building_sprites.animation_data[0].main[0];
		centerTopVec = default(Vector3);
		ref Vector3 reference = ref centerTopVec;
		Vector2Int pos = building.current_tile.pos;
		reference.x = ((Vector2Int)(ref pos)).x;
		ref Vector3 reference2 = ref centerTopVec;
		pos = building.current_tile.pos;
		float num = ((Vector2Int)(ref pos)).y;
		Rect rect = val.rect;
		reference2.y = num + ((Rect)(ref rect)).height * building.asset.scale_base.y;
	}

	public override void update(float pElapsed)
	{
		if (building.asset.smoke && !building.isUnderConstruction())
		{
			if (smokeTimer > 0f)
			{
				smokeTimer -= Time.deltaTime;
				return;
			}
			smokeTimer = building.asset.smoke_interval;
			World.world.particles_smoke.spawn(centerTopVec.x, centerTopVec.y, pRemoveCooldown: true);
		}
	}
}
