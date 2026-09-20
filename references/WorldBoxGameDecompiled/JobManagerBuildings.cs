public class JobManagerBuildings : JobManagerBase<BatchBuildings, Building>
{
	public JobManagerBuildings(string pID)
		: base(pID)
	{
		benchmark_id = "buildings";
	}
}
