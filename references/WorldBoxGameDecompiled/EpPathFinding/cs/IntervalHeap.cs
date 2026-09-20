using System;
using System.Collections;
using System.Collections.Generic;
using C5;

namespace EpPathFinding.cs;

public class IntervalHeap<T> : CollectionValueBase<T>, IPriorityQueue<T>, IExtensible<T>, ICollectionValue<T>, IEnumerable<T>, IEnumerable, IShowable, IFormattable where T : class
{
	private struct Interval
	{
		internal T first;

		internal T last;

		public override string ToString()
		{
			return $"[{first}; {last}]";
		}
	}

	private class Handle : IPriorityQueueHandle<T>
	{
		internal int index = -1;

		public override string ToString()
		{
			return $"[{index}]";
		}
	}

	private int stamp;

	private readonly IComparer<T> comparer;

	private readonly IEqualityComparer<T> itemequalityComparer;

	private Interval[] heap;

	private int size;

	public override EventTypeEnum ListenableEvents => (EventTypeEnum)15;

	public IComparer<T> Comparer => comparer;

	public bool IsReadOnly => false;

	public bool AllowsDuplicates => true;

	public virtual IEqualityComparer<T> EqualityComparer => itemequalityComparer;

	public virtual bool DuplicatesByCounting => false;

	public override bool IsEmpty => size == 0;

	public override int Count => size;

	public override Speed CountSpeed => (Speed)4;

	public T this[IPriorityQueueHandle<T> handle]
	{
		get
		{
			CheckHandle(handle, out var cell, out var isfirst);
			if (!isfirst)
			{
				return heap[cell].last;
			}
			return heap[cell].first;
		}
		set
		{
			Replace(handle, value);
		}
	}

	private void SwapFirstWithLast(int cell1, int cell2)
	{
		T first = heap[cell1].first;
		UpdateFirst(cell1, heap[cell2].last);
		UpdateLast(cell2, first);
	}

	private void SwapLastWithLast(int cell1, int cell2)
	{
		T last = heap[cell2].last;
		UpdateLast(cell2, heap[cell1].last);
		UpdateLast(cell1, last);
	}

	private void SwapFirstWithFirst(int cell1, int cell2)
	{
		T first = heap[cell2].first;
		UpdateFirst(cell2, heap[cell1].first);
		UpdateFirst(cell1, first);
	}

	private bool HeapifyMin(int cell)
	{
		bool result = false;
		if (2 * cell + 1 < size && comparer.Compare(heap[cell].first, heap[cell].last) > 0)
		{
			result = true;
			SwapFirstWithLast(cell, cell);
		}
		int num = cell;
		int num2 = 2 * cell + 1;
		int num3 = num2 + 1;
		if (2 * num2 < size && comparer.Compare(heap[num2].first, heap[num].first) < 0)
		{
			num = num2;
		}
		if (2 * num3 < size && comparer.Compare(heap[num3].first, heap[num].first) < 0)
		{
			num = num3;
		}
		if (num != cell)
		{
			SwapFirstWithFirst(num, cell);
			HeapifyMin(num);
		}
		return result;
	}

	private bool HeapifyMax(int cell)
	{
		bool result = false;
		if (2 * cell + 1 < size && comparer.Compare(heap[cell].last, heap[cell].first) < 0)
		{
			result = true;
			SwapFirstWithLast(cell, cell);
		}
		int num = cell;
		int num2 = 2 * cell + 1;
		int num3 = num2 + 1;
		bool flag = false;
		if (2 * num2 + 1 < size)
		{
			if (comparer.Compare(heap[num2].last, heap[num].last) > 0)
			{
				num = num2;
			}
		}
		else if (2 * num2 + 1 == size && comparer.Compare(heap[num2].first, heap[num].last) > 0)
		{
			num = num2;
			flag = true;
		}
		if (2 * num3 + 1 < size)
		{
			if (comparer.Compare(heap[num3].last, heap[num].last) > 0)
			{
				num = num3;
			}
		}
		else if (2 * num3 + 1 == size && comparer.Compare(heap[num3].first, heap[num].last) > 0)
		{
			num = num3;
			flag = true;
		}
		if (num != cell)
		{
			if (flag)
			{
				SwapFirstWithLast(num, cell);
			}
			else
			{
				SwapLastWithLast(num, cell);
			}
			HeapifyMax(num);
		}
		return result;
	}

	private void BubbleUpMin(int i)
	{
		if (i > 0)
		{
			T first = heap[i].first;
			T val = first;
			_ = (i + 1) / 2;
			int num;
			while (i > 0 && comparer.Compare(val, first = heap[num = (i + 1) / 2 - 1].first) < 0)
			{
				UpdateFirst(i, first);
				i = num;
			}
			UpdateFirst(i, val);
		}
	}

	private void BubbleUpMax(int i)
	{
		if (i > 0)
		{
			T last = heap[i].last;
			T val = last;
			_ = (i + 1) / 2;
			int num;
			while (i > 0 && comparer.Compare(val, last = heap[num = (i + 1) / 2 - 1].last) > 0)
			{
				UpdateLast(i, last);
				i = num;
			}
			UpdateLast(i, val);
		}
	}

	public IntervalHeap()
		: this(16)
	{
	}

	public IntervalHeap(int capacity)
		: this(capacity, (IComparer<T>)Comparer<T>.Default, EqualityComparer<T>.Default)
	{
	}

	private IntervalHeap(int capacity, IComparer<T> comparer, IEqualityComparer<T> itemequalityComparer)
	{
		this.comparer = comparer ?? throw new NullReferenceException("Item comparer cannot be null");
		this.itemequalityComparer = itemequalityComparer ?? throw new NullReferenceException("Item equality comparer cannot be null");
		int num;
		for (num = 1; num < capacity; num <<= 1)
		{
		}
		heap = new Interval[num];
	}

	public T FindMin()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (size == 0)
		{
			throw new NoSuchItemException();
		}
		return heap[0].first;
	}

	public T DeleteMin()
	{
		IPriorityQueueHandle<T> handle;
		return DeleteMin(out handle);
	}

	public T FindMax()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (size == 0)
		{
			throw new NoSuchItemException("Heap is empty");
		}
		if (size == 1)
		{
			return heap[0].first;
		}
		return heap[0].last;
	}

	public T DeleteMax()
	{
		IPriorityQueueHandle<T> handle;
		return DeleteMax(out handle);
	}

	public void Clear()
	{
		stamp++;
		if (size != 0)
		{
			int num = ((size % 2 == 0) ? (size / 2) : (size / 2 + 1));
			Interval[] array = heap;
			for (int i = 0; i < num; i++)
			{
				array[i].first = null;
				array[i].last = null;
			}
			size = 0;
		}
	}

	public bool Add(T item)
	{
		stamp++;
		if (add(item))
		{
			((CollectionValueBase<T>)this).raiseItemsAdded(item, 1);
			((CollectionValueBase<T>)this).raiseCollectionChanged();
			return true;
		}
		return false;
	}

	private bool add(T item)
	{
		if (size == 0)
		{
			size = 1;
			UpdateFirst(0, item);
			return true;
		}
		if (size == 2 * heap.Length)
		{
			Interval[] destinationArray = new Interval[2 * heap.Length];
			Array.Copy(heap, destinationArray, heap.Length);
			heap = destinationArray;
		}
		if (size % 2 == 0)
		{
			int num = size / 2;
			int num2 = (num + 1) / 2 - 1;
			T last = heap[num2].last;
			if (comparer.Compare(item, last) > 0)
			{
				UpdateFirst(num, last);
				UpdateLast(num2, item);
				BubbleUpMax(num2);
			}
			else
			{
				UpdateFirst(num, item);
				if (comparer.Compare(item, heap[num2].first) < 0)
				{
					BubbleUpMin(num);
				}
			}
		}
		else
		{
			int num3 = size / 2;
			T first = heap[num3].first;
			if (comparer.Compare(item, first) < 0)
			{
				UpdateLast(num3, first);
				UpdateFirst(num3, item);
				BubbleUpMin(num3);
			}
			else
			{
				UpdateLast(num3, item);
				BubbleUpMax(num3);
			}
		}
		size++;
		return true;
	}

	private void UpdateLast(int cell, T item)
	{
		heap[cell].last = item;
	}

	private void UpdateFirst(int cell, T item)
	{
		heap[cell].first = item;
	}

	public void AddAll(IEnumerable<T> items)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		stamp++;
		int num = size;
		foreach (T item in items)
		{
			add(item);
		}
		if (size == num)
		{
			return;
		}
		if ((((CollectionValueBase<T>)this).ActiveEvents & 4) != 0)
		{
			foreach (T item2 in items)
			{
				((CollectionValueBase<T>)this).raiseItemsAdded(item2, 1);
			}
		}
		((CollectionValueBase<T>)this).raiseCollectionChanged();
	}

	public override T Choose()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (size == 0)
		{
			throw new NoSuchItemException("Collection is empty");
		}
		return heap[0].first;
	}

	public override IEnumerator<T> GetEnumerator()
	{
		int mystamp = stamp;
		for (int i = 0; i < size; i++)
		{
			if (mystamp != stamp)
			{
				throw new CollectionModifiedException();
			}
			yield return (i % 2 == 0) ? heap[i >> 1].first : heap[i >> 1].last;
		}
	}

	private bool Check(int i, T min, T max)
	{
		bool flag = true;
		Interval interval = heap[i];
		T first = interval.first;
		T last = interval.last;
		if (2 * i + 1 == size)
		{
			if (comparer.Compare(min, first) > 0)
			{
				Logger.Log($"Cell {i}: parent.first({min}) > first({first})  [size={size}]");
				flag = false;
			}
			if (comparer.Compare(first, max) > 0)
			{
				Logger.Log($"Cell {i}: first({first}) > parent.last({max})  [size={size}]");
				flag = false;
			}
			return flag;
		}
		if (comparer.Compare(min, first) > 0)
		{
			Logger.Log($"Cell {i}: parent.first({min}) > first({first})  [size={size}]");
			flag = false;
		}
		if (comparer.Compare(first, last) > 0)
		{
			Logger.Log($"Cell {i}: first({first}) > last({last})  [size={size}]");
			flag = false;
		}
		if (comparer.Compare(last, max) > 0)
		{
			Logger.Log($"Cell {i}: last({last}) > parent.last({max})  [size={size}]");
			flag = false;
		}
		int num = 2 * i + 1;
		int num2 = num + 1;
		if (2 * num < size)
		{
			flag = flag && Check(num, first, last);
		}
		if (2 * num2 < size)
		{
			flag = flag && Check(num2, first, last);
		}
		return flag;
	}

	public bool Check()
	{
		if (size == 0)
		{
			return true;
		}
		if (size == 1)
		{
			return heap[0].first != null;
		}
		return Check(0, heap[0].first, heap[0].last);
	}

	public bool Find(IPriorityQueueHandle<T> handle, out T item)
	{
		if (!(handle is Handle { index: var index } handle2))
		{
			item = null;
			return false;
		}
		int num = index / 2;
		bool flag = index % 2 == 0;
		if (index == -1 || index >= size)
		{
			item = null;
			return false;
		}
		if (null != handle2)
		{
			item = null;
			return false;
		}
		item = (flag ? heap[num].first : heap[num].last);
		return true;
	}

	public bool Add(ref IPriorityQueueHandle<T> handle, T item)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		stamp++;
		Handle handle2 = (Handle)handle;
		if (handle2 == null)
		{
			handle2 = (Handle)(handle = new Handle());
		}
		else if (handle2.index != -1)
		{
			throw new InvalidPriorityQueueHandleException("Handle not valid for reuse");
		}
		if (add(item))
		{
			((CollectionValueBase<T>)this).raiseItemsAdded(item, 1);
			((CollectionValueBase<T>)this).raiseCollectionChanged();
			return true;
		}
		return false;
	}

	public T Delete(IPriorityQueueHandle<T> handle)
	{
		stamp++;
		CheckHandle(handle, out var cell, out var isfirst).index = -1;
		int num = (size - 1) / 2;
		T val;
		if (cell == num)
		{
			if (isfirst)
			{
				val = heap[cell].first;
				if (size % 2 == 0)
				{
					UpdateFirst(cell, heap[cell].last);
					heap[cell].last = null;
				}
				else
				{
					heap[cell].first = null;
				}
			}
			else
			{
				val = heap[cell].last;
				heap[cell].last = null;
			}
			size--;
		}
		else if (isfirst)
		{
			val = heap[cell].first;
			if (size % 2 == 0)
			{
				UpdateFirst(cell, heap[num].last);
				heap[num].last = null;
			}
			else
			{
				UpdateFirst(cell, heap[num].first);
				heap[num].first = null;
			}
			size--;
			if (HeapifyMin(cell))
			{
				BubbleUpMax(cell);
			}
			else
			{
				BubbleUpMin(cell);
			}
		}
		else
		{
			val = heap[cell].last;
			if (size % 2 == 0)
			{
				UpdateLast(cell, heap[num].last);
				heap[num].last = null;
			}
			else
			{
				UpdateLast(cell, heap[num].first);
				heap[num].first = null;
			}
			size--;
			if (HeapifyMax(cell))
			{
				BubbleUpMin(cell);
			}
			else
			{
				BubbleUpMax(cell);
			}
		}
		((CollectionValueBase<T>)this).raiseItemsRemoved(val, 1);
		((CollectionValueBase<T>)this).raiseCollectionChanged();
		return val;
	}

	private Handle CheckHandle(IPriorityQueueHandle<T> handle, out int cell, out bool isfirst)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Handle handle2 = (Handle)handle;
		int index = handle2.index;
		cell = index / 2;
		isfirst = index % 2 == 0;
		if (index == -1 || index >= size)
		{
			throw new InvalidPriorityQueueHandleException("Invalid handle, index out of range");
		}
		if (null != handle2)
		{
			throw new InvalidPriorityQueueHandleException("Invalid handle, doesn't match queue");
		}
		return handle2;
	}

	public T Replace(IPriorityQueueHandle<T> handle, T item)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		stamp++;
		CheckHandle(handle, out var cell, out var isfirst);
		if (size == 0)
		{
			throw new NoSuchItemException();
		}
		T val;
		if (isfirst)
		{
			val = heap[cell].first;
			heap[cell].first = item;
			if (size != 1)
			{
				if (size == 2 * cell + 1)
				{
					int num = (cell + 1) / 2 - 1;
					if (comparer.Compare(item, heap[num].last) > 0)
					{
						UpdateFirst(cell, heap[num].last);
						UpdateLast(num, item);
						BubbleUpMax(num);
					}
					else
					{
						BubbleUpMin(cell);
					}
				}
				else if (HeapifyMin(cell))
				{
					BubbleUpMax(cell);
				}
				else
				{
					BubbleUpMin(cell);
				}
			}
		}
		else
		{
			val = heap[cell].last;
			heap[cell].last = item;
			if (HeapifyMax(cell))
			{
				BubbleUpMin(cell);
			}
			else
			{
				BubbleUpMax(cell);
			}
		}
		((CollectionValueBase<T>)this).raiseItemsRemoved(val, 1);
		((CollectionValueBase<T>)this).raiseItemsAdded(item, 1);
		((CollectionValueBase<T>)this).raiseCollectionChanged();
		return val;
	}

	public T FindMin(out IPriorityQueueHandle<T> handle)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (size == 0)
		{
			throw new NoSuchItemException();
		}
		handle = null;
		return heap[0].first;
	}

	public T FindMax(out IPriorityQueueHandle<T> handle)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (size == 0)
		{
			throw new NoSuchItemException();
		}
		if (size == 1)
		{
			handle = null;
			return heap[0].first;
		}
		handle = null;
		return heap[0].last;
	}

	public T DeleteMin(out IPriorityQueueHandle<T> handle)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		stamp++;
		if (size == 0)
		{
			throw new NoSuchItemException();
		}
		T first = heap[0].first;
		handle = null;
		if (size == 1)
		{
			size = 0;
			heap[0].first = null;
		}
		else
		{
			int num = (size - 1) / 2;
			if (size % 2 == 0)
			{
				UpdateFirst(0, heap[num].last);
				heap[num].last = null;
			}
			else
			{
				UpdateFirst(0, heap[num].first);
				heap[num].first = null;
			}
			size--;
			HeapifyMin(0);
		}
		((CollectionValueBase<T>)this).raiseItemsRemoved(first, 1);
		((CollectionValueBase<T>)this).raiseCollectionChanged();
		return first;
	}

	public T DeleteMax(out IPriorityQueueHandle<T> handle)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		stamp++;
		if (size == 0)
		{
			throw new NoSuchItemException();
		}
		handle = null;
		T val;
		if (size == 1)
		{
			size = 0;
			val = heap[0].first;
			heap[0].first = null;
		}
		else
		{
			val = heap[0].last;
			int num = (size - 1) / 2;
			if (size % 2 == 0)
			{
				UpdateLast(0, heap[num].last);
				heap[num].last = null;
			}
			else
			{
				UpdateLast(0, heap[num].first);
				heap[num].first = null;
			}
			size--;
			HeapifyMax(0);
		}
		((CollectionValueBase<T>)this).raiseItemsRemoved(val, 1);
		((CollectionValueBase<T>)this).raiseCollectionChanged();
		return val;
	}
}
