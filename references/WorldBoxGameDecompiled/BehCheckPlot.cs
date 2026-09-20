using ai.behaviours;

public class BehCheckPlot : BehCheckPlotBase
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasPlot())
		{
			return BehResult.Stop;
		}
		if (!isBasePlotCheckOk(pActor))
		{
			pActor.leavePlot();
			return BehResult.Stop;
		}
		return forceTask(pActor, "progress_plot");
	}
}
