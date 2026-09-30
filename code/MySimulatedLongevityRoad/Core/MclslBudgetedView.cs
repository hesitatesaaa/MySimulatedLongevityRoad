using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MySimulatedLongevityRoad.Core;

/// <summary>Visible archive projections: stable merge sort, bounded work and pooled scratch.</summary>
internal static class MclslBudgetedView
{
    // NML compiles source against Unity's mscorlib and System.Buffers together.
    // Keep the scratch pool local to avoid their ambiguous ArrayPool<T> type.
    private static class ScratchPool<T>
    {
        private static readonly Stack<T[]> Available = new();

        internal static T[] Rent(int count)
        {
            lock (Available)
            {
                while (Available.Count > 0)
                {
                    T[] array = Available.Pop();
                    if (array.Length >= count) return array;
                }
            }
            return new T[count];
        }

        internal static void Return(T[] array)
        {
            if (array == null) return;
            Array.Clear(array, 0, array.Length);
            if (array.Length > 32768) return;
            lock (Available)
                if (Available.Count < 4) Available.Push(array);
        }
    }

    internal static int Compare<T>(T left, T right, bool descending = false, IComparer<T> comparer = null)
        => descending ? (comparer ?? Comparer<T>.Default).Compare(right, left)
            : (comparer ?? Comparer<T>.Default).Compare(left, right);
    internal static IEnumerator Sort<T>(IReadOnlyList<T> source, List<T> destination,
        Comparison<T> compare, Func<T, bool> filter = null, int take = int.MaxValue)
    {
        destination.Clear();
        if (source == null || source.Count == 0 || take <= 0) yield break;
        int count = source.Count, length = 0, operations = 0;
        T[] values = ScratchPool<T>.Rent(count), scratch = ScratchPool<T>.Rent(count);
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 2500;
        try
        {
            for (int i = 0; i < count && i < source.Count; i++)
            {
                T value = source[i];
                if (filter == null || filter(value)) values[length++] = value;
                if (++operations >= 64 || Stopwatch.GetTimestamp() >= deadline)
                { yield return null; operations = 0; deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 2500; }
            }
            for (int width = 1; width < length; width *= 2)
            {
                for (int start = 0; start < length; start += width * 2)
                {
                    int left = start, middle = Math.Min(start + width, length), right = middle;
                    int end = Math.Min(start + width * 2, length);
                    for (int at = start; at < end; at++)
                    {
                        scratch[at] = left < middle && (right >= end || compare(values[left], values[right]) <= 0)
                            ? values[left++] : values[right++];
                        if (++operations >= 64 || Stopwatch.GetTimestamp() >= deadline)
                        { yield return null; operations = 0; deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 2500; }
                    }
                }
                T[] swap = values; values = scratch; scratch = swap;
            }
            int visible = Math.Min(length, take);
            for (int i = 0; i < visible; i++)
            {
                destination.Add(values[i]);
                if (++operations >= 64 || Stopwatch.GetTimestamp() >= deadline)
                { yield return null; operations = 0; deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 2500; }
            }
        }
        finally
        {
            ScratchPool<T>.Return(values);
            ScratchPool<T>.Return(scratch);
        }
    }
}
