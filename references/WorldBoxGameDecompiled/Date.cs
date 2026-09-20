using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

public static class Date
{
	public static string getAgoString(double pTimestamp)
	{
		return formatSeconds(World.world.getWorldTimeElapsedSince(pTimestamp)) + " ago";
	}

	public static string formatSeconds(float pSeconds)
	{
		if (pSeconds < 60f)
		{
			return ((int)pSeconds).ToText() + "s";
		}
		string text = ((int)pSeconds / 60).ToText();
		return text + "m";
	}

	public static float getMonthTime()
	{
		int monthsSince = getMonthsSince(0.0);
		return (float)World.world.getCurWorldTime() - (float)monthsSince * 5f;
	}

	public static string getYearDate(double pTime)
	{
		return getYear(pTime).ToText();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int getYear(double pTime)
	{
		return getYear0(pTime) + 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int getYear0(double pTime)
	{
		return (int)(pTime / 60.0);
	}

	public static int[] getRawDate(double pTime)
	{
		int num = (int)(pTime / 5.0 / 12.0);
		while (pTime < 0.0)
		{
			pTime += 600000.0;
		}
		double num2 = pTime / 5.0;
		double num3 = num2 / 12.0;
		int num4 = (int)num2;
		int num5 = (int)num3;
		int num6 = (int)((pTime - (double)((float)num5 * 5f * 12f)) / 5.0);
		int num7 = (int)((pTime - (double)((float)num4 * 5f)) / 5.0 * 30.0);
		num++;
		num6++;
		num7++;
		return new int[3] { num7, num6, num };
	}

	public static string getDate(double pTime)
	{
		int[] rawDate = getRawDate(pTime);
		int num = rawDate[0];
		int pMonth = rawDate[1];
		int num2 = rawDate[2];
		if (LocalizedTextManager.instance.language == "en")
		{
			using (StringBuilderPool stringBuilderPool = new StringBuilderPool())
			{
				stringBuilderPool.Append(num);
				stringBuilderPool.Append(GetDaySuffix(num));
				stringBuilderPool.Append(" of ");
				stringBuilderPool.Append(formatMonth(pMonth));
				stringBuilderPool.Append(", ");
				stringBuilderPool.Append(num2.ToText());
				return stringBuilderPool.ToString();
			}
		}
		return formatDate(num, pMonth, num2);
	}

	internal static string formatMonth(int pMonth)
	{
		return LocalizedTextManager.getText("month_" + pMonth);
	}

	internal static string formatDate(int pDay, int pMonth, int pYear)
	{
		CultureInfo culture = LocalizedTextManager.getCulture();
		string input = culture.DateTimeFormat.LongDatePattern;
		if (culture.TwoLetterISOLanguageName == "ar")
		{
			input = "/ddMMMMyyyy/";
		}
		string text = Regex.Replace(input, "\\bdddd[,\\s]*", "").Trim();
		string text2 = LocalizedTextManager.getText("inflected_month_" + pMonth);
		MatchCollection matchCollection = null;
		if (text.Contains("'"))
		{
			matchCollection = Regex.Matches(text, "'[^']*'");
			for (int i = 0; i < matchCollection.Count; i++)
			{
				text = text.Replace(matchCollection[i].Value, "{{{" + i + "}}}");
			}
		}
		text = text.Replace("MMMM", "[[[1]]]");
		text = text.Replace("yyyy", "[[[2]]]");
		text = text.Replace("dd", "[[[3]]]");
		text = Regex.Replace(text, "\\b[d]\\b", "[[[4]]]");
		text = text.Replace("MM", "[[[5]]]");
		text = text.Replace("M", "[[[6]]]");
		text = text.Replace("[[[1]]]", text2);
		text = text.Replace("[[[2]]]", pYear.ToText());
		text = text.Replace("[[[3]]]", (pDay < 10) ? ("0" + pDay) : pDay.ToString());
		text = text.Replace("[[[4]]]", pDay.ToString());
		text = text.Replace("[[[5]]]", (pMonth < 10) ? ("0" + pMonth) : pMonth.ToString());
		text = text.Replace("[[[6]]]", pMonth.ToString());
		if (matchCollection != null && matchCollection.Count > 0)
		{
			for (int num = matchCollection.Count - 1; num >= 0; num--)
			{
				text = text.Replace("{{{" + num + "}}}", matchCollection[num].Value.Trim('\''));
			}
		}
		return text;
	}

	public static int getCurrentMonth()
	{
		return getMonth(World.world.getCurWorldTime());
	}

	public static int getMonth(double pTimestamp)
	{
		float num = getYear0(pTimestamp);
		return (int)((pTimestamp - (double)(num * 12f * 5f)) / 5.0 + 1.0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int getCurrentYear()
	{
		return getYear(World.world.getCurWorldTime());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int getYearsSince(double pFrom)
	{
		return getYear0(World.world.getCurWorldTime() - pFrom);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int getMonthsSince(double pFrom)
	{
		return (int)((World.world.getCurWorldTime() - pFrom) / 5.0);
	}

	private static string GetDaySuffix(int day)
	{
		switch (day)
		{
		case 1:
		case 21:
		case 31:
			return "st";
		case 2:
		case 22:
			return "nd";
		case 3:
		case 23:
			return "rd";
		default:
			return "th";
		}
	}

	public static bool isMonolithMonth()
	{
		if (getCurrentMonth() == 4)
		{
			return true;
		}
		return false;
	}

	public static string getUIStringYearMonthShort()
	{
		return "y:" + getCurrentYear().ToText() + ", m:" + getCurrentMonth().ToText();
	}

	public static string getUIStringYearMonth()
	{
		return "y: " + getCurrentYear().ToText() + ", m: " + getCurrentMonth().ToText();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string TimeNow()
	{
		DateTime now = DateTime.Now;
		char[] array = new char[8];
		Write2Chars(array, 0, now.Hour);
		array[2] = ':';
		Write2Chars(array, 3, now.Minute);
		array[5] = ':';
		Write2Chars(array, 6, now.Second);
		return new string(array);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Write2Chars(char[] chars, int offset, int value)
	{
		chars[offset] = Digit(value / 10);
		chars[offset + 1] = Digit(value % 10);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static char Digit(int value)
	{
		return (char)(value + 48);
	}
}
