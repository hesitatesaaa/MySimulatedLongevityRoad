using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Combat;

// 灵契师契约快照（UI 约定）。
internal sealed class MclslContractInfo
{
    internal Actor Owner, Pet;
    internal int Kind;
    internal double Remaining;
    internal string LastPetName;
    internal bool NeedsChoice;
}

// 单契约灵的生命周期与对外选择接口。规则数值集中在 MclslContractRules。
internal static partial class MclslCareerCombat
{
    // ── UI 约定 API ─────────────────────────────────────────────
    internal static MclslContractInfo Contract(Actor actor)
    {
        if (actor?.data == null) return null;
        long id = MclslActorAccessor.Id(actor);
        if (SummonOwners.TryGetValue(id, out long ownerId))
        {
            if (!Battles.TryGetValue(ownerId, out var petBattle)) return null;
            if (!MclslActorRegistry.ResolveKnownOrWorld(ownerId, out var owner) || !MclslActorAccessor.Alive(owner)) return null;
            return BuildContract(owner, petBattle);
        }
        if (!MclslCombatCareerSystem.Applicable(actor)) return null;
        if (MclslCombatCareerSystem.Read(actor).Career != 5) return null;
        Battles.TryGetValue(id, out var battle);
        return BuildContract(actor, battle);
    }

    internal static bool ChooseContract(Actor owner, int kind)
    {
        if (owner?.data == null || !MclslContractRules.ValidKind(kind)) return false;
        if (!MclslCombatCareerSystem.Applicable(owner)) return false;
        var state = MclslCombatCareerSystem.Read(owner);
        // 只允许待选（尚未契约）的灵契师；已绑定者不可刷新。
        if (state.Career != 5 || state.ContractKind >= 0) return false;
        state.ContractKind = kind;
        state.ContractSchema = 1;
        state.NextSummonAt = 0;
        MclslCombatCareerSystem.Write(owner, state);
        Track(owner);
        Wake(owner);
        return true;
    }

    private static MclslContractInfo BuildContract(Actor owner, MclslCareerBattleState battle)
    {
        var state = MclslCombatCareerSystem.Read(owner);
        var info = new MclslContractInfo
        {
            Owner = owner,
            Kind = state.ContractKind,
            LastPetName = state.ContractPetName ?? string.Empty,
            NeedsChoice = state.ContractKind < 0,
            Remaining = MclslContractRules.Remaining(state.NextSummonAt, Now),
        };
        if (battle != null)
        {
            var summon = BoundSummon(battle);
            if (summon != null)
            {
                Actor pet = Resolve(summon.Unit);
                if (MclslActorAccessor.Alive(pet)) { info.Pet = pet; info.Kind = summon.Kind; }
            }
        }
        return info;
    }

    // ── 生命周期 ───────────────────────────────────────────────
    private static void TickSpirit(Actor owner, MclslCareerBattleState battle, MclslCombatCareerState state)
    {
        EnsureContract(owner, battle, state);
        if (state.ContractKind < 0) return;
        var bound = BoundSummon(battle);
        if (bound != null)
        {
            Actor pet = Resolve(bound.Unit);
            if (!MclslActorAccessor.Alive(pet)) { HandlePetLost(owner, battle, bound, pet); return; }
            if (Now >= bound.Pulse)
            {
                bool active = owner.has_attack_target || pet.has_attack_target
                    || owner.getHealth() < owner.getMaxHealth() || pet.getHealth() < pet.getMaxHealth();
                bound.Pulse = Now + (active ? MclslContractRules.PetUpdateInterval : 3d);
                UpdateBoundPet(owner, pet, battle, bound);
            }
            return;
        }
        TryAutoSummon(owner, battle, state);
    }

    // 旧档 v2 迁移：ContractSchema 0 时一次性确定契约（多宠取最小种类，无宠随机），
    // 之后 ContractSchema 1 的待选择不会再被误判为旧档随机。
    private static void EnsureContract(Actor owner, MclslCareerBattleState battle, MclslCombatCareerState state)
    {
        if (state.Career != 5) return;
        if (state.ContractSchema >= 1)
        {
            var existing = BoundSummon(battle);
            if (existing != null && state.ContractKind != existing.Kind) { state.ContractKind = existing.Kind; MclslCombatCareerSystem.Write(owner, state); }
            return;
        }
        var kinds = new List<int>(battle.Summons.Count);
        var aliveFlags = new List<bool>(battle.Summons.Count);
        for (int i = 0; i < battle.Summons.Count; i++)
        {
            var s = battle.Summons[i];
            kinds.Add(s.Kind);
            aliveFlags.Add(MclslActorAccessor.Alive(Resolve(s.Unit)));
        }
        int keepIndex = MclslContractRules.SelectLegacyKeepIndex(kinds, aliveFlags);
        MclslCareerSummon keep = keepIndex >= 0 ? battle.Summons[keepIndex] : null;
        // 其余（含重复/死亡条目）静默移除，不触发奖励或冷却。
        for (int i = 0; i < battle.Summons.Count; i++)
        {
            var s = battle.Summons[i];
            if (s == keep) continue;
            SummonOwners.Remove(s.Unit);
            Actor other = Resolve(s.Unit);
            if (MclslActorAccessor.Alive(other)) other.dieAndDestroy((AttackType)11);
        }
        battle.Summons.Clear();
        if (keep != null)
        {
            keep.Bound = true;
            battle.Summons.Add(keep);
        }
        state.ContractKind = keep != null ? keep.Kind
            : MclslContractRules.RandomKind(MclslCombatCareerSystem.Roll(MclslActorAccessor.Id(owner), 0, 0x5C57));
        if (keep != null)
        {
            Actor pet = Resolve(keep.Unit);
            if (pet != null) state.ContractPetName = PetName(pet);
        }
        state.ContractSchema = 1;
        MclslCombatCareerSystem.Write(owner, state);
    }

    private static void TryAutoSummon(Actor owner, MclslCareerBattleState battle, MclslCombatCareerState state)
    {
        if (state.ContractKind < 0 || Now < state.NextSummonAt) return;
        if (!MclslActorAccessor.IsCultivator(owner)) return;
        int mana = MclslSpellSystem.CurrentMana(owner);
        if (mana < MclslContractRules.SummonManaCost) return; // 灵力不足则等待
        MclslActorAccessor.Set(owner, MclslActorDataKeys.ManaCurrent, mana - MclslContractRules.SummonManaCost);
        SpawnContractPet(owner, battle, state, state.ContractKind);
        state.NextSummonAt = 0;
        MclslCombatCareerSystem.Write(owner, state);
    }

    private static void SpawnContractPet(Actor owner, MclslCareerBattleState battle, MclslCombatCareerState state, int kind)
    {
        Actor pet = World.world.units.createNewUnit("mclsl_career_spirit_" + kind,
            owner.current_tile, false, 0f, null, null, true, false, false, false);
        if (owner.kingdom != null) pet.setKingdom(owner.kingdom);
        ApplyPetBaseStats(owner, pet, kind, true);
        var summon = new MclslCareerSummon
        {
            Unit = MclslActorAccessor.Id(pet), Kind = kind, Bound = true, Pulse = Now,
            NextAttack = Now + MclslContractRules.FalconIntervalSeconds,
            NextHeal = Now + MclslContractRules.DeerIntervalSeconds,
        };
        battle.Summons.Add(summon);
        SummonOwners[summon.Unit] = battle.Owner;
        MclslWorldActorQuery.Track(pet);
        state.ContractPetName = PetName(pet);
    }

    private static void UpdateBoundPet(Actor owner, Actor pet, MclslCareerBattleState battle, MclslCareerSummon summon)
    {
        // 每秒随主人刷新生命/攻击，并按原受伤比例回填。
        ApplyPetBaseStats(owner, pet, summon.Kind, false);
        if (owner.kingdom != null && pet.kingdom != owner.kingdom) pet.setKingdom(owner.kingdom);
        Actor enemy = owner.attack_target as Actor;
        bool inCombat = enemy != null && Enemy(owner, enemy);
        if (inCombat) BattleCoord(pet, summon, enemy);
        else if (pet.has_attack_target) pet.clearAttackTarget();
        // 距主人超过 6 格才发起跟随。
        if (Distance(owner, pet) > MclslContractRules.FollowTiles) pet.moveTo(owner.current_tile);
        if (summon.Kind == 2) FalconTick(owner, pet, battle, summon, inCombat, enemy);
        if (summon.Kind == 3) DeerTick(owner, pet, battle, summon, inCombat);
    }

    private static void BattleCoord(Actor pet, MclslCareerSummon summon, Actor enemy)
    {
        // 狼/龟/鹿沿用模板的原生攻击/反击行为（Init 已为龟、鹿补充狼的作战配置）；
        // 隼只按自身射程手动结算，不设 attack_target 以避免叠加近战。
        if (summon.Kind == 0 || summon.Kind == 1 || summon.Kind == 3)
        {
            pet.attack_target = enemy; pet.beh_actor_target = enemy; pet.has_attack_target = true;
        }
    }

    private static void FalconTick(Actor owner, Actor pet, MclslCareerBattleState battle, MclslCareerSummon summon, bool inCombat, Actor enemy)
    {
        if (!inCombat || enemy == null) return;
        if (Vector2.Distance(pet.current_position, enemy.current_position) > MclslContractRules.FalconRange) return;
        if (Now < summon.NextAttack) return;
        summon.NextAttack = Now + MclslContractRules.FalconIntervalSeconds;
        double factor = MclslContractRules.PetDamageFactor(IsMarked(battle, enemy),
            battle.PetBuffUntil > Now, battle.ResonanceUntil > Now, Passive(owner, 5));
        float damage = pet.stats.get("damage") * (float)factor;
        int before = enemy.getHealth();
        DealPetDamage(pet, enemy, Math.Max(1f, damage));
        if (enemy.getHealth() < before) HostileHit(owner);
    }

    private static void DeerTick(Actor owner, Actor pet, MclslCareerBattleState battle, MclslCareerSummon summon, bool inCombat)
    {
        if (!inCombat) return;
        if (Now < summon.NextHeal) return;
        summon.NextHeal = Now + MclslContractRules.DeerIntervalSeconds;
        double ownerRatio = owner.getMaxHealth() > 0 ? (double)owner.getHealth() / owner.getMaxHealth() : 1d;
        double petRatio = pet.getMaxHealth() > 0 ? (double)pet.getHealth() / pet.getMaxHealth() : 1d;
        bool ownerWithin = Vector2.Distance(owner.current_position, pet.current_position) <= MclslContractRules.DeerRange;
        Actor target = MclslContractRules.DeerHealsOwner(ownerRatio, petRatio, ownerWithin) ? owner : pet;
        if (target.getHealth() >= target.getMaxHealth()) return;
        double percent = MclslContractRules.CareerHealPercent(MclslContractRules.DeerHealPercent, Passive(owner, 4));
        int before = target.getHealth();
        target.restoreHealthPercent((float)percent);
        if (target.getHealth() > before) Touch(owner);
    }

    private static void DealPetDamage(Actor pet, Actor target, float damage)
    {
        bool previous = _directPetDamage; _directPetDamage = true;
        try { MclslSpellCasting.DealDamage(pet, target, damage, "木"); }
        finally { _directPetDamage = previous; }
    }

    internal static void OnActorDied(Actor actor)
    {
        if (_loadState.Status != MclslModuleLoadStatus.Ready || actor?.data == null) return;
        long id = MclslActorAccessor.Id(actor);
        if (Battles.ContainsKey(id)) RemoveOwner(actor);
        if (id <= 0 || !SummonOwners.TryGetValue(id, out long ownerId)) return;
        if (!Battles.TryGetValue(ownerId, out var battle)) return;
        MclslCareerSummon summon = null;
        for (int i = 0; i < battle.Summons.Count; i++)
            if (battle.Summons[i].Unit == id) { summon = battle.Summons[i]; break; }
        if (summon == null || !summon.Bound) return;
        if (!MclslActorRegistry.ResolveKnownOrWorld(ownerId, out var owner)) owner = null;
        HandlePetLost(owner, battle, summon, actor);
    }

    // 契灵死亡或主动解除契约：清空共鸣/距离护持，并从此刻起 30 秒游戏时间内不可重召。
    private static void HandlePetLost(Actor owner, MclslCareerBattleState battle, MclslCareerSummon summon, Actor pet)
    {
        SummonOwners.Remove(summon.Unit);
        battle.Summons.Remove(summon);
        battle.ResonanceUntil = 0; battle.GuardUntil = 0; battle.PetBuffUntil = 0;
        if (owner == null) return;
        owner.finishStatusEffect("mclsl_career_resonance");
        var state = MclslCombatCareerSystem.Read(owner);
        if (pet != null) { string name = PetName(pet); if (!string.IsNullOrEmpty(name)) state.ContractPetName = name; }
        state.NextSummonAt = MclslContractRules.ExtendCooldown(state.NextSummonAt, Now);
        MclslCombatCareerSystem.Write(owner, state);
    }

    // ── 主动技能（自动施放） ────────────────────────────────────
    private static bool TryCastSpirit(Actor owner, MclslCombatCareerState state, MclslCareerBattleState battle, Actor enemyTarget)
    {
        int usable = Math.Min(state.Rank, MclslRealmIds.Index(MclslActorAccessor.Realm(owner)) + 1);
        bool hasPet = HasLivingPet(battle);
        bool hasEnemy = enemyTarget != null && Enemy(owner, enemyTarget);
        bool canRevive = usable >= 4 && hasPet && CooldownReady(state, 4) && ManaReady(owner, 4) && NeedsRevive(owner, battle);
        bool canResonance = usable >= 5 && hasPet && battle.ResonanceUntil <= Now && hasEnemy && CooldownReady(state, 5) && ManaReady(owner, 5);
        bool canBreak = usable >= 3 && hasPet && hasEnemy && PetNear(battle, enemyTarget, MclslContractRules.FalconRange) && CooldownReady(state, 3) && ManaReady(owner, 3);
        bool canInfusion = usable >= 2 && hasPet && battle.PetBuffUntil <= Now && hasEnemy && CooldownReady(state, 2) && ManaReady(owner, 2);
        bool canSigil = usable >= 1 && hasEnemy && CooldownReady(state, 1) && ManaReady(owner, 1);
        int tier = MclslContractRules.SelectTier(canRevive, canResonance, canBreak, canInfusion, canSigil);
        if (tier < 0) return false;
        var skill = MclslCombatCareerCatalog.Skill(5, tier);
        long targetId;
        if (tier == 1 || tier == 3) targetId = MclslActorAccessor.Id(enemyTarget);
        else if (tier == 2 || tier == 4) targetId = BoundPetId(battle);
        else { long pid = BoundPetId(battle); targetId = pid > 0 ? pid : MclslActorAccessor.Id(owner); }
        battle.Cast = new MclslCareerCast { Tier = tier, Target = targetId, Due = Now + skill.Preparation };
        if (skill.Preparation > 0) { owner.stopMovement(); owner.addStatusEffect("mclsl_career_casting", skill.Preparation + 0.1f); }
        MclslSpellEffect.Show(owner, owner, skill.Id, phase: "cast", duration: Math.Max(0.12f, skill.Preparation));
        if (skill.Preparation == 0) TickCast(owner, battle);
        return true;
    }

    private static void ExecuteSpirit(Actor owner, Actor target, MclslCareerBattleState battle, MclslCareerSkill skill)
    {
        int tier = skill.Tier;
        if (tier == 1)
        {
            if (!Enemy(owner, target)) return;
            var mark = Mark(battle, target); mark.ContractMark = Now + MclslContractRules.MarkSeconds;
            int before = target.getHealth();
            // 与其他职业一致：基础系数 × 熟练度(每级5%) × 体质职业伤害加成。
            float power = CareerSkillPower(owner, skill, MclslContractRules.SigilDamagePower);
            MclslSpellCasting.DealDamage(owner, target, Math.Max(1f, (float)(owner.stats.get("damage") * power)), skill.Law);
            if (target.getHealth() < before) HostileHit(owner);
            MclslSpellEffect.Show(owner, target, skill.Id, phase: "hit", duration: 0.4f);
        }
        else if (tier == 2)
        {
            battle.PetBuffUntil = Now + MclslContractRules.InfusionSeconds;
            MclslSpellEffect.Show(owner, BoundPet(battle), skill.Id, phase: "area", duration: 0.6f);
        }
        else if (tier == 3)
        {
            if (!Enemy(owner, target)) return;
            Actor pet = BoundPet(battle);
            if (pet == null) return;
            if (Vector2.Distance(pet.current_position, target.current_position) > MclslContractRules.FalconRange) return;
            // 御灵破势是“契灵追加击”：沿用职业伤害结算（熟练度/体质），按宠物来源记账，
            // 不再次套用契灵基础伤害比例或契印/灌注/共鸣加成。
            float power = CareerSkillPower(owner, skill, MclslContractRules.BreakDamagePower);
            DealPetDamage(pet, target, Math.Max(1f, (float)(owner.stats.get("damage") * power)));
            target.addStatusEffect("mclsl_career_contract_slow", (float)MclslContractRules.BreakSlowSeconds);
            MclslSpellEffect.Show(owner, target, skill.Id, phase: "hit", duration: 0.4f);
        }
        else if (tier == 4)
        {
            Actor pet = BoundPet(battle);
            if (pet == null) return;
            double percent = MclslContractRules.CareerHealPercent(MclslContractRules.ReviveHealPercent, Passive(owner, 4));
            pet.restoreHealthPercent((float)percent);
            if (Vector2.Distance(owner.current_position, pet.current_position) <= MclslContractRules.DeerRange)
                battle.GuardUntil = Now + MclslContractRules.GuardSeconds;
            MclslSpellEffect.Show(owner, pet, skill.Id, phase: "area", duration: 0.6f);
        }
        else
        {
            Actor pet = BoundPet(battle);
            if (pet == null) return;
            battle.ResonanceUntil = Now + MclslContractRules.ResonanceSeconds;
            owner.addStatusEffect("mclsl_career_resonance", (float)MclslContractRules.ResonanceSeconds);
            MclslSpellEffect.Show(owner, pet, skill.Id, phase: "area", duration: 0.8f);
        }
    }

    // ── 小工具 ────────────────────────────────────────────────
    private static MclslCareerSummon BoundSummon(MclslCareerBattleState battle)
    {
        if (battle == null) return null;
        for (int i = 0; i < battle.Summons.Count; i++) if (battle.Summons[i].Bound) return battle.Summons[i];
        return null;
    }
    private static Actor BoundPet(MclslCareerBattleState battle)
    {
        var summon = BoundSummon(battle); if (summon == null) return null;
        Actor pet = Resolve(summon.Unit); return MclslActorAccessor.Alive(pet) ? pet : null;
    }
    private static long BoundPetId(MclslCareerBattleState battle)
    {
        var summon = BoundSummon(battle);
        if (summon == null) return 0;
        return MclslActorAccessor.Alive(Resolve(summon.Unit)) ? summon.Unit : 0;
    }
    private static bool HasLivingPet(MclslCareerBattleState battle) => BoundPet(battle) != null;
    private static bool IsMarked(MclslCareerBattleState battle, Actor target)
        => target != null && battle.Marks.TryGetValue(MclslActorAccessor.Id(target), out var mark) && mark.ContractMark > Now;
    private static bool PetNear(MclslCareerBattleState battle, Actor target, double range)
    {
        Actor pet = BoundPet(battle);
        return pet != null && target != null && Vector2.Distance(pet.current_position, target.current_position) <= range;
    }
    private static float CareerSkillPower(Actor owner, MclslCareerSkill skill, double basePower)
    {
        float power = (float)basePower * (1f + 0.05f * (MclslCombatCareerSystem.Read(owner).Mastery[skill.Tier - 1] - 1));
        power *= 1f + MclslPhysiqueSystem.CareerDamagePercent(owner, skill.Career) / 100f;
        return power;
    }
    private static bool CooldownReady(MclslCombatCareerState state, int tier) => state.Cooldowns[tier - 1] <= Now;
    private static bool ManaReady(Actor owner, int tier)
        => MclslSpellSystem.CurrentMana(owner) >= Mana(owner, MclslCombatCareerCatalog.Skill(5, tier));
    private static bool NeedsRevive(Actor owner, MclslCareerBattleState battle)
    {
        Actor pet = BoundPet(battle); if (pet == null) return false;
        double petRatio = pet.getMaxHealth() > 0 ? (double)pet.getHealth() / pet.getMaxHealth() : 1d;
        if (petRatio < MclslContractRules.RevivePetRatio) return true;
        double ownerRatio = owner.getMaxHealth() > 0 ? (double)owner.getHealth() / owner.getMaxHealth() : 1d;
        return ownerRatio < MclslContractRules.ReviveOwnerRatio
            && Vector2.Distance(owner.current_position, pet.current_position) <= MclslContractRules.DeerRange;
    }
    private static double Distance(Actor a, Actor b) => Vector2.Distance(a.current_position, b.current_position);
    private static string PetName(Actor pet)
    {
        if (pet?.data == null) return string.Empty;
        string name = pet.getName();
        return string.IsNullOrEmpty(name) ? string.Empty : name;
    }
    private static void ApplyPetBaseStats(Actor owner, Actor pet, int kind, bool full)
    {
        int newMax = MclslContractRules.PetHealth(owner.getMaxHealth(), kind, Passive(owner, 1));
        // 取主人 flat damage（BaseStats 的 "damage" 键）；灵契共鸣的主人 +10% 属于
        // "multiplier_damage" 倍率，不进入该 flat 值，因此不会被宠物继承后再叠 35/50%。
        int newAtk = MclslContractRules.PetAttack((int)owner.stats.get("damage"), kind);
        if (full)
        {
            pet.stats.set("health", newMax); pet.stats.set("damage", newAtk);
            pet.restoreHealthPercent(1f);
            return;
        }
        if (pet.stats.get("health") == newMax && pet.stats.get("damage") == newAtk) return;
        int oldMaximum = pet.getMaxHealth();
        double ratio = oldMaximum > 0 ? (double)pet.getHealth() / oldMaximum : 1d;
        pet.stats.set("health", newMax); pet.stats.set("damage", newAtk);
        int newMaximum = pet.getMaxHealth();
        int hp = (int)Math.Round(newMaximum * ratio);
        if (hp < 1) hp = 1;
        if (newMaximum > 0 && hp > newMaximum) hp = newMaximum;
        pet.data.health = hp;
    }
}
