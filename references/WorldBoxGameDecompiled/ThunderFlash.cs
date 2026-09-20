using UnityEngine;

public class ThunderFlash : BaseEffect
{
	private float _last_alpha = 1f;

	private int _blinks;

	private int _cur_blinks = 3;

	private float _timer_blink = 0.1f;

	internal void spawnFlash()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		prepare(Vector2.op_Implicit(Vector3.zero), 0.3f);
		updatePos();
		_blinks = Randy.randomInt(6, 10);
		_cur_blinks = _blinks;
		startBlink();
	}

	private void updatePos()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Texture)sprite_renderer.sprite.texture).width;
		float num2 = ((Texture)sprite_renderer.sprite.texture).height;
		Vector3 position = ((Component)World.world.camera).transform.position;
		float num3 = World.world.camera.orthographicSize * 2f;
		float num4 = num3 / (float)Screen.height * (float)Screen.width / num * 1f;
		float num5 = num3 / num2 * 1f;
		float num6 = 4f;
		float num7 = 4f;
		((Component)this).transform.localPosition = new Vector3(position.x, position.y + num5 * num2 / 2f);
		((Component)this).transform.localScale = new Vector3(num4 * num6, num5 * num7);
	}

	private void setColor(float pAlpha = 1f)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		_last_alpha = pAlpha;
		Color color = default(Color);
		((Color)(ref color))._002Ector(1f, 1f, 1f, pAlpha);
		sprite_renderer.color = color;
	}

	private void startBlink()
	{
		_timer_blink = Randy.randomFloat(0f, 0.1f);
		float color = 0.4f;
		setColor(color);
	}

	public override void update(float pElapsed)
	{
		pElapsed = Time.deltaTime;
		base.update(pElapsed);
		updatePos();
		if (_last_alpha > 0f)
		{
			_last_alpha -= pElapsed * 2f;
			if (_last_alpha < 0f)
			{
				_last_alpha = 0f;
			}
		}
		setColor(_last_alpha);
		if (_timer_blink > 0f)
		{
			_timer_blink -= pElapsed;
			if (_timer_blink > 0f)
			{
				return;
			}
			_cur_blinks--;
			if (_cur_blinks != 0)
			{
				startBlink();
				return;
			}
		}
		if (_last_alpha <= 0f)
		{
			kill();
		}
	}
}
