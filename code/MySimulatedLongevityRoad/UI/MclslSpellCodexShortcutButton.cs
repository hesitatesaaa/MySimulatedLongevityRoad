using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

/// <summary>法术录入口与猫宝快捷按钮同挂在人物窗口背景上。</summary>
internal static class MclslSpellCodexShortcutButton
{
    private const string ButtonName = "MclslSpellCodexShortcutButton";
    private const string IconPath = "ui/Icons/SpellCodexEntrance";
    private static readonly Vector3 ShortcutPosition = new(-226f, 105f, 0f);
    private static readonly Color ShortcutSurface = new(0.035f, 0.13f, 0.15f, 0.58f);
    private static readonly Color ShortcutEdge = new(0.54f, 0.86f, 0.78f, 0.48f);
    private static readonly Dictionary<UnitWindow, Button> WindowButtons = new();

    internal static void ClearRuntime()
    {
        WindowButtons.Clear();
        MclslSpellCodexWindow.ClearTarget(null);
    }

    internal static void Refresh(UnitWindow window)
    {
        if (window?.actor?.data == null || !window.actor.isAlive())
        {
            Hide(window);
            return;
        }
        Transform background = window.transform.Find("Background");
        if (background == null)
        {
            Hide(window);
            return;
        }
        Button button = ResolveButton(window, background);
        if (button == null) return;
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.localPosition = ShortcutPosition;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        ConfigureButton(button, window);
        button.gameObject.SetActive(true);
        MclslSpellCodexWindow.RefreshTarget(window);
    }

    internal static void Hide(UnitWindow window)
    {
        if (window == null) return;
        if (WindowButtons.TryGetValue(window, out Button button) && button != null)
            button.gameObject.SetActive(false);
        MclslSpellCodexWindow.ClearTarget(window);
    }

    private static Button ResolveButton(UnitWindow window, Transform background)
    {
        if (WindowButtons.TryGetValue(window, out Button cached) && cached != null)
        {
            if (cached.transform.parent != background) cached.transform.SetParent(background, false);
            return cached;
        }
        GameObject root = background.Find(ButtonName)?.gameObject;
        if (root == null)
        {
            root = new GameObject(ButtonName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(TipButton));
            root.transform.SetParent(background, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(30f, 30f);
            rect.pivot = rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            GameObject icon = new("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(root.transform, false);
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
            icon.GetComponent<Image>().raycastTarget = false;
        }
        Button button = root.GetComponent<Button>() ?? root.AddComponent<Button>();
        WindowButtons[window] = button;
        return button;
    }

    private static void ConfigureButton(Button button, UnitWindow window)
    {
        Image surface = button.GetComponent<Image>() ?? button.gameObject.AddComponent<Image>();
        Sprite frame = null;
        try { frame = SpriteTextureLoader.getSprite("ui/Ranking/Immortal_Ranking_Components"); }
        catch (Exception ex) { MclslDiagnostics.Error("spell-codex-shortcut-frame", ex.Message); }
        surface.sprite = surface.overrideSprite = frame;
        surface.type = frame == null ? Image.Type.Simple : Image.Type.Sliced;
        surface.color = ShortcutSurface;
        surface.raycastTarget = true;
        Outline outline = button.GetComponent<Outline>() ?? button.gameObject.AddComponent<Outline>();
        outline.effectColor = ShortcutEdge;
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = true;
        Image icon = button.transform.Find("Icon")?.GetComponent<Image>();
        if (icon != null)
        {
            Sprite sprite = null;
            try { sprite = SpriteTextureLoader.getSprite(IconPath) ?? SpriteTextureLoader.getSprite(IconPath + ".png"); }
            catch (Exception ex) { MclslDiagnostics.Error("spell-codex-shortcut-icon", ex.Message); }
            icon.sprite = icon.overrideSprite = sprite;
            icon.enabled = sprite != null;
            icon.color = Color.white;
        }
        button.transition = Selectable.Transition.None;
        button.targetGraphic = surface;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => MclslSpellCodexWindow.ShowForWindow(window));
        TipButton tip = button.GetComponent<TipButton>() ?? button.gameObject.AddComponent<TipButton>();
        tip.textOnClick = MclslLocalizationBridge.RuntimeText("法术录");
        tip.textOnClickDescription = MclslLocalizationBridge.RuntimeText("打开当前人物的法术录，点击法术即可学会。");
    }
}
