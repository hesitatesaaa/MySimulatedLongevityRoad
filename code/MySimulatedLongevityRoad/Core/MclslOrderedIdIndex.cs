using System;
using System.Collections;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Core;

/// <summary>Order statistics treap: updates and indexed reads are expected O(log population).</summary>
internal sealed class MclslOrderedIdIndex<T> : IReadOnlyList<T>
{
    private sealed class Node
    {
        internal long Id;
        internal uint Priority;
        internal T Value;
        internal Node Left, Right;
        internal int Size = 1;
    }
    private readonly Dictionary<long, Node> _nodes = new();
    private readonly Comparison<T> _compare;
    private Node _root;
    internal MclslOrderedIdIndex(Comparison<T> compare) => _compare = compare;
    public int Count => Size(_root);
    internal int IndexOf(long id)
    {
        if (!_nodes.TryGetValue(id, out Node wanted)) return -1;
        int offset = 0;
        Node node = _root;
        while (node != null)
        {
            int order = Compare(wanted, node);
            if (order == 0) return offset + Size(node.Left);
            if (order < 0) node = node.Left;
            else { offset += Size(node.Left) + 1; node = node.Right; }
        }
        return -1;
    }
    private static int Size(Node node) => node?.Size ?? 0;
    private static void Refresh(Node node) => node.Size = 1 + Size(node.Left) + Size(node.Right);
    private int Compare(Node a, Node b)
    {
        int order = _compare(a.Value, b.Value);
        return order != 0 ? order : a.Id.CompareTo(b.Id);
    }
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            Node node = _root;
            while (node != null)
            {
                int left = Size(node.Left);
                if (index == left) return node.Value;
                if (index < left) node = node.Left;
                else { index -= left + 1; node = node.Right; }
            }
            throw new InvalidOperationException("Invalid rank index");
        }
    }
    internal void Upsert(long id, T value)
    {
        if (_nodes.TryGetValue(id, out Node node))
        {
            _root = Delete(_root, node);
            node.Left = node.Right = null; node.Size = 1; node.Value = value;
        }
        else
        {
            node = new Node { Id = id, Value = value, Priority = Priority(id) };
            _nodes.Add(id, node);
        }
        _root = Insert(_root, node);
    }
    internal void Remove(long id)
    {
        if (!_nodes.TryGetValue(id, out Node node)) return;
        _root = Delete(_root, node); _nodes.Remove(id);
    }
    private Node Insert(Node root, Node node)
    {
        if (root == null) return node;
        if (Compare(node, root) < 0)
        {
            root.Left = Insert(root.Left, node);
            if (root.Left.Priority < root.Priority)
            {
                Node next = root.Left; root.Left = next.Right; next.Right = root;
                Refresh(root); Refresh(next); return next;
            }
        }
        else
        {
            root.Right = Insert(root.Right, node);
            if (root.Right.Priority < root.Priority)
            {
                Node next = root.Right; root.Right = next.Left; next.Left = root;
                Refresh(root); Refresh(next); return next;
            }
        }
        Refresh(root); return root;
    }
    private Node Delete(Node root, Node node)
    {
        if (root == null) return null;
        int order = Compare(node, root);
        if (order == 0) return Merge(root.Left, root.Right);
        if (order < 0) root.Left = Delete(root.Left, node);
        else root.Right = Delete(root.Right, node);
        Refresh(root); return root;
    }
    private static Node Merge(Node left, Node right)
    {
        if (left == null) return right;
        if (right == null) return left;
        if (left.Priority < right.Priority)
        { left.Right = Merge(left.Right, right); Refresh(left); return left; }
        right.Left = Merge(left, right.Left); Refresh(right); return right;
    }
    private static uint Priority(long id)
    {
        unchecked
        {
            ulong x = (ulong)id + 0x9e3779b97f4a7c15UL;
            x = (x ^ (x >> 30)) * 0xbf58476d1ce4e5b9UL;
            x = (x ^ (x >> 27)) * 0x94d049bb133111ebUL;
            return (uint)(x ^ (x >> 31));
        }
    }
    internal void Clear() { _root = null; _nodes.Clear(); }
    public IEnumerator<T> GetEnumerator()
    { for (int i = 0; i < Count; i++) yield return this[i]; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class MclslOrderedChangeRange
{
    internal static (int Start, int End) Affected(int oldIndex, int newIndex)
    {
        if (oldIndex < 0 && newIndex < 0) return (int.MaxValue, -1);
        int start = oldIndex < 0 ? newIndex : newIndex < 0 ? oldIndex : Math.Min(oldIndex, newIndex);
        int end = oldIndex < 0 || newIndex < 0 ? int.MaxValue : Math.Max(oldIndex, newIndex);
        return (start, end);
    }
}
