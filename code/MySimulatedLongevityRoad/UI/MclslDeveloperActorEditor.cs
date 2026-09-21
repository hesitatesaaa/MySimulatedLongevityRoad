using System;
using UnityEngine;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.UI;

internal sealed class MclslDeveloperActorEditor : MonoBehaviour
{
    private static MclslDeveloperActorEditor _instance;
    private Actor _actor;
    private Rect _rect = new(420f, 90f, 470f, 760f);
    private string _realm = string.Empty;
    private string _trueEssence = string.Empty;
    private string _aptitude = string.Empty;
    private string _mind = string.Empty;
    private string _contribution = string.Empty;
    private string _spiritStones = string.Empty;
    private string _spaceEssence = string.Empty;
    private string _cultivationSystem = string.Empty;
    private string _technique = string.Empty;
    private string _worldSoul = string.Empty;
    private string _inverseTruth = string.Empty;
    private string _heartProgress = string.Empty;
    private string _foundationWonder = string.Empty;
    private string _goldenCoreLaws = string.Empty;
    private string _nascentCave = string.Empty;
    private string _divineChange = string.Empty;
    private string _divineMarrow = string.Empty;
    private string _huanzhenAnchor = string.Empty;
    private string _huanzhenRestoreCount = string.Empty;
    private string _customKey = string.Empty;
    private string _customValue = string.Empty;
    private string _message = string.Empty;

    internal static void Show(Actor actor)
    {
        if (actor?.data == null || !MclslRuntimeSettings.DebugToolsVisible) return;
        if (_instance == null)
        {
            GameObject host = new("MclslDeveloperActorEditor");
            UnityEngine.Object.DontDestroyOnLoad(host);
            _instance = host.AddComponent<MclslDeveloperActorEditor>();
        }
        _instance._actor = actor;
        _instance.LoadFields();
        _instance.enabled = true;
    }

    private void LoadFields()
    {
        _realm = MclslActorAccessor.Realm(_actor) ?? string.Empty;
        _trueEssence = MclslActorAccessor.GetInt(_actor, MclslActorDataKeys.TrueEssence, 0).ToString();
        _aptitude = MclslActorAccessor.GetInt(_actor, MclslActorDataKeys.Aptitude, 50).ToString();
        _mind = MclslActorAccessor.GetInt(_actor, MclslActorDataKeys.MindState, 0).ToString();
        _contribution = MclslActorAccessor.GetInt(_actor, MclslActorDataKeys.Contribution, 0).ToString();
        _spiritStones = MclslActorAccessor.GetInt(_actor, MclslActorDataKeys.SpiritStones, 0).ToString();
        _spaceEssence = MclslHuanzhenSystem.CurrentSpaceEssence().ToString();
        _cultivationSystem = MclslActorAccessor.GetString(_actor, MclslActorDataKeys.CultivationSystem, string.Empty);
        _technique = MclslActorAccessor.GetString(_actor, MclslActorDataKeys.TechniqueName, string.Empty);
        _worldSoul = MclslActorAccessor.GetString(_actor, MclslActorDataKeys.WorldSoulName, string.Empty);
        _inverseTruth = MclslActorAccessor.GetString(_actor, MclslActorDataKeys.InverseTruthName, string.Empty);
        _heartProgress = MclslActorAccessor.GetInt(_actor, MclslActorDataKeys.HeartTemperingProgress, 0).ToString();
        _foundationWonder = MclslActorAccessor.GetString(_actor, MclslActorDataKeys.FoundationWonderName, string.Empty);
        _goldenCoreLaws = MclslActorAccessor.GetString(_actor, MclslActorDataKeys.GoldenCoreLaws, string.Empty);
        _nascentCave = MclslActorAccessor.GetString(_actor, MclslActorDataKeys.NascentCaveName, string.Empty);
        _divineChange = MclslActorAccessor.GetString(_actor, MclslActorDataKeys.DivineChangeName, string.Empty);
        _divineMarrow = MclslActorAccessor.GetString(_actor, MclslActorDataKeys.DivineMarrowName, string.Empty);
        _huanzhenAnchor = MclslActorAccessor.GetInt(_actor, MclslActorDataKeys.HuanzhenAnchorYear, -1).ToString();
        _huanzhenRestoreCount = MclslActorAccessor.GetInt(_actor, MclslActorDataKeys.HuanzhenRestoreCount, 0).ToString();
        _customKey = string.Empty;
        _customValue = string.Empty;
        _message = string.Empty;
    }

    private void OnGUI()
    {
        if (!enabled || !MclslRuntimeSettings.DebugToolsVisible || _actor?.data == null)
        {
            enabled = false;
            return;
        }
        _rect = GUI.Window(781209, _rect, DrawWindow, "开发者工具·角色编辑");
    }

    private void DrawWindow(int id)
    {
        GUILayout.Label("<b>直接点击人物即可切换当前编辑目标</b>");
        GUILayout.Label("目标：" + MclslActorAccessor.DisplayName(_actor) + " · ID " + MclslActorAccessor.Id(_actor));
        _realm = Field("境界", _realm);
        _trueEssence = Field("真元", _trueEssence);
        _aptitude = Field("资质", _aptitude);
        _mind = Field("心境", _mind);
        _contribution = Field("贡献", _contribution);
        _spiritStones = Field("灵石", _spiritStones);
        _spaceEssence = Field("空间灵蕴", _spaceEssence);
        _cultivationSystem = Field("修行体系", _cultivationSystem);
        _technique = Field("功法名称", _technique);
        _worldSoul = Field("天地之魄", _worldSoul);
        _inverseTruth = Field("逆理名称", _inverseTruth);
        _heartProgress = Field("淬心进度", _heartProgress);
        _foundationWonder = Field("突破造物", _foundationWonder);
        _goldenCoreLaws = Field("金丹法则", _goldenCoreLaws);
        _nascentCave = Field("元婴洞天", _nascentCave);
        _divineChange = Field("神变", _divineChange);
        _divineMarrow = Field("神髓", _divineMarrow);
        _huanzhenAnchor = Field("还真锚点年", _huanzhenAnchor);
        _huanzhenRestoreCount = Field("还真次数", _huanzhenRestoreCount);
        GUILayout.Label("可编辑任意已存在的 Mod 字符串键（必须以 mclsl. 开头）");
        _customKey = Field("自定义键", _customKey);
        _customValue = Field("自定义值", _customValue);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("应用人物数据", GUILayout.Height(32))) ApplyActorData();
        if (GUILayout.Button("关闭", GUILayout.Height(32))) enabled = false;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("授予还真"))
        {
            MclslDeveloperApi.GrantHuanzhenToSelected(out _message);
            MclslDeveloperApi.RefreshSelectedActor();
        }
        if (GUILayout.Button("打开猫宝")) MclslCodexWindow.ShowMaobaoForActor(_actor);
        if (GUILayout.Button(MclslMaobaoArchiveManager.IsActorSaved(_actor) ? "移除猫宝" : "猫宝刻名"))
            MclslMaobaoArchiveManager.ToggleActor(_actor, out _message);
        if (GUILayout.Button("打开修士列传")) MclslCodexWindow.ShowBiographyForActor(_actor);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("打开前世轮盘")) MclslCodexWindow.ShowHuanzhenSpace();
        if (GUILayout.Button("建立当前锚点"))
        {
            if (MclslHuanzhenSystem.TryCreateManualAnchor(string.Empty, out string message)) _message = message;
            else _message = message;
        }
        if (GUILayout.Button("立即进入新法纪元"))
        {
            MclslWorldEpochSystem.ForceNewLawNow(MclslRuntime.CurrentYear());
            _message = "已立即进入新法纪元。";
        }
        GUILayout.EndHorizontal();
        if (!string.IsNullOrWhiteSpace(_message)) GUILayout.Label(_message);
        GUI.DragWindow();
    }

    private string Field(string label, string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(92));
        string result = GUILayout.TextField(value ?? string.Empty);
        GUILayout.EndHorizontal();
        return result;
    }

    private void ApplyActorData()
    {
        if (!int.TryParse(_trueEssence, out int essence) || !int.TryParse(_aptitude, out int aptitude)
            || !int.TryParse(_mind, out int mind) || !int.TryParse(_contribution, out int contribution)
            || !int.TryParse(_spiritStones, out int stones) || !int.TryParse(_heartProgress, out int heartProgress)
            || !long.TryParse(_spaceEssence, out long spaceEssence))
        {
            _message = "数值格式无效。";
            return;
        }
        if (!string.IsNullOrWhiteSpace(_realm) && !string.Equals(_realm, MclslActorAccessor.Realm(_actor), StringComparison.Ordinal))
            MclslCultivationSystem.SetRealm(_actor, _realm.Trim(), MclslRuntime.CurrentYear(), "开发者工具修改");
        string currentRealm = MclslActorAccessor.Realm(_actor);
        MclslCultivationGrowthSystem.SetTrueEssence(_actor, currentRealm, essence, MclslActorAccessor.GetString(_actor, MclslActorDataKeys.CultivationSystem, string.Empty) == MclslCultivationSystemIds.AncientLaw, false);
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.Aptitude, Math.Clamp(aptitude, 1, 100));
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.MindState, mind);
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.Contribution, contribution);
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.SpiritStones, stones);
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.CultivationSystem, _cultivationSystem.Trim());
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.TechniqueName, _technique.Trim());
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.WorldSoulName, _worldSoul.Trim());
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.InverseTruthName, _inverseTruth.Trim());
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.HeartTemperingProgress, heartProgress);
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.FoundationWonderName, _foundationWonder.Trim());
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.GoldenCoreLaws, _goldenCoreLaws.Trim());
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.NascentCaveName, _nascentCave.Trim());
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.DivineChangeName, _divineChange.Trim());
        MclslActorAccessor.Set(_actor, MclslActorDataKeys.DivineMarrowName, _divineMarrow.Trim());
        if (int.TryParse(_huanzhenAnchor, out int anchorYear)) MclslActorAccessor.Set(_actor, MclslActorDataKeys.HuanzhenAnchorYear, anchorYear);
        if (int.TryParse(_huanzhenRestoreCount, out int restoreCount)) MclslActorAccessor.Set(_actor, MclslActorDataKeys.HuanzhenRestoreCount, restoreCount);
        if (!string.IsNullOrWhiteSpace(_customKey) && _customKey.StartsWith("mclsl.", StringComparison.Ordinal))
            MclslActorAccessor.Set(_actor, _customKey.Trim(), _customValue ?? string.Empty);
        MclslActorAccessor.ApplyDisplayName(_actor, currentRealm);
        try { _actor.updateStats(); } catch { }
        MclslDeveloperApi.SetSpaceEssence(spaceEssence, out _message);
        MclslWorldArchiveStore.SaveNow();
        MclslDeveloperApi.RefreshSelectedActor();
        LoadFields();
        if (string.IsNullOrWhiteSpace(_message)) _message = "人物数据已应用并写入世界档案。";
    }

}
