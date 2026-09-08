using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslResourceSystem
{
    private const int SpendCooldownYears = 4;

    internal static void EnsureActorResources(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, -1) < 0)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.Contribution, 0);
        if (MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritStones, -1) < 0)
            MclslActorAccessor.Set(actor, MclslActorDataKeys.SpiritStones, 0);
    }

    internal static void GrantAnnualStipend(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor) || !MclslActorAccessor.IsCultivator(actor)) return;
        EnsureActorResources(actor);
        int realmIndex = Math.Max(0, MclslRealmIds.Index(MclslActorAccessor.Realm(actor)));
        bool hasSettlement = actor.city != null || actor.kingdom != null;
        int seed = PositiveHash(MclslActorAccessor.Id(actor) + "|stipend|" + year);
        int contribution = MclslWorldStateModifierSystem.ScaleResourceIncome(AnnualContributionIncome(realmIndex, hasSettlement, seed), year);
        int stones = MclslWorldStateModifierSystem.ScaleResourceIncome((realmIndex + 1) * (realmIndex + 1) + seed % Math.Max(2, 4 + realmIndex * 3), year);
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.HuaShen)) stones += 12 + realmIndex * 5;
        AddContribution(actor, contribution);
        AddSpiritStones(actor, stones);
        ApplyAnnualMaintenance(actor, realmIndex, seed);
    }

    internal static void GrantRuinContribution(Actor actor, int amount)
    {
        AddContribution(actor, MclslWorldStateModifierSystem.ScaleResourceIncome(Math.Max(0, amount), MclslRuntime.CurrentYear()));
    }

    internal static void GrantRuinSpiritStones(Actor actor, int amount)
    {
        AddSpiritStones(actor, MclslWorldStateModifierSystem.ScaleResourceIncome(Math.Max(0, amount), MclslRuntime.CurrentYear()));
    }

    internal static void GrantCaveRefinementReward(Actor actor, int quality, int compatibility, int contenderCount)
    {
        int baseStones = 18 + quality * 16 + compatibility / 4;
        int baseContribution = 10 + quality * 7 + Math.Max(0, contenderCount - 1) * 3;
        AddSpiritStones(actor, baseStones);
        AddContribution(actor, baseContribution);
    }

    internal static void GrantWorldChangeReward(Actor actor, int quality, int compatibility, int contenderCount)
    {
        int baseStones = 35 + quality * 28 + compatibility / 3;
        int baseContribution = 16 + quality * 10 + Math.Max(0, contenderCount - 1) * 4;
        AddSpiritStones(actor, baseStones);
        AddContribution(actor, baseContribution);
    }

    internal static void GrantFactionReward(Actor actor, int contribution, int spiritStones)
    {
        int year = MclslRuntime.CurrentYear();
        AddContribution(actor, MclslWorldStateModifierSystem.ScaleResourceIncome(Math.Max(0, contribution), year));
        AddSpiritStones(actor, MclslWorldStateModifierSystem.ScaleResourceIncome(Math.Max(0, spiritStones), year));
    }

    internal static void TryAutoSpend(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor) || !MclslActorAccessor.IsCultivator(actor)) return;
        EnsureActorResources(actor);
        int lastYear = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.LastResourceSpendYear, -9999);
        if (year - lastYear < SpendCooldownYears) return;
        if (!MclslDetectionGate.TryEnterActorAttempt(actor, "resource", MclslDetectionGate.ResourceAutoSpend, year, 2))
            return;

        string realm = MclslActorAccessor.Realm(actor);
        int realmIndex = Math.Max(0, MclslRealmIds.Index(realm));
        int contribution = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0);
        int stones = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritStones, 0);
        int seed = PositiveHash(MclslActorAccessor.Id(actor) + "|auto_spend|" + year + "|" + contribution + "|" + stones);
        int willingness = 26 + realmIndex * 4 + Math.Min(28, contribution / 30) + Math.Min(24, stones / 45);
        if (seed % 100 >= Math.Clamp(willingness, 25, 78)) return;

        ResourcePurchase purchase = PickPurchase(actor, realm, contribution, stones, year, seed);
        if (purchase == null) return;
        ApplyPurchase(actor, year, purchase);
    }

    internal static void AddContribution(Actor actor, int amount)
    {
        if (!MclslActorAccessor.Alive(actor) || amount <= 0) return;
        int current = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Contribution, Math.Min(999999, current + amount));
    }

    internal static void AddSpiritStones(Actor actor, int amount)
    {
        if (!MclslActorAccessor.Alive(actor) || amount <= 0) return;
        int current = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritStones, 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpiritStones, Math.Min(9999999, current + amount));
    }

    private static void ApplyAnnualMaintenance(Actor actor, int realmIndex, int seed)
    {
        if (realmIndex < MclslRealmIds.Index(MclslRealmIds.JinDan)) return;

        int contributionCost = Math.Max(1, realmIndex - 1) + seed % Math.Max(1, 1 + realmIndex / 2);
        int stoneCost = realmIndex * 4 + seed % Math.Max(2, 6 + realmIndex * 2);
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.HuaShen)) stoneCost += realmIndex * 6;
        if (realmIndex >= MclslRealmIds.Index(MclslRealmIds.HeDao)) contributionCost += 4;

        int contribution = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0);
        int stones = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritStones, 0);
        int finalContributionCost = Math.Min(contributionCost, Math.Max(0, contribution / 3));
        int finalStoneCost = Math.Min(stoneCost, Math.Max(0, stones / 3));
        if (finalContributionCost > 0) MclslActorAccessor.Set(actor, MclslActorDataKeys.Contribution, contribution - finalContributionCost);
        if (finalStoneCost > 0) MclslActorAccessor.Set(actor, MclslActorDataKeys.SpiritStones, stones - finalStoneCost);
    }

    private static ResourcePurchase PickPurchase(Actor actor, string realm, int contribution, int stones, int year, int seed)
    {
        bool ancientLaw = MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty)
            == MclslCultivationSystemIds.AncientLaw;
        int essence = MclslCultivationGrowthSystem.CurrentTrueEssence(actor);
        int nextRealmMinimum = MclslRealmProgress.NextRealmMinimum(realm, ancientLaw);
        float progress = MclslRealmProgress.ProgressForRealm(realm, essence, ancientLaw);
        ResourcePurchase techniqueExchange = TryBuildTechniqueExchange(actor, realm, contribution, stones, year, seed);
        if (techniqueExchange != null) return techniqueExchange;

        if (realm == MclslRealmIds.LianQi && progress >= 72f && CanAfford(contribution, stones, 45, 25))
            return new ResourcePurchase("筑基奇物线索", 45, 25, "筑基奇遇概率+15%", a => AddFoundationClue(a, 15));
        if (realm == MclslRealmIds.LianQi && CanAfford(contribution, stones, 8, 18))
            return new ResourcePurchase("炼气灵丹", 8, 18, "悟法能力+4，炼心进度+3%", a =>
            {
                MclslActorAccessor.Set(a, MclslActorDataKeys.TechniqueInsight, MclslActorAccessor.GetInt(a, MclslActorDataKeys.TechniqueInsight, 0) + 4);
                AddHeartProgress(a, 3);
            });

        if (realm == MclslRealmIds.ZhuJi && CanAfford(contribution, stones, 55, 35))
            return new ResourcePurchase("功法注解", 55, 35, "悟法能力+14", a =>
            {
                MclslActorAccessor.Set(a, MclslActorDataKeys.TechniqueInsight, MclslActorAccessor.GetInt(a, MclslActorDataKeys.TechniqueInsight, 0) + 14);
            });

        if (realm == MclslRealmIds.JinDan && CanAfford(contribution, stones, 85, 65))
            return new ResourcePurchase("洞天残图", 85, 65, "洞天争夺强度+18", a =>
            {
                MclslActorAccessor.Set(a, MclslActorDataKeys.CaveClaimBonus,
                    Math.Min(90, MclslActorAccessor.GetInt(a, MclslActorDataKeys.CaveClaimBonus, 0) + 18));
            });

        if (realm == MclslRealmIds.YuanYing && CanAfford(contribution, stones, 115, 95))
            return new ResourcePurchase("护髓法器", 115, 95, "抽髓争夺强度+18", a =>
            {
                MclslActorAccessor.Set(a, MclslActorDataKeys.DivineClaimBonus,
                    Math.Min(90, MclslActorAccessor.GetInt(a, MclslActorDataKeys.DivineClaimBonus, 0) + 18));
            });

        if (realm == MclslRealmIds.HuaShen && CanAfford(contribution, stones, 130, 115))
            return new ResourcePurchase("天地异变密录", 130, 115, "心境+2，炼心进度+10%，遗迹阅历+8", a =>
            {
                MclslActorAccessor.Set(a, MclslActorDataKeys.MindState, Math.Min(100, MclslMindSystem.EnsureMindState(a) + 2));
                AddHeartProgress(a, 10);
                MclslActorAccessor.Set(a, MclslActorDataKeys.RuinExperience, MclslActorAccessor.GetInt(a, MclslActorDataKeys.RuinExperience, 0) + 8);
            });

        if ((realm == MclslRealmIds.HeDao || realm == MclslRealmIds.ChangSheng) && CanAfford(contribution, stones, 180, 160))
            return new ResourcePurchase("逆理注疏", 180, 160, "逆理进度+4%，心境+2", a =>
            {
                MclslActorAccessor.Set(a, MclslActorDataKeys.InverseTruthProgress, Math.Min(100, MclslActorAccessor.GetInt(a, MclslActorDataKeys.InverseTruthProgress, 0) + 4));
                MclslActorAccessor.Set(a, MclslActorDataKeys.MindState, Math.Min(100, MclslMindSystem.EnsureMindState(a) + 2));
            });

        if (nextRealmMinimum > 0 && progress < 65f && essence < nextRealmMinimum && CanAfford(contribution, stones, 20, 30) && seed % 100 < 45)
            return new ResourcePurchase("日常修行资粮", 20, 30, "炼心进度+6%，悟法能力+3", a =>
            {
                AddHeartProgress(a, 6);
                MclslActorAccessor.Set(a, MclslActorDataKeys.TechniqueInsight, MclslActorAccessor.GetInt(a, MclslActorDataKeys.TechniqueInsight, 0) + 3);
            });

        if (CanAfford(contribution, stones, 35, 25) && MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0) < 100)
            return new ResourcePurchase("炼心残篇", 35, 25, "炼心进度+8%，心境+1", a =>
            {
                MclslActorAccessor.Set(a, MclslActorDataKeys.HeartMethodKnown, 1);
                AddHeartProgress(a, 8);
                MclslActorAccessor.Set(a, MclslActorDataKeys.MindState, Math.Min(100, MclslMindSystem.EnsureMindState(a) + 1));
            });

        return null;
    }

    private static ResourcePurchase TryBuildTechniqueExchange(Actor actor, string realm, int contribution, int stones, int year, int seed)
    {
        if (actor?.data == null || string.IsNullOrWhiteSpace(realm)) return null;
        if (MclslActorAccessor.GetString(actor, MclslActorDataKeys.CultivationSystem, string.Empty) != MclslCultivationSystemIds.NewLaw) return null;

        int currentRealmIndex = MclslRealmIds.Index(realm);
        if (currentRealmIndex < 0 || currentRealmIndex >= MclslRealmIds.Index(MclslRealmIds.HeDao)) return null;

        string maxRealm = MclslTechniqueRealmLimit.MaxRealm(actor);
        int maxIndex = MclslRealmIds.Index(maxRealm);
        if (maxIndex < 0 || maxIndex >= MclslRealmIds.Index(MclslRealmIds.HeDao)) return null;
        if (maxIndex > currentRealmIndex) return null;

        string targetRealm = MclslRealmIds.Ordered[maxIndex + 1];
        TechniqueExchangeCost cost = CostForTechniqueExchange(targetRealm);
        if (!CanAfford(contribution, stones, cost.Contribution, cost.Stones)) return null;

        int urgency = currentRealmIndex >= maxIndex ? 70 : 35;
        urgency += Math.Min(18, contribution / Math.Max(1, cost.Contribution / 3));
        urgency += Math.Min(12, stones / Math.Max(1, cost.Stones / 3));
        if (seed % 100 >= Math.Clamp(urgency, 45, 92)) return null;

        string faction = MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty);
        if (string.IsNullOrWhiteSpace(faction))
            faction = seed % 100 < 62 ? "wanxian" : "five_elders";
        string itemName = faction == "five_elders" ? MclslRealmIds.Display(targetRealm) + "秘传残卷" : MclslRealmIds.Display(targetRealm) + "功法玉册";
        string oldName = MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, "旧法门");
        string suffix = faction == "five_elders" ? "秘卷" : "玉册";
        string newName = BuildExchangedTechniqueName(oldName, targetRealm, suffix, year, seed);
        if (seed % 100 < 45
            && MclslTechniqueLineageSystem.TryPickInheritedTechnique(year, MclslActorAccessor.Id(actor) + "|exchange|" + seed, 100, out MclslTechniqueDefinition inherited)
            && inherited != null
            && !string.IsNullOrWhiteSpace(inherited.Name))
        {
            newName = inherited.Name + "·" + MclslRealmIds.Display(targetRealm) + suffix;
        }
        string sourceName = faction == "five_elders" ? "五老会" : "万仙盟";
        string finalTechniqueName = newName;
        string effect = "向" + sourceName + "换得《" + finalTechniqueName + "》，最高可至" + MclslRealmIds.Display(targetRealm);

        return new ResourcePurchase(itemName, cost.Contribution, cost.Stones, effect, a =>
        {
            MclslTechniqueRealmLimit.SetMaxRealm(a, targetRealm);
            string newId = "exchange_" + faction + "_" + year + "_" + MclslWorldRunRepository.NextProceduralSequence();
            MclslActorAccessor.Set(a, MclslActorDataKeys.TechniqueId, newId);
            MclslActorAccessor.Set(a, MclslActorDataKeys.TechniqueName, finalTechniqueName);
            MclslActorAccessor.Set(a, MclslActorDataKeys.FactionAffiliation, faction);
            MclslActorAccessor.Set(a, MclslActorDataKeys.TechniqueInsight, MclslActorAccessor.GetInt(a, MclslActorDataKeys.TechniqueInsight, 0) + 10 + maxIndex * 4);
        });
    }

    private static TechniqueExchangeCost CostForTechniqueExchange(string targetRealm)
    {
        int index = Math.Max(0, MclslRealmIds.Index(targetRealm));
        return targetRealm switch
        {
            MclslRealmIds.ZhuJi => new TechniqueExchangeCost(480, 120),
            MclslRealmIds.JinDan => new TechniqueExchangeCost(1450, 360),
            MclslRealmIds.YuanYing => new TechniqueExchangeCost(3600, 900),
            MclslRealmIds.HuaShen => new TechniqueExchangeCost(7800, 1900),
            MclslRealmIds.HeDao => new TechniqueExchangeCost(15000, 3600),
            _ => new TechniqueExchangeCost(480 + index * 900, 120 + index * 320)
        };
    }

    private static int AnnualContributionIncome(int realmIndex, bool hasSettlement, int seed)
    {
        int baseIncome = realmIndex switch
        {
            <= 0 => 2,
            1 => 4,
            2 => 8,
            3 => 14,
            4 => 24,
            5 => 38,
            _ => 52
        };
        int variance = seed % Math.Max(2, 3 + realmIndex * 2);
        int settlement = hasSettlement ? 1 + realmIndex : 0;
        return baseIncome + settlement + variance;
    }

    private static string BuildExchangedTechniqueName(string oldName, string targetRealm, string suffix, int year, int seed)
    {
        string clean = string.IsNullOrWhiteSpace(oldName) ? "无名功法" : oldName.Trim('《', '》', ' ');
        string[] prefixes = { "太玄", "玄黄", "归元", "洞真", "明法", "太上", "天衡", "玉虚" };
        string prefix = prefixes[PositiveHash(seed + "|" + year + "|" + clean) % prefixes.Length];
        return prefix + "·" + clean + "·" + MclslRealmIds.Display(targetRealm) + suffix;
    }

    private static void ApplyPurchase(Actor actor, int year, ResourcePurchase purchase)
    {
        int contribution = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0);
        int stones = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.SpiritStones, 0);
        if (!CanAfford(contribution, stones, purchase.ContributionCost, purchase.SpiritStoneCost)) return;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.Contribution, contribution - purchase.ContributionCost);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.SpiritStones, stones - purchase.SpiritStoneCost);
        purchase.Apply(actor);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastResourceSpendYear, year);
        string summary = SafeName(actor) + "购得“" + purchase.ItemName + "”，耗贡献" + purchase.ContributionCost + "、灵石" + purchase.SpiritStoneCost + "，" + purchase.EffectText + "。";
        MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, summary);
        MclslWorldRunRepository.RegisterResourceSpend(new MclslResourceSpendRecord
        {
            Id = "spend_" + year + "_" + MclslWorldRunRepository.NextProceduralSequence(),
            Year = year,
            ActorId = MclslActorAccessor.Id(actor),
            ActorName = SafeName(actor),
            RealmName = MclslRealmIds.Display(MclslActorAccessor.Realm(actor)),
            ItemName = purchase.ItemName,
            ContributionCost = purchase.ContributionCost,
            SpiritStoneCost = purchase.SpiritStoneCost,
            EffectText = purchase.EffectText,
            Summary = summary
        });
    }

    private static void AddFoundationClue(Actor actor, int amount)
    {
        MclslActorAccessor.Set(actor, MclslActorDataKeys.FoundationChanceBonus,
            Math.Min(80, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.FoundationChanceBonus, 0) + Math.Max(0, amount)));
    }

    private static void AddHeartProgress(Actor actor, int amount)
    {
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeartTemperingProgress,
            Math.Min(100, MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HeartTemperingProgress, 0) + Math.Max(0, amount)));
    }

    private static bool CanAfford(int contribution, int stones, int contributionCost, int stoneCost)
    {
        return contribution >= contributionCost && stones >= stoneCost;
    }

    private static string SafeName(Actor actor)
    {
        try { return MclslActorAccessor.DisplayName(actor); }
        catch { return "无名者"; }
    }

    private static int PositiveHash(string value)
    {
        unchecked { int result = 61; foreach (char c in value ?? string.Empty) result = result * 67 + c; return result & int.MaxValue; }
    }

    private sealed class ResourcePurchase
    {
        internal readonly string ItemName;
        internal readonly int ContributionCost;
        internal readonly int SpiritStoneCost;
        internal readonly string EffectText;
        internal readonly Action<Actor> Apply;

        internal ResourcePurchase(string itemName, int contributionCost, int spiritStoneCost, string effectText, Action<Actor> apply)
        {
            ItemName = itemName;
            ContributionCost = contributionCost;
            SpiritStoneCost = spiritStoneCost;
            EffectText = effectText;
            Apply = apply;
        }
    }

    private readonly struct TechniqueExchangeCost
    {
        internal readonly int Contribution;
        internal readonly int Stones;

        internal TechniqueExchangeCost(int contribution, int stones)
        {
            Contribution = contribution;
            Stones = stones;
        }
    }
}
