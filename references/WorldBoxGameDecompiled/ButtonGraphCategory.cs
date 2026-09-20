using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonGraphCategory : MonoBehaviour
{
	public Sprite sprite_on;

	public Sprite sprite_off;

	public Sprite sprite_on_light;

	private Image _button_graphics;

	private Image _icon;

	public bool is_on;

	private GraphCategoriesContainer _main_container;

	private Text _text;

	private Image _colored_circle;

	private Image _background_circle;

	private TipButton _tip_button;

	private HistoryDataAsset _asset;

	private bool _initialized;

	private void Awake()
	{
		init();
	}

	public void init()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener(new UnityAction(switchCategory));
		_tip_button = ((Component)this).GetComponent<TipButton>();
		_button_graphics = ((Component)this).GetComponent<Image>();
		_icon = ((Component)((Component)this).transform.FindRecursive("Icon")).GetComponent<Image>();
		_main_container = ((Component)this).GetComponentInParent<GraphCategoriesContainer>();
		_text = ((Component)((Component)this).transform.FindRecursive("Title")).GetComponent<Text>();
		_colored_circle = ((Component)((Component)this).transform.FindRecursive("Colored Circle")).GetComponent<Image>();
		_background_circle = ((Component)((Component)this).transform.FindRecursive("Background Circle")).GetComponent<Image>();
		_tip_button.hoverAction = delegate
		{
			if (InputHelpers.mouseSupported)
			{
				showTooltip();
			}
		};
		checkSpriteStatus();
	}

	public void setAsset(HistoryDataAsset pAsset)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (pAsset != null)
		{
			_asset = pAsset;
			((Graphic)_colored_circle).color = pAsset.getColorMain();
			_icon.sprite = SpriteTextureLoader.getSprite(pAsset.path_icon);
		}
	}

	private void Update()
	{
		checkSpriteStatus();
	}

	private void checkSpriteStatus()
	{
		if (is_on)
		{
			_button_graphics.sprite = sprite_on;
			((Component)_background_circle).gameObject.SetActive(true);
		}
		else
		{
			_button_graphics.sprite = sprite_off;
			((Component)_background_circle).gameObject.SetActive(false);
		}
	}

	private void switchCategory()
	{
		if (!InputHelpers.mouseSupported && !Tooltip.isShowingFor(this))
		{
			showTooltip();
		}
		is_on = !is_on;
		_main_container.setCategoryEnabled(((Object)this).name, is_on);
	}

	private void showTooltip()
	{
		TooltipData pData = new TooltipData
		{
			tip_name = _asset.getLocaleID(),
			tip_description = _asset.getDescriptionID(),
			tip_description_2 = "graph_tip"
		};
		Tooltip.show(this, "tip", pData);
	}

	public void turnOff()
	{
		is_on = false;
	}

	public void turnOn()
	{
		is_on = true;
	}
}
