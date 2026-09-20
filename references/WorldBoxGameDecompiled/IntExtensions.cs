public static class IntExtensions
{
	private static readonly (int value, string symbol)[] _roman_number_map = new(int, string)[13]
	{
		(1000, "M"),
		(900, "CM"),
		(500, "D"),
		(400, "CD"),
		(100, "C"),
		(90, "XC"),
		(50, "L"),
		(40, "XL"),
		(10, "X"),
		(9, "IX"),
		(5, "V"),
		(4, "IV"),
		(1, "I")
	};

	public static string ToText(this int pInt)
	{
		return pInt.ToString("##,0.#");
	}

	public static string ToText(this long pLong)
	{
		return pLong.ToString("##,0.#");
	}

	public static string ToText(this float pFloat)
	{
		return pFloat.ToString("##,0.#");
	}

	public static string ToText(this double pDouble)
	{
		return pDouble.ToString("##,0.#");
	}

	public static string ToText(this int pInt, int pMaxLength)
	{
		return Toolbox.formatNumber(pInt, pMaxLength);
	}

	public static string ToText(this long pLong, int pMaxLength)
	{
		return Toolbox.formatNumber(pLong, pMaxLength);
	}

	public static string ToRoman(this int pNumber)
	{
		if (pNumber < 1)
		{
			return "N";
		}
		if (pNumber > 3999)
		{
			return "MMMM";
		}
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		(int, string)[] roman_number_map = _roman_number_map;
		for (int i = 0; i < roman_number_map.Length; i++)
		{
			var (num, value) = roman_number_map[i];
			while (pNumber >= num)
			{
				stringBuilderPool.Append(value);
				pNumber -= num;
			}
		}
		return stringBuilderPool.ToString();
	}
}
