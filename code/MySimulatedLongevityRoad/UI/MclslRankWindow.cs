using System;
using System.Collections.Generic;
using System.Globalization;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslRankWindow
{
    private const string TransparentCloseGlyphName = "MclslRankTransparentCloseGlyph";
    private const float WindowWidth = 620f;
    private const float WindowHeight = 420f;
    private const float LeftPanelWidth = 170f;
    private const float RightPanelWidth = 170f;
    private const float CenterWidth = 250f;
    private const float ListHeight = 288f;
    private const float CardHeight = 46f;

    private static readonly string[] RealmOptions = { "全部", "感气", "炼气", "筑基", "金丹", "元婴", "化神", "合道", "长生" };
    private static readonly string[] RootOptions = { "全部灵根", "金", "木", "水", "火", "土", "风", "雷", "阴", "阳", "空间" };
    private static readonly List<MclslRankSortKey> ActiveSortKeys = new();
    private static readonly List<MclslRankEntry> Entries = new();
    private static readonly List<GameObject> CardInstances = new();
    private static readonly Dictionary<int, GameObject> CardByIndex = new();
    private static readonly List<int> CardIndexBuffer = new();
    private static readonly List<GameObject> SelectedFilterButtons = new();
    private static readonly List<MclslRankFilterSetting> ActiveFilters = new();
    private static readonly List<GameObject> KingdomFilterButtons = new();
    private static readonly List<GameObject> AssetFilterButtons = new();
    private static readonly List<GameObject> TraitFilterButtons = new();
    private static readonly List<GameObject> SelectedSortButtons = new();
    private static readonly List<GameObject> AvailableSortButtons = new();

    private static ScrollWindow _window;
    private static RectTransform _contentRect;
    private static RectTransform _scrollViewRect;
    private static ScrollRect _rankScroll;
    private static Transform _contentTransform;
    private static Transform _selectedFilterContainer;
    private static Transform _kingdomFilterContainer;
    private static Transform _assetFilterContainer;
    private static Transform _traitFilterContainer;
    private static Transform _selectedSortContainer;
    private static Transform _availableSortContainer;
    private static Text _emptyText;
    private static Text _countText;
    private static Dropdown _rootDropdown;
    private static Dropdown _realmDropdown;
    private static InputField _searchInput;
    private static GameObject _cardPrefab;
    private static int _rootFilter;
    private static int _realmFilter;
    private static string _searchQuery = string.Empty;
    private static bool _needRefresh;
    private static bool _filterChoicesInitialized;
    private static int _lastViewStart = int.MaxValue;
    private static int _lastViewEnd = -1;

    private static readonly MclslRankSortDef[] SortDefs =
    {
        new("power", "战力", "ui/Icons/TianDiZhiLi", entry => (float)Math.Min(float.MaxValue, entry.Power), entry => FormatNumber(entry.Power)),
        new("realm", "境界", "trait/realm_7", entry => entry.RealmIndex, entry => entry.RealmName),
        new("root", "灵根品阶", "trait/gifts_6", entry => entry.Aptitude, entry => string.IsNullOrWhiteSpace(entry.GiftName) ? entry.Aptitude.ToString(CultureInfo.InvariantCulture) : entry.GiftName),
        new("essence", "真元", "ui/Icons/ZhenQi", entry => entry.TrueEssence, entry => entry.TrueEssence.ToString(CultureInfo.InvariantCulture)),
        new("mind", "心境", "ui/Icons/XinJing", entry => entry.MindState, entry => entry.MindState.ToString(CultureInfo.InvariantCulture)),
        new("contribution", "贡献", "ui/Icons/GongXianZhi", entry => entry.Contribution, entry => entry.Contribution.ToString(CultureInfo.InvariantCulture)),
        new("stones", "灵石", "ui/Icons/LingShi", entry => entry.SpiritStones, entry => entry.SpiritStones.ToString(CultureInfo.InvariantCulture)),
        new("miasma", "仙凡瘴", "ui/Icons/XianFanZhang", entry => entry.MortalMiasma, entry => entry.MortalMiasmaLimit > 0
            ? entry.MortalMiasma.ToString(CultureInfo.InvariantCulture) + "/" + entry.MortalMiasmaLimit.ToString(CultureInfo.InvariantCulture)
            : entry.MortalMiasma.ToString(CultureInfo.InvariantCulture))
    };

    internal static void ShowWindow()
    {
        bool created = EnsureWindow();
        if (_window == null) return;
        if (created)
        {
            RefreshFilterChoices();
            RefreshSelectedFilterButtons();
            RefreshSelectedSortButtons();
            _needRefresh = true;
        }
        MclslWindowOpenGuard.Show(_window, "MclslRank", created);
    }

    internal static bool ShouldBlockRightClickClose(ScrollWindow target)
    {
        return target != null
            && ReferenceEquals(target, _window)
            && IsRightClickClosingFrame();
    }

    internal static bool ShouldBlockGlobalRightClickClose()
    {
        return _window != null
            && _window.gameObject.activeInHierarchy
            && IsRightClickClosingFrame();
    }

    private static bool IsRightClickClosingFrame()
    {
        try
        {
            return Input.GetMouseButton(1) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1);
        }
        catch
        {
            return false;
        }
    }

    private static bool EnsureWindow()
    {
        if (_window != null) return false;
        _window = WindowCreator.CreateEmptyWindow("MclslRank", string.Empty, "ui/Icons/XuanHuangXiuShiBang");
        if (_window == null) return false;
        RemoveCloseButtonBackground();
        SetupWindowContent();
        _cardPrefab = CreateCardPrefab();
        _cardPrefab.transform.SetParent(_window.transform, false);
        MclslRankWindowUpdater updater = _window.gameObject.AddComponent<MclslRankWindowUpdater>();
        updater.OnOpen = () =>
        {
            MclslRankSnapshotSource.Invalidate();
            RemoveCloseButtonBackground();
            if (ShouldRefreshFilterChoicesOnOpen()) RefreshFilterChoices();
            RefreshSelectedFilterButtons();
            RefreshSelectedSortButtons();
            _needRefresh = true;
        };
        updater.OnUpdate = () => { RemoveCloseButtonBackground(); UpdateVisibleCards(); };
        updater.OnClose = ClearCards;
        return true;
    }

    private static void RemoveCloseButtonBackground()
    {
        if (_window == null) return;
        Transform closeButton = FindCloseButtonTransform();
        if (closeButton == null) return;
        Button button = closeButton.GetComponent<Button>();
        Image rootImage = closeButton.GetComponent<Image>();
        if (rootImage == null) rootImage = closeButton.gameObject.AddComponent<Image>();
        rootImage.sprite = null;
        rootImage.color = new Color(1f, 1f, 1f, 0f);
        rootImage.raycastTarget = true;
        if (button != null)
        {
            button.targetGraphic = rootImage;
            button.transition = Selectable.Transition.None;
        }

        Image[] images = closeButton.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            if (image == null || image == rootImage) continue;
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = false;
        }
        RawImage[] rawImages = closeButton.GetComponentsInChildren<RawImage>(true);
        for (int i = 0; i < rawImages.Length; i++)
        {
            RawImage rawImage = rawImages[i];
            if (rawImage == null) continue;
            rawImage.color = new Color(1f, 1f, 1f, 0f);
            rawImage.raycastTarget = false;
        }

        EnsureTransparentCloseGlyph(closeButton);
    }

    private static void MakeCloseAncestorsTransparent(Transform closeButton)
    {
        Transform root = _window.transform;
        Transform current = closeButton.parent;
        while (current != null && current != root)
        {
            string name = current.name ?? string.Empty;
            bool closeShell = name.IndexOf("close", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("buttonclose", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("x", StringComparison.OrdinalIgnoreCase) >= 0;
            if (closeShell)
            {
                Image[] images = current.GetComponents<Image>();
                for (int i = 0; i < images.Length; i++) MakeTransparentButtonImage(images[i], false);
                RawImage[] rawImages = current.GetComponents<RawImage>();
                for (int i = 0; i < rawImages.Length; i++) MakeTransparentButtonRawImage(rawImages[i], false);
            }
            current = current.parent;
        }
    }

    private static void MakeTransparentButtonImage(Image image, bool keepRaycast)
    {
        if (image == null) return;
        image.sprite = null;
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = keepRaycast;
        Button button = image.GetComponent<Button>();
        if (button != null)
        {
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
        }
    }

    private static void MakeTransparentButtonRawImage(RawImage image, bool keepRaycast)
    {
        if (image == null) return;
        image.texture = null;
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = keepRaycast;
        Button button = image.GetComponent<Button>();
        if (button != null)
        {
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
        }
    }

    private static void HideNativeCloseGraphics(Transform closeButton)
    {
        Graphic[] graphics = closeButton.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            if (graphic == null) continue;
            bool isRoot = graphic.transform == closeButton;
            bool isGlyph = string.Equals(graphic.gameObject.name, TransparentCloseGlyphName, StringComparison.Ordinal);
            if (isGlyph) continue;
            if (isRoot)
            {
                graphic.enabled = true;
                graphic.color = new Color(1f, 1f, 1f, 0f);
                graphic.raycastTarget = true;
                continue;
            }
            graphic.raycastTarget = false;
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0f);
            try { graphic.canvasRenderer.SetAlpha(0f); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs #1: " + mclslEmptyCatchEx.Message); }
            graphic.enabled = false;
        }
    }

    private static void EnsureTransparentHitArea(Transform closeButton)
    {
        Image image = closeButton.GetComponent<Image>();
        if (image == null) image = closeButton.gameObject.AddComponent<Image>();
        image.sprite = null;
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;
        Button button = closeButton.GetComponent<Button>();
        if (button != null)
        {
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
        }
    }

    private static void EnsureTransparentCloseGlyph(Transform closeButton)
    {
        Transform existing = closeButton.Find(TransparentCloseGlyphName);
        Text glyph = existing == null ? null : existing.GetComponent<Text>();
        if (glyph == null)
        {
            GameObject glyphObject = new(TransparentCloseGlyphName);
            glyphObject.transform.SetParent(closeButton, false);
            RectTransform rect = glyphObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            glyph = glyphObject.AddComponent<Text>();
            glyph.raycastTarget = false;
        }
        glyph.text = "×";
        glyph.alignment = TextAnchor.MiddleCenter;
        glyph.fontSize = 22;
        glyph.fontStyle = FontStyle.Bold;
        glyph.color = new Color(1f, 0.08f, 0.06f, 1f);
        glyph.enabled = true;
        glyph.raycastTarget = false;
        glyph.font = LocalizedTextManager.current_font ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        glyph.transform.SetAsLastSibling();
        try { glyph.canvasRenderer.SetAlpha(1f); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs #2: " + mclslEmptyCatchEx.Message); }
    }

    private static void RemoveTransparentCloseGlyphs()
    {
        if (_window == null) return;
        Text[] texts = _window.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            Text text = texts[i];
            if (text == null || !string.Equals(text.gameObject.name, TransparentCloseGlyphName, StringComparison.Ordinal)) continue;
            UnityEngine.Object.Destroy(text.gameObject);
        }
    }

    private static Transform FindCloseButtonTransform()
    {
        Transform root = _window.transform;
        string[] paths =
        {
            "CloseButton",
            "ButtonClose",
            "Close",
            "Background/CloseButton",
            "Background/ButtonClose",
            "Background/Close"
        };
        for (int i = 0; i < paths.Length; i++)
        {
            Transform found = root.Find(paths[i]);
            if (found != null) return found;
        }
        Button[] buttons = _window.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Transform current = buttons[i].transform;
            string name = current.name ?? string.Empty;
            bool closeName = name.IndexOf("close", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("buttonclose", StringComparison.OrdinalIgnoreCase) >= 0;
            bool exactX = string.Equals(name, "x", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "buttonx", StringComparison.OrdinalIgnoreCase);
            if (closeName || exactX)
                return current;
        }
        return null;
    }

    private static void SetupWindowContent()
    {
        Transform background = _window.transform.Find("Background");
        if (background == null) return;
        RectTransform bgRect = background.GetComponent<RectTransform>();
        if (bgRect != null) bgRect.sizeDelta = new Vector2(WindowWidth, WindowHeight);
        CreateLeftPanel(background);
        CreateCenterPanel(background);
        CreateRightPanel(background);
    }

    private static void CreateLeftPanel(Transform parent)
    {
        GameObject panel = new("LeftPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        SetRect(panel, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(LeftPanelWidth, -60));
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.05f);
        panel.AddComponent<MclslRankRightClickHandler>().BlockRightClick = true;
        CreateSectionTitle(panel.transform, "筛选", new Vector2(0, -5));

        GameObject scroll = CreateScrollView(panel.transform, "FilterScroll", new Vector2(5, 30), new Vector2(-5, -25));
        Transform filterContent = scroll.transform.Find("Viewport/Content");
        _selectedFilterContainer = CreateTitledGrid(filterContent, "已选筛选", 25, 5, false);
        _kingdomFilterContainer = CreateTitledGrid(filterContent, "国家", 25, 5, true);
        _assetFilterContainer = CreateTitledGrid(filterContent, "物种", 25, 5, true);
        _traitFilterContainer = CreateTitledGrid(filterContent, "特质", 25, 5, true);
        RefreshSelectedFilterButtons();
        Button clear = CreateButton(panel.transform, "ClearFilters", "清空全部", new Vector2(68, 18), ClearFilters);
        SetRect(clear.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 8), new Vector2(68, 18));
        SetButtonColor(clear, new Color(0.6f, 0.3f, 0.3f));
    }

    private static void CreateCenterPanel(Transform parent)
    {
        Text title = CreateText("RankTitle", parent, "玄黄修士榜", 11, new Color(1f, 0.55f, 0f));
        title.fontStyle = FontStyle.Bold;
        SetRect(title.gameObject, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -14), new Vector2(CenterWidth, 18));

        GameObject topBar = new("TopBar", typeof(RectTransform));
        topBar.transform.SetParent(parent, false);
        SetRect(topBar, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -44), new Vector2(CenterWidth, 26));
        _rootDropdown = CreateDropdown("RootDropdown", topBar.transform, RootOptions, index =>
        {
            _rootFilter = index;
            RefreshSelectedFilterButtons();
            RefreshCurrentList(true);
        });
        SetRect(_rootDropdown.gameObject, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(120, 25));
        _realmDropdown = CreateDropdown("RealmDropdown", topBar.transform, RealmOptions, index =>
        {
            _realmFilter = index;
            RefreshSelectedFilterButtons();
            RefreshCurrentList(true);
        });
        SetRect(_realmDropdown.gameObject, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(120, 25));

        GameObject scrollObj = new("RankScrollView", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);
        _scrollViewRect = SetRect(scrollObj, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -13), new Vector2(CenterWidth, ListHeight));
        Image scrollBg = scrollObj.GetComponent<Image>();
        scrollBg.color = new Color(0f, 0f, 0f, 0.14f);
        scrollObj.GetComponent<Mask>().showMaskGraphic = true;
        _rankScroll = scrollObj.GetComponent<ScrollRect>();
        _rankScroll.horizontal = false;
        _rankScroll.vertical = true;
        _rankScroll.movementType = ScrollRect.MovementType.Clamped;
        _rankScroll.scrollSensitivity = 30f;

        GameObject viewport = new("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRect = SetRect(viewport, Vector2.zero, Vector2.one, offsetMin: Vector2.zero, offsetMax: Vector2.zero);

        GameObject content = new("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        _contentRect = SetRect(content, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
        _contentTransform = content.transform;
        _rankScroll.viewport = viewportRect;
        _rankScroll.content = _contentRect;

        GameObject bottom = new("BottomBar", typeof(RectTransform));
        bottom.transform.SetParent(parent, false);
        SetRect(bottom, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 8), new Vector2(CenterWidth, 26));
        CreateNavButton(bottom.transform, "ToTop", "顶", new Vector2(0.20f, 0.5f), ToTop);
        CreateNavButton(bottom.transform, "Refresh", "刷新", new Vector2(0.50f, 0.5f), RefreshAll);
        CreateNavButton(bottom.transform, "ToBottom", "底", new Vector2(0.80f, 0.5f), ToBottom);

        _countText = CreateText("Count", parent, "共 0 人", 9, new Color(0.72f, 0.72f, 0.72f));
        SetRect(_countText.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(CenterWidth, 16));

        _emptyText = CreateText("Empty", parent, "暂无记录", 14, new Color(0.64f, 0.64f, 0.64f));
        SetRect(_emptyText.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size: new Vector2(200, 50));
    }

    private static void CreateRightPanel(Transform parent)
    {
        GameObject panel = new("RightPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        SetRect(panel, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-8, 0), new Vector2(RightPanelWidth, -60));
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.05f);
        panel.AddComponent<MclslRankRightClickHandler>().BlockRightClick = true;
        CreateSectionTitle(panel.transform, "排序", new Vector2(0, -5));

        GameObject scroll = CreateScrollView(panel.transform, "SortScroll", new Vector2(5, 30), new Vector2(-5, -25));
        Transform sortContent = scroll.transform.Find("Viewport/Content");
        _selectedSortContainer = CreateTitledGrid(sortContent, "当前排序", 25, 5, false);
        _availableSortContainer = CreateTitledGrid(sortContent, "可选排序", 25, 5, true);
        CreateSearchSection(sortContent);
        CreateAvailableSortButtons();
        RefreshSelectedSortButtons();

        Button clear = CreateButton(panel.transform, "ClearSort", "清空排序", new Vector2(68, 18), ClearSortKeys);
        SetRect(clear.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 8), new Vector2(68, 18));
        SetButtonColor(clear, new Color(0.6f, 0.3f, 0.3f));
    }

    private static void RefreshAll()
    {
        MclslRankSnapshotSource.Invalidate();
        RefreshFilterChoices();
        RefreshSelectedFilterButtons();
        RefreshSelectedSortButtons();
        RefreshCurrentList(true);
    }

    private static void RefreshCurrentList(bool resetScroll)
    {
        ClearCards();
        if (resetScroll && _contentRect != null) _contentRect.anchoredPosition = Vector2.zero;
        Entries.Clear();
        IReadOnlyList<MclslRankEntry> snapshot = MclslRankSnapshotSource.EntriesSnapshot(forceRebuild: true);
        string normalizedQuery = NormalizeSearch(_searchQuery);
        for (int i = 0; i < snapshot.Count; i++)
        {
            MclslRankEntry entry = snapshot[i];
            if (entry == null || !MclslActorAccessor.Alive(entry.Actor)) continue;
            if (!PassRootFilter(entry) || !PassRealmFilter(entry) || !PassSearch(entry, normalizedQuery) || !PassActiveFilters(entry.Actor)) continue;
            Entries.Add(entry);
        }
        SortEntries(Entries);
        _contentRect.sizeDelta = new Vector2(0, Math.Max(1, Entries.Count) * CardHeight);
        _lastViewStart = int.MaxValue;
        _lastViewEnd = -1;
        _countText.text = string.IsNullOrWhiteSpace(_searchQuery)
            ? "共 " + Entries.Count.ToString(CultureInfo.InvariantCulture) + " 人"
            : "搜索：" + _searchQuery.Trim() + "  共 " + Entries.Count.ToString(CultureInfo.InvariantCulture) + " 人";
        _emptyText.gameObject.SetActive(Entries.Count == 0);
        CreateInitialVisibleCards();
        RefreshSelectedSortButtons();
    }

    private static bool PassRootFilter(MclslRankEntry entry)
    {
        if (_rootFilter <= 0 || _rootFilter >= RootOptions.Length) return true;
        return string.Equals(entry.RootText ?? string.Empty, RootOptions[_rootFilter], StringComparison.Ordinal);
    }

    private static bool PassRealmFilter(MclslRankEntry entry)
    {
        if (_realmFilter <= 0) return true;
        if (_realmFilter == 1) return string.IsNullOrWhiteSpace(entry.RealmId);
        int index = _realmFilter - 2;
        return index >= 0 && index < MclslRealmIds.Ordered.Length && entry.RealmId == MclslRealmIds.Ordered[index];
    }

    private static bool PassSearch(MclslRankEntry entry, string normalizedQuery)
    {
        if (string.IsNullOrWhiteSpace(normalizedQuery)) return true;
        return (entry.NormalizedSearchText ?? string.Empty).Contains(normalizedQuery, StringComparison.Ordinal);
    }

    private static bool PassActiveFilters(Actor actor)
    {
        ActiveFilters.RemoveAll(filter => filter == null || filter.ToBeRemoved);
        if (ActiveFilters.Count == 0) return true;

        bool hasOrFilter = false;
        bool orMatched = false;
        for (int i = 0; i < ActiveFilters.Count; i++)
        {
            MclslRankFilterSetting filter = ActiveFilters[i];
            bool matched = SafeFilter(filter, actor);
            switch (filter.Type)
            {
                case MclslRankFilterType.And:
                    if (!matched) return false;
                    break;
                case MclslRankFilterType.Or:
                    hasOrFilter = true;
                    orMatched |= matched;
                    break;
                case MclslRankFilterType.Not:
                    if (matched) return false;
                    break;
            }
        }
        return !hasOrFilter || orMatched;
    }

    private static bool SafeFilter(MclslRankFilterSetting filter, Actor actor)
    {
        if (filter?.FilterFunc == null) return true;
        try { return filter.FilterFunc(actor); }
        catch { return false; }
    }

    private static string NormalizeSearch(string value) => (value ?? string.Empty).Trim().Replace(" ", string.Empty).Replace("　", string.Empty);

    private static void SortEntries(List<MclslRankEntry> entries)
    {
        entries.Sort((left, right) =>
        {
            if (ActiveSortKeys.Count == 0)
            {
                int result = right.Power.CompareTo(left.Power);
                if (result != 0) return result;
            }
            else
            {
                for (int i = 0; i < ActiveSortKeys.Count; i++)
                {
                    MclslRankSortKey key = ActiveSortKeys[i];
                    float lv = key.Def.GetValue(left);
                    float rv = key.Def.GetValue(right);
                    int result = (key.Ascending ? 1 : -1) * lv.CompareTo(rv);
                    if (result != 0) return result;
                }
            }
            return string.Compare(left.Name, right.Name, StringComparison.Ordinal);
        });
    }

    private static void CreateInitialVisibleCards()
    {
        if (Entries.Count == 0 || _scrollViewRect == null) return;
        int visible = Mathf.CeilToInt((_scrollViewRect.rect.height > 0f ? _scrollViewRect.rect.height : ListHeight) / CardHeight) + 2;
        int end = Mathf.Min(visible - 1, Entries.Count - 1);
        for (int i = 0; i <= end; i++) CreateActorCard(i);
        _lastViewStart = 0;
        _lastViewEnd = end;
    }

    private static void UpdateVisibleCards()
    {
        if (_needRefresh)
        {
            _needRefresh = false;
            RefreshCurrentList(true);
            return;
        }
        if (Entries.Count == 0 || _contentRect == null || _scrollViewRect == null) return;
        float scrollY = _contentRect.anchoredPosition.y;
        float viewHeight = _scrollViewRect.rect.height > 0f ? _scrollViewRect.rect.height : ListHeight;
        int start = Mathf.Max(0, Mathf.FloorToInt(scrollY / CardHeight) - 1);
        int end = Mathf.Min(Mathf.CeilToInt((scrollY + viewHeight) / CardHeight) + 1, Entries.Count - 1);
        if (start == _lastViewStart && end == _lastViewEnd) return;

        CardIndexBuffer.Clear();
        foreach (KeyValuePair<int, GameObject> pair in CardByIndex)
            if (pair.Key < start || pair.Key > end) CardIndexBuffer.Add(pair.Key);
        for (int i = 0; i < CardIndexBuffer.Count; i++)
        {
            int index = CardIndexBuffer[i];
            if (CardByIndex.TryGetValue(index, out GameObject card) && card != null)
            {
                CardInstances.Remove(card);
                UnityEngine.Object.Destroy(card);
            }
            CardByIndex.Remove(index);
        }
        for (int i = start; i <= end; i++) CreateActorCard(i);
        _lastViewStart = start;
        _lastViewEnd = end;
    }

    private static void CreateActorCard(int index)
    {
        if (index < 0 || index >= Entries.Count || CardByIndex.ContainsKey(index) || _cardPrefab == null || _contentTransform == null) return;
        MclslRankEntry item = Entries[index];
        GameObject card = UnityEngine.Object.Instantiate(_cardPrefab, _contentTransform);
        card.SetActive(true);
        card.transform.localPosition = new Vector3(0, -CardHeight / 2f - index * CardHeight);
        CardInstances.Add(card);
        CardByIndex[index] = card;
        string primary = ActiveSortKeys.Count > 0 ? ActiveSortKeys[0].Def.GetDisplay(item) : FormatNumber(item.Power);
        card.GetComponent<MclslRankCardView>()?.Setup(item, index, primary);
    }

    private static GameObject CreateCardPrefab()
    {
        GameObject card = new("MclslRankCardVNext", typeof(RectTransform), typeof(Image), typeof(Button), typeof(MclslRankCardView));
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(240f, 42f);
        Image bg = card.GetComponent<Image>();
        bg.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
        bg.type = Image.Type.Sliced;
        card.GetComponent<Button>().targetGraphic = bg;
        CreateCardText("RankText", card.transform, new Vector2(5, 0), new Vector2(30, 34), 13, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0f));
        CreateAvatarElement(card.transform);
        CreateCardText("NameText", card.transform, new Vector2(76, 7), new Vector2(92, 18), 8, TextAnchor.MiddleLeft, Color.white);
        CreateCardText("PowerText", card.transform, new Vector2(76, -8), new Vector2(80, 18), 8, TextAnchor.MiddleLeft, new Color(0.4f, 0.8f, 1f));
        CreateCardText("RightText", card.transform, new Vector2(160, -6), new Vector2(44, 18), 8, TextAnchor.MiddleLeft, new Color(1f, 0.84f, 0f));
        CreateCardText("RealmText", card.transform, new Vector2(204, -6), new Vector2(36, 18), 8, TextAnchor.MiddleLeft, new Color(1f, 0.6f, 0.2f));
        card.SetActive(false);
        return card;
    }

    private static void CreateCardText(string name, Transform parent, Vector2 pos, Vector2 size, int fontSize, TextAnchor alignment, Color color)
    {
        Text text = CreateText(name, parent, string.Empty, fontSize, color);
        text.alignment = alignment;
        SetRect(text.gameObject, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), pos, size);
    }

    private static void CreateAvatarElement(Transform parent)
    {
        UiUnitAvatarElement prefab = Resources.Load<UiUnitAvatarElement>("ui/UnitAvatarElement");
        if (prefab != null)
        {
            UiUnitAvatarElement avatar = UnityEngine.Object.Instantiate(prefab, parent);
            avatar.name = "AvatarElement";
            RectTransform rect = avatar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(38, 0);
            rect.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            avatar.show_banner_kingdom = true;
            avatar.show_banner_clan = false;
            if (avatar.kingdomBanner != null) avatar.kingdomBanner.gameObject.SetActive(false);
            if (avatar.clanBanner != null) avatar.clanBanner.gameObject.SetActive(false);
            return;
        }
        Image image = CreateImage("AvatarFallback", parent, GetSafeSprite("ui/icons/iconQuestionMark"), Color.white, new Vector2(30, 30));
        SetRect(image.gameObject, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(30, 30));
    }

    private static void RefreshFilterChoices()
    {
        RefreshKingdomFilters(MclslRankSnapshotSource.KingdomFilterChoicesSnapshot());
        RefreshAssetFilters(MclslRankSnapshotSource.AssetFilterChoicesSnapshot());
        RefreshTraitFilters(MclslRankSnapshotSource.TraitFilterChoicesSnapshot());
        _filterChoicesInitialized = HasAnyAvailableFilterButton();
    }

    private static bool ShouldRefreshFilterChoicesOnOpen()
    {
        return !_filterChoicesInitialized || !HasAnyAvailableFilterButton();
    }

    private static bool HasAnyAvailableFilterButton()
    {
        return KingdomFilterButtons.Count > 0
            || AssetFilterButtons.Count > 0
            || TraitFilterButtons.Count > 0;
    }

    private static void RefreshKingdomFilters(IReadOnlyList<MclslRankKingdomFilterChoice> choices)
    {
        ClearObjectList(KingdomFilterButtons);
        if (_kingdomFilterContainer == null) return;
        choices ??= Array.Empty<MclslRankKingdomFilterChoice>();
        for (int i = 0; i < choices.Count; i++)
        {
            GameObject button = CreateKingdomFilterButton(choices[i]);
            if (button != null) KingdomFilterButtons.Add(button);
        }
        RefreshDynamicGridLayout(_kingdomFilterContainer);
    }

    private static GameObject CreateKingdomFilterButton(MclslRankKingdomFilterChoice choice)
    {
        Kingdom kingdom = choice?.Kingdom;
        if (kingdom?.data == null || _kingdomFilterContainer == null) return null;
        var colorAsset = kingdom.getColor();
        Color mainColor = colorAsset?.getColorMainSecond() ?? Color.gray;
        Color bannerColor = colorAsset?.getColorBanner() ?? Color.white;
        Sprite background = GetSafeKingdomBackground(kingdom);
        Sprite icon = GetSafeKingdomIcon(kingdom);
        Image image = CreateImage("Kingdom_" + kingdom.data.id.ToString(CultureInfo.InvariantCulture), _kingdomFilterContainer, background, mainColor, new Vector2(25, 25));
        image.type = Image.Type.Sliced;
        Image inner = CreateImage("Icon", image.transform, icon, bannerColor, Vector2.zero);
        inner.preserveAspect = true;
        inner.raycastTarget = false;
        SetRect(inner.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(3, 3), offsetMax: new Vector2(-3, -3));
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        long kingdomId = choice.KingdomId;
        string displayName = string.IsNullOrWhiteSpace(choice.DisplayName) ? "未知国家" : choice.DisplayName;
        button.onClick.AddListener(() => AddFilter(
            "kingdom_" + kingdomId.ToString(CultureInfo.InvariantCulture),
            displayName,
            background,
            icon,
            mainColor,
            bannerColor,
            actor => actor?.kingdom?.data != null && actor.kingdom.data.id == kingdomId));
        image.gameObject.AddComponent<MclslRankTooltipTrigger>().TooltipText = displayName;
        return image.gameObject;
    }

    private static void RefreshAssetFilters(IReadOnlyList<MclslRankAssetFilterChoice> choices)
    {
        ClearObjectList(AssetFilterButtons);
        if (_assetFilterContainer == null) return;
        choices ??= Array.Empty<MclslRankAssetFilterChoice>();
        for (int i = 0; i < choices.Count; i++)
        {
            GameObject button = CreateAssetFilterButton(choices[i]);
            if (button != null) AssetFilterButtons.Add(button);
        }
        RefreshDynamicGridLayout(_assetFilterContainer);
    }

    private static GameObject CreateAssetFilterButton(MclslRankAssetFilterChoice choice)
    {
        ActorAsset asset = choice?.Asset;
        if (asset == null || _assetFilterContainer == null) return null;
        Sprite icon = GetAssetIcon(asset);
        string displayName = string.IsNullOrWhiteSpace(choice.DisplayName) ? GetAssetDisplayName(asset) : choice.DisplayName;
        Image image = CreateImage("Asset_" + asset.id, _assetFilterContainer, icon, Color.white, new Vector2(25, 25));
        image.preserveAspect = true;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        string assetId = asset.id;
        button.onClick.AddListener(() => AddFilter(
            "asset_" + assetId,
            displayName,
            icon,
            null,
            Color.white,
            Color.white,
            actor => actor?.asset != null && string.Equals(actor.asset.id, assetId, StringComparison.Ordinal)));
        image.gameObject.AddComponent<MclslRankTooltipTrigger>().TooltipText = displayName;
        return image.gameObject;
    }

    private static void RefreshTraitFilters(IReadOnlyList<MclslRankTraitFilterChoice> choices)
    {
        ClearObjectList(TraitFilterButtons);
        if (_traitFilterContainer == null) return;
        choices ??= Array.Empty<MclslRankTraitFilterChoice>();
        for (int i = 0; i < choices.Count; i++)
        {
            ActorTrait trait = AssetManager.traits?.get(choices[i].TraitId);
            if (trait == null) continue;
            GameObject button = CreateTraitFilterButton(trait, choices[i].DisplayName);
            if (button != null) TraitFilterButtons.Add(button);
        }
        RefreshDynamicGridLayout(_traitFilterContainer);
    }

    private static GameObject CreateTraitFilterButton(ActorTrait trait, string snapshotDisplayName)
    {
        if (trait == null || _traitFilterContainer == null) return null;
        if (MclslLocalizationBridge.IsRuntimeKey(trait.id)) return null;
        Sprite icon = string.IsNullOrWhiteSpace(trait.path_icon) ? null : SpriteTextureLoader.getSprite(trait.path_icon);
        icon ??= GetSafeSprite("ui/icons/iconQuestionMark");
        string displayName = string.IsNullOrWhiteSpace(snapshotDisplayName) ? GetTraitDisplayName(trait) : snapshotDisplayName;
        if (MclslLocalizationBridge.IsRuntimeKey(displayName)) return null;
        Image image = CreateImage("Trait_" + trait.id, _traitFilterContainer, icon, Color.white, new Vector2(25, 25));
        image.preserveAspect = true;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        string traitId = trait.id;
        string traitName = displayName;
        button.onClick.AddListener(() => AddFilter(
            "trait_" + traitId,
            traitName,
            icon,
            null,
            Color.white,
            Color.white,
            actor => actor != null && actor.hasTrait(traitId)));
        image.gameObject.AddComponent<MclslRankTooltipTrigger>().TooltipText = traitName;
        return image.gameObject;
    }

    private static void AddFilter(string id, string displayName, Sprite icon, Sprite innerIcon, Color iconColor, Color innerColor, Func<Actor, bool> filterFunc)
    {
        if (string.IsNullOrWhiteSpace(id) || filterFunc == null || ActiveFilters.Exists(filter => !filter.ToBeRemoved && string.Equals(filter.Id, id, StringComparison.Ordinal))) return;
        ActiveFilters.Add(new MclslRankFilterSetting(id, displayName, icon, innerIcon, iconColor, innerColor, filterFunc));
        RefreshSelectedFilterButtons();
        RefreshCurrentList(true);
    }

    private static void RefreshSelectedFilterButtons()
    {
        ClearObjectList(SelectedFilterButtons);
        ActiveFilters.RemoveAll(filter => filter == null || filter.ToBeRemoved);
        if (_selectedFilterContainer == null) return;
        for (int i = 0; i < ActiveFilters.Count; i++)
            SelectedFilterButtons.Add(CreateSelectedFilterButton(ActiveFilters[i]));
        RefreshDynamicGridLayout(_selectedFilterContainer);
    }

    private static GameObject CreateSelectedFilterButton(MclslRankFilterSetting filter)
    {
        Image border = CreateImage(filter.Id, _selectedFilterContainer, GetSafeSprite("ui/special/windowInnerSliced"), filter.GetTypeColor(), new Vector2(25, 25));
        border.type = Image.Type.Sliced;
        Image icon = CreateImage("Icon", border.transform, filter.Icon ?? GetSafeSprite("ui/icons/iconQuestionMark"), filter.IconColor, Vector2.zero);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        SetRect(icon.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(2, 2), offsetMax: new Vector2(-2, -2));
        if (filter.InnerIcon != null)
        {
            Image inner = CreateImage("Inner", icon.transform, filter.InnerIcon, filter.InnerColor, Vector2.zero);
            inner.preserveAspect = true;
            inner.raycastTarget = false;
            SetRect(inner.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(2, 2), offsetMax: new Vector2(-2, -2));
        }
        Image typeBackground = CreateImage("Type", border.transform, GetSafeSprite("ui/special/darkInputFieldEmpty"), filter.GetTypeColor(), new Vector2(13, 13));
        typeBackground.type = Image.Type.Sliced;
        typeBackground.raycastTarget = false;
        SetRect(typeBackground.gameObject, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(-1, 1), new Vector2(13, 13));
        Text typeText = CreateText("Text", typeBackground.transform, filter.GetTypeText(), 8, Color.white);
        SetRect(typeText.gameObject, Vector2.zero, Vector2.one, offsetMin: Vector2.zero, offsetMax: Vector2.zero);
        Button button = border.gameObject.AddComponent<Button>();
        button.targetGraphic = border;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() =>
        {
            filter.CycleType();
            border.color = filter.GetTypeColor();
            typeBackground.color = filter.GetTypeColor();
            typeText.text = filter.GetTypeText();
            RefreshCurrentList(true);
        });
        MclslRankRightClickHandler rightClick = border.gameObject.AddComponent<MclslRankRightClickHandler>();
        rightClick.OnRightClick = () =>
        {
            filter.ToBeRemoved = true;
            RefreshSelectedFilterButtons();
            RefreshCurrentList(true);
        };
        MclslRankTooltipTrigger tip = border.gameObject.AddComponent<MclslRankTooltipTrigger>();
        tip.TooltipText = filter.DisplayName;
        tip.TooltipDescription = "左键切换 与/或/非，右键移除";
        return border.gameObject;
    }

    private static void RefreshDynamicGridLayout(Transform gridTransform)
    {
        if (gridTransform == null) return;
        RectTransform gridRect = gridTransform as RectTransform ?? gridTransform.GetComponent<RectTransform>();
        GridLayoutGroup layout = gridTransform.GetComponent<GridLayoutGroup>();
        LayoutElement element = gridTransform.GetComponent<LayoutElement>();
        if (gridRect == null || layout == null || element == null) return;
        int columns = Math.Max(1, layout.constraintCount);
        int rows = Math.Max(1, Mathf.CeilToInt(gridTransform.childCount / (float)columns));
        float height = rows * layout.cellSize.y
            + Math.Max(0, rows - 1) * layout.spacing.y
            + layout.padding.top
            + layout.padding.bottom;
        element.minHeight = height;
        element.preferredHeight = height;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
        if (gridTransform.parent is RectTransform parentRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
    }

    private static void CreateAvailableSortButtons()
    {
        ClearObjectList(AvailableSortButtons);
        if (_availableSortContainer == null) return;
        for (int i = 0; i < SortDefs.Length; i++)
            AvailableSortButtons.Add(CreateSortIconButton(SortDefs[i], _availableSortContainer, null));
    }

    private static GameObject CreateSortIconButton(MclslRankSortDef definition, Transform parent, MclslRankSortKey selectedKey)
    {
        Image image = CreateImage("Sort_" + definition.Id, parent, GetSafeSprite(definition.IconPath), Color.white, new Vector2(25, 25));
        image.preserveAspect = true;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        if (selectedKey == null)
        {
            button.onClick.AddListener(() =>
            {
                if (ActiveSortKeys.Exists(k => string.Equals(k.Def.Id, definition.Id, StringComparison.Ordinal))) return;
                ActiveSortKeys.Add(new MclslRankSortKey(definition));
                RefreshSelectedSortButtons();
                RefreshCurrentList(false);
            });
        }
        else
        {
            button.onClick.AddListener(() =>
            {
                selectedKey.Toggle();
                RefreshSelectedSortButtons();
                RefreshCurrentList(false);
            });
            Image arrow = CreateImage("Arrow", image.transform, GetSafeSprite(selectedKey.Ascending ? "ui/icons/iconArrowUP" : "ui/icons/iconArrowDOWN"), Color.white, new Vector2(11, 11));
            arrow.raycastTarget = false;
            SetRect(arrow.gameObject, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(-1, -1), new Vector2(11, 11));
            MclslRankRightClickHandler rightClick = image.gameObject.AddComponent<MclslRankRightClickHandler>();
            rightClick.OnRightClick = () =>
            {
                ActiveSortKeys.Remove(selectedKey);
                RefreshSelectedSortButtons();
                RefreshCurrentList(false);
            };
        }
        MclslRankTooltipTrigger tip = image.gameObject.AddComponent<MclslRankTooltipTrigger>();
        tip.TooltipText = selectedKey == null ? definition.Name : definition.Name + (selectedKey.Ascending ? "（升序）" : "（降序）");
        tip.TooltipDescription = selectedKey == null ? "左键加入排序" : "左键切换升降序，右键移除";
        return image.gameObject;
    }

    private static void RefreshSelectedSortButtons()
    {
        ClearObjectList(SelectedSortButtons);
        if (_selectedSortContainer == null) return;
        for (int i = 0; i < ActiveSortKeys.Count; i++)
            SelectedSortButtons.Add(CreateSortIconButton(ActiveSortKeys[i].Def, _selectedSortContainer, ActiveSortKeys[i]));
    }

    private static void ClearSortKeys()
    {
        ActiveSortKeys.Clear();
        RefreshSelectedSortButtons();
        RefreshCurrentList(false);
    }

    private static void ClearFilters()
    {
        ActiveFilters.Clear();
        _rootFilter = 0;
        _realmFilter = 0;
        _searchQuery = string.Empty;
        _rootDropdown?.SetValueWithoutNotify(0);
        _realmDropdown?.SetValueWithoutNotify(0);
        _rootDropdown?.RefreshShownValue();
        _realmDropdown?.RefreshShownValue();
        _searchInput?.SetTextWithoutNotify(string.Empty);
        RefreshSelectedFilterButtons();
        RefreshCurrentList(true);
    }

    private static void ToTop()
    {
        if (_contentRect != null) _contentRect.anchoredPosition = Vector2.zero;
    }

    private static void ToBottom()
    {
        if (_contentRect == null || _scrollViewRect == null) return;
        float max = Math.Max(0f, _contentRect.sizeDelta.y - _scrollViewRect.rect.height);
        _contentRect.anchoredPosition = new Vector2(0, max);
    }

    private static void ClearCards()
    {
        ClearObjectList(CardInstances);
        CardByIndex.Clear();
        CardIndexBuffer.Clear();
        _lastViewStart = int.MaxValue;
        _lastViewEnd = -1;
    }

    private static void ClearObjectList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i] != null) UnityEngine.Object.Destroy(list[i]);
        list.Clear();
    }

    private static Transform CreateTitledGrid(Transform parent, string title, int cellSize, int columns, bool collapsible)
    {
        GameObject container = new("Grid_" + title, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        container.transform.SetParent(parent, false);
        SetRect(container, size: Vector2.zero);
        VerticalLayoutGroup containerLayout = container.GetComponent<VerticalLayoutGroup>();
        containerLayout.childControlHeight = true;
        containerLayout.childControlWidth = true;
        containerLayout.childForceExpandHeight = false;
        containerLayout.childForceExpandWidth = true;
        containerLayout.spacing = 2f;
        containerLayout.padding = new RectOffset(0, 0, 0, 5);
        container.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject titleBar = new("TitleBar", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        titleBar.transform.SetParent(container.transform, false);
        SetRect(titleBar, size: new Vector2(0, 20f));
        Image titleImage = titleBar.GetComponent<Image>();
        titleImage.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        titleImage.type = Image.Type.Sliced;
        titleImage.color = new Color(0.25f, 0.25f, 0.25f, 0.9f);
        titleBar.GetComponent<LayoutElement>().preferredHeight = 20f;
        Text titleText = CreateText("Title", titleBar.transform, title, 10, new Color(1f, 0.9f, 0.7f));
        titleText.alignment = TextAnchor.MiddleLeft;
        SetRect(titleText.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(5, 0), offsetMax: new Vector2(-42, 0));

        GameObject gridContainer = new("GridContainer", typeof(RectTransform), typeof(Image), typeof(ContentSizeFitter), typeof(VerticalLayoutGroup));
        gridContainer.transform.SetParent(container.transform, false);
        SetRect(gridContainer, size: Vector2.zero);
        Image gridImage = gridContainer.GetComponent<Image>();
        gridImage.sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        gridImage.type = Image.Type.Sliced;
        gridImage.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        gridContainer.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        VerticalLayoutGroup gridContainerLayout = gridContainer.GetComponent<VerticalLayoutGroup>();
        gridContainerLayout.childControlHeight = true;
        gridContainerLayout.childControlWidth = true;
        gridContainerLayout.childForceExpandHeight = false;
        gridContainerLayout.padding = new RectOffset(3, 3, 3, 3);

        GameObject grid = new("Grid", typeof(RectTransform), typeof(LayoutElement), typeof(ContentSizeFitter), typeof(GridLayoutGroup));
        grid.transform.SetParent(gridContainer.transform, false);
        SetRect(grid, size: Vector2.zero);
        grid.GetComponent<LayoutElement>().minHeight = cellSize + 6f;
        grid.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        GridLayoutGroup gridLayout = grid.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(cellSize, cellSize);
        gridLayout.spacing = new Vector2(3f, 3f);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;

        if (collapsible)
        {
            Text toggleText = CreateText("Toggle", titleBar.transform, "[折叠]", 9, new Color(0.7f, 0.7f, 0.7f));
            toggleText.alignment = TextAnchor.MiddleRight;
            SetRect(toggleText.gameObject, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-5, 0), new Vector2(50, 0));
            Button toggle = titleBar.AddComponent<Button>();
            toggle.targetGraphic = titleImage;
            toggle.transition = Selectable.Transition.None;
            toggle.onClick.AddListener(() =>
            {
                bool wasActive = gridContainer.activeSelf;
                gridContainer.SetActive(!wasActive);
                toggleText.text = wasActive ? "[展开]" : "[折叠]";
                Canvas.ForceUpdateCanvases();
                if (container.transform is RectTransform containerRect) LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            });
            titleBar.AddComponent<MclslRankTooltipTrigger>().TooltipText = "点击展开或折叠";
        }
        else
        {
            gridContainer.SetActive(true);
        }
        return grid.transform;
    }

    private static void CreateSearchSection(Transform parent)
    {
        GameObject container = new("ActorSearchSection", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        container.transform.SetParent(parent, false);
        VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 4f;
        layout.padding = new RectOffset(0, 0, 2, 5);
        container.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject titleBar = new("SearchTitle", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        titleBar.transform.SetParent(container.transform, false);
        Image titleImage = titleBar.GetComponent<Image>();
        titleImage.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        titleImage.type = Image.Type.Sliced;
        titleImage.color = new Color(0.25f, 0.25f, 0.25f, 0.75f);
        titleBar.GetComponent<LayoutElement>().preferredHeight = 20f;
        Text titleText = CreateText("Title", titleBar.transform, "角色搜索", 10, new Color(1f, 0.9f, 0.7f));
        titleText.alignment = TextAnchor.MiddleLeft;
        SetRect(titleText.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(5, 0), offsetMax: new Vector2(-5, 0));

        GameObject inputObj = new("ActorSearchInput", typeof(RectTransform), typeof(Image), typeof(InputField), typeof(LayoutElement));
        inputObj.transform.SetParent(container.transform, false);
        Image inputImage = inputObj.GetComponent<Image>();
        inputImage.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        inputImage.type = Image.Type.Sliced;
        inputImage.color = new Color(0.05f, 0.05f, 0.05f, 0.85f);
        inputObj.GetComponent<LayoutElement>().preferredHeight = 26f;
        Text inputText = CreateText("Text", inputObj.transform, string.Empty, 10, Color.white);
        inputText.alignment = TextAnchor.MiddleLeft;
        SetRect(inputText.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(8, 0), offsetMax: new Vector2(-8, 0));
        Text placeholder = CreateText("Placeholder", inputObj.transform, "输入角色名字", 10, new Color(0.7f, 0.7f, 0.7f, 0.75f));
        placeholder.alignment = TextAnchor.MiddleLeft;
        SetRect(placeholder.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(8, 0), offsetMax: new Vector2(-8, 0));
        _searchInput = inputObj.GetComponent<InputField>();
        _searchInput.textComponent = inputText;
        _searchInput.placeholder = placeholder;
        _searchInput.lineType = InputField.LineType.SingleLine;
        _searchInput.characterLimit = 24;
        _searchInput.onValueChanged.AddListener(value =>
        {
            _searchQuery = value ?? string.Empty;
            RefreshCurrentList(true);
        });
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color, Vector2 size)
    {
        GameObject obj = new(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        if (size != Vector2.zero) SetRect(obj, size: size);
        return image;
    }

    private static Sprite GetSafeSprite(string path)
    {
        Sprite sprite = string.IsNullOrWhiteSpace(path) ? null : SpriteTextureLoader.getSprite(path);
        return sprite ?? SpriteTextureLoader.getSprite("ui/icons/iconQuestionMark");
    }

    private static Sprite GetSafeKingdomBackground(Kingdom kingdom)
    {
        try
        {
            Sprite sprite = kingdom?.getElementBackground();
            if (sprite != null) return sprite;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-3", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs #3: " + mclslEmptyCatchEx.Message); }
        return GetSafeSprite("ui/special/backgroundKingdomElement");
    }

    private static Sprite GetSafeKingdomIcon(Kingdom kingdom)
    {
        try
        {
            Sprite sprite = kingdom?.getElementIcon();
            if (sprite != null) return sprite;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-4", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs #4: " + mclslEmptyCatchEx.Message); }
        return GetSafeSprite("ui/icons/iconQuestionMark");
    }

    private static Sprite GetAssetIcon(ActorAsset asset)
    {
        if (asset == null) return GetSafeSprite("ui/icons/iconQuestionMark");
        try
        {
            Sprite sprite = asset.getSpriteIcon();
            if (sprite != null) return sprite;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-5", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs #5: " + mclslEmptyCatchEx.Message); }
        try
        {
            string path = asset.getIconPath();
            if (!string.IsNullOrWhiteSpace(path))
            {
                Sprite sprite = SpriteTextureLoader.getSprite(path);
                if (sprite != null) return sprite;
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-6", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs #6: " + mclslEmptyCatchEx.Message); }
        if (!string.IsNullOrWhiteSpace(asset.icon))
        {
            Sprite sprite = SpriteTextureLoader.getSprite("ui/icons/" + asset.icon);
            if (sprite != null) return sprite;
        }
        return GetSafeSprite("ui/icons/iconQuestionMark");
    }

    private static string GetAssetDisplayName(ActorAsset asset)
    {
        if (asset == null) return "未知生物";
        try
        {
            string name = asset.getLocalizedName();
            if (!string.IsNullOrWhiteSpace(name)) return name;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-7", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs #7: " + mclslEmptyCatchEx.Message); }
        try
        {
            string name = LM.Get(asset.name_locale);
            if (!string.IsNullOrWhiteSpace(name)) return name;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-8", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs #8: " + mclslEmptyCatchEx.Message); }
        return "未知生物";
    }

    private static string GetTraitDisplayName(ActorTrait trait)
    {
        if (trait == null || string.IsNullOrWhiteSpace(trait.id)) return "未知特质";
        if (MclslLocalizationBridge.TryResolveRuntimeKey(trait.id, out string runtimeText)) return runtimeText;
        try
        {
            string localized = LM.Get("trait_" + trait.id);
            if (!string.IsNullOrWhiteSpace(localized)
                && !string.Equals(localized, "trait_" + trait.id, StringComparison.Ordinal)
                && !MclslLocalizationBridge.IsRuntimeKey(localized))
                return localized;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-9", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs #9: " + mclslEmptyCatchEx.Message); }
        try
        {
            string localized = LM.Get(trait.id);
            if (!string.IsNullOrWhiteSpace(localized)
                && !string.Equals(localized, trait.id, StringComparison.Ordinal)
                && !MclslLocalizationBridge.IsRuntimeKey(localized))
                return localized;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-10", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslRankWindow.cs #10: " + mclslEmptyCatchEx.Message); }
        return trait.id;
    }

    private static Button CreateNavButton(Transform parent, string name, string text, Vector2 anchor, Action onClick)
    {
        Button button = CreateButton(parent, name, text, new Vector2(58, 22), onClick);
        SetRect(button.gameObject, anchor, anchor, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(58, 22));
        return button;
    }

    private static Button CreateButton(Transform parent, string name, string text, Vector2 size, Action onClick, Vector2? pos = null)
    {
        GameObject obj = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
        image.type = Image.Type.Sliced;
        SetRect(obj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), pos ?? Vector2.zero, size);
        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => onClick?.Invoke());
        Text label = CreateText("Text", obj.transform, text, 9, Color.white);
        SetRect(label.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(2, 0), offsetMax: new Vector2(-2, 0));
        return button;
    }

    private static Dropdown CreateDropdown(string name, Transform parent, string[] options, Action<int> onChange)
    {
        GameObject obj = new(name, typeof(RectTransform), typeof(Image), typeof(Dropdown), typeof(LayoutElement));
        obj.transform.SetParent(parent, false);
        Image bg = obj.GetComponent<Image>();
        bg.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
        bg.type = Image.Type.Sliced;
        Dropdown dropdown = obj.GetComponent<Dropdown>();
        for (int i = 0; i < options.Length; i++) dropdown.options.Add(new Dropdown.OptionData(options[i]));
        dropdown.onValueChanged.AddListener(index => onChange(index));
        Text label = CreateText("Label", obj.transform, string.Empty, 10, Color.white);
        SetRect(label.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(5, 0), offsetMax: new Vector2(-20, 0));
        dropdown.captionText = label;
        GameObject template = CreateDropdownTemplate(obj.transform, bg.sprite);
        dropdown.template = template.GetComponent<RectTransform>();
        dropdown.itemText = template.transform.Find("Viewport/Content/Item/Item Label")?.GetComponent<Text>();
        template.SetActive(false);
        dropdown.RefreshShownValue();
        obj.GetComponent<LayoutElement>().preferredHeight = 25f;
        return dropdown;
    }

    private static GameObject CreateDropdownTemplate(Transform parent, Sprite sprite)
    {
        GameObject template = new("Template", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        template.transform.SetParent(parent, false);
        SetRect(template, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 120));
        Image bg = template.GetComponent<Image>();
        bg.sprite = sprite;
        bg.type = Image.Type.Sliced;
        template.GetComponent<Mask>().showMaskGraphic = true;
        GameObject viewport = new("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(template.transform, false);
        RectTransform viewportRect = SetRect(viewport, Vector2.zero, Vector2.one, offsetMin: Vector2.zero, offsetMax: Vector2.zero);
        GameObject content = new("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRectLocal = SetRect(content, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
        content.GetComponent<VerticalLayoutGroup>().childControlHeight = true;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        GameObject item = new("Item", typeof(RectTransform), typeof(Toggle), typeof(LayoutElement));
        item.transform.SetParent(content.transform, false);
        SetRect(item, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), size: new Vector2(0, 20));
        item.GetComponent<LayoutElement>().preferredHeight = 20;
        Text itemText = CreateText("Item Label", item.transform, string.Empty, 9, Color.white);
        SetRect(itemText.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(5, 0), offsetMax: new Vector2(-5, 0));
        ScrollRect scroll = template.GetComponent<ScrollRect>();
        scroll.content = contentRectLocal;
        scroll.viewport = viewportRect;
        scroll.horizontal = false;
        return template;
    }

    private static GameObject CreateScrollView(Transform parent, string name, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject scrollObj = new(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);
        SetRect(scrollObj, Vector2.zero, Vector2.one, offsetMin: offsetMin, offsetMax: offsetMax);
        scrollObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.05f);
        ScrollRect scroll = scrollObj.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 20f;
        GameObject viewport = new("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRect = SetRect(viewport, Vector2.zero, Vector2.one, offsetMin: Vector2.zero, offsetMax: Vector2.zero);
        viewport.GetComponent<Image>().color = Color.white;
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        GameObject content = new("Content", typeof(RectTransform), typeof(ContentSizeFitter), typeof(VerticalLayoutGroup));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRectLocal = SetRect(content, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), size: Vector2.zero);
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 5f;
        layout.padding = new RectOffset(0, 0, 5, 5);
        scroll.viewport = viewportRect;
        scroll.content = contentRectLocal;
        return scrollObj;
    }

    private static void CreateSectionTitle(Transform parent, string text, Vector2 pos)
    {
        Text title = CreateText("Title", parent, text, 10, new Color(1f, 0.84f, 0f));
        SetRect(title.gameObject, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), pos, new Vector2(0, 16));
    }

    private static Text CreateText(string name, Transform parent, string text, int fontSize, Color color)
    {
        GameObject obj = new(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        Text label = obj.GetComponent<Text>();
        label.font = LocalizedTextManager.current_font ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;
        label.text = text ?? string.Empty;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        return label;
    }

    private static RectTransform SetRect(GameObject obj, Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? pivot = null, Vector2? pos = null, Vector2? size = null, Vector2? offsetMin = null, Vector2? offsetMax = null)
    {
        RectTransform rect = obj.GetComponent<RectTransform>() ?? obj.AddComponent<RectTransform>();
        if (anchorMin.HasValue) rect.anchorMin = anchorMin.Value;
        if (anchorMax.HasValue) rect.anchorMax = anchorMax.Value;
        if (pivot.HasValue) rect.pivot = pivot.Value;
        if (pos.HasValue) rect.anchoredPosition = pos.Value;
        if (size.HasValue) rect.sizeDelta = size.Value;
        if (offsetMin.HasValue) rect.offsetMin = offsetMin.Value;
        if (offsetMax.HasValue) rect.offsetMax = offsetMax.Value;
        return rect;
    }

    private static void SetButtonColor(Button button, Color color)
    {
        Image image = button == null ? null : button.GetComponent<Image>();
        if (image != null) image.color = color;
    }

    private static string FormatNumber(double value)
    {
        if (value >= 100000000d) return (value / 100000000d).ToString("0.##", CultureInfo.InvariantCulture) + "亿";
        if (value >= 10000d) return (value / 10000d).ToString("0.##", CultureInfo.InvariantCulture) + "万";
        return value.ToString("0", CultureInfo.InvariantCulture);
    }

}
