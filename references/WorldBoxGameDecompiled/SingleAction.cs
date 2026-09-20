public class SingleAction<TTask, TAction> where TTask : BehaviourTaskBase<TAction> where TAction : BehaviourElementAI
{
	public float timer;

	public float interval = 1f;

	public float interval_random = 1f;

	public TTask task;

	public SingleAction(TTask pTask)
	{
		task = pTask;
		interval = pTask.single_interval;
		interval_random = pTask.single_interval_random;
	}

	public void reset()
	{
		timer = interval + Randy.randomFloat(0f, interval_random);
	}
}
