using FMOD.Studio;
using UnityEngine;

public class GroupSpriteObject : MonoBehaviour
{
	internal EventInstance fmod_instance;

	internal Transform m_transform;

	internal SpriteRenderer sprite_renderer;

	private bool _has_sprite_renderer;

	private Vector2 _last_pos_v2;

	private Vector2 _last_scale_v2;

	private Vector3 _last_pos_v3;

	private Vector3 _last_scale_v3;

	private Vector3 _last_angles_v3;

	private Color _last_color;

	private bool _last_flip_x;

	private int _last_sprite_hash_code;

	private int _last_sprite_material;

	public int last_id;

	public bool has_sprite_renderer => _has_sprite_renderer;

	private void Awake()
	{
		create();
	}

	protected void create()
	{
		m_transform = ((Component)this).gameObject.transform;
		sprite_renderer = ((Component)this).gameObject.GetComponent<SpriteRenderer>();
		if ((Object)(object)sprite_renderer != (Object)null)
		{
			_has_sprite_renderer = true;
		}
	}

	public void checkRotation(Vector3 pPos, BaseSimObject pSimObject, float pZ)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		pPos.z = pZ;
		Vector3 pivot = default(Vector3);
		ref Vector3 current_rotation = ref pSimObject.current_rotation;
		if (current_rotation.y != 0f || current_rotation.z != 0f)
		{
			((Vector3)(ref pivot)).Set(pSimObject.cur_transform_position.x, pSimObject.cur_transform_position.y, 0f);
			pPos = Toolbox.RotatePointAroundPivot(ref pPos, ref pivot, ref current_rotation);
			pPos.z = pZ;
		}
		setPosOnly(ref pPos);
		setLocalEulerAngles(pSimObject.current_rotation);
	}

	public void setPosOnly(Vector2 pPosition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (_last_pos_v3.x != pPosition.x || _last_pos_v3.y != pPosition.y)
		{
			_last_pos_v2 = pPosition;
			_last_pos_v3 = Vector2.op_Implicit(pPosition);
			m_transform.localPosition = _last_pos_v3;
		}
	}

	public void setPosOnly(ref Vector2 pPosition)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (_last_pos_v3.x != pPosition.x || _last_pos_v3.y != pPosition.y || _last_pos_v3.z != 0f)
		{
			_last_pos_v2 = pPosition;
			_last_pos_v3 = Vector2.op_Implicit(pPosition);
			m_transform.localPosition = _last_pos_v3;
		}
	}

	public void setPosOnly(ref Vector3 pPosition)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (_last_pos_v3.x != pPosition.x || _last_pos_v3.y != pPosition.y || _last_pos_v3.z != pPosition.z)
		{
			_last_pos_v2 = Vector2.op_Implicit(pPosition);
			_last_pos_v3 = pPosition;
			m_transform.localPosition = _last_pos_v3;
		}
	}

	public void setRotation(ref Vector3 pVec)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (_last_angles_v3.y != pVec.y || _last_angles_v3.z != pVec.z)
		{
			_last_angles_v3 = pVec;
			m_transform.eulerAngles = pVec;
		}
	}

	public void setLocalEulerAngles(Vector3 pVec)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (_last_angles_v3.y != pVec.y || _last_angles_v3.z != pVec.z)
		{
			_last_angles_v3 = pVec;
			m_transform.localEulerAngles = pVec;
		}
	}

	public void setSprite(Sprite pSprite)
	{
		int hashCode = ((object)pSprite).GetHashCode();
		if (_last_sprite_hash_code != hashCode)
		{
			sprite_renderer.sprite = pSprite;
			_last_sprite_hash_code = hashCode;
		}
	}

	public void setFlipX(bool pFlipX)
	{
		if (_last_flip_x != pFlipX)
		{
			_last_flip_x = pFlipX;
			sprite_renderer.flipX = pFlipX;
		}
	}

	public void setSharedMat(Material pMaterial)
	{
		int hashCode = ((object)pMaterial).GetHashCode();
		if (_last_sprite_material != hashCode)
		{
			((Renderer)sprite_renderer).sharedMaterial = pMaterial;
			_last_sprite_material = hashCode;
		}
	}

	public void setColor(ref Color pColor)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (_last_color.r != pColor.r || _last_color.g != pColor.g || _last_color.b != pColor.b || _last_color.a != pColor.a)
		{
			sprite_renderer.color = pColor;
			_last_color = pColor;
		}
	}

	public void setScale(float pScale)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (_last_scale_v3.y != pScale)
		{
			_last_scale_v2 = new Vector2(pScale, pScale);
			_last_scale_v3 = Vector2.op_Implicit(_last_scale_v2);
			m_transform.localScale = _last_scale_v3;
		}
	}

	public void setScale(float pScaleX, float pScaleY)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (_last_scale_v2.y != pScaleY || _last_scale_v2.x != pScaleX)
		{
			_last_scale_v2 = new Vector2(pScaleX, pScaleY);
			_last_scale_v3 = Vector2.op_Implicit(_last_scale_v2);
			m_transform.localScale = _last_scale_v3;
		}
	}

	public void setScale(ref Vector3 pScaleVec)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (_last_scale_v3 != pScaleVec)
		{
			_last_scale_v3 = pScaleVec;
			m_transform.localScale = pScaleVec;
		}
	}

	public void set(ref Vector2 pPosition, ref Vector3 pScale)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (_last_pos_v2.x != pPosition.x || _last_pos_v2.y != pPosition.y)
		{
			_last_pos_v2 = pPosition;
			_last_pos_v3 = Vector2.op_Implicit(pPosition);
			m_transform.localPosition = _last_pos_v3;
		}
		if (_last_scale_v2.y != pScale.y || _last_scale_v2.x != pScale.x)
		{
			_last_scale_v2 = Vector2.op_Implicit(pScale);
			_last_scale_v3 = Vector2.op_Implicit(_last_scale_v2);
			m_transform.localScale = pScale;
		}
	}

	public void set(ref Vector2 pPosition, float pScale)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (_last_pos_v3.x != pPosition.x || _last_pos_v3.y != pPosition.y)
		{
			_last_pos_v2 = pPosition;
			_last_pos_v3 = Vector2.op_Implicit(pPosition);
			m_transform.localPosition = _last_pos_v3;
		}
		if (_last_scale_v2.x != pScale)
		{
			setScale(pScale);
		}
	}

	public void set(ref Vector3 pPosition, float pScale)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (_last_pos_v3.x != pPosition.x || _last_pos_v3.y != pPosition.y || _last_pos_v3.z != pPosition.z)
		{
			_last_pos_v2 = Vector2.op_Implicit(pPosition);
			_last_pos_v3 = pPosition;
			m_transform.localPosition = _last_pos_v3;
		}
		if (_last_scale_v2.x != pScale)
		{
			setScale(pScale);
		}
	}

	public void set(ref Vector3 pPosition, ref Vector2 pScale)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (_last_pos_v3.x != pPosition.x || _last_pos_v3.y != pPosition.y || _last_pos_v3.z != pPosition.z)
		{
			_last_pos_v2 = Vector2.op_Implicit(pPosition);
			_last_pos_v3 = pPosition;
			m_transform.localPosition = _last_pos_v3;
		}
		if (_last_scale_v2.y != pScale.y || _last_scale_v2.x != pScale.x)
		{
			_last_scale_v2 = pScale;
			_last_scale_v3 = Vector2.op_Implicit(_last_scale_v2);
			m_transform.localScale = Vector2.op_Implicit(pScale);
		}
	}

	public void set(ref Vector3 pPosition, ref Vector3 pScale)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (_last_pos_v3.x != pPosition.x || _last_pos_v3.y != pPosition.y)
		{
			_last_pos_v2 = Vector2.op_Implicit(pPosition);
			_last_pos_v3 = pPosition;
			m_transform.localPosition = _last_pos_v3;
		}
		if (_last_scale_v2.y != pScale.y || _last_scale_v2.x != pScale.x)
		{
			_last_scale_v2 = Vector2.op_Implicit(pScale);
			_last_scale_v3 = Vector2.op_Implicit(_last_scale_v2);
			m_transform.localScale = pScale;
		}
	}

	public GroupSpriteObject()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		_last_pos_v2 = new Vector2(-1f, -1f);
		_last_scale_v2 = new Vector2(-1f, -1f);
		_last_pos_v3 = new Vector3(-1f, -1f, -1f);
		_last_scale_v3 = new Vector3(-1f, -1f, -1f);
		_last_angles_v3 = new Vector3(-1f, -1f, -1f);
		_last_sprite_hash_code = -1;
		_last_sprite_material = -1;
		((MonoBehaviour)this)._002Ector();
	}
}
