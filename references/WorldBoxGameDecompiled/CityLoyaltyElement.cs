using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CityLoyaltyElement : MonoBehaviour
{
	private City _city;

	private TooltipData _tooltip_data;

	public void setCity(City pCity)
	{
		_city = pCity;
	}

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		Button component = ((Component)this).GetComponent<Button>();
		((UnityEvent)component.onClick).AddListener(new UnityAction(showTooltip));
		component.OnHover(new UnityAction(showHoverTooltip));
		component.OnHoverOut(new UnityAction(Tooltip.hideTooltip));
	}

	private void showHoverTooltip()
	{
		if (Config.tooltips_active)
		{
			showTooltip();
		}
	}

	private void showTooltip()
	{
		_tooltip_data = new TooltipData
		{
			city = _city
		};
		Tooltip.show(((Component)this).gameObject, "loyalty", _tooltip_data);
	}
}
