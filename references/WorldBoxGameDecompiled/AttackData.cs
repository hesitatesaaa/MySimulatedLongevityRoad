using System;
using UnityEngine;

public readonly struct AttackData
{
	public readonly BaseSimObject initiator;

	public readonly Action kill_action;

	public readonly Kingdom kingdom;

	public readonly WorldTile hit_tile;

	public readonly Vector3 hit_position;

	public readonly Vector3 initiator_position;

	public readonly BaseSimObject target;

	public readonly AttackType attack_type;

	public readonly bool skip_shake;

	public readonly bool metallic_weapon;

	public readonly bool critical;

	public readonly int targets;

	public readonly int critical_damage_multiplier;

	public readonly float area_of_effect;

	public readonly int damage;

	public readonly float damage_range;

	public readonly bool is_projectile;

	public readonly string projectile_id;

	public readonly float knockback;

	public AttackData(BaseSimObject pInitiator, WorldTile pHitTile, Vector3 pHitPosition, Vector3 pInitiatorPosition, BaseSimObject pTarget, Kingdom pKingdom, AttackType pAttackType = AttackType.Other, bool pMetallicWeapon = false, bool pSkipShake = true, bool pProjectile = false, string pProjectileID = "", Action pKillAction = null, float pBonusAreOfEffect = 0f)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		float num = 0f;
		int num2 = 1;
		float num3 = 0.1f;
		int num4 = 1;
		float num5 = 1f;
		float num6 = 1f;
		if (pInitiator != null)
		{
			flag = Randy.randomChance(pInitiator.stats["critical_chance"]);
			num = pInitiator.stats["knockback"];
			num2 = (int)pInitiator.stats["targets"];
			num3 = pInitiator.stats["area_of_effect"];
			num4 = (int)pInitiator.stats["damage"];
			num5 = pInitiator.stats["damage_range"];
			num6 = pInitiator.stats["critical_damage_multiplier"];
		}
		num3 += pBonusAreOfEffect;
		kill_action = pKillAction;
		initiator = pInitiator;
		kingdom = pKingdom;
		hit_tile = pHitTile;
		initiator_position = pInitiatorPosition;
		hit_position = pHitPosition;
		target = pTarget;
		attack_type = pAttackType;
		metallic_weapon = pMetallicWeapon;
		skip_shake = pSkipShake;
		is_projectile = pProjectile;
		projectile_id = pProjectileID;
		targets = num2;
		critical = flag;
		knockback = num;
		area_of_effect = num3;
		damage = num4;
		damage_range = num5;
		critical_damage_multiplier = (int)num6;
	}
}
