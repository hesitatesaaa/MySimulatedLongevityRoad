using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal static class MclslUiManager
{
    private static bool ShowAuthorDebugButtons => MclslHotPathPolicy.ShouldExposeDeveloperTools();

    private static bool _initialized;
    private static PowersTab _tab;

    internal static void Init()
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            Sprite icon = SpriteTextureLoader.getSprite("ui/icon") ?? SpriteTextureLoader.getSprite("ui/Icons/XuanHuangXianLu") ?? SpriteTextureLoader.getSprite("ui/icons/iconBook");
            MclslLocalizationBridge.RegisterKey("mclsl_mod_tab", "我的模拟长生路");
            MclslLocalizationBridge.RegisterKey("mclsl_mod_tab Description", "新法修行、玄黄仙录与轮回档案");
            _tab = TabManager.CreateTab("mclsl_mod_tab", "mclsl_mod_tab", "mclsl_mod_tab Description", icon, "hotkey_tip_tab_other");
            if (_tab == null) throw new InvalidOperationException("无法创建模拟长生路功能页签");
            PowersTabExtension.SetLayout(_tab, new System.Collections.Generic.List<string> { "tab" });
            AddButton("mclsl.codex", "玄黄仙录", "查看新法阶段、天地资源、宗门遗迹、修士生死与世界纪事。", MclslCodexWindow.Show, "ui/Icons/XuanHuangXianLu", "ui/icon", "ui/icons/iconBook");
            AddButton("mclsl.rank", "玄黄修士榜", "按玄鉴式榜单筛选、排序和查看本世修士。", MclslRankWindow.ShowWindow, "ui/Icons/XuanHuangXiuShiBang", "ui/Icons/TianDiZhiLi", "ui/icon");
            if (ShowAuthorDebugButtons)
            {
                AddButton("mclsl.force_transmission", "传法变世测试", "直接触发传法天尊证道，使本世从仙道纪元进入传法变世阶段，便于测试。", ForceTransmissionForTesting, "ui/Icons/TianDiZhiLi", "ui/Icons/XuanHuangXianLu", "ui/icon");
                AddButton("mclsl.trigger_ancient_disaster", "触发灵机灾变", "手动记录一次仙道灵机、地火或星石事件，便于测试仙道机缘。", TriggerAncientDisaster, "ui/Icons/TianDiZhiBian", "ui/icon");
                AddButton("mclsl.trigger_ancient_secret_realm", "触发秘境", "手动记录一次秘境开启，便于测试仙道事件页。", TriggerAncientSecretRealm, "ui/Icons/ZongMenYiJi", "ui/icon");
                AddButton("mclsl.trigger_ancient_ruin", "触发遗府", "手动生成一处遗府并写入玄黄仙录。", TriggerAncientRuin, "ui/Icons/ZongMenYiJi", "ui/icon");
                AddWorldSoulButtons();
            }
            AddButton("mclsl.settings", "模拟长生路设置", "调整新法修行、历史锚点、死亡公告、遗迹游历与还真参数。", ShowSettings, "ui/icons/iconOptions", "ui/icons/iconBook");
        }
        catch (Exception ex)
        {
            _initialized = false;
            Debug.LogError("[模拟长生路][UI] 初始化失败: " + ex);
        }
    }

    private static void ShowSettings()
    {
        if (MySimulatedLongevityRoad.MclslMod.I != null)
            NeoModLoader.ui.ModConfigureWindow.ShowWindow(MySimulatedLongevityRoad.MclslMod.I.GetConfig());
    }

    private static void ForceTransmissionForTesting()
    {
        MclslWorldEpochSystem.ForceTransmissionForTesting(MclslRuntime.CurrentYear());
    }

    private static void TriggerAncientDisaster()
    {
        MclslWorldRunRepository.EnsureCurrentRun(MclslRuntime.CurrentYear());
        MclslAncientLawEventSystem.ManualTriggerSpiritualDisaster(MclslRuntime.CurrentYear());
    }

    private static void TriggerAncientSecretRealm()
    {
        MclslWorldRunRepository.EnsureCurrentRun(MclslRuntime.CurrentYear());
        MclslAncientLawEventSystem.ManualTriggerSecretRealm(MclslRuntime.CurrentYear());
    }

    private static void TriggerAncientRuin()
    {
        MclslWorldRunRepository.EnsureCurrentRun(MclslRuntime.CurrentYear());
        MclslAncientLawEventSystem.ManualTriggerAncientRuin(MclslRuntime.CurrentYear());
    }

    private static void AddWorldSoulButtons()
    {
        AddWorldSoulButton("metal", "金魄", "soul_attr_metal", "Jin_Soul");
        AddWorldSoulButton("wood", "木魄", "soul_attr_wood", "Wood_Soul");
        AddWorldSoulButton("water", "水魄", "soul_attr_water", "Water_Soul");
        AddWorldSoulButton("fire", "火魄", "soul_attr_fire", "Fire_Soul");
        AddWorldSoulButton("earth", "土魄", "soul_attr_earth", "Earth_Soul");
        AddWorldSoulButton("wind", "风魄", "soul_attr_wind", "Wind_Soul");
        AddWorldSoulButton("thunder", "雷魄", "soul_attr_thunder", "Thunder_Soul");
        AddWorldSoulButton("yin", "阴魄", "soul_attr_yin", "Yin_Soul");
        AddWorldSoulButton("yang", "阳魄", "soul_attr_yang", "Yang_Soul");
        AddWorldSoulButton("space", "空魄", "soul_attr_space", "Kong_Soul");
    }

    private static void AddWorldSoulButton(string buttonSuffix, string displayName, string soulId, string soulFolder)
    {
        AddButton(
            "mclsl.spawn_soul_" + buttonSuffix,
            "生成" + displayName,
            "牵引天地之魄·" + displayName + "临世。",
            () => MclslWorldSoulSystem.TryManualManifest(soulId),
            "Souls/" + soulFolder + "/Idle/1",
            "actors/Souls/" + soulFolder + "/Idle/1",
            "ui/Icons/TianDiZhiPo",
            "ui/icon");
    }

    private static void AddButton(string id, string title, string description, Action action, params string[] icons)
    {
        if (_tab == null) return;
        Sprite sprite = null;
        foreach (string path in icons) { sprite = SpriteTextureLoader.getSprite(path); if (sprite != null) break; }
        PowerButton button = PowerButtonCreator.CreateSimpleButton(id, () => { try { PowerButtonSelector.instance?.unselectAll(); } catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslUiManager-cs-1", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslUiManager.cs #1: " + mclslEmptyCatchEx.Message); } action?.Invoke(); }, sprite, null, default);
        SetupButtonTooltip(button, id, title, description);
        PowersTabExtension.AddPowerButton(_tab, "tab", button);
    }

    private static void SetupButtonTooltip(PowerButton button, string id, string title, string description)
    {
        try
        {
            if (button == null) return;
            string titleKey = id;
            string descriptionKey = id + " Description";
            MclslLocalizationBridge.RegisterKey(titleKey, title);
            MclslLocalizationBridge.RegisterKey(descriptionKey, description);
            TipButton tip = button.GetComponent<TipButton>();
            if (tip == null) tip = button.gameObject.AddComponent<TipButton>();
            tip.textOnClick = titleKey;
            tip.textOnClickDescription = descriptionKey;
        }
        catch (System.Exception mclslEmptyCatchEx) { MySimulatedLongevityRoad.Core.MclslDiagnostics.Error("empty-catch-code-MySimulatedLongevityRoad-UI-MclslUiManager-cs-2", "空 catch 捕获: code/MySimulatedLongevityRoad/UI/MclslUiManager.cs #2: " + mclslEmptyCatchEx.Message); }
    }
}
