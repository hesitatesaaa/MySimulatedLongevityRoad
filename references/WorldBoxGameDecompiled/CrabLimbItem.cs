using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CrabLimbItem : MonoBehaviour
{
	public CrabLimb crabLimb;

	public Sprite high_hp;

	public Sprite med_hp;

	public Sprite low_hp;

	internal SpriteRenderer _sprite_renderer;

	private Color _shade;

	private Color _dmg;

	private void Awake()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_sprite_renderer = ((Component)this).GetComponent<SpriteRenderer>();
		_sprite_renderer.sprite = high_hp;
		_shade = _sprite_renderer.color;
	}

	internal void stateChange(CrabLimbState pState)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		switch (pState)
		{
		case CrabLimbState.HighHP:
			_sprite_renderer.sprite = high_hp;
			break;
		case CrabLimbState.MedHP:
			_sprite_renderer.sprite = med_hp;
			break;
		case CrabLimbState.LowHP:
			_sprite_renderer.sprite = low_hp;
			break;
		}
		_sprite_renderer.color = _dmg;
	}

	internal void flicker(float pProgress)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_sprite_renderer.color = Color.Lerp(_dmg, _shade, pProgress);
	}

	public CrabLimbItem()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		_dmg = new Color(1f, 0f, 0f, 1f);
		((MonoBehaviour)this)._002Ector();
	}
}
