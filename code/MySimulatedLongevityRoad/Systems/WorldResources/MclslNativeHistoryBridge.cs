using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslNativeHistoryBridge
{
    private const string AssetPrefix = "mclsl_history_";
    private static bool _initialized;
    private static bool _disabled;
    private static MclslWorldRunState _legacyRun;
    private static readonly HashSet<(int Year, string Title, string Body)> LegacyLineages = new();

    internal static void BeginLegacyClassification(MclslWorldRunState run)
    {
        _legacyRun = run;
        LegacyLineages.Clear();
    }

    internal static void RegisterLegacyClassification(MclslRunEventRecord record)
    {
        if (record.NativeLogged && MclslEventCatalog.IsLineageEvent(record.EventType))
            LegacyLineages.Add((record.Year, record.Title, TrimForNative(record.Body)));
    }

    internal static void EnsureRegistered()
    {
        if (_disabled) return;
        try { EnsureAssets(); }
        catch (Exception ex) { Debug.LogWarning("[模拟长生路] 原生历史分栏注册失败: " + ex.Message); }
    }

    internal static bool Add(MclslRunEventRecord record)
    {
        if (_disabled || record == null || string.IsNullOrWhiteSpace(record.Title)) return false;
        if (!MclslEventCatalog.ShouldMirrorToNativeHistory(record.EventType, record.Importance,
                MclslRuntimeSettings.LineageNativeHistoryEnabled)) return false;
        try
        {
            EnsureAssets();
            string category = MclslEventCatalog.IsLineageEvent(record.EventType)
                ? MclslEventCatalog.Lineage
                : string.IsNullOrWhiteSpace(record.Category) ? MclslEventCatalog.CategoryForType(record.EventType) : record.Category;
            MclslEventCategoryDefinition definition = MclslEventCatalog.Category(category);
            WorldLogMessage message = new()
            {
                asset_id = AssetPrefix + definition.Id,
                timestamp = MclslEventCatalog.NativeTimestampForYear(record.Year),
                special1 = definition.Name,
                special2 = record.Title,
                special3 = TrimForNative(record.Body)
            };
            WorldLogMessageExtensions.add(message);
            return true;
        }
        catch (Exception ex)
        {
            _disabled = true;
            Debug.LogWarning("[模拟长生路] 原生历史记录桥接关闭: " + ex.Message);
            return false;
        }
    }

    private static void EnsureAssets()
    {
        if (_initialized && AssetsStillRegistered()) return;
        bool anyRegistered = false;
        for (int i = 1; i < MclslEventCatalog.Categories.Length; i++)
        {
            MclslEventCategoryDefinition category = MclslEventCatalog.Categories[i];
            anyRegistered |= EnsureGroup(category);
            anyRegistered |= EnsureWorldLogAsset(category);
        }
        _initialized = anyRegistered && AssetsStillRegistered();
    }

    private static bool EnsureGroup(MclslEventCategoryDefinition category)
    {
        try
        {
            string id = "mclsl_" + category.Id;
            HistoryGroupAsset group = AssetManager.history_groups.get(id);
            if (group != null)
            {
                group.icon_path = category.IconPath;
                return true;
            }
            group = new HistoryGroupAsset
            {
                id = id,
                icon_path = category.IconPath
            };
            AssetManager.history_groups.add(group);
            return true;
        }
        catch { return false; }
    }

    private static bool EnsureWorldLogAsset(MclslEventCategoryDefinition category)
    {
        try
        {
            string id = AssetPrefix + category.Id;
            WorldLogAsset asset = AssetManager.world_log_library.get(id);
            if (asset == null)
            {
                asset = new WorldLogAsset { id = id };
                AssetManager.world_log_library.add(asset);
            }
            asset.group = "mclsl_" + category.Id;
            asset.locale_id = id;
            asset.path_icon = category.IconPath;
            asset.color = ParseColor(category.Color);
            asset.text_replacer = FormatLogText;
            return true;
        }
        catch { return false; }
    }

    private static bool AssetsStillRegistered()
    {
        try
        {
            for (int i = 1; i < MclslEventCatalog.Categories.Length; i++)
            {
                string id = MclslEventCatalog.Categories[i].Id;
                if (AssetManager.history_groups.get("mclsl_" + id) == null) return false;
                if (AssetManager.world_log_library.get(AssetPrefix + id) == null) return false;
            }
            return true;
        }
        catch { return false; }
    }

    private static void FormatLogText(WorldLogMessage message, ref string text)
    {
        // Legacy native messages have no event ID. Match all retained fields,
        // old timestamp and owning run; never infer type from title keywords.
        if (ReferenceEquals(_legacyRun, MclslWorldRunRepository.Current)
            && message.asset_id == AssetPrefix + MclslEventCatalog.Ruin
            && LegacyLineages.Contains((message.timestamp, message.special2, message.special3)))
        {
            message.asset_id = AssetPrefix + MclslEventCatalog.Lineage;
            message.special1 = MclslEventCatalog.Category(MclslEventCatalog.Lineage).Name;
            message.timestamp = MclslEventCatalog.NativeTimestampForYear(message.timestamp);
        }
        string category = string.IsNullOrWhiteSpace(message.special1) ? "玄黄" : message.special1;
        string title = string.IsNullOrWhiteSpace(message.special2) ? "玄黄异动" : message.special2;
        string body = string.IsNullOrWhiteSpace(message.special3) ? string.Empty : "：" + message.special3;
        text = "[玄黄·" + category + "] " + TrimForNative(title + body);
    }

    private static string TrimForNative(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return string.Empty;
        string text = Sanitize(body.Trim());
        return text.Length <= 42 ? text : text.Substring(0, 42) + "...";
    }

    private static string Sanitize(string text)
    {
        return (text ?? string.Empty)
            .Replace("灵根仙道延续千年，灵根、功法、真元、神魂与大道感悟构成旧修行秩序；天地将变，上古仙道至此入终纪。", "千年仙道，至此风雨满楼。")
            .Replace("传法天尊逆天地之理，开创新法。自此众生无须天赐灵根，亦可炼气登仙。", "传法立道，天下始闻新法。")
            .Replace("万仙盟依传法新法建立跨国背景秩序，提供标准功法、贡献体系、遗迹任务与修行资源调配，但不取代原生国家。", "万仙盟传檄诸国，新法自此有统。")
            .Replace("变世之后，旧法修士不会自动补成新法修士。旧法遗修保留境界与寿元，但不再使用筑基奇物、洞天、天地之变与天地之魄。", "前代仙修或隐或散，山河间旧影渐稀。")
            .Replace("五位长生天尊先后逆天地之理，结成五老会。第五席逆理暂未揭示，只作为原著背景席位记录。", "五老同席，暗潮入世。")
            .Replace("传法变世十年后，旧秩序基本崩解。万仙盟仍为第一大背景势力，五老会自暗处开始与其长期争衡。", "旧山河远去，新法定世。")
            .Replace("魄核归属开始判定。", string.Empty)
            .Replace("天地法则由此激变。", string.Empty);
    }

    private static Color ParseColor(string color)
    {
        return ColorUtility.TryParseHtmlString(color, out Color parsed) ? parsed : new Color(0.8f, 0.78f, 0.68f);
    }
}
