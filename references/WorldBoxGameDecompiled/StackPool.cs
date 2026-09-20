using System.Collections.Generic;

public class StackPool<T> where T : new()
{
	public Stack<T> pool = new Stack<T>();

	public T get()
	{
		if (pool.Count > 0)
		{
			return pool.Pop();
		}
		return new T();
	}

	public U get<U>() where U : T, new()
	{
		if (pool.Count > 0)
		{
			return (U)(object)pool.Pop();
		}
		return new U();
	}

	public void release(T pObject)
	{
		pool.Push(pObject);
	}

	public void clear()
	{
		pool.Clear();
	}
}
