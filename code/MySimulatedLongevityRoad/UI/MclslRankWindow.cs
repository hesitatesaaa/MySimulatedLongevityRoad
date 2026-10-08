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
    private const int SideGridCellSize = 28;
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
    private static readonly List<GameObject> CardInstances = new();
    private static readonly Stack<GameObject> RecycledCards = new();
    private static readonly Dictionary<int, GameObject> CardByIndex = new();
    private static readonly List<int> CardIndexBuffer = new();
    private static readonly List<GameObject> SelectedFilterButtons = new();
    private static readonly List<MclslRankFilterSetting> ActiveFilters = new();
    private static readonly List<GameObject> KingdomFilterButtons = new();
    private static readonly List<GameObject> AssetFilterButtons = new();
    private static readonly List<GameObject> TraitFilterButtons = new();
    private static readonly List<Transform> TraitFilterGrids = new();
    private static readonly List<GameObject> SelectedSortButtons = new();
    private static readonly List<GameObject> AvailableSortButtons = new();

    private static ScrollWindow _window;
    private static Transform _nativeCloseRoot;
    private static RectTransform _contentRect;
    private static RectTransform _scrollViewRect;
    private static ScrollRect _rankScroll;
    private static ScrollRect _filterScroll;
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
    private static int _dirtyViewStart = int.MaxValue;
    private static int _dirtyViewEnd = -1;
    private static int _filterCursor = -1;
    private static bool _resetPendingScroll;
    private static string _pendingQuery;
    private static string _orderSignature;
    private static bool _filterChoicesInitialized;
    private static int _lastViewStart = int.MaxValue;
    private static int _lastViewEnd = -1;

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
        ApplyWindowScale();
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
        _nativeCloseRoot = _window.transform.Find("CloseBackground") ?? FindCloseButtonTransform();
        SetupWindowContent();
        RestoreNativeCloseButton();
        XianLuScreenGuardianOverlay.Ensure(_window.transform);
        _cardPrefab = CreateCardPrefab();
        _cardPrefab.transform.SetParent(_window.transform, false);
        MclslRankWindowUpdater updater = _window.gameObject.AddComponent<MclslRankWindowUpdater>();
        updater.OnOpen = () =>
        {
            XianLuScreenGuardianOverlay.Ensure(_window.transform);
            RestoreNativeCloseButton();
            ApplyWindowScale();
            if (ShouldRefreshFilterChoicesOnOpen()) RefreshFilterChoices();
            RefreshSelectedFilterButtons();
            RefreshSelectedSortButtons();
            _needRefresh = true;
        };
        // The native close button is restored on creation/open.  Re-scanning all
        // of its child graphics every frame caused needless allocations while a
        // long ranking list was open, without changing its appearance.
        updater.OnUpdate = () => { ApplyWindowScale(); UpdateVisibleCards(); };
        updater.OnClose = () =>
        {
            _filterCursor = -1; NextEntries = null; Entries.Clear(); DisposeCardPool();
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
        _filterScroll = scroll.GetComponent<ScrollRect>();
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
            RefreshSelectedFilterButtons();
            RefreshCurrentList(true);
        });
        SetRect(_rootDropdown.gameObject, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero,
            new Vector2(188f, 22f));
        _realmDropdown = CreateDropdown("RealmDropdown", topBar.transform, RealmOptions, index =>
        {
            _realmFilter = index;
            RefreshSelectedFilterButtons();
            RefreshCurrentList(true);
        });
        SetRect(_realmDropdown.gameObject, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero,
            new Vector2(188f, 22f));

        CreateRankTableHeader(parent);

        GameObject scrollObj = new("RankScrollView", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);
        _scrollViewRect = SetRect(scrollObj, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(0, -(RankListTop + ListHeight / 2f)), new Vector2(CenterWidth, ListHeight));
        Image scrollBg = scrollObj.GetComponent<Image>();
        scrollBg.color = MclslUiTheme.RankSurfaceScroll;
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
        MclslRankSnapshotSource.Invalidate();
        RefreshFilterChoices();
        RefreshSelectedFilterButtons();
        RefreshSelectedSortButtons();
        RefreshCurrentList(false);
    }

    private static void RefreshCurrentList(bool resetScroll)
    {
        RefreshPrimarySortHeader();
        RequestSortOrder();
        NextEntries = new MclslOrderedIdIndex<MclslRankEntry>(_viewCompare);
        _filterCursor = 0;
        _resetPendingScroll = resetScroll;
        _pendingQuery = NormalizeSearch(_searchQuery);
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
            if (entry == null || !MclslActorAccessor.Alive(entry.Actor)) continue;
            if (!PassRootFilter(entry) || !PassRealmFilter(entry) || !PassSearch(entry, _pendingQuery)
                || !PassActiveFilters(entry.Actor)) continue;
            NextEntries.Upsert(entry.ActorId, entry);
        }
        if (_filterCursor < snapshot.Count) return;
        _filterCursor = -1;
        ClearCards();
        if (_resetPendingScroll && _contentRect != null) _contentRect.anchoredPosition = Vector2.zero;
        Entries = NextEntries; NextEntries = null;
        _viewDataDirty = false;
        _dirtyViewStart = int.MaxValue; _dirtyViewEnd = -1;
        _viewRevision = MclslRankSnapshotSource.Revision;
        _contentRect.sizeDelta = new Vector2(0, Math.Max(1, Entries.Count) * CardHeight);
        _lastViewStart = int.MaxValue; _lastViewEnd = -1;
        _countText.text = string.IsNullOrWhiteSpace(_searchQuery)
            ? "共 " + Entries.Count.ToString(CultureInfo.InvariantCulture) + " 人"
            : "搜索：" + _searchQuery.Trim() + "  共 " + Entries.Count.ToString(CultureInfo.InvariantCulture) + " 人";
        if (_rankSummaryText != null)
            _rankSummaryText.text = "总计入榜角色：" + Entries.Count.ToString(CultureInfo.InvariantCulture)
                + "名｜" + (ActiveSortKeys.Count == 0 ? "按综合战力排序" : "首序：" + ActiveSortKeys[0].Def.Name);
        _emptyState.SetActive(Entries.Count == 0);
        CreateInitialVisibleCards();
        RefreshSelectedSortButtons();
    }

    internal static void OnEntryChanged(long id, MclslRankEntry entry)
    {
        if (_window == null || !_window.gameObject.activeInHierarchy) return;
        bool included = entry != null && MclslActorAccessor.Alive(entry.Actor)
            && PassRootFilter(entry) && PassRealmFilter(entry)
            && PassSearch(entry, NormalizeSearch(_searchQuery)) && PassActiveFilters(entry.Actor);
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
                _dirtyViewStart = Math.Min(_dirtyViewStart, changedStart);
                _dirtyViewEnd = Math.Max(_dirtyViewEnd, changedEnd);
                _viewDataDirty = true;
            }
        }
        _viewRevision = MclslRankSnapshotSource.Revision;
    }
    internal static void ClearRuntime()
    {
        Entries.Clear(); NextEntries = null; _filterCursor = -1; _viewRevision = -1;
        _needRefresh = true; _viewDataDirty = false;
        _dirtyViewStart = int.MaxValue; _dirtyViewEnd = -1;
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
        if (_filterCursor >= 0) { ContinueFilteredView(); return; }
        bool dataDirty = _viewDataDirty;
        int dirtyStart = _dirtyViewStart, dirtyEnd = _dirtyViewEnd;
        if (dataDirty)
        {
            _viewDataDirty = false;
            _dirtyViewStart = int.MaxValue; _dirtyViewEnd = -1;
            if (_contentRect != null) _contentRect.sizeDelta = new Vector2(0, Math.Max(1, Entries.Count) * CardHeight);
            if (_countText != null) _countText.text = "共 " + Entries.Count.ToString(CultureInfo.InvariantCulture) + " 人";
            _emptyState?.SetActive(Entries.Count == 0);
        }
        if (_needRefresh)
        {
            _needRefresh = false;
            RefreshCurrentList(true);
            return;
        }
        if (Entries.Count == 0)
        {
            if (CardByIndex.Count > 0) ClearCards();
            return;
        }
        if (_contentRect == null || _scrollViewRect == null) return;
        float scrollY = _contentRect.anchoredPosition.y;
        float viewHeight = _scrollViewRect.rect.height > 0f ? _scrollViewRect.rect.height : ListHeight;
        int start = Mathf.Max(0, Mathf.FloorToInt(scrollY / CardHeight) - 1);
        int end = Mathf.Min(Mathf.CeilToInt((scrollY + viewHeight) / CardHeight) + 1, Entries.Count - 1);
        if (start == _lastViewStart && end == _lastViewEnd && !dataDirty) return;

        CardIndexBuffer.Clear();
        foreach (KeyValuePair<int, GameObject> pair in CardByIndex)
            if (pair.Key < start || pair.Key > end) CardIndexBuffer.Add(pair.Key);
        for (int i = 0; i < CardIndexBuffer.Count; i++)
        {
            int index = CardIndexBuffer[i];
            if (CardByIndex.TryGetValue(index, out GameObject card) && card != null)
            {
                CardInstances.Remove(card);
                card.SetActive(false);
                RecycledCards.Push(card);
            }
            CardByIndex.Remove(index);
        }
        for (int i = start; i <= end; i++)
        {
            if (dataDirty && i >= dirtyStart && i <= dirtyEnd
                && CardByIndex.TryGetValue(i, out GameObject existing) && existing != null)
            {
                MclslRankEntry item = Entries[i];
                existing.transform.localPosition = new Vector3(0, -CardHeight / 2f - i * CardHeight);
                string primary = ActiveSortKeys.Count > 0 ? ActiveSortKeys[0].Def.GetDisplay(item) : FormatNumber(item.Power);
                existing.GetComponent<MclslRankCardView>()?.Setup(item, i, primary);
            }
            else CreateActorCard(i);
        }
        _lastViewStart = start;
        _lastViewEnd = end;
    }

    private static void CreateActorCard(int index)
    {
        if (index < 0 || index >= Entries.Count || CardByIndex.ContainsKey(index) || _cardPrefab == null || _contentTransform == null) return;
        MclslRankEntry item = Entries[index];
        GameObject card = null;
        while (RecycledCards.Count > 0 && card == null) card = RecycledCards.Pop();
        if (card == null) card = UnityEngine.Object.Instantiate(_cardPrefab, _contentTransform);
        else card.transform.SetParent(_contentTransform, false);
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
        Image image = CreateImage("AvatarFallback", parent, GetSafeSprite("ui/icons/iconQuestionMark"), Color.white, new Vector2(25, 25));
        SetRect(image.gameObject, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(28, 0), new Vector2(25, 25));
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
        float scrollOffset = _filterScroll != null && _filterScroll.content != null
            ? _filterScroll.content.anchoredPosition.y : 0f;
        for (int i = 0; i < TraitFilterButtons.Count; i++)
        {
            GameObject oldButton = TraitFilterButtons[i];
            if (oldButton == null) continue;
            oldButton.SetActive(false);
            oldButton.transform.SetParent(null, false);
            UnityEngine.Object.Destroy(oldButton);
        }
        TraitFilterButtons.Clear();
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
        int count = 0;
        foreach (var choice in ordered)
        {
            GameObject button = CreateTraitFilterButton(choice.Trait, choice.DisplayName, TraitFilterGrids[0]);
            if (button != null) TraitFilterButtons.Add(button);
            if (button != null) count++;
        }
        Transform traitGrid = TraitFilterGrids[0];
        traitGrid.parent.parent.gameObject.SetActive(count > 0);
        if (count > 0) RefreshDynamicGridLayout(traitGrid);
        if (_filterScroll?.content != null && _filterScroll.viewport != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_filterScroll.content);
            Canvas.ForceUpdateCanvases();
            Vector2 position = _filterScroll.content.anchoredPosition;
            float maximum = Mathf.Max(0f, _filterScroll.content.rect.height - _filterScroll.viewport.rect.height);
            position.y = Mathf.Clamp(scrollOffset, 0f, maximum);
            _filterScroll.content.anchoredPosition = position;
        }
    }

    private static GameObject CreateTraitFilterButton(ActorTrait trait, string snapshotDisplayName, Transform parent)
    {
        if (trait == null || parent == null) return null;
        if (MclslLocalizationBridge.IsRuntimeKey(trait.id)) return null;
        Sprite icon = string.IsNullOrWhiteSpace(trait.path_icon) ? null : SpriteTextureLoader.getSprite(trait.path_icon);
        icon ??= GetSafeSprite("ui/icons/iconQuestionMark");
        string displayName = string.IsNullOrWhiteSpace(snapshotDisplayName) ? GetTraitDisplayName(trait) : snapshotDisplayName;
        if (MclslLocalizationBridge.IsRuntimeKey(displayName)) return null;
        Image image = CreateImage("Trait_" + trait.id, parent, icon, Color.white, new Vector2(25, 25));
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
                // A newly selected field is the primary sort key.  In particular,
                // selecting 境界 must not leave an earlier 战力 selection in front.
                ActiveSortKeys.Insert(0, new MclslRankSortKey(definition));
                RefreshSelectedSortButtons();
                RefreshCurrentList(false);
            });
        }
        else
        {
            button.onClick.AddListener(() =>
            {
                if (ActiveSortKeys.Count > 0 && ActiveSortKeys[0] != selectedKey)
                {
                    ActiveSortKeys.Remove(selectedKey);
                    ActiveSortKeys.Insert(0, selectedKey);
                }
                else if (!IsProfessionSort(selectedKey.Def.Id)) selectedKey.Toggle();
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
        tip.TooltipDescription = selectedKey == null
            ? "左键设为首排序"
            : IsProfessionSort(definition.Id)
                ? "按品阶天、地、玄、黄及无该职业排序；同品阶按熟练度、境界和稳定ID排序。"
                : "左键设为首排序或切换升降序，右键移除";
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
        for (int i = 0; i < CardInstances.Count; i++)
        {
            GameObject card = CardInstances[i];
            if (card == null) continue;
            card.SetActive(false);
            RecycledCards.Push(card);
        }
        CardInstances.Clear();
        CardByIndex.Clear();
        CardIndexBuffer.Clear();
        _lastViewStart = int.MaxValue;
        _lastViewEnd = -1;
    }

    private static void DisposeCardPool()
    {
        ClearObjectList(CardInstances);
        while (RecycledCards.Count > 0)
        {
            GameObject card = RecycledCards.Pop();
            if (card != null) UnityEngine.Object.Destroy(card);
        }
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
        grid.GetComponent<LayoutElement>().minHeight = cellSize + 6f;
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
                Canvas.ForceUpdateCanvases();
                if (container.transform is RectTransform containerRect) LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
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
        return "未知特征";
    }

    private static Button CreateNavButton(Transform parent, string name, string text, Vector2 anchor, Action onClick)
    {
        Button button = CreateButton(parent, name, text, new Vector2(54, 20), onClick);
        SetRect(button.gameObject, anchor, anchor, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(54, 20));
        return button;
    }

    private static Button CreateButton(Transform parent, string name, string text, Vector2 size, Action onClick, Vector2? pos = null)
    {
        GameObject obj = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
        image.type = Image.Type.Sliced;
        image.color = MclslUiTheme.RankButton;
        SetRect(obj, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), pos ?? Vector2.zero, size);
        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => onClick?.Invoke());
        Text label = CreateText("Text", obj.transform, text, 9, MclslUiTheme.RankTextPrimary);
        SetRect(label.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(2, 0), offsetMax: new Vector2(-2, 0));
        ConfineText(label, 7);
        return button;
    }

    private static Dropdown CreateDropdown(string name, Transform parent, string[] options, Action<int> onChange)
    {
        GameObject obj = new(name, typeof(RectTransform), typeof(Image), typeof(Dropdown), typeof(LayoutElement));
        obj.transform.SetParent(parent, false);
        Image bg = obj.GetComponent<Image>();
        bg.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
        bg.type = Image.Type.Sliced;
        bg.color = MclslUiTheme.RankButton;
        Dropdown dropdown = obj.GetComponent<Dropdown>();
        for (int i = 0; i < options.Length; i++) dropdown.options.Add(new Dropdown.OptionData(options[i]));
        dropdown.onValueChanged.AddListener(index => onChange(index));
        Text label = CreateText("Label", obj.transform, string.Empty, 8, MclslUiTheme.RankTextPrimary);
        SetRect(label.gameObject, Vector2.zero, Vector2.one, offsetMin: new Vector2(5, 0), offsetMax: new Vector2(-20, 0));
        ConfineText(label, 7);
        dropdown.captionText = label;
        GameObject template = CreateDropdownTemplate(obj.transform, bg.sprite);
        dropdown.template = template.GetComponent<RectTransform>();
        dropdown.itemText = template.transform.Find("Viewport/Content/Item/Item Label")?.GetComponent<Text>();
        template.SetActive(false);
        dropdown.RefreshShownValue();
        obj.GetComponent<LayoutElement>().preferredHeight = 22f;
        return dropdown;
    }

    private static GameObject CreateDropdownTemplate(Transform parent, Sprite sprite)
    {
        GameObject template = new("Template", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        template.transform.SetParent(parent, false);
        SetRect(template, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 100));
        Image bg = template.GetComponent<Image>();
        bg.sprite = sprite;
        bg.type = Image.Type.Sliced;
        bg.color = MclslUiTheme.RankSurfacePanel;
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
        Text itemText = CreateText("Item Label", item.transform, string.Empty, 8, MclslUiTheme.RankTextPrimary);
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

    private static Text CreateText(string name, Transform parent, string text, int fontSize, Color color)
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

    private static void ConfineText(Text label, int minFontSize)
    {
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = Mathf.Min(MclslUiTheme.ReadableFontSize(minFontSize), label.fontSize);
        label.resizeTextMaxSize = label.fontSize;
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
