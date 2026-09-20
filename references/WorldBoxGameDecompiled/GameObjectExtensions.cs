using UnityEngine;

public static class GameObjectExtensions
{
	public static T AddOrGetComponent<T>(this GameObject pGameObject) where T : Component
	{
		T result = default(T);
		if (!pGameObject.TryGetComponent<T>(ref result))
		{
			return pGameObject.AddComponent<T>();
		}
		return result;
	}

	public static bool HasComponent<T>(this GameObject pGameObject)
	{
		T val = default(T);
		return pGameObject.TryGetComponent<T>(ref val);
	}

	public static bool HasComponent<T>(this Component pComponent)
	{
		return pComponent.gameObject.HasComponent<T>();
	}

	public static T AddComponent<T>(this Component pComponent) where T : Component
	{
		return pComponent.gameObject.AddComponent<T>();
	}
}
