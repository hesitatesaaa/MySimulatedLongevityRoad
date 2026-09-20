using System.Collections.Generic;

public class JobManagerActors : JobManagerBase<BatchActors, Actor>
{
	public List<BatchActors> active_batches => _batches_active;

	public JobManagerActors(string pID)
		: base(pID)
	{
		benchmark_id = "actors";
	}
}
