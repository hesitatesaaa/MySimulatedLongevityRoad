using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed class MclslFeatureWindow : MonoBehaviour
{
    private enum Page { Guide, Bag, Market }

    private static readonly string[] Categories = { "丹药", "灵植", "灵物", "符箓材料", "符箓", "法宝" };
    private static readonly string[] CategoryIds = { "Pill", "Plant", "SpiritObject", "TalismanMaterial", "Talisman", "Artifact" };
    private static readonly string[] GuideTitles = { "入道", "修炼", "职业", "储物", "交易", "还真" };
    private static readonly string[] JourneyNames = { "灵根显现", "引气入体", "修炼破境", "探索寻材", "天玄镜交易", "还真回溯" };
    private static readonly GUIContent EmptyWindowTitle = GUIContent.none;
    private static readonly string[] GuideBodies =
    {
        "天地灵机复苏，凡人可凭灵根踏上仙途。修士的灵根在五岁判定；高倍速略过五岁时，会在六岁补判。设置中的概率只影响尚未判定的人物。",
        "修士由炼气、筑基、金丹、元婴逐步问道。灵根、根骨、真元、功法和时代共同影响修行。留意人物境界与仙录纪事，可以追踪每次破境。",
        "有根骨的人物在十八岁时可能成为炼丹师、炼器师或制符师。学徒先练习，熟练度与境界同时达标后晋级。职业只能制作当前品阶的成品。",
        "乾坤袋存放丹药、灵植、灵物、符箓材料、符箓和法宝。丹药与符箓可以使用，符箓用后消失；法宝各自保留耐久。",
        "天玄镜使用贡献度进行全世界交易。修士满足自用需求后才会挂牌，买卖双方在成交时交换物品与贡献度。丹药与制符材料可由野外、遗迹和修行机缘发现；高阶材料来源更少。",
        "还真会把世界回到锚点时刻，并将宿主回溯前的部分经历送入前世轮盘供玩家选择。首次使用请按本卷步骤操作：先确认宿主、建立锚点，再选择回溯方式；空间灵蕴不随世界回档，乾坤袋则随锚点存档恢复。\n\n【启用与宿主】还真功能须在设置中启用。新法纪元稳定后会按系统安排出现宿主；仙道纪元可在特质编辑器授予“还真”特质。宿主唯一，授予新宿主会替换原持有者。\n\n【建立锚点】先在游戏内保存当前世界，再从模组功能页打开“还真之门”。宿主必须存活、不在战斗中，生命至少达到最大生命的 85%；手动建立锚点消耗 80 点空间灵蕴。最多保留三枚锚点，达到上限时先选一枚作为替换对象。锚点保存建立当刻的整个世界存档，不只是宿主档案。若保存失败，不会扣除灵蕴或建立锚点。\n\n【手动回溯】在“还真之门”选择一枚锚点，点击“手动还真·回到所选锚点”，再次点击确认。手动回溯不额外消耗空间灵蕴。世界载入锚点存档后，晚于该锚点的锚点会被清理。\n\n【死亡回溯】若宿主死亡，系统会寻找符合当前世界与安全间隔要求的可用锚点并自动回载；没有合格锚点时不会回载。刚回溯后的安全间隔由设置决定，默认 40 年。\n\n【前世轮盘】回溯前宿主的修为与境界、功法、突破造物、天地道果、资质、心境、贡献、灵石及符合条件的特征会形成前世选项。进入轮盘后逐项选择要继承的内容；每项占一个名额，名额数取决于回溯后仍保留的锚点数。乾坤袋物品不是前世轮盘选项，会随整个世界恢复到锚点当时的状态。\n\n【空间灵蕴】空间灵蕴保存在世界回档之外，回溯后保留回溯前余额。主要来源包括宿主晋升、击杀、洞天炼化、天地之变与势力机缘。开启自动锚定后，系统仍须满足至少 80 点灵蕴及设置的锚定间隔；默认间隔 100 年，锚点满额时自动替换最旧的一枚。"
    };
    private static readonly string[][] GuidePoints =
    {
        new[] { "查看人物是否显现灵根", "灵根判定后不会因设置改变" },
        new[] { "观察境界与修行纪事", "不同纪元采用不同修行法门" },
        new[] { "十八岁时判定职业", "练习与制作都会积累熟练度" },
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
    private int _marketTotalCount;
    private string _selectedBagItemKey = string.Empty;
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
        _rect = GUI.Window(781208, _rect, DrawWindow, EmptyWindowTitle, _page == Page.Bag ? _bagWindow : _window);
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
        GUILayout.BeginHorizontal(_page == Page.Bag ? _bagPanel : _card, GUILayout.Height(62f));
        DrawEntryIcon(_page == Page.Guide ? "ui/Icons/GuideEntrance"
            : _page == Page.Bag ? "ui/Icons/QiankunBagEntrance" : "ui/Icons/TianxuanMirrorEntrance", 42f);
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
        GUILayout.Label(_page == Page.Guide ? "入道图鉴 · 观星问道"
            : _page == Page.Bag ? "乾坤袋 · 纳灵藏珍" : "天玄镜 · 万界交易所", _title);
        GUILayout.Label(_page == Page.Guide ? "循灵根、修行、技艺与交易，阅览入道次第。"
            : _page == Page.Bag ? "分类收纳丹药、灵材、符箓与法宝。" : "修士按需自动挂单与成交，浏览全世界交易动态。", _subtitle);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("× 关闭", _page == Page.Bag ? _bagButton : _button,
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
        List<MclslOwnedItem> visibleItems = GetBagItems(bag, _category);
        EnsureBagSelection(visibleItems);

        GUILayout.BeginHorizontal(_bagPanel, GUILayout.Height(56f));
        DrawEntryIcon("ui/Icons/QiankunBagEntrance", 42f);
        GUILayout.BeginVertical();
        GUILayout.Label(MclslActorAccessor.DisplayName(_actor), _section);
        GUILayout.Label("专属纳物匣　·　物品随修士保存", _small);
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
        GUILayout.Label("六格纳珍", _section);
        GUILayout.Label("依物性分匣", _muted);
        GUILayout.Space(8f);
        for (int i = 0; i < Categories.Length; i++)
        {
            int index = i;
            int count = CountBagItems(bag, CategoryIds[i]);
            string label = Categories[i] + "\n" + count + " 件";
            if (GUILayout.Button(label, _category == i ? _bagSelectedCategory : _bagCategory,
                    GUILayout.Height(50f), GUILayout.ExpandWidth(true)))
            {
                _category = index;
                _bagScroll = Vector2.zero;
                _selectedBagItemKey = string.Empty;
            }
            GUILayout.Space(4f);
        }
        GUILayout.FlexibleSpace();
        GUILayout.Label("普通物品合并堆放\n法宝逐件保留耐久", _small);
        GUILayout.EndVertical();
    }

    private void DrawBagInventory(List<MclslOwnedItem> items)
    {
        GUILayout.BeginVertical(_bagPanel, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.BeginHorizontal();
        GUILayout.Label(Categories[_category] + " · 藏珍格", _section);
        GUILayout.FlexibleSpace();
        GUILayout.Label(items.Count + " 种", _muted);
        GUILayout.EndHorizontal();
        GUILayout.Label("点选格位查看物品详情与可用操作。", _muted);
        GUILayout.Space(6f);
        _bagScroll = GUILayout.BeginScrollView(_bagScroll, false, true, GUIStyle.none,
            GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));
        if (items.Count == 0)
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
            item.Category == "Artifact" ? "独立器物" : "堆叠 × " + Math.Max(1, owned.Count), _section);
        if (item.Category == "Artifact")
        {
            int durability = Mathf.Clamp(owned.Durability, 0, 100);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 100f, rect.width - 24f, 22f), "耐久 " + durability + "%", _small);
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
        GUILayout.Space(6f);
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
            GUILayout.Label("独立法宝", _muted);
            GUILayout.Label("耐久　" + Mathf.Clamp(owned.Durability, 0, 100) + "%", _section);
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
                MclslBagState refreshed = MclslBagSystem.Peek(_actor);
                List<MclslOwnedItem> remaining = GetBagItems(refreshed, _category);
                if (!ContainsBagSelection(remaining, _selectedBagItemKey)) _selectedBagItemKey = string.Empty;
            }
        }
        if (item.Category == "Artifact" && GUILayout.Button("装备此法宝", _bagButton, GUILayout.Height(36f)))
            MclslArtifactSystem.TryEquip(_actor, owned.InstanceId);
        GUILayout.EndVertical();
    }

    private void DrawMarketPage()
    {
        IReadOnlyList<MclslMarketListing> listings = MclslTianxuanMarket.Snapshot();
        RefreshMarketCategoryCounts(listings);
        IReadOnlyList<MclslMarketActivity> activity = _marketView == 1
            ? MclslTianxuanMarket.ListingActivitySnapshot()
            : _marketView == 2 ? MclslTianxuanMarket.PurchaseActivitySnapshot() : Array.Empty<MclslMarketActivity>();
        GUILayout.BeginHorizontal(_jadePanel, GUILayout.Height(62f));
        DrawMarketTicker("在售挂单", _marketTotalCount.ToString(), "件");
        DrawMarketTicker("上架记录", MclslTianxuanMarket.ListingActivitySnapshot().Count.ToString(), "笔");
        DrawMarketTicker("购入记录", MclslTianxuanMarket.PurchaseActivitySnapshot().Count.ToString(), "笔");
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(GUILayout.Width(174f));
        GUILayout.Label("万界交易时刻", _muted);
        GUILayout.Label("第 " + MclslRuntime.CurrentYear() + " 年 · 自动撮合", _body);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        string[] views = { "实时挂单", "上架动态", "购入成交" };
        for (int i = 0; i < views.Length; i++)
        {
            int view = i;
            if (GUILayout.Button(views[i], _marketView == view ? _selectedTab : _tab, GUILayout.Height(38f)))
            {
                _marketView = view;
                _marketScroll = Vector2.zero;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
        float categoryWidth = Mathf.Clamp(_rect.width * 0.16f, 120f, 176f);
        float infoWidth = Mathf.Clamp(_rect.width * 0.20f, 164f, 230f);
        DrawMarketCategoryRail(categoryWidth);
        GUILayout.Space(10f);
        float boardMinWidth = Mathf.Clamp(_rect.width * 0.34f, 250f, 390f);
        if (_marketView == 0) DrawMarketListings(listings, boardMinWidth);
        else DrawMarketActivity(activity, boardMinWidth);
        GUILayout.Space(10f);
        DrawMarketExchangeInfo(infoWidth);
        GUILayout.EndHorizontal();
    }

    private void DrawMarketTicker(string label, string value, string unit)
    {
        GUILayout.BeginVertical(_card, GUILayout.Width(142f), GUILayout.ExpandHeight(true));
        GUILayout.Label(label, _muted);
        GUILayout.BeginHorizontal();
        GUILayout.Label(value, _section);
        GUILayout.Label(unit, _small);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawMarketCategoryRail(float width)
    {
        GUILayout.BeginVertical(_jadePanel, GUILayout.Width(width), GUILayout.ExpandHeight(true));
        GUILayout.Label("交易板块", _section);
        GUILayout.Space(8f);
        for (int i = 0; i < Categories.Length; i++)
        {
            int index = i;
            GUIStyle style = _category == index ? _selectedTab : _tab;
            int count = _marketCategoryCounts[index];
            if (GUILayout.Button(Categories[i] + "    " + (_marketView == 0 ? count : "·"), style, GUILayout.Height(37f)))
            {
                _category = index;
                _marketScroll = Vector2.zero;
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.Label("全球修士自动撮合\n满足自用后挂售\n成交同步交割物品与贡献度", _small);
        GUILayout.EndVertical();
    }

    private void DrawMarketListings(IReadOnlyList<MclslMarketListing> listings, float minWidth)
    {
        GUILayout.BeginVertical(_innerPanel, GUILayout.MinWidth(minWidth), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.BeginHorizontal();
        GUILayout.Label(Categories[_category] + " · 实时盘口", _section);
        GUILayout.FlexibleSpace();
        GUILayout.Label("在售 " + _marketCategoryCounts[_category] + " 件", _muted);
        GUILayout.EndHorizontal();
        GUILayout.Space(6f);
        _marketScroll = GUILayout.BeginScrollView(_marketScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar,
            GUILayout.ExpandHeight(true));
        int count = 0;
        foreach (MclslMarketListing listing in listings)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(listing?.Item?.ItemId);
            if (item?.Category != CategoryIds[_category]) continue;
            count++;
            GUILayout.BeginHorizontal(_card, GUILayout.MinHeight(72f));
            DrawIcon(item, 50f);
            GUILayout.BeginVertical(GUILayout.MinWidth(125f), GUILayout.ExpandWidth(true));
            GUILayout.Label(item.Name + "  ·  " + DisplayGradeName(item), _body);
            GUILayout.Label("卖方 " + SellerName(listing.SellerId) + "　挂单年份 " + listing.Year, _small);
            GUILayout.EndVertical();
            GUILayout.BeginVertical(_jadePanel, GUILayout.Width(98f), GUILayout.ExpandHeight(true));
            GUILayout.Label(listing.Price + " 点", _priceStyle);
            GUILayout.Label("单件", _muted);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
        }
        if (count == 0)
        {
            GUILayout.Space(18f);
            GUILayout.Label("此板块暂无挂单", _section);
            GUILayout.Label("修士有可交易余物时会自动挂牌，成交记录将同步归档。", _muted);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawMarketActivity(IReadOnlyList<MclslMarketActivity> activities, float minWidth)
    {
        GUILayout.BeginVertical(_innerPanel, GUILayout.MinWidth(minWidth), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.Label(_marketView == 1 ? "修士上架动态" : "修士购入成交", _section);
        GUILayout.Label(_marketView == 1 ? "展示自动挂牌的物品、卖方与挂牌年份。" : "展示买卖双方、物品与实际成交贡献度。", _muted);
        GUILayout.Space(6f);
        _marketScroll = GUILayout.BeginScrollView(_marketScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar,
            GUILayout.ExpandHeight(true));
        int count = 0;
        foreach (MclslMarketActivity record in activities)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(record.ItemId);
            if (item == null || item.Category != CategoryIds[_category]) continue;
            count++;
            GUILayout.BeginHorizontal(_card, GUILayout.MinHeight(68f));
            DrawIcon(item, 46f);
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label(item.Name + "  ·  " + DisplayGradeName(item), _body);
            string names = _marketView == 1
                ? "上架修士　" + record.ActorName
                : "购入　" + record.ActorName + "　／　售出　" + record.OtherActorName;
            GUILayout.Label(names, _small);
            GUILayout.EndVertical();
            GUILayout.BeginVertical(_jadePanel, GUILayout.Width(106f));
            GUILayout.Label(record.Price + " 点", _priceStyle);
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

    private void DrawMarketExchangeInfo(float width)
    {
        GUILayout.BeginVertical(_jadePanel, GUILayout.Width(width), GUILayout.ExpandHeight(true));
        GUILayout.Label("万界撮合规则", _section);
        GUILayout.Space(8f);
        GUILayout.Label("全体修士依自身境界、职业、库存和贡献度自动买卖。玩家无需选中人物或手动下单。", _body);
        GUILayout.Space(8f);
        GUILayout.Label("交易结算", _section);
        GUILayout.Label("买方扣除贡献度，卖方获得贡献度，物品与法宝耐久随订单交割。无有效买方时挂单留存。", _small);
        GUILayout.Space(8f);
        GUILayout.Label("当前市场", _section);
        for (int i = 0; i < Categories.Length; i++)
            GUILayout.Label(Categories[i] + "　" + _marketCategoryCounts[i] + " 件", _small);
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(_card);
        GUILayout.Label("贡献度价格", _section);
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

    private void RefreshMarketCategoryCounts(IReadOnlyList<MclslMarketListing> listings)
    {
        Array.Clear(_marketCategoryCounts, 0, _marketCategoryCounts.Length);
        _marketTotalCount = 0;
        foreach (MclslMarketListing listing in listings)
        {
            MclslItemDefinition item = MclslItemCatalog.Get(listing?.Item?.ItemId);
            if (item == null) continue;
            _marketTotalCount++;
            int index = item.Category switch
            {
                "Pill" => 0, "Plant" => 1, "SpiritObject" => 2,
                "TalismanMaterial" => 3, "Talisman" => 4, "Artifact" => 5, _ => -1
            };
            if (index >= 0) _marketCategoryCounts[index]++;
        }
    }

    private static List<MclslOwnedItem> GetBagItems(MclslBagState bag, int category)
    {
        List<MclslOwnedItem> items = new();
        if (bag?.Items == null) return items;
        foreach (MclslOwnedItem owned in bag.Items)
        {
            if (owned == null) continue;
            MclslItemDefinition item = MclslItemCatalog.Get(owned.ItemId);
            if (item == null || item.Category != CategoryIds[category]) continue;
            items.Add(owned);
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
            return "artifact:" + (string.IsNullOrEmpty(owned?.InstanceId) ? owned?.ItemId : owned.InstanceId);
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
        "Talisman" => "符箓", "Artifact" => "法宝", _ => "灵物"
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

    private static string GradeName(int grade) => grade switch { 1 => "黄级", 2 => "玄级", 3 => "地级", 4 => "天级", _ => "材料" };

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
}
