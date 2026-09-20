using System;
using UnityEngine;

public class BaseActorComponent : MonoBehaviour, IDisposable
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
