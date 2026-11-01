using System;
using System.Globalization;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>Editor actions, consumed synchronously by the existing trait router.</summary>
internal static class MclslHeavenGrantActions
{
    internal const string PerfectionTraitId = "MclslHeavenGrantPerfection";
    internal const string DaoBookTraitId = "MclslHeavenGrantDaoBook";

    internal static bool HandleAdded(Actor actor, string traitId)
    {
        if (traitId != PerfectionTraitId && traitId != DaoBookTraitId) return false;
        // Keep even the already-consumed outer callback outside the closure.
        if (actor != null && !actor.hasTrait(traitId)) return true;
        return Consume(actor, traitId);
    }

    private static bool Consume(Actor actor, string traitId)
    {
        if (actor == null) { Tell("天道敕赐未成：角色不存在。"); return true; }
        // addTrait(string) calls addTrait(ActorTrait). The inner postfix consumes
        // the action; the outer postfix must not execute or announce it again.
        try
        {
            if (actor.data == null || MclslActorAccessor.Id(actor) <= 0)
                Tell("天道敕赐未成：角色数据无效。");
            else if (!MclslActorAccessor.Alive(actor))
                Tell("天道敕赐未成：角色已经死亡。");
            else if (World.world?.map_stats == null || string.IsNullOrEmpty(MclslWorldContext.WorldId))
                Tell("天道敕赐未成：世界尚未就绪，请稍后重试。");
            else
                MclslTraitGrantRouter.SuppressRouting(() =>
                {
                    if (traitId == PerfectionTraitId) GrantPerfection(actor);
                    else GrantDaoBook(actor);
                });
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("heaven-grant", "天道敕赐失败：" + ex.Message);
            Tell("天道敕赐未成，请查看日志后重试。");
        }
        finally
        {
            MclslTraitGrantRouter.SuppressRouting(() => actor.removeTrait(traitId));
        }
        return true;
    }

    private static void GrantPerfection(Actor actor)
    {
        bool hasRoute = HasRoute(actor);
        bool changed = SetInt(actor, MclslActorDataKeys.ImmortalFate, 100);
        changed |= SetInt(actor, MclslActorDataKeys.Aptitude, 100);
        changed |= SetInt(actor, MclslActorDataKeys.SpellInsight, 100);
        changed |= SetInt(actor, MclslActorDataKeys.MindState, 100);
        changed |= SetInt(actor, MclslActorDataKeys.MortalSeparationChecked, 1);
        bool giftChanged = !actor.hasTrait(MclslTraitRegistration.GiftTraitIdForAptitude(100));
        MclslTraitRegistration.SyncGiftTrait(actor, 100, allowEntry: false, reconcileIdentity: false);
        if (changed || giftChanged) Refresh(actor);
        Tell(hasRoute ? "四项属性已圆满。"
            : "四项属性已圆满。尚未入道，部分修行属性暂不显示或生效。");
    }

    private static void GrantDaoBook(Actor actor)
    {
        if (MclslTechniqueRealmLimit.OwnedTechniqueCeiling(actor, out MclslTechniqueDefinition old)
            >= MclslRealmIds.Index(MclslRealmIds.HeDao))
        {
            Tell("已有合道道书，无需更换。");
            return;
        }

        MclslTechniqueDefinition selected = SelectBook(old?.LawPool, MclslActorAccessor.Id(actor));
        if (selected == null || string.IsNullOrWhiteSpace(selected.Id)
            || string.IsNullOrWhiteSpace(selected.Name) || selected.MaxRealm != MclslRealmIds.HeDao)
            throw new InvalidOperationException("合道功法定义无效");
        bool ancient = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem)
            == MclslCultivationSystemIds.AncientLaw;
        string id = ancient ? "spiritual_" + selected.Id : selected.Id;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueName, selected.Name);
        MclslTechniqueRealmLimit.OnTechniqueAssigned(actor, selected);
        MclslTechniqueStageSystem.SetProgress(actor, 0);
        // The accessor already sends OnTechniqueChanged, exactly once, after
        // the other book fields are ready for consumers of the index.
        MclslActorAccessor.Set(actor, MclslActorDataKeys.TechniqueId, id);
        Refresh(actor);
        Tell("天授道书：获得《" + selected.Name + "》。"
            + (HasRoute(actor) ? string.Empty : "尚未入道，道书已保存。"));
    }

    private static MclslTechniqueDefinition SelectBook(string[] previousLaws, long actorId)
    {
        var candidates = Books.All;
        int best = -1, tied = 0;
        for (int i = 0; i < candidates.Length; i++)
        {
            int score = Overlap(previousLaws, candidates[i].LawPool);
            if (score > best) { best = score; tied = 1; }
            else if (score == best) tied++;
        }
        int pick = MclslStableHash.Positive43x53(actorId.ToString(CultureInfo.InvariantCulture)) % tied;
        for (int i = 0; i < candidates.Length; i++)
            if (Overlap(previousLaws, candidates[i].LawPool) == best && pick-- == 0) return candidates[i];
        return null;
    }

    private static int Overlap(string[] previous, string[] next)
    {
        int count = 0;
        if (previous == null || next == null) return count;
        for (int i = 0; i < next.Length; i++)
            for (int j = 0; j < previous.Length; j++)
                if (next[i] == previous[j]) { count++; break; }
        return count;
    }

    private static class Books
    {
        // Fixed ordinal ID order, resolved once and shared. No per-click sort,
        // candidate array, random state, actor scan or timer is needed.
        internal static readonly MclslTechniqueDefinition[] All = Load();
        private static MclslTechniqueDefinition[] Load()
        {
            string[] ids = { "gongfa_wanxiang", "gongfa_wuxing", "gongfa_xukong", "gongfa_yinyang" };
            var result = new MclslTechniqueDefinition[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                if (!MclslCultivationCatalog.TryTechnique(ids[i], out result[i])
                    || result[i].MaxRealm != MclslRealmIds.HeDao)
                    throw new InvalidOperationException("缺少合道功法：" + ids[i]);
            }
            return result;
        }
    }

    private static bool HasRoute(Actor actor)
    {
        string route = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem);
        return route == MclslCultivationSystemIds.AncientLaw || route == MclslCultivationSystemIds.NewLaw
            || MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) >= 0;
    }

    private static bool SetInt(Actor actor, string key, int value)
    {
        if (MclslActorAccessor.GetInt(actor, key, int.MinValue) == value) return false;
        MclslActorAccessor.Set(actor, key, value);
        return true;
    }

    private static void Refresh(Actor actor)
    {
        MclslRuntimeChanges.Publish(actor, MclslActorChange.Data | MclslActorChange.Membership
            | MclslActorChange.Traits | MclslActorChange.Presentation);
        MclslActorInfoPanel.RefreshOpenForActor(actor);
    }

    private static void Tell(string message)
    {
        try { WorldTip.showNowTop(message); }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("heaven-grant-tip", message + " 提示显示失败：" + ex.Message);
        }
    }
}
