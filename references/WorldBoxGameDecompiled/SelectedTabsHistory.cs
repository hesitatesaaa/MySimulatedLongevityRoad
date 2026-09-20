using System.Collections.Generic;

public class SelectedTabsHistory
{
	private static Stack<TabHistoryData> _stack = new Stack<TabHistoryData>();

	public static void addToHistory(NanoObject pObject)
	{
		if (!_stack.TryPeek(out var result) || result.id != pObject.id || result.meta_type != pObject.getMetaType())
		{
			TabHistoryData item = new TabHistoryData(pObject);
			_stack.Push(item);
		}
	}

	public static bool showPreviousTab()
	{
		if (!_stack.TryPop(out var _))
		{
			return false;
		}
		TabHistoryData result2;
		MetaTypeAsset asset;
		NanoObject nanoObject;
		do
		{
			if (!_stack.TryPop(out result2))
			{
				return false;
			}
			asset = AssetManager.meta_type_library.getAsset(result2.meta_type);
			nanoObject = asset.get(result2.id);
		}
		while (nanoObject.isRekt());
		if (result2.meta_type == MetaType.Unit)
		{
			SelectedUnit.select(nanoObject as Actor);
			SelectedObjects.setNanoObject(SelectedUnit.unit);
			PowerTabController.showTabSelectedUnit();
		}
		else
		{
			asset.selectAndInspect(nanoObject, pFromNameplate: false, pCheckNameplate: false);
		}
		return true;
	}

	public static bool hasHistory()
	{
		return _stack.Count > 0;
	}

	public static int count()
	{
		int num = 0;
		foreach (TabHistoryData item in _stack)
		{
			if (!item.getNanoObject().isRekt())
			{
				num++;
			}
		}
		return num;
	}

	public static TabHistoryData? getPrevData()
	{
		int num = 1;
		while (true)
		{
			int num2 = 0;
			foreach (TabHistoryData item in _stack)
			{
				if (num != num2)
				{
					num2++;
					continue;
				}
				if (item.getNanoObject().isRekt())
				{
					num++;
					if (num > _stack.Count - 1)
					{
						return null;
					}
					break;
				}
				return item;
			}
		}
	}

	public static void clear()
	{
		_stack.Clear();
	}
}
