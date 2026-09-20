using System;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 人物窗口右侧的猫宝快捷登名按钮。
///
/// 该按钮只占用玄鉴仙族登名石使用的固定快捷位，不移动或替换现有猫宝入口。
/// 根节点只使用 Image/Button/TipButton，避免复制原版拖拽、标签或动画运行时组件。
/// </summary>
internal static class MclslMaobaoShortcutButton
{
    private const string ButtonName = "MclslMaobaoInscriptionShortcut";
    private const string IconName = "Icon";
    private const string IconPath = "ui/Icons/MaobaoEntrance";
    private static readonly Vector3 ShortcutPosition = new(116.8f, -112f, 0f);

    internal static void Refresh(UnitWindow window)
    {
        Actor actor = window?.actor;
        if (actor?.data == null)
        {
            Hide(window);
            return;
        }

        Transform favoriteVisual = ResolveFavoriteVisual(window);
        Transform parent = favoriteVisual?.parent;
        if (parent == null) return;

        GameObject buttonObject = FindOrCreate(parent, favoriteVisual.gameObject);
        if (buttonObject == null) return;

        ConfigureButton(buttonObject, window, actor, favoriteVisual.gameObject);
        buttonObject.SetActive(true);
    }

    internal static void Hide(UnitWindow window)
    {
        Transform favoriteVisual = ResolveFavoriteVisual(window);
        Transform parent = favoriteVisual?.parent;
        Transform existing = parent?.Find(ButtonName);
        if (existing != null) existing.gameObject.SetActive(false);
    }

    private static Transform ResolveFavoriteVisual(UnitWindow window)
    {
        try { return window?._icon_favorite?.transform?.parent; }
        catch { return null; }
    }

    private static GameObject FindOrCreate(Transform parent, GameObject template)
    {
        if (parent == null) return null;

        Transform existing = parent.Find(ButtonName);
        GameObject buttonObject = existing?.gameObject;
        if (buttonObject == null)
        {
            buttonObject = new GameObject(ButtonName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(TipButton));
            buttonObject.SetActive(false);
            buttonObject.transform.SetParent(parent, false);
            CopyTemplateVisual(template, buttonObject);
        }

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localPosition = ShortcutPosition;
            rect.localScale = Vector3.one;
        }

        Image image = buttonObject.GetComponent<Image>();
        Button button = buttonObject.GetComponent<Button>();
        if (button != null && image != null) button.targetGraphic = image;
        return buttonObject;
    }

    private static void CopyTemplateVisual(GameObject template, GameObject target)
    {
        RectTransform sourceRect = template?.GetComponent<RectTransform>();
        RectTransform targetRect = target?.GetComponent<RectTransform>();
        if (targetRect != null)
        {
            if (sourceRect != null)
            {
                targetRect.anchorMin = sourceRect.anchorMin;
                targetRect.anchorMax = sourceRect.anchorMax;
                targetRect.pivot = sourceRect.pivot;
                targetRect.sizeDelta = sourceRect.sizeDelta;
                targetRect.anchoredPosition = sourceRect.anchoredPosition;
            }
            else
            {
                targetRect.sizeDelta = new Vector2(28f, 28f);
            }
        }

        Image sourceImage = template?.GetComponent<Image>();
        Image targetImage = target?.GetComponent<Image>();
        if (targetImage != null && sourceImage != null)
        {
            targetImage.sprite = sourceImage.sprite;
            targetImage.overrideSprite = sourceImage.overrideSprite;
            targetImage.type = sourceImage.type;
            targetImage.preserveAspect = sourceImage.preserveAspect;
            targetImage.fillCenter = sourceImage.fillCenter;
            targetImage.color = sourceImage.color;
        }

        Button sourceButton = template?.GetComponent<Button>();
        Button targetButton = target?.GetComponent<Button>();
        if (targetButton != null)
        {
            targetButton.transition = Selectable.Transition.ColorTint;
            if (sourceButton != null) targetButton.colors = sourceButton.colors;
            targetButton.navigation = new Navigation { mode = Navigation.Mode.None };
        }

        Transform sourceIcon = template?.transform.Find(IconName);
        if (sourceIcon != null && target?.transform.Find(IconName) == null)
        {
            GameObject iconObject = new(IconName, typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(target.transform, false);
            CopyIconRect(sourceIcon.GetComponent<RectTransform>(), iconObject.GetComponent<RectTransform>());
            Image sourceIconImage = sourceIcon.GetComponent<Image>();
            Image iconImage = iconObject.GetComponent<Image>();
            if (sourceIconImage != null && iconImage != null)
            {
                iconImage.type = sourceIconImage.type;
                iconImage.preserveAspect = sourceIconImage.preserveAspect;
                iconImage.color = Color.white;
                iconImage.raycastTarget = false;
            }
        }
    }

    private static void CopyIconRect(RectTransform source, RectTransform target)
    {
        if (target == null) return;
        if (source == null)
        {
            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.one;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
            return;
        }

        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.sizeDelta = source.sizeDelta;
        target.anchoredPosition = source.anchoredPosition;
        target.localScale = Vector3.one;
    }

    private static void ConfigureButton(GameObject buttonObject, UnitWindow window, Actor actor, GameObject template)
    {
        Image rootImage = buttonObject.GetComponent<Image>();
        Button button = buttonObject.GetComponent<Button>();
        if (button == null) return;

        Sprite sprite = null;
        try { sprite = SpriteTextureLoader.getSprite(IconPath); }
        catch (Exception ex) { MclslDiagnostics.Error("maobao-shortcut-icon", "加载猫宝快捷按钮图标失败: " + ex.Message); }

        Image icon = buttonObject.transform.Find(IconName)?.GetComponent<Image>();
        if (icon != null)
        {
            icon.sprite = sprite;
            icon.overrideSprite = sprite;
            icon.enabled = sprite != null;
            icon.color = Color.white;
        }
        else if (rootImage != null)
        {
            rootImage.sprite = sprite;
            rootImage.overrideSprite = sprite;
            rootImage.enabled = sprite != null;
            rootImage.color = Color.white;
        }

        button.targetGraphic = rootImage ?? icon;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ToggleCurrentActor(window));

        TipButton tip = buttonObject.GetComponent<TipButton>() ?? buttonObject.AddComponent<TipButton>();
        bool recorded = MclslMaobaoSystem.IsRecorded(MclslActorAccessor.Id(actor));
        tip.textOnClick = recorded ? "移除猫宝登名" : "猫宝登名";
        tip.textOnClickDescription = recorded
            ? "再次点击移除当前人物的猫宝留名。"
            : "点击立即将当前人物刻入猫宝。";

        if (rootImage != null)
            rootImage.color = recorded ? MclslUiTheme.RankHighlight : Color.white;
    }

    private static void ToggleCurrentActor(UnitWindow window)
    {
        Actor actor = window?.actor;
        if (actor?.data == null) return;

        MclslMaobaoSystem.ToggleRecordActor(actor, out string message);
        if (!string.IsNullOrWhiteSpace(message))
        {
            try { WorldTip.showNowTop(message); }
            catch { }
        }

        Refresh(window);
    }
}
