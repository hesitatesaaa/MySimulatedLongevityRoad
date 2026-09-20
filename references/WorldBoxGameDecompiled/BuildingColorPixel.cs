using UnityEngine;

public readonly struct BuildingColorPixel
{
	public readonly Color32 color;

	public readonly Color32 color_abandoned;

	public readonly Color32 color_ruin;

	public BuildingColorPixel(Color32 pColor, Color32 pColorAbandoned, Color32 pColorRuin)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		color = pColor;
		color_abandoned = pColorAbandoned;
		color_ruin = pColorRuin;
	}
}
