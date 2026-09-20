using System;

[Serializable]
public class BehaviourElementAI : Asset
{
	[NonSerialized]
	internal RateCounter rate_counter_calls;

	[NonSerialized]
	internal RateCounter rate_counter_performance;

	public override void create()
	{
		base.create();
		rate_counter_calls = new RateCounter("calls", 1);
		rate_counter_performance = new RateCounter("performance", 1);
	}
}
