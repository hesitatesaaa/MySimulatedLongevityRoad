using System;

[Serializable]
public class RewardedPower
{
	public string name;

	public double timeStamp;

	public RewardedPower(string pName, double pTimeStamp)
	{
		name = pName;
		timeStamp = pTimeStamp;
	}
}
