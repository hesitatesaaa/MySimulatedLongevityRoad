using UnityEngine;
using UnityEngine.UI;

public class ReplacerWorldLawsCursed : MonoBehaviour
{
	[SerializeField]
	private Image _target_icon;

	public Sprite icon_normal;

	public Sprite icon_world_cursed;

	private void OnEnable()
	{
		if (Config.game_loaded)
		{
			if (WorldLawLibrary.world_law_cursed_world.isEnabled())
			{
				_target_icon.sprite = icon_world_cursed;
			}
			else
			{
				_target_icon.sprite = icon_normal;
			}
		}
	}
}
