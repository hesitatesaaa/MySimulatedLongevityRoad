using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslActorOverviewStatsFormatter
{
    private static readonly OverviewStatIcon[] Icons =
    {
        new("MclslImmortalFate", "仙缘", "ui/Icons/XuanHuangXianLu"),
        new("MclslMindState", "心境", "ui/Icons/XinJing", "ui/Icons/HuanZhen"),
        new("MclslMortalMiasma", "仙凡瘴", "ui/Icons/XianFanZhang"),
        new("MclslTrueEssence", "真元", "ui/Icons/ZhenQi"),
        new("MclslContribution", "贡献", "ui/Icons/GongXianZhi"),
        new("MclslSpiritStones", "灵石", "ui/Icons/LingShi")
    };

    internal static void Refresh(UnitWindow window)
    {
        Actor actor = window?.actor;
        if (!ShouldShowOverview(actor))
        {
            HideMclslOverview(window);
            return;
        }

        MclslActorCultivationView cultivation = MclslActorCultivationQuery.Build(actor);
        if (!EnsureIconGroup(window)) return;

        SetIconVisibility(window, "MclslAptitude", false);
        SetIconVisibility(window, "MclslQi", false);
        SetIconValue(window, "MclslImmortalFate", cultivation.ImmortalFate);
        SetIconValue(window, "MclslMindState", cultivation.MindState);
        if (ShouldShowMiasma(cultivation)) SetIconText(window, "MclslMortalMiasma", cultivation.MortalMiasma + "/" + cultivation.MortalMiasmaLimit);
        else SetIconVisibility(window, "MclslMortalMiasma", false);
        if (cultivation.NextRealmMinimum <= 0 && !string.IsNullOrWhiteSpace(cultivation.RealmId)) SetIconText(window, "MclslTrueEssence", "-");
        else SetIconValue(window, "MclslTrueEssence", cultivation.TrueEssence);
        SetIconValue(window, "MclslContribution", cultivation.Contribution);
        SetIconValue(window, "MclslSpiritStones", cultivation.SpiritStones);
        ArrangeOverviewRows(window);
        RefreshFactionAffiliationRow(window, actor, cultivation);
    }

    private static bool ShouldShowOverview(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor)) return false;
        if (IsWorldSoulEntity(actor)) return false;
        if (!MclslEligibility.CanCultivate(actor)) return false;
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
        catch { return false; }
    }

    private static void HideMclslOverview(UnitWindow window)
    {
        if (window == null) return;
        for (int i = 0; i < Icons.Length; i++) SetIconVisibility(window, Icons[i].Id, false);
        SetIconVisibility(window, "MclslAptitude", false);
        SetIconVisibility(window, "MclslQi", false);
        HideFactionAffiliationRow(window);
    }

    private static bool ShouldShowMiasma(MclslActorCultivationView cultivation)
    {
        if (cultivation == null || cultivation.IsSpiritualRootPath || cultivation.MortalMiasmaLimit <= 0) return false;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        return run != null && !string.Equals(run.CultivationEpoch, MclslWorldEpochSystem.AncientLawEpoch, StringComparison.Ordinal);
    }

    private static void RefreshFactionAffiliationRow(UnitWindow window, Actor actor, MclslActorCultivationView cultivation)
    {
        Transform content = ((Component)window).transform.Find("Background/Scroll View/Viewport/Content");
        if (content == null) return;
        Transform row = content.Find("MclslFactionAffiliationRow");
        if (row == null) row = CreateFactionAffiliationRow(content);
        if (row == null) return;

        bool show = cultivation != null
            && !string.IsNullOrWhiteSpace(cultivation.RealmId)
            && MclslWorldEpochSystem.IsNewLawActive(MclslRuntime.CurrentYear());
        row.gameObject.SetActive(show);
        if (!show) return;

        Text text = row.GetComponentInChildren<Text>(true);
        if (text == null) return;
        string affiliation = FactionAffiliationName(actor);
        text.text = "<color=#D8CDAA>已加入：</color><color=#F1D17A>" + (string.IsNullOrWhiteSpace(affiliation) ? "无" : affiliation) + "</color>";
        row.SetAsLastSibling();
    }

    private static void HideFactionAffiliationRow(UnitWindow window)
    {
        Transform content = window == null ? null : ((Component)window).transform.Find("Background/Scroll View/Viewport/Content");
        Transform row = content == null ? null : content.Find("MclslFactionAffiliationRow");
        if (row != null) row.gameObject.SetActive(false);
    }

    private static Transform CreateFactionAffiliationRow(Transform content)
    {
        GameObject row = new GameObject("MclslFactionAffiliationRow", typeof(RectTransform), typeof(LayoutElement));
        row.transform.SetParent(content, false);
        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.sizeDelta = new Vector2(0f, 28f);
        LayoutElement layout = row.GetComponent<LayoutElement>();
        layout.minHeight = 26f;
        layout.preferredHeight = 28f;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(row.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 0f);
        textRect.offsetMax = new Vector2(-12f, 0f);
        Text text = textObject.GetComponent<Text>();
        text.font = LocalizedTextManager.current_font ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 15;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.supportRichText = true;
        return row.transform;
    }

    private static string FactionAffiliationName(Actor actor)
    {
        string value = MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty);
        return value switch
        {
            "wanxian" => "万仙盟",
            "five_elders" => "五老会",
            _ => string.Empty
        };
    }

    private static bool EnsureIconGroup(UnitWindow window)
    {
        Transform content = ((Component)window).transform.Find("Background/Scroll View/Viewport/Content/content_more_icons");
        if (content == null) return false;
        Transform targetGroup = FindKillsGroup(content);
        if (targetGroup == null) return false;
        Transform templateIcon = targetGroup.Find("i_kills");
        if (templateIcon == null && targetGroup.childCount > 0) templateIcon = targetGroup.GetChild(0);
        if (templateIcon == null) return false;

        for (int i = 0; i < Icons.Length; i++)
        {
            if (targetGroup.Find(Icons[i].Id) != null) continue;
            Transform iconTransform = UnityEngine.Object.Instantiate(templateIcon, targetGroup);
            ConfigureIcon(iconTransform, Icons[i]);
        }

        ArrangeKillsRow(content, targetGroup);
        return true;
    }

    private static Transform FindKillsGroup(Transform content)
    {
        if (content == null) return null;
        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            if (child != null && child.Find("i_kills") != null) return child;
        }
        return content.childCount > 4 ? content.GetChild(4) : (content.childCount > 0 ? content.GetChild(content.childCount - 1) : null);
    }

    private static void ArrangeKillsRow(Transform content, Transform targetGroup)
    {
        if (targetGroup == null) return;
        Transform[] orderedIcons = BuildOrderedIcons(targetGroup);
        for (int i = 0; i < orderedIcons.Length; i++)
            if (orderedIcons[i] != null) orderedIcons[i].SetSiblingIndex(i);

        Transform templateGroup = FindTemplateIconRow(content, targetGroup);
        AlignRowLayout(targetGroup, templateGroup);
        RectTransform targetRect = targetGroup as RectTransform;
        RectTransform templateRect = templateGroup as RectTransform;
        if (targetRect != null && templateRect != null)
        {
            CopyRowRectTransform(targetRect, templateRect);
            EnsureRowHeight(targetRect, templateGroup, orderedIcons.Length);
        }

        RectTransform[] columns = GetTemplateColumns(templateGroup);
        for (int i = 0; i < orderedIcons.Length; i++)
        {
            RectTransform iconRect = orderedIcons[i] as RectTransform;
            if (iconRect == null) continue;
            if (columns != null && columns.Length > 0 && i < columns.Length && columns[i] != null)
            {
                CopyRectTransform(iconRect, columns[i]);
                ApplyRowOffset(iconRect, templateGroup, i, columns.Length);
            }
            else ApplyBackupColumn(iconRect, i);
        }
    }

    private static Transform[] BuildOrderedIcons(Transform targetGroup)
    {
        List<Transform> ordered = new(Icons.Length + 1);
        Transform kills = targetGroup.Find("i_kills");
        if (kills != null && kills.gameObject.activeSelf) ordered.Add(kills);
        for (int i = 0; i < Icons.Length; i++)
        {
            Transform icon = targetGroup.Find(Icons[i].Id);
            if (icon != null && icon.gameObject.activeSelf) ordered.Add(icon);
        }
        return ordered.ToArray();
    }

    private static Transform FindTemplateIconRow(Transform content, Transform targetGroup)
    {
        if (content == null) return null;
        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            if (child == null || child == targetGroup) continue;
            if (GetTemplateColumns(child)?.Length >= 4) return child;
        }
        return null;
    }

    private static void AlignRowLayout(Transform targetGroup, Transform templateGroup)
    {
        if (targetGroup == null) return;
        HorizontalLayoutGroup targetHorizontal = targetGroup.GetComponent<HorizontalLayoutGroup>();
        HorizontalLayoutGroup templateHorizontal = templateGroup != null ? templateGroup.GetComponent<HorizontalLayoutGroup>() : null;
        if (targetHorizontal != null)
        {
            if (templateHorizontal != null)
            {
                targetHorizontal.padding = templateHorizontal.padding;
                targetHorizontal.spacing = templateHorizontal.spacing;
                targetHorizontal.childControlWidth = templateHorizontal.childControlWidth;
                targetHorizontal.childControlHeight = templateHorizontal.childControlHeight;
                targetHorizontal.childForceExpandWidth = templateHorizontal.childForceExpandWidth;
                targetHorizontal.childForceExpandHeight = templateHorizontal.childForceExpandHeight;
            }
            targetHorizontal.childAlignment = TextAnchor.MiddleLeft;
        }
        GridLayoutGroup targetGrid = targetGroup.GetComponent<GridLayoutGroup>();
        GridLayoutGroup templateGrid = templateGroup != null ? templateGroup.GetComponent<GridLayoutGroup>() : null;
        if (targetGrid != null)
        {
            if (templateGrid != null)
            {
                targetGrid.cellSize = templateGrid.cellSize;
                targetGrid.spacing = templateGrid.spacing;
                targetGrid.padding = templateGrid.padding;
            }
            targetGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            targetGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            targetGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            targetGrid.constraintCount = 5;
            targetGrid.childAlignment = TextAnchor.UpperLeft;
        }
    }

    private static void EnsureRowHeight(RectTransform target, Transform templateGroup, int itemCount)
    {
        if (target == null || itemCount <= 0) return;
        int columnCount = Math.Min(5, Math.Max(1, GetTemplateColumns(templateGroup)?.Length ?? 5));
        int rowCount = Math.Max(1, (int)Math.Ceiling(itemCount / (double)columnCount));
        if (rowCount <= 1) return;
        float rowStep = ResolveRowStep(templateGroup);
        Vector2 size = target.sizeDelta;
        size.y = Math.Max(size.y, rowStep * rowCount);
        target.sizeDelta = size;
        LayoutElement layout = target.GetComponent<LayoutElement>() ?? target.gameObject.AddComponent<LayoutElement>();
        layout.minHeight = Math.Max(layout.minHeight, size.y);
        layout.preferredHeight = Math.Max(layout.preferredHeight, size.y);
    }

    private static void ApplyRowOffset(RectTransform target, Transform templateGroup, int index, int columnCount)
    {
        if (target == null || columnCount <= 0) return;
        int row = index / columnCount;
        if (row <= 0) return;
        Vector2 position = target.anchoredPosition;
        position.y -= ResolveRowStep(templateGroup) * row;
        target.anchoredPosition = position;
    }

    private static float ResolveRowStep(Transform templateGroup)
    {
        GridLayoutGroup grid = templateGroup != null ? templateGroup.GetComponent<GridLayoutGroup>() : null;
        if (grid != null) return Math.Max(1f, grid.cellSize.y + grid.spacing.y);
        RectTransform[] columns = GetTemplateColumns(templateGroup);
        if (columns != null && columns.Length > 0 && columns[0] != null) return Math.Max(1f, columns[0].sizeDelta.y + 8f);
        return 34f;
    }

    private static RectTransform[] GetTemplateColumns(Transform group)
    {
        if (group == null || group.childCount == 0) return null;
        RectTransform[] columns = new RectTransform[Math.Min(5, group.childCount)];
        int count = 0;
        for (int i = 0; i < group.childCount && count < columns.Length; i++)
        {
            RectTransform rect = group.GetChild(i) as RectTransform;
            if (rect != null) columns[count++] = rect;
        }
        if (count == columns.Length) return columns;
        RectTransform[] trimmed = new RectTransform[count];
        Array.Copy(columns, trimmed, count);
        return trimmed;
    }

    private static void CopyRectTransform(RectTransform target, RectTransform source)
    {
        if (target == null || source == null) return;
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.sizeDelta = source.sizeDelta;
        target.anchoredPosition = source.anchoredPosition;
        target.localScale = source.localScale;
    }

    private static void CopyRowRectTransform(RectTransform target, RectTransform source)
    {
        if (target == null || source == null) return;
        float rowY = target.anchoredPosition.y;
        CopyRectTransform(target, source);
        Vector2 position = target.anchoredPosition;
        position.y = rowY;
        target.anchoredPosition = position;
    }

    private static void ApplyBackupColumn(RectTransform target, int column)
    {
        if (target == null) return;
        Vector2 position = target.anchoredPosition;
        position.x = column * 102f;
        target.anchoredPosition = position;
    }

    private static void ConfigureIcon(Transform iconTransform, OverviewStatIcon icon)
    {
        if (iconTransform == null) return;
        iconTransform.name = icon.Id;
        StatsIcon statsIcon = iconTransform.GetComponent<StatsIcon>();
        if (statsIcon != null)
        {
            statsIcon.name = icon.Id;
            Image iconImage = statsIcon.getIcon();
            Sprite sprite = LoadSprite(icon.ResourcePaths);
            if (sprite != null && iconImage != null) iconImage.sprite = sprite;
        }
        TipButton tip = iconTransform.GetComponent<TipButton>();
        if (tip != null) tip.textOnClick = icon.DisplayName;
    }

    private static void SetIconSprite(UnitWindow window, string id, string resourcePath)
    {
        Transform icon = FindOverviewIcon(window, id);
        Image image = icon?.GetComponent<StatsIcon>()?.getIcon();
        Sprite sprite = LoadSprite(new[] { resourcePath });
        if (image != null && sprite != null) image.sprite = sprite;
    }

    private static Sprite LoadSprite(string[] resourcePaths)
    {
        if (resourcePaths == null) return null;
        for (int i = 0; i < resourcePaths.Length; i++)
        {
            string path = resourcePaths[i]?.Trim().Replace("\\", "/");
            if (string.IsNullOrWhiteSpace(path)) continue;
            Sprite sprite = SpriteTextureLoader.getSprite(path)
                ?? SpriteTextureLoader.getSprite("GameResources/" + path)
                ?? SpriteTextureLoader.getSprite("GameResources/" + path + ".png")
                ?? Resources.Load<Sprite>(path)
                ?? Resources.Load<Sprite>("GameResources/" + path);
            if (sprite != null) return sprite;
        }
        return null;
    }

    private static void SetIconValue(UnitWindow window, string id, int value)
    {
        SetIconVisibility(window, id, true);
        try { window.setIconValue(id, value, null, string.Empty, false, string.Empty, '/'); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslActorOverviewStatsFormatter-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslActorOverviewStatsFormatter.cs #1: " + mclslEmptyCatchEx.Message); }
    }

    private static void SetIconText(UnitWindow window, string id, string value)
    {
        SetIconVisibility(window, id, true);
        try { window.setIconValue(id, 0, null, string.Empty, false, string.Empty, '/'); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslActorOverviewStatsFormatter-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslActorOverviewStatsFormatter.cs #2: " + mclslEmptyCatchEx.Message); }
        Transform icon = FindOverviewIcon(window, id);
        Text[] texts = icon?.GetComponentsInChildren<Text>(true);
        if (texts != null && texts.Length > 0) texts[texts.Length - 1].text = value ?? string.Empty;
    }

    private static void SetIconVisibility(UnitWindow window, string id, bool visible)
    {
        Transform icon = FindOverviewIcon(window, id);
        if (icon != null) icon.gameObject.SetActive(visible);
    }

    private static Transform FindOverviewIcon(UnitWindow window, string id)
    {
        if (window == null || string.IsNullOrWhiteSpace(id)) return null;
        Transform content = ((Component)window).transform.Find("Background/Scroll View/Viewport/Content/content_more_icons");
        Transform targetGroup = FindKillsGroup(content);
        return targetGroup == null ? null : targetGroup.Find(id);
    }

    private static void ArrangeOverviewRows(UnitWindow window)
    {
        Transform content = ((Component)window).transform.Find("Background/Scroll View/Viewport/Content/content_more_icons");
        Transform targetGroup = FindKillsGroup(content);
        if (content != null && targetGroup != null) ArrangeKillsRow(content, targetGroup);
    }

    private readonly struct OverviewStatIcon
    {
        internal readonly string Id;
        internal readonly string DisplayName;
        internal readonly string[] ResourcePaths;

        internal OverviewStatIcon(string id, string displayName, params string[] resourcePaths)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            ResourcePaths = resourcePaths ?? Array.Empty<string>();
        }
    }
}
