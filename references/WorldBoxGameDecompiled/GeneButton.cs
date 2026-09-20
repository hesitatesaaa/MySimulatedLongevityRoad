using System;
using UnityEngine;
using UnityEngine.Events;

public class GeneButton : ChainElement
{
	[SerializeField]
	private GameObject _petri_bg;

	private GeneAssetClickEvent _gene_asset_click_event;

	protected override void create()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.create();
		((UnityEvent)button.onClick).AddListener(new UnityAction(click));
	}

	private void click()
	{
		_gene_asset_click_event?.Invoke(base.gene);
		if (!InputHelpers.mouseSupported)
		{
			((Component)this).GetComponent<TipButton>().hoverAction();
		}
	}

	protected override void onStartDrag(DraggableLayoutElement pOriginalElement)
	{
		base.onStartDrag(pOriginalElement);
		_petri_bg.SetActive(false);
		colorChains();
		bool active = !augmentation_asset.isUnlocked();
		((Component)locked_bg).gameObject.SetActive(active);
	}

	internal void locusChild(UnityAction pAction, int pLocusIndex)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		hideChains();
		((UnityEvent)button.onClick).RemoveListener(new UnityAction(click));
		((UnityEvent)button.onClick).RemoveListener(pAction);
		((UnityEvent)button.onClick).AddListener(pAction);
		locus_index = pLocusIndex;
		disableTooltip();
	}

	protected override void fillTooltipData(GeneAsset pElement)
	{
		Tooltip.show(this, "gene", tooltipDataBuilder());
	}

	protected override TooltipData tooltipDataBuilder()
	{
		return new TooltipData
		{
			gene = base.gene
		};
	}

	public void addGeneClickCallback(GeneAssetClickEvent pAction)
	{
		_gene_asset_click_event = (GeneAssetClickEvent)Delegate.Combine(_gene_asset_click_event, pAction);
	}

	public void removeGeneClickCallback(GeneAssetClickEvent pAction)
	{
		_gene_asset_click_event = (GeneAssetClickEvent)Delegate.Remove(_gene_asset_click_event, pAction);
	}
}
