using System;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Combat;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 双向契约关系栏。作为原版 UnitWindow 左侧属性页 Overview 正文的一行插入，
/// 位于“境界”行下方、“功法”列表上方；随属性页滚动，后续内容自然下移。
/// 自身每秒只刷新文字（生命/倒计时），四选一面板仅在切人、契约状态变化或
/// 提交成功时关闭，普通倒计时刷新不会把面板关掉。
/// </summary>
internal sealed class MclslContractInfoBar : MonoBehaviour
{
    private const string BarName = "MclslContractBar";
    private static readonly string[] KindNames = { "灵狼", "岩龟", "雷隼", "灵鹿" };
    private static readonly string[] KindIcons =
    {
        "ui/Icons/mclsl_career_spirit_0", "ui/Icons/mclsl_career_spirit_1",
        "ui/Icons/mclsl_career_spirit_2", "ui/Icons/mclsl_career_spirit_3"
    };

    private UnitWindow _window;
    private Actor _actor;
    private long _avatarActorId = long.MinValue;
    private long _actorId = long.MinValue;
    private bool _lastNeedsChoice;
    private bool _built;

    private RectTransform _bar;
    private RectTransform _rowOne;
    private Button _barButton;
    private UiUnitAvatarElement _avatar;
    private Image _kindIcon;
    private Text _row1;
    private Text _row2;
    private GameObject _choicePanel;
    private Actor _choiceOwner;
    private double _nextTick;

    /// <summary>由 UnitWindow.showStatsRows / showInfo 的 UI 补丁每帧调用。</summary>
    internal static void Refresh(UnitWindow window)
    {
        if (window == null || !window.gameObject.activeInHierarchy) return;
        Transform content = window.transform.Find("Background/Scroll View/Viewport/Content");
        if (content == null) return;
        Actor actor = window.actor;
        if (actor?.data == null)
        {
            Hide(window);
            return;
        }
        MclslContractInfo contract = MclslCareerCombat.Contract(actor);
        if (contract == null)
        {
            Hide(window);
            return;
        }
        MclslContractInfoBar bar = Ensure(content);
        bar._window = window;
        bar.Place(content);
        bar.Apply(contract, actor);
    }

    internal static void Hide(UnitWindow window)
    {
        if (window == null) return;
        MclslContractInfoBar bar = window.GetComponentInChildren<MclslContractInfoBar>(true);
        if (bar == null) return;
        bar.CloseChoice();
        bar._actorId = long.MinValue;
        if (bar._bar != null) bar._bar.gameObject.SetActive(false);
    }

    private static MclslContractInfoBar Ensure(Transform content)
    {
        Transform existing = content.Find(BarName);
        GameObject barObject = existing != null ? existing.gameObject : new GameObject(BarName, typeof(RectTransform));
        if (existing == null) barObject.transform.SetParent(content, false);
        MclslContractInfoBar bar = barObject.GetComponent<MclslContractInfoBar>() ?? barObject.AddComponent<MclslContractInfoBar>();
        bar.Build(barObject.transform);
        bar._bar.gameObject.SetActive(true);
        return bar;
    }

    private void Build(Transform root)
    {
        if (_built) return;
        _built = true;
        _bar = root as RectTransform;
        _bar.anchorMin = new Vector2(0f, 1f);
        _bar.anchorMax = new Vector2(1f, 1f);
        _bar.pivot = new Vector2(0.5f, 1f);

        VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 1f;
        layout.padding = new RectOffset(10, 10, 2, 2);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        LayoutElement barLayout = root.gameObject.AddComponent<LayoutElement>();
        barLayout.preferredHeight = 50f;
        barLayout.minHeight = 44f;

        Image background = root.gameObject.AddComponent<Image>();
        background.color = new Color(0.07f, 0.16f, 0.15f, 0.45f);
        background.raycastTarget = true;

        var rowOne = new GameObject("Row1", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        rowOne.transform.SetParent(root, false);
        _rowOne = rowOne.GetComponent<RectTransform>();
        HorizontalLayoutGroup rowLayout = rowOne.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 4f;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;
        rowLayout.childAlignment = TextAnchor.MiddleLeft;
        rowOne.GetComponent<LayoutElement>().preferredHeight = 26f;

        _row1 = CreateText(rowOne.transform, "Title");
        _row1.fontSize = MclslUiTheme.ReadableFontSize(11);
        _row1.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        LayoutElement titleLayout = _row1.gameObject.AddComponent<LayoutElement>();
        titleLayout.flexibleWidth = 1f;
        titleLayout.preferredHeight = 24f;

        _row2 = CreateText(root, "Line2");
        _row2.fontSize = MclslUiTheme.ReadableFontSize(9);
        _row2.color = new Color(0.78f, 0.86f, 0.82f, 1f);

        _choicePanel = new GameObject("ChoicePanel", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        _choicePanel.transform.SetParent(root, false);
        HorizontalLayoutGroup choiceLayout = _choicePanel.GetComponent<HorizontalLayoutGroup>();
        choiceLayout.spacing = 4f;
        choiceLayout.childControlWidth = true;
        choiceLayout.childControlHeight = true;
        choiceLayout.childForceExpandWidth = true;
        choiceLayout.childForceExpandHeight = false;
        _choicePanel.GetComponent<LayoutElement>().preferredHeight = 24f;
        for (int i = 0; i < KindNames.Length; i++)
        {
            int kind = i;
            AddChoiceButton(_choicePanel.transform, KindNames[i], () => Choose(kind));
        }
        _choicePanel.SetActive(false);

        _barButton = root.gameObject.AddComponent<Button>();
        _barButton.targetGraphic = background;
        _barButton.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = _barButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.85f, 1f, 0.96f, 1f);
        colors.pressedColor = new Color(0.7f, 0.9f, 0.86f, 1f);
        colors.fadeDuration = 0.08f;
        _barButton.colors = colors;
        _barButton.onClick.AddListener(OnBarClicked);
    }

    private static Text CreateText(Transform parent, string name)
    {
        Transform found = parent.Find(name);
        Text text = found == null ? null : found.GetComponent<Text>();
        if (text == null)
        {
            text = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(ContentSizeFitter)).GetComponent<Text>();
            text.transform.SetParent(parent, false);
        }
        ContentSizeFitter fitter = text.GetComponent<ContentSizeFitter>() ?? text.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        text.font = LocalizedTextManager.current_font ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.supportRichText = true;
        text.resizeTextForBestFit = false;
        text.fontSize = MclslUiTheme.ActorPanelTextSize;
        text.lineSpacing = MclslUiTheme.ActorPanelTextLineSpacing;
        text.color = MclslUiTheme.ActorPanelText;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        Shadow shadow = text.GetComponent<Shadow>() ?? text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(1f, -1f);
        shadow.useGraphicAlpha = true;
        return text;
    }

    private void AddChoiceButton(Transform parent, string label, Action onClick)
    {
        GameObject buttonObject = new GameObject("Choice_" + label,
            typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<LayoutElement>().preferredHeight = 22f;
        Image image = buttonObject.GetComponent<Image>();
        image.color = MclslUiTheme.RankButton;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => onClick());
        Text text = CreateText(buttonObject.transform, "Text");
        text.fontSize = MclslUiTheme.ReadableFontSize(10);
        text.alignment = TextAnchor.MiddleCenter;
        text.text = label;
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        ContentSizeFitter fitter = text.GetComponent<ContentSizeFitter>();
        if (fitter != null) Destroy(fitter);
    }

    /// <summary>原生属性行之后、功法等元信息列表之前，独立于文字语言和修士身份。</summary>
    private void Place(Transform content)
    {
        Transform rows = _window.stats_rows_container.transform;
        Transform parent = rows.parent;
        int index = rows.GetSiblingIndex() + 1;
        if (_bar.parent == parent && _bar.GetSiblingIndex() < index) index--;
        if (_bar.parent != parent) _bar.SetParent(parent, false);
        if (_bar.GetSiblingIndex() != index) _bar.SetSiblingIndex(index);
    }

    private void Apply(MclslContractInfo contract, Actor actor)
    {
        _actor = actor;
        long actorId = MclslActorAccessor.Id(actor);
        bool actorChanged = actorId != _actorId;
        bool stateChanged = contract.NeedsChoice != _lastNeedsChoice;
        _actorId = actorId;
        _lastNeedsChoice = contract.NeedsChoice;
        if (actorChanged || stateChanged || !contract.NeedsChoice) CloseChoice();
        RenderTexts(contract, actor);
    }

    private void RenderTexts(MclslContractInfo contract, Actor actor)
    {
        bool isOwner = ReferenceEquals(actor, contract.Owner);
        if (contract.NeedsChoice)
        {
            _choiceOwner = contract.Owner;
            SetAvatar(null);
            SetKindIcon(-1);
            _row1.text = "<b>本命契灵</b> · <color=#FFD37A>待缔契</color>";
            _row2.text = "点击选择契灵：灵狼 / 岩龟 / 雷隼 / 灵鹿";
            _barButton.interactable = true;
            return;
        }
        if (isOwner)
        {
            _choiceOwner = contract.Owner;
            if (MclslActorAccessor.Alive(contract.Pet))
            {
                Actor pet = contract.Pet;
                SetAvatar(pet);
                SetKindIcon(-1);
                _row1.text = "<b>本命契灵</b> · " + KindName(contract.Kind) + " · " + Name(pet);
                _row2.text = "生命 <color=#FFD37A>" + pet.getHealth() + "</color> / " + pet.getMaxHealth();
                _barButton.interactable = true;
                return;
            }
            SetAvatar(null);
            SetKindIcon(contract.Kind);
            _row1.text = "<b>本命契灵</b> · " + KindName(contract.Kind) + " · " + RecentName(contract);
            _row2.text = RecoverText(contract);
            _barButton.interactable = false;
            return;
        }
        Actor owner = contract.Owner;
        _choiceOwner = owner;
        SetAvatar(owner);
        SetKindIcon(-1);
        _row1.text = "<b>契约主人</b> · " + Name(owner);
        _row2.text = OwnerDetail(owner);
        _barButton.interactable = MclslActorAccessor.Alive(owner);
    }

    private void Update()
    {
        if (_bar == null || !_bar.gameObject.activeInHierarchy) return;
        if (_window == null || !_window.gameObject.activeInHierarchy) return;
        if (Time.unscaledTime < _nextTick) return;
        _nextTick = Time.unscaledTime + 1.0;
        Actor actor = _window.actor;
        if (actor?.data == null) return;
        MclslContractInfo contract = MclslCareerCombat.Contract(actor);
        if (contract == null)
        {
            _bar.gameObject.SetActive(false);
            return;
        }
        if (contract.NeedsChoice != _lastNeedsChoice)
        {
            _lastNeedsChoice = contract.NeedsChoice;
            if (!contract.NeedsChoice) CloseChoice();
        }
        // 仅刷新文字，绝不触碰已打开的四选一面板。
        RenderTexts(contract, actor);
    }

    private void CloseChoice()
    {
        if (_choicePanel != null) _choicePanel.SetActive(false);
    }

    private void OnBarClicked()
    {
        if (_choicePanel != null && _choicePanel.activeSelf)
        {
            CloseChoice();
            return;
        }
        MclslContractInfo info = _actor?.data == null ? null : MclslCareerCombat.Contract(_actor);
        if (info == null) return;
        if (info.NeedsChoice)
        {
            if (_choicePanel != null) _choicePanel.SetActive(true);
            return;
        }
        bool isOwner = ReferenceEquals(_actor, info.Owner);
        Actor target = isOwner ? info.Pet : info.Owner;
        if (MclslActorAccessor.Alive(target)) ActionLibrary.openUnitWindow(target);
    }

    private void Choose(int kind)
    {
        Actor owner = _choiceOwner;
        if (owner?.data == null) return;
        if (!MclslCareerCombat.ChooseContract(owner, kind)) return;
        CloseChoice();
        _lastNeedsChoice = false;
        MclslContractInfo info = MclslCareerCombat.Contract(owner);
        if (info != null) RenderTexts(info, owner);
    }

    private void SetAvatar(Actor actor)
    {
        if (actor == null)
        {
            if (_avatar != null) _avatar.gameObject.SetActive(false);
            _avatarActorId = long.MinValue;
            return;
        }
        if (_avatar == null)
        {
            UiUnitAvatarElement prefab = Resources.Load<UiUnitAvatarElement>("ui/UnitAvatarElement");
            if (prefab == null) return;
            _avatar = Instantiate(prefab, _rowOne);
            _avatar.name = "ContractAvatar";
            _avatar.transform.SetAsFirstSibling();
            _avatar.GetComponent<RectTransform>().sizeDelta = new Vector2(24f, 24f);
            _avatar.show_banner_kingdom = false;
            _avatar.show_banner_clan = false;
            if (_avatar.kingdomBanner != null) _avatar.kingdomBanner.gameObject.SetActive(false);
            if (_avatar.clanBanner != null) _avatar.clanBanner.gameObject.SetActive(false);
            LayoutElement layout = _avatar.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 26f;
            layout.preferredHeight = 26f;
            CanvasGroup group = _avatar.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            _avatarActorId = long.MinValue;
        }
        _avatar.gameObject.SetActive(true);
        long id = MclslActorAccessor.Id(actor);
        if (id != _avatarActorId)
        {
            _avatar.show(actor);
            _avatarActorId = id;
        }
    }

    private void SetKindIcon(int kind)
    {
        if (kind < 0)
        {
            if (_kindIcon != null) _kindIcon.gameObject.SetActive(false);
            return;
        }
        if (_kindIcon == null)
        {
            GameObject iconObject = new GameObject("KindIcon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconObject.transform.SetParent(_rowOne, false);
            iconObject.transform.SetAsFirstSibling();
            iconObject.GetComponent<RectTransform>().sizeDelta = new Vector2(24f, 24f);
            iconObject.GetComponent<LayoutElement>().preferredWidth = 26f;
            iconObject.GetComponent<LayoutElement>().preferredHeight = 26f;
            _kindIcon = iconObject.GetComponent<Image>();
            _kindIcon.preserveAspect = true;
            _kindIcon.raycastTarget = false;
        }
        _kindIcon.sprite = SpriteTextureLoader.getSprite(KindIcons[Math.Clamp(kind, 0, KindIcons.Length - 1)]);
        _kindIcon.gameObject.SetActive(true);
    }

    private static string KindName(int kind)
        => kind >= 0 && kind < KindNames.Length ? KindNames[kind] : "未知";

    private static string Name(Actor actor)
    {
        if (actor?.data == null) return "无";
        string name = (actor.getName() ?? string.Empty).Trim();
        return string.IsNullOrEmpty(name) ? MclslActorAccessor.DisplayName(actor) : name;
    }

    private static string RecentName(MclslContractInfo contract)
    {
        string name = contract.LastPetName ?? string.Empty;
        return string.IsNullOrWhiteSpace(name) ? "无名" : name.Trim();
    }

    private static string RecoverText(MclslContractInfo contract)
    {
        if (contract.Remaining > 0)
            return "重召剩余 <color=#FFD37A>" + Math.Ceiling(contract.Remaining) + "</color> 秒";
        if (MclslSpellSystem.CurrentMana(contract.Owner) < MclslContractRules.SummonManaCost)
            return "<color=#FF8877>等待灵力</color>";
        return "重召就绪";
    }

    private static string OwnerDetail(Actor owner)
    {
        string realm = MclslRealmIds.Display(MclslActorAccessor.Realm(owner));
        if (string.IsNullOrWhiteSpace(realm)) realm = "凡俗";
        string text = "<color=#9CD7FF>" + realm + "</color>";
        if (MclslCombatCareerSystem.Career(owner) == 5)
        {
            int rank = Math.Clamp(MclslCombatCareerSystem.Rank(owner), 0, MclslCombatCareerCatalog.Ranks.Length - 1);
            text += " · 灵契师 <color=#FFD37A>" + MclslCombatCareerCatalog.Ranks[rank] + "</color>";
        }
        return text;
    }
}
