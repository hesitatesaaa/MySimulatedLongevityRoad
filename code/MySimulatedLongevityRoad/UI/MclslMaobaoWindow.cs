using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

/// <summary>
/// 猫宝窗口：完整照搬鬼谷“时光长河”的 ScrollWindow、记录列表、放置和二次删除交互，
/// 只替换标题、文字、存档命令和猫宝图标。
/// </summary>
internal static class MclslMaobaoWindow
{
    private const string WindowId = "MclslMaobaoWindow";
    private const string WindowTitleKey = "mclsl.maobao.window";
    private const string IconPath = "ui/Icons/MaobaoEntrance";
    private const float WindowWidth = 280f;
    private const float WindowHeight = 370f;

    private static ScrollWindow _window;
    private static Text _selectedText;
    private static Text _statusText;
    private static RectTransform _recordsContent;
    private static string _statusMessage = string.Empty;
    private static string _pendingDeleteId;
    private static bool _initialized;

    internal static void Init()
    {
        if (_initialized) return;
        MclslLocalizationBridge.RegisterKey(WindowTitleKey, "猫宝");
        MclslLocalizationBridge.RegisterKey(WindowTitleKey + " Description", "猫宝·时序录");
        MclslMaobaoPlaceMode.EnsureRegistered();
        // WindowCreator resolves its title before the runtime localization table is
        // guaranteed to exist.  Give it the final Chinese title directly; the key
        // is still registered for tooltips and external integrations.
        _window = WindowCreator.CreateEmptyWindow(WindowId, "猫宝", IconPath);
        if (_window == null) return;
        SetupWindow();
        _initialized = true;
    }

    internal static void ShowWindow()
    {
        MclslMaobaoArchiveManager.Init();
        if (!_initialized) Init();
        Refresh();
        if (_window == null) return;
        ScrollWindow.showWindow(WindowId);
        Canvas.ForceUpdateCanvases();
    }

    internal static void Show(Actor actor = null, bool saveActor = false)
    {
        MclslMaobaoArchiveManager.Init();
        if (saveActor && actor?.data != null)
        {
            MclslMaobaoCommandResult result = MclslMaobaoCommands.SaveSelectedActor(actor);
            _statusMessage = result.Message;
            if (result.Success) MclslMaobaoUiHelper.ShowWorldTip(result.Message);
        }
        ShowWindow();
    }

    private static void SetupWindow()
    {
        Transform background = _window.transform.Find("Background");
        if (background == null) return;
        background.GetComponent<RectTransform>().sizeDelta = new Vector2(WindowWidth, WindowHeight);

        _selectedText = CreateText(background, "Selected", new Vector2(-18f, 128f), new Vector2(185f, 18f), 8, TextAnchor.MiddleLeft);
        Button saveButton = CreateButton(background, "SaveBtn", "保存", new Vector2(92f, 128f), new Vector2(38f, 20f), new Color(0.18f, 0.16f, 0.12f, 0.9f));
        saveButton.onClick.AddListener(SaveSelectedActor);
        _statusText = CreateText(background, "Status", new Vector2(0f, 108f), new Vector2(220f, 18f), 7, TextAnchor.MiddleCenter);
        CreateScrollView(background);
    }

    private static void Refresh()
    {
        if (_window == null || _recordsContent == null) return;

        MclslMaobaoCommands.TryGetSelectedActor(out Actor selected);
        if (_selectedText != null)
            _selectedText.text = selected?.data != null
                ? "当前选中：" + SafeName(selected)
                : "当前选中：暂无可保存角色";
        if (_statusText != null) _statusText.text = _statusMessage;

        for (int i = _recordsContent.childCount - 1; i >= 0; i--)
            UnityEngine.Object.Destroy(_recordsContent.GetChild(i).gameObject);

        List<MclslMaobaoArchiveManager.SavedActorPacket> saved = MclslMaobaoArchiveManager.GetSavedActors()
            .Where(x => x != null)
            .OrderByDescending(x => x.SaveTime ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(x => x.ActorId ?? string.Empty, StringComparer.Ordinal)
            .ToList();
        if (saved.Count == 0)
        {
            CreateRecordText(_recordsContent, "暂无猫宝登名记录", 70f);
            return;
        }

        for (int i = 0; i < saved.Count; i++) CreateRecordRow(_recordsContent, saved[i].ActorId, saved[i], i + 1);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_recordsContent);
    }

    private static void SaveSelectedActor()
    {
        MclslMaobaoCommandResult result = MclslMaobaoCommands.SaveSelectedActor();
        _statusMessage = result.Message;
        if (result.Success) MclslMaobaoUiHelper.ShowWorldTip(result.Message);
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
        scrollObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.22f);
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

    private static void CreateRecordRow(Transform parent, string recordId, MclslMaobaoArchiveManager.SavedActorPacket packet, int index)
    {
        GameObject row = new("Row", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        row.GetComponent<RectTransform>().sizeDelta = new Vector2(210f, 42f);
        LayoutElement element = row.GetComponent<LayoutElement>();
        element.minWidth = 210f;
        element.preferredWidth = 210f;
        element.minHeight = 42f;
        element.preferredHeight = 42f;

        Image image = row.GetComponent<Image>();
        image.sprite = SpriteTextureLoader.getSprite("ui/special/backgroundKingdomElement");
        image.type = image.sprite == null ? Image.Type.Simple : Image.Type.Sliced;

        Text rank = CreateText(row.transform, "Rank", new Vector2(-91f, 0f), new Vector2(24f, 28f), 10, TextAnchor.MiddleCenter);
        rank.color = index switch
        {
            1 => new Color(0.75f, 0.93f, 0.87f),
            2 => new Color(0.72f, 0.82f, 0.82f),
            3 => new Color(0.70f, 0.76f, 0.65f),
            _ => new Color(0.92f, 0.96f, 0.95f)
        };
        rank.text = index.ToString();

        string actorName = SafeText(packet?.Name, "未名角色");
        Text name = CreateText(row.transform, "Name", new Vector2(-22f, 9f), new Vector2(112f, 16f), 8, TextAnchor.MiddleLeft);
        name.text = actorName;
        Text time = CreateText(row.transform, "Time", new Vector2(-22f, -9f), new Vector2(112f, 14f), 7, TextAnchor.MiddleLeft);
        time.color = new Color(0.72f, 0.72f, 0.72f);
        time.text = packet?.SaveTime ?? string.Empty;

        Button place = CreateTintedButton(row.transform, "Place", "放", new Vector2(70f, 0f), new Vector2(20f, 20f), new Color(0.18f, 0.55f, 0.18f, 0.9f));
        place.onClick.AddListener(() =>
        {
            _pendingDeleteId = null;
            _statusMessage = MclslMaobaoPlaceMode.Begin(recordId) ? "请选择地图位置" : "放置模式启动失败";
            Refresh();
        });

        Button delete = CreateTintedButton(row.transform, "Delete", "删", new Vector2(94f, 0f), new Vector2(20f, 20f), new Color(0.55f, 0.18f, 0.18f, 0.9f));
        delete.onClick.AddListener(() =>
        {
            if (_pendingDeleteId == recordId)
            {
                MclslMaobaoCommandResult result = MclslMaobaoCommands.RemoveSavedActor(recordId);
                _statusMessage = result.Message;
                _pendingDeleteId = null;
            }
            else
            {
                _pendingDeleteId = recordId;
                _statusMessage = "再次点击“删”确认删除 " + actorName;
            }
            Refresh();
        });

        TipButton tip = row.AddComponent<TipButton>();
        tip.textOnClick = actorName;
        tip.textOnClickDescription = "保存时间：" + SafeText(packet?.SaveTime, "未知") + "\n跨世界完整猫宝人物数据";
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
        text.color = new Color(0.96f, 0.97f, 1f);
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

    private static Button CreateTintedButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Color tint)
    {
        return CreateButton(parent, name, label, position, size, tint);
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
        text.color = new Color(0.96f, 0.97f, 1f);
        text.raycastTarget = false;
        text.text = value;
    }

    private static string SafeName(Actor actor)
    {
        try { return SafeText(actor?.getName(), "未名角色"); }
        catch { return "未名角色"; }
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}

internal static class MclslMaobaoUiHelper
{
    internal static void ShowWorldTip(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        try
        {
            string text = "【猫宝】" + message.Trim();
            if (WorldTip.instance != null) WorldTip.instance.show(text, false, "top", 3.8f);
            else WorldTip.showNow(text, false, "top", 3.8f, "#8FE3D1");
        }
        catch { }
    }
}
