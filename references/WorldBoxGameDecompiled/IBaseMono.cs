using UnityEngine;

public interface IBaseMono
{
	Transform transform { get; }

	GameObject gameObject { get; }

	T GetComponent<T>();

	T AddComponent<T>() where T : Component
	{
		return gameObject.AddComponent<T>();
	}

	bool HasComponent<T>() where T : Component
	{
		return gameObject.HasComponent<T>();
	}
}
