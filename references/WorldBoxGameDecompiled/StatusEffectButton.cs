using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatusEffectButton : MonoBehaviour
{
	private Status _status;

	internal Image image;

	internal bool tooltip_enabled = true;

	internal Button button;

	private bool _updatable_tooltip;

	public Status status => _status;

	private void Awake()
	{
		button = ((Component)this).GetComponent<Button>();
		image = ((Component)((Component)this).transform.Find("icon")).GetComponent<Image>();
		DraggableLayoutElement draggableLayoutElement = default(DraggableLayoutElement);
		if (((Component)this).TryGetComponent<DraggableLayoutElement>(ref draggableLayoutElement))
		{
			DraggableLayoutElement draggableLayoutElement2 = draggableLayoutElement;
			draggableLayoutElement2.start_being_dragged = (Action<DraggableLayoutElement>)Delegate.Combine(draggableLayoutElement2.start_being_dragged, new Action<DraggableLayoutElement>(onStartDrag));
		}
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		((UnityEvent)button.onClick).AddListener(new UnityAction(showTooltip));
		button.OnHover(new UnityAction(showHoverTooltip));
		button.OnHoverOut(new UnityAction(Tooltip.hideTooltip));
	}

	internal void load(Status pData)
	{
		if (pData != null)
		{
			_status = pData;
			image.sprite = pData.asset.getSprite();
		}
	}

	protected virtual void onStartDrag(DraggableLayoutElement pOriginalElement)
	{
		StatusEffectButton component = ((Component)pOriginalElement).GetComponent<StatusEffectButton>();
		load(component._status);
	}

	private void OnDisable()
	{
		Tooltip.hideTooltip();
	}

	private void showHoverTooltip()
	{
		if (Config.tooltips_active)
		{
			showTooltip();
		}
	}

	private void showTooltip()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (tooltip_enabled)
		{
			string pType = (_updatable_tooltip ? "status_updatable" : "status");
			string localeID = _status.asset.getLocaleID();
			string descriptionID = _status.asset.getDescriptionID();
			Tooltip.show(this, pType, new TooltipData
			{
				tip_name = localeID,
				tip_description = descriptionID,
				status = _status
			});
			((Component)this).transform.localScale = new Vector3(1f, 1f, 1f);
			ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, 0.8f, 0.1f), (Ease)26);
		}
	}

	public void setUpdatableTooltip(bool pState)
	{
		_updatable_tooltip = pState;
	}

	private void OnDestroy()
	{
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
	}
}
