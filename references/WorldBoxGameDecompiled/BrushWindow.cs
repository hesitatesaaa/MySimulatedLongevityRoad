using UnityEngine;

public class BrushWindow : MonoBehaviour
{
	public Transform circles;

	public Transform squares;

	public Transform diamonds;

	public Transform special;

	public BrushSelectButton button_prefab;

	public void Awake()
	{
		foreach (BrushData item in AssetManager.brush_library.list)
		{
			if (item.show_in_brush_window)
			{
				Transform val = null;
				switch (item.group)
				{
				case BrushGroup.Circles:
					val = circles;
					break;
				case BrushGroup.Squares:
					val = squares;
					break;
				case BrushGroup.Diamonds:
					val = diamonds;
					break;
				case BrushGroup.Special:
					val = special;
					break;
				default:
					continue;
				}
				Object.Instantiate<BrushSelectButton>(button_prefab, val).setup(item);
			}
		}
	}

	public void selectBrush(GameObject pObject)
	{
		Config.current_brush = ((Object)pObject.transform).name;
		((Component)this).GetComponent<ScrollWindow>().clickHide();
	}
}
