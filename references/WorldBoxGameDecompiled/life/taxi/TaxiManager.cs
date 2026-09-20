using System.Collections.Generic;

namespace life.taxi;

public class TaxiManager
{
	public static List<TaxiRequest> list = new List<TaxiRequest>();

	private static float timer_check = 0f;

	public static void newRequest(Actor pActor, WorldTile pTileTarget)
	{
		if (pActor.is_inside_boat)
		{
			return;
		}
		TaxiRequest taxiRequest = null;
		foreach (TaxiRequest item in list)
		{
			if (item.isSameKingdom(pActor.kingdom) && !item.isState(TaxiRequestState.Transporting) && !item.isState(TaxiRequestState.Finished) && item.getTileTarget().isSameIsland(pTileTarget) && item.getTileStart().isSameIsland(pActor.current_tile))
			{
				taxiRequest = item;
				break;
			}
		}
		if (taxiRequest != null)
		{
			taxiRequest.addActor(pActor);
			return;
		}
		taxiRequest = new TaxiRequest(pActor, pActor.kingdom, pActor.current_tile, pTileTarget);
		list.Add(taxiRequest);
	}

	public static void cancelRequest(TaxiRequest pRequest)
	{
		pRequest.cancel();
		list.Remove(pRequest);
	}

	public static TaxiRequest getRequestForActor(Actor pActor)
	{
		foreach (TaxiRequest item in list)
		{
			if (item.hasActor(pActor))
			{
				return item;
			}
		}
		return null;
	}

	public static TaxiRequest getNewRequestForBoat(Actor pBoatActor)
	{
		TaxiRequest taxiRequest = null;
		foreach (TaxiRequest item in list)
		{
			if (item.isAlreadyUsedByBoat(pBoatActor))
			{
				return item;
			}
			if (item.isState(TaxiRequestState.Pending) && item.isStillLegit() && item.isSameKingdom(pBoatActor.kingdom) && (taxiRequest == null || taxiRequest.countActors() < item.countActors()))
			{
				taxiRequest = item;
			}
		}
		return taxiRequest;
	}

	public static void clear()
	{
		foreach (TaxiRequest item in list)
		{
			item.clear();
		}
		list.Clear();
	}

	public static void finish(TaxiRequest pRequest)
	{
		pRequest.finish();
		list.Remove(pRequest);
	}

	public static void cancelTaxiRequestForActor(Actor pActor)
	{
		TaxiRequest requestForActor = getRequestForActor(pActor);
		if (requestForActor != null)
		{
			cancelRequest(requestForActor);
		}
	}

	public static void update(float pElapsed)
	{
		if (timer_check > 0f)
		{
			timer_check -= pElapsed;
			return;
		}
		timer_check = 5f;
		int num = 0;
		while (list.Count > num)
		{
			TaxiRequest taxiRequest = list[num];
			if (taxiRequest.isStillLegit())
			{
				num++;
				continue;
			}
			taxiRequest.finish();
			list.RemoveAt(num);
		}
	}

	public static void removeDeadUnits()
	{
		foreach (TaxiRequest item in list)
		{
			item.removeDeadUnits();
		}
	}
}
