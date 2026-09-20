using UnityEngine;

public class CenterTipCaller : MonoBehaviour
{
	public string tip_title;

	public string tip_id;

	public void Show()
	{
		Tooltip.show(this, "normal", new TooltipData
		{
			tip_name = tip_title,
			tip_description = tip_id
		});
	}
}
