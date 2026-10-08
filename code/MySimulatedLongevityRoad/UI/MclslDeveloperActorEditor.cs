using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

/// <summary>In-game actor editor. All writes use the same gameplay commands.</summary>
internal sealed class MclslDeveloperActorEditor : MonoBehaviour
{
    private static MclslDeveloperActorEditor _instance;
    private Actor _actor;
    private Rect _window = new(120f, 80f, 760f, 760f);
    private Vector2 _scroll;
    private string _aptitude = "50";
    private string _mind = "50";
    private string _essence = "0";
    private string _contribution = "0";
    private string _stones = "0";
    private string _message = string.Empty;

    internal static void Show(Actor actor)
    {
        if (actor?.data == null) return;
        Ensure();
        _instance._actor = actor;
        _instance.Reload();
        _instance.enabled = true;
    }

    private static void Ensure()
    {
        if (_instance != null) return;
        GameObject host = new("MclslDeveloperActorEditor");
        DontDestroyOnLoad(host);
        _instance = host.AddComponent<MclslDeveloperActorEditor>();
        _instance.enabled = false;
    }

    private void Reload()
    {
        if (_actor?.data == null) return;
        _aptitude = MclslActorAccessor.GetInt(_actor, MclslActorDataKeys.Aptitude, 50).ToString();
        _mind = MclslActorAccessor.GetInt(_actor, MclslActorDataKeys.MindState, 50).ToString();
        _essence = MclslActorAccessor.GetInt(_actor, MclslActorDataKeys.TrueEssence, 0).ToString();
        _contribution = MclslActorAccessor.GetMoney(_actor, MclslActorDataKeys.Contribution, 0).ToString();
        _stones = MclslActorAccessor.GetMoney(_actor, MclslActorDataKeys.SpiritStones, 0).ToString();
    }

    private void OnGUI()
    {
        if (!enabled || _actor?.data == null) return;
        _window = GUI.Window(917503, _window, DrawWindow, "开发者角色编辑器 · 0.5.1");
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("人物：" + MclslActorAccessor.DisplayName(_actor) + "　ID " + MclslActorAccessor.Id(_actor));
        if (GUILayout.Button("刷新", GUILayout.Width(70))) Reload();
        if (GUILayout.Button("关闭", GUILayout.Width(70))) enabled = false;
        GUILayout.EndHorizontal();
        if (!string.IsNullOrWhiteSpace(_message)) GUILayout.Label(_message);

        _scroll = GUILayout.BeginScrollView(_scroll);
        GUILayout.Label("数值（应用时执行领域校验与账本提交）");
        DrawIntField("资质", ref _aptitude, 1, 100, value =>
        {
            MclslActorAccessor.Set(_actor, MclslActorDataKeys.Aptitude, value);
            MclslTraitRegistration.SyncGiftTrait(_actor, value, allowEntry: _actor.getAge() >= 6f);
        });
        DrawIntField("心境", ref _mind, 0, 100,
            value => MclslActorAccessor.Set(_actor, MclslActorDataKeys.MindState, value));
        DrawIntField("真元", ref _essence, 0, int.MaxValue, value =>
        {
            bool ancient = MclslActorAccessor.GetString(_actor, MclslActorDataKeys.CultivationSystem, string.Empty)
                == MclslCultivationSystemIds.AncientLaw;
            MclslCultivationGrowthSystem.SetTrueEssence(_actor, MclslActorAccessor.Realm(_actor), value,
                ancient, enforceRealmMinimum: false);
        });
        DrawMoneyFields();

        GUILayout.Space(8f);
        GUILayout.Label("境界");
        DrawRealmButtons(false);
        DrawRealmButtons(true);

        GUILayout.Space(8f);
        GUILayout.Label("特殊体质（非原版智慧生物会被统一规则拒绝）");
        DrawPhysiques();
        GUILayout.EndScrollView();
        GUI.DragWindow(new Rect(0, 0, 760, 28));
        GUILayout.EndVertical();
    }

    private void DrawIntField(string label, ref string text, int minimum, int maximum, Action<int> apply)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(90));
        text = GUILayout.TextField(text, GUILayout.Width(180));
        if (GUILayout.Button("应用", GUILayout.Width(70)))
        {
            if (!int.TryParse(text, out int value)) _message = label + "不是有效整数。";
            else
            {
                try { apply(Math.Clamp(value, minimum, maximum)); _message = label + "已更新。"; }
                catch (Exception ex) { _message = label + "更新失败：" + ex.Message; }
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void DrawMoneyFields()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("贡献度", GUILayout.Width(90));
        _contribution = GUILayout.TextField(_contribution, GUILayout.Width(120));
        GUILayout.Label("灵石", GUILayout.Width(50));
        _stones = GUILayout.TextField(_stones, GUILayout.Width(120));
        if (GUILayout.Button("原子应用", GUILayout.Width(90)))
        {
            if (!long.TryParse(_contribution, out long contribution) || contribution < 0
                || !long.TryParse(_stones, out long stones) || stones < 0)
                _message = "货币必须是非负整数。";
            else
            {
                try
                {
                    MclslEconomyCommands.RestoreBalances(_actor, contribution, stones);
                    _message = "双币余额已通过账本提交。";
                }
                catch (Exception ex) { _message = "货币更新失败：" + ex.Message; }
            }
        }
        GUILayout.EndHorizontal();
    }

    private void DrawRealmButtons(bool ancient)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(ancient ? "旧法" : "新法", GUILayout.Width(48));
        foreach (string realm in MclslRealmIds.Ordered)
        {
            if (GUILayout.Button(MclslRealmIds.Display(realm), GUILayout.Width(76)))
            {
                string traitId = ancient ? MclslTraitRegistration.LegacyTraitIdForRealm(realm)
                    : MclslTraitRegistration.TraitIdForRealm(realm);
                try
                {
                    bool added = _actor.addTrait(traitId, true);
                    _message = added ? "已设置" + (ancient ? "旧法" : "新法") + MclslRealmIds.Display(realm) + "。"
                        : "境界特质未改变。";
                }
                catch (Exception ex) { _message = "境界更新失败：" + ex.Message; }
            }
        }
        GUILayout.EndHorizontal();
    }

    private void DrawPhysiques()
    {
        int column = 0;
        foreach (string physiqueId in MclslPhysiqueSystem.AllIds)
        {
            if (column == 0) GUILayout.BeginHorizontal();
            bool owned = _actor.hasTrait(physiqueId);
            string label = (owned ? "移除 " : "授予 ") + MclslPhysiqueSystem.DisplayName(physiqueId);
            if (GUILayout.Button(label, GUILayout.Width(175)))
            {
                try
                {
                    bool changed;
                    if (owned)
                    {
                        _actor.removeTrait(physiqueId);
                        changed = !_actor.hasTrait(physiqueId);
                    }
                    else changed = _actor.addTrait(physiqueId, true);
                    _message = changed ? label + "完成。" : label + "被资格规则拒绝或状态未变化。";
                }
                catch (Exception ex) { _message = label + "失败：" + ex.Message; }
                Reload();
            }
            column++;
            if (column == 4)
            {
                GUILayout.EndHorizontal();
                column = 0;
            }
        }
        if (column != 0)
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
