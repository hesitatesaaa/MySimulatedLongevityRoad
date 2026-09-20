using UnityEngine;

public class CrabLeg : MonoBehaviour
{
	public CrabLegLimbPoint limbPoint;

	internal Crabzilla crabzilla;

	private Vector3 _current_position;

	private Vector3 _target_position;

	private Vector3 _random_pos;

	public CrabLegJoint legJoint;

	private Vector3 _target_pos;

	internal void create()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		_target_position = ((Component)limbPoint).transform.position;
		_target_position.z = 0f;
		_current_position = _target_position;
		((Component)this).transform.position = new Vector3(_target_position.x, _target_position.y, 0f);
		((Renderer)((Component)this).GetComponent<SpriteRenderer>()).enabled = false;
	}

	internal void update(float pElapsed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		float num = Toolbox.DistVec3(_current_position, _target_position);
		_current_position = Vector3.MoveTowards(_current_position, _target_position, 1.5f + num / 5f);
		((Component)this).transform.position = new Vector3(_current_position.x, _current_position.y, 0f);
		_target_pos = ((Component)limbPoint).transform.position + _random_pos;
		if (!legJoint.isAngleOk(-20f, 30f))
		{
			moveLeg();
		}
	}

	public void moveLeg()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		_target_pos = ((Component)limbPoint).transform.position + _random_pos;
		_target_pos.z = 0f;
		_target_position = _target_pos;
		_random_pos.x = Randy.randomFloat(-1f, 1f);
		_random_pos.y = Randy.randomFloat(-1f, 1f);
		Vector2 val = ControllableUnit.getMovementVector();
		if (!ControllableUnit.isMovementActionActive())
		{
			val = Vector2.zero;
		}
		if (val.x != 0f)
		{
			if (val.x > 0f)
			{
				_random_pos.x += 2f;
			}
			else
			{
				_random_pos.x -= 2f;
			}
		}
		if (val.y != 0f)
		{
			if (val.y > 0f)
			{
				_random_pos.y += 2f;
			}
			else
			{
				_random_pos.y -= 2f;
			}
		}
		crabzilla.legMoved();
		WorldTile tile = World.world.GetTile((int)_target_pos.x, (int)_target_pos.y);
		if (tile != null)
		{
			MapAction.damageWorld(tile, 3, AssetManager.terraform.get("crab_step"));
			MusicBox.playSound("event:/SFX/UNIQUE/Crabzilla/CrabzillaFootsteps", tile);
		}
	}

	public CrabLeg()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_random_pos = Vector3.zero;
		((MonoBehaviour)this)._002Ector();
	}
}
