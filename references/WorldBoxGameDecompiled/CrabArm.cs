using System.Collections.Generic;
using UnityEngine;

public class CrabArm : MonoBehaviour
{
	internal Crabzilla crabzilla;

	public SpriteRenderer laser;

	public Transform laserPoint;

	public GameObject joint;

	public List<Sprite> laserSprites;

	public bool mirrored;

	private const float LASER_INTERVAL = 0.07f;

	private float _laser_timer = 0.07f;

	private int _laser_frame_index;

	private void Start()
	{
		((Renderer)laser).enabled = false;
	}

	internal void update(float pElapsed)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = World.world.camera.WorldToScreenPoint(crabzilla.armTarget.transform.position);
		val.z = 5.23f;
		Vector3 val2 = World.world.camera.WorldToScreenPoint(joint.transform.position);
		val.x -= val2.x;
		val.y -= val2.y;
		float num = Mathf.Atan2(val.y, val.x) * 57.29578f + 90f;
		if (mirrored)
		{
			num += 180f;
		}
		joint.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, num));
		updateLaser(pElapsed);
		if (crabzilla.isBeamEnabled())
		{
			float x = ((Component)laserPoint).transform.position.x;
			float y = ((Component)laserPoint).transform.position.y;
			MusicBox.inst.playDrawingSound("event:/SFX/UNIQUE/Crabzilla/CrabzillaLazer", x, y);
			World.world.stack_effects.light_blobs.Add(new LightBlobData
			{
				position = new Vector2(((Component)laser).transform.position.x, ((Component)laser).transform.position.y),
				radius = 1.5f
			});
			if (_laser_frame_index > 6 && _laser_frame_index < 10)
			{
				damageWorld();
			}
		}
	}

	private void damageWorld()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float x = ((Component)laserPoint).transform.position.x;
		float y = ((Component)laserPoint).transform.position.y;
		WorldTile tile = World.world.GetTile((int)x, (int)y);
		if (tile != null)
		{
			MapAction.damageWorld(tile, 4, AssetManager.terraform.get("crab_laser"));
		}
	}

	private void updateLaser(float pTime)
	{
		_laser_timer -= pTime;
		if (crabzilla.isBeamEnabled())
		{
			if (_laser_timer <= 0f)
			{
				_laser_frame_index++;
				if (_laser_frame_index >= 10)
				{
					_laser_frame_index = 6;
				}
			}
		}
		else if (_laser_frame_index != 0)
		{
			_laser_frame_index++;
			if (_laser_frame_index > 13)
			{
				_laser_frame_index = 0;
			}
		}
		if (_laser_timer <= 0f)
		{
			_laser_timer = 0.07f;
		}
		if (((Object)laser.sprite).name != ((Object)laserSprites[_laser_frame_index]).name)
		{
			laser.sprite = laserSprites[_laser_frame_index];
		}
		((Renderer)laser).enabled = _laser_frame_index != 0 || crabzilla.isBeamEnabled();
	}
}
