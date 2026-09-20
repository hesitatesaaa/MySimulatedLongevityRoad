using UnityEngine;
using UnityEngine.UI;

public class PowerClockButton : MonoBehaviour
{
	public Image currentSpeedIcon;

	private string _latest_used = string.Empty;

	private void Update()
	{
		if (Config.time_scale_asset != null && Config.time_scale_asset.id != _latest_used)
		{
			_latest_used = Config.time_scale_asset.id;
			currentSpeedIcon.sprite = SpriteTextureLoader.getSprite(Config.time_scale_asset.path_icon);
		}
	}
}
