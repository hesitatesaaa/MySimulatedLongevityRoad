using FMOD.Studio;
using UnityEngine;

public class BaseEffect : BaseAnimatedObject
{
	private const int MAP_MARGIN_TOP = 25;

	private const int MAP_OFFSET_BOTTOM_MIN = -50;

	private const int MAP_OFFSET_BOTTOM_MAX = 30;

	internal bool active;

	internal int effectIndex;

	public const int STATE_START = 1;

	public const int STATE_ON_DEATH = 2;

	public const int STATE_KILLED = 3;

	protected float scale;

	protected float alpha;

	public WorldTile tile;

	internal BaseEffectController controller;

	internal int state;

	private double _timestamp_spawned;

	internal SpriteRenderer sprite_renderer;

	internal BaseCallback callback;

	internal int callbackOnFrame = -1;

	internal EventInstance fmod_instance;

	internal Actor attachedToActor;

	public double timestamp_spawned => _timestamp_spawned;

	public override void Awake()
	{
		sprite_renderer = ((Component)this).GetComponent<SpriteRenderer>();
		base.Awake();
	}

	public void activate()
	{
		active = true;
		((Component)this).gameObject.SetActive(true);
		state = 1;
		_timestamp_spawned = Time.time;
		clear();
	}

	internal void attachTo(Actor pActor)
	{
		attachedToActor = pActor;
		updateAttached();
	}

	internal void makeParentController()
	{
		((Component)this).transform.SetParent(((Component)controller).transform, true);
	}

	internal virtual void prepare(WorldTile pTile, float pScale = 0.5f)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		state = 1;
		((Component)this).transform.localEulerAngles = Vector3.zero;
		Vector2Int pos = pTile.pos;
		float num = (float)((Vector2Int)(ref pos)).x + 0.5f;
		pos = pTile.pos;
		current_position = Vector2.op_Implicit(new Vector3(num, (float)((Vector2Int)(ref pos)).y));
		((Component)this).transform.localPosition = Vector2.op_Implicit(current_position);
		setScale(pScale);
		setAlpha(1f);
		resetAnim();
	}

	public void setScale(float pScale)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		scale = pScale;
		if (scale < 0f)
		{
			scale = 0f;
		}
		((Component)this).transform.localScale = new Vector3(pScale, pScale);
	}

	internal virtual void prepare(Vector2 pVector, float pScale = 1f)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		state = 1;
		((Component)this).transform.rotation = Quaternion.identity;
		((Component)this).transform.localPosition = Vector2.op_Implicit(pVector);
		setScale(pScale);
		setAlpha(1f);
		resetAnim();
	}

	protected void setAlpha(float pVal)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		alpha = pVal;
		Color color = sprite_renderer.color;
		color.a = alpha;
		sprite_renderer.color = color;
	}

	internal virtual void prepare()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = new Vector3((float)Randy.randomInt(-50, 30), (float)Randy.randomInt(0, MapBox.height + 25));
		state = 1;
		setAlpha(0f);
	}

	internal virtual void spawnOnTile(WorldTile pTile)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		tile = pTile;
		((Component)this).transform.localPosition = new Vector3(pTile.posV3.x, pTile.posV3.y);
	}

	internal void startToDie()
	{
		state = 2;
	}

	public virtual void kill()
	{
		state = 3;
		controller.killObject(this);
	}

	public void deactivate()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (((EventInstance)(ref fmod_instance)).isValid())
		{
			((EventInstance)(ref fmod_instance)).stop((STOP_MODE)0);
			((EventInstance)(ref fmod_instance)).release();
		}
		active = false;
		((Component)this).transform.SetParent(((Component)this).transform);
		((Component)this).gameObject.SetActive(false);
		clear();
	}

	public void clear()
	{
		tile = null;
		attachedToActor = null;
		callback = null;
		callbackOnFrame = -1;
	}

	public override void update(float pElapsed)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (controller.asset.draw_light_area)
		{
			Vector2 position = Vector2.op_Implicit(((Component)this).transform.position);
			position.x += controller.asset.draw_light_area_offset_x;
			position.y += controller.asset.draw_light_area_offset_y;
			World.world.stack_effects.light_blobs.Add(new LightBlobData
			{
				position = position,
				radius = controller.asset.draw_light_size
			});
		}
		if (!World.world.isPaused() || !DebugConfig.isOn(DebugOption.PauseEffects))
		{
			if (attachedToActor != null)
			{
				updateAttached();
			}
			base.update(pElapsed);
			if (callbackOnFrame != -1 && sprite_animation.currentFrameIndex == callbackOnFrame)
			{
				callback();
				clear();
			}
		}
	}

	private void updateAttached()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!attachedToActor.isAlive())
		{
			kill();
			return;
		}
		sprite_renderer.flipX = attachedToActor.a.flip;
		((Component)this).transform.localScale = attachedToActor.current_scale;
		((Component)this).transform.localPosition = attachedToActor.cur_transform_position;
		((Component)this).transform.eulerAngles = attachedToActor.current_rotation;
	}

	public void setCallback(int pFrame, BaseCallback pCallback)
	{
		callbackOnFrame = pFrame;
		callback = pCallback;
	}

	public bool isKilled()
	{
		return state == 3;
	}
}
