using System;
using System.Collections.Generic;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 排行榜共用的虚拟行对象池：只负责“当前可见行”的创建、回收、复用与脏行重绘，
/// 不持有窗口布局、筛选或业务状态。窗口提供内容区、卡片预制件和单行绑定回调。
/// </summary>
internal sealed class MclslVirtualCardList
{
    private readonly Transform _content;
    private readonly RectTransform _contentRect;
    private readonly GameObject _prefab;
    private readonly float _cardHeight;
    private readonly Action<GameObject, MclslRankEntry, int> _bindRow;

    private readonly List<GameObject> _instances = new();
    private readonly Stack<GameObject> _recycled = new();
    private readonly Dictionary<int, GameObject> _byIndex = new();
    private readonly List<int> _recycleScratch = new();

    private IReadOnlyList<MclslRankEntry> _entries;
    private bool _dirty;
    private int _dirtyStart = int.MaxValue;
    private int _dirtyEnd = -1;
    private int _lastStart = int.MaxValue;
    private int _lastEnd = -1;

    internal MclslVirtualCardList(Transform content, RectTransform contentRect, GameObject prefab,
        float cardHeight, Action<GameObject, MclslRankEntry, int> bindRow)
    {
        _content = content;
        _contentRect = contentRect;
        _prefab = prefab;
        _cardHeight = cardHeight;
        _bindRow = bindRow;
    }

    /// <summary>标记某些行数据变化，滚动保持不变时也只重绘受影响区间。</summary>
    internal void MarkDirty(int start, int end)
    {
        if (end < 0) return;
        _dirty = true;
        _dirtyStart = Math.Min(_dirtyStart, start);
        _dirtyEnd = Math.Max(_dirtyEnd, end);
    }

    /// <summary>整体替换数据源并重建可见行（用于筛选/排序完成后）。</summary>
    internal void Rebuild(IReadOnlyList<MclslRankEntry> entries, bool resetScroll, float viewHeight)
    {
        ClearRows();
        _entries = entries;
        if (resetScroll && _contentRect != null) _contentRect.anchoredPosition = Vector2.zero;
        if (_contentRect != null) _contentRect.sizeDelta = new Vector2(0f, Math.Max(1, entries.Count) * _cardHeight);
        _dirty = false; _dirtyStart = int.MaxValue; _dirtyEnd = -1;
        CreateInitialRows(viewHeight);
    }

    /// <summary>滚动时同步可见行；数据脏时只重绘受影响区间。</summary>
    internal void Update(float scrollY, float viewHeight)
    {
        int count = _entries?.Count ?? 0;
        bool dataDirty = _dirty;
        int dirtyStart = _dirtyStart, dirtyEnd = _dirtyEnd;
        if (dataDirty && _contentRect != null)
            _contentRect.sizeDelta = new Vector2(0f, Math.Max(1, count) * _cardHeight);
        _dirty = false; _dirtyStart = int.MaxValue; _dirtyEnd = -1;

        if (count == 0)
        {
            if (_byIndex.Count > 0) ClearRows();
            return;
        }
        if (_content == null || _prefab == null) return;

        int start = Mathf.Max(0, Mathf.FloorToInt(scrollY / _cardHeight) - 1);
        int end = Mathf.Min(Mathf.CeilToInt((scrollY + viewHeight) / _cardHeight) + 1, count - 1);
        if (start == _lastStart && end == _lastEnd && !dataDirty) return;

        _recycleScratch.Clear();
        foreach (KeyValuePair<int, GameObject> pair in _byIndex)
            if (pair.Key < start || pair.Key > end) _recycleScratch.Add(pair.Key);
        for (int i = 0; i < _recycleScratch.Count; i++)
        {
            int index = _recycleScratch[i];
            if (_byIndex.TryGetValue(index, out GameObject card) && card != null)
            {
                _instances.Remove(card);
                card.SetActive(false);
                _recycled.Push(card);
            }
            _byIndex.Remove(index);
        }
        for (int i = start; i <= end; i++)
        {
            if (dataDirty && i >= dirtyStart && i <= dirtyEnd
                && _byIndex.TryGetValue(i, out GameObject existing) && existing != null)
            {
                existing.transform.localPosition = new Vector3(0f, -_cardHeight / 2f - i * _cardHeight);
                _bindRow(existing, _entries[i], i);
            }
            else CreateRow(i);
        }
        _lastStart = start;
        _lastEnd = end;
    }

    /// <summary>回收全部可见行到池中（不销毁），用于清空/换世界后重建。</summary>
    internal void ClearRows()
    {
        for (int i = 0; i < _instances.Count; i++)
        {
            GameObject card = _instances[i];
            if (card == null) continue;
            card.SetActive(false);
            _recycled.Push(card);
        }
        _instances.Clear();
        _byIndex.Clear();
        _recycleScratch.Clear();
        _lastStart = int.MaxValue;
        _lastEnd = -1;
    }

    /// <summary>销毁全部行与池，用于关闭窗口或换世界释放。</summary>
    internal void Dispose()
    {
        for (int i = 0; i < _instances.Count; i++)
            if (_instances[i] != null) UnityEngine.Object.Destroy(_instances[i]);
        _instances.Clear();
        while (_recycled.Count > 0)
        {
            GameObject card = _recycled.Pop();
            if (card != null) UnityEngine.Object.Destroy(card);
        }
        _byIndex.Clear();
        _recycleScratch.Clear();
        _entries = null;
        _dirty = false; _dirtyStart = int.MaxValue; _dirtyEnd = -1;
        _lastStart = int.MaxValue;
        _lastEnd = -1;
    }

    private void CreateInitialRows(float viewHeight)
    {
        int count = _entries?.Count ?? 0;
        if (count == 0) return;
        int visible = Mathf.CeilToInt(Mathf.Max(0f, viewHeight) / _cardHeight) + 2;
        int end = Mathf.Min(visible - 1, count - 1);
        for (int i = 0; i <= end; i++) CreateRow(i);
        _lastStart = 0;
        _lastEnd = end;
    }

    private void CreateRow(int index)
    {
        if (_entries == null || index < 0 || index >= _entries.Count || _byIndex.ContainsKey(index)) return;
        GameObject card = null;
        while (_recycled.Count > 0 && card == null) card = _recycled.Pop();
        if (card == null)
        {
            if (_content == null || _prefab == null) return;
            card = UnityEngine.Object.Instantiate(_prefab, _content);
        }
        else card.transform.SetParent(_content, false);
        if (card == null) return;
        card.SetActive(true);
        card.transform.localPosition = new Vector3(0f, -_cardHeight / 2f - index * _cardHeight);
        _instances.Add(card);
        _byIndex[index] = card;
        _bindRow(card, _entries[index], index);
    }
}
