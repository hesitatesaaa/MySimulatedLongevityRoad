using System;

public static class EnumExtensions
{
	public static int Count<TEnum>(this TEnum pEnum) where TEnum : Enum
	{
		int num = 0;
		int num2 = Convert.ToInt32(pEnum);
		while (num2 != 0)
		{
			num2 &= num2 - 1;
			num++;
		}
		return num;
	}
}
