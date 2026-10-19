using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems.Combat;

internal static class MclslBattleProfessionSystem
{
    private const string MainKey = "mclsl.battle.v1.main";
    private const string AnnualKey = "mclsl.battle.v1.study_year";
    internal static readonly string[] Ids = { "sword", "body", "arts", "pact" };
    internal static readonly string[] Names = { "剑修", "体修", "五行术修", "御兽师" };
    internal static readonly string[] Traits = { "MclslBattleSword", "MclslBattleBody", "MclslBattleArts", "MclslBattlePact" };

    private static int Number(string id) => id != null && id.Length == 4 && id[0] == 'C'
        && int.TryParse(id.Substring(1), out int n) && n >= 1 && n <= 16 ? n : 0;
    internal static bool IsProfessionSpell(string id) => Number(id) != 0;
    internal static bool IsBasic(string id) => Number(id) is int n && n > 0 && (n - 1) % 4 < 2;
    internal static string ProfessionForSpell(string id) => Number(id) is int n && n > 0 ? Ids[(n - 1) / 4] : string.Empty;
    private static string Spell(int profession, int slot) => "C" + (profession * 4 + slot + 1).ToString("D3");
    internal static string Main(Actor actor) => MclslActorAccessor.GetString(actor, MainKey, string.Empty);
    internal static string Display(Actor actor) => Array.IndexOf(Ids, Main(actor)) is int i && i >= 0 ? Names[i] : "未入门";
    internal static bool CanLearn(Actor actor, string id)
    {
        int n = Number(id);
        if (n == 0) return true;
        int slot = (n - 1) % 4;
        return actor?.data != null && MclslActorAccessor.HasCultivationPath(actor)
            && MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) >= (slot < 2 ? 0 : slot == 2 ? 2 : 4);
    }
    internal static bool CanUse(Actor actor, string id) => !IsProfessionSpell(id)
        || CanLearn(actor, id) && (IsBasic(id) || Main(actor) == ProfessionForSpell(id));
    internal static bool CanStudy(Actor actor, string id) => !IsProfessionSpell(id)
        || MclslSpellSystem.Knows(actor, id) && CanUse(actor, id);
    internal static void OnLearned(Actor actor, string id)
    {
        if (IsProfessionSpell(id) && IsBasic(id) && string.IsNullOrEmpty(Main(actor)))
            SetMain(actor, ProfessionForSpell(id));
    }
    internal static bool SetMain(Actor actor, string id)
    {
        int index = Array.IndexOf(Ids, id);
        if (actor?.data == null || index < 0) return false;
        if(Main(actor)!=id)MclslSpiritPact.RemoveOwner(actor);
        MclslActorAccessor.Set(actor, MainKey, id);
        MclslTraitGrantRouter.SuppressRouting(() =>
        {
            for (int i = 0; i < Traits.Length; i++)
                if (i != index && actor.hasTrait(Traits[i])) actor.removeTrait(Traits[i]);
            if (!actor.hasTrait(Traits[index])) actor.addTrait(Traits[index]);
        });
        return true;
    }
    internal static void ClearMain(Actor actor){MclslSpiritPact.RemoveOwner(actor);MclslActorAccessor.Set(actor,MainKey,string.Empty);}
    private static int Roll(Actor actor, int year, string salt)
    {
        unchecked
        {
            uint hash = (uint)MclslActorAccessor.Id(actor) * 16777619u ^ (uint)year;
            foreach (char c in salt) hash = (hash ^ c) * 16777619u;
            return (int)(hash % 10000);
        }
    }
    private static int Preferred(Actor actor)
    {
        var technique = MclslCultivationCatalog.Technique(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId));
        string context = (technique?.Name ?? "") + MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientSectOrigin);
        string[] hints = { "剑锋斩", "体拳罡山", "五行雷火冰", "兽灵契御" };
        string[] laws = { "金", "土", "火", "木" };
        string[] roots = MclslSpiritualRootSystem.RootAttributes(actor);
        int aptitude=MclslActorAccessor.GetInt(actor,MclslActorDataKeys.Aptitude,50);
        int best = 0, score = int.MinValue;
        for (int i = 0; i < 4; i++)
        {
            int candidate = Roll(actor, 0, Ids[i]) % 17;
            if(i==2)candidate+=aptitude/10;
            if(i==1)candidate+=(100-aptitude)/10;
            foreach (char hint in hints[i]) if (context.IndexOf(hint) >= 0) candidate += 40;
            if (Array.IndexOf(roots, laws[i]) >= 0) candidate += 20;
            if (technique?.LawPool != null && Array.IndexOf(technique.LawPool, laws[i]) >= 0) candidate += 25;
            if (candidate > score) { best = i; score = candidate; }
        }
        return best;
    }
    internal static void ProgressAnnual(Actor actor)
    {
        if (!CanLearn(actor, "C001")) return;
        int year = MclslRuntime.CurrentYear();
        if (MclslActorAccessor.GetInt(actor, AnnualKey, -1) == year) return;
        MclslActorAccessor.Set(actor, AnnualKey, year);
        if (Roll(actor, year, "basic") >= 2000) return;
        int profession = Array.IndexOf(Ids, Main(actor));
        if (profession < 0) profession = Preferred(actor);
        for (int slot = 0; slot < 2; slot++)
            if (!MclslSpellSystem.Knows(actor, Spell(profession, slot)))
            { MclslSpellSystem.Learn(actor, Spell(profession, slot)); break; }
    }
    internal static string TryTransmission(Actor actor, int year, string source)
    {
        if (!CanLearn(actor, "C001") || Roll(actor, year, source) >= 1500) return string.Empty;
        int profession = Array.IndexOf(Ids, Main(actor));
        if (profession < 0) profession = Preferred(actor);
        for (int slot = 0; slot < 4; slot++)
        {
            string id = Spell(profession, slot);
            if (!MclslSpellSystem.Knows(actor, id) && CanLearn(actor, id) && MclslSpellSystem.Learn(actor, id))
                return MclslSpellSystem.TryGet(id, out var spell) ? "习得斗法传承【" + spell.Name + "】" : string.Empty;
        }
        return string.Empty;
    }
    internal static bool TryBattleInsight(Actor actor, int roll)
    {
        int profession = Array.IndexOf(Ids, Main(actor));
        if (profession < 0 || roll < 0 || roll >= 10000) return false;
        string core = Spell(profession, 2), divine = Spell(profession, 3);
        if (!MclslSpellSystem.Knows(actor, core))
            return roll < 500 && CanLearn(actor, core)
                && MclslSpellProgression.Level(actor, Spell(profession, 0)) >= 10
                && MclslSpellProgression.Level(actor, Spell(profession, 1)) >= 10 && MclslSpellSystem.Learn(actor, core);
        return !MclslSpellSystem.Knows(actor, divine) && roll < 100 && CanLearn(actor, divine)
            && MclslSpellProgression.Level(actor, core) >= 10 && MclslSpellSystem.Learn(actor, divine);
    }
    internal static string AcquisitionDescription(string id) => !IsProfessionSpell(id) ? string.Empty
        : IsBasic(id) ? "炼气起：法门、拜师、宗门传授或自然修行；基础技可兼修。"
        : (Number(id) - 1) % 4 == 2 ? "金丹起：秘籍、师承、宗门或探索；两门基础技满熟练后，合格战斗结束有5%领悟。限主职使用。"
        : "化神起：高阶传承；核心技满熟练后，合格战斗结束有1%领悟。限主职使用。";
}
