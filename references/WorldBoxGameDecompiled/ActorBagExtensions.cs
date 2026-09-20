using System.Collections.Generic;

public static class ActorBagExtensions
{
	public static ActorBag add(this ActorBag pBag, ResourceContainer pResourceContainer)
	{
		return pBag.add(pResourceContainer.id, pResourceContainer.amount);
	}

	public static ActorBag add(this ActorBag pBag, string pID, int pAmount)
	{
		if (pBag == null)
		{
			pBag = new ActorBag();
		}
		if (pBag.dict == null)
		{
			pBag.dict = new Dictionary<string, ResourceContainer>();
		}
		if (pBag.dict.TryGetValue(pID, out var value))
		{
			value.amount += pAmount;
		}
		else
		{
			value = new ResourceContainer(pID, pAmount);
		}
		pBag.dict[pID] = value;
		pBag.last_item_to_render = null;
		return pBag;
	}

	public static ActorBag remove(this ActorBag pBag, string pID, int pAmount)
	{
		if (pBag.isEmpty())
		{
			return null;
		}
		if (pBag.dict.TryGetValue(pID, out var value))
		{
			value.amount -= pAmount;
			if (value.amount <= 0)
			{
				pBag.dict.Remove(pID);
			}
			else
			{
				pBag.dict[pID] = value;
			}
			pBag.last_item_to_render = null;
		}
		return pBag;
	}

	public static Dictionary<string, ResourceContainer> getResources(this ActorBag pBag)
	{
		return pBag.dict;
	}

	public static bool hasResources(this ActorBag pBag)
	{
		if (pBag == null)
		{
			return false;
		}
		return pBag.dict?.Count > 0;
	}

	public static bool isEmpty(this ActorBag pBag)
	{
		return !pBag.hasResources();
	}

	public static string getRandomResourceID(this ActorBag pBag)
	{
		if (pBag.isEmpty())
		{
			return string.Empty;
		}
		if (pBag.dict.Count == 0)
		{
			return string.Empty;
		}
		int num = Randy.randomInt(0, pBag.dict.Count);
		int num2 = 0;
		foreach (string key in pBag.dict.Keys)
		{
			if (num2 == num)
			{
				return key;
			}
			num2++;
		}
		return string.Empty;
	}

	public static int getResource(this ActorBag pBag, string pID)
	{
		if (pBag.isEmpty())
		{
			return 0;
		}
		if (pBag.dict.TryGetValue(pID, out var value))
		{
			return value.amount;
		}
		return 0;
	}

	public static void empty(this ActorBag pBag)
	{
		if (!pBag.isEmpty())
		{
			pBag.dict.Clear();
		}
		pBag = null;
	}

	public static string getItemIDToRender(this ActorBag pBag)
	{
		if (pBag.hasResources())
		{
			if (!string.IsNullOrEmpty(pBag.last_item_to_render))
			{
				return pBag.last_item_to_render;
			}
			using Dictionary<string, ResourceContainer>.ValueCollection.Enumerator enumerator = pBag.getResources().Values.GetEnumerator();
			if (enumerator.MoveNext())
			{
				pBag.last_item_to_render = enumerator.Current.id;
				return pBag.last_item_to_render;
			}
		}
		return string.Empty;
	}

	public static int countResources(this ActorBag pBag)
	{
		if (pBag.isEmpty())
		{
			return 0;
		}
		return pBag.dict.Count;
	}
}
