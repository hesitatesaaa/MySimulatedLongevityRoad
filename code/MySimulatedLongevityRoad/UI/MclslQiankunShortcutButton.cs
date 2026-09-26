using System.Collections.Generic;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslQiankunShortcutButton
{
    private const string Name = "MclslQiankunShortcutButton";
    private static readonly Dictionary<UnitWindow, Button> Buttons = new();

    internal static void Refresh(UnitWindow window)
    {
        if (window?.actor?.data == null) { Hide(window); return; }
        Transform parent = window.transform.Find("Background");
        if (parent == null) return;
        if (!Buttons.TryGetValue(window, out Button button) || button == null)
        {
            Transform existing = parent.Find(Name);
            GameObject root = existing == null ? new GameObject(Name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(TipButton)) : existing.gameObject;
            root.transform.SetParent(parent, false);
            button = root.GetComponent<Button>() ?? root.AddComponent<Button>();
            Buttons[window] = button;
        }
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(30f, 30f);
        rect.localPosition = new Vector3(-225f, 105f, 0f);
        rect.localScale = Vector3.one;
        Image image = button.GetComponent<Image>();
        image.sprite = SpriteTextureLoader.getSprite("ui/Icons/QiankunBagEntrance")
            ?? SpriteTextureLoader.getSprite("ui/Icons/MaobaoEntrance");
        image.color = Color.white;
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => MclslFeatureWindow.ShowBag(window.actor));
        TipButton tip = button.GetComponent<TipButton>();
        tip.textOnClick = "乾坤袋";
        tip.textOnClickDescription = "按类别查看当前修士的灵材、丹药、符箓与法宝。";
        button.gameObject.SetActive(true);
    }

    internal static void Hide(UnitWindow window)
    {
        if (window == null) return;
        if (Buttons.TryGetValue(window, out Button button) && button != null) button.gameObject.SetActive(false);
    }
}
