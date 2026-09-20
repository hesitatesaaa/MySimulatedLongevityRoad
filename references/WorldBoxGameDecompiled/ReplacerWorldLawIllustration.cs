using UnityEngine;
using UnityEngine.UI;

public class ReplacerWorldLawIllustration : MonoBehaviour
{
	private Image _target_image;

	public Sprite image_normal;

	public Sprite image_cursed;

	private void Awake()
	{
		_target_image = ((Component)this).GetComponent<Image>();
	}

	private void OnEnable()
	{
		if (Config.game_loaded)
		{
			if (WorldLawLibrary.world_law_cursed_world.isEnabled())
			{
				_target_image.sprite = image_cursed;
			}
			else
			{
				_target_image.sprite = image_normal;
			}
		}
	}
}
