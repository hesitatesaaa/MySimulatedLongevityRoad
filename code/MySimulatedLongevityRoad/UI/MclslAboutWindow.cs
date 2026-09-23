using System;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 独立的入道指南。它不复用仙录的 tab/sidebar 状态，滚动条和窗口样式也单独维护。
/// </summary>
internal sealed class MclslAboutWindow : MonoBehaviour
{
    private static MclslAboutWindow _instance;
    private static readonly string[] SectionNames =
    {
        "第一次怎么玩", "各入口做什么", "古法与新法", "灵根与境界", "功法传承", "遗府秘境",
        "洞天灾变", "万仙盟与五老会", "还真", "排行榜", "猫宝与设置", "常见问题"
    };
    private static readonly string[] SectionBodies =
    {
        "先开启一张中等地图，等待小人自然成长。五岁进行灵根判定，六岁只是跨帧时的容错窗口；有灵根者会在仙录中留下修行记录。\n\n建议顺序：看仙录了解时代 → 等待修士入道 → 观察传承与遗府 → 在修士榜比较人物 → 需要轮回时再进入还真之门。",
        "玄黄仙录：查看时代、修行、功法传承、遗府秘境、灾变、人物生死与还真纪事。\n玄黄修士榜：按境界、战力、灵根、国家、种属和人物特征筛选。\n还真之门：管理还真空间、锚点和前世遗产。\n猫宝·时光长河：保存完整人物快照并在需要时放置。\n入道指南：查看本页；设置：调整概率、公告、锚点等规则。",
        "古法依赖灵根、功法、真元、神魂和大道感悟；师徒授法、支脉分立、失传和遗迹复现都会被记录。新法由传法变世开启，修士不再以灵根作为唯一门槛，万仙盟和五老会也会在新法时代登场。旧法人物不会被强行改写成新法人物。",
        "灵根在五岁判定，六岁只补偿高倍速跳年。灵根品阶影响资质、悟法和破境表现。境界按炼气、筑基、金丹、元婴、化神、合道、长生推进；不同时代的功法上限和资源条件不同。",
        "仙录会记录功法创立者姓名快照、师承人物快照、当前传承者、分支母脉、失传年份、复现年份以及来源遗迹。人物死亡后，姓名快照仍保留；人物栏的“功法”和“法脉”按钮可直接定位仙录记录。",
        "遗府是失传功法和前人注疏沉积的来源，秘境则可能保留更完整的宗门道统。探索会消耗风险与资源，也可能带来功法复现、遗迹死亡或新的法脉线索。地图上的标记只在档案变化时增量更新。",
        "元婴可借洞天避劫，天地灵机、地火、星石等事件会改变山河条件。洞天有完整度、容量和争夺记录；灾变不会删除修士功能，而是通过事件、资源和风险改变修行环境。",
        "万仙盟是新法时代的公开秩序，提供功法、贡献和遗迹任务；五老会从暗处施加逆理影响。二者都是背景势力，不会替代世界原有国家，也不会把凡俗国度改成宗门国家。",
        "还真会保存宿主的修为、功法、突破造物、特征和世界魄位。可以建立锚点、在死亡后选择有限遗产并继续本世；还真降世的魄身、地图标记和仙录刷新均采用缓存与固定间隔维护。",
        "默认榜单按境界从高到低，同境界再按战力和姓名稳定排序。选择“境界排序”时会使用境界索引，不会拿战力冒充境界；战力仍是独立排序项，可切换升降序。",
        "猫宝适合保存一个人物的完整快照，死亡后仍可查看历史姓名和修行信息。设置入口可以调整幼年灵根概率、公告、遗迹游历、还真锚点与自动收藏；新设置只影响之后尚未判定的对象。",
        "Q：把概率改成 0% 或 100% 会重算旧人物吗？\nA：不会，只影响尚未完成灵根判定的小人，稳定种子和五/六岁逻辑不变。\n\nQ：为什么人物死了还能在仙录里看到？\nA：这是历史快照，不是悬空 Actor 引用。\n\nQ：FPS 下降怎么办？\nA：先确认是否只在还真降世后发生；本版已将地图档案扫描、贴图加载、重绘计时和天地之魄维护改为缓存或固定间隔。"
    };

    private bool _visible;
    private int _section;
    private Vector2 _scroll;
    private Rect _rect = new(150f, 90f, 1180f, 820f);
    private GUIStyle _windowStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _scrollbarStyle;
    private Texture2D _white;
    private bool _stylesReady;

    internal static void Show()
    {
        if (_instance == null)
        {
            GameObject host = new("MclslAboutWindow");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<MclslAboutWindow>();
        }
        _instance._rect = FitRect();
        _instance._section = 0;
        _instance._scroll = Vector2.zero;
        _instance._visible = true;
        _instance.enabled = true;
    }

    private void Update()
    {
        if (_visible && Input.GetKeyDown(KeyCode.Escape)) _visible = false;
    }

    private void OnGUI()
    {
        if (!_visible) return;
        EnsureStyles();
        GUIStyle oldWindow = GUI.skin.window;
        GUIStyle oldLabel = GUI.skin.label;
        GUIStyle oldButton = GUI.skin.button;
        GUI.skin.window = _windowStyle;
        GUI.skin.label = _labelStyle;
        GUI.skin.button = _buttonStyle;
        Color old = GUI.color;
        GUI.color = Color.white;
        try { _rect = GUI.Window(781209, _rect, DrawWindow, "入道指南 · 我的模拟长生路"); }
        finally
        {
            GUI.color = old;
            GUI.skin.window = oldWindow;
            GUI.skin.label = oldLabel;
            GUI.skin.button = oldButton;
        }
    }

    private void DrawWindow(int id)
    {
        GUILayout.Space(12f);
        GUILayout.BeginHorizontal();
        GUILayout.Label("<b><color=#D8C778>入道指南</color></b>  从第一步到长生路的简明卷宗");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("关闭", GUILayout.Width(78f), GUILayout.Height(34f))) _visible = false;
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        GUILayout.BeginHorizontal(GUI.skin.box);
        for (int i = 0; i < SectionNames.Length; i++)
        {
            GUI.backgroundColor = i == _section ? new Color(0.36f, 0.52f, 0.47f, 1f) : new Color(0.15f, 0.23f, 0.23f, 1f);
            if (GUILayout.Button(SectionNames[i], GUILayout.Width(113f), GUILayout.Height(36f)))
            {
                _section = i;
                _scroll = Vector2.zero;
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        _scroll = GUILayout.BeginScrollView(_scroll, false, true, GUIStyle.none, _scrollbarStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        DrawCard(SectionNames[_section], SectionBodies[_section], "#8FE3D1");
        if (_section == 0)
        {
            DrawCard("一条稳妥的游玩路线", "观察出生与灵根 → 等待第一批修士 → 追踪一门功法的师承与分支 → 探索遗府/秘境 → 进入新法时代 → 用还真和猫宝保存你想留下的人物。", "#D8C778");
            DrawCard("小提示", "仙录窗口支持鼠标滚轮、拖动滚动条、切页和重新照录；本指南使用独立的滚动条，不会改变仙录、还真空间或猫宝窗口。", "#9CD7FF");
        }
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }

    private void DrawCard(string title, string body, string accent)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        Rect stripe = GUILayoutUtility.GetRect(100f, 5f, GUILayout.ExpandWidth(true));
        Color old = GUI.color;
        GUI.color = ParseColor(accent, Color.gray);
        GUI.DrawTexture(stripe, _white);
        GUI.color = old;
        GUILayout.Label("<size=21><b><color=" + accent + ">" + title + "</color></b></size>");
        GUILayout.Space(4f);
        GUILayout.Label(body);
        GUILayout.EndVertical();
        GUILayout.Space(8f);
    }

    private void EnsureStyles()
    {
        if (_stylesReady) return;
        _stylesReady = true;
        _white = new Texture2D(1, 1);
        _white.SetPixel(0, 0, Color.white);
        _white.Apply();
        _windowStyle = new GUIStyle(GUI.skin.window) { fontSize = 23, padding = new RectOffset(16, 16, 28, 14) };
        _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, richText = true, wordWrap = true, normal = { textColor = MclslUiTheme.TextPrimary } };
        _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 15, richText = true, wordWrap = true, padding = new RectOffset(6, 6, 4, 4) };
        _scrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar) { fixedWidth = 15f };
        _scrollbarStyle.normal.background = Solid(new Color(0.08f, 0.17f, 0.17f, 0.95f));
        _scrollbarStyle.hover.background = _scrollbarStyle.normal.background;
        _scrollbarStyle.active.background = _scrollbarStyle.normal.background;
    }

    private static Texture2D Solid(Color color)
    {
        Texture2D texture = new(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private static Color ParseColor(string value, Color fallback)
    {
        return ColorUtility.TryParseHtmlString(value, out Color result) ? result : fallback;
    }

    private static Rect FitRect()
    {
        float width = Mathf.Min(1180f, Mathf.Max(900f, Screen.width - 100f));
        float height = Mathf.Min(820f, Mathf.Max(650f, Screen.height - 100f));
        return new Rect(Mathf.Max(30f, (Screen.width - width) / 2f), Mathf.Max(30f, (Screen.height - height) / 2f), width, height);
    }
}
