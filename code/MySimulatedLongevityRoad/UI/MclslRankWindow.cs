using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslRankWindow
{
    private const float WindowWidth = 720f;
    private const float WindowHeight = 408f;
    private const float LeftPanelWidth = 110f;
    private const float RightPanelWidth = 110f;
    private const float CenterWidth = 410f;
    private const float FrameTopExtension = 32f;
    // The artwork already extends 14 units below the window; add 10 more.
    private const float FrameBottomExtension = 24f;
    private const float PanelTopInset = 33f;
    private const float PanelBottomInset = 15f;
    private const float PanelCenterY = (PanelBottomInset - PanelTopInset) * 0.5f;
    private const float PanelHeightDelta = -(PanelTopInset + PanelBottomInset);
    private const float RankControlsLift = 44f;
    private const float RankListTop = 153f - RankControlsLift;
    private const float RankListBottom = 44f;
    private const float ListHeight = WindowHeight - RankListTop - RankListBottom;
    private const float CardHeight = 35f;
    private const int SideGridCellSize = 32;
    private const float NameColumnX = 57f;
    private const float NameColumnWidth = 106f;
    private const float RealmColumnX = 168f;
    private const float RealmColumnWidth = 58f;
    private const float PowerColumnX = 230f;
    private const float PowerColumnWidth = 62f;
    private const float OriginColumnX = 297f;
    private const float OriginColumnWidth = 86f;
    private static readonly string[] ModTraitGroupOrder =
    {
        MclslTraitRegistration.RealmGroupId,
        MclslTraitRegistration.AncientRealmGroupId,
        MclslTraitRegistration.GiftGroupId,
        MclslTraitRegistration.ProfessionGroupId,
        MclslTraitRegistration.ImmortalPathGroupId,
        MclslTraitRegistration.XuanHuangHeavenGroupId,
        MclslTraitRegistration.SpecialGroupId
    };

    private static readonly string[] RealmOptions = { "全部境界", "感气", "炼气", "筑基", "金丹", "元婴", "化神", "合道", "长生" };
    private static readonly string[] RootOptions = { "全部灵根", "金", "木", "水", "火", "土", "风", "雷", "阴", "阳", "空间" };
    private static readonly List<MclslRankSortKey> ActiveSortKeys = new();
    private static MclslOrderedIdIndex<MclslRankEntry> Entries = new((a, b) => b.Power.CompareTo(a.Power));
    private static MclslVirtualCardList _rows;
    private static readonly List<MclslRankFilterSetting> ActiveFilters = new();
    private static readonly List<GameObject> KingdomFilterButtons = new();
    private static readonly List<GameObject> AssetFilterButtons = new();
    private static readonly List<GameObject> TraitFilterButtons = new();
    private static readonly List<Transform> TraitFilterGrids = new();

    // Selected filter/sort chips are reused by stable key instead of being
    // destroyed and recreated on every refresh. The dictionaries survive the
    // window being hidden and reopened because the ScrollWindow instance is kept.
    private sealed class FilterButtonView
    {
        internal GameObject Root;
        internal MclslRankFilterSetting Filter;
        internal Image Border;
        internal Image Icon;
        internal Image Inner;
        internal Image TypeBackground;
        internal Text TypeText;
        internal MclslRankTooltipTrigger Tip;
    }
    private sealed class SortButtonView
    {
        internal string Id;
        internal MclslRankSortDef Def;
        internal GameObject Root;
        internal Image Arrow;
        internal MclslRankTooltipTrigger Tip;
        internal bool Selected;
        internal bool HasVisual;
        internal bool LastAscending;
    }
    private static readonly Dictionary<string, FilterButtonView> SelectedFilterButtonViews = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, SortButtonView> SelectedSortButtonViews = new(StringComparer.Ordinal);
    private static readonly List<SortButtonView> AvailableSortButtonViews = new();
    private static readonly HashSet<string> _activeFilterIds = new(StringComparer.Ordinal);
    private static readonly HashSet<string> _activeSortIds = new(StringComparer.Ordinal);
    private static readonly List<string> _removalScratch = new();

    // Lightweight layout batching: RefreshDynamicGridLayout only records the new
    // height and marks the owning scroll content dirty; one flush per operation
    // round rebuilds the affected wrappers/content and clamps exactly once.
    private struct DirtyScrollContent
    {
        internal ScrollRect Scroll;
        internal float PreviousOffset;
    }
    private static readonly List<Transform> _dirtyGridInners = new();
    private static readonly HashSet<RectTransform> DirtyLayoutRects = new();
    private static readonly List<(RectTransform Rect, int Depth)> OrderedLayoutRects = new();
    private static readonly List<DirtyScrollContent> _dirtyScrollContents = new();
    private static int _layoutBatchDepth;
    private static string _kingdomFilterSignature;
    private static string _assetFilterSignature;
    private static string _traitFilterSignature;

    private static ScrollWindow _window;
    internal static bool IsVisible => _window != null && _window.gameObject.activeInHierarchy;
    private static Transform _nativeCloseRoot;
    private static RectTransform _contentRect;
    private static RectTransform _scrollViewRect;
    private static ScrollRect _rankScroll;
    private static Transform _contentTransform;
    private static Transform _selectedFilterContainer;
    private static Transform _kingdomFilterContainer;
    private static Transform _assetFilterContainer;
    private static Transform _selectedSortContainer;
    private static Transform _availableSortContainer;
    private static GameObject _emptyState;
    private static Text _countText;
    private static Text _rankSummaryText;
    private static Text _primarySortHeader;
    private static Dropdown _rootDropdown;
    private static Dropdown _realmDropdown;
    private static InputField _searchInput;
    private static GameObject _cardPrefab;
    private static int _rootFilter;
    private static int _realmFilter;
    private static string _searchQuery = string.Empty;
    private static bool _needRefresh;
    private static long _viewRevision = -1;
    private static MclslOrderedIdIndex<MclslRankEntry> NextEntries;
    private static Comparison<MclslRankEntry> _viewCompare = (a, b) => b.Power.CompareTo(a.Power);
    private static bool _viewDataDirty;
    private static int _filterCursor = -1;
    private static bool _resetPendingScroll;
    private static string _pendingQuery;
    private static string _orderSignature;
    private static bool _filterChoicesInitialized;

    private static readonly MclslRankSortDef[] SortDefs =
    {
        new("power", "战力", "ui/Icons/TianDiZhiLi", entry => (float)Math.Min(float.MaxValue, entry.Power), entry => FormatNumber(entry.Power)),
        new("realm", "境界", "trait/realm_7", entry => entry.RealmSortRank, entry => entry.RealmName),
        new("root", "灵根品阶", "trait/gifts_6", entry => entry.Aptitude, entry => string.IsNullOrWhiteSpace(entry.GiftName) ? entry.Aptitude.ToString(CultureInfo.InvariantCulture) : entry.GiftName),
        new("profession_alchemist", "炼丹师", "trait/MclslProfessionAlchemist", entry => ProfessionSortRank(entry, MclslProfessionSystem.Alchemist),
            entry => FormatProfession(entry, MclslProfessionSystem.Alchemist)),
        new("profession_refiner", "炼器师", "trait/MclslProfessionRefiner", entry => ProfessionSortRank(entry, MclslProfessionSystem.Refiner),
            entry => FormatProfession(entry, MclslProfessionSystem.Refiner)),
        new("profession_talisman", "制符师", "trait/MclslProfessionTalisman", entry => ProfessionSortRank(entry, MclslProfessionSystem.TalismanMaker),
            entry => FormatProfession(entry, MclslProfessionSystem.TalismanMaker)),
        // 六大战斗职业：各自一栏，选中后按该职业阶位从高到低排列，无该职业者沉底。
        new("career_sword", "剑修", "trait/MclslCombatCareer_sword", entry => CombatCareerSortRank(entry, 0),
            entry => FormatCombatCareer(entry, 0)),
        new("career_martial", "武修", "trait/MclslCombatCareer_martial", entry => CombatCareerSortRank(entry, 1),
            entry => FormatCombatCareer(entry, 1)),
        new("career_elemental", "术修", "trait/MclslCombatCareer_elemental", entry => CombatCareerSortRank(entry, 2),
            entry => FormatCombatCareer(entry, 2)),
        new("career_sigil", "符修", "trait/MclslCombatCareer_sigil", entry => CombatCareerSortRank(entry, 3),
            entry => FormatCombatCareer(entry, 3)),
        new("career_array", "阵修", "trait/MclslCombatCareer_array", entry => CombatCareerSortRank(entry, 4),
            entry => FormatCombatCareer(entry, 4)),
        new("career_spirit", "灵契师", "trait/MclslCombatCareer_spirit", entry => CombatCareerSortRank(entry, 5),
            entry => FormatCombatCareer(entry, 5)),
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
        MclslRankSnapshotSource.RequestVisibleUpdate();
        // Wrap the whole create+show round so every grid touched while building
        // the window collapses into a single layout flush.
        BeginLayoutBatch();
        try
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
            MclslWindowOpenGuard.Show(_window, "MclslRank", created, updateCanvas: false);
            MarkGridLayoutDirty(_selectedSortContainer);
            ApplyWindowScale();
        }
        finally { EndLayoutBatch(); }
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
        _window = WindowCreator.CreateEmptyWindow("MclslRank", "mclsl.rank", "ui/Icons/XuanHuangXiuShiBang");
        if (_window == null) return false;
        // A destroyed window leaves stale references behind; drop cached button
        // views and pending layout work before rebuilding the fresh hierarchy.
        // Do NOT reset _layoutBatchDepth here: EnsureWindow runs inside
        // ShowWindow's outer batch, and clobbering the depth would break nesting.
        ResetReusableButtons();
        _dirtyGridInners.Clear();
        _dirtyScrollContents.Clear();
        _kingdomFilterSignature = null;
        _assetFilterSignature = null;
        _traitFilterSignature = null;
        _nativeCloseRoot = _window.transform.Find("CloseBackground") ?? FindCloseButtonTransform();
        SetupWindowContent();
        RestoreNativeCloseButton();
        XianLuScreenGuardianOverlay.Ensure(_window.transform);
        _cardPrefab = CreateCardPrefab();
        _cardPrefab.transform.SetParent(_window.transform, false);
        _rows = new MclslVirtualCardList(_contentTransform, _contentRect, _cardPrefab, CardHeight, BindRow);
        MclslRankWindowUpdater updater = _window.gameObject.AddComponent<MclslRankWindowUpdater>();
        updater.OnOpen = () =>
        {
            XianLuScreenGuardianOverlay.Ensure(_window.transform);
            RestoreNativeCloseButton();
            ApplyWindowScale();
            BeginLayoutBatch();
            try
            {
                if (ShouldRefreshFilterChoicesOnOpen()) RefreshFilterChoices();
                RefreshSelectedFilterButtons();
                RefreshSelectedSortButtons();
            }
            finally { EndLayoutBatch(); }
            _needRefresh = true;
        };
        // The native close button is restored on creation/open.  Re-scanning all
        // of its child graphics every frame caused needless allocations while a
        // long ranking list was open, without changing its appearance.
        updater.OnUpdate = () => { ApplyWindowScale(); UpdateVisibleCards(); FlushDynamicGridLayout(); };
        updater.OnClose = () =>
        {
            _filterCursor = -1; NextEntries = null; Entries.Clear(); _rows?.Dispose();
            // Only drop pending layout when no batch is open. The first-show path
            // in MclslWindowOpenGuard toggles the object inactive inside
            // ShowWindow's outer batch, so an unconditional clear here would
            // discard still-unflushed grid heights from that same round.
            if (_layoutBatchDepth == 0)
            {
                _dirtyGridInners.Clear();
                _dirtyScrollContents.Clear();
            }
        };
        return true;
    }

    private static void RestoreNativeCloseButton()
    {
        if (_window == null) return;
        Transform closeRoot = _nativeCloseRoot;
        if (closeRoot == null) closeRoot = _window.transform.Find("CloseBackground") ?? FindCloseButtonTransform();
        if (closeRoot == null) return;
        _nativeCloseRoot = closeRoot;

        Transform oldGlyph = closeRoot.Find("MclslRankTransparentCloseGlyph");
        if (oldGlyph != null) UnityEngine.Object.Destroy(oldGlyph.gameObject);

        Transform frameArt = _window.transform.Find("Background/玄黄榜框图");
        if (frameArt != null)
        {
            if (closeRoot.parent != frameArt) closeRoot.SetParent(frameArt, false);
            if (closeRoot is RectTransform closeRect)
            {
                closeRect.anchorMin = Vector2.one;
                closeRect.anchorMax = Vector2.one;
                closeRect.pivot = new Vector2(0.5f, 0.5f);
                closeRect.anchoredPosition = new Vector2(-24f, -24f);
            }
            closeRoot.SetAsLastSibling();
        }
        else if (closeRoot is RectTransform fallbackRect)
        {
            fallbackRect.anchoredPosition = new Vector2(WindowWidth / 2f - 8f, WindowHeight / 2f - 8f);
        }

        Graphic[] graphics = closeRoot.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            if (graphic == null) continue;
            graphic.enabled = true;
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 1f);
            graphic.raycastTarget = graphic.transform == closeRoot || graphic.GetComponent<Button>() != null;
            try { graphic.canvasRenderer.SetAlpha(1f); }
            catch (Exception exception) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("rank-native-close", exception.Message); }
        }

        Button[] buttons = closeRoot.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            buttons[i].enabled = true;
            buttons[i].interactable = true;
        }
    }

    private static void ApplyWindowScale()
    {
        if (_window == null) return;
        Canvas canvas = _window.GetComponentInParent<Canvas>();
        float canvasScale = canvas != null && canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
        float usableWidth = Mathf.Max(1f, Screen.width / canvasScale - 24f);
        float usableHeight = Mathf.Max(1f, Screen.height / canvasScale - 12f);
        // Fit the actual decorated bounds; the old extra height allowance made
        // a window that still fit on screen appear smaller than the marked area.
        float scale = Mathf.Min(1.05f, usableWidth / WindowWidth,
            usableHeight / (WindowHeight + FrameTopExtension + FrameBottomExtension));
        _window.transform.localScale = Vector3.one * scale;
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
        // Batch the whole build so the left-panel filter refresh and the
        // right-panel available/selected sort refresh flush only once.
        BeginLayoutBatch();
        try
        {
            RectTransform windowRect = _window.GetComponent<RectTransform>();
            if (windowRect != null) windowRect.sizeDelta = new Vector2(WindowWidth, WindowHeight);
            Transform background = _window.transform.Find("Background");
            if (background == null) return;
            Image nativeBackground = background.GetComponent<Image>();
            if (nativeBackground != null) nativeBackground.color = Color.clear;
            RectTransform bgRect = background.GetComponent<RectTransform>();
            if (bgRect != null) bgRect.sizeDelta = new Vector2(WindowWidth, WindowHeight);
            CreateCustomBackplate(background);
            CreateLeftPanel(background);
            CreateRightPanel(background);
            CreateCenterPanel(background);
            // The controls sit inside the rails; keep panel fills from covering the extended lower corners.
            background.Find("玄黄榜框图")?.SetAsLastSibling();
            background.Find("榜首")?.SetAsLastSibling();
        }
        finally { EndLayoutBatch(); }
    }

    private static void CreateCustomBackplate(Transform parent)
    {
        GameObject surface = new("玄黄榜卷面", typeof(RectTransform), typeof(Image));
        surface.transform.SetParent(parent, false);
        // Meet the lower edge of the top rail so the game scene cannot show
        // through the gap above the three ranking panels.
        SetRect(surface, Vector2.zero, Vector2.one, offsetMin: new Vector2(32f, 7f), offsetMax: new Vector2(-32f, -18f));
        Image surfaceImage = surface.GetComponent<Image>();
        surfaceImage.sprite = null;
        surfaceImage.color = MclslUiTheme.RankSurfaceWindow;
        surfaceImage.raycastTarget = false;
        GameObject art = new("玄黄榜框图", typeof(RectTransform), typeof(Image));
        art.transform.SetParent(parent, false);
        SetRect(art, Vector2.zero, Vector2.one, offsetMin: new Vector2(0f, -FrameBottomExtension),
            offsetMax: new Vector2(0f, FrameTopExtension));
        XianLuUIResources.Apply(art.GetComponent<Image>(), "frame_ranking", Color.clear);
    }

    private static void CreatePanelAccent(Transform parent, Color color, float height)
    {
        GameObject accent = new("卷首饰线", typeof(RectTransform), typeof(Image));
        accent.transform.SetParent(parent, false);
        SetRect(accent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(-16f, height));
        Image image = accent.GetComponent<Image>();
        image.sprite = null;
        image.color = color;
        image.raycastTarget = false;
    }

    private static void CreateLeftPanel(Transform parent)
    {
        GameObject panel = new("LeftPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        SetRect(panel, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f),
            new Vector2(24f, PanelCenterY), new Vector2(LeftPanelWidth + 16f, PanelHeightDelta));
        panel.GetComponent<Image>().color = MclslUiTheme.RankSurfacePanel;
        panel.AddComponent<MclslRankRightClickHandler>().BlockRightClick = true;
        CreatePanelAccent(panel.transform, MclslUiTheme.RankAccentSecondary, 1.5f);
        CreateSectionTitle(panel.transform, "筛选", new Vector2(0, -6));

        GameObject scroll = CreateScrollView(panel.transform, "FilterScroll", new Vector2(6, 36), new Vector2(-6, -31));
        Transform filterContent = scroll.transform.Find("Viewport/Content");
        int sideColumns = ResolveSideGridColumns();
        _selectedFilterContainer = CreateTitledGrid(filterContent, "已选筛选", SideGridCellSize, sideColumns, false);
        _kingdomFilterContainer = CreateTitledGrid(filterContent, "国家", SideGridCellSize, sideColumns, true);
        _assetFilterContainer = CreateTitledGrid(filterContent, "种属", SideGridCellSize, sideColumns, true);
        TraitFilterGrids.Clear();
        Transform traits = CreateTitledGrid(filterContent, LM.Get("mclsl_rank_traits"), SideGridCellSize, sideColumns, true);
        traits.parent.parent.gameObject.SetActive(false);
        TraitFilterGrids.Add(traits);
        RefreshSelectedFilterButtons();
        Button clear = CreateButton(panel.transform, "ClearFilters", "清空全部", new Vector2(88, 20), ClearFilters);
        SetRect(clear.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(88, 20));
        SetButtonColor(clear, MclslUiTheme.RankDanger);
    }

    private static void CreateCenterPanel(Transform parent)
    {
        CreateRankTableFrame(parent);

        GameObject titlePanel = new("榜首", typeof(RectTransform), typeof(Image));
        titlePanel.transform.SetParent(parent, false);
        SetRect(titlePanel, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(WindowWidth - 190f, 68));
        titlePanel.GetComponent<Image>().color = Color.clear;
        titlePanel.GetComponent<Image>().raycastTarget = false;
        GameObject titleImage = new("RankTitleLogo", typeof(RectTransform), typeof(Image));
        titleImage.transform.SetParent(titlePanel.transform, false);
        // Preserve the original height and aspect ratio. The wider source crop
        // exposes both sides, while the higher anchor places the art on the crest.
        SetRect(titleImage, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, 24f + FrameTopExtension), new Vector2(160f, 30f));
        XianLuUIResources.Apply(titleImage.GetComponent<Image>(), "title_ranking", Color.clear);
        if (titleImage.GetComponent<Image>().sprite == null)
        {
            Text title = CreateText("RankTitleFallback", titlePanel.transform, LM.Get("mclsl_xianlu_title_ranking"), 15, MclslUiTheme.RankAccentSecondary);
            title.fontStyle = FontStyle.Bold;
            SetRect(title.gameObject, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, 24f + FrameTopExtension), new Vector2(160f, 30f));
        }
        _rankSummaryText = CreateText("RankSummary", titlePanel.transform, "总计入榜角色：0名", 8, MclslUiTheme.RankTextMuted);
        SetRect(_rankSummaryText.gameObject, Vector2.zero, Vector2.one,
            offsetMin: new Vector2(8, 2f + RankControlsLift), offsetMax: new Vector2(-8, -51f + RankControlsLift));
        ConfineText(_rankSummaryText, 7);

        GameObject topBar = new("TopBar", typeof(RectTransform));
        topBar.transform.SetParent(parent, false);
        SetRect(topBar, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -108f + RankControlsLift), new Vector2(CenterWidth, 22));
        _rootDropdown = CreateDropdown("RootDropdown", topBar.transform, RootOptions, index =>
        {
            _rootFilter = index;
            BeginLayoutBatch();
            try
            {
                RefreshSelectedFilterButtons();
                RefreshCurrentList(true);
            }
            finally { EndLayoutBatch(); }
        });
        SetRect(_rootDropdown.gameObject, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero,
            new Vector2(188f, 22f));
        _realmDropdown = CreateDropdown("RealmDropdown", topBar.transform, RealmOptions, index =>
        {
            _realmFilter = index;
            BeginLayoutBatch();
            try
            {
                RefreshSelectedFilterButtons();
                RefreshCurrentList(true);
            }
            finally { EndLayoutBatch(); }
        });
        SetRect(_realmDropdown.gameObject, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero,
            new Vector2(188f, 22f));

        CreateRankTableHeader(parent);

        GameObject scrollObj = new("RankScrollView", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);
        _scrollViewRect = SetRect(scrollObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -(RankListTop + ListHeight / 2f)), new Vector2(CenterWidth, ListHeight));
        _rankScroll = MclslRankScrollSetup.ConfigureRankScroll(scrollObj, MclslUiTheme.RankSurfaceScroll);

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
        SetRect(bottom, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 15), new Vector2(CenterWidth, 20));
        // Leave the middle of the lower rail open for its eye ornament.
        CreateNavButton(bottom.transform, "ToTop", LM.Get("mclsl_xianlu_ranking_top"), new Vector2(0.22f, 0.5f), ToTop);
        CreateNavButton(bottom.transform, "Refresh", LM.Get("mclsl_xianlu_ranking_refresh"), new Vector2(0.37f, 0.5f), RefreshAll);
        CreateNavButton(bottom.transform, "ToBottom", LM.Get("mclsl_xianlu_ranking_bottom"), new Vector2(0.80f, 0.5f), ToBottom);

        _countText = CreateText("Count", parent, "共 0 人", 9, MclslUiTheme.RankAccentSecondary);
        SetRect(_countText.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 39), new Vector2(CenterWidth, 17));
        ConfineText(_countText, 7);

        _emptyState = new GameObject("RankEmptyState", typeof(RectTransform), typeof(Image), typeof(Outline));
        _emptyState.transform.SetParent(parent, false);
        SetRect(_emptyState, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            pos: new Vector2(0, -(RankListTop + ListHeight / 2f)), size: new Vector2(324f, 86f));
        _emptyState.GetComponent<Image>().color = MclslUiTheme.RankSurfacePanel;
        _emptyState.GetComponent<Image>().raycastTarget = false;
        _emptyState.GetComponent<Outline>().effectColor = MclslUiTheme.RankFrameEdge;
        Text emptyTitle = CreateText("EmptyTitle", _emptyState.transform,
            LM.Get("mclsl_xianlu_ranking_empty_title"), 11, MclslUiTheme.RankTextMuted);
        emptyTitle.fontStyle = FontStyle.Bold;
        SetRect(emptyTitle.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(-24f, 20f));
        ConfineText(emptyTitle, 9);
        Text emptyBody = CreateText("EmptyBody", _emptyState.transform,
            LM.Get("mclsl_xianlu_ranking_empty_body"), 10, MclslUiTheme.RankTextMuted);
        emptyBody.alignment = TextAnchor.UpperCenter;
        SetRect(emptyBody.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(-24f, 40f));
        ConfineText(emptyBody, 9);
    }

    private static void CreateRankTableFrame(Transform parent)
    {
        GameObject outer = new("玄黄榜框", typeof(RectTransform), typeof(Image));
        outer.transform.SetParent(parent, false);
        SetRect(outer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, PanelCenterY), new Vector2(CenterWidth + 8f, WindowHeight + PanelHeightDelta));
        outer.GetComponent<Image>().color = MclslUiTheme.RankFrameEdge;
        outer.GetComponent<Image>().raycastTarget = false;

        GameObject inner = new("玄黄榜内页", typeof(RectTransform), typeof(Image));
        inner.transform.SetParent(outer.transform, false);
        SetRect(inner, Vector2.zero, Vector2.one, offsetMin: new Vector2(1.5f, 1.5f), offsetMax: new Vector2(-1.5f, -1.5f));
        inner.GetComponent<Image>().color = MclslUiTheme.RankSurfaceDeep;
        inner.GetComponent<Image>().raycastTarget = false;
    }

    private static void CreateRankTableHeader(Transform parent)
    {
        GameObject header = new("榜单列名", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(parent, false);
        SetRect(header, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -137f + RankControlsLift), new Vector2(CenterWidth, 22));
        header.GetComponent<Image>().color = MclslUiTheme.RankButton;
        CreatePanelAccent(header.transform, MclslUiTheme.RankAccentPrimary, 1f);
        CreateHeaderLabel(header.transform, "榜", 4, 22, TextAnchor.MiddleCenter);
        CreateHeaderLabel(header.transform, "姓名/灵根", NameColumnX, NameColumnWidth, TextAnchor.MiddleLeft);
        CreateHeaderLabel(header.transform, "境界", RealmColumnX, RealmColumnWidth, TextAnchor.MiddleCenter);
        _primarySortHeader = CreateHeaderLabel(header.transform, "战力", PowerColumnX, PowerColumnWidth, TextAnchor.MiddleCenter);
        CreateHeaderLabel(header.transform, "归属", OriginColumnX, OriginColumnWidth, TextAnchor.MiddleCenter);
    }

    private static Text CreateHeaderLabel(Transform parent, string value, float x, float width, TextAnchor alignment)
    {
        Text label = CreateText("列_" + value, parent, value, 8, MclslUiTheme.RankAccentSecondary);
        label.alignment = alignment;
        label.fontStyle = FontStyle.Bold;
        SetRect(label.gameObject, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(x, 0), new Vector2(width, 18));
        ConfineText(label, 7);
        return label;
    }

    private static void CreateRightPanel(Transform parent)
    {
        GameObject panel = new("RightPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        SetRect(panel, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f),
            new Vector2(-24f, PanelCenterY), new Vector2(RightPanelWidth + 16f, PanelHeightDelta));
        panel.GetComponent<Image>().color = MclslUiTheme.RankSurfacePanel;
        panel.AddComponent<MclslRankRightClickHandler>().BlockRightClick = true;
        CreatePanelAccent(panel.transform, MclslUiTheme.RankAccentSecondary, 1.5f);
        CreateSectionTitle(panel.transform, "排序", new Vector2(0, -6));

        GameObject scroll = CreateScrollView(panel.transform, "SortScroll", new Vector2(6, 36), new Vector2(-6, -31));
        Transform sortContent = scroll.transform.Find("Viewport/Content");
        int sideColumns = ResolveSideGridColumns();
        _selectedSortContainer = CreateTitledGrid(sortContent, "当前排序", SideGridCellSize, sideColumns, false);
        _availableSortContainer = CreateTitledGrid(sortContent, "可选排序", SideGridCellSize, sideColumns, true);
        CreateSearchSection(sortContent);
        CreateAvailableSortButtons();
        RefreshSelectedSortButtons();

        Button clear = CreateButton(panel.transform, "ClearSort", "清空排序", new Vector2(88, 20), ClearSortKeys);
        SetRect(clear.gameObject, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(88, 20));
        SetButtonColor(clear, MclslUiTheme.RankDanger);
    }

    private static void RefreshAll()
    {
        BeginLayoutBatch();
        try
        {
            MclslRankSnapshotSource.Invalidate();
            RefreshFilterChoices();
            RefreshSelectedFilterButtons();
            RefreshSelectedSortButtons();
            RefreshCurrentList(false);
        }
        finally { EndLayoutBatch(); }
    }

    private static void RefreshCurrentList(bool resetScroll)
    {
        RefreshPrimarySortHeader();
        RequestSortOrder();
        NextEntries = new MclslOrderedIdIndex<MclslRankEntry>(_viewCompare);
        _filterCursor = 0;
        _resetPendingScroll = resetScroll;
        _pendingQuery = MclslRankTextFormat.NormalizeSearch(_searchQuery);
        _viewRevision = MclslRankSnapshotSource.Revision;
    }
    private static void RefreshPrimarySortHeader()
    {
        if (_primarySortHeader == null) return;
        MclslRankSortDef primary = ActiveSortKeys.Count > 0 ? ActiveSortKeys[0].Def : null;
        _primarySortHeader.text = primary == null ? "战力" : primary.Id == "root" ? "灵根" : primary.Name;
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
            // 玄黄榜排除普通妖兽；具名妖兽（帝一/帝叁貘/青神龙）不属于普通妖兽，维持原规则。
            if (entry == null || entry.IsOrdinaryBeast || !MclslActorAccessor.Alive(entry.Actor)) continue;
            if (!PassRootFilter(entry) || !PassRealmFilter(entry) || !PassSearch(entry, _pendingQuery)
                || !PassActiveFilters(entry.Actor)) continue;
            NextEntries.Upsert(entry.ActorId, entry);
        }
        if (_filterCursor < snapshot.Count) return;
        _filterCursor = -1;
        Entries = NextEntries; NextEntries = null;
        _viewDataDirty = false;
        _viewRevision = MclslRankSnapshotSource.Revision;
        _countText.text = string.IsNullOrWhiteSpace(_searchQuery)
            ? "共 " + Entries.Count.ToString(CultureInfo.InvariantCulture) + " 人"
            : "搜索：" + _searchQuery.Trim() + "  共 " + Entries.Count.ToString(CultureInfo.InvariantCulture) + " 人";
        if (_rankSummaryText != null)
            _rankSummaryText.text = "总计入榜角色：" + Entries.Count.ToString(CultureInfo.InvariantCulture)
                + "名｜" + (ActiveSortKeys.Count == 0 ? "按综合战力排序" : "首序：" + ActiveSortKeys[0].Def.Name);
        _emptyState.SetActive(Entries.Count == 0);
        _rows?.Rebuild(Entries, _resetPendingScroll, ViewHeight);
        RefreshSelectedSortButtons();
    }

    internal static void OnEntryChanged(long id, MclslRankEntry entry)
    {
        if (_window == null || !_window.gameObject.activeInHierarchy) return;
        bool included = entry != null && !entry.IsOrdinaryBeast && MclslActorAccessor.Alive(entry.Actor)
            && PassRootFilter(entry) && PassRealmFilter(entry)
            && PassSearch(entry, MclslRankTextFormat.NormalizeSearch(_searchQuery)) && PassActiveFilters(entry.Actor);
        if (_filterCursor >= 0)
        {
            if (included) NextEntries?.Upsert(id, entry); else NextEntries?.Remove(id);
        }
        else
        {
            if (MclslRankEntryChange.Apply(Entries, _rows, id, included, entry)) _viewDataDirty = true;
        }
        _viewRevision = MclslRankSnapshotSource.Revision;
    }
    internal static void ClearRuntime()
    {
        Entries.Clear(); NextEntries = null; _filterCursor = -1; _viewRevision = -1;
        _needRefresh = true; _viewDataDirty = false;
        _rows?.ClearRows();
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

    private static void RequestSortOrder()
    {
        StringBuilder signature = new();
        for (int i = 0; i < ActiveSortKeys.Count; i++)
            signature.Append(ActiveSortKeys[i].Def.Id).Append(ActiveSortKeys[i].Ascending ? '+' : '-').Append('|');
        string key = ActiveSortKeys.Count == 0 ? "power" : signature.ToString();
        if (_orderSignature == key) return;
        _orderSignature = key;
        MclslRankSortKey[] keys = new MclslRankSortKey[ActiveSortKeys.Count];
        for (int i = 0; i < keys.Length; i++)
            keys[i] = new MclslRankSortKey(ActiveSortKeys[i].Def) { Ascending = ActiveSortKeys[i].Ascending };
        _viewCompare = (left, right) =>
        {
            if (keys.Length > 0 && IsCareerSort(keys[0].Def.Id))
                return CompareCombatCareer(left, right, CareerIndexForSortId(keys[0].Def.Id));
            if (keys.Length > 0 && IsProfessionSort(keys[0].Def.Id))
                return CompareProfession(left, right, ProfessionForSortId(keys[0].Def.Id));
            for (int i = 0; i < keys.Length; i++)
            {
                int order = keys[i].Compare(left, right);
                if (order != 0) return order;
            }
            int power = right.Power.CompareTo(left.Power);
            return power != 0 ? power : left.ActorId.CompareTo(right.ActorId);
        };
    }

    private static int CompareProfession(MclslRankEntry left, MclslRankEntry right, string profession)
    {
        int leftGrade = ProfessionSortRank(left, profession);
        int rightGrade = ProfessionSortRank(right, profession);
        int result = rightGrade.CompareTo(leftGrade);
        if (result != 0) return result;

        int leftExperience = left?.ProfessionId == profession ? left.ProfessionExperience : 0;
        int rightExperience = right?.ProfessionId == profession ? right.ProfessionExperience : 0;
        result = rightExperience.CompareTo(leftExperience);
        if (result != 0) return result;

        result = (right?.RealmIndex ?? -1).CompareTo(left?.RealmIndex ?? -1);
        if (result != 0) return result;
        return (left?.ActorId ?? 0L).CompareTo(right?.ActorId ?? 0L);
    }

    private static int ProfessionSortRank(MclslRankEntry entry, string profession)
    {
        if (entry == null || entry.ProfessionId != profession) return 0;
        return Math.Clamp(entry.ProfessionGrade, 0, 4) + 1;
    }

    private static bool IsProfessionSort(string sortId) => sortId is
        "profession_alchemist" or "profession_refiner" or "profession_talisman";

    private static string ProfessionForSortId(string sortId) => sortId switch
    {
        "profession_alchemist" => MclslProfessionSystem.Alchemist,
        "profession_refiner" => MclslProfessionSystem.Refiner,
        "profession_talisman" => MclslProfessionSystem.TalismanMaker,
        _ => string.Empty
    };

    private static string FormatProfession(MclslRankEntry entry, string profession)
    {
        if (entry == null || entry.ProfessionId != profession) return "无该职业";
        string[] grades = { "学徒", "黄级", "玄级", "地级", "天级" };
        int grade = Math.Clamp(entry.ProfessionGrade, 0, grades.Length - 1);
        return grades[grade] + " · 熟练度 " + Math.Max(0, entry.ProfessionExperience).ToString(CultureInfo.InvariantCulture);
    }

    // ── 战斗职业排序（剑修/武修/术修/符修/阵修/灵契师） ────────────────
    private static bool IsCareerSort(string sortId) => sortId is
        "career_sword" or "career_martial" or "career_elemental"
        or "career_sigil" or "career_array" or "career_spirit";

    private static int CareerIndexForSortId(string sortId) => sortId switch
    {
        "career_sword" => 0,
        "career_martial" => 1,
        "career_elemental" => 2,
        "career_sigil" => 3,
        "career_array" => 4,
        "career_spirit" => 5,
        _ => -1
    };

    // 阶位 0..5 换算成 1..6，无该职业记 0 沉底；同阶用熟练度做次序。
    private static int CombatCareerSortRank(MclslRankEntry entry, int career)
        => entry != null && entry.CombatCareer == career ? Math.Clamp(entry.CombatCareerRank, 0, 5) + 1 : 0;

    private static string FormatCombatCareer(MclslRankEntry entry, int career)
    {
        if (entry == null || entry.CombatCareer != career) return "无该职业";
        string[] ranks = MclslCombatCareerCatalog.Ranks;
        int rank = Math.Clamp(entry.CombatCareerRank, 0, ranks.Length - 1);
        return ranks[rank] + " · 熟练度 " + Math.Max(0, entry.CombatCareerExperience).ToString(CultureInfo.InvariantCulture);
    }

    private static int CompareCombatCareer(MclslRankEntry left, MclslRankEntry right, int career)
    {
        int result = CombatCareerSortRank(right, career).CompareTo(CombatCareerSortRank(left, career));
        if (result != 0) return result;
        int leftExp = left?.CombatCareer == career ? left.CombatCareerExperience : 0;
        int rightExp = right?.CombatCareer == career ? right.CombatCareerExperience : 0;
        result = rightExp.CompareTo(leftExp);
        if (result != 0) return result;
        result = (right?.RealmIndex ?? -1).CompareTo(left?.RealmIndex ?? -1);
        if (result != 0) return result;
        result = (right?.MinorRealmIndex ?? -1).CompareTo(left?.MinorRealmIndex ?? -1);
        if (result != 0) return result;
        result = (right?.Power ?? 0d).CompareTo(left?.Power ?? 0d);
        if (result != 0) return result;
        return (left?.ActorId ?? 0L).CompareTo(right?.ActorId ?? 0L);
    }

    private static void UpdateVisibleCards()
    {
        if (_filterCursor >= 0) { ContinueFilteredView(); return; }
        if (_viewDataDirty)
        {
            _viewDataDirty = false;
            if (_countText != null) _countText.text = "共 " + Entries.Count.ToString(CultureInfo.InvariantCulture) + " 人";
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
    {
        string primary = ActiveSortKeys.Count > 0 ? ActiveSortKeys[0].Def.GetDisplay(item) : FormatNumber(item.Power);
        card.GetComponent<MclslRankCardView>()?.Setup(item, index, primary);
    }

    private static GameObject CreateCardPrefab()
    {
        GameObject card = new("MclslRankCardVNext", typeof(RectTransform), typeof(Image), typeof(Button), typeof(MclslRankCardView));
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(CenterWidth - 8f, CardHeight - 1f);
        Image bg = card.GetComponent<Image>();
        bg.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
        bg.type = Image.Type.Sliced;
        bg.color = MclslUiTheme.RankSurfacePanel;
        card.GetComponent<Button>().targetGraphic = bg;
        CreateCardText("RankText", card.transform, new Vector2(4, 0), new Vector2(22, 32), 11, TextAnchor.MiddleCenter, MclslUiTheme.RankAccentSecondary);
        CreateAvatarElement(card.transform);
        CreateCardText("NameText", card.transform, new Vector2(NameColumnX, 5), new Vector2(NameColumnWidth, 15), 8, TextAnchor.MiddleLeft, MclslUiTheme.RankTextPrimary);
        CreateCardText("DetailText", card.transform, new Vector2(NameColumnX, -7), new Vector2(NameColumnWidth, 11), 7, TextAnchor.MiddleLeft, MclslUiTheme.RankTextMuted);
        CreateCardText("RealmText", card.transform, new Vector2(RealmColumnX, 0), new Vector2(RealmColumnWidth, 31), 8, TextAnchor.MiddleCenter, MclslUiTheme.RankTextPrimary);
        CreateCardText("PowerText", card.transform, new Vector2(PowerColumnX, 0), new Vector2(PowerColumnWidth, 31), 8, TextAnchor.MiddleCenter, MclslUiTheme.RankAccentPrimary);
        CreateCardText("RightText", card.transform, new Vector2(OriginColumnX, 0), new Vector2(OriginColumnWidth, 31), 7, TextAnchor.MiddleCenter, MclslUiTheme.RankAccentSecondary);
        card.SetActive(false);
        return card;
    }

    private static void CreateCardText(string name, Transform parent, Vector2 pos, Vector2 size, int fontSize, TextAnchor alignment, Color color)
    {
        Text text = CreateText(name, parent, string.Empty, fontSize, color);
        text.alignment = alignment;
        SetRect(text.gameObject, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), pos, size);
        ConfineText(text, Mathf.Min(fontSize, 6));
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
            rect.anchoredPosition = new Vector2(28, 0);
            rect.localScale = new Vector3(0.48f, 0.48f, 0.48f);
            avatar.show_banner_kingdom = true;
            avatar.show_banner_clan = false;
            if (avatar.kingdomBanner != null) avatar.kingdomBanner.gameObject.SetActive(false);
            if (avatar.clanBanner != null) avatar.clanBanner.gameObject.SetActive(false);
            return;
        }
        Image image = CreateImage("AvatarFallback", parent, GetSafeSprite("ui/icons/iconQuestionMark"), Color.white, new Vector2(32, 32));
        SetRect(image.gameObject, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(28, 0), new Vector2(32, 32));
    }

    private static void RefreshFilterChoices()
    {
        // The three candidate groups share the left scroll; batch them so the
        // whole choice refresh rebuilds that scroll content once.
        BeginLayoutBatch();
        try
        {
            RefreshKingdomFilters(MclslRankSnapshotSource.KingdomFilterChoicesSnapshot());
            RefreshAssetFilters(MclslRankSnapshotSource.AssetFilterChoicesSnapshot());
            RefreshTraitFilters(MclslRankSnapshotSource.TraitFilterChoicesSnapshot());
            _filterChoicesInitialized = HasAnyAvailableFilterButton();
        }
        finally { EndLayoutBatch(); }
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
        BeginLayoutBatch();
        try
        {
            if (_kingdomFilterContainer == null) return;
            choices ??= Array.Empty<MclslRankKingdomFilterChoice>();
            // Candidate buttons are recreated only when the visible set actually
            // changes; an unchanged snapshot keeps the existing buttons.
            StringBuilder signature = new();
            for (int i = 0; i < choices.Count; i++)
            {
                MclslRankKingdomFilterChoice choice = choices[i];
                signature.Append(choice?.KingdomId.ToString(CultureInfo.InvariantCulture)).Append(':')
                    .Append(choice?.DisplayName).Append('|');
                var colors = choice?.Kingdom?.getColor();
                signature.Append(ColorUtility.ToHtmlStringRGBA(colors?.getColorMainSecond() ?? Color.gray))
                    .Append(ColorUtility.ToHtmlStringRGBA(colors?.getColorBanner() ?? Color.white)).Append('|');
            }
            string current = signature.ToString();
            if (string.Equals(current, _kingdomFilterSignature, StringComparison.Ordinal)) return;
            _kingdomFilterSignature = current;
            ClearObjectList(KingdomFilterButtons);
            for (int i = 0; i < choices.Count; i++)
            {
                GameObject button = CreateKingdomFilterButton(choices[i]);
                if (button != null) KingdomFilterButtons.Add(button);
            }
            RefreshDynamicGridLayout(_kingdomFilterContainer);
        }
        finally { EndLayoutBatch(); }
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
        Image image = CreateImage("Kingdom_" + kingdom.data.id.ToString(CultureInfo.InvariantCulture), _kingdomFilterContainer, background, mainColor, new Vector2(32, 32));
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
        BeginLayoutBatch();
        try
        {
            if (_assetFilterContainer == null) return;
            choices ??= Array.Empty<MclslRankAssetFilterChoice>();
            StringBuilder signature = new();
            for (int i = 0; i < choices.Count; i++)
            {
                MclslRankAssetFilterChoice choice = choices[i];
                signature.Append(choice?.Asset?.id).Append(':').Append(choice?.DisplayName).Append('|');
            }
            string current = signature.ToString();
            if (string.Equals(current, _assetFilterSignature, StringComparison.Ordinal)) return;
            _assetFilterSignature = current;
            ClearObjectList(AssetFilterButtons);
            for (int i = 0; i < choices.Count; i++)
            {
                GameObject button = CreateAssetFilterButton(choices[i]);
                if (button != null) AssetFilterButtons.Add(button);
            }
            RefreshDynamicGridLayout(_assetFilterContainer);
        }
        finally { EndLayoutBatch(); }
    }

    private static GameObject CreateAssetFilterButton(MclslRankAssetFilterChoice choice)
    {
        ActorAsset asset = choice?.Asset;
        if (asset == null || _assetFilterContainer == null) return null;
        Sprite icon = GetAssetIcon(asset);
        string displayName = string.IsNullOrWhiteSpace(choice.DisplayName) ? GetAssetDisplayName(asset) : choice.DisplayName;
        Image image = CreateImage("Asset_" + asset.id, _assetFilterContainer, icon, Color.white, new Vector2(32, 32));
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
        BeginLayoutBatch();
        try
        {
            if (TraitFilterGrids.Count != 1) return;
            choices ??= Array.Empty<MclslRankTraitFilterChoice>();
            List<(ActorTrait Trait, string DisplayName, int Group, string OtherGroup)> ordered = new(choices.Count);
            for (int i = 0; i < choices.Count; i++)
            {
                ActorTrait trait = AssetManager.traits?.get(choices[i].TraitId);
                if (trait == null) continue;
                string displayName = string.IsNullOrWhiteSpace(choices[i].DisplayName)
                    ? GetTraitDisplayName(trait) : choices[i].DisplayName;
                if (MclslLocalizationBridge.IsRuntimeKey(trait.id)
                    || MclslLocalizationBridge.IsRuntimeKey(displayName)) continue;
                int group = Array.IndexOf(ModTraitGroupOrder, trait.group_id);
                ordered.Add((trait, displayName, group < 0 ? ModTraitGroupOrder.Length : group,
                    group < 0 ? trait.group_id ?? string.Empty : string.Empty));
            }
            ordered.Sort((left, right) =>
            {
                int group = left.Group.CompareTo(right.Group);
                if (group != 0) return group;
                int otherGroup = string.Compare(left.OtherGroup, right.OtherGroup, StringComparison.Ordinal);
                if (otherGroup != 0) return otherGroup;
                int name = string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal);
                return name != 0 ? name : string.Compare(left.Trait.id, right.Trait.id, StringComparison.Ordinal);
            });
            StringBuilder signature = new();
            for (int i = 0; i < ordered.Count; i++)
                signature.Append(ordered[i].Trait.id).Append(':').Append(ordered[i].DisplayName).Append('|');
            string current = signature.ToString();
            if (string.Equals(current, _traitFilterSignature, StringComparison.Ordinal)) return;
            _traitFilterSignature = current;
            ClearObjectList(TraitFilterButtons);
            int count = 0;
            foreach (var choice in ordered)
            {
                GameObject button = CreateTraitFilterButton(choice.Trait, choice.DisplayName, TraitFilterGrids[0]);
                if (button != null) TraitFilterButtons.Add(button);
                if (button != null) count++;
            }
            Transform traitGrid = TraitFilterGrids[0];
            traitGrid.parent.parent.gameObject.SetActive(count > 0);
            RefreshDynamicGridLayout(traitGrid);
        }
        finally { EndLayoutBatch(); }
    }

    private static GameObject CreateTraitFilterButton(ActorTrait trait, string snapshotDisplayName, Transform parent)
    {
        if (trait == null || parent == null) return null;
        if (MclslLocalizationBridge.IsRuntimeKey(trait.id)) return null;
        Sprite icon = string.IsNullOrWhiteSpace(trait.path_icon) ? null : SpriteTextureLoader.getSprite(trait.path_icon);
        icon ??= GetSafeSprite("ui/icons/iconQuestionMark");
        string displayName = string.IsNullOrWhiteSpace(snapshotDisplayName) ? GetTraitDisplayName(trait) : snapshotDisplayName;
        if (MclslLocalizationBridge.IsRuntimeKey(displayName)) return null;
        Image image = CreateImage("Trait_" + trait.id, parent, icon, Color.white, new Vector2(32, 32));
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
        ActiveFilters.Add(new MclslRankFilterSetting(new MclslRankFilterArgs
        {
            Id = id,
            DisplayName = displayName,
            Icon = icon,
            InnerIcon = innerIcon,
            IconColor = iconColor,
            InnerColor = innerColor,
            FilterFunc = filterFunc
        }));
        BeginLayoutBatch();
        try
        {
            RefreshSelectedFilterButtons();
            RefreshCurrentList(true);
        }
        finally { EndLayoutBatch(); }
    }

    private static void RefreshSelectedFilterButtons()
    {
        long probe = MclslPerformanceProbe.Begin();
        BeginLayoutBatch();
        try
        {
            ActiveFilters.RemoveAll(filter => filter == null || filter.ToBeRemoved);
            if (_selectedFilterContainer == null) { ClearFilterButtonViews(); return; }
            bool layoutChanged = false;

            _activeFilterIds.Clear();
            for (int i = 0; i < ActiveFilters.Count; i++) _activeFilterIds.Add(ActiveFilters[i].Id);

            // Drop buttons whose filter is gone; reuse the rest by id.
            _removalScratch.Clear();
            foreach (KeyValuePair<string, FilterButtonView> pair in SelectedFilterButtonViews)
                if (!_activeFilterIds.Contains(pair.Key)) _removalScratch.Add(pair.Key);
            for (int i = 0; i < _removalScratch.Count; i++)
            {
                if (SelectedFilterButtonViews.TryGetValue(_removalScratch[i], out FilterButtonView stale))
                {
                    DestroyButtonView(stale?.Root);
                    SelectedFilterButtonViews.Remove(_removalScratch[i]);
                    layoutChanged = true;
                }
            }

            for (int i = 0; i < ActiveFilters.Count; i++)
            {
                MclslRankFilterSetting filter = ActiveFilters[i];
                if (!SelectedFilterButtonViews.TryGetValue(filter.Id, out FilterButtonView view) || view == null || view.Root == null)
                {
                    view = CreateSelectedFilterButton(filter);
                    if (view == null) continue;
                    SelectedFilterButtonViews[filter.Id] = view;
                    layoutChanged = true;
                }
                else
                {
                    UpdateSelectedFilterButton(view, filter);
                }
                if (view.Root.transform.GetSiblingIndex() != i)
                {
                    view.Root.transform.SetSiblingIndex(i);
                    layoutChanged = true;
                }
            }

            if (layoutChanged) RefreshDynamicGridLayout(_selectedFilterContainer);
        }
        finally
        {
            EndLayoutBatch();
            MclslPerformanceProbe.End("rank.filter_buttons", probe);
        }
    }

    private static FilterButtonView CreateSelectedFilterButton(MclslRankFilterSetting filter)
    {
        Image border = CreateImage(filter.Id, _selectedFilterContainer, GetSafeSprite("ui/special/windowInnerSliced"), filter.GetTypeColor(), new Vector2(32, 32));
        border.type = Image.Type.Sliced;
        Image icon = CreateImage("Icon", border.transform, filter.Icon ?? GetSafeSprite("ui/icons/iconQuestionMark"), filter.IconColor, Vector2.zero);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        SetRect(icon.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(2, 2), offsetMax: new Vector2(-2, -2));
        Image inner = null;
        if (filter.InnerIcon != null)
        {
            inner = CreateImage("Inner", icon.transform, filter.InnerIcon, filter.InnerColor, Vector2.zero);
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
        FilterButtonView view = new()
        {
            Root = border.gameObject,
            Filter = filter,
            Border = border,
            Icon = icon,
            Inner = inner,
            TypeBackground = typeBackground,
            TypeText = typeText
        };
        Button button = border.gameObject.AddComponent<Button>();
        button.targetGraphic = border;
        button.transition = Selectable.Transition.None;
        // Read the view's current setting each click: the button can outlive the
        // setting instance when a filter with the same id is removed and re-added.
        button.onClick.AddListener(() =>
        {
            MclslRankFilterSetting current = view.Filter;
            if (current == null) return;
            current.CycleType();
            ApplyFilterTypeVisual(view, current);
            RefreshCurrentList(true);
        });
        MclslRankRightClickHandler rightClick = border.gameObject.AddComponent<MclslRankRightClickHandler>();
        rightClick.OnRightClick = () =>
        {
            if (view.Filter != null) view.Filter.ToBeRemoved = true;
            BeginLayoutBatch();
            try
            {
                RefreshSelectedFilterButtons();
                RefreshCurrentList(true);
            }
            finally { EndLayoutBatch(); }
        };
        MclslRankTooltipTrigger tip = border.gameObject.AddComponent<MclslRankTooltipTrigger>();
        tip.TooltipText = filter.DisplayName;
        tip.TooltipDescription = "左键切换 与/或/非，右键移除";
        view.Tip = tip;
        return view;
    }

    private static void ApplyFilterTypeVisual(FilterButtonView view, MclslRankFilterSetting filter)
    {
        if (view == null || filter == null) return;
        Color typeColor = filter.GetTypeColor();
        if (view.Border != null) view.Border.color = typeColor;
        if (view.TypeBackground != null) view.TypeBackground.color = typeColor;
        if (view.TypeText != null) view.TypeText.text = filter.GetTypeText();
    }

    // Rebind a reused filter chip to its current setting. The logical type badge
    // is always refreshed; icon/inner/tooltip are refreshed when the setting
    // instance changed under the same id (e.g. removed then re-added).
    private static void UpdateSelectedFilterButton(FilterButtonView view, MclslRankFilterSetting filter)
    {
        if (view == null || view.Root == null || filter == null) return;
        bool rebound = !ReferenceEquals(view.Filter, filter);
        view.Filter = filter;
        if (rebound)
        {
            if (view.Icon != null)
            {
                view.Icon.sprite = filter.Icon ?? GetSafeSprite("ui/icons/iconQuestionMark");
                view.Icon.color = filter.IconColor;
            }
            if (filter.InnerIcon != null)
            {
                if (view.Inner == null && view.Icon != null)
                {
                    Image inner = CreateImage("Inner", view.Icon.transform, filter.InnerIcon, filter.InnerColor, Vector2.zero);
                    inner.preserveAspect = true;
                    inner.raycastTarget = false;
                    SetRect(inner.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(2, 2), offsetMax: new Vector2(-2, -2));
                    view.Inner = inner;
                }
                else if (view.Inner != null)
                {
                    view.Inner.sprite = filter.InnerIcon;
                    view.Inner.color = filter.InnerColor;
                    if (!view.Inner.gameObject.activeSelf) view.Inner.gameObject.SetActive(true);
                }
            }
            else if (view.Inner != null && view.Inner.gameObject.activeSelf)
            {
                view.Inner.gameObject.SetActive(false);
            }
            if (view.Tip != null) view.Tip.TooltipText = filter.DisplayName;
        }
        ApplyFilterTypeVisual(view, filter);
    }

    private static void RefreshDynamicGridLayout(Transform gridTransform)
    {
        if (gridTransform == null) return;
        GridLayoutGroup layout = gridTransform.GetComponent<GridLayoutGroup>();
        LayoutElement element = gridTransform.GetComponent<LayoutElement>();
        if (layout == null || element == null) return;
        int columns = Math.Max(1, layout.constraintCount);
        // A grid with no items must occupy zero rows, otherwise every empty
        // filter/sort group still reserves one full cell of height.
        int rows = Mathf.CeilToInt(gridTransform.childCount / (float)columns);
        float height = rows * layout.cellSize.y
            + (rows > 0 ? (rows - 1) * layout.spacing.y : 0f)
            + layout.padding.top
            + layout.padding.bottom;
        element.minHeight = height;
        element.preferredHeight = height;
        // Only record the height here; the shared flush rebuilds the affected
        // wrapper/scroll.content once per operation round.
        MarkGridLayoutDirty(gridTransform);
    }

    private static void BeginLayoutBatch() => _layoutBatchDepth++;

    private static void EndLayoutBatch()
    {
        if (_layoutBatchDepth <= 0) return;
        _layoutBatchDepth--;
        if (_layoutBatchDepth == 0) FlushDynamicGridLayout();
    }

    // Records an affected grid and the owning scroll content. Multiple grids in
    // the same scroll share one entry; the offset is captured before any rebuild.
    private static void MarkGridLayoutDirty(Transform inner)
    {
        if (inner == null) return;
        if (!_dirtyGridInners.Contains(inner)) _dirtyGridInners.Add(inner);
        ScrollRect scroll = FindOwningScrollRect(inner);
        if (scroll == null || scroll.viewport == null || scroll.content == null) return;
        for (int i = 0; i < _dirtyScrollContents.Count; i++)
            if (ReferenceEquals(_dirtyScrollContents[i].Scroll, scroll)) return;
        _dirtyScrollContents.Add(new DirtyScrollContent
        {
            Scroll = scroll,
            PreviousOffset = scroll.content.anchoredPosition.y
        });
    }

    // Rebuilds every wrapper chain touched this round, then all affected
    // scroll.content with a single Canvas.ForceUpdateCanvases, and clamps each
    // content offset. Idempotent: a flush with nothing pending is a no-op.
    private static void FlushDynamicGridLayout()
    {
        if (_layoutBatchDepth > 0) return;
        if (_dirtyGridInners.Count == 0 && _dirtyScrollContents.Count == 0) return;
        long probe = MclslPerformanceProbe.Begin();
        DirtyLayoutRects.Clear();
        OrderedLayoutRects.Clear();
        for (int i = 0; i < _dirtyGridInners.Count; i++)
        {
            Transform inner = _dirtyGridInners[i];
            if (inner == null) continue;
            ScrollRect scroll = FindOwningScrollRect(inner);
            RectTransform contentRect = scroll != null ? scroll.content : null;
            for (Transform current = inner; current != null && current != contentRect; current = current.parent)
                if (current is RectTransform rect && rect.gameObject.activeInHierarchy && DirtyLayoutRects.Add(rect))
                {
                    int depth = 0;
                    for (Transform parent = rect.parent; parent != null; parent = parent.parent) depth++;
                    OrderedLayoutRects.Add((rect, depth));
                }
        }
        OrderedLayoutRects.Sort((left, right) => right.Depth.CompareTo(left.Depth));
        for (int i = 0; i < OrderedLayoutRects.Count; i++)
            LayoutRebuilder.ForceRebuildLayoutImmediate(OrderedLayoutRects[i].Rect);
        if (_dirtyScrollContents.Count > 0)
        {
            Canvas.ForceUpdateCanvases();
            for (int i = 0; i < _dirtyScrollContents.Count; i++)
            {
                DirtyScrollContent entry = _dirtyScrollContents[i];
                ScrollRect scroll = entry.Scroll;
                if (scroll == null || scroll.viewport == null || scroll.content == null) continue;
                RectTransform contentRect = scroll.content;
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
                float maximum = Mathf.Max(0f, contentRect.rect.height - scroll.viewport.rect.height);
                contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, Mathf.Clamp(entry.PreviousOffset, 0f, maximum));
            }
        }
        _dirtyGridInners.Clear();
        _dirtyScrollContents.Clear();
        DirtyLayoutRects.Clear();
        OrderedLayoutRects.Clear();
        MclslPerformanceProbe.End("rank.layout", probe);
    }

    // Used by the collapse toggle: mark the touched grid and flush in the same
    // call so the layout collapses this frame.
    private static void RebuildScrollContentLayout(Transform inner)
    {
        MarkGridLayoutDirty(inner);
        FlushDynamicGridLayout();
    }

    private static void ResetReusableButtons()
    {
        ClearFilterButtonViews();
        ClearSelectedSortButtonViews();
        ClearAvailableSortButtonViews();
    }

    private static void ClearFilterButtonViews()
    {
        foreach (KeyValuePair<string, FilterButtonView> pair in SelectedFilterButtonViews)
            DestroyButtonView(pair.Value?.Root);
        SelectedFilterButtonViews.Clear();
    }

    private static void ClearSelectedSortButtonViews()
    {
        foreach (KeyValuePair<string, SortButtonView> pair in SelectedSortButtonViews)
            DestroyButtonView(pair.Value?.Root);
        SelectedSortButtonViews.Clear();
    }

    private static void ClearAvailableSortButtonViews()
    {
        for (int i = 0; i < AvailableSortButtonViews.Count; i++)
            DestroyButtonView(AvailableSortButtonViews[i]?.Root);
        AvailableSortButtonViews.Clear();
    }

    private static void DestroyButtonView(GameObject root)
    {
        if (root == null) return;
        // Destroy is deferred to the end of the frame; deactivate and detach
        // first so the grid stops counting the old button immediately.
        root.SetActive(false);
        root.transform.SetParent(null, false);
        UnityEngine.Object.Destroy(root);
    }

    private static ScrollRect FindOwningScrollRect(Transform inner)
    {
        for (Transform current = inner; current != null; current = current.parent)
        {
            ScrollRect scroll = current.GetComponent<ScrollRect>();
            if (scroll != null) return scroll;
        }
        return null;
    }

    private static void CreateAvailableSortButtons()
    {
        BeginLayoutBatch();
        try
        {
            if (_availableSortContainer == null) { ClearAvailableSortButtonViews(); return; }
            bool upToDate = AvailableSortButtonViews.Count == SortDefs.Length;
            if (upToDate)
            {
                for (int i = 0; i < SortDefs.Length; i++)
                {
                    SortButtonView existing = AvailableSortButtonViews[i];
                    if (existing == null || existing.Root == null
                        || !string.Equals(existing.Id, SortDefs[i].Id, StringComparison.Ordinal))
                    {
                        upToDate = false;
                        break;
                    }
                }
            }
            if (upToDate) return;

            ClearAvailableSortButtonViews();
            for (int i = 0; i < SortDefs.Length; i++)
            {
                SortButtonView view = CreateSortIconButton(SortDefs[i], _availableSortContainer, false);
                if (view != null) AvailableSortButtonViews.Add(view);
            }
            RefreshDynamicGridLayout(_availableSortContainer);
        }
        finally { EndLayoutBatch(); }
    }

    private static SortButtonView CreateSortIconButton(MclslRankSortDef definition, Transform parent, bool selected)
    {
        Image image = CreateImage("Sort_" + definition.Id, parent, GetSafeSprite(definition.IconPath), Color.white, new Vector2(32, 32));
        image.preserveAspect = true;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        SortButtonView view = new()
        {
            Id = definition.Id,
            Def = definition,
            Root = image.gameObject,
            Selected = selected
        };
        string sortId = definition.Id;
        if (selected)
        {
            // Resolve the live key by id on every click so a reused button never
            // fires a stale key captured when it was first created.
            button.onClick.AddListener(() =>
            {
                MclslRankSortKey key = FindActiveSortKey(sortId);
                if (key == null) return;
                if (ActiveSortKeys.Count > 0 && !string.Equals(ActiveSortKeys[0].Def.Id, sortId, StringComparison.Ordinal))
                {
                    ActiveSortKeys.Remove(key);
                    ActiveSortKeys.Insert(0, key);
                }
                else if (!IsProfessionSort(sortId) && !IsCareerSort(sortId)) key.Toggle();
                BeginLayoutBatch();
                try
                {
                    RefreshSelectedSortButtons();
                    RefreshCurrentList(false);
                }
                finally { EndLayoutBatch(); }
            });
            Image arrow = CreateImage("Arrow", image.transform, GetSafeSprite("ui/icons/iconArrowDOWN"), Color.white, new Vector2(11, 11));
            arrow.raycastTarget = false;
            SetRect(arrow.gameObject, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(-1, -1), new Vector2(11, 11));
            view.Arrow = arrow;
            MclslRankRightClickHandler rightClick = image.gameObject.AddComponent<MclslRankRightClickHandler>();
            rightClick.OnRightClick = () =>
            {
                MclslRankSortKey key = FindActiveSortKey(sortId);
                if (key != null) ActiveSortKeys.Remove(key);
                BeginLayoutBatch();
                try
                {
                    RefreshSelectedSortButtons();
                    RefreshCurrentList(false);
                }
                finally { EndLayoutBatch(); }
            };
        }
        else
        {
            button.onClick.AddListener(() =>
            {
                if (ActiveSortKeys.Exists(k => string.Equals(k.Def.Id, sortId, StringComparison.Ordinal))) return;
                // A newly selected field is the primary sort key.  In particular,
                // selecting 境界 must not leave an earlier 战力 selection in front.
                ActiveSortKeys.Insert(0, new MclslRankSortKey(definition));
                BeginLayoutBatch();
                try
                {
                    RefreshSelectedSortButtons();
                    RefreshCurrentList(false);
                }
                finally { EndLayoutBatch(); }
            });
        }
        MclslRankTooltipTrigger tip = image.gameObject.AddComponent<MclslRankTooltipTrigger>();
        view.Tip = tip;
        if (selected)
        {
            UpdateSortButtonVisual(view);
        }
        else
        {
            tip.TooltipText = definition.Name;
            tip.TooltipDescription = "左键设为首排序";
        }
        return view;
    }

    private static MclslRankSortKey FindActiveSortKey(string id)
    {
        for (int i = 0; i < ActiveSortKeys.Count; i++)
            if (string.Equals(ActiveSortKeys[i].Def.Id, id, StringComparison.Ordinal)) return ActiveSortKeys[i];
        return null;
    }

    // Updates only the direction arrow and tooltip of a reused selected chip;
    // skips work when the ascending state did not change.
    private static void UpdateSortButtonVisual(SortButtonView view)
    {
        if (view == null || !view.Selected || view.Root == null) return;
        MclslRankSortKey key = FindActiveSortKey(view.Id);
        bool ascending = key != null && key.Ascending;
        if (view.HasVisual && view.LastAscending == ascending) return;
        view.HasVisual = true;
        view.LastAscending = ascending;
        if (view.Arrow != null)
            view.Arrow.sprite = GetSafeSprite(ascending ? "ui/icons/iconArrowUP" : "ui/icons/iconArrowDOWN");
        if (view.Tip != null)
        {
            view.Tip.TooltipText = view.Def.Name + (ascending ? "（升序）" : "（降序）");
            view.Tip.TooltipDescription = IsCareerSort(view.Id)
                ? "按战斗职业阶位（宗师、大师、精通、熟练、入门、未入门）及熟练度、境界排序；无该职业者排在其后。"
                : IsProfessionSort(view.Id)
                ? "按品阶天、地、玄、黄及无该职业排序；同品阶按熟练度、境界和稳定ID排序。"
                : "左键设为首排序或切换升降序，右键移除";
        }
    }

    private static void RefreshSelectedSortButtons()
    {
        long probe = MclslPerformanceProbe.Begin();
        BeginLayoutBatch();
        try
        {
            if (_selectedSortContainer == null) { ClearSelectedSortButtonViews(); return; }
            bool layoutChanged = false;

            _activeSortIds.Clear();
            for (int i = 0; i < ActiveSortKeys.Count; i++) _activeSortIds.Add(ActiveSortKeys[i].Def.Id);

            _removalScratch.Clear();
            foreach (KeyValuePair<string, SortButtonView> pair in SelectedSortButtonViews)
                if (!_activeSortIds.Contains(pair.Key)) _removalScratch.Add(pair.Key);
            for (int i = 0; i < _removalScratch.Count; i++)
            {
                if (SelectedSortButtonViews.TryGetValue(_removalScratch[i], out SortButtonView stale))
                {
                    DestroyButtonView(stale?.Root);
                    SelectedSortButtonViews.Remove(_removalScratch[i]);
                    layoutChanged = true;
                }
            }

            for (int i = 0; i < ActiveSortKeys.Count; i++)
            {
                MclslRankSortDef def = ActiveSortKeys[i].Def;
                if (!SelectedSortButtonViews.TryGetValue(def.Id, out SortButtonView view) || view == null || view.Root == null)
                {
                    view = CreateSortIconButton(def, _selectedSortContainer, true);
                    if (view == null) continue;
                    SelectedSortButtonViews[def.Id] = view;
                    layoutChanged = true;
                }
                UpdateSortButtonVisual(view);
                if (view.Root.transform.GetSiblingIndex() != i)
                {
                    view.Root.transform.SetSiblingIndex(i);
                    layoutChanged = true;
                }
            }

            if (layoutChanged) RefreshDynamicGridLayout(_selectedSortContainer);
        }
        finally
        {
            EndLayoutBatch();
            MclslPerformanceProbe.End("rank.sort_buttons", probe);
        }
    }

    private static void ClearSortKeys()
    {
        ActiveSortKeys.Clear();
        BeginLayoutBatch();
        try
        {
            RefreshSelectedSortButtons();
            RefreshCurrentList(false);
        }
        finally { EndLayoutBatch(); }
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
        BeginLayoutBatch();
        try
        {
            RefreshSelectedFilterButtons();
            RefreshCurrentList(true);
        }
        finally { EndLayoutBatch(); }
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

    private static void ClearObjectList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject item = list[i];
            if (item == null) continue;
            // Destroy is deferred to the end of the frame; deactivate and detach
            // first so the grid stops counting the old buttons immediately and the
            // rebuild in this same frame sees the real child count.
            item.SetActive(false);
            item.transform.SetParent(null, false);
            UnityEngine.Object.Destroy(item);
        }
        list.Clear();
    }

    private static int ResolveSideGridColumns()
    {
        return LeftPanelWidth < 112f ? 3 : 4;
    }

    private static Transform CreateTitledGrid(Transform parent, string title, int cellSize, int columns, bool collapsible, bool initiallyCollapsed = false)
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
        titleImage.color = MclslUiTheme.RankButton;
        titleBar.GetComponent<LayoutElement>().preferredHeight = 20f;
        Text titleText = CreateText("Title", titleBar.transform, title, 10, MclslUiTheme.RankAccentSecondary);
        titleText.alignment = TextAnchor.MiddleLeft;
        SetRect(titleText.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(5, 0), offsetMax: new Vector2(-42, 0));
        ConfineText(titleText, 8);

        GameObject gridContainer = new("GridContainer", typeof(RectTransform), typeof(Image), typeof(ContentSizeFitter), typeof(VerticalLayoutGroup));
        gridContainer.transform.SetParent(container.transform, false);
        SetRect(gridContainer, size: Vector2.zero);
        Image gridImage = gridContainer.GetComponent<Image>();
        gridImage.sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        gridImage.type = Image.Type.Sliced;
        gridImage.color = MclslUiTheme.RankSurfaceDeep;
        gridContainer.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        VerticalLayoutGroup gridContainerLayout = gridContainer.GetComponent<VerticalLayoutGroup>();
        gridContainerLayout.childControlHeight = true;
        gridContainerLayout.childControlWidth = true;
        gridContainerLayout.childForceExpandHeight = false;
        gridContainerLayout.padding = new RectOffset(3, 3, 3, 3);

        GameObject grid = new("Grid", typeof(RectTransform), typeof(LayoutElement), typeof(ContentSizeFitter), typeof(GridLayoutGroup));
        grid.transform.SetParent(gridContainer.transform, false);
        SetRect(grid, size: Vector2.zero);
        grid.GetComponent<LayoutElement>().minHeight = 0f;
        grid.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        GridLayoutGroup gridLayout = grid.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(cellSize, cellSize);
        gridLayout.spacing = new Vector2(3f, 3f);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;

        if (collapsible)
        {
            Text toggleText = CreateText("Toggle", titleBar.transform, initiallyCollapsed ? "[展开]" : "[折叠]", 9, MclslUiTheme.RankTextMuted);
            toggleText.alignment = TextAnchor.MiddleRight;
            SetRect(toggleText.gameObject, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-5, 0), new Vector2(50, 0));
            ConfineText(toggleText, 8);
            Button toggle = titleBar.AddComponent<Button>();
            toggle.targetGraphic = titleImage;
            toggle.transition = Selectable.Transition.None;
            toggle.onClick.AddListener(() =>
            {
                bool wasActive = gridContainer.activeSelf;
                gridContainer.SetActive(!wasActive);
                toggleText.text = wasActive ? "[展开]" : "[折叠]";
                // Collapsing keeps only the title bar and existing group spacing;
                // rebuild the whole section chain and the scroll content so the
                // groups below shift up immediately instead of leaving a gap.
                RebuildScrollContentLayout(grid.transform);
            });
            titleBar.AddComponent<MclslRankTooltipTrigger>().TooltipText = "点击展开或折叠";
            gridContainer.SetActive(!initiallyCollapsed);
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
        titleImage.color = MclslUiTheme.RankButton;
        titleBar.GetComponent<LayoutElement>().preferredHeight = 20f;
        Text titleText = CreateText("Title", titleBar.transform, "姓名搜索", 10, MclslUiTheme.RankAccentSecondary);
        titleText.alignment = TextAnchor.MiddleLeft;
        SetRect(titleText.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(5, 0), offsetMax: new Vector2(-5, 0));

        GameObject inputObj = new("ActorSearchInput", typeof(RectTransform), typeof(Image), typeof(InputField), typeof(LayoutElement));
        inputObj.transform.SetParent(container.transform, false);
        Image inputImage = inputObj.GetComponent<Image>();
        inputImage.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        inputImage.type = Image.Type.Sliced;
        inputImage.color = MclslUiTheme.RankSurfaceDeep;
        inputObj.GetComponent<LayoutElement>().preferredHeight = 26f;
        Text inputText = CreateText("Text", inputObj.transform, string.Empty, 10, MclslUiTheme.RankTextPrimary);
        inputText.alignment = TextAnchor.MiddleLeft;
        SetRect(inputText.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(8, 0), offsetMax: new Vector2(-8, 0));
        Text placeholder = CreateText("Placeholder", inputObj.transform, "输入角色姓名", 10, MclslUiTheme.RankTextMuted);
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
        catch (System.Exception mclslEmptyCatchEx) { MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-3", mclslEmptyCatchEx, MclslGuardScope.OptionalMember); }
        return GetSafeSprite("ui/special/backgroundKingdomElement");
    }

    private static Sprite GetSafeKingdomIcon(Kingdom kingdom)
    {
        try
        {
            Sprite sprite = kingdom?.getElementIcon();
            if (sprite != null) return sprite;
        }
        catch (System.Exception mclslEmptyCatchEx) { MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-4", mclslEmptyCatchEx, MclslGuardScope.OptionalMember); }
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
        catch (System.Exception mclslEmptyCatchEx) { MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-5", mclslEmptyCatchEx, MclslGuardScope.OptionalMember); }
        try
        {
            string path = asset.getIconPath();
            if (!string.IsNullOrWhiteSpace(path))
            {
                Sprite sprite = SpriteTextureLoader.getSprite(path);
                if (sprite != null) return sprite;
            }
        }
        catch (System.Exception mclslEmptyCatchEx) { MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-UI-MclslRankWindow-cs-6", mclslEmptyCatchEx, MclslGuardScope.OptionalMember); }
        if (!string.IsNullOrWhiteSpace(asset.icon))
        {
            Sprite sprite = SpriteTextureLoader.getSprite("ui/icons/" + asset.icon);
            if (sprite != null) return sprite;
        }
        return GetSafeSprite("ui/icons/iconQuestionMark");
    }

    private static string GetAssetDisplayName(ActorAsset asset) => MclslRankTextFormat.AssetDisplayName(asset);

    private static string GetTraitDisplayName(ActorTrait trait) => MclslRankTextFormat.TraitDisplayName(trait);

    private static Button CreateNavButton(Transform parent, string name, string text, Vector2 anchor, Action onClick)
    {
        Button button = CreateButton(parent, name, text, new Vector2(54, 20), onClick);
        SetRect(button.gameObject, anchor, anchor, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(54, 20));
        return button;
    }

    internal static Button CreateButton(Transform parent, string name, string text, Vector2 size, Action onClick, Vector2? pos = null, Color? buttonColor = null, Color? textColor = null)
    {
        GameObject obj = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
        image.type = Image.Type.Sliced;
        image.color = buttonColor ?? MclslUiTheme.RankButton;
        SetRect(obj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), pos ?? Vector2.zero, size);
        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => onClick?.Invoke());
        Text label = CreateText("Text", obj.transform, text, 9, textColor ?? MclslUiTheme.RankTextPrimary);
        SetRect(label.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(2, 0), offsetMax: new Vector2(-2, 0));
        ConfineText(label, 7);
        return button;
    }

    internal static Dropdown CreateDropdown(string name, Transform parent, string[] options, Action<int> onChange, Color? buttonColor = null, Color? textColor = null, Color? surfaceColor = null)
    {
        GameObject obj = new(name, typeof(RectTransform), typeof(Image), typeof(Dropdown), typeof(LayoutElement));
        obj.transform.SetParent(parent, false);
        Image bg = obj.GetComponent<Image>();
        bg.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
        bg.type = Image.Type.Sliced;
        bg.color = buttonColor ?? MclslUiTheme.RankButton;
        Dropdown dropdown = obj.GetComponent<Dropdown>();
        for (int i = 0; i < options.Length; i++) dropdown.options.Add(new Dropdown.OptionData(options[i]));
        dropdown.onValueChanged.AddListener(index => onChange(index));
        Color itemColor = textColor ?? MclslUiTheme.RankTextPrimary;
        Text label = CreateText("Label", obj.transform, string.Empty, 8, itemColor);
        SetRect(label.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(5, 0), offsetMax: new Vector2(-20, 0));
        ConfineText(label, 7);
        dropdown.captionText = label;
        GameObject template = CreateDropdownTemplate(obj.transform, bg.sprite, surfaceColor, itemColor);
        dropdown.template = template.GetComponent<RectTransform>();
        dropdown.itemText = template.transform.Find("Viewport/Content/Item/Item Label")?.GetComponent<Text>();
        template.SetActive(false);
        dropdown.RefreshShownValue();
        obj.GetComponent<LayoutElement>().preferredHeight = 22f;
        return dropdown;
    }

    private static GameObject CreateDropdownTemplate(Transform parent, Sprite sprite, Color? surfaceColor = null, Color? itemTextColor = null)
    {
        GameObject template = new("Template", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        template.transform.SetParent(parent, false);
        SetRect(template, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 100));
        Image bg = template.GetComponent<Image>();
        bg.sprite = sprite;
        bg.type = Image.Type.Sliced;
        bg.color = surfaceColor ?? MclslUiTheme.RankSurfacePanel;
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
        Text itemText = CreateText("Item Label", item.transform, string.Empty, 8, itemTextColor ?? MclslUiTheme.RankTextPrimary);
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
        scrollObj.GetComponent<Image>().color = MclslUiTheme.RankSurfaceDeep;
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
        Text title = CreateText("Title", parent, text, 10, MclslUiTheme.RankAccentSecondary);
        SetRect(title.gameObject, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), pos, new Vector2(0, 16));
    }

    internal static Text CreateText(string name, Transform parent, string text, int fontSize, Color color)
    {
        GameObject obj = new(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        Text label = obj.GetComponent<Text>();
        label.font = LocalizedTextManager.current_font ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = MclslUiTheme.ReadableFontSize(fontSize);
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;
        label.text = text ?? string.Empty;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        return label;
    }

    internal static void ConfineText(Text label, int minFontSize)
    {
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = Mathf.Min(MclslUiTheme.ReadableFontSize(minFontSize), label.fontSize);
        label.resizeTextMaxSize = label.fontSize;
    }

    internal static RectTransform SetRect(GameObject obj, Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? pivot = null, Vector2? pos = null, Vector2? size = null, Vector2? offsetMin = null, Vector2? offsetMax = null)
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
