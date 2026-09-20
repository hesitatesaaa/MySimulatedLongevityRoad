using ai.behaviours;

public class BehMadnessRandomEmotion : BehaviourActionActor
{
	private const int STATUS_DURATION = 10;

	public override BehResult execute(Actor pActor)
	{
		if (Randy.randomBool())
		{
			using (ListPool<string> listPool = new ListPool<string>())
			{
				listPool.Add("laughing");
				listPool.Add("crying");
				listPool.Add("swearing");
				string random = listPool.GetRandom();
				pActor.addStatusEffect(random, 10f, pColorEffect: false);
				return BehResult.Continue;
			}
		}
		using ListPool<string> listPool2 = new ListPool<string>();
		listPool2.Add("happy_laughing");
		listPool2.Add("crying");
		listPool2.Add("swearing");
		if (listPool2.Count == 0)
		{
			return BehResult.Stop;
		}
		string random2 = listPool2.GetRandom();
		return forceTask(pActor, random2, pClean: false, pForceAction: true);
	}
}
