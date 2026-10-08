using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslFeatureWindow : MonoBehaviour
{
    private enum Page { Guide, Bag, Market }
    private sealed class MarketDisplayRow
    {
        internal MclslItemDefinition Item;
        internal string Title = string.Empty, Detail = string.Empty, Price = string.Empty, PriceDetail = string.Empty, Button = string.Empty;
        internal float Height, Top;
        internal float TitleHeight, DetailHeight, PriceHeight, PriceDetailHeight, DetailTop;
        internal MarketOfferGroup Group;
        internal Actor SellerActor;
    }

    private sealed class MarketOfferGroup
    {
        internal readonly List<MarketDisplayRow> Offers = new();
        internal MarketDisplayRow Summary;
    }

    private static readonly string[] Categories = { "丹药", "灵植", "灵物", "符箓材料", "炼器材料", "符箓", "法宝", "法术卷轴" };
    private static readonly string[] CategoryIds = { "Pill", "Plant", "SpiritObject", "TalismanMaterial", "Material", "Talisman", "Artifact", "SpellScroll" };
    private static readonly string[] BagCategories = { "法宝", "丹药", "符箓", "材料", "传承典籍", "法术" };
    private static readonly string[] GuideTitles = { "时代修行", "势力家族", "探索资源", "技艺战斗", "交易储物", "还真档案" };
    private static readonly GUIContent EmptyWindowTitle = GUIContent.none;
    private static readonly string[] GuideBodies =
    {
        "入口：从力量栏打开模组介绍、玄黄仙录与模组设置。操作：选中修士查看资质、灵根、境界和体质，再到玄黄仙录总览查看时代与七境人数。结果：修士按年度自行积累真元；资质影响速度，灵根在五岁判定，跳过时会补判。",
        "入口：在玄黄仙录打开势力和修仙家族页。操作：查看家主、族人、家库、传承与城市影响，再到玄黄舆图核对城镇主家。结果：十宗、万仙盟和五老会随时代更替；家族事务每五年自动结算。",
        "入口：从玄黄仙录的天下纪事进入玄黄舆图和探索记录。操作：按地点与资源类别查看遗迹、秘境、洞天和灵材来源，并留意修士体质与境界。结果：探索可能带回灵石、传承和材料，也可能造成伤亡；秘境最终死亡率最高40%。",
        "入口：选中修士，打开职业、法术和人物物品界面。操作：为可研习的法术选择目标，查看研习进度、已学法术与装备条件。结果：修士会按职业品阶自动制作，研习逐年推进；已学法术在十年结算和实战施放时获得升级经验。",
        "入口：选中修士打开乾坤袋；从力量栏打开天玄镜。操作：在乾坤袋按类别查看物品和传承，在天玄镜切换当前挂单、最新上架、成交记录及物品类别，展开挂单查看卖家与价格。结果：交易由修士按需求自动完成，使用贡献或灵石结算。",
        "入口：等待还真自然降临，或在特质编辑器赋予修士还真，再从玄黄仙录进入还真空间。操作：确认此世宿主并保存世界；灵蕴达到80点后建立锚点，满三枚时先选替换目标。回溯时选中归途锚点，再次点击确认。结果：当前世进入前世档案，可在前世轮盘按名额选择遗产。"
    };
    private static readonly string[][] GuidePoints =
    {
        new[] { "① 从力量栏进入模组介绍和玄黄仙录，先看总览的时代与境界人数。", "② 选中修士查看资质与灵根；资质越高，常规修炼越快。", "③ 查看人物特质中的玄黄异禀，核对属性、效果与获取方式。", "④ 到修士列传追踪此人的后续经历；修行按年度自动推进。" },
        new[] { "① 在玄黄仙录切换势力与修仙家族，按家族查家主、族人和家库。", "② 查看传承、家族纪事和城市影响；这些事务每五年结算。", "③ 在天下纪事打开玄黄舆图，按影响力辨认城镇主家。", "④ 观察时代变化：仙道纪元的十宗与新法时期的万仙盟、五老会各有事务。" },
        new[] { "① 从天下纪事进入玄黄舆图，查看地点和势力分布。", "② 查遗迹、秘境与洞天记录，了解灵石、传承和材料的来源。", "③ 对照修士境界和体质评估探索风险；部分体质可降低风险或加速探索。", "④ 回到世界纪事查看探索所得与重大变迁。" },
        new[] { "① 选中修士查看炼丹、炼器、制符职业与可用材料。", "② 在法术界面选可研习的目标，观察逐年累积的研习进度。", "③ 查看已学法术的经验；十年修炼结算及实战施放会推动升级。", "④ 使用丹药、符箓、法宝或卷轴前，核对品阶、装备和消耗条件。" },
        new[] { "① 选中修士打开乾坤袋，按分类查物品、典籍及法术。", "② 从力量栏打开天玄镜，先选物品类别，再切换挂单、上架和成交。", "③ 展开物品挂单比较卖家、数量、贡献与灵石价格。", "④ 等待修士自动挂牌和购买；天玄镜用于查看交易，不需要玩家手动下单。" },
        new[] { "① 等待还真自然降临，或在特质编辑器赋予修士还真；确认宿主后保存世界。首次绑定获得80点空间灵蕴。", "② 进入还真空间，灵蕴满80点后建立锚点；最多三枚，满额先选择替换的一枚，也可开启自动锚定。", "③ 需要手动回溯时选择归途锚点，点击回溯按钮，再次点击确认。", "④ 回溯后查看前世档案，在前世轮盘按当前名额选择实际拥有的遗产。" }
    };

    private static MclslFeatureWindow _instance;
    private static readonly Dictionary<string, Sprite> IconCache = new(StringComparer.Ordinal);

    private Page _page;
    private Actor _actor;
    private readonly List<MclslOwnedItem>[] _bagViews = { new(), new(), new(), new(), new(), new() };
    private readonly long[] _bagCategoryCounts = new long[6];
    private readonly Dictionary<string, MclslOwnedItem> _artifactGroups = new(StringComparer.Ordinal);
    private readonly List<MclslOwnedItem> _detailInstances = new();
    private List<MclslSpellDefinition> _knownSpells = new();
    private long _bagRevision = -1, _detailRevision = -1, _spellRevision = -1;
    private long _bagTotalCount;
    private string _spellToken, _detailItem;
    private int _bagFirstRow, _bagLastRow, _bookFirstRow, _bookLastRow;
    private bool _visible;
    private int _category;
    private int _guidePage;
    private int _marketView;
    private readonly int[] _marketCategoryCounts = new int[CategoryIds.Length];
    private readonly int[] _marketListingCounts = new int[CategoryIds.Length];
    private readonly int[] _marketListedCounts = new int[CategoryIds.Length];
    private readonly int[] _marketPurchasedCounts = new int[CategoryIds.Length];
    private readonly Dictionary<int, List<MarketOfferGroup>> _marketOfferGroups = new();
    private readonly Dictionary<int, List<MarketDisplayRow>> _marketListedRows = new(), _marketPurchasedRows = new();
    private readonly List<MarketDisplayRow> _marketRows = new();
    private MarketOfferGroup _selectedMarketGroup;
    private readonly string[] _marketCategoryLabels = new string[CategoryIds.Length];
    private static readonly string[] MarketViewLabels = { "mclsl_xianlu_market_listing", "mclsl_xianlu_market_recent", "mclsl_xianlu_market_deals" };
    private string _marketYearText = string.Empty, _marketListedText = string.Empty, _marketPurchasedText = string.Empty,
        _marketTotalText = string.Empty, _marketBoardText = string.Empty, _marketBoardTitle = string.Empty;
    private MclslWorldRunState _marketSnapshotRun;
    private int _marketRevision = -1;
    private float _marketRowsHeight;
    private int _marketListedTotal;
    private int _marketPurchasedTotal;
    private int _marketTotalCount;
    private string _selectedBagItemKey = string.Empty;
    private string _bagActionMessage = string.Empty;
    private string _expandedMarketItemId = string.Empty;
    private Vector2 _artifactInstanceScroll;
    private Vector2 _marketScroll;
    private Vector2 _marketPageScroll;
    private Vector2 _marketCategoryScroll;
    private Vector2 _marketInfoScroll;
    private Vector2 _marketDetailScroll;
    private Vector2 _marketOffersScroll;
    private bool _marketRulesExpanded;
    private float _marketRowWidth;
    private int _marketScreenWidth, _marketScreenHeight;
    private Vector2 _bagScroll;
    private Vector2 _guideScroll;
    private Rect _rect;
    private bool _pauseCaptured;
    private float _savedTimeScale;
    private bool _savedConfigPaused;
    private GameObject _inputBlocker;

    private GUIStyle _window;
    private GUIStyle _title;
    private GUIStyle _subtitle;
    private GUIStyle _body;
    private GUIStyle _muted;
    private GUIStyle _section;
    private GUIStyle _guidePageNumber;
    private GUIStyle _guidePageShadow;
    private GUIStyle _card;
    private GUIStyle _selectedCard;
    private GUIStyle _tab;
    private GUIStyle _selectedTab;
    private GUIStyle _button;
    private GUIStyle _primaryButton;
    private GUIStyle _small;
    private GUIStyle _priceStyle;
    private GUIStyle _smallButtonStyle;
    private GUIStyle _innerPanel;
    private GUIStyle _jadePanel;
    private GUIStyle _bagWindow;
    private GUIStyle _bagPanel;
    private GUIStyle _bagShelf;
    private GUIStyle _bagSlot;
    private GUIStyle _bagSelectedSlot;
    private GUIStyle _bagCategory;
    private GUIStyle _bagSelectedCategory;
    private GUIStyle _bagButton;
    private GUIStyle _bagPrimaryButton;
    private GUIStyle _marketWindow;
    private GUIStyle _marketPanel;
    private GUIStyle _marketInset;
    private GUIStyle _marketRow;
    private GUIStyle _marketTab;
    private GUIStyle _marketSelectedTab;
    private Texture2D _panelTexture;
    private Texture2D _innerTexture;
    private Texture2D _cardTexture;
    private Texture2D _selectedTexture;
    private Texture2D _jadeTexture;
    private Texture2D _goldTexture;
    private Texture2D _dimTexture;
    private Texture2D _bagWindowTexture;
    private Texture2D _bagPanelTexture;
    private Texture2D _bagShelfTexture;
    private Texture2D _bagSlotTexture;
    private Texture2D _bagSelectedSlotTexture;

    internal static void ShowGuide() => Show(Page.Guide, null);
    internal static void ShowBag(Actor actor = null) => Show(Page.Bag, actor);
    internal static void ShowMarket(Actor actor = null) => Show(Page.Market, actor);

    private static void Show(Page page, Actor actor)
    {
        if (_instance == null)
        {
            GameObject host = new("MclslFeatureWindow");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<MclslFeatureWindow>();
        }
        _instance.ReleaseMarketSnapshot();
        _instance.ReleaseBagSnapshot();
        _instance._page = page;
        _instance._actor = page == Page.Bag ? actor : null;
        if (page == Page.Bag && _instance._actor?.data == null
            && MclslMaobaoCommands.TryGetSelectedActor(out Actor selected)) _instance._actor = selected;
        if (page == Page.Market) MclslWorldRunRepository.EnsureCurrentRun(MclslRuntime.CurrentYear());
        float width = Mathf.Min(page == Page.Guide ? 1240f : page == Page.Market ? 1600f : 1120f, Screen.width - 28f);
        float height = Mathf.Min(page == Page.Market ? 932f : page == Page.Guide ? 800f : 740f, Screen.height - 28f);
        _instance._rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        _instance._marketScroll = Vector2.zero;
        _instance._marketPageScroll = Vector2.zero;
        _instance._marketCategoryScroll = Vector2.zero;
        _instance._marketInfoScroll = Vector2.zero;
        _instance._marketDetailScroll = Vector2.zero;
        _instance._marketOffersScroll = Vector2.zero;
        _instance._marketRulesExpanded = false;
        _instance._marketRowWidth = 0f;
        _instance._marketScreenWidth = Screen.width;
        _instance._marketScreenHeight = Screen.height;
        _instance._bagScroll = Vector2.zero;
        _instance._guideScroll = Vector2.zero;
        _instance._selectedBagItemKey = string.Empty;
        _instance._bagActionMessage = string.Empty;
        _instance._marketView = 0;
        if (page == Page.Market) _instance.CaptureMarketSnapshot();
        _instance._visible = true;
        _instance.enabled = true;
        _instance.CreateInputBlocker();
        _instance.ApplyFeaturePause();
    }

    private void OnGUI()
    {
        if (!_visible) return;
        if (_page == Page.Market && (Screen.width < 100 || Screen.height < 100)) return;
        if (_page == Page.Market && !ReferenceEquals(_marketSnapshotRun, MclslWorldRunRepository.Current))
        {
            CloseWindow();
            return;
        }
        EnsureStyles();
        XianLuUIStyles.Ensure();
        Color previous = GUI.color;
        Color previousBackground = GUI.backgroundColor;
        if (_page != Page.Bag)
        {
            GUI.color = new Color(0.015f, 0.025f, 0.04f, 0.60f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _dimTexture);
            GUI.color = previous;
        }
        XianLuUIRenderer.ScreenGuardians();
        if (_page == Page.Market)
        {
            float width = Mathf.Max(76f, Mathf.Min(_page == Page.Market ? 1600f : 1120f, Screen.width - 24f));
            float height = Mathf.Max(76f, Mathf.Min(932f, Screen.height - 24f));
            if (_marketScreenWidth != Screen.width || _marketScreenHeight != Screen.height)
            {
                _rect.x += (_rect.width - width) * 0.5f;
                _rect.y += (_rect.height - height) * 0.5f;
                _marketScreenWidth = Screen.width;
                _marketScreenHeight = Screen.height;
            }
            _rect.width = width;
            _rect.height = height;
        }
        else
        {
            _rect.width = Mathf.Min(_rect.width, Screen.width - 24f);
            _rect.height = Mathf.Min(_rect.height, Screen.height - 24f);
        }
        _rect.x = Mathf.Clamp(_rect.x, 12f, Screen.width - _rect.width - 12f);
        _rect.y = Mathf.Clamp(_rect.y, 12f, Screen.height - _rect.height - 12f);
        if (_page == Page.Bag)
        {
            // Paint the whole container explicitly. Some game GUI skins blend
            // GUI.Window backgrounds as translucent, even with an opaque style.
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.DrawTexture(_rect, _bagWindowTexture);
        }
        if (_page == Page.Bag)
        {
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
        }
        _rect = GUI.Window(781208, _rect, DrawWindow, EmptyWindowTitle,
            _page == Page.Bag ? _bagWindow : _page == Page.Market ? _marketWindow : _window);
        // Only the part above the window needs this second pass. The artwork
        // inside the window is drawn after its rails and background below.
        if (_visible && _page == Page.Guide)
            XianLuUIRenderer.WindowTitleExpanded(_rect, "title_mod_intro",
                "我的模拟长生路·模组介绍", 8f, _rect.width < 600f ? 32f : 42f,
                20f, 24f, _rect.y);
        GUI.color = previous;
        GUI.backgroundColor = previousBackground;
    }

    private void Update()
    {
        if (!_visible) return;
        if (_page == Page.Market && !ReferenceEquals(_marketSnapshotRun, MclslWorldRunRepository.Current))
        {
            CloseWindow();
            return;
        }
        if (_page == Page.Market && _marketRevision != MclslTianxuanMarket.Revision)
            RefreshMarketSnapshot();
        EnforceFeaturePause();
        if (Input.GetKeyDown(KeyCode.Escape)) CloseWindow();
    }

    private void DrawWindow(int id)
    {
        if (_page != Page.Bag)
        {
            // The market plate ends at the inner rails, rather than behind the outer ornament silhouette.
            Rect surface = _page == Page.Market
                ? new Rect(_rect.width * 0.04f, _rect.height * 0.075f, _rect.width * 0.92f, _rect.height * 0.855f)
                : new Rect(24f, 48f, Mathf.Max(0f, _rect.width - 48f), Mathf.Max(0f, _rect.height - 80f));
            Color old = GUI.color;
            GUI.color = MclslUiTheme.SurfaceWindow;
            GUI.DrawTexture(surface, _dimTexture);
            GUI.color = old;
            if (_page != Page.Market)
                XianLuUIRenderer.Frame(new Rect(0f, 0f, _rect.width, _rect.height), "frame_mod_intro");
            else
                XianLuUIRenderer.Frame(new Rect(0f, 0f, _rect.width, _rect.height), "frame_tianxuanjing", 0f, 0.7f);
        }
        GUIStyle oldScrollbar = GUI.skin.verticalScrollbar;
        GUIStyle oldThumb = GUI.skin.verticalScrollbarThumb;
        GUI.skin.verticalScrollbar = XianLuUIStyles.ScrollbarStyle;
        GUI.skin.verticalScrollbarThumb = XianLuUIStyles.ScrollbarThumbStyle;
        if (_page == Page.Bag)
        {
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.DrawTexture(new Rect(0f, 0f, _rect.width, _rect.height), _bagWindowTexture);
        }
        // A bounded area prevents GUILayout from stretching the window or painting over the bottom frame.
        if (_page == Page.Market)
            GUILayout.BeginArea(new Rect(56f, 15f, Mathf.Max(1f, _rect.width - 112f), Mathf.Max(1f, _rect.height * 0.90f - 15f)));
        GUILayout.BeginVertical();
        DrawHeader();
        GUILayout.Space(10f);
        switch (_page)
        {
            case Page.Guide: DrawGuide(); break;
            case Page.Bag: DrawBagPage(); break;
            default: DrawMarketPage(); break;
        }
        GUILayout.EndVertical();
        if (_page == Page.Market) GUILayout.EndArea();
        if (_page != Page.Bag)
        {
            if (_page == Page.Market)
                XianLuUIRenderer.Frame(new Rect(0f, 0f, _rect.width, _rect.height), "frame_tianxuanjing", 0.7f, 1f);
            XianLuUIRenderer.WindowTitleExpanded(new Rect(0f, 0f, _rect.width, _rect.height),
                _page == Page.Market ? "title_tianxuanjing" : "title_mod_intro",
                _page == Page.Market ? LM.Get("mclsl_xianlu_title_market") : "我的模拟长生路·模组介绍",
                _page == Page.Market ? 35f : 8f, _rect.width < 600f ? 32f : 42f,
                _page == Page.Market ? 24f : 20f, _page == Page.Market ? 0f : 24f);
            if (GUI.Button(new Rect(_rect.width - 164f, 35f, 76f, 32f), "关闭",
                XianLuUIStyles.ButtonSecondaryStyle)) CloseWindow();
            if (_page == Page.Guide)
            {
                string pageNumber = (_guidePage + 1) + " / " + GuideTitles.Length;
                Rect eye = new Rect((_rect.width - 90f) * 0.5f, _rect.height - 50f, 90f, 30f);
                GUI.Label(new Rect(eye.x + 1f, eye.y + 1f, eye.width, eye.height), pageNumber, _guidePageShadow);
                GUI.Label(eye, pageNumber, _guidePageNumber);
            }
        }
        GUI.DragWindow(new Rect(0f, 0f, Mathf.Max(0f, _rect.width - 110f), 60f));
        GUI.skin.verticalScrollbar = oldScrollbar;
        GUI.skin.verticalScrollbarThumb = oldThumb;
    }

    private void DrawHeader()
    {
        if (_page != Page.Bag)
        {
            GUILayout.Space(_page == Page.Market ? 80f : _rect.width < 600f ? 48f : 57f);
            if (_page == Page.Guide)
                GUILayout.Label("还真一世，计较千年，岂是一人一念可证之；仙路无尽，长生未必是生！", _subtitle);
            return;
        }
        GUILayout.BeginHorizontal(_page == Page.Bag ? _bagPanel : _page == Page.Market ? _marketPanel : _card, GUILayout.Height(62f));
        bool narrowMarket = _page == Page.Market && _rect.width < 400f;
        if (!narrowMarket && _page != Page.Market) DrawEntryIcon(_page == Page.Guide ? "ui/Icons/GuideEntrance"
            : _page == Page.Bag ? "ui/Icons/QiankunBagEntrance" : "ui/Icons/TianxuanMirrorEntrance", 42f);
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
        GUILayout.Label("乾坤袋 · 纳灵藏珍", _title);
        if (!narrowMarket && !(_page == Page.Market && _rect.height < 400f)) GUILayout.Label(_page == Page.Guide ? "还真一世，计较千年，岂是一人一念可证之；仙路无尽，长生未必是生！"
            : _page == Page.Bag ? "分类收纳丹药、灵材、符箓与法宝。" : LM.Get("mclsl_xianlu_market_subtitle"), _subtitle);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(narrowMarket ? "×" : "× 关闭", _page == Page.Bag ? _bagButton : _page == Page.Market ? _marketTab : _button,
                GUILayout.Width(narrowMarket ? 42f : 88f), GUILayout.Height(34f))) CloseWindow();
        GUILayout.EndHorizontal();
    }

    private void DrawBagPage()
    {
        if (!TryGetActor())
        {
            DrawNoActor("先在世界中选中一名在世修士，再打开乾坤袋。", "乾坤袋属于人物，不同修士各自保管物品。");
            return;
        }
        MclslBagState bag = MclslBagSystem.Peek(_actor);
        if (MclslBagSystem.IsLocked(_actor))
        {
            MclslLocalizationBridge.TryResolveRuntimeKey("mclsl_qiankun_corrupt_warning", out string warning);
            GUILayout.Label(string.IsNullOrWhiteSpace(warning) ? "乾坤袋数据损坏，已保留原文并暂停写入" : warning, _section);
            return;
        }
        List<MclslOwnedItem> visibleItems = GetBagItems(bag, _category);
        if (_category == 4)
        {
            List<MclslMentorshipBook> books = bag.Books ?? new List<MclslMentorshipBook>();
            if (!books.Any(book => "book:" + book.BookId == _selectedBagItemKey))
                _selectedBagItemKey = books.Count == 0 ? string.Empty : "book:" + books[0].BookId;
        }
        else if (_category != 5) EnsureBagSelection(visibleItems);

        GUILayout.BeginHorizontal(_bagPanel, GUILayout.Height(56f));
        DrawEntryIcon("ui/Icons/QiankunBagEntrance", 42f);
        GUILayout.BeginVertical();
        GUILayout.Label(MclslActorAccessor.DisplayName(_actor), _section);
        GUILayout.Label("专属纳物匣　·　法宝、丹药、符箓、材料与传承典籍", _small);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        DrawBagSummary("收纳件数", (int)Math.Min(int.MaxValue, _bagTotalCount));
        GUILayout.Space(18f);
        DrawBagSummary("贡献度", MclslActorAccessor.GetMoney(_actor, MclslActorDataKeys.Contribution));
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);

        GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
        DrawBagCategoryShelf(bag);
        GUILayout.Space(8f);
        DrawBagInventory(visibleItems);
        GUILayout.Space(8f);
        DrawBagItemDetail(visibleItems);
        GUILayout.EndHorizontal();
    }

    private void DrawBagSummary(string label, long value)
    {
        GUILayout.BeginVertical(GUILayout.Width(112f));
        GUILayout.Label(label, _muted, GUILayout.Height(20f));
        GUILayout.Label(value.ToString(), _section, GUILayout.Height(24f));
        GUILayout.EndVertical();
    }

    private void DrawBagCategoryShelf(MclslBagState bag)
    {
        float width = Mathf.Clamp(_rect.width * 0.18f, 132f, 184f);
        GUILayout.BeginVertical(_bagShelf, GUILayout.Width(width), GUILayout.ExpandHeight(true));
        GUILayout.Label("乾坤袋", _section);
        GUILayout.Label("按物品类别存放", _muted);
        GUILayout.Space(8f);
        for (int i = 0; i < BagCategories.Length; i++)
        {
            int index = i;
            int count = CountBagCategory(bag, i);
            string label = BagCategories[i] + "\n" + count + (i == 4 ? " 册" : i == 5 ? " 门" : " 件");
            if (GUILayout.Button(label, _category == i ? _bagSelectedCategory : _bagCategory,
                    GUILayout.Height(50f), GUILayout.ExpandWidth(true)))
            {
                _category = index;
                _bagScroll = Vector2.zero;
        _selectedBagItemKey = string.Empty;
        _expandedMarketItemId = string.Empty;
            }
            GUILayout.Space(4f);
        }
        GUILayout.FlexibleSpace();
        GUILayout.Label("法宝每种唯一一件\n丹药、符箓与材料按数量堆叠", _small);
        GUILayout.EndVertical();
    }

    private void DrawBagInventory(List<MclslOwnedItem> items)
    {
        GUILayout.BeginVertical(_bagPanel, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.BeginHorizontal();
        GUILayout.Label(BagCategories[_category] + " · 藏珍格", _section);
        GUILayout.FlexibleSpace();
        GUILayout.Label(items.Count + " 种", _muted);
        GUILayout.EndHorizontal();
        GUILayout.Label("点选格位查看物品详情与可用操作。", _muted);
        GUILayout.Space(6f);
        _bagScroll = GUILayout.BeginScrollView(_bagScroll, false, true, GUIStyle.none,
            GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));
        if (_category == 5)
        {
            int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(_actor));
            string studyTarget = MclslSpellProgression.StudyTarget(_actor);
            if (MclslSpellSystem.KnownView(_actor).Count == 0) GUILayout.Label("尚未学会法术", _section);
            foreach (MclslSpellDefinition spell in MclslSpellSystem.KnownView(_actor))
            {
                bool learned = true;
                int level = MclslSpellProgression.Level(_actor, spell.Id);
                int xp = MclslSpellProgression.Experience(_actor, spell.Id);
                int threshold = MclslSpellProgression.ExperienceThreshold(spell, level);
                string progress = learned
                    ? (level >= 10 ? "10/10 级 · 已臻圆满" : level + "/10 级 · 精进 " + xp + "/" + threshold)
                    : "未学 · 参悟 " + (studyTarget == spell.Id ? MclslSpellProgression.StudyProgress(_actor) : 0)
                        + "/" + MclslSpellProgression.StudyThreshold(spell);
                GUILayout.BeginHorizontal(_card, GUILayout.Height(48f));
                Sprite icon = SpriteTextureLoader.getSprite(spell.IconPath);
                if (icon != null) GUILayout.Label(icon.texture, GUILayout.Width(38f), GUILayout.Height(38f));
                GUILayout.Label(MclslRealmIds.Display(MclslRealmIds.Ordered[spell.MinRealm]) + " · " + spell.Name
                    + "　" + progress + "\n耗灵力 " + spell.ManaCost + "　" + spell.Description, _small);
                if (studyTarget == spell.Id)
                {
                    if (GUILayout.Button("停止参悟", _bagButton, GUILayout.Width(76f), GUILayout.Height(34f)))
                        MclslSpellProgression.SetStudyTarget(_actor, string.Empty);
                }
                else if (level < 10 && GUILayout.Button(learned ? "精进" : "参悟", _bagButton, GUILayout.Width(64f), GUILayout.Height(34f)))
                    MclslSpellProgression.SetStudyTarget(_actor, spell.Id);
                GUILayout.EndHorizontal();
            }
        }
        else if (_category == 4)
        {
            List<MclslMentorshipBook> books = MclslBagSystem.Peek(_actor).Books ?? new List<MclslMentorshipBook>();
            if (books.Count == 0)
            {
                GUILayout.Space(28f);
                GUILayout.Label("尚无师门典籍", _section);
                GUILayout.Label("成为旧法修士的弟子后，会在此收录师门传承。", _muted);
            }
            const float bookHeight = 62f;
            if (Event.current.type == EventType.Layout)
            {
                _bookFirstRow = Mathf.Clamp((int)(_bagScroll.y / bookHeight) - 1, 0, books.Count);
                _bookLastRow = Math.Min(books.Count, _bookFirstRow + (int)(_rect.height / bookHeight) + 2);
            }
            if (_bookFirstRow > 0) GUILayout.Space(_bookFirstRow * bookHeight);
            for (int i = _bookFirstRow; i < Math.Min(_bookLastRow, books.Count); i++)
            {
                MclslMentorshipBook book = books[i];
                string bookKey = "book:" + book.BookId;
                if (GUILayout.Button((bookKey == _selectedBagItemKey ? "◆ " : "◇ ") + book.TechniqueName + "\n师父：" + book.TeacherName + "　参悟 " + book.Progress + "%", _bagCategory, GUILayout.Height(58f)))
                    _selectedBagItemKey = bookKey;
                GUILayout.Space(4f);
            }
            if (_bookLastRow < books.Count) GUILayout.Space((books.Count - _bookLastRow) * bookHeight);
        }
        else if (items.Count == 0)
        {
            GUILayout.Space(28f);
            GUILayout.Label("此分匣尚空", _section);
            GUILayout.Label("探索、采集或制作所得的物品会收纳在这里。", _muted);
        }
        else
        {
            int columns = Mathf.Clamp(Mathf.FloorToInt((_rect.width - 570f) / 168f) + 1, 1, 4);
            const float rowHeight = 148f;
            int totalRows = (items.Count + columns - 1) / columns;
            if (Event.current.type == EventType.Layout)
            {
                _bagFirstRow = Mathf.Clamp((int)(_bagScroll.y / rowHeight) - 1, 0, totalRows);
                _bagLastRow = Math.Min(totalRows, _bagFirstRow + (int)(_rect.height / rowHeight) + 2);
            }
            if (_bagFirstRow > 0) GUILayout.Space(_bagFirstRow * rowHeight);
            for (int start = _bagFirstRow * columns; start < Math.Min(items.Count, _bagLastRow * columns); start += columns)
            {
                GUILayout.BeginHorizontal();
                int end = Math.Min(start + columns, items.Count);
                for (int i = start; i < end; i++) DrawBagSlot(items[i]);
                if (end - start < columns) GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(6f);
            }
            if (_bagLastRow < totalRows) GUILayout.Space((totalRows - _bagLastRow) * rowHeight);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawBagSlot(MclslOwnedItem owned)
    {
        MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
        if (item == null) return;
        string key = BagItemSelectionKey(item, owned);
        bool selected = string.Equals(key, _selectedBagItemKey, StringComparison.Ordinal);
        Rect rect = GUILayoutUtility.GetRect(140f, 142f, GUILayout.ExpandWidth(true), GUILayout.Height(142f));
        GUI.Box(rect, GUIContent.none, selected ? _bagSelectedSlot : _bagSlot);
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) _selectedBagItemKey = key;

        DrawIcon(new Rect(rect.x + 11f, rect.y + 12f, 48f, 48f), item);
        float textX = rect.x + 68f;
        float textWidth = Mathf.Max(48f, rect.width - 78f);
        GUI.Label(new Rect(textX, rect.y + 10f, textWidth, 40f), item.Name, _body);
        GUI.Label(new Rect(textX, rect.y + 47f, textWidth, 22f), DisplayGradeName(item), _small);
        GUI.Label(new Rect(rect.x + 12f, rect.y + 70f, rect.width - 24f, 23f),
            item.Category == "Artifact" ? "同类法宝 × " + Math.Max(1, owned.Count) : "堆叠 × " + Math.Max(1, owned.Count), _section);
        if (item.Category == "Artifact")
        {
            GUI.Label(new Rect(rect.x + 12f, rect.y + 100f, rect.width - 24f, 22f), "耐久与实例见右侧详情", _small);
        }
        else
        {
            GUI.Label(new Rect(rect.x + 12f, rect.y + 100f, rect.width - 24f, 22f), CategoryFor(item.Category), _small);
        }
    }

    private void DrawBagItemDetail(List<MclslOwnedItem> items)
    {
        float width = Mathf.Clamp(_rect.width * 0.24f, 178f, 246f);
        GUILayout.BeginVertical(_bagShelf, GUILayout.Width(width), GUILayout.ExpandHeight(true));
        GUILayout.Label("藏品签", _section);
        if (_category != 4 && _category != 5) DrawEquippedArtifactControl();
        GUILayout.Space(6f);
        if (_category == 4)
        {
            DrawMentorshipBookDetail();
            GUILayout.EndVertical();
            return;
        }
        if (_category == 5)
        {
            GUILayout.Label("灵力 " + MclslSpellSystem.CurrentMana(_actor) + "/" + MclslSpellSystem.MaxMana(_actor), _section);
            GUILayout.Label("已学法术在战斗中自动消耗灵力施展；卷轴可在左侧研习。", _muted);
            GUILayout.EndVertical();
            return;
        }
        MclslOwnedItem owned = FindBagSelection(items, out MclslItemDefinition item);
        if (owned == null || item == null)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label("选择一格藏品", _section);
            GUILayout.Label("此处显示品阶、功效、数量与器物耐久。", _muted);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            return;
        }

        GUILayout.BeginHorizontal(_bagPanel, GUILayout.MinHeight(74f));
        DrawIcon(item, 58f);
        GUILayout.BeginVertical();
        GUILayout.Label(item.Name, _section);
        GUILayout.Label(DisplayGradeName(item) + " · " + CategoryFor(item.Category), _small);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);
        GUILayout.Label("物品功效", _muted);
        GUILayout.Label(item.EffectText, _body, GUILayout.MinHeight(70f));
        GUILayout.Space(8f);
        if (item.Category == "Artifact")
        {
            GUILayout.Label("制作配方　" + MclslItemCatalog.IngredientDisplayName(item.IngredientA)
                + " + " + MclslItemCatalog.IngredientDisplayName(item.IngredientB)
                + "（炼器师品阶需达到" + DisplayGradeName(item) + "）", _muted);
            if (_detailRevision != _bagRevision || _detailItem != item.Id)
            {
                _detailInstances.Clear();
                foreach (MclslOwnedItem instance in MclslBagSystem.Peek(_actor).Items)
                    if (instance.ItemId == item.Id) _detailInstances.Add(instance);
                _detailRevision = _bagRevision; _detailItem = item.Id;
            }
            List<MclslOwnedItem> instances = _detailInstances;
            GUILayout.Label("槽位　" + MclslArtifactSystem.SlotDisplayName(item.EquipmentSlot) + "　·　唯一持有", _muted);
            string equippedArtifactId = MclslArtifactSystem.EquippedArtifactId(_actor, item.EquipmentSlot);
            if (equippedArtifactId == item.Id)
                GUILayout.Label("当前装备于" + MclslArtifactSystem.SlotDisplayName(item.EquipmentSlot) + "栏", _section);
            _artifactInstanceScroll = GUILayout.BeginScrollView(_artifactInstanceScroll, false, true,
                GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(Mathf.Min(165f, Math.Max(76f, instances.Count * 42f))));
            for (int i = 0; i < instances.Count; i++)
            {
                MclslOwnedItem instance = instances[i];
                bool instanceEquipped = MclslInventoryDataRules.IsEquipped(MclslBagSystem.Peek(_actor), instance);
                GUILayout.BeginHorizontal(_card, GUILayout.MinHeight(36f));
                GUILayout.Label("数量 " + instance.Count + "　耐久 " + Mathf.Clamp(instance.Durability, 0, 100) + "%　" + (instanceEquipped ? "已装备" : "收纳中"), _small);
                string action = instanceEquipped ? "已装备"
                    : equippedArtifactId.Length > 0 ? "替换" : "装备";
                if (action != "已装备" && GUILayout.Button(action, _bagButton, GUILayout.Width(52f), GUILayout.Height(30f)))
                {
                    MclslArtifactSystem.TryEquip(_actor, instance.InstanceId,
                        replaceExisting: equippedArtifactId.Length > 0);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Label("当前收纳　" + Math.Max(1, owned.Count) + " 件", _section);
        }
        GUILayout.FlexibleSpace();
        if ((item.Category == "Pill" || item.Category == "Talisman")
            && GUILayout.Button(item.Category == "Pill" ? "服用" : "施用", _bagPrimaryButton, GUILayout.Height(40f)))
        {
            if (MclslItemUseSystem.TryUse(_actor, item.Id))
            {
                _bagActionMessage = "使用成功。";
                MclslBagState refreshed = MclslBagSystem.Peek(_actor);
                List<MclslOwnedItem> remaining = GetBagItems(refreshed, _category);
                if (!ContainsBagSelection(remaining, _selectedBagItemKey)) _selectedBagItemKey = string.Empty;
            }
            else _bagActionMessage = MclslItemUseSystem.ManualUseFailureReason(_actor, item.Id);
        }
        if (!string.IsNullOrWhiteSpace(_bagActionMessage)) GUILayout.Label(_bagActionMessage, _muted);
        if (item.Category == "Artifact")
        {
            string equippedId = MclslArtifactSystem.EquippedArtifactId(_actor, item.EquipmentSlot);
            bool isEquipped = equippedId == item.Id;
            string action = isEquipped ? "卸下"
                : equippedId.Length > 0 ? "装备 / 更换" : "装备此法宝";
            if (GUILayout.Button(action, _bagButton, GUILayout.Height(36f)))
            {
                if (isEquipped) MclslArtifactSystem.TryUnequip(_actor, item.EquipmentSlot);
                else MclslArtifactSystem.TryEquip(_actor, owned.InstanceId,
                    replaceExisting: equippedId.Length > 0);
            }
        }
        GUILayout.EndVertical();
    }

    private void DrawEquippedArtifactControl()
    {
        GUILayout.BeginVertical(_bagPanel);
        GUILayout.Label("六个法宝槽", _muted);
        foreach (MclslArtifactEquipmentSlot slot in Enum.GetValues(typeof(MclslArtifactEquipmentSlot)))
        {
            MclslOwnedItem owned = MclslArtifactSystem.EquippedArtifact(_actor, slot);
            MclslItemDefinition equipped = MclslItemCatalog.Get(owned?.ItemId);
            GUILayout.BeginHorizontal();
            GUILayout.Label(MclslArtifactSystem.SlotDisplayName(slot), _small, GUILayout.Width(48f));
            GUILayout.Label(equipped?.Name ?? "空", _section);
            if (equipped != null && GUILayout.Button("卸下", _bagButton, GUILayout.Width(46f), GUILayout.Height(28f))) MclslArtifactSystem.TryUnequip(_actor, slot);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    private void DrawMarketPage()
    {
        // Match the bounded content area and reserve its vertical scrollbar and right-hand gap.
        float contentWidth = Mathf.Max(120f, _rect.width - _marketWindow.padding.horizontal - 20f);
        bool narrow = contentWidth < 640f;
        float categoryWidth = narrow ? 0f : Mathf.Clamp(contentWidth * 0.18f, 150f, 220f);
        float boardWidth = narrow ? contentWidth : contentWidth - categoryWidth - 12f;
        float boardHeight = Mathf.Max(190f, _rect.height - (narrow ? 296f : 346f));
        _marketPageScroll.x = 0f;
        _marketPageScroll = GUILayout.BeginScrollView(_marketPageScroll, false, true,
            GUIStyle.none, GUI.skin.verticalScrollbar,
            GUILayout.MinHeight(0f), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.BeginVertical(GUILayout.Width(contentWidth));
        GUILayout.BeginHorizontal(GUILayout.Width(contentWidth));
        GUILayout.Space(60f);
        GUILayout.Label(LM.Get("mclsl_xianlu_market_stats"), _section, GUILayout.Width(100f));
        GUILayout.Label(LM.Get("mclsl_xianlu_market_subtitle"), _muted, GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();
        float tickerWidth = (contentWidth - 16f) / 3f;
        GUILayout.BeginHorizontal(GUILayout.Width(contentWidth));
        DrawMarketTicker(LM.Get("mclsl_xianlu_market_active"), _marketTotalText, "件", tickerWidth);
        GUILayout.Space(8f);
        DrawMarketTicker(LM.Get("mclsl_xianlu_market_listed_today"), _marketListedText, "", tickerWidth);
        GUILayout.Space(8f);
        DrawMarketTicker(LM.Get("mclsl_xianlu_market_purchased_today"), _marketPurchasedText, "", tickerWidth);
        GUILayout.EndHorizontal();
        GUILayout.Space(5f);
        GUILayout.Label(_marketYearText, _muted, GUILayout.Height(22f));
        GUILayout.Space(5f);
        if (narrow) DrawMarketCategoryTabs(contentWidth);
        GUILayout.BeginHorizontal(GUILayout.Width(contentWidth));
        if (!narrow)
        {
            DrawMarketCategoryRail(categoryWidth, boardHeight + 89f);
            GUILayout.Space(12f);
        }
        GUILayout.BeginVertical(GUILayout.Width(boardWidth));
        GUILayout.BeginHorizontal(GUILayout.Width(boardWidth));
        string[] views = MarketViewLabels;
        for (int i = 0; i < views.Length; i++)
        {
            int view = i;
            if (GUILayout.Button(LM.Get(views[i]), _marketView == view ? _marketSelectedTab : _marketTab,
                GUILayout.Width((boardWidth - 8f) / 3f), GUILayout.Height(38f)))
            {
                _marketView = view;
                _marketScroll = Vector2.zero;
                RebuildMarketRows();
            }
            if (i < views.Length - 1) GUILayout.Space(4f);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        if (_marketView == 0) DrawMarketListings(boardWidth, boardHeight);
        else DrawMarketActivity(boardWidth, boardHeight);
        GUILayout.Space(7f);
        if (GUILayout.Button(_marketRulesExpanded ? LM.Get("mclsl_market_rules_hide") : LM.Get("mclsl_market_rules_show"),
            _marketTab, GUILayout.Width(boardWidth), GUILayout.Height(34f)))
            _marketRulesExpanded = !_marketRulesExpanded;
        if (_marketRulesExpanded)
        {
            GUILayout.Space(5f);
            DrawMarketExchangeInfo(boardWidth, 198f);
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);
        DrawMarketDetail(contentWidth, 174f);
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    private void DrawMarketTicker(string label, string value, string unit, float width)
    {
        GUILayout.BeginVertical(_marketInset, GUILayout.Width(width), GUILayout.Height(72f));
        GUILayout.Label(label, _muted);
        GUILayout.Label(value + (unit.Length == 0 ? string.Empty : " " + unit), _section);
        GUILayout.EndVertical();
    }

    private void DrawMarketCategoryRail(float width, float height)
    {
        GUILayout.BeginVertical(XianLuUIStyles.PanelStyle, GUILayout.Width(width), GUILayout.Height(height));
        GUILayout.Label(LM.Get("mclsl_xianlu_market_categories"), _section);
        GUILayout.Label(LM.Get("mclsl_xianlu_market_categories_hint"), _muted);
        GUILayout.Space(8f);
        _marketCategoryScroll = GUILayout.BeginScrollView(_marketCategoryScroll, false, true);
        for (int i = 0; i < Categories.Length; i++)
        {
            int index = i;
            GUIStyle style = _category == index ? _marketSelectedTab : _marketTab;
            if (GUILayout.Button(_marketCategoryLabels[i], style, GUILayout.Height(37f)))
            {
                _category = index;
                _selectedMarketGroup = null;
                _marketOffersScroll = Vector2.zero;
                _marketScroll = Vector2.zero;
                RebuildMarketRows();
            }
        }
        GUILayout.EndScrollView();
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }

    private void DrawMarketCategoryTabs(float width)
    {
        int columns = width >= 520f ? 4 : 2;
        float buttonWidth = (width - (columns - 1) * 4f) / columns;
        for (int i = 0; i < Categories.Length; i++)
        {
            if (i % columns == 0) GUILayout.BeginHorizontal(GUILayout.Width(width));
            int index = i;
            if (GUILayout.Button(_marketCategoryLabels[i], _category == i ? _marketSelectedTab : _marketTab,
                GUILayout.Width(buttonWidth), GUILayout.Height(34f)))
            {
                _category = index;
                _selectedMarketGroup = null;
                _marketOffersScroll = Vector2.zero;
                _marketScroll = Vector2.zero;
                RebuildMarketRows();
            }
            if (i % columns != columns - 1) GUILayout.Space(4f);
            if (i % columns == columns - 1) GUILayout.EndHorizontal();
        }
    }

    private void DrawMarketListings(float width, float height) => DrawMarketBoard(width, height, true);
    private void DrawMarketActivity(float width, float height) => DrawMarketBoard(width, height, false);

    private void DrawMarketBoard(float width, float height, bool listings)
    {
        GUILayout.BeginVertical(_marketPanel, GUILayout.Width(width), GUILayout.Height(height));
        GUILayout.Label(listings ? _marketBoardTitle
            : _marketView == 1 ? "修士上架动态" : "修士购入成交", _section);
        GUILayout.Label(listings ? _marketBoardText : _marketYearText, _muted);
        if (width >= 520f)
        {
            GUILayout.BeginHorizontal(_marketInset, GUILayout.Height(26f));
            GUILayout.Label(LM.Get("mclsl_market_column_item"), _small);
            GUILayout.Label(LM.Get("mclsl_market_column_price"), _small, GUILayout.Width(128f));
            GUILayout.EndHorizontal();
        }
        float viewportHeight = Mathf.Max(100f, height - (width >= 520f ? 112f : 78f));
        Rect viewport = GUILayoutUtility.GetRect(100f, 10000f, viewportHeight,
            viewportHeight, GUILayout.ExpandWidth(true));
        bool layout = Event.current.type == EventType.Layout;
        float widthInside = Mathf.Max(120f, viewport.width - 18f);
        float measuredWidth = Mathf.Max(120f, Mathf.Floor(widthInside / 16f) * 16f);
        if (!layout && Mathf.Abs(measuredWidth - _marketRowWidth) > 1f)
        {
            int anchor = FirstMarketRow(_marketScroll.y);
            _marketRowWidth = measuredWidth;
            RebuildMarketRows();
            if (anchor < _marketRows.Count) _marketScroll.y = _marketRows[anchor].Top;
        }
        _marketScroll.y = Mathf.Clamp(_marketScroll.y, 0f, Mathf.Max(0f, _marketRowsHeight - viewport.height));
        Vector2 scroll = GUI.BeginScrollView(viewport, _marketScroll,
            new Rect(0f, 0f, widthInside, Mathf.Max(viewport.height, _marketRowsHeight)), false, true);
        if (!layout) _marketScroll = scroll;
        bool changed = false;
        if (!layout)
        {
            long sample = MclslPerformanceProbe.Begin();
            try
            {
                int first = FirstMarketRow(Mathf.Max(0f, scroll.y - 220f));
                float bottom = scroll.y + viewport.height + 220f;
                for (int i = first; i < _marketRows.Count && _marketRows[i].Top < bottom; i++)
                {
                    MarketDisplayRow row = _marketRows[i];
                    GUI.Box(new Rect(0f, row.Top, widthInside, row.Height - 5f), GUIContent.none,
                        row.Item == null ? _marketInset : _marketRow);
                    if (row.Item == null)
                    {
                        GUI.Label(new Rect(8f, row.Top + 5f, widthInside - 16f, row.Height - 12f), row.Detail, _small);
                        continue;
                    }
                    DrawIcon(new Rect(8f, row.Top + 10f, 40f, 40f), row.Item);
                    if (_marketRowWidth >= 360f)
                    {
                        float priceWidth = Mathf.Min(128f, widthInside * 0.30f);
                        float textWidth = Mathf.Max(40f, widthInside - priceWidth - 68f);
                        GUI.Label(new Rect(56f, row.Top + 7f, textWidth, row.TitleHeight), row.Title, _body);
                        GUI.Label(new Rect(56f, row.Top + 9f + row.TitleHeight, textWidth,
                            row.DetailHeight), row.Detail, _small);
                        GUI.Label(new Rect(widthInside - priceWidth - 6f, row.Top + 7f, priceWidth, row.PriceHeight), row.Price, _priceStyle);
                        GUI.Label(new Rect(widthInside - priceWidth - 6f, row.Top + 9f + row.PriceHeight, priceWidth,
                            row.PriceDetailHeight), row.PriceDetail, _muted);
                    }
                    else
                    {
                        float labelWidth = Mathf.Max(40f, widthInside - 64f);
                        float priceTop = row.Top + row.DetailTop + row.DetailHeight + 3f;
                        GUI.Label(new Rect(56f, row.Top + 7f, labelWidth, row.TitleHeight), row.Title, _body);
                        GUI.Label(new Rect(8f, row.Top + row.DetailTop, widthInside - 16f, row.DetailHeight), row.Detail, _small);
                        GUI.Label(new Rect(8f, priceTop, widthInside - 16f, row.PriceHeight), row.Price, _priceStyle);
                        GUI.Label(new Rect(8f, priceTop + row.PriceHeight + 2f, widthInside - 16f,
                            row.PriceDetailHeight), row.PriceDetail, _muted);
                    }
                    float actionWidth = widthInside >= 520f ? 128f : widthInside - 12f;
                    float actionX = widthInside >= 520f ? widthInside - actionWidth - 6f : 6f;
                    if (row.Group != null && GUI.Button(new Rect(actionX, row.Top + row.Height - 39f, actionWidth, 28f),
                        _expandedMarketItemId == row.Item.Id ? "收起卖家挂单" : row.Button, _marketTab))
                    {
                        _selectedMarketGroup = row.Group;
                        _marketOffersScroll = Vector2.zero;
                        _expandedMarketItemId = _expandedMarketItemId == row.Item.Id ? string.Empty : row.Item.Id;
                        changed = true;
                    }
                }
                if (_marketRows.Count == 0)
                    XianLuUIRenderer.EmptyState(new Rect(8f, 12f, widthInside - 16f, 112f),
                        LM.Get("mclsl_xianlu_market_empty_title"), LM.Get("mclsl_xianlu_market_empty_body"));
            }
            finally { MclslPerformanceProbe.End("天玄镜.可见行绘制", sample); }
        }
        GUI.EndScrollView();
        GUILayout.EndVertical();
        if (changed) RebuildMarketRows();
    }

    private int FirstMarketRow(float top)
    {
        int low = 0, high = _marketRows.Count;
        while (low < high)
        {
            int middle = low + (high - low) / 2;
            MarketDisplayRow row = _marketRows[middle];
            if (row.Top + row.Height < top) low = middle + 1;
            else high = middle;
        }
        return low;
    }

    private void DrawMarketDetail(float width, float height)
    {
        GUILayout.BeginVertical(XianLuUIStyles.PanelStyle, GUILayout.Width(width), GUILayout.Height(height));
        GUILayout.Label(LM.Get("mclsl_xianlu_market_detail"), _section);
        GUILayout.Label(LM.Get("mclsl_xianlu_market_detail_hint"), _muted);
        _marketDetailScroll = GUILayout.BeginScrollView(_marketDetailScroll, false, true,
            GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));
        MarketOfferGroup group = _selectedMarketGroup;
        if (_marketView != 0)
        {
            GUILayout.Space(24f);
            GUILayout.Label(LM.Get("mclsl_xianlu_market_activity_hint"), _muted);
        }
        else if (group?.Summary?.Item == null)
        {
            GUILayout.Space(24f);
            GUILayout.Label(LM.Get("mclsl_xianlu_market_empty_title"), _section);
            GUILayout.Label(LM.Get("mclsl_xianlu_market_empty_body"), _muted);
        }
        else
        {
            MclslItemDefinition item = group.Summary.Item;
            GUILayout.Label(item.Name, _section);
            GUILayout.Label("品阶　" + DisplayGradeName(item), _body);
            GUILayout.Label("类别　" + Categories[_category], _body);
            GUILayout.Label("数量　" + group.Summary.Detail, _body);
            GUILayout.Label("价格　" + group.Summary.Price, _priceStyle);
            if (!string.IsNullOrWhiteSpace(item.EffectText))
            {
                GUILayout.Space(8f);
                GUILayout.Label(LM.Get("mclsl_xianlu_market_effect"), _section);
                GUILayout.Label(item.EffectText, _body);
            }
            GUILayout.Space(8f);
            GUILayout.Label(LM.Get("mclsl_xianlu_market_sellers"), _section);
            DrawMarketSellers(group);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawMarketSellers(MarketOfferGroup group)
    {
        const float rowHeight = 76f;
        float viewHeight = Mathf.Min(300f, Mathf.Max(rowHeight, group.Offers.Count * rowHeight));
        Rect viewport = GUILayoutUtility.GetRect(80f, 10000f, viewHeight, viewHeight, GUILayout.ExpandWidth(true));
        float innerWidth = Mathf.Max(80f, viewport.width - 18f);
        float contentHeight = Mathf.Max(viewHeight, group.Offers.Count * rowHeight);
        _marketOffersScroll.y = Mathf.Clamp(_marketOffersScroll.y, 0f, Mathf.Max(0f, contentHeight - viewHeight));
        _marketOffersScroll = GUI.BeginScrollView(viewport, _marketOffersScroll,
            new Rect(0f, 0f, innerWidth, contentHeight), false, true);
        int first = Mathf.Max(0, Mathf.FloorToInt(_marketOffersScroll.y / rowHeight));
        int last = Mathf.Min(group.Offers.Count, first + Mathf.CeilToInt(viewHeight / rowHeight) + 2);
        for (int i = first; i < last; i++)
        {
            MarketDisplayRow offer = group.Offers[i];
            float top = i * rowHeight;
            GUI.Label(new Rect(4f, top + 2f, innerWidth - 8f, 44f), offer.Detail, _small);
            if (MclslActorAccessor.Alive(offer.SellerActor)
                && GUI.Button(new Rect(4f, top + 48f, innerWidth - 8f, 25f),
                    LM.Get("mclsl_xianlu_market_view_seller"), _marketTab))
            {
                Actor seller = offer.SellerActor;
                CloseWindow();
                ActionLibrary.openUnitWindow(seller);
            }
        }
        GUI.EndScrollView();
    }

    private void DrawMarketExchangeInfo(float width, float height)
    {
        _marketInfoScroll = GUILayout.BeginScrollView(_marketInfoScroll, false, true,
            GUILayout.Width(width), GUILayout.Height(height));
        GUILayout.BeginVertical(_marketPanel);
        GUILayout.Label("万界撮合规则", _section);
        GUILayout.Space(8f);
        GUILayout.Label("全体修士依自身境界、职业、库存、贡献与灵石自动买卖。玩家无需选中人物或手动下单。", _body);
        GUILayout.Space(8f);
        GUILayout.Label("交易结算", _section);
        GUILayout.Label("买方可用贡献或灵石支付，卖方获得同种货币；物品与法宝耐久随订单交割。无有效买方时挂单留存。", _small);
        GUILayout.Space(8f);
        GUILayout.Label(_marketView == 0 ? "当前市场" : _marketView == 1 ? "近期上架" : "近期成交", _section);
        for (int i = 0; i < Categories.Length; i++)
            GUILayout.Label(Categories[i] + "　" + _marketCategoryCounts[i] + " 件", _small);
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(_marketInset);
        GUILayout.Label("双币种价格", _section);
        GUILayout.Label("成品依品阶递增；材料按稀有度定价。", _small);
        GUILayout.Label("挂单保留　·　成交有据　·　全域流通", _muted);
        GUILayout.EndVertical();
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    private static string SellerName(long sellerId)
    {
        if (sellerId == 0) return "无主家产";
        if (MclslActorRegistry.ResolveKnownOrWorld(sellerId, out Actor seller) && MclslActorAccessor.Alive(seller))
            return MclslActorAccessor.DisplayName(seller);
        return "云游修士";
    }

    // Copy mutable listings into presentation text once per open. Simulation stays independent.
    private void CaptureMarketSnapshot()
    {
        long sample = MclslPerformanceProbe.Begin();
        try
        {
            _marketSnapshotRun = MclslWorldRunRepository.Current;
            _marketRevision = MclslTianxuanMarket.Revision;
            _marketYearText = "第 " + MclslRuntime.CurrentYear() + " 年 · " + LM.Get("mclsl_market_open_snapshot");
            IReadOnlyList<MclslMarketListing> listings = MclslTianxuanMarket.Snapshot();
            IReadOnlyList<MclslMarketActivity> listed = MclslTianxuanMarket.ListingActivitySnapshot();
            IReadOnlyList<MclslMarketActivity> purchased = MclslTianxuanMarket.PurchaseActivitySnapshot();
            Dictionary<string, List<MclslMarketListing>> byItem = new(StringComparer.Ordinal);
            foreach (MclslMarketListing listing in listings)
            {
                MclslItemDefinition item = MclslItemCatalog.Get(listing?.Item?.ItemId);
                int category = item == null ? -1 : Array.IndexOf(CategoryIds, item.Category);
                if (category < 0) continue;
                int quantity = Math.Max(1, listing.Item.Count);
                _marketListingCounts[category] += quantity;
                _marketTotalCount += quantity;
                if (!byItem.TryGetValue(item.Id, out List<MclslMarketListing> offers)) byItem[item.Id] = offers = new();
                offers.Add(listing);
            }
            Dictionary<long, string> sellerNames = new();
            foreach (List<MclslMarketListing> offers in byItem.Values)
            {
                offers.Sort((a, b) => {
                    int price = ContributionPrice(a).CompareTo(ContributionPrice(b));
                    return price != 0 ? price : a.Year.CompareTo(b.Year);
                });
                MclslItemDefinition item = MclslItemCatalog.Get(offers[0].Item.ItemId);
                int category = Array.IndexOf(CategoryIds, item.Category);
                MarketOfferGroup group = new();
                HashSet<long> sellers = new();
                int total = 0;
                foreach (MclslMarketListing offer in offers)
                {
                    sellers.Add(offer.SellerId);
                    total += Math.Max(1, offer.Item.Count);
                    if (!sellerNames.TryGetValue(offer.SellerId, out string seller))
                        sellerNames[offer.SellerId] = seller = SellerName(offer.SellerId);
                    Actor sellerActor = null;
                    if (offer.SellerId > 0)
                        MclslActorRegistry.ResolveKnownOrWorld(offer.SellerId, out sellerActor);
                    int contribution = ContributionPrice(offer);
                    int stone = offer.SpiritStoneUnitPrice > 0 ? offer.SpiritStoneUnitPrice
                        : MclslTianxuanMarket.SpiritStonePrice(item, contribution);
                    group.Offers.Add(new MarketDisplayRow {
                        SellerActor = sellerActor,
                        Detail = "卖家 " + seller + " ×" + Math.Max(1, offer.Item.Count)
                            + "　贡献" + contribution + "／灵石" + stone + "　第 " + offer.Year + " 年"
                            + (item.Category == "Artifact" ? "　耐久 " + Mathf.Clamp(offer.Item.Durability, 0, 100) + "%" : ""), Height = 62f
                    });
                }
                int minimum = ContributionPrice(offers[0]), maximum = ContributionPrice(offers[offers.Count - 1]);
                group.Summary = new MarketDisplayRow {
                    Item = item, Title = item.Name + " · " + DisplayGradeName(item), Detail = total + " 件挂单　" + sellers.Count + " 位卖家",
                    Price = minimum == maximum ? minimum + " 贡献" : minimum + "—" + maximum + " 贡献",
                    PriceDetail = "另可用灵石", Button = "展开 " + offers.Count + " 条挂单", Height = 114f, Group = group
                };
                if (!_marketOfferGroups.TryGetValue(category, out List<MarketOfferGroup> groups))
                    _marketOfferGroups[category] = groups = new();
                groups.Add(group);
            }
            foreach (List<MarketOfferGroup> groups in _marketOfferGroups.Values)
                groups.Sort((a, b) => StringComparer.Ordinal.Compare(a.Summary.Item.Name, b.Summary.Item.Name));
            CaptureMarketActivities(listed, true, _marketListedRows, _marketListedCounts, ref _marketListedTotal);
            CaptureMarketActivities(purchased, false, _marketPurchasedRows, _marketPurchasedCounts, ref _marketPurchasedTotal);
            _marketTotalText = _marketTotalCount.ToString();
            int year = MclslRuntime.CurrentYear();
            int listedToday = 0, listedQuantityToday = 0, purchasedToday = 0, purchasedQuantityToday = 0;
            foreach (MclslMarketActivity activity in listed)
                if (activity != null && activity.Year == year) { listedToday++; listedQuantityToday += Math.Max(1, activity.Count); }
            foreach (MclslMarketActivity activity in purchased)
                if (activity != null && activity.Year == year) { purchasedToday++; purchasedQuantityToday += Math.Max(1, activity.Count); }
            _marketListedText = listedToday + "笔 · " + listedQuantityToday + "件";
            _marketPurchasedText = purchasedToday + "笔 · " + purchasedQuantityToday + "件";
            RebuildMarketRows();
        }
        finally { MclslPerformanceProbe.End("天玄镜.打开快照", sample); }
    }

    private static int ContributionPrice(MclslMarketListing listing) =>
        listing.ContributionUnitPrice > 0 ? listing.ContributionUnitPrice : listing.Price;

    private static void CaptureMarketActivities(IReadOnlyList<MclslMarketActivity> activities, bool listing,
        Dictionary<int, List<MarketDisplayRow>> rows, int[] counts, ref int total)
    {
        foreach (MclslMarketActivity record in activities)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(record?.ItemId);
            int category = item == null ? -1 : Array.IndexOf(CategoryIds, item.Category);
            if (category < 0) continue;
            int quantity = Math.Max(1, record.Count);
            counts[category] += quantity;
            total += quantity;
            int unitPrice = record.UnitPrice > 0 ? record.UnitPrice : record.Price;
            int price = record.TotalPrice > 0 ? record.TotalPrice : unitPrice * quantity;
            string currency = record.Currency == MclslTianxuanMarket.SpiritStoneCurrency ? "灵石" : "贡献";
            if (!rows.TryGetValue(category, out List<MarketDisplayRow> categoryRows)) rows[category] = categoryRows = new();
            categoryRows.Add(new MarketDisplayRow {
                Item = item, Title = item.Name + " · " + DisplayGradeName(item) + " ×" + quantity,
                Detail = listing ? "上架修士　" + record.ActorName : "购入　" + record.ActorName + "／售出　" + record.OtherActorName,
                Price = currency + price, PriceDetail = "单价 " + unitPrice + "\n第 " + record.Year + " 年", Height = 86f
            });
        }
    }

    private void RebuildMarketRows()
    {
        _marketRows.Clear();
        _marketRowsHeight = 0f;
        int[] counts = _marketView switch { 1 => _marketListedCounts, 2 => _marketPurchasedCounts, _ => _marketListingCounts };
        Array.Copy(counts, _marketCategoryCounts, counts.Length);
        for (int i = 0; i < counts.Length; i++) _marketCategoryLabels[i] = Categories[i] + "  " + counts[i] + "件";
        _marketBoardTitle = Categories[_category] + " · " + LM.Get("mclsl_xianlu_market_listing");
        if (_marketView == 0)
        {
            _marketOfferGroups.TryGetValue(_category, out List<MarketOfferGroup> groups);
            if (groups == null || groups.Count == 0) _selectedMarketGroup = null;
            else if (_selectedMarketGroup == null || !groups.Contains(_selectedMarketGroup))
                _selectedMarketGroup = groups[0];
            if (groups != null) foreach (MarketOfferGroup group in groups)
            {
                AddMarketRow(group.Summary);
                if (_expandedMarketItemId == group.Summary.Item.Id)
                    foreach (MarketDisplayRow row in group.Offers) AddMarketRow(row);
            }
            _marketBoardText = "在售 " + counts[_category] + " 件 · " + (groups?.Count ?? 0) + " 种";
        }
        else
        {
            Dictionary<int, List<MarketDisplayRow>> source = _marketView == 1 ? _marketListedRows : _marketPurchasedRows;
            if (source.TryGetValue(_category, out List<MarketDisplayRow> rows))
                foreach (MarketDisplayRow row in rows) AddMarketRow(row);
        }
    }

    private void AddMarketRow(MarketDisplayRow row)
    {
        if (_marketRowWidth > 0f && _body != null)
        {
            float width = _marketRowWidth;
            if (row.Item == null)
                row.Height = Mathf.Max(62f, MarketTextHeight(_small, row.Detail, width - 16f, 36f) + 20f);
            else if (width >= 360f)
            {
                float priceWidth = Mathf.Min(128f, width * 0.30f);
                float textWidth = Mathf.Max(40f, width - priceWidth - 68f);
                row.TitleHeight = MarketTextHeight(_body, row.Title, textWidth, 30f);
                row.DetailHeight = MarketTextHeight(_small, row.Detail, textWidth, 36f);
                row.PriceHeight = MarketTextHeight(_priceStyle, row.Price, priceWidth, 28f);
                row.PriceDetailHeight = MarketTextHeight(_muted, row.PriceDetail, priceWidth, 40f);
                float textHeight = row.TitleHeight + row.DetailHeight + 4f;
                float priceHeight = row.PriceHeight + row.PriceDetailHeight + 4f;
                row.Height = Mathf.Max(row.Group == null ? 86f : 114f,
                    Mathf.Max(60f, Mathf.Max(textHeight, priceHeight)) + 16f + (row.Group == null ? 0f : 35f));
            }
            else
            {
                float labelWidth = Mathf.Max(40f, width - 64f);
                row.TitleHeight = MarketTextHeight(_body, row.Title, labelWidth, 30f);
                row.DetailTop = Mathf.Max(60f, 10f + row.TitleHeight);
                row.DetailHeight = MarketTextHeight(_small, row.Detail, width - 16f, 24f);
                row.PriceHeight = MarketTextHeight(_priceStyle, row.Price, width - 16f, 24f);
                row.PriceDetailHeight = MarketTextHeight(_muted, row.PriceDetail, width - 16f, 20f);
                row.Height = row.DetailTop + row.DetailHeight + row.PriceHeight + row.PriceDetailHeight
                    + 17f + (row.Group == null ? 0f : 35f);
            }
        }
        row.Top = _marketRowsHeight;
        _marketRows.Add(row);
        _marketRowsHeight += row.Height;
    }

    private static float MarketTextHeight(GUIStyle style, string value, float width, float minimum)
        => Mathf.Max(minimum, style.CalcHeight(new GUIContent(value ?? string.Empty), Mathf.Max(20f, width)));

    private void ReleaseMarketSnapshot()
    {
        _marketSnapshotRun = null;
        _marketRevision = -1;
        _selectedMarketGroup = null;
        _marketOfferGroups.Clear(); _marketListedRows.Clear(); _marketPurchasedRows.Clear(); _marketRows.Clear();
        Array.Clear(_marketListingCounts, 0, _marketListingCounts.Length);
        Array.Clear(_marketListedCounts, 0, _marketListedCounts.Length);
        Array.Clear(_marketPurchasedCounts, 0, _marketPurchasedCounts.Length);
        Array.Clear(_marketCategoryCounts, 0, _marketCategoryCounts.Length);
        Array.Clear(_marketCategoryLabels, 0, _marketCategoryLabels.Length);
        _marketListedTotal = _marketPurchasedTotal = _marketTotalCount = 0;
        _marketRowsHeight = 0f;
        _expandedMarketItemId = string.Empty;
        _marketYearText = _marketListedText = _marketPurchasedText = _marketTotalText = _marketBoardText = _marketBoardTitle = string.Empty;
    }

    private void RefreshMarketSnapshot()
    {
        Vector2 scroll = _marketScroll;
        Vector2 pageScroll = _marketPageScroll;
        Vector2 categoryScroll = _marketCategoryScroll;
        string selectedId = _selectedMarketGroup?.Summary?.Item?.Id;
        ReleaseMarketSnapshot();
        CaptureMarketSnapshot();
        if (!string.IsNullOrEmpty(selectedId)
            && _marketOfferGroups.TryGetValue(_category, out List<MarketOfferGroup> groups))
            foreach (MarketOfferGroup group in groups)
                if (group.Summary.Item.Id == selectedId) { _selectedMarketGroup = group; break; }
        _marketScroll = scroll;
        _marketPageScroll = pageScroll;
        _marketCategoryScroll = categoryScroll;
    }

    private List<MclslOwnedItem> GetBagItems(MclslBagState bag, int category)
    {
        long revision = MclslBagSystem.Revision(_actor);
        string spells = MclslActorAccessor.GetString(_actor, MclslActorDataKeys.LearnedSpells);
        long spellRevision = MclslSpellProgression.Revision(_actor);
        if (_bagRevision != revision || _spellToken != spells || _spellRevision != spellRevision)
        {
            // Keep the same control sequence between Layout and Repaint/input.
            if (_bagRevision >= 0 && Event.current.type != EventType.Layout) return _bagViews[category];
            long sample = MclslPerformanceProbe.Begin();
            try
            {
                for (int i = 0; i < _bagViews.Length; i++) { _bagViews[i].Clear(); _bagCategoryCounts[i] = 0; }
                _artifactGroups.Clear(); _bagTotalCount = 0;
                foreach (MclslOwnedItem owned in bag.Items)
                {
                    MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
                    if (item == null) continue;
                    if (item.Category == "SpellScroll") continue;
                    _bagTotalCount += owned.Count;
                    for (int i = 0; i < _bagViews.Length; i++)
                    {
                        if (!MatchesBagCategory(item, i)) continue;
                        _bagCategoryCounts[i] += owned.Count;
                        if (item.Category != "Artifact") { _bagViews[i].Add(owned); continue; }
                        if (!_artifactGroups.TryGetValue(owned.ItemId, out MclslOwnedItem group))
                        {
                            group = new() { ItemId = owned.ItemId, Count = 0, InstanceId = owned.InstanceId };
                            _artifactGroups[owned.ItemId] = group; _bagViews[i].Add(group);
                        }
                        group.Count = (int)Math.Min(int.MaxValue, (long)group.Count + owned.Count);
                    }
                }
                _knownSpells = MclslSpellSystem.Known(_actor);
                _bagCategoryCounts[4] = bag.Books.Count;
                _bagCategoryCounts[5] = _knownSpells.Count;
                _spellToken = spells; _bagRevision = revision; _spellRevision = spellRevision;
            }
            finally { MclslPerformanceProbe.End("UI.乾坤袋投影", sample); }
        }
        return _bagViews[category];
    }

    private int CountBagCategory(MclslBagState bag, int category)
        => (int)Math.Min(int.MaxValue, _bagCategoryCounts[category]);

    private void ReleaseBagSnapshot()
    {
        for (int i = 0; i < _bagViews.Length; i++) _bagViews[i].Clear();
        Array.Clear(_bagCategoryCounts, 0, _bagCategoryCounts.Length);
        _artifactGroups.Clear(); _detailInstances.Clear(); _knownSpells.Clear();
        _bagRevision = _detailRevision = _spellRevision = -1; _bagTotalCount = 0;
        _spellToken = _detailItem = null; _bagFirstRow = _bagLastRow = _bookFirstRow = _bookLastRow = 0;
    }

    private static bool MatchesBagCategory(MclslItemDefinition item, int category) => category switch
    {
        0 => item.Category == "Artifact",
        1 => item.Category == "Pill",
        2 => item.Category == "Talisman",
        3 => item.Category is "Plant" or "SpiritObject" or "TalismanMaterial" or "Material",
        5 => false,
        _ => false
    };

    private void DrawMentorshipBookDetail()
    {
        GUILayout.Label("师门传承", _section);
        MclslMentorshipBook book = MclslBagSystem.Peek(_actor).Books?
            .FirstOrDefault(x => "book:" + x.BookId == _selectedBagItemKey)
            ?? MclslBagSystem.Peek(_actor).Books?.FirstOrDefault();
        if (book == null)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label("成为旧法修士的弟子后，师父所授功法与心得会收录于此。", _muted);
            GUILayout.FlexibleSpace();
            return;
        }
        GUILayout.Label(book.TechniqueName, _section);
        GUILayout.Label("师父　" + book.TeacherName, _body);
        GUILayout.Label("收徒年份　" + book.StartYear, _small);
        GUILayout.Label("传承参悟　" + book.Progress + "%", _small);
        GUILayout.Space(10f);
        GUILayout.Label("此典籍保存师父传下的旧法功法、法脉与修行心得。参悟进度会随年度师徒传授提高。", _muted);
    }

    private void EnsureBagSelection(List<MclslOwnedItem> items)
    {
        if (ContainsBagSelection(items, _selectedBagItemKey)) return;
        _selectedBagItemKey = items.Count == 0
            ? string.Empty
            : BagItemSelectionKey(MclslItemCatalog.Get(items[0].ItemId), items[0]);
    }

    private MclslOwnedItem FindBagSelection(List<MclslOwnedItem> items, out MclslItemDefinition definition)
    {
        foreach (MclslOwnedItem owned in items)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
            if (item != null && string.Equals(BagItemSelectionKey(item, owned), _selectedBagItemKey, StringComparison.Ordinal))
            {
                definition = item;
                return owned;
            }
        }
        definition = null;
        return null;
    }

    private static bool ContainsBagSelection(List<MclslOwnedItem> items, string key)
    {
        if (string.IsNullOrEmpty(key)) return false;
        foreach (MclslOwnedItem owned in items)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
            if (item != null && string.Equals(BagItemSelectionKey(item, owned), key, StringComparison.Ordinal)) return true;
        }
        return false;
    }

    private static string BagItemSelectionKey(MclslItemDefinition item, MclslOwnedItem owned)
    {
        if (item?.Category == "Artifact")
            return "artifact-group:" + (owned?.ItemId ?? item.Id);
        return "stack:" + (owned?.ItemId ?? string.Empty);
    }

    private void DrawNoActor(string headline, string explanation)
    {
        GUILayout.BeginVertical(_page == Page.Bag ? _bagPanel : _innerPanel, GUILayout.ExpandHeight(true));
        GUILayout.FlexibleSpace();
        GUILayout.Label("尚未建立人物交易印记", _section);
        GUILayout.Label(headline, _body);
        GUILayout.Label(explanation, _muted);
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }

    private bool TryGetActor() => _actor?.data != null && MclslActorAccessor.Alive(_actor);

    private void ApplyFeaturePause()
    {
        if (_pauseCaptured)
        {
            EnforceFeaturePause();
            return;
        }
        _savedTimeScale = Time.timeScale;
        _savedConfigPaused = Config.paused;
        _pauseCaptured = true;
        EnforceFeaturePause();
    }

    private static void EnforceFeaturePause()
    {
        Config.paused = true;
        Time.timeScale = 0f;
    }

    private void ReleaseFeaturePause()
    {
        if (!_pauseCaptured) return;
        Config.paused = _savedConfigPaused;
        Time.timeScale = _savedTimeScale < 0f ? 1f : _savedTimeScale;
        _pauseCaptured = false;
    }

    internal static void ClearRuntime()
    {
        if (_instance != null) _instance.CloseWindow();
    }

    private void CloseWindow()
    {
        _visible = false;
        enabled = false;
        ReleaseInputBlocker();
        ReleaseMarketSnapshot();
        ReleaseBagSnapshot();
        _actor = null;
        ReleaseFeaturePause();
    }

    private void OnDestroy()
    {
        ReleaseInputBlocker();
        ReleaseMarketSnapshot();
        ReleaseBagSnapshot();
        _actor = null;
        ReleaseFeaturePause();
    }

    private void CreateInputBlocker()
    {
        if (_inputBlocker != null) return;
        _inputBlocker = new GameObject("MclslFeatureInputBlocker");
        Canvas canvas = _inputBlocker.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 29989;
        _inputBlocker.AddComponent<GraphicRaycaster>();
        Image image = _inputBlocker.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        image.raycastTarget = true;
        RectTransform rect = _inputBlocker.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        DontDestroyOnLoad(_inputBlocker);
    }

    private void ReleaseInputBlocker()
    {
        if (_inputBlocker == null) return;
        Destroy(_inputBlocker);
        _inputBlocker = null;
    }

    private static string CategoryFor(string category) => category switch
    {
        "Pill" => "丹药", "Plant" => "灵植", "SpiritObject" => "灵物", "TalismanMaterial" => "符箓材料",
        "Material" => "炼器材料", "Talisman" => "符箓", "Artifact" => "法宝", "SpellScroll" => "法术卷轴", _ => "灵物"
    };

    private static void DrawIcon(MclslItemDefinition item, float size)
    {
        Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
        DrawIcon(rect, item);
    }

    private static void DrawIcon(Rect rect, MclslItemDefinition item)
    {
        Sprite icon = GetIcon(item.IconPath);
        if (icon == null) return;
        Texture2D texture = icon.texture;
        Rect area = icon.textureRect;
        GUI.DrawTextureWithTexCoords(rect, texture,
            new Rect(area.x / texture.width, area.y / texture.height, area.width / texture.width, area.height / texture.height));
    }

    private static void DrawEntryIcon(string path, float size)
    {
        Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
        Sprite icon = GetIcon(path);
        if (icon == null) return;
        Texture2D texture = icon.texture;
        Rect area = icon.textureRect;
        GUI.DrawTextureWithTexCoords(rect, texture,
            new Rect(area.x / texture.width, area.y / texture.height, area.width / texture.width, area.height / texture.height));
    }

    private static Sprite GetIcon(string path)
    {
        if (!IconCache.TryGetValue(path, out Sprite icon))
        {
            icon = SpriteTextureLoader.getSprite(path);
            if (icon != null) IconCache[path] = icon;
        }
        return icon;
    }

    private static string GradeName(int grade) => grade switch { 0 => "凡阶", 1 => "黄级", 2 => "玄级", 3 => "地级", 4 => "天级", _ => "材料" };

    private static string DisplayGradeName(MclslItemDefinition item)
    {
        if (item == null) return "材料";
        if (item.MaterialTier != MclslMaterialTier.None)
            return item.MaterialTier switch
            {
                MclslMaterialTier.Huang => "黄阶材料",
                MclslMaterialTier.Xuan => "玄阶材料",
                MclslMaterialTier.Di => "地阶材料",
                MclslMaterialTier.Tian => "天阶材料",
                _ => "材料"
            };
        return GradeName(item.Grade);
    }

    private void EnsureStyles()
    {
        if (_window != null) return;
        XianLuUIStyles.Ensure();
        _panelTexture = Solid(new Color(0.035f, 0.055f, 0.09f, 0.985f));
        _innerTexture = Solid(new Color(0.055f, 0.085f, 0.125f, 0.98f));
        _cardTexture = Solid(new Color(0.085f, 0.13f, 0.17f, 0.96f));
        _selectedTexture = Solid(new Color(0.13f, 0.23f, 0.22f, 0.99f));
        _jadeTexture = Solid(new Color(0.075f, 0.16f, 0.15f, 0.98f));
        _goldTexture = Solid(new Color(0.36f, 0.27f, 0.14f, 0.98f));
        _dimTexture = Solid(Color.white);
        _window = new GUIStyle(XianLuUIStyles.TransparentWindowStyle)
        {
            padding = new RectOffset(64, 64, 15, 40),
            contentOffset = Vector2.zero
        };
        _title = LabelStyle(24, FontStyle.Bold, new Color(0.91f, 0.84f, 0.68f), TextAnchor.MiddleLeft);
        _subtitle = LabelStyle(15, FontStyle.Normal, new Color(0.63f, 0.77f, 0.76f), TextAnchor.MiddleCenter);
        _body = LabelStyle(17, FontStyle.Normal, new Color(0.88f, 0.90f, 0.85f), TextAnchor.UpperLeft);
        _muted = LabelStyle(15, FontStyle.Normal, new Color(0.61f, 0.70f, 0.73f), TextAnchor.MiddleLeft);
        _section = LabelStyle(17, FontStyle.Bold, new Color(0.83f, 0.76f, 0.59f), TextAnchor.MiddleLeft);
        _guidePageNumber = LabelStyle(18, FontStyle.Bold, new Color(1f, 0.91f, 0.67f), TextAnchor.MiddleCenter);
        _guidePageShadow = LabelStyle(18, FontStyle.Bold, new Color(0.08f, 0.07f, 0.05f, 0.85f), TextAnchor.MiddleCenter);
        _small = LabelStyle(15, FontStyle.Normal, new Color(0.71f, 0.79f, 0.78f), TextAnchor.UpperLeft);
        _priceStyle = LabelStyle(16, FontStyle.Bold, new Color(0.91f, 0.79f, 0.51f), TextAnchor.MiddleCenter);
        _card = new GUIStyle(XianLuUIStyles.CardStyle);
        _selectedCard = BoxStyle(_selectedTexture, new RectOffset(9, 9, 7, 7));
        _innerPanel = new GUIStyle(XianLuUIStyles.InnerPanelStyle);
        _jadePanel = new GUIStyle(XianLuUIStyles.PanelStyle);
        _tab = new GUIStyle(XianLuUIStyles.TabNormalStyle);
        _selectedTab = new GUIStyle(XianLuUIStyles.TabSelectedStyle);
        _button = new GUIStyle(XianLuUIStyles.ButtonSecondaryStyle);
        _primaryButton = new GUIStyle(XianLuUIStyles.ButtonPrimaryStyle);
        _smallButtonStyle = ButtonStyle(_selectedTexture, new Color(0.79f, 0.89f, 0.82f), 16);

        // 乾坤袋沿用猫宝留档页的沉稳墨绿底色。所有容器面与格位均为实色，
        // 避免地图从储物界面背后透出，提升物品浏览时的文字与图标辨识度。
        _bagWindowTexture = Solid(MclslUiTheme.RankSurfaceWindow);
        _bagPanelTexture = Solid(new Color(0.16f, 0.23f, 0.20f, 1f));
        _bagShelfTexture = Solid(new Color(0.09f, 0.14f, 0.12f, 1f));
        _bagSlotTexture = Solid(new Color(0.15f, 0.22f, 0.18f, 1f));
        _bagSelectedSlotTexture = Solid(new Color(0.28f, 0.48f, 0.46f, 1f));
        _bagWindow = new GUIStyle(GUI.skin.window)
        {
            normal = { background = _bagWindowTexture, textColor = Color.white },
            hover = { background = _bagWindowTexture, textColor = Color.white },
            active = { background = _bagWindowTexture, textColor = Color.white },
            focused = { background = _bagWindowTexture, textColor = Color.white },
            onNormal = { background = _bagWindowTexture, textColor = Color.white },
            onHover = { background = _bagWindowTexture, textColor = Color.white },
            onActive = { background = _bagWindowTexture, textColor = Color.white },
            onFocused = { background = _bagWindowTexture, textColor = Color.white },
            border = new RectOffset(3, 3, 3, 3),
            padding = new RectOffset(14, 14, 14, 14),
            contentOffset = Vector2.zero
        };
        _bagPanel = BoxStyle(_bagPanelTexture, new RectOffset(12, 12, 10, 10));
        _bagShelf = BoxStyle(_bagShelfTexture, new RectOffset(10, 10, 9, 9));
        _bagSlot = BoxStyle(_bagSlotTexture, new RectOffset(8, 8, 7, 7));
        _bagSelectedSlot = BoxStyle(_bagSelectedSlotTexture, new RectOffset(8, 8, 7, 7));
        _bagCategory = ButtonStyle(_bagSlotTexture, MclslUiTheme.RankTextPrimary, 13);
        _bagSelectedCategory = ButtonStyle(_bagSelectedSlotTexture, MclslUiTheme.RankTextPrimary, 13);
        _bagButton = ButtonStyle(_bagShelfTexture, MclslUiTheme.RankTextPrimary, 13);
        _bagPrimaryButton = ButtonStyle(Solid(new Color(0.34f, 0.31f, 0.22f, 1f)), new Color(1f, 0.94f, 0.77f), 14);

        _marketWindow = new GUIStyle(XianLuUIStyles.TransparentWindowStyle);
        _marketWindow.padding = new RectOffset(56, 56, 15, 58);
        _marketPanel = new GUIStyle(XianLuUIStyles.PanelStyle);
        _marketInset = new GUIStyle(XianLuUIStyles.InnerPanelStyle);
        _marketRow = new GUIStyle(XianLuUIStyles.CardStyle);
        _marketTab = new GUIStyle(XianLuUIStyles.TabNormalStyle);
        _marketSelectedTab = new GUIStyle(XianLuUIStyles.TabSelectedStyle);
    }

    private static GUIStyle LabelStyle(int size, FontStyle weight, Color color, TextAnchor anchor)
    {
        GUIStyle style = new(GUI.skin.label)
        {
            fontSize = MclslUiTheme.ReadableFontSize(size), fontStyle = weight, wordWrap = true, alignment = anchor,
            stretchWidth = true, richText = false
        };
        style.normal.textColor = color;
        return style;
    }

    private static GUIStyle BoxStyle(Texture2D texture, RectOffset padding)
    {
        GUIStyle style = new(GUI.skin.box)
        {
            padding = padding, margin = new RectOffset(0, 0, 0, 0), wordWrap = true
        };
        style.normal.background = texture;
        style.normal.textColor = Color.white;
        return style;
    }

    private static GUIStyle ButtonStyle(Texture2D texture, Color color, int size)
    {
        GUIStyle style = new(GUI.skin.button)
        {
            fontSize = MclslUiTheme.ReadableFontSize(size), wordWrap = true, padding = new RectOffset(8, 8, 5, 5),
            margin = new RectOffset(2, 2, 1, 1)
        };
        style.normal.background = texture;
        style.hover.background = texture;
        style.active.background = texture;
        style.focused.background = texture;
        style.normal.textColor = color;
        style.hover.textColor = Color.white;
        style.active.textColor = Color.white;
        return style;
    }

    private static Texture2D Solid(Color color)
    {
        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private static Texture2D PixelPanel(Color fill, Color edge, Color highlight)
    {
        const int size = 12;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            int distance = Math.Min(Math.Min(x, size - 1 - x), Math.Min(y, size - 1 - y));
            pixels[y * size + x] = distance == 0 ? edge : distance == 1 ? highlight : fill;
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }
}
