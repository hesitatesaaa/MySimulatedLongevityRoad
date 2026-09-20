using System.Collections.Generic;

public class PlotManager : MetaSystemManager<Plot, PlotData>
{
	public PlotManager()
	{
		type_id = "plot";
	}

	public override Plot loadObject(PlotData pData)
	{
		if (AssetManager.plots_library.get(pData.plot_type_id) == null)
		{
			return null;
		}
		return base.loadObject(pData);
	}

	public override void startCollectHistoryData()
	{
	}

	public override void clearLastYearStats()
	{
	}

	public void cancelPlot(Plot pPlot)
	{
		pPlot.finishPlot(PlotState.Cancelled, null);
	}

	public bool tryStartPlot(Actor pActor, PlotAsset pPlotAsset, bool pForced = true)
	{
		if (pPlotAsset.try_to_start_advanced != null)
		{
			return pPlotAsset.try_to_start_advanced(pActor, pPlotAsset, pForced);
		}
		World.world.plots.newPlot(pActor, pPlotAsset, pForced);
		return true;
	}

	public Plot newPlot(Actor pAuthor, PlotAsset pAsset, bool pForced)
	{
		World.world.game_stats.data.plotsStarted++;
		World.world.map_stats.plotsStarted++;
		Plot plot = newObject();
		plot.newPlot(pAuthor, pAsset, pForced);
		plot.data.founder_name = pAuthor.getName();
		if (!pForced)
		{
			pAuthor.spendMoney(pAsset.money_cost);
		}
		return plot;
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		updateProgress(pElapsed);
		updateAnimations(pElapsed);
		checkForgottenPlots();
	}

	private void updateProgress(float pElapsed)
	{
		if (World.world.isPaused())
		{
			return;
		}
		using IEnumerator<Plot> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.updateProgress(pElapsed);
		}
	}

	private void updateAnimations(float pElapsed)
	{
		using IEnumerator<Plot> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.updateAnimations(pElapsed);
		}
	}

	private void checkForgottenPlots()
	{
		using IEnumerator<Plot> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Plot current = enumerator.Current;
			if (current.last_update_progress != 0.0 && current.isActive() && (World.world.getWorldTimeElapsedSince(current.last_update_progress) > SimGlobals.m.forgotten_plot_time || current.isAuthorDead()))
			{
				cancelPlot(current);
			}
		}
	}

	public override void removeObject(Plot pPlot)
	{
		base.removeObject(pPlot);
	}

	protected override void updateDirtyUnits()
	{
		List<Actor> units_only_alive = World.world.units.units_only_alive;
		for (int i = 0; i < units_only_alive.Count; i++)
		{
			Actor actor = units_only_alive[i];
			Plot plot = actor.plot;
			if (plot != null && plot.isDirtyUnits())
			{
				plot.listUnit(actor);
			}
		}
		using IEnumerator<Plot> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Plot current = enumerator.Current;
			if (current.isActive() && current.isDirtyUnits() && current.units.Count == 0)
			{
				cancelPlot(current);
			}
		}
	}

	public bool isPlotTypeAlreadyRunning(Actor pActor, PlotAsset pPlotAsset)
	{
		using (IEnumerator<Plot> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Plot current = enumerator.Current;
				if (current.isActive() && current.isSameType(pPlotAsset))
				{
					return true;
				}
			}
		}
		return false;
	}
}
