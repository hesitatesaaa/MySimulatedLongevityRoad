using UnityEngine;

public class SelectedElementBase<TComponent> : MonoBehaviour where TComponent : Component
{
	[SerializeField]
	protected Transform _grid;

	protected ObjectPoolGenericMono<TComponent> _pool;

	protected void clear()
	{
		_pool?.clear();
	}

	protected virtual void refresh(NanoObject pNano)
	{
	}

	private void OnDisable()
	{
		clear();
	}
}
