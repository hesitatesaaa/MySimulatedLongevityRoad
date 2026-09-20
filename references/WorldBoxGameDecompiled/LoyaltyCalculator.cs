using System.Collections.Generic;

public static class LoyaltyCalculator
{
	public static Dictionary<LoyaltyAsset, int> results = new Dictionary<LoyaltyAsset, int>();

	public static int total = 0;

	public static int calculate(City pCity)
	{
		clear();
		foreach (LoyaltyAsset item in AssetManager.loyalty_library.list)
		{
			int num = item.calc(pCity);
			total += num;
			if (num != 0)
			{
				results.Add(item, num);
			}
		}
		return total;
	}

	private static void clear()
	{
		total = 0;
		results.Clear();
	}
}
