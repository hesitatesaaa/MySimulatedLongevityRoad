using System;
using UnityEngine;

public class Crabzilla : BaseActorComponent
{
	internal const float HIGH_HP_THRESHOLD = 0.7f;

	internal const float MED_HP_THRESHOLD = 0.35f;

	private CrabLeg[] list_legs;

	private CrabLegJoint[] list_joints;

	private CrabLimbGroup[] list_limbs;

	private int active_limb = -1;

	public CrabBody mainBody;

	internal const float angle0_min = -20f;

	internal const float angle0_max = 30f;

	public GameObject armTarget;

	public GameObject mouthSprite;

	private SpriteAnimation mouthSpriteAnim;

	private bool _beam_enabled;

	private Vector3 bodyRotationTarget;

	private Vector3 bodyRotation;

	private float moveRotationLimit = 5f;

	private Vector3 bodyPosTarget;

	private Vector3 bodyPos;

	private float bodyPosTimeout;

	public CrabArm arm1;

	public CrabArm arm2;

	public float z_pos = 10f;

	internal override void create(Actor pActor)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		base.create(pActor);
		((Component)this).transform.position = Vector2.op_Implicit(actor.current_position);
		bodyPos = new Vector3(0f, 27.8f, 0f);
		bodyPosTarget = new Vector3(0f, 27.8f, 0f);
		mouthSpriteAnim = mouthSprite.GetComponent<SpriteAnimation>();
		createLimbs();
		ControllableUnit.setControllableCreatureCrabzilla(actor);
		if (Config.isMobile)
		{
			WorldTip.showNow("crabzilla_controls_mobile", pTranslate: true, "top", 8f);
		}
		else
		{
			WorldTip.showNow("crabzilla_controls_pc", pTranslate: true, "top", 8f);
		}
		if (Config.joyControls)
		{
			UltimateJoystick.ResetJoysticks();
		}
		Vector3 position = ((Component)this).transform.position;
		position.z = z_pos;
		((Component)this).transform.position = position;
		actor.current_position = Vector2.op_Implicit(((Component)this).transform.position);
	}

	public bool isBeamEnabled()
	{
		return _beam_enabled;
	}

	internal void legMoved()
	{
		if (!(bodyPosTimeout > 0f))
		{
			bodyPosTarget.y = 27.8f + Randy.randomFloat(-3f, 3f);
		}
	}

	public override void update(float pElapsed)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		if (bodyPosTimeout > 0f)
		{
			bodyPosTimeout -= pElapsed;
		}
		arm1.update(pElapsed);
		arm2.update(pElapsed);
		CrabLeg[] array = list_legs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].update(pElapsed);
		}
		if (isAnyLimbFlickering())
		{
			list_limbs[active_limb].update(pElapsed);
		}
		bool beam_enabled = ControllableUnit.isAttackPressedLeft();
		_beam_enabled = beam_enabled;
		mouthSprite.SetActive(isBeamEnabled());
		if (mouthSprite.gameObject.activeSelf)
		{
			mouthSpriteAnim.update(pElapsed);
			MusicBox.inst.playDrawingSound("event:/SFX/UNIQUE/Crabzilla/CrabzillaVoice", actor.current_position.x, actor.current_position.y);
		}
		Vector2 val = ControllableUnit.getMovementVector();
		if (!ControllableUnit.isMovementActionActive())
		{
			val = Vector2.zero;
		}
		if (val.x > 0f)
		{
			bodyRotationTarget.z = moveRotationLimit;
		}
		else if (val.x < 0f)
		{
			bodyRotationTarget.z = 0f - moveRotationLimit;
		}
		else
		{
			bodyRotationTarget.z = 0f;
		}
		float num = World.world.elapsed * 60f;
		bodyRotation = Vector3.MoveTowards(bodyRotation, bodyRotationTarget, 0.7f * num);
		if (val.y > 0f && bodyRotation.z > moveRotationLimit)
		{
			bodyRotation.z = moveRotationLimit;
		}
		else if (val.y < 0f && bodyRotation.z < 0f - moveRotationLimit)
		{
			bodyRotation.z = 0f - moveRotationLimit;
		}
		bodyPos.z = 0f;
		bodyPosTarget.z = 0f;
		((Component)mainBody).transform.localRotation = Quaternion.Euler(bodyRotation);
		bodyPos = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(bodyPos), Vector2.op_Implicit(bodyPosTarget), 0.7f * num));
		((Component)mainBody).transform.localPosition = bodyPos;
		Vector3 position = ((Component)this).transform.position;
		if (!object.Equals(val, Vector2.zero))
		{
			Vector2 val2 = Vector2.op_Implicit(((Component)this).transform.position);
			val2 = Vector2.MoveTowards(val2, val2 + val * 0.2f * num, 1f * num);
			((Vector3)(ref position))._002Ector(val2.x, val2.y);
			if (position.x < 0f)
			{
				position.x = 0f;
			}
			if (position.y < 0f)
			{
				position.y = 0f;
			}
			if (position.x > (float)MapBox.width)
			{
				position.x = MapBox.width;
			}
			if (position.y > (float)MapBox.height)
			{
				position.y = MapBox.height;
			}
			position.z = z_pos;
		}
		position.x += actor.shake_offset.x;
		position.y += actor.shake_offset.y;
		((Component)this).transform.position = position;
		actor.current_position = Vector2.op_Implicit(((Component)this).transform.position);
		actor.dirty_current_tile = true;
		updateArms();
	}

	private void updateArms()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (Config.joyControls)
		{
			Vector2 val = Vector2.op_Implicit(armTarget.transform.position);
			float joyAxisVerticalRight = ControllableUnit.getJoyAxisVerticalRight();
			float joyAxisHorizontalRight = ControllableUnit.getJoyAxisHorizontalRight();
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(joyAxisHorizontalRight, joyAxisVerticalRight);
			if (!object.Equals(val2, Vector2.zero))
			{
				val = Vector2.MoveTowards(val, val + val2 * 2f, 1f);
				if (Toolbox.DistVec3(Vector2.op_Implicit(val), ((Component)this).transform.position) > 35f)
				{
					val = Vector2.MoveTowards(Vector2.op_Implicit(((Component)this).transform.position), val, 35f);
				}
			}
			armTarget.transform.position = Vector2.op_Implicit(val);
		}
		else
		{
			Vector3 position = Vector2.op_Implicit(World.world.getMousePos());
			armTarget.transform.position = position;
		}
	}

	private void createLimbs()
	{
		list_joints = ((Component)this).GetComponentsInChildren<CrabLegJoint>(false);
		CrabLegJoint[] array = list_joints;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].crabzilla = this;
		}
		list_legs = ((Component)this).GetComponentsInChildren<CrabLeg>(false);
		CrabLeg[] array2 = list_legs;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].crabzilla = this;
		}
		arm1.crabzilla = this;
		arm2.crabzilla = this;
		list_limbs = new CrabLimbGroup[Enum.GetNames(typeof(CrabLimb)).Length];
		for (int j = 0; j < list_limbs.Length; j++)
		{
			list_limbs[j] = new CrabLimbGroup((CrabLimb)j, actor);
		}
		list_limbs.Shuffle();
		array2 = list_legs;
		foreach (CrabLeg obj in array2)
		{
			obj.create();
			obj.update(World.world.delta_time);
		}
		array = list_joints;
		foreach (CrabLegJoint obj2 in array)
		{
			obj2.create();
			obj2.LateUpdate();
		}
		update(World.world.delta_time);
		array2 = list_legs;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].moveLeg();
		}
	}

	internal static bool getHit(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
	{
		Actor a = pSelf.a;
		Crabzilla actorComponent = a.getActorComponent<Crabzilla>();
		if (a.getHealthRatio() > 0.45f)
		{
			return true;
		}
		actorComponent.ShowLimbDamage();
		return true;
	}

	public void ShowLimbDamage()
	{
		if (!isAnyLimbFlickering())
		{
			active_limb++;
			if (active_limb >= list_limbs.Length)
			{
				active_limb = 0;
				list_limbs.Shuffle();
			}
			actor.startShake(0.05f);
			list_limbs[active_limb].showDamage();
		}
	}

	private bool isAnyLimbFlickering()
	{
		if (active_limb == -1)
		{
			return false;
		}
		if (list_limbs[active_limb].IsFlickering())
		{
			return true;
		}
		return false;
	}
}
