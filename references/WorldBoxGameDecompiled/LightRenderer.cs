using UnityEngine;
using UnityEngine.UI;

public class LightRenderer : MonoBehaviour
{
	public static LightRenderer instance;

	public Camera camera;

	public EffectsCamera effectsCamera;

	private RawImage _rawImage;

	private void Awake()
	{
		instance = this;
		_rawImage = ((Component)this).GetComponent<RawImage>();
	}

	public void update(float pElapsed)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		_rawImage.texture = (Texture)(object)effectsCamera.renderTexture;
		Color light_color = World.world_era.light_color;
		light_color.a = World.world.era_manager.getNightMod() * 0.6f;
		((Graphic)_rawImage).color = light_color;
	}
}
