using UnityEngine;
using UnityEngine.UI;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 角色右侧资料栏。标题和底部操作栏固定，只有正文滚动。
/// 同一套布局同时承载两个纪元的字段，数据仍由 formatter 提供。
/// </summary>
internal static class MclslActorInfoPanel
{
    private const string PanelName = "MclslActorInfoPanel";
    private const string HeaderName = "MclslActorInfoHeader";
    private const string HeaderDividerName = "MclslActorInfoHeaderDivider";
    private const string BodyName = "MclslActorInfoBody";
    private const string ViewportName = "Viewport";
    private const string TextName = "MclslActorInfoText";
    private const string ScrollbarName = "MclslActorInfoScrollbar";
    private const int ActiveWindowRefreshIntervalFrames = 180;
    private static int _lastActiveWindowRefreshFrame = -9999;

    internal static void Refresh(UnitWindow window, bool resetScrollForNewActor = false, bool forceContentRefresh = false)
    {
        if (window == null) return;
        Transform background = ResolvePanelParent(window);
        if (background == null)
        {
            MclslMaobaoShortcutButton.Hide(window);
            return;
        }

        CleanupLegacyPanel(background);
        if (window.actor == null || !window.actor.isAlive())
        {
            MclslMaobaoShortcutButton.Hide(window);
            HidePanel(background);
            return;
        }

        MclslMaobaoShortcutButton.Refresh(window);
        if (!ShouldShowFor(window.actor) && !MclslRuntimeSettings.DebugToolsVisible)
        {
            HidePanel(background);
            return;
        }

        long actorId = MclslActorAccessor.Id(window.actor);
        Text text = EnsurePanel(background, out ScrollRect scroll, out PanelState state, out Text header);
        if (text == null) return;
        Transform panel = text.transform.parent?.parent?.parent;
        EnsureActionBar(panel ?? background, window.actor);
        MclslMaobaoShortcutButton.Refresh(window);
        if (scroll != null) scroll.gameObject.SetActive(true);
        bool actorChanged = state != null && state.ActorId != actorId;

        string formatted = MclslActorInfoFormatter.Format(window.actor);
        if (string.IsNullOrWhiteSpace(formatted))
        {
            if (!MclslRuntimeSettings.DebugToolsVisible)
            {
                HidePanel(background);
                return;
            }
            formatted = "<b>开发者目标</b>\n" + MclslActorAccessor.DisplayName(window.actor) + "\nID " + MclslActorAccessor.Id(window.actor);
        }
        SplitDocument(formatted, out string headerText, out string bodyText);
        if (header != null) header.text = headerText;

        // Only OnEnable/new actor starts a new browsing session. Data refreshes
        // update text in place and retain the position recorded by PanelState.
        bool shouldReset = scroll != null
            && (resetScrollForNewActor || actorChanged || state is { Initialized: false });
        float keepScroll = scroll == null
            ? 1f
            : state is { HasUserScrollPosition: true } ? state.LastNormalizedPosition : scroll.verticalNormalizedPosition;
        Vector2 keepContentPosition = scroll?.content == null
            ? Vector2.zero
            : state is { HasUserScrollPosition: true } ? state.LastContentPosition : scroll.content.anchoredPosition;
        if (state != null)
        {
            state.ActorId = actorId;
            state.Initialized = true;
        }

        bool textChanged = !string.Equals(text.text, bodyText, System.StringComparison.Ordinal);
        if (textChanged)
        {
            text.text = bodyText;
            if (scroll?.content != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(scroll.content);
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            }
        }
        if (scroll == null) return;
        if (shouldReset)
        {
            scroll.StopMovement();
            if (state != null)
            {
                state.HasUserScrollPosition = true;
                state.LastNormalizedPosition = 1f;
                state.LastContentPosition = Vector2.zero;
                state.PendingScrollRestore = false;
                state.PendingScrollRestoreFrames = 0;
            }
            scroll.verticalNormalizedPosition = 1f;
        }
        else if (textChanged)
        {
            if (state != null)
            {
                state.PendingNormalizedPosition = keepScroll;
                state.PendingContentPosition = keepContentPosition;
                state.PendingScrollRestore = true;
                state.PendingScrollRestoreFrames = PanelState.ScrollRestoreFrameCount;
            }
            RestoreScroll(scroll, keepScroll, keepContentPosition);
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

    internal static void OnWindowClosed(UnitWindow window)
    {
        if (window == null) return;
        Transform background = ResolvePanelParent(window);
        if (background != null) HidePanel(background);
        MclslMaobaoShortcutButton.Hide(window);
    }

    private static Text EnsurePanel(Transform parent, out ScrollRect scroll, out PanelState state, out Text header)
    {
        Transform existing = FindSingleRoot(parent);
        GameObject panel = existing?.gameObject;
        if (panel == null)
        {
            panel = new GameObject(PanelName, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(ScrollRect));
            panel.transform.SetParent(parent, false);
        }
        panel.SetActive(true);
        if (existing == null) panel.transform.SetAsLastSibling();
        state = panel.GetComponent<PanelState>() ?? panel.AddComponent<PanelState>();

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(256f, 40f);
        rect.localScale = Vector3.one;
        rect.sizeDelta = new Vector2(172f, 332f);

        Image image = panel.GetComponent<Image>();
        image.color = MclslUiTheme.ActorPanelSurface;
        image.raycastTarget = false;
        Outline outline = panel.GetComponent<Outline>();
        outline.effectColor = MclslUiTheme.ActorPanelEdge;
        outline.effectDistance = new Vector2(1.5f, -1.5f);
        outline.useGraphicAlpha = true;
        Shadow panelShadow = panel.GetComponent<Shadow>() ?? panel.AddComponent<Shadow>();
        panelShadow.effectColor = new Color(0f, 0.02f, 0.025f, 0.62f);
        panelShadow.effectDistance = new Vector2(2f, -2f);
        panelShadow.useGraphicAlpha = true;

        // Keep the old component for scene migration, but the child body owns
        // scrolling so the header and footer never move with the content.
        ScrollRect legacyRootScroll = panel.GetComponent<ScrollRect>();
        legacyRootScroll.enabled = false;

        EnsurePanelAccent(panel.transform);
        header = EnsureHeader(panel.transform);
        // A panel created by 0.1.9 may still contain a direct Viewport child.
        // Hide it once so the migrated body is the only visible content tree.
        Transform legacyViewport = panel.transform.Find(ViewportName);
        if (legacyViewport != null) legacyViewport.gameObject.SetActive(false);
        Transform body = EnsureBody(panel.transform);
        RectTransform viewport = EnsureViewport(body);
        Text text = EnsureText(viewport.transform);
        scroll = body.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.inertia = true;
        scroll.decelerationRate = 0.12f;
        scroll.scrollSensitivity = 20f;
        scroll.viewport = viewport;
        scroll.content = text.GetComponent<RectTransform>();
        EnsureScrollbar(body, scroll);
        state.BindScroll(scroll);
        return text;
    }

    private static void HidePanel(Transform parent)
    {
        if (parent == null) return;
        Transform existing = parent.Find(PanelName);
        if (existing != null)
        {
            try { UnityEngine.Object.Destroy(existing.gameObject); }
            catch
            {
                try { existing.gameObject.SetActive(false); } catch { }
            }
        }
        Transform actions = parent.Find("MclslActorActions");
        if (actions != null)
        {
            try { UnityEngine.Object.Destroy(actions.gameObject); }
            catch { actions.gameObject.SetActive(false); }
        }
    }

    private static void CleanupLegacyPanel(Transform parent)
    {
        Transform legacy = parent?.Find("XuanJianInfoPanel");
        if (legacy == null || legacy.GetComponent<PanelState>() == null) return;
        try { UnityEngine.Object.Destroy(legacy.gameObject); }
        catch { try { legacy.gameObject.SetActive(false); } catch { } }
    }

    private static void EnsureActionBar(Transform panel, Actor actor)
    {
        if (panel == null || actor?.data == null) return;
        Transform existing = panel.Find("MclslActorActions");
        GameObject bar = existing?.gameObject;
        if (bar == null)
        {
            bar = new GameObject("MclslActorActions", typeof(RectTransform), typeof(Image), typeof(Outline));
            bar.transform.SetParent(panel, false);
        }
        bar.SetActive(true);
        bar.transform.SetAsLastSibling();
        RectTransform barRect = bar.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(1f, 0f);
        barRect.pivot = new Vector2(0.5f, 0f);
        barRect.offsetMin = new Vector2(7f, 5f);
        barRect.offsetMax = new Vector2(-7f, 35f);
        barRect.localScale = Vector3.one;
        Image barImage = bar.GetComponent<Image>();
        barImage.color = new Color(0.035f, 0.16f, 0.16f, 0.82f);
        barImage.raycastTarget = false;
        Outline barOutline = bar.GetComponent<Outline>();
        barOutline.effectColor = MclslUiTheme.ActorPanelEdge;
        barOutline.effectDistance = new Vector2(1f, -1f);
        barOutline.useGraphicAlpha = true;
        EnsureActionButton(bar.transform, "Biography", "修士列传", () => MclslCodexWindow.ShowBiographyForActor(actor));
        RemoveActionButton(bar.transform, "Maobao");
        RemoveActionButton(bar.transform, "Debug");
    }

    private static void EnsureActionButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction action)
    {
        Transform existing = parent.Find(name);
        GameObject buttonObject = existing?.gameObject;
        if (buttonObject == null)
        {
            buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            buttonObject.transform.SetParent(parent, false);
            Text text = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(buttonObject.transform, false);
            text.font = LocalizedTextManager.current_font ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 11;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = MclslUiTheme.ActorPanelText;
            text.raycastTarget = false;
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(3f, 2f);
        rect.offsetMax = new Vector2(-3f, -2f);
        rect.localPosition = Vector3.zero;
        rect.localScale = Vector3.one;
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.07f, 0.28f, 0.27f, 0.92f);
        image.raycastTarget = true;
        Outline buttonOutline = buttonObject.GetComponent<Outline>();
        buttonOutline.effectColor = MclslUiTheme.ActorPanelAccent;
        buttonOutline.effectDistance = new Vector2(1f, -1f);
        buttonOutline.useGraphicAlpha = true;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.72f, 1f, 0.94f, 1f);
        colors.pressedColor = new Color(0.55f, 0.88f, 0.80f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        Text labelText = buttonObject.transform.Find("Text")?.GetComponent<Text>();
        if (labelText != null) labelText.text = label;
    }

    private static void RemoveActionButton(Transform parent, string name)
    {
        Transform existing = parent?.Find(name);
        if (existing == null) return;
        try { UnityEngine.Object.Destroy(existing.gameObject); }
        catch { existing.gameObject.SetActive(false); }
    }

    private static Transform EnsureBody(Transform panel)
    {
        Transform found = panel.Find(BodyName);
        GameObject body = found?.gameObject;
        if (body == null)
        {
            body = new GameObject(BodyName, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            body.transform.SetParent(panel, false);
        }
        body.SetActive(true);
        RectTransform rect = body.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(7f, 40f);
        rect.offsetMax = new Vector2(-7f, -39f);
        rect.localScale = Vector3.one;
        Image image = body.GetComponent<Image>();
        image.color = new Color(0.01f, 0.04f, 0.045f, 0.14f);
        image.raycastTarget = true;
        return body.transform;
    }

    private static RectTransform EnsureViewport(Transform body)
    {
        Transform found = body.Find(ViewportName);
        GameObject viewport = found?.gameObject;
        if (viewport == null)
        {
            viewport = new GameObject(ViewportName, typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(body, false);
        }
        RectTransform rect = viewport.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(4f, 5f);
        rect.offsetMax = new Vector2(-11f, -5f);
        Image image = viewport.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.01f);
        image.raycastTarget = true;
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        return rect;
    }

    private static void EnsureScrollbar(Transform body, ScrollRect scroll)
    {
        if (body == null || scroll == null) return;
        Transform found = body.Find(ScrollbarName);
        GameObject scrollbarObject = found?.gameObject;
        if (scrollbarObject == null)
        {
            scrollbarObject = new GameObject(ScrollbarName, typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            scrollbarObject.transform.SetParent(body, false);
            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(scrollbarObject.transform, false);
        }
        RectTransform rect = scrollbarObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.offsetMin = new Vector2(-6f, 5f);
        rect.offsetMax = new Vector2(-2f, -5f);
        Image track = scrollbarObject.GetComponent<Image>();
        track.color = new Color(0.05f, 0.17f, 0.18f, 0.28f);
        track.raycastTarget = true;
        Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.numberOfSteps = 0;
        Transform handleTransform = scrollbarObject.transform.Find("Handle");
        if (handleTransform != null)
        {
            RectTransform handleRect = handleTransform.GetComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;
            Image handleImage = handleTransform.GetComponent<Image>();
            handleImage.color = new Color(0.48f, 0.86f, 0.78f, 0.76f);
            handleImage.raycastTarget = true;
            scrollbar.handleRect = handleRect;
        }
        scrollbar.targetGraphic = scrollbarObject.GetComponent<Image>();
        scroll.verticalScrollbar = scrollbar;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        scroll.verticalScrollbarSpacing = 1f;
    }

    private static void EnsurePanelAccent(Transform panel)
    {
        if (panel == null) return;
        Transform found = panel.Find("Accent");
        GameObject accent = found?.gameObject;
        if (accent == null)
        {
            accent = new GameObject("Accent", typeof(RectTransform), typeof(Image));
            accent.transform.SetParent(panel, false);
        }
        RectTransform rect = accent.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 3f);
        Image image = accent.GetComponent<Image>();
        image.color = MclslUiTheme.ActorPanelAccent;
        image.raycastTarget = false;
        accent.transform.SetAsFirstSibling();
    }

    private static Text EnsureHeader(Transform panel)
    {
        Transform found = panel.Find(HeaderName);
        Text header = found?.GetComponent<Text>();
        if (header == null)
        {
            header = new GameObject(HeaderName, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            header.transform.SetParent(panel, false);
        }
        RectTransform rect = header.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(10f, -35f);
        rect.offsetMax = new Vector2(-10f, -7f);
        header.font = LocalizedTextManager.current_font ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        header.fontSize = 12;
        header.alignment = TextAnchor.MiddleLeft;
        header.supportRichText = true;
        header.color = MclslUiTheme.ActorPanelText;
        header.raycastTarget = false;
        header.transform.SetAsLastSibling();

        Transform dividerFound = panel.Find(HeaderDividerName);
        GameObject divider = dividerFound?.gameObject;
        if (divider == null)
        {
            divider = new GameObject(HeaderDividerName, typeof(RectTransform), typeof(Image));
            divider.transform.SetParent(panel, false);
        }
        RectTransform dividerRect = divider.GetComponent<RectTransform>();
        dividerRect.anchorMin = new Vector2(0f, 1f);
        dividerRect.anchorMax = new Vector2(1f, 1f);
        dividerRect.pivot = new Vector2(0.5f, 1f);
        dividerRect.offsetMin = new Vector2(8f, -39f);
        dividerRect.offsetMax = new Vector2(-8f, -38f);
        Image dividerImage = divider.GetComponent<Image>();
        dividerImage.color = new Color(MclslUiTheme.ActorPanelEdge.r, MclslUiTheme.ActorPanelEdge.g, MclslUiTheme.ActorPanelEdge.b, 0.72f);
        dividerImage.raycastTarget = false;
        divider.transform.SetAsLastSibling();
        header.transform.SetAsLastSibling();
        return header;
    }

    private static Text EnsureText(Transform viewport)
    {
        Transform found = viewport.Find(TextName);
        Text text = found == null ? null : found.GetComponent<Text>();
        bool created = false;
        if (text == null)
        {
            GameObject textObject = new GameObject(TextName, typeof(RectTransform), typeof(Text), typeof(ContentSizeFitter));
            textObject.transform.SetParent(viewport, false);
            text = textObject.GetComponent<Text>();
            created = true;
        }
        ContentSizeFitter fitter = text.GetComponent<ContentSizeFitter>() ?? text.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        RectTransform rect = text.GetComponent<RectTransform>();
        // The content position belongs to the current browsing session. Do not
        // initialize it during data refreshes, or the panel jumps to the top.
        if (created)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }
        text.font = LocalizedTextManager.current_font ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.supportRichText = true;
        text.resizeTextForBestFit = false;
        text.fontSize = 10;
        text.lineSpacing = 0.95f;
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

    private static void SplitDocument(string formatted, out string header, out string body)
    {
        int newline = formatted.IndexOf('\n');
        if (newline < 0)
        {
            header = formatted.Trim();
            body = string.Empty;
            return;
        }
        header = formatted.Substring(0, newline).Trim();
        body = formatted.Substring(newline + 1).TrimStart('\r', '\n');
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

    private static void RestoreScroll(ScrollRect scroll, float normalizedPosition, Vector2 contentPosition)
    {
        if (scroll == null) return;
        scroll.StopMovement();
        if (scroll.content != null) scroll.content.anchoredPosition = contentPosition;
        scroll.verticalNormalizedPosition = Mathf.Clamp01(normalizedPosition);
        scroll.velocity = Vector2.zero;
    }

    private static Transform FindSingleRoot(Transform parent)
    {
        if (parent == null) return null;
        Transform kept = null;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child == null || child.name != PanelName) continue;
            if (kept == null) { kept = child; continue; }
            try { UnityEngine.Object.Destroy(child.gameObject); }
            catch { child.gameObject.SetActive(false); }
        }
        return kept;
    }

    private sealed class PanelState : MonoBehaviour
    {
        internal const int ScrollRestoreFrameCount = 4;
        internal long ActorId = -1L;
        internal bool Initialized;
        internal bool HasUserScrollPosition;
        internal float LastNormalizedPosition = 1f;
        internal Vector2 LastContentPosition;
        internal bool PendingScrollRestore;
        internal float PendingNormalizedPosition = 1f;
        internal Vector2 PendingContentPosition;
        internal int PendingScrollRestoreFrames;
        private ScrollRect _boundScroll;
        private bool IsRestoring;

        internal void BindScroll(ScrollRect scroll)
        {
            if (scroll == null || _boundScroll == scroll) return;
            if (_boundScroll != null) _boundScroll.onValueChanged.RemoveListener(OnScrollChanged);
            _boundScroll = scroll;
            _boundScroll.onValueChanged.AddListener(OnScrollChanged);
            if (!HasUserScrollPosition)
            {
                HasUserScrollPosition = true;
                LastNormalizedPosition = scroll.verticalNormalizedPosition;
                LastContentPosition = scroll.content == null ? Vector2.zero : scroll.content.anchoredPosition;
            }
        }

        private void OnScrollChanged(Vector2 _)
        {
            if (_boundScroll == null || PendingScrollRestore || IsRestoring) return;
            LastNormalizedPosition = _boundScroll.verticalNormalizedPosition;
            LastContentPosition = _boundScroll.content == null ? Vector2.zero : _boundScroll.content.anchoredPosition;
        }

        private void LateUpdate()
        {
            if (!PendingScrollRestore) return;
            ScrollRect scroll = _boundScroll;
            if (scroll == null)
            {
                PendingScrollRestore = false;
                PendingScrollRestoreFrames = 0;
                return;
            }
            Canvas.ForceUpdateCanvases();
            if (scroll.content != null) LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            bool wasRestoring = IsRestoring;
            IsRestoring = true;
            RestoreScroll(scroll, PendingNormalizedPosition, PendingContentPosition);
            IsRestoring = wasRestoring;
            PendingScrollRestoreFrames--;
            if (PendingScrollRestoreFrames <= 0) PendingScrollRestore = false;
        }
    }
}
