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
        AddSwitch(config, items, EnableId, "妖兽自然出现", "常见妖兽每五年按栖息地补充；稀有妖兽每年独立判定。", true);
        AddSlider(config, items, RedMoonId, "红月事件概率（百万分之）", "默认10000，即每年1%。", 10000);
        AddSlider(config, items, RevivalId, "高山复苏事件概率（百万分之）", "默认100，即每年0.01%；成功时再判定兲兽。", 100);
        AddSwitch(config,items,"MCLSL_config_beast_waves","妖兽潮","按世界境界解锁七级兽潮。",true);
        AddSwitch(config,items,"MCLSL_config_spell_effects_enabled","法术阶段特效","显示法术释放、飞行和命中特效。关闭后仍按正常时间结算。",true);
        AddSlider(config,items,"MCLSL_config_beast_wave_interval","兽潮间隔（年）","默认五年。",5);
        AddSlider(config,items,"MCLSL_config_beast_wave_count_percent","兽潮数量（百分比）","默认100，即一倍。",100);
        AddSlider(config,items,"MCLSL_config_beast_wave_max_level","自动兽潮最高等级","一至七级，还受世界解锁限制。",7);
        AddSlider(config,items,"MCLSL_config_beast_auto_limit","自动妖兽总上限","自然与兽潮共享上限。",512);
        AddSlider(config,items,"MCLSL_config_beast_natural_limit","自然妖兽上限","默认128。",128);
        foreach (MclslBeastDefinition beast in MclslBeastCatalog.All)
        {
            bool variant = beast.Id == "moqilin";
            bool common=MclslBeastRules.CommonSpecies.Contains(beast.Id);
            AddSlider(config, items, beast.ConfigId,
                beast.Name + (common?"自然出现权重":variant ? "麒麟变种概率（百万分之）" : "自然出现概率（百万分之）"),
                common?"同一区域的常见妖兽按权重选择；每五年补充一次，受境界和种群上限限制。":variant ? "麒麟出现成功后判定；默认100000，即10%。"
                    : beast.Id == "tianshou" ? "仅高山复苏事件成功后判定；默认1000000，即100%。"
                    : "每年一次；达到区域或世界自然妖兽上限时跳过。", beast.ChancePerMillion);
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
        MclslBeastWorld.Enabled=!items.TryGetValue("MCLSL_config_beast_waves",out var waves)||waves.BoolVal;
        MclslSpellEffect.SetEnabled(!items.TryGetValue("MCLSL_config_spell_effects_enabled",out var effects)||effects.BoolVal);
        MclslBeastWorld.Interval=Math.Clamp(ReadChance(items,"MCLSL_config_beast_wave_interval",5),1,1000);
        MclslBeastWorld.CountMultiplier=Math.Clamp(ReadChance(items,"MCLSL_config_beast_wave_count_percent",100),10,1000)/100f;
        MclslBeastWorld.MaximumLevel=Math.Clamp(ReadChance(items,"MCLSL_config_beast_wave_max_level",7),1,7);
        MclslBeastWorld.AutomaticLimit=Math.Clamp(ReadChance(items,"MCLSL_config_beast_auto_limit",512),1,4096);
        MclslBeastWorld.NaturalLimit=Math.Clamp(ReadChance(items,"MCLSL_config_beast_natural_limit",128),1,4096);
        foreach (MclslBeastDefinition beast in MclslBeastCatalog.All)
            Chances[beast.Id] = ReadChance(items, beast.ConfigId, beast.ChancePerMillion);
    }

    internal static int Chance(MclslBeastDefinition beast)
        => beast != null && Chances.TryGetValue(beast.Id, out int value) ? value : beast?.ChancePerMillion ?? 0;

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
        if(id is "MCLSL_config_beast_auto_limit" or "MCLSL_config_beast_natural_limit"){item.SetIntRange(1,4096);return;}
        typeof(ModConfigItem).GetField("<MinIntVal>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(item, 0);
        typeof(ModConfigItem).GetField("<MaxIntVal>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(item, 1000000);
    }
}
