using UnityEngine;
using UnityEngine.UI;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 保留玄鉴版本角色窗口右侧信息框的尺寸、位置和滚动交互，
/// 仅替换其中的数据来源与显示内容。
/// </summary>
internal static class MclslActorInfoPanel
{
    private const string PanelName = "XuanJianInfoPanel";
    private const string TextName = "XuanJianInfo";
    private const int ActiveWindowRefreshIntervalFrames = 180;
    private static int _lastActiveWindowRefreshFrame = -9999;

    internal static void Refresh(UnitWindow window, bool resetScrollForNewActor = false, bool forceContentRefresh = false)
    {
        if (window == null) return;
        Transform background = ResolvePanelParent(window);
        if (background == null) return;
        if (window.actor == null || !window.actor.isAlive())
        {
            HidePanel(background);
            return;
        }
        if (!ShouldShowFor(window.actor))
        {
            HidePanel(background);
            return;
        }

        long actorId = MclslActorAccessor.Id(window.actor);
        Transform existingPanel = FindSingleRoot(background);
        PanelState existingState = existingPanel == null ? null : existingPanel.GetComponent<PanelState>();
        bool actorChangedBeforeEnsure = existingState == null || existingState.ActorId != actorId;
        if (existingPanel != null
            && existingState is { Initialized: true }
            && !actorChangedBeforeEnsure
            && !forceContentRefresh)
        {
            return;
        }

        Text text = EnsurePanel(background, out ScrollRect scroll, out PanelState state);
        if (text == null) return;
        if (scroll != null) scroll.gameObject.SetActive(true);
        bool wasEmpty = string.IsNullOrEmpty(text.text);
        bool actorChanged = state != null && state.ActorId != actorId;
        if (!actorChanged && !forceContentRefresh && !resetScrollForNewActor && state is { Initialized: true })
        {
            return;
        }

        string formatted = MclslActorInfoFormatter.Format(window.actor);
        if (string.IsNullOrWhiteSpace(formatted))
        {
            HidePanel(background);
            return;
        }
        bool shouldReset = scroll != null && resetScrollForNewActor && (actorChanged || wasEmpty || state is { Initialized: false });
        float keepScroll = scroll == null ? 1f : scroll.verticalNormalizedPosition;
        Vector2 keepContentPosition = scroll?.content == null ? Vector2.zero : scroll.content.anchoredPosition;
        if (state != null)
        {
            state.ActorId = actorId;
            state.Initialized = true;
        }
        bool textChanged = !string.Equals(text.text, formatted, System.StringComparison.Ordinal);
        if (textChanged)
        {
            text.text = formatted;
            if (scroll?.content != null) LayoutRebuilder.MarkLayoutForRebuild(scroll.content);
        }
        if (scroll == null) return;
        if (shouldReset)
        {
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f;
        }
        else if (textChanged)
        {
            if (scroll.content != null) scroll.content.anchoredPosition = keepContentPosition;
            scroll.velocity = Vector2.zero;
            _ = keepScroll;
        }
    }

    internal static void RefreshActiveWindowsThrottled(int frameCount)
    {
        if (frameCount - _lastActiveWindowRefreshFrame < ActiveWindowRefreshIntervalFrames) return;
        _lastActiveWindowRefreshFrame = frameCount;
        UnitWindow[] windows;
        try { windows = Resources.FindObjectsOfTypeAll<UnitWindow>(); }
        catch (System.Exception ex)
        {
            MclslDiagnostics.Error("actor-info-find-active-windows", "查找打开的角色窗口失败: " + ex.Message);
            return;
        }
        for (int i = 0; i < windows.Length; i++)
        {
            UnitWindow window = windows[i];
            if (window == null || !window.gameObject.activeInHierarchy || window.actor?.data == null) continue;
            Refresh(window, resetScrollForNewActor: false, forceContentRefresh: true);
        }
    }

    internal static void RefreshOpenForActor(Actor actor)
    {
        if (actor?.data == null) return;
        long id = MclslActorAccessor.Id(actor);
        UnitWindow[] windows;
        try { windows = Resources.FindObjectsOfTypeAll<UnitWindow>(); }
        catch (System.Exception ex)
        {
            MclslDiagnostics.Error("actor-info-find-actor-window", "查找指定角色窗口失败: " + ex.Message);
            return;
        }
        for (int i = 0; i < windows.Length; i++)
        {
            UnitWindow window = windows[i];
            if (window == null || !window.gameObject.activeInHierarchy || window.actor?.data == null) continue;
            if (MclslActorAccessor.Id(window.actor) != id) continue;
            Refresh(window, resetScrollForNewActor: false, forceContentRefresh: true);
        }
    }

    private static Text EnsurePanel(Transform parent, out ScrollRect scroll, out PanelState state)
    {
        Transform existing = FindSingleRoot(parent);
        GameObject panel = existing?.gameObject;
        if (panel == null)
        {
            panel = new GameObject(PanelName, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(ScrollRect));
            panel.transform.SetParent(parent, false);
        }
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
        state = panel.GetComponent<PanelState>() ?? panel.AddComponent<PanelState>();

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(256f, 40f);
        rect.localScale = Vector3.one;
        rect.sizeDelta = new Vector2(172f, 332f);

        Image image = panel.GetComponent<Image>();
        image.color = new Color(0.025f, 0.035f, 0.05f, 0.36f);
        image.raycastTarget = false;

        Outline outline = panel.GetComponent<Outline>();
        outline.effectColor = new Color(0.55f, 0.82f, 0.76f, 0.28f);
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = true;

        scroll = panel.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.inertia = true;
        scroll.scrollSensitivity = 24f;

        RectTransform viewport = EnsureViewport(panel.transform);
        Text text = EnsureText(viewport.transform);
        scroll.viewport = viewport;
        scroll.content = text.GetComponent<RectTransform>();
        return text;
    }

    private static void HidePanel(Transform parent)
    {
        Transform existing = parent?.Find(PanelName);
        if (existing == null) return;
        try { Object.Destroy(existing.gameObject); }
        catch
        {
            try { existing.gameObject.SetActive(false); }
            catch { }
        }
    }

    private static Transform FindSingleRoot(Transform parent)
    {
        if (parent == null) return null;
        Transform kept = null;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child == null || child.name != PanelName) continue;
            if (kept == null)
            {
                kept = child;
                continue;
            }

            try { Object.Destroy(child.gameObject); }
            catch { child.gameObject.SetActive(false); }
        }
        return kept;
    }

    private static RectTransform EnsureViewport(Transform panel)
    {
        Transform found = panel.Find("Viewport");
        GameObject viewport = found?.gameObject;
        if (viewport == null)
        {
            viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(panel, false);
        }

        RectTransform rect = viewport.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(8f, 8f);
        rect.offsetMax = new Vector2(-8f, -8f);
        Image image = viewport.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.01f);
        image.raycastTarget = true;
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        return rect;
    }

    private static Text EnsureText(Transform viewport)
    {
        Transform found = viewport.Find(TextName);
        Text text = found == null ? null : found.GetComponent<Text>();
        if (text == null)
        {
            GameObject textObject = new GameObject(TextName, typeof(RectTransform), typeof(Text), typeof(ContentSizeFitter));
            textObject.transform.SetParent(viewport, false);
            text = textObject.GetComponent<Text>();
        }

        ContentSizeFitter fitter = text.GetComponent<ContentSizeFitter>() ?? text.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;

        text.font = LocalizedTextManager.current_font ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.supportRichText = true;
        text.resizeTextForBestFit = false;
        text.fontSize = 9;
        text.lineSpacing = 0.9f;
        text.color = new Color(0.97f, 0.98f, 1f, 1f);
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

    private static Transform ResolvePanelParent(UnitWindow window)
    {
        if (window == null) return null;
        try
        {
            Transform found = window.transform.Find("Background");
            if (found != null) return found;
        }
        catch (System.Exception ex) { MclslDiagnostics.Error("actor-info-find-background", "定位角色窗口背景失败: " + ex.Message); }
        try
        {
            Transform found = window.transform.Find("background");
            if (found != null) return found;
        }
        catch (System.Exception ex) { MclslDiagnostics.Error("actor-info-find-background-lower", "定位角色窗口背景失败: " + ex.Message); }
        return window.transform;
    }

    private static bool ShouldShowFor(Actor actor)
    {
        if (actor?.data == null) return false;
        try { if (!actor.isAlive()) return false; } catch { return false; }
        if (IsWorldSoulEntity(actor)) return false;
        if (!MclslEligibility.CanCultivate(actor)) return false;
        return HasCultivationSignal(actor);
    }

    private static bool HasCultivationSignal(Actor actor)
    {
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.Realm(actor))) return true;
        if (MclslSpiritualRootSystem.HasCultivationPotential(actor)) return true;
        return HasTrait(actor, MclslTraitRegistration.HuanzhenTraitId);
    }

    private static bool HasTrait(Actor actor, string traitId)
    {
        if (actor == null || string.IsNullOrWhiteSpace(traitId)) return false;
        try { return actor.hasTrait(traitId); }
        catch { return false; }
    }

    private static bool IsWorldSoulEntity(Actor actor)
    {
        try { return actor?.data != null && !string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulEntityId, string.Empty)); }
        catch (System.Exception ex)
        {
            MclslDiagnostics.Error("actor-info-world-soul-check", "判断天地之魄信息框隐藏状态失败: " + ex.Message);
            return false;
        }
    }

    private sealed class PanelState : MonoBehaviour
    {
        internal long ActorId = -1L;
        internal bool Initialized;
    }
}
