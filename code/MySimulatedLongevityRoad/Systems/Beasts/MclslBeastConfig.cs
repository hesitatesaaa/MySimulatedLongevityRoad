using System;
using System.Collections.Generic;
using System.Reflection;
using MySimulatedLongevityRoad.Data;
using NeoModLoader.api;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBeastConfig
{
    private const string EnableId = "MCLSL_config_beast_natural_spawn";
    private const string RedMoonId = "MCLSL_config_beast_red_moon_chance";
    private const string RevivalId = "MCLSL_config_beast_revival_chance";
    private static readonly Dictionary<string, int> Chances = new(StringComparer.Ordinal);
    internal static bool NaturalSpawnEnabled { get; private set; } = true;
    internal static int RedMoonChancePerMillion { get; private set; } = 10000;
    internal static int RevivalChancePerMillion { get; private set; } = 100;

    internal static void Ensure(ModConfig config)
    {
        if (config == null) return;
        Dictionary<string, ModConfigItem> items = config["ConfigItems"];
        AddSwitch(config, items, EnableId, "妖兽自然出现", "每个世界年按各妖兽概率判定一次。", true);
        AddSlider(config, items, RedMoonId, "红月事件概率（百万分之）", "默认10000，即每年1%。", 10000);
        AddSlider(config, items, RevivalId, "高山复苏事件概率（百万分之）", "默认100，即每年0.01%；成功时再判定兲兽。", 100);
        foreach (MclslBeastDefinition beast in MclslBeastCatalog.All)
        {
            bool variant = beast.Id == "moqilin";
            AddSlider(config, items, beast.ConfigId,
                beast.Name + (variant ? "麒麟变种概率（百万分之）" : "自然出现概率（百万分之）"),
                variant ? "麒麟出现成功后判定；默认100000，即10%。"
                    : beast.Id == "tianshou" ? "仅高山复苏事件成功后判定；默认1000000，即100%。"
                    : "每年一次；达到当前原版亚种种群限制时跳过。", beast.ChancePerMillion);
        }
        Load(config);
    }

    internal static void Load(ModConfig config)
    {
        if (config == null) return;
        Dictionary<string, ModConfigItem> items = config["ConfigItems"];
        NaturalSpawnEnabled = !items.TryGetValue(EnableId, out ModConfigItem enabled) || enabled.BoolVal;
        RedMoonChancePerMillion = ReadChance(items, RedMoonId, 10000);
        RevivalChancePerMillion = ReadChance(items, RevivalId, 100);
        foreach (MclslBeastDefinition beast in MclslBeastCatalog.All)
            Chances[beast.Id] = ReadChance(items, beast.ConfigId, beast.ChancePerMillion);
    }

    internal static int Chance(MclslBeastDefinition beast)
        => beast != null && Chances.TryGetValue(beast.Id, out int value) ? value : beast?.ChancePerMillion ?? 0;

    public static void ReloadBool(bool _) => Load(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe() as ModConfig);
    public static void ReloadInt(int _) => Load(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe() as ModConfig);

    private static int ReadChance(Dictionary<string, ModConfigItem> items, string id, int fallback)
        => items.TryGetValue(id, out ModConfigItem item) ? Math.Clamp(item.IntVal, 0, 1000000) : fallback;

    private static void AddSwitch(ModConfig config, Dictionary<string, ModConfigItem> items,
        string id, string name, string description, bool initial)
    {
        MclslLocalizationBridge.RegisterKey(id, name);
        MclslLocalizationBridge.RegisterKey(id + " Description", description);
        if (!items.ContainsKey(id))
            config.AddConfigItem("ConfigItems", id, ConfigItemType.SWITCH, initial, null,
                "MclslBeastConfig:ReloadBool");
    }

    private static void AddSlider(ModConfig config, Dictionary<string, ModConfigItem> items,
        string id, string name, string description, int initial)
    {
        MclslLocalizationBridge.RegisterKey(id, name);
        MclslLocalizationBridge.RegisterKey(id + " Description", description);
        if (!items.TryGetValue(id, out ModConfigItem item))
            item = config.AddConfigItem("ConfigItems", id, ConfigItemType.INT_SLIDER, initial, null,
                "MclslBeastConfig:ReloadInt");
        if (item == null) return;
        typeof(ModConfigItem).GetField("<MinIntVal>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(item, 0);
        typeof(ModConfigItem).GetField("<MaxIntVal>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(item, 1000000);
    }
}
