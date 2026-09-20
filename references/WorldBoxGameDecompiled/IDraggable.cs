using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggable : IEndDragHandler, IEventSystemHandler
{
	Transform transform { get; }

	bool spawn_particles_on_drag { get; }

	bool HasComponent<T>()
	{
		return ((Component)(object)transform).HasComponent<T>();
	}

	void KillDrag();
}
