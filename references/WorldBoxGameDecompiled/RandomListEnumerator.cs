using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct RandomListEnumerator<T> : IEnumerator<T>, IEnumerator, IDisposable, IEnumerable<T>, IEnumerable
{
	private readonly List<T> _source;

	private readonly int _itemsCount;

	private readonly int _maxItems;

	private int _index;

	private readonly int _offset;

	private readonly int Index => (_index + _offset) % _itemsCount;

	public readonly T Current
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[return: MaybeNull]
		get
		{
			return _source[Index];
		}
	}

	readonly T IEnumerator<T>.Current
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[return: MaybeNull]
		get
		{
			return _source[Index];
		}
	}

	readonly object? IEnumerator.Current
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[return: MaybeNull]
		get
		{
			return _source[Index];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RandomListEnumerator(List<T> source)
		: this(source, source.Count)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RandomListEnumerator(List<T> source, int itemsCount)
	{
		_source = source;
		_itemsCount = (_maxItems = itemsCount);
		_index = -1;
		_offset = Randy.randomInt(0, _itemsCount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RandomListEnumerator(List<T> source, int itemsCount, int maxItems)
	{
		_source = source;
		_itemsCount = itemsCount;
		_maxItems = Mathf.Min(maxItems, _itemsCount);
		_index = -1;
		_offset = Randy.randomInt(0, _itemsCount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool MoveNext()
	{
		return ++_index < _maxItems;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Reset()
	{
		_index = -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RandomListEnumerator<T> GetEnumerator()
	{
		return this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this;
	}

	public readonly void Dispose()
	{
	}
}
