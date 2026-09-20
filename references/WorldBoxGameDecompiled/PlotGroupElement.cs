public class PlotGroupElement : AugmentationCategory<PlotAsset, PlotButton, PlotEditorButton>
{
	protected override bool isUnlocked(PlotButton pButton)
	{
		if (pButton.getElementAsset().isAvailable())
		{
			return true;
		}
		return false;
	}
}
