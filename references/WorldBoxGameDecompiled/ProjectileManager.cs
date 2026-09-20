using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : CoreSystemManager<Projectile, ProjectileData>
{
	private const float COLLISION_DISTANCE = 0.25f;

	private readonly Dictionary<Kingdom, bool> _kingdoms = new Dictionary<Kingdom, bool>();

	public ProjectileManager()
	{
		type_id = "projectile";
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		updateProjectiles(pElapsed);
		checkCollision();
		checkDead();
	}

	private void checkCollision()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (list.Count < 2)
		{
			return;
		}
		for (int i = 0; i < list.Count - 1; i++)
		{
			Projectile projectile = list[i];
			if (!projectile.canBeCollided())
			{
				continue;
			}
			for (int j = i + 1; j < list.Count; j++)
			{
				Projectile projectile2 = list[j];
				if (projectile2.canBeCollided() && (projectile.kingdom != projectile2.kingdom || projectile.kingdom.asset.always_attack_each_other))
				{
					Vector3 val = Vector2.op_Implicit(projectile.getTransformedPositionWithHeight());
					Vector3 val2 = Vector2.op_Implicit(projectile2.getTransformedPositionWithHeight());
					if (Vector3.Distance(val, val2) < 0.25f)
					{
						Vector3 val3 = (val + val2) / 2f;
						val3.y += val3.z;
						val3.z = 0f;
						EffectsLibrary.spawnAt("fx_hit", val3, 0.1f);
						projectile2.getCollided(val);
						projectile.getCollided(val2);
					}
				}
			}
		}
	}

	private void updateProjectiles(float pElapsed)
	{
		_kingdoms.Clear();
		List<Projectile> list = base.list;
		for (int i = 0; i < list.Count; i++)
		{
			Projectile projectile = list[i];
			if (!projectile.isTargetReached())
			{
				_kingdoms[projectile.kingdom] = true;
			}
			projectile.update(pElapsed);
		}
	}

	internal bool hasActiveProjectiles(Kingdom pKingdom)
	{
		return _kingdoms.ContainsKey(pKingdom);
	}

	private void checkDead()
	{
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Projectile projectile = list[num];
			if (projectile.isFinished())
			{
				removeObject(projectile);
			}
		}
	}

	public Projectile spawn(BaseSimObject pInitiator, BaseSimObject pTargetObject, string pAssetID, Vector3 pLaunchPosition, Vector3 pTargetPosition, float pTargetZ = 0f, float pStartPosZ = 0.25f, Action pKillAction = null, Kingdom pForcedKingdom = null)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Projectile projectile = newObject();
		if (projectile == null)
		{
			return null;
		}
		projectile.start(pInitiator, pTargetObject, pLaunchPosition, pTargetPosition, pAssetID, pTargetZ, pStartPosZ, 0f, pKillAction, pForcedKingdom);
		return projectile;
	}
}
