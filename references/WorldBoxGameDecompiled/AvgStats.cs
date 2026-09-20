public readonly struct AvgStats(double pAvg, int pCount, string pName)
{
	public readonly double avg = pAvg;

	public readonly int count = pCount;

	public readonly string name = pName;

	public AvgStats add(double pValue)
	{
		double pAvg = (avg * (double)count + pValue) / (double)(count + 1);
		int pCount = count + 1;
		return new AvgStats(pAvg, pCount, name);
	}
}
