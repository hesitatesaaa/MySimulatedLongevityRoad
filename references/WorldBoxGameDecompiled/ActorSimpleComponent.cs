using System;

public abstract class ActorSimpleComponent : IDisposable
{
	internal Actor actor;

	internal virtual void create(Actor pActor)
	{
		actor = pActor;
	}

	public virtual void update(float pElapsed)
	{
	}

	public virtual void Dispose()
	{
		actor = null;
	}
}
