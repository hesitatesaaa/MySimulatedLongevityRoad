using UnityEngine;

public class WorldAgesParticles : MonoBehaviour
{
	public static bool effects_enabled = true;

	private ParticleSystem _system_rain;

	private Material _mat_rain;

	private ParticleSystem _system_snow;

	private Material _mat_snow;

	private ParticleSystem _system_magic;

	private Material _mat_magic;

	private ParticleSystem _system_ash;

	private Material _mat_ash;

	private ParticleSystem _system_sun_blobs;

	private Material _mat_sun_blobs;

	private ParticleSystem _system_sun_rays;

	private Material _mat_sun_ray;

	private Camera _camera;

	private void Awake()
	{
		setSystem("Rain", out _system_rain, out _mat_rain);
		setSystem("Snow", out _system_snow, out _mat_snow);
		setSystem("Magic", out _system_magic, out _mat_magic);
		setSystem("Ash", out _system_ash, out _mat_ash);
		setSystem("Sun Blobs", out _system_sun_blobs, out _mat_sun_blobs);
		setSystem("Sun Rays", out _system_sun_rays, out _mat_sun_ray);
	}

	private void setSystem(string pID, out ParticleSystem pSystem, out Material pMat)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		pSystem = ((Component)((Component)this).transform.Find(pID)).GetComponent<ParticleSystem>();
		pMat = ((Component)pSystem).GetComponent<Renderer>().material;
		pSystem.Stop(false, (ParticleSystemStopBehavior)0);
		Color color = pMat.color;
		color.a = 0f;
		pMat.color = color;
	}

	private void Update()
	{
		if (!((Object)(object)World.world == (Object)null) && World.world_era != null)
		{
			_camera = World.world.camera;
			updateParticles(_system_rain, _mat_rain, World.world_era.particles_rain);
			updateParticles(_system_snow, _mat_snow, World.world_era.particles_snow);
			updateParticles(_system_magic, _mat_magic, World.world_era.particles_magic);
			updateParticles(_system_ash, _mat_ash, World.world_era.particles_ash);
			updateParticles(_system_sun_blobs, _mat_sun_blobs, World.world_era.particles_sun);
			updateParticles(_system_sun_rays, _mat_sun_ray, World.world_era.particles_sun);
		}
	}

	private void updateParticles(ParticleSystem pSystem, Material pMaterial, bool pEnabled)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		if (!effects_enabled)
		{
			pEnabled = false;
		}
		Color color = pMaterial.color;
		bool flag = MapBox.isRenderGameplay() & pEnabled;
		if (color.a != 0f && !flag && !pSystem.isPlaying)
		{
			return;
		}
		int width = MapBox.width;
		int height = MapBox.height;
		Vector3 localPosition = default(Vector3);
		((Vector3)(ref localPosition))._002Ector((float)(width / 2), (float)(height / 2));
		((Component)pSystem).transform.localPosition = localPosition;
		ShapeModule shape = pSystem.shape;
		((ShapeModule)(ref shape)).scale = new Vector3((float)width * 1.5f, (float)height * 1.5f, 1f);
		if (!flag)
		{
			if (color.a > 0f)
			{
				color.a -= World.world.delta_time * 0.1f;
			}
		}
		else if (color.a < 1f)
		{
			color.a += World.world.delta_time * 0.1f;
			if (color.a > 1f)
			{
				color.a = 1f;
			}
		}
		if (color.a <= 0f)
		{
			color.a = 0f;
			pSystem.Stop(false, (ParticleSystemStopBehavior)0);
		}
		else if (!pSystem.isPlaying)
		{
			pSystem.Play();
		}
		pMaterial.color = color;
	}
}
