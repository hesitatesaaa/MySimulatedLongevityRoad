using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldAgesWindow : MonoBehaviour
{
	private const float SLOW = 0.5f;

	private const float NORMAL = 1f;

	private const float FAST = 2f;

	private const float FAST_VERY = 5f;

	private const float FAST_ULTRA = 10f;

	private const float FAST_SONIC = 20f;

	private static WorldAgesWindow _instance;

	[SerializeField]
	private Text _age_name;

	[SerializeField]
	private WorldAgeButton _age_button_prefab;

	[SerializeField]
	private Sprite _play_sprite;

	[SerializeField]
	private Sprite _pause_sprite;

	[SerializeField]
	private Sprite _age_speed_sprite_slow;

	[SerializeField]
	private Sprite _age_speed_sprite_normal;

	[SerializeField]
	private Sprite _age_speed_sprite_fast;

	[SerializeField]
	private Sprite _age_speed_sprite_fast_very;

	[SerializeField]
	private Sprite _age_speed_sprite_fast_ultra;

	[SerializeField]
	private Sprite _age_speed_sprite_fast_sonic;

	[SerializeField]
	private WorldAgeWheel _age_wheel;

	[SerializeField]
	private Transform _grid_age_buttons;

	[SerializeField]
	private Image _pause_button_icon;

	[SerializeField]
	private Image _age_speed_button_icon;

	[SerializeField]
	private Image _selected_age_background;

	[SerializeField]
	private Image _background_filter;

	private Dictionary<WorldAgeAsset, WorldAgeButton> _buttons = new Dictionary<WorldAgeAsset, WorldAgeButton>();

	private WorldAgeWheelPiece _selected_piece;

	[SerializeField]
	private Text _text_time_info;

	private WorldAgeManager _era_manager => World.world.era_manager;

	private MapStats _map_stats => World.world.map_stats;

	private void Awake()
	{
		_instance = this;
		_age_wheel.init(wheelPieceAction);
		initButtons();
	}

	private void OnEnable()
	{
		selectPiece(_era_manager.getCurrentSlotIndex());
		updateElements();
	}

	private void OnDisable()
	{
		updateElements();
	}

	public static void setAgeAndSelectPiece(WorldAgeAsset pAsset, WorldAgeWheelPiece pPiece)
	{
		_instance.setAgeAndSelectPieceInstance(pAsset, pPiece);
	}

	private void setAgeAndSelectPieceInstance(WorldAgeAsset pAsset, WorldAgeWheelPiece pPiece)
	{
		pPiece.setAge(pAsset);
		_era_manager.setAgeToSlot(pAsset, pPiece.getIndex());
		selectPiece(pPiece);
		_era_manager.setCurrentSlotIndex(pPiece.getIndex(), 0.01f);
		updateElements();
	}

	private void selectPiece(int pIndex)
	{
		WorldAgeWheelPiece piece = _age_wheel.getPiece(pIndex);
		selectPiece(piece);
	}

	private void selectPiece(WorldAgeWheelPiece pPiece)
	{
		_selected_piece = pPiece;
	}

	private void initButtons()
	{
		for (int i = 0; i < AssetManager.era_library.list.Count; i++)
		{
			WorldAgeAsset worldAgeAsset = AssetManager.era_library.list[i];
			WorldAgeButton value = initButton(worldAgeAsset);
			_buttons.Add(worldAgeAsset, value);
		}
	}

	private WorldAgeButton initButton(WorldAgeAsset pAsset)
	{
		WorldAgeButton worldAgeButton = Object.Instantiate<WorldAgeButton>(_age_button_prefab, _grid_age_buttons);
		worldAgeButton.setAge(pAsset);
		worldAgeButton.addClickCallback(ageButtonAction);
		return worldAgeButton;
	}

	private void wheelPieceAction(BaseWorldAgeElement pPiece)
	{
		if (!((Object)(object)_selected_piece == (Object)(object)pPiece))
		{
			selectPiece(pPiece as WorldAgeWheelPiece);
			updateElements();
		}
	}

	private void ageButtonAction(BaseWorldAgeElement pElement)
	{
		if (!InputHelpers.mouseSupported)
		{
			if (!Tooltip.isShowingFor(((Component)pElement).gameObject))
			{
				return;
			}
			Tooltip.hideTooltip();
		}
		WorldAgeAsset asset = pElement.getAsset();
		_selected_piece.setAge(asset);
		_era_manager.setAgeToSlot(asset, _selected_piece.getIndex());
		updateElements();
	}

	public void nextAgeAction()
	{
		_era_manager.startNextAge(0.5f);
		updateElements();
	}

	public void pauseAgesAction()
	{
		_era_manager.togglePlay(_era_manager.isPaused());
		updateElements();
	}

	public void randomizeAgesAction()
	{
		foreach (WorldAgeWheelPiece piece in _age_wheel.getPieces())
		{
			WorldAgeAsset random = AssetManager.era_library.list.GetRandom();
			piece.setAge(random);
			_era_manager.setAgeToSlot(random, piece.getIndex());
		}
		_era_manager.setCurrentSlotIndex(0, 0.01f);
		selectPiece(0);
		updateElements();
	}

	public void toggleAgeSpeedAction()
	{
		float world_ages_speed_multiplier = _map_stats.world_ages_speed_multiplier;
		float num;
		if (world_ages_speed_multiplier <= 1f)
		{
			if (world_ages_speed_multiplier != 0.5f)
			{
				if (world_ages_speed_multiplier != 1f)
				{
					goto IL_0068;
				}
				num = 2f;
			}
			else
			{
				num = 1f;
			}
		}
		else if (world_ages_speed_multiplier != 2f)
		{
			if (world_ages_speed_multiplier != 5f)
			{
				if (world_ages_speed_multiplier != 10f)
				{
					goto IL_0068;
				}
				num = 20f;
			}
			else
			{
				num = 10f;
			}
		}
		else
		{
			num = 5f;
		}
		goto IL_006e;
		IL_0068:
		num = 1f;
		goto IL_006e;
		IL_006e:
		float agesSpeedMultiplier = num;
		_era_manager.setAgesSpeedMultiplier(agesSpeedMultiplier);
		updateElements();
	}

	private void updateElements()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		WorldAgeAsset currentAge = _era_manager.getCurrentAge();
		_age_name.text = LocalizedTextManager.getText(currentAge.getLocaleID());
		((Graphic)_age_name).color = currentAge.title_color;
		updatePiePieces();
		updateAgeButtonSelectors();
		_age_wheel.updateElements();
		_pause_button_icon.sprite = (_era_manager.isPaused() ? _play_sprite : _pause_sprite);
		Image age_speed_button_icon = _age_speed_button_icon;
		float world_ages_speed_multiplier = _map_stats.world_ages_speed_multiplier;
		Sprite sprite;
		if (world_ages_speed_multiplier <= 2f)
		{
			if (world_ages_speed_multiplier != 0.5f)
			{
				if (world_ages_speed_multiplier != 1f)
				{
					if (world_ages_speed_multiplier != 2f)
					{
						goto IL_0104;
					}
					sprite = _age_speed_sprite_fast;
				}
				else
				{
					sprite = _age_speed_sprite_normal;
				}
			}
			else
			{
				sprite = _age_speed_sprite_slow;
			}
		}
		else if (world_ages_speed_multiplier != 5f)
		{
			if (world_ages_speed_multiplier != 10f)
			{
				if (world_ages_speed_multiplier != 20f)
				{
					goto IL_0104;
				}
				sprite = _age_speed_sprite_fast_sonic;
			}
			else
			{
				sprite = _age_speed_sprite_fast_ultra;
			}
		}
		else
		{
			sprite = _age_speed_sprite_fast_very;
		}
		goto IL_010c;
		IL_010c:
		age_speed_button_icon.sprite = sprite;
		_selected_age_background.sprite = World.world_era.getBackground();
		float num = 0.8f;
		if (_era_manager.isPaused())
		{
			num = 0.4f;
		}
		Color color = default(Color);
		((Color)(ref color))._002Ector(num, num, num);
		((Graphic)_selected_age_background).color = color;
		Color color2 = ((Graphic)_background_filter).color;
		color2.r = World.world_era.title_color.r;
		color2.g = World.world_era.title_color.g;
		color2.b = World.world_era.title_color.b;
		((Graphic)_background_filter).color = color2;
		updateTextTimeInfo();
		return;
		IL_0104:
		sprite = _age_speed_sprite_normal;
		goto IL_010c;
	}

	private void updatePiePieces()
	{
		foreach (WorldAgeWheelPiece piece in _age_wheel.getPieces())
		{
			bool flag = isPieceSelected(piece);
			piece.setAge(_era_manager.getAgeFromSlot(piece.getIndex()));
			piece.toggleHighlight(piece.isCurrentAge());
			piece.toggleIconFrame(!flag);
			piece.setIconActiveColor(piece.isCurrentAge());
		}
	}

	private void updateAgeButtonSelectors()
	{
		int currentSlotIndex = _era_manager.getCurrentSlotIndex();
		WorldAgeAsset asset = _age_wheel.getPiece(currentSlotIndex).getAsset();
		foreach (WorldAgeButton value in _buttons.Values)
		{
			bool flag = value.getAsset() == asset;
			value.toggleSelectedButton(flag);
			value.setIconActiveColor(flag);
		}
	}

	private void updateTextTimeInfo()
	{
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		stringBuilderPool.Append(Date.getUIStringYearMonth());
		stringBuilderPool.AppendLine();
		stringBuilderPool.Append("a: ");
		stringBuilderPool.Append(_map_stats.current_age_progress.ToString("P0"));
		stringBuilderPool.AppendLine();
		stringBuilderPool.Append("w: ");
		stringBuilderPool.Append($"{_map_stats.world_age_slot_index + 1}/{8}");
		_text_time_info.text = stringBuilderPool.ToString();
	}

	private bool isPieceSelected(WorldAgeWheelPiece pPiece)
	{
		return (Object)(object)pPiece == (Object)(object)_selected_piece;
	}
}
