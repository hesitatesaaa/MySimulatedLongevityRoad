using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed class MclslFeatureWindow : MonoBehaviour
{
    private enum Page { Guide, Bag, Market }
    private sealed class MarketOfferGroup
    {
        internal MclslItemDefinition Item;
        internal List<MclslMarketListing> Offers;
        internal int Total;
        internal int SellerCount;
        internal int MinimumPrice;
        internal int MaximumPrice;
    }

    private static readonly string[] Categories = { "丹药", "灵植", "灵物", "符箓材料", "炼器材料", "符箓", "法宝", "法术卷轴" };
    private static readonly string[] CategoryIds = { "Pill", "Plant", "SpiritObject", "TalismanMaterial", "Material", "Talisman", "Artifact", "SpellScroll" };
    private static readonly string[] BagCategories = { "法宝", "丹药", "符箓", "材料", "传承典籍", "法术" };
    private static readonly string[] GuideTitles = { "入道", "修炼", "职业", "储物", "交易", "还真" };
    private static readonly string[] JourneyNames = { "灵根显现", "引气入体", "修炼破境", "探索寻材", "天玄镜交易", "还真回溯" };
    private static readonly GUIContent EmptyWindowTitle = GUIContent.none;
    private static readonly string[] GuideBodies =
    {
        "天地灵机复苏，凡人可凭灵根踏上仙途。修士的灵根在五岁判定；高倍速略过五岁时，会在六岁补判。设置中的概率只影响尚未判定的人物。",
        "修士由炼气、筑基、金丹、元婴逐步问道。灵根、根骨、真元、功法和时代共同影响修行。留意人物境界与仙录纪事，可以追踪每次破境。",
        "有根骨的人物在十八岁时可能成为炼丹师、炼器师或制符师。三职业每年最多成功制作一次，优先同阶、缺料时最多降一阶；只有同阶成品增加熟练度。",
        "乾坤袋存放丹药、灵植、灵物、符箓材料、符箓和法宝。丹药与符箓可以使用，符箓用后消失；法宝各自保留耐久。",
        "天玄镜可用贡献或灵石进行全世界交易。修士满足自用需求后才会挂牌，买卖双方在成交时交换物品与所选货币。丹药与制符材料可由野外、遗迹和修行机缘发现；高阶材料来源更少。",
        "【还真操作】\n\n1）启用还真\n在设置中开启“启用还真回溯”。关闭时保留特质与档案，但暂停还真回溯。\n\n2）获得并绑定宿主\n新法纪元稳定后会按系统安排自然出现宿主；仙道纪元可在特质编辑器手动授予“还真”。同一世界只有一名宿主，授予新宿主会替换原持有者。\n\n3）获得空间灵蕴\n首次绑定还真获得 80 点空间灵蕴；当前宿主每经过 10 个游戏年自然增加 1 点。宿主晋升、击杀、洞天炼化、天地之变与势力机缘也可增加灵蕴。灵蕴保存在世界回档之外，回溯后保留当时余额。\n\n4）建立还真锚点\n先在游戏内保存当前世界，再从模组功能页打开“还真之门”。宿主须存活、不在战斗中且生命不低于最大生命的 85%。建立或替换锚点消耗 80 点灵蕴；最多保留三枚，满额时先选择要替换的旧锚点。锚点保存当刻的整个世界。若保存失败，不会扣除灵蕴。开启自动锚定后仍须满足灵蕴和间隔条件，默认间隔 100 年。\n\n5）死亡后回溯\n宿主死亡时，系统会寻找符合当前世界与安全间隔要求的锚点并自动回载；没有合格锚点时不会回载。也可在“还真之门”选择锚点，点击“手动还真·回到所选锚点”并再次确认；手动回溯不额外消耗灵蕴。回载后，晚于所选锚点的锚点会被清理。\n\n6）在前世轮盘选择保留内容\n宿主回溯前的境界、功法、造物、资质、心境、贡献、灵石及符合条件的特征会形成选项。逐项选择要继承的内容，每项占一个名额；名额数取决于回溯后保留的锚点数。乾坤袋随锚点存档恢复，不属于轮盘选项。"
    };
    private static readonly string[][] GuidePoints =
    {
        new[] { "查看人物是否显现灵根", "灵根判定后不会因设置改变" },
        new[] { "观察境界与修行纪事", "不同纪元采用不同修行法门" },
        new[] { "十八岁时判定职业", "仅制作同阶成品增加熟练度" },
        new[] { "人物界面打开乾坤袋", "留意符箓数量与法宝耐久" },
        new[] { "修士依自身需求自动交易", "无需选择人物或手动下单" },
        new[] { "先存档，再在还真之门建立锚点", "选锚点回溯并二次确认；死亡回溯须有合格锚点", "进入前世轮盘逐项选择遗产", "乾坤袋随世界回档，空间灵蕴在回档外保留" }
    };

    private static MclslFeatureWindow _instance;
    private static readonly Dictionary<string, Sprite> IconCache = new(StringComparer.Ordinal);

    private Page _page;
    private Actor _actor;
    private bool _visible;
    private int _category;
    private int _guidePage;
    private int _marketView;
    private readonly int[] _marketCategoryCounts = new int[CategoryIds.Length];
    private readonly int[] _marketListingCounts = new int[CategoryIds.Length];
    private readonly int[] _marketListedCounts = new int[CategoryIds.Length];
    private readonly int[] _marketPurchasedCounts = new int[CategoryIds.Length];
    private readonly Dictionary<int, List<MarketOfferGroup>> _marketOfferGroups = new();
    private int _marketCachedRevision = -1;
    private int _marketListedTotal;
    private int _marketPurchasedTotal;
    private int _marketTotalCount;
    private string _selectedBagItemKey = string.Empty;
    private string _bagActionMessage = string.Empty;
    private string _expandedMarketItemId = string.Empty;
    private Vector2 _artifactInstanceScroll;
    private Vector2 _marketScroll;
    private Vector2 _bagScroll;
    private Vector2 _guideScroll;
    private Rect _rect;
    private bool _pauseCaptured;
    private float _savedTimeScale;
    private bool _savedConfigPaused;

    private GUIStyle _window;
    private GUIStyle _title;
    private GUIStyle _subtitle;
    private GUIStyle _body;
    private GUIStyle _muted;
    private GUIStyle _section;
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
    private Texture2D _marketWindowTexture;
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
        _instance._page = page;
        _instance._actor = page == Page.Bag ? actor : null;
        if (page == Page.Bag && _instance._actor?.data == null
            && MclslMaobaoCommands.TryGetSelectedActor(out Actor selected)) _instance._actor = selected;
        if (page == Page.Market) MclslWorldRunRepository.EnsureCurrentRun(MclslRuntime.CurrentYear());
        float width = Mathf.Min(page == Page.Guide ? 1080f : 1120f, Screen.width - 28f);
        float height = Mathf.Min(740f, Screen.height - 28f);
        _instance._rect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        _instance._marketScroll = Vector2.zero;
        _instance._bagScroll = Vector2.zero;
        _instance._guideScroll = Vector2.zero;
        _instance._selectedBagItemKey = string.Empty;
        _instance._bagActionMessage = string.Empty;
        _instance._marketView = 0;
        _instance._visible = true;
        _instance.enabled = true;
        _instance.ApplyFeaturePause();
    }

    private void OnGUI()
    {
        if (!_visible) return;
        EnsureStyles();
        Color previous = GUI.color;
        Color previousBackground = GUI.backgroundColor;
        if (_page != Page.Bag)
        {
            GUI.color = new Color(0.015f, 0.025f, 0.04f, 0.60f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _dimTexture);
            GUI.color = previous;
        }
        _rect.width = Mathf.Min(_rect.width, Screen.width - 24f);
        _rect.height = Mathf.Min(_rect.height, Screen.height - 24f);
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
        GUI.color = previous;
        GUI.backgroundColor = previousBackground;
    }

    private void Update()
    {
        if (!_visible) return;
        EnforceFeaturePause();
        if (Input.GetKeyDown(KeyCode.Escape)) CloseWindow();
    }

    private void DrawWindow(int id)
    {
        if (_page == Page.Bag)
        {
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.DrawTexture(new Rect(0f, 0f, _rect.width, _rect.height), _bagWindowTexture);
        }
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
        GUI.DragWindow(new Rect(0f, 0f, _rect.width, 68f));
    }

    private void DrawHeader()
    {
        GUILayout.BeginHorizontal(_page == Page.Bag ? _bagPanel : _page == Page.Market ? _marketPanel : _card, GUILayout.Height(62f));
        DrawEntryIcon(_page == Page.Guide ? "ui/Icons/GuideEntrance"
            : _page == Page.Bag ? "ui/Icons/QiankunBagEntrance" : "ui/Icons/TianxuanMirrorEntrance", 42f);
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
        GUILayout.Label(_page == Page.Guide ? "模组介绍 · 入道图鉴｜QQ群：1049012706"
            : _page == Page.Bag ? "乾坤袋 · 纳灵藏珍" : "天玄镜 · 万界交易所", _title);
        GUILayout.Label(_page == Page.Guide ? "循灵根、修行、技艺与交易，阅览入道次第。"
            : _page == Page.Bag ? "分类收纳丹药、灵材、符箓与法宝。" : "修士按需自动挂单与成交，浏览全世界交易动态。", _subtitle);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("× 关闭", _page == Page.Bag ? _bagButton : _page == Page.Market ? _marketTab : _button,
                GUILayout.Width(88f), GUILayout.Height(34f))) CloseWindow();
        GUILayout.EndHorizontal();
    }

    private void DrawGuide()
    {
        GUILayout.BeginHorizontal();
        for (int i = 0; i < GuideTitles.Length; i++)
        {
            int page = i;
            GUIStyle style = _guidePage == page ? _selectedTab : _tab;
            if (GUILayout.Button(GuideTitles[i], style, GUILayout.Height(40f)))
            {
                _guidePage = page;
                _guideScroll = Vector2.zero;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10f);

        GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
        GUILayout.BeginVertical(_innerPanel, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.BeginHorizontal();
        GUILayout.Label("第 " + (_guidePage + 1) + " 卷", _muted, GUILayout.Width(86f));
        GUILayout.Label(GuideTitles[_guidePage] + " · 修行次第", _section);
        GUILayout.FlexibleSpace();
        GUILayout.Label("道途图录", _muted);
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        DrawJourney();
        GUILayout.Space(10f);
        _guideScroll = GUILayout.BeginScrollView(_guideScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar,
            GUILayout.ExpandHeight(true));
        GUILayout.Label(GuideBodies[_guidePage], _body, GUILayout.ExpandWidth(true));
        GUILayout.Space(12f);
        GUILayout.Label("本卷要点", _section);
        foreach (string point in GuidePoints[_guidePage])
        {
            GUILayout.BeginHorizontal(_card, GUILayout.MinHeight(40f));
            GUILayout.Label("◇", _section, GUILayout.Width(28f));
            GUILayout.Label(point, _body);
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.Space(10f);
        GUILayout.BeginVertical(_jadePanel, GUILayout.Width(214f), GUILayout.ExpandHeight(true));
        GUILayout.Label("初入仙途", _section);
        GUILayout.Label("三步览尽入道之路", _muted);
        GUILayout.Space(10f);
        DrawGuideStep("一", "察灵根", "点选人物，查看灵根显现与根骨。", 0);
        DrawGuideStep("二", "观修行", "在玄黄仙录追踪境界与纪事。", 1);
        DrawGuideStep("三", "开宝匣", "人物界面收纳，功能页交易。", 4);
        if (_guidePage == 5)
        {
            GUILayout.Space(6f);
            if (GUILayout.Button("前往还真之门 →", _primaryButton, GUILayout.Height(38f)))
            {
                _visible = false;
                ReleaseFeaturePause();
                MclslCodexWindow.ShowHuanzhenSpace();
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.Label("第 " + (_guidePage + 1) + " / " + GuideTitles.Length + " 卷", _muted);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("‹ 上一卷", _button, GUILayout.Width(120f), GUILayout.Height(34f)))
            _guidePage = (_guidePage + GuideTitles.Length - 1) % GuideTitles.Length;
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("下一卷 ›", _primaryButton, GUILayout.Width(120f), GUILayout.Height(34f)))
        {
            _guidePage = (_guidePage + 1) % GuideTitles.Length;
            _guideScroll = Vector2.zero;
        }
        GUILayout.EndHorizontal();
    }

    private void DrawJourney()
    {
        GUILayout.BeginHorizontal(_card, GUILayout.Height(76f));
        for (int i = 0; i < JourneyNames.Length; i++)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label(i == _guidePage ? "◉" : "○", i == _guidePage ? _section : _muted, GUILayout.Height(23f));
            GUILayout.Label(JourneyNames[i], _small);
            GUILayout.EndVertical();
            if (i < JourneyNames.Length - 1) GUILayout.Label("›", _muted, GUILayout.Width(18f));
        }
        GUILayout.EndHorizontal();
    }

    private void DrawGuideStep(string numeral, string title, string description, int page)
    {
        GUILayout.BeginHorizontal(_card, GUILayout.MinHeight(58f));
        GUILayout.Label(numeral, _section, GUILayout.Width(28f));
        GUILayout.BeginVertical();
        GUILayout.Label(title, _body);
        GUILayout.Label(description, _small);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Space(6f);
        if (GUILayout.Button("读此卷 →", _smallButtonStyle, GUILayout.Height(25f)))
        {
            _guidePage = page;
            _guideScroll = Vector2.zero;
        }
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
        DrawBagSummary("收纳件数", CountBagItems(bag, null));
        GUILayout.Space(18f);
        DrawBagSummary("贡献度", MclslActorAccessor.GetInt(_actor, MclslActorDataKeys.Contribution));
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

    private void DrawBagSummary(string label, int value)
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
            List<MclslSpellDefinition> known = MclslSpellSystem.Known(_actor);
            if (known.Count == 0) GUILayout.Label("尚未学会法术", _section);
            foreach (MclslSpellDefinition spell in known)
            {
                GUILayout.BeginHorizontal(_card, GUILayout.Height(48f));
                Sprite icon = SpriteTextureLoader.getSprite(spell.IconPath);
                if (icon != null) GUILayout.Label(icon.texture, GUILayout.Width(38f), GUILayout.Height(38f));
                GUILayout.Label(spell.Name + "　耗灵力 " + spell.ManaCost + "　" + spell.Description, _small);
                GUILayout.EndHorizontal();
            }
            foreach (MclslOwnedItem scroll in MclslBagSystem.Peek(_actor).Items)
            {
                MclslItemDefinition definition = MclslItemCatalog.Get(scroll?.ItemId);
                if (definition?.Category != "SpellScroll") continue;
                if (GUILayout.Button("研习《" + definition.Name + "》 ×" + scroll.Count, _bagButton, GUILayout.Height(34f)))
                    MclslSpellSystem.TryStudyScroll(_actor, definition.Id);
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
            foreach (MclslMentorshipBook book in books)
            {
                string bookKey = "book:" + book.BookId;
                if (GUILayout.Button((bookKey == _selectedBagItemKey ? "◆ " : "◇ ") + book.TechniqueName + "\n师父：" + book.TeacherName + "　参悟 " + book.Progress + "%", _bagCategory, GUILayout.Height(58f)))
                    _selectedBagItemKey = bookKey;
            }
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
            for (int start = 0; start < items.Count; start += columns)
            {
                GUILayout.BeginHorizontal();
                int end = Math.Min(start + columns, items.Count);
                for (int i = start; i < end; i++) DrawBagSlot(items[i]);
                if (end - start < columns) GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(6f);
            }
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
            List<MclslOwnedItem> instances = MclslBagSystem.Peek(_actor).Items
                .Where(x => x != null && string.Equals(x.ItemId, item.Id, StringComparison.Ordinal))
                .ToList();
            GUILayout.Label("槽位　" + MclslArtifactSystem.SlotDisplayName(item.EquipmentSlot) + "　·　唯一持有", _muted);
            string equippedArtifactId = MclslArtifactSystem.EquippedArtifactId(_actor, item.EquipmentSlot);
            if (equippedArtifactId == item.Id)
                GUILayout.Label("当前装备于" + MclslArtifactSystem.SlotDisplayName(item.EquipmentSlot) + "栏", _section);
            _artifactInstanceScroll = GUILayout.BeginScrollView(_artifactInstanceScroll, false, true,
                GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(Mathf.Min(165f, Math.Max(76f, instances.Count * 42f))));
            for (int i = 0; i < instances.Count; i++)
            {
                MclslOwnedItem instance = instances[i];
                GUILayout.BeginHorizontal(_card, GUILayout.MinHeight(36f));
                GUILayout.Label("耐久 " + Mathf.Clamp(instance.Durability, 0, 100) + "%　" + (equippedArtifactId == item.Id ? "已装备" : "收纳中"), _small);
                string action = equippedArtifactId == item.Id ? "已装备"
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
        IReadOnlyList<MclslMarketListing> listings = MclslTianxuanMarket.Snapshot();
        IReadOnlyList<MclslMarketActivity> listed = MclslTianxuanMarket.ListingActivitySnapshot();
        IReadOnlyList<MclslMarketActivity> purchased = MclslTianxuanMarket.PurchaseActivitySnapshot();
        RefreshMarketCategoryCounts(listings, listed, purchased);
        IReadOnlyList<MclslMarketActivity> activity = _marketView == 1
            ? listed : _marketView == 2 ? purchased : Array.Empty<MclslMarketActivity>();
        bool compact = _rect.width < 900f;
        GUILayout.BeginHorizontal(_marketPanel, GUILayout.Height(compact ? 76f : 66f));
        DrawMarketTicker("在售挂单", _marketTotalCount.ToString(), "件");
        DrawMarketTicker("近期上架", listed.Count + "笔 · " + _marketListedTotal + "件", "");
        DrawMarketTicker("近期成交", purchased.Count + "笔 · " + _marketPurchasedTotal + "件", "");
        GUILayout.FlexibleSpace();
        if (!compact)
        {
            GUILayout.BeginVertical(GUILayout.Width(174f));
            GUILayout.Label("万界交易时刻", _muted);
            GUILayout.Label("第 " + MclslRuntime.CurrentYear() + " 年 · 自动撮合", _body);
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        string[] views = { "实时挂单", "上架动态", "购入成交" };
        for (int i = 0; i < views.Length; i++)
        {
            int view = i;
            if (GUILayout.Button(views[i], _marketView == view ? _marketSelectedTab : _marketTab, GUILayout.Height(38f)))
            {
                _marketView = view;
                _marketScroll = Vector2.zero;
            }
        }
        GUILayout.EndHorizontal();
        // A tab click happens after the first count refresh in this GUI pass.
        // Copy the newly selected cached column before drawing the category rail.
        RefreshMarketCategoryCounts(listings, listed, purchased);
        GUILayout.Space(8f);
        float contentWidth = Mathf.Max(300f, _rect.width - 30f);
        GUILayout.BeginHorizontal(GUILayout.Width(contentWidth), GUILayout.ExpandHeight(true));
        float categoryWidth = compact ? 124f : Mathf.Clamp(_rect.width * 0.16f, 124f, 176f);
        float infoWidth = Mathf.Clamp(_rect.width * 0.20f, 164f, 230f);
        float boardWidth = Mathf.Max(160f, contentWidth - categoryWidth - 7f - (compact ? 0f : infoWidth + 7f));
        DrawMarketCategoryRail(categoryWidth);
        GUILayout.Space(7f);
        if (compact) GUILayout.BeginVertical(GUILayout.Width(boardWidth), GUILayout.ExpandHeight(true));
        if (_marketView == 0) DrawMarketListings(boardWidth);
        else DrawMarketActivity(activity, boardWidth);
        if (compact)
        {
            GUILayout.Space(5f);
            DrawMarketExchangeInfo(0f, true);
            GUILayout.EndVertical();
        }
        else
        {
            GUILayout.Space(7f);
            DrawMarketExchangeInfo(infoWidth, false);
        }
        GUILayout.EndHorizontal();
    }

    private void DrawMarketTicker(string label, string value, string unit)
    {
        GUILayout.BeginVertical(_marketInset, GUILayout.MinWidth(100f), GUILayout.ExpandHeight(true));
        GUILayout.Label(label, _muted);
        GUILayout.BeginHorizontal();
        GUILayout.Label(value, unit.Length == 0 ? _body : _section);
        if (unit.Length > 0) GUILayout.Label(unit, _small);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawMarketCategoryRail(float width)
    {
        GUILayout.BeginVertical(_marketPanel, GUILayout.Width(width), GUILayout.ExpandHeight(true));
        GUILayout.Label("交易板块", _section);
        GUILayout.Space(8f);
        for (int i = 0; i < Categories.Length; i++)
        {
            int index = i;
            GUIStyle style = _category == index ? _marketSelectedTab : _marketTab;
            int count = _marketCategoryCounts[index];
            if (GUILayout.Button(Categories[i] + "  " + count + "件", style, GUILayout.Height(37f)))
            {
                _category = index;
                _marketScroll = Vector2.zero;
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.Label("全球修士自动撮合\n满足自用后挂售\n成交同步交割物品与贡献或灵石", _small);
        GUILayout.EndVertical();
    }

    private void DrawMarketListings(float width)
    {
        List<MarketOfferGroup> groups = _marketOfferGroups.TryGetValue(_category, out List<MarketOfferGroup> cached)
            ? cached : new List<MarketOfferGroup>();
        GUILayout.BeginVertical(_marketPanel, GUILayout.Width(width), GUILayout.ExpandHeight(true));
        GUILayout.BeginHorizontal();
        GUILayout.Label(Categories[_category] + " · 实时盘口", _section);
        GUILayout.FlexibleSpace();
        GUILayout.Label("在售 " + _marketCategoryCounts[_category] + " 件 · " + groups.Count + " 种", _muted);
        GUILayout.EndHorizontal();
        GUILayout.Space(6f);
        _marketScroll = GUILayout.BeginScrollView(_marketScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar,
            GUILayout.ExpandHeight(true));
        foreach (MarketOfferGroup group in groups)
        {
            MclslItemDefinition item = group.Item;
            List<MclslMarketListing> offers = group.Offers;
            GUILayout.BeginHorizontal(_marketRow, GUILayout.MinHeight(72f));
            DrawIcon(item, 50f);
            GUILayout.BeginVertical(GUILayout.MinWidth(125f), GUILayout.ExpandWidth(true));
            GUILayout.Label(item.Name + "  ·  " + DisplayGradeName(item), _body);
            GUILayout.Label(group.Total + " 件挂单　" + group.SellerCount + " 位卖家", _small);
            GUILayout.EndVertical();
            GUILayout.BeginVertical(_marketInset, GUILayout.Width(118f), GUILayout.ExpandHeight(true));
            GUILayout.Label(group.MinimumPrice == group.MaximumPrice ? group.MinimumPrice + " 贡献" : group.MinimumPrice + "—" + group.MaximumPrice + " 贡献", _priceStyle);
            GUILayout.Label("另可用灵石", _muted);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (GUILayout.Button(_expandedMarketItemId == item.Id ? "收起卖家挂单" : "展开 " + offers.Count + " 条挂单", _marketTab, GUILayout.Height(30f)))
                _expandedMarketItemId = _expandedMarketItemId == item.Id ? string.Empty : item.Id;
            if (_expandedMarketItemId == item.Id)
            {
                foreach (MclslMarketListing listing in offers)
                {
                    GUILayout.BeginHorizontal(_marketInset, GUILayout.MinHeight(38f));
                    int contributionPrice = listing.ContributionUnitPrice > 0 ? listing.ContributionUnitPrice : listing.Price;
                    int stonePrice = listing.SpiritStoneUnitPrice > 0 ? listing.SpiritStoneUnitPrice
                        : MclslTianxuanMarket.SpiritStonePrice(item, contributionPrice);
                    GUILayout.Label("卖家 " + SellerName(listing.SellerId) + "　×" + Math.Max(1, listing.Item?.Count ?? 1)
                        + "　单价 贡献" + contributionPrice + "／灵石" + stonePrice + "　挂单年份 " + listing.Year, _small);
                    if (item.Category == "Artifact")
                        GUILayout.Label("耐久 " + Mathf.Clamp(listing.Item?.Durability ?? 0, 0, 100) + "%", _muted, GUILayout.Width(74f));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.Space(5f);
        }
        if (groups.Count == 0)
        {
            GUILayout.Space(18f);
            GUILayout.Label("此板块暂无挂单", _section);
            GUILayout.Label("修士有可交易余物时会自动挂牌，成交记录将同步归档。", _muted);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawMarketActivity(IReadOnlyList<MclslMarketActivity> activities, float width)
    {
        GUILayout.BeginVertical(_marketPanel, GUILayout.Width(width), GUILayout.ExpandHeight(true));
        GUILayout.Label(_marketView == 1 ? "修士上架动态" : "修士购入成交", _section);
        GUILayout.Label(_marketView == 1 ? "展示自动挂牌的数量、物品、卖方与年份。" : "展示买卖双方、成交数量与实际支付币种。", _muted);
        GUILayout.Space(6f);
        _marketScroll = GUILayout.BeginScrollView(_marketScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar,
            GUILayout.ExpandHeight(true));
        int count = 0;
        foreach (MclslMarketActivity record in activities)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(record.ItemId);
            if (item == null || item.Category != CategoryIds[_category]) continue;
            count++;
            GUILayout.BeginHorizontal(_marketRow, GUILayout.MinHeight(68f));
            DrawIcon(item, 46f);
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label(item.Name + "  ·  " + DisplayGradeName(item) + "  ×" + Math.Max(1, record.Count), _body);
            string names = _marketView == 1
                ? "上架修士　" + record.ActorName
                : "购入　" + record.ActorName + "　／　售出　" + record.OtherActorName;
            GUILayout.Label(names, _small);
            GUILayout.EndVertical();
            GUILayout.BeginVertical(_marketInset, GUILayout.Width(106f));
            string currencyName = record.Currency == MclslTianxuanMarket.SpiritStoneCurrency ? "灵石" : "贡献";
            int unitPrice = record.UnitPrice > 0 ? record.UnitPrice : record.Price;
            int totalPrice = record.TotalPrice > 0 ? record.TotalPrice : unitPrice * Math.Max(1, record.Count);
            GUILayout.Label(currencyName + totalPrice, _priceStyle);
            GUILayout.Label("单价 " + unitPrice, _muted);
            GUILayout.Label("第 " + record.Year + " 年", _muted);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
        }
        if (count == 0)
        {
            GUILayout.Space(18f);
            GUILayout.Label("尚无此类记录", _section);
            GUILayout.Label("人物满足自身需求后会自动挂牌或按需购入。", _muted);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawMarketExchangeInfo(float width, bool compact)
    {
        if (compact)
        {
            GUILayout.BeginVertical(_marketInset, GUILayout.ExpandWidth(true), GUILayout.Height(76f));
            GUILayout.Label("万界撮合：修士按职业和自身需要自动买卖；贡献与灵石分别计价，成交后即交割。", _small);
            GUILayout.EndVertical();
            return;
        }
        GUILayout.BeginVertical(_marketPanel, GUILayout.Width(width), GUILayout.ExpandHeight(true));
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
    }

    private static string SellerName(long sellerId)
    {
        if (MclslActorRegistry.ResolveKnownOrWorld(sellerId, out Actor seller) && MclslActorAccessor.Alive(seller))
            return MclslActorAccessor.DisplayName(seller);
        return "云游修士";
    }

    private void RefreshMarketCategoryCounts(IReadOnlyList<MclslMarketListing> listings,
        IReadOnlyList<MclslMarketActivity> listed, IReadOnlyList<MclslMarketActivity> purchased)
    {
        int revision = MclslTianxuanMarket.Revision;
        if (_marketCachedRevision != revision)
        {
            _marketCachedRevision = revision;
            Array.Clear(_marketListingCounts, 0, _marketListingCounts.Length);
            Array.Clear(_marketListedCounts, 0, _marketListedCounts.Length);
            Array.Clear(_marketPurchasedCounts, 0, _marketPurchasedCounts.Length);
            _marketOfferGroups.Clear();
            _marketTotalCount = _marketListedTotal = _marketPurchasedTotal = 0;
            foreach (MclslMarketListing listing in listings)
            {
                MclslItemDefinition item = MclslItemCatalog.Get(listing?.Item?.ItemId);
                int index = item == null ? -1 : Array.IndexOf(CategoryIds, item.Category);
                if (index < 0) continue;
                int count = Math.Max(1, listing.Item.Count);
                _marketListingCounts[index] += count;
                _marketTotalCount += count;
            }
            CountMarketActivity(listed, _marketListedCounts, ref _marketListedTotal);
            CountMarketActivity(purchased, _marketPurchasedCounts, ref _marketPurchasedTotal);
            for (int category = 0; category < CategoryIds.Length; category++)
            {
                List<MarketOfferGroup> groups = listings
                    .Where(x => MclslItemCatalog.Get(x?.Item?.ItemId)?.Category == CategoryIds[category])
                    .GroupBy(x => x.Item.ItemId)
                    .Select(group =>
                    {
                        MclslMarketListing[] offers = group.OrderBy(x => x.ContributionUnitPrice > 0 ? x.ContributionUnitPrice : x.Price)
                            .ThenBy(x => x.Year).ToArray();
                        return new MarketOfferGroup
                        {
                            Item = MclslItemCatalog.Get(group.Key), Offers = offers.ToList(),
                            Total = offers.Sum(x => Math.Max(1, x.Item.Count)),
                            SellerCount = offers.Select(x => x.SellerId).Distinct().Count(),
                            MinimumPrice = offers.Min(x => x.ContributionUnitPrice > 0 ? x.ContributionUnitPrice : x.Price),
                            MaximumPrice = offers.Max(x => x.ContributionUnitPrice > 0 ? x.ContributionUnitPrice : x.Price)
                        };
                    })
                    .OrderBy(x => x.Item?.Name, StringComparer.Ordinal).ToList();
                _marketOfferGroups[category] = groups;
            }
        }
        int[] source = _marketView switch { 1 => _marketListedCounts, 2 => _marketPurchasedCounts, _ => _marketListingCounts };
        Array.Copy(source, _marketCategoryCounts, source.Length);
    }

    private static void CountMarketActivity(IReadOnlyList<MclslMarketActivity> activities, int[] counts, ref int total)
    {
        foreach (MclslMarketActivity record in activities)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(record?.ItemId);
            int index = item == null ? -1 : Array.IndexOf(CategoryIds, item.Category);
            if (index < 0) continue;
            int quantity = Math.Max(1, record.Count);
            counts[index] += quantity;
            total += quantity;
        }
    }

    private static List<MclslOwnedItem> GetBagItems(MclslBagState bag, int category)
    {
        List<MclslOwnedItem> items = new();
        if (bag?.Items == null || category < 0 || category >= BagCategories.Length || category == 4) return items;
        Dictionary<string, MclslOwnedItem> artifactGroups = new(StringComparer.Ordinal);
        foreach (MclslOwnedItem owned in bag.Items)
        {
            if (owned == null) continue;
            MclslItemDefinition item = MclslItemCatalog.Get(owned.ItemId);
            if (item == null || !MatchesBagCategory(item, category)) continue;
            if (item.Category != "Artifact")
            {
                items.Add(owned);
                continue;
            }
            if (!artifactGroups.TryGetValue(owned.ItemId, out MclslOwnedItem group))
            {
                group = new MclslOwnedItem { ItemId = owned.ItemId, Count = 0, InstanceId = owned.InstanceId };
                artifactGroups[owned.ItemId] = group;
                items.Add(group);
            }
            group.Count++;
        }
        return items;
    }

    private static int CountBagItems(MclslBagState bag, string category)
    {
        if (bag?.Items == null) return 0;
        int count = 0;
        foreach (MclslOwnedItem owned in bag.Items)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
            if (item == null || (category != null && item.Category != category)) continue;
            count += item.Category == "Artifact" ? 1 : Math.Max(1, owned.Count);
        }
        return count;
    }

    private int CountBagCategory(MclslBagState bag, int category)
    {
        if (category == 4) return bag?.Books?.Count ?? 0;
        if (category == 5) return MclslSpellSystem.Known(_actor).Count;
        if (bag?.Items == null) return 0;
        int count = 0;
        foreach (MclslOwnedItem owned in bag.Items)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(owned?.ItemId);
            if (item == null || !MatchesBagCategory(item, category)) continue;
            count += item.Category == "Artifact" ? 1 : Math.Max(1, owned.Count);
        }
        return count;
    }

    private static bool MatchesBagCategory(MclslItemDefinition item, int category) => category switch
    {
        0 => item.Category == "Artifact",
        1 => item.Category == "Pill",
        2 => item.Category == "Talisman",
        3 => item.Category is "Plant" or "SpiritObject" or "TalismanMaterial" or "Material",
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

    private void CloseWindow()
    {
        _visible = false;
        ReleaseFeaturePause();
    }

    private void OnDestroy() => ReleaseFeaturePause();

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
        _panelTexture = Solid(new Color(0.035f, 0.055f, 0.09f, 0.985f));
        _innerTexture = Solid(new Color(0.055f, 0.085f, 0.125f, 0.98f));
        _cardTexture = Solid(new Color(0.085f, 0.13f, 0.17f, 0.96f));
        _selectedTexture = Solid(new Color(0.13f, 0.23f, 0.22f, 0.99f));
        _jadeTexture = Solid(new Color(0.075f, 0.16f, 0.15f, 0.98f));
        _goldTexture = Solid(new Color(0.36f, 0.27f, 0.14f, 0.98f));
        _dimTexture = Solid(Color.white);
        _window = new GUIStyle(GUI.skin.window)
        {
            normal = { background = _panelTexture, textColor = Color.white },
            onNormal = { background = _panelTexture, textColor = Color.white },
            border = new RectOffset(3, 3, 3, 3),
            padding = new RectOffset(14, 14, 14, 14),
            contentOffset = Vector2.zero
        };
        _title = LabelStyle(21, FontStyle.Bold, new Color(0.91f, 0.84f, 0.68f), TextAnchor.MiddleLeft);
        _subtitle = LabelStyle(12, FontStyle.Normal, new Color(0.63f, 0.77f, 0.76f), TextAnchor.MiddleLeft);
        _body = LabelStyle(15, FontStyle.Normal, new Color(0.88f, 0.90f, 0.85f), TextAnchor.UpperLeft);
        _muted = LabelStyle(12, FontStyle.Normal, new Color(0.61f, 0.70f, 0.73f), TextAnchor.MiddleLeft);
        _section = LabelStyle(17, FontStyle.Bold, new Color(0.83f, 0.76f, 0.59f), TextAnchor.MiddleLeft);
        _small = LabelStyle(12, FontStyle.Normal, new Color(0.71f, 0.79f, 0.78f), TextAnchor.UpperLeft);
        _priceStyle = LabelStyle(16, FontStyle.Bold, new Color(0.91f, 0.79f, 0.51f), TextAnchor.MiddleCenter);
        _card = BoxStyle(_cardTexture, new RectOffset(9, 9, 7, 7));
        _selectedCard = BoxStyle(_selectedTexture, new RectOffset(9, 9, 7, 7));
        _innerPanel = BoxStyle(_innerTexture, new RectOffset(12, 12, 10, 10));
        _jadePanel = BoxStyle(_jadeTexture, new RectOffset(12, 12, 10, 10));
        _tab = ButtonStyle(_cardTexture, new Color(0.77f, 0.83f, 0.81f), 13);
        _selectedTab = ButtonStyle(_jadeTexture, new Color(0.91f, 0.83f, 0.63f), 13);
        _button = ButtonStyle(_innerTexture, new Color(0.85f, 0.89f, 0.87f), 13);
        _primaryButton = ButtonStyle(_goldTexture, new Color(1f, 0.94f, 0.77f), 16);
        _smallButtonStyle = ButtonStyle(_selectedTexture, new Color(0.79f, 0.89f, 0.82f), 11);

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

        // Market frames are small point-filtered pixel panels with fixed corner
        // pixels. Only the centre stretches; the bag and guide keep their styles.
        Color stoneEdge = new(0.10f, 0.13f, 0.12f, 1f);
        Color stoneLight = new(0.43f, 0.47f, 0.39f, 1f);
        _marketWindowTexture = PixelPanel(MclslUiTheme.RankSurfaceWindow, stoneEdge, stoneLight);
        Texture2D marketPanelTexture = PixelPanel(MclslUiTheme.RankSurfacePanel, stoneEdge, stoneLight);
        Texture2D marketInsetTexture = PixelPanel(MclslUiTheme.RankSurfaceDeep, stoneEdge, new Color(0.32f, 0.38f, 0.31f, 1f));
        Texture2D marketRowTexture = PixelPanel(new Color(0.21f, 0.28f, 0.23f, 1f), stoneEdge, new Color(0.35f, 0.43f, 0.34f, 1f));
        Texture2D marketSelectedTexture = PixelPanel(new Color(0.29f, 0.35f, 0.25f, 1f), stoneEdge, MclslUiTheme.FrameGold);
        _marketWindow = new GUIStyle(GUI.skin.window)
        {
            normal = { background = _marketWindowTexture, textColor = Color.white },
            onNormal = { background = _marketWindowTexture, textColor = Color.white },
            border = new RectOffset(3, 3, 3, 3),
            padding = new RectOffset(12, 12, 12, 12),
            contentOffset = Vector2.zero
        };
        _marketPanel = BoxStyle(marketPanelTexture, new RectOffset(11, 11, 9, 9));
        _marketInset = BoxStyle(marketInsetTexture, new RectOffset(8, 8, 7, 7));
        _marketRow = BoxStyle(marketRowTexture, new RectOffset(9, 9, 7, 7));
        foreach (GUIStyle style in new[] { _marketPanel, _marketInset, _marketRow })
            style.border = new RectOffset(3, 3, 3, 3);
        _marketTab = ButtonStyle(marketInsetTexture, MclslUiTheme.RankTextPrimary, 13);
        _marketSelectedTab = ButtonStyle(marketSelectedTexture, MclslUiTheme.AccentGold, 13);
        _marketTab.border = new RectOffset(3, 3, 3, 3);
        _marketSelectedTab.border = new RectOffset(3, 3, 3, 3);
    }

    private static GUIStyle LabelStyle(int size, FontStyle weight, Color color, TextAnchor anchor)
    {
        GUIStyle style = new(GUI.skin.label)
        {
            fontSize = size, fontStyle = weight, wordWrap = true, alignment = anchor,
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
            fontSize = size, wordWrap = true, padding = new RectOffset(8, 8, 5, 5),
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
