using System.Runtime.CompilerServices;

public static class LongExtension
{
	public const long NULL = -1L;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool hasValue(this long pLong)
	{
		return pLong != -1;
	}

	public static long? toNullLong(this long pLong)
	{
		if (!pLong.hasValue())
		{
			return null;
		}
		return pLong;
	}

	public static long toLong(this long? pLong)
	{
		return pLong ?? (-1);
	}
}
