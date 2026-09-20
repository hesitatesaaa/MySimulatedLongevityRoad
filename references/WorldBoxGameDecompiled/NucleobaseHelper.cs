using UnityEngine;

public static class NucleobaseHelper
{
	private const char SHORT_ADENINE = 'A';

	private const char SHORT_CYTOSINE = 'C';

	private const char SHORT_GUANINE = 'G';

	private const char SHORT_THYMINE = 'T';

	private const string NOT_CONNECTED_TRANSPARENCY = "88";

	private const string COLOR_HEX_ADENINE = "#70FF70";

	private const string COLOR_HEX_CYTOSINE = "#3A8FFF";

	private const string COLOR_HEX_GUANINE = "#FFDF42";

	private const string COLOR_HEX_THYMINE = "#FF3A3A";

	private const string COLOR_HEX_ADENINE_DARK = "#70FF7088";

	private const string COLOR_HEX_CYTOSINE_DARK = "#3A8FFF88";

	private const string COLOR_HEX_GUANINE_DARK = "#FFDF4288";

	private const string COLOR_HEX_THYMINE_DARK = "#FF3A3A88";

	private static readonly Color color_adenine;

	private static readonly Color color_cytosine;

	private static readonly Color color_guanine;

	private static readonly Color color_thymine;

	private static readonly Color color_adenine_dark;

	private static readonly Color color_cytosine_dark;

	private static readonly Color color_guanine_dark;

	private static readonly Color color_thymine_dark;

	public static readonly Color color_bad;

	public const string COLOR_HEX_BAD = "#B159FF";

	public static Color getColor(char pChar, bool pDark = false)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (pDark)
		{
			return (Color)(pChar switch
			{
				'A' => color_adenine_dark, 
				'C' => color_cytosine_dark, 
				'G' => color_guanine_dark, 
				'T' => color_thymine_dark, 
				_ => Color.black, 
			});
		}
		return (Color)(pChar switch
		{
			'A' => color_adenine, 
			'C' => color_cytosine, 
			'G' => color_guanine, 
			'T' => color_thymine, 
			_ => Color.black, 
		});
	}

	public static string getColorHex(char pChar, bool pDark = false)
	{
		string result = string.Empty;
		if (pDark)
		{
			switch (pChar)
			{
			case 'A':
				result = "#70FF7088";
				break;
			case 'C':
				result = "#3A8FFF88";
				break;
			case 'G':
				result = "#FFDF4288";
				break;
			case 'T':
				result = "#FF3A3A88";
				break;
			}
		}
		else
		{
			switch (pChar)
			{
			case 'A':
				result = "#70FF70";
				break;
			case 'C':
				result = "#3A8FFF";
				break;
			case 'G':
				result = "#FFDF42";
				break;
			case 'T':
				result = "#FF3A3A";
				break;
			}
		}
		return result;
	}

	private static string getNucleobaseFullID(char pChar)
	{
		string result = string.Empty;
		switch (pChar)
		{
		case 'A':
			result = "nucleo_adenine";
			break;
		case 'C':
			result = "nucleo_cytosine";
			break;
		case 'G':
			result = "nucleo_guanine";
			break;
		case 'T':
			result = "nucleo_thymine";
			break;
		}
		return result;
	}

	public static string getColoredNucleobaseFull(char pChar)
	{
		string colorHex = getColorHex(pChar);
		string fullNucleobaseName = getFullNucleobaseName(pChar);
		return "<color=" + colorHex + ">" + fullNucleobaseName + "</color>";
	}

	public static string getFullNucleobaseName(char pChar)
	{
		return LocalizedTextManager.getText(getNucleobaseFullID(pChar));
	}

	public static string getColoredSequence(string pGeneticCode)
	{
		string text = string.Empty;
		foreach (char c in pGeneticCode)
		{
			string colorHex = getColorHex(c);
			text += $"<color={colorHex}>{c}</color>";
		}
		return text;
	}

	static NucleobaseHelper()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		color_adenine = Toolbox.makeColor("#70FF70");
		color_cytosine = Toolbox.makeColor("#3A8FFF");
		color_guanine = Toolbox.makeColor("#FFDF42");
		color_thymine = Toolbox.makeColor("#FF3A3A");
		color_adenine_dark = Toolbox.makeColor("#70FF7088");
		color_cytosine_dark = Toolbox.makeColor("#3A8FFF88");
		color_guanine_dark = Toolbox.makeColor("#FFDF4288");
		color_thymine_dark = Toolbox.makeColor("#FF3A3A88");
		color_bad = Color.black;
	}
}
