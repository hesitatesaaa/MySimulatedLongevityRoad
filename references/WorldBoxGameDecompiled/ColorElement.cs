using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorElement : MonoBehaviour
{
	public Button button;

	public Image selection;

	public Image outer;

	public Image inner;

	public int index;

	public MetaCustomizationAsset asset;

	public void setColor(Color pOuter, Color pInner)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)outer).color = pOuter;
		((Graphic)inner).color = pInner;
	}

	public void setSelected(bool pSelected)
	{
		((Behaviour)selection).enabled = pSelected;
	}

	public void setAction(UnityAction pAction)
	{
		((UnityEvent)button.onClick).AddListener(pAction);
	}

	public void showTooltip()
	{
		CustomDataContainer<int> customDataContainer = new CustomDataContainer<int>();
		customDataContainer["color_count"] = asset.color_count();
		customDataContainer["color_current"] = index + 1;
		TooltipData pData = new TooltipData
		{
			tip_name = asset.color_locale,
			custom_data_int = customDataContainer
		};
		Tooltip.show(((Component)this).gameObject, "color_counter", pData);
	}
}
