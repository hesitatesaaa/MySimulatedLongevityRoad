using System;
using System.Collections.Generic;
using NeoModLoader.api;

namespace MySimulatedLongevityRoad.Core;

public static class MclslConfig
{
    public static void AnnualBackpressureCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableAggressivePerformanceCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    internal static void EnsurePhysiqueAutoCollectItem(ModConfig config)
    {
        if (config == null) return;
        const string id = "MCLSL_config_auto_collect_physique";
        Dictionary<string, ModConfigItem> items = config["ConfigItems"];
        if (items.ContainsKey(id)) return;
        ModConfigItem added = config.AddConfigItem("ConfigItems", id, ConfigItemType.SWITCH, true,
            null, "MclslConfig:AutoCollectPhysiqueCallBack");
        if (added == null) throw new InvalidOperationException("无法添加玄黄异禀自动收藏设置");
        // Older user config files do not contain the new item. Keep its visible order
        // next to the Maobao favorite setting when migrating that in-memory config.
        var ordered = new List<KeyValuePair<string, ModConfigItem>>(items);
        items.Clear();
        bool inserted = false;
        foreach (KeyValuePair<string, ModConfigItem> pair in ordered)
        {
            if (pair.Key == id) continue;
            items.Add(pair.Key, pair.Value);
            if (pair.Key != "MCLSL_config_auto_collect_maobao_inscription") continue;
            items.Add(id, added);
            inserted = true;
        }
        if (!inserted) items.Add(id, added);
    }
    public static void ChildhoodRootChanceCallBack(int value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void AlchemistChanceCallBack(int value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void RefinerChanceCallBack(int value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void TalismanChanceCallBack(int value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void PerformanceMonitorCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void ShowFpsCallBack(bool value)
    {
        MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
        MySimulatedLongevityRoad.UI.MclslFpsOverlay.SetVisible(MclslRuntimeSettings.ShowFps);
    }
    public static void ItemAcquisitionHistoryCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void RecordLowMaterialAcquisitionHistoryCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());

    public static void EnableDeathAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableBreakthroughFailureAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableYuanYingBreakthroughAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableHuaShenBreakthroughAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableWorldSoulAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableRuinDeathAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void DeathPopupMinRealmCallBack(int value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableWorldAdventuresCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableFactionCommissionsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableAncientLineageAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void LineageNativeHistoryCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnablePioneerAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableMinorWorldAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableRuinBirthAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableResourceBirthAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableFactionPolicyAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableSurvivalAnnouncementsCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void EnableHuanzhenCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void AutoHuanzhenAnchorCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void HuanzhenAnchorIntervalCallBack(int value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void HuanzhenSafetyGapCallBack(int value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void AutoCollectHuanzhenHostCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void AutoCollectMaobaoInscriptionCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void AutoCollectPhysiqueCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void TransmissionYearCallBack(int value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void AutoCollectYuanYingCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void AutoCollectHuaShenCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void AutoCollectHeDaoCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void AutoCollectChangShengCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void AutoCollectPureRootCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
    public static void AutoCollectHeavenRootCallBack(bool value) => MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
}
