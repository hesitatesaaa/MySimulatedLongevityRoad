using ai.behaviours;

public class BehCheckPlotIsOk : BehCheckPlotBase
{
	public override BehResult execute(Actor pActor)
	{
		if (!isBasePlotCheckOk(pActor))
		{
			pActor.leavePlot();
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
