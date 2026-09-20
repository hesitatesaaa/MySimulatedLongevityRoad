using System.Collections.Generic;
using UnityPools;
using ai.behaviours;

public class BehTryNewPlot : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasPlot())
		{
			return BehResult.Continue;
		}
		if (pActor.isFighting())
		{
			return BehResult.Continue;
		}
		using ListPool<PlotAsset> listPool = new ListPool<PlotAsset>();
		fillRandomPlots(pActor, listPool);
		if (listPool.Count == 0)
		{
			return BehResult.Continue;
		}
		startPlotFromTheList(pActor, listPool);
		return BehResult.Continue;
	}

	private void fillRandomPlots(Actor pActor, ListPool<PlotAsset> pPotPlots)
	{
		fillPlotsToTry(pActor, AssetManager.plots_library.basic_plots, pPotPlots);
		if (pActor.hasReligion() && WorldLawLibrary.world_law_rites.isEnabled())
		{
			fillPlotsToTry(pActor, pActor.religion.possible_rites, pPotPlots);
		}
		pPotPlots.Shuffle();
	}

	private void fillPlotsToTry(Actor pActor, List<PlotAsset> pPlotList, ListPool<PlotAsset> pPotPossiblePlots)
	{
		for (int i = 0; i < pPlotList.Count; i++)
		{
			PlotAsset plotAsset = pPlotList[i];
			if (plotAsset.checkIsPossible(pActor))
			{
				pPotPossiblePlots.AddTimes(plotAsset.pot_rate, plotAsset);
			}
		}
	}

	private void startPlotFromTheList(Actor pActor, ListPool<PlotAsset> pPotList)
	{
		HashSet<PlotAsset> hashSet = UnsafeCollectionPool<HashSet<PlotAsset>, PlotAsset>.Get();
		for (int i = 0; i < pPotList.Count; i++)
		{
			PlotAsset plotAsset = pPotList[i];
			if (!hashSet.Contains(plotAsset))
			{
				if (BehaviourActionBase<Actor>.world.plots.tryStartPlot(pActor, plotAsset))
				{
					break;
				}
				hashSet.Add(plotAsset);
			}
		}
		UnsafeCollectionPool<HashSet<PlotAsset>, PlotAsset>.Release(hashSet);
	}
}
