internal static class ListAccessHelper
{
	internal record ListDataHelper<T>
	{
		public T[] _items;

		public int _size;

		public int _version;
	}

	public static readonly int ItemsOffset = 0;

	public static readonly int SizeOffset = 8;

	public static readonly int VersionOffset = 12;
}
