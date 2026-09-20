using UnityEngine;
using UnityEngine.UI;

public class ReplacerAchievements : MonoBehaviour
{
	[SerializeField]
	private Image _target_icon;

	public Sprite icon_gold;

	public Sprite icon_silver;

	private BuildingAsset _asset;

	private void OnEnable()
	{
		if (Config.game_loaded && !SmoothLoader.isLoading())
		{
			checkIcon();
		}
	}

	private void Start()
	{
		checkIcon();
	}

	private void checkIcon()
	{
		if (AchievementLibrary.isAllUnlocked())
		{
			_target_icon.sprite = icon_gold;
		}
		else
		{
			_target_icon.sprite = icon_silver;
		}
	}
}
