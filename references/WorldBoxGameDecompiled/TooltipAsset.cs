using System;

[Serializable]
public class TooltipAsset : Asset
{
	public string prefab_id = "tooltips/tooltip_normal";

	public string sound;

	public string color;

	public TooltipShowAction callback;

	public TooltipShowAction callback_text_animated;
}
