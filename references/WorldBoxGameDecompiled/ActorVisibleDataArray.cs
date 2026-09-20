using System.Collections;

public class ActorVisibleDataArray
{
	public Actor[] array = new Actor[0];

	public int count;

	public void prepare(int pTargetSize)
	{
		array = Toolbox.checkArraySize(array, pTargetSize);
	}

	public void addFromCollection(ICollection pList)
	{
		if (pList.Count != 0)
		{
			pList.CopyTo(array, count);
			count += pList.Count;
		}
	}
}
