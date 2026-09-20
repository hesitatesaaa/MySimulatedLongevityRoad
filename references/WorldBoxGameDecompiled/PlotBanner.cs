public class PlotBanner : BannerGeneric<Plot, PlotData>
{
	protected override MetaType meta_type => MetaType.Plot;

	protected override string tooltip_id => "plot";

	protected override TooltipData getTooltipData()
	{
		TooltipData tooltipData = base.getTooltipData();
		tooltipData.plot = meta_object;
		return tooltipData;
	}

	protected override void setupBanner()
	{
		base.setupBanner();
		PlotAsset asset = meta_object.getAsset();
		string pPath = "plots/backgrounds/plot_background_0";
		string path_icon = asset.path_icon;
		part_background.sprite = SpriteTextureLoader.getSprite(pPath);
		part_icon.sprite = SpriteTextureLoader.getSprite(path_icon);
	}
}
