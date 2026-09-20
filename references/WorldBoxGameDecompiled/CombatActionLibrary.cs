using System;
using UnityEngine;
using ai;

public class CombatActionLibrary : AssetLibrary<CombatActionAsset>
{
	public static CombatActionAsset combat_attack_melee;

	public static CombatActionAsset combat_attack_range;

	public static CombatActionAsset combat_cast_spell;

	public static CombatActionAsset combat_action_deflect;

	public static CombatActionAsset combat_action_dash;

	public static CombatActionAsset combat_action_backstep;

	public override void init()
	{
		base.init();
		combat_attack_melee = add(new CombatActionAsset
		{
			id = "combat_attack_melee",
			play_unit_attack_sounds = true,
			rate = 6,
			action = attackMeleeAction,
			basic = true
		});
		combat_attack_range = add(new CombatActionAsset
		{
			id = "combat_attack_range",
			play_unit_attack_sounds = true,
			rate = 6,
			action = attackRangeAction,
			basic = true
		});
		combat_cast_spell = add(new CombatActionAsset
		{
			id = "combat_cast_spell",
			play_unit_attack_sounds = true,
			cost_stamina = 5,
			is_spell_use = true,
			rate = 3,
			action = tryToCastSpell
		});
		combat_action_deflect = add(new CombatActionAsset
		{
			id = "combat_deflect_projectile",
			cost_stamina = 5,
			chance = 0.2f,
			pools = new CombatActionPool[1],
			action_actor = doDeflect
		});
		add(new CombatActionAsset
		{
			id = "combat_dodge",
			chance = 0.2f,
			cost_stamina = 5,
			action_actor = doDodgeAction,
			pools = AssetLibrary<CombatActionAsset>.a<CombatActionPool>(CombatActionPool.BEFORE_HIT)
		});
		add(new CombatActionAsset
		{
			id = "combat_block",
			chance = 0.2f,
			cost_stamina = 5,
			cooldown = 0.5f,
			action_actor = doBlockAction,
			pools = AssetLibrary<CombatActionAsset>.a<CombatActionPool>(CombatActionPool.BEFORE_HIT_BLOCK)
		});
		add(new CombatActionAsset
		{
			id = "combat_random_jump",
			cost_stamina = 5,
			cooldown = 2f
		});
		combat_action_dash = add(new CombatActionAsset
		{
			id = "combat_dash",
			cost_stamina = 10,
			chance = 0.2f,
			cooldown = 2f,
			action_actor_target_position = doDashAction,
			pools = AssetLibrary<CombatActionAsset>.a<CombatActionPool>(CombatActionPool.BEFORE_ATTACK_MELEE)
		});
		combat_action_backstep = add(new CombatActionAsset
		{
			id = "combat_backstep",
			cost_stamina = 10,
			chance = 0.2f,
			cooldown = 1f,
			can_do_action = delegate(Actor pSelf, BaseSimObject pAttackTarget)
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				if (pSelf.current_tile.Type.block)
				{
					return false;
				}
				float num = Toolbox.SquaredDistVec2Float(pSelf.current_position, pAttackTarget.current_position);
				float num2 = pSelf.getAttackRangeSquared() * 0.5f;
				return (num < num2) ? true : false;
			},
			action_actor_target_position = doBackstepAction,
			pools = AssetLibrary<CombatActionAsset>.a<CombatActionPool>(CombatActionPool.BEFORE_ATTACK_RANGE)
		});
		add(new CombatActionAsset
		{
			id = "combat_throw_bomb",
			cost_stamina = 5,
			chance = 0.2f,
			cooldown = 8f,
			action_actor_target_position = doThrowBombAction,
			can_do_action = delegate(Actor pSelf, BaseSimObject pAttackTarget)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				float num = Toolbox.SquaredDistVec2Float(pSelf.current_position, pAttackTarget.current_position);
				return num > 36f && num < 2500f;
			},
			pools = AssetLibrary<CombatActionAsset>.a<CombatActionPool>(CombatActionPool.BEFORE_ATTACK_MELEE, CombatActionPool.BEFORE_ATTACK_RANGE)
		});
		add(new CombatActionAsset
		{
			id = "combat_throw_torch",
			cost_stamina = 30,
			chance = 0.2f,
			cooldown = 8f,
			action_actor_target_position = doThrowTorchAction,
			can_do_action = delegate(Actor pSelf, BaseSimObject pAttackTarget)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				float num = Toolbox.SquaredDistVec2Float(pSelf.current_position, pAttackTarget.current_position);
				return num > 36f && num < 2500f;
			},
			pools = AssetLibrary<CombatActionAsset>.a<CombatActionPool>(CombatActionPool.BEFORE_ATTACK_MELEE, CombatActionPool.BEFORE_ATTACK_RANGE)
		});
	}

	private bool doThrowBombAction(Actor pSelf, Vector2 pTarget, WorldTile pTile = null)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		ActionLibrary.throwBombAtTile(pSelf, pTile);
		pSelf.punchTargetAnimation(Vector2.op_Implicit(pTarget), pFlip: true, pReverse: false, 45f);
		return true;
	}

	private bool doThrowTorchAction(Actor pSelf, Vector2 pTarget, WorldTile pTile = null)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		ActionLibrary.throwTorchAtTile(pSelf, pTile);
		pSelf.punchTargetAnimation(Vector2.op_Implicit(pTarget), pFlip: true, pReverse: false, 45f);
		return true;
	}

	private bool doBackstepAction(Actor pActor, Vector2 pTarget, WorldTile pTile = null)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		float pForceAmountDirection = 5f;
		float pForceHeight = 1.2f;
		Vector2 current_position = pActor.current_position;
		pActor.punchTargetAnimation(Vector2.op_Implicit(pTarget), pFlip: false, pReverse: false, -20f);
		pActor.calculateForce(current_position.x, current_position.y, pTarget.x, pTarget.y, pForceAmountDirection, pForceHeight);
		Vector2 current_position2 = pActor.current_position;
		current_position2.y += pActor.getHeight();
		BaseEffect baseEffect = EffectsLibrary.spawnAt("fx_dodge", current_position2, pActor.actor_scale);
		if ((Object)(object)baseEffect != (Object)null)
		{
			((Component)baseEffect).transform.rotation = Toolbox.getEulerAngle(current_position, pTarget);
		}
		return true;
	}

	private bool doDashAction(Actor pActor, Vector2 pTarget, WorldTile pTile = null)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		float pForceAmountDirection = 5f;
		float pForceHeight = 1.2f;
		Vector2 current_position = pActor.current_position;
		pActor.punchTargetAnimation(Vector2.op_Implicit(pTarget), pFlip: true, pReverse: false, 50f);
		pActor.addStatusEffect("dash", 0f, pColorEffect: false);
		pActor.calculateForce(pTarget.x, pTarget.y, current_position.x, current_position.y, pForceAmountDirection, pForceHeight);
		Vector2 current_position2 = pActor.current_position;
		current_position2.y += pActor.getHeight();
		BaseEffect baseEffect = EffectsLibrary.spawnAt("fx_dodge", current_position2, pActor.actor_scale);
		if ((Object)(object)baseEffect != (Object)null)
		{
			((Component)baseEffect).transform.rotation = Toolbox.getEulerAngle(current_position, pTarget);
		}
		return true;
	}

	private bool doBlockAction(Actor pActor, AttackData pData, float pTargetX = 0f, float pTargetY = 0f)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		ActorTool.applyForceToUnit(pData, pActor, 0.1f);
		if (!pActor.is_visible)
		{
			return true;
		}
		Vector2 current_position = pActor.current_position;
		Vector2 val = Vector2.op_Implicit(pData.hit_position);
		pActor.punchTargetAnimation(Vector2.op_Implicit(val), pFlip: false, pReverse: false, -40f);
		BaseEffect baseEffect = EffectsLibrary.spawnAt("fx_block", val, pActor.a.actor_scale);
		if ((Object)(object)baseEffect == (Object)null)
		{
			return true;
		}
		((Component)baseEffect).transform.rotation = Toolbox.getEulerAngle(current_position.x, current_position.y, val.x, val.y);
		return true;
	}

	private bool doDeflect(Actor pActor, AttackData pData, float pTargetX = 0f, float pTargetY = 0f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(pData.initiator_position);
		pActor.spawnSlashPunch(val);
		pActor.stopMovement();
		pActor.punchTargetAnimation(Vector2.op_Implicit(val), pFlip: true, pActor.hasRangeAttack());
		pActor.startAttackCooldown();
		return true;
	}

	private bool doDodgeAction(Actor pActor, AttackData pData, float pTargetX = 0f, float pTargetY = 0f)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		float num = 3f;
		float pForceHeight = 1.5f;
		Vector2 val = Vector2.op_Implicit(pActor.cur_transform_position);
		Vector2 val2 = Vector2.op_Implicit(pData.initiator_position);
		Vector2 pVector = val - val2;
		Vector2 val3 = ((!Randy.randomBool()) ? (val + Toolbox.rotateVector(pVector, -90f) * num) : (val + Toolbox.rotateVector(pVector, 90f) * num));
		pActor.calculateForce(val.x, val.y, val3.x, val3.y, num, pForceHeight);
		pActor.addStatusEffect("dodge", 0f, pColorEffect: false);
		pActor.punchTargetAnimation(Vector2.op_Implicit(val), pFlip: false, pReverse: false, -60f);
		Vector2 current_position = pActor.current_position;
		current_position.y += pActor.getHeight();
		BaseEffect baseEffect = EffectsLibrary.spawnAt("fx_dodge", current_position, pActor.actor_scale);
		if ((Object)(object)baseEffect != (Object)null)
		{
			((Component)baseEffect).transform.rotation = Toolbox.getEulerAngle(val, val3);
		}
		return true;
	}

	public bool attackRangeAction(AttackData pData)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		Actor actor = pData.initiator.a;
		BaseSimObject target = pData.target;
		string projectile_id = pData.projectile_id;
		_ = actor.actor_scale;
		float scaleMod = actor.getScaleMod();
		float num = actor.stats["size"];
		int num2 = (int)actor.stats["projectiles"];
		Vector2 val;
		if (target == null)
		{
			val = Vector2.op_Implicit(pData.hit_position);
		}
		else
		{
			val = getAttackTargetPosition(pData);
			val.y += 0.2f * scaleMod;
		}
		float num3 = actor.stats["accuracy"];
		float pMaxExclusive = Toolbox.DistVec2Float(actor.current_position, val) / num3 * 0.25f;
		pMaxExclusive = Randy.randomFloat(0f, pMaxExclusive);
		pMaxExclusive = Mathf.Clamp(pMaxExclusive, 0f, 2f);
		float pStartPosZ = 0.6f * scaleMod;
		float pTargetZ = 0f;
		float value = 0f;
		Vector2 val2 = default(Vector2);
		for (int i = 0; i < num2; i++)
		{
			((Vector2)(ref val2))._002Ector(val.x, val.y);
			if (num3 < 10f)
			{
				Vector2 innacuracyVector = getInnacuracyVector(num3);
				innacuracyVector *= pMaxExclusive;
				val2 += innacuracyVector;
			}
			Vector3 newPoint = Toolbox.getNewPoint(actor.current_position.x, actor.current_position.y, val2.x, val2.y, num * scaleMod);
			newPoint.y += actor.getHeight();
			if (target != null && target.isInAir())
			{
				pTargetZ = target.getHeight();
			}
			value = World.world.projectiles.spawn(actor, target, projectile_id, newPoint, Vector2.op_Implicit(val2), pTargetZ, pStartPosZ, pData.kill_action, pData.kingdom).getLaunchAngle();
		}
		actor.spawnSlash(val, null, 2f, pTargetZ, 0f, value);
		return true;
	}

	public Vector2 getInnacuracyVector(float pAccuracyStat)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f * (10f - pAccuracyStat) / 10f;
		float num2 = (float)((double)(Randy.random() * 2f) * Math.PI);
		return new Vector2(num * (float)Math.Cos(num2), num * (float)Math.Sin(num2));
	}

	public static bool tryToCastSpell(AttackData pData)
	{
		Actor actor = pData.initiator.a;
		BaseSimObject baseSimObject = pData.target;
		SpellAsset randomSpell = actor.getRandomSpell();
		if (!actor.hasEnoughMana(randomSpell.cost_mana))
		{
			return false;
		}
		if (!Randy.randomChance(randomSpell.chance + randomSpell.chance * actor.stats["skill_spell"]))
		{
			return false;
		}
		if (randomSpell.cast_target == CastTarget.Himself)
		{
			baseSimObject = actor;
		}
		if (randomSpell.cast_entity == CastEntity.BuildingsOnly)
		{
			if (baseSimObject.isActor())
			{
				return false;
			}
		}
		else if (randomSpell.cast_entity == CastEntity.UnitsOnly && baseSimObject.isBuilding())
		{
			return false;
		}
		if (randomSpell.health_ratio > 0f)
		{
			float healthRatio = actor.getHealthRatio();
			if (randomSpell.health_ratio <= healthRatio)
			{
				return false;
			}
		}
		if (randomSpell.min_distance > 0f && (float)Toolbox.SquaredDistTile(actor.current_tile, baseSimObject.current_tile) < randomSpell.min_distance * randomSpell.min_distance)
		{
			return false;
		}
		bool flag = false;
		if (randomSpell.action != null)
		{
			flag = randomSpell.action.RunAnyTrue(actor, baseSimObject, baseSimObject.current_tile);
		}
		if (flag)
		{
			actor.doCastAnimation();
			actor.addStatusEffect("recovery_spell");
		}
		return flag;
	}

	public bool attackMeleeAction(AttackData pData)
	{
		AttackDataResult attackDataResult = MapBox.newAttack(pData);
		if (pData.initiator.a.is_visible && EffectsLibrary.canShowSlashEffect())
		{
			showMeleeSlashAttack(pData);
		}
		pData.kill_action?.Invoke();
		return attackDataResult.state == ApplyAttackState.Hit;
	}

	public void showMeleeSlashAttack(AttackData pData)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		pData.initiator.a.spawnSlash(Vector2.op_Implicit(pData.hit_position));
	}

	public Vector2 getAttackTargetPosition(AttackData pData)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		BaseSimObject target = pData.target;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(pData.hit_position.x, pData.hit_position.y);
		if (target == null)
		{
			return val;
		}
		float num = target.stats["size"];
		if (target.isActor() && target.a.is_moving && target.isFlying())
		{
			val = Vector2.MoveTowards(val, target.a.next_step_position, num * 3f);
			return val;
		}
		return val;
	}
}
