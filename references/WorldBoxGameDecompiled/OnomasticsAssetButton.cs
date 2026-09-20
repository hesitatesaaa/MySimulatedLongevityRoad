using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class OnomasticsAssetButton : MonoBehaviour
{
	private bool _created;

	internal Image image;

	internal bool tooltip_enabled = true;

	internal Button button;

	public OnomasticsAsset onomastics_asset;

	public OnomasticsActionUpdate onomastics_action_update;

	private GetCurrentOnomasticsData _get_current_onomastics_data;

	private void Awake()
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
		OnomasticsAssetButton component = ((Component)pOriginalElement).GetComponent<OnomasticsAssetButton>();
		setupButton(component.onomastics_asset, component._get_current_onomastics_data);
	}

	public void setupButton(OnomasticsAsset pAsset, GetCurrentOnomasticsData pDelegate)
	{
		loadAsset(pAsset);
		setOnomasticsGetter(pDelegate);
		checkSpriteButtonColor();
	}

	public RectTransform getRect()
	{
		return ((Component)this).GetComponent<RectTransform>();
	}

	private void Update()
	{
		checkSpriteButtonColor();
	}

	public bool isGroupType()
	{
		return onomastics_asset.isGroupType();
	}

	private bool doesGroupHaveContent()
	{
		if (_get_current_onomastics_data == null)
		{
			return true;
		}
		OnomasticsData onomasticsData = _get_current_onomastics_data();
		if (onomasticsData == null || onomastics_asset == null)
		{
			return false;
		}
		if (!isGroupType())
		{
			return true;
		}
		return !onomasticsData.isGroupEmpty(onomastics_asset.id);
	}

	public void checkSpriteButtonColor()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (doesGroupHaveContent())
		{
			((Graphic)image).color = Color.white;
		}
		else
		{
			((Graphic)image).color = Color.gray;
		}
	}

	public void setOnomasticsGetter(GetCurrentOnomasticsData pDelegate)
	{
		_get_current_onomastics_data = pDelegate;
	}

	private void Start()
	{
		TipButton tipButton = default(TipButton);
		if (!((Component)this).TryGetComponent<TipButton>(ref tipButton))
		{
			return;
		}
		tipButton.setHoverAction(delegate
		{
			if (InputHelpers.mouseSupported)
			{
				showTooltip();
			}
		});
	}

	public void loadAsset(OnomasticsAsset pAsset)
	{
		onomastics_asset = pAsset;
		image.sprite = onomastics_asset.getSprite();
	}

	public void showTooltip()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (tooltip_enabled)
		{
			tooltipBuilder();
			((Component)this).transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
			ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, 1f, 0.1f), (Ease)26);
		}
	}

	private void tooltipBuilder()
	{
		Tooltip.show(this, "onomastics_asset", new TooltipData
		{
			onomastics_asset = onomastics_asset,
			onomastics_data = _get_current_onomastics_data()
		});
	}

	private void create()
	{
		if (!_created)
		{
			_created = true;
			button = ((Component)this).GetComponent<Button>();
			image = ((Component)((Component)this).transform.Find("TiltEffect/icon")).GetComponent<Image>();
		}
	}

	private void OnDestroy()
	{
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
	}
}
