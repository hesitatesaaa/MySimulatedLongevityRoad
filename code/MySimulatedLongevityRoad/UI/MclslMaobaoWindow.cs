using System;
using System.Collections.Generic;
using System.Linq;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 登名石式猫宝窗口：同样使用原生 ScrollWindow、保存时间、放置和删除行，
/// 仅把入口图标、文字和颜色换成模拟长生路自己的风格。
/// </summary>
internal static class MclslMaobaoWindow
{
    private const string WindowId = "MclslMaobaoWindow";
    private const float WindowWidth = 280f;
    private const float WindowHeight = 370f;

    private static ScrollWindow _window;
    private static Text _selectedText;
    private static Text _statusText;
    private static RectTransform _recordsContent;
    private static string _statusMessage = string.Empty;
    private static string _pendingDeleteId = string.Empty;

    internal static void Show(Actor actor = null, bool saveActor = false)
    {
        MclslMaobaoArchiveManager.Init();
        if (saveActor && actor?.data != null)
            MclslMaobaoArchiveManager.SaveActor(actor, out _statusMessage);
        bool createdNow = _window == null;
        EnsureWindow();
        Refresh();
        MclslWindowOpenGuard.Show(_window, WindowId, createdNow);
    }

    private static void EnsureWindow()
    {
        if (_window != null) return;
        MclslLocalizationBridge.RegisterKey("mclsl.maobao.window", "猫宝");
        MclslMaobaoPlaceMode.EnsureRegistered();
        _window = WindowCreator.CreateEmptyWindow(WindowId, "mclsl.maobao.window", "ui/Icons/MaobaoEntrance");
        if (_window == null) return;
        Transform bg = _window.transform.Find("Background");
        if (bg == null) return;
        bg.GetComponent<RectTransform>().sizeDelta = new Vector2(WindowWidth, WindowHeight);
        _selectedText = CreateText(bg, "Selected", new Vector2(-18f, 128f), new Vector2(185f, 18f), 8, TextAnchor.MiddleLeft);
        Button save = CreateButton(bg, "SaveBtn", "保存", new Vector2(92f, 128f), new Vector2(38f, 20f), MclslUiTheme.RankButton);
        save.onClick.AddListener(SaveSelectedActor);
        _statusText = CreateText(bg, "Status", new Vector2(0f, 108f), new Vector2(236f, 18f), 7, TextAnchor.MiddleCenter);
        CreateScrollView(bg);
    }

    private static void Refresh()
    {
        if (_window == null || _recordsContent == null) return;
        Actor selected = null;
        try { selected = SelectedUnit.unit; } catch { }
        if (_selectedText != null)
            _selectedText.text = selected?.data != null
                ? "当前选中：" + SafeName(selected)
                : "当前选中：暂无可保存角色";
        if (_statusText != null) _statusText.text = _statusMessage;

        for (int i = _recordsContent.childCount - 1; i >= 0; i--)
            UnityEngine.Object.Destroy(_recordsContent.GetChild(i).gameObject);

        List<MclslMaobaoArchiveManager.SavedActorPacket> saved = MclslMaobaoArchiveManager.GetSavedActors()
            .OrderByDescending(x => x?.SaveTime ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(x => x?.ActorId ?? string.Empty, StringComparer.Ordinal)
            .ToList();
        if (saved.Count == 0)
        {
            CreateRecordText(_recordsContent, "暂无猫宝登名记录", 70f);
            return;
        }
        for (int i = 0; i < saved.Count; i++) CreateRecordRow(_recordsContent, saved[i], i + 1);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_recordsContent);
    }

    private static void SaveSelectedActor()
    {
        Actor selected = null;
        try { selected = SelectedUnit.unit; } catch { }
        if (selected?.data == null)
        {
            _statusMessage = "未选中可保存的角色。";
            Refresh();
            return;
        }
        MclslMaobaoArchiveManager.SaveActor(selected, out _statusMessage);
        Refresh();
    }

    private static void CreateScrollView(Transform parent)
    {
        GameObject scrollObject = new("Scroll", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollObject.transform.SetParent(parent, false);
        RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
        scrollRect.anchorMin = scrollRect.anchorMax = scrollRect.pivot = new Vector2(0.5f, 0.5f);
        scrollRect.anchoredPosition = new Vector2(0f, -36f);
        scrollRect.sizeDelta = new Vector2(260f, 268f);
        scrollObject.GetComponent<Image>().color = new Color(0.02f, 0.08f, 0.09f, 0.24f);
        scrollObject.GetComponent<Mask>().showMaskGraphic = true;

        GameObject viewport = new("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(scrollObject.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = viewportRect.offsetMax = Vector2.zero;

        GameObject content = new("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        _recordsContent = content.GetComponent<RectTransform>();
        _recordsContent.anchorMin = new Vector2(0f, 1f);
        _recordsContent.anchorMax = new Vector2(1f, 1f);
        _recordsContent.pivot = new Vector2(0.5f, 1f);
        _recordsContent.anchoredPosition = Vector2.zero;
        _recordsContent.sizeDelta = new Vector2(-12f, 0f);
        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 3f;
        layout.padding = new RectOffset(5, 5, 5, 5);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;
        scroll.viewport = viewportRect;
        scroll.content = _recordsContent;
    }

    private static void CreateRecordRow(Transform parent, MclslMaobaoArchiveManager.SavedActorPacket packet, int index)
    {
        GameObject row = new("Row", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        row.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 42f);
        LayoutElement element = row.GetComponent<LayoutElement>();
        element.minWidth = element.preferredWidth = 210f;
        element.minHeight = element.preferredHeight = 42f;
        Image background = row.GetComponent<Image>();
        background.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
        background.type = background.sprite == null ? Image.Type.Simple : Image.Type.Sliced;

        Text rank = CreateText(row.transform, "Rank", new Vector2(-91f, 0f), new Vector2(24f, 28f), 10, TextAnchor.MiddleCenter);
        rank.color = index switch { 1 => new Color(0.75f, 0.93f, 0.87f), 2 => new Color(0.72f, 0.82f, 0.82f), 3 => new Color(0.70f, 0.76f, 0.65f), _ => MclslUiTheme.RankTextPrimary };
        rank.text = index.ToString();
        Text name = CreateText(row.transform, "Name", new Vector2(-22f, 9f), new Vector2(112f, 16f), 8, TextAnchor.MiddleLeft);
        name.text = SafeText(packet?.Name, "未名角色");
        Text time = CreateText(row.transform, "Time", new Vector2(-22f, -9f), new Vector2(112f, 14f), 7, TextAnchor.MiddleLeft);
        time.color = MclslUiTheme.RankTextMuted;
        time.text = packet?.SaveTime ?? string.Empty;

        Button place = CreateButton(row.transform, "Place", "放", new Vector2(70f, 0f), new Vector2(20f, 20f), new Color(0.18f, 0.50f, 0.42f, 0.95f));
        place.onClick.AddListener(() =>
        {
            _pendingDeleteId = string.Empty;
            _statusMessage = MclslMaobaoPlaceMode.Begin(packet.ActorId) ? "请选择地图位置放置角色。" : "放置模式启动失败。";
            Refresh();
        });
        Button delete = CreateButton(row.transform, "Delete", "删", new Vector2(94f, 0f), new Vector2(20f, 20f), new Color(0.48f, 0.25f, 0.27f, 0.95f));
        delete.onClick.AddListener(() =>
        {
            if (_pendingDeleteId == packet.ActorId)
            {
                MclslMaobaoArchiveManager.Remove(packet.ActorId, out _statusMessage);
                _pendingDeleteId = string.Empty;
            }
            else
            {
                _pendingDeleteId = packet.ActorId;
                _statusMessage = "再次点击“删”确认删除 " + SafeText(packet.Name, "未名角色");
            }
            Refresh();
        });
        TipButton tip = row.AddComponent<TipButton>();
        tip.textOnClick = SafeText(packet.Name, "猫宝角色");
        tip.textOnClickDescription = "完整保存角色数据；可用“放”在当前地图重新生成。";
    }

    private static Text CreateText(Transform parent, string name, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
    {
        GameObject obj = new(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = obj.GetComponent<Text>();
        text.font = LocalizedTextManager.current_font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = MclslUiTheme.RankTextPrimary;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Color color)
    {
        GameObject obj = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = obj.GetComponent<Image>();
        image.color = color;
        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;
        GameObject labelObject = new("Text", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(obj.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        Text text = labelObject.GetComponent<Text>();
        text.font = LocalizedTextManager.current_font;
        text.fontSize = 9;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
        text.raycastTarget = false;
        return button;
    }

    private static void CreateRecordText(Transform parent, string value, float height)
    {
        GameObject obj = new("RecordText", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        obj.transform.SetParent(parent, false);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, height);
        LayoutElement element = obj.GetComponent<LayoutElement>();
        element.minHeight = 28f;
        element.preferredHeight = height;
        Text text = obj.GetComponent<Text>();
        text.font = LocalizedTextManager.current_font;
        text.fontSize = 10;
        text.alignment = TextAnchor.UpperLeft;
        text.color = MclslUiTheme.RankTextPrimary;
        text.raycastTarget = false;
        text.text = value;
    }

    private static string SafeName(Actor actor)
    {
        try { return SafeText(actor?.getName(), "未名角色"); } catch { return "未名角色"; }
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
