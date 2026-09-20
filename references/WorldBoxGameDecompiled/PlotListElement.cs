using UnityEngine;
using UnityEngine.UI;

public class PlotListElement : WindowListElementBase<Plot, PlotData>
{
	[SerializeField]
	private Text _text_name;

	[SerializeField]
	private CountUpOnClick _members;

	[SerializeField]
	private CountUpOnClick _age;

	[SerializeField]
	private CountUpOnClick _progress;

	[SerializeField]
	private UiUnitAvatarElement _avatar_loader;

	[SerializeField]
	private StatBar _bar;

	[SerializeField]
	private GameObject _locked_effect;

	internal override void show(Plot pPlot)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		base.show(pPlot);
		Actor author = pPlot.getAuthor();
		_avatar_loader.show(author);
		ColorAsset colorAsset = null;
		if (author != null)
		{
			colorAsset = author.kingdom.getColor();
		}
		if (colorAsset != null)
		{
			((Graphic)_text_name).color = author.kingdom.getColor().getColorText();
		}
		else
		{
			((Graphic)_text_name).color = Toolbox.color_white;
		}
		_text_name.text = pPlot.data.name;
		_members.setValue(pPlot.getSupporters());
		_progress.setValue((int)pPlot.getProgress(), "/" + pPlot.getProgressMax().ToText());
		float progress = pPlot.getProgress();
		float progressMax = pPlot.getProgressMax();
		_bar.setBar(progress, progressMax, "/" + progressMax.ToText(), pReset: true, pFloat: true);
		_age.setValue(pPlot.getAge());
		if (pPlot.getAsset().isAvailable())
		{
			_locked_effect.gameObject.SetActive(false);
		}
		else
		{
			_locked_effect.gameObject.SetActive(true);
		}
	}

	protected override void tooltipAction()
	{
		Tooltip.show(this, "plot", new TooltipData
		{
			plot = meta_object
		});
	}
}
