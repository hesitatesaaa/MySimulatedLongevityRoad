using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

/// <summary>Native-style icon picker for the bag-only artifact rain pool.</summary>
internal static class MclslArtifactRainEditor
{
    private const string WindowId = "MclslArtifactRainEditor";
    private const string BackgroundSprite = "ui/special/backgroundKingdomElement";
    private const float CellSize = 40f;
    private const float CellGap = 4f;
    private const int Columns = 6;
    private static readonly MclslArtifactEquipmentSlot[] SlotOrder =
    {
        MclslArtifactEquipmentSlot.Weapon, MclslArtifactEquipmentSlot.Helmet,
        MclslArtifactEquipmentSlot.Armor, MclslArtifactEquipmentSlot.Boots,
        MclslArtifactEquipmentSlot.Ring, MclslArtifactEquipmentSlot.Amulet
    };
    private static readonly List<Cell> Cells = new();
    private static ScrollWindow _window;
    private static RectTransform _content;

    private sealed class Cell
    {
        internal MclslItemDefinition Item;
        internal Image Background;
        internal Image Icon;
    }

    internal static void ShowWindow()
    {
        if (_window == null) Init();
        if (_window == null) return;
        Refresh();
        MclslWindowOpenGuard.Show(_window, WindowId, false);
    }

    private static void Init()
    {
        MclslLocalizationBridge.RegisterKey("mclsl_artifact_rain_editor_title", "法宝雨编辑");
        _window = WindowCreator.CreateEmptyWindow(WindowId, "mclsl_artifact_rain_editor_title", "ui/Icons/ArtifactRain");
        Transform background = _window?.transform.Find("Background");
        if (background == null) return;
        background.GetComponent<RectTransform>().sizeDelta = new Vector2(320f, 440f);
        Transform nativeScroll = background.Find("Scroll View");
        if (nativeScroll != null) nativeScroll.gameObject.SetActive(false);
        if (_window.titleText != null)
        {
            RectTransform title = _window.titleText.GetComponent<RectTransform>();
            title.anchorMin = title.anchorMax = title.pivot = new Vector2(0.5f, 1f);
            title.anchoredPosition = new Vector2(0f, -7f);
            _window.titleText.alignment = TextAnchor.MiddleCenter;
        }
        CreateBarButton(background, "全选", -54f, () => { MclslArtifactRain.SelectAll(); Refresh(); });
        CreateBarButton(background, "清空", 54f, () => { MclslArtifactRain.ClearAll(); Refresh(); });
        CreateScroll(background);
        CreateCells();
        _window.gameObject.SetActive(false);
    }

    private static void CreateScroll(Transform parent)
    {
        GameObject scrollObject = new("ArtifactRainScroll", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollObject.transform.SetParent(parent, false);
        RectTransform scrollRect = (RectTransform)scrollObject.transform;
        scrollRect.anchorMin = scrollRect.anchorMax = scrollRect.pivot = new Vector2(0.5f, 0.5f);
        scrollRect.anchoredPosition = new Vector2(0f, -34f);
        scrollRect.sizeDelta = new Vector2(300f, 352f);
        scrollObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.3f);
        scrollObject.GetComponent<Mask>().showMaskGraphic = true;
        ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;
        GameObject viewportObject = new("Viewport", typeof(RectTransform));
        viewportObject.transform.SetParent(scrollObject.transform, false);
        RectTransform viewport = (RectTransform)viewportObject.transform;
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = viewport.offsetMax = Vector2.zero;
        GameObject contentObject = new("Content", typeof(RectTransform));
        contentObject.transform.SetParent(viewport, false);
        _content = (RectTransform)contentObject.transform;
        _content.anchorMin = new Vector2(0f, 1f);
        _content.anchorMax = _content.pivot = new Vector2(1f, 1f);
        _content.anchoredPosition = Vector2.zero;
        _content.sizeDelta = Vector2.zero;
        scroll.viewport = viewport;
        scroll.content = _content;
    }

    private static void CreateCells()
    {
        if (_content == null || Cells.Count != 0) return;
        float cursor = -4f;
        foreach (MclslArtifactEquipmentSlot slot in SlotOrder)
        {
            MclslItemDefinition[] items = MclslArtifactRain.Choices
                .Where(item => item.EquipmentSlot == slot)
                .OrderByDescending(item => item.Grade).ThenBy(item => item.Id, StringComparer.Ordinal).ToArray();
            if (items.Length == 0) continue;
            CreateGroupHeader(SlotLabel(slot), cursor);
            cursor -= 24f;
            for (int index = 0; index < items.Length; index++)
                CreateCell(items[index], index % Columns, index / Columns, cursor);
            int rows = (items.Length + Columns - 1) / Columns;
            cursor -= rows * CellSize + Math.Max(0, rows - 1) * CellGap + 12f;
        }
        _content.sizeDelta = new Vector2(0f, -cursor + 4f);
    }

    private static void CreateGroupHeader(string title, float top)
    {
        GameObject header = new("Header_" + title, typeof(RectTransform), typeof(Text));
        header.transform.SetParent(_content, false);
        RectTransform rect = (RectTransform)header.transform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, top);
        rect.sizeDelta = new Vector2(0f, 20f);
        Text label = header.GetComponent<Text>();
        label.font = LocalizedTextManager.current_font;
        label.fontSize = 11;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = new Color(0.85f, 0.85f, 0.85f);
        label.text = "  " + title;
        label.raycastTarget = false;
    }

    private static void CreateCell(MclslItemDefinition item, int column, int row, float groupTop)
    {
        GameObject cellObject = new("Cell_" + item.Id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(TipButton));
        cellObject.transform.SetParent(_content, false);
        RectTransform rect = (RectTransform)cellObject.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(8f + column * (CellSize + CellGap), groupTop - row * (CellSize + CellGap));
        rect.sizeDelta = new Vector2(CellSize, CellSize);
        Image background = cellObject.GetComponent<Image>();
        background.sprite = SpriteTextureLoader.getSprite(BackgroundSprite);
        background.type = Image.Type.Sliced;
        Button button = cellObject.GetComponent<Button>();
        button.targetGraphic = background;
        GameObject iconObject = new("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(cellObject.transform, false);
        RectTransform iconRect = (RectTransform)iconObject.transform;
        iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(30f, 30f);
        Image icon = iconObject.GetComponent<Image>();
        icon.sprite = SpriteTextureLoader.getSprite(item.IconPath);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        TipButton tip = cellObject.GetComponent<TipButton>();
        tip.textOnClick = MclslLocalizationBridge.RuntimeText(item.Name);
        tip.textOnClickDescription = MclslLocalizationBridge.RuntimeText(item.EffectText + "\n品阶：" + GradeName(item.Grade)
            + "\n类别：" + SlotLabel(item.EquipmentSlot)
            + "\n制作配方：" + MclslItemCatalog.IngredientDisplayName(item.IngredientA)
            + " + " + MclslItemCatalog.IngredientDisplayName(item.IngredientB));
        Cell cell = new() { Item = item, Background = background, Icon = icon };
        button.onClick.AddListener(() =>
        {
            MclslArtifactRain.SetSelected(item.Id, !MclslArtifactRain.IsSelected(item.Id));
            RefreshCell(cell);
        });
        Cells.Add(cell);
    }

    private static void CreateBarButton(Transform parent, string title, float x, UnityAction clicked)
    {
        GameObject buttonObject = new("Button_" + title, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)buttonObject.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(x, -40f);
        rect.sizeDelta = new Vector2(96f, 24f);
        Image background = buttonObject.GetComponent<Image>();
        background.sprite = SpriteTextureLoader.getSprite(BackgroundSprite);
        background.type = Image.Type.Sliced;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(clicked);
        GameObject textObject = new("Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = (RectTransform)textObject.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        Text text = textObject.GetComponent<Text>();
        text.font = LocalizedTextManager.current_font;
        text.fontSize = 10;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = title;
        text.raycastTarget = false;
    }

    private static void Refresh()
    {
        foreach (Cell cell in Cells) RefreshCell(cell);
    }

    private static void RefreshCell(Cell cell)
    {
        bool selected = MclslArtifactRain.IsSelected(cell.Item.Id);
        Color quality = cell.Item.Grade switch
        {
            0 => new Color(0.48f, 0.48f, 0.50f, 0.90f),
            4 => new Color(0.72f, 0.20f, 0.20f, 0.85f),
            3 => new Color(0.78f, 0.63f, 0.17f, 0.85f),
            2 => new Color(0.20f, 0.43f, 0.75f, 0.85f),
            _ => new Color(0.25f, 0.61f, 0.30f, 0.85f)
        };
        cell.Background.color = selected ? quality : new Color(0.22f, 0.22f, 0.26f, 0.5f);
        cell.Icon.color = selected ? Color.white : new Color(1f, 1f, 1f, 0.4f);
    }

    private static string GradeName(int grade) => grade switch
    {
        4 => "天阶", 3 => "地阶", 2 => "玄阶", 1 => "黄阶", _ => "凡阶"
    };

    private static string SlotLabel(MclslArtifactEquipmentSlot slot) => slot switch
    {
        MclslArtifactEquipmentSlot.Weapon => "武器",
        MclslArtifactEquipmentSlot.Helmet => "头冠",
        MclslArtifactEquipmentSlot.Armor => "护甲",
        MclslArtifactEquipmentSlot.Boots => "鞋靴",
        MclslArtifactEquipmentSlot.Ring => "戒指",
        _ => "佩饰"
    };
}
