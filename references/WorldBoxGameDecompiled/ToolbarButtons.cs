using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarButtons : MonoBehaviour, IShakable
{
	public static ToolbarButtons instance;

	public Image main_background;

	public Sprite button_sprite_normal;

	public Sprite button_sprite_unit_exists;

	public float shake_duration { get; } = 0.5f;

	public float shake_strength { get; } = 4f;

	public Tweener shake_tween { get; set; }

	public static Sprite getSpriteButtonNormal()
	{
		if ((Object)(object)instance == (Object)null)
		{
			return null;
		}
		return instance.button_sprite_normal;
	}

	public static Sprite getSpriteButtonUnitExists()
	{
		if ((Object)(object)instance == (Object)null)
		{
			return null;
		}
		return instance.button_sprite_unit_exists;
	}

	private void Awake()
	{
		instance = this;
	}

	public void resetBar()
	{
		((Component)this).gameObject.SetActive(false);
		((Component)this).gameObject.SetActive(true);
	}

	private void Update()
	{
		if (Time.frameCount % 30 == 0)
		{
			PowerButton.checkActorSpawnButtons();
		}
	}

	public Vector3 getPowerBarLeftCornerViewportPos()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		RectTransform rectTransform = ((Graphic)main_background).rectTransform;
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		rectTransform.GetWorldCorners(array);
		return array[1];
	}
}
