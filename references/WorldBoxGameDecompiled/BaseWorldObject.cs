using System;
using UnityEngine;

public class BaseWorldObject : MonoBehaviour, IDisposable
{
	internal bool created;

	internal Transform m_transform;

	private void Start()
	{
		if (!created)
		{
			create();
		}
	}

	public virtual void update(float pElapsed)
	{
	}

	internal virtual void create()
	{
		created = true;
		m_transform = ((Component)this).gameObject.transform;
	}

	public virtual void Dispose()
	{
		m_transform = null;
	}
}
