using System.Collections.Generic;
using UnityEngine;

public class ColorArray
{
	public List<Color32> colors;

	public ColorArray(float pR, float pG, float pB, float pA, float pAmount, float pMod = 1f)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		colors = new List<Color32>();
		Color val = default(Color);
		for (int i = 0; (float)i < pAmount; i++)
		{
			float num = ((i <= 0) ? 0f : (1f / pAmount * (float)i));
			((Color)(ref val))._002Ector(pR, pG, pB, num * 1f * pMod);
			colors.Add(Color32.op_Implicit(val));
		}
	}

	public ColorArray(Color32 pColor, int pAmount)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		this._002Ector((int)pColor.r, (int)pColor.g, (int)pColor.b, (int)pColor.a, pAmount);
	}
}
