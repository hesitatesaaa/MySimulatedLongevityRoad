using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChromosomeElement : MonoBehaviour
{
	private static readonly Color color_synergy_gold;

	private static readonly Color color_normal_blue;

	internal Chromosome chromosome;

	private ChromosomeClickEvent _click_event;

	public Image image;

	private void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		setupTooltip();
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener(new UnityAction(clickChromosome));
		DraggableLayoutElement draggableLayoutElement = default(DraggableLayoutElement);
		if (((Component)this).TryGetComponent<DraggableLayoutElement>(ref draggableLayoutElement))
		{
			DraggableLayoutElement draggableLayoutElement2 = draggableLayoutElement;
			draggableLayoutElement2.start_being_dragged = (Action<DraggableLayoutElement>)Delegate.Combine(draggableLayoutElement2.start_being_dragged, new Action<DraggableLayoutElement>(onStartDrag));
		}
	}

	protected virtual void onStartDrag(DraggableLayoutElement pOriginalElement)
	{
		ChromosomeElement component = ((Component)pOriginalElement).GetComponent<ChromosomeElement>();
		show(component.chromosome, null);
	}

	private void clickChromosome()
	{
		_click_event?.Invoke(chromosome);
	}

	public void show(Chromosome pChromosome, ChromosomeClickEvent pClickEvent)
	{
		chromosome = pChromosome;
		_click_event = pClickEvent;
		if (pChromosome.isAllLociSynergy())
		{
			image.sprite = chromosome.getSpriteGolden();
		}
		else
		{
			image.sprite = chromosome.getSpriteNormal();
		}
	}

	protected virtual void setupTooltip()
	{
		TipButton tipButton = default(TipButton);
		if (((Component)this).TryGetComponent<TipButton>(ref tipButton))
		{
			tipButton.setHoverAction(tooltipAction);
		}
	}

	protected void tooltipAction()
	{
		Tooltip.show(this, "chromosome", new TooltipData
		{
			chromosome = chromosome
		});
	}

	static ChromosomeElement()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		color_synergy_gold = Toolbox.makeColor("#FFF841");
		color_normal_blue = Toolbox.makeColor("#00B0FF");
	}
}
