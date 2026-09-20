using UnityEngine;

public interface ISelectedContainerTrait
{
	Transform transform { get; }

	void update(NanoObject pNano);
}
