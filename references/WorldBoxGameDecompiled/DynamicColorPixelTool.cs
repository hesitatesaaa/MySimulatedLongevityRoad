using System;
using UnityEngine;

public static class DynamicColorPixelTool
{
	private static bool _draw_phenotype;

	private static Color32 _phenotype_color;

	public static Color32 phenotype_shade_0;

	public static Color32 phenotype_shade_1;

	public static Color32 phenotype_shade_2;

	public static Color32 phenotype_shade_3;

	private static readonly Color32 _zombie_blood_color;

	public static Color32 checkSpecialColors(Color32 pColor, ColorAsset pKingdomColor, bool pCheckForLightColors = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		if (Config.EVERYTHING_MAGIC_COLOR)
		{
			return Toolbox.EVERYTHING_MAGIC_COLOR32;
		}
		if (pCheckForLightColors && Toolbox.areColorsEqual(pColor, Toolbox.color_light))
		{
			pColor = Toolbox.color_light_replace;
			return pColor;
		}
		if (pKingdomColor != null)
		{
			if (Toolbox.areColorsEqual(pColor, Toolbox.color_magenta_0))
			{
				pColor = pKingdomColor.k_color_0;
			}
			else if (Toolbox.areColorsEqual(pColor, Toolbox.color_magenta_1))
			{
				pColor = pKingdomColor.k_color_1;
			}
			else if (Toolbox.areColorsEqual(pColor, Toolbox.color_magenta_2))
			{
				pColor = pKingdomColor.k_color_2;
			}
			else if (Toolbox.areColorsEqual(pColor, Toolbox.color_magenta_3))
			{
				pColor = pKingdomColor.k_color_3;
			}
			else if (Toolbox.areColorsEqual(pColor, Toolbox.color_magenta_4))
			{
				pColor = pKingdomColor.k_color_4;
			}
			else if (Toolbox.areColorsEqual(pColor, Toolbox.color_teal_0))
			{
				pColor = pKingdomColor.k2_color_0;
			}
			else if (Toolbox.areColorsEqual(pColor, Toolbox.color_teal_1))
			{
				pColor = pKingdomColor.k2_color_1;
			}
			else if (Toolbox.areColorsEqual(pColor, Toolbox.color_teal_2))
			{
				pColor = pKingdomColor.k2_color_2;
			}
			else if (Toolbox.areColorsEqual(pColor, Toolbox.color_teal_3))
			{
				pColor = pKingdomColor.k2_color_3;
			}
			else if (Toolbox.areColorsEqual(pColor, Toolbox.color_teal_4))
			{
				pColor = pKingdomColor.k2_color_4;
			}
		}
		if (_draw_phenotype)
		{
			if (Toolbox.areColorsEqual(pColor, Toolbox.color_phenotype_green_0))
			{
				pColor = phenotype_shade_0;
			}
			else if (Toolbox.areColorsEqual(pColor, Toolbox.color_phenotype_green_1))
			{
				pColor = phenotype_shade_1;
			}
			else if (Toolbox.areColorsEqual(pColor, Toolbox.color_phenotype_green_2))
			{
				pColor = phenotype_shade_2;
			}
			else if (Toolbox.areColorsEqual(pColor, Toolbox.color_phenotype_green_3))
			{
				pColor = phenotype_shade_3;
			}
		}
		return pColor;
	}

	public static Color32 checkZombieColors(ActorAsset pAsset, Color32 pColor, int pID, bool pHead = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Color32 pTargetBlendColor = Color32.op_Implicit(Toolbox.makeColor(pAsset.zombie_color_hex));
		return addNoiseAndBlood(multiplyBlend(pColor, pTargetBlendColor), pID);
	}

	private static Color32 addNoiseAndBlood(Color32 pTargetColor, int pID)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Random random = new Random(pID);
		if (random.NextDouble() < 0.5)
		{
			return multiplyBlend(pTargetColor, _zombie_blood_color, 0.2f);
		}
		int num = random.Next(0, 20);
		int num2 = Mathf.Clamp(pTargetColor.r + num, 0, 255);
		int num3 = Mathf.Clamp(pTargetColor.g + num, 0, 255);
		int num4 = Mathf.Clamp(pTargetColor.b + num, 0, 255);
		return new Color32((byte)num2, (byte)num3, (byte)num4, pTargetColor.a);
	}

	private static Color32 multiplyBlend(Color32 pBaseColor, Color32 pTargetBlendColor, float pIntensity = 1f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)(int)pBaseColor.r / 255f;
		float num2 = (float)(int)pBaseColor.g / 255f;
		float num3 = (float)(int)pBaseColor.b / 255f;
		float num4 = Mathf.Lerp(1f, (float)(int)pTargetBlendColor.r / 255f, pIntensity);
		float num5 = Mathf.Lerp(1f, (float)(int)pTargetBlendColor.g / 255f, pIntensity);
		float num6 = Mathf.Lerp(1f, (float)(int)pTargetBlendColor.b / 255f, pIntensity);
		float num7 = Mathf.Clamp01(num * num4);
		float num8 = Mathf.Clamp01(num2 * num5);
		float num9 = Mathf.Clamp01(num3 * num6);
		return new Color32((byte)(num7 * 255f), (byte)(num8 * 255f), (byte)(num9 * 255f), pBaseColor.a);
	}

	private static Color32 overlayBlend(Color32 pBaseColor, Color32 pTargetBlendColor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)(int)pBaseColor.r / 255f;
		float num2 = (float)(int)pBaseColor.g / 255f;
		float num3 = (float)(int)pBaseColor.b / 255f;
		float num4 = (float)(int)pTargetBlendColor.r / 255f;
		float num5 = (float)(int)pTargetBlendColor.g / 255f;
		float num6 = (float)(int)pTargetBlendColor.b / 255f;
		float num7 = ((num < 0.5f) ? (2f * num * num4) : (1f - 2f * (1f - num) * (1f - num4)));
		float num8 = ((num2 < 0.5f) ? (2f * num2 * num5) : (1f - 2f * (1f - num2) * (1f - num5)));
		float num9 = ((num3 < 0.5f) ? (2f * num3 * num6) : (1f - 2f * (1f - num3) * (1f - num6)));
		return new Color32((byte)(num7 * 255f), (byte)(num8 * 255f), (byte)(num9 * 255f), pBaseColor.a);
	}

	public static void loadPhenotype(int pPhenotypeIndex, int pPhenotypeShadeIndex)
	{
		loadPhenotype(AssetManager.phenotype_library.getAssetByPhenotypeIndex(pPhenotypeIndex), pPhenotypeShadeIndex);
	}

	public static void loadPhenotype(PhenotypeAsset pPhenotypeAsset, int pPhenotypeShadeIndex)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		_phenotype_color = pPhenotypeAsset.colors[pPhenotypeShadeIndex];
		_draw_phenotype = true;
		phenotype_shade_0 = Color32.op_Implicit(Toolbox.makeDarkerColor(Color32.op_Implicit(_phenotype_color), 1f));
		phenotype_shade_1 = Color32.op_Implicit(Toolbox.makeDarkerColor(Color32.op_Implicit(_phenotype_color), 0.9f));
		phenotype_shade_2 = Color32.op_Implicit(Toolbox.makeDarkerColor(Color32.op_Implicit(_phenotype_color), 0.8f));
		phenotype_shade_3 = Color32.op_Implicit(Toolbox.makeDarkerColor(Color32.op_Implicit(_phenotype_color), 0.7f));
	}

	public static void loadSkinColorsPreview(PhenotypeAsset pPhenotype, int pSkinColor)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		_draw_phenotype = true;
		phenotype_shade_0 = pPhenotype.colors[0];
		phenotype_shade_1 = pPhenotype.colors[1];
		phenotype_shade_2 = pPhenotype.colors[2];
		phenotype_shade_3 = pPhenotype.colors[3];
	}

	public static void resetSkinColors()
	{
		_draw_phenotype = false;
	}

	public static void setPlaceholderSkinColor(Color32 pColor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		_phenotype_color = pColor;
	}

	static DynamicColorPixelTool()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		_zombie_blood_color = Color32.op_Implicit(Toolbox.makeColor("#CE566E"));
	}
}
