using UnityEngine;
using UnityEngine.UI;

public class UiWorldAgeInfo : MonoBehaviour
{
	[SerializeField]
	private Sprite _random_age_sprite;

	[SerializeField]
	private Image _icon_age_left;

	[SerializeField]
	private Image _icon_age_right;

	[SerializeField]
	private Image _icon_age_next_left;

	[SerializeField]
	private Image _icon_age_next_right;

	[SerializeField]
	private Image _icon_clock_left;

	[SerializeField]
	private Image _icon_clock_right;

	[SerializeField]
	private Text _text_age_title;

	[SerializeField]
	private Text _text_year;

	[SerializeField]
	private Text _text_month;

	[SerializeField]
	private StatBar _bar_age;

	[SerializeField]
	private StatBar _bar_year;

	[SerializeField]
	private Sprite _sprite_play;

	[SerializeField]
	private Sprite _sprite_pause;

	private LocalizedText _text_age_title_localized;

	private LocalizedText _text_year_localized;

	private LocalizedText _text_month_localized;

	private LocalizedText _bar_age_localized;

	private void Awake()
	{
		_text_age_title_localized = ((Component)_text_age_title).GetComponent<LocalizedText>();
		_text_year_localized = ((Component)_text_year).GetComponent<LocalizedText>();
		_text_month_localized = ((Component)_text_month).GetComponent<LocalizedText>();
		_bar_age_localized = ((Component)_bar_age.textField).GetComponent<LocalizedText>();
	}

	private void Update()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (Config.game_loaded && !((Object)(object)World.world == (Object)null) && World.world_era != null)
		{
			WorldAgeManager era_manager = World.world.era_manager;
			Sprite sprite = (era_manager.isPaused() ? _sprite_pause : _sprite_play);
			_icon_clock_left.sprite = sprite;
			_icon_clock_right.sprite = sprite;
			_icon_age_left.sprite = World.world_era.getSprite();
			_icon_age_right.sprite = World.world_era.getSprite();
			((Graphic)_text_age_title).color = World.world_era.title_color;
			_text_age_title_localized.setKeyAndUpdate(World.world_era.getLocaleID());
			MapStats map_stats = World.world.map_stats;
			_text_year_localized.setKeyAndUpdate("year_era");
			_text_month_localized.setKeyAndUpdate(AssetManager.months.getMonth(Date.getCurrentMonth()).getLocaleID());
			float pVal = (float)Date.getCurrentMonth() + Date.getMonthTime() / 5f - 1f;
			_bar_year.setBar(pVal, 12f, "", pReset: false, pFloat: true, pUpdateText: false);
			_bar_age.setBar(map_stats.current_age_progress, 1f, "", pReset: false, pFloat: true, pUpdateText: false);
			((Component)_bar_age).gameObject.SetActive(true);
			if (era_manager.isPaused())
			{
				_bar_age_localized.setKeyAndUpdate("ages_paused");
			}
			else
			{
				_bar_age_localized.setKeyAndUpdate("next_age_in_moons");
			}
			WorldAgeAsset nextAge = World.world.era_manager.getNextAge();
			if (nextAge == null)
			{
				_icon_age_next_left.sprite = _random_age_sprite;
				_icon_age_next_right.sprite = _random_age_sprite;
			}
			else
			{
				_icon_age_next_left.sprite = nextAge.getSprite();
				_icon_age_next_right.sprite = nextAge.getSprite();
			}
		}
	}
}
