using System.Linq;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Combat;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

// Use the native species metadata row prefab, including its typography and background.
internal sealed class MclslContractInfoBar : MonoBehaviour
{
    private UnitWindow _window;
    private Actor _actor;
    private KeyValueField _row;
    private GameObject _choicePanel;
    private float _nextRefresh;
    private KeyValueField _species;
    private Actor _shownTarget;
    private string _shownName, _shownRealm;
    private bool _shownOwner;
    private int _shownKind = int.MinValue;
    private static MclslContractInfoBar _visibleBar;

    internal static void NotifyRelatedChange(Actor actor)
    {
        var bar = _visibleBar;
        if (bar != null && bar._window != null && bar._window.gameObject.activeInHierarchy
            && (ReferenceEquals(actor, bar._actor) || ReferenceEquals(actor, bar._shownTarget)))
            Refresh(bar._window);
    }
    private const string RowName = "[KV] mclsl_contract";

    internal static void Refresh(UnitWindow window)
    {
        if (window == null || !window.gameObject.activeInHierarchy) return;
        var bar = window.GetComponent<MclslContractInfoBar>();
        var contract = window.actor?.data == null ? null : MclslCareerCombat.Contract(window.actor);
        if (contract == null) { Hide(window); return; }
        var species = bar?._species;
        if (species == null || species.name != "[KV] subspecies")
            species = window.meta_rows_container.GetComponentsInChildren<KeyValueField>(true)
                .FirstOrDefault(x => x.name == "[KV] subspecies");
        if (species == null) return;
        if (bar == null) bar = window.gameObject.AddComponent<MclslContractInfoBar>();
        bar._window = window;
        bar._species = species;
        _visibleBar = bar;
        if (bar._actor != window.actor && bar._choicePanel != null) bar._choicePanel.SetActive(false);
        bar._actor = window.actor;
        if (bar._row == null)
        {
            bar._row = Instantiate(species, species.transform.parent, false);
            bar._row.name = RowName;
            bar._row.setMetaForTooltip((MetaType)0, -1, null, null);
            bar._row.on_hover_value = null;
            bar._row.on_hover_value_out = null;
        }
        bar._row.transform.SetSiblingIndex(species.transform.GetSiblingIndex() + 1);
        bar._row.gameObject.SetActive(true);
        BindClicks(bar._row, bar.OnClick);
        bar._row.checkOddEvenColor(species.transform.GetSiblingIndex() + 1);
        bool owner = ReferenceEquals(window.actor, contract.Owner);
        Actor target = owner ? contract.Pet : contract.Owner;
        bool alive = MclslActorAccessor.Alive(target);
        string targetName = alive ? target.getName() : null;
        string targetRealm = alive && !owner ? MclslActorAccessor.Realm(target) : null;
        if (!ReferenceEquals(bar._shownTarget, target) || bar._shownName != targetName
            || bar._shownRealm != targetRealm || bar._shownOwner != owner || bar._shownKind != contract.Kind)
        {
            bar._shownTarget = target; bar._shownName = targetName; bar._shownRealm = targetRealm;
            bar._shownOwner = owner; bar._shownKind = contract.Kind;
            string title = owner ? "契约灵宠" : "主人";
            if (bar._row.name_text.text != title) bar._row.name_text.text = title;
            string value = alive
                ? targetName + (owner ? "" : "〔" + MclslRealmIds.Display(targetRealm) + "〕") : "暂无";
            if (bar._row.value.text != value) bar._row.value.text = value;
            Sprite icon = owner
                ? SpriteTextureLoader.getSprite("ui/ContractPets/" + contract.Kind)
                : target.subspecies.getUnitSpriteForBanner();
            if (bar._row.icon.sprite != icon) bar._row.icon.sprite = icon;
            if (bar._row.icon.gameObject.activeSelf != (icon != null)) bar._row.icon.gameObject.SetActive(icon != null);
            bar._row.icon.preserveAspect = true;
        }
        if (bar._choicePanel != null)
        {
            bar._choicePanel.transform.SetSiblingIndex(bar._row.transform.GetSiblingIndex() + 1);
            if (!contract.NeedsChoice) bar._choicePanel.SetActive(false);
        }
    }

    private static void BindClicks(KeyValueField row, UnityEngine.Events.UnityAction click)
    {
        if (row.on_click_value == click) return;
        // Native OnDisable clears on_click_value. Rebind after activation and bypass
        // the native value button's tooltip-first click path for this relationship link.
        row.on_click_value = click;
        Button valueButton = row.value.GetComponent<Button>();
        valueButton.onClick = new Button.ButtonClickedEvent();
        valueButton.onClick.AddListener(click);
        Button rowButton = row.GetComponent<Button>() ?? row.gameObject.AddComponent<Button>();
        rowButton.targetGraphic = row.background;
        rowButton.transition = Selectable.Transition.None;
        rowButton.onClick = new Button.ButtonClickedEvent();
        rowButton.onClick.AddListener(click);
    }

    /// <summary>
    /// 原版重新显示人物窗口会清空 <c>on_click_value</c>（KeyValueField.OnDisable），
    /// 且重新显示后 <c>window.actor</c> 可能已经换成跳转目标。这里按当前显示的人物
    /// 重新绑定一次，避免“点一次就失灵”或点了跳到旧目标。
    /// </summary>
    internal static void RefreshFromOpen()
    {
        var bar = _visibleBar;
        if (bar == null) return;
        var window = bar._window;
        if (window == null || !window.gameObject.activeInHierarchy) return;
        Refresh(window);
    }

    internal static void Hide(UnitWindow window)
    {
        var bar = window.GetComponent<MclslContractInfoBar>();
        if (bar == null) return;
        if (bar._row != null) bar._row.gameObject.SetActive(false);
        if (bar._choicePanel != null) bar._choicePanel.SetActive(false);
    }

    private void Update()
    {
        if (_window == null || !_window.gameObject.activeInHierarchy || Time.unscaledTime < _nextRefresh) return;
        _nextRefresh = Time.unscaledTime + 3f;
        Refresh(_window);
    }

    private void OnClick()
    {
        var contract = MclslCareerCombat.Contract(_actor);
        if (contract == null) return;
        if (contract.NeedsChoice)
        {
            if (_choicePanel == null) BuildChoice();
            _choicePanel.SetActive(!_choicePanel.activeSelf);
            return;
        }
        Actor target = ReferenceEquals(_actor, contract.Owner) ? contract.Pet : contract.Owner;
        if (!MclslActorAccessor.Alive(target)) return;
        // 走统一入口：先结束窗口动画再打开，并在重新显示后重新绑定回调，
        // 否则连续点击会因为原生 OnDisable 清空回调而失效。
        MclslActorWindowOpener.Open(target);
    }

    private void BuildChoice()
    {
        _choicePanel = new GameObject("MclslContractChoice", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        _choicePanel.transform.SetParent(_row.transform.parent, false);
        _choicePanel.GetComponent<LayoutElement>().preferredHeight = 24f;
        var layout = _choicePanel.GetComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.spacing = 4f;
        string[] names = { "灵狼", "岩龟", "雷隼", "灵鹿" };
        for (int i = 0; i < names.Length; i++)
        {
            int kind = i;
            var button = new GameObject(names[i], typeof(RectTransform), typeof(Image), typeof(Button));
            button.transform.SetParent(_choicePanel.transform, false);
            button.GetComponent<Image>().color = MclslUiTheme.RankButton;
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (MclslCareerCombat.ChooseContract(_actor, kind)) { _choicePanel.SetActive(false); Refresh(_window); }
            });
            var label = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            label.transform.SetParent(button.transform, false);
            label.font = LocalizedTextManager.current_font;
            label.fontSize = MclslUiTheme.ReadableFontSize(10);
            label.alignment = TextAnchor.MiddleCenter;
            label.text = names[i]; label.raycastTarget = false;
            label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        }
        _choicePanel.SetActive(false);
    }
}
