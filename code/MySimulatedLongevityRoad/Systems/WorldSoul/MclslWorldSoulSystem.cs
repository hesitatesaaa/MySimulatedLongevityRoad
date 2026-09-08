using System;
using System.Collections.Generic;
using System.Reflection;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems.Death;
using MySimulatedLongevityRoad.Traits;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static partial class MclslWorldSoulSystem
{
    private static int _lastAnnualYear = -1;
    private static bool _claimsRepaired;
    private const int ManifestPeacefulDepartureYears = 5;
    private const int HunterDispatchIntervalYears = 1;
    private const int MaxHuntersPerManifest = 8;
    private static readonly string[] KillerMemberNames =
    {
        "last_attacker", "lastAttacker", "_last_attacker", "attacked_by", "attackedBy", "killer", "last_hit_actor", "lastHitActor"
    };
    private static readonly string[] TargetMemberNames =
    {
        "attack_target", "beh_actor_target", "target", "_attack_target", "current_target", "currentTarget"
    };

    internal static void OnWorldLoaded()
    {
        _lastAnnualYear = -1;
        _claimsRepaired = false;
        RepairDuplicateSoulClaims();
    }

    internal static void TickAnnual(int year)
    {
        if (_lastAnnualYear == year) return;
        _lastAnnualYear = year;
        if (!MclslWorldEpochSystem.IsNewLawActive(year)) return;
        if (!_claimsRepaired) RepairDuplicateSoulClaims();
        Reconcile(year);
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.WorldSouls == null || run.WorldSouls.Count == 0) return;

        foreach (MclslWorldSoulRecord soul in run.WorldSouls)
        {
            if (soul == null) continue;
            if (soul.HolderActorId > 0)
            {
                Actor holder = FindActor(soul.HolderActorId);
                if (MclslActorAccessor.Alive(holder))
                {
                    soul.State = "已祭炼";
                    soul.HolderActorName = SafeName(holder);
                    int dutyGain = DutyGain(holder, soul);
                    soul.DutyProgress = Math.Min(100, soul.DutyProgress + dutyGain);
                    SyncHolderDutyData(holder, soul);
                    ResolveDutyOutcome(holder, soul, year, dutyGain);
                    continue;
                }
                soul.HolderActorId = 0;
                soul.HolderActorName = string.Empty;
                soul.HolderOrigin = string.Empty;
                soul.State = "沉寂";
                soul.NextManifestYear = Math.Max(year + 60, soul.NextManifestYear);
            }
            if (soul.ManifestActorId > 0 && soul.State == "显化")
            {
                Actor manifest = FindActor(soul.ManifestActorId);
                if (!MclslActorAccessor.Alive(manifest))
                {
                    MclslWorldSoulActorRegistration.ForgetActorRuntime(soul.ManifestActorId);
                    soul.ManifestActorId = 0;
                    soul.ManifestActorName = string.Empty;
                    if (soul.HolderActorId <= 0)
                    {
                        soul.State = "沉寂";
                        soul.NextManifestYear = Math.Max(year + 50, soul.NextManifestYear);
                    }
                    continue;
                }

                MaintainWorldSoulEntity(manifest);
                if (IsManifestCombatOpened(manifest))
                    MarkManifestCombatStarted(soul, year);

                if (MclslWorldEpochSystem.IsNewLawActive(year))
                    DispatchHuntersIfNeeded(soul, manifest, year);

                if (soul.ManifestCombatStartedYear <= 0 && year - soul.ManifestYear >= ManifestPeacefulDepartureYears)
                    DepartPeacefully(soul, manifest, year);
            }
        }

        MclslWorldArchiveStore.MarkDirty();
        if (HasActiveManifest(run)) return;
        CountWorldMaturity(out int huaShen, out int goldenCore, out int cultivators);
        if (huaShen <= 0) return;

        MclslWorldSoulRecord candidate = PickNextManifestCandidate(run, year);
        if (candidate != null) Manifest(candidate, year);
    }

    private static bool HasActiveManifest(MclslWorldRunState run)
    {
        if (run?.WorldSouls == null) return false;
        for (int i = 0; i < run.WorldSouls.Count; i++)
        {
            MclslWorldSoulRecord soul = run.WorldSouls[i];
            if (soul == null || soul.State != "显化") continue;
            if (MclslActorAccessor.Alive(FindActor(soul.ManifestActorId))) return true;
        }
        return false;
    }

    private static MclslWorldSoulRecord PickNextManifestCandidate(MclslWorldRunState run, int year)
    {
        if (run?.WorldSouls == null) return null;
        MclslWorldSoulRecord best = null;
        for (int i = 0; i < run.WorldSouls.Count; i++)
        {
            MclslWorldSoulRecord soul = run.WorldSouls[i];
            if (soul == null || soul.HolderActorId > 0 || soul.ManifestActorId > 0 || year < soul.NextManifestYear) continue;
            if (best == null
                || soul.NextManifestYear < best.NextManifestYear
                || (soul.NextManifestYear == best.NextManifestYear && string.CompareOrdinal(soul.Id, best.Id) < 0))
            {
                best = soul;
            }
        }
        return best;
    }

    internal static void TickFrame(int frameCounter)
    {
        if (frameCounter % 4 != 0) return;
        MclslWorldSoulActorRegistration.TickTerrainEffects();
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.WorldSouls == null || run.WorldSouls.Count == 0) return;
        for (int i = 0; i < run.WorldSouls.Count; i++)
        {
            MclslWorldSoulRecord soul = run.WorldSouls[i];
            if (soul == null || soul.ManifestActorId <= 0 || soul.State != "显化") continue;
            Actor actor = FindActor(soul.ManifestActorId);
            if (!MclslActorAccessor.Alive(actor)) continue;
            MaintainWorldSoulEntity(actor);
            if (IsManifestCombatOpened(actor))
                MarkManifestCombatStarted(soul, MclslRuntime.CurrentYear());
            MclslWorldSoulActorRegistration.TickWorldSoulAttack(actor);
        }
    }

    internal static MclslWorldSoulDeathSnapshot CaptureDeath(Actor victim)
    {
        if (victim?.data == null) return MclslWorldSoulDeathSnapshot.Empty;
        string soulId = MclslActorAccessor.GetString(victim, MclslActorDataKeys.WorldSoulEntityId, string.Empty);
        if (string.IsNullOrWhiteSpace(soulId)) return MclslWorldSoulDeathSnapshot.Empty;
        Actor killer = TryGetKiller(victim);
        return new MclslWorldSoulDeathSnapshot(true, soulId, MclslActorAccessor.Id(victim), MclslActorAccessor.Id(killer), SafeNameOrEmpty(killer));
    }

    internal static bool ShouldBlockNonCombatDeath(Actor actor, AttackType attackType)
    {
        if (actor?.data == null) return false;
        string soulId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulEntityId, string.Empty);
        if (string.IsNullOrWhiteSpace(soulId)) return false;
        string cause = attackType.ToString().ToLowerInvariant();
        bool nonCombat = cause.Contains("age") || cause.Contains("old") || cause.Contains("hunger") || cause.Contains("starv") || cause == "none" || cause == "0";
        if (!nonCombat) return false;
        MaintainWorldSoulEntity(actor);
        try
        {
            float max = actor.getMaxHealth();
            if (max > 0f) actor.data.health = Math.Max(1, (int)Math.Min((float)int.MaxValue, max));
        }
        catch (Exception ex) { MclslDiagnostics.Error("world-soul-block-death-heal", "天地之魄阻断非战斗死亡后补血失败: " + ex.Message); }
        return true;
    }

    internal static void CommitDeath(Actor victim, MclslWorldSoulDeathSnapshot snapshot)
    {
        if (!snapshot.Found || victim?.data == null) return;
        MclslWorldSoulActorRegistration.ForgetActorRuntime(snapshot.VictimActorId);
        bool dead;
        try { dead = !victim.isAlive(); } catch { dead = false; }
        if (!dead) return;
        int year = MclslRuntime.CurrentYear();
        MclslWorldSoulRecord soul = FindSoulById(snapshot.SoulId);
        if (soul == null) return;
        string manifestName = string.IsNullOrWhiteSpace(soul.ManifestActorName) ? soul.Name : soul.ManifestActorName;
        soul.ManifestActorId = 0;
        soul.ManifestActorName = string.Empty;
        soul.DeathYear = year;
        Actor killer = FindActor(snapshot.KillerActorId);
        bool validKiller = MclslEligibility.CanClaimWorldSoul(killer);
        if (validKiller)
        {
            string killerName = SafeName(killer);
            MclslWorldRunRepository.AddEvent(year, "world_soul_slain", "天地之魄·" + soul.Name + "被击破", killerName + "击破“" + manifestName + "”。", killer);
            AnnounceWorldSoul(killerName + "击破天地之魄·" + soul.Name + "。", "#D0B067", 9f);
            if (IsLowRealmLastHit(killer))
            {
                string realmName = MclslRealmIds.Display(MclslActorAccessor.Realm(killer));
                MclslWorldRunRepository.AddEvent(year, "world_soul_low_realm_last_hit", killerName + "低境夺魄", realmName + "“" + killerName + "”夺得天地之魄·" + soul.Name + "。", killer);
                AnnounceWorldSoul(killerName + "夺得天地之魄·" + soul.Name + "！", "#E2BE55", 10f);
            }
            GrantHarmony(killer, soul, year);
        }
        else
        {
            soul.State = "散逸";
            soul.NextManifestYear = year + 70 + StableHash(soul.Id + "|respawn|" + year) % 81;
            MclslWorldRunRepository.AddEvent(year, "world_soul_dispersed", "天地之魄·" + soul.Name + "散入天地", "“" + manifestName + "”魄身崩散。");
            AnnounceWorldSoul("天地之魄·" + soul.Name + "散入天地。", "#D0B067", 8f);
        }
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void ReleaseHolderOnDeath(Actor victim)
    {
        if (victim?.data == null) return;
        string soulId = MclslActorAccessor.GetString(victim, MclslActorDataKeys.WorldSoulId, string.Empty);
        if (string.IsNullOrWhiteSpace(soulId)) return;
        MclslWorldSoulRecord soul = FindSoulById(soulId);
        if (soul == null || soul.HolderActorId != MclslActorAccessor.Id(victim)) return;
        int year = MclslRuntime.CurrentYear();
        string name = SafeName(victim);
        soul.HolderActorId = 0;
        soul.HolderActorName = string.Empty;
        soul.HolderOrigin = string.Empty;
        soul.DutyProgress = 0;
        soul.State = "散逸";
        soul.NextManifestYear = year + 70 + StableHash(soul.Id + "|holder_death|" + year) % 81;
        MclslWorldRunRepository.AddEvent(year, "world_soul_holder_death", "合道者" + name + "身死", "天地之魄·" + soul.Name + "复归天地。", victim);
        AnnounceWorldSoul(name + "身死，天地之魄·" + soul.Name + "复归天地。", "#D0B067", 9f);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void ReconcileRestoredHolder(Actor actor, MclslHuanzhenCultivationSnapshot snapshot, int year)
    {
        if (actor?.data == null || snapshot == null || string.IsNullOrWhiteSpace(snapshot.WorldSoulId)) return;
        MclslWorldSoulRecord soul = FindSoulById(snapshot.WorldSoulId);
        if (soul == null) return;

        // 锚点中的历史可能尚未发生夺魄，但还真明确保留死前修为；因此以还真者的死前魄位覆盖锚点旧状态。
        Actor manifest = FindActor(soul.ManifestActorId);
        if (MclslActorAccessor.Alive(manifest))
        {
            try
            {
                MclslWorldSoulActorRegistration.ForgetActorRuntime(MclslActorAccessor.Id(manifest));
                MclslActorAccessor.Set(manifest, MclslActorDataKeys.WorldSoulEntityId, string.Empty);
                ActorTrait marker = AssetManager.traits.get(MclslTraitRegistration.WorldSoulEntityTraitId);
                if (marker != null && manifest.hasTrait(marker.id)) manifest.removeTrait(marker.id);
                MclslNativeKillStatisticsSystem.DetachScriptedDeathAttacker(manifest);
                manifest.die(true, AttackType.Other, true, true);
            }
            catch (Exception ex) { MclslDiagnostics.Error("world-soul-huanzhen-clear-manifest", "还真覆盖魄位时清理旧显化体失败: " + ex.Message); }
        }
        soul.ManifestActorId = 0;
        soul.ManifestActorName = string.Empty;
        soul.State = "已祭炼";
        soul.HolderActorId = MclslActorAccessor.Id(actor);
        soul.HolderActorName = SafeName(actor);
        soul.HolderYear = year;
        soul.HolderOrigin = "还真保留死前祭魄合道结果";
        soul.NextManifestYear = 0;
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static bool IsWorldSoulActor(Actor actor) => actor?.data != null && !string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulEntityId, string.Empty));

    internal static bool TryManualManifest(string soulId)
    {
        int year = MclslRuntime.CurrentYear();
        MclslWorldRunRepository.EnsureCurrentRun(year);
        MclslWorldRunState currentRun = MclslWorldRunRepository.Current;
        if (!MclslWorldEpochSystem.IsNewLawActive(year) || currentRun?.TransmissionProved != true)
        {
            MclslAnnouncementSystem.Enqueue("旧法时代尚无可显化、祭炼的天地之魄。", "#D0B067", 7f, 1);
            return false;
        }
        MclslWorldRunRepository.EnsureNewLawCatalogs(year);
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.WorldSouls == null || run.WorldSouls.Count == 0)
        {
            MclslAnnouncementSystem.Enqueue("天地之魄尚未纳入本世玄黄仙录。", "#D0B067", 7f, 1);
            return false;
        }

        Reconcile(year);
        MclslWorldSoulRecord soul = FindSoulById(run, soulId);
        if (soul == null)
        {
            MclslAnnouncementSystem.Enqueue("未找到对应天地之魄。", "#D0B067", 7f, 1);
            return false;
        }

        Actor holder = FindActor(soul.HolderActorId);
        if (MclslActorAccessor.Alive(holder))
        {
            MclslAnnouncementSystem.Enqueue("天地之魄·" + soul.Name + "已有其主。", "#D0B067", 7f, 1);
            return false;
        }

        Actor manifest = FindActor(soul.ManifestActorId);
        if (MclslActorAccessor.Alive(manifest))
        {
            MclslAnnouncementSystem.Enqueue("天地之魄·" + soul.Name + "已临世。", "#D0B067", 7f, 1);
            return false;
        }

        soul.HolderActorId = 0;
        soul.HolderActorName = string.Empty;
        soul.HolderOrigin = string.Empty;
        soul.ManifestActorId = 0;
        soul.ManifestActorName = string.Empty;
        soul.State = "沉寂";
        soul.NextManifestYear = 0;
        if (Manifest(soul, year, true))
        {
            MclslAnnouncementSystem.Enqueue("已令天地之魄·" + soul.Name + "显化。", "#E2BE55", 8f, 1);
            return true;
        }

        MclslAnnouncementSystem.Enqueue("天地之魄·" + soul.Name + "显化失败，未找到可用地块。", "#D0B067", 7f, 1);
        return false;
    }

    internal static void Clear()
    {
        _lastAnnualYear = -1;
        _claimsRepaired = false;
        MclslWorldSoulObservationSystem.Clear();
        MclslWorldSoulActorRegistration.ClearRuntime();
    }

    internal static bool TryAcquireExistingSoulForConversion(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return false;
        if (!string.IsNullOrWhiteSpace(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulId, string.Empty)))
        {
            MclslWorldSoulRecord existing = FindSoulById(MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulId, string.Empty));
            if (existing != null && CanClaimSoul(existing, MclslActorAccessor.Id(actor)))
            {
                ClaimSoulForActor(actor, existing, year, "旧法转修求得魄位");
                return true;
            }
            ClearActorSoulClaim(actor);
        }

        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.WorldSouls == null) return false;
        MclslWorldSoulRecord soul = PickManualHarmonySoul(run, ResolveManualSoulTags(actor));
        if (soul == null || !CanClaimSoul(soul, MclslActorAccessor.Id(actor))) return false;
        ClaimSoulForActor(actor, soul, year, "旧法转修求得魄位");
        MclslWorldRunRepository.AddEvent(year, "ancient_conversion_world_soul", MclslActorAccessor.DisplayName(actor) + "求得天地之魄",
            MclslActorAccessor.DisplayName(actor) + "为转修新法求得天地之魄·" + soul.Name + "。", actor);
        return true;
    }

    internal static void EnsureManualHarmonyData(Actor actor, int year)
    {
        if (!MclslActorAccessor.Alive(actor)) return;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.WorldSouls == null) return;

        long actorId = MclslActorAccessor.Id(actor);
        string existingId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulId, string.Empty);
        MclslWorldSoulRecord soul = FindSoulById(run, existingId);
        if (soul != null)
        {
            if (!CanClaimSoul(soul, actorId))
            {
                ClearActorSoulClaim(actor);
                MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "该天地之魄已有存活持有者，无法重复追认");
                return;
            }
            ClaimSoulForActor(actor, soul, year, string.IsNullOrWhiteSpace(soul.HolderOrigin) ? "玄黄仙录追认魄位" : soul.HolderOrigin);
            return;
        }
        if (!string.IsNullOrWhiteSpace(existingId)) ClearActorSoulClaim(actor);

        if (!MclslRealmSeatSystem.CanAddHarmony(out string seatReason))
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, seatReason);
            return;
        }

        string[] rootTags = ResolveManualSoulTags(actor);
        soul = PickManualHarmonySoul(run, rootTags);

        if (soul == null)
        {
            MclslActorAccessor.Set(actor, MclslActorDataKeys.LastBreakthroughResult, "十道天地之魄皆已有主或正在显化，暂无法为手动合道追认空闲魄位");
            return;
        }

        ClaimSoulForActor(actor, soul, year, "玄黄仙录追认魄位");
        int compatibility = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HarmonyCompatibility, 0);
        int stability = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.HarmonyStability, 0);
        string displayName = MclslActorAccessor.DisplayName(actor, MclslRealmIds.HeDao);
        MclslWorldRunRepository.AddEvent(year, "manual_harmony_soul_claimed", displayName + "合道魄位追认", "玄黄仙录为“" + displayName + "”补录天地之魄·" + soul.Name + "，天职“" + soul.HeavenlyDuty + "”，祭魄适配" + compatibility + "%，合道稳定" + stability + "%。", actor);
    }

    private static void GrantHarmony(Actor killer, MclslWorldSoulRecord soul, int year)
    {
        string oldSoulId = MclslActorAccessor.GetString(killer, MclslActorDataKeys.WorldSoulId, string.Empty);
        if (!string.IsNullOrWhiteSpace(oldSoulId) && oldSoulId != soul.Id)
        {
            MclslWorldSoulRecord old = FindSoulById(oldSoulId);
            if (old != null)
            {
                old.HolderActorId = 0;
                old.HolderActorName = string.Empty;
                old.HolderOrigin = string.Empty;
                old.State = "沉寂";
                old.NextManifestYear = year + 100;
            }
        }

        string previousRealm = MclslActorAccessor.Realm(killer);
        int previousRealmIndex = MclslRealmIds.Index(previousRealm);
        bool belowHarmony = previousRealmIndex < MclslRealmIds.Index(MclslRealmIds.HeDao);
        bool leap = previousRealmIndex < MclslRealmIds.Index(MclslRealmIds.HuaShen);
        if (belowHarmony && !MclslRealmSeatSystem.CanAddHarmony(out string seatReason))
        {
            soul.State = "散逸";
            soul.HolderActorId = 0;
            soul.HolderActorName = string.Empty;
            soul.ManifestActorId = 0;
            soul.ManifestActorName = string.Empty;
            soul.NextManifestYear = year + 70 + StableHash(soul.Id + "|seat_full|" + year) % 81;
            MclslActorAccessor.Set(killer, MclslActorDataKeys.LastBreakthroughResult, seatReason);
            MclslWorldRunRepository.AddEvent(year, "world_soul_seat_full", "天地之魄·" + soul.Name + "散入天地", "魄位承载已满，" + SafeName(killer) + "未能祭炼天地之魄·" + soul.Name + "；此魄将于后世重新显化。", killer);
            AnnounceWorldSoul("天地之魄·" + soul.Name + "散入天地，合道魄位已满。", "#D0B067", 9f);
            return;
        }
        int compatibility = MclslCultivationLineage.Compatibility(killer, soul.LawTags);
        int integrity = MclslCultivationLineage.Integrity(killer);
        int stability = Math.Clamp(20 + compatibility / 2 + integrity / 3 + MclslMindSystem.StabilityBonus(killer) + (leap ? -25 : 20), 5, 100);
        if (belowHarmony && MclslInverseTruthSystem.IsTruthReversed("truth_player_weak_not_fixed"))
        {
            compatibility = Math.Clamp(compatibility + 10, 0, 100);
            stability = Math.Clamp(stability + 16, 5, 100);
        }
        MclslActorAccessor.Set(killer, MclslActorDataKeys.WorldSoulId, soul.Id);
        MclslActorAccessor.Set(killer, MclslActorDataKeys.WorldSoulName, soul.Name);
        MclslActorAccessor.Set(killer, MclslActorDataKeys.HeavenlyDuty, soul.HeavenlyDuty);
        MclslActorAccessor.Set(killer, MclslActorDataKeys.HarmonyLeap, leap ? 1 : 0);
        MclslActorAccessor.Set(killer, MclslActorDataKeys.HarmonyCompatibility, compatibility);
        MclslActorAccessor.Set(killer, MclslActorDataKeys.HarmonyStability, stability);
        string origin = HarmonyOrigin(leap, belowHarmony, compatibility, stability);
        MclslActorAccessor.Set(killer, MclslActorDataKeys.HarmonyOrigin, origin);
        if (MclslActorAccessor.GetInt(killer, MclslActorDataKeys.Aptitude, 0) <= 0) MclslActorAccessor.Set(killer, MclslActorDataKeys.Aptitude, 20 + StableHash(MclslActorAccessor.Id(killer) + "|soul_apt") % 81);
        MclslTraitRegistration.SyncGiftTrait(killer, Math.Clamp(MclslActorAccessor.GetInt(killer, MclslActorDataKeys.Aptitude, 50), 1, 100));
        MclslMindSystem.EnsureMindState(killer);
        MclslActorAccessor.Set(killer, MclslActorDataKeys.ImmortalFate, Math.Max(80, MclslActorAccessor.GetInt(killer, MclslActorDataKeys.ImmortalFate, 0)));
        MclslActorAccessor.Set(killer, MclslActorDataKeys.Contribution, Math.Max(500, MclslActorAccessor.GetInt(killer, MclslActorDataKeys.Contribution, 0)));
        MclslCultivationSystem.SetRealm(killer, MclslRealmIds.HeDao, year, origin + "：" + soul.Name);
        try
        {
            killer.updateStats();
            float maxHealth = killer.getMaxHealth();
            if (maxHealth > 0f) killer.data.health = Math.Max(1, (int)Math.Min((float)int.MaxValue, maxHealth));
        }
        catch (Exception ex) { MclslDiagnostics.Error("world-soul-claim-refresh-stats", "祭魄合道后刷新角色属性失败: " + ex.Message); }

        soul.State = "已祭炼";
        soul.HolderActorId = MclslActorAccessor.Id(killer);
        soul.HolderActorName = SafeName(killer);
        soul.HolderYear = year;
        soul.HolderOrigin = origin;
        soul.DutyProgress = 0;
        soul.DutyBacklash = Math.Clamp(leap ? 18 + Math.Max(0, 45 - stability) : Math.Max(0, 35 - stability) / 2, 0, 80);
        if (MclslInverseTruthSystem.IsTruthReversed("truth_player_duty_not_fixed"))
            soul.DutyBacklash = Math.Max(0, soul.DutyBacklash - 10);
        soul.LastDutyEventYear = year;
        soul.DutyCompletedYear = 0;
        soul.NextManifestYear = 0;
        SyncHolderDutyData(killer, soul);

        string prior = string.IsNullOrWhiteSpace(previousRealm) ? "凡人" : MclslRealmIds.Display(previousRealm);
        string displayName = MclslActorAccessor.DisplayName(killer, MclslRealmIds.HeDao);
        MclslWorldRunRepository.AddEvent(year, "harmony_last_hit", displayName + "祭魄合道", prior + "“" + displayName + "”取得天地之魄·" + soul.Name + "的祭炼归属；不论先前修为，魄核入身后立刻成就合道，并承接天职“" + soul.HeavenlyDuty + "”。根基：" + MclslCultivationLineage.ChainSummary(killer) + "；祭魄适配" + compatibility + "%，合道稳定" + stability + "%。", killer);
        AnnounceWorldSoul(displayName + "祭炼天地之魄·" + soul.Name + "，立地合道！", "#E2BE55", 12f);
    }

    private static string HarmonyOrigin(bool leap, bool belowHarmony, int compatibility, int stability)
    {
        if (leap) return stability < 35 ? "祭炼天地之魄强行合道，根基不全而天职沉重" : "祭炼天地之魄，不经前境圆满而立地合道";
        if (belowHarmony) return "祭炼天地之魄，越过原本修为立地合道";
        if (compatibility >= 75) return "法则根基与天地之魄相合，顺势祭魄合道";
        if (compatibility >= 45) return "斩灭天地之魄，以自身根基强行祭魄合道";
        return "斩灭天地之魄，法则不合仍强夺魄位";
    }

    private static bool IsLowRealmLastHit(Actor actor)
    {
        if (!MclslActorAccessor.Alive(actor)) return false;
        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(actor));
        return realm < MclslRealmIds.Index(MclslRealmIds.HuaShen);
    }

    private static void MaintainWorldSoulEntity(Actor actor)
    {
        if (actor?.data == null) return;
        try { MclslWorldSoulActorRegistration.NormalizeSubspecies(actor.subspecies); }
        catch (Exception ex) { MclslDiagnostics.Error("world-soul-normalize-subspecies", "归正天地之魄亚种显示失败: " + ex.Message); }
        TrySetNumber(actor.data, "hunger", 100f);
        TrySetNumber(actor.data, "_hunger", 100f);
    }

    private static void TrySetNumber(object target, string name, float value)
    {
        if (target == null || string.IsNullOrWhiteSpace(name)) return;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        Type type = target.GetType();
        try
        {
            FieldInfo field = type.GetField(name, flags);
            if (field != null)
            {
                if (field.FieldType == typeof(float)) field.SetValue(target, value);
                else if (field.FieldType == typeof(int)) field.SetValue(target, (int)value);
                return;
            }
            PropertyInfo property = type.GetProperty(name, flags);
            if (property != null && property.CanWrite)
            {
                if (property.PropertyType == typeof(float)) property.SetValue(target, value);
                else if (property.PropertyType == typeof(int)) property.SetValue(target, (int)value);
            }
        }
        catch (Exception ex) { MclslDiagnostics.Error("world-soul-set-number-member", "设置天地之魄数值成员失败: " + ex.Message); }
    }

    private static void Reconcile(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.WorldSouls == null) return;
        for (int i = 0; i < run.WorldSouls.Count; i++)
        {
            MclslWorldSoulRecord soul = run.WorldSouls[i];
            if (soul == null) continue;
            if (soul.Quality <= 0) soul.Quality = 3;
            if (string.IsNullOrWhiteSpace(soul.NativeTerrainEffect))
                soul.NativeTerrainEffect = MclslNativeTerrainProfileCatalog.ForSoul(MclslGeneratedObjectFactory.SplitTags(soul.LawTags), soul.HeavenlyDuty);
            if (soul.NextManifestYear <= 0 && soul.HolderActorId <= 0 && soul.ManifestActorId <= 0)
                soul.NextManifestYear = Math.Max(run.StartYear + 50 + i * 45, year + 20 + i * 20);
            if (soul.ManifestActorId > 0 && !MclslActorAccessor.Alive(FindActor(soul.ManifestActorId)))
            {
                MclslWorldSoulActorRegistration.ForgetActorRuntime(soul.ManifestActorId);
                soul.ManifestActorId = 0;
                soul.ManifestActorName = string.Empty;
                if (soul.HolderActorId <= 0)
                {
                    soul.State = "沉寂";
                    soul.NextManifestYear = Math.Max(year + 50, soul.NextManifestYear);
                }
            }
        }
    }

    private static Actor TryGetKiller(Actor victim)
    {
        try
        {
            Actor killer = victim.attackedBy as Actor;
            if (killer != null && killer != victim) return killer;
        }
        catch (Exception ex) { MclslDiagnostics.Error("world-soul-read-attacked-by", "读取天地之魄击杀者失败: " + ex.Message); }
        Actor reflected = TryGetActorMember(victim, KillerMemberNames);
        if (reflected != null && reflected != victim) return reflected;

        Actor target = TryGetActorMember(victim, TargetMemberNames);
        if (MclslEligibility.CanClaimWorldSoul(target)) return target;

        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetKnownActorsSnapshot();
        for (int i = 0; i < actors.Count; i++)
        {
            Actor actor = actors[i];
            if (actor == victim || !MclslEligibility.CanClaimWorldSoul(actor)) continue;
            if (Targets(actor, victim)) return actor;
        }
        return null;
    }

    private static bool Targets(Actor actor, Actor victim)
    {
        if (actor == null || victim == null) return false;
        try { if (actor.attack_target == victim || actor.beh_actor_target == victim) return true; }
        catch (Exception ex) { MclslDiagnostics.Error("world-soul-read-actor-target", "读取角色攻击目标失败: " + ex.Message); }
        return TryGetActorMember(actor, TargetMemberNames) == victim;
    }

    private static Actor TryGetActorMember(object source, string[] memberNames)
    {
        if (source == null || memberNames == null) return null;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        Type type = source.GetType();
        for (int i = 0; i < memberNames.Length; i++)
        {
            string name = memberNames[i];
            try
            {
                FieldInfo field = type.GetField(name, flags);
                if (field != null && field.GetValue(source) is Actor actor) return actor;
                PropertyInfo property = type.GetProperty(name, flags);
                if (property != null && property.GetValue(source, null) is Actor propertyActor) return propertyActor;
            }
            catch (Exception ex) { MclslDiagnostics.Error("world-soul-read-actor-member-" + name, "反射读取角色成员失败: " + ex.Message); }
        }
        return null;
    }

    private static bool CanClaimSoul(MclslWorldSoulRecord soul, long actorId)
    {
        if (soul == null) return false;
        if (soul.ManifestActorId > 0 && MclslActorAccessor.Alive(FindActor(soul.ManifestActorId))) return false;
        if (soul.HolderActorId <= 0 || soul.HolderActorId == actorId) return true;
        return !MclslActorAccessor.Alive(FindActor(soul.HolderActorId));
    }

    private static void ClaimSoulForActor(Actor actor, MclslWorldSoulRecord soul, int year, string origin)
    {
        if (actor?.data == null || soul == null) return;
        long actorId = MclslActorAccessor.Id(actor);
        int compatibility = Math.Clamp(MclslCultivationLineage.Compatibility(actor, soul.LawTags), 25, 100);
        int integrity = MclslCultivationLineage.Integrity(actor);
        int stability = Math.Clamp(35 + compatibility / 3 + integrity / 3 + MclslMindSystem.StabilityBonus(actor), 25, 100);
        soul.ManifestActorId = 0;
        soul.ManifestActorName = string.Empty;
        soul.HolderActorId = actorId;
        soul.HolderActorName = SafeName(actor);
        soul.HolderYear = year;
        soul.HolderOrigin = origin ?? string.Empty;
        soul.State = "已祭炼";
        soul.DutyProgress = Math.Clamp(soul.DutyProgress, 0, 100);
        soul.DutyBacklash = Math.Clamp(soul.DutyBacklash, 0, 45);
        soul.LastDutyEventYear = Math.Max(soul.LastDutyEventYear, year);
        soul.NextManifestYear = 0;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.WorldSoulId, soul.Id);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.WorldSoulName, soul.Name);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeavenlyDuty, soul.HeavenlyDuty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyLeap, integrity < 45 ? 1 : 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyCompatibility, compatibility);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyStability, stability);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyOrigin, soul.HolderOrigin);
        SyncHolderDutyData(actor, soul);
        MclslWorldArchiveStore.MarkDirty();
    }

    private static void ClearActorSoulClaim(Actor actor)
    {
        if (actor?.data == null) return;
        MclslActorAccessor.Set(actor, MclslActorDataKeys.WorldSoulId, string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.WorldSoulName, string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeavenlyDuty, string.Empty);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyLeap, 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HeavenlyDutyBacklash, 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyCompatibility, 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyStability, 0);
        MclslActorAccessor.Set(actor, MclslActorDataKeys.HarmonyOrigin, string.Empty);
    }

    private static void RepairDuplicateSoulClaims()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.WorldSouls == null) return;
        Dictionary<string, long> owners = new(StringComparer.Ordinal);
        for (int i = 0; i < run.WorldSouls.Count; i++)
        {
            MclslWorldSoulRecord soul = run.WorldSouls[i];
            if (soul == null) continue;
            Actor holder = FindActor(soul.HolderActorId);
            if (soul.HolderActorId > 0 && MclslActorAccessor.Alive(holder)) owners[soul.Id] = soul.HolderActorId;
            else if (soul.HolderActorId > 0)
            {
                soul.HolderActorId = 0;
                soul.HolderActorName = string.Empty;
                soul.HolderOrigin = string.Empty;
                if (soul.ManifestActorId <= 0) soul.State = "沉寂";
            }
        }
        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot();
        if (actors == null || actors.Count == 0) return;
        for (int i = 0; i < actors.Count; i++)
        {
            Actor actor = actors[i];
            if (!MclslActorAccessor.Alive(actor)) continue;
            string soulId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.WorldSoulId, string.Empty);
            if (string.IsNullOrWhiteSpace(soulId)) continue;
            MclslWorldSoulRecord soul = FindSoulById(run, soulId);
            long actorId = MclslActorAccessor.Id(actor);
            if (soul == null || (owners.TryGetValue(soulId, out long owner) && owner != actorId))
            {
                ClearActorSoulClaim(actor);
                continue;
            }
            if (!owners.ContainsKey(soulId) && CanClaimSoul(soul, actorId))
            {
                ClaimSoulForActor(actor, soul, MclslRuntime.CurrentYear(), string.IsNullOrWhiteSpace(soul.HolderOrigin) ? "旧档魄位修复" : soul.HolderOrigin);
                owners[soulId] = actorId;
            }
        }
        _claimsRepaired = true;
    }

    private static string[] ResolveManualSoulTags(Actor actor)
    {
        string[] roots = MclslCultivationLineage.RootTags(actor);
        if (roots.Length > 0) return FirstTags(roots, 3);
        MclslTechniqueDefinition technique = MclslCultivationCatalog.Technique(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueId, string.Empty));
        return MclslGeneratedObjectFactory.NormalizeTags(technique.LawPool);
    }

    private static MclslWorldSoulRecord FindSoulById(string soulId)
    {
        return FindSoulById(MclslWorldRunRepository.Current, soulId);
    }

    private static MclslWorldSoulRecord FindSoulById(MclslWorldRunState run, string soulId)
    {
        if (run?.WorldSouls == null || string.IsNullOrWhiteSpace(soulId)) return null;
        for (int i = 0; i < run.WorldSouls.Count; i++)
        {
            MclslWorldSoulRecord soul = run.WorldSouls[i];
            if (soul != null && string.Equals(soul.Id, soulId, StringComparison.Ordinal)) return soul;
        }
        return null;
    }

    private static MclslWorldSoulRecord PickManualHarmonySoul(MclslWorldRunState run, string[] rootTags)
    {
        if (run?.WorldSouls == null) return null;
        MclslWorldSoulRecord best = null;
        int bestCompatibility = int.MinValue;
        int bestQuality = int.MinValue;
        for (int i = 0; i < run.WorldSouls.Count; i++)
        {
            MclslWorldSoulRecord soul = run.WorldSouls[i];
            if (soul == null || soul.HolderActorId > 0 || soul.ManifestActorId > 0) continue;
            int compatibility = MclslLawInteractionCatalog.CompatibilityScore(rootTags, MclslGeneratedObjectFactory.SplitTags(soul.LawTags), 0);
            int quality = soul.Quality;
            if (best == null || compatibility > bestCompatibility || (compatibility == bestCompatibility && quality > bestQuality))
            {
                best = soul;
                bestCompatibility = compatibility;
                bestQuality = quality;
            }
        }
        return best;
    }

    private static string[] FirstTags(string[] source, int maxCount)
    {
        if (source == null || source.Length == 0 || maxCount <= 0) return Array.Empty<string>();
        int count = Math.Min(source.Length, maxCount);
        string[] result = new string[count];
        for (int i = 0; i < count; i++) result[i] = source[i];
        return result;
    }

    private static Actor FindActor(long id)
    {
        if (id <= 0 || World.world?.units == null) return null;
        try { return World.world.units.get(id); }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("world-soul-find-actor", "天地之魄按 ID 查找角色失败: " + ex.Message);
            return null;
        }
    }

}
