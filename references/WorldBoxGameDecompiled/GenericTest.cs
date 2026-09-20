using System.Collections.Generic;

public class GenericTest
{
	private List<object> list = new List<object>();

	public T get<T>(int pI) where T : class
	{
		return list[pI] as T;
	}

	public void Add(object pObject)
	{
		list.Add(pObject);
	}
}
