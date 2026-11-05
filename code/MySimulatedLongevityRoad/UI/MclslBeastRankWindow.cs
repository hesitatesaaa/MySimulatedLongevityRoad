using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

// 神兽榜：只收录本世 21 种普通妖兽（含元婴化形，不含具名妖兽）。
// 与玄黄榜共用 MclslRankSnapshotSource 快照、MclslOrderedIdIndex 排序索引、
// UiUnitAvatarElement 头像与 MclslRankTooltipTrigger 提示等既有组件；
// 但拥有完全独立的窗口、筛选、可见行回收状态，以及玄黑暗红古金独立配色。
internal static class MclslBeastRankWindow
{
    private const string WindowId = "MclslBeastRank";
    private const string FrameArtName = "神兽榜框图";
    // frame_beast_ranking.png 为 1536x1024，窗口按 3:2 与美术对齐。
    private const float WindowWidth = 720f;
    private const float WindowHeight = 480f;
    private const float CardWidth = WindowWidth - 48f;
    private const float CardHeight = 34f;
    private const float ListTop = 152f;
    private const float ListBottom = 70f;
    private const float ListHeight = WindowHeight - ListTop - ListBottom;

    private const float RankColumnX = 10f;
    private const float RankColumnWidth = 32f;
    private const float AvatarX = 48f;
    private const float NameColumnX = 77f;
    private const float NameColumnWidth = 143f;
    private const float SpeciesColumnX = 224f;
    private const float SpeciesColumnWidth = 104f;
    private const float RealmColumnX = 331f;
    private const float RealmColumnWidth = 83f;
    private const float PowerColumnX = 418f;
    private const float PowerColumnWidth = 104f;
    private const float TalentColumnX = 527f;
    private const float TalentColumnWidth = 129f;

    private static readonly string[] RealmLabels = { "全部境界", "炼气", "筑基", "金丹", "元婴", "化神", "合道", "长生" };
    private static readonly string[] SortLabels = { "战力（降序）", "境界（降序）", "神通等级（降序）" };
    private static string[] SpeciesLabels = { "全部物种" };
    private static string[] SpeciesIds = Array.Empty<string>();

    // _compare 必须先于 Entries 初始化，静态字段按声明顺序执行。
    private static Comparison<MclslRankEntry> _compare = CompareByPower;
    private static MclslOrderedIdIndex<MclslRankEntry> Entries = new(_compare);
    private static MclslOrderedIdIndex<MclslRankEntry> NextEntries;
    private static MclslVirtualCardList _rows;

    private static ScrollWindow _window;
    internal static bool IsVisible => _window != null && _window.gameObject.activeInHierarchy;
    private static RectTransform _contentRect;
    private static RectTransform _scrollViewRect;
    private static ScrollRect _rankScroll;
    private static Transform _contentTransform;
    private static GameObject _emptyState;
    private static GameObject _cardPrefab;
    private static Text _countText;
    private static Dropdown _speciesDropdown;
    private static Dropdown _realmDropdown;
    private static Dropdown _sortDropdown;
    private static InputField _searchInput;

    private static int _speciesFilter;
    private static int _realmFilter;
    private static int _sortMode;
    private static string _searchQuery = string.Empty;
    private static bool _needRefresh;
    private static bool _viewDataDirty;
    private static int _filterCursor = -1;
    private static bool _resetPendingScroll;
    private static string _pendingQuery;

    internal static void ShowWindow()
    {
        MclslRankSnapshotSource.RequestVisibleUpdate();
        bool created = EnsureWindow();
        if (_window == null) return;
        if (created) _needRefresh = true;
        MclslWindowOpenGuard.Show(_window, WindowId, created);
        ApplyWindowScale();
    }

    private static bool EnsureWindow()
    {
        if (_window != null) return false;
        MclslLocalizationBridge.RegisterKey(WindowId, "神兽榜");
        // 正式美术已按约定落盘：直接引用 ui/Icons/ShenShouBang.png，不做旧榜图标回退。
        _window = WindowCreator.CreateEmptyWindow(WindowId, WindowId, "ui/Icons/ShenShouBang");
        if (_window == null) return false;
        BuildSpeciesOptions();
        SetupWindowContent();
        RepositionNativeCloseButton();
        if (_window.titleText != null) _window.titleText.gameObject.SetActive(false);
        _cardPrefab = CreateCardPrefab();
        _cardPrefab.transform.SetParent(_window.transform, false);
        _rows = new MclslVirtualCardList(_contentTransform, _contentRect, _cardPrefab, CardHeight, BindRow);
        MclslRankWindowUpdater updater = _window.gameObject.AddComponent<MclslRankWindowUpdater>();
        updater.OnOpen = () => { ApplyWindowScale(); _needRefresh = true; };
        updater.OnUpdate = () => { ApplyWindowScale(); UpdateVisibleCards(); };
        updater.OnClose = () =>
        {
            _filterCursor = -1; NextEntries = null; Entries.Clear(); _rows?.Dispose();
        };
        return true;
    }

    private static void RepositionNativeCloseButton()
    {
        if (_window == null) return;
        if (_window.close_background != null)
        {
            RectTransform closeRect = _window.close_background.rectTransform;
            closeRect.SetParent(_window.transform.Find("Background") ?? _window.transform, false);
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = new Vector2(-20f, -20f);
            closeRect.sizeDelta = new Vector2(26f, 26f);
            closeRect.SetAsLastSibling();
        }
        if (_window._close_button_tip != null)
        {
            RectTransform tipRect = _window._close_button_tip.GetComponent<RectTransform>();
            if (tipRect != null)
            {
                tipRect.SetParent(_window.transform.Find("Background") ?? _window.transform, false);
                tipRect.anchorMin = tipRect.anchorMax = new Vector2(1f, 1f);
                tipRect.pivot = new Vector2(0.5f, 0.5f);
                tipRect.anchoredPosition = new Vector2(-20f, -20f);
                tipRect.sizeDelta = new Vector2(26f, 26f);
                tipRect.SetAsLastSibling();
            }
        }
    }

    private static void ApplyWindowScale()
    {
        if (_window == null) return;
        Canvas canvas = _window.GetComponentInParent<Canvas>();
        float canvasScale = canvas != null && canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
        float usableWidth = Mathf.Max(1f, Screen.width / canvasScale - 24f);
        float usableHeight = Mathf.Max(1f, Screen.height / canvasScale - 12f);
        float scale = Mathf.Min(1.05f, usableWidth / WindowWidth, usableHeight / WindowHeight);
        _window.transform.localScale = Vector3.one * scale;
    }

    private static void SetupWindowContent()
    {
        RectTransform windowRect = _window.GetComponent<RectTransform>();
        if (windowRect != null) windowRect.sizeDelta = new Vector2(WindowWidth, WindowHeight);
        Transform background = _window.transform.Find("Background");
        if (background == null) return;
        Image nativeBackground = background.GetComponent<Image>();
        if (nativeBackground != null) nativeBackground.color = Color.clear;
        RectTransform bgRect = background.GetComponent<RectTransform>();
        if (bgRect != null) bgRect.sizeDelta = new Vector2(WindowWidth, WindowHeight);
        CreateBackplate(background);
        CreateTitle(background);
        CreateTopBar(background);
        CreateTableHeader(background);
        CreateList(background);
        CreateFooter(background);
        CreateEmptyState(background);
    }

    private static void CreateBackplate(Transform parent)
    {
        GameObject surface = new("神兽榜卷面", typeof(RectTransform), typeof(Image));
        surface.transform.SetParent(parent, false);
        SetRect(surface, Vector2.zero, Vector2.one, offsetMin: new Vector2(16f, 20f), offsetMax: new Vector2(-16f, -20f));
        Image surfaceImage = surface.GetComponent<Image>();
        surfaceImage.sprite = null;
        surfaceImage.color = MclslUiTheme.BeastRankSurfaceWindow;
        surfaceImage.raycastTarget = false;

        GameObject art = new(FrameArtName, typeof(RectTransform), typeof(Image));
        art.transform.SetParent(parent, false);
        // 1536x1024 美术铺满 720x480 窗口，比例一致不拉伸。
        SetRect(art, Vector2.zero, Vector2.one, offsetMin: Vector2.zero, offsetMax: Vector2.zero);
        XianLuUIResources.Apply(art.GetComponent<Image>(), "frame_beast_ranking", Color.clear);
    }

    private static void CreateTitle(Transform parent)
    {
        // 标题艺术字 1983x793，按约 80 高显示，保持原比例不被压扁；
        // 标题只占顶部一条，其下方留给筛选栏。
        GameObject titleImage = new("兽榜题名", typeof(RectTransform), typeof(Image));
        titleImage.transform.SetParent(parent, false);
        SetRect(titleImage, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -8f), new Vector2(200f, 80f));
        Image titleGraphic = titleImage.GetComponent<Image>();
        titleGraphic.raycastTarget = false;
        XianLuUIResources.Apply(titleGraphic, "title_beast_ranking", Color.clear);
    }

    private static void CreateTopBar(Transform parent)
    {
        GameObject topBar = new("兽榜顶栏", typeof(RectTransform));
        topBar.transform.SetParent(parent, false);
        SetRect(topBar, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -106f), new Vector2(CardWidth, 24f));

        _searchInput = CreateSearchInput(topBar.transform, new Vector2(0f, 0f), new Vector2(224f, 24f));

        _speciesDropdown = CreateDropdown("SpeciesDropdown", topBar.transform, SpeciesLabels, index =>
        {
            _speciesFilter = index;
            RefreshCurrentList(true);
        });
        SetRect(_speciesDropdown.gameObject, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(232f, 0f), new Vector2(135f, 24f));

        _realmDropdown = CreateDropdown("BeastRealmDropdown", topBar.transform, RealmLabels, index =>
        {
            _realmFilter = index;
            RefreshCurrentList(true);
        });
        SetRect(_realmDropdown.gameObject, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(375f, 0f), new Vector2(116f, 24f));

        _sortDropdown = CreateDropdown("BeastSortDropdown", topBar.transform, SortLabels, index =>
        {
            _sortMode = index;
            RefreshSortOrder();
            RefreshCurrentList(true);
        });
        SetRect(_sortDropdown.gameObject, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(498f, 0f), new Vector2(174f, 24f));
    }

    private static void CreateTableHeader(Transform parent)
    {
        GameObject header = new("兽榜列名", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(parent, false);
        SetRect(header, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -130f), new Vector2(CardWidth, 20f));
        header.GetComponent<Image>().color = MclslUiTheme.BeastRankHeader;
        CreateHeaderLabel(header.transform, "名次", RankColumnX, RankColumnWidth, TextAnchor.MiddleCenter);
        CreateHeaderLabel(header.transform, "名称", NameColumnX, NameColumnWidth, TextAnchor.MiddleLeft);
        CreateHeaderLabel(header.transform, "物种", SpeciesColumnX, SpeciesColumnWidth, TextAnchor.MiddleLeft);
        CreateHeaderLabel(header.transform, "境界", RealmColumnX, RealmColumnWidth, TextAnchor.MiddleCenter);
        CreateHeaderLabel(header.transform, "战力", PowerColumnX, PowerColumnWidth, TextAnchor.MiddleCenter);
        CreateHeaderLabel(header.transform, "神通等级", TalentColumnX, TalentColumnWidth, TextAnchor.MiddleCenter);
    }

    private static Text CreateHeaderLabel(Transform parent, string value, float x, float width, TextAnchor alignment)
    {
        Text label = CreateText("列_" + value, parent, value, 8, MclslUiTheme.BeastRankAccentGold);
        label.alignment = alignment;
        label.fontStyle = FontStyle.Bold;
        SetRect(label.gameObject, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(x, 0f), new Vector2(width, 18f));
        ConfineText(label, 7);
        return label;
    }

    private static void CreateList(Transform parent)
    {
        GameObject scrollObj = new("兽榜滚动", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);
        _scrollViewRect = SetRect(scrollObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -(ListTop + ListHeight / 2f)), new Vector2(CardWidth, ListHeight));
        Image scrollBg = scrollObj.GetComponent<Image>();
        scrollBg.color = MclslUiTheme.BeastRankSurfaceScroll;
        scrollObj.GetComponent<Mask>().showMaskGraphic = true;
        _rankScroll = scrollObj.GetComponent<ScrollRect>();
        _rankScroll.horizontal = false;
        _rankScroll.vertical = true;
        _rankScroll.movementType = ScrollRect.MovementType.Clamped;
        _rankScroll.inertia = false;
        _rankScroll.scrollSensitivity = 22f;

        GameObject viewport = new("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRect = SetRect(viewport, Vector2.zero, Vector2.one, offsetMin: Vector2.zero, offsetMax: Vector2.zero);

        GameObject content = new("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        _contentRect = SetRect(content, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
        _contentTransform = content.transform;
        _rankScroll.viewport = viewportRect;
        _rankScroll.content = _contentRect;
    }

    private static void CreateFooter(Transform parent)
    {
        GameObject bottom = new("兽榜底栏", typeof(RectTransform));
        bottom.transform.SetParent(parent, false);
        SetRect(bottom, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(CardWidth, 20f));
        CreateNavButton(bottom.transform, "ToTop", "置顶", new Vector2(0.42f, 0.5f), ToTop);
        CreateNavButton(bottom.transform, "Refresh", "刷新", new Vector2(0.50f, 0.5f), RefreshAll);
        CreateNavButton(bottom.transform, "ToBottom", "置底", new Vector2(0.58f, 0.5f), ToBottom);

        _countText = CreateText("Count", parent, "共 0 只", 9, MclslUiTheme.BeastRankAccentGold);
        SetRect(_countText.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 52f), new Vector2(CardWidth, 17f));
        ConfineText(_countText, 7);
    }

    private static void CreateEmptyState(Transform parent)
    {
        _emptyState = new GameObject("兽榜空状态", typeof(RectTransform), typeof(Image), typeof(Outline));
        _emptyState.transform.SetParent(parent, false);
        SetRect(_emptyState, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            pos: new Vector2(0f, -(ListTop + ListHeight / 2f)), size: new Vector2(324f, 86f));
        _emptyState.GetComponent<Image>().color = MclslUiTheme.BeastRankSurfacePanel;
        _emptyState.GetComponent<Image>().raycastTarget = false;
        _emptyState.GetComponent<Outline>().effectColor = MclslUiTheme.BeastRankFrameEdge;
        Text emptyTitle = CreateText("EmptyTitle", _emptyState.transform, "暂无妖兽", 11, MclslUiTheme.BeastRankTextMuted);
        emptyTitle.fontStyle = FontStyle.Bold;
        SetRect(emptyTitle.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -12f), new Vector2(-24f, 20f));
        ConfineText(emptyTitle, 9);
        Text emptyBody = CreateText("EmptyBody", _emptyState.transform, "神兽榜只收录本世的普通妖兽，含元婴化形。", 10, MclslUiTheme.BeastRankTextMuted);
        emptyBody.alignment = TextAnchor.UpperCenter;
        SetRect(emptyBody.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -36f), new Vector2(-24f, 40f));
        ConfineText(emptyBody, 9);
        _emptyState.SetActive(false);
    }

    private static GameObject CreateCardPrefab()
    {
        GameObject card = new("MclslBeastRankCard", typeof(RectTransform), typeof(Image), typeof(Button), typeof(MclslBeastRankCardView));
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(CardWidth - 8f, CardHeight - 1f);
        Image bg = card.GetComponent<Image>();
        bg.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
        bg.type = Image.Type.Sliced;
        bg.color = MclslUiTheme.BeastRankSurfacePanel;
        card.GetComponent<Button>().targetGraphic = bg;

        CreateCardText("RankText", card.transform, RankColumnX, RankColumnWidth, 11, TextAnchor.MiddleCenter, MclslUiTheme.BeastRankAccentGold);
        CreateAvatarElement(card.transform);
        CreateCardText("NameText", card.transform, NameColumnX, NameColumnWidth, 8, TextAnchor.MiddleLeft, MclslUiTheme.BeastRankTextPrimary);
        CreateCardText("SpeciesText", card.transform, SpeciesColumnX, SpeciesColumnWidth, 8, TextAnchor.MiddleLeft, MclslUiTheme.BeastRankTextEmphasis);
        CreateCardText("RealmText", card.transform, RealmColumnX, RealmColumnWidth, 8, TextAnchor.MiddleCenter, MclslUiTheme.BeastRankTextPrimary);
        CreateCardText("PowerText", card.transform, PowerColumnX, PowerColumnWidth, 8, TextAnchor.MiddleCenter, MclslUiTheme.BeastRankAccentGold);
        CreateCardText("TalentText", card.transform, TalentColumnX, TalentColumnWidth, 8, TextAnchor.MiddleCenter, MclslUiTheme.BeastRankAccentRed);
        card.SetActive(false);
        return card;
    }

    private static void CreateCardText(string name, Transform parent, float x, float width, int fontSize, TextAnchor alignment, Color color)
    {
        Text text = CreateText(name, parent, string.Empty, fontSize, color);
        text.alignment = alignment;
        SetRect(text.gameObject, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(x, 0f), new Vector2(width, CardHeight - 4f));
        ConfineText(text, Mathf.Min(fontSize, 6));
    }

    private static void CreateAvatarElement(Transform parent)
    {
        UiUnitAvatarElement prefab = Resources.Load<UiUnitAvatarElement>("ui/UnitAvatarElement");
        if (prefab == null) return;
        UiUnitAvatarElement avatar = UnityEngine.Object.Instantiate(prefab, parent);
        avatar.name = "AvatarElement";
        RectTransform rect = avatar.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = new Vector2(AvatarX, 0f);
        rect.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        avatar.show_banner_kingdom = false;
        avatar.show_banner_clan = false;
        if (avatar.kingdomBanner != null) avatar.kingdomBanner.gameObject.SetActive(false);
        if (avatar.clanBanner != null) avatar.clanBanner.gameObject.SetActive(false);
    }

    internal static void OnEntryChanged(long id, MclslRankEntry entry)
    {
        if (_window == null || !_window.gameObject.activeInHierarchy) return;
        bool included = entry != null && entry.IsOrdinaryBeast && MclslActorAccessor.Alive(entry.Actor)
            && PassSpeciesFilter(entry) && PassRealmFilter(entry) && PassSearch(entry, MclslRankTextFormat.NormalizeSearch(_searchQuery));
        if (_filterCursor >= 0)
        {
            if (included) NextEntries?.Upsert(id, entry); else NextEntries?.Remove(id);
        }
        else
        {
            int oldIndex = Entries.IndexOf(id);
            if (included) Entries.Upsert(id, entry); else Entries.Remove(id);
            int newIndex = included ? Entries.IndexOf(id) : -1;
            if (oldIndex >= 0 || newIndex >= 0)
            {
                (int changedStart, int changedEnd) = MclslOrderedChangeRange.Affected(oldIndex, newIndex);
                _rows?.MarkDirty(changedStart, changedEnd);
                _viewDataDirty = true;
            }
        }
    }

    internal static void ClearRuntime()
    {
        Entries.Clear(); NextEntries = null; _filterCursor = -1;
        _needRefresh = true; _viewDataDirty = false;
        _rows?.ClearRows();
    }

    private static void RefreshSortOrder()
    {
        _compare = _sortMode switch
        {
            1 => CompareByRealm,
            2 => CompareByTalent,
            _ => CompareByPower
        };
    }

    private static int CompareByPower(MclslRankEntry left, MclslRankEntry right)
    {
        int order = right.Power.CompareTo(left.Power);
        return order != 0 ? order : left.ActorId.CompareTo(right.ActorId);
    }

    private static int CompareByRealm(MclslRankEntry left, MclslRankEntry right)
    {
        int order = right.RealmSortRank.CompareTo(left.RealmSortRank);
        return order != 0 ? order : left.ActorId.CompareTo(right.ActorId);
    }

    private static int CompareByTalent(MclslRankEntry left, MclslRankEntry right)
    {
        int order = right.TalentLevel.CompareTo(left.TalentLevel);
        return order != 0 ? order : left.ActorId.CompareTo(right.ActorId);
    }

    private static void RefreshAll()
    {
        MclslRankSnapshotSource.Invalidate();
        RefreshCurrentList(false);
    }

    private static void RefreshCurrentList(bool resetScroll)
    {
        NextEntries = new MclslOrderedIdIndex<MclslRankEntry>(_compare);
        _filterCursor = 0;
        _resetPendingScroll = resetScroll;
        _pendingQuery = MclslRankTextFormat.NormalizeSearch(_searchQuery);
    }

    private static bool PassSpeciesFilter(MclslRankEntry entry)
    {
        if (_speciesFilter <= 0) return true;
        int index = _speciesFilter - 1;
        return index >= 0 && index < SpeciesIds.Length && string.Equals(entry.SpeciesId, SpeciesIds[index], StringComparison.Ordinal);
    }

    private static bool PassRealmFilter(MclslRankEntry entry)
    {
        if (_realmFilter <= 0) return true;
        int index = _realmFilter - 1;
        return index >= 0 && index < MclslRealmIds.Ordered.Length
            && string.Equals(entry.RealmId, MclslRealmIds.Ordered[index], StringComparison.Ordinal);
    }

    private static bool PassSearch(MclslRankEntry entry, string normalizedQuery)
    {
        if (string.IsNullOrWhiteSpace(normalizedQuery)) return true;
        return MclslRankTextFormat.NormalizeSearch(entry.Name).Contains(normalizedQuery, StringComparison.Ordinal)
            || MclslRankTextFormat.NormalizeSearch(entry.SpeciesName).Contains(normalizedQuery, StringComparison.Ordinal);
    }

    private static void ContinueFilteredView()
    {
        if (_filterCursor < 0) return;
        IReadOnlyList<MclslRankEntry> snapshot = MclslRankSnapshotSource.MembershipEntries;
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 2000;
        int processed = 0;
        while (_filterCursor < snapshot.Count && processed++ < 128 && Stopwatch.GetTimestamp() < deadline)
        {
            MclslRankEntry entry = snapshot[_filterCursor++];
            // 神兽榜只收普通妖兽（含化形），具名妖兽与修士不进入。
            if (entry == null || !entry.IsOrdinaryBeast) continue;
            if (!MclslActorAccessor.Alive(entry.Actor)) continue;
            if (!PassSpeciesFilter(entry) || !PassRealmFilter(entry) || !PassSearch(entry, _pendingQuery)) continue;
            NextEntries.Upsert(entry.ActorId, entry);
        }
        if (_filterCursor < snapshot.Count) return;
        _filterCursor = -1;
        Entries = NextEntries; NextEntries = null;
        _viewDataDirty = false;
        _countText.text = string.IsNullOrWhiteSpace(_searchQuery)
            ? "共 " + Entries.Count.ToString(CultureInfo.InvariantCulture) + " 只"
            : "搜索：" + _searchQuery.Trim() + "  共 " + Entries.Count.ToString(CultureInfo.InvariantCulture) + " 只";
        _emptyState?.SetActive(Entries.Count == 0);
        _rows?.Rebuild(Entries, _resetPendingScroll, ViewHeight);
    }

    private static void UpdateVisibleCards()
    {
        if (_filterCursor >= 0) { ContinueFilteredView(); return; }
        if (_viewDataDirty)
        {
            _viewDataDirty = false;
            if (_countText != null) _countText.text = "共 " + Entries.Count.ToString(CultureInfo.InvariantCulture) + " 只";
            _emptyState?.SetActive(Entries.Count == 0);
        }
        if (_needRefresh)
        {
            _needRefresh = false;
            RefreshCurrentList(true);
            return;
        }
        _rows?.Update(ContentScrollY, ViewHeight);
    }

    private static float ContentScrollY => _contentRect != null ? _contentRect.anchoredPosition.y : 0f;

    private static float ViewHeight => _scrollViewRect != null && _scrollViewRect.rect.height > 0f
        ? _scrollViewRect.rect.height : ListHeight;

    private static void BindRow(GameObject card, MclslRankEntry item, int index)
        => card.GetComponent<MclslBeastRankCardView>()?.Setup(item, index);

    private static void ToTop()
    {
        if (_contentRect != null) _contentRect.anchoredPosition = Vector2.zero;
    }

    private static void ToBottom()
    {
        if (_contentRect == null || _scrollViewRect == null) return;
        float max = Math.Max(0f, _contentRect.sizeDelta.y - _scrollViewRect.rect.height);
        _contentRect.anchoredPosition = new Vector2(0f, max);
    }

    private static void BuildSpeciesOptions()
    {
        MclslBeastDefinition[] all = MclslBeastCatalog.All;
        SpeciesLabels = new string[all.Length + 1];
        SpeciesIds = new string[all.Length];
        SpeciesLabels[0] = "全部物种";
        for (int i = 0; i < all.Length; i++)
        {
            SpeciesLabels[i + 1] = all[i].Name;
            SpeciesIds[i] = all[i].Id;
        }
    }

    private static InputField CreateSearchInput(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject inputObj = new("BeastSearchInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputObj.transform.SetParent(parent, false);
        Image inputImage = inputObj.GetComponent<Image>();
        inputImage.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        inputImage.type = Image.Type.Sliced;
        inputImage.color = MclslUiTheme.BeastRankSurfaceDeep;
        SetRect(inputObj, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), position, size);

        Text inputText = CreateText("Text", inputObj.transform, string.Empty, 10, MclslUiTheme.BeastRankTextPrimary);
        inputText.alignment = TextAnchor.MiddleLeft;
        SetRect(inputText.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(8f, 0f), offsetMax: new Vector2(-8f, 0f));
        Text placeholder = CreateText("Placeholder", inputObj.transform, "搜索名称或物种", 10, MclslUiTheme.BeastRankTextMuted);
        placeholder.alignment = TextAnchor.MiddleLeft;
        SetRect(placeholder.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(8f, 0f), offsetMax: new Vector2(-8f, 0f));

        InputField field = inputObj.GetComponent<InputField>();
        field.textComponent = inputText;
        field.placeholder = placeholder;
        field.lineType = InputField.LineType.SingleLine;
        field.characterLimit = 24;
        field.onValueChanged.AddListener(value =>
        {
            _searchQuery = value ?? string.Empty;
            RefreshCurrentList(true);
        });
        return field;
    }

    private static Button CreateNavButton(Transform parent, string name, string text, Vector2 anchor, Action onClick)
    {
        Button button = CreateButton(parent, name, text, new Vector2(56f, 20f), onClick);
        SetRect(button.gameObject, anchor, anchor, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(56f, 20f));
        return button;
    }

    // 按钮 / 下拉框 / 下拉模板与玄黄榜结构完全一致，仅配色不同：复用 MclslRankWindow 实现并按神兽榜主题传色。
    private static Button CreateButton(Transform parent, string name, string text, Vector2 size, Action onClick, Vector2? pos = null)
        => MclslRankWindow.CreateButton(parent, name, text, size, onClick, pos,
            MclslUiTheme.BeastRankButton, MclslUiTheme.BeastRankTextPrimary);

    private static Dropdown CreateDropdown(string name, Transform parent, string[] options, Action<int> onChange)
        => MclslRankWindow.CreateDropdown(name, parent, options, onChange,
            MclslUiTheme.BeastRankButton, MclslUiTheme.BeastRankTextPrimary, MclslUiTheme.BeastRankSurfacePanel);

    // 纯几何/文本基础件直接复用玄黄榜实现，避免重复整套旧榜机制。
    private static Text CreateText(string name, Transform parent, string text, int fontSize, Color color)
        => MclslRankWindow.CreateText(name, parent, text, fontSize, color);

    private static void ConfineText(Text label, int minFontSize)
        => MclslRankWindow.ConfineText(label, minFontSize);

    private static RectTransform SetRect(GameObject obj, Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? pivot = null,
        Vector2? pos = null, Vector2? size = null, Vector2? offsetMin = null, Vector2? offsetMax = null)
        => MclslRankWindow.SetRect(obj, anchorMin, anchorMax, pivot, pos, size, offsetMin, offsetMax);
}

// 单张横兽牌：名次 / 头像 / 名称 / 物种 / 境界 / 战力 / 神通等级，无国家列。
internal sealed class MclslBeastRankCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Actor _actor;

    internal void Setup(MclslRankEntry item, int index)
    {
        _actor = item?.Actor;
        Image background = GetComponent<Image>();
        if (background != null)
        {
            background.color = index switch
            {
                0 => MclslUiTheme.BeastRankRowFirst,
                1 => MclslUiTheme.BeastRankRowSecond,
                2 => MclslUiTheme.BeastRankRowThird,
                _ when index % 2 == 0 => MclslUiTheme.BeastRankRowEven,
                _ => MclslUiTheme.BeastRankRowOdd
            };
        }
        MclslRankTextFormat.SetText(transform, "RankText", (index + 1).ToString(CultureInfo.InvariantCulture), RankColor(index));
        MclslRankTextFormat.SetText(transform, "NameText", string.IsNullOrWhiteSpace(item.Name) ? "未名妖兽" : item.Name, index < 3 ? RankColor(index) : MclslUiTheme.BeastRankTextPrimary);
        MclslRankTextFormat.SetText(transform, "SpeciesText", string.IsNullOrWhiteSpace(item.SpeciesName) ? "妖兽" : item.SpeciesName, MclslUiTheme.BeastRankTextEmphasis);
        MclslRankTextFormat.SetText(transform, "RealmText", string.IsNullOrWhiteSpace(item.RealmName) ? "未入道" : item.RealmName.Replace("新法·", string.Empty), MclslUiTheme.BeastRankTextPrimary);
        MclslRankTextFormat.SetText(transform, "PowerText", MclslRankTextFormat.FormatPower(item.Power), MclslUiTheme.BeastRankAccentGold);
        MclslRankTextFormat.SetText(transform, "TalentText", item.TalentLevel > 0 ? item.TalentLevel.ToString(CultureInfo.InvariantCulture) : "—", MclslUiTheme.BeastRankAccentRed);

        UiUnitAvatarElement avatar = GetComponentInChildren<UiUnitAvatarElement>(true);
        if (avatar != null)
        {
            bool valid = MclslActorAccessor.Alive(_actor);
            avatar.gameObject.SetActive(valid);
            if (valid)
            {
                avatar.show(_actor);
                if (avatar.kingdomBanner != null) avatar.kingdomBanner.gameObject.SetActive(false);
                if (avatar.clanBanner != null) avatar.clanBanner.gameObject.SetActive(false);
            }
        }

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (MclslActorAccessor.Alive(_actor)) ActionLibrary.openUnitWindow(_actor);
            });
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MclslActorAccessor.Alive(_actor)) _actor.showTooltip(this);
    }

    public void OnPointerExit(PointerEventData eventData) => Tooltip.hideTooltip();

    private void OnDisable()
    {
        _actor = null;
        Tooltip.hideTooltip();
    }

    private static Color RankColor(int rank)
    {
        if (rank == 0) return MclslUiTheme.BeastRankMedalFirst;
        if (rank == 1) return MclslUiTheme.BeastRankMedalSecond;
        if (rank == 2) return MclslUiTheme.BeastRankMedalThird;
        return MclslUiTheme.BeastRankTextEmphasis;
    }


}
