using ai.behaviours;

public class BehCheckPlotProgress : BehCheckPlotBase
{
	public override BehResult execute(Actor pActor)
	{
		if (!isBasePlotCheckOk(pActor))
		{
			pActor.leavePlot();
			return BehResult.Stop;
		}
		Plot plot = pActor.plot;
		plot.updateProgressTarget(pActor, pActor.stats["intelligence"]);
		if (!plot.isActive())
		{
			pActor.leavePlot();
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
