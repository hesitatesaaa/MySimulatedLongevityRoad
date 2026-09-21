using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 猫宝的人物窗口快捷入口。
///
/// 入口结构和鬼谷“时光长河”完全一致：按钮直接挂在 UnitWindow/Background，
/// 使用固定的 -190,105 位置，不参与人物信息栏的布局树，也不会被信息栏刷新带走。
/// 唯一替换的是按钮图标和猫宝自己的存取逻辑。
/// </summary>
internal static class MclslMaobaoShortcutButton
{
    private const string ButtonName = "MclslMaobaoShortcutButton";
    private const string IconName = "Icon";
    private const string IconPath = "ui/Icons/MaobaoEntrance";
    private static readonly Vector3 ShortcutPosition = new(-190f, 105f, 0f);
    private static readonly Color ShortcutSurface = new(0.035f, 0.13f, 0.15f, 0.58f);
    private static readonly Color ShortcutEdge = new(0.54f, 0.86f, 0.78f, 0.48f);

    private static UnitWindow _currentWindow;
    private static readonly Dictionary<UnitWindow, Button> WindowButtons = new();
    private static readonly Dictionary<UnitWindow, Image> WindowIcons = new();

    internal static void Refresh(UnitWindow window) => ShowButton(window);

    internal static void ShowButton(UnitWindow window)
    {
        if (window?.actor?.data == null)
        {
            Hide(window);
            return;
        }

        _currentWindow = window;
        Transform background = window.transform.Find("Background");
        if (background == null)
        {
            Hide(window);
            return;
        }

        Button button = ResolveButton(window, background);
        if (button == null) return;
        WindowButtons[window] = button;
        Image icon = ResolveIcon(window, button);
        ConfigureButton(button, icon, window.actor);

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localPosition = ShortcutPosition;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }
        button.gameObject.SetActive(true);
    }

    internal static void Hide(UnitWindow window)
    {
        if (window == null) return;
        try
        {
            Transform[] all = window.transform.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform child = all[i];
                if (child != null && child.name == ButtonName) child.gameObject.SetActive(false);
            }
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("maobao-shortcut-hide", "隐藏猫宝快捷按钮失败: " + ex.Message);
        }
    }

    private static Button ResolveButton(UnitWindow window, Transform background)
    {
        if (WindowButtons.TryGetValue(window, out Button cached) && cached != null)
        {
            if (cached.transform.parent != background) cached.transform.SetParent(background, false);
            return cached;
        }

        Transform selected = null;
        Transform[] all = window.transform.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            Transform candidate = all[i];
            if (candidate == null || candidate.name != ButtonName) continue;
            if (selected == null || candidate.parent == background)
            {
                if (selected != null && selected != candidate) selected.gameObject.SetActive(false);
                selected = candidate;
                if (candidate.parent == background) break;
            }
            else candidate.gameObject.SetActive(false);
        }

        GameObject buttonObject = selected?.gameObject;
        if (buttonObject == null) buttonObject = CreateButton(background);
        else if (buttonObject.transform.parent != background) buttonObject.transform.SetParent(background, false);

        Button button = buttonObject.GetComponent<Button>() ?? buttonObject.AddComponent<Button>();
        Image image = buttonObject.GetComponent<Image>() ?? buttonObject.AddComponent<Image>();
        button.targetGraphic = image;
        return button;
    }

    private static GameObject CreateButton(Transform background)
    {
        GameObject buttonObject = new(ButtonName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(TipButton));
        buttonObject.transform.SetParent(background, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(30f, 30f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.localPosition = ShortcutPosition;
        rect.localScale = Vector3.one;

        Image backgroundImage = buttonObject.GetComponent<Image>();
        Sprite frame = SpriteTextureLoader.getSprite("ui/Ranking/Immortal_Ranking_Components");
        backgroundImage.sprite = frame;
        backgroundImage.overrideSprite = frame;
        backgroundImage.type = frame == null ? Image.Type.Simple : Image.Type.Sliced;
        backgroundImage.color = ShortcutSurface;
        backgroundImage.raycastTarget = true;
        ConfigureSurfaceOutline(buttonObject);

        GameObject iconObject = new(IconName, typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(buttonObject.transform, false);
        iconObject.transform.localPosition = Vector3.zero;
        iconObject.transform.localScale = Vector3.one;
        iconObject.GetComponent<RectTransform>().sizeDelta = new Vector2(28f, 28f);
        iconObject.GetComponent<Image>().raycastTarget = false;
        return buttonObject;
    }

    private static Image ResolveIcon(UnitWindow window, Button button)
    {
        Image icon = button.transform.Find(IconName)?.GetComponent<Image>();
        if (icon != null)
        {
            WindowIcons[window] = icon;
            return icon;
        }

        GameObject iconObject = new(IconName, typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(button.transform, false);
        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        icon = iconObject.GetComponent<Image>();
        icon.raycastTarget = false;
        WindowIcons[window] = icon;
        return icon;
    }

    private static void ConfigureButton(Button button, Image icon, Actor actor)
    {
        if (button == null || actor?.data == null) return;
        Image rootImage = button.GetComponent<Image>();
        Sprite catSprite = null;
        try { catSprite = SpriteTextureLoader.getSprite(IconPath) ?? SpriteTextureLoader.getSprite(IconPath + ".png"); }
        catch (Exception ex) { MclslDiagnostics.Error("maobao-shortcut-icon", "加载猫宝图标失败: " + ex.Message); }

        if (icon != null)
        {
            icon.sprite = catSprite;
            icon.overrideSprite = catSprite;
            icon.enabled = catSprite != null;
            icon.color = Color.white;
        }
        if (rootImage != null)
        {
            Sprite frame = null;
            try { frame = SpriteTextureLoader.getSprite("ui/Ranking/Immortal_Ranking_Components"); }
            catch (Exception ex) { MclslDiagnostics.Error("maobao-shortcut-frame", "加载猫宝按钮背景失败: " + ex.Message); }
            rootImage.sprite = frame;
            rootImage.overrideSprite = frame;
            rootImage.type = frame == null ? Image.Type.Simple : Image.Type.Sliced;
            rootImage.color = ShortcutSurface;
            rootImage.raycastTarget = true;
            rootImage.enabled = true;
            ConfigureSurfaceOutline(button.gameObject);
        }

        // Do not let Button's default ColorTint transition replace the carefully
        // chosen translucent surface color with an opaque white tint.
        button.transition = Selectable.Transition.None;
        button.targetGraphic = rootImage ?? icon;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ToggleCurrentActor(_currentWindow));

        TipButton tip = button.GetComponent<TipButton>() ?? button.gameObject.AddComponent<TipButton>();
        bool saved = MclslMaobaoSystem.IsRecorded(MclslActorAccessor.Id(actor));
        tip.textOnClick = saved ? "移除猫宝登名" : "猫宝登名";
        tip.textOnClickDescription = saved ? "再次点击移除当前人物的猫宝留名。" : "点击将当前人物刻入猫宝。";
    }

    private static void ConfigureSurfaceOutline(GameObject buttonObject)
    {
        if (buttonObject == null) return;
        Outline outline = buttonObject.GetComponent<Outline>() ?? buttonObject.AddComponent<Outline>();
        outline.effectColor = ShortcutEdge;
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = true;
    }

    private static void ToggleCurrentActor(UnitWindow window)
    {
        Actor actor = window?.actor;
        if (actor?.data == null) return;

        MclslMaobaoSystem.ToggleRecordActor(actor, out string message);
        if (!string.IsNullOrWhiteSpace(message))
        {
            try { WorldTip.showNowTop(message); } catch { }
        }
        ShowButton(window);
    }
}
