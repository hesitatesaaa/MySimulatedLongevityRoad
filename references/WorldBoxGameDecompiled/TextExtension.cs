using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public static class TextExtension
{
	private static Font krutiDev;

	private static List<string> colors = new List<string>();

	public static void SetHindiText(this Text text, string value)
	{
		if ((Object)(object)krutiDev == (Object)null)
		{
			Object obj = Resources.Load("CD_Kruti_Dev_010");
			krutiDev = (Font)(object)((obj is Font) ? obj : null);
		}
		bool flag = value.IndexOf("</color>", StringComparison.Ordinal) > -1;
		if (flag)
		{
			colors.Clear();
			value = value.Replace("</color>", "END_COLOR");
			int num = 0;
			foreach (object item in Regex.Matches(value, "<color.*?>"))
			{
				colors.Add(item.ToString());
				value = value.Replace(item.ToString(), "COLOR_" + num++);
			}
		}
		if (value.IndexOf("'", StringComparison.Ordinal) > -1)
		{
			value = value.Replace("'", "SINGLE_QUOTE");
		}
		value = HindiCorrector.GetCorrectedHindiText(value);
		if (value.IndexOf("SINGLE_QUOTE", StringComparison.Ordinal) > -1)
		{
			value = value.Replace("SINGLE_QUOTE", "'");
		}
		if (flag)
		{
			value = value.Replace("END_COLOR", "</color>");
			int num2 = 0;
			foreach (string color in colors)
			{
				value = value.Replace("COLOR_" + num2++, color);
			}
		}
		text.font = krutiDev;
		text.text = value;
	}
}
