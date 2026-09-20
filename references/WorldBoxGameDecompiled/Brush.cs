using System;

public class Brush
{
	public static string getRandom()
	{
		return AssetManager.brush_library.list.GetRandom().id;
	}

	public static string getRandom(int pMinSize, int pMaxSize = 50, Predicate<BrushData> pMatch = null)
	{
		foreach (BrushData item in AssetManager.brush_library.list.LoopRandom())
		{
			if ((pMatch == null || pMatch(item)) && item.sqr_size >= pMinSize && item.sqr_size <= pMaxSize)
			{
				return item.id;
			}
		}
		return "circ_1";
	}

	public static BrushData get(int pSize, string pID = "circ_")
	{
		string text = pID + pSize;
		BrushData brushData = AssetManager.brush_library.get(text);
		if (brushData != null)
		{
			return brushData;
		}
		brushData = AssetManager.brush_library.clone(text, pID + "1");
		brushData.size = pSize;
		AssetManager.brush_library.post_init();
		return brushData;
	}

	public static BrushData get(string pID)
	{
		return AssetManager.brush_library.get(pID);
	}
}
