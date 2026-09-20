using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

[Serializable]
public sealed class ListPool<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>, IDisposable
{
	public struct Enumerator(T[] source, int itemsCount) : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly T[] _source = source;

		private readonly int _itemsCount = itemsCount;

		private int _index = -1;

		public readonly ref T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[return: MaybeNull]
			get
			{
				return ref _source[_index];
			}
		}

		readonly T IEnumerator<T>.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[return: MaybeNull]
			get
			{
				return _source[_index];
			}
		}

		readonly object? IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[return: MaybeNull]
			get
			{
				return _source[_index];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			return ++_index < _itemsCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_index = -1;
		}

		public readonly void Dispose()
		{
		}
	}

	private const int MinimumCapacity = 32;

	private T[] _items;

	[NonSerialized]
	private object? _syncRoot;

	private static readonly ArrayPool<T> _arrayPool = ArrayPool<T>.Shared;

	private static readonly bool _should_clean = !typeof(T).IsValueType && typeof(string) != typeof(T);

	public int Capacity => _items.Length;

	int ICollection.Count => Count;

	bool IList.IsFixedSize => false;

	bool ICollection.IsSynchronized => false;

	bool IList.IsReadOnly => false;

	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
			}
			return _syncRoot;
		}
	}

	object IList.this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[return: MaybeNull]
		get
		{
			if (index >= Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return _items[index];
		}
		set
		{
			if (index >= Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			if (value is T val)
			{
				_items[index] = val;
				return;
			}
			throw new ArgumentException($"Wrong value type. Expected {typeof(T)}, got: '{value}'.", "value");
		}
	}

	public int Count { get; private set; }

	public bool IsReadOnly => false;

	public T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[return: MaybeNull]
		get
		{
			if (index >= Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return _items[index];
		}
		set
		{
			if (index >= Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			_items[index] = value;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ListPool()
	{
		_items = _arrayPool.Rent(32);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ListPool(int capacity)
	{
		_items = _arrayPool.Rent((capacity < 32) ? 32 : capacity);
	}

	public ListPool(ICollection<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		T[] array = _arrayPool.Rent((collection.Count > 32) ? collection.Count : 32);
		collection.CopyTo(array, 0);
		_items = array;
		Count = collection.Count;
	}

	public ListPool(IEnumerable<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		int num = Enumerable.Count(source);
		_items = _arrayPool.Rent((num > 32) ? num : 32);
		T[] items = _items;
		Count = 0;
		int num2 = 0;
		using IEnumerator<T> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (num2 < items.Length)
			{
				items[num2] = enumerator.Current;
				num2++;
				continue;
			}
			Count = num2;
			AddWithResize(enumerator.Current);
			num2++;
			items = _items;
		}
		Count = num2;
	}

	public ListPool(T[] source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		int minimumLength = ((source.Length > 32) ? source.Length : 32);
		T[] array = _arrayPool.Rent(minimumLength);
		source.CopyTo(array, 0);
		_items = array;
		Count = source.Length;
	}

	public ListPool(ReadOnlySpan<T> source)
	{
		int minimumLength = ((source.Length > 32) ? source.Length : 32);
		T[] array = _arrayPool.Rent(minimumLength);
		source.CopyTo(array);
		_items = array;
		Count = source.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		if (_should_clean)
		{
			Clear();
		}
		Count = 0;
		_arrayPool.Return(_items);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	int IList.Add(object item)
	{
		if (item is T item2)
		{
			Add(item2);
			return Count - 1;
		}
		throw new ArgumentException($"Wrong value type. Expected {typeof(T)}, got: '{item}'.", "item");
	}

	bool IList.Contains(object item)
	{
		if (item is T item2)
		{
			return Contains(item2);
		}
		throw new ArgumentException($"Wrong value type. Expected {typeof(T)}, got: '{item}'.", "item");
	}

	int IList.IndexOf(object item)
	{
		if (item is T item2)
		{
			return IndexOf(item2);
		}
		throw new ArgumentException($"Wrong value type. Expected {typeof(T)}, got: '{item}'.", "item");
	}

	void IList.Remove(object item)
	{
		if (item is T item2)
		{
			Remove(item2);
		}
		else if (item != null)
		{
			throw new ArgumentException($"Wrong value type. Expected {typeof(T)}, got: '{item}'.", "item");
		}
	}

	void IList.Insert(int index, object item)
	{
		if (item is T item2)
		{
			Insert(index, item2);
			return;
		}
		throw new ArgumentException($"Wrong value type. Expected {typeof(T)}, got: '{item}'.", "item");
	}

	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		Array.Copy(_items, 0, array, arrayIndex, Count);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(T item)
	{
		T[] items = _items;
		int count = Count;
		if (count < items.Length)
		{
			items[count] = item;
			Count = count + 1;
		}
		else
		{
			AddWithResize(item);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear()
	{
		if (Count > 0)
		{
			Array.Clear(_items, 0, Count);
			Count = 0;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear(int pAfterIndex)
	{
		if (Count > 0 && pAfterIndex < Count)
		{
			Array.Clear(_items, pAfterIndex, Count - pAfterIndex);
			Count = pAfterIndex;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(T item)
	{
		return IndexOf(item) > -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int IndexOf(T item)
	{
		return Array.IndexOf(_items, item, 0, Count);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CopyTo(T[] array, int arrayIndex)
	{
		Array.Copy(_items, 0, array, arrayIndex, Count);
	}

	public bool Remove(T item)
	{
		if (item == null)
		{
			return false;
		}
		int num = IndexOf(item);
		if (num == -1)
		{
			return false;
		}
		RemoveAt(num);
		return true;
	}

	public void Insert(int index, T item)
	{
		int count = Count;
		T[] items = _items;
		if (items.Length == count)
		{
			int capacity = count * 2;
			EnsureCapacity(capacity);
			items = _items;
		}
		if (index < count)
		{
			Array.Copy(items, index, items, index + 1, count - index);
			items[index] = item;
			Count++;
			return;
		}
		if (index == count)
		{
			items[index] = item;
			Count++;
			return;
		}
		throw new IndexOutOfRangeException("index");
	}

	public void RemoveAt(int index)
	{
		int count = Count;
		T[] items = _items;
		if (index >= count)
		{
			throw new IndexOutOfRangeException("index");
		}
		count--;
		Array.Copy(items, index + 1, items, index, count - index);
		if (_should_clean)
		{
			items[count] = default(T);
		}
		Count = count;
	}

	public int RemoveAll(Predicate<T> match)
	{
		int count = Count;
		T[] items = _items;
		int i;
		for (i = 0; i < count && !match(items[i]); i++)
		{
		}
		if (i >= count)
		{
			return 0;
		}
		int j = i + 1;
		while (j < count)
		{
			for (; j < count && match(items[j]); j++)
			{
			}
			if (j < count)
			{
				items[i++] = items[j++];
			}
		}
		if (_should_clean)
		{
			Array.Clear(items, i, count - i);
		}
		int result = count - i;
		Count = i;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(_items, Count);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(_items, Count);
	}

	public void AddRange(Span<T> items)
	{
		int count = Count;
		T[] items2 = _items;
		if (items2.Length - items.Length - count < 0)
		{
			EnsureCapacity(items2.Length + items.Length);
			items2 = _items;
		}
		items.CopyTo(items2.AsSpan().Slice(count));
		Count += items.Length;
	}

	public void AddRange(ReadOnlySpan<T> items)
	{
		int count = Count;
		T[] items2 = _items;
		if (items2.Length - items.Length - count < 0)
		{
			EnsureCapacity(items2.Length + items.Length);
			items2 = _items;
		}
		items.CopyTo(items2.AsSpan().Slice(count));
		Count += items.Length;
	}

	public void AddRange(T[] items)
	{
		int count = Count;
		T[] items2 = _items;
		if (items2.Length - items.Length - count < 0)
		{
			EnsureCapacity(items2.Length + items.Length);
			items2 = _items;
		}
		Array.Copy(items, 0, items2, count, items.Length);
		Count += items.Length;
	}

	public void AddRange(IEnumerable<T> items)
	{
		int num = Count;
		T[] items2 = _items;
		if (items is ICollection<T> collection)
		{
			if (items2.Length - collection.Count - num < 0)
			{
				EnsureCapacity(items2.Length + collection.Count);
				items2 = _items;
			}
			collection.CopyTo(items2, num);
			Count += collection.Count;
			return;
		}
		foreach (T item in items)
		{
			if (num < items2.Length)
			{
				items2[num] = item;
				num++;
				continue;
			}
			Count = num;
			AddWithResize(item);
			num++;
			items2 = _items;
		}
		Count = num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<T> AsSpan()
	{
		return _items.AsSpan(0, Count);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Memory<T> AsMemory()
	{
		return _items.AsMemory(0, Count);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AddWithResize(T item)
	{
		ArrayPool<T> arrayPool = _arrayPool;
		T[] items = _items;
		T[] array = arrayPool.Rent(items.Length * 2);
		int num = items.Length;
		Array.Copy(items, 0, array, 0, num);
		array[num] = item;
		_items = array;
		Count = num + 1;
		arrayPool.Return(items, _should_clean);
	}

	public void EnsureCapacity(int capacity)
	{
		if (capacity > Capacity)
		{
			ArrayPool<T> arrayPool = _arrayPool;
			T[] array = arrayPool.Rent(capacity);
			T[] items = _items;
			Array.Copy(items, 0, array, 0, items.Length);
			_items = array;
			arrayPool.Return(items, _should_clean);
		}
	}

	public T[] GetRawBuffer()
	{
		return _items;
	}

	public void SetOffsetManually(int offset)
	{
		Count = offset;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Enumerator GetEnumerator()
	{
		return new Enumerator(_items, Count);
	}

	public void Sort()
	{
		Sort(0, Count, null);
	}

	public void Sort(IComparer<T> comparer)
	{
		Sort(0, Count, comparer);
	}

	public void Sort(int index, int count, IComparer<T> comparer)
	{
		Array.Sort(_items, index, count, comparer);
	}

	public void Sort(Comparison<T> comparison)
	{
		Array.Sort(_items, 0, Count, Comparer<T>.Create(comparison));
	}

	public void Reverse()
	{
		Array.Reverse(_items, 0, Count);
	}

	public void Reverse(int index, int count)
	{
		Array.Reverse(_items, index, count);
	}
}
