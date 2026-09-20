using System.Collections.Generic;
using UnityEngine;

public class Boulder : BaseEffect
{
	private const float SPEED = 2.5f;

	private const int BOUNCES_AMOUNT = 3;

	private const float SINGLE_BOUNCE_TIMER = 2f;

	private const float BASE_HEIGHT_MULTIPLIER = 10f;

	private const float BASE_LENGTH_MULTIPLIER = 40f;

	private const float INITIAL_ANGLE_RANGE = 200f;

	private const float CHARGE_VECTOR_MULTIPLIER = 0.777f;

	private const float Z_SORTING_FIX = 5f;

	private const int NO_TOUCH_ID = -2;

	private float angle;

	private float angleRotation;

	private float impactEffect;

	public GameObject mainSprite;

	public GameObject shadowSprite;

	private SpriteRenderer shadowRenderer;

	private Transform mainTransform;

	private Transform shadowTransform;

	private Vector2 _previous_bounce_position;

	private List<Vector2> _bounce_positions = new List<Vector2>();

	private int _bounces_left;

	private float _force_timer;

	private static bool _charge_started;

	private static Vector2 _initial_charge_position;

	private static Touch _latest_touch;

	private static int _latest_touch_id = -2;

	public override void Awake()
	{
		base.Awake();
		sprite_renderer = mainSprite.GetComponent<SpriteRenderer>();
		shadowRenderer = shadowSprite.GetComponent<SpriteRenderer>();
		mainTransform = mainSprite.transform;
		shadowTransform = shadowSprite.transform;
	}

	public override void update(float pElapsed)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base.update(pElapsed);
		updateForce(pElapsed);
		if (impactEffect > 0f)
		{
			impactEffect -= pElapsed;
		}
		if (position_height != 0f)
		{
			angle += angleRotation * pElapsed;
			mainTransform.localEulerAngles = new Vector3(0f, 0f, angle);
		}
	}

	private void updateForce(float pElapsed)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		_force_timer -= pElapsed * 2.5f;
		if (_force_timer <= 0f)
		{
			_force_timer = 2f;
			actionLanded();
			return;
		}
		float heightPosition = getHeightPosition();
		Vector2 val = calcCurrentPos();
		setCurrentPosition(val.x, val.y, heightPosition);
		updateCurrentPosition();
	}

	private void updateShadow()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		float num = (position_height / -5f + 10f) / 10f;
		float shadowAlpha = Mathf.Clamp(num, 0.15f, 1f) * 0.3f;
		setShadowAlpha(shadowAlpha);
		float num2 = Mathf.Clamp(num, 0.25f, 0.9f) * 0.3f;
		Vector3 localScale = shadowTransform.localScale;
		((Vector3)(ref localScale)).Set(num2, num2, 1f);
		shadowTransform.localScale = localScale;
	}

	private void setShadowAlpha(float pVal)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		float num = pVal;
		if (num < 0f)
		{
			num = 0f;
		}
		Color color = shadowRenderer.color;
		color.a = num;
		shadowRenderer.color = color;
	}

	private void spawnEffect(string pEffectID)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!(impactEffect > 0f))
		{
			impactEffect = 0.8f;
			Vector3 pPos = Vector2.op_Implicit(current_position);
			pPos.y -= 2f;
			EffectsLibrary.spawnAt(pEffectID, pPos, mainTransform.localScale.x);
		}
	}

	internal void actionLanded()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		_previous_bounce_position = current_position;
		_bounces_left--;
		current_tile = World.world.GetTile((int)((Component)this).transform.localPosition.x, (int)((Component)this).transform.localPosition.y);
		bool flag = true;
		if (current_tile != null && current_tile.Type.lava)
		{
			flag = false;
		}
		if (_bounces_left < 1)
		{
			flag = false;
		}
		if (flag)
		{
			sequencedBounce();
		}
		else
		{
			explosion();
		}
	}

	private void sequencedBounce()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		Vector3 pVec = Vector2.op_Implicit(current_position);
		pVec.y -= 2f;
		EffectsLibrary.spawnExplosionWave(pVec, (float)_bounces_left * 0.14f, 6f);
		World.world.startShake(0.3f, 0.01f, 1f);
		if (!Toolbox.inMapBorder(ref current_position))
		{
			spawnEffect("fx_boulder_impact_water");
		}
		else if (current_tile != null)
		{
			if (current_tile.Type.ocean)
			{
				spawnEffect("fx_boulder_impact_water");
			}
			else
			{
				spawnEffect("fx_boulder_impact");
			}
			World.world.loopWithBrush(current_tile, Brush.get(5), tileDrawBoulder);
			World.world.applyForceOnTile(current_tile, 5, 0.5f, pForceOut: false);
			World.world.conway_layer.checkKillRange(current_tile.pos, 5);
		}
	}

	private void explosion()
	{
		if (current_tile == null || current_tile.Type.ocean)
		{
			spawnEffect("fx_boulder_impact_water");
		}
		else
		{
			spawnEffect("fx_boulder_impact");
		}
		impactEffect = 0f;
		if (Toolbox.inMapBorder(ref current_position))
		{
			MapAction.damageWorld(current_tile, 10, AssetManager.terraform.get("bomb"));
		}
		spawnEffect("fx_explosion_small");
		controller.killObject(this);
	}

	public static bool tileDrawBoulder(WorldTile pTile, string pPowerID)
	{
		pTile.doUnits(delegate(Actor pActor)
		{
			AchievementLibrary.ball_to_ball.checkBySignal(pActor);
			pActor.getHitFullHealth(AttackType.Gravity);
		});
		if (pTile.Type.ocean && Randy.randomChance(0.3f))
		{
			World.world.drop_manager.spawnParabolicDrop(pTile, "rain", 0f, 1f, 30f, 0.7f, 22f);
		}
		if (pTile.Type.lava && Randy.randomChance(0.3f))
		{
			World.world.drop_manager.spawnParabolicDrop(pTile, "lava", 0f, 1f, 30f, 0.7f, 22f);
		}
		MapAction.decreaseTile(pTile, pDamage: true, "destroy");
		return true;
	}

	public void spawnOn(Vector2 pPosition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		_bounce_positions.Clear();
		if (isRandomLaunch(pPosition))
		{
			_force_timer = 1f;
		}
		else
		{
			_force_timer = 2f;
		}
		_bounces_left = 3;
		angle = 0f;
		angleRotation = Randy.randomFloat(-200f, 200f);
		impactEffect = 0f;
		Vector2 val = default(Vector2);
		if (isRandomLaunch(pPosition))
		{
			val.x = Randy.randomFloat(-40f, 40f);
			val.y = Randy.randomFloat(-40f, 40f);
			val = Vector2.ClampMagnitude(val, 40f);
		}
		else
		{
			val = chargeVector(pPosition) * 0.777f;
		}
		_previous_bounce_position = pPosition;
		_previous_bounce_position.y -= getHeightPosition();
		_previous_bounce_position -= val * getBounceProgress();
		for (int i = 0; i < 3; i++)
		{
			int num = i + 1;
			Vector2 item = new Vector2
			{
				x = _previous_bounce_position.x + val.x * (float)num,
				y = _previous_bounce_position.y + val.y * (float)num
			};
			_bounce_positions.Add(item);
		}
		updateCurrentPosition();
		endCharging();
	}

	private void setCurrentPosition(float pX, float pY, float pHeight)
	{
		current_position.x = pX;
		current_position.y = pY;
		position_height = pHeight;
	}

	private void updateCurrentPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)this).transform.localPosition;
		localPosition.x = current_position.x;
		localPosition.y = current_position.y;
		localPosition.z = position_height + 5f;
		((Component)this).transform.localPosition = localPosition;
		Vector3 localPosition2 = mainTransform.localPosition;
		localPosition2.y = position_height;
		mainTransform.localPosition = localPosition2;
		updateShadow();
	}

	private float getBounceProgress()
	{
		return 1f - _force_timer / 2f;
	}

	private float getBounceProgressMirrored()
	{
		return 1f - Mathf.Abs(getBounceProgress() * 2f - 1f);
	}

	private float getHeightProgress()
	{
		return iTween.easeOutQuad(0f, 1f, getBounceProgressMirrored());
	}

	private float getHeightPosition()
	{
		return (float)_bounces_left * getHeightProgress() * 10f;
	}

	private int getCurrentBounceIndex()
	{
		return 3 - _bounces_left;
	}

	private Vector2 getNextBouncePos()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return _bounce_positions[getCurrentBounceIndex()];
	}

	private Vector2 calcCurrentPos()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.Lerp(_previous_bounce_position, getNextBouncePos(), getBounceProgress());
	}

	public static void chargeBoulder(Vector2 pPosition, Touch pTouch = default(Touch))
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_latest_touch = pTouch;
		if (ScrollWindow.isWindowActive())
		{
			endCharging();
		}
		else if (HotkeyLibrary.many_mod.isHolding() || (!InputHelpers.mouseSupported && DebugConfig.isOn(DebugOption.FastSpawn)))
		{
			if (_charge_started)
			{
				endCharging();
			}
			releaseManyBoulders(pPosition);
		}
		else if (isInteractionJustStarted())
		{
			startCharging(pPosition);
		}
		else if (isInteractionJustEnded())
		{
			releaseBoulder();
		}
	}

	private static void startCharging(Vector2 pPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_charge_started = true;
		_initial_charge_position = pPosition;
		_latest_touch_id = ((Touch)(ref _latest_touch)).fingerId;
	}

	private static void endCharging()
	{
		_charge_started = false;
		_latest_touch_id = -2;
	}

	public static void checkRelease()
	{
		if (!_charge_started)
		{
			return;
		}
		if (!isBoulderPowerSelected())
		{
			endCharging();
			return;
		}
		spawnParticles();
		if (isInteractionJustEnded())
		{
			releaseBoulder();
		}
	}

	private static void releaseManyBoulders(Vector2 pPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		_initial_charge_position = pPosition;
		releaseBoulder();
	}

	private static void releaseBoulder()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 pointerPosition = getPointerPosition();
		EffectsLibrary.spawnAt("fx_boulder", pointerPosition, 1f);
	}

	private static void spawnParticles()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 pPosition = Vector2.zero;
		if (getPointerPositionPure(ref pPosition) && !isRandomLaunch(pPosition))
		{
			EffectsLibrary.spawnAt("fx_boulder_charge", pPosition, 1f);
		}
	}

	private static bool isRandomLaunch(Vector2 pPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = chargeVector(pPosition);
		return ((Vector2)(ref val)).magnitude < 1.5f;
	}

	private static bool isBoulderPowerSelected()
	{
		return PowerButtonSelector.instance.selectedButton?.godPower?.id == "bowling_ball";
	}

	private static Vector2 chargeVector(Vector2 pPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _initial_charge_position - pPosition;
	}

	public static Vector2 chargeVector()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return chargeVector(getPointerPosition());
	}

	private static bool isInteractionJustStarted()
	{
		if (_charge_started)
		{
			return false;
		}
		if (InputHelpers.mouseSupported)
		{
			if (Input.GetMouseButtonDown(0))
			{
				return true;
			}
		}
		else if (((Touch)(ref _latest_touch)).fingerId != _latest_touch_id)
		{
			return true;
		}
		return false;
	}

	private static bool isInteractionJustEnded()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		if (InputHelpers.mouseSupported)
		{
			if (Input.GetMouseButtonUp(0))
			{
				return true;
			}
		}
		else if (Input.touchCount == 0 || (int)((Touch)(ref _latest_touch)).phase == 3)
		{
			return true;
		}
		return false;
	}

	private static Vector2 getPointerPosition()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (InputHelpers.mouseSupported)
		{
			return World.world.getMousePos();
		}
		return Vector2.op_Implicit(World.world.camera.ScreenToWorldPoint(Vector2.op_Implicit(((Touch)(ref _latest_touch)).position)));
	}

	private static bool getPointerPositionPure(ref Vector2 pPosition)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (InputHelpers.mouseSupported)
		{
			pPosition = World.world.getMousePos();
			return true;
		}
		if (World.world.player_control.getTouchPos(out var pTouch))
		{
			pPosition = Vector2.op_Implicit(World.world.camera.ScreenToWorldPoint(Vector2.op_Implicit(((Touch)(ref pTouch)).position)));
			return true;
		}
		return false;
	}
}
