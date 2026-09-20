using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AugmentationButton<TAugmentation> : MonoBehaviour where TAugmentation : BaseAugmentationAsset
{
	[NonSerialized]
	public TAugmentation augmentation_asset;

	internal Image image;

	internal Image locked_bg;

	private IconOutline _outline;

	private Shadow _shadow;

	private bool _tooltip_enabled = true;

	internal Button button;

	internal bool is_editor_button;

	private AugmentationUnlockedAction _on_augmentation_unlocked;

	private AugmentationButtonClickAction _on_button_clicked;

	protected bool created;

	private bool _selected;

	protected virtual string tooltip_type
	{
		get
		{
			throw new NotImplementedException(((object)this).GetType().Name);
		}
	}

	public bool isSelected()
	{
		return _selected;
	}

	protected virtual void Awake()
	{
		create();
		DraggableLayoutElement draggableLayoutElement = default(DraggableLayoutElement);
		if (((Component)this).TryGetComponent<DraggableLayoutElement>(ref draggableLayoutElement))
		{
			DraggableLayoutElement draggableLayoutElement2 = draggableLayoutElement;
			draggableLayoutElement2.start_being_dragged = (Action<DraggableLayoutElement>)Delegate.Combine(draggableLayoutElement2.start_being_dragged, new Action<DraggableLayoutElement>(onStartDrag));
		}
	}

	protected virtual void onStartDrag(DraggableLayoutElement pOriginalElement)
	{
		AugmentationButton<TAugmentation> component = ((Component)pOriginalElement).GetComponent<AugmentationButton<TAugmentation>>();
		load(component.augmentation_asset);
		is_editor_button = component.is_editor_button;
	}

	protected virtual void create()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		if (!created)
		{
			created = true;
			button = ((Component)this).GetComponent<Button>();
			image = ((Component)((Component)this).transform.Find("TiltEffect/icon")).GetComponent<Image>();
			locked_bg = ((Component)((Component)this).transform.Find("TiltEffect/locked_bg")).GetComponent<Image>();
			((Component)locked_bg).gameObject.SetActive(false);
			initTooltip();
			Transform obj = ((Component)this).transform.FindRecursive("outline");
			_outline = ((obj != null) ? ((Component)obj).GetComponent<IconOutline>() : null);
			_shadow = ((Component)image).GetComponent<Shadow>();
			((UnityEvent)button.onClick).AddListener((UnityAction)delegate
			{
				_on_button_clicked?.Invoke(((Component)this).gameObject);
			});
		}
	}

	public virtual void load(TAugmentation pElement)
	{
		throw new NotImplementedException();
	}

	protected virtual void initTooltip()
	{
		TipButton tipButton = default(TipButton);
		if (((Component)this).TryGetComponent<TipButton>(ref tipButton))
		{
			tipButton.setHoverAction(showTooltip);
		}
	}

	protected virtual void Update()
	{
		throw new NotImplementedException();
	}

	protected void loadLegendaryOutline()
	{
		((Behaviour)_shadow).enabled = true;
		if (!((Object)(object)_outline == (Object)null))
		{
			if (getRarity() == Rarity.R3_Legendary)
			{
				showOutline(RarityLibrary.legendary.color_container);
			}
			else
			{
				((Component)_outline).gameObject.SetActive(false);
			}
		}
	}

	private void showOutline(ContainerItemColor pContainer)
	{
		if (!((Object)(object)_outline == (Object)null))
		{
			_outline.show(pContainer);
			((Behaviour)_shadow).enabled = false;
		}
	}

	public void showTooltip()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (_tooltip_enabled)
		{
			if (!is_editor_button && !augmentation_asset.unlocked_with_achievement && !isElementUnlocked() && !WorldLawLibrary.world_law_cursed_world.isEnabled() && unlockElement())
			{
				startSignal();
				_on_augmentation_unlocked?.Invoke();
			}
			if (!is_editor_button || InputHelpers.mouseSupported || !Tooltip.isShowingFor(this))
			{
				fillTooltipData(augmentation_asset);
			}
			((Component)this).transform.localScale = new Vector3(1f, 1f, 1f);
			ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, 0.8f, 0.1f), (Ease)26);
		}
	}

	public void addElementUnlockedAction(AugmentationUnlockedAction pAction)
	{
		_on_augmentation_unlocked = (AugmentationUnlockedAction)Delegate.Combine(_on_augmentation_unlocked, pAction);
	}

	public void removeElementUnlockedAction(AugmentationUnlockedAction pAction)
	{
		_on_augmentation_unlocked = (AugmentationUnlockedAction)Delegate.Remove(_on_augmentation_unlocked, pAction);
	}

	protected virtual void clearActions()
	{
		_on_augmentation_unlocked = null;
		clearClickActions();
	}

	public virtual void updateIconColor(bool pSelected)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		_selected = pSelected;
		if (is_editor_button)
		{
			if (!getElementAsset().isAvailable())
			{
				((Graphic)image).color = Toolbox.color_black;
			}
			else if (pSelected)
			{
				((Graphic)image).color = Toolbox.color_augmentation_selected;
			}
			else
			{
				((Graphic)image).color = Toolbox.color_augmentation_unselected;
			}
		}
	}

	public TAugmentation getElementAsset()
	{
		return augmentation_asset;
	}

	protected bool isElementUnlocked()
	{
		return augmentation_asset.isAvailable();
	}

	protected virtual bool unlockElement()
	{
		throw new NotImplementedException(((object)this).GetType().Name);
	}

	protected virtual void startSignal()
	{
	}

	protected virtual void fillTooltipData(TAugmentation pElement)
	{
		throw new NotImplementedException(((object)this).GetType().Name);
	}

	protected virtual TooltipData tooltipDataBuilder()
	{
		throw new NotImplementedException(((object)this).GetType().Name);
	}

	protected virtual string getElementType()
	{
		throw new NotImplementedException(((object)this).GetType().Name);
	}

	public virtual string getElementId()
	{
		throw new NotImplementedException(((object)this).GetType().Name);
	}

	protected virtual Rarity getRarity()
	{
		throw new NotImplementedException(((object)this).GetType().Name);
	}

	protected virtual void disableTooltip()
	{
		_tooltip_enabled = false;
	}

	public void addClickAction(AugmentationButtonClickAction pAction)
	{
		_on_button_clicked = (AugmentationButtonClickAction)Delegate.Combine(_on_button_clicked, pAction);
	}

	public void removeClickAction(AugmentationButtonClickAction pAction)
	{
		_on_button_clicked = (AugmentationButtonClickAction)Delegate.Remove(_on_button_clicked, pAction);
	}

	private void clearClickActions()
	{
		_on_button_clicked = null;
	}

	private void OnDestroy()
	{
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
	}
}
