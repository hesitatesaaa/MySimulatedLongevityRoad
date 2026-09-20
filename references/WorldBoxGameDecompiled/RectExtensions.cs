using UnityEngine;

public static class RectExtensions
{
	public static Rect Resize(this Rect pRect, float pMultiplier)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Rect)(ref pRect)).width * pMultiplier;
		float num2 = ((Rect)(ref pRect)).height * pMultiplier;
		float num3 = (((Rect)(ref pRect)).width - num) / 2f;
		float num4 = (((Rect)(ref pRect)).height - num2) / 2f;
		((Rect)(ref pRect)).width = num;
		((Rect)(ref pRect)).height = num2;
		((Rect)(ref pRect)).x = ((Rect)(ref pRect)).x + num3;
		((Rect)(ref pRect)).y = ((Rect)(ref pRect)).y + num4;
		return pRect;
	}
}
