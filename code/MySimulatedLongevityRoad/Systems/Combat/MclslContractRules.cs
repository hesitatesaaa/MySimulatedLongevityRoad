using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Systems.Combat;

// 灵契师单契灵规则的纯计算部分：不依赖 Unity/游戏运行时，便于离线回归检查。
// 运行时只从这里取比例、增伤加算、重召时刻与选择优先级，避免规则散落。
internal static class MclslContractRules
{
    internal const double ResummonCooldownSeconds = 30d;
    internal const int SummonManaCost = 18;
    internal const double PetUpdateInterval = 1d;

    // 各契灵（0狼1龟2隼3鹿）基础生命 / 攻击占主人当前数值的百分比。
    private static readonly int[] HealthPercent = { 35, 50, 30, 35 };
    private static readonly int[] AttackPercent = { 25, 12, 20, 8 };
    internal const int MinimumPetHealth = 20;
    internal const int MinimumPetAttack = 1;

    internal const double InfusionSeconds = 6d;
    internal const double ResonanceSeconds = 8d;
    internal const double GuardSeconds = 6d;
    internal const double GuardReduction = 0.15d;
    internal const double TurtleSelfReduction = 0.15d;

    internal const double FollowTiles = 6d;
    internal const double FalconRange = 8d;
    internal const double FalconIntervalSeconds = 2d;
    internal const double DeerRange = 6d;
    internal const double DeerIntervalSeconds = 6d;
    internal const double DeerHealPercent = 0.02d;
    internal const double ReviveHealPercent = 0.25d;
    internal const double RevivePetRatio = 0.65d;
    internal const double ReviveOwnerRatio = 0.75d;
    internal const double PassiveHealthBonus = 0.20d;
    internal const double PassiveHealBonus = 0.20d;

    internal const double SigilDamagePower = 0.65d;
    internal const double BreakDamagePower = 1.10d;
    internal const double BreakSlowSeconds = 2d;
    internal const double MarkSeconds = 4d;
    internal const double MarkBonus = 0.15d;
    internal const double InfusionBonus = 0.20d;
    internal const double ResonanceBonus = 0.35d;
    internal const double ResonanceBonusWithPassive5 = 0.50d;
    internal const double OwnerResonanceBonus = 0.10d;
    internal const double PassiveTwoReduction = 0.08d;

    internal const double AssistWindowSeconds = 3d;
    internal const double AssistCooldownSeconds = 6d;
    internal const double AssistDamagePower = 0.15d;

    internal static bool ValidKind(int kind) { return kind >= 0 && kind <= 3; }

    // kind 已由契约状态校验（0..3）与召唤路径保证，这里不做静默回退。
    internal static int PetHealth(int ownerMaxHealth, int kind, bool passiveOne)
    {
        if (!ValidKind(kind)) kind = 0;
        double ratio = HealthPercent[kind] / 100d * (passiveOne ? 1d + PassiveHealthBonus : 1d);
        return Math.Max(MinimumPetHealth, (int)Math.Round(ownerMaxHealth * ratio));
    }

    internal static int PetAttack(int ownerDamage, int kind)
    {
        if (!ValidKind(kind)) kind = 0;
        return Math.Max(MinimumPetAttack, (int)Math.Round(ownerDamage * AttackPercent[kind] / 100d));
    }

    // 契印 / 灵息灌注 / 灵契共鸣的契灵增伤按加算叠加，不叠层（调用方用计时器控制刷新）。
    internal static double PetDamageFactor(bool marked, bool infusion, bool resonance, bool passiveFive)
    {
        double factor = 1d;
        if (marked) factor += MarkBonus;
        if (infusion) factor += InfusionBonus;
        if (resonance) factor += passiveFive ? ResonanceBonusWithPassive5 : ResonanceBonus;
        return factor;
    }

    internal static double CareerHealPercent(double basePercent, bool passiveFour)
    { return basePercent * (passiveFour ? 1d + PassiveHealBonus : 1d); }

    internal static double Remaining(double nextSummonAt, double now)
    { return nextSummonAt <= 0d ? 0d : Math.Max(0d, nextSummonAt - now); }

    internal static double NextSummonAt(double now) { return now + ResummonCooldownSeconds; }

    // 结束契约时取“已有截止”与 Now+30 的较大值，防止转职/解除后立即重召满血契灵。
    internal static double ExtendCooldown(double existing, double now)
    {
        double next = now + ResummonCooldownSeconds;
        return existing > next ? existing : next;
    }

    // 读档重定位：saved 为旧世界时钟下的绝对时刻，换算到 now 的游戏秒。
    internal static double Rebase(double saved, double savedClock, double now)
    { return saved <= 0d ? 0d : now + Math.Max(0d, saved - savedClock); }

    // 旧档多宠迁移：一次扫描同时按“存活 + 种类编号最小”选出保留条目，返回其下标；无则 -1。
    // 在第一次选择最小 kind 时即固定对应活条目，避免“先取 kind 再二次查条目”误留同号死宠。
    internal static int SelectLegacyKeepIndex(IReadOnlyList<int> kinds, IReadOnlyList<bool> alive)
    {
        int bestKind = -1, bestIndex = -1;
        for (int i = 0; i < kinds.Count; i++)
        {
            if (!alive[i]) continue;
            if (bestKind < 0 || kinds[i] < bestKind) { bestKind = kinds[i]; bestIndex = i; }
        }
        return bestIndex;
    }

    internal static int RandomKind(int roll) { return ((roll % 4) + 4) % 4; }

    // 自动施放优先级：危急回生 > 共鸣 > 破势 > 灌注 > 契印。
    internal static int SelectTier(bool canRevive, bool canResonance, bool canBreak, bool canInfusion, bool canSigil)
    {
        if (canRevive) return 4;
        if (canResonance) return 5;
        if (canBreak) return 3;
        if (canInfusion) return 2;
        if (canSigil) return 1;
        return -1;
    }

    // 鹿只治疗一个目标：主人或自己，取生命比例较低者；主人必须距鹿 6 格内才算候选。
    internal static bool DeerHealsOwner(double ownerRatio, double petRatio, bool ownerWithinRange)
    { return ownerWithinRange && ownerRatio <= petRatio; }

    // 协同猎印：双方 3 秒内命中同一目标且全局 6 秒冷却已过才追加；追加伤害在结算前先推进冷却，
    // 因此它自身造成的命中不会再次触发（不自触发）。
    internal static bool AssistReady(double ownerHitAt, double petHitAt, double now, double nextAssistAt)
    {
        if (ownerHitAt <= 0d || petHitAt <= 0d) return false;
        if (Math.Abs(ownerHitAt - petHitAt) > AssistWindowSeconds) return false;
        return now >= nextAssistAt;
    }
}
