using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BrushSelectButton : MonoBehaviour
{
	public Image icon;

	private BrushData _brush_asset;

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
		{
			Config.current_brush = _brush_asset.id;
			ScrollWindow.hideAllEvent();
		});
	}

	public void setup(BrushData pBrushData)
	{
		_brush_asset = pBrushData;
		((Object)((Component)this).gameObject).name = _brush_asset.id;
		_brush_asset.setupImage(icon);
		((Component)this).GetComponent<TipButton>().textOnClick = _brush_asset.getLocaleID();
	}
}
