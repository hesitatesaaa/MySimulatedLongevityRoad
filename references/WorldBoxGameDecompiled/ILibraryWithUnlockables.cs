using System.Collections.Generic;

public interface ILibraryWithUnlockables
{
	IEnumerable<BaseUnlockableAsset> elements_list { get; }

	int countTotalKnowledge()
	{
		int num = 0;
		foreach (BaseUnlockableAsset item in elements_list)
		{
			if (item.show_in_knowledge_window)
			{
				num++;
			}
		}
		return num;
	}

	int countUnlockedByPlayer()
	{
		int num = 0;
		foreach (BaseUnlockableAsset item in elements_list)
		{
			if (item.show_in_knowledge_window && item.isUnlockedByPlayer())
			{
				num++;
			}
		}
		return num;
	}
}
