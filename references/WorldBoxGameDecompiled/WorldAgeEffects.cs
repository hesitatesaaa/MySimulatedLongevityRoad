using System.Collections.Generic;
using UnityEngine;

public class WorldAgeEffects : MonoBehaviour
{
	internal Dictionary<string, SpriteRenderer> dict_effects = new Dictionary<string, SpriteRenderer>();

	public static WorldAgeEffects instance;

	public bool override_night;

	[Range(0f, 1f)]
	public float night_value_top;

	[Range(0f, 1f)]
	public float night_value_mat;

	public void Awake()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		instance = this;
		for (int i = 0; i < ((Component)this).transform.childCount; i++)
		{
			SpriteRenderer component = ((Component)((Component)this).transform.GetChild(i)).GetComponent<SpriteRenderer>();
			Color color = component.color;
			color.a = 0f;
			component.color = color;
			dict_effects.Add(((Object)component).name, component);
		}
	}

	public void update(float pElapsed)
	{
		fitTheCamera();
		updateEffects(pElapsed);
	}

	private void updateEffects(float pElapsed)
	{
		updateLayer(World.world_era.overlay_chaos, "chaos", pElapsed);
		updateLayer(World.world_era.overlay_moon, "moon", pElapsed);
		updateLayer(World.world_era.overlay_magic, "magic", pElapsed);
		updateLayer(World.world_era.overlay_sun, "sun", pElapsed);
		updateLayer(World.world_era.overlay_rain_darkness, "rain_darkness", pElapsed);
		updateLayer(World.world_era.overlay_winter, "winter", pElapsed);
		updateLayer(World.world_era.overlay_ash, "ash", pElapsed);
		updateLayer(World.world_era.overlay_night, "night", pElapsed);
		updateLayer(World.world_era.overlay_rain, "rain", pElapsed);
	}

	private void updateLayer(bool pEnabled, string pID, float pElapsed)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		SpriteRenderer value = null;
		if (!dict_effects.TryGetValue(pID, out value))
		{
			Debug.LogError((object)("NO ERA EFFECT " + pID));
			return;
		}
		Color color = value.color;
		if (override_night)
		{
			color.a = night_value_top;
			value.color = color;
			return;
		}
		if (pEnabled)
		{
			float num = PlayerConfig.getIntValue("age_overlay_effect");
			float num2 = World.world_era.era_effect_overlay_alpha * (num / 100f);
			((Renderer)value).enabled = num > 0f;
			if (color.a < num2)
			{
				color.a += pElapsed * 0.2f;
				if (color.a > num2)
				{
					color.a = num2;
				}
				value.color = color;
			}
			if (color.a > num2)
			{
				color.a -= pElapsed * 0.7f;
				if (color.a < num2)
				{
					color.a = num2;
				}
				value.color = color;
			}
		}
		if (!pEnabled && ((Renderer)value).enabled && color.a > 0f)
		{
			color.a -= pElapsed * 0.2f;
			if (color.a <= 0f)
			{
				((Renderer)value).enabled = false;
			}
			value.color = color;
		}
	}

	private void fitTheCamera()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localScale = new Vector3(1f, 1f, 1f);
		float num = World.world.camera.orthographicSize * 2f;
		float num2 = num / (float)Screen.height * (float)Screen.width;
		((Component)this).transform.localScale = new Vector3(num2, num);
	}
}
