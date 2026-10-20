using System;
using System.Linq;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

/// <summary>Player-only spell picker using the native scroll-window and icon grid.</summary>
internal static class MclslSpellCodexWindow
{
    private const string WindowId = "MclslSpellCodexWindow";
    private static ScrollWindow _window;
    private static Text _status;
    private static UnitWindow _targetWindow;

    internal static void ShowForWindow(UnitWindow window)
    {
        if (window?.gameObject == null || !window.gameObject.activeInHierarchy
            || window.actor?.data == null || !window.actor.isAlive()) return;
        _targetWindow = window;
        if (_window == null) Init();
        if (_window == null) return;
        RefreshStatus();
        MclslWindowOpenGuard.Show(_window, WindowId, false);
    }

    internal static void RefreshTarget(UnitWindow window)
    {
        if (_targetWindow == window) RefreshStatus();
    }

    internal static void ClearTarget(UnitWindow window)
    {
        if (window != null && _targetWindow != window) return;
        _targetWindow = null;
        RefreshStatus();
    }

    private static bool TryTargetActor(out Actor actor)
    {
        actor = _targetWindow != null && _targetWindow.gameObject.activeInHierarchy ? _targetWindow.actor : null;
        return actor?.data != null && actor.isAlive();
    }

    private static void Init()
    {
        MclslLocalizationBridge.RegisterKey("mclsl_spell_codex_title", "法术录");
        _window = WindowCreator.CreateEmptyWindow(WindowId, "mclsl_spell_codex_title", "ui/Icons/SpellCodexEntrance");
        Transform background = _window?.transform.Find("Background");
        if (background == null) return;
        background.GetComponent<RectTransform>().sizeDelta = new Vector2(360f, 480f);
        Transform native = background.Find("Scroll View");
        if (native != null) native.gameObject.SetActive(false);

        GameObject status = new("Status", typeof(RectTransform), typeof(Text));
        status.transform.SetParent(background, false);
        RectTransform statusRect = (RectTransform)status.transform;
        statusRect.anchorMin = statusRect.anchorMax = statusRect.pivot = new Vector2(0.5f, 1f);
        statusRect.anchoredPosition = new Vector2(0f, -42f);
        statusRect.sizeDelta = new Vector2(332f, 34f);
        _status = status.GetComponent<Text>();
        _status.font = LocalizedTextManager.current_font;
        _status.fontSize = MclslUiTheme.ReadableFontSize(14);
        _status.alignment = TextAnchor.MiddleCenter;
        _status.color = new Color(0.9f, 0.85f, 0.65f);

        GameObject scrollObject = new("SpellCodexScroll", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollObject.transform.SetParent(background, false);
        RectTransform scrollRect = (RectTransform)scrollObject.transform;
        scrollRect.anchorMin = scrollRect.anchorMax = scrollRect.pivot = new Vector2(0.5f, 0.5f);
        scrollRect.anchoredPosition = new Vector2(0f, -30f);
        scrollRect.sizeDelta = new Vector2(340f, 360f);
        scrollObject.GetComponent<Image>().color = new Color(0.04f, 0.23f, 0.23f, 0.95f);
        ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;

        GameObject contentObject = new("Content", typeof(RectTransform), typeof(GridLayoutGroup));
        contentObject.transform.SetParent(scrollObject.transform, false);
        RectTransform content = (RectTransform)contentObject.transform;
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = content.pivot = new Vector2(1f, 1f);
        content.anchoredPosition = Vector2.zero;
        GridLayoutGroup grid = contentObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(48f, 48f);
        grid.spacing = new Vector2(6f, 6f);
        grid.padding = new RectOffset(8, 8, 8, 8);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 6;
        var spells = MclslSpellSystem.All.OrderBy(x => x.MinRealm).ThenBy(x => x.Id, StringComparer.Ordinal).ToArray();
        content.sizeDelta = new Vector2(0f, 16f + ((spells.Length + 5) / 6) * 54f);
        scroll.viewport = scrollRect;
        scroll.content = content;
        foreach (MclslSpellDefinition spell in spells)
        {
            GameObject cell = new("Spell_" + spell.Id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(TipButton));
            cell.transform.SetParent(content, false);
            Image frame = cell.GetComponent<Image>();
            frame.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
            frame.type = Image.Type.Sliced;
            frame.color = new Color(0.38f, 0.78f, 0.74f, 1f);
            GameObject iconObject = new("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(cell.transform, false);
            RectTransform iconRect = (RectTransform)iconObject.transform;
            iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(36f, 36f);
            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = SpriteTextureLoader.getSprite(spell.IconPath);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            TipButton tip = cell.GetComponent<TipButton>();
            tip.textOnClick = MclslLocalizationBridge.RuntimeText(spell.Name);
            tip.textOnClickDescription = MclslLocalizationBridge.RuntimeText(spell.Description + "\n" + LM.Get("mclsl_spell_codex_tip"));
            cell.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (TryTargetActor(out Actor actor))
                    MclslSpellSystem.LearnFromPlayer(actor, spell.Id);
                RefreshStatus();
            });
        }
        _window.gameObject.SetActive(false);
    }

    private static void RefreshStatus()
    {
        if (_status == null) return;
        _status.text = TryTargetActor(out Actor actor)
            ? LM.Get("mclsl_spell_codex_selected") + actor.getName() + " · " + LM.Get("mclsl_spell_codex_click")
            : LM.Get("mclsl_spell_codex_select_first");
    }
}
