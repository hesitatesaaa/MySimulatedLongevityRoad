public class Job<T>
{
	public JobUpdater job_updater;

	public ObjectContainer<T> container;

	public string id;

	public double time_benchmark;

	public int counter;

	public int random_tick_skips;

	public int current_skips;
}
