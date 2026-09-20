using UnityEngine;

public class Cloud : BaseEffect
{
	public CloudAsset asset;

	private float speed = 1f;

	public SpriteShadow spriteShadow;

	private float _timer_action_1;

	private float _timer_action_2;

	internal float alive_time;

	private float _fade_multiplier = 0.2f;

	internal float effect_texture_width;

	internal float effect_texture_height;

	private float _lifespan;

	internal override void create()
	{
		base.create();
	}

	internal override void prepare()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		sprite_renderer.sprite = Randy.getRandom(asset.cached_sprites);
		sprite_renderer.flipX = Randy.randomBool();
		speed = Randy.randomFloat(asset.speed_min, asset.speed_max);
		Rect rect = sprite_renderer.sprite.rect;
		effect_texture_width = ((Rect)(ref rect)).width * 0.08f;
		rect = sprite_renderer.sprite.rect;
		effect_texture_height = ((Rect)(ref rect)).height * 0.04f;
		_timer_action_1 = asset.interval_action_1;
		_lifespan = 0f;
		alive_time = 0f;
		base.prepare();
		setAlpha(0f);
	}

	public void setLifespan(float pLifespan)
	{
		_lifespan = pLifespan;
	}

	internal void setType(CloudAsset pAsset)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		asset = pAsset;
		sprite_renderer.color = asset.color;
	}

	internal void setType(string pType)
	{
		CloudAsset cloudAsset = AssetManager.clouds.get(pType);
		if (cloudAsset != null)
		{
			setType(cloudAsset);
		}
	}

	public void spawn(WorldTile pTile, string pType)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (pTile == null)
		{
			setType(pType);
			prepare();
		}
		else
		{
			prepare(pTile.posV3, pType);
		}
	}

	internal void prepare(Vector3 pVec, string pType)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		setType(pType);
		prepare();
		pVec.y -= spriteShadow.offset.y;
		((Component)this).transform.localPosition = pVec;
	}

	internal override void prepare(WorldTile pTile, float pScale = 0.5f)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		prepare();
		((Component)this).transform.localPosition = new Vector3(pTile.posV3.x, pTile.posV3.y);
	}

	public override void update(float pElapsed)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		alive_time += pElapsed;
		if (Config.time_scale_asset.sonic)
		{
			_fade_multiplier = 0.05f;
		}
		else
		{
			_fade_multiplier = 0.2f;
		}
		if (asset.draw_light_area)
		{
			Vector2 position = Vector2.op_Implicit(((Component)this).transform.localPosition);
			position.x += asset.draw_light_area_offset_x;
			position.y += asset.draw_light_area_offset_y;
			World.world.stack_effects.light_blobs.Add(new LightBlobData
			{
				position = position,
				radius = asset.draw_light_size
			});
		}
		if (!World.world.isPaused())
		{
			((Component)this).transform.Translate(speed * pElapsed, 0f, 0f);
			if (asset.cloud_action_1 != null)
			{
				if (_timer_action_1 > 0f)
				{
					_timer_action_1 -= pElapsed;
				}
				else
				{
					_timer_action_1 = asset.interval_action_1;
					asset.cloud_action_1(this);
				}
			}
			if (asset.cloud_action_2 != null)
			{
				if (_timer_action_2 > 0f)
				{
					_timer_action_2 -= pElapsed;
				}
				else
				{
					_timer_action_2 = asset.interval_action_2;
					asset.cloud_action_2(this);
				}
			}
		}
		if (((Component)this).transform.localPosition.x > (float)MapBox.width || (_lifespan > 0f && alive_time > _lifespan))
		{
			startToDie();
		}
		float num = asset.max_alpha;
		if ((Object)(object)World.world.camera != (Object)null && World.world.camera.orthographicSize > 0f)
		{
			num *= World.world.camera.orthographicSize / 100f;
			if (num > asset.max_alpha)
			{
				num = asset.max_alpha;
			}
		}
		switch (state)
		{
		case 1:
			if (alpha < num)
			{
				alpha += pElapsed * _fade_multiplier;
				if (alpha >= num)
				{
					alpha = num;
				}
			}
			else if (alpha > num)
			{
				alpha -= pElapsed * _fade_multiplier;
				if (alpha <= num)
				{
					alpha = num;
				}
			}
			else
			{
				alpha = num;
			}
			setAlpha(alpha);
			break;
		case 2:
			if (alpha > 0f)
			{
				alpha -= pElapsed * _fade_multiplier;
				setAlpha(alpha);
			}
			else
			{
				alpha = 0f;
				controller.killObject(this);
			}
			break;
		}
	}
}
