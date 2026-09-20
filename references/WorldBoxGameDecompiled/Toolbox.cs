using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityPools;

public static class Toolbox
{
	public static readonly ActorDirection[] directions;

	public static readonly ActorDirection[] directions_all;

	public static readonly Dictionary<ActorDirection, ActorDirection[]> directions_turns;

	public static readonly Dictionary<ActorDirection, ActorDirection[]> directions_all_turns;

	public static readonly Color32 EVERYTHING_MAGIC_COLOR32;

	public static readonly Color32 color_grey_dark;

	public static readonly Color32 color_grey;

	public static readonly Color32 color_transparent_grey;

	public static readonly Color32 color_debug_bar_blue;

	public static readonly Color32 color_debug_bar_red;

	public static readonly Color32 color_phenotype_green_0;

	public static readonly Color32 color_phenotype_green_1;

	public static readonly Color32 color_phenotype_green_2;

	public static readonly Color32 color_phenotype_green_3;

	public static readonly Color32 color_map_icon_green;

	public static readonly Color32 color_magenta_0;

	public static readonly Color32 color_magenta_1;

	public static readonly Color32 color_magenta_2;

	public static readonly Color32 color_magenta_3;

	public static readonly Color32 color_magenta_4;

	public static readonly Color32 color_teal_0;

	public static readonly Color32 color_teal_1;

	public static readonly Color32 color_teal_2;

	public static readonly Color32 color_teal_3;

	public static readonly Color32 color_teal_4;

	public static readonly Color32 color_ocean;

	public static readonly Color32 color_night;

	public static readonly Color32 color_light;

	public static readonly Color32 color_light_100;

	public static readonly Color32 color_light_10;

	public static readonly Color32 color_light_replace;

	public static Color color_augmentation_selected;

	public static Color color_augmentation_unselected;

	public static readonly Color32 color_clear;

	public static Color color_white;

	public static Color color_gray;

	public static Color color_black;

	public static Color32 color_black_32;

	public static readonly Color32 color_white_32;

	public static Color color_red;

	public static Color color_yellow;

	public static Color color_blue;

	public static Color color_green;

	public static Color color_purple;

	public static Color color_cyan;

	public static Color color_cursed;

	public static Color color_abandoned_building;

	public const string color_positive = "#43FF43";

	public const string color_negative = "#FB2C21";

	public const string color_positive_light = "#95DD5D";

	public const string color_negative_light = "#FF8686";

	public static readonly Color color_positive_RGBA;

	public static readonly Color color_negative_RGBA;

	public const string color_report_positive = "#ADADAD";

	public const string color_report_negative = "#919191";

	public const string color_hex_white = "#FFFFFF";

	public const string color_hex_black = "#000000";

	public const string color_hex_neutral = "#F3961F";

	public const string color_hex_brighter = "#FFBC66";

	public const string color_tooltip_hotkey = "#95DD5D";

	public static readonly Color32 clear;

	public static readonly Color32 edge_alpha;

	public static readonly Color color_white_transparent;

	public static readonly Color color_text_default;

	public static readonly Color color_text_default_bright;

	public static readonly Color color_log_good;

	public static readonly Color color_log_warning;

	public static readonly Color color_log_neutral;

	public static readonly Color32 color_fire;

	public const string color_hex_ocean = "#3370CC";

	public const string color_hex_blue = "#4CCFFF";

	public const string color_hex_red = "#FF637D";

	public const string color_hex_green = "#43FF43";

	public const string color_hex_purple = "#E060CD";

	public const string color_hex_yellow = "#FFFF51";

	public const string color_hex_heal = "#23F3FF";

	public const string color_hex_plague = "#CE4A9B";

	public const string color_hex_mush_spores = "#8CFF99";

	public const string color_hex_infected = "#35CC6E";

	public const string color_hex_poisoned = "#D85BC5";

	public static Color color_heal;

	public static Color color_plague;

	public static Color color_mushSpores;

	public static Color color_infected;

	public static Color color_poisoned;

	public static readonly Color[] colors_fire;

	public static readonly Color[] colors_wheat;

	internal static readonly List<WorldTile> temp_list_tiles;

	private static readonly MapChunk[] _temp_array_chunks;

	private static readonly TileZone[] _temp_array_zones;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void fromStringListToHashset(List<string> pList, HashSet<string> pHashset)
	{
		pHashset.UnionWith(pList);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string coloredText(string pText, string pColor, bool pLocalize = false)
	{
		if (pLocalize)
		{
			pText = LocalizedTextManager.getText(pText);
		}
		return "<color=" + pColor + ">" + pText + "</color>";
	}

	public static string coloredGreyPart(object pPart, string pMainColor, bool pUnit = false)
	{
		string empty = string.Empty;
		if (pUnit)
		{
			empty += coloredString(" (", ColorStyleLibrary.m.color_dead_text);
			empty += coloredString(pPart.ToString(), pMainColor);
			return empty + coloredString(")", ColorStyleLibrary.m.color_dead_text);
		}
		empty += coloredString(" [", ColorStyleLibrary.m.color_dead_text);
		empty += coloredString(pPart.ToString(), pMainColor);
		return empty + coloredString("]", ColorStyleLibrary.m.color_dead_text);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool areColorsEqual(Color32 pC1, Color32 pC2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (pC1.r == pC2.r && pC1.g == pC2.g)
		{
			return pC1.b == pC2.b;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool inBounds(float pVal, float pMin, float pMax)
	{
		if (pVal > pMin)
		{
			return pVal < pMax;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string firstLetterToUpper(string str)
	{
		if (str == null)
		{
			return null;
		}
		if (str.Length == 0)
		{
			return str;
		}
		if (str.Length > 1)
		{
			Span<char> span = stackalloc char[str.Length];
			span[0] = char.ToUpper(str[0]);
			str.AsSpan(1).CopyTo(span.Slice(1));
			return new string(span);
		}
		return str.ToUpper();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int loopIndex(int pIndex, int pLength)
	{
		if (pLength < 1)
		{
			return 0;
		}
		return (pIndex % pLength + pLength) % pLength;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 RotatePointAroundPivot(ref Vector3 point, ref Vector3 pivot, ref Vector3 angles)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Euler(angles) * (point - pivot) + pivot;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 RotatePointAroundPivot2(ref Vector3 point, ref Vector3 pivot, ref Vector3 angles)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = point - pivot;
		val = Quaternion.Euler(angles) * val;
		point = val + pivot;
		return point;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 rotateVector(Vector2 pVector, float degrees)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		float num = degrees * (MathF.PI / 180f);
		float num2 = Mathf.Sin(num);
		float num3 = Mathf.Cos(num);
		float x = pVector.x;
		float y = pVector.y;
		return new Vector2(num3 * x - num2 * y, num2 * x + num3 * y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 cubeBezier3(ref Vector3 p0, ref Vector3 p1, ref Vector3 p2, ref Vector3 p3, float t)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f - t;
		float num2 = num * num * num;
		float num3 = num * num * t * 3f;
		float num4 = num * t * t * 3f;
		float num5 = t * t * t;
		return new Vector3(num2 * p0.x + num3 * p1.x + num4 * p2.x + num5 * p3.x, num2 * p0.y + num3 * p1.y + num4 * p2.y + num5 * p3.y, num2 * p0.z + num3 * p1.z + num4 * p2.z + num5 * p3.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 cubeBezier2(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f - t;
		float num2 = num * num * num;
		float num3 = num * num * t * 3f;
		float num4 = num * t * t * 3f;
		float num5 = t * t * t;
		return new Vector2(num2 * p0.x + num3 * p1.x + num4 * p2.x + num5 * p3.x, num2 * p0.y + num3 * p1.y + num4 * p2.y + num5 * p3.y);
	}

	public unsafe static Vector2 cubeBezierN(float pTick, Span<Vector3> pPoints)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (pPoints.Length > 2)
		{
			int num = pPoints.Length - 1;
			Span<Vector3> span;
			if (num < 128)
			{
				int num2 = num;
				span = new Span<Vector3>(stackalloc Vector3[num2], num2);
			}
			else
			{
				span = (Vector3[]?)(object)new Vector3[num];
			}
			Span<Vector3> pPoints2 = span;
			for (int i = 0; i < num; i++)
			{
				pPoints2[i] = Vector2.op_Implicit(Vector2.Lerp(Vector2.op_Implicit(pPoints[i]), Vector2.op_Implicit(pPoints[i + 1]), pTick));
			}
			return cubeBezierN(pTick, pPoints2);
		}
		if (pPoints.Length == 2)
		{
			return Vector2.Lerp(Vector2.op_Implicit(pPoints[0]), Vector2.op_Implicit(pPoints[1]), pTick);
		}
		return Vector2.op_Implicit(pPoints[0]);
	}

	public static string encode(string pString)
	{
		string text = "WorldboxIsAwesome";
		pString = Encryption.EncryptString(pString, text + "555");
		return pString;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float easeInOutQuart(float x)
	{
		if (x < 0.5f)
		{
			return 8f * x * x * x * x;
		}
		return 1f - (float)Math.Pow(-2f * x + 2f, 4.0) / 2f;
	}

	public static string decode(string pString)
	{
		string text = "WorldboxIsAwesome";
		pString = Encryption.DecryptString(pString, text + "555");
		return pString;
	}

	public static string decodeMobile(string pString)
	{
		string deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
		string text = "WorldboxIsAwesome";
		pString = Encryption.DecryptString(pString, text + "555" + deviceUniqueIdentifier);
		return pString;
	}

	public static string generateID_old()
	{
		return shortGUID(Guid.NewGuid());
	}

	public static string shortGUID(Guid guid)
	{
		return Convert.ToBase64String(guid.ToByteArray()).Replace('+', '-').Replace('/', '_')
			.Substring(0, 8);
	}

	public static Vector3 getNewPoint(float pX1, float pY1, float pX2, float pY2, float pDist, bool pConvertNegative = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = default(Vector3);
		float num = Dist(pX1, pY1, pX2, pY2) - pDist;
		float num2;
		if (num == 0f)
		{
			num2 = 1f;
			((Vector3)(ref result)).Set(pX2, pY2, 0f);
			return result;
		}
		num2 = pDist / num;
		if (pConvertNegative && num2 < 0f)
		{
			num2 = 0f - num2;
		}
		float x = (pX1 + num2 * pX2) / (1f + num2);
		float y = (pY1 + num2 * pY2) / (1f + num2);
		result.x = x;
		result.y = y;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 getNewPointVec2(Vector2 pVec1, Vector2 pVec2, float pDist, bool pConvertNegative = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return getNewPointVec2(pVec1.x, pVec1.y, pVec2.x, pVec2.y, pDist, pConvertNegative);
	}

	public static Vector2 getNewPointVec2(float pX1, float pY1, float pX2, float pY2, float pDist, bool pConvertNegative = true)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		float num = Dist(pX1, pY1, pX2, pY2) - pDist;
		if (num == 0f)
		{
			return new Vector2(pX2, pY2);
		}
		float num2 = pDist / num;
		if (pConvertNegative && num2 < 0f)
		{
			num2 = 0f - num2;
		}
		float num3 = 1f / (1f + num2);
		float num4 = (pX1 + num2 * pX2) * num3;
		float num5 = (pY1 + num2 * pY2) * num3;
		return new Vector2(num4, num5);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistVec3(Vector3 pT1, Vector3 pT2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Sqrt((pT1.x - pT2.x) * (pT1.x - pT2.x) + (pT1.y - pT2.y) * (pT1.y - pT2.y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistVec2(Vector2Int pT1, Vector2Int pT2)
	{
		return Mathf.Sqrt((float)((((Vector2Int)(ref pT1)).x - ((Vector2Int)(ref pT2)).x) * (((Vector2Int)(ref pT1)).x - ((Vector2Int)(ref pT2)).x) + (((Vector2Int)(ref pT1)).y - ((Vector2Int)(ref pT2)).y) * (((Vector2Int)(ref pT1)).y - ((Vector2Int)(ref pT2)).y)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistVec2Float(Vector2 pT1, Vector2 pT2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Sqrt((pT1.x - pT2.x) * (pT1.x - pT2.x) + (pT1.y - pT2.y) * (pT1.y - pT2.y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistTile(WorldTile pT1, WorldTile pT2)
	{
		return Mathf.Sqrt((float)((pT1.x - pT2.x) * (pT1.x - pT2.x) + (pT1.y - pT2.y) * (pT1.y - pT2.y)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Dist(float x1, float y1, float x2, float y2)
	{
		return Mathf.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Dist(int x1, int y1, int x2, int y2)
	{
		return Mathf.Sqrt((float)((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SquaredDist(float x1, float y1, float x2, float y2)
	{
		return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int SquaredDist(int x1, int y1, int x2, int y2)
	{
		return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int SquaredDistTile(WorldTile pT1, WorldTile pT2)
	{
		return (pT1.x - pT2.x) * (pT1.x - pT2.x) + (pT1.y - pT2.y) * (pT1.y - pT2.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SquaredDistVec2Float(Vector2 pT1, Vector2 pT2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return (pT1.x - pT2.x) * (pT1.x - pT2.x) + (pT1.y - pT2.y) * (pT1.y - pT2.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int SquaredDistVec2(Vector2Int pT1, Vector2Int pT2)
	{
		return (((Vector2Int)(ref pT1)).x - ((Vector2Int)(ref pT2)).x) * (((Vector2Int)(ref pT1)).x - ((Vector2Int)(ref pT2)).x) + (((Vector2Int)(ref pT1)).y - ((Vector2Int)(ref pT2)).y) * (((Vector2Int)(ref pT1)).y - ((Vector2Int)(ref pT2)).y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SquaredDistVec3(Vector3 pT1, Vector3 pT2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return (pT1.x - pT2.x) * (pT1.x - pT2.x) + (pT1.y - pT2.y) * (pT1.y - pT2.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Color makeColor(string pHex)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Color result = default(Color);
		ColorUtility.TryParseHtmlString(pHex, ref result);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Color makeColor(string pHex, float pAlpha)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Color result = default(Color);
		ColorUtility.TryParseHtmlString(pHex, ref result);
		result.a = pAlpha;
		return result;
	}

	public static string colorToHex(Color32 pColor, bool pAlpha = true)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (pAlpha)
		{
			Span<char> span = stackalloc char[9];
			span[0] = '#';
			ColorUtility.ToHtmlStringRGBA(Color32.op_Implicit(pColor)).AsSpan().CopyTo(span.Slice(1));
			return new string(span);
		}
		Span<char> span2 = stackalloc char[7];
		span2[0] = '#';
		ColorUtility.ToHtmlStringRGB(Color32.op_Implicit(pColor)).AsSpan().CopyTo(span2.Slice(1));
		return new string(span2);
	}

	public static string coloredString(string pText, string pColor)
	{
		if (string.IsNullOrEmpty(pColor))
		{
			return pText;
		}
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		stringBuilderPool.Append("<color=").Append(pColor).Append(">")
			.Append(pText)
			.Append("</color>");
		return stringBuilderPool.ToString();
	}

	public static string colorBetween(double pValue, double pMin, double pMax, string pMinColor = "#FB2C21", string pMaxColor = "#43FF43")
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		float num = 100f;
		if (pMax - pMin != 0.0)
		{
			num = (float)(pValue - pMin) / (float)(pMax - pMin);
		}
		return colorToHex(Color32.op_Implicit(Color.Lerp(makeColor(pMinColor), makeColor(pMaxColor), num)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float getAngle(float pX1, float pY1, float pX2, float pY2)
	{
		float num = pX2 - pX1;
		return (float)Math.Atan2(pY2 - pY1, num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Quaternion getEulerAngle(float pX1, float pY1, float pX2, float pY2)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float angleDegrees = getAngleDegrees(pX1, pY1, pX2, pY2);
		return Quaternion.Euler(new Vector3(0f, 0f, angleDegrees));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Quaternion getEulerAngle(Vector2 pVec1, Vector2 pVec2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		float angleDegrees = getAngleDegrees(pVec1, pVec2);
		return Quaternion.Euler(new Vector3(0f, 0f, angleDegrees));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float getAngleDegrees(Vector2 pVec1, Vector2 pVec2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return getAngle(pVec1.x, pVec1.y, pVec2.x, pVec2.y) * 57.29578f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float getAngleDegrees(float pX1, float pY1, float pX2, float pY2)
	{
		return getAngle(pX1, pY1, pX2, pY2) * 57.29578f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Color makeDarkerColor(Color pColor, float pMod = 0.4f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		return new Color(pColor.r * pMod, pColor.g * pMod, pColor.b * pMod, pColor.a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string makeDarkerColor(string pHexColor, float pMod = 0.4f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return colorToHex(Color32.op_Implicit(makeDarkerColor(makeColor(pHexColor), pMod)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Color blendColor(Color pFrom, Color pTo, float amount)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		float num = pFrom.r * amount + pTo.r * (1f - amount);
		float num2 = pFrom.g * amount + pTo.g * (1f - amount);
		float num3 = pFrom.b * amount + pTo.b * (1f - amount);
		return new Color(num, num2, num3);
	}

	public static Vector2Int getClosestTile(Span<Vector2Int> pArray, WorldTile pTarget)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int result = default(Vector2Int);
		Span<Vector2Int> span = pArray;
		int length = span.Length;
		int num = int.MaxValue;
		for (int i = 0; i < length; i++)
		{
			Vector2Int val = span[i];
			int num2 = SquaredDist(pTarget.x, pTarget.y, ((Vector2Int)(ref val)).x, ((Vector2Int)(ref val)).y);
			if (num2 < num)
			{
				num = num2;
				result = val;
			}
		}
		return result;
	}

	public static WorldTile getClosestTile(WorldTile[] pArray, WorldTile pTarget)
	{
		WorldTile result = null;
		int num = pArray.Length;
		int num2 = int.MaxValue;
		for (int i = 0; i < num; i++)
		{
			WorldTile worldTile = pArray[i];
			int num3 = SquaredDist(pTarget.x, pTarget.y, worldTile.x, worldTile.y);
			if (num3 < num2)
			{
				num2 = num3;
				result = worldTile;
			}
		}
		return result;
	}

	public static WorldTile getClosestTile(List<WorldTile> pArray, WorldTile pTarget)
	{
		WorldTile result = null;
		int count = pArray.Count;
		int num = int.MaxValue;
		for (int i = 0; i < count; i++)
		{
			WorldTile worldTile = pArray[i];
			int num2 = SquaredDist(pTarget.x, pTarget.y, worldTile.x, worldTile.y);
			if (num2 < num)
			{
				num = num2;
				result = worldTile;
			}
		}
		return result;
	}

	public static WorldTile getClosestTile(ListPool<WorldTile> pArray, WorldTile pTarget)
	{
		WorldTile result = null;
		int count = pArray.Count;
		int num = int.MaxValue;
		for (int i = 0; i < count; i++)
		{
			WorldTile worldTile = pArray[i];
			int num2 = SquaredDist(pTarget.x, pTarget.y, worldTile.x, worldTile.y);
			if (num2 < num)
			{
				num = num2;
				result = worldTile;
			}
		}
		return result;
	}

	public static void sortRegionsByDistance(WorldTile pTile, List<MapRegion> pRegions)
	{
		pRegions.Sort((MapRegion x, MapRegion y) => SquaredDistTile(pTile, x.tiles[0]).CompareTo(SquaredDistTile(pTile, y.tiles[0])));
	}

	public static void sortTilesByDistance(WorldTile pTile, ListPool<WorldTile> pTiles)
	{
		pTiles.Sort((WorldTile x, WorldTile y) => SquaredDistTile(pTile, x).CompareTo(SquaredDistTile(pTile, y)));
	}

	public static float maxTileDistance(WorldTile pTile, ListPool<WorldTile> pTiles)
	{
		float num = 0f;
		for (int i = 0; i < pTiles.Count; i++)
		{
			WorldTile pT = pTiles[i];
			float num2 = DistTile(pTile, pT);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	public static MapRegion getClosestRegion(List<MapRegion> pArray, WorldTile pTarget)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		MapRegion result = null;
		int num = int.MaxValue;
		for (int i = 0; i < pArray.Count; i++)
		{
			MapRegion mapRegion = pArray[i];
			Vector2Int pos = pTarget.pos;
			int x = ((Vector2Int)(ref pos)).x;
			pos = pTarget.pos;
			int y = ((Vector2Int)(ref pos)).y;
			pos = mapRegion.tiles[0].pos;
			int x2 = ((Vector2Int)(ref pos)).x;
			pos = mapRegion.tiles[0].pos;
			int num2 = SquaredDist(x, y, x2, ((Vector2Int)(ref pos)).y);
			if (num2 < num)
			{
				num = num2;
				result = mapRegion;
			}
		}
		return result;
	}

	public static Vector2Int getRandomVectorWithinDistance(int pX, int pY, int pRange)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Vector2 pPos = default(Vector2);
		((Vector2)(ref pPos))._002Ector((float)(pX - pRange), (float)(pY - pRange));
		Vector2 pPos2 = default(Vector2);
		((Vector2)(ref pPos2))._002Ector((float)(pX + pRange), (float)(pY + pRange));
		clampToMap(ref pPos);
		clampToMap(ref pPos2);
		Vector2 val = new Vector2
		{
			x = Randy.randomFloat(pPos.x, pPos2.x),
			y = Randy.randomFloat(pPos.y, pPos2.y)
		};
		return new Vector2Int((int)val.x, (int)val.y);
	}

	public static WorldTile getRandomTileWithinDistance(WorldTile pWorldTile, int pRange)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int pos = pWorldTile.pos;
		int x = ((Vector2Int)(ref pos)).x;
		pos = pWorldTile.pos;
		Vector2Int randomVectorWithinDistance = getRandomVectorWithinDistance(x, ((Vector2Int)(ref pos)).y, pRange);
		return World.world.GetTileSimple(((Vector2Int)(ref randomVectorWithinDistance)).x, ((Vector2Int)(ref randomVectorWithinDistance)).y);
	}

	public static WorldTile getRandomTileWithinDistance(WorldTile pWorldTile, int pRange, ListPool<WorldTile> pTiles)
	{
		foreach (WorldTile item in pTiles.LoopRandom())
		{
			if (!(DistTile(pWorldTile, item) > (float)pRange))
			{
				return item;
			}
		}
		return getRandomTileWithinDistance(pWorldTile, pRange);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Actor getClosestActor(HashSet<Actor> pCollection, WorldTile pTile)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Actor result = null;
		int num = int.MaxValue;
		Vector2Int pos = pTile.pos;
		foreach (Actor item in pCollection)
		{
			if (!item.isRekt())
			{
				Vector2Int pos2 = item.current_tile.pos;
				int num2 = SquaredDist(((Vector2Int)(ref pos)).x, ((Vector2Int)(ref pos)).y, ((Vector2Int)(ref pos2)).x, ((Vector2Int)(ref pos2)).y);
				if (num2 < num)
				{
					num = num2;
					result = item;
				}
			}
		}
		return result;
	}

	public static Actor getClosestActor(List<Actor> pCollection, WorldTile pTile)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Actor result = null;
		int num = int.MaxValue;
		Vector2Int pos = pTile.pos;
		int count = pCollection.Count;
		for (int i = 0; i < count; i++)
		{
			Actor actor = pCollection[i];
			Vector2Int pos2 = actor.current_tile.pos;
			int num2 = SquaredDist(((Vector2Int)(ref pos)).x, ((Vector2Int)(ref pos)).y, ((Vector2Int)(ref pos2)).x, ((Vector2Int)(ref pos2)).y);
			if (num2 < num)
			{
				num = num2;
				result = actor;
			}
		}
		return result;
	}

	public static Actor getClosestActor(ListPool<Actor> pCollection, WorldTile pTile)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Actor result = null;
		int num = int.MaxValue;
		Vector2Int pos = pTile.pos;
		int count = pCollection.Count;
		for (int i = 0; i < count; i++)
		{
			Actor actor = pCollection[i];
			Vector2Int pos2 = actor.current_tile.pos;
			int num2 = SquaredDist(((Vector2Int)(ref pos)).x, ((Vector2Int)(ref pos)).y, ((Vector2Int)(ref pos2)).x, ((Vector2Int)(ref pos2)).y);
			if (num2 < num)
			{
				num = num2;
				result = actor;
			}
		}
		return result;
	}

	public static Building getClosestBuilding(List<Building> pCollection, WorldTile pTile)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Building result = null;
		int num = int.MaxValue;
		Vector2Int pos = pTile.pos;
		int count = pCollection.Count;
		for (int i = 0; i < count; i++)
		{
			Building building = pCollection[i];
			Vector2Int pos2 = building.current_tile.pos;
			int num2 = SquaredDist(((Vector2Int)(ref pos)).x, ((Vector2Int)(ref pos)).y, ((Vector2Int)(ref pos2)).x, ((Vector2Int)(ref pos2)).y);
			if (num2 < num)
			{
				num = num2;
				result = building;
			}
		}
		return result;
	}

	public static async Task<byte[]> ReadAllBytes(string filePath)
	{
		byte[] result;
		using (FileStream stream = File.Open(filePath, FileMode.Open))
		{
			result = new byte[stream.Length];
			await stream.ReadAsync(result, 0, (int)stream.Length);
		}
		return result;
	}

	public static Sprite LoadSprite(string path)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		if (File.Exists(path))
		{
			byte[] array = File.ReadAllBytes(path);
			Texture2D val = new Texture2D(1, 1);
			((Texture)val).anisoLevel = 0;
			ImageConversion.LoadImage(val, array);
			Sprite result = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f));
			array = null;
			return result;
		}
		return null;
	}

	public static Sprite LoadResizedSprite(string path, int width, int height)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Sprite val = LoadSprite(path);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		Sprite result = Sprite.Create(ScaleTexture(val.texture, width, height), new Rect(0f, 0f, (float)width, (float)height), new Vector2(0f, 0f));
		Object.DestroyImmediate((Object)(object)val.texture);
		Object.DestroyImmediate((Object)(object)val);
		return result;
	}

	public static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(targetWidth, targetHeight, source.format, true);
		Color[] pixels = val.GetPixels(0);
		float num = 1f / (float)((Texture)source).width * ((float)((Texture)source).width / (float)targetWidth);
		float num2 = 1f / (float)((Texture)source).height * ((float)((Texture)source).height / (float)targetHeight);
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = source.GetPixelBilinear(num * ((float)i % (float)targetWidth), num2 * Mathf.Floor((float)(i / targetWidth)));
		}
		val.SetPixels(pixels, 0);
		val.Apply();
		return val;
	}

	public static string formatTimer(float pTime)
	{
		int num = (int)(pTime / 60f);
		int num2 = (int)(pTime - (float)(num * 60));
		string text = "";
		text = ((num >= 10) ? (num + ":") : ("0" + num + ":"));
		if (num2 < 10)
		{
			return text + "0" + num2;
		}
		return text + num2;
	}

	public static string formatTime(float pTime)
	{
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		TimeSpan timeSpan = TimeSpan.FromSeconds(pTime);
		int num = timeSpan.Days / 7;
		int num2 = timeSpan.Days;
		int num3 = (int)timeSpan.TotalHours;
		if (num > 0)
		{
			stringBuilderPool.Append(num).Append("w ");
			num2 -= num * 7;
			num3 -= num * 7 * 24;
		}
		if (num2 > 1)
		{
			stringBuilderPool.Append(num2).Append("d ");
			num3 -= num2 * 24;
		}
		stringBuilderPool.Append(num3);
		if (timeSpan.Minutes < 10)
		{
			stringBuilderPool.Append(":0").Append(timeSpan.Minutes);
		}
		else
		{
			stringBuilderPool.Append(':').Append(timeSpan.Minutes);
		}
		if (timeSpan.Seconds < 10)
		{
			stringBuilderPool.Append(":0").Append(timeSpan.Seconds);
		}
		else
		{
			stringBuilderPool.Append(':').Append(timeSpan.Seconds);
		}
		return stringBuilderPool.ToString();
	}

	public static string formatNumber(long pNumber)
	{
		long num = Math.Abs(pNumber);
		if (num >= 10000000000L)
		{
			return ((double)pNumber / 1000000000.0).ToString("N0") + "b";
		}
		if (num >= 1000000000)
		{
			return ((double)pNumber / 1000000000.0).ToText() + "b";
		}
		if (num >= 10000000)
		{
			return ((double)pNumber / 1000000.0).ToString("N0") + "m";
		}
		if (num >= 1000000)
		{
			return ((double)pNumber / 1000000.0).ToText() + "m";
		}
		if (num >= 10000)
		{
			return ((float)pNumber / 1000f).ToString("N0") + "k";
		}
		if (num >= 1000)
		{
			return ((float)pNumber / 1000f).ToText() + "k";
		}
		return pNumber.ToText();
	}

	public static string formatNumber(long pNumber, int pMaxSize)
	{
		if (pNumber.ToText().Length <= pMaxSize)
		{
			return pNumber.ToText();
		}
		return formatNumber(pNumber);
	}

	internal static void clearAll()
	{
		temp_list_tiles.Clear();
		_temp_array_chunks.Clear();
		_temp_array_zones.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static MapChunk getRandomChunkFromTile(WorldTile pTile)
	{
		var (pArray, pLength) = getAllChunksFromTile(pTile);
		return pArray.GetRandom(pLength);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static WorldTile getRandomTileAround(WorldTile pTile)
	{
		return getRandomChunkFromTile(pTile).tiles.GetRandom();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static (MapChunk[], int) getAllChunksFromTile(WorldTile pTile)
	{
		return getAllChunksFromChunk(pTile.chunk);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static (MapChunk[], int) getAllChunksFromChunk(MapChunk pChunk)
	{
		MapChunk[] temp_array_chunks = _temp_array_chunks;
		temp_array_chunks[0] = pChunk;
		int num = pChunk.neighbours_all.Length;
		pChunk.neighbours_all.AsSpan().CopyTo(_temp_array_chunks.AsSpan(1));
		return (temp_array_chunks, num + 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static (TileZone[], int) getAllZonesFromTile(WorldTile pTile)
	{
		return getAllZonesFromZone(pTile.zone);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static (TileZone[], int) getAllZonesFromZone(TileZone pZone)
	{
		TileZone[] temp_array_zones = _temp_array_zones;
		temp_array_zones[0] = pZone;
		int num = pZone.neighbours_all.Length;
		pZone.neighbours_all.AsSpan().CopyTo(_temp_array_zones.AsSpan(1));
		return (temp_array_zones, num + 1);
	}

	internal static bool hasDifferentSpeciesInChunkAround(WorldTile pTile, string pSpecies)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1))
		{
			if (!item.a.isSameSpecies(pSpecies))
			{
				return true;
			}
		}
		return false;
	}

	internal static int countUnitsInChunk(WorldTile pTile)
	{
		int num = 0;
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 0))
		{
			_ = item;
			num++;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool inMapBorder(ref Vector2 pPoint)
	{
		if (pPoint.x < (float)MapBox.width && pPoint.y < (float)MapBox.height && pPoint.x >= 0f)
		{
			return pPoint.y >= 0f;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool inMapBorder(ref Vector3 pPoint)
	{
		if (pPoint.x < (float)MapBox.width && pPoint.y < (float)MapBox.height && pPoint.x >= 0f)
		{
			return pPoint.y >= 0f;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void clampToMap(ref Vector2 pPos)
	{
		pPos.x = Mathf.Clamp(pPos.x, 0f, (float)(MapBox.width - 1));
		pPos.y = Mathf.Clamp(pPos.y, 0f, (float)(MapBox.height - 1));
	}

	internal static IEnumerable<Building> getBuildingsTypeFromChunk(MapChunk pChunk, string pType, bool pOnlyNonTargeted, bool pOnlyWithResources)
	{
		foreach (Building item in Finder.getBuildingsFromChunk(pChunk.tiles[0], 0, 0, pRandom: true))
		{
			if ((!pOnlyWithResources || item.hasResourcesToCollect()) && item.isUsable() && (!pOnlyNonTargeted || !item.current_tile.isTargeted()) && item.asset.type == pType)
			{
				yield return item;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Quaternion LookAt2D(Vector2 forward)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Euler(0f, 0f, Mathf.Atan2(forward.y, forward.x) * 57.29578f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string LowerCaseFirst(string pString)
	{
		if (pString.Length == 0)
		{
			return "";
		}
		return char.ToLower(pString[0]) + ((pString.Length > 1) ? pString.Substring(1) : string.Empty);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] resizeArray<T>(T[] pArray, int aPos)
	{
		Array.Resize(ref pArray, aPos);
		return pArray;
	}

	public static string getRoundedTimestamp()
	{
		DateTime utcNow = DateTime.UtcNow;
		new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		string text = ((utcNow.Month < 10) ? ("0" + utcNow.Month) : utcNow.Month.ToString());
		string text2 = ((utcNow.Day < 10) ? ("0" + utcNow.Day) : utcNow.Day.ToString());
		return utcNow.Year + text + text2;
	}

	public static ListPool<string> getDirectories(string pPath)
	{
		ListPool<string> listPool = new ListPool<string>();
		string[] directories = Directory.GetDirectories(pPath);
		foreach (string text in directories)
		{
			if (!text.Contains(".meta"))
			{
				listPool.Add(text);
			}
		}
		return listPool;
	}

	public static ListPool<string> getFiles(string pPath)
	{
		ListPool<string> listPool = new ListPool<string>();
		string[] files = Directory.GetFiles(pPath);
		foreach (string text in files)
		{
			if (!text.Contains(".meta"))
			{
				listPool.Add(text);
			}
		}
		return listPool;
	}

	public static string cacheBuster()
	{
		return DateTime.UtcNow.RoundMinutes().ToFileTime() + "_" + Config.versionCodeText;
	}

	public static DateTime RoundMinutes(this DateTime value)
	{
		return value.RoundMinutes(30);
	}

	public static DateTime RoundMinutes(this DateTime value, int roundMinutes)
	{
		DateTime dateTime = new DateTime(value.Ticks);
		int minute = value.Minute;
		_ = value.Hour;
		int num = minute % roundMinutes;
		if (num <= roundMinutes / 2)
		{
			return dateTime.AddMinutes(-num);
		}
		return dateTime.AddMinutes(roundMinutes - num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static WorldTile getTileAt(float pX, float pY)
	{
		int pX2 = Mathf.Clamp(Mathf.FloorToInt(pX), 0, MapBox.width - 1);
		int pY2 = Mathf.Clamp(Mathf.FloorToInt(pY), 0, MapBox.height - 1);
		return World.world.GetTileSimple(pX2, pY2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static WorldTile getNearestTileToCursor()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		return getTileAt(val.x, val.y);
	}

	public static bool isBlockAt(float pX, float pY)
	{
		return getTileAt(pX, pY)?.Type.block ?? false;
	}

	public static List<string> splitStringIntoList(params string[] pTypes)
	{
		List<string> list = new List<string>();
		foreach (string text in pTypes)
		{
			if (text.Contains("#"))
			{
				string[] array = text.Split('#');
				string item = array[0];
				if (array.Length > 2)
				{
					Debug.LogError((object)("WRONG FORMAT - splitStringIntoList" + text));
					Debug.LogError((object)"RETURN EMPTY STRING");
					return new List<string>();
				}
				int num = int.Parse(array[1]);
				for (int j = 0; j < num; j++)
				{
					list.Add(item);
				}
			}
			else
			{
				list.Add(text);
			}
		}
		return list;
	}

	public static string[] splitStringIntoArray(params string[] pTypes)
	{
		using ListPool<string> listPool = new ListPool<string>(pTypes.Length * 2);
		foreach (string text in pTypes)
		{
			if (text.Contains('#'))
			{
				string[] array = text.Split('#');
				string item = array[0];
				if (array.Length > 2)
				{
					Debug.LogError((object)("WRONG FORMAT - splitStringIntoList" + text));
					Debug.LogError((object)"RETURN EMPTY STRING");
					return new string[0];
				}
				int num = int.Parse(array[1]);
				for (int j = 0; j < num; j++)
				{
					listPool.Add(item);
				}
			}
			else
			{
				listPool.Add(text);
			}
		}
		return listPool.ToArray();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool isFirstLatin(string pString)
	{
		char c = pString[0];
		if (c >= 'A' && c <= 'Z')
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 Parabola(Vector2 pStart, Vector2 pEnd, float pHeight, float pTime)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		pTime = Mathf.Clamp(pTime, 0f, 1f);
		Vector2 val = Vector2.Lerp(pStart, pEnd, pTime);
		return new Vector2(val.x, parabolaHelper(pTime, pHeight) + val.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 ParabolaDrag(Vector2 pStart, Vector2 pEnd, float pHeight, float pTime)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		pTime = Mathf.Clamp(pTime, 0f, 1f);
		float num = Mathf.Lerp(pStart.x, pEnd.x, iTween.easeOutQuad(0f, 1f, pTime));
		float num2 = Mathf.Lerp(pStart.y, pEnd.y, iTween.easeInQuad(0f, 1f, pTime));
		return new Vector2(num, parabolaHelper(pTime, pHeight) + num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float parabolaHelper(float pTime, float pHeight)
	{
		return -4f * pHeight * pTime * pTime + 4f * pHeight * pTime;
	}

	public static bool WriteSafely(string pWhat, string pDataPath, ref string pStringData)
	{
		return WriteSafely(pWhat, pDataPath, ref pStringData, null);
	}

	public static bool WriteSafely(string pWhat, string pDataPath, byte[] pByteData)
	{
		string pStringData = null;
		return WriteSafely(pWhat, pDataPath, ref pStringData, pByteData);
	}

	private static bool WriteSafely(string pWhat, string pDataPath, ref string pStringData, byte[] pByteData)
	{
		bool flag = false;
		try
		{
			if (!string.IsNullOrEmpty(pStringData))
			{
				File.WriteAllText(pDataPath + ".tmp", pStringData);
			}
			if (pByteData != null)
			{
				File.WriteAllBytes(pDataPath + ".tmp", pByteData);
			}
		}
		catch (IOException ex)
		{
			if (IsDiskFull(ex))
			{
				WorldTip.showNow("Error saving " + pWhat + " : Disk full!", pTranslate: false, "top");
			}
			else
			{
				Debug.Log((object)("Could not save " + pWhat + " due to hard drive / IO Error : "));
				Debug.Log((object)ex);
				WorldTip.showNow("Error saving " + pWhat + " due to IOError! Check console for details", pTranslate: false, "top");
			}
			flag = true;
		}
		catch (Exception ex2)
		{
			Debug.Log((object)("Could not save " + pWhat + " due to error : "));
			Debug.Log((object)ex2);
			WorldTip.showNow("Error saving " + pWhat + "! Check console for errors", pTranslate: false, "top");
			flag = true;
		}
		if (flag)
		{
			if (File.Exists(pDataPath + ".tmp"))
			{
				File.Delete(pDataPath + ".tmp");
			}
			return false;
		}
		if (File.Exists(pDataPath))
		{
			File.Delete(pDataPath);
		}
		File.Move(pDataPath + ".tmp", pDataPath);
		return true;
	}

	public static bool MoveSafely(string pOldPath, string pNewPath)
	{
		if (string.IsNullOrEmpty(pOldPath))
		{
			return false;
		}
		if (string.IsNullOrEmpty(pNewPath))
		{
			return false;
		}
		if (File.Exists(pNewPath))
		{
			File.Delete(pNewPath);
		}
		File.Move(pOldPath, pNewPath);
		return true;
	}

	public static bool IsDiskFull(IOException ex)
	{
		if ((ex.HResult & 0xFFFF) != 39)
		{
			return (ex.HResult & 0xFFFF) == 112;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string textureID(string pStringData, string pID)
	{
		return Encryption.EncryptString(pStringData, pID);
	}

	public static int getClosestAngle(int pAngle, AnimationDataBoat pData)
	{
		int num = int.MinValue;
		float num2 = 0f;
		foreach (int key in pData.dict.Keys)
		{
			float num3 = Mathf.Abs(key - pAngle);
			if (num3 < num2 || num == int.MinValue)
			{
				num2 = num3;
				num = key;
			}
		}
		return num;
	}

	public static bool isInTriangle(Vector2 pPoint, Vector2 p0, Vector2 p1, Vector2 p2)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.5f * ((0f - p1.y) * p2.x + p0.y * (0f - p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y);
		int num2 = ((!(num < 0f)) ? 1 : (-1));
		float num3 = (p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * pPoint.x + (p0.x - p2.x) * pPoint.y) * (float)num2;
		float num4 = (p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * pPoint.x + (p1.x - p0.x) * pPoint.y) * (float)num2;
		if (num3 > 0f && num4 > 0f)
		{
			return num3 + num4 < 2f * num * (float)num2;
		}
		return false;
	}

	public static List<string> getListForSave<T>(IReadOnlyCollection<T> pList) where T : Asset
	{
		List<string> list = new List<string>(pList.Count);
		foreach (T p in pList)
		{
			list.Add(p.id);
		}
		return list;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] checkArraySize<T>(T[] pArray, int pTargetSize)
	{
		if (pArray == null || pTargetSize > pArray.Length)
		{
			pArray = new T[nextPowerOfTwo(pTargetSize)];
		}
		return pArray;
	}

	private static int nextPowerOfTwo(int pN)
	{
		pN--;
		pN |= pN >> 1;
		pN |= pN >> 2;
		pN |= pN >> 4;
		pN |= pN >> 8;
		pN |= pN >> 16;
		pN++;
		return pN;
	}

	public static string fillLeft(string pString, int pSize = 1, char pFill = ' ')
	{
		string text = removeRichTextTags(pString);
		if (pString != text)
		{
			pSize += pString.Length - text.Length;
		}
		if (pString.Length >= pSize)
		{
			return pString;
		}
		Span<char> span = stackalloc char[pSize];
		pString.AsSpan().CopyTo(span.Slice(pSize - pString.Length));
		for (int i = 0; i < pSize - pString.Length; i++)
		{
			span[i] = pFill;
		}
		return new string(span);
	}

	public static void fillRight(ref string pString, int pSize = 1, char pFill = ' ')
	{
		int length = pString.Length;
		if (removeRichTextTags(ref pString))
		{
			pSize += length - pString.Length;
		}
		if (pString.Length < pSize)
		{
			Span<char> span = stackalloc char[pSize];
			pString.AsSpan().CopyTo(span.Slice(0, pString.Length));
			for (int i = pString.Length; i < pSize; i++)
			{
				span[i] = pFill;
			}
			pString = new string(span);
		}
	}

	public static string fillRight(string pString, int pSize = 1, char pFill = ' ')
	{
		string text = removeRichTextTags(pString);
		if (pString != text)
		{
			pSize += pString.Length - text.Length;
		}
		if (pString.Length >= pSize)
		{
			return pString;
		}
		Span<char> span = stackalloc char[pSize];
		pString.AsSpan().CopyTo(span.Slice(0, pString.Length));
		for (int i = pString.Length; i < pSize; i++)
		{
			span[i] = pFill;
		}
		return new string(span);
	}

	public static string printRows(ListPool<string[]> pRows, string pAlign = "right", bool pSkipFormatting = false)
	{
		int num = 0;
		int count = pRows.Count;
		for (int i = 0; i < count; i++)
		{
			string[] array = pRows[i];
			if (array.Length > num)
			{
				num = array.Length;
			}
		}
		int[] array2 = new int[num];
		for (int j = 0; j < count; j++)
		{
			string[] array3 = pRows[j];
			for (int k = 0; k < array3.Length; k++)
			{
				int length = removeRichTextTags(array3[k]).Length;
				if (length > array2[k])
				{
					array2[k] = length;
				}
			}
		}
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		for (int l = 0; l <= count; l++)
		{
			if (l == 0 || l == count || pRows[l].Length == 0)
			{
				stringBuilderPool.Append("|");
				for (int m = 0; m < num; m++)
				{
					if (array2[m] != 0)
					{
						stringBuilderPool.Append(fillRight("", array2[m] + 2, '='));
						stringBuilderPool.Append("|");
					}
				}
				stringBuilderPool.Append("\n");
				if (l == count)
				{
					break;
				}
			}
			string[] array4 = pRows[l];
			if (array4.Length == 0)
			{
				continue;
			}
			stringBuilderPool.Append("|");
			for (int n = 0; n < num; n++)
			{
				if (array2[n] == 0)
				{
					continue;
				}
				string pString = "";
				if (n < array4.Length)
				{
					pString = array4[n];
				}
				stringBuilderPool.Append(" ");
				if (n == 0)
				{
					if (!pSkipFormatting)
					{
						stringBuilderPool.Append("<b>");
					}
					stringBuilderPool.Append(fillRight(pString, array2[n]));
					if (!pSkipFormatting)
					{
						stringBuilderPool.Append("</b>");
					}
				}
				else if (pAlign == "right")
				{
					stringBuilderPool.Append(fillLeft(pString, array2[n]));
				}
				else
				{
					stringBuilderPool.Append(fillRight(pString, array2[n]));
				}
				stringBuilderPool.Append(" ");
				stringBuilderPool.Append("|");
			}
			stringBuilderPool.Append("\n");
		}
		return stringBuilderPool.ToString();
	}

	public static string printColumns(params ListPool<string>[] pLists)
	{
		int num = 0;
		int num2 = pLists.Length;
		int[] array = new int[num2];
		for (int i = 0; i < num2; i++)
		{
			ListPool<string> listPool = pLists[i];
			if (listPool.Count > num)
			{
				num = listPool.Count;
			}
			for (int j = 0; j < listPool.Count; j++)
			{
				int length = removeRichTextTags(listPool[j]).Length;
				if (length > array[i])
				{
					array[i] = length;
				}
			}
		}
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		for (int k = 0; k < num; k++)
		{
			if (k == 0 || k == 1)
			{
				stringBuilderPool.Append("|");
				for (int l = 0; l < num2; l++)
				{
					if (array[l] != 0)
					{
						stringBuilderPool.Append(fillRight("", array[l] + 2, '='));
						stringBuilderPool.Append("|");
					}
				}
				stringBuilderPool.Append("\n");
			}
			stringBuilderPool.Append("|");
			for (int m = 0; m < num2; m++)
			{
				if (array[m] != 0)
				{
					string pString = "";
					if (k < pLists[m].Count)
					{
						pString = pLists[m][k];
					}
					stringBuilderPool.Append(" ");
					if (k == 0)
					{
						stringBuilderPool.Append("<b>");
					}
					stringBuilderPool.Append(fillRight(pString, array[m] + 1));
					if (k == 0)
					{
						stringBuilderPool.Append("</b>");
					}
					stringBuilderPool.Append("|");
				}
			}
			stringBuilderPool.Append("\n");
			if (k != num - 1)
			{
				continue;
			}
			stringBuilderPool.Append("|");
			for (int n = 0; n < num2; n++)
			{
				if (array[n] != 0)
				{
					stringBuilderPool.Append(fillRight("", array[n] + 2, '='));
					stringBuilderPool.Append("|");
				}
			}
			stringBuilderPool.Append("\n");
		}
		return stringBuilderPool.ToString();
	}

	public static string getRepeatedString(char pChar, int pCount)
	{
		Span<char> span = stackalloc char[pCount];
		span.Fill(pChar);
		return new string(span);
	}

	public static bool removeRichTextTags(ref string pInput)
	{
		bool result = false;
		while (true)
		{
			int num = pInput.IndexOf('<');
			if (num == -1)
			{
				return result;
			}
			int num2 = pInput.IndexOf('>', num);
			if (num2 == -1)
			{
				break;
			}
			pInput = pInput.Remove(num, num2 - num + 1);
			result = true;
		}
		return result;
	}

	public static string removeRichTextTags(string pInput)
	{
		while (true)
		{
			int num = pInput.IndexOf('<');
			if (num == -1)
			{
				return pInput;
			}
			int num2 = pInput.IndexOf('>', num);
			if (num2 == -1)
			{
				break;
			}
			pInput = pInput.Remove(num, num2 - num + 1);
		}
		return pInput;
	}

	public static bool areListsEqual<T>(IList<T> pList1, IList<T> pList2)
	{
		HashSet<T> hashSet = UnsafeCollectionPool<HashSet<T>, T>.Get();
		hashSet.UnionWith(pList1);
		bool result = hashSet.SetEquals(pList2);
		UnsafeCollectionPool<HashSet<T>, T>.Release(hashSet);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TA[] a<TA>(params TA[] pArgs)
	{
		return pArgs;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static List<TL> l<TL>(params TL[] pArgs)
	{
		return List.Of(pArgs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static HashSet<TH> h<TH>(params TH[] pArgs)
	{
		return new HashSet<TH>(pArgs);
	}

	static Toolbox()
	{
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_050b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_051f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_055b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_0593: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0600: Unknown result type (might be due to invalid IL or missing references)
		//IL_0605: Unknown result type (might be due to invalid IL or missing references)
		//IL_060d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_0629: Unknown result type (might be due to invalid IL or missing references)
		//IL_062e: Unknown result type (might be due to invalid IL or missing references)
		//IL_063a: Unknown result type (might be due to invalid IL or missing references)
		//IL_063f: Unknown result type (might be due to invalid IL or missing references)
		//IL_064b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0650: Unknown result type (might be due to invalid IL or missing references)
		//IL_065c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0661: Unknown result type (might be due to invalid IL or missing references)
		//IL_066d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0672: Unknown result type (might be due to invalid IL or missing references)
		directions = new ActorDirection[4]
		{
			ActorDirection.Up,
			ActorDirection.Right,
			ActorDirection.Down,
			ActorDirection.Left
		};
		directions_all = new ActorDirection[8]
		{
			ActorDirection.Up,
			ActorDirection.UpRight,
			ActorDirection.UpLeft,
			ActorDirection.Right,
			ActorDirection.DownRight,
			ActorDirection.DownLeft,
			ActorDirection.Down,
			ActorDirection.Left
		};
		directions_turns = new Dictionary<ActorDirection, ActorDirection[]>
		{
			{
				ActorDirection.Up,
				new ActorDirection[2]
				{
					ActorDirection.Right,
					ActorDirection.Left
				}
			},
			{
				ActorDirection.Right,
				new ActorDirection[2]
				{
					ActorDirection.Down,
					ActorDirection.Up
				}
			},
			{
				ActorDirection.Down,
				new ActorDirection[2]
				{
					ActorDirection.Left,
					ActorDirection.Right
				}
			},
			{
				ActorDirection.Left,
				new ActorDirection[2]
				{
					ActorDirection.Up,
					ActorDirection.Down
				}
			}
		};
		directions_all_turns = new Dictionary<ActorDirection, ActorDirection[]>
		{
			{
				ActorDirection.Up,
				new ActorDirection[4]
				{
					ActorDirection.Right,
					ActorDirection.UpRight,
					ActorDirection.UpLeft,
					ActorDirection.Left
				}
			},
			{
				ActorDirection.UpRight,
				new ActorDirection[4]
				{
					ActorDirection.DownRight,
					ActorDirection.Right,
					ActorDirection.Up,
					ActorDirection.UpLeft
				}
			},
			{
				ActorDirection.Right,
				new ActorDirection[4]
				{
					ActorDirection.Down,
					ActorDirection.DownRight,
					ActorDirection.UpRight,
					ActorDirection.Up
				}
			},
			{
				ActorDirection.DownRight,
				new ActorDirection[4]
				{
					ActorDirection.DownLeft,
					ActorDirection.Down,
					ActorDirection.Right,
					ActorDirection.UpRight
				}
			},
			{
				ActorDirection.Down,
				new ActorDirection[4]
				{
					ActorDirection.Left,
					ActorDirection.DownLeft,
					ActorDirection.DownRight,
					ActorDirection.Right
				}
			},
			{
				ActorDirection.DownLeft,
				new ActorDirection[4]
				{
					ActorDirection.UpLeft,
					ActorDirection.Left,
					ActorDirection.Down,
					ActorDirection.DownRight
				}
			},
			{
				ActorDirection.Left,
				new ActorDirection[4]
				{
					ActorDirection.Up,
					ActorDirection.UpLeft,
					ActorDirection.DownLeft,
					ActorDirection.Down
				}
			},
			{
				ActorDirection.UpLeft,
				new ActorDirection[4]
				{
					ActorDirection.UpRight,
					ActorDirection.Up,
					ActorDirection.Left,
					ActorDirection.DownLeft
				}
			}
		};
		EVERYTHING_MAGIC_COLOR32 = Color32.op_Implicit(makeColor("#DF7FFF"));
		color_grey_dark = Color32.op_Implicit(makeColor("#5D5D5D"));
		color_grey = Color32.op_Implicit(makeColor("#AAAAAA"));
		color_transparent_grey = Color32.op_Implicit(makeColor("#666666", 0.5f));
		color_debug_bar_blue = Color32.op_Implicit(makeColor("#0092FF", 0.5f));
		color_debug_bar_red = Color32.op_Implicit(makeColor("#FF6262", 0.5f));
		color_phenotype_green_0 = Color32.op_Implicit(makeColor("#B8FF96"));
		color_phenotype_green_1 = Color32.op_Implicit(makeColor("#00FF00"));
		color_phenotype_green_2 = Color32.op_Implicit(makeColor("#00AF00"));
		color_phenotype_green_3 = Color32.op_Implicit(makeColor("#4A831F"));
		color_map_icon_green = Color32.op_Implicit(makeColor("#00FF00"));
		color_magenta_0 = Color32.op_Implicit(makeColor("#FF00FF"));
		color_magenta_1 = Color32.op_Implicit(makeColor("#DE00DE"));
		color_magenta_2 = Color32.op_Implicit(makeColor("#A700A7"));
		color_magenta_3 = Color32.op_Implicit(makeColor("#7F007F"));
		color_magenta_4 = Color32.op_Implicit(makeColor("#580058"));
		color_teal_0 = Color32.op_Implicit(makeColor("#00EFEF"));
		color_teal_1 = Color32.op_Implicit(makeColor("#00DBDB"));
		color_teal_2 = Color32.op_Implicit(makeColor("#00BCBC"));
		color_teal_3 = Color32.op_Implicit(makeColor("#009E9E"));
		color_teal_4 = Color32.op_Implicit(makeColor("#007777"));
		color_ocean = Color32.op_Implicit(makeColor("#3370CC"));
		color_night = Color32.op_Implicit(makeColor("#05003F"));
		color_light = Color32.op_Implicit(makeColor("#FFD800"));
		color_light_100 = Color32.op_Implicit(makeColor("#FFFFFF"));
		color_light_10 = Color32.op_Implicit(makeColor("#FFFFFF", 0.3f));
		color_light_replace = Color32.op_Implicit(makeColor("#000000"));
		color_augmentation_selected = Color.white;
		color_augmentation_unselected = new Color(0.7f, 0.7f, 0.7f, 1f);
		color_clear = Color32.op_Implicit(Color.clear);
		color_white = Color.white;
		color_gray = Color.gray;
		color_black = Color.black;
		color_black_32 = Color32.op_Implicit(Color.black);
		color_white_32 = Color32.op_Implicit(Color.white);
		color_red = Color.red;
		color_yellow = Color.yellow;
		color_blue = Color.blue;
		color_green = Color.green;
		color_purple = new Color(0.5f, 0f, 0.5f);
		color_cyan = Color.cyan;
		color_cursed = new Color(1f, 0f, 71f / 85f);
		color_abandoned_building = new Color(0.8f, 0.8f, 0.8f);
		color_positive_RGBA = makeColor("#43FF43");
		color_negative_RGBA = makeColor("#FB2C21");
		clear = Color32.op_Implicit(Color.clear);
		edge_alpha = Color32.op_Implicit(makeColor("#000000", 0.1f));
		color_white_transparent = makeColor("#FFFFFF", 0f);
		color_text_default = makeColor("#FF9B1C");
		color_text_default_bright = makeColor("#FFBC66");
		color_log_good = makeColor("#95DD5D");
		color_log_warning = makeColor("#FF8686");
		color_log_neutral = makeColor("#F3961F");
		color_fire = Color32.op_Implicit(makeColor("#FF6930"));
		color_heal = makeColor("#23F3FF");
		color_plague = makeColor("#CE4A9B");
		color_mushSpores = makeColor("#8CFF99");
		color_infected = makeColor("#35CC6E");
		color_poisoned = makeColor("#D85BC5");
		colors_fire = (Color[])(object)new Color[10]
		{
			makeColor("#D95032"),
			makeColor("#F27F3D"),
			makeColor("#F2A444"),
			makeColor("#F2C36B"),
			makeColor("#F2CA50"),
			makeColor("#E35632"),
			makeColor("#EEB543"),
			Color.red,
			Color.yellow,
			Color.white
		};
		colors_wheat = (Color[])(object)new Color[5]
		{
			makeColor("#20B22B"),
			makeColor("#2A8E31"),
			makeColor("#20B22B"),
			makeColor("#74A926"),
			makeColor("#FFEB93")
		};
		temp_list_tiles = new List<WorldTile>();
		_temp_array_chunks = new MapChunk[9];
		_temp_array_zones = new TileZone[9];
	}
}
