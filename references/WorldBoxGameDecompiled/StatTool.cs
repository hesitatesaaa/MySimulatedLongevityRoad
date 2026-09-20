public static class StatTool
{
	public static float getDPS(Actor pActor)
	{
		ActorAsset asset = pActor.asset;
		float num = asset.base_stats["damage"];
		float num2 = 1f / asset.base_stats["attack_speed"];
		return num * num2;
	}

	public static float getSecondsLife(Actor pActor)
	{
		return pActor.asset.base_stats["lifespan"] * 60f;
	}

	public static string getStringSecondsLife(Actor pActor)
	{
		float pValue = pActor.asset.base_stats["lifespan"] * 60f;
		return pValue.ToString("0") + toMinutes(pValue);
	}

	public static string getAmountFood(Actor pActor)
	{
		float num = pActor.asset.nutrition_max;
		float interval_nutrition_decay = SimGlobals.m.interval_nutrition_decay;
		return (getSecondsLife(pActor) / (interval_nutrition_decay * num)).ToString("0.0");
	}

	public static string getStringAmountBreeding(Actor pActor)
	{
		ActorAsset asset = pActor.asset;
		if (!pActor.hasSubspecies())
		{
			return "0.0";
		}
		float num = (float)asset.months_breeding_timeout * 5f;
		float num2 = getSecondsLife(pActor) - pActor.subspecies.age_breeding * 60f;
		return (num2 / num).ToString("0.0") + toMinutes(num2);
	}

	private static string toMinutes(float pValue)
	{
		float num = pValue / 60f;
		float num2 = pValue / 60f;
		return " (" + num.ToString("0.0") + "m) " + num2.ToString("0.0") + "y";
	}
}
