using UnityEngine;
using UnityEngine.UI;

public class PlotWindow : WindowMetaGeneric<Plot, PlotData>
{
	[SerializeField]
	private GameObject content_author;

	[SerializeField]
	private PrefabUnitElement author_element;

	public StatsIcon text_info_age;

	public StatsIcon text_info_members;

	public StatsIcon text_info_power;

	public StatsIcon text_info_dead;

	public Text text_description;

	public StatBar bar;

	public override MetaType meta_type => MetaType.Plot;

	protected override Plot meta_object => SelectedMetas.selected_plot;

	public override void startShowingWindow()
	{
		base.startShowingWindow();
		AchievementLibrary.plots_explorer.check();
	}

	protected override void showTopPartInformation()
	{
		Plot plot = meta_object;
		if (plot != null)
		{
			if (plot.getAsset().needs_to_be_explored)
			{
				plot.getAsset().unlock();
			}
			float progress = plot.getProgress();
			float progressMax = plot.getProgressMax();
			bar.setBar(progress, progressMax, "/" + progressMax.ToText(), pReset: true, pFloat: true);
			text_description.text = plot.getAsset().get_formatted_description(plot);
			((Component)text_description).GetComponent<LocalizedText>().checkTextFont();
			((Component)text_description).GetComponent<LocalizedText>().checkSpecialLanguages();
			showAuthor();
		}
	}

	private void showAuthor()
	{
		Actor author = meta_object.getAuthor();
		if (author.isRekt())
		{
			content_author.SetActive(false);
			return;
		}
		content_author.SetActive(true);
		author_element.show(author);
	}

	internal override void showStatsRows()
	{
		Plot plot = meta_object;
		if (plot != null)
		{
			tryShowPastNames();
			showStatRow("started_by", plot.data.founder_name, MetaType.Unit, plot.data.founder_id);
			showStatRow("started_at", plot.getFoundedDate(), MetaType.None, -1L);
		}
	}
}
