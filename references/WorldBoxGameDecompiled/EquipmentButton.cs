using System;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentButton : AugmentationButton<EquipmentAsset>, IBanner, IBaseMono, IRefreshElement
{
	[SerializeField]
	private Image _favorited_icon;

	private Item _item;

	private bool _object_button;

	public MetaCustomizationAsset meta_asset => AssetManager.meta_customization_library.getAsset(MetaType.Item);

	public MetaTypeAsset meta_type_asset => AssetManager.meta_type_library.getAsset(MetaType.Item);

	protected override string tooltip_type => "equipment";

	protected override void Update()
	{
		if (!is_editor_button)
		{
			if (augmentation_asset.unlocked_with_achievement)
			{
				((Component)locked_bg).gameObject.SetActive(false);
				return;
			}
			bool active = !augmentation_asset.isAvailable() && _object_button;
			((Component)locked_bg).gameObject.SetActive(active);
		}
	}

	protected override void onStartDrag(DraggableLayoutElement pOriginalElement)
	{
		EquipmentEditorButton component = ((Component)pOriginalElement).GetComponent<EquipmentEditorButton>();
		if ((Object)(object)component != (Object)null)
		{
			load(component.augmentation_button.augmentation_asset);
			is_editor_button = component.augmentation_button.is_editor_button;
			return;
		}
		EquipmentButton component2 = ((Component)pOriginalElement).GetComponent<EquipmentButton>();
		if (component2._object_button)
		{
			load(component2._item);
			is_editor_button = component2.is_editor_button;
		}
		else
		{
			load(component2.augmentation_asset);
			is_editor_button = component2.is_editor_button;
		}
	}

	public void load(NanoObject pObject)
	{
		load((Item)pObject);
	}

	internal void load(Item pItem)
	{
		_object_button = true;
		create();
		_item = pItem;
		augmentation_asset = _item.getAsset();
		if (augmentation_asset != null)
		{
			image.sprite = _item.getSprite();
			loadLegendaryOutline();
			((Object)((Component)this).gameObject).name = getElementType() + "_" + _item.data.asset_id;
			bool active = _item.isFavorite();
			((Component)_favorited_icon).gameObject.SetActive(active);
		}
	}

	public override void load(EquipmentAsset pItem)
	{
		_object_button = false;
		create();
		augmentation_asset = pItem;
		if (augmentation_asset != null)
		{
			image.sprite = augmentation_asset.getSprite();
			((Object)((Component)this).gameObject).name = getElementType() + "_" + augmentation_asset.id;
			((Component)_favorited_icon).gameObject.SetActive(false);
		}
	}

	protected override void initTooltip()
	{
		base.initTooltip();
		TipButton tipButton = default(TipButton);
		if (!((Component)this).TryGetComponent<TipButton>(ref tipButton))
		{
			return;
		}
		TipButton tipButton2 = tipButton;
		tipButton2.clickAction = (TooltipAction)Delegate.Combine(tipButton2.clickAction, (TooltipAction)delegate
		{
			if (InputHelpers.mouseSupported)
			{
				openItemWindow();
			}
			else if (Tooltip.isShowingFor(this) && !is_editor_button)
			{
				openItemWindow();
			}
			else
			{
				showTooltip();
			}
		});
	}

	private void openItemWindow()
	{
		SelectedMetas.selected_item = _item;
		if (SelectedMetas.selected_item != null)
		{
			ScrollWindow.showWindow("item");
		}
	}

	protected override void fillTooltipData(EquipmentAsset pElement)
	{
		string pType = ((is_editor_button || !_object_button) ? "equipment_in_editor" : "equipment");
		Tooltip.show(this, pType, tooltipDataBuilder());
	}

	protected override bool unlockElement()
	{
		return augmentation_asset.unlock();
	}

	protected override TooltipData tooltipDataBuilder()
	{
		if (!is_editor_button && _object_button)
		{
			return new TooltipData
			{
				item = _item
			};
		}
		return new TooltipData
		{
			item_asset = augmentation_asset
		};
	}

	protected override string getElementType()
	{
		return "equip";
	}

	protected override void startSignal()
	{
		AchievementLibrary.equipment_explorer.checkBySignal();
	}

	public override string getElementId()
	{
		return getElementAsset().id;
	}

	private bool hasDivineRune()
	{
		if (_item == null)
		{
			return false;
		}
		if (!_item.isAlive())
		{
			return false;
		}
		return _item.hasMod("divine_rune");
	}

	protected override Rarity getRarity()
	{
		return _item.getQuality();
	}

	public string getName()
	{
		return _item.getName();
	}

	public NanoObject GetNanoObject()
	{
		return _item;
	}

	T IBaseMono.GetComponent<T>()
	{
		return ((Component)this).GetComponent<T>();
	}
}
