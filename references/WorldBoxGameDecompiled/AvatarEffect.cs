using UnityEngine;
using UnityEngine.UI;

public class AvatarEffect : MonoBehaviour
{
	public Image image;

	private RectTransform _rect_transform;

	private Vector3 _initial_position;

	private StatusAsset _asset;

	private Actor _actor;

	private UnitAvatarLoader _avatar;

	private bool _animated;

	private float _time_between_frames;

	private float _elapsed;

	private int _current_frame;

	public void load(StatusAsset pAsset, Actor pActor, UnitAvatarLoader pAvatar)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		_asset = pAsset;
		_actor = pActor;
		_avatar = pAvatar;
		_animated = pAsset.animated;
		_rect_transform = ((Component)this).GetComponent<RectTransform>();
		int pIndex;
		if (!pAsset.animated)
		{
			if (pAsset.random_frame)
			{
				int pMaxExclusive = pAsset.get_sprites_count(pActor, pAsset);
				pIndex = Randy.randomInt(0, pMaxExclusive);
			}
			else
			{
				pIndex = 0;
			}
		}
		else
		{
			_time_between_frames = pAsset.animation_speed + Randy.randomFloat(0f, pAsset.animation_speed_random);
			pIndex = 0;
		}
		((Component)image).transform.localEulerAngles = getSpriteRotation(_current_frame);
		image.sprite = getSprite(pIndex);
	}

	public void update(float pElapsed)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (_animated)
		{
			_elapsed += pElapsed;
			if (!(_elapsed < _time_between_frames))
			{
				_elapsed = 0f;
				int pLength = _asset.get_sprites_count(_actor, _asset);
				_current_frame = Toolbox.loopIndex(_current_frame + 1, pLength);
				Sprite sprite = getSprite(_current_frame);
				((Component)image).transform.localPosition = _initial_position + getSpritePosition(_current_frame);
				((Component)image).transform.localEulerAngles = getSpriteRotation(_current_frame);
				image.sprite = sprite;
			}
		}
	}

	private Sprite getSprite(int pIndex)
	{
		if (_asset.has_override_sprite)
		{
			return _asset.get_override_sprite_ui(this, pIndex);
		}
		return _asset.sprite_list[pIndex];
	}

	private Vector3 getSpritePosition(int pIndex)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (_asset.has_override_sprite)
		{
			return _asset.get_override_sprite_position_ui(this, pIndex);
		}
		return default(Vector3);
	}

	private Vector3 getSpriteRotation(int pIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = default(Vector3);
		if (_asset.has_override_sprite_rotation_z)
		{
			result.z = _asset.get_override_sprite_rotation_z_ui(this, pIndex);
		}
		else
		{
			result.z = _asset.rotation_z;
		}
		return result;
	}

	public void setInitialPosition(Vector2 pPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_initial_position = Vector2.op_Implicit(pPosition);
	}

	public RectTransform getRectTransform()
	{
		return _rect_transform;
	}

	public UnitAvatarLoader getAvatar()
	{
		return _avatar;
	}

	public StatusAsset getAsset()
	{
		return _asset;
	}
}
