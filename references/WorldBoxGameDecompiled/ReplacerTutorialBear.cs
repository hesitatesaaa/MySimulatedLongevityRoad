using UnityEngine;
using UnityEngine.UI;

public class ReplacerTutorialBear : MonoBehaviour
{
	[SerializeField]
	private Image _target_icon;

	public Sprite icon_animal;

	public Sprite icon_civ;

	private BuildingAsset _asset;

	private void OnEnable()
	{
		if (Config.game_loaded && !SmoothLoader.isLoading())
		{
			if (_asset == null)
			{
				_asset = AssetManager.buildings.get("monolith");
			}
			if (_asset.buildings.Count > 0)
			{
				_target_icon.sprite = icon_civ;
			}
			else
			{
				_target_icon.sprite = icon_animal;
			}
		}
	}
}
