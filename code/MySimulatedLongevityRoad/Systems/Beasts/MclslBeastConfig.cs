using System;
using System.Collections.Generic;
using System.Reflection;
using MySimulatedLongevityRoad.Data;
using NeoModLoader.api;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBeastConfig
{
    private const string EnableId = "MCLSL_config_beast_natural_spawn";
    internal static bool NaturalSpawnEnabled { get; private set; } = true;
    internal const int RedMoonChancePerMillion = 10000;
    internal const int RevivalChancePerMillion = 100;

    internal static void Ensure(ModConfig config)
    {
        if (config == null) return;
        Dictionary<string, ModConfigItem> items = config["ConfigItems"];
        AddSwitch(config, items, EnableId, "妖兽自然出现", "常见妖兽每五年按栖息地补充；稀有妖兽每年独立判定。", true);
        AddSwitch(config,items,"MCLSL_config_beast_waves","妖兽潮","按世界境界解锁七级兽潮。",true);
        AddSwitch(config,items,"MCLSL_config_spell_effects_enabled","法术阶段特效","显示法术释放、飞行和命中特效。关闭后仍按正常时间结算。",true);
        AddSlider(config,items,"MCLSL_config_beast_wave_interval","兽潮间隔（年）","默认五年。",5);
        AddSlider(config,items,"MCLSL_config_beast_wave_count_percent","兽潮数量（百分比）","默认100，即一倍。",100);
        AddSlider(config,items,"MCLSL_config_beast_wave_limit","单次兽潮数量上限","按地图规模、等级及数量倍率计算后，单场最多生成的数量，默认64。",64);
        AddSlider(config,items,"MCLSL_config_beast_wave_max_level","自动兽潮最高等级","一至七级，还受世界解锁限制。",7);
        AddSlider(config,items,"MCLSL_config_beast_auto_limit","自动妖兽总上限","自然与兽潮共享上限。",512);
        AddSlider(config,items,"MCLSL_config_beast_natural_limit","自然妖兽上限","默认128。",128);
        Load(config);
    }

    internal static void Load(ModConfig config)
    {
        if (config == null) return;
        Dictionary<string, ModConfigItem> items = config["ConfigItems"];
        bool removedProbabilitySettings = items.Remove("MCLSL_config_beast_red_moon_chance")
            | items.Remove("MCLSL_config_beast_revival_chance");
        foreach (MclslBeastDefinition beast in MclslBeastCatalog.All)
            removedProbabilitySettings |= items.Remove(beast.ConfigId);
        foreach (MclslBeastDefinition beast in MclslBeastCatalog.Named)
            removedProbabilitySettings |= items.Remove(beast.ConfigId);
        NaturalSpawnEnabled = !items.TryGetValue(EnableId, out ModConfigItem enabled) || enabled.BoolVal;
        MclslBeastWorld.Enabled=!items.TryGetValue("MCLSL_config_beast_waves",out var waves)||waves.BoolVal;
        MclslSpellEffect.SetEnabled(!items.TryGetValue("MCLSL_config_spell_effects_enabled",out var effects)||effects.BoolVal);
        MclslBeastWorld.Interval=Math.Clamp(ReadChance(items,"MCLSL_config_beast_wave_interval",5),1,1000);
        MclslBeastWorld.CountMultiplier=Math.Clamp(ReadChance(items,"MCLSL_config_beast_wave_count_percent",100),10,1000)/100f;
        MclslBeastWorld.MaximumWaveCount=Math.Clamp(ReadChance(items,"MCLSL_config_beast_wave_limit",64),1,256);
        MclslBeastWorld.MaximumLevel=Math.Clamp(ReadChance(items,"MCLSL_config_beast_wave_max_level",7),1,7);
        MclslBeastWorld.AutomaticLimit=Math.Clamp(ReadChance(items,"MCLSL_config_beast_auto_limit",512),1,4096);
        MclslBeastWorld.NaturalLimit=Math.Clamp(ReadChance(items,"MCLSL_config_beast_natural_limit",128),1,4096);
        if (removedProbabilitySettings) config.Save(null);
    }

    internal static int Chance(MclslBeastDefinition beast)
        => beast?.ChancePerMillion ?? 0;

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
        if(id=="MCLSL_config_beast_wave_max_level"){item.SetIntRange(1,7);return;}
        if(id=="MCLSL_config_beast_wave_interval"){item.SetIntRange(1,1000);return;}
        if(id=="MCLSL_config_beast_wave_count_percent"){item.SetIntRange(10,1000);return;}
        if(id=="MCLSL_config_beast_wave_limit"){item.SetIntRange(1,256);return;}
        if(id is "MCLSL_config_beast_auto_limit" or "MCLSL_config_beast_natural_limit"){item.SetIntRange(1,4096);return;}
        typeof(ModConfigItem).GetField("<MinIntVal>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(item, 0);
        typeof(ModConfigItem).GetField("<MaxIntVal>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(item, 1000000);
    }
}
