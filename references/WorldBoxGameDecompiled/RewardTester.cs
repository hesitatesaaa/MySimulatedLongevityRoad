internal sealed class RewardTester
{
	internal bool haveRew(string pPowID)
	{
		RewardedPower rewardedPower = null;
		foreach (RewardedPower rewardedPower2 in PlayerConfig.instance.data.rewardedPowers)
		{
			if (rewardedPower2.name == pPowID)
			{
				rewardedPower = rewardedPower2;
				break;
			}
		}
		if (rewardedPower == null)
		{
			return false;
		}
		return true;
	}

	internal bool checkRew()
	{
		if (PlayerConfig.instance.data.rewardedPowers.Count == 0)
		{
			return false;
		}
		double num = Epoch.Current();
		int num2 = 1860;
		bool flag = false;
		int num3 = 0;
		while (num3 < PlayerConfig.instance.data.rewardedPowers.Count)
		{
			RewardedPower rewardedPower = PlayerConfig.instance.data.rewardedPowers[num3];
			bool flag2 = false;
			if (rewardedPower.timeStamp > num)
			{
				flag2 = true;
			}
			if (num - rewardedPower.timeStamp > (double)num2)
			{
				flag2 = true;
			}
			if (flag2)
			{
				PlayerConfig.instance.data.rewardedPowers.RemoveAt(num3);
				flag = true;
			}
			else
			{
				num3++;
			}
		}
		if (flag)
		{
			PlayerConfig.saveData();
			return true;
		}
		return false;
	}
}
