public class BaseMapObjectSimple
{
	internal bool created;

	public virtual void update(float pElapsed)
	{
	}

	internal virtual void create()
	{
		created = true;
	}
}
