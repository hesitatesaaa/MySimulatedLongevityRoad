using System.Collections.Generic;

public class KingdomCheckCache
{
	public Dictionary<long, bool> dict = new Dictionary<long, bool>();

	public long getHash(Kingdom pK1, Kingdom pK2)
	{
		int hashCode = pK1.GetHashCode();
		int hashCode2 = pK2.GetHashCode();
		if (hashCode > hashCode2)
		{
			return hashCode * 1000000 + hashCode2;
		}
		return hashCode2 * 1000000 + hashCode;
	}

	public void clear()
	{
		dict.Clear();
	}
}
