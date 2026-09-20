using System;

public class BaseBuildingComponent : IDisposable
{
	internal Building building;

	internal virtual void create(Building pBuilding)
	{
		building = pBuilding;
	}

	public virtual void update(float pElapsed)
	{
	}

	public virtual void Dispose()
	{
		building = null;
	}
}
