using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

// Art is loaded once. Generated titles have generous transparent margins; the
// visible bounds below keep the artwork legible without changing the PNG files.
internal static class XianLuUIResources
{
    private const string Root = "ui/XuanhuangSkin/";
    private static readonly Dictionary<string, Sprite> Sprites = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, Sprite> TrimmedTitles = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, Sprite> TrimmedFrames = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, Rect> TitleBounds = new(StringComparer.Ordinal)
    {
        // Alpha bounds of each PNG, with a small margin for antialiased edges.
        ["title_tianxuanjing"] = new Rect(0, 186, 2172, 401),
        ["title_mod_intro"] = new Rect(0, 147, 2172, 372),
        ["title_ranking"] = new Rect(0, 98, 2172, 532),
        ["title_huanzhen_space"] = new Rect(7, 0, 2041, 768),
        ["title_character_panel"] = new Rect(29, 70, 2105, 598),
        ["title_biography"] = new Rect(34, 13, 2108, 688),
        ["title_xuanhuang_record"] = new Rect(30, 59, 2112, 619)
    };

    internal static Sprite Sprite(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        if (Sprites.TryGetValue(name, out Sprite cached)) return cached;
        Sprite result = null;
        string path = Root + name;
        try
        {
            result = SpriteTextureLoader.getSprite(path)
                ?? SpriteTextureLoader.getSprite(path + ".png")
                ?? Resources.Load<Sprite>(path);
        }
        catch (Exception exception)
        {
            MclslDiagnostics.Error("xianlu-image-" + name, "界面图片加载失败 " + path + ": " + exception.Message);
        }
        if (result == null)
            MclslDiagnostics.Error("xianlu-image-" + name, "界面图片缺失：" + path);
        Sprites[name] = result;
        return result;
    }

    internal static Texture2D Texture(string name) => Sprite(name)?.texture;

    internal static Rect VisibleRect(string name, Sprite sprite)
    {
        Rect source = sprite.textureRect;
        if (!TitleBounds.TryGetValue(name, out Rect bounds)) return source;
        float xScale = source.width / sprite.rect.width;
        float yScale = source.height / sprite.rect.height;
        return new Rect(source.x + bounds.x * xScale,
            source.y + source.height - (bounds.y + bounds.height) * yScale,
            bounds.width * xScale, bounds.height * yScale);
    }

    internal static Sprite TitleSprite(string name)
    {
        if (TrimmedTitles.TryGetValue(name, out Sprite cached)) return cached;
        Sprite source = Sprite(name);
        if (source == null) return null;
        if (!TitleBounds.ContainsKey(name)) return source;
        try
        {
            Sprite cropped = UnityEngine.Sprite.Create(source.texture, VisibleRect(name, source),
                new Vector2(0.5f, 0.5f), 100f);
            TrimmedTitles[name] = cropped;
            return cropped;
        }
        catch (Exception exception)
        {
            MclslDiagnostics.Error("xianlu-title-" + name, "标题裁切失败：" + exception.Message);
            TrimmedTitles[name] = source;
            return source;
        }
    }

    // The portrait artwork has wide transparent gutters. Cropping those gutters
    // makes its rails reach the outside of the existing UnitWindow side panel.
    internal static Sprite FrameSprite(string name)
    {
        if (name != "frame_character_panel") return Sprite(name);
        if (TrimmedFrames.TryGetValue(name, out Sprite cached)) return cached;
        Sprite source = Sprite(name);
        if (source == null) return null;
        try
        {
            Rect texture = source.textureRect;
            float scale = texture.width / source.rect.width;
            Rect crop = new(texture.x + 105f * scale, texture.y,
                814f * scale, texture.height);
            Sprite result = UnityEngine.Sprite.Create(source.texture, crop,
                new Vector2(0.5f, 0.5f), 100f);
            TrimmedFrames[name] = result;
            return result;
        }
        catch (Exception exception)
        {
            MclslDiagnostics.Error("xianlu-frame-" + name, "边框裁切失败：" + exception.Message);
            TrimmedFrames[name] = source;
            return source;
        }
    }

    internal static void Apply(Image image, string name, Color fallback)
    {
        if (image == null) return;
        Sprite sprite = name.StartsWith("title_", StringComparison.Ordinal) ? TitleSprite(name) : FrameSprite(name);
        image.sprite = sprite;
        image.color = sprite == null ? fallback : Color.white;
        image.preserveAspect = sprite != null && name.StartsWith("title_", StringComparison.Ordinal);
        image.raycastTarget = false;
    }

    internal static void Guardian(Image image, string name, float opacity)
    {
        if (image == null) return;
        image.sprite = Sprite(name);
        image.color = image.sprite == null ? Color.clear : new Color(1f, 1f, 1f, opacity);
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    internal static Image EnsureGuardian(Transform parent, string objectName, string spriteName,
        Rect bounds, float opacity)
    {
        Transform existing = parent.Find(objectName);
        Image image = existing != null ? existing.GetComponent<Image>() : null;
        if (image == null)
        {
            image = new GameObject(objectName, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false);
        }
        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(bounds.x, -bounds.y);
        rect.sizeDelta = new Vector2(bounds.width, bounds.height);
        Guardian(image, spriteName, opacity);
        return image;
    }

}

internal static class XianLuUIColors
{
    internal static readonly Color BackgroundDark = MclslUiTheme.SurfaceWindow;
    internal static readonly Color BackgroundPanel = MclslUiTheme.SurfacePanel;
    internal static readonly Color BackgroundCard = MclslUiTheme.SurfaceDeep;
    internal static readonly Color BorderGold = MclslUiTheme.FrameGold;
    internal static readonly Color BorderJade = MclslUiTheme.AccentJade;
    internal static readonly Color TextPrimary = MclslUiTheme.TextPrimary;
    internal static readonly Color TextMuted = MclslUiTheme.TextMuted;
    internal static readonly Color AccentGold = MclslUiTheme.AccentGold;
    internal static readonly Color AccentJade = MclslUiTheme.AccentJade;
}

internal static class XianLuUIStyles
{
    private static bool _ready;
    internal static GUIStyle WindowStyle, TransparentWindowStyle, PanelStyle, CardStyle, InnerPanelStyle;
    internal static GUIStyle ButtonPrimaryStyle, ButtonSecondaryStyle, ButtonDisabledStyle;
    internal static GUIStyle TabNormalStyle, TabHoverStyle, TabSelectedStyle;
    internal static GUIStyle ScrollbarStyle, ScrollbarThumbStyle, TooltipStyle;
    internal static GUIStyle HeaderStyle, SectionHeaderStyle, BodyTextStyle, SecondaryTextStyle, ValueTextStyle;
    private static Texture2D _panel, _card, _deep, _normal, _hover, _active, _selected, _disabled, _scrollTrack, _scrollThumb;

    internal static void Ensure()
    {
        if (_ready) return;
        _panel = Tile(new Color(0.065f, 0.105f, 0.105f, 0.97f), new Color(0.28f, 0.38f, 0.34f, 1f));
        _card = Tile(new Color(0.09f, 0.145f, 0.14f, 0.98f), new Color(0.28f, 0.46f, 0.41f, 1f));
        _deep = Tile(new Color(0.035f, 0.07f, 0.075f, 0.99f), new Color(0.19f, 0.32f, 0.31f, 1f));
        _normal = Tile(new Color(0.11f, 0.19f, 0.18f, 1f), new Color(0.31f, 0.44f, 0.40f, 1f));
        _hover = Tile(new Color(0.18f, 0.29f, 0.26f, 1f), new Color(0.51f, 0.70f, 0.60f, 1f));
        _active = Tile(new Color(0.23f, 0.34f, 0.28f, 1f), new Color(0.63f, 0.53f, 0.30f, 1f));
        _selected = Tile(new Color(0.22f, 0.29f, 0.22f, 1f), new Color(0.69f, 0.55f, 0.29f, 1f));
        _disabled = Tile(new Color(0.07f, 0.09f, 0.09f, 1f), new Color(0.20f, 0.24f, 0.22f, 1f));
        _scrollTrack = Tile(new Color(0.025f, 0.055f, 0.055f, 0.85f), new Color(0.18f, 0.27f, 0.25f, 1f));
        _scrollThumb = Tile(new Color(0.32f, 0.48f, 0.41f, 1f), new Color(0.58f, 0.71f, 0.60f, 1f));
        WindowStyle = Box(GUI.skin.window, _deep, 18);
        TransparentWindowStyle = new GUIStyle(WindowStyle);
        TransparentWindowStyle.normal.background = null;
        TransparentWindowStyle.hover.background = null;
        TransparentWindowStyle.active.background = null;
        TransparentWindowStyle.focused.background = null;
        TransparentWindowStyle.onNormal.background = null;
        TransparentWindowStyle.onHover.background = null;
        TransparentWindowStyle.onActive.background = null;
        TransparentWindowStyle.onFocused.background = null;
        TransparentWindowStyle.border = new RectOffset(0, 0, 0, 0);
        PanelStyle = Box(GUI.skin.box, _panel, 12);
        CardStyle = Box(GUI.skin.box, _card, 10);
        InnerPanelStyle = Box(GUI.skin.box, _deep, 10);
        TooltipStyle = Box(GUI.skin.box, _deep, 9);
        ButtonPrimaryStyle = Button(_selected);
        ButtonSecondaryStyle = Button(_normal);
        ButtonDisabledStyle = Button(_disabled);
        ButtonDisabledStyle.normal.textColor = XianLuUIColors.TextMuted;
        ButtonDisabledStyle.hover.background = _disabled;
        ButtonDisabledStyle.active.background = _disabled;
        TabNormalStyle = Button(_normal);
        TabHoverStyle = Button(_hover);
        TabSelectedStyle = Button(_selected);
        TabSelectedStyle.normal.textColor = XianLuUIColors.AccentGold;
        ScrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar)
        {
            fixedWidth = 12f,
            border = new RectOffset(3, 3, 3, 3)
        };
        ScrollbarStyle.normal.background = _scrollTrack;
        ScrollbarThumbStyle = new GUIStyle(GUI.skin.verticalScrollbarThumb)
        {
            fixedWidth = 10f,
            border = new RectOffset(3, 3, 3, 3)
        };
        ScrollbarThumbStyle.normal.background = _scrollThumb;
        ScrollbarThumbStyle.hover.background = _hover;
        ScrollbarThumbStyle.active.background = _active;
        HeaderStyle = Text(23, FontStyle.Bold, XianLuUIColors.AccentGold, TextAnchor.MiddleCenter);
        SectionHeaderStyle = Text(18, FontStyle.Bold, XianLuUIColors.TextPrimary, TextAnchor.MiddleLeft);
        BodyTextStyle = Text(16, FontStyle.Normal, XianLuUIColors.TextPrimary, TextAnchor.UpperLeft);
        SecondaryTextStyle = Text(14, FontStyle.Normal, XianLuUIColors.TextMuted, TextAnchor.MiddleLeft);
        ValueTextStyle = Text(20, FontStyle.Bold, XianLuUIColors.AccentJade, TextAnchor.MiddleLeft);
        _ready = true;
    }

    private static Texture2D Tile(Color fill, Color edge)
    {
        const int size = 12;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            pixels[y * size + x] = x < 2 || y < 2 || x >= size - 2 || y >= size - 2 ? edge : fill;
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private static GUIStyle Box(GUIStyle source, Texture2D background, int padding)
    {
        GUIStyle style = new(source)
        {
            border = new RectOffset(2, 2, 2, 2),
            padding = new RectOffset(padding, padding, padding, padding)
        };
        style.normal.background = background;
        style.hover.background = background;
        style.active.background = background;
        style.focused.background = background;
        style.onNormal.background = background;
        style.onHover.background = background;
        style.onActive.background = background;
        style.onFocused.background = background;
        return style;
    }

    private static GUIStyle Button(Texture2D background)
    {
        GUIStyle style = new(GUI.skin.button)
        {
            border = new RectOffset(2, 2, 2, 2),
            padding = new RectOffset(10, 10, 6, 6),
            fontSize = MclslUiTheme.ReadableFontSize(16),
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.background = background;
        style.hover.background = _hover;
        style.active.background = _active;
        style.focused.background = _hover;
        style.onNormal.background = _selected;
        style.onHover.background = _selected;
        style.onActive.background = _active;
        style.onFocused.background = _selected;
        style.normal.textColor = XianLuUIColors.TextPrimary;
        style.hover.textColor = Color.white;
        style.active.textColor = Color.white;
        style.onNormal.textColor = XianLuUIColors.AccentGold;
        style.onHover.textColor = XianLuUIColors.AccentGold;
        style.onActive.textColor = Color.white;
        style.onFocused.textColor = XianLuUIColors.AccentGold;
        style.focused.textColor = Color.white;
        style.normal.background = background;
        return style;
    }

    private static GUIStyle Text(int size, FontStyle weight, Color color, TextAnchor anchor)
    {
        GUIStyle style = new(GUI.skin.label) { fontSize = MclslUiTheme.ReadableFontSize(size), fontStyle = weight, wordWrap = true, alignment = anchor };
        style.normal.textColor = color;
        return style;
    }
}

internal static class XianLuUIRenderer
{
    internal static void Frame(Rect rect, string name, float fromTop = 0f, float toTop = 1f)
    {
        Sprite sprite = XianLuUIResources.Sprite(name);
        if (sprite == null || Event.current.type != EventType.Repaint) return;
        Color old = GUI.color;
        GUI.color = Color.white;
        Rect source = sprite.textureRect;
        float span = toTop - fromTop;
        Rect target = new Rect(rect.x, rect.y + rect.height * fromTop, rect.width, rect.height * span);
        Rect section = new Rect(source.x, source.y + source.height * (1f - toTop), source.width, source.height * span);
        GUI.DrawTextureWithTexCoords(target, sprite.texture, Uv(section, sprite.texture), true);
        GUI.color = old;
    }

    internal static void ScreenGuardians()
    {
        if (Event.current.type != EventType.Repaint) return;
        DrawScreenGuardian("guardian_bai", true, 0.14f);
        DrawScreenGuardian("guardian_chuanfa", false, 0.18f);
    }

    private static void DrawScreenGuardian(string name, bool left, float opacity)
    {
        Sprite sprite = XianLuUIResources.Sprite(name);
        if (sprite == null) return;
        float width = Mathf.Min(Screen.height * sprite.rect.width / sprite.rect.height,
            Screen.width * 0.37f);
        float height = width * sprite.rect.height / sprite.rect.width;
        Rect target = new(left ? 0f : Screen.width - width,
            (Screen.height - height) * 0.5f, width, height);
        Color old = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, opacity);
        GUI.DrawTextureWithTexCoords(target, sprite.texture, Uv(sprite.textureRect, sprite.texture), true);
        GUI.color = old;
    }

    internal static void WindowTitle(Rect windowRect, string name, string fallback, float top = 7f, float maxHeight = 48f)
    {
        Rect slot = new Rect(windowRect.x + windowRect.width * 0.19f, windowRect.y + top,
            windowRect.width * 0.62f, maxHeight);
        Title(slot, name, fallback);
    }

    // Keep the old title scale and center while expanding the drawable region.
    // The optional upper slice is painted outside GUI.Window's content clip.
    internal static void WindowTitleExpanded(Rect windowRect, string name, string fallback,
        float top, float maxHeight, float expandTop, float expandBottom,
        float clipBottom = float.PositiveInfinity)
    {
        Rect slot = new(windowRect.x + windowRect.width * 0.19f, windowRect.y + top,
            windowRect.width * 0.62f, maxHeight);
        Sprite sprite = XianLuUIResources.Sprite(name);
        if (sprite == null)
        {
            if (!float.IsPositiveInfinity(clipBottom)) return;
            XianLuUIStyles.Ensure();
            GUI.Label(slot, fallback, XianLuUIStyles.HeaderStyle);
            return;
        }
        if (Event.current.type != EventType.Repaint) return;

        Rect visible = XianLuUIResources.VisibleRect(name, sprite);
        Rect source = sprite.textureRect;
        float scale = Mathf.Min(slot.width / visible.width, slot.height / visible.height);
        Rect oldTarget = new(slot.center.x - visible.width * scale * 0.5f,
            slot.center.y - visible.height * scale * 0.5f,
            visible.width * scale, visible.height * scale);
        Rect fullTarget = new(oldTarget.x - (visible.x - source.x) * scale,
            oldTarget.y - (source.yMax - visible.yMax) * scale,
            source.width * scale, source.height * scale);
        Rect clip = new(oldTarget.x, oldTarget.y - expandTop, oldTarget.width,
            oldTarget.height + expandTop + expandBottom);
        clip.height = Mathf.Min(clip.yMax, clipBottom) - clip.y;
        if (clip.height <= 0f) return;
        Color old = GUI.color;
        GUI.color = Color.white;
        GUI.BeginGroup(clip);
        GUI.DrawTextureWithTexCoords(new Rect(fullTarget.x - clip.x, fullTarget.y - clip.y,
            fullTarget.width, fullTarget.height), sprite.texture, Uv(source, sprite.texture), true);
        GUI.EndGroup();
        GUI.color = old;
    }

    internal static void Title(Rect rect, string name, string fallback)
    {
        Sprite sprite = XianLuUIResources.Sprite(name);
        if (sprite == null)
        {
            XianLuUIStyles.Ensure();
            GUI.Label(rect, fallback, XianLuUIStyles.HeaderStyle);
            return;
        }
        if (Event.current.type != EventType.Repaint) return;
        Rect source = XianLuUIResources.VisibleRect(name, sprite);
        float width = Mathf.Min(rect.width, rect.height * source.width / source.height);
        float height = width * source.height / source.width;
        Rect target = new(rect.center.x - width * 0.5f, rect.center.y - height * 0.5f, width, height);
        Color old = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTextureWithTexCoords(target, sprite.texture, Uv(source, sprite.texture), true);
        GUI.color = old;
    }

    internal static void EmptyState(Rect rect, string title, string detail)
    {
        XianLuUIStyles.Ensure();
        GUI.Box(rect, GUIContent.none, XianLuUIStyles.CardStyle);
        GUI.Label(new Rect(rect.x + 16f, rect.y + 14f, rect.width - 32f, 30f), title,
            XianLuUIStyles.HeaderStyle);
        GUI.Label(new Rect(rect.x + 18f, rect.y + 48f, rect.width - 36f, rect.height - 58f), detail,
            XianLuUIStyles.SecondaryTextStyle);
    }

    private static Rect Uv(Rect pixels, Texture2D texture) => new(
        pixels.x / texture.width, pixels.y / texture.height,
        pixels.width / texture.width, pixels.height / texture.height);
}

// Native Unity windows need a canvas-sized layer so the guardians remain at
// the screen edges even when the window moves or changes scale.
internal sealed class XianLuScreenGuardianOverlay : MonoBehaviour
{
    private RectTransform _overlay;
    private RectTransform _left;
    private RectTransform _right;
    private Vector2 _lastSize;

    internal static void Ensure(Transform window)
    {
        if (window == null) return;
        XianLuScreenGuardianOverlay layer = window.GetComponent<XianLuScreenGuardianOverlay>()
            ?? window.gameObject.AddComponent<XianLuScreenGuardianOverlay>();
        layer.Initialize();
        if (layer._overlay != null) layer._overlay.gameObject.SetActive(true);
    }

    internal static void Hide(Transform window)
    {
        XianLuScreenGuardianOverlay layer = window?.GetComponent<XianLuScreenGuardianOverlay>();
        if (layer != null && layer._overlay != null) layer._overlay.gameObject.SetActive(false);
    }

    private void Initialize()
    {
        if (_overlay != null) return;
        Canvas canvas = GetComponentInParent<Canvas>()?.rootCanvas;
        if (canvas == null) return;
        GameObject layer = new("MclslScreenGuardians", typeof(RectTransform));
        _overlay = layer.GetComponent<RectTransform>();
        _overlay.SetParent(canvas.transform, false);
        _overlay.anchorMin = Vector2.zero;
        _overlay.anchorMax = Vector2.one;
        _overlay.offsetMin = Vector2.zero;
        _overlay.offsetMax = Vector2.zero;
        Transform windowLayer = transform;
        while (windowLayer.parent != null && windowLayer.parent != canvas.transform)
            windowLayer = windowLayer.parent;
        if (windowLayer.parent == canvas.transform)
            _overlay.SetSiblingIndex(windowLayer.GetSiblingIndex());
        _left = AddFigure("白先生虚影", "guardian_bai", 0.14f);
        _right = AddFigure("传法天尊虚影", "guardian_chuanfa", 0.18f);
        UpdateLayout();
    }

    private RectTransform AddFigure(string objectName, string spriteName, float opacity)
    {
        Image image = new GameObject(objectName, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(_overlay, false);
        XianLuUIResources.Guardian(image, spriteName, opacity);
        return image.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (_overlay != null) UpdateLayout();
    }

    private void UpdateLayout()
    {
        Vector2 size = _overlay.rect.size;
        if (size == _lastSize) return;
        _lastSize = size;
        Place(_left, true, size);
        Place(_right, false, size);
    }

    private static void Place(RectTransform figure, bool left, Vector2 canvasSize)
    {
        Sprite sprite = figure.GetComponent<Image>().sprite;
        if (sprite == null) return;
        float width = Mathf.Min(canvasSize.y * sprite.rect.width / sprite.rect.height,
            canvasSize.x * 0.37f);
        float height = width * sprite.rect.height / sprite.rect.width;
        Vector2 anchor = new(left ? 0f : 1f, 0.5f);
        figure.anchorMin = anchor;
        figure.anchorMax = anchor;
        figure.pivot = anchor;
        figure.anchoredPosition = Vector2.zero;
        figure.sizeDelta = new Vector2(width, height);
    }

    private void OnEnable()
    {
        if (_overlay != null) _overlay.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (_overlay != null) _overlay.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_overlay != null) Destroy(_overlay.gameObject);
    }
}
