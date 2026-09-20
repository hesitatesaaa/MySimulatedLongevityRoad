using System;
using System.Collections.Generic;

public class WindowHistory
{
	public static readonly List<WindowHistoryData> list = new List<WindowHistoryData>();

	public static Action historyClearCallback;

	public static void clear()
	{
		if (historyClearCallback != null)
		{
			historyClearCallback();
			historyClearCallback = null;
		}
		list.Clear();
	}

	public static void addIntoHistory(ScrollWindow pWindow)
	{
		WindowHistoryData pHistoryData = new WindowHistoryData
		{
			index = list.Count + 1,
			window = pWindow
		};
		foreach (MetaTypeAsset item in AssetManager.meta_type_library.list)
		{
			item.window_history_action_update?.Invoke(ref pHistoryData);
		}
		list.Add(pHistoryData);
	}

	public static void popHistory()
	{
		list?.Pop();
	}

	public static void clickBack()
	{
		if (!ScrollWindow.getCurrentWindow().historyActionEnabled || !returnWindowBack())
		{
			ScrollWindow.hideAllEvent();
		}
	}

	private static bool returnWindowBack()
	{
		if (!canReturnWindowBack())
		{
			return false;
		}
		WindowHistoryData pHistoryData = list.Pop();
		while (list.Count > 0)
		{
			pHistoryData = list.Pop();
			AssetManager.window_library.get(pHistoryData.window.screen_id).meta_type_asset?.window_history_action_restore?.Invoke(ref pHistoryData);
			if (!pHistoryData.window.shouldClose())
			{
				break;
			}
		}
		if (pHistoryData.window.shouldClose())
		{
			return false;
		}
		pHistoryData.window.clickShowLeft();
		return true;
	}

	public static bool canReturnWindowBack()
	{
		if (WorkshopUploadingWorldWindow.uploading)
		{
			return false;
		}
		if (list.Count < 2)
		{
			return false;
		}
		return true;
	}

	public static bool hasHistory()
	{
		return list.Count > 0;
	}

	public static void debug(DebugTool pTool)
	{
		pTool.setText("hasHistory:", hasHistory(), 0f, pShowBar: false, 0L);
		pTool.setText("canReturnWindowBack:", canReturnWindowBack(), 0f, pShowBar: false, 0L);
		pTool.setText("list.Count:", list.Count, 0f, pShowBar: false, 0L);
		foreach (WindowHistoryData item2 in list)
		{
			using ListPool<string> listPool = new ListPool<string>();
			Kingdom kingdom = item2.kingdom;
			if (kingdom != null && kingdom.isAlive())
			{
				listPool.Add(item2.kingdom.getTypeID());
			}
			Culture culture = item2.culture;
			if (culture != null && culture.isAlive())
			{
				listPool.Add(item2.culture.getTypeID());
			}
			Actor unit = item2.unit;
			if (unit != null && unit.isAlive())
			{
				listPool.Add(item2.unit.getTypeID());
			}
			City city = item2.city;
			if (city != null && city.isAlive())
			{
				listPool.Add(item2.city.getTypeID());
			}
			Clan clan = item2.clan;
			if (clan != null && clan.isAlive())
			{
				listPool.Add(item2.clan.getTypeID());
			}
			Plot plot = item2.plot;
			if (plot != null && plot.isAlive())
			{
				listPool.Add(item2.plot.getTypeID());
			}
			War war = item2.war;
			if (war != null && war.isAlive())
			{
				listPool.Add(item2.war.getTypeID());
			}
			Alliance alliance = item2.alliance;
			if (alliance != null && alliance.isAlive())
			{
				listPool.Add(item2.alliance.getTypeID());
			}
			Language language = item2.language;
			if (language != null && language.isAlive())
			{
				listPool.Add(item2.language.getTypeID());
			}
			Subspecies subspecies = item2.subspecies;
			if (subspecies != null && subspecies.isAlive())
			{
				listPool.Add(item2.subspecies.getTypeID());
			}
			Religion religion = item2.religion;
			if (religion != null && religion.isAlive())
			{
				listPool.Add(item2.religion.getTypeID());
			}
			Family family = item2.family;
			if (family != null && family.isAlive())
			{
				listPool.Add(item2.family.getTypeID());
			}
			Army army = item2.army;
			if (army != null && army.isAlive())
			{
				listPool.Add(item2.army.getTypeID());
			}
			Item item = item2.item;
			if (item != null && item.isAlive())
			{
				listPool.Add(item2.item.getTypeID());
			}
			pTool.setText($"history {item2.index} {item2.window.screen_id}:", string.Join(",", listPool), 0f, pShowBar: false, 0L);
		}
	}
}
