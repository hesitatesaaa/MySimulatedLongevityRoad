using System;
using System.Collections.Generic;
using System.Reflection;
using MySimulatedLongevityRoad.Data;
using NeoModLoader.api;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBeastConfig
{
    internal static void Ensure(ModConfig config)
    {
        if (config == null) return;
        Dictionary<string, ModConfigItem> items = config["ConfigItems"];
        AddSwitch(new AddSwitchArgs { Config = config, Items = items, Id = "MCLSL_config_beast_waves", Name = "妖兽潮", Description = "按世界境界解锁七级兽潮。", Initial = true });
        AddSwitch(new AddSwitchArgs { Config = config, Items = items, Id = "MCLSL_config_spell_effects_enabled", Name = "法术阶段特效", Description = "显示法术释放、飞行和命中特效。关闭后仍按正常时间结算。", Initial = true });
        AddSwitch(new AddSwitchArgs { Config = config, Items = items, Id = "MCLSL_config_beast_talent_effects_enabled", Name = "本命神通特效", Description = "独立显示妖兽本命神通动画；关闭只隐藏画面，不影响结算。", Initial = true });
        AddSlider(new AddSliderArgs { Config = config, Items = items, Id = "MCLSL_config_beast_wave_interval", Name = "兽潮间隔（年）", Description = "默认五年。", Initial = 5 });
        AddSlider(new AddSliderArgs { Config = config, Items = items, Id = "MCLSL_config_beast_wave_count_percent", Name = "兽潮数量（百分比）", Description = "默认100，即一倍。", Initial = 100 });
        AddSlider(new AddSliderArgs { Config = config, Items = items, Id = "MCLSL_config_beast_wave_limit", Name = "单次兽潮数量上限", Description = "按地图规模、等级及数量倍率计算后，单场最多生成的数量，默认64。", Initial = 64 });
        AddSlider(new AddSliderArgs { Config = config, Items = items, Id = "MCLSL_config_beast_wave_max_level", Name = "自动兽潮最高等级", Description = "一至七级，还受世界解锁限制。", Initial = 7 });
        AddSlider(new AddSliderArgs { Config = config, Items = items, Id = "MCLSL_config_beast_auto_limit", Name = "自动妖兽总上限", Description = "兽潮与历史自动生成妖兽共享存量上限。", Initial = 512 });
        MigrateLegacySettings(config);
        Load(config);
    }

    private static void MigrateLegacySettings(ModConfig config)
    {
        if (config == null) return;
        Dictionary<string, ModConfigItem> items = config["ConfigItems"];
        bool removedProbabilitySettings = items.Remove("MCLSL_config_beast_natural_spawn")
            | items.Remove("MCLSL_config_beast_natural_limit")
            | items.Remove("MCLSL_config_beast_red_moon_chance")
            | items.Remove("MCLSL_config_beast_revival_chance");
        foreach (MclslBeastDefinition beast in MclslBeastCatalog.All)
            removedProbabilitySettings |= items.Remove("MCLSL_config_beast_" + beast.Id + "_chance");
        foreach (MclslBeastDefinition beast in MclslBeastCatalog.Named)
            removedProbabilitySettings |= items.Remove("MCLSL_config_beast_" + beast.Id + "_chance");
        if (removedProbabilitySettings) config.Save(null);
    }

    // Migration belongs to startup; reads used by settings transactions have no
    // disk writes, config removals, or live-effect changes.
    internal readonly struct Settings
    {
        internal readonly bool Waves, Effects, Talents;
        internal readonly int Interval, WaveLimit, Level, AutomaticLimit;
        internal readonly float Multiplier;
        internal Settings(Dictionary<string, ModConfigItem> items)
        {
            Waves = !items.TryGetValue("MCLSL_config_beast_waves", out var waves) || waves.BoolVal;
            Effects = !items.TryGetValue("MCLSL_config_spell_effects_enabled", out var effects) || effects.BoolVal;
            Talents = !items.TryGetValue("MCLSL_config_beast_talent_effects_enabled", out var talents) || talents.BoolVal;
            Interval = Math.Clamp(ReadChance(items, "MCLSL_config_beast_wave_interval", 5), 1, 1000);
            Multiplier = Math.Clamp(ReadChance(items, "MCLSL_config_beast_wave_count_percent", 100), 10, 1000) / 100f;
            WaveLimit = Math.Clamp(ReadChance(items, "MCLSL_config_beast_wave_limit", 64), 1, 256);
            Level = Math.Clamp(ReadChance(items, "MCLSL_config_beast_wave_max_level", 7), 1, 7);
            AutomaticLimit = Math.Clamp(ReadChance(items, "MCLSL_config_beast_auto_limit", 512), 1, 4096);
        }
    }

    internal static Settings Read(ModConfig config) => new(config["ConfigItems"]);

    internal static void Load(ModConfig config)
    {
        if (config != null) Apply(Read(config));
    }

    internal static void Apply(Settings settings)
    {
        MclslBeastWorld.Enabled = settings.Waves;
        MclslBeastWorld.Interval = settings.Interval;
        MclslBeastWorld.CountMultiplier = settings.Multiplier;
        MclslBeastWorld.MaximumWaveCount = settings.WaveLimit;
        MclslBeastWorld.MaximumLevel = settings.Level;
        MclslBeastWorld.AutomaticLimit = settings.AutomaticLimit;
        MclslSpellEffect.SetEnabled(settings.Effects);
        MclslSpellEffect.SetTalentEnabled(settings.Talents);
    }

    public static void ReloadBool(bool _) => Load(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe() as ModConfig);
    internal static void SetSwitch(string id,bool value)
    {
        if(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe() is not ModConfig config || !config["ConfigItems"].TryGetValue(id,out var item))return;
        item.SetValue(value,false);Load(config);config.Save(null);
    }
    internal static void SetInt(string id,int value)
    {
        if(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe() is not ModConfig config || !config["ConfigItems"].TryGetValue(id,out var item))return;
        item.SetValue(value,false);Load(config);config.Save(null);
    }
    public static void ReloadInt(int _) => Load(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe() as ModConfig);

    private static int ReadChance(Dictionary<string, ModConfigItem> items, string id, int fallback)
        => items.TryGetValue(id, out ModConfigItem item) ? Math.Clamp(item.IntVal, 0, 1000000) : fallback;

    private static void AddSwitch(AddSwitchArgs a)
    {
        ModConfig config = a.Config;
        Dictionary<string, ModConfigItem> items = a.Items;
        string id = a.Id;
        string name = a.Name;
        string description = a.Description;
        bool initial = a.Initial;
        MclslLocalizationBridge.RegisterKey(id, name);
        MclslLocalizationBridge.RegisterKey(id + " Description", description);
        if (!items.ContainsKey(id))
            config.AddConfigItem("ConfigItems", id, ConfigItemType.SWITCH, initial, null,
                "MclslBeastConfig:ReloadBool");
    }

    private static void AddSlider(AddSliderArgs a)
    {
        ModConfig config = a.Config;
        Dictionary<string, ModConfigItem> items = a.Items;
        string id = a.Id;
        string name = a.Name;
        string description = a.Description;
        int initial = a.Initial;
        MclslLocalizationBridge.RegisterKey(id, name);
        MclslLocalizationBridge.RegisterKey(id + " Description", description);
        if (!items.TryGetValue(id, out ModConfigItem item))
            item = config.AddConfigItem("ConfigItems", id, ConfigItemType.INT_SLIDER, initial, null,
                "MclslBeastConfig:ReloadInt");
        if (item == null) return;
        if(id=="MCLSL_config_beast_wave_max_level"){item.SetIntRange(1,7);return;}
        if(id=="MCLSL_config_beast_wave_interval"){item.SetIntRange(1,1000);return;}
        if(id=="MCLSL_config_beast_wave_count_percent"){item.SetIntRange(10,1000);return;}
        if(id=="MCLSL_config_beast_wave_limit"){item.SetIntRange(1,256);return;}
        if(id == "MCLSL_config_beast_auto_limit"){item.SetIntRange(1,4096);return;}
        typeof(ModConfigItem).GetField("<MinIntVal>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(item, 0);
        typeof(ModConfigItem).GetField("<MaxIntVal>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(item, 1000000);
    }
}

internal sealed class AddSwitchArgs
{
    internal ModConfig Config;
    internal Dictionary<string, ModConfigItem> Items;
    internal string Id;
    internal string Name;
    internal string Description;
    internal bool Initial;
}

internal sealed class AddSliderArgs
{
    internal ModConfig Config;
    internal Dictionary<string, ModConfigItem> Items;
    internal string Id;
    internal string Name;
    internal string Description;
    internal int Initial;
}
