using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WorldLawElement : MonoBehaviour
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _selection;

	private WorldLawCategory _category;

	private WorldLawAsset _asset;

	private bool _initialized;

	private string _law_id => _asset?.id;

	public void init(WorldLawAsset pAsset)
	{
		_initialized = true;
		_asset = pAsset;
		if (!string.IsNullOrEmpty(_asset.icon_path))
		{
			_icon.sprite = SpriteTextureLoader.getSprite(_asset.icon_path);
		}
		((Component)_button).GetComponent<TipButton>().setHoverAction(delegate
		{
			if (InputHelpers.mouseSupported)
			{
				showTooltip();
			}
		});
		((Object)_button).name = _law_id;
		_category = ((Component)this).GetComponentInParent<WorldLawCategory>();
	}

	private void OnEnable()
	{
		if (Config.game_loaded && !SmoothLoader.isLoading() && _initialized)
		{
			updateStatus();
		}
	}

	public void click()
	{
		if (!InputHelpers.mouseSupported)
		{
			if (!Tooltip.isShowingFor(this))
			{
				showTooltip();
				return;
			}
			Tooltip.hideTooltipNow();
		}
		if (_asset.requires_premium && !Config.hasPremium)
		{
			ScrollWindow.showWindow("premium_menu");
			return;
		}
		PlayerOptionData playerOptionData = World.world.world_laws.dict[_law_id];
		bool boolVal = playerOptionData.boolVal;
		if (_asset.can_turn_off)
		{
			playerOptionData.boolVal = !playerOptionData.boolVal;
		}
		else if (!playerOptionData.boolVal)
		{
			playerOptionData.boolVal = true;
		}
		if (playerOptionData.boolVal && !boolVal)
		{
			_asset.on_state_enabled?.Invoke(playerOptionData);
		}
		World.world.world_laws.updateCaches();
		playerOptionData.on_switch?.Invoke(playerOptionData);
		updateStatus();
	}

	private void showTooltip()
	{
		Tooltip.show(this, "world_law", new TooltipData
		{
			world_law = _asset
		});
	}

	public void updateStatus()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		bool flag = isLawEnabled();
		((Behaviour)_selection).enabled = flag;
		if (flag)
		{
			((Graphic)_icon).color = Color.white;
		}
		else
		{
			((Graphic)_icon).color = Color.grey;
		}
		_category?.updateCounter();
	}

	public void addListener(UnityAction pAction)
	{
		((UnityEvent)_button.onClick).AddListener(pAction);
	}

	public void setSelectionColor(Color pColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)_selection).color = pColor;
	}

	public bool isLawEnabled()
	{
		return World.world.world_laws.dict[_law_id].boolVal;
	}
}
